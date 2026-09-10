// ============================================================================
//  ViewModel: Listado de Autores (página Index)
// ----------------------------------------------------------------------------
//  Proyecta solo los datos que necesita la tabla del listado, evitando traer
//  entidades completas y sus relaciones innecesarias desde la base de datos.
// ============================================================================
namespace BibliotecaApp.ViewModels
{
    public class AutorIndexViewModel
    {
        /// <summary>Identificador del autor (para los enlaces Editar/Detalles).</summary>
        public int Id { get; set; }

        /// <summary>Nombre del autor.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nacionalidad del autor.</summary>
        public string Nacionalidad { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento (nullable para un formato fácil en la vista).</summary>
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Número de libros registrados del autor (dato calculado).</summary>
        public int TotalLibros { get; set; }
    }
}