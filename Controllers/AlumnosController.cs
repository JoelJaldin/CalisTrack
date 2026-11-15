using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

[Authorize(Roles = Roles.Entrenador)]
public class AlumnosController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    public AlumnosController(CalisTrackContext db, AccesoService acceso) { this.db = db; this.acceso = acceso; }
    public async Task<IActionResult> Index(string? buscar, bool sinRutina = false)
    {
        ViewBag.Buscar = buscar;
        var consulta = db.Vinculos.Include(v => v.Alumno).Where(v => v.EntrenadorId == UsuarioId && v.Activo);
        if (!string.IsNullOrWhiteSpace(buscar)) consulta = consulta.Where(v => v.Alumno.Nombre.Contains(buscar) || v.Alumno.Email.Contains(buscar));
        ViewBag.Disponibles = await db.Usuarios.OfType<Alumno>().Where(a => a.Activo && !db.Vinculos.Any(v => v.AlumnoId == a.Id && v.Activo)).OrderBy(a => a.Nombre).ToListAsync();
        var alumnos = await consulta.OrderBy(v => v.Alumno.Nombre).ToListAsync();
        var ids = alumnos.Select(v => v.AlumnoId).ToList();
        var asignaciones = await db.Asignaciones.Include(a => a.Rutina).Where(a => ids.Contains(a.AlumnoId) && a.Activa && a.Rutina.Activa && a.FechaDesde <= Fecha.Hoy && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy)).ToListAsync();
        ViewBag.Asignaciones = asignaciones;
        ViewBag.Ultimos = await db.Entrenamientos.Where(e => ids.Contains(e.AsignacionRutina.AlumnoId) && e.Finalizado).GroupBy(e => e.AsignacionRutina.AlumnoId).Select(g => new { Id = g.Key, Fecha = g.Max(e => e.Fecha) }).ToDictionaryAsync(e => e.Id, e => e.Fecha);
        ViewBag.Total = alumnos.Count;
        ViewBag.SinRutina = alumnos.Count(v => !asignaciones.Any(a => a.AlumnoId == v.AlumnoId));
        ViewBag.FiltrarSinRutina = sinRutina;
        return View(sinRutina ? alumnos.Where(v => !asignaciones.Any(a => a.AlumnoId == v.AlumnoId)).ToList() : alumnos);
    }
    [HttpPost]
    public async Task<IActionResult> Vincular(int alumnoId)
    {
        if (!await db.Usuarios.OfType<Alumno>().AnyAsync(a => a.Id == alumnoId && a.Activo) || await db.Vinculos.AnyAsync(v => v.AlumnoId == alumnoId && v.Activo))
        { Error("El alumno ya está vinculado o no está disponible."); return RedirectToAction(nameof(Index)); }
        var vinculo = await db.Vinculos.FirstOrDefaultAsync(v => v.AlumnoId == alumnoId && v.EntrenadorId == UsuarioId);
        if (vinculo == null) db.Vinculos.Add(new VinculoEntrenadorAlumno { AlumnoId = alumnoId, EntrenadorId = UsuarioId });
        else { vinculo.Activo = true; vinculo.FechaAlta = DateTime.UtcNow; }
        acceso.Notificar(alumnoId, "Vínculo", "Tu entrenador ya puede acompañar tu progreso.", "/Inicio");
        try { await db.SaveChangesAsync(); Exito("Alumno vinculado."); }
        catch (DbUpdateException) { Error("El alumno ya fue vinculado. Actualizá el listado."); }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Desvincular(int id)
    {
        var v = await db.Vinculos.FirstOrDefaultAsync(v => v.Id == id && v.EntrenadorId == UsuarioId && v.Activo);
        if (v == null) return NotFound();
        v.Desactivar();
        var asignaciones = await db.Asignaciones.Where(a => a.AlumnoId == v.AlumnoId && a.Rutina.EntrenadorId == UsuarioId && a.Activa).ToListAsync();
        asignaciones.ForEach(a => a.Finalizar());
        await db.SaveChangesAsync();
        Exito("Vínculo desactivado. El alumno conserva su historial.");
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Detalle(int id)
    {
        if (!await acceso.Vinculados(UsuarioId, id)) return NotFound();
        var alumno = await db.Usuarios.OfType<Alumno>().FirstAsync(a => a.Id == id);
        ViewBag.Asignaciones = await db.Asignaciones.Include(a => a.Rutina).Where(a => a.AlumnoId == id && a.Rutina.EntrenadorId == UsuarioId && a.Activa).ToListAsync();
        var sesiones = await db.Entrenamientos.Include(e => e.Registros).Include(e => e.Devoluciones).AsSplitQuery().Where(e => e.AsignacionRutina.AlumnoId == id && e.Finalizado).OrderByDescending(e => e.Fecha).ToListAsync();
        ViewBag.Sesiones = sesiones.Count;
        ViewBag.Recientes = sesiones.Take(5).ToList();
        var lunes = Fecha.Hoy.AddDays(-((int)Fecha.Hoy.DayOfWeek + 6) % 7);
        ViewBag.Semanas = Enumerable.Range(0, 6).Select(i =>
        {
            var inicio = lunes.AddDays((i - 5) * 7);
            var semana = sesiones.Where(e => e.Fecha >= inicio && e.Fecha < inicio.AddDays(7)).ToList();
            return new SemanaProgreso { Inicio = inicio, Sesiones = semana.Count, Repeticiones = semana.SelectMany(e => e.Registros).Where(r => r.Completada).Sum(r => r.Repeticiones) };
        }).ToList();
        return View(alumno);
    }
}
