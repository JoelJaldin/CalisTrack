using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class InicioController : BaseController
{
    private readonly CalisTrackContext db;
    public InicioController(CalisTrackContext db) { this.db = db; }
    public async Task<IActionResult> Index()
    {
        var model = new InicioViewModel { Nombre = (await db.Usuarios.FindAsync(UsuarioId))!.Nombre, NoLeidas = await db.Notificaciones.CountAsync(n => n.UsuarioId == UsuarioId && !n.Leida) };
        var sesiones = db.Entrenamientos.Include(e => e.AsignacionRutina).ThenInclude(a => a.Alumno).Where(e => e.Finalizado);
        if (Rol == Roles.Alumno)
        {
            model.Asignaciones = await db.Asignaciones.Include(a => a.Rutina).ThenInclude(r => r.Ejercicios.Where(e => e.Activo)).Include(a => a.Rutina.Entrenador).Where(a => a.AlumnoId == UsuarioId && a.Activa && a.Rutina.Activa && a.FechaDesde <= Fecha.Hoy && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy) && db.Vinculos.Any(v => v.AlumnoId == UsuarioId && v.EntrenadorId == a.Rutina.EntrenadorId && v.Activo && v.Entrenador.Activo)).ToListAsync();
            sesiones = sesiones.Where(e => e.AsignacionRutina.AlumnoId == UsuarioId);
        }
        else if (Rol == Roles.Entrenador)
        {
            model.Alumnos = await db.Vinculos.CountAsync(v => v.EntrenadorId == UsuarioId && v.Activo && v.Alumno.Activo);
            model.Rutinas = await db.Rutinas.CountAsync(r => r.EntrenadorId == UsuarioId && r.Activa);
            model.Ejercicios = await db.Ejercicios.CountAsync(e => e.EntrenadorId == UsuarioId && e.Activo);
            sesiones = sesiones.Where(e => db.Vinculos.Any(v => v.EntrenadorId == UsuarioId && v.AlumnoId == e.AsignacionRutina.AlumnoId && v.Activo && v.Alumno.Activo));
        }
        else
        {
            model.Usuarios = await db.Usuarios.CountAsync();
            model.Alumnos = await db.Usuarios.OfType<Alumno>().CountAsync();
            model.Entrenadores = await db.Usuarios.OfType<Entrenador>().CountAsync();
            model.Inactivos = await db.Usuarios.CountAsync(u => !u.Activo);
            model.UltimosUsuarios = await db.Usuarios.OrderByDescending(u => u.Id).Take(5).ToListAsync();
        }
        if (Rol != Roles.SuperAdmin)
        {
            model.Sesiones = await sesiones.CountAsync();
            model.Recientes = await sesiones.Where(e => Rol != Roles.Entrenador || !e.Devoluciones.Any()).OrderByDescending(e => e.Fecha).Take(5).ToListAsync();
            model.Pendientes = await sesiones.CountAsync(e => !e.Devoluciones.Any());
        }
        if (Rol == Roles.Alumno)
        {
            var lunes = Fecha.Hoy.AddDays(-((int)Fecha.Hoy.DayOfWeek + 6) % 7);
            var desde = lunes.AddDays(-35);
            var recientes = await sesiones.Include(e => e.Registros).Where(e => e.Fecha >= desde).ToListAsync();
            model.Semanas = Enumerable.Range(0, 6).Select(i =>
            {
                var inicio = desde.AddDays(i * 7);
                var semana = recientes.Where(e => e.Fecha >= inicio && e.Fecha < inicio.AddDays(7)).ToList();
                return new SemanaProgreso { Inicio = inicio, Sesiones = semana.Count, Repeticiones = semana.SelectMany(e => e.Registros).Where(r => r.Completada).Sum(r => r.Repeticiones) };
            }).ToList();
            model.SesionesSemana = model.Semanas[^1].Sesiones;
            model.RepeticionesSemana = model.Semanas[^1].Repeticiones;
            var anterior = model.Semanas[^2].Repeticiones;
            if (anterior > 0) model.Variacion = Math.Round((model.RepeticionesSemana - anterior) * 100m / anterior);
            model.UltimaDevolucion = await db.Devoluciones.Include(d => d.Entrenador).Include(d => d.Entrenamiento).Where(d => d.Entrenamiento.AsignacionRutina.AlumnoId == UsuarioId).OrderByDescending(d => d.FechaHora).FirstOrDefaultAsync();
            model.Notificaciones = await db.Notificaciones.Where(n => n.UsuarioId == UsuarioId).OrderByDescending(n => n.FechaHora).Take(3).ToListAsync();
        }
        return View(model);
    }
    [AllowAnonymous]
    public IActionResult Error() { Response.StatusCode = 500; return View(); }
}
