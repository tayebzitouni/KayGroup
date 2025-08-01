using BusinessAccessLayer;
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
    public partial class Factures : Form
    {
        public Factures()
        {
            InitializeComponent();
            guna2Button5.FillColor = Color.Transparent;
            guna2Button4.FillColor = Color.White;
            guna2TabControl1.SelectedIndex = 0;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView1.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView2.EnableHeadersVisualStyles = false;
            guna2DataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView2.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView2.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
        }

        private void Factures_Load(object sender, EventArgs e)
        {
            LoadFacturesClientsIntoGrid();
            LoadFacturesFournisseursIntoGrid();
            guna2DataGridView1.CellPainting += (s, e) =>
            {
                usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(s, e, 8);
            };


            guna2DataGridView2.CellPainting += (s, e) =>
            {
                usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(s, e, 8);
            };


        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            Reset();

            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            clickedButton.ForeColor = Color.Black;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async Task LoadFacturesClientsIntoGrid()
        {
            try
            {
                guna2DataGridView1.Rows.Clear();

                var users = await BussinessAcesssLayer.FactureClientBussinesLayer.GetAllAsync();

                // Optional: Begin bulk update to reduce redraw
                guna2DataGridView1.SuspendLayout();

                foreach (var user in users)
                {
                    // Optimize status check
                    if (user.Status == "Non payé" && DateTime.Parse(user.DateEcheance) < DateTime.Today)
                    {
                        user.Status = "En retard";
                    }

                    string montantTH, tva, total;

                    if (user.devis == "MAD")
                    {
                        montantTH = $"{user.MontantTH} {user.devis}";
                        tva = $"{user.TVa} {user.devis}";
                        total = $"{user.Total} {user.devis}";
                    }
                    else
                    {
                        montantTH = $"{user.MontantTH}{user.devis} ={user.MontantTH*user.rate} MAD ";
                        tva = $"{user.TVa}{user.devis} ={user.TVa*user.rate} MAD ";
                        total = $"{user.Total}{user.devis} ={user.Total*user.rate} MAD ";
                    }

                    // Add row with all values
                    guna2DataGridView1.Rows.Add(
                        $"FC-{user.id}",
                        user.clientname,
                        user.entiteName,
                       
                        user.DateEmission,
                        user.DateEcheance,
                        montantTH,
                        tva,
                        total,
                        user.Status,
                        $"{user.payed} {user.devis}",
                        user.Description
                    );
                }

                // Optional: Resume layout after bulk update
                guna2DataGridView1.ResumeLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }


        private async Task LoadFacturesFournisseursIntoGrid()
        {
            try
            {
                guna2DataGridView2.Rows.Clear();

                var users = await FactureFournisseurBusinessLayer.GetAllAsync();

                // Optional: Suspend layout for better performance during bulk insert
                guna2DataGridView2.SuspendLayout();

                foreach (var user in users)
                {
                    // Update Status
                    if (user.Status == "Non payé" && DateTime.Parse(user.DateEcheance) < DateTime.Today)
                    {
                        user.Status = "En retard";
                    }
                    else if (user.payed >= user.Total)
                    {
                        user.Status = "payé";
                    }

                    // Format monetary values
                    string montantTH, tva, retenue, total;


                    if (user.devis == "MAD")
                    {
                        montantTH = $"{user.MontantTH} {user.devis}";
                        tva = $"{user.TVa} {user.devis}";
                        total = $"{user.Total} {user.devis}";
                        retenue = $"{user.Retenue} {user.devis}";
                    }
                    else
                    {
                        montantTH = $"{user.MontantTH}{user.devis} ={user.MontantTH*user.rate} MAD ";
                        tva = $"{user.TVa}{user.devis} ={user.TVa*user.rate} MAD ";
                        total = $"{user.Total}{user.devis} ={user.Total*user.rate} MAD ";
                        retenue = $"{user.Retenue}{user.devis} ={user.Retenue*user.rate} MAD ";
                    }


                    // Add row
                    guna2DataGridView2.Rows.Add(
                        $"FF-{user.id}",
                        user.fournisseurname,
                        user.entiteName,
                        user.DateReception,
                        user.DateEcheance,
                        montantTH,
                        tva,
                        retenue,
                        user.Status,
                        total,
                        $"{user.payed} ",
                        user.Description
                    );
                }

                // Optional: Resume layout
                guna2DataGridView2.ResumeLayout();
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
                TopMost = false,
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



        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Reset();
            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            clickedButton.ForeColor = Color.Black;
        }

        private void Reset()
        {
            guna2Button4.FillColor = Color.Transparent;
            guna2Button5.FillColor = Color.Transparent;
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            AddFacture frm = new AddFacture();

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

            await LoadFacturesClientsIntoGrid();
            await LoadFacturesFournisseursIntoGrid();

        }

        private async void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private async void guna2DataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView2.Columns["dataGridViewButtonColumn3"].Index)
                {
                    string factureName = guna2DataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                    if (temp > 0)
                    {
                        var userDto = await FactureFournisseurBusinessLayer.GetByIdAsync(temp);
                        userDto.id = temp; // Ensure the ID is set correctly
                        if (userDto != null)
                        {
                            AddFacture frm = new AddFacture(userDto);

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

                            await LoadFacturesFournisseursIntoGrid();
                        }
                        else
                        {
                            MessageBox.Show("Facture non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("facture Non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }


                else if (e.ColumnIndex == guna2DataGridView2.Columns["dataGridViewButtonColumn4"].Index)
                {
                    string factureName = guna2DataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                    if (temp > 0)
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Facture ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            bool success = await FactureFournisseurBusinessLayer.DeleteByIdAsync(temp);
                            if (success)
                            {
                                MessageBox.Show("Facture supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadFacturesFournisseursIntoGrid();

                            }
                            else
                            {
                                MessageBox.Show("Erreur lors de la suppression de l'Facture.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn1"].Index)
                {
                    string factureName = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                    if (temp > 0)
                    {
                        var userDto = await BussinessAcesssLayer.FactureClientBussinesLayer.GetByIdAsync(temp);
                        userDto.id = temp; // Ensure the ID is set correctly
                        if (userDto != null)
                        {
                            AddFacture frm = new AddFacture(userDto);

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

                            await LoadFacturesClientsIntoGrid();

                        }
                        else
                        {
                            MessageBox.Show("Facture non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Identifiant fiscal invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }


                else if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn2"].Index)
                {
                    string factureName = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                    if (temp > 0)
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Facture ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            bool success = await BussinessAcesssLayer.FactureClientBussinesLayer.DeleteByIdAsync(temp);
                            if (success)
                            {
                                MessageBox.Show("Facture supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadFacturesClientsIntoGrid();

                            }
                            else
                            {
                                MessageBox.Show("Erreur lors de la suppression de l'Facture.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }


        private void guna2Button4_Click_1(object sender, EventArgs e)
        {
            guna2Button5.FillColor = Color.Transparent;
            guna2Button4.FillColor = Color.White;
            guna2TabControl1.SelectedIndex = 0;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;

        }

        private void guna2Button5_Click_1(object sender, EventArgs e)
        {
            guna2Button5.FillColor = Color.White;
            guna2Button4.FillColor = Color.Transparent;
            guna2TabControl1.SelectedIndex = 1;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            guna2Button1_Click(sender, e);
        }


        private void guna2DataGridView1_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView1_CellContentClick_1(sender, e);
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView1, guna2TextBox2);
        }

        private void guna2DataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView2_CellContentClick(sender, e);
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView2, guna2TextBox1);
        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2TextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            guna2TextBox2.BorderThickness = 2;
        }

        private void guna2TextBox2_Leave(object sender, EventArgs e)
        {
            guna2TextBox2.BorderThickness = 1;
        }

        private void guna2DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {

        }
    }
}
