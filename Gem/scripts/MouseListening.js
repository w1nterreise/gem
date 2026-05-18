document.addEventListener('mousedown', function(event) {
    if (event.button === 2 && (event.shiftKey || event.ctrlKey || event.altKey)) {
        event.preventDefault();
    }
}, true);

document.addEventListener('mouseup', function(event) {
    window.chrome.webview.postMessage('CLOSE_MENU');

    var selectedText = window.getSelection().toString();
    if (selectedText && selectedText.trim().length > 0) {
        window.chrome.webview.postMessage('AUTO_COPY|||' + selectedText);
    }
});