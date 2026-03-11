using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoMSD.Interfaces;
using ProyectoMSD.Modelos.DTOs;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProyectoMSD.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardMetricsDto Metrics { get; set; } = new();

        // JSON para Chart.js
        public string NavegadoresJson { get; set; } = "{}";
        public string PaginasJson { get; set; } = "{}";
        public string UbicacionJson { get; set; } = "{}";
        public string RolJson { get; set; } = "{}";
        public string AccesosDiaJson { get; set; } = "{}";
        public string DispositivosTipoJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            Metrics = await _dashboardService.GetMetricsAsync();

            // Serialización para Gráficos
            NavegadoresJson = JsonSerializer.Serialize(new {
                labels = Metrics.NavegadoresUsados.Select(x => x.Label),
                data = Metrics.NavegadoresUsados.Select(x => x.Count)
            });

            PaginasJson = JsonSerializer.Serialize(new {
                labels = Metrics.PaginasMasVisitadas.Select(x => x.Label),
                data = Metrics.PaginasMasVisitadas.Select(x => x.Count)
            });

            UbicacionJson = JsonSerializer.Serialize(new {
                labels = Metrics.UsuariosPorUbicacion.Select(x => x.Label),
                data = Metrics.UsuariosPorUbicacion.Select(x => x.Count)
            });

            RolJson = JsonSerializer.Serialize(new {
                labels = Metrics.UsuariosPorRol.Select(x => x.Label),
                data = Metrics.UsuariosPorRol.Select(x => x.Count)
            });

            AccesosDiaJson = JsonSerializer.Serialize(new {
                labels = Metrics.AccesosPorDia.Select(x => x.Label),
                data = Metrics.AccesosPorDia.Select(x => x.Count)
            });

            DispositivosTipoJson = JsonSerializer.Serialize(new {
                labels = Metrics.DispositivosPorTipo.Select(x => x.Label),
                data = Metrics.DispositivosPorTipo.Select(x => x.Count)
            });
        }
    }
}
