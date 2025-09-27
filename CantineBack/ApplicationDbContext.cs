using CantineBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CantineBack
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<Commande> Commandes { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<LigneCommande> LigneCommandes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Entreprise> Entreprises { get; set; }
        public virtual DbSet<Categorie> Categories { get; set; }
        public DbSet<Emplacement>? Emplacement { get; set; }
        public DbSet<PaymentMethod>? PaymentMethods { get; set; }
        public DbSet<Log>? Log { get; set; }
        public DbSet<RefreshToken>? RefreshTokens { get; set; }
    }
}
