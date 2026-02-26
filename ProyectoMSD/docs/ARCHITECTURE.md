# 🏗️ Arquitectura CSS — ProyectoMSD

## Estructura de Archivos CSS

```
wwwroot/css/
├── common-styles.css          ← Estilos compartidos (variables, base, componentes)
├── site.css                   ← Estilos globales del framework ASP.NET
└── pages/
    ├── login.css              ← Página de inicio de sesión
    ├── registrar.css          ← Página de registro
    ├── privacy.css            ← Política de privacidad
    ├── usuarios.css           ← Módulo de usuarios
    ├── dispositivos.css       ← Módulo de dispositivos
    ├── espacios.css           ← Módulo de espacios
    ├── propiedades.css        ← Módulo de propiedades
    ├── soportes.css           ← Módulo de soporte
    └── configuraciones.css    ← Módulo de configuraciones
```

## Cómo Funciona

### 1. `_Layout.cshtml` — Carga CSS Global
Carga `common-styles.css` para **todas** las páginas y define un bloque `@RenderSection("Styles", required: false)` donde cada página inyecta su CSS específico.

### 2. Páginas Individuales — CSS Específico
Cada `.cshtml` usa `@section Styles` para cargar su archivo CSS:

```cshtml
@section Styles {
    <link href="~/css/pages/dispositivos.css" rel="stylesheet" />
}
```

### 3. `common-styles.css` — Componentes Compartidos
Contiene:
- **Variables CSS** (`--primary-blue`, `--secondary-blue`, etc.)
- **Estilos base** (body, tipografía, gradientes de fondo)
- **Componentes reutilizables**: navbar, tablas, formularios, botones, tarjetas, badges, modales, avatares, alertas
- **Responsive** breakpoints

## Variables CSS Principales

| Variable | Valor | Uso |
|----------|-------|-----|
| `--primary-blue` | `#3b82f6` | Color principal |
| `--secondary-blue` | `#60a5fa` | Acento secundario |
| `--light-blue` | `#dbeafe` | Fondos suaves |
| `--text-dark` | `#1e293b` | Texto principal |
| `--text-light` | `#64748b` | Texto secundario |

## Agregar una Nueva Página

1. Crear `wwwroot/css/pages/nueva-pagina.css`
2. En el `.cshtml`, agregar:
   ```cshtml
   @section Styles {
       <link href="~/css/pages/nueva-pagina.css" rel="stylesheet" />
   }
   ```
3. Usar las clases de `common-styles.css` para componentes compartidos
4. Definir estilos únicos en el archivo de la página

## Funciones Clave del Code-Behind

### `OnGetAsync()` — Carga de Datos
Método principal en cada `PageModel` que carga los datos desde la base de datos.

```csharp
// Ejemplo: Pages/Dispositivos/Index.cshtml.cs
public async Task OnGetAsync()
{
    // Carga todos los dispositivos desde el contexto EF
    Dispositivo = await _context.Dispositivos.ToListAsync();
}
```

### `OnPostAsync()` — Procesamiento de Formularios
Maneja las solicitudes POST (crear, editar, eliminar).

```csharp
// Ejemplo: Pages/Dispositivos/Edit.cshtml.cs
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();
    
    _context.Attach(Dispositivo).State = EntityState.Modified;
    await _context.SaveChangesAsync();
    return RedirectToPage("./Index");
}
```

### Autenticación y Roles
El proyecto usa `User.IsInRole("Admin")` para controlar el acceso a acciones CRUD.

```cshtml
@if (User.IsInRole("Admin"))
{
    <a asp-page="./Edit" asp-route-id="@item.Id">Editar</a>
    <a asp-page="./Delete" asp-route-id="@item.Id">Eliminar</a>
}
```
