using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BibliotecaApp.Models
{
    // ==========================================================================
    //  Atributo de validación: FechaNoAnteriorHoy
    // --------------------------------------------------------------------------
    //  Valida que una fecha no sea anterior al día actual (DateTime.Today).
    //  Se usa en el modelo Prestamo sobre FechaPrestamo y FechaDevolucion:
    //   - Aplica TANTO del lado del servidor (IsValid, se comprueba en el
    //     POST de Create/Edit mediante ModelState.IsValid) como del lado del
    //     cliente (IClientModelValidator => mensajes sin recargar la página).
    //   - Si el valor es null se considera válido (caso de FechaDevolucion,
    //     que represente un préstamo aún pendiente de devolución).
    // ==========================================================================
    public sealed class FechaNoAnteriorHoyAttribute : ValidationAttribute, IClientModelValidator
    {
        /// <summary>
        /// Validación servidor: la fecha debe ser igual o posterior a hoy.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // null es válido: la devolución puede no estar registrada todavía.
            if (value is not DateTime fecha)
            {
                return ValidationResult.Success;
            }

            // Solo comparamos la parte de fecha (sin hora).
            if (fecha.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha no puede ser anterior a la fecha actual");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Expone los metadatos (data-val) que jQuery unobtrusive usa para
        /// mostrar el error en el cliente. La regla "fechanoanterior" está
        /// registrada en _ValidationScriptsPartial.cshtml.
        /// </summary>
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-fechanoanterior"] = ErrorMessage ?? "La fecha no puede ser anterior a la fecha actual";
        }
    }
}