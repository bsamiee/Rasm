# [PY_GEOMETRY_MESH_QUALITY]

Mesh-topology conditioning and metrology over an in-memory triangulation — the decimation, subdivision, smoothing, and metric primitive the tessellation, scan-reconstruction, and step hops compose to coarsen, refine, denoise, and grade a built surface before it crosses a downstream rail. `MeshQualityOp` is one tagged union discriminating by operation kind, `apply` is one polymorphic entrypoint folding a single op or a batch sequence through one rail, and `MeshQualityResult` is one union carrier whose case mirrors the op case so a decimated mesh, a subdivided mesh, a smoothed mesh, and a metric record are four arms of one result owner rather than four parallel result types. The conditioning arms (`Decimate`/`Subdivide`/`Smooth`) each carry an algorithm row — the decimation target, the subdivision edge ceiling and iteration count, and the `SmoothKind` filter selector with its `factor`/`iterations` band — so the smoothing family is one parameterized row, never three Taubin/Laplacian/Humphrey entrypoints; the `Metrics` arm folds the cached `trimesh.Trimesh` validity, mass, and topology axes plus per-cell `numpy` shape statistics into one `QualityMetrics` value object carrying the aspect-ratio and equiangular-skewness five-number distributions, the angle-defect mean/std, and the scalar manifold-edge counts and genus, never a metric-per-method family. The spine is `trimesh` plus `numpy`, composed at depth: the conditioning arms weave `simplify_quadric_decimation`/`subdivide_to_size`/`smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` over the in-memory body, and the metric arm weaves the lazily-cached `area`/`volume`/`is_watertight`/`is_winding_consistent`/`vertex_defects` axes plus the exact per-edge incidence and Euler `E` from one sorted half-edge fold over `mesh.faces` with `numpy` `linalg.norm`/`sort`/`unique`/`clip`/`round`/`mean`/`std` reductions into one statistical receipt — never a single flat call. The `manifold3d` exact-topology surface is the one worker-enrichment tier on the worker lane, and it is stacked — not a lone `genus()` call: the offloaded `_topology_kernel` builds its `Manifold` through repair's PUBLIC `to_manifold` kernel (repair is the chartered `manifold3d` owner — no re-spelled `Mesh`/`Mesh64` selection here), reads its `status()` (the catalogue law — `Manifold(mesh)` sets a non-`NoError` status rather than raising on a non-2-manifold soup, so a degenerate surface rails through `QualityFault` rather than yielding a phantom genus), and weaves `genus`/`num_edge`/`num_tri`/`num_vert`/`volume`/`surface_area` into one `ExactTopology` value object that sharpens the genus, the edge/face/vertex counts, AND supersedes the `trimesh` `area`/`volume` mass with the exact kernel measure in a single fold; because a live `Manifold` cannot cross the no-pickle PEP 734 boundary, the capsule caches the returned `ExactTopology` VALUE across metric passes rather than a live `_solid`, so the spine's Euler-characteristic genus and `trimesh`-derived counts are replaced wholesale by exact integers when the tier resolves, never re-built per pass. The spine derives genus from the Euler characteristic over the half-edge fold's own `unique_edges`/`faces`/`vertices` count, resolved per interpreter through `QualityBackend` (`SPINE`/`MANIFOLD3D`), never a second metric owner nor a redundant `edges_unique` read. The `closure_fold` is this owner's PUBLIC exact-closure truth — the one watertight/euler/volume/area/components fold the `scan/reconstruction` and `scan/deviation` consumers compose downward, its component count a `QualityBackend` row (`manifold3d` `decompose()` when the tier resolves, `trimesh.body_count` the `SPINE` fall-through), never a per-consumer re-computation. This owner is the pure kernel conforming structurally to `ReceiptContributor`: it conditions and grades an in-memory `trimesh.Trimesh` and returns in-memory geometry plus typed metrics across the wire — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and render-time decimation is the artifacts figures owner, so the geometry shed never writes a mesh file nor coarsens for display.

## [01]-[INDEX]

- [01]-[QUALITY]: the decimate, subdivide, smooth, and metrics operations under one tagged union over the `trimesh`/`numpy` spine, woven from stacked `.api` members per arm, with the `manifold3d` exact-topology enrichment tier (genus + edge/tri/vert counts + kernel mass superseding the trimesh measure, folded into one cached `ExactTopology`) resolved per interpreter through the two-tier `QualityBackend`, returning one unified `MeshQualityResult` union and contributing one typed `MeshQualityReceipt`.

## [02]-[QUALITY]

- Owner: `MeshQuality` — the boundary capsule binding the in-memory `trimesh.Trimesh` to the four conditioning/metric arms, dispatching every op kind through the tier-aware `_route` (the CPU-bound quadric collapse and `MANIFOLD3D`-metrics arms offloaded onto the lane, the in-place spine arms run under `boundary`) plus the total `_spine`/`_smooth` matches, folding every resolved `Outcome` into the held `MeshQualityReceipt` through the one `_arm` cross-cut method (the mirror of the `mesh/spatial.md#SPATIAL` sibling's `_fold`, fed by both the offloaded `.map` and the synchronous `boundary` thunk), and conforming structurally to the runtime-checkable `ReceiptContributor` Protocol so the last op's facts contribute one `MeshQualityReceipt` row; `MeshQualityOp` the tagged union discriminating by operation so decimate/subdivide/smooth/metrics are four cases of one request rather than four entrypoints; `SmoothKind` the closed three-row `StrEnum` (`TAUBIN`/`LAPLACIAN`/`HUMPHREY`) so the smoothing filter family is one row on the `Smooth` case, never three sibling smoothing methods; `MeshQualityResult` the union carrier whose case mirrors the op case (`Decimate`/`Subdivide`/`Smooth` carrying the conditioned `trimesh.Trimesh`, `Metrics` carrying the `QualityMetrics` value object); `QualityBackend` the closed two-tier `StrEnum` selecting the always-available `SPINE` Euler-characteristic genus versus the `MANIFOLD3D` exact integer topology, with `QualityBackend.resolve` mapping the live interpreter to `MANIFOLD3D` only when its package loads and to `SPINE` otherwise; `ExactTopology` the frozen value object the `MANIFOLD3D` tier folds in one pass — exact `genus`/`edge_count`/`face_count`/`vertex_count` plus the kernel `volume`/`area` mass — inside the offloaded `_topology_kernel` over a `Manifold` built once per hop, the picklable `ExactTopology` VALUE cached on the capsule (`_exact`) so the metric fold overrides the spine's Euler genus and `trimesh` counts wholesale rather than reading one `genus()` and re-deriving the rest; `QualityFault` the typed `@tagged_union(Exception)` the `_topology_kernel` raises INTO the lane's `async_boundary` when the `Manifold` ingest `status()` is not `NoError`, so a degenerate-soup rejection converts through the one `BoundaryFault` taxonomy rather than a domain `raise` nor a trusted phantom solid; `QualityMetrics` the frozen value object carrying the watertight/winding verdict, the area/volume mass, the face/vertex/edge counts, the non-manifold and boundary edge counts, the genus, the Euler characteristic, and the five-number aspect-ratio and equiangular-skewness distributions (min/q1/median/q3/max) plus the angle-defect mean/std, with a `worst` projection folding the distribution tails into the single conditioning verdict the consumer reads; `Outcome` the one struct each arm returns carrying its `MeshQualityResult` plus the receipt facts (the operation tag, the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, the genus, and the backend tier) so the `_arm` cross-cut folds one `MeshQualityReceipt` from a uniform payload instead of a per-arm receipt build; `MeshQualityReceipt` the typed receipt carrying the op tag, the backend tier, the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, the genus, and the conditioning verdict.
- Cases: `MeshQualityOp` cases `Decimate(target_faces)` (the quadric edge-collapse simplification through `simplify_quadric_decimation(face_count)` toward the target triangle budget, the topology coarsening the tessellation hop runs before a downstream solid op — distinct from render decimation, which is the artifacts owner), `Subdivide(max_edge, iterations)` (the edge-length-bounded refinement through `remesh.subdivide_to_size(vertices, faces, max_edge)` toward a maximum edge length, the topology densification the scan-reconstruction hop runs before a curvature-sensitive metric pass), `Smooth(kind, iterations, factor)` (the Laplacian-family denoising through the `SmoothKind`-selected `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` over the in-place body, the noise conditioning the scan-reconstruction hop runs before a deviation pass, the filter a `SmoothKind` row not a parallel entrypoint), and `Metrics()` (the full validity/mass/topology/shape grade folding the cached `trimesh` axes plus the per-cell `numpy` aspect-ratio and equiangular-skewness statistics into one `QualityMetrics`, the gate the daemon and the clash/deviation hops read before trusting a surface) — matched by `match`/`assert_never`, each binding the `.api` members that own the kind. Inputs are face budgets, edge ceilings, iteration counts, and `SmoothKind` rows; outputs are the conditioned `trimesh.Trimesh` or the `QualityMetrics` record carried in the mirrored `MeshQualityResult` arm. No mesh-file format axis lives here — the surface arrives as an in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam and the conditioned body rides back the same way.
- Entry: `apply` is the one polymorphic `async` entrypoint discriminating a single `MeshQualityOp` or a batch `Sequence[MeshQualityOp]` by total `match` (a lone `MeshQualityOp` as one case, a `Sequence` as the other — never an `isinstance` ladder), with `@overload` arms keyed on the input shape so a caller narrows on what it passes rather than re-matching the widened return. A single op awaits one tier-aware `_route`; a batch awaits each `_route` IN ORDER — a conditioning pipeline is sequential because a `Smooth` mutates the shared mesh in place and a downstream `Metrics` reads it, so the routes never fan out concurrently — then folds the resolved `Block` through `traversed(rails, by=Disposition.ABORT)` (the runtime `reliability/faults#FAULT` owner's one disposition-keyed fold, the `ABORT` row short-circuiting to the first faulted op since a downstream conditioning pass cannot trust a faulted upstream coarsening, never a junior `accumulate` boolean) into one `RuntimeRail[Block[MeshQualityResult]]`, never a second batch method nor a quadratic singleton-append. `_route` is the one tier-aware fence: the CPU-bound quadric collapse offloads `_decimate_kernel` onto the lane subinterpreter (`mesh.simplify_quadric_decimation(face_count=target_faces)` keyword-bound — positional arg 0 is `percent`, a fraction, never the face budget — the realized face count read off the returned `Trimesh` rather than asserted); the `MANIFOLD3D`-tier `Metrics` arm offloads `_topology_kernel` for the exact `ExactTopology` then folds the numpy-spine metrics in-process around it, while the `SPINE`-tier `Metrics`, `Subdivide`, and `Smooth` arms run synchronously under one `boundary`. The lane converts a `BrokenWorkerInterpreter`/deadline `TimeoutError` and the `_topology_kernel`'s `QualityFault` through its own `async_boundary` onto the rail, exactly the offload-aware parity the `mesh/repair.md#MESH` and `mesh/spatial.md#SPATIAL` siblings hold. The `Subdivide` arm folds `remesh.subdivide_to_size(mesh.vertices, mesh.faces, max_edge)` over the supplied iteration count, rebuilding a `Trimesh` from the returned vertices/faces; the `Smooth` arm selects the `SmoothKind`-keyed `smoothing.filter_*` filter binding its defining parameters from the one `factor` row (`filter_taubin`'s `lamb`/`nu` shrink-free band-pass pair derived as `lamb=factor`, `nu=factor/(1 − _TAUBIN_K·factor)` — `nu` a POSITIVE magnitude `filter_taubin` SUBTRACTS internally on its odd dilation passes, solved from the reciprocal pass-band so `1/lamb − 1/nu == _TAUBIN_K ∈ (0, 0.1)` holds for every `lamb`, never a negative `nu` that double-negates into a second shrink nor a fixed additive offset that breaks the band at small `lamb`, `filter_laplacian`'s `lamb=factor`, `filter_humphrey`'s `alpha=factor`), runs it in place over the iteration count, and reads `is_watertight`/`is_winding_consistent` after; the `Metrics` arm folds the cached `area`/`volume`/`is_watertight`/`is_winding_consistent` validity-and-mass axes, derives the per-cell aspect-ratio and equiangular-skewness distributions from the triangle edge vectors through `numpy` `linalg.norm`/`sort`/`clip`/`mean`/`std`, counts the boundary (incidence 1), interior-manifold (incidence 2), and non-manifold (incidence ≥3) edges by folding the sorted half-edge array off `mesh.faces` through `numpy.unique(..., axis=0, return_counts=True)` so the per-edge incidence is the exact group-count rather than a positional alignment of `edges_unique` against `face_adjacency`, and supersedes the spine genus/counts/mass with the offloaded `ExactTopology` value when the `MANIFOLD3D` tier resolved — that value summing `manifold3d.Manifold.genus()` over the `decompose()` components (genus is defined per connected body) and reading `num_edge()`/`num_tri()`/`num_vert()`/`volume()`/`surface_area()` off the whole solid in one pass — while `SPINE` derives genus from the Euler characteristic `V − E + F = 2 − 2g` over `len(mesh.vertices)`/the half-edge fold's own `len(unique_edges)`/`len(mesh.faces)`, the same unique-edge array the incidence fold groups so `E` has one source of truth. The cross-cutting concern — folding each arm's resolved `Outcome` into one `MeshQualityReceipt` row with the backend tier and the before/after face count — rides the one `_arm` method the offloaded `.map` and the in-process `boundary` thunk both feed (the `mesh/spatial.md#SPATIAL` sibling's `_fold` convention, not a decorator indirection that would have to bind `self`), so a new case writes only the geometry body producing an `Outcome`. The `_cell_shape` per-triangle aspect/skew kernel and the `_quantiles` five-number fold are the shared `numpy` helpers the `Metrics` arm composes into the held `QualityMetrics` distributions; `QualityMetrics.worst` then reads the precomputed tail off those held tuples (`aspect_ratio[4]`/`skewness[4]`) — the conditioning arms carry no per-cell distribution, so they emit zeroed shape facts and only the `Metrics` arm populates the tail verdict.
- Auto: `Trimesh.simplify_quadric_decimation(face_count)` (`trimesh.md` smoothing/remesh entrypoint row [14]) quadric-collapses toward the target triangle budget and returns a new `Trimesh`, the topology coarsening distinct from the render-time decimation the artifacts owner runs; `remesh.subdivide_to_size(vertices, faces, max_edge)` (row [15]) refines every edge past the ceiling and returns new vertices/faces; `smoothing.filter_taubin`/`filter_laplacian`/`filter_humphrey` (rows [10]/[11]/[12]) mutate the mesh in place toward a denoised surface, Taubin shrink-free, the filter a `SmoothKind` row; `Trimesh.area`/`volume` (`trimesh.md` cached-property row [01]), `is_watertight`/`is_winding_consistent` (manifold-validity row [04], the orientation verdict that subsumes a separate normal-consistency axis) and `vertex_defects` (curvature row [12], the per-vertex angle defect the `angle_defect` mean/std fold reads) are lazily-cached properties read once per metric pass and recomputed only on geometry change, while both the per-edge boundary/manifold/non-manifold incidence AND the Euler `E` count derive from the one sorted half-edge fold off `mesh.faces` through `numpy.unique(..., axis=0, return_counts=True)`, so the unique-edge array is its own `E` source rather than a second `edges_unique` cached-property read; on the `MANIFOLD3D` tier the offloaded `_topology_kernel` builds a `Manifold` from the surface's `Mesh`/`Mesh64` and gates `status() == manifold3d.Error.NoError` (`manifold3d.md` line 45/156 — ingest sets a non-`NoError` status rather than raising on a non-2-manifold soup, so the kernel raises `QualityFault(rejected=status.name)` rather than reading a phantom genus), then sums `manifold3d.Manifold.genus()` (`manifold3d.md` topology row [04], defined per connected body) over `Manifold.decompose()` (transform row [07], the topologically-disconnected component split) and reads `num_vert()`/`num_edge()`/`num_tri()` (query row [07]) plus `volume()`/`surface_area()` (measurement row [05]) off the whole solid in the same fold, the surface entering as `manifold3d.Mesh` (32-bit `tri_verts`) or `Mesh64` (`manifold3d.md` type rows [03]/[04]) selected by vertex count so a surface past the `uint32` ceiling never silently truncates its triangle indices — the one build powers the exact genus, the exact edge/face/vertex counts, and the kernel mass that supersedes the `trimesh` measure, never one `genus()` call discarding the rest of the solid nor a bare `genus()` read on an undecomposed multi-body manifold. The kernel returns the picklable `ExactTopology` VALUE the lane carries back over the no-pickle PEP 734 boundary, and the capsule caches that value (`_exact`) across metric passes rather than the live `Manifold` — a live solid cannot cross the subinterpreter boundary, so the cross-pass reuse the `mesh/spatial.md#SPATIAL` sibling names as this owner's is the result cache, never a held `_solid`.
- Receipt: `MeshQuality` conforms STRUCTURALLY to the runtime `observability/receipts#RECEIPT` runtime-checkable `ReceiptContributor` Protocol (never subclassing it — the base adds nothing) — the one `_arm` cross-cut method folds each arm's resolved `Outcome` into the held `MeshQualityReceipt`, and `contribute` YIELDS one `Receipt.of("mesh.quality", (phase, r.op, facts))` row (never `return`s a bare `Receipt` — the Protocol port and the `@receipted` aspect's `_stream` normalizer both require the `Iterable[Receipt]` stream the `mesh/repair.md#MESH` and `mesh/spatial.md#SPATIAL` siblings hold) through the owner's shape-polymorphic `(Phase, subject, facts)` factory (the op tag the subject), the `facts` carrying the `QualityBackend.value` tier string plus the before/after face count, the watertight verdict, the worst aspect-ratio and skewness, and the genus as native scalars the receipt owner's `enc_hook=repr` renderer serializes without a `str()` coerce; the op tag rides as the receipt subject, never duplicated into a `facts` slot. A stateful capsule accumulating `_last` across a batch, the daemon or serve path harvests the contributor stream under the `@receipted` aspect once after the `apply` fold rather than an inline emit threaded through each arm. A `Metrics` pass over a non-watertight or non-winding-consistent surface keys `phase="admitted"` (the `Phase` literal the receipt owner admits) so the row is a caveat flagging the unreliable genus/volume rather than asserting a closed-solid grade, while a clean conditioning or grade keys `phase="emitted"`. The quality pass produces no `GraduationReceipt` subject — it is the read-and-condition primitive the tessellation, deviation, and reconstruction hops consume, and the graduating subject (`reconstructed-mesh`, `mesh-algebra`) belongs to the `mesh/repair` and reconstruction owners that emit the conditioned solid; the geometry-minted `rasm.geometry.graduation` `GeometrySubject` member is never minted here.
- Packages: `trimesh` (`Trimesh`/`Trimesh.area`/`Trimesh.volume`/`Trimesh.is_watertight`/`Trimesh.is_winding_consistent`/`Trimesh.vertex_defects`/`Trimesh.simplify_quadric_decimation`/`remesh.subdivide_to_size`/`smoothing.filter_taubin`/`smoothing.filter_laplacian`/`smoothing.filter_humphrey`), `numpy` (`ndarray`/`array`/`asarray`/`sort`/`stack`/`concatenate`/`clip`/`round`/`mean`/`std`/`min`/`max`/`sum`/`unique`/`iinfo`/`linalg.norm` over the per-cell `Nx3` edge-vector shapes, the sorted half-edge incidence-and-`E` fold, and the vectorized five-number quantile gather), `manifold3d` (the exact-topology tier — `Manifold`/`Mesh`/`Mesh64`/`Manifold.status`/`Error`/`Manifold.decompose`/`Manifold.genus`/`Manifold.num_edge`/`Manifold.num_tri`/`Manifold.num_vert`/`Manifold.volume`/`Manifold.surface_area`, folded into one `ExactTopology` inside the offloaded `_topology_kernel` reached only on the `MANIFOLD3D` backend row, the ingest `status()` checked against `Error.NoError`), `expression` (`tagged_union`/`tag`/`case` the `MeshQualityOp`/`MeshQualityResult`/`QualityFault` discriminated unions, `Ok` the cache-hit and no-tier `RuntimeRail` short-circuit, `Block`/`Block.of_seq` the sequential batch rail carrier), `msgspec` (`Struct(frozen=True)` the GC-tracked `QualityMetrics`/`Outcome` carriers, `Struct(frozen=True, gc=False)` the leaf-scalar `ExactTopology`/`MeshQualityReceipt`), runtime `reliability/faults#FAULT` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`), `observability/receipts#RECEIPT` (`Phase`/`Receipt`/`ReceiptContributor`), `execution/lanes#LANE` (`LanePolicy`/`LanePolicy.offload` the subinterpreter hop the quadric-collapse / exact-topology kernels ride).
- Boundary: no robust watertight repair, hole-fill, or boolean CSG (that is `mesh/repair`); no proximity, ray, contains, or sampling query (that is `mesh/spatial`); no point-cloud registration or reconstruction (that is `scan/registration`+`scan/reconstruction`); the `manifold3d` exact-genus kernel is reached only through the `MANIFOLD3D` `QualityBackend` row, never a second direct topology owner; render-time decimation for display is NOT this owner — the artifacts figures owner coarsens for a viewport, while this owner coarsens mesh topology for a downstream geometry op, so a render-LOD or display-budget arm here is a deleted form that trespasses the `mesh ⇄ artifacts/scene/export` boundary; mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file. A `decimate`/`subdivide`/`smooth`/`metrics` method family over the `MeshQualityOp` row, three parallel `filter_taubin`/`filter_laplacian`/`filter_humphrey` smoothing entrypoints over the `SmoothKind` row, a metric-per-method family (`aspect_ratio`/`skewness`/`genus`/`manifold_edges` as separate methods) over the one `QualityMetrics` fold, four parallel `DecimateResult`/`SubdivideResult`/`SmoothResult`/`MetricsResult` structs, a hand-rolled quadric-collapse or Loop-subdivision kernel where `trimesh.simplify_quadric_decimation`/`remesh.subdivide_to_size` are admitted, a POSITIONAL `simplify_quadric_decimation(target_faces)` that binds the face budget to the leading `percent` fraction rather than the keyword `face_count=`, a per-arm receipt build duplicating the one `_arm` cross-cut, a module-level `@arm` decorator taking `(self, op, out)` that the call sites reference as `self._arm` yet never bind to an instance (the phantom-aspect dead-seam the `mesh/spatial.md#SPATIAL` sibling's plain `_fold` method avoids), treating the `manifold3d` exact genus as the spine rather than an enrichment tier over the Euler-characteristic default, reading a lone `Manifold.genus()` and discarding the built solid instead of folding the one `ExactTopology` (exact counts + kernel mass superseding the trimesh measure) off the kernel's `Manifold`, reading `Manifold.genus()` on an undecomposed multi-body solid rather than summing it over `decompose()`, trusting the `Manifold` ingest without gating `status() == Error.NoError` so a non-2-manifold soup yields a phantom genus/mass rather than railing through `QualityFault`, caching a live `_solid` `Manifold` on the capsule and reusing it across offload hops where a live solid cannot cross the no-pickle PEP 734 boundary (the cross-pass reuse is the picklable `ExactTopology` VALUE on `_exact`, never the solid), a synchronous `apply` blocking the companion event loop on the CPU-bound quadric collapse or exact-topology fold where the `LanePolicy.offload` subinterpreter hop isolates it, a `lane` accepted in `__init__` yet never composed (the dead-seam form the offload-aware `async apply` resolves by actually awaiting `self._lane.offload(...)`), an `isinstance(op, MeshQualityOp)` dispatch where the polymorphic entry is one total `match` over the single-or-`Sequence` shape, a domain `raise ValueError` on a rejected soup where the typed `QualityFault` converts through the lane's `async_boundary`, dropping the `Smooth` `factor` so Taubin runs without its band-pass `lamb`/`nu` pair OR feeding `filter_taubin` a NEGATIVE `nu` (which trimesh subtracts on its odd dilation pass, double-negating it into a second shrink) rather than the positive in-`[0,1]` dilation magnitude slightly above `lamb`, asserting a closed-solid genus/volume over a non-watertight surface instead of admitting the caveat, a float32 `vert_properties` on the `Mesh64` branch where the f64 carrier (selected past the `uint32` vertex ceiling) must carry f64 positions, a `traversed(accumulate=...)` boolean flag where the runtime `Disposition` row owns the batch strategy, a `QualityMetrics.worst` that re-runs `_cell_shape`/`_quantiles` rather than reading the precomputed distribution tail, a named `lambda` per quantile fraction where one vectorized `_QUANTILE_FRACTIONS` gather owns the five-number fold, a second `Trimesh.edges_unique` cached-property read for the Euler `E` where the one half-edge `np.unique` fold already yields the unique-edge count, a `Receipt.of(phase, owner, subject, facts)` positional arity where the owner mints through the one `Receipt.of(owner, (phase, subject, facts))` shape-polymorphic factory, a `str()`-coerced `dict[str, str]` facts map pre-formatting the native scalars the receipt owner's `enc_hook=repr` renderer serializes, a `contribute` returning a bare `Receipt` rather than yielding the `Iterable[Receipt]` stream the Protocol declares, a `gc=False` on `QualityMetrics`/`Outcome` where the tuple distributions and the nested `MeshQualityResult` keep the struct GC-tracked, a render-LOD/display-budget decimation arm trespassing the artifacts boundary, and ANY `MeshFormat`/`Codec`/`load`/`export` arm re-deriving the `MeshPayload` seam are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import Ok, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.geometry.mesh.repair import to_manifold
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type OpKind = Literal["decimate", "subdivide", "smooth", "metrics"]


class SmoothKind(StrEnum):  # the smoothing filter family is one row on the Smooth case, never three parallel entrypoints
    TAUBIN = "taubin"        # shrink-free band-pass Laplacian
    LAPLACIAN = "laplacian"  # uniform-weight Laplacian
    HUMPHREY = "humphrey"    # Humphrey classes anti-shrink


class QualityBackend(StrEnum):  # only the two tiers this owner dispatches for exact-versus-Euler genus
    SPINE = "spine"            # numpy Euler characteristic V−E+F=2−2g over the half-edge fold's unique-edge count, always-available runtime
    MANIFOLD3D = "manifold3d"  # worker-enrichment companion: the exact integer genus/num_edge surface

    @staticmethod
    def resolve() -> "QualityBackend":  # the runtime venv carries no manifold3d admission, so exact genus resolves only on the worker lane
        return QualityBackend.MANIFOLD3D if find_spec("manifold3d") is not None else QualityBackend.SPINE


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUANTILE_FRACTIONS: Final[np.ndarray] = np.array([0.0, 0.25, 0.5, 0.75, 1.0])  # five-number summary positions, no uncatalogued np.percentile
_TAUBIN_K: Final[float] = 0.05  # the Taubin pass-band target `1/lamb - 1/nu`; nu = lamb/(1 - K*lamb) holds it constant in (0, 0.1) across every lamb, unlike a fixed additive nu offset


# --- [MODELS] ---------------------------------------------------------------------------


class ExactTopology(Struct, frozen=True, gc=False):  # leaf int/float struct, gc-untracked; the MANIFOLD3D tier's one fold over a single cached Manifold, never a lone genus() read
    genus: int
    vertex_count: int
    edge_count: int
    face_count: int
    components: int  # the decompose() body count — the exact arm of the component-count backend row
    volume: float
    area: float


class QualityMetrics(Struct, frozen=True):  # the one shape/validity/topology grade, never a metric-per-method family; holds tuple distributions so it stays GC-tracked
    watertight: bool
    winding_consistent: bool
    area: float
    volume: float
    vertex_count: int
    face_count: int
    edge_count: int
    boundary_edges: int
    nonmanifold_edges: int
    components: int  # backend row: manifold3d decompose() count when the tier resolves, trimesh body_count the CORE fall-through
    genus: int
    euler_characteristic: int
    aspect_ratio: tuple[float, float, float, float, float]   # five-number (min, q1, median, q3, max)
    skewness: tuple[float, float, float, float, float]        # equiangular skewness five-number summary
    angle_defect_mean: float  # mean per-vertex angle defect (Trimesh.vertex_defects), a curvature signal, never vertex valence
    angle_defect_std: float

    @property
    def worst(self) -> tuple[float, float]:  # the conditioning verdict the deviation/clash hops read: tail (max) aspect-ratio and skewness off the held distributions
        return (self.aspect_ratio[4], self.skewness[4])


class MeshQualityReceipt(Struct, frozen=True, gc=False):  # leaf scalar struct, gc-untracked on the receipt egress path
    op: OpKind
    backend: QualityBackend
    faces_before: int
    faces_after: int
    watertight: bool
    worst_aspect_ratio: float
    worst_skewness: float
    genus: int


@tagged_union(frozen=True)
class MeshQualityOp:
    tag: OpKind = tag()
    decimate: int = case()
    subdivide: tuple[float, int] = case()
    smooth: tuple[SmoothKind, int, float] = case()
    metrics: bool = case()

    @staticmethod
    def Decimate(target_faces: int) -> "MeshQualityOp":  # only the catalogued face_count argument; no uncatalogued aggression keyword
        return MeshQualityOp(decimate=target_faces)

    @staticmethod
    def Subdivide(max_edge: float, iterations: int = 1) -> "MeshQualityOp":
        return MeshQualityOp(subdivide=(max_edge, iterations))

    @staticmethod
    def Smooth(kind: SmoothKind = SmoothKind.TAUBIN, iterations: int = 10, factor: float = 0.5) -> "MeshQualityOp":
        return MeshQualityOp(smooth=(kind, iterations, factor))

    @staticmethod
    def Metrics() -> "MeshQualityOp":
        return MeshQualityOp(metrics=True)


@tagged_union(frozen=True)
class MeshQualityResult:
    tag: OpKind = tag()
    decimate: trimesh.Trimesh = case()
    subdivide: trimesh.Trimesh = case()
    smooth: trimesh.Trimesh = case()
    metrics: QualityMetrics = case()

    @staticmethod
    def Decimate(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(decimate=mesh)

    @staticmethod
    def Subdivide(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(subdivide=mesh)

    @staticmethod
    def Smooth(mesh: trimesh.Trimesh) -> "MeshQualityResult":
        return MeshQualityResult(smooth=mesh)

    @staticmethod
    def Metrics(metrics: QualityMetrics) -> "MeshQualityResult":
        return MeshQualityResult(metrics=metrics)


class Outcome(Struct, frozen=True):  # one arm's payload plus the receipt facts the `_arm` cross-cut folds, never a per-arm receipt build
    result: MeshQualityResult
    faces_before: int
    faces_after: int
    watertight: bool
    worst_aspect_ratio: float
    worst_skewness: float
    genus: int


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class QualityFault(Exception):  # raised INTO the lane's async_boundary so a status()-rejected soup converts through the one BoundaryFault taxonomy, never a domain raise ValueError
    tag: Literal["rejected"] = tag()
    rejected: str = case()  # the manifold3d Error status name the ingest gate read


# --- [OPERATIONS] -----------------------------------------------------------------------


def _quantiles(values: np.ndarray) -> tuple[float, float, float, float, float]:  # one vectorized gather over the sorted view, never a named lambda per fraction
    s = np.sort(values)
    if s.size == 0:
        return (0.0, 0.0, 0.0, 0.0, 0.0)
    idx = np.clip(np.round(_QUANTILE_FRACTIONS * (s.size - 1)).astype(np.intp), 0, s.size - 1)
    return tuple(s[idx].tolist())  # type: ignore[return-value]


def _cell_shape(mesh: trimesh.Trimesh) -> tuple[np.ndarray, np.ndarray]:  # per-triangle aspect ratio and equiangular skewness from edge vectors
    tris = np.asarray(mesh.vertices, dtype=np.float64)[np.asarray(mesh.faces)]
    e0, e1, e2 = tris[:, 1] - tris[:, 0], tris[:, 2] - tris[:, 1], tris[:, 0] - tris[:, 2]
    lengths = np.stack([np.linalg.norm(e0, axis=1), np.linalg.norm(e1, axis=1), np.linalg.norm(e2, axis=1)], axis=1)
    longest, shortest = lengths.max(axis=1), np.clip(lengths.min(axis=1), 1e-12, None)
    aspect = longest / shortest
    cos_a = np.clip(-np.sum(e2 * e0, axis=1) / (lengths[:, 2] * lengths[:, 0]), -1.0, 1.0)
    cos_b = np.clip(-np.sum(e0 * e1, axis=1) / (lengths[:, 0] * lengths[:, 1]), -1.0, 1.0)
    cos_c = np.clip(-np.sum(e1 * e2, axis=1) / (lengths[:, 1] * lengths[:, 2]), -1.0, 1.0)
    cosines = np.stack([cos_a, cos_b, cos_c], axis=1)
    skew = np.clip((cosines.min(axis=1) - 0.5) / -0.5, 0.0, 1.0)  # equiangular cell at cos(60°)=0.5 → 0 skew; degenerate → 1
    return aspect, skew


# the offloaded MANIFOLD3D-tier kernel: a module-level Callable the no-pickle PEP 734 subinterpreter receives over
# plain (mesh,) args (the lane never imports it). It composes repair's PUBLIC uint32-ceiling `to_manifold` build —
# repair is the chartered manifold3d owner, this page re-spells no Mesh/Mesh64 selection — gates status() (the .api
# law: `Manifold(mesh)` sets a non-NoError status rather than raising on a non-2-manifold soup), and returns the
# picklable ExactTopology VALUE — a live Manifold cannot cross the boundary, so the capsule caches the computed
# value across metric passes, never a live _solid, the cross-pass reuse the mesh/spatial sibling names as this owner's.
def _topology_kernel(mesh: trimesh.Trimesh) -> ExactTopology:
    import manifold3d

    solid = to_manifold(mesh)
    if solid.status() != manifold3d.Error.NoError:  # gate the ingest verdict; a non-2-manifold soup rails rather than yielding a phantom genus/mass
        raise QualityFault(rejected=solid.status().name)
    parts = solid.decompose()
    genus = sum(int(c.genus()) for c in parts)  # genus() is per connected component (.api row [04] "decompose first"); sum over disconnected bodies
    return ExactTopology(
        genus, solid.num_vert(), solid.num_edge(), solid.num_tri(), len(parts), float(solid.volume()), float(solid.surface_area())
    )


# the offloaded quadric-decimation kernel: the CPU-bound edge-collapse rides the lane subinterpreter, the conditioned
# Trimesh's vertex/face arrays the picklable wire back (the Growth bullet's offloaded conditioning pass).
def _decimate_kernel(mesh: trimesh.Trimesh, target_faces: int) -> trimesh.Trimesh:
    return mesh.simplify_quadric_decimation(face_count=target_faces)  # keyword: positional arg 0 is `percent`, not the face budget


# --- [SERVICES] -------------------------------------------------------------------------


class MeshQuality:  # conforms structurally to the runtime-checkable ReceiptContributor Protocol; subclassing the port is the deleted form
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, backend: QualityBackend | None = None) -> None:
        self._mesh = mesh
        self._lane = lane  # the per-subinterpreter offload seam the quadric-collapse / exact-topology kernels ride; the lane never imports the kernel
        self._backend = backend or QualityBackend.resolve()
        self._last: MeshQualityReceipt | None = None
        self._exact: ExactTopology | None = None  # picklable ExactTopology value cached across passes; a live Manifold cannot cross the no-pickle boundary

    @overload
    async def apply(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]": ...
    @overload
    async def apply(self, op: Sequence[MeshQualityOp]) -> "RuntimeRail[Block[MeshQualityResult]]": ...
    async def apply(self, op: MeshQualityOp | Sequence[MeshQualityOp]) -> "RuntimeRail[MeshQualityResult] | RuntimeRail[Block[MeshQualityResult]]":
        # arity is a property of the argument: a lone op awaits one tier-aware route, a batch awaits each route IN ORDER
        # then folds under one Disposition — a conditioning pipeline is sequential (a Smooth mutates the shared mesh in
        # place, a downstream Metrics reads it), so the routes never fan out concurrently, never an isinstance ladder.
        match op:
            case MeshQualityOp() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(rails, by=Disposition.ABORT)  # the batch aborts on the first faulted op; the runtime owns the strategy row, not a boolean flag

    async def _route(self, op: MeshQualityOp) -> "RuntimeRail[MeshQualityResult]":
        # the one tier-aware fence: the CPU-bound quadric collapse offloads its kernel onto the lane subinterpreter, the
        # in-place subdivide/smooth and the numpy-spine metrics run synchronously under `boundary`, and the MANIFOLD3D
        # Metrics arm offloads its exact-topology kernel then folds the numpy-spine metrics in-process around the value.
        match op:
            case MeshQualityOp(tag="decimate", decimate=target_faces):
                before = len(self._mesh.faces)
                offloaded = await self._lane.offload(_decimate_kernel, self._mesh, target_faces)  # async_boundary rails BrokenWorkerInterpreter/TimeoutError
                return offloaded.map(
                    lambda out: self._arm(op, Outcome(MeshQualityResult.Decimate(out), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)))
            case MeshQualityOp(tag="metrics"):
                # the exact-topology kernel rides the offload fence (its own async_boundary); the CPU-bound numpy
                # half-edge/cell-shape fold around it ALWAYS runs under `boundary` on both tiers, so a degenerate-mesh
                # numpy raise converts to a BoundaryFault rather than escaping the rail — never a bare `.map` fold.
                exact = await self._exact_topology() if self._backend is QualityBackend.MANIFOLD3D and bool(self._mesh.is_watertight) else Ok(None)
                return exact.bind(lambda e: boundary("mesh.quality.metrics", lambda: self._arm(op, self._metrics_outcome(e))))
            case _:
                return boundary(f"mesh.quality.{op.tag}", lambda: self._arm(op, self._spine(op)))

    async def _exact_topology(self) -> "RuntimeRail[ExactTopology]":  # offload the manifold3d build/fold once, then reuse the returned VALUE across passes
        if self._exact is not None:
            return Ok(self._exact)
        rail = await self._lane.offload(_topology_kernel, self._mesh)
        return rail.map(self._cache_exact)

    def _cache_exact(self, exact: ExactTopology) -> ExactTopology:
        self._exact = exact
        return exact

    def _arm(self, op: MeshQualityOp, out: Outcome) -> MeshQualityResult:  # receipt fold the `.map`+`boundary` feed; a new arm writes only the Outcome body
        self._last = MeshQualityReceipt(
            op.tag, self._backend, out.faces_before, out.faces_after,
            out.watertight, out.worst_aspect_ratio, out.worst_skewness, out.genus,
        )
        return out.result

    def contribute(self) -> Iterable[Receipt]:  # the ReceiptContributor port YIELDS the stream @receipted's _stream normalizes; never a bare Receipt return
        r = self._last or MeshQualityReceipt("metrics", self._backend, 0, 0, True, 0.0, 0.0, 0)
        phase: Phase = "emitted" if r.watertight else "admitted"
        facts: dict[str, object] = {  # native scalars; the receipts owner's enc_hook=repr renderer serializes without a str() coerce
            "backend": r.backend.value, "faces_before": r.faces_before,
            "faces_after": r.faces_after, "watertight": r.watertight,
            "worst_aspect_ratio": r.worst_aspect_ratio, "worst_skewness": r.worst_skewness,
            "genus": r.genus,
        }
        yield Receipt.of("mesh.quality", (phase, r.op, facts))  # the owner's shape-polymorphic (Phase, subject, facts) factory; subject is the op tag

    def _spine(self, op: MeshQualityOp) -> Outcome:  # the in-place conditioning arms only; decimate is offloaded and metrics has its own fenced arm in _route
        before = len(self._mesh.faces)
        match op:
            case MeshQualityOp(tag="subdivide", subdivide=(max_edge, iterations)):
                verts, faces = self._mesh.vertices, self._mesh.faces
                for _ in range(iterations):
                    verts, faces = trimesh.remesh.subdivide_to_size(verts, faces, max_edge)
                out = trimesh.Trimesh(vertices=verts, faces=faces, process=True)  # merge edge-split vertices so is_watertight reads true, parity mesh/repair
                return Outcome(MeshQualityResult.Subdivide(out), before, len(out.faces), bool(out.is_watertight), 0.0, 0.0, 0)
            case MeshQualityOp(tag="smooth", smooth=(kind, iterations, factor)):
                self._smooth(kind, iterations, factor)
                return Outcome(MeshQualityResult.Smooth(self._mesh), before, len(self._mesh.faces), bool(self._mesh.is_watertight), 0.0, 0.0, 0)
            case _ as unreachable:  # decimate offloads and metrics is fenced in _route; only subdivide/smooth reach here, a stray kind is a routing fault
                raise AssertionError(unreachable.tag)

    def _smooth(self, kind: SmoothKind, iterations: int, factor: float) -> None:
        match kind:  # one factor row drives each filter's defining parameter; Taubin folds its shrink-free band-pass lamb/nu pair, never a bare iteration count
            case SmoothKind.TAUBIN:
                # trimesh subtracts nu on odd passes (`v -= nu*dot`); nu is a POSITIVE magnitude solved from the reciprocal band
                # so `1/lamb - 1/nu == _TAUBIN_K` for every lamb, never a negative nu (doubles the shrink) nor a fixed offset (breaks the band at small lamb)
                trimesh.smoothing.filter_taubin(self._mesh, lamb=factor, nu=factor / (1.0 - _TAUBIN_K * factor), iterations=iterations)
            case SmoothKind.LAPLACIAN:
                trimesh.smoothing.filter_laplacian(self._mesh, lamb=factor, iterations=iterations)
            case SmoothKind.HUMPHREY:
                trimesh.smoothing.filter_humphrey(self._mesh, alpha=factor, iterations=iterations)
            case unreachable:
                assert_never(unreachable)

    def _metrics_outcome(self, exact: ExactTopology | None) -> Outcome:  # the exact arg arrives resolved from the offloaded kernel (MANIFOLD3D) or None (SPINE)
        metrics = closure_fold(self._mesh, exact)
        worst = metrics.worst
        before = len(self._mesh.faces)
        return Outcome(MeshQualityResult.Metrics(metrics), before, before, metrics.watertight, worst[0], worst[1], metrics.genus)


# the PUBLIC exact-closure fold — the ONE watertight/euler/volume/area/components truth the scan
# reconstruction and deviation consumers compose (quality tiers below the scan producers); a
# re-computed per-consumer closure is the deleted form.
def closure_fold(mesh: trimesh.Trimesh, exact: ExactTopology | None = None) -> QualityMetrics:
    faces = np.asarray(mesh.faces)
    half_edges = np.sort(  # the three directed half-edges per face, endpoint-sorted so an edge and its reverse group together
        np.concatenate([faces[:, [0, 1]], faces[:, [1, 2]], faces[:, [2, 0]]]), axis=1
    )
    unique_edges, counts = np.unique(half_edges, axis=0, return_counts=True)  # exact per-unique-edge incidence; no positional edges_unique/face_adjacency
    boundary_edges = int(np.sum(counts == 1))      # incidence 1 → boundary
    nonmanifold_edges = int(np.sum(counts >= 3))   # incidence ≥3 → non-manifold
    spine = (len(mesh.vertices), len(unique_edges), len(mesh.faces))  # E is the unique-edge fold's own count, one truth, no redundant edges_unique read
    v, e, f = (exact.vertex_count, exact.edge_count, exact.face_count) if exact else spine
    genus = exact.genus if exact else max(0, (2 - (spine[0] - spine[1] + spine[2])) // 2)  # exact override, else Euler V−E+F = 2−2g
    area = exact.area if exact else float(mesh.area)  # the worker tier's kernel mass supersedes the cached trimesh measure
    volume = exact.volume if exact else float(mesh.volume)
    components = exact.components if exact else int(mesh.body_count)  # the backend row: exact decompose() count, trimesh body_count fall-through
    aspect, skew = _cell_shape(mesh)
    defects = np.asarray(mesh.vertex_defects)  # per-vertex angle defect (Gaussian-curvature proxy), folded as angle_defect mean/std, never vertex valence
    return QualityMetrics(
        bool(mesh.is_watertight), bool(mesh.is_winding_consistent),
        area, volume, v, f, e,
        boundary_edges, nonmanifold_edges, components, genus, v - e + f,
        _quantiles(aspect), _quantiles(skew),
        float(np.mean(defects)) if defects.size else 0.0,
        float(np.std(defects)) if defects.size else 0.0,
    )
```
