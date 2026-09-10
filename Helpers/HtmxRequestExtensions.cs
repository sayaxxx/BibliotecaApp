// ============================================================================
//  Helpers / HtmxRequestExtensions
// ----------------------------------------------------------------------------
//  Extensiones auxiliares para trabajar con HTMX en los controladores.
//  HTMX envía la cabecera "HX-Request: true" en todas sus peticiones AJAX.
// ============================================================================
using Microsoft.AspNetCore.Http;

namespace BibliotecaApp.Helpers
{
    public static class HtmxRequestExtensions
    {
        /// <summary>
        /// True si la petición HTTP actual proviene de HTMX (cabecera HX-Request).
        /// Permite a los controladores devolver solo la parcial en lugar de la
        /// vista completa (Sobre-Especificación de respuesta de hypermedia).
        /// </summary>
        public static bool IsHtmx(this HttpRequest request)
        {
            return string.Equals(
                request.Headers["HX-Request"].ToString(),
                "true",
                StringComparison.OrdinalIgnoreCase);
        }
    }
}