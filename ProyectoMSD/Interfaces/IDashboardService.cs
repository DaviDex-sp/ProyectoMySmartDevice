using ProyectoMSD.Modelos.DTOs;

namespace ProyectoMSD.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Obtiene todas las métricas procesadas para el Dashboard Administrativo.
    /// Resuelve el anti-patrón Smart UI evitando que las Razor Pages consulten a la BD directamente.
    /// </summary>
    /// <returns>DashboardMetricsDto con todos los KPIs, gráficos y tablas.</returns>
    Task<DashboardMetricsDto> GetMetricsAsync();
}
