using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System.Security.Cryptography;
using ProyectoMSD.Interfaces;

namespace ProyectoMSD.Pages
{
    public class RegistrarModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;

        public RegistrarModel(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        public void OnGet()
        {
            // Inicializar valores por defecto
            Usuario = new Usuario
            {
                Permisos = "limitado",    
                Acesso = "pendiente"      
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validaciones personalizadas
            await ValidarDatos();

            if (!ModelState.IsValid)
            {
                return Page();
            }
          
            try
            {
                // Limpiar y formatear datos
                LimpiarDatos();

                // FORZAR valores seguros para registro público (SIEMPRE)
                Usuario.Permisos = "limitado";      // SIEMPRE limitado
                Usuario.Acesso = "pendiente";       // SIEMPRE pendiente 
                
                await _usuarioService.CreateUsuarioAsync(Usuario);

                TempData["SuccessMessage"] = "¡Cuenta creada exitosamente! Tu cuenta está pendiente de activación por un administrador.";
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al crear la cuenta. Inténtalo de nuevo.");
                return Page();
            }
        }

        private async Task ValidarDatos()
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(Usuario.Nombre))
                ModelState.AddModelError("Usuario.Nombre", "El nombre es obligatorio.");
            else if (Usuario.Nombre.Length < 2)
                ModelState.AddModelError("Usuario.Nombre", "El nombre debe tener al menos 2 caracteres.");
            else if (Usuario.Nombre.Length > 100)
                ModelState.AddModelError("Usuario.Nombre", "El nombre no puede exceder 100 caracteres.");

            // Validar correo
            if (string.IsNullOrWhiteSpace(Usuario.Correo))
                ModelState.AddModelError("Usuario.Correo", "El correo es obligatorio.");
            else if (!EsEmailValido(Usuario.Correo))
                ModelState.AddModelError("Usuario.Correo", "El formato del correo no es v�lido.");
            else if (Usuario.Correo.Length > 200)
                ModelState.AddModelError("Usuario.Correo", "El correo no puede exceder 200 caracteres.");
            else
            {
                // Verificar si el correo ya existe
                var existeCorreo = await _usuarioService.ExisteCorreoAsync(Usuario.Correo);

                if (existeCorreo)
                    ModelState.AddModelError("Usuario.Correo", "Ya existe un usuario con este correo.");
            }

            // Validar contrase�a
            if (string.IsNullOrWhiteSpace(Usuario.Clave))
                ModelState.AddModelError("Usuario.Clave", "La contrase�a es obligatoria.");
            else if (Usuario.Clave.Length < 6)
                ModelState.AddModelError("Usuario.Clave", "La contrase�a debe tener al menos 6 caracteres.");
            else if (Usuario.Clave.Length > 50)
                ModelState.AddModelError("Usuario.Clave", "La contrase�a no puede exceder 50 caracteres.");

            // Validar tel�fono
            if (string.IsNullOrWhiteSpace(Usuario.Telefono))
                ModelState.AddModelError("Usuario.Telefono", "El tel�fono debe ser un n�mero v�lido.");
            else if (Usuario.Telefono.Trim().Length < 7)
                ModelState.AddModelError("Usuario.Telefono", "El tel�fono debe tener al menos 7 d�gitos.");
            else if (Usuario.Telefono.Trim().Length > 15)
                ModelState.AddModelError("Usuario.Telefono", "El tel�fono no puede exceder 15 d�gitos.");

            // Validar ubicaci�n
            if (string.IsNullOrWhiteSpace(Usuario.Ubicacion))
                ModelState.AddModelError("Usuario.Ubicacion", "La ubicaci�n es obligatoria.");
            else if (Usuario.Ubicacion.Length < 3)
                ModelState.AddModelError("Usuario.Ubicacion", "La ubicaci�n debe tener al menos 3 caracteres.");
            else if (Usuario.Ubicacion.Length > 150)
                ModelState.AddModelError("Usuario.Ubicacion", "La ubicaci�n no puede exceder 150 caracteres.");

            // Validar documento si se proporciona
            if (Usuario.Documento.HasValue)
            {
                if (Usuario.Documento <= 0)
                    ModelState.AddModelError("Usuario.Documento", "El documento debe ser un n�mero v�lido.");
                else if (Usuario.Documento.ToString().Length < 6)
                    ModelState.AddModelError("Usuario.Documento", "El documento debe tener al menos 6 d�gitos.");
                else
                {
                    var existeDocumento = await _usuarioService.ExisteDocumentoAsync(Usuario.Documento.Value);

                    if (existeDocumento)
                        ModelState.AddModelError("Usuario.Documento", "Ya existe un usuario con este documento.");
                }
            }

            // Validar RUT si se proporciona
            if (!string.IsNullOrWhiteSpace(Usuario.Rut))
            {
                if (Usuario.Rut.Length < 8)
                    ModelState.AddModelError("Usuario.Rut", "El RUT debe tener al menos 8 caracteres.");
                else if (Usuario.Rut.Length > 15)
                    ModelState.AddModelError("Usuario.Rut", "El RUT no puede exceder 15 caracteres.");
            }

            // Validar rol (aunque sea forzado despu�s)
            if (string.IsNullOrWhiteSpace(Usuario.Rol))
                ModelState.AddModelError("Usuario.Rol", "Debe seleccionar un rol.");
        }

        private void LimpiarDatos()
        {
            Usuario.Nombre = Usuario.Nombre?.Trim();
            Usuario.Correo = Usuario.Correo?.Trim().ToLower();
            Usuario.Ubicacion = Usuario.Ubicacion?.Trim();
            Usuario.Rut = string.IsNullOrWhiteSpace(Usuario.Rut) ? null : Usuario.Rut.Trim();
            Usuario.Clave = Usuario.Clave?.Trim();
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
