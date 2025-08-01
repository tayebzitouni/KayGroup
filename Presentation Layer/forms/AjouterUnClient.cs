using freelanceProject1.Presentation_Layer.usefulFunction;
using Guna.UI2.WinForms;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AjouterUnClient : Form
    {
        private string oldFiscalIdentite = " ";
        private bool Add = true;
        private int id;
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 70;
        public AjouterUnClient()
        {
            this.SuspendLayout();
            InitializeComponent();
            this.ResumeLayout(false);
        }

        private async void AjouterUnClient_Load(object sender, EventArgs e)
        {
            // Place au bord droit mais cachée
            this.SuspendLayout();
            UsefulFuncitonClass.AttachEvents(this);
            this.Opacity = 0;
            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;
            await usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox1);
            this.FormBorderStyle = FormBorderStyle.None;
            // this.BackColor = Color.WhiteSmoke;
            this.Opacity = 100;

            this.Width = 600;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            // Place au bord droit mais cachée
            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;
            this.ResumeLayout(false);
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
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }

        // Coins arrondis
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int left, int top, int right, int bottom, int width, int height);


        public AjouterUnClient(Dtos.Dtos.ClientDto client)
        {
            InitializeComponent();
            guna2TextBox1.Text = client.identifiantFiscal.ToString();
            guna2TextBox4.Text = client.Name;
            guna2TextBox2.Text = client.Contact;
            guna2TextBox3.Text = client.Email;
            guna2TextBox5.Text = client.Phone;
            guna2NumericUpDown1.Value = client.DelayDePayment;
            guna2NumericUpDown2.Value = (decimal)client.ExnLimite;
            guna2ComboBox2.SelectedItem = client.StatusTVA;
            id = client.id;
            oldFiscalIdentite = client.identifiantFiscal;
            guna2ComboBox1.SelectedValue = client.entityId;
            Add = false;
            label13.Text = "Complétez le formulaire pour Update Le Client";
        }



        private void guna2TextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox4_MouseEnter(object sender, EventArgs e)
        {

        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {

            Dtos.Dtos.ClientDto temp = new Dtos.Dtos.ClientDto();


            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text))
            {
                MessageBox.Show("Le Fiscal Identite ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else if (!int.TryParse(guna2TextBox1.Text, out _))
            {
                MessageBox.Show("Le Fiscal Identite doit être numérique.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else
            {
                temp.identifiantFiscal = guna2TextBox1.Text.ToString();
            }

            if (Add)
            {
                if (await BussinessAcesssLayer.ClientBussinesLayer.ClientExists(guna2TextBox1.Text.ToString()) == true)
                {
                    MessageBox.Show("Il Existe un Client With The same Identifiant Fiscal.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                if (await BussinessAcesssLayer.ClientBussinesLayer.UpdateClientExists(id, guna2TextBox1.Text.ToString()))
                {
                    MessageBox.Show("Il Existe un Client With The same Identifiant Fiscal.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                temp.Name = guna2TextBox4.Text.ToString();
                temp.Contact = guna2TextBox2.Text.ToString();
            }


            if (guna2ComboBox2.SelectedItem.ToString() != null)
            {
                temp.StatusTVA = guna2ComboBox2.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner TVA Status.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown1, "Delay De payment"))
            {
                temp.DelayDePayment = Convert.ToInt32(guna2NumericUpDown1.Value);
            }
            else
            {
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.CheckEmail(guna2TextBox3))
            {
                temp.Email = guna2TextBox3.Text.ToString();
            }
            else
            {
                return;
            }


            if (guna2ComboBox1.SelectedValue != null)
            {
                temp.entityId = Convert.ToInt32(guna2ComboBox1.SelectedValue);

            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown2, "limite De l exoneration"))
            {
                if (temp.StatusTVA == "Exonéré")
                {
                    temp.ExnLimite = Convert.ToDecimal(guna2NumericUpDown2.Value);
                    temp.ExnUtiliser = Convert.ToDecimal(guna2NumericUpDown3.Value);
                }
                else
                {
                    temp.ExnLimite = 0;
                }
            }
            else
            {
                return;
            }

            temp.Phone = guna2TextBox5.Text.ToString();


            string a = " ";
            if (Add)
            {
                a = await BussinessAcesssLayer.ClientBussinesLayer.AddClient(temp);
            }
            else
            {
                a = await BussinessAcesssLayer.ClientBussinesLayer.UpdateClient(oldFiscalIdentite, temp);

            }
            if (a == "")
            {
                MessageBox.Show("Opeartion end Succefully", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox2.SelectedItem != null)
            {
                string selectedValue = guna2ComboBox2.SelectedItem.ToString().ToLower();

                if (selectedValue == "Exonéré")
                {
                    guna2NumericUpDown2.Enabled = false;
                }
                else
                {
                    guna2NumericUpDown2.Enabled = true;
                }
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (this.Owner is CLientsForm parentForm)
            {
                parentForm.HideOverlay(); // ✅ Appelle directement la méthode du parent
            }
            // Fermer la fenêtre
            this.Close();
        }
    }
}
