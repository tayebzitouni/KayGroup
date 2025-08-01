using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Controllers
{
    public class PaymentDocumentController
    {

        private readonly AppDbContext _context;

        public PaymentDocumentController(AppDbContext context)
        {
            _context = context;
        }

        public List<PaymentDocument> GetAll()
        {
            return _context.PaymentDocuments.ToList();
        }

        public PaymentDocument GetById(int id)
        {
            return _context.PaymentDocuments.Find(id);
        }

        public void Add(PaymentDocument document)
        {
            _context.PaymentDocuments.Add(document);
            _context.SaveChanges();
        }

        public void Update(PaymentDocument document)
        {
            var existing = _context.PaymentDocuments.Find(document.DocumentId);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(document);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var document = _context.PaymentDocuments.Find(id);
            if (document != null)
            {
                _context.PaymentDocuments.Remove(document);
                _context.SaveChanges();
            }
        }
    }

}

