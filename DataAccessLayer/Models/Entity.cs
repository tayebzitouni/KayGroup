using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Entity
    {
        [Key]
        public int id { get; set; }
        public string Name { get; set; }
        public string code { get; set; }
        public string Adress { get; set; }
        public string Patent { get; set; }
        public string identifiantfiscal { get; set; }
        public string RC { get; set; }
        public string ICE { get; set; }
        public string CNSS { get; set; }
        public string Nom { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public ICollection<FactureClient> factureClients { get; set; }
        public ICollection<FactureFournisseur> factureFournisseurs { get; set; }
        public ICollection<Client> clients { get; set; }
        public ICollection<Fournisseur> fournisseurs { get; set; }
        public ICollection<Utilisatuer> utilisatuers { get; set; }
        public ICollection<CompteBancaire> compteBancaires { get; set; }

    }
}
