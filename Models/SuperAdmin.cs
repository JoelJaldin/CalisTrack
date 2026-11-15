using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class SuperAdmin : Usuario
{
    public SuperAdmin() { Rol = Roles.SuperAdmin; }
}
