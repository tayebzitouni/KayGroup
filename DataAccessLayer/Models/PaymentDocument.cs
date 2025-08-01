using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataAccessLayer.Models
{
    public class PaymentDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [NotMapped] // ← هذه الخاصية ليست في قاعدة البيانات
        public string FileSourcePath { get; set; }

        [Required]
        public string FileName { get; set; }

        public string FilePath { get; set; } // Full path to the stored file

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [ForeignKey("PaymentId")]
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
    }

}
