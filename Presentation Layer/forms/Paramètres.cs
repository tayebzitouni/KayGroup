using BussinessAcesssLayer;
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class Paramètres : Form
    {
        public Paramètres()
        {
            InitializeComponent();
            // sIdeBar1.MainFormReference = this;


            //guna2TabControl1.TabMenuVisible = true;


        }
        private Panel dimPanel;
        ///here i change
        private async void Paramètres_Load(object sender, EventArgs e)
        {
            ////  guna2TabControl1.ItemSize = new Size(0, 1);
            ////  guna2TabControl1.SizeMode = TabSizeMode.Fixed;
            //  guna2TabControl1.TabStop = false;
            await LoadUsersIntoGrid();

            guna2DataGridView11.EnableHeadersVisualStyles = false;
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;

            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView1.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;


            guna2TextBox6.Text = SettingsService.GetName();
            guna2NumericUpDown1.Value = int.Parse(SettingsService.GetAnnee());
            guna2NumericUpDown3.Value = SettingsService.GetTvaRate()*100;
            guna2NumericUpDown2.Value = SettingsService.GetIsRate()*100;

            await LoadEntitesIntoGrid();

        }




        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Button1_Click_1(sender, e);
        }

        private async void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == guna2DataGridView11.Columns["Column1"].Index && e.RowIndex >= 0)
            {
                string email = guna2DataGridView11.Rows[e.RowIndex].Cells[2].Value?.ToString();
                if (!string.IsNullOrEmpty(email))
                {

                    var userDto = await BussinessAcesssLayer.UtilisatuerBussiness.GetUserByEmail(email);

                    if (userDto != null)
                    {


                        var editForm = new AjouterUtilisatuer(userDto);
                        editForm.ShowDialog();
                        editForm.Close();


                        await LoadUsersIntoGrid();

                    }
                    else
                    {
                        MessageBox.Show("Utilisateur non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
            else if (e.ColumnIndex == guna2DataGridView11.Columns["Column2"].Index && e.RowIndex >= 0)
            {
                string email = guna2DataGridView11.Rows[e.RowIndex].Cells[2].Value?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet utilisateur ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        bool success = await BussinessAcesssLayer.UtilisatuerBussiness.DeleteUtilisateurByEmailAsync(email);
                        if (success)
                        {
                            MessageBox.Show("Utilisateur supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadUsersIntoGrid();
                        }
                        else
                        {
                            MessageBox.Show("Erreur This Utilisateur Has Payments please delete them first", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        private void guna2DataGridView11_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellStyle cell = new DataGridViewCellStyle();
            cell.BackColor = Color.Red;
            if (e.RowIndex > -1)
            {
                guna2DataGridView11.Rows[e.RowIndex].DefaultCellStyle = cell;
            }
        }



        private void guna2DataGridView11_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellStyle cell = new DataGridViewCellStyle();
            cell.BackColor = Color.Red;
            if (e.RowIndex >= 0)
            {
                guna2DataGridView11.Rows[e.RowIndex].DefaultCellStyle = cell;
            }
        }

        private void guna2DataGridView11_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCellStyle cell = new DataGridViewCellStyle();
            cell.BackColor = Color.White;
            if (e.RowIndex > -1)
            {
                guna2DataGridView11.Rows[e.RowIndex].DefaultCellStyle = cell;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            AjouterUtilisatuer form = new AjouterUtilisatuer();
            form.ShowDialog(this);
            LoadUsersIntoGrid();

        }

        private async Task LoadUsersIntoGrid()
        {
            try
            {
                guna2DataGridView11.Rows.Clear();
                var users = await BussinessAcesssLayer.UtilisatuerBussiness.GetAllUsers();

                foreach (var user in users)
                {
                    guna2DataGridView11.Rows.Add(
                        user.Id,
                        user.Name,
                        user.Email,
                        user.phone,
                        user.Password,
                        user.Role
                    );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }




        private async Task LoadEntitesIntoGrid()
        {

            try
            {
                guna2DataGridView1.Rows.Clear();
                var users = await BussinessAcesssLayer.EntityBussiness.GetAllEntites();

                foreach (var user in users)
                {
                    guna2DataGridView1.Rows.Add(
                        user.Id,
                        user.Name,
                        user.code,
                        user.Patent,
                        user.Adress,
                        user.identifiantfiscal,
                        user.RC,
                        user.ICE,
                        user.CNSS, user.Name,
                        user.Email,
                        user.Phone
                    );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private async void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn1"].Index && e.RowIndex >= 0)
            {
                string name = guna2DataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();
                if (!string.IsNullOrEmpty(name))
                {

                    var userDto = await BussinessAcesssLayer.EntityBussiness.EntityExiste(name);

                    if (userDto != null)
                    {



                        var editForm = new AjouterEntity(userDto);

                        editForm.ShowDialog(this);
                        editForm.Close();

                        await LoadEntitesIntoGrid();

                    }
                    else
                    {
                        MessageBox.Show("Entity non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
            else if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn2"].Index && e.RowIndex >= 0)
            {
                string email = guna2DataGridView1.Rows[e.RowIndex].Cells[1].Value?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Entity ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        bool success = await BussinessAcesssLayer.EntityBussiness.DeleteEntityByNameAsync(email);
                        if (success)
                        {
                            MessageBox.Show("Entity supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadEntitesIntoGrid();
                        }
                        else
                        {
                            MessageBox.Show("Erreur This Entity Has fournisseurs Clients factures...please delete them before", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private async void guna2Button3_Click(object sender, EventArgs e)
        {
            AjouterEntity entity = new AjouterEntity();
            entity.ShowDialog();
            await LoadEntitesIntoGrid();
        }

        private async void guna2GradientPanel4_Paint(object sender, PaintEventArgs e)
        {

        }


        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2GradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {



            // Change the clicked button's color
            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
        }



        private void guna2Button4_Click(object sender, EventArgs e)
        {

            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
        }







        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(guna2TextBox6.Text))
            {
                MessageBox.Show("Nom de l'entreprise ne peut pas être vide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
                if (guna2NumericUpDown1.Value == 0)
                {
                    MessageBox.Show("Veuillez sélectionner annee Fiscal", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (guna2NumericUpDown1.Value < 2025)
                {
                    MessageBox.Show("Veuillez sélectionner valide annee Fiscal >= 2025", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SettingsService.SetName(guna2TextBox6.Text);

                SettingsService.SetAnnee(guna2NumericUpDown1.Value.ToString());
                MessageBox.Show("✅ Vos changements ont bien été enregistrés !", "Sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            guna2Button2_Click(sender, e);
        }

        private void guna2DataGridView11_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView11_CellContentClick(sender, e);
        }

        private void guna2GradientPanel4_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button3_Click_1(object sender, EventArgs e)
        {
            guna2Button3_Click(sender, e);
        }

        private void guna2DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView1_CellContentClick(sender, e);
        }

        private void guna2Button1_Click_2(object sender, EventArgs e)
        {
            guna2Button1_Click_1(sender, e);
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
          
                if (guna2NumericUpDown3.Value <= 0)
                {
                    MessageBox.Show("Veuillez sélectionner TVA ", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (guna2NumericUpDown2.Value <= 0)
                {
                    MessageBox.Show("Veuillez sélectionner valide IS Taxe ", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SettingsService.SetTvaRate(guna2NumericUpDown3.Value/100);

                SettingsService.SetIsRate(guna2NumericUpDown2.Value/100);
                MessageBox.Show("✅ Vos changements ont bien été enregistrés !", "Sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            

        }
    }
}

