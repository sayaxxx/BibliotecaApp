// ============================================================================
//  Modelo de dominio: Libro
// ----------------------------------------------------------------------------
//  Representa un libro del catálogo. Relaciones:
//      - N:1 con Autor   (AutorId)   -> cada libro pertenece a un autor.
//      - 1:N con Prestamo            -> un libro puede tener varios préstamos.
//  Su disponibilidad se calcula en tiempo de consulta: está "prestado" si
//  existe algún Prestamo cuya FechaDevolucion es NULL.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.Models
{
    public class Libro
    {
        /// <summary>Clave primaria, autogenerada por SQL Server.</summary>
        public int Id { get; set; }

        /// <summary>Título del libro.</summary>
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>Código ISBN (identificador internacional del libro).</summary>
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;

        /// <summary>Año de publicación del libro.</summary>
        [Display(Name = "Año de Publicación")]
        public int AnioPublicacion { get; set; }

        /// <summary>Clave foránea hacia el autor del libro.</summary>
        [Display(Name = "Autor")]
        public int AutorId { get; set; }

        /// <summary>Autor asociado (navegación N:1).</summary>
        public virtual Autor? Autor { get; set; }

        /// <summary>Historial de préstamos del libro (navegación 1:N).</summary>
        public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}