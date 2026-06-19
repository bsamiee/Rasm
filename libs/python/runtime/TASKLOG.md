# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

## [1]-[OPEN]

[BLOCKED] [CRDT_OPLOG_LZ4]:
- Decompress the C# `MessagePackCompression.Lz4BlockArray` op-log envelope on the cp315 core at `transport/serve#CRDT_DECODE`; the `msgspec.msgpack.Decoder(CrdtArm)` decode of the uncompressed delta (the `CrdtArm` union of the ten wire structs) is settled and cp315-clean, the LZ4 decompression is the single absent leg.
- Integrate `lz4` (`lz4.block`, companion `python_version<'3.15'` band); no cp315/abi3 `lz4` wheel synced.
- Producer is `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtWire.Encode` under `Lz4BlockArray`; the `Lz4BlockArray` envelope is a MessagePack-csharp block-array framing (a msgpack `ext` wrapping independently-LZ4-block-compressed chunks), not a raw LZ4 frame read by `lz4.block`/`lz4.frame` or `msgspec` natively — the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` delta carries the producer-owned framing decision (a `MessagePackCompression.None` companion lane the cp315 decode fills `decompress` with identity over, or the published envelope spec). The consumer-side framing note (which callable fills the already-injected `CrdtOpDecode.decode(decompress)` seam, `transport/serve#CRDT_DECODE` `[7]-[RESEARCH]` `[CRDT_DECODE_LZ4]`) is authored and settled this campaign; only the wheel and the producer publication remain.
- [UPSTREAM-BLOCKED] on the cp315 `lz4` wheel AND the producer `Lz4BlockArray`-envelope framing publication; the companion `<3.15` band decodes the present build today.

## [2]-[CLOSED]

(none)
