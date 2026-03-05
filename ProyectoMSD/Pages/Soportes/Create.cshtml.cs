using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

namespace ProyectoMSD.Pages.Soportes
{
    [Authorize(Roles = "Usuario")]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Soporte Soporte { get; set; } = default!;

        // Datos del usuario en sesión (para mostrarlo en la vista)
        public Usuario UsuarioSesion { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var usuarioId = ObtenerIdUsuarioSesion();
            if (usuarioId == null)
                return RedirectToPage("/Index");

            var usuario = await _context.Usuarios.FindAsync(usuarioId.Value);
            if (usuario == null)
                return RedirectToPage("/Index");

            UsuarioSesion = usuario;

            Soporte = new Soporte
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                IdUsuarios = usuarioId.Value,
                Respuesta = "Pendiente"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuarioId = ObtenerIdUsuarioSesion();
            if (usuarioId == null)
                return RedirectToPage("/Index");

            // Forzar siempre los valores que el usuario NO debe poder cambiar
            Soporte.Fecha = DateOnly.FromDateTime(DateTime.Now);
            Soporte.IdUsuarios = usuarioId.Value;
            Soporte.Respuesta = "Pendiente";

            // Limpiar errores previos de Model Binding para campos que asignamos manualmente o no vienen del form
            ModelState.Remove("Soporte.Fecha");
            ModelState.Remove("Soporte.IdUsuarios");
            ModelState.Remove("Soporte.Respuesta");
            ModelState.Remove("Soporte.IdUsuariosNavigation");

            // Validaciones solo de los campos que el usuario sí rellena
            if (string.IsNullOrWhiteSpace(Soporte.Descripcion))
                ModelState.AddModelError("Soporte.Descripcion", "La descripción es obligatoria.");

            if (string.IsNullOrWhiteSpace(Soporte.Tipo))
                ModelState.AddModelError("Soporte.Tipo", "Debe seleccionar un tipo de consulta.");

            if (!ModelState.IsValid)
            {
                // Recargar usuario para la vista
                UsuarioSesion = (await _context.Usuarios.FindAsync(usuarioId.Value))!;
                return Page();
            }

            try
            {
                // Cargar UsuarioSesion para usar su nombre en la notificación
                UsuarioSesion = (await _context.Usuarios.FindAsync(usuarioId.Value))!;

                _context.Soportes.Add(Soporte);
                await _context.SaveChangesAsync();

                // Notificar a todos los administradores del sistema
                var administradores = await _context.Usuarios.Where(u => u.Rol == "Admin").ToListAsync();
                foreach (var admin in administradores)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        IdUsuarios = admin.Id,
                        Titulo = "Nuevo Ticket de Soporte",
                        Mensaje = $"El usuario {UsuarioSesion.Nombre} ha creado un nuevo ticket de tipo {Soporte.Tipo}.",
                        Tipo = "NuevoTicket",
                        Leida = false,
                        FechaCreacion = DateTime.Now,
                        RutaRedireccion = $"/Soportes/Responder/{Soporte.Id}"
                    });
                }
                
                if (administradores.Any())
                {
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Ticket creado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error al crear el ticket.");
                UsuarioSesion = (await _context.Usuarios.FindAsync(usuarioId.Value))!;
                return Page();
            }
        }

        private int? ObtenerIdUsuarioSesion()
        {
            var claim = User.FindFirst("UserId");
            if (claim != null && int.TryParse(claim.Value, out int id))
                return id;
            return null;
        }
    }
}
