using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Entrenador : Usuario
{
    public Entrenador() { Rol = Roles.Entrenador; }
}
