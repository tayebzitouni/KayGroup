namespace freelanceProject1
{
    partial class FiscaleEntityIs
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
            label14 = new Label();
            label3 = new Label();
            label5 = new Label();
            label2 = new Label();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            label1 = new Label();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.BackColor = Color.Transparent;
            label14.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.Black;
            label14.Location = new Point(8, 6);
            label14.Name = "label14";
            label14.Size = new Size(86, 21);
            label14.TabIndex = 26;
            label14.Text = "Kay Group";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(893, 10);
            label3.Name = "label3";
            label3.Size = new Size(81, 19);
            label3.TabIndex = 33;
            label3.Text = "4.920,00";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(981, 11);
            label5.Name = "label5";
            label5.Size = new Size(36, 19);
            label5.TabIndex = 35;
            label5.Text = "MAD";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.DarkSlateGray;
            label2.Location = new Point(8, 34);
            label2.Name = "label2";
            label2.Size = new Size(301, 17);
            label2.TabIndex = 36;
            label2.Text = "Provision impôt sur les sociétés (année courante) :";
            // 
            // guna2Panel1
            // 
            guna2Panel1.BorderColor = Color.Silver;
            guna2Panel1.BorderThickness = 1;
            guna2Panel1.Controls.Add(label1);
            guna2Panel1.Controls.Add(label2);
            guna2Panel1.Controls.Add(label5);
            guna2Panel1.Controls.Add(label14);
            guna2Panel1.Controls.Add(label3);
            guna2Panel1.CustomizableEdges = customizableEdges1;
            guna2Panel1.Location = new Point(11, 7);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(902, 64);
            guna2Panel1.TabIndex = 37;
            guna2Panel1.Paint += guna2Panel1_Paint;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(315, 35);
            label1.Name = "label1";
            label1.Size = new Size(294, 17);
            label1.TabIndex = 37;
            label1.Text = "Provision impôt sur les sociétés (année courante)";
            // 
            // FiscaleEntityIs
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(guna2Panel1);
            Name = "FiscaleEntityIs";
            Size = new Size(1063, 80);
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label14;
        private Label label3;
        private Label label5;
        private Label label2;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Label label1;
    }
}
