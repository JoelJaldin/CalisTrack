using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Alumno : Usuario
{
    [StringLength(500)] public string Objetivo { get; set; } = "";
    public Alumno() { Rol = Roles.Alumno; }
}
