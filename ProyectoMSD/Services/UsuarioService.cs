using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Interfaces;
using ProyectoMSD.Modelos;

namespace ProyectoMSD.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _db;

        public UsuarioService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> AuthenticateAsync(string correo, string password)
        {
            var hashPassword = HashPassword(password);
            
            return await _db.Usuarios
                .Where(x => x.Correo == correo && x.Clave == hashPassword)
                .FirstOrDefaultAsync();
        }

        public async Task<Usuario> AuthenticateGoogleAsync(string email, string name)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == email);

            if (usuario == null)
            {
                // Crear nuevo usuario con valores por defecto
                usuario = new Usuario
                {
                    Nombre = name ?? "Usuario Google",
                    Correo = email,
                    Clave = "[Autenticado por Google]",
                    Rol = "Usuario",
                    Telefono = 0,
                    Ubicacion = "Por Defecto",
                    Permisos = "Basico",
                    Acesso = "Activo",
                    Documento = 0
                };

                _db.Usuarios.Add(usuario);
                await _db.SaveChangesAsync();
            }

            return usuario;
        }

        public async Task RegisterAccessAsync(int userId, string action, string? ip, string? userAgent, string path)
        {
            var registro = new RegistroAcceso
            {
                IdUsuarios = userId,
                FechaAcceso = DateTime.Now,
                TipoAccion = action,
                DireccionIp = ip,
                Navegador = userAgent,
                PaginaVisitada = path
            };
            
            _db.RegistroAccesos.Add(registro);
            await _db.SaveChangesAsync();
        }

        public async Task RegisterLogoutAsync(int userId, string? ip, string? userAgent, string path)
        {
            // Buscar el ultimo login para calcular duracion de sesion
            var ultimoLogin = await _db.RegistroAccesos
                .Where(r => r.IdUsuarios == userId && r.TipoAccion == "Login")
                .OrderByDescending(r => r.FechaAcceso)
                .FirstOrDefaultAsync();

            var registro = new RegistroAcceso
            {
                IdUsuarios = userId,
                FechaAcceso = DateTime.Now,
                TipoAccion = "Logout",
                DireccionIp = ip,
                Navegador = userAgent,
                PaginaVisitada = path,
                DuracionSesion = ultimoLogin != null
                    ? (int)(DateTime.Now - ultimoLogin.FechaAcceso).TotalSeconds
                    : null
            };
            
            _db.RegistroAccesos.Add(registro);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Usuario>> GetAllUsuariosAsync()
        {
            return await _db.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task CreateUsuarioAsync(Usuario usuario)
        {
            if (usuario.Clave != null && !usuario.Clave.StartsWith("[Autenticado") && !usuario.Clave.Contains(":")) 
            {
                usuario.Clave = HashPassword(usuario.Clave);
            }
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            _db.Attach(usuario).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUsuarioAsync(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _db.Usuarios.Remove(usuario);
                await _db.SaveChangesAsync();
            }
        }

        public bool EsEmailValido(string email)
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

        public string HashPassword(string password)
        {
            // Hash con PBKDF2 similar a la lógica actual (salt vacio en la imp original)
            byte[] salt = new byte[0];
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public async Task<bool> ExisteCorreoAsync(string correo)
        {
            return await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == correo.ToLower());
        }

        public async Task<bool> ExisteDocumentoAsync(long documento)
        {
            return await _db.Usuarios.AnyAsync(u => u.Documento == documento);
        }
    }
}
