# [INTERCHANGE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and bullets naming the capability, shape, unlock, anchors, and optional tension. One idea spawns one or more tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
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

[INTERCHANGE_WIRE_DECODE_PEERS]-[QUEUED]: The TypeScript interchange package and the Python companion DECODE the C#-minted MaterialWire (OpenPBR vector, now through the Thinktecture generated codec) and MtlxDocument and never re-mint the OpenPBR algebra, the ConductorIor table, or the MaterialX schema — a peer re-mint is the named cross-language drift defect the interchange page declares.
- Capability: The host-free peer decoders mirroring the MaterialWire/OpenPbrGroupsWire/WireProvenance/WireColor record shape and the MtlxDocument node-graph field-for-field as pure structural decoders reading the Thinktecture-generated-codec wire shape (the System.Text.Json byte format outside-Rhino, and the MessagePack companion format where bound), the scene-linear RgbLinear triple, and the Hex preview without re-deriving the ACEScg working space or the OpenPBR lowering.
- Shape: A typescript:interchange codec module and a python:data/runtime dataclass decoding the C# MaterialWire and MtlxDocument the MaterialProjection.Mint and Mtlx.FromGraph/ToOpenPbr produce through the Thinktecture codec, the consumer end of the Appearance/interchange [WIRE] seam.
- Unlocks: The web SPA renders a material swatch from the wire Hex/linear triple and the DCC ecosystem reads the .mtlx, both decode-only so the OpenPBR algebra lives once in C# and the cross-language single-mint invariant holds against the generated-codec canonical shape.
- Anchors: interchange.md C# sole producer, peers decode-only, re-mint is the named drift defect, the realized TS interface and Python dataclass decode contracts; ARCHITECTURE.md [01]-[DOMAIN_MAP] C# mints once, peers decode (line 39); the THINKTECTURE_WIRE_CODEC task this decode tracks (the generated System.Text.Json/MessagePack codec shape).
- Ripple: counterpart of `Rasm.Materials` `[THINKTECTURE_WIRE_CODEC]` + `[MATERIALX_NODE_CATEGORY_MAPPING]` tasks (decode-only of the C#-minted `MaterialWire`).

[TS_INTERCHANGE_WIRE_PARITY]-[QUEUED]: The TypeScript interchange peer decodes the Bim BimWire/BcfWire/DiffWire/OpLogWire/BimWireDescriptor/IdsAudit vocabulary by its key/case discriminant and reproduces the WireFixture XxHash128 golden bytes, so the web peer consumes the one sealed model through the snapshot/op-log/grpc faces without re-minting a parallel BIM shape.
- Capability: Closes the cross-runtime wire contract: the TS peer decodes each closed family by the generated key/case discriminant the C# BimWireContext mints and validates the golden-byte corpus, so a model snapshot, a BCF topic, a diff change-set, and an IDS audit cross to the browser as one content-keyed payload.
- Shape: A TS interchange decoder mirroring the BimWireContext [JsonSerializable] rows (the IfcClass/Classification smart-enum keys, the ElementPredicate/AssemblyRel/ElementChange union case discriminants, the three BimWireFace projections) reproducing the WireFixture XxHash128 golden bytes for the wire types.
- Unlocks: The web peer scrubs the model, highlights the diff against the snapshot it holds, anchors BCF topics on the same GlobalIds, and reads IDS verdicts: one element identity across C# and TS, never a parallel diff/topic shape.
- Anchors: Exchange/wire#WIRE_PROJECTION BimWireContext/BimWireFace/WireFixture (ONE_MODEL_THREE_FACES + WireFixture XxHash128 corpus confirmed L14,L17); Review/diff#TS_PROJECTION DiffWire; Review/issues#TS_PROJECTION BcfWire/BcfTopicWire; typescript:interchange.
- Tension: The TS peer reproduces the C# XxHash128 golden bytes exactly: the wire shape is the C# owner's mint and the TS decoder aligns to it at the cross-libs synthesis tier, never authoring a second wire vocabulary.
- Ripple: counterpart of `Rasm.Bim` Exchange `wire` + Review `diff`/`issues` projections (`BimWire`/`DiffWire`/`BcfWire`).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
