using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using ProyectoMSD.Interfaces;

namespace ProyectoMSD.Pages.Usuarios
{
    public class DetailsModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;

        public DetailsModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public Usuario Usuario { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(id.Value);

            if (usuario is not null)
            {
                Usuario = usuario;

                return Page();
            }

            return NotFound();
        }
    }
}
