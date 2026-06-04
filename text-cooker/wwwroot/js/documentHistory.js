let currentHistory = [];
let currentDocumentId = null;
let templatesCache = null; // Cache для шаблонов
let allDocuments = []; // Cache для всех документов

document.addEventListener('DOMContentLoaded', () => {
    const documentSelect = document.getElementById('documentSelect');
    const historyContainer = document.getElementById('historyContainer');

    // Load all documents and populate dropdown
    loadDocuments();

    // Handle document selection
    documentSelect.addEventListener('change', () => {
        const selectedValue = documentSelect.value;
        if (!selectedValue) {
            currentDocumentId = null;
            historyContainer.innerHTML = '<div class="loading">Выберите документ для просмотра истории</div>';
            return;
        }
        currentDocumentId = parseInt(selectedValue);
        localStorage.setItem('currentDocumentId', selectedValue);
        loadHistory();
    });

    // Try to get document ID from localStorage and select it
    const savedDocId = localStorage.getItem('currentDocumentId');
    if (savedDocId) {
        // Will be set after documents are loaded
        currentDocumentId = parseInt(savedDocId);
    }

    async function loadDocuments() {
        try {
            const result = await getAllDocuments();
            if (result.success) {
                allDocuments = result.documents;
                populateDropdown(result.documents);
                
                // Select saved document if exists
                if (currentDocumentId) {
                    documentSelect.value = currentDocumentId.toString();
                    loadHistory();
                }
            } else {
                documentSelect.innerHTML = '<option value="">Ошибка загрузки документов</option>';
                showAlert(result.error || 'Ошибка загрузки документов', 'error');
            }
        } catch (error) {
            console.error('Error loading documents:', error);
            documentSelect.innerHTML = '<option value="">Ошибка загрузки документов</option>';
            showAlert('Произошла ошибка при загрузке документов', 'error');
        }
    }

    function populateDropdown(documents) {
        documentSelect.innerHTML = '<option value="">Выберите документ...</option>';
        
        if (!documents || documents.length === 0) {
            documentSelect.innerHTML = '<option value="">Нет доступных документов</option>';
            return;
        }

        documents.forEach(doc => {
            const option = document.createElement('option');
            option.value = doc.documentId.toString();
            // Display first 40 characters of preview text
            const displayText = doc.previewText || '(пустой документ)';
            option.textContent = displayText;
            documentSelect.appendChild(option);
        });
    }

    async function loadHistory() {
        if (!currentDocumentId) return;

        historyContainer.innerHTML = '<div class="loading">Загрузка истории...</div>';

        try {
            // Load templates cache if not already loaded
            if (!templatesCache) {
                const templatesResult = await getAllTemplates();
                if (templatesResult.success) {
                    templatesCache = templatesResult.templates;
                }
            }
            
            const result = await getHistory(currentDocumentId);
            
            if (result.success) {
                currentHistory = result.history;
                displayHistory(result.history);
            } else {
                historyContainer.innerHTML = `<div class="alert alert-error show">${result.error || 'Ошибка загрузки истории'}</div>`;
            }
        } catch (error) {
            console.error('Error loading history:', error);
            historyContainer.innerHTML = '<div class="alert alert-error show">Произошла ошибка при загрузке истории</div>';
        }
    }
    
    function getTemplateText(templateId) {
        if (!templatesCache || !templateId) return null;
        const template = templatesCache.find(t => t.templateId === templateId);
        return template ? (template.text || template.rawText || '') : null;
    }

    function displayHistory(history) {
        if (!history || history.length === 0) {
            historyContainer.innerHTML = '<div class="loading">История пуста</div>';
            return;
        }

        historyContainer.innerHTML = '';

        for (let i = 0; i < history.length; i++) {
            const commit = history[i];
            const prevCommit = i > 0 ? history[i - 1] : null;

            // Display template message (user message) if exists
            if (commit.templateId && prevCommit) {
                const templateText = getTemplateText(commit.templateId);
                const templateMsg = document.createElement('div');
                templateMsg.className = 'message user';
                
                let templateContent = 'Template ID: ' + commit.templateId;
                if (templateText) {
                    // Show template text (truncated if too long)
                    const truncatedText = templateText.length > 200 
                        ? templateText.substring(0, 200) + '...' 
                        : templateText;
                    templateContent = escapeHtml(truncatedText);
                }
                
                templateMsg.innerHTML = `
                    <div class="message-content">
                        <strong>Шаблон применен</strong><br>
                        ${templateContent}
                    </div>
                `;
                historyContainer.appendChild(templateMsg);
            }

            // Display commit snapshot (system message)
            const commitMsg = document.createElement('div');
            commitMsg.className = 'message system';
            
            let content = commit.snapshot;
            
            // Highlight changed part if there was a previous commit
            if (prevCommit && commit.changeStart !== undefined && commit.changeEnd !== undefined) {
                const before = commit.snapshot.substring(0, commit.changeStart);
                const changed = commit.snapshot.substring(commit.changeStart, commit.changeEnd);
                const after = commit.snapshot.substring(commit.changeEnd);
                
                content = `${escapeHtml(before)}<span class="highlighted-text">${escapeHtml(changed)}</span>${escapeHtml(after)}`;
            } else {
                content = escapeHtml(content);
            }

            commitMsg.innerHTML = `
                <div class="message-content">
                    <strong>Версия ${commit.versionNumber}</strong><br>
                    ${content}
                </div>
                ${i < history.length - 1 ? `<button class="btn btn-danger rollback-btn" data-version="${commit.versionNumber}">Rollback</button>` : ''}
            `;

            historyContainer.appendChild(commitMsg);

            // Add rollback handler
            if (i < history.length - 1) {
                const rollbackBtn = commitMsg.querySelector('.rollback-btn');
                rollbackBtn.addEventListener('click', async () => {
                    await rollbackToVersion(commit.versionNumber);
                });
            }
        }

        // Scroll to bottom
        historyContainer.scrollTop = historyContainer.scrollHeight;
    }

    async function rollbackToVersion(version) {
        if (!currentDocumentId) return;

        if (!confirm(`Вы уверены, что хотите откатиться к версии ${version}?`)) {
            return;
        }

        try {
            const result = await rollback(currentDocumentId, version);
            
            if (result.success) {
                showAlert(`Откат к версии ${version} выполнен успешно`, 'success');
                // Reload history
                await loadHistory();
            } else {
                showAlert(result.error || 'Ошибка отката', 'error');
            }
        } catch (error) {
            console.error('Error rolling back:', error);
            showAlert('Произошла ошибка при откате', 'error');
        }
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


