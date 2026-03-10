using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoMSD.Modelos;

namespace ProyectoMSD.Interfaces
{
    public interface IUsuarioService
    {
        // Autenticación
        Task<Usuario?> AuthenticateAsync(string correo, string password);
        Task<Usuario> AuthenticateGoogleAsync(string email, string name);
        Task RegisterAccessAsync(int userId, string action, string? ip, string? userAgent, string path);
        Task RegisterLogoutAsync(int userId, string? ip, string? userAgent, string path);

        // CRUD de Usuarios
        Task<List<Usuario>> GetAllUsuariosAsync();
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task CreateUsuarioAsync(Usuario usuario);
        Task UpdateUsuarioAsync(Usuario usuario);
        Task DeleteUsuarioAsync(int id);
        
        // Validación
        bool EsEmailValido(string email);
        string HashPassword(string password);
        Task<bool> ExisteCorreoAsync(string correo);
        Task<bool> ExisteDocumentoAsync(long documento);
    }
}
