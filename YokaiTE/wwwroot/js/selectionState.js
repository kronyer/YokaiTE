window.selectionStateInterop = {
    registerSelectionEvents: function (dotNetRef) {
        // Evite múltiplos registros
        if (window._selectionStateInteropRegistered) return;
        window._selectionStateInteropRegistered = true;

        window._selectionStateInteropNotify = function () {
            dotNetRef.invokeMethodAsync('UpdateSelectionFormatting');
        };

        document.addEventListener('selectionchange', window._selectionStateInteropNotify);
        document.addEventListener('keyup', window._selectionStateInteropNotify);
        document.addEventListener('mouseup', window._selectionStateInteropNotify);
    },
    unregisterSelectionEvents: function () {
        if (!window._selectionStateInteropRegistered) return;
        document.removeEventListener('selectionchange', window._selectionStateInteropNotify);
        document.removeEventListener('keyup', window._selectionStateInteropNotify);
        document.removeEventListener('mouseup', window._selectionStateInteropNotify);
        window._selectionStateInteropRegistered = false;
        window._selectionStateInteropNotify = null;
    },
    getFormatting: function () {
        var selection = window.getSelection();
        var state = { Bold: false, Italic: false, Underline: false, Strike: false };
        if (!selection.isCollapsed && selection.focusNode) {
            var node = selection.focusNode;
            while (node) {
                if (node.nodeName === "B" || node.nodeName === "STRONG") state.Bold = true;
                if (node.nodeName === "I" || node.nodeName === "EM") state.Italic = true;
                if (node.nodeName === "U") state.Underline = true;
                if (node.nodeName === "S" || node.nodeName === "STRIKE") state.Strike = true;
                node = node.parentNode;
            }
        }
        return state;
    }
};