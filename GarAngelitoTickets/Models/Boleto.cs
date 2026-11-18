using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GarAngelitoTickets.Models
{
    [Table("Boletos")]
    public class Boleto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConciertoId { get; set; }

        [ForeignKey("ConciertoId")]
        [ValidateNever]
        public Concierto? Concierto { get; set; }

        [Required(ErrorMessage = "Ingresa el nombre del titular.")]
        [StringLength(120)]
        public string NombreComprador { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa un correo electrónico válido.")]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa un número de teléfono.")]
        [StringLength(25)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Zona { get; set; } = "General";

        [Range(1, 10, ErrorMessage = "Selecciona entre 1 y 10 boletos.")]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagado { get; set; }

        [StringLength(6)]
        [ValidateNever]
        public string CodigoNumerico { get; set; } = string.Empty;

        public DateTime FechaCompra { get; set; } = DateTime.Now;
    }
}