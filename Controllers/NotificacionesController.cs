using CalisTrack.Data;
using CalisTrack.Models;
using CalisTrack.Services;
using CalisTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Controllers;

public class NotificacionesController : BaseController
{
    private readonly CalisTrackContext db;
    public NotificacionesController(CalisTrackContext db) { this.db = db; }
    public async Task<IActionResult> Index(bool soloPendientes = false)
    {
        ViewBag.SoloPendientes = soloPendientes;
        ViewBag.Total = await db.Notificaciones.CountAsync(n => n.UsuarioId == UsuarioId);
        ViewBag.NoLeidas = await db.Notificaciones.CountAsync(n => n.UsuarioId == UsuarioId && !n.Leida);
        return View(await db.Notificaciones.Where(n => n.UsuarioId == UsuarioId && (!soloPendientes || !n.Leida)).OrderByDescending(n => n.FechaHora).Take(200).ToListAsync());
    }
    [HttpPost]
    public async Task<IActionResult> Leer(int id)
    {
        var n = await db.Notificaciones.FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == UsuarioId);
        if (n == null) return NotFound();
        n.MarcarComoLeida(); await db.SaveChangesAsync();
        return Url.IsLocalUrl(n.Enlace) ? LocalRedirect(n.Enlace) : RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> LeerTodas()
    {
        await db.Notificaciones.Where(n => n.UsuarioId == UsuarioId && !n.Leida).ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        Exito("Todas las notificaciones están leídas."); return RedirectToAction(nameof(Index));
    }
}
