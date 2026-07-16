# [PY_RUNTIME_SHAPES]

The Python half of the suite wire vocabulary: every canonical `msgspec.Struct` the companion transport carries is minted HERE, field-for-field against the C# `Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` producer table. Single-mint law: every shape DECODES the C#-minted proto and none is authored wire-first — the `.proto` files, the HLC mint, and the `FaultDetail` schema are the producer's; additive producer drift is tolerated by construction because cross-language evolution rides the one C# `ContractGuard.AdditiveOnly` classifier, never a Python-side `forbid_unknown_fields` that breaks the additive contract.

`PROTO_VOCABULARY` binds each shape to its generated `channels_pb2` message, and `aligned` is the descriptor-pool drift gate proving compiled descriptors match the rows — field names and 64-bit decode floors — before the first RPC; the registry lives one tier below `transport/wire#PROTO_TRANSCODE` because a wire-side registry forces a `shapes -> wire` back-edge from the gate. The canonical-bytes obligation is recorded here and re-minted nowhere: the C# `CanonicalWriter` is the sole mint of the count-prefixed content-key byte stream the `evidence/identity#IDENTITY` key runs over; the CRDT op-log msgpack arrays are self-delimiting, the amendment governing content keys, not wire framing.

## [01]-[INDEX]

- [01]-[VOCABULARY]: the wire slot types, the `GeometryPayload` envelope family, the canonical service shapes, and the tessellation pair.
- [02]-[REGISTRY_AND_DRIFT]: the `PROTO_VOCABULARY` seed table, the static-codegen contract, and the `aligned` drift gate.

## [02]-[VOCABULARY]

- Owner: field names are the producer's snake_case proto names verbatim — `MessageToDict(preserving_proto_field_name=True)` keys the mapping by them — so no `rename=` layer exists and the struct declaration IS the wire contract.
- Cases: every scalar slot carries its proto3 zero default because `MessageToDict` omits a field at its default value — a default-less slot rejects the producer's legitimate zero, so presence is the proto3 no-presence contract, never a required-field re-mint. A nested message slot is `T | None = None` — proto message absence is a real wire value, the one place `None` crosses inward, collapsed by the consuming owner at its seam. `SolveRequest` deliberately carries bare column-major `float64` bytes, never a tensor envelope, per the producer's no-geometry-envelope law.
- Auto: no shape lifts the causal halves to `Hlc` — the `clock#CLOCK` owner reconstructs causal cells at full 100-ns tick fidelity from the carrier slots, and a `datetime`-mediated lift here truncates to microseconds. `Packed` types the two producer-open envelopes, open within the additive-only contract by the producer's own design and never widened past the declared slots. The tessellation pair is C#-minted and streamed over the existing `artifact_frame` leg — geometry `mesh/serve` binds the decoded field floor by symbol, minting no wire shape.
- Receipt: `FaultDetail` is the typed conflict the whole suite converges on, riding the `TransactionReceipt.conflict` slot in band and the `grpc-status-details-bin` trailer out of band; `transport/serve#SERVE` owns the Python trailer egress and ingress — this page owns only the shape.
- Packages: `msgspec`, `protobuf`, and the faults rail per the fence imports; `transport/wire#PROTO_TRANSCODE` runs the `convert(strict=False)` decimal-string coercion leg.
- Growth: a new producer message is one `Struct` plus one `PROTO_VOCABULARY` row the gate proves on the next boot; a new field on an existing message is one slot with its proto3 zero default; a new sibling-consumer field floor is one registry row pair, never a sibling vocabulary.
- Boundary: shapes and the registry only — no codec, span, retry, or transcode body (`transport/wire#PROTO_TRANSCODE`), no causal lift (`clock#CLOCK`), no trailer pack/unpack (`transport/serve#SERVE`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime
from typing import Annotated

import msgspec
from msgspec import Meta, Struct

# --- [TYPES] ----------------------------------------------------------------------------

# proto3 JSON emits 64-bit fields as decimal strings; the msgspec C core rejects any integer bound past int64 at codec/convert
# build, so only the ge=0 floor rides the slot — the <2**64 ceiling is the clock#CLOCK owner's railed decode check.
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
    case_: str = msgspec.field(name="case", default="")  # the bare name is claimed by the expression case-DSL corpus-wide
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

- Owner: `PROTO_VOCABULARY` binds each canonical shape to its generated message; the nested envelope shapes are registered rows because `_drift` walks only a registered row's top-level fields — an unregistered nested message drifts field-silently while its parent row still passes. `transport/wire#PROTO_TRANSCODE` builds its codec instances from the rows and `transport/serve#SERVE` resolves servicer codec pairs by name; the daemon composition root runs `aligned` once before serve binds.
- Auto: the codegen contract is static and build-time — `grpc_tools.protoc.main(["-I<proto-root>", "--python_out=<pkg>", "--pyi_out=<pkg>", "rasm/channels.proto"])` mints `rasm.runtime._pb2.channels_pb2` from the C#-owned `.proto`, with `command.build_package_protos(package_root, strict_mode=True)` the CI form failing on the first compile error. The `sys.meta_path` dynamic-stub path backing `grpc.protos(...)` is sealed off by `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS=1` — the runtime imports the generated module, never generates at import time.
- Packages: `protobuf`, `msgspec.inspect`, `grpcio-tools`, and `expression` per the fence imports.
- Growth: a new wire pair is one `PROTO_VOCABULARY` row the gate proves and wire transcodes; a new structural assertion is one arm in `_drift`, never a second gate; a new sibling consumer binds existing rows by symbol.
- Boundary: the gate proves structure, not values — byte-level round-trip parity is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, and the additive-drift admission decision is the C# `ContractGuard.AdditiveOnly` classifier's. An extra compiled field the struct lacks IS flagged here: the companion dropping a producer field is a decode gap even when the contract admits the addition.

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
    # importing channels_pb2 registered its serialized file in the default pool — this lookup is a read, never AddSerializedFile.
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
