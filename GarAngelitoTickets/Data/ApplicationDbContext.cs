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
    }
}