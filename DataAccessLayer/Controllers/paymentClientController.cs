using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Controllers
{
    public class paymentClientController
    {
        public static async Task AddAsync(PaymentParClient payment, AppDbContext context)
        {
            await context.paymentParClients.AddAsync(payment);
            await context.SaveChangesAsync();
        }

        public static async Task<PaymentParClient?> GetByIdAsync(int id)
        {
            using var context = new AppDbContext();
            return await context.paymentParClients
             
               .Include(p => p.entity).Include(p => p.RegisteredBy)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public static async Task<List<PaymentParClient>> GetAllAsync()
        {
            using var context = new AppDbContext();
            return await context.paymentParClients
                .Include(p => p.entity).Include(p => p.RegisteredBy).Include(p=>p.compteBancaire)
                  .Where(p => p.IsAutrePayment == false && p.Type!= "Outcomes")
                .ToListAsync();
        }


        public static  List<PaymentParClient> GetAllNoAsync()
        {
            using var context = new AppDbContext();
            return  context.paymentParClients
                .Include(p => p.entity).Include(p => p.RegisteredBy)
                  .Where(p => p.IsAutrePayment == false && p.Type != "Outcomes")
                .ToList();
        }


        public static decimal GetEntityPaymentAsync(int entityid, DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            return context.paymentParClients.Where(p => p.entityid == entityid && p.IsAutrePayment == false && p.Type != "Outcomes" && (p.PaymentDate > from && p.PaymentDate < to)).Sum(p => p.Amount);    
        }


        public static async Task UpdateAsync(PaymentParClient payment, AppDbContext context)
        {
            context.paymentParClients.Update(payment);
            await context.SaveChangesAsync();
        }

        public static async Task DeleteAsync(PaymentParClient payment, AppDbContext context)
        {
            context.paymentParClients.Remove(payment);
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

