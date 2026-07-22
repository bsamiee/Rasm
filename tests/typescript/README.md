# [TYPESCRIPT_TESTING]

Authoring law for every TypeScript spec, kit member, gauge, and e2e suite. Every spec composes `@rasm/ts-testkit` through the workspace graph, and unit specs colocate beside their source under `libs/typescript`.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled TypeScript-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per dev-plane package; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../contracts/README.md): Corpus consumer law — byte-identical round-trips over every C#-emitted asset.

## [02]-[TOPOLOGY]

- Unit specs colocate beside their source in `libs/typescript` per the vitest idiom — spec placement is language-idiomatic, kit placement is not; a unit spec placed under `tests/typescript/` is misfiled.
- `tests/typescript/` owns only the shared kit package (`_testkit`), the architecture suite (`_architecture`), the playwright e2e estate (`e2e`), and the dev-tool API catalog tier (`.api`).
- Kit falsification suites colocate beside kit source — every kit capability carries the spec that proves it can fail.
- `.api/` catalogs dev tooling only, under the one-tier catalog law: a package is catalogued at exactly one tier, versions live only in `pnpm-workspace.yaml`, and runtime packages never catalog here.

## [03]-[KIT]

`@rasm/ts-testkit` is the one shared kit — a private, source-exporting workspace package imported via the workspace graph, never by reach-around.

Kit ships no barrel — each owner is an exports-map subpath — and pins no versions; every dependency resolves via the workspace catalog or graph:
- [01]-`/corpus`: Manifest-keyed corpus readers with typed absence (`Emitted`/`Awaiting`/`Blocked`), `ContentDigest`, `CANONICAL_BYTE_IDENTITY`.
- [02]-`/laws`: Witness-mandatory `Law` combinators — construction demands a refuting foil, and registration runs the tautology audit.
- [03]-`/arbitraries`: Schema-derived arbitraries with the field-absence and distinct-payload lanes.
- [04]-`/harness`: Mints the harness `Layer`s — pglite fast lane, container rows as data, transactional `sandbox`, object store, loopback capsule.
- [05]-`/bench`: Folds the autosaved ledger into the sustained-regression gate.
- [06]-`/gauges`: Owns the import-graph and snapshot-hygiene gauge engines.
- [07]-`/telemetry`: One capture returning the work's `Exit`, per-metric drift rows, and every opened span — an audit law asserts as one lookup.
- [08]-`/e2e`: Serves the fixture tower's platform substrate — paused clock, multi-context cohort, virtual authenticator, axe wcag audit.
- [09]-`/setup`: Runner boot — `addEqualityTesters()` wires structural `Equal.equals` into every `toEqual` estate-wide.

## [04]-[LAWS]

Specs are written against the installed vitest major — the `@effect/vitest` peer range is resolved explicitly as a peer rule in `pnpm-workspace.yaml`, never by silent tolerance:
- `it.effect` runs a law inside the Effect test environment with `TestClock` control; `it.live` opts into real time and real services when the law is about wall-clock behavior.
- `it.layer` shares an expensive `Layer` across a suite; layer construction inside each test body is a density failure.
- `it.effect.prop` is the one property surface: fast-check arbitraries feed Effect laws without losing `TestClock`. `@fast-check/vitest` is rejected for exactly that loss.
- Property engine is the one `effect/FastCheck` re-export. A direct `fast-check` workspace admission is rejected: a second engine copy breaks `Arbitrary` class identity across the kit's input dispatch, so `Arbitrary.make`, `it.effect.prop`, and every kit combinator must resolve the same engine instance.
- `it.scoped` and direct `TestServices` reach-ins are banned; scoping rides the effect under test, service substitution rides layers.

Time-dependent laws advance `TestClock` deterministically instead of sleeping; a spec that awaits real time in the unit lane is an integration test mislabeled or a flake being incubated.

## [05]-[ORACLES]

Schema, failure-tag, and union proofs ride these rails; typed error tags and exhaustive union arms are the TS spelling of the Grade C failure-category rail:
- Schema-first boundaries prove as decode/encode round-trip laws over generated arbitraries; a boundary without a round-trip law is unproven.
- Failure lanes prove by tag: assert the typed error case identity, never message substrings.
- Generators derive from the owning schema where one exists; hand-rolled arbitraries are for domains the schema cannot express, and they generate distinct payload values wherever a subject transports multiple inputs — equal placeholders hide swapped arguments.
- Union-shaped subjects prove exhaustively: one property or matrix per union that visits every arm, so an added arm fails loudly instead of passing silently.

## [06]-[GAUGES]

`tests/typescript/_architecture` is the gauge home — the branch-boundary suites the `@rasm/ts` exports map cannot express, the analog of `tests/csharp/_architecture`. Its charter: the edge-ledger import audit, per-runtime subpath purity, the external-admission and per-sub-folder package-admission audits, and the branch-wide migrator-import ban.

- Permitted-edge ledger parses live from its owning page — the strata flowchart in `libs/typescript/.planning/ARCHITECTURE.md` is the single source: `subgraph W<n>` clusters carry the wave marks, `[IMPORT]`-labeled edges carry the permitted crossings, and a reshaped or vanished fence fails the gauge loudly. Acyclicity, wave direction, and the runtime/plane tag law prove against the parsed rows and folder tag triples.
- Manifest gauges run now: every spec-estate pin across every dependency block resolves through `catalog:` or `workspace:` only, the refused property engine cannot enter a manifest, the recorded workspace facts hold as pattern rows, and a package is catalogued at exactly one `.api` tier.
- Lint legislature gauges beside them: `biome.json` must realize the promoted GritQL rule roster one-to-one against `tools/biome/` on disk, every rule registering at error with its firing and non-firing proof spans, the domain arming held to its closed ruling (`test` at `recommended`; `project`/`types` disarmed — whole-graph inference belongs to the dual compiler floor), an explicit `noExplicitAny` error row, and the estate override promoting `noConsole` and `noDefaultExport` to error.
- Laws no single-pattern rule can express hold as declared review-only rows, so a missing rule is a ruling, never an oversight.
- Roster is proven live, not textually: the gauge executes the real binary over each rule's own spans in a scratch config, every `FIRES` line must draw the plugin diagnostic as its own isolated fixture — a multi-arm rule proves every arm, never just its loudest — and the `CLEAN` spans must lint silent as one file; a span that fails to parse proves nothing and fails red.
- Source-walking audits run the real rule set — ledger edges, folder-scoped external admissions, the security sub-folder crypto rows, the migrator ban — through the kit's import engine: while `libs/typescript` ships no source the verdict is `Unsupported`, never a vacuous green, and every verdict is proven red-capable against synthetic sources, so the audit is armed the day source lands.
- Estate-wide snapshot-hygiene sweep stands here as the kit gauge's one standing consumer.
- A gauge verdict is a structural fact — a law the compiler or exports map already enforces physically is never re-proven here.

## [07]-[E2E]

Browser-mode suites run real browser semantics through the vitest browser provider and stay in the unit/property lanes' runner: the root config's `browser` project drives the `playwright()` provider over a chromium instance row, matches only the `*.browser.{test,spec}` dialect, and activates the day the first browser spec lands. E2e suites live under `tests/typescript/e2e` on the playwright rail and own full user-flow proof — the split is capability-driven: browser-mode proves DOM-coupled units, e2e proves flows across pages and processes.

Browser-mode suites are never mutated: the mutation runner does not support them, the mutate scope excludes the dialect by glob, and a mutated browser suite is noise, not signal.

- Root `playwright.config.ts` bounds the playwright rail — root residency is deliberate self-defense: playwright resolves its config from cwd only, so a config-less root invocation sweeps every `*.spec.ts` in the tree, executes foreign module top-levels, and writes a root `test-results/`; auto-discovery of the root config makes every bare run scoped, capped, and routed under `.artifacts/typescript/e2e/`.
- E2e specs carry the `*.pw.ts` suffix and k6 scripts the `*.k6.ts` suffix — both disjoint from the vitest globs by construction, so the two runners can never sweep each other's estates.
- Target roster is config data: each row is one system-under-test carrying its engine lanes, and a served product lands as one row — origin, serve command, prefixed lane names — whose projects and `webServer` lifecycle mint from the roster; the hermetic row owns the empty prefix so committed golden paths never move, and a new engine is one lane row with its out-of-band `playwright install <engine>`, per-lane `testMatch` keeping screenshot goldens single-engine.
- Evidence capture is a policy row keyed by profile: CI traces the retry pass and keeps failure video; a local run has zero retries, so the trace itself carries the failure and video stays off.
- Screenshot goldens commit under `tests/typescript/e2e/goldens/` through the `snapshotPathTemplate`, keyed per-project and per-platform so a new CI platform lands as a golden mint, never a break; file-backed aria goldens route engine-invariant under `goldens/aria/` through their own template while small aria contracts stay inline; run artifacts never mix with committed goldens.
- One fixture tower in `tests/typescript/e2e/fixtures.ts` is target-agnostic: every row resolves its origin through the one arming seam — an unset `baseURL` is the hermetic row, armed in-context from the kit's route-fulfilled corpus; a served project's `baseURL` rides the real server — so a spec speaks `target.open`/`target.origin` and never knows which world serves it.
- That tower composes the kit's `/e2e` substrate, and every platform capability lands with the falsification twin that proves it can fail; a scenario class blocked on an unbuilt producer is a named skip carrying its activation condition, never a simulacrum flow.
- k6's load lane is a subprocess boundary: typed scripts are input artifacts to the spawned binary, the kit decodes the summary receipt, the exit code is the verdict authority, and the lane self-activates when a k6 binary is on PATH.

## [08]-[GATES]

- Root `vitest.config.ts` is the runner authority: two projects over one inherited option spine — the `unit` lane owns the node estate, the `browser` lane owns the real-engine dialect — with coverage and result artifacts routed to `.artifacts/typescript/` under the per-file thresholds the config owns, runner cache in `.cache/vitest`, and worker concurrency and test/hook timeouts bounded in the config.
- Root `package.json` scripts are the named gate entrypoints: `typecheck` runs the dual compiler floor (`tsgo` then `tsc`) over the root solution and every spec-estate project, `check` runs the sole lint rail with the promoted GritQL rules, and `test`, `coverage`, `bench`, `mutation`, and `e2e` own their runners — `coverage` is the committed arming of the per-file thresholds, `bench` the committed feeder of the sustained-regression ledger.
- Benchmark runs autosave their report into `.artifacts/typescript/bench/` through the same config, and the kit's bench gate folds the accumulated ledger into a sustained-regression verdict — a gate that only fires on a manual save is dead.
- Container-lane images pin in `tests/containers.json` — the polyglot owner every language's container row resolves — and `RASM_TESTKIT_CONTAINERS` activates the live lanes; an inactive lane skips loudly by name, never silently. Container suites ride testcontainers with the Ryuk reaper on by default — abandoned runs cannot strand containers, networks, or volumes, and nothing in the repo sets `TESTCONTAINERS_RYUK_DISABLED`.
- Nx target defaults in `nx.json` mirror the test and e2e outputs for the project graph; bench and mutation stay direct runner invocations outside it.
- Mutation runs ride StrykerJS from the root `stryker.config.json` — root residency keeps every invocation, flagged or bare, inside auto-discovery so sandbox state can never fall back to a root `.stryker-tmp/`; the config carries the concurrency cap, dry-run and per-test timeouts, and the explicit mutate scope, temp state rides `.cache/stryker/`, reports land in `.artifacts/typescript/stryker`, and the incremental file stays uncommitted.

## [09]-[SNAPSHOTS]

`toMatchFileSnapshot` owns file-backed goldens, and its primary consumer is the contracts corpus: decode a `tests/contracts/` asset, re-encode it, and snapshot-prove equivalence under the corpus law. Inline snapshots carry small stable projections only. Kit's snapshot-hygiene gauge stands over every golden dialect in the estate — runner snapshots and playwright goldens alike: a snapshot whose owning spec no longer exists is stale evidence flagged for deletion, never silently retained.

## [10]-[DENSITY_AND_BANS]

A spec file is strong when one generated domain attacks decoding, projection, failure tags, and an independent oracle together. Before a second `it` that shares setup with an existing one, collapse into `it.effect.prop`, a case matrix, or an `it.layer` suite; a spec earns lines only through a new oracle, boundary, or product-bug guard. Architecture suite gauges the 175-LOC density cap over colocated runtime-branch specs live; the kit falsification and gauge suites under `tests/typescript/` are the declared carve-out.

[BANNED_SHAPES]:
- Real-time waits in the unit lane; `TestClock` owns time.
- Type-cast escapes in specs: an `any` or unchecked cast in a test hides exactly the boundary the test exists to prove.
- Kit bypass: a shared harness or helper landed beside a spec instead of as a kit export — the duplication seed every sibling then imports wrong.
