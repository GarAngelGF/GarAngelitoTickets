using GarAngelitoTickets.Data;
using GarAngelitoTickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Cryptography;

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

        // 1. Pantalla principal de Cartelera
        public async Task<IActionResult> Index()
        {
            var conciertos = await _context.Conciertos.OrderBy(c => c.Fecha).ToListAsync();
            return View(conciertos);
        }

        // 2. Formulario de Compra (Estricto sin datos de prellenado)
        [HttpGet]
        public async Task<IActionResult> Comprar(int id)
        {
            var concierto = await _context.Conciertos.FindAsync(id);
            if (concierto == null) return RedirectToAction(nameof(Index));

            ViewBag.Concierto = concierto;
            // Se envía un modelo completamente vacío para prohibir prellenado
            ModelState.Clear();
            return View(new Boleto { ConciertoId = concierto.Id, NombreComprador = "", Correo = "", Telefono = "", Zona = "" });
        }

        // 3. Procesamiento de Compra y Generación de Código de 6 dígitos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(Boleto boleto)
        {
            var concierto = await _context.Conciertos.FindAsync(boleto.ConciertoId);
            if (concierto == null) return RedirectToAction(nameof(Index));

            ModelState.Remove("CodigoNumerico");
            ModelState.Remove("Concierto");

            if (!ModelState.IsValid)
            {
                ViewBag.Concierto = concierto;
                return View(boleto);
            }

            // Cálculo del monto total según la zona elegida
            decimal precioUnitario = boleto.Zona == "VIP" ? concierto.PrecioVIP : concierto.PrecioGeneral;
            boleto.TotalPagado = precioUnitario * boleto.Cantidad;
            boleto.FechaCompra = DateTime.Now;

            // Obtención del código único de 6 posiciones numéricas
            boleto.CodigoNumerico = await ObtenerCodigoDesdeMicroservicioAsync();

            // Descuento de cupo y guardado en SQL Server
            concierto.CupoDisponible = Math.Max(0, concierto.CupoDisponible - boleto.Cantidad);
            _context.Boletos.Add(boleto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Confirmacion), new { codigo = boleto.CodigoNumerico });
        }

        // 4. Pantalla de Confirmación con Código QR
        [HttpGet]
        public async Task<IActionResult> Confirmacion(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return RedirectToAction(nameof(Index));

            var boleto = await _context.Boletos
                .Include(b => b.Concierto)
                .FirstOrDefaultAsync(b => b.CodigoNumerico == codigo);

            if (boleto == null) return RedirectToAction(nameof(Index));

            // Al escanear el QR en cualquier celular, muestra directamente el código de 6 dígitos y los datos del boleto
            string contenidoQr = $"CODIGO: {boleto.CodigoNumerico}\n" +
                                 $"GarAngelitoTickets\n" +
                                 $"Titular: {boleto.NombreComprador}\n" +
                                 $"Evento: {boleto.Concierto?.Artista}\n" +
                                 $"Zona: {boleto.Zona} ({boleto.Cantidad} accesos)";

            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQr, QRCodeGenerator.ECCLevel.Q);
            using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            ViewBag.QrBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
            return View(boleto);
        }

        // 5. Consulta de Boletos Comprados y Filtro por Código de 6 dígitos
        [HttpGet]
        public async Task<IActionResult> Consultar(string? buscarCodigo)
        {
            var query = _context.Boletos
                .Include(b => b.Concierto)
                .OrderByDescending(b => b.FechaCompra)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscarCodigo))
            {
                buscarCodigo = buscarCodigo.Trim();
                query = query.Where(b => b.CodigoNumerico == buscarCodigo);
            }

            ViewBag.CodigoBuscado = buscarCodigo;
            var listaBoletos = await query.ToListAsync();
            return View(listaBoletos);
        }

        private async Task<string> ObtenerCodigoDesdeMicroservicioAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MicroservicioCodigos");
                var response = await client.GetFromJsonAsync<CodigoDto>("api/microservicio/generar-codigo");
                if (response != null && !string.IsNullOrEmpty(response.Codigo))
                {
                    return response.Codigo;
                }
            }
            catch
            {
                // Respaldo automático local si se ejecuta directamente desde Visual Studio sin contenedor auxiliar
            }

            string codigo;
            bool existe;
            do
            {
                codigo = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
                existe = await _context.Boletos.AnyAsync(b => b.CodigoNumerico == codigo);
            } while (existe);

            return codigo;
        }

        private class CodigoDto
        {
            public string Codigo { get; set; } = string.Empty;
        }
    }
}