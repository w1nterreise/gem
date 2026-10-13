(function () {
    var style = document.createElement('style');
    style.innerHTML = `
        .webview-toast { 
            position: fixed; 
            bottom: 25px; 
            left: 50%; 
            transform: translateX(-50%); 
            background-color: #323232; 
            color: #ffffff; 
            padding: 10px 20px; 
            border-radius: 4px; 
            font-family: system-ui, -apple-system, sans-serif; 
            font-size: 14px; 
            z-index: 999999; 
            opacity: 0; 
            transition: opacity 0.3s ease, bottom 0.3s ease; 
            pointer-events: none; 
            box-shadow: 0 3px 6px rgba(0,0,0,0.3); 
        } 
        .webview-toast.show { 
            opacity: 1; 
            bottom: 35px;
        }
    `;
    if (document.head) {
        document.head.appendChild(style);
    } else {
        document.addEventListener('DOMContentLoaded', () => document.head.appendChild(style));
    }

    function showToast(text) {
        let toast = document.querySelector('.webview-toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.className = 'webview-toast';
            document.body.appendChild(toast);
        }
        toast.innerText = text;
        toast.classList.add('show');
        setTimeout(() => {
            toast.classList.remove('show');
        }, 666);
    }

    document.addEventListener('mousedown', function (event) {
        if (event.button === 2 && (event.shiftKey || event.ctrlKey || event.altKey)) {
            event.preventDefault();
        }
    }, true);

    document.addEventListener('mouseup', function (event) {
        window.chrome.webview.postMessage('CLOSE_MENU');

        var selection = window.getSelection();
        if (!selection.isCollapsed && selection.rangeCount > 0) {
            var range = selection.getRangeAt(0);
            var boundary = /[\s,.\-!?"";:()«»''’]/;

            var initialText = range.toString().trim();
            var isWithinSingleWord = range.startContainer === range.endContainer && !boundary.test(initialText);

            if (!isWithinSingleWord) {
                while (range.startOffset > 0 && !boundary.test(range.startContainer.textContent[range.startOffset - 1])) {
                    range.setStart(range.startContainer, range.startOffset - 1);
                }
                while (range.endOffset < range.endContainer.textContent.length && !boundary.test(range.endContainer.textContent[range.endOffset])) {
                    range.setEnd(range.endContainer, range.endOffset + 1);
                }
                selection.removeAllRanges();
                selection.addRange(range);
            }
        }

        var selectedText = selection.toString();
        if (selectedText && selectedText.trim().length > 0) {
            showToast('Текст скопирован');
            window.chrome.webview.postMessage('AUTO_COPY|||' + selectedText);
        }
    });

    document.addEventListener('click', function (e) {
        const anchor = e.target.closest('a');
        if (anchor) {
            const targetUrl = anchor.href;
            if (targetUrl && !targetUrl.startsWith('javascript:') && !targetUrl.includes('google.com')) {
                e.preventDefault();
                e.stopPropagation();
                showToast('Ссылка скопирована в буфер обмена');
                window.chrome.webview.postMessage('COPY_LINK|||' + targetUrl);
            }
        }
    }, true);
})();
