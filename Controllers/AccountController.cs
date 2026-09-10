// ============================================================================
//  AccountController : autenticación mediante ASP.NET Core Identity
// ----------------------------------------------------------------------------
//  Gestiona el ciclo de vida de la sesión de los usuarios:
//      - Register       : alta de cuentas nuevas (siempre con rol Lector).
//      - Login          : inicio de sesión con email + contraseña.
//      - Logout         : cierre de sesión (POST, con antiforgery).
//      - AccessDenied   : página que se muestra cuando un rol no puede entrar.
//  Usa UserManager / SignInManager de Identity (NO accede a BD directamente).
// ============================================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BibliotecaApp.Models;
using BibliotecaApp.ViewModels;

namespace BibliotecaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Account/Register — Muestra el formulario de registro.
        /// </summary>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Register — Crea la cuenta con el rol predeterminado
        /// "Lector" y deja la sesión iniciada. Devuelve a la página de origen.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Los usuarios que se registran solos siempre son Lectores.
            var usuario = new ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                EmailConfirmed = true,
                NombreCompleto = viewModel.NombreCompleto
            };

            var resultado = await _userManager.CreateAsync(usuario, viewModel.Password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, "Lector");
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                _logger.LogInformation("Nueva cuenta creada: {Email}", viewModel.Email);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            // Traducción de los errores de Identity a mensajes del formulario.
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        /// <summary>
        /// GET: /Account/Login — Muestra el formulario de acceso.
        /// Conserva la URL a la que se intentaba entrar antes de loguearse.
        /// </summary>
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/Login — Autentica con email y contraseña.
        /// Si tiene éxito, redirige a returnUrl (si es local) o al inicio.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(
                    viewModel.Email, viewModel.Password, viewModel.RememberMe, lockoutOnFailure: false);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation("Sesión iniciada: {Email}", viewModel.Email);

                    // Evita open redirect: solo se vuelve a URLs del propio sitio.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            }

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Account/Logout — Cierra la sesión del usuario actual.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Sesión cerrada.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Página mostrada cuando un usuario autenticado no tiene el rol
        /// requerido por la acción (configurado como AccessDeniedPath).
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}