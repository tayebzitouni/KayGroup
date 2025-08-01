using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class UserSettings
    {

        [Key]
        public int Id { get; set; }
        public decimal TvaRate { get; set; } = 0.2m;
        public decimal IsRate { get; set; } = 0.33m;
        public decimal RetenueRate { get; set; }= 0.1m;
        public string Name { get; set; } = "Kay Group";
        public string Devis { get; set; } = "MAD";
        public string Annefiscal { get; set; } = DateTime.Now.Year.ToString();
        public DateTime LastModified { get; set; }
    }
}
