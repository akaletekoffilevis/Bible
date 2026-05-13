window.bibleDb = {
    db: null,
    _supported: true,

    initialize: function (dbName, version) {
        var self = this;
        if (typeof indexedDB === 'undefined') {
            this._supported = false;
            return Promise.resolve();
        }
        return new Promise(function(resolve) {
            try {
                var request = indexedDB.open(dbName, version);
                request.onerror = function() {
                    self._supported = false;
                    resolve();
                };
                request.onsuccess = function() {
                    self.db = request.result;
                    resolve();
                };
                request.onupgradeneeded = function(event) {
                    var db = event.target.result;
                    ['bookmarks', 'notes', 'highlights', 'history', 'progress'].forEach(function(store) {
                        if (!db.objectStoreNames.contains(store)) {
                            db.createObjectStore(store, { keyPath: 'id' });
                        }
                    });
                };
            } catch(e) {
                self._supported = false;
                resolve();
            }
        });
    },

    getAll: function (storeName) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readonly');
            const store = tx.objectStore(storeName);
            const request = store.getAll();
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    },

    getById: function (storeName, id) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readonly');
            const store = tx.objectStore(storeName);
            const request = store.get(id);
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    },

    put: function (storeName, id, value) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readwrite');
            const store = tx.objectStore(storeName);
            value.id = id;
            const request = store.put(value);
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    },

    delete: function (storeName, id) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readwrite');
            const store = tx.objectStore(storeName);
            const request = store.delete(id);
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    },

    clear: function (storeName) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readwrite');
            const store = tx.objectStore(storeName);
            const request = store.clear();
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    },

    count: function (storeName) {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(storeName, 'readonly');
            const store = tx.objectStore(storeName);
            const request = store.count();
            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result);
        });
    }
};

// Keyboard shortcuts for navigation
window.bibleKeyboard = {
    dotNetRef: null,

    init: function (dotNetRef) {
        this.dotNetRef = dotNetRef;
        document.addEventListener('keydown', this.handler);
    },

    handler: function (e) {
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;
        if (window.bibleKeyboard.dotNetRef == null) return;

        if (e.key === 'ArrowLeft') {
            e.preventDefault();
            window.bibleKeyboard.dotNetRef.invokeMethodAsync('OnKeyDown', 'prev');
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            window.bibleKeyboard.dotNetRef.invokeMethodAsync('OnKeyDown', 'next');
        }
    },

    dispose: function () {
        document.removeEventListener('keydown', this.handler);
        this.dotNetRef = null;
    }
};

// Clipboard helper + localStorage helpers
window.bibleUtils = {
    getWindowWidth: function () {
        return window.innerWidth;
    },

    localStorageGet: function (key) {
        return localStorage.getItem(key);
    },

    localStorageSet: function (key, value) {
        localStorage.setItem(key, value);
    },

    localStorageClear: function () {
        localStorage.clear();
    },

    clearIndexedDB: function () {
        return indexedDB.databases().then(function (dbs) {
            dbs.forEach(function (db) { indexedDB.deleteDatabase(db.name); });
        });
    },

    toggleGoogleTranslate: function (show) {
        var el = document.getElementById('google_translate_element');
        if (!el) return;
        if (show) {
            if (!window._googleTranslateLoaded) {
                window._googleTranslateLoaded = true;
                window.googleTranslateElementInit = function () {
                    new google.translate.TranslateElement({
                        pageLanguage: 'fr',
                        includedLanguages: 'en,es,pt,de,it,nl,ru,ar,zh-CN,ja,ko',
                        layout: google.translate.TranslateElement.InlineLayout.SIMPLE,
                        autoDisplay: false
                    }, 'google_translate_element');
                };
                var s = document.createElement('script');
                s.src = 'https://translate.google.com/translate_a/element.js?cb=googleTranslateElementInit';
                document.body.appendChild(s);
            }
            el.style.display = 'block';
        } else {
            el.style.display = 'none';
        }
    },

    setBodyStyles: function (fontSize, fontFamily, lineHeight) {
        document.body.style.setProperty('--bible-font-size', fontSize);
        document.body.style.setProperty('--bible-font-family', fontFamily);
        document.body.style.setProperty('--bible-line-height', lineHeight);
    },

    scrollToElement: function (elementId) {
        setTimeout(function () {
            var el = document.getElementById(elementId);
            if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }, 200);
    },

    copyToClipboard: function (text) {
        if (navigator.clipboard && navigator.clipboard.writeText) {
            return navigator.clipboard.writeText(text).then(function () {
                return true;
            });
        }
        return new Promise(function (resolve) {
            var ta = document.createElement('textarea');
            ta.value = text;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.select();
            document.execCommand('copy');
            document.body.removeChild(ta);
            resolve(true);
        });
    },

    shareOrCopy: function (text) {
        if (navigator.share) {
            navigator.share({ text: text }).catch(function () { });
        } else {
            return this.copyToClipboard(text);
        }
        return Promise.resolve(true);
    }
};

// Text-to-Speech
window.bibleTts = {
    synth: window.speechSynthesis,
    utterance: null,
    currentVerse: -1,
    onVerseChange: null,
    isPaused: false,
    verses: [],
    verseIndex: 0,
    voicesLoaded: false,
    ready: false,
    bestVoice: null,
    primed: false,

    init: function () {
        var self = this;
        if (!this.synth) return false;
        if (this.voicesLoaded) return true;

        try {
            var voices = this.synth.getVoices();
            if (voices.length > 0) {
                self.pickVoice(voices);
                this.voicesLoaded = true;
                this.ready = true;
                return true;
            }
            this.synth.onvoiceschanged = function () {
                var v = self.synth.getVoices();
                self.pickVoice(v);
                self.voicesLoaded = true;
                self.ready = true;
            };
            setTimeout(function () {
                self.ready = true;
                if (!self.bestVoice && self.synth) {
                    var v = self.synth.getVoices();
                    self.pickVoice(v);
                }
            }, 3000);
            return true;
        } catch (e) {
            this.ready = true;
            return true;
        }
    },

    prime: function () {
        if (this.primed || !this.synth) return;
        this.primed = true;
        try {
            var u = new SpeechSynthesisUtterance(' ');
            u.volume = 0;
            u.rate = 1;
            u.lang = 'fr-FR';
            this.synth.speak(u);
            this.synth.cancel();
        } catch (e) {}
    },

    pickVoice: function (voices) {
        if (!voices || voices.length === 0) return;

        var self = this;
        var frenchVoices = [];
        for (var i = 0; i < voices.length; i++) {
            if (voices[i].lang && voices[i].lang.startsWith('fr')) {
                frenchVoices.push(voices[i]);
            }
        }

        var priority = ['Google français', 'Microsoft Hortense', 'Microsoft Julie',
            'Amélie', 'Monica', 'Samantha', 'Thomas', 'Pierre'];

        for (var p = 0; p < priority.length; p++) {
            for (var f = 0; f < frenchVoices.length; f++) {
                if (frenchVoices[f].name.indexOf(priority[p]) !== -1) {
                    self.bestVoice = frenchVoices[f];
                    return;
                }
            }
        }

        if (frenchVoices.length > 0) {
            var defaultVoice = null;
            for (var f = 0; f < frenchVoices.length; f++) {
                if (frenchVoices[f].default) {
                    defaultVoice = frenchVoices[f];
                    break;
                }
            }
            self.bestVoice = defaultVoice || frenchVoices[0];
        }
    },

    speak: function (verses, lang, dotNetRef) {
        var self = this;
        this.stop();
        this.onVerseChange = dotNetRef;
        this.verses = verses;
        this.verseIndex = 0;
        this.isPaused = false;
        this.currentVerse = 0;

        this.init();
        this.prime();

        if (this.onVerseChange) {
            this.onVerseChange.invokeMethodAsync('OnVerseChanged', 1);
        }

        setTimeout(function () {
            self.speakNext();
        }, 300);
    },

    speakNext: function () {
        if (this.isPaused) return;
        if (this.verseIndex >= this.verses.length) {
            this.currentVerse = -1;
            return;
        }

        var text = this.verses[this.verseIndex];
        this.currentVerse = this.verseIndex + 1;

        this.utterance = new SpeechSynthesisUtterance(text);
        this.utterance.lang = 'fr-FR';
        this.utterance.rate = 0.85;
        this.utterance.pitch = 1.0;

        if (this.bestVoice) {
            this.utterance.voice = this.bestVoice;
        }

        var self = this;
        var idx = this.verseIndex;

        this.utterance.onstart = function () {
            if (self.onVerseChange) {
                self.onVerseChange.invokeMethodAsync('OnVerseChanged', idx + 1);
            }
        };

        this.utterance.onend = function () {
            if (!self.isPaused) {
                self.verseIndex = idx + 1;
                self.speakNext();
            }
        };

        this.utterance.onerror = function (e) {
            if (!self.isPaused) {
                self.verseIndex = idx + 1;
                setTimeout(function () { self.speakNext(); }, 500);
            }
        };

        try {
            this.synth.speak(this.utterance);
        } catch (e) {
            this.verseIndex = idx + 1;
            setTimeout(function () { self.speakNext(); }, 500);
        }
    },

    pause: function () {
        this.isPaused = true;
        if (this.synth.speaking) {
            this.synth.pause();
        }
    },

    resume: function () {
        this.isPaused = false;
        if (this.synth.paused) {
            this.synth.resume();
        } else {
            this.speakNext();
        }
    },

    stop: function () {
        this.isPaused = false;
        this.currentVerse = -1;
        this.verseIndex = 0;
        this.verses = [];
        if (this.synth) {
            try {
                if (this.synth.speaking) {
                    this.synth.cancel();
                }
            } catch (e) { }
        }
        this.utterance = null;
    },

    isSpeaking: function () {
        return this.synth && this.synth.speaking;
    }
};

// Bible Map
window.bibleMap = {
    map: null,
    markers: null,

    init: function (elementId) {
        if (this.map) this.destroy();

        this.map = L.map(elementId).setView([31.5, 35.5], 8);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 18,
            attribution: '© OpenStreetMap'
        }).addTo(this.map);

        var locations = [
            { name: 'Jérusalem', lat: 31.7683, lng: 35.2137, ref: 'Genèse 14:18' },
            { name: 'Bethléem', lat: 31.7054, lng: 35.2024, ref: 'Michée 5:1' },
            { name: 'Nazareth', lat: 32.6996, lng: 35.3049, ref: 'Luc 1:26' },
            { name: 'Jéricho', lat: 31.8572, lng: 35.4446, ref: 'Josué 6:1' },
            { name: 'Mer Morte', lat: 31.5, lng: 35.5, ref: 'Genèse 19:24' },
            { name: 'Mont Sinaï', lat: 28.5392, lng: 33.9733, ref: 'Exode 19:2' },
            { name: 'Galilée', lat: 32.8, lng: 35.5, ref: 'Matthieu 4:18' },
            { name: 'Capharnaüm', lat: 32.8804, lng: 35.5753, ref: 'Matthieu 4:13' },
            { name: 'Damascus', lat: 33.5138, lng: 36.2765, ref: 'Actes 9:3' },
            { name: 'Antioche', lat: 36.2, lng: 36.15, ref: 'Actes 11:26' },
            { name: 'Babylone', lat: 32.5364, lng: 44.4208, ref: 'Daniel 1:1' },
            { name: 'Egypte (Gizeh)', lat: 29.9792, lng: 31.1342, ref: 'Genèse 12:10' },
        ];

        this.markers = L.layerGroup();

        locations.forEach(function (loc) {
            var marker = L.marker([loc.lat, loc.lng])
                .bindPopup('<strong>' + loc.name + '</strong><br/><em>' + loc.ref + '</em>');
            window.bibleMap.markers.addLayer(marker);
        });

        this.markers.addTo(this.map);
        this.map.fitBounds(this.markers.getBounds().pad(0.1));
    },

    destroy: function () {
        if (this.map) {
            this.map.remove();
            this.map = null;
            this.markers = null;
        }
    }
};

// Fullscreen
window.bibleFullscreen = {
    toggle: function () {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    }
};

// Verset image generation — backgrounds variés + texte toujours lisible
window.bibleImage = {
    _palettes: [
        { bg: ['#1a237e', '#3949ab'], text: '#ffffff', accent: '#ffd54f' },
        { bg: ['#004d40', '#00796b'], text: '#ffffff', accent: '#ffab00' },
        { bg: ['#4a148c', '#7b1fa2'], text: '#ffffff', accent: '#69f0ae' },
        { bg: ['#b71c1c', '#d32f2f'], text: '#ffffff', accent: '#ffeb3b' },
        { bg: ['#e65100', '#ff6d00'], text: '#ffffff', accent: '#b2ff59' },
        { bg: ['#1a237e', '#283593'], text: '#e8eaf6', accent: '#ffd54f' },
        { bg: ['#fff8e1', '#ffecb3'], text: '#3e2723', accent: '#bf360c' },
        { bg: ['#e8f5e9', '#c8e6c9'], text: '#1b5e20', accent: '#00695c' },
        { bg: ['#e3f2fd', '#bbdefb'], text: '#0d47a1', accent: '#e65100' },
        { bg: ['#fce4ec', '#f8bbd0'], text: '#880e4f', accent: '#1a237e' },
        { bg: ['#263238', '#37474f'], text: '#eceff1', accent: '#ffab00' },
        { bg: ['#3e2723', '#4e342e'], text: '#efebe9', accent: '#ffcc02' },
    ],

    _pick: function (ref) {
        var h = 0;
        for (var i = 0; i < ref.length; i++) h = ((h << 5) - h) + ref.charCodeAt(i);
        return this._palettes[Math.abs(h) % this._palettes.length];
    },

    _draw: function (texte, reference, versetUrl) {
        var canvas = document.createElement('canvas');
        canvas.width = 600;
        canvas.height = 440;
        var ctx = canvas.getContext('2d');
        var palette = this._pick(reference);
        var siteUrl = versetUrl || 'bibeli.vercel.app';

        // Background gradient
        var grad = ctx.createLinearGradient(0, 0, 600, 440);
        grad.addColorStop(0, palette.bg[0]);
        grad.addColorStop(1, palette.bg[1]);
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, 600, 440);

        // Inner border decoration
        ctx.fillStyle = palette.text + '15';
        ctx.fillRect(16, 16, 568, 408);

        ctx.textAlign = 'center';

        // Opening quote
        ctx.fillStyle = palette.text + '40';
        ctx.font = 'bold 36px serif';
        ctx.fillText('"', 300, 70);

        // Word wrap text
        ctx.font = '18px serif';
        var words = texte.split(' ');
        var lines = [];
        var line = '';
        var maxWidth = 500;
        for (var i = 0; i < words.length; i++) {
            var test = line + words[i] + ' ';
            if (ctx.measureText(test).width > maxWidth) {
                lines.push(line.trim());
                line = words[i] + ' ';
            } else {
                line = test;
            }
        }
        lines.push(line.trim());

        var lineHeight = 32;
        var totalHeight = lines.length * lineHeight;
        var yPos = (440 - totalHeight - 60) / 2;

        // Draw text with shadow for readability
        ctx.shadowColor = 'rgba(0,0,0,0.3)';
        ctx.shadowBlur = 4;
        ctx.shadowOffsetY = 2;
        lines.forEach(function (l) {
            ctx.fillStyle = palette.text;
            ctx.font = '18px serif';
            ctx.fillText(l, 300, yPos);
            yPos += lineHeight;
        });

        // Reset shadow for reference
        ctx.shadowColor = 'transparent';
        ctx.shadowBlur = 0;
        ctx.shadowOffsetY = 0;

        // Reference line
        ctx.font = 'bold 14px serif';
        ctx.fillStyle = palette.accent;
        ctx.fillText('— ' + reference + ' —', 300, yPos + 16);

        // Bottom decorative line
        ctx.fillStyle = palette.accent + '50';
        ctx.fillRect(200, yPos + 28, 200, 2);

        // Site URL branding
        ctx.shadowColor = 'transparent';
        ctx.shadowBlur = 0;
        ctx.font = '12px sans-serif';
        ctx.fillStyle = palette.text + '80';
        ctx.fillText('📖 ' + siteUrl, 300, 420);

        return canvas;
    },

    generate: function (texte, reference, url) {
        var canvas = this._draw(texte, reference, url);
        var link = document.createElement('a');
        link.download = 'verset-' + reference.replace(/[^a-zA-Z0-9]/g, '-') + '.png';
        link.href = canvas.toDataURL();
        link.click();
    },

    generateBlob: function (texte, reference, url) {
        var canvas = this._draw(texte, reference, url);
        return new Promise(function (resolve) {
            canvas.toBlob(function (blob) { resolve(blob); }, 'image/png');
        });
    },

    share: function (texte, reference, url) {
        var self = this;
        var siteUrl = url || 'https://bibeli.vercel.app';
        var shareText = '"' + texte + '" — ' + reference + ' (LSG)\n\n📖 ' + siteUrl;
        var canvas = this._draw(texte, reference, siteUrl.replace('https://', ''));
        canvas.toBlob(function (blob) {
            var file = new File([blob], 'verset.png', { type: 'image/png' });
            if (navigator.share && navigator.canShare({ files: [file] })) {
                navigator.share({
                    title: reference + ' — LSG',
                    text: shareText,
                    files: [file]
                }).catch(function () {});
            } else {
                var link = document.createElement('a');
                link.download = 'verset-' + reference.replace(/[^a-zA-Z0-9]/g, '-') + '.png';
                link.href = canvas.toDataURL();
                link.click();
                if (navigator.clipboard && navigator.clipboard.writeText) {
                    navigator.clipboard.writeText(shareText);
                }
            }
        }, 'image/png');
    }
};

// Print chapter
window.biblePrint = {
    print: function () {
        window.print();
    }
};

// Drawer helper — close sans casser MudBlazor
window.bibleDrawer = {
    close: function () {
        document.body.style.overflow = '';
    },
    opened: function () {
        document.body.style.overflow = 'hidden';
    }
};

// PWA install prompt
window.bibleInstall = {
    deferredPrompt: null,
    canInstall: false,

    init: function () {
        var self = this;
        window.addEventListener('beforeinstallprompt', function (e) {
            e.preventDefault();
            self.deferredPrompt = e;
            self.canInstall = true;
        });
        window.addEventListener('appinstalled', function () {
            self.canInstall = false;
            self.deferredPrompt = null;
        });
    },

    isInstallable: function () {
        return this.canInstall;
    },

    promptInstall: function () {
        if (!this.deferredPrompt) return false;
        this.deferredPrompt.prompt();
        this.deferredPrompt.userChoice.then(function (result) {
            if (result.outcome === 'accepted') {
                console.log('App installed');
            }
            window.bibleInstall.deferredPrompt = null;
            window.bibleInstall.canInstall = false;
        });
        return true;
    }
};

// Dynamic Leaflet loader (lazy load only on Carte page)
window.bibleLeaflet = {
    loaded: false,
    pending: [],

    load: function () {
        var self = this;
        if (this.loaded) return Promise.resolve();
        if (this.pending.length > 0) return new Promise(function (r) { self.pending.push(r); });

        return new Promise(function (resolve, reject) {
            self.pending.push(resolve);
            var link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
            link.onload = function () {
                var script = document.createElement('script');
                script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
                script.onload = function () {
                    self.loaded = true;
                    self.pending.forEach(function (r) { r(); });
                    self.pending = [];
                };
                script.onerror = reject;
                document.body.appendChild(script);
            };
            link.onerror = reject;
            document.head.appendChild(link);
        });
    }
};

// Auto-init
document.addEventListener('DOMContentLoaded', function () {
    window.bibleInstall.init();
});
