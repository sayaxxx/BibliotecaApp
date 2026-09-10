// ============================================================================
//  ApplicationUser : modelo de usuario de ASP.NET Core Identity
// ----------------------------------------------------------------------------
//  Extiende IdentityUser agregando el campo NombreCompleto. La tabla de
//  usuarios del sistema es AspNetUsers y se administra con los managers de
//  Identity (UserManager/IdentityRole) en lugar de accederla directamente.
//
//  Roles definidos en el sistema:
//      - Admin         : acceso total (gestión de usuarios y roles).
//      - Bibliotecario : Autores, Libros y préstamos/devoluciones.
//      - Lector        : lectura del catálogo y consulta de sus préstamos.
// ============================================================================
using Microsoft.AspNetCore.Identity;

namespace BibliotecaApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Nombre completo o visible del usuario (se muestra en la barra
        /// de navegación y en el módulo de préstamos).
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;
    }
}