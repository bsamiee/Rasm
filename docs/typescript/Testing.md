# [TESTING]

Vitest and Playwright suites of the TypeScript packages Nx tests.

## [01]-[PRINCIPLES]

Rules every TypeScript suite follows:
- Vitest runs unit, integration, and process-level tests
- Shared test options sit in one file, and each config states only what differs from it
- Tests import their package by name and exercise `exports` as a consumer reads them
- Real dependencies and temporary folders replace module mocks
- Effect code swaps a service through a test layer
- Scripts, agents, and CI run `vitest run`, as `vitest` alone watches files in an interactive terminal
- Tests stay out of published output through their own package or a folder no emitting program includes
- `allowOnly` defaults to `!process.env.CI`, runs outside CI alone write new snapshots, and a `.only` test or a missing snapshot fails a CI run

## [02]-[CONFIG_LAYOUT]

Vitest facts that decide the layout:
- Vitest searches no parent folder for a config, and runs start at the workspace root or pass `--config`
- Inline projects in `test.projects` inherit every root option, as `extends` defaults to `true`
- Projects named by file or glob inherit nothing from root and merge shared options themselves
- `name` and `projects` are never inherited, and root `globalSetup` reaches no project
- Options Vitest's reference marks root-only (`coverage`, `reporters`, `passWithNoTests`, `silent`, `attachmentsDir`) act in root config alone
- Root config counts as a project only when `test.projects` lists it
- Inline projects setting `root`, `alias`, `plugins`, or `resolve` each start their own Vite server in place of the shared one
- `mergeConfig` concatenates arrays, and `include`, `exclude`, and `setupFiles` append to shared values
- `--project <name>` runs one project, repeats, and takes `*` and `!` patterns

Nx facts that decide the layout:
- `@rasm/nx-vitest` reads the resolved root config and infers one `test--vitest` target per Nx project holding an inline project's `root`
- Each inferred target runs the inline projects rooted in its Nx project
- Projects holding an enabled browser take tag `host:browser` and their provider's name (`playwright`) as a technology beside `vitest`
- One `nx` run batches every target of one config into one `vitest run` with one worker pool, and its own projects' test modules decide each target
- Inferred targets take root `vitest.config.ts` as an input, and editing shared options invalidates every cached result

Root `vitest.config.ts` derives its inline projects from the tree:
- Workspace packages with `vitest` in `devDependencies` and not in `peerDependencies` are one project each, rooted there and named by manifest `name`
- Suites listing `@cloudflare/vitest-plugin` in `devDependencies` are projects `<name> workers` and `<name> node`, split by file name
- Projects `<name> workers` run `*.workers.test.ts` through `cloudflareTest` over their suite's `cloudflare.config.ts`, `<name> node` other files
- Suites listing `vitest-browser-react` in `devDependencies` run in browser mode on Playwright Chromium
- Packages listing `@storybook/addon-vitest` in `devDependencies` add a browser project through `storybookTest`, named by package
- Suites listing `nx` or `@nx/devkit` in `devDependencies` run with the Nx daemon off, workspace data in `.nx/workspace-data`, and color off
- Projects write browser screenshots and traces beside the resolved `attachmentsDir`, and a lone `@rasm/nx-vitest` target sets it under `outputPath`
- Target batches write them once beside root `attachmentsDir`, and each project's attachments to its own target's `outputPath`
- Root `test` writes one report for the whole run, and inline projects inherit its `chaiConfig`

## [03]-[ENVIRONMENTS]

Environment per runtime:

| [INDEX] | [ENVIRONMENT]           | [USE]                                                      |
| :-----: | :---------------------- | :--------------------------------------------------------- |
|  [01]   | `node`                  | Default for libraries, servers, CLIs                       |
|  [02]   | Browser mode            | DOM, layout, focus, browser APIs, provider as factory call |
|  [03]   | Platform pool or plugin | Runtimes (workerd) through platform Vitest integration     |

Environment and isolation rules:
- Browser mode launches Chromium, and `[deps.playwright]` installs it for the locked Playwright before `mise exec` on the owner's Mac
- `// @vitest-environment <name>` at a file's top overrides one file
- Pool `forks` is the default, and native addons that are not thread-safe need it
- `isolate: false` reuses one module graph across files that share no state
- `sequence.groupOrder` orders projects that compete for one resource (a database container)
- `fileParallelism: false` serializes files that touch tracked fixtures or git state
- Ports come from the operating system (port `0`)
- Waits poll until a deadline in place of a fixed sleep

## [04]-[SOURCE_RESOLUTION]

Module resolution under Vitest and Node:
- `paths` in tsconfig reach Vite only with `resolve.tsconfigPaths: true`, and Node never reads them
- Vitest uses the `import` and `default` conditions by default
- `ssr.resolve.conditions` sets conditions for the `node` environment, `resolve.conditions` for browser mode
- Modules Node loads natively take a condition through `--conditions=<name>` in `test.execArgv`
- `require()` goes to Node and skips Vite plugins, aliases, transforms, and mocks
- Files both imported and required run twice

## [05]-[COVERAGE]

Coverage provider defaults and thresholds:
- `v8` is the default provider, and `istanbul` its alternative
- `coverage.thresholds` takes a number per `lines`, `functions`, `branches`, and `statements`, and `100: true` sets all four to 100
- `thresholds.perFile` and `thresholds.autoUpdate` default to `false`
- Glob-keyed thresholds inherit neither the global thresholds nor `perFile`
- Per-project test tasks pass each its own `--coverage.reportsDirectory`, and a merged report takes its own step

## [06]-[TYPE_TESTS]

Type tests and their checker:
- Vitest `typecheck` runs `*.test-d.ts` files under `typecheck.enabled`, or `--typecheck` on one run
- Runtime tests cover executed behavior, type tests cover inference, assignability, rejection, and displayed types
- `expectTypeOf` and `assertType` state type assertions Vitest analyzes and never runs
- `typecheck.checker` defaults to `tsc`, run with `--noEmit`
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
- Gates compare deterministic quantities alone: bundle bytes against the base, `tsc --extendedDiagnostics` instantiation counts
- Wall time needs CPU simulation, a dedicated machine, or base and head runs paired in one job
- Process-level timing runs `hyperfine --warmup <n> --min-runs <n> --export-json <file>` over the built CLI
- Benchmark tasks depend on the build they measure and stay out of task caching

## [08]-[PLAYWRIGHT]

Playwright suites open `playwright.config.ts` with `config(import.meta.dirname)` of `@rasm/testing/playwright`:
- `@rasm/nx-playwright` infers tag `host:browser` and `test--playwright` per Playwright config, and reports and `test-results` go to `outputPath`
- Project `test` runs `*.spec.ts` and `*.test.ts` files, and project `media` runs `*.media.ts` files outside the default `testMatch`
- Project `media` writes to `<outputPath>/media` at a fixed scale factor, color scheme, locale, and zone, with traces and failure screenshots off
- `media(scenes)` registers a Chrome Web Store screenshot per scene and both promo tiles from the built manifest, `icons/mark.svg`, and `config.brand`
- Projects matching files with an `outputDir` under `outputPath` run in `pack--playwright` after `test--playwright`, the rest in `test--playwright`
- `pack--playwright` runs reporter `list` and outputs each `outputDir`, and `attach--gh` gets no earlier file, as Playwright empties them first
- `test--playwright` runs after `build--*` of its suite's dependencies and each reused `webServer` task, and a suite naming a product loads its build
- `loaded` launches one persistent Chromium context per worker and loads its suite's extension into it for each test over CDP
- `loaded` takes the one unpacked Manifest V3 extension among build outputs of its suite's dependencies
- `attach--gh` of a host package depends on `pack--*` of each workspace package depending on it and uploads the visible files they write
- Fixture `axe` of `it` scans the page through `AxeBuilder` at WCAG A and AA tags through 2.2
- Suites of a served product start its built Node bundle on an allocated port in a fixture, each test with its own database file and object storage
- Fixtures keep that state in a temporary folder they remove, object storage a Miniflare R2 bucket behind its S3 route
- No suite reads a developer's local state
- Suites sign in through an `ACCESS` source `Forwarded`, as the headers of a request state its user

## [09]-[WEBDRIVER]

WebDriver suites drive the installed Safari and Firefox, as Playwright and Vitest browser providers launch WebKit and Firefox builds that differ from them in the CSS they support:
- Suites are Vitest `node` projects tagged `host:browser` and `os:all` in their manifest, and `selenium-webdriver` starts one session per browser per file
- Firefox runs headless on every OS, and Safari on macOS alone through `safaridriver`
- Selenium Manager finds an installed Firefox and `geckodriver`, and downloads the stable release of each where a machine holds none
- Cases a Chromium and a WebDriver suite share are rows of viewer, width, in-page read, and expected value, exported by the Chromium suite
- Each case sets a viewport of its width by 900, loads the page as a new document, and polls its read until it equals the expected value
- Cases first assert a visible page, as Safari draws no frame while its window is covered, minimized, or on another desktop
- Firefox takes a viewport through WebDriver BiDi `browsingContext.setViewport`, as its window stops at 500px, and Safari through its window size
- Owner cases load through a proxy stating the owner's `Forwarded` headers, as WebDriver sets no request header
- Safari comes to the front once as its session opens, and the suite then activates the app that was in front
- Suites end their session, and quit Safari through its own quit where it was not running before
- `safaridriver --enable` and Safari's remote automation setting are one-time machine state, which GitHub-hosted macOS images hold
