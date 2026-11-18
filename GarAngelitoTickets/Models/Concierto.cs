using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarAngelitoTickets.Models
{
    [Table("Conciertos")]
    public class Concierto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Artista { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Tour { get; set; }

        [Required]
        [StringLength(150)]
        public string Recinto { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioGeneral { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVIP { get; set; }

        public int CupoDisponible { get; set; }
    }
}