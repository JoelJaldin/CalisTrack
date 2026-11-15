using CalisTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Data;

public class CalisTrackContext : DbContext
{
    public CalisTrackContext(DbContextOptions<CalisTrackContext> options) : base(options) { }
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<VinculoEntrenadorAlumno> Vinculos => Set<VinculoEntrenadorAlumno>();
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<MaterialTecnico> Materiales => Set<MaterialTecnico>();
    public DbSet<Rutina> Rutinas => Set<Rutina>();
    public DbSet<RutinaEjercicio> RutinaEjercicios => Set<RutinaEjercicio>();
    public DbSet<AsignacionRutina> Asignaciones => Set<AsignacionRutina>();
    public DbSet<Entrenamiento> Entrenamientos => Set<Entrenamiento>();
    public DbSet<RegistroSerie> Registros => Set<RegistroSerie>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<ArchivoAdjunto> Archivos => Set<ArchivoAdjunto>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Usuario>().HasDiscriminator(u => u.Rol)
            .HasValue<Alumno>(Roles.Alumno)
            .HasValue<Entrenador>(Roles.Entrenador)
            .HasValue<SuperAdmin>(Roles.SuperAdmin);
        model.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        model.Entity<VinculoEntrenadorAlumno>().HasIndex(v => new { v.EntrenadorId, v.AlumnoId }).IsUnique();
        model.Entity<VinculoEntrenadorAlumno>().HasIndex(v => v.AlumnoId).IsUnique().HasFilter("[Activo] = 1");
        model.Entity<Conversacion>().HasIndex(c => new { c.EntrenadorId, c.AlumnoId }).IsUnique();
        model.Entity<RegistroSerie>().HasIndex(r => new { r.EntrenamientoId, r.RutinaEjercicioId, r.NumeroSerie }).IsUnique();
        model.Entity<Entrenamiento>().HasIndex(e => e.AsignacionRutinaId).IsUnique().HasFilter("[Finalizado] = 0");
        foreach (var relation in model.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relation.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
