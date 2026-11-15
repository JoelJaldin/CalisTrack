using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public abstract class Usuario
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, StringLength(20)] public string Rol { get; set; } = "";
    public bool Activo { get; set; } = true;
    public Guid VersionSesion { get; set; } = Guid.NewGuid();
    public void ActualizarPerfil(string nombre, string email) { Nombre = nombre; Email = email; }
}
