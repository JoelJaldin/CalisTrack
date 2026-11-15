using System.Security.Claims;
using System.Threading.RateLimiting;
using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false).AddEnvironmentVariables();
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddMvcOptions(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddDbContext<CalisTrackContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CalisTrack")));
builder.Services.AddScoped<PasswordHasher<Usuario>>();
builder.Services.AddScoped<AccesoService>();
builder.Services.AddScoped<ArchivoService>();
var almacenamiento = builder.Configuration["Storage:Root"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data");
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(almacenamiento, "keys")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.Cookie.Name = "CalisTrack.Sesion";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnValidatePrincipal = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<CalisTrackContext>();
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var version = context.Principal?.FindFirstValue("VersionSesion");
        var usuario = int.TryParse(id, out var userId) ? await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId) : null;
        if (usuario == null || !usuario.Activo || usuario.VersionSesion.ToString() != version || !context.Principal!.IsInRole(usuario.Rol))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("acceso", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 21 * 1024 * 1024);
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Inicio/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Inicio}/{action=Index}/{id?}");
if (!args.Contains("--sin-inicializar"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CalisTrackContext>();
    await db.Database.MigrateAsync();
    if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("SeedDemo"))
        await DatosIniciales.CargarAsync(db, scope.ServiceProvider.GetRequiredService<PasswordHasher<Usuario>>());
}
app.Run();
