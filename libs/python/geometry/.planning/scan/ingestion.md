# [PY_GEOMETRY_SCAN_INGESTION]

`ScanIngestion` fronts the host-free scan plane registration-ready — the raw-scan cleaning the `data` branch declines — and owns the reality-capture companion decode the C# residency lane consumes. One frozen owner discriminates over a `ScanOp` `@tagged_union` whose cases carry their own codec: `arrow_las` holds the content-keyed `PointRecordTable` from `data/spatial/mesh#POINTCLOUD` (LAS/LAZ/COPC already decoded, its CRS and point format already parsed), `e57` holds the `pye57` structured multi-scan read that bridge does not own, `e57_write` is the inverse leg on the same surface because the provider lives HERE and no data-branch owner holds an E57 codec, and `splat` decodes a `realitycapture` SPZ/SOG container into the gaussian-splat carrier. Every product leaves as one closed `IngestProduct` case beside one `IngestReceipt`. This owner mints the scan plane's sealed cloud crossing: a live `open3d` point cloud is a pybind11 handle no pickler carries, so clouds cross every worker seam as bare `positions`/`colors`/`normals` arrays on the frozen `Cloud` carrier, which also owns its own content digest so a consumer keys a cloud without re-spelling one. Graph shape is policy, not code — an `IngestStage` row sequence folded over the `pdal` `|` pipe, so ground classification, outlier removal, downsampling, and range cropping order and membership are `IngestPolicy` rows a rebuild reorders without touching the fold; a block-scale cloud rides the streaming arm when `IngestPolicy.stream_chunk` is non-zero.

Provider presence is PROVED, never assumed: `pye57` and `open3d` both carry interpreter markers, so `_UNREACHED` resolves their absence once at import through `find_spec` and every arm that needs one refuses by name through the folder roster's own `INGEST_UNREACHED` row naming the missing module, where an ungated arm dies as a bare `ModuleNotFoundError` inside an offloaded worker and reaches the caller as a worker-crash rail. `run` is `async`, keeping the multi-second SMRF/voxel sweep off the event loop: it composes the graduation `evidence_run` weave (span + fence + receipt harvest, `EvidenceScope.SCAN_INGESTION` the seed, the owner's composition `ScopeKey` the custody stamp) around the `lane.offload` crossing on `Kernel.of(_ingest_kernel, KernelTrait.HOSTILE)` — the `pdal`/`pye57`/`open3d` band holds process-global native state and imports under no isolated subinterpreter — and the stage graph builds worker-side inside the kernel, so no `pdal` object meets the pickle seam. A cleaned `Cloud` is the precondition `scan/registration#REGISTRATION` consumes across a same-folder read-only seam.

## [01]-[INDEX]

- [02]-[INGESTION]: verb-discriminated scan IO — the `ScanOp` intake folded through one policy-ordered `pdal` `|` filter graph to the registration-ready `Cloud`, the E57 egress leg, and the gaussian-splat companion decode, offloaded to the warm process pool under the graduation weave.

## [02]-[INGESTION]

- Owner: `ScanIngestion`, the frozen dispatch owner carrying the composition `ScopeKey` its weave stamps; `ScanOp`'s tag IS the codec-carrying discriminant across BOTH directions — read and write ride one surface because the domain admits the inverse and a sibling `write_scan` entry would fork the provider gate, the receipt, and the weave. `Cloud` mints HERE as the scan plane's sealed cloud crossing — bare ndarray fields, the `tensor()`/`legacy()` rebuild pair, and its own `digest` content key — and `scan/registration`, `scan/deviation`, `scan/reconstruction` import it downward, never a per-page carrier twin and never a per-page cloud hash. It carries no rigid re-pose: every correspondence-search arm publishes its own initial-transform argument, and the one EM arm that publishes none pre-poses at its own admission, so a carrier-level pre-pose would pay a whole-cloud copy per solve for a seed the consuming arm already owns. `SplatScan` byte-mirrors the C#-minted `GaussianSplatScan` wire vocabulary field for field, so the companion produces exactly what the residency lane admits.
- Cases: `ScanOp` arms `arrow_las` (the data-branch `PointRecordTable` carrier — table, point count, point format, CRS WKT, and content key together), `e57` (the `pye57` structured multi-scan source read per-scan with acquisition pose applied and `ScanHeader` provenance harvested), `e57_write` (the `write_scan_raw` append leg, pose from the supplied rotation/translation), and `splat` (raw SPZ/SOG container bytes the signature-dispatched `_container` reader opens worker-side and the companion decodes), matched by `match`/`assert_never`. `IngestProduct` mirrors the outcome — `cloud`, `splat`, or the written `ContentKey` — one closed family, never an erased `object` a consumer re-discriminates. `IngestStage` rows — `GROUND_CLASSIFY` (`SMRF` default / `PMF` alternate), `OUTLIER_REMOVE`, `DECIMATE` (`DECIMATION` / `VOXELDOWNSIZE`), `RANGE_CROP` — each build their `pdal.Filter` through one `_STAGE` row, the swappable rows dispatching to the policy-chosen `_FILTER` factory, so a stage's driver and option dict are one row read; `FARTHEST_POINT` is the carrier-fold row on `_CARRIER`, bounding a point budget geometry-uniformly where every `pdal` decimator bounds by file order or by cell occupancy.
- Law: a splat container's `format_key` is LEGAL only where the runtime `transport/shapes#VOCABULARY` `SPLAT_FORMS` matrix publishes it, and the harmonic band ceiling and alpha activation the shape gate proves are that admitted row's own DECLARED grounding — a ceiling transcribed here would let a published release raising its band pass one end of the wire and fail the other, and a refused release would decode as garbage under a gate that cannot see the encoding it declines.
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
- Boundary: the inbound LAS/LAZ/COPC decode and the `PointRecordTable` mint are `data/spatial/mesh#POINTCLOUD`'s (`laspy` full decode and the COPC octree subset live there), so ingestion never re-reads LAS nor crosses a `pdal` `Pipeline` at the data seam; the E57 path is ingestion's in BOTH directions because `pye57` is absent from the data branch and no data owner holds an E57 codec, so declining the write leg to that seam would leave E57 egress unowned in the whole branch. The `GaussianSplatScan` wire vocabulary is C#-minted and the runtime `transport/shapes` registry codec pair carries it — this owner produces the typed `SplatScan` and its content key, never a proto shape, and the outward frame is the runtime `ArtifactFrame` seam an app root drives. Registration is `scan/registration#REGISTRATION`'s; ingestion never registers, deviates, reconstructs, tessellates, stores, or mutates a Rhino/GH document.

```python signature
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

from rasm.geometry.graduation import EvidenceScope, GeometryLeg, evidence_run
from rasm.runtime.faults import FAULT_CONF, TERMINAL, FaultRow, Posture, RuntimeRail, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, receipted
from rasm.runtime.shapes import Activation, SplatGrounding, splat_form
from rasm.runtime.workers import Kernel, KernelTrait

# the compiled scan band, each a module-scope proxy behind its floor gate: `pdal` resolves on every floor and the
# other three are interpreter-marked, so `_UNREACHED` refuses their arms by name before a proxy is ever touched.
# A `lazy` statement is module-scope only — inside a function body it is a SyntaxError.
lazy from PIL import Image, UnidentifiedImageError
lazy import open3d as o3d
lazy import pdal
lazy import pye57

if TYPE_CHECKING:  # S2 -> S1 crossing shape, annotation-only: the columnar carrier's own module stays unimported here
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
    FARTHEST_POINT = "farthest-point"  # geometry-uniform budget bound; a carrier fold, not a `pdal` driver


# `.value` is the `filters.*` driver string the swappable stages select over.
class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


class SplatFormat(StrEnum):
    SPZ_V4 = "spz-v4"  # Niantic SPZ: v4 is a 32-byte plaintext header + per-stream ZSTD behind a TOC; pre-v4 one gzip member
    SOG_V2 = "sog-v2"  # PlayCanvas SOG v2: a ZIP whose root holds `meta.json` beside the lossless-WebP planes it names


class SplatChannel(StrEnum):
    # the wire channels the C#-minted `GaussianSplatScan` admits, in the order its `SplatScan` record spells them,
    # plus one CONTAINER-held member: `COLOR` is the SPZ DC-colour block, which the wire carries INSIDE the
    # harmonic band (the wire's `(degree + 1)^2 * 3` width counts the DC triple the container stores separately),
    # so `_splatted` folds it onto the harmonic head and no wire column exists for it.
    POSITION = "positions"
    SCALE = "scales"
    ROTATION = "rotations"
    HARMONIC = "harmonics"
    ALPHA = "alphas"
    COLOR = "colors"


# rigid pose as the raveled row-major 4x4 the registration seeding law threads.
type Pose = tuple[float, ...]

# --- [CONSTANTS] ------------------------------------------------------------------------


# one structured layout the Arrow carrier and E57 blocks both fill; `pdal` reads X/Y/Z and threads the optional
# full-range u16 Red/Green/Blue band through every filter stage untouched, so COLORED_ICP receives source color.
_DTYPE: Final = np.dtype([(axis, np.float64) for axis in ("X", "Y", "Z")])
_COLOR_DTYPE: Final = np.dtype([*_DTYPE.descr, *((band, np.uint16) for band in ("Red", "Green", "Blue"))])

# every marked distribution this page can reach, and the absence set derived ONCE at import. A selected arm or a
# rostered stage with no floor gate ahead of its lazy provider is the deleted form: the import lands inside an
# offloaded worker and the lane converts it to a worker-crash rail where a provisioning refusal by name is the
# honest answer. The carrier's own native projections read the same set — a consumer calls those on its own floor
# rather than through `run`, and they return provider HANDLES, so absence raises INTO the enclosing lane fence
# naming the module exactly as the entry gate does, never a probe re-run per projection call.
_CLOUD_MODULE: Final[str] = "open3d"
_IMAGE_MODULE: Final[str] = "PIL"  # the top-level module `find_spec` resolves and the refusal names; distribution `pillow`
_UNREACHED: Final[frozenset[str]] = frozenset(m for m in ("pye57", _CLOUD_MODULE, _IMAGE_MODULE) if find_spec(m) is None)

# the THREE axes a marked provider enters on: the verb's own codec, the CONTAINER a byte-fed verb carries, and a
# cleaning stage whose fold is native rather than a `pdal` driver. All resolve against one absence set, so a host
# provisioned for one and not another reads one refusal naming the module it lacks instead of two half-facts a
# caller meets on consecutive runs. The container axis is its own row set because the splat verb spans two formats
# with different provider demands: an SPZ payload decodes on the interpreter floor alone, so a verb-level image row
# would refuse a decode the host can serve.
_OP_MODULE: Final[Map[OpKind, str]] = Map.of_seq([(OpKind.E57, "pye57"), (OpKind.E57_WRITE, "pye57")])
_FORMAT_MODULE: Final[Map[SplatFormat, str]] = Map.of_seq([(SplatFormat.SOG_V2, _IMAGE_MODULE)])
_STAGE_MODULE: Final[Map[IngestStage, str]] = Map.of_seq([(IngestStage.FARTHEST_POINT, _CLOUD_MODULE)])

# the NAMED source a `Posture.defaulted` CRS quotes, spelled once so a receipt reader and this fold cannot disagree
# about where an inherited coordinate reference came from.
_INHERITED_CRS: Final[str] = "PointRecordTable.crs"

# this module's whole raise roster: the ONE floor refusal anchors one row, so the gate spells no subject and
# the `rostered` door seats every row on the branch census, proving `geometry.scan.ingestion` against a real module at import. TERMINAL, because a
# distribution absent from the venv stays absent for every re-issue on that host — a provisioning fact, not a
# transient. The retired literal spelled `import_=module`, a BARE string into a two-slot case, so the door also
# repairs an arity the union always demanded.
INGEST_UNREACHED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.INGESTION, point="floor", arm="import_", defect="module-absent", retriability=TERMINAL, slots=("module",)
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([INGEST_UNREACHED]))

# the SPZ container preamble, verified against the packer source (`load-spz.cc`): `NGSP` little-endian magic, the
# 32-byte v4 `NgspFileHeader` (`magic, version, numPoints, shDegree, fractionalBits, flags, numStreams,
# tocByteOffset`, twelve reserved bytes), and the 16-byte pre-v4 header the gzip body frames. Version law is the
# packer's own: ZSTD-stream containers span `MIN_ZSTD..LATEST` (both 4 today), smallest-three rotations begin at
# version 3, and the packer's point-count admission caps at `INT32_MAX` with the 1024x compression-ratio bound over
# the positions-stream floor of nine bytes per splat.
_NGSP_MAGIC: Final[bytes] = b"NGSP"  # the leading octets both the signature table and the legacy body check read
_NGSP_HEADER: Final[struct.Struct] = struct.Struct("<IIIBBBBI12x")
_LEGACY_HEADER: Final[struct.Struct] = struct.Struct("<IIIBBBB")
_ZSTD_VERSIONS: Final[range] = range(4, 5)
_LEGACY_VERSION: Final[int] = 3  # the one gzip-framed layout `_LAYOUT`'s SPZ row declares; v1/v2 encodings are unrostered

# the SOG v2 container's own declarations. `meta.json` is the fixed root member naming every other one; v2 is the
# one manifest shape this reader decodes, because v1 carries NO version key and stores per-property affine
# mins/maxs instead of codebooks — a different encoding a v2 reader would silently misread. Every codebook is
# exactly the 8-bit index space its plane channel addresses, the centroid grid seats a fixed 64 centroids per row,
# and the quaternion's omitted-component index rides the alpha byte as an offset from `_QUAT_MODE_BASE`.
_SOG_MANIFEST: Final[str] = "meta.json"
_SOG_VERSION: Final[int] = 2
_SOG_CODEBOOK: Final[int] = 256
_SOG_CENTROID_ROW: Final[int] = 64
_SOG_MAX_CENTROIDS: Final[int] = 65_536
_QUAT_MODE_BASE: Final[int] = 252
_WEBP: Final[tuple[str, ...]] = ("WEBP",)  # the plugin restriction `Image.open` probes, so an archive member never walks the whole codec registry
_PLANE_MODE: Final[Map[int, str]] = Map.of_seq([(3, "RGB"), (4, "RGBA")])  # the band count a plane row declares, as the mode `convert` narrows to

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IngestFault(Exception):
    # raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps: the two legs that
    # cannot return a rail — a native projection handing back a provider handle, and a decode inside the offloaded
    # kernel — name their own failure here, and the fence carries the kwarg whole. The kernel leg crosses the worker
    # seam as `CrossedFault` DATA and re-mints parent-side per `execution/workers#CROSSING`, so this page edits nothing.
    tag: Literal["unprovisioned", "malformed"] = tag()
    unprovisioned: str = case()  # the absent module a native projection needed at the interpreter floor
    malformed: str = case()  # the shape law a decoded container breached, named by channel

    def __str__(self) -> str:
        # `BoundaryFault.of` admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so this family crosses the
        # conversion door WHOLE on the `domain` case and the catch-all's `str(cause)` half never renders it. A
        # worker seam carries it whole too: `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA
        # at `shipped` and re-mints this family's own case parent-side, so a raise inside a HOSTILE kernel needs no
        # edit here. `__str__` serves the LOG and HOST edge alone — a token surfacing in a worker traceback or a log
        # line before the seam lowers it — where `Exception.__str__` answers the EMPTY string for a kwarg-only
        # union. The law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift.
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
    # Exemption: the projections below return provider handles rather than rails, so the floor proof raises. The
    # absence set derives once at import, so this is a membership read and never a probe per call.
    if _CLOUD_MODULE in _UNREACHED:
        raise IngestFault(unprovisioned=_CLOUD_MODULE)


# --- [MODELS] ---------------------------------------------------------------------------


class Cloud(Struct, frozen=True):
    # sealed cloud crossing: bare arrays and the rebuild recipe, because a live open3d cloud is a pybind11 handle no
    # pickler carries; a struct-wrapped buffer rides the PICKLE wire by the workers span law, and every consumer
    # re-inflates through tensor()/legacy() where its own native work begins — the brep unsealed() analogue.
    positions: np.ndarray  # (N, 3) float64
    colors: np.ndarray = field(default_factory=lambda: np.empty((0, 3)))
    normals: np.ndarray = field(default_factory=lambda: np.empty((0, 3)))

    def __len__(self) -> int:
        return int(self.positions.shape[0])

    @property
    def digest(self) -> ContentKey:
        # the carrier's OWN content key over its positions: every downstream producer that keys evidence on "which
        # cloud" reads this rather than re-spelling a hash, so a session digest, a reconstruction key, and a cache
        # probe all address one identity. `ContentIdentity.key` is total over a buffer source, so no rail here.
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
        # the INVERSE of `legacy()` on the same owner, so a native cleaning fold hands its provider cloud straight
        # back to the carrier and the color and normal bands survive the stage rather than silently collapsing to
        # positions alone. An absent band is an EMPTY provider vector, so `asarray` yields the zero-extent array the
        # carrier's own default already holds and no `has_*` probe stands between two spellings of one absence.
        return cls(
            positions=np.asarray(cloud.points, dtype=np.float64),
            colors=np.asarray(cloud.colors, dtype=np.float64),
            normals=np.asarray(cloud.normals, dtype=np.float64),
        )


class SplatScan(Struct, frozen=True):
    # byte-mirrors the C#-minted `GaussianSplatScan` wire record field for field, so the residency lane's own shape
    # gate and this producer read one contract: positions/scales at 3 components per splat, rotations at 4
    # (quaternion), harmonics at the derived band width WITH the DC triple at its head, alphas the sigmoid-activated
    # per-splat opacity in [0, 1] (the container stores `sigmoid(logit) * 255`; the pre-activation logit is the
    # consumer's inverse transform), and the format key naming the container it came from. `alphas` appends past
    # the frozen columns under the wire's additive-only law.
    format_key: str
    positions: np.ndarray  # (N, 3) float32
    scales: np.ndarray  # (N, 3) float32
    rotations: np.ndarray  # (N, 4) float32 quaternion
    harmonics: np.ndarray  # (N, harmonic_width) float32 — DC triple first, then the banded coefficients
    harmonic_degree: int
    splat_count: int
    alphas: np.ndarray  # (N,) float32 sigmoid-activated opacity

    @staticmethod
    def harmonic_width(degree: int) -> int:
        # ONE derivation of the band width both ends read; a transcribed column is the drift this forecloses.
        return (degree + 1) * (degree + 1) * 3

    @property
    def digest(self) -> "RuntimeRail[ContentKey]":
        # content key over the five channel blocks in wire order — the identity the ArtifactSyncService frame addresses.
        # `parts` is the MODALITY, never a bare tuple of buffers: four semantic fields whose boundaries carry
        # meaning, so the identity owner's own length-and-count framing runs and a byte moving from the tail of one
        # channel to the head of the next changes the key. Concatenated undelimited, two different channel splits
        # over identical total octets hash the SAME — a live preimage collision on the one value the residency lane
        # dedupes on. A pre-lifted source rides `of`, since the bare `key` accessor admits no lifted case, so this
        # projection carries the rail its entry does and the framing width stays the identity owner's alone.
        return ContentIdentity.of(
            "gaussian-splat",
            IdentitySource(
                parts=(self.positions.tobytes(), self.scales.tobytes(), self.rotations.tobytes(), self.harmonics.tobytes(), self.alphas.tobytes())
            ),
        )

    def breached(self, grounding: SplatGrounding) -> Option[str]:
        # the same shape law the C# residency gate applies, proved at the PRODUCING end so a malformed container
        # names its offending channel here rather than crossing the wire and failing an admission the companion
        # could have prevented. The band ceiling arrives as the ROW that admitted this release rather than as a local
        # constant, so a published release raising its band can never pass one end of the wire and fail the other.
        # The ordered fold reports the FIRST breach, which is the one a reader acts on.
        width = SplatScan.harmonic_width(self.harmonic_degree)
        checks = (
            (self.splat_count > 0, f"count:{self.splat_count}"),
            (0 <= self.harmonic_degree <= grounding.degree, f"degree:{self.harmonic_degree}"),
            (grounding.activation is Activation.SIGMOID, f"activation:{grounding.activation.value}"),
            (self.positions.size >= self.splat_count * 3, "positions"),
            (self.scales.size >= self.splat_count * 3, "scales"),
            (self.rotations.size >= self.splat_count * 4, "rotations"),
            (self.harmonics.size >= self.splat_count * width, "harmonics"),
            (self.alphas.size >= self.splat_count, "alphas"),
        )
        return Block.of_seq(checks).choose(lambda row: Nothing if row[0] else Some(row[1])).try_head()


class ChannelCodec(StrEnum):
    # how a packed block becomes canonical float32. `AFFINE` covers every channel whose stored width IS a numpy
    # dtype and whose dequantization is one multiply-add. The other two exist because the container's own encoding
    # refuses that model: SPZ stores positions as 24-bit little-endian signed fixed point, a width no dtype names,
    # and rotations as a bit-packed smallest-three quaternion whose largest component is DERIVED rather than stored.
    # An affine row over either reads garbage silently — the block width alone would already be wrong — so the codec
    # is a declared column and a new container's encoding is one member and one arm.
    AFFINE = "affine"
    FIXED24 = "fixed24"  # 3-byte little-endian signed fixed point, `gain` the 2^-fractional_bits scale
    SMALLEST_THREE = "smallest-three"  # 2-bit largest-component index plus three 10-bit signed magnitudes


class ChannelSpec(Struct, frozen=True, gc=False):
    # one declared block per channel: components per splat, the packed dtype, the codec, and the affine
    # dequantization onto the canonical float32 regime — so a container format is a LAYOUT row and the unpack stays
    # one offset fold over declared columns rather than a per-format reader.
    # `Nothing` names a block the CONTAINER writes that the wire record carries no field for: the fold advances the
    # offset and keeps nothing, so an unheld block is a declared row rather than an offset a later row's reader
    # would have to bake in — and the day the wire record grows that field, the row gains a member and nothing else.
    channel: Option[SplatChannel]
    width: int  # 0 = derive from the header's harmonic band; the stored ELEMENT count per splat, never the byte size
    dtype: str
    gain: float
    bias: float
    codec: ChannelCodec = ChannelCodec.AFFINE


class SogVersion(Struct, frozen=True, gc=False):
    # the drift gate alone, decoded BEFORE the manifest shape: a v1 archive carries neither `quats` nor `sh0`, so
    # decoding the whole shape first would report a missing field where the honest refusal names the version. The
    # default IS the v1 discriminant, because v1 writes no version key at all.
    version: int = 0


class SogBlock(Struct, frozen=True, gc=False):
    # ONE manifest block shape spanning every property SOG v2 declares, because the properties differ only in which
    # columns they fill: `means` carries two planes and the per-axis span its 16-bit codes lerp inside, `scales` and
    # `sh0` a 256-entry codebook their plane channels index, `quats` a bare plane, and `shN` a centroid/label plane
    # pair beside its centroid census and band count. A struct per property would fork one wire shape five ways and
    # give the decode five readers over four inert columns; the defaults ARE the per-property inertness.
    files: tuple[str, ...]  # order is significant — `means` reads low plane then high, `shN` centroids then labels
    codebook: tuple[float, ...] = ()
    mins: tuple[float, float, float] = (0.0, 0.0, 0.0)
    maxs: tuple[float, float, float] = (0.0, 0.0, 0.0)
    count: int = 0
    bands: int = 0


class SogMeta(Struct, frozen=True, gc=False):
    # the SOG manifest as the container declares it. Unknown keys are ADMITTED rather than forbidden: the container
    # is externally versioned and `SogVersion` is the drift gate, so a v2 archive carrying a later optional block
    # this reader has no field for stays readable, exactly as the reference reader treats it — the manifest's
    # `antialias` flag is one such key, and the wire record carries no field it could land on.
    count: int
    means: SogBlock
    scales: SogBlock
    quats: SogBlock
    sh0: SogBlock
    shN: SogBlock | None = None  # absent ⇒ harmonic degree 0, the DC-only band


# co-located with the shapes they decode, because a `Decoder` binds its type at construction and one module-level
# instance serves every read.
_SOG_VERSION_DECODER: Final[Decoder[SogVersion]] = Decoder(type=SogVersion)
_SOG_META_DECODER: Final[Decoder[SogMeta]] = Decoder(type=SogMeta)


class SplatHeader(Struct, frozen=True, gc=False):
    # the container's OWN declaration, admitted rather than assumed: every format publishes its splat count and
    # harmonic band in-band, so the channel fold derives every extent from this record and no census literal is
    # hardcoded. `_container` is the signature-dispatched reader that produces it from raw bytes; the fold, the
    # shape gate, the content key, and the carrier ride below it.
    fmt: SplatFormat
    splat_count: int
    harmonic_degree: int
    fractional_bits: int = 0  # the SPZ FIXED24 resolution; a planar container stores no fixed point and reads the absent case


@tagged_union(frozen=True)
class SplatBody:
    # what a container hands its own decode fold, and the reason `_container` is not typed to `bytes`: SPZ resolves
    # to one concatenated block body an offset fold indexes, while SOG resolves to a parsed manifest beside the
    # named planes it addresses. Erasing both to `bytes` would force the planar arm to re-open the archive and
    # re-decode the manifest a second time, giving one fact two owners.
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
    farthest_points: int = 50_000  # the FARTHEST_POINT stage's budget; a census at or under it is the identity
    stream_chunk: int = 0  # 0 = whole-array execute(); positive streams iterator(chunk_size) when the graph is streamable, else degrades to execute()

    @property
    def range_limits(self) -> str:
        # `filters.range` grammar string derives from the typed axis/band pair, never a raw literal.
        lo, hi = self.range_band
        return f"{self.range_axis}[{lo:g}:{hi:g}]"


class StationFact(Struct, frozen=True, gc=False):
    # pose/acquisition surface the raw read path drops; native slots ride the receipt facts.
    guid: str
    points: int
    translation: tuple[float, float, float]


@tagged_union(frozen=True)
class ScanOp:
    # ONE surface over both directions and every source: the read verbs, the E57 append leg, and the companion
    # decode. A sibling `write_scan` entry would fork the floor gate, the receipt, and the weave three ways.
    tag: OpKind = tag()
    arrow_las: "PointRecordTable" = case()
    e57: str = case()
    e57_write: tuple[str, Cloud, tuple[StationFact, ...], Pose] = case()
    splat: bytes = case()  # raw container bytes; `_container` parses the preamble worker-side


@tagged_union(frozen=True)
class IngestProduct:
    # the closed outcome family: a cleaned cloud, a decoded splat scan, or the content key of what was written.
    tag: Literal["cloud", "splat", "written"] = tag()
    cloud: Cloud = case()
    splat: SplatScan = case()
    written: ContentKey = case()


class IngestReceipt(Struct, frozen=True, gc=False):
    # every measured slot that an arm may not measure rides a CARRIER, so absence spells itself and the fact
    # projection omits the key rather than publishing a ratio, a CRS, or a format census no read produced. The point
    # census joins them: the write and container arms run NO filter graph, so the retired `n, n` they passed made
    # every one of them report a perfect 1.0 decimation for a cleaning pass that never existed. `srs` is the one slot
    # `Option` cannot carry — a CRS the executed pipeline reported and a CRS inherited from the producing carrier are
    # both present and NOT the same fact — so it rides `Posture`, whose `defaulted` arm NAMES the source it came from.
    source: OpKind
    stages: tuple[IngestStage, ...]
    input_points: Option[int]
    output_points: Option[int]
    decimation: Option[float] = Nothing  # absent where no graph ran or the input carried no points, so no ratio was measured
    srs: Posture[str] = Posture(absent=None)
    dimensions: Option[int] = Nothing  # the executed pipeline's schema dimension census
    point_format: Option[int] = Nothing  # the LAS point-format id the carrier declared; absent off every other arm
    source_key: Option[ContentKey] = Nothing  # the producing decode's content key, chaining cleaning provenance back
    stations: tuple[StationFact, ...] = ()  # E57 per-station provenance; () off every other arm

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
        # the ratio exists only where BOTH ends of the census were measured and the input carried points, so a
        # missing half and a zero-point input answer the same absence rather than a fabricated or infinite ratio.
        ratio = input_points.bind(lambda seen: output_points.map(lambda kept: kept / seen) if seen else Nothing)
        return IngestReceipt(op.tag, applied, input_points, output_points, ratio, srs, dimensions, point_format, source_key, stations)

    @staticmethod
    @receipted(OPEN)  # no secret field in the facts, so the runtime keep-all policy binds
    def _emit(receipt: "IngestReceipt") -> "IngestReceipt":
        return receipt  # egress is the @receipted decorator rail.

    def facts(self) -> dict[str, object]:
        # native slots and tuple axes, so the receipts renderer serializes without a str() pre-coerce; an absent
        # measure leaves the map, because a dashboard reading `1.0` for "nothing was removed" reads a measurement
        # no pass took. ONE omit-fold walks every optional slot — the census pair included — so a new optional
        # measure is one roster pair and no arm re-spells a presence test. `srs` also publishes the SOURCE that
        # supplied it wherever the value was inherited rather than reported, which is the axis `Option` erases.
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
        # one `emitted`-phase row; the subject is the verb tag.
        return (Receipt.of("rasm.geometry.scan.ingestion", ("emitted", self.source.value, self.facts())),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _demanded(op: ScanOp) -> Block[str]:
    # the CONTAINER's provider demand, resolved off the payload's own leading signature rather than off the verb —
    # `splat` spans two formats and an SPZ payload decodes on the interpreter floor alone, so a verb-level image row
    # would refuse a decode the host can serve. The probe reads the same `_SIGNATURE` table `_container` dispatches
    # on, so one owner holds every magic and the gate can never disagree with the reader it gates.
    if op.tag is not OpKind.SPLAT:
        return Block.empty()
    return _signed(op.splat).bind(lambda row: _FORMAT_MODULE.try_find(row[1])).map(Block.singleton).default_value(Block.empty())


def _reached(op: ScanOp, policy: IngestPolicy) -> "RuntimeRail[ScanOp]":
    # the ONE floor gate, ahead of the offload, over every axis a marked provider enters on: the verb's own codec,
    # the container a byte-fed verb carries, and every rostered stage whose fold is native. A provider absent at
    # import refuses by MODULE NAME, so the operator reads a provisioning fact instead of a worker death carrying a
    # private path — and a policy naming a native stage on a host that lacks its distribution refuses at the same
    # seam its verb would, rather than surviving the gate and dying inside the graph.
    verb = _OP_MODULE.try_find(op.tag).map(Block.singleton).default_value(Block.empty())
    demanded = verb.append(_demanded(op)).append(Block.of_seq(policy.stages).choose(_STAGE_MODULE.try_find))
    unreached = demanded.filter(lambda module: module in _UNREACHED).try_head()
    return unreached.map(lambda module: Error(INGEST_UNREACHED.raised(module))).default_value(Ok(op))


@beartype(conf=FAULT_CONF)
def _structured(x: np.ndarray, y: np.ndarray, z: np.ndarray, rgb: tuple[np.ndarray, np.ndarray, np.ndarray] | None = None) -> np.ndarray:
    # a non-ndarray column raises the BeartypeCallHintViolation the fence lifts onto the rail; a color triple widens the
    # layout by the u16 band, a u8 source (E57 colors) scaling by 257 onto the LAS full range so one canonical
    # color regime crosses the pipeline regardless of source depth.
    out = np.empty(x.shape[0], dtype=_COLOR_DTYPE if rgb is not None else _DTYPE)
    out["X"], out["Y"], out["Z"] = x, y, z
    for band, values in zip(("Red", "Green", "Blue"), rgb or (), strict=False):
        arr = np.asarray(values)
        out[band] = arr.astype(np.uint16) * 257 if arr.dtype.kind in "iu" and np.iinfo(arr.dtype).max == 255 else arr
    return out


def _read_e57(path: str) -> tuple[np.ndarray, tuple[StationFact, ...]]:
    # read_scan(transform=True) is the conditioned intake: coordinate-system auto-detect, spherical
    # projection, invalid-state mask, and per-scan pose all applied.
    with pye57.E57(path, mode="r") as handle:
        stations = tuple(
            StationFact(str(h.guid), int(h.point_count), tuple(float(v) for v in h.translation))
            for h in (handle.get_header(index) for index in range(handle.scan_count))
        )
        scans = tuple(handle.read_scan(index, transform=True, colors=True, ignore_missing_fields=True) for index in range(handle.scan_count))
    # color survives only when EVERY station carries it — a half-attributed set drops the band whole, because a
    # cloud padded with fabricated zeros would poison the COLORED_ICP objective it exists to feed.
    colored = bool(scans) and all("colorRed" in scan for scan in scans)
    blocks = Block.of_seq(
        _structured(
            *(scan[f"cartesian{axis}"] for axis in ("X", "Y", "Z")),
            tuple(scan[f"color{band}"] for band in ("Red", "Green", "Blue")) if colored else None,
        )
        for scan in scans
    )
    # a scan-less E57 is the structural Nothing the empty-array arm lifts, never a falsy-Block guard.
    points = blocks.try_head().map(lambda _: np.concatenate(blocks)).default_value(np.empty(0, dtype=_DTYPE))
    return points, stations


def _write_e57(path: str, cloud: Cloud, stations: tuple[StationFact, ...], pose: Pose) -> ContentKey:
    # `write_scan_raw` is the provider's ONE append entry — no per-field write family — and the pose splits into the
    # rotation/translation pair it names; the returned key addresses the octets actually written, so an egress
    # receipt chains to the file rather than to the in-memory cloud it was derived from.
    matrix = np.reshape(np.asarray(pose, dtype=np.float64), (4, 4))
    data = {"cartesianX": cloud.positions[:, 0], "cartesianY": cloud.positions[:, 1], "cartesianZ": cloud.positions[:, 2]}
    with pye57.E57(path, mode="w") as handle:
        for station in stations:  # Exemption: the provider appends one scan node per call, its own streaming seam.
            handle.write_scan_raw(data, name=station.guid, rotation=matrix[:3, :3], translation=matrix[:3, 3])
    return ContentIdentity.key("e57", Path(path).read_bytes())


# --- [PACKED_DECODE]


def _fixed24(raw: np.ndarray, count: int, gain: float) -> np.ndarray:
    # 24-bit little-endian SIGNED fixed point: three `u1` lanes lift to the integer, then the sign bit at 2**23
    # folds the two's-complement wrap that a bare byte sum reads as a large positive coordinate. `gain` carries the
    # container's own `2 ** -fractional_bits` resolution, so the precision knob stays a header fact and not a literal.
    lanes = raw.reshape(count, 3).astype(np.int32)
    lifted = lanes[:, 0] | (lanes[:, 1] << 8) | (lanes[:, 2] << 16)
    return (lifted - ((lifted >> 23 & 1) << 24)).astype(np.float32) * gain


def _smallest_three(raw: np.ndarray, count: int) -> np.ndarray:
    # the quaternion's LARGEST component is not stored: a 2-bit index names which one it was and the other three
    # ride 10-bit signed magnitudes, so the fold rebuilds the missing component from the unit-norm constraint and
    # scatters all four back into declaration order. Reading the stored triple as x/y/z and deriving w — the older
    # first-three encoding — silently rotates every splat whose largest component was not w.
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
    # ONE offset fold over the format's declared layout: each block is `splat_count * width` elements of its packed
    # dtype, decoded by its own CODEC row onto the canonical float32 regime, and the harmonic row derives its width
    # from the header band rather than carrying a transcribed column. A new container is a `_LAYOUT` row; a new
    # encoding is one `ChannelCodec` member and one arm here.
    def decoded(spec: ChannelSpec, packed: np.ndarray, width: int) -> np.ndarray:
        match spec.codec:
            case ChannelCodec.AFFINE:
                return (packed.astype(np.float32) * spec.gain + spec.bias).reshape(header.splat_count, width)
            case ChannelCodec.FIXED24:
                # the scale is the HEADER's own `2 ** -fractional_bits`, never the row's gain column — the container
                # declares its resolution per file, so a static row cannot carry it and the row's gain stays inert.
                return _fixed24(packed, header.splat_count, 2.0**-header.fractional_bits).reshape(header.splat_count, width // 3)
            case ChannelCodec.SMALLEST_THREE:
                return _smallest_three(packed, header.splat_count)
            case _ as unreachable:
                assert_never(unreachable)

    def block(state: tuple[int, dict[SplatChannel, np.ndarray]], spec: ChannelSpec) -> tuple[int, dict[SplatChannel, np.ndarray]]:
        offset, held = state
        # the container's sh stream EXCLUDES the DC band (the packer's dimForDegree: 0/9/24/45 elements per splat),
        # so the derived harmonic width is the wire band minus the DC triple the COLOR block carries; degree 0
        # derives width 0 and reads nothing, exactly the zero-size stream the v4 packer skips.
        width = spec.width or SplatScan.harmonic_width(header.harmonic_degree) - 3
        packed = np.frombuffer(body, dtype=np.dtype(spec.dtype), count=header.splat_count * width, offset=offset)
        # an unheld block advances the offset and decodes nothing, so a container's own extra streams cost one row
        # rather than a baked-in skip inside the next row's offset.
        spec.channel.map(lambda channel: held.__setitem__(channel, decoded(spec, packed, width)))
        return offset + packed.nbytes, held

    return _LAYOUT[header.fmt].fold(block, (0, {}))[1]


# --- [PLANAR_DECODE]


def _plane(planes: Map[str, bytes], name: str, bands: int) -> np.ndarray:
    # the ONE plane-admission seam, owning every refusal a named member can carry — absent from the archive,
    # unrecognized, oversized, truncated mid-decode, or carrying fewer bands than its row declares — each named by
    # MEMBER, so an operator reads which plane broke rather than which wire channel noticed. `formats` pins the WebP
    # plugin, so an arbitrary archive member never walks the whole codec registry; `convert` narrows a wider source
    # to the declared band count, because a lossless WebP written without alpha opens `RGB` and a fixed `[..., 3]`
    # would index past its last band. `np.asarray` reads Pillow's `__array_interface__`, which hands over an owned
    # bytes copy rather than a view onto the decoder buffer, so the array outlives the window that produced it.
    payload = planes.try_find(name).default_with(lambda: _malformed(f"plane-absent:{name}"))
    try:
        with Image.open(BytesIO(payload), formats=_WEBP) as opened:
            if len(opened.getbands()) < bands:
                _malformed(f"plane-bands:{name}")
            return np.asarray(opened.convert(_PLANE_MODE[bands]))
    except UnidentifiedImageError:  # the narrower arm reads first: it subclasses OSError
        return _malformed(f"plane-unrecognized:{name}")
    except Image.DecompressionBombError:
        # the bomb ceiling raises from `open` itself, off the DECLARED dimensions and before a pixel decodes, and it
        # is no `OSError` — an arm short of it lets a hostile plane header cross the worker seam as a bare raise.
        return _malformed(f"plane-oversized:{name}")
    except OSError:
        return _malformed(f"plane-truncated:{name}")


def _rows(planes: Map[str, bytes], name: str, bands: int, count: int) -> np.ndarray:
    # the per-splat projection: the container's pixel order IS the splat order — `i = x + y * W` is exactly a
    # row-major flatten of the (H, W, bands) grid — so the read is one reshape and a head slice, and the trailing
    # pixels a plane pads its final row with fall away. A plane that cannot cover the census refuses by MEMBER,
    # where short-slicing would surface later as a shape-gate breach naming only the wire channel it fed.
    plane = _plane(planes, name, bands)
    flat = plane.reshape(-1, plane.shape[-1])
    if flat.shape[0] < count:
        _malformed(f"plane-extent:{name}")
    return flat[:count]


def _codebook(values: tuple[float, ...], name: str) -> np.ndarray:
    # a codebook is exactly the 8-bit index space its plane channel addresses, so its extent is admitted before the
    # lookup: a short table is an out-of-range fancy index, never a decode that merely reads slightly wrong.
    if len(values) != _SOG_CODEBOOK:
        _malformed(f"codebook:{name}")
    return np.asarray(values, dtype=np.float32)


def _lerped(low: np.ndarray, high: np.ndarray, mins: tuple[float, float, float], maxs: tuple[float, float, float]) -> np.ndarray:
    # positions are ONE value bit-split across two planes — `q = (u << 8) | l` at 16 bits per axis — so a
    # single-plane read is wrong by the whole high byte rather than merely coarse. The normalized code lerps inside
    # the manifest's own per-axis span and the packer's signed log inverse restores the coordinate; `expm1` is the
    # exact form of `exp(x) - 1` where the plain subtraction cancels catastrophically near the origin.
    base = np.asarray(mins, dtype=np.float32)
    q = (high.astype(np.uint32) << 8) | low.astype(np.uint32)
    n = base + (np.asarray(maxs, dtype=np.float32) - base) * (q.astype(np.float32) / 65535.0)
    return np.sign(n) * np.expm1(np.abs(n))


def _quats(plane: np.ndarray) -> np.ndarray:
    # SOG's smallest-three is NOT the SPZ codec, and reusing that arm rotates every splat whose omitted slot differs
    # — a divergence no shape gate can see: the omitted component is named by the ALPHA byte as an offset from
    # `_QUAT_MODE_BASE` rather than by a 2-bit field inside a packed word, the kept three ride full 8-bit channels
    # over the [-1/√2, 1/√2] window rather than 10-bit signed lanes, and only the derived component's positivity is
    # shared. An alpha outside the four declared modes is RESERVED by the container, so it refuses rather than
    # decoding through a mode the writer never meant.
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
    # the AC band, reached through DOUBLE indirection no other channel needs: a per-splat label selects a shared
    # centroid, the centroid's own channels are codebook indices, and the centroid plane is dimensioned by
    # coefficient rather than by splat — `_SOG_CENTROID_ROW` centroids per row, each holding `coeffs` consecutive
    # columns. The per-splat coefficient count DERIVES as the wire band minus its DC triple, so the widths the bands
    # imply are one derivation rather than a transcribed column the two ends can disagree on. An absent block yields
    # the zero-width array the DC head alone composes to `harmonic_width(0)`.
    if shN is None:
        return np.empty((count, 0), dtype=np.float32)
    coeffs = SplatScan.harmonic_width(degree) // 3 - 1
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
    # the `_channels` SIBLING: no concatenated body and no offset fold, because every channel is its own plane the
    # manifest NAMES, two of them reach their values through a codebook rather than an affine gain, positions span
    # two planes bit-combined, and the AC band indirects twice through a plane of different dimension — five
    # structural breaks a `_LAYOUT` row cannot express. The product is the same channel dict `_channels` yields, so
    # the DC head, the shape gate, and the framed key stay one owner above both containers.
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
        # the scale codebook is LOG-domain, so a stored index resolves to a log scale the exponential lifts.
        SplatChannel.SCALE: np.exp(_codebook(meta.scales.codebook, "scales")[_rows(planes, meta.scales.files[0], 3, count)]),
        SplatChannel.ROTATION: _quats(_rows(planes, meta.quats.files[0], 4, count)),
        # the DC coefficient is the codebook entry RAW: the `0.5 + c * SH_C0` a viewer applies is a RENDER transform,
        # and folding it here would ship a pre-lit colour where the wire's harmonic band declares a coefficient.
        SplatChannel.COLOR: _codebook(meta.sh0.codebook, "sh0")[sh0[:, :3]],
        SplatChannel.HARMONIC: _harmonics(meta.shN, planes, count, header.harmonic_degree),
        # the alpha byte is already the sigmoid-activated opacity the wire admits, exactly as the SPZ row reads it.
        SplatChannel.ALPHA: sh0[:, 3].astype(np.float32) / 255.0,
    }


# --- [CONTAINERS]


def _ngsp(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    # the v4 plaintext zone is `[header][extensions][TOC]` with the ZSTD frames concatenated after the TOC in
    # attribute order — positions, alphas, colors, scales, rotations, harmonics, zero-size attributes skipped —
    # which IS `_LAYOUT` row order, so the decompressed concatenation is exactly the body the offset fold indexes.
    # An extension zone between header and TOC is skipped by construction: the wire record declares no field for
    # it, the container-level analogue of a `Nothing`-channelled block. Admission bounds are the packer's own:
    # version window, `INT32_MAX` point cap with the compression-ratio floor, and the TOC seated inside the file.
    # The extent guard reads BEFORE the unpack, because `unpack_from` on a short buffer raises a `struct.error` the
    # lane would carry as a provider trace where a typed refusal naming the length is the honest answer.
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
        # each TOC pair is `(compressed, uncompressed)`; the frame's offset accumulates from the TOC end, and a
        # chunk expanding to any other length than its declared size is the packer's own stream-size refusal.
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
    # the pre-v4 gzip framing: one 16-byte header, then the attribute blocks in the same order the v4 streams take.
    # Version 3 is the one legacy layout the SPZ `_LAYOUT` row declares (24-bit fixed positions, smallest-three
    # rotations); v1 stores float16 positions and v1/v2 store first-three 3-byte rotations — encodings no declared
    # row carries — so those versions refuse by name rather than decoding garbage under a shape gate that cannot
    # see the difference, and a gzip body claiming a ZSTD-era version is out of the packer's own contract.
    if len(body) < _LEGACY_HEADER.size or body[:4] != _NGSP_MAGIC:
        _malformed("legacy:header")
    _magic, version, points, degree, fractional, _flags, _reserved = _LEGACY_HEADER.unpack_from(body)
    if version != _LEGACY_VERSION:
        _malformed(f"legacy-version:{version}")
    if not 0 < points <= 0x7FFFFFFF:
        _malformed(f"count:{points}")
    return SplatHeader(SplatFormat.SPZ_V4, points, degree, fractional), SplatBody(packed=body[_LEGACY_HEADER.size :])


def _manifest[T](decoder: Decoder[T], body: bytes) -> T:
    # the manifest's two wire-rejection paths stay distinct, because a constraint or discriminant miss is not
    # malformed JSON and collapsing them loses which one a reader must fix; the narrower arm reads first, since
    # `ValidationError` subclasses `DecodeError`.
    try:
        return decoder.decode(body)
    except ValidationError:
        return _malformed("sog-meta:constraint")
    except DecodeError:
        return _malformed("sog-meta:malformed")


def _sog(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    # the bundled `.sog` is a ZIP whose members sit at the archive ROOT with `meta.json` naming every other one, and
    # the raw-bytes ingress reaches exactly that bundled form — a loose plane directory is a filesystem source no
    # byte payload carries. The archive reads WHOLE into an in-memory map, so no handle outlives the window that
    # opened it, which is also what lets the manifest decode exactly once and ride onward on `SplatBody.planar`
    # instead of being re-read by the plane fold. Plane FILENAMES are arbitrary — the manifest names them and the
    # order inside each block's `files` is significant — so nothing here matches on an extension or a fixed name
    # beyond the one manifest member the container fixes.
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
    # the ONE magic read: the first `_SIGNATURE` row whose leading octets prefix the payload names both the format
    # the floor gate resolves a provider against and the reader that opens it, so the gate and the decode can never
    # disagree about what a payload is.
    return _SIGNATURE.filter(lambda row: raw[: len(row[0])] == row[0]).try_head()


def _container(raw: bytes) -> tuple[SplatHeader, SplatBody]:
    # raw container bytes -> the container's own declaration plus whatever body its decode fold consumes. SPZ stream
    # decompression is stdlib `compression.zstd`/`gzip` and the SOG archive read is stdlib `zipfile`, all
    # interpreter-floor surfaces, so the only distribution any container arm demands is the image codec the SOG
    # planes decode through, which `_FORMAT_MODULE` gates by signature ahead of the offload.
    return _signed(raw).map(lambda row: row[2](raw)).default_with(lambda: _malformed("container:unrecognized"))


def _splatted(header: SplatHeader, body: SplatBody) -> tuple[SplatScan, ContentKey]:
    # declaration first, channels second, gate third, key fourth: the header carries the container's own extents,
    # the body's own case selects the fold that produces the canonical channel dict, the shape gate proves the
    # result against the wire law before it leaves the companion, and the framed digest keys it. The key resolves
    # HERE beside the gate rather than at each consumer, so the two refusals a decoded container can carry — a
    # breached channel and an unreadable preimage — land on one seam and reach the lane fence through one raise.
    grounding = _grounded(header.fmt)
    match body:
        case SplatBody(tag="packed", packed=blob):
            channels = _channels(header, blob)
        case SplatBody(tag="planar", planar=(meta, planes)):
            channels = _planes(header, meta, planes)
        case _ as unreachable:
            assert_never(unreachable)
    scan = SplatScan(
        header.fmt.value,
        channels[SplatChannel.POSITION],
        channels[SplatChannel.SCALE],
        channels[SplatChannel.ROTATION],
        # the wire harmonic band leads with the DC triple the container stores as its own colour block, so the
        # composed width lands at exactly `harmonic_width(degree)` and the shape gate below proves it.
        np.concatenate((channels[SplatChannel.COLOR], channels[SplatChannel.HARMONIC]), axis=1),
        header.harmonic_degree,
        header.splat_count,
        channels[SplatChannel.ALPHA].reshape(-1),
    )
    scan.breached(grounding).map(lambda offending: _malformed(offending))
    return scan, scan.digest.default_with(lambda fault: _malformed(fault.tag))


def _grounded(fmt: SplatFormat) -> SplatGrounding:
    # the ONE read of the shared `SPLAT_FORMS` release matrix, raising INTO the lane fence: the wire owner declares
    # each published release's band ceiling and alpha activation, so no reader here transcribes a number the two ends
    # could drift on, and a release this branch DECLINES refuses by quoting the encoding it declines rather than
    # decoding garbage under a shape gate that cannot see the difference.
    return splat_form(fmt.value).default_with(lambda fault: _malformed(str(fault.facts().get("detail", fault.subject))))


def _malformed[T](offending: str) -> T:
    raise IngestFault(malformed=offending)


def _farthest(cloud: Cloud, policy: IngestPolicy) -> Cloud:
    # the geometry-uniform budget bound `pdal` cannot express: `filters.decimation` keeps every Nth point in FILE
    # order and `filters.voxeldownsize` keeps one point per occupied cell, so neither bounds a point BUDGET while
    # preserving shape — the exact guarantee a non-rigid solve needs, whose correspondence cost is quadratic in
    # points and whose deformation field is only as faithful as the sampling is uniform. A census at or under the
    # budget is the IDENTITY, so an over-budget policy costs nothing rather than resampling a cloud it cannot shrink.
    return (
        cloud
        if policy.farthest_points <= 0 or len(cloud) <= policy.farthest_points
        else Cloud.of_legacy(cloud.legacy().farthest_point_down_sample(policy.farthest_points))
    )


def _piped(policy: IngestPolicy) -> Option[Block["pdal.Filter"]]:
    # the injected `Filter` class threads into each `_STAGE`/`_FILTER` closure, so the tables never resolve an
    # unbound `pdal` global and the proxy reifies exactly here, at the one place a `_STAGE` row is invoked. Only the
    # FILTER rows compose the pipe; a carrier row contributes no stage to it.
    built = Block.of_seq(policy.stages).choose(_STAGE.try_find).map(lambda build: build(pdal.Filter, policy))
    return built.try_head().map(lambda _: built)


def _execute(stages: Block["pdal.Filter"], points: np.ndarray, chunk: int) -> tuple[np.ndarray, str, int]:
    # array enters once at the head stage's `.pipeline(points)` wrap, never a redundant Reader re-read.
    pipeline = stages.tail().fold(lambda acc, stage: acc | stage, stages.head().pipeline(points))
    if chunk and pipeline.streamable:
        # iterator() requires every composed stage streamable; a blocking stage degrades to execute().
        cleaned = np.concatenate(tuple(pipeline.iterator(chunk_size=chunk)))
    else:
        pipeline.execute()
        cleaned = pipeline.arrays[0]
    schema = pipeline.schema.get("schema", {}).get("dimensions", ())
    # a pipeline reporting no SRS text answers ABSENCE, never the empty string a consumer would render as a CRS.
    return cleaned, Some(text) if (text := str(pipeline.srswkt2 or "")) else Nothing, Some(len(schema))


def _filter_graph(points: np.ndarray, policy: IngestPolicy) -> tuple[Cloud, Option[str], Option[int]]:
    # a policy naming NO stage runs no pipeline at all, so neither the SRS nor the dimension census exists to report
    # — the retired `("", 0)` published an empty CRS and a zero-wide schema as though a graph had measured them.
    cleaned, srs, dims = (
        _piped(policy).map(lambda stages: _execute(stages, points, policy.stream_chunk)).default_value((points, Nothing, Nothing))
    )
    # color band surviving the filter graph lands unit-scaled on the Cloud; normals stay absent by
    # construction — neither LAS nor pye57 exposes them, so the registration owner's estimation stage mints them.
    colors = (
        np.column_stack((cleaned["Red"], cleaned["Green"], cleaned["Blue"])).astype(np.float64) / 65535.0
        if {"Red", "Green", "Blue"} <= set(cleaned.dtype.names or ())
        else np.empty((0, 3))
    )
    carrier = Cloud(positions=np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float64), colors=colors)
    # the pipe runs FIRST as one execute over the structured array, then the carrier folds run in policy order over
    # the `Cloud` it produced. The split is structural rather than a preference: a `pdal` stage consumes and yields a
    # structured array inside one pipeline, so a carrier fold cannot sit between two of them without breaking the
    # pipe into two executes and paying a second full pass. `output_points` therefore reads the CARRIER's census,
    # not the pipeline array's, since a fold that dropped points would otherwise report a decimation it did not make.
    return Block.of_seq(policy.stages).choose(_CARRIER.try_find).fold(lambda held, fold: fold(held, policy), carrier), srs, dims


def _cleaned(
    points: np.ndarray, op: ScanOp, policy: IngestPolicy, stations: tuple[StationFact, ...], carrier: "Option[PointRecordTable]"
) -> tuple[IngestProduct, IngestReceipt]:
    # one egress fold both reading arms share, and one POSTURE over the CRS: the executed pipeline's own `srswkt2` is
    # DECLARED, the producing carrier's already-admitted `crs` is DEFAULTED and NAMES itself as the source, and a run
    # with neither is absent outright. Three states one empty string fused into one, and the fused form let a
    # consumer read an inherited CRS as one this cleaning pass measured.
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


def _ingest_kernel(op: ScanOp, policy: IngestPolicy) -> tuple[IngestProduct, IngestReceipt]:
    # module-level HOSTILE kernel: ships REFERENCE, runs on the warm process pool, and returns only picklable material.
    match op:
        case ScanOp(tag=OpKind.ARROW_LAS, arrow_las=carrier):
            # the carrier crosses WHOLE — point format, CRS, and content key ride beside the table, so the receipt
            # publishes what the data owner already parsed instead of re-deriving an empty CRS downstream. A
            # color-bearing LAS point format rides its band into the layout; a colorless one yields the bare X/Y/Z
            # dtype rather than a fabricated zero band.
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
            # the write leg runs NO filter graph, so it has no INPUT census to compare against: the retired
            # `len(cloud), len(cloud)` published a before-and-after pair the arm never measured and a 1.0 decimation
            # no pass performed. What it does measure is how many points it WROTE, which is the output half alone.
            key = _write_e57(path, cloud, stations, pose)
            return IngestProduct(written=key), IngestReceipt.of(op, (), Nothing, Some(len(cloud)), Posture(absent=None), Nothing, stations)
        case ScanOp(tag=OpKind.SPLAT, splat=raw):
            # the container arm decodes; it cleans nothing, so the decoded EXTENT is the output half alone and never
            # a before-and-after pair, and a splat container declares no coordinate reference at all.
            scan, key = _splatted(*_container(raw))
            return IngestProduct(splat=scan), IngestReceipt.of(
                op, (), Nothing, Some(scan.splat_count), Posture(absent=None), Nothing, (), source_key=Some(key)
            )
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanIngestion(Struct, frozen=True):
    lane: LanePolicy
    policy: IngestPolicy = IngestPolicy()
    composition: ScopeKey = DEFAULT_SCOPE  # the custody key the weave and its harvest stamp

    async def run(self, op: ScanOp) -> "RuntimeRail[tuple[IngestProduct, IngestReceipt]]":
        # floor gate BEFORE the weave: an unprovisioned verb is a refusal about the host, not a measured crossing,
        # so it never opens an evidence span it would immediately close on an import fault. `partial` keeps the
        # dispatch a coroutine function the weave's modality probe reads; HOSTILE is the declared trait because the
        # pdal/pye57 band holds process-global native state and imports under no isolated subinterpreter.
        match _reached(op, self.policy):
            case Result(tag="ok", ok=admitted):
                rail = await evidence_run(
                    EvidenceScope.SCAN_INGESTION,
                    f"run.{admitted.tag.value}",
                    partial(self.lane.offload, Kernel.of(_ingest_kernel, KernelTrait.HOSTILE), admitted, self.policy),
                    composition=self.composition,
                )
                return rail.map(lambda pair: (pair[0], IngestReceipt._emit(pair[1])))
            case Result(tag="error") as refused:
                return refused


# --- [TABLES] ---------------------------------------------------------------------------

# one builder per driver over the injected `Filter.<name>` staticmethods; the SMRF `window` versus
# PMF `max_window_size` spelling is one row arm, so a raw `Filter(type=...)` is rejected.
_FILTER: Final[Map[IngestFilter, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestFilter.SMRF, lambda flt, p: flt.smrf(window=p.ground_window, cell=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.PMF, lambda flt, p: flt.pmf(max_window_size=p.ground_window, cell_size=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.OUTLIER, lambda flt, p: flt.outlier(method="statistical", mean_k=p.outlier_mean_k, multiplier=p.outlier_multiplier)),
    (IngestFilter.DECIMATION, lambda flt, p: flt.decimation(step=p.decimate_step)),
    (IngestFilter.VOXELDOWNSIZE, lambda flt, p: flt.voxeldownsize(cell=p.voxel_cell)),
    (IngestFilter.RANGE, lambda flt, p: flt.range(limits=p.range_limits)),
])

# one builder per PIPE stage threading the injected `Filter` class through to `_FILTER`: the swappable rows read the
# policy override, the fixed rows bind their `_FILTER` row directly. A stage absent here is a CARRIER stage resolved
# on `_CARRIER` instead — the two tables partition the vocabulary rather than one table carrying a union case per
# row, so neither fold ever holds a row of the other kind and a new stage lands in exactly one of them.
_STAGE: Final[Map[IngestStage, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, lambda flt, p: _FILTER[p.ground_filter](flt, p)),
    (IngestStage.OUTLIER_REMOVE, _FILTER[IngestFilter.OUTLIER]),
    (IngestStage.DECIMATE, lambda flt, p: _FILTER[p.decimate_filter](flt, p)),
    (IngestStage.RANGE_CROP, _FILTER[IngestFilter.RANGE]),
])

# the carrier-level folds: a stage whose transform no `pdal` driver expresses, applied to the `Cloud` the pipe
# produced, in policy order. Each row names its provider on `_STAGE_MODULE` so the floor gate refuses it by name
# ahead of the offload exactly as it refuses a verb.
_CARRIER: Final[Map[IngestStage, Callable[[Cloud, IngestPolicy], Cloud]]] = Map.of_seq([(IngestStage.FARTHEST_POINT, _farthest)])

# one channel layout per container, ordered as the packed body stores the blocks. Channel identity and the
# component counts (3/3/4 plus the derived harmonic band) are the wire contract's; the packed dtype, the codec, and
# the dequantization columns are the CONTAINER's, read from its own published encoding.
#
# The SPZ row is measured against the format's own packer: positions are nine `u1` lanes forming three 24-bit
# little-endian signed fixed-point coordinates whose scale the FIXED24 arm reads as `2 ** -fractional_bits` off the
# header (the row's gain column is inert for that codec), so the row's
# `width` is the stored ELEMENT count and the codec re-groups it; alphas are the sigmoid-activated opacity byte
# (`byte / 255` IS the [0, 1] opacity — the packer writes `sigmoid(logit) * 255`); DC colours decode as
# `(byte / 255 - 0.5) / 0.15` (the packer's `colorScale = 0.15` wide-RGB window), folded onto the HARMONIC head at
# `_splatted` because the wire's `(degree + 1)^2 * 3` band width counts the DC triple the container stores apart;
# scales are `u8` log-encoded as `byte / 16 - 10`; rotations are one 32-bit smallest-three word per splat;
# harmonics are `u8` over `(byte - 128) / 128` and the container stream EXCLUDES the DC band, so the fold derives
# its width as the wire band MINUS the DC triple. Block ORDER is the packer's, not the wire record's.
_LAYOUT: Final[Map[SplatFormat, Block[ChannelSpec]]] = Map.of_seq([
    (
        SplatFormat.SPZ_V4,
        Block.of_seq((
            ChannelSpec(Some(SplatChannel.POSITION), 9, "u1", 1.0, 0.0, ChannelCodec.FIXED24),
            ChannelSpec(Some(SplatChannel.ALPHA), 1, "u1", 1.0 / 255.0, 0.0),  # sigmoid-activated opacity in [0, 1]
            ChannelSpec(Some(SplatChannel.COLOR), 3, "u1", 1.0 / (255.0 * 0.15), -0.5 / 0.15),  # SH DC coefficient triple
            ChannelSpec(Some(SplatChannel.SCALE), 3, "u1", 1.0 / 16.0, -10.0),
            ChannelSpec(Some(SplatChannel.ROTATION), 4, "u1", 1.0, 0.0, ChannelCodec.SMALLEST_THREE),
            ChannelSpec(Some(SplatChannel.HARMONIC), 0, "u1", 1.0 / 128.0, -1.0),
        )),
    ),
])

# one row per container signature: the leading octets a payload declares itself with, the format they name, and the
# reader that opens it. `_container` dispatches on this table and `_demanded` reads its format column for the floor
# gate, so one owner holds every magic and no second probe can disagree with the reader it gates. Row order is read
# order, and the three prefixes are mutually disjoint, so no row shadows another; only the SPZ rows share a format,
# because the v4 preamble-and-TOC read and the gzip-framed legacy body decode to one packed layout.
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
