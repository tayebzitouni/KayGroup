using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    
        public class PaymentFournisseur : Payment
        {
         
        
      
      // [ForeignKey(nameof(factureFournisseur))]
        public string fournisseurFacture { get; set; }
        public string fournisseurname { get; set; }
       // public FactureFournisseur factureFournisseur { get; set; }
    }
    
}
