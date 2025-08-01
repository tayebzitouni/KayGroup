using BusinessAccessLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Controllers;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.user_controls;
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
using Microsoft.Web.WebView2.WinForms;


namespace freelanceProject1.Presentation_Layer.forms
{
    public partial class Fiscal : Form
    {
        private Color _statusColor = Color.Gray;
        private InvoiceCalendar invoiceCalendar;
        private DateTime from = new DateTime(DateTime.Today.Year, 1, 1);
        private DateTime to = new DateTime(DateTime.Today.Year, 12, 31);
        public Fiscal()
        {

            InitializeComponent();
            guna2DateTimePicker2.Value = to;

            guna2DateTimePicker1.FillColor = guna2DateTimePicker1.Parent.BackColor;
            // guna2DateTimePicker1.BorderColor = SystemColors.ButtonFace; // or any color you like
            guna2DateTimePicker1.BorderThickness = 0;
            guna2DateTimePicker1.HoverState.FillColor = guna2DateTimePicker1.Parent.BackColor;
            guna2DateTimePicker2.FillColor = guna2DateTimePicker2.Parent.BackColor;
            //  guna2DateTimePicker2.BorderColor = SystemColors.ButtonFace; // or any color you like
            guna2DateTimePicker2.BorderThickness = 0;
            guna2DateTimePicker2.HoverState.FillColor = guna2DateTimePicker2.Parent.BackColor;

            InitializeInvoiceCalendar();
            //guna2Panel3.AutoScrollMinSize = new Size(0, 2000); // force vertical scroll
            //guna2Panel3.VerticalScroll.Enabled = true;
            //guna2Panel3.VerticalScroll.Visible = true;
            //guna2Panel3.AutoScroll = true;
            //guna2Panel3.AutoScrollMinSize = new Size(0, 1200); // or any value greater than Form.Height



        }



        private void  LoadTaxesParEntité()
        {
            try
            {
                var overdue =  EntityBussiness.GetAllEntitesNotAsync();

                if (overdue != null)
                {

                    guna2TabControl1.TabPages[0].Controls.Clear();


                    // Add each invoice
                    foreach (var invoice in overdue)
                    {

                        var invoiceControl = new tresoireRecevoire(from, to, invoice) // Your custom control
                        {

                            Dock = DockStyle.Top,
                            Margin = new Padding(10),
                            Height = 90
                        };
                        guna2TabControl1.TabPages[0].Controls.Add(invoiceControl);
                    }

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement: {ex.Message}",
                              "Erreur",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }


        private void Reset()
        {
            guna2Button4.FillColor = Color.Transparent;
        }

       

        private void Initial()
        {
            int i = EntityBussiness.TVADeadline();
            if (i <= 7)
            {
                guna2Panel5.BorderColor = Color.Red;
                _statusColor = Color.Red;
            }
            else if (i <= 15)
            {
                guna2Panel5.BorderColor = Color.Orange;
                _statusColor = Color.Orange;
            }
            else
            {
                _statusColor = Color.FromArgb(74, 222, 128);
                guna2Panel5.BorderColor = _statusColor;
            }

            label41.Text = i.ToString() + " Days Remaining";
            label38.Text = DateTime.Today.AddDays(i).ToShortDateString();
            label37.Text = i.ToString() + " Days Remaining";
            label42.Text = DateTime.Today.AddDays(i).ToShortDateString();
        }

        private void Tva()
        {
            decimal tvacollect = FactureClientBussinesLayer.GetTotalTVACollectedInCurrentTrimester(from, to);
            decimal tvadeductible = FactureFournisseurBusinessLayer.GetTotalTVACollectedInCurrentTrimester(from, to);
            decimal c = FactureFournisseurBusinessLayer.GetTotalTVAReturnCollectedInCurrentTrimester(from, to);
            label33.Text = tvacollect.ToString() + " MAD";
            label31.Text = tvadeductible.ToString() + " MAD";
            label29.Text = (tvacollect - tvadeductible-c).ToString() + " MAD";
            usefulFunction.UsefulFuncitonClass.TextColor(tvacollect - tvadeductible-c, label29);

            int a = 0;
            try
            {
                a = (int)((tvadeductible / tvacollect) * 100);
                guna2ProgressBar1.Value = a;
                    }
            catch (Exception)
            {
                guna2ProgressBar1.Value = 0;
            }
            label24.Text = "TVA déductible: " + a.ToString() + " %";
        }

        private async void Fiscal_Load(object sender, EventArgs e)
        {
          
            
            guna2Panel5.Paint += guna2Panel5_Paint2;
            guna2DateTimePicker1.Value = from;
            Initial();
            Tva();
             LoadTaxesParEntité();
            
        }



        private void InitializeInvoiceCalendar()
        {
            invoiceCalendar = new InvoiceCalendar();
            invoiceCalendar.Dock = DockStyle.Fill;
            Controls.Add(invoiceCalendar);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        

        private Control GetDeepestChildAtPoint(Control parent, Point point)
        {
            Control child = parent.GetChildAtPoint(point, GetChildAtPointSkip.Invisible);

            if (child == null || child == parent)
                return parent;

            Point childPoint = parent.PointToClient(child.PointToScreen(Point.Empty));
            Point relative = new Point(point.X - childPoint.X, point.Y - childPoint.Y);

            return GetDeepestChildAtPoint(child, relative);
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e) { }

        private void guna2Panel3_Paint_1(object sender, PaintEventArgs e) { }

        private void guna2GradientPanel3_Paint(object sender, PaintEventArgs e) { }

        private void guna2Panel3_Paint_2(object sender, PaintEventArgs e) { }

        private void guna2GradientPanel2_Paint(object sender, PaintEventArgs e) { }

        private void label45_Click(object sender, EventArgs e) { }

        private void guna2ProgressBar1_ValueChanged(object sender, EventArgs e) { }

        private void guna2Panel5_Paint2(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            int textX = label41.Location.X;///360;
            int textY = label41.Location.Y;
            DrawStatusIndicator(e, textX, textY, _statusColor);
        }
        private void DrawStatusIndicator(PaintEventArgs e, int textX, int textY, Color color)
        {
            int dotSize = 8;
            int spacing = 5;
            int dotX = textX - dotSize - spacing;
            int dotY = textY + (int)(e.Graphics.MeasureString("A", this.Font).Height - dotSize) / 2;

            using (Brush brush = new SolidBrush(color))
            {
                e.Graphics.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            Reset();
            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            clickedButton.ForeColor = Color.Black;
            guna2TabControl1.SelectedIndex = 0;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Reset();
            Guna.UI2.WinForms.Guna2Button clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;
            clickedButton.FillColor = Color.White;
            clickedButton.ForeColor = Color.Black;
            guna2TabControl1.SelectedIndex = 1;
        }

        private void invoiceCalendar1_Load(object sender, EventArgs e)
        {

        }

        private void label41_Click(object sender, EventArgs e)
        {

        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            from = guna2DateTimePicker1.Value;
           
            Tva();
            LoadTaxesParEntité();
        }

        private void guna2DateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            to = guna2DateTimePicker2.Value;
           
            Tva();
             LoadTaxesParEntité();
        }
    }
}
