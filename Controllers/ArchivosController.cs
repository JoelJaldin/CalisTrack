using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class ArchivosController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    private readonly ArchivoService archivos;
    public ArchivosController(CalisTrackContext db, AccesoService acceso, ArchivoService archivos) { this.db = db; this.acceso = acceso; this.archivos = archivos; }
    public async Task<IActionResult> Ver(int id, bool descargar = false)
    {
        var archivo = await db.Archivos.FindAsync(id);
        if (archivo == null) return NotFound();
        var permitido = archivo.PropietarioId == UsuarioId;
        if (!permitido)
        {
            var mensaje = await db.Mensajes.FirstOrDefaultAsync(m => m.ArchivoAdjuntoId == id && (m.EmisorId == UsuarioId || m.ReceptorId == UsuarioId));
            permitido = mensaje != null;
        }
        if (!permitido)
        {
            var devolucion = await db.Devoluciones.Include(d => d.Entrenamiento).ThenInclude(e => e.AsignacionRutina).FirstOrDefaultAsync(d => d.ArchivoAdjuntoId == id);
            permitido = devolucion != null && await acceso.VerEntrenamiento(UsuarioId, Rol, devolucion.Entrenamiento);
        }
        if (!permitido && Rol == Roles.Alumno)
        {
            permitido = await db.Materiales.AnyAsync(m => m.ArchivoAdjuntoId == id && db.Asignaciones.Any(a => a.AlumnoId == UsuarioId && a.Activa && a.Rutina.Activa && a.FechaDesde <= Fecha.Hoy && (a.FechaHasta == null || a.FechaHasta >= Fecha.Hoy) && a.Rutina.Ejercicios.Any(r => r.EjercicioId == m.EjercicioId && r.Activo) && db.Vinculos.Any(v => v.EntrenadorId == a.Rutina.EntrenadorId && v.AlumnoId == UsuarioId && v.Activo && v.Entrenador.Activo)));
        }
        if (!permitido || !System.IO.File.Exists(archivos.Ruta(archivo))) return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        if (descargar || archivo.Tipo == "application/pdf") return PhysicalFile(archivos.Ruta(archivo), archivo.Tipo, archivo.Nombre, enableRangeProcessing: true);
        return PhysicalFile(archivos.Ruta(archivo), archivo.Tipo, enableRangeProcessing: true);
    }
}
