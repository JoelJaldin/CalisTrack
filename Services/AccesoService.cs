using CalisTrack.Data;
using CalisTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Services;

public class AccesoService
{
    private readonly CalisTrackContext db;
    public AccesoService(CalisTrackContext db) { this.db = db; }
    public Task<bool> Vinculados(int entrenadorId, int alumnoId) => db.Vinculos.AnyAsync(v => v.EntrenadorId == entrenadorId && v.AlumnoId == alumnoId && v.Activo && v.Entrenador.Activo && v.Alumno.Activo && v.Entrenador.Rol == Roles.Entrenador && v.Alumno.Rol == Roles.Alumno);
    public void Notificar(int usuarioId, string tipo, string texto, string enlace)
    {
        db.Notificaciones.Add(new Notificacion { UsuarioId = usuarioId, Tipo = tipo, Texto = texto, Enlace = enlace });
    }
    public async Task<bool> VerEntrenamiento(int usuarioId, string rol, Entrenamiento entrenamiento)
    {
        var asignacion = entrenamiento.AsignacionRutina;
        return rol == Roles.Alumno ? asignacion.AlumnoId == usuarioId : rol == Roles.Entrenador && await Vinculados(usuarioId, asignacion.AlumnoId);
    }
}
