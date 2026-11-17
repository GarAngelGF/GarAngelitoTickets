using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarAngelitoTickets.Models
{
    public class Boleto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConciertoId { get; set; }

        [ForeignKey("ConciertoId")]
        public Concierto? Concierto { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150)]
        public string NombreComprador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Ingresa un número de 10 dígitos.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una zona.")]
        public string Zona { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indica la cantidad de boletos.")]
        [Range(1, 6, ErrorMessage = "Puedes adquirir entre 1 y 6 boletos.")]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPagado { get; set; }

        [StringLength(6)]
        public string CodigoNumerico { get; set; } = string.Empty;

        public DateTime FechaCompra { get; set; } = DateTime.Now;
    }
}