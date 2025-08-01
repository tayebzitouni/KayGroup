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
using Microsoft.Extensions.Primitives;


namespace DataAccessLayer.Controllers
    {
        public static class UtilisateurController
        {
            private static  AppDbContext _context = new AppDbContext();

            public static void initialize(AppDbContext context)
            {
                _context = context;
            }


        public static async Task<bool> CreateUtilisateurAsync(Dtos.Dtos.UtilisatuerDto dto)
        {
            Utilisatuer temp = new Utilisatuer();
            temp.Email = dto.Email;
            temp.EntityId = dto.EntityId;
            temp.Password = dto.Password;
            temp.Role = dto.Role;
            temp.Name = dto.Name;
            temp.phone = dto.phone;
            try
            {
                _context.Utilisateurs.Add(temp);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        
        }

            public static async Task<Utilisatuer?> GetUtilisateurByIdAsync(int id)
            {
                return await _context.Utilisateurs
                    .Include(u => u.entity)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }

            public static async Task<List<Utilisatuer>> GetAllUtilisateursAsync()
            {
            return await _context.Utilisateurs
                    .ToListAsync();
            }

        public static  List<Utilisatuer> GetAllUtilisateursNonAsync()
        {
            return  _context.Utilisateurs
                    .ToList();
        }

        public static async Task DeleteAsync(Utilisatuer utilisatuer)
        {
            _context.Utilisateurs.Remove(utilisatuer);
            await _context.SaveChangesAsync();
        }


        public static async Task<bool> UpdateUtilisateurAsync(string email, Dtos.Dtos.UtilisatuerDto dto)
            {


            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

            if (utilisateur == null)
                    return false;
                
                utilisateur.Name = dto.Name;
                utilisateur.Email = dto.Email;
                utilisateur.Password = dto.Password;
                utilisateur.Role = dto.Role;
                utilisateur.EntityId = dto.EntityId;
            utilisateur.phone = dto.phone;
               
                await _context.SaveChangesAsync();
                return true;
            }

          
            public static async Task<Utilisatuer?> GetUtilisateurByEmailAsync(string email)
            {
                return await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == email);
            }

        public static async Task<Utilisatuer?> IsEmailFoundAsync(string email)
        {
            return await _context.Utilisateurs
                .SingleAsync(u => u.Email == email);
        }



        public static async Task<bool> Login(string email, string password)
        {
           data.Session.CurrentUser = _context.Utilisateurs
                .Include(u => u.entity)
                
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            return  data.Session.CurrentUser != null;
        }
        public static void Logout()
        {
            data.Session.CurrentUser = null;

        }
    }
}
