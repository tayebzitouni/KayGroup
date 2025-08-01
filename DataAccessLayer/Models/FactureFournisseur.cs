using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class FactureFournisseur : Facutre
    {
        public string DateReception { get; set; }
        public decimal Retenue { get; set; }
        [ForeignKey("fournisseur")]
        public int fournisseurId { get; set; }
        public Fournisseur fournisseur { get; set; }
    }
}
