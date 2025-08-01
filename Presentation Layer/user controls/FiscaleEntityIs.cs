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
    public partial class FiscaleEntityIs : UserControl
    {
        public FiscaleEntityIs()
        {
            InitializeComponent();
        }
        public FiscaleEntityIs(Dtos.Dtos.EntityDto ent, int year)
        {
            InitializeComponent();
            label14.Text = ent.Name + "(" + ent.code + ")";
            decimal a = PaymentBussiness.getbanksolode(year);
            label1.Text = a.ToString()+ "MAD";
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
