using BussinessAcesssLayer;
using Guna.UI2.WinForms;
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

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddFournissuer : Form
    {
        private string oldFiscalIdentite = " ";
        private bool Add = true;
        private int id;
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 40;
        public AddFournissuer()
        {
            InitializeComponent();



        }

        public AddFournissuer(Dtos.Dtos.FournisseurDto client)
        {
            InitializeComponent();
            guna2TextBox1.Text = client.identifiantFiscal;
            guna2TextBox2.Text = client.Name;
            guna2TextBox4.Text = client.Contact;
            guna2TextBox6.Text = client.Email;
            label13.Text = " Complétez le formulaire pour Update Le fournisseur ";
            guna2TextBox3.Text = client.Phone;
            guna2TextBox7.Text = client.Rib;
            oldFiscalIdentite = guna2TextBox1.Text.Trim();
            id = client.id;
            guna2NumericUpDown1.Value = (decimal)client.TauxDeReturn;
            guna2NumericUpDown2.Value = client.delay;
            guna2ComboBox1.SelectedItem = client.StatusTVA;
            guna2ComboBox2.SelectedValue = client.entityId;
            Add = false;

            //guna2TextBox1.Enabled = false;
        }

        private void AddFournissuer_Load(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox2);


        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }


        private async void guna2Button1_Click(object sender, EventArgs e)
        {

            Dtos.Dtos.FournisseurDto temp = new Dtos.Dtos.FournisseurDto();


            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("Le Fiscal Identite ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else if (!BigInteger.TryParse(guna2TextBox1.Text, out _))
            {
                MessageBox.Show("Le Fiscal Identite doit être numérique.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else
            {
                temp.identifiantFiscal = guna2TextBox1.Text;
            }

            if (Add)
            {
                if (await BussinessAcesssLayer.FournisseurBussinesLayer.FournisseurExists(guna2TextBox1.Text.ToString().Trim()) == true)
                {
                    MessageBox.Show("Il Existe un Fournisseur With The same Identifiant Fiscal.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (BussinessAcesssLayer.FournisseurBussinesLayer.UpdateFournisseurExists(id, guna2TextBox1.Text.ToString()) == true)
                {
                    MessageBox.Show("Il Existe un Fournisseur With The same Identifiant Fiscal.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }


            if (string.IsNullOrWhiteSpace(guna2TextBox4.Text) || string.IsNullOrWhiteSpace(guna2TextBox2.Text))
            {
                MessageBox.Show("Le Nom  ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Name = guna2TextBox2.Text.ToString();
                temp.Contact = guna2TextBox4.Text.ToString();
            }


            if (guna2ComboBox1.SelectedItem.ToString() != null)
            {
                temp.StatusTVA = guna2ComboBox1.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner TVA Status.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown1, "Choose le Taux De Return TVA"))
            {
                temp.TauxDeReturn = Convert.ToDouble(guna2NumericUpDown1.Value);
            }
            else
            {
                temp.TauxDeReturn = 0;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown2, "Choose le Delai de paiement"))
            {
                temp.delay = Convert.ToInt32(guna2NumericUpDown2.Value);
            }
            else
            {
                return;
            }



            if (usefulFunction.UsefulFuncitonClass.CheckEmail(guna2TextBox6))
            {
                temp.Email = guna2TextBox6.Text.ToString();
            }
            else
            {
                return;
            }


            if (guna2ComboBox2.SelectedValue != null)
            {
                temp.entityId = Convert.ToInt32(guna2ComboBox2.SelectedValue);

            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (string.IsNullOrWhiteSpace(guna2TextBox7.Text))
            {
                MessageBox.Show("RIB ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Rib = guna2TextBox7.Text;
            }

            if (string.IsNullOrWhiteSpace(guna2TextBox3.Text))
            {
                MessageBox.Show("numero de Telephone ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Phone = guna2TextBox3.Text;
            }




            string a = " ";
            if (Add)
            {
                a = await BussinessAcesssLayer.FournisseurBussinesLayer.AddFournisserur(temp);
            }
            else
            {
                a = await BussinessAcesssLayer.FournisseurBussinesLayer.UpdateFournisseur(oldFiscalIdentite, temp);

            }
            if (a == "")
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
                MessageBox.Show(a, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }


        private void AddFournissuer_Load_1(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox2);
          
            this.FormBorderStyle = FormBorderStyle.None;
            
            this.Opacity = 0;

            this.Width = 550;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;

        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);

        //    this.Opacity = 0;

        //    // ✅ Timer avec nom complet pour éviter le conflit
        //    System.Windows.Forms.Timer fade = new System.Windows.Forms.Timer();
        //    fade.Interval = 20;
        //    fade.Tick += (s, ev) =>
        //    {
        //        if (this.Opacity < 1)
        //            this.Opacity += 0.05;
        //        else
        //            fade.Stop();
        //    };
        //    fade.Start();
        //}

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
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }

        // Coins arrondis
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int left, int top, int right, int bottom, int width, int height);


        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public event EventHandler AnnulerClicked;

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (this.Owner is Fournisseur parentForm)
            {
                parentForm.HideOverlay(); // ✅ Appelle directement la méthode du parent
            }
            // Fermer la fenêtre
            this.Close();

        }

        private void guna2TextBox2_Enter(object sender, EventArgs e)
        {

        }

        private void Input_Focus(object sender, EventArgs e)
        {
            if (sender is Guna.UI2.WinForms.Guna2TextBox tb)
            {
                tb.BorderThickness = 2;

            }
            else if (sender is Guna.UI2.WinForms.Guna2ComboBox cb)
            {
                cb.BorderThickness = 2;

            }
        }

        private void Input_LostFocus(object sender, EventArgs e)
        {
            if (sender is Guna.UI2.WinForms.Guna2TextBox tb)
            {
                tb.BorderThickness = 1;
                // or default
            }
            else if (sender is Guna.UI2.WinForms.Guna2ComboBox cb)
            {
                cb.BorderThickness = 1;

            }
        }




        private void guna2TextBox2_Click(object sender, EventArgs e)
        {
            guna2TextBox2.BorderThickness = 2;
        }

        private void guna2TextBox2_Leave(object sender, EventArgs e)
        {
            guna2TextBox2.BorderThickness = 1;
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_Enter(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_Click(object sender, EventArgs e)
        {
            guna2ComboBox1.BorderThickness = 2;
        }

        private void guna2TextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_ClientSizeChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_Click(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 2;
        }

        private void guna2TextBox4_Click(object sender, EventArgs e)
        {
            guna2TextBox4.BorderThickness = 2;
        }

        private void guna2TextBox3_Click(object sender, EventArgs e)
        {
            guna2TextBox3.BorderThickness = 2;
        }

        private void guna2TextBox6_Click(object sender, EventArgs e)
        {
            guna2TextBox6.BorderThickness = 2;
        }

        private void guna2ComboBox2_Enter(object sender, EventArgs e)
        {
            guna2ComboBox2.BorderThickness = 2;
        }

        private void guna2ComboBox2_Click(object sender, EventArgs e)
        {
            guna2ComboBox2.BorderThickness = 2;
        }

        private void guna2TextBox7_Enter(object sender, EventArgs e)
        {
            guna2TextBox7.BorderThickness = 2;
        }

        private void guna2TextBox7_Click(object sender, EventArgs e)
        {
            guna2TextBox7.BorderThickness = 2;
        }

        private void guna2NumericUpDown1_Click(object sender, EventArgs e)
        {
            guna2NumericUpDown1.BorderThickness = 2;
        }

        private void guna2NumericUpDown1_Enter(object sender, EventArgs e)
        {
            guna2NumericUpDown1.BorderThickness = 2;
        }

        private void guna2TextBox1_Leave(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 1;
        }

        private void guna2ComboBox1_Leave(object sender, EventArgs e)
        {
            guna2ComboBox1.BorderThickness = 1;
        }

        private void guna2TextBox4_Leave(object sender, EventArgs e)
        {
            guna2TextBox4.BorderThickness = 1;
        }

        private void guna2TextBox3_Leave(object sender, EventArgs e)
        {
            guna2TextBox3.BorderThickness = 1;
        }

        private void guna2TextBox6_Load(object sender, EventArgs e)
        {

        }

        private void guna2TextBox6_Leave(object sender, EventArgs e)
        {
            guna2TextBox6.BorderThickness = 1;
        }

        private void guna2ComboBox2_Leave(object sender, EventArgs e)
        {
            guna2ComboBox2.BorderThickness = 1;
        }

        private void guna2TextBox7_Leave(object sender, EventArgs e)
        {
            guna2TextBox7.BorderThickness = 1;
        }

        private void guna2NumericUpDown1_Leave(object sender, EventArgs e)
        {
            guna2NumericUpDown1.BorderThickness = 1;
        }
    }
}

