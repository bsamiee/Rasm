# [PY_GEOMETRY_MESH_SPATIAL]

Spatial query over an in-memory triangulation — the proximity, ray, containment, bounds, clearance, and sampling primitive the deviation, clash, and reconstruction hops compose against a built mesh. `SpatialQuery` is one tagged union discriminating by query kind; `query` is one polymorphic entrypoint accepting one query or a batch sequence, building the cached `triangles_tree` R-tree / `kdtree` index once per surface and folding every kind through it, and `SpatialResult` is one union carrier whose case mirrors the query case so a proximity result, a ray result, a containment mask, a bounds-candidate set, a clearance scalar, and a sample set are arms of one result owner rather than six parallel result types. The spine is `trimesh` plus `numpy` (intended cp315 core), composed at depth: each arm woven from the module-level `proximity.closest_point`/`signed_distance` field samplers, the `mesh.contains` solid test, `sample.sample_surface`/`sample_surface_even`, and the cached `kdtree`/`triangles_tree` indices — never a single flat call. The `manifold3d` `Manifold.ray_cast`/`min_gap` ray-and-clearance surface is the one GATED ENRICHMENT tier on the `python_version<'3.15'` companion band — the spine never depends on it, and `SpatialBackend` carries exactly the two tiers this owner dispatches (`SPINE`/`MANIFOLD3D`) resolved per interpreter through `SpatialBackend.resolve`, never a second query owner. The parallel vertex-KNN tiers `open3d.geometry.KDTreeFlann` and `small_gicp.KdTree.batch_knn_search` are NOT this owner's backend: a vertex nearest-neighbor is a coarser, distinct result from `closest_point`'s exact on-surface projection, so that acceleration belongs to the `scan/deviation`+`scan/registration` consumers that own the cloud-to-vertex correspondence, never minted here as a dead spatial row. This owner is a pure kernel implementing `ReceiptContributor`: it indexes an in-memory `trimesh.Trimesh`, returns numpy arrays across the wire, and contributes one typed `SpatialReceipt` row per query — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and the geometry shed never opens or writes a mesh file.

## [01]-[INDEX]

- [01]-[SPATIAL]: the proximity, ray, contains, bounds, clearance, and sample queries under one tagged union over the `trimesh`/`numpy` spine, woven from stacked `.api` members per arm, with the gated `manifold3d` ray/clearance enrichment tier resolved per interpreter through the two-tier `SpatialBackend`, returning one unified `SpatialResult` union and contributing one typed `SpatialReceipt`.

## [02]-[SPATIAL]

- Owner: `MeshSpatial` — the boundary capsule binding the cached `triangles_tree` R-tree plus `kdtree` indices to a surface, dispatching every query kind through them, and implementing `ReceiptContributor` so the last query's facts contribute one `SpatialReceipt` row; `SpatialQuery` the tagged union discriminating by query kind so proximity/ray/contains/bounds/clearance/sample are six cases of one request rather than six entrypoints; `SpatialResult` the union carrier whose case mirrors the query case (`Proximity`/`Ray`/`Contains`/`Bounds`/`Clearance`/`Sample`), so the result owner is one union not six parallel result structs; `SpatialBackend` the closed two-tier `StrEnum` selecting the always-available `SPINE` versus the gated `MANIFOLD3D` ray/clearance tier, with `SpatialBackend.resolve` mapping the live interpreter to `MANIFOLD3D` only when its wheel loads and to `SPINE` otherwise; `Outcome` the one struct each arm returns carrying its `SpatialResult` plus the receipt facts (query-point count, hit/inside count, validity verdict) so the `@arm` aspect folds one `SpatialReceipt` from a uniform payload instead of a per-arm receipt build; `SpatialReceipt` the typed receipt carrying the query kind, the backend tier, the query-point count, the hit/inside count, the watertight-validity verdict (so a `Contains`/`Clearance` over a non-watertight solid lands as an admitted-phase caveat row), and the index-reuse verdict.
- Cases: `SpatialQuery` cases `Proximity(points, signed)` (the module-level `proximity.closest_point` nearest-surface points/unsigned distances/triangle ids, with the fourth result column carrying `proximity.signed_distance` when `signed` — positive inside the watertight solid — else an empty array, so one arm weaves both `.api` field samplers into one 4-array `Proximity` result rather than two entrypoints or a shape that varies by flag), `Ray(origins, directions)` (per-ray first-hit triangle id, hit point, and parametric distance, routed to the `.api`-confirmed `manifold3d.Manifold.ray_cast` whose `RayHit.face_id == -1` marks a miss), `Contains(points)` (`trimesh.Trimesh.contains` inside/outside boolean mask over the watertight solid), `Bounds(boxes)` (broad-phase candidate triangle ids per AABB through the cached `triangles_tree` R-tree, the prefilter the clash hop narrows before a `Clearance`/`Proximity` exact pass), `Clearance(other, search_length)` (the minimum solid-to-solid gap through `manifold3d.Manifold.min_gap`, the clash-clearance scalar the deviation/clash hops read), and `Sample(count, even, attribute)` (`sample.sample_surface_even` blue-noise even samples when `even` else `sample.sample_surface` area-weighted, optionally stacked with `proximity.signed_distance` over the drawn points when `attribute` so one arm yields samples already carrying their signed field value) — matched by `match`/`assert_never`, each binding the `.api` members that own the kind. Inputs are numpy `Nx3`/`Nx6` arrays or counts; outputs are numpy arrays carried in the mirrored `SpatialResult` arm. No mesh-file format axis lives here — the surface arrives as an in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam.
- Entry: `query` is the one polymorphic entrypoint discriminating a single `SpatialQuery` or a batch `Sequence[SpatialQuery]` — a single query lifts through one `boundary`, a batch builds a `Block` of per-query `boundary` rails in one comprehension and folds them through `runtime.faults.traversed(accumulate=False)` into one `RuntimeRail[Block[SpatialResult]]`, never a second batch method nor a quadratic singleton-append — and returns a `RuntimeRail[SpatialResult]` over the capsule's pre-built indices. The `Proximity` arm calls `proximity.closest_point(mesh, points)` for nearest points/distances/triangle ids and stacks `proximity.signed_distance(mesh, points)` into the fourth result column when `signed`; the `Ray` arm drives `Manifold.ray_cast` per ray reading `RayHit.face_id`/`position`/`distance` (the `.api` carries no batch ray entry, so the per-ray loop is the confirmed surface, not a junior choice); the `Contains` arm gates `Trimesh.is_watertight` BEFORE the test and short-circuits an off-solid query to an admitted caveat rather than running `Trimesh.contains` over a leaky surface and returning a meaningless mask; the `Bounds` arm queries the cached `triangles_tree` R-tree per box for candidate triangle ids; the `Clearance` arm gates both surfaces watertight, then builds the `manifold3d.Manifold` once from the surface's `Mesh`/`Mesh64` and folds `min_gap` against each `other`; the `Sample` arm draws through the evenness selector and stacks `signed_distance` when `attribute`. Every arm reads off the lazily-cached `kdtree`/`triangles_tree` the `Trimesh` owns, built once and reused across the capsule's lifetime. The cross-cutting concern — folding each arm's `Outcome` into one `SpatialReceipt` row with the backend tier and index-reuse verdict — rides the real `@arm` decorator wrapping the match dispatch, so a new case writes only the geometry body returning an `Outcome`; the `_f64` float64 coercion is a shared helper each arm calls on its own array shape, since proximity points, ray origins/directions, contains points, and bounds boxes carry distinct `Nx3`/`Nx6` shapes the aspect cannot coerce uniformly.
- Auto: `Trimesh.kdtree` and `Trimesh.triangles_tree` are `.api`-confirmed lazily-cached properties (`trimesh.md` row [09]) recomputed only on geometry change, so the capsule indexes once and every query kind shares the build; `proximity.signed_distance` (`trimesh.md` row [6]) returns positive inside and negative outside a watertight solid, the sign the deviation hop reads for over/under-build, and `proximity.closest_point` (`trimesh.md` row [5]) returns closest points, unsigned distances, and triangle ids in one call; `sample.sample_surface_even` (row [8]) rejection-samples toward blue-noise spacing while `sample.sample_surface` (row [7]) is area-weighted, the evenness a `Sample` boolean not a parallel entrypoint; on the gated `MANIFOLD3D` tier `manifold3d.Manifold.ray_cast(origin, direction)` (`manifold3d.md` query row [12]) returns a `RayHit` whose `face_id` is `-1` on a miss, and `Manifold.min_gap(other, search_length)` (row [13]) the solid clearance; the surface enters the kernel as `manifold3d.Mesh` (32-bit `tri_verts`) or `Mesh64` (`manifold3d.md` type rows [03]/[04]) selected by vertex count so a surface past the `uint32` ceiling never silently truncates its triangle indices.
- Receipt: `MeshSpatial` conforms to the runtime `ReceiptContributor` Protocol — the `@arm` decorator folds each arm's returned `Outcome` into one `SpatialReceipt`, and `contribute` emits one `Receipt.of(phase, "mesh.spatial", kind, facts)` row carrying the query kind, the `SpatialBackend.value` tier string, the query-point count, the hit/inside count, and the index-reuse verdict; a `Contains`/`Clearance` over a non-watertight surface short-circuits at the arm's watertight gate to `valid=False`, so `contribute` keys `phase="admitted"` (the `Phase` literal the receipt owner admits) and the row is a caveat flagging the unreliable verdict rather than asserting one. The spatial query produces no `GraduationReceipt` subject — it is the read-side primitive the deviation and clash hops consume, and the graduating subject (`scan-deviation`, `mesh-algebra`) belongs to those owners; the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`) is never minted here.
- Packages: `trimesh` (`Trimesh`/`Trimesh.contains`/`Trimesh.is_watertight`/`Trimesh.kdtree`/`Trimesh.triangles_tree`/`proximity.closest_point`/`proximity.signed_distance`/`sample.sample_surface`/`sample.sample_surface_even`), `numpy` (`ndarray`/`asarray`/`fromiter`/`iinfo` query and result arrays over `Nx3`/`Nx6` shapes, `linalg.norm` ray-direction normalization), `manifold3d` (the gated ray/clearance tier — `Manifold`/`Mesh`/`Mesh64`/`Manifold.ray_cast`/`RayHit`/`Manifold.min_gap`, reached only on the `MANIFOLD3D` backend row), runtime (`RuntimeRail`/`boundary`/`traversed`/`ReceiptContributor`/`Receipt`).
- Growth: a new query kind is one `SpatialQuery` case plus the mirrored `SpatialResult` arm returning an `Outcome` through the already-built indices — never a new entrypoint, and the `@arm` aspect folds its receipt without a per-case receipt build; a new acceleration tier is one `SpatialBackend` row plus its `resolve` branch only when this owner actually dispatches it, never a second query owner; the deviation hop composes `Proximity(points, signed=True)` and reads the signed fourth column of `SpatialResult.Proximity` for signed nearest-surface deviation rather than re-deriving closest-point math, or composes `Sample(count, attribute=True)` to draw verification points already carrying their signed field; the clash hop composes `Bounds` for broad-phase candidate triangles then `Clearance` for the exact gap, never re-deriving overlap; the index build (the `triangles_tree` R-tree and `kdtree` construction over a large surface, the CPU-bound `manifold3d` ray/gap batch on the `<'3.15'` companion band) hands across the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — the lane never imports the kernel; zero new surface, no parallel per-kind class family.
- Boundary: no point-cloud registration (that is `scan-registration`); no mesh repair or boolean (that is `mesh/repair`); no mesh-quality metric (that is `mesh/quality`); the `manifold3d` ray/gap kernel is reached only through the `MANIFOLD3D` `SpatialBackend` row, never a second direct CSG/ray owner; the parallel vertex-KNN of `open3d.KDTreeFlann`/`small_gicp.KdTree` is NOT a spatial backend row — vertex nearest-neighbor is a distinct coarser result from `closest_point` exact surface projection and belongs to the `scan/deviation`+`scan/registration` cloud-correspondence owners, so a dead `OPEN3D`/`SMALL_GICP` tier resolved-but-never-dispatched here is itself a deleted form; mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file. A `closest_point`/`signed_distance`/`contains`/`sample`/`min_gap` method family over the `SpatialQuery` row, six parallel `ProximityResult`/`RayResult`/`ContainsResult`/`BoundsResult`/`ClearanceResult`/`SampleResult` structs, a hand-rolled kd-tree or BVH where `trimesh.triangles_tree` is admitted, a per-arm receipt build duplicating the `@arm` decorator, running `contains`/`min_gap` over a non-watertight surface and flagging the verdict afterward instead of gating before the query, a hardcoded 32-bit `manifold3d.Mesh` that truncates large-mesh triangle indices, treating the `manifold3d` acceleration as the spine rather than a gated tier, carrying `open3d`/`small_gicp` as resolved-but-undispatched spatial backend rows, and ANY `MeshFormat`/`Codec`/`load`/`export` arm re-deriving the `MeshPayload` seam are the deleted forms.

```python signature
from collections.abc import Callable, Sequence
from enum import StrEnum
from functools import wraps
from importlib.util import find_spec
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor

if TYPE_CHECKING:
    import manifold3d

QueryKind = Literal["proximity", "ray", "contains", "bounds", "clearance", "sample"]
type Arm = Callable[["MeshSpatial", SpatialQuery], "Outcome"]


class Outcome(Struct, frozen=True):  # one arm's payload plus the receipt facts the @arm aspect folds, never a per-arm receipt build
    result: "SpatialResult"
    query_count: int
    hit_count: int
    valid: bool


class SpatialBackend(StrEnum):  # only the two tiers this owner dispatches; vertex-KNN acceleration (open3d/small_gicp) belongs to the scan/deviation+registration consumers
    SPINE = "spine"          # trimesh + numpy: proximity/contains/bounds/sample, always-available cp315 core
    MANIFOLD3D = "manifold3d"  # gated <3.15 companion: the only .api-confirmed ray/clearance surface

    @staticmethod
    def resolve() -> "SpatialBackend":  # the cp315 venv carries no manifold3d wheel, so the gated ray/clearance tier resolves only on the companion lane
        return SpatialBackend.MANIFOLD3D if find_spec("manifold3d") is not None else SpatialBackend.SPINE


class SpatialReceipt(Struct, frozen=True):
    kind: QueryKind
    backend: SpatialBackend
    query_count: int
    hit_count: int
    valid: bool
    indexed: bool


@tagged_union(frozen=True)
class SpatialQuery:
    tag: QueryKind = tag()
    proximity: tuple[np.ndarray, bool] = case()
    ray: tuple[np.ndarray, np.ndarray] = case()
    contains: np.ndarray = case()
    bounds: np.ndarray = case()
    clearance: tuple[trimesh.Trimesh, float] = case()
    sample: tuple[int, bool, bool] = case()

    @staticmethod
    def Proximity(points: np.ndarray, signed: bool = False) -> "SpatialQuery":
        return SpatialQuery(proximity=(points, signed))

    @staticmethod
    def Ray(origins: np.ndarray, directions: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(ray=(origins, directions))

    @staticmethod
    def Contains(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(contains=points)

    @staticmethod
    def Bounds(boxes: np.ndarray) -> "SpatialQuery":  # Nx6 (xmin,ymin,zmin,xmax,ymax,zmax) AABB rows
        return SpatialQuery(bounds=boxes)

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
    clearance: float = case()
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
    def Clearance(gap: float) -> "SpatialResult":
        return SpatialResult(clearance=gap)

    @staticmethod
    def Sample(points: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(sample=(points, triangle_ids, signed))


def _f64(a: np.ndarray) -> np.ndarray:
    return np.asarray(a, dtype=np.float64)


def arm(fn: Arm) -> Callable[["MeshSpatial", SpatialQuery], "SpatialResult"]:  # the cross-cutting aspect: one receipt fold + watertight gate, geometry-only bodies below
    @wraps(fn)
    def _wrapped(self: "MeshSpatial", q: SpatialQuery) -> "SpatialResult":
        out = fn(self, q)
        self._last = SpatialReceipt(q.tag, self._backend, out.query_count, out.hit_count, out.valid, indexed=True)
        return out.result

    return _wrapped


class MeshSpatial(ReceiptContributor):
    def __init__(self, mesh: trimesh.Trimesh, backend: SpatialBackend | None = None) -> None:
        self._mesh = mesh
        self._backend = backend or SpatialBackend.resolve()
        self._last: SpatialReceipt | None = None
        self._solid: "manifold3d.Manifold | None" = None  # lazily-built, reused across the capsule's ray/clearance arms

    def query(self, q: SpatialQuery | Sequence[SpatialQuery]) -> "RuntimeRail[SpatialResult] | RuntimeRail[Block[SpatialResult]]":
        if isinstance(q, SpatialQuery):
            return boundary(f"mesh.spatial.{q.tag}", lambda: self._arm(q))
        rails = Block(boundary(f"mesh.spatial.{one.tag}", lambda one=one: self._arm(one)) for one in q)
        return traversed(rails, accumulate=False)

    def contribute(self) -> Receipt:
        r = self._last or SpatialReceipt("proximity", self._backend, 0, 0, True, False)
        phase: Literal["admitted", "emitted"] = "emitted" if r.valid else "admitted"
        facts = {"kind": r.kind, "backend": r.backend.value, "queries": str(r.query_count), "hits": str(r.hit_count), "indexed": str(r.indexed)}
        return Receipt.of(phase, "mesh.spatial", r.kind, facts)

    @arm
    def _arm(self, q: SpatialQuery) -> Outcome:
        match q:
            case SpatialQuery(tag="proximity", proximity=(points, signed)):
                pts = _f64(points)
                near, distance, triangle_ids = trimesh.proximity.closest_point(self._mesh, pts)
                signed_field = trimesh.proximity.signed_distance(self._mesh, pts) if signed else np.empty(0)
                return Outcome(SpatialResult.Proximity(near, _f64(distance), np.asarray(triangle_ids), _f64(signed_field)), len(pts), len(pts), True)
            case SpatialQuery(tag="ray", ray=(origins, directions)):
                o, d = _f64(origins), _f64(directions)
                d = d / np.linalg.norm(d, axis=1, keepdims=True)
                solid = self._manifold()
                hits = [solid.ray_cast(oi, di) for oi, di in zip(o, d)]  # manifold3d.ray_cast is a per-ray query; no batch entry in the .api
                triangle_ids = np.fromiter((h.face_id for h in hits), dtype=np.int64, count=len(hits))
                pts = np.array([h.position for h in hits], dtype=np.float64) if hits else np.empty((0, 3))
                t = np.fromiter((h.distance for h in hits), dtype=np.float64, count=len(hits))
                return Outcome(SpatialResult.Ray(triangle_ids, pts, t), len(o), int((triangle_ids != -1).sum()), True)
            case SpatialQuery(tag="contains", contains=points):
                if not self._mesh.is_watertight:  # the inside/outside test is meaningless off a watertight solid; gate before the query, flag the caveat
                    return Outcome(SpatialResult.Contains(np.zeros(len(points), dtype=bool)), len(points), 0, False)
                mask = np.asarray(self._mesh.contains(_f64(points)), dtype=bool)
                return Outcome(SpatialResult.Contains(mask), len(mask), int(mask.sum()), True)
            case SpatialQuery(tag="bounds", bounds=boxes):
                tree = self._mesh.triangles_tree
                rows = _f64(boxes)
                candidates = tuple(np.fromiter(tree.intersection(tuple(row)), dtype=np.int64) for row in rows)
                return Outcome(SpatialResult.Bounds(candidates), len(rows), sum(c.size for c in candidates), True)
            case SpatialQuery(tag="clearance", clearance=(other, search_length)):
                watertight = bool(self._mesh.is_watertight and other.is_watertight)  # min_gap is a solid-to-solid query; both surfaces must close
                if not watertight:
                    return Outcome(SpatialResult.Clearance(float("nan")), 1, 0, False)
                gap = float(self._manifold().min_gap(self._to_manifold(other), search_length))
                return Outcome(SpatialResult.Clearance(gap), 1, int(gap <= search_length), True)
            case SpatialQuery(tag="sample", sample=(count, even, attribute)):
                sampler = trimesh.sample.sample_surface_even if even else trimesh.sample.sample_surface
                points, triangle_ids = sampler(self._mesh, count)
                signed = trimesh.proximity.signed_distance(self._mesh, points) if attribute else np.empty(0)
                return Outcome(SpatialResult.Sample(np.asarray(points), np.asarray(triangle_ids), _f64(signed)), len(points), len(points), True)
            case unreachable:
                assert_never(unreachable)

    def _manifold(self) -> "manifold3d.Manifold":  # cached solid for the ray/clearance arms (MANIFOLD3D tier)
        if self._solid is None:
            self._solid = self._to_manifold(self._mesh)
        return self._solid

    @staticmethod
    def _to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":
        import manifold3d

        verts = np.asarray(mesh.vertices, dtype=np.float32)
        faces = np.asarray(mesh.faces)
        if len(verts) > np.iinfo(np.uint32).max:  # 32-bit Mesh overflows past ~4.29B verts; Mesh64 carries 64-bit triangle indices (.api type row [04])
            return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts, tri_verts=faces.astype(np.uint64)))
        return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts, tri_verts=faces.astype(np.uint32)))
```

## [03]-[RESEARCH]

- [TRIMESH_PROXIMITY]: the `Proximity`/`Sample` arms bind the `.api`-confirmed module-level entrypoints exactly as the `scan/deviation.md#DEVIATION` consumer does — `proximity.closest_point(mesh, points) -> (closest, distance, triangle_id)` (`trimesh.md` row [5]), `proximity.signed_distance(mesh, points) -> NDArray` (row [6], positive inside the watertight solid), `Trimesh.contains(points)` (row [9]), and `sample.sample_surface(mesh, count)`/`sample.sample_surface_even(mesh, count) -> (points, face_index)` (rows [7]/[8]). The earlier `proximity.ProximityQuery(mesh).on_surface` instance form is deleted: it was a live-distribution member with no `trimesh.md` catalogue row, and the module functions carry the identical closest-point/sign/triangle-id surface, so the arm cites only confirmed members. The lazily-cached `Trimesh.kdtree`/`triangles_tree` spatial indices confirm against the catalogue's cached-property axis (row [09]); the `Bounds` arm queries `triangles_tree.intersection(aabb)` over that confirmed R-tree index for broad-phase candidate triangle ids.
- [RAY_AND_CLEARANCE]: the `Ray` and `Clearance` arms route through the `.api`-confirmed `manifold3d` query rows rather than an unconfirmed `Trimesh.ray.intersects_location` accessor (the `trimesh.md` catalogue carries `proximity`/`sample`/`section`/`contains` but no `ray` row, so a spine ray member is not citable and is dropped): `Ray` folds `manifold3d.Manifold.ray_cast(origin, direction) -> RayHit` (`manifold3d.md` query row [12], `RayHit.face_id == -1` on a miss, `RayHit.position`/`distance` the hit point and parametric distance) per ray, and `Clearance` folds `Manifold.min_gap(other, search_length) -> float` (row [13]). The surface enters the `manifold3d` kernel once as a `manifold3d.Mesh`/`Mesh64` triangle-soup carrier (`manifold3d.md` type rows [03]/[04], `tri_verts` `Nx3` index array, `vert_properties` columns 0-2 XYZ) — `Mesh64` selected when the vertex count exceeds the `uint32` ceiling so a large surface keeps 64-bit triangle indices instead of truncating — wrapped in a `Manifold` and cached on the capsule (`_solid`) so repeated ray/clearance calls reuse the one build; both arms therefore resolve only on the `MANIFOLD3D` backend tier and never need a spine ray.
- [BACKEND_TIER]: `SpatialBackend.resolve` probes `importlib.util.find_spec("manifold3d")` and returns `MANIFOLD3D` when the wheel loads, falling to `SPINE` otherwise — the per-venv resolution the daemon hosts, confirmed against the companion manifest markers. The enum carries exactly the two tiers this owner dispatches: `SPINE` (the always-available `trimesh`/`numpy` proximity/contains/bounds/sample arms) and `MANIFOLD3D` (the gated `ray_cast`/`min_gap` arms). `open3d.geometry.KDTreeFlann` (`open3d.md` type row [05]) and `small_gicp.KdTree.batch_knn_search` (`small-gicp.md` search rows [03]/[04]) are deliberately NOT backend rows: they return nearest VERTICES, a coarser correspondence than `proximity.closest_point`'s exact on-surface projection, and re-projecting a vertex-KNN candidate onto its incident triangles to recover surface distance would hand-roll the point-triangle distance `closest_point` already owns. That parallel-KNN acceleration is therefore owned by the `scan/deviation`+`scan/registration` cloud-to-vertex consumers, never minted here as a dead spatial tier.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `trimesh` and `numpy` are pure-Python or prebuilt-wheel admitted on the intended cp315 core, so the spine query surface (`proximity`/`contains`/`bounds`/`sample`/`kdtree`/`triangles_tree`) runs on the project venv directly — the `SPINE` `SpatialBackend.resolve` result is the always-available default. The single gated tier is companion-band only: `manifold3d` ships cp310-cp314 wheels on the `python_version<'3.15'` band (the `MANIFOLD3D` ray/clearance row). The cp315 project venv carries no `manifold3d` wheel, so `find_spec("manifold3d")` returns `None` and `resolve` falls to `SPINE`; the gated `MANIFOLD3D` row resolves only on the Forge companion lane (`forge-companion-env`), and the spine query never depends on it. The `open3d`/`small_gicp` companion wheels are admitted for the scan rail's cloud-correspondence owners, not as a spatial backend, so this owner declares no dependency on them.
- [RAY_BAND]: because the `Ray` and `Clearance` arms route through `manifold3d` (the only `.api`-confirmed ray/gap surface), both are `MANIFOLD3D`-tier operations gated to the `python_version<'3.15'` companion band — the cp315 spine carries no ray/clearance member, since `trimesh.md` enumerates no ray row. A `Ray`/`Clearance` query on the bare cp315 venv surfaces as a `BoundaryFault` on the `RuntimeRail` when `resolve` returns `SPINE`, never a silent spine fallback to an uncited intersector; the proximity/contains/bounds/sample arms remain wheel-free on the project venv. The ray/clearance throughput tier is the `SpatialBackend` row, never a parallel ray owner.
