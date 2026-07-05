# [STACK_TOOLING] — TS toolchain capability dossier

Live-verified 2026-07-04 against vendor docs/changelogs/context7. Installed pins from `pnpm-workspace.yaml` catalog. Every finding carries the exact config-key spelling the current major expects.

## [00]-[VERSION_RECONCILIATION]

| Tool | Installed pin | Current-stable reality | Verdict |
| --- | --- | --- | --- |
| `typescript` (JS/tsc floor) | `6.0.3` | 6.0 is the final JS-based release (shipped Mar 2026), the tsc floor of the dual-gate | correct; maintained until TS7 stable defaults |
| `@typescript/native-preview` (tsgo floor) | `7.0.0-dev.20260702.3` | TS7.0 **RC** landed 2026-06-18; native now also ships from `typescript@rc` (command `tsc`); GA "within ~a month" of RC (imminent as of 2026-07-04) | nightly still valid; GA migration is a catalog bump, not a config-shape change |
| `@effect/tsgo` | `0.15.0` | wraps tsgo; used as Nx `compiler: "tsgo"` native lane | correct |
| `@biomejs/biome` | `2.5.2` | 2.5 = 500 lint rules, **GritQL plugin code-fix**, cross-file linting; `plugins`/`domains`/`assist` all GA | correct pin; config UNDER-USED (see [01]) |
| `vite` | `8.1.3` | Vite 8 stable (Mar 2026), Rolldown default bundler, Oxc transforms, Lightning CSS minify | correct; one deprecated Rolldown key in factory (see [03]) |
| `vitest` + `@vitest/*` | `4.1.9` | v4 = `workspace`→`projects`, browser mode `instances`+provider, v8 AST-remap coverage, `coverage.all` default true | correct pin; browser lane not wired (see [04]) |
| `@vitest/browser-playwright` | `4.1.9` | the v4 browser provider (`playwright()` factory) | INSTALLED, UNWIRED |
| `@playwright/test` | `1.61.1` | `toMatchAriaSnapshot` first-class, `expect.toMatchAriaSnapshot.pathTemplate`, `--update-source-method`, `webServer.gracefulShutdown`, `toBeInViewport` | correct; aria-golden template missing (see [05]) |
| `@stryker-mutator/*` | `9.6.1` | 9.6 adds `--testFiles`; vitest-runner hitcount fix for v4.1 | correct pin; config UNDER-USED (see [06]) |
| `nx` + `@nx/*` | `23.0.1` | 23.0.1 (2026-06-23); release-config default shifts; `@nx/conformance` = Enterprise-only | correct; well-formed (see [07]) |
| `tailwindcss` + `@tailwindcss/vite` + `lightningcss` | `4.3.2` / `4.3.2` / `1.32.0` | Tailwind 4 vite-plugin, Lightning CSS is Vite 8's default CSS engine | correct; wired correctly |
| `@swc/core` + `@swc-node/register` | `1.15.43` / `1.11.1` | SWC = Nx TS-execution register lane only; React Compiler rides babel, not SWC | correct; no overlap defect |
| `@ast-grep/cli` | `0.44.0` | structural-search lane (assay `code`); orthogonal to Biome GritQL | correct |

Net: pins are current. The gap is **capability the estate installed but never wired** (GritQL plugins, Biome domains, browser-mode lane, Stryker typescript-checker + perTest + ignoreStatic, aria goldens) plus **two concrete config-shape defects** (`rules.preset`, Rolldown `manualChunks`).

## [01]-[BIOME] — the sole lint rail, under-legislated

Doctrine [04]-[RULE_ENFORCEMENT]: Biome owns formatter+linter+assist; every doctrine anti-pattern with no compiler flag is a **GritQL plugin rule promoted to error**. The current `biome.json` ships ZERO plugins and one invalid key — the largest doctrine-vs-reality gap in the estate.

Confirmed config-shape facts (2.5.2):
- Linter rules root is `"rules": { "recommended": true, "<group>": { "<rule>": "error|warn|info|off" } }`. There is **no `preset` key**. The estate's `"rules": { "preset": "recommended" }` is an unknown key — `recommended` is on by default so rules still run, but `preset` is dead config that trips `unknownOptions` diagnostics. **Fix: `"recommended": true`.**
- Plugins: `"plugins": ["./path/rule.grit"]` at config root (string path, or object `{ "path", "includes", "excludes" }`). A `.grit` file declares `language js (typescript, jsx);` then a pattern + `register_diagnostic(span=…, message=…, severity="error")`. GritQL **code-fix** (autofix via `fixer=`) shipped in 2.5 — promoted rules can carry fixes.
- Domains: `"linter": { "domains": { "project": "all|recommended|none", "test": "…", "types": "…", "next": "…" } }`. The `types` domain unlocks type-inference rules (e.g. `noFloatingPromises`) — multi-file/type-aware; it only produces signal once `libs/typescript` ships source, but wiring it now arms it. The `project` domain unlocks import-graph rules (`noUndeclaredDependencies`, cycle detection).
- Assist: `"assist": { "actions": { "source": { "organizeImports": "on", "useSortedKeys": "on" } } }` — 10 assist actions; `useSortedKeys` is available and unused.
- `files.includes` `!!` prefix = **force-ignore** (valid since 2.3): scanner never indexes those trees even in project/type mode. The estate's `!!**/node_modules`, `!!**/dist`, etc. are CORRECT (not a defect) — but note force-ignore withholds those trees from the `types`/`project` domains by design; that is the intended posture for output/cache dirs.

Under-used / mis-used, with exact spellings:
- `linter.rules.preset` (INVALID) → replace with `linter.rules.recommended: true`.
- `plugins` (ABSENT) → add the promoted-rule array. Doctrine names the rules to encode: `const`+`type` restatement, parallel schema beside a `Schema` owner, standalone brand export, arity twin (`getX`/`getMany`), barrel file, thin rename wrapper, `throw` in domain flow, mutable accumulation, stdlib-leak. Each is one `.grit` under a repo-owned dir (e.g. `tools/biome/`), registered at `severity="error"`, shipped with a positive-span fixture and a dense must-not-fire fixture (doctrine Shape law).
- `linter.domains` (ABSENT) → add `{ "project": "recommended", "test": "recommended" }` now; add `"types": "recommended"` co-landed with the first `libs/typescript` source wave (it needs indexable source, which the force-ignored trees deliberately exclude).
- `suspicious.noExplicitAny` (implicit) → elevate explicitly to `"error"` (doctrine maps `any` to error; explicit elevation is the promoted-rule contract, and specs already ban casts).
- `assist.actions.source.useSortedKeys` (ABSENT) → `"on"` for deterministic config/schema key order.
- The estate's two suppressions — `complexity.useLiteralKeys: "off"`, `suspicious.noShadowRestrictedNames: "off"` — are valid rule paths; keep only if a promoted GritQL rule or the code shape justifies them, else they are ceremony (doctrine Finding law).

## [02]-[TYPESCRIPT] — dual-floor, clean

`tsconfig.base.json` is exemplary: the full doctrine strictness set is present (`strict`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noPropertyAccessFromIndexSignature`, `noImplicitOverride`, `noFallthroughCasesInSwitch`, `noUncheckedSideEffectImports`, `strictBuiltinIteratorReturn`, `stableTypeOrdering`, `isolatedModules`, `isolatedDeclarations`, `erasableSyntaxOnly`, `verbatimModuleSyntax`, `moduleDetection: "force"`, `moduleResolution: "bundler"` + `customConditions: ["types","import"]`). No deprecated TS6 options present (none of `target: es5`, `moduleResolution: node10/classic`, `baseUrl`, `outFile`, `downlevelIteration`, legacy `module` values).

Actions:
- No config-shape change. The dual floor (tsgo native + tsc JS) is realizable today. Track TS7.0 GA: when it defaults, the native lane migrates `@typescript/native-preview` (nightly) → `typescript@rc`/stable in the catalog, and `@effect/tsgo` follows; a construct the two compilers disagree on is rewritten, never suppressed (doctrine parity claim). This is a `pnpm-workspace.yaml` catalog bump under `# tooling`, not a `tsconfig` edit.
- `nx.json` already selects the native lane via `@nx/js/typescript` `"compiler": "tsgo"` and `@nx/vite` `"compiler": "tsgo"`. The tsc floor is the plain `typescript` 6.0.3. Keep both.

## [03]-[VITE_ROLLDOWN] — one deprecated chunk key

Vite 8 ships Rolldown as the single bundler; `build.rolldownOptions` is the accepted key (the compat layer also converts `rollupOptions`/`esbuild`). Native Rust plugins default-on (`'v1'`). The factory's `css.transformer: 'lightningcss'` + `cssMinify: 'lightningcss'` + `target: 'baseline-widely-available'` are all current.

Under-used / deprecated, exact spellings:
- `vite.factory.ts` app build: `rolldownOptions.output.manualChunks: chunk` — **`manualChunks` is deprecated in Rolldown.** Migrate to `output.advancedChunks: { groups: [...] }`. The existing `B.chunks` weight table (`vendor-react` p=`react(?:-dom)?` w=3, `vendor-effect` p=`@effect` w=2, `vendor` p=`node_modules` w=1) maps directly:
  ```ts
  advancedChunks: {
    groups: [
      { name: 'vendor-react', test: /\/react(?:-dom)?\//, priority: 3 },
      { name: 'vendor-effect', test: /@effect/, priority: 2 },
      { name: 'vendor', test: /node_modules/, priority: 1 },
    ],
  }
  ```
  `priority` replaces the hand-rolled `.sort((a,b)=>b.w-a.w)` weight walk; `test` accepts the same regexes; the `chunk()` Option-fold operation collapses into the declarative group table (denser owner, doctrine collapse). `advancedChunks` also exposes `minSize`/`shareCount`/`includeDependenciesRecursively` for the tiny-chunk-proliferation control the naive default lacks — but `minSize` is inert without `groups`, so it rides the group rows.
- `worker.rolldownOptions.output` uses `output('workers/')` (no chunk map) — leave; workers rarely need advanced grouping.
- The `library`/`server` modes use `rolldownOptions` without `manualChunks` — no change.

## [04]-[VITEST] — browser-mode lane not wired

`vitest.config.ts` is a strong single-project runner: artifacts route to `.artifacts/typescript/`, cache to `.cache/vitest`, coverage thresholds correctly nested under `coverage.thresholds`, no deprecated keys (`environmentMatchGlobs`/`poolMatchGlobs`/`workspace` all absent), `@effect/vitest` peer resolved via pnpm `allowedVersions`.

Under-used, exact spellings:
- `test.projects` (ABSENT) — the tests README [08] mandates a browser-mode lane through the vitest browser provider, and `@vitest/browser-playwright` is installed but never referenced. Add a `projects` array splitting a node unit lane from a browser lane:
  ```ts
  import { playwright } from '@vitest/browser-playwright';
  // test.projects:
  [
    { test: { name: 'unit', environment: 'node', include: [...node globs] } },
    { test: { name: 'browser', browser: { enabled: true, provider: playwright(),
      headless: true, instances: [{ browser: 'chromium' }] },
      include: ['**/*.browser.{test,spec}.ts'] } },
  ]
  ```
  Browser-mode suites are never mutated (README [08]) — Stryker's `mutate` glob already excludes specs, so no cross-wiring. The `instances` array is the v4 shape (the old top-level `browser.name` is superseded).
- `coverage.all` defaults `true` in v4 (untouched files included). The estate scopes via `coverage.include` glob + `skipFull: true`, which is the correct v4 posture — document that removing the `include` glob would pull the whole tree into the report.
- v8 provider uses AST-based remapping (Istanbul-accurate) since 3.2 — `provider: 'v8'` is correct; no move to Istanbul needed.

## [05]-[PLAYWRIGHT] — aria goldens untemplated

`playwright.config.ts` is strong: root-residency self-defense, per-lane `testMatch`, `*.pw.ts` suffix disjoint from vitest globs, per-project+per-platform `snapshotPathTemplate` for screenshot goldens, capped workers/timeouts, artifacts under `.artifacts/typescript/e2e`.

Under-used, exact spellings:
- `expect.toMatchAriaSnapshot.pathTemplate` (ABSENT) — the tests README [08] names "aria goldens" alongside screenshot goldens, but only `snapshotPathTemplate` (screenshot) is set. `toMatchAriaSnapshot` is first-class in 1.61; give aria snapshots their own templated home:
  ```ts
  expect: {
    timeout: 5_000,
    toHaveScreenshot: { maxDiffPixelRatio: 0.02 },
    toMatchAriaSnapshot: { pathTemplate: '{testDir}/goldens/aria/{testFilePath}/{arg}{ext}' },
  }
  ```
  Aria snapshots are engine-invariant (one golden across browsers), so their template omits `{projectName}`/`{platform}` — distinct from the screenshot template by design.
- `updateSnapshots: 'missing'` is the effective default the config relies on (new-platform mint lands as committed files). For the aria patch-review flow, `--update-source-method=3way` is the CLI companion; no config key needed unless a default is pinned.
- `webServer.gracefulShutdown` — only relevant once an e2e `webServer` block exists (none now); note for when the served product flow lands.

## [06]-[STRYKER] — checker unwired, perf knobs absent, "duplicate" is a misread

The `stryker-config.json` / `stryker.config.json` pair is **not a duplicate to collapse** — they are two different tools with two different default filenames:
- `stryker-config.json` = **Stryker.NET (C#)**: `test-runner: mtp`, `solution: Workspace.slnx`, output `.artifacts/csharp/stryker`. Owned by the C# plane, out of TS scope.
- `stryker.config.json` = **StrykerJS (TS)**: `testRunner: vitest`, mutate `libs/typescript/**`, output `.artifacts/typescript/stryker`.

Ruling: the TS estate owns `stryker.config.json` only; leave `stryker-config.json` to the C# plane. There is nothing to merge within the TS scope — collapsing them would break one language. (If root-residency of the C# file is later contested, that is a C#-plane decision, not this campaign's.)

`stryker.config.json` under-used, exact spellings (all verified 9.x):
- `coverageAnalysis` (ABSENT) → `"perTest"`. The vitest runner supports perTest; it is the prerequisite for `ignoreStatic` and sharpens incremental matching.
- `ignoreStatic` (ABSENT) → `true`. Skips load-time-only mutants (large perf win); requires `coverageAnalysis: "perTest"`.
- `checkers` (ABSENT) → `["typescript"]` plus `"tsconfigFile": "tsconfig.base.json"`. `@stryker-mutator/typescript-checker@9.6.1` is INSTALLED but never referenced — a dead catalog pin. Wiring it rejects type-invalid mutants before they run (fewer wasted test runs, higher signal). Alternative if the type-check cost is too high: `"disableTypeChecks": true` (glob or bool) to strip type errors from mutated files — but wiring the checker is the higher-signal choice given the doctrine's typed-rail mandate.
- `reporters: ["html","json"]` → add `"progress"` for live CLI signal (mirrors the C# `["json","html","progress"]`); `"clear-text"` optional for terminal summaries.
- `--testFiles` (9.6 feature) is available to scope mutation runs to changed suites in CI.
- Incremental caveat (document, do not "fix"): the vitest runner reports "tests per file without location" — so `incremental: true` reuses file-granular results but cannot match individual test changes the way the Jest/Cucumber runners can. The `incrementalFile` under `.cache/stryker/` and `tempDirName` are correct and stay uncommitted.

## [07]-[NX] — well-formed, boundaries correctly off-lint

`nx.json` is coherent: `namedInputs` (`default`/`production`/`sharedGlobals`), `targetDefaults` mirror the `.artifacts/typescript/` outputs, `@nx/js/typescript` + `@nx/vite` + `@nx/playwright` plugins wired with `compiler: "tsgo"` (native lane), `build: false` on the TS plugin (packages own their Vite build). Project tags (`scope:*`, `runtime:*`, `plane:*`) are graph metadata that drive affected computation and feed the `_architecture` gauge parse — they correctly do **not** gate as lint (doctrine Boundary law: boundaries ride the exports map + gauges, never `@nx/enforce-module-boundaries`, which would demand ESLint the estate refuses).

Actions: none required. `@nx/conformance` is Enterprise-only — not admissible as a free gate. Optional (not defects): `nx.json` `"sync"` for TS project-reference sync if `references` are ever populated (currently `tsconfig.json.references: []`).

## [08]-[EXPORTS_MAP + TAGS] — the structural boundary gate

`libs/typescript/package.json` `exports` is the doctrine Boundary gate: per-runtime subpaths (`server`/`browser`/`wasm`/`default`) on `core`/`security`/`data`/`runtime` keep server code physically unresolvable from browser resolution; `ui`/`ui/viewer`/`iac` are single-condition (ui=browser-only, iac=node-only) by design. An unexported interior module is unresolvable — the gate is module resolution, not lint. Tags in the six `project.json` files are well-formed triples the `_architecture` boundaries gauge parses live. No change; this is the reference shape the campaign preserves.

## [09]-[PER_FILE_IMPROVEMENT_MAP]

| File | Capability gained | Exact edit |
| --- | --- | --- |
| `biome.json` | valid rules root | `linter.rules.preset` → `linter.rules.recommended: true` |
| `biome.json` | GritQL promoted rules (doctrine [04]) | add `"plugins": ["./tools/biome/<rule>.grit", …]` at root, one `.grit` per doctrine anti-pattern at `severity="error"` + fixtures |
| `biome.json` | type-aware + import-graph rules | add `linter.domains: { "project": "recommended", "test": "recommended" }`; add `"types": "recommended"` co-landed with first `libs/typescript` source |
| `biome.json` | explicit `any` gate | `suspicious.noExplicitAny: "error"` |
| `biome.json` | deterministic key order | `assist.actions.source.useSortedKeys: "on"` |
| `vite.factory.ts` | supported chunking, denser owner | `rolldownOptions.output.manualChunks: chunk` → `output.advancedChunks.groups` table (name/test/priority); fold `chunk()` + `B.chunks` weight-sort into the declarative rows |
| `vitest.config.ts` | browser-mode lane (README [08]) | add `test.projects` splitting node `unit` from a `browser` lane wiring `provider: playwright()` + `instances: [{ browser: 'chromium' }]` |
| `playwright.config.ts` | aria goldens | add `expect.toMatchAriaSnapshot.pathTemplate` (engine-invariant, omit `{projectName}`/`{platform}`) |
| `stryker.config.json` | perTest + static-skip perf | add `"coverageAnalysis": "perTest"`, `"ignoreStatic": true` |
| `stryker.config.json` | type-checked mutants (wire dead dep) | add `"checkers": ["typescript"]`, `"tsconfigFile": "tsconfig.base.json"` |
| `stryker.config.json` | CLI signal | `reporters: ["html","json","progress"]` |
| `stryker-config.json` | — | out of TS scope (Stryker.NET / C# plane); no TS edit; NOT a duplicate of `stryker.config.json` |
| `pnpm-workspace.yaml` | TS7 GA tracking | on GA, bump native lane `@typescript/native-preview` → `typescript@rc`/stable + `@effect/tsgo` under `# tooling` (catalog-only, no tsconfig change) |
| `tsconfig.base.json` | — | no change; exemplary, full doctrine strictness set, zero deprecated options |
| `nx.json` | — | no change; boundaries correctly off-lint; conformance = Enterprise-only |
| `libs/typescript/package.json` | — | no change; exports map is the boundary gate reference shape |
| `libs/typescript/*/project.json` | — | no change; tag triples well-formed, feed gauge parse |
| `tsconfig.json` (root) | — | no change; anchors the four root configs |

## [10]-[DOCTRINE_FOUR_GATE_STATUS]

The campaign's TOOLING-AS-LAW (point 11) demands the doctrine [04] four-gate stack made real:
1. **Compiler dual-floor** — REAL (tsgo native + tsc JS; parity claim standing). Only pending action is the TS7 GA catalog bump.
2. **Biome sole lint + GritQL rules at error** — PARTIAL. Biome is the sole rail (correct), but the promoted-rule layer is EMPTY (`plugins: []`) and the rules root has an invalid `preset` key. This is the single largest gap: the doctrine's "a law with no mechanical gate is a plugin rule" is currently unenforced.
3. **Exports map boundary gate** — REAL (per-runtime subpaths, unresolvable interiors).
4. **Tests gauges** — REAL (`_architecture` admission + boundaries suites parse the ledger/tags live, red-capable against synthetic sources).

Priority for the improver: (a) `biome.json` `preset`→`recommended` + `plugins` array + `domains` (unblocks gate 2); (b) `vite.factory` `manualChunks`→`advancedChunks` (removes the one deprecated key); (c) `vitest` browser `projects` + `stryker` checker/perTest/ignoreStatic (wire installed-but-dead capability).
