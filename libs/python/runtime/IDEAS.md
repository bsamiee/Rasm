# [PY_RUNTIME_IDEAS]

The forward pool of higher-order concepts for `runtime`, grounded in the folder's domain and the monorepo purpose. Each open idea is a card — a bracketed slug, the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more `TASKLOG.md` tasks. A finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. The design pool is realized at the C# parity bar: the metrics spine, the subinterpreter offload lane, the content-addressed lane cache, the structural drift guard, the proto-transcode codec, the credential-axis decode, the MessagePack op-log decode, the capability-SDK consume, the seed-reproduction parity binding, the cp315 catalogue capture, the composition-root OTLP install owner, the inbound W3C trace-context extract, the outbound gRPC client-span/hooks, the C#-minted causal/tenant inbound frame, the streaming-transport acquisition, and the OS-keystore secret boundary are all closed. The one open card is the op-log LZ4 decompression genuinely [UPSTREAM-BLOCKED] on the cp315 `lz4` wheel and the producer-owned `Lz4BlockArray` framing — its consumer-side framing note (which callable fills the existing `CrdtOpDecode.decode(decompress)` seam) is authored and settled; only the wheel and the producer publication gate the compressed-envelope decode.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[CRDT_OPLOG_LZ4_DECODE]-[QUEUED]: cp315 core decodes the compressed C# CRDT op-log envelope.
- Capability: `runtime` completes the compressed-envelope leg of the C#-minted CRDT op-log decode without re-minting the op vocabulary or changing the settled `msgspec.msgpack` union lift.
- Shape: `CrdtOpDecode.decode(decompress)` keeps the existing decompression seam; the chosen callable reads either an uncompressed `MessagePackCompression.None` companion payload or the published `MessagePackCompression.Lz4BlockArray` envelope for `OpLogEntry.Payload`.
- Unlocks: collaborative op decode runs end to end on the cp315 core instead of relying on the `<3.15` companion band for the present compressed build.
- Anchors: `transport/serve#CRDT_DECODE`, `CRDT_OPLOG_WIRE_AMENDMENT`, `msgspec.msgpack.Decoder(CrdtArm)`, C# `CrdtWire.Encode`, and the MessagePack-csharp `Lz4BlockArray` ext98 block-array framing.
- Tension: cp315/abi3 `lz4` wheel support and the producer decision between the uncompressed companion lane and a published envelope spec.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
