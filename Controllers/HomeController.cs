// ============================================================================
//  HomeController : controlador de la página principal
// ----------------------------------------------------------------------------
//  Proporciona el dashboard con estadísticas globales de la biblioteca,
//  la página de privacidad y la vista de errores de la aplicación.
// ============================================================================
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BibliotecaApp.Data;
using BibliotecaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApp.Controllers;

public class HomeController : Controller
{
    // Contexto de Entity Framework inyectado por el contenedor de DI.
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Dashboard principal: calcula indicadores y los expone vía ViewBag
    /// para que la vista los pinte sin necesidad de un modelo complejo.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Total de autores registrados.
        ViewBag.TotalAutores = await _context.Autores.CountAsync();

        // Total de libros en el catálogo.
        ViewBag.TotalLibros = await _context.Libros.CountAsync();

        // Libros disponibles: aquellos sin ningún préstamo activo (sin devolver).
        ViewBag.TotalLibrosDisponibles = await _context.Libros
            .CountAsync(l => !l.Prestamos.Any(p => p.FechaDevolucion == null));

        // Préstamos activos: registros cuya fecha de devolución aún no está registrada.
        ViewBag.TotalPrestamosActivos = await _context.Prestamos
            .CountAsync(p => p.FechaDevolucion == null);

        // Los 5 libros más recientes (por orden de alta en el sistema).
        ViewBag.UltimosLibros = await _context.Libros
            .OrderByDescending(l => l.Id)
            .Take(5)
            .ToListAsync();

        return View();
    }

    /// <summary>
    /// Página informativa de privacidad / datos de contacto.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Página de error global. Se muestra cuando ocurre una excepción
    /// no controlada (solo visible fuera del entorno de desarrollo).
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Identificador de la petición actual para facilitar el diagnóstico.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}