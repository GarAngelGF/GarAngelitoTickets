using GarAngelitoTickets.Data;
using GarAngelitoTickets.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null)
    ));

builder.Services.AddHttpClient();

var app = builder.Build();

// Sincronización automática con la base de datos al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        // Solo inserta registros base si tu tabla Conciertos está completamente vacía
        if (!context.Conciertos.Any())
        {
            context.Conciertos.AddRange(
                new Concierto
                {
                    Artista = "NSQK",
                    Tour = "ALTER EGO TOUR",
                    Recinto = "Palacio de los Deportes, CDMX",
                    Fecha = new DateTime(2026, 11, 28, 21, 0, 0),
                    PrecioGeneral = 950.00m,
                    PrecioVIP = 1850.00m,
                    CupoDisponible = 450
                },
                new Concierto
                {
                    Artista = "Álvaro Díaz",
                    Tour = "SAYONARA TOUR",
                    Recinto = "Auditorio Nacional, CDMX",
                    Fecha = new DateTime(2026, 12, 12, 20, 30, 0),
                    PrecioGeneral = 1100.00m,
                    PrecioVIP = 2200.00m,
                    CupoDisponible = 320
                }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al conectar o inicializar la base de datos SQL Server.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Boletos}/{action=Index}/{id?}");

app.Run();