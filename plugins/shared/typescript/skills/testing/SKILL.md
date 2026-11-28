---
name: testing
description: "Use when writing, running, or debugging TypeScript tests, covering Vitest and @effect/vitest, isolation options, type tests, benchmarks, controlled time, in-memory contract clients, service doubles, generated inputs, fixtures, browser and Workers suites, containers, and Playwright"
---

# [TYPESCRIPT_TESTING]

Covers suites under `tests/` that Vitest runs through root `vitest.config.ts` and Playwright through each suite's `playwright.config.ts`. Use `code:test` for test design: oracles, cases, structure, independence, doubles, generated-input kinds, and grading.

Tests use each module below before a hand form:

| [INDEX] | [FIELD]                     | [MODULES]                                                                                              |
| :-----: | :-------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | Runner, assertions          | `vitest`, `@effect/vitest` (`it`, `layer`, `assert`)                                                   |
|  [02]   | Controlled time and console | `TestClock`, `TestConsole` of `effect/testing`                                                         |
|  [03]   | Schema codecs               | `TestSchema.Asserts` of `effect/testing`                                                               |
|  [04]   | Generated inputs            | `effect/Arbitrary` through `it.prop` and `it.effect.prop`                                              |
|  [05]   | Contract clients, servers   | `HttpApiTest`, `NodeHttpServer.layerTest`, `McpTest`, `RpcTest`, `GraphQLClient.layerTest`, `ChatTest` |
|  [06]   | Cluster, workflow           | `Entity.makeTestClient`, `TestRunner.layer`, `WorkflowEngine.layerMemory`                              |
|  [07]   | Platform doubles            | `FileSystem.make`, `Stdio.layerTest`, `Terminal.make`, `ChildProcessSpawner.make`                      |
|  [08]   | Services in containers      | `testcontainers`, `@testcontainers/*`                                                                  |
|  [09]   | Chromium                    | `@vitest/browser-playwright`, `vitest-browser-react`, `@playwright/test`, `@axe-core/playwright`       |
|  [10]   | Firefox                     | `selenium-webdriver`                                                                                   |
|  [11]   | Workers runtime             | `@cloudflare/vitest-plugin` (`cloudflare:test`, `cloudflare:workers`)                                  |
|  [12]   | Shared harnesses            | `@rasm/testing`                                                                                        |

[REFERENCES]:
- [01]-[CLIENTS](references/clients.md): In-memory contract clients, servers on an ephemeral port, routed `fetch`, and what each client encodes
- [02]-[DOUBLES](references/doubles.md): Platform service doubles, command runs over given arguments, model doubles, and module mocks
- [03]-[GENERATED](references/generated.md): Schema-derived arbitraries, property checks, seeds, replay tokens, shrinking, and codec round trips
- [04]-[BROWSER](references/browser.md): Vitest browser mode, Storybook, Playwright fixtures, extensions, accessibility, store media, and WebDriver
- [05]-[ENVIRONMENTS](references/environments.md): Containers, Workers bindings, conformance programs, reference implementations, and Nx workspaces
- [06]-[VITEST](references/vitest.md): Isolation options, type tests, and benchmarks

## [01]-[RUNNING]

`nx test <project>` runs the `test--vitest` or `test--playwright` target `@rasm/nx-vitest` and `@rasm/nx-playwright` infer for each suite:
- Root `vitest.config.ts` builds a Vitest project from each workspace manifest naming `vitest`, its dev dependencies choosing the environment
- Node suites sharing a configuration and options run in one `vitest run` batch, and a browser suite runs alone
- Words after `--` join `vitest run` (`nx test <project> -- -t '<title>'`)
- Suites run with `CI=false`, and suites depending on Nx with `NX_DAEMON=false`
- `test--vitest` sets `allowOnly: false` and `update: 'none'` unless words after `--` set them, so a `.only` test or a missing snapshot fails
- `testTimeout` and `hookTimeout` hold 15 minutes as a hang bound alone, so each wait a test makes states its own deadline
- `vitest run <file> --inspect-brk --no-file-parallelism` at the root waits for a debugger on port 9229, and its workers start without `--process-timeout`
- `silent: 'passed-only'` prints the console of failing tests, and `truncateThreshold: 0` prints each compared value whole
- `flakyTest` of `@effect/vitest` and the Vitest `retry` option turn a flaky failure green and stay out of every suite

Dev dependencies select a suite's environment, first row winning:

| [INDEX] | [DEPENDENCY]                | [ENVIRONMENT]                                       |
| :-----: | :-------------------------- | :-------------------------------------------------- |
|  [01]   | `@storybook/addon-vitest`   | Stories as tests in headless Chromium               |
|  [02]   | `vitest-browser-react`      | Components in headless Chromium through Playwright  |
|  [03]   | `@cloudflare/vitest-plugin` | `*.workers.test.ts` in workerd, other files in Node |
|  [04]   | `vitest`                    | Node                                                |

Vitest suites with a browser provider and Playwright suites take tag `host:browser`, and `@rasm/nx-docker` tags `host:docker` each project with a manifest naming `testcontainers` or `@pulumi/docker`. Use [ENVIRONMENTS](references/environments.md) for containers and the engine they run on.

## [02]-[EFFECT_TESTS]

`@effect/vitest` registers effects as tests, each in a scope closing when its test ends:

| [INDEX] | [MEMBER]                               | [SERVICES]                         | [USE]                                     |
| :-----: | :------------------------------------- | :--------------------------------- | :---------------------------------------- |
|  [01]   | `it.effect`                            | `TestClock`, `TestConsole`         | Logic over time, retries, schedules, logs |
|  [02]   | `it.live`                              | Live services                      | Sockets, processes, files, real deadlines |
|  [03]   | `layer(L, options)(name, (it) => ...)` | `L`, test services unless excluded | Services built once for a block           |
|  [04]   | `it.effect.each(rows)`                 | As `it.effect`                     | One case per row through Vitest `it.for`  |
|  [05]   | `it.prop`, `it.effect.prop`            | As their tester, no fixtures       | Properties over arbitraries               |
|  [06]   | `makeMethods(test.extend(...))`        | Vitest fixtures                    | Effect tests reading fixtures             |
|  [07]   | `beforeAll`, `beforeEach`              | Hook scope                         | Effect setup released after suite or test |

- Blocks of `layer` offer `it.effect` and no `it.live`, and `excludeTestServices: true` runs their tests on live services, the form for sockets, containers, and processes
- Layer builds die on failure, failing every test of a block, and `{ buildPerTest: true }` builds per test, so a failure fails that test alone
- Nested `it.layer` blocks build into a fork of their parent's memo map, reusing services the parent built
- Bodies are `Effect.fnUntraced(function* () { ... })` generators, and `describe(<function>, ...)` titles a block with the function under test
- Row titles interpolate fields (`'$case refuses $input'`) and primitives (`%s`), so a failure names its row

Assertions compare observed facts against values written in the test:
- `assert.deepStrictEqual(<facts>, <want>)` compares one tuple or record of facts against a literal, printing both sides
- Absence and failure compare against a constructed case (`Option.none()`, `Exit.fail(error)`), and `@effect/vitest/utils` members (`assertSome`, `assertFailure`, `assertExitFailure`) narrow the type beside the comparison
- `Effect.flip` returns an expected failure, and the test asserts its `reason._tag` and subject, as an assertion that something failed passes for every wrong failure
- `Effect.exit` keeps defects and interruptions for an `Exit` or `Cause` assertion
- `addEqualityTesters()` registers no tester in 4.1.6, so no assertion reads `Equal`, and values `Equal` compares (`HashSet`, `HashMap`) convert to sorted arrays first
- `expect.schemaMatching(Schema.toStandardSchemaV1(S))` accepts any value `S` decodes, for a field holding a value of one run (a span's instants)

## [03]-[TIME]

`TestClock` starts at epoch 0 and moves through `TestClock.adjust(duration)` and `TestClock.setTime(millis)` alone, each running every sleep due by the new time in order:
- Effects that sleep fork first (`Effect.forkChild`), the test moves the clock, then joins the fiber and asserts
- Absence holds exactly once the clock passes every deadline: `reads a failing peer identity once and dials no more` adjusts 1,000 redial intervals and asserts one dial
- Instants a check compares set through `setTime` (`acceptance.test.ts` signs at `signedAt` and verifies at each tolerance offset)
- `TestClock.withLive(effect)` runs one effect on the live clock inside `it.effect`
- Sleeps never advanced log `A test is using time, but is not advancing the test clock` after one second, naming the hang
- `TestConsole.logLines` and `errorLines` return what `Console` wrote, for code whose output is its behavior
- Effect code takes time from the `Clock` service, so its tests move `TestClock` in place of `vi.useFakeTimers`

## [04]-[ASYNC]

Async tests await each event they assert and observe absence after its source ends:
- Doubles complete a `Deferred` the test awaits in place of a sleep or poll
- Spies are a `Queue` or `Ref` a double writes, read once the source under test ends, so a count of zero is exact
- `it.live` bounds each wait through `Effect.timeoutOrElse`, failing with a reason naming what never arrived
- Browser and Playwright tests wait on a DOM state through `expect.poll` and `expect.element`, which retry until their deadline
- `onTestFinished` releases what a test outside Effect opened, in reverse registration order

## [05]-[FIXTURES]

Tests own what they create under a name no other test uses, released when their scope ends:
- Files sit under `fs.makeTempDirectoryScoped()` in Effect and `mkdtempDisposableSync` outside it, and servers listen on port 0
- Handles release through `Effect.acquireDisposable` or `Effect.acquireRelease` in the test's scope, and through `onTestFinished` outside Effect
- Variables `vi.stubEnv` sets reset through `vi.unstubAllEnvs`
- `test.extend('<name>', { scope }, fn)` declares a fixture returning its value with `onCleanup` releasing it, scope `'test'`, `'file'`, or `'worker'`
- Fixtures and tests reading fixtures destructure their first parameter, `({}, use)` reading none, as Vitest and Playwright refuse other forms

`@rasm/testing` holds harnesses more than one suite reads:

| [INDEX] | [EXPORT]                   | [HOLDS]                                                                                           |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `@rasm/testing/nx`         | `Workspace`, `Workspace.scoped`, `bounded`, `it` with `workspace`, `fileWorkspace`, and `scratch` |
|  [02]   | `@rasm/testing/http`       | `transport(app)`, an `HttpClient` layer whose `fetch` runs an app in process                      |
|  [03]   | `@rasm/testing/playwright` | `config`, `it` with `axe`, `loaded` with an unpacked extension, `media`, `resource`               |
|  [04]   | `@rasm/testing/errors`     | `refused(pattern)`, a match on an Nx plugin error's nested messages                               |
|  [05]   | `@rasm/testing/path`       | `windows(actual)`, `node:path` with Windows separators for `vi.mock`                              |

## [06]-[SUITES]

Suites are private packages `@rasm/test-<name>`, one per tested project:
- Manifests name in `devDependencies` every package a suite imports, its product through `workspace:*`
- `"imports": { "#/*": "./*.ts" }` reaches a suite's own helpers as `#/<file>`, and products import by package name
- Rows and fixtures one suite shares across its files sit beside them (`fixtures.ts`, `rows.ts`), and a harness a second suite needs moves to `@rasm/testing`
- Files end `.test.ts` or `.test.tsx` for Vitest, `.workers.test.ts` for workerd, `.spec.ts` for Playwright, and `.media.ts` for store media
