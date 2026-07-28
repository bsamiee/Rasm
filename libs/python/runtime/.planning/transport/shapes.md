# [PY_RUNTIME_SHAPES]

Python's mint of the suite wire vocabulary: every canonical `msgspec.Struct` the transport carries is minted HERE, field-for-field against the corpus-homed `.proto` the `DESCRIPTOR_DRIFT` contract defines. Single-mint law: this page is the branch's one wire-shape owner and no sibling authors a second — the `.proto` sources, the HLC layout, and the `FaultDetail` schema are contract facts every branch mints against, `csharp:Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` the peer mint of the same table. Additive drift is tolerated by construction because cross-language evolution rides the contract's additive-only rule, which this branch's own gate applies exactly as its peers apply theirs, never a Python-side `forbid_unknown_fields` that breaks the additive contract.

`PROTO_VOCABULARY` binds each shape to its generated `channels_pb2` message, and `aligned` is the descriptor-pool drift gate proving compiled descriptors match the rows — field names and 64-bit decode floors — before the first RPC; the registry lives one tier below `transport/wire#PROTO_TRANSCODE` because a wire-side registry forces a `shapes -> wire` back-edge from the gate. Canonical-bytes custody is recorded here and re-minted nowhere in this folder: the length- and count-framed content-key byte stream the `evidence/identity#IDENTITY` key runs over is the payload-agnostic `CANONICAL_BYTE_IDENTITY` contract layout, each branch writing it from its own canonical writer with parity proving at the corpus; the CRDT op-log msgpack arrays are self-delimiting, the amendment governing content keys, not wire framing.

## [01]-[INDEX]

- [02]-[VOCABULARY]: wire slot types, the `GeometryPayload` envelope family, the canonical service shapes, and the tessellation pair.
- [03]-[REGISTRY_AND_DRIFT]: `PROTO_VOCABULARY` seed table, the static-codegen contract, and the `aligned` drift gate.

## [02]-[VOCABULARY]

- Owner: field names are the producer's snake_case proto names verbatim — `MessageToDict(preserving_proto_field_name=True)` keys the mapping by them — so no `rename=` layer exists and the struct declaration IS the wire contract. Producers whose own serializers spell a second casing change nothing here — the C# appearance leg emits Web camelCase off its PascalCase members, and that casing and this one are both mechanical projections of the ONE snake_case proto name, so a camelCase slot on these structs decodes nothing.
- Cases: every scalar slot carries its proto3 zero default because `MessageToDict` omits a field at its default value — a default-less slot rejects the producer's legitimate zero, so presence is the proto3 no-presence contract, never a required-field re-mint. Nested message slots spell `T | None = None` — proto message absence is a real wire value, the one place `None` crosses inward, collapsed by the consuming owner at its seam. Explicit presence rides every scalar the producer declares `optional`, spelling `T | None = None` for that reason alone: the field distinguishes unmeasured from measured-zero, so folding it onto the zero default reads a measurement no producer took — `TextureSetWire`'s press divergence is the standing instance, and a scalar without the producer's `optional` keyword never takes this shape. `SolveRequest` deliberately carries bare column-major `float64` bytes, never a tensor envelope, per the producer's no-geometry-envelope law.
- Auto: no shape lifts the causal halves to `Hlc` — the `clock/clock#CLOCK` owner reconstructs causal cells at full 100-ns tick fidelity from the carrier slots, and a `datetime`-mediated lift here truncates to microseconds. `Packed` types the two producer-open envelopes, open within the additive-only contract by the producer's own design and never widened past the declared slots. `TessellationRequest`/`TessellationReceipt` are contract rows this registry mints and streams over the existing `artifact_frame` leg — geometry `mesh/serve` binds the field floor by symbol, minting no wire shape.
- Law: PRODUCER DIRECTION is a per-family fact this page records and never averages. `MaterialWire`, `OpenPbrGroupsWire`, `AppearanceSummaryWire`, and the `WireColor`/`WireProvenance` leaves form the appearance family, DECODE-ONLY: `csharp:Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` is the sole producer of the OpenPBR parameter algebra, the conductor key, the capture receipt, and the appearance content hash, so these structs mirror that projection field-for-field and a python-side lowering, conductor table, or key derivation is the named cross-language drift defect. `TextureSetWire` with its `ChannelWire`/`PackWire`/`PressReceiptWire` leaves forms the baked-set family, DECODE-ONLY on the same authority, `csharp:Rasm.Materials/Raster/set#TEXTURE_SET` pressing the plane bytes and keying the set, and it REUSES `WireProvenance` rather than minting a second capture receipt. `AssetSetManifest` and its `MapEntry`/`PackEntry`/`IblEntry` leaves run the other way — `artifacts/graphic/texture/set#TEXTURE_SET` FILLS them behind its merkle set key and the peers decode. Both set documents stand DISTINCT and neither name covers the other: the C# one hangs behind an `AppearanceKey` and carries a press receipt, the python one carries a `kind` discriminant and an IBL entry, and one channel roster, transfer vocabulary, normal convention, and pack order serve both — two producers under two names sharing one frozen vocabulary. Their hex spellings also diverge and neither derives from the other: the C# document carries its content addresses UPPERCASE and the python one lowercase, so a consumer joining a key across the pair lowers, never uppercases. Every family spells repeated slots `list[...]` because the frozen cross-branch fragment declares them so and the producer constructs them so; `convert` coerces either container inward, so the declaration follows the fragment rather than re-spelling it, and the interior owners that consume these documents hold their own immutable collections.
- Receipt: `FaultDetail` is the typed conflict the whole suite converges on, riding the `TransactionReceipt.conflict` slot in band and the `grpc-status-details-bin` trailer out of band; `transport/serve#SERVE` owns the Python trailer egress and ingress — this page owns only the shape.
- Packages: `msgspec`, `protobuf`, and the faults rail per the fence imports; `transport/wire#PROTO_TRANSCODE` runs the `convert(strict=False)` decimal-string coercion leg.
- Growth: a new producer message is one `Struct` with one `PROTO_VOCABULARY` row the gate proves on the next boot; a new field on an existing message is one slot with its proto3 zero default; a new sibling-consumer field floor is one registry row pair, never a sibling vocabulary. Every peer-produced DOCUMENT lands as its whole struct family — each nested envelope carries its own row, because the gate walks a registered row's top-level fields alone and an unregistered leaf drifts field-silently while its parent still passes.
- Boundary: shapes and the registry only — no codec, span, retry, or transcode body (`transport/wire#PROTO_TRANSCODE`), no causal lift (`clock/clock#CLOCK`), no trailer pack/unpack (`transport/serve#SERVE`).

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


class SupportBundleRequest(Struct, frozen=True):
    # diagnostic-capsule pull: an empty roster selects every collector, a named roster bounds the capture to the rows.
    collectors: tuple[str, ...] = ()


class SupportBundleReply(Struct, frozen=True):
    # Capsule bytes cross as one zstd-compressed archive beside their content key; rosters witness what a capture
    # collected and what its gates or fences skipped, so an operator reads coverage without opening the archive.
    content_key: str = ""
    archive: bytes = b""
    collected: tuple[str, ...] = ()
    skipped: tuple[str, ...] = ()


class WireColor(Struct, frozen=True, gc=False):
    # scene-linear triple beside the clipped hex a web swatch reads; the producer derives both, this branch neither
    r: float = 0.0
    g: float = 0.0
    b: float = 0.0
    hex: str = ""


class WireProvenance(Struct, frozen=True, gc=False):
    # Mirrors the producer's capture receipt. `fit_condition_number` carries a native non-finite double on this leg —
    # its named "Infinity" literal is the producer's JSON spelling, so a decode branch for it here reads a value proto
    # never writes.
    device: str = ""
    wavelength_count: int = 0
    fit_residual: float = 0.0
    measured: bool = False
    method: str = ""
    angular_samples: int = 0
    fit_condition_number: float = 0.0
    fit_rank: int = 0
    dominant_wavelength_nm: float = 0.0
    excitation_purity: float = 0.0
    cct_kelvin: float = 0.0
    cct_duv: float = 0.0
    # Neural-acquisition tail the producer stamps on an inferred row: `model_card` names the registry row and
    # `license` its grant class, so a redistribution check reads the receipt. Dropping this pair decodes an
    # unlicensed product as an unmarked one, which is the one provenance loss no downstream surface recovers.
    model_card: str = ""
    license: str = ""


class AppearanceSummaryWire(Struct, frozen=True, gc=False):
    # Carries the NEUTRAL seam shape behind the appearance content key: consumers read these seven scalars flat and
    # never the lobe graph. `transmissive` is the refractive flag, DISTINCT from `opacity`.
    appearance_key: str = ""
    base_color_r: float = 0.0
    base_color_g: float = 0.0
    base_color_b: float = 0.0
    metallic: float = 0.0
    roughness: float = 0.0
    opacity: float = 0.0
    transmissive: bool = False


class OpenPbrGroupsWire(Struct, frozen=True):
    base_weight: float = 0.0
    base_color: WireColor | None = None
    base_metalness: float = 0.0
    base_diffuse_roughness: float = 0.0
    base_specular_tint: float = 0.0
    specular_weight: float = 0.0
    specular_color: WireColor | None = None
    specular_roughness: float = 0.0
    specular_ior: float = 0.0
    specular_anisotropy: float = 0.0
    transmission_weight: float = 0.0
    transmission_roughness: float = 0.0
    subsurface_weight: float = 0.0
    subsurface_radius_r: float = 0.0  # the producer's three-band carrier FLATTENS per channel; a triple here re-mints its shape
    subsurface_radius_g: float = 0.0
    subsurface_radius_b: float = 0.0
    coat_weight: float = 0.0
    coat_color: WireColor | None = None
    coat_roughness: float = 0.0
    coat_ior: float = 0.0
    fuzz_weight: float = 0.0
    fuzz_color: WireColor | None = None
    fuzz_roughness: float = 0.0
    thin_film_weight: float = 0.0
    thin_film_thickness: float = 0.0
    thin_film_ior: float = 0.0
    emission_color: WireColor | None = None
    emission_luminance: float = 0.0
    geometry_opacity: float = 0.0


class MaterialWire(Struct, frozen=True):
    # Carries the FULL parameter payload behind the appearance key; `conductor` is empty for a dielectric
    id: str = ""
    open_pbr: OpenPbrGroupsWire | None = None
    conductor: str = ""
    provenance: WireProvenance | None = None
    preview: WireColor | None = None


class ChannelWire(Struct, frozen=True, gc=False):
    role: str = ""  # a canonical channel name; an unknown key is the producer's own decode refusal, never a widened slot
    transfer: str = ""  # a display transfer on a channel plane refuses: a bake target is scene-referred
    format: str = ""  # the storage-texel key verbatim, where the python document records the same row as a (depth, channels) pair
    channels: int = 0  # the SEMANTIC component count; storage width rounds it up through {1, 2, 4}
    alpha_mode: str = ""
    mips: int = 0
    mip_policy: str = ""
    block_format: str = ""
    ktx_payload: str = ""  # a raw block payload no basis transcoder reads refuses here: the viewer this set feeds cannot open it
    blob: str = ""  # the plane's content address in the write-once object store, UPPERCASE hex
    file: str = ""
    byte_length: WireU64 = 0


class PackWire(Struct, frozen=True):
    pack: str = ""  # the packing order names the slot order; a wire list of slots would be a second truth
    present: list[bool] = msgspec.field(default_factory=list)
    format: str = ""
    mips: int = 0
    blob: str = ""
    file: str = ""
    byte_length: WireU64 = 0


class PressReceiptWire(Struct, frozen=True, gc=False):
    backend: str = ""  # persisted bytes are always CPU-minted, so an accelerator value here is the producer's decode refusal
    plan_key: str = ""
    graph_key: str = ""
    seed: WireU64 = 0  # replays the per-texel jitter; the plan and graph keys alone do not reproduce the bytes
    texels: WireU64 = 0
    elapsed_ms: float = 0.0
    # Producer declares this `optional` because a single-lane press MEASURES no divergence and a zero there reads
    # to a parity gate as a perfect match. Telemetry alone — the content key never folds it.
    gpu_delta_max: float | None = None


class TextureSetWire(Struct, frozen=True):
    # Hangs the appearance-coupled BAKED set BEHIND the seven-value appearance preimage as a payload field: a set
    # column on the summary forks the peer dedup key and re-ids every node carrying an appearance. Carries no
    # kind slot because its one kind IS the baked set — an environment product rides the python document instead.
    appearance_key: str = ""
    set_key: str = ""
    material_id: str = ""
    conductor: str = ""  # empty for a dielectric; the set-level metal fact no per-texel plane carries
    width: int = 0
    height: int = 0
    layers: int = 0
    layer_law: str = ""  # cube faces, arrays, volumes, and flipbooks are rows here, never a second document shape
    normal_convention: str = ""  # the INGEST-source record; the plane bytes always carry the canonical convention
    alpha_mode: str = ""
    height_scale: float = 0.0  # the millimetre span the normalized height plane resolves against; never baked into the plane
    tiled: bool = False
    udim_tiles: list[int] = msgspec.field(default_factory=list)
    channels: list[ChannelWire] = msgspec.field(default_factory=list)  # roster-ordered, and that order IS the set-key preimage
    packs: list[PackWire] = msgspec.field(default_factory=list)  # a channel inside a pack carries no standalone row
    provenance: WireProvenance | None = None  # the appearance family's own capture receipt, never a second shape
    press: PressReceiptWire | None = None  # absent for an ingested set


class MapEntry(Struct, frozen=True, gc=False):
    role: str = ""
    file: str = ""
    digest: str = ""
    color_space: str = ""
    depth: str = ""
    format: str = ""  # the container the file holds, never a storage-texel key
    channels: int = 0  # the SEMANTIC component count; storage width rounds it up through {1, 2, 4}
    mips: int = 0
    ktx_payload: str = ""
    byte_length: WireU64 = 0


class PackEntry(Struct, frozen=True):
    pack: str = ""  # the packing order names the slot order; a wire list of slots would be a second truth
    present: list[bool] = msgspec.field(default_factory=list)
    format: str = ""
    mips: int = 0
    digest: str = ""
    file: str = ""
    byte_length: WireU64 = 0


class IblEntry(Struct, frozen=True):
    sh9: list[float] = msgspec.field(default_factory=list)  # EXACTLY 27, band-major with RGB interleaved at i*3+c
    equirect_file: str = ""
    equirect_digest: str = ""
    specular_files: list[str] = msgspec.field(default_factory=list)
    roughness_per_mip: list[float] = msgspec.field(default_factory=list)
    brdf_lut_file: str = ""
    brdf_lut_digest: str = ""
    luminance_cdf_file: str = ""
    intensity: float = 0.0  # applied ON READ; a producer baking it into the planes forks every consumer sharing the digest
    up_axis: str = ""  # FROZEN to the one shading frame; a Y-up runtime remaps its direction basis at the read and never the bands


class AssetSetManifest(Struct, frozen=True):
    manifest_key: str = ""  # lowercase hex, where the C# set document spells its own keys uppercase
    kind: str = ""  # the baked-set / environment discriminant the C# document has no room for, which is why two documents exist
    source: str = ""  # the ingest root or generator id; an absolute host path here leaks the producing machine onto the wire
    width: int = 0
    height: int = 0
    normal_convention: str = ""  # the INGEST-source record; the plane bytes always carry the canonical convention
    alpha_mode: str = ""
    udim: str = ""
    udim_tiles: list[int] = msgspec.field(default_factory=list)
    tiled: bool = False
    maps: list[MapEntry] = msgspec.field(default_factory=list)
    packs: list[PackEntry] = msgspec.field(default_factory=list)
    ibl: IblEntry | None = None
    unresolved: list[str] = msgspec.field(default_factory=list)
    tool: str = ""
    tool_version: str = ""
    license_class: str = ""
```

## [03]-[REGISTRY_AND_DRIFT]

- Owner: `PROTO_VOCABULARY` binds each canonical shape to its generated message; the nested envelope shapes are registered rows because `_drift` walks only a registered row's top-level fields — an unregistered nested message drifts field-silently while its parent row still passes. `transport/wire#PROTO_TRANSCODE` builds its codec instances from the rows and `transport/serve#SERVE` resolves servicer codec pairs by name; the daemon composition root runs `aligned` once before serve binds.
- Auto: the codegen contract is static and build-time — `grpc_tools.protoc.main(["-I<proto-root>", "--python_out=<pkg>", "--pyi_out=<pkg>", "rasm/channels.proto"])` mints `rasm.runtime._pb2.channels_pb2` from the corpus-homed `.proto`, with `command.build_package_protos(package_root, strict_mode=True)` the CI form failing on the first compile error. `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS=1` seals off the `sys.meta_path` dynamic-stub path backing `grpc.protos(...)` — the runtime imports the generated module, never generates at import time.
- Packages: `protobuf`, `msgspec.inspect`, `grpcio-tools`, and `expression` per the fence imports.
- Growth: a new wire pair is one `PROTO_VOCABULARY` row the gate proves and wire transcodes; a new structural assertion is one arm in `_drift`, never a second gate; a new sibling consumer binds existing rows by symbol.
- Boundary: the gate proves structure, not values — byte-level round-trip parity is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, and the additive-drift admission rule is the contract's, applied by this branch's own gate rather than read off a peer's classifier. Extra compiled fields the struct lacks stay flagged here: this branch dropping a contract field is a decode gap even when the contract admits the addition.

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
    ("support_bundle", SupportBundleRequest, channels_pb2.SupportBundleRequest),
    ("support_bundle_reply", SupportBundleReply, channels_pb2.SupportBundleReply),
    ("material_wire", MaterialWire, channels_pb2.MaterialWire),
    ("open_pbr_groups_wire", OpenPbrGroupsWire, channels_pb2.OpenPbrGroupsWire),
    ("appearance_summary_wire", AppearanceSummaryWire, channels_pb2.AppearanceSummaryWire),
    ("wire_color", WireColor, channels_pb2.WireColor),
    ("wire_provenance", WireProvenance, channels_pb2.WireProvenance),
    ("texture_set_wire", TextureSetWire, channels_pb2.TextureSetWire),
    ("channel_wire", ChannelWire, channels_pb2.ChannelWire),
    ("pack_wire", PackWire, channels_pb2.PackWire),
    ("press_receipt_wire", PressReceiptWire, channels_pb2.PressReceiptWire),
    ("asset_set_manifest", AssetSetManifest, channels_pb2.AssetSetManifest),
    ("map_entry", MapEntry, channels_pb2.MapEntry),
    ("pack_entry", PackEntry, channels_pb2.PackEntry),
    ("ibl_entry", IblEntry, channels_pb2.IblEntry),
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
