(function() {

    var escEvent = new KeyboardEvent('keydown', {
        bubbles: true, cancelable: true, key: 'Escape', code: 'Escape', keyCode: 27
    });
    document.dispatchEvent(escEvent);

    var textareas = document.querySelectorAll('textarea, [contenteditable="true"]');
    if (textareas.length < 1) return;
    
    var textarea = textareas[textareas.length - 1];
    textarea.value = /*PLACEHOLDER*/; 
    
    textarea.dispatchEvent(new Event('input', { bubbles: true }));
    textarea.focus();
    
    var enterEvent = new KeyboardEvent('keydown', {
        bubbles: true, cancelable: true, key: 'Enter', code: 'Enter', keyCode: 13
    });
    textarea.dispatchEvent(enterEvent);
})();