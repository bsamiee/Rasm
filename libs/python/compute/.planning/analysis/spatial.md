# [PY_COMPUTE_SPATIAL]

One array-native computational-geometry owner rules: `SpatialQuery` discriminates Qhull tessellation, KD-tree proximity, the pairwise/condensed distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold over a point set, and `resolve` folds every case to a `SpatialEvidence` outcome the `SpatialReceipt` carries whole. This owner emits point-set evidence as compute-native receipts and never re-owns the geometry-branch `trimesh` mesh surface; the graduation direction is closed one-way — geometry's reconstruction plane mints `reconstructed-mesh`, the alpha-shape `Boundary` stays a compute-native receipt product, and an outward crossing requires a named consumer and a compute-owned axis case, never the geometry case.

Each point set admits through `numerics/array#PAYLOAD` for the finite gate and the operand `ContentKey`; the receipt keys the RESULT through the query-owned `identity_parts` fold handed to `IdentitySource(parts=...)`, so the count-and-length framing runs at the identity owner and two different queries over one point set never share a key; the resolved receipt is the `ReceiptContributor` the weave harvest and the study spine consume. `scipy.spatial` is not Array-API-aware, so the point set is the numpy `np.ndarray` the Qhull/KD-tree/BLAS backends bind under the `RELEASING` trait, isolation and band deriving at the runtime `Kernel` crossing. The direct native kernel enters through `LanePolicy.whole`, and the resulting `LaneGrant.width` is the KD-tree scan width threaded through the kernel — never an allocator-total read or the unbounded `workers=-1` team that multiplies inner and outer parallelism.

## [01]-[INDEX]

- [02]-[SPATIAL]: the `SpatialQuery` cases over one point set, evidence discriminated over `SpatialEvidence`, the two KD-tree routes degrading through the data-driven `NEIGHBOUR_FLOOR`.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one owner discriminated by the geometric question, never a `Neighbours`/`Hull`/`Triangulate` method family. `Align` is a paired correspondence fit — `source` and `target` carry the same row count, `procrustes` raising `ValueError` on a mismatch, the fault converting on the `boundary` fence. `AlphaShape` folds its boundary locally because no `scipy.spatial` alpha-shape primitive exists; `_circumradius` stays private to that kernel, never a module-level sibling of the dispatch.
- Output: `SpatialEvidence` parameterizes the result per case, and the `Complex` `cardinality` is the primitive count its `kind` string discriminates — hull facets, Delaunay simplices, Voronoi ridges, halfspace vertices, distance pairs — so a distance summary never wears a `simplices` label and four outcome vocabularies never smuggle through two overloaded columns. Adding a query writes only its geometry body returning a `SpatialEvidence`; `assert_never` closes the dispatch.
- Faults: one `SPATIAL_RESOLVE` fence row spans every query — the tag is a span fact, never nine subject spellings — and its `catch` admits `QhullError` through its `RuntimeError` base, since naming the attribute would reify the lazy proxy and defeat the floor. `_proximity` scopes its `try` to the import dereference ALONE, so a raise out of a live KD-tree query is the defect the fence converts rather than a silent demotion to floor evidence.
- Packages: `scipy.spatial`, `numpy`, `expression`, and `msgspec` per the fence imports; `scipy.spatial` and `optimize.linprog` bind once each as module-scope `lazy` names that defer both trees to the first kernel body, so `resolve` stays a pure tag-dispatch and `linprog` costs nothing until the halfspace Chebyshev-centre interior point asks for it.
- Growth: a new spatial query is one `SpatialQuery` case with one `resolve` arm and one `identity_parts` arm; a new evidence shape is one `SpatialEvidence` case with its `facts()` arm — the receipt carries the evidence whole and needs no edit; a new distance metric is one `Metric` row; a new tessellation backend is one `Tessellation` row; a new degrading route is one `NEIGHBOUR_FLOOR` row.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import combinations
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import scipy.spatial as sp
lazy from scipy.optimize import linprog

if TYPE_CHECKING:
    from scipy.spatial import cKDTree

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["neighbours", "radius", "pairs", "distances", "hull", "triangulate", "tessellate", "alpha_shape", "align"]
type NeighbourReduction = Callable[[np.ndarray, np.ndarray, float], "SpatialEvidence"]
type KdReduction = Callable[["cKDTree"], "SpatialEvidence"]


class Metric(StrEnum):
    EUCLIDEAN = "euclidean"
    CITYBLOCK = "cityblock"
    CHEBYSHEV = "chebyshev"
    COSINE = "cosine"
    CORRELATION = "correlation"


class Tessellation(StrEnum):
    VORONOI = "voronoi"
    SPHERICAL = "spherical"
    HALFSPACE = "halfspace"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpatialEvidence:
    tag: Literal["proximity", "complex_", "boundary", "alignment"] = tag()
    proximity: tuple[int, Option[float], Option[float]] = case()
    complex_: tuple[str, int, float] = case()
    boundary: tuple[int, float] = case()
    alignment: tuple[float, float] = case()

    @staticmethod
    def Proximity(count: int, mean_distance: Option[float] = Nothing, radius: Option[float] = Nothing) -> "SpatialEvidence":
        return SpatialEvidence(proximity=(count, mean_distance, radius))

    @staticmethod
    def Complex(kind: str, cardinality: int, measure: float) -> "SpatialEvidence":
        return SpatialEvidence(complex_=(kind, cardinality, measure))

    @staticmethod
    def Boundary(facets: int, total_radius: float) -> "SpatialEvidence":
        return SpatialEvidence(boundary=(facets, total_radius))

    @staticmethod
    def Alignment(rmsd: float, disparity: float) -> "SpatialEvidence":
        return SpatialEvidence(alignment=(rmsd, disparity))

    def facts(self) -> dict[str, object]:
        match self:
            case SpatialEvidence(tag="proximity", proximity=(count, mean_distance, radius)):
                return {
                    "count": count,
                    **mean_distance.map(lambda mean: {"mean_distance": mean}).default_value({}),
                    **radius.map(lambda r: {"radius": r}).default_value({}),
                }
            case SpatialEvidence(tag="complex_", complex_=(kind, cardinality, measure)):
                return {"kind": kind, "cardinality": cardinality, "measure": measure}
            case SpatialEvidence(tag="boundary", boundary=(facets, total_radius)):
                return {"facets": facets, "total_radius": total_radius}
            case SpatialEvidence(tag="alignment", alignment=(rmsd, disparity)):
                return {"rmsd": rmsd, "disparity": disparity}
            case _ as unreachable:
                assert_never(unreachable)


class SpatialReceipt(Struct, frozen=True):
    query: str
    points: int
    lineage: Provenance
    evidence: SpatialEvidence

    @staticmethod
    def of(tag: Tag, points: int, lineage: Provenance, evidence: SpatialEvidence) -> "SpatialReceipt":
        return SpatialReceipt(tag, points, lineage, evidence)

    @property
    def content_key(self) -> ContentKey:
        return self.lineage.produced

    def contribute(self) -> Iterable[Receipt]:
        facts = {"query": self.query, "points": self.points, **self.evidence.facts()}
        yield Receipt.of(
            EvidenceScope.SPATIAL.value,
            ("emitted", self.query, facts),
            key=Some(self.lineage.produced),
            provenance=Some(self.lineage),
        )


@tagged_union(frozen=True)
class SpatialQuery:
    tag: Tag = tag()
    neighbours: tuple[np.ndarray, np.ndarray, int] = case()
    radius: tuple[np.ndarray, np.ndarray, float] = case()
    pairs: tuple[np.ndarray, float] = case()
    distances: tuple[np.ndarray, np.ndarray | None, Metric] = case()
    hull: np.ndarray = case()
    triangulate: np.ndarray = case()
    tessellate: tuple[np.ndarray, Tessellation, float] = case()
    alpha_shape: tuple[np.ndarray, float] = case()
    align: tuple[np.ndarray, np.ndarray] = case()

    @staticmethod
    def Neighbours(points: np.ndarray, queries: np.ndarray, k: int = 1) -> "SpatialQuery":
        return SpatialQuery(neighbours=(points, queries, k))

    @staticmethod
    def Radius(points: np.ndarray, queries: np.ndarray, radius: float) -> "SpatialQuery":
        return SpatialQuery(radius=(points, queries, radius))

    @staticmethod
    def Pairs(points: np.ndarray, radius: float) -> "SpatialQuery":
        return SpatialQuery(pairs=(points, radius))

    @staticmethod
    def Distances(left: np.ndarray, right: np.ndarray | None = None, metric: Metric = Metric.EUCLIDEAN) -> "SpatialQuery":
        return SpatialQuery(distances=(left, right, metric))

    @staticmethod
    def Hull(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(hull=points)

    @staticmethod
    def Triangulate(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(triangulate=points)

    @staticmethod
    def Tessellate(points: np.ndarray, kind: Tessellation = Tessellation.VORONOI, radius: float = 1.0) -> "SpatialQuery":
        return SpatialQuery(tessellate=(points, kind, radius))

    @staticmethod
    def AlphaShape(points: np.ndarray, alpha: float) -> "SpatialQuery":
        return SpatialQuery(alpha_shape=(points, alpha))

    @staticmethod
    def Align(source: np.ndarray, target: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(align=(source, target))

    @property
    def points(self) -> np.ndarray:
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, _)) | SpatialQuery(tag="radius", radius=(pts, qs, _)):
                return np.concatenate([pts, qs])
            case SpatialQuery(tag="distances", distances=(left, right, _)) if right is not None:
                return np.concatenate([left, right])
            case SpatialQuery(tag="align", align=(source, target)):
                return np.concatenate([source, target])
            case (
                SpatialQuery(tag="pairs", pairs=(pts, _))
                | SpatialQuery(tag="distances", distances=(pts, *_))
                | SpatialQuery(tag="tessellate", tessellate=(pts, *_))
                | SpatialQuery(tag="alpha_shape", alpha_shape=(pts, _))
                | SpatialQuery(tag="hull", hull=pts)
                | SpatialQuery(tag="triangulate", triangulate=pts)
            ):
                return pts
            case _ as unreachable:
                assert_never(unreachable)

    def identity_parts(self, operand_key: ContentKey) -> tuple[bytes, ...]:
        row: tuple[object, ...]
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(_, _, k)):
                row = (k,)
            case SpatialQuery(tag="radius", radius=(_, _, r)) | SpatialQuery(tag="pairs", pairs=(_, r)):
                row = (r,)
            case SpatialQuery(tag="distances", distances=(_, _, metric)):
                row = (metric.value,)
            case SpatialQuery(tag="tessellate", tessellate=(_, kind, r)):
                row = (kind.value, r)
            case SpatialQuery(tag="alpha_shape", alpha_shape=(_, alpha)):
                row = (alpha,)
            case SpatialQuery(tag="hull") | SpatialQuery(tag="triangulate") | SpatialQuery(tag="align"):
                row = ()
            case _ as unreachable:
                assert_never(unreachable)
        return (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )

    @property
    def cardinality(self) -> int:
        match self:
            case (
                SpatialQuery(tag="neighbours", neighbours=(pts, *_))
                | SpatialQuery(tag="radius", radius=(pts, *_))
                | SpatialQuery(tag="distances", distances=(pts, *_))
                | SpatialQuery(tag="align", align=(pts, _))
            ):
                return int(pts.shape[0])
            case _:
                return int(self.points.shape[0])

    def resolve(self, workers: int) -> SpatialEvidence:
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                if k < 1 or pts.shape[0] == 0:
                    raise ValueError(f"neighbours requires k >= 1 and a non-empty reference set, got k={k}, points={int(pts.shape[0])}")
                kth = min(k, int(pts.shape[0]))
                return _proximity(
                    "neighbours", pts, qs, float(kth), lambda tree: _knn_distances(np.asarray(tree.query(qs, k=kth, workers=workers)[0], dtype=float))
                )
            case SpatialQuery(tag="radius", radius=(pts, qs, r)):
                return _proximity(
                    "radius",
                    pts,
                    qs,
                    r,
                    lambda tree: SpatialEvidence.Proximity(int(sum(len(hit) for hit in tree.query_ball_point(qs, r=r, workers=workers))), radius=Some(r)),
                )
            case SpatialQuery(tag="pairs", pairs=(pts, r)):
                return _pairs(pts, r)
            case SpatialQuery(tag="distances", distances=(left, right, metric)):
                return _distances(left, right, metric)
            case SpatialQuery(tag="hull", hull=pts):
                return _hull(pts)
            case SpatialQuery(tag="triangulate", triangulate=pts):
                return _triangulate(pts)
            case SpatialQuery(tag="tessellate", tessellate=(pts, kind, radius)):
                return _tessellate(pts, kind, radius)
            case SpatialQuery(tag="alpha_shape", alpha_shape=(pts, alpha)):
                return _alpha_shape(pts, alpha)
            case SpatialQuery(tag="align", align=(source, target)):
                return _align(source, target)
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

SPATIAL_RESOLVE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.SPATIAL, point="resolve", arm="boundary", defect="kernel-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([SPATIAL_RESOLVE]))

NEIGHBOUR_FLOOR: Final[Map[Tag, NeighbourReduction]] = Map.of_seq([
    ("neighbours", lambda pts, qs, k: _floor_knn(pts, qs, int(k))),
    ("radius", lambda pts, qs, r: _floor_radius(pts, qs, r)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity(tag: Tag, pts: np.ndarray, qs: np.ndarray, scale: float, reduce: "KdReduction") -> SpatialEvidence:
    try:
        tree = sp.cKDTree(pts)
    except ImportError:
        return NEIGHBOUR_FLOOR[tag](pts, qs, scale)
    return reduce(tree)


def _pairwise_sq(pts: np.ndarray, qs: np.ndarray) -> np.ndarray:
    diff = qs[:, None, :] - pts[None, :, :]
    return np.einsum("qnd,qnd->qn", diff, diff)


def _knn_distances(distances: np.ndarray) -> SpatialEvidence:
    return SpatialEvidence.Proximity(int(distances.size), Some(float(np.mean(distances))) if distances.size else Nothing)


def _floor_knn(pts: np.ndarray, qs: np.ndarray, k: int) -> SpatialEvidence:
    kth = min(k, pts.shape[0])
    return _knn_distances(np.sqrt(np.sort(_pairwise_sq(pts, qs), axis=1)[:, :kth]))


def _floor_radius(pts: np.ndarray, qs: np.ndarray, r: float) -> SpatialEvidence:
    return SpatialEvidence.Proximity(int(np.count_nonzero(_pairwise_sq(pts, qs) <= r * r)), radius=Some(r))


def _pairs(pts: np.ndarray, r: float) -> SpatialEvidence:
    return SpatialEvidence.Proximity(len(sp.cKDTree(pts).query_pairs(r)), radius=Some(r))


def _hull(pts: np.ndarray) -> SpatialEvidence:
    hull = sp.ConvexHull(pts)
    return SpatialEvidence.Complex("hull", int(hull.simplices.shape[0]), float(hull.volume))


def _triangulate(pts: np.ndarray) -> SpatialEvidence:
    tri = sp.Delaunay(pts)
    return SpatialEvidence.Complex("delaunay", int(tri.simplices.shape[0]), float(pts.shape[0]))


def _distances(left: np.ndarray, right: np.ndarray | None, metric: Metric) -> SpatialEvidence:
    pairwise = sp.distance.pdist(left, metric=metric.value) if right is None else sp.distance.cdist(left, right, metric=metric.value)
    return SpatialEvidence.Complex(f"distance-{metric.value}", int(pairwise.size), float(np.mean(pairwise)))


def _tessellate(points: np.ndarray, kind: Tessellation, radius: float) -> SpatialEvidence:
    match kind:
        case Tessellation.VORONOI:
            vor = sp.Voronoi(points)
            return SpatialEvidence.Complex("voronoi", int(vor.ridge_points.shape[0]), float(len(vor.regions)))
        case Tessellation.SPHERICAL:
            sphere = sp.SphericalVoronoi(points, radius=radius)
            return SpatialEvidence.Complex("spherical-voronoi", int(sphere.vertices.shape[0]), float(len(sphere.regions)))
        case Tessellation.HALFSPACE:
            verts = sp.HalfspaceIntersection(points, _interior_point(points)).intersections
            return SpatialEvidence.Complex("halfspace", int(verts.shape[0]), float(np.linalg.norm(verts.max(axis=0) - verts.min(axis=0))))
        case _ as unreachable:
            assert_never(unreachable)


def _interior_point(halfspaces: np.ndarray) -> np.ndarray:
    a, b = halfspaces[:, :-1], halfspaces[:, -1]
    norms = np.linalg.norm(a, axis=1, keepdims=True)
    result = linprog(np.concatenate([np.zeros(a.shape[1]), [-1.0]]), A_ub=np.hstack([a, norms]), b_ub=-b, bounds=(None, None))
    if not result.success:
        raise ValueError(f"halfspace stack admits no strictly feasible interior point: {result.message}")
    return np.asarray(result.x[:-1], dtype=float)


def _align(source: np.ndarray, target: np.ndarray) -> SpatialEvidence:
    _, rssd = sp.transform.Rotation.align_vectors(target, source)
    _, _, disparity = sp.procrustes(target, source)
    return SpatialEvidence.Alignment(float(rssd / np.sqrt(source.shape[0])), float(disparity))


def _alpha_shape(points: np.ndarray, alpha: float) -> SpatialEvidence:
    tri = sp.Delaunay(points)
    radii = np.asarray([_circumradius(simplex) for simplex in points[tri.simplices]])
    retained = tri.simplices[kept := radii < alpha]
    facets = np.sort(np.concatenate([retained[:, list(combo)] for combo in combinations(range(retained.shape[1]), retained.shape[1] - 1)]), axis=1)
    unique, counts = np.unique(facets, axis=0, return_counts=True)
    return SpatialEvidence.Boundary(int(unique[counts == 1].shape[0]), float(np.sum(radii[kept])))


def _circumradius(simplex: np.ndarray) -> float:
    base = simplex[1:] - simplex[0]
    rhs = 0.5 * np.einsum("ij,ij->i", base, base)
    return float(np.linalg.norm(np.linalg.solve(base, rhs)))


def _spatial_kernel(query: SpatialQuery, workers: int) -> "RuntimeRail[SpatialReceipt]":
    return ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"spatial.{query.tag}", IdentitySource(parts=query.identity_parts(payload.content_key))).bind(
            lambda result_key: boundary(
                SPATIAL_RESOLVE,
                lambda: SpatialReceipt.of(
                    query.tag, query.cardinality,
                    Provenance(consumed=Block.singleton(payload.content_key), produced=result_key),
                    query.resolve(workers),
                ),
                catch=(np.linalg.LinAlgError, ValueError, RuntimeError),
            )
        )
    )


async def solve(query: SpatialQuery, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[SpatialReceipt]":
    async def dispatch() -> RuntimeRail[SpatialReceipt]:
        return (
            await lane.whole(
                lambda grant: lane.offload(Kernel.of(_spatial_kernel, KernelTrait.RELEASING), query, grant.width)
            )
        ).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.SPATIAL, f"spatial.{query.tag}", dispatch, facts={"query": query.tag, "points": query.cardinality}, composition=composition)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
