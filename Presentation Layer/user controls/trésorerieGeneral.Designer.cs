namespace freelanceProject1
{
    partial class trésorerieGeneral
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
            label2 = new Label();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            label6 = new Label();
            label14 = new Label();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Panel1
            // 
            guna2Panel1.BackColor = Color.Transparent;
            guna2Panel1.BorderColor = Color.FromArgb(224, 224, 224);
            guna2Panel1.BorderRadius = 10;
            guna2Panel1.BorderThickness = 1;
            guna2Panel1.Controls.Add(label2);
            guna2Panel1.Controls.Add(label4);
            guna2Panel1.Controls.Add(label3);
            guna2Panel1.Controls.Add(label1);
            guna2Panel1.Controls.Add(label6);
            guna2Panel1.Controls.Add(label14);
            guna2Panel1.CustomizableEdges = customizableEdges1;
            guna2Panel1.ForeColor = Color.Coral;
            guna2Panel1.Location = new Point(3, 12);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(907, 64);
            guna2Panel1.TabIndex = 0;
            guna2Panel1.Paint += guna2Panel1_Paint;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(0, 192, 0);
            label2.Location = new Point(754, 22);
            label2.Name = "label2";
            label2.RightToLeft = RightToLeft.No;
            label2.Size = new Size(132, 19);
            label2.TabIndex = 33;
            label2.Text = "1800000000000000000";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            label2.Click += label2_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI", 9.75F);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(292, 36);
            label4.Name = "label4";
            label4.Size = new Size(76, 17);
            label4.TabIndex = 32;
            label4.Text = "18000 MAD";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(230, 35);
            label3.Name = "label3";
            label3.Size = new Size(62, 17);
            label3.TabIndex = 30;
            label3.Text = "A payer :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 9.75F);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(91, 37);
            label1.Name = "label1";
            label1.Size = new Size(76, 17);
            label1.TabIndex = 29;
            label1.Text = "18000 MAD";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold);
            label6.ForeColor = Color.Black;
            label6.Location = new Point(19, 36);
            label6.Name = "label6";
            label6.Size = new Size(80, 17);
            label6.TabIndex = 28;
            label6.Text = "A recevoir : ";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.BackColor = Color.Transparent;
            label14.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.FromArgb(56, 135, 156);
            label14.Location = new Point(16, 11);
            label14.Name = "label14";
            label14.Size = new Size(86, 21);
            label14.TabIndex = 27;
            label14.Text = "Kay Group";
            // 
            // trésorerieGeneral
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel1);
            Name = "trésorerieGeneral";
            Size = new Size(918, 79);
            Load += trésorerieGeneral_Load;
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Label label14;
        private Label label4;
        private Label label3;
        private Label label1;
        private Label label6;
        private Label label2;
    }
}
