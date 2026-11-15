import datetime
import html
import http.cookiejar
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid

ROOT = Path(__file__).resolve().parents[1]
BASE = 'http://localhost:5081'
TODAY = (datetime.datetime.now(datetime.timezone.utc) - datetime.timedelta(hours=3)).date()
RESULTS = []

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None

class Client:
    def __init__(self):
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()), NoRedirect())

    def request(self, path, data=None, headers=None, follow=True):
        request = urllib.request.Request(BASE + path, data=data, headers=headers or {})
        try:
            response = self.opener.open(request, timeout=30)
        except urllib.error.HTTPError as error:
            response = error
        status, location, body = response.code, response.headers.get('Location', ''), response.read().decode('utf-8', errors='replace')
        if follow and status in (301, 302, 303):
            return self.request(location if location.startswith('/') else urllib.parse.urlsplit(location).path)
        return status, location, body

    def get(self, path, status=200):
        result = self.request(path)
        assert result[0] == status, f'GET {path}: {result[0]} en lugar de {status}\n{result[2][:1000]}'
        return html.unescape(result[2])

    def token(self, page):
        body = self.get(page)
        found = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
        assert found, f'Sin token en {page}'
        return found.group(1)

    def post(self, path, fields, page=None, status=302, file=None):
        items = list(fields.items()) if isinstance(fields, dict) else list(fields)
        items.append(('__RequestVerificationToken', self.token(page or path)))
        if file:
            boundary = 'CalisTrack' + uuid.uuid4().hex
            parts = []
            for key, value in items:
                parts.append(f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{value}\r\n'.encode())
            key, filename, content, mime = file
            parts.append(f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"; filename="{filename}"\r\nContent-Type: {mime}\r\n\r\n'.encode() + content + b'\r\n')
            parts.append(f'--{boundary}--\r\n'.encode())
            data, headers = b''.join(parts), {'Content-Type': 'multipart/form-data; boundary=' + boundary}
        else:
            data, headers = urllib.parse.urlencode(items).encode(), {'Content-Type': 'application/x-www-form-urlencoded'}
        result = self.request(path, data, headers, follow=False)
        assert result[0] == status, f'POST {path}: {result[0]} en lugar de {status}\n{html.unescape(result[2])[:3000]}'
        return result[1], html.unescape(result[2])

    def login(self, email, password='CalisTrack123!'):
        location, _ = self.post('/Cuenta/Login', {'Email': email, 'Password': password})
        assert location == '/' or location.startswith('/Inicio'), location
        return self

def check(name, condition=True):
    assert condition, name
    RESULTS.append(name)
    print('OK · ' + name, flush=True)

def identifier(body, controller, action='Editar'):
    match = re.search(r'href="/' + controller + '/' + action + r'/(\d+)"', body)
    assert match, f'No se encontró ID de {controller}/{action}'
    return int(match.group(1))

def exercise_fields(name):
    return {'Nombre': name, 'Descripcion': '', 'GrupoMuscular': 'Core', 'Enlace': ''}

def routine_fields(name, exercise):
    return {'Nombre': name, 'Descripcion': '', 'Ejercicios[0].EjercicioId': exercise, 'Ejercicios[0].Series': 2, 'Ejercicios[0].Repeticiones': 8, 'Ejercicios[0].DescansoSegundos': 30, 'Ejercicios[0].Observaciones': ''}

def run_checks():
    anonymous = Client()
    assert anonymous.request('/Rutinas', follow=False)[0] == 302
    assert anonymous.request('/Cuenta/Login', b'Email=x', follow=False)[0] == 400
    check('Acceso privado y protección antifalsificación')
    admin = Client().login('admin@calistrack.local')
    trainer = Client().login('entrenador@calistrack.local')
    student = Client().login('alumno@calistrack.local')
    seed_student_id = identifier(admin.get('/Usuarios?buscar=alumno%40calistrack.local'), 'Usuarios')
    seed_admin_id = identifier(admin.get('/Usuarios?buscar=admin%40calistrack.local'), 'Usuarios')
    seed_training_id = identifier(student.get('/Historial'), 'Entrenamientos', 'Detalle')
    for client, pages in [(admin, ['/Inicio', '/Usuarios', '/Usuarios/Editar', '/Cuenta/Perfil', '/Notificaciones']), (trainer, ['/Inicio', '/Alumnos', f'/Alumnos/Detalle/{seed_student_id}', '/Biblioteca', '/Biblioteca/Editar', '/Rutinas', '/Rutinas/Editar', '/Rutinas/Asignar/1', f'/Historial?alumnoId={seed_student_id}', f'/Entrenamientos/Detalle/{seed_training_id}', '/Chat', '/Cuenta/Perfil', '/Notificaciones']), (student, ['/Inicio', '/Rutinas/MiRutina', '/Historial', f'/Entrenamientos/Detalle/{seed_training_id}', '/Chat', '/Cuenta/Perfil', '/Notificaciones'])]:
        for page in pages:
            body = client.get(page)
            assert '<h1>' in body or '<h1 ' in body, page
    check('Todas las páginas de los tres roles responden correctamente')
    student.get('/Usuarios', 403)
    student.get('/Biblioteca', 403)
    trainer.get('/Usuarios', 403)
    student.get('/Entrenamientos/Detalle/99999', 404)
    check('Permisos por rol y recursos inexistentes')
    _, invalid = anonymous.post('/Cuenta/Registro', {'Nombre': '', 'Email': 'invalido', 'Password': '123', 'ConfirmarPassword': '123'}, status=200)
    assert 'validation-summary-errors' in invalid or 'field-validation-error' in invalid
    newcomer = Client()
    newcomer.post('/Cuenta/Registro', {'Nombre': 'Alumno de prueba', 'Email': 'prueba@calistrack.local', 'Password': 'Prueba123!', 'ConfirmarPassword': 'Prueba123!', 'Objetivo': ''})
    new_id = identifier(admin.get('/Usuarios?buscar=prueba%40calistrack.local'), 'Usuarios')
    _, duplicate = anonymous.post('/Cuenta/Registro', {'Nombre': 'Duplicado', 'Email': 'PRUEBA@CALISTRACK.LOCAL', 'Password': 'Prueba123!', 'ConfirmarPassword': 'Prueba123!'}, status=200)
    assert 'ya está registrado' in duplicate
    check('Registro real, validaciones y email único sin distinguir mayúsculas')
    _, invalid_role = anonymous.post('/Cuenta/Registro', {'Nombre': 'Rol no permitido', 'Email': 'rol-invalido@calistrack.local', 'Password': 'Prueba123!', 'ConfirmarPassword': 'Prueba123!', 'Rol': 'SuperAdmin'}, status=200)
    assert 'Elegí Alumno o Entrenador' in invalid_role
    assert 'rol-invalido@calistrack.local' not in admin.get('/Usuarios?buscar=rol-invalido')
    registered_coach = Client()
    registered_coach.post('/Cuenta/Registro', {'Nombre': 'Entrenador registrado', 'Email': 'registro-entrenador@calistrack.local', 'Password': 'Prueba123!', 'ConfirmarPassword': 'Prueba123!', 'Rol': 'Entrenador'})
    registered_coach.get('/Biblioteca')
    registered_coach.get('/Usuarios', 403)
    check('Registro de entrenadores y rechazo del alta pública de administradores')
    trainer.post('/Alumnos/Vincular', {'alumnoId': new_id}, page='/Alumnos')
    check('Vinculación de un alumno recién registrado', 'Alumno de prueba' in trainer.get('/Alumnos'))
    trainer.post('/Biblioteca/Editar', exercise_fields('Ejercicio de prueba'))
    exercise_id = identifier(trainer.get('/Biblioteca?buscar=Ejercicio%20de%20prueba'), 'Biblioteca')
    fields = exercise_fields('Ejercicio editado'); fields['Id'] = exercise_id
    trainer.post('/Biblioteca/Editar', fields, page=f'/Biblioteca/Editar/{exercise_id}')
    check('Alta y edición de ejercicios con campos opcionales vacíos', 'Ejercicio editado' in trainer.get('/Biblioteca'))
    fields = exercise_fields('Archivo inválido')
    _, invalid = trainer.post('/Biblioteca/Editar', fields, file=('Archivo', 'falso.png', b'contenido invalido', 'image/png'), status=200)
    assert 'Formato no admitido' in invalid
    check('Rechazo de archivos con extensión falsa')
    pdf = b'%PDF-1.4\n1 0 obj << /Type /Catalog >> endobj\n%%EOF'
    fields = exercise_fields('Ejercicio con material')
    trainer.post('/Biblioteca/Editar', fields, file=('Archivo', 'tecnica.pdf', pdf, 'application/pdf'))
    material_exercise_id = identifier(trainer.get('/Biblioteca?buscar=Ejercicio%20con%20material'), 'Biblioteca')
    material_body = trainer.get(f'/Biblioteca/Editar/{material_exercise_id}')
    file_id = identifier(material_body, 'Archivos', 'Ver')
    student.get(f'/Archivos/Ver/{file_id}', 404)
    trainer.get(f'/Archivos/Ver/{file_id}')
    check('Carga de archivos y descarga protegida')
    _, invalid = trainer.post('/Rutinas/Editar', {'Nombre': 'Rutina vacía'}, status=200)
    assert 'entre 1 y 30 ejercicios' in invalid
    fields = routine_fields('Rutina de prueba', exercise_id); fields['Ejercicios[0].Series'] = 0
    trainer.post('/Rutinas/Editar', fields, status=200)
    check('Rechazo de rutinas vacías y series inválidas')
    create_fields = routine_fields('Rutina de prueba', exercise_id); create_fields['accion'] = 'asignar'
    assign_location, _ = trainer.post('/Rutinas/Editar', create_fields)
    routine_id = identifier(trainer.get('/Rutinas?buscar=Rutina%20de%20prueba'), 'Rutinas')
    check('Guardar una rutina y continuar a su asignación', assign_location == f'/Rutinas/Asignar/{routine_id}')
    trainer.post('/Rutinas/Asignar', [('RutinaId', routine_id), ('AlumnoIds', new_id), ('FechaDesde', TODAY.isoformat()), ('FechaHasta', '')], page=f'/Rutinas/Asignar/{routine_id}')
    assert 'Rutina de prueba' in newcomer.get('/Rutinas/MiRutina')
    _, invalid = trainer.post('/Rutinas/Asignar', [('RutinaId', routine_id), ('AlumnoIds', new_id), ('FechaDesde', TODAY.isoformat())], page=f'/Rutinas/Asignar/{routine_id}', status=200)
    assert 'ya tiene una rutina' in invalid
    check('Asignación real, visibilidad del alumno y prevención de períodos superpuestos')
    student_body = newcomer.get('/Rutinas/MiRutina')
    technique_id = identifier(student_body, 'Rutinas', 'Tecnica')
    technique = newcomer.get(f'/Rutinas/Tecnica/{technique_id}')
    assert 'Ejercicio editado' in technique and 'Explicación técnica' in technique
    student.get(f'/Rutinas/Tecnica/{technique_id}', 404)
    trainer.get(f'/Rutinas/Tecnica/{technique_id}', 403)
    check('Vista de técnica accesible solo para el alumno con esa rutina vigente')
    assignment_id = int(re.search(r'name="asignacionId"[^>]*value="(\d+)"', student_body).group(1))
    student.post('/Entrenamientos/Comenzar', {'asignacionId': assignment_id}, page='/Rutinas/MiRutina', status=404)
    location, _ = newcomer.post('/Entrenamientos/Comenzar', {'asignacionId': assignment_id}, page='/Rutinas/MiRutina')
    training_id = int(location.rsplit('/', 1)[1])
    same, _ = newcomer.post('/Entrenamientos/Comenzar', {'asignacionId': assignment_id}, page='/Rutinas/MiRutina')
    assert same == location
    check('Inicio de entrenamiento propio y reutilización de sesión pendiente')
    body = newcomer.get(location)
    series_ids = re.findall(r'name="Series\[\d+\]\.Id"[^>]*value="(\d+)"', body)
    assert len(series_ids) == 2
    fields = {'Id': training_id, 'Fecha': TODAY.isoformat(), 'Comentario': '', 'accion': 'guardar'}
    for i, series_id in enumerate(series_ids):
        fields.update({f'Series[{i}].Id': series_id, f'Series[{i}].Repeticiones': 8 + i, f'Series[{i}].TiempoSegundos': 30, f'Series[{i}].Completada': 'true'})
    invalid_fields = dict(fields); invalid_fields['Fecha'] = (TODAY + datetime.timedelta(days=1)).isoformat()
    _, invalid = newcomer.post('/Entrenamientos/Entrenar', invalid_fields, page=location, status=200)
    assert 'no puede ser futura' in invalid
    invalid_fields = dict(fields); invalid_fields['Series[0].Repeticiones'] = -1
    newcomer.post('/Entrenamientos/Entrenar', invalid_fields, page=location, status=200)
    invalid_fields = dict(fields); invalid_fields['Series[0].Id'] = '99999'
    newcomer.post('/Entrenamientos/Entrenar', invalid_fields, page=location, status=400)
    check('Validación de fechas, números y pertenencia de series')
    newcomer.post('/Entrenamientos/Entrenar', fields, page=location)
    assert 'value="8"' in newcomer.get(location)
    fields['accion'] = 'finalizar'; fields['Comentario'] = '<script>alert(1)</script>'
    newcomer.post('/Entrenamientos/Entrenar', fields, page=location)
    raw = newcomer.request(f'/Entrenamientos/Detalle/{training_id}')[2]
    assert '&lt;script&gt;' in raw and '<script>alert(1)</script>' not in raw
    assert 'Rutina de prueba' in newcomer.get('/Historial')
    check('Guardado parcial, finalización, historial y escape de texto')
    fields = routine_fields('Rutina modificada', exercise_id); fields['Id'] = routine_id; fields['Ejercicios[0].Repeticiones'] = 20
    trainer.post('/Rutinas/Editar', fields, page=f'/Rutinas/Editar/{routine_id}')
    detail = newcomer.get(f'/Entrenamientos/Detalle/{training_id}')
    assert 'Rutina de prueba' in detail and '8 reps' in detail and '20 reps' not in detail
    check('El historial conserva los objetivos originales al editar la rutina')
    trainer.post('/Entrenamientos/Devolver', {'entrenamientoId': training_id, 'texto': 'Buen trabajo de prueba'}, page=f'/Entrenamientos/Detalle/{training_id}', file=('archivo', 'devolucion.pdf', pdf, 'application/pdf'))
    assert 'Buen trabajo de prueba' in newcomer.get(f'/Entrenamientos/Detalle/{training_id}')
    assert 'devolución' in newcomer.get('/Notificaciones')
    check('Devolución con adjunto y notificación al alumno')
    trainer.post('/Chat/Enviar', {'contactoId': new_id, 'texto': 'Mensaje de prueba'}, page=f'/Chat?contactoId={new_id}', file=('archivo', 'chat.pdf', pdf, 'application/pdf'))
    assert 'Mensaje de prueba' in newcomer.get('/Chat')
    assert 'Leído' in trainer.get(f'/Chat?contactoId={new_id}')
    student.post('/Chat/Enviar', {'contactoId': new_id, 'texto': 'Mensaje no autorizado'}, page='/Chat', status=404)
    check('Chat persistente, adjuntos, lectura y bloqueo de contactos ajenos')
    newcomer.post('/Notificaciones/LeerTodas', {}, page='/Notificaciones')
    assert 'Estás al día' in newcomer.get('/Notificaciones?soloPendientes=true')
    check('Notificaciones marcadas como leídas')
    admin.post('/Usuarios/Editar', {'Nombre': 'Entrenador de prueba', 'Email': 'otro@calistrack.local', 'Password': 'Prueba123!', 'Rol': 'Entrenador', 'Activo': 'true'})
    other_id = identifier(admin.get('/Usuarios?buscar=otro%40calistrack.local'), 'Usuarios')
    other = Client().login('otro@calistrack.local', 'Prueba123!')
    other.get(f'/Rutinas/Editar/{routine_id}', 404)
    other.get(f'/Biblioteca/Editar/{exercise_id}', 404)
    other.get(f'/Entrenamientos/Detalle/{training_id}', 404)
    check('Aislamiento entre entrenadores')
    admin.post('/Usuarios/Editar', {'Id': other_id, 'Nombre': 'Rol cambiado', 'Email': 'otro@calistrack.local', 'Password': '', 'Rol': 'Alumno', 'Activo': 'true'}, page=f'/Usuarios/Editar/{other_id}')
    assert other.request('/Inicio', follow=False)[0] == 302
    other = Client().login('otro@calistrack.local', 'Prueba123!')
    other.get('/Rutinas/MiRutina')
    other.get('/Biblioteca', 403)
    check('Cambio de rol persistente e invalidación de sesión anterior')
    admin.post('/Usuarios/Editar', {'Id': other_id, 'Nombre': 'Cuenta inactiva', 'Email': 'otro@calistrack.local', 'Password': '', 'Rol': 'Alumno', 'Activo': 'false'}, page=f'/Usuarios/Editar/{other_id}')
    assert other.request('/Inicio', follow=False)[0] == 302
    Client().post('/Cuenta/Login', {'Email': 'otro@calistrack.local', 'Password': 'Prueba123!'}, status=200)
    check('Desactivación inmediata de cuentas y bloqueo de ingreso')
    _, invalid = admin.post('/Usuarios/Editar', {'Id': seed_admin_id, 'Nombre': 'Admin', 'Email': 'admin@calistrack.local', 'Password': '', 'Rol': 'Alumno', 'Activo': 'false'}, page=f'/Usuarios/Editar/{seed_admin_id}', status=200)
    assert 'propia cuenta' in invalid
    check('Protección del administrador activo')
    trainer.post('/Rutinas/Eliminar', {'id': routine_id}, page='/Rutinas')
    assert 'Rutina modificada' not in newcomer.get('/Rutinas/MiRutina')
    assert 'Rutina de prueba' in newcomer.get('/Historial')
    assert 'Rutina modificada' in trainer.get('/Rutinas?inactivas=true')
    newcomer.get(f'/Rutinas/Tecnica/{technique_id}', 404)
    trainer.post('/Biblioteca/Eliminar', {'id': exercise_id}, page='/Biblioteca')
    assert 'Ejercicio editado' not in trainer.get('/Biblioteca')
    check('Baja lógica de rutina y ejercicio sin perder el historial')
    info = json.loads(subprocess.check_output([str(ROOT/'scripts/dotnet.sh'), 'run', '--project', 'tests/Database/Database.csproj', '--no-build'], cwd=ROOT, env=ENV, text=True))
    check('Persistencia real en SQL Server, migración y contraseñas almacenadas como hash', info['Migraciones'] == 1 and info['Entrenamientos'] == 9 and info['Tablas'] >= 15)
    return info

config = json.loads((ROOT/'appsettings.Local.json').read_text())
original = config['ConnectionStrings']['CalisTrack']
DB_NAME = 'CalisTrackPruebas_' + uuid.uuid4().hex[:10]
connection = re.sub(r'(?i)Database=[^;]+', 'Database=' + DB_NAME, original)
assert connection != original
test_storage = ROOT/'.local'/DB_NAME
ENV = dict(os.environ, ASPNETCORE_ENVIRONMENT='Development', ConnectionStrings__CalisTrack=connection, Storage__Root=str(test_storage))
log_path = ROOT/'.local/pruebas.log'
process = None
try:
    with log_path.open('w') as log:
        process = subprocess.Popen([str(ROOT/'scripts/dotnet.sh'), 'run', '--no-build', '--no-launch-profile', '--urls', BASE], cwd=ROOT, env=ENV, stdout=log, stderr=subprocess.STDOUT)
        for _ in range(90):
            if process.poll() is not None:
                raise RuntimeError('La app de pruebas no inició. Revisá .local/pruebas.log')
            try:
                if Client().request('/Cuenta/Login')[0] == 200:
                    break
            except (urllib.error.URLError, ConnectionError, TimeoutError):
                pass
            time.sleep(0.5)
        else:
            raise RuntimeError('La app de pruebas no respondió.')
        info = run_checks()
        print(json.dumps({'resultado': 'CORRECTO', 'verificaciones': len(RESULTS), 'base': info}, ensure_ascii=False), flush=True)
finally:
    if process and process.poll() is None:
        process.terminate()
        try: process.wait(timeout=15)
        except subprocess.TimeoutExpired:
            process.kill()
            process.wait()
    subprocess.run([str(ROOT/'scripts/dotnet.sh'), 'run', '--project', 'tests/Database/Database.csproj', '--no-build', '--', 'drop'], cwd=ROOT, env=ENV, check=True)
    if test_storage.exists():
        shutil.rmtree(test_storage)
