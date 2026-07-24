# [TS_IAC_API_PULUMI_DOCKER]

`@pulumi/docker` owns the Docker-Engine runtime surface behind the `selfhosted-docker` dispatch arm — container/swarm/registry workloads, daemon-state reads, and the `Provider` targeting a local, `tcp://`, or `ssh://` daemon. Every resource shares one Pulumi ABI, so a new resource is a row on the construction pattern; a new image build routes to `@pulumi/docker-build.Image` (`.api/pulumi-docker-build.md`), leaving `docker.Image` for adoption of an existing build resource alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/docker`
- package: `@pulumi/docker` (Apache-2.0)
- import: `@pulumi/docker` flat resource + function barrel `{ config, types }`; `config` carries ambient provider vars, `types.input`/`types.output` carry the `DockerBuild`/`Registry`/`ContainerPort`/`ContainerMount`/`ServiceTaskSpec`/… arg trees
- rail: fabric / selfhosted-docker
- runtime: a reachable Docker Engine — local socket, `tcp://`+TLS, or `ssh://`
- depends-on: `@pulumi/pulumi` `CustomResource`/`Input`/`Output` (`.api/pulumi-pulumi.md`)
- abi-note: every output prop mirrors its `Args` field as `Output<T>`; `XState` (the `.get` shape) mirrors `XArgs` with all-optional Output fields

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized resource shape every class instantiates
- One shape owns every resource; a new one is a row on this pattern, never a new mechanism. Construction is `new X(name, XArgs, opts?)` where `opts` is the universal `pulumi.CustomResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`/`ignoreChanges`/`import`, `.api/pulumi-pulumi.md`); adoption is `static get` and every output prop is an `Output<T>` mirror of its arg.

| [INDEX] | [MEMBER]                          | [SHAPE_BOUNDARY]                                                     |
| :-----: | :-------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)`       | construct any resource; `XArgs` fields are `Input<T>`                |
|  [02]   | `X.get(name, id, XState?, opts?)` | adopt an existing daemon object by id into the graph                 |
|  [03]   | `X.isInstance(obj)`               | multi-SDK-safe type guard `obj is X`                                 |
|  [04]   | `x.<prop>: Output<T>`             | resolved output mirror of each arg; thread via `.apply`/`pulumi.all` |

## [03]-[RESOURCE_FAMILIES]

[BUILD_SCOPE]: image build-and-push
- `Image` builds then pushes to `registry`, emitting the immutable `repoDigest` a workload pins — never a mutable tag. It constructs with `imageName`/`build`/`registry`/`skipPush`/`buildOnPreview` and emits `repoDigest`/`imageName`/`registryServer`/`baseImageName`; a new build authors on `@pulumi/docker-build.Image` (`.api/pulumi-docker-build.md`) and `docker.Image` here only adopts an existing build resource. `RegistryImage`/`RemoteImage` are the push-metadata/pull complements; `Tag`/`BuildxBuilder` manage tagging and the buildx builder.

| [INDEX] | [SYMBOL]                        | [SHAPE]                                                                                             |
| :-----: | :------------------------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Image`                         | build+push → immutable `repoDigest` (pin this, never a tag)                                         |
|  [02]   | `types.input.DockerBuild`       | `context`/`dockerfile`/`args`/`cacheFrom`/`target`/`platform`/`network`/`addHosts`/`builderVersion` |
|  [03]   | `types.input.Registry`          | `server`/`username`/`password` (bind a `doppler` secret Output)                                     |
|  [04]   | `RegistryImage` / `RemoteImage` | push registry metadata / pull a remote image into daemon state                                      |
|  [05]   | `Tag` / `BuildxBuilder`         | re-tag an image / manage a buildx builder instance                                                  |

[RUNTIME_SCOPE]: container runtime
- `Container` is the deep runtime surface: `image` pins an immutable digest, `command`/`entrypoints` shape the process, `envs` carries `KEY=VALUE` rows, `gpus`/`devices`/`capabilities`/`memory`/`cpus` bound resources, `labels` tag it. `Network` and `Volume` each emit `name: Output<string>`; the `ContainerPort`/`ContainerNetworksAdvanced`/`ContainerVolume`/`ContainerMount` rows are the nested `ports`/`networksAdvanced`/`volumes`/`mounts` arg records.

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
- Replicated or global swarm workloads carry rolling `updateConfig`/`rollbackConfig` and `endpointSpec` publish. `ServiceConfig`/`Secret` inject config and secret material — bind `Secret.data` to a Doppler-sourced `Output`.

| [INDEX] | [SYMBOL]                   | [KEY_ARGS]                                                                                              |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Service`                  | `taskSpec`, `mode`, `endpointSpec`, `updateConfig`/`rollbackConfig`, `convergeConfig`, `auth`, `labels` |
|  [02]   | `ServiceConfig` / `Secret` | `data`/`dataRaw`, `labels` — swarm config / secret material                                             |
|  [03]   | `Plugin`                   | managed engine plugin (`enabled`, `grantAllPermissions`, `envs`)                                        |

## [04]-[DATA_SOURCES]

[DATASOURCE_SCOPE]: daemon-state reads — the `Promise`/`Output` mirror pair
- Each read exposes `getX(args, InvokeOptions?): Promise<GetXResult>` (eager, for an `async` inline program) and `getXOutput(args): Output<GetXResult>` (graph-threaded); reach for `getXOutput` when the fact feeds a resource `Input`, never a shell probe.

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
- Each `selfhosted-docker` arm constructs one `Provider` and passes it to every resource via `opts.provider`. `host` selects the daemon — a local socket, `tcp://host:2376` with `caMaterial`/`certMaterial`/`keyMaterial`, or `ssh://user@vps` with `sshOpts`; `registryAuth` authenticates pushes and `config.*` mirrors these as ambient vars.

| [INDEX] | [FIELD]                                       | [TYPE]                             | [MEANING]                                         |
| :-----: | :-------------------------------------------- | :--------------------------------- | :------------------------------------------------ |
|  [01]   | `host`                                        | `Input<string>`                    | daemon endpoint (`unix://` / `tcp://` / `ssh://`) |
|  [02]   | `sshOpts`                                     | `Input<string[]>`                  | extra `ssh` args for an `ssh://` host             |
|  [03]   | `caMaterial` / `certMaterial` / `keyMaterial` | `Input<string>`                    | inline mutual-TLS material for a `tcp://` daemon  |
|  [04]   | `certPath`                                    | `Input<string>`                    | TLS cert directory path, alternative to inline    |
|  [05]   | `registryAuth`                                | `Input<ProviderRegistryAuth[]>`    | per-registry push credentials                     |
|  [06]   | `context` / `disableDockerDaemonCheck`        | `Input<string>` / `Input<boolean>` | Docker CLI context / skip the reachability probe  |

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Provider` decoded from the arm's `StackSpec` threads to every resource via `opts.provider`; a per-resource provider is rejected.
- A workload pins `Container.image` and every downstream image ref to an immutable digest (`Image.repoDigest`), never a mutable tag; the build → run edge rides the `Output` reference as an implicit `dependsOn`.
- Existing daemon state reads through `getXOutput` when it feeds an `Input` and `getX` for an eager `async` read, never a shell probe.
- Runtime workloads are `Container` + `Network` + `Volume` over typed `ports`/`mounts`/`networks` args; the folder authors zero compose YAML.

[STACKING]:
- `@pulumi/docker-build`(`.api/pulumi-docker-build.md`): the canonical image build owner; its `Image` `digest`/`ref` pins a `Container.image` here, and `docker.Image` builds only to adopt an existing resource.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `registryAuth`, `Registry.password`, and `Secret.data` bind its secret `Output`s, never literals.
- `@pulumi/command`(`.api/pulumi-command.md`): an `ssh://` `Provider.host` targets the VPS daemon `command` bootstrapped.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`) + `@pulumi/awsx`(`.api/pulumi-awsx.md`): a built image digest crosses into a k8s workload image ref when a stack mixes selfhosted-docker with k8s; `awsx.ecr.Image` is the ECR-targeted build counterpart on the `aws` arm.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): `opts` is `CustomResourceOptions`, every output prop threads as `Output<T>` via `.apply`/`pulumi.all`, and the arm folds the run into `automation.UpResult`.
- effect(`libs/typescript/.api/effect.md`): the `selfhosted-docker` `Match.exhaustive` arm decodes the `StackSpec` through `Schema` and owns the arm dispatch.

[LOCAL_ADMISSION]:
- `Provider.host` targets the `ssh://user@vps` daemon `@pulumi/command` bootstrapped on the metal, and registry auth binds `@pulumiverse/doppler` `Output`s — the two repo-specific bindings the arm supplies at composition.

[RAIL_LAW]:
- Package: `@pulumi/docker`
- Owns: container/network/volume runtime, swarm service/config/secret, registry pull/push/tag, plugin + buildx-builder management, daemon-state data sources, `ssh://`-remote daemon targeting
- Accept: one arm-scoped `Provider` (local/`tcp`/`ssh`), a digest-pinned `Container`, typed `Network`/`Volume` wiring, `getXOutput` reads, Doppler-bound registry auth
- Reject: mutable-tag image refs, `docker.Image` for a new build, a `command`-shelled `docker build`/`docker run`, per-resource providers, authored compose YAML, literal registry credentials
