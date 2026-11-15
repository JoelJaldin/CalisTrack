using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class RutinaEjercicio
{
    public int Id { get; set; }
    public int RutinaId { get; set; }
    public Rutina Rutina { get; set; } = null!;
    public int EjercicioId { get; set; }
    public Ejercicio Ejercicio { get; set; } = null!;
    public int Orden { get; set; }
    [Range(1, 20)] public int Series { get; set; }
    [Range(1, 1000)] public int Repeticiones { get; set; }
    [Range(0, 3600)] public int DescansoSegundos { get; set; }
    [StringLength(500)] public string Observaciones { get; set; } = "";
    public bool Activo { get; set; } = true;
}
