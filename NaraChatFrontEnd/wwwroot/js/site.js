function getScrollContainer(element) {
    const el = resolveElement(element);
    if (!el) return null;
    return el.closest?.("[data-scroll-container]") ?? el;
}

window.scrollToBottom = (element) => {

    const elem = document.getElementById("mainchat");
    if (!elem) return;

    const last = elem.lastElementChild;
    setTimeout(() => {
        if (last) {
            try {
                last.scrollIntoView({ behavior: "smooth", block: "end" });
            } catch {
                elem.scrollTop = elem.scrollHeight;
            }
        } else {
            elem.scrollTop = elem.scrollHeight;
        }
    }, 50);







};
function resolveElement(el) {
    if (!el) return null;

    if (el instanceof Element || el === window || el === document) return el;

    if (el.id) return document.getElementById(el.id);
    return null;
}
function browserNotify(data) {
    if (!("Notification" in window)) {
        alert(data.title);
    }

    var option = {
        body: data.message,
        dir: "rtl",
        icon: data.avatar
    };

    if (Notification.permission == "granted") {
        var notification = new Notification(data.name, option);
        notification.onclick = function (event) {
            event.preventDefault;
            window.location.href = data.url;
            notification.close();
        }
    } else if (Notification.permission != "granted") {
        Notification.requestPermission().then(function (permission) {
            if (permission == "garnted") {
                var notification = new Notification(data.name, option);
                notification.onclick = function (event) {
                    event.preventDefault;
                    window.location.href = data.url;
                    notification.close();
                }
            }
        });
    }
}

window.scrollToElement = (id) => {
    const element = document.getElementById(id);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
};
function triggerDownload2(fileData, fileName) {
    const link = document.createElement("a");
    link.href = fileData;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
function triggerDownload(base64Data, fileName, mimeType = "application/octet-stream") {
    const link = document.createElement("a");
    link.href = `data:${mimeType};base64,${base64Data}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

(function () {
    document.addEventListener('click', function (e) {
        const a = e.target.closest('a[data-mention-link]');
        if (!a) return;
        e.preventDefault();
        const href = a.getAttribute('href');
        if (window.Blazor && typeof window.Blazor.navigateTo === 'function') {
            window.Blazor.navigateTo(href);
        } else {
            window.location.href = href;
        }
    }, true);
})();

window.initScrollListener = (element, dotnetHelper) => {
    const container = document.getElementById("mainchat");
    if (!container) return;


    if (container._scrollHandler) {
        container.removeEventListener("scroll", container._scrollHandler);
        container._scrollHandler = null;
    }

    const handler = () => {
        if (container.scrollTop === 0) {
            if (element === 1)
                dotnetHelper.invokeMethodAsync("LoadMoreMessages");
            else
                dotnetHelper.invokeMethodAsync("LoadMoreChannelMessages");
        }
    };
    container._scrollHandler = handler;
    container.addEventListener("scroll", handler, { passive: true });
};

window.disposeScrollListener = (element) => {
    const container = getScrollContainer(element);
    if (!container || !container._scrollHandler) return;
    container.removeEventListener("scroll", container._scrollHandler);
    container._scrollHandler = null;
};


window.getScrollPosition = (element) => {
    const container = getScrollContainer(element);
    return container ? container.scrollTop : 0;
};

window.setScrollPosition = (element, position) => {
    const container = getScrollContainer(element);
    if (container) {
        container.scrollTop = position;
    }
};

window.getScrollHeight = (element) => {
    const container = getScrollContainer(element);
    return container ? container.scrollHeight : 0;
};

window.getWindowSize = () => {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
    }).catch(function (err) {
        console.error('Could not copy text: ', err);
    });
}
// ============================================================================
// naraUpload — آپلود با نمایش درصد پیشرفت، و دانلود بدون Base64
// این بلوک عمداً export ندارد: site.js یک اسکریپت معمولی است، نه ES module.
// ============================================================================

window.naraUpload = (function () {

    // فایل‌های انتخاب‌شده تا زمان آپلود اینجا نگه داشته می‌شوند، چون بعد از
    // بسته شدن دیالوگ عنصر input از DOM حذف می‌شود.
    const files = new Map();
    const active = new Map();
    const downloads = new Map();

    function newHandle() {
        return (window.crypto && crypto.randomUUID)
            ? crypto.randomUUID()
            : 'h' + Date.now() + '-' + Math.random().toString(36).slice(2);
    }

    return {

        // ------------------------------------------------------------ آپلود

        capture: function (hostElementId) {
            const input = document.querySelector('#' + hostElementId + ' input[type=file]');
            if (!input || !input.files || input.files.length === 0) return null;

            const f = input.files[0];
            const handle = newHandle();
            files.set(handle, f);

            return {
                handle: handle,
                name: f.name,
                size: f.size,
                contentType: f.type || 'application/octet-stream'
            };
        },

        // ثبت یک آبجکت File که از جای دیگری آمده (مثلاً drag & drop)
        register: function (file) {
            const handle = newHandle();
            files.set(handle, file);
            return {
                handle: handle,
                name: file.name,
                size: file.size,
                contentType: file.type || 'application/octet-stream'
            };
        },

        release: function (handle) {
            files.delete(handle);
            active.delete(handle);
        },

        abort: function (handle) {
            const xhr = active.get(handle);
            if (xhr) xhr.abort();
        },

        send: function (handle, url, token, fields, dotNetRef) {
            return new Promise(function (resolve) {
                const file = files.get(handle);
                if (!file) {
                    resolve({ status: 0, body: '', error: 'file-not-found' });
                    return;
                }

                const form = new FormData();
                for (const key in fields) {
                    if (Object.prototype.hasOwnProperty.call(fields, key)) {
                        form.append(key, fields[key]);
                    }
                }
                form.append('file', file, file.name);

                const xhr = new XMLHttpRequest();
                active.set(handle, xhr);

                xhr.open('POST', url, true);
                xhr.setRequestHeader('Authorization', 'Bearer ' + token);
                xhr.timeout = 0; // بدون محدودیت زمانی — برای فایل بزرگ حیاتی است

                let lastPercent = -1;
                xhr.upload.onprogress = function (e) {
                    if (!e.lengthComputable) return;
                    const percent = Math.floor((e.loaded / e.total) * 100);
                    if (percent === lastPercent) return;
                    lastPercent = percent;
                    dotNetRef.invokeMethodAsync('OnUploadProgress', percent, e.loaded, e.total);
                };

                xhr.onload = function () {
                    active.delete(handle);
                    resolve({ status: xhr.status, body: xhr.responseText, error: null });
                };
                xhr.onerror = function () {
                    active.delete(handle);
                    resolve({ status: xhr.status || 0, body: '', error: 'network' });
                };
                xhr.onabort = function () {
                    active.delete(handle);
                    resolve({ status: 0, body: '', error: 'aborted' });
                };

                xhr.send(form);
            });
        },

        // ------------------------------------------------------------ دانلود

        download: function (url, token, fallbackName, dotNetRef, downloadId) {

            function saveBlob(blob, fileName) {
                const objectUrl = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = objectUrl;
                a.download = fileName;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);

                // بلافاصله آزاد نکن، وگرنه دانلود در بعضی مرورگرها لغو می‌شود
                setTimeout(function () { URL.revokeObjectURL(objectUrl); }, 60000);

                return { ok: true, status: 200 };
            }

            const controller = new AbortController();
            if (downloadId) downloads.set(downloadId, controller);

            function cleanup() {
                if (downloadId) downloads.delete(downloadId);
            }

            return fetch(url, {
                headers: { 'Authorization': 'Bearer ' + token },
                signal: controller.signal
            })
                .then(function (response) {
                    if (!response.ok) {
                        cleanup();
                        return { ok: false, status: response.status };
                    }

                    // اسم اصلی فایل سمت سرور ذخیره نمی‌شود، پس نامی که فرانت
                    // می‌دهد معتبرتر است. Content-Disposition فقط وقتی استفاده
                    // می‌شود که فرانت چیزی نداده باشد.
                    let fileName = fallbackName;
                    if (!fileName) {
                        fileName = 'download';
                        const disposition = response.headers.get('content-disposition');
                        if (disposition) {
                            const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
                            const plain = /filename="?([^";]+)"?/i.exec(disposition);
                            if (utf8) fileName = decodeURIComponent(utf8[1]);
                            else if (plain) fileName = plain[1];
                        }
                    }

                    // ممکن است نیامده باشد (فشرده‌سازی یا chunked). آن وقت
                    // درصد نداریم ولی همچنان بایت خوانده‌شده را گزارش می‌کنیم.
                    const total = parseInt(response.headers.get('content-length') || '0', 10);
                    const t0 = Date.now();

                    if (window.naraDebug) {
                        console.log('[nara] پاسخ رسید. dotNetRef=' + (dotNetRef ? 'دارد' : 'ندارد') +
                            '  content-length=' + total +
                            '  streaming=' + (!!(response.body && response.body.getReader)));
                    }

                    if (!response.body || !response.body.getReader) {
                        return response.blob().then(function (blob) {
                            cleanup();
                            return saveBlob(blob, fileName);
                        });
                    }

                    const reader = response.body.getReader();
                    const chunks = [];
                    let loaded = 0;
                    let lastPercent = -2;
                    let lastReportAt = 0;

                    // throttle زمانی: حداکثر هر ۱۲۰ میلی‌ثانیه یک گزارش.
                    // گزارش به ازای هر تکه، رشته‌ی interop را اشباع می‌کند و
                    // در Blazor WASM که تک‌نخی است رندر را عقب می‌اندازد.
                    function shouldReport(now) {
                        if (!dotNetRef) return false;
                        return (now - lastReportAt) >= 120;
                    }

                    function doReport(now, force) {
                        lastReportAt = now;

                        if (window.naraDebug) {
                            console.log('[nara] +' + (now - t0) + 'ms  loaded=' +
                                Math.round(loaded / 1024) + 'KB');
                        }

                        if (total > 0) {
                            const percent = Math.min(100, Math.floor((loaded / total) * 100));
                            if (percent === lastPercent && !force) return;
                            lastPercent = percent;
                            dotNetRef.invokeMethodAsync('OnDownloadProgress', percent, loaded, total);
                        } else {
                            // طول کل نامعلوم: percent = -1 یعنی نامعین
                            dotNetRef.invokeMethodAsync('OnDownloadProgress', -1, loaded, 0);
                        }
                    }

                    function pump() {
                        return reader.read().then(function (res) {
                            if (res.done) {
                                cleanup();
                                if (window.naraDebug) {
                                    console.log('[nara] تمام شد. کل=' + Math.round(loaded / 1024) +
                                        'KB در ' + (Date.now() - t0) + 'ms');
                                }
                                if (dotNetRef) {
                                    dotNetRef.invokeMethodAsync(
                                        'OnDownloadProgress', 100, loaded, loaded);
                                }
                                return saveBlob(new Blob(chunks), fileName);
                            }

                            chunks.push(res.value);
                            loaded += res.value.length;

                            const now = Date.now();
                            if (shouldReport(now)) {
                                doReport(now, false);

                                // یک تیک به حلقه‌ی رویداد مهلت بده تا مرورگر
                                // فرصت رندر پیدا کند. بدون این، زنجیره‌ی microtask
                                // تا آخر فایل بدون هیچ نقاشی مجددی اجرا می‌شود.
                                return new Promise(function (resolve) {
                                    setTimeout(resolve, 0);
                                }).then(pump);
                            }

                            return pump();
                        });
                    }

                    return pump();
                })
                .catch(function (e) {
                    cleanup();
                    if (e && e.name === 'AbortError') {
                        return { ok: false, status: 0, aborted: true };
                    }
                    return { ok: false, status: 0, aborted: false };
                });
        },

        abortDownload: function (downloadId) {
            const controller = downloads.get(downloadId);
            if (controller) {
                controller.abort();
                downloads.delete(downloadId);
            }
        }
    };

})();

// ============================================================================
// naraDropZone — مدیریت مستقیم درگ‌اند‌دراپ
// به رفتار داخلی MudFileUpload وابسته نیست: فایل را از dataTransfer می‌گیرد و
// مستقیم در همان رجیستری naraUpload ثبت می‌کند.
// ============================================================================

(function () {

    // جلوگیری از رفتار پیش‌فرض مرورگر (باز کردن فایل و ترک صفحه)
    // وقتی کاربر فایل را بیرون از کادر رها می‌کند.
    ['dragover', 'drop'].forEach(function (name) {
        window.addEventListener(name, function (e) {
            if (e.dataTransfer && Array.prototype.indexOf.call(e.dataTransfer.types || [], 'Files') !== -1) {
                e.preventDefault();
            }
        }, false);
    });

    let styleInjected = false;
    function injectStyle() {
        if (styleInjected) return;
        styleInjected = true;
        const style = document.createElement('style');
        style.textContent =
            '.nara-drag-over { outline: 3px dashed #1976d2 !important;' +
            ' outline-offset: -6px; background-color: rgba(25,118,210,0.08) !important; }';
        document.head.appendChild(style);
    }

    window.naraDropZone = {

        init: function (zoneId, dotNetRef) {
            injectStyle();

            const zone = document.getElementById(zoneId);
            if (!zone) return false;

            // اگر قبلاً روی همین عنصر ثبت شده بود، اول پاکش کن
            if (zone._naraCleanup) zone._naraCleanup();

            let depth = 0;

            function hasFiles(e) {
                return e.dataTransfer &&
                    Array.prototype.indexOf.call(e.dataTransfer.types || [], 'Files') !== -1;
            }

            function onDragEnter(e) {
                if (!hasFiles(e)) return;
                e.preventDefault();
                e.stopPropagation();
                depth++;
                zone.classList.add('nara-drag-over');
            }

            function onDragOver(e) {
                if (!hasFiles(e)) return;
                e.preventDefault();
                e.stopPropagation();
                e.dataTransfer.dropEffect = 'copy';
            }

            function onDragLeave(e) {
                if (!hasFiles(e)) return;
                e.preventDefault();
                e.stopPropagation();
                depth--;
                if (depth <= 0) {
                    depth = 0;
                    zone.classList.remove('nara-drag-over');
                }
            }

            function onDrop(e) {
                if (!hasFiles(e)) return;
                e.preventDefault();
                e.stopPropagation();
                depth = 0;
                zone.classList.remove('nara-drag-over');

                const dropped = e.dataTransfer.files;
                if (!dropped || dropped.length === 0) return;

                const info = window.naraUpload.register(dropped[0]);
                dotNetRef.invokeMethodAsync('OnFileDropped', info);
            }

            // فاز capture: قبل از اینکه input داخلی MudBlazor رویداد را ببیند
            zone.addEventListener('dragenter', onDragEnter, true);
            zone.addEventListener('dragover', onDragOver, true);
            zone.addEventListener('dragleave', onDragLeave, true);
            zone.addEventListener('drop', onDrop, true);

            zone._naraCleanup = function () {
                zone.removeEventListener('dragenter', onDragEnter, true);
                zone.removeEventListener('dragover', onDragOver, true);
                zone.removeEventListener('dragleave', onDragLeave, true);
                zone.removeEventListener('drop', onDrop, true);
                zone.classList.remove('nara-drag-over');
                zone._naraCleanup = null;
            };

            return true;
        },

        dispose: function (zoneId) {
            const zone = document.getElementById(zoneId);
            if (zone && zone._naraCleanup) zone._naraCleanup();
        }
    };

})();