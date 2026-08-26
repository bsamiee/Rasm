# [TESTS]

`tests/` is the polyglot proof tree: one adversarial kit per language, per-package suite homes, and the live-host scenario lane. Everything under this root exists to falsify production behavior; breaking old tests is never a reason to preserve them, a gate nobody can run is deleted. Settled decisions live in the repo `RULINGS.md` registry — read before re-deciding, extended when a decision lacks a durable home.

## [01]-[LAYOUT]

One folder scheme spans all languages:

```text
tests/
├── dotnet/
│   ├── .api/           # Dev-tool API catalogs the kit and suites compose
│   ├── scenariokit/    # Host-aware scenario SDK (Rasm.ScenarioKit)
│   ├── testkit/        # Host-free adversarial law substrate (Rasm.TestKit)
│   ├── scenarios/      # Scenario content home (Rasm.Scenarios)
│   ├── tools/          # Infra suites: rhino-bridge Contract/Supervisor
│   └── libs/           # Per-package suites mirroring libs/dotnet
├── python/
│   ├── .api/           # Dev-tool API catalogs the kit and suites compose
│   ├── testkit/        # Project-agnostic kit: spec/strategies/doubles/env/bench/laws/runtime
│   └── libs/           # Per-package suites mirroring libs/python
└── typescript/
    ├── .api/           # Dev-tool API catalogs the kit and suites compose
    ├── testkit/        # @rasm/ts-testkit: laws, arbitraries, harness, bench, telemetry
    └── libs/           # Per-package suites mirroring libs/typescript
```

[CASING_LAW]:
- Tier, grouping, and kit directories are lowercase.
- PascalCase begins at a C# project boundary and continues through source folders inside it; grouping directories above a project stay lowercase.
- Spec files follow the owning language's source casing.

[KIT_LAW]:
- Shared test logic lives in exactly one per-language `testkit`, C# adding `scenariokit` for the host-aware scenario SDK.
- Kits never live under `libs/` — libs is the production plane.

## [02]-[LANES]

Test lanes are orthogonal to language; every suite declares its lane through the owning route, never through folder improvisation:

| [INDEX] | [LANE]      | [BOUNDARY]                                                | [ROUTE]                                                |
| :-----: | :---------- | :-------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | unit        | in-process, deterministic time, no sockets                | default test run per language                          |
|  [02]   | property    | generated-input law over a unit subject                   | `Spec.ForAll`, `@spec` + Hypothesis, `it.effect.prop`  |
|  [03]   | integration | real process/IO boundary: subprocess, loopback            | `network`/`subprocess` (Py); boundary suites elsewhere |
|  [04]   | benchmark   | measurement in a separate session, never inside unit runs | `-m benchmark`, bench include glob                     |

Lane vocabulary reserves `integration` for the real process/IO boundary. Tests running in-process with doubles are unit tests regardless of how many owners they span; calling one integration inflates the lane and hides the missing boundary proof.

## [03]-[PROOF_LAW]

Every test is an adversarial law with an independent oracle, never confirmation of current output. Oracles predict behavior from an independent source: closed-form math, conservation, fixture geometry, a category contract, runtime observation, or documented external behavior. Grade the proof before writing it — Grade A is an independent prediction, Grade B a metamorphic or model relation, Grade C a durable failure-category check, Grade D a shape-only inspection of values the test itself constructed. Grade D stands alone nowhere: it pairs with an A/B oracle or a C check, or it is deleted.

Every law family is witness-mandatory: registration carries a refuting witness the law must fail on. Witnesses the law survives expose a tautology no mutant can violate, and that registration is itself the failure.

[BANNED_SHAPES]:
- Existence tests: asserting a symbol, export, case, or member exists — the compiler, importer, or type checker already proves it.
- Mirror tests: asserting a constructed value's fields, re-implementing the production algorithm as oracle, or snapshotting a value the test built.
- Speculative-state tests: laws over states the production surface cannot construct.
- Per-function spam: one thin test per function when a single generated domain covers the family.

Failing laws are evidence: investigate the production owner before weakening the test, and when the law found a real bug, fix the owner — never dilute the law into shape-only proof.

## [04]-[ARTIFACT_ROUTING]

Every tool writes reports under `.artifacts/` and temp/work state under `.cache/<tool>/`; the repo root stays litter-free, and exact directories live in the owning configuration:

| [INDEX] | [TOOL]            | [SURFACE]                             | [ROUTE_OWNER]                                                            |
| :-----: | :---------------- | :------------------------------------ | :----------------------------------------------------------------------- |
|  [01]   | coverlet.MTP      | C# coverage                           | `--coverlet` on the run; lands beside results                            |
|  [02]   | MTP results       | C# test results, dumps, xunit reports | root `Directory.Build.targets` `--results-directory` per `IsTestProject` |
|  [03]   | pytest + coverage | Python coverage + caches              | `pyproject.toml` tool tables                                             |
|  [04]   | Hypothesis        | example database + observability      | `tests/python/testkit/runtime.py`                                        |
|  [05]   | pytest-benchmark  | Python benchmark storage              | `pyproject.toml` addopts                                                 |
|  [06]   | Vitest            | TS coverage + results + bench ledger  | root `vitest.config.ts`                                                  |
|  [07]   | StrykerJS         | TS mutation                           | `stryker.config.json`                                                    |
|  [08]   | Nx                | target outputs + cache                | `nx.json` targetDefaults                                                 |

Tool-admission litter rule: a change that admits or reconfigures any tool proves its caches and outputs land under `.cache/` or `.artifacts/` before it lands — through the tool's own documented configuration, config-file setting first, CLI flag second, never wrapper scripts or conftest shims. Gate: after the change's checks run, `git status` and a root listing show zero new root entries.

## [05]-[EXTENSION_PROTOCOL]

Every new suite, kit capability, fixture, or corpus asset has exactly one home; extending the canonical owner always beats adding a sibling, and an owner whose shape is no longer the densest is rebuilt ground-up, never accreted around:

| [INDEX] | [ADDITION]                  | [HOME]                                                                        |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | C# scenario                 | `tests/dotnet/scenarios`                                                      |
|  [02]   | C# kit capability           | `tests/dotnet/testkit` (host-free) or `tests/dotnet/scenariokit` (host-aware) |
|  [03]   | C# infra-tool suite         | `tests/dotnet/tools/<tool>/`                                                  |
|  [04]   | Python per-package suite    | `tests/python/libs/<package>/`                                                |
|  [05]   | Python kit capability       | the owning module in `tests/python/testkit`                                   |
|  [06]   | Python dev-tool API catalog | `tests/python/.api/`, one catalog per dev-plane package                       |
|  [07]   | TS unit spec                | beside its source in `libs/typescript`                                        |
|  [08]   | TS kit capability           | `tests/typescript/testkit`                                                    |
|  [09]   | TS dev-tool API catalog     | `tests/typescript/.api/`, one catalog per dev-plane package                   |

Per-package mirror law: where the ecosystem separates tests from source, suite homes mirror the production tree — Python suites under `tests/python/libs` mirror `libs/python`, and a C# package suite lands beside the kit when the first library ships. TS unit specs instead colocate beside source per the vitest idiom, so `tests/typescript/` never hosts unit specs.

## [06]-[SCENARIO_PIPELINE]

Scenario proof flows through one route, content to verdict:

`Rasm.Scenarios` owns methods marked with `[RhinoScenario]`; `Rasm.ScenarioKit` owns the Rhino/GH2 document brackets, assertions, scratch routing, and captures those methods compose. The supervisor stages that closure, Cargo discovers and runs it inside Rhino, and the returned `SessionEnvelope` carries the verdict, lifecycle facts, cleanup proof, and artifact index. [tools/rhino-bridge/README.md](../tools/rhino-bridge/README.md) owns the live command and failure-reading contract.

`Contract` and `Supervisor` suites under `tests/dotnet/tools/rhino-bridge` prove the wire contract and the supervisor fold that this pipeline rides; a bridge protocol change lands with its suite change or it does not land.

## [07]-[GATE_OWNERSHIP]

- StrykerJS policy lives in the root `stryker.config.json`.
- Zero mutant discovery is a failed run for the C# and TS Stryker lanes, never a green pass.
- Coverage aggregates as cobertura (C#) and lcov (Python, TS) under `.artifacts/` — each language-native reporter owns its output shape.

## [08]-[TOOLING_AWARENESS]

Before touching any testing surface, an agent checks the owners that carry the facts:

| [INDEX] | [SURFACE]                                             | [CARRIES]                                                                     |
| :-----: | :---------------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `Directory.Packages.props`                            | C# test-stack pins                                                            |
|  [02]   | each suite `.csproj` + root `Directory.Build.targets` | the suite's declared MTP runner and packages; the derived results routing     |
|  [03]   | root `pyproject.toml`                                 | Python test dependencies, pytest/coverage policy, markers                     |
|  [04]   | `pnpm-workspace.yaml`                                 | TS catalog pins, peer-rule resolutions, workspace package globs               |
|  [05]   | Forge `dev-tools.nix`                                 | dotnet global tools (stryker, coverage, reportgenerator, diagnostics) on PATH |
|  [06]   | `vitest.config.ts` + `stryker*.json` + `nx.json`      | TS runner defaults, artifact outputs, Stryker configs, project-graph targets  |
