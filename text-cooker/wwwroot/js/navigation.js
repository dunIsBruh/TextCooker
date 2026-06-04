// Navigation and UI updates
function updateUserIcon() {
    const userIcon = document.querySelector('.user-icon');
    const userLoginElement = document.getElementById('userLogin');
    
    if (!userIcon) return;

    const role = getUserRole();
    const isAuth = isAuthenticated();
    const login = getUserLogin();
    
    userIcon.className = 'user-icon';
    
    if (role === 'Anonymous' || !isAuth) {
        userIcon.classList.add('guest');
        userIcon.textContent = '👤';
        if (userLoginElement) {
            userLoginElement.style.display = 'none';
        }
    } else if (role === 'Admin') {
        userIcon.classList.add('admin');
        userIcon.textContent = '👤';
        if (userLoginElement && login) {
            userLoginElement.textContent = login;
            userLoginElement.style.display = 'block';
        }
    } else {
        userIcon.classList.add('user');
        userIcon.textContent = '👤';
        if (userLoginElement && login) {
            userLoginElement.textContent = login;
            userLoginElement.style.display = 'block';
        }
    }
}

function updateNavigation() {
    const navLinks = document.querySelectorAll('.nav-menu a');
    const isAuth = isAuthenticated();
    const logoutBtn = document.getElementById('logoutBtn');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        
        // Pages that require authentication
        if (href === 'index.html' || href === 'documentHistory.html' || href === 'templateList.html') {
            if (!isAuth) {
                link.classList.add('disabled');
            } else {
                link.classList.remove('disabled');
            }
        }
        
        // Pages that should be hidden when authenticated
        if (href === 'login.html' || href === 'register.html') {
            if (isAuth) {
                link.style.display = 'none';
            } else {
                link.style.display = 'block';
            }
        }
    });
    
    // Show/hide logout button based on authentication status
    if (logoutBtn) {
        if (isAuth) {
            logoutBtn.style.display = 'block';
        } else {
            logoutBtn.style.display = 'none';
        }
    }
}

function setActivePage() {
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    const navLinks = document.querySelectorAll('.nav-menu a');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentPage) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
}

// Initialize navigation on page load
document.addEventListener('DOMContentLoaded', () => {
    updateUserIcon();
    updateNavigation();
    setActivePage();
    
    // Setup logout button handler
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            await logout();
        });
    }
    
    // Redirect to login if not authenticated and trying to access protected pages
    const protectedPages = ['index.html', 'documentHistory.html', 'templateList.html'];
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    
    if (protectedPages.includes(currentPage) && !isAuthenticated()) {
        window.location.href = 'login.html';
    }
});


