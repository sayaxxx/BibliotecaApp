// ============================================================================
//  ViewModel: Crear / Editar Autor
// ----------------------------------------------------------------------------
//  Se usa en los formularios de alta y edición de autores (módulo refactorizado).
//  Aísla la vista de las entidades de dominio: la vista valida contra este
//  objeto y nunca recibe el modelo Autor directamente.
//  Las DataAnnotations generan la validación del lado del cliente y del servidor.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.ViewModels
{
    public class CrearEditarAutorViewModel
    {
        /// <summary>Nombre completo del autor (obligatorio, máx. 100).</summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Nacionalidad del autor (obligatorio, máx. 50).</summary>
        [Required(ErrorMessage = "La nacionalidad es obligatoria")]
        [StringLength(50, ErrorMessage = "La nacionalidad no puede exceder 50 caracteres")]
        [Display(Name = "Nacionalidad")]
        public string Nacionalidad { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de nacimiento. Es nullable porque el selector de fecha del
        /// navegador puede no enviar valor; se valida como requerida.
        /// </summary>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }
    }
}