using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KayOne;

/// <summary>
/// Persisted KAY ONE business kernel. One BusinessOperation is the aggregate root and every
/// commercial, fiscal, accounting, treasury and audit artefact keeps its OperationId.
/// The engine is UI agnostic and can be called directly by WebDashboardForm.
/// </summary>
public sealed class EnterpriseEngine
{
    private readonly object sync = new();
    private readonly EnterpriseJsonStore store;
    private readonly Func<DateTimeOffset> clock;
    private EnterpriseDatabase data;

    public EnterpriseEngine(EnterpriseEngineOptions? options = null)
    {
        options ??= new EnterpriseEngineOptions();
        clock = options.Clock ?? (() => DateTimeOffset.Now);
        store = new EnterpriseJsonStore(options.DataFilePath);
        store.EnforceAuthorization = options.EnforceAuthorization;
        data = store.Load();
        NormalizeDatabase();
        if (options.SeedDemoData && data.Companies.Count == 0)
            SeedDemoData();
        else
            SaveUnsafe();
    }

    public string DataFilePath => store.FilePath;

    public EnterpriseSnapshot GetSnapshot()
    {
        lock (sync)
        {
            var now = DateOnly.FromDateTime(clock().LocalDateTime);
            var customerAging = BuildAgedSummary(DueKind.Receivable, now);
            var supplierAging = BuildAgedSummary(DueKind.Debt, now);
            var metrics = new EnterpriseDashboardMetrics
            {
                RevenueMad = Round(data.ReportingFacts.Where(IsCurrentFact).Sum(x => x.RevenueMad)),
                ExpensesMad = Round(data.ReportingFacts.Where(IsCurrentFact).Sum(x => x.ExpenseMad)),
                MarginMad = Round(data.ReportingFacts.Where(IsCurrentFact).Sum(x => x.RevenueMad - x.ExpenseMad)),
                ProfitBeforeTaxMad = Round(data.ReportingFacts.Where(IsCurrentFact).Sum(x => x.RevenueMad - x.ExpenseMad)),
                CashBalanceMad = Round(data.BankAccounts.Sum(x => x.BalanceMad) + data.CashBoxes.Sum(x => x.BalanceMad)),
                ReceivablesMad = Round(data.DueItems.Where(x => x.Kind == DueKind.Receivable && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutstandingMad)),
                PayablesMad = Round(data.DueItems.Where(x => x.Kind == DueKind.Debt && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutstandingMad)),
                VatPayableMad = Round(data.TaxImpacts.Where(x => x.Status is not (ImpactState.Cancelled or ImpactState.Superseded) && IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutputVatMad - x.InputVatMad)),
                OverdueMad = Round(customerAging.TotalOverdueMad + supplierAging.TotalOverdueMad),
                OpenOperations = data.Operations.Count(x => x.Status is not (OperationLifecycle.Posted or OperationLifecycle.Cancelled)),
                UnreconciledBankItems = data.BankOperations.Count(x => x.ReconciliationStatus == ReconciliationStatus.Unreconciled)
            };

            var snapshot = new EnterpriseSnapshot
            {
                GeneratedAt = clock(),
                SchemaVersion = data.SchemaVersion,
                Metrics = metrics,
                CustomerAging = customerAging,
                SupplierAging = supplierAging,
                Companies = data.Companies.ToArray(),
                Sites = data.Sites.ToArray(),
                Laboratories = data.Laboratories.ToArray(),
                Activities = data.Activities.ToArray(),
                CostCenters = data.CostCenters.ToArray(),
                Projects = data.Projects.ToArray(),
                Parties = data.Parties.ToArray(),
                Operations = data.Operations.OrderByDescending(x => x.OperationDate).ThenByDescending(x => x.CreatedAt).ToArray(),
                Impacts = data.ImpactTraces.ToArray(),
                Documents = data.Documents.ToArray(),
                Invoices = data.Invoices.ToArray(),
                DueItems = data.DueItems.ToArray(),
                Payments = data.Payments.ToArray(),
                TaxRules = data.TaxRules.ToArray(),
                TaxImpacts = data.TaxImpacts.ToArray(),
                ExchangeRates = data.ExchangeRates.ToArray(),
                AccountingEntries = data.AccountingEntries.ToArray(),
                TreasuryMovements = data.TreasuryMovements.ToArray(),
                BankAccounts = data.BankAccounts.ToArray(),
                CashBoxes = data.CashBoxes.ToArray(),
                CashMovements = data.CashMovements.ToArray(),
                BankStatements = data.BankStatements.ToArray(),
                BankOperations = data.BankOperations.ToArray(),
                Reconciliations = data.Reconciliations.ToArray(),
                ExemptionCertificates = data.ExemptionCertificates.ToArray(),
                Contracts = data.Contracts.ToArray(),
                Commitments = data.Commitments.ToArray(),
                ImportFiles = data.ImportFiles.ToArray(),
                CommissionRules = data.CommissionRules.ToArray(),
                Commissions = data.Commissions.ToArray(),
                FixedAssets = data.FixedAssets.ToArray(),
                DepreciationEntries = data.DepreciationEntries.ToArray(),
                MasterImports = data.MasterImports.ToArray(),
                CollectionActions = data.CollectionActions.ToArray(),
                ReportingFacts = data.ReportingFacts.ToArray(),
                Roles = data.Roles.ToArray(),
                Users = data.Users.Select(SanitizeUser).ToArray(),
                Settings = data.Settings,
                AuditLog = data.AuditLog.OrderByDescending(x => x.OccurredAt).ToArray()
            };
            return Clone(snapshot);
        }
    }

    public EnterpriseSnapshot GetSnapshot(EnterpriseActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.View);
            if (actor.IsSystem || !store.EnforceAuthorization) return GetSnapshot();
            var user = actor.UserId.HasValue ? data.Users.FirstOrDefault(x => x.Id == actor.UserId.Value && x.IsActive) : null;
            if (user is null) throw new EnterpriseAuthorizationException("Utilisateur non authentifié ou inactif.");
            var allowedCompanies = user.CompanyIds.ToHashSet(); var snapshot = GetSnapshot();
            snapshot.Companies = snapshot.Companies.Where(x => allowedCompanies.Contains(x.Id)).ToArray();
            snapshot.Sites = snapshot.Sites.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var siteIds = snapshot.Sites.Select(x => x.Id).ToHashSet();
            snapshot.Laboratories = snapshot.Laboratories.Where(x => siteIds.Contains(x.SiteId)).ToArray();
            snapshot.CostCenters = snapshot.CostCenters.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); snapshot.Projects = snapshot.Projects.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray();
            snapshot.Operations = snapshot.Operations.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var operationIds = snapshot.Operations.Select(x => x.Id).ToHashSet();
            snapshot.Impacts = snapshot.Impacts.Where(x => operationIds.Contains(x.OperationId)).ToArray(); snapshot.Documents = snapshot.Documents.Where(x => x.OperationId.HasValue && operationIds.Contains(x.OperationId.Value)).ToArray();
            snapshot.Invoices = snapshot.Invoices.Where(x => operationIds.Contains(x.OperationId)).ToArray(); snapshot.DueItems = snapshot.DueItems.Where(x => operationIds.Contains(x.OperationId)).ToArray(); snapshot.Payments = snapshot.Payments.Where(x => operationIds.Contains(x.OperationId)).ToArray();
            snapshot.TaxImpacts = snapshot.TaxImpacts.Where(x => operationIds.Contains(x.OperationId)).ToArray(); snapshot.AccountingEntries = snapshot.AccountingEntries.Where(x => x.OperationId.HasValue && operationIds.Contains(x.OperationId.Value)).ToArray(); snapshot.TreasuryMovements = snapshot.TreasuryMovements.Where(x => x.OperationId.HasValue && operationIds.Contains(x.OperationId.Value) || x.CashBoxId.HasValue && snapshot.CashBoxes.Any(c => c.Id == x.CashBoxId && allowedCompanies.Contains(c.CompanyId))).ToArray();
            snapshot.BankAccounts = snapshot.BankAccounts.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var bankIds = snapshot.BankAccounts.Select(x => x.Id).ToHashSet();
            snapshot.CashBoxes = snapshot.CashBoxes.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var cashBoxIds = snapshot.CashBoxes.Select(x => x.Id).ToHashSet(); snapshot.CashMovements = snapshot.CashMovements.Where(x => cashBoxIds.Contains(x.CashBoxId)).ToArray();
            snapshot.BankStatements = snapshot.BankStatements.Where(x => bankIds.Contains(x.BankAccountId)).ToArray(); snapshot.BankOperations = snapshot.BankOperations.Where(x => x.BankAccountId.HasValue && bankIds.Contains(x.BankAccountId.Value)).ToArray(); var bankOperationIds = snapshot.BankOperations.Select(x => x.Id).ToHashSet(); snapshot.Reconciliations = snapshot.Reconciliations.Where(x => bankOperationIds.Contains(x.BankOperationId)).ToArray();
            snapshot.Contracts = snapshot.Contracts.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var contractIds = snapshot.Contracts.Select(x => x.Id).ToHashSet(); snapshot.Commitments = snapshot.Commitments.Where(x => contractIds.Contains(x.ContractId)).ToArray(); snapshot.ImportFiles = snapshot.ImportFiles.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray();
            snapshot.ReportingFacts = snapshot.ReportingFacts.Where(x => operationIds.Contains(x.OperationId)).ToArray(); snapshot.Commissions = snapshot.Commissions.Where(x => operationIds.Contains(x.SourceOperationId)).ToArray(); snapshot.FixedAssets = snapshot.FixedAssets.Where(x => allowedCompanies.Contains(x.CompanyId)).ToArray(); var assetIds = snapshot.FixedAssets.Select(x => x.Id).ToHashSet(); snapshot.DepreciationEntries = snapshot.DepreciationEntries.Where(x => assetIds.Contains(x.FixedAssetId)).ToArray(); snapshot.CollectionActions = snapshot.CollectionActions.Where(x => operationIds.Contains(x.OperationId)).ToArray();
            var partyIds = snapshot.Operations.Where(x => x.PartyId.HasValue).Select(x => x.PartyId!.Value).Concat(snapshot.Contracts.Where(x => x.PartyId.HasValue).Select(x => x.PartyId!.Value)).Concat(snapshot.ImportFiles.Select(x => x.SupplierId)).ToHashSet(); snapshot.Parties = snapshot.Parties.Where(x => partyIds.Contains(x.Id)).ToArray(); snapshot.ExemptionCertificates = snapshot.ExemptionCertificates.Where(x => partyIds.Contains(x.ClientId)).ToArray();
            if (!HasPermission(actor, EnterprisePermission.ManageSecurity)) { snapshot.Roles = Array.Empty<EnterpriseRole>(); snapshot.Users = Array.Empty<EnterpriseUser>(); }
            if (!HasPermission(actor, EnterprisePermission.ManageMasterData)) snapshot.MasterImports = Array.Empty<EnterpriseMasterImportJob>();
            snapshot.AuditLog = snapshot.AuditLog.Where(x => x.OperationId.HasValue && operationIds.Contains(x.OperationId.Value) || x.ActorUserId == actor.UserId).ToArray();
            snapshot.CustomerAging = BuildAgedSummary(snapshot.DueItems, DueKind.Receivable, DateOnly.FromDateTime(clock().LocalDateTime), operationIds);
            snapshot.SupplierAging = BuildAgedSummary(snapshot.DueItems, DueKind.Debt, DateOnly.FromDateTime(clock().LocalDateTime), operationIds);
            snapshot.Metrics = BuildScopedMetrics(snapshot, operationIds);
            return Clone(snapshot);
        }
    }

    /// <summary>
    /// Stable command boundary used by WebDashboardForm. Unknown/read-only UI commands are
    /// never presented as a domain mutation: they are audit-recorded and returned as NoOp.
    /// </summary>
    public EnterpriseActionResult HandleAction(string action, JsonElement payload) => HandleAction(action, payload, null);

    public EnterpriseActionResult HandleAction(string action, JsonElement payload, EnterpriseActor? actor)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new EnterpriseValidationException("Action manquante.");
        var command = action.Trim().ToLowerInvariant();
        var effectiveActor = ResolveActor(actor);
        try
        {
            object? result = command switch
            {
                "save-enterprise-operation" => CreateOperation(RequestFromJson(payload), effectiveActor),
                "submit-operation" => SubmitOperation(ResolveOperationId(payload), JsonText(payload, "comment"), effectiveActor),
                "validate-operation" => ValidateOperation(ResolveOperationId(payload), JsonText(payload, "comment"), effectiveActor),
                "post-operation" => PostOperation(ResolveOperationId(payload), JsonText(payload, "comment"), effectiveActor),
                "cancel-operation" => CancelOperation(ResolveOperationId(payload), JsonText(payload, "reason", "comment") ?? throw new EnterpriseValidationException("Motif d'annulation requis."), effectiveActor),
                "create-client" => CreatePartyFromJson(payload, PartyKind.Client, effectiveActor),
                "create-supplier" => CreatePartyFromJson(payload, PartyKind.Supplier, effectiveActor),
                "create-contract" => CreateContractFromJson(payload, effectiveActor),
                "create-import" => CreateImportFromJson(payload, effectiveActor),
                "create-certificate" => CreateCertificateFromJson(payload, effectiveActor),
                "create-role" => CreateRoleFromJson(payload, effectiveActor),
                "create-user" => CreateUserFromJson(payload, effectiveActor),
                "set-user-password" => SetUserPasswordFromJson(payload, effectiveActor),
                "save-import-costing" => SaveImportCosting(payload, effectiveActor),
                "prepare-payment" => PreparePayment(PaymentRequestFromJson(payload), effectiveActor),
                "approve-payment" => ApprovePayment(JsonGuid(payload, "paymentId", "id") ?? throw new EnterpriseValidationException("Paiement requis."), JsonText(payload, "comment"), effectiveActor),
                "cancel-payment" => CancelPayment(JsonGuid(payload, "paymentId", "id") ?? throw new EnterpriseValidationException("Paiement requis."), JsonText(payload, "reason") ?? "Annulation demandée", effectiveActor),
                "undo-reconciliation" => UndoReconciliation(JsonGuid(payload, "reconciliationId", "id") ?? throw new EnterpriseValidationException("Rapprochement requis."), JsonText(payload, "reason") ?? "Annulation demandée", effectiveActor),
                "reconcile-selected" => ReconcileSelected(payload, effectiveActor),
                "save-security" or "resolve-sod" => SaveSecurity(payload, effectiveActor),
                "generate-commitments" => GenerateCommitments(effectiveActor),
                "save-commission-rule" => SaveCommissionRule(payload, effectiveActor),
                "calculate-commissions" => CalculateCommissions(payload, effectiveActor),
                "run-depreciation" => RunDepreciation(payload, effectiveActor),
                "save-cash-box" => SaveCashBox(payload, effectiveActor),
                "save-cash-movement" => SaveCashMovement(payload, effectiveActor),
                "save-tax-rule" => SaveTaxRule(payload, effectiveActor),
                "toggle-tax-rule" => ToggleTaxRule(payload, effectiveActor),
                "submit-document-upload" => SubmitDocumentUpload(payload, effectiveActor),
                "run-document-ocr" => RunDocumentOcr(payload, effectiveActor),
                "save-master-record" => SaveMasterRecord(payload, effectiveActor),
                "submit-master-import" => SubmitMasterImport(payload, effectiveActor),
                "run-aged-actions" => RunAgedActions(payload, effectiveActor),
                "check-certificate-balance" => CheckCertificateBalance(payload, effectiveActor),
                "verify-audit-integrity" => AuditIntegrityAction(effectiveActor),
                _ => RecordNoOpAction(command, payload, effectiveActor)
            };
            return new EnterpriseActionResult { Success = true, Action = command, Message = ActionMessage(command, result), Data = result, Snapshot = MutatingAction(command) ? GetSnapshot(effectiveActor) : null };
        }
        catch (Exception ex) when (ex is EnterpriseValidationException or EnterpriseAuthorizationException or EnterpriseConcurrencyException or FormatException)
        {
            return new EnterpriseActionResult { Success = false, Action = command, Message = ex.Message, ErrorCode = ex.GetType().Name };
        }
    }

    /// <summary>Runs destructive workflow checks only against an isolated temporary datastore.</summary>
    public EnterpriseAcceptanceReport RunAcceptanceChecks()
    {
        var report = new EnterpriseAcceptanceReport { StartedAt = clock() };
        var folder = Path.Combine(Path.GetTempPath(), "KayOne-Acceptance-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "acceptance.json");
        try
        {
            var test = new EnterpriseEngine(new EnterpriseEngineOptions { DataFilePath = path, SeedDemoData = true, EnforceAuthorization = false, Clock = clock });
            var seed = test.GetSnapshot();
            var company = seed.Companies[0]; var customer = seed.Parties.First(x => x.Kind == PartyKind.Client); var supplier = seed.Parties.First(x => x.Kind == PartyKind.Supplier);
            BusinessOperation? created = null;
            Check(report, "operation.single-entry", () => { created = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Test intégration", CompanyId = company.Id, SiteId = seed.Sites.First(x => x.CompanyId == company.Id).Id, ActivityId = seed.Activities[0].Id, CostCenterId = seed.CostCenters.First(x => x.CompanyId == company.Id).Id, PartyId = customer.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Currency = "MAD", Amount = 10000, PaymentTermDays = 30, ExternalInvoiceNumber = "EXT-ACCEPT-01", DocumentFileName = "test.pdf", DocumentStorageKey = "acceptance/test.pdf", InvoiceLines = new List<CreateInvoiceLineRequest> { new() { Description = "Analyse A", Quantity = 1, UnitPriceMad = 4000, VatRate = .20m }, new() { Description = "Analyse B", Quantity = 2, UnitPriceMad = 3000, VatRate = .20m } } }); return created.Reference; });
            Check(report, "impacts.linked-and-persisted", () =>
            {
                var reloaded = new EnterpriseEngine(new EnterpriseEngineOptions { DataFilePath = path, SeedDemoData = false, Clock = clock });
                var trace = reloaded.GetOperationTrace(created!.Id);
                var concrete = trace.Documents.Count > 0 && trace.Invoices.Count > 0 && trace.DueItems.Count > 0 && trace.TaxImpacts.Count > 0 && trace.AccountingEntries.Count > 0 && trace.TreasuryMovements.Count > 0 && trace.ReportingFacts.Count > 0;
                if (!concrete || trace.Impacts.Select(x => x.Kind).Distinct().Count() < 10) throw new InvalidOperationException("Impacts incomplets.");
                return $"{trace.Impacts.Count} impacts tracés";
            });
            Check(report, "update.preserves-linked-metadata", () => { var revenue = test.GetSnapshot().Metrics.RevenueMad; test.UpdateOperation(created!.Id, new UpdateBusinessOperationRequest { ExpectedRowVersion = created.RowVersion, Description = "Description mise à jour", ChangeReason = "Acceptance" }); var snapshot = test.GetSnapshot(); var invoice = snapshot.Invoices.Single(x => x.OperationId == created.Id && x.Status != InvoiceStatus.Superseded); var document = snapshot.Documents.Single(x => x.OperationId == created.Id && x.Status != DocumentStatus.Superseded); if (invoice.ExternalNumber != "EXT-ACCEPT-01" || invoice.Lines.Count != 2 || document.ObjectStorageKey != "acceptance/test.pdf" || snapshot.Metrics.RevenueMad != revenue) throw new InvalidOperationException("Métadonnées perdues ou reporting doublé."); created = snapshot.Operations.First(x => x.Id == created.Id); return "Document, facture et reporting conservés"; });
            Check(report, "lifecycle.draft-excluded-from-official-ledgers", () =>
            {
                var certificate = test.GetSnapshot().ExemptionCertificates.First(); var certClient = test.GetSnapshot().Parties.First(x => x.Id == certificate.ClientId);
                var before = test.GetSnapshot();
                var draft = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Brouillon exonéré", CompanyId = company.Id, PartyId = certClient.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 100, ExemptionCertificateId = certificate.Id });
                var after = test.GetSnapshot(); var refreshedCertificate = after.ExemptionCertificates.First(x => x.Id == certificate.Id);
                if (after.Metrics.RevenueMad != before.Metrics.RevenueMad || after.Metrics.ReceivablesMad != before.Metrics.ReceivablesMad || after.Metrics.VatPayableMad != before.Metrics.VatPayableMad || refreshedCertificate.ConsumedAmountMad != certificate.ConsumedAmountMad) throw new InvalidOperationException("Le brouillon a alimenté un indicateur officiel ou consommé le certificat.");
                test.CancelOperation(draft.Id, "Nettoyage acceptance"); return "Brouillons exclus; certificat non consommé";
            });
            Check(report, "workflow.handle-action-commands", () =>
            {
                var submit = test.HandleAction("submit-operation", JsonSerializer.SerializeToElement(new { operationId = created!.Id }, EnterpriseJson.Options));
                var validate = test.HandleAction("validate-operation", JsonSerializer.SerializeToElement(new { operationId = created.Id }, EnterpriseJson.Options));
                var purchase = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Achat, Nature = "Workflow fournisseur", CompanyId = company.Id, PartyId = supplier.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), DueDate = DateOnly.FromDateTime(clock().LocalDateTime).AddDays(-5), Amount = 500 });
                var purchaseSubmit = test.HandleAction("submit-operation", JsonSerializer.SerializeToElement(new { operationReference = purchase.Reference }, EnterpriseJson.Options));
                var purchaseValidate = test.HandleAction("validate-operation", JsonSerializer.SerializeToElement(new { operationId = purchase.Id }, EnterpriseJson.Options));
                var post = test.HandleAction("post-operation", JsonSerializer.SerializeToElement(new { operationId = purchase.Id }, EnterpriseJson.Options));
                var disposable = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.OperationDiverse, Nature = "À annuler", CompanyId = company.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 10 });
                var cancel = test.HandleAction("cancel-operation", JsonSerializer.SerializeToElement(new { operationId = disposable.Id, reason = "Acceptance" }, EnterpriseJson.Options));
                if (new[] { submit, validate, purchaseSubmit, purchaseValidate, post, cancel }.Any(x => !x.Success) || test.GetOperation(created.Id)?.Status != OperationLifecycle.Validated || test.GetOperation(purchase.Id)?.Status != OperationLifecycle.Posted || test.GetOperation(disposable.Id)?.Status != OperationLifecycle.Cancelled) throw new InvalidOperationException("Une commande de workflow n'a pas appliqué son état.");
                return "Soumission, validation, comptabilisation et annulation persistées";
            });
            Check(report, "operation.idempotency", () => { var key = "acceptance-idempotency"; var request = new CreateBusinessOperationRequest { Type = BusinessOperationType.Achat, Nature = "Idempotence", CompanyId = company.Id, PartyId = supplier.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 100, IdempotencyKey = key }; var first = test.CreateOperation(request); var second = test.CreateOperation(request); if (first.Id != second.Id) throw new InvalidOperationException("Deux agrégats ont été créés."); return first.Reference; });
            Check(report, "tax.no-double-vat-on-cash", () => { var cash = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Encaissement, Nature = "Encaissement direct", CompanyId = company.Id, PartyId = customer.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 5000 }); if (cash.VatAmountMad != 0 || test.GetOperationTrace(cash.Id).Invoices.Any()) throw new InvalidOperationException("TVA ou facture indue sur encaissement."); return "TVA 0, sans seconde facture"; });
            Check(report, "reporting.profit-before-tax-excludes-vat", () =>
            {
                var snapshot = test.GetSnapshot();
                if (snapshot.Metrics.VatPayableMad <= 0) throw new InvalidOperationException("TVA collectée absente du scénario de contrôle.");
                if (snapshot.Metrics.ProfitBeforeTaxMad != snapshot.Metrics.MarginMad) throw new InvalidOperationException("La TVA a été soustraite du résultat avant impôt.");
                return $"{snapshot.Metrics.ProfitBeforeTaxMad:N2} MAD = CA HT - coûts, TVA suivie séparément ({snapshot.Metrics.VatPayableMad:N2} MAD)";
            });
            Check(report, "accounting.balanced", () => { var entries = test.GetSnapshot().AccountingEntries; if (entries.Any(e => Math.Abs(e.Lines.Sum(l => l.DebitMad) - e.Lines.Sum(l => l.CreditMad)) > .01m)) throw new InvalidOperationException("Écriture déséquilibrée."); return $"{entries.Count} écritures équilibrées"; });
            Check(report, "audit.hash-chain", () => test.VerifyAuditChain(out var invalid) ? "Chaîne valide" : throw new InvalidOperationException($"Rupture {invalid}"));
            Check(report, "tax.exemption-insufficient", () =>
            {
                var certificate = seed.ExemptionCertificates.First(); var certClient = seed.Parties.First(x => x.Id == certificate.ClientId);
                try { test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Test exonération", CompanyId = company.Id, PartyId = certClient.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Currency = "MAD", Amount = certificate.RemainingAmountMad + 700, ExemptionCertificateId = certificate.Id }); }
                catch (EnterpriseValidationException ex) when (ex.Message.Contains("insuffisant", StringComparison.OrdinalIgnoreCase)) { return "Blocage correct"; }
                throw new InvalidOperationException("Le dépassement aurait dû être bloqué.");
            });
            Check(report, "currency.fx-metadata", () => { var fx = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Achat, Nature = "Test EUR", CompanyId = company.Id, PartyId = supplier.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Currency = "EUR", Amount = 100, ExchangeRate = 10.22m, ExchangeRateDate = DateOnly.FromDateTime(clock().LocalDateTime), ExchangeRateSource = "Acceptance", SettlementExchangeRate = 10.25m, BankFeesMad = 25 }); if (fx.AmountMad != 1022m || fx.ExchangeDifferenceMad != 3m || fx.ExchangeRateSource != "Acceptance") throw new InvalidOperationException("Conversion ou métadonnées FX incorrectes."); return $"100 EUR = {fx.AmountMad} MAD"; });
            Check(report, "aged-balance.clients-and-suppliers", () => { var snap = test.GetSnapshot(); if (snap.CustomerAging.TotalMad <= 0 || snap.SupplierAging.TotalMad <= 0) throw new InvalidOperationException("Balance âgée manquante."); return $"Clients {snap.CustomerAging.TotalMad:N2}; fournisseurs {snap.SupplierAging.TotalMad:N2}"; });
            Check(report, "aged-balance.future-due-is-not-due", () =>
            {
                var today = DateOnly.FromDateTime(clock().LocalDateTime); var before = test.GetSnapshot().CustomerAging.NotDueMad;
                var future = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Échéance future Acceptance", CompanyId = company.Id, PartyId = customer.Id, OperationDate = today, DueDate = today.AddDays(45), Amount = 1234m });
                test.SubmitOperation(future.Id); test.ValidateOperation(future.Id);
                var after = test.GetSnapshot().CustomerAging.NotDueMad;
                if (after < before + 1234m) throw new InvalidOperationException("Une échéance future a été classée en retard.");
                return "Délai 45 jours classé Non échue jusqu'à la date d'échéance";
            });
            Check(report, "global-search.reference-amount-iban", () => { var byRef = test.Search(created!.Reference); var byAmount = test.Search("10 000 DH"); var bank = test.GetSnapshot().BankAccounts[0]; var byIban = test.Search(bank.Iban); if (!byRef.Any(x => x.OperationId == created.Id) || !byAmount.Any(x => x.OperationId == created.Id) || !byIban.Any(x => x.Kind == "BankOperation")) throw new InvalidOperationException("Recherche incomplète."); return "Référence, montant et IBAN trouvés"; });
            Check(report, "snapshot.detached", () => { var snapshot = test.GetSnapshot(); var original = snapshot.Companies[0].Name; snapshot.Companies[0].Name = "MUTATION INTERDITE"; if (test.GetSnapshot().Companies[0].Name != original) throw new InvalidOperationException("Une mutation non auditée a atteint le store."); return "Copie défensive active"; });
            Check(report, "references.unique", () => { var refs = test.GetSnapshot().Operations.Select(x => x.Reference).ToArray(); if (refs.Distinct(StringComparer.OrdinalIgnoreCase).Count() != refs.Length) throw new InvalidOperationException("Références dupliquées."); return $"{refs.Length} références uniques"; });
            Check(report, "advanced.commission-rule-and-calculation", () =>
            {
                var saved = test.HandleAction("save-commission-rule", JsonSerializer.SerializeToElement(new { name = "Commission commerciale", basis = "Ventes facturées", rate = 2.5m, capMad = 5000m }, EnterpriseJson.Options));
                var calculated = test.HandleAction("calculate-commissions", JsonSerializer.SerializeToElement(new { period = "ACCEPTANCE" }, EnterpriseJson.Options));
                var snapshot = test.GetSnapshot(); if (!saved.Success || !calculated.Success || snapshot.CommissionRules.Count == 0 || snapshot.Commissions.Count == 0 || snapshot.Commissions.Any(x => x.AmountMad <= 0 || !snapshot.Operations.Any(o => o.Id == x.SourceOperationId))) throw new InvalidOperationException("Règle ou commissions non persistées/lien source absent.");
                return $"{snapshot.Commissions.Count} commissions liées aux ventes";
            });
            Check(report, "advanced.fixed-assets-and-depreciation", () =>
            {
                var asset = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Immobilisation, Nature = "Analyseur Acceptance", CompanyId = company.Id, SiteId = seed.Sites.First(x => x.CompanyId == company.Id).Id, CostCenterId = seed.CostCenters.First(x => x.CompanyId == company.Id).Id, PartyId = supplier.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime).AddMonths(-3), Amount = 120000, CustomFields = new Dictionary<string, string> { ["serviceDate"] = DateOnly.FromDateTime(clock().LocalDateTime).AddMonths(-2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), ["depreciationYears"] = "5", ["depreciationMethod"] = "Linéaire" } });
                test.SubmitOperation(asset.Id); test.ValidateOperation(asset.Id);
                var action = test.HandleAction("run-depreciation", JsonSerializer.SerializeToElement(new { asOfDate = DateOnly.FromDateTime(clock().LocalDateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, EnterpriseJson.Options));
                var snapshot = test.GetSnapshot(); var depreciation = snapshot.DepreciationEntries.FirstOrDefault(x => x.SourceOperationId == asset.Id);
                if (!action.Success || depreciation is null || !snapshot.FixedAssets.Any(x => x.SourceOperationId == asset.Id && x.AccumulatedDepreciationMad > 0) || !snapshot.AccountingEntries.Any(x => x.Id == depreciation.AccountingEntryId && Math.Abs(x.Lines.Sum(l => l.DebitMad) - x.Lines.Sum(l => l.CreditMad)) <= .01m)) throw new InvalidOperationException("Dotation ou écriture d'amortissement manquante.");
                return $"Dotation {depreciation.AmountMad:N2} MAD";
            });
            Check(report, "advanced.cash-box-and-movement", () =>
            {
                var boxResult = test.HandleAction("save-cash-box", JsonSerializer.SerializeToElement(new { name = "Caisse Acceptance", companyId = company.Id, siteId = seed.Sites.First(x => x.CompanyId == company.Id).Id, currency = "MAD" }, EnterpriseJson.Options));
                var box = test.GetSnapshot().CashBoxes.First(x => x.Name == "Caisse Acceptance");
                var movementResult = test.HandleAction("save-cash-movement", JsonSerializer.SerializeToElement(new { cashBoxId = box.Id, kind = "Entrée", amount = 250m, label = "Fonds de caisse Acceptance" }, EnterpriseJson.Options));
                var outResult = test.HandleAction("save-cash-movement", JsonSerializer.SerializeToElement(new { cashBoxId = box.Id, kind = "Sortie", amount = 90m, label = "Achat caisse Acceptance" }, EnterpriseJson.Options));
                var snapshot = test.GetSnapshot(); var movement = snapshot.CashMovements.FirstOrDefault(x => x.CashBoxId == box.Id && x.Direction == TreasuryDirection.Inflow); var outflow = snapshot.CashMovements.FirstOrDefault(x => x.CashBoxId == box.Id && x.Direction == TreasuryDirection.Outflow);
                if (!boxResult.Success || !movementResult.Success || !outResult.Success || movement is null || outflow is null || snapshot.CashBoxes.First(x => x.Id == box.Id).BalanceMad != 160m || !snapshot.TreasuryMovements.Any(x => x.CashBoxId == box.Id && x.Direction == TreasuryDirection.Outflow && x.AmountMad == 90m) || !snapshot.AccountingEntries.Any(x => x.Id == movement.AccountingEntryId) || !snapshot.AccountingEntries.Any(x => x.Id == outflow.AccountingEntryId)) throw new InvalidOperationException("Mouvement, solde ou impacts de caisse manquants.");
                return "Entrée +250 MAD, sortie -90 MAD, solde 160 MAD";
            });
            Check(report, "advanced.tax-rule-version-and-toggle", () =>
            {
                var save = test.HandleAction("save-tax-rule", JsonSerializer.SerializeToElement(new { code = "RAS-ACCEPT", name = "RAS Acceptance", kind = "RAS", rate = 7.5m, effectiveFrom = "2026-01-01" }, EnterpriseJson.Options));
                var rule = test.GetSnapshot().TaxRules.First(x => x.Code == "RAS-ACCEPT");
                var toggle = test.HandleAction("toggle-tax-rule", JsonSerializer.SerializeToElement(new { taxRuleId = rule.Id, isActive = false }, EnterpriseJson.Options));
                var stored = test.GetSnapshot().TaxRules.First(x => x.Id == rule.Id);
                if (!save.Success || !toggle.Success || stored.IsActive || stored.Rate != .075m || stored.Kind != TaxRuleKind.Withholding) throw new InvalidOperationException("Version ou activation de règle fiscale incorrecte.");
                return "Règle RAS paramétrée puis désactivée";
            });
            Check(report, "advanced.document-intake-and-ocr-queue", () =>
            {
                var upload = test.HandleAction("submit-document-upload", JsonSerializer.SerializeToElement(new { files = new[] { new { name = "facture-acceptance.pdf", size = 2048, type = "application/pdf" } } }, EnterpriseJson.Options));
                var document = test.GetSnapshot().Documents.First(x => x.FileName == "facture-acceptance.pdf");
                var ocr = test.HandleAction("run-document-ocr", JsonSerializer.SerializeToElement(new { documentId = document.Id }, EnterpriseJson.Options));
                var queued = test.GetSnapshot().Documents.First(x => x.Id == document.Id);
                if (!upload.Success || !ocr.Success || queued.FileSizeBytes != 2048 || !queued.OcrStatus.Contains("OCR", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Intake documentaire ou file OCR non persisté.");
                return queued.OcrStatus;
            });
            Check(report, "advanced.master-data-and-import", () =>
            {
                var master = test.HandleAction("save-master-record", JsonSerializer.SerializeToElement(new { domain = "projects", code = "PRJ-ACCEPT", name = "Projet Acceptance", companyId = company.Id, status = "Actif" }, EnterpriseJson.Options));
                var import = test.HandleAction("submit-master-import", JsonSerializer.SerializeToElement(new { domain = "projects", files = new[] { new { name = "projects.csv", size = 512, type = "text/csv" } } }, EnterpriseJson.Options));
                var snapshot = test.GetSnapshot(); if (!master.Success || !import.Success || !snapshot.Projects.Any(x => x.Code == "PRJ-ACCEPT") || !snapshot.MasterImports.Any(x => x.FileName == "projects.csv" && x.Status.Contains("attente", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Référentiel ou demande d'import non persisté.");
                return "Référentiel créé; import tracé en attente du binaire";
            });
            Check(report, "advanced.aged-actions", () =>
            {
                var overdue = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Créance échue Acceptance", CompanyId = company.Id, PartyId = customer.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime).AddDays(-40), DueDate = DateOnly.FromDateTime(clock().LocalDateTime).AddDays(-10), Amount = 900 });
                test.SubmitOperation(overdue.Id); test.ValidateOperation(overdue.Id);
                var action = test.HandleAction("run-aged-actions", JsonSerializer.SerializeToElement(new { side = "customer" }, EnterpriseJson.Options));
                var snapshot = test.GetSnapshot(); if (!action.Success || !snapshot.CollectionActions.Any(x => x.OperationId == overdue.Id && x.Side == DueKind.Receivable && x.DaysOverdue >= 10)) throw new InvalidOperationException("Action de relance non planifiée.");
                return "Relance client planifiée et liée à l'échéance";
            });
            Check(report, "security.separation-of-duties", () =>
            {
                var securePath = Path.Combine(folder, "secure.json");
                var secure = new EnterpriseEngine(new EnterpriseEngineOptions { DataFilePath = securePath, SeedDemoData = true, EnforceAuthorization = true, Clock = clock });
                var s = secure.GetSnapshot(); var adminRole = s.Roles.First(r => r.Name == "Administrateur"); var makerUser = s.Users.First(x => x.RoleIds.Contains(adminRole.Id));
                var checkerUser = secure.UpsertUser(new EnterpriseUser { DisplayName = "Contrôleur Acceptance", Email = "checker@acceptance.local", IsActive = true, RoleIds = new List<Guid> { adminRole.Id }, CompanyIds = new List<Guid> { s.Companies[0].Id } }, EnterpriseActor.System);
                var maker = new EnterpriseActor(makerUser.Id, makerUser.DisplayName); var checker = new EnterpriseActor(checkerUser.Id, checkerUser.DisplayName);
                var op = secure.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "SoD", CompanyId = s.Companies[0].Id, PartyId = s.Parties.First(x => x.Kind == PartyKind.Client).Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 1000 }, maker);
                secure.SubmitOperation(op.Id, null, maker);
                try { secure.ValidateOperation(op.Id, null, maker); throw new InvalidOperationException("La validation par le créateur aurait dû être bloquée."); } catch (EnterpriseValidationException ex) when (ex.Message.Contains("Séparation", StringComparison.OrdinalIgnoreCase)) { }
                secure.ValidateOperation(op.Id, null, checker);
                var prepared = secure.PreparePayment(new RegisterPaymentRequest { OperationId = op.Id, PaymentDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 1200, Currency = "MAD", BankAccountId = s.BankAccounts.First(x => x.CompanyId == s.Companies[0].Id && x.Currency == "MAD").Id }, maker);
                try { secure.ApprovePayment(prepared.Id, null, maker); throw new InvalidOperationException("Le préparateur a validé son paiement."); } catch (EnterpriseValidationException ex) when (ex.Message.Contains("Séparation", StringComparison.OrdinalIgnoreCase)) { }
                var executed = secure.ApprovePayment(prepared.Id, "Validation indépendante", checker); if (executed.Status != PaymentStatus.Executed) throw new InvalidOperationException("Paiement non exécuté après validation.");
                return "Maker-checker opération et paiement appliqué";
            });
            Check(report, "security.password-authentication", () =>
            {
                var authPath = Path.Combine(folder, "auth.json"); var auth = new EnterpriseEngine(new EnterpriseEngineOptions { DataFilePath = authPath, SeedDemoData = true, EnforceAuthorization = true, Clock = clock });
                if (auth.HasConfiguredAdministrator) throw new InvalidOperationException("L'initialisation devait demander un mot de passe.");
                auth.SetupFirstAdministrator("owner@acceptance.local", "Owner Acceptance", "StrongPass!2026");
                var rejected = auth.Authenticate("owner@acceptance.local", "incorrect-password"); var accepted = auth.Authenticate("owner@acceptance.local", "StrongPass!2026");
                if (rejected.Success || !accepted.Success || !accepted.PasswordVerified || accepted.Actor is null || accepted.User is null || accepted.User.PasswordHash is not null || !auth.HasConfiguredAdministrator || !auth.VerifyAuditChain(out _)) throw new InvalidOperationException("Authentification PBKDF2 ou audit de connexion incorrect.");
                var securedSnapshot = auth.GetSnapshot(accepted.Actor);
                var createdUser = auth.HandleAction("create-user", JsonSerializer.SerializeToElement(new { displayName = "Utilisateur Acceptance", email = "user@acceptance.local", roleId = securedSnapshot.Roles.First().Id, companyId = securedSnapshot.Companies.First().Id }, EnterpriseJson.Options), accepted.Actor);
                if (!createdUser.Success || createdUser.Data is not EnterpriseUser newUser) throw new InvalidOperationException("Création utilisateur impossible depuis le workflow d'administration.");
                var passwordConfigured = auth.HandleAction("set-user-password", JsonSerializer.SerializeToElement(new { userId = newUser.Id, password = "UserPass!2026" }, EnterpriseJson.Options), accepted.Actor);
                var userLogin = auth.Authenticate("user@acceptance.local", "UserPass!2026");
                if (!passwordConfigured.Success || !userLogin.Success || !userLogin.PasswordVerified) throw new InvalidOperationException("Le compte créé n'est pas utilisable avec le mot de passe initial.");
                return "PBKDF2; création utilisateur; secrets hors snapshot; audit de connexion";
            });
            Check(report, "reconciliation.cancelled-state-cannot-resurrect", () =>
            {
                var cancelled = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Encaissement, Nature = "Encaissement annulé", CompanyId = company.Id, PartyId = customer.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 50 });
                var bankOperation = test.GetSnapshot().BankOperations.First(x => x.OperationId == cancelled.Id); test.CancelOperation(cancelled.Id, "Acceptance");
                var bank = test.GetSnapshot().BankAccounts.First(x => x.Id == bankOperation.BankAccountId);
                var statement = test.ImportBankStatement(new BankStatementImportRequest { BankAccountId = bank.Id, PeriodStart = DateOnly.FromDateTime(clock().LocalDateTime), PeriodEnd = DateOnly.FromDateTime(clock().LocalDateTime), OpeningBalanceMad = 0, ClosingBalanceMad = 50, Lines = new List<BankStatementLineRequest> { new() { BookingDate = DateOnly.FromDateTime(clock().LocalDateTime), ValueDate = DateOnly.FromDateTime(clock().LocalDateTime), Label = "Ligne annulée", AmountMad = 50 } } });
                try { test.ReconcileBankOperation(new ReconcileBankRequest { BankOperationId = bankOperation.Id, StatementLineId = statement.Lines[0].Id }); throw new InvalidOperationException("Un élément rejeté a été rapproché."); }
                catch (EnterpriseValidationException) { }
                if (test.GetOperation(cancelled.Id)?.Status != OperationLifecycle.Cancelled) throw new InvalidOperationException("L'opération annulée a été ressuscitée.");
                return "Rapprochement rejeté; annulation préservée";
            });
            Check(report, "cancellation.reverses-live-impacts", () => { var before = test.GetSnapshot().Metrics.RevenueMad; var op = test.CreateOperation(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Annulation", CompanyId = company.Id, PartyId = customer.Id, OperationDate = DateOnly.FromDateTime(clock().LocalDateTime), Amount = 777 }); test.SubmitOperation(op.Id); test.ValidateOperation(op.Id); var withOperation = test.GetSnapshot().Metrics.RevenueMad; test.CancelOperation(op.Id, "Acceptance"); var after = test.GetSnapshot(); if (withOperation <= before || after.Metrics.RevenueMad != before || after.Invoices.Any(x => x.OperationId == op.Id && x.Status != InvoiceStatus.Cancelled) || after.DueItems.Any(x => x.OperationId == op.Id && x.Status != DueStatus.Cancelled)) throw new InvalidOperationException("Impacts d'annulation incomplets."); return "Facture, échéance, fiscalité et reporting neutralisés"; });
            Check(report, "audit.final-hash-chain", () => test.VerifyAuditChain(out var invalid) ? "Chaîne valide après tous workflows" : throw new InvalidOperationException($"Rupture {invalid}"));
            Check(report, "persistence.atomic-reload", () => { var before = test.GetSnapshot().Operations.Count; var after = new EnterpriseEngine(new EnterpriseEngineOptions { DataFilePath = path, SeedDemoData = false, Clock = clock }).GetSnapshot().Operations.Count; if (before != after) throw new InvalidOperationException("Écart après rechargement."); return $"{after} opérations rechargées"; });
        }
        finally
        {
            report.FinishedAt = clock();
            try { if (Directory.Exists(folder)) Directory.Delete(folder, true); } catch (IOException) { }
        }
        return report;
    }

    public BusinessOperation? GetOperation(Guid operationId)
    {
        lock (sync) { var operation = data.Operations.FirstOrDefault(x => x.Id == operationId); return operation is null ? null : Clone(operation); }
    }

    public BusinessOperation? GetOperation(Guid operationId, EnterpriseActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        lock (sync) { DemandPermission(actor, EnterprisePermission.View); var operation = data.Operations.FirstOrDefault(x => x.Id == operationId); if (operation is null) return null; DemandCompanyAccess(actor, operation.CompanyId); return Clone(operation); }
    }

    public BusinessOperation? GetOperation(string reference)
    {
        lock (sync) { var operation = data.Operations.FirstOrDefault(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)); return operation is null ? null : Clone(operation); }
    }

    public OperationTrace GetOperationTrace(Guid operationId)
    {
        lock (sync)
        {
            var operation = RequireOperation(operationId);
            return Clone(new OperationTrace
            {
                Operation = operation,
                Impacts = data.ImpactTraces.Where(x => x.OperationId == operationId).ToArray(),
                Documents = data.Documents.Where(x => x.OperationId == operationId).ToArray(),
                Invoices = data.Invoices.Where(x => x.OperationId == operationId).ToArray(),
                DueItems = data.DueItems.Where(x => x.OperationId == operationId).ToArray(),
                Payments = data.Payments.Where(x => x.OperationId == operationId).ToArray(),
                TaxImpacts = data.TaxImpacts.Where(x => x.OperationId == operationId).ToArray(),
                AccountingEntries = data.AccountingEntries.Where(x => x.OperationId == operationId).ToArray(),
                TreasuryMovements = data.TreasuryMovements.Where(x => x.OperationId == operationId).ToArray(),
                BankOperations = data.BankOperations.Where(x => x.OperationId == operationId).ToArray(),
                ReportingFacts = data.ReportingFacts.Where(x => x.OperationId == operationId).ToArray(),
                AuditLog = data.AuditLog.Where(x => x.EntityId == operationId.ToString("D") || x.OperationId == operationId).OrderBy(x => x.OccurredAt).ToArray()
            });
        }
    }

    public OperationTrace GetOperationTrace(Guid operationId, EnterpriseActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        lock (sync) { DemandPermission(actor, EnterprisePermission.View); var operation = RequireOperation(operationId); DemandCompanyAccess(actor, operation.CompanyId); return GetOperationTrace(operationId); }
    }

    public BusinessOperation CreateOperation(CreateBusinessOperationRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.CreateOperation);
            DemandCompanyAccess(effectiveActor, request.CompanyId);
            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existing = data.Operations.FirstOrDefault(x => string.Equals(x.IdempotencyKey, request.IdempotencyKey.Trim(), StringComparison.OrdinalIgnoreCase));
                if (existing is not null) return Clone(existing);
            }
            var checkpoint = Checkpoint();
            try { return Clone(CreateOperationUnsafe(request, effectiveActor, save: true)); }
            catch { data = checkpoint; throw; }
        }
    }

    /// <summary>Updates a draft while retaining old generated impacts as Superseded for traceability.</summary>
    public BusinessOperation UpdateOperation(Guid operationId, UpdateBusinessOperationRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.EditOperation);
            var current = RequireOperation(operationId);
            if (current.Status != OperationLifecycle.Draft)
                throw new EnterpriseValidationException("Seule une opération au brouillon peut être modifiée. Utilisez une extourne pour une opération validée.");
            if (request.ExpectedRowVersion.HasValue && request.ExpectedRowVersion != current.RowVersion)
                throw new EnterpriseConcurrencyException("L'opération a été modifiée par un autre utilisateur. Rechargez les données.");

            var checkpoint = Checkpoint();
            try
            {
                var before = JsonSerializer.Serialize(current, EnterpriseJson.Options);
                var merged = ToCreateRequest(current, request);
                ValidateOperationRequest(merged);
                DemandCompanyAccess(effectiveActor, merged.CompanyId);
                SupersedeImpacts(operationId);
                ApplyRequest(current, merged);
                current.RowVersion++;
                current.UpdatedAt = clock();
                GenerateAutomaticImpacts(current, merged, effectiveActor);
                AddAudit("BusinessOperation", current.Id.ToString("D"), current.Id, "Updated", before, JsonSerializer.Serialize(current, EnterpriseJson.Options), request.ChangeReason, effectiveActor);
                SaveUnsafe();
                return Clone(current);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public BusinessOperation SubmitOperation(Guid operationId, string? comment = null, EnterpriseActor? actor = null) =>
        ChangeStatus(operationId, OperationLifecycle.Submitted, comment, actor);

    public BusinessOperation ValidateOperation(Guid operationId, string? comment = null, EnterpriseActor? actor = null) =>
        ChangeStatus(operationId, OperationLifecycle.Validated, comment, actor);

    public BusinessOperation PostOperation(Guid operationId, string? comment = null, EnterpriseActor? actor = null) =>
        ChangeStatus(operationId, OperationLifecycle.Posted, comment, actor);

    public BusinessOperation ChangeStatus(Guid operationId, OperationLifecycle target, string? comment = null, EnterpriseActor? actor = null)
    {
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            var operation = RequireOperation(operationId);
            DemandCompanyAccess(effectiveActor, operation.CompanyId);
            var permission = target switch
            {
                OperationLifecycle.Submitted => EnterprisePermission.SubmitOperation,
                OperationLifecycle.Validated => EnterprisePermission.ValidateOperation,
                OperationLifecycle.Posted => EnterprisePermission.PostAccounting,
                _ => EnterprisePermission.EditOperation
            };
            DemandPermission(effectiveActor, permission);
            EnsureTransition(operation.Status, target);
            if (target == OperationLifecycle.Validated && operation.CreatedByUserId.HasValue && operation.CreatedByUserId == effectiveActor.UserId && !effectiveActor.IsSystem)
                throw new EnterpriseValidationException("Séparation des tâches : le créateur ne peut pas valider sa propre opération.");
            var checkpoint = Checkpoint();
            try
            {
                var previous = operation.Status;
                if (target == OperationLifecycle.Validated) ConsumeExemptionOnValidation(operation);
                operation.Status = target;
                operation.RowVersion++;
                operation.UpdatedAt = clock();
                if (target == OperationLifecycle.Validated) operation.ValidatedByUserId = effectiveActor.UserId;
                if (target == OperationLifecycle.Posted)
                {
                    foreach (var entry in data.AccountingEntries.Where(x => x.OperationId == operationId && x.Status == AccountingEntryStatus.Draft))
                        entry.Status = AccountingEntryStatus.Posted;
                    SetImpactState(operationId, ImpactKind.Accounting, ImpactState.Posted);
                }
                AddAudit("BusinessOperation", operation.Id.ToString("D"), operation.Id, $"Status:{previous}->{target}", previous.ToString(), target.ToString(), comment, effectiveActor);
                SaveUnsafe();
                return Clone(operation);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public BusinessOperation CancelOperation(Guid operationId, string reason, EnterpriseActor? actor = null)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new EnterpriseValidationException("Le motif d'annulation est obligatoire.");
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.CancelOperation);
            var operation = RequireOperation(operationId);
            DemandCompanyAccess(effectiveActor, operation.CompanyId);
            if (operation.Status == OperationLifecycle.Cancelled) return Clone(operation);
            if (data.Payments.Any(x => x.OperationId == operationId && x.Status != PaymentStatus.Cancelled))
                throw new EnterpriseValidationException("Annulez ou contre-passez les paiements liés avant l'opération.");
            if (data.BankOperations.Any(x => x.OperationId == operationId && x.ReconciliationStatus == ReconciliationStatus.Reconciled))
                throw new EnterpriseValidationException("Une opération bancaire rapprochée doit d'abord faire l'objet d'une contre-passation de rapprochement.");

            foreach (var due in data.DueItems.Where(x => x.OperationId == operationId)) due.Status = DueStatus.Cancelled;
            foreach (var invoice in data.Invoices.Where(x => x.OperationId == operationId)) invoice.Status = InvoiceStatus.Cancelled;
            foreach (var document in data.Documents.Where(x => x.OperationId == operationId)) document.Status = DocumentStatus.Archived;
            if (operation.ExemptionCertificateId.HasValue && operation.ExemptionConsumed)
            {
                var certificate = data.ExemptionCertificates.FirstOrDefault(x => x.Id == operation.ExemptionCertificateId);
                if (certificate is not null) { certificate.ConsumedAmountMad = Round(Math.Max(0, certificate.ConsumedAmountMad - operation.AmountMad)); RefreshCertificateStatus(certificate); }
                operation.ExemptionConsumed = false;
            }
            foreach (var tax in data.TaxImpacts.Where(x => x.OperationId == operationId)) tax.Status = ImpactState.Cancelled;
            foreach (var movement in data.TreasuryMovements.Where(x => x.OperationId == operationId)) movement.Status = TreasuryMovementStatus.Cancelled;
            foreach (var fact in data.ReportingFacts.Where(x => x.OperationId == operationId)) fact.Status = ImpactState.Cancelled;
            foreach (var bankOperation in data.BankOperations.Where(x => x.OperationId == operationId && x.ReconciliationStatus != ReconciliationStatus.Reconciled)) bankOperation.ReconciliationStatus = ReconciliationStatus.Rejected;
            foreach (var entry in data.AccountingEntries.Where(x => x.OperationId == operationId && x.Status == AccountingEntryStatus.Draft)) entry.Status = AccountingEntryStatus.Superseded;
            foreach (var impact in data.ImpactTraces.Where(x => x.OperationId == operationId)) impact.State = ImpactState.Cancelled;

            var postedEntries = data.AccountingEntries.Where(x => x.OperationId == operationId && x.Status == AccountingEntryStatus.Posted).ToList();
            foreach (var posted in postedEntries)
            {
                var reversal = new EnterpriseAccountingEntry
                {
                    Id = Guid.NewGuid(), OperationId = operationId, Reference = NextReference("EXT"), EntryDate = DateOnly.FromDateTime(clock().LocalDateTime),
                    JournalCode = posted.JournalCode, Label = $"Extourne {posted.Reference} — {reason}", Status = AccountingEntryStatus.Posted,
                    ReversesEntryId = posted.Id,
                    Lines = posted.Lines.Select(x => new EnterpriseAccountingLine
                    {
                        AccountCode = x.AccountCode, Label = "Extourne — " + x.Label, DebitMad = x.CreditMad, CreditMad = x.DebitMad, Dimensions = x.Dimensions
                    }).ToList()
                };
                data.AccountingEntries.Add(reversal);
                posted.Status = AccountingEntryStatus.Reversed;
            }
            operation.Status = OperationLifecycle.Cancelled;
            operation.CancellationReason = reason.Trim();
            operation.RowVersion++;
            operation.UpdatedAt = clock();
            AddAudit("BusinessOperation", operation.Id.ToString("D"), operation.Id, "Cancelled", null, operation.Status.ToString(), reason, effectiveActor);
            SaveUnsafe();
            return Clone(operation);
        }
    }

    public EnterprisePayment RegisterPayment(RegisterPaymentRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.CreatePayment);
            if (store.EnforceAuthorization && !effectiveActor.IsSystem)
                throw new EnterpriseValidationException("En mode sécurisé, utilisez PreparePayment puis ApprovePayment afin d'appliquer la séparation des tâches.");
            var operation = RequireOperation(request.OperationId);
            DemandCompanyAccess(effectiveActor, operation.CompanyId);
            if (operation.Status is OperationLifecycle.Draft or OperationLifecycle.Submitted or OperationLifecycle.Cancelled)
                throw new EnterpriseValidationException("L'opération doit être validée avant paiement et ne peut pas être annulée.");
            if (request.Amount <= 0) throw new EnterpriseValidationException("Le montant du paiement doit être supérieur à zéro.");
            var currency = NormalizeCurrency(request.Currency ?? operation.Currency);
            var rate = ResolveExchangeRate(currency, request.PaymentDate, request.ExchangeRate);
            var amountMad = Round(request.Amount * rate);
            var dues = data.DueItems.Where(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).OrderBy(x => x.DueDate).ToList();
            var totalOutstanding = dues.Sum(x => x.OutstandingMad);
            if (dues.Count == 0) throw new EnterpriseValidationException("Aucune échéance ouverte ne peut recevoir ce paiement.");
            if (dues.Count > 0 && amountMad > totalOutstanding + 0.01m)
                throw new EnterpriseValidationException($"Le paiement dépasse le solde ouvert de {totalOutstanding:N2} MAD.");
            var bank = data.BankAccounts.FirstOrDefault(x => x.Id == request.BankAccountId)
                ?? throw new EnterpriseValidationException("Compte bancaire introuvable.");
            if (!bank.IsActive || bank.CompanyId != operation.CompanyId) throw new EnterpriseValidationException("Le compte bancaire doit être actif et appartenir à la société de l'opération.");
            if (!request.AllowCurrencyConversion && !bank.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)) throw new EnterpriseValidationException("La devise du paiement doit correspondre au compte bancaire, sauf conversion explicitement autorisée.");
            Guid? approverId = request.ApprovedByUserId;
            if (store.EnforceAuthorization && !effectiveActor.IsSystem)
            {
                if (!approverId.HasValue || approverId == effectiveActor.UserId) throw new EnterpriseValidationException("Séparation des tâches : le préparateur et le valideur du paiement doivent être différents.");
                var approver = data.Users.FirstOrDefault(x => x.Id == approverId && x.IsActive) ?? throw new EnterpriseValidationException("Valideur de paiement introuvable ou inactif.");
                if (!data.Roles.Where(x => approver.RoleIds.Contains(x.Id)).SelectMany(x => x.Permissions).Contains(EnterprisePermission.ValidatePayment)) throw new EnterpriseAuthorizationException("Le valideur ne possède pas la permission ValidatePayment.");
            }

            var payment = new EnterprisePayment
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("PAY"), PaymentDate = request.PaymentDate,
                Amount = Round(request.Amount), Currency = currency, ExchangeRate = rate, AmountMad = amountMad,
                BankAccountId = bank.Id, Method = request.Method, ExternalReference = Clean(request.ExternalReference),
                Status = PaymentStatus.Executed, PreviousOperationStatus = operation.Status, PreparedByUserId = effectiveActor.UserId, ApprovedByUserId = approverId, CreatedAt = clock()
            };
            data.Payments.Add(payment);

            var remaining = amountMad;
            foreach (var due in dues)
            {
                var applied = Math.Min(remaining, due.OutstandingMad);
                due.PaidMad = Round(due.PaidMad + applied);
                due.OutstandingMad = Round(due.OriginalAmountMad - due.PaidMad);
                due.Status = due.OutstandingMad <= 0.01m ? DueStatus.Paid : DueStatus.PartiallyPaid;
                payment.Allocations.Add(new PaymentAllocation { DueItemId = due.Id, AmountMad = applied });
                remaining -= applied;
                if (remaining <= 0.01m) break;
            }

            var incoming = operation.Direction == OperationDirection.Incoming;
            bank.BalanceMad = Round(bank.BalanceMad + (incoming ? amountMad : -amountMad));
            data.TreasuryMovements.Add(new EnterpriseTreasuryMovement
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("TRM"),
                MovementDate = request.PaymentDate, Direction = incoming ? TreasuryDirection.Inflow : TreasuryDirection.Outflow,
                AmountMad = amountMad, Currency = currency, BankAccountId = bank.Id, Status = TreasuryMovementStatus.Executed,
                Label = $"{payment.Reference} · {operation.Reference}"
            });
            var bankOperation = new EnterpriseBankOperation
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("BOP"),
                BankAccountId = bank.Id, OperationDate = request.PaymentDate, ValueDate = request.PaymentDate,
                AmountMad = incoming ? amountMad : -amountMad, Currency = currency, Label = $"{request.Method} {operation.Reference}",
                ReconciliationStatus = ReconciliationStatus.Unreconciled
            };
            data.BankOperations.Add(bankOperation);
            AddPaymentAccountingEntry(operation, payment, incoming);

            var stillOpen = data.DueItems.Any(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid);
            foreach (var invoice in data.Invoices.Where(x => x.OperationId == operation.Id && x.Status is InvoiceStatus.Open or InvoiceStatus.PartiallyPaid))
                invoice.Status = stillOpen ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Paid;
            operation.Status = stillOpen ? OperationLifecycle.PartiallyPaid : OperationLifecycle.Paid;
            operation.RowVersion++;
            operation.UpdatedAt = clock();
            SetImpactState(operation.Id, ImpactKind.Treasury, ImpactState.Generated);
            SetImpactState(operation.Id, ImpactKind.Bank, ImpactState.Pending);
            SetImpactState(operation.Id, ImpactKind.AgedBalance, stillOpen ? ImpactState.Active : ImpactState.Settled);
            AddAudit("Payment", payment.Id.ToString("D"), operation.Id, "Created", null, JsonSerializer.Serialize(payment, EnterpriseJson.Options), request.Comment, effectiveActor);
            SaveUnsafe();
            return Clone(payment);
        }
    }

    public EnterprisePayment PreparePayment(RegisterPaymentRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.CreatePayment);
            if (effectiveActor.IsSystem && store.EnforceAuthorization) throw new EnterpriseAuthorizationException("Un utilisateur authentifié doit préparer le paiement.");
            var operation = RequireOperation(request.OperationId);
            DemandCompanyAccess(effectiveActor, operation.CompanyId);
            if (operation.Status is OperationLifecycle.Draft or OperationLifecycle.Submitted or OperationLifecycle.Cancelled) throw new EnterpriseValidationException("L'opération doit être validée avant paiement.");
            var currency = NormalizeCurrency(request.Currency ?? operation.Currency); var rate = ResolveExchangeRate(currency, request.PaymentDate, request.ExchangeRate); var amountMad = Round(request.Amount * rate);
            if (request.Amount <= 0) throw new EnterpriseValidationException("Le montant doit être supérieur à zéro.");
            var outstanding = data.DueItems.Where(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Sum(x => x.OutstandingMad);
            if (outstanding <= 0 || amountMad > outstanding + .01m) throw new EnterpriseValidationException("Le paiement ne correspond pas au solde des échéances ouvertes.");
            var bank = data.BankAccounts.FirstOrDefault(x => x.Id == request.BankAccountId && x.IsActive && x.CompanyId == operation.CompanyId) ?? throw new EnterpriseValidationException("Compte bancaire actif introuvable pour cette société.");
            if (!request.AllowCurrencyConversion && !bank.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)) throw new EnterpriseValidationException("La devise du paiement et celle du compte bancaire diffèrent.");
            var checkpoint = Checkpoint();
            try
            {
                var payment = new EnterprisePayment { Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("PAY"), PaymentDate = request.PaymentDate, Amount = Round(request.Amount), Currency = currency, ExchangeRate = rate, AmountMad = amountMad, BankAccountId = bank.Id, Method = Required(request.Method, "Mode de paiement"), ExternalReference = Clean(request.ExternalReference), Status = PaymentStatus.Prepared, PreviousOperationStatus = operation.Status, PreparedByUserId = effectiveActor.UserId, CreatedAt = clock() };
                data.Payments.Add(payment); operation.Status = OperationLifecycle.PaymentPending; operation.RowVersion++; operation.UpdatedAt = clock();
                AddAudit("Payment", payment.Id.ToString("D"), operation.Id, "Prepared", null, JsonSerializer.Serialize(payment, EnterpriseJson.Options), request.Comment, effectiveActor); SaveUnsafe(); return Clone(payment);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public EnterprisePayment ApprovePayment(Guid paymentId, string? comment = null, EnterpriseActor? actor = null)
    {
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ValidatePayment);
            if (effectiveActor.IsSystem && store.EnforceAuthorization) throw new EnterpriseAuthorizationException("Un utilisateur authentifié doit valider le paiement.");
            var payment = data.Payments.FirstOrDefault(x => x.Id == paymentId) ?? throw new EnterpriseValidationException("Paiement introuvable.");
            if (payment.Status != PaymentStatus.Prepared) throw new EnterpriseValidationException("Seul un paiement préparé peut être validé.");
            if (payment.PreparedByUserId == effectiveActor.UserId && !effectiveActor.IsSystem) throw new EnterpriseValidationException("Séparation des tâches : le préparateur ne peut pas valider son paiement.");
            var operation = RequireOperation(payment.OperationId); var bank = data.BankAccounts.FirstOrDefault(x => x.Id == payment.BankAccountId) ?? throw new EnterpriseValidationException("Compte bancaire introuvable.");
            DemandCompanyAccess(effectiveActor, operation.CompanyId);
            var checkpoint = Checkpoint();
            try
            {
                payment.Status = PaymentStatus.Approved; payment.ApprovedByUserId = effectiveActor.UserId; payment.ApprovedAt = clock();
                ExecutePreparedPaymentUnsafe(operation, payment, bank, effectiveActor, comment); SaveUnsafe(); return Clone(payment);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public EnterprisePayment CancelPayment(Guid paymentId, string reason, EnterpriseActor? actor = null)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new EnterpriseValidationException("Le motif de contre-passation est obligatoire.");
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ValidatePayment);
            var payment = data.Payments.FirstOrDefault(x => x.Id == paymentId) ?? throw new EnterpriseValidationException("Paiement introuvable.");
            var scopedOperation = RequireOperation(payment.OperationId); DemandCompanyAccess(effectiveActor, scopedOperation.CompanyId);
            if (payment.Status == PaymentStatus.Cancelled) return Clone(payment);
            if (data.BankOperations.Any(x => x.PaymentId == payment.Id && x.ReconciliationStatus == ReconciliationStatus.Reconciled)) throw new EnterpriseValidationException("Annulez d'abord le rapprochement du paiement.");
            var checkpoint = Checkpoint();
            try
            {
                var priorPaymentStatus = payment.Status;
                foreach (var allocation in payment.Allocations)
                {
                    var due = data.DueItems.FirstOrDefault(x => x.Id == allocation.DueItemId); if (due is null) continue;
                    due.PaidMad = Round(Math.Max(0, due.PaidMad - allocation.AmountMad)); due.OutstandingMad = Round(due.OriginalAmountMad - due.PaidMad); due.Status = due.PaidMad <= .01m ? DueStatus.Open : DueStatus.PartiallyPaid;
                }
                var operation = scopedOperation; var incoming = operation.Direction == OperationDirection.Incoming;
                var bank = data.BankAccounts.FirstOrDefault(x => x.Id == payment.BankAccountId); if (bank is not null && (priorPaymentStatus is PaymentStatus.Executed or PaymentStatus.Reconciled)) bank.BalanceMad = Round(bank.BalanceMad + (incoming ? -payment.AmountMad : payment.AmountMad));
                foreach (var movement in data.TreasuryMovements.Where(x => x.PaymentId == payment.Id)) movement.Status = TreasuryMovementStatus.Cancelled;
                foreach (var bankOperation in data.BankOperations.Where(x => x.PaymentId == payment.Id)) bankOperation.ReconciliationStatus = ReconciliationStatus.Rejected;
                foreach (var entry in data.AccountingEntries.Where(x => x.PaymentId == payment.Id).ToList())
                {
                    if (entry.Status == AccountingEntryStatus.Draft) entry.Status = AccountingEntryStatus.Superseded;
                    else if (entry.Status == AccountingEntryStatus.Posted) { data.AccountingEntries.Add(new EnterpriseAccountingEntry { Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("EXT"), EntryDate = DateOnly.FromDateTime(clock().LocalDateTime), JournalCode = entry.JournalCode, Label = $"Extourne {entry.Reference}", Status = AccountingEntryStatus.Posted, ReversesEntryId = entry.Id, Lines = entry.Lines.Select(x => new EnterpriseAccountingLine { AccountCode = x.AccountCode, Label = "Extourne — " + x.Label, DebitMad = x.CreditMad, CreditMad = x.DebitMad, Dimensions = x.Dimensions }).ToList() }); entry.Status = AccountingEntryStatus.Reversed; }
                }
                payment.Status = PaymentStatus.Cancelled; payment.CancelledAt = clock(); payment.CancellationReason = reason.Trim();
                RecomputeOperationPaymentLifecycle(operation); operation.RowVersion++; operation.UpdatedAt = clock();
                foreach (var invoice in data.Invoices.Where(x => x.OperationId == operation.Id && x.Status != InvoiceStatus.Cancelled))
                {
                    var invoiceDues = data.DueItems.Where(x => x.InvoiceId == invoice.Id).ToArray(); var paid = invoiceDues.Sum(x => x.PaidMad);
                    invoice.Status = invoiceDues.All(x => x.Status == DueStatus.Paid) ? InvoiceStatus.Paid : paid <= .01m ? InvoiceStatus.Open : InvoiceStatus.PartiallyPaid;
                }
                AddAudit("Payment", payment.Id.ToString("D"), operation.Id, "Cancelled", null, payment.Status.ToString(), reason, effectiveActor); SaveUnsafe(); return Clone(payment);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public EnterpriseReconciliation UndoReconciliation(Guid reconciliationId, string reason, EnterpriseActor? actor = null)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new EnterpriseValidationException("Le motif d'annulation du rapprochement est obligatoire.");
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ReconcileBank);
            var reconciliation = data.Reconciliations.FirstOrDefault(x => x.Id == reconciliationId) ?? throw new EnterpriseValidationException("Rapprochement introuvable.");
            if (reconciliation.IsReversed) return Clone(reconciliation);
            var scopedBankOperation = data.BankOperations.FirstOrDefault(x => x.Id == reconciliation.BankOperationId) ?? throw new EnterpriseValidationException("Opération bancaire introuvable.");
            var scopedBankAccount = data.BankAccounts.FirstOrDefault(x => x.Id == scopedBankOperation.BankAccountId) ?? throw new EnterpriseValidationException("Compte bancaire introuvable.");
            DemandCompanyAccess(effectiveActor, scopedBankAccount.CompanyId);
            var line = data.BankStatements.SelectMany(x => x.Lines).FirstOrDefault(x => x.Id == reconciliation.StatementLineId); if (line is not null) { line.ReconciliationStatus = ReconciliationStatus.Unreconciled; line.LinkedBankOperationId = null; }
            var bankOperation = data.BankOperations.FirstOrDefault(x => x.Id == reconciliation.BankOperationId); if (bankOperation is not null) { bankOperation.ReconciliationStatus = ReconciliationStatus.Unreconciled; bankOperation.StatementLineId = null; var payment = data.Payments.FirstOrDefault(x => x.Id == bankOperation.PaymentId); if (payment is not null) payment.Status = PaymentStatus.Executed; }
            reconciliation.IsReversed = true; reconciliation.ReversedAt = clock(); reconciliation.ReversedByUserId = effectiveActor.UserId; reconciliation.ReversalReason = reason.Trim();
            if (reconciliation.OperationId is Guid operationId) { var operation = RequireOperation(operationId); var open = data.DueItems.Any(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid); if (operation.Status != OperationLifecycle.Posted) operation.Status = open ? OperationLifecycle.PartiallyPaid : OperationLifecycle.Paid; operation.RowVersion++; operation.UpdatedAt = clock(); SetImpactState(operation.Id, ImpactKind.Bank, ImpactState.Pending); }
            AddAudit("Reconciliation", reconciliation.Id.ToString("D"), reconciliation.OperationId, "Reversed", null, "Unreconciled", reason, effectiveActor); SaveUnsafe(); return Clone(reconciliation);
        }
    }

    private void ExecutePreparedPaymentUnsafe(BusinessOperation operation, EnterprisePayment payment, EnterpriseBankAccount bank, EnterpriseActor actor, string? comment)
    {
        var dues = data.DueItems.Where(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).OrderBy(x => x.DueDate).ToList();
        var remaining = payment.AmountMad; if (remaining > dues.Sum(x => x.OutstandingMad) + .01m) throw new EnterpriseValidationException("Le paiement dépasse le solde ouvert.");
        foreach (var due in dues) { var applied = Math.Min(remaining, due.OutstandingMad); due.PaidMad = Round(due.PaidMad + applied); due.OutstandingMad = Round(due.OriginalAmountMad - due.PaidMad); due.Status = due.OutstandingMad <= .01m ? DueStatus.Paid : DueStatus.PartiallyPaid; payment.Allocations.Add(new PaymentAllocation { DueItemId = due.Id, AmountMad = applied }); remaining -= applied; if (remaining <= .01m) break; }
        var incoming = operation.Direction == OperationDirection.Incoming; bank.BalanceMad = Round(bank.BalanceMad + (incoming ? payment.AmountMad : -payment.AmountMad));
        data.TreasuryMovements.Add(new EnterpriseTreasuryMovement { Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("TRM"), MovementDate = payment.PaymentDate, Direction = incoming ? TreasuryDirection.Inflow : TreasuryDirection.Outflow, AmountMad = payment.AmountMad, Currency = payment.Currency, BankAccountId = bank.Id, Status = TreasuryMovementStatus.Executed, Label = $"{payment.Reference} · {operation.Reference}" });
        data.BankOperations.Add(new EnterpriseBankOperation { Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("BOP"), BankAccountId = bank.Id, OperationDate = payment.PaymentDate, ValueDate = payment.PaymentDate, AmountMad = incoming ? payment.AmountMad : -payment.AmountMad, Currency = payment.Currency, Label = $"{payment.Method} {operation.Reference}", ReconciliationStatus = ReconciliationStatus.Unreconciled });
        AddPaymentAccountingEntry(operation, payment, incoming); payment.Status = PaymentStatus.Executed; payment.ExecutedAt = clock();
        var stillOpen = data.DueItems.Any(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid); foreach (var invoice in data.Invoices.Where(x => x.OperationId == operation.Id && x.Status is InvoiceStatus.Open or InvoiceStatus.PartiallyPaid)) invoice.Status = stillOpen ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Paid;
        operation.Status = stillOpen ? OperationLifecycle.PartiallyPaid : OperationLifecycle.Paid; operation.RowVersion++; operation.UpdatedAt = clock(); SetImpactState(operation.Id, ImpactKind.Treasury, ImpactState.Generated); SetImpactState(operation.Id, ImpactKind.Bank, ImpactState.Pending); SetImpactState(operation.Id, ImpactKind.AgedBalance, stillOpen ? ImpactState.Active : ImpactState.Settled);
        AddAudit("Payment", payment.Id.ToString("D"), operation.Id, "ApprovedAndExecuted", PaymentStatus.Prepared.ToString(), PaymentStatus.Executed.ToString(), comment, actor);
    }

    private void RecomputeOperationPaymentLifecycle(BusinessOperation operation)
    {
        if (operation.Status is OperationLifecycle.Cancelled or OperationLifecycle.Posted) return;
        var dues = data.DueItems.Where(x => x.OperationId == operation.Id && x.Status is not (DueStatus.Cancelled or DueStatus.Superseded)).ToArray();
        var livePayments = data.Payments.Where(x => x.OperationId == operation.Id && x.Status != PaymentStatus.Cancelled).ToArray();
        var hasReconciled = livePayments.Any(x => x.Status == PaymentStatus.Reconciled);
        var hasExecuted = livePayments.Any(x => x.Status is PaymentStatus.Executed or PaymentStatus.Reconciled);
        var hasPrepared = livePayments.Any(x => x.Status is PaymentStatus.Prepared or PaymentStatus.Approved);
        var outstanding = dues.Sum(x => x.OutstandingMad); var paid = dues.Sum(x => x.PaidMad);
        operation.Status = hasReconciled && outstanding <= .01m ? OperationLifecycle.Reconciled
            : hasExecuted && outstanding <= .01m ? OperationLifecycle.Paid
            : paid > .01m ? OperationLifecycle.PartiallyPaid
            : hasPrepared ? OperationLifecycle.PaymentPending
            : OperationLifecycle.Validated;
    }

    public EnterpriseReconciliation ReconcileBankOperation(ReconcileBankRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.ReconcileBank);
            var bankOperation = data.BankOperations.FirstOrDefault(x => x.Id == request.BankOperationId)
                ?? throw new EnterpriseValidationException("Opération bancaire introuvable.");
            var statement = data.BankStatements.FirstOrDefault(x => x.Lines.Any(l => l.Id == request.StatementLineId))
                ?? throw new EnterpriseValidationException("Ligne de relevé introuvable.");
            var bankAccount = data.BankAccounts.FirstOrDefault(x => x.Id == statement.BankAccountId) ?? throw new EnterpriseValidationException("Compte bancaire introuvable.");
            DemandCompanyAccess(effectiveActor, bankAccount.CompanyId);
            var line = statement.Lines.First(x => x.Id == request.StatementLineId);
            if (line.ReconciliationStatus != ReconciliationStatus.Unreconciled || bankOperation.ReconciliationStatus != ReconciliationStatus.Unreconciled)
                throw new EnterpriseValidationException("Seuls des éléments non rapprochés et actifs peuvent être rapprochés.");
            var linkedPaymentBefore = data.Payments.FirstOrDefault(x => x.Id == bankOperation.PaymentId);
            if (linkedPaymentBefore?.Status == PaymentStatus.Cancelled) throw new EnterpriseValidationException("Un paiement annulé ne peut pas être rapproché.");
            var linkedOperationBefore = data.Operations.FirstOrDefault(x => x.Id == bankOperation.OperationId);
            if (linkedOperationBefore?.Status == OperationLifecycle.Cancelled) throw new EnterpriseValidationException("Une opération annulée ne peut pas être rapprochée.");
            var difference = Round(Math.Abs(line.AmountMad - bankOperation.AmountMad));
            var tolerance = Math.Clamp(request.AllowedDifferenceMad, 0, data.Settings.MaxReconciliationDifferenceMad);
            if (statement.BankAccountId != bankOperation.BankAccountId) throw new EnterpriseValidationException("Le relevé et l'opération doivent appartenir au même compte bancaire.");
            if (difference > tolerance)
                throw new EnterpriseValidationException($"Écart de rapprochement {difference:N2} MAD supérieur à la tolérance.");

            var reconciliation = new EnterpriseReconciliation
            {
                Id = Guid.NewGuid(), Reference = NextReference("RAP"), BankOperationId = bankOperation.Id,
                BankStatementId = statement.Id, StatementLineId = line.Id, OperationId = bankOperation.OperationId,
                DifferenceMad = difference, ReconciledAt = clock(), ReconciledByUserId = effectiveActor.UserId, Comment = Clean(request.Comment)
            };
            data.Reconciliations.Add(reconciliation);
            line.ReconciliationStatus = ReconciliationStatus.Reconciled;
            line.LinkedBankOperationId = bankOperation.Id;
            bankOperation.ReconciliationStatus = ReconciliationStatus.Reconciled;
            bankOperation.StatementLineId = line.Id;
            var linkedPayment = data.Payments.FirstOrDefault(x => x.Id == bankOperation.PaymentId);
            if (linkedPayment is not null) linkedPayment.Status = PaymentStatus.Reconciled;
            var operation = data.Operations.FirstOrDefault(x => x.Id == bankOperation.OperationId);
            var hasOutstandingDue = operation is not null && data.DueItems.Any(x => x.OperationId == operation.Id && x.Status is DueStatus.Open or DueStatus.PartiallyPaid);
            if (operation is not null && !hasOutstandingDue && !data.BankOperations.Any(x => x.OperationId == operation.Id && x.ReconciliationStatus != ReconciliationStatus.Reconciled))
            {
                operation.Status = OperationLifecycle.Reconciled;
                operation.RowVersion++;
                operation.UpdatedAt = clock();
                SetImpactState(operation.Id, ImpactKind.Bank, ImpactState.Reconciled);
            }
            AddAudit("Reconciliation", reconciliation.Id.ToString("D"), bankOperation.OperationId, "Created", null, JsonSerializer.Serialize(reconciliation, EnterpriseJson.Options), request.Comment, effectiveActor);
            SaveUnsafe();
            return Clone(reconciliation);
        }
    }

    public EnterpriseBankStatement ImportBankStatement(BankStatementImportRequest request, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor);
            DemandPermission(effectiveActor, EnterprisePermission.ImportBankStatement);
            var account = data.BankAccounts.FirstOrDefault(x => x.Id == request.BankAccountId) ?? throw new EnterpriseValidationException("Compte bancaire introuvable.");
            DemandCompanyAccess(effectiveActor, account.CompanyId);
            if (!account.IsActive) throw new EnterpriseValidationException("Le compte bancaire est inactif.");
            if (request.PeriodEnd < request.PeriodStart) throw new EnterpriseValidationException("La période du relevé est invalide.");
            var statement = new EnterpriseBankStatement
            {
                Id = Guid.NewGuid(), Reference = NextReference("REL"), BankAccountId = request.BankAccountId,
                PeriodStart = request.PeriodStart, PeriodEnd = request.PeriodEnd, OpeningBalanceMad = request.OpeningBalanceMad,
                ClosingBalanceMad = request.ClosingBalanceMad, SourceDocumentKey = Clean(request.SourceDocumentKey), ImportedAt = clock(),
                Lines = request.Lines.Select(x => new EnterpriseBankStatementLine
                {
                    Id = Guid.NewGuid(), BookingDate = x.BookingDate, ValueDate = x.ValueDate, Label = Required(x.Label, "Libellé bancaire"),
                    Reference = Clean(x.Reference), AmountMad = Round(x.AmountMad), ReconciliationStatus = ReconciliationStatus.Unreconciled
                }).ToList()
            };
            data.BankStatements.Add(statement);
            AddAudit("BankStatement", statement.Id.ToString("D"), null, "Imported", null, JsonSerializer.Serialize(statement, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe();
            return Clone(statement);
        }
    }

    public IReadOnlyList<EnterpriseSearchResult> Search(string? query, EnterpriseSearchFilter? filter = null)
    {
        lock (sync)
        {
            filter ??= new EnterpriseSearchFilter();
            var q = NormalizeSearch(query);
            decimal? queryAmount = TryParseAmount(query, out var parsed) ? parsed : null;
            var results = new List<EnterpriseSearchResult>();
            bool TextMatch(params string?[] fields) => q.Length == 0 || queryAmount.HasValue || fields.Any(x => NormalizeSearch(x).Contains(q, StringComparison.Ordinal));
            bool AmountMatch(decimal? amount, string? currency, Guid? companyId, DateOnly? date, Guid? partyId)
            {
                if (filter.CompanyId.HasValue && companyId != filter.CompanyId) return false;
                if (filter.PartyId.HasValue && partyId != filter.PartyId) return false;
                if (!string.IsNullOrWhiteSpace(filter.Currency) && !string.Equals(currency, filter.Currency, StringComparison.OrdinalIgnoreCase)) return false;
                if (filter.StartDate.HasValue && date < filter.StartDate) return false;
                if (filter.EndDate.HasValue && date > filter.EndDate) return false;
                if (filter.ExactAmountMad.HasValue && (!amount.HasValue || Math.Abs(amount.Value - filter.ExactAmountMad.Value) > filter.AmountToleranceMad)) return false;
                if (queryAmount.HasValue && filter.MatchAmountFromQuery && (!amount.HasValue || Math.Abs(Math.Abs(amount.Value) - queryAmount.Value) > filter.AmountToleranceMad)) return false;
                if (filter.MinimumAmountMad.HasValue && amount < filter.MinimumAmountMad) return false;
                if (filter.MaximumAmountMad.HasValue && amount > filter.MaximumAmountMad) return false;
                return true;
            }
            void Add(string kind, string id, Guid? operationId, string title, string subtitle, decimal? amount, string? currency, string route, int score)
            {
                results.Add(new EnterpriseSearchResult { Kind = kind, EntityId = id, OperationId = operationId, Title = title, Subtitle = subtitle, Amount = amount, Currency = currency, Route = route, Score = score });
            }

            foreach (var x in data.Operations)
                if ((!filter.OperationTypes.Any() || filter.OperationTypes.Contains(x.Type)) && TextMatch(x.Reference, x.Nature, x.SourceReference, PartyName(x.PartyId)) && AmountMatch(x.AmountMad, x.Currency, x.CompanyId, x.OperationDate, x.PartyId))
                    Add("Operation", x.Id.ToString("D"), x.Id, x.Reference, $"{x.Type} · {x.Nature} · {PartyName(x.PartyId)}", x.AmountMad, "MAD", $"operations/{x.Id}", 100);
            foreach (var x in data.Invoices)
                if (TextMatch(x.Number, x.ExternalNumber, PartyName(x.PartyId)) && AmountMatch(x.TotalMad, x.Currency, x.CompanyId, x.InvoiceDate, x.PartyId))
                    Add("Invoice", x.Id.ToString("D"), x.OperationId, x.Number, $"Facture · {PartyName(x.PartyId)}", x.TotalMad, "MAD", $"invoices/{x.Id}", 95);
            foreach (var x in data.Payments)
                if (TextMatch(x.Reference, x.ExternalReference) && AmountMatch(x.AmountMad, x.Currency, OperationCompany(x.OperationId), x.PaymentDate, OperationParty(x.OperationId)))
                    Add("Payment", x.Id.ToString("D"), x.OperationId, x.Reference, $"Paiement · {x.Method}", x.AmountMad, "MAD", $"payments/{x.Id}", 90);
            foreach (var x in data.BankOperations)
                if (TextMatch(x.Reference, x.Label, BankIban(x.BankAccountId)) && AmountMatch(Math.Abs(x.AmountMad), x.Currency, OperationCompany(x.OperationId), x.OperationDate, OperationParty(x.OperationId)))
                    Add("BankOperation", x.Id.ToString("D"), x.OperationId, x.Reference, $"Banque · {x.Label}", x.AmountMad, "MAD", $"treasury/bank/{x.Id}", 85);
            foreach (var x in data.AccountingEntries)
                if (TextMatch(x.Reference, x.Label, string.Join(' ', x.Lines.Select(l => l.AccountCode))) && AmountMatch(x.Lines.Sum(l => l.DebitMad), "MAD", OperationCompany(x.OperationId), x.EntryDate, OperationParty(x.OperationId)))
                    Add("AccountingEntry", x.Id.ToString("D"), x.OperationId, x.Reference, $"Écriture · {x.JournalCode}", x.Lines.Sum(l => l.DebitMad), "MAD", $"accounting/entries/{x.Id}", 80);
            foreach (var x in data.Documents)
                if (TextMatch(x.Reference, x.FileName, x.DocumentType, x.ObjectStorageKey))
                    Add("Document", x.Id.ToString("D"), x.OperationId, x.Reference, $"Document · {x.FileName}", null, null, $"documents/{x.Id}", 75);
            foreach (var x in data.Contracts)
                if (TextMatch(x.Reference, x.Title, PartyName(x.PartyId)) && AmountMatch(x.BaseAmountMad, "MAD", x.CompanyId, x.StartDate, x.PartyId))
                    Add("Contract", x.Id.ToString("D"), null, x.Reference, $"Contrat · {x.Title}", x.BaseAmountMad, "MAD", $"contracts/{x.Id}", 70);
            foreach (var x in data.TaxImpacts)
                if (TextMatch(x.Reference, x.RuleCode) && AmountMatch(x.OutputVatMad + x.InputVatMad + x.WithholdingMad, "MAD", OperationCompany(x.OperationId), x.TaxDate, OperationParty(x.OperationId)))
                    Add("Tax", x.Id.ToString("D"), x.OperationId, x.Reference, $"Fiscalité · {x.RuleCode}", x.OutputVatMad + x.InputVatMad + x.WithholdingMad, "MAD", $"tax/{x.Id}", 65);

            return results.OrderByDescending(x => x.Score).ThenByDescending(x => x.Amount).Take(Math.Clamp(filter.Limit, 1, 250)).ToArray();
        }
    }

    public IReadOnlyList<EnterpriseSearchResult> Search(string? query, EnterpriseSearchFilter? filter, EnterpriseActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.View);
            var results = Search(query, filter);
            if (actor.IsSystem || !store.EnforceAuthorization) return results;
            var user = actor.UserId.HasValue ? data.Users.FirstOrDefault(x => x.Id == actor.UserId.Value && x.IsActive) : null;
            if (user is null) throw new EnterpriseAuthorizationException("Utilisateur non authentifié ou inactif.");
            var companies = user.CompanyIds.ToHashSet();
            return results.Where(result => result.OperationId.HasValue && data.Operations.Any(x => x.Id == result.OperationId.Value && companies.Contains(x.CompanyId))).ToArray();
        }
    }

    public EnterpriseCompany UpsertCompany(EnterpriseCompany company, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(company);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageMasterData);
            company.Name = Required(company.Name, "Dénomination"); company.Code = Required(company.Code, "Code société").ToUpperInvariant();
            var existing = company.Id == Guid.Empty ? null : data.Companies.FirstOrDefault(x => x.Id == company.Id);
            if (data.Companies.Any(x => x.Id != company.Id && x.Code.Equals(company.Code, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Ce code société existe déjà.");
            var action = existing is null ? "Created" : "Updated";
            if (existing is null) { company.Id = Guid.NewGuid(); data.Companies.Add(company); }
            else CopyCompany(company, existing);
            AddAudit("Company", company.Id.ToString("D"), null, action, null, JsonSerializer.Serialize(company, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? company);
        }
    }

    public EnterpriseParty UpsertParty(EnterpriseParty party, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(party);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageMasterData);
            party.Name = Required(party.Name, "Nom du tiers");
            var existing = party.Id == Guid.Empty ? null : data.Parties.FirstOrDefault(x => x.Id == party.Id);
            if (!string.IsNullOrWhiteSpace(party.InternalCode) && data.Parties.Any(x => x.Id != party.Id && x.InternalCode.Equals(party.InternalCode, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Ce code tiers existe déjà.");
            if (existing is null)
            {
                party.Id = Guid.NewGuid();
                party.InternalCode = string.IsNullOrWhiteSpace(party.InternalCode) ? NextPartyCode(party.Kind) : party.InternalCode.Trim().ToUpperInvariant();
                data.Parties.Add(party);
            }
            else CopyParty(party, existing);
            AddAudit("Party", party.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? party, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? party);
        }
    }

    public EnterpriseExemptionCertificate UpsertExemptionCertificate(EnterpriseExemptionCertificate certificate, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageTaxRules);
            if (certificate.EndDate < certificate.StartDate) throw new EnterpriseValidationException("La période du certificat est invalide.");
            if (certificate.AuthorizedAmountMad <= 0 || certificate.ConsumedAmountMad < 0 || certificate.ConsumedAmountMad > certificate.AuthorizedAmountMad)
                throw new EnterpriseValidationException("Les montants du certificat sont invalides.");
            if (!data.Parties.Any(x => x.Id == certificate.ClientId && x.Kind == PartyKind.Client)) throw new EnterpriseValidationException("Client du certificat introuvable.");
            certificate.Number = Required(certificate.Number, "Numéro du certificat");
            var existing = certificate.Id == Guid.Empty ? null : data.ExemptionCertificates.FirstOrDefault(x => x.Id == certificate.Id);
            if (data.ExemptionCertificates.Any(x => x.Id != certificate.Id && x.Number.Equals(certificate.Number, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Ce numéro de certificat existe déjà.");
            if (existing is null) { certificate.Id = Guid.NewGuid(); data.ExemptionCertificates.Add(certificate); }
            else CopyCertificate(certificate, existing);
            AddAudit("ExemptionCertificate", certificate.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? certificate, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? certificate);
        }
    }

    public EnterpriseContract UpsertContract(EnterpriseContract contract, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(contract);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageContracts);
            contract.Reference = Required(contract.Reference, "Référence contrat"); contract.Title = Required(contract.Title, "Objet du contrat");
            if (contract.EndDate < contract.StartDate) throw new EnterpriseValidationException("La période du contrat est invalide.");
            var existing = contract.Id == Guid.Empty ? null : data.Contracts.FirstOrDefault(x => x.Id == contract.Id);
            if (data.Contracts.Any(x => x.Id != contract.Id && x.Reference.Equals(contract.Reference, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Cette référence de contrat existe déjà.");
            if (existing is null) { contract.Id = Guid.NewGuid(); data.Contracts.Add(contract); }
            else CopyContract(contract, existing);
            RebuildCommitments(existing ?? contract);
            AddAudit("Contract", contract.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? contract, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? contract);
        }
    }

    public EnterpriseImportFile UpsertImportFile(EnterpriseImportFile importFile, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(importFile);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageImports);
            importFile.Reference = Required(importFile.Reference, "Référence dossier import");
            if (!data.Companies.Any(x => x.Id == importFile.CompanyId)) throw new EnterpriseValidationException("Société du dossier import introuvable.");
            if (!data.Parties.Any(x => x.Id == importFile.SupplierId && x.Kind == PartyKind.Supplier)) throw new EnterpriseValidationException("Fournisseur du dossier import introuvable.");
            importFile.TotalAcquisitionCostMad = Round(importFile.SupplierInvoiceMad + importFile.Costs.Sum(x => x.AmountMad));
            var existing = importFile.Id == Guid.Empty ? null : data.ImportFiles.FirstOrDefault(x => x.Id == importFile.Id);
            if (data.ImportFiles.Any(x => x.Id != importFile.Id && x.Reference.Equals(importFile.Reference, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Cette référence de dossier import existe déjà.");
            if (existing is null) { importFile.Id = Guid.NewGuid(); data.ImportFiles.Add(importFile); }
            else CopyImportFile(importFile, existing);
            AddAudit("ImportFile", importFile.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? importFile, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? importFile);
        }
    }

    public EnterpriseTaxRule UpsertTaxRule(EnterpriseTaxRule rule, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(rule);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageTaxRules);
            rule.Code = Required(rule.Code, "Code règle fiscale").ToUpperInvariant();
            if (rule.Rate < 0 || rule.Rate > 1) throw new EnterpriseValidationException("Le taux fiscal doit être compris entre 0 et 1.");
            var existing = rule.Id == Guid.Empty ? null : data.TaxRules.FirstOrDefault(x => x.Id == rule.Id);
            if (data.TaxRules.Any(x => x.Id != rule.Id && x.Code.Equals(rule.Code, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Ce code de règle fiscale existe déjà.");
            if (existing is null) { rule.Id = Guid.NewGuid(); data.TaxRules.Add(rule); }
            else CopyTaxRule(rule, existing);
            AddAudit("TaxRule", rule.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? rule, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? rule);
        }
    }

    public EnterpriseRole UpsertRole(EnterpriseRole role, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(role);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageSecurity);
            role.Name = Required(role.Name, "Nom du rôle");
            var existing = role.Id == Guid.Empty ? null : data.Roles.FirstOrDefault(x => x.Id == role.Id);
            if (data.Roles.Any(x => x.Id != role.Id && x.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Ce nom de rôle existe déjà.");
            if (existing is null) { role.Id = Guid.NewGuid(); data.Roles.Add(role); }
            else { existing.Name = role.Name; existing.Description = role.Description; existing.Permissions = role.Permissions.Distinct().ToList(); }
            AddAudit("Role", role.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, JsonSerializer.Serialize(existing ?? role, EnterpriseJson.Options), null, effectiveActor);
            SaveUnsafe(); return Clone(existing ?? role);
        }
    }

    public EnterpriseUser UpsertUser(EnterpriseUser user, EnterpriseActor? actor = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageSecurity);
            user.DisplayName = Required(user.DisplayName, "Nom utilisateur"); user.Email = Required(user.Email, "E-mail").ToLowerInvariant();
            if (user.RoleIds.Any(id => !data.Roles.Any(r => r.Id == id))) throw new EnterpriseValidationException("Un rôle sélectionné n'existe pas.");
            var existing = user.Id == Guid.Empty ? null : data.Users.FirstOrDefault(x => x.Id == user.Id);
            if (data.Users.Any(x => x.Id != user.Id && x.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))) throw new EnterpriseValidationException("Cette adresse e-mail existe déjà.");
            if (existing is null) { user.Id = Guid.NewGuid(); user.CreatedAt = clock(); data.Users.Add(user); }
            else CopyUser(user, existing);
            AddAudit("User", user.Id.ToString("D"), null, existing is null ? "Created" : "Updated", null, user.Email, null, effectiveActor);
            SaveUnsafe(); return SanitizeUser(existing ?? user);
        }
    }

    public void ConfigurePassword(Guid userId, string password, EnterpriseActor? actor = null)
    {
        ValidatePasswordPolicy(password);
        lock (sync)
        {
            var effectiveActor = ResolveActor(actor); DemandPermission(effectiveActor, EnterprisePermission.ManageSecurity);
            var user = data.Users.FirstOrDefault(x => x.Id == userId) ?? throw new EnterpriseValidationException("Utilisateur introuvable.");
            user.PasswordHash = HashPassword(password);
            user.MustChangePassword = false;
            AddAudit("User", user.Id.ToString("D"), null, "PasswordChanged", null, "[protected]", null, effectiveActor);
            SaveUnsafe();
        }
    }

    public bool HasConfiguredAdministrator
    {
        get
        {
            lock (sync)
            {
                var administratorRoleIds = data.Roles.Where(x => x.Name.Equals("Administrateur", StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToHashSet();
                return data.Users.Any(x => x.IsActive && !string.IsNullOrWhiteSpace(x.PasswordHash) && x.RoleIds.Any(administratorRoleIds.Contains));
            }
        }
    }

    public EnterpriseUser SetupFirstAdministrator(string email, string displayName, string password)
    {
        ValidatePasswordPolicy(password);
        lock (sync)
        {
            if (HasConfiguredAdministrator) throw new EnterpriseValidationException("Un administrateur configuré existe déjà.");
            var normalizedEmail = Required(email, "E-mail").ToLowerInvariant();
            var normalizedName = Required(displayName, "Nom utilisateur");
            var checkpoint = Checkpoint();
            try
            {
                var role = data.Roles.FirstOrDefault(x => x.Name.Equals("Administrateur", StringComparison.OrdinalIgnoreCase));
                if (role is null) { role = new EnterpriseRole { Id = Guid.NewGuid(), Name = "Administrateur", Description = "Administration complète" }; data.Roles.Add(role); }
                role.Permissions = Enum.GetValues<EnterprisePermission>().Where(x => x != EnterprisePermission.None).Distinct().ToList();
                var user = data.Users.FirstOrDefault(x => x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    ?? data.Users.FirstOrDefault(x => x.RoleIds.Contains(role.Id) && string.IsNullOrWhiteSpace(x.PasswordHash));
                if (user is null) { user = new EnterpriseUser { Id = Guid.NewGuid(), CreatedAt = clock() }; data.Users.Add(user); }
                user.Email = normalizedEmail; user.DisplayName = normalizedName; user.IsActive = true;
                user.RoleIds = new List<Guid> { role.Id }; user.CompanyIds = data.Companies.Select(x => x.Id).ToList(); user.PasswordHash = HashPassword(password); user.MustChangePassword = false;
                AddAudit("User", user.Id.ToString("D"), null, "FirstAdministratorConfigured", null, user.Email, null, EnterpriseActor.System); SaveUnsafe(); return SanitizeUser(user);
            }
            catch { data = checkpoint; throw; }
        }
    }

    public EnterpriseAuthenticationResult Authenticate(string email, string password)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        lock (sync)
        {
            var user = data.Users.FirstOrDefault(x => x.IsActive && x.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
            var valid = user is not null && VerifyPassword(password ?? string.Empty, user.PasswordHash);
            var loginActor = new EnterpriseActor(user?.Id, string.IsNullOrWhiteSpace(normalizedEmail) ? "Connexion inconnue" : normalizedEmail, "local");
            if (!valid)
            {
                AddAudit("Authentication", user?.Id.ToString("D") ?? "unknown", null, "LoginFailed", null, null, "Identifiants invalides", loginActor); SaveUnsafe();
                return new EnterpriseAuthenticationResult { Success = false, Message = "E-mail ou mot de passe incorrect." };
            }
            user!.LastLoginAt = clock();
            var actor = new EnterpriseActor(user.Id, user.DisplayName, "local");
            AddAudit("Authentication", user.Id.ToString("D"), null, "Authenticated", null, null, null, actor); SaveUnsafe();
            return new EnterpriseAuthenticationResult { Success = true, PasswordVerified = true, Message = "Authentification réussie.", User = SanitizeUser(user), Actor = actor };
        }
    }

    public IReadOnlyList<ExpirationAlert> GetExpirationAlerts(DateOnly? asOf = null)
    {
        lock (sync)
        {
            var date = asOf ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var alerts = new List<ExpirationAlert>();
            foreach (var certificate in data.ExemptionCertificates.Where(x => x.Status != CertificateStatus.Cancelled))
                AddExpirationAlert(alerts, "ExemptionCertificate", certificate.Id, certificate.Number, certificate.EndDate, date, "Nouvelle attestation requise");
            foreach (var contract in data.Contracts.Where(x => x.Status == ContractStatus.Active))
                AddExpirationAlert(alerts, "Contract", contract.Id, contract.Reference, contract.EndDate, date, "Renouvellement ou clôture à traiter");
            foreach (var commitment in data.Commitments.Where(x => x.Status == CommitmentStatus.Scheduled))
                AddExpirationAlert(alerts, "Commitment", commitment.Id, commitment.Reference, commitment.DueDate, date, "Échéance contractuelle à traiter");
            return alerts.OrderBy(x => x.DaysRemaining).ToArray();
        }
    }

    public bool VerifyAuditChain(out Guid? firstInvalidEntryId)
    {
        lock (sync)
        {
            string previous = string.Empty;
            foreach (var entry in data.AuditLog.OrderBy(x => x.Sequence))
            {
                var expected = ComputeAuditHash(entry.Id, entry.Sequence, entry.OccurredAt, entry.EntityType, entry.EntityId, entry.OperationId, entry.Action, entry.ActorUserId, entry.ActorDisplayName, entry.IpAddress, entry.BeforeJson, entry.AfterJson, entry.Reason, previous);
                if (!string.Equals(entry.PreviousHash, previous, StringComparison.Ordinal) || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(entry.Hash), Encoding.UTF8.GetBytes(expected)))
                { firstInvalidEntryId = entry.Id; return false; }
                previous = entry.Hash;
            }
            firstInvalidEntryId = null; return true;
        }
    }

    private CreateBusinessOperationRequest RequestFromJson(JsonElement payload)
    {
        var type = ParseOperationType(JsonText(payload, "operationType", "type") ?? "Autre");
        var companyId = ResolveCompanyId(JsonText(payload, "companyId", "company"));
        var partyId = ResolvePartyId(JsonText(payload, "partyId", "party", "client", "supplier"));
        var currency = JsonText(payload, "currency", "devise") ?? "MAD";
        var amount = JsonDecimal(payload, "amount", "montant", "amountExcludingTax") ?? 0;
        var vat = PercentRate(JsonDecimal(payload, "vatRate", "tva"));
        var withholding = PercentRate(JsonDecimal(payload, "withholdingRate", "ras"));
        var custom = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (payload.ValueKind == JsonValueKind.Object)
            foreach (var property in payload.EnumerateObject())
                if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                    custom[property.Name] = property.Value.ToString();
        var fileName = JsonText(payload, "documentName", "documentFileName");
        return new CreateBusinessOperationRequest
        {
            Type = type, Nature = JsonText(payload, "nature") ?? type.ToString(), CompanyId = companyId,
            SiteId = ResolveNamedId(JsonText(payload, "siteId", "site"), data.Sites, x => x.Id, x => new[] { x.Code, x.Name }),
            LaboratoryId = ResolveNamedId(JsonText(payload, "laboratoryId", "laboratory"), data.Laboratories, x => x.Id, x => new[] { x.Code, x.Name }),
            ActivityId = ResolveNamedId(JsonText(payload, "activityId", "activity"), data.Activities, x => x.Id, x => new[] { x.Code, x.Name }),
            CostCenterId = ResolveNamedId(JsonText(payload, "costCenterId", "costCenter"), data.CostCenters, x => x.Id, x => new[] { x.Code, x.Name }),
            ProjectId = ResolveNamedId(JsonText(payload, "projectId", "project"), data.Projects, x => x.Id, x => new[] { x.Code, x.Name }),
            PartyId = partyId, ContractId = ResolveNamedId(JsonText(payload, "contractId", "contract"), data.Contracts, x => x.Id, x => new[] { x.Reference, x.Title }),
            ImportFileId = ResolveNamedId(JsonText(payload, "importFileId", "importFile"), data.ImportFiles, x => x.Id, x => new[] { x.Reference }),
            OperationDate = JsonDate(payload, "operationDate", "date") ?? DateOnly.FromDateTime(clock().LocalDateTime),
            DueDate = JsonDate(payload, "dueDate"), PaymentTermDays = JsonInt(payload, "paymentTermDays", "paymentDays"),
            Currency = currency, Amount = amount, ExchangeRate = JsonDecimal(payload, "exchangeRate", "rate"),
            ExchangeRateDate = JsonDate(payload, "exchangeRateDate"), ExchangeRateSource = JsonText(payload, "exchangeRateSource"),
            SettlementExchangeRate = JsonDecimal(payload, "settlementExchangeRate"), BankFeesMad = JsonDecimal(payload, "bankFeesMad", "bankFees") ?? 0,
            VatRate = vat, WithholdingRate = withholding, ExemptionCertificateId = JsonGuid(payload, "exemptionCertificateId", "certificateId"),
            ExternalInvoiceNumber = JsonText(payload, "externalInvoiceNumber", "invoiceNumber"), SourceReference = JsonText(payload, "sourceReference", "reference"),
            Description = JsonText(payload, "description", "label"), DocumentType = JsonText(payload, "documentType") ?? "Justificatif",
            DocumentFileName = fileName, DocumentStorageKey = JsonText(payload, "documentStorageKey", "storageKey"),
            DocumentMimeType = JsonText(payload, "documentMimeType"), IdempotencyKey = JsonText(payload, "idempotencyKey", "requestId"), CustomFields = custom, Comment = JsonText(payload, "comment")
        };
    }

    private EnterpriseParty CreatePartyFromJson(JsonElement payload, PartyKind kind, EnterpriseActor actor)
    {
        var party = new EnterpriseParty
        {
            Kind = kind, Name = JsonText(payload, "name", "raisonSociale", "companyName", "label") ?? throw new EnterpriseValidationException("La raison sociale est obligatoire."),
            Ice = JsonText(payload, "ice", "taxIdentifier"), TaxId = JsonText(payload, "taxId", "if"), Email = JsonText(payload, "email"),
            BankIban = JsonText(payload, "iban", "rib"), Address = JsonText(payload, "address"), CountryCode = JsonText(payload, "countryCode", "country") ?? "MA",
            PaymentTermsDays = JsonInt(payload, "paymentTermsDays", "paymentDays", "terms") ?? 30, IsActive = true
        };
        return UpsertParty(party, actor);
    }

    private RegisterPaymentRequest PaymentRequestFromJson(JsonElement payload)
    {
        var operationId = JsonGuid(payload, "operationId") ?? throw new EnterpriseValidationException("Opération requise.");
        var operation = RequireOperation(operationId);
        var bankId = JsonGuid(payload, "bankAccountId") ?? data.BankAccounts.FirstOrDefault(x => x.CompanyId == operation.CompanyId && x.IsActive)?.Id ?? throw new EnterpriseValidationException("Compte bancaire requis.");
        return new RegisterPaymentRequest { OperationId = operationId, PaymentDate = JsonDate(payload, "paymentDate", "date") ?? DateOnly.FromDateTime(clock().LocalDateTime), Amount = JsonDecimal(payload, "amount") ?? 0, Currency = JsonText(payload, "currency"), ExchangeRate = JsonDecimal(payload, "exchangeRate"), BankAccountId = bankId, AllowCurrencyConversion = JsonBool(payload, "allowCurrencyConversion") ?? false, Method = JsonText(payload, "method") ?? "Virement", ExternalReference = JsonText(payload, "externalReference", "reference"), Comment = JsonText(payload, "comment") };
    }

    private EnterpriseContract CreateContractFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var company = ResolveCompanyId(JsonText(payload, "companyId", "company"));
        var party = ResolvePartyId(JsonText(payload, "partyId", "party"));
        var start = JsonDate(payload, "startDate") ?? DateOnly.FromDateTime(clock().LocalDateTime);
        return UpsertContract(new EnterpriseContract
        {
            Reference = JsonText(payload, "reference") ?? NextReference("CTR"), CompanyId = company, PartyId = party,
            Title = JsonText(payload, "title", "name", "label") ?? throw new EnterpriseValidationException("Le libellé du contrat est obligatoire."),
            Category = JsonText(payload, "category") ?? "Engagement", StartDate = start, EndDate = JsonDate(payload, "endDate") ?? start.AddYears(1),
            BaseAmountMad = JsonDecimal(payload, "amount", "baseAmountMad") ?? 0, FrequencyMonths = JsonInt(payload, "frequencyMonths") ?? 1,
            RevisionPercent = JsonDecimal(payload, "revisionPercent") ?? 0, RevisionEveryYears = JsonInt(payload, "revisionEveryYears") ?? 0,
            Status = ContractStatus.Active, DocumentStorageKey = JsonText(payload, "documentStorageKey"), SpecialTerms = JsonText(payload, "specialTerms")
        }, actor);
    }

    private EnterpriseImportFile CreateImportFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var company = ResolveCompanyId(JsonText(payload, "companyId", "company"));
        var supplier = ResolvePartyId(JsonText(payload, "supplierId", "supplier")) ?? data.Parties.FirstOrDefault(x => x.Kind == PartyKind.Supplier)?.Id
            ?? throw new EnterpriseValidationException("Fournisseur requis.");
        var amount = JsonDecimal(payload, "supplierInvoiceAmount", "invoiceValue", "amount") ?? 0;
        var currency = JsonText(payload, "currency", "devise") ?? "EUR";
        var date = JsonDate(payload, "openedDate", "date") ?? DateOnly.FromDateTime(clock().LocalDateTime);
        var rate = ResolveExchangeRate(currency, date, JsonDecimal(payload, "exchangeRate"));
        return UpsertImportFile(new EnterpriseImportFile
        {
            Reference = JsonText(payload, "reference") ?? NextReference("IMP"), CompanyId = company, SupplierId = supplier, OpenedDate = date,
            Currency = NormalizeCurrency(currency), SupplierInvoiceAmount = amount, SupplierInvoiceMad = Round(amount * rate),
            AllocationRule = JsonText(payload, "allocationRule") ?? "Valeur", Status = ImportFileStatus.InProgress
        }, actor);
    }

    private EnterpriseExemptionCertificate CreateCertificateFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var client = ResolvePartyId(JsonText(payload, "clientId", "client")) ?? data.Parties.FirstOrDefault(x => x.Kind == PartyKind.Client)?.Id
            ?? throw new EnterpriseValidationException("Client requis.");
        var start = JsonDate(payload, "startDate") ?? DateOnly.FromDateTime(clock().LocalDateTime);
        return UpsertExemptionCertificate(new EnterpriseExemptionCertificate
        {
            Number = JsonText(payload, "number", "reference") ?? throw new EnterpriseValidationException("Numéro de certificat obligatoire."),
            ClientId = client, IssueDate = JsonDate(payload, "issueDate") ?? start, StartDate = start, EndDate = JsonDate(payload, "endDate", "expirationDate") ?? start.AddYears(1),
            AuthorizedAmountMad = JsonDecimal(payload, "authorizedAmountMad", "amount", "allowed") ?? 0,
            ConsumedAmountMad = JsonDecimal(payload, "consumedAmountMad", "used") ?? 0, DocumentStorageKey = JsonText(payload, "documentStorageKey"), Status = CertificateStatus.Active
        }, actor);
    }

    private EnterpriseRole CreateRoleFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var permissions = JsonStrings(payload, "permissions").Select(ParsePermission).Where(x => x != EnterprisePermission.None).Distinct().ToList();
        if (permissions.Count == 0) permissions.Add(EnterprisePermission.View);
        return UpsertRole(new EnterpriseRole { Name = JsonText(payload, "name", "roleName", "label") ?? throw new EnterpriseValidationException("Nom du rôle obligatoire."), Description = JsonText(payload, "description"), Permissions = permissions }, actor);
    }

    private EnterpriseUser CreateUserFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var roleIds = JsonStrings(payload, "roleIds").Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).ToList();
        if (roleIds.Count == 0 && JsonGuid(payload, "roleId") is Guid roleId) roleIds.Add(roleId);
        if (roleIds.Count == 0 && JsonText(payload, "role", "roleName") is string roleName)
        {
            var role = data.Roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)); if (role is not null) roleIds.Add(role.Id);
        }
        if (roleIds.Count == 0) throw new EnterpriseValidationException("Au moins un rôle valide est requis.");
        var companyIds = JsonStrings(payload, "companyIds").Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).ToList();
        if (companyIds.Count == 0 && JsonGuid(payload, "companyId") is Guid companyId) companyIds.Add(companyId);
        if (companyIds.Count == 0 && JsonText(payload, "company") is string companyText) companyIds.Add(ResolveCompanyId(companyText));
        if (companyIds.Count == 0) throw new EnterpriseValidationException("Au moins une société autorisée est requise.");
        return UpsertUser(new EnterpriseUser { Email = JsonText(payload, "email") ?? throw new EnterpriseValidationException("E-mail requis."), DisplayName = JsonText(payload, "displayName", "name") ?? throw new EnterpriseValidationException("Nom utilisateur requis."), IsActive = JsonBool(payload, "isActive") ?? true, MustChangePassword = true, RoleIds = roleIds, CompanyIds = companyIds }, actor);
    }

    private object SetUserPasswordFromJson(JsonElement payload, EnterpriseActor actor)
    {
        var id = JsonGuid(payload, "userId", "id"); var email = JsonText(payload, "email");
        var user = (id.HasValue ? data.Users.FirstOrDefault(x => x.Id == id.Value) : null) ?? (!string.IsNullOrWhiteSpace(email) ? data.Users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) : null) ?? throw new EnterpriseValidationException("Utilisateur introuvable.");
        ConfigurePassword(user.Id, JsonText(payload, "password") ?? throw new EnterpriseValidationException("Mot de passe requis."), actor);
        return new { userId = user.Id, configured = true, secretReturned = false };
    }

    private EnterpriseImportFile SaveImportCosting(JsonElement payload, EnterpriseActor actor)
    {
        DemandPermission(actor, EnterprisePermission.ManageImports);
        var id = JsonGuid(payload, "importFileId", "id");
        var reference = JsonText(payload, "reference");
        var file = (id.HasValue ? data.ImportFiles.FirstOrDefault(x => x.Id == id) : null)
            ?? (!string.IsNullOrWhiteSpace(reference) ? data.ImportFiles.FirstOrDefault(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)) : null)
            ?? data.ImportFiles.FirstOrDefault(x => x.Status is ImportFileStatus.InProgress or ImportFileStatus.Draft)
            ?? throw new EnterpriseValidationException("Aucun dossier import actif.");
        var mapping = new[] { ("transport", "Transport"), ("insurance", "Assurance"), ("assurance", "Assurance"), ("transit", "Transit"), ("port", "Port"), ("duties", "Droits"), ("customs", "Droits et douane"), ("other", "Autres frais") };
        foreach (var (key, label) in mapping)
        {
            var amount = JsonDecimal(payload, key);
            if (!amount.HasValue) continue;
            var cost = file.Costs.FirstOrDefault(x => x.Kind.Equals(label, StringComparison.OrdinalIgnoreCase));
            if (cost is null) file.Costs.Add(new EnterpriseImportCost { Id = Guid.NewGuid(), Kind = label, AmountMad = Round(amount.Value) });
            else cost.AmountMad = Round(amount.Value);
        }
        file.SupplierInvoiceMad = JsonDecimal(payload, "invoice", "supplierInvoiceMad") ?? file.SupplierInvoiceMad;
        file.TotalAcquisitionCostMad = Round(file.SupplierInvoiceMad + file.Costs.Sum(x => x.AmountMad));
        AddAudit("ImportFile", file.Id.ToString("D"), null, "CostingValidated", null, JsonSerializer.Serialize(file.Costs, EnterpriseJson.Options), null, actor);
        SaveUnsafe(); return file;
    }

    private object ReconcileSelected(JsonElement payload, EnterpriseActor actor)
    {
        var identifiers = JsonStrings(payload, "ids"); var reconciled = new List<string>(); var unmatched = new List<string>();
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ReconcileBank);
            foreach (var identifier in identifiers)
            {
                var line = data.BankStatements.SelectMany(x => x.Lines).FirstOrDefault(x => x.Id.ToString("D").Equals(identifier, StringComparison.OrdinalIgnoreCase) || string.Equals(x.Reference, identifier, StringComparison.OrdinalIgnoreCase));
                if (line is null || line.ReconciliationStatus == ReconciliationStatus.Reconciled) { unmatched.Add(identifier); continue; }
                var statement = data.BankStatements.First(x => x.Lines.Any(candidate => candidate.Id == line.Id));
                var bankOperation = data.BankOperations.Where(x => x.ReconciliationStatus == ReconciliationStatus.Unreconciled && x.BankAccountId == statement.BankAccountId && Math.Abs(x.AmountMad - line.AmountMad) <= .01m && !data.Operations.Any(operation => operation.Id == x.OperationId && operation.Status == OperationLifecycle.Cancelled) && !data.Payments.Any(payment => payment.Id == x.PaymentId && payment.Status == PaymentStatus.Cancelled)).OrderBy(x => Math.Abs(x.ValueDate.DayNumber - line.ValueDate.DayNumber)).FirstOrDefault();
                if (bankOperation is null) { unmatched.Add(identifier); continue; }
                var request = new ReconcileBankRequest { BankOperationId = bankOperation.Id, StatementLineId = line.Id, AllowedDifferenceMad = .01m, Comment = "Rapprochement sélectionné depuis l'interface" };
                var item = ReconcileBankOperation(request, actor); reconciled.Add(item.Reference);
            }
            AddAudit("UIAction", "reconcile-selected", null, "Executed", null, JsonSerializer.Serialize(new { reconciled, unmatched }, EnterpriseJson.Options), null, actor);
            SaveUnsafe();
        }
        return new { reconciled, unmatched };
    }

    private EnterpriseRole SaveSecurity(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageSecurity);
            var roleId = JsonGuid(payload, "roleId"); var roleName = JsonText(payload, "role", "roleName");
            var role = (roleId.HasValue ? data.Roles.FirstOrDefault(x => x.Id == roleId) : null)
                ?? (!string.IsNullOrWhiteSpace(roleName) ? data.Roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)) : null)
                ?? data.Roles.FirstOrDefault(x => x.Name != "Administrateur") ?? data.Roles.First();
            var permissions = JsonStrings(payload, "permissions").Select(ParsePermission).Where(x => x != EnterprisePermission.None).Distinct().ToList();
            if (permissions.Count == 0) throw new EnterpriseValidationException("Sélectionnez au moins une permission.");
            var before = string.Join(',', role.Permissions); role.Permissions = permissions;
            data.Settings.UpdatedAt = clock(); data.Settings.UpdatedByUserId = actor.UserId;
            AddAudit("Role", role.Id.ToString("D"), null, "PermissionsChanged", before, string.Join(',', permissions), null, actor); SaveUnsafe(); return role;
        }
    }

    private object GenerateCommitments(EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageContracts);
            var before = data.Commitments.Count; foreach (var contract in data.Contracts.Where(x => x.Status == ContractStatus.Active)) RebuildCommitments(contract);
            var generated = data.Commitments.Count - before; AddAudit("Commitment", "batch", null, "Generated", null, generated.ToString(CultureInfo.InvariantCulture), null, actor); SaveUnsafe();
            return new { generated, active = data.Commitments.Count(x => x.Status == CommitmentStatus.Scheduled) };
        }
    }

    private EnterpriseCommissionRule SaveCommissionRule(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            var name = JsonText(payload, "name") ?? throw new EnterpriseValidationException("Libellé de règle requis.");
            var basis = JsonText(payload, "basis") ?? throw new EnterpriseValidationException("Assiette de commission requise.");
            var rate = JsonDecimal(payload, "rate") ?? throw new EnterpriseValidationException("Taux de commission requis.");
            var cap = JsonDecimal(payload, "capMad", "cap");
            if (rate <= 0 || rate > 100) throw new EnterpriseValidationException("Le taux de commission doit être compris entre 0 et 100 %.");
            if (cap is < 0) throw new EnterpriseValidationException("Le plafond de commission ne peut pas être négatif.");
            var effectiveFrom = JsonDate(payload, "effectiveFrom") ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var effectiveTo = JsonDate(payload, "effectiveTo");
            if (effectiveTo.HasValue && effectiveTo < effectiveFrom) throw new EnterpriseValidationException("La fin d'effet précède le début.");
            var record = JsonText(payload, "record", "id", "code");
            var existing = Guid.TryParse(record, out var id) ? data.CommissionRules.FirstOrDefault(x => x.Id == id) : data.CommissionRules.FirstOrDefault(x => x.Code.Equals(record, StringComparison.OrdinalIgnoreCase));
            var checkpoint = Checkpoint();
            try
            {
                var before = existing is null ? null : JsonSerializer.Serialize(existing, EnterpriseJson.Options);
                var rule = existing ?? new EnterpriseCommissionRule { Id = Guid.NewGuid(), Code = NextReference("COM-RULE"), CreatedAt = clock() };
                rule.Name = name; rule.Basis = basis; rule.Rate = Round(rate); rule.CapMad = cap.HasValue && cap.Value > 0 ? Round(cap.Value) : null;
                rule.EffectiveFrom = effectiveFrom; rule.EffectiveTo = effectiveTo; rule.IsActive = JsonBool(payload, "isActive") ?? true; rule.UpdatedAt = clock();
                if (existing is null) data.CommissionRules.Add(rule);
                AddAudit("CommissionRule", rule.Id.ToString("D"), null, existing is null ? "Created" : "Updated", before, JsonSerializer.Serialize(rule, EnterpriseJson.Options), null, actor);
                SaveUnsafe(); return Clone(rule);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object CalculateCommissions(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            var asOf = JsonDate(payload, "asOfDate", "date") ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var period = JsonText(payload, "period") ?? asOf.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var checkpoint = Checkpoint();
            try
            {
                var created = new List<EnterpriseCommissionEntry>();
                var rules = data.CommissionRules.Where(x => x.IsActive && x.EffectiveFrom <= asOf && (!x.EffectiveTo.HasValue || x.EffectiveTo >= asOf)).ToArray();
                foreach (var rule in rules)
                {
                    var usesCollections = NormalizeSearch(rule.Basis).Contains("ENCAISS");
                    var operations = data.Operations.Where(x => x.Type is BusinessOperationType.Vente or BusinessOperationType.Export && x.Status is not (OperationLifecycle.Draft or OperationLifecycle.Cancelled) && ActorCanAccessCompany(actor, x.CompanyId));
                    foreach (var operation in operations)
                    {
                        if (data.Commissions.Any(x => x.CommissionRuleId == rule.Id && x.SourceOperationId == operation.Id && x.Period == period)) continue;
                        var basisAmount = usesCollections
                            ? data.Payments.Where(x => x.OperationId == operation.Id && x.Status is PaymentStatus.Executed or PaymentStatus.Reconciled).Sum(x => x.AmountMad)
                            : operation.AmountMad;
                        if (basisAmount <= 0) continue;
                        var amount = Round(basisAmount * rule.Rate / 100m);
                        if (rule.CapMad.HasValue) amount = Math.Min(amount, rule.CapMad.Value);
                        var party = operation.PartyId.HasValue ? data.Parties.FirstOrDefault(x => x.Id == operation.PartyId) : null;
                        var entry = new EnterpriseCommissionEntry
                        {
                            Id = Guid.NewGuid(), Reference = NextReference("COM"), CommissionRuleId = rule.Id, SourceOperationId = operation.Id,
                            PartyId = operation.PartyId, BeneficiaryName = party?.Name ?? "Bénéficiaire à affecter", BasisLabel = rule.Basis,
                            BasisAmountMad = Round(basisAmount), Rate = rule.Rate, AmountMad = amount, Period = period, PeriodDate = asOf, Status = "Calculée", CalculatedAt = clock()
                        };
                        data.Commissions.Add(entry); created.Add(entry);
                        AddAudit("CommissionEntry", entry.Id.ToString("D"), operation.Id, "Calculated", null, JsonSerializer.Serialize(entry, EnterpriseJson.Options), null, actor);
                    }
                }
                AddAudit("CommissionBatch", period, null, "Calculated", null, JsonSerializer.Serialize(new { period, created = created.Count, activeRules = rules.Length }, EnterpriseJson.Options), null, actor);
                SaveUnsafe(); return new { period, created = created.Count, entries = Clone(created) };
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object RunDepreciation(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.PostAccounting);
            var asOf = JsonDate(payload, "asOfDate", "date") ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var checkpoint = Checkpoint();
            try
            {
                var generated = new List<EnterpriseDepreciationEntry>();
                var eligible = data.Operations.Where(x => x.Type == BusinessOperationType.Immobilisation && x.Status is not (OperationLifecycle.Draft or OperationLifecycle.Cancelled) && ActorCanAccessCompany(actor, x.CompanyId)).ToArray();
                foreach (var operation in eligible)
                {
                    var yearsText = operation.CustomFields.GetValueOrDefault("depreciationYears") ?? operation.CustomFields.GetValueOrDefault("durationYears");
                    if (!int.TryParse(yearsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var years) || years is < 1 or > 100) continue;
                    var serviceDateText = operation.CustomFields.GetValueOrDefault("serviceDate") ?? operation.CustomFields.GetValueOrDefault("commissioningDate");
                    var serviceDate = DateOnly.TryParse(serviceDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate) ? parsedDate : operation.OperationDate;
                    if (serviceDate > asOf) continue;
                    var asset = data.FixedAssets.FirstOrDefault(x => x.SourceOperationId == operation.Id);
                    if (asset is null)
                    {
                        asset = new EnterpriseFixedAsset
                        {
                            Id = Guid.NewGuid(), Reference = NextReference("IMM"), SourceOperationId = operation.Id, CompanyId = operation.CompanyId,
                            SiteId = operation.SiteId, CostCenterId = operation.CostCenterId, Name = operation.Nature, Category = operation.CustomFields.GetValueOrDefault("category") ?? operation.CustomFields.GetValueOrDefault("assetTreatment") ?? "Immobilisation",
                            AcquisitionDate = operation.OperationDate, ServiceDate = serviceDate, AcquisitionValueMad = operation.AmountMad, DurationYears = years,
                            DepreciationMethod = operation.CustomFields.GetValueOrDefault("depreciationMethod") ?? "Linéaire", AnnualDepreciationMad = Round(operation.AmountMad / years), NetBookValueMad = operation.AmountMad
                        };
                        data.FixedAssets.Add(asset);
                        AddAudit("FixedAsset", asset.Id.ToString("D"), operation.Id, "Registered", null, JsonSerializer.Serialize(asset, EnterpriseJson.Options), null, actor);
                    }
                    if (data.DepreciationEntries.Any(x => x.FixedAssetId == asset.Id && x.FiscalYear == asOf.Year)) continue;
                    var remaining = Round(asset.AcquisitionValueMad - asset.AccumulatedDepreciationMad);
                    if (remaining <= 0) { asset.Status = "Entièrement amortie"; continue; }
                    var amount = asset.AnnualDepreciationMad;
                    if (asOf.Year == serviceDate.Year) amount = Round(amount * (13 - serviceDate.Month) / 12m);
                    amount = Math.Min(remaining, amount);
                    if (amount <= 0) continue;
                    var accounting = new EnterpriseAccountingEntry
                    {
                        Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("OD-AMO"), EntryDate = asOf, JournalCode = "OD", Label = $"Dotation {asset.Reference} · {asOf.Year}", Status = AccountingEntryStatus.Draft,
                        Lines = new List<EnterpriseAccountingLine>
                        {
                            new() { AccountCode = "6193", Label = "Dotations aux amortissements", DebitMad = amount, Dimensions = DimensionsOf(operation) },
                            new() { AccountCode = "2835", Label = "Amortissements cumulés", CreditMad = amount, Dimensions = DimensionsOf(operation) }
                        }
                    };
                    data.AccountingEntries.Add(accounting);
                    var depreciation = new EnterpriseDepreciationEntry { Id = Guid.NewGuid(), Reference = NextReference("AMO"), FixedAssetId = asset.Id, SourceOperationId = operation.Id, FiscalYear = asOf.Year, EntryDate = asOf, AmountMad = amount, AccountingEntryId = accounting.Id, Status = "Calculée", CalculatedAt = clock() };
                    data.DepreciationEntries.Add(depreciation); generated.Add(depreciation);
                    asset.AccumulatedDepreciationMad = Round(asset.AccumulatedDepreciationMad + amount); asset.NetBookValueMad = Round(asset.AcquisitionValueMad - asset.AccumulatedDepreciationMad);
                    if (asset.NetBookValueMad <= 0) asset.Status = "Entièrement amortie";
                    AddAudit("DepreciationEntry", depreciation.Id.ToString("D"), operation.Id, "Calculated", null, JsonSerializer.Serialize(depreciation, EnterpriseJson.Options), null, actor);
                }
                AddAudit("DepreciationBatch", asOf.Year.ToString(CultureInfo.InvariantCulture), null, "Calculated", null, JsonSerializer.Serialize(new { fiscalYear = asOf.Year, generated = generated.Count }, EnterpriseJson.Options), null, actor);
                SaveUnsafe(); return new { fiscalYear = asOf.Year, generated = generated.Count, entries = Clone(generated) };
            }
            catch { data = checkpoint; throw; }
        }
    }

    private EnterpriseCashBox SaveCashBox(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            var name = JsonText(payload, "name") ?? throw new EnterpriseValidationException("Nom de caisse requis.");
            var companyId = JsonGuid(payload, "companyId") ?? ResolveCompanyId(JsonText(payload, "company"));
            DemandCompanyAccess(actor, companyId);
            var siteId = JsonGuid(payload, "siteId") ?? ResolveNamedId(JsonText(payload, "site"), data.Sites.Where(x => x.CompanyId == companyId), x => x.Id, x => new[] { x.Code, x.Name });
            if (siteId.HasValue && !data.Sites.Any(x => x.Id == siteId && x.CompanyId == companyId)) throw new EnterpriseValidationException("Le site n'appartient pas à la société.");
            var currency = NormalizeCurrency(JsonText(payload, "currency") ?? "MAD");
            var checkpoint = Checkpoint();
            try
            {
                var box = new EnterpriseCashBox { Id = Guid.NewGuid(), CompanyId = companyId, SiteId = siteId, Code = NextReference("CAI"), Name = name, Currency = currency, IsActive = true };
                data.CashBoxes.Add(box); AddAudit("CashBox", box.Id.ToString("D"), null, "Created", null, JsonSerializer.Serialize(box, EnterpriseJson.Options), null, actor); SaveUnsafe(); return Clone(box);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private EnterpriseCashMovement SaveCashMovement(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.CreateOperation);
            var boxId = JsonGuid(payload, "cashBoxId", "record") ?? throw new EnterpriseValidationException("Caisse requise.");
            var box = data.CashBoxes.FirstOrDefault(x => x.Id == boxId) ?? throw new EnterpriseValidationException("Caisse introuvable.");
            DemandCompanyAccess(actor, box.CompanyId);
            if (!box.IsActive) throw new EnterpriseValidationException("La caisse est fermée.");
            var amount = JsonDecimal(payload, "amount") ?? 0; if (amount <= 0) throw new EnterpriseValidationException("Montant de mouvement invalide.");
            var label = JsonText(payload, "label") ?? throw new EnterpriseValidationException("Justificatif ou libellé requis.");
            var kind = NormalizeSearch(JsonText(payload, "kind") ?? "Entrée");
            var direction = kind.Contains("SORTIE") || kind.Contains("DECAISS") ? TreasuryDirection.Outflow : TreasuryDirection.Inflow;
            var date = JsonDate(payload, "date", "movementDate") ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var rate = ResolveExchangeRate(box.Currency, date, null); var amountMad = Round(amount * rate);
            if (direction == TreasuryDirection.Outflow && box.BalanceMad < amountMad) throw new EnterpriseValidationException("Solde de caisse insuffisant.");
            var checkpoint = Checkpoint();
            try
            {
                var previousBalance = box.BalanceMad; box.BalanceMad = Round(box.BalanceMad + (direction == TreasuryDirection.Inflow ? amountMad : -amountMad));
                var entry = new EnterpriseAccountingEntry
                {
                    Id = Guid.NewGuid(), Reference = NextReference("OD-CAI"), EntryDate = date, JournalCode = "CAI", Label = label, Status = AccountingEntryStatus.Draft,
                    Lines = direction == TreasuryDirection.Inflow
                        ? new List<EnterpriseAccountingLine> { new() { AccountCode = "5161", Label = box.Name, DebitMad = amountMad, Dimensions = new EnterpriseAnalyticDimensions { CompanyId = box.CompanyId, SiteId = box.SiteId } }, new() { AccountCode = "7588", Label = label, CreditMad = amountMad, Dimensions = new EnterpriseAnalyticDimensions { CompanyId = box.CompanyId, SiteId = box.SiteId } } }
                        : new List<EnterpriseAccountingLine> { new() { AccountCode = "6588", Label = label, DebitMad = amountMad, Dimensions = new EnterpriseAnalyticDimensions { CompanyId = box.CompanyId, SiteId = box.SiteId } }, new() { AccountCode = "5161", Label = box.Name, CreditMad = amountMad, Dimensions = new EnterpriseAnalyticDimensions { CompanyId = box.CompanyId, SiteId = box.SiteId } } }
                };
                data.AccountingEntries.Add(entry);
                var movement = new EnterpriseCashMovement { Id = Guid.NewGuid(), Reference = NextReference("MVT-CAI"), CashBoxId = box.Id, CompanyId = box.CompanyId, MovementDate = date, Direction = direction, Amount = Round(amount), Currency = box.Currency, AmountMad = amountMad, Label = label, AccountingEntryId = entry.Id, CreatedAt = clock(), CreatedByUserId = actor.UserId };
                data.CashMovements.Add(movement);
                data.TreasuryMovements.Add(new EnterpriseTreasuryMovement { Id = Guid.NewGuid(), CashBoxId = box.Id, Reference = movement.Reference, MovementDate = date, Direction = direction, AmountMad = amountMad, Currency = box.Currency, Status = TreasuryMovementStatus.Executed, Label = label });
                AddAudit("CashMovement", movement.Id.ToString("D"), null, "Created", null, JsonSerializer.Serialize(movement, EnterpriseJson.Options), null, actor);
                AddAudit("CashBox", box.Id.ToString("D"), null, "BalanceChanged", previousBalance.ToString(CultureInfo.InvariantCulture), box.BalanceMad.ToString(CultureInfo.InvariantCulture), movement.Reference, actor);
                SaveUnsafe(); return Clone(movement);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private EnterpriseTaxRule SaveTaxRule(JsonElement payload, EnterpriseActor actor)
    {
        var kindText = NormalizeSearch(JsonText(payload, "kind") ?? "TVA");
        var kind = kindText.Contains("RAS") || kindText.Contains("RETENUE") ? TaxRuleKind.Withholding : kindText.Contains("DOUANE") ? TaxRuleKind.Customs : TaxRuleKind.Vat;
        var rate = PercentRate(JsonDecimal(payload, "rate")) ?? throw new EnterpriseValidationException("Taux fiscal requis.");
        var code = JsonText(payload, "code") ?? throw new EnterpriseValidationException("Code fiscal requis.");
        var name = JsonText(payload, "name") ?? throw new EnterpriseValidationException("Libellé fiscal requis.");
        var operationTypes = kind == TaxRuleKind.Withholding
            ? new List<BusinessOperationType> { BusinessOperationType.Achat, BusinessOperationType.Import }
            : new List<BusinessOperationType> { BusinessOperationType.Achat, BusinessOperationType.Vente, BusinessOperationType.Import, BusinessOperationType.Export, BusinessOperationType.Immobilisation, BusinessOperationType.NoteDeFrais };
        return UpsertTaxRule(new EnterpriseTaxRule { Id = JsonGuid(payload, "taxRuleId", "id") ?? Guid.NewGuid(), Code = code, Name = name, Kind = kind, Rate = rate, EffectiveFrom = JsonDate(payload, "effectiveFrom") ?? DateOnly.FromDateTime(clock().LocalDateTime), EffectiveTo = JsonDate(payload, "effectiveTo"), OperationTypes = operationTypes, Priority = JsonInt(payload, "priority") ?? 10, IsActive = JsonBool(payload, "isActive") ?? true }, actor);
    }

    private EnterpriseTaxRule ToggleTaxRule(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageTaxRules);
            var record = JsonText(payload, "taxRuleId", "record", "id") ?? throw new EnterpriseValidationException("Règle fiscale requise.");
            var rule = Guid.TryParse(record, out var id) ? data.TaxRules.FirstOrDefault(x => x.Id == id) : data.TaxRules.FirstOrDefault(x => x.Code.Equals(record, StringComparison.OrdinalIgnoreCase));
            if (rule is null) throw new EnterpriseValidationException("Règle fiscale introuvable.");
            var checkpoint = Checkpoint();
            try
            {
                var before = rule.IsActive; rule.IsActive = JsonBool(payload, "isActive", "enabled") ?? !rule.IsActive;
                AddAudit("TaxRule", rule.Id.ToString("D"), null, "ActivationChanged", before.ToString(), rule.IsActive.ToString(), null, actor); SaveUnsafe(); return Clone(rule);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object SubmitDocumentUpload(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageImports);
            if (!TryJsonProperty(payload, new[] { "files" }, out var node) || node.ValueKind != JsonValueKind.Array) throw new EnterpriseValidationException("Aucun document sélectionné.");
            var operationId = JsonGuid(payload, "operationId");
            if (operationId.HasValue && !data.Operations.Any(x => x.Id == operationId)) throw new EnterpriseValidationException("Opération liée introuvable.");
            var checkpoint = Checkpoint();
            try
            {
                var received = new List<EnterpriseDocument>();
                foreach (var file in node.EnumerateArray())
                {
                    var name = JsonText(file, "name") ?? throw new EnterpriseValidationException("Nom de fichier requis.");
                    var size = JsonLong(file, "size") ?? 0; if (size <= 0) throw new EnterpriseValidationException($"Le fichier {name} est vide.");
                    var mime = JsonText(file, "type") ?? "application/octet-stream";
                    var extension = Path.GetExtension(name).ToLowerInvariant();
                    if (extension is not (".pdf" or ".png" or ".jpg" or ".jpeg" or ".tif" or ".tiff")) throw new EnterpriseValidationException($"Format non pris en charge : {name}.");
                    var document = new EnterpriseDocument { Id = Guid.NewGuid(), OperationId = operationId, Reference = NextReference("DOC"), DocumentType = extension == ".pdf" ? "Document PDF" : "Image justificative", FileName = Path.GetFileName(name), MimeType = mime, FileSizeBytes = size, Status = DocumentStatus.Expected, OcrStatus = "En attente du transfert binaire", CreatedAt = clock(), UploadedByUserId = actor.UserId };
                    data.Documents.Add(document); received.Add(document);
                    AddAudit("Document", document.Id.ToString("D"), operationId, "IntakeRegistered", null, JsonSerializer.Serialize(document, EnterpriseJson.Options), "Métadonnées reçues; contenu binaire à transférer vers le stockage objet", actor);
                }
                SaveUnsafe(); return new { received = received.Count, documents = Clone(received), binaryTransferRequired = true };
            }
            catch { data = checkpoint; throw; }
        }
    }

    private EnterpriseDocument RunDocumentOcr(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageImports);
            var record = JsonText(payload, "documentId", "record", "id") ?? throw new EnterpriseValidationException("Document requis.");
            var document = Guid.TryParse(record, out var id) ? data.Documents.FirstOrDefault(x => x.Id == id) : data.Documents.FirstOrDefault(x => x.Reference.Equals(record, StringComparison.OrdinalIgnoreCase));
            if (document is null) throw new EnterpriseValidationException("Document introuvable.");
            if (document.Status is DocumentStatus.Archived or DocumentStatus.Rejected or DocumentStatus.Superseded) throw new EnterpriseValidationException("Ce document ne peut pas être envoyé à l'OCR.");
            var checkpoint = Checkpoint();
            try
            {
                var before = document.OcrStatus;
                document.OcrStatus = string.IsNullOrWhiteSpace(document.ObjectStorageKey) ? "OCR en attente du fichier binaire" : "OCR en file";
                document.OcrConfidence = null;
                AddAudit("Document", document.Id.ToString("D"), document.OperationId, "OcrQueued", before, document.OcrStatus, null, actor); SaveUnsafe(); return Clone(document);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object SaveMasterRecord(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            var domain = (JsonText(payload, "domain") ?? throw new EnterpriseValidationException("Domaine de référentiel requis.")).Trim();
            var normalizedDomain = NormalizeSearch(domain).Replace(" ", "", StringComparison.Ordinal);
            var code = JsonText(payload, "code") ?? throw new EnterpriseValidationException("Code interne requis.");
            var name = JsonText(payload, "name") ?? throw new EnterpriseValidationException("Libellé requis.");
            var active = !NormalizeSearch(JsonText(payload, "status") ?? "Actif").Contains("INACTIF");
            var companyId = normalizedDomain is "COMPANIES" or "ACTIVITIES" or "PARTIES" ? (Guid?)null : JsonGuid(payload, "companyId") ?? ResolveCompanyId(JsonText(payload, "company"));
            if (companyId.HasValue) DemandCompanyAccess(actor, companyId.Value);
            var checkpoint = Checkpoint();
            try
            {
                object entity;
                switch (normalizedDomain)
                {
                    case "COMPANIES":
                        var company = data.Companies.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (company is null) { company = new EnterpriseCompany { Id = Guid.NewGuid(), Code = code }; data.Companies.Add(company); }
                        company.Name = name; company.LegalName ??= name; company.IsActive = active; entity = company; break;
                    case "SITES":
                        var site = data.Sites.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (site is null) { site = new EnterpriseSite { Id = Guid.NewGuid(), Code = code, CompanyId = companyId!.Value }; data.Sites.Add(site); }
                        site.CompanyId = companyId!.Value; site.Name = name; site.IsActive = active; entity = site; break;
                    case "LABORATORIES":
                        var siteId = JsonGuid(payload, "siteId") ?? ResolveNamedId(JsonText(payload, "site"), data.Sites.Where(x => x.CompanyId == companyId), x => x.Id, x => new[] { x.Code, x.Name }) ?? data.Sites.FirstOrDefault(x => x.CompanyId == companyId && x.IsActive)?.Id ?? throw new EnterpriseValidationException("Créez d'abord un site pour cette société.");
                        var laboratory = data.Laboratories.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (laboratory is null) { laboratory = new EnterpriseLaboratory { Id = Guid.NewGuid(), Code = code }; data.Laboratories.Add(laboratory); }
                        laboratory.SiteId = siteId; laboratory.Name = name; laboratory.IsActive = active; entity = laboratory; break;
                    case "ACTIVITIES":
                        var activity = data.Activities.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (activity is null) { activity = new EnterpriseActivity { Id = Guid.NewGuid(), Code = code }; data.Activities.Add(activity); }
                        activity.Name = name; activity.IsActive = active; entity = activity; break;
                    case "COSTCENTERS":
                        var center = data.CostCenters.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (center is null) { center = new EnterpriseCostCenter { Id = Guid.NewGuid(), Code = code, CompanyId = companyId!.Value }; data.CostCenters.Add(center); }
                        center.CompanyId = companyId!.Value; center.Name = name; center.IsActive = active; entity = center; break;
                    case "PROJECTS":
                        var project = data.Projects.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (project is null) { project = new EnterpriseProject { Id = Guid.NewGuid(), Code = code, CompanyId = companyId!.Value }; data.Projects.Add(project); }
                        project.CompanyId = companyId!.Value; project.Name = name; project.IsActive = active; entity = project; break;
                    case "PARTIES":
                        var party = data.Parties.FirstOrDefault(x => x.InternalCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (party is null) { party = new EnterpriseParty { Id = Guid.NewGuid(), InternalCode = code, Kind = NormalizeSearch(code).StartsWith("FR") ? PartyKind.Supplier : PartyKind.Client }; data.Parties.Add(party); }
                        party.Name = name; party.IsActive = active; entity = party; break;
                    case "BANKACCOUNTS":
                        var account = data.BankAccounts.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.CompanyId == companyId);
                        if (account is null) { account = new EnterpriseBankAccount { Id = Guid.NewGuid(), CompanyId = companyId!.Value }; data.BankAccounts.Add(account); }
                        account.Name = name; account.BankName = JsonText(payload, "bankName") ?? name; account.Iban = JsonText(payload, "iban", "code") ?? code; account.Currency = NormalizeCurrency(JsonText(payload, "currency") ?? "MAD"); account.IsActive = active; entity = account; break;
                    case "CASHBOXES":
                        var box = data.CashBoxes.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                        if (box is null) { box = new EnterpriseCashBox { Id = Guid.NewGuid(), Code = code, CompanyId = companyId!.Value }; data.CashBoxes.Add(box); }
                        box.CompanyId = companyId!.Value; box.Name = name; box.Currency = NormalizeCurrency(JsonText(payload, "currency") ?? "MAD"); box.IsActive = active; entity = box; break;
                    default: throw new EnterpriseValidationException($"Domaine de référentiel non pris en charge : {domain}.");
                }
                var entityId = (Guid)(entity.GetType().GetProperty("Id")?.GetValue(entity) ?? Guid.Empty);
                AddAudit("MasterData", entityId.ToString("D"), null, "Upserted", null, JsonSerializer.Serialize(new { domain, entity }, EnterpriseJson.Options), null, actor); SaveUnsafe(); return Clone(entity);
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object SubmitMasterImport(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            if (!TryJsonProperty(payload, new[] { "files" }, out var node) || node.ValueKind != JsonValueKind.Array) throw new EnterpriseValidationException("Fichier de référentiel requis.");
            var domain = JsonText(payload, "domain") ?? "À sélectionner";
            var checkpoint = Checkpoint();
            try
            {
                var jobs = new List<EnterpriseMasterImportJob>();
                foreach (var file in node.EnumerateArray())
                {
                    var name = JsonText(file, "name") ?? throw new EnterpriseValidationException("Nom de fichier requis.");
                    var extension = Path.GetExtension(name).ToLowerInvariant(); if (extension is not (".csv" or ".xlsx")) throw new EnterpriseValidationException($"Format de référentiel non pris en charge : {name}.");
                    var size = JsonLong(file, "size") ?? 0; if (size <= 0) throw new EnterpriseValidationException($"Le fichier {name} est vide.");
                    var job = new EnterpriseMasterImportJob { Id = Guid.NewGuid(), Reference = NextReference("IMP-REF"), Domain = domain, FileName = Path.GetFileName(name), FileSizeBytes = size, MimeType = JsonText(file, "type") ?? "application/octet-stream", Status = "En attente du contenu", SubmittedAt = clock(), SubmittedByUserId = actor.UserId };
                    data.MasterImports.Add(job); jobs.Add(job); AddAudit("MasterImport", job.Id.ToString("D"), null, "Submitted", null, JsonSerializer.Serialize(job, EnterpriseJson.Options), "Import différé jusqu'au transfert du contenu", actor);
                }
                SaveUnsafe(); return new { submitted = jobs.Count, jobs = Clone(jobs), binaryTransferRequired = true };
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object RunAgedActions(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.ManageMasterData);
            var today = JsonDate(payload, "date", "asOfDate") ?? DateOnly.FromDateTime(clock().LocalDateTime);
            var sideText = NormalizeSearch(JsonText(payload, "side") ?? "");
            DueKind? side = sideText.Contains("CLIENT") || sideText.Contains("CUSTOMER") ? DueKind.Receivable : sideText.Contains("FOURN") || sideText.Contains("SUPPLIER") ? DueKind.Debt : null;
            var requestedParties = JsonStrings(payload, "partyIds").Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).ToHashSet();
            var checkpoint = Checkpoint();
            try
            {
                var actions = new List<EnterpriseCollectionAction>();
                var dues = data.DueItems.Where(x => x.Status is DueStatus.Open or DueStatus.PartiallyPaid && x.DueDate < today && IsFinanciallyActiveOperation(x.OperationId) && (!side.HasValue || x.Kind == side) && (requestedParties.Count == 0 || x.PartyId.HasValue && requestedParties.Contains(x.PartyId.Value)) && ActorCanAccessCompany(actor, x.CompanyId)).ToArray();
                foreach (var due in dues)
                {
                    if (data.CollectionActions.Any(x => x.DueItemId == due.Id && x.Status == "Planifiée")) continue;
                    var action = new EnterpriseCollectionAction { Id = Guid.NewGuid(), Reference = NextReference(due.Kind == DueKind.Receivable ? "REL" : "PAY-FU"), DueItemId = due.Id, OperationId = due.OperationId, PartyId = due.PartyId, Side = due.Kind, ActionType = due.Kind == DueKind.Receivable ? "Relance client" : "Suivi paiement fournisseur", DaysOverdue = today.DayNumber - due.DueDate.DayNumber, OutstandingMad = due.OutstandingMad, ScheduledDate = today, Status = "Planifiée", CreatedAt = clock(), CreatedByUserId = actor.UserId };
                    data.CollectionActions.Add(action); actions.Add(action); AddAudit("CollectionAction", action.Id.ToString("D"), due.OperationId, "Scheduled", null, JsonSerializer.Serialize(action, EnterpriseJson.Options), null, actor);
                }
                AddAudit("AgedActionBatch", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), null, "Scheduled", null, JsonSerializer.Serialize(new { scheduled = actions.Count, side = side?.ToString() ?? "Both" }, EnterpriseJson.Options), null, actor);
                SaveUnsafe(); return new { scheduled = actions.Count, actions = Clone(actions) };
            }
            catch { data = checkpoint; throw; }
        }
    }

    private object CheckCertificateBalance(JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.View);
            var id = JsonGuid(payload, "certificateId", "id"); var reference = JsonText(payload, "number", "reference");
            var certificate = (id.HasValue ? data.ExemptionCertificates.FirstOrDefault(x => x.Id == id) : null)
                ?? (!string.IsNullOrWhiteSpace(reference) ? data.ExemptionCertificates.FirstOrDefault(x => x.Number.Equals(reference, StringComparison.OrdinalIgnoreCase)) : null)
                ?? throw new EnterpriseValidationException("Certificat introuvable.");
            var amount = JsonDecimal(payload, "amount") ?? 0; if (amount <= 0) throw new EnterpriseValidationException("Montant à contrôler invalide.");
            var today = DateOnly.FromDateTime(clock().LocalDateTime); var active = certificate.Status == CertificateStatus.Active && today >= certificate.StartDate && today <= certificate.EndDate;
            var sufficient = active && certificate.RemainingAmountMad >= amount; var shortage = sufficient ? 0 : Math.Max(0, amount - certificate.RemainingAmountMad);
            var result = new { certificate.Id, certificate.Number, active, sufficient, availableMad = certificate.RemainingAmountMad, remainingAfterMad = sufficient ? certificate.RemainingAmountMad - amount : certificate.RemainingAmountMad, shortageMad = shortage };
            AddAudit("ExemptionCertificate", certificate.Id.ToString("D"), null, "BalanceChecked", null, JsonSerializer.Serialize(result, EnterpriseJson.Options), null, actor); SaveUnsafe(); return result;
        }
    }

    private object AuditIntegrityAction(EnterpriseActor actor)
    {
        lock (sync)
        {
            DemandPermission(actor, EnterprisePermission.View);
            var valid = VerifyAuditChain(out var invalid); var result = new { valid, firstInvalidEntryId = invalid, checkedEntries = data.AuditLog.Count };
            if (valid) { AddAudit("AuditChain", "global", null, "IntegrityChecked", null, JsonSerializer.Serialize(result, EnterpriseJson.Options), null, actor); SaveUnsafe(); }
            return result;
        }
    }

    private object RecordNoOpAction(string command, JsonElement payload, EnterpriseActor actor)
    {
        lock (sync)
        {
            var serialized = payload.ValueKind == JsonValueKind.Undefined ? "{}" : payload.GetRawText();
            AddAudit("UIAction", command, null, "ReadOnlyOrUnsupported", null, serialized, "Aucune mutation métier appliquée", actor); SaveUnsafe();
            return new { noOp = true, persistedAudit = true, action = command };
        }
    }

    private static void Check(EnterpriseAcceptanceReport report, string name, Func<string> assertion)
    {
        try { report.Checks.Add(new EnterpriseAcceptanceCheck { Name = name, Passed = true, Detail = assertion() }); }
        catch (Exception ex) { report.Checks.Add(new EnterpriseAcceptanceCheck { Name = name, Passed = false, Detail = ex.Message }); }
    }

    private Guid ResolveCompanyId(string? value)
    {
        if (Guid.TryParse(value, out var id) && data.Companies.Any(x => x.Id == id)) return id;
        if (!string.IsNullOrWhiteSpace(value))
        {
            var normalized = NormalizeSearch(value);
            var match = data.Companies.FirstOrDefault(x => normalized.Contains(NormalizeSearch(x.Code)) || normalized.Contains(NormalizeSearch(x.Name)));
            if (match is not null) return match.Id;
        }
        return data.Companies.FirstOrDefault(x => x.IsActive)?.Id ?? throw new EnterpriseValidationException("Aucune société active.");
    }

    private Guid? ResolvePartyId(string? value)
    {
        if (Guid.TryParse(value, out var id) && data.Parties.Any(x => x.Id == id)) return id;
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = NormalizeSearch(value);
        return data.Parties.FirstOrDefault(x => normalized.Contains(NormalizeSearch(x.InternalCode)) || normalized.Contains(NormalizeSearch(x.Name)))?.Id;
    }

    private Guid ResolveOperationId(JsonElement payload)
    {
        var id = JsonGuid(payload, "operationId", "id");
        if (id.HasValue && data.Operations.Any(x => x.Id == id.Value)) return id.Value;
        var reference = JsonText(payload, "operationReference", "reference", "record");
        var operation = !string.IsNullOrWhiteSpace(reference) ? data.Operations.FirstOrDefault(x => x.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase)) : null;
        return operation?.Id ?? throw new EnterpriseValidationException("Opération introuvable.");
    }

    private static Guid? ResolveNamedId<T>(string? value, IEnumerable<T> items, Func<T, Guid> id, Func<T, IEnumerable<string?>> terms)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Guid.TryParse(value, out var guid) && items.Any(x => id(x) == guid)) return guid;
        var normalized = NormalizeSearch(value);
        var item = items.FirstOrDefault(x => terms(x).Any(term => normalized.Contains(NormalizeSearch(term))));
        return item is null ? null : id(item);
    }

    private static BusinessOperationType ParseOperationType(string value)
    {
        var normalized = NormalizeSearch(value).Replace(" ", "", StringComparison.Ordinal).Replace("'", "", StringComparison.Ordinal);
        return normalized switch
        {
            "ACHAT" => BusinessOperationType.Achat, "VENTE" => BusinessOperationType.Vente, "ENCAISSEMENT" => BusinessOperationType.Encaissement,
            "DECAISSEMENT" => BusinessOperationType.Decaissement, "BANQUE" => BusinessOperationType.Banque, "CAISSE" => BusinessOperationType.Caisse,
            "IMPORT" => BusinessOperationType.Import, "EXPORT" => BusinessOperationType.Export, "IMMOBILISATION" => BusinessOperationType.Immobilisation,
            "PAIE" => BusinessOperationType.Paie, "FISCALITE" => BusinessOperationType.Fiscalite, "NOTEDEFRAIS" => BusinessOperationType.NoteDeFrais,
            "OPERATIONDIVERSE" => BusinessOperationType.OperationDiverse, _ => BusinessOperationType.Autre
        };
    }

    private static EnterprisePermission ParsePermission(string value)
    {
        var normalized = NormalizeSearch(value).Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
        return Enum.GetValues<EnterprisePermission>().FirstOrDefault(x => NormalizeSearch(x.ToString()).Replace(" ", "", StringComparison.Ordinal) == normalized);
    }

    private static string? JsonText(JsonElement element, params string[] names)
    {
        if (!TryJsonProperty(element, names, out var node)) return null;
        return node.ValueKind switch { JsonValueKind.String => Clean(node.GetString()), JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => node.ToString(), _ => null };
    }
    private static decimal? JsonDecimal(JsonElement element, params string[] names) { var text = JsonText(element, names); if (string.IsNullOrWhiteSpace(text)) return null; var cleaned = text.Replace(" ", "", StringComparison.Ordinal).Replace(',', '.'); return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null; }
    private static int? JsonInt(JsonElement element, params string[] names) { var value = JsonDecimal(element, names); return value.HasValue ? decimal.ToInt32(value.Value) : null; }
    private static long? JsonLong(JsonElement element, params string[] names) { var text = JsonText(element, names); return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null; }
    private static Guid? JsonGuid(JsonElement element, params string[] names) => Guid.TryParse(JsonText(element, names), out var value) ? value : null;
    private static DateOnly? JsonDate(JsonElement element, params string[] names) => DateOnly.TryParse(JsonText(element, names), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;
    private static bool? JsonBool(JsonElement element, params string[] names) { if (!TryJsonProperty(element, names, out var node)) return null; if (node.ValueKind is JsonValueKind.True or JsonValueKind.False) return node.GetBoolean(); return bool.TryParse(node.ToString(), out var value) ? value : null; }
    private static List<string> JsonStrings(JsonElement element, params string[] names) { if (!TryJsonProperty(element, names, out var node)) return new(); if (node.ValueKind == JsonValueKind.Array) return node.EnumerateArray().Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList(); var text = JsonText(element, names); return string.IsNullOrWhiteSpace(text) ? new() : text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(); }
    private static bool TryJsonProperty(JsonElement element, IEnumerable<string> names, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
            foreach (var property in element.EnumerateObject())
                if (names.Any(x => property.Name.Equals(x, StringComparison.OrdinalIgnoreCase))) { value = property.Value; return true; }
        value = default; return false;
    }
    private static decimal? PercentRate(decimal? value) => value.HasValue ? (value.Value > 1 ? value.Value / 100m : value.Value) : null;
    private static bool MutatingAction(string action) => action is "save-enterprise-operation" or "submit-operation" or "validate-operation" or "post-operation" or "cancel-operation" or "create-client" or "create-supplier" or "create-contract" or "create-import" or "create-certificate" or "create-role" or "create-user" or "set-user-password" or "save-import-costing" or "prepare-payment" or "approve-payment" or "cancel-payment" or "undo-reconciliation" or "reconcile-selected" or "save-security" or "resolve-sod" or "generate-commitments" or "save-commission-rule" or "calculate-commissions" or "run-depreciation" or "save-cash-box" or "save-cash-movement" or "save-tax-rule" or "toggle-tax-rule" or "submit-document-upload" or "run-document-ocr" or "save-master-record" or "submit-master-import" or "run-aged-actions";
    private static string ActionMessage(string action, object? result) => action switch { "save-enterprise-operation" => "Opération enregistrée et impacts générés.", "submit-operation" => "Opération soumise au contrôle.", "validate-operation" => "Opération validée.", "post-operation" => "Opération comptabilisée.", "cancel-operation" => "Opération annulée avec traçabilité.", "check-certificate-balance" => "Solde d'exonération vérifié.", "verify-audit-integrity" => "Intégrité de la chaîne d'audit vérifiée.", _ when result is not null => "Action exécutée et persistée.", _ => "Action traitée." };

    private BusinessOperation CreateOperationUnsafe(CreateBusinessOperationRequest request, EnterpriseActor actor, bool save)
    {
        ValidateOperationRequest(request);
        var operation = new BusinessOperation
        {
            Id = Guid.NewGuid(), Reference = NextReference(OperationPrefix(request.Type)), Type = request.Type,
            Direction = request.Direction ?? DefaultDirection(request.Type), Nature = request.Nature.Trim(), CompanyId = request.CompanyId,
            SiteId = request.SiteId, LaboratoryId = request.LaboratoryId, ActivityId = request.ActivityId, CostCenterId = request.CostCenterId,
            ProjectId = request.ProjectId, PartyId = request.PartyId, ContractId = request.ContractId, ImportFileId = request.ImportFileId,
            OperationDate = request.OperationDate, DueDate = ResolveDueDate(request), Currency = NormalizeCurrency(request.Currency),
            Amount = Round(request.Amount), ExchangeRate = ResolveExchangeRate(request.Currency, request.OperationDate, request.ExchangeRate),
            ExchangeRateDate = request.ExchangeRateDate ?? request.OperationDate, ExchangeRateSource = Clean(request.ExchangeRateSource),
            SettlementExchangeRate = request.SettlementExchangeRate, BankFeesMad = Round(request.BankFeesMad), SourceReference = Clean(request.SourceReference),
            Description = Clean(request.Description), Status = OperationLifecycle.Draft, CreatedAt = clock(), UpdatedAt = clock(), CreatedByUserId = actor.UserId,
            IdempotencyKey = Clean(request.IdempotencyKey),
            Tags = request.Tags.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            CustomFields = new Dictionary<string, string>(request.CustomFields, StringComparer.OrdinalIgnoreCase)
        };
        operation.AmountMad = Round(operation.Amount * operation.ExchangeRate);
        var tax = CalculateTax(request, operation);
        operation.VatRate = tax.VatRate; operation.VatAmountMad = tax.VatAmountMad; operation.WithholdingRate = tax.WithholdingRate;
        operation.WithholdingAmountMad = tax.WithholdingAmountMad; operation.TotalMad = tax.TotalMad; operation.NetPayableMad = tax.NetPayableMad;
        operation.ExemptionCertificateId = request.ExemptionCertificateId;
        operation.ExchangeDifferenceMad = operation.SettlementExchangeRate.HasValue ? Round(operation.Amount * (operation.SettlementExchangeRate.Value - operation.ExchangeRate)) : 0;
        data.Operations.Add(operation);
        GenerateAutomaticImpacts(operation, request, actor);
        AddAudit("BusinessOperation", operation.Id.ToString("D"), operation.Id, "Created", null, JsonSerializer.Serialize(operation, EnterpriseJson.Options), request.Comment, actor);
        if (save) SaveUnsafe();
        return operation;
    }

    private void GenerateAutomaticImpacts(BusinessOperation operation, CreateBusinessOperationRequest request, EnterpriseActor actor)
    {
        var commercial = IsCommercial(operation.Type);
        var hasDue = commercial && operation.NetPayableMad > 0;
        var now = clock();

        var document = new EnterpriseDocument
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("DOC"), DocumentType = request.DocumentType ?? "Justificatif",
            FileName = string.IsNullOrWhiteSpace(request.DocumentFileName) ? $"{operation.Reference}.pdf" : request.DocumentFileName.Trim(),
            ObjectStorageKey = Clean(request.DocumentStorageKey), MimeType = request.DocumentMimeType ?? "application/pdf", Status = string.IsNullOrWhiteSpace(request.DocumentStorageKey) ? DocumentStatus.Expected : DocumentStatus.Uploaded,
            OcrStatus = string.IsNullOrWhiteSpace(request.DocumentStorageKey) ? "En attente du document" : "À traiter", CreatedAt = now,
            UploadedAt = string.IsNullOrWhiteSpace(request.DocumentStorageKey) ? null : now, UploadedByUserId = actor.UserId
        };
        data.Documents.Add(document);
        AddImpact(operation.Id, ImpactKind.Document, document.Id, document.Reference, string.IsNullOrWhiteSpace(document.ObjectStorageKey) ? ImpactState.Pending : ImpactState.Generated, document.Status.ToString());

        EnterpriseInvoice? invoice = null;
        if (commercial)
        {
            invoice = new EnterpriseInvoice
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, CompanyId = operation.CompanyId, PartyId = operation.PartyId,
                Number = NextReference(operation.Direction == OperationDirection.Incoming ? "FV" : "FA"), ExternalNumber = Clean(request.ExternalInvoiceNumber),
                InvoiceDate = operation.OperationDate, DueDate = operation.DueDate!.Value, Currency = operation.Currency,
                AmountExcludingTaxMad = operation.AmountMad, VatMad = operation.VatAmountMad, WithholdingMad = operation.WithholdingAmountMad,
                TotalMad = operation.TotalMad, NetPayableMad = operation.NetPayableMad, Status = InvoiceStatus.Open,
                Lines = request.InvoiceLines.Count > 0 ? request.InvoiceLines.Select(x => new EnterpriseInvoiceLine
                {
                    Id = Guid.NewGuid(), Description = Required(x.Description, "Description ligne"), Quantity = x.Quantity,
                    UnitPriceMad = Round(x.UnitPriceMad), VatRate = x.VatRate, TotalMad = Round(x.Quantity * x.UnitPriceMad)
                }).ToList() : new List<EnterpriseInvoiceLine> { new() { Id = Guid.NewGuid(), Description = operation.Nature, Quantity = 1, UnitPriceMad = operation.AmountMad, VatRate = operation.VatRate, TotalMad = operation.AmountMad } }
            };
            data.Invoices.Add(invoice);
            AddImpact(operation.Id, ImpactKind.Invoice, invoice.Id, invoice.Number, ImpactState.Generated, invoice.Status.ToString());
        }
        else AddImpact(operation.Id, ImpactKind.Invoice, null, operation.Reference, ImpactState.NotApplicable, "Type d'opération sans facture");

        if (hasDue && invoice is not null)
        {
            var due = new EnterpriseDueItem
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, InvoiceId = invoice.Id, Reference = NextReference("ECH"), PartyId = operation.PartyId,
                CompanyId = operation.CompanyId, Kind = operation.Direction == OperationDirection.Incoming ? DueKind.Receivable : DueKind.Debt,
                DueDate = operation.DueDate!.Value, OriginalAmountMad = operation.NetPayableMad, OutstandingMad = operation.NetPayableMad,
                Currency = operation.Currency, Status = DueStatus.Open
            };
            data.DueItems.Add(due);
            AddImpact(operation.Id, due.Kind == DueKind.Receivable ? ImpactKind.Receivable : ImpactKind.Debt, due.Id, due.Reference, ImpactState.Active, due.Status.ToString());
            AddImpact(operation.Id, ImpactKind.AgedBalance, due.Id, due.Reference, ImpactState.Active, "Intégré jusqu'au règlement complet");
        }
        else
        {
            AddImpact(operation.Id, ImpactKind.Receivable, null, operation.Reference, ImpactState.NotApplicable, "Sans échéance");
            AddImpact(operation.Id, ImpactKind.AgedBalance, null, operation.Reference, ImpactState.NotApplicable, "Sans échéance");
        }

        var taxImpact = new EnterpriseTaxImpact
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("TAX"), TaxDate = operation.OperationDate,
            RuleCode = ResolveTaxRuleCode(request, operation), TaxableBaseMad = operation.AmountMad,
            OutputVatMad = operation.Direction == OperationDirection.Incoming ? operation.VatAmountMad : 0,
            InputVatMad = operation.Direction == OperationDirection.Outgoing ? operation.VatAmountMad : 0,
            WithholdingMad = operation.WithholdingAmountMad, ExemptionCertificateId = request.ExemptionCertificateId,
            Status = ImpactState.Generated
        };
        data.TaxImpacts.Add(taxImpact);
        AddImpact(operation.Id, ImpactKind.Tax, taxImpact.Id, taxImpact.Reference, ImpactState.Generated, taxImpact.RuleCode);

        var accounting = BuildAccountingEntry(operation);
        data.AccountingEntries.Add(accounting);
        AddImpact(operation.Id, ImpactKind.Accounting, accounting.Id, accounting.Reference, ImpactState.Pending, "À valider");

        var treasury = new EnterpriseTreasuryMovement
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("TRM"), MovementDate = operation.DueDate ?? operation.OperationDate,
            Direction = operation.Direction == OperationDirection.Incoming ? TreasuryDirection.Inflow : operation.Direction == OperationDirection.Outgoing ? TreasuryDirection.Outflow : TreasuryDirection.Neutral,
            AmountMad = operation.NetPayableMad, Currency = operation.Currency, Status = hasDue ? TreasuryMovementStatus.Forecast : TreasuryMovementStatus.Information,
            Label = $"Prévision · {operation.Reference}"
        };
        data.TreasuryMovements.Add(treasury);
        AddImpact(operation.Id, ImpactKind.Treasury, treasury.Id, treasury.Reference, hasDue ? ImpactState.Active : ImpactState.Generated, treasury.Status.ToString());

        if (IsDirectBankOperation(operation.Type))
        {
            var bank = data.BankAccounts.FirstOrDefault(x => x.CompanyId == operation.CompanyId && x.Currency == operation.Currency) ?? data.BankAccounts.FirstOrDefault(x => x.CompanyId == operation.CompanyId);
            var bankOperation = new EnterpriseBankOperation
            {
                Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("BOP"), BankAccountId = bank?.Id,
                OperationDate = operation.OperationDate, ValueDate = operation.OperationDate,
                AmountMad = operation.Direction == OperationDirection.Outgoing ? -operation.NetPayableMad : operation.NetPayableMad,
                Currency = operation.Currency, Label = operation.Nature, ReconciliationStatus = ReconciliationStatus.Unreconciled
            };
            data.BankOperations.Add(bankOperation);
            AddImpact(operation.Id, ImpactKind.Bank, bankOperation.Id, bankOperation.Reference, ImpactState.Pending, "À rapprocher");
        }
        else AddImpact(operation.Id, ImpactKind.Bank, null, operation.Reference, ImpactState.Pending, "Généré au paiement");

        var fact = new EnterpriseReportingFact
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, Date = operation.OperationDate, CompanyId = operation.CompanyId,
            SiteId = operation.SiteId, LaboratoryId = operation.LaboratoryId, ActivityId = operation.ActivityId, CostCenterId = operation.CostCenterId,
            ProjectId = operation.ProjectId, PartyId = operation.PartyId,
            RevenueMad = operation.Type is BusinessOperationType.Vente or BusinessOperationType.Export ? operation.AmountMad : 0,
            ExpenseMad = operation.Type is BusinessOperationType.Achat or BusinessOperationType.Import or BusinessOperationType.Immobilisation or BusinessOperationType.Paie or BusinessOperationType.NoteDeFrais ? operation.AmountMad : 0,
            TaxMad = operation.VatAmountMad + operation.WithholdingAmountMad,
            CashImpactMad = operation.Type is BusinessOperationType.Encaissement ? operation.NetPayableMad : operation.Type is BusinessOperationType.Decaissement ? -operation.NetPayableMad : 0,
            Status = ImpactState.Generated
        };
        data.ReportingFacts.Add(fact);
        AddImpact(operation.Id, ImpactKind.Reporting, fact.Id, operation.Reference, ImpactState.Generated, "Dimensions analytiques alimentées");
        AddImpact(operation.Id, ImpactKind.Audit, null, operation.Reference, ImpactState.Generated, "Chaîne d'audit active");
    }

    private TaxCalculation CalculateTax(CreateBusinessOperationRequest request, BusinessOperation operation)
    {
        var taxableCommercialOperation = IsCommercial(operation.Type);
        var exemption = request.ExemptionCertificateId.HasValue ? data.ExemptionCertificates.FirstOrDefault(x => x.Id == request.ExemptionCertificateId) : null;
        if (request.ExemptionCertificateId.HasValue && exemption is null) throw new EnterpriseValidationException("Certificat d'exonération introuvable.");
        decimal vatRate;
        if (exemption is not null)
        {
            if (!taxableCommercialOperation) throw new EnterpriseValidationException("Un certificat d'exonération ne peut être appliqué qu'à une facture commerciale.");
            if (string.IsNullOrWhiteSpace(exemption.DocumentStorageKey)) throw new EnterpriseValidationException("Le certificat d'exonération doit disposer d'un justificatif.");
            if (exemption.ClientId != operation.PartyId || exemption.Status != CertificateStatus.Active || operation.OperationDate < exemption.StartDate || operation.OperationDate > exemption.EndDate)
                throw new EnterpriseValidationException("Le certificat d'exonération n'est pas valide pour ce client et cette date.");
            if (exemption.RemainingAmountMad + 0.01m < operation.AmountMad)
                throw new EnterpriseValidationException($"Solde d'exonération insuffisant de {operation.AmountMad - exemption.RemainingAmountMad:N2} MAD.");
            vatRate = 0;
        }
        else vatRate = taxableCommercialOperation ? request.VatRate ?? ResolveTaxRate(TaxRuleKind.Vat, request.Type, request.Nature, request.OperationDate) : 0;

        var withholdingRate = taxableCommercialOperation ? request.WithholdingRate ?? ResolveTaxRate(TaxRuleKind.Withholding, request.Type, request.Nature, request.OperationDate) : 0;
        var vat = Round(operation.AmountMad * vatRate);
        var withholding = Round(operation.AmountMad * withholdingRate);
        var total = Round(operation.AmountMad + vat);
        var net = Round(total - withholding + request.BankFeesMad);
        return new TaxCalculation(vatRate, vat, withholdingRate, withholding, total, net);
    }

    private EnterpriseAccountingEntry BuildAccountingEntry(BusinessOperation operation)
    {
        var entry = new EnterpriseAccountingEntry
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, Reference = NextReference("ECR"), EntryDate = operation.OperationDate,
            JournalCode = operation.Direction == OperationDirection.Incoming ? "VTE" : operation.Direction == OperationDirection.Outgoing ? "ACH" : "OD",
            Label = $"{operation.Reference} · {operation.Nature}", Status = AccountingEntryStatus.Draft
        };
        var dimensions = DimensionsOf(operation);
        if (operation.Type is BusinessOperationType.Vente or BusinessOperationType.Export)
        {
            entry.Lines.Add(Line("3421", "Client", operation.NetPayableMad, 0, dimensions));
            entry.Lines.Add(Line("7111", "Produit", 0, operation.AmountMad, dimensions));
            if (operation.VatAmountMad > 0) entry.Lines.Add(Line("4455", "TVA facturée", 0, operation.VatAmountMad, dimensions));
            if (operation.WithholdingAmountMad > 0) entry.Lines.Add(Line("3458", "RAS à récupérer", operation.WithholdingAmountMad, 0, dimensions));
            BalanceEntry(entry);
        }
        else if (operation.Type is BusinessOperationType.Achat or BusinessOperationType.Import or BusinessOperationType.Immobilisation or BusinessOperationType.NoteDeFrais)
        {
            entry.Lines.Add(Line(operation.Type == BusinessOperationType.Immobilisation ? "2355" : "6111", "Charge / acquisition", operation.AmountMad, 0, dimensions));
            if (operation.VatAmountMad > 0) entry.Lines.Add(Line("3455", "TVA récupérable", operation.VatAmountMad, 0, dimensions));
            entry.Lines.Add(Line("4411", "Fournisseur", 0, operation.NetPayableMad, dimensions));
            if (operation.WithholdingAmountMad > 0) entry.Lines.Add(Line("4458", "RAS à payer", 0, operation.WithholdingAmountMad, dimensions));
            if (operation.BankFeesMad > 0) entry.Lines.Add(Line("6147", "Frais bancaires", operation.BankFeesMad, 0, dimensions));
            BalanceEntry(entry);
        }
        else
        {
            var amount = Math.Abs(operation.NetPayableMad);
            var incoming = operation.Direction == OperationDirection.Incoming;
            entry.Lines.Add(Line("5141", "Banque / caisse", incoming ? amount : 0, incoming ? 0 : amount, dimensions));
            entry.Lines.Add(Line("4711", "Compte d'attente", incoming ? 0 : amount, incoming ? amount : 0, dimensions));
        }
        return entry;
    }

    private void AddPaymentAccountingEntry(BusinessOperation operation, EnterprisePayment payment, bool incoming)
    {
        var dimensions = DimensionsOf(operation);
        var entry = new EnterpriseAccountingEntry
        {
            Id = Guid.NewGuid(), OperationId = operation.Id, PaymentId = payment.Id, Reference = NextReference("ECR"), EntryDate = payment.PaymentDate,
            JournalCode = "BQ", Label = $"Paiement {payment.Reference} · {operation.Reference}", Status = AccountingEntryStatus.Draft,
            Lines = new List<EnterpriseAccountingLine>
            {
                Line("5141", "Banque", incoming ? payment.AmountMad : 0, incoming ? 0 : payment.AmountMad, dimensions),
                Line(incoming ? "3421" : "4411", incoming ? "Client" : "Fournisseur", incoming ? 0 : payment.AmountMad, incoming ? payment.AmountMad : 0, dimensions)
            }
        };
        data.AccountingEntries.Add(entry);
    }

    private void ValidateOperationRequest(CreateBusinessOperationRequest request)
    {
        if (!Enum.IsDefined(request.Type)) throw new EnterpriseValidationException("Type d'opération invalide.");
        if (string.IsNullOrWhiteSpace(request.Nature)) throw new EnterpriseValidationException("La nature est obligatoire.");
        if (request.Amount <= 0) throw new EnterpriseValidationException("Le montant doit être supérieur à zéro.");
        if (!data.Companies.Any(x => x.Id == request.CompanyId && x.IsActive)) throw new EnterpriseValidationException("Société introuvable ou inactive.");
        if (request.PartyId.HasValue && !data.Parties.Any(x => x.Id == request.PartyId && x.IsActive)) throw new EnterpriseValidationException("Client ou fournisseur introuvable ou inactif.");
        if ((request.Type is BusinessOperationType.Vente or BusinessOperationType.Export) && (!request.PartyId.HasValue || data.Parties.First(x => x.Id == request.PartyId).Kind != PartyKind.Client))
            throw new EnterpriseValidationException("Une vente ou un export exige un client actif.");
        if ((request.Type is BusinessOperationType.Achat or BusinessOperationType.Import or BusinessOperationType.Immobilisation) && (!request.PartyId.HasValue || data.Parties.First(x => x.Id == request.PartyId).Kind != PartyKind.Supplier))
            throw new EnterpriseValidationException("Un achat, import ou immobilisation exige un fournisseur actif.");
        var requiredDirection = DefaultDirection(request.Type);
        if (requiredDirection != OperationDirection.Neutral && request.Direction.HasValue && request.Direction != requiredDirection)
            throw new EnterpriseValidationException($"Le sens {request.Direction} est incompatible avec le type {request.Type}.");
        ValidateDimension(request.SiteId, data.Sites.Select(x => x.Id), "site");
        ValidateDimension(request.LaboratoryId, data.Laboratories.Select(x => x.Id), "laboratoire");
        ValidateDimension(request.ActivityId, data.Activities.Select(x => x.Id), "activité");
        ValidateDimension(request.CostCenterId, data.CostCenters.Select(x => x.Id), "centre de coût");
        ValidateDimension(request.ProjectId, data.Projects.Select(x => x.Id), "projet");
        if (request.SiteId.HasValue && data.Sites.First(x => x.Id == request.SiteId).CompanyId != request.CompanyId) throw new EnterpriseValidationException("Le site n'appartient pas à la société sélectionnée.");
        if (request.LaboratoryId.HasValue)
        {
            var laboratory = data.Laboratories.First(x => x.Id == request.LaboratoryId);
            var laboratorySite = data.Sites.First(x => x.Id == laboratory.SiteId);
            if (laboratorySite.CompanyId != request.CompanyId || (request.SiteId.HasValue && laboratory.SiteId != request.SiteId)) throw new EnterpriseValidationException("Le laboratoire n'appartient pas au site et à la société sélectionnés.");
        }
        if (request.CostCenterId.HasValue && data.CostCenters.First(x => x.Id == request.CostCenterId).CompanyId != request.CompanyId) throw new EnterpriseValidationException("Le centre de coût n'appartient pas à la société sélectionnée.");
        if (request.ProjectId.HasValue && data.Projects.First(x => x.Id == request.ProjectId).CompanyId != request.CompanyId) throw new EnterpriseValidationException("Le projet n'appartient pas à la société sélectionnée.");
        if (request.ContractId.HasValue && !data.Contracts.Any(x => x.Id == request.ContractId && x.CompanyId == request.CompanyId)) throw new EnterpriseValidationException("Contrat introuvable pour cette société.");
        if (request.ImportFileId.HasValue && !data.ImportFiles.Any(x => x.Id == request.ImportFileId && x.CompanyId == request.CompanyId)) throw new EnterpriseValidationException("Dossier import introuvable pour cette société.");
        var currency = NormalizeCurrency(request.Currency);
        if (currency != "MAD" && !request.ExchangeRate.HasValue && !data.ExchangeRates.Any(x => x.Currency == currency && x.RateDate <= request.OperationDate))
            throw new EnterpriseValidationException($"Aucun cours de change n'est paramétré pour {currency}.");
        if (request.VatRate is < 0 or > 1 || request.WithholdingRate is < 0 or > 1) throw new EnterpriseValidationException("Les taux doivent être compris entre 0 et 1.");
        if (request.InvoiceLines.Any(x => string.IsNullOrWhiteSpace(x.Description) || x.Quantity <= 0 || x.UnitPriceMad < 0)) throw new EnterpriseValidationException("Une ligne de facture est invalide.");
    }

    private void SeedDemoData()
    {
        var today = DateOnly.FromDateTime(clock().LocalDateTime);
        var company = new EnterpriseCompany
        {
            Id = Guid.NewGuid(), Code = "KAY-LAB", Name = "KAY Laboratoires", LegalName = "KAY Laboratoires SARL",
            Ice = "001234567890123", TaxId = "12345678", TradeRegister = "AGD-45210", Cnss = "7845210", Address = "Agadir, Maroc",
            Rib = "011 780 0001234567890123 45", TaxRegime = "IS", VatSettings = "TVA mensuelle", IsActive = true
        };
        var company2 = new EnterpriseCompany
        {
            Id = Guid.NewGuid(), Code = "KAY-MED", Name = "KAY Medical", LegalName = "KAY Medical SARL", Ice = "002345678901234",
            TaxId = "23456789", TradeRegister = "CAS-82115", Cnss = "8956321", Address = "Casablanca, Maroc", Rib = "007 810 0009876543210123 11",
            TaxRegime = "IS", VatSettings = "TVA mensuelle", IsActive = true
        };
        data.Companies.AddRange(new[] { company, company2 });
        var site = new EnterpriseSite { Id = Guid.NewGuid(), CompanyId = company.Id, Code = "AGA-01", Name = "Site Agadir", City = "Agadir", IsActive = true };
        var site2 = new EnterpriseSite { Id = Guid.NewGuid(), CompanyId = company2.Id, Code = "CAS-01", Name = "Site Casablanca", City = "Casablanca", IsActive = true };
        data.Sites.AddRange(new[] { site, site2 });
        var lab = new EnterpriseLaboratory { Id = Guid.NewGuid(), SiteId = site.Id, Code = "LAB-ENV", Name = "Laboratoire Environnement", IsActive = true };
        data.Laboratories.Add(lab);
        var activity = new EnterpriseActivity { Id = Guid.NewGuid(), Code = "ANA", Name = "Analyses laboratoire", IsActive = true };
        var activity2 = new EnterpriseActivity { Id = Guid.NewGuid(), Code = "MED", Name = "Dispositifs médicaux", IsActive = true };
        data.Activities.AddRange(new[] { activity, activity2 });
        var costCenter = new EnterpriseCostCenter { Id = Guid.NewGuid(), CompanyId = company.Id, Code = "CC-LAB", Name = "Exploitation laboratoire", IsActive = true };
        data.CostCenters.Add(costCenter);
        data.Projects.Add(new EnterpriseProject { Id = Guid.NewGuid(), CompanyId = company.Id, Code = "PRJ-ISO", Name = "Accréditation ISO 17025", IsActive = true });

        var customer = new EnterpriseParty { Id = Guid.NewGuid(), InternalCode = "CL-000245", Kind = PartyKind.Client, Name = "Bio Santé", Ice = "003456789012345", TaxId = "34567890", AccountingAccount = "3421000245", PaymentTermsDays = 30, Risk = RiskLevel.Low, IsActive = true, Email = "finance@biosante.ma" };
        var customer2 = new EnterpriseParty { Id = Guid.NewGuid(), InternalCode = "CL-000192", Kind = PartyKind.Client, Name = "PharmaMed", Ice = "004567890123456", TaxId = "45678901", AccountingAccount = "3421000192", PaymentTermsDays = 60, Risk = RiskLevel.Medium, IsActive = true, Email = "compta@pharmamed.ma" };
        var supplier = new EnterpriseParty { Id = Guid.NewGuid(), InternalCode = "FR-000318", Kind = PartyKind.Supplier, Name = "Atlas Maintenance", Ice = "005678901234567", TaxId = "56789012", AccountingAccount = "4411000318", PaymentTermsDays = 45, Risk = RiskLevel.Low, IsActive = true, BankIban = "MA640115190000012345678901" };
        var foreignSupplier = new EnterpriseParty { Id = Guid.NewGuid(), InternalCode = "FR-000401", Kind = PartyKind.Supplier, Name = "EuroLab Systems", CountryCode = "FR", TaxId = "FR45812009911", AccountingAccount = "4411000401", PaymentTermsDays = 30, Risk = RiskLevel.Medium, IsActive = true, BankIban = "FR7630006000011234567890189" };
        data.Parties.AddRange(new[] { customer, customer2, supplier, foreignSupplier });

        data.TaxRules.AddRange(new[]
        {
            new EnterpriseTaxRule { Id = Guid.NewGuid(), Code = "TVA-20", Kind = TaxRuleKind.Vat, Name = "TVA taux normal", Rate = .20m, EffectiveFrom = new DateOnly(2024,1,1), RegulatoryReference = "CGI Maroc", Priority = 10, IsActive = true },
            new EnterpriseTaxRule { Id = Guid.NewGuid(), Code = "TVA-EXPORT-0", Kind = TaxRuleKind.Vat, Name = "Export exonéré", Rate = 0, EffectiveFrom = new DateOnly(2024,1,1), OperationTypes = new List<BusinessOperationType>{ BusinessOperationType.Export }, RegulatoryReference = "CGI Maroc — export", Priority = 100, IsActive = true },
            new EnterpriseTaxRule { Id = Guid.NewGuid(), Code = "RAS-SERVICE-ETR-10", Kind = TaxRuleKind.Withholding, Name = "RAS service étranger", Rate = .10m, EffectiveFrom = new DateOnly(2024,1,1), NatureContains = "étranger", RegulatoryReference = "Convention / CGI — paramétrable", Priority = 100, IsActive = true }
        });
        data.ExchangeRates.AddRange(new[]
        {
            new EnterpriseExchangeRate { Id = Guid.NewGuid(), Currency = "MAD", RateDate = today, RateToMad = 1, Source = "Monnaie locale" },
            new EnterpriseExchangeRate { Id = Guid.NewGuid(), Currency = "EUR", RateDate = today.AddDays(-10), RateToMad = 10.22m, Source = "Bank Al-Maghrib" },
            new EnterpriseExchangeRate { Id = Guid.NewGuid(), Currency = "USD", RateDate = today.AddDays(-10), RateToMad = 9.36m, Source = "Bank Al-Maghrib" }
        });
        var bank = new EnterpriseBankAccount { Id = Guid.NewGuid(), CompanyId = company.Id, Name = "Compte principal", BankName = "Attijariwafa bank", Iban = "MA640115190000012345678901", Currency = "MAD", BalanceMad = 845600, IsActive = true };
        var eurBank = new EnterpriseBankAccount { Id = Guid.NewGuid(), CompanyId = company.Id, Name = "Compte EUR", BankName = "Attijariwafa bank", Iban = "MA640115190000098765432109", Currency = "EUR", BalanceMad = 204400, IsActive = true };
        data.BankAccounts.AddRange(new[] { bank, eurBank });
        data.CashBoxes.Add(new EnterpriseCashBox { Id = Guid.NewGuid(), CompanyId = company.Id, SiteId = site.Id, Code = "CAI-AGA", Name = "Caisse Agadir", Currency = "MAD", BalanceMad = 18500, IsActive = true });

        var adminRole = new EnterpriseRole { Id = Guid.NewGuid(), Name = "Administrateur", Description = "Administration complète", Permissions = Enum.GetValues<EnterprisePermission>().Where(x => x != EnterprisePermission.None).ToList() };
        var accountantRole = new EnterpriseRole { Id = Guid.NewGuid(), Name = "Comptable", Description = "Saisie, fiscalité et comptabilisation", Permissions = new List<EnterprisePermission> { EnterprisePermission.View, EnterprisePermission.CreateOperation, EnterprisePermission.EditOperation, EnterprisePermission.SubmitOperation, EnterprisePermission.PostAccounting, EnterprisePermission.CreatePayment, EnterprisePermission.ReconcileBank, EnterprisePermission.ImportBankStatement } };
        data.Roles.AddRange(new[] { adminRole, accountantRole });
        var admin = new EnterpriseUser { Id = Guid.NewGuid(), Email = "admin@kayone.ma", DisplayName = "Administrateur KAY", IsActive = true, RoleIds = new List<Guid> { adminRole.Id }, CompanyIds = new List<Guid> { company.Id, company2.Id }, CreatedAt = clock(), MustChangePassword = true };
        var maker = new EnterpriseUser { Id = Guid.NewGuid(), Email = "comptable@kayone.ma", DisplayName = "Comptable KAY", IsActive = true, RoleIds = new List<Guid> { accountantRole.Id }, CompanyIds = new List<Guid> { company.Id }, CreatedAt = clock(), MustChangePassword = true };
        data.Users.AddRange(new[] { admin, maker });
        var systemActor = EnterpriseActor.System;

        var certificate = new EnterpriseExemptionCertificate { Id = Guid.NewGuid(), Number = "EXO-2026-018", ClientId = customer2.Id, IssueDate = today.AddMonths(-1), StartDate = today.AddMonths(-1), EndDate = today.AddMonths(11), AuthorizedAmountMad = 100000, ConsumedAmountMad = 82000, DocumentStorageKey = "certificates/EXO-2026-018.pdf", Status = CertificateStatus.Active };
        data.ExemptionCertificates.Add(certificate);
        var contract = new EnterpriseContract { Id = Guid.NewGuid(), Reference = "CTR-LOY-2026-01", CompanyId = company.Id, PartyId = supplier.Id, Title = "Maintenance laboratoire", Category = "Maintenance", StartDate = today.AddMonths(-6), EndDate = today.AddYears(3), BaseAmountMad = 3000, FrequencyMonths = 1, RevisionPercent = 10, RevisionEveryYears = 3, Status = ContractStatus.Active, DocumentStorageKey = "contracts/CTR-LOY-2026-01.pdf" };
        data.Contracts.Add(contract); RebuildCommitments(contract);

        var importFile = new EnterpriseImportFile { Id = Guid.NewGuid(), Reference = "IMP-2026-0012", CompanyId = company.Id, SupplierId = foreignSupplier.Id, OpenedDate = today.AddDays(-20), Currency = "EUR", SupplierInvoiceAmount = 1200, SupplierInvoiceMad = 12264, Status = ImportFileStatus.InProgress, Costs = new List<EnterpriseImportCost> { new() { Id = Guid.NewGuid(), Kind = "Transport", AmountMad = 10000 }, new() { Id = Guid.NewGuid(), Kind = "Assurance", AmountMad = 2000 }, new() { Id = Guid.NewGuid(), Kind = "Transit", AmountMad = 3000 }, new() { Id = Guid.NewGuid(), Kind = "Port", AmountMad = 2500 }, new() { Id = Guid.NewGuid(), Kind = "Droits", AmountMad = 18000 } } };
        importFile.TotalAcquisitionCostMad = importFile.SupplierInvoiceMad + importFile.Costs.Sum(x => x.AmountMad); data.ImportFiles.Add(importFile);

        CreateOperationUnsafe(new CreateBusinessOperationRequest { Type = BusinessOperationType.Vente, Nature = "Analyse laboratoire", CompanyId = company.Id, SiteId = site.Id, LaboratoryId = lab.Id, ActivityId = activity.Id, CostCenterId = costCenter.Id, PartyId = customer.Id, OperationDate = today.AddDays(-2), Currency = "MAD", Amount = 96800, ExternalInvoiceNumber = "INV-2026-00458", SourceReference = "CMD-2026-00318", Description = "Campagne d'analyses microbiologiques", PaymentTermDays = 30 }, systemActor, false);
        CreateOperationUnsafe(new CreateBusinessOperationRequest { Type = BusinessOperationType.Achat, Nature = "Service maintenance laboratoire", CompanyId = company.Id, SiteId = site.Id, LaboratoryId = lab.Id, ActivityId = activity.Id, CostCenterId = costCenter.Id, PartyId = supplier.Id, ContractId = contract.Id, OperationDate = today.AddDays(-4), Currency = "MAD", Amount = 42500, ExternalInvoiceNumber = "ATM-2026-122", PaymentTermDays = 45 }, systemActor, false);
        CreateOperationUnsafe(new CreateBusinessOperationRequest { Type = BusinessOperationType.Import, Nature = "Service étranger — installation équipement", CompanyId = company.Id, SiteId = site.Id, LaboratoryId = lab.Id, ActivityId = activity.Id, CostCenterId = costCenter.Id, PartyId = foreignSupplier.Id, ImportFileId = importFile.Id, OperationDate = today.AddDays(-10), Currency = "EUR", Amount = 1200, ExchangeRate = 10.22m, ExchangeRateSource = "Bank Al-Maghrib", BankFeesMad = 280, PaymentTermDays = 30 }, systemActor, false);
        var receipt = CreateOperationUnsafe(new CreateBusinessOperationRequest { Type = BusinessOperationType.Encaissement, Nature = "Règlement client", CompanyId = company.Id, SiteId = site.Id, PartyId = customer2.Id, OperationDate = today.AddDays(-6), Currency = "MAD", Amount = 120000, SourceReference = "VIR-882145" }, systemActor, false);
        receipt.Status = OperationLifecycle.Paid;

        var directBankOperation = data.BankOperations.FirstOrDefault(x => x.OperationId == receipt.Id);
        var statement = new EnterpriseBankStatement { Id = Guid.NewGuid(), Reference = "REL-2026-008", BankAccountId = bank.Id, PeriodStart = today.AddDays(-10), PeriodEnd = today, OpeningBalanceMad = 725600, ClosingBalanceMad = 845600, ImportedAt = clock(), Lines = new List<EnterpriseBankStatementLine> { new() { Id = Guid.NewGuid(), BookingDate = today.AddDays(-6), ValueDate = today.AddDays(-6), Label = "VIR PHARMAMED VIR-882145", Reference = "VIR-882145", AmountMad = 120000, ReconciliationStatus = ReconciliationStatus.Unreconciled } } };
        data.BankStatements.Add(statement);
        if (directBankOperation is not null)
        {
            var line = statement.Lines[0]; line.LinkedBankOperationId = directBankOperation.Id; line.ReconciliationStatus = ReconciliationStatus.Reconciled;
            directBankOperation.StatementLineId = line.Id; directBankOperation.ReconciliationStatus = ReconciliationStatus.Reconciled;
            data.Reconciliations.Add(new EnterpriseReconciliation { Id = Guid.NewGuid(), Reference = "RAP-2026-0001", BankOperationId = directBankOperation.Id, BankStatementId = statement.Id, StatementLineId = line.Id, OperationId = receipt.Id, DifferenceMad = 0, ReconciledAt = clock(), ReconciledByUserId = admin.Id, Comment = "Rapprochement automatique exact" });
            receipt.Status = OperationLifecycle.Reconciled;
        }
        AddAudit("System", "seed", null, "Seeded", null, "Données de démonstration KAY ONE", "Initialisation", systemActor);
        SaveUnsafe();
    }

    private void NormalizeDatabase()
    {
        data.SchemaVersion = EnterpriseDatabase.CurrentSchemaVersion;
        data.Sequences ??= new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        data.Companies ??= new(); data.Sites ??= new(); data.Laboratories ??= new(); data.Activities ??= new(); data.CostCenters ??= new(); data.Projects ??= new();
        data.Parties ??= new(); data.Operations ??= new(); data.ImpactTraces ??= new(); data.Documents ??= new(); data.Invoices ??= new(); data.DueItems ??= new(); data.Payments ??= new();
        data.TaxRules ??= new(); data.TaxImpacts ??= new(); data.ExchangeRates ??= new(); data.AccountingEntries ??= new(); data.TreasuryMovements ??= new();
        data.BankAccounts ??= new(); data.CashBoxes ??= new(); data.CashMovements ??= new(); data.BankStatements ??= new(); data.BankOperations ??= new(); data.Reconciliations ??= new();
        data.ExemptionCertificates ??= new(); data.Contracts ??= new(); data.Commitments ??= new(); data.ImportFiles ??= new(); data.CommissionRules ??= new(); data.Commissions ??= new(); data.FixedAssets ??= new(); data.DepreciationEntries ??= new(); data.MasterImports ??= new(); data.CollectionActions ??= new(); data.ReportingFacts ??= new();
        data.Roles ??= new(); data.Users ??= new(); data.AuditLog ??= new();
        data.Settings ??= new EnterpriseSettings();
        foreach (var document in data.Documents)
        {
            document.OcrExtractedFields ??= new(StringComparer.OrdinalIgnoreCase);
            if (document.CreatedAt == default) document.CreatedAt = document.UploadedAt ?? data.LastSavedAt;
            if (string.IsNullOrWhiteSpace(document.OcrStatus)) document.OcrStatus = document.Status == DocumentStatus.Uploaded ? "À traiter" : "En attente du document";
        }
    }

    private void SaveUnsafe() { data.LastSavedAt = clock(); store.Save(data); }
    private EnterpriseDatabase Checkpoint() => JsonSerializer.Deserialize<EnterpriseDatabase>(JsonSerializer.Serialize(data, EnterpriseJson.Options), EnterpriseJson.Options) ?? throw new InvalidOperationException("Impossible de créer le point de restauration transactionnel.");
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, EnterpriseJson.Options), EnterpriseJson.Options) ?? throw new InvalidOperationException("Impossible de copier la donnée demandée.");

    private void AddAudit(string entityType, string entityId, Guid? operationId, string action, string? before, string? after, string? reason, EnterpriseActor actor)
    {
        var previous = data.AuditLog.OrderByDescending(x => x.Sequence).FirstOrDefault();
        var entry = new EnterpriseAuditLog
        {
            Id = Guid.NewGuid(), Sequence = (previous?.Sequence ?? 0) + 1, OccurredAt = clock(), EntityType = entityType, EntityId = entityId,
            OperationId = operationId, Action = action, ActorUserId = actor.UserId, ActorDisplayName = actor.DisplayName,
            IpAddress = actor.IpAddress, BeforeJson = before, AfterJson = after, Reason = Clean(reason), PreviousHash = previous?.Hash ?? string.Empty
        };
        entry.Hash = ComputeAuditHash(entry.Id, entry.Sequence, entry.OccurredAt, entityType, entityId, operationId, action, entry.ActorUserId, entry.ActorDisplayName, entry.IpAddress, before, after, entry.Reason, entry.PreviousHash);
        data.AuditLog.Add(entry);
    }

    private static string ComputeAuditHash(Guid id, long sequence, DateTimeOffset occurredAt, string entityType, string entityId, Guid? operationId, string action, Guid? actorUserId, string actor, string? ipAddress, string? before, string? after, string? reason, string previousHash)
    {
        var canonical = string.Join('|', id.ToString("D"), sequence.ToString(CultureInfo.InvariantCulture), occurredAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), entityType, entityId, operationId?.ToString("D") ?? "", action, actorUserId?.ToString("D") ?? "", actor, ipAddress ?? "", before ?? "", after ?? "", reason ?? "", previousHash);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private EnterpriseActor ResolveActor(EnterpriseActor? actor) => actor ?? EnterpriseActor.System;

    private void DemandPermission(EnterpriseActor actor, EnterprisePermission permission)
    {
        if (actor.IsSystem || !store.EnforceAuthorization) return;
        var user = actor.UserId.HasValue ? data.Users.FirstOrDefault(x => x.Id == actor.UserId && x.IsActive) : null;
        if (user is null) throw new EnterpriseAuthorizationException("Utilisateur non authentifié ou inactif.");
        var allowed = data.Roles.Where(x => user.RoleIds.Contains(x.Id)).SelectMany(x => x.Permissions).Contains(permission);
        if (!allowed) throw new EnterpriseAuthorizationException($"Permission requise : {permission}.");
    }

    private void DemandCompanyAccess(EnterpriseActor actor, Guid companyId)
    {
        if (actor.IsSystem || !store.EnforceAuthorization) return;
        var user = actor.UserId.HasValue ? data.Users.FirstOrDefault(x => x.Id == actor.UserId && x.IsActive) : null;
        if (user is null || !user.CompanyIds.Contains(companyId)) throw new EnterpriseAuthorizationException("L'utilisateur n'est pas autorisé pour cette société.");
    }

    private bool ActorCanAccessCompany(EnterpriseActor actor, Guid companyId)
    {
        if (actor.IsSystem || !store.EnforceAuthorization) return true;
        return actor.UserId.HasValue && data.Users.Any(x => x.Id == actor.UserId.Value && x.IsActive && x.CompanyIds.Contains(companyId));
    }

    private bool HasPermission(EnterpriseActor actor, EnterprisePermission permission)
    {
        if (actor.IsSystem || !store.EnforceAuthorization) return true;
        var user = actor.UserId.HasValue ? data.Users.FirstOrDefault(x => x.Id == actor.UserId.Value && x.IsActive) : null;
        return user is not null && data.Roles.Where(x => user.RoleIds.Contains(x.Id)).SelectMany(x => x.Permissions).Contains(permission);
    }

    private void SupersedeImpacts(Guid operationId)
    {
        var operation = data.Operations.FirstOrDefault(x => x.Id == operationId);
        if (operation?.ExemptionCertificateId is Guid certificateId && operation.ExemptionConsumed)
        {
            var certificate = data.ExemptionCertificates.FirstOrDefault(x => x.Id == certificateId);
            if (certificate is not null) { certificate.ConsumedAmountMad = Round(Math.Max(0, certificate.ConsumedAmountMad - operation.AmountMad)); RefreshCertificateStatus(certificate); }
            operation.ExemptionConsumed = false;
        }
        foreach (var impact in data.ImpactTraces.Where(x => x.OperationId == operationId && x.State != ImpactState.Cancelled)) impact.State = ImpactState.Superseded;
        foreach (var invoice in data.Invoices.Where(x => x.OperationId == operationId && x.Status != InvoiceStatus.Cancelled)) invoice.Status = InvoiceStatus.Superseded;
        foreach (var due in data.DueItems.Where(x => x.OperationId == operationId && x.Status != DueStatus.Cancelled)) due.Status = DueStatus.Superseded;
        foreach (var tax in data.TaxImpacts.Where(x => x.OperationId == operationId && x.Status != ImpactState.Cancelled)) tax.Status = ImpactState.Superseded;
        foreach (var entry in data.AccountingEntries.Where(x => x.OperationId == operationId && x.Status == AccountingEntryStatus.Draft)) entry.Status = AccountingEntryStatus.Superseded;
        foreach (var movement in data.TreasuryMovements.Where(x => x.OperationId == operationId && x.Status != TreasuryMovementStatus.Cancelled)) movement.Status = TreasuryMovementStatus.Superseded;
        foreach (var fact in data.ReportingFacts.Where(x => x.OperationId == operationId && x.Status != ImpactState.Cancelled)) fact.Status = ImpactState.Superseded;
        foreach (var document in data.Documents.Where(x => x.OperationId == operationId && x.Status != DocumentStatus.Archived)) document.Status = DocumentStatus.Superseded;
        foreach (var bankOperation in data.BankOperations.Where(x => x.OperationId == operationId && x.ReconciliationStatus != ReconciliationStatus.Reconciled)) bankOperation.ReconciliationStatus = ReconciliationStatus.Rejected;
    }

    private CreateBusinessOperationRequest ToCreateRequest(BusinessOperation operation, UpdateBusinessOperationRequest update)
    {
        var invoice = data.Invoices.LastOrDefault(x => x.OperationId == operation.Id && x.Status != InvoiceStatus.Superseded);
        var document = data.Documents.LastOrDefault(x => x.OperationId == operation.Id && x.Status != DocumentStatus.Superseded);
        return new CreateBusinessOperationRequest
        {
        Type = update.Type ?? operation.Type, Direction = update.Direction ?? operation.Direction, Nature = update.Nature ?? operation.Nature,
        CompanyId = update.CompanyId ?? operation.CompanyId, SiteId = update.SiteId ?? operation.SiteId, LaboratoryId = update.LaboratoryId ?? operation.LaboratoryId,
        ActivityId = update.ActivityId ?? operation.ActivityId, CostCenterId = update.CostCenterId ?? operation.CostCenterId, ProjectId = update.ProjectId ?? operation.ProjectId,
        PartyId = update.PartyId ?? operation.PartyId, ContractId = update.ContractId ?? operation.ContractId, ImportFileId = update.ImportFileId ?? operation.ImportFileId,
        OperationDate = update.OperationDate ?? operation.OperationDate, DueDate = update.DueDate ?? operation.DueDate, Currency = update.Currency ?? operation.Currency,
        Amount = update.Amount ?? operation.Amount, ExchangeRate = update.ExchangeRate ?? operation.ExchangeRate, ExchangeRateDate = update.ExchangeRateDate ?? operation.ExchangeRateDate,
        ExchangeRateSource = update.ExchangeRateSource ?? operation.ExchangeRateSource, SettlementExchangeRate = update.SettlementExchangeRate ?? operation.SettlementExchangeRate,
        BankFeesMad = update.BankFeesMad ?? operation.BankFeesMad, VatRate = update.VatRate ?? operation.VatRate, WithholdingRate = update.WithholdingRate ?? operation.WithholdingRate,
        SourceReference = update.SourceReference ?? operation.SourceReference, Description = update.Description ?? operation.Description,
        ExemptionCertificateId = update.ExemptionCertificateId ?? operation.ExemptionCertificateId,
        ExternalInvoiceNumber = update.ExternalInvoiceNumber ?? invoice?.ExternalNumber,
        DocumentType = update.DocumentType ?? document?.DocumentType, DocumentFileName = update.DocumentFileName ?? document?.FileName,
        DocumentStorageKey = update.DocumentStorageKey ?? document?.ObjectStorageKey, DocumentMimeType = update.DocumentMimeType ?? document?.MimeType,
        InvoiceLines = update.InvoiceLines ?? invoice?.Lines.Select(x => new CreateInvoiceLineRequest { Description = x.Description, Quantity = x.Quantity, UnitPriceMad = x.UnitPriceMad, VatRate = x.VatRate }).ToList() ?? new(),
        IdempotencyKey = operation.IdempotencyKey, Tags = update.Tags ?? operation.Tags, CustomFields = update.CustomFields ?? operation.CustomFields, Comment = update.ChangeReason
        };
    }

    private void ApplyRequest(BusinessOperation operation, CreateBusinessOperationRequest request)
    {
        operation.Type = request.Type; operation.Direction = request.Direction ?? DefaultDirection(request.Type); operation.Nature = request.Nature.Trim();
        operation.CompanyId = request.CompanyId; operation.SiteId = request.SiteId; operation.LaboratoryId = request.LaboratoryId; operation.ActivityId = request.ActivityId;
        operation.CostCenterId = request.CostCenterId; operation.ProjectId = request.ProjectId; operation.PartyId = request.PartyId; operation.ContractId = request.ContractId;
        operation.ImportFileId = request.ImportFileId; operation.OperationDate = request.OperationDate; operation.DueDate = ResolveDueDate(request);
        operation.Currency = NormalizeCurrency(request.Currency); operation.Amount = Round(request.Amount); operation.ExchangeRate = ResolveExchangeRate(request.Currency, request.OperationDate, request.ExchangeRate);
        operation.AmountMad = Round(operation.Amount * operation.ExchangeRate); operation.ExchangeRateDate = request.ExchangeRateDate ?? request.OperationDate;
        operation.ExchangeRateSource = Clean(request.ExchangeRateSource); operation.SettlementExchangeRate = request.SettlementExchangeRate; operation.BankFeesMad = Round(request.BankFeesMad);
        operation.SourceReference = Clean(request.SourceReference); operation.Description = Clean(request.Description); operation.Tags = request.Tags.ToList();
        operation.ExemptionCertificateId = request.ExemptionCertificateId;
        operation.CustomFields = new Dictionary<string, string>(request.CustomFields, StringComparer.OrdinalIgnoreCase);
        var tax = CalculateTax(request, operation); operation.VatRate = tax.VatRate; operation.VatAmountMad = tax.VatAmountMad; operation.WithholdingRate = tax.WithholdingRate;
        operation.WithholdingAmountMad = tax.WithholdingAmountMad; operation.TotalMad = tax.TotalMad; operation.NetPayableMad = tax.NetPayableMad;
        operation.ExchangeDifferenceMad = operation.SettlementExchangeRate.HasValue ? Round(operation.Amount * (operation.SettlementExchangeRate.Value - operation.ExchangeRate)) : 0;
    }

    private void RebuildCommitments(EnterpriseContract contract)
    {
        foreach (var old in data.Commitments.Where(x => x.ContractId == contract.Id && x.Status == CommitmentStatus.Scheduled)) old.Status = CommitmentStatus.Superseded;
        if (contract.FrequencyMonths <= 0 || contract.BaseAmountMad <= 0) return;
        var due = contract.StartDate; var index = 0;
        while (due <= contract.EndDate && index < 600)
        {
            var elapsedYears = due.Year - contract.StartDate.Year;
            var revisions = contract.RevisionEveryYears > 0 ? elapsedYears / contract.RevisionEveryYears : 0;
            var amount = contract.BaseAmountMad * (decimal)Math.Pow(1 + (double)(contract.RevisionPercent / 100m), revisions);
            data.Commitments.Add(new EnterpriseCommitment { Id = Guid.NewGuid(), ContractId = contract.Id, Reference = NextReference("ENG"), DueDate = due, AmountMad = Round(amount), Status = CommitmentStatus.Scheduled, ClauseReference = revisions > 0 ? $"Révision {contract.RevisionPercent:N2}% · palier {revisions}" : "Montant initial" });
            due = due.AddMonths(contract.FrequencyMonths); index++;
        }
    }

    private static void AddExpirationAlert(List<ExpirationAlert> target, string kind, Guid id, string reference, DateOnly expiry, DateOnly today, string action)
    {
        var days = expiry.DayNumber - today.DayNumber;
        var threshold = days < 0 ? "Expired" : days <= 7 ? "J-7" : days <= 30 ? "J-30" : days <= 90 ? "J-90" : null;
        if (threshold is not null) target.Add(new ExpirationAlert { Kind = kind, EntityId = id, Reference = reference, ExpirationDate = expiry, DaysRemaining = days, Threshold = threshold, RequiredAction = action });
    }

    private void AddImpact(Guid operationId, ImpactKind kind, Guid? entityId, string reference, ImpactState state, string detail) => data.ImpactTraces.Add(new EnterpriseImpactTrace { Id = Guid.NewGuid(), OperationId = operationId, Kind = kind, GeneratedEntityId = entityId, Reference = reference, State = state, Detail = detail, CreatedAt = clock() });
    private void SetImpactState(Guid operationId, ImpactKind kind, ImpactState state) { foreach (var x in data.ImpactTraces.Where(x => x.OperationId == operationId && x.Kind == kind && x.State != ImpactState.Superseded)) x.State = state; }
    private BusinessOperation RequireOperation(Guid id) => data.Operations.FirstOrDefault(x => x.Id == id) ?? throw new EnterpriseValidationException("Opération introuvable.");
    private string NextReference(string prefix)
    {
        var year = clock().Year; var key = $"{prefix}-{year}"; data.Sequences.TryGetValue(key, out var value); string candidate;
        do { value++; candidate = $"{prefix}-{year}-{value:00000}"; } while (ReferenceExists(candidate));
        data.Sequences[key] = value; return candidate;
    }
    private bool ReferenceExists(string value) => data.Operations.Any(x => x.Reference == value) || data.Documents.Any(x => x.Reference == value) || data.Invoices.Any(x => x.Number == value) || data.DueItems.Any(x => x.Reference == value) || data.Payments.Any(x => x.Reference == value) || data.TaxImpacts.Any(x => x.Reference == value) || data.AccountingEntries.Any(x => x.Reference == value) || data.TreasuryMovements.Any(x => x.Reference == value) || data.BankOperations.Any(x => x.Reference == value) || data.BankStatements.Any(x => x.Reference == value) || data.Reconciliations.Any(x => x.Reference == value) || data.Contracts.Any(x => x.Reference == value) || data.Commitments.Any(x => x.Reference == value) || data.ImportFiles.Any(x => x.Reference == value) || data.CommissionRules.Any(x => x.Code == value) || data.Commissions.Any(x => x.Reference == value) || data.FixedAssets.Any(x => x.Reference == value) || data.DepreciationEntries.Any(x => x.Reference == value) || data.CashMovements.Any(x => x.Reference == value) || data.MasterImports.Any(x => x.Reference == value) || data.CollectionActions.Any(x => x.Reference == value);
    private string NextPartyCode(PartyKind kind) { var prefix = kind == PartyKind.Client ? "CL" : "FR"; var key = $"PARTY-{prefix}"; data.Sequences.TryGetValue(key, out var value); value = Math.Max(value + 1, kind == PartyKind.Client ? 246 : 319); data.Sequences[key] = value; return $"{prefix}-{value:000000}"; }
    private decimal ResolveExchangeRate(string currency, DateOnly date, decimal? explicitRate) { var code = NormalizeCurrency(currency); if (code == "MAD") return 1; if (explicitRate is > 0) return explicitRate.Value; return data.ExchangeRates.Where(x => x.Currency == code && x.RateDate <= date).OrderByDescending(x => x.RateDate).FirstOrDefault()?.RateToMad ?? throw new EnterpriseValidationException($"Cours {code}/MAD introuvable."); }
    private decimal ResolveTaxRate(TaxRuleKind kind, BusinessOperationType type, string nature, DateOnly date) => data.TaxRules.Where(x => x.IsActive && x.Kind == kind && x.EffectiveFrom <= date && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date) && (x.OperationTypes.Count == 0 || x.OperationTypes.Contains(type)) && (string.IsNullOrWhiteSpace(x.NatureContains) || nature.Contains(x.NatureContains, StringComparison.OrdinalIgnoreCase))).OrderByDescending(x => x.Priority).ThenByDescending(x => x.EffectiveFrom).FirstOrDefault()?.Rate ?? 0;
    private string ResolveTaxRuleCode(CreateBusinessOperationRequest request, BusinessOperation operation) { if (request.ExemptionCertificateId.HasValue) return "CERTIFICAT-EXONERATION"; return data.TaxRules.Where(x => x.IsActive && x.Kind == TaxRuleKind.Vat && x.EffectiveFrom <= operation.OperationDate && (!x.EffectiveTo.HasValue || x.EffectiveTo >= operation.OperationDate) && (x.OperationTypes.Count == 0 || x.OperationTypes.Contains(operation.Type))).OrderByDescending(x => x.Priority).FirstOrDefault()?.Code ?? "SANS-TVA"; }
    private DateOnly? ResolveDueDate(CreateBusinessOperationRequest request)
    {
        if (!IsCommercial(request.Type)) return request.DueDate;
        var negotiatedDays = request.PartyId.HasValue ? data.Parties.FirstOrDefault(x => x.Id == request.PartyId)?.PaymentTermsDays : null;
        return request.DueDate ?? request.OperationDate.AddDays(request.PaymentTermDays ?? negotiatedDays ?? 30);
    }
    private static string OperationPrefix(BusinessOperationType type) => type switch { BusinessOperationType.Achat => "ACH", BusinessOperationType.Vente => "VTE", BusinessOperationType.Encaissement => "ENC", BusinessOperationType.Decaissement => "DEC", BusinessOperationType.Banque => "BNQ", BusinessOperationType.Caisse => "CAI", BusinessOperationType.Import => "IMP", BusinessOperationType.Export => "EXP", BusinessOperationType.Immobilisation => "IMM", BusinessOperationType.Paie => "PAI", BusinessOperationType.Fiscalite => "FIS", BusinessOperationType.NoteDeFrais => "NDF", BusinessOperationType.OperationDiverse => "OD", _ => "AUT" };
    private static OperationDirection DefaultDirection(BusinessOperationType type) => type switch { BusinessOperationType.Vente or BusinessOperationType.Export or BusinessOperationType.Encaissement => OperationDirection.Incoming, BusinessOperationType.Achat or BusinessOperationType.Import or BusinessOperationType.Immobilisation or BusinessOperationType.Paie or BusinessOperationType.Fiscalite or BusinessOperationType.NoteDeFrais or BusinessOperationType.Decaissement => OperationDirection.Outgoing, _ => OperationDirection.Neutral };
    private static bool IsCommercial(BusinessOperationType type) => type is BusinessOperationType.Achat or BusinessOperationType.Vente or BusinessOperationType.Import or BusinessOperationType.Export or BusinessOperationType.Immobilisation or BusinessOperationType.NoteDeFrais;
    private static bool IsDirectBankOperation(BusinessOperationType type) => type is BusinessOperationType.Encaissement or BusinessOperationType.Decaissement or BusinessOperationType.Banque or BusinessOperationType.Caisse;
    private bool IsCurrentFact(EnterpriseReportingFact fact) => fact.Status is not (ImpactState.Cancelled or ImpactState.Superseded) && IsFinanciallyActiveOperation(fact.OperationId);
    private bool IsFinanciallyActiveOperation(Guid operationId) => data.Operations.FirstOrDefault(x => x.Id == operationId)?.Status is OperationLifecycle.Validated or OperationLifecycle.PaymentPending or OperationLifecycle.PartiallyPaid or OperationLifecycle.Paid or OperationLifecycle.Reconciled or OperationLifecycle.Posted;
    private void RefreshCertificateStatus(EnterpriseExemptionCertificate certificate)
    {
        if (certificate.Status is CertificateStatus.Cancelled or CertificateStatus.Draft) return;
        var today = DateOnly.FromDateTime(clock().LocalDateTime);
        certificate.Status = today > certificate.EndDate ? CertificateStatus.Expired
            : certificate.RemainingAmountMad <= .01m ? CertificateStatus.Exhausted
            : CertificateStatus.Active;
    }
    private void ConsumeExemptionOnValidation(BusinessOperation operation)
    {
        if (!operation.ExemptionCertificateId.HasValue || operation.ExemptionConsumed) return;
        var certificate = data.ExemptionCertificates.FirstOrDefault(x => x.Id == operation.ExemptionCertificateId.Value) ?? throw new EnterpriseValidationException("Certificat d'exonération introuvable.");
        if (string.IsNullOrWhiteSpace(certificate.DocumentStorageKey) || certificate.ClientId != operation.PartyId || certificate.Status != CertificateStatus.Active || operation.OperationDate < certificate.StartDate || operation.OperationDate > certificate.EndDate)
            throw new EnterpriseValidationException("Le certificat d'exonération n'est plus valide au moment de la validation.");
        if (certificate.RemainingAmountMad + .01m < operation.AmountMad) throw new EnterpriseValidationException($"Solde d'exonération insuffisant de {operation.AmountMad - certificate.RemainingAmountMad:N2} MAD.");
        certificate.ConsumedAmountMad = Round(certificate.ConsumedAmountMad + operation.AmountMad); RefreshCertificateStatus(certificate); operation.ExemptionConsumed = true;
    }
    private static void EnsureTransition(OperationLifecycle current, OperationLifecycle target) { var valid = (current, target) switch { (OperationLifecycle.Draft, OperationLifecycle.Submitted) => true, (OperationLifecycle.Submitted, OperationLifecycle.Validated) => true, (OperationLifecycle.Validated, OperationLifecycle.Posted) => true, (OperationLifecycle.Paid, OperationLifecycle.Reconciled) => true, (OperationLifecycle.Reconciled, OperationLifecycle.Posted) => true, _ => false }; if (!valid) throw new EnterpriseValidationException($"Transition interdite : {current} → {target}."); }
    private static EnterpriseAccountingLine Line(string account, string label, decimal debit, decimal credit, EnterpriseAnalyticDimensions dimensions) => new() { AccountCode = account, Label = label, DebitMad = Round(debit), CreditMad = Round(credit), Dimensions = dimensions };
    private static void BalanceEntry(EnterpriseAccountingEntry entry) { var difference = Round(entry.Lines.Sum(x => x.DebitMad) - entry.Lines.Sum(x => x.CreditMad)); if (difference > 0) entry.Lines.Add(Line("4711", "Équilibrage automatique", 0, difference, entry.Lines[0].Dimensions)); else if (difference < 0) entry.Lines.Add(Line("4711", "Équilibrage automatique", -difference, 0, entry.Lines[0].Dimensions)); }
    private static EnterpriseAnalyticDimensions DimensionsOf(BusinessOperation x) => new() { CompanyId = x.CompanyId, SiteId = x.SiteId, LaboratoryId = x.LaboratoryId, ActivityId = x.ActivityId, CostCenterId = x.CostCenterId, ProjectId = x.ProjectId, PartyId = x.PartyId };
    private AgedBalanceSummary BuildAgedSummary(DueKind kind, DateOnly today) { var summary = new AgedBalanceSummary { Kind = kind }; foreach (var x in data.DueItems.Where(x => x.Kind == kind && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId))) { var days = today.DayNumber - x.DueDate.DayNumber; if (days <= 0) summary.NotDueMad += x.OutstandingMad; else if (days <= 30) summary.Days1To30Mad += x.OutstandingMad; else if (days <= 60) summary.Days31To60Mad += x.OutstandingMad; else if (days <= 90) summary.Days61To90Mad += x.OutstandingMad; else summary.Over90DaysMad += x.OutstandingMad; } summary.RoundValues(); return summary; }
    private AgedBalanceSummary BuildAgedSummary(IEnumerable<EnterpriseDueItem> dues, DueKind kind, DateOnly today, HashSet<Guid> operationIds) { var summary = new AgedBalanceSummary { Kind = kind }; foreach (var x in dues.Where(x => operationIds.Contains(x.OperationId) && x.Kind == kind && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId))) { var days = today.DayNumber - x.DueDate.DayNumber; if (days <= 0) summary.NotDueMad += x.OutstandingMad; else if (days <= 30) summary.Days1To30Mad += x.OutstandingMad; else if (days <= 60) summary.Days31To60Mad += x.OutstandingMad; else if (days <= 90) summary.Days61To90Mad += x.OutstandingMad; else summary.Over90DaysMad += x.OutstandingMad; } summary.RoundValues(); return summary; }
    private EnterpriseDashboardMetrics BuildScopedMetrics(EnterpriseSnapshot snapshot, HashSet<Guid> operationIds)
    {
        var facts = snapshot.ReportingFacts.Where(x => x.Status is not (ImpactState.Cancelled or ImpactState.Superseded) && operationIds.Contains(x.OperationId) && IsFinanciallyActiveOperation(x.OperationId)).ToArray();
        return new EnterpriseDashboardMetrics
        {
            RevenueMad = Round(facts.Sum(x => x.RevenueMad)), ExpensesMad = Round(facts.Sum(x => x.ExpenseMad)), MarginMad = Round(facts.Sum(x => x.RevenueMad - x.ExpenseMad)),
            CashBalanceMad = Round(snapshot.BankAccounts.Sum(x => x.BalanceMad) + snapshot.CashBoxes.Sum(x => x.BalanceMad)),
            ReceivablesMad = Round(snapshot.DueItems.Where(x => x.Kind == DueKind.Receivable && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutstandingMad)),
            PayablesMad = Round(snapshot.DueItems.Where(x => x.Kind == DueKind.Debt && x.Status is DueStatus.Open or DueStatus.PartiallyPaid).Where(x => IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutstandingMad)),
            VatPayableMad = Round(snapshot.TaxImpacts.Where(x => x.Status is not (ImpactState.Cancelled or ImpactState.Superseded) && IsFinanciallyActiveOperation(x.OperationId)).Sum(x => x.OutputVatMad - x.InputVatMad)),
            OverdueMad = Round(snapshot.CustomerAging.TotalOverdueMad + snapshot.SupplierAging.TotalOverdueMad), OpenOperations = snapshot.Operations.Count(x => x.Status is not (OperationLifecycle.Posted or OperationLifecycle.Cancelled)), UnreconciledBankItems = snapshot.BankOperations.Count(x => x.ReconciliationStatus == ReconciliationStatus.Unreconciled)
        };
    }
    private static void ValidatePasswordPolicy(string password) { if (string.IsNullOrWhiteSpace(password) || password.Length < 12) throw new EnterpriseValidationException("Le mot de passe doit contenir au moins 12 caractères."); }
    private static string HashPassword(string password) { var salt = RandomNumberGenerator.GetBytes(16); var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA256, 32); return $"PBKDF2-SHA256$210000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"; }
    private static bool VerifyPassword(string password, string? encoded)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(encoded)) return false;
            var parts = encoded.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) || iterations < 100_000) return false;
            var salt = Convert.FromBase64String(parts[2]); var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
    private static EnterpriseUser SanitizeUser(EnterpriseUser user) => new() { Id = user.Id, Email = user.Email, DisplayName = user.DisplayName, IsActive = user.IsActive, MustChangePassword = user.MustChangePassword, RoleIds = user.RoleIds.ToList(), CompanyIds = user.CompanyIds.ToList(), CreatedAt = user.CreatedAt, LastLoginAt = user.LastLoginAt, PasswordHash = null };
    private string PartyName(Guid? id) => data.Parties.FirstOrDefault(x => x.Id == id)?.Name ?? "—";
    private Guid? OperationCompany(Guid? id) => data.Operations.FirstOrDefault(x => x.Id == id)?.CompanyId;
    private Guid? OperationParty(Guid? id) => data.Operations.FirstOrDefault(x => x.Id == id)?.PartyId;
    private string BankIban(Guid? id) => data.BankAccounts.FirstOrDefault(x => x.Id == id)?.Iban ?? "";
    private static bool TryParseAmount(string? value, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        var cleaned = value.Trim().ToUpperInvariant();
        foreach (var token in new[] { "MAD", "DHS", "DH", "EUR", "USD", "GBP" }) cleaned = cleaned.Replace(token, "", StringComparison.Ordinal);
        cleaned = cleaned.Replace("\u00A0", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal);
        if (cleaned.Length == 0 || cleaned.Any(c => !char.IsDigit(c) && c is not (',' or '.' or '-' or '+'))) return false;
        if (cleaned.Count(c => c is '-' or '+') > 1 || cleaned.Skip(1).Any(c => c is '-' or '+')) return false;
        if (cleaned.Count(c => c == ',') == 1 && !cleaned.Contains('.')) cleaned = cleaned.Replace(',', '.');
        return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount);
    }
    private static string NormalizeSearch(string? value) { if (string.IsNullOrWhiteSpace(value)) return ""; return string.Concat(value.Normalize(NormalizationForm.FormD).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)).ToUpperInvariant().Trim(); }
    private static string NormalizeCurrency(string? value) { var result = string.IsNullOrWhiteSpace(value) ? "MAD" : value.Trim().ToUpperInvariant(); if (result.Length != 3) throw new EnterpriseValidationException("Le code devise doit contenir trois lettres."); return result; }
    private static void ValidateDimension(Guid? id, IEnumerable<Guid> valid, string label) { if (id.HasValue && !valid.Contains(id.Value)) throw new EnterpriseValidationException($"Dimension {label} introuvable."); }
    private static string Required(string? value, string label) => string.IsNullOrWhiteSpace(value) ? throw new EnterpriseValidationException($"{label} obligatoire.") : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    private static void CopyCompany(EnterpriseCompany s, EnterpriseCompany d) { d.Code = s.Code; d.Name = s.Name; d.LegalName = s.LegalName; d.Ice = s.Ice; d.TaxId = s.TaxId; d.TradeRegister = s.TradeRegister; d.Cnss = s.Cnss; d.Address = s.Address; d.Rib = s.Rib; d.TaxRegime = s.TaxRegime; d.VatSettings = s.VatSettings; d.AccountingPlan = s.AccountingPlan; d.IsActive = s.IsActive; }
    private static void CopyParty(EnterpriseParty s, EnterpriseParty d) { d.InternalCode = s.InternalCode; d.Kind = s.Kind; d.Name = s.Name; d.Ice = s.Ice; d.TaxId = s.TaxId; d.AccountingAccount = s.AccountingAccount; d.CountryCode = s.CountryCode; d.Address = s.Address; d.Email = s.Email; d.Phone = s.Phone; d.BankIban = s.BankIban; d.PaymentTermsDays = s.PaymentTermsDays; d.Risk = s.Risk; d.IsActive = s.IsActive; d.DocumentKeys = s.DocumentKeys.ToList(); d.ContactIds = s.ContactIds.ToList(); }
    private static void CopyCertificate(EnterpriseExemptionCertificate s, EnterpriseExemptionCertificate d) { d.Number = s.Number; d.ClientId = s.ClientId; d.IssueDate = s.IssueDate; d.StartDate = s.StartDate; d.EndDate = s.EndDate; d.AuthorizedAmountMad = s.AuthorizedAmountMad; d.ConsumedAmountMad = s.ConsumedAmountMad; d.DocumentStorageKey = s.DocumentStorageKey; d.Status = s.Status; }
    private static void CopyContract(EnterpriseContract s, EnterpriseContract d) { d.Reference = s.Reference; d.CompanyId = s.CompanyId; d.PartyId = s.PartyId; d.Title = s.Title; d.Category = s.Category; d.StartDate = s.StartDate; d.EndDate = s.EndDate; d.BaseAmountMad = s.BaseAmountMad; d.FrequencyMonths = s.FrequencyMonths; d.RevisionPercent = s.RevisionPercent; d.RevisionEveryYears = s.RevisionEveryYears; d.Status = s.Status; d.DocumentStorageKey = s.DocumentStorageKey; d.SpecialTerms = s.SpecialTerms; }
    private static void CopyImportFile(EnterpriseImportFile s, EnterpriseImportFile d) { d.Reference = s.Reference; d.CompanyId = s.CompanyId; d.SupplierId = s.SupplierId; d.OpenedDate = s.OpenedDate; d.Currency = s.Currency; d.SupplierInvoiceAmount = s.SupplierInvoiceAmount; d.SupplierInvoiceMad = s.SupplierInvoiceMad; d.Costs = s.Costs.ToList(); d.TotalAcquisitionCostMad = s.TotalAcquisitionCostMad; d.AllocationRule = s.AllocationRule; d.Status = s.Status; d.DocumentKeys = s.DocumentKeys.ToList(); }
    private static void CopyTaxRule(EnterpriseTaxRule s, EnterpriseTaxRule d) { d.Code = s.Code; d.Kind = s.Kind; d.Name = s.Name; d.Rate = s.Rate; d.EffectiveFrom = s.EffectiveFrom; d.EffectiveTo = s.EffectiveTo; d.OperationTypes = s.OperationTypes.ToList(); d.NatureContains = s.NatureContains; d.RegulatoryReference = s.RegulatoryReference; d.Priority = s.Priority; d.IsActive = s.IsActive; }
    private static void CopyUser(EnterpriseUser s, EnterpriseUser d) { d.Email = s.Email; d.DisplayName = s.DisplayName; d.IsActive = s.IsActive; d.MustChangePassword = s.MustChangePassword; d.RoleIds = s.RoleIds.ToList(); d.CompanyIds = s.CompanyIds.ToList(); if (!string.IsNullOrWhiteSpace(s.PasswordHash)) d.PasswordHash = s.PasswordHash; }
    private readonly record struct TaxCalculation(decimal VatRate, decimal VatAmountMad, decimal WithholdingRate, decimal WithholdingAmountMad, decimal TotalMad, decimal NetPayableMad);
}

internal sealed class EnterpriseJsonStore
{
    public EnterpriseJsonStore(string? explicitPath)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KayOne");
        FilePath = string.IsNullOrWhiteSpace(explicitPath) ? Path.Combine(folder, "enterprise-data.json") : Path.GetFullPath(explicitPath);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath) ?? folder);
    }
    public string FilePath { get; }
    public bool EnforceAuthorization { get; set; }
    public EnterpriseDatabase Load()
    {
        if (!File.Exists(FilePath)) return new EnterpriseDatabase();
        try { return JsonSerializer.Deserialize<EnterpriseDatabase>(File.ReadAllText(FilePath), EnterpriseJson.Options) ?? new EnterpriseDatabase(); }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            var corrupt = FilePath + $".corrupt-{DateTime.UtcNow:yyyyMMddHHmmss}";
            File.Move(FilePath, corrupt, false);
            var backup = FilePath + ".bak";
            if (File.Exists(backup))
                try { return JsonSerializer.Deserialize<EnterpriseDatabase>(File.ReadAllText(backup), EnterpriseJson.Options) ?? new EnterpriseDatabase(); } catch (Exception backupEx) when (backupEx is JsonException or IOException) { }
            return new EnterpriseDatabase();
        }
    }
    public void Save(EnterpriseDatabase database)
    {
        var temp = FilePath + ".tmp"; var backup = FilePath + ".bak";
        using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
            JsonSerializer.Serialize(stream, database, EnterpriseJson.Options);
        if (File.Exists(FilePath)) File.Copy(FilePath, backup, true);
        File.Move(temp, FilePath, true);
    }
}

internal static class EnterpriseJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() }
    };
}

public sealed class EnterpriseEngineOptions
{
    public string? DataFilePath { get; init; }
    public bool SeedDemoData { get; init; } = true;
    public bool EnforceAuthorization { get; init; }
    public Func<DateTimeOffset>? Clock { get; init; }
}

public sealed class EnterpriseDatabase
{
    public const int CurrentSchemaVersion = 1;
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public DateTimeOffset LastSavedAt { get; set; }
    public Dictionary<string, long> Sequences { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<EnterpriseCompany> Companies { get; set; } = new();
    public List<EnterpriseSite> Sites { get; set; } = new();
    public List<EnterpriseLaboratory> Laboratories { get; set; } = new();
    public List<EnterpriseActivity> Activities { get; set; } = new();
    public List<EnterpriseCostCenter> CostCenters { get; set; } = new();
    public List<EnterpriseProject> Projects { get; set; } = new();
    public List<EnterpriseParty> Parties { get; set; } = new();
    public List<BusinessOperation> Operations { get; set; } = new();
    public List<EnterpriseImpactTrace> ImpactTraces { get; set; } = new();
    public List<EnterpriseDocument> Documents { get; set; } = new();
    public List<EnterpriseInvoice> Invoices { get; set; } = new();
    public List<EnterpriseDueItem> DueItems { get; set; } = new();
    public List<EnterprisePayment> Payments { get; set; } = new();
    public List<EnterpriseTaxRule> TaxRules { get; set; } = new();
    public List<EnterpriseTaxImpact> TaxImpacts { get; set; } = new();
    public List<EnterpriseExchangeRate> ExchangeRates { get; set; } = new();
    public List<EnterpriseAccountingEntry> AccountingEntries { get; set; } = new();
    public List<EnterpriseTreasuryMovement> TreasuryMovements { get; set; } = new();
    public List<EnterpriseBankAccount> BankAccounts { get; set; } = new();
    public List<EnterpriseCashBox> CashBoxes { get; set; } = new();
    public List<EnterpriseCashMovement> CashMovements { get; set; } = new();
    public List<EnterpriseBankStatement> BankStatements { get; set; } = new();
    public List<EnterpriseBankOperation> BankOperations { get; set; } = new();
    public List<EnterpriseReconciliation> Reconciliations { get; set; } = new();
    public List<EnterpriseExemptionCertificate> ExemptionCertificates { get; set; } = new();
    public List<EnterpriseContract> Contracts { get; set; } = new();
    public List<EnterpriseCommitment> Commitments { get; set; } = new();
    public List<EnterpriseImportFile> ImportFiles { get; set; } = new();
    public List<EnterpriseCommissionRule> CommissionRules { get; set; } = new();
    public List<EnterpriseCommissionEntry> Commissions { get; set; } = new();
    public List<EnterpriseFixedAsset> FixedAssets { get; set; } = new();
    public List<EnterpriseDepreciationEntry> DepreciationEntries { get; set; } = new();
    public List<EnterpriseMasterImportJob> MasterImports { get; set; } = new();
    public List<EnterpriseCollectionAction> CollectionActions { get; set; } = new();
    public List<EnterpriseReportingFact> ReportingFacts { get; set; } = new();
    public List<EnterpriseRole> Roles { get; set; } = new();
    public List<EnterpriseUser> Users { get; set; } = new();
    public List<EnterpriseAuditLog> AuditLog { get; set; } = new();
    public EnterpriseSettings Settings { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BusinessOperationType { Achat, Vente, Encaissement, Decaissement, Banque, Caisse, Import, Export, Immobilisation, Paie, Fiscalite, NoteDeFrais, OperationDiverse, Autre }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum OperationDirection { Incoming, Outgoing, Neutral }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum OperationLifecycle { Draft, Submitted, Validated, PaymentPending, PartiallyPaid, Paid, Reconciled, Posted, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum ImpactKind { Document, Invoice, Receivable, Debt, Tax, Accounting, Treasury, Bank, AgedBalance, Reporting, Audit }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum ImpactState { Pending, Generated, Active, Settled, Reconciled, Posted, NotApplicable, Superseded, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum PartyKind { Client, Supplier }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum DueKind { Receivable, Debt }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum DueStatus { Open, PartiallyPaid, Paid, Superseded, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum InvoiceStatus { Draft, Open, PartiallyPaid, Paid, Superseded, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum DocumentStatus { Expected, Uploaded, Verified, Rejected, Superseded, Archived }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum TaxRuleKind { Vat, Withholding, CorporateTax, IncomeTax, Customs }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum AccountingEntryStatus { Draft, Posted, Reversed, Superseded }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum TreasuryDirection { Inflow, Outflow, Neutral }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum TreasuryMovementStatus { Forecast, Executed, Information, Superseded, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum ReconciliationStatus { Unreconciled, Suggested, Reconciled, Rejected }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum PaymentStatus { Prepared, Approved, Executed, Reconciled, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum CertificateStatus { Draft, Active, Exhausted, Expired, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum ContractStatus { Draft, Active, Suspended, Expired, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum CommitmentStatus { Scheduled, Due, Paid, Superseded, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum ImportFileStatus { Draft, InProgress, Cleared, Closed, Cancelled }
[JsonConverter(typeof(JsonStringEnumConverter))] public enum RiskLevel { Low, Medium, High, Blocked }

[Flags, JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnterprisePermission { None = 0, View = 1, CreateOperation = 2, EditOperation = 4, SubmitOperation = 8, ValidateOperation = 16, CancelOperation = 32, CreatePayment = 64, ValidatePayment = 128, ReconcileBank = 256, PostAccounting = 512, ManageTaxRules = 1024, ManageContracts = 2048, ManageImports = 4096, ManageMasterData = 8192, ManageSecurity = 16384, ImportBankStatement = 32768, ExportData = 65536 }

public sealed record EnterpriseActor(Guid? UserId, string DisplayName, string? IpAddress = null, bool IsSystem = false)
{
    public static EnterpriseActor System { get; } = new(null, "KAY ONE SYSTEM", "local", true);
}

public sealed class CreateBusinessOperationRequest
{
    public BusinessOperationType Type { get; init; }
    public OperationDirection? Direction { get; init; }
    public string Nature { get; init; } = "";
    public Guid CompanyId { get; init; }
    public Guid? SiteId { get; init; }
    public Guid? LaboratoryId { get; init; }
    public Guid? ActivityId { get; init; }
    public Guid? CostCenterId { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? PartyId { get; init; }
    public Guid? ContractId { get; init; }
    public Guid? ImportFileId { get; init; }
    public DateOnly OperationDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DueDate { get; init; }
    public int? PaymentTermDays { get; init; }
    public string Currency { get; init; } = "MAD";
    public decimal Amount { get; init; }
    public decimal? ExchangeRate { get; init; }
    public DateOnly? ExchangeRateDate { get; init; }
    public string? ExchangeRateSource { get; init; }
    public decimal? SettlementExchangeRate { get; init; }
    public decimal BankFeesMad { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? WithholdingRate { get; init; }
    public Guid? ExemptionCertificateId { get; init; }
    public string? ExternalInvoiceNumber { get; init; }
    public string? SourceReference { get; init; }
    public string? Description { get; init; }
    public string? DocumentType { get; init; }
    public string? DocumentFileName { get; init; }
    public string? DocumentStorageKey { get; init; }
    public string? DocumentMimeType { get; init; }
    public string? IdempotencyKey { get; init; }
    public List<CreateInvoiceLineRequest> InvoiceLines { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public Dictionary<string, string> CustomFields { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public string? Comment { get; init; }
}

public sealed class UpdateBusinessOperationRequest
{
    public int? ExpectedRowVersion { get; init; }
    public BusinessOperationType? Type { get; init; }
    public OperationDirection? Direction { get; init; }
    public string? Nature { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? SiteId { get; init; }
    public Guid? LaboratoryId { get; init; }
    public Guid? ActivityId { get; init; }
    public Guid? CostCenterId { get; init; }
    public Guid? ProjectId { get; init; }
    public Guid? PartyId { get; init; }
    public Guid? ContractId { get; init; }
    public Guid? ImportFileId { get; init; }
    public DateOnly? OperationDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string? Currency { get; init; }
    public decimal? Amount { get; init; }
    public decimal? ExchangeRate { get; init; }
    public DateOnly? ExchangeRateDate { get; init; }
    public string? ExchangeRateSource { get; init; }
    public decimal? SettlementExchangeRate { get; init; }
    public decimal? BankFeesMad { get; init; }
    public decimal? VatRate { get; init; }
    public decimal? WithholdingRate { get; init; }
    public Guid? ExemptionCertificateId { get; init; }
    public string? SourceReference { get; init; }
    public string? Description { get; init; }
    public string? ExternalInvoiceNumber { get; init; }
    public string? DocumentType { get; init; }
    public string? DocumentFileName { get; init; }
    public string? DocumentStorageKey { get; init; }
    public string? DocumentMimeType { get; init; }
    public List<CreateInvoiceLineRequest>? InvoiceLines { get; init; }
    public List<string>? Tags { get; init; }
    public Dictionary<string, string>? CustomFields { get; init; }
    public string? ChangeReason { get; init; }
}

public sealed class CreateInvoiceLineRequest { public string Description { get; init; } = ""; public decimal Quantity { get; init; } = 1; public decimal UnitPriceMad { get; init; } public decimal VatRate { get; init; } }
public sealed class RegisterPaymentRequest { public Guid OperationId { get; init; } public DateOnly PaymentDate { get; init; } = DateOnly.FromDateTime(DateTime.Today); public decimal Amount { get; init; } public string? Currency { get; init; } public decimal? ExchangeRate { get; init; } public Guid BankAccountId { get; init; } public Guid? ApprovedByUserId { get; init; } public bool AllowCurrencyConversion { get; init; } public string Method { get; init; } = "Virement"; public string? ExternalReference { get; init; } public string? Comment { get; init; } }
public sealed class ReconcileBankRequest { public Guid BankOperationId { get; init; } public Guid StatementLineId { get; init; } public decimal AllowedDifferenceMad { get; init; } = .01m; public string? Comment { get; init; } }
public sealed class BankStatementImportRequest { public Guid BankAccountId { get; init; } public DateOnly PeriodStart { get; init; } public DateOnly PeriodEnd { get; init; } public decimal OpeningBalanceMad { get; init; } public decimal ClosingBalanceMad { get; init; } public string? SourceDocumentKey { get; init; } public List<BankStatementLineRequest> Lines { get; init; } = new(); }
public sealed class BankStatementLineRequest { public DateOnly BookingDate { get; init; } public DateOnly ValueDate { get; init; } public string Label { get; init; } = ""; public string? Reference { get; init; } public decimal AmountMad { get; init; } }

public sealed class BusinessOperation
{
    public Guid Id { get; set; } public string Reference { get; set; } = ""; public BusinessOperationType Type { get; set; }
    public OperationDirection Direction { get; set; } public string Nature { get; set; } = ""; public Guid CompanyId { get; set; }
    public Guid? SiteId { get; set; } public Guid? LaboratoryId { get; set; } public Guid? ActivityId { get; set; }
    public Guid? CostCenterId { get; set; } public Guid? ProjectId { get; set; } public Guid? PartyId { get; set; }
    public Guid? ContractId { get; set; } public Guid? ImportFileId { get; set; } public Guid? ExemptionCertificateId { get; set; } public bool ExemptionConsumed { get; set; } public DateOnly OperationDate { get; set; }
    public DateOnly? DueDate { get; set; } public string Currency { get; set; } = "MAD"; public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1; public DateOnly ExchangeRateDate { get; set; } public string? ExchangeRateSource { get; set; }
    public decimal? SettlementExchangeRate { get; set; } public decimal AmountMad { get; set; } public decimal VatRate { get; set; }
    public decimal VatAmountMad { get; set; } public decimal WithholdingRate { get; set; } public decimal WithholdingAmountMad { get; set; }
    public decimal TotalMad { get; set; } public decimal NetPayableMad { get; set; } public decimal BankFeesMad { get; set; }
    public decimal ExchangeDifferenceMad { get; set; } public string? SourceReference { get; set; } public string? Description { get; set; }
    public OperationLifecycle Status { get; set; } public string? CancellationReason { get; set; } public int RowVersion { get; set; } = 1;
    public string? IdempotencyKey { get; set; }
    public Guid? CreatedByUserId { get; set; } public Guid? ValidatedByUserId { get; set; } public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } public List<string> Tags { get; set; } = new(); public Dictionary<string, string> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class EnterpriseCompany { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? LegalName { get; set; } public string? Ice { get; set; } public string? TaxId { get; set; } public string? TradeRegister { get; set; } public string? Cnss { get; set; } public string? Address { get; set; } public string? Rib { get; set; } public string? TaxRegime { get; set; } public string? VatSettings { get; set; } public string? AccountingPlan { get; set; } public bool IsActive { get; set; } = true; }
public sealed class EnterpriseSite { public Guid Id { get; set; } public Guid CompanyId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? City { get; set; } public bool IsActive { get; set; } = true; }
public sealed class EnterpriseLaboratory { public Guid Id { get; set; } public Guid SiteId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public sealed class EnterpriseActivity { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public sealed class EnterpriseCostCenter { public Guid Id { get; set; } public Guid CompanyId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public sealed class EnterpriseProject { public Guid Id { get; set; } public Guid CompanyId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }

public sealed class EnterpriseParty
{
    public Guid Id { get; set; } public string InternalCode { get; set; } = ""; public PartyKind Kind { get; set; } public string Name { get; set; } = "";
    public string? Ice { get; set; } public string? TaxId { get; set; } public string? AccountingAccount { get; set; } public string CountryCode { get; set; } = "MA";
    public string? Address { get; set; } public string? Email { get; set; } public string? Phone { get; set; } public string? BankIban { get; set; }
    public int PaymentTermsDays { get; set; } = 30; public RiskLevel Risk { get; set; } public bool IsActive { get; set; } = true;
    public List<Guid> ContactIds { get; set; } = new(); public List<string> DocumentKeys { get; set; } = new();
}

public sealed class EnterpriseImpactTrace { public Guid Id { get; set; } public Guid OperationId { get; set; } public ImpactKind Kind { get; set; } public Guid? GeneratedEntityId { get; set; } public string Reference { get; set; } = ""; public ImpactState State { get; set; } public string Detail { get; set; } = ""; public DateTimeOffset CreatedAt { get; set; } }
public sealed class EnterpriseDocument { public Guid Id { get; set; } public Guid? OperationId { get; set; } public string Reference { get; set; } = ""; public string DocumentType { get; set; } = ""; public string FileName { get; set; } = ""; public string? ObjectStorageKey { get; set; } public string MimeType { get; set; } = "application/pdf"; public long FileSizeBytes { get; set; } public DocumentStatus Status { get; set; } public string OcrStatus { get; set; } = "À traiter"; public decimal? OcrConfidence { get; set; } public Dictionary<string, string> OcrExtractedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase); public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset? UploadedAt { get; set; } public Guid? UploadedByUserId { get; set; } }
public sealed class EnterpriseInvoice { public Guid Id { get; set; } public Guid OperationId { get; set; } public Guid CompanyId { get; set; } public Guid? PartyId { get; set; } public string Number { get; set; } = ""; public string? ExternalNumber { get; set; } public DateOnly InvoiceDate { get; set; } public DateOnly DueDate { get; set; } public string Currency { get; set; } = "MAD"; public decimal AmountExcludingTaxMad { get; set; } public decimal VatMad { get; set; } public decimal WithholdingMad { get; set; } public decimal TotalMad { get; set; } public decimal NetPayableMad { get; set; } public InvoiceStatus Status { get; set; } public List<EnterpriseInvoiceLine> Lines { get; set; } = new(); }
public sealed class EnterpriseInvoiceLine { public Guid Id { get; set; } public string Description { get; set; } = ""; public decimal Quantity { get; set; } public decimal UnitPriceMad { get; set; } public decimal VatRate { get; set; } public decimal TotalMad { get; set; } }
public sealed class EnterpriseDueItem { public Guid Id { get; set; } public Guid OperationId { get; set; } public Guid InvoiceId { get; set; } public string Reference { get; set; } = ""; public Guid? PartyId { get; set; } public Guid CompanyId { get; set; } public DueKind Kind { get; set; } public DateOnly DueDate { get; set; } public decimal OriginalAmountMad { get; set; } public decimal PaidMad { get; set; } public decimal OutstandingMad { get; set; } public string Currency { get; set; } = "MAD"; public DueStatus Status { get; set; } }
public sealed class EnterprisePayment { public Guid Id { get; set; } public Guid OperationId { get; set; } public string Reference { get; set; } = ""; public DateOnly PaymentDate { get; set; } public decimal Amount { get; set; } public string Currency { get; set; } = "MAD"; public decimal ExchangeRate { get; set; } public decimal AmountMad { get; set; } public Guid BankAccountId { get; set; } public string Method { get; set; } = ""; public string? ExternalReference { get; set; } public PaymentStatus Status { get; set; } public OperationLifecycle PreviousOperationStatus { get; set; } = OperationLifecycle.Validated; public Guid? PreparedByUserId { get; set; } public Guid? ApprovedByUserId { get; set; } public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset? ApprovedAt { get; set; } public DateTimeOffset? ExecutedAt { get; set; } public DateTimeOffset? CancelledAt { get; set; } public string? CancellationReason { get; set; } public List<PaymentAllocation> Allocations { get; set; } = new(); }
public sealed class PaymentAllocation { public Guid DueItemId { get; set; } public decimal AmountMad { get; set; } }

public sealed class EnterpriseTaxRule { public Guid Id { get; set; } public string Code { get; set; } = ""; public TaxRuleKind Kind { get; set; } public string Name { get; set; } = ""; public decimal Rate { get; set; } public DateOnly EffectiveFrom { get; set; } public DateOnly? EffectiveTo { get; set; } public List<BusinessOperationType> OperationTypes { get; set; } = new(); public string? NatureContains { get; set; } public string? RegulatoryReference { get; set; } public int Priority { get; set; } public bool IsActive { get; set; } = true; }
public sealed class EnterpriseTaxImpact { public Guid Id { get; set; } public Guid OperationId { get; set; } public string Reference { get; set; } = ""; public DateOnly TaxDate { get; set; } public string RuleCode { get; set; } = ""; public decimal TaxableBaseMad { get; set; } public decimal OutputVatMad { get; set; } public decimal InputVatMad { get; set; } public decimal WithholdingMad { get; set; } public Guid? ExemptionCertificateId { get; set; } public ImpactState Status { get; set; } }
public sealed class EnterpriseExchangeRate { public Guid Id { get; set; } public string Currency { get; set; } = ""; public DateOnly RateDate { get; set; } public decimal RateToMad { get; set; } public string Source { get; set; } = ""; }
public sealed class EnterpriseAccountingEntry { public Guid Id { get; set; } public Guid? OperationId { get; set; } public Guid? PaymentId { get; set; } public string Reference { get; set; } = ""; public DateOnly EntryDate { get; set; } public string JournalCode { get; set; } = ""; public string Label { get; set; } = ""; public AccountingEntryStatus Status { get; set; } public Guid? ReversesEntryId { get; set; } public List<EnterpriseAccountingLine> Lines { get; set; } = new(); }
public sealed class EnterpriseAccountingLine { public string AccountCode { get; set; } = ""; public string Label { get; set; } = ""; public decimal DebitMad { get; set; } public decimal CreditMad { get; set; } public EnterpriseAnalyticDimensions Dimensions { get; set; } = new(); }
public sealed class EnterpriseAnalyticDimensions { public Guid CompanyId { get; set; } public Guid? SiteId { get; set; } public Guid? LaboratoryId { get; set; } public Guid? ActivityId { get; set; } public Guid? CostCenterId { get; set; } public Guid? ProjectId { get; set; } public Guid? PartyId { get; set; } }
public sealed class EnterpriseTreasuryMovement { public Guid Id { get; set; } public Guid? OperationId { get; set; } public Guid? PaymentId { get; set; } public Guid? CashBoxId { get; set; } public string Reference { get; set; } = ""; public DateOnly MovementDate { get; set; } public TreasuryDirection Direction { get; set; } public decimal AmountMad { get; set; } public string Currency { get; set; } = "MAD"; public Guid? BankAccountId { get; set; } public TreasuryMovementStatus Status { get; set; } public string Label { get; set; } = ""; }

public sealed class EnterpriseBankAccount { public Guid Id { get; set; } public Guid CompanyId { get; set; } public string Name { get; set; } = ""; public string BankName { get; set; } = ""; public string Iban { get; set; } = ""; public string Currency { get; set; } = "MAD"; public decimal BalanceMad { get; set; } public bool IsActive { get; set; } = true; }
public sealed class EnterpriseCashBox { public Guid Id { get; set; } public Guid CompanyId { get; set; } public Guid? SiteId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string Currency { get; set; } = "MAD"; public decimal BalanceMad { get; set; } public bool IsActive { get; set; } = true; }
public sealed class EnterpriseCashMovement { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid CashBoxId { get; set; } public Guid CompanyId { get; set; } public DateOnly MovementDate { get; set; } public TreasuryDirection Direction { get; set; } public decimal Amount { get; set; } public string Currency { get; set; } = "MAD"; public decimal AmountMad { get; set; } public string Label { get; set; } = ""; public TreasuryMovementStatus Status { get; set; } = TreasuryMovementStatus.Executed; public Guid? AccountingEntryId { get; set; } public DateTimeOffset CreatedAt { get; set; } public Guid? CreatedByUserId { get; set; } }
public sealed class EnterpriseBankStatement { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid BankAccountId { get; set; } public DateOnly PeriodStart { get; set; } public DateOnly PeriodEnd { get; set; } public decimal OpeningBalanceMad { get; set; } public decimal ClosingBalanceMad { get; set; } public string? SourceDocumentKey { get; set; } public DateTimeOffset ImportedAt { get; set; } public List<EnterpriseBankStatementLine> Lines { get; set; } = new(); }
public sealed class EnterpriseBankStatementLine { public Guid Id { get; set; } public DateOnly BookingDate { get; set; } public DateOnly ValueDate { get; set; } public string Label { get; set; } = ""; public string? Reference { get; set; } public decimal AmountMad { get; set; } public ReconciliationStatus ReconciliationStatus { get; set; } public Guid? LinkedBankOperationId { get; set; } }
public sealed class EnterpriseBankOperation { public Guid Id { get; set; } public Guid? OperationId { get; set; } public Guid? PaymentId { get; set; } public string Reference { get; set; } = ""; public Guid? BankAccountId { get; set; } public DateOnly OperationDate { get; set; } public DateOnly ValueDate { get; set; } public decimal AmountMad { get; set; } public string Currency { get; set; } = "MAD"; public string Label { get; set; } = ""; public ReconciliationStatus ReconciliationStatus { get; set; } public Guid? StatementLineId { get; set; } }
public sealed class EnterpriseReconciliation { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid BankOperationId { get; set; } public Guid BankStatementId { get; set; } public Guid StatementLineId { get; set; } public Guid? OperationId { get; set; } public decimal DifferenceMad { get; set; } public DateTimeOffset ReconciledAt { get; set; } public Guid? ReconciledByUserId { get; set; } public string? Comment { get; set; } public bool IsReversed { get; set; } public DateTimeOffset? ReversedAt { get; set; } public Guid? ReversedByUserId { get; set; } public string? ReversalReason { get; set; } }

public sealed class EnterpriseExemptionCertificate { public Guid Id { get; set; } public string Number { get; set; } = ""; public Guid ClientId { get; set; } public DateOnly IssueDate { get; set; } public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; } public decimal AuthorizedAmountMad { get; set; } public decimal ConsumedAmountMad { get; set; } public decimal RemainingAmountMad => Math.Max(0, AuthorizedAmountMad - ConsumedAmountMad); public string? DocumentStorageKey { get; set; } public CertificateStatus Status { get; set; } }
public sealed class EnterpriseContract { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid CompanyId { get; set; } public Guid? PartyId { get; set; } public string Title { get; set; } = ""; public string Category { get; set; } = ""; public DateOnly StartDate { get; set; } public DateOnly EndDate { get; set; } public decimal BaseAmountMad { get; set; } public int FrequencyMonths { get; set; } = 1; public decimal RevisionPercent { get; set; } public int RevisionEveryYears { get; set; } public ContractStatus Status { get; set; } public string? DocumentStorageKey { get; set; } public string? SpecialTerms { get; set; } }
public sealed class EnterpriseCommitment { public Guid Id { get; set; } public Guid ContractId { get; set; } public string Reference { get; set; } = ""; public DateOnly DueDate { get; set; } public decimal AmountMad { get; set; } public CommitmentStatus Status { get; set; } public string? ClauseReference { get; set; } public Guid? GeneratedOperationId { get; set; } }
public sealed class EnterpriseImportFile { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid CompanyId { get; set; } public Guid SupplierId { get; set; } public DateOnly OpenedDate { get; set; } public string Currency { get; set; } = "EUR"; public decimal SupplierInvoiceAmount { get; set; } public decimal SupplierInvoiceMad { get; set; } public List<EnterpriseImportCost> Costs { get; set; } = new(); public decimal TotalAcquisitionCostMad { get; set; } public string AllocationRule { get; set; } = "Valeur"; public ImportFileStatus Status { get; set; } public List<string> DocumentKeys { get; set; } = new(); }
public sealed class EnterpriseImportCost { public Guid Id { get; set; } public string Kind { get; set; } = ""; public decimal AmountMad { get; set; } public string? SupplierReference { get; set; } public string? DocumentKey { get; set; } }
public sealed class EnterpriseCommissionRule { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string Basis { get; set; } = "Ventes facturées"; public decimal Rate { get; set; } public decimal? CapMad { get; set; } public DateOnly EffectiveFrom { get; set; } public DateOnly? EffectiveTo { get; set; } public bool IsActive { get; set; } = true; public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset UpdatedAt { get; set; } }
public sealed class EnterpriseCommissionEntry { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid CommissionRuleId { get; set; } public Guid SourceOperationId { get; set; } public Guid? PartyId { get; set; } public string BeneficiaryName { get; set; } = ""; public string BasisLabel { get; set; } = ""; public decimal BasisAmountMad { get; set; } public decimal Rate { get; set; } public decimal AmountMad { get; set; } public string Period { get; set; } = ""; public DateOnly PeriodDate { get; set; } public string Status { get; set; } = "Calculée"; public DateTimeOffset CalculatedAt { get; set; } }
public sealed class EnterpriseFixedAsset { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid SourceOperationId { get; set; } public Guid CompanyId { get; set; } public Guid? SiteId { get; set; } public Guid? CostCenterId { get; set; } public string Name { get; set; } = ""; public string Category { get; set; } = "Immobilisation"; public DateOnly AcquisitionDate { get; set; } public DateOnly ServiceDate { get; set; } public decimal AcquisitionValueMad { get; set; } public int DurationYears { get; set; } public string DepreciationMethod { get; set; } = "Linéaire"; public decimal AnnualDepreciationMad { get; set; } public decimal AccumulatedDepreciationMad { get; set; } public decimal NetBookValueMad { get; set; } public string Status { get; set; } = "Actif"; }
public sealed class EnterpriseDepreciationEntry { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid FixedAssetId { get; set; } public Guid SourceOperationId { get; set; } public int FiscalYear { get; set; } public DateOnly EntryDate { get; set; } public decimal AmountMad { get; set; } public Guid AccountingEntryId { get; set; } public string Status { get; set; } = "Calculée"; public DateTimeOffset CalculatedAt { get; set; } }
public sealed class EnterpriseMasterImportJob { public Guid Id { get; set; } public string Reference { get; set; } = ""; public string Domain { get; set; } = ""; public string FileName { get; set; } = ""; public long FileSizeBytes { get; set; } public string MimeType { get; set; } = "application/octet-stream"; public string Status { get; set; } = "En attente du contenu"; public int ImportedRecords { get; set; } public int RejectedRecords { get; set; } public DateTimeOffset SubmittedAt { get; set; } public Guid? SubmittedByUserId { get; set; } }
public sealed class EnterpriseCollectionAction { public Guid Id { get; set; } public string Reference { get; set; } = ""; public Guid DueItemId { get; set; } public Guid OperationId { get; set; } public Guid? PartyId { get; set; } public DueKind Side { get; set; } public string ActionType { get; set; } = "Relance"; public int DaysOverdue { get; set; } public decimal OutstandingMad { get; set; } public DateOnly ScheduledDate { get; set; } public string Status { get; set; } = "Planifiée"; public DateTimeOffset CreatedAt { get; set; } public Guid? CreatedByUserId { get; set; } }
public sealed class EnterpriseReportingFact { public Guid Id { get; set; } public Guid OperationId { get; set; } public DateOnly Date { get; set; } public Guid CompanyId { get; set; } public Guid? SiteId { get; set; } public Guid? LaboratoryId { get; set; } public Guid? ActivityId { get; set; } public Guid? CostCenterId { get; set; } public Guid? ProjectId { get; set; } public Guid? PartyId { get; set; } public decimal RevenueMad { get; set; } public decimal ExpenseMad { get; set; } public decimal TaxMad { get; set; } public decimal CashImpactMad { get; set; } public ImpactState Status { get; set; } = ImpactState.Generated; }

public sealed class EnterpriseRole { public Guid Id { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public List<EnterprisePermission> Permissions { get; set; } = new(); }
public sealed class EnterpriseUser { public Guid Id { get; set; } public string Email { get; set; } = ""; public string DisplayName { get; set; } = ""; public string? PasswordHash { get; set; } public bool IsActive { get; set; } = true; public bool MustChangePassword { get; set; } = true; public List<Guid> RoleIds { get; set; } = new(); public List<Guid> CompanyIds { get; set; } = new(); public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset? LastLoginAt { get; set; } }
public sealed class EnterpriseSettings { public int SessionTimeoutMinutes { get; set; } = 30; public bool RequireMakerChecker { get; set; } = true; public decimal MaxReconciliationDifferenceMad { get; set; } = 1m; public int BackupRetentionDays { get; set; } = 30; public string BaseCurrency { get; set; } = "MAD"; public DateTimeOffset UpdatedAt { get; set; } public Guid? UpdatedByUserId { get; set; } }
public sealed class EnterpriseAuditLog { public Guid Id { get; set; } public long Sequence { get; set; } public DateTimeOffset OccurredAt { get; set; } public string EntityType { get; set; } = ""; public string EntityId { get; set; } = ""; public Guid? OperationId { get; set; } public string Action { get; set; } = ""; public Guid? ActorUserId { get; set; } public string ActorDisplayName { get; set; } = ""; public string? IpAddress { get; set; } public string? BeforeJson { get; set; } public string? AfterJson { get; set; } public string? Reason { get; set; } public string PreviousHash { get; set; } = ""; public string Hash { get; set; } = ""; }

public sealed class EnterpriseSnapshot
{
    public DateTimeOffset GeneratedAt { get; set; } public int SchemaVersion { get; set; } public EnterpriseDashboardMetrics Metrics { get; set; } = new(); public AgedBalanceSummary CustomerAging { get; set; } = new(); public AgedBalanceSummary SupplierAging { get; set; } = new();
    public IReadOnlyList<EnterpriseCompany> Companies { get; set; } = Array.Empty<EnterpriseCompany>(); public IReadOnlyList<EnterpriseSite> Sites { get; set; } = Array.Empty<EnterpriseSite>(); public IReadOnlyList<EnterpriseLaboratory> Laboratories { get; set; } = Array.Empty<EnterpriseLaboratory>(); public IReadOnlyList<EnterpriseActivity> Activities { get; set; } = Array.Empty<EnterpriseActivity>(); public IReadOnlyList<EnterpriseCostCenter> CostCenters { get; set; } = Array.Empty<EnterpriseCostCenter>(); public IReadOnlyList<EnterpriseProject> Projects { get; set; } = Array.Empty<EnterpriseProject>(); public IReadOnlyList<EnterpriseParty> Parties { get; set; } = Array.Empty<EnterpriseParty>();
    public IReadOnlyList<BusinessOperation> Operations { get; set; } = Array.Empty<BusinessOperation>(); public IReadOnlyList<EnterpriseImpactTrace> Impacts { get; set; } = Array.Empty<EnterpriseImpactTrace>(); public IReadOnlyList<EnterpriseDocument> Documents { get; set; } = Array.Empty<EnterpriseDocument>(); public IReadOnlyList<EnterpriseInvoice> Invoices { get; set; } = Array.Empty<EnterpriseInvoice>(); public IReadOnlyList<EnterpriseDueItem> DueItems { get; set; } = Array.Empty<EnterpriseDueItem>(); public IReadOnlyList<EnterprisePayment> Payments { get; set; } = Array.Empty<EnterprisePayment>();
    public IReadOnlyList<EnterpriseTaxRule> TaxRules { get; set; } = Array.Empty<EnterpriseTaxRule>(); public IReadOnlyList<EnterpriseTaxImpact> TaxImpacts { get; set; } = Array.Empty<EnterpriseTaxImpact>(); public IReadOnlyList<EnterpriseExchangeRate> ExchangeRates { get; set; } = Array.Empty<EnterpriseExchangeRate>(); public IReadOnlyList<EnterpriseAccountingEntry> AccountingEntries { get; set; } = Array.Empty<EnterpriseAccountingEntry>(); public IReadOnlyList<EnterpriseTreasuryMovement> TreasuryMovements { get; set; } = Array.Empty<EnterpriseTreasuryMovement>();
    public IReadOnlyList<EnterpriseBankAccount> BankAccounts { get; set; } = Array.Empty<EnterpriseBankAccount>(); public IReadOnlyList<EnterpriseCashBox> CashBoxes { get; set; } = Array.Empty<EnterpriseCashBox>(); public IReadOnlyList<EnterpriseCashMovement> CashMovements { get; set; } = Array.Empty<EnterpriseCashMovement>(); public IReadOnlyList<EnterpriseBankStatement> BankStatements { get; set; } = Array.Empty<EnterpriseBankStatement>(); public IReadOnlyList<EnterpriseBankOperation> BankOperations { get; set; } = Array.Empty<EnterpriseBankOperation>(); public IReadOnlyList<EnterpriseReconciliation> Reconciliations { get; set; } = Array.Empty<EnterpriseReconciliation>();
    public IReadOnlyList<EnterpriseExemptionCertificate> ExemptionCertificates { get; set; } = Array.Empty<EnterpriseExemptionCertificate>(); public IReadOnlyList<EnterpriseContract> Contracts { get; set; } = Array.Empty<EnterpriseContract>(); public IReadOnlyList<EnterpriseCommitment> Commitments { get; set; } = Array.Empty<EnterpriseCommitment>(); public IReadOnlyList<EnterpriseImportFile> ImportFiles { get; set; } = Array.Empty<EnterpriseImportFile>(); public IReadOnlyList<EnterpriseCommissionRule> CommissionRules { get; set; } = Array.Empty<EnterpriseCommissionRule>(); public IReadOnlyList<EnterpriseCommissionEntry> Commissions { get; set; } = Array.Empty<EnterpriseCommissionEntry>(); public IReadOnlyList<EnterpriseFixedAsset> FixedAssets { get; set; } = Array.Empty<EnterpriseFixedAsset>(); public IReadOnlyList<EnterpriseDepreciationEntry> DepreciationEntries { get; set; } = Array.Empty<EnterpriseDepreciationEntry>(); public IReadOnlyList<EnterpriseMasterImportJob> MasterImports { get; set; } = Array.Empty<EnterpriseMasterImportJob>(); public IReadOnlyList<EnterpriseCollectionAction> CollectionActions { get; set; } = Array.Empty<EnterpriseCollectionAction>(); public IReadOnlyList<EnterpriseReportingFact> ReportingFacts { get; set; } = Array.Empty<EnterpriseReportingFact>(); public IReadOnlyList<EnterpriseRole> Roles { get; set; } = Array.Empty<EnterpriseRole>(); public IReadOnlyList<EnterpriseUser> Users { get; set; } = Array.Empty<EnterpriseUser>(); public EnterpriseSettings Settings { get; set; } = new(); public IReadOnlyList<EnterpriseAuditLog> AuditLog { get; set; } = Array.Empty<EnterpriseAuditLog>();
}
public sealed class EnterpriseDashboardMetrics { public decimal RevenueMad { get; set; } public decimal ExpensesMad { get; set; } public decimal MarginMad { get; set; } public decimal ProfitBeforeTaxMad { get; set; } public decimal CashBalanceMad { get; set; } public decimal ReceivablesMad { get; set; } public decimal PayablesMad { get; set; } public decimal VatPayableMad { get; set; } public decimal OverdueMad { get; set; } public int OpenOperations { get; set; } public int UnreconciledBankItems { get; set; } }
public sealed class AgedBalanceSummary { public DueKind Kind { get; set; } public decimal NotDueMad { get; set; } public decimal Days1To30Mad { get; set; } public decimal Days31To60Mad { get; set; } public decimal Days61To90Mad { get; set; } public decimal Over90DaysMad { get; set; } public decimal TotalMad => NotDueMad + TotalOverdueMad; public decimal TotalOverdueMad => Days1To30Mad + Days31To60Mad + Days61To90Mad + Over90DaysMad; internal void RoundValues() { NotDueMad = Math.Round(NotDueMad, 2); Days1To30Mad = Math.Round(Days1To30Mad, 2); Days31To60Mad = Math.Round(Days31To60Mad, 2); Days61To90Mad = Math.Round(Days61To90Mad, 2); Over90DaysMad = Math.Round(Over90DaysMad, 2); } }
public sealed class OperationTrace { public BusinessOperation Operation { get; set; } = new(); public IReadOnlyList<EnterpriseImpactTrace> Impacts { get; set; } = Array.Empty<EnterpriseImpactTrace>(); public IReadOnlyList<EnterpriseDocument> Documents { get; set; } = Array.Empty<EnterpriseDocument>(); public IReadOnlyList<EnterpriseInvoice> Invoices { get; set; } = Array.Empty<EnterpriseInvoice>(); public IReadOnlyList<EnterpriseDueItem> DueItems { get; set; } = Array.Empty<EnterpriseDueItem>(); public IReadOnlyList<EnterprisePayment> Payments { get; set; } = Array.Empty<EnterprisePayment>(); public IReadOnlyList<EnterpriseTaxImpact> TaxImpacts { get; set; } = Array.Empty<EnterpriseTaxImpact>(); public IReadOnlyList<EnterpriseAccountingEntry> AccountingEntries { get; set; } = Array.Empty<EnterpriseAccountingEntry>(); public IReadOnlyList<EnterpriseTreasuryMovement> TreasuryMovements { get; set; } = Array.Empty<EnterpriseTreasuryMovement>(); public IReadOnlyList<EnterpriseBankOperation> BankOperations { get; set; } = Array.Empty<EnterpriseBankOperation>(); public IReadOnlyList<EnterpriseReportingFact> ReportingFacts { get; set; } = Array.Empty<EnterpriseReportingFact>(); public IReadOnlyList<EnterpriseAuditLog> AuditLog { get; set; } = Array.Empty<EnterpriseAuditLog>(); }
public sealed class EnterpriseSearchFilter { public decimal? ExactAmountMad { get; init; } public decimal AmountToleranceMad { get; init; } = .01m; public decimal? MinimumAmountMad { get; init; } public decimal? MaximumAmountMad { get; init; } public string? Currency { get; init; } public Guid? CompanyId { get; init; } public Guid? PartyId { get; init; } public DateOnly? StartDate { get; init; } public DateOnly? EndDate { get; init; } public bool MatchAmountFromQuery { get; init; } = true; public List<BusinessOperationType> OperationTypes { get; init; } = new(); public int Limit { get; init; } = 50; }
public sealed class EnterpriseSearchResult { public string Kind { get; set; } = ""; public string EntityId { get; set; } = ""; public Guid? OperationId { get; set; } public string Title { get; set; } = ""; public string Subtitle { get; set; } = ""; public decimal? Amount { get; set; } public string? Currency { get; set; } public string Route { get; set; } = ""; public int Score { get; set; } }
public sealed class ExpirationAlert { public string Kind { get; set; } = ""; public Guid EntityId { get; set; } public string Reference { get; set; } = ""; public DateOnly ExpirationDate { get; set; } public int DaysRemaining { get; set; } public string Threshold { get; set; } = ""; public string RequiredAction { get; set; } = ""; }
public sealed class EnterpriseActionResult { public bool Success { get; set; } public string Action { get; set; } = ""; public string Message { get; set; } = ""; public string? ErrorCode { get; set; } public object? Data { get; set; } public EnterpriseSnapshot? Snapshot { get; set; } }
public sealed class EnterpriseAuthenticationResult { public bool Success { get; set; } public bool PasswordVerified { get; set; } public string Message { get; set; } = ""; public EnterpriseUser? User { get; set; } public EnterpriseActor? Actor { get; set; } }
public sealed class EnterpriseAcceptanceReport { public DateTimeOffset StartedAt { get; set; } public DateTimeOffset FinishedAt { get; set; } public List<EnterpriseAcceptanceCheck> Checks { get; set; } = new(); public int Total => Checks.Count; public int Passed => Checks.Count(x => x.Passed); public bool Success => Total > 0 && Total == Passed; }
public sealed class EnterpriseAcceptanceCheck { public string Name { get; set; } = ""; public bool Passed { get; set; } public string Detail { get; set; } = ""; }

public class EnterpriseValidationException(string message) : InvalidOperationException(message);
public sealed class EnterpriseConcurrencyException(string message) : EnterpriseValidationException(message);
public sealed class EnterpriseAuthorizationException(string message) : UnauthorizedAccessException(message);
