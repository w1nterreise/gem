var style = document.createElement('style');
style.innerHTML = 'button[aria-label="Микрофон"], button[aria-label="Search by voice"], button[aria-label="Microphone"] { display: none !important; }';
document.head ? document.head.appendChild(style) : document.addEventListener('DOMContentLoaded', () => document.head.appendChild(style));
