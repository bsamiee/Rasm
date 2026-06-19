# [API_CATALOGUE] @pulumi/docker

`@pulumi/docker` supplies Pulumi resource classes for Docker engine management: `Image` for building and pushing images with a `DockerBuild` context, `Container` for lifecycle-managed containers, `RemoteImage` for pulling registry images, `RegistryImage` for pushing images to registries, `Network` for Docker networks, `Volume` for volumes, and lookup functions `getLogs`, `getNetwork`, `getRemoteImage`, and `getRegistryImage` for querying existing Docker objects.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/docker`
- package: `@pulumi/docker`
- module: `@pulumi/docker`
- asset: Docker image build/push, container lifecycle, network, volume, registry resource classes, lookup functions
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image resource family
- rail: deployment

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                                                  |
| :-----: | :------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `Image`             | resource class | build and push a Docker image                           |
|  [02]   | `ImageArgs`         | args interface | `imageName` (required), `build`, `registry`, `skipPush` |
|  [03]   | `RemoteImage`       | resource class | pull or build an image from a registry                  |
|  [04]   | `RemoteImageArgs`   | args interface | `name` (required), `build`, `pullTriggers`              |
|  [05]   | `RegistryImage`     | resource class | push image to and manage lifecycle in registry          |
|  [06]   | `RegistryImageArgs` | args interface | `name`, `keepRemotely`, `authConfig`, `build`           |

[PUBLIC_TYPE_SCOPE]: Image output properties
- rail: deployment

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                 | [RAIL]                                                      |
| :-----: | :--------------------- | :---------------------------- | :---------------------------------------------------------- |
|  [01]   | `Image.imageName`      | `Output<string>`              | fully qualified image name `repository[:tag]`               |
|  [02]   | `Image.repoDigest`     | `Output<string>`              | manifest digest `repository@<alg>:<hash>` or local image ID |
|  [03]   | `Image.baseImageName`  | `Output<string>`              | base image name after push                                  |
|  [04]   | `Image.context`        | `Output<string>`              | build context path                                          |
|  [05]   | `Image.dockerfile`     | `Output<string>`              | Dockerfile path                                             |
|  [06]   | `Image.platform`       | `Output<string \| undefined>` | target platform, e.g. `linux/amd64`                         |
|  [07]   | `Image.registryServer` | `Output<string>`              | registry server hostname                                    |

[PUBLIC_TYPE_SCOPE]: build input types
- rail: deployment

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                                                                        |
| :-----: | :--------------- | :-------------- | :---------------------------------------------------------------------------- |
|  [01]   | `DockerBuild`    | input interface | `context`, `dockerfile`, `args`, `platform`, `cacheFrom`, `target`, `network` |
|  [02]   | `Registry`       | input interface | `server`, `username`, `password`                                              |
|  [03]   | `CacheFrom`      | input interface | `images: string[]` — list of images for layer cache                           |
|  [04]   | `BuilderVersion` | const enum      | Docker builder version selector                                               |

[PUBLIC_TYPE_SCOPE]: container resource family
- rail: deployment

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [RAIL]                                                                           |
| :-----: | :-------------- | :------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Container`     | resource class | lifecycle-managed Docker container                                               |
|  [02]   | `ContainerArgs` | args interface | `image` (required), `name`, `command`, `ports`, `envs`, `volumes`, `networkMode` |

[PUBLIC_TYPE_SCOPE]: Container output properties
- rail: deployment

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                               |
| :-----: | :------------------------ | :----------------- | :----------------------------------- |
|  [01]   | `Container.command`       | `Output<string[]>` | resolved start command               |
|  [02]   | `Container.bridge`        | `Output<string>`   | network bridge interface             |
|  [03]   | `Container.containerLogs` | `Output<string>`   | container logs when `attach` is done |

[PUBLIC_TYPE_SCOPE]: network and volume resource family
- rail: deployment

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [RAIL]                              |
| :-----: | :------------ | :------------- | :---------------------------------- |
|  [01]   | `Network`     | resource class | Docker network                      |
|  [02]   | `NetworkArgs` | args interface | `name`, `driver`, `ipv6`, `options` |
|  [03]   | `Volume`      | resource class | Docker volume                       |
|  [04]   | `VolumeArgs`  | args interface | `name`, `driver`, `driverOpts`      |

[PUBLIC_TYPE_SCOPE]: Network output properties
- rail: deployment

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]                  | [RAIL]                |
| :-----: | :----------------- | :----------------------------- | :-------------------- |
|  [01]   | `Network.name`     | `Output<string>`               | network name          |
|  [02]   | `Network.driver`   | `Output<string>`               | network driver        |
|  [03]   | `Network.scope`    | `Output<string>`               | network scope         |
|  [04]   | `Network.ipv6`     | `Output<boolean \| undefined>` | IPv6 enabled flag     |
|  [05]   | `Network.internal` | `Output<boolean>`              | internal-only network |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource constructors
- rail: deployment

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `new Image(name, args: ImageArgs, opts?)`                 | constructor    | build + optional push; `imageName` required |
|  [02]   | `new RemoteImage(name, args: RemoteImageArgs, opts?)`     | constructor    | pull or build image; `name` required        |
|  [03]   | `new RegistryImage(name, args: RegistryImageArgs, opts?)` | constructor    | push image to registry                      |
|  [04]   | `new Container(name, args: ContainerArgs, opts?)`         | constructor    | start container; `image` required           |
|  [05]   | `new Network(name, args: NetworkArgs, opts?)`             | constructor    | create Docker network                       |
|  [06]   | `new Volume(name, args: VolumeArgs, opts?)`               | constructor    | create Docker volume                        |

[ENTRYPOINT_SCOPE]: lookup functions
- rail: deployment

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :-------------------------------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `getLogs(args: GetLogsArgs, opts?): Promise<GetLogsResult>`           | async lookup   | fetch container log lines   |
|  [02]   | `getLogsOutput(args, opts?): Output<GetLogsResult>`                   | output lookup  | same, `Output`-wrapped      |
|  [03]   | `getNetwork(args: GetNetworkArgs, opts?): Promise<GetNetworkResult>`  | async lookup   | fetch network by name       |
|  [04]   | `getNetworkOutput(args, opts?): Output<GetNetworkResult>`             | output lookup  | same, `Output`-wrapped      |
|  [05]   | `getRemoteImage(args, opts?): Promise<GetRemoteImageResult>`          | async lookup   | fetch remote image metadata |
|  [06]   | `getRemoteImageOutput(args, opts?): Output<GetRemoteImageResult>`     | output lookup  | same, `Output`-wrapped      |
|  [07]   | `getRegistryImage(args, opts?): Promise<GetRegistryImageResult>`      | async lookup   | fetch registry image digest |
|  [08]   | `getRegistryImageOutput(args, opts?): Output<GetRegistryImageResult>` | output lookup  | same, `Output`-wrapped      |

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_TOPOLOGY]:
- `Image.repoDigest` is the unique per-build identifier; it is `repository@sha256:<hash>` for pushed images and `sha256:<hash>` for local-only images; pass `repoDigest` (not `imageName`) to container orchestration resources to force updates on image rebuild
- `ImageArgs.skipPush` skips registry push and leaves the image local only; `repoDigest` is then the local image ID
- `ImageArgs.build.cacheFrom.images` must point to an image with a cache manifest; authentication to the cache registry must be provided separately
- `RemoteImage` does not pull updated layers unless `pullTriggers` includes a digest; use `getRegistryImage().sha256Digest` as a trigger to enable pull-on-update

[CONTAINER_TOPOLOGY]:
- `ContainerArgs.image` accepts a plain string image name or an `Output<string>`; pass `RemoteImage.imageId` or `Image.imageName` to create a dependency
- `Container` is a stateful resource in Docker engine state; Pulumi tracks it in state and destroys/recreates on image or config changes

[LOCAL_ADMISSION]:
- `Registry.password` should be wrapped with `pulumi.secret()` when sourced from config or external credentials
- `DockerBuild.platform` accepts a single platform string (`linux/amd64`); multi-platform builds are not supported through this provider
- `RegistryImage.keepRemotely = true` prevents remote tag deletion on `pulumi destroy`; set to `false` for cleanup on destroy

[RAIL_LAW]:
- Package: `@pulumi/docker`
- Owns: Docker engine resource lifecycle — images, containers, networks, volumes
- Accept: `Image.repoDigest` for cross-resource image references that trigger updates; `getRegistryImage().sha256Digest` as `pullTriggers` on `RemoteImage`
- Reject: using `Image.imageName` as a cross-resource dependency when update propagation is needed; it does not change between builds with the same tag
