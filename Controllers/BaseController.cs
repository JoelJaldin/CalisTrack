using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CalisTrack.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string Rol => User.FindFirstValue(ClaimTypes.Role)!;
    protected void Exito(string mensaje) { TempData["Exito"] = mensaje; }
    protected void Error(string mensaje) { TempData["Error"] = mensaje; }
}
