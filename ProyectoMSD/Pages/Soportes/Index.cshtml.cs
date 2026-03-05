using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoMSD.Pages.Soportes
{
    [Authorize(Roles = "Admin,Usuario")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Soporte> Soporte { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (User.IsInRole("Admin"))
            {
                // Admin ve TODOS los tickets, ordenados: Actualizados (No Pendientes) primero, luego por Fecha más reciente
                Soporte = await _context.Soportes
                    .Include(s => s.IdUsuariosNavigation)
                    .OrderBy(s => s.Respuesta == "Pendiente" ? 1 : 0) // Actualizados/Respondidos (0) van antes que Pendientes (1)
                    .ThenByDescending(s => s.Fecha)
                    .ToListAsync();
            }
            else
            {
                // Usuario ve solo sus propios tickets, ordenados: Actualizados (No Pendientes) primero, luego por Fecha
                var claimValue = User.FindFirst("UserId")?.Value;
                if (int.TryParse(claimValue, out int usuarioId))
                {
                    Soporte = await _context.Soportes
                        .Include(s => s.IdUsuariosNavigation)
                        .Where(s => s.IdUsuarios == usuarioId)
                        .OrderBy(s => s.Respuesta == "Pendiente" ? 1 : 0) // Actualizados/Respondidos (0) van antes que Pendientes (1)
                        .ThenByDescending(s => s.Fecha)
                        .ToListAsync();
                }
                else
                {
                    Soporte = new List<Soporte>();
                }
            }
        }
    }
}
