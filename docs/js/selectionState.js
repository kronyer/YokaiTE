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
        var state = {
            Bold: false,
            Italic: false,
            Underline: false,
            Strike: false,
            TextAlign: "left"
        };

        if (selection.focusNode) {
            var node = selection.focusNode;
            var blockElement = null;

            // Encontra o elemento de bloco mais próximo para verificar alinhamento
            while (node && node.nodeType !== 9) {
                if (node.nodeType === 1) {
                    var nodeName = node.nodeName.toLowerCase();

                    // Detecta formatação de texto
                    if (nodeName === "b" || nodeName === "strong") state.Bold = true;
                    if (nodeName === "i" || nodeName === "em") state.Italic = true;
                    if (nodeName === "u") state.Underline = true;
                    if (nodeName === "s" || nodeName === "strike") state.Strike = true;

                    // Detecta elemento de bloco para alinhamento
                    if (!blockElement && (nodeName === "div" || nodeName === "p" || nodeName === "h1" ||
                        nodeName === "h2" || nodeName === "h3" || nodeName === "h4" || nodeName === "h5" ||
                        nodeName === "h6" || nodeName === "blockquote" || nodeName === "li")) {
                        blockElement = node;
                    }
                }
                node = node.parentNode;
            }

            // Verifica alinhamento no elemento de bloco
            if (blockElement) {
                var computedStyle = window.getComputedStyle(blockElement);
                var textAlign = computedStyle.textAlign;

                if (textAlign === "center") state.TextAlign = "center";
                else if (textAlign === "right") state.TextAlign = "right";
                else if (textAlign === "justify") state.TextAlign = "full";
                else state.TextAlign = "left";
            }
        }
        return state;
    }
};