using BussinessAcesssLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.usefulFunction;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Dtos.Dtos;

namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class AddFacture : Form
    {
        private bool Add = true; private int factureId = 0;
        private decimal utiliser = 0;
        private System.Windows.Forms.Timer slideTimer;
        private int targetLeft;
        private int slideSpeed = 40;
        private decimal OldHT = 0;
        private int dure = 0;
        private int dure2 = 0;
        private bool isInitializing = true;
        private bool isInitializing2 = true;

        public AddFacture()
        {
            InitializeComponent();
            guna2ComboBox5.Enabled = false;
            guna2ComboBox1.Enabled = false;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2NumericUpDown8.Enabled = false;
            guna2NumericUpDown9.Enabled = false;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
            guna2DateTimePicker1.Value = Convert.ToDateTime(DateTime.Today);
            //guna2DateTimePicker2.Value = Convert.ToDateTime(DateTime.Today);
            guna2DateTimePicker3.Value = Convert.ToDateTime(DateTime.Today);
            // guna2DateTimePicker4.Value = Convert.ToDateTime(DateTime.Today);

            isInitializing = false;
            isInitializing2 = false;

        }

        //public async Task<> initialize(Dtos.Dtos.FactureClientDto f)
        //{
        //    InitializeComponent();
        //    await usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox3);


        //  await  usefulFunction.UsefulFuncitonClass.loadcomboboxofClientWithData(guna2ComboBox4);
        //    factureId = f.id;
        //    guna2Panel2.Visible = false;
        //    label1.Text = "Update Client Facture";
        //    guna2Button4.Visible = false;
        //    guna2Button5.Visible = false;
        //    guna2TabControl1.TabPages.Remove(tabPage2);
        //    guna2TabControl1.TabMenuVisible = false;
        //    guna2TabControl1.Location = new Point(20, 45);
        //    guna2ComboBox3.SelectedIndex = 1;
        //    guna2ComboBox4.SelectedValue = f.clientId;
        //    guna2NumericUpDown2.Value = f.MontantTH;
        //    guna2ComboBox5.SelectedItem = f.Status;
        //    guna2ComboBox7.SelectedItem = f.ModeDePayment;
        //    guna2DateTimePicker3.Value = Convert.ToDateTime(f.DateEmission);
        //    guna2DateTimePicker4.Value = Convert.ToDateTime(f.DateEcheance);
        //    guna2TextBox3.Text = f.Description;
        //    guna2NumericUpDown3.Value = f.TVa;
        //    guna2NumericUpDown5.Value = f.Total;

        //    Add = false;
        //}


        public AddFacture(Dtos.Dtos.FactureClientDto f)
        {
            InitializeComponent();
            Add = false;
            guna2ComboBox5.Enabled = true;
            factureId = f.id;
            guna2Panel2.Visible = false;
            label1.Text = "Update Client Facture"; label24.Text = "Complétez le formulaire pour Update Le Facture";
            guna2Button4.Visible = false;
            guna2Button5.Visible = false;
            guna2TabControl1.TabPages.Remove(tabPage2);
            guna2TabControl1.TabMenuVisible = false;
            guna2TabControl1.Location = new Point(20, 80);
            guna2NumericUpDown2.Value = f.MontantTH;
            guna2NumericUpDown8.Enabled = true;
            guna2NumericUpDown9.Enabled = true;
            guna2ComboBox5.SelectedItem = f.Status;
            guna2ComboBox7.SelectedItem = f.ModeDePayment;
            guna2ComboBox4.SelectedValue = f.clientId;
            guna2DateTimePicker3.Value = Convert.ToDateTime(f.DateEmission);
            guna2DateTimePicker4.Value = Convert.ToDateTime(f.DateEcheance);
            guna2NumericUpDown8.Value = f.payed;
            guna2TextBox3.Text = f.Description;
            guna2NumericUpDown3.Value = f.TVa;
            guna2NumericUpDown5.Value = f.Total;
            guna2ComboBox9.SelectedItem = f.devis;
            OldHT = f.MontantTH;


            // Handle async combo box loading after form is shown
            this.Shown += async (sender, e) =>
            {
                try
                {

                    UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox3);
                    // guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
                    UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox4);
                    //  guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;

                    guna2ComboBox3.SelectedValue = f.entiteId;

                    if (guna2ComboBox4.Items.Count > 0 && f.clientId != null)
                    {
                        guna2ComboBox4.SelectedValue = f.clientId;
                        guna2NumericUpDown4.ValueChanged -= guna2NumericUpDown4_ValueChanged;

                        // Set the value
                        guna2NumericUpDown4.Value = f.MontantTH;

                        // Reattach event handler
                        guna2NumericUpDown4.ValueChanged += guna2NumericUpDown4_ValueChanged;
                    }
                    else
                    {
                        MessageBox.Show("Client data could not be loaded", "Warning",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    isInitializing = false;
                }
            };
        }

        public AddFacture(Dtos.Dtos.FactureFournisseurDto f)
        {
            InitializeComponent();
            Add = false;
            // Set all synchronous properties first
            factureId = f.id;
            guna2Panel2.Visible = false;

            label1.Text = "Update Fournissuer Facture";
            label24.Text = "Complétez le formulaire pour Update Le Facture";
            guna2TabControl1.Location = new Point(20, 80);
            guna2Button4.Visible = false;
            guna2Button5.Visible = false;
            guna2TabControl1.TabPages.Remove(tabPage1);
            guna2TabControl1.TabMenuVisible = false;
            // guna2NumericUpDown9.Enabled = false;
            guna2ComboBox1.Enabled = true;
            guna2ComboBox1.SelectedItem = f.Status;

            guna2DateTimePicker1.Value = Convert.ToDateTime(f.DateReception);
            guna2DateTimePicker2.Value = Convert.ToDateTime(f.DateEcheance);
            guna2TextBox1.Text = f.Description;  // Fixed from TextBox3 to TextBox1
            guna2NumericUpDown6.Value = f.TVa;
            guna2NumericUpDown1.Value = f.Total;
            guna2NumericUpDown2.Value = f.Retenue;
            guna2NumericUpDown8.Enabled = true;
            guna2NumericUpDown9.Enabled = true;
            guna2NumericUpDown9.Value = f.payed;
            guna2ComboBox10.SelectedItem = f.devis;
            this.Shown += async (sender, e) =>
            {
                try
                {

                    //   await UsefulFuncitonClass.loadcomboboxofEntityWithData(guna2ComboBox2);
                    UsefulFuncitonClass.loadcomboboxofFournisseurWithDataWithNoAsync(guna2ComboBox6);

                    //guna2ComboBox3.SelectedValue = f.entiteId;
                    //if (guna2ComboBox2.Items.Count > 0 && f.entiteId != null)
                    //{
                    //    guna2ComboBox2.SelectedValue = f.entiteId;
                    //}
                    if (guna2ComboBox6.Items.Count > 0 && f.fournisseurId != null)
                    {
                        guna2ComboBox6.SelectedValue = f.fournisseurId;
                        guna2NumericUpDown7.ValueChanged -= guna2NumericUpDown7_ValueChanged;


                        // Set the value
                        guna2NumericUpDown7.Value = f.MontantTH;

                        // Reattach event handler
                        guna2NumericUpDown7.ValueChanged += guna2NumericUpDown7_ValueChanged;
                    }
                    if (guna2ComboBox8.Items.Count > 0 && f.ModeDePayment != null)
                    {
                        guna2ComboBox8.SelectedItem = f.ModeDePayment;
                    }
                    else
                    {
                        MessageBox.Show("Client data could not be loaded", "Warning",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    isInitializing2 = false;
                }
            };


        }



        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            Reset();

            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            clickedButton.ForeColor = Color.Black;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
            guna2TabControl1.SelectedIndex = 0;
            var tabPage = guna2TabControl1.SelectedTab;
            if (tabPage != null && tabPage.AutoScroll)
            {
                tabPage.VerticalScroll.Value = 0; // Scroll to top
                tabPage.PerformLayout(); // Refresh UI
            }

        }

        private void Reset()
        {
            guna2Button4.FillColor = Color.Transparent;
            guna2Button5.FillColor = Color.Transparent;
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

        private async void AddFacture_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            UsefulFuncitonClass.AttachEvents(this);
            this.Opacity = 0;

            this.Width = 600;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Top = 0;

            this.Left = Screen.PrimaryScreen.WorkingArea.Width;
            this.StartPosition = FormStartPosition.Manual;


            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox3);//await
                                                                                                    // guna2ComboBox4.SelectedIndexChanged -= guna2ComboBox4_SelectedIndexChanged;
            usefulFunction.UsefulFuncitonClass.loadcomboboxofClientWithDataNoaysnc(guna2ComboBox4);
            //guna2ComboBox4.SelectedIndexChanged += guna2ComboBox4_SelectedIndexChanged;
            if (Add)
            {
                usefulFunction.UsefulFuncitonClass.loadcomboboxofFournisseurWithDataWithNoAsync(guna2ComboBox6);
            }
            usefulFunction.UsefulFuncitonClass.loadcomboboxofEntityWithDataNotAsync(guna2ComboBox2);//

            guna2NumericUpDown3.Enabled = false;
            guna2NumericUpDown5.Enabled = false;
            guna2NumericUpDown1.Enabled = false;
            guna2NumericUpDown2.Enabled = false;
            guna2NumericUpDown6.Enabled = false;


        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Reset();
            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            guna2Button4.HoverState.FillColor = guna2Button4.FillColor;
            guna2Button5.HoverState.FillColor = guna2Button5.FillColor;
            clickedButton.ForeColor = Color.Black;
            guna2TabControl1.SelectedIndex = 1;
            var tabPage = guna2TabControl1.SelectedTab;
            if (tabPage != null && tabPage.AutoScroll)
            {
                tabPage.VerticalScroll.Value = 0; // Scroll to top
                tabPage.PerformLayout(); // Refresh UI
            }
        }

        private void guna2ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox4.SelectedItem is ClientDto temp)
            {
                dure = temp.DelayDePayment;
            }
            guna2DateTimePicker3_ValueChanged(guna2DateTimePicker3, EventArgs.Empty);



        }

        private async void guna2Button7_Click(object sender, EventArgs e)
        {

            Dtos.Dtos.FactureClientDto temp = new Dtos.Dtos.FactureClientDto();
            if (guna2ComboBox4.SelectedItem is ClientDto selectedEntity2)
            {
                temp.clientId = selectedEntity2.id;
            }
            //if (guna2ComboBox4.SelectedValue.ToString() != null)
            //{
            //    //Dtos.Dtos.ClientDto client = await ClientBussinesLayer.GetClientById(Convert.ToInt32(guna2ComboBox4.SelectedValue));
            //    temp.clientId = Convert.ToInt32(guna2ComboBox4.SelectedValue);
            //    //client.ExnUtiliser += utiliser;
            //    // await ClientBussinesLayer.UpdateClientByID(client.id, client);
            //}
            else
            {
                MessageBox.Show("Veuillez sélectionner Client.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (guna2ComboBox9.SelectedItem is string devis)
            {
                temp.devis = devis;
                if (devis == "MAD")
                {
                    temp.rate = 1;
                }
                else
                {
                    temp.rate = await usefulFunction.UsefulFuncitonClass.RateDetransfere(devis, "MAD");
                }
            }

            //if (guna2ComboBox3.SelectedItem.ToString() != null)
            //{
            //    temp.entiteId = Convert.ToInt32(guna2ComboBox3.SelectedValue);
            //}
            if (guna2ComboBox3.SelectedItem is EntityDto selectedEntity)
            {
                temp.entiteId = selectedEntity.Id;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Entite.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (guna2NumericUpDown8.Value >= 0)
            {
                temp.payed = Convert.ToInt32(guna2NumericUpDown8.Value);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Valid montant payee >=0.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown4, "Montant Ht"))
            {
                temp.MontantTH = Convert.ToDecimal(guna2NumericUpDown4.Value);
            }
            else
            {
                return;
            }


            if (!Add)
            {
                if (guna2ComboBox5.SelectedItem.ToString() != null)
                {
                    temp.Status = guna2ComboBox5.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Veuillez sélectionner Status.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                temp.Status = "Non payé";
            }

            if (guna2ComboBox7.SelectedItem.ToString() != null)
            {
                temp.ModeDePayment = guna2ComboBox7.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Methode De Payment.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2DateTimePicker3.ToString() != null)
            {
                temp.DateEmission = guna2DateTimePicker3.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Date Emission.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2DateTimePicker4.ToString() != null)
            {
                temp.DateEcheance = guna2DateTimePicker4.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Date Échéance.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(guna2TextBox3.ToString()))
            {
                temp.Description = guna2TextBox3.Text;
            }
            else
            {
                temp.Description = " ";
            }
            temp.TVa = Convert.ToDecimal(guna2NumericUpDown3.Value);
            temp.Total = Convert.ToDecimal(guna2NumericUpDown5.Value);


            string a = " ";
            if (Add)
            {
                temp.payed = 0;
                a = await BussinessAcesssLayer.FactureClientBussinesLayer.AddAsync(utiliser, temp);
            }
            else
            {
                a = await BussinessAcesssLayer.FactureClientBussinesLayer.UpdateAsync(utiliser, OldHT, factureId, temp);

            }
            if (a == "")
            {
                MessageBox.Show("Opeartion end Succefully", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show(a, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }


        private void tvaclient()
        {
            Dtos.Dtos.TVAEXNUTILISER temp;
            if (Add)
            {
                temp = FactureClientBussinesLayer.calcuateTVA(Convert.ToInt32(guna2ComboBox4.SelectedValue), Convert.ToDecimal(guna2NumericUpDown4.Value), guna2ComboBox9.SelectedItem.ToString());
            }
            else
            {
                temp = FactureClientBussinesLayer.calcuateTVA(Convert.ToInt32(guna2ComboBox4.SelectedValue), Convert.ToDecimal(guna2NumericUpDown4.Value), guna2ComboBox9.ToString(), OldHT, false);

            }
            guna2NumericUpDown3.Value = temp.TVA;
            utiliser = temp.ExnUtiliser;
            guna2NumericUpDown5.Value = Convert.ToDecimal(guna2NumericUpDown4.Value) + Convert.ToDecimal(guna2NumericUpDown3.Value);
        }

        private async void guna2NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            tvaclient();
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2Button2_Click(object sender, EventArgs e)
        {

            //guna2ComboBox2.SelectedValue = f.entiteId;
            //guna2ComboBox6.SelectedValue = f.fournisseurId;
            //guna2ComboBox8.SelectedValue = f.Status;
            //guna2NumericUpDown7.Value = f.MontantTH;
            //guna2ComboBox1.SelectedItem = f.Status;
            //guna2DateTimePicker1.Value = Convert.ToDateTime(f.DateReception);
            //guna2DateTimePicker2.Value = Convert.ToDateTime(f.DateEcheance);
            //guna2TextBox3.Text = f.Description;
            //guna2NumericUpDown6.Value = f.TVa;
            //guna2NumericUpDown1.Value = f.Total;
            //guna2NumericUpDown2.Value = f.Retenue;



            Dtos.Dtos.FactureFournisseurDto temp = new Dtos.Dtos.FactureFournisseurDto();


            if (guna2ComboBox6.SelectedItem is FournisseurDto selectedEntity3)
            {
                temp.fournisseurId = selectedEntity3.id;
            }
            //if (guna2ComboBox6.SelectedValue.ToString() != null)
            //{
            //    Dtos.Dtos.FournisseurDto client = await FournisseurBussinesLayer.GetFournisseurById(Convert.ToInt32(guna2ComboBox6.SelectedValue));
            //    temp.fournisseurId = client.id;

            //    //   await FournisseurBussinesLayer.UpdateFournisseurByID(client.id, client);
            //}
            else
            {
                MessageBox.Show("Veuillez sélectionner Fournisseur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            if (guna2ComboBox10.SelectedItem is string devis)
            {
                temp.devis = devis;
                if (devis == "MAD")
                {
                    temp.rate = 1;
                }
                else
                {


                    temp.rate = await usefulFunction.UsefulFuncitonClass.RateDetransfere(devis, "MAD");
                    if (temp.rate == 0)
                    {
                        return;
                    }
                }
            }

            if (guna2ComboBox2.SelectedItem is EntityDto selectedEntity4)
            {
                temp.entiteId = selectedEntity4.Id;
            }
            //if (guna2ComboBox2.SelectedItem.ToString() != null)
            //{


            //    temp.entiteId = Convert.ToInt32(guna2ComboBox2.SelectedValue);
            //}
            else
            {
                MessageBox.Show("Veuillez sélectionner Entite.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2NumericUpDown9.Value >= 0)
            {


                temp.payed = Convert.ToInt32(guna2NumericUpDown9.Value);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner valid Payed montant >=0.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }




            if (usefulFunction.UsefulFuncitonClass.checkValidation(guna2NumericUpDown7, "Montant Ht"))
            {
                temp.MontantTH = Convert.ToDecimal(guna2NumericUpDown7.Value);
            }
            else
            {
                return;
            }


            if (!Add)
            {
                if (guna2ComboBox1.SelectedItem.ToString() != null)
                {
                    temp.Status = guna2ComboBox1.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Veuillez sélectionner Status.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                temp.Status = "Non payé";
            }

            if (guna2ComboBox8.SelectedItem.ToString() != null)
            {
                temp.ModeDePayment = guna2ComboBox8.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Methode De Payment.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2DateTimePicker1.ToString() != null)
            {
                temp.DateReception = guna2DateTimePicker1.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Date Reception.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (guna2DateTimePicker2.ToString() != null)
            {
                temp.DateEcheance = guna2DateTimePicker2.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner Date Échéance.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(guna2TextBox1.ToString()))
            {
                temp.Description = guna2TextBox1.Text;
            }
            else
            {
                temp.Description = " ";
            }
            temp.TVa = Convert.ToDecimal(guna2NumericUpDown6.Value);
            temp.Total = Convert.ToDecimal(guna2NumericUpDown1.Value);
            temp.Retenue = Convert.ToDecimal(guna2NumericUpDown2.Value);
            temp.name = "FF";


            string a = " ";
            if (Add)
            {
                temp.payed = 0;
                a = await FactureFournisseurBusinessLayer.AddAsync(temp);
            }
            else
            {
                a = await FactureFournisseurBusinessLayer.UpdateAsync(factureId, temp);

            }
            if (a == "")
            {

                MessageBox.Show("Opeartion end Succefully", "sucess", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show(a, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tvafournisseur()
        {
            Dtos.Dtos.TVAReturne temp = FactureFournisseurBusinessLayer.calcuateTVA(Convert.ToInt32(guna2ComboBox6.SelectedValue), Convert.ToDecimal(guna2NumericUpDown7.Value), Convert.ToString(guna2ComboBox10.SelectedItem));
            guna2NumericUpDown6.Value = temp.TVA;
            guna2NumericUpDown2.Value = temp.returne;

            guna2NumericUpDown1.Value = FactureFournisseurBusinessLayer.CalculateNetAPayer(guna2NumericUpDown7.Value, temp.TVA, temp.returne);
        }

        private void guna2NumericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            tvafournisseur();

        }
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
           int left, int top, int right, int bottom, int width, int height);

        private void guna2Button6_Click(object sender, EventArgs e)
        {


            // Notify parent form to hide overlay if it exists
            if (this.Owner is Factures parentForm)
            {
                parentForm.HideOverlay();
            }
            this.DialogResult = DialogResult.Cancel;


            this.Close();
        }

        private void guna2DateTimePicker4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Control_Click(object sender, EventArgs e)
        {
            if (sender is Guna.UI2.WinForms.Guna2TextBox tb)
                tb.BorderThickness = 2;

        }

        private void Control_Leave(object sender, EventArgs e)
        {
            if (sender is Guna.UI2.WinForms.Guna2TextBox tb)
                tb.BorderThickness = 1;

        }


        private void guna2Button1_Click(object sender, EventArgs e)
        {

            if (this.Owner is Factures parentForm)
            {
                parentForm.HideOverlay();
            }
            this.DialogResult = DialogResult.Cancel;


            this.Close();
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (!isInitializing2)
            {
                guna2DateTimePicker2.Value = guna2DateTimePicker1.Value.AddDays(dure2);
            }
        }

        private void guna2DateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            if (!isInitializing)
            {
                guna2DateTimePicker4.Value = guna2DateTimePicker3.Value.AddDays(dure);
            }
        }

        private void guna2ComboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBox6.SelectedItem is FournisseurDto temp)
            {
                dure2 = temp.delay;
            }
            guna2DateTimePicker1_ValueChanged(guna2DateTimePicker1, EventArgs.Empty);
        }

        private void guna2ComboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvafournisseur();
        }

        private void guna2ComboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvaclient();
        }
    }
}
