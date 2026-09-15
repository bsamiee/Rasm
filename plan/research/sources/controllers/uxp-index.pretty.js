"use strict";
(() => {
    var __require = /* @__PURE__ */ ((x) =>
        typeof require !== "undefined"
            ? require
            : typeof Proxy !== "undefined"
              ? new Proxy(x, {
                    get: (a, b) => (typeof require !== "undefined" ? require : a)[b],
                })
              : x)(function (x) {
        if (typeof require !== "undefined") return require.apply(this, arguments);
        throw Error('Dynamic require of "' + x + '" is not supported');
    });

    // src/connection.ts
    var WS_OPEN = 1;
    var HELP_BASE = "https://sidekick.eastpole.nl/help";
    var DISCONNECT_INFO = {
        "ws-failure": {
            description: "Can't reach your AI Assistant. Make sure it's open and has the Sidekick extension installed.",
            helpUrl: `${HELP_BASE}/ai-assistant-not-running`,
        },
        "hello-ack-timeout": {
            description: "The AI Assistant isn't responding. Try restarting it.",
            helpUrl: `${HELP_BASE}/ai-assistant-not-responding`,
        },
        "heartbeat-miss": {
            description: "The AI Assistant stopped responding. Try restarting it.",
            helpUrl: `${HELP_BASE}/ai-assistant-stopped-responding`,
        },
        "server-closed": {
            description: "The AI Assistant was closed. Reopen it to reconnect.",
            helpUrl: `${HELP_BASE}/ai-assistant-was-closed`,
        },
    };
    var Connection = class _Connection {
        constructor(deps) {
            this.deps = deps;
            this.socket = null;
            this.reconnectTimeout = null;
            this.helloAckTimeout = null;
            this.heartbeatTimeout = null;
            this.handshakeComplete = false;
            this.stopped = false;
            /**
             * Whether the current socket ever fired onopen. In UXP, connection-refused
             * doesn't throw from the constructor — it surfaces as an onclose event, the
             * same event a dropped live connection produces. This flag is what lets
             * onClose tell the two apart (issue #252).
             */
            this.socketOpened = false;
            this.setTimeoutFn = deps.setTimeoutFn ?? setTimeout;
            this.clearTimeoutFn = deps.clearTimeoutFn ?? clearTimeout;
            this.helloAckTimeoutMs = deps.helloAckTimeoutMs ?? 5e3;
            this.heartbeatTimeoutMs = deps.heartbeatTimeoutMs ?? 1e4;
            this.reconnectDelayMs = deps.reconnectDelayMs ?? 2e3;
        }
        start() {
            if (!this.stopped && (this.socket || this.reconnectTimeout)) return;
            this.stopped = false;
            this.deps.onStatus({ text: "Connecting", variant: "connecting" });
            this.connect();
        }
        stop() {
            this.stopped = true;
            this.clearReconnectTimeout();
            this.clearHelloAckTimeout();
            this.clearHeartbeat();
            if (this.socket) {
                _Connection.detachHandlers(this.socket);
                this.socket.close();
                this.socket = null;
            }
        }
        /**
         * Detach all event handlers from a socket we're abandoning. UXP sockets
         * can deliver queued events late; a stale onopen from a discarded socket
         * would mark the *next* socket's socketOpened flag and send a spurious
         * plugin-hello, and a stale onclose would double-fire the reconnect.
         */
        static detachHandlers(socket) {
            socket.onopen = null;
            socket.onclose = null;
            socket.onerror = null;
            socket.onmessage = null;
        }
        /**
         * Send a raw string to the server. Silently no-ops if the socket isn't open.
         */
        send(data) {
            if (this.socket && this.socket.readyState === WS_OPEN) {
                this.socket.send(data);
            }
        }
        connect() {
            if (this.stopped) return;
            if (this.socket && this.socket.readyState === WS_OPEN) return;
            let socket;
            try {
                socket = this.deps.webSocketFactory(this.deps.url);
            } catch (err) {
                console.error("Failed to create WebSocket:", err);
                this.scheduleReconnect("ws-failure");
                return;
            }
            this.socket = socket;
            this.socketOpened = false;
            socket.onopen = () => this.onOpen();
            socket.onclose = () => this.onClose();
            socket.onerror = (event) => this.onError(event);
            socket.onmessage = (event) => this.onMessage(event);
        }
        onOpen() {
            this.socketOpened = true;
            this.handshakeComplete = false;
            this.clearReconnectTimeout();
            this.socket?.send(
                JSON.stringify({
                    type: "plugin-hello",
                    version: this.deps.pluginVersion,
                }),
            );
            this.helloAckTimeout = this.setTimeoutFn(() => {
                if (!this.handshakeComplete) {
                    this.closeAndReconnect("Server did not respond with hello-ack - closing and retrying", "hello-ack-timeout");
                }
            }, this.helloAckTimeoutMs);
        }
        /**
         * Close the current socket and schedule a reconnect. Survives `close()`
         * throwing — important because UXP's WebSocket implementation has known
         * edge cases where calls don't behave per spec. Also suppresses `onclose`
         * after a successful close so it doesn't fire scheduleReconnect a second
         * time pointlessly. Use this from any path that needs to abandon the
         * current socket and try again (helloAck timeout, heartbeat miss, etc.).
         */
        closeAndReconnect(reason, cause) {
            console.error(reason);
            if (this.socket) {
                const sock = this.socket;
                this.socket = null;
                _Connection.detachHandlers(sock);
                try {
                    sock.close();
                } catch (err) {
                    console.error("close() threw:", err);
                }
            }
            this.scheduleReconnect(cause);
        }
        onClose() {
            this.socket = null;
            this.clearHelloAckTimeout();
            this.scheduleReconnect(this.socketOpened ? "server-closed" : "ws-failure");
        }
        onError(event) {
            const errMsg = event.message;
            console.error("WebSocket error - type:", event.type, "message:", errMsg, "readyState:", this.socket?.readyState);
        }
        onMessage(event) {
            let parsed;
            try {
                parsed = JSON.parse(event.data);
            } catch (err) {
                console.error("Failed to parse message:", err);
                return;
            }
            const msg = parsed;
            if (msg.type === "hello-ack") {
                console.log(`Received hello-ack (server version: ${msg.serverVersion})`);
                this.clearHelloAckTimeout();
                this.handshakeComplete = true;
                this.deps.onStatus({ text: "Connected", variant: "connected" });
                this.resetHeartbeat();
                return;
            }
            if (msg.type === "ping") {
                this.resetHeartbeat();
                return;
            }
            if (msg.type === "hello-nack") {
                this.clearHelloAckTimeout();
                const text = msg.message || "Connection rejected";
                const description = msg.description || "";
                console.error(`Server rejected connection: ${text} - ${description}`);
                this.deps.onStatus({
                    text,
                    variant: "error",
                    description,
                    helpUrl: `${HELP_BASE}/sidekick-update-required`,
                });
                this.stopped = true;
                if (this.socket) {
                    _Connection.detachHandlers(this.socket);
                    this.socket.close();
                    this.socket = null;
                }
                return;
            }
            if (msg.jsonrpc === "2.0" && msg.id !== void 0 && typeof msg.method === "string") {
                this.deps.onJsonRpcRequest(msg);
                return;
            }
            console.error("Invalid JSON-RPC request:", msg);
        }
        scheduleReconnect(cause) {
            if (this.stopped) return;
            this.clearReconnectTimeout();
            this.clearHeartbeat();
            const info = DISCONNECT_INFO[cause];
            this.deps.onStatus({
                text: "Connecting",
                variant: "connecting",
                description: info.description,
                helpUrl: info.helpUrl,
            });
            this.reconnectTimeout = this.setTimeoutFn(() => {
                this.reconnectTimeout = null;
                this.connect();
            }, this.reconnectDelayMs);
        }
        resetHeartbeat() {
            this.clearHeartbeat();
            this.heartbeatTimeout = this.setTimeoutFn(() => {
                this.closeAndReconnect("Heartbeat timeout - server not responding", "heartbeat-miss");
            }, this.heartbeatTimeoutMs);
        }
        clearHeartbeat() {
            if (this.heartbeatTimeout) {
                this.clearTimeoutFn(this.heartbeatTimeout);
                this.heartbeatTimeout = null;
            }
        }
        clearHelloAckTimeout() {
            if (this.helloAckTimeout) {
                this.clearTimeoutFn(this.helloAckTimeout);
                this.helloAckTimeout = null;
            }
        }
        clearReconnectTimeout() {
            if (this.reconnectTimeout) {
                this.clearTimeoutFn(this.reconnectTimeout);
                this.reconnectTimeout = null;
            }
        }
    };

    // src/markdown.ts
    function parseMarkdown(text) {
        if (!text) return "";
        let html = text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        html = html.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");
        html = html.replace(/(?<!\*)\*([^*\n]+)\*(?!\*)/g, "<em>$1</em>");
        html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
        const lines = html.split("\n");
        const processedLines = [];
        let inBulletList = false;
        let inNumberedList = false;
        for (const line of lines) {
            const trimmed = line.trim();
            const bulletMatch = trimmed.match(/^[-*]\s+(.+)$/);
            if (bulletMatch) {
                if (!inBulletList) {
                    if (inNumberedList) {
                        processedLines.push("</ol>");
                        inNumberedList = false;
                    }
                    processedLines.push("<ul>");
                    inBulletList = true;
                }
                processedLines.push(`<li>${bulletMatch[1]}</li>`);
                continue;
            }
            const numberedMatch = trimmed.match(/^\d+\.\s+(.+)$/);
            if (numberedMatch) {
                if (!inNumberedList) {
                    if (inBulletList) {
                        processedLines.push("</ul>");
                        inBulletList = false;
                    }
                    processedLines.push("<ol>");
                    inNumberedList = true;
                }
                processedLines.push(`<li>${numberedMatch[1]}</li>`);
                continue;
            }
            if (inBulletList) {
                processedLines.push("</ul>");
                inBulletList = false;
            }
            if (inNumberedList) {
                processedLines.push("</ol>");
                inNumberedList = false;
            }
            if (trimmed) {
                processedLines.push(`<p>${trimmed}</p>`);
            }
        }
        if (inBulletList) {
            processedLines.push("</ul>");
        }
        if (inNumberedList) {
            processedLines.push("</ol>");
        }
        return processedLines.join("");
    }

    // src/snapshot-page-range.ts
    function computePageRange(target, index, pageCount, spreadPageCounts) {
        if (target === "page") {
            if (index < 0 || index >= pageCount) {
                throw new Error(`Page index ${index} out of range (document has ${pageCount} pages)`);
            }
            return `+${index + 1}`;
        }
        if (target === "spread") {
            if (index < 0 || index >= spreadPageCounts.length) {
                throw new Error(`Spread index ${index} out of range (document has ${spreadPageCounts.length} spreads)`);
            }
            let pagesBefore = 0;
            for (let i = 0; i < index; i++) pagesBefore += spreadPageCounts[i];
            const firstAbs = pagesBefore + 1;
            const lastAbs = pagesBefore + spreadPageCounts[index];
            return firstAbs === lastAbs ? `+${firstAbs}` : `+${firstAbs}-+${lastAbs}`;
        }
        throw new Error(`Unknown target type: ${target}`);
    }

    // src/index.ts
    (() => {
        const indesign = __require("indesign");
        const { app, JPEGOptionsQuality, ExportRangeOrAllPages, ExportFormat } = indesign;
        const uxp = __require("uxp");
        const uxpStorage = uxp.storage;
        const fs = uxpStorage.localFileSystem;
        const WEBSOCKET_URL = "ws://localhost:6001";
        const PLUGIN_VERSION = "1.0.22";
        let statusEl = null;
        let statusTextEl = null;
        let connectingDotsEl = null;
        let activityEl = null;
        let activityTextEl = null;
        let descriptionEl = null;
        let helpLinkEl = null;
        let connection = null;
        let connectingDotsInterval = null;
        const CONNECTING_DOT_FRAMES = ["", ".", "..", "..."];
        const CONNECTING_DOT_INTERVAL_MS = 350;
        function startConnectingDots() {
            if (connectingDotsInterval || !connectingDotsEl) return;
            let frame = 0;
            connectingDotsEl.textContent = CONNECTING_DOT_FRAMES[0];
            connectingDotsInterval = setInterval(() => {
                frame = (frame + 1) % CONNECTING_DOT_FRAMES.length;
                if (connectingDotsEl) connectingDotsEl.textContent = CONNECTING_DOT_FRAMES[frame];
            }, CONNECTING_DOT_INTERVAL_MS);
        }
        function stopConnectingDots() {
            if (connectingDotsInterval) {
                clearInterval(connectingDotsInterval);
                connectingDotsInterval = null;
            }
            if (connectingDotsEl) connectingDotsEl.textContent = "";
        }
        let lastAppliedStatus = null;
        function statusesEqual(a, b) {
            return a !== null && a.text === b.text && a.variant === b.variant && a.description === b.description && a.helpUrl === b.helpUrl;
        }
        function applyStatus(status) {
            if (statusesEqual(lastAppliedStatus, status)) return;
            lastAppliedStatus = status;
            if (statusEl) {
                statusEl.className = `container ${status.variant}`;
            }
            if (statusTextEl) {
                statusTextEl.textContent = status.text;
            }
            if (status.variant === "connecting") {
                startConnectingDots();
            } else {
                stopConnectingDots();
            }
            if (status.variant !== "connected") {
                hideActivity();
            }
            if (descriptionEl) {
                if (status.description) {
                    descriptionEl.textContent = status.description;
                    descriptionEl.style.display = "";
                } else {
                    descriptionEl.textContent = "";
                    descriptionEl.style.display = "none";
                }
            }
            if (helpLinkEl) {
                if (status.helpUrl) {
                    helpLinkEl.dataset.url = status.helpUrl;
                    helpLinkEl.style.display = "inline-block";
                } else {
                    helpLinkEl.removeAttribute("data-url");
                    helpLinkEl.style.display = "none";
                }
            }
        }
        function setActivity(description, isRunning) {
            if (!activityEl || !activityTextEl) return;
            if (descriptionEl) {
                descriptionEl.style.display = "none";
            }
            activityEl.classList.add("visible");
            activityTextEl.innerHTML = parseMarkdown(description);
            if (isRunning) {
                activityEl.classList.add("running");
            } else {
                activityEl.classList.remove("running");
            }
        }
        function hideActivity() {
            if (activityEl) {
                activityEl.classList.remove("visible");
            }
        }
        async function executeCode(code) {
            try {
                const AsyncFunction = Object.getPrototypeOf(async () => {}).constructor;
                const fn = new AsyncFunction(code);
                const result = await fn();
                return { success: true, result };
            } catch (err) {
                const error = err instanceof Error ? err.message : String(err);
                const errorType = err instanceof Error ? err.name : void 0;
                const stack = err instanceof Error ? err.stack : void 0;
                return { success: false, error, errorType, stack };
            }
        }
        async function takeSnapshot(target, index) {
            try {
                const doc = app.activeDocument;
                if (!doc) {
                    throw new Error("No active document");
                }
                const spreadPageCounts = [];
                if (target === "spread") {
                    for (let i = 0; i < doc.spreads.length; i++) {
                        spreadPageCounts.push(doc.spreads.item(i).pages.length);
                    }
                }
                const pageRange = computePageRange(target, index, doc.pages.length, spreadPageCounts);
                const tempFolder = await fs.getTemporaryFolder();
                const tempFileName = `indesign_snapshot_${Date.now()}.jpg`;
                const tempFile = await tempFolder.createFile(tempFileName, { overwrite: true });
                app.jpegExportPreferences.jpegQuality = JPEGOptionsQuality.HIGH;
                app.jpegExportPreferences.exportResolution = 72;
                app.jpegExportPreferences.pageString = pageRange;
                app.jpegExportPreferences.jpegExportRange = ExportRangeOrAllPages.EXPORT_RANGE;
                app.jpegExportPreferences.exportingSpread = target === "spread";
                try {
                    doc.exportFile(ExportFormat.JPG, tempFile.nativePath);
                } catch (exportErr) {
                    const message = exportErr instanceof Error ? exportErr.message : String(exportErr);
                    throw new Error(`InDesign JPEG export failed: ${message}`);
                }
                const fileData = await tempFile.read({ format: uxpStorage.formats.binary });
                const bytes = new Uint8Array(fileData);
                let binary = "";
                for (let i = 0; i < bytes.byteLength; i++) {
                    binary += String.fromCharCode(bytes[i]);
                }
                const base64 = btoa(binary);
                await tempFile.delete();
                return { success: true, result: { base64, mimeType: "image/jpeg" } };
            } catch (err) {
                const error = err instanceof Error ? err.message : String(err);
                return { success: false, error };
            }
        }
        function handleJsonRpcRequest(msg) {
            const params = msg.params;
            const description = params?.description;
            if (description) {
                setActivity(description, true);
            }
            if (msg.method === "execute") {
                const code = params?.code;
                if (!code) {
                    connection?.send(
                        JSON.stringify({
                            jsonrpc: "2.0",
                            id: msg.id,
                            error: { code: -32602, message: "Missing 'code' parameter" },
                        }),
                    );
                    return;
                }
                executeCode(code).then((result) => {
                    if (description) {
                        setActivity(description, false);
                    }
                    if (result.success) {
                        connection?.send(
                            JSON.stringify({
                                jsonrpc: "2.0",
                                id: msg.id,
                                result: result.result,
                            }),
                        );
                    } else {
                        connection?.send(
                            JSON.stringify({
                                jsonrpc: "2.0",
                                id: msg.id,
                                error: {
                                    code: -32e3,
                                    message: result.error,
                                    data: {
                                        type: result.errorType,
                                        stack: result.stack?.slice(0, 500),
                                    },
                                },
                            }),
                        );
                    }
                });
            } else if (msg.method === "snapshot") {
                const target = params?.target;
                const index = params?.index;
                if (!target || index === void 0) {
                    connection?.send(
                        JSON.stringify({
                            jsonrpc: "2.0",
                            id: msg.id,
                            error: { code: -32602, message: "Missing 'target' or 'index' parameter" },
                        }),
                    );
                    return;
                }
                takeSnapshot(target, index).then((result) => {
                    if (description) {
                        setActivity(description, false);
                    }
                    if (result.success) {
                        connection?.send(
                            JSON.stringify({
                                jsonrpc: "2.0",
                                id: msg.id,
                                result: result.result,
                            }),
                        );
                    } else {
                        connection?.send(
                            JSON.stringify({
                                jsonrpc: "2.0",
                                id: msg.id,
                                error: { code: -32e3, message: result.error },
                            }),
                        );
                    }
                });
            } else {
                connection?.send(
                    JSON.stringify({
                        jsonrpc: "2.0",
                        id: msg.id,
                        error: { code: -32601, message: `Unknown method: ${msg.method}` },
                    }),
                );
            }
        }
        function cleanup() {
            connection?.stop();
            connection = null;
            stopConnectingDots();
        }
        function init() {
            statusEl = document.getElementById("status");
            statusTextEl = document.querySelector(".status-text");
            connectingDotsEl = document.querySelector(".connecting-dots");
            descriptionEl = document.getElementById("description");
            helpLinkEl = document.getElementById("help-link");
            if (helpLinkEl) {
                const uxpShell = uxp.shell;
                helpLinkEl.addEventListener("click", (event) => {
                    event.preventDefault();
                    const url = helpLinkEl?.dataset.url;
                    if (!url) return;
                    uxpShell.openExternal(url, "Open the Sidekick help page").catch((err) => {
                        console.error("Failed to open external URL:", err);
                    });
                });
            }
            activityEl = document.getElementById("activity");
            activityTextEl = document.getElementById("activity-text");
            const versionEl = document.getElementById("version");
            if (versionEl) {
                versionEl.textContent = `v${PLUGIN_VERSION}`;
            }
            connection = new Connection({
                url: WEBSOCKET_URL,
                pluginVersion: PLUGIN_VERSION,
                webSocketFactory: (url) => new WebSocket(url),
                onStatus: applyStatus,
                onJsonRpcRequest: handleJsonRpcRequest,
            });
            connection.start();
        }
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", init);
        } else {
            init();
        }
        window.addEventListener("unload", cleanup);
    })();
})();
//# sourceMappingURL=index.js.map
