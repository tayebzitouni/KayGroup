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
    public partial class Avoir : Form
    {
        public Avoir()
        {
            InitializeComponent();
            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            Load();
        }

        private void guna2ContainerControl3_Click(object sender, EventArgs e)
        {

        }

        private async void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView11.Columns["Column1"].Index)
                {
                    string fiscalIdStr = guna2DataGridView11.Rows[e.RowIndex].Cells[0].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var userDto = await BussinessAcesssLayer.AvoirBusinessLayer.GetAvoirById(Convert.ToInt32(fiscalIdStr));
                        if (userDto != null)
                        {
                            var frm = new AddAvoir(userDto);
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

                            await Load();

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
                    string fiscalIdStr = guna2DataGridView11.Rows[e.RowIndex].Cells[0].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Avoir ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            bool success = await BussinessAcesssLayer.AvoirBusinessLayer.DeleteAvoirById(Convert.ToInt32(fiscalIdStr));
                            if (success)
                            {
                                MessageBox.Show("Avoir supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await Load();

                            }
                            else
                            {
                                MessageBox.Show("error", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
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

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private async Task Load()
        {
            try
            {
                guna2DataGridView11.Rows.Clear();
                decimal a = 0;
                var users = await BussinessAcesssLayer.AvoirBusinessLayer.GetAllAvoirs();

                foreach (var user in users)
                {


                    guna2DataGridView11.Rows.Add(
                    user.id,
                        user.numero,
                    user.date,
                    user.type,
                    user.name,
                    user.montant,
                    user.reason,
                    user.status

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
            AddAvoir frm = new AddAvoir();

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

            await Load();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView11, guna2TextBox1);
        }

        private void guna2ComboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filterCombobox(guna2DataGridView11, guna2ComboBox7);
        }
    }
}
