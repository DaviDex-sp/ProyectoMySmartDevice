using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoMSD.Modelos;
using System.Security.Claims;

namespace ProyectoMSD.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(AppDbContext db, ILogger<LogoutModel> logger)
        {
            _db = db;
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
                    // Buscar el ultimo login para calcular duracion de sesion
                    var ultimoLogin = _db.RegistroAccesos
                        .Where(r => r.IdUsuarios == userId && r.TipoAccion == "Login")
                        .OrderByDescending(r => r.FechaAcceso)
                        .FirstOrDefault();

                    var registro = new RegistroAcceso
                    {
                        IdUsuarios = userId,
                        FechaAcceso = DateTime.Now,
                        TipoAccion = "Logout",
                        DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Navegador = Request.Headers["User-Agent"].ToString(),
                        PaginaVisitada = "/Logout",
                        DuracionSesion = ultimoLogin != null
                            ? (int)(DateTime.Now - ultimoLogin.FechaAcceso).TotalSeconds
                            : null
                    };
                    _db.RegistroAccesos.Add(registro);
                    await _db.SaveChangesAsync();
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
