# [API_CATALOGUE] protoc-gen-capability-es

`protoc-gen-capability-es` is the local buf plugin (a `buf.gen.yaml` `local:` PATH binary, not a published library) that emits `src/gen/capabilities_pb.ts` from the C# `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` `DiscoveryResultWire[]` catalog descriptor. It derives the typed capability-command SDK surface — one polymorphic `CapabilityClient.invoke` keyed by descriptor id, the `discover` catalog accessor, and the per-descriptor `argumentSchema` JSON-Schema accessor binding the C# `JsonSchemaExporter` schema one-per-descriptor — plus the MCP tool projection leg the `transport/transport.md` `CapabilitySdkLive` fold reads. This catalogue is GATED: the plugin does not yet exist, so the generated member spellings and `.d.ts` shapes below are the OBLIGATION the runtime-action plugin must satisfy, captured here verbatim from the emitted `.d.ts` the moment the plugin emits `capabilities_pb.ts` — never asserted from a design page.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protoc-gen-capability-es`
- package: `protoc-gen-capability-es` (local buf plugin binary, not a published npm package)
- module: emitted `src/gen/capabilities_pb.ts` (`@bufbuild/protobuf`-runtime generated; consumed via `@bufbuild/protobuf` + `@connectrpc/connect` `createClient`)
- asset: `buf.gen.yaml` `local:` plugin binary on PATH; one generated `_pb.ts` file per capability descriptor input
- rail: codegen, capability-sdk
- status: GATED — the plugin and its emitted `capabilities_pb.ts` do not yet exist; member spellings resolve against the emitted `.d.ts` post-authoring

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generated SDK surface (obligation — confirmed against the emitted `.d.ts`)
- rail: capability-sdk

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                                                   |
| :-----: | :----------------------------- | :----------------- | :---------------------------------------------------------------------- |
|   [1]   | `CommandService`               | `DescService`      | the `CommandService` wire verb descriptor the SDK dials over            |
|   [2]   | `CapabilityClient`             | generated client   | one `createClient(CommandService, transport)` row; never hand-written   |
|   [3]   | `DiscoveryResultWire`          | generated message  | the catalog row shape (`descriptor`/`surface`/`effect`/`idempotency`/`estimated`/`scopeHash`) — mirrors `transport.md` `[3]-[CODEGEN_TOOLING]` verbatim |
|   [4]   | `CapabilityCommandReceiptWire` | generated message  | the command receipt (`descriptor`/`txn`/`charged`/`elapsed`/`correlation`) |
|   [5]   | `CostVectorWire`               | generated message  | the `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls` cost row |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated `CapabilityClient` members (obligation — three confirmed spellings)
- rail: capability-sdk

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]   | [RAIL]                                                                |
| :-----: | :--------------------------------- | :--------------- | :------------------------------------------------------------------- |
|   [1]   | `client.discover()`                | catalog accessor | `Promise<DiscoveryResultWire[]>` — the full descriptor catalog       |
|   [2]   | `client.invoke(descriptor, args)`  | command dispatch | `Promise<CapabilityCommandReceiptWire>` — ONE polymorphic method keyed by descriptor id, never a sibling method per descriptor |
|   [3]   | `client.argumentSchema(descriptor)` | schema accessor  | the per-descriptor JSON Schema (the C# `JsonSchemaExporter` output, identical digest across all three SDK targets) — never a hand-built `{type:"object"}` stub |

[ENTRYPOINT_SCOPE]: build-time generation
- rail: codegen

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------------- |
|   [1]   | `local: protoc-gen-capability-es`      | buf plugin     | the SECOND `buf.gen.yaml` plugin row (drafted `transport.md` line 147) on the SAME single v2 pipeline |
|   [2]   | `opt: target=ts`                       | plugin option  | TypeScript output target                                        |
|   [3]   | `opt: emit_mcp_client=true`            | plugin option  | emit the MCP tool projection leg beside the command surface     |

## [4]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- the plugin is a `local:` buf plugin per `.api/bufbuild-buf.md` (`plugins[].local` is a PATH binary name or path); it runs as the second plugin row on the one `buf.gen.yaml` v2 pipeline beside `protoc-gen-es`, never a parallel config
- input is the C#-emitted `DiscoveryResultWire[]` catalog descriptor published beside the discovery manifest (`csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN`); the plugin re-authors no capability shape, reading the descriptor verbatim
- output is `src/gen/capabilities_pb.ts` — a typed-per-descriptor effect-classed command surface plus one MCP client leg, both derived from the descriptor, never hand-shaped

[LOCAL_ADMISSION]:
- `CapabilityClient` is one `createClient(CommandService, transport)` over the SAME shared `WireTransportLive` transport — never a second transport, never a hand-written client
- the three member spellings (`discover`, `invoke`, `argumentSchema`) are the OBLIGATION; the `transport.md` `CapabilitySdkLive` fold resolves them against the emitted `.d.ts` before transcription, never asserting an unverified generated member from a design page
- `catalog`/`invoke` decode the generated reply through `Schema.decodeUnknown(DiscoveryResultWire)`/`(CapabilityCommandReceiptWire)` mapping the `ParseError` through `faultDetailRail.fromConnect`; `mcpTools.inputSchema` reads `argumentSchema(descriptor)`, the descriptor's real generated argument contract
- the `EffectClassKey`/`IdempotencyKey`/`CostUnitKey` literal vocabularies and the `txn` union literals are transcribed VERBATIM from the C# enum source under the `contracts/wire-inventory#WIRE_LAW` cross-language drift law — a new C# value folds an unknown to `quarantine/drift-terminal.md` `Additive`, never a silent branch-side drift

[RAIL_LAW]:
- Package: `protoc-gen-capability-es` (local buf plugin)
- Owns: TypeScript capability-SDK code generation from the C# `DiscoveryResultWire[]` catalog descriptor
- Accept: the published C# catalog descriptor; `buf.gen.yaml` `local:` admission; `target=ts`/`emit_mcp_client=true` options
- Reject: a hand-written `CapabilityClient`, a sibling `invoke` method per descriptor, a hand-built `inputSchema` stub, a second `buf.gen.yaml`, a member-spelling asserted from a design page rather than the emitted `.d.ts`
