// ============================================================================
//  Program.cs : punto de entrada de la aplicación (host ASP.NET Core)
// ----------------------------------------------------------------------------
//  Se encarga de:
//      1. Crear el WebApplicationBuilder.
//      2. Registrar servicios en el contenedor de DI
//         (MVC, EF Core, servicios de negocio y ASP.NET Core Identity).
//      3. Aplicar migraciones y sembrar datos al arrancar.
//      4. Configurar el pipeline de middleware (enrutado, autenticación,
//         autorización, archivos estáticos).
//   La cadena de conexión se toma de appsettings.json -> "DefaultConnection".
// ============================================================================
using BibliotecaApp.Data;
using BibliotecaApp.Models;
using BibliotecaApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// 1. Builder del host que arranca la aplicación web.
var builder = WebApplication.CreateBuilder(args);

// 2a. Registro de MVC (controladores + vistas Razor).
builder.Services.AddControllersWithViews();

// 2a2. Antiforgery: aceptar la cabecera X-RequestVerificationToken que envía HTMX
//      (por defecto ASP.NET Core solo reconoce "RequestVerificationToken").
builder.Services.AddAntiforgery(options => options.HeaderName = "X-RequestVerificationToken");

// 2b. Registro de Entity Framework Core con SQL Server.
//     La base de datos se crea en App_Data mediante AttachDbFilename
//     (ver appsettings.json) y el esquema se aplica con las migraciones.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2c. Registro del servicio de autores (interfaz + implementación) con
//     ciclo de vida Scoped: una instancia por petición HTTP.
builder.Services.AddScoped<IAutorService, AutorService>();

// 2d. ASP.NET Core Identity: usuarios (ApplicationUser) y roles (IdentityRole).
//     El almacenamiento usa el mismo ApplicationDbContext (tablas AspNet*).
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Opciones de contraseña (política de seguridad razonable para la demo).
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 2e. Configuración de la cookie de autenticación: rutas de login y acceso
//     denegado, y duración de la sesión.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// 3. Construcción de la aplicación.
var app = builder.Build();

// 4. Aplicar migraciones y cargar roles/usuarios + datos de ejemplo.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbSeeder.SeedAsync(context, userManager, roleManager);
        Console.WriteLine("Seed (roles, usuarios y datos) aplicado correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al aplicar el seed: " + ex.Message);
    }
}

// 5. Pipeline de middleware ----------------------------------------------
//    En producción se muestra una página de error global; en desarrollo se
//    usa la DeveloperExceptionPage (añadida automáticamente por el host).
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Enrutado de los controladores (coincide por convención de nombres).
app.UseRouting();

// Autenticación: identifica al usuario de cada petición (cookie de Identity).
app.UseAuthentication();

// Autorización: aplica los [Authorize] / [Authorize(Roles=...)] de los controladores.
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