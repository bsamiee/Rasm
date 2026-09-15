#!/usr/bin/env node

/**
 * Smoke test for the MCPB launcher bundle.
 * Verifies the bundle can be extracted and contains expected files.
 *
 * This test:
 * 1. Extracts the MCPB bundle
 * 2. Validates manifest.json
 * 3. Checks required files exist
 * 4. Verifies bundle size is reasonable (not bloated with vips)
 */

import { execSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, rmSync, statSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const packageJson = JSON.parse(readFileSync(join(__dirname, "..", "package.json"), "utf-8"));
const bundlePath = join(
  __dirname,
  "..",
  "dist",
  "mcpb",
  `indesign-sidekick-${packageJson.version}.mcpb`
);

console.log(`Smoke testing MCPB bundle: ${bundlePath}`);

if (!existsSync(bundlePath)) {
  console.error(`\n❌ Bundle not found: ${bundlePath}`);
  console.error("Run 'npm run pack:mcpb' first.");
  process.exit(1);
}

const extractDir = join(tmpdir(), `mcpb-smoke-test-${Date.now()}`);

try {
  // Check bundle size - should be under 500KB (was 4MB+ when vips was included)
  const bundleStats = statSync(bundlePath);
  const bundleSizeKB = bundleStats.size / 1024;
  console.log(`Bundle size: ${bundleSizeKB.toFixed(2)} KB`);

  if (bundleSizeKB > 500) {
    console.error(`\n❌ Bundle too large: ${bundleSizeKB.toFixed(2)} KB > 500 KB`);
    console.error("This suggests vips/wasm modules may have been accidentally included.");
    process.exit(1);
  }
  console.log("✓ Bundle size is reasonable");

  // Extract bundle
  mkdirSync(extractDir, { recursive: true });
  execSync(`unzip -q "${bundlePath}" -d "${extractDir}"`);
  console.log("✓ Bundle extracted successfully");

  // Check required files
  const requiredFiles = ["manifest.json", "dist/index.js", "node_modules/ws/package.json"];

  for (const file of requiredFiles) {
    const filePath = join(extractDir, file);
    if (!existsSync(filePath)) {
      console.error(`\n❌ Missing required file: ${file}`);
      process.exit(1);
    }
  }
  console.log("✓ All required files present");

  // Validate manifest
  const manifest = JSON.parse(readFileSync(join(extractDir, "manifest.json"), "utf-8"));
  if (!manifest.name || !manifest.version || !manifest.server?.entry_point) {
    console.error("\n❌ Invalid manifest.json - missing required fields");
    process.exit(1);
  }
  if (manifest.version !== packageJson.version) {
    console.error(`\n❌ Manifest version mismatch: ${manifest.version} !== ${packageJson.version}`);
    process.exit(1);
  }
  console.log(`✓ Manifest valid: ${manifest.name} v${manifest.version}`);

  // Check index.js size - should be under 1MB uncompressed
  const indexStats = statSync(join(extractDir, "dist/index.js"));
  const indexSizeKB = indexStats.size / 1024;
  console.log(`Index.js size: ${indexSizeKB.toFixed(2)} KB`);

  if (indexSizeKB > 1024) {
    console.error(`\n❌ index.js too large: ${indexSizeKB.toFixed(2)} KB > 1024 KB`);
    console.error("This suggests impl dependencies may have been accidentally bundled.");
    process.exit(1);
  }
  console.log("✓ index.js size is reasonable");

  console.log("\n✅ MCPB smoke test passed");
} catch (error) {
  console.error(`\n❌ Smoke test failed: ${error.message}`);
  process.exit(1);
} finally {
  // Cleanup
  rmSync(extractDir, { recursive: true, force: true });
}
