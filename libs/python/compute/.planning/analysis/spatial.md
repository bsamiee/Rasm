# [PY_COMPUTE_SPATIAL]

One array-native computational-geometry owner rules: `SpatialQuery` discriminates Qhull tessellation, KD-tree proximity, the pairwise/condensed distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold over a point set, and `resolve` folds every case to a `SpatialEvidence` outcome the `SpatialReceipt` carries whole. This owner emits point-set evidence as compute-native receipts and never re-owns the geometry-branch `trimesh` mesh surface; the graduation direction is closed one-way — geometry's reconstruction plane mints `reconstructed-mesh`, the alpha-shape `Boundary` stays a compute-native receipt product, and an outward crossing requires a named consumer plus a compute-owned axis case, never the geometry case.

Each point set admits through `numerics/array.md#PAYLOAD` for the finite gate and the operand `ContentKey`; the receipt keys the RESULT through the query-owned `identity_buffer` fold, so two different queries over one point set never share a key; the resolved receipt is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect. `scipy.spatial` is not Array-API-aware, so the point set is the numpy `np.ndarray` the Qhull/KD-tree/BLAS backends bind under the `RELEASING` trait, isolation and band deriving at the runtime `Kernel` crossing; the KD-tree scan team binds to `LanePolicy.capacity` threaded through the kernel, never the unbounded `workers=-1` team that oversubscribes an already-offloaded kernel against the band.

## [01]-[INDEX]

- [01]-[SPATIAL]: the `SpatialQuery` cases over one point set, evidence discriminated over `SpatialEvidence`, the two KD-tree routes degrading through the data-driven `NEIGHBOUR_FLOOR`.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — one owner discriminated by the geometric question, never a `Neighbours`/`Hull`/`Triangulate` method family. `Align` is a paired correspondence fit — `source` and `target` carry the same row count, `procrustes` raising `ValueError` on a mismatch, the fault converting on the `boundary` fence. `AlphaShape` folds its boundary locally because no `scipy.spatial` alpha-shape primitive exists; `_circumradius` stays private to that kernel, never a module-level sibling of the dispatch.
- Output: `SpatialEvidence` parameterizes the result per case, and the `Complex` `cardinality` is the primitive count its `kind` string discriminates — hull facets, Delaunay simplices, Voronoi ridges, halfspace vertices, distance pairs — so a distance summary never wears a `simplices` label and four outcome vocabularies never smuggle through two overloaded columns. Adding a query writes only its geometry body returning a `SpatialEvidence`; `assert_never` closes the dispatch.
- Packages: `scipy.spatial`, `numpy`, `expression`, and `msgspec` per the fence imports; each kernel binds `import scipy.spatial as sp` once at its head so `resolve` stays a pure tag-dispatch, and `optimize.linprog` enters only for the halfspace Chebyshev-centre interior point.
- Growth: a new spatial query is one `SpatialQuery` case plus one `resolve` arm and one `identity_buffer` arm; a new evidence shape is one `SpatialEvidence` case plus its `facts()` arm — the receipt carries the evidence whole and needs no edit; a new distance metric is one `Metric` row; a new tessellation backend is one `Tessellation` row; a new degrading route is one `NEIGHBOUR_FLOOR` row.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import combinations
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    # declared here so the `KdReduction` signature names the real carrier rather than degrading to a bare `object`.
    from scipy.spatial import cKDTree

# --- [TYPES] ----------------------------------------------------------------------------

type Tag = Literal["neighbours", "radius", "pairs", "distances", "hull", "triangulate", "tessellate", "alpha_shape", "align"]
type NeighbourReduction = Callable[[np.ndarray, np.ndarray, float], "SpatialEvidence"]
type KdReduction = Callable[["cKDTree"], "SpatialEvidence"]


class Metric(StrEnum):  # the scipy.spatial.distance metric= argument as a bounded vocabulary, never a string knob
    EUCLIDEAN = "euclidean"  # Minkowski p=2
    CITYBLOCK = "cityblock"  # Minkowski p=1
    CHEBYSHEV = "chebyshev"  # Minkowski p=inf
    COSINE = "cosine"
    CORRELATION = "correlation"


class Tessellation(StrEnum):  # the Voronoi backend selector; the value is the scipy carrier name lowercased
    VORONOI = "voronoi"
    SPHERICAL = "spherical"
    HALFSPACE = "halfspace"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SpatialEvidence:
    tag: Literal["proximity", "complex_", "boundary", "alignment"] = tag()
    proximity: tuple[int, float, float] = case()
    complex_: tuple[str, int, float] = case()
    boundary: tuple[int, float] = case()
    alignment: tuple[float, float] = case()

    @staticmethod
    def Proximity(count: int, mean_distance: float, radius: float = 0.0) -> "SpatialEvidence":
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
                return {"count": count, "mean_distance": f"{mean_distance:.6g}", "radius": f"{radius:.6g}"}
            case SpatialEvidence(tag="complex_", complex_=(kind, cardinality, measure)):
                return {"kind": kind, "cardinality": cardinality, "measure": f"{measure:.6g}"}
            case SpatialEvidence(tag="boundary", boundary=(facets, total_radius)):
                return {"facets": facets, "total_radius": f"{total_radius:.6g}"}
            case SpatialEvidence(tag="alignment", alignment=(rmsd, disparity)):
                return {"rmsd": f"{rmsd:.6g}", "disparity": f"{disparity:.6g}"}
            case _ as unreachable:
                assert_never(unreachable)


class SpatialReceipt(Struct, frozen=True):
    query: str
    points: int
    content_key: ContentKey
    evidence: SpatialEvidence

    @staticmethod
    def of(tag: Tag, points: int, key: ContentKey, evidence: SpatialEvidence) -> "SpatialReceipt":
        return SpatialReceipt(tag, points, key, evidence)

    def contribute(self) -> Iterable[Receipt]:
        facts = {"query": self.query, "points": self.points, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of("compute.spatial", ("emitted", self.query, facts))


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
        # `right=None` is the self-distance discriminant (condensed `pdist`); a real `right` is the
        # cross matrix (`cdist`) — never a mutable `np.empty` class-definition-time default.
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
        # OPERAND buffer the ArrayPayload admits, recovered from every case so admission stays one path; the two-set routes stack
        # both operands so both coordinate buffers seed the operand key and a shared-`left` query with a distinct `right`/`target` never collides.
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

    def identity_buffer(self, operand_key: ContentKey) -> bytes:
        # enum rows serialize by value, numeric rows as canonical float64 bytes; length-prefixed parts keep the buffer unambiguous.
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
        parts = (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )
        return b"".join(len(part).to_bytes(8, "big") + part for part in parts)

    @property
    def cardinality(self) -> int:
        # Receipt's reference-set count — the leading coordinate buffer's row count. `query.points.shape[0]` reports
        # `len(pts) + len(queries)` on the two-set routes, conflating the query set into the reference count; the two concerns
        # stay split across the identity operand and this receipt count.
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
        # `workers` is the lane capacity both KD-tree scans bind — `-1` fans a full-CPU team inside a band-bounded worker.
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                # admission precedes the clamp: a non-positive `k` and an empty reference set are caller defects the
                # `spatial.neighbours` boundary fence converts — clamped or floored past this point, each emits a nan
                # Proximity mean as valid evidence instead of a typed refusal, the same raise-at-the-fence contract
                # the align arm's `procrustes` mismatch rides.
                if k < 1 or pts.shape[0] == 0:
                    raise ValueError(f"neighbours requires k >= 1 and a non-empty reference set, got k={k}, points={int(pts.shape[0])}")
                # `k` clamps to the point count at the ONE dispatch site, so the cKDTree route and the numpy floor
                # aggregate identical slot counts — an unclamped `k > n` query pads inf distances that poison the
                # Proximity mean on one path while the floor's slice silently narrows on the other.
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
                    lambda tree: SpatialEvidence.Proximity(int(sum(len(hit) for hit in tree.query_ball_point(qs, r=r, workers=workers))), 0.0, r),
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

# Data-driven numpy proximity floor: the ImportError arm reads its row and folds the same Proximity evidence the cKDTree path
# produces, so the floor is table membership rather than per-route try/except blocks. A tag absent from this table has no floor —
# Qhull, the BLAS distance kernel, and the rotation SVD are the gated capability itself.
NEIGHBOUR_FLOOR: Final[Map[Tag, NeighbourReduction]] = Map.of_seq([
    ("neighbours", lambda pts, qs, k: _floor_knn(pts, qs, int(k))),
    ("radius", lambda pts, qs, r: _floor_radius(pts, qs, r)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity(tag: Tag, pts: np.ndarray, qs: np.ndarray, scale: float, reduce: "KdReduction") -> SpatialEvidence:
    # one symmetric fold for both KD-tree proximity routes: run the scipy reduction, or fall to this tag's floor row when the
    # package is absent — both terminate in `Proximity`, so the body carries no per-tag ternary.
    try:
        import scipy.spatial as sp

        return reduce(sp.cKDTree(pts))
    except ImportError:
        return NEIGHBOUR_FLOOR[tag](pts, qs, scale)


def _pairwise_sq(pts: np.ndarray, qs: np.ndarray) -> np.ndarray:
    # One squared-distance kernel both floor rows read: the kNN floor sorts+`sqrt`s it, the radius floor thresholds it against
    # `r**2` with no per-pair `sqrt`.
    diff = qs[:, None, :] - pts[None, :, :]
    return np.einsum("qnd,qnd->qn", diff, diff)


def _knn_distances(distances: np.ndarray) -> SpatialEvidence:
    # One Proximity mean both the cKDTree `query` distances and the floor's sorted block fold, so the two paths terminate
    # identically; an empty query set folds to a zero mean — truthful vacuous evidence, never a nan riding `np.mean` over nothing.
    return SpatialEvidence.Proximity(int(distances.size), float(np.mean(distances)) if distances.size else 0.0)


def _floor_knn(pts: np.ndarray, qs: np.ndarray, k: int) -> SpatialEvidence:
    # `kth` clamps to the point count so a `k >= n` request mirrors the cKDTree tolerance rather
    # than slicing past the column count; `sqrt` lands only on the retained k-nearest columns.
    kth = min(k, pts.shape[0])
    return _knn_distances(np.sqrt(np.sort(_pairwise_sq(pts, qs), axis=1)[:, :kth]))


def _floor_radius(pts: np.ndarray, qs: np.ndarray, r: float) -> SpatialEvidence:
    return SpatialEvidence.Proximity(int(np.count_nonzero(_pairwise_sq(pts, qs) <= r * r)), 0.0, r)


def _pairs(pts: np.ndarray, r: float) -> SpatialEvidence:
    import scipy.spatial as sp

    return SpatialEvidence.Proximity(len(sp.cKDTree(pts).query_pairs(r)), 0.0, r)


def _hull(pts: np.ndarray) -> SpatialEvidence:
    import scipy.spatial as sp

    hull = sp.ConvexHull(pts)
    return SpatialEvidence.Complex("hull", int(hull.simplices.shape[0]), float(hull.volume))


def _triangulate(pts: np.ndarray) -> SpatialEvidence:
    import scipy.spatial as sp

    # Delaunay carries no `.volume` the way ConvexHull does, so `measure` reads the triangulated point cardinality — a real fact,
    # never a simplex-count echo.
    tri = sp.Delaunay(pts)
    return SpatialEvidence.Complex("delaunay", int(tri.simplices.shape[0]), float(pts.shape[0]))


def _distances(left: np.ndarray, right: np.ndarray | None, metric: Metric) -> SpatialEvidence:
    import scipy.spatial as sp

    # self-distance reduces the condensed `pdist` vector directly, never a dense squareform materialized only to mean over it.
    pairwise = sp.distance.pdist(left, metric=metric.value) if right is None else sp.distance.cdist(left, right, metric=metric.value)
    return SpatialEvidence.Complex(f"distance-{metric.value}", int(pairwise.size), float(np.mean(pairwise)))


def _tessellate(points: np.ndarray, kind: Tessellation, radius: float) -> SpatialEvidence:
    import scipy.spatial as sp

    match kind:
        case Tessellation.VORONOI:
            vor = sp.Voronoi(points)
            return SpatialEvidence.Complex("voronoi", int(vor.ridge_points.shape[0]), float(len(vor.regions)))
        case Tessellation.SPHERICAL:
            sphere = sp.SphericalVoronoi(points, radius=radius)
            return SpatialEvidence.Complex("spherical-voronoi", int(sphere.vertices.shape[0]), float(len(sphere.regions)))
        case Tessellation.HALFSPACE:
            # Operand is the `(N, d+1)` halfspace stack `[A | b]`, not a point set; `HalfspaceIntersection` needs a strictly
            # feasible interior point, so the `linprog` Chebyshev centre replaces an `np.zeros` Qhull rejects as infeasible for any
            # stack not straddling the origin.
            verts = sp.HalfspaceIntersection(points, _interior_point(points)).intersections
            return SpatialEvidence.Complex("halfspace", int(verts.shape[0]), float(np.linalg.norm(verts.max(axis=0) - verts.min(axis=0))))
        case _ as unreachable:
            assert_never(unreachable)


def _interior_point(halfspaces: np.ndarray) -> np.ndarray:
    from scipy.optimize import linprog

    a, b = halfspaces[:, :-1], halfspaces[:, -1]
    norms = np.linalg.norm(a, axis=1, keepdims=True)
    result = linprog(np.concatenate([np.zeros(a.shape[1]), [-1.0]]), A_ub=np.hstack([a, norms]), b_ub=-b, bounds=(None, None))
    return np.asarray(result.x[:-1], dtype=float)


def _align(source: np.ndarray, target: np.ndarray) -> SpatialEvidence:
    import scipy.spatial as sp

    # `align_vectors` returns the rotation and the root-sum-square deviation; the per-vector RMSD
    # is rssd / sqrt(n), so the evidence reports a sample-independent fit error, not a raw sum.
    _, rssd = sp.transform.Rotation.align_vectors(target, source)
    _, _, disparity = sp.procrustes(target, source)
    return SpatialEvidence.Alignment(float(rssd / np.sqrt(source.shape[0])), float(disparity))


def _alpha_shape(points: np.ndarray, alpha: float) -> SpatialEvidence:
    import scipy.spatial as sp

    tri = sp.Delaunay(points)
    radii = np.asarray([_circumradius(simplex) for simplex in points[tri.simplices]])
    retained = tri.simplices[kept := radii < alpha]
    # each (d+1)-simplex contributes its (d+1) facets — the (d)-vertex subsets `combinations`
    # enumerates; a facet shared by two retained simplices is interior, one in exactly one is boundary.
    facets = np.sort(np.concatenate([retained[:, list(combo)] for combo in combinations(range(retained.shape[1]), retained.shape[1] - 1)]), axis=1)
    unique, counts = np.unique(facets, axis=0, return_counts=True)
    return SpatialEvidence.Boundary(int(unique[counts == 1].shape[0]), float(np.sum(radii[kept])))


def _circumradius(simplex: np.ndarray) -> float:
    base = simplex[1:] - simplex[0]
    rhs = 0.5 * np.einsum("ij,ij->i", base, base)
    return float(np.linalg.norm(np.linalg.solve(base, rhs)))


def _spatial_kernel(query: SpatialQuery, workers: int) -> "RuntimeRail[SpatialReceipt]":
    # module-level so REFERENCE shipping resolves it by import — a closure pays an eager cloudpickle round-trip
    # no thread arm needs; SYNCHRONOUS by contract, an async def hands the worker a bare coroutine object.
    return ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"spatial.{query.tag}", query.identity_buffer(payload.content_key)).bind(
            lambda result_key: boundary(
                f"spatial.{query.tag}", lambda: SpatialReceipt.of(query.tag, query.cardinality, result_key, query.resolve(workers))
            )
        )
    )


async def solve(query: SpatialQuery, lane: LanePolicy) -> "RuntimeRail[SpatialReceipt]":
    # Weave owns span, fence, and the `@receipted` receipt harvest; `lane.capacity` bounds the KD-tree scan team.
    async def dispatch() -> RuntimeRail[SpatialReceipt]:
        # One flatten from `RuntimeRail[RuntimeRail[SpatialReceipt]]` to `RuntimeRail[SpatialReceipt]`.
        return (await lane.offload(Kernel.of(_spatial_kernel, KernelTrait.RELEASING), query, lane.capacity)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.SPATIAL, f"spatial.{query.tag}", dispatch)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
