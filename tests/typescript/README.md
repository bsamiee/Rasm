# [TYPESCRIPT_TESTING]

Authoring law for every TypeScript spec, kit member, gauge, and e2e suite. Every spec composes `@rasm/ts-testkit` through the workspace graph, and unit specs colocate beside their source under `libs/typescript`.

## [01]-[ROUTER]

- [01]-[RULINGS](RULINGS.md): Settled TypeScript-tree testing decisions — package admissions, oracle discriminants, structure retirements.
- [02]-[API](.api/): Dev-tool API catalogs, one per dev-plane package; kit members and specs transcribe at catalog-verified spellings.
- [03]-[CONTRACTS](../../libs/contracts/README.md): Estate conformance law — TypeScript proves verified vectors through their elected oracle.

## [02]-[TOPOLOGY]

- Unit specs colocate beside their source in `libs/typescript` per the vitest idiom; a unit spec under `tests/typescript/` is misfiled.
- `tests/typescript/` owns the shared kit, the architecture suite, the playwright e2e estate, and the dev-tool catalog tier alone.
- Kit falsification suites colocate beside kit source — every kit capability carries the spec that proves it can fail.
- `.api/` catalogs dev tooling alone under the one-tier law, each package catalogued at exactly one tier with versions in `pnpm-workspace.yaml`.

## [03]-[KIT]

`@rasm/ts-testkit` is the one shared kit — a private, source-exporting workspace package imported via the workspace graph, never by reach-around.

Kit ships no barrel — each owner is an exports-map subpath — and pins no versions; every dependency resolves via the workspace catalog or graph:
- [01]-`/corpus`: Strict V2 Effect Schema admission, exact asset custody, generated-descriptor round-trip, typed facts parity, and case-aware proof.
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
- `it.effect` runs a law under `TestClock` control; `it.live` takes real time and real services where the law is about wall-clock behavior.
- `it.layer` shares an expensive `Layer` across a suite; layer construction inside each test body is a density failure.
- `it.effect.prop` is the one property surface, feeding arbitraries to Effect laws without losing `TestClock`; `@fast-check/vitest` loses it.
- `effect/FastCheck` is the one property engine, since a second `fast-check` copy breaks `Arbitrary` class identity across the kit's input dispatch.
- `it.scoped` and direct `TestServices` reach-ins are banned; scoping rides the effect under test, service substitution rides layers.

Time-dependent laws advance `TestClock` deterministically instead of sleeping; a spec that awaits real time in the unit lane is an integration test mislabeled or a flake being incubated.

## [05]-[ORACLES]

Schema, failure-tag, and union proofs ride these rails; typed error tags and exhaustive union arms are the TS spelling of the Grade C failure-category rail:
- Schema-first boundaries prove as decode/encode round-trip laws over generated arbitraries; a boundary without a round-trip law is unproven.
- Failure lanes prove by tag: assert the typed error case identity, never message substrings.
- Generators derive from the owning schema, a hand-rolled one covering what no schema expresses and drawing distinct payloads so no swap hides.
- Union-shaped subjects prove exhaustively, one property or matrix visiting every arm, so an added arm fails loudly.

## [06]-[GAUGES]

`tests/typescript/_architecture` is the gauge home — the branch-boundary suites no single manifest can express, the analog of `tests/dotnet/_architecture`. Its charter: the edge-ledger import audit, per-package entrypoint purity, the manifest-edge/tsconfig-reference/tag agreement gate, per-package isolation-completeness, the app-island audit, the external-admission and per-sub-folder package-admission audits, and the branch-wide migrator-import ban.

- Permitted-edge ledger parses live off the branch strata flowchart, so acyclicity, direction, and tag law prove on rows a reshaped fence fails.
- Manifest gauges hold every spec-estate pin to `catalog:` or `workspace:`, bar the refused property engine, and cap a package at one `.api` tier.
- Lint gauges hold `biome.json` one-to-one against the `tools/biome/` GritQL roster, every rule at error with its firing and non-firing proof spans.
- Laws no single-pattern rule can express hold as declared review-only rows, so a missing rule is a ruling, never an oversight.
- Roster proves live: the gauge runs the real binary over each rule's spans, every `FIRES` line drawing its own diagnostic and `CLEAN` linting silent.
- Source-walking audits run the real rule set through the kit's import engine, `Unsupported` over no source and red-capable against synthetics.
- Estate-wide snapshot-hygiene sweep stands here as the kit gauge's one standing consumer.
- Gauge verdicts are structural facts — a law the compiler or exports map already enforces physically is never re-proven here.

## [07]-[E2E]

Browser-mode suites run real browser semantics through the vitest browser provider and stay in the unit/property lanes' runner: the root config's `browser` project drives the `playwright()` provider over a chromium instance row, matches only the `*.browser.{test,spec}` dialect, and activates the day the first browser spec lands. E2e suites live under `tests/typescript/e2e` on the playwright rail and own full user-flow proof — the split is capability-driven: browser-mode proves DOM-coupled units, e2e proves flows across pages and processes.

Browser-mode suites are never mutated: the mutation runner does not support them, the mutate scope excludes the dialect by glob, and a mutated browser suite is noise, not signal.

- Root `playwright.config.ts` bounds the rail: playwright resolves config from cwd alone, so root residency scopes and routes every bare invocation.
- E2e specs carry `*.pw.ts` and k6 scripts `*.k6.ts`, both disjoint from the vitest globs, so neither runner sweeps the other's estate.
- Target roster is config data, one row per system-under-test minting its projects and `webServer` lifecycle; the hermetic row owns the empty prefix.
- Evidence capture is a policy row keyed by profile: CI traces the retry pass and keeps failure video, a zero-retry local run tracing the failure.
- Screenshot goldens commit through `snapshotPathTemplate` keyed per project and platform, so a new CI platform mints a golden, never breaking one.
- One target-agnostic fixture tower resolves every origin at one arming seam, so a spec speaks `target.open`/`target.origin`, blind to its world.
- Every platform capability lands with the falsification twin proving it can fail; a blocked scenario class is a named skip carrying its activation.
- k6's load lane is a subprocess boundary: the kit decodes the summary JSON, the exit code decides, and the lane self-activates on a k6 binary.

## [08]-[GATES]

- Root `vitest.config.ts` is the runner authority: package projects derive from its `createProject` export; one `browser` lane arms the real engine.
- `tsc --build` over the solution references is the one compiler gate, Biome `check` the sole lint rail; manifest scripts own nothing.
- Benchmark runs autosave their report, and the kit's bench gate folds the accumulated ledger into a sustained-regression verdict.
- `tests/containers.json` pins container images and `RASM_TESTKIT_CONTAINERS` arms the live lanes under Ryuk; an inactive lane skips loudly by name.
- `nx.json` registers `@nx/vitest`; each package's `vitest.config.ts` lands its inferred `test` target, and `nx run-many -t test` runs the estate.
- Mutation rides StrykerJS directly, outside the Nx graph; `stryker.config.json` root residency keeps auto-discovery bounded, out of `.stryker-tmp/`.
- Stryker runs uncheckered: the TypeScript checker's `typescript` peer resolves to the TS7 API stub and cannot boot; vitest alone carries the verdict.
- Playwright `browsers` provisioning is an `@rasm/ts-e2e` target; root residency stands because playwright resolves config from cwd alone.

## [09]-[SNAPSHOTS]

`toMatchFileSnapshot` owns file-backed goldens from independent producers. Contract proof strictly decodes V2 `manifest.json`, fingerprints every verified asset, resolves generated Protobuf descriptors by exact FQN, and executes semantic round-trip, value parity, semantic conformance, external digest, or publisher digest as elected. HDF5 facts remain custody-only in TypeScript because no TypeScript peer decodes that seam. Inline snapshots carry small stable projections only. Snapshot hygiene still rejects every golden whose owning spec no longer exists.

## [10]-[DENSITY_AND_BANS]

Spec files are strong when one generated domain attacks decoding, projection, failure tags, and an independent oracle together. Before a second `it` that shares setup with an existing one, collapse into `it.effect.prop`, a case matrix, or an `it.layer` suite; a spec earns lines only through a new oracle, boundary, or product-bug guard. Architecture suite gauges the 175-LOC density cap over colocated runtime-branch specs live; the kit falsification and gauge suites under `tests/typescript/` are the declared carve-out.

[BANNED_SHAPES]:
- Real-time waits in the unit lane; `TestClock` owns time.
- Type-cast escapes in specs: an `any` or unchecked cast in a test hides exactly the boundary the test exists to prove.
- Kit bypass: a shared harness or helper landed beside a spec instead of as a kit export — the duplication seed every sibling then imports wrong.
