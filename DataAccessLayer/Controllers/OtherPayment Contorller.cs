using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Controllers
{
    public class OtherPayment_Contorller
    {
          public static async Task AddAsync(Payment payment, AppDbContext context)
            {
                await context.payments.AddAsync(payment);
                await context.SaveChangesAsync();
            }

        public static decimal GetEntityPaymentAsync(bool isincome ,int entityid, DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            if (isincome)
            {
                return context.payments.Where(p => p.entityid == entityid && p.IsAutrePayment == true && p.Type!="Outcomes" &&(p.PaymentDate > from && p.PaymentDate < to)).Sum(p => p.Amount);
            }
            else
            {
                return context.payments.Where(p => p.entityid == entityid && p.IsAutrePayment == true && p.Type=="Outcomes" && (p.PaymentDate > from && p.PaymentDate < to)).Sum(p => p.Amount);
            }
        }

        public static async Task<Payment?> GetByIdAsync(int id)
            {
                using var context = new AppDbContext();
                return await context.payments.
                Include(p => p.entity).Include(p => p.RegisteredBy)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            public static async Task<List<Payment>> GetAllAsync()
            {
                using var context = new AppDbContext();
                return await context.payments
                    .Include(p => p.entity).Include(p => p.RegisteredBy)
                    .Where(p=>p.IsAutrePayment == true)
                    .ToListAsync();
            }

            public static async Task UpdateAsync(Payment payment, AppDbContext context)
            {
                context.payments.Update(payment);
                await context.SaveChangesAsync();
            }

            public static async Task DeleteAsync(Payment payment, AppDbContext context)
            {
                context.payments.Remove(payment);
                await context.SaveChangesAsync();
            }

            public static void RemoveDocuments(IEnumerable<PaymentDocument> documents, AppDbContext context)
            {
                context.PaymentDocuments.RemoveRange(documents);
            }

            public static void AddDocuments(IEnumerable<PaymentDocument> documents, AppDbContext context)
            {
                context.PaymentDocuments.AddRange(documents);
            }
        
    }
}
