# CalisTrack

Aplicación de seguimiento de calistenia para PNT1, desarrollada con ASP.NET Core MVC, C#, vistas Razor, HTML5, CSS, JavaScript y Entity Framework Core con SQL Server. Incluye las diez funcionalidades del documento del proyecto y el modelo de clases con herencia de Usuario.

La interfaz está adaptada al PDF de 28 pantallas exportado de Figma: fondo oscuro `#0B0D12`, paneles `#161A22`, barra lateral `#0F1218` y verde lima `#C8FF2E`. Incluye la portada de acceso, navegación por rol, tablas de alumnos y rutinas, técnica de ejercicios, entrenamiento por ejercicio, progreso, devoluciones, chat y administración. Inter y Montserrat se sirven desde el proyecto, con sus licencias incluidas.

La comprobación visual final en un navegador de escritorio y móvil queda pendiente: el navegador automatizado no estuvo disponible. Las pruebas HTTP y SQL verifican los recorridos y permisos, pero no sustituyen esa revisión visual.

## Probar en esta computadora

Las herramientas ya están preparadas en `/home/yow/jow/.calistrack-tools`. No necesitás instalar Node ni ejecutar npm.

Abrí una terminal para la base de datos:

```bash
cd /home/yow/jow/CalisTrack
./scripts/iniciar-db.sh
```

Cuando aparezca `SQL Server is now ready for client connections`, dejá esa terminal abierta. Si la base ya está funcionando, no vuelvas a iniciarla.

En una segunda terminal:

```bash
cd /home/yow/jow/CalisTrack
./iniciar.sh
```

Abrí **http://localhost:5080**. El primer inicio aplica las migraciones y carga los datos de ejemplo si no hay usuarios. Los siguientes inicios conservan los cambios; la seed no los sobrescribe.

Para detener la aplicación, presioná `Ctrl+C` en su terminal. Para detener la base, hacé lo mismo en la terminal de SQL Server, después de detener la aplicación. Los datos permanecen guardados.

Para modificar código durante las pruebas y recargar automáticamente:

```bash
./scripts/dotnet.sh watch run
```

## Cuentas de ejemplo

Todas usan la contraseña `CalisTrack123!`.

| Rol | Email |
| --- | --- |
| Super Admin | admin@calistrack.local |
| Entrenador | entrenador@calistrack.local |
| Alumno | alumno@calistrack.local |
| Alumna vinculada | sofia@calistrack.local |
| Alumno disponible para vincular | mateo@calistrack.local |

Los botones demo del login completan el formulario; para ingresar hay que pulsar Iniciar sesión. Solo aparecen en desarrollo. El registro público permite elegir Alumno o Entrenador. Las cuentas Super Admin se crean desde la administración. El Super Admin crea entrenadores, alumnos y administradores, y puede cambiar sus roles.

## Recorrido sugerido

1. Entrá como entrenador. En Biblioteca, creá un ejercicio y adjuntá una imagen, video o PDF, o agregá un enlace HTTPS.
2. Creá una rutina con ejercicios, series, repeticiones, descansos y observaciones. Podés ordenar los ejercicios con las flechas.
3. En Mis alumnos, vinculá a Mateo o a un alumno que se haya registrado. Asignale la rutina con su período de vigencia.
4. Salí e ingresá como ese alumno. Abrí Mi rutina, consultá el material y comenzá el entrenamiento.
5. Cargá repeticiones y tiempos. Marcá las series realizadas. Guardá un avance para continuar después o finalizá la sesión.
6. Revisá Historial y progreso. Filtrá por fechas, rutina o ejercicio. El gráfico se calcula con las sesiones y series efectivamente guardadas.
7. Volvé como entrenador. Abrí el perfil del alumno y su historial. Escribí una devolución y, si querés, adjuntá material.
8. Probá el chat desde ambas cuentas y revisá las notificaciones. El botón Actualizar mensajes vuelve a consultar la conversación; el chat no usa WebSockets.
9. Entrá como Super Admin y probá crear usuarios, editar sus datos, cambiar roles y activar o desactivar cuentas.

La desactivación de una cuenta revoca su sesión y finaliza vínculos y asignaciones. Cambiar un rol también desactiva las rutinas creadas por esa cuenta. El último administrador activo no puede eliminar su acceso. La reactivación de la cuenta no reactiva automáticamente sus vínculos anteriores.

## Base de datos local

- Motor: SQL Server 2025 Developer, limitado a 2 GB de memoria para estas pruebas.
- Dirección: `127.0.0.1,14333`.
- Base: `CalisTrack`.
- Configuración privada: `appsettings.Local.json`, excluida de Git y con acceso restringido al usuario.
- Directorio de datos: `.local/sqlserver/data`, accesible por el enlace `/var/opt/mssql`.
- Archivos adjuntos: `App_Data/archivos`.
- Claves de las sesiones: `App_Data/keys`.

La contraseña de SQL Server fue generada para esta instalación. No es la contraseña de los usuarios demo. La cadena de conexión está en el archivo local privado. No publiques ese archivo, los datos de SQL Server, los adjuntos ni las claves de sesión en GitHub.

Se usan los enlaces `/opt/mssql` y `/var/opt/mssql`, creados para esta instalación. No muevas la carpeta del proyecto mientras los estés usando. Los scripts locales sirven para esta computadora; en otra máquina podés usar una instalación normal de SQL Server o Docker.

## Ejecutar en otra computadora

1. Instalá el SDK de [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Instalá [SQL Server Developer o Express](https://www.microsoft.com/sql-server/sql-server-downloads), o iniciá una instancia propia de SQL Server.
3. Copiá `appsettings.Local.example.json` como `appsettings.Local.json` y completá la conexión de tu instancia.
4. Desde la carpeta del proyecto, ejecutá:

```bash
dotnet restore
dotnet run
```

En Visual Studio, abrí `CalisTrack.csproj`, configurá la conexión y elegí el perfil `CalisTrack`. En Windows, la conexión de LocalDB puede ser:

```text
Server=(localdb)\MSSQLLocalDB;Database=CalisTrack;Trusted_Connection=True;TrustServerCertificate=True
```

En un archivo JSON la barra invertida se escribe doble: `\\`. Para SQL Server con usuario y contraseña:

```text
Server=localhost,1433;Database=CalisTrack;User Id=USUARIO;Password=TU_CONTRASENA;Encrypt=True;TrustServerCertificate=True
```

El usuario SQL necesita poder crear la base y aplicar migraciones durante la preparación local. `TrustServerCertificate=True` permite el certificado local del servidor de desarrollo; al publicar habrá que configurar el certificado y los secretos del entorno de hosting.

## Estructura y relación con la guía de Entity Framework

| Carpeta | Responsabilidad |
| --- | --- |
| Models | Clases del dominio: Usuario, Alumno, Entrenador, SuperAdmin y el resto del diagrama |
| Data | DbContext, mapeo, migraciones y seed |
| Controllers | Reciben formularios, validan permisos y hacen las operaciones con EF |
| ViewModels | Datos que necesitan los formularios y las pantallas |
| Views | Vistas Razor que generan el HTML |
| Services | Ayudas compartidas para permisos/notificaciones, archivos, fechas e iniciales |
| wwwroot | CSS, JavaScript e iconos locales |
| tests | Verificación HTTP integral y consulta de la base temporal |

Es un solo proyecto MVC. No hay una API separada, frontend SPA, repositorios genéricos, CQRS ni arquitectura por capas adicional. Los controladores trabajan directamente con el contexto, consultas LINQ, `Add`, entidades modificadas y `SaveChangesAsync`, siguiendo la línea Code First de la guía.

Las clases `Alumno`, `Entrenador` y `SuperAdmin` heredan de `Usuario`. EF las guarda en la tabla Usuarios usando la columna Rol para distinguirlas. Las operaciones que requieren HTTP, autenticación o acceso a datos se resuelven desde los controladores, aunque el UML conceptual las ubique en las clases de negocio.

Se agregaron claves foráneas, propiedades de navegación, validaciones y algunos campos prácticos: grupo muscular, descripción de rutina, versión de sesión y copias de nombres/objetivos de una sesión. Estos últimos evitan que una edición posterior de la rutina cambie el historial. La biblioteca pertenece al entrenador; el alumno accede al material a través de su rutina.

Las bajas de rutinas, ejercicios y vínculos son lógicas. Una rutina activa debe tener al menos un ejercicio. Un alumno tiene como máximo un vínculo activo y una rutina por período; puede conservar asignaciones e historial anteriores. El fin de una asignación o de un vínculo no borra entrenamientos. Las fechas de negocio se calculan con UTC−3, y los mensajes y avisos se almacenan en UTC.

Los archivos admitidos son JPG, PNG, WebP, PDF, MP4 y WebM, de hasta 20 MB, con comprobación de su encabezado. Se guardan fuera de la carpeta pública y se entregan por un controlador que verifica permisos. Los mensajes y notificaciones se guardan en SQL Server; no se simulan en el navegador.

## Migraciones

La migración inicial está incluida. La aplicación la aplica automáticamente al comenzar.

Si modificás una clase del modelo:

```bash
./scripts/dotnet.sh tool restore
./scripts/dotnet.sh ef migrations add NombreDelCambio
./scripts/dotnet.sh ef database update
```

En una computadora con `dotnet` instalado globalmente, podés reemplazar `./scripts/dotnet.sh` por `dotnet`. No uses `EnsureCreated` junto con migraciones.

## Verificaciones

Verificación del 15/11/2026: compilación sin errores ni advertencias y 26 comprobaciones integrales aprobadas contra SQL Server, incluyendo el registro por rol, el acceso a técnica y el flujo Guardar y asignar. La base de ejemplo contiene 5 usuarios, 8 entrenamientos y 72 registros de series.

Con SQL Server iniciado:

```bash
./scripts/dotnet.sh build tests/Database/Database.csproj
python3 tests/verificar.py
```

El script crea una base temporal llamada `CalisTrackPruebas_...`, inicia la aplicación en `localhost:5081` y comprueba los recorridos cliente/servidor con formularios reales y protección antifalsificación. Al terminar, elimina exclusivamente esa base temporal y sus archivos de prueba. No modifica la base CalisTrack. Los adjuntos y claves de las pruebas se crean en una carpeta temporal exclusiva para esa ejecución.

Se revisaron las 28 pantallas del PDF de Figma y se incorporaron sus colores, distribución y controles principales. El CSS adapta las pantallas a escritorio y móvil; queda pendiente observar la aplicación renderizada en ambos tamaños. Los números del inicio y los gráficos se calculan con datos reales. El chat se actualiza con su botón; no se muestra presencia en línea.

## Problemas comunes

- **No se puede conectar a SQL Server:** comprobá que esté abierta la terminal de la base y que `appsettings.Local.json` apunte al puerto correcto.
- **El puerto está ocupado:** probablemente la aplicación o SQL Server ya estén ejecutándose. Usá la instancia que ya está abierta.
- **No aparece una rutina al alumno:** revisá el vínculo activo, las fechas, el estado del alumno y el estado de la rutina.
- **No se puede asignar otra rutina:** finalizá la asignación anterior desde el perfil del alumno o elegí un período que no se superponga.
- **El archivo no se admite:** verificá su formato real y que no supere 20 MB.
- **No encontrás `dotnet`:** en esta computadora usá `./scripts/dotnet.sh`; en otras instalá el SDK, no solamente el Runtime.

## Repositorio y publicación

Repositorio público: [JoelJaldin/CalisTrack](https://github.com/JoelJaldin/CalisTrack).

```bash
git clone https://github.com/JoelJaldin/CalisTrack.git
cd CalisTrack
```

Luego seguí la sección «Ejecutar en otra computadora». El repositorio contiene código, migraciones y datos de ejemplo definidos en C#. La base creada en esta computadora, sus contraseñas, las claves de sesión y los archivos subidos quedan excluidos de Git.

El hosting todavía no está configurado. Al elegirlo habrá que preparar una instancia de SQL Server, la conexión mediante secretos del entorno, almacenamiento persistente para los adjuntos y las claves, y HTTPS. La seed de ejemplo solo se carga en desarrollo.
