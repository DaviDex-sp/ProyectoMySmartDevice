using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata;
using ProyectoMSD.Modelos;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Google;
using ProyectoMSD.Interfaces;

namespace ProyectoMSD.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger, IUsuarioService usuarioService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public string Correo { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _usuarioService.AuthenticateAsync(Correo, Password);

            if (usuario != null)
            {
                // Crear claims de autenticacion
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol),
                    new Claim("UserId", usuario.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Registrar el acceso en la tabla de seguimiento
                try
                {
                    await _usuarioService.RegisterAccessAsync(
                        usuario.Id, 
                        "Login", 
                        HttpContext.Connection.RemoteIpAddress?.ToString(), 
                        Request.Headers["User-Agent"].ToString(), 
                        "/Login");
                }
                catch (Exception ex)
                {
                    // No bloquear el login si falla el registro
                    _logger.LogWarning(ex, "No se pudo registrar el acceso del usuario {UserId}", usuario.Id);
                }

                return RedirectToPage("/Usuarios/Index");
            }

            ModelState.AddModelError("", "Usuario o contrasena invalidos");
            return Page();
        }

        public IActionResult OnPostGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Page("./Login", pageHandler: "GoogleCallback") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> OnGetGoogleCallbackAsync()
        {
            // Leer la información del usuario desde la cookie temporal
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error al autenticarse con Google.");
                return Page();
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "No se pudo recuperar el correo electrónico de Google.");
                return Page();
            }

            // Buscar/Crear usuario en DB mediante servicio
            var usuario = await _usuarioService.AuthenticateGoogleAsync(email, name);

            // Crear claims y hacer SignIn local
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("UserId", usuario.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Borrar la cookie temporal de Google
            await HttpContext.SignOutAsync("ExternalCookie");

            // Registrar el acceso en la tabla de seguimiento
            try
            {
                await _usuarioService.RegisterAccessAsync(
                    usuario.Id, 
                    "Login Google", 
                    HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers["User-Agent"].ToString(), 
                    "/Login");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar el acceso de Google del usuario {UserId}", usuario.Id);
            }

            return RedirectToPage("/Usuarios/Index");
        }
    }
}
