# [API_CATALOGUE] @stryker-mutator/core

`@stryker-mutator/core` is the programmatic mutation-testing runner: `Stryker` mutates the source under `mutate`, runs the test suite against each mutant through a pluggable `TestRunner`, and returns `MutantResult[]` — each result the initial `Mutant` shape merged with the `mutation-testing-report-schema` verdict (`status`). Configuration is `PartialStrykerOptions`, a `DeepPartial` of the full `StrykerOptions` schema, merged over the `stryker.config.mjs` file. `StrykerCli` wires the same runner to `commander` `argv`. `projection` drives it as the kill-ratio gate over `convergence/law` and `query/window`: `new Stryker({}).runMutationTest()` scores the `@effect/vitest` + `fast-check` property suite through the co-admitted `@stryker-mutator/vitest-runner`, and a mutated `conflictStep`/`windowFold` guard that survives the property laws fails the gate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@stryker-mutator/core`
- package: `@stryker-mutator/core` (9.6.1, Apache-2.0, © Stryker Team)
- module format: ESM only (`"type": "module"`); `.` → `dist/src/index.js`, `.d.ts` at `dist/src/index.d.ts`; barrel exports `Stryker`, `StrykerCli`, and `default Stryker`
- runtime target: Node `>=20.0.0`; a `devDependency` — the runner and its plugins never enter a shipped bundle
- config source: `StrykerOptions` and the config/result types resolve from `@stryker-mutator/api` (subpaths `./core`, `./test-runner`, `./check`, `./plugin`, `./report`, `./ignore`, `./logging`); the `status` schema is `mutation-testing-report-schema@3.7.3`
- plugins: the kill-ratio gate co-admits `@stryker-mutator/vitest-runner` (a `TestRunner`) and `@stryker-mutator/typescript-checker` (a `Checker`), both pinned `9.6.1` alongside core in the workspace catalog
- rail: proof / mutation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the barrel — the only two public classes
- rail: proof / mutation
- entry: `@stryker-mutator/core`

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------- | :------------ | :-------------------------------------------- |
|  [01]   | `Stryker`    | class         | programmatic runner; `runMutationTest()` → `Promise<MutantResult[]>` |
|  [02]   | `StrykerCli` | class         | `commander`-backed CLI entry over the same runner |

[PUBLIC_TYPE_SCOPE]: configuration types (from `@stryker-mutator/api/core`)
- rail: proof / mutation
- entry: `@stryker-mutator/api/core`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `StrykerOptions`          | interface     | the full configuration schema (every field required)                    |
|  [02]   | `PartialStrykerOptions`   | type alias    | `DeepPartial<StrykerOptions>` — the `Stryker` constructor argument       |
|  [03]   | `CoverageAnalysis`        | string union  | `"off" \| "all" \| "perTest"`                                           |
|  [04]   | `LogLevel`                | string union  | `"off" \| "fatal" \| "error" \| "warn" \| "info" \| "debug" \| "trace"` |
|  [05]   | `MutationScoreThresholds` | interface     | `{ high, low, break }` gate thresholds                                   |
|  [06]   | `MutatorDescriptor`       | interface     | `{ plugins, excludedMutations }` mutation selection                     |
|  [07]   | `CommandRunnerOptions` / `WarningOptions` | interface | command-runner config; per-warning enable flags               |

[PUBLIC_TYPE_SCOPE]: result types (`@stryker-mutator/api/core` × `mutation-testing-report-schema`)
- rail: proof / mutation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Mutant`             | interface     | initial mutant: `id`, `fileName`, `location`, `mutatorName`, `replacement`        |
|  [02]   | `MutantTestCoverage` | type alias    | `Mutant` + `coveredBy` + `static` coverage fields                                |
|  [03]   | `MutantResult`       | type alias    | `Mutant & schema.MutantResult` — the final reportable verdict                    |
|  [04]   | `MutantStatus`       | string union  | `"Killed" \| "Survived" \| "NoCoverage" \| "CompileError" \| "RuntimeError" \| "Timeout" \| "Ignored" \| "Pending"` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the programmatic runner — the gate path
- rail: proof / mutation

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `new Stryker(cliOptions: PartialStrykerOptions, injectorFactory?)` | constructor | merges CLI options over the `stryker.config.mjs` file; the injector factory is internal |
|  [02]   | `Stryker#runMutationTest()`                      | instance method | runs the full mutation test → `Promise<MutantResult[]>`   |
|  [03]   | `Stryker.run(injector, args)`                    | `@internal` static | receives an already-prepared DI scope — never call from consumer code |

[ENTRYPOINT_SCOPE]: the CLI entry
- rail: proof / mutation

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :----------------------------------------- |
|  [01]   | `new StrykerCli(argv, program?, runMutationTest?, runMutationTestingServer?)` | constructor     | wires `commander` to the runner (and, optionally, the mutation-testing server) |
|  [02]   | `StrykerCli#run(createInjectorImpl?)`                                         | instance method | parses `argv` (`stryker run`) and dispatches |

[ENTRYPOINT_SCOPE]: representative `StrykerOptions` fields (the schema is one owner; `PartialStrykerOptions` overrides the file)
- rail: proof / mutation

| [INDEX] | [FIELD]            | [TYPE]                          | [DEFAULT]                         |
| :-----: | :----------------- | :------------------------------ | :-------------------------------- |
|  [01]   | `testRunner`       | `string`                        | `"command"` (the gate sets `"vitest"`) |
|  [02]   | `coverageAnalysis` | `CoverageAnalysis`              | `"perTest"`                       |
|  [03]   | `mutate`           | `string[]`                      | `["{src,lib}/**/!(*.spec\|*.test).{js,ts,…}", "!{src,lib}/**/__tests__/**"]` — src/lib glob that excludes spec/test files |
|  [04]   | `checkers`         | `string[]`                      | `[]` (the gate sets `["typescript"]`) |
|  [05]   | `reporters`        | `string[]`                      | `["clear-text","progress","html"]` |
|  [06]   | `concurrency`      | `number \| undefined`           | `n-1` logical CPUs                |
|  [07]   | `thresholds`       | `MutationScoreThresholds`       | `{ high: 80, low: 60, break: null }` |
|  [08]   | `plugins` / `appendPlugins` | `string[]`             | all `@stryker-mutator/*` by default; `appendPlugins` extends without clearing |
|  [09]   | `timeoutFactor` / `timeoutMS` | `number`             | `1.5` / `5000`                    |
|  [10]   | `incremental` / `incrementalFile` | `boolean` / `string` | `false` / `reports/stryker-incremental.json` |

## [04]-[IMPLEMENTATION_LAW]

[MUTATION_TOPOLOGY]:
- `Stryker` is the single programmatic entry: instantiate with `PartialStrykerOptions` (all fields optional via `DeepPartial`) and call `runMutationTest()`. When `stryker.config.mjs` is present the CI gate passes `{}` — the file is the config, the argument only overrides.
- `MutantResult[]` carries `status`; the kill-ratio gate reads `status === "Killed"` over total non-`Ignored` mutants. `NoCoverage`, `Survived`, `Timeout`, `CompileError`, and `RuntimeError` are all non-killed — the gate must treat `RuntimeError`/`Timeout` as gaps, not silently as kills.
- `Stryker.run` is `@internal` (a prepared `typed-inject` scope); the public form is always `new Stryker(options).runMutationTest()`. `StrykerCli` is the binary path (`stryker run`).
- `incremental: true` writes `incrementalFile`; a second run reuses unchanged verdicts, so the gate over `convergence`/`query/window` only re-mutates changed law surface.

[PLUGIN_SURFACE]:
- Stryker is a plugin host typed by `@stryker-mutator/api/{test-runner,check,plugin}`: `@stryker-mutator/vitest-runner` implements `TestRunner` and `@stryker-mutator/typescript-checker` implements `Checker`. Both are `@stryker-mutator/*` packages, so they load by default; `plugins`/`appendPlugins` scope the set.
- the runner reuses the project's real Vitest config, so the mutation suite IS the `@effect/vitest` + `fast-check` property suite — no separate mutation harness. A new law kind (a `crdt` op arm, a `GraphFork` window) is covered the moment its property spec lands under the `mutate` glob.

[STACKING]:
- `@effect/vitest` + `fast-check`: the vitest-runner executes exactly the `it.prop` suites `convergence/law` and `query/window` author (see `fast-check.md`, `effect-vitest.md`); the mutation score is the strength signal on those generators — a mutant of the merge guard or the Z-set signed-delta fold must break a property law to be killed, so a thin arbitrary that never collides leaves mutants alive and drops the ratio below `break`.
- `@stryker-mutator/typescript-checker`: type-checks each mutant before it runs, turning a mutant that no longer type-checks into `CompileError` rather than a wasted test run — it shares the same `tsconfig` the folder builds against.
- gate shape: `new Stryker({ mutate: [...convergence, ...query/window] }).runMutationTest()` folds `MutantResult[]` into the kill ratio; `thresholds.break` fails CI. The config lives in `stryker.config.mjs` at the branch root, registered once with `vitest`/`typescript` plugins.

[RAIL_LAW]:
- package: `@stryker-mutator/core`
- owns: mutation-test execution, per-mutant scheduling over a `TestRunner`/`Checker`, and `MutantResult[]` aggregation
- accept: `PartialStrykerOptions` at the `Stryker` constructor; plugin/runner/checker identifiers from the workspace catalog; the file config as the base
- reject: direct use of `Stryker.run`, `coreTokens`, the `typed-inject` injector, or `dist/src/di` internals; a bespoke mutation harness where the vitest-runner already scores the property suite; treating `RuntimeError`/`Timeout` mutants as kills
