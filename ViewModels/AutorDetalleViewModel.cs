// ============================================================================
//  ViewModel: Detalle de Autor
// ----------------------------------------------------------------------------
//  Alimenta la vista Details de Autores. Además de los datos básicos del autor,
//  incluye el total de libros y la lista de títulos para mostrar en pantalla.
// ============================================================================
namespace BibliotecaApp.ViewModels
{
    public class AutorDetalleViewModel
    {
        /// <summary>Identificador del autor.</summary>
        public int Id { get; set; }

        /// <summary>Nombre del autor.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nacionalidad del autor.</summary>
        public string Nacionalidad { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento del autor.</summary>
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Total de libros escritos por el autor.</summary>
        public int TotalLibros { get; set; }

        /// <summary>Títulos de los libros del autor (para mostrarlos en una lista).</summary>
        public List<string> TitulosLibros { get; set; } = new List<string>();
    }
}