using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class AsignacionRutina
{
    public int Id { get; set; }
    public int RutinaId { get; set; }
    public Rutina Rutina { get; set; } = null!;
    public int AlumnoId { get; set; }
    public Usuario Alumno { get; set; } = null!;
    public DateTime FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public bool Activa { get; set; } = true;
    public void Finalizar() { Activa = false; }
}
