/**
 * Local preview harness for the snapshot viewer — a REAL host loop.
 *
 *   npm run preview:snapshot-view   (from packages/mcp-server)
 *
 * Writes the viewer + a parent "host" page under ./tmp and opens it. The parent
 * embeds the viewer in an iframe and drives it with the official AppBridge host
 * (bundled on demand here), completing the same ui/initialize handshake Claude
 * Desktop performs, then pushes a sample tool result. Lets you verify the
 * widget — handshake, render, fit-to-width, zoom — in a browser without
 * InDesign or an MCP host.
 *
 * This is the closest local proxy for Claude Desktop, but not identical; the
 * authoritative check remains a real snapshot in Claude Desktop.
 */

import { execFile } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { build } from "esbuild";
import { SNAPSHOT_VIEW_HTML } from "../src/resources/snapshot-view-html.js";

const here = dirname(fileURLToPath(import.meta.url));
const pkgRoot = join(here, "..");

// A scalable sample "page" so fit-to-width and zoom are visually meaningful.
const sampleSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="850" height="1100" viewBox="0 0 850 1100">
  <rect width="850" height="1100" fill="#ffffff"/>
  <rect x="60" y="60" width="730" height="120" fill="#38a3ee"/>
  <text x="80" y="140" font-family="Georgia, serif" font-size="56" fill="#ffffff">Sample page</text>
  ${Array.from({ length: 14 }, (_, i) => `<rect x="80" y="${230 + i * 52}" width="${690 - (i % 3) * 60}" height="18" rx="4" fill="#d7dde3"/>`).join("\n  ")}
</svg>`;
const sampleBase64 = Buffer.from(sampleSvg, "utf8").toString("base64");

// The CallToolResult the host pushes to the View (mirrors show_snapshot's shape).
const sampleResult = {
  content: [{ type: "image", data: sampleBase64, mimeType: "image/svg+xml" }],
  structuredContent: {
    image: { base64: sampleBase64, mimeType: "image/svg+xml" },
    label: "Page 1 (sample)",
  },
};

// Bundle the AppBridge host, inlining the sample result via esbuild `define`.
const hostBundle = (
  await build({
    entryPoints: [join(pkgRoot, "src/widget/snapshot-view-harness-host.ts")],
    bundle: true,
    format: "iife",
    platform: "browser",
    target: "es2020",
    minify: true,
    legalComments: "none",
    write: false,
    define: { __SAMPLE_RESULT__: JSON.stringify(sampleResult) },
  })
).outputFiles[0].text.replace(/<\/script>/gi, "<\\/script>");

const parentHtml = `<!doctype html>
<html lang="en">
<head><meta charset="utf-8" /><title>Snapshot viewer preview</title>
<style>
  body { margin: 0; font: 13px sans-serif; background: #2a2a2c; color: #ddd; }
  header { padding: 8px 12px; }
  /* Initial height only — the harness host resizes this to the height the
     viewer reports via ui/notifications/size-changed, like a real host. */
  iframe { display: block; width: 100vw; height: 160px; border: 0; background: #1b1b1d; }
</style></head>
<body>
  <header>Preview harness — the frame below is the real ui://sidekick/snapshot-view.html, driven through the AppBridge handshake.</header>
  <iframe id="view" src="./snapshot-view.html"></iframe>
  <script>${hostBundle}</script>
</body>
</html>`;

const tmpDir = join(pkgRoot, "tmp");
mkdirSync(tmpDir, { recursive: true });
writeFileSync(join(tmpDir, "snapshot-view.html"), SNAPSHOT_VIEW_HTML, "utf8");
const parentPath = join(tmpDir, "snapshot-view-preview.html");
writeFileSync(parentPath, parentHtml, "utf8");

console.log(`Wrote preview to ${parentPath}`);
const opener =
  process.platform === "darwin" ? "open" : process.platform === "win32" ? "explorer" : "xdg-open";
execFile(opener, [parentPath], (err) => {
  if (err) console.log(`Open it manually: ${parentPath}`);
});
