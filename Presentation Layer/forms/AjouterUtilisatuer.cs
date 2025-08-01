using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AjouterUtilisatuer : Form
    {
        bool Add = true; string email = "";
        public AjouterUtilisatuer()
        {
            InitializeComponent();

        }

        public AjouterUtilisatuer(Utilisatuer user) : this()
        {
            if (user != null)
            {
                Add = false;

                guna2TextBox1.Text = user.Email;
                guna2TextBox2.Text = user.Name;
                email = user.Email;
                guna2TextBox3.Text = user.Password;
                guna2TextBox4.Text = user.Role;
                guna2TextBox5.Text = user.phone;
                if (user.EntityId != 0)
                {
                    guna2ComboBox2.SelectedValue = user.EntityId;
                }

                guna2ComboBox2.SelectedText = user.Role;

            }
        }



        private void AjouterUtilisatuer_Load(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox2);
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            if (Add)
            {
                if (await BussinessAcesssLayer.UtilisatuerBussiness.UserExists(guna2TextBox1.Text.ToString()) == true)
                {
                    MessageBox.Show("Il Existe un Utilisateur With The same Email.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (BussinessAcesssLayer.UtilisatuerBussiness.IsValidEmail(guna2TextBox1.Text.ToString()) == false)
            {
                MessageBox.Show("Your Email doesnt respect the standard email Format", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (guna2TextBox3.Text.Length < 6)
            {
                MessageBox.Show("Le mot de passe doit contenir au moins 6 caractères.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            Dtos.Dtos.UtilisatuerDto temp = new Dtos.Dtos.UtilisatuerDto();
            if (string.IsNullOrWhiteSpace(guna2TextBox2.Text.ToString()))
            {
                MessageBox.Show("Le nom ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Name = guna2TextBox2.Text.ToString();
            }
            temp.Email = guna2TextBox1.Text.ToString();
            temp.Password = guna2TextBox3.Text.ToString();
            if (guna2ComboBox2.SelectedValue != null)
            {
                temp.EntityId = Convert.ToInt32(guna2ComboBox2.SelectedValue);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une entité.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (!string.IsNullOrWhiteSpace(guna2TextBox4.Text))
            {
                temp.Role = guna2TextBox4.Text;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un rôle.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(guna2TextBox5.Text))
            {
                temp.phone = guna2TextBox5.Text;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner le numero de Telephone.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool b;
            if (Add)
            {
                b = await BussinessAcesssLayer.UtilisatuerBussiness.AddUtilisateur(temp);
            }
            else
            {
                b = await BussinessAcesssLayer.UtilisatuerBussiness.UpdateUtilisateur(email, temp);

            }
            if (b)
            {
                MessageBox.Show("Opeartion end Succefully", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("There is Eroor Please check the information of the User.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            this.Close();
        }


        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
