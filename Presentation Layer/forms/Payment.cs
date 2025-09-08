using BussinessAcesssLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.user_controls;
using Guna.UI2.WinForms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Dtos.Dtos;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class Payment : Form
    {
        private List<PaymentFournisseurDto> _allPayments = new List<PaymentFournisseurDto>();
        // This stores (DataGridView, ColumnIndex) => Image
        private Dictionary<(DataGridView, int), Image> hoverConfigs = new();

        DateTime from = new DateTime(DateTime.Now.Year, 1, 1);
        DateTime to = new DateTime(DateTime.Now.Year, 12, 31);

        private void SetupHoverConfigs()
        {
            hoverConfigs[(guna2DataGridView1, 11)] = Properties.Resources.icons8_visible_20__1_;
            
            hoverConfigs[(guna2DataGridView3, 9)] = Properties.Resources.icons8_visible_20__1_;
            hoverConfigs[(guna2DataGridView1, 12)] = Properties.Resources.icons8_télécharger_24;
         
            hoverConfigs[(guna2DataGridView3, 10)] = Properties.Resources.icons8_télécharger_24;
            hoverConfigs[(guna2DataGridView1, 13)] = Properties.Resources.icons8_edit_20;
            
            hoverConfigs[(guna2DataGridView3, 11)] = Properties.Resources.icons8_edit_20;
            hoverConfigs[(guna2DataGridView1, 14)] = Properties.Resources.icons8_annuler_24;
            
            hoverConfigs[(guna2DataGridView3, 12)] = Properties.Resources.icons8_annuler_24;

        }

        private void DataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;

            var key = (dgv, e.ColumnIndex);
            if (hoverConfigs.TryGetValue(key, out Image originalImage))
            {
                var zoomedImage = new Bitmap(originalImage, new Size(originalImage.Width + 2, originalImage.Height + 2));
                dgv.Cursor = Cursors.Hand;
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = zoomedImage;
            }
        }

        private void DataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;

            var key = (dgv, e.ColumnIndex);
            if (hoverConfigs.TryGetValue(key, out Image originalImage))
            {
                dgv.Cursor = Cursors.Default;
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = originalImage;
            }
        }

        public Payment()
        {
            InitializeComponent();

            guna2DataGridView1.EnableHeadersVisualStyles = false;
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;

           
            guna2DataGridView3.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DataGridView3.EnableHeadersVisualStyles = false;
            guna2DataGridView3.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView3.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;

            guna2DataGridView1.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            guna2DateTimePicker1.Value = from;
            guna2DateTimePicker2.Value = to;




        }


        private async Task LoadPaymentsFournisseurFacturesIntoGrid()
        {
            try
            {
                guna2DataGridView1.Rows.Clear();
                var users = await BussinessAcesssLayer.PaymentFournisseurBusinessLayer.GetAllAsync ();

                foreach (var user in users)
                {
                    string montantTH;


                    if (user.devis == "MAD")
                    {
                        montantTH = $"{user.Amount} {user.devis}";
                     
                    }
                    else
                    {
                        montantTH = $"{user.Amount}{user.devis} ={user.Amount * user.rate} MAD ";
                      
                    }
                    guna2DataGridView1.Rows.Add(
                    "PF-" + user.PaymentId,
                     user.FactureName,
                    user.entityName.ToString(),
                    user.fournisseurName.ToString(),
                    user.compte.ToString(),
                    user.PaymentDate.ToString("yyyy-MM-dd"),
                  montantTH,
                    user.RegisteredName,

                    user.MethodeDePayment,
                    user.Note,
                    user.reference,
                     Properties.Resources.icons8_visible_20__1_,

                Properties.Resources.icons8_télécharger_24,
                 freelanceProject1.Properties.Resources.icons8_edit_20,
                     Properties.Resources.icons8_annuler_24

                    );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }




        private async Task LoadPaymentsFacturesIntoGrid()
        {
            try
            {
                guna2DataGridView3.Rows.Clear();
                var users = await BussinessAcesssLayer.PaymentBussiness.GetAllAsync(true);

                foreach (var user in users)
                {
                    string montantTH;


                    if (user.devis == "MAD")
                    {
                        montantTH = $"{user.Amount} {user.devis}";

                    }
                    else
                    {
                        montantTH = $"{user.Amount}{user.devis} ={user.Amount * user.rate} MAD ";

                    }
                    guna2DataGridView3.Rows.Add(
                    "PA-" + user.PaymentId,
                    user.RegisteredName.ToString() + "  ",
                    user.entityName.ToString(),
                    user.fullname,
                    user.PaymentDate.ToString("yyyy-MM-dd"),

montantTH                 , user.Note,
                   user.MethodeDePayment,
                   user.reference,
                     Properties.Resources.icons8_visible_20__1_,

                Properties.Resources.icons8_télécharger_24,
                 freelanceProject1.Properties.Resources.icons8_edit_20,
                     Properties.Resources.icons8_annuler_24

                    );
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }



        private async void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {

                if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn2"].Index)
                {
                    string factureName = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);
                    List<PaymentDocument> documents = await PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);
                    if (temp > 0)
                    {
                        Dtos.Dtos.PaymentFournisseurDto userDto = await PaymentFournisseurBusinessLayer.GetByIdAsync(temp);
                        userDto.PaymentId = temp;
                        if (userDto != null)
                        {
                            AddPayment frm = new AddPayment(userDto, documents);

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
                            await LoadPaymentsFournisseurFacturesIntoGrid();
                            LoadEntityPaymentControls();
                        }
                        else
                        {
                            MessageBox.Show("Paiement non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Paiement Non trouvé.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }



                else if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewTextBoxColumn18"].Index)
                {
                    string paymentname = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(paymentname);

                    if (temp > 0)
                    {
                        var documents = await BussinessAcesssLayer.PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);

                        if (documents.Count == 0)
                        {
                            MessageBox.Show("Aucun document trouvé pour ce paiement.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        //                   string folder = Path.Combine(
                        //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        //"KayGroupApp", "UploadedFiles"
                        string folder = Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                       "UploadedFiles"); // ✅ Remove "KayGroupApp"




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

                else if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewButtonColumn1"].Index)
                {
                    var cellValue = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value;
                    if (cellValue == null) return;

                    string paymentname = cellValue.ToString();
                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(paymentname);

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

                                string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UploadedFiles");
                                string sourcePath = Path.Combine(basePath, doc.FileName);

                                if (!File.Exists(sourcePath))
                                {
                                    MessageBox.Show($"Fichier introuvable: {sourcePath}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    a = false;
                                    continue;
                                }



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
                else if (e.ColumnIndex == guna2DataGridView1.Columns["Column2"].Index)
                {
                    string factureName = guna2DataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    string factureName2 = guna2DataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                    int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);
                    int temp2 = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName2);
                    if (temp > 0 && temp2 > 0)
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Paiement ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            var success = await BussinessAcesssLayer.PaymentFournisseurBusinessLayer.DeleteAsync(temp, temp2);
                            if (success.IsSuccess)
                            {
                                MessageBox.Show("Paiement supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadPaymentsFournisseurFacturesIntoGrid();
                                LoadEntityPaymentControls();

                            }
                            else
                            {
                                MessageBox.Show("Erreur lors de la suppression de l'Paiement.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }




        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Payment_Load(object sender, EventArgs e)
        {
            LoadPaymentsFournisseurFacturesIntoGrid();
          
            LoadPaymentsFacturesIntoGrid();
            LoadEntityPaymentControls();
            guna2DataGridView1.CellMouseEnter += DataGridView_CellMouseEnter;
            guna2DataGridView1.CellMouseLeave += DataGridView_CellMouseLeave;

           
            guna2DataGridView3.CellMouseEnter += DataGridView_CellMouseEnter;
            guna2DataGridView3.CellMouseLeave += DataGridView_CellMouseLeave;

            // Initialize configuration
            SetupHoverConfigs();
            usefulFunction.UsefulFuncitonClass.AttachEvents(guna2TabControl1);
            guna2DataGridView4.EnableHeadersVisualStyles = false;
            guna2DataGridView4.ColumnHeadersDefaultCellStyle.BackColor = Color.White; // Your preferred header color
            guna2DataGridView4.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
        }


        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            AddPayment frm = new AddPayment();

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


            LoadPaymentsFournisseurFacturesIntoGrid();
          
            LoadPaymentsFacturesIntoGrid();
            LoadEntityPaymentControls();

        }

        private int hoveredRow = -1;
        private int hoveredCol = -1;

        private void guna2DataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewTextBoxColumn18"].Index)
                {


                    guna2DataGridView1.Cursor = Cursors.Hand;
                }
            }
        }

        private void guna2DataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == guna2DataGridView1.Columns["dataGridViewTextBoxColumn18"].Index)
                {


                    guna2DataGridView1.Cursor = Cursors.Default;
                }
            }
        }

      

        private async void guna2DataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string factureName = guna2DataGridView3.Rows[e.RowIndex].Cells[0].Value.ToString();

                int temp = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(factureName);

                if (e.ColumnIndex == guna2DataGridView3.Columns["dataGridViewImageColumn3"].Index)
                {

                    List<PaymentDocument> documents = await PaymentDocumentsFournisseurService.GetDocumentsByPaymentIdAsync(temp);
                    if (temp > 0)
                    {
                        Dtos.Dtos.PaymentDto userDto = await PaymentBussiness.GetByIdAsync(temp);
                        userDto.PaymentId = temp;
                        if (userDto != null)
                        {
                            AddPayment frm = new AddPayment(userDto, documents);

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

                            await LoadPaymentsFacturesIntoGrid();
                            LoadEntityPaymentControls();
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



                else if (e.ColumnIndex == guna2DataGridView3.Columns["dataGridViewImageColumn1"].Index)
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

                else if (e.ColumnIndex == guna2DataGridView3.Columns["dataGridViewImageColumn2"].Index)
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
                else if (e.ColumnIndex == guna2DataGridView3.Columns["dataGridViewImageColumn4"].Index)
                {


                    if (temp > 0)
                    {
                        var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet Paiement ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            var success = await BussinessAcesssLayer.PaymentBussiness.DeleteAdvanceAsync(temp);
                            if (success.IsSuccess)
                            {
                                MessageBox.Show("Payment supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                await LoadPaymentsFacturesIntoGrid();
                                LoadEntityPaymentControls();

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

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }



        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {


            


           
        }

        private void dataGridView1_CellMouseEnter(Guna2DataGridView g, object sender, DataGridViewCellEventArgs e, Image img, int i)
        {
            // or from the cell


            if (e.RowIndex >= 0 && e.ColumnIndex == i)
            {
                var zoomedImage = new Bitmap(img, new Size(img.Width + 2, img.Height + 2));

                g.Cursor = Cursors.Hand;
                g.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = zoomedImage;
            }
        }

        private void dataGridView1_CellMouseLeave(Guna2DataGridView g, object sender, DataGridViewCellEventArgs e, Image img, int i)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == i)
            {
                g.Cursor = Cursors.Default;
                g.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = img;
            }
        }

        private void FilterDataGridView(Guna2DataGridView dataGridView, string searchText)
        {
            // If search is empty, make all rows visible
            if (string.IsNullOrEmpty(searchText))
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Visible = true;
                }
                return;
            }

            // Search through each row
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                bool rowVisible = false;

                // Check each cell in the row (skip the last columns if they contain buttons/images)
                for (int i = 0; i < row.Cells.Count - 4; i++) // Adjust -4 based on your column count
                {
                    if (row.Cells[i].Value != null &&
                        row.Cells[i].Value.ToString().ToLower().Contains(searchText))
                    {
                        rowVisible = true;
                        break;
                    }
                }

                row.Visible = rowVisible;
            }
        }
  

        private void LoadEntityPaymentControls()
        {
            guna2Panel1.Controls.Clear();

            // Set up proper layout for the panel
            guna2Panel1.AutoScroll = true; // Enable scrolling if many entities
            guna2Panel1.Padding = new Padding(3); // Small padding for the container

            var entities = EntityBussiness.GetAllEntitesNotAsync();

            foreach (var entity in entities)
            {
                var paymentControl = new PaymentParEntity(false, entity, from, to);
                guna2Panel1.Controls.Add(paymentControl);

                // Better layout settings:
                paymentControl.Margin = new Padding(0, 0, 0, 15); // Only bottom margin for spacing
                paymentControl.Dock = DockStyle.Top; // Stack controls vertically

            }
        }
        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            from = guna2DateTimePicker1.Value;
            LoadEntityPaymentControls();
           
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void guna2DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            to = guna2DateTimePicker2.Value;
            LoadEntityPaymentControls(); 
        }

        private void guna2Panel4_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            guna2Button1_Click(sender, e);
        }

        private void guna2DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView1_CellContentClick(sender, e);
        }

        private void guna2DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
            //usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 1);


        }

        private void guna2DataGridView2_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 8);
        }

        private void guna2DataGridView3_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.guna2DataGridView11_CellPainting(sender, e, 2);

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
        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

          
            guna2Button6.FillColor = Color.Transparent;
            guna2Button3.FillColor = Color.White;
            reset();
            guna2Button3.ForeColor = Color.Black;
            guna2TabControl1.SelectedIndex = 0;



        }

        private void reset()
        {
            guna2Button3.HoverState.FillColor = guna2Button3.FillColor;
            
            guna2Button6.HoverState.FillColor = guna2Button6.FillColor;


            guna2Button3.ForeColor = Color.FromArgb(144, 144, 144);
            
            guna2Button6.ForeColor = Color.FromArgb(144, 144, 144);
        }
        private void guna2Button6_Click(object sender, EventArgs e)
        {

          
            guna2Button3.FillColor = Color.Transparent;
            guna2Button6.FillColor = Color.White;
            reset();
            guna2Button6.ForeColor = Color.Black;
            guna2TabControl1.SelectedIndex = 1;


        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            guna2Button6.FillColor = Color.Transparent;
            guna2Button3.FillColor = Color.Transparent;
            
            reset();
            
            guna2TabControl1.SelectedIndex = 2;
        }

        private void guna2DataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void guna2DataGridView3_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView3_CellContentClick(sender, e);
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView1, guna2TextBox2);
        }

        private void guna2TextBox1_TextChanged_1(object sender, EventArgs e)
        {
               
        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {
            usefulFunction.UsefulFuncitonClass.filter(guna2DataGridView3, guna2TextBox3);
        }

        private void guna2Panel4_Paint_2(object sender, PaintEventArgs e)
        {

        }

        private void guna2DateTimePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            guna2DateTimePicker1_ValueChanged(sender, e);
        }

        private void guna2DateTimePicker2_ValueChanged_1(object sender, EventArgs e)
        {
            guna2DateTimePicker2_ValueChanged(sender, e);
        }

        private void guna2Panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button4_Click_1(object sender, EventArgs e)
        {
            guna2Button5.FillColor = Color.Transparent;
            guna2Button4.FillColor = Color.White;
            guna2TabControl2.SelectedIndex = 0;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            guna2Button5.FillColor = Color.White;
            guna2Button4.FillColor = Color.Transparent;
            guna2TabControl2.SelectedIndex = 1;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
        }
    }
}


