#!/usr/bin/env node
/**
 * Tiny WebSocket server that accepts connections on port 6001 and ignores
 * everything the plugin sends. Used for manually testing plugin recovery
 * paths that real servers don't normally trigger:
 *
 *   - helloAckTimeout: plugin sends `plugin-hello`, server never responds
 *     → plugin's helloAckTimeout (5s) fires → reconnect cycle starts
 *
 *   - Sustained "Disconnected" + retry loop: kill this script with Ctrl+C,
 *     wait, restart, observe the plugin reconnects.
 *
 * Usage:
 *   node scripts/silent-server.mjs               # listens on port 6001
 *   PORT=7000 node scripts/silent-server.mjs     # custom port
 *
 * Or via npm:
 *   npm run test:silent-server --workspace @indesign-mcp/server
 *
 * Stop with Ctrl+C.
 */
import { WebSocketServer } from "ws";

const port = Number(process.env.PORT ?? 6001);

const wss = new WebSocketServer({ port });

const ts = () => new Date().toISOString();

wss.on("listening", () => {
  console.error(`[${ts()}] silent-server listening on ws://localhost:${port}`);
  console.error(`[${ts()}] accepting connections, ignoring all messages`);
});

wss.on("error", (err) => {
  console.error(`[${ts()}] server error: ${err.message}`);
  if (err.code === "EADDRINUSE") {
    console.error(
      `[${ts()}] port ${port} is already in use — kill the existing server (probably your real Sidekick MCP server) first`
    );
  }
  process.exit(1);
});

let connectionCount = 0;
wss.on("connection", (ws, req) => {
  connectionCount += 1;
  const id = connectionCount;
  console.error(`[${ts()}] #${id} connected from ${req.socket.remoteAddress}`);

  ws.on("message", (data) => {
    const text = data.toString();
    const preview = text.length > 200 ? `${text.slice(0, 200)}…` : text;
    console.error(`[${ts()}] #${id} received (ignoring): ${preview}`);
  });

  ws.on("close", () => {
    console.error(`[${ts()}] #${id} disconnected`);
  });

  ws.on("error", (err) => {
    console.error(`[${ts()}] #${id} error: ${err.message}`);
  });
});

process.on("SIGINT", () => {
  console.error(`\n[${ts()}] shutting down`);
  wss.close(() => process.exit(0));
});
