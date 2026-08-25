# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration over an N-cloud session, not a fixed pair: `ScanRegistration` discriminates every alignment strategy by `RegistrationMode` over a `RegistrationSession`, the two-or-more-cloud tuple whose `>=2` arity the PEP-646 type carries statically and `register` re-proves at runtime, so a length-1 session refuses typed at the boundary, never as a worker-side `IndexError`. Sessions carry the `scan/ingestion#INGESTION` `Cloud` array carrier, never a live `open3d` cloud — a tensor point cloud is a pybind11 handle no pickler carries, so the arrays cross the process seam whole and each kernel arm re-inflates through `Cloud.tensor()`/`legacy()` where its own native solve begins. Pairwise modes read the first two clouds and the multiway mode folds all N, so a pair is the degenerate `N==2` case of the session, never a parallel surface. The `NONRIGID` row extends that same session with a correspondence-free probabilistic arm: an EM solve over Gaussian mixtures recovers a warp whose per-point displacement crosses back as sealed arrays beside the pose, so `scan/deviation#DEVIATION` partitions a construction deviation from a structural deformation instead of reading one rigid residual over both. Nearest-neighbor registration and pose-graph alignment are this owner's charter — `mesh/spatial#SPATIAL` composes `open3d`/`small-gicp` as query libraries but owns no registration capability.

`register` runs `async`, so no ICP, RANSAC, or pose-graph solve touches the event loop: it composes the graduation `evidence_run` weave (seeded `EvidenceScope.SCAN_REGISTRATION`, no page-local tracer or span/`_ok` pair) around the `lane.offload` crossing on `Kernel.of(_register_kernel, KernelTrait.HOSTILE)` — the `open3d`/`small-gicp`/`kiss-matcher`/`probreg` band holds process-global native state and imports under no isolated subinterpreter, so the module-level kernel ships `REFERENCE` onto the warm process pool and the lane imports neither the kernel nor any compiled package. `probreg` pulls `open3d` transitively at import, so the non-rigid row adds no second native band to the crossing. Each transform graduates through the `rasm.geometry.graduation` spine as `GeometrySubject.REGISTRATION_TRANSFORM`; `graduates()` returns the local `GeometryHandoff` on a key its own `spec` projection derives from the session digest, the mode, the resolved engine, the resolved seed, and the complete tuning-and-ceiling bytes — no caller threads an evidence key — and alignment ceilings ride `RegistrationPolicy` rows.

## [01]-[INDEX]

- [02]-[REGISTRATION]: mode-discriminated alignment over an N-cloud session — global bootstrap, coarse-to-fine and colored ICP, VGICP fine-refinement, probabilistic non-rigid warp, and multiway pose-graph — behind one `async` graduation-weave entry.

## [02]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` over a `RegistrationSession`; `RegistrationPolicy` is the one tuning carrier for every voxel/correspondence/Tukey/solver-gain/multiway bar including the graduation ceilings, with a derived `voxel_schedule`; `RegistrationResult` the `gc=False` receipt whose `of` factory ravels the transform once and defaults the single-pose tuple, sharing one `_from_tensor` projector across the tensor arms and conforming structurally to the `ReceiptContributor` the weave's harvest reads; `BootstrapEngine` (`KISS_MATCHER` | `OPEN3D_FGR`) the global-coarse-pose vocabulary and `NonRigidEngine` (`CPD` | `FILTERREG`) the probabilistic-warp one, `FilterRegObjective` the geometry term the permutohedral row discriminates on. `DeformationField` is the sealed non-rigid product — warped positions, per-point displacement magnitudes, and the mixture's own `sigma2`/`q` convergence evidence — riding the ONE result carrier's optional slot, never a parallel non-rigid receipt: every arm answers `RegistrationResult`, and the arms that recover a warp fill one more slot on it.
- Cases: `RegistrationMode` rows — `GLOBAL` (initialization-free coarse pose, no initial pose), `MULTISCALE` (coarse-to-fine tensor ICP, Tukey-robust point-to-plane), `COLORED_ICP` (colored point-to-plane), `VGICP` (`small-gicp` voxelized parallel fine-refinement speed path), `NONRIGID` (`probreg` correspondence-free EM warp), `MULTIWAY` (N-cloud pose-graph). `NONRIGID`'s algorithm is a POLICY coordinate, never a mode row of its own: `NonRigidEngine.CPD` reaches `cpd.registration_cpd` with `tf_type_name="nonrigid"` — the string IS the deformation family, the Gaussian-RBF transformation whose `transform` warps arbitrary query points — and `NonRigidEngine.FILTERREG` reaches `filterreg.registration_filterreg`, whose published discriminant is the `objective_type` geometry term consuming the target's own normal band for the point-to-plane arm, so the permutohedral row is the probabilistic RIGID baseline against which the CPD row's field reads as deformation. `GLOBAL`'s coarse pose seeds every fine arm on the `_SEEDABLE` roster — `register` admits it as an `Option[RegistrationResult]` session input, re-proves the held pose as sixteen finite floats at the parent boundary, and refuses a seed on the initialization-free rows (`GLOBAL`, and `MULTIWAY`, whose every edge derives its pose from its own coarse solve), so a pose no solver consumes never joins the evidence key; `bootstrapped` is the composed two-stage fold that produces it and gates its fine mode on that same roster, so the seeding law is a real entry rather than a claim; each arm binds the engine and estimator that owns it.
- Law: `_distributed` records the `REGISTRATION_TRANSFORM` charter row through the graduation `charter_record` derivation on the parent side of the offload — the HOSTILE kernel's meter is the worker's no-op, so a worker-side record meters nothing — reading the one `facts()` fold the receipt already emits, so the measure and the receipt line can never disagree and no spelling is hand-picked here.
- Law: `bench` rides the graduation `bench_seam` fold over the whole `register` crossing — arity re-proof, offload, solver, weave — cloud-size-parameterized: the subject keys the exact `RegistrationMode` row and the source point count as `rasm.geometry.scan.registration.<mode>.p<points>`, so a latency row compares like-for-like across scan densities; latency and throughput rows per arm, zero instrument rows, graduation's `bench_terminal` wrapping the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Entry: `register` admits a session and a mode and returns `RuntimeRail[RegistrationResult]`. Its weave opens the seeded span, `async_boundary` fences the offload, `_flat` absorbs the lane's already-fenced rail un-nested, and the harvest emits the conforming result once on the cleared `Ok` while an `open3d`/`kiss-matcher` raise stays an `Error(BoundaryFault)` on the live span. Kernels take the lane conduit's pickled tap as a trailing offload arg and beat the graduation `GeometryPulse.REGISTRATION` point through `pulsed` — one solve-start beat per mode, one per multiway edge — so a `Hooks` tap streams convergence progress under the lane's lossy drop law.
- Auto: `_engine` folds ONE ordered provider map through one `find_spec` probe over BOTH engine families and answers `Option` — the caller names the members it admits, the whole bootstrap family or the single non-rigid row the policy picked, and the head that resolves wins. Every one of `kiss_matcher`, `open3d`, and `probreg` is interpreter-marked, so a probe that tests one and falls through to another as a floor reports a capability that floor cannot deliver; a `Nothing` refuses typed at the arm that needs it rather than raising `ModuleNotFoundError` inside a worker. The `NONRIGID` gate reads that same map PARENT-SIDE in `register`, before the crossing, onto `Error(REG_NONRIGID.raised(...))` naming `probreg` — module presence is identical on every floor of the one shared venv, so the probe picks a capability tier and never an offload route, and the kernel's non-rigid arm runs past a decision already made. Every arm (`GLOBAL`, each `MULTIWAY` edge, `NONRIGID`) reuses that one map; the tensor arms share the `_tukey` robust kernel and the `_from_tensor` projector rather than re-reading the `open3d` result per arm.
- Auto: the coarse pose reaches EVERY fine arm through that arm's OWN initial-transform argument — the open3d tensor `init_source_to_target` slot on `icp` and `multi_scale_icp`, the `small_gicp.align` `init_T_target_source` slot — so `_seeded` builds one 4x4 and each solver seeds its correspondence search directly and returns the full source-to-target pose it already composed. The deleted mechanism is a whole-cloud copy per solve plus its normal rotation plus a post-solve matrix product, all of which the provider performs inside its own iteration for free. Every correspondence-search arm publishes that slot; the `NONRIGID` arm is the ONE stated exception, and it pays the pre-pose price knowingly: `registration_cpd` and `registration_filterreg` take the outlier weight, the iteration budget, the tolerance, and the objective term alone, so the seed applies ONCE at the arm's own admission as a pre-posed source array. That copy is bounded by the arm's shape — one EM pass over one pair, so one copy, where a multi-scale arm would have paid one per scale — and the pose it applied rides back in the result's own `transform` slot as the arm's rigid component, so the deformation field measures exactly what the pose could not explain. A local ICP started from identity diverges on any pair whose gross misalignment exceeds `max_correspondence`, which is the whole reason the `GLOBAL` arm exists and why an unseeded arm passes the identity those slots already default to rather than skipping the argument.
- Auto: native registration enters `LanePolicy.whole`; runtime grants the lane once and `LaneGrant.width` becomes the provider thread count beside the pulse tap. Single-model parallelism stays available without multiplying a copied width by concurrent outer admissions or reading allocator internals.
- Receipt: emission is the weave's harvest — the conforming `RegistrationResult.contribute` streams once on the cleared `Ok`, never an inline emit or page-local `@receipted` leg. `inlier_rmse` is OPTIONAL because the `KISS_MATCHER` arm measures none, so `graduates` derives its ceiling roster PER MEASURE: the `1 - fitness` misfit is graded on every arm and the RMSE bar joins only where an RMSE exists, since a fabricated `0.0` clears every ceiling and graduates a coarse pose as a converged alignment. The `NONRIGID` arm measures both on the WARPED cloud against the target through the same `evaluate_registration` fold the multiway edges read, so a probabilistic warp grades on the identical bars rather than on a mixture objective no other arm shares; `sigma2` and `q` ride the receipt as the EM's own convergence evidence beside them — with the EFFECTIVE FilterReg objective where that arm ran, since the pt2pt fallback on a normal-less target is solver evidence the policy bytes cannot recover — and the deformation magnitudes ride as a QUANTILE ladder — median, p95, extremum, mean — because a receipt never grows with rows and one extremum never replaces a distribution, the per-point field itself being the payload `scan/deviation#DEVIATION` consumes. `deformation_max` joins the graduation roster on that same per-measure derivation, against the `deformation_ceiling` monitoring bar: a field past it is the structural-deformation alarm, and an arm that recovered no field is never graded on a bar it was in no position to measure. That misfit rides the graduation owner's single `_admit` residual-over-ceiling direction, so no second admission direction is minted here.
- Packages: `kiss_matcher`, `open3d`, `small_gicp`, `probreg` (the compiled registration backends, each a module-scope `lazy import`/`lazy from` so the marked distributions stay cold until their own arm runs — never an eager module-top import and never a function-local one the module-top roster hides), `numpy` (transform assembly via `np.eye`/`np.ravel`/`np.reshape`, never the uncatalogued `np.identity`/`ndarray.flatten`), `expression` (`Block.mapi` the per-edge multiway fold, `Block.collect` the session-preimage flat-map), `msgspec`, and the geometry graduation spine (`evidence_run`/`GeometryHandoff`/`GeometrySubject`, `charter_record` the charter measure authority, `bench_seam`/`bench_terminal`, `GeometryPulse` the pulse id roster) and runtime rails per the fence imports, the mid-operation payload being the runtime `StageMark` this page marks with its own closed `RegistrationStage` roster.
- Growth: a new registration engine is one `RegistrationMode` row, one kernel arm inheriting the carrier pre-pose with no seeding edit, and — where its solver consumes a seed — one `_SEEDABLE` member; a new bootstrap backend is one `BootstrapEngine` member, one `_ENGINE_MODULE` probe row, and one `_bootstrap` arm; a new probabilistic estimator is one `NonRigidEngine` member, its `_ENGINE_MODULE` row, and one `_nonrigid` arm answering the same `DeformationField`; a feature-space correspondence is one `feature_fn` policy row on the `FILTERREG` arm; a stricter graduation bar is a `RegistrationPolicy` ceiling the caller passes. `registration_ransac_based_on_feature_matching` is the named next `BootstrapEngine` row when a scene defeats both standing engines.
- Boundary: the cleaned input `Cloud` is `scan/ingestion#INGESTION`'s product and carrier mint; deviation against a reference is `scan/deviation#DEVIATION`; surface reconstruction is `scan/reconstruction#RECONSTRUCTION`. The deformation field mints HERE and crosses to the deviation owner, which partitions it against its own signed band and never re-solves a warp; a live `probreg` `Transformation` never leaves this kernel, because it is an `open3d`-coupled native handle the pickle seam cannot carry. No mesh repair, tessellation, or durable store here.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from queue import Queue
from typing import Final, Literal, assert_never

import numpy as np
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field
from msgspec.structs import replace

from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometryLeg,
    GeometryPulse,
    GeometrySubject,
    bench_seam,
    bench_subject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.hooks import StageMark
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LaneGrant, LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import kiss_matcher
lazy import open3d as o3d
lazy import small_gicp

lazy from probreg import cpd, filterreg

# --- [TYPES] ----------------------------------------------------------------------------


class RegistrationStage(StrEnum):
    SOLVE = "solve"
    EDGE = "edge"


class RegistrationMode(StrEnum):
    GLOBAL = "global"
    MULTISCALE = "multiscale"
    COLORED_ICP = "colored-icp"
    VGICP = "vgicp"
    NONRIGID = "nonrigid"
    MULTIWAY = "multiway"


class BootstrapEngine(StrEnum):
    KISS_MATCHER = "kiss-matcher"
    OPEN3D_FGR = "open3d-fgr"


class NonRigidEngine(StrEnum):
    CPD = "cpd"
    FILTERREG = "filterreg"


class FilterRegObjective(StrEnum):
    PT2PT = "pt2pt"
    PT2PL = "pt2pl"


type RegistrationSession = tuple[Cloud, Cloud, *tuple[Cloud, ...]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENGINE_MODULE: Final[Block[tuple[BootstrapEngine | NonRigidEngine, str]]] = Block.of_seq((
    (BootstrapEngine.KISS_MATCHER, "kiss_matcher"),
    (BootstrapEngine.OPEN3D_FGR, "open3d"),
    (NonRigidEngine.CPD, "probreg"),
    (NonRigidEngine.FILTERREG, "probreg"),
))

_SEEDABLE: Final[frozenset[RegistrationMode]] = frozenset({
    RegistrationMode.MULTISCALE,
    RegistrationMode.COLORED_ICP,
    RegistrationMode.VGICP,
    RegistrationMode.NONRIGID,
})

# --- [TABLES] ---------------------------------------------------------------------------

REG_ARITY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="session.arity", arm="config", defect="session-arity", retriability=TERMINAL, slots=("mode", "members")
)
REG_HOLLOW: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="session.member", arm="config", defect="empty-cloud", retriability=TERMINAL, slots=("mode", "index")
)
REG_TUNING: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="policy", arm="config", defect="tuning-breached", retriability=TERMINAL, slots=("mode", "breached")
)
REG_SEED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="seed", arm="config", defect="seed-refused", retriability=TERMINAL, slots=("mode", "flaw")
)
REG_UNSEEDABLE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="bootstrap", arm="config", defect="unseedable-fine", retriability=TERMINAL, slots=("mode",)
)
REG_NONRIGID: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.REGISTRATION, point="nonrigid", arm="import_", defect="engine-absent", retriability=TERMINAL, slots=("module", "engine")
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([REG_ARITY, REG_HOLLOW, REG_TUNING, REG_SEED, REG_UNSEEDABLE, REG_NONRIGID]))


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RegistrationFault(Exception):
    tag: Literal["unprovisioned"] = tag()
    unprovisioned: str = case()

    def __str__(self) -> str:
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case RegistrationFault(tag="unprovisioned", unprovisioned=capability):
                return capability
            case _ as unreachable:
                assert_never(unreachable)


def _unprovisioned[T](capability: str) -> T:
    raise RegistrationFault(unprovisioned=capability)


# --- [MODELS] ---------------------------------------------------------------------------


class RegistrationPolicy(Struct, frozen=True):
    voxel: float = 0.05
    max_correspondence: float = 0.1
    tukey_k: float = 0.1
    multiscale_iterations: tuple[int, ...] = (50, 30)
    colored_lambda_geometric: float = 0.968
    use_quatro: bool = False
    thr_linearity: float = 1.0
    num_max_corr: int = 5000
    robin_noise_bound_gain: float = 1.0
    solver_noise_bound_gain: float = 0.75
    edge_uncertain_below_fitness: float = 0.5
    preference_loop_closure: float = 0.25
    nonrigid: NonRigidEngine = NonRigidEngine.CPD
    objective: FilterRegObjective = FilterRegObjective.PT2PL
    outlier_weight: float = 0.1
    em_iterations: int = 50
    em_tolerance: float = 0.001
    rmse_ceiling: float = 0.01
    misfit_ceiling: float = 0.7
    deformation_ceiling: float = 0.02

    @property
    def voxel_schedule(self) -> tuple[tuple[float, float], ...]:
        return ((self.voxel * 4, self.max_correspondence), (self.voxel, self.voxel))

    @property
    def mixture_variance(self) -> float:
        return self.voxel * self.voxel

    @property
    def spec(self) -> bytes:
        return (
            f"{self.voxel:.17g}|{self.max_correspondence:.17g}|{self.tukey_k:.17g}"
            f"|{self.colored_lambda_geometric:.17g}|{self.multiscale_iterations}"
            f"|{int(self.use_quatro)}:{self.thr_linearity:.17g}:{self.num_max_corr}"
            f":{self.robin_noise_bound_gain:.17g}:{self.solver_noise_bound_gain:.17g}"
            f"|{self.edge_uncertain_below_fitness:.17g}:{self.preference_loop_closure:.17g}"
            f"|{self.nonrigid.value}:{self.objective.value}:{self.outlier_weight:.17g}"
            f":{self.em_iterations}:{self.em_tolerance:.17g}"
            f"|{self.rmse_ceiling:.17g}:{self.misfit_ceiling:.17g}:{self.deformation_ceiling:.17g}"
        ).encode()

    def divergences(self) -> "Block[str]":
        positive = {
            "voxel": self.voxel,
            "max_correspondence": self.max_correspondence,
            "tukey_k": self.tukey_k,
            "thr_linearity": self.thr_linearity,
            "robin_noise_bound_gain": self.robin_noise_bound_gain,
            "solver_noise_bound_gain": self.solver_noise_bound_gain,
            "em_tolerance": self.em_tolerance,
            "rmse_ceiling": self.rmse_ceiling,
            "misfit_ceiling": self.misfit_ceiling,
            "deformation_ceiling": self.deformation_ceiling,
            "colored_lambda_geometric": self.colored_lambda_geometric,
        }
        unit = {
            "colored_lambda_geometric": self.colored_lambda_geometric,
            "edge_uncertain_below_fitness": self.edge_uncertain_below_fitness,
            "preference_loop_closure": self.preference_loop_closure,
            "outlier_weight": self.outlier_weight,
        }
        counts = {"em_iterations": self.em_iterations, "num_max_corr": self.num_max_corr}
        return Block.of_seq([
            *(f"non-positive:{name}={value!r}" for name, value in positive.items() if not (np.isfinite(value) and value > 0.0)),
            *(f"off-unit:{name}={value!r}" for name, value in unit.items() if not (np.isfinite(value) and 0.0 <= value <= 1.0)),
            *(("share-at-one:outlier_weight",) if self.outlier_weight == 1.0 else ()),
            *(f"no-pass:{name}={value}" for name, value in counts.items() if value < 1),
            *(f"no-pass:multiscale_iterations[{index}]={budget}" for index, budget in enumerate(self.multiscale_iterations) if budget < 1),
            *(
                (f"scale-arity:multiscale_iterations={len(self.multiscale_iterations)}!={len(self.voxel_schedule)}",)
                if len(self.multiscale_iterations) != len(self.voxel_schedule)
                else ()
            ),
        ])


class DeformationField(Struct, frozen=True, gc=False):
    warped: np.ndarray
    magnitude: np.ndarray
    sigma2: float
    q: float
    objective: Option[str] = Nothing

    @staticmethod
    def of(posed: np.ndarray, warped: np.ndarray, sigma2: float, q: float, objective: Option[str] = Nothing) -> "DeformationField":
        return DeformationField(warped, np.linalg.norm(warped - posed, axis=1), float(sigma2), float(q), objective)

    @property
    def extreme(self) -> float:
        return float(self.magnitude.max())

    def cloud(self, source: Cloud) -> Cloud:
        return replace(source, positions=self.warped, normals=np.empty((0, 3)))

    def facts(self) -> dict[str, object]:
        median, upper = np.percentile(self.magnitude, (50.0, 95.0))
        return {
            "deformation_median": float(median),
            "deformation_p95": float(upper),
            "deformation_max": self.extreme,
            "deformation_mean": float(self.magnitude.mean()),
            "sigma2": self.sigma2,
            "q": self.q,
        } | self.objective.map(lambda held: {"objective": held}).default_value({})


class RegistrationResult(Struct, frozen=True, gc=False):
    mode: RegistrationMode
    engine: Option[BootstrapEngine | NonRigidEngine]
    transform: tuple[float, ...]
    poses: tuple[tuple[float, ...], ...]
    fitness: float
    inlier_rmse: Option[float]
    inliers: int
    session_key: Option[ContentKey] = Nothing
    rotation_inliers: Option[int] = Nothing
    timings: dict[str, float] = field(default_factory=dict)
    deformation: Option[DeformationField] = Nothing
    seed: tuple[float, ...] = ()

    @staticmethod
    def of(
        mode: RegistrationMode,
        transform: np.ndarray,
        fitness: float,
        inlier_rmse: Option[float],
        inliers: int,
        *,
        engine: Option[BootstrapEngine | NonRigidEngine] = Nothing,
        poses: tuple[tuple[float, ...], ...] = (),
        rotation_inliers: Option[int] = Nothing,
        timings: dict[str, float] | None = None,
        deformation: Option[DeformationField] = Nothing,
    ) -> "RegistrationResult":
        flat = tuple(np.ravel(np.asarray(transform)))
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), inlier_rmse, int(inliers),
            Nothing, rotation_inliers.map(int), timings or {}, deformation,
        )

    def keyed(self, session: "RegistrationSession", seed: "Option[tuple[float, ...]]" = Nothing) -> "RegistrationResult":
        bands = Block.of_seq(session).collect(lambda cloud: (cloud.digest.memory, cloud.colors.tobytes(), cloud.normals.tobytes()))
        return replace(
            self,
            session_key=Some(ContentIdentity.key("scan-session", IdentitySource(parts=tuple(bands)))),
            seed=tuple(float(value) for value in np.ravel(_seeded(seed))),
        )

    @staticmethod
    def _from_tensor(
        mode: RegistrationMode, reg: "o3d.t.pipelines.registration.RegistrationResult", source: "o3d.t.geometry.PointCloud"
    ) -> "RegistrationResult":
        return RegistrationResult.of(
            mode, reg.transformation.numpy(), reg.fitness, Some(float(reg.inlier_rmse)), int(reg.fitness * source.point.positions.shape[0])
        )

    def facts(self) -> dict[str, object]:
        measured: Block[tuple[str, Option[object]]] = Block.of_seq([
            ("engine", self.engine.map(lambda held: held.value)),
            ("inlier_rmse", self.inlier_rmse),
            ("rotation_inliers", self.rotation_inliers),
        ])
        return (
            {"mode": self.mode.value, "fitness": self.fitness, "inliers": self.inliers}
            | dict(measured.choose(lambda slot: slot[1].map(lambda held: (slot[0], held))))
            | self.deformation.map(lambda field_: field_.facts()).default_value({})
            | {f"t.{stage}": seconds for stage, seconds in self.timings.items()}
        )

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.registration", ("emitted", self.mode.value, self.facts())),)

    def spec(self, policy: RegistrationPolicy) -> bytes:
        posed = ",".join(f"{value:.17g}" for value in self.seed)
        engine = self.engine.map(lambda held: held.value).default_value("")
        keyed = self.session_key.map(lambda held: held.hex).default_value("")
        return f"{self.mode.value}|{engine}|{keyed}|{posed}|".encode() + policy.spec

    def graduates(self, policy: RegistrationPolicy) -> GeometryHandoff:
        warp = self.deformation.map(lambda held: {"deformation_max": held.extreme}).default_value({})
        measured = {"misfit": 1.0 - self.fitness} | self.inlier_rmse.map(lambda held: {"inlier_rmse": held}).default_value({}) | warp
        ceilings = (
            {"misfit": policy.misfit_ceiling}
            | self.inlier_rmse.map(lambda _: {"inlier_rmse": policy.rmse_ceiling}).default_value({})
            | ({} if not warp else {"deformation_max": policy.deformation_ceiling})
        )
        return GeometryHandoff.of(
            GeometrySubject.REGISTRATION_TRANSFORM,
            evidence_key(GeometrySubject.REGISTRATION_TRANSFORM, self.spec(policy)),
            measured,
            ceilings,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _engine[E: (BootstrapEngine, NonRigidEngine)](*wanted: E) -> "Option[E]":
    admitted = frozenset(wanted)
    return _ENGINE_MODULE.choose(lambda row: Some(row[0]) if row[0] in admitted and find_spec(row[1]) is not None else Nothing).try_head()


def _seeded(seed: "Option[tuple[float, ...]]") -> np.ndarray:
    return seed.map(lambda flat: np.reshape(np.asarray(flat, dtype=np.float64), (4, 4))).default_value(np.eye(4))


def _tukey(policy: RegistrationPolicy) -> "o3d.t.pipelines.registration.robust_kernel.RobustKernel":
    rk = o3d.t.pipelines.registration.robust_kernel
    return rk.RobustKernel(rk.RobustKernelMethod.TukeyLoss, policy.tukey_k)


def _homogeneous(rotation: np.ndarray, translation: np.ndarray) -> np.ndarray:
    transform = np.eye(4)
    transform[:3, :3] = rotation
    transform[:3, 3] = translation
    return transform


def _bootstrap(source: Cloud, target: Cloud, engine: BootstrapEngine, policy: RegistrationPolicy) -> RegistrationResult:
    match engine:
        case BootstrapEngine.KISS_MATCHER:
            config = kiss_matcher.KISSMatcherConfig(
                voxel_size=policy.voxel,
                use_quatro=policy.use_quatro,
                thr_linearity=policy.thr_linearity,
                num_max_corr=policy.num_max_corr,
                robin_noise_bound_gain=policy.robin_noise_bound_gain,
                solver_noise_bound_gain=policy.solver_noise_bound_gain,
            )
            matcher = kiss_matcher.KISSMatcher(config)
            src = np.asarray(source.positions, dtype=np.float64).T
            src_matched, tgt_matched = matcher.match(src, np.asarray(target.positions, dtype=np.float64).T)
            solution = matcher.prune_and_solve(src_matched, tgt_matched)
            inliers = matcher.get_num_final_inliers() if solution.valid else 0
            return RegistrationResult.of(
                RegistrationMode.GLOBAL,
                _homogeneous(np.asarray(solution.rotation), np.asarray(solution.translation)),
                float(inliers) / max(src.shape[1], 1),
                Nothing,
                inliers,
                engine=Some(BootstrapEngine.KISS_MATCHER),
                rotation_inliers=Some(matcher.get_num_rotation_inliers()) if solution.valid else Nothing,
                timings={
                    "extraction": matcher.get_extraction_time(),
                    "matching": matcher.get_matching_time(),
                    "rejection": matcher.get_rejection_time(),
                    "solver": matcher.get_solver_time(),
                    "processing": matcher.get_processing_time(),
                },
            )
        case BootstrapEngine.OPEN3D_FGR:
            reg = o3d.pipelines.registration
            search = o3d.geometry.KDTreeSearchParamHybrid(policy.voxel * 5, 100)
            normals = o3d.geometry.KDTreeSearchParamHybrid(policy.voxel * 2, 30)
            down = tuple(
                cloud.estimate_normals(normals) or cloud
                for cloud in (s.legacy().voxel_down_sample(policy.voxel) for s in (source, target))
            )
            features = tuple(reg.compute_fpfh_feature(cloud, search) for cloud in down)
            result = reg.registration_fgr_based_on_feature_matching(down[0], down[1], features[0], features[1])
            return RegistrationResult.of(
                RegistrationMode.GLOBAL,
                np.asarray(result.transformation),
                result.fitness,
                Some(float(result.inlier_rmse)),
                len(result.correspondence_set),
                engine=Some(BootstrapEngine.OPEN3D_FGR),
            )
        case unreachable:
            assert_never(unreachable)


def _nonrigid(source: Cloud, target: Cloud, policy: RegistrationPolicy, initial: np.ndarray) -> RegistrationResult:
    posed = source.positions @ initial[:3, :3].T + initial[:3, 3]
    match policy.nonrigid:
        case NonRigidEngine.CPD:
            resolved = None
            estimated = cpd.registration_cpd(
                posed, target.positions, tf_type_name="nonrigid", w=policy.outlier_weight, maxiter=policy.em_iterations, tol=policy.em_tolerance
            )
        case NonRigidEngine.FILTERREG:
            normals = target.normals if target.normals.size else None
            resolved = policy.objective if normals is not None else FilterRegObjective.PT2PT
            estimated = filterreg.registration_filterreg(
                posed,
                target.positions,
                target_normals=normals,
                objective_type=resolved.value,
                sigma2=policy.mixture_variance,
                w=policy.outlier_weight,
                maxiter=policy.em_iterations,
                tol=policy.em_tolerance,
            )
        case unreachable:
            assert_never(unreachable)
    warped = np.asarray(estimated.transformation.transform(posed), dtype=np.float64)
    field = DeformationField.of(posed, warped, estimated.sigma2, estimated.q, Option.of_obj(resolved).map(lambda held: held.value))
    evaluated = o3d.pipelines.registration.evaluate_registration(
        field.cloud(source).legacy(), target.legacy(), policy.max_correspondence, np.eye(4)
    )
    return RegistrationResult.of(
        RegistrationMode.NONRIGID,
        initial,
        evaluated.fitness,
        Some(float(evaluated.inlier_rmse)),
        len(evaluated.correspondence_set),
        engine=Some(policy.nonrigid),
        deformation=Some(field),
    )


def _edge(
    reg: "o3d.pipelines.registration",
    legacy: tuple["o3d.geometry.PointCloud", ...],
    i: int,
    total: int,
    solution: RegistrationResult,
    policy: RegistrationPolicy,
    tap: "Queue[PulseFact | None]",
) -> tuple["o3d.pipelines.registration.PoseGraphNode", "o3d.pipelines.registration.PoseGraphEdge"]:
    pulsed(tap, GeometryPulse.REGISTRATION, StageMark(stage=RegistrationStage.EDGE.value, done=i + 1, total=Some(total)))
    pose = np.reshape(np.asarray(solution.transform), (4, 4))
    fitness = reg.evaluate_registration(legacy[i + 1], legacy[0], policy.max_correspondence, pose).fitness
    node = reg.PoseGraphNode(pose)
    edge = reg.PoseGraphEdge(i + 1, 0, pose, uncertain=fitness < policy.edge_uncertain_below_fitness)
    return node, edge


def _multiway(session: RegistrationSession, policy: RegistrationPolicy, tap: "Queue[PulseFact | None]") -> RegistrationResult:
    reg = o3d.pipelines.registration
    engine = _engine(*BootstrapEngine).default_with(lambda: _unprovisioned("bootstrap"))
    legacy = tuple(cloud.legacy() for cloud in session)
    pairs = Block.of_seq(session[1:]).mapi(
        lambda i, cloud: _edge(reg, legacy, i, len(session) - 1, _bootstrap(cloud, session[0], engine, policy), policy, tap)
    )
    graph = reg.PoseGraph()
    graph.nodes.append(reg.PoseGraphNode(np.eye(4)))
    for node, edge in pairs:
        graph.nodes.append(node)
        graph.edges.append(edge)
    reg.global_optimization(
        graph,
        reg.GlobalOptimizationLevenbergMarquardt(),
        reg.GlobalOptimizationConvergenceCriteria(),
        reg.GlobalOptimizationOption(
            max_correspondence_distance=policy.max_correspondence,
            preference_loop_closure=policy.preference_loop_closure,
            reference_node=0,
        ),
    )
    poses = tuple(np.asarray(node.pose) for node in graph.nodes)
    final = reg.evaluate_registration(legacy[-1], legacy[0], policy.max_correspondence, poses[-1])
    return RegistrationResult.of(
        RegistrationMode.MULTIWAY,
        poses[-1],
        final.fitness,
        Some(float(final.inlier_rmse)),
        len(final.correspondence_set),
        engine=Some(engine),
        poses=tuple(tuple(np.ravel(pose)) for pose in poses),
    )


def _register_kernel(
    session: RegistrationSession,
    mode: RegistrationMode,
    policy: RegistrationPolicy,
    threads: int,
    seed: "Option[tuple[float, ...]]",
    tap: "Queue[PulseFact | None]",
) -> RegistrationResult:
    source, target = session[0], session[1]
    initial = _seeded(seed)
    pulsed(tap, GeometryPulse.REGISTRATION, StageMark(stage=RegistrationStage.SOLVE.value, done=0, total=Some(1)))
    reg_t = o3d.t.pipelines.registration
    match mode:
        case RegistrationMode.VGICP:
            result = small_gicp.align(
                target.positions,
                source.positions,
                init_T_target_source=initial,
                registration_type="VGICP",
                downsampling_resolution=policy.voxel,
                num_threads=threads,
            )
            return RegistrationResult.of(
                mode,
                result.T_target_source,
                result.num_inliers / max(len(source), 1),
                Some(float(result.error)),
                result.num_inliers,
                timings={"iterations": float(result.iterations)},
            )
        case RegistrationMode.MULTISCALE:
            voxels, corrs = zip(*policy.voxel_schedule)
            src_t = source.tensor()
            reg = reg_t.multi_scale_icp(
                src_t,
                target.tensor(),
                o3d.utility.DoubleVector(voxels),
                [reg_t.ICPConvergenceCriteria(max_iteration=it) for it in policy.multiscale_iterations],
                o3d.utility.DoubleVector(corrs),
                init_source_to_target=o3d.core.Tensor(initial),
                estimation_method=reg_t.TransformationEstimationPointToPlane(_tukey(policy)),
            )
            return RegistrationResult._from_tensor(mode, reg, src_t)
        case RegistrationMode.COLORED_ICP:
            colored = reg_t.TransformationEstimationForColoredICP(policy.colored_lambda_geometric, _tukey(policy))
            src_t = source.tensor()
            reg = reg_t.icp(
                src_t, target.tensor(), policy.max_correspondence, init_source_to_target=o3d.core.Tensor(initial), estimation_method=colored
            )
            return RegistrationResult._from_tensor(mode, reg, src_t)
        case RegistrationMode.GLOBAL:
            return _bootstrap(source, target, _engine(*BootstrapEngine).default_with(lambda: _unprovisioned("bootstrap")), policy)
        case RegistrationMode.NONRIGID:
            return _nonrigid(source, target, policy, initial)
        case RegistrationMode.MULTIWAY:
            return _multiway(session, policy, tap)
        case unreachable:
            assert_never(unreachable)


def _distributed(result: RegistrationResult, composition: ScopeKey) -> RegistrationResult:
    charter_record(GeometrySubject.REGISTRATION_TRANSFORM, result.facts(), composition=composition)
    return result


# --- [SERVICES] -------------------------------------------------------------------------


class ScanRegistration(Struct, frozen=True):
    lane: LanePolicy
    policy: RegistrationPolicy = RegistrationPolicy()
    composition: ScopeKey = DEFAULT_SCOPE

    async def register(
        self, session: RegistrationSession, mode: RegistrationMode, seed: "Option[RegistrationResult]" = Nothing
    ) -> "RuntimeRail[RegistrationResult]":
        async def dispatch() -> "RuntimeRail[RegistrationResult]":
            if len(session) < 2:
                return Error(REG_ARITY.raised(mode, str(len(session))))
            if (hollow := next((index for index, cloud in enumerate(session) if not len(cloud)), None)) is not None:
                return Error(REG_HOLLOW.raised(mode, str(hollow)))
            if not (breached := self.policy.divergences()).is_empty():
                return Error(REG_TUNING.raised(mode, ";".join(breached)))
            flawed = seed.bind(
                lambda held: Some("unconsumed")
                if mode not in _SEEDABLE
                else Some(f"shape:{len(held.transform)}")
                if len(held.transform) != 16 or not bool(np.isfinite(held.transform).all())
                else Nothing
            )
            if flawed.is_some():
                return Error(REG_SEED.raised(mode, flawed.default_value("")))
            if mode is RegistrationMode.NONRIGID and _engine(self.policy.nonrigid).is_none():
                return Error(REG_NONRIGID.raised("probreg", self.policy.nonrigid.value))
            coarse = seed.map(lambda held: held.transform)
            async def granted(grant: LaneGrant) -> "RuntimeRail[RegistrationResult]":
                return await self.lane.offload(
                    Kernel.of(_register_kernel, KernelTrait.HOSTILE),
                    session,
                    mode,
                    self.policy,
                    grant.width,
                    coarse,
                    self.lane.pulses.tap,
                )

            offloaded = await self.lane.whole(granted)
            return offloaded.map(lambda result: _distributed(result.keyed(session, coarse), self.composition))

        return await evidence_run(EvidenceScope.SCAN_REGISTRATION, f"register.{mode}", dispatch, composition=self.composition)

    async def bootstrapped(self, session: RegistrationSession, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        if mode not in _SEEDABLE:
            return Error(REG_UNSEEDABLE.raised(mode))
        match await self.register(session, RegistrationMode.GLOBAL):
            case Result(tag="ok", ok=coarse):
                return await self.register(session, mode, Some(coarse))
            case Result(tag="error") as refused:
                return refused

    def bench(self, session: RegistrationSession, mode: RegistrationMode, *, rounds: int = 32, warmup: int = 4) -> "RuntimeRail[BenchmarkReceipt]":
        return bench_seam(
            bench_subject(EvidenceScope.SCAN_REGISTRATION, mode, f"p{len(session[0])}"),
            partial(self.register, session, mode),
            rounds=rounds,
            warmup=warmup,
            composition=self.composition,
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
