using System.ComponentModel.DataAnnotations;

namespace CalisTrack.Models;

public class Ejercicio
{
    public int Id { get; set; }
    public int EntrenadorId { get; set; }
    public Usuario Entrenador { get; set; } = null!;
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [StringLength(2000)] public string Descripcion { get; set; } = "";
    [StringLength(40)] public string GrupoMuscular { get; set; } = "General";
    public bool Activo { get; set; } = true;
    public List<MaterialTecnico> Materiales { get; set; } = new();
}
