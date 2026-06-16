import effect from "@effect/eslint-plugin";
import tseslint from "typescript-eslint";

// --- [CONSTANTS] ------------------------------------------------------------------------

const _NODE_RUNTIME = [
  "@effect/platform-node",
  "@effect/platform-node/*",
  "@effect/cluster",
  "@effect/cluster/*",
  "@effect/workflow",
  "@effect/workflow/*",
  "@effect/sql",
  "@effect/sql/*",
  "@effect/sql-pg",
  "@effect/sql-pg/*",
  "@effect/rpc",
  "@effect/rpc/*",
  "@effect/ai",
  "@effect/ai-*",
  "ioredis",
  "@pulumi/*",
  "@aws-sdk/*",
  "@effect-aws/*",
  "@dopplerhq/node-sdk",
  "@simplewebauthn/server",
  "exceljs",
  "papaparse",
  "jspdf",
  "jszip",
  "otplib",
  "nodemailer",
  "sharp",
  "testcontainers",
  "@opentelemetry/sdk-trace-node",
] as const;

const _BROWSER_RUNTIME = [
  "@effect/platform-browser",
  "@effect/platform-browser/*",
  "react",
  "react-dom",
  "react-dom/*",
  "react-aria",
  "react-aria-components",
  "react-stately",
  "react-error-boundary",
  "@effect-atom/atom-react",
  "maplibre-gl",
  "@deck.gl/*",
  "arctic",
  "idb-keyval",
  "nuqs",
  "workbox-build",
  "workbox-window",
  "vite-plugin-pwa",
  "@radix-ui/*",
  "@react-aria/*",
  "@tanstack/react-table",
  "@tanstack/react-virtual",
  "@use-gesture/react",
  "class-variance-authority",
  "cmdk",
  "vaul",
  "tailwindcss",
  "isomorphic-dompurify",
  "@opentelemetry/sdk-trace-web",
] as const;

const _NEUTRAL_FOLDERS = ["**/ui/**", "**/platform/**", "**/services/**", "../ui", "../ui/*", "../platform", "../platform/*", "../services", "../services/*"] as const;

const _BROWSER_FOLDERS = ["**/services/**", "../services", "../services/*"] as const;

const _NODE_FOLDERS = ["**/ui/**", "**/platform/**", "../ui", "../ui/*", "../platform", "../platform/*"] as const;

// --- [COMPOSITION] ----------------------------------------------------------------------

export default tseslint.config(
  {
    plugins: { "@effect": effect },
    languageOptions: { parserOptions: { projectService: true } },
  },
  // [1] neutral stratum — interchange/** + projection/** import only neutral folders + effect/@effect/platform
  {
    files: ["interchange/**/*.ts", "interchange/**/*.tsx", "projection/**/*.ts", "projection/**/*.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            { group: [..._NODE_RUNTIME], message: "neutral stratum: node-only dependency forbidden in interchange/projection." },
            { group: [..._BROWSER_RUNTIME], message: "neutral stratum: browser-only dependency forbidden in interchange/projection." },
            { group: [..._NEUTRAL_FOLDERS], message: "neutral stratum: must not import the browser (ui/platform) or node (services) folders." },
          ],
        },
      ],
    },
  },
  // [1b] projection-no-connect — the sole mechanical guard keeping the fold interior transport-free (RULE_ENFORCEMENT row [1])
  {
    files: ["projection/**/*.ts", "projection/**/*.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            { group: ["@connectrpc/*"], message: "projection-no-connect: the transport-free fold interior must never import @connectrpc/*." },
            { group: [..._NODE_RUNTIME], message: "neutral stratum: node-only dependency forbidden in projection." },
            { group: [..._BROWSER_RUNTIME], message: "neutral stratum: browser-only dependency forbidden in projection." },
            { group: [..._NEUTRAL_FOLDERS], message: "neutral stratum: must not import the browser (ui/platform) or node (services) folders." },
          ],
        },
      ],
    },
  },
  // [2] browser stratum (shared) — ui/** + platform/** import only browser + neutral folders; node deps forbidden
  {
    files: ["ui/**/*.ts", "ui/**/*.tsx", "platform/**/*.ts", "platform/**/*.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            { group: [..._NODE_RUNTIME], message: "browser stratum: node-only dependency forbidden in ui/platform." },
            { group: [..._BROWSER_FOLDERS], message: "browser stratum: must not import the node (services) folder." },
          ],
        },
      ],
    },
  },
  // [7] ui-no-platform-import — ui/** is the lower AppUi-analog library; a ui->platform import is the named browser-internal coupling defect (RULE_ENFORCEMENT row [7])
  {
    files: ["ui/**/*.ts", "ui/**/*.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            { group: ["**/platform/**", "../platform", "../platform/*"], message: "ui-no-platform-import: ui is the AppUi-analog library below platform; platform composes ui, never the reverse." },
            { group: [..._NODE_RUNTIME], message: "browser stratum: node-only dependency forbidden in ui." },
            { group: [..._BROWSER_FOLDERS], message: "browser stratum: must not import the node (services) folder." },
          ],
        },
      ],
    },
  },
  // [3] node stratum — services/** import only node + neutral folders; browser deps forbidden
  {
    files: ["services/**/*.ts", "services/**/*.tsx"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            { group: [..._BROWSER_RUNTIME], message: "node stratum: browser-only dependency forbidden in services." },
            { group: [..._NODE_FOLDERS], message: "node stratum: must not import the browser (ui/platform) folders." },
          ],
        },
      ],
    },
  },
);
