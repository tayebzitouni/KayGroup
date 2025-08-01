using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static Dtos.Dtos;

namespace DataAccessLayer.Controllers
{
    public class ClientController
    {
        private static AppDbContext _context = new AppDbContext();

        public static void Initialize(AppDbContext context)
        {
            _context = context;
        }

        public static async Task<bool> CreateClientAsync(ClientDto dto)
        {
            var client = new Client
            {
               // id = dto.Id,
                Name = dto.Name,
                identifiantFiscal = dto.identifiantFiscal,
                StatusTVA = dto.StatusTVA,
                Contact = dto.Contact,
                DelayDePayment = dto.DelayDePayment,
                ExnLimite = dto.ExnLimite,
                ExnUtiliser = dto.ExnUtiliser,
                entityId = dto.entityId,
                Email = dto.Email,
                Phone = dto.Phone,

            };

            try
            {
                _context.clients.Add(client);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<Client?> GetClientByIdAsync(int id)
        {
            return await _context.clients
                .Include(c => c.entity)
                .FirstOrDefaultAsync(c => c.id == id);
        }
        public static async Task<Client?> GetClientByfiscalIdAsync(string id)
        {
            return await _context.clients
                .Include(c => c.entity)
                .FirstOrDefaultAsync(c => c.identifiantFiscal == id);
        }

        public static Client GetClientById(int id)
        {
            return  _context.clients
                .Include(c => c.entity)
                .FirstOrDefault(c => c.id == id);
        }


        //public static async Task<int> GetClientIdByFiscalIdAsync(int id)
        //{
        //    var client = _context.clients
        //        .FirstOrDefaultAsync(c => c.identifiantFiscal == id);
        //    if (client == null)
        //    {
        //        return -1;
        //    }
        //    return client.Result.id;
        //}

        public static async Task<List<Client>> GetAllClientsAsync()
        {
            return await _context.clients
                .Include(c => c.entity)
                .ToListAsync();
        }

        public static List<Client> GetAllClientsNoAsync()
        {
            return  _context.clients
                .Include(c => c.entity)
                .ToList();
        }

        public static async Task DeleteAsync(Client client)
        {
            
                _context.clients.Remove(client);
                await _context.SaveChangesAsync();
          
        }

        public static async Task<bool> UpdateClientAsync(string id, ClientDto dto)
        {
            var client = await _context.clients.FirstOrDefaultAsync(c => c.identifiantFiscal == id);
            if (client == null) return false;

            
            client.Name = dto.Name;
            client.identifiantFiscal = dto.identifiantFiscal;
            client.StatusTVA = dto.StatusTVA;
            client.Contact = dto.Contact;
            client.DelayDePayment = dto.DelayDePayment;
            client.ExnLimite = dto.ExnLimite;
            client.ExnUtiliser = dto.ExnUtiliser;
            client.entityId = dto.entityId;
            client.Email = dto.Email;
            client.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> UpdateClientModelAsync(string id, Client dto)
        {
            var client = await _context.clients.FirstOrDefaultAsync(c => c.identifiantFiscal == id);
            if (client == null) return false;


            client.Name = dto.Name;
            client.identifiantFiscal = dto.identifiantFiscal;
            client.StatusTVA = dto.StatusTVA;
            client.Contact = dto.Contact;
            client.DelayDePayment = dto.DelayDePayment;
            client.ExnLimite = dto.ExnLimite;
            client.ExnUtiliser = dto.ExnUtiliser;
            client.entityId = dto.entityId;
            client.Email = dto.Email;
            client.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return true;
        }


        public static async Task<bool> UpdateClientAsyncByID(int id, ClientDto dto)
        {
            var client = await _context.clients.FirstOrDefaultAsync(c => c.id == id);
            if (client == null) return false;


            client.Name = dto.Name;
            client.identifiantFiscal = dto.identifiantFiscal;
            client.id = dto.id;
            client.StatusTVA = dto.StatusTVA;
            client.Contact = dto.Contact;
            client.DelayDePayment = dto.DelayDePayment;
            client.ExnLimite = dto.ExnLimite;
            client.ExnUtiliser = dto.ExnUtiliser;
            client.entityId = dto.entityId;
            client.Email = dto.Email;
            client.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return true;
        }


        public static async Task<bool> GetClientByIdentifiantAsync(int id,string identifiant)
        {
            return  _context.clients
                .Any(c => c.identifiantFiscal == identifiant&& c.id!=id);
        }

        public static Client GetClientByIdentifiant(string identifiant)
        {
            return  _context.clients
                .FirstOrDefault(c => c.identifiantFiscal == identifiant);
        }
    }
}
