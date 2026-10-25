// naraMedia.js — پخش ویس و ویدیو، ضبط ویس، و نقشه‌ی پیام‌های موقعیت.
//
// تقسیم کار با Blazor: Blazor ساختار و وضعیت‌های کم‌تکرار را می‌کشد (بارگذاری،
// پخش/توقف، سرعت). هر چیزی که چند بار در ثانیه عوض می‌شود — نوار پیشرفت، زمان،
// سطح صدای میکروفون — مستقیم همین‌جا روی DOM نوشته می‌شود تا هر فریم یک
// رندر Blazor نسازد. برای همین JS فقط روی عناصری می‌نویسد که Blazor در
// مارک‌آپشان محتوا یا style نمی‌گذارد (span خالی، data-* و متغیر CSS).
(function () {
    'use strict';

    // ------------------------------------------------------------------ کمکی‌ها

    const FA = '۰۱۲۳۴۵۶۷۸۹';
    const fa = s => String(s).replace(/\d/g, d => FA[d]);

    function fmtTime(sec) {
        if (!isFinite(sec) || sec < 0) sec = 0;
        sec = Math.floor(sec);
        const h = Math.floor(sec / 3600), m = Math.floor((sec % 3600) / 60), s = sec % 60;
        const t = h > 0
            ? h + ':' + String(m).padStart(2, '0') + ':' + String(s).padStart(2, '0')
            : m + ':' + String(s).padStart(2, '0');
        return fa(t);
    }

    function call(ref, method, ...args) {
        if (!ref) return;
        try { ref.invokeMethodAsync(method, ...args).catch(() => { }); } catch { /* کامپوننت از بین رفته */ }
    }

    // در هر لحظه فقط یک ویس یا ویدیو پخش شود؛ با شروع یکی، قبلی می‌ایستد.
    let playing = null;
    function claim(media) {
        if (playing && playing !== media && !playing.paused) playing.pause();
        playing = media;
    }

    async function fetchBlob(url, token, onProgress, signal) {
        const res = await fetch(url, {
            headers: token ? { Authorization: 'Bearer ' + token } : {},
            signal
        });
        if (!res.ok) throw new Error('HTTP ' + res.status);

        const total = parseInt(res.headers.get('content-length') || '0', 10);
        if (!res.body || !res.body.getReader) return await res.blob();

        const reader = res.body.getReader();
        const chunks = [];
        let loaded = 0, last = 0;
        for (; ;) {
            const { done, value } = await reader.read();
            if (done) break;
            chunks.push(value);
            loaded += value.length;
            const now = Date.now();
            if (onProgress && now - last > 120) {
                last = now;
                onProgress(total > 0 ? Math.min(99, Math.floor(loaded * 100 / total)) : -1, loaded, total);
            }
        }
        return new Blob(chunks);
    }

    // نوع واقعی فایل از روی چند بایت اول. ویس‌های قدیمی با پسوند wav ذخیره
    // شده‌اند ولی محتوایشان webm است؛ نوع اشتباه بعضی مرورگرها را گیج می‌کند.
    async function sniff(blob) {
        const b = new Uint8Array(await blob.slice(0, 16).arrayBuffer());
        const s = (i, n) => String.fromCharCode(...b.slice(i, i + n));
        if (b[0] === 0x1A && b[1] === 0x45 && b[2] === 0xDF && b[3] === 0xA3) return 'webm';
        if (s(0, 4) === 'OggS') return 'ogg';
        if (s(0, 4) === 'RIFF') return 'wav';
        if (s(4, 4) === 'ftyp') return 'mp4';
        if (s(0, 3) === 'ID3' || (b[0] === 0xFF && (b[1] & 0xE0) === 0xE0)) return 'mp3';
        if (s(0, 4) === 'fLaC') return 'flac';
        return null;
    }

    const AUDIO_MIME = { webm: 'audio/webm', ogg: 'audio/ogg', wav: 'audio/wav', mp4: 'audio/mp4', mp3: 'audio/mpeg', flac: 'audio/flac' };
    const VIDEO_MIME = { webm: 'video/webm', ogg: 'video/ogg', mp4: 'video/mp4' };

    // شکل موج واقعی. OfflineAudioContext لازم نیست «شروع» شود، پس قانون
    // پخش خودکار مرورگر هشداری نمی‌دهد.
    async function decodePeaks(blob, count) {
        if (blob.size > 12 * 1024 * 1024) return null;
        const OAC = window.OfflineAudioContext || window.webkitOfflineAudioContext;
        if (!OAC) return null;
        try {
            const ctx = new OAC(1, 1, 44100);
            const buf = await ctx.decodeAudioData(await blob.arrayBuffer());
            const data = buf.getChannelData(0);
            const step = Math.max(1, Math.floor(data.length / count));
            const peaks = [];
            for (let i = 0; i < count; i++) {
                let max = 0;
                const start = i * step, end = Math.min(start + step, data.length);
                for (let j = start; j < end; j += 8) {
                    const v = Math.abs(data[j]);
                    if (v > max) max = v;
                }
                peaks.push(max);
            }
            const top = Math.max(...peaks) || 1;
            return {
                duration: buf.duration,
                peaks: peaks.map(p => Math.max(0.12, Math.round(p / top * 100) / 100))
            };
        } catch {
            return null;
        }
    }

    // MediaRecorder در کروم فایل webm بدون مدت می‌سازد (duration = Infinity).
    // راه شناخته‌شده: یک پرش بزرگ، مرورگر مدت را حساب می‌کند، برمی‌گردیم اول.
    function mediaDuration(m) {
        return new Promise(resolve => {
            const finish = () => resolve(isFinite(m.duration) ? m.duration : 0);
            const afterMeta = () => {
                if (isFinite(m.duration) && m.duration > 0) return finish();
                const onTime = () => {
                    m.removeEventListener('timeupdate', onTime);
                    m.currentTime = 0;
                    finish();
                };
                m.addEventListener('timeupdate', onTime);
                try { m.currentTime = 1e101; } catch { finish(); }
                setTimeout(finish, 3000);
            };
            if (m.readyState >= 1) afterMeta();
            else {
                m.addEventListener('loadedmetadata', afterMeta, { once: true });
                m.addEventListener('error', () => resolve(0), { once: true });
            }
        });
    }

    const EXT = { webm: '.webm', ogg: '.ogg', wav: '.wav', mp4: '.mp4', mp3: '.mp3', flac: '.flac' };

    function saveBlobUrl(url, name, kind) {
        name = name || 'file';
        if (kind && EXT[kind] && !/\.[a-z0-9]{2,4}$/i.test(name)) name += (kind === 'mp4' && /voice/i.test(name) ? '.m4a' : EXT[kind]);
        const a = document.createElement('a');
        a.href = url;
        a.download = name;
        document.body.appendChild(a);
        a.click();
        a.remove();
    }

    const RATES = [1, 1.5, 2];
    function readRate(key) {
        try { const r = parseFloat(localStorage.getItem(key)); return RATES.includes(r) ? r : 1; } catch { return 1; }
    }
    function writeRate(key, r) {
        try { localStorage.setItem(key, String(r)); } catch { }
    }

    // نسبت موقعیت اشاره‌گر روی نوار. نوارها راست‌به‌چپ پر می‌شوند.
    function fractionRtl(el, clientX) {
        const r = el.getBoundingClientRect();
        if (r.width <= 0) return 0;
        return Math.min(1, Math.max(0, (r.right - clientX) / r.width));
    }

    function bindScrub(bar, onFraction) {
        let drag = false;
        bar.addEventListener('pointerdown', e => {
            if (e.button !== 0) return;
            drag = true;
            try { bar.setPointerCapture(e.pointerId); } catch { }
            onFraction(fractionRtl(bar, e.clientX), false);
            e.preventDefault();
            e.stopPropagation();
        });
        bar.addEventListener('pointermove', e => { if (drag) onFraction(fractionRtl(bar, e.clientX), false); });
        const end = e => { if (drag) { drag = false; onFraction(fractionRtl(bar, e.clientX), true); } };
        bar.addEventListener('pointerup', end);
        bar.addEventListener('pointercancel', () => { drag = false; });
        bar.addEventListener('click', e => e.stopPropagation());
    }

    // ------------------------------------------------------------------ ویس

    const voices = new WeakMap();

    function paintVoice(st) {
        const a = st.audio;
        const d = st.duration || (a && isFinite(a.duration) ? a.duration : 0);
        const cur = a ? a.currentTime : 0;
        const p = d > 0 ? Math.min(1, cur / d) : 0;
        if (st.wave) st.wave.style.setProperty('--p', (p * 100).toFixed(2) + '%');
        if (st.time) st.time.textContent = (a && (cur > 0 || !a.paused)) ? fmtTime(cur) + ' / ' + fmtTime(d) : fmtTime(d);
    }

    function voiceLoop(st) {
        cancelAnimationFrame(st.raf);
        const tick = () => {
            paintVoice(st);
            if (st.audio && !st.audio.paused) st.raf = requestAnimationFrame(tick);
        };
        st.raf = requestAnimationFrame(tick);
    }

    window.naraVoice = {
        init(el, ref) {
            if (!el) return 1;
            if (voices.has(el)) return voices.get(el).rate;
            const st = {
                ref, el,
                wave: el.querySelector('.nv-wave'),
                time: el.querySelector('.nv-time'),
                audio: null, url: null, duration: 0, ctrl: null, loading: null, raf: 0,
                rate: readRate('nara.voiceRate')
            };
            voices.set(el, st);
            if (st.wave) {
                bindScrub(st.wave, f => {
                    if (!st.audio || !(st.duration > 0)) return;
                    st.audio.currentTime = f * st.duration;
                    paintVoice(st);
                });
            }
            return st.rate;
        },

        load(el, url, token, bars, autoplay) {
            const st = voices.get(el);
            if (!st) return Promise.resolve({ error: 'gone' });
            if (st.audio) {
                if (autoplay) this.toggle(el);
                return Promise.resolve({ duration: st.duration, durationText: fmtTime(st.duration) });
            }
            if (st.loading) return st.loading;

            st.ctrl = new AbortController();
            st.loading = (async () => {
                try {
                    const raw = await fetchBlob(url, token, p => call(st.ref, 'OnVoiceProgress', p), st.ctrl.signal);
                    const kind = await sniff(raw);
                    st.kind = kind;
                    const blob = kind && AUDIO_MIME[kind] ? new Blob([raw], { type: AUDIO_MIME[kind] }) : raw;

                    const info = await decodePeaks(blob, bars);

                    st.url = URL.createObjectURL(blob);
                    const a = new Audio();
                    a.preload = 'auto';
                    a.src = st.url;
                    a.playbackRate = st.rate;
                    st.audio = a;

                    a.addEventListener('play', () => { claim(a); voiceLoop(st); call(st.ref, 'OnVoiceState', true); });
                    a.addEventListener('pause', () => { paintVoice(st); call(st.ref, 'OnVoiceState', false); });
                    a.addEventListener('ended', () => { a.currentTime = 0; paintVoice(st); call(st.ref, 'OnVoiceState', false); });
                    a.addEventListener('seeked', () => paintVoice(st));

                    st.duration = info && info.duration > 0 ? info.duration : await mediaDuration(a);
                    paintVoice(st);

                    if (autoplay) {
                        claim(a);
                        try { await a.play(); } catch { /* مرورگر اجازه نداد؛ کاربر دوباره می‌زند */ }
                    }
                    return { duration: st.duration, durationText: fmtTime(st.duration), peaks: info ? info.peaks : null };
                } catch (e) {
                    if (e && e.name === 'AbortError') return { error: 'aborted' };
                    console.error('[nara] voice load failed:', e);
                    return { error: 'failed' };
                } finally {
                    st.loading = null;
                    st.ctrl = null;
                }
            })();
            return st.loading;
        },

        abort(el) {
            const st = voices.get(el);
            if (st && st.ctrl) st.ctrl.abort();
        },

        async toggle(el) {
            const st = voices.get(el);
            if (!st || !st.audio) return false;
            if (st.audio.paused) {
                claim(st.audio);
                try { await st.audio.play(); } catch { return false; }
                return true;
            }
            st.audio.pause();
            return false;
        },

        cycleRate(el) {
            const st = voices.get(el);
            if (!st) return 1;
            st.rate = RATES[(RATES.indexOf(st.rate) + 1) % RATES.length];
            if (st.audio) st.audio.playbackRate = st.rate;
            writeRate('nara.voiceRate', st.rate);
            return st.rate;
        },

        download(el, name) {
            const st = voices.get(el);
            if (st && st.url) saveBlobUrl(st.url, name, st.kind);
        },

        dispose(el) {
            const st = voices.get(el);
            if (!st) return;
            if (st.ctrl) st.ctrl.abort();
            cancelAnimationFrame(st.raf);
            if (st.audio) { st.audio.pause(); st.audio.removeAttribute('src'); st.audio.load(); }
            if (playing === st.audio) playing = null;
            if (st.url) URL.revokeObjectURL(st.url);
            st.ref = null;
            voices.delete(el);
        }
    };

    // ------------------------------------------------------------------ ویدیو

    const videos = new WeakMap();

    function paintVideo(st) {
        const v = st.v;
        const d = isFinite(v.duration) ? v.duration : (st.duration || 0);
        const p = d > 0 ? Math.min(1, v.currentTime / d) : 0;
        if (st.seek) st.seek.style.setProperty('--p', (p * 100).toFixed(2) + '%');
        if (st.time) st.time.textContent = fmtTime(v.currentTime) + ' / ' + fmtTime(d);
    }

    function videoLoop(st) {
        cancelAnimationFrame(st.raf);
        const tick = () => {
            paintVideo(st);
            if (!st.v.paused) st.raf = requestAnimationFrame(tick);
        };
        st.raf = requestAnimationFrame(tick);
    }

    function wake(st) {
        st.el.dataset.idle = '0';
        clearTimeout(st.idleTimer);
        st.idleTimer = setTimeout(() => {
            if (!st.v.paused) st.el.dataset.idle = '1';
        }, 2600);
    }

    function isFullscreen(el) {
        return (document.fullscreenElement || document.webkitFullscreenElement) === el;
    }

    function videoAction(st, act) {
        const v = st.v;
        switch (act) {
            case 'toggle':
                if (v.paused) { claim(v); v.play().catch(() => { }); } else v.pause();
                break;
            case 'mute':
                v.muted = !v.muted;
                st.el.dataset.muted = v.muted ? '1' : '0';
                break;
            case 'rate': {
                const r = RATES[(RATES.indexOf(v.playbackRate) + 1) % RATES.length] || 1;
                v.playbackRate = r;
                if (st.rateEl) st.rateEl.textContent = fa(r) + '×';
                break;
            }
            case 'back': v.currentTime = Math.max(0, v.currentTime - 10); break;
            case 'fwd': v.currentTime = Math.min(v.duration || 0, v.currentTime + 10); break;
            case 'full':
                if (isFullscreen(st.el)) {
                    (document.exitFullscreen || document.webkitExitFullscreen).call(document);
                } else if (st.el.requestFullscreen) {
                    st.el.requestFullscreen().catch(() => { });
                } else if (st.el.webkitRequestFullscreen) {
                    st.el.webkitRequestFullscreen();
                } else if (v.webkitEnterFullscreen) {
                    v.webkitEnterFullscreen();   // آیفون: فقط خود ویدیو تمام‌صفحه می‌شود
                }
                break;
            case 'pip':
                if (document.pictureInPictureElement === v) document.exitPictureInPicture().catch(() => { });
                else if (v.requestPictureInPicture) v.requestPictureInPicture().catch(() => { });
                break;
            case 'download':
                if (st.url) saveBlobUrl(st.url, st.name, st.kind);
                break;
        }
        paintVideo(st);
    }

    window.naraVideo = {
        init(el, ref) {
            if (!el || videos.has(el)) return;
            const v = el.querySelector('video');
            const st = {
                el, v, ref,
                seek: el.querySelector('.nvid-seek'),
                time: el.querySelector('.nvid-time'),
                rateEl: el.querySelector('.nvid-rate'),
                url: null, name: 'video', ctrl: null, raf: 0, idleTimer: 0, duration: 0
            };
            videos.set(el, st);

            el.dataset.playing = '0';
            el.dataset.muted = '0';
            el.dataset.idle = '0';
            el.dataset.pip = (document.pictureInPictureEnabled && v.requestPictureInPicture) ? '1' : '0';
            if (st.rateEl) st.rateEl.textContent = fa(1) + '×';

            v.addEventListener('play', () => { claim(v); el.dataset.playing = '1'; videoLoop(st); wake(st); });
            v.addEventListener('pause', () => { el.dataset.playing = '0'; el.dataset.idle = '0'; paintVideo(st); });
            v.addEventListener('ended', () => { el.dataset.playing = '0'; el.dataset.idle = '0'; paintVideo(st); });
            v.addEventListener('loadedmetadata', () => {
                if (v.videoWidth && v.videoHeight) el.style.setProperty('--ar', v.videoWidth + ' / ' + v.videoHeight);
                paintVideo(st);
            });
            v.addEventListener('volumechange', () => { el.dataset.muted = v.muted ? '1' : '0'; });

            // کلیک‌ها فقط بعد از بارگذاری به JS می‌رسند؛ قبل از آن دکمه‌ی پخش
            // روی پوستر مال Blazor است.
            el.addEventListener('click', e => {
                if (!st.url) return;
                const btn = e.target.closest('[data-act]');
                if (btn && el.contains(btn)) { e.stopPropagation(); videoAction(st, btn.dataset.act); wake(st); return; }
                if (e.target.closest('.nvid-bar')) return;
                videoAction(st, 'toggle');
            });
            el.addEventListener('dblclick', e => {
                if (!st.url || e.target.closest('.nvid-bar')) return;
                videoAction(st, 'full');
            });
            el.addEventListener('pointermove', () => { if (st.url) wake(st); });
            el.addEventListener('keydown', e => {
                if (!st.url) return;
                const map = { ' ': 'toggle', k: 'toggle', m: 'mute', f: 'full', ArrowRight: 'fwd', ArrowLeft: 'back' };
                const act = map[e.key];
                if (act) { e.preventDefault(); videoAction(st, act); wake(st); }
            });

            if (st.seek) {
                bindScrub(st.seek, f => {
                    const d = isFinite(v.duration) ? v.duration : st.duration;
                    if (!st.url || !(d > 0)) return;
                    v.currentTime = f * d;
                    paintVideo(st);
                    wake(st);
                });
            }
        },

        async load(el, url, token, name, autoplay) {
            const st = videos.get(el);
            if (!st) return { error: 'gone' };
            if (st.url) { if (autoplay) videoAction(st, 'toggle'); return { ok: true }; }
            if (st.ctrl) return { error: 'busy' };

            st.name = name || 'video';
            st.ctrl = new AbortController();
            try {
                const raw = await fetchBlob(url, token,
                    (p, loaded, total) => call(st.ref, 'OnVideoProgress', p, loaded, total),
                    st.ctrl.signal);
                const kind = await sniff(raw);
                st.kind = kind;
                const blob = kind && VIDEO_MIME[kind] ? new Blob([raw], { type: VIDEO_MIME[kind] }) : raw;
                st.url = URL.createObjectURL(blob);
                st.v.src = st.url;
                st.duration = await mediaDuration(st.v);
                paintVideo(st);
                if (autoplay) { claim(st.v); try { await st.v.play(); } catch { } }
                return { ok: true, duration: st.duration };
            } catch (e) {
                if (e && e.name === 'AbortError') return { error: 'aborted' };
                console.error('[nara] video load failed:', e);
                return { error: 'failed' };
            } finally {
                st.ctrl = null;
            }
        },

        abort(el) {
            const st = videos.get(el);
            if (st && st.ctrl) st.ctrl.abort();
        },

        dispose(el) {
            const st = videos.get(el);
            if (!st) return;
            if (st.ctrl) st.ctrl.abort();
            cancelAnimationFrame(st.raf);
            clearTimeout(st.idleTimer);
            st.v.pause();
            if (playing === st.v) playing = null;
            if (st.url) { st.v.removeAttribute('src'); st.v.load(); URL.revokeObjectURL(st.url); }
            st.ref = null;
            videos.delete(el);
        }
    };

    // ------------------------------------------------------------------ ضبط ویس

    let rec = null;
    const MAX_RECORD_SEC = 15 * 60;

    function pickMime() {
        if (!window.MediaRecorder || !MediaRecorder.isTypeSupported) return '';
        const list = ['audio/webm;codecs=opus', 'audio/ogg;codecs=opus', 'audio/mp4', 'audio/webm'];
        return list.find(t => MediaRecorder.isTypeSupported(t)) || '';
    }

    function blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const r = new FileReader();
            r.onloadend = () => resolve(String(r.result).split(',')[1] || '');
            r.onerror = reject;
            r.readAsDataURL(blob);
        });
    }

    function releaseRecorder(st) {
        clearInterval(st.timer);
        cancelAnimationFrame(st.raf);
        try { st.stream.getTracks().forEach(t => t.stop()); } catch { }
        try { if (st.ac) st.ac.close(); } catch { }
    }

    window.naraRecorder = {
        async start(el, ref) {
            if (rec) return { error: 'busy' };
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia || !window.MediaRecorder)
                return { error: 'unsupported' };

            let stream;
            try {
                stream = await navigator.mediaDevices.getUserMedia({
                    audio: { echoCancellation: true, noiseSuppression: true, autoGainControl: true }
                });
            } catch (e) {
                const n = e && e.name;
                if (n === 'NotAllowedError' || n === 'SecurityError') return { error: 'denied' };
                if (n === 'NotFoundError' || n === 'OverconstrainedError') return { error: 'nomic' };
                if (n === 'NotReadableError') return { error: 'busy-mic' };
                return { error: 'failed' };
            }

            const mime = pickMime();
            let mr;
            try {
                mr = new MediaRecorder(stream, mime ? { mimeType: mime, audioBitsPerSecond: 32000 } : undefined);
            } catch {
                mr = new MediaRecorder(stream);
            }

            const st = { el, ref, stream, mr, chunks: [], started: performance.now(), timer: 0, raf: 0, ac: null };
            mr.ondataavailable = e => { if (e.data && e.data.size) st.chunks.push(e.data); };
            mr.start(250);
            rec = st;

            const timeEl = el && el.querySelector('.nr-time');
            if (timeEl) timeEl.textContent = fmtTime(0);
            st.timer = setInterval(() => {
                const sec = (performance.now() - st.started) / 1000;
                if (timeEl) timeEl.textContent = fmtTime(sec);
                if (sec >= MAX_RECORD_SEC) call(st.ref, 'OnRecorderLimit');
            }, 250);

            // سطح صدا: میله‌ها از راست وارد می‌شوند و به چپ می‌روند
            const bars = el ? Array.from(el.querySelectorAll('.nr-level i')) : [];
            const AC = window.AudioContext || window.webkitAudioContext;
            if (bars.length && AC) {
                try {
                    st.ac = new AC();
                    const an = st.ac.createAnalyser();
                    an.fftSize = 512;
                    st.ac.createMediaStreamSource(stream).connect(an);
                    const buf = new Uint8Array(an.fftSize);
                    const hist = new Array(bars.length).fill(0.08);
                    let lastPush = 0;
                    const tick = t => {
                        an.getByteTimeDomainData(buf);
                        let sum = 0;
                        for (let i = 0; i < buf.length; i++) { const x = (buf[i] - 128) / 128; sum += x * x; }
                        const lvl = Math.min(1, Math.sqrt(sum / buf.length) * 4);
                        if (t - lastPush > 90) {
                            lastPush = t;
                            hist.pop();
                            hist.unshift(Math.max(0.08, lvl));
                            for (let i = 0; i < bars.length; i++) bars[i].style.height = Math.round(hist[i] * 100) + '%';
                        }
                        st.raf = requestAnimationFrame(tick);
                    };
                    st.raf = requestAnimationFrame(tick);
                } catch { /* بدون نمایش سطح صدا هم ضبط کار می‌کند */ }
            }
            return { ok: true };
        },

        async stop(send) {
            const st = rec;
            if (!st) return null;
            rec = null;

            const duration = (performance.now() - st.started) / 1000;
            if (st.mr.state !== 'inactive') {
                await new Promise(resolve => {
                    st.mr.addEventListener('stop', resolve, { once: true });
                    try { st.mr.requestData(); } catch { }
                    st.mr.stop();
                    setTimeout(resolve, 2000);
                });
            }
            releaseRecorder(st);

            if (!send) return null;
            if (duration < 0.8 || st.chunks.length === 0) return { error: 'short' };

            const type = (st.mr.mimeType || pickMime() || 'audio/webm').split(';')[0];
            const blob = new Blob(st.chunks, { type });
            const ext = type.indexOf('ogg') >= 0 ? '.ogg' : type.indexOf('mp4') >= 0 ? '.m4a' : '.weba';
            return { data: await blobToBase64(blob), ext, mime: type, seconds: Math.round(duration) };
        },

        isRecording() { return !!rec; }
    };

    // ------------------------------------------------------------------ موقعیت

    const maps = new Map();
    const TEHRAN = [35.6997, 51.3380];
    const TILE = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';

    function pin() {
        return L.divIcon({
            className: 'nara-pin',
            html: '<span></span>',
            iconSize: [30, 42],
            iconAnchor: [15, 38]
        });
    }

    function destroyMap(id) {
        const m = maps.get(id);
        if (m) { try { m.map.remove(); } catch { } maps.delete(id); }
    }

    window.naraLocation = {
        locate() {
            return new Promise(resolve => {
                if (!navigator.geolocation) return resolve({ error: 'unsupported' });
                navigator.geolocation.getCurrentPosition(
                    p => resolve({ lat: p.coords.latitude, lng: p.coords.longitude, accuracy: Math.round(p.coords.accuracy || 0) }),
                    e => resolve({ error: e && e.code === 1 ? 'denied' : e && e.code === 3 ? 'timeout' : 'unavailable' }),
                    { enableHighAccuracy: true, timeout: 15000, maximumAge: 30000 });
            });
        },

        // نقشه‌ی کوچک داخل پیام: ثابت، بدون کشیدن و زوم با اسکرول تا اسکرول
        // گفتگو را نگیرد.
        mini(id, lat, lng) {
            const el = document.getElementById(id);
            if (!el || !window.L) return;
            destroyMap(id);
            const map = L.map(el, {
                zoomControl: false, dragging: false, touchZoom: false, doubleClickZoom: false,
                scrollWheelZoom: false, boxZoom: false, keyboard: false, attributionControl: false
            }).setView([lat, lng], 15);
            L.tileLayer(TILE, { maxZoom: 19 }).addTo(map);
            L.marker([lat, lng], { icon: pin(), interactive: false }).addTo(map);
            maps.set(id, { map });
        },

        // نقشه‌ی انتخاب در پنجره‌ی ارسال: با کلیک یا کشیدن سنجاق جابه‌جا می‌شود.
        picker(id, lat, lng, accuracy, ref) {
            const el = document.getElementById(id);
            if (!el || !window.L) return false;
            destroyMap(id);
            const has = typeof lat === 'number' && typeof lng === 'number';
            const center = has ? [lat, lng] : TEHRAN;
            const map = L.map(el, { attributionControl: false }).setView(center, has ? 16 : 11);
            L.tileLayer(TILE, { maxZoom: 19 }).addTo(map);

            const entry = { map, marker: null, circle: null, ref };
            maps.set(id, entry);

            const onDrag = () => {
                const p = entry.marker.getLatLng();
                if (entry.circle) { map.removeLayer(entry.circle); entry.circle = null; }
                call(entry.ref, 'OnPicked', p.lat, p.lng);
            };
            entry.onDrag = onDrag;
            const place = (ll, notify) => {
                const p = L.latLng(ll);
                if (!entry.marker) {
                    entry.marker = L.marker(p, { icon: pin(), draggable: true }).addTo(map);
                    entry.marker.on('dragend', onDrag);
                } else {
                    entry.marker.setLatLng(p);
                }
                if (notify) call(entry.ref, 'OnPicked', p.lat, p.lng);
            };
            entry.place = place;

            if (has) {
                place(center, false);
                if (accuracy > 0 && accuracy < 5000) {
                    entry.circle = L.circle(center, { radius: accuracy, weight: 1, fillOpacity: 0.08 }).addTo(map);
                }
            }
            map.on('click', e => {
                if (entry.circle) { map.removeLayer(entry.circle); entry.circle = null; }
                place(e.latlng, true);
            });
            // پنجره با انیمیشن باز می‌شود؛ اندازه‌ی نهایی را دوباره بخوان
            setTimeout(() => map.invalidateSize(), 250);
            return true;
        },

        moveTo(id, lat, lng, accuracy) {
            const e = maps.get(id);
            if (!e) return;
            e.map.setView([lat, lng], 16);
            e.place([lat, lng], false);
            if (e.circle) { e.map.removeLayer(e.circle); e.circle = null; }
            if (accuracy > 0 && accuracy < 5000)
                e.circle = L.circle([lat, lng], { radius: accuracy, weight: 1, fillOpacity: 0.08 }).addTo(e.map);
        },

        destroy(id) { destroyMap(id); },

        copy(text) {
            try { return navigator.clipboard.writeText(text).then(() => true, () => false); } catch { return false; }
        }
    };
})();
