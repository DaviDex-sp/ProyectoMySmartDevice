using System.Collections.Generic;

namespace ProyectoMSD.Modelos.DTOs;

public class DashboardMetricsDto
{
    // KPIs Generales
    public int TotalUsuarios { get; set; }
    public int TotalDispositivos { get; set; }
    public int TotalPropiedades { get; set; }
    public int TotalEspacios { get; set; }
    public int TotalSoportes { get; set; }
    public int SoportesPendientes { get; set; }
    public int TotalConfiguraciones { get; set; }
    public int IngresosUltimoMes { get; set; }
    public double TiempoPromedioSesionMinutos { get; set; }

    // Listas para Chart.js
    public List<ChartDataDto> NavegadoresUsados { get; set; } = new();
    public List<ChartDataDto> PaginasMasVisitadas { get; set; } = new();
    public List<ChartDataDto> UsuariosPorUbicacion { get; set; } = new(); // Demografía geográfica
    public List<ChartDataDto> UsuariosPorRol { get; set; } = new(); // Demografía por tipo de cliente
    public List<ChartDataDto> AccesosPorDia { get; set; } = new();
    public List<ChartDataDto> DispositivosPorTipo { get; set; } = new();

    // Tablas de acceso rápido
    public List<RegistroAccesoResumenDto> UltimosAccesos { get; set; } = new();
    public List<Soporte> UltimosSoportes { get; set; } = new();
}

public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RegistroAccesoResumenDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string TipoAccion { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
}
