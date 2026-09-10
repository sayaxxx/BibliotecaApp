// ============================================================================
//  AutoresController : controlador del módulo "Autores"
// ----------------------------------------------------------------------------
//  Gestiona el catálogo de autores. Está REFACTORIZADO para trabajar con la
//  capa de servicios (IAutorService) y ViewModels en lugar de usar DbContext
//  directamente, siguiendo los requisitos de las guías HMTL 1..4.
//
//  Inyección de dependencias: el contenedor registra IAutorService -> AutorService
//  en Program.cs, y el framework lo inyecta en este constructor.
// ============================================================================
using BibliotecaApp.Services;
using BibliotecaApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApp.Controllers
{
    public class AutoresController : Controller
    {
        // Servicio de lógica de negocio del módulo de autores (inyectado por DI).
        private readonly IAutorService _autorService;

        public AutoresController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        /// <summary>
        /// GET: Autores/Index — Lista los autores aplicando un filtro opcional
        /// por nombre ('buscar') que viene del cuadro de búsqueda.
        /// </summary>
        /// <param name="buscar">Texto que se busca dentro del nombre del autor.</param>
        public async Task<IActionResult> Index(string? buscar)
        {
            // Delega la consulta en el servicio y de paso mapea a AutorIndexViewModel.
            var autores = await _autorService.ObtenerTodosAsync(buscar);

            // Se reenvía el texto buscado a la vista para que el campo no se pierda.
            ViewBag.Buscar = buscar;
            return View(autores);
        }

        /// <summary>
        /// GET: Autores/Details/{id} — Muestra el detalle del autor y sus libros.
        /// </summary>
        /// <param name="id">Identificador del autor.</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // El servicio devuelve null si el autor no existe.
            var detalle = await _autorService
                .ObtenerDetalleAsync(id.Value);

            if (detalle == null)
            {
                return NotFound();
            }

            return View(detalle);
        }

        /// <summary>
        /// GET: Autores/Create — Formulario de alta. Se entrega un ViewModel
        /// vacío para que la vista valide sobre él (no sobre el modelo).
        /// </summary>
        public IActionResult Create()
        {
            var viewModel = new CrearEditarAutorViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// POST: Autores/Create — Recibe el formulario y crea el autor en BD.
        /// </summary>
        /// <param name="viewModel">Datos validados del formulario.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CrearEditarAutorViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            await _autorService.CrearAutorAsync(viewModel);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Autores/Edit/{id} — Precarga el formulario con los datos actuales.
        /// </summary>
        /// <param name="id">Identificador del autor a editar.</param>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _autorService
                .ObtenerViewModelParaEditarAsync(id.Value);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        /// <summary>
        /// POST: Autores/Edit/{id} — Aplica los cambios del formulario.
        /// </summary>
        /// <param name="id">Identificador del autor (debe coincidir con el registro).</param>
        /// <param name="viewModel">Datos validados del formulario.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            CrearEditarAutorViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // El servicio devuelve false si el autor ya no existe.
            var exito = await _autorService
                .ActualizarAutorAsync(id, viewModel);

            if (!exito)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: Autores/Delete/{id} — Página de confirmación del borrado.
        /// </summary>
        /// <param name="id">Identificador del autor a eliminar.</param>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _autorService
                .ObtenerParaEliminarAsync(id.Value);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        /// <summary>
        /// POST: Autores/Delete — Confirmación del borrado (ActionName para
        /// que el formulario POST llame a la acción "Delete").
        /// </summary>
        /// <param name="id">Identificador del autor a eliminar.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var exito = await _autorService
                .EliminarAutorAsync(id);

            if (!exito)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}