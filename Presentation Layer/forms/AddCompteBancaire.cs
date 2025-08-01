using Guna.UI2.WinForms;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddCompteBancaire : Form
    {
        private bool Add = true; private int id;private string enityid;
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 40;
        public AddCompteBancaire()
        {
            InitializeComponent();
        }
        public AddCompteBancaire(Dtos.Dtos.CompteBancaireDto compte)
        {
            InitializeComponent();
            Add = false;
            guna2TextBox2.Text = compte.Intitule;
            guna2TextBox1.Text = compte.Banque;
            guna2TextBox4.Text = compte.Agence;
            guna2TextBox3.Text = compte.IBAN;
            guna2TextBox6.Text = compte.RIB;
            guna2TextBox5.Text = compte.SwiftCode;
            enityid = compte.entitename;
            guna2NumericUpDown1.Value = compte.SoldeInitial;
            

            guna2ComboBox2.SelectedItem = compte.Devise;

            

            // sélectionne actif ou inactif
            if (compte.EstActif)
                guna2RadioButton1.Checked = true;
            else
                guna2RadioButton2.Checked = true;

            
            label13.Text = "Complétez le formulaire pour modifier le compte bancaire";
            id = compte.Id;
           
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            Dtos.Dtos.CompteBancaireDto temp = new Dtos.Dtos.CompteBancaireDto();


            if (string.IsNullOrWhiteSpace(guna2TextBox2.Text))
            {
                MessageBox.Show("Le Initule ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Intitule = guna2TextBox2.Text.ToString();
            }





            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text) || string.IsNullOrWhiteSpace(guna2TextBox2.Text))
            {
                MessageBox.Show("Le Nom de Banque ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Banque = guna2TextBox1.Text.ToString();

            }

            if (string.IsNullOrWhiteSpace(guna2TextBox4.Text) || string.IsNullOrWhiteSpace(guna2TextBox4.Text))
            {
                MessageBox.Show("L'Agence ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Agence = guna2TextBox4.Text.ToString();

            }
            if (string.IsNullOrWhiteSpace(guna2TextBox3.Text) || string.IsNullOrWhiteSpace(guna2TextBox3.Text))
            {
                MessageBox.Show("L'IBAN ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.IBAN = guna2TextBox3.Text.ToString();

            }

            if (string.IsNullOrWhiteSpace(guna2TextBox6.Text) || string.IsNullOrWhiteSpace(guna2TextBox6.Text))
            {
                MessageBox.Show("L'RIB ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.RIB = guna2TextBox6.Text.ToString();

            }
            if (string.IsNullOrWhiteSpace(guna2TextBox5.Text) || string.IsNullOrWhiteSpace(guna2TextBox5.Text))
            {
                MessageBox.Show("L'SwiftCode ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.SwiftCode = guna2TextBox5.Text.ToString();

            }





            var a = guna2ComboBox1.SelectedItem as Dtos.Dtos.EntityDto;
            if (a != null)
            {
                temp.EntiteId = a.Id;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Entite.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (guna2ComboBox2.SelectedItem != null)
            {
                temp.Devise = guna2ComboBox2.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Device.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown1, "Choose le Solde Initial"))
            {
                temp.SoldeInitial = Convert.ToDecimal(guna2NumericUpDown1.Value);
            }
            else
            {
                temp.SoldeInitial = 0;
            }

            if (Add)
            {
                temp.DateOuverture = DateTime.Today;
            }
            if (guna2RadioButton1.Checked)
            {
                temp.EstActif = true;
            }
            else if (guna2RadioButton2.Checked)
            {
                temp.EstActif = false;
            }










            string ab = " ";
            if (Add)
            {
                ab = await BussinessAcesssLayer.CompteBancaireBusinessLayer.AddCompte(temp);
            }
            else
            {
                ab = await BussinessAcesssLayer.CompteBancaireBusinessLayer.UpdateCompte(id, temp);

            }
            if (ab == "")
            {
                MessageBox.Show("Opeartion complete avec Sucess", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Task.Delay(1000);
                if (this.Owner is Fournisseur parentForm)
                {
                    parentForm.HideOverlay(); // ✅ Appelle directement la méthode du parent
                }
                // Fermer la fenêtre
                this.Close();
            }
            else
            {
                MessageBox.Show(ab, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 🔧 Position cible = coller à droite de l’écran
            targetLeft = Screen.PrimaryScreen.WorkingArea.Width - this.Width;

            // 🔁 Lancer l’animation slide+fade
            slideTimer = new System.Windows.Forms.Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideIn;
            slideTimer.Start();
        }
        private void SlideIn(object sender, EventArgs e)
        {
            // Avancer de droite vers la gauche
            if (this.Left > targetLeft)
            {
                this.Left -= slideSpeed;
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
            }
            else
            {
                // Fin animation
                this.Left = targetLeft;
                this.Opacity = 1;
                slideTimer.Stop();
                

            }
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.TopMost = true; // ✅ pour que le calendar s’affiche bien
            this.BringToFront();
            this.Focus();
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }

        // Coins arrondis
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int left, int top, int right, int bottom, int width, int height);
        private void AddCompteBancaire_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;

            this.Opacity = 0;

            this.Width = 550;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            this.Height = Screen.PrimaryScreen.Bounds.Height;

            this.StartPosition = FormStartPosition.Manual;
          
                usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox1);
            if (!Add)
            {
                for (int i = 0; i < guna2ComboBox1.Items.Count; i++)
                {
                    var item = guna2ComboBox1.Items[i];
                    if (item is Dtos.Dtos.EntityDto entity && entity.Name == enityid)
                    {
                        guna2ComboBox1.SelectedIndex = i;
                        break;
                    }
                }
            }
           
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (this.Owner is hi parentForm)
            {
                parentForm.HideOverlay(); 
            }
            
            this.Close();
        }
    }
}
