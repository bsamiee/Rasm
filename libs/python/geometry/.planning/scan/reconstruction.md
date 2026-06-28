# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

Registered-cloud-to-watertight-mesh reconstruction — the producer of the `reconstructed-mesh` `GeometrySubject` the compute graduation union already declares. `ScanReconstruction` is one frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, parity with the sibling `ScanRegistration`/`ScanDeviation`: the algorithm is a STATIC `TriangleMesh` constructor choice, never a runtime mode flag, so reconstruction selects WHICH open3d constructor builds the surface — `create_from_point_cloud_poisson` (the watertight indicator-function default), `create_from_point_cloud_ball_pivoting` (the detail-preserving rolling-ball surface over the raw samples), `create_from_point_cloud_alpha_shape` (the concave-hull surface for sparse or open scans). The constructor choice is data, not a `match` arm: `_CONSTRUCT` is the one `Map[ReconstructionMethod, ConstructSpec]` behavior table whose row binds each method's static open3d constructor as a `Callable[[o3d.geometry.PointCloud, ReconPolicy], o3d.geometry.TriangleMesh]`, so a new algorithm is one row, not a new dispatch arm — the ingestion `_STAGE`/deviation classification-table parity. The shared `estimate_normals`/`orient_normals_consistent_tangent_plane` pre-step conditions every method (Poisson and ball-pivoting both require globally consistent oriented normals), and an optional `cluster_dbscan` split reconstructs each density cluster separately for a multi-object scene. The open3d `TriangleMesh` egresses as GLB through `trimesh.Trimesh.export` (open3d's `io` writes `PLY`/`OBJ`/`STL`/`OFF` only), and watertight conditioning routes the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm rather than re-implementing repair here.

`MeshQuality` is the manifold-quality fold this owner folds the reconstructed body's closure algebra into ONCE — `is_watertight`/`is_winding_consistent`/`euler_number`/`is_volume`/`body_count` plus the `volume`/`area`/vertex/triangle counts read in one pass through `MeshQuality.fold(body)`, the `DeviationBand.fold` parity the page lacked. The band owns the whole quality concern: `facts()` projects the receipt fact map, `residuals`/`ceiling` project the graduation ledger, so the receipt and the admission gate read the same fold rather than the receipt re-scanning the body stat-by-stat. The residual ledger is no longer the single binary `nonwatertight` flag — `MeshQuality.residuals` carries `nonwatertight` AND `noncontiguous` (the `body_count - 1` over-segmentation residual, `0.0` for the single welded solid) so a Poisson balloon that closes into two disjoint shells fails the gate the single watertight flag would pass.

`ReconReceipt` graduates the watertight solid through the compute `graduation/handoff.md#GRADUATION` `GraduationReceipt.graduates` admission fold against the `MeshQuality`-projected `_CEILING` residual ledger — the same residual-over-ceiling rail the sibling `RegistrationResult`/`DeviationResult` cross, so the `reconstructed-mesh` `GeometrySubject` literal is imported from `rasm.compute.graduation.handoff`, never minted as a bare `str`, and the ledger finiteness refinement is the compute `_admit` `@beartype(conf=FAULT_CONF)` fence the rail already owns, never re-fenced here. Receipt egress rides the `observability/receipts#RECEIPT` `@receipted(_REDACTION)` AOP aspect over the inner `_emit` rather than an inline `Signals.emit` threaded through the body — the decorator rail the compute `handoff.md` `_emit` establishes — and the multi-second solve runs inside one `geometry.reconstruct` OTel span weaving the `msgspec` carriers, the runtime `boundary` fault fence, and the receipt aspect into a single fenced rail. The registered pose from `scan/registration.md#REGISTRATION` is the precondition; the reconstructed mesh graduates as the geometry-branch mesh, distinct from the compute `analysis/spatial.md#SPATIAL` non-mesh "reconstructed boundary" same-axis handoff. This page is the missing PRODUCER of an already-declared subject and the supplier of its measured/ceiling ledger, never the author of the compute interior.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: method-discriminated surface reconstruction over one frozen owner, the `_CONSTRUCT` constructor table, the `MeshQuality` manifold-closure fold, the shared normal-estimation pre-step, the optional multi-object cluster split, and the GLB egress, the cross-cutting concerns folded as aspects — `boundary` the exception-to-fault fence, the `geometry.reconstruct` OTel span, the `@receipted(_REDACTION)` egress aspect, the `@beartype(conf=FAULT_CONF)` `DensityField` finiteness fence on the in-page `_trim_poisson` density reduction (the sibling `SignedField`/`_structured` parity), `LanePolicy.offload` the CPU-offload seam, `GraduationReceipt.graduates` the watertight admission gate (whose graduation-ledger finiteness the compute `_admit` `@beartype(conf=FAULT_CONF)` fence owns, distinct from the local density-array fence and never re-fenced here).

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` — the frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, carrying a `ReconPolicy` value object (the normal-search radius/max-nn, the orientation k, the Poisson depth/scale/density-quantile, the ball-pivoting radius schedule, the alpha value, the DBSCAN eps/min-points) with derived `normal_search`/`radii` projections rather than loose scalars, parity with the sibling `RegistrationPolicy`/`DeviationPolicy`; `MeshQuality` the leaf value object folding the reconstructed body's closure algebra once — `MeshQuality.fold(body)` reads the trimesh body's `is_watertight`/`is_winding_consistent`/`euler_number`/`is_volume`/`body_count`/`volume`/`area` and the vertex/triangle counts in one pass, with `facts()` projecting the receipt map and `residuals`/`ceiling` projecting the graduation ledger so the receipt and the gate read one fold (the `DeviationBand` parity); `ReconReceipt` the per-reconstruction typed receipt carrying the method, the input point count, the cluster count, and the folded `MeshQuality`, with a `ReconReceipt.of` factory reading the trimesh body once into the band so the one arm constructs through one keyword fold, a `contribute` `ReceiptContributor` egress under the `@receipted(_REDACTION)` aspect, and a `graduates(evidence_key)` admission rail; `ReconstructionMethod` the watertight-vs-detail constructor vocabulary the `_CONSTRUCT` table keys; `ConstructSpec` the one `Struct` behavior row binding a method's static open3d constructor as a `build: Callable[[o3d.geometry.PointCloud, ReconPolicy], o3d.geometry.TriangleMesh]`.
- Cases: `ReconstructionMethod` rows `POISSON` (the `create_from_point_cloud_poisson` screened-Poisson indicator-function surface, watertight by construction, the default reconstruction) · `BALL_PIVOTING` (the `create_from_point_cloud_ball_pivoting` rolling-ball surface over the oriented samples across a radius schedule, detail-preserving, never closed) · `ALPHA_SHAPE` (the `create_from_point_cloud_alpha_shape` concave-hull surface for sparse or open scans) — each method one `_CONSTRUCT` `Map[ReconstructionMethod, ConstructSpec]` row binding the STATIC open3d constructor that owns it, resolved by one `_CONSTRUCT[method].build(cloud, policy)` row read rather than a `match`/`assert_never` over three near-identical constructor arms; the method is a constructor choice per `open3d.md#153`, never a runtime branch inside one constructor. The three methods are rows of one `reconstruct` pipeline keyed by the request value, never three parallel result shapes — the ingestion `_STAGE`/deviation classification-table parity collapsing the former three-arm `_construct` match into a data table.
- Entry: `ScanReconstruction.reconstruct` admits a registered cloud and a method, and returns a `RuntimeRail[tuple[bytes, ReconReceipt]]` through one `boundary(f"reconstruct.{method}", ...)` exception-to-fault conversion woven under one `geometry.reconstruct` OTel span (the span the measured operation opens, `trace.get_tracer`/`start_as_current_span`, widening with `receipt.quality.span_facts` behind the `is_recording()` gate on the `Ok` arm and leaving the `Error` arm to the `boundary` fence's `_convert`), so the interior raises only inside the `boundary` thunk and the open3d/trimesh fault lifts to a `BoundaryFault` exactly once at that egress; the optional `lane: LanePolicy | None` field is the imported per-subinterpreter offload seam the Growth bullet hands the multi-second Poisson/ball-pivoting solve across through the one `LanePolicy.offload(kernel, *args)` call (the same seam the registration/ingestion/deviation siblings carry), `LanePolicy` the imported lane field so the seam exists and the lane never imports the kernel. The interior `_dispatch` binds the shared `_estimate` normal pre-step once above the cluster split (every method consumes oriented normals), splits into density clusters through `_cluster` only when `ReconPolicy.dbscan_eps > 0.0`, folds each cluster's `_CONSTRUCT[method].build` mesh into one accumulated `TriangleMesh` through `Block.of_seq(...).fold` over the immutable open3d `+` merge — never the in-place `+=` that would mutate the seed across the fold — (the `expression` `Block` carrier the rail already speaks, never a bare `functools.reduce`), lifts the vertices/triangles into a `trimesh.Trimesh` through `_trimesh`, and returns the bare `Trimesh.export(file_type="glb")` bytes paired with the pure `ReconReceipt.of` row; emission rides the entry-level `_emit` wrapper composed INSIDE the `boundary` thunk (`boundary(..., lambda: self._emit(self._dispatch(...)))`, the registration `RegistrationResult._emit`-inside-the-fence parity) so the `@receipted(_REDACTION)` aspect on `ReconReceipt._emit` harvests `contribute` on the receipt slot while the GLB bytes ride through untouched and a render/sink raise folds onto the same rail — the kernel `_dispatch` stays pure of the emission aspect.
- Auto: `PointCloud.estimate_normals(KDTreeSearchParamHybrid(radius, max_nn))` (`open3d.md` rows [03]/[17]) fills the per-point normals and `orient_normals_consistent_tangent_plane(k)` (`open3d.md` row [04]) propagates a globally consistent orientation across the tangent-plane graph (the orientation Poisson and ball-pivoting both require for a correctly-signed surface); the `POISSON` row's `build` calls `TriangleMesh.create_from_point_cloud_poisson(cloud, depth, scale)` (`open3d.md` row [11]) returning the reconstructed mesh plus the per-vertex density array the low-density trim reads, the `BALL_PIVOTING` row `create_from_point_cloud_ball_pivoting(cloud, radii: DoubleVector)` (`open3d.md` rows [12]/[18]) rolling the ball over the oriented samples across the `DoubleVector` radius schedule, and the `ALPHA_SHAPE` row `create_from_point_cloud_alpha_shape(cloud, alpha)` (`open3d.md` row [15]) building the alpha-complex concave hull; the Poisson row's `remove_vertices_by_mask(NDArray[bool])` (`open3d.md` row [16]) drops the low-density balloon artifacts screened-Poisson extrudes past the sample support, the cutoff the `poisson_density_quantile` order statistic read off `numpy.sort(density)` (`numpy.md` row [15], the catalogued sort owning the quantile — `numpy.quantile` is uncatalogued) at the `int(q * (n - 1))` fractional index and the mask the `numpy.asarray(density) < cutoff` breach (`numpy.md` row [10] `asarray`); `cluster_dbscan(eps, min_points)` (`open3d.md` row [08]) labels the cloud into density clusters so a multi-object scene reconstructs each cluster separately through `select_by_index(numpy.where(labels == label)[0])` (`open3d.md` row [10], `numpy.md` row [09] `where`); `MeshQuality.fold` reads the open3d `TriangleMesh.vertices`/`.triangles` folded into a `trimesh.Trimesh(vertices, faces, process=False)` (`trimesh.md` type row [01]) and harvests the body's `is_watertight`/`is_winding_consistent` (`trimesh.md` cached-property row [04]), `euler_number` (row [04]), `is_volume`/`body_count` (row [05]), and `volume`/`area` (row [01]) cached properties in one pass, the `.export(file_type="glb") -> bytes` (`trimesh.md` row [07], the only encode path, never a hand-rolled GLB serializer) writing the GLB the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm conditions to a guaranteed watertight solid.
- Receipt: `ReconReceipt.contribute` returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port (`observability/receipts#RECEIPT`) streams — `Receipt.of("geometry.scan.reconstruction", ("emitted", method.value, facts))` against the runtime two-argument `of(owner, evidence)` contract minting the `fact` case at `emitted`, the facts spread from `MeshQuality.facts()` so the receipt and the graduation ledger read one fold rather than a stat-by-stat re-scan, native `int`/`float`/`bool` slots the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce; the receipt-stream egress rides the `@receipted(_REDACTION)` AOP aspect over an inner `_emit(receipt) -> ReconReceipt` (the decorator rail the compute `handoff.md` `_emit` establishes), never an inline `Signals.emit` threaded through the body. `ReconReceipt.graduates(evidence_key)` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=_SUBJECT)`, gating the `MeshQuality.residuals` ledger against the `MeshQuality.ceiling` per-key fold — the `nonwatertight` residual (`0.0` watertight else `1.0`, against the `0.0` ceiling) AND the `noncontiguous` residual (`float(body_count - 1)`, against the `0.0` ceiling, so a closed surface that fragments into disjoint shells fails the gate the single watertight flag passes) — so a non-watertight or multi-shell surface is an `Error(BoundaryFault)` on the graduation rail rather than a graduated handoff (the watertight solid graduates only after the `mesh/repair.md#MESH` weld), both keys riding the compute owner's residual-over-ceiling `_admit` direction unchanged, never a second admission direction minted here. The subject is typed as the compute-owned `GeometrySubject` `"reconstructed-mesh"` literal (imported from `rasm.compute.graduation.handoff`, never a bare `str`, so an unlisted literal fails at the type boundary); this owner is the CONSUMER of the already-declared subject and the supplier of its measured/ceiling ledger, the residual-over-ceiling fold itself the one admission gate the compute owner owns.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies the aligned cloud, never re-derived here); raw-scan ingestion and decimation route the `scan/ingestion.md#INGESTION` sibling, never re-owned here; watertight mesh repair and hole-fill route the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm, never re-implemented here (a non-watertight ball-pivoting or alpha-shape surface becomes a valid solid only through that seam); scan-vs-model deviation routes the `scan/deviation.md#DEVIATION` sibling; no IFC tessellation (that is `tessellation`); no durable store; no Rhino/GH mutation. The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary" is a DIFFERENT, non-mesh handoff on the same geometry axis — this owner's `reconstructed-mesh` is the geometry-branch mesh, the spatial subject crosses the wire as a non-mesh boundary aligned to the scan companion, so the two never collide on the `geometry` axis. A `reconstruct_poisson`/`reconstruct_ball`/`reconstruct_alpha` method family, a `match`/`assert_never` over three near-identical constructor arms where the `_CONSTRUCT` table resolves the constructor by row, a runtime-flag branch inside one constructor where the algorithm is a static-constructor choice, a stat-by-stat receipt build instead of one `MeshQuality.fold`, an inline `Signals.emit` threaded through the body where the `@receipted(_REDACTION)` aspect owns egress, a vacuous graduation that mints a `reconstructed-mesh` handoff for a non-watertight or multi-shell surface, a single binary `nonwatertight` ledger that passes a fragmented closed surface where the `noncontiguous` residual rails it, a `numpy.quantile` call where the catalogued `numpy.sort` order statistic owns the cutoff, a hand-rolled Poisson or alpha-complex kernel where open3d is admitted, a hand-rolled GLB serializer where `trimesh.export` writes it, and a re-implemented watertight repair where `mesh-utility` owns it are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import numpy as np
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Final

from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis

if TYPE_CHECKING:  # type-only: every runtime open3d call does its own boundary-scope `import open3d as o3d  # noqa: PLC0415`, so the module loads clean on runtime and the import-policy ban on module-level open3d holds (the ingestion/registration sibling shape)
    import open3d as o3d


# --- [TYPES] ----------------------------------------------------------------------------


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"
    BALL_PIVOTING = "ball-pivoting"
    ALPHA_SHAPE = "alpha-shape"


# the Poisson density array is solver output: a degenerate solve can emit `NaN`/`±inf` that would
# corrupt the `np.sort` ordering and the `< cutoff` mask silently, so the `_poisson` trim folds
# behind this fence under the shared `FAULT_CONF` — the same in-page-array finiteness contract the
# sibling `scan/deviation.md#DEVIATION` `SignedField`/`scan/ingestion.md#INGESTION` `_structured`
# fences hold, distinct from the graduation-ledger finiteness the compute `_admit` fence owns.
type DensityField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: GeometrySubject = "reconstructed-mesh"
# graduation gate per quality residual against a zero ceiling: a non-watertight OR multi-shell surface
# fails and graduates only after the mesh/repair.md#MESH weld, never on the vacuous reconstruction alone.
_CEILING: Final[dict[str, float]] = {"nonwatertight": 0.0, "noncontiguous": 0.0}
# graduation facts carry no secret field, so the @receipted egress binds the keep-all redaction.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_TRACER: Final[trace.Tracer] = trace.get_tracer("geometry.scan.reconstruction")


# --- [MODELS] ---------------------------------------------------------------------------


class ReconPolicy(Struct, frozen=True):
    normal_radius: float = 0.1
    normal_max_nn: int = 30
    orient_k: int = 30
    poisson_depth: int = 9
    poisson_scale: float = 1.1
    poisson_density_quantile: float = 0.02
    ball_radii: tuple[float, ...] = (0.05, 0.1, 0.2)
    alpha: float = 0.1
    dbscan_eps: float = 0.0
    dbscan_min_points: int = 16

    @property
    def normal_search(self) -> "o3d.geometry.KDTreeSearchParamHybrid":
        import open3d as o3d  # noqa: PLC0415  boundary-scope: the property runs inside the offloaded kernel, never at module load

        return o3d.geometry.KDTreeSearchParamHybrid(self.normal_radius, self.normal_max_nn)

    @property
    def radii(self) -> "o3d.utility.DoubleVector":
        import open3d as o3d  # noqa: PLC0415

        return o3d.utility.DoubleVector(self.ball_radii)


class MeshQuality(Struct, frozen=True, gc=False):
    vertex_count: int
    triangle_count: int
    body_count: int          # connected components; >1 is a fragmented (over-segmented) reconstruction
    watertight: bool
    winding_consistent: bool
    is_volume: bool          # closed AND consistently wound: a usable signed-distance solid
    euler_number: int        # V - E + F manifold characteristic
    volume: float
    area: float

    @staticmethod
    def fold(body: "trimesh.Trimesh") -> MeshQuality:
        # one pass over the trimesh body's content-hash-keyed manifold algebra; `volume`/`is_volume`
        # are valid only on a closed mesh, so the open surface reports 0.0 volume off the same fold.
        watertight = bool(body.is_watertight)
        return MeshQuality(
            vertex_count=len(body.vertices), triangle_count=len(body.faces), body_count=int(body.body_count),
            watertight=watertight, winding_consistent=bool(body.is_winding_consistent), is_volume=bool(body.is_volume),
            euler_number=int(body.euler_number), volume=float(body.volume) if watertight else 0.0, area=float(body.area),
        )

    @property
    def residuals(self) -> dict[str, float]:
        # binary closure residual + the over-segmentation residual: a Poisson balloon that closes into two
        # disjoint shells is watertight yet `body_count == 2`, so `noncontiguous = 1.0` fails the gate.
        return {"nonwatertight": 0.0 if self.watertight else 1.0, "noncontiguous": float(self.body_count - 1)}

    @property
    def ceiling(self) -> dict[str, float]:
        return _CEILING

    @property
    def span_facts(self) -> dict[str, str | int]:
        return {"vertex_count": self.vertex_count, "triangle_count": self.triangle_count, "body_count": self.body_count}

    def facts(self) -> dict[str, object]:
        return {
            "vertex_count": self.vertex_count, "triangle_count": self.triangle_count, "body_count": self.body_count,
            "watertight": self.watertight, "winding_consistent": self.winding_consistent, "is_volume": self.is_volume,
            "euler_number": self.euler_number, "volume": self.volume, "area": self.area,
        }


class ReconReceipt(Struct, frozen=True, gc=False):
    method: ReconstructionMethod
    input_points: int
    clusters: int
    quality: MeshQuality

    @staticmethod
    def of(method: ReconstructionMethod, *, input_points: int, body: "trimesh.Trimesh", clusters: int) -> ReconReceipt:
        return ReconReceipt(method, int(input_points), int(clusters), MeshQuality.fold(body))

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: ReconReceipt) -> ReconReceipt:
        return receipt  # the @receipted aspect harvests `contribute` and emits on exit; egress is the decorator rail

    def facts(self) -> dict[str, object]:
        return {"method": self.method.value, "input_points": self.input_points, "clusters": self.clusters, **self.quality.facts()}

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("geometry.scan.reconstruction", ("emitted", self.method.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey) -> RuntimeRail[GraduationReceipt]:
        # the quality fold projects BOTH residual keys; a non-watertight or multi-shell surface fails the
        # gate and graduates only after the mesh/repair.md#MESH weld, never on the vacuous reconstruction.
        return GraduationReceipt.graduates(
            "geometry.scan.reconstruction", HandoffAxis(geometry=_SUBJECT), evidence_key,
            self.quality.residuals, self.quality.ceiling,
        )


# --- [TABLES] ---------------------------------------------------------------------------

# one row per method binding its STATIC open3d constructor; a new algorithm is one row, not a new
# dispatch arm. The Poisson row owns the density-trim because its constructor returns the density array.
class ConstructSpec(Struct, frozen=True, gc=False):
    build: Callable[["o3d.geometry.PointCloud", ReconPolicy], "o3d.geometry.TriangleMesh"]


@beartype(conf=FAULT_CONF)
def _trim_poisson(mesh: "o3d.geometry.TriangleMesh", density: DensityField, quantile: float) -> "o3d.geometry.TriangleMesh":
    # the `DensityField` `Is[isfinite]` refinement fires before the order statistic, so a non-finite
    # solver density rails through the faults `CLASSIFY` `api` row rather than corrupting the cutoff.
    if density.size == 0:  # a degenerate solve emits an empty mesh; the order-statistic index would `IndexError`
        return mesh        # (vacuously finite under the fence) — the `DeviationBand.fold` `size == 0` parity
    samples = np.sort(density)  # the catalogued sort owns the quantile; `numpy.quantile` is uncatalogued
    cutoff = samples[int(quantile * (samples.size - 1))]
    mesh.remove_vertices_by_mask(density < cutoff)  # drop the low-density balloon artifacts past the sample support
    return mesh


# each builder does its own boundary-scope `import open3d as o3d` so the `_CONSTRUCT` rows never close
# over a module-level open3d global the import-policy ban leaves unbound — the table holds the builder
# functions, every open3d call deferred to the offloaded kernel that imports the worker package.
def _poisson(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(cloud, depth=policy.poisson_depth, scale=policy.poisson_scale)
    return _trim_poisson(mesh, np.asarray(density), policy.poisson_density_quantile)


def _ball_pivoting(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, policy.radii)


def _alpha_shape(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    return o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(cloud, policy.alpha)


_CONSTRUCT: Final[Map[ReconstructionMethod, ConstructSpec]] = Map.of_seq([
    (ReconstructionMethod.POISSON, ConstructSpec(build=_poisson)),
    (ReconstructionMethod.BALL_PIVOTING, ConstructSpec(build=_ball_pivoting)),
    (ReconstructionMethod.ALPHA_SHAPE, ConstructSpec(build=_alpha_shape)),
])


# --- [SERVICES] -------------------------------------------------------------------------


class ScanReconstruction(Struct, frozen=True):
    policy: ReconPolicy = ReconPolicy()
    lane: LanePolicy | None = None

    def reconstruct(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> RuntimeRail[tuple[bytes, ReconReceipt]]:
        with _TRACER.start_as_current_span("geometry.reconstruct") as span:
            if span.is_recording():
                span.set_attributes({"method": method.value})
            rail = boundary(f"reconstruct.{method}", lambda: self._emit(self._dispatch(cloud, method)))
            match rail:
                case Ok((_, receipt)):
                    if span.is_recording():
                        span.set_attributes(receipt.quality.span_facts)  # widen the recording span with the folded counts
                    span.set_status(Status(StatusCode.OK))  # the ERROR side is the `boundary` fence's `_convert`, never re-set here
                case Error(_):
                    pass
            return rail

    @staticmethod
    def _emit(payload: tuple[bytes, ReconReceipt]) -> tuple[bytes, ReconReceipt]:
        # the `@receipted` aspect on `ReconReceipt._emit` harvests `contribute` and emits on exit; the
        # GLB bytes ride through untouched so emission stays a decorator rail over the receipt slot.
        return payload[0], ReconReceipt._emit(payload[1])

    def _dispatch(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> tuple[bytes, ReconReceipt]:
        import open3d as o3d  # noqa: PLC0415  boundary-scope: the kernel runs inside the offload hop, never at module load

        oriented = self._estimate(cloud)
        clusters = self._cluster(oriented) if self.policy.dbscan_eps > 0.0 else (oriented,)
        build = _CONSTRUCT[method].build
        mesh = Block.of_seq(clusters).fold(lambda acc, part: acc + build(part, self.policy), o3d.geometry.TriangleMesh())
        body = self._trimesh(mesh)
        return body.export(file_type="glb"), ReconReceipt.of(method, input_points=len(oriented.points), body=body, clusters=len(clusters))

    def _estimate(self, cloud: "o3d.geometry.PointCloud") -> "o3d.geometry.PointCloud":
        cloud.estimate_normals(self.policy.normal_search)
        cloud.orient_normals_consistent_tangent_plane(self.policy.orient_k)
        return cloud

    def _cluster(self, cloud: "o3d.geometry.PointCloud") -> tuple["o3d.geometry.PointCloud", ...]:
        labels = np.asarray(cloud.cluster_dbscan(self.policy.dbscan_eps, self.policy.dbscan_min_points))
        return tuple(cloud.select_by_index(np.where(labels == label)[0]) for label in range(int(labels.max()) + 1))

    def _trimesh(self, mesh: "o3d.geometry.TriangleMesh") -> "trimesh.Trimesh":
        import trimesh  # noqa: PLC0415

        return trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
```

## [03]-[RESEARCH]

- [ALPHA_SHAPE_ARITY]: the `TriangleMesh.create_from_point_cloud_alpha_shape(cloud, alpha)` constructor is folder-`.api`-confirmed (`open3d.md` row [15]) alongside the `poisson` `(cloud, depth, scale)` return tuple (mesh plus per-vertex density `DoubleVector`, `open3d.md` row [11]) and the `ball_pivoting` `(cloud, radii: DoubleVector)` rows ([12]/[18]); the `ALPHA_SHAPE` `_CONSTRUCT` row passes the single `alpha` positional and never the optional `tetra_mesh`/`pt_map` reuse overload, so the only live-run residual is whether a pre-built `tetra_mesh` should be threaded for a multi-cluster split to skip the per-cluster Delaunay rebuild, an owner-local performance choice, not an existence question. The Poisson depth/scale schedule and the ball-pivoting radius schedule are owner-local `ReconPolicy` parameters threaded through `normal_search`/`radii`, not catalogue dependencies.
- [DENSITY_TRIM]: the Poisson low-density-vertex trim lives in the `POISSON` `_CONSTRUCT` row's `_poisson` build (the only method whose constructor returns the density array), which hands the per-vertex density `DoubleVector` the `create_from_point_cloud_poisson` second return carries into the `@beartype(conf=FAULT_CONF)`-fenced `_trim_poisson` over the `DensityField = Annotated[np.ndarray, Is[isfinite]]` refinement (`beartype.md` `vale.Is` ENTRYPOINTS [01]) so a `NaN`/`±inf` solver density raises the canonical `BeartypeCallHintViolation` the faults `CLASSIFY` `api` row rails before it corrupts the `numpy.sort` ordering or the `< cutoff` mask — the same in-page-array finiteness contract the sibling `scan/deviation.md#DEVIATION` `SignedField` fence on `DeviationBand.fold` and the `scan/ingestion.md#INGESTION` `_structured` fence hold, distinct from the graduation-ledger finiteness the compute `_admit` fence owns. The trim folds the density against the `poisson_density_quantile` cutoff through `remove_vertices_by_mask(NDArray[bool])` (`open3d.md` row [16]), removing the balloon artifacts screened-Poisson extrudes past the sample support. The cutoff is the order statistic read off `numpy.sort(density)` (`numpy.md` row [15]) at the `int(q * (n - 1))` fractional index — the catalogued sort owns the quantile rather than an uncatalogued `numpy.quantile` member — and the mask the `density < cutoff` breach over the `numpy.asarray`-conditioned array (`numpy.md` row [10] `asarray`); the quantile threshold is an owner-local heuristic the live run tunes against the scan density.
- [MESH_QUALITY_FOLD]: `MeshQuality.fold(body)` reads the trimesh body's content-hash-keyed manifold algebra in ONE pass — `is_watertight`/`is_winding_consistent`/`euler_number` (`trimesh.md` cached-property row [04]), `is_volume`/`body_count` (row [05]), and `volume`/`area` (row [01]) — so the receipt facts and the graduation residual ledger read one fold rather than the receipt re-scanning the body stat-by-stat (the `DeviationBand.fold` parity). The `volume` slot is valid only on a closed solid, so the open `BALL_PIVOTING`/`ALPHA_SHAPE` surface reports `0.0` volume off the same fold rather than the trimesh degenerate-volume artifact. The fold is the single place the body algebra is read; `facts`, `residuals`, `ceiling`, and `span_facts` all project from it and never re-probe the body.
- [WATERTIGHT_GATE]: the `Trimesh.is_watertight`/`body_count` cached properties (`trimesh.md` cached-property rows [04]/[05]) are the gate the `MeshQuality.residuals` ledger projects — `nonwatertight` is `0.0` when closed else `1.0`, `noncontiguous` is `float(body_count - 1)` (`0.0` for the single welded shell), both against the `_CEILING` zero bar through the compute `GraduationReceipt.graduates` residual-over-ceiling fold. The two keys are independent precisely because a Poisson balloon can close into two disjoint shells — `is_watertight` is `True` yet `body_count == 2`, so the single binary watertight flag would graduate the fragmented surface the `noncontiguous` residual rejects. The open3d mesh carries its own `is_watertight`, but the receipt reads the trimesh body's flags because that is the exported GLB the downstream `mesh/repair.md#MESH` seam and the C# owner consume; the gate is a CONSUMER read of the compute admission rail, not a local admission body.

## [04]-[UPSTREAM]

- [GRADUATION_SUBJECT]: the `reconstructed-mesh` `GeometrySubject` this owner graduates is already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (joining `registration-transform`, `scan-deviation`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`), so `_SUBJECT` imports the literal from `rasm.compute.graduation.handoff` rather than minting a bare `str` — an unlisted literal fails at the `GeometrySubject` type boundary, the compute owner owning the union. `ReconReceipt.graduates` routes the `MeshQuality`-projected TWO-key measured ledger (the binary `nonwatertight` residual AND the `noncontiguous` over-segmentation residual) through the one compute `GraduationReceipt.graduates` admission fold against the `_CEILING` zero bars — the per-key residual-over-ceiling gate the compute owner owns, not a local admission body — so a non-watertight OR multi-shell surface is an `Error(BoundaryFault)` rather than a graduated handoff, both keys riding the compute owner's existing upper-bound `_admit` direction with no second admission direction minted here, the same two-key residual-ledger shape the sibling `registration.md#REGISTRATION` (`inlier_rmse`/`misfit`) and `deviation.md#DEVIATION` (`max_distance`/`noncompliant_fraction`) graduate through. The finiteness refinement on the LEDGER inputs is the compute `_admit` `@beartype(conf=FAULT_CONF)` fence (`handoff.md#GRADUATION` Refinements), so this page supplies the measured/ceiling ledger and never re-fences it; the in-page `_trim_poisson` density-array finiteness is a distinct concern the compute fence never sees (the density is consumed and discarded before any ledger forms), so this page fences it locally through its own `DensityField` `@beartype(conf=FAULT_CONF)` refinement, the sibling `SignedField`/`_structured` parity. The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary graduates outward ... never crossing as a geometry-branch mesh" subject is a DIFFERENT, non-mesh handoff on the same `geometry` axis — the spatial owner's boundary crosses the wire aligned to the scan companion as a non-mesh artifact, while this owner's reconstructed mesh crosses as the watertight GLB solid. The two are non-colliding subjects on the geometry axis; the one-pass synthesis reconciliation keeps them distinct, never folding the spatial boundary into the geometry-branch mesh surface. This page is the CONSUMER of the already-declared subject and the compute admission rail, supplying only its measured/ceiling ledger, never authoring the compute interior.
