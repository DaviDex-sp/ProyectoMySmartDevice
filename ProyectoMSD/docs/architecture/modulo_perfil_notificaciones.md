# Plano Técnico Arquitectónico: Módulo de Perfil y Notificaciones

## A. Diagnóstico de la Arquitectura Actual
Actualmente existe un despliegue de notificaciones usando Action Filters (`NotificacionNavbarFilter.cs`) en el cual el `AppDbContext` se lee recursivamente.

**Problemas detectados:**
- **Deuda Técnica en el Filter:** El filtro extrae datos directamente usando EF. Idealmente, los filtros deberían valerse de servicios para no conocer la estructura de la base de datos.
- **Responsabilidad Mezclada en Perfil:** La vista de perfil y su lógica de negocio de edición actual se está construyendo como lógica Razor sin control subyacente de reglas (como validaciones complejas de duplicación de teléfonos o correos contra *otros* usuarios).

## B. Estructura de Carpetas Propuesta

```text
/ProyectoMSD/Interfaces/IPerfilService.cs
/ProyectoMSD/Interfaces/INotificacionService.cs
/ProyectoMSD/Services/PerfilService.cs
/ProyectoMSD/Services/NotificacionService.cs
```

## C. Contratos (Interfaces)

### `IPerfilService.cs`
A diferencia de `IUsuarioService` (que controla Auth y CRUD administrativo), el *PerfilService* encapsula las capacidades de lo que un Huesped/Usuario ordinario puede tocar sobre sí mismo.
```csharp
using ProyectoMSD.Modelos;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IPerfilService
    {
        Task<Usuario?> GetPerfilByUserIdAsync(int userId);
        Task<bool> UpdatePerfilAsync(Usuario usuario);
        Task<bool> UpdatePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
```

### `INotificacionService.cs`
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface INotificacionService
    {
        Task<List<Notificacion>> GetNotificacionesByUserIdAsync(int userId, int limit = 5);
        Task<int> GetCountNoLeidasAsync(int userId);
        Task MarcarComoLeidaAsync(int notificacionId);
        Task EnviarNotificacionSistemaAsync(int userId, string titulo, string mensaje, string tipo, string rutaRecomendada);
    }
}
```

## D. Registro en Pipeline (`Program.cs`)

```csharp
// Registrar la capa de Servicios del Módulo de Perfil y Notificaciones
builder.Services.AddScoped<ProyectoMSD.Interfaces.IPerfilService, ProyectoMSD.Services.PerfilService>();
builder.Services.AddScoped<ProyectoMSD.Interfaces.INotificacionService, ProyectoMSD.Services.NotificacionService>();
```

## 5. Decisiones Arquitectónicas y Trade-offs (Tech Lead Feedback)
**Disyuntiva:** ¿Refactorizamos el `NotificacionNavbarFilter` a Middleware puro, o lo dejamos como ServiceFilter consumiendo interfaces?

- **Rendimiento:** Un Middleware se ejecuta en cada Request (CSS, JS incluido a menos que se acote), lo cual penalizaría la BD inmensamente midiendo `CountNoLeidas`. Un `ServiceFilter` inyectado por convenciones de Razor Pages solo se ejecuta al solicitar páginas HTML completas.
- **Mantenibilidad:** El ServiceFilter actual es mucho más limpio para escenarios MVC/Razor debido a su gancho natural con el `ViewData/ViewBag`.
- **Complejidad:** El Middleware requeriría una inyección dependiente del HttpContext para inyectar en ViewData, muy aparatoso de acoplar en Razor.

**Dictamen:** Modificaremos el `NotificacionNavbarFilter` actual, que usará la dependencia `INotificacionService` en lugar de `AppDbContext`. Se continuará inyectando mediante convención `options.Conventions.ConfigureFilter()` estrictamente de la UI.
