# [testcontainers] — one parameterized container builder; the pg-18.4-with-extensions row and the S3-compatible object-store row are DATA on it

[PACKAGE_SURFACE]:
- package: `testcontainers` · version `12.0.4` · license `MIT`
- module: CommonJS (`type: commonjs`; `main: build/index.js`, `build/index.d.ts`); no ESM/subpath map — the whole surface is the one barrel.
- asset: `build/index.d.ts`; drives the container runtime through `dockerode` + `undici` + `docker-compose` (bundled deps).
- runtime: node-only; requires a Docker-API-compatible engine on `DOCKER_HOST` (Docker / Colima / Podman — the Forge owns the local Colima socket). NOT platform-neutral: no browser, no bun-wasm. A `Ryuk`/reaper sidecar auto-reaps leaked containers.
- plane: `plane:dev` — the `_testkit` container lane (real server); the fast in-process counterpart is `electric-sql-pglite.md` (WASM pg, no server extensions).
- rail: persistence-verification / real-server harness.

`testcontainers` is the real-server half of the `_testkit` harness (`tests/typescript/_testkit`) — the lane for what the pglite WASM unit lane cannot serve: SERVER extensions (`pgvector`, `postgis`, the CNPG image rows) and a real S3-compatible object store for presign/round-trip verification. The whole package is ONE parameterized builder: `GenericContainer(image)` carries a fluent `with*` chain and yields a `StartedTestContainer` handle exposing the container's mapped host port. The `_testkit` container-lane "rows" — pg-18.4-with-extensions and the S3-compatible object store — are NOT two container classes; they are two DATA rows (image + exposed ports + environment + wait-strategy) feeding the same builder. A new lane is a row, never a new mechanism. Every started handle is `AsyncDisposable`, which is the exact seam onto `Effect.acquireRelease` — the container becomes a scoped Effect `Layer` shared across a spec block via `@effect/vitest` `layer(...)`, identical in shape to the pglite unit Layer but bound to a real engine.

## [01]-[CONTAINER_BUILDER]

[PUBLIC_TYPE_SCOPE]: the one builder and its contract. `TestContainer` is the fluent interface; `GenericContainer` is the concrete builder (also `implements TestContainer`); `GenericContainerBuilder` is the `fromDockerfile` variant for a built image. Every `with*` returns `this` — the whole roster below is the parameterization surface, discriminated by which rows a lane sets, never by subclass.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]                    | [CAPABILITY]                                           |
| :-----: | :----------------------------- | :------------------------------- | :----------------------------------------------------- |
|  [01]   | `GenericContainer`             | class `implements TestContainer` | `new GenericContainer(image)`; the one builder         |
|  [02]   | `TestContainer`                | interface                        | the fluent `with*` contract; yields `start()`          |
|  [03]   | `GenericContainerBuilder`      | class                            | image-build variant via `fromDockerfile(ctx).build()`  |
|  [04]   | `Environment` / `Labels`       | record type                      | `{ [k: string]: string }` — env + label rows           |
|  [05]   | `HealthCheck`                  | type                             | `{ test; interval?; timeout?; retries? }` healthcheck  |
|  [06]   | `ResourcesQuota`               | type                             | `{ memory?; cpu? }` — bounded resource envelope        |
|  [07]   | `ContentToCopy` / `FileToCopy` | type                             | seed content/files into the image (init SQL, fixtures) |

```ts signature
// build/generic-container.d.ts — one builder; a lane is the SET of with* rows it applies. Every setter returns `this`.
declare class GenericContainer implements TestContainer {
  constructor(image: string)
  static fromDockerfile(context: string, dockerfileName?: string): GenericContainerBuilder
  withExposedPorts(...ports: PortWithOptionalBinding[]): this   // container port → random host port (getMappedPort)
  withEnvironment(environment: Environment): this               // POSTGRES_PASSWORD, MINIO_ROOT_USER, …
  withCommand(command: string[]): this; withEntrypoint(entrypoint: string[]): this
  withWaitStrategy(waitStrategy: WaitStrategy): this            // readiness gate — see [03]
  withStartupTimeout(startupTimeoutMs: number): this
  withHealthCheck(healthCheck: HealthCheck): this               // pairs with Wait.forHealthCheck()
  withCopyContentToContainer(contentsToCopy: ContentToCopy[]): this   // inline init SQL / seed bytes
  withCopyFilesToContainer(filesToCopy: FileToCopy[]): this
  withBindMounts(bindMounts: BindMount[]): this; withTmpFs(tmpFs: TmpFs): this
  withNetwork(network: StartedNetwork): this; withNetworkAliases(...aliases: string[]): this
  withReuse(): this                                             // keep a warm container across runs (dev-loop speed)
  withResourcesQuota(quota: ResourcesQuota): this; withUlimits(ulimits: Ulimits): this
  withPullPolicy(pullPolicy: ImagePullPolicy): this; withPlatform(platform: string): this
  withLabels(labels: Labels): this; withName(name: string): this; withLogConsumer(c: (s: Readable) => unknown): this
  start(): Promise<StartedTestContainer>
}
```

## [02]-[STARTED_HANDLE]

[PUBLIC_TYPE_SCOPE]: the started container — the port/exec/lifecycle surface a spec reads, and `AbstractStartedContainer`, the base a lane subclasses to expose a typed capability handle (e.g. a `getConnectionUri()` on the pg row). `StartedTestContainer extends AsyncDisposable` — the Effect scoped-release seam.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]                           | [CAPABILITY]                                                |
| :-----: | :------------------------- | :-------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `StartedTestContainer`     | interface `extends AsyncDisposable`     | `getHost`/`getMappedPort`/`exec`/`stop`/`restart`/`logs`    |
|  [02]   | `AbstractStartedContainer` | class `implements StartedTestContainer` | subclass base for a typed lane handle                       |
|  [03]   | `StoppedTestContainer`     | interface                               | post-stop; `copyArchiveFromContainer` for artifacts         |
|  [04]   | `ExecResult`               | type                                    | `{ output; stdout; stderr; exitCode }` in-container receipt |
|  [05]   | `ExecOptions`              | type                                    | `{ workingDir; user; env }` exec context (`psql`/`mc`)      |

```ts signature
// build/test-container.d.ts — the started handle; getMappedPort is the whole point, AsyncDisposable is the Effect seam.
interface StartedTestContainer extends AsyncDisposable {
  getHost(): string
  getFirstMappedPort(): number
  getMappedPort(port: number, protocol?: string): number          // 5432 → the random host port to connect to
  getName(): string; getId(): string; getLabels(): Labels
  getNetworkId(networkName: string): string; getIpAddress(networkName: string): string
  exec(command: string | string[], opts?: Partial<ExecOptions>): Promise<ExecResult>   // psql -c, mc mb, pg_dump …
  logs(opts?: { since?: number; tail?: number }): Promise<Readable>
  restart(options?: Partial<RestartOptions>): Promise<void>
  stop(options?: Partial<StopOptions>): Promise<StoppedTestContainer>                   // { timeout; remove; removeVolumes }
  [Symbol.asyncDispose](): Promise<void>                                                // ← Effect.acquireRelease release
}
type ExecResult = { output: string; stdout: string; stderr: string; exitCode: number }
```

## [03]-[WAIT_STRATEGY]

`Wait` is the readiness-gate factory — a container is "started" only when its wait strategy resolves, so each lane's row includes a wait strategy matched to its image's ready signal. This is one parameterized family: `forLogMessage` for a pg ready-log, `forHttp(...).forStatusCode(...)` for an S3 health endpoint, `forHealthCheck` for a container `HEALTHCHECK`, `forAll` to compose. The `HttpWaitStrategy` carries its own fluent refinement chain.

| [INDEX] | [SURFACE]                                   | [PRODUCES]              | [CAPABILITY]                                                |
| :-----: | :------------------------------------------ | :---------------------- | :---------------------------------------------------------- |
|  [01]   | `Wait.forListeningPorts()`                  | `WaitStrategy`          | ready when exposed ports accept TCP (coarse default)        |
|  [02]   | `Wait.forLogMessage(msg \| RegExp, times?)` | `WaitStrategy`          | ready on a log line N times — the pg `ready to accept` gate |
|  [03]   | `Wait.forHealthCheck()`                     | `WaitStrategy`          | ready when the container `HEALTHCHECK` is healthy           |
|  [04]   | `Wait.forHttp(path, port, opts?)`           | `HttpWaitStrategy`      | ready on an HTTP probe — the S3 `/minio/health/live` gate   |
|  [05]   | `Wait.forSuccessfulCommand(command)`        | `ShellWaitStrategy`     | ready when an in-container command exits 0 (`pg_isready`)   |
|  [06]   | `Wait.forAll([...])`                        | `CompositeWaitStrategy` | conjoin gates (port + log + http) for a multi-signal image  |
|  [07]   | `Wait.forOneShotStartup()`                  | `WaitStrategy`          | ready when a run-to-completion container exits              |

```ts signature
// build/wait-strategies — the HTTP gate refines fluently; the pg + S3 rows differ only in which factory + refinements.
declare class Wait {
  static forLogMessage(message: string | RegExp, times?: number): WaitStrategy
  static forHttp(path: string, port: number, options?: { abortOnContainerExit?: boolean }): HttpWaitStrategy
  static forAll(waitStrategies: WaitStrategy[]): CompositeWaitStrategy
}
declare class HttpWaitStrategy {
  forStatusCode(statusCode: number): this
  forStatusCodeMatching(predicate: (statusCode: number) => boolean): this
  forResponsePredicate(predicate: (response: string) => boolean): this
  withMethod(method: string): this; withHeaders(headers: Record<string, string>): this
  withBasicCredentials(username: string, password: string): this; usingTls(): this; allowInsecure(): this
}
```

## [04]-[NETWORK_RUNTIME_COMPOSE]

Cross-container wiring, runtime detection, image policy, and the multi-service compose lane — the surface a two-row harness (pg + store on one network) reaches for.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]           | [CAPABILITY]                                                           |
| :-----: | :------------------------------------ | :---------------------- | :--------------------------------------------------------------------- |
|  [01]   | `Network` / `StartedNetwork`          | class                   | `new Network().start()`; `withNetwork(net)` wires containers together  |
|  [02]   | `TestContainers.exposeHostPorts(...)` | static                  | forward host ports INTO containers (a host service reachable inside)   |
|  [03]   | `PullPolicy` / `ImagePullPolicy`      | class + interface       | `PullPolicy.alwaysPull()` / `defaultPolicy()`; `{ shouldPull() }`      |
|  [04]   | `getContainerRuntimeClient()`         | function                | `Promise<ContainerRuntimeClient>` — engine handle (info/image/network) |
|  [05]   | `ImageName`                           | class                   | `ImageName.fromString(str)` — registry/image/tag parse                 |
|  [06]   | `DockerComposeEnvironment`            | class                   | multi-service lane; `.up()` → `StartedDockerComposeEnvironment`        |
|  [07]   | `StartedDockerComposeEnvironment`     | class `AsyncDisposable` | `.getContainer(name).getMappedPort(p)`; `.down()`                      |

```ts signature
// build/network + test-containers + docker-compose-environment — the two-row wiring surface.
declare class Network { constructor(uuid?: Uuid); start(): Promise<StartedNetwork> }
declare class TestContainers { static exposeHostPorts(...ports: number[]): Promise<void> }
declare function getContainerRuntimeClient(): Promise<ContainerRuntimeClient>   // detects Docker vs Colima vs Podman
declare class DockerComposeEnvironment {
  constructor(composeFilePath: string, composeFiles: string | string[], uuid?: Uuid)
  withWaitStrategy(containerName: string, waitStrategy: WaitStrategy): this
  withProfiles(...profiles: string[]): this; withProjectName(projectName: string): this
  up(services?: string[]): Promise<StartedDockerComposeEnvironment>              // both rows in one compose file
}
```

## [05]-[INTEGRATION]

[STACK: `GenericContainer` + `Effect.acquireRelease` + `@effect/vitest` `layer`] — a container is a scoped Effect `Layer`, never a per-spec `beforeAll`. `Layer.scoped(PgContainer, Effect.acquireRelease(Effect.promise(() => pgRow.start()), c => Effect.promise(() => c.stop())))` builds one real pg once; `layer(PgContainer)("suite", (it) => …)` (`fast-check.md` [05]) shares it across the block, and `Effect.tryPromise({ try: () => c.exec([...]), catch: … })` folds `exec`/`getMappedPort` into the folder's typed error rail. `StartedTestContainer` is `AsyncDisposable`, so `Effect.acquireRelease` (or `Effect.scoped` over `[Symbol.asyncDispose]`) reclaims the container on scope close — success, failure, or interrupt.

[STACK: both harness rows as data on the one builder] — the pg-18.4 row and the S3 store row differ only in their `with*` data, not in mechanism:

```ts signature
// TWO ROWS, ONE BUILDER — image + ports + env + wait are the parameterization; a third lane is a third row.
const pgRow    = new GenericContainer("postgres:18.4-<ext-image>")   // pgvector/postgis server-extension image
  .withExposedPorts(5432).withEnvironment({ POSTGRES_PASSWORD: "…" })
  .withWaitStrategy(Wait.forLogMessage(/database system is ready to accept connections/, 2))
const storeRow = new GenericContainer("<s3-compatible-image>")       // MinIO / localstack S3 lane
  .withExposedPorts(9000).withEnvironment({ /* root creds */ })
  .withWaitStrategy(Wait.forHttp("/minio/health/live", 9000).forStatusCode(200))
```

[STACK: `getMappedPort` → `@effect/sql-pg` `PgClient` / S3 client] — the pg row's `getMappedPort(5432)` + `getHost()` feed a `PgClient.layerConfig(Config.Wrap<PgClientConfig>)` real-driver `Layer<PgClient | SqlClient, ConfigError | SqlError>` (`data/.api/effect-sql-pg.md`) — the exact seam pglite has no dialect for, so any real-server-extension assertion binds the actual `pg` driver here. Server-extension DDL seeds via raw `sql` execute as idempotent declarative ensure, NOT `PgMigrator` — the `tests/typescript/_architecture` suite bans `@effect/sql/Migrator` / `@effect/sql-pg/PgMigrator` branch-wide (`electric-sql-pglite.md` [04]), so this real-server lane is migrator-free exactly as the unit lane is: the container lane owns real-pg, never a migration path. The store row's `getMappedPort(9000)` feeds an S3 client for the object-presign + round-trip verification lane. This is the boundary pglite cannot cross: any assertion needing a real server extension or a real object store is a container-lane spec by definition; everything else stays in the microsecond WASM unit lane.

[STACK: mutation-run reuse] — under `stryker-mutator-vitest-runner.md` the vitest worker is reused across mutants, so a container `Layer` is built once and re-entered per `mutantRun`; the acquire/release must be idempotent and leave no cross-mutant rows (truncate-in-acquire or a fresh schema per mutant), or one mutant's writes mask another's `Survived`.

## [06]-[RAIL_LAW]

- Owns: real Docker-engine container lifecycle for the server lane — the one `GenericContainer` builder, its `Wait` readiness gates, `Network` wiring, host-port forwarding, image pull policy, runtime detection, and the compose multi-service variant; `getMappedPort`/`getHost`/`exec` as the connection + in-container command surface.
- Accept: `GenericContainer(image).with*(…).start()` as the row per lane; a `Wait` strategy matched to the image's ready signal; `Effect.acquireRelease` + `layer(...)` for scoped Layer sharing; `withReuse()` for the dev loop; `withResourcesQuota` to bound a heavy image; the compose lane when both rows share one file.
- Reject: hand-rolled `docker run`/`dockerode` calls (the builder owns invocation); a per-spec `beforeAll` container instead of a scoped Layer; a bare `Promise` container without `acquireRelease` (leaks on interrupt — the reaper is a backstop, not the design); routing a no-server-extension query here (that is the pglite unit lane — `electric-sql-pglite.md`); `PgMigrator` for schema (purity-banned branch-wide — seed via raw DDL execute); importing this package from any `plane:runtime` folder — dev lane only.
- Boundary: node + a live Docker-API engine only — no browser, no WASM, no CI runner without a container socket. Startup is seconds, not microseconds; a spec that does not need a real server or object store belongs in the pglite lane. `getMappedPort` returns a RANDOM host port per start — never hardcode `5432`/`9000` on the host side; always read the mapped port.
