#!/usr/bin/env node

/**
 * Smoke test for the bundled implementation.
 * Catches bundling issues like dynamic require failures before release.
 *
 * This test:
 * 1. Loads the bundled implementation via dynamic import
 * 2. Verifies required exports exist
 * 3. Catches runtime errors (e.g., dynamic require('fs') failures)
 */

import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const packageJson = JSON.parse(readFileSync(join(__dirname, "..", "package.json"), "utf-8"));
const bundlePath = join(__dirname, "..", "dist", "impl", `${packageJson.version}.js`);

console.log(`Smoke testing bundle: ${bundlePath}`);

if (!existsSync(bundlePath)) {
  console.error(`\n❌ Bundle not found: ${bundlePath}`);
  console.error("Run 'npm run pack:impl' first.");
  process.exit(1);
}

try {
  // Import the bundle - this will fail if there are dynamic require issues
  const impl = await import(bundlePath);
  console.log("✓ Bundle loaded successfully");

  // Verify the main export exists
  if (typeof impl.getTools !== "function") {
    throw new Error("Missing or invalid getTools export");
  }
  console.log("✓ getTools export present");

  console.log("\n✅ Smoke test passed");
} catch (error) {
  console.error("\n❌ Smoke test failed:");
  console.error(error.message);
  if (error.stack) {
    console.error("\nStack trace:");
    console.error(error.stack);
  }
  process.exit(1);
}
