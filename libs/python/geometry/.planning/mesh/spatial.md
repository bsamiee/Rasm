# [PY_GEOMETRY_MESH_SPATIAL]

Spatial query over an in-memory triangulation: the proximity, ray, containment, bounds, clearance, and sampling primitive `scan/deviation#DEVIATION` composes against a built reference mesh — the one owner of the surface-projection index, so no consumer re-spells a `ProximityQuery` build of its own. `SpatialQuery` discriminates the kind on one polymorphic entrypoint, `SpatialResult` mirrors the query case, and every kind flows through one work-class-aware sweep over a single `_dispatch` body — an index-cached kind runs synchronously under `boundary` against the capsule mesh's own lazily-cached indices, a batch-heavy kind offloads the SAME body as a `KernelTrait.HOSTILE` kernel onto the warm process pool, so the offload-vs-sync choice never forks the geometry dispatch. Offload membership is declared by work class, never derived from module presence: a process-pool worker shares the one venv, so a native module absent in-process is equally absent on the worker floor. This owner indexes an in-memory `trimesh.Trimesh` and returns numpy arrays across the wire; mesh-file decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`).

Spine is `trimesh` and `numpy` — never a phantom `scipy` spine, since no geometry fence imports a scipy member — with the admitted `rtree`/`python-fcl` native band composed at the DIRECT surface, not a one-call trimesh veneer. The clearance kernel reads the `mesh/repair#MESH` `ManifoldTier` probe, the folder's ONE exact-geometry capability vocabulary: the exact `manifold3d.Manifold.min_gap` supersedes the conservative FCL separation, both operands built through repair's public `to_manifold`, and a floor resolving neither answers the same invalid verdict an open pair answers rather than raising inside a worker. `Contains`/`Clearance` arms gate `is_watertight` BEFORE the query, so an off-solid test is an admitted caveat rather than a meaningless mask.

## [01]-[INDEX]

- [02]-[SPATIAL]: query kinds under tagged union over the `trimesh`/`numpy` spine and the `rtree`/`python-fcl`/`manifold3d` native band, work-class-routed under one batch sweep, returning `SpatialResult` union.

## [02]-[SPATIAL]

- Owner: `MeshSpatial` — the boundary capsule over the one module-level `_dispatch`; the `_OFFLOAD` membership is declared work-class data — the batch-heavy kinds ride the process kernel, the index-cached kinds stay in-process — never a hardcoded per-case branch and never a module-presence probe, so a new kind writes only its `_dispatch` arm and its membership row.
- Cases: `Ray` reduces the all-hits return to the nearest hit per ray in one vectorized pass, never a per-ray cast; `Bounds`/`Nearest` run ONE vectorized `intersection_v`/`nearest_v` call for the whole box set, never a per-box loop; `Contains` answers `Option[np.ndarray]`, `Nothing` off a non-watertight mesh, so an off-solid test never reads as an empty inside set; `Clearance` carries a backend-shaped payload over TWO independent `Option` slots — the FCL arm returns the signed gap and the `nearest_points` witness pair, the exact `manifold3d` arm the gap alone, and an open pair or an unprovisioned floor neither, where the retired `float("nan")` handed a consumer a magnitude it could compare, sort, and average as though a kernel had measured it.
- Law: arity is absorbed at the head and the CROSSING is per work class, never per query — the batch-heavy members of a whole batch ship as ONE `HOSTILE` kernel invocation, so the worker's `trimesh` mesh caches its `triangles_tree` and proximity index across every member and an N-query batch pays one index construction where a per-member hop pays N; the index-cached members read the capsule's own cached tree in-process, and both legs reassemble by admission ordinal so the returned `Block` aligns with the input.
- Law: each sweep writes ONE `structlog` line at the capsule — query count, offloaded count, and the resolved tier — bound to the capsule's composition logger, so a batch reads as one line and an empty sweep writes nothing.
- Entry: `query` is the one polymorphic read and `benched` its macro-bench; the capsule's composition `ScopeKey` binds the logger, so an embedded root's lines partition from the process root's.
- Auto: an offloaded sweep rebuilds whatever index its hop needs inside the worker process — a live R-tree, FCL model, or `Manifold` is a native handle no pickler carries, so only the numpy-backed `Trimesh` crosses the seam and the capsule caches no native handle — while the in-process kinds read the lazily-cached `triangles_tree`/`kdtree` indices the capsule `Trimesh` owns and amortizes across calls.
- Packages: `trimesh` (proximity/ray/contains/sample and the cached indices), `numpy`, `rtree` (the `triangles_tree` R-tree), `python-fcl` (the direct narrow-phase `fcl.distance`, deferred through one module-scope `lazy import` so its interpreter-marked absence costs nothing until the witness tier is selected), `manifold3d` (through repair's `to_manifold`), `expression`, `structlog` through the runtime `logger`, and the runtime rails per the fence imports.

- Growth: a new query kind is one `SpatialQuery` case and its mirrored `SpatialResult` arm and one `_dispatch` arm — `assert_never` forces the closure; a new exact-geometry provider is one `ManifoldTier` row at `mesh/repair#MESH`, never a probe minted here.
- Boundary: vertex-KNN acceleration (`open3d.geometry.KDTreeFlann`, `small_gicp.KdTree.batch_knn_search`) is NOT this owner's backend — a vertex nearest-neighbor is a coarser, distinct result from `closest_point`'s exact on-surface projection, so that acceleration belongs to the `scan/registration` consumer that owns the cloud-to-vertex correspondence; IFC clash detection is `ifc/analysis#ANALYSIS`'s `ifcclash` drive, never this index; conditioning is `mesh/repair#MESH`'s and the capability probe with it; metrology is `mesh/quality#QUALITY`'s.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Sequence
from functools import partial
from typing import Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map


from rasm.geometry.graduation import EvidenceScope, GeometryLeg, bench_seam, bench_subject
from rasm.geometry.mesh.repair import ManifoldTier, to_manifold
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey, logger
from rasm.runtime.profiles import Benchmark
from rasm.runtime.workers import Kernel, KernelTrait

lazy import fcl

# --- [TYPES] ----------------------------------------------------------------------------

type QueryKind = Literal["proximity", "ray", "contains", "bounds", "nearest", "clearance", "sample"]
type Witness = tuple[tuple[float, float, float], tuple[float, float, float]]
type Gap = tuple[float, Option[Witness]]


@tagged_union(frozen=True)
class SpatialQuery:
    tag: QueryKind = tag()
    proximity: tuple[np.ndarray, bool] = case()
    ray: tuple[np.ndarray, np.ndarray, float] = case()
    contains: np.ndarray = case()
    bounds: np.ndarray = case()
    nearest: tuple[np.ndarray, int] = case()
    clearance: tuple[trimesh.Trimesh, float] = case()
    sample: tuple[int, bool, bool] = case()

    @staticmethod
    def Proximity(points: np.ndarray, signed: bool = False) -> "SpatialQuery":
        return SpatialQuery(proximity=(points, signed))

    @staticmethod
    def Ray(
        origins: np.ndarray, directions: np.ndarray, max_distance: float = float("inf")
    ) -> "SpatialQuery":
        return SpatialQuery(ray=(origins, directions, max_distance))

    @staticmethod
    def Contains(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(contains=points)

    @staticmethod
    def Bounds(boxes: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(bounds=boxes)

    @staticmethod
    def Nearest(boxes: np.ndarray, num_results: int = 1) -> "SpatialQuery":
        return SpatialQuery(nearest=(boxes, num_results))

    @staticmethod
    def Clearance(other: trimesh.Trimesh, search_length: float) -> "SpatialQuery":
        return SpatialQuery(clearance=(other, search_length))

    @staticmethod
    def Sample(count: int, even: bool = True, attribute: bool = False) -> "SpatialQuery":
        return SpatialQuery(sample=(count, even, attribute))


@tagged_union(frozen=True)
class SpatialResult:
    tag: QueryKind = tag()
    proximity: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray] = case()
    ray: tuple[np.ndarray, np.ndarray, np.ndarray] = case()
    contains: Option[np.ndarray] = case()
    bounds: tuple[np.ndarray, ...] = case()
    nearest: tuple[np.ndarray, ...] = case()
    clearance: tuple[Option[float], Option[Witness]] = case()
    sample: tuple[np.ndarray, np.ndarray, np.ndarray] = case()

    @staticmethod
    def Proximity(points: np.ndarray, distances: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(proximity=(points, distances, triangle_ids, signed))

    @staticmethod
    def Ray(triangle_ids: np.ndarray, points: np.ndarray, t: np.ndarray) -> "SpatialResult":
        return SpatialResult(ray=(triangle_ids, points, t))

    @staticmethod
    def Contains(mask: Option[np.ndarray] = Nothing) -> "SpatialResult":
        return SpatialResult(contains=mask)

    @staticmethod
    def Bounds(candidates: tuple[np.ndarray, ...]) -> "SpatialResult":
        return SpatialResult(bounds=candidates)

    @staticmethod
    def Nearest(candidates: tuple[np.ndarray, ...]) -> "SpatialResult":
        return SpatialResult(nearest=candidates)

    @staticmethod
    def Clearance(gap: Option[float] = Nothing, witness: Option[Witness] = Nothing) -> "SpatialResult":
        return SpatialResult(clearance=(gap, witness))

    @staticmethod
    def Sample(points: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(sample=(points, triangle_ids, signed))


# --- [CONSTANTS] ------------------------------------------------------------------------

_OFFLOAD: Final[frozenset[QueryKind]] = frozenset(("proximity", "ray", "contains", "clearance", "sample"))

_INDEX_RAISES: Final[Catch] = (IndexError, TypeError, ValueError)

# --- [TABLES] ---------------------------------------------------------------------------

SPATIAL_INDEXED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.SPATIAL, point="indexed", arm="boundary", defect="index-read-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SPATIAL_INDEXED]))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _f64(a: np.ndarray) -> np.ndarray:
    return np.asarray(a, dtype=np.float64)


def _unit(d: np.ndarray) -> np.ndarray:
    norm = np.linalg.norm(d, axis=1, keepdims=True)
    return np.divide(d, norm, out=np.zeros_like(d), where=norm > 0)


def _nearest_hits(
    n: int, locations: np.ndarray, ray_idx: np.ndarray, tri_idx: np.ndarray, origins: np.ndarray, max_distance: float
) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    face = np.full(n, -1, dtype=np.int64)
    pos = np.full((n, 3), np.nan, dtype=np.float64)
    dist = np.full(n, np.nan, dtype=np.float64)
    if ray_idx.size == 0:
        return face, pos, dist
    hit_dist = np.linalg.norm(locations - origins[ray_idx], axis=1)
    order = np.lexsort((hit_dist, ray_idx))
    ri = ray_idx[order]
    keep = np.concatenate(([True], ri[1:] != ri[:-1])) & (hit_dist[order] <= max_distance)
    face[ri[keep]], pos[ri[keep]], dist[ri[keep]] = tri_idx[order][keep], locations[order][keep], hit_dist[order][keep]
    return face, pos, dist


def _gap(mesh: trimesh.Trimesh, other: trimesh.Trimesh, search_length: float, tier: "Option[ManifoldTier]") -> "Option[Gap]":
    match tier:
        case Option(tag="some", some=ManifoldTier.EXACT):
            return Some((float(to_manifold(mesh).min_gap(to_manifold(other), search_length)), Nothing))
        case Option(tag="some", some=ManifoldTier.FCL_WITNESS):
            return Some(_fcl_gap(mesh, other))
        case _:
            return Nothing


def _dispatch(mesh: trimesh.Trimesh, q: SpatialQuery, tier: "Option[ManifoldTier]") -> SpatialResult:
    match q:
        case SpatialQuery(tag="proximity", proximity=(points, signed)):
            pts = _f64(points)
            near, distance, triangle_ids = trimesh.proximity.closest_point(mesh, pts)
            signed_field = trimesh.proximity.signed_distance(mesh, pts) if signed else np.empty(0)
            return SpatialResult.Proximity(near, _f64(distance), np.asarray(triangle_ids), _f64(signed_field))
        case SpatialQuery(tag="ray", ray=(origins, directions, max_distance)):
            o = _f64(origins)
            locations, ray_idx, tri_idx = mesh.ray.intersects_location(o, _unit(_f64(directions)))
            return SpatialResult.Ray(*_nearest_hits(len(o), _f64(locations), np.asarray(ray_idx), np.asarray(tri_idx), o, max_distance))
        case SpatialQuery(tag="contains", contains=points):
            return SpatialResult.Contains(Some(np.asarray(mesh.contains(_f64(points)), dtype=bool)) if mesh.is_watertight else Nothing)
        case SpatialQuery(tag="bounds", bounds=boxes):
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.intersection_v(rows[:, :3], rows[:, 3:])
            return SpatialResult.Bounds(tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1])))
        case SpatialQuery(tag="nearest", nearest=(boxes, num_results)):
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.nearest_v(rows[:, :3], rows[:, 3:], num_results=num_results)
            return SpatialResult.Nearest(tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1])))
        case SpatialQuery(tag="sample", sample=(count, even, attribute)):
            sampler = trimesh.sample.sample_surface_even if even else trimesh.sample.sample_surface
            points, triangle_ids = sampler(mesh, count)
            signed = trimesh.proximity.signed_distance(mesh, points) if attribute else np.empty(0)
            return SpatialResult.Sample(np.asarray(points), np.asarray(triangle_ids), _f64(signed))
        case SpatialQuery(tag="clearance", clearance=(other, search_length)):
            gapped = _gap(mesh, other, search_length, tier) if bool(mesh.is_watertight and other.is_watertight) else Nothing
            return gapped.map(lambda pair: SpatialResult.Clearance(Some(pair[0]), pair[1])).default_with(SpatialResult.Clearance)
        case _ as unreachable:
            assert_never(unreachable)


def _swept_kernel(mesh: trimesh.Trimesh, queries: tuple[SpatialQuery, ...], tier: "Option[ManifoldTier]") -> tuple[SpatialResult, ...]:
    return tuple(_dispatch(mesh, one, tier) for one in queries)


def _bvh(mesh: trimesh.Trimesh) -> "fcl.CollisionObject":
    model = fcl.BVHModel()
    model.beginModel(len(mesh.vertices), len(mesh.faces))
    model.addSubModel(mesh.vertices, mesh.faces)
    model.endModel()
    return fcl.CollisionObject(model)


def _fcl_gap(mesh: trimesh.Trimesh, other: trimesh.Trimesh) -> Gap:
    request = fcl.DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)
    result = fcl.DistanceResult()
    gap = float(fcl.distance(_bvh(mesh), _bvh(other), request, result))
    a, b = result.nearest_points
    return gap, Some((tuple(float(v) for v in a), tuple(float(v) for v in b)))


def _ordered(held: Map[int, SpatialResult]) -> Block[SpatialResult]:
    return Block.of_seq(held.items()).map(lambda row: row[1])


# --- [SERVICES] -------------------------------------------------------------------------


class MeshSpatial:
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> None:
        self._mesh = mesh
        self._lane = lane
        self._tier = ManifoldTier.resolve()
        self._composition = composition

    @overload
    async def query(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]": ...
    @overload
    async def query(self, q: Sequence[SpatialQuery]) -> "RuntimeRail[Block[SpatialResult]]": ...
    async def query(self, q: SpatialQuery | Sequence[SpatialQuery]) -> "RuntimeRail[SpatialResult] | RuntimeRail[Block[SpatialResult]]":
        match q:
            case SpatialQuery() as one:
                return (await self._swept(Block.singleton(one))).map(lambda kept: kept.head())
            case batch:
                return await self._swept(Block.of_seq(batch))

    async def _swept(self, queries: Block[SpatialQuery]) -> "RuntimeRail[Block[SpatialResult]]":
        rows = queries.mapi(lambda i, one: (i, one))
        heavy, light = rows.partition(lambda row: row[1].tag in _OFFLOAD)
        indexed = boundary(
            SPATIAL_INDEXED, lambda: light.map(lambda row: (row[0], _dispatch(self._mesh, row[1], self._tier))), catch=_INDEX_RAISES
        )
        logger(self._composition).info(
            "spatial.swept", queries=len(rows), offloaded=len(heavy), tier=self._tier.map(lambda held: held.value).default_value("")
        )
        return (await self._heavy(heavy)).map2(indexed, lambda a, b: Map.of_seq(a.append(b))).map(_ordered)

    async def _heavy(self, rows: Block[tuple[int, SpatialQuery]]) -> "RuntimeRail[Block[tuple[int, SpatialResult]]]":
        if rows.is_empty():
            return Ok(Block.empty())
        kernel = Kernel.of(_swept_kernel, KernelTrait.HOSTILE)
        railed = await self._lane.offload(kernel, self._mesh, tuple(row[1] for row in rows), self._tier)
        return railed.map(lambda out: Block.of_seq(zip((row[0] for row in rows), out, strict=True)))

    def benched(self, q: SpatialQuery | Sequence[SpatialQuery], *, rounds: int = 32, warmup: int = 4) -> "RuntimeRail[Benchmark]":
        kind = q.tag if isinstance(q, SpatialQuery) else "batch"
        return bench_seam(bench_subject(EvidenceScope.MESH_SPATIAL, kind), partial(self.query, q), rounds=rounds, warmup=warmup)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
