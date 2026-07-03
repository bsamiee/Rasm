# [API_CATALOGUE] @pulumi/docker

`@pulumi/docker` is the Docker-engine provider and, at v5, the image-BUILD owner (the `iac` roster pins it over the unadmitted `@pulumi/docker-build` 0.0.x). Its surface is generated from the provider schema into three parameterized families, not a hand-picked roster: a `<Resource>` / `<Resource>Args` / `<Resource>State` triple per managed resource (13 of them — `Image`, `Container`, `Network`, `Volume`, `BuildxBuilder`, `RemoteImage`, `RegistryImage`, `Tag`, `Plugin`, `Service`, `ServiceConfig`, `Secret`, plus `Provider`); a `getX` / `getXOutput` async/output-mirror per data source (6 lookups); and the `types.input.<Resource><Facet>` / `types.output.<Resource><Facet>` nested-property family (~90 interfaces filled inline as `Input<…>` object literals). For the self-hosted deploy tier the load-bearing resources are `Image` (build+push), `Container`/`Network`/`Volume` (the Docker-compose-equivalent executor), and `Provider` (daemon connection); the Swarm resources (`Service`/`ServiceConfig`/`Secret`) and `BuildxBuilder`/`Plugin` are the fuller orchestration surface.

- package: `@pulumi/docker`
- version: `5.1.0`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, `./provisioning` (`iac`) subpath; never on the runtime hot path, never browser-reachable.
- rail: deployment

## [01]-[PACKAGE_SURFACE]

Every resource is a `pulumi.CustomResource`; every lookup returns a bare `Promise` (async) and an `Output`-wrapped mirror. The `<Resource>State` type is the input to `<Resource>.get(name, id, state?, opts?)` — the adopt-existing-object escape hatch — so the triple is uniform across all 13.

[RESOURCES]: `new <R>(name, args: <R>Args, opts?)` + `<R>.get(name, id, state?, opts?)`:

| [INDEX] | [RESOURCE]      | [REQUIRED_ARG]        | [ROLE]                                                   |
| :-----: | :-------------- | :-------------------- | :------------------------------------------------------ |
|  [01]   | `Image`         | `imageName`           | build a `DockerBuild` context and push to a `Registry`  |
|  [02]   | `Container`     | `image`               | lifecycle-managed container (self-hosted executor)      |
|  [03]   | `Network`       | —                     | Docker network (bridge/overlay, IPAM)                   |
|  [04]   | `Volume`        | —                     | Docker volume (local or cluster driver)                 |
|  [05]   | `BuildxBuilder` | —                     | buildx builder instance (docker-container/k8s/remote)   |
|  [06]   | `RemoteImage`   | `name`                | pull or build an image into the local daemon            |
|  [07]   | `RegistryImage` | —                     | push+manage an image's registry lifecycle               |
|  [08]   | `Tag`           | `sourceImage`,`targetImage` | tag a source image as a target ref                |
|  [09]   | `Plugin`        | —                     | install/enable a Docker engine plugin                   |
|  [10]   | `Service`       | `taskSpec`            | Swarm service (replicated/global task spec)             |
|  [11]   | `ServiceConfig` | —                     | Swarm config object                                     |
|  [12]   | `Secret`        | `data`                | Swarm secret object                                     |
|  [13]   | `Provider`      | —                     | explicit daemon handle for `ResourceOptions.provider`   |

[LOOKUPS]: `getX(args, opts?): Promise<GetXResult>` + `getXOutput(args, opts?): Output<GetXResult>` — one mirror pair per data source: `getLogs` (container log lines), `getNetwork` (network by name), `getPlugin` (installed plugin), `getRegistryImage` (registry digest — `sha256Digest`), `getRegistryImageManifests` (multi-arch manifest list), `getRemoteImage` (local image metadata). Each also exports `GetXArgs` / `GetXResult` / `GetXOutputArgs`.

## [02]-[IMAGE_AND_BUILD]

[IMAGE]: `Image` — build a context and (unless `skipPush`) push it. `repoDigest` is the unique per-build identity; wire it, not `imageName`, into anything that must update on rebuild.

```ts
interface ImageArgs { imageName: Input<string>; build?: Input<types.input.DockerBuild>; registry?: Input<types.input.Registry>; skipPush?: Input<boolean>; buildOnPreview?: Input<boolean> }
// outputs: imageName, repoDigest, baseImageName, context, dockerfile, registryServer: Output<string>; platform: Output<string | undefined>
```

[BUILD_INPUTS]: the `types.input.*` build shapes filled inline:

```ts
interface DockerBuild { context?: Input<string>; dockerfile?: Input<string>; args?: Input<{[k:string]: Input<string>}>; cacheFrom?: Input<CacheFrom>; target?: Input<string>; platform?: Input<string>; network?: Input<string>; addHosts?: Input<string[]>; builderVersion?: Input<enums.BuilderVersion> }
interface Registry  { server?: Input<string>; username?: Input<string>; password?: Input<string> }   // wrap password in pulumi.secret()
interface CacheFrom { images?: Input<string[]> }   // images must carry a cache manifest; auth to the cache registry is separate
```

[BUILDX]: `BuildxBuilder` — a named buildx builder the build binds to (multi-node, remote, or k8s-driven builds): `driver?`, `driverOptions?`, `platforms?`, `dockerContainer?`/`kubernetes?`/`remote?` (per-driver config), `bootstrap?`, `use?`, `append?`, `buildkitConfig?`/`buildkitFlags?`, `name?`, `node?`, `endpoint?`.

[REGISTRY_AND_TAG]: `RemoteImage` (`name` required; `build?: RemoteImageBuild`, `pullTriggers?`, `keepLocally?`, `forceRemove?`, `platform?`, `triggers?`), `RegistryImage` (`name?`, `build?: RegistryImageBuild`, `authConfig?`, `keepRemotely?`, `insecureSkipVerify?`, `triggers?`; output `sha256Digest`), `Tag` (`sourceImage`, `targetImage`, `tagTriggers?`).

## [03]-[CONTAINER_TIER]

[CONTAINER]: `Container` — the self-hosted-executor resource (the Docker-compose equivalent). `ContainerArgs` is ~55 fields; the load-bearing set the deploy tier fills, with the rest reachable through the `types.input.Container*` sub-family (`ContainerHealthcheck`, `ContainerPort`, `ContainerMount`/`ContainerMountBindOptions`/`…TmpfsOptions`/`…VolumeOptions`, `ContainerVolume`, `ContainerUlimit`, `ContainerCapabilities`, `ContainerDevice`, `ContainerNetworksAdvanced`, `ContainerHost`, `ContainerLabel`, `ContainerDeviceRead/WriteBp`/`Iop`, `ContainerDeviceRequest`):

```ts
interface ContainerArgs {          // image required; the rest optional
  image: Input<string>             // pass Image.repoDigest / RemoteImage.imageId to force update-on-rebuild
  name?: Input<string>; command?: Input<string[]>; entrypoints?: Input<string[]>; envs?: Input<string[]>
  ports?: Input<ContainerPort[]>; mounts?: Input<ContainerMount[]>; volumes?: Input<ContainerVolume[]>
  networkMode?: Input<string>; networksAdvanced?: Input<ContainerNetworksAdvanced[]>
  healthcheck?: Input<ContainerHealthcheck>; capabilities?: Input<ContainerCapabilities>; devices?: Input<ContainerDevice[]>
  restart?: Input<string>; labels?: Input<ContainerLabel[]>; memory?: Input<number>; cpus?: Input<string>; gpus?: Input<string>
  privileged?: Input<boolean>; readOnly?: Input<boolean>; user?: Input<string>; workingDir?: Input<string>; attach?: Input<boolean>
  // + cpuShares/cpuPeriod/cpuQuota, ulimits, dns/dnsOpts/dnsSearches, hosts, init, runtime, pidMode/ipcMode, logDriver/logOpts, …
}
// outputs: id, bridge, command: Output<string[]>, containerLogs (when attach), image, name, ports, networkDatas: Output<ContainerNetworkData[]>, …
```

[NETWORK_VOLUME]: `Network` (`driver?`, `attachable?`, `internal?`, `ingress?`, `ipamDriver?`, `ipamConfigs?: NetworkIpamConfig[]`, `ipamOptions?`, `labels?: NetworkLabel[]`, `options?`, `name?`; outputs `name`/`driver`/`scope`/`internal`/`ipv6`), `Volume` (`driver?`, `driverOpts?`, `cluster?: VolumeCluster`, `labels?: VolumeLabel[]`, `name?`).

[SWARM]: `Service` (`taskSpec: ServiceTaskSpec` required — the `ServiceTaskSpec*` sub-family carries containerSpec/resources/placement/restartPolicy; plus `mode?: ServiceMode`, `endpointSpec?`, `updateConfig?`/`rollbackConfig?`, `convergeConfig?`, `auth?`, `labels?`), `ServiceConfig` (`data?`/`dataRaw?`, `labels?`, `name?`), `Secret` (`data` required, `labels?`, `name?`).

[PROVIDER]: `Provider` — the daemon connection: `host?` (`unix://`/`tcp://`/`ssh://`), `context?`, `sshOpts?`, `caMaterial?`/`certMaterial?`/`keyMaterial?`/`certPath?` (TLS), `registryAuth?: ProviderRegistryAuth[]`, `disableDockerDaemonCheck?`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Image.repoDigest` is `repository@sha256:<hash>` for pushed images and the local image ID for `skipPush: true`; it changes every rebuild, so it is the correct cross-resource reference. `imageName` is stable across rebuilds with the same tag and must NOT be the update trigger.
- `RemoteImage` does not re-pull unless `pullTriggers` includes a changing digest — feed `getRegistryImage(...).sha256Digest` as a `pullTrigger` for pull-on-update.
- `DockerBuild.platform` is a single platform string; multi-arch builds go through a `BuildxBuilder` driver, not this field.
- `<Resource>State` + `<Resource>.get(name, id, state?, opts?)` adopts an already-running engine object into state without recreating it.
- `RegistryImage.keepRemotely: true` preserves the remote tag on `pulumi destroy`.

[DEPLOY_STACK]: how the `provisioning/contract#PROVISIONING` self-hosted arm stacks this onto `@pulumi/pulumi` (`pulumi-pulumi.md`) core and the Effect rails.
- The `DeployMode` `Match.exhaustive` `self-hosted` arm builds the compute tier from `Container` + `Network` + `Volume` (the RDS→postgres-alpine / ElastiCache→redis-alpine / S3→MinIO-Garage equivalence rows), each a child of the `TierStack` `ComponentResource` (`{ parent: this }`); the observe tier is a Traefik/Alloy `Container` on the same `Network`.
- Cross-resource: `Container.image` takes the `Image.repoDigest` `Output<string>` so a rebuilt image redeploys the container; `Container.networksAdvanced` references the `Network.name` `Output`; `Volume.name` feeds `Container.volumes`. ACME/Traefik cert bootstrap and one-shot steps ride a `@pulumi/command` `local.Command` gated on the container's readiness via `dependsOn`.
- Effect boundary: the whole graph is applied through the `@pulumi/pulumi/automation` `Stack` under `Effect.tryPromise`/`Effect.async` (the deploy/drift fold in `pulumi-pulumi.md`); `Registry.password`/daemon TLS material arrive as `Config.requireSecret`/`pulumi.secret()` `Output`s so they never widen in state.

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Output`/`Input` algebra, `ComponentResource`/`registerOutputs`, and the `CustomResourceOptions` (`parent`/`dependsOn`/`provider`/`protect`) these constructors take.
- `@pulumi/awsx` (`pulumi-awsx.md`) is the CLOUD-tier image-build peer (`awsx.ecr.Image` builds+pushes to ECR); `@pulumi/docker` owns the self-hosted daemon build of the same `DockerBuild` shape — the `DeployMode` dispatch selects which, never a parallel program.
- `@pulumi/kubernetes` (`pulumi-kubernetes.md`) is the cloud peer this self-hosted tier mirrors resource-for-resource; `@pulumi/command` (`pulumi-command.md`) owns the imperative steps `Container` cannot express; `effect` owns the `Match.exhaustive` `DeployMode` fold and the `Effect.tryPromise` boundary.

[RAIL_LAW]:
- Package: `@pulumi/docker`
- Owns: Docker-engine resource lifecycle — image build/push, containers, networks, volumes, buildx builders, Swarm services/configs/secrets, plugins, tags, and registry/log lookups.
- Accept: `Image.repoDigest` (never `imageName`) as the cross-resource update trigger; `getRegistryImage().sha256Digest` as a `RemoteImage.pullTriggers` entry; the `types.input.Container*` family filled inline for the self-hosted executor; `Registry.password`/daemon TLS as `pulumi.secret()`.
- Reject: `Image.imageName` as an update-propagating reference; multi-arch through `DockerBuild.platform` instead of `BuildxBuilder`; a parallel cloud/self-hosted codebase instead of the one `DeployMode` dispatch.
