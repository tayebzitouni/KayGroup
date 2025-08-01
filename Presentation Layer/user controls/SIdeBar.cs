using freelanceProject1.Presentation_Layer.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace freelanceProject1
{
    public partial class SIdeBar : UserControl
    {
        public Paramètres MainFormReference { get; set; }

        public SIdeBar()
        {
            InitializeComponent();
            // Disable Guna2's default styling for the dropdown

            // Enable custom drawing for Guna2ComboBox
            guna2ComboBox1.DrawMode = DrawMode.OwnerDrawFixed;
            guna2ComboBox1.DrawItem += Guna2ComboBox1_DrawItem;
            guna2ComboBox1.Items.Add("Key Finance");
            guna2ComboBox1.Items.Add("Key Consulting");
            guna2ComboBox1.ItemHeight = 30;
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {
            guna2GroupBox1.BackColor = Color.FromArgb(0, 145, 179);

            Fiscal fs = new Fiscal();
            fs.ShowDialog();


        }

        private void guna2GroupBox1_MouseHover(object sender, EventArgs e)
        {
            guna2GroupBox1.BackColor = ColorTranslator.FromHtml("#102328");



        }


        private void Guna2ComboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Always draw the background first
            e.DrawBackground();

            if (e.Index < 0) return;

            // 1. Draw blue checkbox square
            Rectangle boxRect = new Rectangle(
                e.Bounds.X + 5,
                e.Bounds.Y + (e.Bounds.Height - 16) / 2, // Center vertically
                16,
                16
            );

            using (Pen bluePen = new Pen(Color.FromArgb(0, 82, 204), 2))
            {
                e.Graphics.DrawRectangle(bluePen, boxRect);
            }

            // 2. Draw text
            using (SolidBrush textBrush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(
                    guna2ComboBox1.Items[e.Index].ToString(),
                    new Font("Segoe UI", 10),
                    textBrush,
                    e.Bounds.X + 25, // Right of checkbox
                    e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2 // Center text
                );
            }
        }
        private void guna2GroupBox7_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox2_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.Gray;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void guna2GroupBox3_Click(object sender, EventArgs e)
        {


            this.BackColor = Color.FromArgb(0, 145, 179);
            LoadFormInPanel(new AddFacture());

        }

        private void guna2GroupBox1_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.Transparent;
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2ComboBox1_MouseHover(object sender, EventArgs e)
        {
            //guna2ComboBox1.FillColor = ColorTranslator.FromHtml("#f9e4a7");
        }

        private void guna2ComboBox1_MouseLeave(object sender, EventArgs e)
        {
            //   guna2ComboBox1.FillColor = Color.Black;
        }

        private void guna2ContainerControl2_Click(object sender, EventArgs e)
        {

        }
        private void LoadFormInPanel(Form form)
        {
            guna2Panel3.Controls.Clear();           // Clear any existing control
            form.TopLevel = false;                  // Critical: make it a child
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            guna2Panel3.Controls.Add(form);
            form.Show();                            // Important: show the form
        }


        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ComboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
        }

        private void guna2GroupBox5_Click(object sender, EventArgs e)
        {
            guna2GroupBox5.BackColor = Color.LightBlue;
        }

        private void guna2ComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
