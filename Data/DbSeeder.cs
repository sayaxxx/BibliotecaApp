// ============================================================================
//  DbSeeder : datos de ejemplo (seed)
// ----------------------------------------------------------------------------
//  Puebla la base de datos con datos iniciales la primera vez que se arranca
//  la aplicación (se invoca desde Program.cs tras aplicar las migraciones).
//  Si ya existe algún autor en la tabla, el seed se omite (idempotente).
// ============================================================================
using BibliotecaApp.Models;

namespace BibliotecaApp.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Inserta autores, libros y préstamos de ejemplo si la base
        /// de datos está vacía.
        /// </summary>
        /// <param name="context">Contexto de EF en el que se insertan los datos.</param>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Idempotencia: si ya hay autores, no volvemos a sembrar datos.
            if (context.Autores.Any())
            {
                return;
            }

            // --- Autores de ejemplo -----------------------------------------
            var autores = new List<Autor>
            {
                new Autor { Nombre = "Gabriel García Márquez", Nacionalidad = "Colombiano", FechaNacimiento = new DateTime(1927, 3, 6) },
                new Autor { Nombre = "Mario Vargas Llosa", Nacionalidad = "Peruano", FechaNacimiento = new DateTime(1936, 3, 28) },
                new Autor { Nombre = "Jorge Luis Borges", Nacionalidad = "Argentino", FechaNacimiento = new DateTime(1899, 8, 24) }
            };

            await context.Autores.AddRangeAsync(autores);
            await context.SaveChangesAsync();

            // --- Libros de ejemplo (AutorId 1..3 ya generados) --------------
            var libros = new List<Libro>
            {
                new Libro { Titulo = "Cien Años de Soledad", ISBN = "978-0307474728", AnioPublicacion = 1967, AutorId = 1 },
                new Libro { Titulo = "El Amor en los Tiempos del Cólera", ISBN = "978-0307389732", AnioPublicacion = 1985, AutorId = 1 },
                new Libro { Titulo = "La Ciudad y los Perros", ISBN = "978-0307475671", AnioPublicacion = 1963, AutorId = 2 },
                new Libro { Titulo = "Ficciones", ISBN = "978-0394177601", AnioPublicacion = 1944, AutorId = 3 }
            };

            await context.Libros.AddRangeAsync(libros);
            await context.SaveChangesAsync();

            // --- Préstamos de ejemplo ---------------------------------------
            // Nota: María ya devolvió (FechaDevolucion con valor); los otros dos
            // siguen pendientes, lo que permite probar el filtro del listado.
            var prestamos = new List<Prestamo>
            {
                new Prestamo { NombreUsuario = "Juan Pérez", FechaPrestamo = DateTime.Now.AddDays(-10), LibroId = 1 },
                new Prestamo { NombreUsuario = "María García", FechaPrestamo = DateTime.Now.AddDays(-5), FechaDevolucion = DateTime.Now, LibroId = 2 },
                new Prestamo { NombreUsuario = "Carlos López", FechaPrestamo = DateTime.Now.AddDays(-2), LibroId = 3 }
            };

            await context.Prestamos.AddRangeAsync(prestamos);
            await context.SaveChangesAsync();
        }
    }
}