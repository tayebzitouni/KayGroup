using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class Fournisseur : Form
    {
        public Fournisseur()
        {
            InitializeComponent();

            LoadFournisseursIntoGrid();
            guna2DataGridView11.EnableHeadersVisualStyles = false;
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;


        }

        private async void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Fournisseur_Load(object sender, EventArgs e)
        {


        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async Task LoadFournisseursIntoGrid()
        {



            try
            {
                guna2DataGridView11.Rows.Clear();
                var users = await BussinessAcesssLayer.FournisseurBussinesLayer.GetAllFournisseurs();

                foreach (var user in users)
                {

                    guna2DataGridView11.Rows.Add(
                    user.Name,
                    user.identifiantFiscal,
                    user.StatusTVA,
                    user.Rib,
                    user.Contact,
                    user.Phone,
                    user.Email,
                    user.TauxDeReturn + " %",
                    user.delay + " Days"
                );
                }



            }



            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private static Form overlayForm;

        public void ShowOverlay()
        {
            // Créer un panneau semi-transparent sur tout le formulaire
            if (overlayForm != null)
            {
                overlayForm.Close();
                overlayForm.Dispose();
                overlayForm = null;
            }

            // Créer le formulaire de fond semi-transparent
            overlayForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.Black,
                Opacity = 0.5, // ✅ vrai effet semi-transparent
                Bounds = this.Bounds, // même taille que le parent
                TopMost = true,
                Owner = this // lie le form secondaire au parent
            };

            // Clic pour fermer (optionnel)
            overlayForm.Click += (s, e) =>
            {
                overlayForm.Close();
                overlayForm = null;
            };

            overlayForm.Show();
        }

        public void HideOverlay()
        {
            if (overlayForm != null)
            {
                this.Controls.Remove(overlayForm);
                overlayForm.Dispose();
                overlayForm = null;
            }
        }


        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            AddFournissuer frm = new AddFournissuer();
            ShowOverlay();
            frm.Owner = this;

            guna2Button1.Focus();
            frm.FormClosed += (s, ev) => HideOverlay();

            frm.FormClosed += (s, ev) =>
            {
                HideOverlay();
            };
            frm.ShowDialog();
            
            frm.FormClosed += (s, ev) => HideOverlay();

            frm.FormClosed += (s, ev) =>
            {
                HideOverlay();
            };

            await LoadFournisseursIntoGrid();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView11, guna2TextBox1);
        }




        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2DataGridView11_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView11.Columns["Column1"].Index)
                {
                    string fiscalIdStr = guna2DataGridView11.Rows[e.RowIndex].Cells[1].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var userDto = await BussinessAcesssLayer.FournisseurBussinesLayer.GetClientByIdentifiantFiscal(fiscalIdStr);
                        if (userDto != null)
                        {
                            var frm = new AddFournissuer(userDto);
                            ShowOverlay();
                            frm.Owner = this;

                            guna2Button1.Focus();
                            frm.FormClosed += (s, ev) => HideOverlay();

                            frm.FormClosed += (s, ev) =>
                            {
                                HideOverlay();
                            };
                            frm.ShowDialog();
                            frm.FormClosed += (s, ev) => HideOverlay();

                            frm.FormClosed += (s, ev) =>
                            {
                                HideOverlay();
                            };

                            await LoadFournisseursIntoGrid();
                           
                        }
                        else
                        {
                            MessageBox.Show("fournisseur non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Identifiant fiscal invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }


                else if (e.ColumnIndex == guna2DataGridView11.Columns["Column2"].Index)
                {
                    string fiscalIdStr = guna2DataGridView11.Rows[e.RowIndex].Cells[1].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Fournisseur ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            string success = await BussinessAcesssLayer.FournisseurBussinesLayer.DeleteFournisseurByIdAsync(fiscalIdStr);
                            if (success == "Sucess")
                            {
                                MessageBox.Show("Fournisseur supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadFournisseursIntoGrid();

                            }
                            else
                            {
                                MessageBox.Show(success, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void guna2DataGridView11_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_Enter(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 2;
        }

        private void guna2TextBox1_Leave(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 1;
        }
    }
}
