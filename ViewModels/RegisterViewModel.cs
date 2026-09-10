// ============================================================================
//  ViewModel: Registro de usuario
// ----------------------------------------------------------------------------
//  Datos del formulario de creación de cuenta. Los nuevos usuarios se crean
//  siempre con el rol "Lector" (consulta del catálogo y sus préstamos).
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.ViewModels
{
    public class RegisterViewModel
    {
        /// <summary>Nombre completo que se mostrará en la app.</summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>Correo electrónico que se usará como nombre de usuario.</summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña (debe cumplir la política de Identity).</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Confirmación de la contraseña (debe coincidir).</summary>
        [Required(ErrorMessage = "Confirma la contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}