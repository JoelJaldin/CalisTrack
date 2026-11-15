using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

[Authorize(Roles = Roles.Entrenador)]
public class BibliotecaController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly ArchivoService archivos;
    private readonly AccesoService acceso;
    public BibliotecaController(CalisTrackContext db, ArchivoService archivos, AccesoService acceso) { this.db = db; this.archivos = archivos; this.acceso = acceso; }
    public async Task<IActionResult> Index(string? buscar)
    {
        ViewBag.Buscar = buscar;
        var query = db.Ejercicios.Include(e => e.Materiales).Where(e => e.EntrenadorId == UsuarioId && e.Activo);
        if (!string.IsNullOrWhiteSpace(buscar)) query = query.Where(e => e.Nombre.Contains(buscar) || e.GrupoMuscular.Contains(buscar));
        return View(await query.OrderBy(e => e.Nombre).ToListAsync());
    }
    public async Task<IActionResult> Editar(int id = 0)
    {
        if (id == 0) return View(new EjercicioViewModel());
        var e = await db.Ejercicios.Include(e => e.Materiales).FirstOrDefaultAsync(e => e.Id == id && e.EntrenadorId == UsuarioId && e.Activo);
        return e == null ? NotFound() : View(new EjercicioViewModel { Id = id, Nombre = e.Nombre, Descripcion = e.Descripcion, GrupoMuscular = e.GrupoMuscular, Materiales = e.Materiales });
    }
    [HttpPost]
    public async Task<IActionResult> Editar(EjercicioViewModel model)
    {
        var e = model.Id == 0 ? new Ejercicio { EntrenadorId = UsuarioId } : await db.Ejercicios.Include(e => e.Materiales).FirstOrDefaultAsync(e => e.Id == model.Id && e.EntrenadorId == UsuarioId && e.Activo);
        if (e == null) return NotFound();
        model.Materiales = e.Materiales;
        if (!string.IsNullOrWhiteSpace(model.Enlace) && (!Uri.TryCreate(model.Enlace, UriKind.Absolute, out var uri) || uri.Scheme != "https")) ModelState.AddModelError("Enlace", "Usá un enlace completo que comience con https://.");
        if (!ModelState.IsValid) return View(model);
        ArchivoAdjunto? archivo;
        try { archivo = await archivos.Guardar(model.Archivo, UsuarioId); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("Archivo", ex.Message); return View(model); }
        e.Nombre = model.Nombre.Trim(); e.Descripcion = (model.Descripcion ?? "").Trim(); e.GrupoMuscular = (model.GrupoMuscular ?? "").Trim();
        if (archivo != null) e.Materiales.Add(new MaterialTecnico { Tipo = archivo.Tipo.StartsWith("image/") ? "Imagen" : archivo.Tipo.StartsWith("video/") ? "Video" : "PDF", Descripcion = archivo.Nombre, ArchivoAdjunto = archivo });
        if (!string.IsNullOrWhiteSpace(model.Enlace)) e.Materiales.Add(new MaterialTecnico { Tipo = "Enlace", ArchivoUrl = model.Enlace.Trim(), Descripcion = "Ver demostración" });
        if (model.Id == 0) db.Ejercicios.Add(e);
        if (model.Id != 0 && (archivo != null || !string.IsNullOrWhiteSpace(model.Enlace)))
        {
            var alumnos = await db.Asignaciones.Where(a => a.Activa && a.Rutina.Activa && a.Rutina.Ejercicios.Any(r => r.EjercicioId == e.Id && r.Activo) && db.Vinculos.Any(v => v.Activo && v.AlumnoId == a.AlumnoId && v.EntrenadorId == UsuarioId && v.Alumno.Activo)).Select(a => a.AlumnoId).Distinct().ToListAsync();
            foreach (var id in alumnos) acceso.Notificar(id, "Material", "Nuevo material para " + e.Nombre + ".", "/Rutinas/MiRutina");
        }
        await db.SaveChangesAsync(); Exito("Ejercicio guardado."); return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var e = await db.Ejercicios.FirstOrDefaultAsync(e => e.Id == id && e.EntrenadorId == UsuarioId);
        if (e == null) return NotFound();
        if (await db.RutinaEjercicios.AnyAsync(r => r.EjercicioId == id && r.Activo && r.Rutina.Activa)) Error("Quitá el ejercicio de las rutinas activas antes de darlo de baja.");
        else { e.Activo = false; await db.SaveChangesAsync(); Exito("Ejercicio dado de baja."); }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> EliminarMaterial(int id)
    {
        var m = await db.Materiales.FirstOrDefaultAsync(m => m.Id == id && m.Ejercicio.EntrenadorId == UsuarioId);
        if (m == null) return NotFound();
        db.Materiales.Remove(m); await db.SaveChangesAsync(); Exito("Material eliminado del ejercicio.");
        return RedirectToAction(nameof(Editar), new { id = m.EjercicioId });
    }
}
