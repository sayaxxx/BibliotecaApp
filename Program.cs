// ============================================================================
//  Program.cs : punto de entrada de la aplicación (host ASP.NET Core)
// ----------------------------------------------------------------------------
//  Se encarga de:
//      1. Crear el WebApplicationBuilder.
//      2. Registrar servicios en el contenedor de DI (MVC, EF Core, servicios).
//      3. Aplicar migraciones y sembrar datos al arrancar.
//      4. Configurar el pipeline de middleware (enrutado, autorización,
//         archivos estáticos).
//   La cadena de conexión se toma de appsettings.json -> "DefaultConnection".
// ============================================================================
using BibliotecaApp.Data;
using BibliotecaApp.Services;
using Microsoft.EntityFrameworkCore;

// 1. Builder del host que arranca la aplicación web.
var builder = WebApplication.CreateBuilder(args);

// 2a. Registro de MVC (controladores + vistas Razor).
builder.Services.AddControllersWithViews();

// 2b. Registro de Entity Framework Core con SQL Server.
//     La base de datos se crea en App_Data mediante AttachDbFilename
//     (ver appsettings.json) y el esquema se aplica con las migraciones.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2c. Registro del servicio de autores (interfaz + implementación) con
//     ciclo de vida Scoped: una instancia por petición HTTP.
builder.Services.AddScoped<IAutorService, AutorService>();

// 3. Construcción de la aplicación.
var app = builder.Build();

// 4. Aplicar migraciones y cargar datos de ejemplo al primer arranque.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(context);
        Console.WriteLine("Datos seed insertados correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al insertar datos seed: " + ex.Message);
    }
}

// 5. Pipeline de middleware -----------------------------------------------
//    En producción se muestra una página de error global; en desarrollo se
//    usa la DeveloperExceptionPage (añadida automáticamente por el host).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Enrutado de los controladores (coincide por convención de nombres).
app.UseRouting();

// Autorización (sin autenticación, pero requerida por ASP.NET MVC).
app.UseAuthorization();

// Archivos estáticos optimizados (css/js con fingerprin de versión).
app.MapStaticAssets();

// Ruta por defecto: {controller=Home}/{action=Index}/{id?}.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 6. Inicia el servidor Kestrel y bloquea el hilo principal.
app.Run();