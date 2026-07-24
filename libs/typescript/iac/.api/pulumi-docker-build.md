# [TS_IAC_API_PULUMI_DOCKER_BUILD]

`@pulumi/docker-build` owns the buildx-native image build+push: multi-platform builds, pluggable cache and export backend families, by-value build secrets, SSH forwarding, and the `Index` manifest-list resource. Its `Image` supersedes the classic `@pulumi/docker.Image` build path and is the builder `awsx.ecr.Image` bundles internally; `Image`/`Index`/`Provider` share one Pulumi resource ABI, and the pushed `ref` (`tag@digest`) and `digest` outputs are the immutable values a downstream workload pins.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/docker-build`
- package: `@pulumi/docker-build` (Apache-2.0)
- import: `@pulumi/docker-build` barrel `{ Image, Index, Provider, config, types }` + enums `Platform`/`NetworkMode`/`CacheMode`; `config` carries ambient `host`/`registries`, `types.input`/`…output` carry the cache/export/context/dockerfile/registry/ssh arg trees
- rail: fabric / selfhosted-docker (canonical image build) + aws (bundled by `awsx.ecr.Image`)
- runtime: a reachable BuildKit backend — the `Provider` embeds a Docker client + buildx by default (`exec: true` shells a host `docker-buildx`); a push needs registry credentials
- depends-on: `@pulumi/pulumi` `CustomResource`/`ProviderResource`/`Input`/`Output` (`.api/pulumi-pulumi.md`)
- abi-note: `push` is required on `ImageArgs` — no implicit push, unlike classic `docker.Image`; every output prop mirrors its arg as `Output<T>`, with computed `ref`/`digest`/`contextHash`

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized resource shape all three classes instantiate
- `Image`/`Index`/`Provider` share `new X(name, XArgs, opts?)` where `opts` is the universal `pulumi.CustomResourceOptions`/`ResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`, `.api/pulumi-pulumi.md`); adoption is `static get(name, id, opts?)` and every output prop is an `Output<T>` mirror. `Provider` extends `pulumi.ProviderResource`; `Image`/`Index` extend `pulumi.CustomResource`.

| [INDEX] | [MEMBER]                    | [SHAPE_BOUNDARY]                                         |
| :-----: | :-------------------------- | :------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)` | construct; `XArgs` fields are `Input<T>`                 |
|  [02]   | `X.get(name, id, opts?)`    | adopt an existing image/index by id                      |
|  [03]   | `X.isInstance(obj)`         | multi-SDK-safe guard `obj is X`                          |
|  [04]   | `x.<prop>: Output<T>`       | resolved output mirror; thread via `.apply`/`pulumi.all` |

## [03]-[BUILD_SURFACE]

[IMAGE_SCOPE]: `Image` — the buildx build+push resource
- `Image` builds a Dockerfile through buildx and pushes under `push: true` or a `registry` export, emitting `ref` (`tag@digest`), `digest` (the sha256 a workload pins), and `contextHash`. `dockerfile`/`context` locate the build; `buildArgs`/`target`/`labels`/`tags`/`network` shape it; `platforms` drives multi-arch; `secrets`/`ssh` inject build material by value; `noCache`/`pull`/`load`/`builder`/`buildOnPreview`/`exec` are the execution knobs; `cacheFrom`/`cacheTo`/`exports` are the pluggable backend families below.

| [INDEX] | [SYMBOL]                       | [SHAPE]                                                               |
| :-----: | :----------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `Image`                        | `push` required — no implicit push → `ref` / `digest` / `contextHash` |
|  [02]   | `types.input.DockerfileArgs`   | `location` (path) XOR `inline` (literal Dockerfile body)              |
|  [03]   | `types.input.BuildContextArgs` | `location` (path/URL/git) + `named` (named build contexts)            |
|  [04]   | `types.input.RegistryArgs`     | `address`/`username`/`password` (bind a `doppler` secret Output)      |

[BACKEND_SCOPE]: the parameterized cache + export backend families
- `cacheFrom`/`cacheTo`/`exports` are each one parameterized slot; the set sub-key selects the backend, so a new backend lands as a row, never a new arg.

| [INDEX] | [SLOT]                       | [BACKEND_KEYS_ONE_PER_ENTRY]                                                           |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `cacheFrom: CacheFromArgs[]` | `registry` \| `local` \| `s3` \| `gha` \| `azblob` \| `raw`                            |
|  [02]   | `cacheTo: CacheToArgs[]`     | `registry` \| `local` \| `s3` \| `gha` \| `azblob` \| `inline` \| `raw`                |
|  [03]   | `exports: ExportArgs[]`      | `registry` \| `image` \| `docker` \| `oci` \| `local` \| `tar` \| `cacheonly` \| `raw` |
|  [04]   | `platforms: Platform[]`      | the multi-arch build targets (`Platform` enum constants)                               |

[INDEX_SCOPE]: `Index` — multi-platform manifest list
- `Index` fuses per-platform image tags (`sources`) into one multi-arch manifest under `tag`, emitting the combined `ref` — the manifest-list complement to a single per-`platforms` `Image`.

| [INDEX] | [SYMBOL] | [KEY_ARGS_OUTPUTS]                                                        |
| :-----: | :------- | :------------------------------------------------------------------------ |
|  [01]   | `Index`  | `{ sources (required tag refs), tag (required), push, registry }` → `ref` |

## [04]-[PROVIDER]

[PROVIDER_SCOPE]: the build-daemon connection
- Arm dispatch constructs one `Provider` and threads it via `opts.provider`. `host` selects the BuildKit endpoint (default the embedded client); `registries` pre-authenticates pushes; `config.*` mirrors these as ambient vars.

| [INDEX] | [FIELD]      | [TYPE]                  | [MEANING]                                          |
| :-----: | :----------- | :---------------------- | :------------------------------------------------- |
|  [01]   | `host`       | `Input<string>`         | build-daemon address (default the embedded client) |
|  [02]   | `registries` | `Input<RegistryArgs[]>` | pre-authenticated push registries                  |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `docker-build.Image` is the buildx-native build owner across the branch, superseding the classic `@pulumi/docker.Image` (`.api/pulumi-docker.md`) on the `selfhosted-docker` arm and serving as the builder `awsx.ecr.Image` (`.api/pulumi-awsx.md`) bundles on the `aws` arm; author `docker-build.Image` for a new image.
- A workload pins the immutable `digest` (sha256) or `ref` (`tag@digest`), never a mutable tag; `push` is required-explicit, so a build without an export stays local.
- A registry/s3/gha cache backend reuses layers across CI runs rather than rebuilding them.

[STACKING]:
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `registries[].password` and `secrets` bind its `Secret` `Output` by value — no on-disk or env material.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`) + `@pulumi/awsx`(`.api/pulumi-awsx.md`): `Image.digest`/`ref` feeds a workload image ref or an ECS TaskDefinition container image; `awsx.ecr.Image` bundles `docker-build` on the `aws` arm.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): a build failure folds into the `automation.UpResult` run receipt, and `opts.provider` threads the arm's build `Provider`.
- effect(`libs/typescript/.api/effect.md`): owns arm dispatch and the StackSpec/StackOutputs `Schema`.

[LOCAL_ADMISSION]:
- Arm dispatch constructs one arm-scoped build `Provider` threaded through `opts.provider`; a per-resource provider is rejected.
- Registry credentials and `secrets` bind Doppler `Output`s, never literals.

[RAIL_LAW]:
- Package: `@pulumi/docker-build`
- Owns: buildx-native image build+push, multi-platform builds, pluggable cache import/export, pluggable output exports, by-value build secrets, SSH forwarding, the multi-arch manifest-list `Index`
- Accept: `docker-build.Image` as the build owner, `digest`/`ref`-pinned downstream refs, parameterized cache/export backends, `platforms` multi-arch, Doppler-bound auth and by-value secrets, one arm-scoped build `Provider`
- Reject: the classic `docker.Image` build path where `docker-build.Image` is available, a `command`-shelled `docker build`, mutable-tag refs, literal registry credentials, per-resource providers
