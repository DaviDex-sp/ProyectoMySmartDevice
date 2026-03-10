using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ProyectoMSD.Pages.Perfil
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        // Campos editables del formulario
        [BindProperty]
        public string? NuevaClave { get; set; }

        public string? MensajeExito { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = ObtenerUsuarioId();
            if (userId == null) return RedirectToPage("/Index");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null) return NotFound();

            Usuario = usuario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = ObtenerUsuarioId();
            if (userId == null) return RedirectToPage("/Index");

            var usuarioDb = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);
            if (usuarioDb == null) return NotFound();

            // Actualizar solo los campos permitidos
            usuarioDb.Nombre = Usuario.Nombre;
            usuarioDb.Correo = Usuario.Correo;
            usuarioDb.PrefijoTelefono = Usuario.PrefijoTelefono;
            usuarioDb.Telefono = Usuario.Telefono;
            usuarioDb.Ubicacion = Usuario.Ubicacion;

            // Si se proporcionó una nueva contraseña, hashearla
            bool contrasenaCambiada = false;
            if (!string.IsNullOrWhiteSpace(NuevaClave))
            {
                byte[] salt = new byte[0];
                var pbkdf2 = new Rfc2898DeriveBytes(NuevaClave, salt, 100_000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);
                usuarioDb.Clave = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
                contrasenaCambiada = true;
            }

            try
            {
                await _context.SaveChangesAsync();
                bool datosPersonalesCambiados = true;

                if (datosPersonalesCambiados)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        IdUsuarios = userId.Value,
                        Titulo = "Perfil actualizado",
                        Mensaje = "Has actualizado tu información personal exitosamente.",
                        Tipo = "PerfilActualizado",
                        Leida = false,
                        FechaCreacion = DateTime.Now
                    });
                }

                if (contrasenaCambiada)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        IdUsuarios = userId.Value,
                        Titulo = "Contraseña cambiada",
                        Mensaje = "Tu contraseña ha sido cambiada exitosamente.",
                        Tipo = "ContraseñaCambiada",
                        Leida = false,
                        FechaCreacion = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                MensajeExito = "Perfil actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(e => e.Id == userId))
                    return NotFound();
                throw;
            }

            // Recargar datos para que la vista los muestre actualizados
            Usuario = usuarioDb;
            return Page();
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
