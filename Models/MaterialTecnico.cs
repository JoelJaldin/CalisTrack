using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class MaterialTecnico
{
    public int Id { get; set; }
    public int EjercicioId { get; set; }
    public Ejercicio Ejercicio { get; set; } = null!;
    [StringLength(20)] public string Tipo { get; set; } = "Enlace";
    [StringLength(2048)] public string ArchivoUrl { get; set; } = "";
    [StringLength(200)] public string Descripcion { get; set; } = "";
    public int? ArchivoAdjuntoId { get; set; }
    public ArchivoAdjunto? ArchivoAdjunto { get; set; }
}
