using DataAccessLayer.data;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dtos.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataAccessLayer.Controllers
{
    public class FactureClientController
    {
        public static async Task<bool> CreateFactureClientAsync(decimal utiliser, FactureClientDto dto)
        {
            using var context = new AppDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var facture = new FactureClient
                {
                    name = "FC",
                    DateEcheance = dto.DateEcheance,
                    MontantTH = dto.MontantTH,
                    TVa = dto.TVa,
                    Total = dto.Total,
                    ModeDePayment = dto.ModeDePayment,
                    Description = dto.Description,
                    entiteId = dto.entiteId,
                    DateEmission = dto.DateEmission,
                    clientId = dto.clientId,
                    Status = dto.Status,
                    payed = dto.payed,
                    devis = dto.devis,
                    rate = dto.rate
                };

                context.factureClients.Add(facture);
                await context.SaveChangesAsync();

                var client = ClientController.GetClientById(facture.clientId);
                if (client.StatusTVA == "Exonéré")
                {
                    client.ExnUtiliser = utiliser;
                    ClientController.UpdateClientModelAsync(client.identifiantFiscal, client);
                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public static async Task<List<FactureClient?>> GetFactureClientsByClientIdAsync(int id)
        {
            using var context = new AppDbContext();
            return await context.factureClients
                .Where(f => f.clientId == id)
                .Include(f => f.client)
                .Include(f => f.entity)
                .ToListAsync();
        }

        public static async Task<FactureClient?> GetFactureClientByIdAsync(int id)
        {
            using var context = new AppDbContext();
            return await context.factureClients
                .Include(f => f.client)
                .Include(f => f.entity)
                .FirstOrDefaultAsync(f => f.id == id);
        }

        public static async Task<List<FactureClient>> GetAllFactureClientsAsync()
        {
            using var context = new AppDbContext();
            return await context.factureClients
                .Include(f => f.client)
                .Include(f => f.entity)
                .ToListAsync();
        }

        public static async Task<List<FactureClient>> GetLast10FactureClientsAsync()
        {
            using (var context = new AppDbContext())
            {
                return await context.factureClients
                    .OrderByDescending(f => f.id) // أو f.DateEmission إن وُجد
                    .Take(10)
                    .Include(f => f.client)
                    .Include(f => f.entity)
                    .ToListAsync();
            }
        }


        public static async Task<bool> DeleteAsync(int factureId)
        {
            using var context = new AppDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var facture = await context.factureClients.FirstOrDefaultAsync(f => f.id == factureId);
                if (facture == null) return false;

                var client = ClientController.GetClientById(facture.clientId);

                if (client.StatusTVA == "Exonéré")
                {
                    client.ExnUtiliser -= facture.MontantTH;
                    if (client.ExnUtiliser < 0) client.ExnUtiliser = 0;
                    ClientController.UpdateClientModelAsync(client.identifiantFiscal, client);
                }

                context.factureClients.Remove(facture);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public static async Task<bool> UpdateUsedTaxExemptAmount(string clientId, decimal newUsedAmount)
        {
            using var context = new AppDbContext();
            var client = context.clients.FirstOrDefault(c => c.identifiantFiscal == clientId);
            if (client == null) return false;

            client.ExnUtiliser = newUsedAmount;
            context.SaveChanges();
            return true;
        }

        public static async Task<bool> UpdateFactureClientAsync(decimal utiliser, decimal oldht, int id, FactureClientDto dto)
        {
            using var context = new AppDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var facture = await context.factureClients.FirstOrDefaultAsync(f => f.id == id);
                if (facture == null) return false;

                var client = ClientController.GetClientById(facture.clientId);

                facture.DateEcheance = dto.DateEcheance;
                facture.MontantTH = dto.MontantTH;
                facture.TVa = dto.TVa;
                facture.Total = dto.Total;
                facture.ModeDePayment = dto.ModeDePayment;
                facture.Description = dto.Description;
                facture.Status = dto.Status;
                facture.payed = dto.payed;
                facture.clientId = dto.clientId;
                facture.entiteId = dto.entiteId;
                facture.DateEmission = dto.DateEmission;
                facture.DateEcheance = dto.DateEcheance;
                facture.devis = dto.devis;
                facture.rate = dto.rate;
                if (client.StatusTVA == "Exonéré")
                {
                    client.ExnUtiliser = utiliser;
                    ClientController.UpdateClientModelAsync(client.identifiantFiscal, client);
                }
                if (dto.payed >= dto.Total)
                {
                    facture.Status = "Payé";
                }
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public static decimal GetTotalAmountOfAllFacturesAsync(DateTime from, DateTime to)
        {
            using var context = new AppDbContext();
            string startStr = from.ToString("yyyy-MM-dd");
            string endStr = to.ToString("yyyy-MM-dd");

            var invoicesInPeriod = context.factureClients
                .Where(f => f.DateEmission != null &&
                           f.DateEmission.CompareTo(startStr) >= 0 &&
                           f.DateEcheance.CompareTo(endStr) <= 0)
                .ToList();

            return invoicesInPeriod.Sum(f => f.Total*f.rate);
        }

        public static decimal GetTotalAmountOfNonpayeAndRetardFacturesAsync()
        {
            using var context = new AppDbContext();
            var invoicesInPeriod = context.factureClients
                .Where(f => f.Status == "En retard" || f.Status == "Non payé")
                .ToList();

            return invoicesInPeriod.Sum(f => (f.Total-f.payed)*f.rate);
        }

        public static decimal GetTotalAmountOfEntityFacturesAsync(int id)
        {
            using var context = new AppDbContext();
            return context.factureClients
                .Where(f => f.entiteId == id && f.Status != "Payé")
                .Sum(f => ((f.Total-f.payed)*f.rate));
        }

        public static async Task<List<FactureClient>> GetOverdueFacturesAsync()
        {
            using var context = new AppDbContext();
            var allFactures = await context.factureClients
                .Include(f => f.client)
                .Include(f => f.entity)
                .ToListAsync();

            return allFactures
                .Where(f => UsefulData.IsDateBeforeToday(f.DateEcheance) &&
                            f.Status != "Payé")
                .OrderByDescending(f => UsefulData.ParseDate(f.DateEcheance))
                .ToList();
        }

        public static async Task<List<FactureClient>> GetOverdueFacturesAsync2()
        {
            using var context = new AppDbContext();
            var allFactures = await context.factureClients
                .Include(f => f.client)
                .Include(f => f.entity)
                .ToListAsync();

            return allFactures
                .Where(f =>
                            f.Status != "Payé")
                .OrderByDescending(f => UsefulData.ParseDate(f.DateEcheance))
                .ToList();
        }
        public static async Task<List<FactureClient>> GetOverdueFacturesAsync2(int entityid, int id, string devis)
        {
            using var context = new AppDbContext();
            var allFactures = await context.factureClients.Where(p=>p.clientId==id && p.entiteId == entityid && p.devis ==devis )
                .Include(f => f.client)
                .Include(f => f.entity)
                .ToListAsync();

            return allFactures
                .Where(f =>
                            f.Status != "Payé")
                .OrderByDescending(f => UsefulData.ParseDate(f.DateEcheance))
                .ToList();
        }


        public static decimal GetTotalTVACollectedInCurrentTrimester(DateTime startTrimester, DateTime endTrimester)
        {
            using var context = new AppDbContext();
            string startStr = startTrimester.ToString("yyyy-MM-dd");
            string endStr = endTrimester.ToString("yyyy-MM-dd");

            var invoicesInPeriod = context.factureClients
                .Where(f => f.DateEmission != null &&
                           f.DateEmission.CompareTo(startStr) >= 0 &&
                           f.DateEmission.CompareTo(endStr) <= 0)
                .ToList();

            return invoicesInPeriod.Sum(f => f.TVa*f.rate);
        }

        public static decimal GetTotalTVACollectedInCurrentTrimester(DateTime startTrimester, DateTime endTrimester, int id)
        {
            using var context = new AppDbContext();
            string startStr = startTrimester.ToString("yyyy-MM-dd");
            string endStr = endTrimester.ToString("yyyy-MM-dd");

            var invoicesInPeriod = context.factureClients
                .Where(f => f.DateEmission != null &&
                           f.DateEmission.CompareTo(startStr) >= 0 &&
                           f.DateEmission.CompareTo(endStr) <= 0 &&
                           f.entiteId == id)
                .ToList();

            return invoicesInPeriod.Sum(f => f.TVa * f.rate);
        }
    }
}
