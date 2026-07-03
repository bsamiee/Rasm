# [API_CATALOGUE] testcontainers

`testcontainers` is the Docker-backed ephemeral-infrastructure harness the `services` integration tests ride: `new GenericContainer(image)` configures any image through one chainable `with*` builder family (every method returns `this`), `start()` resolves a `StartedTestContainer`, and `Wait.*` produces the readiness strategy the start blocks on. `DockerComposeEnvironment` lifts a whole Compose file, `Network` creates isolated bridges, `SocatContainer` proxies TCP to sidecars, and a Ryuk reaper (`getReaper`) plus session labels guarantee cleanup even on a crashed run. Every started resource `implements AsyncDisposable`, so `await using` (JS explicit resource management) is the native cleanup, and in Effect that `Symbol.asyncDispose` maps one-to-one onto `Effect.acquireRelease`/`Scope` — the container becomes a scoped resource in an `@effect/vitest` `it.scoped`/`it.layer` block, its `getMappedPort`/`getHost` feeding the real `@effect/sql-pg` `PgClient`, `ioredis` client, or S3-compatible endpoint under test. It has no design-page code fence: it is the `[TEST_HARNESS]` rail that stands up real Postgres/Redis/object-store backends so `persistence/store`, `messaging`, and `persistence/object` prove against live services, not mocks. A hand-rolled `dockerode`/`docker run` in test setup, or polling a port without a `WaitStrategy`, is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `testcontainers`
- package: `testcontainers` (12.0.4, MIT; © testcontainers.org)
- module format: CJS `build/index.js` (`main`), `.d.ts` beside each `.js`; a re-export barrel (`build/index.d.ts`) — self-typed, no `@types`
- reflected: TSDECL — `node_modules/testcontainers/build/index.d.ts` (+ `test-container`/`generic-container`/`wait-strategies`/`docker-compose-environment`/`types` submodules)
- runtime target: `node`; wraps `dockerode` — requires a reachable Docker/Podman socket (`ContainerRuntime = "docker" \| "podman"`); on this machine the Colima socket, resolved by the Forge container tooling
- ABI: a Ryuk reaper sidecar tears down labelled containers/networks/volumes on session end (`LABEL_TESTCONTAINERS_SESSION_ID`); `withReuse()` keys a container by config hash for cross-run reuse; `TestContainers.exposeHostPorts` routes host ports inward via a port-forwarder sidecar
- consumer: no `.planning` design-page fence — the `[TEST_HARNESS]` rail; stacks under `@effect/vitest` `it.scoped`/`it.layer` as an `Effect.acquireRelease`/`Scope` resource feeding `@effect/sql-pg`/`ioredis`/object-store integration proofs
- rail: test-infrastructure

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container lifecycle family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `TestContainer`            | interface     | the pre-start builder contract — the `start()` + `with*` fluent surface |
|  [02]   | `StartedTestContainer`     | interface     | `extends AsyncDisposable` — the running query/control/copy/exec surface |
|  [03]   | `StoppedTestContainer`     | interface     | `getId()` + `copyArchiveFromContainer(path)`                       |
|  [04]   | `GenericContainer`         | class         | `implements TestContainer` for any image; adds `withName`/`withLabels`/`withHealthCheck`/`withIpcMode`/`withNetworkAliases` |
|  [05]   | `GenericContainerBuilder`  | class         | Dockerfile build config — `withBuildArgs`/`withCache`/`withBuildkit`/`withTarget`/`withPlatform` → `build(image?, BuildOptions?)` |
|  [06]   | `AbstractStartedContainer` / `AbstractStoppedContainer` | class | delegating bases for custom started/stopped wrappers          |
|  [07]   | `RestartOptions` / `StopOptions` | interface | `{timeout}` and `{timeout, remove, removeVolumes}` (passed `Partial<…>`) |
|  [08]   | `SocatContainer` / `StartedSocatContainer` | class | `extends GenericContainer` — a socat TCP-proxy sidecar via `withTarget(exposePort, host, internalPort?)` |
|  [09]   | `BuildOptions`             | type          | `{deleteOnExit: boolean}` for `GenericContainerBuilder.build`      |

[PUBLIC_TYPE_SCOPE]: Compose environment family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `DockerComposeEnvironment`        | class         | Compose builder — 12 `with*` methods → `up(services?)`        |
|  [02]   | `StartedDockerComposeEnvironment` | class         | `implements AsyncDisposable`; `getContainer(name)`/`stop()`/`down(Partial<ComposeDownOptions>?)` |
|  [03]   | `StoppedDockerComposeEnvironment` / `DownedDockerComposeEnvironment` | class | terminal Compose states from `stop()`/`down()`          |

[PUBLIC_TYPE_SCOPE]: network and wait-strategy family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `Network`                 | class         | `new Network(uuid?).start()` → `StartedNetwork`               |
|  [02]   | `StartedNetwork`          | class         | `implements AsyncDisposable`; `getId`/`getName`/`stop()`      |
|  [03]   | `StoppedNetwork`          | class         | terminal network state                                        |
|  [04]   | `Wait`                    | class         | the static wait-strategy factory (7 `for*` methods)           |
|  [05]   | `WaitStrategy`            | interface     | `waitUntilReady`/`withStartupTimeout`/`isStartupTimeoutSet`/`getStartupTimeout` |
|  [06]   | `StartupCheckStrategy`    | abstract class | custom startup-probe base — implement `checkStartupState → StartupStatus` |
|  [07]   | `StartupStatus`           | union         | `"PENDING" \| "SUCCESS" \| "FAIL"`                             |
|  [08]   | `HttpWaitStrategyOptions` | interface     | `{abortOnContainerExit?}` — passed to `Wait.forHttp`          |
|  [09]   | `HttpWaitStrategy`        | class (return of `Wait.forHttp`) | the chainable HTTP probe (not a barrel export — reached via `forHttp`) |

[PUBLIC_TYPE_SCOPE]: config, exec, and port structural vocabulary
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ExecResult` / `ExecOptions`            | type          | `{output, stdout, stderr, exitCode}`; `{workingDir, user, env}` (barrel-exported) |
|  [02]   | `InspectResult`                         | type          | `{name, hostname, ports, healthCheckStatus, networkSettings, state, labels}` (barrel-exported) |
|  [03]   | `CommitOptions` / `Content` / `CopyToContainerOptions` | type | commit-to-image opts; `string\|Buffer\|Readable`; put-archive opts (barrel-exported) |
|  [04]   | `Environment` / `Labels` / `BuildArgs` / `TmpFs` | type | `Record<string,string>` config maps (structural param shapes — not barrel-exported) |
|  [05]   | `BindMount` (`BindMode`) / `FileToCopy` / `DirectoryToCopy` / `ContentToCopy` / `ArchiveToCopy` | type | mount and copy descriptors (`mode: "rw"\|"ro"\|"z"\|"Z"`) |
|  [06]   | `HealthCheck` (`HealthCheckStatus`) / `Ulimits` / `ResourcesQuota` / `ExtraHost` | type | Docker healthcheck spec, ulimits, `{memory,cpu}`, extra-hosts |
|  [07]   | `PortWithOptionalBinding` (`PortWithBinding`) | union   | `number \| "${n}/tcp\|udp" \| {container, host, protocol?}` — the `withExposedPorts` input (fixed host port via object form) |
|  [08]   | `ImagePullPolicy`                       | interface     | `{shouldPull(): boolean}` — the pull-policy contract           |
|  [09]   | `ContainerRuntime` / `Ports` / `HostPortBindings` / `NetworkSettings` / `RegistryConfig` | type | runtime kind, port maps, network settings, registry auth |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the `with*` builder family (every method returns `this`)
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new GenericContainer(image)` / `GenericContainer.fromDockerfile(context, name?)` | construct | container builder for an image; Dockerfile → `GenericContainerBuilder` |
|  [02]   | `withCommand` / `withEntrypoint` / `withEnvironment` / `withName` / `withLabels` / `withWorkingDir` | builder: command/env | image runtime overrides |
|  [03]   | `withExposedPorts(...PortWithOptionalBinding)` / `withNetwork` / `withNetworkMode` / `withNetworkAliases` / `withExtraHosts` / `withHostname` | builder: network | port map (fixed or ephemeral) + network attach |
|  [04]   | `withBindMounts` / `withTmpFs` / `withCopyFilesToContainer` / `withCopyDirectoriesToContainer` / `withCopyContentToContainer` / `withCopyArchivesToContainer` / `withCopyToContainerOptions` | builder: mounts/copy | host mounts and start-time file/content/archive injection |
|  [05]   | `withWaitStrategy` / `withStartupTimeout(ms)` / `withHealthCheck` / `withPullPolicy` / `withReuse` / `withAutoCleanup` / `withAutoRemove` | builder: lifecycle | readiness, timeout, pull/reuse/cleanup policy |
|  [06]   | `withUser` / `withPrivilegedMode` / `withSecurityOpt` / `withAddedCapabilities` / `withDroppedCapabilities` / `withIpcMode` / `withPlatform` | builder: security/host | user, caps, privileged, platform |
|  [07]   | `withResourcesQuota({memory,cpu})` / `withUlimits` / `withSharedMemorySize(bytes)` / `withLogConsumer(fn)` / `withDefaultLogDriver` | builder: resources | cgroup limits, shm, log streaming |
|  [08]   | `start(): Promise<StartedTestContainer>`                         | lifecycle      | build + start + block on the wait strategy                     |

[ENTRYPOINT_SCOPE]: started container — query, control, exec, copy
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `getHost()` / `getHostname()` / `getId()` / `getName()` / `getLabels()` | query    | connection host, container identity, labels                    |
|  [02]   | `getFirstMappedPort()` / `getMappedPort(port, protocol?)` / `getMappedPort("5432/tcp")` | query | resolve the ephemeral host port after start (number or `${n}/tcp\|udp` overload) |
|  [03]   | `getIpAddress(networkName)` / `getNetworkNames()` / `getNetworkId(networkName)` | query | per-network IP/name/id                                       |
|  [04]   | `exec(command: string \| string[], opts?): Promise<ExecResult>`   | execute        | run a command; `ExecResult` = `{output, stdout, stderr, exitCode}` |
|  [05]   | `logs(opts?: {since?, tail?}): Promise<Readable>`                  | execute        | container log stream                                          |
|  [06]   | `copyArchiveFromContainer(path)` / `copyArchiveToContainer(tar, target?, opts?)` | copy | tar in/out of a running container                             |
|  [07]   | `copyFilesToContainer` / `copyDirectoriesToContainer` / `copyContentToContainer` | copy | file/dir/in-memory content into a running container            |
|  [08]   | `commit(CommitOptions): Promise<string>`                          | snapshot       | freeze the running container to a new image id                |
|  [09]   | `stop(Partial<StopOptions>?)` / `restart(Partial<RestartOptions>?)` / `[Symbol.asyncDispose]()` | lifecycle | stop → `StoppedTestContainer`; restart; `await using` cleanup |

[ENTRYPOINT_SCOPE]: wait-strategy factory and HTTP-probe chain
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Wait.forListeningPorts()` / `Wait.forHealthCheck()` / `Wait.forOneShotStartup()` | factory | port-accept, Docker healthcheck-pass, clean-exit strategies |
|  [02]   | `Wait.forLogMessage(message: string \| RegExp, times?)` | factory        | wait for a log pattern N times                                |
|  [03]   | `Wait.forHttp(path, port, options?): HttpWaitStrategy`   | factory        | HTTP readiness; returns the chainable probe                   |
|  [04]   | `Wait.forSuccessfulCommand(command)` / `Wait.forAll(strategies[])` | factory | zero-exit shell probe; composite (all must pass)              |
|  [05]   | `forHttp(...).forStatusCode(n)` / `.forStatusCodeMatching(fn)` / `.forResponsePredicate(fn)` | http chain | assert status/body |
|  [06]   | `forHttp(...).withMethod(m)` / `.withHeaders(h)` / `.withBasicCredentials(u,p)` / `.withReadTimeout(ms)` / `.usingTls()` / `.allowInsecure()` | http chain | method/headers/auth/tls tuning |

[ENTRYPOINT_SCOPE]: network, Compose, sidecars, and runtime client
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]  | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `new Network(uuid?).start()` → `getId`/`getName`/`stop()`                      | network         | isolated Docker bridge; `AsyncDisposable`                |
|  [02]   | `new DockerComposeEnvironment(path, files: string \| string[], uuid?)`         | compose         | Compose environment builder                              |
|  [03]   | `compose.withBuild()` / `.withEnvironment` / `.withEnvironmentFile` / `.withProfiles(...)` / `.withNoRecreate()` / `.withProjectName` / `.withPullPolicy` / `.withDefaultWaitStrategy` / `.withStartupTimeout` / `.withClientOptions` | compose builder | full Compose configuration |
|  [04]   | `compose.withWaitStrategy(containerName, ws)` / `compose.up(services?)`        | compose         | per-service wait; start → `StartedDockerComposeEnvironment` |
|  [05]   | `startedCompose.getContainer(name)` / `.stop()` / `.down(opts?)`              | compose         | reach one service; teardown                              |
|  [06]   | `new SocatContainer(image?).withTarget(exposePort, host, internalPort?).start()` | sidecar      | TCP proxy to a non-Docker host/service                   |
|  [07]   | `TestContainers.exposeHostPorts(...ports)` / `getReaper()` / `waitForContainer(...)` | harness   | host-port forwarding, Ryuk reaper handle, low-level wait |
|  [08]   | `getContainerRuntimeClient(): Promise<ContainerRuntimeClient>` / `ImageName` / `PullPolicy.defaultPolicy()` / `PullPolicy.alwaysPull()` | runtime | raw `{compose, container, image, network}` client; image-ref parse; the two built-in pull policies |
|  [09]   | `getContainerPort(port)` / `hasHostBinding(port)` / `new RandomPortGenerator()` / `Uuid` / `Retry` | primitive | port helpers, port generator, session uuid/retry primitives |

## [04]-[IMPLEMENTATION_LAW]

[TESTCONTAINERS_TOPOLOGY]:
- `GenericContainer implements TestContainer`; `start()` → `StartedTestContainer extends AsyncDisposable`. `await using container = await new GenericContainer(image).withExposedPorts(5432).start()` stops with full cleanup at scope exit.
- Port mapping is ephemeral by default: `withExposedPorts(5432)` maps a container port to a random host port; `getMappedPort(5432)` (or `getMappedPort("5432/tcp")`) reads it after start. A fixed host port needs the object form `withExposedPorts({container: 5432, host: 5432})` (`PortWithBinding`).
- `GenericContainer.fromDockerfile(context, file?)` → `GenericContainerBuilder`; `build(image?, {deleteOnExit})` produces a named image consumed by `new GenericContainer(image)`.
- `DockerComposeEnvironment.withWaitStrategy(containerName, ws)` scopes readiness to a named service; `up()` blocks until every service's strategy passes; `getContainer(name)` returns that service's started handle.
- `Network`, `StartedNetwork`, and every started container/Compose env `implement AsyncDisposable`, so a Ryuk reaper plus `Symbol.asyncDispose` guarantee teardown on crash or scope exit.

[STACKING_LAW]:
- The `AsyncDisposable` contract maps directly onto Effect structured resources: `Effect.acquireRelease(Effect.promise(() => new GenericContainer(image).start()), (c) => Effect.promise(() => c.stop()))` yields a `Scope`-bound container; under `@effect/vitest`, `it.scoped(name, Effect.gen(...))` binds it to the test scope and `it.layer(containerLayer)(...)` shares one container across a describe block.
- `getHost()` + `getMappedPort(port)` build the connection config the real client under test consumes: a Postgres container feeds the `@effect/sql-pg` `PgClient` (`persistence/store` proofs), a Redis container feeds `ioredis` (`messaging`/backplane proofs), a MinIO/S3-compatible container feeds the `@effect-aws/client-s3` `S3Service` (`persistence/object` proofs) — every integration test runs against a live backend, never a mock.
- `exec()`/`ExecResult` runs migrations or seed commands in-container; `Wait.forLogMessage`/`forHttp`/`forHealthCheck` gates start on genuine readiness so the first client call never races the boot.
- `SocatContainer` + `TestContainers.exposeHostPorts` bridge a host-run service (e.g. a local `@effect/cluster` node) into the container network; `Network` isolates a multi-container topology (app + db + redis) for one scenario.

[RAIL_LAW]:
- Package: `testcontainers`
- Owns: Docker/Compose container, network, and sidecar lifecycle for test isolation, plus the pull/wait/reaper policy surface
- Accept: one `GenericContainer` per service; the `with*` fluent chain; `Wait.*` for readiness; `await using`/`Effect.acquireRelease` for deterministic cleanup; `getMappedPort`/`getHost` to wire real clients; `DockerComposeEnvironment` for a fixed multi-service topology
- Reject: a hand-rolled `dockerode`/`docker run` in test setup; polling a port without a `WaitStrategy`; a fixed host port where the ephemeral map suffices; the removed `PullPolicy.Always`/`IfNotPresent` names (only `defaultPolicy()`/`alwaysPull()` exist); assuming a `StartupStatus` of `"FAILURE"` (the value is `"FAIL"`)
