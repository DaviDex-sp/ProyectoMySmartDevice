# Plano Técnico Arquitectónico: Módulo Administrativo (Dashboard, Configuraciones, Almacenan)

## A. Diagnóstico de la Arquitectura Actual
Este módulo realiza consultas transaccionales críticas, como contar el total de usuarios, espacios y dispositivos del sistema (`Index.cshtml` del Dashboard general), además de interactuar con la tabla pivote de `Almacenan` (Dispositivos en Espacios) y `Configuraciones` del usuario. 

**Problemas detectados:**
- **Pérdida de Responsabilidad:** El `DashboardModel` consulta las 3 tablas pricipales sin abstracción, lo que lo hace vulnerable a cambios repentinos de Schema si una tabla cambia de nombre.
- **Consultas Pesadas:** La lógica de "Almacenan" (M:N entre Dispositivos y Espacios) podría volverse lenta si se ejecuta filtrado directamente en memoria. 
- **Reglas de Negocio en OnGet:** Las estadísticas vitales de la plataforma (como contar Usuarios Activos vs inactivos) residen en el PageModel. 

## B. Estructura de Carpetas Propuesta

```text
/ProyectoMSD/Interfaces/IDashboardConfigService.cs
/ProyectoMSD/Interfaces/IAlmacenajeService.cs
/ProyectoMSD/Services/DashboardConfigService.cs
/ProyectoMSD/Services/AlmacenajeService.cs
```

## C. Contratos (Interfaces)

### `IDashboardConfigService.cs`
Agrupa las métricas del sistema y las opciones de configuración del usuario que alimenta la carga inicial de un administrador.
```csharp
using ProyectoMSD.Modelos;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IDashboardConfigService
    {
        // Estadísticas Críticas
        Task<int> GetTotalUsuariosAsync();
        Task<int> GetTotalDispositivosAsync();
        Task<int> GetTotalEspaciosAsync();
        Task<int> GetTotalSoportesPendientesAsync();

        // Configuraciones Personales
        Task<Configuracione?> GetConfiguracionByUserIdAsync(int userId);
        Task UpdateConfiguracionAsync(Configuracione config);
    }
}
```

### `IAlmacenajeService.cs`
Maneja estrictamente la relación M:N de qué dispositivo reside en qué espacio (la tabla `Almacenan`).
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IAlmacenajeService
    {
        Task<List<Almacenan>> GetAllRelacionesAsync();
        Task<List<Almacenan>> GetRelacionesByEspacioIdAsync(int espacioId);
        Task<List<Almacenan>> GetRelacionesByDispositivoIdAsync(int dispositivoId);
        Task AddDispositivoToEspacioAsync(int dispositivoId, int espacioId);
        Task RemoveDispositivoFromEspacioAsync(int idRelacion);
    }
}
```

## D. Registro en Pipeline (`Program.cs`)

```csharp
// Registrar la capa de Servicios del Módulo Administrativo
builder.Services.AddScoped<ProyectoMSD.Interfaces.IDashboardConfigService, ProyectoMSD.Services.DashboardConfigService>();
builder.Services.AddScoped<ProyectoMSD.Interfaces.IAlmacenajeService, ProyectoMSD.Services.AlmacenajeService>();
```

## 5. Decisiones Arquitectónicas y Trade-offs (Tech Lead Feedback)
**Disyuntiva:** ¿Por qué crear un `DashboardConfigService` si puedo inyectar `IUsuarioService`, `IDispositivoService` y `IEspacioService` directamente en el DashboardModel para obtener sus totales?

- **Rendimiento:** Inyectar múltiples servicios en un controlador que a su vez manejan un `AppDbContext` cada uno es viable gracias a `Scoped`, pero implica un grafo de inyección pesado para solo contar (`Count()`) registros.
- **Mantenibilidad:** Altamente recomendable tener una Clase `DashboardService` que maneje consultas de Agregación pesadas mediante SQL (`GroupBy`, `Count`), aislándolas de los servicios base (CRUDs).
- **Complejidad:** Un componente de Dashboard usualmente muta para requerir Reportes (PDF, Excel) y gráficas. Agruparlo en un servicio independiente promueve la separación de responsabilidades.

**Dictamen:** Se prohíbe el encadenamiento (`chaining`) de servicios core en el Dashboard. Validamos **crear el `IDashboardConfigService`** que ataque la BD (`AppDbContext`) únicamente buscando proyecciones ligeras (`.Select`, `.Count`) en lugar de arrastrar las Entidades Completas (`.ToListAsync`) que consumirían enorme memoria RAM.
