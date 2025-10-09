(function () {
    const audioUrl = '/audio/keypress.wav';
    let audioContext = null;
    let audioBuffer = null;
    let initialized = false;

    async function init() {
        if (initialized) return;
        initialized = true;

        try {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
            // busca e decodifica o áudio uma vez
            const resp = await fetch(audioUrl);
            if (!resp.ok) throw new Error('audio fetch failed');
            const arrayBuffer = await resp.arrayBuffer();
            audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
        } catch (err) {
            console.warn('keyboardSound init failed:', err);
            audioContext = null;
            audioBuffer = null;
            return;
        }

        // resume em primeiro gesto (alguns navegadores bloqueiam AudioContext até interação)
        const resumeIfNeeded = () => {
            if (audioContext && audioContext.state === 'suspended') {
                audioContext.resume().catch(() => { /* ignore */ });
            }
            window.removeEventListener('touchstart', resumeIfNeeded);
            window.removeEventListener('mousedown', resumeIfNeeded);
            window.removeEventListener('keydown', resumeIfNeeded);
        };
        window.addEventListener('touchstart', resumeIfNeeded, { once: true });
        window.addEventListener('mousedown', resumeIfNeeded, { once: true });
        window.addEventListener('keydown', resumeIfNeeded, { once: true });

        window.addEventListener('keydown', playOnce);
    }

    function playOnce(ev) {
        try {
            if (!audioContext || !audioBuffer) return;
            const src = audioContext.createBufferSource();
            src.buffer = audioBuffer;
            src.connect(audioContext.destination);
            // opcional: ajustar gain/pan aqui se necessário
            src.start(0);
        } catch (e) {
            // silencioso em erros de reprodução
        }
    }

    // expõe para debug/chamada manual
    window.keyboardSound = {
        init,
        play: playOnce
    };

    // tenta inicializar sem bloquear (falha silente se arquivo não existir)
    // não aguarda; init fará fetch/decodificação async
    document.addEventListener('DOMContentLoaded', () => {
        init().catch(() => { /* ignore */ });
    });
})();