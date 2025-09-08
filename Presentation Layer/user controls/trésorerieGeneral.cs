using BussinessAcesssLayer;
using BussinessAcesssLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1
{
    public partial class trésorerieGeneral : UserControl
    {
        public trésorerieGeneral()
        {
            InitializeComponent();
        }

        public void calcuate(decimal a, decimal b, Label l)
        {
            decimal result = a - b;
            if (result > 0)
            {
                l.ForeColor = Color.FromArgb(0, 192, 0); 
              //  label5.ForeColor = Color.FromArgb(0, 192, 0);
               
            }
            else
            {
                l.ForeColor = Color.Red;
              //  label5.ForeColor = Color.Red;
              
            }
            l.Text = result.ToString()+ " MAD";

         

        }



        public trésorerieGeneral(Dtos.Dtos.EntityDto t)
        {
            InitializeComponent();
            decimal a = FactureClientBussinesLayer.GetTotalAmount_of_Entity_FacturesAsync(t.Id);
            decimal b = FactureFournisseurBusinessLayer.GetTotalAmountOfEntityFacturesAsync(t.Id);
            label14.Text = t.Name + "(" + t.code + ")";
            label1.Text = a.ToString() + " MAD";
            label4.Text = b.ToString() + " MAD";
            calcuate(a, b, label2);
        }

        private void trésorerieGeneral_Load(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
