# 🏗️ Arquitectura del Proyecto — mySmartDevice

mySmartDevice está construido sobre **ASP.NET Core Razor Pages** utilizando Entity Framework Core para el acceso a datos. La arquitectura sigue un patrón orientado a Páginas (similar a MVVM - Model-View-ViewModel) que agrupa la vista HTML y su lógica C# en un mismo lugar, ideal para aplicaciones web escalables y modulares.

---

## 📁 Estructura Principal de Archivos

```
ProyectoMSD/
│
├── 📂 Modelos/               ← (Capa de Datos - Model)
│   ├── AppDbContext.cs       ← Configuración de Entity Framework (conexión a MySQL)
│   ├── Usuario.cs            ← Entidades/Tablas de la Base de Datos
│   ├── Dispositivo.cs
│   └── Notificacion.cs
│
├── 📂 Pages/                 ← (Capa de Presentación y Lógica - View & ViewModel)
│   ├── 📂 Shared/            ← Vistas compartidas
│   │   ├── _Layout.cshtml    ← Navbar, Footer, estructura base de HTML
│   │   └── _Validation...    ← Scripts de validación
│   │
│   ├── 📂 Modulos (Ej: Soportes, Dispositivos, Perfil)
│   │   ├── Index.cshtml      ← Archivo HTML de la Vista (Razor)
│   │   └── Index.cshtml.cs    ← Code-Behind (Lógica de servidor en C#)
│   │
│   ├── _ViewImports.cshtml   ← Importación global de namespaces
│   └── _ViewStart.cshtml     ← Define el _Layout global
│
├── 📂 Filters/               ← (Middleware - Interceptores)
│   └── NotificacionNavbarFilter.cs  ← Lógica que se ejecuta ANTES de cargar las páginas
│
├── 📂 wwwroot/               ← (Archivos Estáticos y Assets Públicos)
│   ├── 📂 css/               ← Estilos (Ver Arquitectura CSS abajo)
│   ├── 📂 js/                ← Scripts Frontend compartidos
│   ├── 📂 images/            ← Logotipos y recursos gráficos fijos
│   └── 📂 uploads/           ← Archivos subidos por los usuarios (Ej. Avatares)
│
├── 📂 docs/                  ← Documentación técnica del proyecto
│
├── appsettings.json          ← Configuración (Cadenas de conexión, Secretos)
└── Program.cs                ← Punto de entrada: Inyección de Dependencias, Pipeline, Routing
```

---

## 🛠️ Cómo Funciona la Arquitectura (Razor Pages)

El patrón **Razor Pages** agrupa todo lo que necesita una página en una carpeta. Esto hace que escalar el proyecto sea muy limpio, ya que en lugar de tener controladores gigantes y vistas separadas, cada funcionalidad está encapsulada.

### Ejemplo de Vida de una Petición (Ej. Crear un Ticket)
1. **El Usuario visita `/Soportes/Create`:**
   - El servidor carga primero `_ViewStart.cshtml`, que envuelve el contenido en `_Layout.cshtml`.
   - Se ejecuta el `NotificacionNavbarFilter.cs` para inyectar la campana en el Navbar.
   - ASP.NET ejecuta el backend en `Create.cshtml.cs` (`OnGetAsync()`).
   - El servidor devuelve el HTML renderizado de `Create.cshtml`.

2. **El Usuario envía el formulario:**
   - La petición llega al método `OnPostAsync()` de `Create.cshtml.cs`.
   - El código de C# valida los datos y usa el `AppDbContext` (capa de `Modelos/`) para guardar en la base de datos (MySQL).
   - Se genera una Notificación.
   - Si tiene éxito, devuelve un redireccionamiento (`RedirectToPage`).

---

## 🎨 Arquitectura CSS

Para evitar un archivo CSS gigante que afecte el rendimiento, el proyecto utiliza un enfoque modular basado en **Componentes Globales + CSS Cargar Bajo Demanda**.

```
wwwroot/css/
├── common-styles.css          ← Variables, colores, botones, navbar (Carga SIEMPRE)
├── site.css                   ← Defaults limpios
└── pages/                     ← CSS exclusivo de cada página (Carga SOLO cuando es necesario)
    ├── login.css
    ├── soportes.css
    └── notificaciones.css
```

### Reglas para Escalar el CSS:
1. **Componentes repetitivos:** Si vas a crear un botón o una tarjeta que se verá igual en 5 páginas, pon la clase en `common-styles.css`. Usa las variables globales (`--primary-blue`).
2. **Estilos únicos:** Si vas a modificar el layout de una gráfica que solo existe en `Estadisticas.cshtml`, crea un archivo `wwwroot/css/pages/estadisticas.css` y cárgalo en la vista usando:
   ```cshtml
   @section Styles {
       <link href="~/css/pages/estadisticas.css" rel="stylesheet" />
   }
   ```

---

## 📡 Patrones de Comunicación Global

Para que el proyecto crezca sin ensuciar el código, se han implementado patrones estandarizados:

### 1. Sistema Pasivo de Notificaciones (Desacoplamiento)
En lugar de que una página de "Dispositivos" se encargue de lidiar con la UI de notificaciones, usamos la base de datos como puente.

**Al crear o modificar datos importantes, solo invoca esto:**
```csharp
_context.Notificaciones.Add(new Notificacion {
    IdUsuarios = usarioDestino.Id,
    Titulo = "Alerta del Sistema",
    Mensaje = "Operación exitosa",
    Tipo = "Success",
    RutaRedireccion = $"/Modulo/Accion/{{id}}" // <-- Te permite navegar al hacer click
});
// ¡Y listo! El usuario verá la campanita titilar gracias al Filtro Global.
```

### 2. Filtros de Página (`NotificacionNavbarFilter.cs`)
Consultar cosas globales (como saber cuántas notificaciones no leídas tienes) en CADA página destruiría el rendimiento o te obligaría a hacer copy/paste del código 100 veces. Usamos **Filtros Globales**.
Se registran en `Program.cs` y actúan como "Porteros". Cada vez que un usuario intenta cargar cualquier página, el Filtro "ataja" la petición, inyecta los datos en el `ViewData` y deja que el `_Layout.cshtml` los dibuje de forma transparente. ¡Tu código en los módulos C# se mantiene 100% libre y enfocado en su negocio!
