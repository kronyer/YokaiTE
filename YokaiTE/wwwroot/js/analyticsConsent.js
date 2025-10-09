window.analyticsConsent = {
    getConsent: function () {
        try {
            return localStorage.getItem('analyticsConsent'); // "granted", "denied" ou null
        } catch (e) {
            return null;
        }
    },

    setConsent: function (granted, measurementId) {
        try {
            localStorage.setItem('analyticsConsent', granted ? 'granted' : 'denied');
        } catch (e) { /* ignore */ }

        if (granted && measurementId) {
            // carrega o gtag imediatamente
            window.analyticsConsent.loadGtag(measurementId);
        }
    },

    loadGtag: function (measurementId) {
        if (!measurementId) return;
        if (window.gtagLoaded) return;

        var s = document.createElement('script');
        s.async = true;
        s.src = 'https://www.googletagmanager.com/gtag/js?id=' + measurementId;
        document.head.appendChild(s);

        window.dataLayer = window.dataLayer || [];
        window.gtag = function () { dataLayer.push(arguments); };
        window.gtag('js', new Date());
        // anotar anonymize_ip para privacidade
        window.gtag('config', measurementId, { 'anonymize_ip': true });

        window.gtagLoaded = true;
    }
};