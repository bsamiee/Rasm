# [PY_GEOMETRY_MESH_SPATIAL]

Spatial query over an in-memory triangulation — the proximity, ray, containment, bounds, clearance, and sampling primitive the deviation, clash, and reconstruction hops compose against a built mesh. `SpatialQuery` is one tagged union discriminating by query kind; `query` is one polymorphic `async` entrypoint accepting one query or a batch sequence, amortizing the cached `kdtree` / `triangles_tree` / `ray` indices once per surface and folding every kind through one capability-aware `_route` — a core-resident kind on the synchronous `boundary`, a native-backend kind whose compiled package is absent on the local interpreter offloaded onto the `LanePolicy.offload` PEP 734 subinterpreter hop the companion carries — and `SpatialResult` is one union carrier whose case mirrors the query case so a proximity result, a ray result, a containment mask, a bounds-candidate set, a clearance scalar, and a sample set are arms of one result owner rather than six parallel result types. The pure-Python spine is `trimesh` plus `numpy` on the runtime: the module-level `proximity.closest_point`/`signed_distance` field samplers (riding the `Trimesh`-cached nearest index), the `mesh.contains` solid test, the `mesh.ray.intersects_location` batch ray intersector (the pure-Python `ray.ray_triangle.RayMeshIntersector`, transparently superseded by the `ray.ray_pyembree` Embree mirror when the native `embreex` package resolves), and `sample.sample_surface`/`sample_surface_even` — never a single flat call and never a phantom `scipy` spine (no geometry fence imports a scipy member). Three arms ride the ADMITTED native band (`rtree` and `python-fcl` are manifest worker-lane rows with folder-tier catalogs) composed at the DIRECT surface, not a one-call trimesh veneer: `Bounds`/`Nearest` over the `rtree`-backed `triangles_tree` R-tree through the numpy-vectorized batch `intersection_v`/`nearest_v` candidate generators (one vectorized call for the whole box set, never a per-box Python loop), the conservative `Clearance` over the direct `fcl.distance(o1, o2, DistanceRequest(enable_nearest_points=True, enable_signed_distance=True), DistanceResult())` narrow-phase read — SIGNED separation plus the witness contact pair `DistanceResult.nearest_points` carries, richer than the unsigned `CollisionManager.min_distance_single` float — and the exact `Clearance` over the `manifold3d.Manifold.min_gap` watertight gap that SUPERSEDES the FCL separation when its package is the richer one, both operands built through repair's public `to_manifold` kernel (repair is the chartered `manifold3d` owner; no re-spelled `Mesh`/`Mesh64` build here). `SpatialBackend` is the per-interpreter native-capability resolver — `CORE` (every pure-Python arm plus the conservative FCL clearance and the rtree bounds when those packages happen to resolve in-process), `MANIFOLD3D` (the exact-gap clearance superseding FCL) — computed once through `SpatialBackend.resolve` off real `find_spec` probes of `rtree`/`fcl`/`manifold3d`, never a second query owner and never an asserted-always runtime floor. The parallel vertex-KNN tiers `open3d.geometry.KDTreeFlann` and `small_gicp.KdTree.batch_knn_search` are NOT this owner's backend: a vertex nearest-neighbor is a coarser, distinct result from `closest_point`'s exact on-surface projection, so that acceleration belongs to the `scan/deviation`+`scan/registration` consumers that own the cloud-to-vertex correspondence, never minted here as a dead spatial row. This owner is a pure kernel conforming structurally to the runtime-checkable `ReceiptContributor` Protocol (never subclassing it): it indexes an in-memory `trimesh.Trimesh`, returns numpy arrays across the wire, and contributes one typed `SpatialReceipt` row per query — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and the geometry shed never opens or writes a mesh file.

## [01]-[INDEX]

- [01]-[SPATIAL]: the proximity, ray, contains, bounds, nearest, clearance, and sample queries under one tagged union over the `trimesh`/`numpy` spine with the admitted `rtree`/`python-fcl` native band composed direct, woven from stacked `.api` members per arm, dispatched through one capability-aware `async` `_route` that runs every pure-Python core kind on the synchronous `boundary` and offloads the native-backend kinds whose compiled package is absent in-process (`rtree` bounds, `python-fcl` clearance, the exact `manifold3d.min_gap` clearance) onto the `LanePolicy.offload` PEP 734 subinterpreter the companion carries, resolved per interpreter through `SpatialBackend` off real `find_spec` probes, returning one unified `SpatialResult` union and contributing one typed `SpatialReceipt` stream.

## [02]-[SPATIAL]

- Entry: `query` is the one polymorphic `async` entrypoint discriminating a single `SpatialQuery` or a batch `Sequence[SpatialQuery]` by total `match` (a lone `SpatialQuery` as one case, a `Sequence` as the other — never an `isinstance` ladder), with `@overload` arms keyed on the input shape so a caller narrows on what it passes rather than re-matching the widened return. A single query awaits one `_route`; a batch routes each query in turn into a `Block` of rails (the offload lane's own `CapacityLimiter` already bounds the concurrent subinterpreter hops, so the batch needs no second task-group pool) and folds them through `traversed(rails, by=Disposition.ACCUMULATE)` (the runtime `reliability/faults#FAULT` owner's one disposition-keyed fold; `ACCUMULATE` keeps a faulted query addressable in the aggregate while every successful `SpatialResult` already landed, never a junior `accumulate` boolean flag) into one `RuntimeRail[Block[SpatialResult]]`, never a second batch method nor a quadratic singleton-append. `_route` is the one capability-aware fence over the SINGLE module-level `_dispatch(mesh, q, backend) -> Outcome` body: it probes `q.tag in self._offload` (the data-driven set of native-backend kinds whose compiled package is absent in-process) and either runs `_dispatch` synchronously inside `boundary(f"mesh.spatial.{q.tag}", lambda: self._fold(q, _dispatch(self._mesh, q, self._backend)))` for a kind whose backend is resident or awaits `self._lane.offload(_dispatch, self._mesh, q, self._backend)` for a worker kind (mapping the offloaded `Outcome` through `self._fold`) — the SAME exhaustive `_dispatch` runs either way, so the offload-vs-sync choice never forks the geometry body. The pure-Python `Proximity`/`Ray`/`Contains`/`Sample` arms run on the synchronous `boundary`: `proximity`/`contains`/`sample` ride the trimesh-cached index plus the `numpy` core and the `Ray` arm is one batch `mesh.ray.intersects_location` call over the in-memory `RayMeshIntersector` (the pure-Python `ray_triangle` form, the `ray_pyembree` Embree mirror a transparent in-process upgrade when `embreex` resolves), not a per-ray Python loop, so none stalls the loop nor carries a hard native dependency. The native-compiled kinds are worker: the `rtree` `Bounds`/`Nearest` index build plus vectorized query, the direct `python-fcl` `fcl.distance` signed separation, and the exact `manifold3d.Manifold` build plus `min_gap` fold each ride one PEP 734 subinterpreter hop when their package is missing in-process, under the lane's shared `CapacityLimiter` and deadline, the lane importing neither the native module nor `_dispatch` (a module-level arg-only `Callable` the no-pickle hop receives verbatim) and converting a `BrokenWorkerInterpreter`/deadline `TimeoutError` through its own `async_boundary`, exactly the offload-aware parity the `mesh/repair.md#MESH` and `mesh/quality.md#QUALITY` siblings hold. `_dispatch` is one total `match`/`assert_never` over every kind: `Proximity` calls `proximity.closest_point(mesh, points)` for nearest points/distances/triangle ids and stacks `proximity.signed_distance(mesh, points)` into the fourth result column when `signed`; `Ray` calls the vectorized `mesh.ray.intersects_location(origins, _unit(directions))` once for the whole batch and `_nearest_hits` reduces the all-hits `(locations, ray_idx, tri_idx)` to the nearest hit per ray through one `lexsort` on `(ray_idx, dist)` and the first-of-group mask, scattering into the dense origin order — a ray absent from `ray_idx` or whose nearest hit exceeds `max_distance` fills `tri_idx == -1` and a `NaN` hit point — never a per-ray cast; `Contains` gates `Trimesh.is_watertight` BEFORE the test and short-circuits an off-solid query to an admitted caveat rather than running `Trimesh.contains` over a leaky surface and returning a meaningless mask; `Bounds` queries the `rtree`-backed `triangles_tree` R-tree through ONE vectorized `intersection_v(mins, maxs)` call whose flat `(ids, counts)` return splits per box (never a per-box Python loop), and `Nearest` reads `nearest_v(mins, maxs, num_results)` for the k-nearest candidate triangles per probe box; `Sample` draws through the evenness selector and stacks `signed_distance` when `attribute`; `Clearance` gates both surfaces watertight, then folds the exact `manifold3d.Manifold.min_gap(other, search_length)` when `backend is MANIFOLD3D` (building each operand once through repair's public `to_manifold`) else the direct `fcl.distance` narrow-phase read over two `BVHModel`-lifted `fcl.CollisionObject` operands (`beginModel`/`addSubModel`/`endModel`) with `DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)`, reading `DistanceResult.min_distance` AND the `nearest_points` witness pair the clearance result carries. An offloaded kind rebuilds whatever index its hop needs inside the subinterpreter (a live `triangles_tree` R-tree or `Manifold` cannot cross the no-pickle boundary, so the capsule caches no native handle across hops — the per-hop build is the offload cost the lane isolates, distinct from the `mesh/quality.md#QUALITY` in-process `ExactTopology` VALUE reuse), while the in-process pure-Python arms read off the lazily-cached `kdtree`/`ray` indices the `Trimesh` owns, built once and reused across the capsule's lifetime. The cross-cutting concern — folding each `Outcome` into one `SpatialReceipt` row with the backend tier and index-reuse verdict — rides the one `_fold(q, out)` method `_route` calls on both the synchronous and the offloaded `Outcome`, so a new kind writes only its `_dispatch` arm returning an `Outcome` (the watertight gate stays inside the `Contains`/`Clearance` arms, which carry the shape-specific short-circuit the fold cannot apply uniformly); the `_f64` float64 coercion is a shared helper each arm calls on its own array shape, since proximity points, ray origins/directions, contains points, and bounds boxes carry distinct `Nx3`/`Nx6` shapes the fold cannot coerce uniformly.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import TYPE_CHECKING, Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type QueryKind = Literal["proximity", "ray", "contains", "bounds", "nearest", "clearance", "sample"]


class SpatialBackend(
    StrEnum
):  # the resolved exact-clearance KERNEL; vertex-KNN acceleration (open3d/small_gicp) belongs to the scan/deviation+registration consumers
    CORE = "core"  # the conservative python-fcl CollisionManager separation (worker native, no package)
    MANIFOLD3D = "manifold3d"  # the exact Manifold.min_gap superseding the FCL separation when the richer package resolves

    @staticmethod
    def resolve() -> (
        "SpatialBackend"
    ):  # the exact gap supersedes FCL only where the manifold3d package loads (companion); CORE folds the conservative FCL separation otherwise
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
    ) -> "SpatialQuery":  # intersects_location casts infinite rays; a finite max_distance clamps the first hit to a miss past the range
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

# each native-COMPILED-backend kind mapped to its `find_spec` probe module — `bounds` over the libspatialindex
# `rtree` R-tree, `clearance` over the `python-fcl` engine (import name `fcl`); both are optional `trimesh` extras
# keys whose probe is absent in-process, plus `clearance` when only the companion carries the exact gap), so the
# `_route` fence reads one data-driven membership rather than a hardcoded per-case `if q.tag == ...` branch, and
# every pure-Python kind (proximity/ray/contains/sample) stays off the set and runs synchronously under `boundary`.
_NATIVE: Final[Map[QueryKind, str]] = Map.of_seq((("bounds", "rtree"), ("nearest", "rtree"), ("clearance", "fcl")))


# --- [MODELS] ---------------------------------------------------------------------------


class Outcome(
    Struct, frozen=True
):  # one arm's payload plus the receipt facts the `_fold` cross-cut folds; GC-tracked since `result` nests the SpatialResult union and its numpy arrays, never a per-arm receipt build
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


def _unit(d: np.ndarray) -> np.ndarray:  # zero-direction rows degrade to a zero ray (an absent-ray_idx miss downstream) rather than dividing by zero
    norm = np.linalg.norm(d, axis=1, keepdims=True)
    return np.divide(d, norm, out=np.zeros_like(d), where=norm > 0)


def _nearest_hits(
    n: int, locations: np.ndarray, ray_idx: np.ndarray, tri_idx: np.ndarray, origins: np.ndarray, max_distance: float
) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    # intersects_location returns ALL hits per ray keyed by ray_idx; reduce to the nearest-per-ray first hit by one
    # lexsort on (ray_idx, dist) and the first-of-group mask, scatter into the dense Nx ray order (a missed ray keeps
    # tri==-1/NaN), and clamp a hit past max_distance back to a miss so a finite range bounds the infinite cast.
    face = np.full(n, -1, dtype=np.int64)
    pos = np.full((n, 3), np.nan, dtype=np.float64)
    dist = np.full(n, np.nan, dtype=np.float64)
    if ray_idx.size == 0:  # every ray missed; the dense miss-filled arrays are the result
        return face, pos, dist
    hit_dist = np.linalg.norm(locations - origins[ray_idx], axis=1)
    order = np.lexsort((hit_dist, ray_idx))  # primary ray_idx, secondary distance ascending => first-of-group is the nearest hit
    ri = ray_idx[order]
    keep = np.concatenate(([True], ri[1:] != ri[:-1])) & (hit_dist[order] <= max_distance)
    face[ri[keep]], pos[ri[keep]], dist[ri[keep]] = tri_idx[order][keep], locations[order][keep], hit_dist[order][keep]
    return face, pos, dist


# the one exhaustive geometry dispatch over EVERY query kind — module-level and arg-only (`(mesh, query, backend)`)
# so the no-pickle PEP 734 subinterpreter receives it verbatim as the offloaded kernel, the lane importing neither it
# nor any native backend. The pure-Python kinds bind the trimesh/numpy `.api` members; the native-backend arms
# (`bounds` over rtree, `clearance` over fcl/manifold3d) build their index per call (a live R-tree or Manifold cannot
# cross the subinterpreter boundary, so no cached native handle here, unlike the in-process mesh/quality value reuse).
# The total `match`/`assert_never` forces a new SpatialQuery case to add its arm here, and `_route` decides
# synchronous-vs-offload by the `_offload` set, never a second dispatch body. `backend` selects the clearance kernel.
def _dispatch(mesh: trimesh.Trimesh, q: SpatialQuery, backend: SpatialBackend) -> Outcome:
    match q:
        case SpatialQuery(tag="proximity", proximity=(points, signed)):
            pts = _f64(points)
            near, distance, triangle_ids = trimesh.proximity.closest_point(mesh, pts)
            signed_field = trimesh.proximity.signed_distance(mesh, pts) if signed else np.empty(0)
            return Outcome(SpatialResult.Proximity(near, _f64(distance), np.asarray(triangle_ids), _f64(signed_field)), len(pts), len(pts), True)
        case SpatialQuery(
            tag="ray", ray=(origins, directions, max_distance)
        ):  # pure-Python RayMeshIntersector batch, Embree only an in-process accel; core-resident, no offload
            o = _f64(origins)
            locations, ray_idx, tri_idx = mesh.ray.intersects_location(o, _unit(_f64(directions)))  # ALL hits per ray, keyed by ray_idx
            face, pos, dist = _nearest_hits(len(o), _f64(locations), np.asarray(ray_idx), np.asarray(tri_idx), o, max_distance)
            return Outcome(SpatialResult.Ray(face, pos, dist), len(o), int((face != -1).sum()), True)
        case SpatialQuery(tag="contains", contains=points):
            if not mesh.is_watertight:  # the inside/outside test is meaningless off a watertight solid; gate before the query, flag the caveat
                return Outcome(SpatialResult.Contains(np.zeros(len(points), dtype=bool)), len(points), 0, False)
            mask = np.asarray(mesh.contains(_f64(points)), dtype=bool)
            return Outcome(SpatialResult.Contains(mask), len(mask), int(mask.sum()), True)
        case SpatialQuery(
            tag="bounds", bounds=boxes
        ):  # the rtree-backed triangles_tree (native libspatialindex); ONE vectorized batch call, never a per-box loop
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.intersection_v(rows[:, :3], rows[:, 3:])  # flat candidate ids + per-box counts
            candidates = tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1]))
            return Outcome(SpatialResult.Bounds(candidates), len(rows), int(ids.size), True)
        case SpatialQuery(tag="nearest", nearest=(boxes, num_results)):
            tree, rows = mesh.triangles_tree, _f64(boxes)
            ids, counts = tree.nearest_v(rows[:, :3], rows[:, 3:], num_results=num_results)  # k nearest candidates per probe box
            candidates = tuple(np.asarray(chunk, dtype=np.int64) for chunk in np.split(ids, np.cumsum(counts)[:-1]))
            return Outcome(SpatialResult.Nearest(candidates), len(rows), int(ids.size), True)
        case SpatialQuery(tag="sample", sample=(count, even, attribute)):
            sampler = trimesh.sample.sample_surface_even if even else trimesh.sample.sample_surface
            points, triangle_ids = sampler(mesh, count)
            signed = trimesh.proximity.signed_distance(mesh, points) if attribute else np.empty(0)
            return Outcome(SpatialResult.Sample(np.asarray(points), np.asarray(triangle_ids), _f64(signed)), len(points), len(points), True)
        case SpatialQuery(
            tag="clearance", clearance=(other, search_length)
        ):  # both backends native worker; exact manifold3d gap supersedes the conservative fcl separation
            if not (mesh.is_watertight and other.is_watertight):  # a solid-to-solid gap demands both surfaces close on either backend
                return Outcome(SpatialResult.Clearance(float("nan")), 1, 0, False)
            if backend is SpatialBackend.MANIFOLD3D:
                gap, witness = float(to_manifold(mesh).min_gap(to_manifold(other), search_length)), None
            else:
                gap, witness = _fcl_gap(mesh, other)
            return Outcome(SpatialResult.Clearance(gap, witness), 1, int(gap <= search_length), True)
        case _ as unreachable:
            assert_never(unreachable)


def _bvh(mesh: trimesh.Trimesh) -> "fcl.CollisionObject":
    # the catalog-verified BVH lift: beginModel/addSubModel/endModel over the raw vertex/face arrays,
    # bound into a CollisionObject — never a veneer helper the catalog does not carry.
    import fcl  # noqa: PLC0415

    model = fcl.BVHModel()
    model.beginModel(len(mesh.vertices), len(mesh.faces))
    model.addSubModel(mesh.vertices, mesh.faces)
    model.endModel()
    return fcl.CollisionObject(model)


def _fcl_gap(
    mesh: trimesh.Trimesh, other: trimesh.Trimesh
) -> tuple[float, tuple[tuple[float, float, float], tuple[float, float, float]]]:
    # the CORE clearance at the DIRECT fcl narrow-phase surface: signed separation plus the witness
    # contact pair — richer than the unsigned CollisionManager.min_distance_single float veneer.
    import fcl  # noqa: PLC0415

    request = fcl.DistanceRequest(enable_nearest_points=True, enable_signed_distance=True)
    result = fcl.DistanceResult()
    gap = float(fcl.distance(_bvh(mesh), _bvh(other), request, result))
    a, b = result.nearest_points
    return gap, (tuple(float(v) for v in a), tuple(float(v) for v in b))


# --- [SERVICES] -------------------------------------------------------------------------


class MeshSpatial:  # conforms structurally to the runtime-checkable ReceiptContributor Protocol; subclassing the port is the deleted form
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, backend: SpatialBackend | None = None) -> None:
        self._mesh = mesh
        self._lane = (
            lane  # the per-subinterpreter offload seam a native-backend arm rides when its package is absent; the lane never imports the kernel
        )
        self._backend = backend or SpatialBackend.resolve()
        # a kind offloads iff its native backend module is missing IN-PROCESS: `bounds` probes `rtree`, `clearance`
        # probes the module its resolved kernel needs (`manifold3d` under MANIFOLD3D, else the `fcl` separation). The
        # pure-Python kinds carry no `_NATIVE` row and never offload. Resolving the set once at construction means the
        clearance_mod = "manifold3d" if self._backend is SpatialBackend.MANIFOLD3D else _NATIVE["clearance"]
        probe = _NATIVE.add("clearance", clearance_mod)  # persistent insert overriding the clearance probe with the resolved kernel's module
        self._offload: frozenset[QueryKind] = frozenset(k for k, mod in probe.items() if find_spec(mod) is None)
        self._last: SpatialReceipt | None = None

    @overload
    async def query(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]": ...
    @overload
    async def query(self, q: Sequence[SpatialQuery]) -> "RuntimeRail[Block[SpatialResult]]": ...
    async def query(self, q: SpatialQuery | Sequence[SpatialQuery]) -> "RuntimeRail[SpatialResult] | RuntimeRail[Block[SpatialResult]]":
        # arity is a property of the argument: a lone query awaits one capability-aware route, a sequence routes each
        # in turn (the offload lane's own CapacityLimiter bounds the concurrent subinterpreter hops, so the batch
        # needs no second pool) and folds the rails under one Disposition — never an isinstance ladder nor a second method.
        match q:
            case SpatialQuery() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(
                    rails, by=Disposition.ACCUMULATE
                )  # one faulted query stays addressable; the runtime owns the strategy row, never a boolean accumulate flag

    async def _route(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]":
        # the one capability-aware fence over the single `_dispatch` body: a kind whose backend is resident runs
        # `_dispatch` synchronously under `boundary`, a native-backend kind whose package is absent in-process offloads
        # the SAME `_dispatch` onto the lane subinterpreter under the shared CapacityLimiter and deadline, the lane
        # converting a BrokenWorkerInterpreter/TimeoutError through its own async_boundary onto the rail. Both paths
        # produce the same Outcome the one `_fold` cross-cut turns into the receipt row plus the SpatialResult, so the
        # offload-vs-sync split never forks the geometry dispatch.
        offloaded = q.tag in self._offload
        if offloaded:
            return (await self._lane.offload(_dispatch, self._mesh, q, self._backend)).map(lambda out: self._fold(q, out, offloaded))
        return boundary(f"mesh.spatial.{q.tag}", lambda: self._fold(q, _dispatch(self._mesh, q, self._backend), offloaded))

    def _fold(
        self, q: SpatialQuery, out: Outcome, offloaded: bool
    ) -> SpatialResult:  # the cross-cutting receipt aspect: one uniform SpatialReceipt off each Outcome, geometry-only `_dispatch` body elsewhere
        self._last = SpatialReceipt(q.tag, self._backend, offloaded, out.query_count, out.hit_count, out.valid, indexed=not offloaded)
        return out.result

    def contribute(
        self,
    ) -> Iterable[Receipt]:  # the ReceiptContributor port YIELDS the stream the @receipted aspect's _stream normalizes; never a bare Receipt return
        r = self._last or SpatialReceipt("proximity", self._backend, False, 0, 0, True, False)
        phase: Phase = "emitted" if r.valid else "admitted"
        # native int/bool/str ride the EventDict dict[str, object]; the receipts Encoder(enc_hook=repr) serializes them without a str() coerce
        facts: dict[str, object] = {
            "backend": r.backend.value,
            "offloaded": r.offloaded,
            "queries": r.query_count,
            "hits": r.hit_count,
            "indexed": r.indexed,
        }
        yield Receipt.of(
            "mesh.spatial", (phase, r.kind, facts)
        )  # the owner's shape-polymorphic (Phase, subject, facts) factory; subject is the query kind
```
