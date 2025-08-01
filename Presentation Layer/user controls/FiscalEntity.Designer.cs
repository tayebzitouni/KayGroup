namespace freelanceProject1
{
    partial class FiscalEntity
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
            label2 = new Label();
            label15 = new Label();
            label3 = new Label();
            label6 = new Label();
            label7 = new Label();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.BackColor = Color.Transparent;
            label14.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.Black;
            label14.Location = new Point(15, 9);
            label14.Name = "label14";
            label14.Size = new Size(86, 21);
            label14.TabIndex = 25;
            label14.Text = "Kay Group";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.DarkSlateGray;
            label2.Location = new Point(16, 37);
            label2.Name = "label2";
            label2.Size = new Size(64, 17);
            label2.TabIndex = 29;
            label2.Text = "Collectée:";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.BackColor = Color.Transparent;
            label15.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label15.ForeColor = Color.DarkSlateGray;
            label15.Location = new Point(77, 38);
            label15.Name = "label15";
            label15.Size = new Size(77, 15);
            label15.TabIndex = 30;
            label15.Text = "478.000,00";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(633, 25);
            label3.Name = "label3";
            label3.Size = new Size(81, 19);
            label3.TabIndex = 32;
            label3.Text = "4.920,00";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.DarkSlateGray;
            label6.Location = new Point(293, 40);
            label6.Name = "label6";
            label6.Size = new Size(77, 15);
            label6.TabIndex = 36;
            label6.Text = "478.000,00";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.Transparent;
            label7.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.DarkSlateGray;
            label7.Location = new Point(217, 38);
            label7.Name = "label7";
            label7.Size = new Size(77, 17);
            label7.TabIndex = 35;
            label7.Text = "Déductible :";
            // 
            // guna2Panel1
            // 
            guna2Panel1.BorderColor = Color.Silver;
            guna2Panel1.BorderRadius = 10;
            guna2Panel1.BorderThickness = 1;
            guna2Panel1.Controls.Add(label3);
            guna2Panel1.Controls.Add(label6);
            guna2Panel1.Controls.Add(label7);
            guna2Panel1.Controls.Add(label15);
            guna2Panel1.Controls.Add(label2);
            guna2Panel1.Controls.Add(label14);
            guna2Panel1.CustomizableEdges = customizableEdges1;
            guna2Panel1.Location = new Point(3, 2);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(641, 66);
            guna2Panel1.TabIndex = 38;
            guna2Panel1.Paint += guna2Panel1_Paint;
            // 
            // FiscalEntity
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(guna2Panel1);
            Name = "FiscalEntity";
            Size = new Size(657, 68);
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label14;
        private Label label2;
        private Label label15;
        private Label label3;
        private Label label6;
        private Label label7;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
    }
}
