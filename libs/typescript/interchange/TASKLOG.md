# [INTERCHANGE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and bullets naming the capability, shape, unlock, anchors, and optional tension. One idea spawns one or more tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[HLC_TWO_HALF_PARITY]-[BLOCKED]: assert the C# two-half HLC stamp only after the frozen corpus row exists.
- Capability: `Codec/parity.md` carries the settled `HlcTwoHalfParity` algebra: `compose` over the two 32-bit halves, the `DataView` `setBigUint64`/`getBigUint64(_, false)` big-endian round-trip, and the `@msgpack/msgpack` `Decoder({ useBigInt64: true })` bigint mapping the `Codec/codec.md` `decodeCrdtOp` and `decodeSnapshotHeader` paths already read.
- Shape: one `HLC_REFERENCE` row plugs into `assertsReference` beside `CANONICAL_REFERENCE`; the codec bigint reads stay in `Codec/codec.md`, the parity assertion stays in `Codec/parity.md`, and no TypeScript branch owner re-mints an HLC encoding.
- Unlocks: the `projection` conflict-presence fold can trust the C#-owned half order before it treats a fresh op as stale, so a logical-half swap fails at the corpus equality boundary instead of surfacing as silent convergence drift.
- Anchors: `csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS` row [6] `HLC_TWO_HALF`, `csharp:Rasm.AppHost/Runtime/ports#HLC_FANIN`, `Codec/parity.md` `HlcTwoHalfParity`, `Codec/codec.md` `decodeCrdtOp` and `decodeSnapshotHeader`, and `@msgpack/msgpack` `useBigInt64`.
- Tension: `HLC_TWO_HALF` remains DESIGN-PIN because `Runtime/ports#HLC_FANIN` does not yet carry the producer-frozen two-half bigint fixture; the existing `HlcStampWire` is the receipt-envelope `logical: number` stamp, not the op-log two-half reference, and no fabricated stamp stands in. [UPSTREAM-BLOCKED on csharp HLC_FANIN#HLC_TWO_HALF fixture pin]

[CAPABILITY_SDK_CODEGEN]-[BLOCKED]: admit the local capability SDK plugin only after it emits confirmable TypeScript bindings.
- Capability: the second `buf.gen.yaml` plugin row derives the typed effect-classed command surface and MCP tool projection from the C# `DiscoveryResultWire[]` capability catalog; `Transport/transport.md` already fixes the `CapabilitySdk` service contract and the blocked `CapabilitySdkLive` fold shape.
- Shape: `protoc-gen-capability-es` emits `src/gen/capabilities_pb.ts` on the same `buf` v2 pipeline as `@bufbuild/protoc-gen-es`, with one `CapabilityClient` over the shared `WireTransportLive`, `discover()`, `invoke(descriptor, args)`, and `argumentSchema(descriptor)` members resolved from the emitted `.d.ts`.
- Unlocks: TypeScript gains the generated descriptor catalog, one polymorphic invoke-by-descriptor command face, and the MCP tool `inputSchema` projection without hand-written capability methods, hand-built JSON Schema stubs, or a second transport.
- Anchors: `@bufbuild/buf`, `@bufbuild/protoc-gen-es`, local `protoc-gen-capability-es`, `Transport/transport.md` `CODEGEN_TOOLING`, `.api/protoc-gen-capability-es.md`, and `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` / `#TS_PROJECTION`.
- Tension: the C# descriptor and wire shapes are settled, but the local plugin and emitted `capabilities_pb.ts` do not yet exist; no `CapabilitySdkLive` fold calls `CapabilityClient.discover`, `invoke`, or `argumentSchema` until the generated module confirms those members. [UPSTREAM-BLOCKED on the absent protoc-gen-capability-es generated capabilities_pb.ts]

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
