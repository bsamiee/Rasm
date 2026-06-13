# readme — currency sweep addendum

## PART A — ECOSYSTEM CURRENCY SWEEP (June 2026)

---

### 1. Effect Version Status

- Latest stable: **effect 3.21.3** (released ~June 5, 2026 per GitHub releases page)
- Effect 4.x status: **beta** — `npm install effect@beta` installs it; latest beta tag is **4.0.0-beta.78** (June 3, 2026)
- Beta announced February 18, 2026; stable v4 not yet shipped as of June 12, 2026
- v4 headline changes vs 3.x:
  - Runtime fully rewritten: lower memory overhead, faster fiber execution, simpler internals
  - Bundle: minimal program 70 kB → ~20 kB
  - **Unified versioning**: all ecosystem packages share one version number; @effect/platform, @effect/rpc, @effect/cluster functionality absorbed into `effect` core; platform-specific/provider-specific packages remain separate
  - **ServiceMap renamed back to Context** (re-exported from `effect/References`)
  - Schema: `makeUnsafe` → `make`; `ExtendableClass` merged into `Class`; `SchemaParser.makeUnsafe` → `SchemaParser.make`; `Schema.Codec.ToAsserts` removed (use `asserts(schema, input)` directly); `Model.Generated` → `Model.GeneratedByDb`
  - `Effect.Yieldable` eliminated
  - New unstable module path: `effect/unstable/*` — breaking changes allowed in minor releases before graduation
  - 17 unstable modules shipping: AI, HTTP, Schema, SQL, RPC, CLI, workflows, clustering
  - `sql.resolverSingle*` removed — replaced by `effect/Cache` + schema APIs

---

### 2. TypeScript Status

| Version | Status | Notes |
|---|---|---|
| TS 6.0 | **Stable** (March 23, 2026) | Last JS-based release; transition milestone; 40-60% faster incremental builds |
| TS 7.0 beta | **Beta** (April 21, 2026) | Go-based (`tsgo`); 10× faster than 6.x; stable expected mid-2026 |

- TS 7.0 beta npm package: **`@typescript/native-preview@beta`** (also installable via `tsgo` CLI)
- Breaking changes in TS 7.0 beta vs 6.x:
  - `strict: true` now default
  - `module` defaults to `esnext`
  - `noUncheckedSideEffectImports: true` default
  - `stableTypeOrdering: true` default, non-disableable
  - `rootDir` defaults to `./`
  - `types` defaults to empty array (no implicit inclusion)
  - **ES5 target dropped**
  - **AMD / UMD / SystemJS module systems removed**
  - **Classic Node module resolution removed**
  - Prior deprecations turned into hard errors

---

### 3. Effect-Native React State

- **`effect-rx` renamed to `effect-atom`**; install path: `@effect-atom/atom-react`
- Latest: `@effect-atom/atom-react` **0.5.0** (Jan 28, 2026); `@effect-atom/atom` **0.5.3** (~March 2026)
- Maintainer: tim-smart (1 maintainer); ~9,025 weekly downloads
- Capabilities confirmed: `Atom`, `useAtom`, `useAtomValue`, `useAtomSuspense`, `Result` handling, `Atom.family`, `Atom.kvs` + `BrowserKeyValueStore` persistence, suspense/async flows
- New addition: **`AtomRpc` module** integrating `@effect/rpc` with effect-atom
- Status: active but pre-1.0, single maintainer — low adoption signal; **not a drop-in replacement for zustand at production scale**

---

### 4. Missing-Lane Scan

| Lane | Current best-in-class | Effect owner? |
|---|---|---|
| **Background jobs/queues** | `@effect/cluster` **0.56.1** (June 4, 2026) + `@effect/workflow` **0.18.2**; v4 absorbs both into `effect` core under `effect/unstable/*` — workflow engine + ClusterWorkflowEngine exist; production-readiness: **beta/unstable label still applies** | Partial — use bullmq (`bullmq`) for battle-tested Redis-backed queues outside cluster |
| **Email** | No Effect-native client. Modern stack: **`resend`** (HTTP API, TS SDK first-class) + **`react-email`** for templates; nodemailer stays only for raw SMTP/self-hosted | None |
| **Redis** | No `@effect/sql-redis` or dedicated Effect Redis client. `effect/experimental` ships `Persistence/Redis.ts` (key-value persistence layer only, not full Redis client). Use **`ioredis`** (v5, official TS declarations, cluster/pub-sub/streams) | None for full Redis; experimental persistence layer only |
| **WebSockets/realtime** | `@effect/platform` owns `Socket.ts` — `makeWebSocket`, `makeWebSocketChannel`, `fromWebSocket`; browser (`BrowserSocket`), Node, Bun implementations. **Effect owns this lane.** | Yes — `@effect/platform` |
| **File/object storage (S3)** | `@effect-aws/client-s3` **1.11.0** (May 2026, actively maintained, published ~1 month ago) | Yes — effect-aws ecosystem |
| **Payments/webhooks** | No Effect-native Stripe/payment client. Use Stripe's official TS SDK (`stripe`) directly; wire webhooks via `@effect/platform` HTTP handlers | None |
| **Search** | No Effect-native search client. Use provider SDK directly (Typesense, Meilisearch, Elasticsearch official TS clients) | None |

---

### 5. Pulumi

- Latest stable: **v3.245.0** (June 4, 2026) — still major v3; no v4 shipped
- Automation API: stable, available in TS/JS, Python, Go, C#, Java — embeds Pulumi as a library for programmatic stack lifecycle
- Pulumi ESC now integrated into Automation API (dynamic config, CI/CD)
- **No first-class Effect integration packages exist** — none found on npm or GitHub; wire Automation API to Effect programs manually via `Effect.promise` / `Effect.tryPromise`

---

### 6. Biome 2.x

- Latest: **`@biomejs/biome` 2.4.16** (May 27, 2026)
- v2 codename "Biotype" — re-implements TypeScript checker in Rust without invoking `tsc`
- **`noFloatingPromises`**: shipped stable in **v2.4** (promoted from nursery); also `noMisusedPromises`, `useAwaitThenable` promoted to stable
- Detection accuracy: ~85% of cases vs typescript-eslint's full coverage (cross-module generic wrapper detection added in v2.1)
- v2.4 introduces **`types` domain** — isolates type-inference-requiring rules from project-graph-only rules
- v2.4 also adds: embedded CSS/GraphQL linting in JS/TS files, unified type signature options

---

Sources:
- [Effect GitHub Releases](https://github.com/Effect-TS/effect/releases)
- [Effect v4 Beta Blog](https://effect.website/blog/releases/effect/40-beta/)
- [Effect v4 Beta Recap](https://effect.website/blog/effect-v4beta-launch-to-may-recap/)
- [InfoQ: Effect v4 Beta](https://www.infoq.com/news/2026/04/effect-v4-beta/)
- [TypeScript 7.0 Beta — Visual Studio Magazine](https://visualstudiomagazine.com/articles/2026/04/21/typescript-7-0-beta-arrives-on-go-based-foundation-with-10x-speed-claim.aspx)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [TypeScript 7.0 Beta — devblogs.microsoft.com](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [effect-rx renamed to effect-atom — X/tim_smart](https://x.com/tim_smart/status/1953754224958878039)
- [@effect-atom/atom-react npm](https://www.npmjs.com/package/@effect-atom/atom-react)
- [@effect/cluster npm](https://www.npmjs.com/package/@effect/cluster)
- [@effect-aws/client-s3 npm](https://www.npmjs.com/package/@effect-aws/client-s3)
- [Biome v2 — Biotype](https://biomejs.dev/blog/biome-v2/)
- [Biome v2.1](https://biomejs.dev/blog/biome-v2-1/)
- [What's New in Biome v2.4](https://medium.com/@onix_react/whats-new-in-biome-v2-4-00890baad13b)
- [Pulumi Releases](https://github.com/pulumi/pulumi/releases)
- [Pulumi Automation API Docs](https://www.pulumi.com/docs/iac/concepts/automation-api/)
- [Socket.ts — effect platform](https://effect-ts.github.io/effect/platform/Socket.ts.html)
- [Redis.ts — effect experimental](https://effect-ts.github.io/effect/experimental/Persistence/Redis.ts.html)
