namespace freelanceProject1
{
    partial class tresoireRecevoire
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
            guna2Panel1.Location = new Point(7, 10);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(870, 78);
            guna2Panel1.TabIndex = 1;
            guna2Panel1.Paint += guna2Panel1_Paint;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Consolas", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Gray;
            label2.Location = new Point(297, 44);
            label2.Name = "label2";
            label2.Size = new Size(80, 18);
            label2.TabIndex = 36;
            label2.Text = "18000 MAD";
            label2.Click += label2_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.Gray;
            label4.Location = new Point(222, 45);
            label4.Name = "label4";
            label4.Size = new Size(79, 17);
            label4.TabIndex = 35;
            label4.Text = "Déductible: ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(723, 30);
            label3.Name = "label3";
            label3.Size = new Size(90, 19);
            label3.TabIndex = 34;
            label3.Text = "18000 MAD";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Consolas", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Gray;
            label1.Location = new Point(84, 45);
            label1.Name = "label1";
            label1.Size = new Size(80, 18);
            label1.TabIndex = 29;
            label1.Text = "18000 MAD";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.Gray;
            label6.Location = new Point(17, 45);
            label6.Name = "label6";
            label6.Size = new Size(65, 17);
            label6.TabIndex = 28;
            label6.Text = "Collectée:";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.BackColor = Color.Transparent;
            label14.Font = new Font("Segoe UI", 12.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.ForeColor = Color.FromArgb(39, 39, 42);
            label14.Location = new Point(16, 11);
            label14.Name = "label14";
            label14.Size = new Size(94, 23);
            label14.TabIndex = 27;
            label14.Text = "Kay Group";
            // 
            // tresoireRecevoire
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel1);
            Name = "tresoireRecevoire";
            Size = new Size(884, 98);
            Load += tresoireRecevoire_Load;
            guna2Panel1.ResumeLayout(false);
            guna2Panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Label label1;
        private Label label6;
        private Label label14;
        private Label label3;
        private Label label2;
        private Label label4;
    }
}
