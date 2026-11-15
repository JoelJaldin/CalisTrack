using System.ComponentModel.DataAnnotations;
using CalisTrack.Models;

namespace CalisTrack.ViewModels;

public class EjercicioViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Ingresá el nombre del ejercicio."), StringLength(100)] public string Nombre { get; set; } = "";
    [StringLength(2000)] public string Descripcion { get; set; } = "";
    [StringLength(40)] public string GrupoMuscular { get; set; } = "General";
    [StringLength(2048)] public string? Enlace { get; set; }
    public IFormFile? Archivo { get; set; }
    public List<MaterialTecnico> Materiales { get; set; } = new();
}
public class RutinaViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Ingresá el nombre de la rutina."), StringLength(100)] public string Nombre { get; set; } = "";
    [StringLength(1000)] public string Descripcion { get; set; } = "";
    public List<FilaRutina> Ejercicios { get; set; } = new();
    public List<Ejercicio> Biblioteca { get; set; } = new();
}
public class FilaRutina
{
    public int EjercicioId { get; set; }
    [Range(1, 20, ErrorMessage = "Las series deben estar entre 1 y 20.")] public int Series { get; set; } = 3;
    [Range(1, 1000, ErrorMessage = "Las repeticiones deben ser mayores a cero.")] public int Repeticiones { get; set; } = 10;
    [Range(0, 3600)] public int DescansoSegundos { get; set; } = 60;
    [StringLength(500)] public string Observaciones { get; set; } = "";
}
public class AsignarViewModel
{
    public int RutinaId { get; set; }
    public string NombreRutina { get; set; } = "";
    public List<int> AlumnoIds { get; set; } = new();
    [DataType(DataType.Date)] public DateTime FechaDesde { get; set; } = Services.Fecha.Hoy;
    [DataType(DataType.Date)] public DateTime? FechaHasta { get; set; }
    public List<Usuario> Alumnos { get; set; } = new();
}
public class EntrenarViewModel
{
    public int Id { get; set; }
    public string NombreRutina { get; set; } = "";
    [DataType(DataType.Date)] public DateTime Fecha { get; set; }
    [StringLength(2000)] public string Comentario { get; set; } = "";
    public List<FilaSerie> Series { get; set; } = new();
}
public class FilaSerie
{
    public int RutinaEjercicioId { get; set; }
    public int Id { get; set; }
    public string NombreEjercicio { get; set; } = "";
    public int NumeroSerie { get; set; }
    public int Objetivo { get; set; }
    public int Descanso { get; set; }
    [Range(0, 10000)] public int Repeticiones { get; set; }
    [Range(0, 86400)] public int? TiempoSegundos { get; set; }
    public bool Completada { get; set; }
}
public class HistorialViewModel
{
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = "";
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? EjercicioId { get; set; }
    public int? RutinaId { get; set; }
    public List<Entrenamiento> Entrenamientos { get; set; } = new();
    public List<Ejercicio> Ejercicios { get; set; } = new();
    public List<Rutina> Rutinas { get; set; } = new();
    public List<SemanaProgreso> Semanas { get; set; } = new();
}
public class SemanaProgreso
{
    public DateTime Inicio { get; set; }
    public int Sesiones { get; set; }
    public int Repeticiones { get; set; }
    public int Segundos { get; set; }
}
public class ChatViewModel
{
    public List<Usuario> Contactos { get; set; } = new();
    public Usuario? Contacto { get; set; }
    public List<Mensaje> Mensajes { get; set; } = new();
    public int UsuarioId { get; set; }
}
public class InicioViewModel
{
    public string Nombre { get; set; } = "";
    public int Alumnos { get; set; }
    public int Rutinas { get; set; }
    public int Ejercicios { get; set; }
    public int Sesiones { get; set; }
    public int Usuarios { get; set; }
    public int NoLeidas { get; set; }
    public int Entrenadores { get; set; }
    public int Inactivos { get; set; }
    public int Pendientes { get; set; }
    public int SesionesSemana { get; set; }
    public int RepeticionesSemana { get; set; }
    public decimal? Variacion { get; set; }
    public Devolucion? UltimaDevolucion { get; set; }
    public List<SemanaProgreso> Semanas { get; set; } = new();
    public List<Notificacion> Notificaciones { get; set; } = new();
    public List<Usuario> UltimosUsuarios { get; set; } = new();
    public List<AsignacionRutina> Asignaciones { get; set; } = new();
    public List<Entrenamiento> Recientes { get; set; } = new();
}
