(function () {
    const queries = {
        mobile: '(max-width: 767px)',
        tablet: '(min-width: 768px) and (max-width: 1023px)',
        desktop: '(min-width: 1024px)'
    };

    let dotNetRef = null;
    const mqls = {};

    function getBreakpoint() {
        if (window.matchMedia(queries.mobile).matches) return 'mobile';
        if (window.matchMedia(queries.tablet).matches) return 'tablet';
        return 'desktop';
    }

    function notify() {
        try {
            const bp = getBreakpoint();
            if (dotNetRef) dotNetRef.invokeMethodAsync('NotifyBreakpoint', bp).catch(() => { });
        } catch (e) {
            console.error('breakpoint notify error', e);
        }
    }

    window.registerBreakpointListener = function (dotNetObject) {
        dotNetRef = dotNetObject;
        // cria listeners
        Object.keys(queries).forEach(key => {
            const q = queries[key];
            const mql = window.matchMedia(q);
            mqls[key] = mql;
            // handler
            const handler = () => notify();
            // addEventListener se disponível, fallback em addListener
            if (typeof mql.addEventListener === 'function') mql.addEventListener('change', handler);
            else if (typeof mql.addListener === 'function') mql.addListener(handler);
        });
        // notifica estado atual
        notify();
    };

    window.unregisterBreakpointListener = function () {
        // remove listeners
        Object.keys(mqls).forEach(key => {
            const mql = mqls[key];
            if (!mql) return;
            try {
                if (typeof mql.removeEventListener === 'function') mql.removeEventListener('change', notify);
                else if (typeof mql.removeListener === 'function') mql.removeListener(notify);
            } catch { }
            delete mqls[key];
        });
        if (dotNetRef) {
            try { dotNetRef.dispose(); } catch { }
            dotNetRef = null;
        }
    };

    window.getCurrentBreakpoint = function () {
        return getBreakpoint();
    };
})();