#!/usr/bin/env node

/**
 * Script to build the implementation bundle for R2 distribution.
 *
 * This creates a single JavaScript file containing all tool implementations
 * that the launcher downloads and dynamically loads.
 *
 * Output: dist/impl/{version}.js
 */

import { execSync } from "node:child_process";
import { copyFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import JavaScriptObfuscator from "javascript-obfuscator";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const ROOT = join(__dirname, "..");

// Read package.json for version
const packageJson = JSON.parse(readFileSync(join(ROOT, "package.json"), "utf-8"));
const version = packageJson.version;

const outputDir = join(ROOT, "dist", "impl");
const bundleOutput = join(outputDir, `${version}.js`);
const tempOutput = join(outputDir, `${version}.temp.js`);

console.log(`Building implementation bundle v${version}`);

// Ensure output directory exists
mkdirSync(outputDir, { recursive: true });

// Generate wasm-vips embedded assets to a separate file (gitignored)
console.log("Generating wasm-vips embedded assets...");
execSync("node scripts/generate-vips-embedded.mjs", {
  cwd: ROOT,
  stdio: "inherit",
});

// Compile TypeScript (uses stub vips-embedded.ts for type checking)
console.log("Compiling TypeScript...");
execSync("npm run build", {
  cwd: ROOT,
  stdio: "inherit",
});

// Also compile the generated file to JS
console.log("Compiling generated vips-embedded...");
execSync(
  "npx tsc src/lib/vips-embedded.generated.ts --outDir dist/lib --module NodeNext --moduleResolution NodeNext --esModuleInterop --declaration",
  {
    cwd: ROOT,
    stdio: "inherit",
  }
);

// Swap the compiled files: replace stub with generated content in dist/
// This way esbuild bundles the full embedded content
const stubCompiledPath = join(ROOT, "dist", "lib", "vips-embedded.js");
const generatedCompiledPath = join(ROOT, "dist", "lib", "vips-embedded.generated.js");
copyFileSync(generatedCompiledPath, stubCompiledPath);
console.log("Swapped vips-embedded.js with generated content in dist/");

// Bundle impl.ts with esbuild
console.log("Bundling with esbuild...");
const entryPoint = join(ROOT, "dist", "impl.js");

if (!existsSync(entryPoint)) {
  console.error("Error: dist/impl.js not found. Run 'npm run build' first.");
  process.exit(1);
}

// Create require shim banner for ESM compatibility
// opentype.js uses dynamic require("fs") which fails in pure ESM context
// because `require` is undefined when the bundle is loaded via dynamic import()
const requireShimBanner = `import { createRequire as __sidekick_createRequire } from 'node:module';
var require = __sidekick_createRequire(import.meta.url);`;

execSync(
  `npx esbuild "${entryPoint}" --bundle --platform=node --format=esm --outfile="${tempOutput}"`,
  {
    cwd: ROOT,
    stdio: "inherit",
  }
);

// Prepend the banner to the output
const bundledCode = readFileSync(tempOutput, "utf-8");
writeFileSync(tempOutput, `${requireShimBanner}\n${bundledCode}`);

// Obfuscate the bundled code (optional, can be disabled for debugging)
const shouldObfuscate = process.argv.includes("--obfuscate");

if (shouldObfuscate) {
  console.log("Obfuscating code...");
  const bundledCode = readFileSync(tempOutput, "utf-8");
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
  // Clean up temp file
  const { rmSync } = await import("node:fs");
  rmSync(tempOutput);
} else {
  // Just rename temp to final
  const { renameSync } = await import("node:fs");
  renameSync(tempOutput, bundleOutput);
}

// Show final bundle size
const { statSync } = await import("node:fs");
const stats = statSync(bundleOutput);
const sizeKB = (stats.size / 1024).toFixed(2);

console.log(`\nImplementation bundle created: ${bundleOutput} (${sizeKB} KB)`);
console.log("\nTo upload to R2:");
console.log(`  aws s3 cp ${bundleOutput} s3://indesign-sidekick-releases/impl/${version}.js`);
