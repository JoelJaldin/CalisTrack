using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Conversacion
{
    public int Id { get; set; }
    public int EntrenadorId { get; set; }
    public Usuario Entrenador { get; set; } = null!;
    public int AlumnoId { get; set; }
    public Usuario Alumno { get; set; } = null!;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public List<Mensaje> Mensajes { get; set; } = new();
}
