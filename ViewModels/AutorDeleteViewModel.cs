// ============================================================================
//  ViewModel: Confirmación de Eliminación de Autor
// ----------------------------------------------------------------------------
//  Alimenta la vista Delete de Autores. Permite advertir cuántos libros se
//  eliminarán en cascada antes de confirmar el borrado del autor.
// ============================================================================
namespace BibliotecaApp.ViewModels
{
    public class AutorDeleteViewModel
    {
        /// <summary>Identificador del autor a eliminar.</summary>
        public int Id { get; set; }

        /// <summary>Nombre del autor.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nacionalidad del autor.</summary>
        public string Nacionalidad { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento del autor.</summary>
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Total de libros asociados (se eliminarán en cascada).</summary>
        public int TotalLibros { get; set; }
    }
}