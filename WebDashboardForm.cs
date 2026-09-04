using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace KayOne;

public sealed class WebDashboardForm : Form
{
    private readonly EnterpriseEngine engine = new(new EnterpriseEngineOptions
    {
        DataFilePath = Environment.GetEnvironmentVariable("KAYONE_DATA_FILE"),
        SeedDemoData = true,
        EnforceAuthorization = true
    });
    private readonly WebView2 webView = new() { Dock = DockStyle.Fill };
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly bool runSmokeTest = Environment.GetEnvironmentVariable("KAYONE_UI_SMOKE_TEST") == "1";
    private readonly string? captureDirectory = Environment.GetEnvironmentVariable("KAYONE_CAPTURE_DIRECTORY");
    private readonly bool captureAuthentication = Environment.GetEnvironmentVariable("KAYONE_CAPTURE_AUTH") == "1";
    private readonly System.Windows.Forms.Timer sessionTimer = new() { Interval = 30_000 };
    private EnterpriseActor? authenticatedActor;
    private EnterpriseUser? authenticatedUser;
    private DateTimeOffset lastActivityAt;
    private bool smokeTestStarted;
    private bool captureStarted;

    public WebDashboardForm()
    {
        Text = "KAY ONE · Financial & Business Operating System";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1180, 760);
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(245, 247, 251);
        Controls.Add(webView);
        sessionTimer.Tick += (_, _) => CheckSessionTimeout();
        sessionTimer.Start();
        Shown += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            var settings = webView.CoreWebView2.Settings;
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDevToolsEnabled = Environment.GetEnvironmentVariable("KAYONE_DEVTOOLS") == "1";
            settings.IsStatusBarEnabled = false;
            settings.IsZoomControlEnabled = true;

            var frontend = Path.Combine(AppContext.BaseDirectory, "Frontend");
            if (!Directory.Exists(frontend)) throw new DirectoryNotFoundException(frontend);

            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.kayone.local",
                frontend,
                CoreWebView2HostResourceAccessKind.Allow);
            webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            if (runSmokeTest || (!string.IsNullOrWhiteSpace(captureDirectory) && !captureAuthentication)) EstablishAutomationSession();
            webView.CoreWebView2.NavigationCompleted += async (_, args) =>
            {
                if (!args.IsSuccess) return;
                if (authenticatedActor is not null)
                {
                    SendAuthSuccess();
                    SendSnapshot();
                }
                else SendAuthState();
                if (runSmokeTest && !smokeTestStarted)
                {
                    smokeTestStarted = true;
                    await RunSmokeTestAsync();
                }
                else if (!string.IsNullOrWhiteSpace(captureDirectory) && !captureStarted)
                {
                    captureStarted = true;
                    await CaptureScreensAsync(captureDirectory);
                }
            };
            webView.Source = new Uri("https://app.kayone.local/index.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible de charger l’interface KAY ONE. Vérifiez que Microsoft Edge WebView2 Runtime est installé.\n\n" + ex.Message,
                "KAY ONE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var document = JsonDocument.Parse(e.WebMessageAsJson);
            var root = document.RootElement;
            var type = root.TryGetProperty("type", out var typeNode) ? typeNode.GetString()?.Trim() : null;
            if (string.IsNullOrWhiteSpace(type)) return;

            var payload = root.TryGetProperty("payload", out var payloadNode)
                ? payloadNode
                : EmptyJsonObject();

            if (HandleAuthenticationMessage(type, payload)) return;
            if (authenticatedActor is null)
            {
                SendAuthState("Veuillez vous connecter pour continuer.");
                return;
            }
            lastActivityAt = DateTimeOffset.Now;

            if (type.Equals("refresh", StringComparison.OrdinalIgnoreCase))
            {
                SendSnapshot();
                return;
            }

            if (type.Equals("global-search", StringComparison.OrdinalIgnoreCase))
            {
                var query = ReadText(payload, "query") ?? string.Empty;
                var filter = JsonSerializer.Deserialize<EnterpriseSearchFilter>(payload.GetRawText(), jsonOptions) ?? new EnterpriseSearchFilter();
                SendMessage(new { type = "search-results", query, results = engine.Search(query, filter, CurrentActor()) });
                return;
            }

            if (type.Equals("operation-trace", StringComparison.OrdinalIgnoreCase))
            {
                SendOperationTrace(payload);
                return;
            }

            var command = type.Equals("save-operation", StringComparison.OrdinalIgnoreCase)
                ? "save-enterprise-operation"
                : type;
            var result = engine.HandleAction(command, payload, authenticatedActor);
            if (!result.Success)
            {
                SendMessage(new { type = "enterprise-error", message = result.Message, code = result.ErrorCode });
                return;
            }

            if (command.Equals("save-enterprise-operation", StringComparison.OrdinalIgnoreCase) && result.Data is BusinessOperation operation)
            {
                SendMessage(new { type = "enterprise-operation-saved", reference = operation.Reference, operationId = operation.Id });
            }
            else
            {
                SendMessage(new { type = "enterprise-action-completed", action = command, message = result.Message, data = result.Data });
            }
            SendSnapshot();
        }
        catch (Exception ex)
        {
            SendMessage(new { type = "enterprise-error", message = ex.Message, code = ex.GetType().Name });
        }
    }

    private void SendOperationTrace(JsonElement payload)
    {
        var reference = ReadText(payload, "reference");
        Guid? operationId = Guid.TryParse(ReadText(payload, "id"), out var id) ? id : null;
        if (!operationId.HasValue && !string.IsNullOrWhiteSpace(reference))
            operationId = engine.Search(reference, null, CurrentActor())
                .FirstOrDefault(result => result.OperationId.HasValue && result.Title.Equals(reference, StringComparison.OrdinalIgnoreCase))?.OperationId;
        if (!operationId.HasValue)
        {
            SendMessage(new { type = "enterprise-error", message = "Opération introuvable." });
            return;
        }
        SendMessage(new { type = "operation-trace-result", trace = engine.GetOperationTrace(operationId.Value, CurrentActor()) });
    }

    private EnterpriseActor CurrentActor()
    {
        return authenticatedActor ?? throw new EnterpriseAuthorizationException("Session utilisateur requise.");
    }

    private void EstablishAutomationSession()
    {
        var snapshot = engine.GetSnapshot();
        var administratorRole = snapshot.Roles.FirstOrDefault(x => x.Name.Equals("Administrateur", StringComparison.OrdinalIgnoreCase));
        authenticatedUser = administratorRole is null
            ? snapshot.Users.FirstOrDefault(x => x.IsActive)
            : snapshot.Users.FirstOrDefault(x => x.IsActive && x.RoleIds.Contains(administratorRole.Id));
        if (authenticatedUser is null) throw new EnterpriseAuthorizationException("Aucun administrateur actif n’est configuré pour les tests.");
        authenticatedActor = new EnterpriseActor(authenticatedUser.Id, authenticatedUser.DisplayName, "automation", false);
        lastActivityAt = DateTimeOffset.Now;
    }

    private bool HandleAuthenticationMessage(string type, JsonElement payload)
    {
        if (type.Equals("auth-state", StringComparison.OrdinalIgnoreCase))
        {
            if (authenticatedActor is null) SendAuthState(); else { SendAuthSuccess(); SendSnapshot(); }
            return true;
        }
        if (type.Equals("setup-admin", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var email = ReadText(payload, "email") ?? string.Empty;
                var password = ReadText(payload, "password") ?? string.Empty;
                engine.SetupFirstAdministrator(email, ReadText(payload, "displayName") ?? "Administrateur KAY", password);
                BeginAuthentication(email, password);
            }
            catch (Exception ex) when (ex is EnterpriseValidationException or EnterpriseAuthorizationException)
            {
                SendMessage(new { type = "auth-error", setup = true, email = ReadText(payload, "email"), message = ex.Message });
            }
            return true;
        }
        if (type.Equals("login", StringComparison.OrdinalIgnoreCase))
        {
            BeginAuthentication(ReadText(payload, "email") ?? string.Empty, ReadText(payload, "password") ?? string.Empty);
            return true;
        }
        if (type.Equals("logout", StringComparison.OrdinalIgnoreCase))
        {
            ClearSession();
            SendAuthState("Session fermée en toute sécurité.");
            return true;
        }
        return false;
    }

    private void BeginAuthentication(string email, string password)
    {
        var result = engine.Authenticate(email, password);
        if (result.Success && result.Actor is not null && result.User is not null)
        {
            EstablishSession(result);
            return;
        }
        if (!result.PasswordVerified)
        {
            SendMessage(new { type = "auth-error", stage = "login", email, message = result.Message });
            return;
        }
    }

    private void EstablishSession(EnterpriseAuthenticationResult result)
    {
        authenticatedActor = result.Actor;
        authenticatedUser = result.User;
        lastActivityAt = DateTimeOffset.Now;
        SendAuthSuccess();
        SendSnapshot();
    }

    private void SendAuthState(string? message = null)
    {
        var setup = !engine.HasConfiguredAdministrator;
        SendMessage(new { type = "auth-required", setup, email = setup ? "admin@kayone.ma" : string.Empty, message });
    }

    private void SendAuthSuccess()
    {
        if (authenticatedUser is null) return;
        var snapshot = engine.GetSnapshot(CurrentActor());
        var role = snapshot.Roles.FirstOrDefault(item => authenticatedUser.RoleIds.Contains(item.Id))?.Name ?? "Utilisateur";
        SendMessage(new { type = "auth-success", user = new { authenticatedUser.Id, authenticatedUser.DisplayName, authenticatedUser.Email, role } });
    }

    private void CheckSessionTimeout()
    {
        if (authenticatedActor is null || runSmokeTest || !string.IsNullOrWhiteSpace(captureDirectory)) return;
        var timeout = Math.Max(1, engine.GetSnapshot(CurrentActor()).Settings.SessionTimeoutMinutes);
        if (DateTimeOffset.Now - lastActivityAt <= TimeSpan.FromMinutes(timeout)) return;
        ClearSession();
        SendMessage(new { type = "session-expired" });
    }

    private void ClearSession()
    {
        authenticatedActor = null;
        authenticatedUser = null;
    }

    private void SendSnapshot()
    {
        if (webView.CoreWebView2 is null) return;
        var snapshot = engine.GetSnapshot(CurrentActor());
        var ui = BuildEnterpriseUiData(snapshot);
        var actor = CurrentActor();
        var currentUser = authenticatedUser ?? snapshot.Users.FirstOrDefault(x => x.Id == actor.UserId);
        var payload = new
        {
            type = "snapshot",
            data = new
            {
                totals = new
                {
                    sales = snapshot.Metrics.RevenueMad,
                    receivables = snapshot.Metrics.ReceivablesMad,
                    payables = snapshot.Metrics.PayablesMad,
                    cash = snapshot.Metrics.CashBalanceMad,
                    vatDue = snapshot.Metrics.VatPayableMad,
                    nonDue = snapshot.CustomerAging.NotDueMad,
                    bucket30 = snapshot.CustomerAging.Days1To30Mad,
                    bucket60 = snapshot.CustomerAging.Days31To60Mad + snapshot.CustomerAging.Days61To90Mad,
                    bucket90 = snapshot.CustomerAging.Over90DaysMad
                },
                transactions = ui.Operations,
                companies = snapshot.Companies.Select(x => new { x.Code, x.Name, x.Ice }),
                parties = snapshot.Parties.Select(x => new { code = x.InternalCode, name = $"{x.InternalCode} · {x.Name}", kind = PartyKindLabel(x.Kind), x.PaymentTermsDays }),
                bankAccounts = snapshot.BankAccounts.Select(x => new { x.Name, bank = x.BankName, x.Iban, x.Currency, balance = x.BalanceMad }),
                exchangeRates = snapshot.ExchangeRates.GroupBy(x => x.Currency).ToDictionary(x => x.Key, x => x.OrderByDescending(rate => rate.RateDate).First().RateToMad),
                currentUser = currentUser is null ? null : new { currentUser.Id, currentUser.DisplayName, currentUser.Email, role = snapshot.Roles.FirstOrDefault(role => currentUser.RoleIds.Contains(role.Id))?.Name ?? "Utilisateur" },
                enterprise = ui
            }
        };
        SendMessage(payload);
    }

    private EnterpriseUiData BuildEnterpriseUiData(EnterpriseSnapshot snapshot)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var companyNames = snapshot.Companies.ToDictionary(x => x.Id, x => x.Name);
        var partyNames = snapshot.Parties.ToDictionary(x => x.Id, x => x.Name);
        var operationImpacts = snapshot.Impacts.GroupBy(x => x.OperationId).ToDictionary(x => x.Key, x => x.Count(y => y.State != ImpactState.Cancelled && y.State != ImpactState.Superseded));
        var operations = snapshot.Operations.Select(x => new EnterpriseUiOperation
        {
            Id = x.Reference,
            OperationId = x.Id,
            Reference = x.Reference,
            Date = x.OperationDate.ToString("yyyy-MM-dd"),
            Type = OperationTypeLabel(x.Type),
            Nature = x.Nature,
            Party = x.PartyId.HasValue && partyNames.TryGetValue(x.PartyId.Value, out var party) ? party : "—",
            Company = companyNames.GetValueOrDefault(x.CompanyId, "KAY Groupe"),
            Amount = x.Amount,
            AmountMad = x.AmountMad,
            Currency = x.Currency,
            Status = LifecycleLabel(x.Status),
            Impacts = operationImpacts.GetValueOrDefault(x.Id),
            DueDate = x.DueDate?.ToString("yyyy-MM-dd"),
            CreatedByUserId = x.CreatedByUserId
        }).ToArray();

        var clients = snapshot.Parties.Where(x => x.Kind == PartyKind.Client).Select(x =>
        {
            var dues = snapshot.DueItems.Where(d => d.PartyId == x.Id && d.Kind == DueKind.Receivable && d.Status is DueStatus.Open or DueStatus.PartiallyPaid).ToArray();
            return new EnterpriseUiParty
            {
                Id = x.InternalCode, EntityId = x.Id, Name = x.Name, Ice = x.Ice ?? "—", Balance = dues.Sum(d => d.OutstandingMad),
                Overdue = dues.Where(d => d.DueDate < today).Sum(d => d.OutstandingMad), Dso = x.PaymentTermsDays,
                Risk = RiskLabel(x.Risk), Contact = "Service financier", Email = x.Email ?? "—",
                Invoices = snapshot.Invoices.Count(i => i.PartyId == x.Id), Status = x.IsActive ? "Actif" : "Suspendu"
            };
        }).ToArray();
        var suppliers = snapshot.Parties.Where(x => x.Kind == PartyKind.Supplier).Select(x =>
        {
            var dues = snapshot.DueItems.Where(d => d.PartyId == x.Id && d.Kind == DueKind.Debt && d.Status is DueStatus.Open or DueStatus.PartiallyPaid).ToArray();
            return new EnterpriseUiParty
            {
                Id = x.InternalCode, EntityId = x.Id, Name = x.Name, Ice = x.Ice ?? x.TaxId ?? "—", Balance = dues.Sum(d => d.OutstandingMad),
                Due = dues.Where(d => d.DueDate <= today).Sum(d => d.OutstandingMad), Terms = x.PaymentTermsDays,
                Risk = RiskLabel(x.Risk), Category = x.CountryCode == "MA" ? "Fournisseur local" : "Fournisseur étranger",
                Bank = MaskIban(x.BankIban), Invoices = snapshot.Invoices.Count(i => i.PartyId == x.Id), Status = x.IsActive ? "Actif" : "Suspendu"
            };
        }).ToArray();
        var contracts = snapshot.Contracts.Select(x =>
        {
            var next = snapshot.Commitments.Where(c => c.ContractId == x.Id && c.Status is CommitmentStatus.Scheduled or CommitmentStatus.Due && c.DueDate >= today).OrderBy(c => c.DueDate).FirstOrDefault();
            return new EnterpriseUiContract
            {
                Id = x.Reference, EntityId = x.Id, Name = x.Title, Party = x.PartyId.HasValue ? partyNames.GetValueOrDefault(x.PartyId.Value, "—") : "—",
                Type = x.Category, Amount = x.BaseAmountMad, Frequency = FrequencyLabel(x.FrequencyMonths), Next = (next?.DueDate ?? x.StartDate).ToString("yyyy-MM-dd"),
                End = x.EndDate.ToString("yyyy-MM-dd"), Revision = x.RevisionPercent == 0 ? "Fixe" : $"+{x.RevisionPercent:0.##} % / {x.RevisionEveryYears} ans", Status = ContractStatusLabel(x.Status)
            };
        }).ToArray();
        var imports = snapshot.ImportFiles.Select(x =>
        {
            decimal Cost(string name) => x.Costs.Where(c => c.Kind.Contains(name, StringComparison.OrdinalIgnoreCase)).Sum(c => c.AmountMad);
            return new EnterpriseUiImport
            {
                Id = x.Reference, EntityId = x.Id, Supplier = partyNames.GetValueOrDefault(x.SupplierId, "—"), Country = snapshot.Parties.FirstOrDefault(p => p.Id == x.SupplierId)?.CountryCode ?? "—",
                Currency = x.Currency, Invoice = x.SupplierInvoiceMad, Transport = Cost("Transport"), Insurance = Cost("Assurance"), Transit = Cost("Transit"),
                Port = Cost("Port"), Duties = Cost("Droit"), Other = x.Costs.Where(c => !new[] { "Transport", "Assurance", "Transit", "Port", "Droit" }.Any(k => c.Kind.Contains(k, StringComparison.OrdinalIgnoreCase))).Sum(c => c.AmountMad),
                Status = ImportStatusLabel(x.Status), Progress = ImportProgress(x.Status), Docs = x.DocumentKeys.Count, DocsTotal = 10
            };
        }).ToArray();
        var bankById = snapshot.BankAccounts.ToDictionary(x => x.Id, x => x);
        var bankOperations = snapshot.BankOperations.ToDictionary(x => x.Id, x => x);
        var bankLines = snapshot.BankStatements.SelectMany(statement => statement.Lines.Select(line =>
        {
            var linked = line.LinkedBankOperationId.HasValue ? bankOperations.GetValueOrDefault(line.LinkedBankOperationId.Value) : null;
            var account = bankById.GetValueOrDefault(statement.BankAccountId);
            return new EnterpriseUiBankLine
            {
                Id = line.Id.ToString("D"), StatementId = statement.Id, Date = line.BookingDate.ToString("yyyy-MM-dd"), Label = line.Label,
                Amount = line.AmountMad, Direction = line.AmountMad >= 0 ? "Crédit" : "Débit", Account = account is null ? "—" : $"{account.BankName} · {LastFour(account.Iban)}",
                Suggestion = linked?.Reference ?? "Aucune suggestion", Confidence = linked is null ? 0 : 100, Status = ReconciliationLabel(line.ReconciliationStatus)
            };
        })).ToArray();
        var certificates = snapshot.ExemptionCertificates.Select(x => new EnterpriseUiCertificate
        {
            Id = x.Id.ToString("D"), Number = x.Number, Client = partyNames.GetValueOrDefault(x.ClientId, "—"), Start = x.StartDate.ToString("yyyy-MM-dd"),
            End = x.EndDate.ToString("yyyy-MM-dd"), Allowed = x.AuthorizedAmountMad, Used = x.ConsumedAmountMad, Status = CertificateStatusLabel(x.Status), Document = x.DocumentStorageKey ?? "Document attendu"
        }).ToArray();
        var audit = snapshot.AuditLog.Select(x => new EnterpriseUiAudit
        {
            At = x.OccurredAt.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss"), User = x.ActorDisplayName, Action = x.Action,
            Object = OperationReference(snapshot, x.OperationId) ?? x.EntityId, Detail = x.Reason ?? x.Action, Ip = x.IpAddress ?? "local",
            Level = x.Action.Contains("Refus", StringComparison.OrdinalIgnoreCase) ? "Alerte" : x.ActorDisplayName.Contains("SYSTEM", StringComparison.OrdinalIgnoreCase) ? "Automatique" : "Succès"
        }).ToArray();

        return new EnterpriseUiData
        {
            GeneratedAt = snapshot.GeneratedAt,
            SchemaVersion = snapshot.SchemaVersion,
            Totals = new EnterpriseUiTotals { Sales = snapshot.Metrics.RevenueMad, Receivables = snapshot.Metrics.ReceivablesMad, Payables = snapshot.Metrics.PayablesMad, Cash = snapshot.Metrics.CashBalanceMad, VatDue = snapshot.Metrics.VatPayableMad },
            Metrics = snapshot.Metrics,
            CustomerAging = snapshot.CustomerAging,
            SupplierAging = snapshot.SupplierAging,
            Companies = snapshot.Companies,
            Sites = snapshot.Sites,
            Laboratories = snapshot.Laboratories,
            Activities = snapshot.Activities,
            CostCenters = snapshot.CostCenters,
            Projects = snapshot.Projects,
            Parties = snapshot.Parties.Select(x => new EnterpriseUiPartyOption { Id = x.Id, Code = x.InternalCode, Name = $"{x.InternalCode} · {x.Name}", Kind = PartyKindLabel(x.Kind) }).ToArray(),
            Operations = operations,
            Clients = clients,
            Suppliers = suppliers,
            Contracts = contracts,
            Imports = imports,
            BankLines = bankLines,
            Certificates = certificates,
            Audit = audit,
            Raw = snapshot,
            Alerts = engine.GetExpirationAlerts()
        };
    }

    private void SendMessage(object payload) =>
        webView.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(payload, jsonOptions));

    private async Task RunSmokeTestAsync()
    {
        await Task.Delay(1_000);
        var encoded = await webView.CoreWebView2.ExecuteScriptAsync("JSON.stringify(window.runAllProductionChecks ? window.runAllProductionChecks() : {ok:false,error:'runAllProductionChecks absent'})");
        var uiJson = JsonSerializer.Deserialize<string>(encoded) ?? "{\"ok\":false}";
        using var uiDocument = JsonDocument.Parse(uiJson);
        var uiResult = uiDocument.RootElement.Clone();
        var engineResult = engine.RunAcceptanceChecks();
        var report = new { generatedAt = DateTimeOffset.Now, engine = engineResult, ui = uiResult, success = engineResult.Success && uiResult.TryGetProperty("ok", out var ok) && ok.GetBoolean() };
        var reportPath = Environment.GetEnvironmentVariable("KAYONE_TEST_REPORT") ?? Path.Combine(Path.GetTempPath(), "kayone-acceptance.json");
        File.WriteAllText(reportPath, JsonSerializer.Serialize(report, jsonOptions));
        Environment.ExitCode = report.success ? 0 : 1;
        BeginInvoke(Close);
    }

    private async Task CaptureScreensAsync(string directory)
    {
        Directory.CreateDirectory(directory);
        await Task.Delay(1_200);
        var configured = Environment.GetEnvironmentVariable("KAYONE_CAPTURE_PAGES");
        var pages = string.IsNullOrWhiteSpace(configured)
            ? new[] { "dashboard", "operation", "operations", "payments", "aged", "reporting", "admin" }
            : configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var page in pages.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var pageJson = JsonSerializer.Serialize(page);
            await webView.CoreWebView2.ExecuteScriptAsync($"window.navigate({pageJson})");
            await Task.Delay(350);
            var safeName = string.Concat(page.Where(character => char.IsLetterOrDigit(character) || character is '-' or '_'));
            await using var stream = File.Create(Path.Combine(directory, $"{safeName}.png"));
            await webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
        }
        BeginInvoke(Close);
    }

    private static JsonElement EmptyJsonObject() => JsonSerializer.SerializeToElement(new { });
    private static string? ReadText(JsonElement element, string property) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var node) ? node.ToString() : null;
    private static string OperationTypeLabel(BusinessOperationType type) => type switch { BusinessOperationType.Decaissement => "Décaissement", BusinessOperationType.Fiscalite => "Fiscalité", BusinessOperationType.NoteDeFrais => "Note de frais", BusinessOperationType.OperationDiverse => "Opération diverse", _ => type.ToString() };
    private static string PartyKindLabel(PartyKind kind) => kind == PartyKind.Client ? "Client" : "Fournisseur";
    private static string LifecycleLabel(OperationLifecycle status) => status switch { OperationLifecycle.Draft => "Brouillon", OperationLifecycle.Submitted => "À valider", OperationLifecycle.Validated => "Validée", OperationLifecycle.PaymentPending => "À payer", OperationLifecycle.PartiallyPaid => "Partiellement payée", OperationLifecycle.Paid => "Payée", OperationLifecycle.Reconciled => "Rapprochée", OperationLifecycle.Posted => "Comptabilisée", OperationLifecycle.Cancelled => "Annulée", _ => status.ToString() };
    private static string RiskLabel(RiskLevel risk) => risk switch { RiskLevel.Low => "Faible", RiskLevel.Medium => "Moyen", RiskLevel.High => "Élevé", RiskLevel.Blocked => "Bloqué", _ => risk.ToString() };
    private static string FrequencyLabel(int months) => months switch { 1 => "Mensuel", 3 => "Trimestriel", 6 => "Semestriel", 12 => "Annuel", _ => $"Tous les {months} mois" };
    private static string ContractStatusLabel(ContractStatus status) => status switch { ContractStatus.Active => "Actif", ContractStatus.Draft => "Brouillon", ContractStatus.Suspended => "Suspendu", ContractStatus.Expired => "À renouveler", ContractStatus.Cancelled => "Annulé", _ => status.ToString() };
    private static string ImportStatusLabel(ImportFileStatus status) => status switch { ImportFileStatus.Draft => "Brouillon", ImportFileStatus.InProgress => "En transit", ImportFileStatus.Cleared => "Dédouanement", ImportFileStatus.Closed => "Clôturé", ImportFileStatus.Cancelled => "Annulé", _ => status.ToString() };
    private static int ImportProgress(ImportFileStatus status) => status switch { ImportFileStatus.Draft => 10, ImportFileStatus.InProgress => 55, ImportFileStatus.Cleared => 85, ImportFileStatus.Closed => 100, _ => 0 };
    private static string ReconciliationLabel(ReconciliationStatus status) => status switch { ReconciliationStatus.Reconciled => "Rapproché", ReconciliationStatus.Suggested => "À rapprocher", ReconciliationStatus.Unreconciled => "Non identifié", ReconciliationStatus.Rejected => "Rejeté", _ => status.ToString() };
    private static string CertificateStatusLabel(CertificateStatus status) => status switch { CertificateStatus.Active => "Valide", CertificateStatus.Draft => "Brouillon", CertificateStatus.Exhausted => "Épuisé", CertificateStatus.Expired => "Expiré", CertificateStatus.Cancelled => "Annulé", _ => status.ToString() };
    private static string MaskIban(string? iban) => string.IsNullOrWhiteSpace(iban) ? "—" : $"{iban[..Math.Min(4, iban.Length)]} •••• {LastFour(iban)}";
    private static string LastFour(string? value) => string.IsNullOrEmpty(value) ? "—" : value[^Math.Min(4, value.Length)..];
    private static string? OperationReference(EnterpriseSnapshot snapshot, Guid? operationId) => operationId.HasValue ? snapshot.Operations.FirstOrDefault(x => x.Id == operationId.Value)?.Reference : null;
}

public sealed class EnterpriseUiData
{
    public DateTimeOffset GeneratedAt { get; set; }
    public int SchemaVersion { get; set; }
    public EnterpriseUiTotals Totals { get; set; } = new();
    public EnterpriseDashboardMetrics Metrics { get; set; } = new();
    public AgedBalanceSummary CustomerAging { get; set; } = new();
    public AgedBalanceSummary SupplierAging { get; set; } = new();
    public IReadOnlyList<EnterpriseCompany> Companies { get; set; } = Array.Empty<EnterpriseCompany>();
    public IReadOnlyList<EnterpriseSite> Sites { get; set; } = Array.Empty<EnterpriseSite>();
    public IReadOnlyList<EnterpriseLaboratory> Laboratories { get; set; } = Array.Empty<EnterpriseLaboratory>();
    public IReadOnlyList<EnterpriseActivity> Activities { get; set; } = Array.Empty<EnterpriseActivity>();
    public IReadOnlyList<EnterpriseCostCenter> CostCenters { get; set; } = Array.Empty<EnterpriseCostCenter>();
    public IReadOnlyList<EnterpriseProject> Projects { get; set; } = Array.Empty<EnterpriseProject>();
    public IReadOnlyList<EnterpriseUiPartyOption> Parties { get; set; } = Array.Empty<EnterpriseUiPartyOption>();
    public IReadOnlyList<EnterpriseUiOperation> Operations { get; set; } = Array.Empty<EnterpriseUiOperation>();
    public IReadOnlyList<EnterpriseUiParty> Clients { get; set; } = Array.Empty<EnterpriseUiParty>();
    public IReadOnlyList<EnterpriseUiParty> Suppliers { get; set; } = Array.Empty<EnterpriseUiParty>();
    public IReadOnlyList<EnterpriseUiContract> Contracts { get; set; } = Array.Empty<EnterpriseUiContract>();
    public IReadOnlyList<EnterpriseUiImport> Imports { get; set; } = Array.Empty<EnterpriseUiImport>();
    public IReadOnlyList<EnterpriseUiBankLine> BankLines { get; set; } = Array.Empty<EnterpriseUiBankLine>();
    public IReadOnlyList<EnterpriseUiCertificate> Certificates { get; set; } = Array.Empty<EnterpriseUiCertificate>();
    public IReadOnlyList<EnterpriseUiAudit> Audit { get; set; } = Array.Empty<EnterpriseUiAudit>();
    public EnterpriseSnapshot Raw { get; set; } = new();
    public IReadOnlyList<ExpirationAlert> Alerts { get; set; } = Array.Empty<ExpirationAlert>();
}

public sealed class EnterpriseUiTotals { public decimal Sales { get; set; } public decimal Receivables { get; set; } public decimal Payables { get; set; } public decimal Cash { get; set; } public decimal VatDue { get; set; } }
public sealed class EnterpriseUiPartyOption { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string Kind { get; set; } = ""; }
public sealed class EnterpriseUiOperation { public string Id { get; set; } = ""; public Guid OperationId { get; set; } public string Reference { get; set; } = ""; public string Date { get; set; } = ""; public string Type { get; set; } = ""; public string Nature { get; set; } = ""; public string Party { get; set; } = ""; public string Company { get; set; } = ""; public decimal Amount { get; set; } public decimal AmountMad { get; set; } public string Currency { get; set; } = "MAD"; public string Status { get; set; } = ""; public int Impacts { get; set; } public string? DueDate { get; set; } public Guid? CreatedByUserId { get; set; } }
public sealed class EnterpriseUiParty { public string Id { get; set; } = ""; public Guid EntityId { get; set; } public string Name { get; set; } = ""; public string Ice { get; set; } = ""; public decimal Balance { get; set; } public decimal Overdue { get; set; } public decimal Due { get; set; } public int Dso { get; set; } public int Terms { get; set; } public string Risk { get; set; } = ""; public string Contact { get; set; } = ""; public string Email { get; set; } = ""; public string Category { get; set; } = ""; public string Bank { get; set; } = ""; public int Invoices { get; set; } public string Status { get; set; } = ""; }
public sealed class EnterpriseUiContract { public string Id { get; set; } = ""; public Guid EntityId { get; set; } public string Name { get; set; } = ""; public string Party { get; set; } = ""; public string Type { get; set; } = ""; public decimal Amount { get; set; } public string Frequency { get; set; } = ""; public string Next { get; set; } = ""; public string End { get; set; } = ""; public string Revision { get; set; } = ""; public string Status { get; set; } = ""; }
public sealed class EnterpriseUiImport { public string Id { get; set; } = ""; public Guid EntityId { get; set; } public string Supplier { get; set; } = ""; public string Country { get; set; } = ""; public string Currency { get; set; } = ""; public decimal Invoice { get; set; } public decimal Transport { get; set; } public decimal Insurance { get; set; } public decimal Transit { get; set; } public decimal Port { get; set; } public decimal Duties { get; set; } public decimal Other { get; set; } public string Status { get; set; } = ""; public int Progress { get; set; } public int Docs { get; set; } public int DocsTotal { get; set; } }
public sealed class EnterpriseUiBankLine { public string Id { get; set; } = ""; public Guid StatementId { get; set; } public string Date { get; set; } = ""; public string Label { get; set; } = ""; public decimal Amount { get; set; } public string Direction { get; set; } = ""; public string Account { get; set; } = ""; public string Suggestion { get; set; } = ""; public int Confidence { get; set; } public string Status { get; set; } = ""; }
public sealed class EnterpriseUiCertificate { public string Id { get; set; } = ""; public string Number { get; set; } = ""; public string Client { get; set; } = ""; public string Start { get; set; } = ""; public string End { get; set; } = ""; public decimal Allowed { get; set; } public decimal Used { get; set; } public string Status { get; set; } = ""; public string Document { get; set; } = ""; }
public sealed class EnterpriseUiAudit { public string At { get; set; } = ""; public string User { get; set; } = ""; public string Action { get; set; } = ""; public string Object { get; set; } = ""; public string Detail { get; set; } = ""; public string Ip { get; set; } = ""; public string Level { get; set; } = ""; }
