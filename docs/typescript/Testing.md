# [TESTING]

Vitest in an Effect-based pnpm and Nx monorepo.

## [01]-[PRINCIPLES]

- Vitest runs unit, integration, and process-level tests
- Shared test options sit in one file, and each config states only what differs from it
- Tests import their package by name, so they exercise `exports` as a consumer reads them
- Vitest strips types without checking them
- Real dependencies and temporary folders replace module mocks
- Effect code swaps a service through a test layer
- Scripts, agents, and CI run `vitest run`, as `vitest` alone watches files in an interactive terminal
- Tests stay out of published output through their own package or a folder no emitting program includes
- Expensive suites (containers, remote services) take a file suffix (`*.integration.test.ts`) and run where a variable enables them

## [02]-[CONFIG_LAYOUT]

Vitest facts that decide the layout:
- Vitest 5 searches no parent folder for a config, so runs start at the workspace root or pass `--config`
- Inline projects in `test.projects` inherit every root option, as `extends` defaults to `true` from Vitest 5.0
- Projects named by file or glob inherit nothing from root, so each merges shared options itself
- `name` and `projects` are never inherited, and root `globalSetup` reaches no project
- Options Vitest's reference marks root-only (`coverage`, `reporters`, `passWithNoTests`, `silent`, `attachmentsDir`) act in root config alone
- Root config counts as a project only when `test.projects` lists it
- Inline projects setting `root`, `alias`, `plugins`, or `resolve` each start their own Vite server in place of the shared one
- `mergeConfig` concatenates arrays, so `include`, `exclude`, and `setupFiles` append to shared values
- `--project <name>` runs one project, repeats, and takes `*` and `!` patterns

Nx facts that decide the layout:
- `@nx/vitest` infers `test` for a folder holding its own `vitest.config.*`, or a `vite.config.*` with a `test` block
- `@nx/vitest` registers no project for a root config whose `test.projects` is an array, so it infers no target from inline projects
- `ciTargetName` on its plugin row splits each config's target into one target per test file
- Inline projects reach Nx through a workspace plugin reading the resolved root config, one `test` target per inline project
- Each inferred target runs `vitest run --project <name>` from the workspace root, on the Nx project at that inline project's `root`
- Inferred targets take root `vitest.config.ts` as an input, so editing shared options invalidates every cached result

Root `vitest.config.ts` holds one shared options object and one inline project per package:

```ts
import { defineConfig, mergeConfig, type ViteUserConfig } from 'vitest/config';

const shared: ViteUserConfig = { test: { include: ['test/**/*.test.ts'], sequence: { concurrent: true } } };

export default defineConfig({
    test: {
        projects: [
            mergeConfig(shared, { root: 'packages/core', test: { name: '@scope/core' } }),
            mergeConfig(shared, { root: 'packages/browser', test: { name: '@scope/browser', environment: 'happy-dom' } }),
        ],
    },
});
```

- `root` names the package folder, and `test.name` the package name `--project` selects
- Shared options stay out of root `test`, as inherited arrays and merged arrays then hold each entry twice
- Per-package differences (`environment`, `isolate`, `include`) sit in that package's project object alone
- `coverage` and `reporters` sit beside `projects` in root `test`, one report for the whole run
- Lists a project replaces in place of extending take an assignment after `mergeConfig` (`merged.test.exclude = [...]`)

## [03]-[ENVIRONMENTS]

| [INDEX] | [ENVIRONMENT]           | [USE]                                                               |
| :-----: | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `node`                  | Default for libraries, servers, CLIs                                |
|  [02]   | `happy-dom`, `jsdom`    | DOM logic without layout, set per project                           |
|  [03]   | Browser mode            | Real layout, focus, and browser APIs, provider as a factory call    |
|  [04]   | Platform pool or plugin | Runtimes such as workerd, through the platform's Vitest integration |

- `// @vitest-environment <name>` at a file's top overrides one file
- Pool `forks` is the default, and native addons that are not thread-safe need it
- `isolate: false` reuses one module graph across files that share no state
- `sequence.groupOrder` orders projects that compete for one resource (a database container)
- `fileParallelism: false` serializes files that touch tracked fixtures or git state
- Ports come from the operating system (port `0`)
- Waits poll until a deadline in place of a fixed sleep

## [04]-[SOURCE_RESOLUTION]

- `paths` in tsconfig reach Vite only with `resolve.tsconfigPaths: true`, and Node never reads them
- Vitest uses the `import` and `default` conditions by default
- `ssr.resolve.conditions` sets conditions for the `node` environment, `resolve.conditions` for browser, `happy-dom`, and `jsdom`
- Modules Node loads natively take a condition through `--conditions=<name>` in `test.execArgv`
- `require()` goes to Node and skips Vite plugins, aliases, transforms, and mocks
- Files both imported and required run twice

## [05]-[COVERAGE]

- `v8` is the default provider, and `istanbul` its alternative
- Default reporters are `text`, `html`, `clover`, and `json`, written to `./coverage`
- `coverage.thresholds` takes a number per `lines`, `functions`, `branches`, and `statements`, and `100: true` sets all four to 100
- `thresholds.perFile` and `thresholds.autoUpdate` default to `false`
- Glob-keyed thresholds inherit neither the global thresholds nor `perFile`
- Per-project test tasks pass each its own `--coverage.reportsDirectory`, and a merged report takes its own step

## [06]-[TYPE_TESTS]

| [INDEX] | [FORM]             | [FILES]       | [RUN]                                               |
| :-----: | :----------------- | :------------ | :-------------------------------------------------- |
|  [01]   | Vitest `typecheck` | `*.test-d.ts` | `typecheck.enabled`, or `--typecheck` on one run    |
|  [02]   | TSTyche            | `*.tst.ts`    | `tstyche`, with `--target` naming compiler versions |

- Runtime tests cover executed behavior, type tests cover inference, assignability, rejection, and displayed types
- `expectTypeOf` and `assertType` state type assertions, which Vitest analyzes and never runs
- `typecheck.checker` defaults to `tsc`, run with `--noEmit`, which resolves to TypeScript 7 where `@typescript/native` provides `tsc`
- `typecheck.tsconfig` defaults to the nearest `tsconfig.json`
- Source errors fail type tests unless `typecheck.ignoreSourceErrors` is on

## [07]-[BENCHMARKS]

```ts
import { test } from 'vitest';

test('decode', async ({ bench }) => {
    await bench('decode', () => decode(input)).run();
});
```

`decode` and `input` are the function and value under measurement:
- Benchmarks sit in `*.bench.ts`, matched by `benchmark.include`, and the `bench` fixture throws in any other file
- `vitest bench` runs benchmarks alone, and `benchmark.enabled: true` runs them after regular tests
- `bench.compare()` interleaves iterations of each case and prints one table
- `writeResult` stores a result as JSON, and `bench.from()` reads it back for comparison
- `expect(a).toBeFasterThan(b, { delta })` asserts a relative difference
- Benchmarks import built output, as Vite's module runner wraps each cross-module call of source in a getter
- CI gates deterministic quantities alone: bundle bytes against the base, `tsc --extendedDiagnostics` instantiation counts
- Wall time needs CPU simulation, a dedicated machine, or base and head runs paired in one job
- Process-level timing runs `hyperfine --warmup <n> --min-runs <n> --export-json <file>` over the built CLI
- Benchmark tasks depend on the build they measure and stay out of task caching

## [08]-[REGRESSIONS]

| [INDEX] | [WRONG]                                                      | [CORRECT]                                                 |
| :-----: | :----------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Config file per package with copied options                  | One root config, one inline project per package           |
|  [02]   | Expecting `@nx/vitest` targets from inline projects          | Workspace plugin inferring one target per inline project  |
|  [03]   | Config builders with a merge of their own                    | `mergeConfig` from `vitest/config`                        |
|  [04]   | Suite path tables beside the project list                    | Package options inside that package's project object      |
|  [05]   | Suite configs reading `nx.json` or plugin internals          | Options from Vitest's own configuration                   |
|  [06]   | `vitest.workspace.ts`                                        | `test.projects`                                           |
|  [07]   | `globals: true` with no `vitest/globals` types               | Imports from `vitest`                                     |
|  [08]   | `allowOnly: true`, global `retry` as a flake net             | Stray `.only` fails, flaky tests get fixed at their cause |
|  [09]   | Top-level `import { bench } from 'vitest'`                   | `bench` fixture inside `test()`                           |
|  [10]   | Aliases repeated in Vite and tsconfig with differing targets | `exports` and `imports`, which every resolver reads       |
|  [11]   | Benchmark numbers pasted into prose and never rerun          | Stored results compared by `bench.from()`                 |
