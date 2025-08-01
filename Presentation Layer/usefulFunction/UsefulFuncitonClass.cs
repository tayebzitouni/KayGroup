using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessAcesssLayer;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using BusinessAccessLayer;
using System.Windows.Forms;
using static Dtos.Dtos;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace freelanceProject1.Presentation_Layer.usefulFunction
{
    public class UsefulFuncitonClass
    {
       
        List<Transaction> bankTxs = new();
        public static int ExtractIdFromFactureName(string name)
        {
            if ((name.StartsWith("FC-") || (name.StartsWith("FF-") || name.StartsWith("PF-") || name.StartsWith("PU-")
                || name.StartsWith("PA-") || name.StartsWith("PC-") && name.Length > 2)))
            {
                string idPart = name.Substring(3);
                if (int.TryParse(idPart, out int id))
                {
                    return id;
                }
            }
            throw new ArgumentException("Invalid facture name format.");
        }
        public static  async Task<decimal> RateDetransfere(string from, string to)
        {
            try
            {
                decimal rate = await BussinessAcesssLayer.devis.GetExchangeRateAsync(from,to);
                return rate;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return 0;
            }
        }



        public static void AttachEvents(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // Handle all focusable Guna2 controls
                if (ctrl is Guna2TextBox ||
                    ctrl is Guna2ComboBox ||
                    ctrl is Guna2DateTimePicker ||
                    ctrl is Guna2NumericUpDown)
                {
                    ctrl.Enter += Control_GotFocus;
                    ctrl.Leave += Control_LostFocus;
                }

                // Special handling for TabControl
                if (ctrl is Guna2TabControl tabControl)
                {
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        AttachEvents(tabPage);
                    }
                }
                else if (ctrl.HasChildren)
                {
                    AttachEvents(ctrl);
                }
            }
        }
        private static void Control_GotFocus(object sender, EventArgs e)
        {
            if (sender is Guna2TextBox tb)
                tb.BorderThickness = 2;
            else if (sender is Guna2ComboBox cb)
                cb.BorderThickness = 2;
            else if (sender is Guna2DateTimePicker dtp)
                dtp.BorderThickness = 2;
            else if (sender is Guna2NumericUpDown nud)
                nud.BorderThickness = 2;
        }

        private static void Control_LostFocus(object sender, EventArgs e)
        {
            if (sender is Guna2TextBox tb)
                tb.BorderThickness = 1;
            else if (sender is Guna2ComboBox cb)
                cb.BorderThickness = 1;
            else if (sender is Guna2DateTimePicker dtp)
                dtp.BorderThickness = 1;
            else if (sender is Guna2NumericUpDown nud)
                nud.BorderThickness = 1;
        }

        public static void PreparerFlowLayoutPanel(FlowLayoutPanel flowLayoutPanel1)
        {
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.WrapContents = false;
        }


        public static void RefreshUserData(Guna2DataGridView dgvUsers)
        {

            dgvUsers.DataSource = null;
            dgvUsers.DataSource = BussinessAcesssLayer.UtilisatuerBussiness.GetAllUsers();
            dgvUsers.Refresh();
        }
        public static async Task loadcomboboxofEntityWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.EntityDto> entites = await BussinessAcesssLayer.EntityBussiness.GetAllEntites();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "Id";
        }


        public static async Task loadcomboboxWithFactures(Guna2ComboBox guna2ComboBox1, Guna2DataGridView view) 
           
        {

           

        }

        public static async Task loadcomboboxWithClientsFactures(Guna2ComboBox guna2ComboBox1, Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.FactureClientDto> entites = await FactureClientBussinesLayer.GetFacturesClientsByFournisseurId(Convert.ToInt32(guna2ComboBox1.SelectedValue));
            var displayList = entites.Select(e => new
            {
                id = e.id,
                Display = $"{e.id}-{e.name}"
            }).ToList();

            guna2ComboBox2.DataSource = displayList;
            guna2ComboBox2.DisplayMember = "Display";
            guna2ComboBox2.ValueMember = "id";


        }


        public static async Task loadcomboboxWithFournisseur(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.FournisseurDto> entites = await FournisseurBussinesLayer.GetAllFournisseurs();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.ValueMember = "id";
            guna2ComboBox2.DisplayMember = "Name";
        }



        public static void loadcomboboxofEntityWithDataNotAsync(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.EntityDto> entites = BussinessAcesssLayer.EntityBussiness.GetAllEntitesNotAsync();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "Id";
        }

        public static async Task loadcomboboxofClientWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.ClientDto> entites = await BussinessAcesssLayer.ClientBussinesLayer.GetAllClients();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "id";
        }


        public static void loadcomboboxofClientWithDataNoaysnc(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.ClientDto> entites = BussinessAcesssLayer.ClientBussinesLayer.GetAllClientsNoAsync();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "id";
        }


        public static async Task loadcomboboxofUtilisateursWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.UtilisatuerDto> entites = await BussinessAcesssLayer.UtilisatuerBussiness.GetAllUsers();
            guna2ComboBox2.DataSource = entites;

            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "Id";
        }
        public static void loadcomboboxofUtilisateursWithDataNoAsync(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.UtilisatuerDto> entites =  BussinessAcesssLayer.UtilisatuerBussiness.GetAllUsersNotAsync();
            guna2ComboBox2.DataSource = entites;

            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "Id";
        }



        public static async Task loadcomboboxofFournisseurWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.FournisseurDto> entites = await BussinessAcesssLayer.FournisseurBussinesLayer.GetAllFournisseurs();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.ValueMember = "id";
            guna2ComboBox2.DisplayMember = "Name";



        }

        public static void loadFilterofFournisseurWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.FournisseurDto> entites = BussinessAcesssLayer.FournisseurBussinesLayer.GetAllFournisseursWithNoAsync();
            entites.Insert(0, new Dtos.Dtos.FournisseurDto
            {
                id = 0,
                Name = "Tous les fournisseurs"
            });
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.ValueMember = "id";
            guna2ComboBox2.DisplayMember = "Name";



        }


        public static List<CompteComboBoxDto> GetComptesForComboBox(int entityId)
        {
            var comptes = CompteBancaireBusinessLayer.GetAllByEntiteId(entityId);

            return comptes.Select(cb => new CompteComboBoxDto
            {
                Id = cb.Id,
                DisplayText = $"{cb.Banque} - {cb.RIB}",
                Devise = cb.Devise
            }).ToList();
        }

        


        public static void loadFilterofEntitesWithData(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.EntityDto> entites = BussinessAcesssLayer.EntityBussiness.GetAllEntitesNotAsync();
            entites.Insert(0, new Dtos.Dtos.EntityDto
            {
                Id = 0,
                Name = "Tous les Entities"
            });
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.ValueMember = "Id";
            guna2ComboBox2.DisplayMember = "Name";



        }



        public static void LoadComboBoxOfComptesBancaires(Guna2ComboBox comboBox, int entityId)
        {
            var comptes = GetComptesForComboBox(entityId);

            comboBox.DataSource = comptes;
            comboBox.DisplayMember = "DisplayText";
            comboBox.ValueMember = "Id";
        }


        public static async Task LoadComboBoxOfAllComptesBancaires(Guna2ComboBox comboBox)
        {
            var comptes = await CompteBancaireBusinessLayer.GetAllComptes();

            // تحويل إلى DTO لعرض النصوص بشكل جيد
            var comptesDto = comptes.Select(cb => new CompteComboBoxDto
            {
                Id = cb.Id,
                DisplayText = $"{cb.Banque} - {cb.RIB}",
                Devise = cb.Devise
            }).ToList();

            // ربط الـ ComboBox بالبيانات الصحيحة
            comboBox.DataSource = comptesDto;
            comboBox.DisplayMember = "DisplayText";
            comboBox.ValueMember = "Id";
        }






        public static void loadcomboboxofFournisseurWithDataWithNoAsync(Guna2ComboBox guna2ComboBox2)
        {
            List<Dtos.Dtos.FournisseurDto> entites =  BussinessAcesssLayer.FournisseurBussinesLayer.GetAllFournisseursWithNoAsync();
            guna2ComboBox2.DataSource = entites;
            guna2ComboBox2.DisplayMember = "Name";
            guna2ComboBox2.ValueMember = "id";
        }

        public static void guna2DataGridView11_CellPainting(object sender, DataGridViewCellPaintingEventArgs e,int i)
        {
            if (e.RowIndex >= 0) // Only handle data rows (not header)
            {
                string value = e.FormattedValue?.ToString();

                // Handle column index 2 (original status values)
                if (e.ColumnIndex == 2)
                {
                    e.Handled = true;
                    e.PaintBackground(e.CellBounds, true);

                    Color bgColor = Color.Transparent;
                    Color textColor = Color.Black;

                    if (value == "Assujetti")
                    {
                        bgColor = Color.FromArgb(220, 255, 220); // Light green
                        textColor = Color.Green;
                    }
                    else if (value == "Exonéré")
                    {
                        bgColor = Color.FromArgb(220, 235, 255); // Light blue
                        textColor = Color.RoyalBlue;
                    }
                    else if (value == "Non Assujetti" || value == "Débité")
                    {

                        //bgColor = Color.FromArgb(255, 230, 230); // Light red
                        //textColor = Color.FromArgb(200, 0, 0);
                        bgColor = ColorTranslator.FromHtml("#F4E8FF");
                        textColor = ColorTranslator.FromHtml("#6A1B9A");
                        bgColor = Color.FromArgb(204, 239, 245); // Light blue
                        textColor = Color.FromArgb(0, 57, 72);
                        //bgColor = Color.FromArgb(240, 240, 240);
                        //textColor = Color.FromArgb(100, 100, 100);


                    }
                    else
                    {
                        bgColor = Color.FromArgb(204, 239, 245); // Light blue
                        textColor = Color.FromArgb(0, 57, 72);
                    }

                    DrawStyledCell(e, bgColor, textColor, Color.Transparent, false);
                }
                else if (e.ColumnIndex == i )
                {
                    e.Handled = true;
                    e.PaintBackground(e.CellBounds, true);

                    Color bgColor = Color.Transparent;
                    Color textColor = Color.Black;
                    Color borderColor = Color.Transparent;
                    bool drawBorder = true; // Always draw border for status cells

                    if (value.ToLower() == "payé" || value=="True" || value =="true")
                    {
                        bgColor = Color.FromArgb(230, 245, 230); // Light green
                        textColor = Color.FromArgb(0, 128, 0); // Dark green
                        borderColor = Color.FromArgb(180, 220, 180); // Border color
                    }
                    else if (value == "Non payé"|| value =="false" || value =="False" )
                    {
                        bgColor = Color.FromArgb(255, 230, 230); // Light red
                        textColor = Color.FromArgb(200, 0, 0); // Dark red
                        borderColor = Color.FromArgb(220, 180, 180); // Border color
                    }
                    else if (value == "En retard")
                    {
                        bgColor = Color.FromArgb(255, 243, 205); // Light yellow
                        textColor = Color.FromArgb(133, 100, 4); // Dark yellow/brown
                        borderColor = Color.FromArgb(220, 200, 150); // Border color
                    }
                    else // Default styling if none match
                    {
                        bgColor = Color.FromArgb(255, 243, 205); // Light yellow
                        textColor = Color.FromArgb(133, 100, 4); // Dark yellow/brown
                        borderColor = Color.FromArgb(220, 200, 150); //
                        bgColor = Color.FromArgb(220, 255, 220); // Light green
                        textColor = Color.Green;
                        bgColor = Color.FromArgb(220, 235, 255); // Light blue
                        textColor = Color.RoyalBlue;
                     
                        //bgColor = Color.FromArgb(204, 239, 245); // Light blue
                        //textColor = Color.FromArgb(0, 57, 72);
                        //bgColor = Color.FromArgb(240, 240, 240);
                        //textColor = Color.FromArgb(100, 100, 100);
                        //borderColor = Color.FromArgb(200, 200, 200);
                    }

                    DrawStyledCell(e, bgColor, textColor, borderColor, false);
                }
               
            }
            
        }

        public static void DrawStyledCell(DataGridViewCellPaintingEventArgs e, Color bgColor, Color textColor, Color borderColor, bool drawBorder)
        {
            string value = e.FormattedValue?.ToString();
            SizeF textSize = e.Graphics.MeasureString(value, e.CellStyle.Font);
            int padding = 8;
            Rectangle rect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - (int)textSize.Width - padding * 2) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - (int)textSize.Height - 4)/2,
                (int)textSize.Width + padding * 2,
                (int)textSize.Height + 4
            );

            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                usefulFunction.GraphicsExtensions.FillRoundedRectangle(e.Graphics, brush, rect, 10);
            }

            if (drawBorder)
            {
                using (Pen pen = new Pen(borderColor, 1.5f)) // Thicker border
                {
                    usefulFunction.GraphicsExtensions.DrawRoundedRectangle(e.Graphics, pen, rect, 10);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                value,
                e.CellStyle.Font,
                rect,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }


        public static bool checkValidation(Guna2NumericUpDown guna2NumericUpDown1,string message)
        {

            if (string.IsNullOrWhiteSpace(guna2NumericUpDown1.Text))
            {
                MessageBox.Show(message +" ne peut pas être vide..", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (!double.TryParse(guna2NumericUpDown1.Text, out _))
            {
                MessageBox.Show(message +" doit être numérique.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                return true;
            }
        }

          public static  bool CheckEmail(Guna2TextBox guna2TextBox3)
        {
            if (BussinessAcesssLayer.UtilisatuerBussiness.IsValidEmail(guna2TextBox3.Text.ToString()) == false)
            {
                MessageBox.Show("Your Email doesnt respect the standard email Format", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                return true;
            }
        }

       

        public static void ShowOverlay(Form overlayForm, Form parent)
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
                Bounds = parent.Bounds, // même taille que le parent
                TopMost = true,
                Owner = parent // lie le form secondaire au parent
            };

            // Clic pour fermer (optionnel)
            overlayForm.Click += (s, e) =>
            {
                overlayForm.Close();
                overlayForm = null;
            };

            overlayForm.Show();
        }

        public static void HideOverlay(Form overlayForm, Form parent)
        {
            if (overlayForm != null)
            {
                parent.Controls.Remove(overlayForm);
                overlayForm.Dispose();
                overlayForm = null;
            }
        }


        public static void filter(Guna2DataGridView guna2DataGridView11, Guna2TextBox guna2TextBox1)
        {
            string searchText = guna2TextBox1.Text.Trim().ToLower();
            bool isSearchEmpty = string.IsNullOrEmpty(searchText);

            guna2DataGridView11.SuspendLayout();

            foreach (DataGridViewRow row in guna2DataGridView11.Rows)
            {
                if (row.IsNewRow) continue;

                bool matchFound = false;

                if (isSearchEmpty)
                {
                    matchFound = true;
                }
                else
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string cellValue = cell.Value?.ToString().ToLower() ?? "";
                        if (cellValue.Contains(searchText))
                        {
                            matchFound = true;
                            break;
                        }
                    }
                }

                row.Visible = matchFound;
            }

            guna2DataGridView11.ResumeLayout();
        }
        

        public static void TextColor(decimal a ,Label label29)
        {
            if (a > 0)
            {
                label29.ForeColor = Color.SeaGreen;
            }
            else if (a < 0)
            {
                label29.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

    }
    


    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle( Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }


        public static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}