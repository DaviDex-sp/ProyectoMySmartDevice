using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProyectoMSD.Modelos;

var builder = WebApplication.CreateBuilder(args);

// ELIMINAMOS el UseUrls("http://0.0.0.0:5000") porque rompe Azure. 
// En local, Visual Studio abrirá el puerto correcto automáticamente.

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

// Configurar el DbContext usando la versión compatible con TiDB (MySQL 8.0) y tolerancia a fallos
var connString = builder.Configuration.GetConnectionString("ConexionSQL");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connString,
        ServerVersion.Parse("8.0-mysql"),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // Intentará conectarse 5 veces antes de rendirse
            maxRetryDelay: TimeSpan.FromSeconds(10), // Esperará hasta 10 segundos entre intentos
            errorNumbersToAdd: null)
    )
);

var app = builder.Build();

// Proteger la base de datos en el arranque para que la web no colapse si Ngrok o MySQL demoran
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error de DB en el arranque: " + ex.Message);
    }
}

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