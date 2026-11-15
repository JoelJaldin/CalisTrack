using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class RutinasController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    public RutinasController(CalisTrackContext db, AccesoService acceso) { this.db = db; this.acceso = acceso; }
    [Authorize(Roles = Roles.Entrenador)]
    public async Task<IActionResult> Index(string? buscar, bool inactivas = false)
    {
        ViewBag.Buscar = buscar;
        var query = db.Rutinas.Include(r => r.Ejercicios.Where(e => e.Activo)).ThenInclude(e => e.Ejercicio).Where(r => r.EntrenadorId == UsuarioId && r.Activa != inactivas);
        if (!string.IsNullOrWhiteSpace(buscar)) query = query.Where(r => r.Nombre.Contains(buscar));
        var rutinas = await query.OrderByDescending(r => r.FechaCreacion).ToListAsync();
        var ids = rutinas.Select(r => r.Id).ToList();
        ViewBag.Asignaciones = await db.Asignaciones.Include(a => a.Alumno).Where(a => ids.Contains(a.RutinaId) && a.Activa && a.Alumno.Activo && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy)).ToListAsync();
        ViewBag.Activas = await db.Rutinas.CountAsync(r => r.EntrenadorId == UsuarioId && r.Activa);
        ViewBag.Inactivas = await db.Rutinas.CountAsync(r => r.EntrenadorId == UsuarioId && !r.Activa);
        ViewBag.FiltrarInactivas = inactivas;
        return View(rutinas);
    }
    [Authorize(Roles = Roles.Entrenador)]
    public async Task<IActionResult> Editar(int id = 0)
    {
        var model = new RutinaViewModel();
        if (id != 0)
        {
            var rutina = await db.Rutinas.Include(r => r.Ejercicios.Where(e => e.Activo)).FirstOrDefaultAsync(r => r.Id == id && r.EntrenadorId == UsuarioId && r.Activa);
            if (rutina == null) return NotFound();
            model.Id = id; model.Nombre = rutina.Nombre; model.Descripcion = rutina.Descripcion;
            model.Ejercicios = rutina.Ejercicios.OrderBy(e => e.Orden).Select(e => new FilaRutina { EjercicioId = e.EjercicioId, Series = e.Series, Repeticiones = e.Repeticiones, DescansoSegundos = e.DescansoSegundos, Observaciones = e.Observaciones }).ToList();
        }
        if (model.Ejercicios.Count == 0) model.Ejercicios.Add(new FilaRutina());
        await CargarBiblioteca(model); return View(model);
    }
    [Authorize(Roles = Roles.Entrenador), HttpPost]
    public async Task<IActionResult> Editar(RutinaViewModel model, string? accion)
    {
        await CargarBiblioteca(model);
        if (model.Ejercicios.Count == 0 || model.Ejercicios.Count > 30) ModelState.AddModelError("", "La rutina debe tener entre 1 y 30 ejercicios.");
        if (model.Ejercicios.Any(e => !model.Biblioteca.Any(b => b.Id == e.EjercicioId))) ModelState.AddModelError("", "Elegí ejercicios activos de tu biblioteca.");
        if (model.Ejercicios.Select(e => e.EjercicioId).Distinct().Count() != model.Ejercicios.Count) ModelState.AddModelError("", "No repitas ejercicios en la misma rutina.");
        if (!ModelState.IsValid)
        {
            if (model.Ejercicios.Count == 0) model.Ejercicios.Add(new FilaRutina());
            return View(model);
        }
        var rutina = model.Id == 0 ? new Rutina { EntrenadorId = UsuarioId } : await db.Rutinas.Include(r => r.Ejercicios).FirstOrDefaultAsync(r => r.Id == model.Id && r.EntrenadorId == UsuarioId && r.Activa);
        if (rutina == null) return NotFound();
        rutina.Nombre = model.Nombre.Trim(); rutina.Descripcion = (model.Descripcion ?? "").Trim();
        foreach (var item in rutina.Ejercicios) item.Activo = false;
        for (var i = 0; i < model.Ejercicios.Count; i++)
        {
            var item = model.Ejercicios[i];
            rutina.Ejercicios.Add(new RutinaEjercicio { EjercicioId = item.EjercicioId, Orden = i + 1, Series = item.Series, Repeticiones = item.Repeticiones, DescansoSegundos = item.DescansoSegundos, Observaciones = (item.Observaciones ?? "").Trim() });
        }
        if (model.Id == 0) db.Rutinas.Add(rutina);
        else
        {
            var ids = await db.Asignaciones.Where(a => a.RutinaId == rutina.Id && a.Activa && a.Alumno.Activo).Select(a => a.AlumnoId).Distinct().ToListAsync();
            foreach (var id in ids) acceso.Notificar(id, "Rutina", "Tu entrenador actualizó " + rutina.Nombre + ".", "/Rutinas/MiRutina");
        }
        await db.SaveChangesAsync(); Exito("Rutina guardada."); return accion == "asignar" ? RedirectToAction(nameof(Asignar), new { id = rutina.Id }) : RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = Roles.Entrenador), HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var rutina = await db.Rutinas.FirstOrDefaultAsync(r => r.Id == id && r.EntrenadorId == UsuarioId);
        if (rutina == null) return NotFound();
        rutina.DarDeBaja();
        var asignaciones = await db.Asignaciones.Where(a => a.RutinaId == id && a.Activa).ToListAsync();
        asignaciones.ForEach(a => a.Finalizar());
        await db.SaveChangesAsync(); Exito("Rutina dada de baja. El historial se conserva."); return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = Roles.Entrenador)]
    public async Task<IActionResult> Asignar(int id)
    {
        var rutina = await db.Rutinas.FirstOrDefaultAsync(r => r.Id == id && r.EntrenadorId == UsuarioId && r.Activa);
        if (rutina == null) return NotFound();
        var model = new AsignarViewModel { RutinaId = id, NombreRutina = rutina.Nombre };
        await CargarAlumnos(model); return View(model);
    }
    [Authorize(Roles = Roles.Entrenador), HttpPost]
    public async Task<IActionResult> Asignar(AsignarViewModel model)
    {
        var rutina = await db.Rutinas.FirstOrDefaultAsync(r => r.Id == model.RutinaId && r.EntrenadorId == UsuarioId && r.Activa);
        if (rutina == null) return NotFound();
        model.NombreRutina = rutina.Nombre; await CargarAlumnos(model);
        if (model.AlumnoIds.Count == 0 || model.AlumnoIds.Any(id => !model.Alumnos.Any(a => a.Id == id))) ModelState.AddModelError("", "Seleccioná al menos un alumno vinculado y activo.");
        if (model.FechaDesde == default || (model.FechaHasta.HasValue && model.FechaHasta.Value.Date < model.FechaDesde.Date)) ModelState.AddModelError("", "La fecha hasta debe ser igual o posterior a la fecha desde.");
        if (!await db.RutinaEjercicios.AnyAsync(e => e.RutinaId == rutina.Id && e.Activo)) ModelState.AddModelError("", "La rutina no tiene ejercicios.");
        if (!ModelState.IsValid) return View(model);
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var hasta = model.FechaHasta?.Date ?? DateTime.MaxValue.Date;
        if (await db.Asignaciones.AnyAsync(a => model.AlumnoIds.Contains(a.AlumnoId) && a.Activa && a.FechaDesde <= hasta && (a.FechaHasta == null || a.FechaHasta >= model.FechaDesde.Date)))
        {
            ModelState.AddModelError("", "Un alumno seleccionado ya tiene una rutina en ese período. Finalizá la asignación anterior desde su perfil o elegí otras fechas."); return View(model);
        }
        foreach (var id in model.AlumnoIds.Distinct())
        {
            db.Asignaciones.Add(new AsignacionRutina { AlumnoId = id, RutinaId = rutina.Id, FechaDesde = model.FechaDesde.Date, FechaHasta = model.FechaHasta?.Date });
            acceso.Notificar(id, "Rutina", "Tenés una nueva rutina: " + rutina.Nombre + ".", "/Rutinas/MiRutina");
        }
        await db.SaveChangesAsync(); await transaction.CommitAsync(); Exito("Rutina asignada y alumnos notificados."); return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = Roles.Entrenador), HttpPost]
    public async Task<IActionResult> FinalizarAsignacion(int id)
    {
        var a = await db.Asignaciones.FirstOrDefaultAsync(a => a.Id == id && a.Rutina.EntrenadorId == UsuarioId && db.Vinculos.Any(v => v.EntrenadorId == UsuarioId && v.AlumnoId == a.AlumnoId && v.Activo));
        if (a == null) return NotFound();
        a.Finalizar(); await db.SaveChangesAsync(); Exito("Asignación finalizada."); return RedirectToAction("Detalle", "Alumnos", new { id = a.AlumnoId });
    }
    [Authorize(Roles = Roles.Alumno)]
    public async Task<IActionResult> MiRutina()
    {
        var asignaciones = await db.Asignaciones.AsSplitQuery().Include(a => a.Rutina.Entrenador).Include(a => a.Rutina).ThenInclude(r => r.Ejercicios.Where(e => e.Activo)).ThenInclude(e => e.Ejercicio).ThenInclude(e => e.Materiales).Where(a => a.AlumnoId == UsuarioId && a.Activa && a.Rutina.Activa && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy) && db.Vinculos.Any(v => v.AlumnoId == UsuarioId && v.EntrenadorId == a.Rutina.EntrenadorId && v.Activo && v.Entrenador.Activo)).OrderBy(a => a.FechaDesde).ToListAsync();
        return View(asignaciones);
    }
    [Authorize(Roles = Roles.Alumno)]
    public async Task<IActionResult> Tecnica(int id)
    {
        var ejercicio = await db.RutinaEjercicios.Include(e => e.Ejercicio).ThenInclude(e => e.Materiales).Include(e => e.Rutina).FirstOrDefaultAsync(e => e.Id == id && e.Activo && e.Rutina.Activa);
        if (ejercicio == null || !await acceso.Vinculados(ejercicio.Rutina.EntrenadorId, UsuarioId) || !await db.Asignaciones.AnyAsync(a => a.RutinaId == ejercicio.RutinaId && a.AlumnoId == UsuarioId && a.Activa && a.FechaDesde <= Fecha.Hoy && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy))) return NotFound();
        return View(ejercicio);
    }
    private async Task CargarBiblioteca(RutinaViewModel model) => model.Biblioteca = await db.Ejercicios.Where(e => e.EntrenadorId == UsuarioId && e.Activo).OrderBy(e => e.Nombre).ToListAsync();
    private async Task CargarAlumnos(AsignarViewModel model) => model.Alumnos = await db.Vinculos.Where(v => v.EntrenadorId == UsuarioId && v.Activo && v.Alumno.Activo).Select(v => v.Alumno).OrderBy(a => a.Nombre).ToListAsync();
}
