# [API_CATALOGUE] protoc-gen-capability-es

`protoc-gen-capability-es` is the local buf plugin (a `buf.gen.yaml` `local:` PATH binary, not a published library) that emits `src/gen/capabilities_pb.ts` from the C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` `DiscoveryResultWire[]` catalog descriptor. It derives the typed capability-command SDK surface — one polymorphic `CapabilityClient.invoke` keyed by descriptor id, the `discover` catalog accessor, and the per-descriptor `argumentSchema` JSON-Schema accessor binding the C# `JsonSchemaExporter` schema one-per-descriptor — plus, under `emit_mcp_client=true`, the MCP tool projection leg the `Transport/transport.md` `CapabilitySdkLive` fold reads. This catalogue is GATED: the plugin does not yet exist, so the member spellings and `.d.ts` shapes below are the OBLIGATION the runtime-action plugin must satisfy, captured verbatim from `Transport/transport.md` `[03]-[CODEGEN_TOOLING]` and confirmed against the emitted `.d.ts` the moment the plugin emits `capabilities_pb.ts` — never asserted from a design page as verified.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protoc-gen-capability-es`
- package: `protoc-gen-capability-es` (local buf plugin binary, unversioned, not a published npm package)
- module format: the emitted `src/gen/capabilities_pb.ts` is a `@bufbuild/protobuf`-runtime generated ESM module, consumed via `@bufbuild/protobuf` (`create`/`fromBinary`) + `@connectrpc/connect` `createClient`
- runtime target: build-time plugin (a `buf.gen.yaml` `local:` PATH binary); the EMITTED module is isomorphic generated TypeScript, one `_pb.ts` file per capability descriptor input
- asset: PATH binary run by `buf generate` as the SECOND plugin row on the one v2 pipeline beside `protoc-gen-es`; the emitted `.d.ts` is the sole verification surface for the member spellings below
- rail: codegen, capability-sdk
- status: GATED — the plugin and its emitted `capabilities_pb.ts` do not yet exist; every member below resolves against the emitted `.d.ts` post-authoring, never as a verified binding

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generated message + client surface (obligation — resolved against the emitted `.d.ts` post-authoring)
- rail: capability-sdk

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                                                                                                                                                  |
| :-----: | :----------------------------- | :---------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `CommandService`               | `DescService`     | the `CommandService` wire verb descriptor the SDK dials over                                                                                            |
|  [02]   | `CapabilityClient`             | generated client  | one `createClient(CommandService, transport)` row; never hand-written                                                                                   |
|  [03]   | `DiscoveryResultWire`          | generated message | the catalog row shape (`descriptor`/`surface`/`effect`/`idempotency`/`estimated`/`scopeHash`) — mirrors `transport.md` `[3]-[CODEGEN_TOOLING]` verbatim |
|  [04]   | `CapabilityCommandReceiptWire` | generated message | the command receipt (`descriptor`/`txn`/`charged`/`elapsed`/`correlation`)                                                                              |
|  [05]   | `CostVectorWire`               | generated message | the `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls` cost row                                                                           |

[PUBLIC_TYPE_SCOPE]: MCP projection leg (emitted under `emit_mcp_client=true`)
- rail: capability-sdk

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                                                                                                          |
| :-----: | :-------------- | :---------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `McpToolWire`   | generated message | the MCP tool descriptor (`name`/`description`/`effect`/`idempotency`/`inputSchema`) the host advertises        |
|  [02]   | `EffectClassKey`| literal union     | `"pure" \| "read" \| "write" \| "external" \| "irreversible"` — verbatim from the C# `EffectClass` smart-enum  |
|  [03]   | `IdempotencyKey`| literal union     | `"idempotent" \| "keyed" \| "single-shot" \| "non-idempotent"` — verbatim from the C# `Idempotency` smart-enum |
|  [04]   | `CostUnitKey`   | literal union     | `"cpu-millis" \| "wall-millis" \| "bytes-egress" \| "model-tokens" \| "calls"` — the `CostVectorWire` key set   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated `CapabilityClient` members (obligation — three spellings resolved against the emitted `.d.ts`)
- rail: capability-sdk

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]   | [RAIL]                                                                                                                                                         |
| :-----: | :---------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `client.discover()`                 | catalog accessor | `Promise<DiscoveryResultWire[]>` — the full descriptor catalog                                                                                                 |
|  [02]   | `client.invoke(descriptor, args)`   | command dispatch | `Promise<CapabilityCommandReceiptWire>` — ONE polymorphic method keyed by descriptor id, never a sibling method per descriptor                                 |
|  [03]   | `client.argumentSchema(descriptor)` | schema accessor  | the per-descriptor JSON Schema (the C# `JsonSchemaExporter` output, identical digest across all three SDK targets) — never a hand-built `{type:"object"}` stub |

[ENTRYPOINT_SCOPE]: build-time generation
- rail: codegen

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                                                                                |
| :-----: | :-------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `local: protoc-gen-capability-es` | buf plugin     | the SECOND `buf.gen.yaml` plugin row (drafted `transport.md` line 147) on the SAME single v2 pipeline |
|  [02]   | `opt: target=ts`                  | plugin option  | TypeScript output target                                                                              |
|  [03]   | `opt: emit_mcp_client=true`       | plugin option  | emit the MCP tool projection leg (`McpToolWire`) beside the command surface                           |

## [04]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- the plugin is a `local:` buf plugin per `.api/bufbuild-buf.md` (`plugins[].local` is a PATH binary name or path); it runs as the second plugin row on the one `buf.gen.yaml` v2 pipeline beside `protoc-gen-es`, never a parallel config
- input is the C#-emitted `DiscoveryResultWire[]` catalog descriptor published beside the discovery manifest (`csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN`); the plugin re-authors no capability shape, reading the descriptor verbatim
- output is `src/gen/capabilities_pb.ts` — a typed-per-descriptor effect-classed command surface plus, under `emit_mcp_client=true`, one MCP client leg, both derived from the descriptor, never hand-shaped

[STACKS_WITH]:
- `@bufbuild/buf` (`.api/bufbuild-buf.md`): `buf generate` is the build-time driver; this plugin is the second `plugins[].local` row on the single `version: v2` `buf.gen.yaml`, sharing `out: src/gen` with `protoc-gen-es` — never a parallel `buf.gen.yaml`, never a shell-encoded plugin option
- `@bufbuild/protoc-gen-es` (`.api/bufbuild-protoc-gen-es.md`): the FIRST plugin on the same pipeline emits the message-and-service `_pb.ts`; capability-es runs beside it so `CommandService` and the wire messages resolve from the same generated tree, the `@connectrpc/protoc-gen-connect-es` split being the rejected form
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): the generated `DiscoveryResultWire`/`CapabilityCommandReceiptWire`/`CostVectorWire` messages extend `GenMessage`/`DescMessage`, `create`/`fromBinary` construct them, and `CommandService` is the `DescService` `createClient` keys on
- `@connectrpc/connect` (`.api/connectrpc-connect.md`): `CapabilityClient` is ONE `createClient(CommandService, transport)` over the SAME shared `WireTransportLive` transport — never a second transport, never a hand-written client
- `effect` (`.api/effect.md`): the `CapabilitySdkLive` fold decodes `client.discover()`/`client.invoke()` replies through `Schema.decodeUnknown(DiscoveryResultWire)`/`(CapabilityCommandReceiptWire)` and maps the `ParseError` through `Ingress/fault.md` `faultDetailRail.fromConnect`; `mcpTools.inputSchema` reads `client.argumentSchema(descriptor)`, the descriptor's real generated argument contract; the `EffectClassKey`/`IdempotencyKey`/`CostUnitKey` literal vocabularies and the `txn` union literals are transcribed VERBATIM from the C# enum source under the `Contract/inventory#WIRE_LAW` cross-language drift law, a new C# value folding an unknown to `Ingress/quarantine.md` `Additive`, never a silent branch-side drift

[LOCAL_ADMISSION]:
- `CapabilityClient` is one `createClient(CommandService, transport)` over the SAME shared `WireTransportLive` transport — never a second transport, never a hand-written client.
- the three member spellings (`discover`, `invoke`, `argumentSchema`) are the OBLIGATION; the `transport.md` `CapabilitySdkLive` fold resolves them against the emitted `.d.ts` before transcription, never asserting an unverified generated member from a design page.
- `catalog`/`invoke` decode the generated reply through `Schema.decodeUnknown(DiscoveryResultWire)`/`(CapabilityCommandReceiptWire)` mapping the `ParseError` through `faultDetailRail.fromConnect`; `mcpTools.inputSchema` reads `argumentSchema(descriptor)`, never a hand-built `{type:"object"}` stub.
- `emit_mcp_client=true` is the only source of the `McpToolWire` leg; a hand-maintained MCP tool list beside the generated one is the rejected form.

[RAIL_LAW]:
- Package: `protoc-gen-capability-es` (local buf plugin)
- Owns: TypeScript capability-SDK code generation from the C# `DiscoveryResultWire[]` catalog descriptor, plus the `emit_mcp_client=true` MCP tool projection
- Accept: the published C# catalog descriptor; `buf.gen.yaml` `local:` admission; `target=ts`/`emit_mcp_client=true` options
- Reject: a hand-written `CapabilityClient`, a sibling `invoke` method per descriptor, a hand-built `inputSchema` stub, a second `buf.gen.yaml`, a member-spelling asserted from a design page rather than the emitted `.d.ts`
