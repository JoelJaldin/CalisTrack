using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Rutina
{
    public int Id { get; set; }
    public int EntrenadorId { get; set; }
    public Usuario Entrenador { get; set; } = null!;
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activa { get; set; } = true;
    public List<RutinaEjercicio> Ejercicios { get; set; } = new();
    public void DarDeBaja() { Activa = false; }
}
