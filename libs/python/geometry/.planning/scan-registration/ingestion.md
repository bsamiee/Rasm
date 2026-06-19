# [PY_GEOMETRY_SCAN_INGESTION]

Raw-scan preprocessing — the registration-ready front door of the host-free scan companion, the cleaning the `data` branch deliberately does not run. `ScanIngestion` is one source-discriminated owner over a composable `pdal` filter graph: the inbound `pyarrow.Table` columnar point-record bridge from `data/mesh/exchange.md#POINTCLOUD` (LAS/LAZ/COPC already decoded, never re-read here) folds through an `IngestStage`-keyed `Reader | Filter | Writer` stage sequence — SMRF/PMF ground classification, statistical/radius outlier removal, voxel/decimation downsampling, and range cropping — into one registration-ready `o3d.t.geometry.PointCloud`, while the `pye57` E57 path reads the structured multi-scan-plus-pose source the columnar bridge does not own and feeds the same filter graph. The stage graph is composed from an `IngestStage` row sequence over the `pdal` `|` pipe operator, not a fixed pipeline, so the order and membership of the cleaning steps are policy, not code. The cleaned cloud is the precondition the `scan-registration/registration.md#REGISTRATION` `register` rail consumes (a same-folder read-only seam); ingestion never registers, never deviates, never tessellates, and never re-owns LAS decode.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[INGESTION]`: source-discriminated raw-scan preprocessing under one owner over the `pdal` `IngestStage` filter graph and the `pye57` E57 structured-scan path, egressing a registration-ready `o3d.t.geometry.PointCloud`.

## [2]-[INGESTION]

- Owner: `ScanIngestion` — the frozen owner discriminating by `ScanSource` row over either the data-branch `pyarrow.Table` point-record bridge or an E57 `ResourceRef`; `IngestStage` the `pdal` filter-graph stage vocabulary, each row binding the `filters.*` type string it owns; `IngestPolicy` the per-stage knob carrier; `IngestReceipt` the source, the applied-stage tuple, and the input/output point counts.
- Cases: `ScanSource` rows `ARROW_LAS` (the data-branch `pyarrow.Table` bridge — `x`/`y`/`z` plus the LAS dimension columns the `data/mesh/exchange.md#POINTCLOUD` owner already decoded) and `E57` (the `pye57` structured multi-scan source the columnar bridge does not own, read per-scan with its acquisition pose applied) — matched by `match`/`assert_never`, each binding the inbound decode that owns it and converging on the shared `IngestStage` filter graph and the one `o3d.t.geometry.PointCloud` egress; `IngestStage` rows `GROUND_CLASSIFY` (`filters.smrf` the simple-morphological-filter default, `filters.pmf` the progressive-morphological-filter alternate), `OUTLIER_REMOVE` (`filters.outlier` statistical/radius removal), `DECIMATE` (`filters.decimation` every-nth sampling, `filters.voxeldownsize` voxel-centroid downsampling), and `RANGE_CROP` (`filters.range` dimension-bounded crop) — each `IngestStage` row resolving to the `filters.*` type string and the `IngestPolicy`-keyed option dict the `pdal` `Filter` stage consumes.
- Entry: `ScanIngestion.ingest` admits a source token (the `pyarrow.Table` for `ARROW_LAS`, the E57 path for `E57`), the `ScanSource` discriminant, and returns a `RuntimeRail[tuple[PointCloud, IngestReceipt]]` through one `boundary(f"ingestion.{source}", ...)`; the `ARROW_LAS` arm threads the private `_arrow_to_structured` adapter folding the Arrow columns into the `pdal`-shaped structured `numpy` array, the `E57` arm threads the private `_read_e57` folding every scan's pose-applied raw fields into the same structured array, and both arms thread the shared `_filter_graph` building the `IngestStage`-keyed `pdal.Pipeline` over the `|` pipe operator, running `p.execute()`, and lifting `p.arrays[0]` into the egress `PointCloud`.
- Auto: the `_filter_graph` folds the `IngestPolicy.stages` row sequence into a chain of `pdal.Filter` stages over `|` (`functools.reduce` over the pipe operator), binding the in-memory structured array to the head stage through `stages[0].pipeline(points)` (`pdal.md#77`, the documented stage-array-to-`Pipeline` wrap) rather than a `Reader` — so the array enters once at the first stage, never a redundant `Reader` re-read and never a double `inputs` reassignment — runs `p.execute()` (`pdal.md#43`) to populate `p.arrays` (`pdal.md#53`), and reads `p.arrays[0]` (the single output array of the linear filter chain), an empty `stages` policy lifting the raw array straight through; the cleaned array's `X`/`Y`/`Z` fields lift into an `o3d.core.Tensor` positions block on a `t.geometry.PointCloud`; the `IngestStage`-to-`filters.*` resolution is `_filter_type` over the frozen `_FILTER_TYPE` default table — the swappable `GROUND_CLASSIFY` and `DECIMATE` stages reading their `IngestPolicy.ground_filter`/`decimate_filter` override (`filters.smrf`/`filters.pmf`, `filters.decimation`/`filters.voxeldownsize`), the fixed stages reading the table — fed as the single positional `type` to `pdal.Filter(type, **options)` so the `type` is never also a key inside the option dict; each stage's `_options` dict carries only the non-`type` knobs read from `IngestPolicy` (the SMRF/PMF window/cell/slope, the outlier mean-k/multiplier, the decimation step or voxel cell size, the range limits) so a stage's knobs are a row, never a branch; the `E57` path reads `e.scan_count` scans (`pye57.md#42`), `e.read_scan_raw(index)` for each scan's field-keyed dict (`pye57.md#47`), applies the per-scan pose through `e.to_global(points, header.rotation, header.translation)` (`pye57.md#49`/`#67`/`#68`) — `to_global` takes the length-4 pose quaternion (`ScanHeader.rotation`) and derives the rotation matrix internally, never the pre-derived `rotation_matrix` — folding the multi-scan global-frame Cartesian arrays into one structured array, and then converges on the same `_filter_graph`.
- Receipt: each ingest folds an `IngestReceipt` carrying the `ScanSource`, the applied `IngestStage` tuple (the realized order the filter graph ran), and the input/output point counts (the structured-array length before the graph and `p.arrays[0]` length after), and contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the source, the stage tuple, the decimation ratio, and elapsed; ingestion mints no `GraduationReceipt` subject — the cleaned cloud is an intra-folder precondition the `register` rail graduates downstream, never a geometry-case handoff of its own.
- Packages: `pdal` (`Filter(type, **options)` (`pdal.md#73`)/`stage.pipeline(*arrays) -> Pipeline` in-memory array bind (`pdal.md#77`)/`stage | other -> Pipeline` pipe composition (`pdal.md#75`)/`p.execute()` (`pdal.md#43`)/`p.arrays -> list[structured ndarray]` (`pdal.md#53`)), `pye57` (`E57(path, 'r')`/`e.scan_count` (`pye57.md#42`)/`e.read_scan_raw(index)` (`pye57.md#47`)/`e.get_header(index).rotation` (the length-4 pose quaternion) `/.translation` (`pye57.md#67`/`#68`)/`e.to_global(points, rotation, translation)` (`pye57.md#49`)/`e.close()`), `open3d` (`o3d.core.Tensor`/`o3d.t.geometry.PointCloud` egress), `numpy` (structured-array assembly from the Arrow columns and the E57 field dict), runtime (`RuntimeRail`/`boundary`/`ReceiptContributor`). `laspy` is consumed transitively through the data-branch bridge, never imported or re-owned here.
- Growth: a new cleaning stage is one `IngestStage` row plus one `_FILTER_TYPE` table entry (or an `_filter_type` policy-override arm for a swappable stage) plus one `_options` match arm plus one `IngestPolicy` option field — never a new method; a new source format is one `ScanSource` row plus one inbound decode arm converging on the shared `_filter_graph`; a new ground-classification model is the `filters.smrf`/`filters.pmf` row swap inside `GROUND_CLASSIFY`, an `IngestPolicy` field; the `pdal` filter-graph kernel (the multi-second SMRF/voxel sweep over a dense scan) hands across the runtime `concurrency/lanes.md#LANES` `LanePolicy` CPU-offload variant (`anyio.to_interpreter.run_sync` with `to_process.run_sync` fallback under one `CapacityLimiter`) as ONE hand-off call once that branch variant lands — the lane never imports the kernel, and the offloaded `_filter_graph` callable is picklable on the `to_process` fallback; zero new surface, no parallel per-stage filter class family, no per-format `read_las`/`read_e57` entrypoint family.
- Boundary: the inbound LAS/LAZ/COPC decode and the `pyarrow.Table` columnar point-record bridge are the `data/mesh/exchange.md#POINTCLOUD` owner's (`laspy` full decode and the `pdal` COPC octree subset live there), so ingestion never re-reads LAS and never crosses a `pdal` `Pipeline` object at the data seam — it receives a decoded `pyarrow.Table` and owns only the filter graph the data owner does not run; the E57 path is legitimately ingestion's because `pye57` is absent from the data branch; the registration that consumes the cleaned cloud is `scan-registration/registration.md#REGISTRATION` (a same-folder read-only seam, never re-derived here); no IFC parse (that is `ifc-analysis`), no registration, no deviation, no reconstruction, no tessellation, no durable store, no Rhino/GH mutation; a `Reader`-based file re-read where the data bridge already decoded, a per-stage filter method family, a stringly-typed stage dispatch, a hand-rolled SMRF/outlier/voxel kernel where `pdal` owns the filter, and a `laspy` LAS re-decode where `data/mesh/exchange.md#POINTCLOUD` owns it are the deleted forms — the structured array enters in-memory, the `IngestStage` table drives the graph, and the cleaned cloud egresses as one tensor `PointCloud`.

```python contract
import functools
import numpy as np
from enum import StrEnum
from typing import assert_never
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


# --- [TYPES] ----------------------------------------------------------------------------

class ScanSource(StrEnum):
    ARROW_LAS = "arrow-las"
    E57 = "e57"


class IngestStage(StrEnum):
    GROUND_CLASSIFY = "ground-classify"
    OUTLIER_REMOVE = "outlier-remove"
    DECIMATE = "decimate"
    RANGE_CROP = "range-crop"


# --- [MODELS] ---------------------------------------------------------------------------

class IngestPolicy(Struct, frozen=True):
    stages: tuple[IngestStage, ...] = (IngestStage.OUTLIER_REMOVE, IngestStage.DECIMATE)
    ground_filter: str = "filters.smrf"
    ground_window: float = 18.0
    ground_cell: float = 1.0
    ground_slope: float = 0.15
    outlier_mean_k: int = 8
    outlier_multiplier: float = 2.2
    decimate_filter: str = "filters.voxeldownsize"
    decimate_step: int = 4
    voxel_cell: float = 0.05
    range_limits: str = "Z[0:30]"


class IngestReceipt(Struct, frozen=True):
    source: ScanSource
    stages: tuple[IngestStage, ...]
    input_points: int
    output_points: int


# --- [SERVICES] -------------------------------------------------------------------------

class ScanIngestion(Struct, frozen=True):
    policy: IngestPolicy = IngestPolicy()

    def ingest(self, source_token: object, source: ScanSource) -> "RuntimeRail[tuple[object, IngestReceipt]]":
        return boundary(f"ingestion.{source}", lambda: self._dispatch(source_token, source))

    def _dispatch(self, source_token: object, source: ScanSource) -> tuple[object, IngestReceipt]:
        match source:
            case ScanSource.ARROW_LAS:
                points = self._arrow_to_structured(source_token)
            case ScanSource.E57:
                points = self._read_e57(source_token)
            case unreachable:
                assert_never(unreachable)
        cloud, cleaned, applied = self._filter_graph(points)
        receipt = IngestReceipt(source, applied, int(points.shape[0]), int(cleaned.shape[0]))
        return cloud, receipt

    def _arrow_to_structured(self, table: object) -> "np.ndarray":
        dtype = np.dtype([(name, np.float64) for name in ("X", "Y", "Z")])
        out = np.empty(table.num_rows, dtype=dtype)
        for arrow_name, pdal_name in (("x", "X"), ("y", "Y"), ("z", "Z")):
            out[pdal_name] = table.column(arrow_name).to_numpy(zero_copy_only=False)
        return out

    def _read_e57(self, path: str) -> "np.ndarray":
        import pye57  # noqa: PLC0415

        handle = pye57.E57(path, mode="r")
        try:
            blocks: list[np.ndarray] = []
            for index in range(handle.scan_count):
                fields = handle.read_scan_raw(index)
                header = handle.get_header(index)
                local = np.column_stack((fields["cartesianX"], fields["cartesianY"], fields["cartesianZ"]))
                world = handle.to_global(local, header.rotation, header.translation)
                block = np.empty(world.shape[0], dtype=np.dtype([(n, np.float64) for n in ("X", "Y", "Z")]))
                block["X"], block["Y"], block["Z"] = world[:, 0], world[:, 1], world[:, 2]
                blocks.append(block)
        finally:
            handle.close()
        return np.concatenate(blocks) if blocks else np.empty(0, dtype=np.dtype([(n, np.float64) for n in ("X", "Y", "Z")]))

    def _filter_graph(self, points: "np.ndarray") -> "tuple[object, np.ndarray, tuple[IngestStage, ...]]":
        import open3d as o3d  # noqa: PLC0415
        import pdal  # noqa: PLC0415

        stages = tuple(pdal.Filter(self._filter_type(stage), **self._options(stage)) for stage in self.policy.stages)
        if stages:
            pipeline = functools.reduce(lambda acc, stage: acc | stage, stages[1:], stages[0].pipeline(points))
            pipeline.execute()
            cleaned = pipeline.arrays[0]
        else:
            cleaned = points
        positions = np.column_stack((cleaned["X"], cleaned["Y"], cleaned["Z"])).astype(np.float32)
        cloud = o3d.t.geometry.PointCloud()
        cloud.point.positions = o3d.core.Tensor(positions)
        return cloud, cleaned, self.policy.stages

    def _filter_type(self, stage: IngestStage) -> str:
        match stage:
            case IngestStage.GROUND_CLASSIFY:
                return self.policy.ground_filter
            case IngestStage.DECIMATE:
                return self.policy.decimate_filter
            case _:
                return _FILTER_TYPE[stage]

    def _options(self, stage: IngestStage) -> dict[str, object]:
        match stage:
            case IngestStage.GROUND_CLASSIFY:
                window = {"window": self.policy.ground_window} if self.policy.ground_filter == "filters.smrf" else {"max_window_size": self.policy.ground_window}
                return {"cell": self.policy.ground_cell, "slope": self.policy.ground_slope, **window}
            case IngestStage.OUTLIER_REMOVE:
                return {"method": "statistical", "mean_k": self.policy.outlier_mean_k, "multiplier": self.policy.outlier_multiplier}
            case IngestStage.DECIMATE:
                return {"step": self.policy.decimate_step} if self.policy.decimate_filter == "filters.decimation" else {"cell": self.policy.voxel_cell}
            case IngestStage.RANGE_CROP:
                return {"limits": self.policy.range_limits}
            case unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

_FILTER_TYPE: dict[IngestStage, str] = {
    IngestStage.GROUND_CLASSIFY: "filters.smrf",
    IngestStage.OUTLIER_REMOVE: "filters.outlier",
    IngestStage.DECIMATE: "filters.decimation",
    IngestStage.RANGE_CROP: "filters.range",
}
```

## [3]-[RESEARCH]

- [FILTER_TYPE_MEMBERS]: the `pdal` filter `type` strings the `IngestStage` table binds (`filters.smrf`/`filters.pmf` ground classification, `filters.outlier` statistical/radius removal, `filters.decimation`/`filters.voxeldownsize` downsampling, `filters.range` crop) and their option keys (the SMRF `window`/`cell`/`slope`, the PMF `max_window_size`, the outlier `method`/`mean_k`/`multiplier`, the decimation `step`, the voxeldownsize `cell`, the range `limits` dimension-bounded string) confirm against the live `pdal` driver registry on the companion interpreter — `pdal.md` carries the `Filter(type, **options)` construction (`#73-2`), the `stage.pipeline(*arrays) -> Pipeline` in-memory array bind (`#77-6`), the pipe-operator `stage | other -> Pipeline` composition (`#75-4`), the `p.execute() -> int` run (`#43-2`), and the `p.arrays -> list[structured ndarray]` egress (`#53-1`); the only open items are the exact PMF option-key spelling versus SMRF and the `filters.range` multi-dimension `limits` syntax, live-run-tuned heuristics, not catalogue dependencies.
- [E57_FIELD_KEYS]: the `pye57.read_scan_raw(index) -> dict` field keys (`cartesianX`/`cartesianY`/`cartesianZ` for the Cartesian point block — `pye57.md#85` documents the `read_scan` field set, `read_scan_raw` returning the same field names without the inline pose transform), the `e.scan_count` multi-scan count (`#42-2`), and the `e.to_global(points, rotation, translation) -> ndarray` global-frame application (`#49-9`) confirm against the `pye57` catalogue on the companion interpreter. `to_global` is the `@staticmethod` that derives the rotation matrix from the length-4 pose quaternion internally (`Quaternion(rotation).rotation_matrix`), so the arm passes `header.rotation` (the quaternion, `#67-7`) and `header.translation` (`#68-8`), never the pre-derived `header.rotation_matrix`. The open item is whether a sensor-frame source omits the pose (`header.has_pose()` — `#62-2`) so the `to_global` step is conditional, a per-source guard the live run reads, not a catalogue dependency.

## [4]-[UPSTREAM]

- [COMPANION_BAND]: `pdal`, `pye57`, and `open3d` ride the `python_version<'3.15'` companion band the branch manifest owns — `pdal` is sdist-only (native `libpdal` C++ dependency, no cp315 wheel), `pye57` is the `libe57` wrapper with no cp315 wheel, and `open3d` caps at the cp312 compiled core — so all three import function-local under `# noqa: PLC0415` at boundary scope inside `_read_e57` and `_filter_graph`, never module-top, per the manifest import policy; `laspy` (cp315-clean) is never imported here because the LAS decode rides the data-branch bridge. The `open3d` row carries the manifest-admission delta shared with `scan-registration/reconstruction.md` (verified absent from the manifest despite `registration.md` and `deviation.md` already consuming it): one `open3d` row at `python_version<'3.15'` admits on the branch manifest, a pre-existing delta two authored pages already need, not introduced by this page.
- [OFFLOAD_LANE]: the `_filter_graph` SMRF/voxel kernel is one of the heavy geometry CPU kernels the runtime `concurrency/lanes.md#LANES` `LanePolicy` CPU-offload variant absorbs once that branch owner lands; the hand-off is a uniform one-call growth on the Growth bullet shared with `daemon.md#DAEMON`, `registration.md#REGISTRATION`, `reconstruction.md#RECONSTRUCTION`, and `repair.md#MESH`, sequence-after the runtime lane, never a second concurrency surface minted in geometry.
