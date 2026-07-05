# [BRANCH_TIER_CENSUS] — libs/typescript

Read-only census of the branch-tier surface: `libs/typescript/.api/`, `libs/typescript/.planning/`, `libs/typescript/package.json`, all `project.json` files, and the TS catalog rows of `pnpm-workspace.yaml`. Folder-local `.api`/`.planning` (core, security, data, runtime, ui, iac) are out of scope for this dossier; only their `project.json` files and the branch router's references to them are covered.

## [01]-[FILE_REGISTER]

| File | Owned capability | Entry surfaces | Fence mass |
| --- | --- | --- | --- |
| `.planning/README.md` | Branch router: six-domain wave order, substrate package roster, `.api` catalogue law | Router links to per-folder `README.md`; no code fences | Router — 21 LOC, prose only |
| `.planning/ARCHITECTURE.md` | Branch package map, cross-language seam table (C#↔TS wire rows), permitted-edge dependency table (W0-W4), port-satisfaction law | `codemap`/`seams` text blocks + one edge table | Structural — 46 LOC, no code fences (declarative tables/diagrams only) |
| `.planning/dataflow-system.md` | The one data spine: content identity (`XxHash128`/`ContentKey`), interchange plane (wire decode-once law), journal spine, HLC time/order, tenancy/scope, cross-language invariants, extension-recipe table | Prose + one extension-recipe table; zero code fences — spine law lives here, fences live in folder pages | Spine doctrine — 42 LOC |
| `.planning/IDEAS.md` | Cross-package idea concert template (empty) | Card template only | Empty — 19 LOC, `(none)` |
| `.planning/TASKLOG.md` | Cross-package task concert template (empty) | Card template only | Empty — 20 LOC, `(none)` |
| `package.json` | Branch npm package `@rasm/ts`, per-domain subpath exports with `server`/`browser`/`wasm`/`default` conditions for `core`/`security`/`data`/`runtime`; single-condition exports for `ui`, `ui/viewer`, `iac` | `exports` map only | Manifest — no fences |
| `core/project.json` | Nx project registration: `scope:core`, `runtime:neutral`, `plane:runtime` | `sourceRoot: core/src` (does not exist on disk) | Manifest |
| `security/project.json` | Nx registration: `scope:security`, `runtime:neutral`, `plane:runtime` | `sourceRoot: security/src` (does not exist) | Manifest |
| `data/project.json` | Nx registration: `scope:data`, `runtime:neutral`, `plane:runtime` | `sourceRoot: data/src` (does not exist) | Manifest |
| `runtime/project.json` | Nx registration: `scope:runtime`, `runtime:neutral`, `plane:runtime` | `sourceRoot: runtime/src` (does not exist) | Manifest |
| `ui/project.json` | Nx registration: `scope:ui`, `runtime:browser`, `plane:runtime` | `sourceRoot: ui/src` (does not exist) | Manifest |
| `iac/project.json` | Nx registration: `scope:iac`, `runtime:node`, `plane:deploy` | `sourceRoot: iac/src` (does not exist) | Manifest |
| `ui/viewer/project.json` | Nx registration for nested project: `scope:viewer`, `runtime:browser`, `plane:runtime` | `sourceRoot: ui/viewer/src` (does not exist) | Manifest |

All seven `project.json` files declare a `sourceRoot` under a `src/` directory that is absent on disk — the branch is planning-only, zero implementation source exists under any of the six folders or the viewer sub-project.

## [02]-[API_ROSTER]

Eight substrate catalogs at `.api/`, each documenting exactly one package per the branch's one-catalog-per-package law (`.planning/README.md` [03]):

| Catalog | Package documented | LOC | Depth signal |
| --- | --- | --- | --- |
| `effect.md` | `effect` 3.21.4 | 161 | Deepest — full carrier/Schema/Layer/Match/Stream/STM/concurrency/observability/collection/caching surface; public-type tables per rail |
| `effect-platform.md` | `@effect/platform` 0.96.2 | 130 | Full declarative HTTP-API family, client/server/router, system-API Tag contracts, frame codecs, config/logger boundary |
| `effect-experimental.md` | `@effect/experimental` 0.60.0 | 124 | Full overlay-lane roster (EventLog, Machine, Persistence family, Reactivity, Sse, RateLimiter, RequestResolver, DevTools) with explicit KEEP `[R19]` catalog-verdict and record-of-truth boundary law |
| `effect-opentelemetry.md` | `@effect/opentelemetry` 0.63.0 | 94 | Native OTLP lane + SDK-bridge lane fully separated; `[R3]` collapse-target noted for SDK machinery retirement |
| `effect-platform-bun.md` | `@effect/platform-bun` 0.90.0 | 91 | Full module roster (Runtime/Context/HttpServer/CommandExecutor/Worker/KeyValueStore/Socket/Cluster runners); `ADD [R1]` verdict gated on Leg-16 install proof |
| `effect-platform-node.md` | `@effect/platform-node` 0.107.0 | 79 | Full module roster with undici/ws internals, hard-peer table (cluster/rpc/sql required at install) |
| `ssh2.md` | `ssh2` (unpinned in catalog line, `1.17.0` in workspace catalog) | 75 | Full connection/exec/shell/SFTP/port-forward/jump-host surface; explicit boundary-kernel adaptation law (`Effect.acquireRelease`, `NodeStream.fromDuplex`, `Stream.asyncPush`) |
| `effect-platform-browser.md` | `@effect/platform-browser` 0.76.0 | 69 | Full Web-API binding roster (BrowserRuntime/KeyValueStore/Worker/HttpClient/Socket/Stream/Clipboard/Geolocation/Permissions); explicit `TIER_SPLIT` naming which native-DOM ingress is NOT wrapped here (owned by `runtime/browser/*` folder pages instead) |

No catalog exists for `@effect/vitest` (declared a branch substrate in `.planning/README.md` [02] but its catalog sits at `tests/typescript/.api/effect-vitest.md`, correctly outside this tier per the dev-tool-tier law).

## [03]-[CAPABILITY_MAP]

What the branch tier genuinely owns today versus what the router/architecture docs claim, with every mismatch named:

- **Claimed**: `.planning/README.md` states the branch ships "composable `Layer`/`Service` families" as "the lib-grade substrate for building hundreds of complex apps." **Actual**: zero `src/` exists in any of the six folders or the viewer sub-project — the entire branch is design-doc + manifest scaffolding with no realized code. This is the expected state for a planning-phase census, not a defect, but any downstream phase reading "substrate" language must not infer implemented capability.
- **Claimed**: `.planning/README.md` [02] lists six branch substrate packages (`effect`, `@effect/platform`, `@effect/experimental`, `@effect/opentelemetry`, `ssh2`, `@effect/vitest`). **Actual**: only five have a `.api/` catalog at the branch tier; `@effect/vitest` catalogs correctly under `tests/typescript/.api/` — not a mismatch, but the router's own list mixes a dev-tier package into a substrate-tier enumeration without flagging the tier split (the `ssh2.md`/`effect-platform-browser.md` catalogs use an explicit `[TIER_SPLIT]`/boundary-law callout; the README's package list does not carry the same precision).
- **Claimed**: `.api/effect-platform-bun.md` and the `platform-node`/`platform-browser` catalogs describe the branch's dual-runtime (`node`/`bun`) execution law as already operative (`ADD [R1]` gated on "Leg-16 install proof"). **Actual**: no lockfile-verified install proof artifact exists in this tier's scope to corroborate the gate has been satisfied; the catalog itself flags this as a precondition, not a settled fact — future phases should not treat `platform-bun` rows as load-bearing until that proof is located.
- **Claimed**: `ARCHITECTURE.md` [02] enumerates nine `SEAMS` rows, all C#-endpoint (`csharp:Rasm*`) wire dependencies landing into `core`/`runtime`/`ui`. **Actual**: consistent with the dataflow spine's "C# owns every `*Wire` shape, TS decodes verbatim" law; no seam claims a TS-authored wire family, so no internal contradiction found in this tier.
- **Claimed**: `ARCHITECTURE.md` [03] permitted-edge table asserts strict downward dependency (`core → security → data → runtime → ui/iac`) with named port-satisfaction exceptions (security ports satisfied by data Layers at composition, data's `Embedder`/`Reranker` satisfied by runtime ai). **Actual**: cannot be verified against real imports because no source exists yet; the law is internally consistent as written and will be the binding constraint the next census phase (folder-level) must check against actual `src/` once authored.
- **Mismatch, naming precision**: `.planning/README.md` calls `dataflow-system.md` a "branch-level page beside this router," but `ARCHITECTURE.md` [00] preamble also calls it "the data spine," and both correctly avoid claiming it carries code fences — confirmed: `dataflow-system.md` has zero code fences, purely doctrine/table content, consistent with its stated role as spine law rather than executable design.
- **project.json plane tags**: `core`/`security`/`data`/`runtime` all declare `plane:runtime` and `runtime:neutral`; `ui`/`ui/viewer` declare `plane:runtime`/`runtime:browser`; `iac` declares `plane:deploy`/`runtime:node`. This matches `ARCHITECTURE.md`'s W0-W4 wave labels and the "iac ... deploy plane" distinction — no contradiction between the Nx tags and the architecture doc.
