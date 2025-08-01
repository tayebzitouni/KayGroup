using BusinessAccessLayer;
using BussinessAcesssLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer.usefulFunction;
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
    public partial class tresoireRecevoire : UserControl
    {
        public tresoireRecevoire(DateTime from , DateTime to ,Dtos.Dtos.EntityDto entity)
        {
            InitializeComponent();
            label14.Text = entity.Name + " (" + entity.code + ")";
            decimal a = FactureClientBussinesLayer.GetTVACollectedInCurrentTrimByEntitu(from, to, entity.Id);
            decimal b = FactureFournisseurBusinessLayer.GetTVACollectedInCurrentTrimByEntitu(from , to ,entity.Id);
            decimal c =FactureFournisseurBusinessLayer.GetTotalTVAReturnCollectedInCurrentTrimesterbyid(from, to, entity.Id);
            label1.Text = a.ToString() + " MAD";
            label2.Text = b.ToString() + " MAD";
            label3.Text = (a - b-c).ToString() + " MAD";
            UsefulFuncitonClass.TextColor(a - b-c, label3);
        }

        private void tresoireRecevoire_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
