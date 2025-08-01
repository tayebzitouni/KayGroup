using System;
using System.Drawing;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.forms
{
    // Main Form
    public partial class Form1 : Form
    {
        private DataGridView clientsDgv;
        private Button addClientBtn;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Key Finance - Gestion comptable";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            // Add sidebar (now a UserControl)
            this.Controls.Add(new SidebarUserControl());

            BuildHeader();
            BuildClientTable();
        }

        private void BuildHeader()
        {
            Label title = new Label
            {
                Text = "Liste des clients",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(270, 20),
                AutoSize = true
            };
            this.Controls.Add(title);
        }

        private void BuildClientTable()
        {
            clientsDgv = new DataGridView
            {
                Location = new Point(270, 60),
                Width = 1000,
                Height = 500,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray
            };

            // Header styling
            clientsDgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            clientsDgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            clientsDgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            clientsDgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            clientsDgv.EnableHeadersVisualStyles = false;

            // Cell styling
            clientsDgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            clientsDgv.DefaultCellStyle.BackColor = Color.White;
            clientsDgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 250);
            clientsDgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            clientsDgv.RowTemplate.Height = 30;
            clientsDgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            // Add columns
            clientsDgv.Columns.Add("Nom", "Nom");
            clientsDgv.Columns.Add("FiscalId", "Identifiant fiscal");
            clientsDgv.Columns.Add("VATStatus", "Statut TVA");
            clientsDgv.Columns.Add("Contact", "Contact");
            clientsDgv.Columns.Add("PaymentDelay", "Délai paiement");
            clientsDgv.Columns.Add("Exemption", "Exonération");

            // Add sample data
            clientsDgv.Rows.Add("IndustrielMaroc", "987654321", "Asugletti", "Samir Mansouri", "30 jours", "N/A");
            clientsDgv.Rows.Add("AgricultureDurable", "456789123", "Esonéré", "Fatima Zahra", "45 jours", "Utilisé: Limite: 320,000,00 NAD 500,000,00 NAD");
            clientsDgv.Rows.Add("ExportFruits", "789123456", "Asugletti", "Mohammed Berrada", "60 jours", "N/A");
            clientsDgv.Rows.Add("PharmaMed", "321654987", "Asugletti", "Leila Benjelloun", "15 jours", "N/A");

            this.Controls.Add(clientsDgv);

            // Add Client Button
            addClientBtn = new Button
            {
                Text = "Ajouter un client",
                Location = new Point(270, 570),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 112, 192),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addClientBtn.FlatAppearance.BorderSize = 0;
            addClientBtn.Click += AddClientBtn_Click;
            this.Controls.Add(addClientBtn);
        }

        private void AddClientBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonctionnalité 'Ajouter un client'");
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }
    }

    // Sidebar UserControl (can be edited visually in designer)
    public partial class SidebarUserControl : UserControl
    {
        public SidebarUserControl()
        {
            //InitializeComponent();
            this.Dock = DockStyle.Left;
            this.Width = 250;
            this.BackColor = Color.FromArgb(0, 56, 80);
            BuildSidebar();
        }

        private void BuildSidebar()
        {
            // Key Finance Title
            Label title = new Label
            {
                Text = "Key Finance",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(title);

            // Subtitle
            Label subtitle = new Label
            {
                Text = "Gestion comptable",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(20, 50),
                AutoSize = true
            };
            this.Controls.Add(subtitle);

            // Key Group Section
            Label groupTitle = new Label
            {
                Text = "Key Group",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 100),
                AutoSize = true
            };
            this.Controls.Add(groupTitle);

            // Menu Items
            string[] menuItems = {
                "Tableau de bord",
                "Fournisseurs",
                "Clients",
                "Factures",
                "Trésorerie",
                "Fiscal",
                "Paramètres"
            };

            int yPos = 130;
            foreach (var item in menuItems)
            {
                Label menuItem = new Label
                {
                    Text = "• " + item,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.White,
                    Location = new Point(30, yPos),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };
                menuItem.Click += (s, e) => { /* Handle menu clicks */ };
                this.Controls.Add(menuItem);
                yPos += 30;
            }
        }
    }
}