(function installKayAuthentication(){
  'use strict';
  const state={mode:'loading',email:'',user:null,pendingEmail:'',message:''};
  const esc=value=>String(value??'').replace(/[&<>"']/g,char=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[char]));
  const post=(type,payload={})=>window.chrome?.webview?.postMessage({type,payload});
  const layer=document.createElement('div');layer.id='kay-auth-layer';layer.className='kay-auth-layer';document.body.appendChild(layer);

  function logo(){return '<div class="kay-auth-logo"><span>K</span><div><strong>KAY ONE</strong><small>FINANCIAL & BUSINESS OS</small></div></div>'}
  function render(){
    layer.hidden=false;
    if(state.mode==='loading'){layer.innerHTML=`<section class="kay-auth-card kay-auth-loading">${logo()}<div class="kay-auth-spinner"></div><p>Vérification de la session sécurisée…</p></section>`;return}
    if(state.mode==='setup'){
      layer.innerHTML=`<section class="kay-auth-card">${logo()}<header><span>PREMIÈRE INITIALISATION</span><h1>Créer l’administrateur</h1><p>Définissez l’accès propriétaire de cette installation. Le mot de passe est dérivé avec PBKDF2 et n’est jamais stocké en clair.</p></header><form data-auth-form="setup"><label><span>Nom complet</span><input name="displayName" autocomplete="name" required value="Administrateur KAY"></label><label><span>Adresse e-mail</span><input name="email" type="email" autocomplete="username" required value="${esc(state.email||'admin@kayone.ma')}"></label><label><span>Mot de passe</span><input name="password" type="password" autocomplete="new-password" minlength="12" required></label><label><span>Confirmer le mot de passe</span><input name="confirmation" type="password" autocomplete="new-password" minlength="12" required></label><div class="kay-auth-requirements">12 caractères minimum. Utilisez une phrase de passe unique.</div>${message()}<button type="submit" class="primary-button">Initialiser KAY ONE</button></form></section>`;return
    }
    layer.innerHTML=`<section class="kay-auth-card kay-auth-compact">${logo()}<header><span>ESPACE SÉCURISÉ</span><h1>Connexion</h1><p>Accédez uniquement aux sociétés et fonctions autorisées pour votre rôle.</p></header><form data-auth-form="login"><label><span>Adresse e-mail</span><input name="email" type="email" autocomplete="username" required value="${esc(state.email)}"></label><label><span>Mot de passe</span><input name="password" type="password" autocomplete="current-password" required></label>${message()}<button type="submit" class="primary-button">Se connecter</button></form><footer>Les tentatives de connexion et actions sensibles sont inscrites dans l’Audit Trail.</footer></section>`;
  }
  function message(){return state.message?`<div class="kay-auth-message" role="alert">${esc(state.message)}</div>`:''}
  function setMode(mode,details={}){state.mode=mode;state.message=details.message||'';if(details.email!==undefined)state.email=details.email||'';render();setTimeout(()=>layer.querySelector('[autofocus],input')?.focus(),0)}
  function submit(event){
    const form=event.target.closest('[data-auth-form]');if(!form)return;event.preventDefault();if(!form.reportValidity())return;
    const payload=Object.fromEntries(new FormData(form).entries());state.message='';
    if(form.dataset.authForm==='setup'){
      if(payload.password!==payload.confirmation){state.message='Les deux mots de passe ne correspondent pas.';render();return}
      delete payload.confirmation;post('setup-admin',payload);disable(form,'Initialisation…');return;
    }
    if(form.dataset.authForm==='login'){state.pendingEmail=payload.email;post('login',payload);disable(form,'Vérification…');return}
  }
  function disable(form,label){const button=form.querySelector('[type="submit"]');if(button){button.disabled=true;button.textContent=label}}
  function openAccount(){
    document.querySelector('.kay-account-layer')?.remove();const account=document.createElement('div');account.className='kay-account-layer';
    const user=state.user||{};account.innerHTML=`<section class="kay-account-card" role="dialog" aria-modal="true"><header><div class="avatar">${esc(String(user.displayName||'KO').split(/\s+/).slice(0,2).map(x=>x[0]).join('').toUpperCase())}</div><div><strong>${esc(user.displayName||'Utilisateur KAY ONE')}</strong><span>${esc(user.email||'')}</span></div><button type="button" data-auth-action="close-account" aria-label="Fermer">×</button></header><div class="kay-account-security"><span>Session protégée</span><strong>Mot de passe</strong></div><div class="kay-account-actions"><button type="button" class="secondary-button" data-auth-action="security">Sécurité & rôles</button><button type="button" class="kay-auth-logout" data-auth-action="logout">Se déconnecter</button></div></section>`;document.body.appendChild(account);
  }
  document.addEventListener('submit',submit,true);
  document.addEventListener('click',event=>{const control=event.target.closest('[data-auth-action]'),action=control?.dataset.authAction;if(!action)return;event.preventDefault();event.stopImmediatePropagation();if(action==='close-account')event.target.closest('.kay-account-layer')?.remove();if(action==='security'){event.target.closest('.kay-account-layer')?.remove();navigate('security')}if(action==='logout'){event.target.closest('.kay-account-layer')?.remove();post('logout');setMode('loading')}},true);
  if(window.chrome?.webview&&typeof window.chrome.webview.addEventListener==='function')window.chrome.webview.addEventListener('message',event=>{
    const message=event.data;if(!message||typeof message!=='object')return;
    if(message.type==='auth-required')setMode(message.setup?'setup':'login',{email:message.email||'',message:message.message||''});
    if(message.type==='auth-error')setMode(message.setup?'setup':'login',{email:message.email||state.pendingEmail||state.email,message:message.message||'Authentification impossible.'});
    if(message.type==='auth-success'){state.user=message.user||null;state.mode='authenticated';state.message='';layer.hidden=true;layer.innerHTML=''}
    if(message.type==='session-expired')setMode('login',{email:state.user?.email||'',message:'Votre session a expiré. Reconnectez-vous.'});
  });
  window.runAuthUiChecks=()=>{const checks={layer_present:Boolean(document.querySelector('#kay-auth-layer')),authenticated_state:state.mode==='authenticated'&&layer.hidden,account_api:typeof openAccount==='function'};if(state.mode==='authenticated'){openAccount();const account=document.querySelector('.kay-account-layer');checks.account_dialog=Boolean(account?.querySelector('[role="dialog"]'));checks.security_action=Boolean(account?.querySelector('[data-auth-action="security"]'));checks.logout_action=Boolean(account?.querySelector('[data-auth-action="logout"]'));account?.remove()}else{checks.account_dialog=false;checks.security_action=false;checks.logout_action=false}const values=Object.values(checks);return{ok:values.every(Boolean),total:values.length,passed:values.filter(Boolean).length,checks}};
  window.KayAuth=Object.freeze({openAccount,user:()=>state.user,authenticated:()=>state.mode==='authenticated'});
  render();post('auth-state');
})();
