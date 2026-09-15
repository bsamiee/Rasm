#!/usr/bin/env node

/**
 * Script to create a launcher MCPB bundle for distribution.
 *
 * This creates a small MCPB that contains only the launcher code.
 * The launcher downloads the full implementation from R2 on first run.
 *
 * Unlike the full MCPB, this bundle:
 * - Is much smaller (~50KB vs ~2MB)
 * - Rarely needs updates (implementation updates happen via R2)
 * - Can be distributed via Claude Desktop extension directory
 */

import { execSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import JavaScriptObfuscator from "javascript-obfuscator";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const ROOT = join(__dirname, "..");

// Read package.json for version
const packageJson = JSON.parse(readFileSync(join(ROOT, "package.json"), "utf-8"));
const version = packageJson.version;

// Read launcher manifest
const manifestPath = join(ROOT, "manifest-launcher.json");
if (!existsSync(manifestPath)) {
  console.error("Error: manifest-launcher.json not found");
  process.exit(1);
}
const manifest = JSON.parse(readFileSync(manifestPath, "utf-8"));
manifest.version = version;

const bundleName = `indesign-sidekick-${version}.mcpb`;
const tempDir = join(ROOT, ".launcher-temp");
const outputDir = join(ROOT, "dist", "mcpb");
const outputPath = join(outputDir, bundleName);

console.log(`Creating launcher MCPB bundle: ${bundleName}`);

// Ensure output directory exists
mkdirSync(outputDir, { recursive: true });

// Clean up any previous temp directory
if (existsSync(tempDir)) {
  rmSync(tempDir, { recursive: true });
}

// Create temp directory structure
mkdirSync(join(tempDir, "dist"), { recursive: true });

// Write updated manifest.json
writeFileSync(join(tempDir, "manifest.json"), JSON.stringify(manifest, null, 2));

// Bundle launcher with esbuild
// External: ws (has native bindings)
console.log("Bundling launcher with esbuild...");
const entryPoint = join(ROOT, "dist", "launcher.js");

if (!existsSync(entryPoint)) {
  console.error("Error: dist/launcher.js not found. Run 'npm run build' first.");
  process.exit(1);
}

const bundleOutput = join(tempDir, "dist", "index.js");

// External modules:
// - ws: has native bindings, must be installed alongside the bundle
//
// The bundled launcher dynamically imports impl.js (for dev mode only).
// In production, impl.js is downloaded from R2 and loaded at runtime.
// Since the bundled code doesn't statically import impl.ts, we don't need
// to externalize vips modules.
//
// We use --format=esm so import.meta.url works correctly.
execSync(
  `npx esbuild "${entryPoint}" --bundle --platform=node --format=esm --outfile="${bundleOutput}" --external:ws`,
  {
    cwd: ROOT,
    stdio: "inherit",
  }
);

// Obfuscate the bundled code
console.log("Obfuscating code...");
const bundledCode = readFileSync(bundleOutput, "utf-8");
const obfuscatedResult = JavaScriptObfuscator.obfuscate(bundledCode, {
  compact: true,
  controlFlowFlattening: false,
  deadCodeInjection: false,
  debugProtection: false,
  disableConsoleOutput: false,
  identifierNamesGenerator: "hexadecimal",
  log: false,
  numbersToExpressions: false,
  renameGlobals: false,
  selfDefending: false,
  simplify: true,
  splitStrings: false,
  stringArray: true,
  stringArrayCallsTransform: false,
  stringArrayEncoding: ["base64"],
  stringArrayIndexShift: true,
  stringArrayRotate: true,
  stringArrayShuffle: true,
  stringArrayWrappersCount: 1,
  stringArrayWrappersChainedCalls: false,
  stringArrayWrappersParametersMaxCount: 2,
  stringArrayWrappersType: "variable",
  stringArrayThreshold: 0.75,
  target: "node",
  transformObjectKeys: false,
  unicodeEscapeSequence: false,
});

writeFileSync(bundleOutput, obfuscatedResult.getObfuscatedCode());

// Install only the external dependencies (ws)
console.log("Installing external dependencies (ws)...");
writeFileSync(
  join(tempDir, "package.json"),
  JSON.stringify(
    {
      name: "indesign-sidekick-launcher",
      version: packageJson.version,
      type: "commonjs",
      dependencies: {
        ws: packageJson.dependencies.ws,
      },
    },
    null,
    2
  )
);

execSync("npm install --production --ignore-scripts", {
  cwd: tempDir,
  stdio: "inherit",
});

// Remove the temp package.json
rmSync(join(tempDir, "package.json"));

// Remove any previous bundle
if (existsSync(outputPath)) {
  rmSync(outputPath);
}

// Create the ZIP archive
console.log("Creating archive...");
execSync(`zip -r "${outputPath}" .`, {
  cwd: tempDir,
  stdio: "inherit",
});

// Clean up temp directory
rmSync(tempDir, { recursive: true });

// Show final bundle size
const { statSync } = await import("node:fs");
const stats = statSync(outputPath);
const sizeKB = (stats.size / 1024).toFixed(2);

console.log(`\nLauncher bundle created: ${outputPath} (${sizeKB} KB)`);
console.log("\nTo install in Claude Desktop:");
console.log("  1. Double-click the .mcpb file, or");
console.log("  2. Go to Settings > Extensions > Install Extension");
