using GarAngelitoTickets.Models;
using Microsoft.EntityFrameworkCore;

namespace GarAngelitoTickets.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Concierto> Conciertos { get; set; }
        public DbSet<Boleto> Boletos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Concierto>().ToTable("Conciertos");
            modelBuilder.Entity<Boleto>().ToTable("Boletos");
        }
    }
}