// API Configuration
const API_BASE_URL = 'http://localhost:8080/api';

// Token refresh timer
let tokenRefreshTimer = null;

// Token management - helper functions to always read from localStorage
function getAccessToken() {
    return localStorage.getItem('accessToken');
}

function getRefreshToken() {
    return localStorage.getItem('refreshToken');
}

function setTokens(access, refresh) {
    if (access) {
        localStorage.setItem('accessToken', access);
    } else {
        localStorage.removeItem('accessToken');
    }
    if (refresh) {
        localStorage.setItem('refreshToken', refresh);
    } else {
        localStorage.removeItem('refreshToken');
    }
    
    // Перезапускаем таймер обновления при установке нового токена
    if (access) {
        startTokenRefreshTimer();
    }
}

// Получить время истечения токена из JWT
function getTokenExpiration(accessToken) {
    try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        // exp - это Unix timestamp в секундах
        return payload.exp ? payload.exp * 1000 : null; // Конвертируем в миллисекунды
    } catch (error) {
        console.error('Error parsing token expiration:', error);
        return null;
    }
}

// Запустить таймер для автоматического обновления токена
function startTokenRefreshTimer() {
    // Останавливаем предыдущий таймер, если он есть
    if (tokenRefreshTimer) {
        clearTimeout(tokenRefreshTimer);
        tokenRefreshTimer = null;
    }
    
    const accessToken = getAccessToken();
    if (!accessToken) {
        return;
    }
    
    const expirationTime = getTokenExpiration(accessToken);
    if (!expirationTime) {
        return;
    }
    
    const now = Date.now();
    const timeUntilExpiration = expirationTime - now;
    
    // Обновляем токен за 2 минуты до истечения (или раньше, если осталось меньше 2 минут)
    const refreshDelay = Math.max(timeUntilExpiration - 2 * 60 * 1000, 30 * 1000); // Минимум 30 секунд
    
    if (refreshDelay <= 0) {
        // Токен уже истек или скоро истечет, обновляем немедленно
        refreshAccessToken();
        return;
    }
    
    console.log(`Token will be refreshed in ${Math.round(refreshDelay / 1000)} seconds`);
    
    tokenRefreshTimer = setTimeout(async () => {
        console.log('Auto-refreshing access token...');
        const refreshed = await refreshAccessToken();
        if (!refreshed) {
            console.error('Auto-refresh failed, user will be logged out');
        }
    }, refreshDelay);
}

// Остановить таймер обновления токена
function stopTokenRefreshTimer() {
    if (tokenRefreshTimer) {
        clearTimeout(tokenRefreshTimer);
        tokenRefreshTimer = null;
    }
}

// Clear all user data from localStorage
function clearAllUserData() {
    // Clear tokens
    setTokens(null, null);
    
    // Clear user document data
    localStorage.removeItem('currentDocumentId');
    localStorage.removeItem('currentText');
    localStorage.removeItem('currentTemplate');
}

// API Helper Functions
async function apiRequest(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    
    // Always read tokens from localStorage before making a request
    const accessToken = getAccessToken();
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    if (accessToken) {
        defaultOptions.headers['Authorization'] = `Bearer ${accessToken}`;
    }

    const finalOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...(options.headers || {}),
        },
    };

    try {
        const response = await fetch(url, finalOptions);
        
        // Read refreshToken from localStorage
        const refreshToken = getRefreshToken();
        if (response.status === 401 && refreshToken) {
            // Try to refresh token
            const refreshed = await refreshAccessToken();
            if (refreshed) {
                // Retry original request with new token
                const newAccessToken = getAccessToken();
                finalOptions.headers['Authorization'] = `Bearer ${newAccessToken}`;
                return await fetch(url, finalOptions);
            }
        }

        return response;
    } catch (error) {
        console.error('API request failed:', error);
        throw error;
    }
}

async function refreshAccessToken() {
    // Always read refreshToken from localStorage
    const refreshToken = getRefreshToken();
    if (!refreshToken) return false;

    try {
        const response = await fetch(`${API_BASE_URL}/refresh`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ refreshToken }),
        });

        if (response.ok) {
            const data = await response.json();
            // Update tokens in localStorage (setTokens автоматически перезапустит таймер)
            setTokens(data.accessToken, data.refreshToken);
            return true;
        }
    } catch (error) {
        console.error('Token refresh failed:', error);
    }

    // If refresh failed, logout
    stopTokenRefreshTimer();
    await logout();
    return false;
}

async function logout() {
    // Останавливаем таймер обновления токена
    stopTokenRefreshTimer();
    
    // Always read refreshToken from localStorage
    const refreshToken = getRefreshToken();
    
    // Call logout API endpoint if we have a refresh token
    if (refreshToken) {
        try {
            await apiRequest('/logout', {
                method: 'POST',
                body: JSON.stringify({ refreshToken }),
            });
        } catch (error) {
            console.error('Logout API call failed:', error);
            // Continue with local logout even if API call fails
        }
    }
    
    // Clear all user data from localStorage
    clearAllUserData();
    
    window.location.href = 'login.html';
}

// Auth API
async function loginFunc(login, password) {
    const response = await apiRequest('/login', {
        method: 'POST',
        body: JSON.stringify({ login, password }),
    });

    if (response.ok) {
        const data = await response.json();
        // Store tokens in localStorage (setTokens автоматически запустит таймер обновления)
        setTokens(data.accessToken, data.refreshToken);
        return { success: true };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || 'Login failed' };
    }
}

async function register(login, password, confirmPassword) {
    const response = await apiRequest('/register', {
        method: 'POST',
        body: JSON.stringify({ login: login, password, confirmPassword }),
    });

    if (response.ok) {
        // After registration, automatically login
        return await loginFunc(login, password);
    } else {
        const error = await response.json();
        return { success: false, error: error.error || 'Registration failed' };
    }
}

// Document API
async function createDocument(title, initialText) {
    const response = await apiRequest('/documents/create', {
        method: 'POST',
        body: JSON.stringify({ title, initialText }),
        
    });

    if (response.ok) {
        const data = await response.json();
        return { success: true, documentId: data.document_id };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to create document' };
    }
}

async function fullCommit(documentId, templateId) {
    const response = await apiRequest(`/documents/${documentId}/commit`, {
        method: 'POST',
        body: JSON.stringify({ templateId }),
    });

    if (response.ok) {
        return { success: true };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to apply template' };
    }
}

async function partialCommit(documentId, templateId, start, end) {
    const response = await apiRequest(`/documents/${documentId}/commit/partial`, {
        method: 'POST',
        body: JSON.stringify({ templateId, start, end }),
    });

    if (response.ok) {
        return { success: true };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to apply template' };
    }
}

async function getAllDocuments() {
    const response = await apiRequest('/documents/all');

    if (response.ok) {
        const data = await response.json();
        return { success: true, documents: data };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to get documents' };
    }
}

async function getHistory(documentId) {
    const response = await apiRequest(`/documents/${documentId}/history`);

    if (response.ok) {
        const data = await response.json();
        return { success: true, history: data };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to get history' };
    }
}

async function rollback(documentId, version) {
    const response = await apiRequest(`/documents/${documentId}/rollback/${version}`, {
        method: 'POST',
    });

    if (response.ok) {
        return { success: true };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to rollback' };
    }
}

// Template API
async function getAllTemplates() {
    const response = await apiRequest('/templates/all');

    if (response.ok) {
        const data = await response.json();
        return { success: true, templates: data };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to get templates' };
    }
}

async function createTemplate(name, rawText, isPublic) {
    const response = await apiRequest('/templates/create', {
        method: 'POST',
        body: JSON.stringify({ name, rawText, isPublic }),
    });

    if (response.ok) {
        const data = await response.json();
        return { success: true, templateId: data.template_id };
    } else {
        const error = await response.json();
        return { success: false, error: error.error || error.message || 'Failed to create template' };
    }
}

// User info (from JWT token)
function getUserInfo() {
    // Always read accessToken from localStorage
    const accessToken = getAccessToken();
    if (!accessToken) return null;
    
    try {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        return {
            id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
            role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User',
            login: payload.unique_name || null,
        };
    } catch (error) {
        return null;
    }
}

function isAuthenticated() {
    // Always check localStorage for accessToken
    return !!getAccessToken();
}

function getUserRole() {
    const userInfo = getUserInfo();
    return userInfo ? userInfo.role : 'Anonymous';
}

function getUserLogin() {
    const userInfo = getUserInfo();
    return userInfo ? userInfo.login : null;
}

// Инициализация: запустить таймер обновления токена при загрузке страницы, если пользователь авторизован
(function initTokenRefresh() {
    // Ждем загрузки DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            if (isAuthenticated()) {
                startTokenRefreshTimer();
            }
        });
    } else {
        // DOM уже загружен
        if (isAuthenticated()) {
            startTokenRefreshTimer();
        }
    }
})();

