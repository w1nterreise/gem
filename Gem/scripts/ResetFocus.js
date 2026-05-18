(function() {
    var escEvent = new KeyboardEvent('keydown', {
        bubbles: true, cancelable: true, key: 'Escape', code: 'Escape', keyCode: 27
    });
    document.dispatchEvent(escEvent);
})();