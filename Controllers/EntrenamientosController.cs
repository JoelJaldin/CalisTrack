using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class EntrenamientosController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    private readonly ArchivoService archivos;
    public EntrenamientosController(CalisTrackContext db, AccesoService acceso, ArchivoService archivos) { this.db = db; this.acceso = acceso; this.archivos = archivos; }
    [Authorize(Roles = Roles.Alumno), HttpPost]
    public async Task<IActionResult> Comenzar(int asignacionId)
    {
        var a = await db.Asignaciones.Include(a => a.Rutina).ThenInclude(r => r.Ejercicios.Where(e => e.Activo)).ThenInclude(e => e.Ejercicio).FirstOrDefaultAsync(a => a.Id == asignacionId && a.AlumnoId == UsuarioId);
        if (a == null || !await AsignacionVigente(a)) return NotFound();
        var pendiente = await db.Entrenamientos.FirstOrDefaultAsync(e => e.AsignacionRutinaId == a.Id && !e.Finalizado);
        if (pendiente != null) return RedirectToAction(nameof(Entrenar), new { id = pendiente.Id });
        var e = new Entrenamiento { AsignacionRutinaId = a.Id, Fecha = Fecha.Hoy, NombreRutina = a.Rutina.Nombre };
        foreach (var item in a.Rutina.Ejercicios.OrderBy(r => r.Orden))
            for (var n = 1; n <= item.Series; n++)
                e.Registros.Add(new RegistroSerie { RutinaEjercicioId = item.Id, NumeroSerie = n, NombreEjercicio = item.Ejercicio.Nombre, RepeticionesObjetivo = item.Repeticiones, DescansoSegundos = item.DescansoSegundos });
        if (e.Registros.Count == 0) { Error("La rutina no tiene ejercicios."); return RedirectToAction("MiRutina", "Rutinas"); }
        db.Entrenamientos.Add(e);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            var id = await db.Entrenamientos.Where(x => x.AsignacionRutinaId == a.Id && !x.Finalizado).Select(x => x.Id).FirstOrDefaultAsync();
            if (id == 0) throw;
            return RedirectToAction(nameof(Entrenar), new { id });
        }
        return RedirectToAction(nameof(Entrenar), new { id = e.Id });
    }
    [Authorize(Roles = Roles.Alumno)]
    public async Task<IActionResult> Entrenar(int id)
    {
        var e = await Cargar(id);
        if (e == null || e.AsignacionRutina.AlumnoId != UsuarioId || !await AsignacionVigente(e.AsignacionRutina)) return NotFound();
        if (e.Finalizado) return RedirectToAction(nameof(Detalle), new { id });
        return View(Formulario(e));
    }
    [Authorize(Roles = Roles.Alumno), HttpPost]
    public async Task<IActionResult> Entrenar(EntrenarViewModel model, string accion)
    {
        var e = await Cargar(model.Id);
        if (e == null || e.AsignacionRutina.AlumnoId != UsuarioId || !await AsignacionVigente(e.AsignacionRutina)) return NotFound();
        if (e.Finalizado) return RedirectToAction(nameof(Detalle), new { id = e.Id });
        if (model.Fecha == default || model.Fecha.Date > Fecha.Hoy || model.Fecha.Date < e.AsignacionRutina.FechaDesde || (e.AsignacionRutina.FechaHasta.HasValue && model.Fecha.Date > e.AsignacionRutina.FechaHasta.Value)) ModelState.AddModelError("Fecha", "La fecha debe estar dentro de la asignación y no puede ser futura.");
        if (model.Series.Count != e.Registros.Count || model.Series.Select(s => s.Id).Distinct().Count() != e.Registros.Count || model.Series.Any(s => !e.Registros.Any(r => r.Id == s.Id))) return BadRequest();
        if (accion != "guardar" && accion != "finalizar") return BadRequest();
        if (accion == "finalizar" && !model.Series.Any(s => s.Completada)) ModelState.AddModelError("", "Marcá al menos una serie como realizada para finalizar.");
        model.NombreRutina = e.NombreRutina;
        foreach (var s in model.Series)
        {
            var original = e.Registros.First(r => r.Id == s.Id);
            s.RutinaEjercicioId = original.RutinaEjercicioId; s.NombreEjercicio = original.NombreEjercicio; s.NumeroSerie = original.NumeroSerie; s.Objetivo = original.RepeticionesObjetivo; s.Descanso = original.DescansoSegundos;
        }
        if (!ModelState.IsValid) return View(model);
        e.Fecha = model.Fecha.Date; e.Comentario = (model.Comentario ?? "").Trim();
        foreach (var s in model.Series)
        {
            var item = e.Registros.First(r => r.Id == s.Id);
            item.Repeticiones = s.Repeticiones; item.TiempoSegundos = s.TiempoSegundos; item.Completada = s.Completada;
        }
        if (accion == "finalizar")
        {
            e.Finalizar();
            acceso.Notificar(e.AsignacionRutina.Rutina.EntrenadorId, "Entrenamiento", "Un alumno finalizó " + e.NombreRutina + ".", "/Entrenamientos/Detalle/" + e.Id);
        }
        await db.SaveChangesAsync();
        Exito(accion == "finalizar" ? "¡Entrenamiento finalizado! Tu progreso quedó guardado." : "Avance guardado. Podés continuar más tarde.");
        return RedirectToAction(accion == "finalizar" ? nameof(Detalle) : nameof(Entrenar), new { id = e.Id });
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var e = await Cargar(id);
        if (e == null || !await acceso.VerEntrenamiento(UsuarioId, Rol, e) || (Rol == Roles.Entrenador && !e.Finalizado)) return NotFound();
        return View(e);
    }
    [Authorize(Roles = Roles.Entrenador), HttpPost]
    public async Task<IActionResult> Devolver(int entrenamientoId, string? texto, IFormFile? archivo, int devolucionId = 0)
    {
        var e = await Cargar(entrenamientoId);
        if (e == null || !e.Finalizado || !await acceso.VerEntrenamiento(UsuarioId, Rol, e)) return NotFound();
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > 2000) { Error("Escribí una devolución de entre 1 y 2000 caracteres."); return RedirectToAction(nameof(Detalle), new { id = e.Id }); }
        var d = devolucionId == 0 ? new Devolucion { EntrenadorId = UsuarioId, EntrenamientoId = e.Id } : await db.Devoluciones.FirstOrDefaultAsync(d => d.Id == devolucionId && d.EntrenadorId == UsuarioId && d.EntrenamientoId == e.Id);
        if (d == null) return NotFound();
        try { var adjunto = await archivos.Guardar(archivo, UsuarioId); if (adjunto != null) d.ArchivoAdjunto = adjunto; }
        catch (InvalidOperationException ex) { Error(ex.Message); return RedirectToAction(nameof(Detalle), new { id = e.Id }); }
        d.Texto = texto.Trim(); d.FechaHora = DateTime.UtcNow;
        if (devolucionId == 0) db.Devoluciones.Add(d);
        acceso.Notificar(e.AsignacionRutina.AlumnoId, "Devolución", "Tu entrenador dejó una devolución sobre " + e.NombreRutina + ".", "/Entrenamientos/Detalle/" + e.Id);
        await db.SaveChangesAsync(); Exito("Devolución guardada."); return RedirectToAction(nameof(Detalle), new { id = e.Id });
    }
    private Task<Entrenamiento?> Cargar(int id) => db.Entrenamientos.Include(e => e.AsignacionRutina).ThenInclude(a => a.Rutina).Include(e => e.AsignacionRutina).ThenInclude(a => a.Alumno).Include(e => e.Registros).ThenInclude(r => r.RutinaEjercicio).Include(e => e.Devoluciones).ThenInclude(d => d.Entrenador).Include(e => e.Devoluciones).ThenInclude(d => d.ArchivoAdjunto).AsSplitQuery().FirstOrDefaultAsync(e => e.Id == id);
    private async Task<bool> AsignacionVigente(AsignacionRutina a) => a.Activa && a.Rutina.Activa && a.FechaDesde <= Fecha.Hoy && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy) && await acceso.Vinculados(a.Rutina.EntrenadorId, a.AlumnoId);
    private static EntrenarViewModel Formulario(Entrenamiento e) => new() { Id = e.Id, NombreRutina = e.NombreRutina, Fecha = e.Fecha, Comentario = e.Comentario, Series = e.Registros.OrderBy(r => r.RutinaEjercicio.Orden).ThenBy(r => r.NumeroSerie).Select(r => new FilaSerie { Id = r.Id, RutinaEjercicioId = r.RutinaEjercicioId, NombreEjercicio = r.NombreEjercicio, NumeroSerie = r.NumeroSerie, Objetivo = r.RepeticionesObjetivo, Descanso = r.DescansoSegundos, Repeticiones = r.Repeticiones, TiempoSegundos = r.TiempoSegundos, Completada = r.Completada }).ToList() };
}
