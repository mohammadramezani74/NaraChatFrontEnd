// مسیر: NaraChatFrontEnd/wwwroot/js/videoplayer.js
// کل فایل را با این جایگزین کن.
//
// تفاوت با نسخه‌ی قبل: به‌جای response.blob() که کل فایل را یکجا می‌گیرد و
// هیچ خبری نمی‌دهد، بدنه‌ی پاسخ تکه‌تکه خوانده می‌شود تا بشود درصد را
// گزارش داد. همان الگوی naraUpload.download در site.js.

const videoControllers = new Map();

window.playVideoFromApi = async (videoElementId, apiUrl, token, dotNetRef) => {

    const controller = new AbortController();
    videoControllers.set(videoElementId, controller);

    function report(percent, loaded, total) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnVideoProgress', percent, loaded, total);
        }
    }

    try {
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: { "Authorization": `Bearer ${token}` },
            signal: controller.signal
        });

        if (!response.ok) {
            report(-2, 0, 0);   // ۲- یعنی خطا
            return;
        }

        const total = parseInt(response.headers.get('content-length') || '0', 10);

        let blob;

        if (!response.body || !response.body.getReader) {
            // مرورگر استریم نمی‌دهد — بدون درصد ادامه بده
            blob = await response.blob();
        } else {
            const reader = response.body.getReader();
            const chunks = [];
            let loaded = 0;
            let lastPercent = -2;
            let lastReportAt = 0;

            while (true) {
                const { done, value } = await reader.read();
                if (done) break;

                chunks.push(value);
                loaded += value.length;

                // throttle زمانی — گزارش به ازای هر تکه، رشته‌ی interop را
                // اشباع می‌کند و در Blazor WASM که تک‌نخی است رندر عقب می‌افتد
                const now = Date.now();
                if (now - lastReportAt >= 120) {
                    lastReportAt = now;

                    if (total > 0) {
                        const percent = Math.min(100, Math.floor((loaded / total) * 100));
                        if (percent !== lastPercent) {
                            lastPercent = percent;
                            report(percent, loaded, total);
                        }
                    } else {
                        report(-1, loaded, 0);   // ۱- یعنی طول کل نامعلوم
                    }

                    // یک تیک به حلقه‌ی رویداد مهلت بده تا مرورگر رندر کند
                    await new Promise(r => setTimeout(r, 0));
                }
            }

            blob = new Blob(chunks, { type: 'video/mp4' });
        }

        const videoUrl = URL.createObjectURL(blob);
        const videoElement = document.getElementById(`video-${videoElementId}`);

        if (videoElement) {
            videoElement.src = videoUrl;
            videoElement.play();

            // وقتی عنصر جایگزین شد حافظه را آزاد کن
            videoElement.addEventListener('emptied', () => {
                URL.revokeObjectURL(videoUrl);
            }, { once: true });
        }

        report(100, blob.size, blob.size);

    } catch (error) {
        if (error && error.name === 'AbortError') {
            report(-3, 0, 0);   // ۳- یعنی لغو شده
        } else {
            console.error("Error fetching video:", error);
            report(-2, 0, 0);
        }
    } finally {
        videoControllers.delete(videoElementId);
    }
};

window.abortVideoLoad = (videoElementId) => {
    const controller = videoControllers.get(videoElementId);
    if (controller) {
        controller.abort();
        videoControllers.delete(videoElementId);
    }
};