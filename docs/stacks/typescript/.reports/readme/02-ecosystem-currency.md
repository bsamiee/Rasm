# readme — ecosystem currency and catalog audit

## PART A — Ecosystem Currency Sweep (June 2026)

**[EFFECT]:**
- Latest stable: `effect` 3.21.3; catalog pins 3.21.2
- Effect 4 status: **beta** (`4.0.0-beta.78`, June 3 2026); stable not shipped
- v4 headline changes: runtime rewrite (70 kB → ~20 kB bundle); unified versioning (all `@effect/*` packages collapse into `effect` core, platform-specific packages remain); `Schema.makeUnsafe` → `make`; `ExtendableClass` merged into `Class`; `Effect.Yieldable` eliminated; 17 modules under `effect/unstable/*` (AI, HTTP, SQL, RPC, CLI, workflows, clustering); `sql.resolverSingle*` removed
- **Doctrine targets 3.x surface**

**[TYPESCRIPT]:**
- TS 6.0.3 stable (March 2026); last JS-based release; 40-60% faster incremental builds
- TS 7.0 beta (`@typescript/native-preview@beta`, April 2026); Go-based tsgo; 10× faster; breaking: ES5 target dropped, AMD/UMD/SystemJS removed, Classic node resolution removed, `strict: true` default, `noUncheckedSideEffectImports: true` default, `stableTypeOrdering: true` non-disableable
- Doctrine pins TS 6.0.3; TS 7 roadmap note covers upgrade signals

**[EFFECT_REACT_STATE]:**
- `effect-rx` renamed to **`@effect-atom/atom-react`** (tim-smart); latest `0.5.0` (Jan 2026)
- Capabilities: `Atom`, `useAtom`, `useAtomValue`, `useAtomSuspense`, `Result`, `Atom.family`, `Atom.kvs` + `BrowserKeyValueStore`, new `AtomRpc` module for `@effect/rpc` integration
- Pre-1.0, single maintainer, ~9k weekly downloads; real and maintained; admit as the React-Effect component bridge

**[MISSING_LANES]:**
- Background jobs: `@effect/cluster` 0.56.1 + `@effect/workflow` 0.18.2 labeled `unstable`; for battle-tested Redis-backed queues use `bullmq`
- Email: no Effect-native SMTP; `nodemailer` 8.0.7 stays; modern alternative `resend` for HTTP-first delivery
- Redis: no `@effect/sql-redis`; `effect/experimental` has KV persistence layer only; `ioredis` 5.10.1 stays for full client
- WebSockets: **Effect owns this** — `@effect/platform` `Socket.ts` (`makeWebSocket`, `makeWebSocketChannel`, `fromWebSocket`, BrowserSocket)
- S3/object storage: `@effect-aws/client-s3` 1.11.0 (May 2026, actively maintained) — **Effect owns this**
- Payments/webhooks: no Effect-native; use Stripe SDK directly + `@effect/platform` HTTP handlers for webhooks
- Search: no Effect-native; use provider SDK directly (Typesense/Meilisearch/Elasticsearch)

**[PULUMI]:**
- Latest: v3.245.0 (June 2026); no v4 shipped; Automation API stable across TS/Python/Go/C#
- **No first-class Effect integration packages exist** — wire via `Effect.tryPromise` wrapping Automation API calls

**[BIOME]:**
- Latest: `2.4.16` (May 2026); catalog pins `2.4.14`
- `noFloatingPromises`, `noMisusedPromises`, `useAwaitThenable` promoted to **stable** in v2.4
- v2.4 `types` domain isolates type-inference-requiring rules; embedded CSS/GraphQL linting in JS/TS files added

---

## PART B — Catalog Contradiction Audit

### [EFFECT_CONTRADICTORY]

Effect or its ecosystem owns this concern outright.

| Package | Version | Effect Replacement |
| :------ | :------ | :----------------- |
| `nanoid` | 5.1.11 | `Effect.uuid` / `Random.nextUUID` from `@effect/platform` |
| `uuid` | 14.0.0 | `Effect.uuid` / `Random.nextUUID` from `@effect/platform` |
| `idb-keyval` | 6.2.2 | `KeyValueStore` from `@effect/platform-browser` |
| `immer` | 11.1.4 | `Schema.Class` spread-update or `Data.struct` `with` expression |
| `date-fns` | 4.1.0 | `Schema.DateTimeUtc`, `Schema.Duration`, native `Temporal` API (TS 6) |
| `zustand` | 5.0.12 | `Effect.Ref`/`SynchronizedRef` for domain state; `@effect-atom/atom-react` for component subscriptions |
| `zundo` | 2.3.0 | `Effect.Ref` + `Chunk` snapshot for undo history |
| `zustand-computed` | 2.1.2 | `Effect.Ref` derived map / `@effect-atom/atom-react` computed atoms |
| `zustand-slices` | 0.4.0 | `Effect.Service` method decomposition |
| `ts-toolbelt` | 9.6.0 | `Effect/Types`, `Schema.Schema.Type`, TS 6 native conditionals |
| `ts-essentials` | 10.2.0 | `Effect/Types`, `Brand`, TS 6 native utility types |
| `type-fest` | 5.6.0 | `Effect/Types`, `Brand`, `Schema` inference, TS 6 `satisfies`/`infer` |

### [DOMAIN_OWNER]

| Package | Version | Concern |
| :------ | :------ | :------ |
| `papaparse` + `@types/papaparse` | 5.5.3 / 5.5.2 | CSV parsing |
| `exceljs` | 4.4.0 | Excel workbook read/write |
| `sharp` | 0.34.5 | Server-side image processing |
| `jspdf` | 4.2.1 | PDF generation |
| `jszip` | 3.10.1 | Zip archive manipulation |
| `sax` + `@types/sax` | 1.6.0 / 1.2.7 | SAX XML streaming parser |
| `yaml` | 2.8.4 | YAML parse/stringify |
| `rfc6902` | 5.2.0 | JSON Patch (RFC 6902) |
| `isomorphic-dompurify` | 3.12.0 | HTML sanitization |
| `colorjs.io` | 0.6.1 | Color space conversion |
| `arctic` | 3.7.0 | OAuth 2.0 provider clients |
| `otplib` | 13.4.0 | TOTP/HOTP |
| `@simplewebauthn/server` | 13.3.0 | WebAuthn server-side |
| `nodemailer` + `@types/nodemailer` | 8.0.7 / 8.0.0 | SMTP email (no Effect-native SMTP) |
| `ioredis` | 5.10.1 | Full Redis client |
| `@aws-sdk/client-s3` | 3.1041.0 | Raw S3 SDK (boundary under `@effect-aws/client-s3`) |
| `@aws-sdk/client-sesv2` | 3.1041.0 | AWS SES v2 |
| `@aws-sdk/s3-request-presigner` | 3.1041.0 | S3 presigned URLs |
| `@octokit/rest` | 22.0.1 | GitHub REST API |
| `testcontainers` | 11.14.0 | Docker lifecycle for integration tests |
| `fast-check` | 4.7.0 | PBT core (integrated by `@effect/vitest`) |
| `@anthropic-ai/tokenizer` | 0.0.4 | Client-side token counting |
| `react-error-boundary` | 6.1.1 | React render-tree synchronous throw recovery |

### [BUILD_TOOL]

`nx` + all `@nx/*`, `@nx-tools/*`, `@nx-extend/*`, `@nxlv/*`; `@biomejs/biome`, `@berenddeboer/nx-biome`; `vite` + all `vite-plugin-*`, `@tailwindcss/vite`, `@vitejs/plugin-react`; `vitest` + `@vitest/*`, `@effect/vitest`, `@playwright/test`; `typescript`, `tslib`, `tsx`, `@swc/core`, `@swc-node/register`; `babel-plugin-react-compiler`, `react-compiler-runtime`; `lightningcss`, `browserslist`, `@ast-grep/cli`, `@mermaid-js/mermaid-cli`; `rollup-plugin-visualizer`, `@rolldown/plugin-babel`, `@rollup/plugin-typescript`; `@types/k6`, `happy-dom`, `jsdom`.

### [FRONTEND_STACK]

`react`, `react-dom`, `@types/react`, `@types/react-dom`; `react-aria`, `react-aria-components`, `react-stately`, `@react-aria/live-announcer`; `@radix-ui/react-*` (4 entries); `@floating-ui/react`, `@floating-ui/react-dom`; `@tanstack/react-table`, `@tanstack/react-virtual`; `@use-gesture/react`; `tailwindcss`, `tailwind-merge`, `tailwindcss-react-aria-components`, `tw-animate-css`, `class-variance-authority`, `clsx`; `lucide-react`, `cmdk`, `vaul`, `nuqs`.

### [INFRA]

`@pulumi/pulumi`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/command`, `@pulumi/docker`, `@pulumi/kubernetes`, `@pulumi/random`; `@effect-aws/client-s3`; `@dopplerhq/node-sdk`.

### [UNCERTAIN]

| Package | Version | Reason |
| :------ | :------ | :----- |
| `@effect/ai` | 0.35.0 | Effect AI owns inference lane; v4 moves to `effect/unstable/Ai` — version alignment with 3.x catalog pin needed before next Effect bump |
| `@effect/ai-google` | 0.14.0 | Provider package tied to `@effect/ai` — same alignment concern |
| `@effect/ai-openai` | 0.39.2 | Provider package tied to `@effect/ai` — same alignment concern |

---

## Further Considerations

**Effect 4 beta unified versioning has a version-skew trap.** The catalog currently mixes `effect 3.21.2` with versioned `@effect/*` packages (e.g., `@effect/platform 0.96.1`, `@effect/cluster 0.58.2`). When v4 goes stable, all these collapse into one version number — but any partial upgrade (bumping `effect` before `@effect/platform`) will produce dual-runtime peer conflicts that `catalogMode: strict` surfaces immediately. The doctrine's MATERIAL law and the `[ADMISSIONS_RECORD]` in the planning pages should pre-document this as a coordinated-bump constraint, not a per-package update.

**`noFloatingPromises` stable in Biome 2.4 changes the enforcement posture for Effect.** The rule fires on any `Effect<...>` return value that is discarded at a call site — it is not Effect-aware, so `Effect.runFork(program)` is fine but `Effect.runFork` without parens or a mistyped discard silently fires. The language page should state the explicit `void` annotation pattern (`void Effect.runFork(...)`) or service-layer `Layer.launch` as the canonical form that satisfies the lint rule without suppression.

**`@effect-atom/atom-react` pre-1.0 surface instability is a real risk at the architecture boundary.** It has one maintainer and 0.5.x semver, meaning breaking API changes are possible before Effect 4 stabilizes. The doctrine should note that if `@effect-atom/atom-react` breaks between v4 beta and v4 stable, the fallback is `ManagedRuntime.make(layer)` exposed as a React context value consumed by `useContext` + manual `Fiber.join` — this is more verbose but depends only on stable Effect core, and the doctrine should show both spellings so agents don't regress to `ManagedRuntime` when the atom package API shifts.
