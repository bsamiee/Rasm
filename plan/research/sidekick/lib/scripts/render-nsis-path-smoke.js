#!/usr/bin/env node

/**
 * Render a minimal NSIS installer containing only the (patched) "Set PATH"
 * section of oclif's win template, for the Windows CI regression test of
 * issue #346. Compiling and running this smoke installer exercises the real
 * PATH-append logic without building the full 70MB installer.
 *
 * Usage: node scripts/render-nsis-path-smoke.js <output.nsi>
 */

import { readFileSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";
import { patchWinTemplate } from "./patch-oclif-win-installer.js";

export function renderSmokeNsi(templateSource) {
  const patched = patchWinTemplate(templateSource);
  const start = patched.indexOf('Section "Set PATH');
  const end = patched.indexOf("SectionEnd", start) + "SectionEnd".length;
  if (start === -1 || end < start) {
    throw new Error("Could not locate the Set PATH section in the patched template");
  }
  const snippet = patched.slice(start, end);
  // The snippet is template-literal source. Render it without eval: fill the
  // one interpolation oclif uses, then resolve backslash escapes (\\ \` \$)
  // left-to-right, matching JS template-literal semantics for those chars.
  const interpolated = snippet.replaceAll("${config.name}", "path-smoke");
  let rendered = "";
  for (let i = 0; i < interpolated.length; i++) {
    if (interpolated[i] === "\\" && i + 1 < interpolated.length) {
      rendered += interpolated[i + 1];
      i++;
    } else {
      rendered += interpolated[i];
    }
  }
  return [
    "!define HWND_BROADCAST 0xffff",
    "!define WM_WININICHANGE 0x001A",
    'Name "sidekick-path-smoke"',
    'OutFile "path-smoke.exe"',
    "RequestExecutionLevel user",
    'InstallDir "C:\\Program Files\\indesign-sidekick"',
    rendered,
    "",
  ].join("\n");
}

function main() {
  const out = process.argv[2];
  if (!out) {
    console.error("Usage: node scripts/render-nsis-path-smoke.js <output.nsi>");
    process.exit(1);
  }
  const require = createRequire(import.meta.url);
  const winJs = require
    .resolve("oclif/package.json")
    .replace(/package\.json$/, "lib/commands/pack/win.js");
  writeFileSync(out, renderSmokeNsi(readFileSync(winJs, "utf8")));
  console.log(`Wrote ${out}`);
}

if (process.argv[1]?.endsWith("render-nsis-path-smoke.js")) {
  main();
}
