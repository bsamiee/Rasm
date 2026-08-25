# [PY_GEOMETRY_SCAN_INGESTION]

`ScanIngestion` fronts the host-free scan plane registration-ready — the raw-scan cleaning the `data` branch declines — and owns the reality-capture companion decode the C# residency lane consumes. One frozen owner discriminates over a `ScanOp` `@tagged_union` whose cases carry their own codec: `arrow_las` holds the content-keyed `PointRecordTable` from `data/spatial/mesh#POINTCLOUD` (LAS/LAZ/COPC already decoded, its CRS and point format already parsed), `e57` holds the `pye57` structured multi-scan read that bridge does not own, `e57_write` is the inverse leg on the same surface because the provider lives HERE and no data-branch owner holds an E57 codec, and `splat` decodes a `realitycapture` SPZ/SOG container into the gaussian-splat carrier. Every product leaves as one closed `IngestProduct` case beside one `IngestReceipt`. This owner mints the scan plane's sealed cloud crossing: a live `open3d` point cloud is a pybind11 handle no pickler carries, so clouds cross every worker seam as bare `positions`/`colors`/`normals` arrays on the frozen `Cloud` carrier, which also owns its own content digest so a consumer keys a cloud without re-spelling one. Graph shape is policy, not code — an `IngestStage` row sequence folded over the `pdal` `|` pipe, so ground classification, outlier removal, downsampling, and range cropping order and membership are `IngestPolicy` rows a rebuild reorders without touching the fold; a block-scale cloud rides the streaming arm when `IngestPolicy.stream_chunk` is non-zero.

Provider presence is PROVED, never assumed: `pye57` and `open3d` both carry interpreter markers, so `_UNREACHED` resolves their absence once at import through `find_spec` and every arm that needs one refuses by name through the folder roster's own `INGEST_UNREACHED` row naming the missing module, where an ungated arm dies as a bare `ModuleNotFoundError` inside an offloaded worker and reaches the caller as a worker-crash rail. `run` is `async`, keeping the multi-second SMRF/voxel sweep off the event loop: it composes the graduation `evidence_run` weave (span + fence + receipt harvest, `EvidenceScope.SCAN_INGESTION` the seed, the owner's composition `ScopeKey` the custody stamp) around the `lane.offload` crossing on `Kernel.of(_ingest_kernel, KernelTrait.HOSTILE)` — the `pdal`/`pye57`/`open3d` band holds process-global native state and imports under no isolated subinterpreter — and the stage graph builds worker-side inside the kernel, so no `pdal` object meets the pickle seam. A cleaned `Cloud` is the precondition `scan/registration#REGISTRATION` consumes across a same-folder read-only seam.

## [01]-[INDEX]

- [02]-[INGESTION]: verb-discriminated scan IO — the `ScanOp` intake folded through one policy-ordered `pdal` `|` filter graph to the registration-ready `Cloud`, the E57 egress leg, and the gaussian-splat companion decode, offloaded to the warm process pool under the graduation weave.

## [02]-[INGESTION]

- Owner: `ScanIngestion.run`, the frozen dispatch entry carrying the composition `ScopeKey` its weave stamps and the shared `ArtifactTransfer` its splat egress publishes through; `ScanOp`'s tag IS the codec-carrying discriminant across BOTH directions — read and write ride one surface because the domain admits the inverse and a sibling `write_scan` entry would fork the provider gate, the receipt, and the weave. `Cloud` mints HERE as the scan plane's sealed cloud crossing — bare ndarray fields, the `tensor()`/`legacy()` rebuild pair, and its own `digest` content key — and `scan/registration`, `scan/deviation`, `scan/reconstruction` import it downward, never a per-page carrier twin and never a per-page cloud hash. It carries no rigid re-pose: every correspondence-search arm publishes its own initial-transform argument, and the one EM arm that publishes none pre-poses at its own admission, so a carrier-level pre-pose would pay a whole-cloud copy per solve for a seed the consuming arm already owns. The splat arm constructs generated `GaussianSplatScan` directly, validates its descriptor, and publishes those exact protobuf octets; no Python carrier or semantic content key repeats the artifact coordinate.
- Cases: `ScanOp` arms `arrow_las` (the data-branch `PointRecordTable` carrier — table, point count, point format, CRS WKT, and content key together), `e57` (the `pye57` structured multi-scan source read per-scan with acquisition pose applied and `ScanHeader` provenance harvested), `e57_write` (the `write_scan_raw` append leg, pose from the supplied rotation/translation), and `splat` (raw SPZ/SOG container bytes the signature-dispatched `_container` reader opens worker-side and the companion decodes), matched by `match`/`assert_never`. `IngestProduct` closes the outward outcome — `cloud`, the splat body's exact generated `ArtifactRef`, or the written `ContentKey` — never an erased `object` a consumer re-discriminates. The worker alone may return the validated generated scan before the parent publishes it; that temporary body never crosses the public result. `IngestStage` rows — `GROUND_CLASSIFY` (`SMRF` default / `PMF` alternate), `OUTLIER_REMOVE`, `DECIMATE` (`DECIMATION` / `VOXELDOWNSIZE`), `RANGE_CROP` — each build their `pdal.Filter` through one `_STAGE` row, the swappable rows dispatching to the policy-chosen `_FILTER` factory, so a stage's driver and option dict are one row read; `FARTHEST_POINT` is the carrier-fold row on `_CARRIER`, bounding a point budget geometry-uniformly where every `pdal` decimator bounds by file order or by cell occupancy.
- Law: a splat container's `format` — the generated `SplatFormat` member — is LEGAL only where the runtime `transport/shapes#VOCABULARY` `SPLAT_FORMS` matrix grounds it, and the harmonic band ceiling and alpha activation the shape gate proves are that admitted row's own DECLARED grounding — a ceiling transcribed here would let a published release raising its band pass one end of the wire and fail the other, and a refused release would decode as garbage under a gate that cannot see the encoding it declines.
- Law: a below-floor provider refuses BY NAME — `_UNREACHED` derives once at import from `find_spec` over every marked module this page can reach, and `_reached` gates all THREE axes a provider enters on, the verb's own codec through `_OP_MODULE`, the container a byte-fed verb carries through `_FORMAT_MODULE`, and every rostered native stage through `_STAGE_MODULE`, onto `Error(INGEST_UNREACHED.raised(module))` before the offload — the row's own `import_` arm and derived subject, where the retired literal handed a BARE string to a two-slot case; so an unprovisioned host reads a provisioning refusal naming `pye57`, `open3d`, or `PIL` rather than a worker-death rail carrying a private module path, and a policy naming a native stage refuses at the same seam its verb would instead of surviving the gate and dying inside the graph. The container axis is its own row set rather than a verb row because `splat` spans two formats with different demands — an SPZ payload decodes on the interpreter floor alone — so the gate resolves the format off the payload's leading signature through the same `_SIGNATURE` table `_container` dispatches on, and a verb-level image row would refuse a decode the host can serve. The refusal names the IMPORT module, so a host reading `PIL` provisions the `pillow` distribution. The gate is capability presence, never an offload route, since a process-pool worker shares the one venv.
- Law: a cleaning stage is a PIPE row or a CARRIER row and the two tables partition the vocabulary — `_STAGE` builds the `pdal` filters the `|` graph composes, `_CARRIER` holds the folds no driver expresses, and the pipe runs whole before the carrier folds run in policy order. The split is structural rather than a preference: a `pdal` stage consumes and yields a structured array inside one pipeline, so a carrier fold between two of them would break the pipe into two executes and pay a second full pass. That is why `FARTHEST_POINT` is a carrier row — `filters.decimation` keeps every Nth point in FILE order and `filters.voxeldownsize` one point per occupied cell, so neither bounds a point BUDGET while preserving shape, which is the exact guarantee a non-rigid solve needs. `output_points` therefore reads the CARRIER's census rather than the pipeline array's, since a fold that dropped points would otherwise report a decimation it never made.
- Law: the crossing carrier is admitted WHOLE — the point count, point format, CRS posture, and content key the data owner already parsed all cross on `PointRecordTable`, so `srs` rides a `Posture` whose DECLARED arm is the executed pipeline's own `srswkt2` and whose DEFAULTED arm quotes the carrier's `crs` beside the source that supplied it, `point_format` joins the fact census, and `source_key` chains the cleaning provenance back to the decode; taking `.table` alone would discard four facts and then re-derive an absent CRS from an in-memory array that never carried one, and fusing the two arms into one string would let a consumer read an inherited reference as one this pass measured.
- Law: the splat container admits on its OWN leading signature and its own declared bounds — one `_SIGNATURE` row per magic names the format and the reader together, so the floor gate and the decode read one owner and can never disagree about what a payload is. The SPZ arms open the NGSP v4 preamble-and-TOC read or the legacy gzip body, decompress through stdlib `compression.zstd`/`gzip`, and enforce the packer's version window, `INT32_MAX` point cap, ratio bound, and per-stream size declarations as typed `malformed` refusals against a header whose `fractional_bits` owns the position scale; a legacy version whose encodings the `_LAYOUT` roster does not declare (v1 float16 positions, v1/v2 first-three rotations) refuses by version name rather than decoding garbage. The SOG arm opens the bundled `.sog` — a ZIP whose members sit at the archive root — reads it whole through stdlib `zipfile` so no handle outlives its window, decodes `meta.json` VERSION-first (v1 writes no version key and stores per-property affine mins/maxs where v2 stores codebooks, so it refuses by version rather than through this reader's rows), and admits the count and band declarations before any plane is touched. The raw-bytes ingress reaches the bundled form alone; a loose plane directory is a filesystem source no byte payload carries.
- Law: `_channels` and `_planes` are SIBLING folds under one `SplatBody` case each, never one table stretched over both — SOG breaks the packed-block model in five structural ways: no concatenated body, codebook indirection where the layout row carries an affine gain, positions spanning two planes bit-combined, an AC band indirecting twice through a plane dimensioned by coefficient rather than by splat, and a smallest-three quaternion whose omitted-component index rides an alpha byte rather than a packed 2-bit field. Reusing the SPZ quaternion arm rotates every splat whose omitted slot differs, which no shape gate can see. Both folds yield one canonical channel dict, so the DC head, the shape gate, and the framed key stay one owner above every container.
- Law: SOG planes are LOSSLESS WebP by the container's own encoder contract, and that contract is an ADMISSION ASSUMPTION rather than a decodable fact — the decoder exposes no lossless flag, so a lossy plane decodes silently into wrong codebook indices and quantized codes that every gate here passes. What the reader does admit is per-MEMBER: absence, an unrecognized or truncated payload, a declared pixel count past the decoder's own bomb ceiling, fewer bands than the plane's row declares, a pixel census short of the splat count, a codebook off the 8-bit index space, and a centroid label or grid extent outside the manifest's own declaration, each refusing by member name so an operator reads which plane broke rather than which wire channel noticed.
- Law: an unmeasured ratio is absent, never `1.0` — an empty input cloud measures no decimation, so the slot is `None` and the fact projection omits it, where a fabricated `1.0` publishes "nothing was removed" over a pass that removed nothing because there was nothing to remove.
- Receipt: `IngestReceipt.of` derives the decimation ratio from counts and integer-narrows at one factory; `facts` emits the native slots and tuple axes once, omitting every absent measure; `_emit` carries the `@receipted(OPEN)` aspect. Ingestion mints no graduation subject — a cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, so no `scan-ingestion` member sits on the `rasm.geometry.graduation` `GeometrySubject` union.
- Packages: `pdal` (the injected `Filter.smrf`/`.pmf`/`.outlier`/`.decimation`/`.voxeldownsize`/`.range` factories, the `|` pipe composition, the `execute()`/`iterator(chunk_size)` runs, and the `srswkt2`/`schema` output metadata), `pye57` (`E57` context-manager open, `read_scan(transform=True)` the conditioned global-frame intake, `get_header` the typed `ScanHeader`, `write_scan_raw` the one append entry), `open3d` (touched only by the `Cloud.tensor()`/`legacy()` rebuild projections a consumer calls on its own native floor), `pillow` (`Image.open` over the archive member's octets under the `formats` plugin pin, `getbands`/`convert` the band admission and narrowing, and the `__array_interface__` copy `np.asarray` reads — the SOG plane decode alone, never an ingestion-side raster op), `pyarrow` (the carrier's columns), `numpy` (structured-array assembly over the shared `_DTYPE`, the splat channel unpack, the TOC read, and the codebook/centroid fancy indexing), stdlib `compression.zstd` + `gzip` + `zipfile` + `io.BytesIO` (SPZ stream decompression and the SOG archive read — interpreter-floor surfaces, never a `zstandard` distribution), `expression` (`Block`/`Map`/`Option` folds), `beartype` (the `_structured` fence), `msgspec` (frozen carriers and the one `json.Decoder` pair the SOG manifest admits through), the geometry `evidence_run`/`EvidenceScope` weave, and the runtime `RuntimeRail`/`LanePolicy.offload`/`Kernel`/`ContentIdentity`/`Receipt`/`receipted` rails. `laspy` is consumed transitively through the data-branch carrier, never imported here; every compiled band is a module-scope `lazy import` behind its floor gate.
- Growth: a new cleaning stage is one `IngestStage` member and one row on `_STAGE` or `_CARRIER` by which regime expresses it (and one `_FILTER` row when a new driver backs a pipe stage, one `_STAGE_MODULE` row when a carrier fold is native); a new driver alternative on a swappable stage is one `IngestFilter` member and one `_FILTER` row and the policy default; a new scan verb is one `ScanOp` case, one `_dispatch` arm, and — where it needs a marked provider — one `_OP_MODULE` row; a new splat container is one `SplatFormat` member, one `_SIGNATURE` row, and then whichever regime expresses it — a `_LAYOUT` row when the body is a packed-block roster the offset fold indexes, or one `SplatBody` case with its own `_splatted` arm and decode fold when the container's encoding breaks that model, and a container whose planes need a marked provider adds one `_FORMAT_MODULE` row; a new packed ENCODING inside a `_LAYOUT` row is one `ChannelCodec` member and one arm; a packed block the wire record carries no field for is one `Nothing`-channelled row; a new output-metadata fact is one `facts` slot read off the executed pipeline.
- Boundary: the inbound LAS/LAZ/COPC decode and the `PointRecordTable` mint are `data/spatial/mesh#POINTCLOUD`'s (`laspy` full decode and the COPC octree subset live there), so ingestion never re-reads LAS nor crosses a `pdal` `Pipeline` at the data seam; the E57 path is ingestion's in BOTH directions because `pye57` is absent from the data branch and no data owner holds an E57 codec, so declining the write leg to that seam would leave E57 egress unowned in the whole branch. The corpus owns `GaussianSplatScan`; this producer serializes that generated message once, passes the exact octets to `ArtifactTransfer.put`, and returns the transfer's generated `ArtifactRef` unchanged. The contracts SDK owns staging, SHA-256, extent, framing, receipt equality, and cleanup; a parallel `ContentKey` for the same body is deleted. Registration is `scan/registration#REGISTRATION`'s; ingestion never registers, deviates, reconstructs, tessellates, mints storage paths, or mutates a Rhino/GH document.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import gzip
import struct
import zipfile
from collections.abc import Callable
from compression import zstd
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from io import BytesIO
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import DecodeError, Struct, ValidationError, field
from msgspec.json import Decoder
from protovalidate import CompilationError, EvaluationError, ValidationError as ContractValidationError, validate

from rasm.runtime.transport.artifact import ArtifactTransfer
from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef
from rasm.contracts.rasm.contracts.scan.gaussian_pb import GaussianSplatScan, SplatFormat
from rasm.geometry.graduation import EvidenceScope, GeometryLeg, evidence_run
from rasm.runtime.faults import FAULT_CONF, TERMINAL, FaultRow, Posture, RuntimeRail, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, receipted
from rasm.runtime.shapes import SplatGrounding, splat_form
from rasm.runtime.workers import Kernel, KernelTrait

lazy from PIL import Image, UnidentifiedImageError
lazy import open3d as o3d
lazy import pdal
lazy import pye57

if TYPE_CHECKING:
    from rasm.data.spatial.mesh import PointRecordTable

# --- [TYPES] ----------------------------------------------------------------------------


class OpKind(StrEnum):
    ARROW_LAS = "arrow_las"
    E57 = "e57"
    E57_WRITE = "e57_write"
    SPLAT = "splat"


class IngestStage(StrEnum):
    GROUND_CLASSIFY = "ground-classify"
    OUTLIER_REMOVE = "outlier-remove"
    DECIMATE = "decimate"
    RANGE_CROP = "range-crop"
    FARTHEST_POINT = "farthest-point"


class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


class SplatChannel(StrEnum):
    POSITION = "positions"
    SCALE = "scales"
    ROTATION = "rotations"
    HARMONIC = "harmonics"
    ALPHA = "alphas"
    COLOR = "colors"


type Pose = tuple[float, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------


_DTYPE: Final = np.dtype([(axis, np.float64) for axis in ("X", "Y", "Z")])
_COLOR_DTYPE: Final = np.dtype([*_DTYPE.descr, *((band, np.uint16) for band in ("Red", "Green", "Blue"))])

_CLOUD_MODULE: Final[str] = "open3d"
_IMAGE_MODULE: Final[str] = "PIL"
_UNREACHED: Final[frozenset[str]] = frozenset(m for m in ("pye57", _CLOUD_MODULE, _IMAGE_MODULE) if find_spec(m) is None)

_OP_MODULE: Final[Map[OpKind, str]] = Map.of_seq([(OpKind.E57, "pye57"), (OpKind.E57_WRITE, "pye57")])
_FORMAT_MODULE: Final[Map[SplatFormat, str]] = Map.of_seq([(SplatFormat.SOG_V2, _IMAGE_MODULE)])
_STAGE_MODULE: Final[Map[IngestStage, str]] = Map.of_seq([(IngestStage.FARTHEST_POINT, _CLOUD_MODULE)])

_INHERITED_CRS: Final[str] = "PointRecordTable.crs"

INGEST_UNREACHED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.INGESTION, point="floor", arm="import_", defect="module-absent", retriability=TERMINAL, slots=("module",)
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([INGEST_UNREACHED]))

_NGSP_MAGIC: Final[bytes] = b"NGSP"
_NGSP_HEADER: Final[struct.Struct] = struct.Struct("<IIIBBBBI12x")
_LEGACY_HEADER: Final[struct.Struct] = struct.Struct("<IIIBBBB")
_ZSTD_VERSIONS: Final[range] = range(4, 5)
_LEGACY_VERSION: Final[int] = 3

_SOG_MANIFEST: Final[str] = "meta.json"
_SOG_VERSION: Final[int] = 2
_SOG_CODEBOOK: Final[int] = 256
_SOG_CENTROID_ROW: Final[int] = 64
_SOG_MAX_CENTROIDS: Final[int] = 65_536
_QUAT_MODE_BASE: Final[int] = 252
_WEBP: Final[tuple[str, ...]] = ("WEBP",)
_PLANE_MODE: Final[Map[int, str]] = Map.of_seq([(3, "RGB"), (4, "RGBA")])

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IngestFault(Exception):
    tag: Literal["unprovisioned", "malformed"] = tag()
    unprovisioned: str = case()
    malformed: str = case()

    def __str__(self) -> str:
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case IngestFault(tag="unprovisioned", unprovisioned=module):
                return module
            case IngestFault(tag="malformed", malformed=offending):
                return offending
            case _ as unreachable:
                assert_never(unreachable)


def _native() -> None:
    if _CLOUD_MODULE in _UNREACHED:
        raise IngestFault(unprovisioned=_CLOUD_MODULE)


# --- [MODELS] ---------------------------------------------------------------------------


class Cloud(Struct, frozen=True):
    positions: np.ndarray
    colors: np.ndarray = field(default_factory=lambda: np.empty((0, 3)))
    normals: np.ndarray = field(default_factory=lambda: np.empty((0, 3)))

    def __len__(self) -> int:
        return int(self.positions.shape[0])

    @property
    def digest(self) -> ContentKey:
        return ContentIdentity.key("pointcloud", self.positions.tobytes())

    def tensor(self) -> "o3d.t.geometry.PointCloud":
        _native()
        cloud = o3d.t.geometry.PointCloud()
        cloud.point.positions = o3d.core.Tensor(self.positions.astype(np.float32))
        if self.colors.size:
            cloud.point.colors = o3d.core.Tensor(self.colors.astype(np.float32))
        if self.normals.size:
            cloud.point.normals = o3d.core.Tensor(self.normals.astype(np.float32))
        return cloud

    def legacy(self) -> "o3d.geometry.PointCloud":
        _native()
        cloud = o3d.geometry.PointCloud()
        cloud.points = o3d.utility.Vector3dVector(self.positions)
        if self.colors.size:
            cloud.colors = o3d.utility.Vector3dVector(self.colors)
        if self.normals.size:
            cloud.normals = o3d.utility.Vector3dVector(self.normals)
        return cloud

    @classmethod
    def of_legacy(cls, cloud: "o3d.geometry.PointCloud") -> "Cloud":
        return cls(
            positions=np.asarray(cloud.points, dtype=np.float64),
            colors=np.asarray(cloud.colors, dtype=np.float64),
            normals=np.asarray(cloud.normals, dtype=np.float64),
        )


def _harmonic_width(degree: int) -> int:
    return (degree + 1) * (degree + 1) * 3


class ChannelCodec(StrEnum):
    AFFINE = "affine"
    FIXED24 = "fixed24"
    SMALLEST_THREE = "smallest-three"


class ChannelSpec(Struct, frozen=True, gc=False):
    channel: Option[SplatChannel]
    width: int
    dtype: str
    gain: float
    bias: float
    codec: ChannelCodec = ChannelCodec.AFFINE


class SogVersion(Struct, frozen=True, gc=False):
    version: int = 0


class SogBlock(Struct, frozen=True, gc=False):
    files: tuple[str, ...]
    codebook: tuple[float, ...] = ()
    mins: tuple[float, float, float] = (0.0, 0.0, 0.0)
    maxs: tuple[float, float, float] = (0.0, 0.0, 0.0)
    count: int = 0
    bands: int = 0


class SogMeta(Struct, frozen=True, gc=False):
    count: int
    means: SogBlock
    scales: SogBlock
    quats: SogBlock
    sh0: SogBlock
    shN: SogBlock | None = None


_SOG_VERSION_DECODER: Final[Decoder[SogVersion]] = Decoder(type=SogVersion)
_SOG_META_DECODER: Final[Decoder[SogMeta]] = Decoder(type=SogMeta)


class SplatHeader(Struct, frozen=True, gc=False):
    fmt: SplatFormat
    splat_count: int
    harmonic_degree: int
    fractional_bits: int = 0


@tagged_union(frozen=True)
class SplatBody:
    tag: Literal["packed", "planar"] = tag()
    packed: bytes = case()
    planar: tuple[SogMeta, Map[str, bytes]] = case()


class IngestPolicy(Struct, frozen=True, gc=False):
    stages: tuple[IngestStage, ...] = (IngestStage.OUTLIER_REMOVE, IngestStage.DECIMATE)
    ground_filter: IngestFilter = IngestFilter.SMRF
    ground_window: float = 18.0
    ground_cell: float = 1.0
    ground_slope: float = 0.15
    outlier_mean_k: int = 8
    outlier_multiplier: float = 2.2
    decimate_filter: IngestFilter = IngestFilter.VOXELDOWNSIZE
    decimate_step: int = 4
    voxel_cell: float = 0.05
    range_axis: str = "Z"
    range_band: tuple[float, float] = (0.0, 30.0)
    farthest_points: int = 50_000
    stream_chunk: int = 0

    @property
    def range_limits(self) -> str:
        lo, hi = self.range_band
        return f"{self.range_axis}[{lo:g}:{hi:g}]"


class StationFact(Struct, frozen=True, gc=False):
    guid: str
    points: int
    translation: tuple[float, float, float]


@tagged_union(frozen=True)
class ScanOp:
    tag: OpKind = tag()
    arrow_las: "PointRecordTable" = case()
    e57: str = case()
    e57_write: tuple[str, Cloud, tuple[StationFact, ...], Pose] = case()
    splat: bytes = case()


@tagged_union(frozen=True)
class IngestProduct:
    tag: Literal["cloud", "splat", "written"] = tag()
    cloud: Cloud = case()
    splat: ArtifactRef = case()
    written: ContentKey = case()


class IngestReceipt(Struct, frozen=True, gc=False):
    source: OpKind
    stages: tuple[IngestStage, ...]
    input_points: Option[int]
    output_points: Option[int]
    decimation: Option[float] = Nothing
    srs: Posture[str] = Posture(absent=None)
    dimensions: Option[int] = Nothing
    point_format: Option[int] = Nothing
    source_key: Option[ContentKey] = Nothing
    stations: tuple[StationFact, ...] = ()

    @staticmethod
    def of(
        op: ScanOp,
        applied: tuple[IngestStage, ...],
        input_points: Option[int],
        output_points: Option[int],
        srs: Posture[str],
        dimensions: Option[int],
        stations: tuple[StationFact, ...],
        *,
        point_format: Option[int] = Nothing,
        source_key: Option[ContentKey] = Nothing,
    ) -> "IngestReceipt":
        ratio = input_points.bind(lambda seen: output_points.map(lambda kept: kept / seen) if seen else Nothing)
        return IngestReceipt(op.tag, applied, input_points, output_points, ratio, srs, dimensions, point_format, source_key, stations)

    @staticmethod
    @receipted(OPEN)
    def _emit(receipt: "IngestReceipt") -> "IngestReceipt":
        return receipt

    def facts(self) -> dict[str, object]:
        measured: Block[tuple[str, Option[object]]] = Block.of_seq([
            ("input_points", self.input_points),
            ("output_points", self.output_points),
            ("decimation", self.decimation),
            ("dimensions", self.dimensions),
            ("point_format", self.point_format),
            ("srs", self.srs.option()),
            ("srs_source", self.srs.source),
            ("source_key", self.source_key.map(lambda key: key.hex)),
        ])
        return {
            "source": self.source.value,
            "stages": tuple(s.value for s in self.stages),
            "stations": tuple((s.guid, s.points, s.translation) for s in self.stations),
        } | dict(measured.choose(lambda slot: slot[1].map(lambda held: (slot[0], held))))

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.ingestion", ("emitted", self.source.value, self.facts())),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _demanded(op: ScanOp) -> Block[str]:
    if op.tag is not OpKind.SPLAT:
        return Block.empty()
    return _signed(op.splat).bind(lambda row: _FORMAT_MODULE.try_find(row[1])).map(Block.singleton).default_value(Block.empty())


def _reached(op: ScanOp, policy: IngestPolicy) -> "RuntimeRail[ScanOp]":
    verb = _OP_MODULE.try_find(op.tag).map(Block.singleton).default_value(Block.empty())
    demanded = verb.append(_demanded(op)).append(Block.of_seq(policy.stages).choose(_STAGE_MODULE.try_find))
    unreached = demanded.filter(lambda module: module in _UNREACHED).try_head()
    return unreached.map(lambda module: Error(INGEST_UNREACHED.raised(module))).default_value(Ok(op))


@beartype(conf=FAULT_CONF)
def _structured(x: np.ndarray, y: np.ndarray, z: np.ndarray, rgb: tuple[np.ndarray, np.ndarray, np.ndarray] | None = None) -> np.ndarray:
    out = np.empty(x.shape[0], dtype=_COLOR_DTYPE if rgb is not None else _DTYPE)
    out["X"], out["Y"], out["Z"] = x, y, z
    for band, values in zip(("Red", "Green", "Blue"), rgb or (), strict=False):
        arr = np.asarray(values)
        out[band] = arr.astype(np.uint16) * 257 if arr.dtype.kind in "iu" and np.iinfo(arr.dtype).max == 255 else arr
    return out


def _read_e57(path: str) -> tuple[np.ndarray, tuple[StationFact, ...]]:
    with pye57.E57(path, mode="r") as handle:
        stations = tuple(
            StationFact(str(h.guid), int(h.point_count), tuple(float(v) for v in h.translation))
            for h in (handle.get_header(index) for index in range(handle.scan_count))
        )
        scans = tuple(handle.read_scan(index, transform=True, colors=True, ignore_missing_fields=True) for index in range(handle.scan_count))
    colored = bool(scans) and all("colorRed" in scan for scan in scans)
    blocks = Block.of_seq(
        _structured(
            *(scan[f"cartesian{axis}"] for axis in ("X", "Y", "Z")),
            tuple(scan[f"color{band}"] for band in ("Red", "Green", "Blue")) if colored else None,
        )
        for scan in scans
    )
    points = blocks.try_head().map(lambda _: np.concatenate(blocks)).default_value(np.empty(0, dtype=_DTYPE))
    return points, stations


def _write_e57(path: str, cloud: Cloud, stations: tuple[StationFact, ...], pose: Pose) -> ContentKey:
    matrix = np.reshape(np.asarray(pose, dtype=np.float64), (4, 4))
    data = {"cartesianX": cloud.positions[:, 0], "cartesianY": cloud.positions[:, 1], "cartesianZ": cloud.positions[:, 2]}
    with pye57.E57(path, mode="w") as handle:
        for station in stations:
            handle.write_scan_raw(data, name=station.guid, rotation=matrix[:3, :3], translation=matrix[:3, 3])
    return ContentIdentity.key("e57", Path(path).read_bytes())


# --- [PACKED_DECODE]


def _fixed24(raw: np.ndarray, count: int, gain: float) -> np.ndarray:
    lanes = raw.reshape(count, 3).astype(np.int32)
    lifted = lanes[:, 0] | (lanes[:, 1] << 8) | (lanes[:, 2] << 16)
    return (lifted - ((lifted >> 23 & 1) << 24)).astype(np.float32) * gain


def _smallest_three(raw: np.ndarray, count: int) -> np.ndarray:
    word = raw.reshape(count, 4).astype(np.uint32)
    packed = word[:, 0] | (word[:, 1] << 8) | (word[:, 2] << 16) | (word[:, 3] << 24)
    index = (packed >> 30).astype(np.intp)
    triple = np.stack([((packed >> shift) & 0x3FF).astype(np.int32) for shift in (0, 10, 20)], axis=1)
    signed = (triple - ((triple >> 9 & 1) << 10)).astype(np.float32) / float(1 << 9)
    out = np.zeros((count, 4), dtype=np.float32)
    slots = np.stack([np.delete(np.arange(4), largest) for largest in range(4)])[index]
    np.put_along_axis(out, slots, signed, axis=1)
    np.put_along_axis(out, index[:, None], np.sqrt(np.clip(1.0 - (signed**2).sum(axis=1), 0.0, None))[:, None], axis=1)
    return out


def _channels(header: SplatHeader, body: bytes) -> dict[SplatChannel, np.ndarray]:
    def decoded(spec: ChannelSpec, packed: np.ndarray, width: int) -> np.ndarray:
        match spec.codec:
            case ChannelCodec.AFFINE:
                return (packed.astype(np.float32) * spec.gain + spec.bias).reshape(header.splat_count, width)
            case ChannelCodec.FIXED24:
                return _fixed24(packed, header.splat_count, 2.0**-header.fractional_bits).reshape(header.splat_count, width // 3)
            case ChannelCodec.SMALLEST_THREE:
                return _smallest_three(packed, header.splat_count)
            case _ as unreachable:
                assert_never(unreachable)

    def block(state: tuple[int, dict[SplatChannel, np.ndarray]], spec: ChannelSpec) -> tuple[int, dict[SplatChannel, np.ndarray]]:
        offset, held = state
        width = spec.width or _harmonic_width(header.harmonic_degree) - 3
        packed = np.frombuffer(body, dtype=np.dtype(spec.dtype), count=header.splat_count * width, offset=offset)
        spec.channel.map(lambda channel: held.__setitem__(channel, decoded(spec, packed, width)))
        return offset + packed.nbytes, held

    return _LAYOUT[header.fmt].fold(block, (0, {}))[1]


# --- [PLANAR_DECODE]


def _plane(planes: Map[str, bytes], name: str, bands: int) -> np.ndarray:
    payload = planes.try_find(name).default_with(lambda: _malformed(f"plane-absent:{name}"))
    try:
        with Image.open(BytesIO(payload), formats=_WEBP) as opened:
            if len(opened.getbands()) < bands:
                _malformed(f"plane-bands:{name}")
            return np.asarray(opened.convert(_PLANE_MODE[bands]))
    except UnidentifiedImageError:
        return _malformed(f"plane-unrecognized:{name}")
    except Image.DecompressionBombError:
        return _malformed(f"plane-oversized:{name}")
    except OSError:
        return _malformed(f"plane-truncated:{name}")


def _rows(planes: Map[str, bytes], name: str, bands: int, count: int) -> np.ndarray:
    plane = _plane(planes, name, bands)
    flat = plane.reshape(-1, plane.shape[-1])
    if flat.shape[0] < count:
        _malformed(f"plane-extent:{name}")
    return flat[:count]


def _codebook(values: tuple[float, ...], name: str) -> np.ndarray:
    if len(values) != _SOG_CODEBOOK:
        _malformed(f"codebook:{name}")
    return np.asarray(values, dtype=np.float32)


def _lerped(low: np.ndarray, high: np.ndarray, mins: tuple[float, float, float], maxs: tuple[float, float, float]) -> np.ndarray:
    base = np.asarray(mins, dtype=np.float32)
    q = (high.astype(np.uint32) << 8) | low.astype(np.uint32)
    n = base + (np.asarray(maxs, dtype=np.float32) - base) * (q.astype(np.float32) / 65535.0)
    return np.sign(n) * np.expm1(np.abs(n))


def _quats(plane: np.ndarray) -> np.ndarray:
    mode = plane[:, 3].astype(np.intp) - _QUAT_MODE_BASE
    if bool(np.any((mode < 0) | (mode > 3))):
        _malformed("quats:reserved-mode")
    kept = (plane[:, :3].astype(np.float32) / 255.0 - 0.5) * (2.0 / np.sqrt(2.0))
    out = np.zeros((plane.shape[0], 4), dtype=np.float32)
    slots = np.stack([np.delete(np.arange(4), omitted) for omitted in range(4)])[mode]
    np.put_along_axis(out, slots, kept, axis=1)
    np.put_along_axis(out, mode[:, None], np.sqrt(np.clip(1.0 - (kept**2).sum(axis=1), 0.0, None))[:, None], axis=1)
    return out


def _harmonics(shN: SogBlock | None, planes: Map[str, bytes], count: int, degree: int) -> np.ndarray:
    if shN is None:
        return np.empty((count, 0), dtype=np.float32)
    coeffs = _harmonic_width(degree) // 3 - 1
    if len(shN.files) != 2 or not 0 < shN.count <= _SOG_MAX_CENTROIDS:
        _malformed(f"shN:{shN.count}")
    labels = _rows(planes, shN.files[1], 3, count)
    label = labels[:, 0].astype(np.intp) | (labels[:, 1].astype(np.intp) << 8)
    grid = _plane(planes, shN.files[0], 3)
    if int(label.max(initial=0)) >= shN.count or grid.shape[0] * _SOG_CENTROID_ROW < shN.count or grid.shape[1] < _SOG_CENTROID_ROW * coeffs:
        _malformed("shN:centroid-extent")
    picked = grid[
        (label // _SOG_CENTROID_ROW)[:, None],
        ((label % _SOG_CENTROID_ROW) * coeffs)[:, None] + np.arange(coeffs)[None, :],
        :,
    ]
    return _codebook(shN.codebook, "shN")[picked].reshape(count, coeffs * 3)


def _planes(header: SplatHeader, meta: SogMeta, planes: Map[str, bytes]) -> dict[SplatChannel, np.ndarray]:
    count = header.splat_count
    if (len(meta.means.files), len(meta.scales.files), len(meta.quats.files), len(meta.sh0.files)) != (2, 1, 1, 1):
        _malformed("sog:plane-arity")
    sh0 = _rows(planes, meta.sh0.files[0], 4, count)
    return {
        SplatChannel.POSITION: _lerped(
            _rows(planes, meta.means.files[0], 3, count),
            _rows(planes, meta.means.files[1], 3, count),
            meta.means.mins,
            meta.means.maxs,
        ),
        SplatChannel.SCALE: np.exp(_codebook(meta.scales.codebook, "scales")[_rows(planes, meta.scales.files[0], 3, count)]),
        SplatChannel.ROTATION: _quats(_rows(planes, meta.quats.files[0], 4, count)),
        SplatChannel.COLOR: _codebook(meta.sh0.codebook, "sh0")[sh0[:, :3]],
        SplatChannel.HARMONIC: _harmonics(meta.shN, planes, count, header.harmonic_degree),
        SplatChannel.ALPHA: sh0[:, 3].astype(np.float32) / 255.0,
    }


# --- [CONTAINERS]


def _ngsp(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    if len(raw) < _NGSP_HEADER.size:
        _malformed(f"truncated:{len(raw)}")
    _magic, version, points, degree, fractional, _flags, streams, toc = _NGSP_HEADER.unpack_from(raw)
    toc_end = toc + streams * 16
    checks = (
        (version in _ZSTD_VERSIONS, f"version:{version}"),
        (0 < points <= 0x7FFFFFFF and points * 9 <= len(raw) * 1024, f"count:{points}"),
        (_NGSP_HEADER.size <= toc and toc_end <= len(raw), f"toc:{toc}"),
    )
    Block.of_seq(checks).choose(lambda row: Nothing if row[0] else Some(row[1])).try_head().map(_malformed)

    def frame(state: tuple[int, tuple[bytes, ...]], pair: tuple[int, int]) -> tuple[int, tuple[bytes, ...]]:
        offset, held = state
        compressed, expanded = pair
        chunk = zstd.decompress(raw[offset : offset + compressed])
        if len(chunk) != expanded:
            _malformed(f"stream:{len(held)}")
        return offset + compressed, (*held, chunk)

    pairs = np.frombuffer(raw, dtype="<u8", count=streams * 2, offset=toc).reshape(streams, 2)
    tail, body = Block.of_seq((int(row[0]), int(row[1])) for row in pairs).fold(frame, (toc_end, ()))
    if tail != len(raw):
        _malformed(f"container:trailing:{len(raw) - tail}")
    return SplatHeader(SplatFormat.SPZ_V4, points, degree, fractional), SplatBody(packed=b"".join(body))


def _legacy(body: bytes) -> tuple[SplatHeader, SplatBody]:
    if len(body) < _LEGACY_HEADER.size or body[:4] != _NGSP_MAGIC:
        _malformed("legacy:header")
    _magic, version, points, degree, fractional, _flags, _reserved = _LEGACY_HEADER.unpack_from(body)
    if version != _LEGACY_VERSION:
        _malformed(f"legacy-version:{version}")
    if not 0 < points <= 0x7FFFFFFF:
        _malformed(f"count:{points}")
    return SplatHeader(SplatFormat.SPZ_V4, points, degree, fractional), SplatBody(packed=body[_LEGACY_HEADER.size :])


def _manifest[T](decoder: Decoder[T], body: bytes) -> T:
    try:
        return decoder.decode(body)
    except ValidationError:
        return _malformed("sog-meta:constraint")
    except DecodeError:
        return _malformed("sog-meta:malformed")


def _sog(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    try:
        with zipfile.ZipFile(BytesIO(raw)) as archive:
            held = Map.of_seq((name, archive.read(name)) for name in archive.namelist())
    except (zipfile.BadZipFile, OSError):
        return _malformed("sog:archive")
    body = held.try_find(_SOG_MANIFEST).default_with(lambda: _malformed(f"plane-absent:{_SOG_MANIFEST}"))
    version = _manifest(_SOG_VERSION_DECODER, body).version
    if version != _SOG_VERSION:
        _malformed(f"sog-version:{version}")
    meta = _manifest(_SOG_META_DECODER, body)
    degree = 0 if meta.shN is None else meta.shN.bands
    checks = (
        (0 < meta.count <= 0x7FFFFFFF, f"count:{meta.count}"),
        (0 <= degree <= _grounded(SplatFormat.SOG_V2).degree, f"bands:{degree}"),
    )
    Block.of_seq(checks).choose(lambda row: Nothing if row[0] else Some(row[1])).try_head().map(_malformed)
    return SplatHeader(SplatFormat.SOG_V2, meta.count, degree), SplatBody(planar=(meta, held))


def _signed(raw: bytes) -> Option[tuple[bytes, SplatFormat, Callable[[bytes], tuple[SplatHeader, SplatBody]]]]:
    return _SIGNATURE.filter(lambda row: raw[: len(row[0])] == row[0]).try_head()


def _container(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    return _signed(raw).map(lambda row: row[2](raw)).default_with(lambda: _malformed("container:unrecognized"))


def _splatted(header: SplatHeader, body: SplatBody) -> GaussianSplatScan:
    match body:
        case SplatBody(tag="packed", packed=blob):
            channels = _channels(header, blob)
        case SplatBody(tag="planar", planar=(meta, planes)):
            channels = _planes(header, meta, planes)
        case _ as unreachable:
            assert_never(unreachable)
    scan = GaussianSplatScan(
        format=header.fmt,
        positions=np.ascontiguousarray(channels[SplatChannel.POSITION], dtype=np.float32).tobytes(),
        scales=np.ascontiguousarray(channels[SplatChannel.SCALE], dtype=np.float32).tobytes(),
        rotations=np.ascontiguousarray(channels[SplatChannel.ROTATION], dtype=np.float32).tobytes(),
        harmonics=np.ascontiguousarray(
            np.concatenate((channels[SplatChannel.COLOR], channels[SplatChannel.HARMONIC]), axis=1), dtype=np.float32
        ).tobytes(),
        harmonic_degree=header.harmonic_degree,
        splat_count=header.splat_count,
        alphas=np.ascontiguousarray(channels[SplatChannel.ALPHA].reshape(-1), dtype=np.float32).tobytes(),
    )
    try:
        validate(scan)
    except (CompilationError, ContractValidationError, EvaluationError) as cause:
        raise IngestFault(malformed="gaussian-splat-contract") from cause
    return scan


def _grounded(fmt: SplatFormat) -> SplatGrounding:
    return splat_form(fmt).default_with(lambda fault: _malformed(str(fault.facts().get("detail", fault.subject))))


def _malformed[T](offending: str) -> T:
    raise IngestFault(malformed=offending)


def _farthest(cloud: Cloud, policy: IngestPolicy) -> Cloud:
    return (
        cloud
        if policy.farthest_points <= 0 or len(cloud) <= policy.farthest_points
        else Cloud.of_legacy(cloud.legacy().farthest_point_down_sample(policy.farthest_points))
    )


def _piped(policy: IngestPolicy) -> Option[Block["pdal.Filter"]]:
    built = Block.of_seq(policy.stages).choose(_STAGE.try_find).map(lambda build: build(pdal.Filter, policy))
    return built.try_head().map(lambda _: built)


def _execute(stages: Block["pdal.Filter"], points: np.ndarray, chunk: int) -> tuple[np.ndarray, str, int]:
    pipeline = stages.tail().fold(lambda acc, stage: acc | stage, stages.head().pipeline(points))
    if chunk and pipeline.streamable:
        cleaned = np.concatenate(tuple(pipeline.iterator(chunk_size=chunk)))
    else:
        pipeline.execute()
        cleaned = pipeline.arrays[0]
    schema = pipeline.schema.get("schema", {}).get("dimensions", ())
    return cleaned, Some(text) if (text := str(pipeline.srswkt2 or "")) else Nothing, Some(len(schema))


def _filter_graph(points: np.ndarray, policy: IngestPolicy) -> tuple[Cloud, Option[str], Option[int]]:
    cleaned, srs, dims = (
        _piped(policy).map(lambda stages: _execute(stages, points, policy.stream_chunk)).default_value((points, Nothing, Nothing))
    )
    colors = (
        np.column_stack((cleaned["Red"], cleaned["Green"], cleaned["Blue"])).astype(np.float64) / 65535.0
        if {"Red", "Green", "Blue"} <= set(cleaned.dtype.names or ())
        else np.empty((0, 3))
    )
    carrier = Cloud(positions=np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float64), colors=colors)
    return Block.of_seq(policy.stages).choose(_CARRIER.try_find).fold(lambda held, fold: fold(held, policy), carrier), srs, dims


def _cleaned(
    points: np.ndarray, op: ScanOp, policy: IngestPolicy, stations: tuple[StationFact, ...], carrier: "Option[PointRecordTable]"
) -> tuple[IngestProduct, IngestReceipt]:
    cloud, srs, dims = _filter_graph(points, policy)
    crs = (
        srs.map(lambda text: Posture(declared=text))
        .or_else_with(lambda: carrier.bind(lambda held: held.crs.option()).map(lambda text: Posture(defaulted=(text, _INHERITED_CRS))))
        .default_value(Posture(absent=None))
    )
    return IngestProduct(cloud=cloud), IngestReceipt.of(
        op,
        policy.stages,
        Some(points.shape[0]),
        Some(len(cloud)),
        crs,
        dims,
        stations,
        point_format=carrier.map(lambda held: held.point_format),
        source_key=carrier.map(lambda held: held.content_key),
    )


def _ingest_kernel(op: ScanOp, policy: IngestPolicy) -> tuple[IngestProduct | GaussianSplatScan, IngestReceipt]:
    match op:
        case ScanOp(tag=OpKind.ARROW_LAS, arrow_las=carrier):
            table = carrier.table
            rgb = (
                tuple(table.column(band).to_numpy(zero_copy_only=False) for band in ("red", "green", "blue"))
                if {"red", "green", "blue"} <= set(table.column_names)
                else None
            )
            points = _structured(*(table.column(axis).to_numpy(zero_copy_only=False) for axis in ("x", "y", "z")), rgb)
            return _cleaned(points, op, policy, (), Some(carrier))
        case ScanOp(tag=OpKind.E57, e57=path):
            points, stations = _read_e57(path)
            return _cleaned(points, op, policy, stations, Nothing)
        case ScanOp(tag=OpKind.E57_WRITE, e57_write=(path, cloud, stations, pose)):
            key = _write_e57(path, cloud, stations, pose)
            return IngestProduct(written=key), IngestReceipt.of(op, (), Nothing, Some(len(cloud)), Posture(absent=None), Nothing, stations)
        case ScanOp(tag=OpKind.SPLAT, splat=raw):
            scan = _splatted(*_container(raw))
            return scan, IngestReceipt.of(op, (), Nothing, Some(scan.splat_count), Posture(absent=None), Nothing, ())
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanIngestion(Struct, frozen=True):
    lane: LanePolicy
    artifacts: ArtifactTransfer
    policy: IngestPolicy = IngestPolicy()
    composition: ScopeKey = DEFAULT_SCOPE

    async def run(self, op: ScanOp) -> "RuntimeRail[tuple[IngestProduct, IngestReceipt]]":
        match _reached(op, self.policy):
            case Result(tag="ok", ok=admitted):
                async def execute() -> tuple[IngestProduct, IngestReceipt]:
                    product, receipt = await self.lane.offload(
                        Kernel.of(_ingest_kernel, KernelTrait.HOSTILE), admitted, self.policy
                    )
                    if isinstance(product, GaussianSplatScan):
                        return IngestProduct(splat=await self.artifacts.put(product.to_binary())), receipt
                    return product, receipt

                rail = await evidence_run(
                    EvidenceScope.SCAN_INGESTION,
                    f"run.{admitted.tag.value}",
                    execute,
                    composition=self.composition,
                )
                return rail.map(lambda pair: (pair[0], IngestReceipt._emit(pair[1])))
            case Result(tag="error") as refused:
                return refused


# --- [TABLES] ---------------------------------------------------------------------------

_FILTER: Final[Map[IngestFilter, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestFilter.SMRF, lambda flt, p: flt.smrf(window=p.ground_window, cell=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.PMF, lambda flt, p: flt.pmf(max_window_size=p.ground_window, cell_size=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.OUTLIER, lambda flt, p: flt.outlier(method="statistical", mean_k=p.outlier_mean_k, multiplier=p.outlier_multiplier)),
    (IngestFilter.DECIMATION, lambda flt, p: flt.decimation(step=p.decimate_step)),
    (IngestFilter.VOXELDOWNSIZE, lambda flt, p: flt.voxeldownsize(cell=p.voxel_cell)),
    (IngestFilter.RANGE, lambda flt, p: flt.range(limits=p.range_limits)),
])

_STAGE: Final[Map[IngestStage, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, lambda flt, p: _FILTER[p.ground_filter](flt, p)),
    (IngestStage.OUTLIER_REMOVE, _FILTER[IngestFilter.OUTLIER]),
    (IngestStage.DECIMATE, lambda flt, p: _FILTER[p.decimate_filter](flt, p)),
    (IngestStage.RANGE_CROP, _FILTER[IngestFilter.RANGE]),
])

_CARRIER: Final[Map[IngestStage, Callable[[Cloud, IngestPolicy], Cloud]]] = Map.of_seq([(IngestStage.FARTHEST_POINT, _farthest)])

_LAYOUT: Final[Map[SplatFormat, Block[ChannelSpec]]] = Map.of_seq([
    (
        SplatFormat.SPZ_V4,
        Block.of_seq((
            ChannelSpec(Some(SplatChannel.POSITION), 9, "u1", 1.0, 0.0, ChannelCodec.FIXED24),
            ChannelSpec(Some(SplatChannel.ALPHA), 1, "u1", 1.0 / 255.0, 0.0),
            ChannelSpec(Some(SplatChannel.COLOR), 3, "u1", 1.0 / (255.0 * 0.15), -0.5 / 0.15),
            ChannelSpec(Some(SplatChannel.SCALE), 3, "u1", 1.0 / 16.0, -10.0),
            ChannelSpec(Some(SplatChannel.ROTATION), 4, "u1", 1.0, 0.0, ChannelCodec.SMALLEST_THREE),
            ChannelSpec(Some(SplatChannel.HARMONIC), 0, "u1", 1.0 / 128.0, -1.0),
        )),
    ),
])

_SIGNATURE: Final[Block[tuple[bytes, SplatFormat, Callable[[bytes], tuple[SplatHeader, SplatBody]]]]] = Block.of_seq((
    (_NGSP_MAGIC, SplatFormat.SPZ_V4, _ngsp),
    (b"\x1f\x8b", SplatFormat.SPZ_V4, lambda raw: _legacy(gzip.decompress(raw))),
    (b"PK\x03\x04", SplatFormat.SOG_V2, _sog),
))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
