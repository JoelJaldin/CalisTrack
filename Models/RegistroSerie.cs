using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class RegistroSerie
{
    public int Id { get; set; }
    public int EntrenamientoId { get; set; }
    public Entrenamiento Entrenamiento { get; set; } = null!;
    public int RutinaEjercicioId { get; set; }
    public RutinaEjercicio RutinaEjercicio { get; set; } = null!;
    public int NumeroSerie { get; set; }
    [Range(0, 10000)] public int Repeticiones { get; set; }
    [Range(0, 86400)] public int? TiempoSegundos { get; set; }
    public bool Completada { get; set; }
    [StringLength(100)] public string NombreEjercicio { get; set; } = "";
    public int RepeticionesObjetivo { get; set; }
    public int DescansoSegundos { get; set; }
}
