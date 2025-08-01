using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dtos.Dtos;

namespace DataAccessLayer.Controllers
{
    public  class pyamentUtilsateurController
    {
        
            public static async Task AddAsync(PaymentUtilisatuer payment, AppDbContext context)
            {
                await context.paymentUtilisatuers.AddAsync(payment);
                await context.SaveChangesAsync();
            }

            public static async Task<PaymentUtilisatuer?> GetByIdAsync(int id)
            {
                using var context = new AppDbContext();
                return await context.paymentUtilisatuers
                   .Include(p => p.entity).Include(p => p.RegisteredBy).Include(p=>p.UsedBy)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            public static async Task<List<PaymentUtilisatuer>> GetAllAsync()
            {
                using var context = new AppDbContext();
                return await context.paymentUtilisatuers
                   .Where(p => p.IsAutrePayment == false)
                  .Include(p => p.entity).Include(p => p.RegisteredBy)

                    .ToListAsync();
            }



        public static decimal GetEntityPaymentAsync(int entityid, DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            return context.paymentUtilisatuers.Where(p => p.entityid == entityid &&  p.IsAutrePayment == false && (p.PaymentDate > from && p.PaymentDate < to)).Sum(p => p.Amount);
        }

        public static decimal GetEntityDebitAsync(int entityid, DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            return context.paymentUtilisatuers.Where(p => p.entityid == entityid && p.IsAutrePayment == false && (p.PaymentDate > from && p.PaymentDate < to)).Sum(p => p.debit);
        }


        public static async Task UpdateAsync(PaymentUtilisatuer payment, AppDbContext context)
            {
                context.paymentUtilisatuers.Update(payment);
                await context.SaveChangesAsync();
            }

            public static async Task DeleteAsync(PaymentUtilisatuer payment, AppDbContext context)
            {
                context.paymentUtilisatuers.Remove(payment);
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
        public static List<PaymentByCompteResult> GetGroupedPaymentsByEntityAndCompte(DateTime from, DateTime to)
        {
            using var context = new AppDbContext();

            return context.paymentUtilisatuers
                .Where(p => !p.IsAutrePayment && p.PaymentDate >= from && p.PaymentDate <= to)
                .GroupBy(p => new { p.entityname, p.compte })
                .Select(g => new PaymentByCompteResult
                {
                    EntityName = g.Key.entityname,
                    Compte = g.Key.compte,
                    Total = g.Sum(x => x.debit)
                })
                .ToList();
        }


    }
}









