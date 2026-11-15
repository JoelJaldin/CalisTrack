using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    [StringLength(30)] public string Tipo { get; set; } = "";
    [StringLength(300)] public string Texto { get; set; } = "";
    [StringLength(200)] public string Enlace { get; set; } = "";
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public bool Leida { get; set; }
    public void MarcarComoLeida() { Leida = true; }
}
