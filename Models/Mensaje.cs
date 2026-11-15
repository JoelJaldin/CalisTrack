using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Mensaje
{
    public int Id { get; set; }
    public int ConversacionId { get; set; }
    public Conversacion Conversacion { get; set; } = null!;
    public int EmisorId { get; set; }
    public Usuario Emisor { get; set; } = null!;
    public int ReceptorId { get; set; }
    public Usuario Receptor { get; set; } = null!;
    [StringLength(2000)] public string Texto { get; set; } = "";
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; }
    public int? ArchivoAdjuntoId { get; set; }
    public ArchivoAdjunto? ArchivoAdjunto { get; set; }
    public void MarcarComoLeido() { Leido = true; }
}
