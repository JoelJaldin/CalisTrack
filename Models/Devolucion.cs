using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Devolucion
{
    public int Id { get; set; }
    public int EntrenamientoId { get; set; }
    public Entrenamiento Entrenamiento { get; set; } = null!;
    public int EntrenadorId { get; set; }
    public Usuario Entrenador { get; set; } = null!;
    [Required, StringLength(2000)] public string Texto { get; set; } = "";
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public int? ArchivoAdjuntoId { get; set; }
    public ArchivoAdjunto? ArchivoAdjunto { get; set; }
}
