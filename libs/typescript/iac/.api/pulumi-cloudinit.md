# [TS_IAC_API_PULUMI_CLOUDINIT]

`@pulumi/cloudinit` renders multi-part MIME cloud-init user-data as a typed graph value — the pre-SSH first-boot leg of the bootstrap rail. One part shape (`content`/`contentType`/`filename`/`mergeType`) feeds one render surface with a `Promise`/`Output` invoke mirror (`getConfig`/`getConfigOutput`); `rendered` is the product and `gzip`/`base64Encode`/`boundary` shape the wire encoding the consuming host API reads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/cloudinit`
- package: `@pulumi/cloudinit` (Apache-2.0)
- module: `@pulumi/cloudinit` → `{ Config, getConfig, getConfigOutput, Provider, types }`
- runtime: Node deploy-host; the render is a pure provider invoke, no target reachability
- rail: fabric / cluster-bootstrap
- depends-on: `@pulumi/pulumi` (`Input`/`Output`, `InvokeOptions`/`InvokeOutputOptions`)
- abi-note: `GetConfigOutputArgs` double-wraps parts — `parts: Input<Input<GetConfigPartArgs>[]>` — so a part's content binds an upstream `Output`; `GetConfigArgs` takes plain values for the eager `Promise` read

## [02]-[CONFIG_RENDER]

[RENDER_SCOPE]: the invoke mirror pair
- `getConfigOutput` is canonical — parts thread the graph as `Input`s and `rendered` lands as an `Output<string>` a host-provisioning resource consumes as user-data; `getConfig` is the eager `Promise` mirror for an `async` program body. `Config` is the provider-deprecated resource twin, never authored new.

| [INDEX] | [SURFACE]                                                    | [SHAPE_BOUNDARY]                                                        |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `getConfigOutput(GetConfigOutputArgs, InvokeOutputOptions?)` | `Output<GetConfigResult>`; graph-threaded, fields `Input`-wrapped       |
|  [02]   | `getConfig(GetConfigArgs, InvokeOptions?)`                   | `Promise<GetConfigResult>`; eager render, plain-valued args             |
|  [03]   | `GetConfigResult.rendered`                                   | `string` — assembled user-data body; `.id` is its CRC-32 checksum       |
|  [04]   | `gzip` / `base64Encode`                                      | `boolean`, default `true`; `base64Encode` can't be `false` under `gzip` |
|  [05]   | `boundary`                                                   | `string` — custom MIME separator when the default collides              |
|  [06]   | `class Config`                                               | deprecated twin; `ConfigPart` parts, `rendered: Output<string>`         |
|  [07]   | `Provider`                                                   | empty-args `ProviderResource` marker; the render carries no credential  |

[PART_SCOPE]: `GetConfigPartArgs` — the one part shape
- `parts` order is cloud-init execution order; `contentType` selects the handler (`text/cloud-config` declarative YAML, `text/x-shellscript` imperative) and `mergeType` sets the `X-Merge-Type` directive so composed cloud-config parts merge instead of overwrite.

| [INDEX] | [FIELD]       | [TYPE]                     | [MEANING]                                                             |
| :-----: | :------------ | :------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `content`     | `Input<string>` (required) | segment body; binds rendered templates and upstream `Output`s         |
|  [02]   | `contentType` | `Input<string>`            | handler MIME type; defaults `text/plain`                              |
|  [03]   | `filename`    | `Input<string>`            | part identity inside the archive; keys per-part instance state        |
|  [04]   | `mergeType`   | `Input<string>`            | `X-Merge-Type` directive combining multiple `text/cloud-config` parts |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- boot-split: cloud-init owns first boot — package install, user/key layout, daemon enablement on a fresh VPS or metal image; a first-boot step re-run over SSH, or an SSH step folded into user-data, is the same defect in two directions.
- material: user-data is metadata-endpoint-readable on every cloud, so `parts[].content` carries coordinates and installers only — a PEM or password inside a part is the named defect.
- composition: one `text/cloud-config` part carries declarative host state, ordered `text/x-shellscript` parts carry what cloud-config cannot express; multiple cloud-config parts set `mergeType` or the later part silently wins.
- encoding: `gzip`/`base64Encode` are boundary facts of the consuming host API, set explicitly per target — encoded for size-limited user-data slots, plain when the body feeds a file-staging path.
- render-purity: a changed part changes `rendered`, which replaces the consuming host resource through that resource's own diff; thread the spec `epoch` into templated content to force a rebuild without a content change.

[STACKING]:
- `@pulumi/command`(`.api/pulumi-command.md`): cloud-init `rendered` lays the SSH surface during first boot, then `remote.Command` over the shared `StackSpec.Connection` owns steady state — sequential complements across one boot boundary, and the `epoch`-forced re-render mirrors the `triggers`/`keepers` rotation.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): a first-boot part installs the Doppler CLI, and the runtime pulls its material through the `ServiceToken.key` (`DOPPLER_TOKEN`) env channel, keeping every secret out of `parts[].content`.
- within-lib: part content arrives as rendered template data like `Dispatch.Pins.install`; the lib hardcodes no script body.

[RAIL_LAW]:
- Package: `@pulumi/cloudinit`
- Owns: multi-part MIME user-data composition for raw VPS/metal first boot — the pre-SSH half of the bootstrap row
- Accept: `getConfigOutput` for graph-threaded renders, `getConfig` for eager reads, typed parts with explicit `contentType`, `mergeType` on composed cloud-config parts, explicit encoding per consuming boundary
- Reject: the deprecated `Config` resource in new work, credential material inside part content, hand-concatenated MIME bodies, first-boot logic duplicated into `remote.Command`, script bodies hardcoded in the lib instead of arriving as pins
