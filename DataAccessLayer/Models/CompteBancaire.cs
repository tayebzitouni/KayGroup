using iText.StyledXmlParser.Jsoup.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class CompteBancaire
    {
   
            public int Id { get; set; }
            public string Intitule { get; set; }
            public string Banque { get; set; }
            public string Agence { get; set; }
            public string RIB { get; set; }
            public string IBAN { get; set; }
            public string SwiftCode { get; set; }
            public string Devise { get; set; }
            public decimal SoldeInitial { get; set; }
            public DateTime DateOuverture { get; set; }

            public int EntiteId { get; set; }
            public Entity Entite { get; set; }

            public bool EstActif { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
       
        

    }
    
}
