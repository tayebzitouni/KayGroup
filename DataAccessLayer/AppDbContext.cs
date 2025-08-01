using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection.Emit;

namespace DataAccessLayer
{
    public class AppDbContext : DbContext
    {
        public DbSet<Payment> payments { get; set; }
        public DbSet<PaymentFournisseur> paymentFournisseurs { get; set; }
        public DbSet<PaymentUtilisatuer> paymentUtilisatuers { get; set; }
        public DbSet<PaymentParClient> paymentParClients { get; set; }
        public DbSet<PaymentDocument> PaymentDocuments { get; set; }
        public DbSet<Entity> Entities { get; set; } 
        public DbSet<Utilisatuer> Utilisateurs { get; set; } 
        public DbSet<Client> clients  { get; set; }
        public DbSet<Fournisseur> fournisseurs { get; set; }
        public DbSet<FactureClient> factureClients { get; set; }
        public DbSet<FactureFournisseur> factureFournisseurs { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<CompteBancaire> compteBancaires { get; set; }





        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DATABASE", "KayGroupDb.mdf");

            //    optionsBuilder.UseSqlServer(
            //        $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True;TrustServerCertificate=True;",
            //        sqlOptions => sqlOptions.EnableRetryOnFailure()
            //    );
            //}

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
     "Server=localhost;Database=KayGroupDb;User Id=sa;Password=123456;TrustServerCertificate=True;"
    
 );

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
          

            modelBuilder.Entity<FactureClient>()
      .HasOne(fc => fc.client)
      .WithMany(c => c.FacturesClients)
      .HasForeignKey(fc => fc.clientId)
      .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<CompteBancaire>()
    .HasOne(fc => fc.Entite)
    .WithMany(c => c.compteBancaires)
    .HasForeignKey(fc => fc.EntiteId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FactureClient>()
                .HasOne(fc => fc.entity)
                .WithMany(e => e.factureClients)
                .HasForeignKey(fc => fc.entiteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Client>()
      .HasOne(fc => fc.entity)
      .WithMany(c => c.clients)
      .HasForeignKey(fc => fc.entityId)
      .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
    .HasOne(fc => fc.compteBancaire)
    .WithMany(c => c.Payments)
    .HasForeignKey(fc => fc.comptebancaireId)
    .OnDelete(DeleteBehavior.Cascade);

         


            modelBuilder.Entity<FactureFournisseur>()
    .HasOne(fc => fc.fournisseur)
    .WithMany(c => c.factureFournisseurs)
    .HasForeignKey(fc => fc.fournisseurId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FactureFournisseur>()
                .HasOne(fc => fc.entity)
                .WithMany(e => e.factureFournisseurs)
                .HasForeignKey(fc => fc.entiteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Fournisseur>()
      .HasOne(fc => fc.entity)
      .WithMany(c => c.fournisseurs)
      .HasForeignKey(fc => fc.entityId)
      .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
     .HasOne(p => p.RegisteredBy)
     .WithMany()
     .HasForeignKey(p => p.RegisteredById)
     .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentUtilisatuer>()
                .HasOne(p => p.UsedBy)
                .WithMany()
                .HasForeignKey(p => p.UsedById)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<PaymentUtilisatuer>()
                .HasOne(p => p.entity)
                .WithMany()
                .HasForeignKey(p => p.entityid)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Payment>()
                .HasOne(p => p.entity)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentUtilisatuer>()
               .HasOne(p => p.entity)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<PaymentFournisseur>()
               .HasOne(p => p.entity)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentParClient>()
              .HasOne(p => p.entity)
              .WithMany()
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Utilisatuer>()
    .HasOne(pd => pd.entity)
    .WithMany(p => p.utilisatuers)
    .HasForeignKey(pd => pd.EntityId)
    .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<PaymentDocument>()
     .HasOne(pd => pd.Payment)
     .WithMany(p => p.Documents)
     .HasForeignKey(pd => pd.PaymentId)
     .OnDelete(DeleteBehavior.Cascade);

            // PaymentFournisseur
            //modelBuilder.Entity<PaymentFournisseur>()
            //    .HasOne(pf => pf.factureFournisseur)
            //    .WithMany()
            //    .OnDelete(DeleteBehavior.Cascade);


         




        }

    }
}
