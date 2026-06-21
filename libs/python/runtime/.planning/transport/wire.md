# [PY_RUNTIME_WIRE]

The single wire-codec owner the companion transport composes. `WireProtoCodec` transcodes the canonical `msgspec` `Struct` shapes interior code holds to the generated protobuf `Message` the C# gRPC services carry, and back — `protobuf 6.33`-backed, never a hand-rolled varint; `CrdtOp` is the canonical op-log delta discriminated union the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner, ten arms tag 0-9 dense and append-only; `CrdtOpDecode` is the one polymorphic decoder dispatching on op kind through a total `match`, lifting the MessagePack wire arm onto the canonical op reconstructing the `clock#CLOCK` `Hlc` and `ElementId` each arm carries. The op-log crosses as MessagePack under a `Lz4BlockArray` envelope, distinct from the gRPC proto wire — `decompress` is a dependency-injected `DecompressFn` seam the decode closes over, never a hardwired `lz4` import (LZ4 is gated `python_version<'3.15'` and the compressed-envelope decode is deferred). The page owns the companion's decode of every C#-minted wire shape and mints no wire vocabulary; `msgspec` is the in-memory frame layer and `protobuf` the wire. `WireProtoCodec`/`CrdtOp*`/`CrdtOpDecode` formerly mis-lived in `transport/serve`; they collapse here so the serve lifecycle references one codec owner rather than carrying the full codec body inline.

## [01]-[INDEX]

- [01]-[PROTO_TRANSCODE]: the one msgspec-canonical to protobuf-wire codec at the gRPC seam, the inbound `FaultDetail` decode carrying the `hlc_physical`/`hlc_logical`/`tenant` fields the serve owner lifts.
- [02]-[CRDT_DECODE]: the MessagePack op-log CRDT-op discriminated union, the polymorphic decoder, and the dependency-injected decompress seam (the durability codec, NOT protobuf).

## [02]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` — the one boundary codec generic over the canonical `msgspec` `Struct` type and its paired generated protobuf `Message` class, transcoding interior shapes to the wire and back across the gRPC seam so interior code never touches a `Message` and the wire never sees a `Struct`; it mints no second wire vocabulary, owning only the projection.
- Entry: `WireProtoCodec.encode` projects a canonical `Struct` to wire bytes — `msgspec.to_builtins` lowers the struct to a JSON-compatible mapping, `json_format.ParseDict` fills a fresh generated `Message` (snake_case field names preserved against the proto schema), and `Message.SerializeToString` emits the canonical protobuf binary; `WireProtoCodec.decode` reverses it — a fresh generated `Message` decodes the wire bytes through `Message.ParseFromString`, `json_format.MessageToDict` with `preserving_proto_field_name=True` projects it to a mapping, and `msgspec.convert` validates that mapping into the typed canonical `Struct`, a raised `msgspec.ValidationError` or protobuf decode error crossing the one `reliability/faults#FAULT` `boundary` so the codec failure lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`.
- Auto: one polymorphic `encode`/`decode` pair discriminates on the type arguments `[S: Struct, M: Message]`, never a per-message hand client; `to_builtins`/`convert` are the `msgspec` lowering/raising pair keeping the canonical shape the single source of truth, and `ParseDict`/`MessageToDict(preserving_proto_field_name=True)` hold the snake_case field parity the proto schema and the `Struct` share; the generated `_pb2` `Message` classes arrive from `grpcio-tools` compiling the C# `.proto` descriptors and the codec decodes them through the message-instance `Message.ParseFromString`, never authoring a message shape, the descriptors resolving in the one `descriptor_pool.Default()` pool the protobuf runtime indexes. The concrete pairs decode the producer messages field-for-field — `TransactionRequest`/`TransactionReceipt` (the `DocumentService.ExecuteTransaction` parity seam), `QueryRequest`/`QueryResponse`, `InferRequest`/`InferResponse`, `SolveRequest`/`SolveResponse`, `GenerateRequest`/`TokenChunk`, `GraphDiffRequest`/`GraphDiffResponse`, `SubtreeFetchRequest`/`GraphChunk`, `ArtifactFrame`, and `FaultDetail` — the C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` owner's eighteen-rpc/seventeen-message family.
- Packages: `protobuf` (`json_format.ParseDict` JSON-codec ENTRYPOINTS [04] / `json_format.MessageToDict(preserving_proto_field_name=True)` JSON-codec ENTRYPOINTS [02] / `Message.SerializeToString` message-instance ENTRYPOINTS [01] / `Message.ParseFromString` message-instance ENTRYPOINTS [02] / `descriptor_pool.Default` registries ENTRYPOINTS [02]), `msgspec` (`to_builtins` ENTRYPOINTS [06] / `convert` ENTRYPOINTS [05] / `ValidationError` PUBLIC_TYPES [03] / `Struct` PUBLIC_TYPES [01]).
- Growth: a new wire message is one `(Struct, Message)` type pair passed to the existing `encode`/`decode`; zero new surface, no second codec.
- Boundary: the codec transcodes only — no second wire vocabulary, no message shape authored here, no JSON-as-wire-format on the production path (protobuf binary is the gRPC wire); a per-message hand client, a `Message` leaking into interior code, a `Struct` crossing the wire unprojected, and a hand-rolled varint/tag framing the protobuf core already owns are the deleted forms. The `FaultDetail` decode is the inbound complement of the C# `WireFault.Decode(FaultDetail)` arm — the companion reads `(package, code, case, message, evidence, correlation, hlc_physical, hlc_logical, tenant)` off the receipt slot or the `grpc-status-details-bin` trailer onto a canonical `Struct`, the typed conflict the retry owner decodes, never a re-derived status string. The `FaultDetail` pair is just another `(Struct, Message)` codec row: the decoded `FaultDetail` `Struct` carries the `hlc_physical`/`hlc_logical`/`tenant` FIELDS, and the `clock#CLOCK` `CausalFrame.of` lift onto the `execution/admission#CONTEXT` `RuntimeContext.causal` is performed by the inbound owner `transport/serve#SERVE` `ServerHost.inbound`, NOT by `WireProtoCodec.decode` (whose body is the pure generic `Struct`↔`Message` transcode returning `msgspec.convert(mapping, self._struct)`); the two-half order rides the value-level physical-high/logical-low two-half order the `clock#CLOCK` owner reconstructs from the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] (DESIGN-PIN), an integer pack/unpack distinct from a byte-serialized little-endian claim.

```python signature
from google.protobuf import json_format
from google.protobuf.message import Message
import msgspec
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary


class WireProtoCodec[S: Struct, M: Message]:
    def __init__(self, struct: type[S], message: type[M]) -> None:
        self._struct, self._message = struct, message

    def encode(self, value: S) -> RuntimeRail[bytes]:
        return boundary("wire", lambda: json_format.ParseDict(msgspec.to_builtins(value), self._message()).SerializeToString())

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            message = self._message()
            message.ParseFromString(payload)
            mapping = json_format.MessageToDict(message, preserving_proto_field_name=True)
            return msgspec.convert(mapping, self._struct)

        return boundary("wire", project)
```

## [03]-[CRDT_DECODE]

- Owner: `CrdtOp` — the canonical op-log delta union the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` `[MessagePack.Union]` owner, ten arms tag 0-9 dense and append-only, each reconstructing the `clock#CLOCK` `Hlc` cell and `ElementId` it carries; `CrdtOpDecode` the one decode surface reading the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `DecompressFn` the dependency-injected decompress protocol the decode closes over. The op-log crosses as MessagePack under `Lz4BlockArray`, NOT protobuf — the durability codec the C# snapshot ladder and the wire seam share, distinct from the gRPC proto wire at `[2]`.
- Cases: ten op arms mirror the producer union tag-for-tag — `set`(0), `write`(1), `add`(2), `remove`(3), `increment`(4), `insert_after`(5), `delete`(6), `maintain`(7), `beat`(8), `leave`(9); LWW survives only as the `set` arm reconstructing the `LwwRegister`, the `beat`/`leave` arms carry the `EphemeralMap` presence delta the late-joining companion reconstructs from the op-log prefix. Each arm's field positions decode the producer `[Key]` order exactly — `set` is `(field, value, physical_ticks, logical, origin)` at keys 0-4, `write` adds the `(origin, seq)[]` version-vector context, `add`/`remove` carry the `(origin, logical)` element tags, `insert_after` carries the predecessor/id `(origin, logical)` pairs, `beat` carries `(field, origin, state, physical_ticks, logical)`, `leave` carries `(field, origin, physical_ticks, logical)`; the `(physical_ticks, logical)` pair lifts to the `clock#CLOCK` `Hlc` and the `(origin, logical)` pair to the `ElementId`.
- Entry: `CrdtOpDecode.decode` reads the decompressed MessagePack payload through `msgspec.msgpack.Decoder(CrdtArm)` (the `CrdtArm` union of the ten `[MessagePack.Union]` wire structs) into the tagged union, then `CrdtOpDecode.lift` projects the wire arm onto the canonical `CrdtOp` reconstructing the `Hlc` from the `(physical_ticks, logical)` pair and the `ElementId` from the `(origin, logical)` pairs — one total `match` over the ten tags, `assert_never` on the exhausted union so a producer op the companion does not decode is a typed build failure, never a silent drop; the `decompress: DecompressFn` injected at the seam chooses WHAT decompresses (identity over a `MessagePackCompression.None` companion lane, or the published `Lz4BlockArray`-envelope reader once the producer framing lands and the cp315 `lz4` wheel arrives) without a body rewrite; a decode that fails validation crosses the one `reliability/faults#FAULT` `boundary("wire", ...)` conversion and lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`, never a partial op.
- Auto: the wire is MessagePack — `msgspec.msgpack.Decoder` decodes the producer's tag-keyed array envelope where the C# `[MessagePack.Union(n, typeof(Case))]` integer `n` is the msgpack union tag (the `tag=n` integer discriminant on the `array_like=True` `Struct`) and each `[property: Key(k)]` is the array position. The envelope framings are NOT identical and the parity is a producer-side seam obligation, not a settled identity: MessagePack-csharp `[Union]` emits a two-element `[tag, sub-object]` array (the integer tag then a nested map/array of the case body), whereas a msgspec `array_like` tagged `Struct` decodes a FLAT `[tag, field0, field1, ...]` array. The `array_like=True` flat decode the page models holds only if the producer flattens the union case body into the tag array (a keyed-flat-array emission, not the default nested-union `MessagePackSerializer` shape); that producer framing decision is the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` obligation on `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, and if the producer keeps the default nested two-element union the decode prepends one intermediate-array layer (a per-case `array_like` body struct under the integer-tagged envelope) — settled against the published producer framing, never asserted ahead of it. The companion authors no op kind the wire does not carry and re-mints no op vocabulary, the C# owner being the sole mint per the single-mint invariant; `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count the `clock#CLOCK` `Hlc` raises back to the NodaTime-equivalent instant, and the `(origin, logical)` `ElementId` is the HLC-stable peer-local identity the RGA and OR-set address by, never positional; the `set` arm is the LWW `Adjudicate` survivor and the ten-arm union is the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion decoding the prefix reconstructs the identical materialized state the producer holds.
- Packages: `msgspec` (`msgpack.Decoder` ENTRYPOINTS [07] / `msgpack.Ext` PUBLIC_TYPES [03] / `Struct` / `ValidationError`), `lz4` (`block.decompress` ENTRYPOINTS [02] — the per-chunk codec inside the `Lz4BlockArray` msgpack-`ext` envelope, not the self-describing `frame` codec; injected through `DecompressFn` on the `python_version<'3.15'` companion band only, never imported here).
- Growth: a new op kind is one `CrdtOpWire` tagged-union arm plus one `CrdtOp` arm plus one `lift` match arm — the producer adds the wire tag, the companion adds the decode arm, never ahead of the wire; zero new surface, no second op vocabulary.
- Boundary: the op-log delta is the C#-owned MessagePack wire — a protobuf framing of the op-log (the prior mismodel), a typeless payload, a JSON-array delta, and a re-minted op union are the deleted forms; the decode is read-only, the companion never encoding an op the C# owner did not mint; `decompress` is a `DecompressFn` protocol seam, never a hardwired `lz4` import — the `Lz4BlockArray` envelope is the MessagePack-csharp block-array framing (a msgpack `ext` envelope wrapping independently-LZ4-block-compressed chunks), so the LZ4 decompression leg gates on the cp315 `lz4` wheel and the producer's `MessagePackCompression.None` companion-lane decision, and a hardwired LZ4 import that breaks the cp315-core import is the deleted form; the `Hlc`/`ElementId` reconstruction reads the one `clock#CLOCK` owner and re-mints no stamp.

```python signature
from typing import Protocol, assert_never

import msgspec
from msgspec import Struct

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import RuntimeRail, boundary


class DecompressFn(Protocol):
    def __call__(self, payload: bytes) -> bytes: ...


class CrdtOp(Struct, frozen=True, tag_field="op"):
    pass


class SetOp(CrdtOp, tag="set"):
    field: str
    value: bytes
    cell: Hlc
    origin: bytes


class WriteOp(CrdtOp, tag="write"):
    field: str
    value: bytes
    context: list[tuple[bytes, int]]
    cell: Hlc
    origin: bytes


class AddOp(CrdtOp, tag="add"):
    field: str
    element: bytes
    tag_id: ElementId


class RemoveOp(CrdtOp, tag="remove"):
    field: str
    element: bytes
    observed_tags: list[ElementId]


class IncrementOp(CrdtOp, tag="increment"):
    field: str
    origin: bytes
    delta: int


class InsertAfterOp(CrdtOp, tag="insert_after"):
    field: str
    predecessor: ElementId
    id: ElementId
    value: bytes


class DeleteOp(CrdtOp, tag="delete"):
    field: str
    id: ElementId


class MaintainOp(CrdtOp, tag="maintain"):
    field: str
    quiescent: list[tuple[bytes, int]]


class BeatOp(CrdtOp, tag="beat"):
    field: str
    origin: bytes
    state: bytes
    cell: Hlc


class LeaveOp(CrdtOp, tag="leave"):
    field: str
    origin: bytes
    cell: Hlc


type CrdtOpArm = SetOp | WriteOp | AddOp | RemoveOp | IncrementOp | InsertAfterOp | DeleteOp | MaintainOp | BeatOp | LeaveOp


class CrdtOpWire(Struct, frozen=True, tag_field="tag", array_like=True):
    pass


class SetWire(CrdtOpWire, tag=0):
    field: str
    value: bytes
    physical_ticks: int
    logical: int
    origin: bytes


class WriteWire(CrdtOpWire, tag=1):
    field: str
    value: bytes
    context: list[tuple[bytes, int]]
    physical_ticks: int
    logical: int
    origin: bytes


class AddWire(CrdtOpWire, tag=2):
    field: str
    element: bytes
    tag_origin: bytes
    tag_logical: int


class RemoveWire(CrdtOpWire, tag=3):
    field: str
    element: bytes
    observed_tags: list[tuple[bytes, int]]


class IncrementWire(CrdtOpWire, tag=4):
    field: str
    origin: bytes
    delta: int


class InsertAfterWire(CrdtOpWire, tag=5):
    field: str
    pred_origin: bytes
    pred_logical: int
    id_origin: bytes
    id_logical: int
    value: bytes


class DeleteWire(CrdtOpWire, tag=6):
    field: str
    id_origin: bytes
    id_logical: int


class MaintainWire(CrdtOpWire, tag=7):
    field: str
    quiescent: list[tuple[bytes, int]]


class BeatWire(CrdtOpWire, tag=8):
    field: str
    origin: bytes
    state: bytes
    physical_ticks: int
    logical: int


class LeaveWire(CrdtOpWire, tag=9):
    field: str
    origin: bytes
    physical_ticks: int
    logical: int


type CrdtArm = SetWire | WriteWire | AddWire | RemoveWire | IncrementWire | InsertAfterWire | DeleteWire | MaintainWire | BeatWire | LeaveWire


class CrdtOpDecode:
    _decoder: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)

    @staticmethod
    def lift(wire: CrdtArm) -> CrdtOpArm:
        match wire:
            case SetWire(field, value, physical_ticks, logical, origin):
                return SetOp(field, value, Hlc(physical_ticks, logical), origin)
            case WriteWire(field, value, context, physical_ticks, logical, origin):
                return WriteOp(field, value, context, Hlc(physical_ticks, logical), origin)
            case AddWire(field, element, tag_origin, tag_logical):
                return AddOp(field, element, ElementId(tag_origin, tag_logical))
            case RemoveWire(field, element, observed_tags):
                return RemoveOp(field, element, [ElementId(o, n) for o, n in observed_tags])
            case IncrementWire(field, origin, delta):
                return IncrementOp(field, origin, delta)
            case InsertAfterWire(field, pred_origin, pred_logical, id_origin, id_logical, value):
                return InsertAfterOp(field, ElementId(pred_origin, pred_logical), ElementId(id_origin, id_logical), value)
            case DeleteWire(field, id_origin, id_logical):
                return DeleteOp(field, ElementId(id_origin, id_logical))
            case MaintainWire(field, quiescent):
                return MaintainOp(field, quiescent)
            case BeatWire(field, origin, state, physical_ticks, logical):
                return BeatOp(field, origin, state, Hlc(physical_ticks, logical))
            case LeaveWire(field, origin, physical_ticks, logical):
                return LeaveOp(field, origin, Hlc(physical_ticks, logical))
            case _ as unreachable:
                assert_never(unreachable)

    @classmethod
    def decode(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[CrdtOpArm]:
        return boundary("wire", lambda: cls.lift(cls._decoder.decode(decompress(payload))))
```

## [04]-[RESEARCH]

- [PROTO_TRANSCODE]: [COMPLETE] — reflection-confirmed against the runtime catalogs `.api/protobuf.md` and `.api/msgspec.md`. `msgspec.to_builtins`/`msgspec.convert` are the canonical lowering/raising pair (ENTRYPOINTS [06]/[05]), and `json_format.ParseDict` (JSON-codec ENTRYPOINTS [04]) / `json_format.MessageToDict(preserving_proto_field_name=True)` (JSON-codec ENTRYPOINTS [02]) / the message-instance `Message.ParseFromString` (ENTRYPOINTS [02]) / `Message.SerializeToString` (ENTRYPOINTS [01]) are the protobuf seam projection — the decode constructs a fresh generated `Message` and parses in place rather than a `proto` functional call the runtime catalog does not carry; `protobuf 6.33.6` is cp315-clean core-direct (no environment marker), so the proto codec rides the core floor. The codec is settled; the concrete `(Struct, Message)` pairs decode the eighteen-rpc/seventeen-message C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` family and await only `grpcio-tools` compiling the landed `.proto` descriptors to `_pb2`.
- [CRDT_DECODE_MSGPACK]: [COMPLETE] (cp315-clean leg) — the op-log crosses as MessagePack (`CrdtOpWire` `[MessagePack.Union]` tags 0-9, the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner), decoded through `msgspec.msgpack.Decoder` (`.api/msgspec.md` ENTRYPOINTS [07], cp315-clean core). The `msgspec.Struct` tagged-union with `tag_field`/`array_like=True` and integer `tag=n` decodes the producer's tag-keyed array envelope against the `[property: Key(k)]` order, validated arm-for-arm — the flat `[tag, field0, ...]` `array_like` decode matching the producer only if `CRDT_WIRE` emits the keyed-flat-array framing (the `[MessagePack.Union]` default nests the case body as a two-element `[tag, sub-object]` array, so the flat-vs-nested framing is the `CRDT_OPLOG_WIRE_AMENDMENT` producer-seam obligation, not a position-for-position identity the companion may assume ahead of the published framing): `set`/`beat`/`leave` split the `(physical_ticks, logical)` pair the `clock#CLOCK` `Hlc` reconstruction reads, `add`/`remove`/`insert_after`/`delete` carry the `(origin, logical)` `ElementId` pairs. `CrdtOpDecode.lift` is the total `match`/`assert_never` projection onto the canonical `CrdtOp` union mirroring the C# `CrdtWire.Map` switch — `decode` runs `lift ∘ Decoder.decode ∘ decompress`, so an unmapped wire arm is a typed build failure, never a raw-wire leak. The MessagePack decode and the canonical lift are settled on the cp315 core.
- [CRDT_DECODE_LZ4]: [UPSTREAM-BLOCKED] — the C# producer compresses the op-log payload under `MessagePackCompression.Lz4BlockArray`, the MessagePack-csharp block-array framing (a msgpack `ext` type wrapping independently-LZ4-block-compressed chunks), not a raw LZ4 frame. The `CrdtOpDecode.decode(cls, payload, decompress)` seam injects the `decompress: DecompressFn` protocol, so the resolution is purely WHICH callable fills it, never a body rewrite — (a) a `MessagePackCompression.None` companion lane fills `decompress` with the identity function over the uncompressed delta the settled `msgspec.msgpack.Decoder(CrdtArm)` leg already decodes, needing no LZ4 dependency on the core; or (b) the published `Lz4BlockArray` envelope reader (per-chunk `.api/lz4.md` `block.decompress` ENTRYPOINTS [02] over each block inside the msgpack-`ext` envelope, the raw-block codec the catalog block-law scopes to an outer envelope, NOT the self-describing `frame` codec) the three runtimes share, gated on the cp315 `lz4` wheel. Two genuinely-upstream gates remain: (1) `lz4` is manifest-declared `lz4; python_version<'3.15'` with NO cp315 wheel synced, so the LZ4 decompression rides the `python_version<'3.15'` companion band exactly like `xxhash`; (2) the producer `Lz4BlockArray`-envelope framing publication (the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` delta on `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, where LZ4 stays C#-internal and the `MessagePackCompression.None` lane serves cross-runtime consumers). The `msgspec.msgpack` decode of the uncompressed delta is proven on the cp315 core; the compressed-envelope link stays gated through the `DecompressFn` seam, never a hardwired `lz4` import that breaks the cp315-core import.
