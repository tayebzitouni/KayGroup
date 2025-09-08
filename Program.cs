
using BussinessAcesssLayer;
using DataAccessLayer;
using DataAccessLayer.Models;
using freelanceProject1.Presentation_Layer;
using freelanceProject1.Presentation_Layer.forms;

namespace freelanceProject1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            using var appDbContext = new AppDbContext();

            appDbContext.Database.EnsureCreated();


            SettingsService.Initialize(appDbContext);


            var settings = appDbContext.UserSettings.FirstOrDefault();
            if (settings == null || settings.Id == 0)
            {
                settings = new UserSettings();
                UserSettingsManager.Save(appDbContext, settings);
            }

            var settings3 = appDbContext.Entities.FirstOrDefault();
            if (settings3 == null || settings3.id == 0)
            {
                try
                {
                    settings3 = new Entity
                    {
                        Name = "Kay Group ",
                        code = "KAY-GRP",
                        Email = "hi@gmail.com",
                        Adress = "hh",
                        ICE= "gh",
                        Patent ="hg",
                        Nom = "gg",
                        CNSS = "hf",
                        Phone="ae",
                        RC= "fdhd",
                        identifiantfiscal ="jfd",
        
                    };
                    appDbContext.Entities.Add(settings3);
                    appDbContext.SaveChanges();
                }
                catch (Exception)
                {
                    MessageBox.Show("There is Error");
                }
            }

            var settings2 = appDbContext.Utilisateurs.FirstOrDefault();
            if (settings2 == null || settings2.Id == 0)
            {
                try
                {
                    settings2 = new Utilisatuer
                    {
                        Name = "Admin",
                        Email = "Admin@gmail.com",
                        phone = "+213",
                        Role = "Admin",
                        Password = "123456",
                        EntityId = appDbContext.Entities.FirstOrDefault().id
                    };
                    appDbContext.Utilisateurs.Add(settings2);
                    appDbContext.SaveChanges();
                }
                catch(Exception)
                {
                    MessageBox.Show("There is Error");
                }
            }
            Application.Run(new LogIn());
          
           
            






        }
    }
};