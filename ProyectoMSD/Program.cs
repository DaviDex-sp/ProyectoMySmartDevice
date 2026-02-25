using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Servicios de Razor Pages
builder.Services.AddRazorPages();

// Configuración de Autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";
        options.AccessDeniedPath = "/Denegada";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.ClaimsIssuer = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Cookie.Name = "MySmartDeviceCookie";
    });

// 1. LEEMOS LA CADENA DESDE LA CONFIGURACIÓN (Seguridad Profesional)
// En local leerá el appsettings.json. En la nube leerá la variable de Azure.
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
    // Muestra errores detallados solo cuando estás desarrollando en tu PC
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