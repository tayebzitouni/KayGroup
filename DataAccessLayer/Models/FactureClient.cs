using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class FactureClient : Facutre
    {
        public string DateEmission { get; set; }
     [ForeignKey("client")]
     public int clientId { get; set; }
     public Client client { get; set; }
    }
}
