using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.user_controls;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
    public partial class SuiviCaisse : Form
    {
        DateTime from = new DateTime(DateTime.Now.Year, 1, 1);
        DateTime to = new DateTime(DateTime.Now.Year, 12, 31);
        public SuiviCaisse()
        {
            InitializeComponent();
            guna2DataGridView2.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView2.EnableHeadersVisualStyles = false;
            guna2DataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView2.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            guna2DataGridView4.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView4.EnableHeadersVisualStyles = false;
            guna2DataGridView4.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView4.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            LoadPaymentsUtilsateurFacturesIntoGrid();
            guna2DateTimePicker1.Value = from;
            guna2DateTimePicker2.Value = to;
            LoadEntityComptePayments();
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

        private async Task LoadPaymentsUtilsateurFacturesIntoGrid()
        {
            try
            {
                guna2DataGridView2.Rows.Clear();
                var users = await BussinessAcesssLayer.UtilisateurPaymentBussinessLayer.GetAllAsync();

                foreach (var user in users)
                {

                    guna2DataGridView2.Rows.Add(
                    "PU-" + user.PaymentId,
                    user.utilisateurname.ToString(),
                    user.entityName.ToString(),

                    user.ville,
                    user.Status,
                    user.compte,
                    user.PaymentDate.ToString("yyyy-MM-dd"),
                    user.MethodeDePayment,
                    user.Note,
                    user.reference,
                   user.Amount + " MAD",
                   user.debit + " " + user.devis,
                     user.RegisteredName,
                     user.months,
                     Properties.Resources.icons8_visible_20__1_,

                Properties.Resources.icons8_télécharger_24,
                 freelanceProject1.Properties.Resources.icons8_edit_20,
                     Properties.Resources.icons8_annuler_24

                    );
                }

                decimal totalThisMonth = UtilisateurPaymentBussinessLayer.GetTotalSoldeThisMonth();
                label28.Text = totalThisMonth.ToString() + " MAD";

                DateTime from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime to = DateTime.Now;

                List<Dtos.Dtos.VilleTotalDto> topVilles =
    UtilisateurPaymentBussinessLayer.GetTop3TotalsByVille(from, to);

                if (topVilles == null || topVilles.Count == 0)
                    return;



                // First
                if (topVilles.Count >= 1)
                {
                    label23.Text = topVilles[0].Ville;
                    label22.Text = topVilles[0].TotalAmount.ToString();
                }

                // Second
                if (topVilles.Count >= 2)
                {
                    label20.Text = topVilles[1].Ville;
                    label19.Text = topVilles[1].TotalAmount.ToString();
                }

                // Third
                if (topVilles.Count >= 3)
                {
                    label47.Text = topVilles[2].Ville;
                    label46.Text = topVilles[2].TotalAmount.ToString();
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
        private async void guna2DataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string factureName = guna2DataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();

                int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                if (e.ColumnIndex == guna2DataGridView2.Columns["Column3"].Index)
                {

                    List<PaymentDocument> documents = await PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);
                    if (temp > 0)
                    {
                        Dtos.Dtos.PaymentUtilisatuerDto userDto = await UtilisateurPaymentBussinessLayer.GetByIdAsync(temp);
                        userDto.PaymentId = temp;
                        if (userDto != null)
                        {
                            AddSuiviCaisse frm = new AddSuiviCaisse(userDto, documents);

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

                            await LoadPaymentsUtilsateurFacturesIntoGrid();
                            LoadEntityComptePayments();

                        }
                        else
                        {
                            MessageBox.Show("Payment non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Payment Non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }



                else if (e.ColumnIndex == guna2DataGridView2.Columns["Column1"].Index)
                {

                    if (temp > 0)
                    {
                        var documents = await BussinessAcesssLayer.PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);

                        if (documents.Count == 0)
                        {
                            MessageBox.Show("Aucun document trouvé pour ce paiement.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        string folder = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
      "KayGroupApp", "UploadedFiles"
  );


                        foreach (var doc in documents)
                        {
                            string fullPath = Path.Combine(folder, doc.FileName);

                            if (File.Exists(fullPath))
                            {
                                try
                                {
                                    // يفتح الملف بالبرنامج المناسب حسب نوعه
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                    {
                                        FileName = fullPath,
                                        UseShellExecute = true
                                    });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Erreur lors de l'ouverture du fichier {doc.FileName}: {ex.Message}");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Fichier introuvable: {doc.FileName}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }

                else if (e.ColumnIndex == guna2DataGridView2.Columns["Col2"].Index)
                {


                    if (temp > 0)
                    {
                        var documents = await BussinessAcesssLayer.PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);

                        if (documents.Count == 0)
                        {
                            MessageBox.Show("Aucun document trouvé pour ce paiement.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // اختيار مجلد الوجهة
                        using (var folderDialog = new FolderBrowserDialog())
                        {
                            folderDialog.Description = "Choisissez un dossier pour enregistrer les documents";
                            bool a = true;
                            if (folderDialog.ShowDialog() != DialogResult.OK)
                                return; // المستخدم ألغى

                            string targetFolder = folderDialog.SelectedPath;

                            foreach (var doc in documents)
                            {
                                string sourcePath = Path.Combine(
     Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
     "KayGroupApp", "UploadedFiles",
     doc.FileName
 );

                                string targetPath = Path.Combine(targetFolder, doc.FileName);

                                try
                                {
                                    if (File.Exists(sourcePath))
                                    {
                                        File.Copy(sourcePath, targetPath, overwrite: true);
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Fichier introuvable: {doc.FileName}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        a = false;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Erreur lors de la copie de {doc.FileName}: {ex.Message}");
                                    a = false;
                                }
                            }
                            if (a)
                            {
                                MessageBox.Show("Téléchargement terminé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                        }
                    }
                }
                else if (e.ColumnIndex == guna2DataGridView2.Columns["Col4"].Index)
                {


                    if (temp > 0)
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet utilisateur ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            var success = await BussinessAcesssLayer.UtilisateurPaymentBussinessLayer.DeleteAdvanceAsync(temp);
                            if (success.IsSuccess)
                            {
                                MessageBox.Show("Paiement supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadPaymentsUtilsateurFacturesIntoGrid();
                                LoadEntityComptePayments();


                            }
                            else
                            {
                                MessageBox.Show("Erreur lors de la suppression de Paiement.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            AddSuiviCaisse frm = new AddSuiviCaisse();

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



            LoadPaymentsUtilsateurFacturesIntoGrid();
            LoadEntityComptePayments();

        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void LoadEntityComptePayments()
        {
            var entityTotals = UtilisateurPaymentBussinessLayer.GetEntityCompteTotals(from, to);

            // Clear previous data
            guna2DataGridView4.Rows.Clear();

            // Loop through each entity and add to the DataGridView
            foreach (var dto in entityTotals)
            {
                guna2DataGridView4.Rows.Add(
                    dto.EntityName,
                    dto.CaisseP,
                    dto.Gasoil,
                    dto.Lavage,
                    dto.Deplac,
                    dto.Divers,
                    dto.DivSansBN,
                    dto.Entretien,
                    dto.Port,
                    dto.ACaisse
                );
            }

            // Optional: Add total row at the bottom
            guna2DataGridView4.Rows.Add(
                "TOTAL",
                entityTotals.Sum(x => x.CaisseP),
                entityTotals.Sum(x => x.Gasoil),
                entityTotals.Sum(x => x.Lavage),
                entityTotals.Sum(x => x.Deplac),
                entityTotals.Sum(x => x.Divers),
                entityTotals.Sum(x => x.DivSansBN),
                entityTotals.Sum(x => x.Entretien),
                entityTotals.Sum(x => x.Port),
                entityTotals.Sum(x => x.ACaisse)
            );
        }



        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            from = guna2DateTimePicker1.Value;
            LoadEntityComptePayments();
        }

        private void guna2DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            to = guna2DateTimePicker2.Value;
            LoadEntityComptePayments();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
