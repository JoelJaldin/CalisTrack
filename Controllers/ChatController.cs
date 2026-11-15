using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

[Authorize(Roles = Roles.Alumno + "," + Roles.Entrenador)]
public class ChatController : BaseController
{
    private readonly CalisTrackContext db;
    private readonly AccesoService acceso;
    private readonly ArchivoService archivos;
    public ChatController(CalisTrackContext db, AccesoService acceso, ArchivoService archivos) { this.db = db; this.acceso = acceso; this.archivos = archivos; }
    public async Task<IActionResult> Index(int? contactoId)
    {
        var model = new ChatViewModel { UsuarioId = UsuarioId };
        var vinculos = db.Vinculos.Where(v => v.Activo && v.Entrenador.Activo && v.Alumno.Activo);
        model.Contactos = Rol == Roles.Entrenador ? await vinculos.Where(v => v.EntrenadorId == UsuarioId).Select(v => v.Alumno).OrderBy(u => u.Nombre).ToListAsync() : await vinculos.Where(v => v.AlumnoId == UsuarioId).Select(v => v.Entrenador).OrderBy(u => u.Nombre).ToListAsync();
        model.Contacto = contactoId.HasValue ? model.Contactos.FirstOrDefault(c => c.Id == contactoId) : model.Contactos.FirstOrDefault();
        if (contactoId.HasValue && model.Contacto == null) return NotFound();
        if (model.Contacto != null)
        {
            var otroId = model.Contacto.Id;
            model.Mensajes = await db.Mensajes.Include(m => m.ArchivoAdjunto).Where(m => (m.EmisorId == UsuarioId && m.ReceptorId == otroId) || (m.EmisorId == otroId && m.ReceptorId == UsuarioId)).OrderByDescending(m => m.Id).Take(200).OrderBy(m => m.Id).ToListAsync();
            await db.Mensajes.Where(m => m.ReceptorId == UsuarioId && m.EmisorId == otroId && !m.Leido).ExecuteUpdateAsync(s => s.SetProperty(m => m.Leido, true));
            model.Mensajes.Where(m => m.ReceptorId == UsuarioId).ToList().ForEach(m => m.MarcarComoLeido());
        }
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Enviar(int contactoId, string? texto, IFormFile? archivo)
    {
        var entrenadorId = Rol == Roles.Entrenador ? UsuarioId : contactoId;
        var alumnoId = Rol == Roles.Alumno ? UsuarioId : contactoId;
        if (!await acceso.Vinculados(entrenadorId, alumnoId)) return NotFound();
        if ((string.IsNullOrWhiteSpace(texto) && (archivo == null || archivo.Length == 0)) || texto?.Length > 2000)
        { Error("Ingresá un mensaje de hasta 2000 caracteres o adjuntá un archivo."); return RedirectToAction(nameof(Index), new { contactoId }); }
        ArchivoAdjunto? adjunto;
        try { adjunto = await archivos.Guardar(archivo, UsuarioId); }
        catch (InvalidOperationException ex) { Error(ex.Message); return RedirectToAction(nameof(Index), new { contactoId }); }
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var conversacion = await db.Conversaciones.FirstOrDefaultAsync(c => c.EntrenadorId == entrenadorId && c.AlumnoId == alumnoId);
        if (conversacion == null) conversacion = new Conversacion { EntrenadorId = entrenadorId, AlumnoId = alumnoId };
        db.Mensajes.Add(new Mensaje { Conversacion = conversacion, EmisorId = UsuarioId, ReceptorId = contactoId, Texto = texto?.Trim() ?? "", ArchivoAdjunto = adjunto });
        acceso.Notificar(contactoId, "Mensaje", "Tenés un mensaje nuevo en el chat.", "/Chat?contactoId=" + UsuarioId);
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        return RedirectToAction(nameof(Index), new { contactoId });
    }
    public async Task<IActionResult> Anteriores(int contactoId, int antesDe)
    {
        var entrenadorId = Rol == Roles.Entrenador ? UsuarioId : contactoId;
        var alumnoId = Rol == Roles.Alumno ? UsuarioId : contactoId;
        if (!await acceso.Vinculados(entrenadorId, alumnoId)) return NotFound();
        var mensajes = await db.Mensajes.Include(m => m.ArchivoAdjunto).Where(m => m.Id < antesDe && ((m.EmisorId == UsuarioId && m.ReceptorId == contactoId) || (m.ReceptorId == UsuarioId && m.EmisorId == contactoId))).OrderByDescending(m => m.Id).Take(200).OrderBy(m => m.Id).ToListAsync();
        ViewBag.ContactoId = contactoId;
        return View(mensajes);
    }
}
