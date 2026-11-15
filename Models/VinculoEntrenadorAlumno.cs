using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class VinculoEntrenadorAlumno
{
    public int Id { get; set; }
    public int EntrenadorId { get; set; }
    public Usuario Entrenador { get; set; } = null!;
    public int AlumnoId { get; set; }
    public Usuario Alumno { get; set; } = null!;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
    public void Desactivar() { Activo = false; }
}
