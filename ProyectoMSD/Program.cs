using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

var builder = WebApplication.CreateBuilder(args);

// Configurar para escuchar en todas las interfaces de red en el puerto 5000

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

var connString = builder.Configuration.GetConnectionString("ConexionSQL");

if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("DemoDatabase"));
}
else
{
    var serverVersion = ServerVersion.AutoDetect(connString);

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connString, serverVersion));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsProduction())
    {
        context.Database.EnsureCreated();

        if (!context.Usuarios.Any())
        {
            var adminUser = new Usuario
            {
                Nombre = "Administrador",
                Clave = "123456",
                Correo = "admin@demo.com",
                Rol = "Admin",
                Telefono = 300000000,
                Ubicacion = "Demo",
                Permisos = "Full",
                Documento = 123456789,
                Rut = "RUT-ADMIN",
                Acesso = "Total"
            };

            var normalUser = new Usuario
            {
                Nombre = "Usuario Demo",
                Clave = "123456",
                Correo = "user@demo.com",
                Rol = "User",
                Telefono = 300000001,
                Ubicacion = "Demo",
                Permisos = "Limited",
                Documento = 987654321,
                Rut = "RUT-USER",
                Acesso = "Parcial"
            };

            context.Usuarios.Add(adminUser);
            context.Usuarios.Add(normalUser);

            context.SaveChanges();
        }
    }
}


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