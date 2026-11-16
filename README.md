# CalisTrack

Aplicación de seguimiento de calistenia para PNT1. Los entrenadores pueden armar rutinas, asignarlas a sus alumnos y dejar devoluciones. Los alumnos registran sus entrenamientos y consultan su progreso. También incluye chat, notificaciones y administración de usuarios.

Está desarrollada con ASP.NET Core MVC, C#, Razor y Entity Framework Core con SQL Server.

## Ejecutar en esta computadora

Las herramientas ya están configuradas. Necesitás dos terminales: una para la base de datos y otra para la aplicación.

En la primera, iniciá SQL Server:

```bash
cd /home/yow/jow/CalisTrack
./scripts/iniciar-db.sh
```

Esperá a que aparezca `SQL Server is now ready for client connections`. Este comando inicia la base de datos; la web se levanta en el siguiente paso.

En la segunda terminal:

```bash
cd /home/yow/jow/CalisTrack
./iniciar.sh
```

Cuando aparezca `Now listening on: http://localhost:5080`, abrí [http://localhost:5080](http://localhost:5080).

Dejá ambas terminales abiertas mientras usás la app. Para detenerla, presioná `Ctrl+C` primero en la terminal de la aplicación y después en la de SQL Server. Los datos quedan guardados.

## Instalar en otra computadora

Necesitás el SDK de [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) y una instancia de [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads).

Cloná el proyecto y copiá el archivo de configuración de ejemplo:

```bash
git clone https://github.com/JoelJaldin/CalisTrack.git
cd CalisTrack
cp appsettings.Local.example.json appsettings.Local.json
```

Editá la conexión en `appsettings.Local.json` con el servidor, la base y las credenciales de tu instancia. El usuario de SQL Server necesita permisos para crear la base y aplicar las migraciones. Este archivo está excluido de Git para que las credenciales queden en tu equipo.

Con SQL Server funcionando, ejecutá:

```bash
dotnet restore
dotnet run
```

La aplicación aplica las migraciones al arrancar y queda disponible en [http://localhost:5080](http://localhost:5080). Si usás Visual Studio, abrí `CalisTrack.csproj` y elegí el perfil `CalisTrack`.

## Cuentas de prueba

Se crean en desarrollo cuando la base no tiene usuarios y `SeedDemo` está activado. Todas usan la contraseña `CalisTrack123!`.

| Cuenta | Email |
| --- | --- |
| Super Admin | admin@calistrack.local |
| Entrenador | entrenador@calistrack.local |
| Alumno | alumno@calistrack.local |
| Sofía, alumna vinculada | sofia@calistrack.local |
| Mateo, alumno disponible para vincular | mateo@calistrack.local |

Para probar el recorrido principal, entrá como entrenador, vinculá a Mateo y asignale una rutina. Después ingresá con la cuenta de Mateo para registrar un entrenamiento y revisar el progreso.

## Desarrollo

Para recargar la aplicación al modificar código, con la base iniciada:

```bash
./scripts/dotnet.sh watch run
```

Si cambiás el modelo de datos, creá una migración:

```bash
./scripts/dotnet.sh tool restore
./scripts/dotnet.sh ef migrations add NombreDelCambio
```

La migración se aplica la próxima vez que iniciás la app. También podés aplicarla con `./scripts/dotnet.sh ef database update`.

Si tenés el SDK instalado globalmente, podés reemplazar `./scripts/dotnet.sh` por `dotnet`.

## Pruebas

Con SQL Server iniciado y Python 3 disponible:

```bash
./scripts/dotnet.sh build tests/Database/Database.csproj
python3 tests/verificar.py
```

Las pruebas usan una base temporal y el puerto `5081`. Al terminar, eliminan sus datos de prueba sin modificar la base `CalisTrack`.

## Problemas al iniciar

- **La base está encendida pero no abre la web:** ejecutá `./iniciar.sh` en otra terminal.
- **No se puede conectar a SQL Server:** comprobá que esté iniciado y revisá la conexión en `appsettings.Local.json`. En esta computadora usa `127.0.0.1,14333`.
- **El puerto está ocupado:** revisá si ya hay una instancia ejecutándose antes de volver a iniciarla.
- **No se encuentra `dotnet`:** en esta computadora usá `./scripts/dotnet.sh`; en otra, instalá el SDK de .NET 10.
