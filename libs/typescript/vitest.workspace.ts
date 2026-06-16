import react from "@vitejs/plugin-react";
import { defineWorkspace } from "vitest/config";

// --- [COMPOSITION] ----------------------------------------------------------------------

// One shared workspace; one project per domain folder. The neutral + node strata run
// node-mode against happy-dom-free pure folds; the two browser folders (ui + platform)
// share ONE browser-mode project "browser" over the playwright chromium provider, since
// the playwright provider is folder-agnostic and the browser publication is one runtime.
export default defineWorkspace([
  {
    test: {
      name: "interchange",
      environment: "node",
      include: ["interchange/**/*.{unit-pbt,integration,contract,system}.spec.ts"],
    },
  },
  {
    test: {
      name: "projection",
      environment: "node",
      include: ["projection/**/*.{unit-pbt,integration,contract,system}.spec.ts"],
    },
  },
  {
    test: {
      name: "services",
      environment: "node",
      include: ["services/**/*.{unit-pbt,integration,contract,system}.spec.ts"],
    },
  },
  {
    plugins: [react()],
    test: {
      name: "browser",
      include: ["ui/**/*.{unit-pbt,e2e}.spec.{ts,tsx}", "platform/**/*.{unit-pbt,e2e}.spec.{ts,tsx}"],
      browser: {
        enabled: true,
        provider: "playwright",
        headless: true,
        instances: [{ browser: "chromium" }],
      },
    },
  },
]);
