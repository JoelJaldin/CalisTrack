using System;
using CalisTrack.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CalisTrack.Data.Migrations
{
    [DbContext(typeof(CalisTrackContext))]
    [Migration("20261115004415_Inicial")]
    partial class Inicial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CalisTrack.Models.ArchivoAdjunto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("PropietarioId")
                        .HasColumnType("int");

                    b.Property<string>("Ruta")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<long>("Tamano")
                        .HasColumnType("bigint");

                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("PropietarioId");

                    b.ToTable("Archivos");
                });

            modelBuilder.Entity("CalisTrack.Models.AsignacionRutina", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activa")
                        .HasColumnType("bit");

                    b.Property<int>("AlumnoId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaDesde")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("FechaHasta")
                        .HasColumnType("datetime2");

                    b.Property<int>("RutinaId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AlumnoId");

                    b.HasIndex("RutinaId");

                    b.ToTable("Asignaciones");
                });

            modelBuilder.Entity("CalisTrack.Models.Conversacion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AlumnoId")
                        .HasColumnType("int");

                    b.Property<int>("EntrenadorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AlumnoId");

                    b.HasIndex("EntrenadorId", "AlumnoId")
                        .IsUnique();

                    b.ToTable("Conversaciones");
                });

            modelBuilder.Entity("CalisTrack.Models.Devolucion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ArchivoAdjuntoId")
                        .HasColumnType("int");

                    b.Property<int>("EntrenadorId")
                        .HasColumnType("int");

                    b.Property<int>("EntrenamientoId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaHora")
                        .HasColumnType("datetime2");

                    b.Property<string>("Texto")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.HasKey("Id");

                    b.HasIndex("ArchivoAdjuntoId");

                    b.HasIndex("EntrenadorId");

                    b.HasIndex("EntrenamientoId");

                    b.ToTable("Devoluciones");
                });

            modelBuilder.Entity("CalisTrack.Models.Ejercicio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<int>("EntrenadorId")
                        .HasColumnType("int");

                    b.Property<string>("GrupoMuscular")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("EntrenadorId");

                    b.ToTable("Ejercicios");
                });

            modelBuilder.Entity("CalisTrack.Models.Entrenamiento", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AsignacionRutinaId")
                        .HasColumnType("int");

                    b.Property<string>("Comentario")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<DateTime>("Fecha")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Finalizado")
                        .HasColumnType("bit");

                    b.Property<string>("NombreRutina")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("AsignacionRutinaId")
                        .IsUnique()
                        .HasFilter("[Finalizado] = 0");

                    b.ToTable("Entrenamientos");
                });

            modelBuilder.Entity("CalisTrack.Models.MaterialTecnico", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ArchivoAdjuntoId")
                        .HasColumnType("int");

                    b.Property<string>("ArchivoUrl")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("EjercicioId")
                        .HasColumnType("int");

                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("ArchivoAdjuntoId");

                    b.HasIndex("EjercicioId");

                    b.ToTable("Materiales");
                });

            modelBuilder.Entity("CalisTrack.Models.Mensaje", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ArchivoAdjuntoId")
                        .HasColumnType("int");

                    b.Property<int>("ConversacionId")
                        .HasColumnType("int");

                    b.Property<int>("EmisorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaHora")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Leido")
                        .HasColumnType("bit");

                    b.Property<int>("ReceptorId")
                        .HasColumnType("int");

                    b.Property<string>("Texto")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.HasKey("Id");

                    b.HasIndex("ArchivoAdjuntoId");

                    b.HasIndex("ConversacionId");

                    b.HasIndex("EmisorId");

                    b.HasIndex("ReceptorId");

                    b.ToTable("Mensajes");
                });

            modelBuilder.Entity("CalisTrack.Models.Notificacion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Enlace")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("FechaHora")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Leida")
                        .HasColumnType("bit");

                    b.Property<string>("Texto")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("Notificaciones");
                });

            modelBuilder.Entity("CalisTrack.Models.RegistroSerie", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Completada")
                        .HasColumnType("bit");

                    b.Property<int>("DescansoSegundos")
                        .HasColumnType("int");

                    b.Property<int>("EntrenamientoId")
                        .HasColumnType("int");

                    b.Property<string>("NombreEjercicio")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("NumeroSerie")
                        .HasColumnType("int");

                    b.Property<int>("Repeticiones")
                        .HasColumnType("int");

                    b.Property<int>("RepeticionesObjetivo")
                        .HasColumnType("int");

                    b.Property<int>("RutinaEjercicioId")
                        .HasColumnType("int");

                    b.Property<int?>("TiempoSegundos")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RutinaEjercicioId");

                    b.HasIndex("EntrenamientoId", "RutinaEjercicioId", "NumeroSerie")
                        .IsUnique();

                    b.ToTable("Registros");
                });

            modelBuilder.Entity("CalisTrack.Models.Rutina", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activa")
                        .HasColumnType("bit");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<int>("EntrenadorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaCreacion")
                        .HasColumnType("datetime2");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("EntrenadorId");

                    b.ToTable("Rutinas");
                });

            modelBuilder.Entity("CalisTrack.Models.RutinaEjercicio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<int>("DescansoSegundos")
                        .HasColumnType("int");

                    b.Property<int>("EjercicioId")
                        .HasColumnType("int");

                    b.Property<string>("Observaciones")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("Orden")
                        .HasColumnType("int");

                    b.Property<int>("Repeticiones")
                        .HasColumnType("int");

                    b.Property<int>("RutinaId")
                        .HasColumnType("int");

                    b.Property<int>("Series")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EjercicioId");

                    b.HasIndex("RutinaId");

                    b.ToTable("RutinaEjercicios");
                });

            modelBuilder.Entity("CalisTrack.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Rol")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<Guid>("VersionSesion")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Usuarios");

                    b.HasDiscriminator<string>("Rol").HasValue("Usuario");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("CalisTrack.Models.VinculoEntrenadorAlumno", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Activo")
                        .HasColumnType("bit");

                    b.Property<int>("AlumnoId")
                        .HasColumnType("int");

                    b.Property<int>("EntrenadorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaAlta")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("AlumnoId")
                        .IsUnique()
                        .HasFilter("[Activo] = 1");

                    b.HasIndex("EntrenadorId", "AlumnoId")
                        .IsUnique();

                    b.ToTable("Vinculos");
                });

            modelBuilder.Entity("CalisTrack.Models.Alumno", b =>
                {
                    b.HasBaseType("CalisTrack.Models.Usuario");

                    b.Property<string>("Objetivo")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.HasDiscriminator().HasValue("Alumno");
                });

            modelBuilder.Entity("CalisTrack.Models.Entrenador", b =>
                {
                    b.HasBaseType("CalisTrack.Models.Usuario");

                    b.HasDiscriminator().HasValue("Entrenador");
                });

            modelBuilder.Entity("CalisTrack.Models.SuperAdmin", b =>
                {
                    b.HasBaseType("CalisTrack.Models.Usuario");

                    b.HasDiscriminator().HasValue("SuperAdmin");
                });

            modelBuilder.Entity("CalisTrack.Models.ArchivoAdjunto", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Propietario")
                        .WithMany()
                        .HasForeignKey("PropietarioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Propietario");
                });

            modelBuilder.Entity("CalisTrack.Models.AsignacionRutina", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Alumno")
                        .WithMany()
                        .HasForeignKey("AlumnoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Rutina", "Rutina")
                        .WithMany()
                        .HasForeignKey("RutinaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Alumno");

                    b.Navigation("Rutina");
                });

            modelBuilder.Entity("CalisTrack.Models.Conversacion", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Alumno")
                        .WithMany()
                        .HasForeignKey("AlumnoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Usuario", "Entrenador")
                        .WithMany()
                        .HasForeignKey("EntrenadorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Alumno");

                    b.Navigation("Entrenador");
                });

            modelBuilder.Entity("CalisTrack.Models.Devolucion", b =>
                {
                    b.HasOne("CalisTrack.Models.ArchivoAdjunto", "ArchivoAdjunto")
                        .WithMany()
                        .HasForeignKey("ArchivoAdjuntoId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("CalisTrack.Models.Usuario", "Entrenador")
                        .WithMany()
                        .HasForeignKey("EntrenadorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Entrenamiento", "Entrenamiento")
                        .WithMany("Devoluciones")
                        .HasForeignKey("EntrenamientoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ArchivoAdjunto");

                    b.Navigation("Entrenador");

                    b.Navigation("Entrenamiento");
                });

            modelBuilder.Entity("CalisTrack.Models.Ejercicio", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Entrenador")
                        .WithMany()
                        .HasForeignKey("EntrenadorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Entrenador");
                });

            modelBuilder.Entity("CalisTrack.Models.Entrenamiento", b =>
                {
                    b.HasOne("CalisTrack.Models.AsignacionRutina", "AsignacionRutina")
                        .WithMany()
                        .HasForeignKey("AsignacionRutinaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AsignacionRutina");
                });

            modelBuilder.Entity("CalisTrack.Models.MaterialTecnico", b =>
                {
                    b.HasOne("CalisTrack.Models.ArchivoAdjunto", "ArchivoAdjunto")
                        .WithMany()
                        .HasForeignKey("ArchivoAdjuntoId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("CalisTrack.Models.Ejercicio", "Ejercicio")
                        .WithMany("Materiales")
                        .HasForeignKey("EjercicioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ArchivoAdjunto");

                    b.Navigation("Ejercicio");
                });

            modelBuilder.Entity("CalisTrack.Models.Mensaje", b =>
                {
                    b.HasOne("CalisTrack.Models.ArchivoAdjunto", "ArchivoAdjunto")
                        .WithMany()
                        .HasForeignKey("ArchivoAdjuntoId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("CalisTrack.Models.Conversacion", "Conversacion")
                        .WithMany("Mensajes")
                        .HasForeignKey("ConversacionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Usuario", "Emisor")
                        .WithMany()
                        .HasForeignKey("EmisorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Usuario", "Receptor")
                        .WithMany()
                        .HasForeignKey("ReceptorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ArchivoAdjunto");

                    b.Navigation("Conversacion");

                    b.Navigation("Emisor");

                    b.Navigation("Receptor");
                });

            modelBuilder.Entity("CalisTrack.Models.Notificacion", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("CalisTrack.Models.RegistroSerie", b =>
                {
                    b.HasOne("CalisTrack.Models.Entrenamiento", "Entrenamiento")
                        .WithMany("Registros")
                        .HasForeignKey("EntrenamientoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.RutinaEjercicio", "RutinaEjercicio")
                        .WithMany()
                        .HasForeignKey("RutinaEjercicioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Entrenamiento");

                    b.Navigation("RutinaEjercicio");
                });

            modelBuilder.Entity("CalisTrack.Models.Rutina", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Entrenador")
                        .WithMany()
                        .HasForeignKey("EntrenadorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Entrenador");
                });

            modelBuilder.Entity("CalisTrack.Models.RutinaEjercicio", b =>
                {
                    b.HasOne("CalisTrack.Models.Ejercicio", "Ejercicio")
                        .WithMany()
                        .HasForeignKey("EjercicioId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Rutina", "Rutina")
                        .WithMany("Ejercicios")
                        .HasForeignKey("RutinaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Ejercicio");

                    b.Navigation("Rutina");
                });

            modelBuilder.Entity("CalisTrack.Models.VinculoEntrenadorAlumno", b =>
                {
                    b.HasOne("CalisTrack.Models.Usuario", "Alumno")
                        .WithMany()
                        .HasForeignKey("AlumnoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CalisTrack.Models.Usuario", "Entrenador")
                        .WithMany()
                        .HasForeignKey("EntrenadorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Alumno");

                    b.Navigation("Entrenador");
                });

            modelBuilder.Entity("CalisTrack.Models.Conversacion", b =>
                {
                    b.Navigation("Mensajes");
                });

            modelBuilder.Entity("CalisTrack.Models.Ejercicio", b =>
                {
                    b.Navigation("Materiales");
                });

            modelBuilder.Entity("CalisTrack.Models.Entrenamiento", b =>
                {
                    b.Navigation("Devoluciones");

                    b.Navigation("Registros");
                });

            modelBuilder.Entity("CalisTrack.Models.Rutina", b =>
                {
                    b.Navigation("Ejercicios");
                });
#pragma warning restore 612, 618
        }
    }
}
