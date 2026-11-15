using CalisTrack.Models;
using CalisTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CalisTrack.Data;

public static class DatosIniciales
{
    public static async Task CargarAsync(CalisTrackContext db, PasswordHasher<Usuario> hasher)
    {
        if (await db.Usuarios.AnyAsync()) return;
        await using var transaction = await db.Database.BeginTransactionAsync();
        var admin = new SuperAdmin { Nombre = "Admin CalisTrack", Email = "admin@calistrack.local" };
        var entrenador = new Entrenador { Nombre = "Santiago Torres", Email = "entrenador@calistrack.local" };
        var alumno = new Alumno { Nombre = "Lucas Martínez", Email = "alumno@calistrack.local", Objetivo = "Mejorar mi fuerza y conseguir mi primera dominada estricta." };
        var sofia = new Alumno { Nombre = "Sofía García", Email = "sofia@calistrack.local", Objetivo = "Ganar fuerza y constancia." };
        var mateo = new Alumno { Nombre = "Mateo López", Email = "mateo@calistrack.local", Objetivo = "Aprender los básicos de calistenia." };
        foreach (var usuario in new Usuario[] { admin, entrenador, alumno, sofia, mateo })
        {
            usuario.PasswordHash = hasher.HashPassword(usuario, "CalisTrack123!");
            db.Usuarios.Add(usuario);
        }
        await db.SaveChangesAsync();
        db.Vinculos.AddRange(new VinculoEntrenadorAlumno { EntrenadorId = entrenador.Id, AlumnoId = alumno.Id }, new VinculoEntrenadorAlumno { EntrenadorId = entrenador.Id, AlumnoId = sofia.Id });
        var ejercicios = new[]
        {
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Dominadas", GrupoMuscular = "Espalda", Descripcion = "Comenzá colgado de la barra. Activá las escápulas y subí de forma controlada, sin balanceos. Adaptá la dificultad con asistencia si lo necesitás." },
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Flexiones", GrupoMuscular = "Pecho", Descripcion = "Apoyá las manos al ancho de hombros y mantené el cuerpo alineado. Bajá con control y empujá para volver a la posición inicial." },
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Fondos en paralelas", GrupoMuscular = "Tríceps", Descripcion = "Mantené los hombros estables. Flexioná los codos dentro de un rango cómodo y empujá para volver a subir." },
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Sentadillas", GrupoMuscular = "Piernas", Descripcion = "Separá los pies al ancho de hombros. Flexioná caderas y rodillas manteniendo los talones apoyados. Volvé a subir de manera controlada." },
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Plancha", GrupoMuscular = "Core", Descripcion = "Apoyá antebrazos y puntas de pies. Mantené el abdomen activo y el cuerpo alineado durante el tiempo indicado." },
            new Ejercicio { EntrenadorId = entrenador.Id, Nombre = "Remo australiano", GrupoMuscular = "Espalda", Descripcion = "Debajo de una barra baja, mantené el cuerpo alineado y llevá el pecho hacia la barra. Bajá sin perder el control." }
        };
        db.Ejercicios.AddRange(ejercicios);
        await db.SaveChangesAsync();
        var rutina = new Rutina { EntrenadorId = entrenador.Id, Nombre = "Fuerza · Tren superior", Descripcion = "Una sesión para construir una base sólida. Priorizá la técnica y respetá los descansos." };
        for (var i = 0; i < 3; i++) rutina.Ejercicios.Add(new RutinaEjercicio { EjercicioId = ejercicios[i].Id, Orden = i + 1, Series = 3, Repeticiones = i == 0 ? 6 : 10, DescansoSegundos = 90, Observaciones = "Movimiento lento y controlado." });
        var baseRutina = new Rutina { EntrenadorId = entrenador.Id, Nombre = "Primeros pasos · Full body", Descripcion = "Trabajo de cuerpo completo para comenzar con constancia." };
        baseRutina.Ejercicios.Add(new RutinaEjercicio { EjercicioId = ejercicios[3].Id, Orden = 1, Series = 3, Repeticiones = 12, DescansoSegundos = 60 });
        baseRutina.Ejercicios.Add(new RutinaEjercicio { EjercicioId = ejercicios[5].Id, Orden = 2, Series = 3, Repeticiones = 8, DescansoSegundos = 90 });
        db.Rutinas.AddRange(rutina, baseRutina);
        await db.SaveChangesAsync();
        var asignacion = new AsignacionRutina { AlumnoId = alumno.Id, RutinaId = rutina.Id, FechaDesde = Fecha.Hoy.AddDays(-35) };
        db.Asignaciones.AddRange(asignacion, new AsignacionRutina { AlumnoId = sofia.Id, RutinaId = baseRutina.Id, FechaDesde = Fecha.Hoy.AddDays(-14) });
        await db.SaveChangesAsync();
        for (var i = 0; i < 8; i++)
        {
            var sesion = new Entrenamiento { AsignacionRutinaId = asignacion.Id, Fecha = Fecha.Hoy.AddDays(-28 + i * 3), NombreRutina = rutina.Nombre, Finalizado = true, Comentario = i == 7 ? "Hoy pude completar todas las series con mejor control." : "Sesión completada. Seguimos sumando." };
            foreach (var item in rutina.Ejercicios)
                for (var n = 1; n <= item.Series; n++)
                    sesion.Registros.Add(new RegistroSerie { RutinaEjercicioId = item.Id, NumeroSerie = n, Repeticiones = Math.Max(1, item.Repeticiones - 3 + i / 2), RepeticionesObjetivo = item.Repeticiones, NombreEjercicio = ejercicios.First(e => e.Id == item.EjercicioId).Nombre, DescansoSegundos = item.DescansoSegundos, Completada = true });
            if (i == 7) sesion.Devoluciones.Add(new Devolucion { EntrenadorId = entrenador.Id, Texto = "¡Buen trabajo, Lucas! Se nota la constancia. En la próxima sesión mantené el mismo control en las últimas repeticiones." });
            db.Entrenamientos.Add(sesion);
        }
        var conversacion = new Conversacion { EntrenadorId = entrenador.Id, AlumnoId = alumno.Id };
        conversacion.Mensajes.Add(new Mensaje { EmisorId = entrenador.Id, ReceptorId = alumno.Id, Texto = "¡Hola, Lucas! Ya tenés tu rutina lista. Contame cómo te sentís después de entrenar.", FechaHora = DateTime.UtcNow.AddHours(-2) });
        conversacion.Mensajes.Add(new Mensaje { EmisorId = alumno.Id, ReceptorId = entrenador.Id, Texto = "¡Gracias, profe! Hoy voy a enfocarme en la técnica de las dominadas.", FechaHora = DateTime.UtcNow.AddHours(-1), Leido = true });
        db.Conversaciones.Add(conversacion);
        db.Notificaciones.AddRange(new Notificacion { UsuarioId = alumno.Id, Tipo = "Rutina", Texto = "Tu rutina de fuerza ya está disponible.", Enlace = "/Rutinas/MiRutina" }, new Notificacion { UsuarioId = alumno.Id, Tipo = "Mensaje", Texto = "Santiago te envió un mensaje.", Enlace = "/Chat" }, new Notificacion { UsuarioId = entrenador.Id, Tipo = "Entrenamiento", Texto = "Lucas completó una nueva sesión.", Enlace = "/Historial?alumnoId=" + alumno.Id });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
