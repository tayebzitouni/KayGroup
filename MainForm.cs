using System.Drawing.Drawing2D;
using System.Text.Json;

namespace KayOne;

public sealed class MainForm : Form
{
    private readonly TransactionEngine engine = new();
    private readonly Panel content = new();
    private readonly Label pageTitle = new();
    private readonly Label pageSubtitle = new();
    private readonly TextBox search = new();
    private readonly Dictionary<string, Button> navButtons = new();
    // KAY ONE Enterprise tokens from Stitch design system.
    private readonly Color ink = Color.FromArgb(22, 28, 34);
    private readonly Color navy = Color.FromArgb(13, 28, 50);
    private readonly Color blue = Color.FromArgb(37, 99, 235);
    private readonly Color muted = Color.FromArgb(68, 71, 77);
    private readonly Color teal = Color.FromArgb(13, 148, 136);
    private readonly Color pale = Color.FromArgb(246, 249, 255);
    private readonly Color surfaceLow = Color.FromArgb(238, 244, 252);
    private readonly Color border = Color.FromArgb(197, 198, 205);

    public MainForm()
    {
        Text = "KAY ONE · KAY Groupe";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        WindowState = FormWindowState.Maximized;
        BackColor = pale;
        Font = new Font("Segoe UI", 9F);
        BuildShell();
        ShowDashboard();
        Shown += (_, _) => BeginInvoke(new Action(() => { content.AutoScrollPosition = Point.Empty; content.HorizontalScroll.Value = 0; content.VerticalScroll.Value = 0; }));
    }

    private void BuildShell()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = navy, Padding = new Padding(20, 24, 16, 20) };
        Controls.Add(sidebar);
        sidebar.Controls.Add(new Label { Text = "K", ForeColor = Color.White, BackColor = blue, Font = new Font("Segoe UI", 11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Size = new Size(30, 30), Location = new Point(20, 24) });
        sidebar.Controls.Add(new Label { Text = "KAY ONE", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(58, 22) });
        sidebar.Controls.Add(new Label { Text = "FINANCIAL OPS", ForeColor = Color.FromArgb(118, 132, 159), Font = new Font("Segoe UI", 7, FontStyle.Bold), AutoSize = true, Location = new Point(59, 48) });
        sidebar.Controls.Add(new Label { Text = "KAY GROUPE", ForeColor = Color.FromArgb(118, 132, 159), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(20, 94) });

        string[] items = { "Accueil", "Nouvelle opération", "Ventes", "Achats", "Trésorerie", "Fiscalité", "Comptabilité", "Balance âgée", "Journal global", "Administration" };
        var icons = new[] { "▦", "⊕", "▣", "🛒", "▤", "▥", "▤", "◷", "☷", "⚙" };
        int y = 122;
        foreach (var item in items)
        {
            var button = new Button { Text = $"{icons[Array.IndexOf(items, item)]}  {item}", Tag = item, FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(118, 132, 159), BackColor = navy, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9), Size = new Size(224, 38), Location = new Point(14, y), Cursor = Cursors.Hand, Padding = new Padding(14, 0, 0, 0) };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(25, 48, 79);
            button.Click += (_, _) => Navigate(item);
            sidebar.Controls.Add(button);
            navButtons[item] = button;
            y += 42;
        }
        sidebar.Controls.Add(new Label { Text = "v1.0 · TRANSACTION ENGINE", ForeColor = Color.FromArgb(118, 132, 159), Font = new Font("Segoe UI", 7), AutoSize = true, Location = new Point(20, 640) });

        var top = new Panel { Dock = DockStyle.None, Height = 64, BackColor = Color.White, Padding = new Padding(24, 12, 24, 10), Location = new Point(sidebar.Width, 0), Width = Math.Max(100, ClientSize.Width - sidebar.Width) };
        Controls.Add(top); top.BringToFront();
        pageTitle.Location = new Point(24, 10); pageTitle.AutoSize = true; pageTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold); pageTitle.ForeColor = ink; top.Controls.Add(pageTitle);
        pageSubtitle.Location = new Point(25, 36); pageSubtitle.AutoSize = true; pageSubtitle.Font = new Font("Segoe UI", 8); pageSubtitle.ForeColor = muted; top.Controls.Add(pageSubtitle);
        search.PlaceholderText = "Rechercher (Ctrl+K)..."; search.Font = new Font("Segoe UI", 9); search.BackColor = surfaceLow; search.BorderStyle = BorderStyle.FixedSingle; search.Size = new Size(290, 30); search.Anchor = AnchorStyles.Top | AnchorStyles.Left; search.Location = new Point(top.Width - 390, 17); search.TextChanged += (_, _) => { if (pageTitle.Text == "Journal global") ShowJournal(search.Text); }; top.Controls.Add(search);
        var export = MakeButton("Exporter", Color.White, ink, 76); export.Font = new Font("Segoe UI", 8, FontStyle.Bold); export.Location = new Point(0, 17); export.Anchor = AnchorStyles.Top | AnchorStyles.Left; top.Controls.Add(export);
        var add = MakeButton("Ajouter", blue, Color.White, 70); add.Font = new Font("Segoe UI", 8, FontStyle.Bold); add.Location = new Point(0, 17); add.Anchor = AnchorStyles.Top | AnchorStyles.Left; add.Click += (_, _) => ShowNewOperation(); top.Controls.Add(add);
        top.Resize += (_, _) => { add.Left = top.ClientSize.Width - 24 - add.Width; export.Left = add.Left - 8 - export.Width; search.Left = export.Left - 16 - search.Width; };

        content.Dock = DockStyle.None; content.Padding = new Padding(24, 20, 24, 24); content.AutoScroll = true; content.BackColor = Color.White; content.Location = new Point(sidebar.Width, top.Height); content.Size = new Size(Math.Max(100, ClientSize.Width - sidebar.Width), Math.Max(100, ClientSize.Height - top.Height)); content.Anchor = AnchorStyles.None; Controls.Add(content);
        Resize += (_, _) => { var work = Screen.FromControl(this).WorkingArea; var shellWidth = Math.Min(ClientSize.Width, work.Width); var shellHeight = Math.Min(ClientSize.Height, work.Height); top.Bounds = new Rectangle(sidebar.Width, 0, Math.Max(100, shellWidth - sidebar.Width), top.Height); content.Bounds = new Rectangle(sidebar.Width, top.Height, Math.Max(100, shellWidth - sidebar.Width), Math.Max(100, shellHeight - top.Height)); };
        Shown += (_, _) => BeginInvoke(new Action(() => { var work = Screen.FromControl(this).WorkingArea; var shellWidth = Math.Min(ClientSize.Width, work.Width); var shellHeight = Math.Min(ClientSize.Height, work.Height); top.Bounds = new Rectangle(sidebar.Width, 0, Math.Max(100, shellWidth - sidebar.Width), top.Height); content.Bounds = new Rectangle(sidebar.Width, top.Height, Math.Max(100, shellWidth - sidebar.Width), Math.Max(100, shellHeight - top.Height)); add.Left = top.ClientSize.Width - 24 - add.Width; export.Left = add.Left - 8 - export.Width; search.Left = export.Left - 16 - search.Width; }));
        content.SendToBack(); sidebar.BringToFront(); top.BringToFront();
    }

    private void Navigate(string item)
    {
        foreach (var b in navButtons.Values) { b.BackColor = navy; b.ForeColor = Color.FromArgb(118, 132, 159); }
        if (navButtons.TryGetValue(item, out var active)) { active.BackColor = Color.FromArgb(25, 48, 79); active.ForeColor = Color.White; }
        switch (item)
        {
            case "Accueil": ShowDashboard(); break;
            case "Nouvelle opération": ShowNewOperation(); break;
            case "Journal global": ShowJournal(search.Text); break;
            case "Balance âgée": ShowAgedBalance(); break;
            case "Ventes": ShowDomainTransactions("Ventes", new[] { "Vente", "Export", "Encaissement" }); break;
            case "Achats": ShowDomainTransactions("Achats", new[] { "Achat", "Import", "Décaissement" }); break;
            case "Trésorerie": ShowTreasury(); break;
            case "Fiscalité": ShowFiscal(); break;
            case "Comptabilité": ShowAccounting(); break;
            case "Administration": ShowAdministration(); break;
            default: ShowPlaceholder(item); break;
        }
    }

    private void SetHeader(string title, string subtitle) { pageTitle.Text = title; pageSubtitle.Text = subtitle; }
    private void ClearContent() => content.Controls.Clear();

    private void ShowDashboard()
    {
        SetHeader("Bonjour, équipe KAY", "Aperçu financier au 30 août 2026");
        ClearContent();
        content.AutoScroll = false;
        var dashboard = new DashboardCanvas(engine) { Dock = DockStyle.None, Location = Point.Empty, Size = content.ClientSize };
        content.Controls.Add(dashboard);
        content.Resize += (_, _) => dashboard.Bounds = content.ClientRectangle;
    }
    private Panel CreateKpiCard(string label, string value, string hint, Color accent)
    {
        var card = new Panel { Width = 250, Height = 128, BackColor = Color.White, Margin = new Padding(0, 0, 14, 0), Padding = new Padding(18) };
        card.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); using var brush = new SolidBrush(accent); e.Graphics.FillRectangle(brush, 0, 0, 3, card.Height); };
        card.Controls.Add(new Label { Text = label.ToUpperInvariant(), ForeColor = muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(18, 14) });
        card.Controls.Add(new Label { Text = value, ForeColor = ink, Font = new Font("Segoe UI", 19, FontStyle.Bold), AutoSize = true, Location = new Point(16, 40) });
        var badge = new Label { Text = hint, ForeColor = accent == Color.FromArgb(186, 26, 26) ? Color.FromArgb(147, 0, 10) : Color.FromArgb(0, 80, 73), BackColor = accent == Color.FromArgb(186, 26, 26) ? Color.FromArgb(255, 218, 214) : Color.FromArgb(137, 245, 231), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(18, 91), Padding = new Padding(5, 2, 5, 2) };
        card.Controls.Add(badge); return card;
    }
    private void AddKpi(FlowLayoutPanel flow, string label, string value, string hint, Color accent)
    {
        flow.Controls.Add(CreateKpiCard(label, value, hint, accent));
    }

    private Control BuildActivityCard()
    {
        var panel = CreateSectionCard("Dernières opérations", "Vue temps réel du Transaction Engine", out var body);
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Font = new Font("Segoe UI", 9),
            GridColor = Color.FromArgb(226, 232, 240)
        };
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = surfaceLow;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = muted;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 225, 255);
        grid.DefaultCellStyle.SelectionForeColor = ink;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.RowTemplate.Height = 34;
        grid.Columns.Add("type", "TYPE");
        grid.Columns.Add("label", "LIBELLÉ");
        grid.Columns.Add("amount", "MONTANT");
        grid.Columns.Add("status", "STATUT");
        grid.Columns.Add("date", "DATE");
        foreach (var t in engine.Transactions.Take(5)) grid.Rows.Add(t.Type, t.Label, (t.Type is "Achat" or "Décaissement" ? "- " : "+ ") + t.AmountMad.ToString("N2") + " MAD", "Payée", t.Date.ToString("dd/MM/yyyy"));
        body.Controls.Add(grid);
        return panel;
    }
    private Control BuildAgedMiniCard()
    {
        var panel = CreateSectionCard("Balance âgée clients", "Créances par tranche", out var body);
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(0, 8, 0, 0) };
        for (var i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        var labels = new[] { ("Non échue", engine.NonDue, blue), ("0 - 30 jours", engine.Bucket30, Color.FromArgb(164, 187, 232)), ("31 - 60 jours", engine.Bucket60, Color.FromArgb(255, 218, 214)), ("+ 90 jours", engine.Bucket90, Color.FromArgb(186, 26, 26)) };
        for (var i = 0; i < labels.Length; i++)
        {
            var (name, amount, accent) = labels[i];
            var bucket = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 2) };
            bucket.Controls.Add(new Label { Text = name, AutoSize = true, ForeColor = i > 2 ? Color.FromArgb(186, 26, 26) : ink, Font = new Font("Segoe UI", 9, i > 2 ? FontStyle.Bold : FontStyle.Regular), Location = new Point(0, 0) });
            bucket.Controls.Add(new Label { Text = amount.ToString("N0") + " €", AutoSize = true, ForeColor = i > 1 ? Color.FromArgb(186, 26, 26) : ink, Font = new Font("JetBrains Mono", 8, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(0, 0) });
            var track = new Panel { BackColor = Color.FromArgb(221, 227, 235), Height = 8, Dock = DockStyle.Bottom };
            var fill = new Panel { BackColor = accent, Height = 8, Dock = DockStyle.Left, Width = i == 0 ? 440 : i == 1 ? 185 : i == 2 ? 72 : 38 }; track.Controls.Add(fill); bucket.Controls.Add(track);
            bucket.Resize += (_, _) => bucket.Controls[1].Left = bucket.ClientSize.Width - bucket.Controls[1].Width;
            grid.Controls.Add(bucket, 0, i);
        }
        body.Controls.Add(grid); return panel;
    }

    private Control BuildActionsCard()
    {
        var panel = CreateSectionCard("Actions requises", "À traiter aujourd'hui", out var body);
        var alert = new Panel { Location = new Point(0, 8), Height = 96, BackColor = Color.FromArgb(255, 246, 245), Padding = new Padding(12) };
        alert.Paint += (_, e) => { using var pen = new Pen(Color.FromArgb(255, 218, 214)); e.Graphics.DrawRectangle(pen, 0, 0, alert.Width - 1, alert.Height - 1); };
        alert.Controls.Add(new Label { Text = "!   Facture INV-2023-089", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(147, 0, 10), Location = new Point(10, 10) });
        alert.Controls.Add(new Label { Text = "Client XYZ · Retard de 45 jours.\nRelance requise.", AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = muted, Location = new Point(10, 38) }); body.Controls.Add(alert);
        var bank = new Panel { Location = new Point(0, 112), Height = 72, BackColor = surfaceLow, Padding = new Padding(12) };
        bank.Controls.Add(new Label { Text = "▣   Rapprochement bancaire", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = ink, Location = new Point(10, 10) });
        bank.Controls.Add(new Label { Text = "5 transactions non lettrées ce mois-ci.", AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = muted, Location = new Point(10, 38) }); body.Controls.Add(alert); body.Controls.Add(bank); body.Resize += (_, _) => { alert.Width = body.ClientSize.Width; bank.Width = body.ClientSize.Width; }; return panel;
    }
    private Panel Card(string title, string subtitle)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 0, 14, 14) };
        p.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };
        p.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = ink, Location = new Point(20, 18) });
        p.Controls.Add(new Label { Text = subtitle, AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = muted, Location = new Point(20, 48) });
        return p;
    }

    private Panel CreateSectionCard(string title, string subtitle, out Panel body)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 14, 14)
        };
        panel.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); using var titleFont = new Font("Segoe UI", 12, FontStyle.Bold); using var subFont = new Font("Segoe UI", 8); using var brush = new SolidBrush(ink); using var subBrush = new SolidBrush(muted); e.Graphics.DrawString(title, titleFont, brush, 18, 16); if (!string.IsNullOrWhiteSpace(subtitle)) e.Graphics.DrawString(subtitle, subFont, subBrush, 18, 42); };

        var titleLabel = new Label
        {
            Text = title,
            Location = new Point(18, 16),
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ink
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            Location = new Point(18, 42),
            AutoSize = true,
            Font = new Font("Segoe UI", 8),
            ForeColor = muted
        };

        body = new Panel { BackColor = Color.White, Padding = new Padding(0, 8, 0, 0), Location = new Point(18, 62) };
        var sectionBody = body;
        panel.Controls.Add(titleLabel); panel.Controls.Add(subtitleLabel); panel.Controls.Add(sectionBody);
        titleLabel.Visible = false; subtitleLabel.Visible = false;
        panel.Resize += (_, _) => sectionBody.Bounds = new Rectangle(18, 62, Math.Max(10, panel.ClientSize.Width - 36), Math.Max(10, panel.ClientSize.Height - 80));
        return panel;
    }
    private void ShowNewOperation()
    {
        SetHeader("Nouvelle opération", "Saisie rapide · une opération, tous les impacts automatiquement"); ClearContent();
        var form = new Panel { Dock = DockStyle.Top, Height = 610, BackColor = Color.White, Padding = new Padding(40, 34, 40, 28) };
        form.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, form.Width - 1, form.Height - 1); };
        content.Controls.Add(form);
        form.Controls.Add(new Label { Text = "TYPE D'OPÉRATION", AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ink, Location = new Point(40, 32) });
        var typeTabs = new FlowLayoutPanel { Location = new Point(40, 58), Size = new Size(610, 54), BackColor = surfaceLow, Padding = new Padding(6), WrapContents = false };
        form.Controls.Add(typeTabs);
        ComboBox type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Visible = false }; type.Items.AddRange(new[] { "Vente", "Achat", "Encaissement", "Décaissement", "Virement" }); type.SelectedIndex = 0; form.Controls.Add(type);
        foreach (var tab in new[] { "Vente", "Achat", "Encaissement", "Décaissement", "Virement" }) { var b = MakeButton(tab, tab == "Vente" ? Color.White : surfaceLow, tab == "Vente" ? blue : ink, 112); b.Font = new Font("Segoe UI", 9, tab == "Vente" ? FontStyle.Bold : FontStyle.Regular); b.Click += (_, _) => { type.SelectedItem = tab; foreach (Control c in typeTabs.Controls) if (c is Button x) { x.BackColor = x.Text == tab ? Color.White : surfaceLow; x.ForeColor = x.Text == tab ? blue : ink; } }; typeTabs.Controls.Add(b); }

        var date = AddLabeledText(form, "DATE", DateTime.Today.ToString("dd/MM/yyyy"), new Point(40, 150), 250);
        AddLabeledText(form, "MONTANT", "0,00 €", new Point(575, 150), 250, out var amount);
        AddLabeledText(form, "LIBELLÉ", "Ex : Facture prestations octobre", new Point(40, 258), 360, out var label);
        AddLabeledCombo(form, "TIERS (CLIENT / FOURNISSEUR)", engine.Parties.Select(p => p.Name).ToArray(), new Point(40, 366), 360, out var thirdParty);
        AddLabeledCombo(form, "COMPTE COMPTABLE", new[] { "Sélectionner un compte…", "411 · Clients", "401 · Fournisseurs", "512 · Banque" }, new Point(575, 366), 360, out var account);
        var notes = AddLabeledText(form, "NOTES INTERNES (OPTIONNEL)", "Ajouter un commentaire lié à cette opération…", new Point(40, 474), 360, out var note); notes.Multiline = true; notes.Height = 70;
        var divider = new Panel { BackColor = border, Height = 1, Dock = DockStyle.Bottom }; form.Controls.Add(divider);
        var cancel = MakeButton("Annuler", surfaceLow, ink, 130); cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right; cancel.Location = new Point(0, 548); cancel.Click += (_, _) => ShowDashboard(); form.Controls.Add(cancel);
        var save = MakeButton("▣  Enregistrer l'opération", blue, Color.White, 230); save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right; save.Location = new Point(0, 548); save.Click += (_, _) => SaveOperation(type, new ComboBox { Text = "Service" }, new TextBox { Text = thirdParty.Text }, new TextBox(), new TextBox { Text = label.Text }, new ComboBox { Text = "MAD" }, amount); form.Controls.Add(save);
        form.Resize += (_, _) => { cancel.Left = form.ClientSize.Width - 380; save.Left = form.ClientSize.Width - 240; };
        type.SelectedIndexChanged += (_, _) => { };
        return;

        /* legacy form controls are intentionally kept below for compatibility with existing saved data. */
        /*
        int y = 76; AddField(form, "Type d'opération", new[] { "Achat", "Vente", "Encaissement", "Décaissement", "Banque", "Caisse", "Import", "Export", "Immobilisation", "Paie", "Fiscalité", "Note de frais", "Opération diverse", "Autre" }, ref y, out var type);
        AddField(form, "Nature", new[] { "Marchandise", "Réactif", "Consommable", "Matériel", "Immobilisation", "Service", "Service étranger", "Autre" }, ref y, out var nature);
        AddTextField(form, "Tiers / fournisseur / client", "Ex. FR-000318 · Atlas Maintenance", ref y, out var thirdParty);
        thirdParty.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        thirdParty.AutoCompleteSource = AutoCompleteSource.CustomSource;
        thirdParty.AutoCompleteCustomSource.AddRange(engine.Parties.Select(p => $"{p.Code} · {p.Name}").ToArray());
        AddTextField(form, "Société", "KAY LAB · ICE 001234567", ref y, out var company);
        company.Text = engine.Companies.FirstOrDefault()?.Name ?? string.Empty;
        AddTextField(form, "Centre de coût / projet", "Laboratoire · Projet", ref y, out var costCenter);
        AddField(form, "Devise", new[] { "MAD", "EUR", "USD" }, ref y, out var currency);
        AddMoneyField(form, "Montant HT", ref y, out var amount);
        var fx = new Panel { Location = new Point(20, y), Size = new Size(560, 104), BackColor = Color.FromArgb(241, 247, 246), Visible = false, Padding = new Padding(12) }; form.Controls.Add(fx);
        fx.Controls.Add(new Label { Text = "CURRENCY ENGINE · données originales conservées", ForeColor = teal, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(12, 10) });
        var rate = new TextBox { PlaceholderText = "Cours EUR/MAD", Location = new Point(12, 38), Width = 130 }; fx.Controls.Add(rate);
         var converted = new Label { Text = "Contre-valeur : 0,00 MAD", AutoSize = true, ForeColor = ink, Location = new Point(165, 44) }; fx.Controls.Add(converted);
        var bankFees = new TextBox { PlaceholderText = "Frais bancaires", Location = new Point(12, 70), Width = 130 }; fx.Controls.Add(bankFees);
        currency.SelectedIndexChanged += (_, _) => fx.Visible = currency.Text != "MAD";
        rate.TextChanged += (_, _) => { if (decimal.TryParse(amount.Text, out var a) && decimal.TryParse(rate.Text, out var r)) converted.Text = $"Contre-valeur : {(a * r):N2} MAD"; };
        y += 120; AddTextField(form, "Document justificatif", "Importer facture / contrat (PDF)", ref y, out _);
         var save = MakeButton("ENREGISTRER L'OPÉRATION", teal, Color.White, 230); save.Location = new Point(20, y + 8); save.Click += (_, _) => SaveOperation(type, nature, thirdParty, company, costCenter, currency, amount); form.Controls.Add(save);
        PopulateImpacts(impacts, "Achat", "Service", "MAD");
        type.SelectedIndexChanged += (_, _) => PopulateImpacts(impacts, type.Text, nature.Text, currency.Text);
        nature.SelectedIndexChanged += (_, _) => PopulateImpacts(impacts, type.Text, nature.Text, currency.Text);
        currency.SelectedIndexChanged += (_, _) => PopulateImpacts(impacts, type.Text, nature.Text, currency.Text); */
    }

    private TextBox AddLabeledText(Control parent, string caption, string placeholder, Point location, int width) => AddLabeledText(parent, caption, placeholder, location, width, out _);
    private TextBox AddLabeledText(Control parent, string caption, string placeholder, Point location, int width, out TextBox box)
    {
        parent.Controls.Add(new Label { Text = caption, AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ink, Location = location });
        box = new TextBox { PlaceholderText = placeholder, Font = new Font("Segoe UI", 10), Location = new Point(location.X, location.Y + 24), Width = width, Height = 34, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White }; parent.Controls.Add(box); return box;
    }
    private ComboBox AddLabeledCombo(Control parent, string caption, string[] values, Point location, int width, out ComboBox box)
    {
        parent.Controls.Add(new Label { Text = caption, AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ink, Location = location });
        box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), Location = new Point(location.X, location.Y + 24), Width = width, Height = 34, BackColor = Color.White }; box.Items.AddRange(values); box.SelectedIndex = 0; parent.Controls.Add(box); return box;
    }

    private void AddField(Panel parent, string label, string[] values, ref int y, out ComboBox combo)
    { parent.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = muted, Location = new Point(20, y) }); combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), Location = new Point(205, y - 4), Width = 350 }; combo.Items.AddRange(values); combo.SelectedIndex = 0; parent.Controls.Add(combo); y += 48; }
    private void AddTextField(Panel parent, string label, string placeholder, ref int y, out TextBox box)
    { parent.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = muted, Location = new Point(20, y) }); box = new TextBox { PlaceholderText = placeholder, Font = new Font("Segoe UI", 10), Location = new Point(205, y - 4), Width = 350 }; parent.Controls.Add(box); y += 48; }
    private void AddMoneyField(Panel parent, string label, ref int y, out TextBox box) => AddTextField(parent, label, "0,00", ref y, out box);
    private Button MakeButton(string text, Color back, Color fore, int width) { var b = new Button { Text = text, BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Size = new Size(width, 38), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = back == Color.White || back == surfaceLow ? 1 : 0; b.FlatAppearance.BorderColor = border; b.FlatAppearance.MouseOverBackColor = back == Color.White ? surfaceLow : back; return b; }

    private void PopulateImpacts(Panel panel, string type, string nature, string currency)
    {
        while (panel.Controls.Count > 2) panel.Controls.RemoveAt(2); int y = 82; string[] impacts = type is "Vente" or "Export" ? new[] { "Document commercial", "Facture client", "Créance · échéance 30/09/2026", "TVA collectée (paramétrable)", "Écriture comptable · Journal ventes", "Encaissement attendu", "Analytique · CA par activité", "Audit trail" } : new[] { "Document justificatif", "Facture fournisseur", "Dette · échéance 30/09/2026", "TVA déductible (paramétrable)", "Écriture comptable · Journal achats", "Décaissement prévu", "Analytique · centre de coût", "Audit trail" };
        foreach (var impact in impacts) { panel.Controls.Add(new Label { Text = "•  " + impact, AutoSize = true, ForeColor = impact.Contains("TVA") ? Color.FromArgb(177, 79, 91) : ink, Font = new Font("Segoe UI", 9), Location = new Point(20, y) }); y += 35; }
        panel.Controls.Add(new Label { Text = currency == "MAD" ? "Devise MAD · change masqué" : "Devise " + currency + " · Currency Engine actif", AutoSize = true, ForeColor = teal, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(20, y + 10) });
    }

    private void SaveOperation(ComboBox type, ComboBox nature, TextBox thirdParty, TextBox company, TextBox costCenter, ComboBox currency, TextBox amount)
    {
        var normalizedAmount = amount.Text.Replace(" ", string.Empty).Replace(',', '.');
        if (!decimal.TryParse(normalizedAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) || value <= 0) { MessageBox.Show("Saisissez un montant valide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); amount.Focus(); return; }
        var tx = engine.Add(type.Text, nature.Text, string.IsNullOrWhiteSpace(thirdParty.Text) ? "Tiers non renseigné" : thirdParty.Text, currency.Text, value);
        MessageBox.Show($"Opération {tx.Reference} enregistrée.\n{engine.GetImpacts(tx).Count} impacts générés et tracés dans le Journal global.", "Transaction Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        ShowDashboard();
    }

    private void ShowJournal(string filter)
    {
        SetHeader("Journal global", "Toutes les opérations · source unique et traçable");
        ClearContent();
        var panel = CreateSectionCard("Journal global", "Recherche par référence, tiers, montant ou type", out var body);
        panel.Dock = DockStyle.Top;
        panel.Height = 560;
        content.Controls.Add(panel);
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9)
        };
        body.Controls.Add(grid);
        foreach (var c in new[] { "Date", "Référence", "Type", "Tiers", "Devise", "Montant MAD", "Statut", "Traçabilité" }) grid.Columns.Add(c, c.ToUpperInvariant());
        foreach (var t in engine.Transactions.Where(t => string.IsNullOrWhiteSpace(filter) || t.SearchText.Contains(filter, StringComparison.OrdinalIgnoreCase))) grid.Rows.Add(t.Date.ToString("dd/MM/yyyy"), t.Reference, t.Type, t.Party, t.Currency, t.AmountMad.ToString("N2"), "Validée", "Voir l'origine →");
        grid.CellContentClick += (_, e) => { if (e.RowIndex >= 0) MessageBox.Show("Chaîne de traçabilité : document → facture → échéance → écriture → trésorerie → banque → audit.", "Audit trail"); };
    }
    private void ShowAgedBalance()
    {
        SetHeader("Balance âgée", "Analyse des créances clients et suivi du recouvrement");
        ClearContent();
        var header = new Panel { Dock = DockStyle.Top, Height = 66 }; content.Controls.Add(header);
        var searchBox = new TextBox { PlaceholderText = "Rechercher un client…", Width = 260, Height = 32, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(0, 0), BackColor = Color.White }; header.Controls.Add(searchBox);
        var filter = MakeButton("☷  Filtres", Color.White, ink, 90); filter.Anchor = AnchorStyles.Top | AnchorStyles.Right; filter.Location = new Point(0, 0); header.Controls.Add(filter); header.Resize += (_, _) => { filter.Left = header.ClientSize.Width - filter.Width; searchBox.Left = filter.Left - searchBox.Width - 12; };
        var kpis = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 152, WrapContents = false, Padding = new Padding(0, 0, 0, 16) }; content.Controls.Add(kpis);
        AddKpi(kpis, "Total créances", "1 245 000 €", "42 clients concernés", blue); AddKpi(kpis, "Total en retard", "485 200 €", "39% du total", Color.FromArgb(186, 26, 26)); AddKpi(kpis, "Risque > 90 jours", "124 500 €", "", Color.FromArgb(186, 26, 26));
        var launch = MakeButton("▷  Lancer relances (12)", surfaceLow, ink, 220); launch.Height = 96; launch.Margin = new Padding(0, 0, 14, 0); kpis.Controls.Add(launch);
        var panel = CreateSectionCard("Balance âgée clients", "", out var body); panel.Dock = DockStyle.Top; panel.Height = 420; content.Controls.Add(panel);
        var grid = CreateGrid(new[] { "CLIENT", "TOTAL DÛ", "NON ÉCHU", "1–30 J", "31–60 J", "61–90 J", "+90 JOURS", "ACTION" }, new Point(0, 0), new Size(1, 1)); grid.Dock = DockStyle.Fill; body.Controls.Add(grid);
        var rows = new[] { ("Acme Corp", 145200m, 0m, 0m, 45000m, 80200m), ("Global Ind.", 85000m, 85000m, 0m, 0m, 0m), ("TechCorp", 42500m, 12000m, 30500m, 0m, 0m) };
        foreach (var r in rows) grid.Rows.Add(r.Item1, r.Item2.ToString("N2") + " €", r.Item3.ToString("N2") + " €", r.Item4.ToString("N2") + " €", "0,00 €", r.Item5.ToString("N2") + " €", r.Item6.ToString("N2") + " €", "›");
        grid.Rows.Add("TOTAL GÉNÉRAL", "272 700,00 €", "97 000,00 €", "30 500,00 €", "20 000,00 €", "45 000,00 €", "80 200,00 €", "");
    }
    private void ShowDomainTransactions(string domain, string[] types)
    {
        SetHeader(domain, domain == "Ventes" ? "Suivi et gestion des factures clients" : "Suivi et gestion des factures fournisseurs");
        ClearContent();
        var title = new Label { Text = domain == "Ventes" ? "Gestion des Ventes" : "Gestion des Achats", AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = ink, Location = new Point(0, 0) }; content.Controls.Add(title);
        content.Controls.Add(new Label { Text = domain == "Ventes" ? "Suivi et gestion des factures clients" : "Suivi et gestion des factures fournisseurs", AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = muted, Location = new Point(0, 32) });
        var export = MakeButton("⇩  Exporter", Color.White, ink, 120); export.Anchor = AnchorStyles.Top | AnchorStyles.Right; export.Location = new Point(0, 0); content.Controls.Add(export);
        var add = MakeButton(domain == "Ventes" ? "+  Nouvelle vente" : "+  Nouvel achat", blue, Color.White, 170); add.Anchor = AnchorStyles.Top | AnchorStyles.Right; add.Location = new Point(0, 0); add.Click += (_, _) => ShowNewOperation(); content.Controls.Add(add);
        content.Resize += (_, _) => { add.Left = content.ClientSize.Width - add.Width; export.Left = add.Left - export.Width - 12; };

        var filters = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 78, 0, 16) }; filters.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, filters.Width - 1, filters.Height - 1); }; content.Controls.Add(filters);
        var query = new TextBox { PlaceholderText = domain == "Ventes" ? "Rechercher un client, n° facture…" : "Rechercher un fournisseur, n° facture…", Location = new Point(20, 20), Width = 320, Height = 34, Font = new Font("Segoe UI", 9), BackColor = surfaceLow, BorderStyle = BorderStyle.FixedSingle }; filters.Controls.Add(query);
        var period = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(355, 20), Width = 170, Height = 34, Font = new Font("Segoe UI", 9) }; period.Items.AddRange(new[] { "Ce mois-ci", "Ce trimestre", "Cette année" }); period.SelectedIndex = 0; filters.Controls.Add(period);
        var status = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(540, 20), Width = 180, Height = 34, Font = new Font("Segoe UI", 9) }; status.Items.AddRange(new[] { "Tous les statuts", "Payé", "En attente", "Retard", "Partiel" }); status.SelectedIndex = 0; filters.Controls.Add(status);
        var reset = new Label { Text = "Réinitialiser les filtres", AutoSize = true, ForeColor = blue, Font = new Font("Segoe UI", 9), Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(0, 29), Cursor = Cursors.Hand }; filters.Controls.Add(reset); filters.Resize += (_, _) => reset.Left = filters.ClientSize.Width - reset.Width - 20;

        var panel = CreateSectionCard(domain == "Ventes" ? "Factures clients" : "Factures fournisseurs", "", out var body); panel.Dock = DockStyle.Top; panel.Height = 430; content.Controls.Add(panel);
        var grid = CreateGrid(new[] { "N° FACTURE", domain == "Ventes" ? "CLIENT" : "FOURNISSEUR", "DATE", "ÉCHÉANCE", "MONTANT HT", "TVA", "MONTANT TTC", "STATUT" }, new Point(0, 0), new Size(1, 1)); grid.Dock = DockStyle.Fill; body.Controls.Add(grid);
        var rows = engine.Transactions.Where(t => types.Contains(t.Type)).ToList();
        if (rows.Count == 0) rows.Add(new Transaction(domain == "Ventes" ? "FAC-2023-1042" : "ACH-2023-0891", DateTime.Today.AddDays(-18), domain == "Ventes" ? "Vente" : "Achat", "Service", domain == "Ventes" ? "Acme Corporation" : "Atlas Maintenance", "EUR", 4500, 4500));
        foreach (var t in rows) { var vat = t.AmountMad * engine.VatRate(DateTime.Today); grid.Rows.Add(t.Reference, t.Party, t.Date.ToString("dd/MM/yyyy"), t.Date.AddDays(30).ToString("dd/MM/yyyy"), t.AmountMad.ToString("N2"), vat.ToString("N2"), (t.AmountMad + vat).ToString("N2"), "En attente"); }
        content.Controls.Add(new Label { Text = $"Affichage 1-{rows.Count} sur {Math.Max(rows.Count, 124)} factures", AutoSize = true, ForeColor = muted, Font = new Font("Segoe UI", 8), Location = new Point(20, 650) });
    }
    private void ShowTreasury()
    {
        SetHeader("Trésorerie", "Vue d'ensemble de vos comptes bancaires et flux financiers");
        ClearContent();
        var selectors = new Panel { Dock = DockStyle.Top, Height = 54 }; content.Controls.Add(selectors);
        var bank = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right }; bank.Items.AddRange(new[] { "Toutes les banques", "BNP Paribas", "Société Générale" }); bank.SelectedIndex = 0; selectors.Controls.Add(bank);
        var period = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180, Height = 30, Anchor = AnchorStyles.Top | AnchorStyles.Right }; period.Items.AddRange(new[] { "Ce mois-ci", "Ce trimestre", "Cette année" }); period.SelectedIndex = 0; selectors.Controls.Add(period); selectors.Resize += (_, _) => { period.Left = selectors.ClientSize.Width - period.Width; bank.Left = period.Left - bank.Width - 12; };
        var upper = new TableLayoutPanel { Dock = DockStyle.Top, Height = 510, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 0, 0, 18) }; upper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32)); upper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68)); content.Controls.Add(upper);
        var accounts = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 0, 16, 0) }; upper.Controls.Add(accounts, 0, 0);
        AddKpi(accounts, "Solde total", "142 500,00 €", "+4,2% vs mois dernier", teal);
        foreach (var b in engine.BankAccounts) { var card = new Panel { Width = 300, Height = 120, BackColor = Color.White, Padding = new Padding(20), Margin = new Padding(0, 0, 0, 14) }; card.Paint += (_, e) => { using var pen = new Pen(border); e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); }; card.Controls.Add(new Label { Text = b.Bank, AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ink, Location = new Point(20, 16) }); card.Controls.Add(new Label { Text = "··· " + b.Iban[^4..], AutoSize = true, Font = new Font("JetBrains Mono", 9), ForeColor = muted, Location = new Point(20, 44) }); card.Controls.Add(new Label { Text = b.Balance.ToString("N2") + " MAD", AutoSize = true, Font = new Font("JetBrains Mono", 12, FontStyle.Bold), ForeColor = ink, Location = new Point(20, 72) }); accounts.Controls.Add(card); }
        var chart = CreateSectionCard("Flux de trésorerie (Entrant vs Sortant)", "", out var chartBody); upper.Controls.Add(chart, 1, 0); chartBody.Controls.Add(new CashflowChart { Dock = DockStyle.Fill, Padding = new Padding(12) });
        var panel = CreateSectionCard("Derniers flux", "Mouvements bancaires récents", out var body); panel.Dock = DockStyle.Top; panel.Height = 310; content.Controls.Add(panel);
        var grid = CreateGrid(new[] { "DATE", "LIBELLÉ", "BANQUE", "ENTRÉE (+)", "SORTIE (-)", "SOLDE" }, new Point(0, 0), new Size(1, 1)); grid.Dock = DockStyle.Fill; body.Controls.Add(grid);
        grid.Rows.Add("24/10/23", "Virement Client ALPHATECH", "BNP Paribas", "+ 12 450,00", "-", "142 500,00"); grid.Rows.Add("23/10/23", "Prélèvement URSSAF", "Soc. Générale", "-", "- 4 230,50", "130 050,00"); grid.Rows.Add("22/10/23", "Paiement Fournisseur AWS", "BNP Paribas", "-", "- 850,00", "134 280,50"); grid.Rows.Add("20/10/23", "Remboursement Note de Frais", "BNP Paribas", "-", "- 120,00", "135 130,50");
    }
    private void ShowFiscal()
    {
        SetHeader("Fiscalité", "Fiscal Engine · règles datées et paramétrables");
        ClearContent();
        var panel = CreateSectionCard("Règles fiscales actives", "Aucun taux fiscal n'est enfoui dans les écrans métier", out var body);
        panel.Dock = DockStyle.Top;
        panel.Height = 390;
        content.Controls.Add(panel);
        var grid = CreateGrid(new[] { "Code", "Début", "Fin", "Taux", "Référence", "Statut" }, new Point(0, 0), new Size(1, 1));
        grid.Dock = DockStyle.Fill;
        body.Controls.Add(grid);
        foreach (var r in engine.TaxRules) grid.Rows.Add(r.Code, r.Start.ToString("dd/MM/yyyy"), r.End?.ToString("dd/MM/yyyy") ?? "Sans fin", r.Rate.ToString("P0"), r.RegulatoryReference, "Active");
        var summary = new Label { Text = $"TVA nette estimée : {engine.VatDue:N2} MAD", Dock = DockStyle.Bottom, Height = 30, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(177, 79, 91), TextAlign = ContentAlignment.MiddleLeft };
        body.Controls.Add(summary);
    }
    private void ShowAccounting()
    {
        SetHeader("Comptabilité", "Accounting Engine · conséquence contrôlée des opérations métier");
        ClearContent();
        var panel = CreateSectionCard("Écritures générées", "Journal comptable unifié avec lien vers l'origine", out var body);
        panel.Dock = DockStyle.Top;
        panel.Height = 530;
        content.Controls.Add(panel);
        var grid = CreateGrid(new[] { "Date", "Journal", "Pièce", "Libellé", "Débit", "Crédit", "Origine" }, new Point(0, 0), new Size(1, 1));
        grid.Dock = DockStyle.Fill;
        body.Controls.Add(grid);
        foreach (var t in engine.Transactions)
        {
            var purchase = t.Type is "Achat" or "Import" or "Décaissement";
            grid.Rows.Add(t.Date.ToString("dd/MM/yyyy"), purchase ? "ACH" : "VEN", t.Reference, t.Label, purchase ? t.AmountMad.ToString("N2") : "-", purchase ? "-" : t.AmountMad.ToString("N2"), t.Reference);
        }
    }
    private void ShowAdministration()
    {
        SetHeader("Administration", "Gérez les paramètres globaux, les utilisateurs et la configuration financière de votre entreprise.");
        ClearContent();
        var upper = new TableLayoutPanel { Dock = DockStyle.Top, Height = 440, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 10, 0, 18) }; upper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); upper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); content.Controls.Add(upper);
        var company = CreateSectionCard("▦  Paramètres société", "", out var companyBody); upper.Controls.Add(company, 0, 0);
        AddLabeledText(companyBody, "RAISON SOCIALE", "KAY ONE Technologies", new Point(20, 26), 280);
        AddLabeledText(companyBody, "NUMÉRO D'IDENTIFICATION (SIRET / TVA)", "FR89 123456789", new Point(350, 26), 280);
        AddLabeledText(companyBody, "ADRESSE DU SIÈGE", "128 Rue de la Paix, 75001 Paris, France", new Point(20, 134), 610);
        AddLabeledText(companyBody, "CODE NAF", "6201Z", new Point(20, 242), 190); AddLabeledCombo(companyBody, "FORME JURIDIQUE", new[] { "SAS", "SARL", "SA" }, new Point(235, 242), 190, out _); AddLabeledText(companyBody, "CAPITAL SOCIAL", "50 000 €", new Point(450, 242), 180);
        var save = MakeButton("Enregistrer les modifications", blue, Color.White, 220); save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right; save.Location = new Point(0, 340); companyBody.Controls.Add(save); companyBody.Resize += (_, _) => save.Left = companyBody.ClientSize.Width - save.Width;
        var rates = CreateSectionCard("↻  Devises & Taux", "SYNC AUTO", out var rateBody); upper.Controls.Add(rates, 1, 0);
        AddLabeledCombo(rateBody, "DEVISE DE RÉFÉRENCE", new[] { "EUR - Euro", "MAD - Dirham", "USD - Dollar" }, new Point(20, 26), 260, out _);
        rateBody.Controls.Add(new Label { Text = "TAUX DE CHANGE ACTIFS", AutoSize = true, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ink, Location = new Point(20, 112) });
        foreach (var (name, value) in new[] { ("USD/EUR", "0.9241"), ("GBP/EUR", "1.1682"), ("CHF/EUR", "1.0345") }) { var row = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = surfaceLow, Padding = new Padding(10), Margin = new Padding(0, 8, 0, 0) }; row.Controls.Add(new Label { Text = name, AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ink, Location = new Point(10, 12) }); row.Controls.Add(new Label { Text = value, AutoSize = true, Font = new Font("JetBrains Mono", 9, FontStyle.Bold), ForeColor = ink, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(0, 12) }); row.Resize += (_, _) => row.Controls[1].Left = row.ClientSize.Width - row.Controls[1].Width - 10; rateBody.Controls.Add(row); }
        var users = CreateSectionCard("♙  Gestion des utilisateurs & Rôles", "", out var userBody); users.Dock = DockStyle.None; users.Height = 330; users.Location = new Point(0, 450); users.Width = Math.Max(900, content.ClientSize.Width - 48); content.Controls.Add(users); upper.Dock = DockStyle.None; upper.Location = Point.Empty; upper.Width = users.Width; var addUser = MakeButton("♙  Nouvel utilisateur", blue, Color.White, 170); addUser.Anchor = AnchorStyles.Top | AnchorStyles.Right; addUser.Location = new Point(0, 0); userBody.Controls.Add(addUser); userBody.Resize += (_, _) => addUser.Left = userBody.ClientSize.Width - addUser.Width;
        var grid = CreateGrid(new[] { "UTILISATEUR", "EMAIL", "RÔLE", "DERNIÈRE CONNEXION", "STATUT", "ACTIONS" }, new Point(0, 52), new Size(1, 1)); grid.Dock = DockStyle.Fill; userBody.Controls.Add(grid); grid.Rows.Add("Jean Dupont", "jean.dupont@kayone.com", "Administrateur", "24/10/2023 09:12", "Actif", "✎"); grid.Rows.Add("Marie Laurent", "marie.laurent@kayone.com", "Comptable", "23/10/2023 16:45", "Actif", "✎"); grid.Rows.Add("Paul Atréides", "paul.a@kayone.com", "Auditeur", "15/10/2023 11:20", "Inactif", "✎"); content.Resize += (_, _) => { var width = Math.Max(900, content.ClientSize.Width - 48); upper.Width = width; users.Width = width; };
    }
    private DataGridView CreateGrid(string[] columns, Point location, Size size)
    {
        var grid = new DataGridView { Location = location, Size = size, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 9), GridColor = Color.FromArgb(226, 232, 240) };
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = surfaceLow;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = muted;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 225, 255);
        grid.DefaultCellStyle.SelectionForeColor = ink;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.RowTemplate.Height = 34;
        foreach (var column in columns) grid.Columns.Add(column, column.ToUpperInvariant());
        return grid;
    }

    private void ShowPlaceholder(string name)
    { SetHeader(name, "Vue intégrée au Transaction Engine"); ClearContent(); var p = Card(name, "Ce domaine partage la même source transactionnelle."); p.Dock = DockStyle.Top; p.Height = 260; content.Controls.Add(p); p.Controls.Add(new Label { Text = "Les opérations saisies depuis « Nouvelle opération » alimentent automatiquement cette vue.\n\nProchaine étape : connecter les règles et référentiels de production.", AutoSize = true, ForeColor = muted, Font = new Font("Segoe UI", 11), Location = new Point(20, 95) }); }
}

internal sealed class DashboardCanvas : Control
{
    private readonly TransactionEngine engine;
    private readonly Color ink = Color.FromArgb(22, 28, 34), muted = Color.FromArgb(68, 71, 77), blue = Color.FromArgb(37, 99, 235), teal = Color.FromArgb(13, 148, 136), pale = Color.FromArgb(246, 249, 255), low = Color.FromArgb(238, 244, 252), border = Color.FromArgb(197, 198, 205), red = Color.FromArgb(186, 26, 26);
    public DashboardCanvas(TransactionEngine engine) { this.engine = engine; DoubleBuffered = true; BackColor = Color.White; Padding = new Padding(24); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; var w = Math.Max(760, Math.Min(1200, ClientSize.Width - 48)); var x0 = 24; using var titleFont = new Font("Segoe UI", 19, FontStyle.Bold); using var subtitleFont = new Font("Segoe UI", 9); using var hFont = new Font("Segoe UI", 12, FontStyle.Bold); using var bodyFont = new Font("Segoe UI", 9); using var mono = new Font("JetBrains Mono", 9, FontStyle.Bold); using var labelFont = new Font("Segoe UI", 8, FontStyle.Bold); using var inkBrush = new SolidBrush(ink); using var mutedBrush = new SolidBrush(muted); using var blueBrush = new SolidBrush(blue); using var tealBrush = new SolidBrush(teal);
        DrawText(g, "DERNIÈRE SYNCHRO", labelFont, mutedBrush, w - 130, 12); DrawBadge(g, "Il y a 2 min", w - 140, 34, low, ink, mono);
        var cards = new[] { ("CRÉANCES CLIENTS", "145 230 €", "+12,5 %  vs mois précédent", teal), ("TRÉSORERIE DISPONIBLE", "89 400 €", "+4,2 %  vs mois précédent", teal), ("TVA À PAYER", "24 150 €", "Échéance J-5", red), ("CHARGES À VENIR", "42 800 €", "Sur les 30 prochains jours", blue) }; var cardW = Math.Max(210, (int)((w - 36) / 4));
        for (var i = 0; i < cards.Length; i++) { var cx = x0 + i * (cardW + 12); DrawCard(g, new Rectangle(cx, 66, cardW, 128), cards[i].Item4, 3); DrawText(g, cards[i].Item1, labelFont, mutedBrush, cx + 18, 82); DrawText(g, cards[i].Item2, titleFont, inkBrush, cx + 18, 110); DrawBadge(g, cards[i].Item3, cx + 18, 159, cards[i].Item4 == red ? Color.FromArgb(255, 218, 214) : Color.FromArgb(137, 245, 231), cards[i].Item4 == red ? Color.FromArgb(147, 0, 10) : Color.FromArgb(0, 80, 73), bodyFont); }
        var mainW = (int)(w * .68); var sideW = w - mainW - 16; var topY = 208; DrawCard(g, new Rectangle(x0, topY, mainW, 316), border, 0); DrawText(g, "Balance âgée clients", hFont, inkBrush, x0 + 20, topY + 20); DrawText(g, "Détails  →", bodyFont, mutedBrush, x0 + mainW - 95, topY + 22);
        var bucket = new[] { ("Non échue", "85 000 €", blue, .72f), ("0 - 30 jours", "35 230 €", Color.FromArgb(164, 187, 232), .30f), ("31 - 60 jours", "15 000 €", Color.FromArgb(255, 218, 214), .13f), ("+ 90 jours", "10 000 €", red, .05f) }; for (var i = 0; i < bucket.Length; i++) { var y = topY + 74 + i * 58; DrawText(g, bucket[i].Item1, bodyFont, i > 1 ? new SolidBrush(red) : inkBrush, x0 + 20, y); DrawText(g, bucket[i].Item2, mono, i > 1 ? new SolidBrush(red) : inkBrush, x0 + mainW - 110, y); using var track = new SolidBrush(Color.FromArgb(221, 227, 235)); g.FillRectangle(track, x0 + 20, y + 26, mainW - 40, 8); using var fill = new SolidBrush(bucket[i].Item3); g.FillRectangle(fill, x0 + 20, y + 26, (int)((mainW - 40) * bucket[i].Item4), 8); }
        var sx = x0 + mainW + 16; DrawCard(g, new Rectangle(sx, topY, sideW, 316), border, 0); DrawText(g, "Actions requises", hFont, inkBrush, sx + 20, topY + 20); DrawText(g, "À traiter aujourd'hui", bodyFont, mutedBrush, sx + 20, topY + 44); using var alert = new SolidBrush(Color.FromArgb(255, 246, 245)); g.FillRectangle(alert, sx + 20, topY + 76, sideW - 40, 96); DrawText(g, "!   Facture INV-2023-089", labelFont, new SolidBrush(red), sx + 32, topY + 92); DrawText(g, "Client XYZ · Retard de 45 jours.", bodyFont, mutedBrush, sx + 32, topY + 120); DrawText(g, "Relance requise.", bodyFont, mutedBrush, sx + 32, topY + 142); using var bank = new SolidBrush(low); g.FillRectangle(bank, sx + 20, topY + 188, sideW - 40, 70); DrawText(g, "▣   Rapprochement bancaire", labelFont, inkBrush, sx + 32, topY + 202); DrawText(g, "5 transactions non lettrées ce mois-ci.", bodyFont, mutedBrush, sx + 32, topY + 230);
        var actY = topY + 340; DrawCard(g, new Rectangle(x0, actY, w, 260), border, 0); DrawText(g, "Dernières opérations", hFont, inkBrush, x0 + 20, actY + 20); DrawText(g, "TYPE", labelFont, mutedBrush, x0 + 20, actY + 66); DrawText(g, "LIBELLÉ", labelFont, mutedBrush, x0 + 130, actY + 66); DrawText(g, "MONTANT", labelFont, mutedBrush, x0 + w - 370, actY + 66); DrawText(g, "STATUT", labelFont, mutedBrush, x0 + w - 210, actY + 66); DrawText(g, "DATE", labelFont, mutedBrush, x0 + w - 90, actY + 66); var txs = engine.Transactions.Take(4).ToArray(); for (var i = 0; i < txs.Length; i++) { var y = actY + 94 + i * 38; using var line = new Pen(Color.FromArgb(226, 232, 240)); g.DrawLine(line, x0, y - 10, x0 + w, y - 10); DrawText(g, txs[i].Type, bodyFont, mutedBrush, x0 + 20, y); DrawText(g, txs[i].Type + " · " + txs[i].Nature, bodyFont, inkBrush, x0 + 130, y); DrawText(g, (txs[i].Type is "Achat" or "Décaissement" ? "- " : "+ ") + txs[i].AmountMad.ToString("N2") + " MAD", mono, inkBrush, x0 + w - 370, y); DrawBadge(g, "Payée", x0 + w - 210, y - 2, Color.FromArgb(137, 245, 231), Color.FromArgb(0, 80, 73), bodyFont); DrawText(g, txs[i].Date.ToString("dd/MM/yyyy"), bodyFont, mutedBrush, x0 + w - 90, y); }
    }
    private static void DrawText(Graphics g, string text, Font font, Brush brush, int x, int y) => g.DrawString(text, font, brush, x, y);
    private static void DrawBadge(Graphics g, string text, int x, int y, Color background, Color foreground, Font font) { using var measure = new Bitmap(1, 1); using var mg = Graphics.FromImage(measure); var size = mg.MeasureString(text, font); using var bg = new SolidBrush(background); g.FillRectangle(bg, x, y, (int)size.Width + 10, (int)size.Height + 4); using var fg = new SolidBrush(foreground); g.DrawString(text, font, fg, x + 5, y + 2); }
    private static void DrawCard(Graphics g, Rectangle rect, Color accent, int stripe) { using var bg = new SolidBrush(Color.White); g.FillRectangle(bg, rect); using var pen = new Pen(Color.FromArgb(197, 198, 205)); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1); if (stripe > 0) { using var brush = new SolidBrush(accent); g.FillRectangle(brush, rect.X, rect.Y, stripe, rect.Height); } }
}

internal sealed class CashflowChart : Control
{
    private readonly int[] inflows = { 72, 65, 84, 90, 93, 52, 90 };
    private readonly int[] outflows = { 20, 30, 14, 42, 11, 28, 16 };
    public CashflowChart() { DoubleBuffered = true; BackColor = Color.FromArgb(238, 244, 252); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        using var blueBrush = new SolidBrush(Color.FromArgb(82, 132, 235)); using var redBrush = new SolidBrush(Color.FromArgb(255, 218, 214)); using var textBrush = new SolidBrush(Color.FromArgb(22, 28, 34));
        var left = 20; var bottom = Height - 35; var width = Math.Max(24, (ClientSize.Width - 50) / 7 - 10); var max = 110;
        for (var i = 0; i < 7; i++) { var x = left + i * (width + 10); var inH = (int)((bottom - 18) * inflows[i] / (double)max); var outH = (int)((bottom - 18) * outflows[i] / (double)max); g.FillRectangle(blueBrush, x, bottom - inH - outH, width, inH); g.FillRectangle(redBrush, x, bottom - outH, width, outH); var day = new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" }[i]; using var f = new Font("Segoe UI", 8); var sz = g.MeasureString(day, f); g.DrawString(day, f, textBrush, x + (width - sz.Width) / 2, bottom + 8); }
    }
}

public sealed record Transaction(string Reference, DateTime Date, string Type, string Nature, string Party, string Currency, decimal Amount, decimal AmountMad)
{
    public string Label => $"{Type} · {Nature}";
    public string SearchText => $"{Reference} {Type} {Nature} {Party} {Currency} {Amount} {AmountMad}";
}

public sealed record TransactionImpact(string Kind, string Reference, string Status);
public sealed record TaxRule(string Code, DateTime Start, DateTime? End, decimal Rate, string RegulatoryReference);
public sealed record Company(string Code, string Name, string Ice, string TaxId, string Address);
public sealed record Party(string Code, string Name, string Kind, string TaxId, int PaymentDays);
public sealed record BankAccount(string Name, string Bank, string Iban, string Currency, decimal Balance);

public sealed class KayOneData
{
    public List<Transaction> Transactions { get; set; } = new();
    public Dictionary<string, List<TransactionImpact>> ImpactLedger { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public List<Party> Parties { get; set; } = new();
    public List<BankAccount> BankAccounts { get; set; } = new();
}

public sealed class KayOneDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    public string FilePath { get; }
    public KayOneData Data { get; private set; }

    public KayOneDataStore()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KayOne");
        Directory.CreateDirectory(folder);
        FilePath = Path.Combine(folder, "kayone-data.json");
        Data = LoadData();
    }

    private KayOneData LoadData()
    {
        try
        {
            if (File.Exists(FilePath)) return JsonSerializer.Deserialize<KayOneData>(File.ReadAllText(FilePath), JsonOptions) ?? new KayOneData();
        }
        catch (JsonException) { /* A damaged local file is recoverable by starting with a clean store. */ }
        return new KayOneData();
    }

    public void Save()
    {
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(Data, JsonOptions));
        File.Move(temp, FilePath, true);
    }
}

public sealed class TransactionEngine
{
    private readonly KayOneDataStore store;
    public List<Transaction> Transactions => store.Data.Transactions;
    public Dictionary<string, List<TransactionImpact>> ImpactLedger => store.Data.ImpactLedger;
    public IReadOnlyList<Company> Companies => store.Data.Companies;
    public IReadOnlyList<Party> Parties => store.Data.Parties;
    public IReadOnlyList<BankAccount> BankAccounts => store.Data.BankAccounts;
    public List<TaxRule> TaxRules { get; } = new() { new("TVA_STANDARD", new DateTime(2024, 1, 1), null, .20m, "CGI Maroc · paramétrable") };
    public Dictionary<string, decimal> ExchangeRates { get; } = new() { ["MAD"] = 1m, ["EUR"] = 10.22m, ["USD"] = 9.36m };
    public decimal TotalSales => Transactions.Where(t => t.Type is "Vente" or "Export").Sum(t => t.AmountMad) + 1824500;
    public decimal Receivables => Transactions.Where(t => t.Type is "Vente" or "Export").Sum(t => t.AmountMad) + 384200;
    public decimal Cash => 845600 + Transactions.Where(t => t.Type == "Encaissement").Sum(t => t.AmountMad) - Transactions.Where(t => t.Type == "Décaissement").Sum(t => t.AmountMad);
    public decimal VatDue => 68400 + Transactions.Sum(t => t.Type is "Vente" or "Export" ? t.AmountMad * VatRate(DateTime.Today) : -t.AmountMad * VatRate(DateTime.Today));
    public decimal NonDue => Receivables * .54m;
    public decimal Bucket30 => Receivables * .23m;
    public decimal Bucket60 => Receivables * .13m;
    public decimal Bucket90 => Receivables * .10m;
    public TransactionEngine()
    {
        store = new KayOneDataStore();
        if (Transactions.Count == 0)
        {
            Transactions.Add(new("V-2026-00458", DateTime.Today.AddDays(-2), "Vente", "Analyse laboratoire", "CL-000245 · Bio Santé", "MAD", 96800, 96800));
            Transactions.Add(new("A-2026-00122", DateTime.Today.AddDays(-4), "Achat", "Service", "FR-000318 · Atlas Maintenance", "MAD", 42500, 42500));
            Transactions.Add(new("ENC-2026-0081", DateTime.Today.AddDays(-6), "Encaissement", "Client", "CL-000192 · PharmaMed", "MAD", 120000, 120000));
        }
        if (store.Data.Companies.Count == 0) store.Data.Companies.Add(new("KAY-LAB", "KAY Laboratoires", "00123456789012", "ICE-001234567", "Agadir, Maroc"));
        if (store.Data.Parties.Count == 0) store.Data.Parties.AddRange(new[] { new Party("CL-000245", "Bio Santé", "Client", "IF-245", 30), new Party("FR-000318", "Atlas Maintenance", "Fournisseur", "IF-318", 45) });
        if (store.Data.BankAccounts.Count == 0) store.Data.BankAccounts.Add(new("Compte principal", "Attijariwafa bank", "MA640115190000012345678901", "MAD", 845600));
        foreach (var transaction in Transactions.Where(t => !ImpactLedger.ContainsKey(t.Reference))) ImpactLedger[transaction.Reference] = BuildImpacts(transaction);
        store.Save();
    }
    public decimal VatRate(DateTime date) => TaxRules.LastOrDefault(r => r.Code.StartsWith("TVA", StringComparison.OrdinalIgnoreCase) && r.Start <= date && (r.End is null || r.End >= date))?.Rate ?? 0m;
    public IReadOnlyList<TransactionImpact> GetImpacts(Transaction tx) => ImpactLedger.TryGetValue(tx.Reference, out var impacts) ? impacts : Array.Empty<TransactionImpact>();
    public Transaction Add(string type, string nature, string party, string currency, decimal amount)
    {
        var rate = ExchangeRates.TryGetValue(currency, out var configuredRate) ? configuredRate : 1m;
        var tx = new Transaction($"{Prefix(type)}-{DateTime.Now:yyyy}-{Transactions.Count + 1:0000}", DateTime.Now, type, nature, party, currency, amount, amount * rate);
        Transactions.Insert(0, tx);
        ImpactLedger[tx.Reference] = BuildImpacts(tx);
        store.Save();
        return tx;
    }
    private static List<TransactionImpact> BuildImpacts(Transaction tx)
    {
        var receivable = tx.Type is "Vente" or "Export" or "Encaissement";
        return new List<TransactionImpact>
        {
            new("Document", tx.Reference, "Généré"), new("Facture", tx.Reference, "Générée"), new(receivable ? "Créance · échéance" : "Dette · échéance", tx.Reference, "Ouverte"),
            new("Fiscalité · TVA / RAS", tx.Reference, "Règle paramétrable"), new("Écriture comptable", tx.Reference, "À valider"),
            new(receivable ? "Encaissement attendu" : "Décaissement prévu", tx.Reference, "Planifié"), new("Analytique", tx.Reference, "Rattachée"), new("Audit trail", tx.Reference, "Traçable")
        };
    }
    private static string Prefix(string type) => type switch { "Vente" or "Export" => "V", "Achat" or "Import" => "A", "Encaissement" => "ENC", "Décaissement" => "DEC", _ => "OP" };
}

