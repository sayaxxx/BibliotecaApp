// ============================================================================
//  PrestamosController : controlador del módulo "Préstamos"
// ----------------------------------------------------------------------------
//  Gestiona el ciclo de vida de los préstamos de la biblioteca:
//      - Index         : listado completo con filtro por estado
//      - Prestar       : registrar un nuevo préstamo (fecha automática = hoy)
//      - Devolver      : registrar la devolución (fecha automática = hoy)
//      - Create/Edit/Delete : gestión CRUD completa
//      - MisPrestamos  : consulta EXCLUSIVA de los préstamos del usuario
//                        autenticado (acción pensada para el rol Lector).
//
//  Seguridad (ASP.NET Core Identity):
//      - Index/Details/Create/Edit/Delete/Prestar/Devolver : Admin y Bibliotecario.
//      - MisPrestamos : cualquier usuario autenticado (filtrado por LectorId).
// ============================================================================
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliotecaApp.Data;
using BibliotecaApp.Helpers;
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
        /// Solo Admin y Bibliotecario.
        /// </summary>
        /// <param name="estado">Valor seleccionado en el combobox de filtro.</param>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Index(string? estado)
        {
            // 1. Ejecutamos la consulta con el filtro elegido (si lo hay).
            var prestamos = await ObtenerPrestamosFiltradosAsync(estado);

            // 2. Enviamos la opción seleccionada a la vista para que el combobox no se reinicie
            ViewData["Estado"] = estado ?? "";

            // 3. Petición htmx => solo el fragmento de la tabla (filtro instantáneo).
            if (Request.IsHtmx())
            {
                return PartialView("_TablaPrestamos", prestamos);
            }

            // 4. Devolvemos la vista completa.
            return View(prestamos);
        }

        /// <summary>
        /// Detalle de un préstamo (incluye el libro asociado).
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// GET: formula semántico y directo para REGISTRAR un préstamo.
        /// Solo Admin y Bibliotecario. La fecha de préstamo se fija al día
        /// actual de forma automática (no se puede marcar una fecha pasada).
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public IActionResult Prestar()
        {
            // Desplegables del formulario: libros y usuarios (lectores) con cuenta.
            ViewData["LibroId"] = new SelectList(_context.Libros.OrderBy(l => l.Titulo), "Id", "Titulo");
            ViewData["Lectores"] = new SelectList(
                _context.Users.OrderBy(u => u.Email), "Id", "Email");
            return View();
        }

        /// <summary>
        /// POST: registra el préstamo. FechaPrestamo = hoy, FechaDevolucion = null.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Prestar([Bind("Id,NombreUsuario,FechaPrestamo,FechaDevolucion,LibroId,LectorId")] Prestamo prestamo)
        {
            // La fecha de préstamo la pone el sistema (hoy): evita fechas pasadas.
            prestamo.FechaPrestamo = DateTime.Today;
            prestamo.FechaDevolucion = null;
            ModelState.Remove("FechaPrestamo");

            if (ModelState.IsValid)
            {
                _context.Add(prestamo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["LibroId"] = new SelectList(_context.Libros.OrderBy(l => l.Titulo), "Id", "Titulo", prestamo.LibroId);
            ViewData["Lectores"] = new SelectList(_context.Users.OrderBy(u => u.Email), "Id", "Email", prestamo.LectorId);
            return View(prestamo);
        }

        /// <summary>
        /// GET: página de confirmación para REGISTRAR una devolución.
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Devolver(int? id)
        {
            if (id == null) return NotFound();

            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prestamo == null) return NotFound();

            return View(prestamo);
        }

        /// <summary>
        /// POST: registra la devolución poniendo FechaDevolucion = hoy.
        /// </summary>
        [HttpPost, ActionName("Devolver")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> DevolverConfirmed(int id)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null) return NotFound();

            // La fecha de devolución la pone el sistema (hoy).
            prestamo.FechaDevolucion = DateTime.Today;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: registra la devolución poniendo FechaDevolucion = hoy
        /// sin recargar la página (HTMX): devuelve la tabla filtrada y emite
        /// HX-Trigger para que el cliente muestre una notificación.
        /// </summary>
        [HttpPost, ActionName("DevolverAjax")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> DevolverAjax(int id, string? estado)
        {
            var prestamo = await _context.Prestamos.FindAsync(id);
            if (prestamo == null) return NotFound();

            // La fecha de devolución la pone el sistema (hoy).
            prestamo.FechaDevolucion = DateTime.Today;
            await _context.SaveChangesAsync();

            // Notificación al cliente (la escucha el puente de site.js).
            var trigger = new Dictionary<string, object>
            {
                ["toast"] = new { message = $"Devolución de «{prestamo.NombreUsuario}» registrada", type = "success" }
            };
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(trigger);

            // Se re-evalúa el mismo filtro que tenía aplicado el usuario.
            var prestamos = await ObtenerPrestamosFiltradosAsync(estado);
            return PartialView("_TablaPrestamos", prestamos);
        }

        /// <summary>
        /// "Mis Préstamos": consulta EXCLUSIVA de los préstamos del Lector
        /// autenticado, filtrados por su UserId (ClaimTypes.NameIdentifier).
        /// </summary>
        [Authorize]
        public async Task<IActionResult> MisPrestamos()
        {
            // Id del usuario actual según la cookie de Identity.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Solo se devuelven los préstamos cuyo LectorId coincide con el usuario.
            var prestamos = await _context.Prestamos
                .Where(p => p.LectorId == userId)
                .Include(p => p.Libro)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();

            return View(prestamos);
        }

        /// <summary>
        /// GET: muestra el formulario vacío para registrar un préstamo nuevo
        /// (CRUD clásico). Carga los libros disponibles en el desplegable.
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
        public IActionResult Create()
        {
            ViewData["LibroId"] = new SelectList(_context.Libros, "Id", "Titulo");
            return View();
        }

        /// <summary>
        /// POST: recibe los datos del formulario y guarda el nuevo préstamo.
        /// Si la validación falla, vuelve a mostrar el formulario con los errores.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// Solo Admin y Bibliotecario.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Bibliotecario")]
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
        /// </summary>
        private bool PrestamoExists(int id)
        {
            return _context.Prestamos.Any(e => e.Id == id);
        }

        /// <summary>
        /// Consulta de préstamos filtrada por estado (pendiente / devuelto / todos),
        /// incluyendo el libro asociado a cada préstamo. La comparte Index y
        /// DevolverAjax para que ambas mantengan la misma consulta.
        /// </summary>
        private async Task<List<Prestamo>> ObtenerPrestamosFiltradosAsync(string? estado)
        {
            // 1. Iniciamos la consulta LINQ incluyendo el libro asociado a cada préstamo
            IQueryable<Prestamo> prestamos = _context.Prestamos
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

            return await prestamos.ToListAsync();
        }
    }
}