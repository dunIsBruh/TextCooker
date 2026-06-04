document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('loginForm');
    const loginInput = document.getElementById('login');
    const passwordInput = document.getElementById('password');
    const loginError = document.getElementById('loginError');
    const passwordError = document.getElementById('passwordError');

    // Redirect if already logged in
    if (isAuthenticated()) {
        window.location.href = 'index.html';
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        // Clear previous errors
        clearErrors();

        const login_input = loginInput.value;
        const password = passwordInput.value;

        // Validation
        let hasErrors = false;

        if (!login_input) {
            showError(loginError, 'Имя пользователя обязательно');
            hasErrors = true;
        }

        if (!password) {
            showError(passwordError, 'Пароль обязателен');
            hasErrors = true;
        }

        if (hasErrors) {
            return;
        }

        // Disable form
        form.querySelector('button').disabled = true;
        form.querySelector('button').textContent = 'Вход...';

        try {
            const result = await loginFunc(login_input, password);
            
            if (result.success) {
                showAlert('Успешный вход!', 'success');
                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1000);
            } else {
                showAlert(result.error || 'Ошибка входа', 'error');
                form.querySelector('button').disabled = false;
                form.querySelector('button').textContent = 'Войти';
            }
        } catch (error) {
            console.error('Login error:', error);
            showAlert('Произошла ошибка при входе', 'error');
            form.querySelector('button').disabled = false;
            form.querySelector('button').textContent = 'Войти';
        }
    });

    function clearErrors() {
        loginError.classList.remove('show');
        passwordError.classList.remove('show');
        loginError.textContent = '';
        passwordError.textContent = '';
    }

    function showError(element, message) {
        element.textContent = message;
        element.classList.add('show');
    }

    function showAlert(message, type) {
        const container = document.getElementById('alertContainer');
        container.innerHTML = `<div class="alert alert-${type} show">${message}</div>`;
        setTimeout(() => {
            container.innerHTML = '';
        }, 5000);
    }
});



