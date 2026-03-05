using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

namespace ProyectoMSD.Pages.Notificaciones
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Notificacion> Notificaciones { get; set; } = new();
        public int CantidadNoLeidas { get; set; }
        public bool MostrarTodas { get; set; }

        public async Task<IActionResult> OnGetAsync(bool mostrarTodas = false)
        {
            var userId = ObtenerUsuarioId();
            if (userId == null) return RedirectToPage("/Login");

            var query = _context.Notificaciones
                .Where(n => n.IdUsuarios == userId.Value)
                .OrderByDescending(n => n.FechaCreacion);

            MostrarTodas = mostrarTodas;

            if (mostrarTodas)
            {
                Notificaciones = await query.ToListAsync();
            }
            else
            {
                Notificaciones = await query.Take(5).ToListAsync();
            }

            CantidadNoLeidas = await _context.Notificaciones.CountAsync(n => n.IdUsuarios == userId.Value && !n.Leida);

            return Page();
        }

        public async Task<IActionResult> OnPostMarcarLeidaAsync(int id)
        {
            var userId = ObtenerUsuarioId();
            if (userId == null) return RedirectToPage("/Index");

            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuarios == userId.Value);

            if (notificacion != null)
            {
                notificacion.Leida = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarcarTodasAsync()
        {
            var userId = ObtenerUsuarioId();
            if (userId == null) return RedirectToPage("/Index");

            var noLeidas = await _context.Notificaciones
                .Where(n => n.IdUsuarios == userId.Value && !n.Leida)
                .ToListAsync();

            foreach (var n in noLeidas)
            {
                n.Leida = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private int? ObtenerUsuarioId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }
    }
}
