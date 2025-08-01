using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class PaymentUtilisatuer :Payment
    {
     
        public decimal debit { get; set; }
        public string Status { get; set; }
        [ForeignKey("UsedBy")]
        public int UsedById { get; set; }
        public Utilisatuer UsedBy { get; set; }
        public DateTime datedefacture { get; set; }
        public int months { get; set; }
        public string compte { get; set; }
        public string ville { get; set; }
       
    }
}