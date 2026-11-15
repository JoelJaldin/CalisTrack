using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

[Authorize(Roles = Roles.Alumno + "," + Roles.Entrenador)]
public class HistorialController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    public HistorialController(CalisTrackContext db, AccesoService acceso) { this.db = db; this.acceso = acceso; }
    public async Task<IActionResult> Index(int? alumnoId, DateTime? desde, DateTime? hasta, int? ejercicioId, int? rutinaId)
    {
        var id = Rol == Roles.Alumno ? UsuarioId : alumnoId ?? 0;
        if (Rol == Roles.Entrenador && !await acceso.Vinculados(UsuarioId, id)) return RedirectToAction("Index", "Alumnos");
        var alumno = await db.Usuarios.FindAsync(id);
        if (alumno == null) return NotFound();
        var model = new HistorialViewModel { AlumnoId = id, NombreAlumno = alumno.Nombre, Desde = desde, Hasta = hasta, EjercicioId = ejercicioId, RutinaId = rutinaId };
        var todas = db.Entrenamientos.Where(e => e.AsignacionRutina.AlumnoId == id && e.Finalizado);
        model.Ejercicios = await db.Registros.Where(r => r.Entrenamiento.AsignacionRutina.AlumnoId == id && r.Entrenamiento.Finalizado).Select(r => r.RutinaEjercicio.Ejercicio).Distinct().OrderBy(e => e.Nombre).ToListAsync();
        model.Rutinas = await todas.Select(e => e.AsignacionRutina.Rutina).Distinct().OrderBy(r => r.Nombre).ToListAsync();
        var query = todas;
        if (desde.HasValue) query = query.Where(e => e.Fecha >= desde.Value.Date);
        if (hasta.HasValue) query = query.Where(e => e.Fecha <= hasta.Value.Date);
        if (desde > hasta) ModelState.AddModelError("", "La fecha desde debe ser anterior a la fecha hasta.");
        if (rutinaId.HasValue) query = query.Where(e => e.AsignacionRutina.RutinaId == rutinaId);
        if (ejercicioId.HasValue) query = query.Where(e => e.Registros.Any(r => r.RutinaEjercicio.EjercicioId == ejercicioId));
        model.Entrenamientos = await query.AsSplitQuery().Include(e => e.Registros).ThenInclude(r => r.RutinaEjercicio).Include(e => e.Devoluciones).OrderByDescending(e => e.Fecha).ToListAsync();
        model.Semanas = model.Entrenamientos.GroupBy(e => e.Fecha.Date.AddDays(-((int)e.Fecha.DayOfWeek + 6) % 7)).OrderBy(g => g.Key).Select(g => new SemanaProgreso
        {
            Inicio = g.Key, Sesiones = g.Count(),
            Repeticiones = g.SelectMany(e => e.Registros).Where(r => r.Completada && (!ejercicioId.HasValue || r.RutinaEjercicio.EjercicioId == ejercicioId)).Sum(r => r.Repeticiones),
            Segundos = g.SelectMany(e => e.Registros).Where(r => r.Completada && (!ejercicioId.HasValue || r.RutinaEjercicio.EjercicioId == ejercicioId)).Sum(r => r.TiempoSegundos ?? 0)
        }).ToList();
        return View(model);
    }
}
