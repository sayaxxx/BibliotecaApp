// ============================================================================
//  site.js : JavaScript global de BibliotecaApp (HTMX + Alpine.js)
// ----------------------------------------------------------------------------
//  Contenido:
//    1. Configuración global de HTMX (cabecera antiforgery para peticiones
//       que modifican datos, indicador de carga global .fx-busy, error 401/403).
//    2. Puente server->cliente: el servidor emite HX-Trigger {toast: {...}} y
//       aquí se reenvía al store de toasts de Alpine.
//    3. Store de Alpine "toast" (pila de notificaciones con auto-cierre).
//    4. Componente Alpine "countUp" (contador animado para las estadísticas).
//    5. Componente Alpine "quote" (rotador de citas literarias).
//  Dependencias: htmx.min.js y alpine.min.js cargados con defer en _Layout.
// ============================================================================
(function () {
    "use strict";

    /* ---------------------------------------------------------------
       1. CONFIGURACIÓN GLOBAL DE HTMX
       --------------------------------------------------------------- */

    // Cabecera anti-CSRF: todas las peticiones htmx que modifiquen datos
    // (POST/PUT/PATCH/DELETE) llevan el token que Razor ya emitió en _Layout.
    document.body.addEventListener("htmx:configRequest", function (e) {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput && e.detail.verb.toLowerCase() !== "get") {
            e.detail.headers["X-RequestVerificationToken"] = tokenInput.value;
        }
    });

    // Indicador de actividad global: suma de peticiones htmx en vuelo.
    var hxActive = 0;
    document.body.addEventListener("htmx:beforeRequest", function () {
        hxActive++;
        document.body.classList.add("fx-busy");
    });
    document.body.addEventListener("htmx:afterRequest", function () {
        hxActive = Math.max(0, hxActive - 1);
        if (hxActive === 0) {
            document.body.classList.remove("fx-busy");
        }
    });

    // Errores HTTP: 401/403 => devolvemos al login; resto => toast de error.
    document.body.addEventListener("htmx:responseError", function (e) {
        if (e.detail.xhr.status === 401 || e.detail.xhr.status === 403) {
            window.location.href = "/Account/Login";
            return;
        }
        if (window.Alpine) {
            Alpine.store("toast").push("Hubo un error al procesar la solicitud.", "error");
        }
    });

    // El contenido que HTMX intercala puede contener componentes Alpine
    // nuevos: se re-ejecuta el arranque de Alpine sobre el árbol sustituido.
    document.body.addEventListener("htmx:afterSwap", function (e) {
        if (window.Alpine && e.target) {
            e.target.querySelectorAll("[x-data]").forEach(function (el) {
                Alpine.initTree(el);
            });
        }
    });

    /* ---------------------------------------------------------------
       2. PUENTE SERVER -> CLIENTE (toasts vía HX-Trigger)
       El controlador responde con HX-Trigger: {"toast": {message, type}}.
       --------------------------------------------------------------- */
    document.body.addEventListener("toast", function (e) {
        if (!window.Alpine) return;
        var d = e.detail || {};
        Alpine.store("toast").push(d.message || "Operación completada", d.type || "success");
    });

    /* ---------------------------------------------------------------
       3. STORE ALPINE "toast"
       --------------------------------------------------------------- */
    document.addEventListener("alpine:init", function () {
        Alpine.store("toast", {
            items: [],
            counter: 0,
            icons: { success: "✅", error: "⚠️", info: "ℹ️" },
            push: function (message, type) {
                type = type || "success";
                var id = ++this.counter;
                this.items.push({
                    id: id,
                    message: message,
                    type: type,
                    icon: this.icons[type] || this.icons.success,
                    visible: true
                });
                setTimeout(function () { this.remove(id); }.bind(this), 4200);
            },
            remove: function (id) {
                var item = this.items.find(function (i) { return i.id === id; });
                if (item) item.visible = false;
                setTimeout(function () {
                    this.items = this.items.filter(function (i) { return i.id !== id; });
                }.bind(this), 350);
            }
        });
    });

    /* ---------------------------------------------------------------
       4. COMPONENTE ALPINE "countUp" (contador animado)
       x-data="countUp(totales)"
       --------------------------------------------------------------- */
    document.addEventListener("alpine:init", function () {
        Alpine.data("countUp", function (target) {
            return {
                value: 0,
                init: function () {
                    var observer = new IntersectionObserver(
                        function (entries) {
                            entries.forEach(function (en) {
                                if (en.isIntersecting) {
                                    this._animate(target);
                                    observer.disconnect();
                                }
                            }.bind(this));
                        }.bind(this),
                        { threshold: 0.4 }
                    );
                    observer.observe(this.$el);
                },
                _animate: function (target) {
                    var duration = 900;
                    var start = performance.now();
                    var step = function (now) {
                        var p = Math.min(1, (now - start) / duration);
                        this.value = Math.round(target * p);
                        if (p < 1) requestAnimationFrame(step.bind(this));
                    }.bind(this);
                    requestAnimationFrame(step.bind(this));
                }
            };
        });
    });

    /* ---------------------------------------------------------------
       5. COMPONENTE ALPINE "quote" (cita bibliotecaria rotativa)
       x-data="quote('Autor', 'Obra')"
       --------------------------------------------------------------- */
    document.addEventListener("alpine:init", function () {
        Alpine.data("quote", function (text, author) {
            return {
                text: text,
                author: author,
                fade: false,
                ticks: 0,
                rotation: [
                    { text: "La lectura es un viaje sin billete de vuelta.", author: "Desconocido" },
                    { text: "Los libros son espejos: solo se ve en ellos lo que uno lleva dentro.", author: "Carlos Ruiz Zafón" },
                    { text: "Un lector vive mil vidas antes de morir.", author: "George R. R. Martin" },
                    { text: "La biblioteca es una sala donde el silencio tiene acentos.", author: "Anónimo" },
                    { text: "Los libros, que para el vulgo son soledades, son para nosotros patria y refugio.", author: "Jorge Luis Borges" },
                    { text: "La educación es el arma más poderosa para cambiar el mundo.", author: "Nelson Mandela" },
                    { text: "Presta tus libros, porque así se multiplican.", author: "Sabiduría bibliotecaria" }
                ],
                init: function () {
                    this._tick();
                    setInterval(function () { this._tick(); }.bind(this), 6000);
                },
                _tick: function () {
                    var toShow = this.rotation[this.ticks % this.rotation.length];
                    this.fade = true;
                    var self = this;
                    setTimeout(function () {
                        self.text = toShow.text;
                        self.author = toShow.author;
                        self.fade = false;
                    }, 380);
                    this.ticks++;
                }
            };
        });
    });
})();