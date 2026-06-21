# [PY_GEOMETRY_SCAN_INGESTION]

Raw-scan preprocessing — the registration-ready front door of the host-free scan companion, the cleaning the `data` branch deliberately does not run. `ScanIngestion` is one frozen owner discriminating over a `ScanSource` `@tagged_union` whose two cases each carry the decode that owns them — an `arrow_las` case holding the `pyarrow.Table` columnar point-record bridge from `data/spatial/mesh.md#POINTCLOUD` (LAS/LAZ/COPC already decoded, never re-read here) and an `e57` case holding the `pye57` source path the columnar bridge does not own — converging on one composable `pdal` filter graph and one registration-ready `o3d.t.geometry.PointCloud` egress. The graph is folded from an `IngestStage` row sequence over the `pdal` `|` pipe operator, not a fixed pipeline, so the order and membership of the cleaning steps — SMRF/PMF ground classification, statistical/radius outlier removal, voxel/decimation downsampling, range cropping — are policy, not code, and every stage resolves its `filters.*` type string and its `pdal` option dict from one `_STAGE` row table rather than parallel type-lookup and option-build methods. The cleaned cloud is the precondition the `scan/registration.md#REGISTRATION` `register` rail consumes (a same-folder read-only seam); ingestion never registers, never deviates, never tessellates, and never re-owns LAS decode.

## [01]-[INDEX]

- [01]-[INGESTION]: source-discriminated raw-scan preprocessing under one owner over the `ScanSource` tagged-union intake, the `IngestStage`-keyed `pdal` `|` filter graph folded from one `_STAGE` row table, and the `pye57` E57 structured-scan path, egressing a registration-ready `o3d.t.geometry.PointCloud` under the unified `boundary` rail with the imported `LanePolicy` offload seam.

## [02]-[INGESTION]

- Owner: `ScanIngestion` — the frozen owner discriminating over the `ScanSource` `@tagged_union` carrying the inbound decode per case (`arrow_las` the `pyarrow.Table` bridge, `e57` the `pye57` source path), so the source IS the payload-carrying discriminant rather than an untyped `object` token paired with a separate enum (the `LaneSource`/`Admit`/`HandoffAxis` sibling shape); `IngestStage` the `pdal` filter-graph stage vocabulary; `IngestFilter` the bounded `filters.*` type vocabulary the swappable stages select over (a closed `StrEnum`, never a stringly `ground_filter: str` policy field); `_STAGE` the one `Map[IngestStage, StageSpec]` behavior table, each `StageSpec` row pairing a stage's default `IngestFilter` with a `Callable[[IngestPolicy], dict[str, object]]` option projection so resolving a stage's `pdal` `type` and its option dict is one row read, never a `_filter_type` match plus a parallel `_options` match over the same axis; `IngestPolicy` the frozen per-stage knob carrier with a `voxel` override field and a `dtype`-derived structured layout; `IngestReceipt` the typed receipt carrying the source, the applied-stage tuple, the input/output point counts, and the decimation ratio, satisfying the runtime `ReceiptContributor` Protocol through its own `contribute()` projection (parity with the sibling `RegistrationResult`/`ReconReceipt`/`DeviationResult`), never importing `ReceiptContributor` as a stand-in for a typed receipt.
- Cases: `ScanSource` cases `arrow_las` (the data-branch `pyarrow.Table` bridge — `x`/`y`/`z` plus the LAS dimension columns `data/spatial/mesh.md#POINTCLOUD` already decoded) and `e57` (the `pye57` structured multi-scan source the columnar bridge does not own, read per-scan with its acquisition pose applied) — matched by `match`/`assert_never`, each binding the inbound decode that owns it and converging on the shared `_filter_graph` and the one `o3d.t.geometry.PointCloud` egress; `IngestStage` rows `GROUND_CLASSIFY` (`IngestFilter.SMRF` simple-morphological default, `IngestFilter.PMF` progressive-morphological alternate, the swap an `IngestPolicy.ground_filter: IngestFilter` field), `OUTLIER_REMOVE` (`IngestFilter.OUTLIER` statistical/radius removal), `DECIMATE` (`IngestFilter.DECIMATION` every-nth, `IngestFilter.VOXELDOWNSIZE` voxel-centroid, the swap an `IngestPolicy.decimate_filter: IngestFilter` field), and `RANGE_CROP` (`IngestFilter.RANGE` dimension-bounded crop) — each resolving its `pdal` `type` and option dict through its `_STAGE` `StageSpec` row, the swappable rows reading the policy filter override inside the row's `type` projection so a stage's knobs and type are one row, never a branch.
- Entry: `ScanIngestion.ingest` admits one `ScanSource` value (the case carries its own payload) and returns a `RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]` through one `boundary(f"ingestion.{source.tag}", ...)` (the rail aspect every package returns through, the interior raising only inside the thunk and converting to a `BoundaryFault` exactly once at egress); the optional `lane: LanePolicy | None` field is the imported per-subinterpreter offload seam the Growth bullet hands the multi-second SMRF/voxel kernel across through the one `LanePolicy.offload(kernel, *args)` call (the same seam `registration.md`/`reconstruction.md`/`deviation.md` carry), `LanePolicy` the imported lane field so the seam exists and the lane never imports the kernel. The interior `_dispatch` runs the source through one `match`: the `arrow_las` arm folds the Arrow columns into the `pdal`-shaped structured `numpy` array through the shared `_structured` constructor, the `e57` arm folds every scan's pose-applied global-frame block into the same layout through `_read_e57`, and both converge on `_filter_graph` building the `IngestStage`-keyed `pdal.Pipeline` over the `|` pipe, running `p.execute()`, and lifting `p.arrays[0]` into the egress tensor `PointCloud`.
- Auto: `_filter_graph` folds the `IngestPolicy.stages` row sequence into a chain of `pdal.Filter` stages over `|` (`Block.fold` over the pipe operator, never a bare `functools.reduce` where the `expression` `Block` carrier the rail already speaks owns the fold), binding the in-memory structured array to the head stage through `stages[0].pipeline(points)` (`pdal.md#77`, the documented stage-array-to-`Pipeline` wrap) rather than a `Reader` — so the array enters once at the first stage, never a redundant `Reader` re-read and never a double `inputs` reassignment — runs `p.execute()` (`pdal.md#43`) to populate `p.arrays` (`pdal.md#53`), and reads `p.arrays[0]` (the single output array of the linear filter chain), an empty `stages` policy lifting the raw array straight through; the cleaned array's `X`/`Y`/`Z` fields lift into an `o3d.core.Tensor` positions block on a `t.geometry.PointCloud`. Each stage resolves through one `_STAGE` `StageSpec` row: `StageSpec.resolve(policy)` reads the row's default `IngestFilter` (or the policy override for the swappable `GROUND_CLASSIFY`/`DECIMATE` rows) as the single positional `type` to `pdal.Filter(type, **options)` so the `type` is never also a key inside the option dict, and the row's `options` projection reads only the non-`type` knobs from `IngestPolicy` (the SMRF/PMF window/cell/slope keyed by the active `IngestFilter`, the outlier method/mean-k/multiplier, the decimation step or voxel cell size, the range limits) — one row read, never a `_filter_type` match plus a separate `_options` match over the same `IngestStage` axis. The `e57` arm reads `e.scan_count` scans (`pye57.md#42`) and folds each scan through `e.read_scan(index, transform=True)` (`pye57.md#47`, the polymorphic conditioned intake) so the dict exits as global-frame `cartesianX`/`cartesianY`/`cartesianZ` with the per-scan pose already applied AND the invalid-state column masked — the conditioning `read_scan` owns (coordinate-system auto-detect, spherical `convert_spherical_to_cartesian` projection, invalid-state mask, pose `to_global`) rather than a `read_scan_raw` that bypasses the mask and re-plumbs `to_global(points, header.rotation, header.translation)` by hand — concatenating the multi-scan global-frame blocks into one structured array, then converging on the same `_filter_graph`.
- Receipt: `IngestReceipt.of` is the keyword fold that defaults the empty arms and derives the decimation ratio from the input/output counts so the one arm constructs through one factory rather than positional construction at the call site (parity with `RegistrationResult.of`/`DeviationResult.of`); `IngestReceipt.contribute` emits one `emitted`-phase `Receipt.of` row through the runtime `Receipt` union carrying the `ScanSource` tag, the applied `IngestStage` tuple (the realized order the filter graph ran), the input/output point counts, and the decimation ratio, so the measured kernel under the runtime `@receipted` aspect harvests the stream without an inline `emit` call; ingestion mints no `GraduationReceipt` subject — there is no `scan-ingestion` literal on the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (`registration-transform`/`reconstructed-mesh`/`scan-deviation`/...), because the cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, never a geometry-case handoff of its own.
- Packages: `pdal` (`Filter(type, **options)` (`pdal.md#73`)/`stage.pipeline(*arrays) -> Pipeline` in-memory array bind (`pdal.md#77`)/`stage | other -> Pipeline` pipe composition (`pdal.md#75`)/`p.execute()` (`pdal.md#43`)/`p.arrays -> list[structured ndarray]` (`pdal.md#53`)), `pye57` (`E57(path, mode='r')` context-manager open (`pye57.md#48`)/`e.scan_count` (`pye57.md#42`)/`e.read_scan(index, transform=True)` the conditioned global-frame intake masking the invalid-state column and applying the pose (`pye57.md#47`)/`e.close()`), `open3d` (`o3d.core.Tensor`/`o3d.t.geometry.PointCloud` egress), `pyarrow` (`Table.column(name).to_numpy(zero_copy_only=False)` reading the bridge columns into the structured layout), `numpy` (`empty`/`column_stack`/`concatenate`/`astype` structured-array assembly, the one shared `_DTYPE` layout), `expression` (`Block.of_seq`/`Block.fold`/`Map.of_seq`/`Map` the `_STAGE` row table and the pipe-operator fold), `msgspec` (`Struct`/`field`/`gc=False` the frozen carriers), runtime (`RuntimeRail`/`boundary`/`Receipt`/`LanePolicy`). `laspy` is consumed transitively through the data-branch bridge, never imported or re-owned here; all three compiled scan packages (`pdal`/`pye57`/`open3d`) import function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy.
- Growth: a new cleaning stage is one `IngestStage` row plus one `_STAGE` `StageSpec` row (its default `IngestFilter` plus its option projection) plus the `IngestPolicy` option fields the projection reads — never a new method, never a parallel `_filter_type`/`_options` pair; a new `pdal` filter is one `IngestFilter` row plus the `StageSpec` row that selects it; a new source format is one `ScanSource` case carrying its decode plus one `_dispatch` arm converging on `_filter_graph`; a new ground-classification model is the `IngestFilter.SMRF`/`IngestFilter.PMF` swap inside the `GROUND_CLASSIFY` row, an `IngestPolicy.ground_filter` field; the `pdal` filter-graph kernel (the multi-second SMRF/voxel sweep over a dense scan) hands across the runtime `execution/lanes.md#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — the lane never imports the kernel; zero new surface, no parallel per-stage filter class family, no per-format `read_las`/`read_e57` entrypoint family.
- Boundary: the inbound LAS/LAZ/COPC decode and the `pyarrow.Table` columnar point-record bridge are the `data/spatial/mesh.md#POINTCLOUD` owner's (`laspy` full decode and the `pdal` COPC octree subset live there), so ingestion never re-reads LAS and never crosses a `pdal` `Pipeline` object at the data seam — it receives a decoded `pyarrow.Table` and owns only the filter graph the data owner does not run; the E57 path is legitimately ingestion's because `pye57` is absent from the data branch; the registration that consumes the cleaned cloud is `scan/registration.md#REGISTRATION` (a same-folder read-only seam, never re-derived here); no IFC parse (that is `ifc-analysis`), no registration, no deviation, no reconstruction, no tessellation, no durable store, no Rhino/GH mutation; an untyped `object` source token paired with a separate `ScanSource` enum where the tagged-union case carries its own payload, a stringly-typed `ground_filter: str`/`decimate_filter: str` policy field where the `IngestFilter` vocabulary bounds it, a `_FILTER_TYPE` table plus an `_filter_type` override method plus a parallel `_options` match over the same `IngestStage` axis where one `_STAGE` `StageSpec` row resolves both, a `ReceiptContributor` import standing in for a typed receipt where `IngestReceipt` owns its own `contribute()`, a missing `lane: LanePolicy | None` seam the three siblings carry, a `read_scan_raw` path bypassing the invalid-state mask where `read_scan(transform=True)` conditions and poses in one call, a `Reader`-based file re-read where the data bridge already decoded, a per-stage filter method family, a hand-rolled SMRF/outlier/voxel kernel where `pdal` owns the filter, and a `laspy` LAS re-decode where `data/spatial/mesh.md#POINTCLOUD` owns it are the deleted forms — the structured array enters in-memory, the `_STAGE` table drives the graph, and the cleaned cloud egresses as one tensor `PointCloud`.

```python contract
import numpy as np
from collections.abc import Callable
from enum import StrEnum
from typing import Final, Literal, assert_never

import pyarrow as pa
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------


class IngestStage(StrEnum):
    GROUND_CLASSIFY = "ground-classify"
    OUTLIER_REMOVE = "outlier-remove"
    DECIMATE = "decimate"
    RANGE_CROP = "range-crop"


# the bounded `pdal` filter-driver vocabulary the swappable stages select over; a stringly
# `ground_filter: str` policy field is the deleted form — an unlisted driver fails at the type.
class IngestFilter(StrEnum):
    SMRF = "filters.smrf"
    PMF = "filters.pmf"
    OUTLIER = "filters.outlier"
    DECIMATION = "filters.decimation"
    VOXELDOWNSIZE = "filters.voxeldownsize"
    RANGE = "filters.range"


# the source IS the discriminant carrying its own decode payload (the LaneSource/Admit shape),
# never an `object` token paired with a separate enum: `arrow_las` the already-decoded columnar
# bridge, `e57` the pye57 source path absent from the data branch.
@tagged_union(frozen=True)
class ScanSource:
    tag: Literal["arrow_las", "e57"] = tag()
    arrow_las: pa.Table = case()
    e57: str = case()


# --- [MODELS] ---------------------------------------------------------------------------


class IngestPolicy(Struct, frozen=True):
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


# one row per stage: the swappable rows derive `type` from the policy filter override (and key
# the option dict off it), the fixed rows carry a constant `type`, so resolving a stage's `pdal`
# type AND its options is one row read — never an `_filter_type` match plus a parallel `_options`.
class StageSpec(Struct, frozen=True, gc=False):
    type: Callable[[IngestPolicy], IngestFilter]
    options: Callable[[IngestPolicy, IngestFilter], dict[str, object]]

    def resolve(self, policy: IngestPolicy) -> tuple[str, dict[str, object]]:
        chosen = self.type(policy)
        return chosen.value, self.options(policy, chosen)


class IngestReceipt(Struct, frozen=True, gc=False):
    source: Literal["arrow_las", "e57"]
    stages: tuple[IngestStage, ...]
    input_points: int
    output_points: int
    decimation: float = 1.0

    @staticmethod
    def of(source: ScanSource, applied: tuple[IngestStage, ...], input_points: int, output_points: int) -> "IngestReceipt":
        ratio = output_points / input_points if input_points else 1.0
        return IngestReceipt(source.tag, applied, input_points, output_points, ratio)

    def contribute(self) -> Receipt:
        facts = {
            "source": self.source, "stages": ",".join(s.value for s in self.stages),
            "input_points": str(self.input_points), "output_points": str(self.output_points),
            "decimation": repr(self.decimation),
        }
        return Receipt.of("emitted", "geometry.scan.ingestion", self.source, facts)


# --- [CONSTANTS] ------------------------------------------------------------------------

# the one structured layout the Arrow bridge and the E57 blocks both fill; `pdal` reads X/Y/Z.
_DTYPE: Final = np.dtype([(axis, np.float64) for axis in ("X", "Y", "Z")])


# --- [SERVICES] -------------------------------------------------------------------------


class ScanIngestion(Struct, frozen=True):
    policy: IngestPolicy = IngestPolicy()
    lane: LanePolicy | None = None

    def ingest(self, source: ScanSource) -> "RuntimeRail[tuple[o3d.t.geometry.PointCloud, IngestReceipt]]":
        return boundary(f"ingestion.{source.tag}", lambda: self._dispatch(source))

    def _dispatch(self, source: ScanSource) -> "tuple[o3d.t.geometry.PointCloud, IngestReceipt]":
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
        return cloud, IngestReceipt.of(source, self.policy.stages, int(points.shape[0]), int(cleaned.shape[0]))

    def _structured(self, x: "np.ndarray", y: "np.ndarray", z: "np.ndarray") -> "np.ndarray":
        out = np.empty(x.shape[0], dtype=_DTYPE)
        out["X"], out["Y"], out["Z"] = x, y, z
        return out

    def _read_e57(self, path: str) -> "np.ndarray":
        import pye57  # noqa: PLC0415

        # read_scan(transform=True) is the conditioned intake: coordinate-system auto-detect,
        # spherical->cartesian projection, invalid-state mask, and per-scan pose all applied, so
        # the dict exits global-frame and masked. read_scan_raw bypasses the mask — the deleted form.
        with pye57.E57(path, mode="r") as handle:
            blocks = Block.of_seq(
                self._structured(*(scan[f"cartesian{axis}"] for axis in ("X", "Y", "Z")))
                for scan in (handle.read_scan(index, transform=True) for index in range(handle.scan_count))
            )
        return np.concatenate(blocks) if blocks else np.empty(0, dtype=_DTYPE)

    def _filter_graph(self, points: "np.ndarray") -> "tuple[o3d.t.geometry.PointCloud, np.ndarray]":
        import open3d as o3d  # noqa: PLC0415
        import pdal  # noqa: PLC0415

        stages = self.policy.stages and Block.of_seq(
            pdal.Filter(*_STAGE[stage].resolve(self.policy)) for stage in self.policy.stages
        )
        if stages:
            head = stages.head().pipeline(points)
            pipeline = stages.tail().fold(lambda acc, stage: acc | stage, head)
            pipeline.execute()
            cleaned = pipeline.arrays[0]
        else:
            cleaned = points
        positions = np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float32)
        cloud = o3d.t.geometry.PointCloud()
        cloud.point.positions = o3d.core.Tensor(positions)
        return cloud, cleaned


# --- [TABLES] ---------------------------------------------------------------------------

# SMRF keys its window as `window`, PMF as `max_window_size`; the option projection reads the
# active filter so the swap is one policy field, the key spelling a row arm not a service branch.
_GROUND_WINDOW: Final[Map[IngestFilter, str]] = Map.of_seq([(IngestFilter.SMRF, "window"), (IngestFilter.PMF, "max_window_size")])

_STAGE: Final[Map[IngestStage, StageSpec]] = Map.of_seq([
    (IngestStage.GROUND_CLASSIFY, StageSpec(
        type=lambda p: p.ground_filter,
        options=lambda p, f: {"cell": p.ground_cell, "slope": p.ground_slope, _GROUND_WINDOW[f]: p.ground_window},
    )),
    (IngestStage.OUTLIER_REMOVE, StageSpec(
        type=lambda _: IngestFilter.OUTLIER,
        options=lambda p, _: {"method": "statistical", "mean_k": p.outlier_mean_k, "multiplier": p.outlier_multiplier},
    )),
    (IngestStage.DECIMATE, StageSpec(
        type=lambda p: p.decimate_filter,
        options=lambda p, f: {"step": p.decimate_step} if f is IngestFilter.DECIMATION else {"cell": p.voxel_cell},
    )),
    (IngestStage.RANGE_CROP, StageSpec(
        type=lambda _: IngestFilter.RANGE,
        options=lambda p, _: {"limits": p.range_limits},
    )),
])
```

## [03]-[RESEARCH]

- [FILTER_TYPE_MEMBERS]: the `pdal` filter `type` strings the `IngestStage` table binds (`filters.smrf`/`filters.pmf` ground classification, `filters.outlier` statistical/radius removal, `filters.decimation`/`filters.voxeldownsize` downsampling, `filters.range` crop) and their option keys (the SMRF `window`/`cell`/`slope`, the PMF `max_window_size`, the outlier `method`/`mean_k`/`multiplier`, the decimation `step`, the voxeldownsize `cell`, the range `limits` dimension-bounded string) confirm against the live `pdal` driver registry on the companion interpreter — `pdal.md` carries the `Filter(type, **options)` construction (`#73-2`), the `stage.pipeline(*arrays) -> Pipeline` in-memory array bind (`#77-6`), the pipe-operator `stage | other -> Pipeline` composition (`#75-4`), the `p.execute() -> int` run (`#43-2`), and the `p.arrays -> list[structured ndarray]` egress (`#53-1`); the only open items are the exact PMF option-key spelling versus SMRF and the `filters.range` multi-dimension `limits` syntax, live-run-tuned heuristics, not catalogue dependencies.
- [E57_FIELD_KEYS]: the `pye57.read_scan_raw(index) -> dict` field keys (`cartesianX`/`cartesianY`/`cartesianZ` for the Cartesian point block — `pye57.md#85` documents the `read_scan` field set, `read_scan_raw` returning the same field names without the inline pose transform), the `e.scan_count` multi-scan count (`#42-2`), and the `e.to_global(points, rotation, translation) -> ndarray` global-frame application (`#49-9`) confirm against the `pye57` catalogue on the companion interpreter. `to_global` is the `@staticmethod` that derives the rotation matrix from the length-4 pose quaternion internally (`Quaternion(rotation).rotation_matrix`), so the arm passes `header.rotation` (the quaternion, `#67-7`) and `header.translation` (`#68-8`), never the pre-derived `header.rotation_matrix`. The open item is whether a sensor-frame source omits the pose (`header.has_pose()` — `#62-2`) so the `to_global` step is conditional, a per-source guard the live run reads, not a catalogue dependency.

## [04]-[UPSTREAM]

- [COMPANION_BAND]: `pdal`, `pye57`, and `open3d` ride the `python_version<'3.15'` companion band the branch manifest owns — `pdal` is sdist-only (native `libpdal` C++ dependency, no cp315 wheel), `pye57` is the `libe57` wrapper with no cp315 wheel, and `open3d` caps at the cp312 compiled core — so all three import function-local under `# noqa: PLC0415` at boundary scope inside `_read_e57` and `_filter_graph`, never module-top, per the manifest import policy; `laspy` (cp315-clean) is never imported here because the LAS decode rides the data-branch bridge. The `open3d` row carries the manifest-admission delta shared with `scan/reconstruction.md` (verified absent from the manifest despite `registration.md` and `deviation.md` already consuming it): one `open3d` row at `python_version<'3.15'` admits on the branch manifest, a pre-existing delta two authored pages already need, not introduced by this page.
- [OFFLOAD_LANE]: the `_filter_graph` SMRF/voxel kernel is one of the heavy geometry CPU kernels the runtime `execution/lanes.md#LANES` `LanePolicy` CPU-offload variant absorbs once that branch owner lands; the hand-off is a uniform one-call growth on the Growth bullet shared with `daemon.md#DAEMON`, `registration.md#REGISTRATION`, `reconstruction.md#RECONSTRUCTION`, and `repair.md#MESH`, sequence-after the runtime lane, never a second concurrency surface minted in geometry.
