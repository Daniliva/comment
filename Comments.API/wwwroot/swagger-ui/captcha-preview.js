window.addEventListener('load', () => {
    const updateCaptcha = () => {
        const img = document.getElementById('swagger-captcha-img');
        const code = document.querySelector('.response-col_description pre code');
        if (img && code) {
            try {
                const json = JSON.parse(code.textContent);
                if (json.data?.imageData && !img.src.includes(json.data.imageData)) {
                    img.src = 'data:image/png;base64,' + json.data.imageData;
                }
            } catch { }
        }
    };

    const observer = new MutationObserver(updateCaptcha);
    observer.observe(document.body, { childList: true, subtree: true });
    setInterval(updateCaptcha, 1000);
});