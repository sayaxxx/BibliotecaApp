// ============================================================================
//  Modelo de vista: ErrorViewModel
// ----------------------------------------------------------------------------
//  Modelo mínimo para la vista de error global (/Home/Error).
//  Solo transporta el identificador de la petición a modo de diagnóstico.
// ============================================================================
namespace BibliotecaApp.Models;

public class ErrorViewModel
{
    /// <summary>Identificador de la petición HTTP que provocó el error.</summary>
    public string? RequestId { get; set; }

    /// <summary>Indica si existe un RequestId para mostrar en la vista.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}