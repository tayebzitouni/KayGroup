using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.user_controls
{
    public partial class Retard : UserControl
    {
        public Retard()
        {
            InitializeComponent();
        }
        public Retard(FactureClient f)
        {
            InitializeComponent();
            guna2HtmlLabel1.Text = f.name+"-"+f.id;
            guna2HtmlLabel2.Text = f.client.Name + " pour " + f.entity.Name;
            guna2HtmlLabel3.Text = (f.Total-f.payed).ToString()+" "+f.devis;
        }

        public Retard(FactureFournisseur f)
        {
            InitializeComponent();
            guna2HtmlLabel1.Text = f.name + "-" + f.id;
            guna2HtmlLabel2.Text = f.fournisseur.Name + " pour " + f.entity.Name;
            guna2HtmlLabel3.Text = (f.Total - f.payed).ToString() +" "+f.devis;
        }


        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {

        }

        private void DrawStatusIndicator(PaintEventArgs e, string status, int textX, int textY)
        {
            // Skip if no status
            if (string.IsNullOrEmpty(status))
                return;

            // Determine dot color
            Color dotColor = Color.Gray;
            if (status.StartsWith("En retard", StringComparison.OrdinalIgnoreCase))
                dotColor = Color.Red;
            // Add other status colors here if needed

            // Dot settings
            int dotSize = 8;
            int spacing = 5; // Space between dot and text

            // Calculate dot position (left of text)
            int dotX = textX - dotSize - spacing;
            int dotY = textY + (int)(e.Graphics.MeasureString("A", this.Font).Height - dotSize) / 2;

            // Draw the dot
            using (Brush brush = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }
        }


        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            // Your existing text drawing code
            string status = "En retard";
            int textX = 307; // Your text's X position
            int textY = 22;  // Your text's Y position

           
            // Then draw the status indicator
            DrawStatusIndicator(e, status, textX, textY);
        }

        private void Retard_Load(object sender, EventArgs e)
        {
            
        }
    }
}
