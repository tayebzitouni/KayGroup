using BussinessAcesssLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.usefulFunction;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Dtos.Dtos;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddOutcomePayment : Form
    {
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 40;
        string fournisseurid = "";
        private bool isEditMode = false;
        private EntityDto a = new EntityDto();
        private Dictionary<TabPage, List<string>> tabDocuments = new Dictionary<TabPage, List<string>>();
        private Dictionary<TabPage, List<PaymentDocument>> tabOldDocuments = new Dictionary<TabPage, List<PaymentDocument>>();
        private List<string> selectedFiles = new List<string>();

        private List<PaymentDocument> oldDocuments = new List<PaymentDocument>();
        private Dtos.Dtos.PaymentClientDto existingPayment;

        private Dtos.Dtos.PaymentDto existingPayment3;
        private List<PaymentDocument> existingDocuments;

        private List<PaymentDocument> existingDocuments3;
        public AddOutcomePayment()
        {
            InitializeComponent();
            guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
            UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox4);
            guna2ComboBox4.SelectedIndex = -1;
            guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;

        }

        private void LoadUPaymentsDataToForm()
        {
            if (existingPayment3 == null) return;
            guna2ComboBox10.SelectedIndexChanged -= guna2ComboBox10_SelectedIndexChanged;
            UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox10);
            guna2ComboBox10.SelectedValue = -1;
            guna2ComboBox10.SelectedIndexChanged += guna2ComboBox10_SelectedIndexChanged;
            guna2ComboBox9.SelectedItem = existingPayment3.MethodeDePayment;
            guna2NumericUpDown5.Value = existingPayment3.Amount;
            guna2TextBox2.Text = existingPayment3.Note;
            guna2TextBox4.Text = existingPayment3.reference;
            oldDocuments.Clear();
            selectedFiles.Clear();
            flowLayoutPanel3.Controls.Clear();

            foreach (var doc in existingDocuments)
            {
                if (!string.IsNullOrEmpty(doc.FilePath))
                {
                    oldDocuments.Add(doc);
                    selectedFiles.Add(doc.FileSourcePath);
                    AddFileCard(flowLayoutPanel3, doc.FileName, isOld: true);
                }
            }
        }

        public AddOutcomePayment(Dtos.Dtos.PaymentDto pay, List<PaymentDocument> docs)
        {
            InitializeComponent();
            guna2TabControl1.TabPages.RemoveAt(0);


            existingPayment3 = pay;
            existingDocuments = docs;
            isEditMode = true;
            LoadUPaymentsDataToForm();
        }

        private void AddFileCard(FlowLayoutPanel p, string filePath, bool isOld = false)
        {
            Panel card = new Panel
            {
                Width = 280,
                Height = 30,
                BackColor = Color.White,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = isOld ? "old" : "new"
            };

            Label lbl = new Label
            {
                Text = Path.GetFileName(filePath),
                AutoSize = false,
                Width = 200,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };

            Guna2ImageButton btnRemove = new Guna2ImageButton
            {
                Image = Properties.Resources.icons8_annuler_24,
                Size = new Size(30, 30),
                ImageSize = new Size(20, 20),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            btnRemove.Location = new Point(card.Width - 35, 0);
            btnRemove.Click += (s, e) =>
            {
                p.Controls.Remove(card);
                selectedFiles.Remove(filePath);

                if (isOld)
                {
                    var toRemove = oldDocuments.FirstOrDefault(d => d.FileName == filePath);
                    if (toRemove != null)
                    {
                        oldDocuments.Remove(toRemove);

                    }
                }

            };

            card.Controls.Add(lbl);
            card.Controls.Add(btnRemove);
            p.Controls.Add(card);
        }

        public AddOutcomePayment(Dtos.Dtos.PaymentClientDto pay, List<PaymentDocument> docs)
        {
            InitializeComponent();

            guna2TabControl1.TabPages.RemoveAt(1);

            existingPayment = pay;
            existingDocuments = docs;
            isEditMode = true;
            LoadDataToForm();
        }


        private async void LoadDataToForm()
        {
            if (existingPayment == null) return;
            guna2ComboBox8.SelectedValue = existingPayment.entityId;
            guna2ComboBox8.Enabled = false;

            // guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
            guna2ComboBox4.SelectedValue = existingPayment.clientid;
            guna2ComboBox4.Enabled = false;
            guna2ComboBox7.SelectedItem = existingPayment.MethodeDePayment;
            //  guna2ComboBox8.SelectedValue = existingPayment.entityId;
            guna2TextBox1.Text = existingPayment.reference ?? "";
            guna2NumericUpDown4.Value = existingPayment.Amount;
            guna2TextBox3.Text = existingPayment.Note;
            fournisseurid = existingPayment.clientname;
            guna2ComboBox8.SelectedIndexChanged -= guna2ComboBox8_SelectedIndexChanged;
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox8);
            guna2ComboBox8.SelectedIndexChanged += guna2ComboBox8_SelectedIndexChanged;
            guna2ComboBox8.SelectedValue = existingPayment.entityId;

            guna2ComboBox8_SelectedIndexChanged(guna2ComboBox8, EventArgs.Empty);
            guna2ComboBox8.Enabled = false;            //guna2ComboBox11.SelectedIndexChanged -= guna2ComboBox11_SelectedIndexChanged;
            //usefulFunction.UsefulFuncitonClass.LoadComboBoxOfComptesBancaires(guna2ComboBox11,existingPayment.entityId);
            //guna2ComboBox11.SelectedValue = existingPayment.comptebancaireId;
            guna2ComboBox2.Enabled = false;
            // guna2ComboBox2.SelectedValue = existingPayment.comptebancaireId;
             guna2ComboBox2.SelectedValue = -1;
            guna2ComboBox2.Enabled = false;
            guna2ComboBox1.SelectedItem = existingPayment.devis;
            guna2ComboBox1.Enabled = false;

            foreach (var doc in existingDocuments)
            {
                if (!string.IsNullOrEmpty(doc.FilePath))
                {
                    oldDocuments.Add(doc);
                    selectedFiles.Add(doc.FileSourcePath);
                    Add2FileCard(flowLayoutPanel1, doc.FileName, isOld: true);
                }
            }
            try
            {

                guna2DataGridView3.Rows.Clear();
                var user = await FactureClientBussinesLayer.GetByIdAsync(UsefulFuncitonClass.ExtractIdFromFactureName(existingPayment.factureclient));
                label6.Visible = false;
                label4.Visible = false;
                guna2DataGridView3.Rows.Add(
                false,
                    "FC-" + user.id,
                user.DateEmission,
                user.DateEcheance,
                user.Total * user.rate,
                user.payed,

               "",
                user.Status
                );

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }

        }

        private void Add2FileCard(FlowLayoutPanel p, string filePath, bool isOld = false)
        {
            Panel card = new Panel
            {
                Width = 280,
                Height = 30,
                BackColor = Color.White,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = isOld ? "old" : "new"
            };

            Label lbl = new Label
            {
                Text = Path.GetFileName(filePath),
                AutoSize = false,
                Width = 200,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };

            Guna2ImageButton btnRemove = new Guna2ImageButton
            {
                Image = Properties.Resources.icons8_annuler_24,
                Size = new Size(30, 30),
                ImageSize = new Size(20, 20),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            btnRemove.Location = new Point(card.Width - 35, 0);
            btnRemove.Click += (s, e) =>
            {
                p.Controls.Remove(card);
                selectedFiles.Remove(filePath);

                if (isOld)
                {
                    var toRemove = oldDocuments.FirstOrDefault(d => d.FileName == filePath);
                    if (toRemove != null)
                    {
                        oldDocuments.Remove(toRemove);

                    }
                }

            };

            card.Controls.Add(lbl);
            card.Controls.Add(btnRemove);
            p.Controls.Add(card);
        }


        private void reset()
        {
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button9.HoverState.FillColor = guna2Button9.FillColor;

        }

        private async void AddOutcomePayment_Load(object sender, EventArgs e)
        {
            // await UsefulFuncitonClass.loadcomboboxofClientWithData(guna2ComboBox4);
            UsefulFuncitonClass.PreparerFlowLayoutPanel(flowLayoutPanel1);

            this.FormBorderStyle = FormBorderStyle.None;
            UsefulFuncitonClass.AttachEvents(this);
            this.Opacity = 0;

            this.Width = 620;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;

            UsefulFuncitonClass.PreparerFlowLayoutPanel(flowLayoutPanel3);
            tabDocuments[tabPage1] = new List<string>();

            if (!isEditMode)
            {
                guna2ComboBox8.SelectedIndexChanged -= guna2ComboBox8_SelectedIndexChanged;
                UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox8);
                guna2ComboBox8.SelectedIndex = -1;
                guna2ComboBox8.SelectedIndex = 0;
                UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox10);
            }

            //UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox10);


            tabDocuments[tabPage3] = new List<string>();

            tabOldDocuments[tabPage1] = new List<PaymentDocument>();

            tabOldDocuments[tabPage3] = new List<PaymentDocument>();

            guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
            UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox4);
            guna2ComboBox4.SelectedIndex = -1;

            guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;

            guna2ComboBox1.SelectedIndexChanged -= guna2ComboBox1_SelectedIndexChanged;
            if (!isEditMode && guna2ComboBox1.Items.Count > 0)
            {
                if (guna2ComboBox1.Items.Count > 0)
                {
                    guna2ComboBox1.SelectedIndex = 0;
                }
                guna2ComboBox1.SelectedIndexChanged += guna2ComboBox1_SelectedIndexChanged;
            }


            if (isEditMode)
            {
                guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
                for (int i = 0; i < guna2ComboBox4.Items.Count; i++)
                {
                    if (guna2ComboBox4.GetItemText(guna2ComboBox4.Items[i]) == fournisseurid)
                    {
                        guna2ComboBox4.SelectedIndex = i;
                        break;
                    }
                }

                guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;

            }
            else
            {

                if (guna2ComboBox4.Items.Count > 0)
                {

                    guna2ComboBox4.SelectedIndex = 0;
                }
            }


            if (isEditMode == false)
            {
                guna2ComboBox8.SelectedIndex = -1;
            }

            guna2ComboBox8.SelectedIndexChanged += guna2ComboBox8_SelectedIndexChanged;
            //  guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;

            //  UsefulFuncitonClass.loadcomboboxWithClientsFactures(guna2ComboBox4, guna2ComboBox3);





        }

        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async Task load(int entid, int i, string devis)
        {
            decimal tot = 0;
            try
            {

                guna2DataGridView3.Rows.Clear();
                var users = await FactureClientBussinesLayer.GetFacturesAvecStatutsAsync(false, entid, i, devis);
                string montant;
                foreach (var user in users)
                {
                    tot += user.total - user.payed;
                    if (devis != "MAD")
                    {
                        montant = user.total + " " + user.devis + " = " + user.total * user.rate + " MAD";

                    }
                    else
                    {
                        montant = user.total + " " + user.devis;
                    }
                    guna2DataGridView3.Rows.Add(
                    false,
                        user.NumeroFacture,
                    user.DateEmission,
                    user.DateEcheance,
                    montant,
                    user.payed,

                   "",
                    user.Statut
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void UpdateTotalSelected()
        {
            decimal sum = 0; int tot = 0;
            foreach (DataGridViewRow row in guna2DataGridView3.Rows)
            {
                if (Convert.ToBoolean(row.Cells["hi"].Value))
                {
                    string rawValue = row.Cells["Total"].Value.ToString();

                    // Extract numeric part using regex (handles dot or comma decimal separators)
                    string numericPart = Regex.Match(rawValue, @"[\d.,]+").Value;

                    // Replace comma with dot only if needed (i.e., if your decimal separator is dot)
                    numericPart = numericPart.Replace(",", ".");

                    // Convert to decimal using invariant culture (uses dot as decimal separator)
                    decimal total = Convert.ToDecimal(numericPart, CultureInfo.InvariantCulture);

                    decimal payed = Convert.ToDecimal(row.Cells["Payed"].Value);
                    sum += total - payed;
                    tot++;
                }
                //if (Convert.ToBoolean(row.Cells["hi"].Value))
                //{
                //    decimal total = Convert.ToDecimal(row.Cells["Total"].Value);
                //    decimal payed = Convert.ToDecimal(row.Cells["Payed"].Value);
                //    sum += total - payed;
                //    tot++;
                //}
            }
            label4.Text = "Total Factures Sélectionnées : " + tot;
            label6.Text = $"{sum} " + guna2ComboBox1.SelectedItem;
        }

        private void guna2ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            garantirdevisandfournisserandfactures();

        }


        private void garantirdevisandfournisserandfactures()
        {

            if (!(guna2ComboBox4.SelectedItem is ClientDto abc))
            {
                MessageBox.Show("Veuillez sélectionner Un Client.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!(guna2ComboBox1.SelectedItem is string a))
            {
                MessageBox.Show("Veuillez sélectionner Le Devis Or Compte Bancaire .", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (!(guna2ComboBox8.SelectedItem is EntityDto ab))
            {
                MessageBox.Show("Veuillez sélectionner Le Entity .", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else
                load(ab.Id, abc.id, a);


        }

        private async void guna2Button7_Click(object sender, EventArgs e)
        {
            tabDocuments[tabPage1] = selectedFiles;
            tabOldDocuments[tabPage1] = oldDocuments;
            var temp = new Dtos.Dtos.PaymentClientDto();

            if (guna2ComboBox4.SelectedValue != null)
            {
                temp.clientid = Convert.ToInt32(guna2ComboBox4.SelectedValue);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Un Client.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox2.SelectedItem is CompteComboBoxDto selectedEntity3)
            {
                temp.comptebancaireId = selectedEntity3.Id;
            }

            else
            {
                MessageBox.Show("Veuillez sélectionner Un Compte Bancaire.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox1.SelectedIndex != -1)
            {
                temp.devis = guna2ComboBox1.SelectedItem.ToString();
                temp.rate = await usefulFunction.UsefulFuncitonClass.RateDetransfere(temp.devis, "MAD");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner le Devis", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<FactureSelectionnee> facturesChoisies = new List<FactureSelectionnee>();

            foreach (DataGridViewRow row in guna2DataGridView3.Rows)
            {
                if (Convert.ToBoolean(row.Cells["hi"].Value) == true)
                {
                    var numeroFacture = row.Cells["NumeroFacture"].Value?.ToString();

                    string totalStr = row.Cells["Total"].Value?.ToString()?.Split(' ')[0].Replace(",", ".");
                    string payedStr = row.Cells["Payed"].Value?.ToString()?.Split(' ')[0].Replace(",", ".");

                    if (decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal total) &&
                        decimal.TryParse(payedStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal payed))
                    {
                        decimal montantRestant = total - payed;

                        facturesChoisies.Add(new FactureSelectionnee
                        {
                            NumeroFacture = numeroFacture,
                            MontantAPayer = montantRestant,
                            factureid = usefulFunction.UsefulFuncitonClass.ExtractIdFromFactureName(numeroFacture)
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Erreur de conversion pour la facture: {numeroFacture}");
                    }
                }
            }


            if (facturesChoisies.Count == 0)
            {
                MessageBox.Show("Veuillez sélectionner au moins une facture.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (!String.IsNullOrEmpty(guna2TextBox1.Text.ToString()))
            {
                temp.reference = guna2TextBox1.Text.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner le Reference.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            if (guna2ComboBox7.SelectedItem != null)
            {
                temp.MethodeDePayment = guna2ComboBox7.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Mode De Payment.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox8.SelectedValue != null)
            {
                temp.entityId = Convert.ToInt32(guna2ComboBox8.SelectedValue);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Un Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown4, "Montant Ht"))
            {
                temp.Amount = Convert.ToDecimal(guna2NumericUpDown4.Value);
            }
            else
            {
                MessageBox.Show("Veuillez entre Le Montant.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            temp.Note = string.IsNullOrEmpty(guna2TextBox3.Text) ? " " : guna2TextBox3.Text;
            temp.PaymentDate = DateTime.Today;
            temp.PaidBy = UtilisatuerBussiness.getLogInUtilsatuer().Id;
            var selectedEntity = guna2ComboBox8.SelectedItem as Dtos.Dtos.EntityDto;
            if (selectedEntity != null)
            {
                temp.entityName = selectedEntity.Name; // DisplayMember value
            }
            if (!isEditMode)
            {
                var selectedFournisseur = guna2ComboBox4.SelectedItem as Dtos.Dtos.ClientDto;
                if (selectedFournisseur != null)
                {
                    temp.clientname = selectedFournisseur.Name;
                }
            }
            else
            {
                temp.clientname = fournisseurid;
            }
            // temp.fournisseurName = guna2ComboBox4.SelectedValue.ToString();

            temp.RegisteredName = UtilisatuerBussiness.getLogInUtilsatuer().Name;
            //string uploadFolderPath = Path.Combine(Application.StartupPath, "UploadedFiles");
            //if (!Directory.Exists(uploadFolderPath))
            //    Directory.CreateDirectory(uploadFolderPath);
            string uploadFolderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "KayGroupApp", "UploadedFiles");

            if (!Directory.Exists(uploadFolderPath))
                Directory.CreateDirectory(uploadFolderPath);

            List<PaymentDocument> documentsToSave = new List<PaymentDocument>();

            //// Add remaining old documents
            documentsToSave.AddRange(oldDocuments);


            foreach (var filePath in selectedFiles)
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    continue;

                if (!oldDocuments.Any(d => d.FilePath == filePath))
                {
                    string destPath = Path.Combine(uploadFolderPath, Path.GetFileName(filePath));

                    try
                    {
                        File.Copy(filePath, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la copie du fichier : {ex.Message}");
                    }

                    documentsToSave.Add(new PaymentDocument
                    {
                        FileName = Path.GetFileName(filePath),
                        FileSourcePath = filePath,
                        FilePath = destPath
                    });
                }
            }
            Dtos.Dtos.OperationResult result;
            if (!documentsToSave.Any())
            {
                MessageBox.Show("Veuillez sélectionner au moins un document de paiement.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            if (isEditMode)
            {
                temp.PaymentId = existingPayment.PaymentId;
                temp.factureclient = facturesChoisies[0].NumeroFacture;
                result = await paymentClientBussinesslayer.UpdatePaymentAsync(temp.PaymentId, temp, UsefulFuncitonClass.ExtractIdFromFactureName(existingPayment.factureclient), documentsToSave);
            }
            else
            {
                result = await paymentClientBussinesslayer.AddPaymentAsync(temp, documentsToSave, facturesChoisies);
            }

            if (result.IsSuccess)
            {
                MessageBox.Show(isEditMode ? "Paiement modifié avec succès." : "Paiement ajouté avec succès.",
                                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                selectedFiles.Clear();
                flowLayoutPanel1.Controls.Clear();
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!string.IsNullOrWhiteSpace(file) && File.Exists(file) && !selectedFiles.Contains(file))
                    {

                        selectedFiles.Add(file);
                        Add2FileCard(flowLayoutPanel1, file);
                    }
                }
            }
        }





        private void RefreshDocumentDisplay()
        {
            flowLayoutPanel1.Controls.Clear();
            foreach (var file in selectedFiles)
            {
                AddFileCard(flowLayoutPanel1, file, false);
            }
            foreach (var doc in oldDocuments)
            {
                AddFileCard(flowLayoutPanel1, doc.FileName, true);
            }
        }

        private void guna2TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFiles = tabDocuments[guna2TabControl1.SelectedTab];
            oldDocuments = tabOldDocuments[guna2TabControl1.SelectedTab];

            RefreshDocumentDisplay();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 🔧 Position cible = coller à droite de l’écran
            targetLeft = Screen.PrimaryScreen.WorkingArea.Width - this.Width;

            // 🔁 Lancer l’animation slide+fade
            slideTimer = new System.Windows.Forms.Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideIn;
            slideTimer.Start();
        }
        private void SlideIn(object sender, EventArgs e)
        {
            // Avancer de droite vers la gauche
            if (this.Left > targetLeft)
            {
                this.Left -= slideSpeed;
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
            }
            else
            {
                // Fin animation
                this.Left = targetLeft;
                this.Opacity = 1;
                slideTimer.Stop();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
        }
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
         int left, int top, int right, int bottom, int width, int height);

        private async void guna2Button11_Click(object sender, EventArgs e)
        {
            tabDocuments[tabPage3] = selectedFiles;
            tabOldDocuments[tabPage3] = oldDocuments;
            var temp = new Dtos.Dtos.PaymentDto();


            if (guna2ComboBox3.SelectedItem is CompteComboBoxDto selectedEntity6)
            {
                temp.comptebancaireId = selectedEntity6.Id;
                var g = await BussinessAcesssLayer.CompteBancaireBusinessLayer.GetCompteById(selectedEntity6.Id);
                temp.devis = g.Devise;
                temp.rate = await usefulFunction.UsefulFuncitonClass.RateDetransfere(temp.devis, "MAD");
            }

            else
            {
                MessageBox.Show("Veuillez sélectionner Un Compte Bancaire.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox9.SelectedItem != null)
            {
                temp.MethodeDePayment = guna2ComboBox9.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Mode De Payment.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox10.SelectedItem is EntityDto selectedEntity2 && guna2ComboBox10.SelectedValue != null)
            {
                temp.entityId = selectedEntity2.Id;
                temp.entityName = selectedEntity2.code;
            }

            else
            {
                MessageBox.Show("Veuillez sélectionner Un Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown5, "Montant Donne"))
            {
                temp.Amount = Convert.ToDecimal(guna2NumericUpDown5.Value);
            }
            else
            {
                MessageBox.Show("Veuillez entre le Montant.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(guna2TextBox4.Text))
            {
                temp.reference = guna2TextBox4.Text.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez Enter le Reference.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            temp.Note = string.IsNullOrEmpty(guna2TextBox2.Text) ? " " : guna2TextBox2.Text;
            temp.PaymentDate = DateTime.Today;
            temp.PaidBy = UtilisatuerBussiness.getLogInUtilsatuer().Id;
            temp.RegisteredName = UtilisatuerBussiness.getLogInUtilsatuer().Name;
            //string uploadFolderPath = Path.Combine(Application.StartupPath, "UploadedFiles");
            //if (!Directory.Exists(uploadFolderPath))
            //    Directory.CreateDirectory(uploadFolderPath);
            string uploadFolderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "KayGroupApp", "UploadedFiles");

            if (!Directory.Exists(uploadFolderPath))
                Directory.CreateDirectory(uploadFolderPath);

            List<PaymentDocument> documentsToSave = new List<PaymentDocument>();


            documentsToSave.AddRange(oldDocuments);


            foreach (var filePath in selectedFiles)
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    continue;

                if (!oldDocuments.Any(d => d.FilePath == filePath))
                {
                    string destPath = Path.Combine(uploadFolderPath, Path.GetFileName(filePath));

                    try
                    {
                        File.Copy(filePath, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors de la copie du fichier : {ex.Message}");
                    }

                    documentsToSave.Add(new PaymentDocument
                    {
                        FileName = Path.GetFileName(filePath),
                        FileSourcePath = filePath,
                        FilePath = destPath
                    });
                }
            }



            Dtos.Dtos.OperationResult result;
            if (!documentsToSave.Any())
            {
                MessageBox.Show("Veuillez sélectionner au moins un document de paiement.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            if (isEditMode)
            {
                temp.PaymentId = existingPayment3.PaymentId;
                result = await PaymentBussiness.UpdateAdvanceAsync(temp.PaymentId, temp, documentsToSave);
            }
            else
            {
                result = await PaymentBussiness.AddAdvanceAsync(temp, documentsToSave, false);
            }

            if (result.IsSuccess)
            {
                MessageBox.Show(isEditMode ? "Paiement modifié avec succès." : "Paiement ajouté avec succès.",
                                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                selectedFiles.Clear();
                flowLayoutPanel3.Controls.Clear();
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2GradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ImageButton3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!string.IsNullOrWhiteSpace(file) && File.Exists(file) && !selectedFiles.Contains(file))
                    {

                        selectedFiles.Add(file);
                        Add2FileCard(flowLayoutPanel3, file);
                    }
                }
            }
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            guna2Button9.FillColor = Color.Transparent;

            guna2Button4.FillColor = Color.White;
            guna2TabControl1.SelectedIndex = 0;
            reset();
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            guna2Button4.FillColor = Color.Transparent;

            guna2Button9.FillColor = Color.White;
            guna2TabControl1.SelectedIndex = 1;
            reset();
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            if (this.Owner is IncomesPayment parentForm)
            {
                parentForm.HideOverlay();
            }
            this.DialogResult = DialogResult.Cancel;


            this.Close();
        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            if (this.Owner is IncomesPayment parentForm)
            {
                parentForm.HideOverlay();
            }
            this.DialogResult = DialogResult.Cancel;


            this.Close();
        }

        private void guna2DataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == guna2DataGridView3.Columns["hi"].Index && e.RowIndex >= 0)
            {
                bool current = Convert.ToBoolean(guna2DataGridView3.Rows[e.RowIndex].Cells["hi"].Value);
                guna2DataGridView3.Rows[e.RowIndex].Cells["hi"].Value = !current;
                UpdateTotalSelected();
            }
        }

        private void guna2DataGridView3_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            guna2DataGridView3_CellContentClick(sender, e);
        }

        private void guna2ComboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            a = guna2ComboBox8.SelectedItem as EntityDto;
            if (a != null)
            {
                usefulFunction.UsefulFuncitonClass.LoadComboBoxOfComptesBancaires(guna2ComboBox2, (a.Id));
                if (!isEditMode)
                    garantirdevisandfournisserandfactures();

            }
            else
            {
                MessageBox.Show("Veuillez sélectionner L' Entite.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isEditMode)
                garantirdevisandfournisserandfactures();
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox2.SelectedItem is CompteComboBoxDto selectedCompte)
            {
                if (guna2ComboBox1.Items.Contains(selectedCompte.Devise))
                {
                    guna2ComboBox1.SelectedItem = selectedCompte.Devise;

                }
            }
        }

        private void guna2ComboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            a = guna2ComboBox10.SelectedItem as EntityDto;
            if (a != null)
            {
                usefulFunction.UsefulFuncitonClass.LoadComboBoxOfComptesBancaires(guna2ComboBox3, (a.Id));


            }
            else
            {
                MessageBox.Show("Veuillez sélectionner L' Entite.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
    }
}
