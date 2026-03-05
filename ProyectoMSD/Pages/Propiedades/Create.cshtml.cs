using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoMSD.Pages.Propiedades
{
    public class CreateModel : PageModel
    {
        private readonly ProyectoMSD.Modelos.AppDbContext _context;

        public CreateModel(ProyectoMSD.Modelos.AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // CORREGIDO: Mostrar nombre del usuario, no el ID
            await LoadUsuariosDropdown();
            return Page();
        }

        [BindProperty]
        public Propiedade Propiedade { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            // DEBUGGING - Verificar qué datos llegan
            Console.WriteLine($"===== DEBUGGING DATOS =====");
            Console.WriteLine($"IdUsuarios: {Propiedade.IdUsuarios}");
            Console.WriteLine($"Direccion: {Propiedade.Direccion}");
            Console.WriteLine($"Tipo: {Propiedade.Tipo}");
            Console.WriteLine($"Pisos: {Propiedade.Pisos}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            // VALIDACIÓN ADICIONAL para la llave foránea
            if (Propiedade.IdUsuarios <= 0)
            {
                ModelState.AddModelError("Propiedade.IdUsuarios", "Debe seleccionar un usuario válido");
            }
            else
            {
                // VERIFICAR que el usuario existe en la base de datos
                var usuarioExists = await _context.Usuarios
                    .AnyAsync(u => u.Id == Propiedade.IdUsuarios);

                if (!usuarioExists)
                {
                    ModelState.AddModelError("Propiedade.IdUsuarios", "El usuario seleccionado no existe");
                }
            }

            if (!ModelState.IsValid)
            {
                // CRÍTICO: Re-cargar el dropdown cuando hay errores
                await LoadUsuariosDropdown();

                // DEBUGGING - Mostrar errores
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"Error en {modelError.Key}: {error.ErrorMessage}");
                    }
                }

                return Page();
            }

            try
            {
                Console.WriteLine("===== INTENTANDO GUARDAR =====");
                _context.Propiedades.Add(Propiedade);

                // NOTIFICACIÓN PARA EL USUARIO ASIGNADO
                _context.Notificaciones.Add(new Notificacion
                {
                    IdUsuarios = Propiedade.IdUsuarios,
                    Titulo = "Nueva Propiedad Asignada",
                    Mensaje = $"Se te ha asignado una nueva propiedad: {Propiedade.Direccion}",
                    Tipo = "Informacion",
                    FechaCreacion = DateTime.Now,
                    Leida = false,
                    RutaRedireccion = "/Propiedades/Index"
                });

                // NOTIFICACIÓN PARA EL ADMINISTRADOR (CREADOR)
                var adminIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(adminIdClaim, out int adminId))
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        IdUsuarios = adminId,
                        Titulo = "Propiedad Creada Exitosamente",
                        Mensaje = $"Has registrado la propiedad: {Propiedade.Direccion}",
                        Tipo = "Exito",
                        FechaCreacion = DateTime.Now,
                        Leida = false,
                        RutaRedireccion = "/Propiedades/Index"
                    });
                }

                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Filas afectadas: {result}");
                Console.WriteLine("===== GUARDADO EXITOSO =====");

                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"===== ERROR AL GUARDAR =====");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // MOSTRAR error específico de llave foránea
                if (ex.InnerException?.Message?.Contains("FOREIGN KEY") == true)
                {
                    ModelState.AddModelError("Propiedade.IdUsuarios", "Error: El usuario seleccionado no es válido");
                }
                else
                {
                    ModelState.AddModelError("", $"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
                }

                await LoadUsuariosDropdown();
                return Page();
            }
        }

        private async Task LoadUsuariosDropdown()
        {
            // MÉTODO CORRECTO: Mostrar nombre y correo, valor = Id
            var usuarios = await _context.Usuarios
                .Where(u => u.Id != null) // Solo usuarios activos
                .OrderBy(u => u.Nombre)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.Nombre} - {u.Correo}" // ESTO es lo que ve el usuario
                })
                .ToListAsync();

            ViewData["IdUsuarios"] = new SelectList(usuarios, "Value", "Text");

            // DEBUGGING - Verificar que se cargaron usuarios
            Console.WriteLine($"Usuarios cargados en dropdown: {usuarios.Count}");
        }
    }
}

