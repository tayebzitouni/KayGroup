using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dtos.Dtos;

namespace DataAccessLayer.Controllers
{
    public static class CompteBancaireController
    {
        public static async Task<List<CompteBancaire>> GetAllAsync()
        {
            using var db = new AppDbContext();
            return await db.compteBancaires.Include(p=>p.Entite).ToListAsync();
        }

        public static async Task<CompteBancaire?> GetByIdAsync(string id)
        {
            using var db = new AppDbContext();
            return await db.compteBancaires.Include(c => c.Entite)
                                            .FirstOrDefaultAsync(c => c.RIB == id);
        }

        public static async Task<CompteBancaire?> RealGetByIdAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.compteBancaires
                                            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public static async Task CreateAsync(CompteBancaireDto dto)
        {
            using var db = new AppDbContext();
            var cb = new CompteBancaire
            {
                Intitule = dto.Intitule,
                Banque = dto.Banque,
                Agence = dto.Agence,
                RIB = dto.RIB,
                IBAN = dto.IBAN,
                SwiftCode = dto.SwiftCode,
                Devise = dto.Devise,
                SoldeInitial = dto.SoldeInitial,
                DateOuverture = dto.DateOuverture,
                EstActif = dto.EstActif,
                EntiteId = dto.EntiteId
            };

            db.compteBancaires.Add(cb);
            await db.SaveChangesAsync();
        }
        public static List<CompteBancaire> GetByEntiteId(int? entiteId)
        {
            if (entiteId == null || entiteId <= 0)
                return new List<CompteBancaire>();

            using (var context = new AppDbContext())
            {
                return context.compteBancaires
                              .Where(cb => cb.EntiteId == entiteId)
                              .Include(cb => cb.Entite) // to get entitename
                              .ToList();
            }
        }

        public static async Task UpdateAsync(int id, CompteBancaireDto dto)
        {
            using var db = new AppDbContext();
            var cb = await db.compteBancaires.FindAsync(id);
            if (cb == null) return;

            cb.Intitule = dto.Intitule;
            cb.Banque = dto.Banque;
            cb.Agence = dto.Agence;
            cb.RIB = dto.RIB;
            cb.IBAN = dto.IBAN;
            cb.SwiftCode = dto.SwiftCode;
            cb.Devise = dto.Devise;
            cb.SoldeInitial = dto.SoldeInitial;
            
            cb.EstActif = dto.EstActif;
            cb.EntiteId = dto.EntiteId;

            db.compteBancaires.Update(cb);
            await db.SaveChangesAsync();
        }

        public static async Task<string> DeleteAsync(CompteBancaire cb)
        {
            using var db = new AppDbContext();
            db.compteBancaires.Remove(cb);
            await db.SaveChangesAsync();
            return "Compte supprimé avec succès";
        }
    }
}
