namespace CalisTrack.Models;

public static class Roles
{
    public const string Alumno = "Alumno";
    public const string Entrenador = "Entrenador";
    public const string SuperAdmin = "SuperAdmin";
    public static readonly string[] Todos = [Alumno, Entrenador, SuperAdmin];
}
