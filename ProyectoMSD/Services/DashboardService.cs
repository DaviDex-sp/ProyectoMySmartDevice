using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Interfaces;
using ProyectoMSD.Modelos;
using ProyectoMSD.Modelos.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoMSD.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync()
    {
        var dto = new DashboardMetricsDto();

        // 1. KPIs Generales Básicos
        dto.TotalUsuarios = await _db.Usuarios.CountAsync();
        dto.TotalDispositivos = await _db.Dispositivos.CountAsync();
        dto.TotalPropiedades = await _db.Propiedades.CountAsync();
        dto.TotalEspacios = await _db.Espacios.CountAsync();
        dto.TotalSoportes = await _db.Soportes.CountAsync();
        dto.TotalConfiguraciones = await _db.Configuraciones.CountAsync();

        dto.SoportesPendientes = await _db.Soportes
            .Where(s => string.IsNullOrEmpty(s.Respuesta))
            .CountAsync();

        // 2. Ingresos del último mes (Nuevas Métricas Solicitadas)
        var haceUnMes = DateTime.Now.AddMonths(-1);
        dto.IngresosUltimoMes = await _db.RegistroAccesos
            .Where(r => r.FechaAcceso >= haceUnMes && r.TipoAccion == "Login")
            .CountAsync();

        // 3. Tiempo Promedio de Sesión
        // Excluimos nulos o duraciones irreales (mayores a 24 horas = 86400 seg)
        var tiempos = await _db.RegistroAccesos
            .Where(r => r.DuracionSesion != null && r.DuracionSesion > 0 && r.DuracionSesion < 86400)
            .Select(r => r.DuracionSesion.Value)
            .ToListAsync();

        if (tiempos.Any())
        {
            double promedioSegundos = tiempos.Average();
            dto.TiempoPromedioSesionMinutos = Math.Round(promedioSegundos / 60.0, 1);
        }

        // 4. Navegadores más usados
        dto.NavegadoresUsados = await _db.RegistroAccesos
            .Where(r => !string.IsNullOrEmpty(r.Navegador))
            .GroupBy(r => r.Navegador)
            .Select(g => new ChartDataDto {
                Label = g.Key.Length > 20 ? g.Key.Substring(0, 20) + "..." : g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .Take(5)
            .ToListAsync();

        // 5. Páginas más visitadas
        dto.PaginasMasVisitadas = await _db.RegistroAccesos
            .Where(r => r.TipoAccion == "PageView" && !string.IsNullOrEmpty(r.PaginaVisitada))
            .GroupBy(r => r.PaginaVisitada)
            .Select(g => new ChartDataDto {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .Take(5)
            .ToListAsync();

        // 6. Demografía: Usuarios por Ubicación (Ciudades)
        dto.UsuariosPorUbicacion = await _db.Usuarios
            .Where(u => !string.IsNullOrEmpty(u.Ubicacion))
            .GroupBy(u => u.Ubicacion)
            .Select(g => new ChartDataDto {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .Take(6)
            .ToListAsync();

        // 7. Demografía: Usuarios por Rol (Tipo de Cliente)
        dto.UsuariosPorRol = await _db.Usuarios
            .Where(u => !string.IsNullOrEmpty(u.Rol))
            .GroupBy(u => u.Rol)
            .Select(g => new ChartDataDto {
                Label = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        // 8. Accesos de los últimos 7 días (Línea de tiempo)
        var hace7dias = DateTime.Today.AddDays(-6);
        var accesosPorDia = await _db.RegistroAccesos
            .Where(r => r.FechaAcceso >= hace7dias && r.TipoAccion == "Login")
            .ToListAsync();

        var agrupados = accesosPorDia
            .GroupBy(r => r.FechaAcceso.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        dto.AccesosPorDia = Enumerable.Range(0, 7)
            .Select(i => hace7dias.AddDays(i))
            .Select(fecha => new ChartDataDto {
                Label = fecha.ToString("dd/MM"),
                Count = agrupados.ContainsKey(fecha) ? agrupados[fecha] : 0
            }).ToList();

        // 9. Dispositivos por Tipo (Para mantener un KPI importante del negocio)
        dto.DispositivosPorTipo = await _db.Dispositivos
            .GroupBy(d => d.Tipo)
            .Select(g => new ChartDataDto {
                Label = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // 10. Listas Rápidas (Tablas en pantalla)
        var ultimos = await _db.RegistroAccesos
            .Include(r => r.IdUsuariosNavigation)
            .OrderByDescending(r => r.FechaAcceso)
            .Take(8)
            .ToListAsync();

        dto.UltimosAccesos = ultimos.Select(r => new RegistroAccesoResumenDto {
            NombreUsuario = r.IdUsuariosNavigation?.Nombre ?? "N/A",
            Fecha = r.FechaAcceso,
            TipoAccion = r.TipoAccion,
            Ip = r.DireccionIp ?? "—"
        }).ToList();

        dto.UltimosSoportes = await _db.Soportes
            .Include(s => s.IdUsuariosNavigation)
            .OrderByDescending(s => s.Fecha)
            .Take(5)
            .ToListAsync();

        return dto;
    }
}
