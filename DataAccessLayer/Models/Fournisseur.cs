using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Fournisseur
    {

        [Key]
        public int id { get; set; }

        [Required]
        public string Name { get; set; }

        public string identifiantFiscal { get; set; }

        public string StatusTVA { get; set; }

        public string Contact { get; set; }

        public String Email { get; set; }

        public string Phone { get; set; }

       public string Rib { get; set; }

       public double TauxDeReturn { get; set; }
        public int  delay { get; set; }

        [ForeignKey("entity")]
        public int entityId { get; set; }

        public Entity entity { get; set; }

        public ICollection<FactureFournisseur> factureFournisseurs { get; set; }
    }
}
