using BussinessAcesssLayer;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace freelanceProject1.Presentation_Layer.user_controls
{
    public partial class PaymentParEntity : UserControl
    {
       
        
        private decimal CaculatPaymentFournisseurByEntityId(int entityId, DateTime from , DateTime to )
        {
            decimal a = PaymentFournisseurBusinessLayer.GetPaymentByEntityIdAsync(entityId,from,to);
            if (a>0)
            {
                return a;
            }
            else
            {
                return 0;
            }
              
        }

        private decimal CaculatPaymentUtilsateurByEntityId(int entityId,DateTime from , DateTime to)
        {
            decimal a = UtilisateurPaymentBussinessLayer.GetPaymentByEntityIdAsync(entityId,from,to);
            if (a > 0)
            {
                return a;
            }
            else
            {
                return 0;
            }

        }
        private decimal calcuatedebitutilisateur(int entityId, DateTime from, DateTime to)
        {
            decimal a = UtilisateurPaymentBussinessLayer.GetcebitByEntityIdAsync(entityId, from, to);
            if (a > 0)
            {
                return a;
            }
            else
            {
                return 0;
            }

        }


        private decimal CaculatPaymentByEntityId(bool isincom ,int entityId,DateTime from , DateTime to)
        {
            decimal a = PaymentBussiness.GetPaymentByEntityIdAsync(isincom,entityId,from ,to);
            if (a > 0)
            {
                return a;
            }
            else
            {
                return 0;
            }

        }


        private decimal CaculatPaymentClientByEntityId(int entityId, DateTime from, DateTime to)
        {
            decimal a = paymentClientBussinesslayer.GetPaymentByEntityIdAsync(entityId, from, to);
            if (a > 0)
            {
                return a;
            }
            else
            {
                return 0;
            }

        }



        //private decimal CaculatAutrePaymentIncomesByEntityId(bool isint entityId, DateTime from, DateTime to)
        //{
        //    decimal a = PaymentBussiness.GetPaymentByEntityIdAsync(entityId, from, to);
        //    if (a > 0)
        //    {
        //        return a;
        //    }
        //    else
        //    {
        //        return 0;
        //    }

        //}



        public PaymentParEntity(bool isincome ,Dtos.Dtos.EntityDto entity, DateTime from , DateTime to)
        {
            InitializeComponent();decimal a = 0; decimal b = 0; decimal c = 0;
            if (!isincome)
            {
                guna2HtmlLabel1.Text = entity.Name + "(" + entity.code + ")";
                a = CaculatPaymentFournisseurByEntityId(entity.Id, from, to);
                guna2HtmlLabel5.Text = a.ToString() + " MAD";
                b = CaculatPaymentUtilsateurByEntityId(entity.Id, from, to);
                guna2HtmlLabel4.Text = b.ToString() + " MAD";
                c = CaculatPaymentByEntityId(false,entity.Id, from, to);
                guna2HtmlLabel6.Text = c.ToString() + " MAD";
                //guna2HtmlLabel8.Text = (a + b + c).ToString() + " MAD";
                guna2HtmlLabel9.Text = calcuatedebitutilisateur(entity.Id, from, to).ToString()+" MAD";
            }
            else
            {
                guna2HtmlLabel2.Text = "Clients Paiements : ";
                guna2HtmlLabel3.Text = "Autre : ";
                guna2HtmlLabel7.Visible = false;
                guna2HtmlLabel6.Visible = false;
                guna2HtmlLabel9.Visible = false;
                guna2HtmlLabel10.Visible = false;
               //    guna2HtmlLabel8.Location = new Point(775,23);
               guna2HtmlLabel1.Text = entity.Name + "(" + entity.code + ")";
                a= CaculatPaymentClientByEntityId(entity.Id, from, to);
                guna2HtmlLabel5.Location = new Point(guna2HtmlLabel5.Location.X+30, guna2HtmlLabel5.Location.Y);
                guna2HtmlLabel5.Text = a.ToString() + " MAD";
                b = CaculatPaymentByEntityId(true, entity.Id, from, to);
                guna2HtmlLabel3.Location = new Point(guna2HtmlLabel3.Location.X+30, guna2HtmlLabel3.Location.Y);
                guna2HtmlLabel4.Location = new Point(guna2HtmlLabel4.Location.X + 20, guna2HtmlLabel4.Location.Y);
                guna2HtmlLabel4.Text = b.ToString() + " MAD";
               // guna2HtmlLabel8.Text = (a + b).ToString() + " MAD";

            }
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
