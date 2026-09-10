// ============================================================================
//  Contrato del servicio de Autores (IAutorService)
// ----------------------------------------------------------------------------
//  Define la lógica de negocio del módulo Autores. Al inyectar esta interfaz
//  en los controladores, el código queda desacoplado de la implementación
//  concreta (AutorService) y es fácil de testear o sustituir.
//  Todos los métodos devuelven ViewModels, nunca entidades de dominio.
// ============================================================================
using BibliotecaApp.ViewModels;

namespace BibliotecaApp.Services
{
    public interface IAutorService
    {
        /// <summary>
        /// Obtiene todos los autores con sus totales de libros.
        /// Permite filtrar por nombre si se pasa un texto de búsqueda.
        /// </summary>
        Task<IEnumerable<AutorIndexViewModel>> ObtenerTodosAsync(string? buscar);

        /// <summary>Obtiene el detalle de un autor (con sus títulos de libros).</summary>
        Task<AutorDetalleViewModel?> ObtenerDetalleAsync(int id);

        /// <summary>Obtiene un autor listo para el formulario de edición.</summary>
        Task<CrearEditarAutorViewModel?> ObtenerViewModelParaEditarAsync(int id);

        /// <summary>Actualiza un autor. Devuelve false si el id no existe.</summary>
        Task<bool> ActualizarAutorAsync(int id, CrearEditarAutorViewModel viewModel);

        /// <summary>Crea un autor nuevo a partir del ViewModel del formulario.</summary>
        Task<bool> CrearAutorAsync(CrearEditarAutorViewModel viewModel);

        /// <summary>Obtiene un autor para la pantalla de confirmación de borrado.</summary>
        Task<AutorDeleteViewModel?> ObtenerParaEliminarAsync(int id);

        /// <summary>Elimina un autor. Devuelve false si el id no existe.</summary>
        Task<bool> EliminarAutorAsync(int id);
    }
}