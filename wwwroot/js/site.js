const menu = document.querySelector('.mobile-toggle');
menu?.addEventListener('click', () => {
    const abierto = document.querySelector('#sidebar').classList.toggle('open');
    menu.setAttribute('aria-expanded', String(abierto));
});
document.addEventListener('click', event => {
    const sidebar = document.querySelector('#sidebar');
    if (sidebar?.classList.contains('open') && !sidebar.contains(event.target) && !menu.contains(event.target)) {
        sidebar.classList.remove('open');
        menu.setAttribute('aria-expanded', 'false');
    }
});
document.querySelectorAll('[data-password]').forEach(button => {
    button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.password);
        const mostrar = input.type === 'password';
        input.type = mostrar ? 'text' : 'password';
        button.textContent = mostrar ? 'Ocultar' : 'Ver';
        button.setAttribute('aria-label', mostrar ? 'Ocultar contraseña' : 'Mostrar contraseña');
    });
});
document.querySelectorAll('[data-dialog]').forEach(button => button.addEventListener('click', () => document.getElementById(button.dataset.dialog).showModal()));
document.querySelectorAll('[data-close-dialog]').forEach(button => button.addEventListener('click', () => button.closest('dialog').close()));
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => {
    if (!window.confirm(form.dataset.confirm)) event.preventDefault();
}));
const rows = document.getElementById('routine-rows');
if (rows) {
    const template = rows.firstElementChild.cloneNode(true);
    const renumerar = () => {
        Array.from(rows.children).forEach((row, index) => {
            row.querySelector('.row-number').textContent = `Ejercicio ${index + 1}`;
            row.querySelectorAll('[name]').forEach(input => input.name = input.name.replace(/Ejercicios\[\d+\]/, `Ejercicios[${index}]`));
            row.querySelectorAll('[id]').forEach(input => input.id = input.id.replace(/Ejercicios_\d+__/, `Ejercicios_${index}__`));
            row.querySelectorAll('label[for]').forEach(label => label.htmlFor = label.htmlFor.replace(/Ejercicios_\d+__/, `Ejercicios_${index}__`));
        });
    };
    document.getElementById('add-exercise').addEventListener('click', () => {
        if (rows.children.length >= 30) return;
        const row = template.cloneNode(true);
        row.querySelectorAll('input, select').forEach(input => {
            if (input.tagName === 'SELECT') input.value = '';
            else if (input.name.endsWith('.Series')) input.value = '3';
            else if (input.name.endsWith('.Repeticiones')) input.value = '10';
            else if (input.name.endsWith('.DescansoSegundos')) input.value = '60';
            else input.value = '';
        });
        rows.append(row);
        renumerar();
        row.querySelector('select').focus();
    });
    rows.addEventListener('click', event => {
        const button = event.target.closest('button');
        if (!button) return;
        const row = button.closest('.routine-row');
        if (button.hasAttribute('data-remove-exercise') && rows.children.length > 1) row.remove();
        if (button.dataset.move === 'up' && row.previousElementSibling) rows.insertBefore(row, row.previousElementSibling);
        if (button.dataset.move === 'down' && row.nextElementSibling) rows.insertBefore(row.nextElementSibling, row);
        renumerar();
    });
}
const training = document.getElementById('training-form');
if (training) {
    const checks = Array.from(training.querySelectorAll('[data-series-check]'));
    const display = document.getElementById('timer-display');
    let end = 0;
    let timer;
    let changed = false;
    const renderTimer = () => {
        const seconds = Math.max(0, Math.ceil((end - Date.now()) / 1000));
        display.textContent = `${String(Math.floor(seconds / 60)).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`;
        if (seconds === 0) clearInterval(timer);
    };
    const startRest = seconds => {
        clearInterval(timer);
        end = Date.now() + seconds * 1000;
        renderTimer();
        timer = setInterval(renderTimer, 250);
    };
    checks.forEach(check => check.addEventListener('change', () => {
        check.closest('.series-row').classList.toggle('is-completed', check.checked);
        const count = checks.filter(c => c.checked).length;
        document.getElementById('completed-count').textContent = count;
        document.getElementById('training-progress-fill').style.width = `${count * 100 / checks.length}%`;
        if (check.checked) startRest(Number(check.dataset.rest));
    }));
    training.querySelectorAll('[data-start-rest]').forEach(button => button.addEventListener('click', () => startRest(Number(button.dataset.startRest))));
    document.getElementById('timer-reset').addEventListener('click', () => startRest(0));
    training.addEventListener('input', () => changed = true);
    training.addEventListener('submit', () => changed = false);
    window.addEventListener('beforeunload', event => { if (changed) { event.preventDefault(); event.returnValue = ''; } });
}
const messages = document.getElementById('messages');
if (messages) messages.scrollTop = messages.scrollHeight;
document.querySelectorAll('input[type=file]').forEach(input => input.addEventListener('change', () => {
    input.setCustomValidity(input.files[0]?.size > 20 * 1024 * 1024 ? 'El archivo no puede superar los 20 MB.' : '');
    input.reportValidity();
}));
document.querySelectorAll('dialog[data-open-on-load="true"]').forEach(dialog => dialog.showModal());
document.getElementById('buscar-disponible')?.addEventListener('input', event => {
    const texto = event.target.value.toLocaleLowerCase('es').trim();
    document.querySelectorAll('[data-student-search]').forEach(row => row.hidden = !row.dataset.studentSearch.toLocaleLowerCase('es').includes(texto));
});
const exerciseTabs = Array.from(document.querySelectorAll('[data-exercise-tab]'));
const selectExercise = id => {
    exerciseTabs.forEach(tab => {
        const selected = tab.dataset.exerciseTab === id;
        tab.classList.toggle('selected', selected);
        tab.setAttribute('aria-pressed', String(selected));
        document.getElementById(tab.dataset.exerciseTab).hidden = !selected;
    });
};
if (exerciseTabs.length) {
    selectExercise(exerciseTabs[0].dataset.exerciseTab);
    exerciseTabs.forEach(tab => tab.addEventListener('click', () => selectExercise(tab.dataset.exerciseTab)));
    const updateCompleted = () => exerciseTabs.forEach(tab => {
        const checks = Array.from(document.getElementById(tab.dataset.exerciseTab).querySelectorAll('[data-series-check]'));
        tab.classList.toggle('done', checks.length > 0 && checks.every(check => check.checked));
    });
    training.addEventListener('change', updateCompleted);
    training.addEventListener('invalid', event => {
        const exercise = event.target.closest('.training-exercise');
        if (exercise) selectExercise(exercise.id);
    }, true);
    updateCompleted();
}
const studentSelection = Array.from(document.querySelectorAll('input[name=AlumnoIds]'));
studentSelection.forEach(input => input.addEventListener('change', () => {
    document.getElementById('selected-students').textContent = `${studentSelection.filter(student => student.checked).length} seleccionados`;
}));
