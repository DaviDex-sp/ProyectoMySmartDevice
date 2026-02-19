using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Configurar para escuchar en todas las interfaces de red en el puerto 5000
//builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Index";
        options.AccessDeniedPath = "/Denegada";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.ClaimsIssuer = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Cookie.Name = "MySmartDeviceCookie";
    });

// Configurar el DbContext con MySQL
var connString = builder.Configuration.GetConnectionString("ConexionSQL");
var serverVersion = ServerVersion.AutoDetect(connString);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connString,
        serverVersion)
);

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
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