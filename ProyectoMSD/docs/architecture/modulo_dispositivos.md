# Plano Técnico Arquitectónico: Módulo de Dispositivos y Soportes

## A. Diagnóstico de la Arquitectura Actual
Actualmente, las páginas de Razor (`Pages/Dispositivos` y `Pages/Soportes`) y la base de datos están vinculadas de manera fuertemente acoplada en el code-behind.

**Problemas detectados:**
- **Acoplamiento Fuerte:** La capa de presentación se ve obligada a comprender el modelo de datos (`Entity Framework Core`).
- **Riesgos de Seguridad:** Exponer información completa de la topología de base de datos a un nivel tan alto como la capa web.
- **Dificultad de Testing:** No se puede inyectar un "Mock" de la base de datos fácilmente durante las pruebas aisladas, imposibilitando los Unit Tests eficientes.

## B. Estructura de Carpetas Propuesta

```text
/ProyectoMSD/Interfaces/IDispositivoService.cs
/ProyectoMSD/Interfaces/ISoporteService.cs
/ProyectoMSD/Services/DispositivoService.cs
/ProyectoMSD/Services/SoporteService.cs
```

## C. Contratos (Interfaces)

### `IDispositivoService.cs`
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IDispositivoService
    {
        Task<List<Dispositivo>> GetAllDispositivosAsync();
        Task<Dispositivo?> GetDispositivoByIdAsync(int id);
        
        // Relacional
        Task<List<Almacenan>> GetUbicacionesDispositivoAsync(int dispositivoId);

        Task CreateDispositivoAsync(Dispositivo dispositivo);
        Task UpdateDispositivoAsync(Dispositivo dispositivo);
        Task DeleteDispositivoAsync(int id);
    }
}
```

### `ISoporteService.cs`
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface ISoporteService
    {
        Task<List<Soporte>> GetAllSoportesAsync();
        Task<Soporte?> GetSoporteByIdAsync(int id);
        Task<List<Soporte>> GetSoportesByUserIdAsync(int userId);
        Task CreateSoporteAsync(Soporte soporte);
        Task UpdateSoporteAsync(Soporte soporte);
        Task DeleteSoporteAsync(int id);
    }
}
```

## D. Registro en Pipeline (`Program.cs`)

```csharp
// Registrar la capa de Servicios del Módulo de Dispositivos y Soportes
builder.Services.AddScoped<ProyectoMSD.Interfaces.IDispositivoService, ProyectoMSD.Services.DispositivoService>();
builder.Services.AddScoped<ProyectoMSD.Interfaces.ISoporteService, ProyectoMSD.Services.SoporteService>();
```

## 5. Decisiones Arquitectónicas y Trade-offs (Tech Lead Feedback)
**Disyuntiva:** ¿Crear un repositorio genérico `IRepository<T>` o servicios específicos (`IDispositivoService`)?

- **Rendimiento:** Idéntico (EF Core usa internamente DbSets genéricos y específicos).
- **Mantenibilidad:** Un Repositorio Genérico puede sonar "Limpio", pero en .NET, `DbContext` ya de por sí implementa el patrón Unit of Work y Repository. Agregarle uno propio a menudo resulta en el anti-patrón "Generic Repository over Entity Framework DbContext".
- **Complejidad:** Los servicios (Services) específicos pueden contener Lógica de Negocio (ej., validaciones de dispositivos únicos, revisión de tickets de soporte repetidos). Un repositorio puro genérico no debería tener lógica de validación de negocio.

**Dictamen:** Usaremos **Service Pattern Específico** en capa BLL (Business Logic Layer). No inyectaremos repositorios genéricos sobre EF Core, inyectaremos directamente `AppDbContext` en los respectivos `Services` (`DispositivoService`, `SoporteService`), aislando así la persistencia de las vistas Razor.
