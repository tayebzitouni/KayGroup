/*
 * KAY ONE enterprise SPA integration.
 *
 * Loading assumptions:
 *  - This file is loaded as a classic script, after app.js, core-workflows.js
 *    and enterprise-modules.js. It intentionally uses the classic-script
 *    globals `pages`, `nav`, `state`, `render`, `navigate` and `showToast`.
 *  - KayEnterprise receives a merged view of state.data and
 *    state.data.enterprise. Enterprise values take precedence while the
 *    classic totals remain available.
 *  - Native commands use the existing WebView contract `{ type, payload }`.
 *    `navigate` is always handled inside the SPA and is never sent natively.
 *  - Navigation is two levels (section -> page). Tabs/details rendered by a
 *    page form the optional third level; no deeper navigation is introduced.
 */
(function installKayEnterpriseIntegration() {
  'use strict';

  const routeMeta = Object.freeze({
    operations: { label: 'Centre des opérations', icon: 'journal', section: 'Opérations' },
    clients: { label: 'Clients 360°', icon: 'users', section: 'Cycles métier' },
    suppliers: { label: 'Fournisseurs 360°', icon: 'building', section: 'Cycles métier' },
    contracts: { label: 'Contrats & engagements', icon: 'calendar', section: 'Cycles métier' },
    imports: { label: 'Dossiers import', icon: 'download', section: 'Cycles métier' },
    reporting: { label: 'Reporting & BI', icon: 'trending', section: 'Pilotage' },
    audit: { label: 'Audit Trail', icon: 'journal', section: 'Finance & contrôle' },
    reconciliation: { label: 'Rapprochement', icon: 'bank', section: 'Finance & contrôle' },
    exemptions: { label: 'Exonérations', icon: 'tax', section: 'Finance & contrôle' },
    security: { label: 'Sécurité & rôles', icon: 'settings', section: 'Système' },
    assistant: { label: 'Assistant IA', icon: 'help', section: 'Pilotage' }
  });

  const expectedTitles = Object.freeze({
    operations: 'Registre des opérations',
    clients: 'Clients 360°',
    clients360: 'Clients 360°',
    suppliers: 'Fournisseurs 360°',
    suppliers360: 'Fournisseurs 360°',
    contracts: 'Contrats & engagements',
    imports: 'Dossiers import',
    reporting: 'Reporting & BI',
    audit: 'Audit Trail',
    reconciliation: 'Rapprochement bancaire',
    exemptions: 'Certificats d’exonération',
    security: 'Sécurité & rôles',
    assistant: 'Assistant financier IA'
  });

  const missing = [];
  if (typeof pages !== 'object' || !pages) missing.push('pages');
  if (!Array.isArray(typeof nav === 'undefined' ? null : nav)) missing.push('nav');
  if (typeof state !== 'object' || !state) missing.push('state');
  if (typeof render !== 'function') missing.push('render');
  if (typeof navigate !== 'function') missing.push('navigate');
  if (!window.KayEnterprise || typeof window.KayEnterprise.render !== 'function' || typeof window.KayEnterprise.bind !== 'function') missing.push('window.KayEnterprise');

  if (missing.length) {
    const reason = `Enterprise integration not installed; missing: ${missing.join(', ')}`;
    console.error(reason);
    window.runEnterpriseUiChecks = function runUnavailableEnterpriseChecks() {
      return { ok: false, total: 1, passed: 0, failed: 1, checks: { prerequisites: false }, errors: [reason], visited: [] };
    };
    return;
  }

  if (window.KayEnterpriseIntegration?.installed) return;

  const enterprise = window.KayEnterprise;
  const enterpriseRoutes = Object.keys(enterprise.pages);
  const enterpriseRouteSet = new Set(enterpriseRoutes);
  const canonicalRoutes = Object.keys(routeMeta);
  let detachCurrentPage = null;
  let checkMode = false;
  let suppressedMessages = [];

  function enterpriseData() {
    const classic = state.data && typeof state.data === 'object' ? state.data : {};
    const extended = classic.enterprise && typeof classic.enterprise === 'object' ? classic.enterprise : {};
    return {
      ...classic,
      ...extended,
      totals: { ...(classic.totals || {}), ...(extended.totals || {}) }
    };
  }

  function registerPages() {
    for (const route of enterpriseRoutes) {
      pages[route] = function renderRegisteredEnterprisePage() {
        return enterprise.render(route, enterpriseData());
      };
    }
  }

  function navItem(id, label, icon, parent) {
    const existing = nav.find(item => item && item.id === id) || {};
    return { ...existing, id, label, icon, parent, level: 2, enterprise: enterpriseRouteSet.has(id) };
  }

  function installNavigation() {
    const blueprint = [
      { section: 'Pilotage', level: 1 },
      navItem('dashboard', 'Tableau de bord', 'home', 'Pilotage'),
      navItem('reporting', routeMeta.reporting.label, routeMeta.reporting.icon, 'Pilotage'),
      navItem('assistant', routeMeta.assistant.label, routeMeta.assistant.icon, 'Pilotage'),

      { section: 'Opérations', level: 1 },
      navItem('operation', 'Nouvelle opération', 'plus', 'Opérations'),
      navItem('operations', routeMeta.operations.label, routeMeta.operations.icon, 'Opérations'),

      { section: 'Cycles métier', level: 1 },
      navItem('sales', 'Ventes', 'sales', 'Cycles métier'),
      navItem('purchases', 'Achats', 'cart', 'Cycles métier'),
      navItem('clients', routeMeta.clients.label, routeMeta.clients.icon, 'Cycles métier'),
      navItem('suppliers', routeMeta.suppliers.label, routeMeta.suppliers.icon, 'Cycles métier'),
      navItem('contracts', routeMeta.contracts.label, routeMeta.contracts.icon, 'Cycles métier'),
      navItem('imports', routeMeta.imports.label, routeMeta.imports.icon, 'Cycles métier'),

      { section: 'Finance & contrôle', level: 1 },
      navItem('treasury', 'Trésorerie', 'bank', 'Finance & contrôle'),
      navItem('reconciliation', routeMeta.reconciliation.label, routeMeta.reconciliation.icon, 'Finance & contrôle'),
      navItem('aged', 'Balance âgée', 'clock', 'Finance & contrôle'),
      navItem('tax', 'Fiscalité', 'tax', 'Finance & contrôle'),
      navItem('exemptions', routeMeta.exemptions.label, routeMeta.exemptions.icon, 'Finance & contrôle'),
      navItem('accounting', 'Comptabilité', 'book', 'Finance & contrôle'),
      navItem('journal', 'Journal global', 'journal', 'Finance & contrôle'),
      navItem('audit', routeMeta.audit.label, routeMeta.audit.icon, 'Finance & contrôle'),

      { section: 'Système', level: 1 },
      navItem('admin', 'Administration', 'settings', 'Système'),
      navItem('security', routeMeta.security.label, routeMeta.security.icon, 'Système')
    ];

    nav.splice(0, nav.length, ...blueprint);
  }

  function normaliseMessage(typeOrMessage, payload) {
    if (typeOrMessage && typeof typeOrMessage === 'object') {
      return {
        type: String(typeOrMessage.type || ''),
        payload: typeOrMessage.payload && typeof typeOrMessage.payload === 'object' ? typeOrMessage.payload : {}
      };
    }
    return { type: String(typeOrMessage || ''), payload: payload && typeof payload === 'object' ? payload : {} };
  }

  function sendEnterpriseMessage(typeOrMessage, payload) {
    const message = normaliseMessage(typeOrMessage, payload);
    if (!message.type) return;

    if (checkMode) {
      suppressedMessages.push(message);
      return;
    }

    if (message.type === 'navigate') {
      const target = message.payload.page;
      if (typeof target === 'string' && pages[target]) {
        queueMicrotask(() => {
          if (state.page !== target) navigate(target);
        });
      }
      return;
    }

    if (window.chrome?.webview && typeof window.chrome.webview.postMessage === 'function') {
      window.chrome.webview.postMessage(message);
      return;
    }

    window.dispatchEvent(new CustomEvent('kayone:enterprise-command', { detail: message }));
  }

  function disposeEnterpriseBinding() {
    if (typeof detachCurrentPage === 'function') detachCurrentPage();
    detachCurrentPage = null;
  }

  const renderClassicSpa = render;
  function renderIntegratedSpa() {
    disposeEnterpriseBinding();
    const result = renderClassicSpa.apply(this, arguments);
    const route = state.page;
    if (!enterpriseRouteSet.has(route)) return result;

    const root = document.querySelector('#app-content');
    if (!root) return result;

    /*
     * app.js has a broad document-level click handler. Stop enterprise actions
     * at #app-content after the module's root-level listeners have had access,
     * preventing duplicate toasts and a second, generic action handler.
     */
    const isolation = new AbortController();
    root.addEventListener('click', event => {
      const action = event.target.closest('button[data-action]');
      if (action && root.contains(action)) event.stopPropagation();
    }, { signal: isolation.signal });

    const unbindModule = enterprise.bind(route, enterpriseData(), sendEnterpriseMessage);
    root.dataset.enterprisePage = route;
    detachCurrentPage = function detachBoundEnterprisePage() {
      isolation.abort();
      if (typeof unbindModule === 'function') unbindModule();
    };
    return result;
  }

  function handleNativeEnterpriseMessage(event) {
    const message = event?.data;
    if (!message || typeof message !== 'object') return;

    if (message.type === 'enterprise-snapshot') {
      const incoming = message.data || message.payload || {};
      state.data = {
        ...(state.data || {}),
        enterprise: { ...((state.data || {}).enterprise || {}), ...incoming }
      };
      renderIntegratedSpa();
      return;
    }

    if (message.type === 'enterprise-operation-saved') {
      const reference = message.reference || message.payload?.reference || message.data?.reference || '';
      if (typeof showToast === 'function') showToast(`Opération ${reference} enregistrée`.trim());
      navigate('operations');
      return;
    }

    if (message.type === 'enterprise-action-completed' && typeof showToast === 'function') {
      showToast(message.message || 'Action enregistrée avec succès.');
    }

    if (message.type === 'enterprise-error' && typeof showToast === 'function') {
      showToast(message.message || 'Une erreur est survenue.', 'error');
    }
  }

  function runEnterpriseUiChecks() {
    const checks = {};
    const errors = [];
    const visited = [];
    const originalPage = state.page;
    const originalOperationType = state.operationType;
    const originalScrollX = window.scrollX;
    const originalScrollY = window.scrollY;
    suppressedMessages = [];
    checkMode = true;

    const check = (name, value) => { checks[name] = Boolean(value); };
    const capture = (name, action) => {
      try { check(name, action()); }
      catch (error) { checks[name] = false; errors.push(`${name}: ${error?.message || error}`); }
    };
    const slug = value => value.replace(/[^a-z0-9]+/gi, '_').replace(/^_|_$/g, '').toLowerCase();

    const requiredSelectors = {
      operations: ['[data-enterprise-row]', '[data-action="new-operation"]'],
      clients: ['[data-role="entity-detail"]', '[data-action="select-client"]'],
      clients360: ['[data-role="entity-detail"]', '[data-action="select-client"]'],
      suppliers: ['[data-role="entity-detail"]', '[data-action="select-supplier"]'],
      suppliers360: ['[data-role="entity-detail"]', '[data-action="select-supplier"]'],
      contracts: ['.ke-timeline', '[data-action="generate-commitments"]'],
      imports: ['[data-import-cost]', '[data-action="save-import-costing"]'],
      reporting: ['.ke-report-grid', '[data-action="refresh-report"]'],
      audit: ['[data-enterprise-row]', '[data-action="verify-audit-integrity"]'],
      reconciliation: ['.ke-row-check', '[data-action="reconcile-selected"]'],
      exemptions: ['[data-role="certificate-client"]', '[data-action="check-certificate-balance"]'],
      security: ['[data-permission]', '[data-action="save-security"]'],
      assistant: ['[data-role="assistant-form"]', '[data-action="assistant-prompt"]']
    };

    try {
      check('prerequisites', true);
      check('all_enterprise_routes_registered', enterpriseRoutes.every(route => typeof pages[route] === 'function'));
      check('canonical_navigation_complete', canonicalRoutes.every(route => nav.some(item => item.id === route)));
      check('navigation_has_unique_pages', new Set(nav.filter(item => item.id).map(item => item.id)).size === nav.filter(item => item.id).length);
      check('navigation_depth_max_three', nav.every(item => !item.level || item.level <= 2));

      for (const route of enterpriseRoutes) {
        navigate(route);
        visited.push(route);
        const key = slug(route);
        const root = document.querySelector('#app-content');
        const actions = root ? [...root.querySelectorAll('button[data-action]')] : [];
        check(`page_${key}_state`, state.page === route);
        check(`page_${key}_title`, root?.querySelector('h1')?.textContent.trim() === expectedTitles[route]);
        check(`page_${key}_not_missing`, !root?.querySelector('.ke-missing'));
        check(`page_${key}_bound`, root?.dataset.enterprisePage === route && Boolean(root.__kayEnterpriseController));
        check(`page_${key}_actions`, actions.length > 0 && actions.every(button => Boolean(button.dataset.action) && !button.disabled));
        check(`page_${key}_controls`, (requiredSelectors[route] || []).every(selector => Boolean(root?.querySelector(selector))));
        if (route === 'reporting') check('reporting_profit_before_tax_excludes_vat_label', !/marge moins impacts fiscaux/i.test(root?.textContent || ''));

        const search = root?.querySelector('[data-role="enterprise-search"]');
        const rows = root ? [...root.querySelectorAll('[data-enterprise-row]')] : [];
        if (search && rows.length) {
          search.value = '__kay_one_no_match__';
          search.dispatchEvent(new Event('input', { bubbles: true }));
          check(`page_${key}_search`, rows.every(row => row.hidden));
          search.value = '';
          search.dispatchEvent(new Event('input', { bubbles: true }));
        }
      }

      navigate('operation');
      const operationRoot = document.querySelector('#app-content');
      const expectedTypes = ['Achat', 'Vente', 'Encaissement', 'Décaissement', 'Banque', 'Caisse', 'Import', 'Export', 'Immobilisation', 'Paie', 'Fiscalité', 'Note de frais', 'Opération diverse', 'Autre'];
      const typeButtons = operationRoot ? [...operationRoot.querySelectorAll('[data-operation-type]')] : [];
      check('operation_form_present', Boolean(operationRoot?.querySelector('#enterprise-operation-form')));
      check('operation_fourteen_types', typeButtons.length === expectedTypes.length && expectedTypes.every(type => typeButtons.some(button => button.dataset.operationType === type)));
      check('operation_analytic_dimensions', ['company', 'site', 'laboratory', 'activity', 'costCenter', 'project'].every(name => Boolean(operationRoot?.querySelector(`[name="${name}"]`))));
      check('operation_document_control', Boolean(operationRoot?.querySelector('#operation-document')));

      const currency = operationRoot?.querySelector('#enterprise-currency');
      const amount = operationRoot?.querySelector('#enterprise-amount');
      if (currency && amount) {
        currency.value = 'EUR';
        currency.dispatchEvent(new Event('change', { bubbles: true }));
        amount.value = '1200';
        amount.dispatchEvent(new Event('input', { bubbles: true }));
        const fx = document.querySelector('#fx-fields');
        const equivalent = document.querySelector('#mad-equivalent');
        check('operation_fx_progressive_disclosure', Boolean(fx) && !fx.hidden);
        check('operation_currency_suffix', document.querySelector('#amount-currency')?.textContent === 'EUR');
        check('operation_fx_preview', Boolean(equivalent?.value) && !/^0(?:[,.]0+)?(?:\s*MAD)?$/.test(equivalent.value.trim()));
        currency.value = 'MAD';
        currency.dispatchEvent(new Event('change', { bubbles: true }));
        check('operation_mad_hides_fx', Boolean(document.querySelector('#fx-fields')?.hidden));
      } else {
        check('operation_fx_progressive_disclosure', false);
        check('operation_currency_suffix', false);
        check('operation_fx_preview', false);
        check('operation_mad_hides_fx', false);
      }

      const assetButton = [...document.querySelectorAll('[data-operation-type]')].find(button => button.dataset.operationType === 'Immobilisation');
      assetButton?.click();
      check('operation_asset_fields', Boolean(document.querySelector('#asset-fields')) && !document.querySelector('#asset-fields').hidden);

      const importButton = [...document.querySelectorAll('[data-operation-type]')].find(button => button.dataset.operationType === 'Import');
      importButton?.click();
      check('operation_import_fields', Boolean(document.querySelector('#import-fields')) && !document.querySelector('#import-fields').hidden);
      check('no_native_side_effects', suppressedMessages.length === 0);
    } catch (error) {
      checks.unexpected_exception = false;
      errors.push(error?.stack || error?.message || String(error));
    } finally {
      state.operationType = originalOperationType;
      try { navigate(originalPage); }
      catch (error) { errors.push(`restore_page: ${error?.message || error}`); }
      window.scrollTo({ left: originalScrollX, top: originalScrollY, behavior: 'instant' });
      checkMode = false;
    }

    const values = Object.values(checks);
    const passed = values.filter(Boolean).length;
    return {
      ok: passed === values.length && errors.length === 0,
      total: values.length,
      passed,
      failed: values.length - passed,
      checks,
      errors,
      visited,
      suppressedMessages: suppressedMessages.map(message => message.type)
    };
  }

  registerPages();
  installNavigation();
  render = renderIntegratedSpa;
  window.render = renderIntegratedSpa;
  window.runEnterpriseUiChecks = runEnterpriseUiChecks;

  if (window.chrome?.webview && typeof window.chrome.webview.addEventListener === 'function') {
    window.chrome.webview.addEventListener('message', handleNativeEnterpriseMessage);
  }

  window.KayEnterpriseIntegration = Object.freeze({
    installed: true,
    routes: Object.freeze([...enterpriseRoutes]),
    navigationRoutes: Object.freeze([...canonicalRoutes]),
    data: enterpriseData,
    send: sendEnterpriseMessage,
    renderCurrent: renderIntegratedSpa
  });

  renderIntegratedSpa();
})();
