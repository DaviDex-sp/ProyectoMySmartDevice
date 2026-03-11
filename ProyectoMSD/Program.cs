using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google; // Agregado para Google
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;
using ProyectoMSD.Filters;

var builder = WebApplication.CreateBuilder(args);

// Servicios de Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(NotificacionNavbarFilter)));
});
builder.Services.AddScoped<NotificacionNavbarFilter>();

// Registrar la capa de Servicios del Módulo de Usuarios
builder.Services.AddScoped<ProyectoMSD.Interfaces.IUsuarioService, ProyectoMSD.Services.UsuarioService>();

// Registrar la capa de Servicios del Dashboard (Métricas)
builder.Services.AddScoped<ProyectoMSD.Interfaces.IDashboardService, ProyectoMSD.Services.DashboardService>();

// Configuración de Autenticación Múltiple (Cookies locales + Google)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Denegada";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.ClaimsIssuer = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Cookie.Name = "MySmartDeviceCookie";
    })
    .AddCookie("ExternalCookie") // Cookie temporal para el inicio de sesión con Google
    .AddGoogle(googleOptions =>
    {
        googleOptions.SignInScheme = "ExternalCookie"; // Asignar al esquema temporal
        
        // Leemos las llaves de forma segura desde Azure (o appsettings local)
        googleOptions.ClientId = builder.Configuration["GoogleClientId"] ?? throw new InvalidOperationException("Falta GoogleClientId");
        googleOptions.ClientSecret = builder.Configuration["GoogleClientSecret"] ?? throw new InvalidOperationException("Falta GoogleClientSecret");
        
        // La ruta estándar donde Google nos devolverá al usuario
        googleOptions.CallbackPath = "/signin-google";
    });

// 1. LEEMOS LA CADENA DESDE LA CONFIGURACIÓN
var connString = builder.Configuration.GetConnectionString("ConexionSQL");

// 2. CONFIGURACIÓN DEL DBCONTEXT PARA MYSQL 8.0 (Aiven/Azure)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connString,
        ServerVersion.Parse("8.0-mysql"),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)
    )
);

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();