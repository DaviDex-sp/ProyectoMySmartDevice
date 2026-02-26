using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using System.Text.Json;

namespace ProyectoMSD.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        // KPI Totales
        public int TotalUsuarios { get; set; }
        public int TotalDispositivos { get; set; }
        public int TotalPropiedades { get; set; }
        public int TotalEspacios { get; set; }
        public int TotalSoportes { get; set; }
        public int SoportesPendientes { get; set; }
        public int TotalConfiguraciones { get; set; }
        public int TotalAccesosHoy { get; set; }

        // JSON para Chart.js
        public string DispositivosPorTipoJson { get; set; } = "{}";
        public string DispositivosPorEstadoJson { get; set; } = "{}";
        public string SoportesPorTipoJson { get; set; } = "{}";
        public string UsuariosPorRolJson { get; set; } = "{}";
        public string PropiedadesPorTipoJson { get; set; } = "{}";
        public string AccesosPorDiaJson { get; set; } = "{}";

        // Tablas
        public List<RegistroAcceso> UltimosAccesos { get; set; } = new();
        public List<UsuarioActivo> UsuariosMasActivos { get; set; } = new();
        public List<Soporte> UltimosSoportes { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsuarios = await _db.Usuarios.CountAsync();
            TotalDispositivos = await _db.Dispositivos.CountAsync();
            TotalPropiedades = await _db.Propiedades.CountAsync();
            TotalEspacios = await _db.Espacios.CountAsync();
            TotalSoportes = await _db.Soportes.CountAsync();
            TotalConfiguraciones = await _db.Configuraciones.CountAsync();

            SoportesPendientes = await _db.Soportes
                .Where(s => string.IsNullOrEmpty(s.Respuesta))
                .CountAsync();

            TotalAccesosHoy = await _db.RegistroAccesos
                .Where(r => r.FechaAcceso.Date == DateTime.Today)
                .CountAsync();

            // Dispositivos por tipo
            var dispPorTipo = await _db.Dispositivos
                .GroupBy(d => d.Tipo)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();
            DispositivosPorTipoJson = JsonSerializer.Serialize(new {
                labels = dispPorTipo.Select(x => x.Label),
                data = dispPorTipo.Select(x => x.Count)
            });

            // Dispositivos por estado
            var dispPorEstado = await _db.Dispositivos
                .GroupBy(d => d.Estado)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();
            DispositivosPorEstadoJson = JsonSerializer.Serialize(new {
                labels = dispPorEstado.Select(x => x.Label),
                data = dispPorEstado.Select(x => x.Count)
            });

            // Soportes por tipo
            var sopPorTipo = await _db.Soportes
                .GroupBy(s => s.Tipo)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();
            SoportesPorTipoJson = JsonSerializer.Serialize(new {
                labels = sopPorTipo.Select(x => x.Label),
                data = sopPorTipo.Select(x => x.Count)
            });

            // Usuarios por rol
            var usrPorRol = await _db.Usuarios
                .GroupBy(u => u.Rol)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();
            UsuariosPorRolJson = JsonSerializer.Serialize(new {
                labels = usrPorRol.Select(x => x.Label),
                data = usrPorRol.Select(x => x.Count)
            });

            // Propiedades por tipo
            var propPorTipo = await _db.Propiedades
                .GroupBy(p => p.Tipo)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .ToListAsync();
            PropiedadesPorTipoJson = JsonSerializer.Serialize(new {
                labels = propPorTipo.Select(x => x.Label),
                data = propPorTipo.Select(x => x.Count)
            });

            // Accesos ultimos 7 dias
            var hace7dias = DateTime.Today.AddDays(-6);
            var accesosPorDia = await _db.RegistroAccesos
                .Where(r => r.FechaAcceso >= hace7dias && r.TipoAccion == "Login")
                .ToListAsync();

            var agrupados = accesosPorDia
                .GroupBy(r => r.FechaAcceso.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var diasCompletos = Enumerable.Range(0, 7)
                .Select(i => hace7dias.AddDays(i))
                .Select(fecha => new {
                    Label = fecha.ToString("dd/MM"),
                    Count = agrupados.ContainsKey(fecha) ? agrupados[fecha] : 0
                }).ToList();

            AccesosPorDiaJson = JsonSerializer.Serialize(new {
                labels = diasCompletos.Select(x => x.Label),
                data = diasCompletos.Select(x => x.Count)
            });

            // Ultimos 10 accesos
            UltimosAccesos = await _db.RegistroAccesos
                .Include(r => r.IdUsuariosNavigation)
                .OrderByDescending(r => r.FechaAcceso)
                .Take(10)
                .ToListAsync();

            // Usuarios mas activos
            var accesosAgrupados = await _db.RegistroAccesos
                .Where(r => r.TipoAccion == "Login")
                .Include(r => r.IdUsuariosNavigation)
                .ToListAsync();

            UsuariosMasActivos = accesosAgrupados
                .GroupBy(r => new { r.IdUsuarios, r.IdUsuariosNavigation.Nombre })
                .Select(g => new UsuarioActivo {
                    Nombre = g.Key.Nombre,
                    TotalAccesos = g.Count(),
                    UltimoAcceso = g.Max(r => r.FechaAcceso)
                })
                .OrderByDescending(u => u.TotalAccesos)
                .Take(5)
                .ToList();

            // Ultimos 5 soportes
            UltimosSoportes = await _db.Soportes
                .Include(s => s.IdUsuariosNavigation)
                .OrderByDescending(s => s.Fecha)
                .Take(5)
                .ToListAsync();
        }
    }

    public class UsuarioActivo
    {
        public string Nombre { get; set; } = "";
        public int TotalAccesos { get; set; }
        public DateTime UltimoAcceso { get; set; }
    }
}
