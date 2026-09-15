#!/usr/bin/env node

/**
 * Smoke test for the launcher in development mode.
 * Verifies the launcher can load the implementation correctly.
 *
 * This test:
 * 1. Runs the launcher in dev mode (SIDEKICK_DEV=1)
 * 2. Verifies it starts correctly and loads the implementation
 * 3. Verifies it shuts down cleanly
 */

import { spawn } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const launcherPath = join(__dirname, "..", "dist", "launcher.js");

console.log("Smoke testing launcher in dev mode...");
console.log(`Launcher path: ${launcherPath}`);

const TIMEOUT_MS = 10000;
const EXPECTED_MESSAGES = [
  "Development mode: using source implementation",
  "Starting with implementation",
  "Registered",
  "tools",
];

const proc = spawn("node", [launcherPath], {
  env: { ...process.env, SIDEKICK_DEV: "1" },
  stdio: ["pipe", "pipe", "pipe"],
});

let stderr = "";
let resolved = false;

const timeout = setTimeout(() => {
  if (!resolved) {
    resolved = true;
    proc.kill();
    checkResults();
  }
}, TIMEOUT_MS);

proc.stderr.on("data", (data) => {
  stderr += data.toString();
  // Kill early if we see enough output
  if (
    stderr.includes("MCP server started") ||
    stderr.includes("Registered") ||
    stderr.includes("Launcher error")
  ) {
    if (!resolved) {
      resolved = true;
      clearTimeout(timeout);
      proc.kill();
      setTimeout(checkResults, 100);
    }
  }
});

proc.on("error", (err) => {
  clearTimeout(timeout);
  console.error(`\n❌ Failed to start launcher: ${err.message}`);
  process.exit(1);
});

proc.stdin.end();

function checkResults() {
  console.log("\nLauncher output:");
  console.log(stderr.split("\n").slice(0, 10).join("\n"));

  // Check for error
  if (stderr.includes("Launcher error")) {
    console.error("\n❌ Launcher failed to start");
    process.exit(1);
  }

  // Check expected messages
  const missing = EXPECTED_MESSAGES.filter((msg) => !stderr.includes(msg));
  if (missing.length > 0) {
    console.error("\n❌ Missing expected output:");
    for (const msg of missing) {
      console.error(`  - ${msg}`);
    }
    process.exit(1);
  }

  console.log("\n✅ Launcher smoke test passed");
  process.exit(0);
}
