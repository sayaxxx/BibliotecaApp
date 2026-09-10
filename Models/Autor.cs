// ============================================================================
//  Modelo de dominio: Autor
// ----------------------------------------------------------------------------
//  Representa a un autor del catálogo. Los atributos DataAnnotations
//  ([Display]) controlan las etiquetas que se muestran en las vistas.
//  Un Autor puede tener muchos Libros (relación 1:N con Libro).
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.Models
{
    public class Autor
    {
        /// <summary>Clave primaria, autogenerada por SQL Server.</summary>
        public int Id { get; set; }

        /// <summary>Nombre completo del autor.</summary>
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nacionalidad del autor.</summary>
        [Display(Name = "Nacionalidad")]
        public string Nacionalidad { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento del autor.</summary>
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Colección de libros escritos por el autor (lado "muchos" de la relación).
        /// Se inicializa vacía para poder agregar elementos sin error de null.
        /// </summary>
        public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
    }
}