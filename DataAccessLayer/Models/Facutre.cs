using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Facutre
    {
        public int id { get; set; }
        public string name { get; set; }
        public string DateEcheance { get; set; }
        public decimal MontantTH { get; set; }
        public decimal TVa { get; set; }
        public decimal Total { get; set; }
        public string ModeDePayment { get; set; }
        public String Description { get; set; }
        public decimal rate { get; set; }
        public string devis { get; set; }
        [ForeignKey("entity")]
        public int entiteId { get; set; }
        public Entity entity { get; set; } 
        public String Status { get; set; }
       public decimal payed { get; set; }  
    }
}
