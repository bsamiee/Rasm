# [TESTS]

`tests/` is the polyglot proof estate: one adversarial kit per language, per-package suite homes, the live-host scenario rail, and the cross-language contract corpus. Everything under this root exists to falsify production behavior, and everything under it is rebuilt ground-up the moment a denser shape exists — kit files, suite folders, external test libraries, and tooling configuration alike. No workarounds, no aliasing, no band-aids, no backwards compatibility: breaking old tests is never a reason to preserve chaff, and a gate nobody can run is deleted, not kept as aspiration.

## [01]-[LAYOUT]

One folder scheme spans all languages:

```text conceptual
tests/
├── contracts/         # cross-language frozen corpus; C# produces, Python/TS round-trip
├── csharp/
│   ├── .api/          # dev-tool API catalogs the kit and suites compose
│   ├── _architecture/ # boundary + infra-primitive laws proving both kits
│   ├── _benchmarks/   # BenchmarkDotNet switcher + the regression gate verb
│   ├── _scenariokit/  # host-aware scenario SDK (Rasm.ScenarioKit)
│   ├── _testkit/      # host-free adversarial law substrate (Rasm.TestKit)
│   ├── libs/          # per-package suite shells mirroring libs/csharp
│   ├── scenarios/     # scenario content home (Rasm.Scenarios)
│   └── tools/         # infra suites: cs-analyzer, rhino-bridge Contract/Supervisor
├── python/
│   ├── .api/          # dev-tool API catalogs the kit and suites compose
│   ├── _testkit/      # project-agnostic kit: spec/strategies/seams/env/bench/laws/runtime
│   ├── libs/          # per-package suites mirroring libs/python
│   └── tools/         # assay suites
└── typescript/
    ├── .api/          # dev-tool API catalogs the kit and suites compose
    ├── _architecture/ # branch-boundary gauge suites; manifest gauges live, source gauges self-activate
    ├── _testkit/      # @rasm/ts-testkit: corpus readers, laws, arbitraries, harness, bench, gauges
    └── e2e/           # playwright + k6 estate: fixture tower, engine rows, committed goldens
```

[CASING_LAW]:
- Tier and grouping directories are lowercase; kit directories carry the `_` prefix.
- PascalCase begins at a C# project boundary (`Rasm.*`, `Contract`, `Supervisor`) and continues through source folders inside it (`Meta`); grouping directories above a project are never PascalCase.
- Spec files follow the owning language's source casing.

[KIT_LAW]:
- Shared test logic lives in exactly one per-language kit: `tests/csharp/_testkit` (plus `_scenariokit` for the host-aware scenario SDK), `tests/python/_testkit`, and `tests/typescript/_testkit`.
- Kits never live under `libs/` — libs is the production plane.
- Nothing cross-language lives inside a single language's tree; the neutral seams are `tests/contracts/`, `tests/containers.json` (the one container-image pin every language's container row resolves), proto descriptors, provisioned service containers, and the assay operator.

## [02]-[LANES]

Test lanes are orthogonal to language; every suite declares its lane through the owning route, never through folder improvisation:

| [INDEX] | [LANE]      | [BOUNDARY]                                                 | [ROUTE]                                                     |
| :-----: | :---------- | :--------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | unit        | in-process, deterministic time, no sockets                 | default test run per language                               |
|  [02]   | property    | generated-input law over a unit subject                    | `Spec.ForAll`, `@spec` + Hypothesis, `it.effect.prop`       |
|  [03]   | integration | real process/IO boundary: containers, subprocess, loopback | `network`/`subprocess` (Py); boundary suites elsewhere      |
|  [04]   | scenario    | live-host evidence through the rhino bridge                | `[RhinoScenario]` content + the assay bridge rail           |
|  [05]   | benchmark   | measurement in a separate session, never inside unit runs  | `_benchmarks` switcher, `-m benchmark`, bench include glob  |
|  [06]   | mutation    | assay-gated survivor discovery                             | assay routes over root Stryker configs + staged Python gate |

The word `integration` is reserved for the real process/IO boundary. A test that runs in-process with doubles is a unit test regardless of how many owners it spans; calling it integration inflates the lane and hides the missing boundary proof.

## [03]-[PROOF_LAW]

A test is an adversarial law with an independent oracle, never confirmation of current output. An oracle predicts behavior from an independent source: closed-form math, conservation, fixture geometry, a category contract, runtime observation, or documented external behavior. Grade the proof before writing it — Grade A is an independent prediction, Grade B a metamorphic or model relation, Grade C a durable failure-category rail, Grade D a shape-only inspection of values the test itself constructed. Grade D stands alone nowhere: it pairs with an A/B oracle or a C rail, or it is deleted.

Every law family is witness-mandatory: registration carries a refuting witness the law must fail on. A witness the law survives exposes a tautology no mutant can violate, and that registration is itself the failure.

[BANNED_SHAPES]:
- Existence tests: asserting a symbol, export, case, or member exists — the compiler, importer, or type checker already proves it.
- Mirror tests: constructing a value and asserting its own fields, re-implementing the production algorithm as its own oracle, or snapshotting a value the test itself built in the same body.
- Speculative-state tests: laws over states the production surface cannot construct.
- Per-function spam: one thin test per function when a single generated domain covers the family.

A failing law is evidence: investigate the production owner before weakening the test, and when the law found a real bug, fix the owner — never dilute the law into shape-only proof. Each language README carries the language spelling of these laws plus its own additional bans.

## [04]-[ARTIFACT_ROUTING]

Every tool writes reports under `.artifacts/` and temp/work state under `.cache/<tool>/`; the repo root stays litter-free, and exact directories live in the owning configuration:

| [INDEX] | [TOOL]            | [SURFACE]                            | [ROUTE_OWNER]                                                    |
| :-----: | :---------------- | :----------------------------------- | :--------------------------------------------------------------- |
|  [01]   | coverlet.MTP      | C# coverage                          | `Directory.Build.props` Coverage block                           |
|  [02]   | MTP TrxReport     | C# test results                      | invocation law in `tests/csharp/README.md`                       |
|  [03]   | Stryker.NET       | C# mutation                          | root `stryker-config.json` + assay mutation rail (staged)        |
|  [04]   | BenchmarkDotNet   | C# benchmarks                        | `tests/csharp/_benchmarks` session config                        |
|  [05]   | pytest + coverage | Python coverage + caches             | `pyproject.toml` tool tables                                     |
|  [06]   | Hypothesis        | example database + observability     | `tests/python/_testkit/runtime.py`                               |
|  [07]   | pytest-benchmark  | Python benchmark storage             | `pyproject.toml` addopts                                         |
|  [08]   | mutmut            | Python mutation                      | `pyproject.toml` `[tool.mutmut]` + `.config/coverage-mutmut.ini` |
|  [09]   | inline-snapshot   | snapshot storage                     | `pyproject.toml` `[tool.inline-snapshot]`                        |
|  [10]   | Vitest            | TS coverage + results + bench ledger | root `vitest.config.ts`                                          |
|  [11]   | StrykerJS         | TS mutation                          | `stryker.config.json`                                            |
|  [12]   | Playwright        | e2e traces + results                 | root `playwright.config.ts` (auto-discovery self-defense)        |
|  [13]   | Nx                | target outputs + cache               | `nx.json` targetDefaults                                         |
|  [14]   | import-linter     | grimp cache                          | assay static rail invocation (`--cache-dir .cache/grimp`)        |

Tool-admission litter rule: a change that admits or reconfigures any tool proves its caches and outputs land under `.cache/` or `.artifacts/` before it lands — routed through the tool's own documented configuration, config-file setting first, CLI flag second, never wrapper scripts or conftest shims. Gate: after the change's checks run, `git status` plus a root listing shows zero new root entries.

## [05]-[EXTENSION_PROTOCOL]

Every new suite, kit capability, fixture, or corpus asset has exactly one home; extending the canonical owner always beats adding a sibling, and an owner whose shape is no longer the densest is rebuilt ground-up, never accreted around:

| [INDEX] | [ADDITION]                  | [HOME]                                                                                   |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | C# per-package suite        | `tests/csharp/libs/<Package>/`, specs mirroring source paths as `<Source>.spec.cs`       |
|  [02]   | C# scenario                 | `tests/csharp/scenarios`                                                                 |
|  [03]   | C# kit capability           | `tests/csharp/_testkit` (host-free) or `tests/csharp/_scenariokit` (host-aware)          |
|  [04]   | C# infra-tool suite         | `tests/csharp/tools/<tool>/`                                                             |
|  [05]   | C# gated benchmark          | `tests/csharp/_benchmarks` — a gated case is a registry row beside the switcher          |
|  [06]   | Python per-package suite    | `tests/python/libs/<package>/`                                                           |
|  [07]   | Python kit capability       | the owning module in `tests/python/_testkit`                                             |
|  [08]   | Python tool suite           | `tests/python/tools/<tool>/`                                                             |
|  [09]   | Python dev-tool API catalog | `tests/python/.api/`, one catalog per dev-plane package                                  |
|  [10]   | TS unit spec                | beside its source in `libs/typescript`                                                   |
|  [11]   | TS kit capability           | `tests/typescript/_testkit`                                                              |
|  [12]   | TS e2e suite                | `tests/typescript/e2e`                                                                   |
|  [13]   | TS architecture gauge       | `tests/typescript/_architecture` — branch-boundary suites the exports map cannot express |
|  [14]   | TS dev-tool API catalog     | `tests/typescript/.api/`, one catalog per dev-plane package                              |
|  [15]   | contract corpus seam        | `tests/contracts/<seam>/` per the corpus law                                             |

Per-package mirror law: where the ecosystem separates tests from source, suite homes mirror the production tree — C# shells under `tests/csharp/libs` mirror `libs/csharp`, Python suites under `tests/python/libs` mirror `libs/python`. TS unit specs instead colocate beside source per the vitest idiom, so `tests/typescript/` never hosts unit specs.

## [06]-[SCENARIO_PIPELINE]

Scenario proof flows through one route, content to verdict:
1. Content: scenarios live in `tests/csharp/scenarios` as source-only `[RhinoScenario]` statics composing the `Rasm.ScenarioKit` SDK; the project is an `AssayTestShell`, so the routing closure keeps it out of unit-test runs.
2. Closure: `uv run python -m tools.assay bridge build` compiles the bridge plugin and stages scenario content plus its dependency closure for the host.
3. Evidence: the live RhinoWIP host executes the staged scenarios; `ScenarioContext` fact streams, manifests, and captures fold into the assay-owned artifact scopes.
4. Verdict: `uv run python -m tools.assay bridge verify` folds the run into one bridge Envelope; `bridge status` reports host health, and `bridge quit` terminates the host cleanly.

Reference lifecycle: `--evidence author` runs write candidate references under `tests/csharp/scenarios/_references/<theme>/`, human review promotes a candidate by renaming it to `<method>.reference.json`, and a verify run over an unpromoted corpus degrades rather than fails. The full lifecycle, tolerance, and admission law is [tools/rhino-bridge/README.md](../tools/rhino-bridge/README.md).

The `Contract` and `Supervisor` suites under `tests/csharp/tools/rhino-bridge` prove the wire contract and the supervisor fold that this pipeline rides; a bridge protocol change lands with its suite change or it does not land.

## [07]-[GATE_OWNERSHIP]

The assay operator is the single mutation and coverage gate authority in all three languages; thresholds and kill-floors live in the owning configs, never in docs or specs:
- Stryker.NET policy — solution mode, concurrency cap, output routing, baseline, thresholds — lives in the root `stryker-config.json`; the assay mutation rail owns the staged invocation with absolute anchors, and root residency keeps a bare `dotnet stryker` inside auto-discovery, capped and routed instead of solution-wide at default parallelism.
- StrykerJS policy lives in the root `stryker.config.json`; the TS invocation law is [tests/typescript/README.md](typescript/README.md).
- The Python mutation lane is a staged gate under assay scored against its kill-floor; the lane law is [tests/python/README.md](python/README.md).
- Zero mutant discovery is a failed rail in every language, never a green pass.
- Both Stryker rails emit `mutation-testing-report-schema` JSON natively into `.artifacts/`; assay's kill-floor verdict is the single cross-language authority over the results.
- Coverage aggregates as cobertura (C#) plus lcov (Python, TS) under `.artifacts/` — no invented merged format; each language-native reporter owns its output shape.

Heavy-lane invocation law: the bounded lanes — unit, property, and benchmark sessions per language — may be launched directly by a human or an agent; mutation, solution-wide static, and bridge verify ride assay, which owns staging, governor caps, and artifact scopes. Defense-in-depth holds regardless of invoker: every heavy tool's auto-discovered configuration carries its own concurrency cap, per-run and per-test timeouts, an explicit mutate/target scope, and `.artifacts/`/`.cache/` output routing, so a bare invocation outside assay is small, self-limiting, and cheap to kill — never a machine-saturating sweep, never a root write.

## [08]-[CONTRACTS_CORPUS]

`tests/contracts/` is the cross-language frozen corpus: wire bytes plus canonical JSON per seam and message, C#-produced, consumed read-only by Python and TypeScript. The full producer/consumer, layout, and regeneration law is [tests/contracts/README.md](contracts/README.md).

## [09]-[TOOLING_AWARENESS]

Before touching any testing surface, an agent checks the owners that carry the facts:

| [INDEX] | [SURFACE]                                            | [CARRIES]                                                                         |
| :-----: | :--------------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props` + `Directory.Build.props` | C# test-stack pins, project classifiers, lane wiring, coverage routing            |
|  [02]   | `Directory.Build.targets`                            | classifier vocabulary sealing: each tests/csharp project carries one classifier   |
|  [03]   | `pyproject.toml`                                     | Python test dependencies, pytest/coverage/mutmut/import-linter policy, markers    |
|  [04]   | `pnpm-workspace.yaml`                                | TS catalog pins, peer-rule resolutions, workspace package globs                   |
|  [05]   | `.config/`                                           | mutmut coverage side-file, dotnet tool manifest                                   |
|  [06]   | `vitest.config.ts` + `stryker*.json` + `nx.json`     | TS runner defaults, artifact outputs, root Stryker configs, project-graph targets |
|  [07]   | `tools/assay`                                        | gate rails: `static`/`test`/`bridge`/`docs`/`code`/`package`/`api`/`provision`    |

The operator is itself a tested surface: every `tools/` operator owns a suite under `tests/<language>/tools/<tool>`, and operator and suite move in the same change — `tools/assay` with `tests/python/tools/assay`, `tools/cs-analyzer` with `tests/csharp/tools/cs-analyzer`. A rail change without its spec change is an incomplete change.
