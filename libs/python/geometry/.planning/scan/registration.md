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
- Auto: `_engine` folds ONE ordered provider map through one `find_spec` probe over BOTH engine families and answers `Option` — the caller names the members it admits, the whole bootstrap family or the single non-rigid row the policy picked, and the head that resolves wins. Every one of `kiss_matcher`, `open3d`, and `probreg` is interpreter-marked, so a probe that tests one and falls through to another as a floor reports a capability that floor cannot deliver; a `Nothing` refuses typed at the arm that needs it rather than raising `ModuleNotFoundError` inside a worker. The `NONRIGID` gate reads that same map PARENT-SIDE in `register`, before the crossing, onto `Error(BoundaryFault(import_=))` naming `probreg` — module presence is identical on every floor of the one shared venv, so the probe picks a capability tier and never an offload route, and the kernel's non-rigid arm runs past a decision already made. Every arm (`GLOBAL`, each `MULTIWAY` edge, `NONRIGID`) reuses that one map; the tensor arms share the `_tukey` robust kernel and the `_from_tensor` projector rather than re-reading the `open3d` result per arm.
- Auto: the coarse pose reaches EVERY fine arm through that arm's OWN initial-transform argument — the open3d tensor `init_source_to_target` slot on `icp` and `multi_scale_icp`, the `small_gicp.align` `init_T_target_source` slot — so `_seeded` builds one 4x4 and each solver seeds its correspondence search directly and returns the full source-to-target pose it already composed. The deleted mechanism is a whole-cloud copy per solve plus its normal rotation plus a post-solve matrix product, all of which the provider performs inside its own iteration for free. Every correspondence-search arm publishes that slot; the `NONRIGID` arm is the ONE stated exception, and it pays the pre-pose price knowingly: `registration_cpd` and `registration_filterreg` take the outlier weight, the iteration budget, the tolerance, and the objective term alone, so the seed applies ONCE at the arm's own admission as a pre-posed source array. That copy is bounded by the arm's shape — one EM pass over one pair, so one copy, where a multi-scale arm would have paid one per scale — and the pose it applied rides back in the result's own `transform` slot as the arm's rigid component, so the deformation field measures exactly what the pose could not explain. A local ICP started from identity diverges on any pair whose gross misalignment exceeds `max_correspondence`, which is the whole reason the `GLOBAL` arm exists and why an unseeded arm passes the identity those slots already default to rather than skipping the argument.
- Auto: intra-kernel parallelism binds from `LanePolicy.capacity`, threaded as a trailing kernel argument beside the pulse tap — the daemon's law, one folder, one answer — because a literal thread count inside each of `capacity` concurrent process slots oversubscribes the machine by exactly that factor.
- Receipt: emission is the weave's harvest — the conforming `RegistrationResult.contribute` streams once on the cleared `Ok`, never an inline emit or page-local `@receipted` leg. `inlier_rmse` is OPTIONAL because the `KISS_MATCHER` arm measures none, so `graduates` derives its ceiling roster PER MEASURE: the `1 - fitness` misfit is graded on every arm and the RMSE bar joins only where an RMSE exists, since a fabricated `0.0` clears every ceiling and graduates a coarse pose as a converged alignment. The `NONRIGID` arm measures both on the WARPED cloud against the target through the same `evaluate_registration` fold the multiway edges read, so a probabilistic warp grades on the identical bars rather than on a mixture objective no other arm shares; `sigma2` and `q` ride the receipt as the EM's own convergence evidence beside them — with the EFFECTIVE FilterReg objective where that arm ran, since the pt2pt fallback on a normal-less target is solver evidence the policy bytes cannot recover — and the deformation magnitudes ride as a QUANTILE ladder — median, p95, extremum, mean — because a receipt never grows with rows and one extremum never replaces a distribution, the per-point field itself being the payload `scan/deviation#DEVIATION` consumes. `deformation_max` joins the graduation roster on that same per-measure derivation, against the `deformation_ceiling` monitoring bar: a field past it is the structural-deformation alarm, and an arm that recovered no field is never graded on a bar it was in no position to measure. That misfit rides the graduation owner's single `_admit` residual-over-ceiling direction, so no second admission direction is minted here.
- Packages: `kiss_matcher`, `open3d`, `small_gicp`, `probreg` (the compiled registration backends, each a module-scope `lazy import`/`lazy from` so the marked distributions stay cold until their own arm runs — never an eager module-top import and never a function-local one the module-top roster hides), `numpy` (transform assembly via `np.eye`/`np.ravel`/`np.reshape`, never the uncatalogued `np.identity`/`ndarray.flatten`), `expression` (`Block.mapi` the per-edge multiway fold), `msgspec`, and the geometry graduation spine (`evidence_run`/`GeometryHandoff`/`GeometrySubject`, `charter_record` the charter measure authority, `bench_seam`/`bench_terminal`, `GeometryPulse`/`PulseBeat`) and runtime rails per the fence imports.
- Growth: a new registration engine is one `RegistrationMode` row, one kernel arm inheriting the carrier pre-pose with no seeding edit, and — where its solver consumes a seed — one `_SEEDABLE` member; a new bootstrap backend is one `BootstrapEngine` member, one `_ENGINE_MODULE` probe row, and one `_bootstrap` arm; a new probabilistic estimator is one `NonRigidEngine` member, its `_ENGINE_MODULE` row, and one `_nonrigid` arm answering the same `DeformationField`; a feature-space correspondence is one `feature_fn` policy row on the `FILTERREG` arm; a stricter graduation bar is a `RegistrationPolicy` ceiling the caller passes. `registration_ransac_based_on_feature_matching` is the named next `BootstrapEngine` row when a scene defeats both standing engines.
- Boundary: the cleaned input `Cloud` is `scan/ingestion#INGESTION`'s product and carrier mint; deviation against a reference is `scan/deviation#DEVIATION`; surface reconstruction is `scan/reconstruction#RECONSTRUCTION`. The deformation field mints HERE and crosses to the deviation owner, which partitions it against its own signed band and never re-solves a warp; a live `probreg` `Transformation` never leaves this kernel, because it is an `open3d`-coupled native handle the pickle seam cannot carry. No mesh repair, tessellation, or durable store here.

```python signature
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
    GeometryPulse,
    GeometrySubject,
    PulseBeat,
    bench_seam,
    bench_subject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# the three compiled registration backends, each interpreter-marked: the module-scope proxy keeps every one cold
# until its own arm runs, so a loop floor importing this module for the `RegistrationMode` vocabulary loads none of
# them and an absent provider surfaces through `_engine`'s typed refusal rather than an import at module load.
lazy import kiss_matcher
lazy import open3d as o3d
lazy import small_gicp

# the probabilistic band binds its two entrypoint modules, not the package: `probreg` pulls `open3d` transitively at
# import, so the proxy keeps BOTH cold until the non-rigid arm runs and the parent-side gate answers absence off
# `find_spec` alone rather than off an import that would already have paid the native band.
lazy from probreg import cpd, filterreg

# --- [TYPES] ----------------------------------------------------------------------------


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
    # probreg's own `objective_type` spelling: the value crosses the boundary verbatim, so no mapping table stands
    # between the policy row and the estimator's discriminant.
    PT2PT = "pt2pt"
    PT2PL = "pt2pl"


# >=2 Cloud carriers: pairwise modes read [0]/[1], MULTIWAY needs two for one edge; arrays cross, o3d rebuilds worker-side
type RegistrationSession = tuple[Cloud, Cloud, *tuple[Cloud, ...]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# engine -> probe MODULE across BOTH families, ordered by precedence within each; `_engine` walks it once over the
# members its caller admits and takes the head that resolves, so a floor carrying none answers the typed refusal
# instead of importing into a worker-side crash. Both non-rigid rows probe the ONE `probreg` distribution, because the
# estimator choice is a policy coordinate inside a single provider whose `cpd` and `filterreg` modules ship
# unconditionally, never a second provider to fall back to — and probing `probreg.cpd` is unspellable besides,
# since `find_spec` on a submodule imports the parent package and pays the whole transitive native band this
# parent-side gate exists to defer.
_ENGINE_MODULE: Final[Block[tuple[BootstrapEngine | NonRigidEngine, str]]] = Block.of_seq((
    (BootstrapEngine.KISS_MATCHER, "kiss_matcher"),
    (BootstrapEngine.OPEN3D_FGR, "open3d"),
    (NonRigidEngine.CPD, "probreg"),
    (NonRigidEngine.FILTERREG, "probreg"),
))

# modes whose solver CONSUMES a coarse seed — an `init_source_to_target`/`init_T_target_source` slot on the
# tensor and VGICP arms, the one knowing pre-pose on `NONRIGID`. `GLOBAL` and `MULTIWAY` stay off the roster because
# they are initialization-free by construction (every multiway edge runs its own coarse solve), so a seed handed to
# them would steer nothing yet join the evidence key — identity forked over an input the solve never read — and
# `register` refuses it typed instead; a new seed-consuming mode is one member here, zero admission edits.
_SEEDABLE: Final[frozenset[RegistrationMode]] = frozenset({
    RegistrationMode.MULTISCALE,
    RegistrationMode.COLORED_ICP,
    RegistrationMode.VGICP,
    RegistrationMode.NONRIGID,
})


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RegistrationFault(Exception):
    # raised INTO the lane's `async_boundary`, never a domain `raise ValueError` the lane re-wraps; the lane's fence
    # names the absent capability at the seam where a bare ModuleNotFoundError would name a private module path.
    tag: Literal["unprovisioned"] = tag()
    unprovisioned: str = case()  # the capability whose WHOLE provider set resolved absent at the interpreter floor


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
    outlier_weight: float = 0.1  # the mixture's expected share of source points with no target counterpart
    em_iterations: int = 50
    em_tolerance: float = 0.001
    rmse_ceiling: float = 0.01  # policy-row alignment bar, never a module Final
    misfit_ceiling: float = 0.7  # the 1-fitness bar
    deformation_ceiling: float = 0.02  # the bar past which a recovered field reads as structural deformation, not solve noise

    @property
    def voxel_schedule(self) -> tuple[tuple[float, float], ...]:
        return ((self.voxel * 4, self.max_correspondence), (self.voxel, self.voxel))

    @property
    def mixture_variance(self) -> float:
        # the EM's INITIAL variance seeded at the session's own sampling scale: a variance is squared distance, so
        # the voxel edge squared starts the mixture where the clouds were conditioned rather than at a whole-cloud
        # estimate the outlier tail inflates, and no second length literal enters the page to say it.
        return self.voxel * self.voxel

    @property
    def spec(self) -> bytes:
        # tuning bytes DEFINE an alignment: two runs at different voxel, correspondence, or robust-kernel
        # bars are two distinct pieces of evidence, so the policy joins the crossing key rather than averaging into
        # it — and the preimage is COMPLETE over the behavior-affecting rows, the bootstrap solver's config block
        # and the multiway pose-graph bars included, because a knob that steers a solve yet skips the preimage lets
        # two different alignments share one key. The graduation ceilings join the same preimage — they are the
        # verdict inputs `graduates` grades against, so one solve graded under two ceiling sets is two pieces of
        # evidence, never one key carrying contradictory verdict rows.
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
        # ONE admission fold naming EVERY breached bar at once, read parent-side in `register` before any provider
        # or graduation work: no native arm refuses these gates itself — an out-of-domain outlier share or a NaN
        # tolerance converges the EM on garbage, and a non-finite ceiling grades every solve one way — and
        # finiteness folds into each range check because an infinity clears a bare inequality. Every length, gain,
        # tolerance, and ceiling proves strictly positive (`colored_lambda_geometric` additionally at most one, the
        # estimator's geometric blend), the unit fractions prove [0, 1], `outlier_weight` proves the mixture's own
        # [0, 1) probability domain — zero is the lawful no-outlier mixture, one is the degenerate all-outlier one —
        # iteration budgets prove at least one whole pass, and the multiscale budget list proves one criterion
        # per derived scale, the arity `multi_scale_icp` requires of its parallel vectors.
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
    # the SEALED non-rigid product: a live `probreg` `Transformation` is an open3d-coupled native handle no pickler
    # carries, so the warp is applied INSIDE the kernel and only arrays cross — the same law the `Cloud` carrier
    # holds for clouds. `magnitude` aligns index-for-index with the source point order, which is what lets the
    # deviation owner partition its own per-point signed band against it with no correspondence search of its own.
    warped: np.ndarray  # (N, 3) float64 — the pre-posed source warped through the recovered transformation
    magnitude: np.ndarray  # (N,) float64 — per-point displacement magnitude the warp introduced past the pose
    sigma2: float  # the mixture's final variance: the EM's own residual scale
    q: float  # the final log-likelihood objective the estimator converged on
    objective: str | None = None  # the EFFECTIVE FilterReg geometry term the solve ran; None off the CPD arm

    @staticmethod
    def of(posed: np.ndarray, warped: np.ndarray, sigma2: float, q: float, objective: str | None = None) -> "DeformationField":
        # the displacement is measured against the POSED source, never the raw one: the seed pose is the arm's rigid
        # component and folding it into the field would report a gross misalignment as structural deformation.
        return DeformationField(warped, np.linalg.norm(warped - posed, axis=1), float(sigma2), float(q), objective)

    @property
    def extreme(self) -> float:
        # ONE authority for the worst displacement: the receipt fact and the graduation measure read this, so a
        # ledger row and a ceiling verdict can never disagree about the same number.
        return float(self.magnitude.max())

    def cloud(self, source: Cloud) -> Cloud:
        # the warped source as the folder's own array carrier. The color band rides the warp unchanged and the
        # NORMAL band DROPS: a non-rigid warp turns every local frame, so a carried normal set is stale evidence a
        # point-to-plane consumer would read as truth, and an empty band re-estimates where its own solver needs it.
        return replace(source, positions=self.warped, normals=np.empty((0, 3)))

    def facts(self) -> dict[str, object]:
        # the field as a DISTRIBUTION, never one extremum: a quantile ladder of fixed width plus the mixture's own
        # convergence evidence, so the receipt reads how the deformation is shaped without growing with the cloud.
        # The per-point rows are the PAYLOAD the deviation split consumes, so they never enter a receipt at all.
        median, upper = np.percentile(self.magnitude, (50.0, 95.0))
        return {
            "deformation_median": float(median),
            "deformation_p95": float(upper),
            "deformation_max": self.extreme,
            "deformation_mean": float(self.magnitude.mean()),
            "sigma2": self.sigma2,
            "q": self.q,
        } | ({} if self.objective is None else {"objective": self.objective})


class RegistrationResult(Struct, frozen=True, gc=False):
    # `inlier_rmse` is OPTIONAL: the TEASER-style global solver reports inlier counts and stage timings and measures
    # no residual, so absence is spelled rather than filled with a zero every ceiling clears. `deformation` is the
    # same law one axis over: only the arms that recover a warp fill it, and ONE result carrier serves every mode
    # rather than a non-rigid twin whose consumers would have to discriminate on shape.
    mode: RegistrationMode
    engine: BootstrapEngine | NonRigidEngine | None
    transform: tuple[float, ...]
    poses: tuple[tuple[float, ...], ...]
    fitness: float
    inlier_rmse: float | None
    inliers: int
    session_key: ContentKey | None = None  # digest over the session's cloud digests; the evidence key's other half
    rotation_inliers: int = 0
    timings: dict[str, float] = field(default_factory=dict)
    deformation: DeformationField | None = None
    seed: tuple[float, ...] = ()  # the RESOLVED 4x4 the solve started from, stamped by `keyed`; the identity when unseeded

    @staticmethod
    def of(
        mode: RegistrationMode,
        transform: np.ndarray,
        fitness: float,
        inlier_rmse: float | None,
        inliers: int,
        *,
        engine: BootstrapEngine | NonRigidEngine | None = None,
        poses: tuple[tuple[float, ...], ...] = (),
        rotation_inliers: int = 0,
        timings: dict[str, float] | None = None,
        deformation: DeformationField | None = None,
    ) -> "RegistrationResult":
        flat = tuple(np.ravel(np.asarray(transform)))  # the catalogued row-major flatten
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), inlier_rmse, int(inliers), None, int(rotation_inliers), timings or {}, deformation
        )

    def keyed(self, session: "RegistrationSession", seed: "Option[tuple[float, ...]]" = Nothing) -> "RegistrationResult":
        # `keyed` is the ONE identity-stamping transition: the session digest folds each member cloud's own content
        # key BESIDE its color and normal bytes — the carrier's digest covers positions alone, and the bands steer
        # real solves (COLORED_ICP reads the color band, the FILTERREG pt2pl term reads the target's normals and
        # falls back to pt2pt without them) — so two sessions equal in positions and apart in bands key apart, and the
        # effective objective is a function of the keyed inputs. The seed stamps RESOLVED through the same
        # `_seeded` fold the kernel ran — an absent seed IS the identity every solver slot defaults to, so unseeded
        # and identity-seeded key together while a distinct coarse pose keys apart; the position array is never
        # re-hashed, its carrier digest standing in.
        return replace(
            self,
            session_key=ContentIdentity.key(
                "scan-session", tuple((cloud.digest, cloud.colors.tobytes(), cloud.normals.tobytes()) for cloud in session)
            ),
            seed=tuple(float(value) for value in np.ravel(_seeded(seed))),
        )

    @staticmethod
    def _from_tensor(
        mode: RegistrationMode, reg: "o3d.t.pipelines.registration.RegistrationResult", source: "o3d.t.geometry.PointCloud"
    ) -> "RegistrationResult":
        # open3d `.fitness` is the matched-source fraction; `fitness * |source|` is the inlier estimate. The solve
        # took its seed through `init_source_to_target`, so `reg.transformation` is ALREADY the full
        # source-to-target pose and no composition fold stands between the solver's answer and the receipt.
        return RegistrationResult.of(mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse, int(reg.fitness * source.point.positions.shape[0]))

    def facts(self) -> dict[str, object]:
        # native float/int slots the receipts renderer serializes without a str()/repr() coerce; an unmeasured
        # residual leaves the map entirely, so a dashboard reads absence rather than a perfect zero.
        return (
            {
                "mode": self.mode.value,
                "engine": self.engine.value if self.engine else "",
                "fitness": self.fitness,
                "inliers": self.inliers,
                "rotation_inliers": self.rotation_inliers,
            }
            | ({} if self.inlier_rmse is None else {"inlier_rmse": self.inlier_rmse})
            | ({} if self.deformation is None else self.deformation.facts())
            | {f"t.{stage}": seconds for stage, seconds in self.timings.items()}
        )

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.registration", ("emitted", self.mode.value, self.facts())),)

    def spec(self, policy: RegistrationPolicy) -> bytes:
        # `spec` is the byte projection that DEFINES this evidence: which clouds, which mode, which RESOLVED engine,
        # which seed pose, which tuning — two solves of one session from distinct coarse poses answer distinct
        # alignments, so the resolved seed joins the key exactly as the policy bytes do, and the engine joins it
        # because the ordered probe resolves different solvers on different floors: a KISS-Matcher pose and an FGR
        # pose over one session are two pieces of evidence one engine-blind key would merge.
        posed = ",".join(f"{value:.17g}" for value in self.seed)
        engine = self.engine.value if self.engine else ""
        return f"{self.mode.value}|{engine}|{self.session_key.hex if self.session_key else ''}|{posed}|".encode() + policy.spec

    def graduates(self, policy: RegistrationPolicy) -> GeometryHandoff:
        # ceilings derive PER MEASURE: the inlier-ratio misfit grades every arm, the RMSE bar joins only where the
        # solver measured one, and the deformation bar only where an arm recovered a field — so a coarse global pose
        # is graded on what it computed instead of clearing a residual ceiling vacuously on a placeholder the page
        # never had a reason to mint, and a rigid arm never breaches a monitoring bar it could not measure.
        warp = {} if self.deformation is None else {"deformation_max": self.deformation.extreme}
        measured = {"misfit": 1.0 - self.fitness} | ({} if self.inlier_rmse is None else {"inlier_rmse": self.inlier_rmse}) | warp
        ceilings = (
            {"misfit": policy.misfit_ceiling}
            | ({} if self.inlier_rmse is None else {"inlier_rmse": policy.rmse_ceiling})
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
    # ONE probe over the REAL provider set for BOTH families: the caller spells the members it admits — `*` over the
    # whole bootstrap family for the ordered coarse fallback, the single policy-chosen member for the non-rigid arm
    # — and precedence stays the map's, so no caller re-orders a decision the map already owns. EVERY rostered
    # backend is interpreter-marked, so a probe treating one as an always-available floor reports a capability the
    # floor cannot deliver and the failure lands as a worker-side ImportError instead of a typed refusal at
    # selection. An empty admitted set is unrepresentable in practice because each caller names its family.
    admitted = frozenset(wanted)
    return _ENGINE_MODULE.choose(lambda row: Some(row[0]) if row[0] in admitted and find_spec(row[1]) is not None else Nothing).try_head()


def _seeded(seed: "Option[tuple[float, ...]]") -> np.ndarray:
    # the coarse pose as the PROVIDER's own initial-transform argument. Every correspondence-search arm publishes
    # one — the open3d tensor `icp`/`multi_scale_icp` `init_source_to_target` slot and the `small_gicp.align`
    # `init_T_target_source` slot — so the seed reaches the solver's search instead of a pre-transformed copy of the
    # source cloud, and each arm returns the FULL source-to-target pose with no residual composition to get wrong.
    # The deleted mechanism is exactly that pair: a whole-cloud copy per solve, its normal rotation, and a post-fold
    # matrix product, all of which the provider does inside its own iteration for free. The `NONRIGID` arm is the
    # one arm that must pay it — the EM estimators publish no such slot — and `_nonrigid` pays it there, once, on
    # the one pass it makes. An absent seed is the identity every published slot already defaults to, so seeded and
    # unseeded are ONE expression.
    return seed.map(lambda flat: np.reshape(np.asarray(flat, dtype=np.float64), (4, 4))).default_value(np.eye(4))


def _tukey(policy: RegistrationPolicy) -> "o3d.t.pipelines.registration.robust_kernel.RobustKernel":
    rk = o3d.t.pipelines.registration.robust_kernel
    return rk.RobustKernel(rk.RobustKernelMethod.TukeyLoss, policy.tukey_k)


def _homogeneous(rotation: np.ndarray, translation: np.ndarray) -> np.ndarray:
    transform = np.eye(4)  # catalogued identity creator, never `np.identity`
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
            # `float64 (3, n)` array overload (`match`/`prune_and_solve`), not `estimate`'s `float32 (3, 1)`
            # form, keeps every stage-timing accessor populated; the carrier arrays feed it with no o3d rebuild
            src = np.asarray(source.positions, dtype=np.float64).T
            src_matched, tgt_matched = matcher.match(src, np.asarray(target.positions, dtype=np.float64).T)
            solution = matcher.prune_and_solve(src_matched, tgt_matched)
            inliers = matcher.get_num_final_inliers() if solution.valid else 0
            return RegistrationResult.of(
                RegistrationMode.GLOBAL,
                _homogeneous(np.asarray(solution.rotation), np.asarray(solution.translation)),
                float(inliers) / max(src.shape[1], 1),
                None,  # the TEASER-style solver reports inliers and stage timings, never a residual — absence, not zero
                inliers,
                engine=BootstrapEngine.KISS_MATCHER,
                rotation_inliers=matcher.get_num_rotation_inliers() if solution.valid else 0,
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
            # estimate_normals mutates in place returning None; `or cloud` yields the cloud past that
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
                result.inlier_rmse,
                len(result.correspondence_set),
                engine=BootstrapEngine.OPEN3D_FGR,
            )
        case unreachable:
            assert_never(unreachable)


def _nonrigid(source: Cloud, target: Cloud, policy: RegistrationPolicy, initial: np.ndarray) -> RegistrationResult:
    # the ONE arm that pre-poses. The EM estimators publish no initial-transform slot — they take the outlier
    # weight, the iteration budget, the tolerance, and the objective term — so the seed applies HERE as a pre-posed
    # source array: one copy for the arm's single pass, where a multi-scale arm would have paid one per scale. That
    # pose rides back in the result's `transform` slot as the arm's rigid component, so the field measures exactly
    # what the pose could not explain and a gross misalignment never reads as structural deformation.
    posed = source.positions @ initial[:3, :3].T + initial[:3, 3]
    match policy.nonrigid:
        case NonRigidEngine.CPD:
            # `tf_type_name` IS the deformation family — the Gaussian-RBF transformation whose `transform` warps
            # arbitrary query points — so the algorithm axis stays one entrypoint and a string, never a function family.
            resolved = None
            estimated = cpd.registration_cpd(
                posed, target.positions, tf_type_name="nonrigid", w=policy.outlier_weight, maxiter=policy.em_iterations, tol=policy.em_tolerance
            )
        case NonRigidEngine.FILTERREG:
            # the permutohedral-filter GMM discriminates on the GEOMETRY term, and its point-to-plane arm consumes
            # the target's OWN normal band: a target carrying none resolves pt2pt, because a plane term handed an
            # absent normal set answers a distance to a plane nothing estimated. The EFFECTIVE term binds ONCE —
            # that fallback is solver evidence the policy bytes cannot recover, so it rides the field's own receipt
            # slot beside `sigma2`/`q`.
            normals = target.normals if target.normals.size else None
            resolved = policy.objective if normals is not None else FilterRegObjective.PT2PT
            # EM controls ride ONE policy row both estimators consume — `w`/`maxiter`/`tol` forward here exactly as
            # on the CPD arm, so the knobs `RegistrationPolicy.spec` folds into the evidence key are the knobs the
            # solver ran, never a recorded bar the provider default silently overrode.
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
    # the warp applies IN-KERNEL: `transform` answers an open3d buffer, so `asarray` seals it into an owned float64
    # block and the estimated transformation dies with the worker frame rather than meeting the pickle seam.
    warped = np.asarray(estimated.transformation.transform(posed), dtype=np.float64)
    field = DeformationField.of(posed, warped, estimated.sigma2, estimated.q, None if resolved is None else resolved.value)
    # the EM converges on a mixture objective no other arm shares, so fitness and residual are measured the way
    # every other arm measures them — the WARPED cloud against the target, at identity because the warp is already
    # applied — and the `NONRIGID` row grades on the same two ceilings instead of on a bar of its own.
    evaluated = o3d.pipelines.registration.evaluate_registration(
        field.cloud(source).legacy(), target.legacy(), policy.max_correspondence, np.eye(4)
    )
    return RegistrationResult.of(
        RegistrationMode.NONRIGID,
        initial,  # the pre-applied seed, identity when unseeded: the rigid half of a solve whose other half is the field
        evaluated.fitness,
        evaluated.inlier_rmse,
        len(evaluated.correspondence_set),
        engine=policy.nonrigid,
        deformation=field,
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
    # uncertain = measured fitness vs the policy floor, never hardcoded False; the pose maps cloud(i+1)->cloud(0),
    # so the edge is source=i+1,target=0 — an (0, i+1) edge carries it inverted
    pulsed(tap, GeometryPulse.REGISTRATION, PulseBeat(stage="edge", done=i + 1, total=total))  # per-edge convergence beat, lossy by lane law
    pose = np.reshape(np.asarray(solution.transform), (4, 4))
    fitness = reg.evaluate_registration(legacy[i + 1], legacy[0], policy.max_correspondence, pose).fitness
    node = reg.PoseGraphNode(pose)
    edge = reg.PoseGraphEdge(i + 1, 0, pose, uncertain=fitness < policy.edge_uncertain_below_fitness)
    return node, edge


def _multiway(session: RegistrationSession, policy: RegistrationPolicy, tap: "Queue[PulseFact | None]") -> RegistrationResult:
    reg = o3d.pipelines.registration
    # one engine read; every edge solves on the same bootstrap engine, and an unprovisioned floor refuses typed
    # HERE rather than each edge dying inside the peel.
    engine = _engine(*BootstrapEngine).default_with(lambda: _unprovisioned("bootstrap"))
    legacy = tuple(cloud.legacy() for cloud in session)  # one worker-side rebuild per cloud, reused by every edge evaluation
    # folds once into decided (node, edge) pairs; the PoseGraph bind is a pure append
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
        # keyword-bound: positional order interleaves edge_prune_threshold between the two gains, so
        # preference_loop_closure must name its slot
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
        final.inlier_rmse,
        len(final.correspondence_set),
        engine=engine,
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
    # module-level HOSTILE kernel: Cloud carriers cross the process seam as arrays, each arm re-inflates the o3d form
    # its solver needs, and a raise converts through the lane's async_boundary onto the rail. The coarse pose reaches
    # every fine arm through that arm's OWN initial-transform argument, so the seeding law costs one 4x4 rather than
    # a whole-cloud copy per solve, and each solver returns the full source-to-target pose it already composed.
    source, target = session[0], session[1]
    initial = _seeded(seed)
    pulsed(tap, GeometryPulse.REGISTRATION, PulseBeat(stage=f"solve.{mode.value}", done=0, total=1))  # solve-start beat before the native arm
    reg_t = o3d.t.pipelines.registration
    match mode:
        case RegistrationMode.VGICP:
            # small_gicp.align consumes bare (N, 3) arrays, so the carrier feeds it with no o3d rebuild at all
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
                result.error,
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
            # no probe here: `register` cleared `probreg` parent-side before the crossing, so the arm runs past a
            # decision already made rather than re-asking a question whose answer cannot differ on this floor.
            return _nonrigid(source, target, policy, initial)
        case RegistrationMode.MULTIWAY:
            return _multiway(session, policy, tap)
        case unreachable:
            assert_never(unreachable)


def _distributed(result: RegistrationResult, composition: ScopeKey) -> RegistrationResult:
    # parent-side charter projection: the HOSTILE kernel's meter is the worker's no-op, so the
    # REGISTRATION_TRANSFORM charter row records here off the returned facts — the one `facts()` fold feeds the
    # receipt and the measure alike, spellings derived from the charter, never hand-picked — stamped with the
    # owner's composition so an embedded root's series partitions from the process root's.
    charter_record(GeometrySubject.REGISTRATION_TRANSFORM, result.facts(), composition=composition)
    return result


# --- [SERVICES] -------------------------------------------------------------------------


class ScanRegistration(Struct, frozen=True):
    lane: LanePolicy
    policy: RegistrationPolicy = RegistrationPolicy()
    composition: ScopeKey = DEFAULT_SCOPE  # the custody key every weave, charter record, and bench emission stamps

    async def register(
        self, session: RegistrationSession, mode: RegistrationMode, seed: "Option[RegistrationResult]" = Nothing
    ) -> "RuntimeRail[RegistrationResult]":
        async def dispatch() -> "RuntimeRail[RegistrationResult]":
            # PEP-646 arity is static evidence only, so the two-cloud minimum re-proves HERE at runtime — INSIDE the
            # evidence span, so a short session lands on the SCAN_REGISTRATION receipt as this typed refusal
            # instead of an unwitnessed early return or a worker-side IndexError.
            if len(session) < 2:
                return Error(BoundaryFault(config=(f"scan.registration.{mode}", f"session-arity:{len(session)}<2")))
            # an EMPTY member is representable — ingestion admits a scan-less read as a zero-point cloud — and past
            # this line it meets native solvers and the field's percentile/extremum folds, every one an opaque
            # worker fault; the first hollow cloud refuses HERE by index on the same receipt, a typed admission
            # fact the caller repairs at its source.
            if (hollow := next((index for index, cloud in enumerate(session) if not len(cloud)), None)) is not None:
                return Error(BoundaryFault(config=(f"scan.registration.{mode}", f"empty-cloud:{hollow}")))
            # policy tuning admits ONCE at this parent boundary — one fold naming every breached bar before any
            # provider or graduation work runs, so an out-of-domain mixture share or a non-finite ceiling is a typed
            # refusal on this receipt rather than a native solve converging on garbage past the crossing.
            if not (breached := self.policy.divergences()).is_empty():
                return Error(BoundaryFault(config=(f"scan.registration.{mode}", ";".join(breached))))
            # a seed is admitted only where a solver CONSUMES it: the `_SEEDABLE` roster names the arms with an
            # initial-transform slot (or the `NONRIGID` pre-pose), so a seed on an initialization-free row refuses
            # typed instead of riding the evidence key unconsumed, and the held pose re-proves HERE as sixteen
            # finite floats — a malformed transform is this parent-side refusal, never a worker-side reshape fault.
            flawed = seed.bind(
                lambda held: Some(f"seed-unconsumed:{mode.value}")
                if mode not in _SEEDABLE
                else Some(f"seed-shape:{len(held.transform)}")
                if len(held.transform) != 16 or not bool(np.isfinite(held.transform).all())
                else Nothing
            )
            if flawed.is_some():
                return Error(BoundaryFault(config=(f"scan.registration.{mode}", flawed.default_value(""))))
            # the marked-provider gate runs HERE, parent-side and BEFORE the crossing: module presence is identical
            # on every floor of the one shared venv, so a probe picks a capability tier and never an offload route,
            # and an absent `probreg` lands on the live span as a provisioning refusal naming the distribution to
            # install rather than as a worker death carrying a private module path.
            if mode is RegistrationMode.NONRIGID and _engine(self.policy.nonrigid).is_none():
                return Error(BoundaryFault(import_=("probreg", f"nonrigid:{self.policy.nonrigid.value}")))
            # HOSTILE is the declared trait because the compiled registration band imports under no isolated subinterpreter;
            # `lane.capacity` is the intra-kernel thread budget the lane's own slot allocator already bounds, and the
            # trailing tap is the lane conduit's pickled proxy the kernel's pulse beats write through.
            coarse = seed.map(lambda held: held.transform)
            offloaded = await self.lane.offload(
                Kernel.of(_register_kernel, KernelTrait.HOSTILE),
                session,
                mode,
                self.policy,
                self.lane.capacity,
                coarse,
                self.lane.pulses.tap,
            )
            return offloaded.map(lambda result: _distributed(result.keyed(session, coarse), self.composition))

        return await evidence_run(EvidenceScope.SCAN_REGISTRATION, f"register.{mode}", dispatch, composition=self.composition)

    async def bootstrapped(self, session: RegistrationSession, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        # composed two-stage fold, exactly as the Cases line promises: one initialization-free coarse solve, then the fine
        # mode seeded by its pose. The fine mode gates on the `_SEEDABLE` roster BEFORE the coarse stage runs —
        # `GLOBAL` re-run as its own fine stage and `MULTIWAY`, whose every edge derives its pose from its own
        # coarse solve, consume no session seed, so seeding them would burn a whole coarse kernel to decorate an
        # identity the fine solve never reads. Each admitted stage opens its own weave, because each is a real
        # solve whose cost and residual a reader prices separately — never one span hiding two kernels — and a
        # refused coarse stage short-circuits rather than letting a fine mode run from identity under a seeding claim.
        if mode not in _SEEDABLE:
            return Error(BoundaryFault(config=(f"scan.registration.{mode}", f"unseedable-fine:{mode.value}")))
        match await self.register(session, RegistrationMode.GLOBAL):
            case Result(tag="ok", ok=coarse):
                return await self.register(session, mode, Some(coarse))
            case Result(tag="error") as refused:
                return refused

    def bench(self, session: RegistrationSession, mode: RegistrationMode, *, rounds: int = 32, warmup: int = 4) -> "RuntimeRail[BenchmarkReceipt]":
        # cloud-size-parameterized macro-bench: the subject keys the exact mode row and the source point count, so
        # a latency row compares like-for-like across scan densities; each round drives the whole register crossing
        # — arity re-proof, offload, solver, weave — never an in-kernel probe (the pulse boundary).
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
