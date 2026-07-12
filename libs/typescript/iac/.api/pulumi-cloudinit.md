# [TS_IAC_API_PULUMI_CLOUDINIT]

`@pulumi/cloudinit` renders multi-part MIME cloud-init user-data as a typed graph value — the pre-SSH complement to the `@pulumi/command` bootstrap row. The whole package is ONE part shape (`content`/`contentType`/`filename`/`mergeType`) carried by one render surface with a `Promise`/`Output` invoke mirror (`getConfig`/`getConfigOutput`) and a deprecated resource twin (`Config`); `rendered` is the product, `gzip`/`base64Encode`/`boundary` shape the wire encoding for the consuming host API. In `iac` this is the first-boot leg of the selfhosted arms: cloud-init owns what happens BEFORE `StackSpec.Connection` is reachable — package install, user/key layout, daemon enablement on a fresh VPS or metal image — and `remote.Command` owns everything after, so the `Bootstrap` tier's SSH surface is a cloud-init product, never a manual step.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/cloudinit`
- package: `@pulumi/cloudinit`
- license: Apache-2.0
- import: `@pulumi/cloudinit` → `{ Config, getConfig, getConfigOutput, Provider, types }`
- owner: `iac`
- rail: fabric / cluster-bootstrap
- runtime: Node deploy-host; the render is a pure provider invoke, no target reachability required
- depends-on: `@pulumi/pulumi` (`Input`/`Output`, `InvokeOptions`/`InvokeOutputOptions`)
- capability: multi-part MIME user-data composition (typed parts → one `rendered` string), gzip + base64 wire encoding, custom MIME `boundary`, graph-threaded or eager render reads
- abi-note: `GetConfigArgs` takes plain values for the eager `Promise` read; `GetConfigOutputArgs` mirrors every field `Input`-wrapped (`parts: Input<Input<GetConfigPartArgs>[]>`) so part content binds upstream `Output`s

## [02]-[CONFIG_RENDER]

[RENDER_SCOPE]: the invoke mirror pair — the one entry
- rail: cluster-bootstrap
- `getConfigOutput` is the canonical spelling: parts thread the graph as `Input`s, and `rendered` lands as an `Output<string>` a host-provisioning resource consumes as user-data. `getConfig` is the eager `Promise` mirror for an `async` program body — the same `run`/`runOutput` symmetry `@pulumi/command` carries. The `Config` resource is the provider-deprecated twin (identical fields, `rendered` as a resource output plus `static get`/`isInstance`); never author it new.

| [INDEX] | [SURFACE]                                                    | [SHAPE_BOUNDARY]                                                                                                           |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `getConfigOutput(GetConfigOutputArgs, InvokeOutputOptions?)` | `Output<GetConfigResult>`; graph-threaded render (all fields `Input`-wrapped)                                              |
|  [02]   | `getConfig(GetConfigArgs, InvokeOptions?)`                   | `Promise<GetConfigResult>`; eager render, plain-valued args                                                                |
|  [03]   | `GetConfigResult.rendered`                                   | `string` — the assembled user-data body; `.id` is its checksum                                                             |
|  [04]   | `gzip` / `base64Encode`                                      | `boolean` toggles; set explicitly per consuming boundary, never left ambient                                               |
|  [05]   | `boundary`                                                   | `string` — custom MIME part separator when the default collides with content                                               |
|  [06]   | `class Config`                                               | DEPRECATED resource twin (`ConfigArgs.parts: Input<Input<ConfigPart>[]>`, `rendered: Output<string>`); never author it new |
|  [07]   | `Provider`                                                   | empty-args `ProviderResource` marker; the render carries no credential                                                     |

[PART_SCOPE]: `GetConfigPartArgs` — the one part shape
- rail: cluster-bootstrap
- A part is one typed MIME segment; the `parts` array order is the cloud-init execution order. `contentType` selects the handler, `mergeType` is the cloud-init merge-strategy directive that lets multiple `text/cloud-config` parts compose instead of overwrite.

| [INDEX] | [FIELD]       | [TYPE]                     | [MEANING]                                                                                                   |
| :-----: | :------------ | :------------------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `content`     | `Input<string>` (required) | the segment body; binds rendered templates and upstream `Output`s                                           |
|  [02]   | `contentType` | `Input<string>`            | MIME handler selector — `text/cloud-config` for declarative YAML, `text/x-shellscript` for imperative steps |
|  [03]   | `filename`    | `Input<string>`            | part identity inside the archive; keys per-part instance state                                              |
|  [04]   | `mergeType`   | `Input<string>`            | merge-strategy directive combining multiple `text/cloud-config` parts                                       |

## [03]-[IMPLEMENTATION_LAW]

[BOOTSTRAP_TOPOLOGY]:
- split law: cloud-init owns first boot, `command` owns steady state — the rendered user-data lays the SSH surface (users, keys, packages, daemon) the `StackSpec.Connection` coordinates then reach, and the `Bootstrap` tier's `remote.Command` (`.api/pulumi-command.md`) takes over from there; a first-boot step re-run over SSH, or an SSH step folded into user-data, is the same defect in two directions.
- material law: user-data is metadata-endpoint-readable on every cloud, so `parts[].content` carries coordinates and installers only, never credential material — a first-boot part installs the Doppler CLI and the runtime pulls its material through the token channel the workload assembly owns (`.api/pulumiverse-doppler.md`); a PEM or password inside a part is the named defect.
- composition law: one `text/cloud-config` part for declarative host state plus ordered `text/x-shellscript` parts for what cloud-config cannot express; multiple cloud-config parts declare `mergeType` or the later part silently wins. Part content is rendered template data handed in like `Dispatch.Pins.install` — the lib hardcodes no script body.
- encoding law: `gzip`/`base64Encode` are boundary facts of the consuming host API — set them explicitly per target (encoded for size-limited user-data slots, plain when the body feeds a file-staging path), never left to provider defaults the boundary cannot see.
- re-render law: the render is pure — a changed part changes `rendered`, which replaces the consuming host resource through that resource's own diff; thread the spec `epoch` into templated content when a rebuild must be forced without a content change, mirroring the `triggers`/`keepers` rotation law.

[RAIL_LAW]:
- Package: `@pulumi/cloudinit`
- Owns: multi-part MIME user-data composition for raw VPS/metal first boot — the pre-SSH half of the bootstrap row
- Accept: `getConfigOutput` for graph-threaded renders, `getConfig` for eager reads, typed parts with explicit `contentType`, `mergeType` on composed cloud-config parts, explicit encoding per consuming boundary
- Reject: the deprecated `Config` resource in new work, credential material inside part content, hand-concatenated MIME bodies, first-boot logic duplicated into `remote.Command`, script bodies hardcoded in the lib instead of arriving as pins
