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
        const sel = window.getSelection();
        const state = {
            Bold: false,
            Italic: false,
            Underline: false,
            Strike: false,
            TextAlign: "left",
            FontSize: ""
        };

        if (!sel || !sel.rangeCount) return state;

        const range = sel.getRangeAt(0);
        const focusNode = sel.focusNode;
        if (!focusNode) return state;

        const focusEl = focusNode.nodeType === Node.ELEMENT_NODE
            ? focusNode
            : focusNode.parentElement;
        if (!focusEl) return state;

        const editorRoot = focusEl.closest('[contenteditable="true"]');
        if (!editorRoot) return state;

        // === FONT-SIZE com prioridade para spans ===
        let fontSize = "";

        // Se há seleção, verifica se há múltiplos tamanhos
        if (!range.collapsed) {
            const fontSizes = new Set();
            const walker = document.createTreeWalker(
                range.commonAncestorContainer,
                NodeFilter.SHOW_ELEMENT,
                {
                    acceptNode: function(node) {
                        return range.intersectsNode(node) ?
                            NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
                    }
                }
            );

            let node;
            while (node = walker.nextNode()) {
                const fs = window.getComputedStyle(node).fontSize;
                if (fs) fontSizes.add(fs);
            }

            // Se múltiplos tamanhos, deixa vazio
            if (fontSizes.size > 1) {
                fontSize = "";
            } else if (fontSizes.size === 1) {
                fontSize = Array.from(fontSizes)[0];
            }
        } else {
            // Cursor sem seleção - pega do elemento mais próximo
            let node = focusEl;
            while (node && node !== editorRoot) {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    const fs = window.getComputedStyle(node).fontSize;
                    if (fs && node.style.fontSize) { // Prioriza elementos com style inline
                        fontSize = fs;
                        break;
                    }
                }
                node = node.parentElement;
            }

            // Se não achou com style inline, pega qualquer um
            if (!fontSize) {
                node = focusEl;
                while (node && node !== editorRoot) {
                    if (node.nodeType === Node.ELEMENT_NODE) {
                        const fs = window.getComputedStyle(node).fontSize;
                        if (fs) {
                            fontSize = fs;
                            break;
                        }
                    }
                    node = node.parentElement;
                }
            }
        }

        state.FontSize = fontSize;

        // === Formatação inline ===
        let node = focusNode;
        while (node && node !== editorRoot) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                const name = node.nodeName.toLowerCase();
                if (name === 'b' || name === 'strong') state.Bold = true;
                if (name === 'i' || name === 'em') state.Italic = true;
                if (name === 'u') state.Underline = true;
                if (name === 's' || name === 'strike') state.Strike = true;
            }
            node = node.parentNode;
        }

        // === Alinhamento do bloco ===
        if (blockElement) {
            const textAlign = window.getComputedStyle(blockElement).textAlign;
            if (textAlign === "center") state.TextAlign = "center";
            else if (textAlign === "right") state.TextAlign = "right";
            else if (textAlign === "justify") state.TextAlign = "full";
            else state.TextAlign = "left";
        }

        return state;
    },
    applyFontColor: function (color) {
        document.execCommand('foreColor', false, color);
    },
    applyHighlightColor: function (color) {
        document.execCommand('hiliteColor', false, color);
    },
    applyFontFamily: function (font) {
        document.execCommand('fontName', false, font);
    }
};