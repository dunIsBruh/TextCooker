document.addEventListener('DOMContentLoaded', () => {
    const refreshBtn = document.getElementById('refreshBtn');
    const templateGrid = document.getElementById('templateGrid');

    refreshBtn.addEventListener('click', loadTemplates);
    
    // Load templates on page load
    loadTemplates();

    async function loadTemplates() {
        refreshBtn.disabled = true;
        refreshBtn.textContent = 'Загрузка...';
        templateGrid.innerHTML = '<div class="loading">Загрузка шаблонов...</div>';

        try {
            const result = await getAllTemplates();
            
            if (result.success) {
                displayTemplates(result.templates);
            } else {
                templateGrid.innerHTML = `<div class="alert alert-error show">${result.error || 'Ошибка загрузки шаблонов'}</div>`;
            }
        } catch (error) {
            console.error('Error loading templates:', error);
            templateGrid.innerHTML = '<div class="alert alert-error show">Произошла ошибка при загрузке шаблонов</div>';
        } finally {
            refreshBtn.disabled = false;
            refreshBtn.textContent = 'Обновить список';
        }
    }

    function displayTemplates(templates) {
        if (!templates || templates.length === 0) {
            templateGrid.innerHTML = '<div class="loading">Шаблоны не найдены</div>';
            return;
        }

        templateGrid.innerHTML = '';

        templates.forEach(template => {
            const card = document.createElement('div');
            card.className = 'template-card';
            
            const content = template.text || template.rawText || '';
            const truncatedContent = content.length > 200 ? content.substring(0, 200) + '...' : content;

            card.innerHTML = `
                <h3>${escapeHtml(template.name || 'Без названия')}</h3>
                <div class="template-content">${escapeHtml(truncatedContent)}</div>
                <button class="btn btn-success use-template-btn" data-template-id="${template.templateId}">Use</button>
            `;

            const useBtn = card.querySelector('.use-template-btn');
            useBtn.addEventListener('click', () => {
                useTemplate(template);
            });

            templateGrid.appendChild(card);
        });
    }

    function useTemplate(template) {
        // Get template text (rawText or text)
        const templateText = template.rawText || template.text || '';
        
        if (!templateText) {
            showAlert('Шаблон не содержит текста', 'error');
            return;
        }

        // Save template text to localStorage
        localStorage.setItem('selectedTemplate', templateText);
        
        // Redirect to index page
        window.location.href = 'index.html';
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function showAlert(message, type) {
        const container = document.getElementById('alertContainer');
        container.innerHTML = `<div class="alert alert-${type} show">${message}</div>`;
        setTimeout(() => {
            container.innerHTML = '';
        }, 5000);
    }
});


