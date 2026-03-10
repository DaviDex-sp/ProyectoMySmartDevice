# Plano Técnico Arquitectónico: Módulo de Inmuebles (Propiedades y Espacios)

## A. Diagnóstico de la Arquitectura Actual
Actualmente, las páginas de Razor (`Pages/Propiedades` y `Pages/Espacios`) operan bajo un modelo de acoplamiento fuerte donde `AppDbContext` está inyectado directamente en los `PageModels`. 

**Problemas detectados:**
- **Anti-Smart UI:** Las reglas de negocio (creación de jerarquías entre Propiedades y Espacios, validación de permisos de espacios y señales) residen en el código base (UI).
- **Falta de Reusabilidad:** Si una API o proceso en background necesita listar o crear un Espacio, se duplicaría la lógica.
- **Consultas Ineficientes:** Peligro de realizar consultas `N+1` directamente desde las vistas Razor en caso de intentar acceder a datos relacionales (ej. dispositivos asociados).

## B. Estructura de Carpetas Propuesta
La refactorización ubicará los contratos e implementaciones en las carpetas base del proyecto para consolidar el patrón de N-Capas:

```text
/ProyectoMSD/Interfaces/IPropiedadService.cs
/ProyectoMSD/Interfaces/IEspacioService.cs
/ProyectoMSD/Services/PropiedadService.cs
/ProyectoMSD/Services/EspacioService.cs
```

*(Opcional a futuro: agregar la carpeta `/DTOs` si las vistas requieren proyecciones complejas en lugar de las entidades crudas de DB).*

## C. Contratos (Interfaces)

### `IPropiedadService.cs`
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IPropiedadService
    {
        Task<List<Propiedade>> GetAllPropiedadesAsync();
        Task<Propiedade?> GetPropiedadByIdAsync(int id);
        Task<List<Propiedade>> GetPropiedadesByUserIdAsync(int userId);
        Task CreatePropiedadAsync(Propiedade propiedad);
        Task UpdatePropiedadAsync(Propiedade propiedad);
        Task DeletePropiedadAsync(int id);
    }
}
```

### `IEspacioService.cs`
```csharp
using ProyectoMSD.Modelos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoMSD.Interfaces
{
    public interface IEspacioService
    {
        Task<List<Espacio>> GetAllEspaciosAsync();
        Task<List<Espacio>> GetEspaciosByPropiedadIdAsync(int propiedadId);
        Task<Espacio?> GetEspacioByIdAsync(int id);
        Task CreateEspacioAsync(Espacio espacio);
        Task UpdateEspacioAsync(Espacio espacio);
        Task DeleteEspacioAsync(int id);
    }
}
```

## D. Registro en Pipeline (`Program.cs`)
Para que los controladores inyecten estas interfaces, es necesario darlas de alta en el contenedor nativo de inyección de dependencias con ciclo de vida `Scoped`, ya que estas interactúan con el Contexto de BD.

```csharp
// Registrar la capa de Servicios del Módulo de Inmuebles
builder.Services.AddScoped<ProyectoMSD.Interfaces.IPropiedadService, ProyectoMSD.Services.PropiedadService>();
builder.Services.AddScoped<ProyectoMSD.Interfaces.IEspacioService, ProyectoMSD.Services.EspacioService>();
```

## 5. Decisiones Arquitectónicas y Trade-offs (Tech Lead Feedback)
**Disyuntiva:** ¿Se deben fusionar `PropiedadService` y `EspacioService` en un único `InmuebleService`?

- **Rendimiento:** Neutro. El compilador y DI manejan ambas opciones con latencia prácticamente invisible.
- **Mantenibilidad:** Separados (IPropiedadService e IEspacioService) violan menos el Principio de Responsabilidad Única (SRP). Fusionados crean un servicio monolítico («God object»).
- **Complejidad:** Mantenerlos separados genera ligeramente más inyecciones, pero aísla los fallos.

**Dictamen:** Mantendremos servicios **SEPARADOS** (`IPropiedadService` e `IEspacioService`). Si se requiere una operación transaccional entre ambas (ej. crear una propiedad con sus 3 espacios defectuosos de un solo golpe), se puede manejar a futuro mediante un patrón `Facade` o manejando la transacción dentro del servicio específico, conservando el alcance `Scoped` compartido por el `AppDbContext`.
