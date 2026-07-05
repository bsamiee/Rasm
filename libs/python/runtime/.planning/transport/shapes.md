# [PY_RUNTIME_SHAPES]

The Python half of the suite wire vocabulary. Every canonical `msgspec.Struct` the companion transport carries is minted HERE, field-for-field against the C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` producer table — the sixteen service shapes, the `GeometryPayload` tensor envelope family they nest, and the geometry tessellation pair the `artifact_frame` leg streams. `PROTO_VOCABULARY` is the one seed table binding each shape to its generated `channels_pb2` message under its semantic name, and `aligned` is the `descriptor_pool`-backed drift gate proving the compiled descriptors match the registry rows structurally — field names AND 64-bit decode floors — before the first RPC. `transport/wire#PROTO_TRANSCODE` imports the table and owns only transcode machinery; a registry living wire-side would force a `shapes -> wire` back-edge from the gate, so the vocabulary, the binding rows, and the gate live one tier down and wire builds its codec instances from the rows.

Single-mint law: every shape DECODES the C#-minted proto and none is authored wire-first — the `.proto` files, the `CredentialPolicy` axis, the HLC mint, and the `FaultDetail` schema are the producer's; the companion adds no field the `.proto` does not carry, and additive producer drift is tolerated by construction (unknown mapping keys pass `msgspec.convert` unrejected) because cross-language evolution rides the one C# `ContractGuard.AdditiveOnly` classifier, never a Python-side `forbid_unknown_fields` that would break the additive contract.

Two 64-bit contracts anchor here. WIRE SLOTS: proto3 JSON emits every `uint64`/`int64`/`fixed64` field as a DECIMAL STRING to survive JS number precision, so `msgspec.convert(mapping, struct, strict=False)` coerces the string onto the typed `int` slot under the field's own `Meta` bound — `WireU64` carries the `ge=0` floor the msgspec C core admits at `Decoder` construction (a `ge=0, lt=2**64` bound overflows int64 and crashes the build), the full unsigned domain round-trips, and the `<2**64` ceiling stays the `clock#CLOCK` owner's — enforced at its own `convert`-decode gate, `Meta` running only at decode, so on these slots the ceiling rides the single-mint producer domain, never a construction-time guard. CANONICAL BYTES: the content-key byte stream is count-prefixed per the `.archive/RASM-COMPONENT-PARADIGM-DECISION.md` `[AMENDMENTS]` canonical-bytes law — the C# `CanonicalWriter` is the sole mint, the `evidence/identity#IDENTITY` key runs over those producer bytes, and the C#-side Python wire-alignment campaign builds against this anchor; this page records the obligation and re-mints nothing (the CRDT op-log msgpack arrays are self-delimiting — the amendment governs canonical bytes for content keys, not wire framing).

## [01]-[INDEX]

- [02]-[VOCABULARY]: the `WireU64`/`Stamp`/`Packed` slot types, the `GeometryPayload` tensor envelope family, the sixteen canonical service shapes with proto3-zero defaults — the nine-field `FaultDetail` among them — and the geometry `TessellationRequest`/`TessellationReceipt` pair.
- [03]-[REGISTRY_AND_DRIFT]: the `PROTO_VOCABULARY` seed table, the `grpcio-tools` static-codegen contract minting `rasm.runtime._pb2.channels_pb2`, and the `aligned` descriptor-pool drift gate.

## [02]-[VOCABULARY]

- Owner: the closed shape family — sixteen canonical `msgspec.Struct` service shapes (`TransactionRequest`/`TransactionReceipt`, `QueryRequest`/`QueryResponse`, `InferRequest`/`InferResponse`, `SolveRequest`/`SolveResponse`, `GenerateRequest`/`TokenChunk`, `GraphDiffRequest`/`GraphDiffResponse`, `SubtreeFetchRequest`/`GraphChunk`, `ArtifactFrame`, `FaultDetail`), the nested `GeometryPayload` envelope family (`PointCloudTensor`/`MeshTensor`/`VoxelTensor`/`SymbolicDim`) the Infer pair carries, and the tessellation pair (`TessellationRequest`/`TessellationReceipt`) the geometry `mesh/serve` servicer registers. Field names are the producer's snake_case proto names verbatim — `json_format.MessageToDict(message, preserving_proto_field_name=True)` keys the mapping by them and `msgspec.convert` raises it by name, so no `rename=` layer exists and the struct declaration IS the wire contract.
- Cases: every scalar slot carries its proto3 zero default (`""`/`0`/`0.0`/`False`/`b""`/`()`) because `MessageToDict` omits a field at its default value — a default-less slot would reject the producer's legitimate zero, so presence is the proto3 no-presence contract, never a Python-side required-field re-mint. A nested message slot (`conflict`, `payload`, `hlc_physical`, the `GeometryPayload` oneof members) is `T | None = None` — proto message absence is a real wire value, the one place `None` crosses inward, collapsed by the consuming owner at its seam. The `GeometryPayload` oneof (`point_cloud`/`mesh`/`voxel`) decodes as three nullable members of which the producer sets at most one; the numeric-lane `SolveRequest` deliberately carries bare column-major `float64` bytes, never a tensor envelope (the producer's own no-geometry-envelope law).
- Cases: `FaultDetail` is the nine-field typed conflict the whole suite converges on — `package`/`code`/`case_`/`message`/`evidence`/`correlation`/`hlc_physical`/`hlc_logical`/`tenant` — riding the `TransactionReceipt.conflict` receipt slot in band and the `grpc-status-details-bin` trailer out of band; the C# `WireFault.Decode(FaultDetail)` and `WireFault.PackConflict` are the producer's two arms, and `transport/serve#SERVE` owns the Python egress (`settle` packs the trailer) and ingress (the invoke fault path decodes it) — this page owns only the shape. `case_` spells the proto `case` field through `msgspec.field(name="case")` because the bare name is claimed by the `expression` case-DSL vocabulary corpus-wide; the wire name stays `case`.
- Cases: the tessellation pair is C#-minted by `Rasm.Bim/Exchange/tessellation` + `Rasm.Compute/Runtime/codecs` and streamed over the existing `artifact_frame` leg; the request carries the source modality, the source bytes, and the tessellation-policy echo, the receipt carries the content key, the element/triangle tally, the semantic header, the whole-artifact `XxHash128` hex, and the replay phase — the decoded field floor geometry `mesh/serve` binds by symbol, a sibling importing the rows and minting no wire shape.
- Auto: the causal halves stay wire-typed — `hlc_physical` is the RFC 3339 `Stamp` the Timestamp JSON mapping emits and `hlc_logical` the `WireU64` decimal-string coercion — and no shape lifts them to `Hlc`: the `clock#CLOCK` owner reconstructs causal cells at full 100-ns tick fidelity from its own carrier slots, and a `datetime`-mediated lift here would truncate to microseconds, so the lift is the consuming owner's, never a convenience property that silently loses precision. `Packed` types the two producer-open envelopes — the `@type`-keyed `Any` op rows and the `google.protobuf.Struct` predicate/row payloads — open within the additive-only contract by the producer's own design, dispatched by the document verb owner on the `@type` discriminant, never widened past these declared slots.
- Packages: `msgspec` (`Struct`/`Meta`/`field(name=)` PUBLIC_TYPES [01]/[09], the `convert(strict=False)` decimal-string coercion leg `transport/wire#PROTO_TRANSCODE` runs), `protobuf` (the generated `channels_pb2` classes the registry binds; `json_format.MessageToDict(preserving_proto_field_name=True)`/`ParseDict` the mapping seam), `reliability/faults#FAULT` (`BoundaryFault`/`RuntimeRail` the gate's rail).
- Growth: a new producer message is one `Struct` plus one `PROTO_VOCABULARY` row — the drift gate proves it against the compiled descriptor on the next boot and `transport/wire#PROTO_TRANSCODE` transcodes it with zero new surface; a new field on an existing message is one slot with its proto3 zero default; a new sibling-consumer field floor (the next geometry/data streamed artifact) is one registry row pair, never a sibling vocabulary.
- Boundary: this page mints shapes and the registry only — no codec, no span, no retry, no transcode body (`transport/wire#PROTO_TRANSCODE` owns both legs), no causal lift (`clock#CLOCK`), no trailer pack/unpack (`transport/serve#SERVE`). The deleted forms: a wire shape authored ahead of the producer `.proto`; a `rename=` camel layer where `preserving_proto_field_name` keeps snake_case end-to-end; `forbid_unknown_fields` breaking the additive-evolution contract; a required-field re-mint over the proto3 no-presence contract; an `Hlc`-lifting property that truncates ticks through `datetime`; a parallel `TessellationRequestWire` beside the one registry row; and a `clock.U64`-typed wire slot whose `lt=2**64` bound crashes the msgspec `Decoder` build.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime
from typing import Annotated

import msgspec
from msgspec import Meta, Struct

# --- [TYPES] ----------------------------------------------------------------------------

# the wire-slot decode floor: proto3 JSON emits 64-bit fields as decimal strings, and the
# msgspec C core rejects a `lt=2**64` bound at Decoder construction, so the floor alone rides
# the slot and the ceiling rides the single-mint producer domain (clock#CLOCK enforces its
# own halves at convert-decode, never at a direct construction).
type WireU64 = Annotated[int, Meta(ge=0)]
type Stamp = Annotated[datetime, Meta(tz=True)]
type Packed = dict[str, object]

# --- [MODELS] ---------------------------------------------------------------------------


class SymbolicDim(Struct, frozen=True, gc=False):
    name: str = ""
    bound: int = 0


class PointCloudTensor(Struct, frozen=True, gc=False):
    count: int = 0
    channels: int = 0
    dtype: str = ""
    data: bytes = b""


class MeshTensor(Struct, frozen=True, gc=False):
    vertex_count: int = 0
    vertices: bytes = b""
    face_count: int = 0
    faces: bytes = b""


class VoxelTensor(Struct, frozen=True):
    dims: tuple[int, ...] = ()
    dtype: str = ""
    data: bytes = b""


class GeometryPayload(Struct, frozen=True):
    # producer oneof: at most one of point_cloud/mesh/voxel is set per frame.
    point_cloud: PointCloudTensor | None = None
    mesh: MeshTensor | None = None
    voxel: VoxelTensor | None = None
    symbolic_dims: tuple[SymbolicDim, ...] = ()


class FaultDetail(Struct, frozen=True):
    package: str = ""
    code: int = 0
    case_: str = msgspec.field(name="case", default="")
    message: str = ""
    evidence: dict[str, str] = msgspec.field(default_factory=dict)
    correlation: str = ""
    hlc_physical: Stamp | None = None
    hlc_logical: WireU64 = 0
    tenant: str = ""


class TransactionRequest(Struct, frozen=True):
    idempotency_key: bytes = b""
    ops: tuple[Packed, ...] = ()
    expected_epoch: WireU64 = 0
    hlc_physical: Stamp | None = None
    hlc_logical: WireU64 = 0
    correlation: str = ""


class TransactionReceipt(Struct, frozen=True):
    idempotency_key: bytes = b""
    committed: bool = False
    new_epoch: WireU64 = 0
    applied: tuple[str, ...] = ()
    conflict: FaultDetail | None = None
    hlc_physical: Stamp | None = None
    hlc_logical: WireU64 = 0


class QueryRequest(Struct, frozen=True):
    scope: str = ""
    predicate: Packed = msgspec.field(default_factory=dict)
    cursor: str = ""
    mask: str = ""


class QueryResponse(Struct, frozen=True):
    rows: tuple[Packed, ...] = ()
    cursor: str = ""
    total: int = 0


class InferRequest(Struct, frozen=True):
    payload: GeometryPayload | None = None
    model_checksum: str = ""
    correlation: str = ""


class InferResponse(Struct, frozen=True):
    payload: GeometryPayload | None = None
    provider: str = ""


class SolveRequest(Struct, frozen=True, gc=False):
    matrix: bytes = b""
    rhs: bytes = b""
    factorization_kind: str = ""
    sparse_format: str = ""
    shard_tile: int = 0


class SolveResponse(Struct, frozen=True, gc=False):
    solution: bytes = b""
    provider: str = ""
    decomposition: str = ""
    rows: int = 0
    cols: int = 0
    nnz: int = 0


class GenerateRequest(Struct, frozen=True, gc=False):
    model_checksum: str = ""
    prompt: str = ""
    max_length: float = 0.0
    guidance_kind: str = ""
    guidance_data: str = ""
    tools: str = ""


class TokenChunk(Struct, frozen=True, gc=False):
    piece: str = ""
    token_index: int = 0
    done: bool = False


class GraphDiffRequest(Struct, frozen=True, gc=False):
    base_hash: str = ""
    target_hash: str = ""


class GraphDiffResponse(Struct, frozen=True):
    added: tuple[str, ...] = ()
    removed: tuple[str, ...] = ()


class SubtreeFetchRequest(Struct, frozen=True):
    content_keys: tuple[str, ...] = ()


class GraphChunk(Struct, frozen=True, gc=False):
    content_key: str = ""
    payload: bytes = b""
    ordinal: int = 0


class ArtifactFrame(Struct, frozen=True, gc=False):
    artifact_id: bytes = b""
    artifact_bytes: int = 0
    offset: int = 0
    frame_crc: WireU64 = 0
    payload: bytes = b""


class TessellationRequest(Struct, frozen=True):
    source_modality: str = ""
    source: bytes = b""
    policy: dict[str, str] = msgspec.field(default_factory=dict)


class TessellationReceipt(Struct, frozen=True):
    content_key: str = ""
    element_count: int = 0
    triangle_count: int = 0
    semantic_header: Packed = msgspec.field(default_factory=dict)
    artifact_hash: str = ""
    replay_phase: str = ""
```

## [03]-[REGISTRY_AND_DRIFT]

- Owner: `PROTO_VOCABULARY` — the one seed table binding each canonical shape to its generated `channels_pb2` message under its semantic name, twenty-three `(name, type[Struct], type[Message])` rows — the sixteen service legs, the tessellation pair, and the five nested `GeometryPayload` envelope shapes, registered because `_drift` walks only a registered row's top-level fields, so an unregistered nested message would drift field-silently while its parent row still passed — the whole transport plane reads: `transport/wire#PROTO_TRANSCODE` builds its `WIRE_REGISTRY` codec instances from the rows, `transport/serve#SERVE` resolves servicer codec pairs by name, and the drift gate folds the rows against the process descriptor pool. `aligned` is that gate — the executable assertion that the compiled `.proto` and this vocabulary agree, run once by the daemon composition root before serve binds.
- Entry: `aligned()` folds every row through `_drift` and returns `RuntimeRail[int]` — the row count on `Ok`, one `config`-tagged `BoundaryFault` naming every divergence on `Error`. Per row, `descriptor_pool.Default().FindMessageTypeByName(message.DESCRIPTOR.full_name)` resolves the compiled `Descriptor` (importing `channels_pb2` registered its serialized file in the default pool, so the lookup is a read, never an `AddSerializedFile` re-registration), `msgspec.inspect.type_info(struct)` yields the `StructType` whose `Field.encode_name` set is the struct's wire-name contract, and the fold asserts three laws structurally: every compiled field has a struct slot (absent slot = a producer field the companion silently drops), every struct slot has a compiled field (phantom slot = a field that decodes to its default forever), and every 64-bit compiled slot (`TYPE_INT64`/`TYPE_SINT64`/`TYPE_SFIXED64`/`TYPE_UINT64`/`TYPE_FIXED64`) lands on an `inspect.IntType` node — carrying the `ge=0` `WireU64` floor when the wire type is unsigned — so the decimal-string coercion contract is proven per slot, never hand-eyeballed against a prose table.
- Auto: the codegen contract is static, build-time, and one command — `grpc_tools.protoc.main(["-I<proto-root>", "--python_out=<pkg>", "--pyi_out=<pkg>", "rasm/channels.proto"])` mints `rasm.runtime._pb2.channels_pb2` (+ `.pyi` stubs the type gates read) from the C#-owned `.proto`, `command.build_package_protos(package_root, strict_mode=True)` is the CI form that fails the build on the first compile error, and the generated `_pb2` package imports nothing intra-package, sitting immediately below this module in the import DAG. The `sys.meta_path` dynamic-stub path (`ProtoFinder`/`ProtoLoader` backing `grpc.protos(...)`) is the DELETED form for this pinned vocabulary — the runtime imports the generated module, never generates at import time, and `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS=1` seals the hooks off.
- Packages: `protobuf` (`descriptor_pool.Default()`/`DescriptorPool.FindMessageTypeByName` registry ENTRYPOINTS [01]/[02], `FieldDescriptor` type constants the 64-bit fold keys), `msgspec` (`inspect.type_info`/`inspect.StructType`/`inspect.Field.encode_name`/`inspect.IntType` introspection ENTRYPOINTS [01] + type nodes — the struct-side field-type tree the gate cross-checks), `grpcio-tools` (`protoc.main` codegen ENTRYPOINTS [01], `command.build_package_protos(strict_mode=True)` [03]), `expression` (`Block.of_seq`/`Block.collect`/`Block.choose` the drift fold).
- Growth: a new wire pair is one `PROTO_VOCABULARY` row the gate proves and wire transcodes with zero new surface; a new structural assertion (a repeated-cardinality check, a oneof-membership check) is one arm in `_drift`, never a second gate; a new sibling consumer binds existing rows by symbol.
- Boundary: the gate proves structure, not values — byte-level round-trip parity is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, and the additive-drift admission decision is the C# `ContractGuard.AdditiveOnly` classifier's (an extra compiled field the struct lacks IS flagged here, because the companion dropping a producer field is a decode gap even when the contract admits the addition). The deleted forms: a prose field table standing in for the executable assertion; a runtime `AddSerializedFile` re-registration of a file the generated import already registered; a per-message hand comparison; and the import-time dynamic-stub generation the static codegen contract rejects.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final

import msgspec
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
from google.protobuf import descriptor_pool
from google.protobuf.descriptor import FieldDescriptor
from google.protobuf.message import Message
from msgspec import Struct

from rasm.runtime._pb2 import channels_pb2
from rasm.runtime.faults import BoundaryFault, RuntimeRail

# --- [CONSTANTS] ------------------------------------------------------------------------

_WIDE: Final[frozenset[int]] = frozenset({
    FieldDescriptor.TYPE_INT64,
    FieldDescriptor.TYPE_SINT64,
    FieldDescriptor.TYPE_SFIXED64,
    FieldDescriptor.TYPE_UINT64,
    FieldDescriptor.TYPE_FIXED64,
})
_UNSIGNED: Final[frozenset[int]] = frozenset({FieldDescriptor.TYPE_UINT64, FieldDescriptor.TYPE_FIXED64})

# --- [TABLES] ---------------------------------------------------------------------------

PROTO_VOCABULARY: Final[tuple[tuple[str, type[Struct], type[Message]], ...]] = (
    ("execute_transaction", TransactionRequest, channels_pb2.TransactionRequest),
    ("transaction_receipt", TransactionReceipt, channels_pb2.TransactionReceipt),
    ("query", QueryRequest, channels_pb2.QueryRequest),
    ("query_response", QueryResponse, channels_pb2.QueryResponse),
    ("infer", InferRequest, channels_pb2.InferRequest),
    ("infer_response", InferResponse, channels_pb2.InferResponse),
    ("solve", SolveRequest, channels_pb2.SolveRequest),
    ("solve_response", SolveResponse, channels_pb2.SolveResponse),
    ("generate", GenerateRequest, channels_pb2.GenerateRequest),
    ("token_chunk", TokenChunk, channels_pb2.TokenChunk),
    ("graph_diff", GraphDiffRequest, channels_pb2.GraphDiffRequest),
    ("graph_diff_response", GraphDiffResponse, channels_pb2.GraphDiffResponse),
    ("subtree_fetch", SubtreeFetchRequest, channels_pb2.SubtreeFetchRequest),
    ("graph_chunk", GraphChunk, channels_pb2.GraphChunk),
    ("artifact_frame", ArtifactFrame, channels_pb2.ArtifactFrame),
    ("fault_detail", FaultDetail, channels_pb2.FaultDetail),
    ("geometry_payload", GeometryPayload, channels_pb2.GeometryPayload),
    ("point_cloud_tensor", PointCloudTensor, channels_pb2.PointCloudTensor),
    ("mesh_tensor", MeshTensor, channels_pb2.MeshTensor),
    ("voxel_tensor", VoxelTensor, channels_pb2.VoxelTensor),
    ("symbolic_dim", SymbolicDim, channels_pb2.SymbolicDim),
    ("tessellate", TessellationRequest, channels_pb2.TessellationRequest),
    ("tessellation_receipt", TessellationReceipt, channels_pb2.TessellationReceipt),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _drift(name: str, struct: type[Struct], message: type[Message]) -> Block[str]:
    compiled = descriptor_pool.Default().FindMessageTypeByName(message.DESCRIPTOR.full_name)
    slots = {field.encode_name: field for field in msgspec.inspect.type_info(struct).fields}
    absent = Block.of_seq(sorted(frozenset(compiled.fields_by_name) - frozenset(slots))).map(lambda gap: f"{name}.{gap}:producer-field-unmapped")
    phantom = Block.of_seq(sorted(frozenset(slots) - frozenset(compiled.fields_by_name))).map(lambda gap: f"{name}.{gap}:slot-never-decodes")

    def floored(wire: FieldDescriptor) -> Option[str]:
        node = slots[wire.name].type
        held = isinstance(node, msgspec.inspect.IntType) and (wire.type not in _UNSIGNED or node.ge == 0)
        return Nothing if held else Some(f"{name}.{wire.name}:64-bit-slot-without-WireU64-floor")

    wide = Block.of_seq(field for field in compiled.fields_by_name.values() if field.type in _WIDE and field.name in slots).choose(floored)
    return absent.append(phantom).append(wide)


def aligned() -> RuntimeRail[int]:
    names = frozenset(name for name, _, _ in PROTO_VOCABULARY)
    drift = Block.of_seq(PROTO_VOCABULARY).collect(lambda row: _drift(*row))
    return (
        Error(BoundaryFault(config=("shapes.registry", "duplicate-row-name")))
        if len(names) != len(PROTO_VOCABULARY)
        else Ok(len(PROTO_VOCABULARY))
        if drift.is_empty()
        else Error(BoundaryFault(config=("shapes.drift", ";".join(drift))))
    )
```
