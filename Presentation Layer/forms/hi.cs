using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;
using System.Data;
using System.IO;
using HtmlAgilityPack;
using static Dtos.Dtos;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using Guna.UI2.WinForms;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text.RegularExpressions;
using System.Diagnostics;
using PdfiumViewer;

using ClosedXML.Excel;
using Tesseract;
using System.Globalization;


namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class hi : Form
    {
        List<Dtos.Dtos.Transaction> bankTxs = new();
        List<Transaction> systemTxs;


        public hi()
        {
            InitializeComponent();
        }




        private List<Dtos.Dtos.Transaction> LoadBankTransactionsFromExcel(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var transactions = new List<Dtos.Dtos.Transaction>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0]; // First worksheet

                for (int i = 1; i < table.Rows.Count; i++) // Skip header
                {
                    var row = table.Rows[i];

                    try
                    {
                        var code = row[0]?.ToString()?.Trim();
                        var dateStr = row[1]?.ToString()?.Trim();
                        var creditStr = row[3]?.ToString()?.Trim();
                        var debitStr = row[4]?.ToString()?.Trim();

                        // Handle amount
                        decimal amount = 0;
                        if (decimal.TryParse(creditStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var credit) && credit > 0)
                        {
                            amount = credit;
                        }
                        else if (decimal.TryParse(debitStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var debit) && debit > 0)
                        {
                            amount = debit;
                        }

                        // Try parse date
                        DateTime date;
                        string[] formats = { "dd MM yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "M/d/yyyy", "dd-MM-yyyy", "MM/dd/yyyy" };
                        bool parsed = false;

                        if (!string.IsNullOrWhiteSpace(dateStr))
                        {
                            parsed = DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                                     || DateTime.TryParse(dateStr, out date);
                        }
                        else
                        {
                            date = DateTime.MinValue;
                        }

                        if (!parsed)
                        {
                            MessageBox.Show(
                                $"⛔ التاريخ غير صالح في السطر {i + 1}:\n\"{dateStr}\"\n\nالرجاء تصحيح تنسيق التاريخ (مثلاً: dd/MM/yyyy أو yyyy-MM-dd).",
                                "خطأ في التاريخ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return null;
                        }

                        // Build transaction
                        var transaction = new Dtos.Dtos.Transaction
                        {
                            referenc = code,
                            Date = date,
                            Amount = amount
                        };

                        transactions.Add(transaction);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في السطر {i + 1}:\n\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }

            return transactions;
        }


        private async Task LoadCompteBancaire()
        {

            try
            {
                guna2DataGridView1.Rows.Clear();
                var users = await BussinessAcesssLayer.CompteBancaireBusinessLayer.GetAllComptes();

                foreach (var user in users)
                {
                    guna2DataGridView1.Rows.Add(
                    user.Intitule,
                    user.Banque,
                    user.entitename,
                    user.Agence,
                    user.RIB,
                    user.IBAN,
                    user.SwiftCode,
                    user.Devise,
                    user.DateOuverture,
                    user.EstActif
                );
                }



            }



            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }



        private void hi_Load(object sender, EventArgs e)
        {
           
            usefulFunction.UsefulFuncitonClass.LoadComboBoxOfAllComptesBancaires(guna2ComboBox2);
            guna2ComboBox2.SelectedIndex = -1;
           
            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView1.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            LoadCompteBancaire();
            usefulFunction.UsefulFuncitonClass.AttachEvents(this);
            guna2DateTimePicker1.FillColor = Color.White;
            guna2DateTimePicker1.BorderColor = Color.Black;
            guna2DateTimePicker1.HoverState.FillColor = Color.FromArgb(0, Color.White);
           




        }


        public List<Transaction> LoadSystemTransactionsFromDb(int compteId, DateTime dt)
        {
            using var db = new AppDbContext();

            var start = new DateTime(dt.Year, dt.Month, 1);
            var end = start.AddMonths(1); // أول يوم في الشهر التالي

            return db.payments
                .Where(p =>
                    p.Amount != null &&
                    p.comptebancaireId == compteId &&
                    p.PaymentDate >= start &&
                    p.PaymentDate < end)
                .Select(p => new Transaction
                {
                    Date = p.PaymentDate,
                    referenc = p.reference ?? "",
                    Amount = p.Amount,
                    Discriminator = EF.Property<string>(p, "Discriminator")
                })
                .ToList();
        }





        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }





        private void ReconcileTransactions()
        {
            var unmatchedList = new List<UnifiedTransaction>();
            var matchedFlags = new bool[bankTxs.Count];
            if (guna2ComboBox2.SelectedItem is CompteComboBoxDto gg)
            {
                systemTxs = LoadSystemTransactionsFromDb(gg.Id, guna2DateTimePicker1.Value);
            }
            else
            {
                MessageBox.Show("⚠️ Please select a bank account from the dropdown.");
                return;
            }
            foreach (var sys in systemTxs)
            {
                bool matched = false;

                for (int i = 0; i < bankTxs.Count; i++)
                {
                    var bank = bankTxs[i];

                    bool referenceMatch = string.Equals(
                        sys.referenc?.Trim(),
                        bank.referenc?.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    );

                    bool dateMatch = sys.Date.Date == bank.Date.Date;

                    bool amountMatch = Math.Round(sys.Amount, 2) == Math.Round(bank.Amount, 2);

                    if (!matchedFlags[i] && referenceMatch && dateMatch)
                    {
                        matchedFlags[i] = true;

                        if (amountMatch)
                        {
                            // تطابق كامل: لا نضيفه إلى القائمة
                            matched = true;
                            break;
                        }
                        else
                        {
                            // المرجع والتاريخ متطابقان لكن المبلغ مختلف
                            unmatchedList.Add(new UnifiedTransaction
                            {
                                Date = sys.Date,
                                reference = sys.referenc,
                                Amount = sys.Amount,
                                Status = "🟣 In System only",
                                Discriminator = sys.Discriminator,
                                Reason = $"💸 Different amount than bank: {bank.Amount}"
                            });

                            // نضيف أيضاً نسخة البنك لتكتمل المقارنة
                            unmatchedList.Add(new UnifiedTransaction
                            {
                                Date = bank.Date,
                                reference = bank.referenc,
                                Amount = bank.Amount,
                                Status = "🔵 In Bank only",
                                Discriminator = "-",
                                Reason = $"💸 Different amount than system: {sys.Amount}"
                            });

                            matched = true;
                            break;
                        }
                    }
                }

                if (!matched)
                {
                    unmatchedList.Add(new UnifiedTransaction
                    {
                        Date = sys.Date,
                        reference = sys.referenc,
                        Amount = sys.Amount,
                        Status = "🟣 In System only",
                        Discriminator = sys.Discriminator,
                        Reason = "❌ Not found in Excel"
                    });
                }
            }

            for (int i = 0; i < bankTxs.Count; i++)
            {
                if (!matchedFlags[i])
                {
                    var bank = bankTxs[i];

                    unmatchedList.Add(new UnifiedTransaction
                    {
                        Date = bank.Date,
                        reference = bank.referenc,
                        Amount = bank.Amount,
                        Status = "🔵 In Bank only",
                        Discriminator = "-",
                        Reason = "❌ Not found in DB"
                    });
                }
            }

            guna2DataGridView11.DataSource = unmatchedList;
            guna2DataGridView11.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            guna2HtmlLabel3.Text = unmatchedList.Count == 0
                ? "✅ Everything is perfect. All transactions are matched."
                : $"⚠️ Unmatched transactions: {unmatchedList.Count}";
        }


        private void guna2Button1_Click_1(object sender, EventArgs e)
        {

            var ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx|PDF Files|*.pdf"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(ofd.FileName).ToLower();

                if (ext == ".xlsx" || ext == ".xls")
                {
                    bankTxs = LoadBankTransactionsFromExcel(ofd.FileName);
                    if (bankTxs != null)
                    {
                        ReconcileTransactions();
                    }
                }
                else
                {
                    MessageBox.Show("Unsupported file format. Please select an Excel file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }



        }



        private async void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView1.Columns["Column1"].Index)
                {
                    string fiscalIdStr = guna2DataGridView1.Rows[e.RowIndex].Cells[4].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var userDto = await BussinessAcesssLayer.CompteBancaireBusinessLayer.GetCompteById(fiscalIdStr);
                        if (userDto != null)
                        {
                            var frm = new AddCompteBancaire(userDto);
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

                            await LoadCompteBancaire();

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


                else if (e.ColumnIndex == guna2DataGridView1.Columns["Column2"].Index)
                {
                    string fiscalIdStr = guna2DataGridView1.Rows[e.RowIndex].Cells[4].Value?.ToString();

                    if (!string.IsNullOrEmpty(fiscalIdStr))
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Fournisseur ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            string success = await BussinessAcesssLayer.CompteBancaireBusinessLayer.DeleteCompte(fiscalIdStr);
                            if (success == "Sucess")
                            {
                                MessageBox.Show("Fournisseur supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadCompteBancaire();

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
                Opacity = 0.5,
                Bounds = this.Bounds,
                TopMost = false,
                Owner = this
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

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2Button1_Click_2(object sender, EventArgs e)
        {
            AddCompteBancaire frm = new AddCompteBancaire();
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

            await LoadCompteBancaire();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView1, guna2TextBox2);
        }

        private void guna2DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 9);
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Button1_Click_1(sender, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
