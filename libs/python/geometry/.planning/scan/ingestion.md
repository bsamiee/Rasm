# [PY_GEOMETRY_SCAN_INGESTION]

Raw-scan preprocessing — the registration-ready front door of the host-free scan companion, the cleaning the `data` branch deliberately does not run. `ScanIngestion` is one frozen owner discriminating over a `ScanSource` `@tagged_union` whose two cases each carry the decode that owns them — an `arrow_las` case holding the `pyarrow.Table` columnar point-record bridge from `data/spatial/mesh.md#POINTCLOUD` (LAS/LAZ/COPC already decoded, never re-read here) and an `e57` case holding the `pye57` source path the columnar bridge does not own — converging on one composable `pdal` filter graph and one registration-ready `o3d.t.geometry.PointCloud` egress. The graph is folded from an `IngestStage` row sequence over the `pdal` `|` pipe operator, not a fixed pipeline, so the order and membership of the cleaning steps — SMRF/PMF ground classification, statistical/radius outlier removal, voxel/decimation downsampling, range cropping — are policy, not code, and every stage builds its `pdal` `Filter` through one `_STAGE` builder row over the injected typed driver factories rather than a parallel type-lookup and option-build method pair. Block-scale clouds ride the streaming arm: a non-zero `IngestPolicy.stream_chunk` runs the graph through `Pipeline.iterator(chunk_size)` so a cloud larger than memory folds chunk-by-chunk, the whole-array `execute()` the zero-chunk default — one policy row, never a second entrypoint.

The entry is `async` and the multi-second SMRF/voxel sweep never runs on the event loop: `ingest` composes the graduation `evidence_run` weave (span + fence + receipt harvest, `EvidenceScope.SCAN_INGESTION` the seed row — no page-local tracer mint) around `lane.offload(_ingest_kernel, ...)`, the runtime per-subinterpreter hop the kernel rides as a module-level picklable callable; the `pdal` stage graph additionally survives the process-modality fallback because the pipeline JSON round-trips its own `__getstate__`/`__setstate__` pickle state. Emission rides the `@receipted(_REDACTION)` aspect on `IngestReceipt._emit` harvesting `contribute` on exit (the sibling parity), never an inline `Signals.emit`. The receipt carries the run's output metadata as facts — the realized stage axis, the input/output counts, the decimation ratio, the `srswkt2` spatial-reference text and the `schema` dimension census the executed pipeline reports, and the per-station E57 provenance (`ScanHeader` guid, point count, translation) the typed header surface yields — so the ingest evidence names what was cleaned, where it sat, and which stations produced it. The cleaned cloud is the precondition the `scan/registration.md#REGISTRATION` `register` rail consumes (a same-folder read-only seam); ingestion never registers, never deviates, never tessellates, and never re-owns LAS decode.

## [01]-[INDEX]

- [01]-[INGESTION]: source-discriminated raw-scan preprocessing under one owner over the `ScanSource` tagged-union intake, the `IngestStage`-keyed `pdal` `|` filter graph folded from one `_STAGE` builder table over the injected `Filter.<name>` factories with the policy-gated streaming arm, the `pye57` E57 structured-scan path with typed `ScanHeader` provenance, egressing a registration-ready `o3d.t.geometry.PointCloud` — the entry an `async` composition of the graduation `evidence_run` weave over `lane.offload`, the `@beartype(conf=FAULT_CONF)` `_structured` contract fence, the `Option`-folded `_stages`/`_execute` filter-graph split, and the `@receipted(_REDACTION)` egress aspect over `IngestReceipt._emit`.

## [02]-[INGESTION]

- Owner: `ScanIngestion` — the frozen owner discriminating over the `ScanSource` `@tagged_union` carrying the inbound decode per case (`arrow_las` the `pyarrow.Table` bridge, `e57` the `pye57` source path), so the source IS the payload-carrying discriminant rather than an untyped `object` token paired with a separate enum; `IngestStage` the `pdal` filter-graph stage vocabulary the realized-order receipt records; `IngestFilter` the bounded `filters.*` driver vocabulary the swappable stages select over (a closed `StrEnum` whose `.value` is the `filters.*` driver string, never a stringly `ground_filter: str` policy field); `_FILTER` the one `Map[IngestFilter, Callable[[type[pdal.Filter], IngestPolicy], pdal.Filter]]` row table mapping each filter to its injected `Filter.<name>` factory call over the `Filter` class threaded from `_stages` (so the SMRF `window` versus PMF `max_window_size` option spelling is one row arm, never a service branch, and the module-level closure never resolves a `pdal` global the boundary-scope import leaves unbound); `_STAGE` the one `Map[IngestStage, Callable[[type[pdal.Filter], IngestPolicy], pdal.Filter]]` builder table, each row reading the threaded `Filter` class plus the policy and returning a fully-built `pdal.Filter` so resolving a stage's driver and its option dict is one row read; `IngestPolicy` the frozen `gc=False` per-stage knob carrier with the `ground_filter`/`decimate_filter` `IngestFilter` swap fields, the per-driver option scalars, the parameterized `range_axis`/`range_band` crop (the `limits` string DERIVES as `f"{axis}[{lo:g}:{hi:g}]"`, never a raw grammar literal), and the `stream_chunk` streaming gate; `StationFact` the typed per-scan E57 provenance row (guid, point count, pose translation) read off the `pye57` `ScanHeader`; `IngestReceipt` the typed `gc=False` receipt carrying the source, the applied-stage tuple, the input/output point counts, the decimation ratio, the executed pipeline's `srswkt2` text and schema dimension count, and the station provenance rows, producing its slot map once through `IngestReceipt.facts`, with the `@receipted(_REDACTION)`-decorated `IngestReceipt._emit` egress aspect harvesting `contribute` on exit — the receipt conforms structurally to the runtime-checkable `ReceiptContributor` Protocol through its own `contribute()`, never importing the port as a base.
- Cases: `ScanSource` cases `arrow_las` (the data-branch `pyarrow.Table` bridge — `x`/`y`/`z` plus the LAS dimension columns `data/spatial/mesh.md#POINTCLOUD` already decoded) and `e57` (the `pye57` structured multi-scan source the columnar bridge does not own, read per-scan with its acquisition pose applied and its `ScanHeader` provenance harvested) — matched by `match`/`assert_never`, each binding the inbound decode that owns it and converging on the shared `_filter_graph` and the one `o3d.t.geometry.PointCloud` egress; `IngestStage` rows `GROUND_CLASSIFY` (`IngestFilter.SMRF` simple-morphological default, `IngestFilter.PMF` progressive-morphological alternate, the swap an `IngestPolicy.ground_filter: IngestFilter` field), `OUTLIER_REMOVE` (`IngestFilter.OUTLIER` statistical/radius removal), `DECIMATE` (`IngestFilter.DECIMATION` every-nth, `IngestFilter.VOXELDOWNSIZE` voxel-centroid, the swap an `IngestPolicy.decimate_filter: IngestFilter` field), and `RANGE_CROP` (`IngestFilter.RANGE` dimension-bounded crop over the derived `limits`) — each building its `pdal.Filter` through its `_STAGE` builder row, the swappable rows reading the policy filter override and dispatching to the `_FILTER` factory row so a stage's knobs and driver are one row, never a branch.
- Entry: `ScanIngestion.ingest` is `async` — it admits one `ScanSource` value and returns `RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]` by composing `evidence_run(EvidenceScope.SCAN_INGESTION, f"ingest.{source.tag}", partial(self.lane.offload, _ingest_kernel, source, self.policy))` — the graduation weave opens the seeded span, `async_boundary` fences the offload, the weave's `_flat` absorbs the lane's already-fenced rail un-nested, and the cleared `Ok` threads the receipt slot through the page's `@receipted` `_emit` map so emission fires exactly once on success. The kernel `_ingest_kernel` is module-level and picklable: the lane imports neither `pdal` nor `pye57` nor the kernel, the pdal stage graph crosses the process-modality fallback by its own JSON pickle state, and a decode/filter raise inside the hop converts through the lane's `async_boundary` onto the rail the weave records on the live span. The interior runs the source through one `match`: the `arrow_las` arm folds the Arrow columns into the `pdal`-shaped structured `numpy` array through the `@beartype(conf=FAULT_CONF)`-fenced `_structured` constructor, the `e57` arm folds every scan's pose-applied global-frame block plus its `ScanHeader` provenance through `_read_e57`, and both converge on `_filter_graph` building the `IngestStage`-keyed `pdal.Pipeline` over the `|` pipe, running the policy-selected whole-array `execute()` or chunked `iterator(chunk_size)` arm, reading the pipeline's `srswkt2`/`schema` output metadata, and lifting the cleaned array into the egress tensor `PointCloud`.
- Auto: `_filter_graph` folds the optional filter graph through one `Option` rail — `_stages` builds the `IngestPolicy.stages` row sequence into a `Block[pdal.Filter]` through the `_STAGE` table and gates emptiness through `built.try_head().map(lambda _: built)`, so an empty `stages` policy is the structural `Nothing` the `.default_value(...)` arm lifts the raw array straight through rather than a bare `and` truthiness branch. The present-graph arm runs `_execute`, which binds the in-memory structured array to the head stage through `stages.head().pipeline(points)` (the documented stage-array-to-`Pipeline` wrap) rather than a `Reader` — so the array enters once at the first stage — folds the `tail()` over `|` (`Block.fold` over the pipe operator), then runs the policy-selected execution arm: `stream_chunk == 0` runs `p.execute()` and reads `p.arrays[0]`; a non-zero chunk gates on the composed graph's `p.streamable` fact and folds `np.concatenate(tuple(p.iterator(chunk_size=chunk)))` so a block-scale cloud never materializes whole — a non-streamable stage in the graph (smrf/pmf/outlier/voxeldownsize all block) degrades the chunk request to `execute()` rather than raising — and reads the executed pipeline's `srswkt2` spatial-reference text plus the `schema` dimension census as output metadata the receipt carries. The `import pdal` side-effect that binds the injected `Filter.<name>` factories lives in `_stages`, the one place a `_STAGE` row is invoked, so the boundary import completes before any factory reference and `_stages` threads the imported `pdal.Filter` class into every row. Each stage builds through one `_STAGE` row: the row reads the threaded `Filter` class plus the `IngestPolicy`, resolves the swappable `GROUND_CLASSIFY`/`DECIMATE` rows to their policy-chosen `IngestFilter`, and dispatches to the `_FILTER` row that calls the injected `Filter.<name>` factory with the non-`type` option kwargs (the SMRF `window`/`cell`/`slope` versus the PMF `max_window_size`, the outlier `mean_k`/`multiplier`, the decimation `step` or the voxeldownsize `cell`, the derived range `limits`) — one row read building one `pdal.Filter`, never a raw `Filter(type=...)` plus a parallel option match. The `e57` arm reads `e.scan_count` scans and folds each scan through `e.read_scan(index, transform=True)` — the conditioned intake owning coordinate-system auto-detect, spherical projection, invalid-state mask, and pose `to_global` — while `e.get_header(index)` yields the typed `ScanHeader` whose `guid`/`point_count`/`translation` fold one `StationFact` provenance row per station (the pose/acquisition surface `read_scan_raw` bypasses and the receipt would otherwise drop), concatenating the multi-scan global-frame blocks into one structured array before the shared `_filter_graph`.
- Receipt: `IngestReceipt.of` is the keyword fold that derives the decimation ratio from the input/output counts and integer-narrows the counts at one factory; `IngestReceipt.facts` produces the slot map once carrying native slots — the `ScanSource` tag (`str`), the realized `IngestStage` order as a native `tuple[str, ...]` axis, the input/output point counts (`int`), the decimation ratio (`float`), the `srswkt2` text (`str`), the schema dimension count (`int`), and the station provenance as native `(guid, points, translation)` rows — so the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` and its `Encoder(enc_hook=repr, order="deterministic")` renderer serialize the sequence without a `",".join(...)`/`str()` pre-coerce. Emission rides the `@receipted(_REDACTION)` AOP aspect over the inner `IngestReceipt._emit`, harvesting `contribute` on exit, which yields one `emitted`-phase `Receipt.of("geometry.scan.ingestion", ("emitted", self.source, self.facts()))` row — the two-argument `Receipt.of(owner, evidence)` contract. Ingestion mints no graduation subject — there is no `scan-ingestion` member on the geometry-minted `rasm.geometry.graduation` `GeometrySubject` union, because the cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, never a geometry-case handoff of its own.
- Packages: `pdal` (the injected typed driver factories `Filter.smrf`/`.pmf`/`.outlier`/`.decimation`/`.voxeldownsize`/`.range` the `inject_pdal_drivers()` import side-effect binds with `type='filters.<name>'` pre-filled so a raw `Filter(type=...)` is the rejected form; `stage.pipeline(*arrays) -> Pipeline` the in-memory array bind; `stage | other -> Pipeline` the pipe composition; `p.execute() -> int` the whole-array run; `p.iterator(chunk_size) -> PipelineIterator` the streaming run block-scale clouds ride; `p.arrays -> list[structured ndarray]` the output; `p.srswkt2 -> str` and `p.schema -> dict` the executed pipeline's spatial-reference and dimension-census output metadata the receipt folds; the pipeline-JSON `__getstate__`/`__setstate__` pickle state the process-modality fallback rides), `pye57` (`E57(path, mode='r')` the context-manager open, `e.scan_count`, `e.read_scan(index, transform=True)` the conditioned global-frame intake, `e.get_header(index) -> ScanHeader` the typed pose/acquisition surface — `guid`/`point_count`/`translation` folded as station provenance), `open3d` (`o3d.core.Tensor`/`o3d.t.geometry.PointCloud` egress), `pyarrow` (`Table.column(name).to_numpy(zero_copy_only=False)` reading the bridge columns), `numpy` (`empty`/`column_stack`/`concatenate`/`astype` structured-array assembly, the one shared `_DTYPE` layout), `expression` (`Block.of_seq`/`Block.fold`/`Block.head`/`Block.tail` the pipe-operator fold, `Block.try_head`/`Option.map`/`Option.default_value` the absent-graph fold, `Map.of_seq` the `_STAGE`/`_FILTER` row tables and the keep-all redaction), `beartype` (`@beartype(conf=FAULT_CONF)` fencing `_structured`), geometry (`evidence_run`/`EvidenceScope` the graduation weave — the span, fence, and harvest this page composes instead of minting), `msgspec` (`Struct`/`gc=False` the frozen carriers), runtime (`RuntimeRail`/`FAULT_CONF`, `LanePolicy.offload` the per-subinterpreter hop, `Receipt`/`Redaction`/`receipted`). `laspy` is consumed transitively through the data-branch bridge, never imported or re-owned here; all three compiled scan packages (`pdal`/`pye57`/`open3d`) import function-local at boundary scope under `# noqa: PLC0415`.
- Growth: a new cleaning stage is one `IngestStage` member plus one `_STAGE` row (and one `_FILTER` row when a new driver backs it); a new driver alternative on a swappable stage is one `IngestFilter` member plus one `_FILTER` row plus the policy default; a new source format is one `ScanSource` case plus one dispatch arm; a new output-metadata fact is one `facts` slot read off the executed pipeline; a per-stage metadata harvest (the `p.metadata` per-stage JSON) is one receipt slot when a consumer demands stage-grain provenance; zero new surface.
- Boundary: the inbound LAS/LAZ/COPC decode and the `pyarrow.Table` columnar point-record bridge are the `data/spatial/mesh.md#POINTCLOUD` owner's (`laspy` full decode and the `pdal` COPC octree subset live there), so ingestion never re-reads LAS and never crosses a `pdal` `Pipeline` object at the data seam; the E57 path is legitimately ingestion's because `pye57` is absent from the data branch; the registration that consumes the cleaned cloud is `scan/registration.md#REGISTRATION`; the E57 write path (`write_scan_raw`) is declined — scan egress is the data seam's. No IFC parse, no registration, no deviation, no reconstruction, no tessellation, no durable store, no Rhino/GH mutation. The deleted forms: a sync `ingest` blocking the event loop on the SMRF/voxel sweep where the lane offload isolates it; a `lane: LanePolicy | None` accepted yet never composed (the dead-seam form this page's `async` entry resolves by actually offloading); a page-local `trace.get_tracer` mint or hand-authored span/`_ok` pair where `evidence_run` owns the weave; an untyped `object` source token paired with a separate enum; a stringly `ground_filter: str` policy field; a raw `pdal.Filter(type=..., **options)` construction; a raw `"Z[0:30]"` grammar literal where `range_axis`/`range_band` derive it; a whole-array `execute()` forced on a block-scale cloud where the `stream_chunk` row selects `iterator`; a `read_scan_raw` path bypassing the invalid-state mask and dropping the header provenance; a four-positional `Receipt.of` call or a `str()`-coerced facts map; a `contribute` returning a bare `Receipt`; a `Reader`-based file re-read; a per-stage filter method family; a hand-rolled SMRF/outlier/voxel kernel; and a `laspy` LAS re-decode.

```python contract
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


# the bounded `pdal` driver vocabulary the swappable stages select over; `.value` is the
# `filters.*` driver string. A stringly `ground_filter: str` policy field is the deleted form.
class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


# the source IS the discriminant carrying its own decode payload, never an `object` token paired
# with a separate enum: `arrow_las` the already-decoded columnar `pyarrow.Table` bridge (quoted so
# the heavy compiled import stays at boundary scope), `e57` the pye57 source path.
@tagged_union(frozen=True)
class ScanSource:
    tag: Literal["arrow_las", "e57"] = tag()
    arrow_las: "pa.Table" = case()
    e57: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# ingestion facts carry no secret field, so the @receipted egress binds the keep-all redaction.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# the one structured layout the Arrow bridge and the E57 blocks both fill; `pdal` reads X/Y/Z.
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
    stream_chunk: int = 0  # 0 = whole-array execute(); a positive chunk streams iterator(chunk_size) when the composed graph is streamable, else degrades to execute()

    @property
    def range_limits(self) -> str:
        # the `filters.range` grammar string DERIVES from the typed axis/band pair — never a raw literal.
        lo, hi = self.range_band
        return f"{self.range_axis}[{lo:g}:{hi:g}]"


class StationFact(Struct, frozen=True, gc=False):
    # one typed E57 provenance row per station off the pye57 ScanHeader — the pose/acquisition
    # surface the raw read path drops; native slots ride the receipt facts directly.
    guid: str
    points: int
    translation: tuple[float, float, float]


class IngestReceipt(Struct, frozen=True, gc=False):
    source: Literal["arrow_las", "e57"]
    stages: tuple[IngestStage, ...]
    input_points: int
    output_points: int
    decimation: float = 1.0
    srs: str = ""  # the executed pipeline's srswkt2 spatial-reference text; empty for an SRS-less in-memory array
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
        return receipt  # the @receipted aspect harvests `contribute` and emits on exit; egress is the decorator rail

    def facts(self) -> dict[str, object]:
        # native slots and native tuple axes: the receipts EventDict is dict[str, object] and its
        # Encoder(enc_hook=repr, order="deterministic") renderer serializes without a str() pre-coerce.
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
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints the `fact` case at `emitted`, never a four-positional call against the port.
        return (Receipt.of("geometry.scan.ingestion", ("emitted", self.source, self.facts())),)


# --- [OPERATIONS] -----------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _structured(x: np.ndarray, y: np.ndarray, z: np.ndarray) -> np.ndarray:
    # the contract fence the CLASSIFY `api` row folds: a non-ndarray Arrow/E57 column raises the
    # canonical BeartypeCallHintViolation the enclosing fence lifts onto the `api` rail at the seam.
    out = np.empty(x.shape[0], dtype=_DTYPE)
    out["X"], out["Y"], out["Z"] = x, y, z
    return out


def _read_e57(path: str) -> tuple[np.ndarray, tuple[StationFact, ...]]:
    import pye57  # noqa: PLC0415

    # read_scan(transform=True) is the conditioned intake: coordinate-system auto-detect,
    # spherical->cartesian projection, invalid-state mask, and per-scan pose all applied; the typed
    # ScanHeader yields the guid/point-count/translation provenance the raw path would drop.
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
    import pdal  # noqa: PLC0415  the import side-effect binds the injected Filter.<name> factories before any _STAGE row call

    # the injected `Filter` class threads into each module-level `_STAGE`/`_FILTER` closure, so the
    # tables never resolve a `pdal` global the boundary-scope import policy leaves unbound.
    built = Block.of_seq(_STAGE[stage](pdal.Filter, policy) for stage in policy.stages)
    return built.try_head().map(lambda _: built)


def _execute(stages: Block["pdal.Filter"], points: np.ndarray, chunk: int) -> tuple[np.ndarray, str, int]:
    # the array enters once at the head stage's `.pipeline(points)` wrap (never a redundant Reader
    # re-read), the tail folds over the `|` pipe; the policy-selected execution arm is whole-array
    # execute() or the chunked iterator a block-scale cloud folds through np.concatenate — one graph,
    # two execution rows. The executed pipeline's srswkt2/schema output metadata rides the receipt.
    pipeline = stages.tail().fold(lambda acc, stage: acc | stage, stages.head().pipeline(points))
    if chunk and pipeline.streamable:
        # `iterator()` REQUIRES every composed stage streamable (`p.streamable` folds
        # drivers.StreamableTypes); a blocking stage (smrf/pmf/outlier/voxeldownsize) degrades
        # the chunk request to whole-array execute() instead of raising mid-pipeline.
        cleaned = np.concatenate(tuple(pipeline.iterator(chunk_size=chunk)))
    else:
        pipeline.execute()
        cleaned = pipeline.arrays[0]
    schema = pipeline.schema.get("schema", {}).get("dimensions", ())
    return cleaned, str(pipeline.srswkt2 or ""), len(schema)


def _filter_graph(points: np.ndarray, policy: IngestPolicy) -> tuple["o3d.t.geometry.PointCloud", np.ndarray, str, int]:
    import open3d as o3d  # noqa: PLC0415

    # an empty `stages` policy lifts the raw array straight through (`_stages` gates emptiness via
    # Block.try_head), never an `and` truthiness branch standing in for the absent-graph case.
    cleaned, srs, dims = (
        _stages(policy).map(lambda stages: _execute(stages, points, policy.stream_chunk)).default_value((points, "", 0))
    )
    positions = np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float32)
    cloud = o3d.t.geometry.PointCloud()
    cloud.point.positions = o3d.core.Tensor(positions)
    return cloud, cleaned, srs, dims


def _ingest_kernel(source: ScanSource, policy: IngestPolicy) -> tuple["o3d.t.geometry.PointCloud", IngestReceipt]:
    # the module-level picklable kernel the lane offloads: the lane imports neither pdal nor pye57 nor
    # this callable, and the pdal stage graph crosses the process-modality fallback by its own JSON
    # pickle state; a raise here converts through the lane's async_boundary onto the rail.
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
        # the one composed entry: the graduation weave (seeded span + fence + harvest) wraps the lane
        # offload — `partial` keeps the dispatch a coroutine function the weave's modality probe reads,
        # and the weave's `_flat` absorbs the lane's already-fenced rail un-nested. The cleared Ok
        # threads the receipt slot through the page's @receipted `_emit`, so emission fires exactly
        # once on success and a worker raise stays an Error(BoundaryFault) recorded on the live span.
        rail = await evidence_run(
            EvidenceScope.SCAN_INGESTION, f"ingest.{source.tag}", partial(self.lane.offload, _ingest_kernel, source, self.policy)
        )
        return rail.map(lambda pair: (pair[0], IngestReceipt._emit(pair[1])))


# --- [TABLES] ---------------------------------------------------------------------------

# one factory builder per filter driver over the injected `Filter.<name>` staticmethods the
# `import pdal` side-effect binds; the SMRF `window` versus PMF `max_window_size` option spelling
# is one row arm, never a service branch. A raw `Filter(type=...)` is the form the pdal RAIL_LAW rejects.
_FILTER: Final[Map[IngestFilter, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestFilter.SMRF, lambda flt, p: flt.smrf(window=p.ground_window, cell=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.PMF, lambda flt, p: flt.pmf(max_window_size=p.ground_window, cell_size=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.OUTLIER, lambda flt, p: flt.outlier(method="statistical", mean_k=p.outlier_mean_k, multiplier=p.outlier_multiplier)),
    (IngestFilter.DECIMATION, lambda flt, p: flt.decimation(step=p.decimate_step)),
    (IngestFilter.VOXELDOWNSIZE, lambda flt, p: flt.voxeldownsize(cell=p.voxel_cell)),
    (IngestFilter.RANGE, lambda flt, p: flt.range(limits=p.range_limits)),
])

# one builder per stage threading the injected `Filter` class through to `_FILTER`: the swappable rows
# read the policy filter override, the fixed rows bind their `_FILTER` row directly (same arity) —
# building a stage's `pdal.Filter` is one row read, never a type match plus a parallel options match.
_STAGE: Final[Map[IngestStage, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, lambda flt, p: _FILTER[p.ground_filter](flt, p)),
    (IngestStage.OUTLIER_REMOVE, _FILTER[IngestFilter.OUTLIER]),
    (IngestStage.DECIMATE, lambda flt, p: _FILTER[p.decimate_filter](flt, p)),
    (IngestStage.RANGE_CROP, _FILTER[IngestFilter.RANGE]),
])
```
