// ============================================================================
//  ViewModel: Inicio de Sesión
// ----------------------------------------------------------------------------
//  Transporta las credenciales del formulario de login de AccountController.
// ============================================================================
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApp.ViewModels
{
    public class LoginViewModel
    {
        /// <summary>Correo electrónico del usuario.</summary>
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña del usuario.</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Mantener la sesión iniciada tras cerrar el navegador.</summary>
        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        /// <summary>URL a la que volver tras iniciar sesión con éxito.</summary>
        public string? ReturnUrl { get; set; }
    }
}