using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ProyectoMSD.Interfaces;

namespace ProyectoMSD.Pages.Usuarios
{
    public class EditModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;

        public EditModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario =  await _usuarioService.GetUsuarioByIdAsync(id.Value);
            if (usuario == null)
            {
                return NotFound();
            }
            Usuario = usuario;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Si la clave viene nula o vacía (el usuario no la cambió en el form), mantenerla (aquí dependería de la lógica de UI, asumo que se reasigna o hash si cambió)
            if (Usuario.Clave != null && !Usuario.Clave.Contains(":")) 
            {
                Usuario.Clave = _usuarioService.HashPassword(Usuario.Clave);
            }

            try
            {
                await _usuarioService.UpdateUsuarioAsync(Usuario);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _usuarioService.GetUsuarioByIdAsync(Usuario.Id) != null;
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
