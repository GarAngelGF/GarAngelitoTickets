using GarAngelitoTickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GarAngelitoTickets.Controllers
{
    [Route("api/microservicio")]
    [ApiController]
    public class MicroservicioCodigoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MicroservicioCodigoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("generar-codigo")]
        public async Task<IActionResult> GenerarCodigoUnico()
        {
            string codigo;
            bool existe;

            do
            {
                int numero = RandomNumberGenerator.GetInt32(100000, 1000000);
                codigo = numero.ToString("D6");
                existe = await _context.Boletos.AnyAsync(b => b.CodigoNumerico == codigo);
            } while (existe);

            return Ok(new { Codigo = codigo, Timestamp = DateTime.UtcNow });
        }
    }
}