using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Entrenamiento
{
    public int Id { get; set; }
    public int AsignacionRutinaId { get; set; }
    public AsignacionRutina AsignacionRutina { get; set; } = null!;
    public DateTime Fecha { get; set; }
    [StringLength(2000)] public string Comentario { get; set; } = "";
    [StringLength(100)] public string NombreRutina { get; set; } = "";
    public bool Finalizado { get; set; }
    public List<RegistroSerie> Registros { get; set; } = new();
    public List<Devolucion> Devoluciones { get; set; } = new();
    public void Finalizar() { Finalizado = true; }
}
