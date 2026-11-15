using System.Security.Claims;
using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class CuentaController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly PasswordHasher<Usuario> hasher;
    public CuentaController(CalisTrackContext db, PasswordHasher<Usuario> hasher) { this.db = db; this.hasher = hasher; }

    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Inicio");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }
    [AllowAnonymous, HttpPost, EnableRateLimiting("acceso")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = (model.Email ?? "").Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (usuario == null || !usuario.Activo || hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Email o contraseña incorrectos, o cuenta desactivada.");
            return View(model);
        }
        await Ingresar(usuario);
        return model.ReturnUrl != null && Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl) : RedirectToAction("Index", "Inicio");
    }
    [AllowAnonymous, HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [AllowAnonymous, HttpPost, EnableRateLimiting("acceso")]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == model.Email)) ModelState.AddModelError("Email", "Este email ya está registrado.");
        if (!ModelState.IsValid) return View(model);
        Usuario usuario = model.Rol == Roles.Entrenador ? new Entrenador() : new Alumno { Objetivo = (model.Objetivo ?? "").Trim() };
        usuario.Nombre = model.Nombre.Trim();
        usuario.Email = model.Email;
        usuario.PasswordHash = hasher.HashPassword(usuario, model.Password);
        db.Usuarios.Add(usuario);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("Email", "No se pudo registrar ese email. Probá con otro."); return View(model); }
        await Ingresar(usuario);
        Exito(model.Rol == Roles.Entrenador ? "Tu cuenta está lista. Ya podés asociar alumnos y crear rutinas." : "Tu cuenta está lista. Un entrenador puede vincularte desde Mis alumnos.");
        return RedirectToAction("Index", "Inicio");
    }
    [HttpPost]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var u = await db.Usuarios.FindAsync(UsuarioId);
        return View(new PerfilViewModel { Nombre = u!.Nombre, Email = u.Email, Objetivo = (u as Alumno)?.Objetivo ?? "" });
    }
    [HttpPost]
    public async Task<IActionResult> Perfil(PerfilViewModel model)
    {
        var u = (await db.Usuarios.FindAsync(UsuarioId))!;
        var email = (model.Email ?? "").Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.Email == email && x.Id != UsuarioId)) ModelState.AddModelError("Email", "Este email ya está registrado.");
        if ((!string.IsNullOrEmpty(model.PasswordNueva) || email != u.Email) && (string.IsNullOrEmpty(model.PasswordActual) || hasher.VerifyHashedPassword(u, u.PasswordHash, model.PasswordActual) == PasswordVerificationResult.Failed))
            ModelState.AddModelError("PasswordActual", "Ingresá tu contraseña actual para cambiar el email o la contraseña.");
        if (!ModelState.IsValid) return View(model);
        u.ActualizarPerfil(model.Nombre.Trim(), email);
        if (u is Alumno alumno) alumno.Objetivo = (model.Objetivo ?? "").Trim();
        if (!string.IsNullOrEmpty(model.PasswordNueva)) { u.PasswordHash = hasher.HashPassword(u, model.PasswordNueva); u.VersionSesion = Guid.NewGuid(); }
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError("Email", "No se pudo guardar ese email."); return View(model); }
        await Ingresar(u);
        Exito("Perfil actualizado.");
        return RedirectToAction(nameof(Perfil));
    }
    [AllowAnonymous]
    public IActionResult AccesoDenegado() { Response.StatusCode = 403; return View(); }

    private Task Ingresar(Usuario u)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()), new Claim(ClaimTypes.Name, u.Nombre), new Claim(ClaimTypes.Role, u.Rol), new Claim("VersionSesion", u.VersionSesion.ToString()) };
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}
