# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

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

[CRDT_OPLOG_LZ4]-[BLOCKED]: decode the compressed CRDT op-log envelope.
- Capability: the cp315 core decodes the C# `MessagePackCompression.Lz4BlockArray` op-log envelope at `transport/serve#CRDT_DECODE`, with the settled `msgspec.msgpack.Decoder(CrdtArm)` uncompressed-delta leg remaining the canonical MessagePack union decode.
- Shape: `CrdtOpDecode.decode(decompress)` keeps the consumer-side seam as one injected callable: either identity over a producer-owned `MessagePackCompression.None` companion lane, or the published `Lz4BlockArray` reader over `lz4.block`; raw `lz4.frame`, raw block bytes, protobuf, JSON, and a second op vocabulary stay outside the owner.
- Unlocks: collaborative CRDT op decode runs end to end on the cp315 core instead of relying on the current `python_version<'3.15'` companion band for compressed builds.
- Anchors: `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, `CrdtWire.Encode`, `MessagePackCompression.Lz4BlockArray`, `CRDT_OPLOG_WIRE_AMENDMENT`, `transport/serve#CRDT_DECODE` `[CRDT_DECODE_LZ4]`, `msgspec.msgpack.Decoder(CrdtArm)`, and `lz4`.
- Tension: blocked on both a cp315/abi3 `lz4` wheel and the producer publication decision: expose a `MessagePackCompression.None` companion lane or publish the shared MessagePack-csharp `Lz4BlockArray` ext-envelope spec.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
