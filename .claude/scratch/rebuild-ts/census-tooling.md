# [CENSUS_TOOLING] TypeScript tooling estate

Read-only truthful census. All paths repo-root-relative.

## [A]-[FILE_REGISTER]

### Root tooling configs

| File | Owns | Entry surfaces | Mass |
| --- | --- | --- | --- |
| `tsconfig.json` | Root TS project anchor for config files only (`playwright.config.ts`, `vite.config.ts`, `vite.factory.ts`, `vitest.config.ts`); `references: []` | extends `tsconfig.base.json` | trivial (5 lines) |
| `tsconfig.base.json` | Global compiler-option law: `isolatedDeclarations`, `verbatimModuleSyntax`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `moduleResolution: bundler`, `customConditions: [types, import]`, `stableTypeOrdering`, target `esnext`, no per-project `include`/`references` | consumed by every downstream `tsconfig.json` via `extends` | small, dense |
| `biome.json` | Sole lint+format authority (schema 2.5.2): 4-space/150-col formatter, single quotes, trailing commas, `organizeImports` assist, recommended lint preset with two rule overrides (`useLiteralKeys` off, `noShadowRestrictedNames` off), broad ignore globs (`.claude`, `.nx`, `.venv`, `_tmp`, lockfiles) | whole-repo (not TS-only — ignore list covers csharp/python build dirs too) | small |
| `nx.json` | Nx workspace: `workspaceLayout.libsDir = libs/typescript`, `defaultBase: main`, target defaults for `build`/`typecheck`/`test`/`lint`/`e2e` wired to `.artifacts/typescript/*`, three plugins (`@nx/js/typescript` on `tsgo`, `@nx/vite/plugin`, `@nx/playwright/plugin`) | project-graph inference over every `project.json` | small |
| `vite.config.ts` | Root Vite anchor — executes `vite.factory.ts`'s `createConfig` in `library` mode named `WorkspaceFoundation`, strips the `build` block so root emits no artifact | `default` config export; consumed only by tooling that resolves cwd-adjacent Vite config, not by app/package builds | trivial |
| `vite.factory.ts` | The one parameterized Vite config factory: `Cfg` union (`app`/`library`/`server` modes) decoded via `effect/Schema`, per-mode plugin/resolve/build tables, manual-chunk heuristic, CSP/PWA/compression/image-optimizer wiring for `app` mode | exports `{ B, createConfig }`; every app/package config imports `createConfig` | 425 LOC, dense (the real config surface) |
| `vitest.config.ts` | Root Vitest runner authority: coverage (`v8`, 95% all-four thresholds, disabled by default), artifact routing to `.artifacts/typescript/{coverage,test-results,bench}`, `include`/`exclude` glob law (`tests/typescript/**` + `libs/typescript/**` colocated specs), fake-timers, reporters split CI-vs-local, `setupFiles: [tests/typescript/_testkit/src/setup.ts]` | `default` config export | 134 LOC; header comment records the estate is **currently empty** — every former inline project matched zero files and was deleted |
| `playwright.config.ts` | E2E runner authority, deliberately root-resident (playwright has no upward config search) to bound bare `playwright test` invocations: three lanes (`chromium`, `viewer` with swiftshader GPU flags, `webkit`), `testDir: tests/typescript/e2e`, `testMatch: **/*.pw.ts`, goldens keyed `{projectName}/{platform}` | `default` config export | 87 LOC |
| `stryker-config.json` | **Misnamed relative to its content** — this is the C#/StrykerNET config (`test-runner: mtp`, `solution: Workspace.slnx`, output `.artifacts/csharp/stryker`), not a TypeScript file at all | consumed by `dotnet stryker` via `--config-file`/discovery, not by any TS tool | trivial |
| `stryker.config.json` | The real TS mutation config (StrykerJS schema): `testRunner: vitest`, `mutate: libs/typescript/**/src/**/*.{ts,tsx}` excluding specs/`.d.ts`, concurrency 4, incremental, reports to `.artifacts/typescript/stryker` | consumed by `npx stryker run` (root-resident by the same self-defense rationale as Playwright, per `tests/typescript/README.md` [09]) | trivial |
| `package.json` (root) | Workspace root manifest: `name`, `version`, `private`, `type: module`, `packageManager: pnpm@11.9.0`, `engines.node >=24`, `browserslist`, and the full `devDependencies` block (every `catalog:`-pinned dev tool: Nx suite, Biome, Stryker triad, Vite/Vitest/Playwright, `@ast-grep/cli`, `@mermaid-js/mermaid-cli`, `@octokit/rest`) | **no `scripts` key and no `workspaces` key** — pnpm workspace membership comes from `pnpm-workspace.yaml` alone | large devDependencies list, zero scripts |
| `pnpm-workspace.yaml` | Sole package-manager topology + version owner: `packages: [libs/typescript, tests/typescript/*]`, then one long `catalog:` block (442 lines) grouping every pinned TS package by domain comment (substrate/core/security/data/…) | resolved by `catalog:` references in every `package.json` in the tree | 442 lines, dense manifest |
| `libs/typescript/package.json` | The `@rasm/ts` package: `exports` map with per-folder subpaths, four folders (`core`, `security`, `data`, `runtime`) carrying `server`/`browser`/`wasm`/`default` conditions, two folders (`ui`, `iac`) carrying a bare `default`-only export, plus `./ui/viewer` as its own subpath | consumed by any package importing `@rasm/ts/<folder>` | 34 lines |

### `libs/typescript/<folder>/project.json` (Nx project descriptors, one per folder + one for `ui/viewer`)

All seven files are near-identical skeletons: `$schema`, `name`, `projectType: library`, `sourceRoot: libs/typescript/<folder>/src`, `tags: [scope:<folder>, runtime:<neutral|node|browser>, plane:<runtime|deploy>]`. No `targets` block in any of them — target inference comes entirely from the three Nx plugins in `nx.json`. `runtime:` tag values observed: `neutral` (core, data, runtime, security), `node` (iac), `browser` (ui, viewer). `plane:` values: `runtime` (five folders + viewer) vs `deploy` (iac alone). None of the seven `sourceRoot` paths exist on disk yet — no folder has a `src/` directory.

### `libs/typescript/<folder>/{ARCHITECTURE,README,IDEAS,TASKLOG}.md` (design-page skeleton, per folder)

Each of the six folders (`core`, `data`, `iac`, `runtime`, `security`, `ui`) carries the same four-file skeleton plus a `.planning/` design-page tree and a `.api/` catalog tree (see [B]). `README.md` is the folder router: one narrative paragraph, `[01]-[ROUTER]` linking every `.planning/` page in build order, `[02]-[DOMAIN_PACKAGES]` (folder-local externals grouped by concept-tagged bracket labels), `[03]-[SUBSTRATE_PACKAGES]` (shared substrate this folder draws from, e.g. `effect`, `@effect/platform*`). `ARCHITECTURE.md` is the folder domain map: a `text codemap` fence naming eventual `src/` files with one-line owns-comments, plus `[02]-[SEAMS]` rows mirroring the branch-level cross-language seam table. `IDEAS.md`/`TASKLOG.md` are the deferred-idea and open-work ledgers (~19-20 lines each, near-boilerplate length across all six folders — these read as freshly reset, not yet folder-specific).

`libs/typescript/ui/viewer/project.json` is the only file under `ui/viewer/` — no README/ARCHITECTURE/IDEAS/TASKLOG/`.planning` exist for the viewer sub-project; its design pages (`viewer/scene.md`, `viewer/geo.md`, `viewer/mark.md`, `viewer/panel.md`, `viewer/probe.md`) live under the parent `ui/.planning/viewer/` instead, per the `ui` README's `[01]-[ROUTER]` — a naming mismatch between the Nx project split (`ui` vs `viewer` as two projects) and the planning-page location (both under one `ui/.planning`).

### `libs/typescript/.planning/` (branch-level, outside any folder)

`README.md` (branch router: six folders in wave order 0-4, substrate-package registry, `.api` catalogue law), `ARCHITECTURE.md` (branch package map codemap, cross-language seam table, the five-wave `[FROM]/[MAY_IMPORT]` dependency-direction table — this exact table is what `tests/typescript/_architecture/src/boundaries.spec.ts` parses live per the testing README), `dataflow-system.md` (branch data-spine design page: content identity, interchange plane, journal spine, time/order, tenancy, cross-language invariants — 42 LOC per `loc`, i.e. a stub relative to its stated scope), `IDEAS.md`, `TASKLOG.md`.

### `tests/typescript/` estate

| Path | Owns | Entry surfaces | Mass |
| --- | --- | --- | --- |
| `tests/typescript/README.md` | Full TS testing-authoring law: topology (colocated unit specs vs. shared kit), kit contract, architecture-suite charter, `.api` dev-tier law, Effect idioms (`it.effect`/`it.layer`/`it.effect.prop`, the one `effect/FastCheck` engine), oracle/generator law, snapshot law, browser-mode vs. e2e split, gate wiring (Nx, Stryker, container manifest, Ryuk), density/banned-shapes | narrative law, no code | dense prose, 61 lines |
| `_testkit/package.json` + `tsconfig.json` | `@rasm/ts-testkit`, a private source-exporting workspace package, exports map with 8 subpaths (`/arbitraries`, `/bench`, `/corpus`, `/e2e`, `/gauges`, `/harness`, `/laws`, `/setup`) — **no barrel** | consumed by every colocated spec via `@rasm/ts-testkit/<owner>` | small |
| `_testkit/src/harness.ts` (+ `harness.spec.ts`) | `Layer`s: `Containers` (pglite fast lane + real-container rows via `testcontainers`, image pins resolved through a typed reader), `PgLanes`, `ObjectStores` (in-process double + real S3 lane with presign), `Loopbacks` (hermetic HTTP capsule) | exports `{ Containers, HarnessFault, Loopback, Loopbacks, ObjectStore, ObjectStores, PgLane, PgLanes, PinsPath }` | 483 LOC — the largest single kit file |
| `_testkit/src/corpus.ts` (+ spec) | Manifest-keyed corpus readers over `tests/contracts/` with typed absence (`Emitted`/`Awaiting`/`Blocked`) + seed-zero `ContentDigest` + `CANONICAL_BYTE_IDENTITY` frozen expectation | reads `tests/contracts/MANIFEST.md` | 207 LOC |
| `_testkit/src/laws.ts` (+ spec) | Witness-mandatory `Law` combinators requiring a refuting foil, tautology-audit on registration | — | 175 LOC |
| `_testkit/src/arbitraries.ts` (+ spec) | Schema-derived arbitraries: field-absence lane (encoded-side optional-key variation), distinct-payload lane | — | 105 LOC |
| `_testkit/src/gauges.ts` (+ spec) | Import-graph and snapshot-hygiene gauge engines | — | 197 LOC |
| `_testkit/src/bench.ts` (+ spec) | Sustained-regression bench gate over the autosaved ledger in `.artifacts/typescript/bench` | — | 189 LOC |
| `_testkit/src/e2e.ts` (+ spec) | The `/e2e` substrate: hermetic route-fulfilled origin, paused-clock control, multi-context cohort, CDP virtual-authenticator ceremony | consumed by `tests/typescript/e2e/fixtures.ts` | 169 LOC |
| `_testkit/src/setup.ts` | Vitest global setup file wired directly into root `vitest.config.ts` | — | 8 LOC (trivial) |
| `_testkit/src/kit.bench.ts` | The kit's own bench entry (falsification suite for the bench gate itself, per the "every kit capability carries the spec that proves it can fail" law) | — | 27 LOC |
| `_architecture/package.json` | `@rasm/ts-architecture`, depends on `@rasm/ts-testkit` via `workspace:*` | — | small |
| `_architecture/src/boundaries.spec.ts` | The edge-ledger import audit: acyclicity, wave direction, runtime/plane tag law, parses the branch `ARCHITECTURE.md` permitted-edge table live — a reshaped or vanished table fails the gauge loudly | — | 343 LOC, largest architecture spec |
| `_architecture/src/admission.spec.ts` | Manifest gauges: every spec-estate pin resolves through `catalog:`/`workspace:` only, refused-property-engine ban, one-`.api`-tier-per-package check | — | 166 LOC |
| `_architecture/src/hygiene.spec.ts` | Estate-wide snapshot-hygiene sweep (stale-golden detection) | — | 25 LOC (thin) |
| `e2e/package.json` + `tsconfig.json` | `@rasm/ts-e2e`, depends on `@rasm/ts-testkit`, `@playwright/test`, `@types/k6` | — | small |
| `e2e/fixtures.ts` | The one fixture tower composing the kit's `/e2e` substrate | — | ~3.6KB |
| `e2e/engine/portability.pw.ts` | Cross-engine (chromium/webkit) portability suite | — | small |
| `e2e/gpu/context.pw.ts` | GPU/WebGPU acquisition suite (the `viewer` lane) | — | small |
| `e2e/load/{breach,probe}.k6.ts` + `live.pw.ts` | k6 load-lane scripts (subprocess boundary, typed input artifacts) + the pw driver | — | small |
| `e2e/platform/*.pw.ts` (7 files) | `blocked`, `caps`, `channel`, `clock`, `har`, `visual`, `webauthn` platform-capability suites, one `replay.har` fixture | — | small each |
| `e2e/goldens/chromium/darwin/` | Committed screenshot/aria goldens tree, keyed per-project/per-platform via `snapshotPathTemplate` | — | binary/golden assets |

**No spec file under `tests/typescript/` targets branch source** — every `.spec.ts` file present is the kit's own falsification suite for its own capability (harness, corpus, laws, arbitraries, gauges, bench, e2e substrate) or an architecture gauge; none exercise `libs/typescript/<folder>/src` because no folder has a `src/` tree yet. This matches the `boundaries.spec.ts` "verdict is `Unsupported`, never a vacuous green" law recorded in the testing README [03].

### `tests/contracts/`

`README.md` (corpus law: C#-sole-producer, round-trip-read-only consumers, manifest-precedes-directory layout, machine-record schema for `MANIFEST.md`), `MANIFEST.md` (9 fixture rows: 2 `REAL`-pinned — `CANONICAL_BYTE_IDENTITY`, `CLASH_GOLDEN` — and 7 `DESIGN-PIN` rows awaiting a producer freeze — `MATERIAL_LAYER_GOLDEN`, `FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, `HLC_TWO_HALF`, `IFC_WIRE`, `DESCRIPTOR_DRIFT`). **No `<seam>/` asset directories exist on disk** — the manifest is the only artifact; every payload is still gated on its named C# producer page pinning the byte-deriving input, consistent with the "manifest never leads the producer" law. Three of the nine rows name TypeScript consumers (`CANONICAL_BYTE_IDENTITY` → `core/value/contentKey`; `FAULT_TRIPLES` → `core/interchange/codec`; `CRDT_OP_SET` → `core/interchange/format`+`core/state/merge`; `GLB_BY_KEY` → `ui/viewer/scene`; `HLC_TWO_HALF` → `core/value/clock`; `IFC_WIRE` → `core/interchange/codec`) — six of nine, not three.

## [B]-[API_ROSTER]

62 `.api/` catalog files across five tiers, each one package per file, versions never repeated in the catalog (they resolve through `pnpm-workspace.yaml` alone). Depth signal = LOC per `loc` above (proxy for member-coverage depth, not complexity — these are flat evidence transcripts).

| Tier | Files | Depth range | Deepest | Shallowest |
| --- | --- | --- | --- | --- |
| `libs/typescript/.api/` (branch substrate) | 8 | 49-161 | `effect.md` (161) | `ssh2.md` (75) |
| `core/.api/` | 11 | 56-136 | `electric-sql-d2ts.md` (136) | `connectrpc-connect-web.md` (56) |
| `data/.api/` | 20 | 31-140 | `effect-sql.md` (140) | `apache-arrow.md` (31) |
| `iac/.api/` | 22 | 37-215 | `pulumi-kubernetes.md`/`pulumi-pulumi.md` (215 each) | `pulumi-synced-folder.md` (37) |
| `runtime/.api/` | 38 | 44-354 | `effect-ai.md` (354) | `opentelemetry-sdk-trace-node.md` (44) |
| `security/.api/` | 9 | 49-102 | `simplewebauthn-server.md` (102) | `node-rs-argon2.md` (49) |
| `ui/.api/` | 59 | 29-126 | `deck.gl-core.md` (126) | `visx-axis.md` (29) |
| `tests/typescript/.api/` (dev-tool tier) | 15 | untabulated by `loc` above (not scanned) | — | — |

Every catalogued package name maps 1:1 to a `pnpm-workspace.yaml` catalog entry — no orphan catalog file was found pointing at an unpinned package in the folders inspected. The `tests/typescript/.api/` tier catalogs the dev/test-only packages (`effect-vitest`, `electric-sql-pglite`, `fast-check`, `happy-dom`, `jsdom`, `playwright-test`, the Stryker triad, `testcontainers`, `types-k6`, `vitest-browser-playwright`, `vitest-coverage-v8`, `vitest-ui`, `vitest`) — correctly disjoint from every runtime `.api/` tier per the one-tier-per-package law.

## [C]-[CAPABILITY_MAP] — claimed vs. real

| Claim (README/ARCHITECTURE) | Real state | Mismatch |
| --- | --- | --- |
| "Six capability domains shipping composable `Layer`/`Service` families" (`libs/typescript/.planning/README.md`) | Zero `src/` directories exist under any of the six folders; every `sourceRoot` in every `project.json` points at a nonexistent path | **Total** — the branch is planning-only; no runtime code has been authored |
| `libs/typescript/package.json` `exports` map wires `./core/src/server.ts`, `./core/src/browser.ts`, `./core/src/wasm.ts`, `./core/src/index.ts` (and equivalents for security/data/runtime) | None of those files exist | The exports map is aspirational — resolving any subpath today throws a module-not-found | Confirmed mismatch |
| `vitest.config.ts` header: "The TS test estate is currently EMPTY: every former inline project matched zero files and was deleted; per-package projects return with the TS buildout" | Accurate self-report — this is the one piece of TS tooling doc that is honest about the gap | Not a mismatch — a rare case of the config itself stating ground truth |
| `_architecture/src/boundaries.spec.ts` parses the branch `ARCHITECTURE.md` permitted-edge table and asserts acyclicity/wave-direction/tag law "against the parsed rows" | Per the testing README [03]: "while `libs/typescript` ships no source the verdict is `Unsupported`, never a vacuous green" | Consistent, not a mismatch — the gauge is honestly gated, not silently passing |
| `ui` README: "`viewer` as a second Nx project carrying the spatial tier" | `ui/viewer/project.json` exists standalone with its own `tags`/`sourceRoot`, but `viewer`'s five `.planning` pages (`scene`, `geo`, `mark`, `panel`, `probe`) live under the **parent** `ui/.planning/viewer/`, and `viewer` has no README/ARCHITECTURE/IDEAS/TASKLOG of its own | Structural split-brain: two Nx projects, one planning-page owner |
| `tests/contracts/README.md`: manifest-first layout, "a seam directory exists only when a real producer emits into it" | Zero seam directories exist; 7 of 9 manifest rows are `DESIGN-PIN`, blocked on C# producer pages that have not yet frozen their byte-deriving input | Consistent with stated law, but means **no cross-language round-trip proof is currently possible** for TS — the `_testkit/src/corpus.ts` readers have nothing real to decode except the two `REAL` rows |
| `stryker-config.json` present at repo root alongside `stryker.config.json` | `stryker-config.json` is the **C#/StrykerNET** config (`solution: Workspace.slnx`, `test-runner: mtp`); `stryker.config.json` is the real TypeScript StrykerJS config | Naming collision, not a duplicate — a reader assuming both files govern the same tool will misconfigure either |
| Root `package.json` implies a conventional workspace root (has `devDependencies`, `engines`, `packageManager`) | No `scripts` key and no `workspaces` key at all — workspace membership is declared solely in `pnpm-workspace.yaml`; every task (`build`/`test`/`lint`/`typecheck`/`e2e`) routes through Nx target inference, never an npm script | Not wrong, but easy to miss — there is no `pnpm run <x>` entry point at the root; the only entry points are `nx <target> <project>` invocations |
| `libs/typescript/.planning/dataflow-system.md` billed as "the data spine: content identity, interchange plane, journal spine, time/order, tenancy, cross-language invariants" | 42 LOC per `loc` — thinner than any single folder-level `.planning` page (compare `core/.planning/interchange/codec.md` at 1035 LOC) | The branch-level spine page is a stub relative to the depth implied by its charter and by the folder pages that will depend on it |
| `libs/csharp` git-log entries reference "ts _tmp/interchange retired by operator" | No `_tmp` directory exists under `libs/typescript` today; `biome.json` still carries a `!!**/_tmp` ignore glob | Stale ignore-glob rule surviving past the artifact it was written to exclude — harmless but dead weight |

| `tests/typescript/README.md` [03]: "The permitted-edge table parses live from its owning page — `libs/typescript/.planning/composition-system.md` is the single source" | **No file named `composition-system.md` exists anywhere in the repo.** `boundaries.spec.ts` line 174 reads the real owning page, `libs/typescript/.planning/ARCHITECTURE.md` | Confirmed stale cross-reference in the testing README — the filename in prose does not match the file the gauge actually parses |

## [D]-[DUPLICATE_RULING]

Two files exist as `stryker*.json` at root: `stryker-config.json` (C#/StrykerNET, wrong estate) and `stryker.config.json` (TypeScript/StrykerJS, correct estate, referenced by `tests/typescript/README.md` [09]). **Ruling: `stryker.config.json` is the one TypeScript-governing file; `stryker-config.json` belongs to the C# `docs/stacks/csharp` estate and is out of scope for this TS census** — it is not a duplicate to collapse, it is a misleading co-location that a downstream phase should either rename (e.g. `stryker.dotnet.json`) or relocate so the root directory does not carry two `stryker*` files governing two different languages under near-identical names.
