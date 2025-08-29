(function () {
    // ---------- helpers ----------
    function isBlock(el) {
        if (!el || el.nodeType !== Node.ELEMENT_NODE) return false;
        const name = el.nodeName.toLowerCase();
        return name === "div" || name === "p" || name === "blockquote" || name === "li" ||
            name === "h1" || name === "h2" || name === "h3" || name === "h4" || name === "h5" || name === "h6";
    }

    function unwrap(node) {
        const parent = node.parentNode;
        if (!parent) return;
        while (node.firstChild) parent.insertBefore(node.firstChild, node);
        parent.removeChild(node);
    }

    function removeEmptySpans(root) {
        const spans = root.querySelectorAll("span");
        spans.forEach(s => {
            const onlyWhitespace = !s.textContent || s.textContent.replace(/\u200B/g, "").trim() === "";
            const noStyle = !s.getAttribute("style");
            if ((onlyWhitespace && s.childNodes.length === 0) || (noStyle && s.attributes.length === 0)) {
                unwrap(s);
            }
        });
    }

    function unwrapSpansAroundBlocks(root) {
        const spans = root.querySelectorAll("span");
        spans.forEach(s => {
            const hasBlockChild = Array.from(s.childNodes).some(n => isBlock(n));
            if (hasBlockChild) unwrap(s);
        });
    }

    function mergeAdjacentSpans(root) {
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT, {
            acceptNode(n) { return n.nodeName === "SPAN" ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP; }
        });
        const toRemove = [];
        let n;
        while ((n = walker.nextNode())) {
            const next = n.nextSibling;
            if (next && next.nodeType === Node.ELEMENT_NODE && next.nodeName === "SPAN") {
                const a = (n.getAttribute("style") || "").trim();
                const b = (next.getAttribute("style") || "").trim();
                if (a === b) {
                    while (next.firstChild) n.appendChild(next.firstChild);
                    toRemove.push(next);
                }
            }
        }
        toRemove.forEach(el => el.remove());
    }

    function removeEmptyBlocks(root) {
        const blocks = root.querySelectorAll("div,p,blockquote,li,h1,h2,h3,h4,h5,h6");
        blocks.forEach(b => {
            const text = (b.textContent || "").replace(/\u200B/g, "").trim();
            if (text === "") b.remove();
        });
    }

    function normalizeEditor(root) {
        if (!root) return;
        unwrapSpansAroundBlocks(root);
        removeEmptySpans(root);
        mergeAdjacentSpans(root);
        removeEmptyBlocks(root);
    }

    function getEditorRootFromSelection() {
        const sel = window.getSelection();
        if (!sel || !sel.focusNode) return null;
        const focusEl = sel.focusNode.nodeType === Node.ELEMENT_NODE ? sel.focusNode : sel.focusNode.parentElement;
        return focusEl ? focusEl.closest('[contenteditable="true"]') : null;
    }

    function ensureSpanForTextNode(textNode) {
        const parent = textNode.parentNode;
        if (parent && parent.nodeType === Node.ELEMENT_NODE && parent.nodeName === "SPAN") {
            return parent; // reuse
        }
        const span = document.createElement("span");
        parent.insertBefore(span, textNode);
        span.appendChild(textNode);
        return span;
    }

    function splitTextNodeToSelection(textNode, range) {
        let tn = textNode;
        let start = 0, end = tn.data.length;
        if (tn === range.startContainer) start = range.startOffset;
        if (tn === range.endContainer) end = range.endOffset;

        if (end < tn.length) tn.splitText(end);
        if (start > 0) tn = tn.splitText(start);
        return tn; // fully inside selection
    }

    // ---------- apply: font-size, color, highlight, family ----------
    function applyInlineStyle(propName, value) {
        const sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) return;
        const range = sel.getRangeAt(0);
        const editorRoot = getEditorRootFromSelection();
        if (!editorRoot) return;

        if (range.collapsed) {
            const caretEl = sel.focusNode.nodeType === Node.ELEMENT_NODE ? sel.focusNode : sel.focusNode.parentElement;
            let span = caretEl && caretEl.closest("span");
            if (!span || !editorRoot.contains(span)) {
                span = document.createElement("span");
                const zwsp = document.createTextNode("\u200B");
                range.insertNode(span);
                span.appendChild(zwsp);
                sel.removeAllRanges();
                const r = document.createRange();
                r.setStart(zwsp, 1);
                r.collapse(true);
                sel.addRange(r);
            }
            span.style[propName] = value;
            normalizeEditor(editorRoot);
            return;
        }

        const ancestor = range.commonAncestorContainer.nodeType === Node.ELEMENT_NODE
            ? range.commonAncestorContainer
            : range.commonAncestorContainer.parentElement;

        const walker = document.createTreeWalker(ancestor, NodeFilter.SHOW_TEXT, {
            acceptNode(n) {
                if (!range.intersectsNode(n)) return NodeFilter.FILTER_REJECT;
                return n.nodeValue.trim().length > 0 ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
            }
        });

        const targets = [];
        let t;
        while ((t = walker.nextNode())) targets.push(t);

        targets.forEach(textNode => {
            const tn = splitTextNodeToSelection(textNode, range);
            const span = ensureSpanForTextNode(tn);
            span.style[propName] = value;
        });

        normalizeEditor(editorRoot);
    }

    function applyFontSize(size) {
        applyInlineStyle("fontSize", size);
    }

    function applyFontColor(color) {
        applyInlineStyle("color", color);
    }

    function applyHighlightColor(color) {
        applyInlineStyle("backgroundColor", color);
    }

    function applyFontFamily(family) {
        applyInlineStyle("fontFamily", family);
    }

    // ---------- apply: text-align ----------
    function applyTextAlign(align) {
        const sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) return;
        const range = sel.getRangeAt(0);
        const editorRoot = getEditorRootFromSelection();
        if (!editorRoot) return;

        const cssVal = align === "full" ? "justify" : align;

        const collectBlockForNode = (n) => {
            if (!n) return null;
            let el = n.nodeType === Node.ELEMENT_NODE ? n : n.parentElement;
            while (el && el !== editorRoot) {
                if (isBlock(el)) return el;
                el = el.parentElement;
            }
            return editorRoot;
        };

        if (range.collapsed) {
            const block = collectBlockForNode(sel.focusNode);
            if (block) block.style.textAlign = cssVal;
            normalizeEditor(editorRoot);
            return;
        }

        const root = (range.commonAncestorContainer.nodeType === Node.ELEMENT_NODE)
            ? range.commonAncestorContainer
            : range.commonAncestorContainer.parentElement;

        const blocks = new Set();

        if (isBlock(root) && range.intersectsNode(root)) blocks.add(root);

        const iter = document.createNodeIterator(root, NodeFilter.SHOW_ELEMENT);
        let el;
        while ((el = iter.nextNode())) {
            if (isBlock(el) && range.intersectsNode(el)) blocks.add(el);
        }

        if (blocks.size === 0) {
            const a = collectBlockForNode(range.startContainer);
            const b = collectBlockForNode(range.endContainer);
            if (a) blocks.add(a);
            if (b) blocks.add(b);
        }

        blocks.forEach(b => b.style.textAlign = cssVal);
        normalizeEditor(editorRoot);
    }

    // ---------- selection state ----------
    function getFormatting() {
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

        const focusEl = focusNode.nodeType === Node.ELEMENT_NODE ? focusNode : focusNode.parentElement;
        if (!focusEl) return state;

        const editorRoot = focusEl.closest('[contenteditable="true"]');
        if (!editorRoot) return state;

        // Font size (handles text-node ancestors and mixed selections)
        let fontSize = "";

        if (!range.collapsed) {
            const sizes = new Set();

            const root = (range.commonAncestorContainer.nodeType === Node.ELEMENT_NODE)
                ? range.commonAncestorContainer
                : range.commonAncestorContainer.parentElement;

            if (root && range.intersectsNode(root)) {
                const fs = window.getComputedStyle(root).fontSize;
                if (fs) sizes.add(fs);
            }

            const iter = document.createNodeIterator(root, NodeFilter.SHOW_ELEMENT);
            let el;
            while ((el = iter.nextNode())) {
                if (range.intersectsNode(el)) {
                    const fs = window.getComputedStyle(el).fontSize;
                    if (fs) sizes.add(fs);
                }
            }

            if (sizes.size === 0) {
                const startEl = (range.startContainer.nodeType === Node.ELEMENT_NODE)
                    ? range.startContainer
                    : range.startContainer.parentElement;
                const endEl = (range.endContainer.nodeType === Node.ELEMENT_NODE)
                    ? range.endContainer
                    : range.endContainer.parentElement;

                const fsStart = startEl ? window.getComputedStyle(startEl).fontSize : "";
                const fsEnd = endEl ? window.getComputedStyle(endEl).fontSize : "";
                if (fsStart && fsStart === fsEnd) fontSize = fsStart;
            } else {
                fontSize = (sizes.size === 1) ? Array.from(sizes)[0] : "";
            }
        } else {
            let n = focusEl;
            while (n && n !== editorRoot) {
                if (n.nodeType === Node.ELEMENT_NODE) {
                    const fs = window.getComputedStyle(n).fontSize;
                    if (fs && n.style && n.style.fontSize) { fontSize = fs; break; }
                }
                n = n.parentElement;
            }
            if (!fontSize) {
                n = focusEl;
                while (n && n !== editorRoot) {
                    if (n.nodeType === Node.ELEMENT_NODE) {
                        const fs = window.getComputedStyle(n).fontSize;
                        if (fs) { fontSize = fs; break; }
                    }
                    n = n.parentElement;
                }
            }
        }

        state.FontSize = fontSize;

        // Inline formatting + nearest block for alignment
        let node = focusNode;
        let blockElement = null;

        while (node && node !== editorRoot && node.nodeType !== Node.DOCUMENT_NODE) {
            if (node.nodeType === Node.ELEMENT_NODE) {
                const name = node.nodeName.toLowerCase();

                if (name === "b" || name === "strong") state.Bold = true;
                if (name === "i" || name === "em") state.Italic = true;
                if (name === "u") state.Underline = true;
                if (name === "s" || name === "strike") state.Strike = true;

                if (!blockElement && isBlock(node)) blockElement = node;
            }
            node = node.parentNode;
        }

        if (blockElement) {
            const textAlign = window.getComputedStyle(blockElement).textAlign;
            if (textAlign === "center") state.TextAlign = "center";
            else if (textAlign === "right") state.TextAlign = "right";
            else if (textAlign === "justify") state.TextAlign = "full";
            else state.TextAlign = "left";
        }

        return state;
    }

    // ---------- wire up ----------
    const interop = window.selectionStateInterop || {};
    interop.registerSelectionEvents = function (dotNetRef) {
        if (window._selectionStateInteropRegistered) return;
        window._selectionStateInteropRegistered = true;

        window._selectionStateInteropNotify = function () {
            dotNetRef.invokeMethodAsync('UpdateSelectionFormatting');
        };

        document.addEventListener('selectionchange', window._selectionStateInteropNotify);
        document.addEventListener('keyup', window._selectionStateInteropNotify);
        document.addEventListener('mouseup', window._selectionStateInteropNotify);
    };
    interop.unregisterSelectionEvents = function () {
        if (!window._selectionStateInteropRegistered) return;
        document.removeEventListener('selectionchange', window._selectionStateInteropNotify);
        document.removeEventListener('keyup', window._selectionStateInteropNotify);
        document.removeEventListener('mouseup', window._selectionStateInteropNotify);
        window._selectionStateInteropRegistered = false;
        window._selectionStateInteropNotify = null;
    };
    interop.getFormatting = getFormatting;

    // editor ops used from C\#
    interop.applyFontColor = applyFontColor;
    interop.applyHighlightColor = applyHighlightColor;
    interop.applyFontFamily = applyFontFamily;
    interop.normalizeCurrentEditor = function () {
        const root = getEditorRootFromSelection();
        if (root) normalizeEditor(root);
    };

    window.selectionStateInterop = interop;

    // globals expected by `Edit.razor`
    window.applyFontSize = applyFontSize;
    window.applyTextAlign = applyTextAlign;
})();