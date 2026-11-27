# [ENVIRONMENTS]

## [01]-[CONTAINERS]

Services a suite cannot run in process (PostgreSQL, ClickHouse, S3, sshd) start in containers through `testcontainers`, a module package (`@testcontainers/postgresql`) where one exists, or a suite `Dockerfile`:

| [INDEX] | [SOURCE]           | [FORM]                                                                     |
| :-----: | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | Module package     | `new PostgreSqlContainer(image)`, `new ClickHouseContainer(image)`         |
|  [02]   | Published image    | `new GenericContainer(image)` with ports, environment, and a wait strategy |
|  [03]   | Suite `Dockerfile` | `GenericContainer.fromDockerfile(dir).withBuildkit().build(name, opts)`    |

Started containers implement `Symbol.asyncDispose` as `stop()`, so `Effect.acquireDisposable` owns each one and scope close stops it. Suites build one container per file in an outer `layer` and give each test its own database in an inner block with `buildPerTest: true`:

```ts
class Postgres extends Context.Service<Postgres, { readonly container: StartedPostgreSqlContainer; readonly admin: SqlClient.SqlClient; readonly created: Ref.Ref<number> }>()('@rasm/test-database/postgres.test/Postgres') {
    static readonly layer = Effect.gen(function* () {
        const container = yield* Effect.acquireDisposable(Effect.promise(() => new PostgreSqlContainer('postgres:19.0-alpine3.24').withCommand(['postgres', '-c', 'fsync=off', '-c', 'synchronous_commit=off', '-c', 'full_page_writes=off']).withTmpFs({ '/var/lib/postgresql': 'rw' }).start()));
        const admin = Context.get(yield* Layer.build(PgClient.layer({ url: Redacted.make(container.getConnectionUri()) })), SqlClient.SqlClient);
        return { container, admin, created: yield* Ref.make(0) };
    }).pipe(Layer.effect(Postgres));
    static readonly database = Layer.unwrap(
        Effect.gen(function* () {
            const { container, admin, created } = yield* Postgres;
            const name = `test_${yield* Ref.updateAndGet(created, Number.increment)}`;
            yield* Effect.acquireRelease(admin`CREATE DATABASE ${admin(name)}`, () => Effect.orDie(admin`DROP DATABASE ${admin(name)} WITH (FORCE)`));
            return Pg.layer({ url: Redacted.make(Object.assign(new URL(container.getConnectionUri()), { pathname: name }).href) });
        }),
    );
}

layer(Postgres.layer, { excludeTestServices: true, concurrent: true })('postgres', (outer) => outer.layer(Postgres.database, { buildPerTest: true })(cases));
```

Data a test run discards needs no durability:
- `withTmpFs({ '<data dir>': 'rw' })` keeps a database's files in memory, and server flags turn off what protects them across a crash (`fsync`, `synchronous_commit`, `full_page_writes`)
- Databases a test creates drop with force at release (`WITH (FORCE)` on PostgreSQL, `SYNC` on ClickHouse), as an open connection blocks a plain drop
- `container.exec(words)` runs a command inside a started container, its `{ exitCode, output, stdout }` read before the test trusts it
- `withCopyFilesToContainer` and `withCopyContentToContainer` place keys and configuration a test generated before start

`start()` returns once a wait strategy passes, bounded by `withStartupTimeout(ms)` (60 seconds by default, 120 for `PostgreSqlContainer`):

| [INDEX] | [STRATEGY]                           | [READY_WHEN]                                         |
| :-----: | :----------------------------------- | :--------------------------------------------------- |
|  [01]   | None set                             | Health check of container or image, else ports below |
|  [02]   | `Wait.forListeningPorts()`           | Each exposed port accepts on host and in container   |
|  [03]   | `Wait.forHealthCheck()`              | Docker reports container healthy                     |
|  [04]   | `Wait.forHttp(path, port)`           | Path answers `response.ok`, or `forStatusCode(code)` |
|  [05]   | `Wait.forLogMessage(text, n)`        | Log holds text or `RegExp` match `n` times           |
|  [06]   | `Wait.forSuccessfulCommand(command)` | `/bin/sh -c <command>` exits 0 inside container      |
|  [07]   | `Wait.forAll([...])`                 | Every listed strategy passes                         |

Servers that listen before they serve take the signal they print when serving (`Wait.forLogMessage('Server listening on 0.0.0.0 port 22.')`, `Wait.forHttp('/health/ready', 9000)`), as an open port alone passes before requests succeed. Module packages set their own (`PostgreSqlContainer` waits for health check and ports).

Ports follow who binds them:
- Container ports declared through `withExposedPorts(port)` map to a free host port the engine chooses, and tests dial `container.getHost()` at `container.getMappedPort(port)`
- Programs a test starts that bind a port it names take a free number from `getPort()` of `get-port` (an `ssh -L` bind address, a served product)
- Listeners a test opens listen on port 0 and read the bound port from their address
- Concurrent tests binding ports inside one container lease them from a fixed exposed range through a `Queue` under `Effect.acquireRelease`, returning each at release
- Images a suite builds pass `{ deleteOnExit: false }` to `build`, keeping the image cached across runs outside the reaper's session

Projects reach an engine through Nx and mise alone:
- `@rasm/nx-docker` tags a project `host:docker` when its manifest names `testcontainers` or `@pulumi/docker`, or a direct dependency names one in its `dependencies`
- `nx.json` `targetDefaults` row `{test,install}--*` filtered to `tag:host:docker` adds `docker:install--docker` to `dependsOn`, which starts the Colima VM on macOS and leaves it running, and confirms the systemd engine answers on Linux
- `.config/mise.macos.toml` points `DOCKER_HOST` at Colima's socket, and `TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE` names `/var/run/docker.sock` inside the VM for the reaper container's mount

## [02]-[WORKERS]

Suites naming `@cloudflare/vitest-plugin` split into project `<name> node`, excluding `**/*.workers.test.ts`, and project `<name> workers`, running those files in workerd under `cloudflareTest({ config: true })`:
- `config: true` loads `cloudflare.config.ts` from the suite folder, a string names another path, and `miniflare` options merge over its Worker
- `config` refuses `wrangler` beside it, and config functions read `ctx.mode` as `test`
- Workers declaring `entrypoint` set `main`, and suites declaring none call their bindings directly

`cloudflare.config.ts` declares a Worker and its bindings through `cf/config`:

```ts
export default defineConfig({ worker: defineWorker({ name: 'database', compatibilityDate: '2026-10-01', env: { DB: bindings.d1() } }) });
```

Bindings reach tests through `env` of `cloudflare:workers`, and `env` and `SELF` of `cloudflare:test` are deprecated:
- `codegen--cf` writes `.cloudflare/types` through `cf workers types --include-runtime=false` from that file, and `test--vitest` depends on `codegen--*`
- `reset()` of `cloudflare:test` deletes every attached binding's data, and a layer built per test runs it first, so each test starts empty
- `cloudflare:test` also creates handler arguments (`createExecutionContext`, `createScheduledController`, `createMessageBatch`) and drives Durable Objects, D1 migrations, and Workflows

```ts
layer(d1Engine({ db: env.DB }).pipe(Layer.provide(Layer.effectDiscard(Effect.promise(reset)))), { buildPerTest: true, excludeTestServices: true })('d1', (it) => { ... });
```

Type programs split by runtime, and suite `tsconfig.json` holds `"files": []` and references both:
- `tsconfig.worker.json` takes `types` `@cloudflare/workers-types/experimental` and `@cloudflare/vitest-plugin/types`, and includes Workers tests, files they import, `cloudflare.config.ts`, and `.cloudflare/types`
- `tsconfig.node.json` takes `types` `node` and excludes `*.workers.test.ts`
- Cases both runtimes run sit in one file exporting a function of the differing service (`signatures(crypto)`), each runtime file passing its layer

## [03]-[PROGRAMS]

Programs implementing a contract independently serve as reference oracles, each pinned where its ecosystem pins tools:

| [INDEX] | [PROGRAM]                   | [PIN]                                  | [SUITE]              |
| :-----: | :-------------------------- | :------------------------------------- | :------------------- |
|  [01]   | `conformance` MCP scenarios | Lock of a mise `npm:` row              | `ai/mcp`             |
|  [02]   | Vendor webhook libraries    | Catalog `devDependencies` of the suite | `messaging/webhooks` |
|  [03]   | `sh`, `bash`, `zsh`, `dash` | Host OS                                | `system/shell`       |

`conformance server` checks a served MCP endpoint per scenario against a baseline of expected failures:
- Tests serve the endpoint on `127.0.0.1` port 0 and run `conformance server --url <url> --scenario <id> --expected-failures <yaml> --output-dir <dir>`
- `conformance` exits 1 on an unexpected failure and on a baseline entry that now passes, so a stale entry fails as a regression does
- `expected-failures.yaml` lists scenario ids under `server:`, each under a comment naming its cause outside the library
- Assertions read `checks.json` beside exit 0, as a scenario that ran no check also exits 0
- Suite `package.json` `nx.targets.test--vitest.inputs` adds `.config/mise/locks/npm-modelcontextprotocol-conformance/**/*`, so a new conformance release reruns the suite

Reference libraries exchange values with code under test in both directions:
- Reference verifies what code under test signs, and code under test verifies what reference signs (`standardwebhooks`, `svix`, `stripe`, `@octokit/webhooks-methods`)
- Exchanges run under `it.live`, as references read the system clock, and known-answer vectors run under `it.effect` at a `TestClock` instant

Real shells run each script whose static reading a test checks, recording what they executed:
- `PATH` names a temporary `bin` alone, holding a `rec` script that appends its arguments, folder, name, and input to `$RECORD`
- `extendEnv: false`, `HOME`, `ZDOTDIR`, and `BASH_ENV` point at empty temporary files, so no startup file of the machine runs
- Shell binaries resolve from the inherited `PATH` that `Config.String('PATH')` reads, before the child's environment replaces it
- Assertions compare invocations read from source with records each shell wrote, per dialect

## [04]-[NX_WORKSPACES]

Nx plugin and executor suites run against a workspace in a temporary folder from `@rasm/testing/nx`:

| [INDEX] | [FIXTURE]       | [SCOPE] | [RELEASE]                        |
| :-----: | :-------------- | :------ | :------------------------------- |
|  [01]   | `workspace`     | Test    | `onTestFinished` of its test     |
|  [02]   | `fileWorkspace` | File    | `onCleanup` after the file's end |

Fixtures create `realpath(mkdtemp('<tmpdir>/nx-workspace-'))`, `chdir` into it, set Nx's workspace root, and set `NX_DAEMON=false`, restoring each at release. Working folder and workspace root are process-wide, so a file reading either fixture runs its tests in sequence. Files needing one expensive graph for many assertions extend `fileWorkspace` into a file-scoped fixture returning that graph (`nx.extend('fixture', { scope: 'file' }, async ({ fileWorkspace }) => ...)`).

`Workspace` methods:

| [INDEX] | [MEMBER]                                     | [DOES]                                                                         |
| :-----: | :------------------------------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `write(files)`, `remove(...paths)`           | Writes or deletes files by workspace path, returning the workspace             |
|  [02]   | `install(...manifests)`                      | Links each package folder into `node_modules` and its `bin` into `.bin`        |
|  [03]   | `graph(plugin, options)`                     | Builds a project graph from the plugin beside Nx's `package.json` plugins      |
|  [04]   | `run(graph, project, target, configuration)` | Runs an `nx:run-commands` target, returning `{ success, terminalOutput }`      |
|  [05]   | `ready(graph, project, target)`              | Returns Nx's readiness probe of a task by `kind`, or `null` for none           |
|  [06]   | `poll(graph, project, target)`               | Spawns a `command` probe, returning a deadline function over its exit `Option` |
|  [07]   | `cached(graph, project, target)`             | Lists files on disk matching a target's declared `outputs`                     |
|  [08]   | `files(...ignored)`, `context(graph, ...)`   | Lists workspace files, builds an `ExecutorContext` for an executor call        |

- `install` takes a `package.json` file URL (`import.meta.resolve('<pkg>/package.json')`), or `[name, url]` to link under another name, as `typescript` links `@typescript/native`
- `graph` binds options into `createNodes` and `createDependencies` and builds in process with no daemon
- `poll` returns `(wait) => Promise<Option<code>>`, so a test reads `Option.none()` before a port listens and `Option.some(0)` after

Module functions serve tool runs, remote caches, and graph checks:
- `environment(root, set)` returns a child `nx` environment with daemon, TUI, and color off and `node_modules/.bin` first on `PATH`
- `environment` drops inherited `NX_*`, `VITEST*`, `npm_*`, `NODE_ENV`, `NO_COLOR`, `VIRTUAL_ENV`, and `GITHUB_ACTIONS`, so a child run reads no parent state
- `pathed(folder, scripts)` writes `#!/bin/sh` scripts and sets `PATH` to that folder alone, so every unscripted tool is missing
- `shimmed(folder, scripts)` prepends that folder to `PATH`, and a shim strips its own entry to run the real tool (`PATH="${PATH#*:}" exec uv "$@"`)
- `refusing(tool)` returns rows for a missing executable and a script exiting 3, each with the refusal pattern `@rasm/nx` raises, read through `pathed` and `refused`
- `remote(token, answer)` serves a scoped HTTP cache on port 0, storing `PUT` bodies by URL, logging `<method> <url>`, and answering each request with `answer`
- `answered` maps a request to 401 without a matching bearer token, 404 for an absent entry, and 200 otherwise, and `Option.none()` leaves a request unanswered
- `listen(port)` opens a TCP listener closed at test end, `listen()` taking a free port a test then closes and hands to a configuration
- `targetOptions`, `outputs`, and `taskGraph` read a target's resolved options, its output paths, and Nx's task graph
- `unaggregated(graph)` lists `<verb>--<tool>` targets their verb target neither runs nor hashes, and `unportable(graph)` lists stated paths holding `\`, a leading `/`, or a drive letter

```ts
it.for(refusing('jscpd'))('refuses the graph with the command and $case of jscpd', async ({ scripts, refusal }, { workspace }) => {
    pathed(join(workspace.root, '.bin'), scripts);

    await expect(workspace.write({ ...ROOT, '.jscpd.json': '{}' }).graph({ createNodes })).rejects.toSatisfy(refused(refusal));
});
```

Effect harnesses a Vitest fixture holds take a scope from `Scope.makeUnsafe()`, closed through `Scope.close(scope, Exit.void)` in `onCleanup`, and run through `Effect.provideService(remote(token, answer), Scope.Scope, scope)`.
