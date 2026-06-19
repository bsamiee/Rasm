# [PY_RUNTIME_IDEAS]

The forward pool of higher-order concepts for `runtime`, grounded in the folder's domain and the monorepo purpose. Each open idea is a card — a bracketed slug, the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more `TASKLOG.md` tasks. A finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. The design pool is realized at the C# parity bar: the metrics spine, the subinterpreter offload lane, the content-addressed lane cache, the structural drift guard, the proto-transcode codec, the credential-axis decode, the MessagePack op-log decode, the capability-SDK consume, the seed-reproduction parity binding, the cp315 catalogue capture, the composition-root OTLP install owner, the inbound W3C trace-context extract, the outbound gRPC client-span/hooks, the C#-minted causal/tenant inbound frame, the streaming-transport acquisition, and the OS-keystore secret boundary are all closed. The one open card is the op-log LZ4 decompression genuinely [UPSTREAM-BLOCKED] on the cp315 `lz4` wheel and the producer-owned `Lz4BlockArray` framing — its consumer-side framing note (which callable fills the existing `CrdtOpDecode.decode(decompress)` seam) is authored and settled; only the wheel and the producer publication gate the compressed-envelope decode.

## [1]-[OPEN]

[CRDT_OPLOG_LZ4_DECODE]:
- Decompress the C# `MessagePackCompression.Lz4BlockArray` op-log envelope on the cp315 core so the settled `msgspec.msgpack` op-log decode reads the compressed delta the `OpLogEntry.Payload` carries.
- Unlocks the cp315-core collaborative op decode end to end; the MessagePack decode through `msgspec.msgpack.Decoder` is cp315-clean and only the LZ4 decompression is absent.
- [UPSTREAM-BLOCKED] on the cp315/abi3 `lz4` wheel (manifest `lz4; python_version<'3.15'`, no cp315 wheel) AND the producer-owned `Lz4BlockArray` framing: the envelope is a MessagePack-csharp block-array (a msgpack `ext` wrapping LZ4 blocks) no Python package reads natively, so the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` delta carries the framing decision (a `MessagePackCompression.None` companion lane or the published envelope spec); the companion `<3.15` band decodes the present build today.

## [2]-[CLOSED]

(none)
