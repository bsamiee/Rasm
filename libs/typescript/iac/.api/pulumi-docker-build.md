# [TS_IAC_API_PULUMI_DOCKER_BUILD]

`@pulumi/docker-build` is the buildx-native image builder — the CANONICAL modern build owner for the `selfhosted-docker` arm and (bundled internally) the `aws` arm's `awsx.ecr.Image`. Its `Image` resource is a strict superset of the classic `@pulumi/docker.Image` / `awsx.ecr.Image` build paths: multi-platform builds (`platforms`), a pluggable cache-backend family (`cacheFrom`/`cacheTo` over registry/local/s3/gha/azblob), a pluggable export-backend family (`exports` over docker/image/local/oci/registry/tar), by-value build `secrets` (no on-disk/env requirement), `ssh` forwarding, and an `Index` manifest-list resource that fuses per-platform tags into one multi-arch ref. Three resources share ONE Pulumi ABI (`new X(name, XArgs, opts?)` + `static get`/`isInstance`): `Image` (build+push), `Index` (multi-platform manifest list), and a `Provider` carrying the BuildKit daemon address. The pushed `ref` (`tag@digest`) / `digest` (sha256) outputs are the immutable values downstream workloads pin.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/docker-build`
- package: `@pulumi/docker-build`
- license: Apache-2.0
- import: `@pulumi/docker-build` → `{ Image, Index, Provider, config, types }` + top-level enums (`Platform`/`NetworkMode`/`CacheMode`)
- owner: `iac`
- rail: fabric / selfhosted-docker (canonical image build) + aws (bundled by `awsx.ecr.Image`)
- runtime: a reachable BuildKit backend — the provider embeds a catalog-bound Docker client + catalog-bound buildx by default (`exec: true` shells a host `docker-buildx` instead); a push needs registry credentials
- build-floor: `@pulumi/pulumi` `^catalog`
- depends-on: `@pulumi/pulumi` (`CustomResource`/`ProviderResource`/`Input`/`Output`, `.api/pulumi-pulumi.md`)
- namespaces: root barrel (`Image`/`Index`/`Provider` + enums), `docker-build.config` (ambient `host`/`registries`), `docker-build.types.input`/`…output` (the cache/export/context/dockerfile/registry/ssh arg trees)
- capability: buildx-native multi-platform image build+push, pluggable cache import/export, pluggable output exports, by-value build secrets, SSH forwarding, multi-arch manifest-list `Index`
- abi-note: `push` is REQUIRED on `ImageArgs` (no implicit push, unlike classic `docker.Image`); every output prop is an `Output<T>` mirror of its arg, plus the computed `ref`/`digest`/`contextHash`

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized resource shape all three classes instantiate
- rail: selfhosted-docker
- One shape owns `Image`/`Index`/`Provider`; construction is `new X(name, XArgs, opts?)` where `opts` is the universal `pulumi.CustomResourceOptions`/`ResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`, `.api/pulumi-pulumi.md`). Adoption is `static get(name, id, opts?)`; every output prop is an `Output<T>` mirror. `Provider` extends `pulumi.ProviderResource`; `Image`/`Index` extend `pulumi.CustomResource`.

| [INDEX] | [MEMBER] | [SHAPE_BOUNDARY] |
|:-----: |:------- |:----------------- |
| [01] | `new X(name, XArgs, opts?)` | construct; `XArgs` fields are `Input<T>` |
| [02] | `X.get(name, id, opts?)` | adopt an existing image/index by id |
| [03] | `X.isInstance(obj)` | multi-SDK-safe guard `obj is X` |
| [04] | `x.<prop>: Output<T>` | resolved output mirror; thread via `.apply`/`pulumi.all` |

## [03]-[BUILD_SURFACE]

[IMAGE_SCOPE]: `Image` — the buildx build+push resource
- rail: selfhosted-docker
- `Image` builds a Dockerfile through buildx and (with `push: true` or a `registry` export) pushes it, emitting `ref` (a `tag@digest` convenience) and `digest` (the stable sha256 downstream resources pin). `dockerfile`/`context` locate the build; `platforms` drives multi-arch; `secrets`/`ssh` inject build-time material by value; `cacheFrom`/`cacheTo`/`exports` are the pluggable backend families.

| [INDEX] | [SYMBOL] | [KEY_ARGS_OUTPUTS] |
|:-----: |:------- |:------------------- |
| [01] | `Image` | `{ push (required), tags, dockerfile, context, buildArgs, target, platforms, secrets, ssh, labels, network, noCache, pull, load, builder, buildOnPreview, exec }` → `ref`, `digest`, `contextHash` |
| [02] | `types.input.DockerfileArgs` | `location` (path) XOR `inline` (literal Dockerfile body) |
| [03] | `types.input.BuildContextArgs` | `location` (path/URL/git) + `named` (named build contexts) |
| [04] | `types.input.RegistryArgs` | `address`/`username`/`password` (bind a `doppler` secret Output) |

[BACKEND_SCOPE]: the parameterized cache + export backend families
- rail: selfhosted-docker
- `cacheFrom`/`cacheTo`/`exports` are each ONE parameterized slot whose backend is selected by which sub-key is set — a new backend is a row, never a new arg. This IS the mechanism; the backend keys are seed data.

| [INDEX] | [SLOT] | [BACKEND_KEYS_ONE_PER_ENTRY] |
|:-----: |:----- |:----------------------------- |
| [01] | `cacheFrom: CacheFromArgs[]` | `registry` \| `local` \| `s3` \| `gha` \| `azblob` \| `raw` |
| [02] | `cacheTo: CacheToArgs[]` | `registry` \| `local` \| `s3` \| `gha` \| `azblob` \| `inline` \| `raw` |
| [03] | `exports: ExportArgs[]` | `registry` \| `image` \| `docker` \| `oci` \| `local` \| `tar` \| `cacheonly` \| `raw` |
| [04] | `platforms: Platform[]` | the multi-arch build targets (`Platform` enum constants) |

[INDEX_SCOPE]: `Index` — multi-platform manifest list
- rail: selfhosted-docker
- `Index` fuses per-platform image tags (`sources`) into ONE multi-arch manifest under `tag`, emitting the combined `ref` — the manifest-list complement to a single per-`platforms` `Image`.

| [INDEX] | [SYMBOL] | [KEY_ARGS_OUTPUTS] |
|:-----: |:------- |:------------------- |
| [01] | `Index` | `{ sources (required tag refs), tag (required), push, registry }` → `ref` |

## [04]-[PROVIDER]

[PROVIDER_SCOPE]: the build-daemon connection
- rail: selfhosted-docker
- The dispatch arm constructs ONE `Provider` and threads it via `opts.provider`. `host` selects the BuildKit endpoint (default: the embedded client); `registries` pre-authenticates pushes. `config.*` mirrors these as ambient vars.

| [INDEX] | [FIELD] | [TYPE] | [MEANING] |
|:-----: |:------ |:----- |:-------- |
| [01] | `host` | `Input<string>` | build-daemon address (default embedded catalog-bound-client / buildx-0.12) |
| [02] | `registries` | `Input<RegistryArgs[]>` | pre-authenticated push registries |

## [05]-[IMPLEMENTATION_LAW]

[BUILD_TOPOLOGY]:
- canonical-owner law: `docker-build.Image` is the modern buildx-native build owner across the branch — it SUPERSEDES the classic `@pulumi/docker.Image` (`.api/pulumi-docker.md`) on the `selfhosted-docker` arm and is the builder `awsx.ecr.Image` (`.api/pulumi-awsx.md`) bundles internally on the `aws` arm. Author `docker-build.Image` for a new image; never a `command`-shelled `docker build`.
- digest law: a workload pins to the immutable `digest` (sha256) or `ref` (`tag@digest`), never a mutable tag; `push` is required-explicit (no implicit push), so a build without an export / `push:true` stays local by design.
- backend law: `cacheFrom`/`cacheTo`/`exports` are parameterized backend slots — select a backend by its sub-key (`registry`/`s3`/`gha`/…); a build reuses a registry/s3/gha cache across CI runs rather than rebuilding layers.
- multi-arch law: a single `Image` with `platforms: [...]` builds multi-arch; `Index` fuses independently-built per-platform tags into one manifest.

[LOCAL_ADMISSION]:
- `registries[].password` + `secrets` bind `@pulumiverse/doppler` secret Outputs (`.api/pulumiverse-doppler.md`), never literals; secrets pass by value (no on-disk/env requirement).
- `Image.ref`/`digest` cross arms: the same built digest feeds a `@pulumi/kubernetes` workload image ref (`.api/pulumi-kubernetes.md`) or an ECS TaskDefinition container image when a stack mixes a build with a runtime arm.
- the arm folds build failures into the `program/automation` typed run receipt (`@pulumi/pulumi` `automation.UpResult`, `.api/pulumi-pulumi.md`); `effect` owns arm dispatch and the StackSpec/StackOutputs `Schema`; `opts.provider` threads the arm's build `Provider`.
- canonical-spelling law: `docker-build.Image` over classic `docker.Image`; `digest`/`ref` (immutable) over a tag; one shared build `Provider` per arm.

[RAIL_LAW]:
- Package: `@pulumi/docker-build`
- Owns: the canonical buildx-native image build+push, multi-platform builds, pluggable cache import/export, pluggable output exports, by-value build secrets, SSH forwarding, multi-arch manifest-list `Index`
- Accept: `docker-build.Image` as the modern build owner, `digest`/`ref`-pinned downstream refs, parameterized cache/export backends, `platforms` multi-arch, Doppler-bound registry auth + by-value secrets, one arm-scoped build `Provider`
- Reject: the classic `docker.Image` build path where `docker-build.Image` is available, a `command`-shelled `docker build`, mutable-tag refs, literal registry credentials, per-resource providers
