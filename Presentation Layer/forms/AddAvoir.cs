using freelanceProject1.Presentation_Layer.usefulFunction;
using Guna.UI2.WinForms.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddAvoir : Form
    {
        private bool Add = true;
        private int oldFiscalIdentite = 0;
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 70;
        public AddAvoir()
        {
            InitializeComponent();
            guna2ComboBox7.SelectedIndex = 0;
        }
        public AddAvoir(DataAccessLayer.Models.Avoir av)
        {
            InitializeComponent();
            oldFiscalIdentite = av.id;
            // Set the type in the comboBox7
            if (av.type == "Client")
                guna2ComboBox7.SelectedIndex = 0;
            else
                guna2ComboBox7.SelectedIndex = 1;

            // Load the corresponding clients or fournisseurs into comboBox1
            if (guna2ComboBox7.SelectedIndex == 0)
                usefulFunction.UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox1);
            else
                usefulFunction.UsefulFuncitonClass.loadcomboboxofFournisseurWithDataWithNoAsync(guna2ComboBox1);

            // Select the proper employee/client in comboBox1
            for (int i = 0; i < guna2ComboBox1.Items.Count; i++)
            {
                if (guna2ComboBox1.Items[i] is Dtos.Dtos.ClientDto client && client.Name == av.name)
                {
                    guna2ComboBox1.SelectedIndex = i;
                    break;
                }
                else if (guna2ComboBox1.Items[i] is Dtos.Dtos.FournisseurDto fournisseur && fournisseur.Name == av.name)
                {
                    guna2ComboBox1.SelectedIndex = i;
                    break;
                }
            }

            // Fill the other controls
            guna2TextBox3.Text = av.numero;
            guna2DateTimePicker3.Value = av.date;
            guna2NumericUpDown4.Value = av.montant;
            guna2TextBox1.Text = av.reason;

            // Select status
            for (int i = 0; i < guna2ComboBox2.Items.Count; i++)
            {
                if (guna2ComboBox2.Items[i].ToString() == av.status)
                {
                    guna2ComboBox2.SelectedIndex = i;
                    break;
                }
            }

            // This form is in edit mode
            Add = false;
        }


        private async void guna2Button7_Click(object sender, EventArgs e)
        {
            Dtos.Dtos.AvoirDto temp = new Dtos.Dtos.AvoirDto();

            if (String.IsNullOrEmpty(guna2TextBox3.Text))
            {
                MessageBox.Show("Le Numero Avoir ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox7.SelectedIndex == -1)

            {
                MessageBox.Show("Veuillez sélectionner un type d'avoir Soit Client Soit Fournisseur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox1.SelectedIndex == -1)

            {
                MessageBox.Show("Veuillez sélectionner Le Client Or Fournisseur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2NumericUpDown4.Value <= 0)
            {
                MessageBox.Show("Le Montant Doit Etre Superieur A zero.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (String.IsNullOrEmpty(guna2TextBox1.Text))
            {
                MessageBox.Show("Le Raison ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox2.SelectedIndex == -1)

            {
                MessageBox.Show("Veuillez sélectionner Le Statut.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox1.SelectedItem is Dtos.Dtos.FournisseurDto tb1 )
            {
                temp.employeeid = tb1.id;
                temp.name =tb1.Name;
            }
            else
                  if (guna2ComboBox1.SelectedItem is Dtos.Dtos.ClientDto tb2)
            {
                temp.employeeid = tb2.id;
                temp.name = tb2.Name;
            }
            temp.numero = guna2TextBox3.Text;
            temp.date = guna2DateTimePicker3.Value;
            temp.type = guna2ComboBox7.SelectedItem.ToString();
            
           
            temp.montant = guna2NumericUpDown4.Value;
            temp.reason = guna2TextBox1.Text;
            temp.status = guna2ComboBox2.SelectedItem.ToString();

            string a = " ";
            if (Add)
            {
                a = await BussinessAcesssLayer.AvoirBusinessLayer.AddAvoir(temp);
            }
            else
            {
                    a = await BussinessAcesssLayer.AvoirBusinessLayer.UpdateAvoir(oldFiscalIdentite, temp);

            }
            if (a == "")
            {
                MessageBox.Show("Avoir ajouté avec succès", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
               
            }
            else
            {
                MessageBox.Show(a, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }

        private void AddAvoir_Load(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.ApplyTopMostFix(this);   
            this.SuspendLayout();
            UsefulFuncitonClass.AttachEvents(this);
            this.Opacity = 0;
            //this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(450, 130);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            // this.BackColor = Color.WhiteSmoke;
            this.Opacity = 100;

            //   this.Width = 600;
            //   this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            //  this.Top = 0;

            // Place au bord droit mais cachée
            // this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            //this.StartPosition = FormStartPosition.Manual;
            this.ResumeLayout(false);
            
        }

        private void guna2ComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox7.SelectedIndex == 1)
            {
                usefulFunction.UsefulFuncitonClass.loadcomboboxofFournisseurWithDataWithNoAsync(guna2ComboBox1);
            }
            else if (guna2ComboBox7.SelectedIndex == 0)
                usefulFunction.UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox1);
        }
        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    // 🔧 Position cible = coller à droite de l’écran
        //    targetLeft = Screen.PrimaryScreen.WorkingArea.Width - this.Width;

        //    // 🔁 Lancer l’animation slide+fade
        //    slideTimer = new System.Windows.Forms.Timer();
        //    slideTimer.Interval = 10;
        //    slideTimer.Tick += SlideIn;
        //    slideTimer.Start();
        //}
        //private void SlideIn(object sender, EventArgs e)
        //{
        //    // Avancer de droite vers la gauche
        //    if (this.Left > targetLeft)
        //    {
        //        this.Left -= slideSpeed;
        //        if (this.Opacity < 1)
        //            this.Opacity += 0.05;
        //    }
        //    else
        //    {
        //        // Fin animation
        //        this.Left = targetLeft;
        //        this.Opacity = 1;
        //        slideTimer.Stop();
        //    }
        //}
        //protected override void OnShown(EventArgs e)
        //{
        //    base.OnShown(e);
        //    this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        //}

        // Coins arrondis
        //[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        //private static extern IntPtr CreateRoundRectRgn(
        //    int left, int top, int right, int bottom, int width, int height);
    }
}
