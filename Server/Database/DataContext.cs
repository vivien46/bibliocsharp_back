using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Livre> Livres { get; set; }
        public DbSet<Emprunt> Emprunts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.User)
                .WithMany(u => u.Emprunts)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Livre)
                .WithMany(l => l.Emprunts)
                .HasForeignKey(e => e.LivreId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<int>();

        }
    }
}