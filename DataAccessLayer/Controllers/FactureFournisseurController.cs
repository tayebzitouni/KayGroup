using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static Dtos.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataAccessLayer.Controllers
{
    public class FactureFournisseurController
    {
        public static async Task<bool> CreateFactureFournisseurAsync(FactureFournisseurDto dto)
        {
            using (var context = new AppDbContext())
            {
                var facture = new FactureFournisseur
                {
                    name = dto.name,
                    DateReception = dto.DateReception,
                    DateEcheance = dto.DateEcheance,
                    MontantTH = dto.MontantTH,
                    TVa = dto.TVa,
                    ModeDePayment = dto.ModeDePayment,
                    Description = dto.Description,
                    entiteId = dto.entiteId,
                    Retenue = dto.Retenue,
                    
                    Total = dto.Total,
                    Status = dto.Status,
                    fournisseurId = dto.fournisseurId,
                    payed = dto.payed,
                    rate = dto.rate,
                    devis = dto.devis

                };

                try
                {
                    context.factureFournisseurs.Add(facture);
                    await context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static async Task<FactureFournisseur?> GetFactureFournisseurByIdAsync(int id)
        {
            using (var context = new AppDbContext())
            {
                return await context.factureFournisseurs
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .FirstOrDefaultAsync(f => f.id == id);
            }
        }

        public static async Task<List<FactureFournisseur?>> GetFactureFournisseurByFournisseruIdAsync(int id)
        {
            using (var context = new AppDbContext())
            {
                return await context.factureFournisseurs.Where(f => f.fournisseurId == id)
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();
            }
        }

        public static async Task<List<FactureFournisseur>> GetAllFacturesFournisseurAsync()
        {
            using (var context = new AppDbContext())
            {
                return await context.factureFournisseurs
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();
            }
        }

        public static async Task<List<FactureFournisseur>> GetLast10FacturesFournisseurAsync()
        {
            using (var context = new AppDbContext())
            {
                return await context.factureFournisseurs
                    .OrderByDescending(f => f.id) // أو f.DateReception إن كانت تمثل تاريخ الإدخال
                    .Take(10)
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();
            }
        }


        public static async Task DeleteAsync(FactureFournisseur facture)
        {
            using (var context = new AppDbContext())
            {
                context.factureFournisseurs.Remove(facture);
                await context.SaveChangesAsync();
            }
        }

        public static async Task<bool> UpdateFactureFournisseurAsync(int id, FactureFournisseurDto dto)
        {
            using (var context = new AppDbContext())
            {
                var facture = await context.factureFournisseurs.FirstOrDefaultAsync(f => f.id == id);
                if (facture == null) return false;
               
                facture.name = dto.name;
                facture.DateReception = dto.DateReception;
                facture.DateEcheance = dto.DateEcheance;
                facture.MontantTH = dto.MontantTH;
                facture.TVa = dto.TVa;
                facture.Total = dto.Total;
                facture.Status = dto.Status;
                facture.ModeDePayment = dto.ModeDePayment;
                facture.Description = dto.Description;
                facture.entiteId = dto.entiteId;
                facture.Retenue = dto.Retenue;
                facture.payed = dto.payed;
                facture.fournisseurId = dto.fournisseurId;
                facture.payed = dto.payed;
                facture.rate = dto.rate;
                facture.devis = dto.devis;
                if (dto.payed >= dto.Total)
                {
                    facture.Status = "Payé";
                }
                await context.SaveChangesAsync();
                return true;
            }
        }

        public static decimal GetTotalAmountOfAllFacturesFournissuers(DateTime from, DateTime to)
        {
            using (var context = new AppDbContext())
            {
                string startStr = from.ToString("yyyy-MM-dd");
                string endStr = to.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateEcheance.CompareTo(endStr) <= 0)
                    .ToList();

                return invoicesInPeriod.Sum(f => f.Total*f.rate);
            }
        }

        public static decimal GetTotalAmountOfEntityFacturesFournissuers(int id)
        {
            using (var context = new AppDbContext())
            {
                return context.factureFournisseurs
                    .Where(f => f.entiteId == id && f.Status != "payé")
                    .Sum(f => ((f.Total-f.payed)*f.rate));
            }
        }

        public static async Task<List<FactureFournisseur>> GetOverdueFacturesAsync()
        {
            using (var context = new AppDbContext())
            {
                var allFactures = await context.factureFournisseurs
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();

                return allFactures
                    .Where(f => data.UsefulData.IsDateBeforeToday(f.DateEcheance) &&
                               (f.Status == "En retard" || f.Status == "Non payé" ||f.Status == "Partiellement payé"))
                    .OrderByDescending(f => data.UsefulData.ParseDate(f.DateEcheance))
                    .ToList();
            }
        }
        public static async Task<List<FactureFournisseur>> GetOverdueFacturesAsync2()
        {
            using (var context = new AppDbContext())
            {
                var allFactures = await context.factureFournisseurs
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();

                return allFactures
                    .Where(f => 
                               f.Status == "En retard" || f.Status == "Non payé" || f.Status == "Partiellement payé")
                    .OrderByDescending(f => data.UsefulData.ParseDate(f.DateEcheance))
                    .ToList();
            }
        }
        public static async Task<List<FactureFournisseur>> GetOverdueFacturesAsync2(int entityid,int id,string devis)
        {
            using (var context = new AppDbContext())
            {
                var allFactures = await context.factureFournisseurs.Where(p=>p.fournisseurId==id && p.devis==devis && p.entiteId==entityid)
                    .Include(f => f.fournisseur)
                    .Include(f => f.entity)
                    .ToListAsync();

                return allFactures
                    .Where(f =>
                               (f.Status == "En retard" || f.Status == "Non payé" || f.Status== "Partiellement payé"))
                    .OrderByDescending(f => data.UsefulData.ParseDate(f.DateEcheance))
                    .ToList();
            }
        }

        public static decimal GetTotalTVACollectedInCurrentTrimester(DateTime startTrimester, DateTime endTrimester)
        {
            using (var context = new AppDbContext())
            {
                string startStr = startTrimester.ToString("yyyy-MM-dd");
                string endStr = endTrimester.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateReception.CompareTo(endStr) <= 0)
                    .ToList();

                return invoicesInPeriod.Sum(f => (f.TVa - f.Retenue)*f.rate);
            }
        }

        public static decimal GetTotalRetardAmount()
        {
            using (var context = new AppDbContext())
            {
                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.Status == "En retard" || f.Status == "Non payé"|| f.Status == "Partiellement payé")
                    .ToList();

                return invoicesInPeriod.Sum(f => (f.Total-f.payed)*f.rate);
            }
        }
        public static decimal GetTotalRetardAmount2()
        {
            using (var context = new AppDbContext())
            {


                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateEcheance.CompareTo(DateTime.Today.ToString("yyyy-MM-dd")) < 0 && f.Status == "En retard" || f.Status == "Non payé"|| f.Status == "Partiellement payé")
                               
                    .ToList();

                return invoicesInPeriod.Sum(f => f.Total - f.payed);
            }
        }

        public static decimal GetTotalISICollectedInCurrentTrimester(DateTime startTrimester, DateTime endTrimester)
        {
            using (var context = new AppDbContext())
            {
                string startStr = startTrimester.ToString("yyyy-MM-dd");
                string endStr = endTrimester.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateEcheance.CompareTo(endStr) <= 0)
                    .ToList();

                return invoicesInPeriod.Sum(f => f.Retenue);
            }
        }

        public static decimal GetTotalReturnTVACollectedInCurrentTrimester(DateTime startTrimester, DateTime endTrimester)
        {
            using (var context = new AppDbContext())
            {
                string startStr = startTrimester.ToString("yyyy-MM-dd");
                string endStr = endTrimester.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateReception.CompareTo(endStr) <= 0)
                    .ToList();

                return invoicesInPeriod.Sum(f => f.Retenue*f.rate);
            }
        }
        public static decimal GetTotalReturnTVACollectedInCurrentTrimesterbyid(DateTime startTrimester, DateTime endTrimester,int id)
        {
            using (var context = new AppDbContext())
            {
                string startStr = startTrimester.ToString("yyyy-MM-dd");
                string endStr = endTrimester.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.entiteId==id && f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateReception.CompareTo(endStr) <= 0)
                    .ToList();

                return invoicesInPeriod.Sum(f => f.Retenue * f.rate);
            }
        }

        

        public static decimal GetTotalTVACollectedInCurrentTrimesterByEntity(int entityId, DateTime startTrimester, DateTime endTrimester)
        {
            using (var context = new AppDbContext())
            {
                string startStr = startTrimester.ToString("yyyy-MM-dd");
                string endStr = endTrimester.ToString("yyyy-MM-dd");

                var invoicesInPeriod = context.factureFournisseurs
                    .Where(f => f.DateReception != null &&
                               f.DateReception.CompareTo(startStr) >= 0 &&
                               f.DateReception.CompareTo(endStr) <= 0 &&
                               f.entiteId == entityId)
                    .ToList();

                return invoicesInPeriod.Sum(f => (f.TVa-f.Retenue)*f.rate);
            }
        }
    }
}
