using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.usefulFunction;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDevHtmlRenderer.Adapters;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddSuiviCaisse : Form
    {


        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 40;
        private bool isfirst = true;

        private List<string> tabDocuments = new List<string>();
        private List<PaymentDocument> tabOldDocuments = new List<PaymentDocument>();
        private List<string> selectedFiles = new List<string>();



        private List<PaymentDocument> oldDocuments = new List<PaymentDocument>();

        private Dtos.Dtos.PaymentUtilisatuerDto existingPayment2;


        private List<PaymentDocument> existingDocuments;


        private Dtos.Dtos.EntityDto a = new Dtos.Dtos.EntityDto();
        private List<PaymentDocument> removedOldDocuments = new List<PaymentDocument>();

        private bool isEditMode = false;


        public AddSuiviCaisse()
        {
            InitializeComponent();
        }

        public AddSuiviCaisse(Dtos.Dtos.PaymentUtilisatuerDto pay, List<PaymentDocument> docs)
        {
            InitializeComponent();


            existingPayment2 = pay;
            existingDocuments = docs;
            isEditMode = true;
            LoadUtilsatueruDataToForm();
        }
        private void LoadUtilsatueruDataToForm()
        {


            // Chargement des utilisateurs
            usefulFunction.UsefulFuncitonClass.loadcomboboxofUtilisateursWithDataNoAsync(guna2ComboBox5);
            guna2ComboBox5.SelectedValue = existingPayment2.UtilisatuerId;

            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox2);
            guna2ComboBox2.SelectedValue = existingPayment2.entityId;
            guna2ComboBox13.SelectedItem = existingPayment2.MethodeDePayment;
            //    


            // Chargement de la méthode de paiement
            guna2ComboBox1.SelectedItem = existingPayment2.compte;

            // Chargement des entités


            // Montants
            guna2NumericUpDown1.Value = existingPayment2.Amount;
            guna2NumericUpDown2.Value = existingPayment2.debit;
            guna2NumericUpDown3.Value = existingPayment2.months;

            // Champs texte
            guna2TextBox1.Text = existingPayment2.Note;
            guna2TextBox6.Text = existingPayment2.ville;
            guna2TextBox7.Text = existingPayment2.reference;

            // Date du paiement
            if (existingPayment2.PaymentDate != null)
                guna2DateTimePicker1.Value = existingPayment2.PaymentDate;
            else
                guna2DateTimePicker1.Value = DateTime.Today;

            // Statut
            guna2ComboBox6.SelectedItem = existingPayment2.Status;

            foreach (var doc in existingDocuments)
            {
                if (!string.IsNullOrEmpty(doc.FilePath))
                {
                    oldDocuments.Add(doc);
                    selectedFiles.Add(doc.FileSourcePath); // OR doc.FilePath
                    AddFileCard(flowLayoutPanel2, doc.FileName, isOld: true);
                }
            }
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
                        removedOldDocuments.Add(toRemove); // track it for deletion
                    }
                }

            };

            card.Controls.Add(lbl);
            card.Controls.Add(btnRemove);
            p.Controls.Add(card);
        }
        private async void guna2Button2_Click(object sender, EventArgs e)
        {

            var temp = new Dtos.Dtos.PaymentUtilisatuerDto();


            if (guna2ComboBox5.SelectedItem is Dtos.Dtos.UtilisatuerDto selectedEntity2)
            {
                temp.UtilisatuerId = selectedEntity2.Id;
                temp.utilisateurname = selectedEntity2.Name;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Un Utilisateur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2ComboBox1.SelectedItem != null)
            {
                temp.compte = guna2ComboBox1.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner L'compte.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (guna2ComboBox2.SelectedItem is Dtos.Dtos.EntityDto selectedEntity3)
            {
                temp.entityId = selectedEntity3.Id;
                temp.entityName = selectedEntity3.Name;

            }




            else
            {
                MessageBox.Show("Veuillez sélectionner Un Entity.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }







            temp.MethodeDePayment = guna2ComboBox13.SelectedItem.ToString();

            if (guna2ComboBox6.SelectedItem != null)
            {
                temp.Status = guna2ComboBox6.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Le Status.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown1, "Montant Donne"))
            {
                temp.Amount = Convert.ToDecimal(guna2NumericUpDown1.Value);
            }
            else
            {
                MessageBox.Show("Veuillez entre le Montant.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown2, "Montant debit"))
            {
                temp.debit = Convert.ToDecimal(guna2NumericUpDown2.Value);
            }
            else temp.debit = 0;
            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown3, "Mois"))
            {
                temp.months = Convert.ToInt32(guna2NumericUpDown3.Value);
            }
            else
            {
                MessageBox.Show("Veuillez entre le mois.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            temp.Note = string.IsNullOrEmpty(guna2TextBox1.Text) ? " " : guna2TextBox1.Text;

            if (!string.IsNullOrEmpty(guna2TextBox6.Text))
            {
                temp.ville = guna2TextBox6.Text.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez enter le ville.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(guna2TextBox7.Text))
            {
                temp.reference = guna2TextBox7.Text.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez enter le reference.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2DateTimePicker1.Value != null)
            {
                temp.PaymentDate = guna2DateTimePicker1.Value;
            }
            else
            {
                temp.PaymentDate = DateTime.Today;
            }
            temp.PaidBy = UtilisatuerBussiness.getLogInUtilsatuer().Id;
            temp.RegisteredName = UtilisatuerBussiness.getLogInUtilsatuer().Name;

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
                temp.PaymentId = existingPayment2.PaymentId;
                result = await UtilisateurPaymentBussinessLayer.UpdateAdvanceAsync(temp.PaymentId, temp, documentsToSave);
            }
            else
            {
                result = await UtilisateurPaymentBussinessLayer.AddAdvanceAsync(temp, documentsToSave);
            }

            if (result.IsSuccess)
            {
                MessageBox.Show(isEditMode ? "Paiement modifié avec succès." : "Paiement ajouté avec succès.",
                                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                selectedFiles.Clear();
                flowLayoutPanel2.Controls.Clear();
                Task.Delay(1000);
                if (this.Owner is Fournisseur parentForm)
                {
                    parentForm.HideOverlay(); // ✅ Appelle directement la méthode du parent
                }
                // Fermer la fenêtre
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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

        private void AddSuiviCaisse_Load(object sender, EventArgs e)
        {
            UsefulFuncitonClass.PreparerFlowLayoutPanel(flowLayoutPanel2);

            this.FormBorderStyle = FormBorderStyle.None;
            UsefulFuncitonClass.AttachEvents(this);
            this.Opacity = 0;

            this.Width = 620;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;

            tabDocuments = new List<string>();

            if (!isEditMode)
            {
                UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox2);
            }
            tabOldDocuments = new List<PaymentDocument>();




            if (!isEditMode)
            {
                UsefulFuncitonClass.loadcomboboxofUtilisateursWithDataNoAsync(guna2ComboBox5);

            }




        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
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
                        AddFileCard(flowLayoutPanel2, file);
                    }
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (this.Owner is SuiviCaisse parentForm)
            {
                parentForm.HideOverlay();
            }
            this.DialogResult = DialogResult.Cancel;


            this.Close();
        }
    }
}
