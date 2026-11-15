using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class ArchivoAdjunto
{
    public int Id { get; set; }
    public int PropietarioId { get; set; }
    public Usuario Propietario { get; set; } = null!;
    [StringLength(200)] public string Nombre { get; set; } = "";
    [StringLength(100)] public string Tipo { get; set; } = "";
    [StringLength(100)] public string Ruta { get; set; } = "";
    public long Tamano { get; set; }
}
