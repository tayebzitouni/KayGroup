using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class PaymentParClient : Payment
    {
       
       
        public string  FactureClient { get; set; }
        public string clientname { get; set; }
       
    }
}
