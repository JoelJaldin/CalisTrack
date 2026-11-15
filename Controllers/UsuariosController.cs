using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

[Authorize(Roles = Roles.SuperAdmin)]
public class UsuariosController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<Usuario> hasher;
    public UsuariosController(CalisTrackContext db, Microsoft.AspNetCore.Identity.PasswordHasher<Usuario> hasher) { this.db = db; this.hasher = hasher; }
    public async Task<IActionResult> Index(string? buscar, string? rol, bool inactivos = false)
    {
        ViewBag.Buscar = buscar; ViewBag.RolFiltro = rol;
        ViewBag.InactivosFiltro = inactivos;
        ViewBag.Total = await db.Usuarios.CountAsync();
        ViewBag.Alumnos = await db.Usuarios.OfType<Alumno>().CountAsync();
        ViewBag.Entrenadores = await db.Usuarios.OfType<Entrenador>().CountAsync();
        ViewBag.Inactivos = await db.Usuarios.CountAsync(u => !u.Activo);
        var query = db.Usuarios.AsQueryable();
        if (inactivos) query = query.Where(u => !u.Activo);
        if (!string.IsNullOrWhiteSpace(buscar)) query = query.Where(u => u.Nombre.Contains(buscar) || u.Email.Contains(buscar));
        if (Roles.Todos.Contains(rol)) query = query.Where(u => u.Rol == rol);
        return View(await query.OrderBy(u => u.Nombre).ToListAsync());
    }
    public async Task<IActionResult> Editar(int id = 0)
    {
        if (id == 0) return View(new UsuarioViewModel());
        var u = await db.Usuarios.FindAsync(id);
        return u == null ? NotFound() : View(new UsuarioViewModel { Id = u.Id, Nombre = u.Nombre, Email = u.Email, Rol = u.Rol, Activo = u.Activo });
    }
    [HttpPost]
    public async Task<IActionResult> Editar(UsuarioViewModel model)
    {
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        if (!Roles.Todos.Contains(model.Rol)) ModelState.AddModelError("Rol", "Seleccioná un rol válido.");
        if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError("Password", "Ingresá una contraseña inicial.");
        if (await db.Usuarios.AnyAsync(u => u.Email == model.Email && u.Id != model.Id)) ModelState.AddModelError("Email", "Este email ya está registrado.");
        if (model.Id == UsuarioId && (!model.Activo || model.Rol != Roles.SuperAdmin)) ModelState.AddModelError("", "No podés desactivar tu propia cuenta ni quitarte el rol de administrador.");
        if (!ModelState.IsValid) return View(model);
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var u = model.Id == 0 ? Nuevo(model.Rol) : await db.Usuarios.FirstOrDefaultAsync(u => u.Id == model.Id);
        if (u == null) return NotFound();
        var cambiaRol = u.Rol != model.Rol;
        if (u.Rol == Roles.SuperAdmin && u.Activo && (cambiaRol || !model.Activo) && await db.Usuarios.CountAsync(u => u.Rol == Roles.SuperAdmin && u.Activo) <= 1)
        { ModelState.AddModelError("", "Debe quedar al menos un Super Admin activo."); return View(model); }
        u.Nombre = model.Nombre.Trim(); u.Email = model.Email; u.Activo = model.Activo;
        if (!string.IsNullOrWhiteSpace(model.Password)) u.PasswordHash = hasher.HashPassword(u, model.Password);
        if (cambiaRol || !model.Activo || !string.IsNullOrWhiteSpace(model.Password)) u.VersionSesion = Guid.NewGuid();
        if (model.Id == 0) db.Usuarios.Add(u);
        if (model.Id != 0 && (cambiaRol || !model.Activo))
        {
            var vinculos = await db.Vinculos.Where(v => v.AlumnoId == u.Id || v.EntrenadorId == u.Id).ToListAsync();
            vinculos.ForEach(v => v.Desactivar());
            var asignaciones = await db.Asignaciones.Where(a => a.AlumnoId == u.Id || a.Rutina.EntrenadorId == u.Id).ToListAsync();
            asignaciones.ForEach(a => a.Finalizar());
            if (cambiaRol)
            {
                var rutinas = await db.Rutinas.Where(r => r.EntrenadorId == u.Id).ToListAsync();
                rutinas.ForEach(r => r.DarDeBaja());
            }
        }
        try
        {
            await db.SaveChangesAsync();
            if (cambiaRol) await db.Usuarios.Where(x => x.Id == u.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Rol, model.Rol));
            await transaction.CommitAsync();
        }
        catch (DbUpdateException) { ModelState.AddModelError("", "No se pudo guardar. Revisá que el email no esté en uso."); return View(model); }
        Exito("Usuario guardado."); return RedirectToAction(nameof(Index));
    }
    private static Usuario Nuevo(string rol) => rol switch { Roles.Entrenador => new Entrenador(), Roles.SuperAdmin => new SuperAdmin(), _ => new Alumno() };
}
