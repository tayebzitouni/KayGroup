using BussinessAcesssLayer;
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
    public partial class Reporting : Form
    {
        public Reporting()
        {
            InitializeComponent();
            guna2DataGridView11.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView1.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            LoadFournisseur();
            LoadClient();


        }

        private async Task LoadClient()
        {
            try
            {
                guna2DataGridView11.Rows.Clear();

                var clients = await FactureClientBussinesLayer.GetTotalClientWithDebits();

                foreach (var client in clients)
                {
                    foreach (var currency in client.ByCurrency)
                    {
                        guna2DataGridView11.Rows.Add(
                            client.UserName,        // client name
                            currency.Currency,         // total invoices in this currency
                            currency.Total,      // currency type (MAD, USD, EUR)
                            currency.Payee,                   // you can fill other columns if needed
                            currency.Restant       // remaining debit
                                                // status
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private async Task LoadFournisseur()
        {
            try
            {
                guna2DataGridView1.Rows.Clear();

                var clients = await FactureFournisseurBusinessLayer.GetTotalFournisserWithDebits();

                foreach (var client in clients)
                {
                    foreach (var currency in client.ByCurrency)
                    {
                        guna2DataGridView1.Rows.Add(
                            client.UserName,        // client name
                            currency.Currency,         // total invoices in this currency
                            currency.Total,      // currency type (MAD, USD, EUR)
                            currency.Payee,                   // you can fill other columns if needed
                            currency.Restant       // remaining debit
                                                   // status
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void guna2DataGridView11_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
