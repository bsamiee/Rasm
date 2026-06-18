# [API_CATALOGUE] @stryker-mutator/core

`@stryker-mutator/core` supplies the programmatic mutation testing runner: `Stryker` (the main class with `runMutationTest()`) and `StrykerCli` (the command-line entry). Configuration flows through `PartialStrykerOptions`, a deep-partial projection of `StrykerOptions`, and results are returned as `MutantResult[]` — each result combining the initial `Mutant` shape with the full `schema.MutantResult` from `mutation-testing-report-schema`. The kill-ratio gate in the projection package invokes `Stryker.runMutationTest()` with a config object; `StrykerCli` drives the same run from `argv`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@stryker-mutator/core`
- package: `@stryker-mutator/core`
- module: `.` → `dist/src/index.js` (exports `Stryker`, `StrykerCli`, and `default Stryker`)
- asset: `dist/src/index.d.ts`
- rail: testing / mutation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: public exports from `@stryker-mutator/core`
- rail: testing / mutation
- entry: `@stryker-mutator/core`

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------- | :------------ | :------------------------------- |
|   [1]   | `Stryker`    | class         | programmatic mutation runner     |
|   [2]   | `StrykerCli` | class         | CLI entry point over `commander` |

[PUBLIC_TYPE_SCOPE]: configuration types (from `@stryker-mutator/api/core`)
- rail: testing / mutation
- entry: `@stryker-mutator/api/core`

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------- |
|   [1]   | `StrykerOptions`          | interface     | full configuration schema (all fields required)                         |
|   [2]   | `PartialStrykerOptions`   | type alias    | `DeepPartial<StrykerOptions>` for `Stryker` constructor                 |
|   [3]   | `CoverageAnalysis`        | string union  | `"off" \| "all" \| "perTest"`                                           |
|   [4]   | `LogLevel`                | string union  | `"off" \| "fatal" \| "error" \| "warn" \| "info" \| "debug" \| "trace"` |
|   [5]   | `MutationScoreThresholds` | interface     | `high`, `low`, `break` thresholds                                       |
|   [6]   | `CommandRunnerOptions`    | interface     | `{ command: string }`                                                   |
|   [7]   | `MutatorDescriptor`       | interface     | excludedMutations and plugin fields                                     |

[PUBLIC_TYPE_SCOPE]: result types (from `@stryker-mutator/api/core`)
- rail: testing / mutation

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------------- |
|   [1]   | `Mutant`             | interface     | initial mutant shape: `id`, `fileName`, `location`, `mutatorName`, `replacement` |
|   [2]   | `MutantTestCoverage` | type alias    | `Mutant` plus `coveredBy` and `static` fields                                    |
|   [3]   | `MutantResult`       | type alias    | `Mutant & schema.MutantResult` — final reportable result                         |
|   [4]   | `MutantStatus`       | string union  | re-export from `mutation-testing-report-schema/api`                              |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: programmatic runner
- rail: testing / mutation

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------- | :-------------- | :--------------------------------------------------------- |
|   [1]   | `new Stryker(cliOptions: PartialStrykerOptions)` | constructor     | creates runner with merged CLI + file config               |
|   [2]   | `Stryker#runMutationTest()`                      | instance method | runs full mutation test, returns `Promise<MutantResult[]>` |
|   [3]   | `Stryker.run(injector, args)`                    | static method   | internal; runs inside a prepared injector scope            |

[ENTRYPOINT_SCOPE]: CLI entry
- rail: testing / mutation

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :----------------------------------------- |
|   [1]   | `new StrykerCli(argv, program?, runMutationTest?, runMutationTestingServer?)` | constructor     | wires `commander` CLI to the runner        |
|   [2]   | `StrykerCli#run(createInjectorImpl?)`                                         | instance method | parses `argv` and dispatches to the runner |
|   [3]   | `guardMinimalNodeVersion(processVersion?)`                                    | guard function  | throws if Node version is below minimum    |

[ENTRYPOINT_SCOPE]: key `StrykerOptions` fields
- rail: testing / mutation

| [INDEX] | [SURFACE]          | [TYPE_FAMILY]                   | [DEFAULT]                         |
| :-----: | :----------------- | :------------------------------ | :-------------------------------- |
|   [1]   | `testRunner`       | `string`                        | `"command"`                       |
|   [2]   | `coverageAnalysis` | `CoverageAnalysis`              | `"perTest"`                       |
|   [3]   | `mutate`           | `string[]`                      | `["src/**/*.ts"]` pattern         |
|   [4]   | `checkers`         | `string[]`                      | `[]`                              |
|   [5]   | `reporters`        | `string[]`                      | `["clear-text","progress"]`       |
|   [6]   | `concurrency`      | `number \| string \| undefined` | `n-1` logical CPUs                |
|   [7]   | `thresholds`       | `MutationScoreThresholds`       | `{ high: 80, low: 60, break: 0 }` |
|   [8]   | `plugins`          | `string[]`                      | all `@stryker-mutator/*`          |
|   [9]   | `timeoutFactor`    | `number`                        | `1.5`                             |
|  [10]   | `incremental`      | `boolean`                       | `false`                           |

## [4]-[IMPLEMENTATION_LAW]

[MUTATION_TOPOLOGY]:
- `Stryker` is the single programmatic entry; instantiate with `PartialStrykerOptions` and call `runMutationTest()` — all fields in `PartialStrykerOptions` are optional via `DeepPartial`
- The returned `MutantResult[]` carries `status` (`Killed`, `Survived`, `NoCoverage`, `Timeout`, `CompileError`, `Ignored`, `Pending`) from `mutation-testing-report-schema/api`; the kill-ratio gate reads `status === "Killed"` over total non-ignored mutants
- `Stryker.run` is a `@internal` static; do not call it from consumer code — it receives an already-prepared dependency-injection scope
- `StrykerCli` is the binary entry (`stryker run`); the programmatic form is always `new Stryker(options).runMutationTest()` from CI scripts or test gates
- Plugin loading: all `node_modules/@stryker-mutator/*` packages are loaded by default; `plugins` overrides the list, `appendPlugins` extends it without clearing the default

[LOCAL_ADMISSION]:
- The kill-ratio gate installs `@stryker-mutator/vitest-runner` and `@stryker-mutator/typescript-checker` as peer plugins; both are declared in the workspace catalog alongside `@stryker-mutator/core`
- `stryker.config.mjs` (or `.cjs` / `.json`) in the project root is the configuration file; `PartialStrykerOptions` passed to `new Stryker()` merges with (and overrides) the file-based config
- `incremental: true` writes a `reports/stryker-incremental.json` artifact; CI gate reads `stryker.config.mjs` and runs `new Stryker({}).runMutationTest()` — no config argument needed when the file is present

[RAIL_LAW]:
- Package: `@stryker-mutator/core`
- Owns: mutation test execution, result aggregation, and `MutantResult[]` reporting
- Accept: `PartialStrykerOptions` at the `Stryker` constructor; plugin and runner identifiers from the workspace catalog
- Reject: direct use of internal `coreTokens`, `typed-inject` injector, or `dist/src/di` — these are implementation details, not the public surface
