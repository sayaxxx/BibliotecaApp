// ============================================================================
//  DbSeeder : datos de ejemplo (seed)
// ----------------------------------------------------------------------------
//  Puebla la base de datos con datos iniciales la primera vez que se arranca
//  la aplicación (se invoca desde Program.cs tras aplicar las migraciones):
//      1. Roles de Identity: Admin, Bibliotecario y Lector.
//      2. Cuentas de prueba con contraseñas seguras y sus roles.
//      3. Autores, libros y préstamos de ejemplo.
//  Es idempotente: si ya existe algún autor, el catalog no se repite; los roles
//  y usuarios se crean solo cuando faltan.
// ============================================================================
using Microsoft.AspNetCore.Identity;
using BibliotecaApp.Models;

namespace BibliotecaApp.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Inicializa roles y usuarios (Identity) y datos de catálogo.
        /// </summary>
        /// <param name="context">Contexto de EF en el que se insertan los datos.</param>
        /// <param name="userManager">Manager de Identity para crear usuarios.</param>
        /// <param name="roleManager">Manager de Identity para crear roles.</param>
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // -------------------------------------------------------------------
            // 1. Roles del sistema (se crean solo si no existen).
            // -------------------------------------------------------------------
            var roles = new[] { "Admin", "Bibliotecario", "Lector" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // -------------------------------------------------------------------
            // 2. Cuentas de prueba con contraseñas seguras y rol asignado.
            // -------------------------------------------------------------------
            await CrearUsuarioSiNoExisteAsync(userManager, "admin@biblioteca.com", "Admin123!", "Administrador del Sistema", "Admin");
            await CrearUsuarioSiNoExisteAsync(userManager, "bibliotecario@biblioteca.com", "Bibliotecario123!", "Bibliotecario Central", "Bibliotecario");

            // Guardamos la referencia del lector para vincular sus préstamos.
            var lector = await CrearUsuarioSiNoExisteAsync(userManager, "lector@biblioteca.com", "Lector123!", "Lector Invitado", "Lector");

            // -------------------------------------------------------------------
            // 3. Catálogo: autores y libros (solo la primera vez).
            // -------------------------------------------------------------------
            if (!context.Autores.Any())
            {
                var autores = new List<Autor>
                {
                    new Autor { Nombre = "Gabriel García Márquez", Nacionalidad = "Colombiano", FechaNacimiento = new DateTime(1927, 3, 6) },
                    new Autor { Nombre = "Mario Vargas Llosa", Nacionalidad = "Peruano", FechaNacimiento = new DateTime(1936, 3, 28) },
                    new Autor { Nombre = "Jorge Luis Borges", Nacionalidad = "Argentino", FechaNacimiento = new DateTime(1899, 8, 24) }
                };

                await context.Autores.AddRangeAsync(autores);
                await context.SaveChangesAsync();

                var libros = new List<Libro>
                {
                    new Libro { Titulo = "Cien Años de Soledad", ISBN = "978-0307474728", AnioPublicacion = 1967, AutorId = 1 },
                    new Libro { Titulo = "El Amor en los Tiempos del Cólera", ISBN = "978-0307389732", AnioPublicacion = 1985, AutorId = 1 },
                    new Libro { Titulo = "La Ciudad y los Perros", ISBN = "978-0307475671", AnioPublicacion = 1963, AutorId = 2 },
                    new Libro { Titulo = "Ficciones", ISBN = "978-0394177601", AnioPublicacion = 1944, AutorId = 3 }
                };

                await context.Libros.AddRangeAsync(libros);
                await context.SaveChangesAsync();
            }

            // -------------------------------------------------------------------
            // 4. Préstamos de ejemplo (vinculados al perfil del lector).
            //    Nota: María ya devolvió; los otros dos siguen pendientes,
            //    lo que permite probar el filtro del listado y "Mis Préstamos".
            // -------------------------------------------------------------------
            if (!context.Prestamos.Any())
            {
                var prestamos = new List<Prestamo>
                {
                    new Prestamo { NombreUsuario = "Juan Pérez", FechaPrestamo = DateTime.Now.AddDays(-10), LibroId = 1, LectorId = lector?.Id },
                    new Prestamo { NombreUsuario = "María García", FechaPrestamo = DateTime.Now.AddDays(-5), FechaDevolucion = DateTime.Now, LibroId = 2, LectorId = lector?.Id },
                    new Prestamo { NombreUsuario = "Carlos López", FechaPrestamo = DateTime.Now.AddDays(-2), LibroId = 3, LectorId = lector?.Id }
                };

                await context.Prestamos.AddRangeAsync(prestamos);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Crea un usuario con contraseña y rol si no existe aún por email.
        /// Devuelve el usuario creado (o el existente) para poder usarlo después.
        /// </summary>
        private static async Task<ApplicationUser?> CrearUsuarioSiNoExisteAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string nombreCompleto,
            string rol)
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario != null)
            {
                return usuario;
            }

            var nuevo = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NombreCompleto = nombreCompleto
            };

            var resultado = await userManager.CreateAsync(nuevo, password);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(nuevo, rol);
                return nuevo;
            }

            return null;
        }
    }
}