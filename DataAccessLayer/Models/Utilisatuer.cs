using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Utilisatuer
    {
       
            [Key]
            virtual public int Id { get; set; }
            public string Name { get; set; }
        public string phone { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }


            #region foregnkeys
            public int EntityId { get; set; }
          public Entity entity { get; set; }
             
            #endregion
        }



    }

