using System.Security.Cryptography;
using GarAngelitoTickets.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("generar")]
        public async Task<IActionResult> GenerarCodigoUnico()
        {
            string codigo;
            do
            {
                int numero = RandomNumberGenerator.GetInt32(100000, 1000000);
                codigo = numero.ToString();
            }
            while (await _context.Boletos.AnyAsync(b => b.CodigoNumerico == codigo));

            return Ok(new { codigo });
        }
    }
}