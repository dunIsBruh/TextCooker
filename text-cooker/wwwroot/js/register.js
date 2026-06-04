document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('registerForm');
    const loginInput = document.getElementById('login');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const loginError = document.getElementById('loginError');
    const passwordError = document.getElementById('passwordError');
    const confirmPasswordError = document.getElementById('confirmPasswordError');

    // Redirect if already logged in
    if (isAuthenticated()) {
        window.location.href = 'index.html';
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        // Clear previous errors
        clearErrors();

        const login = loginInput.value;
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        // Validation (same as backend)
        let hasErrors = false;

        // Login validation
        if (!login) {
            showError(loginError, 'Имя пользователя обязательно');
            hasErrors = true;
        } else if (login.length < 8) {
            showError(loginError, 'Имя должно содержать не менее 8 символов');
            hasErrors = true;
        }

        // Password validation
        if (!password) {
            showError(passwordError, 'Пароль обязателен');
            hasErrors = true;
        } else {
            if (password.length < 8) {
                showError(passwordError, 'Пароль должен содержать не менее 8 символов');
                hasErrors = true;
            } else if (!/[0-9]/.test(password)) {
                showError(passwordError, 'Пароль должен содержать хотя бы одну цифру');
                hasErrors = true;
            } else if (!/[A-Z]/.test(password)) {
                showError(passwordError, 'Пароль должен содержать хотя бы одну заглавную букву');
                hasErrors = true;
            } else if (!/[!@#$%^&*]/.test(password)) {
                showError(passwordError, 'Пароль должен содержать хотя бы один спецсимвол (!@#$%^&*)');
                hasErrors = true;
            }
        }

        // Confirm password validation
        if (!confirmPassword) {
            showError(confirmPasswordError, 'Подтверждение пароля обязательно');
            hasErrors = true;
        } else if (password !== confirmPassword) {
            showError(confirmPasswordError, 'Пароль и его подтверждение не совпадают');
            hasErrors = true;
        }

        if (hasErrors) {
            return;
        }

        // Disable form
        form.querySelector('button').disabled = true;
        form.querySelector('button').textContent = 'Регистрация...';

        try {
            const result = await register(login, password, confirmPassword);
            
            if (result.success) {
                showAlert('Регистрация успешна! Выполняется вход...', 'success');
                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1000);
            } else {
                if (result.error && result.error.includes('already exists')) {
                    showError(loginError, 'Пользователь с таким логином уже существует');
                } else {
                    showAlert(result.error || 'Ошибка регистрации', 'error');
                }
                form.querySelector('button').disabled = false;
                form.querySelector('button').textContent = 'Зарегистрироваться';
            }
        } catch (error) {
            console.error('Register error:', error);
            showAlert('Произошла ошибка при регистрации', 'error');
            form.querySelector('button').disabled = false;
            form.querySelector('button').textContent = 'Зарегистрироваться';
        }
    });

    function clearErrors() {
        loginError.classList.remove('show');
        passwordError.classList.remove('show');
        confirmPasswordError.classList.remove('show');
        loginError.textContent = '';
        passwordError.textContent = '';
        confirmPasswordError.textContent = '';
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



