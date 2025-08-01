namespace freelanceProject1.Presentation_Layer.user_controls
{
    partial class Retard
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            guna2HtmlLabel4 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2HtmlLabel3 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2HtmlLabel2 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Panel1
            // 
            guna2Panel1.BackColor = Color.White;
            guna2Panel1.BorderColor = Color.FromArgb(224, 224, 224);
            guna2Panel1.BorderRadius = 5;
            guna2Panel1.BorderThickness = 1;
            guna2Panel1.Controls.Add(guna2HtmlLabel4);
            guna2Panel1.Controls.Add(guna2HtmlLabel3);
            guna2Panel1.Controls.Add(guna2HtmlLabel2);
            guna2Panel1.Controls.Add(guna2HtmlLabel1);
            guna2Panel1.CustomBorderColor = Color.Transparent;
            guna2Panel1.CustomizableEdges = customizableEdges1;
            guna2Panel1.Location = new Point(4, 3);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(463, 57);
            guna2Panel1.TabIndex = 0;
            guna2Panel1.Paint += guna2Panel1_Paint;
            // 
            // guna2HtmlLabel4
            // 
            guna2HtmlLabel4.BackColor = Color.Transparent;
            guna2HtmlLabel4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            guna2HtmlLabel4.ForeColor = Color.Black;
            guna2HtmlLabel4.Location = new Point(307, 22);
            guna2HtmlLabel4.Name = "guna2HtmlLabel4";
            guna2HtmlLabel4.Size = new Size(50, 17);
            guna2HtmlLabel4.TabIndex = 3;
            guna2HtmlLabel4.Text = "En retard";
            // 
            // guna2HtmlLabel3
            // 
            guna2HtmlLabel3.BackColor = Color.Transparent;
            guna2HtmlLabel3.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            guna2HtmlLabel3.ForeColor = Color.Black;
            guna2HtmlLabel3.Location = new Point(370, 22);
            guna2HtmlLabel3.Name = "guna2HtmlLabel3";
            guna2HtmlLabel3.Size = new Size(87, 17);
            guna2HtmlLabel3.TabIndex = 2;
            guna2HtmlLabel3.Text = "2.801,30 MAD";
            guna2HtmlLabel3.Click += guna2HtmlLabel3_Click;
            // 
            // guna2HtmlLabel2
            // 
            guna2HtmlLabel2.BackColor = Color.Transparent;
            guna2HtmlLabel2.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            guna2HtmlLabel2.ForeColor = Color.Gray;
            guna2HtmlLabel2.Location = new Point(12, 29);
            guna2HtmlLabel2.Name = "guna2HtmlLabel2";
            guna2HtmlLabel2.Size = new Size(177, 19);
            guna2HtmlLabel2.TabIndex = 1;
            guna2HtmlLabel2.Text = "Service 1 pour Kay Consulting";
            // 
            // guna2HtmlLabel1
            // 
            guna2HtmlLabel1.BackColor = Color.Transparent;
            guna2HtmlLabel1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            guna2HtmlLabel1.Location = new Point(13, 7);
            guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            guna2HtmlLabel1.Size = new Size(59, 22);
            guna2HtmlLabel1.TabIndex = 0;
            guna2HtmlLabel1.Text = "FC-3475";
            guna2HtmlLabel1.Click += guna2HtmlLabel1_Click;
            // 
            // Retard
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CausesValidation = false;
            Controls.Add(guna2Panel1);
            Name = "Retard";
            Size = new Size(471, 65);
            Load += Retard_Load;
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel1;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel2;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel3;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel4;
    }
}
