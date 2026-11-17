using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarAngelitoTickets.Models
{
    public class Concierto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Artista { get; set; } = string.Empty;

        [Required]
        public string Tour { get; set; } = string.Empty;

        [Required]
        public string Recinto { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioGeneral { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVIP { get; set; }

        public int CupoDisponible { get; set; }
    }
}