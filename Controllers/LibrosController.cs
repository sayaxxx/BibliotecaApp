// ============================================================================
//  LibrosController : controlador del módulo "Libros"
// ----------------------------------------------------------------------------
//  Gestiona el catálogo de libros:
//      - Index   : listado con filtro por disponibilidad (disponible / prestado)
//      - Details : detalle de un libro con su autor y préstamos
//      - Create  : alta de un libro nuevo
//      - Edit    : modificación de un libro
//      - Delete  : eliminación con confirmación
//
//  Seguridad: Index y Details para cualquier usuario autenticado;
//  Create/Edit/Delete solo para Admin y Bibliotecario.
// ============================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliotecaApp.Data;
using BibliotecaApp.Helpers;
using BibliotecaApp.Models;

namespace BibliotecaApp.Controllers
{
    [Authorize]
    public class LibrosController : Controller
    {
        // Contexto de Entity Framework inyectado por el contenedor de DI.
        private readonly ApplicationDbContext _context;

        public LibrosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Recibe la opción elegida desde el combo box a través del parámetro 'estado'
        public async Task<IActionResult> Index(string? estado)
        {
            // 1. Iniciamos la consulta LINQ incluyendo los préstamos para evaluar su disponibilidad
            var libros = _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Prestamos)
                .AsQueryable();

            // 2. Evaluamos el valor seleccionado en el desplegable
            if (estado == "disponible")
            {
                // Trae libros que NO tienen préstamos activos (sin devolver)
                libros = libros.Where(l => !l.Prestamos.Any(p => p.FechaDevolucion == null));
            }
            else if (estado == "prestado")
            {
                // Trae libros que TIENEN préstamos activos
                libros = libros.Where(l => l.Prestamos.Any(p => p.FechaDevolucion == null));
            }

            // 3. Enviamos la opción seleccionada a la vista para que el desplegable no se reinicie
            ViewData["Estado"] = estado ?? "";

            // 4. Ejecutamos la consulta en SQL de forma asíncrona y devolvemos la vista
            var resultado = await libros.ToListAsync();

            // Petición htmx => solo el fragmento de la tabla (filtro instantáneo).
            if (Request.IsHtmx())
            {
                return PartialView("_TablaLibros", resultado);
            }

            return View(resultado);
        }

        /// <summary>
        /// Muestra el detalle de un libro junto con su autor y sus préstamos.
        /// </summary>
        /// <param name="id">Identificador del libro.</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Prestamos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (libro == null) return NotFound();

            return View(libro);
        }

        /// <summary>
        /// GET: muestra el formulario vacío de alta de libro con los autores
        /// disponibles en el desplegable "Autor". Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public IActionResult Create()
        {
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre");
            return View();
        }

        /// <summary>
        /// POST: recibe los datos del formulario y guarda el libro nuevo.
        /// </summary>
        /// <param name="libro">Entidad Libro recibida mediante model binding.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Create([Bind("Id,Titulo,ISBN,AnioPublicacion,AutorId")] Libro libro)
        {
            if (ModelState.IsValid)
            {
                _context.Add(libro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Si hay errores se recarga el desplegable de autores para no perder la selección
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            return View(libro);
        }

/// <summary>
        /// GET: precarga el formulario de edición con los datos del libro.
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros.FindAsync(id);
            if (libro == null) return NotFound();

            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            return View(libro);
        }

        /// <summary>
        /// POST: aplica los cambios del formulario al libro existente.
        /// </summary>
        /// <param name="id">Identificador que debe coincidir con libro.Id.</param>
        /// <param name="libro">Datos editados recibidos mediante model binding.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,ISBN,AnioPublicacion,AutorId")] Libro libro)
        {
            if (id != libro.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(libro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Si el registro ya no existe devolvemos 404; si no, relanzamos
                    if (!LibroExists(libro.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AutorId"] = new SelectList(_context.Autores, "Id", "Nombre", libro.AutorId);
            return View(libro);
        }

/// <summary>
        /// GET: muestra la confirmación de borrado. Incluye el autor y los
        /// préstamos para advertir sobre la eliminación en cascada.
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var libro = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Prestamos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (libro == null) return NotFound();

            return View(libro);
        }

/// <summary>
        /// POST: elimina definitivamente el libro. Si tiene préstamos asociados,
        /// el borrado en cascada configurado en el DbContext los eliminará también.
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro != null)
            {
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Comprueba si existe un libro con el id indicado.
        /// </summary>
        private bool LibroExists(int id)
        {
            return _context.Libros.Any(e => e.Id == id);
        }
    }
}