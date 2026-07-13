# [TS_IAC_API_PULUMI_DOCKER]

`@pulumi/docker` is the Docker-Engine provider backing the `selfhosted-docker` dispatch arm's RUNTIME resources. It is a bridged provider: 13 resource classes share ONE Pulumi resource ABI (`new X(name, XArgs, opts?)` + `static get`/`isInstance` + `XArgs`/`XState`), so the catalog documents the ABI as the mechanism and seeds it with the families the deploy plane composes — the `Container`/`Network`/`Volume` runtime trio, the `Service`/`ServiceConfig`/`Secret` swarm trio, and the `RegistryImage`/`RemoteImage`/`Tag`/`Plugin`/`BuildxBuilder` registry set. Its `Image` (classic build-and-push over a `DockerBuild`/`Registry` spec → an immutable `repoDigest`) is the RETIRED build path, SUPERSEDED by the admitted buildx-native `@pulumi/docker-build.Image` (`.api/pulumi-docker-build.md`) — `@pulumi/docker` stays the runtime/swarm/registry owner, but a new image build is authored on `docker-build`. Six `getX`/`getXOutput` `Promise`/`Output` mirror pairs read existing daemon state. The `Provider` carries the daemon connection — a local socket OR an `ssh://` host (`sshOpts`) with `registryAuth`, so one selfhosted-docker arm targets an owned VPS daemon that `@pulumi/command` bootstrapped.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/docker`
- package: `@pulumi/docker`
- license: Apache-2.0
- import: `@pulumi/docker` (flat resource + function barrel; `{ config, types }` namespaces)
- owner: `iac`
- rail: fabric / selfhosted-docker
- runtime: a reachable Docker Engine (local socket, TCP+TLS, or `ssh://`); build resources also need a local Docker/buildx CLI on the deploy host
- build-floor: `@pulumi/pulumi` `^catalog`
- depends-on: `@pulumi/pulumi` (`CustomResource`/`Input`/`Output`); the modern buildx-native build owner is the admitted sibling `@pulumi/docker-build` (`.api/pulumi-docker-build.md`) — `@pulumi/docker.Image` is the RETIRED build-and-push it supersedes
- namespaces: root barrel (all resources + `getX`/`getXOutput`), `docker.config` (ambient provider vars), `docker.types.input`/`docker.types.output` (`DockerBuild`/`Registry`/`CacheFrom`/`ContainerPort`/`ContainerMount`/`ServiceTaskSpec`/…)
- capability: container/network/volume runtime, swarm service/config/secret, registry pull/push/tag, plugin + buildx-builder management, daemon-state data sources, `ssh://`-remote daemon targeting, retired image build-and-push (superseded by `@pulumi/docker-build`)
- abi-note: every resource output prop mirrors its `Args` field resolved through `Output<T>`; `XState` (the `.get` shape) mirrors `XArgs` with all-optional Output fields

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized resource shape every class instantiates
- rail: selfhosted-docker
- One shape owns all 13 resources; a new one is a row on this pattern, never a new mechanism. Construction is `new X(name, XArgs, opts?)`; `opts` is the universal `pulumi.CustomResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`/`ignoreChanges`/`import`, `.api/pulumi-pulumi.md`). Adoption of daemon-existing objects is `static get`; every output prop is an `Output<T>` mirror of the corresponding arg.

| [INDEX] | [MEMBER]                          | [SHAPE_BOUNDARY]                                                     |
| :-----: | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)`       | construct any resource; `XArgs` fields are `Input<T>`                |
|  [02]   | `X.get(name, id, XState?, opts?)` | adopt an existing daemon object by id into the graph                 |
|  [03]   | `X.isInstance(obj)`               | multi-SDK-safe type guard `obj is X`                                 |
|  [04]   | `x.<prop>: Output<T>`             | resolved output mirror of each arg; thread via `.apply`/`pulumi.all` |

## [03]-[RESOURCE_FAMILIES]

[BUILD_SCOPE]: image build-and-push
- rail: selfhosted-docker
- `Image` (RETIRED — prefer `@pulumi/docker-build.Image`, `.api/pulumi-docker-build.md`) runs a build then pushes to `registry`, emitting an immutable `repoDigest` — the value downstream resources pin, never a mutable tag. It constructs with `imageName`, `build?: DockerBuild`, `registry?: Registry`, `skipPush?`, `buildOnPreview?` and emits `repoDigest`/`imageName`/`registryServer`/`baseImageName`. `RegistryImage`/`RemoteImage` are the push-metadata / pull complements; `Tag`/`BuildxBuilder` manage tagging and the buildx builder instance — these registry/tag resources stay `@pulumi/docker`-owned, only the `Image` BUILD role moves to `docker-build`.

| [INDEX] | [SYMBOL]                        | [SHAPE]                                                                                             |
| :-----: | :------------------------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Image`                         | RETIRED build+push → immutable `repoDigest` (pin this, never a tag)                                 |
|  [02]   | `types.input.DockerBuild`       | `context`/`dockerfile`/`args`/`cacheFrom`/`target`/`platform`/`network`/`addHosts`/`builderVersion` |
|  [03]   | `types.input.Registry`          | `server`/`username`/`password` (bind a `doppler` secret Output)                                     |
|  [04]   | `RegistryImage` / `RemoteImage` | push registry metadata / pull a remote image into daemon state                                      |
|  [05]   | `Tag` / `BuildxBuilder`         | re-tag an image / manage a buildx builder instance                                                  |

[RUNTIME_SCOPE]: container runtime
- rail: selfhosted-docker
- The service-equivalence workloads of the selfhosted-docker arm. `Container` is the deep runtime surface: pin `image` to an `Image.repoDigest`; `command`/`entrypoints` shape the process, `envs` carries `KEY=VALUE` rows, `gpus`/`devices`/`capabilities`/`memory`/`cpus` bound resources, and `labels` tag it. `Network` and `Volume` each emit `name: Output<string>`. The nested arg records below are the `ports`/`networksAdvanced`/`volumes`/`mounts` shapes.

| [INDEX] | [SYMBOL]                    | [KEY_ARGS]                                                                                                 |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `Container`                 | `image`, `ports`, `envs`, `mounts`/`volumes`, `networksAdvanced`, `healthcheck`, `restart`/`maxRetryCount` |
|  [02]   | `Network`                   | `driver`, `ipamConfigs`/`ipamDriver`, `attachable`, `internal`, `ingress`, `options`, `labels`             |
|  [03]   | `Volume`                    | `driver`/`driverOpts`, `cluster`, `labels`                                                                 |
|  [04]   | `ContainerPort`             | `internal` (required), `external?`, `ip?`, `protocol?`                                                     |
|  [05]   | `ContainerNetworksAdvanced` | `name` (required — bind `Network.name`), `aliases?`, `ipv4Address?`, `driverOpts?`                         |
|  [06]   | `ContainerVolume`           | `volumeName?` (bind `Volume.name`), `containerPath?`, `hostPath?`, `readOnly?`, `fromContainer?`           |
|  [07]   | `ContainerMount`            | `target`, `type` required, `source?`, `readOnly?`, `bindOptions?`, `volumeOptions?`, `tmpfsOptions?`       |

[SWARM_SCOPE]: swarm-mode services
- rail: selfhosted-docker
- Replicated/global swarm workloads with rolling `updateConfig`/`rollbackConfig` and `endpointSpec` publish. `ServiceConfig`/`Secret` inject config and secret material (bind `Secret.data` to a `doppler`-sourced Output).

| [INDEX] | [SYMBOL]                   | [KEY_ARGS]                                                                                              |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Service`                  | `taskSpec`, `mode`, `endpointSpec`, `updateConfig`/`rollbackConfig`, `convergeConfig`, `auth`, `labels` |
|  [02]   | `ServiceConfig` / `Secret` | `data`/`dataRaw`, `labels` — swarm config / secret material                                             |
|  [03]   | `Plugin`                   | managed engine plugin (`enabled`, `grantAllPermissions`, `envs`)                                        |

## [04]-[DATA_SOURCES]

[DATASOURCE_SCOPE]: daemon-state reads — the `Promise`/`Output` mirror pair
- rail: selfhosted-docker
- Every read exposes both `getX(args, InvokeOptions?): Promise<GetXResult>` (eager, for an `async` inline program) and `getXOutput(args, InvokeOutputOptions?): Output<GetXResult>` (graph-threaded). Choose `getXOutput` when the fact feeds a resource `Input`; never re-derive existing daemon state by shelling out.

| [INDEX] | [PAIR]                                  | [READS]                                    |
| :-----: | :-------------------------------------- | :----------------------------------------- |
|  [01]   | `getLogs` / `getLogsOutput`             | container log lines                        |
|  [02]   | `getNetwork` / `getNetworkOutput`       | an existing network (id/driver/ipam/scope) |
|  [03]   | `getPlugin` / `getPluginOutput`         | an installed plugin                        |
|  [04]   | `getRegistryImage` / `…Output`          | registry image digest/metadata             |
|  [05]   | `getRegistryImageManifests` / `…Output` | multi-arch manifest list                   |
|  [06]   | `getRemoteImage` / `…Output`            | a pulled remote image's repo digest        |

## [05]-[PROVIDER]

[PROVIDER_SCOPE]: the daemon connection — the selfhosted-docker credential seam
- rail: selfhosted-docker
- The dispatch arm constructs ONE `Provider` and passes it to every resource via `opts.provider`. `host` selects the daemon: a local socket, `tcp://host:2376` with `caMaterial`/`certMaterial`/`keyMaterial`, or `ssh://user@vps` with `sshOpts` — the same VPS `@pulumi/command` bootstrapped. `registryAuth` authenticates image pushes; `config.*` mirrors these as ambient vars.

| [INDEX] | [FIELD]                                       | [TYPE]                             | [MEANING]                                         |
| :-----: | :-------------------------------------------- | :--------------------------------- | :------------------------------------------------ |
|  [01]   | `host`                                        | `Input<string>`                    | daemon endpoint (`unix://` / `tcp://` / `ssh://`) |
|  [02]   | `sshOpts`                                     | `Input<string[]>`                  | extra `ssh` args for an `ssh://` host             |
|  [03]   | `caMaterial` / `certMaterial` / `keyMaterial` | `Input<string>`                    | inline mutual-TLS material for a `tcp://` daemon  |
|  [04]   | `certPath`                                    | `Input<string>`                    | TLS cert directory path, alternative to inline    |
|  [05]   | `registryAuth`                                | `Input<ProviderRegistryAuth[]>`    | per-registry push credentials                     |
|  [06]   | `context` / `disableDockerDaemonCheck`        | `Input<string>` / `Input<boolean>` | Docker CLI context / skip the reachability probe  |

## [06]-[IMPLEMENTATION_LAW]

[DOCKER_TOPOLOGY]:
- arm law: the `selfhosted-docker` `Match.exhaustive` arm (`provider/dispatch`, `libs/typescript/.api/effect.md`) constructs one `docker.Provider` from a `Schema`-decoded `StackSpec` (daemon host, registry ref) and threads it as `opts.provider` to every resource; a per-resource provider is rejected.
- digest law: a workload pins `Container.image`/a downstream image ref to `Image.repoDigest` (immutable), never to a mutable tag; the build → run edge is `dependsOn`-implicit through the Output reference.
- build-owner law: `@pulumi/docker-build.Image` (`.api/pulumi-docker-build.md`) is the CANONICAL modern buildx-native build owner; `@pulumi/docker.Image` is the RETIRED build-and-push it supersedes — author `docker-build.Image` for a new image, reach for the classic `docker.Image` only to adopt an existing retired resource, and never hand-roll a `command`-shelled `docker build`.
- runtime law: the service-equivalence rows are `Container` + `Network` + `Volume`; ports/mounts/networks are typed args, not an authored compose file (the folder authors zero YAML).
- read law: existing daemon state is a `getXOutput` when it feeds an `Input`, `getX` for an eager `async` read; never a `local.Command` shell probe.

[LOCAL_ADMISSION]:
- `Provider.host: "ssh://user@vps"` targets the daemon on the metal that `@pulumi/command` (`.api/pulumi-command.md`) bootstrapped; `registryAuth`/`Registry.password` bind `@pulumiverse/doppler` secret Outputs (`.api/pulumiverse-doppler.md`), never literals.
- a built image digest crosses arms: the `docker-build.Image` `ref`/`digest` (`.api/pulumi-docker-build.md`; or the retired `docker.Image.repoDigest`) feeds a `@pulumi/kubernetes` workload image ref (`.api/pulumi-kubernetes.md`) when a stack mixes selfhosted-docker builds with a k8s runtime; `awsx.ecr.Image` (`.api/pulumi-awsx.md`, itself bundling `docker-build`) is the ECR-targeted build counterpart on the `aws` arm.
- the arm folds build/run failures into the `program/automation` typed run receipt (`@pulumi/pulumi` `automation.UpResult`, `.api/pulumi-pulumi.md`); `effect` owns the arm dispatch and the StackSpec/StackOutputs `Schema`.
- canonical-spelling law: `getXOutput` for graph-threaded reads; `Image.repoDigest` (immutable) over a tag; one shared `Provider` per arm.

[RAIL_LAW]:
- Package: `@pulumi/docker`
- Owns: container/network/volume runtime, swarm service/config/secret, registry pull/push/tag, plugin + buildx management, daemon-state data sources, `ssh://`-remote daemon targeting; retired image build-and-push (superseded by `@pulumi/docker-build`)
- Accept: one arm-scoped `Provider` (local/`tcp`/`ssh`), a `docker-build.Image` digest / `ref`-pinned `Container`, typed `Network`/`Volume` wiring, `getXOutput` reads, Doppler-bound registry auth
- Reject: mutable-tag image refs, the classic `docker.Image` build path where `docker-build.Image` is available, a `command`-shelled `docker build`/`docker run`, per-resource providers, authored compose YAML, literal registry credentials
