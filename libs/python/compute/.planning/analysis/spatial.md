# [PY_COMPUTE_SPATIAL]

The one array-native computational-geometry owner over `scipy.spatial`. `SpatialQuery` is a `@tagged_union` discriminating Qhull tessellation, KD-tree proximity, the pairwise/condensed distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold over a point set, and `SpatialQuery.resolve` is the one total `match` folding every case to a `SpatialEvidence` discriminated outcome — `Proximity`, `Complex`, `Boundary`, or `Alignment` — that `SpatialReceipt` carries whole, the evidence owning its own `facts()` slot projection, exactly as `analysis/symbolic.md#OP` collapses its `Outcome` and the way the sibling `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` owners cite this owner for collapsing `Proximity`/`Complex`/`Boundary`. The point set admits through `numerics/array.md#PAYLOAD` so the `ArrayPayload.content_key` keys the receipt by `ContentIdentity` and a repeated query on identical points is a cache hit by reference. A data-driven `NEIGHBOUR_FLOOR` supplies a numpy brute-force pairwise reduction for the two KD-tree proximity routes so a runtime run with the scipy package absent still returns a `SpatialReceipt` rather than `Error(import_)`; the Qhull hull/Delaunay/Voronoi, the rotation algebra, and the distance matrix carry no floor because their backend is the gated capability itself. `@receipted` is the cross-cutting emit aspect so the point-cloud and sample-set neighbourhood evidence the study spine reads streams without an inline `emit`. This owner emits point-set evidence and never re-owns the geometry-branch `geometry/mesh/spatial.md#SPATIAL` `trimesh` mesh surface, meeting it only at the graduation wire.

## [01]-[INDEX]

- [01]-[SPATIAL]: the `SpatialQuery` `@tagged_union` over Qhull tessellation, KD-tree proximity, the pairwise distance matrix, the rotation-and-alignment algebra, and the alpha-shape boundary fold; `SpatialQuery.resolve` the one total `match` folding every case to the `SpatialEvidence` discriminated outcome `SpatialReceipt` carries whole through the evidence's own `facts()` projection; `Metric`/`Tessellation` the bounded backend-argument vocabularies; `NEIGHBOUR_FLOOR` the data-driven numpy proximity floor for the two KD-tree routes; the `ArrayPayload`-admitted `ContentIdentity` key the study spine's `@receipted` aspect harvests off the `Ok`-arm contributor.

## [02]-[SPATIAL]

- Owner: `SpatialQuery` — the array-native computational-geometry cases over a point set discriminated by the geometric question, never a per-query owner and never a `Neighbours`/`Hull`/`Triangulate` method family. `Neighbours(points, queries, k)` over `scipy.spatial.cKDTree.query` (the `workers=-1` thread-parallel k-nearest), `Radius(points, queries, radius)` over `cKDTree.query_ball_point`, `Pairs(points, radius)` over `cKDTree.query_pairs` (the within-set proximity graph), `Distances(left, right, metric)` over `scipy.spatial.distance.cdist` (cross-set) or `distance.pdist`+`squareform` (self, `right` empty) discriminated by the `Metric` vocabulary, `Hull(points)` over `scipy.spatial.ConvexHull`, `Triangulate(points)` over `scipy.spatial.Delaunay`, `Tessellate(points, kind, radius)` over `scipy.spatial.Voronoi`/`SphericalVoronoi`/`HalfspaceIntersection` discriminated by the `Tessellation` vocabulary, `AlphaShape(points, alpha)` folding the boundary from the Delaunay circumradius, and `Align(source, target)` over `scipy.spatial.transform.Rotation.align_vectors` (the rotation Kabsch fit) and `scipy.spatial.procrustes` (the optimal similarity disparity) — a paired correspondence fit, so `source` and `target` carry the same row count (`align_vectors` aligns the position-vector pairs and `procrustes` raises `ValueError` on a shape mismatch, that fault converting on the `boundary` fence). `solve` returns `RuntimeRail[SpatialReceipt]`, admitting the operand through `numerics/array.md#PAYLOAD` for the finite gate and `content_key`, folding the resolved `SpatialEvidence` to one `SpatialReceipt`, and recording the reference-set count through `SpatialQuery.cardinality` rather than the concatenated identity buffer's row count.
- Outcome: `SpatialEvidence` is the `@tagged_union` parameterizing the result the way the input is parameterized — `Proximity(count, mean_distance, radius)` for the two KD-tree routes and the within-set `Pairs` graph, `Complex(kind, cardinality, measure)` for the Qhull hull/Delaunay/Voronoi/halfspace tessellation and the distance-matrix summary, `Boundary(facets, total_radius)` for the alpha-shape reconstruction, and `Alignment(rmsd, disparity)` for the Kabsch/procrustes fit — and `SpatialQuery.resolve` is the one total `match` over the union tag returning `SpatialEvidence`, closed by `assert_never`, so adding a query writes only its geometry body returning a `SpatialEvidence` rather than a parallel `_neighbours`/`_hull`/`_triangulate` helper family. The evidence owns its own `SpatialEvidence.facts()` slot projection so `Proximity` names `count`/`mean_distance`/`radius`, `Complex` names `kind`/`cardinality`/`measure`, `Boundary` names `facets`/`total_radius`, and `Alignment` names `rmsd`/`disparity` — the `Complex` `cardinality` is the primitive count the `kind` string discriminates (hull facets, Delaunay simplices, Voronoi ridges, halfspace vertices, distance pairs), never a `simplices` label lying over a pair count for the distance route, and never a coarse `(count, measure)` pair smuggling four distinct outcome vocabularies through two overloaded columns — the same self-projecting shape `analysis/transform.md#TRANSFORM` holds over `TransformEvidence.facts()` and `analysis/signal.md#DSP` over `Spectral`/`Multiresolution`/`Scale`/`Packet`.
- Alpha-shape fold: `_alpha_shape` is the one local kernel beside the scipy queries — no `scipy.spatial` alpha-shape primitive exists. It triangulates the point set, computes each `Delaunay` simplex circumradius from its vertex coordinates through `np.linalg.solve` over the edge basis, retains the simplices whose circumradius is below `alpha`, and reduces them to the boundary facets appearing in exactly one retained simplex through `itertools.combinations(range(d+1), d)` over the facet-vertex subsets and `np.unique(..., return_counts=True)` — the reconstructed boundary the study spine reads, the facet enumeration the stdlib `combinations` primitive rather than a hand-rolled drop-one index generator. `_circumradius` is private to the kernel, not a module-level sibling of the dispatch.
- Receipt: `SpatialReceipt.of(tag, points, key, evidence)` folds the `(query, points, content_key, evidence)` carrier with no per-case projection — the discrimination lives in `SpatialEvidence.facts()`, not a `match` over flattened columns — the same evidence-on-receipt carry `analysis/signal.md#DSP` and `analysis/symbolic.md#OP` hold. The `points` slot is the `SpatialQuery.cardinality` reference-set count, the leading coordinate buffer's row count, never the `query.points.shape[0]` concatenated identity buffer that sums `len(pts) + len(queries)` on the `neighbours`/`radius`/`align`/cross-`distances` two-set routes — the identity buffer concatenates both operands so a shared-`left` query keys distinctly, but the receipt's count is the geometric subject, the two concerns split across `points`/`cardinality` rather than conflated through one `operand.shape[0]`. `contribute` `yield`s into the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`) — the one corpus-wide port-type annotation every owner carries, the `yield` body the analysis siblings use and the one-element tuple-return body the numerics/solver/graduation siblings use both satisfying it, never a narrowed `tuple[Receipt, ...]` annotation drifting from the port declaration — one `Receipt.of("compute.spatial", ("emitted", query_tag, facts))` row, the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit, whose `facts` spreads the `query`/`points`/`content_key.project("hex")` render plus the matched `SpatialEvidence.facts()` slots a study run records. `solve` is the `RuntimeRail[SpatialReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `solve` but the study spine harvesting the resolved `SpatialReceipt` contributor through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm — the same convention `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` hold, the receipt the contributor and the rail the boundary form. A reconstructed boundary graduates outward through `graduation/handoff.md#GRADUATION` on the `geometry` `HandoffAxis` case aligned to the scan companion at the wire, never crossing as a geometry-branch mesh.
- Packages: `scipy` (`spatial.cKDTree.query`/`query_ball_point`/`query_pairs`, `spatial.ConvexHull`, `spatial.Delaunay`, `spatial.Voronoi`, `spatial.SphericalVoronoi`, `spatial.HalfspaceIntersection`, `spatial.distance.cdist`/`pdist`/`squareform`, `spatial.transform.Rotation.align_vectors`, `spatial.procrustes` — every spelling catalogued in `compute/.api/scipy.md`'s `scipy.spatial` entrypoint table — and `optimize.linprog` for the halfspace cell's Chebyshev-centre interior point; each kernel binds `import scipy.spatial as sp` once at its head the way the `analysis/signal.md#DSP` and `analysis/transform.md#TRANSFORM` kernels bind `import scipy.signal as sig`/`import scipy.fft as fft`, so `resolve` stays a pure tag-dispatch with no `from scipy.spatial import` scattered across its arms), `numpy` (`asarray`, `einsum` the squared-distance/edge-basis contraction owner, `linalg.norm`, `linalg.solve`, `sort` the k-selection of the brute kNN floor, `sqrt`, `unique`, `concatenate`, `zeros`, `mean`, `count_nonzero`), `expression` (`tagged_union`/`tag`/`case` the `SpatialQuery` and `SpatialEvidence` ADTs, `Map` the `NEIGHBOUR_FLOOR` table), `msgspec` (`Struct(frozen=True)` the `SpatialReceipt`), `numerics/array.md#PAYLOAD` (`ArrayPayload.admit`/`ArraySource.Live`/`FiniteGate` admitting the point set and keying the `content_key`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor` from `runtime/receipts`, `ContentKey.project` carried by the admitted payload). The `runtime/observability/receipts#RECEIPT` `@receipted`/`Redaction` aspect is the study spine's harvest of the `Ok`-arm contributor, not an import this owner threads. `scipy.spatial` is not Array-API-aware (unlike the `scipy.fft` the sibling transform owner rides; the `analysis/signal.md#DSP` owner is itself numpy-bound because its `sosfiltfilt`/`welch`/`spectrogram`/`find_peaks`/`resample_poly` entrypoints are out-of-scope or skip-backend for jax/dask/torch in scipy 1.17), so the point set is the numpy `np.ndarray` the Qhull/KD-tree/BLAS backends bind, not the resolved `xp`.
- Growth: a new spatial query is one `SpatialQuery` case plus one `resolve` arm returning a `SpatialEvidence`; a new evidence shape is one `SpatialEvidence` case plus one `facts()` arm (`SpatialReceipt` carries the evidence whole, so the receipt needs no edit); a new distance metric is one `Metric` row; a new tessellation backend is one `Tessellation` row; a new degrading route is one `NEIGHBOUR_FLOOR` table entry; zero new owner surface, never a per-query method family and never a re-implementation of the geometry-branch mesh surface.

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
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # the companion-gated KD-tree the `_proximity` reduction binds; declared here so the
    # reduction signature names the real carrier rather than degrading to a bare `object`.
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
        # the identity buffer the ArrayPayload admits and the ContentKey seeds, recovered from every
        # case so admission and identity stay one path; the two-set routes (cross-distance, align)
        # stack both operands so both coordinate buffers seed the key and a shared-`left` query with
        # a distinct `right`/`target` never collides, the concatenated-`tobytes` key
        # `numerics/statistics.md#STAT_CONTENT_KEY` holds over its one-or-two sample buffer. This is
        # the *identity* operand, never the receipt's point count — `cardinality` carries that.
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
        # the receipt's reference-set count — the leading coordinate buffer's row count, distinct
        # from the `points` identity buffer that concatenates both operands on the two-set routes.
        # `query.points.shape[0]` would report `len(pts) + len(queries)` for the
        # `neighbours`/`radius`/`align`/cross-`distances` routes, conflating the query set into the
        # reference count; the single-buffer routes already return that buffer through the `_` arm.
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

    def resolve(self) -> SpatialEvidence:
        match self:
            case SpatialQuery(tag="neighbours", neighbours=(pts, qs, k)):
                return _proximity(
                    "neighbours", pts, qs, float(k), lambda tree: _knn_distances(np.asarray(tree.query(qs, k=k, workers=-1)[0], dtype=float))
                )
            case SpatialQuery(tag="radius", radius=(pts, qs, r)):
                return _proximity(
                    "radius", pts, qs, r, lambda tree: SpatialEvidence.Proximity(int(sum(len(hit) for hit in tree.query_ball_point(qs, r=r))), 0.0, r)
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

# the data-driven numpy proximity floor: the `neighbours`/`radius` ImportError arm reads its
# row and folds the same Proximity evidence the cKDTree path produces, so the floor is one
# table membership rather than two try/except blocks each re-deriving a pairwise distance.
# A tag absent from this table (hull/triangulate/tessellate/distances/pairs/align) has no
# floor — Qhull, the BLAS distance kernel, and the rotation SVD are the gated capability.
NEIGHBOUR_FLOOR: Final[Map[Tag, NeighbourReduction]] = Map.of_seq([
    ("neighbours", lambda pts, qs, k: _floor_knn(pts, qs, int(k))),
    ("radius", lambda pts, qs, r: _floor_radius(pts, qs, r)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _proximity(tag: Tag, pts: np.ndarray, qs: np.ndarray, scale: float, reduce: "KdReduction") -> SpatialEvidence:
    # one symmetric fold for both KD-tree proximity routes: the `cKDTree` reduction the arm passes
    # and the `NEIGHBOUR_FLOOR` row both terminate in `Proximity`, so the body carries no per-tag
    # ternary — run the scipy reduction, or fall to this tag's floor row when the package is absent so
    try:
        import scipy.spatial as sp

        return reduce(sp.cKDTree(pts))
    except ImportError:
        return NEIGHBOUR_FLOOR[tag](pts, qs, scale)


def _pairwise_sq(pts: np.ndarray, qs: np.ndarray) -> np.ndarray:
    # the one squared-distance kernel both floor rows read: the `(q, n)` cross block contracts
    # through `einsum` (the catalogued reduction owner) rather than `linalg.norm`, so neither floor
    # row re-derives the broadcast difference — the kNN floor sorts+`sqrt`s it, the radius floor
    # thresholds it against `r**2` with no per-pair `sqrt`.
    diff = qs[:, None, :] - pts[None, :, :]
    return np.einsum("qnd,qnd->qn", diff, diff)


def _knn_distances(distances: np.ndarray) -> SpatialEvidence:
    # the shared kNN reduction the scipy `query` distances and the floor's sorted block both fold:
    # the mean over the k-nearest distance array is the one `Proximity` read, so the cKDTree path
    # and the brute floor terminate identically rather than each minting its own mean.
    return SpatialEvidence.Proximity(int(distances.size), float(np.mean(distances)))


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

    # `measure` is the triangulated point cardinality, distinct from the simplex `cardinality`:
    # Delaunay carries no `.volume` the way ConvexHull does, so the count read off the admitted
    # operand keeps the Complex measure a real fact rather than a simplex-count echo.
    tri = sp.Delaunay(pts)
    return SpatialEvidence.Complex("delaunay", int(tri.simplices.shape[0]), float(pts.shape[0]))


def _distances(left: np.ndarray, right: np.ndarray | None, metric: Metric) -> SpatialEvidence:
    import scipy.spatial as sp

    # self-distance (`right is None`) reduces the condensed vector directly — never a dense
    # squareform materialized only to mean over it; cross-distance reads the full cdist matrix.
    # The `cardinality` slot is the pair count; `kind` discriminates that it is a distance summary.
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
            # the operand is the `(N, d+1)` halfspace stack `[A | b]` (`A·x + b <= 0`), not a point
            # set; `HalfspaceIntersection` requires a strictly feasible interior point, so the
            # Chebyshev centre is read off the catalogued `linprog` (maximise the inscribed radius
            # `x* = argmax r s.t. A·x + ||A_i|| r <= -b`) rather than an `np.zeros` that Qhull
            # rejects as infeasible for any stack not straddling the origin.
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


def solve(query: SpatialQuery) -> "RuntimeRail[SpatialReceipt]":
    # `points` is the concatenated identity buffer (so a shared-`left` query keys distinctly);
    # `cardinality` is the reference-set count the receipt records, never the conflated
    # `len(pts) + len(queries)` sum the concatenated buffer's `shape[0]` carries on a two-set route.
    return ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT).bind(
        lambda payload: boundary(
            f"spatial.{query.tag}", lambda: SpatialReceipt.of(query.tag, query.cardinality, payload.content_key, query.resolve())
        )
    )
```

## [03]-[RESEARCH]

- [SPATIAL_EVIDENCE]: the terminal `SpatialEvidence` `@tagged_union` over `Proximity`/`Complex`/`Boundary`/`Alignment` collapses what a flat `SpatialReceipt` of `(query, count, measure, content_key)` would have stored with the `measure` column overloaded across hull volume, Delaunay simplex count, Voronoi region count, and procrustes disparity into one discriminated outcome that `SpatialReceipt(query, points, content_key, evidence)` carries whole, the evidence owning its own `facts()` slot projection so each shape names its real vocabulary, mirroring `analysis/transform.md#TRANSFORM`'s `TransformEvidence.facts()` carry, `analysis/symbolic.md#OP`'s `Outcome` union, and `analysis/signal.md#DSP`'s `Spectral`/`Multiresolution`/`Scale`/`Packet` collapse; the `Alignment` case carries the Kabsch fit RMSD (`align_vectors` returns the root-sum-square deviation the fold normalizes by `sqrt(n)` to a sample-independent error) and the procrustes disparity, so the rotation algebra is first-class evidence rather than smuggled through a shared scalar, and a new outcome is one `SpatialEvidence` case plus one `facts()` arm rather than a new nullable field on a fat struct or a re-derived `SpatialReceipt.of` column.
- [POINT_CONTENT_KEY]: `solve` admits the point set through `ArrayPayload.admit(ArraySource.Live(query.points), (), FiniteGate.REJECT)` from `numerics/array.md#PAYLOAD` and binds the resolved payload into the `boundary` thunk, so a non-finite point set returns the admission fault rather than producing a degenerate hull or a NaN-poisoned distance, the finite gate and identity resolve their namespace once inside admission through `array_namespace`, and the `ArrayPayload.content_key` keys the `SpatialReceipt` through `ContentIdentity` the way `numerics/statistics.md#STAT_CONTENT_KEY` keys its sample buffer — a repeated query on identical points a cache hit by reference. The `FiniteGate.REJECT` row forbids any non-finite (NaN or ±inf) at admission. The admission resolves the finite gate and `content_key` over the `query.points` buffer; the numpy-bound `scipy.spatial` kernels bind the per-case coordinate arrays (`pts`/`qs`/`left`/`right`/`source`/`target`) directly off the union — the admitted buffer is the finite/identity witness, not the compute operand, since `scipy.spatial` is not Array-API-aware and the two-set routes need the unconcatenated buffers (`cKDTree(pts)` binds the reference set, not the `[pts, qs]` identity concatenation). The owner never re-rolls the `array_namespace` dispatch, the `FiniteGate` masked reduction, or the `ascontiguousarray(...).tobytes()` canonical-buffer hashing the payload owner holds; `SpatialQuery.points` concatenates both operands on a two-set route so a shared-`left` query keys distinctly, while `SpatialQuery.cardinality` reads the leading buffer's row count for the receipt — admission, identity, and count stay one path across the union without conflating the two-set identity buffer into the point count.
