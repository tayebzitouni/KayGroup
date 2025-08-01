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
    public class FournisseurController
    {
            private static AppDbContext _context = new AppDbContext();

            public static void Initialize(AppDbContext context)
            {
                _context = context;
            }

            public static async Task<bool> CreateFournisseurAsync(FournisseurDto dto)
            {
                var fournisseur = new Fournisseur
                {
                    Name = dto.Name,
                    identifiantFiscal = dto.identifiantFiscal,
                    StatusTVA = dto.StatusTVA,
                    Contact = dto.Contact,
                    Rib = dto.Rib,
                    entityId = dto.entityId,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    TauxDeReturn = dto.TauxDeReturn,
                    delay = dto.delay
                    
                };

                try
                {
                    _context.fournisseurs.Add(fournisseur);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static async Task<Fournisseur?> GetFournisseurByIdAsync(int id)
            {
                return await _context.fournisseurs
                    .Include(c => c.entity)
                    .FirstOrDefaultAsync(c => c.id == id);
            }

        public static  Fournisseur GetFournisseurById(int id)
        {
            return  _context.fournisseurs
                .Include(c => c.entity)
                .FirstOrDefault(c => c.id == id);
        }

        public static async Task<List<Fournisseur>> GetAllFournisseursAsync()
            {
                return await _context.fournisseurs
                    .Include(c => c.entity)
                    .ToListAsync();
            }


        public static  List<Fournisseur> GetAllFournisseursWithNoAsync()
        {
            return  _context.fournisseurs
                .Include(c => c.entity)
                .ToList();
        }

     
            public static async Task<string> DeleteAsync(Fournisseur client)
            {
            try
            {
                _context.fournisseurs.Remove(client);
                await _context.SaveChangesAsync();
                return "Sucess";
            }
            catch(Exception e)
            {
                return "Ce fournisseur a des factures et des paiements associés. Veuillez les supprimer d'abord.";
            }
            }

            public static async Task<bool> UpdateFournisserAsync(string id, FournisseurDto dto)
            {
                var client = await _context.fournisseurs.FirstOrDefaultAsync(c => c.identifiantFiscal == id);
                if (client == null) return false;
           
                client.Name = dto.Name; 
                client.StatusTVA = dto.StatusTVA;
                client.Contact = dto.Contact;
                client.TauxDeReturn = dto.TauxDeReturn;
                client.Rib = dto.Rib;
                client.entityId = dto.entityId;
                client.Email = dto.Email;
                client.Phone = dto.Phone;
            client.identifiantFiscal = dto.identifiantFiscal;
            client.delay = dto.delay;

                await _context.SaveChangesAsync();
                return true;
            }

        public static async Task<bool> UpdateFournisserAsyncByID(int id, FournisseurDto dto)
        {
            var client = await _context.fournisseurs.FirstOrDefaultAsync(c => c.id == id);
            if (client == null) return false;
            client.id = dto.id;
            client.Name = dto.Name;
            client.StatusTVA = dto.StatusTVA;
            client.Contact = dto.Contact;
            client.TauxDeReturn = dto.TauxDeReturn;
            client.Rib = dto.Rib;
            client.entityId = dto.entityId;
            client.Email = dto.Email;
            client.Phone = dto.Phone;
            client.delay = dto.delay;

            await _context.SaveChangesAsync();
            return true;
        }
        

        public static async Task<Fournisseur?> GetFournisseurByIdentifiantAsync(string identifiant)
            {
                return await _context.fournisseurs
                    .FirstOrDefaultAsync(c => c.identifiantFiscal.Trim() == identifiant);
            }
        
    public static bool GetUpdateFournisseurByIdentifiantAsync(int id ,string identifiant)
    {
        return  _context.fournisseurs.Any(c => c.identifiantFiscal == identifiant && c.id!=id);
    }
}
    }

