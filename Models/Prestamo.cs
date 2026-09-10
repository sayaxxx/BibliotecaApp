// ============================================================================
//  Modelo de dominio: Prestamo
// ----------------------------------------------------------------------------
//  Representa el préstamo de un libro a un usuario.
//  El estado del préstamo se deduce de FechaDevolucion:
//      - NULL     -> préstamo ACTIVO (pendiente de devolución).
//      - con valor-> préstamo DEVUELTO (la fecha es la de la devolución).
//  Relación N:1 con Libro (LibroId) usando el nombre del usuario como texto.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.Models
{
    public class Prestamo
    {
        /// <summary>Clave primaria, autogenerada por SQL Server.</summary>
        public int Id { get; set; }

        /// <summary>Nombre del usuario que recibe el libro.</summary>
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>Fecha en que se entregó el libro (no puede ser anterior a hoy).</summary>
        [Display(Name = "Fecha de Préstamo")]
        [FechaNoAnteriorHoy(ErrorMessage = "La fecha de préstamo no puede ser anterior a la fecha actual")]
        public DateTime FechaPrestamo { get; set; }

        /// <summary>
        /// Fecha de devolución. Es nula mientras el préstamo está vigente;
        /// al registrarla, el préstamo pasa a estado "devuelto".
        /// No puede ser anterior al día actual.
        /// </summary>
        [Display(Name = "Fecha de Devolución")]
        [FechaNoAnteriorHoy(ErrorMessage = "La fecha de devolución no puede ser anterior a la fecha actual")]
        public DateTime? FechaDevolucion { get; set; }

        /// <summary>Clave foránea hacia el libro prestado.</summary>
        [Display(Name = "Libro")]
        public int LibroId { get; set; }

        /// <summary>Libro prestado (navegación N:1).</summary>
        public virtual Libro? Libro { get; set; }
    }
}