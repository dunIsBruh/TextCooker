// Current document state
let currentDocumentId = localStorage.getItem('currentDocumentId') ? parseInt(localStorage.getItem('currentDocumentId')) : null;
let currentText = '';

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    const textInput = document.getElementById('textInput');
    const templateInput = document.getElementById('templateInput');
    const applyBtn = document.getElementById('applyTemplateBtn');
    const exportBtn = document.getElementById('exportBtn');

    // Check if template was selected from template list
    const selectedTemplate = localStorage.getItem('selectedTemplate');
    if (selectedTemplate) {
        templateInput.value = selectedTemplate;
        localStorage.setItem('currentTemplate', selectedTemplate);
        // Clear the selected template flag
        localStorage.removeItem('selectedTemplate');
    }

    // Load saved text if exists
    const savedText = localStorage.getItem('currentText');
    const savedTemplate = localStorage.getItem('currentTemplate');
    if (savedText) {
        textInput.value = savedText;
        currentText = savedText;
    }
    if (savedTemplate && !selectedTemplate) {
        templateInput.value = savedTemplate;
    }

    // Load latest commit if document exists
    if (currentDocumentId) {
        loadLatestCommit();
    }

    async function loadLatestCommit() {
        try {
            const historyResult = await getHistory(currentDocumentId);
            if (historyResult.success && historyResult.history.length > 0) {
                const lastCommit = historyResult.history[historyResult.history.length - 1];
                textInput.value = lastCommit.snapshot;
                currentText = lastCommit.snapshot;
                localStorage.setItem('currentText', currentText);
            }
        } catch (error) {
            console.error('Error loading latest commit:', error);
        }
    }

    // Save text on input
    textInput.addEventListener('input', () => {
        currentText = textInput.value;
        localStorage.setItem('currentText', currentText);
    });

    templateInput.addEventListener('input', () => {
        localStorage.setItem('currentTemplate', templateInput.value);
    });

    // Apply template
    applyBtn.addEventListener('click', async () => {
        const text = textInput.value;
        const templateText = templateInput.value.trim();

        if (!text.trim()) {
            showAlert('Введите текст для форматирования', 'error');
            return;
        }

        if (!templateText) {
            showAlert('Введите шаблон', 'error');
            return;
        }

        // Get current text (might be updated from history)
        const currentTextValue = textInput.value;
        
        // Check if text is selected
        const selectionStart = textInput.selectionStart;
        const selectionEnd = textInput.selectionEnd;
        const hasSelection = selectionStart !== selectionEnd && selectionStart >= 0 && selectionStart < currentTextValue.length;

        applyBtn.disabled = true;
        applyBtn.textContent = 'Применение...';

        try {
            // First, create or get document
            if (!currentDocumentId) {
                const result = await createDocument('Untitled Document', text);
                if (!result.success) {
                    showAlert(result.error || 'Ошибка создания документа', 'error');
                    applyBtn.disabled = false;
                    applyBtn.textContent = 'Применить шаблон';
                    return;
                }
                currentDocumentId = result.documentId;
                localStorage.setItem('currentDocumentId', currentDocumentId);
            } else {
                // Get latest commit to ensure we're working with the latest version
                const historyResult = await getHistory(currentDocumentId);
                if (historyResult.success && historyResult.history.length > 0) {
                    const lastCommit = historyResult.history[historyResult.history.length - 1];
                    // Update text input with latest version if it differs
                    if (textInput.value !== lastCommit.snapshot) {
                        textInput.value = lastCommit.snapshot;
                        currentText = lastCommit.snapshot;
                        // Reset selection if text was updated
                        textInput.setSelectionRange(0, 0);
                    }
                }
            }
            
            // Use current text from textarea for commit
            const textToUse = textInput.value;

            // Create template from input
            const templateResult = await createTemplate(
                `Template_${Date.now()}`,
                templateText,
                false
            );

            if (!templateResult.success) {
                showAlert(templateResult.error || 'Ошибка создания шаблона', 'error');
                applyBtn.disabled = false;
                applyBtn.textContent = 'Применить шаблон';
                return;
            }

            let commitResult;
            
            // Apply template to selected text or full text
            if (hasSelection) {
                // Partial commit - use current selection
                const start = Math.min(selectionStart, selectionEnd);
                const end = Math.max(selectionStart, selectionEnd);
                commitResult = await partialCommit(
                    currentDocumentId,
                    templateResult.templateId,
                    start,
                    end
                );
            } else {
                // Full commit
                commitResult = await fullCommit(currentDocumentId, templateResult.templateId);
            }

            if (!commitResult.success) {
                showAlert(commitResult.error || 'Ошибка применения шаблона', 'error');
                applyBtn.disabled = false;
                applyBtn.textContent = 'Применить шаблон';
                return;
            }

            // Get updated text from history
            const historyResult = await getHistory(currentDocumentId);
            if (historyResult.success && historyResult.history.length > 0) {
                const lastCommit = historyResult.history[historyResult.history.length - 1];
                currentText = lastCommit.snapshot;
                textInput.value = currentText;
                localStorage.setItem('currentText', currentText);
                showAlert(hasSelection ? 'Шаблон успешно применен к выделенному тексту!' : 'Шаблон успешно применен!', 'success');
            }

        } catch (error) {
            console.error('Error applying template:', error);
            showAlert('Произошла ошибка при применении шаблона', 'error');
        } finally {
            applyBtn.disabled = false;
            applyBtn.textContent = 'Применить шаблон';
        }
    });

    // Export text
    exportBtn.addEventListener('click', () => {
        const text = textInput.value;
        if (!text.trim()) {
            showAlert('Нет текста для экспорта', 'error');
            return;
        }

        const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `text_${new Date().toISOString().slice(0, 10)}.txt`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        showAlert('Текст успешно экспортирован!', 'success');
    });
});

function showAlert(message, type) {
    const container = document.getElementById('alertContainer');
    container.innerHTML = `<div class="alert alert-${type} show">${message}</div>`;
    setTimeout(() => {
        container.innerHTML = '';
    }, 5000);
}
