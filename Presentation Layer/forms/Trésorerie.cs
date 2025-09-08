using BussinessAcesssLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.user_controls;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class Trésorerie : Form
    {
        public Trésorerie()
        {
            InitializeComponent();

           

            guna2GroupBox5.Click += ActivateGroupBox5;
            label16.Click += ActivateGroupBox5;
            guna2PictureBox2.Click += ActivateGroupBox5;

            guna2GroupBox6.Click += ActivateGroupBox6;
            label21.Click += ActivateGroupBox6;
            guna2PictureBox3.Click += ActivateGroupBox6;

            guna2GroupBox7.Click += ActivateGroupBox7;
            label23.Click += ActivateGroupBox7;
            guna2PictureBox4.Click += ActivateGroupBox7;
        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void Trésorerie_Load(object sender, EventArgs e)
        {


            await LoadEntityControls();
            await LoadOverdueInvoices();
            await LoadOverdueInvoicesOfFournisseurs();
            guna2DataGridView3.CellPainting += guna2DataGridView3_CellPainting;
            guna2DataGridView4.CellPainting += guna2DataGridView4_CellPainting;
            decimal a = FactureClientBussinesLayer.GetTotalAmountOfNonPayeFacturesAsync();
            label10.Text = a.ToString() + " MAD";
            decimal b = FactureFournisseurBusinessLayer.GetTotalNonpayeFactureFacturesFourinsseurs();
            label106.Text = b.ToString() + " MAD";
            decimal c = await PaymentBussiness.BankSolde();
            label15.Text =c.ToString() + " MAD";
            label22.Text = (c + (a - b)).ToString() + " MAD";
        }


        private async Task LoadOverdueInvoices()
        {
            try
            {
                var overdue = await FactureClientBussinesLayer.GetOverdueFacturesAsync();
                if (overdue != null)
                {

                    guna2Panel1.Controls.Clear();


                    // Add each invoice
                    foreach (var invoice in overdue)
                    {

                        var invoiceControl = new Retard(invoice) // Your custom control
                        {

                            Dock = DockStyle.Top,
                            Margin = new Padding(10),
                            Height = 60
                        };

                        guna2Panel1.Controls.Add(invoiceControl);
                    }

                    guna2HtmlLabel5.Text = overdue.Count.ToString() + " facture(s) client en retard pour un montant total de " + overdue.Sum(f => (f.Total-f.payed)*f.rate).ToString() + " MAD";



                }

                guna2HtmlLabel5.Text = overdue.Count.ToString() + " facture(s) client en retard pour un montant total de " + overdue.Sum(f => (f.Total-f.payed)*f.rate).ToString() + " MAD";


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}",
                              "Erreur",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private async Task LoadOverdueInvoicesOfFournisseurs()
        {
            try
            {
            //    decimal a = FactureFournisseurBusinessLayer.GetTotalNonpayeFactureFacturesFourinsseurs2();
                var overdue = await FactureFournisseurBusinessLayer.GetOverdueFacturesAsync();
                if (overdue != null)
                {

                    guna2Panel4.Controls.Clear();


                    // Add each invoice
                    foreach (var invoice in overdue)
                    {

                        var invoiceControl = new Retard(invoice) // Your custom control
                        {

                            Dock = DockStyle.Top,
                            Margin = new Padding(10),
                            Height = 60
                        };

                        guna2Panel4.Controls.Add(invoiceControl);
                    }

                    guna2HtmlLabel6.Text = overdue.Count.ToString() + " facture(s) Fournisseur en retard pour un montant total de " + overdue.Sum(f => f.Total - f.payed).ToString() + " MAD";



                }

                guna2HtmlLabel6.Text = overdue.Count.ToString() + " facture(s) Fournisseur en retard pour un montant total de " + overdue.Sum(f => f.Total - f.payed).ToString() + " MAD";


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}",
                              "Erreur",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }


        private void guna2GroupBox25_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label106_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }



        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        // Méthode principale appelée à chaque clic
        private void click(Guna2GroupBox gp)
        {
            Reset();
            gp.CustomBorderColor = Color.White;
        }

        // Réinitialise les couleurs
        private void Reset()
        {
            guna2GroupBox5.CustomBorderColor = Color.Transparent;
            guna2GroupBox6.CustomBorderColor = Color.Transparent;
            guna2GroupBox7.CustomBorderColor = Color.Transparent;

            label23.ForeColor = Color.Gray;
            label16.ForeColor = Color.Gray;
            label21.ForeColor = Color.Gray;
        }

        // 4 méthodes d'activation
       

        private void ActivateGroupBox5(object sender, EventArgs e)
        {
            click(guna2GroupBox5);
            label16.ForeColor = Color.Black;
            guna2TabControl3.SelectedIndex = 2;
        }

        private void ActivateGroupBox6(object sender, EventArgs e)
        {
            click(guna2GroupBox6);
            label21.ForeColor = Color.Black;
            guna2TabControl3.SelectedIndex = 1;
        }

        private void ActivateGroupBox7(object sender, EventArgs e)
        {
            click(guna2GroupBox7);
            label23.ForeColor = Color.Black;
            guna2TabControl3.SelectedIndex = 0;
        }


        private async Task LoadEntityControls()
        {
            List<Dtos.Dtos.EntityDto> entities = await EntityBussiness.GetAllEntites(); // à adapter selon ton BLL

            foreach (var entity in entities)
            {
                var control = new trésorerieGeneral(entity);
                control.Dock = DockStyle.Top; // Ou autre : Bottom, Fill, etc.
                tabPage1.Controls.Add(control); // Assure-toi que tabPage1 est bien la bonne page
            }
        }







        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2GroupBox24_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }


        private async Task LoadFacturesFournisseursStatusIntoGrid()
        {

            decimal tot = 0;
            try
            {

                guna2DataGridView4.Rows.Clear();
                var users = await FactureFournisseurBusinessLayer.GetFacturesAvecStatutsAsync(true);

                foreach (var user in users)
                {
                    tot +=(user.total-user.payed)*user.payed;
                    guna2DataGridView4.Rows.Add(
                    user.Entite,
                    user.Client,
                    user.NumeroFacture,
                    user.DateEmission,
                    user.DateEcheance,
                    user.total-user.payed+" "+user.devis,
                    user.ModeDePayment,
                   "",
                    user.Statut
                    );
                }
                guna2HtmlLabel3.Text = tot.ToString() + " MAD";
                label25.Text = "Total factures en attente: " + users.Count.ToString();


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async Task LoadFacturesClientsStatusIntoGrid()
        {

            decimal tot = 0;
            try
            {

                guna2DataGridView3.Rows.Clear();
                var users = await BussinessAcesssLayer.FactureClientBussinesLayer.GetFacturesAvecStatutsAsync(true);

                foreach (var user in users)
                {
                    tot += (user.total-user.payed)*user.rate;
                    guna2DataGridView3.Rows.Add(
                    user.Entite,
                    user.Client,
                    user.NumeroFacture,
                    user.DateEmission,
                    user.DateEcheance,
                    user.total - user.payed+" "+user.devis,
                   "",
                    user.Statut
                    );
                }
                guna2HtmlLabel2.Text = tot.ToString() + " MAD";
                label3.Text = "Total factures en attente: " + users.Count.ToString();


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }


        private void guna2DataGridView4_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {




            if (e.ColumnIndex == guna2DataGridView4.Columns["dataGridViewTextBoxColumn32"].Index && e.RowIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                string statut = guna2DataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value?.ToString();

                if (string.IsNullOrEmpty(statut))
                    return;

                // Choisir la couleur du cercle
                Color color = Color.Gray;
                if (statut.StartsWith("À échéance"))
                    color = Color.Green;
                else if (statut.StartsWith("À échéance (in"))
                    color = Color.Orange;
                else if (statut.StartsWith("En Retard"))
                    color = Color.Red;
                else if (statut.StartsWith("À présenter"))
                    color = Color.FromArgb(59, 130, 246);

                // Dessiner un petit cercle coloré
                int diameter = 10;
                int x = e.CellBounds.Left + 5;
                int y = e.CellBounds.Top + (e.CellBounds.Height - diameter) / 2;

                using (Brush brush = new SolidBrush(color))
                {
                    e.Graphics.FillEllipse(brush, x, y, diameter, diameter);
                }
            }




            guna2DataGridView4.Columns["dataGridViewTextBoxColumn33"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            guna2DataGridView4.Columns["dataGridViewTextBoxColumn33"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            if (guna2DataGridView4.Columns[e.ColumnIndex].Name == "dataGridViewTextBoxColumn32" && e.RowIndex >= 0)
            {
                // Force left alignment for the Status column
                guna2DataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.Alignment =
                    DataGridViewContentAlignment.MiddleRight; // or DataGridViewContentAlignment.TopLeft
            }


            if (guna2DataGridView4.Columns[e.ColumnIndex].Name == "dataGridViewTextBoxColumn33" && e.RowIndex >= 0)
            {
                // Force left alignment for the Status column
                guna2DataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.Alignment =
                    DataGridViewContentAlignment.MiddleLeft; // or DataGridViewContentAlignment.TopLeft
            }
        }




        private void guna2DataGridView3_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == guna2DataGridView3.Columns["Column1"].Index && e.RowIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                string statut = guna2DataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value?.ToString();

                if (string.IsNullOrEmpty(statut))
                    return;

                // Choisir la couleur du cercle
                Color color = Color.Gray;
                if (statut.StartsWith("À échéance"))
                    color = Color.Green;
                else if (statut.StartsWith("À échéance (in"))
                    color = Color.Orange;
                else if (statut.StartsWith("En Retard"))
                    color = Color.Red;

                // Dessiner un petit cercle coloré
                int diameter = 10;
                int x = e.CellBounds.Left + 5;
                int y = e.CellBounds.Top + (e.CellBounds.Height - diameter) / 2;

                using (Brush brush = new SolidBrush(color))
                {
                    e.Graphics.FillEllipse(brush, x, y, diameter, diameter);
                }
            }
            //if (e.RowIndex >= 0)
            //    {
            //        string value = e.FormattedValue?.ToString();


            //        if (e.ColumnIndex == 0)
            //        {
            //            e.Handled = true;
            //            e.PaintBackground(e.CellBounds, true);
            //            Color bgColor = Color.Transparent;
            //            Color textColor = Color.Black;
            //            bgColor = Color.FromArgb(204, 239, 245); // Light blue
            //            textColor = Color.FromArgb(0, 57, 72);


            //            usefulFunction.UsefulFuncitonClass.DrawStyledCell(e, bgColor, textColor, Color.Transparent, false);
            //        }


            //    }



            guna2DataGridView3.Columns["dataGridViewTextBoxColumn27"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            guna2DataGridView3.Columns["dataGridViewTextBoxColumn27"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            if (guna2DataGridView3.Columns[e.ColumnIndex].Name == "Column1" && e.RowIndex >= 0)
            {
                // Force left alignment for the Status column
                guna2DataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.Alignment =
                    DataGridViewContentAlignment.MiddleRight; // or DataGridViewContentAlignment.TopLeft
            }


            if (guna2DataGridView3.Columns[e.ColumnIndex].Name == "dataGridViewTextBoxColumn27" && e.RowIndex >= 0)
            {
                // Force left alignment for the Status column
                guna2DataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.Alignment =
                    DataGridViewContentAlignment.MiddleLeft; // or DataGridViewContentAlignment.TopLeft
            }
        }



        private async void guna2TabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2TabControl3.SelectedIndex == 1)
            {
                await LoadFacturesClientsStatusIntoGrid();
                guna2DataGridView3.EnableHeadersVisualStyles = false;
                guna2DataGridView3.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
                guna2DataGridView3.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;


                guna2DataGridView3.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;

            }
            if (guna2TabControl3.SelectedIndex == 2)
            {
                await LoadFacturesFournisseursStatusIntoGrid();
                guna2DataGridView4.EnableHeadersVisualStyles = false;
                guna2DataGridView4.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
                guna2DataGridView4.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;


                guna2DataGridView4.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;

            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage9_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox10_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void sIdeBar1_Load(object sender, EventArgs e)
        {

        }
    }

}
