using System.Security.Cryptography;
using System.Text.Json;
using GarAngelitoTickets.Data;
using GarAngelitoTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace GarAngelitoTickets.Controllers
{
    public class BoletosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public BoletosController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var conciertos = await _context.Conciertos
                .AsNoTracking()
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            return View(conciertos);
        }

        [HttpGet]
        public async Task<IActionResult> Comprar(int id)
        {
            var concierto = await _context.Conciertos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (concierto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Concierto = concierto;
            return View(new Boleto { ConciertoId = concierto.Id, Cantidad = 1, Zona = "General" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(Boleto boleto)
        {
            var concierto = await _context.Conciertos.FirstOrDefaultAsync(c => c.Id == boleto.ConciertoId);
            if (concierto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove(nameof(boleto.Concierto));
            ModelState.Remove(nameof(boleto.CodigoNumerico));

            if (boleto.Cantidad > concierto.CupoDisponible)
            {
                ModelState.AddModelError("Cantidad", $"Solo quedan {concierto.CupoDisponible} lugares disponibles.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Concierto = concierto;
                return View(boleto);
            }

            decimal precioUnitario = string.Equals(boleto.Zona, "VIP", StringComparison.OrdinalIgnoreCase)
                ? concierto.PrecioVIP
                : concierto.PrecioGeneral;

            boleto.TotalPagado = precioUnitario * boleto.Cantidad;
            boleto.FechaCompra = DateTime.Now;
            boleto.CodigoNumerico = await ObtenerCodigoUnicoAsync();

            concierto.CupoDisponible -= boleto.Cantidad;

            _context.Boletos.Add(boleto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmacion), new { id = boleto.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmacion(int id)
        {
            var boleto = await _context.Boletos
                .AsNoTracking()
                .Include(b => b.Concierto)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boleto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            string contenidoQr = $"GARANGELITO|{boleto.CodigoNumerico}|{boleto.Concierto?.Artista}|{boleto.Zona}|{boleto.Cantidad}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(contenidoQr, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            ViewBag.QrBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

            return View(boleto);
        }

        [HttpGet]
        public async Task<IActionResult> Consultar(string? busqueda)
        {
            ViewBag.Busqueda = busqueda;

            var query = _context.Boletos
                .AsNoTracking()
                .Include(b => b.Concierto)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                string filtro = busqueda.Trim();
                query = query.Where(b =>
                    b.CodigoNumerico.Contains(filtro) ||
                    b.Correo.Contains(filtro) ||
                    b.NombreComprador.Contains(filtro));
            }

            var resultados = await query
                .OrderByDescending(b => b.FechaCompra)
                .ToListAsync();

            return View(resultados);
        }

        private async Task<string> ObtenerCodigoUnicoAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var response = await client.GetAsync($"{baseUrl}/api/microservicio/generar");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("codigo", out var codProp))
                    {
                        var codigo = codProp.GetString();
                        if (!string.IsNullOrEmpty(codigo))
                        {
                            return codigo;
                        }
                    }
                }
            }
            catch
            {
                // Respaldo directo si el llamado HTTP local no responde
            }

            string nuevoCodigo;
            do
            {
                nuevoCodigo = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            }
            while (await _context.Boletos.AnyAsync(b => b.CodigoNumerico == nuevoCodigo));

            return nuevoCodigo;
        }
    }
}