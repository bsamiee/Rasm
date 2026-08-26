# [PY_COMPUTE_SPATIAL]

`SpatialQuery` returns the computed neighbour, distance, Qhull, boundary, or alignment product directly. Geometry retains the `trimesh` mesh surface, while compute owns these array-native point-set products.

Each point set admits through `numerics/array#PAYLOAD`. `LanePolicy.whole` threads the admitted width into SciPy's KD-tree calls.

## [01]-[INDEX]

- [02]-[SPATIAL]: `SpatialQuery` dispatch over one point set returning each native product.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` discriminates the geometric question and resolves its native product. `AlphaShape` owns its local Delaunay boundary fold.
- Output: Each query returns KD-tree distances and indices, radius hit indices, pair indices, a distance array, SciPy's Qhull object, alpha boundary facets, or the provider alignment values.
- Faults: `SPATIAL_RESOLVE` fences provider and numeric failures. `_proximity` selects the array floor only when SciPy's tree cannot load.
- Packages: `scipy.spatial` owns KD-tree, Qhull, distance, Procrustes, and rotation products; `numpy` owns the array products.
- Growth: add one `SpatialQuery` case and one `resolve` arm.

```python
from collections.abc import Callable
from enum import StrEnum
from itertools import combinations
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block
from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, boundary, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import scipy.spatial as sp
lazy from scipy.optimize import linprog

if TYPE_CHECKING:
    from scipy.spatial import ConvexHull, Delaunay, HalfspaceIntersection, SphericalVoronoi, Voronoi, cKDTree
    from scipy.spatial.transform import Rotation

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["neighbours", "radius", "pairs", "distances", "hull", "triangulate", "tessellate", "alpha_shape", "align"]
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

    def resolve(
        self, workers: int
    ) -> (
        "np.ndarray | tuple[np.ndarray | Rotation | float, ...] | set[tuple[int, int]]"
        " | ConvexHull | Delaunay | Voronoi | SphericalVoronoi | HalfspaceIntersection"
    ):
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                if k < 1 or pts.shape[0] == 0:
                    raise ValueError(f"neighbours requires k >= 1 and a non-empty reference set, got k={k}, points={int(pts.shape[0])}")
                kth = min(k, int(pts.shape[0]))
                distances, indices = _proximity(
                    pts,
                    lambda tree: tree.query(qs, k=kth, workers=workers),
                    lambda: _floor_knn(pts, qs, kth),
                )
                return np.asarray(distances).reshape(qs.shape[0], -1), np.asarray(indices).reshape(qs.shape[0], -1)
            case SpatialQuery(tag="radius", radius=(pts, qs, r)):
                hits = _proximity(
                    pts,
                    lambda tree: tree.query_ball_point(qs, r=r, workers=workers, return_sorted=True),
                    lambda: _floor_radius(pts, qs, r),
                )
                return tuple(np.asarray(hit, dtype=np.intp) for hit in hits)
            case SpatialQuery(tag="pairs", pairs=(pts, r)):
                return sp.cKDTree(pts).query_pairs(r, output_type="set")
            case SpatialQuery(tag="distances", distances=(left, right, metric)):
                pairwise = sp.distance.pdist(left, metric=metric.value) if right is None else sp.distance.cdist(left, right, metric=metric.value)
                return pairwise
            case SpatialQuery(tag="hull", hull=pts):
                return sp.ConvexHull(pts)
            case SpatialQuery(tag="triangulate", triangulate=pts):
                return sp.Delaunay(pts)
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

# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity[T](pts: np.ndarray, query: Callable[["cKDTree"], T], floor: Callable[[], T]) -> T:
    try:
        tree = sp.cKDTree(pts)
    except ImportError:
        return floor()
    return query(tree)


def _pairwise_sq(pts: np.ndarray, qs: np.ndarray) -> np.ndarray:
    diff = qs[:, None, :] - pts[None, :, :]
    return np.einsum("qnd,qnd->qn", diff, diff)


def _floor_knn(pts: np.ndarray, qs: np.ndarray, k: int) -> tuple[np.ndarray, np.ndarray]:
    squared = _pairwise_sq(pts, qs)
    indices = np.argsort(squared, axis=1)[:, :k]
    return np.sqrt(np.take_along_axis(squared, indices, axis=1)), indices


def _floor_radius(pts: np.ndarray, qs: np.ndarray, r: float) -> tuple[np.ndarray, ...]:
    return tuple(np.flatnonzero(row) for row in _pairwise_sq(pts, qs) <= r * r)


def _tessellate(
    points: np.ndarray, kind: Tessellation, radius: float
) -> "Voronoi | SphericalVoronoi | HalfspaceIntersection":
    match kind:
        case Tessellation.VORONOI:
            return sp.Voronoi(points)
        case Tessellation.SPHERICAL:
            return sp.SphericalVoronoi(points, radius=radius)
        case Tessellation.HALFSPACE:
            return sp.HalfspaceIntersection(points, _interior_point(points))
        case _ as unreachable:
            assert_never(unreachable)


def _interior_point(halfspaces: np.ndarray) -> np.ndarray:
    a, b = halfspaces[:, :-1], halfspaces[:, -1]
    norms = np.linalg.norm(a, axis=1, keepdims=True)
    result = linprog(np.concatenate([np.zeros(a.shape[1]), [-1.0]]), A_ub=np.hstack([a, norms]), b_ub=-b, bounds=(None, None))
    if not result.success:
        raise ValueError(f"halfspace stack admits no strictly feasible interior point: {result.message}")
    return np.asarray(result.x[:-1], dtype=float)


def _align(source: np.ndarray, target: np.ndarray) -> "tuple[Rotation, float, np.ndarray, np.ndarray, float]":
    rotation, rssd = sp.transform.Rotation.align_vectors(target, source)
    standard_target, standard_source, disparity = sp.procrustes(target, source)
    return rotation, float(rssd), standard_target, standard_source, float(disparity)


def _alpha_shape(points: np.ndarray, alpha: float) -> np.ndarray:
    tri = sp.Delaunay(points)
    radii = np.asarray([_circumradius(simplex) for simplex in points[tri.simplices]])
    retained = tri.simplices[kept := radii < alpha]
    facets = np.sort(np.concatenate([retained[:, list(combo)] for combo in combinations(range(retained.shape[1]), retained.shape[1] - 1)]), axis=1)
    unique, counts = np.unique(facets, axis=0, return_counts=True)
    return unique[counts == 1]


def _circumradius(simplex: np.ndarray) -> float:
    base = simplex[1:] - simplex[0]
    rhs = 0.5 * np.einsum("ij,ij->i", base, base)
    return float(np.linalg.norm(np.linalg.solve(base, rhs)))


def _spatial_kernel(
    query: SpatialQuery, workers: int
) -> (
    "RuntimeResult[np.ndarray | tuple[np.ndarray | Rotation | float, ...] | set[tuple[int, int]]"
    " | ConvexHull | Delaunay | Voronoi | SphericalVoronoi | HalfspaceIntersection]"
):
    return ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT).bind(
        lambda _: boundary(
            SPATIAL_RESOLVE,
            lambda: query.resolve(workers),
            catch=(np.linalg.LinAlgError, ValueError, RuntimeError),
        )
    )


async def solve(
    query: SpatialQuery, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
) -> (
    "RuntimeResult[np.ndarray | tuple[np.ndarray | Rotation | float, ...] | set[tuple[int, int]]"
    " | ConvexHull | Delaunay | Voronoi | SphericalVoronoi | HalfspaceIntersection]"
):
    async def dispatch() -> (
        "RuntimeResult[np.ndarray | tuple[np.ndarray | Rotation | float, ...] | set[tuple[int, int]]"
        " | ConvexHull | Delaunay | Voronoi | SphericalVoronoi | HalfspaceIntersection]"
    ):
        return (
            await lane.whole(
                lambda grant: lane.offload(Kernel.of(_spatial_kernel, KernelTrait.RELEASING), query, grant.width)
            )
        ).bind(lambda held: held)

    return await evidence_run(
        EvidenceScope.SPATIAL,
        f"spatial.{query.tag}",
        dispatch,
        facts={"query": query.tag, "points": query.cardinality},
        composition=composition,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
