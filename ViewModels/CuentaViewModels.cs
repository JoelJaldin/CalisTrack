using System.ComponentModel.DataAnnotations;
using CalisTrack.Models;

namespace CalisTrack.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu email."), EmailAddress(ErrorMessage = "Ingresá un email válido.")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Ingresá tu contraseña."), DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public class RegistroViewModel
{
    [Required, RegularExpression("Alumno|Entrenador", ErrorMessage = "Elegí Alumno o Entrenador.")] public string Rol { get; set; } = Roles.Alumno;
    [Required(ErrorMessage = "Ingresá tu nombre."), StringLength(100)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres."), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden."), DataType(DataType.Password)] public string ConfirmarPassword { get; set; } = "";
    [StringLength(500)] public string Objetivo { get; set; } = "";
}
public class PerfilViewModel
{
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [StringLength(500)] public string Objetivo { get; set; } = "";
    [DataType(DataType.Password)] public string? PasswordActual { get; set; }
    [StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string? PasswordNueva { get; set; }
}
public class UsuarioViewModel
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required] public string Rol { get; set; } = Roles.Alumno;
    public bool Activo { get; set; } = true;
    [StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string? Password { get; set; }
}
