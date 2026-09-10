// ============================================================================
//  Implementación del servicio de Autores (AutorService)
// ----------------------------------------------------------------------------
//  Encapsula el acceso a datos del módulo Autores usando Entity Framework.
//  Convierte (mapea) entidades de dominio (Autor) a ViewModels para que la
//  capa de presentación nunca maneje el modelo de datos directamente.
// ============================================================================
using BibliotecaApp.Data;
using BibliotecaApp.Models;
using BibliotecaApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApp.Services
{
    public class AutorService : IAutorService
    {
        // Contexto de EF inyectado por DI para acceder a la base de datos.
        private readonly ApplicationDbContext _context;

        public AutorService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista de autores para la página Index. Si 'buscar' tiene texto,
        /// filtra por coincidencia parcial (LIKE) en el nombre.
        /// </summary>
        /// <param name="buscar">Texto opcional de búsqueda por nombre.</param>
        public async Task<IEnumerable<AutorIndexViewModel>> ObtenerTodosAsync(string? buscar)
        {
            // Consulta inicial sobre la tabla de autores.
            var query = _context.Autores.AsQueryable();

            // Aplicamos el filtro por nombre si el usuario escribió algo.
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(a => a.Nombre.Contains(buscar));
            }

            // Mapeamos de Autor (modelo) a AutorIndexViewModel, proyectando
            // la cantidad de libros de cada autor como dato calculado.
            return await query.Select(a => new AutorIndexViewModel
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Nacionalidad = a.Nacionalidad,
                FechaNacimiento = a.FechaNacimiento,
                TotalLibros = a.Libros.Count
            }).ToListAsync();
        }

        /// <summary>
        /// Detalle de un autor para la vista Details. Incluye los títulos
        /// de sus libros. Devuelve null si el autor no existe.
        /// </summary>
        public async Task<AutorDetalleViewModel?> ObtenerDetalleAsync(int id)
        {
            // Se incluye la colección Libros para poder mostrarla.
            var autor = await _context.Autores
                .Include(a => a.Libros)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (autor == null)
            {
                return null;
            }

            // Mapeo modelo -> ViewModel de detalle.
            return new AutorDetalleViewModel
            {
                Id = autor.Id,
                Nombre = autor.Nombre,
                Nacionalidad = autor.Nacionalidad,
                FechaNacimiento = autor.FechaNacimiento,
                TotalLibros = autor.Libros.Count,
                TitulosLibros = autor.Libros.Select(l => l.Titulo).ToList()
            };
        }

        /// <summary>
        /// Obtiene los datos actuales de un autor para precargar el formulario
        /// de edición. Devuelve null si no existe.
        /// </summary>
        public async Task<CrearEditarAutorViewModel?> ObtenerViewModelParaEditarAsync(int id)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
            {
                return null;
            }

            // Mapear del modelo al ViewModel (solo campos editables).
            return new CrearEditarAutorViewModel
            {
                Nombre = autor.Nombre,
                Nacionalidad = autor.Nacionalidad,
                FechaNacimiento = autor.FechaNacimiento
            };
        }

        /// <summary>
        /// Actualiza los datos de un autor a partir del formulario.
        /// Devuelve false si el autor ya no existe.
        /// </summary>
        public async Task<bool> ActualizarAutorAsync(int id, CrearEditarAutorViewModel viewModel)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
            {
                return false;
            }

            // Actualizar solo las propiedades permitidas (evita sobrescritura accidental).
            autor.Nombre = viewModel.Nombre;
            autor.Nacionalidad = viewModel.Nacionalidad;
            autor.FechaNacimiento = viewModel.FechaNacimiento.GetValueOrDefault();

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Crea un autor nuevo a partir del ViewModel del formulario.
        /// </summary>
        public async Task<bool> CrearAutorAsync(CrearEditarAutorViewModel viewModel)
        {
            // Crear un nuevo objeto Autor a partir del ViewModel.
            var autor = new Autor
            {
                Nombre = viewModel.Nombre,
                Nacionalidad = viewModel.Nacionalidad,
                FechaNacimiento = viewModel.FechaNacimiento.GetValueOrDefault()
            };

            // Agregarlo a la base de datos.
            _context.Autores.Add(autor);

            // Guardar los cambios.
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Obtiene los datos del autor para la pantalla de confirmación de
        /// borrado (incluye cuántos libros se eliminarán en cascada).
        /// </summary>
        public async Task<AutorDeleteViewModel?> ObtenerParaEliminarAsync(int id)
        {
            var autor = await _context.Autores
                .Include(a => a.Libros)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (autor == null)
            {
                return null;
            }

            // Mapeo modelo -> ViewModel de confirmación de borrado.
            return new AutorDeleteViewModel
            {
                Id = autor.Id,
                Nombre = autor.Nombre,
                Nacionalidad = autor.Nacionalidad,
                FechaNacimiento = autor.FechaNacimiento,
                TotalLibros = autor.Libros.Count
            };
        }

        /// <summary>
        /// Elimina un autor de la base de datos (los libros asociados se
        /// eliminan en cascada por la configuración del DbContext).
        /// </summary>
        public async Task<bool> EliminarAutorAsync(int id)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
            {
                return false;
            }

            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}