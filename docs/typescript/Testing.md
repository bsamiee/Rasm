# [TESTING]

Vitest in an Effect-based pnpm and Nx monorepo.

## [01]-[PRINCIPLES]

- Vitest runs unit, integration, and process-level tests
- Shared test options sit in one file, and each config states only what differs from it
- Tests import their package by name, so they exercise `exports` as a consumer reads them
- Real dependencies and temporary folders replace module mocks
- Effect code swaps a service through a test layer
- Scripts, agents, and CI run `vitest run`, as `vitest` alone watches files in an interactive terminal
- Tests stay out of published output through their own package or a folder no emitting program includes

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
- `@rasm/nx-vitest` reads the resolved root config and infers one `test--vitest` target per Nx project holding an inline project's `root`
- Each inferred target runs `vitest run` from the workspace root with one `--project <name>` per inline project rooted in its Nx project
- Inferred targets take root `vitest.config.ts` as an input, so editing shared options invalidates every cached result

Root `vitest.config.ts` derives its inline projects from the tree:
- Folders holding the nearest `package.json` of a `tests/**/*.test.{ts,tsx}` file are one project each, rooted there and named by manifest `name`
- Suites holding `cloudflare.config.ts` run test files importing no `@effect/platform-node` through `cloudflareTest`
- Suites holding test files of both kinds are projects `<name> node` and `<name> workers`
- Suites listing `vitest-browser-react` in `devDependencies` run in browser mode on Playwright Chromium
- Each `{products,libs}/**/.storybook/main.ts` adds a browser project through `storybookTest`, named by its package
- Root `test` holds `coverage`, `reporters`, `silent`, and `projects`, one report for the whole run, and inline projects inherit its `chaiConfig`

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
