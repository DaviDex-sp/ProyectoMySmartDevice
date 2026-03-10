using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoMSD.Modelos;
using ProyectoMSD.Interfaces;
using System.Security.Claims;

namespace ProyectoMSD.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IUsuarioService usuarioService, ILogger<LogoutModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Registrar el logout antes de cerrar sesion
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    await _usuarioService.RegisterLogoutAsync(
                        userId, 
                        HttpContext.Connection.RemoteIpAddress?.ToString(), 
                        Request.Headers["User-Agent"].ToString(), 
                        "/Logout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar el logout");
            }

            // Cerrar sesion
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirigir al login
            return RedirectToPage("/Index");
        }
    }
}
