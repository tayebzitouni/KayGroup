using Guna.UI2.WinForms;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AjouterEntity : Form
    {
        private bool Add = true;
        string name = "";// Variable to determine if we are adding or editing an entity

        public AjouterEntity()
        {
            InitializeComponent();

        }

        public AjouterEntity(Dtos.Dtos.EntityDto entity)
        {
            InitializeComponent();
            Add = false;
            guna2Button3.Text = "Update Entity";

            if (entity != null)
            {
                name = entity.Name;

                guna2TextBox2.Text = entity.Name;                // Name
                guna2TextBox5.Text = entity.code;                // Code
                guna2TextBox1.Text = entity.identifiantfiscal;   // IF
                guna2TextBox8.Text = entity.Adress;              // Address
                guna2TextBox7.Text = entity.RC;                  // RC
                guna2TextBox10.Text = entity.ICE;                // ICE
                guna2TextBox9.Text = entity.CNSS;                // CNSS
                guna2TextBox4.Text = entity.Nom;                 // Contact Name
                guna2TextBox6.Text = entity.Email;               // Email
                guna2TextBox3.Text = entity.Phone;
                guna2TextBox11.Text = entity.Patent;// Phone
            }
        }


        private void AjouterEntity_Load(object sender, EventArgs e)
        {
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            //if (Add)
            //{
            //    if ( BussinessAcesssLayer.EntityBussiness.EntityExiste(guna2TextBox2.Text.ToString().Trim()) != null)
            //    {
            //        MessageBox.Show("Il Existe un entity With The same Name.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }


            //}

            Dtos.Dtos.EntityDto temp = new Dtos.Dtos.EntityDto();
            if (string.IsNullOrWhiteSpace(guna2TextBox2.Text.ToString()))
            {
                MessageBox.Show("Le nom ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Name = guna2TextBox2.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox5.Text.ToString()))
            {
                MessageBox.Show("Le Code ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.code = guna2TextBox5.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox1.Text.ToString()))
            {
                MessageBox.Show("Le Identifiant Fiscal ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.identifiantfiscal = guna2TextBox1.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox8.Text.ToString()))
            {
                MessageBox.Show("Le  Adress ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Adress = guna2TextBox8.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox7.Text.ToString()))
            {
                MessageBox.Show("Le  RC ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.RC = guna2TextBox7.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox10.Text.ToString()))
            {
                MessageBox.Show("Le ICE ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.ICE = guna2TextBox10.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox9.Text.ToString()))
            {
                MessageBox.Show("Le CNSS ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.CNSS = guna2TextBox9.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox4.Text.ToString()))
            {
                MessageBox.Show("Le Nom de Contact ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Nom = guna2TextBox4.Text.ToString();
            }
            if (!usefulFunction.UsefulFuncitonClass.CheckEmail(guna2TextBox6))
            {
                return;
            }
            else
            {
                temp.Email = guna2TextBox6.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox3.Text.ToString()))
            {
                MessageBox.Show("Le Phone ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Phone = guna2TextBox3.Text.ToString();
            }
            if (string.IsNullOrWhiteSpace(guna2TextBox11.Text.ToString()))
            {
                MessageBox.Show("Le Patent ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                temp.Patent = guna2TextBox11.Text.ToString();
            }

            bool b;
            if (Add)
            {
                b = await BussinessAcesssLayer.EntityBussiness.AddEntity(temp);
            }
            else
            {
                b = await BussinessAcesssLayer.EntityBussiness.UpdateEntituy(name, temp);
            }


            if (b)
            {
                MessageBox.Show("Opeartion end Succefully", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("There is Error Please check the information of the Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

            this.Close();

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Button1_Click( sender,  e);
        }
    }
}

