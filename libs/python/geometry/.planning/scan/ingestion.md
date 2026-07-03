# [PY_GEOMETRY_SCAN_INGESTION]

Raw-scan preprocessing — the registration-ready front door of the host-free scan companion, the cleaning the `data` branch deliberately does not run. `ScanIngestion` is one frozen owner discriminating over a `ScanSource` `@tagged_union` whose two cases each carry the decode that owns them — an `arrow_las` case holding the `pyarrow.Table` columnar point-record bridge from `data/spatial/mesh.md#POINTCLOUD` (LAS/LAZ/COPC already decoded, never re-read here) and an `e57` case holding the `pye57` source path the columnar bridge does not own — converging on one composable `pdal` filter graph and one registration-ready `o3d.t.geometry.PointCloud` egress. The graph is folded from an `IngestStage` row sequence over the `pdal` `|` pipe operator, not a fixed pipeline, so the order and membership of the cleaning steps — SMRF/PMF ground classification, statistical/radius outlier removal, voxel/decimation downsampling, range cropping — are policy, not code, and every stage builds its `pdal` `Filter` through one `_STAGE` builder row over the injected typed driver factories rather than a parallel type-lookup and option-build method pair. Cross-cutting concerns ride as one fenced rail: `_TRACER.start_as_current_span("geometry.ingest")` opens the measured span, `boundary` runs the kernel eagerly inside it so a decode/filter raise records once through the faults `_convert` weave, the `@beartype(conf=FAULT_CONF)` `_structured` fence folds a malformed column array onto the `api` rail, `LanePolicy.offload` hands the multi-second SMRF/voxel sweep across the per-subinterpreter lane, and the `@receipted(_REDACTION)` aspect on `IngestReceipt._emit` harvests `IngestReceipt.contribute` on exit so emission rides the decorator rail (the `RegistrationResult._emit`/`ReconReceipt._emit`/`DeviationResult._emit` sibling parity), never an inline `Signals.emit` nor a deferral to a caller's aspect this owner does not own. The cleaned cloud is the precondition the `scan/registration.md#REGISTRATION` `register` rail consumes (a same-folder read-only seam); ingestion never registers, never deviates, never tessellates, and never re-owns LAS decode.

## [01]-[INDEX]

- [01]-[INGESTION]: source-discriminated raw-scan preprocessing under one owner over the `ScanSource` tagged-union intake, the `IngestStage`-keyed `pdal` `|` filter graph folded from one `_STAGE` builder table over the injected `Filter.<name>` factories, and the `pye57` E57 structured-scan path, egressing a registration-ready `o3d.t.geometry.PointCloud`, the cross-cutting concerns folded as aspects — `boundary` the one exception-to-fault fence under a `geometry.ingest` OTel span whose `Ok` arm widens with `IngestReceipt.span_facts`, the `@beartype(conf=FAULT_CONF)` `_structured` contract fence, the `Option`-folded `_stages`/`_execute` filter-graph split, `LanePolicy.offload` the CPU-offload seam, the `@receipted(_REDACTION)` egress aspect over the inner `IngestReceipt._emit` harvesting the `IngestReceipt.facts`-once `ReceiptContributor` stream.

## [02]-[INGESTION]

- Owner: `ScanIngestion` — the frozen owner discriminating over the `ScanSource` `@tagged_union` carrying the inbound decode per case (`arrow_las` the `pyarrow.Table` bridge, `e57` the `pye57` source path), so the source IS the payload-carrying discriminant rather than an untyped `object` token paired with a separate enum (the `LaneSource`/`Admit`/`HandoffAxis` sibling shape); `IngestStage` the `pdal` filter-graph stage vocabulary the realized-order receipt records; `IngestFilter` the bounded `filters.*` driver vocabulary the swappable stages select over (a closed `StrEnum` whose `.value` is the `filters.*` driver string, never a stringly `ground_filter: str` policy field); `_FILTER` the one `Map[IngestFilter, Callable[[type[pdal.Filter], IngestPolicy], pdal.Filter]]` row table mapping each filter to its injected `Filter.<name>` factory call over the `Filter` class threaded from `_stages` (so the SMRF `window` versus PMF `max_window_size` option spelling is one row arm, never a service branch, and the module-level closure never resolves a `pdal` global the boundary-scope import leaves unbound); `_STAGE` the one `Map[IngestStage, Callable[[type[pdal.Filter], IngestPolicy], pdal.Filter]]` builder table, each row reading the threaded `Filter` class plus the policy and returning a fully-built `pdal.Filter` so resolving a stage's driver and its option dict is one row read, never a `_filter_type` match plus a parallel `_options` match over the same axis; `IngestPolicy` the frozen `gc=False` per-stage knob carrier with the `ground_filter`/`decimate_filter` `IngestFilter` swap fields and the per-driver option scalars; `IngestReceipt` the typed `gc=False` receipt carrying the source, the applied-stage tuple, the input/output point counts, and the decimation ratio, producing its slot map once through `IngestReceipt.facts` (parity with the sibling `RegistrationResult.facts`/`ReconReceipt.facts`/`DeviationBand.facts`) so the receipt fold lives in one place, a `span_facts` bounded `str | int` scalar projection both the `geometry.ingest` span and the receipt read (parity with `RegistrationResult.span_facts`/`MeshQuality.span_facts`), the `@receipted(_REDACTION)`-decorated `IngestReceipt._emit` egress aspect that harvests `contribute` on exit so emission rides the decorator rail (the `RegistrationResult._emit`/`ReconReceipt._emit`/`DeviationResult._emit` parity) rather than an inline `Signals.emit`, satisfying the runtime `ReceiptContributor` Protocol through its own `contribute()` that streams `facts()`, never importing `ReceiptContributor` as a stand-in for a typed receipt nor inlining the facts dict at the `contribute` call site.
- Cases: `ScanSource` cases `arrow_las` (the data-branch `pyarrow.Table` bridge — `x`/`y`/`z` plus the LAS dimension columns `data/spatial/mesh.md#POINTCLOUD` already decoded) and `e57` (the `pye57` structured multi-scan source the columnar bridge does not own, read per-scan with its acquisition pose applied) — matched by `match`/`assert_never`, each binding the inbound decode that owns it and converging on the shared `_filter_graph` and the one `o3d.t.geometry.PointCloud` egress; `IngestStage` rows `GROUND_CLASSIFY` (`IngestFilter.SMRF` simple-morphological default, `IngestFilter.PMF` progressive-morphological alternate, the swap an `IngestPolicy.ground_filter: IngestFilter` field), `OUTLIER_REMOVE` (`IngestFilter.OUTLIER` statistical/radius removal), `DECIMATE` (`IngestFilter.DECIMATION` every-nth, `IngestFilter.VOXELDOWNSIZE` voxel-centroid, the swap an `IngestPolicy.decimate_filter: IngestFilter` field), and `RANGE_CROP` (`IngestFilter.RANGE` dimension-bounded crop) — each building its `pdal.Filter` through its `_STAGE` builder row, the swappable rows reading the policy filter override and dispatching to the `_FILTER` factory row so a stage's knobs and driver are one row, never a branch.
- Entry: `ScanIngestion.ingest` admits one `ScanSource` value (the case carries its own payload) and returns a `RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]` through the span-then-fence discipline the sibling `registration.md`/`reconstruction.md`/`deviation.md` own: the one `_TRACER.start_as_current_span("geometry.ingest")` widens with the bounded `source` tag behind `is_recording()`, then `boundary(f"ingestion.{source.tag}", lambda: self._emit(self._dispatch(source)))` runs the kernel eagerly inside the live `with` so the interior raises only inside the thunk and the `pdal`/`pye57`/`open3d` fault records on the open span through the faults `_convert` weave and lifts to a `BoundaryFault` exactly once at egress; the rail then `match`es — the `Ok((_, receipt))` arm widens the recording span with `receipt.span_facts` and sets `Status(StatusCode.OK)`, the `Error(_)` arm is `pass` because the fence's `_convert` already `record_exception`d the cause and set `Status(StatusCode.ERROR, fault.tag)` on the active span — so the OTel annotation rides the measured span this owner opens rather than a per-page tracer minted on the conversion. The `_emit` wrapper composed inside the thunk splits the tuple to thread the receipt slot through `IngestReceipt._emit` (the `@receipted(_REDACTION)` aspect harvesting `contribute` on exit) while the cleaned cloud rides through untouched, the reconstruction `_emit`-inside-the-fence parity, so a render/sink raise folds onto the same rail and `_dispatch` stays pure of the emission aspect. The optional `lane: LanePolicy | None` field is the imported per-subinterpreter offload seam the Growth bullet hands the multi-second SMRF/voxel kernel across through the one `LanePolicy.offload(kernel, *args)` call (the same seam `registration.md`/`reconstruction.md`/`deviation.md` carry), `LanePolicy` the imported lane field so the seam exists and the lane never imports the kernel. The interior `_dispatch` runs the source through one `match`: the `arrow_las` arm folds the Arrow columns into the `pdal`-shaped structured `numpy` array through the `@beartype(conf=FAULT_CONF)`-fenced `_structured` constructor, the `e57` arm folds every scan's pose-applied global-frame block into the same layout through `_read_e57`, and both converge on `_filter_graph` building the `IngestStage`-keyed `pdal.Pipeline` over the `|` pipe, running `p.execute()`, and lifting `p.arrays[0]` into the egress tensor `PointCloud`.
- Auto: `_filter_graph` folds the optional filter graph through one `Option` rail — `_stages` builds the `IngestPolicy.stages` row sequence into a `Block[pdal.Filter]` through the `_STAGE` table and gates emptiness through `built.try_head().map(lambda _: built)` (the same `Block.try_head` emptiness gate the faults `traversed` owner reads), so an empty `stages` policy is the structural `Nothing` the `.default_value(points)` arm lifts the raw array straight through rather than a bare `and` truthiness branch standing in for the absent-graph case. The present-graph arm runs `_execute`, which binds the in-memory structured array to the head stage through `stages.head().pipeline(points)` (`pdal.md` stage-construction ENTRYPOINT [05], the documented stage-array-to-`Pipeline` wrap) rather than a `Reader` — so the array enters once at the first stage, never a redundant `Reader` re-read and never a double `inputs` reassignment — folds the `tail()` over `|` (`Block.fold` over the pipe operator, never a bare `functools.reduce` where the `expression` `Block` carrier the rail already speaks owns the fold), runs `p.execute()` (`pdal.md` execution ENTRYPOINT [02]) to populate `p.arrays` (`pdal.md` output ENTRYPOINT [01]), and reads `p.arrays[0]` (the single output array of the linear filter chain); the cleaned array's `X`/`Y`/`Z` fields lift into an `o3d.core.Tensor` positions block on a `t.geometry.PointCloud`. The `import pdal` side-effect that binds the injected `Filter.<name>` factories lives in `_stages`, the one place a `_STAGE` row is invoked, so the boundary import completes before any factory reference and `_stages` threads the imported `pdal.Filter` class into every row — the module-level `_STAGE`/`_FILTER` closures take that class as their first argument rather than resolving a `pdal` global the boundary-scope import policy never binds at module level (the free-`pdal.Filter.smrf` closure being the `NameError` form). Each stage builds through one `_STAGE` row: the row reads the threaded `Filter` class plus the `IngestPolicy`, resolves the swappable `GROUND_CLASSIFY`/`DECIMATE` rows to their policy-chosen `IngestFilter` (the fixed rows binding their `_FILTER` row directly at the same `(flt, p)` arity), and dispatches to the `_FILTER` row that calls the injected `Filter.<name>` factory with the non-`type` option kwargs (the SMRF `window`/`cell`/`slope` versus the PMF `max_window_size`, the outlier `mean_k`/`multiplier`, the decimation `step` or the voxeldownsize `cell`, the range `limits`) — one row read building one `pdal.Filter`, never a raw `Filter(type=...)` plus a parallel option match. The `e57` arm reads `e.scan_count` scans (`pye57.md` lifecycle ENTRYPOINT [02]) and folds each scan through `e.read_scan(index, transform=True)` (`pye57.md` lifecycle ENTRYPOINT [04], the polymorphic conditioned intake) so the dict exits as global-frame `cartesianX`/`cartesianY`/`cartesianZ` with the per-scan pose already applied AND the invalid-state column masked — the conditioning `read_scan` owns (coordinate-system auto-detect, spherical `convert_spherical_to_cartesian` projection, invalid-state mask, pose `to_global`) rather than a `read_scan_raw` that bypasses the mask and re-plumbs `to_global(points, header.rotation, header.translation)` by hand — concatenating the multi-scan global-frame blocks into one structured array, then converging on the same `_filter_graph`.
- Receipt: `IngestReceipt.of` is the keyword fold that derives the decimation ratio from the input/output counts and integer-narrows the counts at one factory rather than positional construction at the call site (parity with `RegistrationResult.of`/`DeviationResult.of`); `IngestReceipt.facts` produces the slot map once (parity with `RegistrationResult.facts`/`ReconReceipt.facts`/`DeviationBand.facts`) carrying native slots — the `ScanSource` tag (`str`), the realized `IngestStage` order as a native `tuple[str, ...]` axis, the input/output point counts (`int`), and the decimation ratio (`float`) — so the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` and its `Encoder(enc_hook=repr, order="deterministic")` renderer serialize the sequence without a `",".join(...)`/`str()` pre-coerce, the joined-string facts map being the deleted form the receipts renderer is built to avoid. emission rides the `@receipted(_REDACTION)` AOP aspect over the inner `IngestReceipt._emit` (the decorator rail the receipts owner declares and the sibling `RegistrationResult._emit`/`ReconReceipt._emit`/`DeviationResult._emit` establish), never an inline `Signals.emit` threaded through the body; the aspect harvests `IngestReceipt.contribute` on exit, which yields one `emitted`-phase `Receipt.of("geometry.scan.ingestion", ("emitted", self.source, self.facts()))` row through the runtime `Receipt` union — the two-argument `Receipt.of(owner, evidence)` contract where the `(Phase, subject, facts)` triple mints the `fact` case, never a four-positional call against the port — and returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` `Iterable[Receipt]` port admits; the bounded `span_facts` scalars (`source`, the input/output counts) are the only subset the `geometry.ingest` span widens with on the `Ok` arm, the decimation ratio and the realized `IngestStage` axis riding the receipt facts alone; ingestion mints no `GraduationReceipt` subject — there is no `scan-ingestion` literal on the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (`registration-transform`/`reconstructed-mesh`/`scan-deviation`/...), because the cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, never a geometry-case handoff of its own.
- Packages: `pdal` (the injected typed driver factories `Filter.smrf`/`.pmf`/`.outlier`/`.decimation`/`.voxeldownsize`/`.range` the `inject_pdal_drivers()` import side-effect binds, `pdal.md` driver-factory ENTRYPOINT [02], `type='filters.<name>'` pre-filled so a raw `Filter(type=...)` is the rejected form/`stage.pipeline(*arrays) -> Pipeline` in-memory array bind ENTRYPOINT [05]/`stage | other -> Pipeline` pipe composition ENTRYPOINT [04]/`p.execute() -> int` execution ENTRYPOINT [02]/`p.arrays -> list[structured ndarray]` output ENTRYPOINT [01]), `pye57` (`E57(path, mode='r')` context-manager open lifecycle ENTRYPOINT [01]/`e.scan_count` query ENTRYPOINT [02]/`e.read_scan(index, transform=True)` the conditioned global-frame intake masking the invalid-state column and applying the pose lifecycle ENTRYPOINT [04]), `open3d` (`o3d.core.Tensor`/`o3d.t.geometry.PointCloud` egress), `pyarrow` (`Table.column(name).to_numpy(zero_copy_only=False)` reading the bridge columns into the structured layout), `numpy` (`empty`/`column_stack`/`concatenate`/`astype` structured-array assembly, the one shared `_DTYPE` layout), `expression` (`Block.of_seq`/`Block.fold`/`Block.head`/`Block.tail` the pipe-operator fold, `Block.try_head`/`Option.map`/`Option.default_value` the absent-graph fold lifting an empty `stages` policy to the raw array, `Ok`/`Error` the span-arm `match` on the rail, `Map.of_seq`/`Map`/`Map.empty` the `_STAGE`/`_FILTER` row tables and the keep-all `Redaction.classified` table), `beartype` (`@beartype(conf=FAULT_CONF)` the shared domain conf fencing `_structured` so a malformed column array raises the canonical `BeartypeCallHintViolation` the faults `CLASSIFY` `api` row folds onto the rail), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` the one `geometry.ingest` span), `msgspec` (`Struct`/`gc=False` the frozen carriers), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF`/`Receipt`/`Redaction`/`receipted`/`LanePolicy`, the `ReceiptContributor` port `IngestReceipt.contribute` satisfies structurally and the `@receipted(_REDACTION)` aspect `IngestReceipt._emit` binds for egress). `laspy` is consumed transitively through the data-branch bridge, never imported or re-owned here; all three compiled scan packages (`pdal`/`pye57`/`open3d`) import function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy, the `import pdal` side-effect binding the injected `Filter.<name>` factories living in `_stages` before any `_STAGE` factory call.
- Boundary: the inbound LAS/LAZ/COPC decode and the `pyarrow.Table` columnar point-record bridge are the `data/spatial/mesh.md#POINTCLOUD` owner's (`laspy` full decode and the `pdal` COPC octree subset live there), so ingestion never re-reads LAS and never crosses a `pdal` `Pipeline` object at the data seam — it receives a decoded `pyarrow.Table` and owns only the filter graph the data owner does not run; the E57 path is legitimately ingestion's because `pye57` is absent from the data branch; the registration that consumes the cleaned cloud is `scan/registration.md#REGISTRATION` (a same-folder read-only seam, never re-derived here); no IFC parse (that is `ifc-analysis`), no registration, no deviation, no reconstruction, no tessellation, no durable store, no Rhino/GH mutation; an untyped `object` source token paired with a separate `ScanSource` enum where the tagged-union case carries its own payload, a stringly-typed `ground_filter: str`/`decimate_filter: str` policy field where the `IngestFilter` vocabulary bounds it, a raw `pdal.Filter(type=..., **options)` construction where the injected `Filter.<name>` factory the `pdal` RAIL_LAW mandates owns the driver, a `StageSpec.type`/`options` Callable split plus a parallel `_GROUND_WINDOW` map where one `_STAGE` builder row returns a built `Filter`, a four-positional `Receipt.of(phase, owner, subject, facts)` call and a `str()`/`",".join(...)`-coerced facts map where the two-argument `Receipt.of(owner, evidence)` contract and the native-typed `dict[str, object]` (the realized `IngestStage` axis riding as a native `tuple[str, ...]`) the receipts encoder serializes own the egress, an inlined facts dict at the `contribute` call site where `IngestReceipt.facts` projects it once parity with the siblings, a `contribute` returning a bare `Receipt` where the `ReceiptContributor` port yields an `Iterable[Receipt]`, a `ReceiptContributor` import standing in for a typed receipt where `IngestReceipt` owns its own `contribute()`, an `ingest` that returns the raw `boundary(...)` rail and defers emission to a caller's `@receipted` aspect this owner does not own (so the receipt is built yet never streamed) where the sibling parity is the owner's own `@receipted(_REDACTION)` aspect over `IngestReceipt._emit` composed inside the thunk, a `_dispatch` kernel threading the emission aspect inline where the `_emit`-inside-the-fence wrapper keeps the kernel pure, a rail returned without the `Ok`/`Error` span-result `match` where the `Ok` arm widens with `span_facts` and sets `Status(StatusCode.OK)`, a missing `geometry.ingest` OTel span where the sibling registration/reconstruction/deviation owners open a measured span behind `is_recording()` and `boundary` runs the kernel eagerly inside it, an unfenced `_structured` assembly where `@beartype(conf=FAULT_CONF)` folds a malformed column array onto the `api` rail, an `and`-truthiness `stages = self.policy.stages and Block.of_seq(...)` branch where the `Block.try_head().map(...)` `Option` fold lifts the absent graph to the raw array, a missing `lane: LanePolicy | None` seam the three siblings carry, a `read_scan_raw` path bypassing the invalid-state mask where `read_scan(transform=True)` conditions and poses in one call, a `Reader`-based file re-read where the data bridge already decoded, a per-stage filter method family, a hand-rolled SMRF/outlier/voxel kernel where `pdal` owns the filter, and a `laspy` LAS re-decode where `data/spatial/mesh.md#POINTCLOUD` owns it are the deleted forms — the span widens, the structured array enters in-memory, the `_STAGE` table drives the graph, and the cleaned cloud egresses as one tensor `PointCloud`.

```python contract
import numpy as np
from collections.abc import Callable
from enum import StrEnum
from typing import Final, Literal, assert_never

from beartype import beartype
from expression import Error, Ok, Option, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted


# --- [TYPES] ----------------------------------------------------------------------------


class IngestStage(StrEnum):
    GROUND_CLASSIFY = "ground-classify"
    OUTLIER_REMOVE = "outlier-remove"
    DECIMATE = "decimate"
    RANGE_CROP = "range-crop"


# the bounded `pdal` driver vocabulary the swappable stages select over; `.value` is the
# `filters.*` driver string. A stringly `ground_filter: str` policy field is the deleted form —
# an unlisted driver fails at the type, and `_FILTER` maps each member to its injected factory.
class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


# the source IS the discriminant carrying its own decode payload (the LaneSource/Admit shape),
# never an `object` token paired with a separate enum: `arrow_las` the already-decoded columnar
# `pyarrow.Table` bridge (quoted so the heavy compiled import stays at boundary scope, never the
# contract top), `e57` the pye57 source path absent from the data branch.
@tagged_union(frozen=True)
class ScanSource:
    tag: Literal["arrow_las", "e57"] = tag()
    arrow_las: "pa.Table" = case()
    e57: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# ingestion facts carry no secret field, so the @receipted egress binds the keep-all redaction.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_TRACER: Final[trace.Tracer] = trace.get_tracer("geometry.scan.ingestion")

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
    range_limits: str = "Z[0:30]"


class IngestReceipt(Struct, frozen=True, gc=False):
    source: Literal["arrow_las", "e57"]
    stages: tuple[IngestStage, ...]
    input_points: int
    output_points: int
    decimation: float = 1.0

    @staticmethod
    def of(source: ScanSource, applied: tuple[IngestStage, ...], input_points: int, output_points: int) -> IngestReceipt:
        ratio = output_points / input_points if input_points else 1.0
        return IngestReceipt(source.tag, applied, int(input_points), int(output_points), ratio)

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: IngestReceipt) -> IngestReceipt:
        return receipt  # the @receipted aspect harvests `contribute` and emits on exit; egress is the decorator rail

    @property
    def span_facts(self) -> dict[str, str | int]:
        # the bounded scalar subset the `geometry.ingest` span widens with; the decimation ratio and the
        # realized-stage axis ride the receipt facts alone, never the span attribute map.
        return {"source": self.source, "input_points": self.input_points, "output_points": self.output_points}

    def facts(self) -> dict[str, object]:
        # native int/float slots and a native `tuple[str, ...]` realized-stage axis: the
        # `observability/receipts#RECEIPT` `EventDict` is `dict[str, object]` and its
        # `Encoder(enc_hook=repr, order="deterministic")` renderer serializes the sequence
        # without a `",".join(...)`/`str()` pre-coerce — the joined-string facts map is the
        # deleted form, parity with `RegistrationResult.facts`/`ReconReceipt.facts`.
        return {
            "source": self.source,
            "stages": tuple(s.value for s in self.stages),
            "input_points": self.input_points,
            "output_points": self.output_points,
            "decimation": self.decimation,
        }

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints the `fact` case at `emitted`, never a four-positional call against the port.
        return (Receipt.of("geometry.scan.ingestion", ("emitted", self.source, self.facts())),)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanIngestion(Struct, frozen=True):
    policy: IngestPolicy = IngestPolicy()
    lane: LanePolicy | None = None

    def ingest(self, source: ScanSource) -> RuntimeRail[tuple["o3d.t.geometry.PointCloud", IngestReceipt]]:
        # span-then-fence (the reconstruction/deviation discipline): the `geometry.ingest` span widens
        # with the bounded `source` tag behind `is_recording()`, then `boundary` runs the kernel eagerly
        # inside the live `with` so a decode/filter raise records on the open span through the faults
        # `_convert` weave; the `Ok` arm widens with `receipt.span_facts` and the `Error` arm leaves
        # status to the fence's `_convert`. Emission rides the `@receipted(_REDACTION)` aspect composed
        # INSIDE the thunk (`_emit(self._dispatch(...))`, the reconstruction `_emit`-inside-the-fence
        # parity) so the cleaned cloud rides through untouched and a render/sink raise folds onto the
        # same rail, never an inline `Signals.emit` threaded through the body.
        with _TRACER.start_as_current_span("geometry.ingest") as span:
            if span.is_recording():
                span.set_attributes({"source": source.tag})
            rail = boundary(f"ingestion.{source.tag}", lambda: self._emit(self._dispatch(source)))
            match rail:
                case Ok((_, receipt)):
                    if span.is_recording():
                        span.set_attributes(receipt.span_facts)
                    span.set_status(Status(StatusCode.OK))
                case Error(_):
                    pass
            return rail

    def _emit(self, payload: tuple["o3d.t.geometry.PointCloud", IngestReceipt]) -> tuple["o3d.t.geometry.PointCloud", IngestReceipt]:
        # the `@receipted` aspect on `IngestReceipt._emit` harvests `contribute` and emits on exit; the
        # tensor cloud rides through unobserved, the receipt slot the only contributor the aspect streams.
        cloud, receipt = payload
        return cloud, IngestReceipt._emit(receipt)

    def _dispatch(self, source: ScanSource) -> tuple["o3d.t.geometry.PointCloud", IngestReceipt]:
        match source:
            case ScanSource(tag="arrow_las", arrow_las=table):
                points = self._structured(
                    table.column("x").to_numpy(zero_copy_only=False),
                    table.column("y").to_numpy(zero_copy_only=False),
                    table.column("z").to_numpy(zero_copy_only=False),
                )
            case ScanSource(tag="e57", e57=path):
                points = self._read_e57(path)
            case _ as unreachable:
                assert_never(unreachable)
        cloud, cleaned = self._filter_graph(points)
        return cloud, IngestReceipt.of(source, self.policy.stages, points.shape[0], cleaned.shape[0])

    @beartype(conf=FAULT_CONF)
    def _structured(self, x: np.ndarray, y: np.ndarray, z: np.ndarray) -> np.ndarray:
        # the contract fence the `CLASSIFY` `api` row folds: a non-`ndarray` Arrow/E57 column (a
        # `None`/object column) raises the canonical `BeartypeCallHintViolation` the `boundary` thunk
        # lifts onto the `api` rail at the seam; a length-mismatched but well-typed column still rails
        # through the broad `boundary` `catch` as the structured assignment's `ValueError`.
        out = np.empty(x.shape[0], dtype=_DTYPE)
        out["X"], out["Y"], out["Z"] = x, y, z
        return out

    def _read_e57(self, path: str) -> np.ndarray:
        import pye57  # noqa: PLC0415

        # read_scan(transform=True) is the conditioned intake: coordinate-system auto-detect,
        # spherical->cartesian projection, invalid-state mask, and per-scan pose all applied, so
        # the dict exits global-frame and masked. read_scan_raw bypasses the mask — the deleted form.
        with pye57.E57(path, mode="r") as handle:
            blocks = Block.of_seq(
                self._structured(*(scan[f"cartesian{axis}"] for axis in ("X", "Y", "Z")))
                for scan in (handle.read_scan(index, transform=True) for index in range(handle.scan_count))
            )
        # a scan-less E57 is the structural `Nothing` the empty-array arm lifts, never a bare `if blocks`
        # truthiness branch on the `Block` (the same absent-sequence gate `_stages` reads) — `concatenate`
        # over an empty sequence raises, so the emptiness is the `Option` fold, not a falsy-Block guard.
        return blocks.try_head().map(lambda _: np.concatenate(blocks)).default_value(np.empty(0, dtype=_DTYPE))

    def _filter_graph(self, points: np.ndarray) -> tuple["o3d.t.geometry.PointCloud", np.ndarray]:
        import open3d as o3d  # noqa: PLC0415

        # an empty `stages` policy lifts the raw array straight through (`_stages` gates emptiness via
        # `Block.try_head().map(...)`, `Nothing` when the policy carries no stage), never an `and`
        # truthiness branch standing in for the absent-graph case.
        cleaned = self._stages().map(lambda stages: self._execute(stages, points)).default_value(points)
        positions = np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float32)
        cloud = o3d.t.geometry.PointCloud()
        cloud.point.positions = o3d.core.Tensor(positions)
        return cloud, cleaned

    def _stages(self) -> Option[Block["pdal.Filter"]]:
        import pdal  # noqa: PLC0415  the import side-effect binds the injected Filter.<name> factories before any _STAGE row call

        # the injected `Filter` class threads into each module-level `_STAGE`/`_FILTER` closure, so the
        # tables never resolve a `pdal` global the boundary-scope import policy leaves unbound.
        built = Block.of_seq(_STAGE[stage](pdal.Filter, self.policy) for stage in self.policy.stages)
        return built.try_head().map(lambda _: built)

    def _execute(self, stages: Block["pdal.Filter"], points: np.ndarray) -> np.ndarray:
        # the array enters once at the head stage's `.pipeline(points)` wrap (never a redundant `Reader`
        # re-read), the tail folds over the `|` pipe through the `expression` `Block` carrier the rail
        # already speaks, and the single output array of the linear filter chain reads off `arrays[0]`.
        pipeline = stages.tail().fold(lambda acc, stage: acc | stage, stages.head().pipeline(points))
        pipeline.execute()
        return pipeline.arrays[0]


# --- [TABLES] ---------------------------------------------------------------------------

# one factory builder per filter driver over the injected `Filter.<name>` staticmethods the
# `import pdal` side-effect binds; the SMRF `window` versus PMF `max_window_size` option spelling
# is one row arm, never a service branch. A raw `Filter(type=...)` is the form the pdal RAIL_LAW rejects.
# The injected `Filter` CLASS is threaded as the first row argument from `_stages` (the one place the
# function-local `import pdal` runs), so these module-level closures never resolve a `pdal` global that
# the boundary-scope import policy leaves unbound — a free `pdal.Filter.smrf` here is the NameError form.
_FILTER: Final[Map[IngestFilter, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestFilter.SMRF, lambda flt, p: flt.smrf(window=p.ground_window, cell=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.PMF, lambda flt, p: flt.pmf(max_window_size=p.ground_window, cell_size=p.ground_cell, slope=p.ground_slope)),
    (IngestFilter.OUTLIER, lambda flt, p: flt.outlier(method="statistical", mean_k=p.outlier_mean_k, multiplier=p.outlier_multiplier)),
    (IngestFilter.DECIMATION, lambda flt, p: flt.decimation(step=p.decimate_step)),
    (IngestFilter.VOXELDOWNSIZE, lambda flt, p: flt.voxeldownsize(cell=p.voxel_cell)),
    (IngestFilter.RANGE, lambda flt, p: flt.range(limits=p.range_limits)),
])

# one builder per stage threading the injected `Filter` class through to `_FILTER`: the swappable rows
# read the policy filter override and dispatch through `_FILTER`, the fixed rows bind their `_FILTER`
# row directly (same `(flt, p)` arity) — building a stage's `pdal.Filter` is one row read, never an
# `_filter_type` match plus a parallel `_options` match over the same axis.
_STAGE: Final[Map[IngestStage, Callable[[type["pdal.Filter"], IngestPolicy], "pdal.Filter"]]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, lambda flt, p: _FILTER[p.ground_filter](flt, p)),
    (IngestStage.OUTLIER_REMOVE, _FILTER[IngestFilter.OUTLIER]),
    (IngestStage.DECIMATE, lambda flt, p: _FILTER[p.decimate_filter](flt, p)),
    (IngestStage.RANGE_CROP, _FILTER[IngestFilter.RANGE]),
])
```

## [03]-[RESEARCH]

- [FILTER_DRIVER_FACTORIES]: the `pdal` filter drivers the `_FILTER` table builds (`filters.smrf`/`filters.pmf` ground classification, `filters.outlier` statistical/radius removal, `filters.decimation`/`filters.voxeldownsize` downsampling, `filters.range` crop) are constructed through the injected typed driver factories (`Filter.smrf`/`Filter.pmf`/`Filter.outlier`/`Filter.decimation`/`Filter.voxeldownsize`/`Filter.range`) the `inject_pdal_drivers()` import side-effect binds — `pdal.md` driver-factory ENTRYPOINT [02] carries the `Filter.<name>(**opts)` factory family with `type='filters.<name>'` pre-filled, and the LOCAL_ADMISSION RAIL_LAW rejects a raw `Filter(type=...)` where an injected `Filter.<name>` factory exists — the `stage.pipeline(*arrays) -> Pipeline` in-memory array bind (ENTRYPOINT [05]), the pipe-operator `stage | other -> Pipeline` composition (ENTRYPOINT [04]), the `p.execute() -> int` run (execution ENTRYPOINT [02]), and the `p.arrays -> list[structured ndarray]` egress (output ENTRYPOINT [01]) confirm against the `pdal` catalogue on the companion interpreter. The concrete driver short-names are registry-runtime values from `libpdalpython.getDrivers()`; the open items are the per-driver option-key spelling (the PMF `max_window_size`/`cell_size` versus the SMRF `window`/`cell`, the `filters.range` multi-dimension `limits` syntax) the live `<driver>.__doc__` registry read confirms, live-run-tuned heuristics, not catalogue dependencies.

## [04]-[UPSTREAM]
