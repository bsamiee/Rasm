#!/usr/bin/env node

/**
 * Generate vips-embedded.ts containing the wasm-vips worker script and WASM binary.
 * These are embedded as string literals so they can be bundled into impl.js.
 *
 * Run this before building impl.js:
 *   node scripts/generate-vips-embedded.mjs
 */

import { readFileSync, writeFileSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, "..");

const require = createRequire(import.meta.url);
const vipsPath = dirname(require.resolve("wasm-vips"));

console.log("Generating vips-embedded.ts...");

// Read the worker script
const workerScript = readFileSync(join(vipsPath, "vips-node.mjs"), "utf-8");
console.log(`  Worker script: ${(workerScript.length / 1024).toFixed(1)} KB`);

// Read and encode WASM binary
const wasmBinary = readFileSync(join(vipsPath, "vips.wasm"));
const wasmBase64 = wasmBinary.toString("base64");
console.log(`  WASM binary: ${(wasmBinary.length / 1024 / 1024).toFixed(2)} MB`);
console.log(`  WASM base64: ${(wasmBase64.length / 1024 / 1024).toFixed(2)} MB`);

// Generate TypeScript file
const output = `/**
 * Auto-generated file containing embedded wasm-vips assets.
 * DO NOT EDIT - regenerate with: node scripts/generate-vips-embedded.mjs
 *
 * Generated: ${new Date().toISOString()}
 * Worker script: ${(workerScript.length / 1024).toFixed(1)} KB
 * WASM binary: ${(wasmBinary.length / 1024 / 1024).toFixed(2)} MB
 */

// Worker script (vips-node.mjs) - written to temp file at runtime
export const VIPS_WORKER_SCRIPT = ${JSON.stringify(workerScript)};

// WASM binary (vips.wasm) - base64 encoded, decoded at runtime
export const VIPS_WASM_BASE64 = "${wasmBase64}";
`;

const outputPath = join(ROOT, "src", "lib", "vips-embedded.generated.ts");
writeFileSync(outputPath, output);

console.log(`\nGenerated: ${outputPath}`);
console.log(`Total size: ${(output.length / 1024 / 1024).toFixed(2)} MB`);
