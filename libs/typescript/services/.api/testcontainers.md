# [API_CATALOGUE] testcontainers

`testcontainers` supplies Docker-backed ephemeral test infrastructure: `GenericContainer` builds and starts any Docker image as a `StartedTestContainer`, `DockerComposeEnvironment` lifts a Compose file, `Network` creates isolated Docker networks, and the `Wait` factory produces ready-to-use wait strategies. Containers implement `AsyncDisposable` for `await using` resource cleanup.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `testcontainers`
- package: `testcontainers`
- module: `testcontainers`
- asset: `GenericContainer`, `DockerComposeEnvironment`, `Network`, `Wait`, container lifecycle interfaces, type helpers
- rail: test-infrastructure

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container lifecycle family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------- |
|   [1]   | `TestContainer`            | interface     | pre-start container builder contract                   |
|   [2]   | `StartedTestContainer`     | interface     | running container query + control surface              |
|   [3]   | `StoppedTestContainer`     | interface     | stopped container; `getId`, `copyArchiveFromContainer` |
|   [4]   | `GenericContainer`         | class         | `TestContainer` implementation for any image           |
|   [5]   | `GenericContainerBuilder`  | class         | Dockerfile-based image build configuration             |
|   [6]   | `AbstractStartedContainer` | class         | base for custom started-container wrappers             |
|   [7]   | `AbstractStoppedContainer` | class         | base for custom stopped-container wrappers             |
|   [8]   | `RestartOptions`           | interface     | `{ timeout: number }`                                  |
|   [9]   | `StopOptions`              | interface     | `{ timeout, remove, removeVolumes }`                   |

[PUBLIC_TYPE_SCOPE]: Compose environment family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :-------------------------------- | :------------ | :------------------------------- |
|   [1]   | `DockerComposeEnvironment`        | class         | Compose file environment builder |
|   [2]   | `StartedDockerComposeEnvironment` | class         | running Compose environment      |
|   [3]   | `DownedDockerComposeEnvironment`  | class         | downed Compose environment       |
|   [4]   | `StoppedDockerComposeEnvironment` | class         | stopped Compose environment      |

[PUBLIC_TYPE_SCOPE]: network and wait strategy family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------ | :------------ | :-------------------------------------------- |
|   [1]   | `Network`                 | class         | Docker network builder                        |
|   [2]   | `StartedNetwork`          | class         | running network; `implements AsyncDisposable` |
|   [3]   | `StoppedNetwork`          | class         | stopped network                               |
|   [4]   | `Wait`                    | class         | wait strategy factory (all static methods)    |
|   [5]   | `WaitStrategy`            | interface     | wait strategy contract                        |
|   [6]   | `StartupCheckStrategy`    | class         | custom startup check strategy base            |
|   [7]   | `StartupStatus`           | enum-like     | `SUCCESS` / `FAILURE` / `PENDING`             |
|   [8]   | `HttpWaitStrategy`        | class         | HTTP wait strategy with method chaining       |
|   [9]   | `HttpWaitStrategyOptions` | interface     | HTTP wait strategy configuration              |

[PUBLIC_TYPE_SCOPE]: message and exec type family
- rail: test-infrastructure

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                 |
| :-----: | :---------------- | :------------ | :----------------------------------------------------- |
|   [1]   | `ExecResult`      | type          | `{ output, stdout, stderr, exitCode }`                 |
|   [2]   | `ExecOptions`     | type          | exec command options                                   |
|   [3]   | `InspectResult`   | type          | container inspect metadata                             |
|   [4]   | `Environment`     | type          | `Record<string, string>` env vars                      |
|   [5]   | `BindMount`       | type          | `{ source, target, mode? }`                            |
|   [6]   | `FileToCopy`      | type          | `{ source, target, mode? }`                            |
|   [7]   | `ContentToCopy`   | type          | `{ content: string\|Buffer\|Readable, target, mode? }` |
|   [8]   | `HealthCheck`     | type          | Docker healthcheck spec                                |
|   [9]   | `ImagePullPolicy` | interface     | pull policy contract                                   |
|  [10]   | `PullPolicy`      | class         | built-in pull policy implementations                   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container construction and builder
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `new GenericContainer(image: string)`                             | constructor    | creates container builder for a Docker image      |
|   [2]   | `GenericContainer.fromDockerfile(context, dockerfileName?)`       | static factory | creates `GenericContainerBuilder` from Dockerfile |
|   [3]   | `container.withCommand(command: string[])`                        | builder        | overrides container command                       |
|   [4]   | `container.withEnvironment(environment: Environment)`             | builder        | sets environment variables                        |
|   [5]   | `container.withExposedPorts(...ports)`                            | builder        | declares ports to expose and map                  |
|   [6]   | `container.withWaitStrategy(waitStrategy)`                        | builder        | sets readiness wait strategy                      |
|   [7]   | `container.withStartupTimeout(ms)`                                | builder        | sets startup timeout in milliseconds              |
|   [8]   | `container.withNetwork(network: StartedNetwork)`                  | builder        | attaches to a running Docker network              |
|   [9]   | `container.withNetworkMode(mode)`                                 | builder        | sets network mode (`"host"`, etc.)                |
|  [10]   | `container.withBindMounts(bindMounts: BindMount[])`               | builder        | mounts host paths into container                  |
|  [11]   | `container.withHealthCheck(healthCheck: HealthCheck)`             | builder        | configures Docker health check                    |
|  [12]   | `container.withCopyFilesToContainer(files: FileToCopy[])`         | builder        | copies files at container start                   |
|  [13]   | `container.withCopyContentToContainer(contents: ContentToCopy[])` | builder        | copies in-memory content at start                 |
|  [14]   | `container.withPullPolicy(pullPolicy: ImagePullPolicy)`           | builder        | overrides image pull behaviour                    |
|  [15]   | `container.withReuse()`                                           | builder        | reuses existing container with same config        |
|  [16]   | `container.withPrivilegedMode()`                                  | builder        | runs container in privileged mode                 |
|  [17]   | `container.withUser(user)`                                        | builder        | sets container user                               |
|  [18]   | `container.start()`                                               | lifecycle      | `Promise<StartedTestContainer>`                   |

[ENTRYPOINT_SCOPE]: started container operations
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :--------------------------------------- | :------------- | :--------------------------------------------- |
|   [1]   | `started.getHost()`                      | query          | mapped host address                            |
|   [2]   | `started.getFirstMappedPort()`           | query          | first mapped host port                         |
|   [3]   | `started.getMappedPort(port, protocol?)` | query          | specific mapped host port                      |
|   [4]   | `started.getId()`                        | query          | container ID string                            |
|   [5]   | `started.getName()`                      | query          | container name                                 |
|   [6]   | `started.getIpAddress(networkName)`      | query          | container IP on a named network                |
|   [7]   | `started.exec(command, opts?)`           | execute        | `Promise<ExecResult>`                          |
|   [8]   | `started.logs(opts?)`                    | execute        | `Promise<Readable>` — container log stream     |
|   [9]   | `started.copyArchiveFromContainer(path)` | copy           | `Promise<NodeJS.ReadableStream>` — tar archive |
|  [10]   | `started.copyFilesToContainer(files)`    | copy           | copies files into running container            |
|  [11]   | `started.stop(options?)`                 | lifecycle      | `Promise<StoppedTestContainer>`                |
|  [12]   | `started.restart(options?)`              | lifecycle      | `Promise<void>`                                |
|  [13]   | `started[Symbol.asyncDispose]()`         | lifecycle      | `AsyncDisposable` — `await using` cleanup      |

[ENTRYPOINT_SCOPE]: wait strategy factory
- rail: test-infrastructure

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------ | :------------- | :----------------------------------------------- |
|   [1]   | `Wait.forListeningPorts()`            | wait factory   | waits until all exposed ports accept connections |
|   [2]   | `Wait.forLogMessage(message, times?)` | wait factory   | waits for regex or string in logs                |
|   [3]   | `Wait.forHealthCheck()`               | wait factory   | waits until Docker health check passes           |
|   [4]   | `Wait.forHttp(path, port, options?)`  | wait factory   | waits for HTTP response on path                  |
|   [5]   | `Wait.forSuccessfulCommand(command)`  | wait factory   | waits for zero-exit shell command                |
|   [6]   | `Wait.forOneShotStartup()`            | wait factory   | waits for container process to exit cleanly      |
|   [7]   | `Wait.forAll(waitStrategies)`         | wait factory   | composite; all strategies must pass              |

[ENTRYPOINT_SCOPE]: network and Compose
- rail: test-infrastructure

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]  | [RAIL]                                     |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :----------------------------------------- |
|   [1]   | `new Network(uuid?).start()`                                                  | network         | `Promise<StartedNetwork>`                  |
|   [2]   | `startedNetwork.getId()` / `getName()` / `stop()`                             | network         | query and cleanup                          |
|   [3]   | `new DockerComposeEnvironment(path, files, uuid?)`                            | compose         | Compose environment builder                |
|   [4]   | `compose.withBuild()` / `.withEnvironment(…)` / `.withWaitStrategy(name, ws)` | compose builder | Compose configuration                      |
|   [5]   | `compose.up(services?)`                                                       | compose         | `Promise<StartedDockerComposeEnvironment>` |

## [4]-[IMPLEMENTATION_LAW]

[TESTCONTAINERS_TOPOLOGY]:
- `GenericContainer` implements `TestContainer`; calling `start()` resolves to `StartedTestContainer` which implements `AsyncDisposable`.
- `await using container = await new GenericContainer(image).start()` triggers `stop` with full cleanup on scope exit.
- Port mapping: `withExposedPorts(5432)` maps a container port; `getMappedPort(5432)` retrieves the ephemeral host port after start.
- `GenericContainer.fromDockerfile(context, file?)` returns a `GenericContainerBuilder`; call `.build()` on it to produce a named image, then pass to `new GenericContainer(image)`.
- `DockerComposeEnvironment.withWaitStrategy(containerName, ws)` scopes a wait strategy to a named service.
- `Network.start()` returns `StartedNetwork` which also implements `AsyncDisposable`.

[LOCAL_ADMISSION]:
- `getContainerRuntimeClient()` is exported for advanced Docker API access via the `ContainerRuntimeClient` surface.
- `ImageName` carries registry, name, and tag parsing; used inside `GenericContainer.imageName`.
- `PullPolicy` provides `Always`, `IfNotPresent`, and `DefaultPullPolicy` via `PullPolicy.*` static members.
- `TestContainers.exposeHostPorts(...ports)` routes host ports into container networking via the port-forwarder sidecar.

[RAIL_LAW]:
- Package: `testcontainers`
- Owns: Docker container and Compose environment lifecycle for test isolation
- Accept: one `GenericContainer` per service; `await using` for deterministic cleanup; `Wait.*` for readiness
- Reject: hand-rolled Docker API calls in test setup; polling without a `WaitStrategy`
