(function installKayProductionPages(){
  'use strict';
  if(typeof pages!=='object'||typeof render!=='function'||typeof navigate!=='function')return;

  const routes=new Set(['dashboard','sales','purchases','treasury','tax','accounting','journal','admin','support']);
  const titles={dashboard:'Tableau de bord',sales:'Ventes',purchases:'Achats',treasury:'Trésorerie',tax:'Fiscalité',accounting:'Comptabilité',journal:'Journal global',admin:'Administration',support:'Centre d’aide'};
  const esc=value=>String(value??'').replace(/[&<>"']/g,char=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
  const arr=value=>Array.isArray(value)?value:[];
  const num=value=>Number(value)||0;
  const money=(value,currency='MAD')=>`${new Intl.NumberFormat('fr-MA',{minimumFractionDigits:2,maximumFractionDigits:2}).format(num(value))} ${esc(currency)}`;
  const date=value=>{if(!value)return'—';const parsed=new Date(`${String(value).slice(0,10)}T12:00:00`);return Number.isNaN(parsed.valueOf())?esc(value):parsed.toLocaleDateString('fr-FR')};
  const data=()=>window.KayEnterpriseIntegration?.data?.()||state.data?.enterprise||state.data||{};
  const raw=source=>source.raw||source.Raw||{};
  const icon=name=>`<span class="nav-icon" data-icon="${esc(name)}"></span>`;
  const button=(label,action,kind='secondary',iconName='',attrs='')=>`<button type="button" class="${kind}-button" data-kp-action="${esc(action)}" ${attrs}>${iconName?icon(iconName):''}<span>${esc(label)}</span></button>`;
  const head=(title,subtitle,actions='')=>`<div class="page-head kp-page-head"><div><div class="eyebrow">KAY ONE · DONNÉES TEMPS RÉEL</div><h1>${esc(title)}</h1><p>${esc(subtitle)}</p></div><div class="page-actions">${actions}</div></div>`;
  const empty=(title,text)=>`<div class="kp-empty">${icon('journal')}<strong>${esc(title)}</strong><p>${esc(text)}</p></div>`;
  const statusTone=value=>/pay|valid|posted|reconcil|actif|active|succès|closed/i.test(value)?'paid':/cancel|late|overdue|expir|rejet|blocked|élevé/i.test(value)?'late':/draft|brouillon|information/i.test(value)?'draft':'pending';
  const status=value=>`<span class="status ${statusTone(value)}">${esc(value||'—')}</span>`;
  const kpi=(label,value,meta,iconName,tone='')=>`<article class="panel kp-kpi ${tone?`kp-${tone}`:''}"><div><span>${esc(label)}</span><i>${icon(iconName)}</i></div><strong>${esc(value)}</strong><small>${esc(meta)}</small></article>`;
  const operationId=operation=>operation.operationId||operation.id||'';
  const operationRef=operation=>operation.reference||operation.id||'—';
  const operationAmount=operation=>num(operation.amountMad??operation.amount);
  const operationDisplayAmount=operation=>num(operation.amount??operation.amountMad);
  const liveOperations=source=>arr(source.operations).filter(item=>!/(cancel|annul)/i.test(item.status||''));
  const financialOperations=source=>liveOperations(source).filter(item=>!/(draft|brouillon|submitted|à valider|a valider)/i.test(item.status||''));
  const rawOperations=source=>arr(raw(source).operations).filter(item=>!/(cancel)/i.test(item.status||''));
  const typeIs=(value,...patterns)=>patterns.some(pattern=>new RegExp(pattern,'i').test(String(value||'')));

  function monthSeries(source){
    const values=new Map();
    financialOperations(source).forEach(item=>{
      const key=String(item.date||'').slice(0,7);if(!/^\d{4}-\d{2}$/.test(key))return;
      const current=values.get(key)||{sales:0,costs:0};
      if(typeIs(item.type,'vente','export'))current.sales+=operationAmount(item);
      if(typeIs(item.type,'achat','import','note de frais','décaissement'))current.costs+=operationAmount(item);
      values.set(key,current);
    });
    return [...values.entries()].sort(([a],[b])=>a.localeCompare(b)).slice(-6).map(([key,value])=>({key,label:new Date(`${key}-01T12:00:00`).toLocaleDateString('fr-FR',{month:'short'}),...value}));
  }

  function dashboard(){
    const source=data(),metrics=source.metrics||{},operations=liveOperations(source),series=monthSeries(source),maximum=Math.max(1,...series.flatMap(item=>[item.sales,item.costs]));
    const ageing=source.customerAging||{},ageValues=[num(ageing.notDueMad),num(ageing.days1To30Mad),num(ageing.days31To60Mad)+num(ageing.days61To90Mad),num(ageing.over90DaysMad)],ageTotal=Math.max(1,ageValues.reduce((a,b)=>a+b,0));
    const alerts=arr(source.alerts),recent=operations.slice().sort((a,b)=>String(b.date).localeCompare(String(a.date))).slice(0,6);
    return `${head('Tableau de bord','La situation consolidée du Groupe, calculée depuis le Transaction Engine.',button('Nouvelle opération','new-operation','primary','plus')+button('Actualiser','refresh','secondary','refresh'))}
      <section class="grid kp-kpis">${kpi('Chiffre d’affaires',money(metrics.revenueMad),'Opérations de vente actives','sales')}${kpi('Trésorerie disponible',money(metrics.cashBalanceMad),'Banques et caisses','bank')}${kpi('Créances clients',money(metrics.receivablesMad),`${money(metrics.overdueMad)} en retard`,'invoice',metrics.overdueMad?'warning':'')}${kpi('Dettes fournisseurs',money(metrics.payablesMad),'Échéances encore ouvertes','cart')}</section>
      <section class="grid kp-dashboard-grid"><article class="panel kp-chart"><div class="panel-head"><div><h2>Activité mensuelle</h2><p>Ventes et coûts issus des opérations enregistrées</p></div><div class="kp-legend"><span><i></i>Ventes</span><span><i></i>Coûts</span></div></div>${series.length?`<div class="kp-bars" aria-label="Activité mensuelle interactive">${series.map(item=>`<div class="kp-bar-group"><div class="kp-bar-pair"><button type="button" class="kp-bar sales" data-kp-action="chart-point" data-label="${esc(item.label)}" data-sales="${item.sales}" data-costs="${item.costs}" style="height:${Math.max(3,item.sales/maximum*100)}%" title="Ventes ${money(item.sales)}"><span>Ventes ${money(item.sales)}</span></button><button type="button" class="kp-bar costs" data-kp-action="chart-point" data-label="${esc(item.label)}" data-sales="${item.sales}" data-costs="${item.costs}" style="height:${Math.max(3,item.costs/maximum*100)}%" title="Coûts ${money(item.costs)}"><span>Coûts ${money(item.costs)}</span></button></div><span>${esc(item.label)}</span></div>`).join('')}</div>`:empty('Aucune activité datée','Les graphiques se rempliront après les premières opérations.')}</article>
      <article class="panel kp-aging"><div class="panel-head"><div><h2>Balance âgée clients</h2><p>Répartition de l’encours réel par date d’échéance</p></div><button type="button" class="text-link" data-kp-action="navigate" data-page="aged">Voir le détail →</button></div><div class="kp-aging-body"><button type="button" class="kp-donut" data-kp-action="aging-info" data-notdue="${ageValues[0]}" data-age30="${ageValues[1]}" data-age90="${ageValues[2]}" data-over90="${ageValues[3]}" style="--a:${ageValues[0]/ageTotal*360}deg;--b:${(ageValues[0]+ageValues[1])/ageTotal*360}deg;--c:${(ageValues[0]+ageValues[1]+ageValues[2])/ageTotal*360}deg"><div><strong>${money(ageValues.reduce((a,b)=>a+b,0))}</strong><span>encours</span></div></button><ul>${[['Non échue',ageValues[0]],['1–30 jours',ageValues[1]],['31–90 jours',ageValues[2]],['Plus de 90 jours',ageValues[3]]].map(([label,value],index)=>`<li class="age-${index}" data-kp-action="aging-info" data-notdue="${ageValues[0]}" data-age30="${ageValues[1]}" data-age90="${ageValues[2]}" data-over90="${ageValues[3]}"><i></i><span>${label}</span><strong>${money(value)}</strong></li>`).join('')}</ul></div><p class="kp-aging-note">Note : un délai de paiement de 45 jours reste “Non échue” tant que la date d’échéance n’est pas dépassée.</p></article></section>
      <section class="grid kp-bottom-grid"><article class="panel table-panel"><div class="panel-head"><div><h2>Dernières opérations</h2><p>${operations.length} opération${operations.length===1?'':'s'} active${operations.length===1?'':'s'}</p></div><button type="button" class="text-link" data-kp-action="navigate" data-page="operations">Tout afficher →</button></div>${recent.length?operationTable(recent,false):empty('Aucune opération','Créez la première opération pour alimenter le système.')}</article><article class="panel kp-alerts"><div class="panel-head"><div><h2>Alertes à traiter</h2><p>Échéances documentaires et contractuelles</p></div></div>${alerts.length?alerts.slice(0,6).map(alert=>`<div class="kp-alert"><i class="${num(alert.daysRemaining)<0?'danger':''}">${icon('bell')}</i><div><strong>${esc(alert.reference)}</strong><p>${esc(alert.requiredAction||alert.kind)}</p></div><span>${esc(alert.threshold||`${alert.daysRemaining} j`)}</span></div>`).join(''):empty('Aucune alerte','Aucune expiration ne requiert une action aujourd’hui.')}</article></section>`;
  }

  function operationTable(items,showNature=true){
    return `<div class="kp-table-scroll"><table class="data-table kp-table"><thead><tr><th>Référence</th><th>Date</th><th>Type</th>${showNature?'<th>Nature</th>':''}<th>Tiers</th><th>Montant</th><th>Statut</th><th></th></tr></thead><tbody>${items.map(item=>`<tr data-kp-row><td class="ref">${esc(operationRef(item))}</td><td>${date(item.date||item.operationDate)}</td><td>${esc(item.type)}</td>${showNature?`<td>${esc(item.nature||'—')}</td>`:''}<td>${esc(item.party||'—')}</td><td class="amount">${money(operationDisplayAmount(item),item.currency||'MAD')}</td><td>${status(item.status)}</td><td><button type="button" class="row-action" data-kp-action="trace" data-id="${esc(operationId(item))}" data-reference="${esc(operationRef(item))}" aria-label="Ouvrir ${esc(operationRef(item))}">→</button></td></tr>`).join('')}</tbody></table></div>`;
  }

  function operationDomain(kind){
    const source=data(),sales=kind==='sales',patterns=sales?['vente','export']:['achat','import'],items=liveOperations(source).filter(item=>typeIs(item.type,...patterns)),recognized=financialOperations(source).filter(item=>typeIs(item.type,...patterns));
    const total=recognized.reduce((sum,item)=>sum+operationAmount(item),0),open=items.filter(item=>!/(pay|paid|reconcil|posted|comptabilis|rapproch)/i.test(item.status||'')).length;
    const title=sales?'Ventes':'Achats',counterparty=sales?'clients':'fournisseurs';
    return `${head(title,sales?'Factures, proformas, avoirs, export et encaissements reliés à leur origine.':'Achats locaux, services, matériel, import et dettes reliés à leur origine.',button(`Nouvelle ${sales?'vente':'dépense'}`,'new-type','primary','plus',`data-operation-type="${sales?'Vente':'Achat'}"`)+button(`Voir les ${counterparty}`,'navigate','secondary',sales?'users':'building',`data-page="${sales?'clients':'suppliers'}"`))}
      <section class="grid kp-domain-kpis">${kpi(`${title} actives`,String(items.length),'Toutes sociétés','journal')}${kpi('Montant reconnu',money(total),'Hors brouillons et soumissions','invoice')}${kpi('À traiter',String(open),'Workflow non clôturé','clock',open?'warning':'')}${kpi('Traçabilité',`${items.filter(item=>num(item.impacts)>0).length}/${items.length}`,'Opérations avec impacts','link')}</section>
      <div class="filters kp-filters"><label>${icon('search')}<input data-kp-filter placeholder="Référence, nature, tiers ou montant…"></label>${button('Exporter','export','secondary','download')}</div><article class="panel table-panel">${items.length?operationTable(items):empty(`Aucune ${sales?'vente':'dépense'}`,`Les nouvelles ${sales?'ventes':'dépenses'} apparaîtront ici automatiquement.`)}</article>`;
  }

  function treasury(){
    const source=data(),snapshot=raw(source),accounts=arr(snapshot.bankAccounts),cashBoxes=arr(snapshot.cashBoxes),movements=arr(snapshot.treasuryMovements),bankOperations=arr(snapshot.bankOperations),metrics=source.metrics||{};
    const unreconciled=bankOperations.filter(item=>!/^(reconciled|rapproch)/i.test(item.reconciliationStatus||'')).length;
    return `${head('Trésorerie','Banques, caisses, mouvements et rapprochements sur une même chaîne.',button('Nouveau mouvement','new-type','primary','plus','data-operation-type="Banque"')+button('Préparer les paiements','navigate','secondary','wallet','data-page="payments"'))}
      <section class="grid kp-domain-kpis">${kpi('Solde consolidé',money(metrics.cashBalanceMad),'Comptes bancaires et caisses','bank')}${kpi('Comptes bancaires',String(accounts.length),'Référentiel actif','building')}${kpi('Caisses',String(cashBoxes.length),'Par société et site','wallet')}${kpi('À rapprocher',String(unreconciled),'Opérations bancaires','clock',unreconciled?'warning':'')}</section>
      <section class="grid kp-account-grid">${accounts.length?accounts.map(account=>`<article class="panel kp-account"><div><span>${icon('bank')}</span>${status(account.isActive?'Actif':'Inactif')}</div><h3>${esc(account.name||account.bankName)}</h3><p>${esc(account.bankName)} · ${esc(account.currency)}</p><strong>${money(account.balanceMad,'MAD')}</strong><small>${esc(account.iban||'IBAN non renseigné')}</small></article>`).join(''):empty('Aucun compte bancaire','Ajoutez les comptes dans le Référentiel Groupe.')}</section>
      <article class="panel table-panel"><div class="panel-head"><div><h2>Mouvements de trésorerie</h2><p>Flux générés automatiquement par les opérations</p></div><div>${button('Rapprochement','navigate','secondary','bank','data-page="reconciliation"')}${button('Caisses','navigate','secondary','wallet','data-page="cashBoxes"')}</div></div>${movements.length?`<div class="kp-table-scroll"><table class="data-table kp-table"><thead><tr><th>Référence</th><th>Date</th><th>Nature</th><th>Direction</th><th>Montant</th><th>Statut</th><th></th></tr></thead><tbody>${movements.map(item=>`<tr data-kp-row><td class="ref">${esc(item.reference)}</td><td>${date(item.movementDate)}</td><td>${esc(item.label||item.kind||'Mouvement')}</td><td>${esc(item.direction)}</td><td class="amount">${money(item.amountMad)}</td><td>${status(item.status)}</td><td><button type="button" class="row-action" data-kp-action="trace" data-id="${esc(item.operationId||'')}" aria-label="Voir la source">→</button></td></tr>`).join('')}</tbody></table></div>`:empty('Aucun mouvement','Les flux apparaîtront dès qu’une opération affectera la trésorerie.')}</article>`;
  }

  function fiscal(){
    const source=data(),snapshot=raw(source),eligibleOperations=new Set(arr(snapshot.operations).filter(item=>!/(draft|submitted|cancel)/i.test(item.status||'')).map(item=>item.id)),impacts=arr(snapshot.taxImpacts).filter(item=>!/(cancel|superseded)/i.test(item.status||'')&&eligibleOperations.has(item.operationId)),rules=arr(snapshot.taxRules),vatOut=impacts.reduce((sum,item)=>sum+num(item.outputVatMad),0),vatIn=impacts.reduce((sum,item)=>sum+num(item.inputVatMad),0),withholding=impacts.reduce((sum,item)=>sum+num(item.withholdingMad),0);
    return `${head('Fiscalité','TVA, RAS et exonérations déterminées par des règles versionnées.',button('Nouvelle opération fiscale','new-type','primary','plus','data-operation-type="Fiscalité"')+button('Gérer les règles','navigate','secondary','settings','data-page="fiscalRules"'))}<section class="grid kp-domain-kpis">${kpi('TVA collectée',money(vatOut),'Impacts fiscaux actifs','tax')}${kpi('TVA déductible',money(vatIn),'Impacts fiscaux actifs','invoice')}${kpi('TVA nette',money(Math.max(0,vatOut-vatIn)),'Position calculée','book')}${kpi('Retenues à la source',money(withholding),`${rules.length} règle${rules.length===1?'':'s'} configurée${rules.length===1?'':'s'}`,'clock')}</section>
      <article class="panel table-panel"><div class="panel-head"><div><h2>Impacts fiscaux</h2><p>Chaque ligne remonte à son opération source</p></div>${button('Certificats d’exonération','navigate','secondary','tax','data-page="exemptions"')}</div>${impacts.length?`<div class="kp-table-scroll"><table class="data-table kp-table"><thead><tr><th>Référence</th><th>Date</th><th>Règle</th><th>Base</th><th>TVA collectée</th><th>TVA déductible</th><th>RAS</th><th>Statut</th><th></th></tr></thead><tbody>${impacts.map(item=>`<tr data-kp-row><td class="ref">${esc(item.reference)}</td><td>${date(item.taxDate)}</td><td>${esc(item.ruleCode||'—')}</td><td class="amount">${money(item.taxableBaseMad)}</td><td class="amount">${money(item.outputVatMad)}</td><td class="amount">${money(item.inputVatMad)}</td><td class="amount">${money(item.withholdingMad)}</td><td>${status(item.status)}</td><td><button type="button" class="row-action" data-kp-action="trace" data-id="${esc(item.operationId||'')}" aria-label="Voir la source">→</button></td></tr>`).join('')}</tbody></table></div>`:empty('Aucun impact fiscal','Le Fiscal Engine générera les impacts selon les règles applicables.')}</article>`;
  }

  function accounting(){
    const source=data(),entries=arr(raw(source).accountingEntries),active=entries.filter(item=>!/(superseded|cancel)/i.test(item.status||'')),balanced=active.filter(item=>{const lines=arr(item.lines);return Math.abs(lines.reduce((s,x)=>s+num(x.debitMad),0)-lines.reduce((s,x)=>s+num(x.creditMad),0))<.01}).length,total=active.reduce((sum,item)=>sum+arr(item.lines).reduce((s,line)=>s+num(line.debitMad),0),0);
    return `${head('Comptabilité','Écritures automatiques, journaux et piste d’audit reliés à la transaction source.',button('Nouvelle OD','new-type','primary','plus','data-operation-type="Opération diverse"')+button('Audit Trail','navigate','secondary','journal','data-page="audit"'))}<section class="grid kp-domain-kpis">${kpi('Écritures actives',String(active.length),`${balanced} équilibrée${balanced===1?'':'s'}`,'book')}${kpi('Mouvements débit',money(total),'Somme des écritures actives','invoice')}${kpi('À comptabiliser',String(active.filter(item=>/draft/i.test(item.status||'')).length),'Statut brouillon','clock')}${kpi('Équilibre',active.length?`${Math.round(balanced/active.length*100)} %`:'100 %','Débit = crédit','check')}</section>
      <article class="panel table-panel"><div class="panel-head"><div><h2>Journal comptable</h2><p>Écritures générées par l’Accounting Engine</p></div>${button('Exporter','export','secondary','download')}</div>${active.length?`<div class="kp-table-scroll"><table class="data-table kp-table"><thead><tr><th>Référence</th><th>Date</th><th>Journal</th><th>Libellé</th><th>Débit</th><th>Crédit</th><th>Statut</th><th></th></tr></thead><tbody>${active.map(item=>{const debit=arr(item.lines).reduce((s,x)=>s+num(x.debitMad),0),credit=arr(item.lines).reduce((s,x)=>s+num(x.creditMad),0);return`<tr data-kp-row><td class="ref">${esc(item.reference)}</td><td>${date(item.entryDate)}</td><td>${esc(item.journalCode)}</td><td>${esc(item.label)}</td><td class="amount">${money(debit)}</td><td class="amount">${money(credit)}</td><td>${status(item.status)}</td><td><button type="button" class="row-action" data-kp-action="trace" data-id="${esc(item.operationId||'')}" aria-label="Voir la source">→</button></td></tr>`}).join('')}</tbody></table></div>`:empty('Aucune écriture','L’Accounting Engine créera les écritures équilibrées automatiquement.')}</article>`;
  }

  function journal(){
    const source=data(),operations=liveOperations(source);
    return `${head('Journal global','Toutes les opérations et leurs impacts dans une vue auditée.',button('Nouvelle opération','new-operation','primary','plus')+button('Exporter','export','secondary','download'))}<div class="filters kp-filters"><label>${icon('search')}<input data-kp-filter placeholder="Référence, tiers, nature ou montant…"></label><span class="status paid">${operations.length} opération${operations.length===1?'':'s'} active${operations.length===1?'':'s'}</span></div><article class="panel table-panel">${operations.length?operationTable(operations):empty('Journal vide','Créez une opération pour initialiser le journal global.')}</article>`;
  }

  function administration(){
    const source=data(),snapshot=raw(source),companies=arr(snapshot.companies),sites=arr(snapshot.sites),labs=arr(snapshot.laboratories),centers=arr(snapshot.costCenters),users=arr(snapshot.users),roles=arr(snapshot.roles),activeUsers=users.filter(item=>item.isActive);
    return `${head('Administration','Référentiel Groupe, utilisateurs, rôles et paramètres du moteur.',button('Référentiel Groupe','navigate','primary','building','data-page="masterData"')+button('Sécurité & rôles','navigate','secondary','settings','data-page="security"'))}<section class="grid kp-domain-kpis">${kpi('Sociétés',String(companies.length),`${sites.length} site${sites.length===1?'':'s'}`,'building')}${kpi('Laboratoires',String(labs.length),`${centers.length} centre${centers.length===1?'':'s'} de coût`,'journal')}${kpi('Utilisateurs actifs',String(activeUsers.length),`${roles.length} rôle${roles.length===1?'':'s'}`,'users')}${kpi('Sessions','Mot de passe','Accès local simplifié','shield','positive')}</section>
      <section class="grid kp-admin-grid"><article class="panel"><div class="panel-head"><div><h2>Sociétés du Groupe</h2><p>Données légales et statut</p></div></div>${companies.length?companies.map(company=>`<div class="kp-admin-row"><span>${icon('building')}</span><div><strong>${esc(company.name)}</strong><p>${esc(company.code)} · ICE ${esc(company.ice||'non renseigné')}</p></div>${status(company.isActive?'Actif':'Inactif')}</div>`).join(''):empty('Aucune société','Initialisez le Référentiel Groupe.')}</article><article class="panel"><div class="panel-head"><div><h2>Utilisateurs & rôles</h2><p>Droits gérés avec séparation des tâches</p></div></div>${users.length?users.map(user=>{const roleNames=arr(user.roleIds).map(id=>roles.find(role=>role.id===id)?.name).filter(Boolean).join(', ');return`<div class="kp-admin-row"><span>${icon('users')}</span><div><strong>${esc(user.displayName)}</strong><p>${esc(user.email)} · ${esc(roleNames||'Sans rôle')}</p></div>${status(user.isActive?'Actif':'Suspendu')}</div>`}).join(''):empty('Aucun utilisateur','Ajoutez les comptes et affectez leurs rôles.')}</article></section>`;
  }

  function support(){
    return `${head('Centre d’aide','Accès rapide aux contrôles, à la traçabilité et aux réglages KAY ONE.')}<section class="grid kp-support-grid"><article class="panel kp-support-card">${icon('journal')}<h2>Traiter une opération</h2><p>Commencez toujours par Nouvelle opération. Le moteur adapte le formulaire et produit les impacts liés.</p>${button('Nouvelle opération','new-operation','primary','plus')}</article><article class="panel kp-support-card">${icon('search')}<h2>Retrouver une pièce</h2><p>Utilisez Ctrl+K pour rechercher une facture, un montant, un IBAN, un paiement ou une écriture.</p>${button('Ouvrir la recherche','navigate','secondary','search','data-page="search"')}</article><article class="panel kp-support-card">${icon('shield')}<h2>Contrôler la traçabilité</h2><p>Le journal d’audit conserve la création, la validation, les modifications, les annulations et les traitements automatiques.</p>${button('Ouvrir l’Audit Trail','navigate','secondary','shield','data-page="audit"')}</article></section><article class="panel kp-support-note"><strong>Besoin d’administration ?</strong><p>Gérez les sociétés, tiers et axes analytiques dans le Référentiel Groupe, puis les habilitations dans Sécurité & rôles.</p><div>${button('Référentiel Groupe','navigate','secondary','building','data-page="masterData"')}${button('Sécurité & rôles','navigate','secondary','settings','data-page="security"')}</div></article>`;
  }

  pages.dashboard=dashboard;
  pages.sales=()=>operationDomain('sales');
  pages.purchases=()=>operationDomain('purchases');
  pages.treasury=treasury;
  pages.tax=fiscal;
  pages.accounting=accounting;
  pages.journal=journal;
  pages.admin=administration;
  pages.support=support;

  function exportVisible(root){
    const table=root.querySelector('table');if(!table){showToast('Aucun tableau à exporter.','error');return}
    const rows=[...table.querySelectorAll('tr')].filter(row=>!row.hidden).map(row=>[...row.querySelectorAll('th,td')].slice(0,-1).map(cell=>`"${cell.textContent.trim().replaceAll('"','""')}"`).join(';'));
    const blob=new Blob(['\ufeff'+rows.join('\n')],{type:'text/csv;charset=utf-8'}),url=URL.createObjectURL(blob),link=document.createElement('a');link.href=url;link.download=`kay-one-${state.page}-${new Date().toISOString().slice(0,10)}.csv`;link.click();setTimeout(()=>URL.revokeObjectURL(url),500);showToast('Export CSV généré.');
  }
  function bind(root){
    const click=event=>{const target=event.target.closest('[data-kp-action]');if(!target||!root.contains(target))return;event.preventDefault();event.stopPropagation();const action=target.dataset.kpAction;
      if(action==='navigate'){navigate(target.dataset.page);return}
      if(action==='new-operation'){navigate('operation');return}
      if(action==='new-type'){state.operationType=target.dataset.operationType;navigate('operation');return}
      if(action==='refresh'){window.KayEnterpriseIntegration?.send?.('refresh');showToast('Données actualisées.');return}
      if(action==='trace'){if(target.dataset.id)window.KayEnterpriseIntegration?.send?.('operation-trace',{id:target.dataset.id});else if(target.dataset.reference)window.KayEnterpriseIntegration?.send?.('operation-trace',{reference:target.dataset.reference});return}
      if(action==='chart-point'){const sales=num(target.dataset.sales),costs=num(target.dataset.costs);showPopover(target,`Activité · ${target.dataset.label||'Mois'}`,[{icon:'V',title:'Ventes',text:money(sales)},{icon:'C',title:'Coûts',text:money(costs)},{icon:sales-costs>=0?'+':'−',title:'Marge brute estimée',text:money(sales-costs)}]);return}
      if(action==='aging-info'){showPopover(target,'Lecture de la balance âgée',[{icon:'0',title:'Non échue',text:`${money(target.dataset.notdue)} · échéance future ou aujourd’hui`},{icon:'30',title:'1–30 jours',text:`${money(target.dataset.age30)} · retard depuis l’échéance`},{icon:'90',title:'31–90 jours',text:`${money(target.dataset.age90)} · retard confirmé`},{icon:'!',title:'Plus de 90 jours',text:`${money(target.dataset.over90)} · risque élevé`}]);return}
      if(action==='export'){exportVisible(root);return}
    };
    const filter=event=>{if(!event.target.matches('[data-kp-filter]'))return;const query=event.target.value.toLocaleLowerCase('fr');root.querySelectorAll('[data-kp-row]').forEach(row=>row.hidden=!row.textContent.toLocaleLowerCase('fr').includes(query))};
    root.addEventListener('click',click);root.addEventListener('input',filter);root.dataset.productionPage=state.page;
    return()=>{root.removeEventListener('click',click);root.removeEventListener('input',filter)};
  }

  function updateChrome(){
    const source=data(),snapshot=raw(source),companies=arr(snapshot.companies),users=arr(snapshot.users),current=source.currentUser||users.find(item=>item.isActive)||{};
    const workspace=document.querySelector('#workspace-switcher strong');if(workspace)workspace.textContent=companies[0]?.name||'KAY Groupe';
    const user=document.querySelector('.user-card>div:nth-child(2)');if(user)user.innerHTML=`<strong>${esc(current.displayName||'Utilisateur KAY ONE')}</strong><span>${esc(current.role||'Session locale')}</span>`;
    const avatar=document.querySelector('.user-card>.avatar');if(avatar)avatar.textContent=String(current.displayName||'KO').split(/\s+/).slice(0,2).map(part=>part[0]).join('').toUpperCase();
    const dot=document.querySelector('.notification-dot');if(dot)dot.hidden=!arr(source.alerts).length;
    const donut=document.querySelector('.kp-donut');if(donut){const ageing=source.customerAging||{},total=num(ageing.notDueMad)+num(ageing.days1To30Mad)+num(ageing.days31To60Mad)+num(ageing.days61To90Mad)+num(ageing.over90DaysMad);donut.classList.toggle('empty',total<=0)}
  }
  let dispose=null;const baseRender=render;
  function renderProduction(){if(dispose)dispose();dispose=null;const result=baseRender.apply(this,arguments);mountIcons();updateChrome();if(routes.has(state.page)){const root=document.querySelector('#app-content');if(root)dispose=bind(root)}return result}
  render=renderProduction;window.render=renderProduction;
  const baseChecks=window.runAllProductionChecks;
  window.runProductionDomainChecks=()=>{const original=state.page,checks={};try{for(const route of routes){navigate(route);const root=document.querySelector('#app-content'),buttons=[...root.querySelectorAll('button')];checks[`${route}_title`]=root.querySelector('h1')?.textContent.trim()===titles[route];checks[`${route}_bound`]=root.dataset.productionPage===route;checks[`${route}_data_source`]=root.querySelector('.eyebrow')?.textContent.includes('TEMPS RÉEL');checks[`${route}_buttons`]=buttons.every(item=>Boolean(item.dataset.kpAction)||item.type==='submit');if(route==='dashboard'){checks.dashboard_chart_interactive=Boolean(root.querySelector('[data-kp-action="chart-point"]'))||!root.querySelector('.kp-bars');checks.dashboard_aging_interactive=Boolean(root.querySelector('[data-kp-action="aging-info"]'));checks.dashboard_aging_explains_not_due=/délai de paiement de 45 jours/i.test(root.textContent)}}}finally{navigate(original)}const values=Object.values(checks);return{ok:values.every(Boolean),total:values.length,passed:values.filter(Boolean).length,checks}};
  window.runAllProductionChecks=()=>{const foundation=baseChecks?.()||{ok:false};const domains=window.runProductionDomainChecks();const authentication=window.runAuthUiChecks?.()||{ok:false,total:1,passed:0,checks:{available:false}};return{ok:Boolean(foundation.ok&&domains.ok&&authentication.ok),foundation,domains,authentication}};
  window.KayProductionPages=Object.freeze({routes:[...routes],installed:true});
  document.addEventListener('click',event=>{
    const button=event.target.closest('button');if(!button)return;
    if(button.id==='quick-add'){event.preventDefault();event.stopImmediatePropagation();navigate('operation');return}
    if(button.id==='sync-button'){event.preventDefault();event.stopImmediatePropagation();window.KayEnterpriseIntegration?.send?.('refresh');showToast('Données synchronisées.');return}
    if(button.id==='notification-button'){
      event.preventDefault();event.stopImmediatePropagation();const alerts=arr(data().alerts);
      showPopover(button,'Alertes KAY ONE',alerts.length?alerts.slice(0,8).map(alert=>({icon:num(alert.daysRemaining)<0?'!':String(alert.daysRemaining),title:alert.reference,text:alert.requiredAction||alert.kind})):[{icon:'✓',title:'Aucune alerte',text:'Aucune échéance documentaire à traiter.'}]);return;
    }
    if(button.id==='workspace-switcher'){
      event.preventDefault();event.stopImmediatePropagation();const companies=arr(raw(data()).companies);
      showPopover(button,'Sociétés du Groupe',companies.length?companies.map(company=>({icon:String(company.code||'KG').slice(0,2),title:company.name,text:`${company.code} · ${company.isActive?'Active':'Inactive'}`})):[{icon:'KG',title:'Aucune société',text:'Configurez le Référentiel Groupe.'}]);return;
    }
    if(button.id==='user-menu-button'){
      event.preventDefault();event.stopImmediatePropagation();if(window.KayAuth?.openAccount){window.KayAuth.openAccount();return}const source=data(),users=arr(raw(source).users),current=source.currentUser||users.find(user=>user.isActive)||{};
      showPopover(button,'Compte actif',[{icon:'U',title:current.displayName||'Utilisateur KAY ONE',text:current.email||'Session locale'},{icon:'MDP',title:'Authentification',text:'Mot de passe local'},{icon:'⚙',title:'Habilitations',text:'Gérées dans Sécurité & rôles'}]);return;
    }
  },true);
  renderProduction();
})();
