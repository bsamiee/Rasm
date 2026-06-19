# [INTERCHANGE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and bullets naming the capability or file to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks.

## [01]-[OPEN]

[BLOCKED] HLC_TWO_HALF_PARITY — assert the bigint two-half round-trip against the frozen stamp.
- The `Codec/parity.md` `HlcTwoHalfParity` round-trip algebra is fenced — `compose` over the two 32-bit halves and the `DataView` `setBigUint64`/`getBigUint64(_, false)` big-endian round-trip plus the `@msgpack/msgpack` `useBigInt64` bigint mapping the `Codec/codec.md` `decodeCrdtOp` and `decodeSnapshotHeader` carry — but the cross-runtime byte-equality assertion needs the FROZEN two-half HLC reference stamp.
- Integrate the verified `@msgpack/msgpack` `Decoder({ useBigInt64: true })` and the `DataView` big-endian read; the algebra is settled, only the producer-frozen stamp gates the equality.
- Asserts the C#-owned two-64-bit-half order the `projection` conflict-presence fold reads — an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal, so the half-order assertion is load-bearing.
- Blocked on the absent `csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [6] `HLC_TWO_HALF`, itself DESIGN-PIN on `csharp:Rasm.AppHost/Runtime/ports#HLC_FANIN` — a producer cluster the runtime-ports page does not yet carry (its `HlcStampWire` is the receipt-envelope `logical: number` stamp, not the two-half bigint fixture). No fabricated stamp stands in. [UPSTREAM-BLOCKED on csharp HLC_FANIN#HLC_TWO_HALF fixture pin]

[BLOCKED] CAPABILITY_SDK_CODEGEN — admit the second codegen plugin.
- Build the second `buf.gen.yaml` plugin row and the `CapabilitySdkLive` `Effect.Service` fold in `Transport/transport.md` `CODEGEN_TOOLING` deriving the typed effect-classed command surface and the MCP tool projection from the generated `capabilities_pb.ts`; the `CapabilitySdk` SERVICE CONTRACT is already fixed on that page, the `*Live` fold lands here against the confirmed generated `CapabilityClient.discover`/`invoke`/argument-schema members.
- Integrate `@bufbuild/buf`, `@bufbuild/protoc-gen-es`, and the capability-descriptor codegen plugin.
- The upstream C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` producer is SETTLED — the `SdkTarget.Typescript` renderer emits `{Method}(args): Promise<ReceiptEnvelopeWire<CapabilityCommandReceiptWire>> { return this.run(...) }`, the `DiscoveryResultWire`/`CapabilityCommandReceiptWire` descriptor shapes are fixed at `#TS_PROJECTION` and transcribed verbatim on `transport.md`, and the `invoke`-by-descriptor / per-descriptor `JsonSchemaExporter` `inputSchema` law is producer-fixed. The remaining gate is NOT the descriptor shape but the emitted `capabilities_pb.ts` module: `protoc-gen-capability-es` is a local buf plugin that does not yet exist, so `CapabilityClient.discover`/`invoke`/`argumentSchema` are not `.d.ts`-confirmable and no `*Live` fold calls unverified generated members.
- Blocked on the absent `protoc-gen-capability-es` plugin emitting `capabilities_pb.ts`; `invoke` is one polymorphic method keyed by descriptor id, never a sibling method per descriptor; the `inputSchema` is the generated per-descriptor JSON Schema, never a hand-built stub. [UPSTREAM-BLOCKED on the absent protoc-gen-capability-es generated capabilities_pb.ts]

## [02]-[CLOSED]

(none)
