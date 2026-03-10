
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using ProyectoMSD.Interfaces;

namespace ProyectoMSD.Pages.Usuarios
{
    [Authorize(Roles = "Admin,Usuario,Huesped")]
    public class IndexModel : PageModel
    {
        private readonly ProyectoMSD.Modelos.AppDbContext _context;
        private readonly IUsuarioService _usuarioService;

        public IndexModel(ProyectoMSD.Modelos.AppDbContext context, IUsuarioService usuarioService) 
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        public IList<Usuario> Usuario { get;set; } = default!;
        public IList<Espacio> Espacios { get; set; } = default!;
        public IList<Dispositivo> Dispositivos { get; set; } = default!;

        public bool RequiereInformacionBasica { get; set; }

        // Propiedades calculadas para las estadísticas
        public int TotalUsuarios => Usuario?.Count ?? 0;
        public int TotalEspacios => Espacios?.Count ?? 0;
        public int TotalDispositivos => Dispositivos?.Count ?? 0;
        public int UsuariosActivos => Usuario?.Count() ?? 0;
        public int DispositivosActivos => Dispositivos?.Count() ?? 0;
        

        public async Task OnGetAsync()
        {
            // Cargar TODOS los datos que necesitas
            Usuario = await _usuarioService.GetAllUsuariosAsync();

            // Si tienes modelo Espacio
            Espacios = await _context.Espacios  // ← Incluye dispositivos relacionados
                .ToListAsync();

            // Si tienes modelo Dispositivo
            Dispositivos = await _context.Dispositivos
                .ToListAsync();

            // Lógica para Alerta de Información Básica
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var currentUser = Usuario.FirstOrDefault(u => u.Id == userId);
                if (currentUser != null)
                {
                    // Determinar si falta información que generalmente no se obtiene de Google
                    if (currentUser.Documento == 0 || currentUser.Telefono == 0)
                    {
                        RequiereInformacionBasica = true;
                    }
                }
            }

          
        }
    }
}

