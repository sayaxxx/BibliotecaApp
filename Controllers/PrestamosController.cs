// ============================================================================
//  PrestamosController : controlador del módulo "Préstamos"
// ----------------------------------------------------------------------------
//  Gestiona el ciclo de vida de los préstamos de la biblioteca:
//      - Index   : listado con filtro por estado (No devueltos / Devueltos)
//      - Details : detalle de un préstamo concreto
//      - Create  : alta de un nuevo préstamo
//      - Edit    : modificación (p. ej. registrar la fecha de devolución)
//      - Delete  : eliminación con confirmación
//  Accede a la base de datos a través de ApplicationDbContext (Entity Framework).
// ============================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliotecaApp.Data;
using BibliotecaApp.Models;

namespace BibliotecaApp.Controllers
{
    public class PrestamosController : Controller
    {
        // Contexto de Entity Framework inyectado por el contenedor de DI.
        private readonly ApplicationDbContext _context;

        public PrestamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Muestra el listado de préstamos. Admite el filtro por estado:
        /// "pendiente" (préstamos no devueltos) y "devuelto" (ya devueltos).
        /// Si el parámetro llega vacío o nulo, se muestran todos.
        /// </summary>
        /// <param name="estado">Valor seleccionado en el combobox de filtro.</param>
        /// <returns>Vista Index con la colección de préstamos filtrada.</returns>
        public async Task<IActionResult> Index(string? estado)
        {
            // 1. Iniciamos la consulta LINQ incluyendo el libro asociado a cada préstamo
            var prestamos = _context.Prestamos
                .Include(p => p.Libro)
                .AsQueryable();

            // 2. Evaluamos el valor elegido en el desplegable
            if (estado == "pendiente")
            {
                // Préstamos activos: la fecha de devolución aún no se ha registrado (NULL)
                prestamos = prestamos.Where(p => p.FechaDevolucion == null);
            }
            else if (estado == "devuelto")
            {
                // Préstamos ya devueltos: la fecha de devolución está registrada
                prestamos = prestamos.Where(p => p.FechaDevolucion != null);
            }

            // 3. Enviamos la opción seleccionada a la vista para que el combobox no se reinicie
            ViewData["Estado"] = estado ?? "";

            // 4. Ejecutamos la consulta de forma asíncrona y devolvemos la vista
            return View(await prestamos.ToListAsync());
        }

        /// <summary>
        /// Muestra los detalles de un préstamo (incluye el libro asociado).
        /// </summary>
        /// <param name="id">Identificador del préstamo.</param>
        /// <returns>Vista Details, o NotFound si el id es inválido o no existe.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prestamo == null) return NotFound();

            return View(prestamo);
        }

        /// <summary>
        /// GET: muestra el formulario vacío para registrar un préstamo nuevo.
        /// Carga los libros disponibles en el desplegable "Libro".
        /// </summary>
        public IActionResult Create()
        {
            ViewData["LibroId"] = new SelectList(_context.Libros, "Id", "Titulo");
            return View();
        }

        /// <summary>
        /// POST: recibe los datos del formulario y guarda el nuevo préstamo.
        /// Si la validación falla, vuelve a mostrar el formulario con los errores.
        /// </summary>
        /// <param name="prestamo">Entidad Prestamo recibida mediante model binding.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreUsuario,FechaPrestamo,FechaDevolucion,LibroId")] Prestamo prestamo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prestamo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Si hay errores se recarga el desplegable de libros para que no pierda la selección
            ViewData["LibroId"] = new SelectList(_context.Libros, "Id", "Titulo", prestamo.LibroId);
            return View(prestamo);
        }

        /// <summary>
        /// GET: precarga el formulario de edición con los datos del préstamo.
        /// </summary>
        /// <param name="id">Identificador del préstamo a editar.</param>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null) return NotFound();

            ViewData["LibroId"] = new SelectList(_context.Libros, "Id", "Titulo", prestamo.LibroId);
            return View(prestamo);
        }

        /// <summary>
        /// POST: aplica los cambios del formulario al préstamo existente.
        /// </summary>
        /// <param name="id">Identificador que debe coincidir con prestamo.Id.</param>
        /// <param name="prestamo">Datos editados recibidos mediante model binding.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreUsuario,FechaPrestamo,FechaDevolucion,LibroId")] Prestamo prestamo)
        {
            if (id != prestamo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prestamo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Si el registro ya no existe devolvemos 404; si no, relanzamos
                    if (!PrestamoExists(prestamo.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LibroId"] = new SelectList(_context.Libros, "Id", "Titulo", prestamo.LibroId);
            return View(prestamo);
        }

        /// <summary>
        /// GET: muestra la página de confirmación antes de eliminar el préstamo.
        /// </summary>
        /// <param name="id">Identificador del préstamo a eliminar.</param>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prestamo == null) return NotFound();

            return View(prestamo);
        }

        /// <summary>
        /// POST: elimina definitivamente el préstamo de la base de datos.
        /// Se recibe a través de un formulario con antiforgery (ActionName "Delete").
        /// </summary>
        /// <param name="id">Identificador del préstamo a eliminar.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo != null)
            {
                _context.Prestamos.Remove(prestamo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Comprueba si existe un préstamo con el id indicado.
        /// Se usa para distinguir una eliminación concurrente de otros errores.
        /// </summary>
        private bool PrestamoExists(int id)
        {
            return _context.Prestamos.Any(e => e.Id == id);
        }
    }
}