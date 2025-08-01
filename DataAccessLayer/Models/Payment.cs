using DataAccessLayer.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccessLayer.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public string MethodeDePayment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

       public string reference { get; set; }

        public string Note { get; set; }

        public virtual ICollection<PaymentDocument> Documents { get; set; }

        public string devis { get; set; }
         
        public decimal rate { get; set; }

        public bool IsAutrePayment { get; set; } = false;

        public string Type { get; set; }

        

        [ForeignKey("RegisteredBy")]
        public int RegisteredById { get; set; }
        public Utilisatuer RegisteredBy { get; set; }
       
        [ForeignKey("comptebancaireId")]
        public int ? comptebancaireId { get; set; }
       
        public CompteBancaire compteBancaire { get; set; }


        public string entityname { get; set; }

        public string registername { get; set; }

        [ForeignKey("entity")]
        public int entityid { get; set; }

        public Entity entity { get; set; }
    }

}