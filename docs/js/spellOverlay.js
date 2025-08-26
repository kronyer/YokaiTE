// wwwroot/spell-overlay.js
const rx = /[\p{L}\p{M}]+(?:[-’'][\p{L}\p{M}]+)*/gu;

function raf2(fn) {
    // espera 2 frames para garantir layout/estilos aplicados
    requestAnimationFrame(() => requestAnimationFrame(fn));
}

function buildIndex(editor) {
    const walker = document.createTreeWalker(editor, NodeFilter.SHOW_TEXT, {
        acceptNode(node) { return /\S/.test(node.nodeValue || '') ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT; }
    });
    const items = [];
    let node, cum = 0;
    while ((node = walker.nextNode())) {
        const text = node.nodeValue || '';
        items.push({ node, start: cum, end: cum + text.length });
        cum += text.length;
    }
    return { items, length: cum };
}

function posFromIndex(index, indexer) {
    // varredura simples; pode trocar por binária se quiser
    for (const it of indexer.items) {
        if (index >= it.start && index <= it.end) {
            return { node: it.node, offset: index - it.start };
        }
    }
    const last = indexer.items[indexer.items.length - 1];
    return last ? { node: last.node, offset: last.node.nodeValue.length } : null;
}

function ensureCanvasSize(canvas, editor) {
    if (!editor || typeof editor.getBoundingClientRect !== 'function') return null;
    const rect = editor.getBoundingClientRect();
    const dpr = window.devicePixelRatio || 1;
    const w = Math.max(1, Math.floor(rect.width * dpr));
    const h = Math.max(1, Math.floor(rect.height * dpr));
    if (canvas.width !== w || canvas.height !== h) {
        canvas.width = w; canvas.height = h;
        canvas.style.width = rect.width + 'px';
        canvas.style.height = rect.height + 'px';
    }
    const ctx = canvas.getContext('2d');
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    return ctx;
}

function drawUnderlineRects(ctx, editorRect, rects) {
    ctx.setLineDash([2, 2]);
    for (const r of rects) {
        const x1 = r.left - editorRect.left;
        const x2 = r.right - editorRect.left;
        const y = r.bottom - editorRect.top;
        ctx.beginPath();
        ctx.moveTo(x1, y);
        ctx.lineTo(x2, y);
        ctx.stroke();
    }
}

export function init(editor, canvas) {
    raf2(() => {
        const ctx = ensureCanvasSize(canvas, editor);
        if (!ctx) return;
        ctx.strokeStyle = 'red';
        ctx.lineWidth = 2;

        // resize do editor
        const ro = new ResizeObserver(() => ensureCanvasSize(canvas, editor));
        ro.observe(editor);
        editor.__spellOverlayRO = ro;

        // sincroniza com scroll
        editor.addEventListener('scroll', () => ensureCanvasSize(canvas, editor), { passive: true });
    });
}

export function check(editor, canvas, predicate = (w) => w.toLowerCase() === 'blazor') {
    raf2(() => {
        const ctx = ensureCanvasSize(canvas, editor);
        if (!ctx) return;
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        const text = editor.textContent || '';
        const indexer = buildIndex(editor);
        const editorRect = editor.getBoundingClientRect();

        rx.lastIndex = 0;
        for (let m; (m = rx.exec(text));) {
            const word = m[0];
            if (!predicate(word)) continue;

            const start = m.index, end = start + word.length;
            const a = posFromIndex(start, indexer);
            const b = posFromIndex(end, indexer);
            if (!a || !b) continue;

            const range = document.createRange();
            range.setStart(a.node, a.offset);
            range.setEnd(b.node, b.offset);

            const rects = range.getClientRects();
            drawUnderlineRects(ctx, editorRect, rects);
        }
    });
}

// spellOverlay.js (ESM ou global; adapte o export conforme seu arquivo)
export function dispose(editor, canvas) {
    // nada a fazer se não há editor
    if (!editor || typeof editor !== 'object') return;

    // 1) parar ResizeObserver, se existir
    const ro = editor.__spellOverlayRO;
    if (ro && typeof ro.disconnect === 'function') {
        try { ro.disconnect(); } catch { /* ignore */ }
    }
    // não assuma que editor sempre existe depois
    if (editor) {
        try { editor.__spellOverlayRO = undefined; } catch { /* ignore */ }
    }

    // 2) remover listener de scroll, se instalado
    const onScroll = editor.__spellOverlayScroll;
    if (onScroll && typeof editor.removeEventListener === 'function') {
        try { editor.removeEventListener('scroll', onScroll); } catch { /* ignore */ }
    }
    if (editor) {
        try { editor.__spellOverlayScroll = undefined; } catch { /* ignore */ }
    }

    // 3) limpar canvas (opcional, ajuda a evitar “fantasmas” visuais)
    if (canvas && canvas.getContext) {
        const ctx = canvas.getContext('2d');
        if (ctx) {
            try { ctx.clearRect(0, 0, canvas.width, canvas.height); } catch { /* ignore */ }
        }
    }
}

