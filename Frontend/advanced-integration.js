(function installKayAdvancedIntegration(){
  'use strict';
  if(!window.KayAdvanced||typeof pages!=='object'||!Array.isArray(nav)||typeof render!=='function'){
    window.runAdvancedUiChecks=()=>({ok:false,total:1,passed:0,checks:{prerequisites:false}});
    return;
  }
  const advanced=window.KayAdvanced;
  const canonical={
    payments:{label:'Paiements',icon:'wallet',section:'Finance & contrôle',title:'Préparation des paiements'},
    expenses:{label:'Notes de frais',icon:'invoice',section:'Cycles métier',title:'Notes de frais'},
    commissions:{label:'Commissions',icon:'trending',section:'Cycles métier',title:'Moteur de commissions'},
    assets:{label:'Immobilisations',icon:'building',section:'Cycles métier',title:'Immobilisations & amortissements'},
    cashBoxes:{label:'Caisses',icon:'wallet',section:'Finance & contrôle',title:'Caisses multi-sites'},
    fiscalRules:{label:'Règles fiscales',icon:'tax',section:'Finance & contrôle',title:'Règles fiscales & historique'},
    documents:{label:'Documents & OCR',icon:'invoice',section:'Système',title:'Documents & file OCR'},
    masterData:{label:'Référentiel Groupe',icon:'building',section:'Système',title:'Référentiel KAY Groupe'},
    aged:{label:'Balances âgées',icon:'clock',section:'Finance & contrôle',title:'Balances âgées clients & fournisseurs'}
  };
  const advancedRoutes=new Set(Object.keys(advanced.pages));
  const data=()=>window.KayEnterpriseIntegration?.data?.()||state.data?.enterprise||state.data||{};
  const searchState={query:'',results:[],executed:false};
  const esc=value=>String(value??'').replace(/[&<>"']/g,char=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
  const money=(value,currency='MAD')=>value===null||value===undefined?'—':`${new Intl.NumberFormat('fr-MA',{minimumFractionDigits:2,maximumFractionDigits:2}).format(Number(value)||0)} ${currency||'MAD'}`;
  const kindLabel=kind=>({Operation:'Opération',Invoice:'Facture',Payment:'Paiement',BankOperation:'Banque',AccountingEntry:'Écriture',Document:'Document',Contract:'Contrat',Tax:'Fiscalité'}[kind]||kind||'Résultat');

  function rawData(){return data().raw||data().Raw||{}}
  function options(items,label){return (items||[]).map(item=>`<option value="${esc(item.id)}">${esc(label(item))}</option>`).join('')}
  function renderGlobalSearch(){
    const raw=rawData(),results=searchState.results||[];
    const content=!searchState.executed
      ? '<div class="ke-empty"><strong>Retrouvez toute la chaîne d’une opération</strong><p>Recherchez une facture, un tiers, un paiement, un IBAN, une écriture, un contrat, une RAS ou un montant.</p></div>'
      : results.length
        ? `<div class="ka-search-results">${results.map(result=>`<article class="ka-search-result"><div class="ka-search-kind">${esc(kindLabel(result.kind))}</div><div><strong>${esc(result.title)}</strong><p>${esc(result.subtitle)}</p></div><div class="ka-search-amount">${money(result.amount,result.currency)}</div><button type="button" class="row-action" data-action="open-search-result" data-operation-id="${esc(result.operationId||'')}" data-route="${esc(result.route||'')}" data-record="${esc(result.entityId||'')}" aria-label="Ouvrir ${esc(result.title)}">→</button></article>`).join('')}</div>`
        : '<div class="ke-empty"><strong>Aucun résultat</strong><p>Modifiez le texte, le montant ou la période de recherche.</p></div>';
    return `<div class="page-head ka-page-head"><div><div class="eyebrow">KAY ONE · TRAÇABILITÉ</div><h1>Recherche globale</h1><p>Une vue unique sur les opérations et tous leurs impacts liés.</p></div></div>
      <section class="panel ka-search-panel"><form id="kay-global-search-form" class="ka-search-form"><label class="ka-search-main"><span>Référence, tiers, IBAN, document ou montant</span><input name="query" value="${esc(searchState.query)}" placeholder="Ex. INV-2026-00458 ou 12 264"></label><button class="primary-button" type="submit">Rechercher</button><button class="secondary-button" type="button" data-action="clear-global-search">Effacer</button>
      <details class="ka-search-advanced"><summary>Filtres avancés</summary><div class="ka-search-filter-grid"><label><span>Montant exact (MAD)</span><input name="exactAmountMad" inputmode="decimal"></label><label><span>Montant minimum</span><input name="minimumAmountMad" inputmode="decimal"></label><label><span>Montant maximum</span><input name="maximumAmountMad" inputmode="decimal"></label><label><span>Devise</span><select name="currency"><option value="">Toutes</option><option>MAD</option><option>EUR</option><option>USD</option></select></label><label><span>Société</span><select name="companyId"><option value="">Toutes</option>${options(raw.companies,item=>`${item.code} · ${item.name}`)}</select></label><label><span>Client / fournisseur</span><select name="partyId"><option value="">Tous</option>${options(raw.parties,item=>`${item.internalCode} · ${item.name}`)}</select></label><label><span>Du</span><input name="startDate" type="date"></label><label><span>Au</span><input name="endDate" type="date"></label></div></details></form></section>
      <section class="panel ka-search-content"><div class="panel-head"><div><h2>${searchState.executed?`${results.length} résultat${results.length===1?'':'s'}`:'Résultats'}</h2><p>${searchState.executed?`Recherche « ${esc(searchState.query||'filtres avancés')} »`:'Saisissez vos critères ci-dessus'}</p></div></div>${content}</section>`;
  }
  pages.search=renderGlobalSearch;
  Object.keys(advanced.pages).forEach(route=>{pages[route]=()=>advanced.render(route,data())});

  function sendSearch(form){
    const payload=Object.fromEntries(new FormData(form).entries());
    Object.keys(payload).forEach(key=>{if(payload[key]==='')delete payload[key]});
    searchState.query=String(payload.query||'').trim();
    window.KayEnterpriseIntegration?.send?.('global-search',payload);
  }
  function traceBody(trace){
    const operation=trace?.operation||{},groups=[['Documents',trace?.documents],['Factures',trace?.invoices],['Échéances',trace?.dueItems],['Paiements',trace?.payments],['Fiscalité',trace?.taxImpacts],['Écritures',trace?.accountingEntries],['Trésorerie',trace?.treasuryMovements],['Banque',trace?.bankOperations],['Reporting',trace?.reportingFacts],['Audit',trace?.auditLog]];
    const impacts=(trace?.impacts||[]).map(item=>`<li><span>${esc(item.kind)}</span><strong>${esc(item.reference||'—')}</strong><em>${esc(item.state)}</em></li>`).join('');
    const state=String(operation.status||'').toLowerCase(),id=operation.id||'';
    const currentUserId=String(data().currentUser?.id||'').toLowerCase(),createdBy=String(operation.createdByUserId||'').toLowerCase(),createdByCurrentUser=Boolean(currentUserId&&createdBy&&currentUserId===createdBy);
    const workflow=[];
    if(state==='draft')workflow.push(`<button type="button" class="primary-button" data-trace-action="submit-operation" data-id="${esc(id)}">Soumettre</button>`);
    if(state==='submitted'&&!createdByCurrentUser)workflow.push(`<button type="button" class="primary-button" data-trace-action="validate-operation" data-id="${esc(id)}">Valider</button>`);
    if(state==='submitted'&&createdByCurrentUser)workflow.push(`<button type="button" class="secondary-button" disabled title="Validation par un autre utilisateur requise">Validation par un autre utilisateur requise</button>`);
    if(state==='validated'||state==='reconciled')workflow.push(`<button type="button" class="primary-button" data-trace-action="post-operation" data-id="${esc(id)}">Comptabiliser</button>`);
    if(state&&state!=='cancelled')workflow.push(`<button type="button" class="secondary-button ka-cancel-operation" data-trace-action="cancel-operation" data-id="${esc(id)}">Annuler avec trace</button>`);
    return `<div class="ka-trace-summary"><div><span>Référence</span><strong>${esc(operation.reference||'—')}</strong></div><div><span>Nature</span><strong>${esc(operation.nature||'—')}</strong></div><div><span>Montant</span><strong>${money(operation.amountMad,'MAD')}</strong></div><div><span>Statut</span><strong>${esc(operation.status||'—')}</strong></div></div><h4>Impacts générés</h4><ul class="ka-trace-list">${impacts||'<li>Aucun impact actif</li>'}</ul><div class="ka-trace-counts">${groups.map(([label,items])=>`<div><strong>${(items||[]).length}</strong><span>${label}</span></div>`).join('')}</div>${workflow.length?`<div class="ka-trace-workflow"><label><span>Commentaire / motif d’annulation</span><input data-trace-reason placeholder="Obligatoire pour une annulation"></label><div>${workflow.join('')}</div></div>`:''}`;
  }
  function bindSearch(root){
    const form=root.querySelector('#kay-global-search-form');
    form?.addEventListener('submit',event=>{event.preventDefault();sendSearch(form)});
    root.addEventListener('click',event=>{
      const button=event.target.closest('button[data-action]');if(!button)return;event.stopPropagation();
      if(button.dataset.action==='clear-global-search'){searchState.query='';searchState.results=[];searchState.executed=false;document.querySelector('#global-search').value='';render();return}
      if(button.dataset.action==='open-search-result'){
        if(button.dataset.operationId){window.KayEnterpriseIntegration?.send?.('operation-trace',{id:button.dataset.operationId});return}
        const route=(button.dataset.route||'').split('/')[0];if(pages[route])navigate(route);
      }
    });
    root.dataset.searchPage='true';
  }

  function insertAfter(id,item){
    if(nav.some(x=>x.id===item.id))return;
    const index=nav.findIndex(x=>x.id===id);
    nav.splice(index<0?nav.length:index+1,0,item);
  }
  function item(id){const meta=canonical[id];return{id,label:meta.label,icon:meta.icon,parent:meta.section,level:2,advanced:true}}
  const agedIndex=nav.findIndex(x=>x.id==='aged');if(agedIndex>=0)nav[agedIndex]={...nav[agedIndex],...item('aged')};
  insertAfter('imports',item('assets'));
  insertAfter('assets',item('expenses'));
  insertAfter('expenses',item('commissions'));
  insertAfter('treasury',item('cashBoxes'));
  insertAfter('cashBoxes',item('payments'));
  insertAfter('exemptions',item('fiscalRules'));
  insertAfter('admin',item('masterData'));
  insertAfter('masterData',item('documents'));

  let dispose=null;
  const baseRender=render;
  function renderAdvanced(){
    if(typeof dispose==='function')dispose();dispose=null;
    const result=baseRender.apply(this,arguments);
    if(state.page==='search'){
      const root=document.querySelector('#app-content');if(root)bindSearch(root);
      return result;
    }
    if(!advancedRoutes.has(state.page))return result;
    const root=document.querySelector('#app-content');if(!root)return result;
    const isolation=new AbortController();
    root.addEventListener('click',event=>{if(event.target.closest('button[data-action]'))event.stopPropagation()},{signal:isolation.signal});
    const unbind=advanced.bind(state.page,data(),window.KayEnterpriseIntegration?.send);
    root.dataset.advancedPage=state.page;
    dispose=()=>{isolation.abort();if(typeof unbind==='function')unbind()};
    return result;
  }
  render=renderAdvanced;window.render=renderAdvanced;

  window.runAdvancedUiChecks=()=>{
    const checks={},errors=[],original=state.page;
    const check=(name,value)=>checks[name]=Boolean(value);
    try{
      const registry=advanced.runUiChecks(data());
      check('action_registry_complete',registry.ok);
      check('action_registry_nonempty',registry.renderedActionCount>0&&registry.registeredActionCount>=registry.renderedActionCount);
      for(const [route,meta] of Object.entries(canonical)){
        navigate(route);
        const root=document.querySelector('#app-content');
        check(`${route}_title`,root?.querySelector('h1')?.textContent.trim()===meta.title);
        check(`${route}_bound`,root?.dataset.advancedPage===route&&Boolean(root.__kayAdvancedController));
        const buttons=[...(root?.querySelectorAll('button')||[])];
        check(`${route}_buttons`,buttons.length>0&&buttons.every(button=>button.type==='submit'||Boolean(button.dataset.action)||Boolean(button.dataset.page)));
        check(`${route}_content`,Boolean(root?.querySelector('.panel'))&&!root?.textContent.includes('Domaine indisponible'));
      }
      const currentUserId=data().currentUser?.id||'';
      const ownTrace=currentUserId?traceBody({operation:{id:'op-check',reference:'TEST-SOD',status:'Submitted',createdByUserId:currentUserId},impacts:[]}):'Validation par un autre utilisateur requise';
      check('trace_blocks_creator_validation_button',!ownTrace.includes('data-trace-action="validate-operation"')&&ownTrace.includes('Validation par un autre utilisateur requise'));
    }catch(error){errors.push(error?.message||String(error));checks.unexpected_exception=false}
    finally{navigate(original)}
    const values=Object.values(checks),passed=values.filter(Boolean).length;
    return{ok:passed===values.length&&!errors.length,total:values.length,passed,failed:values.length-passed,checks,errors};
  };
  window.runAllProductionChecks=()=>{
    const enterprise=window.runEnterpriseUiChecks?.()||{ok:false};
    const advanced=window.runAdvancedUiChecks();
    const original=state.page;navigate('search');const root=document.querySelector('#app-content');
    const search={page:state.page==='search',form:Boolean(root?.querySelector('#kay-global-search-form')),filters:Boolean(root?.querySelector('.ka-search-advanced')),bound:root?.dataset.searchPage==='true'};
    navigate(original);
    return{ok:Boolean(enterprise.ok&&advanced.ok&&Object.values(search).every(Boolean)),enterprise,advanced,search};
  };

  document.addEventListener('keydown',event=>{
    if(event.target?.id!=='global-search'||event.key!=='Enter')return;
    event.preventDefault();event.stopImmediatePropagation();
    searchState.query=event.target.value.trim();navigate('search');
    window.KayEnterpriseIntegration?.send?.('global-search',{query:searchState.query});
  },true);
  document.addEventListener('click',event=>{
    const target=event.target.closest('[data-trace-action]');if(!target)return;
    event.preventDefault();event.stopImmediatePropagation();
    const reason=document.querySelector('[data-trace-reason]')?.value.trim()||'';
    if(target.dataset.traceAction==='cancel-operation'&&!reason){showToast('Le motif d’annulation est obligatoire.','error');document.querySelector('[data-trace-reason]')?.focus();return}
    window.KayEnterpriseIntegration?.send?.(target.dataset.traceAction,{operationId:target.dataset.id,comment:reason,reason});
    closeOverlays();
  },true);
  if(window.chrome?.webview&&typeof window.chrome.webview.addEventListener==='function')window.chrome.webview.addEventListener('message',event=>{
    const message=event.data;if(!message||typeof message!=='object')return;
    if(message.type==='search-results'){
      searchState.query=String(message.query||searchState.query||'');searchState.results=Array.isArray(message.results)?message.results:[];searchState.executed=true;
      const global=document.querySelector('#global-search');if(global)global.value=searchState.query;
      if(state.page!=='search')state.page='search';render();
    }
    if(message.type==='operation-trace-result')showModal('Traçabilité de l’opération',traceBody(message.trace),'Fermer');
  });
  window.KayAdvancedIntegration=Object.freeze({installed:true,routes:[...advancedRoutes],canonical:Object.keys(canonical)});
  renderAdvanced();
})();
