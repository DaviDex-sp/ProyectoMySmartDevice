using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System.Threading.Tasks;

namespace ProyectoMSD.Pages.Soportes
{
    [Authorize(Roles = "Admin")]
    public class ResponderModel : PageModel
    {
        private readonly AppDbContext _context;

        public ResponderModel(AppDbContext context)
        {
            _context = context;
        }

        public Soporte Soporte { get; set; } = default!;

        [BindProperty]
        public string Respuesta { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var soporte = await _context.Soportes
                .Include(s => s.IdUsuariosNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (soporte == null)
                return NotFound();

            Soporte = soporte;
            Respuesta = soporte.Respuesta ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var soporte = await _context.Soportes.FindAsync(id);
            if (soporte == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(Respuesta))
            {
                ModelState.AddModelError(nameof(Respuesta), "La respuesta no puede estar vacía.");
                // Recargar para mostrar datos del ticket
                Soporte = await _context.Soportes
                    .Include(s => s.IdUsuariosNavigation)
                    .FirstAsync(s => s.Id == id);
                return Page();
            }

            // Solo actualizamos el campo Respuesta
            soporte.Respuesta = Respuesta.Trim();
            await _context.SaveChangesAsync();

            _context.Notificaciones.Add(new Notificacion
            {
                IdUsuarios = soporte.IdUsuarios,
                Titulo = "Ticket respondido",
                Mensaje = $"Tu ticket #{id} ha sido respondido por el equipo de soporte.",
                Tipo = "TicketRespondido",
                Leida = false,
                FechaCreacion = DateTime.Now,
                RutaRedireccion = $"/Soportes/Details?id={id}"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ticket #{id} respondido exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
