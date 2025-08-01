using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.forms
{




    public partial class CLientsForm : Form
    {
        public CLientsForm()
        {
            InitializeComponent();

            LoadClientsIntoGrid();

            guna2DataGridView11.EnableHeadersVisualStyles = false;
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView11.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
        }


        private static Form overlayForm;

        private void ShowOverlay()
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


        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView11.Columns["Column1"].Index)
                {
                    string fiscalIdStr = guna2DataGridView11.Rows[e.RowIndex].Cells[1].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var userDto = await BussinessAcesssLayer.ClientBussinesLayer.GetClientByIdentifiantFiscal(fiscalIdStr);
                        if (userDto != null)
                        {
                            //this.Enabled = false;
                            var frm = new AjouterUnClient(userDto);
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

                            LoadClientsIntoGrid();
                        }
                        else
                        {
                            MessageBox.Show("Client non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Client ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            bool success = await BussinessAcesssLayer.ClientBussinesLayer.DeleteClientByIdAsync(fiscalIdStr);
                            if (success)
                            {
                                MessageBox.Show("Client supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadClientsIntoGrid();

                            }
                            else
                            {
                                MessageBox.Show("Erreur lors de la suppression de l'Client..." +
                                    "This Client Has A payments And Factres please Delete it first", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }



        private async Task LoadClientsIntoGrid()
        {
            try
            {
                guna2DataGridView11.Rows.Clear();
                decimal a = 0;
                var users = await BussinessAcesssLayer.ClientBussinesLayer.GetAllClients();

                foreach (var user in users)
                {
                    string Exp = "N/A";
                    a = user.ExnUtiliser;
                    if (user.ExnUtiliser > user.ExnLimite)
                    {
                        a = user.ExnLimite;
                    }
                    if (user.StatusTVA == "Exonéré")
                    {
                        Exp = "Utiliser : " + a + "\n " + "Limite : " + user.ExnLimite;
                    }

                    guna2DataGridView11.Rows.Add(
                    user.Name,
                    user.identifiantFiscal,
                    user.StatusTVA,
                    user.Contact,

                    user.Email,
                    user.Phone,
                    user.DelayDePayment + " Days",
                    Exp
                );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            AjouterUnClient frm = new AjouterUnClient();
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

            await LoadClientsIntoGrid();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            guna2Button1_Click(sender, e);
        }

        private void guna2DataGridView11_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView11_CellContentClick(sender, e);
        }

        private void guna2DataGridView11_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView11_CellContentClick(sender, e);
        }

        private void guna2DataGridView11_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
        }

        private void guna2TextBox1_TextChanged_1(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView11, guna2TextBox1);
        }

        private void guna2TextBox1_Click(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 2;
        }

        private void guna2TextBox1_Leave(object sender, EventArgs e)
        {
            guna2TextBox1.BorderThickness = 1;
        }
    }
}
