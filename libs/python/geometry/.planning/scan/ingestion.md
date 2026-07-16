# [PY_GEOMETRY_SCAN_INGESTION]

`ScanIngestion` is the registration-ready front door of the host-free scan companion — the raw-scan cleaning the `data` branch deliberately declines. One frozen owner discriminates over a `ScanSource` `@tagged_union` whose two cases each carry their own decode: `arrow_las` holds the `pyarrow.Table` columnar point-record bridge from `data/spatial/mesh.md#POINTCLOUD` (LAS/LAZ/COPC already decoded), `e57` holds the `pye57` source path the columnar bridge does not own. Both converge on one composable `pdal` filter graph and one registration-ready `o3d.t.geometry.PointCloud` egress. The graph is policy, not code — an `IngestStage` row sequence folded over the `pdal` `|` pipe, so the order and membership of ground classification, outlier removal, downsampling, and range cropping are `IngestPolicy` rows a rebuild reorders without touching the fold, and a block-scale cloud rides the streaming arm when `IngestPolicy.stream_chunk` is non-zero.

The `async` entry keeps the multi-second SMRF/voxel sweep off the event loop: `ingest` composes the graduation `evidence_run` weave (span + fence + receipt harvest, `EvidenceScope.SCAN_INGESTION` the seed) around `lane.offload(_ingest_kernel, ...)`, and the `pdal` stage graph survives the process-modality fallback through the pipeline JSON's own pickle state. Emission rides the `@receipted(_REDACTION)` aspect over `IngestReceipt._emit`, harvesting `contribute` on exit. The receipt names what was cleaned, where it sat, and which stations produced it — the realized stage axis, input/output counts, decimation ratio, the executed pipeline's `srswkt2`/`schema` metadata, and the per-station E57 provenance off the typed `ScanHeader`. The cleaned cloud is the precondition `scan/registration.md#REGISTRATION` consumes across a same-folder read-only seam.

## [01]-[INDEX]

- [01]-[INGESTION]: source-discriminated raw-scan preprocessing — the `ScanSource` intake folded through one policy-ordered `pdal` `|` filter graph to a registration-ready `open3d` point cloud, offloaded under the graduation weave.

## [02]-[INGESTION]

- Owner: `ScanIngestion` the frozen dispatch owner; the `ScanSource` tag IS the decode-carrying discriminant (`arrow_las` the `pyarrow.Table` bridge, `e57` the `pye57` path), never an untyped `object` token paired with a separate enum. `IngestReceipt` conforms structurally to the runtime `ReceiptContributor` through its own `contribute()`, never importing the port as a base.
- Cases: `ScanSource` arms `arrow_las` (the data-branch `pyarrow.Table` bridge, `x`/`y`/`z` plus the LAS columns already decoded) and `e57` (the `pye57` structured multi-scan source read per-scan with its acquisition pose applied and `ScanHeader` provenance harvested), matched by `match`/`assert_never`, converging on the shared `_filter_graph` and one egress. `IngestStage` rows — `GROUND_CLASSIFY` (`SMRF` default / `PMF` alternate, swap on `IngestPolicy.ground_filter`), `OUTLIER_REMOVE`, `DECIMATE` (`DECIMATION` / `VOXELDOWNSIZE`, swap on `IngestPolicy.decimate_filter`), `RANGE_CROP` — each build their `pdal.Filter` through one `_STAGE` row, the swappable rows dispatching to the policy-chosen `_FILTER` factory, so a stage's driver and its option dict are one row read, never a type match plus a parallel option match. `IngestFilter` is the closed `filters.*` driver vocabulary whose `.value` is the driver string.
- Entry: `ingest` is `async`, admits one `ScanSource`, and returns `RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]` by composing `evidence_run(EvidenceScope.SCAN_INGESTION, f"ingest.{source.tag}", partial(lane.offload, _ingest_kernel, source, policy))`. `_ingest_kernel` is module-level and picklable — the lane imports neither `pdal` nor `pye57` nor the kernel, and a decode/filter raise inside the hop converts through the lane's `async_boundary` onto the recorded rail. The interior runs the source through one `match`: `arrow_las` folds the Arrow columns into the `pdal`-shaped structured array through the `@beartype(conf=FAULT_CONF)` `_structured` fence, `e57` folds every pose-applied scan block plus its provenance through `_read_e57`, and both reach `_filter_graph`.
- Auto: `_filter_graph` folds the graph through one `Option` rail — `_stages` builds the `IngestPolicy.stages` sequence through the `_STAGE` table and gates emptiness via `try_head`, so an empty policy is the structural `Nothing` `default_value` lifts the raw array through, never an `and` truthiness branch. The present-graph arm binds the array once at the head stage's `.pipeline(points)` wrap (never a `Reader` re-read), folds the tail over `|`, then selects execution by `stream_chunk`: zero runs whole-array `execute()`, a positive chunk runs `iterator(chunk_size)` when the composed graph is streamable and degrades to `execute()` otherwise, since a blocking stage (smrf/pmf/outlier/voxeldownsize) cannot stream. The `import pdal` side-effect that binds the injected `Filter.<name>` factories lives in `_stages`, the one place a `_STAGE` row is invoked, so the boundary import completes before any factory reference.
- Receipt: `IngestReceipt.of` derives the decimation ratio from the counts and integer-narrows at one factory; `facts` emits the native slots and tuple axes once; `_emit` carries the `@receipted(_REDACTION)` aspect. Ingestion mints no graduation subject — the cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, so there is no `scan-ingestion` member on the `rasm.geometry.graduation` `GeometrySubject` union.
- Packages: `pdal` (the injected `Filter.smrf`/`.pmf`/`.outlier`/`.decimation`/`.voxeldownsize`/`.range` factories, the `|` pipe composition, the `execute()`/`iterator(chunk_size)` runs, and the `srswkt2`/`schema` output metadata; the pipeline-JSON pickle state rides the modality fallback), `pye57` (`E57` context-manager open, `read_scan(transform=True)` the conditioned global-frame intake, `get_header` the typed `ScanHeader`), `open3d` (the `o3d.t.geometry.PointCloud` egress tensor), `pyarrow` (the bridge columns), `numpy` (structured-array assembly over the shared `_DTYPE`), `expression` (`Block`/`Map`/`Option` folds), `beartype` (the `_structured` fence), `msgspec` (frozen carriers), the geometry `evidence_run`/`EvidenceScope` weave, and the runtime `RuntimeRail`/`LanePolicy.offload`/`Receipt`/`receipted` rails. `laspy` is consumed transitively through the data-branch bridge, never imported here; the three compiled scan packages (`pdal`/`pye57`/`open3d`) import function-local at boundary scope.
- Growth: a new cleaning stage is one `IngestStage` member plus one `_STAGE` row (and one `_FILTER` row when a new driver backs it); a new driver alternative on a swappable stage is one `IngestFilter` member plus one `_FILTER` row plus the policy default; a new source format is one `ScanSource` case plus one dispatch arm; a new output-metadata fact is one `facts` slot read off the executed pipeline.
- Boundary: the inbound LAS/LAZ/COPC decode and the `pyarrow.Table` bridge are `data/spatial/mesh.md#POINTCLOUD`'s (`laspy` full decode and the COPC octree subset live there), so ingestion never re-reads LAS nor crosses a `pdal` `Pipeline` at the data seam; the E57 path is legitimately ingestion's because `pye57` is absent from the data branch, but the E57 write path (`write_scan_raw`) is declined — scan egress is the data seam's. Registration is `scan/registration.md#REGISTRATION`'s; ingestion never registers, deviates, reconstructs, tessellates, stores, or mutates a Rhino/GH document.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from typing import Final, Literal, assert_never

import numpy as np
from beartype import beartype
from expression import Option, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceScope, evidence_run
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------


class IngestStage(StrEnum):
    GROUND_CLASSIFY = "ground-classify"
    OUTLIER_REMOVE = "outlier-remove"
    DECIMATE = "decimate"
    RANGE_CROP = "range-crop"


# `.value` is the `filters.*` driver string the swappable stages select over.
class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


# `arrow_las` is quoted so the compiled `pyarrow` import stays at boundary scope.
@tagged_union(frozen=True)
class ScanSource:
    tag: Literal["arrow_las", "e57"] = tag()
    arrow_las: "pa.Table" = case()
    e57: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# no secret field in the facts, so the @receipted egress binds the keep-all redaction.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# the one structured layout the Arrow bridge and E57 blocks both fill; `pdal` reads X/Y/Z.
_DTYPE: Final = np.dtype([(axis, np.float64) for axis in ("X", "Y", "Z")])

# --- [MODELS] ---------------------------------------------------------------------------


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
    stream_chunk: int = 0  # 0 = whole-array execute(); positive streams iterator(chunk_size) when the graph is streamable, else degrades to execute()

    @property
    def range_limits(self) -> str:
        # the `filters.range` grammar string derives from the typed axis/band pair, never a raw literal.
        lo, hi = self.range_band
        return f"{self.range_axis}[{lo:g}:{hi:g}]"


class StationFact(Struct, frozen=True, gc=False):
    # the pose/acquisition surface the raw read path drops; native slots ride the receipt facts.
    guid: str
    points: int
    translation: tuple[float, float, float]


class IngestReceipt(Struct, frozen=True, gc=False):
    source: Literal["arrow_las", "e57"]
    stages: tuple[IngestStage, ...]
    input_points: int
    output_points: int
    decimation: float = 1.0
    srs: str = ""  # the executed pipeline's srswkt2 text; empty for an SRS-less in-memory array
    dimensions: int = 0  # the executed pipeline's schema dimension census
    stations: tuple[StationFact, ...] = ()  # E57 per-station provenance; () on the arrow_las arm

    @staticmethod
    def of(
        source: ScanSource,
        applied: tuple[IngestStage, ...],
        input_points: int,
        output_points: int,
        srs: str,
        dimensions: int,
        stations: tuple[StationFact, ...],
    ) -> "IngestReceipt":
        ratio = output_points / input_points if input_points else 1.0
        return IngestReceipt(source.tag, applied, int(input_points), int(output_points), ratio, srs, dimensions, stations)

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: "IngestReceipt") -> "IngestReceipt":
        return receipt  # egress is the @receipted decorator rail.

    def facts(self) -> dict[str, object]:
        # native slots and tuple axes, so the receipts renderer serializes without a str() pre-coerce.
        return {
            "source": self.source,
            "stages": tuple(s.value for s in self.stages),
            "input_points": self.input_points,
            "output_points": self.output_points,
            "decimation": self.decimation,
            "srs": self.srs,
            "dimensions": self.dimensions,
            "stations": tuple((s.guid, s.points, s.translation) for s in self.stations),
        }

    def contribute(self) -> tuple[Receipt, ...]:
        # one `emitted`-phase row; the subject is the source tag.
        return (Receipt.of("geometry.scan.ingestion", ("emitted", self.source, self.facts())),)


# --- [OPERATIONS] -----------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _structured(x: np.ndarray, y: np.ndarray, z: np.ndarray) -> np.ndarray:
    # a non-ndarray column raises the BeartypeCallHintViolation the fence lifts onto the rail.
    out = np.empty(x.shape[0], dtype=_DTYPE)
    out["X"], out["Y"], out["Z"] = x, y, z
    return out


def _read_e57(path: str) -> tuple[np.ndarray, tuple[StationFact, ...]]:
    import pye57  # noqa: PLC0415

    # read_scan(transform=True) is the conditioned intake: coordinate-system auto-detect, spherical
    # projection, invalid-state mask, and per-scan pose all applied.
    with pye57.E57(path, mode="r") as handle:
        stations = tuple(
            StationFact(str(h.guid), int(h.point_count), tuple(float(v) for v in h.translation))
            for h in (handle.get_header(index) for index in range(handle.scan_count))
        )
        blocks = Block.of_seq(
            _structured(*(scan[f"cartesian{axis}"] for axis in ("X", "Y", "Z")))
            for scan in (handle.read_scan(index, transform=True) for index in range(handle.scan_count))
        )
    # a scan-less E57 is the structural Nothing the empty-array arm lifts, never a falsy-Block guard.
    points = blocks.try_head().map(lambda _: np.concatenate(blocks)).default_value(np.empty(0, dtype=_DTYPE))
    return points, stations


def _stages(policy: IngestPolicy) -> Option[Block["pdal.Filter"]]:
    import pdal  # noqa: PLC0415  binds the injected Filter.<name> factories before any _STAGE row call

    # the injected `Filter` class threads into each `_STAGE`/`_FILTER` closure, so the tables never
    # resolve an unbound `pdal` global.
    built = Block.of_seq(_STAGE[stage](pdal.Filter, policy) for stage in policy.stages)
    return built.try_head().map(lambda _: built)


def _execute(stages: Block["pdal.Filter"], points: np.ndarray, chunk: int) -> tuple[np.ndarray, str, int]:
    # the array enters once at the head stage's `.pipeline(points)` wrap, never a redundant Reader re-read.
    pipeline = stages.tail().fold(lambda acc, stage: acc | stage, stages.head().pipeline(points))
    if chunk and pipeline.streamable:
        # iterator() requires every composed stage streamable; a blocking stage degrades to execute().
        cleaned = np.concatenate(tuple(pipeline.iterator(chunk_size=chunk)))
    else:
        pipeline.execute()
        cleaned = pipeline.arrays[0]
    schema = pipeline.schema.get("schema", {}).get("dimensions", ())
    return cleaned, str(pipeline.srswkt2 or ""), len(schema)


def _filter_graph(points: np.ndarray, policy: IngestPolicy) -> tuple["o3d.t.geometry.PointCloud", np.ndarray, str, int]:
    import open3d as o3d  # noqa: PLC0415

    cleaned, srs, dims = (
        _stages(policy).map(lambda stages: _execute(stages, points, policy.stream_chunk)).default_value((points, "", 0))
    )
    positions = np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float32)
    cloud = o3d.t.geometry.PointCloud()
    cloud.point.positions = o3d.core.Tensor(positions)
    return cloud, cleaned, srs, dims


def _ingest_kernel(source: ScanSource, policy: IngestPolicy) -> tuple["o3d.t.geometry.PointCloud", IngestReceipt]:
    # the module-level picklable kernel the lane offloads.
    match source:
        case ScanSource(tag="arrow_las", arrow_las=table):
            points = _structured(
                table.column("x").to_numpy(zero_copy_only=False),
                table.column("y").to_numpy(zero_copy_only=False),
                table.column("z").to_numpy(zero_copy_only=False),
            )
            stations: tuple[StationFact, ...] = ()
        case ScanSource(tag="e57", e57=path):
            points, stations = _read_e57(path)
        case _ as unreachable:
            assert_never(unreachable)
    cloud, cleaned, srs, dims = _filter_graph(points, policy)
    return cloud, IngestReceipt.of(source, policy.stages, points.shape[0], cleaned.shape[0], srs, dims, stations)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanIngestion(Struct, frozen=True):
    lane: LanePolicy
    policy: IngestPolicy = IngestPolicy()

    async def ingest(self, source: ScanSource) -> "RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]":
        # `partial` keeps the dispatch a coroutine function the weave's modality probe reads.
        rail = await evidence_run(
            EvidenceScope.SCAN_INGESTION, f"ingest.{source.tag}", partial(self.lane.offload, _ingest_kernel, source, self.policy)
        )
        return rail.map(lambda pair: (pair[0], IngestReceipt._emit(pair[1])))


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

# one builder per stage threading the injected `Filter` class through to `_FILTER`: the swappable
# rows read the policy override, the fixed rows bind their `_FILTER` row directly.
_STAGE: Final[Map[IngestStage, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, lambda flt, p: _FILTER[p.ground_filter](flt, p)),
    (IngestStage.OUTLIER_REMOVE, _FILTER[IngestFilter.OUTLIER]),
    (IngestStage.DECIMATE, lambda flt, p: _FILTER[p.decimate_filter](flt, p)),
    (IngestStage.RANGE_CROP, _FILTER[IngestFilter.RANGE]),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
