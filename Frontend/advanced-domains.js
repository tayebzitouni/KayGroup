(function () {
  'use strict';

  const ICONS = {
    plus: '<path d="M12 5v14M5 12h14"/>', download: '<path d="M12 3v12m-5-5 5 5 5-5M5 21h14"/>',
    search: '<circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/>', filter: '<path d="M4 5h16M7 12h10M10 19h4"/>',
    more: '<circle cx="5" cy="12" r="1"/><circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/>', check: '<path d="m5 12 4 4L19 6"/>',
    close: '<path d="m6 6 12 12M18 6 6 18"/>', arrow: '<path d="M5 12h14M13 6l6 6-6 6"/>',
    wallet: '<path d="M20 7V5a2 2 0 0 0-2-2H5a3 3 0 0 0 0 6h15v12H5a3 3 0 0 1-3-3V6"/><path d="M16 13h2"/>',
    bank: '<path d="m3 10 9-6 9 6M5 10v8m4-8v8m6-8v8m4-8v8M3 21h18M2 18h20"/>',
    globe: '<circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a15 15 0 0 1 0 18M12 3a15 15 0 0 0 0 18"/>',
    receipt: '<path d="M6 2h12v20l-3-2-3 2-3-2-3 2V2Z"/><path d="M9 7h6M9 11h6M9 15h3"/>',
    users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>',
    percent: '<path d="M19 5 5 19M7 5h.01M17 19h.01"/><circle cx="7" cy="5" r="2"/><circle cx="17" cy="19" r="2"/>',
    asset: '<rect x="3" y="8" width="18" height="12" rx="2"/><path d="M7 8V5h10v3M8 13h8M8 17h5"/>',
    cash: '<rect x="3" y="6" width="18" height="13" rx="2"/><circle cx="12" cy="12.5" r="3"/><path d="M7 9h.01M17 16h.01"/>',
    tax: '<path d="M6 2h9l4 4v16H6z"/><path d="M14 2v5h5M9 13h6M9 17h6"/>',
    file: '<path d="M6 2h9l4 4v16H6z"/><path d="M14 2v5h5"/>', upload: '<path d="M12 16V4m-5 5 5-5 5 5M5 20h14"/>',
    scan: '<path d="M4 8V4h4M16 4h4v4M20 16v4h-4M8 20H4v-4M7 12h10"/>',
    building: '<rect x="4" y="3" width="16" height="18" rx="2"/><path d="M9 21v-4h6v4M8 7h.01M12 7h.01M16 7h.01M8 11h.01M12 11h.01M16 11h.01"/>',
    chart: '<path d="M4 20V10m6 10V4m6 16v-7m5 7H2"/>', clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    alert: '<path d="M10.3 3.2 2.1 17.4A2 2 0 0 0 3.8 20h16.4a2 2 0 0 0 1.7-2.6L13.7 3.2a2 2 0 0 0-3.4 0Z"/><path d="M12 9v4M12 17h.01"/>',
    link: '<path d="M10 13a5 5 0 0 0 7.1.1l2-2a5 5 0 0 0-7.1-7.1l-1.1 1.1"/><path d="M14 11a5 5 0 0 0-7.1-.1l-2 2A5 5 0 0 0 12 20l1.1-1.1"/>',
    refresh: '<path d="M20 6v5h-5M4 18v-5h5"/><path d="M18.5 9A7 7 0 0 0 6 6.5L4 11m16 2-2 4.5A7 7 0 0 1 5.5 15"/>',
    calendar: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M16 3v4M8 3v4M3 11h18"/>',
    map: '<path d="m3 6 6-3 6 3 6-3v15l-6 3-6-3-6 3V6Z"/><path d="M9 3v15M15 6v15"/>',
    calculator: '<rect x="5" y="2" width="14" height="20" rx="2"/><path d="M8 6h8v4H8zM8 14h.01M12 14h.01M16 14h.01M8 18h.01M12 18h.01M16 18h.01"/>',
    shield: '<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z"/><path d="m9 12 2 2 4-5"/>'
  };

  const esc = value => String(value == null ? '' : value).replace(/[&<>'"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[c]));
  const read = (item, ...keys) => { for (const key of keys) if (item && item[key] != null) return item[key]; return ''; };
  const sources = data => [data, data?.enterprise, data?.raw, data?.enterprise?.raw, data?.snapshot].filter(Boolean);
  const list = (data, ...keys) => { for (const source of sources(data)) for (const key of keys) if (Array.isArray(source?.[key])) return source[key]; return []; };
  const rawList = (data, ...keys) => { const candidates = [data?.raw, data?.enterprise?.raw, data, data?.enterprise, data?.snapshot].filter(Boolean); for (const source of candidates) for (const key of keys) if (Array.isArray(source?.[key])) return source[key]; return []; };
  const numeric = value => {
    if (typeof value === 'number') return Number.isFinite(value) ? value : 0;
    const normalized = String(value ?? '').replace(/[\s\u00a0\u202f]/g, '').replace(',', '.');
    const match = normalized.match(/-?\d+(?:\.\d+)?/);
    return match ? Number(match[0]) : 0;
  };
  const number = value => new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 2 }).format(numeric(value)).replaceAll('\u202f', ' ');
  const money = (value, currency = 'MAD') => `${number(value)} ${esc(currency || 'MAD')}`;
  const shortDate = value => { if (!value) return '—'; const parsed = new Date(value); return Number.isNaN(parsed.valueOf()) ? esc(value) : parsed.toLocaleDateString('fr-FR'); };
  const svg = name => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${ICONS[name] || ICONS.more}</svg>`;
  const button = (action, label, kind = 'secondary', icon = '', attrs = '') => `<button type="button" class="${kind}-button ka-button" data-action="${esc(action)}" ${attrs}>${icon ? svg(icon) : ''}<span>${esc(label)}</span></button>`;
  const iconButton = (action, label, icon = 'more', attrs = '') => `<button type="button" class="ka-icon-button" data-action="${esc(action)}" aria-label="${esc(label)}" title="${esc(label)}" ${attrs}>${svg(icon)}</button>`;
  const pageHead = (title, subtitle, actions = '') => `<div class="page-head ka-page-head"><div><div class="eyebrow">KAY ONE · TRANSACTION ENGINE</div><h1>${esc(title)}</h1><p>${esc(subtitle)}</p></div><div class="page-actions">${actions}</div></div>`;
  const tone = value => /payé|paid|valid|verified|active|actif|traité|executed|approved|rapproch|reconciled|succès|ouvert|open|conforme/i.test(value) ? 'paid' : /rejet|rejected|retard|overdue|expir|bloqu|erreur|annul|cancelled/i.test(value) ? 'late' : /soumis|submitted|attente|pending|prépar|prepared|à payer|ocr|uploaded|expected|révision|forecast/i.test(value) ? 'pending' : 'draft';
  const badge = value => `<span class="status ${tone(value)}">${esc(value || 'Non renseigné')}</span>`;
  const ref = item => read(item, 'reference', 'id', 'code', 'number') || '—';
  const sum = (items, getter) => items.reduce((total, item) => total + numeric(getter(item)), 0);
  const metric = (label, value, meta, icon, semantic = '') => `<article class="panel ka-metric ${semantic ? `ka-${semantic}` : ''}"><div><span>${esc(label)}</span><i>${svg(icon)}</i></div><strong>${value == null || value === '' ? '—' : esc(value)}</strong><small>${esc(meta || 'Aucune donnée disponible')}</small></article>`;
  const searchBar = (placeholder, filters = '') => `<div class="filters ka-toolbar"><label class="ka-search">${svg('search')}<input data-role="ka-search" placeholder="${esc(placeholder)}" aria-label="${esc(placeholder)}"></label>${filters}<span class="filter-spacer"></span>${button('toggle-advanced-filters', 'Filtres avancés', 'secondary', 'filter')}<div class="ka-advanced-filters"><label>Date de début<input type="date" data-filter="start"></label><label>Date de fin<input type="date" data-filter="end"></label><label>Montant minimum<input type="number" min="0" data-filter="minimum" placeholder="0"></label><label>Devise<select data-filter="currency"><option value="">Toutes</option><option>MAD</option><option>EUR</option><option>USD</option></select></label></div></div>`;
  const empty = (title, text, action = '', label = '') => `<div class="ka-empty">${svg('file')}<h3>${esc(title)}</h3><p>${esc(text)}</p>${action ? button(action, label || 'Créer le premier élément', 'primary', 'plus') : ''}</div>`;
  const table = (headers, rows, count) => `<article class="panel table-panel ka-table-panel"><div class="ka-table-scroll"><table class="data-table ka-table"><thead><tr>${headers.map(h => `<th>${esc(h)}</th>`).join('')}</tr></thead><tbody>${rows || `<tr class="ka-empty-row"><td colspan="${headers.length}">${empty('Aucun enregistrement', 'Les données apparaîtront ici dès leur création dans KAY ONE.')}</td></tr>`}</tbody></table></div><footer><span data-role="ka-count">${number(count)} enregistrement${count === 1 ? '' : 's'}</span>${count > 10 ? `<div class="pagination-buttons"><button type="button" class="page-button active" data-action="advanced-page" data-page-number="1">1</button><button type="button" class="page-button" data-action="advanced-page" data-page-number="2">2</button><button type="button" class="page-button" data-action="advanced-page" data-page-number="next" aria-label="Page suivante">›</button></div>` : ''}</footer></article>`;

  function operations(data) { return list(data, 'operations', 'transactions'); }
  function parties(data) { return list(data, 'parties'); }
  function partyName(data, id) { const party = parties(data).find(item => String(read(item, 'id', 'internalCode')) === String(id)); return read(party, 'name') || id || '—'; }
  function companyName(data, id) { const item = list(data, 'companies').find(x => String(read(x, 'id')) === String(id)); return read(item, 'name', 'denomination') || id || '—'; }
  function financiallyActiveOperationIds(data) {
    return new Set(rawList(data, 'operations').filter(item => /^(validated|paymentpending|partiallypaid|paid|reconciled|posted|validée?|paiement en attente|partiellement payée?|payée?|rapprochée?|comptabilisée?)$/i.test(String(read(item, 'status')).replace(/\s+/g, ' ').trim())).map(item => String(read(item, 'id', 'operationId'))));
  }
  function isOpenDue(item) { return /^(open|partiallypaid|ouvert|partiellement payé)$/i.test(String(read(item, 'status')).replace(/\s+/g, ' ').trim()); }

  function payableItems(data) {
    const activeIds = financiallyActiveOperationIds(data), allDue = rawList(data, 'dueItems'), due = allDue.filter(item => {
      const kind = String(read(item, 'kind', 'type')).toLowerCase();
      return activeIds.has(String(read(item, 'operationId'))) && isOpenDue(item) && numeric(read(item, 'outstandingMad', 'outstanding', 'remainingAmountMad')) > 0 && (!kind || /supplier|fourn|payable|debt|dette|achat/.test(kind));
    });
    if (allDue.length) return due;
    return operations(data).filter(item => /achat|décaissement|import/i.test(read(item, 'type')) && !/payé|soldé|annulé/i.test(read(item, 'status'))).map(item => ({
      id: read(item, 'id'), reference: ref(item), operationId: read(item, 'id'), partyId: read(item, 'partyId'), party: read(item, 'party'), dueDate: read(item, 'dueDate'), currency: read(item, 'currency') || 'MAD', outstandingMad: read(item, 'netPayableMad', 'totalMad', 'amountMad', 'amount'), status: read(item, 'status') || 'À payer'
    }));
  }

  function renderPayments(data = {}) {
    const due = payableItems(data), payments = list(data, 'payments'), bankAccounts = rawList(data, 'bankAccounts'), rawOperations = rawList(data, 'operations');
    const total = sum(due, item => read(item, 'outstandingMad', 'outstanding', 'amountMad'));
    const international = due.filter(item => String(read(item, 'currency')).toUpperCase() !== 'MAD');
    const rows = due.map(item => { const amountMad = numeric(read(item, 'outstandingMad', 'outstanding', 'amountMad')), currency = read(item, 'currency') || 'MAD', id = ref(item), supplier = read(item, 'partyName', 'party') || partyName(data, read(item, 'partyId')), operationId = read(item, 'operationId'), operation = rawOperations.find(x => String(read(x, 'id')) === String(operationId)), rate = numeric(read(operation, 'exchangeRate')) || 1, paymentAmount = currency === 'MAD' ? amountMad : amountMad / rate; return `<tr data-ka-row data-date="${esc(read(item, 'dueDate'))}" data-amount="${amountMad}" data-currency="${esc(currency)}"><td><input class="ka-select-row" type="checkbox" data-id="${esc(id)}" data-operation-id="${esc(operationId)}" data-company-id="${esc(read(item, 'companyId') || read(operation, 'companyId'))}" data-currency="${esc(currency)}" data-payment-amount="${paymentAmount}" data-amount="${amountMad}" aria-label="Sélectionner ${esc(id)}"></td><td class="ref">${esc(id)}</td><td>${esc(supplier)}</td><td>${shortDate(read(item, 'dueDate'))}</td><td class="amount">${money(amountMad)}</td><td>${esc(currency)}</td><td>${badge(read(item, 'status') || 'À payer')}</td><td>${iconButton('open-payment-source', `Ouvrir ${id}`, 'more', `data-record="${esc(id)}"`)}</td></tr>`; }).join('');
    return `${pageHead('Préparation des paiements', 'Sélectionnez les échéances, séparez la préparation de la validation et traitez les virements internationaux.', `${button('export-payments', 'Exporter', 'secondary', 'download')}${button('create-payment-order', 'Créer un ordre de paiement', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Échéances à payer', due.length ? money(total) : null, due.length ? `${due.length} ligne${due.length > 1 ? 's' : ''} ouverte${due.length > 1 ? 's' : ''}` : '', 'wallet')}${metric('Sélection en cours', due.length ? money(0) : null, 'Cochez les lignes à préparer', 'check')}${metric('Paiements enregistrés', payments.length ? number(payments.length) : null, payments.length ? 'Données Transaction Engine' : '', 'bank')}${metric('Exposition en devises', international.length ? number(international.length) : null, international.length ? [...new Set(international.map(x => read(x, 'currency')))].join(' · ') : '', 'globe')}</section>
      <section class="grid ka-payment-layout"><div><div class="ka-selection-bar"><span><strong data-role="selected-count">0 échéance sélectionnée</strong><small data-role="selected-total">0 MAD</small></span>${button('prepare-selected-payments', 'Préparer la sélection', 'primary', 'check')}</div>${searchBar('Facture, fournisseur, montant…', `<select class="filter-control" data-filter="status"><option value="">Tous les statuts</option><option>À payer</option><option>En attente</option></select>`)}${table(['', 'Référence', 'Fournisseur', 'Échéance', 'Net à payer', 'Devise', 'Statut', ''], rows, due.length)}</div><aside class="panel ka-transfer-card"><span class="ka-overline">VIREMENT INTERNATIONAL</span><h2>Préparer un transfert</h2><p>Le cours, les frais, la RAS et l’écart de change restent reliés à l’opération source.</p><form data-form="international-transfer"><label>Opération source<select name="operationId" required><option value="">Sélectionner une dette en devise</option>${international.map(item => `<option value="${esc(read(item, 'operationId'))}">${esc(ref(item))} · ${esc(read(item, 'currency'))}</option>`).join('')}</select></label><label>Compte à débiter<select name="bankAccountId" required><option value="">Sélectionner</option>${bankAccounts.map(x => `<option value="${esc(read(x, 'id', 'name'))}">${esc(read(x, 'name'))} · ${esc(read(x, 'currency'))}</option>`).join('')}</select></label><label>Bénéficiaire<input name="beneficiary" required placeholder="Fournisseur étranger"></label><div class="ka-form-pair"><label>Montant<input name="amount" type="number" min="0.01" step="0.01" required></label><label>Devise<select name="currency" required><option>EUR</option><option>USD</option><option>GBP</option></select></label></div><div class="ka-form-pair"><label>Cours de règlement<input name="exchangeRate" type="number" min="0.0001" step="0.0001" required></label><label>Référence bancaire<input name="externalReference" placeholder="Référence / motif"></label></div><div class="ka-transfer-preview"><span>Contre-valeur estimée</span><strong data-role="transfer-countervalue">—</strong><small>Hors RAS éventuelle</small></div><button type="submit" class="primary-button ka-button" data-action="submit-international-transfer">${svg('globe')}<span>Soumettre pour validation</span></button></form></aside></section>`;
  }

  function expenseRecords(data) { return operations(data).filter(item => /note\s*de\s*frais|notedefrais|expense/i.test(`${read(item, 'type')} ${read(item, 'nature')}`)); }
  function renderExpenses(data = {}) {
    const items = expenseRecords(data), total = sum(items, x => read(x, 'totalMad', 'amountMad', 'amount')), awaiting = items.filter(x => /soumis|submitted|validation|attente|pending/i.test(read(x, 'status'))), paid = items.filter(x => /payé|paid|soldé/i.test(read(x, 'status')));
    const stages = ['Brouillon', 'Soumise', 'Validée', 'À payer', 'Payée'];
    const rows = items.map(item => { const id = ref(item), operationId = read(item, 'id') || id, statusValue = read(item, 'status') || 'Brouillon', employee = read(item, 'employeeName', 'party') || partyName(data, read(item, 'partyId')); return `<tr data-ka-row data-status="${esc(statusValue)}" data-date="${esc(read(item, 'operationDate', 'date'))}" data-amount="${numeric(read(item, 'totalMad', 'amountMad', 'amount'))}"><td class="ref">${esc(id)}</td><td>${shortDate(read(item, 'operationDate', 'date'))}</td><td>${esc(employee)}</td><td>${esc(read(item, 'nature', 'description'))}</td><td>${esc(companyName(data, read(item, 'companyId')))}</td><td class="amount">${money(read(item, 'totalMad', 'amountMad', 'amount'))}</td><td>${badge(statusValue)}</td><td><div class="ka-row-actions">${!(/payé|annulé/i.test(statusValue)) ? iconButton('advance-expense', 'Faire avancer le workflow', 'arrow', `data-record="${esc(operationId)}" data-reference="${esc(id)}" data-status="${esc(statusValue)}"`) : ''}${iconButton('open-expense', `Ouvrir ${id}`, 'more', `data-record="${esc(operationId)}"`)}</div></td></tr>`; }).join('');
    return `${pageHead('Notes de frais', 'Saisie → soumission → validation → mise en paiement → règlement, sans rupture de traçabilité.', `${button('export-expenses', 'Exporter', 'secondary', 'download')}${button('new-expense', 'Nouvelle note de frais', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Montant enregistré', items.length ? money(total) : null, items.length ? `${items.length} note${items.length > 1 ? 's' : ''}` : '', 'receipt')}${metric('En validation', awaiting.length ? money(sum(awaiting, x => read(x, 'totalMad', 'amountMad', 'amount'))) : null, awaiting.length ? `${awaiting.length} demande${awaiting.length > 1 ? 's' : ''}` : '', 'clock')}${metric('Payées', paid.length ? money(sum(paid, x => read(x, 'totalMad', 'amountMad', 'amount'))) : null, paid.length ? `${paid.length} note${paid.length > 1 ? 's' : ''}` : '', 'check', 'positive')}${metric('Justificatifs', items.length ? `${items.filter(x => read(x, 'documentId') || read(x, 'document')).length}/${items.length}` : null, items.length ? 'Pièces rattachées' : '', 'file')}</section>
      <article class="panel ka-workflow"><div><span class="ka-overline">WORKFLOW</span><h2>Traitement des notes de frais</h2></div><div>${stages.map((stage, index) => `<span><i>${index + 1}</i><b>${stage}</b><small>${items.filter(x => String(read(x, 'status')).toLowerCase().includes(stage.toLowerCase().replace('soumise', 'soumis').replace('validée', 'valid'))).length}</small></span>`).join('')}</div></article>
      ${searchBar('Référence, collaborateur, motif…', `<select class="filter-control" data-filter="status"><option value="">Tous les statuts</option>${stages.map(x => `<option>${x}</option>`).join('')}</select>`)}
      ${items.length ? table(['Référence', 'Date', 'Collaborateur', 'Motif', 'Société', 'Montant TTC', 'Workflow', ''], rows, items.length) : `<article class="panel">${empty('Aucune note de frais', 'Aucune opération de type « Note de frais » n’est encore enregistrée.', 'new-expense', 'Créer une note de frais')}</article>`}`;
  }

  function commissionRecords(data) {
    const explicit = list(data, 'commissions', 'commissionEntries');
    return explicit.length ? explicit : operations(data).filter(item => /commission/i.test(`${read(item, 'type')} ${read(item, 'nature')}`));
  }
  function renderCommissions(data = {}) {
    const items = commissionRecords(data), rules = list(data, 'commissionRules', 'tariffs').filter(x => /commission/i.test(`${read(x, 'kind', 'type')} ${read(x, 'name')}`)), total = sum(items, x => read(x, 'amountMad', 'totalMad', 'amount'));
    const rows = items.map(item => `<tr data-ka-row data-status="${esc(read(item, 'status'))}" data-date="${esc(read(item, 'operationDate', 'periodDate'))}" data-amount="${numeric(read(item, 'amountMad', 'totalMad', 'amount'))}"><td class="ref">${esc(ref(item))}</td><td>${esc(read(item, 'beneficiaryName', 'employeeName') || partyName(data, read(item, 'partyId')))}</td><td>${esc(read(item, 'basisLabel', 'nature', 'description'))}</td><td class="amount">${money(read(item, 'basisAmountMad', 'baseAmountMad'))}</td><td>${read(item, 'rate') !== '' ? `${number(read(item, 'rate'))} %` : '—'}</td><td class="amount">${money(read(item, 'amountMad', 'totalMad', 'amount'))}</td><td>${badge(read(item, 'status'))}</td><td>${iconButton('open-commission', `Ouvrir ${ref(item)}`, 'more', `data-record="${esc(ref(item))}"`)}</td></tr>`).join('');
    return `${pageHead('Moteur de commissions', 'Calculez les commissions depuis des règles paramétrables et conservez le lien avec chaque vente source.', `${button('export-commissions', 'Exporter', 'secondary', 'download')}${button('calculate-commissions', 'Lancer le calcul', 'secondary', 'calculator')}${button('new-commission-rule', 'Nouvelle règle', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Commissions calculées', items.length ? money(total) : null, items.length ? `${items.length} ligne${items.length > 1 ? 's' : ''}` : '', 'percent')}${metric('Règles actives', rules.length ? number(rules.filter(x => read(x, 'isActive') !== false).length) : null, rules.length ? `${rules.length} règle${rules.length > 1 ? 's' : ''} configurée${rules.length > 1 ? 's' : ''}` : '', 'calculator')}${metric('À valider', items.length ? number(items.filter(x => /attente|calculé|soumis/i.test(read(x, 'status'))).length) : null, items.length ? 'Séparation des tâches active' : '', 'clock')}${metric('Période', items.length ? esc(read(items[0], 'period', 'periodLabel') || 'Données disponibles') : null, items.length ? 'Dernier calcul enregistré' : '', 'calendar')}</section>
      <article class="panel ka-engine-note">${svg('calculator')}<div><strong>Calcul basé sur les données réelles</strong><p>Assiette de vente, encaissement, taux, seuil, plafond, annulation et avoir restent paramétrables. Aucun montant n’est estimé sans règle enregistrée.</p></div></article>
      ${items.length ? `${searchBar('Bénéficiaire, vente source, période…')}${table(['Référence', 'Bénéficiaire', 'Assiette', 'Base', 'Taux', 'Commission', 'Statut', ''], rows, items.length)}` : `<article class="panel">${empty('Aucune commission calculée', rules.length ? 'Des règles existent, mais aucun calcul de commission n’a encore été enregistré.' : 'Créez d’abord une règle de commission paramétrable.', rules.length ? 'calculate-commissions' : 'new-commission-rule', rules.length ? 'Lancer le premier calcul' : 'Créer une règle')}</article>`}`;
  }

  function assetRecords(data) {
    const explicit = list(data, 'fixedAssets', 'assets'); if (explicit.length) return explicit;
    return rawList(data, 'operations').filter(item => { const custom = read(item, 'customFields') || {}; return /immobilisation/i.test(read(item, 'type')) || /true|oui/i.test(read(custom, 'asset')) || /immobilisation/i.test(`${read(custom, 'treatment')} ${read(custom, 'materialTreatment')} ${read(custom, 'assetTreatment')}`); });
  }
  function assetValues(item) {
    const acquisition = numeric(read(item, 'acquisitionValueMad', 'amountMad', 'totalMad', 'amount'));
    const custom = read(item, 'customFields') || {};
    const duration = numeric(read(item, 'durationYears', 'depreciationYears') || read(custom, 'durationYears', 'depreciationYears')) || 0;
    const annual = duration ? acquisition / duration : numeric(read(item, 'annualDepreciationMad'));
    const accumulated = numeric(read(item, 'accumulatedDepreciationMad'));
    return { acquisition, duration, annual, accumulated, net: Math.max(0, acquisition - accumulated) };
  }
  function renderAssets(data = {}) {
    const items = assetRecords(data), values = items.map(assetValues), acquisition = values.reduce((s, x) => s + x.acquisition, 0), net = values.reduce((s, x) => s + x.net, 0), annual = values.reduce((s, x) => s + x.annual, 0);
    const rows = items.map((item, index) => { const v = values[index], id = ref(item), custom = read(item, 'customFields') || {}, category = read(item, 'category') || read(custom, 'category', 'assetTreatment', 'materialTreatment'), serviceDate = read(item, 'serviceDate', 'commissioningDate', 'operationDate', 'date') || read(custom, 'serviceDate'); return `<tr data-ka-row data-status="${esc(read(item, 'status'))}" data-date="${esc(serviceDate)}" data-amount="${v.acquisition}"><td class="ref">${esc(id)}</td><td><strong>${esc(read(item, 'name', 'nature', 'description'))}</strong><small>${esc(category)}</small></td><td>${esc(companyName(data, read(item, 'companyId')))}</td><td>${shortDate(serviceDate)}</td><td class="amount">${money(v.acquisition)}</td><td>${v.duration ? `${v.duration} ans` : '—'}</td><td class="amount">${v.annual ? money(v.annual) : '—'}</td><td class="amount">${money(v.net)}</td><td>${badge(read(item, 'status') || 'Actif')}</td><td>${iconButton('open-asset', `Ouvrir ${id}`, 'more', `data-record="${esc(id)}"`)}</td></tr>`; }).join('');
    return `${pageHead('Immobilisations & amortissements', 'De l’achat de matériel à la fiche immobilisation, la dotation et la localisation analytique.', `${button('export-assets', 'Exporter', 'secondary', 'download')}${button('run-depreciation', 'Calculer les dotations', 'secondary', 'calculator')}${button('new-asset', 'Nouvelle immobilisation', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Valeur d’acquisition', items.length ? money(acquisition) : null, items.length ? `${items.length} immobilisation${items.length > 1 ? 's' : ''}` : '', 'asset')}${metric('Valeur nette comptable', items.length ? money(net) : null, items.length ? 'Après amortissements enregistrés' : '', 'wallet')}${metric('Dotation annuelle', annual ? money(annual) : null, annual ? 'Selon durées paramétrées' : '', 'calculator')}${metric('Mises en service', items.length ? number(items.filter(x => read(x, 'serviceDate', 'commissioningDate')).length) : null, items.length ? 'Dates documentées' : '', 'calendar')}</section>
      ${items.length ? `${searchBar('Code immobilisation, désignation, site…')}${table(['Code', 'Immobilisation', 'Société', 'Mise en service', 'Acquisition', 'Durée', 'Dotation annuelle', 'VNC', 'Statut', ''], rows, items.length)}` : `<article class="panel">${empty('Aucune immobilisation enregistrée', 'Les opérations « Immobilisation » apparaîtront ici après validation de leur fiche d’actif.', 'new-asset', 'Enregistrer une immobilisation')}</article>`}`;
  }

  function renderCashBoxes(data = {}) {
    const boxes = list(data, 'cashBoxes'), moves = list(data, 'treasuryMovements').filter(x => read(x, 'cashBoxId') || /caisse|cash/i.test(`${read(x, 'kind')} ${read(x, 'type')}`));
    const total = sum(boxes, x => read(x, 'balanceMad', 'balance'));
    const boxesById = new Map(boxes.map(box => [String(read(box, 'id')), box]));
    const isCashOut = item => /out|sortie|décaissement|decaissement|outflow/i.test(String(read(item, 'direction', 'kind', 'type')));
    const cashBoxLabel = item => read(boxesById.get(String(read(item, 'cashBoxId'))), 'name') || read(item, 'cashBoxName', 'cashBoxId') || 'Caisse';
    const cards = boxes.map(box => { const boxMoves = moves.filter(x => String(read(x, 'cashBoxId')) === String(read(box, 'id'))), id = read(box, 'id'), boxStatus = read(box, 'status') || (read(box, 'isActive') === false ? 'Fermée' : 'Ouverte'); return `<article class="panel ka-cash-card" data-ka-card><header><span>${svg('cash')}</span><div><strong>${esc(read(box, 'name'))}</strong><small>${esc(companyName(data, read(box, 'companyId')))} · ${esc(read(box, 'currency') || 'MAD')}</small></div>${badge(boxStatus)}</header><div class="ka-cash-balance"><span>Solde disponible</span><strong>${money(read(box, 'balanceMad', 'balance'), read(box, 'currency') || 'MAD')}</strong></div><dl><div><dt>Mouvements</dt><dd>${boxMoves.length}</dd></div><div><dt>Dernière activité</dt><dd>${shortDate(read(boxMoves[0], 'movementDate', 'date'))}</dd></div></dl><footer>${button('new-cash-movement', 'Mouvement', 'secondary', 'plus', `data-record="${esc(id)}"`)}${iconButton('open-cash-box', `Ouvrir ${read(box, 'name')}`, 'arrow', `data-record="${esc(id)}"`)}</footer></article>`; }).join('');
    const moveRows = moves.map(item => { const outgoing = isCashOut(item), amount = Math.abs(numeric(read(item, 'amountMad', 'amount'))), direction = outgoing ? 'Sortie' : 'Entrée'; return `<tr data-ka-row data-date="${esc(read(item, 'movementDate', 'date'))}" data-amount="${amount}" data-direction="${direction}"><td>${shortDate(read(item, 'movementDate', 'date'))}</td><td class="ref">${esc(ref(item))}</td><td>${badge(direction)}</td><td>${esc(read(item, 'label', 'description', 'kind'))}</td><td>${esc(cashBoxLabel(item))}</td><td class="amount ${outgoing ? 'negative' : 'ka-positive-text'}">${outgoing ? '− ' : '+ '}${money(amount)}</td><td>${badge(read(item, 'status') || 'Comptabilisé')}</td><td>${iconButton('open-cash-movement', `Ouvrir ${ref(item)}`, 'more', `data-record="${esc(ref(item))}"`)}</td></tr>`; }).join('');
    return `${pageHead('Caisses multi-sites', 'Suivez les soldes, sessions et mouvements physiques par société et site.', `${button('export-cash-boxes', 'Exporter', 'secondary', 'download')}${button('new-cash-box', 'Nouvelle caisse', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Solde consolidé', boxes.length ? money(total) : null, boxes.length ? `${boxes.length} caisse${boxes.length > 1 ? 's' : ''}` : '', 'cash')}${metric('Caisses ouvertes', boxes.length ? number(boxes.filter(x => read(x, 'isActive') !== false && !/ferm/i.test(read(x, 'status'))).length) : null, boxes.length ? 'Toutes sociétés' : '', 'wallet')}${metric('Mouvements enregistrés', moves.length ? number(moves.length) : null, moves.length ? 'Période disponible' : '', 'receipt')}${metric('Écarts de caisse', moves.length ? money(sum(moves.filter(x => /écart/i.test(`${read(x, 'kind')} ${read(x, 'label')}`)), x => read(x, 'amountMad', 'amount'))) : null, moves.length ? 'Selon mouvements identifiés' : '', 'alert')}</section>
      ${boxes.length ? `<section class="grid ka-cash-grid">${cards}</section>${searchBar('Référence, caisse, sens ou libellé…')}${table(['Date', 'Référence', 'Sens', 'Libellé', 'Caisse', 'Montant', 'Statut', ''], moveRows, moves.length)}` : `<article class="panel">${empty('Aucune caisse configurée', 'Ajoutez les caisses physiques du Groupe avec leur société, site, devise et responsable.', 'new-cash-box', 'Configurer une caisse')}</article>`}`;
  }

  function renderFiscalRules(data = {}) {
    const rules = list(data, 'taxRules'), impacts = list(data, 'taxImpacts');
    const active = rules.filter(x => read(x, 'isActive') === true || !/inactif|false/i.test(String(read(x, 'status', 'isActive'))));
    const vat = sum(impacts, x => numeric(read(x, 'outputVatMad')) - numeric(read(x, 'inputVatMad')) || read(x, 'vatAmountMad'));
    const ras = sum(impacts, x => read(x, 'withholdingMad', 'withholdingAmountMad'));
    const rulesByCode = new Map(rules.map(rule => [String(read(rule, 'code')), rule]));
    const ruleRows = rules.map(item => { const id = read(item, 'id', 'code'), enabled = read(item, 'isActive') !== false; return `<tr data-ka-row data-status="${enabled ? 'Actif' : 'Inactif'}" data-date="${esc(read(item, 'effectiveFrom'))}"><td class="ref">${esc(read(item, 'code', 'id'))}</td><td><strong>${esc(read(item, 'name'))}</strong><small>${esc(read(item, 'kind'))}</small></td><td>${number(read(item, 'rate'))} %</td><td>${shortDate(read(item, 'effectiveFrom'))}</td><td>${shortDate(read(item, 'effectiveTo'))}</td><td>${number(read(item, 'priority'))}</td><td>${badge(enabled ? 'Actif' : 'Inactif')}</td><td><button type="button" class="toggle ${enabled ? 'on' : ''}" data-action="toggle-tax-rule" data-record="${esc(id)}" aria-label="Activer ou désactiver la règle"><i></i></button></td><td>${iconButton('open-tax-rule', `Ouvrir ${id}`, 'more', `data-record="${esc(id)}"`)}</td></tr>`; }).join('');
    const impactRows = impacts.slice(0, 12).map(item => { const ruleCode = read(item, 'ruleCode', 'taxRuleId'), rule = rulesByCode.get(String(ruleCode)), withholding = numeric(read(item, 'withholdingMad', 'withholdingAmountMad')), netVat = numeric(read(item, 'outputVatMad')) - numeric(read(item, 'inputVatMad')), amount = withholding || netVat || numeric(read(item, 'amountMad', 'taxAmountMad')), kind = withholding ? 'RAS' : (read(item, 'kind', 'taxKind') || 'TVA'); return `<tr data-ka-row data-date="${esc(read(item, 'taxDate', 'occurredAt', 'date'))}" data-amount="${Math.abs(amount)}"><td>${shortDate(read(item, 'taxDate', 'occurredAt', 'date'))}</td><td class="ref">${esc(read(item, 'operationReference', 'operationId'))}</td><td>${esc(kind)}</td><td>${esc(ruleCode)}</td><td>${read(rule, 'rate') !== '' ? `${number(read(rule, 'rate'))} %` : '—'}</td><td class="amount ${amount < 0 ? 'ka-positive-text' : ''}">${money(amount)}</td><td>${badge(read(item, 'status') || 'Calculé')}</td><td>${iconButton('open-tax-impact', 'Ouvrir l’impact fiscal', 'more', `data-record="${esc(ref(item))}"`)}</td></tr>`; }).join('');
    return `${pageHead('Règles fiscales & historique', 'Administrez la TVA, la RAS et leurs périodes d’effet sans taux figé dans le code.', `${button('export-tax-history', 'Exporter l’historique', 'secondary', 'download')}${button('new-tax-rule', 'Nouvelle règle fiscale', 'primary', 'plus')}`)}
      <section class="grid ka-metric-grid">${metric('Règles actives', rules.length ? number(active.length) : null, rules.length ? `${rules.length} règle${rules.length > 1 ? 's' : ''} au total` : '', 'tax')}${metric('TVA calculée', impacts.length ? money(vat) : null, impacts.length ? 'Impacts enregistrés' : '', 'calculator')}${metric('RAS calculée', impacts.length ? money(ras) : null, impacts.length ? 'Impacts enregistrés' : '', 'percent')}${metric('Historique fiscal', impacts.length ? number(impacts.length) : null, impacts.length ? 'Éléments traçables' : '', 'clock')}</section>
      <article class="panel ka-fiscal-banner">${svg('shield')}<div><strong>Règles versionnées</strong><p>Chaque calcul conserve le code, le taux et la période de la règle effectivement appliquée.</p></div>${button('verify-tax-rules', 'Contrôler les chevauchements', 'secondary', 'check')}</article>
      <div class="tabs ka-domain-tabs"><button type="button" class="tab active" data-action="fiscal-tab" data-target="rules">Règles fiscales</button><button type="button" class="tab" data-action="fiscal-tab" data-target="history">Historique TVA / RAS</button></div>
      <section data-panel="rules">${rules.length ? `${searchBar('Code, règle, nature fiscale…')}${table(['Code', 'Règle', 'Taux', 'Début', 'Fin', 'Priorité', 'Statut', 'Activation', ''], ruleRows, rules.length)}` : `<article class="panel">${empty('Aucune règle fiscale configurée', 'Créez des règles versionnées de TVA et de RAS avant de traiter les opérations fiscales.', 'new-tax-rule', 'Créer une règle fiscale')}</article>`}</section>
      <section data-panel="history" hidden>${impacts.length ? table(['Date', 'Opération', 'Nature', 'Règle', 'Taux', 'Montant', 'Statut', ''], impactRows, impacts.length) : `<article class="panel">${empty('Aucun impact fiscal', 'L’historique sera alimenté automatiquement par les opérations validées.')}</article>`}</section>`;
  }

  function documentRecords(data) { return list(data, 'documents'); }
  function renderDocuments(data = {}) {
    const docs = documentRecords(data), pending = docs.filter(x => /attente|ocr|à traiter|uploaded|nouveau/i.test(read(x, 'status', 'ocrStatus'))), errors = docs.filter(x => /erreur|rejet|faible/i.test(read(x, 'status', 'ocrStatus')));
    const rows = docs.map(item => { const id = ref(item), confidence = read(item, 'ocrConfidence', 'confidence'), statusValue = read(item, 'ocrStatus', 'status') || 'À traiter'; return `<tr data-ka-row data-status="${esc(statusValue)}" data-date="${esc(read(item, 'uploadedAt', 'createdAt'))}"><td><span class="ka-file-icon">${svg('file')}</span></td><td><strong>${esc(read(item, 'fileName', 'name', 'title') || id)}</strong><small>${esc(id)}</small></td><td>${esc(read(item, 'documentType', 'kind', 'mimeType'))}</td><td>${shortDate(read(item, 'uploadedAt', 'createdAt'))}</td><td>${confidence !== '' ? `<div class="ka-confidence"><i style="width:${Math.max(0, Math.min(100, numeric(confidence)))}%"></i></div><small>${number(confidence)} %</small>` : '—'}</td><td class="ref">${esc(read(item, 'operationReference', 'operationId') || 'Non lié')}</td><td>${badge(statusValue)}</td><td><div class="ka-row-actions">${!/valid|traité/i.test(statusValue) ? iconButton('run-document-ocr', `Lancer OCR ${id}`, 'scan', `data-record="${esc(id)}"`) : ''}${iconButton('open-document', `Ouvrir ${id}`, 'more', `data-record="${esc(id)}"`)}</div></td></tr>`; }).join('');
    return `${pageHead('Documents & file OCR', 'Importez, analysez, contrôlez et rattachez chaque justificatif à son opération métier.', `${button('export-document-log', 'Exporter le journal', 'secondary', 'download')}${button('upload-documents', 'Importer des documents', 'primary', 'upload')}`)}
      <section class="grid ka-metric-grid">${metric('Documents reçus', docs.length ? number(docs.length) : null, docs.length ? 'File documentaire' : '', 'file')}${metric('En attente de traitement', pending.length ? number(pending.length) : docs.length ? '0' : null, docs.length ? 'OCR ou validation requis' : '', 'clock')}${metric('Erreurs à contrôler', errors.length ? number(errors.length) : docs.length ? '0' : null, docs.length ? 'Intervention humaine' : '', 'alert', errors.length ? 'danger' : '')}${metric('Confiance OCR moyenne', docs.some(x => read(x, 'ocrConfidence', 'confidence') !== '') ? `${number(sum(docs.filter(x => read(x, 'ocrConfidence', 'confidence') !== ''), x => read(x, 'ocrConfidence', 'confidence')) / docs.filter(x => read(x, 'ocrConfidence', 'confidence') !== '').length)} %` : null, docs.length ? 'Documents analysés uniquement' : '', 'scan')}</section>
      <article class="panel ka-ocr-flow"><span>${svg('upload')}<b>Import</b></span><i>→</i><span>${svg('scan')}<b>OCR</b></span><i>→</i><span>${svg('check')}<b>Contrôle</b></span><i>→</i><span>${svg('link')}<b>Rattachement</b></span><i>→</i><span>${svg('shield')}<b>Audit</b></span></article>
      ${docs.length ? `${searchBar('Fichier, référence, opération liée…', `<select class="filter-control" data-filter="status"><option value="">Tous les statuts</option><option>À traiter</option><option>OCR en cours</option><option>Validé</option><option>Erreur</option></select>`)}${table(['', 'Document', 'Type', 'Importé le', 'Confiance OCR', 'Opération liée', 'Statut', ''], rows, docs.length)}` : `<article class="panel">${empty('Aucun document dans la file', 'Importez une facture, un relevé bancaire ou un justificatif. Aucun indicateur OCR n’est estimé sans document.', 'upload-documents', 'Importer des documents')}</article>`}`;
  }

  const REFERENCE_DOMAINS = [
    ['companies', 'Sociétés', 'building'], ['sites', 'Sites', 'map'], ['laboratories', 'Laboratoires', 'asset'], ['activities', 'Activités', 'chart'], ['costCenters', 'Centres de coût', 'calculator'], ['projects', 'Projets', 'file'], ['parties', 'Tiers', 'users'], ['bankAccounts', 'Banques', 'bank'], ['cashBoxes', 'Caisses', 'cash']
  ];
  const referenceItems = (data, key) => ['parties', 'bankAccounts', 'cashBoxes'].includes(key) ? rawList(data, key) : list(data, key);
  function referenceRows(data, key) {
    return referenceItems(data, key).map(item => `<tr data-ka-row data-status="${esc(read(item, 'status') || (read(item, 'isActive') === false ? 'Inactif' : 'Actif'))}"><td class="ref">${esc(read(item, 'internalCode', 'code', 'id'))}</td><td><strong>${esc(read(item, 'name', 'denomination', 'label'))}</strong><small>${esc(read(item, 'kind', 'type'))}</small></td><td>${esc(companyName(data, read(item, 'companyId')))}</td><td>${esc(read(item, 'ice', 'taxId', 'currency', 'city', 'accountingAccount'))}</td><td>${badge(read(item, 'status') || (read(item, 'isActive') === false ? 'Inactif' : 'Actif'))}</td><td>${iconButton('edit-master-record', `Modifier ${ref(item)}`, 'more', `data-record="${esc(ref(item))}" data-domain="${esc(key)}"`)}</td></tr>`).join('');
  }
  function renderMasterData(data = {}) {
    const companies = referenceItems(data, 'companies'), total = REFERENCE_DOMAINS.reduce((count, [key]) => count + referenceItems(data, key).length, 0), initialKey = REFERENCE_DOMAINS.find(([key]) => referenceItems(data, key).length)?.[0] || 'companies';
    const domains = REFERENCE_DOMAINS.map(([key, label, icon]) => { const count = referenceItems(data, key).length; return `<button type="button" class="ka-reference-tab ${key === initialKey ? 'active' : ''}" data-action="reference-domain" data-domain="${esc(key)}">${svg(icon)}<span><strong>${esc(label)}</strong><small>${number(count)} élément${count === 1 ? '' : 's'}</small></span><b>→</b></button>`; }).join('');
    const panels = REFERENCE_DOMAINS.map(([key, label]) => { const items = referenceItems(data, key); return `<section data-reference-panel="${esc(key)}" ${key === initialKey ? '' : 'hidden'}><div class="ka-reference-head"><div><h2>${esc(label)}</h2><p>Référentiel partagé par le Transaction Engine</p></div>${button('new-master-record', `Ajouter · ${label}`, 'primary', 'plus', `data-domain="${esc(key)}"`)}</div>${items.length ? table(['Code interne', 'Libellé', 'Société', 'Identification', 'Statut', ''], referenceRows(data, key), items.length) : `<article class="panel">${empty(`Aucun élément · ${label}`, 'Ce référentiel ne contient encore aucun enregistrement.', 'new-master-record', `Ajouter · ${label}`)}</article>`}</section>`; }).join('');
    return `${pageHead('Référentiel KAY Groupe', 'Sociétés, sites, laboratoires, axes analytiques, tiers, banques et caisses dans une architecture Groupe.', `${button('export-master-data', 'Exporter', 'secondary', 'download')}${button('import-master-data', 'Importer un référentiel', 'secondary', 'upload')}`)}
      <section class="grid ka-metric-grid">${metric('Sociétés', companies.length ? number(companies.length) : null, companies.length ? 'Entités juridiques' : '', 'building')}${metric('Éléments référentiels', total ? number(total) : null, total ? 'Collections actives' : '', 'file')}${metric('Tiers', referenceItems(data, 'parties').length ? number(referenceItems(data, 'parties').length) : null, referenceItems(data, 'parties').length ? 'Clients et fournisseurs' : '', 'users')}${metric('Axes analytiques', ['sites', 'laboratories', 'activities', 'costCenters', 'projects'].some(k => referenceItems(data, k).length) ? number(['sites', 'laboratories', 'activities', 'costCenters', 'projects'].reduce((s, k) => s + referenceItems(data, k).length, 0)) : null, 'Site · activité · coût · projet', 'chart')}</section>
      <section class="grid ka-reference-layout"><aside class="panel ka-reference-nav">${domains}</aside><div class="ka-reference-content">${panels}</div></section>`;
  }

  const BUCKETS = [
    ['nonDue', 'Non échue'], ['days1To30', '1–30 jours'], ['days31To60', '31–60 jours'], ['days61To90', '61–90 jours'], ['days91To120', '91–120 jours'], ['over120', '> 120 jours']
  ];
  function agingValue(item, key) {
    const aliases = { nonDue: ['nonDue', 'notDue', 'current'], days1To30: ['days1To30', 'bucket30', 'from1To30'], days31To60: ['days31To60', 'bucket60', 'from31To60'], days61To90: ['days61To90', 'bucket90', 'from61To90'], days91To120: ['days91To120', 'bucket120', 'from91To120'], over120: ['over120', 'bucketOver120', 'moreThan120'] };
    return numeric(read(item, ...aliases[key]));
  }
  function deriveAging(data, side) {
    const explicit = list(data, side === 'customer' ? 'customerAging' : 'supplierAging'); if (explicit.length) return explicit;
    const due = rawList(data, 'dueItems'), activeIds = financiallyActiveOperationIds(data), partyMap = new Map(rawList(data, 'parties').map(x => [String(read(x, 'id')), x])), today = new Date();
    const relevant = due.filter(item => { const party = partyMap.get(String(read(item, 'partyId'))), kind = String(read(item, 'kind') || read(party, 'kind')).toLowerCase(); return activeIds.has(String(read(item, 'operationId'))) && isOpenDue(item) && (side === 'customer' ? /client|customer|receivable|créance/.test(kind) : /supplier|fourn|payable|debt|dette/.test(kind)); });
    const groups = new Map();
    relevant.forEach(item => { const id = String(read(item, 'partyId') || 'unknown'), party = partyMap.get(id), row = groups.get(id) || { partyId: id, partyName: read(party, 'name') || id, nonDue: 0, days1To30: 0, days31To60: 0, days61To90: 0, days91To120: 0, over120: 0 }; const outstanding = numeric(read(item, 'outstandingMad', 'outstanding')), dueDate = new Date(read(item, 'dueDate')), days = Number.isNaN(dueDate.valueOf()) ? 0 : Math.floor((today - dueDate) / 86400000); const key = days <= 0 ? 'nonDue' : days <= 30 ? 'days1To30' : days <= 60 ? 'days31To60' : days <= 90 ? 'days61To90' : days <= 120 ? 'days91To120' : 'over120'; row[key] += outstanding; groups.set(id, row); });
    return groups.size ? [...groups.values()] : [];
  }
  function agingSection(data, side, hidden) {
    const rows = deriveAging(data, side), label = side === 'customer' ? 'clients' : 'fournisseurs', totals = Object.fromEntries(BUCKETS.map(([key]) => [key, sum(rows, item => agingValue(item, key))])), total = Object.values(totals).reduce((a, b) => a + b, 0), overdue = total - totals.nonDue;
    const debtors = rows.map(item => ({ item, total: BUCKETS.reduce((s, [key]) => s + agingValue(item, key), 0), overdue: BUCKETS.slice(1).reduce((s, [key]) => s + agingValue(item, key), 0) })).sort((a, b) => b.overdue - a.overdue);
    const tableRows = debtors.map(({ item, total: rowTotal }) => `<tr data-ka-row data-amount="${rowTotal}"><td><strong>${esc(read(item, 'partyName', 'name') || partyName(data, read(item, 'partyId')))}</strong><small>${esc(read(item, 'partyCode', 'internalCode', 'partyId'))}</small></td>${BUCKETS.map(([key]) => `<td class="amount ${key === 'over120' && agingValue(item, key) ? 'negative' : ''}">${money(agingValue(item, key))}</td>`).join('')}<td class="amount"><strong>${money(rowTotal)}</strong></td><td>${iconButton('open-aged-party', 'Ouvrir le détail', 'more', `data-record="${esc(read(item, 'partyId', 'partyCode'))}" data-side="${side}"`)}</td></tr>`).join('');
    return `<section data-aging-panel="${side}" ${hidden ? 'hidden' : ''}><section class="grid ka-metric-grid">${metric(`Encours ${label}`, rows.length ? money(total) : null, rows.length ? `${rows.length} tiers` : '', 'wallet')}${metric('Non échu', rows.length ? money(totals.nonDue) : null, rows.length && total ? `${number(totals.nonDue / total * 100)} % du total` : '', 'check')}${metric('Total en retard', rows.length ? money(overdue) : null, rows.length && total ? `${number(overdue / total * 100)} % du total` : '', 'clock', overdue ? 'warning' : '')}${metric('Risque > 120 jours', rows.length ? money(totals.over120) : null, rows.length ? 'Priorité de traitement' : '', 'alert', totals.over120 ? 'danger' : '')}</section>${rows.length ? `<section class="grid ka-aging-summary"><article class="panel ka-aging-bars"><div class="panel-head"><div><h2>Répartition en six tranches</h2><p>Montants ouverts jusqu’au règlement complet</p></div><strong>${money(total)}</strong></div>${BUCKETS.map(([key, title], index) => `<div class="ka-aging-bar"><span>${esc(title)}</span><div><i class="${index === 0 ? 'current' : index === 5 ? 'critical' : ''}" style="width:${total ? Math.max(1, totals[key] / total * 100) : 0}%"></i></div><strong>${money(totals[key])}</strong></div>`).join('')}</article><article class="panel ka-top-debtors"><div class="panel-head"><div><h2>Top débiteurs</h2><p>Classés par montant en retard</p></div></div>${debtors.slice(0, 5).map((x, i) => `<div><i>${i + 1}</i><span><strong>${esc(read(x.item, 'partyName', 'name') || partyName(data, read(x.item, 'partyId')))}</strong><small>${x.overdue ? 'Encours en retard' : 'Non échu'}</small></span><b class="${x.overdue ? 'negative' : ''}">${money(x.overdue || x.total)}</b></div>`).join('')}</article></section>${searchBar(`Rechercher un ${side === 'customer' ? 'client' : 'fournisseur'}…`)}${table(['Tiers', ...BUCKETS.map(x => x[1]), 'Total', ''], tableRows, rows.length)}` : `<article class="panel">${empty(`Aucune balance âgée ${label}`, `Aucune échéance ${side === 'customer' ? 'client' : 'fournisseur'} ouverte n’est disponible dans le moteur transactionnel.`)}</article>`}</section>`;
  }
  function renderAgedBalances(data = {}) {
    return `${pageHead('Balances âgées clients & fournisseurs', 'Six tranches d’ancienneté, principaux débiteurs et lien direct vers chaque échéance source.', `${button('export-aged-balances', 'Exporter', 'secondary', 'download')}${button('run-aged-actions', 'Actions de recouvrement', 'primary', 'arrow')}`)}<div class="tabs ka-domain-tabs ka-aging-tabs"><button type="button" class="tab active" data-action="aging-side" data-target="customer">Clients</button><button type="button" class="tab" data-action="aging-side" data-target="supplier">Fournisseurs</button></div>${agingSection(data, 'customer', false)}${agingSection(data, 'supplier', true)}`;
  }

  const pages = {
    payments: renderPayments, paymentOrders: renderPayments, paymentPreparation: renderPayments, internationalTransfers: renderPayments, 'payment-preparation': renderPayments,
    expenses: renderExpenses, expenseNotes: renderExpenses, expenseWorkflow: renderExpenses, 'expense-notes': renderExpenses,
    commissions: renderCommissions, commissionEngine: renderCommissions, 'commission-engine': renderCommissions,
    assets: renderAssets, fixedAssets: renderAssets, 'fixed-assets': renderAssets,
    cashBoxes: renderCashBoxes, cashboxes: renderCashBoxes, 'cash-boxes': renderCashBoxes,
    fiscalRules: renderFiscalRules, taxRules: renderFiscalRules, taxAdministration: renderFiscalRules, 'fiscal-rules': renderFiscalRules,
    documents: renderDocuments, ocr: renderDocuments, documentIntake: renderDocuments, 'document-ocr': renderDocuments,
    masterData: renderMasterData, groupReference: renderMasterData, groupMasterData: renderMasterData, 'master-data': renderMasterData,
    agedBalances: renderAgedBalances, agedDual: renderAgedBalances, aged: renderAgedBalances, 'aged-balances': renderAgedBalances
  };

  /*
   * Deterministic interaction contract.  Integration/tests can inspect this
   * registry to prove that every rendered action is either completed in the
   * browser or forwarded using an explicit Transaction Engine command.
   */
  const supportedActions = Object.freeze({
    'toggle-advanced-filters': { mode: 'local', effect: 'toggle-filters' },
    'advanced-page': { mode: 'local', effect: 'paginate' },
    'close-advanced-modal': { mode: 'local', effect: 'close-modal' },
    'fiscal-tab': { mode: 'local', effect: 'switch-panel' },
    'reference-domain': { mode: 'local', effect: 'switch-panel' },
    'aging-side': { mode: 'local', effect: 'switch-panel' },
    'export-payments': { mode: 'local', effect: 'export-csv' },
    'export-expenses': { mode: 'local', effect: 'export-csv' },
    'export-commissions': { mode: 'local', effect: 'export-csv' },
    'export-assets': { mode: 'local', effect: 'export-csv' },
    'export-cash-boxes': { mode: 'local', effect: 'export-csv' },
    'export-tax-history': { mode: 'local', effect: 'export-csv' },
    'export-document-log': { mode: 'local', effect: 'export-csv' },
    'export-master-data': { mode: 'local', effect: 'export-csv' },
    'export-aged-balances': { mode: 'local', effect: 'export-csv' },
    'open-payment-source': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-expense': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-commission': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-asset': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-cash-box': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-cash-movement': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-tax-rule': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-tax-impact': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-document': { mode: 'local+dispatch', effect: 'detail-modal' },
    'edit-master-record': { mode: 'local+dispatch', effect: 'detail-modal' },
    'open-aged-party': { mode: 'local+dispatch', effect: 'detail-modal' },
    'create-payment-order': { mode: 'dispatch', command: 'prepare-payment' },
    'prepare-selected-payments': { mode: 'dispatch', command: 'prepare-payment' },
    'submit-international-transfer': { mode: 'dispatch', command: 'prepare-payment' },
    'new-expense': { mode: 'local', effect: 'open-form' },
    'save-expense': { mode: 'dispatch', command: 'save-enterprise-operation' },
    'advance-expense': { mode: 'dispatch', command: 'advance-expense' },
    'new-commission-rule': { mode: 'local', effect: 'open-form' },
    'save-commission-rule': { mode: 'dispatch', command: 'save-commission-rule' },
    'calculate-commissions': { mode: 'dispatch', command: 'calculate-commissions' },
    'new-asset': { mode: 'local', effect: 'open-form' },
    'save-asset': { mode: 'dispatch', command: 'save-enterprise-operation' },
    'run-depreciation': { mode: 'dispatch', command: 'run-depreciation' },
    'new-cash-box': { mode: 'local', effect: 'open-form' },
    'save-cash-box': { mode: 'dispatch', command: 'save-cash-box' },
    'new-cash-movement': { mode: 'local', effect: 'open-form' },
    'save-cash-movement': { mode: 'dispatch', command: 'save-cash-movement' },
    'new-tax-rule': { mode: 'local', effect: 'open-form' },
    'save-tax-rule': { mode: 'dispatch', command: 'save-tax-rule' },
    'toggle-tax-rule': { mode: 'dispatch', command: 'toggle-tax-rule' },
    'verify-tax-rules': { mode: 'dispatch', command: 'verify-tax-rules' },
    'upload-documents': { mode: 'local', effect: 'open-form' },
    'submit-document-upload': { mode: 'dispatch', command: 'submit-document-upload' },
    'run-document-ocr': { mode: 'dispatch', command: 'run-document-ocr' },
    'new-master-record': { mode: 'local', effect: 'open-form' },
    'save-master-record': { mode: 'dispatch', command: 'save-master-record' },
    'import-master-data': { mode: 'local', effect: 'open-form' },
    'submit-master-import': { mode: 'dispatch', command: 'submit-master-import' },
    'run-aged-actions': { mode: 'dispatch', command: 'run-aged-actions' }
  });

  function notify(text, type = 'success') {
    if (typeof window.showToast === 'function') return window.showToast(text, type);
    let host = document.querySelector('#toast-host'); if (!host) { host = document.createElement('div'); host.id = 'toast-host'; document.body.appendChild(host); }
    const item = document.createElement('div'); item.className = `toast ${type}`; item.textContent = text; host.appendChild(item); setTimeout(() => item.remove(), 3500);
  }
  function emit(send, type, payload = {}) {
    const message = { type, payload };
    try { if (typeof send === 'function') send.length >= 2 ? send(type, payload) : send(message); else window.chrome?.webview?.postMessage(message); } catch (error) { console.error('KayAdvanced emit failed', error); }
  }
  function modal(root, title, body, confirmAction, confirmLabel = 'Enregistrer') {
    root.querySelector('.ka-modal-layer')?.remove(); const layer = document.createElement('div'); layer.className = 'ka-modal-layer';
    layer.innerHTML = `<section class="ka-modal" role="dialog" aria-modal="true"><header><h3>${esc(title)}</h3>${iconButton('close-advanced-modal', 'Fermer', 'close')}</header><div class="ka-modal-body">${body}</div><footer>${button('close-advanced-modal', 'Annuler')}${confirmAction ? `<button type="submit" form="ka-modal-form" class="primary-button ka-button" data-action="${esc(confirmAction)}">${svg('check')}<span>${esc(confirmLabel)}</span></button>` : ''}</footer></section>`;
    root.appendChild(layer); layer.querySelector('input,select,textarea')?.focus();
  }
  function formMarkup(kind) {
    const definitions = {
      expense: [['Date', 'date', 'date'], ['Collaborateur', 'text', 'employeeName'], ['Motif', 'text', 'nature'], ['Montant TTC', 'number', 'amount']], commissionRule: [['Libellé', 'text', 'name'], ['Assiette', 'text', 'basis'], ['Taux (%)', 'number', 'rate'], ['Plafond', 'number', 'capMad']], asset: [['Désignation', 'text', 'nature'], ['Valeur d’acquisition', 'number', 'amount'], ['Date de mise en service', 'date', 'serviceDate'], ['Durée (années)', 'number', 'depreciationYears']], cashBox: [['Nom de la caisse', 'text', 'name'], ['Société', 'text', 'company'], ['Site', 'text', 'site'], ['Devise', 'text', 'currency']], taxRule: [['Code', 'text', 'code'], ['Libellé', 'text', 'name'], ['Nature (TVA/RAS)', 'text', 'kind'], ['Taux (%)', 'number', 'rate']], master: [['Code interne', 'text', 'code'], ['Libellé', 'text', 'name'], ['Société', 'text', 'company'], ['Statut', 'text', 'status']]
    }[kind] || [['Libellé', 'text', 'name'], ['Référence', 'text', 'reference']];
    return `<form id="ka-modal-form" class="ka-modal-form" data-kind="${esc(kind)}">${definitions.map(([label, type, name]) => `<label><span>${esc(label)}</span><input name="${esc(name)}" data-label="${esc(label)}" type="${type}" ${type === 'number' ? 'min="0" step="0.01"' : ''} required></label>`).join('')}</form>`;
  }
  function entityOptions(items, valueKeys, labelKeys) {
    return items.map(item => `<option value="${esc(read(item, ...valueKeys))}">${esc(read(item, ...labelKeys))}</option>`).join('');
  }
  function operationForm(data, kind) {
    const companies = list(data, 'companies'), suppliers = rawList(data, 'parties').filter(item => /supplier|fournisseur/i.test(read(item, 'kind')));
    if (kind === 'expense') return `<form id="ka-modal-form" class="ka-modal-form" data-kind="expense"><label><span>Société</span><select name="companyId" required><option value="">Sélectionner</option>${entityOptions(companies, ['id', 'code', 'name'], ['name', 'denomination'])}</select></label><label><span>Date</span><input name="date" type="date" required></label><label><span>Collaborateur</span><input name="employeeName" required></label><label><span>Montant TTC</span><input name="amount" type="number" min="0.01" step="0.01" required></label><label class="full"><span>Motif</span><input name="nature" required></label><input name="currency" type="hidden" value="MAD"></form>`;
    return `<form id="ka-modal-form" class="ka-modal-form" data-kind="asset"><label><span>Société</span><select name="companyId" required><option value="">Sélectionner</option>${entityOptions(companies, ['id', 'code', 'name'], ['name', 'denomination'])}</select></label><label><span>Fournisseur</span><select name="partyId" required><option value="">Sélectionner</option>${entityOptions(suppliers, ['id', 'internalCode'], ['name'])}</select></label><label class="full"><span>Désignation</span><input name="nature" required></label><label><span>Valeur d’acquisition</span><input name="amount" type="number" min="0.01" step="0.01" required></label><label><span>Date d’acquisition</span><input name="date" type="date" required></label><label><span>Date de mise en service</span><input name="serviceDate" type="date" required></label><label><span>Durée d’amortissement</span><input name="depreciationYears" type="number" min="1" max="50" required></label><input name="currency" type="hidden" value="MAD"><input name="assetTreatment" type="hidden" value="Immobilisation"></form>`;
  }
  function cashBoxForm(data) {
    const companies = list(data, 'companies'), sites = list(data, 'sites');
    return `<form id="ka-modal-form" class="ka-modal-form" data-kind="cashBox"><label class="full"><span>Nom de la caisse</span><input name="name" required></label><label><span>Société</span><select name="companyId" required><option value="">Sélectionner</option>${entityOptions(companies, ['id', 'code', 'name'], ['name', 'denomination'])}</select></label><label><span>Site</span><select name="siteId"><option value="">Aucun site</option>${entityOptions(sites, ['id', 'code', 'name'], ['name', 'label'])}</select></label><label><span>Devise</span><select name="currency" required><option>MAD</option><option>EUR</option><option>USD</option></select></label><label><span>Responsable</span><input name="responsibleName"></label><input name="isActive" type="hidden" value="true"></form>`;
  }
  function taxRuleForm() {
    return `<form id="ka-modal-form" class="ka-modal-form" data-kind="taxRule"><label><span>Code</span><input name="code" required></label><label><span>Nature</span><select name="kind" required><option value="VAT">TVA</option><option value="WITHHOLDING">RAS</option></select></label><label class="full"><span>Libellé</span><input name="name" required></label><label><span>Taux (%)</span><input name="rate" type="number" min="0" max="100" step="0.01" required></label><label><span>Priorité</span><input name="priority" type="number" min="0" step="1" value="100" required></label><label><span>Début d’effet</span><input name="effectiveFrom" type="date" required></label><label><span>Fin d’effet</span><input name="effectiveTo" type="date"></label><input name="isActive" type="hidden" value="true"></form>`;
  }
  function masterRecordForm(data, domain) {
    const companies = list(data, 'companies');
    return `<form id="ka-modal-form" class="ka-modal-form" data-kind="masterRecord"><input type="hidden" name="domain" value="${esc(domain)}"><label><span>Code interne</span><input name="code" required></label><label><span>Libellé</span><input name="name" required></label><label><span>Société</span><select name="companyId"><option value="">Groupe / aucune</option>${entityOptions(companies, ['id', 'code', 'name'], ['name', 'denomination'])}</select></label><label><span>Statut</span><select name="status"><option value="Active">Actif</option><option value="Inactive">Inactif</option></select></label></form>`;
  }
  function exportTable(root, page) {
    const current = [...root.querySelectorAll('section[data-panel],section[data-aging-panel],section[data-reference-panel]')].find(x => !x.hidden) || root;
    const tableNode = current.querySelector('.ka-table') || root.querySelector('.ka-table'); if (!tableNode) { notify('Aucun tableau disponible.', 'error'); return; }
    const lines = [...tableNode.querySelectorAll('tr')].filter(row => !row.hidden).map(row => [...row.querySelectorAll('th,td')].map(cell => `"${cell.textContent.trim().replaceAll('"', '""')}"`).join(';'));
    const blob = new Blob(['\ufeff' + lines.join('\n')], { type: 'text/csv;charset=utf-8' }), url = URL.createObjectURL(blob), a = document.createElement('a'); a.href = url; a.download = `kay-one-${page}-${new Date().toISOString().slice(0, 10)}.csv`; a.click(); setTimeout(() => URL.revokeObjectURL(url), 500); notify('Export CSV généré.');
  }
  function filterRows(root) {
    const query = (root.querySelector('[data-role="ka-search"]')?.value || '').toLocaleLowerCase('fr'), filters = [...root.querySelectorAll('select[data-filter]')].map(x => x.value.toLocaleLowerCase('fr')).filter(Boolean), minimum = numeric(root.querySelector('[data-filter="minimum"]')?.value), start = root.querySelector('[data-filter="start"]')?.value || '', end = root.querySelector('[data-filter="end"]')?.value || '';
    root.querySelectorAll('[data-ka-row]').forEach(row => { const text = row.textContent.toLocaleLowerCase('fr'), date = row.dataset.date || '', amount = numeric(row.dataset.amount); const show = (!query || text.includes(query)) && filters.every(x => text.includes(x)) && (!minimum || amount >= minimum) && (!start || !date || date >= start) && (!end || !date || date <= end); row.dataset.filterHidden = String(!show); }); paginate(root, 1);
  }
  function paginate(root, request = 1) {
    const activeContainer = [...root.querySelectorAll('section[data-panel],section[data-aging-panel],section[data-reference-panel]')].find(x => !x.hidden) || root;
    const panel = activeContainer.querySelector('.ka-table-panel') || root.querySelector('.ka-table-panel'); if (!panel) return;
    const rows = [...panel.querySelectorAll('[data-ka-row]')], eligible = rows.filter(row => row.dataset.filterHidden !== 'true'), pageCount = Math.max(1, Math.ceil(eligible.length / 10)), current = numeric(panel.dataset.page) || 1;
    let page = request === 'next' ? current + 1 : request === 'previous' ? current - 1 : numeric(request) || 1; page = Math.max(1, Math.min(pageCount, page)); panel.dataset.page = String(page);
    rows.forEach(row => { row.hidden = true; }); eligible.forEach((row, index) => { row.hidden = index < (page - 1) * 10 || index >= page * 10; });
    panel.querySelectorAll('.page-button').forEach(item => item.classList.toggle('active', numeric(item.dataset.pageNumber) === page));
    const countNode = panel.querySelector('[data-role="ka-count"]'), first = eligible.length ? (page - 1) * 10 + 1 : 0, last = Math.min(page * 10, eligible.length); if (countNode) countNode.textContent = `Affichage ${first}–${last} sur ${eligible.length}`;
  }
  function detailBody(record) { return `<div class="ka-detail"><span>${svg('link')}</span><div><strong>Chaîne de traçabilité</strong><p>Document → opération → fiscalité → comptabilité → échéance → paiement → banque → audit.</p><dl><dt>Référence</dt><dd>${esc(record || '—')}</dd><dt>Source</dt><dd>Transaction Engine</dd></dl></div></div>`; }

  function bind(page, data = {}, send) {
    const root = document.querySelector('#app-content'); if (!root) return () => {};
    root.__kayAdvancedController?.abort(); const controller = new AbortController(); root.__kayAdvancedController = controller; const signal = controller.signal;
    root.addEventListener('click', event => {
      const target = event.target.closest('button[data-action]'); if (!target || !root.contains(target)) return; event.stopPropagation(); const action = target.dataset.action, record = target.dataset.record || '', domain = target.dataset.domain || target.closest('[data-reference-panel]')?.dataset.referencePanel || '';
      if (target.type === 'submit') return;
      const contract = supportedActions[action];
      if (!contract) { notify(`Action non enregistrée : ${action}`, 'error'); return; }
      if (contract.mode === 'local+dispatch' && contract.effect !== 'detail-modal') emit(send, action, { page, record, domain });
      if (action === 'close-advanced-modal') { root.querySelector('.ka-modal-layer')?.remove(); return; }
      if (action === 'toggle-advanced-filters') { target.closest('.ka-toolbar')?.querySelector('.ka-advanced-filters')?.classList.toggle('open'); return; }
      if (/^export-/.test(action)) { exportTable(root, page); return; }
      if (action === 'prepare-selected-payments' || action === 'create-payment-order') { const selected = [...root.querySelectorAll('.ka-select-row:checked')]; if (!selected.length) { notify('Sélectionnez au moins une échéance.', 'error'); return; } const accounts = rawList(data, 'bankAccounts'), today = new Date().toISOString().slice(0, 10); let sent = 0; selected.forEach(item => { const account = accounts.find(x => String(read(x, 'companyId')) === String(item.dataset.companyId) && String(read(x, 'currency')).toUpperCase() === String(item.dataset.currency).toUpperCase()) || accounts.find(x => String(read(x, 'companyId')) === String(item.dataset.companyId)); if (!item.dataset.operationId || !account) return; emit(send, 'prepare-payment', { operationId: item.dataset.operationId, paymentDate: today, amount: numeric(item.dataset.paymentAmount), currency: item.dataset.currency || 'MAD', bankAccountId: read(account, 'id'), allowCurrencyConversion: String(read(account, 'currency')).toUpperCase() !== String(item.dataset.currency).toUpperCase(), method: 'Virement', externalReference: item.dataset.id }); sent++; }); if (!sent) { notify('Aucun compte bancaire compatible avec la sélection.', 'error'); return; } notify(`${sent} ordre${sent > 1 ? 's' : ''} de paiement envoyé${sent > 1 ? 's' : ''} en préparation.`); return; }
      if (action === 'new-expense') { modal(root, 'Nouvelle note de frais', operationForm(data, 'expense'), 'save-expense', 'Soumettre'); return; }
      if (action === 'new-commission-rule') { modal(root, 'Nouvelle règle de commission', formMarkup('commissionRule'), 'save-commission-rule'); return; }
      if (action === 'new-asset') { modal(root, 'Nouvelle immobilisation', operationForm(data, 'asset'), 'save-asset'); return; }
      if (action === 'new-cash-box') { modal(root, 'Nouvelle caisse', cashBoxForm(data), 'save-cash-box'); return; }
      if (action === 'new-tax-rule') { modal(root, 'Nouvelle règle fiscale', taxRuleForm(), 'save-tax-rule'); return; }
      if (action === 'new-master-record') { modal(root, 'Nouvel élément de référentiel', masterRecordForm(data, domain), 'save-master-record'); return; }
      if (action === 'new-cash-movement') { modal(root, 'Nouveau mouvement de caisse', `<form id="ka-modal-form" class="ka-modal-form"><input type="hidden" name="cashBoxId" value="${esc(record)}"><label><span>Nature</span><select name="kind"><option>Entrée</option><option>Sortie</option></select></label><label><span>Montant</span><input name="amount" type="number" min="0.01" step="0.01" required></label><label class="full"><span>Justificatif / libellé</span><input name="label" required></label></form>`, 'save-cash-movement'); return; }
      if (action === 'upload-documents' || action === 'import-master-data') { modal(root, action === 'upload-documents' ? 'Importer des documents' : 'Importer un référentiel', `<form id="ka-modal-form" class="ka-modal-form"><label class="ka-file-drop"><input name="files" type="file" ${action === 'upload-documents' ? 'multiple accept=".pdf,.png,.jpg,.jpeg,.tif,.tiff"' : 'accept=".csv,.xlsx"'} required><span>${svg('upload')}<strong>Choisir ou déposer les fichiers</strong><small>${action === 'upload-documents' ? 'PDF et images · OCR après import' : 'CSV ou Excel · contrôle avant import'}</small></span></label></form>`, action === 'upload-documents' ? 'submit-document-upload' : 'submit-master-import', 'Importer'); return; }
      if (action === 'advance-expense') { const row = target.closest('tr'), badgeNode = row?.querySelector('.status'), stages = ['Brouillon', 'Soumise', 'Validée', 'À payer', 'Payée'], current = stages.findIndex(x => String(badgeNode?.textContent).toLowerCase().includes(x.toLowerCase().replace('soumise', 'soumis').replace('validée', 'valid'))), next = stages[Math.min(stages.length - 1, current + 1)] || 'Soumise'; if (badgeNode) { badgeNode.textContent = next; badgeNode.className = `status ${tone(next)}`; } emit(send, action, { page, operationId: record, operationReference: target.dataset.reference || '', status: next }); notify(`Note de frais passée au statut « ${next} ».`); return; }
      if (action === 'fiscal-tab') { root.querySelectorAll('[data-action="fiscal-tab"]').forEach(x => x.classList.toggle('active', x === target)); root.querySelectorAll('[data-panel]').forEach(x => { x.hidden = x.dataset.panel !== target.dataset.target; }); return; }
      if (action === 'toggle-tax-rule') { target.classList.toggle('on'); const enabled = target.classList.contains('on'), rowBadge = target.closest('tr')?.querySelector('.status'); if (rowBadge) { rowBadge.textContent = enabled ? 'Actif' : 'Inactif'; rowBadge.className = `status ${enabled ? 'paid' : 'draft'}`; } emit(send, action, { page, taxRuleId: record, isActive: enabled }); return; }
      if (action === 'reference-domain') { root.querySelectorAll('.ka-reference-tab').forEach(x => x.classList.toggle('active', x === target)); root.querySelectorAll('[data-reference-panel]').forEach(x => { x.hidden = x.dataset.referencePanel !== target.dataset.domain; }); return; }
      if (action === 'aging-side') { root.querySelectorAll('[data-action="aging-side"]').forEach(x => x.classList.toggle('active', x === target)); root.querySelectorAll('[data-aging-panel]').forEach(x => { x.hidden = x.dataset.agingPanel !== target.dataset.target; }); return; }
      if (action === 'run-document-ocr') { const row = target.closest('tr'), state = row?.querySelector('.status'); if (state) { state.textContent = 'OCR en cours'; state.className = 'status pending'; } emit(send, action, { page, documentId: record }); notify('Document placé dans la file OCR.'); return; }
      if (/^open-|^edit-/.test(action)) { modal(root, `Détail ${record || 'KAY ONE'}`, detailBody(record), '', ''); return; }
      if (action === 'calculate-commissions') { emit(send, action, { page, calculationDate: new Date().toISOString().slice(0, 10) }); notify('Calcul des commissions demandé au moteur.'); return; }
      if (action === 'run-depreciation') { emit(send, action, { page, postingDate: new Date().toISOString().slice(0, 10) }); notify('Calcul des dotations demandé au moteur.'); return; }
      if (action === 'verify-tax-rules') { emit(send, action, { page, effectiveDate: new Date().toISOString().slice(0, 10) }); notify('Contrôle des règles fiscales demandé au moteur.'); return; }
      if (action === 'run-aged-actions') { const side = root.querySelector('[data-action="aging-side"].active')?.dataset.target || 'customer', partyIds = [...root.querySelectorAll(`[data-aging-panel="${side}"] [data-action="open-aged-party"]`)].map(item => item.dataset.record).filter(Boolean); emit(send, action, { page, side, partyIds }); notify('Actions de relance demandées au moteur.'); return; }
      if (action === 'advanced-page') { paginate(root, target.dataset.pageNumber); return; }
      notify(`${target.textContent.trim() || target.title} : demande envoyée.`);
    }, { signal });
    root.addEventListener('change', event => {
      if (event.target.matches('.ka-select-row')) { const selected = [...root.querySelectorAll('.ka-select-row:checked')], total = sum(selected, x => x.dataset.amount); const countNode = root.querySelector('[data-role="selected-count"]'), totalNode = root.querySelector('[data-role="selected-total"]'); if (countNode) countNode.textContent = `${selected.length} échéance${selected.length === 1 ? '' : 's'} sélectionnée${selected.length === 1 ? '' : 's'}`; if (totalNode) totalNode.textContent = money(total); emit(send, 'payment-selection-change', { page, ids: selected.map(x => x.dataset.id), totalMad: total }); }
      if (event.target.matches('[data-filter]')) filterRows(root);
    }, { signal });
    root.addEventListener('input', event => {
      if (event.target.matches('[data-role="ka-search"],[data-filter]')) filterRows(root);
      if (event.target.closest('[data-form="international-transfer"]')) { const form = event.target.closest('form'), amount = numeric(form.elements.amount?.value), rate = numeric(form.elements.exchangeRate?.value), output = form.querySelector('[data-role="transfer-countervalue"]'); if (output) output.textContent = amount && rate ? money(amount * rate) : '—'; }
    }, { signal });
    root.addEventListener('submit', event => {
      const form = event.target; event.preventDefault(); if (!form.reportValidity()) return; const submitter = event.submitter, action = submitter?.dataset.action || 'submit-advanced-form';
      const contract = supportedActions[action]; if (!contract || contract.mode !== 'dispatch' || !contract.command) { notify(`Commande de formulaire non enregistrée : ${action}`, 'error'); return; }
      let payload;
      if (form.querySelector('input[type="file"]')) { const files = [...form.querySelector('input[type="file"]').files].map(file => ({ name: file.name, size: file.size, type: file.type })); payload = { files, kind: form.dataset.kind || '', record: submitter?.dataset.record || '' }; }
      else { payload = { ...Object.fromEntries(new FormData(form).entries()), record: submitter?.dataset.record || '' }; if (form.dataset.kind) payload.kind = form.dataset.kind; }
      const backendAction = contract.command;
      if (action === 'save-expense') payload = { ...payload, operationType: 'Note de frais', description: payload.nature, operationDate: payload.date };
      if (action === 'save-asset') payload = { ...payload, operationType: 'Immobilisation', description: payload.nature, operationDate: payload.date };
      if (action === 'save-cash-box') payload = { ...payload, isActive: payload.isActive === 'true' };
      if (action === 'save-tax-rule') payload = { ...payload, isActive: payload.isActive === 'true' };
      if (action === 'submit-international-transfer') payload = { ...payload, allowCurrencyConversion: true, method: 'Virement international', paymentDate: new Date().toISOString().slice(0, 10) };
      emit(send, backendAction, { page, ...payload }); root.querySelector('.ka-modal-layer')?.remove(); notify('Demande enregistrée et envoyée au workflow.');
    }, { signal });
    return () => controller.abort();
  }

  function render(page, data) { return pages[page] ? pages[page](data || {}) : `<article class="panel">${empty('Domaine indisponible', `La page « ${page} » n’est pas enregistrée dans KayAdvanced.`)}</article>`; }

  function runUiChecks(data = {}) {
    const renderers = [...new Set(Object.values(pages))], missingDataAction = [], unregisteredActions = [], actions = new Set();
    renderers.forEach(renderer => {
      const html = renderer(data || {}), name = renderer.name || 'anonymous';
      for (const match of html.matchAll(/<button\b[^>]*>/gi)) {
        const actionMatch = match[0].match(/\bdata-action="([^"]+)"/i);
        if (!actionMatch) missingDataAction.push({ renderer: name, button: match[0] });
        else { actions.add(actionMatch[1]); if (!supportedActions[actionMatch[1]]) unregisteredActions.push({ renderer: name, action: actionMatch[1] }); }
      }
    });
    return Object.freeze({ ok: !missingDataAction.length && !unregisteredActions.length, renderedActionCount: actions.size, registeredActionCount: Object.keys(supportedActions).length, missingDataAction, unregisteredActions, actions: Object.freeze([...actions].sort()) });
  }

  window.KayAdvanced = Object.freeze({ pages, render, bind, supportedActions, runUiChecks, renderPayments, renderExpenses, renderCommissions, renderAssets, renderCashBoxes, renderFiscalRules, renderDocuments, renderMasterData, renderAgedBalances });
})();
