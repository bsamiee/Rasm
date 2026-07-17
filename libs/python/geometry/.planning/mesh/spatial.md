# [PY_GEOMETRY_MESH_SPATIAL]

Spatial query over an in-memory triangulation: the proximity, ray, containment, bounds, clearance, and sampling primitive the deviation, clash, and reconstruction hops compose against a built mesh. `SpatialQuery` discriminates the kind on one polymorphic entrypoint, `SpatialResult` mirrors the query case, and every kind flows through one work-class-aware `_route` over a single `_dispatch` body — an index-cached kind runs synchronously under `boundary` against the capsule mesh's own lazily-cached indices, a batch-heavy kind offloads the SAME body as a `KernelTrait.HOSTILE` kernel onto the warm process pool, so the offload-vs-sync choice never forks the geometry dispatch. Offload membership is declared by work class, never derived from module presence: a process-pool worker shares the one venv, so a native module absent in-process is equally absent on the worker floor, and a missing admission is an import fault at the seam, never a routing signal. This owner indexes an in-memory `trimesh.Trimesh` and returns numpy arrays across the wire; mesh-file decode/encode is the data `MeshPayload` owner's (`rasm.data.spatial.mesh`).

The spine is `trimesh` plus `numpy` — never a phantom `scipy` spine, since no geometry fence imports a scipy member — with the admitted `rtree`/`python-fcl` native band composed at the DIRECT surface, not a one-call trimesh veneer. `SpatialBackend` resolves once per capsule off the `find_spec` capability probe: the exact `manifold3d.Manifold.min_gap` clearance supersedes the conservative FCL separation when the richer package is admitted, both operands built through repair's public `to_manifold` (repair is the chartered `manifold3d` owner). The `Contains`/`Clearance` arms gate `is_watertight` BEFORE the query, so an off-solid test is an admitted caveat rather than a meaningless mask.

## [01]-[INDEX]

- [01]-[SPATIAL]: query kinds under tagged union over the `trimesh`/`numpy` spine and the `rtree`/`python-fcl`/`manifold3d` native band, work-class-routed, returning `SpatialResult` union.

## [02]-[SPATIAL]

- Owner: `MeshSpatial` — the boundary capsule over the one module-level `_dispatch`; the `_OFFLOAD` membership is declared work-class data — the batch-heavy kinds ride the process kernel, the index-cached kinds stay in-process — never a hardcoded per-case branch and never a module-presence probe, and the `_fold` cross-cut turns every `Outcome` into the one held receipt, so a new kind writes only its `_dispatch` arm plus its membership row.
- Cases: `Ray` reduces the all-hits return to the nearest hit per ray in one vectorized pass, never a per-ray cast; `Bounds`/`Nearest` run ONE vectorized `intersection_v`/`nearest_v` call for the whole box set, never a per-box loop; `Clearance` carries a backend-shaped payload — the FCL arm returns the signed gap plus the `nearest_points` witness pair, the exact `manifold3d` gap carries no witness.
- Auto: an offloaded kind rebuilds whatever index its hop needs inside the worker process — a live R-tree, FCL model, or `Manifold` is a native handle no pickler carries, so only the numpy-backed `Trimesh` crosses the seam and the capsule caches no native handle — while the in-process kinds read the lazily-cached `triangles_tree`/`kdtree` indices the capsule `Trimesh` owns and amortizes across calls.
- Packages: `trimesh` (proximity/ray/contains/sample and the cached indices), `numpy`, `rtree` (the `triangles_tree` R-tree), `python-fcl` (the direct narrow-phase `fcl.distance`), `manifold3d` (through repair's `to_manifold`), `expression`, `msgspec`, and the runtime rails per the fence imports.
- Growth: a new query kind is one `SpatialQuery` case plus its mirrored `SpatialResult` arm plus one `_dispatch` arm — `assert_never` forces the closure; a new native backend is one `SpatialBackend` row plus its probe module.
- Boundary: vertex-KNN acceleration (`open3d.geometry.KDTreeFlann`, `small_gicp.KdTree.batch_knn_search`) is NOT this owner's backend — a vertex nearest-neighbor is a coarser, distinct result from `closest_point`'s exact on-surface projection, so that acceleration belongs to the `scan/deviation`+`scan/registration` consumers that own the cloud-to-vertex correspondence; conditioning is `mesh/repair.md#MESH`'s, metrology is `mesh/quality.md#QUALITY`'s.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import TYPE_CHECKING, Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # annotation-only; the runtime binding is the function-local narrow-phase import
    import fcl

# --- [TYPES] ----------------------------------------------------------------------------

type QueryKind = Literal["proximity", "ray", "contains", "bounds", "nearest", "clearance", "sample"]


class SpatialBackend(StrEnum):
    CORE = "core"  # the conservative python-fcl separation
    MANIFOLD3D = "manifold3d"  # the exact Manifold.min_gap, superseding FCL where the richer package resolves

    @staticmethod
    def resolve() -> "SpatialBackend":
        return SpatialBackend.MANIFOLD3D if find_spec("manifold3d") is not None else SpatialBackend.CORE


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
    ) -> "SpatialQuery":  # intersects_location casts infinite rays; a finite max_distance clamps a far hit back to a miss
        return SpatialQuery(ray=(origins, directions, max_distance))

    @staticmethod
    def Contains(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(contains=points)

    @staticmethod
    def Bounds(boxes: np.ndarray) -> "SpatialQuery":  # Nx6 (xmin,ymin,zmin,xmax,ymax,zmax) AABB rows
        return SpatialQuery(bounds=boxes)

    @staticmethod
    def Nearest(boxes: np.ndarray, num_results: int = 1) -> "SpatialQuery":  # Nx6 AABB probes; k nearest candidate triangles per probe
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
    contains: np.ndarray = case()
    bounds: tuple[np.ndarray, ...] = case()
    nearest: tuple[np.ndarray, ...] = case()
    clearance: tuple[float, tuple[tuple[float, float, float], tuple[float, float, float]] | None] = case()
    sample: tuple[np.ndarray, np.ndarray, np.ndarray] = case()

    @staticmethod
    def Proximity(points: np.ndarray, distances: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(proximity=(points, distances, triangle_ids, signed))

    @staticmethod
    def Ray(triangle_ids: np.ndarray, points: np.ndarray, t: np.ndarray) -> "SpatialResult":
        return SpatialResult(ray=(triangle_ids, points, t))

    @staticmethod
    def Contains(mask: np.ndarray) -> "SpatialResult":
        return SpatialResult(contains=mask)

    @staticmethod
    def Bounds(candidates: tuple[np.ndarray, ...]) -> "SpatialResult":
        return SpatialResult(bounds=candidates)

    @staticmethod
    def Nearest(candidates: tuple[np.ndarray, ...]) -> "SpatialResult":
        return SpatialResult(nearest=candidates)

    @staticmethod
    def Clearance(gap: float, witness: tuple[tuple[float, float, float], tuple[float, float, float]] | None = None) -> "SpatialResult":
        # the witness contact pair rides the fcl arm (`DistanceResult.nearest_points`); the exact
        # manifold3d gap carries no witness — one case, backend-shaped payload.
        return SpatialResult(clearance=(gap, witness))

    @staticmethod
    def Sample(points: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(sample=(points, triangle_ids, signed))


# --- [CONSTANTS] ------------------------------------------------------------------------

# declared work-class membership: the batch-heavy kinds (surface projection, ray casting, containment, sampling, and
# the native narrow-phase gap) offload as HOSTILE process kernels, while `bounds`/`nearest` stay in-process to reuse
# the capsule mesh's cached `triangles_tree` R-tree a per-hop worker rebuild would forfeit.
_OFFLOAD: Final[frozenset[QueryKind]] = frozenset(("proximity", "ray", "contains", "clearance", "sample"))


# --- [MODELS] ---------------------------------------------------------------------------


class Outcome(Struct, frozen=True):  # one arm's payload plus receipt facts; GC-tracked since `result` nests numpy arrays
    result: SpatialResult
    query_count: int
    hit_count: int
    valid: bool


class SpatialReceipt(Struct, frozen=True, gc=False):
    kind: QueryKind
    backend: SpatialBackend
    offloaded: bool
    query_count: int
    hit_count: int
    valid: bool
    indexed: bool


# --- [OPERATIONS] -----------------------------------------------------------------------


def _f64(a: np.ndarray) -> np.ndarray:
    return np.asarray(a, dtype=np.float64)


def _unit(d: np.ndarray) -> np.ndarray:  # a zero-direction row degrades to a zero ray (a downstream miss), never a divide-by-zero
    norm = np.linalg.norm(d, axis=1, keepdims=True)
    return np.divide(d, norm, out=np.zeros_like(d), where=norm > 0)


def _nearest_hits(
    n: int, locations: np.ndarray, ray_idx: np.ndarray, tri_idx: np.ndarray, origins: np.ndarray, max_distance: float
) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    # ALL hits per ray reduce to nearest-per-ray via one lexsort and the first-of-group mask, scattered into the
    # dense ray order — a missed ray keeps tri==-1/NaN, and a hit past max_distance clamps back to a miss.
    face = np.full(n, -1, dtype=np.int64)
    pos = np.full((n, 3), np.nan, dtype=np.float64)
    dist = np.full(n, np.nan, dtype=np.float64)
    if ray_idx.size == 0:  # every ray missed
        return face, pos, dist
    hit_dist = np.linalg.norm(locations - origins[ray_idx], axis=1)
    order = np.lexsort((hit_dist, ray_idx))  # primary ray_idx, secondary distance => first-of-group is the nearest hit
    ri = ray_idx[order]
    keep = np.concatenate(([True], ri[1:] != ri[:-1])) & (hit_dist[order] <= max_distance)
    face[ri[keep]], pos[ri[keep]], dist[ri[keep]] = tri_idx[order][keep], locations[order][keep], hit_dist[order][keep]
    return face, pos, dist


# the one exhaustive dispatch, module-level and arg-only so REFERENCE shipping resolves it on the worker floor;
# `backend` selects the clearance kernel, and `_route` decides sync-vs-offload — never a second dispatch body.
def _dispatch(mesh: trimesh.Trimesh, q: SpatialQuery, backend: SpatialBackend) -> Outcome:
    match q:
        case SpatialQuery(tag="proximity", proximity=(points, signed)):
            pts = _f64(points)
            near, distance, triangle_ids = trimesh.proximity.closest_point(mesh, pts)
            signed_field = trimesh.proximity.signed_distance(mesh, pts) if signed else np.empty(0)
            return Outcome(SpatialResult.Proximity(near, _f64(distance), np.asarray(triangle_ids), _f64(signed_field)), len(pts), len(pts), True)
        case SpatialQuery(
            tag="ray", ray=(origins, directions, max_distance)
        ):  # pure-Python RayMeshIntersector batch; the Embree mirror is a transparent in-process upgrade when embreex resolves
            o = _f64(origins)
            locations, ray_idx, tri_idx = mesh.ray.intersects_location(o, _unit(_f64(directions)))
            face, pos, dist = _nearest_hits(len(o), _f64(locations), np.asarray(ray_idx), np.asarray(tri_idx), o, max_distance)
            return Outcome(SpatialResult.Ray(face, pos, dist), len(o), int((face != -1).sum()), True)
        case SpatialQuery(tag="contains", contains=points):
            if not mesh.is_watertight:  # inside/outside is meaningless off a watertight solid; gate first, flag the caveat
                return Outcome(SpatialResult.Contains(np.zeros(len(points), dtype=bool)), len(points), 0, False)
            mask = np.asarray(mesh.contains(_f64(points)), dtype=bool)
            return Outcome(SpatialResult.Contains(mask), len(mask), int(mask.sum()), True)
        case SpatialQuery(tag="bounds", bounds=boxes):
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.intersection_v(rows[:, :3], rows[:, 3:])  # ONE vectorized call: flat candidate ids + per-box counts
            candidates = tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1]))
            return Outcome(SpatialResult.Bounds(candidates), len(rows), int(ids.size), True)
        case SpatialQuery(tag="nearest", nearest=(boxes, num_results)):
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.nearest_v(rows[:, :3], rows[:, 3:], num_results=num_results)  # k nearest candidate triangles per probe box
            candidates = tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1]))
            return Outcome(SpatialResult.Nearest(candidates), len(rows), int(ids.size), True)
        case SpatialQuery(tag="sample", sample=(count, even, attribute)):
            sampler = trimesh.sample.sample_surface_even if even else trimesh.sample.sample_surface
            points, triangle_ids = sampler(mesh, count)
            signed = trimesh.proximity.signed_distance(mesh, points) if attribute else np.empty(0)
            return Outcome(SpatialResult.Sample(np.asarray(points), np.asarray(triangle_ids), _f64(signed)), len(points), len(points), True)
        case SpatialQuery(tag="clearance", clearance=(other, search_length)):
            if not (mesh.is_watertight and other.is_watertight):  # a solid-to-solid gap demands both surfaces close
                return Outcome(SpatialResult.Clearance(float("nan")), 1, 0, False)
            if backend is SpatialBackend.MANIFOLD3D:
                gap, witness = float(to_manifold(mesh).min_gap(to_manifold(other), search_length)), None
            else:
                gap, witness = _fcl_gap(mesh, other)
            return Outcome(SpatialResult.Clearance(gap, witness), 1, int(gap <= search_length), True)
        case _ as unreachable:
            assert_never(unreachable)


def _bvh(mesh: trimesh.Trimesh) -> "fcl.CollisionObject":
    import fcl  # noqa: PLC0415

    model = fcl.BVHModel()
    model.beginModel(len(mesh.vertices), len(mesh.faces))
    model.addSubModel(mesh.vertices, mesh.faces)
    model.endModel()
    return fcl.CollisionObject(model)


def _fcl_gap(
    mesh: trimesh.Trimesh, other: trimesh.Trimesh
) -> tuple[float, tuple[tuple[float, float, float], tuple[float, float, float]]]:
    # the DIRECT narrow-phase read: signed separation plus the witness pair — richer than the unsigned
    # CollisionManager.min_distance_single float veneer.
    import fcl  # noqa: PLC0415

    request = fcl.DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)
    result = fcl.DistanceResult()
    gap = float(fcl.distance(_bvh(mesh), _bvh(other), request, result))
    a, b = result.nearest_points
    return gap, (tuple(float(v) for v in a), tuple(float(v) for v in b))


# --- [SERVICES] -------------------------------------------------------------------------


class MeshSpatial:  # structural ReceiptContributor conformance — no subclass
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, backend: SpatialBackend | None = None) -> None:
        self._mesh = mesh
        self._lane = lane  # the offload seam; the lane never imports the kernel
        self._backend = backend or SpatialBackend.resolve()  # the one find_spec read: tier selection, never offload routing
        self._last: SpatialReceipt | None = None

    @overload
    async def query(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]": ...
    @overload
    async def query(self, q: Sequence[SpatialQuery]) -> "RuntimeRail[Block[SpatialResult]]": ...
    async def query(self, q: SpatialQuery | Sequence[SpatialQuery]) -> "RuntimeRail[SpatialResult] | RuntimeRail[Block[SpatialResult]]":
        # the lane's own CapacityLimiter bounds the concurrent hops, so the batch needs no second pool.
        match q:
            case SpatialQuery() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(rails, by=Disposition.ACCUMULATE)  # a faulted query stays addressable in the aggregate

    async def _route(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]":
        # index-cached kinds run under `boundary`; batch-heavy kinds offload the SAME `_dispatch` as a HOSTILE kernel —
        # the native band imports under no isolated subinterpreter — both paths producing the one `Outcome` the
        # `_fold` cross-cut consumes.
        offloaded = q.tag in _OFFLOAD
        if offloaded:
            kernel = Kernel.of(_dispatch, KernelTrait.HOSTILE)
            return (await self._lane.offload(kernel, self._mesh, q, self._backend)).map(lambda out: self._fold(q, out, offloaded))
        return boundary(f"mesh.spatial.{q.tag}", lambda: self._fold(q, _dispatch(self._mesh, q, self._backend), offloaded))

    def _fold(self, q: SpatialQuery, out: Outcome, offloaded: bool) -> SpatialResult:  # the receipt cross-cut off each Outcome
        self._last = SpatialReceipt(q.tag, self._backend, offloaded, out.query_count, out.hit_count, out.valid, indexed=not offloaded)
        return out.result

    def contribute(self) -> Iterable[Receipt]:
        r = self._last or SpatialReceipt("proximity", self._backend, False, 0, 0, True, False)
        phase: Phase = "emitted" if r.valid else "admitted"
        facts: dict[str, object] = {
            "backend": r.backend.value,
            "offloaded": r.offloaded,
            "queries": r.query_count,
            "hits": r.hit_count,
            "indexed": r.indexed,
        }
        yield Receipt.of("mesh.spatial", (phase, r.kind, facts))  # subject is the query kind
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
