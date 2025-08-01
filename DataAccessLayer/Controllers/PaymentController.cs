using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Controllers
{
    public class PaymentFournisseurRepository
    {
        public static async Task AddAsync(PaymentFournisseur payment, AppDbContext context)
        {
            await context.paymentFournisseurs.AddAsync(payment);
            await context.SaveChangesAsync();
        }

        public static async Task<PaymentFournisseur?> GetByIdAsync(int id)
        {
            using var context = new AppDbContext();
            return await context.paymentFournisseurs
          //Include(p => p.factureFournisseur)
               .Include(p => p.entity).Include(p => p.RegisteredBy)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public static async Task<decimal> GetTotalOutcomesInMADAsync()
        {
            using var context = new AppDbContext();

            // --- 1. Outcomes من PaymentFournisseur ---
            var fournisseurOutcomes = await context.paymentFournisseurs
                .Where(p => p.Type == "Outcomes")
                .SumAsync(p => p.Amount * p.rate);

            // --- 2. Outcomes من PaymentUtilisateur ---
            var utilisateurOutcomes = await context.paymentUtilisatuers
                .Where(p => p.Type == "Outcomes")
                .SumAsync(p => p.Amount * p.rate);

            // --- 3. Outcomes من جدول payments مباشرة (بدون أبناء) ---
            var directPayments = await context.payments
                .Where(p => p.Type == "Outcomes" &&
                            !context.paymentFournisseurs.Any(f => f.Id == p.Id) &&
                            !context.paymentUtilisatuers.Any(u => u.Id == p.Id))
                .SumAsync(p => p.Amount); // نأخذ القيمة كما هي (نفترض أنها بالدرهم)

            return fournisseurOutcomes + utilisateurOutcomes + directPayments;
        }

        public static async Task<decimal> GetTotalIncomesInMADAsync()
        {
            using var context = new AppDbContext();

            // --- 1. Incomes من PaymentParClient ---
            var clientIncomes = await context.paymentParClients
                .Where(p => p.Type == "Incomes")
                .SumAsync(p => p.Amount * p.rate);

            // --- 2. Incomes من PaymentUtilisateur ---
            var utilisateurIncomes = await context.paymentUtilisatuers
                .Where(p => p.Type == "Outcomes")
                .SumAsync(p => p.debit * p.rate);

            // --- 3. Incomes من جدول payments مباشرة (التي ليست مرتبطة بأي ابن) ---
            var directPayments = await context.payments
                .Where(p => p.Type == "Incomes" &&
                            !context.paymentParClients.Any(c => c.Id == p.Id) &&
                            !context.paymentUtilisatuers.Any(u => u.Id == p.Id))
                .SumAsync(p => p.Amount); // نأخذ القيمة كما هي (نفترض أنها بالدرهم)

            return clientIncomes + utilisateurIncomes + directPayments;
        }



        public static async Task<decimal> GetBankSolde()
        {
           
          

            using (var context = new AppDbContext())
            {
                decimal incomes = await GetTotalIncomesInMADAsync();
                decimal outcomes = await GetTotalOutcomesInMADAsync(); // من الكود السابق

                return incomes - outcomes;



            }
        }

        public static async Task<decimal> GetBankSolde(int year)
        {



            decimal solde = 0;
            using (var context = new AppDbContext())
            {
                var income = await context.payments
                    .Where(p => p.Type == "Incomes" && p.PaymentDate.Year == year)
                    .SumAsync(p => p.Amount);

                var outcome = await context.payments
                    .Where(p => p.Type == "Outcomes" && p.PaymentDate.Year == year)
                    .SumAsync(p => p.Amount);

                solde = income - outcome;


            }




            if (solde == null)
            {
                return 0;
            }


            return solde;
        }




        public static async Task<List<PaymentFournisseur>> GetAllAsync()
        {
            using var context = new AppDbContext();
            return await context.paymentFournisseurs.
              Include(p=>p.entity).Include(p=>p.RegisteredBy).Include
              (p=>p.compteBancaire)
                  .Where(p=>p.IsAutrePayment == false)
                .ToListAsync();
        }


        public static  decimal GetEntityPaymentAsync(int entityid, DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            return context.paymentFournisseurs.Where(p => p.entityid == entityid &&  p.IsAutrePayment == false && (p.PaymentDate>from && p.PaymentDate<to)).Sum(p => p.Amount);
        }


        public static async Task UpdateAsync(PaymentFournisseur payment, AppDbContext context)
        {
            context.paymentFournisseurs.Update(payment);
            await context.SaveChangesAsync();
        }

        public static async Task DeleteAsync(PaymentFournisseur payment, AppDbContext context)
        {
            context.paymentFournisseurs.Remove(payment);
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
