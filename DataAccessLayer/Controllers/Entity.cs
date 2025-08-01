using DataAccessLayer.data;

using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dtos;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;


namespace DataAccessLayer.Controllers
    {
        public static class EntityController
        {
            private static  AppDbContext _context = new AppDbContext();

            public static void initialize(AppDbContext context)
            {
                _context = context;
            }

  
           
        public static async Task<List<Entity>> GetAllEntitesAsync()
            {
            return await _context.Entities
                    .ToListAsync();
            }

        public static  List<Entity> GetAllEntitesAsyncNotAsync()
        {
            return  _context.Entities
                    .ToList();
        }

        

        public static async Task<bool> CreateEntityAsync(Models.Entity dto)
        {
            
            try
            {
                _context.Entities.Add(dto);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<Models.Entity> IsEntityFoundAsync(string name)
        {
            return await _context.Entities
                .FirstOrDefaultAsync(e => e.Name == name);
        }
        public static async Task<Models.Entity> IsEntityFoundAsync2(string name)
        {
            return await _context.Entities
                .FirstOrDefaultAsync(e => e.identifiantfiscal == name);
        }


        public static async Task<Entity?> GetEntityByNameAsync(string name)
        {
            return await _context.Entities
               
                .FirstOrDefaultAsync(u => u.Name == name);
        }


        public static async Task DeleteAsync(Entity utilisatuer)
        {
            _context.Entities.Remove(utilisatuer);
            await _context.SaveChangesAsync();
        }


        public static async Task<bool> UpdateEntityAsync(string name, Dtos.Dtos.EntityDto dto)
        {


            var utilisateur = await _context.Entities.FirstOrDefaultAsync(u => u.Name == name);

            if (utilisateur == null)
                return false;

            utilisateur.Name = dto.Name;
            utilisateur.code = dto.code;
            utilisateur.Adress = dto.Adress;
            utilisateur.identifiantfiscal = dto.identifiantfiscal;
            utilisateur.RC = dto.RC;
            utilisateur.CNSS = dto.CNSS;
            utilisateur.Email = dto.Email;
            utilisateur.ICE = dto.ICE;
            utilisateur.Nom = dto.Nom;
            utilisateur.Patent = dto.Patent;
            utilisateur.Phone = dto.Phone;

         

            await _context.SaveChangesAsync();
            return true;
        }


   
            public static int GetRemainingDaysForTVA()
            {
                DateTime today = DateTime.Today;
                int currentMonth = today.Month;
                DateTime nextDeadline;

                if (currentMonth >= 1 && currentMonth <= 3)
                    nextDeadline = new DateTime(today.Year, 4, 30); // Q1 → 20 أبريل
                else if (currentMonth >= 4 && currentMonth <= 6)
                    nextDeadline = new DateTime(today.Year, 7, 30); // Q2 → 20 يوليوز
                else if (currentMonth >= 7 && currentMonth <= 9)
                    nextDeadline = new DateTime(today.Year, 10, 30); // Q3 → 20 أكتوبر
                else // من أكتوبر إلى ديسمبر
                    nextDeadline = new DateTime(today.Year + 1, 1, 30); // Q4 → 20 يناير السنة القادمة

                int daysLeft = (nextDeadline - today).Days;
                return daysLeft < 0 ? 0 : daysLeft;
            }

        





    }
}
