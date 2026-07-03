# [RASM_TS_PLATFORM_DECISION]

`libs/typescript` is re-architected from a five-folder wire-consumer companion into a thirteen-folder first-class platform library: the lib-grade substrate for building hundreds of complex apps, complete in isolation and aligned — never coupled — with the C# and Python branches. This brief carries every verdict, map, and leg the stand-up runs against. The durable per-folder guidance lives in `libs/typescript/BLUEPRINT.md`; the two files divide labor and restate nothing.

## [00]-[SHARED_LAW_HEAD]

### [PARADIGM]

- One npm package `@rasm/ts` with per-folder exports subpaths (`./kernel` … `./iac`); the inconsistent corpus `@rasm/*` spellings die. Unexported subpaths are physically unresolvable — module resolution enforces interior/contract splits at a second altitude beneath the Nx tag graph. The exports map never ships a test-infra subpath: the dev plane lives under `tests/`, never `libs/` — plane separation is physical.
- Every folder ships composable `Layer`/`Service` families a thin app composition root selects. An app is a ~30-line `main.ts` that merges Layer families and calls one `runMain`. A lib edit forced by an app need is the named failure (the `Parametric_Portal` inversion); the acceptance gate below makes it mechanical.
- Effect is the settled substrate: `Schema`-first decode-once boundaries, branded types, tagged-union dispatch with `Match.exhaustive`, `Data.TaggedError` rails, `Option` over null-absence, zero `any`/`throw`/`enum`. Vocabulary is values; capability growth is a row, case, policy value, or dispatch arm on the owning surface.
- Zero YAML ever: IaC is Pulumi Automation-API typed programs; Helm values are typed objects; dashboards are total functions. No migrations ever: persistence is an append-only journal with read-time upcasting; `PgMigrator` is banned branch-wide.
- PG-first self-hosting posture: PostgreSQL 18.4 with its extension surface is the durable spine; Redis cache/rate rows exist only as prepared rows an app finalizes.

### [ROSTER]

Thirteen folders: twelve runtime domains inside the soft zone, plus `iac` on the deploy plane — a plane-distinct citizen outside the runtime accounting. The dev plane lives under `tests/` (`tests/typescript/_testkit`, `tests/typescript/_architecture`, `tests/typescript/e2e`, `tests/contracts/`), never under `libs/`. Single-word lowercase; every folder is a genuine higher-order domain with a multi-page sub-domain tree.

| # | Folder | Charter | Higher-order justification |
|---|--------|---------|---------------------------|
| 01 | `kernel` | Cross-language identity, clock, schema-brand, quantity, and fault-classification values | The contract floor carries VALUES only — no wire shape, no transport; every folder types against it, so it can belong to no sibling |
| 02 | `state` | Host-free fold algebra: keyed folds, CRDT merge, causality, evidence, live queries | One algebra, two altitudes (browser in-memory, node durable); homing it in `store` would make zero-durable-state browser apps depend on the SQL folder |
| 03 | `host` | Process runtime: Node/Bun exec rows, config provider chain, flag verdicts, net client/channel policy, lifecycle | The one owner for process-runtime selection; distributing config/flags/exec/net policy across folders forks under app growth |
| 04 | `security` | Authn, authz, sessions, secrets, signing as Effect-owned Layers over stateless primitives | No framework owns our schema; five admission-bounded crypto surfaces form one domain no sibling can absorb |
| 05 | `telemetry` | Four-signal observability plane: OTLP, conventions, RUM, audit/meter fact streams, SLO algebra, dashboards-as-functions | Serves hundreds of apps through app-identity parameterization; merging into `host` would conflate process runtime with the observability contract |
| 06 | `wire` | ALL C#-minted `*Wire` decode, codec dispatch, frames, gateway, contract drift, fault reconstruction, capability SDK | The one boundary rail to the C# wire; a boundary concern inside an owning folder, never the branch gravitational center |
| 07 | `work` | Durable execution: cluster entities, workflows, queues, schedules, signed egress | In-process durable-actor altitude, distinct from deployment topology (`iac`) and from queue-as-data (`store`) by law |
| 08 | `store` | Event-sourced durable persistence: journal, projections, capability rows, tenancy scopes, lanes, retrieval, objects | The no-migration store as lib code; `state` lifted out keeps it the SQL-rail owner, not a grab-bag |
| 09 | `ai` | Intelligence rail: model provider rows, embeddings, tools, durable agents, MCP hosting | Lifted out of `work`: browser/edge AI consumption and the copilot archetype need a peer owner, not a sub-domain of durable execution |
| 10 | `edge` | The one public front door: HttpApi assembly, serve rows, realtime, webhooks, CLI verbs, problem mapping | Public ingress is its own altitude; the god-contract is impossible because the `HttpApi` VALUE exists only in the app |
| 11 | `browser` | Browser runtime: single-boot law, PWA shell, local persistence, decode transport, Navigation-API routing, session ceremonies | Runtime is not capability: `browser` and `ui` are peers an app composes; neither imports the other |
| 12 | `ui` | Component capability: atoms, tokens, interaction, intl, headless views; `viewer` as a second Nx project for spatial/GLB/geo/BCF | React 19 spine as capability rows; the viewer project makes heavy spatial deps compile-time excludable for the non-spatial majority |
| 13 | `iac` | Deploy plane: Pulumi typed programs, provider dispatch, K8s tiers, secrets provisioning, observability stacks, policy | Deployment topology; `plane:deploy`, depended on by nothing at runtime |

Deliberately NOT in the roster: `services` (the named seven-domain over-consolidation — shattered along its own seams), `platform` (three planes wearing one name — split into `host`/`browser`/`telemetry`), `interchange` as branch root (the decode-root posture IS the companion inversion), `projection` as a name (reborn as `state`, substance intact), a top-level `viewer` (second Nx project inside `ui`), `testing`/`proof` (test infrastructure never lives under `libs/` — the frozen corpus is `tests/contracts/`, the TS kit is `tests/typescript/_testkit`, the gauge audits are the `tests/typescript/_architecture` suite, per the `tests/typescript/README.md` law), `geometry` (C#/Python own it; TS consumes GLB), `routing` (Navigation API law — the capability is owned by `browser/route`), `queue` (`work` owns execution semantics, `store` owns queue-as-data), `config`/`flags` (owned by `host`), `cache` (PG-first; Redis rows return as prepared `store/capability` rows), `search` (a `store/retrieve` sub-domain — pg capability, never a peer), `agent` (a sub-domain of `ai`), `notifications`/`email` (delivery rows in `work/deliver`), and any `common`/`shared`/`utils` grab-bag (`kernel` carries cross-language VALUES only; a kitchen-sink kernel is the named defect).

### [EDGE_LEDGER]

The permitted-edge table is the boundary law; roster growth is a reviewed row-add. Acyclic; wave-consistent with the build order. Enforcement is structural, never lint: the `@rasm/ts` exports map makes an unexported interior physically unresolvable, the `tests/typescript/_architecture` suite audits the ledger, and the tag triples stay on every project as graph metadata.

| Folder | May import | Wave |
|--------|-----------|------|
| `kernel` | — | W0 |
| `state` | `kernel` | W1 |
| `host` | `kernel` | W1 |
| `security` | `kernel`, `host` | W1 |
| `telemetry` | `kernel`, `host` | W1 |
| `wire` | `kernel`, `state`, `host` | W2 |
| `work` | `kernel`, `host`, `security`, `telemetry` | W2 |
| `store` | `kernel`, `state`, `host`, `security` | W3 |
| `ai` | `kernel`, `host`, `work` | W3 |
| `edge` | `kernel`, `state`, `host`, `security`, `telemetry` | W4 |
| `browser` | `kernel`, `state`, `host`, `wire`, `security`, `telemetry` | W4 |
| `ui` | `kernel`, `state`, `wire` (`#vocab` subpath only) | W4 |
| `iac` | `kernel`, `store` (capability vocabulary), `telemetry` (board functions) | W4 |

Forced resolutions the ledger encodes:
- `work` never imports `store` (wave-illegal): `work` composes the `@effect/sql` core `SqlClient` Tag and the `@effect/cluster` `MessageStorage` Tag; the app root provides the `store`-owned driver Layer. `@effect/sql` core is admissible in `work`; the `-pg`/`-sqlite-*` DRIVERS are banned outside `store`. The package-level ban cannot see module subpaths: the `tests/typescript/_architecture` suite asserts zero `@effect/sql/Migrator` / `@effect/sql-pg/PgMigrator` imports branch-wide.
- `security` never imports `store` (wave-illegal): `security` declares `SessionStore`/`IdentityJournal` port Tags against its own models; the app root composes `store` journal Layers into them.
- `store → security` is a direct edge (wave-legal): `store/journal` imports the `security/sign` AES-GCM envelope primitive as its crypto-shredding `Shredder`.
- `telemetry` never imports `wire`: crash capture reconstructs `FaultDetail` through the `kernel`-declared enricher contract; `wire` provides the Layer at the app root.
- `ui` never imports `browser` and vice versa: `ui` declares port records (decode-worker residency for `GlbViewport`); `browser` provides Layers at app composition.
- `host/flag` owns flag-verdict evaluation; `security/authz` keeps entitlement CLAIMS and consumes verdicts (`security → host`).
- Only `browser` and `ui` (`#vocab` subpath) carry a `wire` edge; every other runtime folder's row excludes it — the companion inversion, measured by the ledger itself.

Tags: every project carries `scope:<folder>` (plus `scope:viewer` for the `ui` viewer project), `runtime:{neutral,node,bun,browser}`, `plane:{runtime,deploy,dev}`. `runtime:browser` depends only on `{browser, neutral}`; `runtime:node` only on `{node, neutral}`; `plane:deploy` is depended on by nothing; `plane:dev` (the `tests/` estate) imports anything, is imported by nothing.

External-admission union (folder-scoped; enforced by the exports map and the `tests/typescript/_architecture` ledger audit, never a lint layer):
- `@connectrpc/*`, `@bufbuild/*`, `@msgpack/msgpack`, `cbor-x`, `rfc6902` outside `scope:wire`
- `@effect/sql-pg`, `@effect/sql-sqlite-*` outside `scope:store` (+ the `tests/typescript/_testkit` harnesses); `@effect/sql` core exempt
- `@aws-sdk/*`, `sharp` outside `scope:store`
- `@electric-sql/d2ts`, `@electric-sql/d2mini`, `@effect/typeclass` outside `scope:state` (+ the `tests/typescript/_testkit` law combinators)
- `@effect/cluster`, `@effect/workflow` outside `scope:work`
- `@effect/ai*`, `@modelcontextprotocol/sdk` outside `scope:ai`
- `@opentelemetry/*` outside `scope:telemetry`
- `@effect/cli`, `@effect/rpc`, `@effect/printer*` outside `scope:edge`
- `@pulumi/*`, `@pulumiverse/*` outside `scope:iac`
- `react*` outside `scope:{ui,viewer,browser}`
- `three`, `maplibre-gl`, `@deck.gl/*`, `@geoarrow/*`, `@google/model-viewer`, `apache-arrow`, `@lume/kiwi`, `@turf/turf` outside `scope:viewer`
- `hash-wasm` outside `scope:kernel`
- `jose`, `arctic`, `@simplewebauthn/*`, `otplib`, `@node-rs/argon2`, `@oslojs/*`, `@dopplerhq/node-sdk` outside `scope:security`
- `workbox-*`, `idb-keyval`, `nuqs` outside `scope:browser`
- `nodemailer`, `exceljs`, `jspdf`, `jszip`, `papaparse` outside `scope:work`
- `@effect/platform-node`, `@effect/platform-bun`, `node:*` inside `runtime:browser`; `@effect/platform-browser` inside `runtime:node`
- `fast-check`, `testcontainers`, `@electric-sql/pglite`, `@stryker-mutator/*` outside `plane:dev`

Runtime-spanning folders (`store`, `security`, `telemetry`, `wire`, `host`) publish per-runtime subpath exports (`./wasm`, `./browser`, `./server`); the exports map keeps node code physically unreachable from browser resolution. The `tests/typescript/_architecture` suite owns the subpath-purity check Nx cannot express.

### [CATALOG]

`pnpm-workspace.yaml` verdicts. Baseline: `catalogMode: strict` (L178), `saveExact: true` (L233). Every version below is registry-verified current; a judgment that remains open carries its `[Rn]` marker. The Effect family moves as one peer-locked wave under its declared peer ranges; the published `effect` 4.0.0-beta line is pinned out until stable. Leg 16 re-verifies every row against the registry at application time — drift since this gate is a mechanical bump, never a new judgment. Allocation law — which rows catalogue at `libs/typescript/.api/` versus folder-local versus the `tests/typescript/.api/` dev-tool tier — is BLUEPRINT `[15]-[CATALOG_STRUCTURE]`. The dev tier stays in this catalog as workspace dev dependencies, consumed from the tests estate (`tests/typescript`), never from a lib folder.

DROPS (eight, each named):

| Package | Reason |
|---------|--------|
| `ioredis` 5.11.1 | PG-first self-hosting posture; Redis cache/rate/persistence rows return as prepared `store/capability` rows an app finalizes |
| `eslint` 10.6.0, `@nx/eslint` 23.0.1, `@nx/eslint-plugin` 23.0.1, `@effect/eslint-plugin` 0.3.2 | No boundary-lint layer: the edge ledger is enforced by the exports map and audited by the `tests/typescript/_architecture` suite; eslint existed only as the boundary host, and Biome is the one lint rail |
| `@anthropic-ai/tokenizer` 0.0.4 | Redundant with `@effect/ai-anthropic` `AnthropicTokenizer` |
| `@effect-aws/client-s3` 1.11.0 | One S3 owner: raw `@aws-sdk/client-s3` behind the `store/object` service |
| `@aws-sdk/client-sesv2` 3.1067.0 | `nodemailer` row owns mail egress |
| `@pulumi/esc-sdk` 0.14.0 | Doppler canonical; ESC demoted to a prepared row |
| `@nx-extend/pulumi` 12.0.3 | The Automation API driven from `iac/program` needs no plugin |
| `yaml` 2.9.0 | Zero-YAML law; no consumer remains |

REJECTED — never admitted; the catalog's standing negative space (a rejected package returns only by overturning its recorded reason, never by silent re-admission):

| Rejection | Reason |
|-----------|--------|
| `@effect/sql-mysql2` / `-mssql` / `-clickhouse` / `-libsql` / `-d1` / `-sqlite-do` / `-sqlite-react-native` | The sql lanes are pg + sqlite — the dialect law; out-of-scope engines stay prepared dialect rows in `store/capability`, the Cloudflare lanes are rows an app finalizes |
| `kysely`, `drizzle-orm`, `@effect/sql-kysely`, `@effect/sql-drizzle` | Query-builder bridges fragment the one `SqlClient` owner; native `Model`/`SqlSchema`/`Statement` suffice |
| `@effect/schema`, `@effect/match`, `@effect/data` | npm-deprecated, merged into core `effect`; admission is a defect |
| `@effect/rpc-http`, `@effect/cluster-node`, `@effect/cluster-workflow` | Frozen at superseded versions; transports and runner entrypoints live inside the owning core packages |
| `better-auth`, `@openauthjs/openauth`, `oslo`, `@oslojs/jwt` | Framework-owned schema/migrations or superseded monolith splits; `jose` is the one JWT owner and `security` composes stateless primitives |
| `pg-boss`, `graphile-worker` | Promise-based queue frameworks; cluster `MessageStorage` + `@effect/workflow` own the altitude |
| `emmett`, `pongo`, `alvyn`, `EventFabric` | Promise+pg event sourcing; pattern mines only (upcaster/outbox/snapshot lineage) for `store/journal`, never admissions |
| `zustand`, `zundo`, `immer`, `@effect-atom/atom-vue` | `@effect-atom` on the React spine is the one state binding |
| `web-vitals` | Native `PerformanceObserver` budgets own RUM |
| `@pulumi/azure-native` | The fourth cloud stays a prepared-row identity, never a pin |

SUBSTRATE tier (7 runtime + 1 dev):

| Package | Version | Verdict |
|---------|---------|---------|
| `effect` | 3.21.4 | BUMP (3.21.3) |
| `@effect/platform` | 0.96.2 | BUMP (0.96.1) |
| `@effect/platform-node` | 0.107.0 | KEEP |
| `@effect/platform-bun` | 0.90.0 | ADD `[R1]` — peer ranges verified; bun rows go load-bearing only after the Leg-16 install proof |
| `@effect/platform-browser` | 0.76.0 | KEEP |
| `@effect/experimental` | 0.60.0 | KEEP `[R19]` — overlay only; the record of truth never depends on experimental |
| `@effect/opentelemetry` | 0.63.0 | KEEP |
| `@effect/vitest` | 0.29.0 | KEEP (dev substrate) |

FOLDER-LOCAL tiers:

| Owner | Package | Version | Verdict |
|-------|---------|---------|---------|
| kernel | `hash-wasm` | 4.12.0 | FIX exact (was `^4.12.0`, a `saveExact` violation) `[R2]` |
| wire | `@bufbuild/protobuf` | 2.12.1 | BUMP (2.12.0) |
| wire | `@connectrpc/connect` | 2.1.2 | KEEP |
| wire | `@connectrpc/connect-web` | 2.1.2 | KEEP |
| wire | `@msgpack/msgpack` | 3.1.3 | KEEP `[R9]` — drops if platform `MsgPack` covers standalone-payload decode |
| wire | `cbor-x` | 1.6.4 | ADD `[R10]` — canonical-decode member verification before `SnapshotHeader` lands |
| wire | `rfc6902` | 5.2.0 | KEEP |
| state | `@electric-sql/d2ts` | 0.1.8 | KEEP `[R12]` |
| state | `@electric-sql/d2mini` | 0.1.8 | KEEP `[R12]` |
| state | `@effect/typeclass` | 0.40.0 | ADD `[R24]` — Semigroup/semilattice instances for crdt merge + the `tests/typescript/_testkit` law combinators; the else-branch rejection is recorded |
| store | `@effect/sql` | 0.51.1 | KEEP |
| store | `@effect/sql-pg` | 0.52.1 | KEEP |
| store | `@effect/sql-sqlite-node` | 0.52.0 | ADD |
| store | `@effect/sql-sqlite-bun` | 0.52.0 | ADD `[R1]` |
| store | `@effect/sql-sqlite-wasm` | 0.52.0 | ADD `[R13]` |
| store | `@aws-sdk/client-s3` | 3.1078.0 | BUMP (3.1067.0) |
| store | `@aws-sdk/s3-request-presigner` | 3.1078.0 | BUMP (3.1067.0) |
| store | `sharp` | 0.35.3 | BUMP (0.35.1) — object derivative rows |
| work | `@effect/cluster` | 0.59.0 | KEEP `[R5]` |
| work | `@effect/workflow` | 0.18.2 | KEEP |
| work | `nodemailer` | 9.0.3 | BUMP (8.0.11, major) |
| work | `@types/nodemailer` | 8.0.1 | KEEP (types lag; realign at install) |
| work | `exceljs` | 4.4.0 | KEEP — report egress rows |
| work | `jspdf` | 4.2.1 | KEEP |
| work | `jszip` | 3.10.1 | KEEP |
| work | `papaparse` | 5.5.4 | BUMP (5.5.3) |
| work | `@types/papaparse` | 5.5.2 | KEEP |
| ai | `@effect/ai` | 0.36.0 | KEEP |
| ai | `@effect/ai-anthropic` | 0.26.0 | KEEP |
| ai | `@effect/ai-openai` | 0.40.1 | BUMP (0.40.0) |
| ai | `@effect/ai-google` | 0.15.0 | KEEP |
| ai | `@effect/ai-amazon-bedrock` | 0.16.1 | BUMP (0.16.0) |
| ai | `@effect/ai-openrouter` | 0.11.0 | KEEP |
| ai | `@modelcontextprotocol/sdk` | 1.29.0 | ADD — the MCP-client lane only; hosting rides the native `@effect/ai` `McpServer`/`McpSchema` |
| edge | `@effect/cli` | 0.75.2 | KEEP |
| edge | `@effect/rpc` | 0.75.1 | KEEP |
| edge | `@effect/printer` | 0.49.0 | ADD — declared peer of `@effect/cli` 0.75.2; `cli/render` Doc rows |
| edge | `@effect/printer-ansi` | 0.49.0 | ADD — declared peer of `@effect/cli` 0.75.2 |
| security | `jose` | 6.2.3 | ADD — the one JWT/JWS/JWKS owner |
| security | `arctic` | 3.7.0 | KEEP |
| security | `@simplewebauthn/server` | 13.3.2 | BUMP (13.3.1) |
| security | `@simplewebauthn/browser` | 13.3.0 | KEEP |
| security | `otplib` | 13.4.1 | KEEP |
| security | `@node-rs/argon2` | 2.0.2 | ADD |
| security | `@oslojs/crypto` | 1.0.1 | ADD |
| security | `@oslojs/encoding` | 1.1.0 | ADD |
| security | `@dopplerhq/node-sdk` | 1.3.0 | KEEP |
| telemetry | `@opentelemetry/semantic-conventions` | 1.41.1 | KEEP (survives R3) |
| telemetry | `@opentelemetry/core` | 2.8.0 | KEEP `[R3]` — the whole sdk/exporter block collapses when native-Otlp parity closes |
| telemetry | `@opentelemetry/resources` | 2.8.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/sdk-metrics` | 2.8.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/sdk-logs` | 0.219.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/sdk-trace-base` | 2.8.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/sdk-trace-node` | 2.8.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/sdk-trace-web` | 2.8.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/exporter-trace-otlp-http` | 0.219.0 | KEEP `[R3]` |
| telemetry | `@opentelemetry/exporter-metrics-otlp-http` | 0.219.0 | KEEP `[R3]` |
| browser | `idb-keyval` | 6.2.6 | BUMP (6.2.5) |
| browser | `workbox-window` | 7.4.1 | KEEP |
| browser | `workbox-build` | 7.4.1 | KEEP |
| browser | `nuqs` | 2.9.0 | BUMP (2.8.9) `[R17]` — the `route/navigate` composition |
| ui | `react` | 19.2.7 | KEEP `[R16]` |
| ui | `react-dom` | 19.2.7 | KEEP |
| ui | `@types/react` | 19.2.17 | KEEP |
| ui | `@types/react-dom` | 19.2.3 | KEEP (currency verified; R18 closed) |
| ui | `react-aria-components` | 1.19.0 | BUMP (1.18.0) |
| ui | `react-aria` | 3.50.0 | BUMP (3.49.0) |
| ui | `react-stately` | 3.48.0 | BUMP (3.47.0) |
| ui | `@react-aria/live-announcer` | 3.5.1 | KEEP |
| ui | `babel-plugin-react-compiler` | 1.0.0 | KEEP |
| ui | `react-compiler-runtime` | 1.0.0 | KEEP |
| ui | `react-error-boundary` | 6.1.2 | KEEP |
| ui | `@effect-atom/atom` | 0.5.3 | KEEP — the one state binding |
| ui | `@effect-atom/atom-react` | 0.5.0 | KEEP |
| ui | `@floating-ui/react` | 0.27.19 | KEEP |
| ui | `@floating-ui/react-dom` | 2.1.8 | KEEP |
| ui | `@radix-ui/react-label` | 2.1.11 | BUMP (2.1.10) |
| ui | `@radix-ui/react-separator` | 1.1.11 | BUMP (1.1.10) |
| ui | `@radix-ui/react-slot` | 1.3.0 | KEEP |
| ui | `@radix-ui/react-visually-hidden` | 1.2.7 | BUMP (1.2.5) |
| ui | `@tanstack/react-table` | 8.21.3 | KEEP |
| ui | `@tanstack/react-virtual` | 3.14.5 | BUMP (3.14.3) |
| ui | `@use-gesture/react` | 10.3.1 | KEEP |
| ui | `cmdk` | 1.1.1 | KEEP |
| ui | `vaul` | 1.1.2 | KEEP |
| ui | `lucide-react` | 1.23.0 | BUMP (1.20.0) |
| ui | `class-variance-authority` | 0.7.1 | KEEP |
| ui | `clsx` | 2.1.1 | KEEP |
| ui | `tailwindcss` | 4.3.2 | BUMP (4.3.1, lockstep with `@tailwindcss/vite`) |
| ui | `tailwindcss-react-aria-components` | 2.2.0 | BUMP (2.1.1) |
| ui | `tailwind-merge` | 3.6.0 | KEEP |
| ui | `tw-animate-css` | 1.4.0 | KEEP |
| ui | `colorjs.io` | 0.6.1 | KEEP |
| ui | `isomorphic-dompurify` | 3.18.0 | BUMP (3.16.0) |
| viewer | `three` | 0.185.1 | BUMP (0.184.0) |
| viewer | `maplibre-gl` | 5.24.0 | KEEP |
| viewer | `@deck.gl/core` | 9.3.5 | BUMP (9.3.4, family) |
| viewer | `@deck.gl/layers` | 9.3.5 | BUMP |
| viewer | `@deck.gl/geo-layers` | 9.3.5 | BUMP |
| viewer | `@deck.gl/mapbox` | 9.3.5 | BUMP |
| viewer | `@geoarrow/deck.gl-geoarrow` | 0.4.1 | FIX exact (was `^0.4.1`, a `saveExact` violation) |
| viewer | `apache-arrow` | 21.1.0 | ADD (R8 closed: the geoarrow lane requires Arrow tables) |
| viewer | `@google/model-viewer` | 4.3.1 | KEEP |
| viewer | `@lume/kiwi` | 0.4.4 | ADD (R7 closed: maintained Cassowary; `kiwi.js` 1.1.3 rejected stale) |
| viewer | `@turf/turf` | 7.3.5 | ADD — planar ops ONLY; turf does not parse WKB `[R6]` |
| viewer | `@webgpu/types` | 0.1.71 | BUMP (0.1.70) |
| iac | `@pulumi/pulumi` | 3.250.0 | BUMP (3.246.0) |
| iac | `@pulumi/kubernetes` | 4.32.0 | KEEP |
| iac | `@pulumi/aws` | 7.35.0 | BUMP (7.33.0; prepared row) |
| iac | `@pulumi/awsx` | 3.6.0 | KEEP |
| iac | `@pulumi/gcp` | 9.29.0 | ADD (prepared row) |
| iac | `@pulumi/cloudflare` | 6.17.0 | ADD (prepared row) |
| iac | `@pulumi/postgresql` | 3.17.0 | ADD — per-app database/schema provisioning |
| iac | `@pulumi/tls` | 5.5.0 | ADD |
| iac | `@pulumi/random` | 4.21.0 | KEEP |
| iac | `@pulumi/command` | 1.2.1 | KEEP |
| iac | `@pulumi/docker` | 5.1.0 | BUMP (5.0.0; owns builds; `@pulumi/docker-build` 0.0.x not admitted) |
| iac | `@pulumi/policy` | 1.21.0 | KEEP |
| iac | `@pulumiverse/doppler` | 0.9.0 | ADD |
| iac | `@pulumiverse/grafana` | 2.33.0 | ADD |
| dev | `vitest` | 4.1.9 | KEEP |
| dev | `@vitest/browser-playwright` | 4.1.9 | KEEP |
| dev | `@vitest/coverage-v8` | 4.1.9 | KEEP |
| dev | `@vitest/ui` | 4.1.9 | KEEP |
| dev | `fast-check` | 4.8.0 | KEEP |
| dev | `testcontainers` | 12.0.4 | BUMP (12.0.2) |
| dev | `@electric-sql/pglite` | 0.5.3 | ADD — fast unit lane; no-server-extensions caveat stated in the `tests/typescript/_testkit` harness law |
| dev | `@playwright/test` | 1.61.1 | BUMP (1.61.0; R21 closed) |
| dev | `@stryker-mutator/core` | 9.6.1 | KEEP (R21 closed) |
| dev | `@stryker-mutator/typescript-checker` | 9.6.1 | KEEP |
| dev | `@stryker-mutator/vitest-runner` | 9.6.1 | KEEP |
| dev | `@types/k6` | 2.0.0 | KEEP |
| dev | `happy-dom` | 20.10.6 | BUMP (20.10.5) |
| dev | `jsdom` | 29.1.1 | KEEP |
| tooling | `typescript` | 6.0.3 | KEEP |
| tooling | `@typescript/native-preview` | 7.0.0-dev.20260616.1 | KEEP `[R20]` |
| tooling | `@effect/tsgo` | 0.15.0 | BUMP (0.14.5) |
| tooling | `@effect/language-service` | 0.86.2 | KEEP |
| tooling | `@effect/build-utils` | 0.8.9 | KEEP |
| tooling | `@effect/docgen` | 0.5.2 | KEEP |
| tooling | `nx` | 23.0.1 | BUMP (23.0.0; whole `@nx/*` family: `devkit`, `js`, `node`, `playwright`, `plugin`, `react`, `vite`, `vitest`, `docker`, `dotnet`) |
| tooling | `@nxlv/python` | — | DROP — executor-based plugin, not a Crystal plugin; duplicates the assay operator (testing-campaign verdict; already removed from the disk manifests) |
| tooling | `@nx-tools/nx-container` | 7.3.0 | BUMP (7.2.3) |
| tooling | `@berenddeboer/nx-biome` | 1.0.3 | KEEP |
| tooling | `@biomejs/biome` | 2.5.2 | BUMP (2.5.0) |
| tooling | `@ast-grep/cli` | 0.44.0 | BUMP (0.43.0) |
| tooling | `@bufbuild/buf` | 1.71.0 | KEEP |
| tooling | `@bufbuild/protoc-gen-es` | 2.12.1 | BUMP (2.12.0, lockstep with `@bufbuild/protobuf`) |
| tooling | `@swc/core` | 1.15.43 | BUMP (1.15.41) |
| tooling | `@swc-node/register` | 1.11.1 | KEEP |
| tooling | `tsx` | 4.22.4 | KEEP |
| tooling | `tslib` | 2.8.1 | KEEP |
| tooling | `@types/node` | 26.1.0 | BUMP (25.9.3) |
| tooling | `vite` | 8.1.3 | BUMP (8.0.16) |
| tooling | `@vitejs/plugin-react` | 6.0.3 | BUMP (6.0.2) |
| tooling | `@tailwindcss/vite` | 4.3.2 | BUMP (4.3.1) |
| tooling | `@rolldown/plugin-babel` | 0.2.3 | KEEP |
| tooling | `rollup-plugin-visualizer` | 7.0.1 | KEEP |
| tooling | `browserslist` | 4.28.4 | BUMP (4.28.2) |
| tooling | `lightningcss` | 1.32.0 | KEEP |
| tooling | `vite-plugin-pwa` | 1.3.0 | KEEP |
| tooling | `vite-plugin-compression` | 0.5.1 | KEEP |
| tooling | `vite-plugin-csp` | 1.1.2 | KEEP |
| tooling | `vite-plugin-image-optimizer` | 2.0.3 | KEEP |
| tooling | `vite-plugin-inspect` | 11.4.1 | KEEP |
| tooling | `vite-plugin-svgr` | 5.2.0 | KEEP |
| tooling | `vite-plugin-webfont-dl` | 3.12.0 | KEEP |
| tooling | `@octokit/rest` | 22.0.1 | KEEP — workspace automation |
| tooling | `@mermaid-js/mermaid-cli` | 11.16.0 | BUMP (11.15.0) |

NOT admitted, held as prepared/gated: `@grafana/grafana-foundation-sdk` 0.0.18 `[R14]` (board specs compile without it), a WKB parser `[R6]` (`wkx` 0.5.0 is the verified candidate identity; maintenance judgment open), Redis rows (prepared `store/capability` rows), the ESC row (prepared in `iac/secret/inject`), a fourth cloud row (an `iac/provider` dispatch identity); `@pulumi/minio` 0.17.0 rides the `[R15]` MinIO-vs-Garage object-row selection.

Parity posture: ~180 owner-labeled entries against 467 C# pins and ~130 Python deps. Parity is in KIND — event store, extension-tier PG, durable execution, four-signal observability, AI, identity, retrieval. TS differs by SUBTRACTION (no geometry/tensor/solver/CAS/IFC tiers — siblings own them; TS consumes GLB and decoded wire only) and by ADDITION (`iac` and `browser` — no sibling owns either).

### [SEAM_MAP]

Every re-mirrored row names both endpoints. Old rows are edited in place as mechanical string replacements: replace the `typescript:<old>` token with the new-row token; apply the listed label rewrite where the label text names a stale page/anchor; leave every other byte identical. All endpoint pages are design pages under `libs/typescript/<folder>/.planning/`. Source keys: BR=`libs/csharp/.planning/ARCHITECTURE.md`, K=`libs/csharp/Rasm/ARCHITECTURE.md`, AH=`libs/csharp/Rasm.AppHost/ARCHITECTURE.md`, CO=`libs/csharp/Rasm.Compute/ARCHITECTURE.md`, PE=`libs/csharp/Rasm.Persistence/ARCHITECTURE.md`, AU=`libs/csharp/Rasm.AppUi/ARCHITECTURE.md`, BM=`libs/csharp/Rasm.Bim/ARCHITECTURE.md`, EL=`libs/csharp/Rasm.Element/ARCHITECTURE.md`, MA=`libs/csharp/Rasm.Materials/ARCHITECTURE.md`.

| Row | Old (verbatim) | New row | Endpoint page(s) → consumer |
|-----|----------------|---------|------------------------------|
| BR:29 | `Rasm.AppHost → typescript:interchange # [WIRE]: ReceiptEnvelope/HLC/Tenant + capability SDK` | `typescript:wire` | `wire/codec/envelope.md` + `wire/invoke/capability.md`; HLC vocabulary `kernel/clock/hlc.md` |
| BR:30 | `Rasm.Compute → typescript:interchange # [WIRE]: proto suite wire + FaultDetail` | `typescript:wire` | `wire/codec/proto.md` + `wire/fault/detail.md` |
| BR:31 | `Rasm.Persistence → typescript:interchange # [WIRE]: OpLog/Snapshot CRDT wire` | `typescript:wire` | `wire/codec/oplog.md` + `wire/codec/snapshot.md` → `store/journal` |
| BR:34 | `Rasm.Element ⇄ typescript:interchange # [WIRE]: ElementGraph/Node/Relationship content-keyed wire the TypeScript peer decodes` | `typescript:wire` | `wire/codec/graph.md` |
| K:47 | `Geometry/Spatial/reconciliation ⇄ typescript:interchange/codec # [CONTENT_KEY]: content-hashing wasm reproducing the one Domain/ContentHash seed (XxHash128 seed-zero)` | `typescript:kernel` | `kernel/identity/contentkey.md`; driver: the `tests/contracts` corpus (TS reader in `tests/typescript/_testkit`) |
| AH:52 | `Agent/Capability.cs → typescript:interchange/codec # [CONTENT_KEY]: CapabilityDescriptor command-shape` | `typescript:wire/invoke` | `wire/invoke/capability.md` |
| AH:53 | `Runtime/Ports.cs → typescript:interchange/codec # [CONTENT_KEY]: HLC two-half bigint round-trip parity` | `typescript:kernel` | `kernel/clock/hlc.md` |
| AH:55 | `* → typescript:services # [WIRE]: CredentialPemWire redacted carrier` | `typescript:wire` | `wire/codec/credential.md` → `security/secret/material.md` |
| AH:56 | `* → typescript:interchange # [WIRE]: support-capture verb` | `typescript:wire/gateway` | `wire/gateway/support.md` → `telemetry/signal/crash.md` |
| AH:58 | `Observability/Health.cs → typescript:projection/evidence # [WIRE]: DegradationLevel / CommandAvailabilityWire` | `typescript:state` | `state/evidence/availability.md` (decode `wire/codec/envelope.md`) |
| AH:60 | `Observability/Telemetry.cs → typescript:ui/render # [WIRE]: BenchmarkClaimWire / HostFingerprintWire identity gate` | `typescript:wire` | `wire/codec/claim.md` → `ui/viewer` `probe/benchmark.md` |
| AH:63 | `Runtime/Ports.cs → typescript:projection/evidence # [WIRE]: ReceiptEnvelopeWire / HlcStampWire / TenantContextWire` | `typescript:state` | `state/evidence/receipt.md` (decode `wire/codec/envelope.md`) |
| AH:64 | `Runtime/Ports.cs → python:runtime/clock + typescript:projection/convergence # [WIRE]: HLC two-half + tenant [gated: hash-wasm / xxhash cp315]` | python half unchanged; `typescript:kernel` | `kernel/clock/hlc.md`; fixtures: the `tests/contracts` corpus (TS reader in `tests/typescript/_testkit`) `[R22]` |
| AH:65 | `Wire/Livewire.cs → typescript:ui/render # [WIRE]: BindingStatusWire / CoercedValueWire / WriteReceiptWire` | `typescript:wire` | `wire/codec/livewire.md` → `ui/viewer` `panel/binding.md` |
| AH:66 | `Observability/Telemetry.cs → typescript:platform/observability # [TRANSPORT]: OtelExport OTLP egress` | `typescript:telemetry` | `telemetry/otlp/export.md` |
| AH:83 | `Runtime/Features.cs → typescript:platform # [WIRE]: FlagVerdictWire over the shared OpenFeature evaluation contract (ONE_FEATURE_FLAG_PROJECTION)` | `typescript:host` | `host/flag/verdict.md` (decode `wire/codec/flag.md`) |
| CO:67 | `Runtime/channels → typescript:interchange/codec # [WIRE]: ReceiptEnvelopeWire / FaultDetailWire / proto vocabulary` | `typescript:wire` | `wire/codec/proto.md` + `wire/fault/detail.md` |
| CO:68 | `Runtime/channels → typescript:interchange/contract # [WIRE]: FileDescriptorSet ContractDrift verdict` | `typescript:wire/contract` | `wire/contract/descriptor.md` |
| CO:69 | `Runtime/channels → typescript:platform/transport # [WIRE]: ArtifactFrameWire reassembly` | `typescript:wire/frame` | `wire/frame/artifact.md` → `browser/transport/pool.md` |
| CO:70 | `Runtime/channels → typescript:ui/render # [WIRE]: GeometryPayload proto descriptor / MeshTensor view` | `typescript:wire/frame` | `wire/frame/geometry.md` → `ui/viewer` `scene/glb.md` |
| CO:73 | `Runtime/progress → typescript:projection/evidence # [WIRE]: ProgressMarkWire` | `typescript:state` | `state/evidence/progress.md` (decode `wire/codec/progress.md`) |
| CO:76 | `Runtime/progress → typescript:interchange/codec # [PROJECTION]: ProgressStore stream proto` | `typescript:wire` | `wire/codec/progress.md` |
| CO:96 | `Symbolic ⇄ python:compute + typescript:interchange # [WIRE]: QuantityFamily SI canonicalization consumed by host-free peers over the wire (…)` | python half unchanged; `typescript:kernel` | `kernel/schema/quantity.md` (decode transit `wire/codec/proto.md`) |
| CO:110 | `Runtime/codecs → python:runtime/evidence/identity + typescript:interchange/Codec/frame # [WIRE]: XxHash128 seed-zero two-half [gated: hash-wasm / xxhash cp315]` | python half unchanged; `typescript:kernel` | `kernel/identity/contentkey.md` + `wire/frame/artifact.md` `[R2]` |
| PE:43 | `Element/codec → typescript:interchange/codec # [WIRE]: SnapshotHeader + canonical-CBOR content-stable bytes` | `typescript:wire` | `wire/codec/snapshot.md` → `store/journal/snapshot.md` `[R10]` |
| PE:44 | `Version/commits → typescript:interchange/codec # [WIRE]: CrdtOpWire MessagePack union + Hlc 16-byte cell` | `typescript:wire` | `wire/codec/crdt.md` → `state/crdt/merge.md` |
| PE:46 | `Version/commits → typescript:interchange/refinement # [SHAPE]: commit/branch/version-vector/Merkle wire shapes` | `typescript:state` | `state/causal/vector.md` (decode `wire/codec/version.md`) |
| PE:47 | `Version/merge → typescript:interchange/codec # [SHAPE]: JsonPatchDocument RFC 6902 EntityEdit egress` | `typescript:wire` | `wire/codec/patch.md` |
| AU:59 | `Shell/commands → typescript:interchange/transport # [WIRE]: CommandPayloadWire + AvailabilityStore gate` | `typescript:wire/gateway` | `wire/gateway/command.md` (gate port typed against `state/evidence/availability.md`) |
| AU:60 | `Render/capture → typescript:interchange/codec # [PROJECTION]: RenderReceiptWire frame-hash proof` | `typescript:wire` | `wire/codec/envelope.md` → `ui/viewer` `probe/receipt.md` |
| AU:61 | `Render/evidence → typescript:projection/evidence # [PROJECTION]: EvidenceFeed / EvidenceTimeline` | `typescript:state` | `state/evidence/timeline.md` |
| AU:62 | `Render/pipeline → typescript:platform/transport # [PROJECTION]: GeometryResidencyWire ResidencyManifest content-key` | `typescript:wire/frame` | `wire/frame/residency.md` → `browser/transport/pool.md` |
| AU:63 | `Render/glb → typescript:ui/render # [RECEIPT]: ResidencyManifest content-key-keyed mesh residency` | `typescript:ui/viewer` | `ui/viewer` `scene/glb.md` |
| AU:82 | `Shell/controls → typescript:interchange/transport # [WIRE]: ControlIntentWire kind-discriminated control vocabulary` | `typescript:wire` | `wire/codec/control.md` → `ui/viewer` `panel/control.md` |
| AU:83 | `Shell/solver → typescript:ui/render # [WIRE]: LayoutConstraintWire ordered Kiwi constraint program` | `typescript:wire` | `wire/codec/layout.md` → `ui/viewer` `panel/layout.md` |
| BM:63 | `* → typescript:interchange # [WIRE]: BcfTopicWire / BcfViewpointWire` | `typescript:wire` | `wire/codec/bcf.md` |
| BM:64 | `Review/issues → typescript:ui/overlay # [WIRE]: BcfTopicWire / BcfViewpointWire` | `typescript:ui/viewer` | `ui/viewer` `mark/bcf.md` |
| BM:67 | `Model/elements → typescript:ui/overlay # [SHAPE]: GlobalId element selection set` | `typescript:ui/viewer` | `ui/viewer` `mark/selection.md` |
| BM:77 | `Semantics/geospatial → typescript:interchange # [WIRE]: GeoFeature WKB decode via turf (NTS-equivalent planar peer)` | `typescript:wire` | `wire/codec/geo.md` — WKB parse is its own row `[R6]`; turf owns planar ops only, in `ui/viewer` `geo/layers.md`; label rewrite: `via turf (NTS-equivalent planar peer)` → `(WKB parser [R6]; turf NTS-equivalent planar peer in ui/viewer)` |
| BM:98 | `Exchange/wire → typescript:interchange # [WIRE]: BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity` | `typescript:wire` | `wire/codec/bim.md` + `wire/codec/graph.md`; fixtures: the `tests/contracts` corpus (TS reader in `tests/typescript/_testkit`) |
| BM:99 | `Exchange/wire → typescript:ui/overlay # [WIRE]: BcfWire/DiffWire GlobalId anchor decode` | `typescript:ui/viewer` | `ui/viewer` `mark/bcf.md` |
| EL:68 | `Graph/wire ⇄ typescript:interchange # [WIRE]: the rasm.element.v1 ElementGraphWire/NodeWire/RelationshipWire proto the interchange Codec proto row decodes under the Contract/descriptor FileDescriptorSet drift gate (Identical/Additive/Breaking) — the element.proto this folder owns is the descriptor source that gate diffs` | `typescript:wire` | `wire/codec/graph.md` + `wire/contract/descriptor.md`; label rewrites: `the interchange Codec proto row` → `the wire codec/graph row`, `the Contract/descriptor FileDescriptorSet drift gate` → `the wire contract/descriptor drift gate` |
| EL IDEAS:40 | `Ripple pointers python:geometry/ifc [SEAM_WIRE_DECODE] / typescript:interchange [SEAM_WIRE_DECODE] retained for the peer card pools` | `typescript:wire` | `wire/codec/graph.md` |
| MA:65 | `Appearance/interchange → typescript:interchange/codec # [WIRE]: decode-only MaterialWire/OpenPbrGroupsWire/AppearanceSummary mirroring the C# projection field-for-field; a peer re-mint of the OpenPBR algebra is the CROSS_LANGUAGE_WIRE drift defect` | `typescript:wire` | `wire/codec/appearance.md` → `ui/viewer` `scene/appearance.md` (the index-promised cluster, authored fresh) |

Residual page/idea rows (the sweep beyond ARCHITECTURE files; each a mechanical token replacement):

| Row | Old token(s) | New token(s) |
|-----|--------------|--------------|
| `Rasm.AppUi/.planning/Render/pipeline.md:548` | `libs/typescript/ui` worker; `typescript:ui/render/glb#GLB_VIEWPORT` | `libs/typescript/browser` worker (`browser/transport/pool.md`); `typescript:ui/viewer/scene/glb#GLB_VIEWPORT` |
| `Rasm.AppUi/.planning/Shell/controls.md:204` | `typescript:interchange` decodes; `typescript:ui/render` materializes | `typescript:wire` decodes; `typescript:ui/viewer` materializes (`panel/control.md`) |
| `Rasm.AppUi/.planning/Shell/solver.md:216` | `typescript:ui/render` head; `kiwi.js` web tableau | `typescript:ui/viewer` head (`panel/layout.md`); `@lume/kiwi` web tableau |
| `Rasm.Compute/.planning/Runtime/codecs.md:760` | `lang:typescript:interchange/Codec/frame#CONTENT_HASHING`; `lang:typescript:services/persistence/object` `ObjectKey` | `lang:typescript:wire/frame#CONTENT_HASHING`; `lang:typescript:store/object` `ObjectKey` |
| `Rasm/Geometry/.planning/Spatial/reconciliation.md:119` | `typescript:interchange/Codec/frame#CONTENT_HASHING`; `the new typescript:testing/ top-level reads the corpus` | `typescript:wire/frame#CONTENT_HASHING`; `the tests/contracts corpus (TS readers in tests/typescript/_testkit)` |
| `Rasm.Element/ARCHITECTURE.md:67` | `typescript:interchange` (graph-wire row tail: `the python:geometry/ifc + typescript:interchange graph-wire rows`) | `typescript:wire` (`wire/codec/graph.md`) |
| `Rasm.Element/.planning/Graph/wire.md:3,18,1058` | `typescript:interchange` `Contract/descriptor` (three hits, one token pair) | `typescript:wire` `contract/descriptor` (`wire/contract/descriptor.md`) |
| `Rasm.AppHost/IDEAS.md:23` | `…web flow and policy/role admin surface in libs/typescript` | `…web flow in typescript:security + typescript:browser; policy/role admin surface in typescript:ui (over security/authz)` |
| `Rasm.AppHost/IDEAS.md:39` | `…OpenFeature client provider in libs/typescript` | `…OpenFeature client provider in typescript:host (host/flag)` |

Python-side seams: zero edits — no Python doc carries a TS folder token (verified: every `libs/python` match is pathless peer prose or a tree-sitter grammar name). The TS↔Python relationship remains the C#-owned wire plus the GLB residency rail, met at `wire/frame/residency.md` → `browser/transport/pool.md` → `ui/viewer` `scene/glb.md`. No TS↔Python seam is invented.

### [INVARIANTS]

Frozen cross-language law; the `tests/contracts` corpus drivers (TS readers in `tests/typescript/_testkit`) assert each:
1. C# owns every `*Wire` shape. TS decodes verbatim and authors no wire shape; app-authored `store` events never cross the C# wire. A TS re-mint of any `*Wire`, a second content-address notion, or a non-zero hash seed is the named cross-language drift defect.
2. `XxHash128` seed-zero content identity, `:x32` spelling: ONE mint in `kernel/identity`; exactly three delegating sites (`wire/frame`, the `browser/transport` worker, `store/object` `ObjectKey`); LE→BE normalize at every site; bit-parity against `CANONICAL_BYTE_IDENTITY` + `MATERIAL_LAYER_GOLDEN` `[R2]`.
3. HLC compose order is byte-identical to the C# port law: physical half first, logical half second, both little-endian; two-half parity vectors `[R22]`.
4. SI canonicalization happens once at C# admission; `kernel/schema` `Quantity` carries SI magnitude + dimension; a `{value, unit}` re-decode is the rejected form.
5. The C# typed-receipt family never collapses into one erased TS receipt.
6. Fault altitudes stay three and never merge: `wire/fault` (wire-only `FaultDetail` reconstruction), per-folder `Data.TaggedError` rails, `edge/problem` (outbound-only). A node rail importing `FaultDetail` for a local failure is the named defect.
7. TS owns no geometry; the GLB tessellation rail is consume-only. IFC/BCF vocabulary is confined to `wire` codecs and `ui/viewer` marks — no other folder imports it.
8. Adopted-verbatim names exist at the decode boundary only: `ContentKey`, `Hlc`, `TenantContext`, `ReceiptEnvelope`, `FaultDetail` — one name, one owning side. No TS folder name mirrors a sibling package (`store`≠Persistence, `state`≠AppUi, `work`≠Compute, `wire`≠AppHost).
9. TS imports zero C#/Python artifacts. Alignment travels through wire bytes, the frozen corpora, and the `wire/contract` descriptor drift gate.

### [ARCHIVE_AND_SALVAGE]

Archive law: `libs/typescript/{interchange,platform,projection,services,ui}` move wholesale to `libs/typescript/_tmp/` (Leg 00). `_tmp` is read-only mining material: no import from it, no path salvage, no page pasted verbatim; content is re-authored at the doctrine bar. `_tmp` is deleted at campaign close.

Salvage law: the BLUEPRINT names per-folder mine/discard targets; salvage mines CONTENT, never paths — all pre-reorg import paths die wholesale. Discards are settled in the BLUEPRINT tables; a salvage leg that resurrects a named discard is a defect.

Named deaths, justified: `interchange` as branch root (the decode-root posture IS the companion inversion — machinery→`wire`, identity/clock→`kernel`, corpora→`tests/contracts`); `services` (seven-domain over-consolidation, shattered along its own seams); `platform` (three planes wearing one name); `projection` as a name (reborn as `state`); old `ui`'s one-file `binding/`+`theming/` sub-folders (die into `atom/`+`token/`); the Migrator-DDL-at-startup posture; the `glb.md` code-keyed `HopFault` construction (closed `HopReason` vocabulary wins); the stale RESEARCH-blocked annotations in ui bcf/dashboard pages (superseded by the seam map); the stale `platform/persistence` seam label; the five-feed budget as universal lib law (demoted to Rasm-product guidance); the three phantom citations (`edge` stood up; `worker` → `browser/transport` + `host/exec`; `testing` → the tests estate: `tests/contracts` + `tests/typescript/_testkit`).

### [ACCEPTANCE]

Per-leg done condition, not page count: six archetype composition roots — SaaS product, realtime dashboard, geospatial/BIM viewer, headless service, CLI tool, AI copilot — each a ~30-line `main.ts` merging the selected folders' Layer families into one `runMain`, with ZERO lib edits per app, ever. A folder leg closes only when every archetype that selects it composes it thinly.

| Folder | SaaS | Dashboard | Viewer | Headless | CLI | Copilot |
|--------|------|-----------|--------|----------|-----|---------|
| kernel | x | x | x | x | x | x |
| state | x | x | x | — | — | x |
| host | x | x | x | x | x | x |
| security | x | x | x | x | — | x |
| telemetry | x | x | x | x | x | x |
| wire | — | x | x | x | — | — |
| work | x | — | — | x | — | x |
| store | x | x | — | x | x | x |
| ai | x | — | — | x | — | x |
| edge | x | x | — | x | x | x |
| browser | x | x | x | — | — | — |
| ui | x | x | x | — | — | x |
| viewer (project) | — | x | x | — | — | — |
| iac | x | x | — | x | — | x |

Every folder clears the ≥2-archetype bar.

### [RESEARCH_LEDGER]

Carried rows — undecided, never asserted; each blocks only its named surface:
- `[R1]` peer ranges registry-verified: `@effect/platform-bun` 0.90.0 declares `effect ^3.21.2` / `@effect/platform ^0.96.1` / `@effect/sql ^0.51.1` / `@effect/cluster ^0.59.0` / `@effect/rpc ^0.75.1` — every settled pin satisfies every range; the open residual is the Leg-16 install proof gating any bun row load-bearing.
- `[R2]` `hash-wasm` `XxHash128` seed-zero two-half bit-parity vs `CANONICAL_BYTE_IDENTITY` + `MATERIAL_LAYER_GOLDEN` — gates the kernel mint going load-bearing.
- `[R3]` native `@effect/opentelemetry` Otlp coverage parity — closes with collapse of the `@opentelemetry` sdk/exporter block; `semantic-conventions` survives.
- `[R4]` `SqlEventJournal`/`SqlEventLogServer` bootstrap: adopt only if verifiably ensure-shaped, else the journal DDL is owned locally.
- `[R5]` `@effect/cluster` `MessageStorage` schema bootstrap posture — absorbed into the ensure row if migration-shaped.
- `[R6]` WKB parser identity (`wkx` 0.5.0 is the verified candidate; maintenance judgment open); turf planar-only law settled regardless.
- `[R9]` `@effect/platform` `MsgPack` standalone-payload decode — drop `@msgpack/msgpack` if covered.
- `[R10]` `cbor-x` canonical-decode member surface for `SnapshotHeader` content-stable bytes.
- `[R11]` extension matrix: VectorChord pin + PG 18.4 compatibility; postgis version; `pg_uuidv7` vs native `uuidv7()`; pgmq SQL ergonomics; `pg_ivm` currency. Target floors ride in `store/capability/matrix.md`; every probe is fail-closed by construction. Candidate floors (RESEARCH, probes decide): timescaledb 2.28.2, pg_partman 5.4.3, pgmq 1.11.1, pg_cron 1.6.7, pg_ivm 1.15, pgvector 0.8.4, pg_search 0.24.1, pg_duckdb 1.1.1, pg_graphql 1.6.1, pg_jsonschema 0.3.4, pgaudit 18.0, pg_uuidv7 1.7.0; postgis + h3 floors open.
- `[R12]` `@electric-sql/d2ts`/`d2mini` 0.1.8 maintenance currency.
- `[R13]` `@effect/sql-sqlite-wasm` OPFS capability depth.
- `[R14]` `@grafana/grafana-foundation-sdk` (0.0.18) pre-1.0 churn gate — board specs compile without it.
- `[R15]` CNPG operator row judgment; MinIO-vs-Garage object row selection.
- `[R16]` React `<ViewTransition>`/`<Activity>` member verification vs 19.2.7 — `ui/act` degrades to the native View Transitions API.
- `[R17]` `nuqs` composes-with-Navigation-API verification (else dropped; zero-routing-package law regardless).
- `[R19]` `@effect/experimental` `EventLog` overlay member surface (system-of-record boundary is law, not research).
- `[R20]` `@typescript/native-preview` channel currency.
- `[R22]` upstream blocker: the C# HLC two-half fixture corpus landing — blocks the `tests/contracts` HLC vectors (TS readers in `tests/typescript/_testkit`); not resolvable TS-side.
- `[R23]` GLB-rail meshopt posture — if the C# IfcConvert artifact emits `EXT_meshopt_compression`, a viewer-local decoder admission follows (identity and version RESEARCH); blocks only `viewer` `scene/glb`.
- `[R24]` `@effect/typeclass` 0.40.0 admission — lands only if the `state/crdt` merge algebra and the `tests/typescript/_testkit` law combinators verifiably compose its Semigroup/semilattice instances; else effect-core `Order`/`Equivalence` suffice and the rejection joins the ledger.
- `[R25]` `@effect-atom` `AtomHttpApi`/`AtomRpc` direct-binding member surface — blocks only the `ui/atom` direct-binding rows; `Atom` + effect-result folds carry the binding meanwhile.

Closed by this gate: R7 (`@lume/kiwi` 0.4.4 settled; `kiwi.js` rejected stale), R8 (`apache-arrow` 21.1.0 admitted), R18 (`@types/react-dom` 19.2.3 verified current), R21 (Stryker 9.6.1 current; `@playwright/test` bumps 1.61.1).

## [LEG_00]-[ARCHIVE_MOVE]

Move `libs/typescript/interchange`, `libs/typescript/platform`, `libs/typescript/projection`, `libs/typescript/services`, `libs/typescript/ui` → `libs/typescript/_tmp/<same-name>` via `git mv`. No content edit. `libs/typescript/.planning/` and `libs/typescript/.api/` stay in place for Leg 15/16 rebuild. Nothing may import from `_tmp`.

## [LEG_01]-[KERNEL]

Cross-language VALUES only — no wire shape, no transport, no sibling import. Files: `libs/typescript/kernel/{README,ARCHITECTURE,IDEAS,TASKLOG}.md` (`IDEAS.md`/`TASKLOG.md` land as empty placeholders — here and in every folder leg), `libs/typescript/kernel/.api/.gitkeep`, and `.planning/` page stubs:
`identity/contentkey.md`, `identity/appidentity.md`, `clock/hlc.md`, `clock/uncertainty.md`, `schema/brand.md`, `schema/quantity.md`, `fault/classify.md`, `fault/budget.md`.
Laws: the ONE `XxHash128` mint (invariant 2) `[R2]`; HLC order law (invariant 3); `AppIdentity`/`AppKey`/`TenantContext` are the values `telemetry` Resources, `browser` boot, and `store` scopes all derive from; the fault enricher CONTRACT is declared here (telemetry consumes, wire implements); `Quantity` carries SI magnitude + dimension (invariant 4). The branded roster names Guid-v7, OrdinalKey, JsonPointer, and BCP-47 `Locale` with `INGRESS_BUDGET` decode budgets; `AppIdentity` spans {app, tenant, build, host-fingerprint}. Schema-derived arbitraries for the kernel brands live in `@rasm/ts-testkit` (`tests/typescript/_testkit`), never on a `@rasm/ts` subpath — `fast-check` never rides a runtime graph.

## [LEG_02]-[PROOF_DISSOLUTION]

ROUTE record — executed by the testing-campaign reconciliation, not a stand-up leg. `proof` is dissolved out of `libs/typescript` entirely: test infrastructure never lives under `libs/` (the testing-campaign law), and the `@rasm/ts` exports map ships no `./proof` or any other test-infra subpath — plane separation is physical. The re-homing map: corpus BYTES and the producer/consumer law → `tests/contracts/` (C# sole producer; the frozen byte-identity digest, `MATERIAL_LAYER_GOLDEN`, BimWire golden bytes, and HLC two-half vectors `[R22]` land there, unblocking `ContentKeyParity`, `HlcTwoHalfParity`, `CONVERGENCE_FIXTURE_CORPUS`); TS corpus readers, law combinators, Schema-derived arbitraries, and harness layers (testcontainers pg-18.4-with-extensions + the S3-compatible object-store row — the `store/capability` and `store/object` presign verification lanes; the pglite fast unit lane with its no-server-extensions caveat; `@effect/vitest` layer sharing, never a hand-rolled wrapper) → `tests/typescript/_testkit` (`@rasm/ts-testkit`); the gauge audits (edge-ledger import audit, per-runtime subpath purity, external-admission and per-sub-folder package-admission audits — the checks the exports map cannot express) → the `tests/typescript/_architecture` suite (home documented now, suites land at TS buildout); mutation thresholds-as-data → `.config/stryker.config.json`; e2e drivers → `tests/typescript/e2e`; the dev-tool `.api` catalogs → `tests/typescript/.api/`. Specs colocate beside their owning folders and import the kit through the workspace graph. Later legs that name gauge audits point at the `tests/typescript/_architecture` home.

## [LEG_03]-[STATE]

The host-free algebra; the old projection fold-algebra survives whole and gains a second consumer: one algebra, two altitudes — browser apps fold wire-decoded events in memory; node apps fold journal events durably (`store/project` binds). Files: four index docs, `.api/.gitkeep`, stubs:
`fold/algebra.md`, `fold/replay.md`, `crdt/merge.md`, `crdt/converge.md`, `causal/vector.md`, `causal/order.md`, `evidence/receipt.md`, `evidence/availability.md`, `evidence/progress.md`, `evidence/timeline.md`, `query/live.md`, `query/window.md`.
Laws: `state` never imports `wire` — wire decodes INTO state vocabulary; phantom-testing citations repoint to the tests estate (`tests/contracts` + `tests/typescript/_testkit`); array re-sort fallbacks are the named discard; incremental-dataflow lane rides d2ts `[R12]`; presence semantics live here (`edge/live` serves them); `query/window.md` carries AsOf 3-coordinate time-travel reads + `asOfDiff` and HLC event-time watermarks; `causal/order.md` carries the causal delivery buffer, the stability frontier (GLB meet), causal finalize, and the retention-frontier handoff to `store/journal/retain`.

## [LEG_04]-[HOST]

Process-runtime substrate. Files: four index docs, `.api/.gitkeep`, stubs:
`exec/runtime.md`, `exec/process.md`, `net/client.md`, `net/channel.md`, `config/provider.md`, `config/schema.md`, `flag/verdict.md`, `flag/rollout.md`, `life/cycle.md`, `life/health.md`.
Laws: Node|Bun runtime rows — a bun swap is a Layer selection in the app root, never a fork `[R1]`; `WorkerRunner` pools; the `ConfigProvider` chain (env, `doppler run` injection, file, remote); `host/flag` owns `FlagVerdict` evaluation over the shared OpenFeature evaluation contract (decode via `wire/codec/flag.md`; claims stay in `security/authz`); `flag/verdict.md` is runtime-neutral so browser apps evaluate flags, re-evaluating over a live SSE stream row (remote provider) with verdict cache/stickiness rows on `flag/rollout.md`. `net/client.md` owns the branch-wide `HttpClient` default-policy rows (timeout/retry/proxy) and `net/channel.md` the Socket/Ndjson channel rows — `ai` providers, `work` runner discovery, and OTLP export compose both.

## [LEG_05]-[SECURITY]

Effect-owned Layers over stateless primitives; no framework owns our schema (the better-auth rejection as architecture). Files: four index docs, `.api/.gitkeep`, stubs:
`authn/oauth.md`, `authn/webauthn.md`, `authn/otp.md`, `authn/apikey.md`, `session/token.md`, `session/cookie.md`, `authz/policy.md`, `authz/claim.md`, `secret/doppler.md`, `secret/material.md`, `sign/jwt.md`, `sign/crypto.md`.
Laws: admission boundaries ban-enforced — `arctic` only in `authn`; `jose` only in `sign` (the one JWT/JWS/JWKS owner); `@simplewebauthn` only in `authn` (browser-safe ceremony subpath); `otplib` only in `authn`; `@node-rs/argon2` + `@oslojs/*` only in `sign` (node-only subpaths); `@dopplerhq/node-sdk` only in `secret`. Identity state is event-sourced through `SessionStore`/`IdentityJournal` port Tags the app root satisfies with `store` journal Layers. `session` owns the token/refresh/cookie vocabulary (httpOnly, sameSite, path-scoped rows), the refresh rotation/revocation law, and the CSRF law. `secret` owns TTL-leased rotation and `Redacted` end-to-end; `CredentialPemWire` decodes in `wire` and terminates in `secret/material.md`. `sign/crypto.md` owns HMAC webhook signing and the AES-GCM envelope `Shredder` primitive `store/journal` consumes. The tenancy contract (`app.current_tenant`) declared in `authz/claim.md` is what `store` enforces as RLS. `authn/apikey.md` owns the machine-credential family — mint, digest-at-rest, rotate/revoke, prefix-indexed byHash resolve; hashing delegates `sign/crypto`. `authn/otp.md` carries recovery/backup-code rows.

## [LEG_06]-[TELEMETRY]

One plane serving hundreds of apps through app-identity parameterization. Files: four index docs, `.api/.gitkeep`, stubs:
`otlp/export.md`, `otlp/context.md`, `signal/convention.md`, `signal/vital.md`, `signal/crash.md`, `signal/audit.md`, `signal/meter.md`, `slo/burnrate.md`, `slo/alert.md`, `board/model.md`, `board/library.md`.
Laws: the OTel Resource derives from the same `AppIdentity` value `browser` boot and `StoreHandle` use; signal conventions are vocabulary rows (names as data). Export is the `@effect/opentelemetry` Otlp family with NodeSdk/WebSdk rows; W3C composite extract-and-continue at every ingress; the `@opentelemetry` pin block collapses when `[R3]` closes. Browser RUM: native `PerformanceObserver` budgets (zero `web-vitals`); crash capture reconstructs `FaultDetail` through the kernel-declared enricher contract (telemetry never imports `wire`); session replay with redaction-at-capture. SLO is algebra: multi-window multi-burn-rate typed policy rows. `board` exports TOTAL FUNCTIONS `AppIdentity -> DashboardModel` — a per-app dashboard fork is structurally impossible; `@grafana/grafana-foundation-sdk` stays behind the facade `[R14]`. Effect DevTools ships as the dev-only `./dev` export row, `plane:dev`-fenced. `signal/audit.md` owns the audit fact stream — actor/action/target vocabulary, typed diff evidence, retention classes — durable through a journal port Tag the app root satisfies with `store`. `signal/meter.md` owns the usage/cost metering fact stream — `(app, tenant)`-keyed request/compute/storage/token counters with rating policy rows, durable through the same journal port law — the billing and cost-attribution source every multi-tenant archetype rolls up. OTLP egress carries redaction policy rows at the export boundary, distinct from capture-time replay redaction.

## [LEG_07]-[WIRE]

ALL C#-minted `*Wire` decode; the largest leg, sequenced inside by sub-domain (codec → frame → gateway → invoke → contract → fault). Files: four index docs, `.api/.gitkeep`, stubs:
`codec/envelope.md`, `codec/proto.md`, `codec/graph.md`, `codec/oplog.md`, `codec/snapshot.md`, `codec/crdt.md`, `codec/version.md`, `codec/patch.md`, `codec/progress.md`, `codec/credential.md`, `codec/claim.md`, `codec/livewire.md`, `codec/flag.md`, `codec/control.md`, `codec/layout.md`, `codec/bcf.md`, `codec/geo.md` `[R6]`, `codec/bim.md`, `codec/appearance.md`, `frame/artifact.md`, `frame/geometry.md`, `frame/residency.md`, `gateway/command.md`, `gateway/support.md`, `invoke/capability.md`, `invoke/client.md`, `contract/descriptor.md`, `contract/drift.md`, `fault/detail.md`, `fault/quarantine.md`.
Laws: decode INTO kernel/state vocabulary where a domain owner exists; own the decoded shape otherwise; consumers reach decoded values through the `#vocab` exports subpath or ports declared at the shared vocabulary owner — the machinery interior is unresolvable. The availability gate in `gateway/command.md` is a port typed against `state/evidence/availability.md`. `frame` delegates the kernel mint (invariant 2). `contract` owns the `FileDescriptorSet` drift gate. `fault/detail.md` is wire-only (invariant 6), owning the closed `HopReason` hop vocabulary and the `fromConnect` total reconstruction fold. `invoke/client.md` carries the protocol axis (connect | grpc-web) with retryable-wire schedules; `invoke/capability.md` documents the C# `SdkTarget.TypeScript` generated emit. The QuantityFamily SI-scalar decode transits `codec/proto.md` into the kernel `Quantity` (invariant 4). `[R9]` `[R10]` ride the codec pages that consume them.

## [LEG_08]-[WORK]

Durable execution only — AI lifted out. Files: four index docs, `.api/.gitkeep`, stubs:
`engine/entity.md`, `engine/storage.md` `[R5]`, `flow/durable.md`, `flow/activity.md`, `queue/job.md`, `queue/schedule.md`, `deliver/webhook.md`, `deliver/mail.md`, `deliver/report.md`, `deliver/relay.md`.
Laws: cluster Entities + `DurableQueue` + job families + `ClusterCron`; `work` composes the `@effect/sql` core `SqlClient` Tag and the `@effect/cluster` `MessageStorage` Tag — the app root provides the store-owned driver Layer (never a `work → store` import); pgmq stays a `store/capability` row (work owns execution semantics, store owns queue-as-data); job families carry priority + dedupe/batch keys + DLQ/replay rows and schedules carry misfire/window policy rows; `deliver/relay.md` owns the outbox drain (SKIP LOCKED + LISTEN/NOTIFY through the `SqlClient` port) feeding the channel rows under one fan-out policy row, per-tenant egress quota + DeliverAt; per-tenant fenced-quota rows live on `engine/entity.md` and `edge/hook` types against them via port; cluster runner entrypoint Layers are selected via `host/exec` runtime rows at the app root — `work` never imports platform-node/bun; `deliver` signs egress via `security/sign` and carries mail (`nodemailer` with locale-keyed template rows — message catalogs arrive as app-passed values, never a `ui` import — plus suppression/unsubscribe rows) and report/document egress rows (`exceljs`, `jspdf`, `jszip`, `papaparse`); `SloBudget` content re-homes to `telemetry`; `FaultDetail` imports in node rails are the named discard; cluster's `K8sHttpClient` is runner discovery, not provisioning.

## [LEG_09]-[STORE]

No migrations, by construction. Files: four index docs, `.api/.gitkeep`, stubs:
`journal/append.md`, `journal/outbox.md`, `journal/snapshot.md`, `journal/upcast.md`, `journal/retain.md`, `project/inline.md`, `project/async.md`, `project/rebuild.md`, `capability/row.md`, `capability/matrix.md` `[R11]`, `scope/handle.md`, `scope/tenant.md`, `lane/sqlite.md`, `lane/wasm.md` `[R13]`, `retrieve/hybrid.md`, `retrieve/index.md`, `object/key.md`, `object/presign.md`.
Laws: one append surface — streams keyed `(appKey, tenantId, aggregate)`, events as closed `Schema.TaggedClass` families with `eventVersion` (app-authored; never wire-minted), OCC append by expected version, the idempotency ledger (`ON CONFLICT DO UPDATE RETURNING (xmax = 0)` claim), transactional outbox atomic with the append. `SqlEventJournal`/`SqlEventLogServer` adopt-or-own `[R4]`; `EventLog` is the local-first sync OVERLAY only `[R19]`. Schema evolution is read-time upcasting (per-type `eventVersion` folds + `snapshot_schema_version`-keyed snapshot upcasters, totality proven through the `tests/typescript/_testkit` law combinators); the raw log is never rewritten. All DDL is idempotent declarative ensure, additive-only, with the split as law: `iac` applies at provision time, `store` VERIFIES at startup, runtime never mutates schema; `PgMigrator` is banned branch-wide. Read side: inline lanes (same-transaction read-your-writes) and async lanes (checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED). Capability rows: one closed vocabulary `{extension, floor, probeSql, capabilities, layer}` with fail-closed probes — pgvector (VectorChord as stronger drop-in row), pg_search, timescaledb, pg_partman, pgmq, pg_cron, pg_ivm, pg_duckdb, pg_graphql, pg_jsonschema, pgaudit, postgis, pg_uuidv7, h3 `[R11]`, pg_trgm/fuzzystrmatch + LISTEN/NOTIFY channelization + advisory-lock claim families + COPY bulk lanes (in-core); extensions are deployment-image facts the `iac/kube` CNPG image satisfies, never JS deps. Tenancy: `StoreHandle` — a Layer family keyed `(appKey, tenancy policy)` resolving to schema-per-app or database-per-app + the RLS `app.current_tenant` GUC row set in-transaction, per-tenant Layers cached via `LayerMap`; isolation is a scope value, never a fork. Sqlite lanes carry the same journal/projection contracts with an explicit capability-degradation table. `retrieve` (hybrid RRF over the FTS | trigram | phonetic | fuzzy | semantic lane roster, embedding-fingerprint keys per vector row, a rerank row, facet/snippet/keyset-cursor query families; `Embedder` port satisfied by `ai/embed`) and `object` (`ObjectKey` = kernel `ContentKey`, presign, codec fan-out with `sharp` derivatives, conditional-put content-address idempotency — If-None-Match, 412 = idempotent noop) complete breadth-in-kind. The host-free algebra stays in `state`; `store` publishes `runtime:neutral` subpaths for its vocabulary with drivers banned on them.

## [LEG_10]-[AI]

The intelligence rail as a peer folder. Files: four index docs, `.api/.gitkeep`, stubs:
`model/provider.md`, `model/token.md`, `embed/embedder.md`, `embed/chunk.md`, `tool/toolkit.md`, `tool/mcp.md`, `agent/actor.md`, `agent/memory.md`.
Laws: `LanguageModel`/`EmbeddingModel` provider rows (anthropic, openai, google, bedrock, openrouter) with the capability-asymmetry table replacing provider-uniformity assumptions; `model/token.md` owns tokenizer budgets (`AnthropicTokenizer` — the pruned standalone tokenizer's replacement); `embed` satisfies the `store/retrieve` `Embedder` port at app composition; `tool` defines Schema-typed toolkits as data; `agent` builds durable agents over `work` entities; `tool/mcp.md` owns MCP hosting on the native `@effect/ai` `McpServer`/`McpSchema` — app toolkits projected as MCP tools, selected at the app root — while `@modelcontextprotocol/sdk` carries only the MCP-client lane (consuming external servers); `model/provider.md` carries cost/latency tier-routing policy rows and guardrail rows (input/output moderation folds + Schema-refusal admission — every provider row passes the same gate); `model/token.md` carries context-assembly rows over app-passed `store/retrieve` results — retrieval results arrive as values, never a `store` import.

## [LEG_11]-[EDGE]

The one public front door. Files: four index docs, `.api/.gitkeep`, stubs:
`api/group.md`, `api/middleware.md`, `api/serve.md`, `api/emit.md`, `problem/detail.md`, `problem/policy.md`, `live/socket.md`, `live/presence.md`, `hook/verify.md`, `hook/admit.md`, `cli/verb.md`, `cli/render.md`.
Laws: domain folders contribute `HttpApiGroup` families as data; the APP assembles exactly one `HttpApi` from selected groups and serves it through one serve row (`NodeHttpServer` | `BunHttpServer` | `toWebHandler` fetch row, plus the SPA/static-asset row with immutable-asset cache headers) — the god-contract is structurally impossible because the api VALUE exists only in the app. Middleware: auth + API-key admission (`security`), W3C trace continuation (`telemetry`), rate/quota + load-shed (concurrency caps, queue-depth 503/Retry-After), CORS + security-header, idempotency-key admission, locale negotiation (Accept-Language → the kernel `Locale` FiberRef row), FiberRef request/tenant context rows. The assembled api VALUE is the single derivation source: `api/emit.md` derives OpenApi document emission, the `HttpApiScalar` reference surface, and the `HttpApiClient` typed SDK from it, with version-prefix + pagination conventions as vocabulary rows on `api/group.md`; `RpcGroup`/`RpcServer` rows (http | websocket | worker) are the second contribution family under the same assembly law. Entry families generalize under ONE assembly law: HTTP (`api`), terminal (`cli/verb` contribution families — the APP assembles exactly one `Command` root from selected verb families the way it assembles the `HttpApi`; doctor/replay/inspect ship as the lib ops family — executing over `host/exec`), MCP (hosting in `ai/mcp`, selected at the app root). `live` serves WS/SSE over `state` Subscribables with SSE resume-token rows and carries the protocol-handler mount port — an HttpApp port Tag the app root satisfies; the `store` EventLog sync server (the local-first archetype's server half) is the standing example. `hook` owns inbound webhook admission (signature verify via `security/sign`, replay protection); admitted hooks enqueue through a declared ingress port Tag the app root satisfies with a `work` queue or `store` journal Layer, and quota admission types against the `work` fenced-quota rows via port; durable signed EGRESS is `work/deliver`. `cli/render.md` composes Doc rows through `@effect/printer`(-ansi). `problem` is the outbound-only fault altitude (RFC 9457 mapping + status/redaction policy rows).

## [LEG_12]-[BROWSER]

Browser runtime, peer of `ui`. Files: four index docs, `.api/.gitkeep`, stubs:
`boot/runtime.md`, `boot/connect.md`, `shell/worker.md`, `shell/install.md`, `persist/kv.md`, `persist/opfs.md` `[R13]` `[R19]`, `transport/pool.md`, `transport/fetch.md`, `session/ceremony.md`, `session/store.md`, `route/navigate.md`, `route/guard.md`.
Laws: `boot` ships the single `BrowserRuntime.runMain` law (a second boot is the named defect) and the `AppSpec` budget VALUE apps construct — the closed five-feed budget (`wire.WireClients`, `wire.CommandGateway`, `store.SnapshotFeed`, `store.RuntimeFeed`, `store.EvidenceFeed`) is Rasm-product guidance, not universal lib law. `shell` owns workbox + background-sync replay + PWA build rows. `persist` owns `idb-keyval`, the OPFS sqlite-wasm lane, and the `EventLog` local-first client (overlay law). `transport` owns the decode worker pool (frame reassembly + content-key verify off-thread; the delegating mint site) and fetch/stream rows with backpressure. `session` composes `security`'s runtime-neutral ceremony subpaths — node-only crypto stays out of browser bundles by tag law. `route` owns Navigation-API routing: a Schema-typed route table + traversal folds (`navigate.md` — the R17 nuqs composition site) and admission/confirm guard folds over `security`/`host` verdicts (`guard.md`); the zero-routing-package law stands. Dangling `decode.worker.ts` references die; the private `visibilitychange` ingresses die into `boot/connect.md`. The render posture is law: client-rendered PWA + build-time prerender rows (per-route static HTML emitted at app build; hydration is `boot`'s law) own the SEO surface; a streaming-SSR react server runtime is the named non-goal — the `react*` scope tags keep it structurally unreachable from `edge`.

## [LEG_13]-[UI]

Component capability; two Nx projects (`ui` core, `ui/viewer`). Files: four index docs, `.api/.gitkeep`, stubs:
`atom/binding.md`, `atom/derive.md`, `token/theme.md`, `token/scale.md`, `act/transition.md` `[R16]`, `act/gesture.md`, `view/primitive.md`, `view/compose.md`, `intl/message.md`, `intl/format.md`, `viewer/scene/glb.md` `[R23]`, `viewer/scene/appearance.md`, `viewer/geo/layers.md`, `viewer/geo/project.md`, `viewer/mark/bcf.md`, `viewer/mark/selection.md`, `viewer/probe/receipt.md`, `viewer/probe/benchmark.md`, `viewer/panel/binding.md`, `viewer/panel/control.md`, `viewer/panel/layout.md`.
Laws: React 19.2.7 spine, react-aria headless primitives, react-compiler enabled; `@effect-atom` is the one state binding (`ONE_FOLD_ONE_BINDING` re-homes to `atom/binding.md`); `<ViewTransition>`/`<Activity>` stay gated with `act` degrading to the native View Transitions API; `ui` declares port records where a component needs a runtime capability (`GlbViewport` decode-worker residency) and `browser` provides the Layer at app composition; `ui` types decoded wire values through `wire#vocab` only. `intl` owns the localization plane: Schema-typed message-catalog rows with plural/select folds over native `Intl`, format rows composing react-aria `I18nProvider`/`useLocale` — zero i18n package; catalogs are app data keyed by the kernel `Locale` brand. `viewer` carries the spatial tier (`three`, `maplibre-gl`, `@deck.gl/*`, geoarrow/arrow, `@google/model-viewer`, `@lume/kiwi`, `@turf/turf`) compile-time excluded from non-spatial apps by project tag. The `glb.md` code-keyed `HopFault` construction is the named discard.

## [LEG_14]-[IAC]

The deploy plane; zero authored YAML anywhere. Files: four index docs, `.api/.gitkeep`, stubs:
`program/automation.md`, `program/spec.md`, `stack/component.md`, `stack/output.md`, `provider/dispatch.md`, `provider/surface.md`, `kube/workload.md`, `kube/data.md` `[R15]`, `kube/traffic.md`, `secret/doppler.md`, `secret/inject.md`, `observe/stack.md`, `observe/apply.md`, `policy/guard.md`, `policy/drift.md`.
Laws: Automation-API inline programs (`LocalWorkspace.createOrSelectStack`; no `Pulumi.yaml`; the pulumi CLI-binary-on-PATH deploy-host fact wrapped once in `program`); `program` emits typed run receipts (up | preview | refresh | destroy ledger). `stack` ships ComponentResource tiers and typed StackOutputs; StackOutputs → `ShardingConfig` is the SOLE meeting seam of the iac/work altitudes. `provider` is one closed `Match.exhaustive` dispatch — `selfhosted-k8s` (first-class: typed `@pulumi/kubernetes` workloads, CNPG operator row provisioning the PG18.4-extension image realizing `store/capability` with scheduled-backup + PITR rows to the object-store row, MinIO-vs-Garage object row, cert/dns/ingress rows, the cluster-bootstrap row — `@pulumi/command` over owned metal/VPS), `selfhosted-docker`, and `aws`/`gcp`/`cloudflare` prepared rows with the service-equivalence map. An app finalizes a cloud row by supplying a `StackSpec` VALUE (target arm, capability profile, region/domain, Doppler project ref); adding a provider is one new dispatch arm in the lib; finalizing one is app data. Secrets: Doppler canonical via `@pulumiverse/doppler`; runtime injection `doppler run` + the `security/secret` read path; ESC demoted to a prepared row. Observability: LGTM + the OTel collector row through `helm.v4` typed values (upstream charts as typed objects); `telemetry/board` outputs applied through `@pulumiverse/grafana`. `policy` owns CrossGuard packs and the drift `previewRefresh` fold over `OpType`. The old `bootstrap.sh` shell entry is the named discard.

## [LEG_15]-[BRANCH_DOC_REBUILD]

Rewrite `libs/typescript/.planning/{README,ARCHITECTURE,IDEAS,TASKLOG}.md` to the thirteen-folder codemap: README routes; ARCHITECTURE carries the edge ledger, tag law, and seam surface; IDEAS/TASKLOG land as empty placeholders while the standing branch cards close into owning-folder law, never surviving as cards — `ONE_FOLD_ONE_BINDING`→`ui/atom`, `CONTENT_ADDRESSED_OFF_THREAD`→`kernel`+`browser`, `HONEST_CLOCK_UNCERTAINTY`→`state` (vocabulary `kernel/clock/uncertainty.md`), `CLOSED_FAMILY_LINT_FENCE`→branch law, `PUBLIC_EDGE_INGRESS`→closed by `edge`, `NATIVE_TRANSITION_PAIR`→`ui/act`. The `WIRE_*`/`PROMOTE_*` TASKLOG twins close with their IDEAS counterparts (`WIRE_FOLD_BINDING_READ_SEAM`, `WIRE_CONTENT_ADDRESSED_OFF_THREAD`, `WIRE_NATIVE_ROUTE_TRANSITION_PER`, `WIRE_CAUSAL_ORDERING_READ_OFF`, `PROMOTE_CLOSED_FAMILY_UNION_LINT`); the tooling/catalogue cards close into legs — `WIRE_MUTATION_GATE_DESCRIPTOR_CODEGEN` + `AUTHOR_PER_PACKAGE_MANIFESTS_SUBPATH` + `MATERIALIZE_MECHANICAL_STRATUM_IMPORT_GUARDS`→Leg 16 (tooling wiring, `@rasm/ts` subpaths, generated boundaries), `EXTEND_BRANCH_ORDERED_COLLECTION_STREAM`→this leg's `.api` substrate catalogue (`effect.md` ordered-collection/`Subscribable` members feeding `state`), `EXTRACT_EVENT_STREAM_CATALOGUES_RESEARCH`→the owning folder `.api` tiers (`iac` automation events with Leg 14, `wire` rfc6902 with Leg 7), `REVERSE_HTTPAPI_OUT_SCOPE_NOTE`→the `edge` folder `.api` tier with Leg 11. Stand up `libs/typescript/.api/` as the substrate catalogue (the 8 substrate packages) — folder tiers landed with their legs, never duplicated.

## [LEG_16]-[CATALOG_REBUILD]

Rewrite `pnpm-workspace.yaml` per the [CATALOG] verdict table: owner-group labels (one-line comments per group), all BUMP/ADD/FIX rows applied exact, the seven DROP rows removed, `packages:` glob extended to the thirteen folder projects (+ the `ui/viewer` project), `catalogMode: strict`, `saveExact: true`, `overrides`/`peerDependencyRules` retained, `allowBuilds` retained minus rows whose package is absent from the rewritten catalog. Nx stand-up in the same leg: `nx.json` + per-folder `project.json` carrying the tag triples as graph metadata; the `@rasm/ts` exports-subpath package identity with per-runtime subpaths for the spanning folders. Boundary enforcement is the exports map (physical) plus the `tests/typescript/_architecture` ledger audit; no lint boundary layer exists.

## [LEG_17]-[CROSS_LIBS_RECONCILE]

Applied in order:
1. `libs/.planning/planning-targets.md` L16-21 — replace the five-folder `[TYPESCRIPT]` block's `Planning Folders:` row with the thirteen-folder roster (applied).
2. `libs/.planning/architecture.md` L75 — rewrite the role sentence (`TypeScript is the host-free web/edge platform: wire-interchange, the browser SPA + UI, and durable node services. It consumes the C# wire only and owns no geometry.`) to the first-class platform charter: thirteen-folder substrate, wire decode a boundary concern inside `wire`, geometry law unchanged. L56-57 and L84 verified still true under the new roster — no edit.
3. The [SEAM_MAP] ARCHITECTURE rows — mechanical token replacements at BR:29-34, K:47, AH:52-83, CO:67-110, PE:43-47, AU:59-83, BM:63-99, EL:68, EL IDEAS:40, MA:65.
4. The [SEAM_MAP] residual rows — pipeline.md:548, controls.md:204, solver.md:216, codecs.md:760, reconciliation.md:119, Element ARCHITECTURE.md:67, Element Graph/wire.md:3/18/1058, AppHost IDEAS.md:23/39. Then the closing sweep: `rg "typescript:|libs/typescript" libs/csharp` must return only tokens matching the new roster.
5. Python: zero edits (verified — no Python doc names a TS seam; none is invented).
