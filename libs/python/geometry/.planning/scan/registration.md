# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration over an N-cloud session, not a fixed pair. `ScanRegistration` is one registration owner discriminating by `RegistrationMode` row over a `RegistrationSession` — the `tuple[PointCloud, PointCloud, *tuple[PointCloud, ...]]` of two-or-more `o3d.t.geometry.PointCloud` clouds (the pairwise modes read the first two, the multiway mode folds all N), the `>=2` arity carried in the type so a length-1 session is a boundary type error, never a runtime `IndexError` — so the pair is the degenerate `N==2` case of the session, never a parallel surface. The modes: the initialization-free global bootstrap (Faster-PFH keypoints, ROBIN outlier pruning, GNC/Quatro solving — no initial pose, the `kiss-matcher` primary on the `<3.13` worker lane, the `open3d` Fast Global Registration fallback on `>=3.13` without `kiss-matcher` resolves), coarse-to-fine `multi_scale_icp` over the open3d tensor backend with a Tukey-robust point-to-plane estimator, colored point-to-plane ICP, the `small-gicp` VGICP parallel-multithreaded fine-refinement speed path (a Forge-worker gated enrichment row, never the spine), and N-cloud multiway pose-graph optimization, with the robust point-to-plane estimator and voxel/correspondence schedule as the shared pre-step. The `GLOBAL` coarse pose seeds the fine modes and every multiway pairwise edge. The registration transform graduates via the compute `graduation/handoff.md#GRADUATION` `HandoffAxis` geometry case as the `registration-transform` `GeometrySubject`; reconstruction and mesh repair route the `mesh-utility` sub-domain.

## [01]-[INDEX]

- [01]-[REGISTRATION]: mode-discriminated registration over an N-cloud session, the interpreter-gated global bootstrap (`kiss-matcher` primary / `open3d` FGR fallback), the N-cloud multiway pose-graph, and the shared estimation pre-step, the cross-cutting concerns folded as aspects — `boundary` the one exception-to-fault fence, the `geometry.register` OTel span, the `@receipted(_REDACTION)` egress aspect over the inner `_emit`, `LanePolicy.offload` the CPU-offload seam, `GraduationReceipt.graduates` the residual-over-ceiling admission gate.

## [02]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` row over a `RegistrationSession` (the `type RegistrationSession = tuple[PointCloud, PointCloud, *tuple[PointCloud, ...]]` PEP-646 alias whose `>=2` arity the type carries so a length-1 session fails at the boundary; the pairwise modes read `session[0]`/`session[1]`, the multiway mode folds the whole tuple); `RegistrationPolicy` the frozen tuning carrier (voxel, max-correspondence, Tukey scale, the multiscale iteration schedule, the colored geometric-weight, and the `kiss-matcher` solver gains `use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain` plus the multiway edge-uncertainty fitness floor and loop-closure preference) with a derived `voxel_schedule` property, parity with the sibling `reconstruction.md` `ReconPolicy`/`deviation.md` `DeviationPolicy` value object rather than loose scalars on the service; `RegistrationResult` the typed `gc=False` receipt carrying the mode, the chosen `BootstrapEngine`, the 4x4 transform (the session-final node pose for multiway), the per-cloud transform tuple (one identity-rooted pose per session cloud, length-1 for the pairwise modes), the fitness, the inlier RMSE, the inlier count, the rotation-consistent inlier count, and the per-stage timings, with a `RegistrationResult.of` factory that ravels the transform once and defaults the single-pose tuple so every arm constructs through one keyword fold rather than near-identical positional constructions, a `RegistrationResult._from_tensor` projector the two tensor arms (`MULTISCALE`/`COLORED_ICP`) share so the open3d `RegistrationResult`-to-receipt fold — reading `.transformation`/`.fitness`/`.inlier_rmse` and deriving the inlier estimate from the matched-source fitness — lives in one place rather than duplicated per tensor arm, a `span_facts` bounded `str | int` scalar projection both the `geometry.register` span and the receipt read (parity with `MeshQuality.span_facts`/`GraduationReceipt.span_facts`), and the `@receipted(_REDACTION)`-decorated `_emit` egress aspect that harvests `contribute` on exit so emission rides the decorator rail (the `ReconReceipt._emit`/compute `handoff.md` `_emit` parity) rather than an inline `Signals.emit`; `BootstrapEngine` the global-coarse-pose vocabulary (`KISS_MATCHER` | `OPEN3D_FGR`) the engine selection selects.
- Cases: `RegistrationMode` rows `GLOBAL` (the initialization-free coarse pose — `kiss-matcher` Faster-PFH/ROBIN/GNC on `<3.13`, `open3d` FGR-over-FPFH on `>=3.13`, no initial pose) · `MULTISCALE` (coarse-to-fine `t.pipelines.multi_scale_icp` with a Tukey-robust point-to-plane estimator) · `COLORED_ICP` (colored point-to-plane over the open3d tensor backend) · `VGICP` (the `small-gicp` voxelized parallel fine-refinement speed path) · `MULTIWAY` (N-cloud pose-graph optimization over a multi-station session) — matched by `match`/`assert_never`, each binding the engine and estimator that owns it.
- Entry: `ScanRegistration.register` admits a `RegistrationSession` and a mode, opens one `geometry.register` OTel span carrying the bounded `mode` scalar behind the `is_recording()` gate, runs the mode's pipeline through `boundary(f"scan.{mode}", ...)` eagerly inside the live span (the span-then-fence discipline the sibling `reconstruction.md`/`deviation.md` and the compute `graduation/handoff.md#GRADUATION` hold), then matches the rail — the `Ok` arm widens the recording span with `receipt.span_facts` and sets `Status(StatusCode.OK)`, the `Error` arm is `pass` because the `boundary` fence's `_convert` already `record_exception`d the cause and set `Status(StatusCode.ERROR, fault.tag)` on the active span — and returns the `RuntimeRail[RegistrationResult]`; the `GLOBAL` arm threads the private `_bootstrap(source, target, engine)` coarse pose (no initial pose required, the `BootstrapEngine` resolved once by the `_engine` engine-selection read and passed in, never re-read per call) that seeds the fine modes and every multiway edge; the `MULTIWAY` arm threads the private `_multiway` that reads `_engine` once and folds each session cloud's pairwise `_bootstrap(cloud, session[0], engine)` solution (source the i-th cloud, target the reference cloud, so the solved pose maps cloud(i)->cloud(0) in the same direction `evaluate_registration` and the `PoseGraphNode` absolute-pose convention assert) into a `PoseGraph` and runs `global_optimization`; the two tensor arms (`MULTISCALE`/`COLORED_ICP`) bind the shared Tukey `RobustKernel` through the private `_tukey` projector — built only on those arms, never constructed on the `GLOBAL`/`MULTIWAY` paths that own no tensor estimator — and fold the open3d tensor result through the one `RegistrationResult._from_tensor` projector rather than re-spelling the `transformation`/`fitness`/`inlier_rmse`/inlier-estimate construction per arm.
- Auto: the `_engine` floor reads `sys.version_info` once against the `_KISS_FLOOR` `(3, 13)` anchor — below it selects `BootstrapEngine.KISS_MATCHER`, otherwise `BootstrapEngine.OPEN3D_FGR` (the `[KISS_MATCHER_FALLBACK_FGR]` deferred USAGE card, [BLOCKED], reference only — the FGR arm body is authored as the dispatch row but the per-interpreter selection it serves lands with that card); the tensor path runs `t.pipelines.registration.multi_scale_icp` with `TransformationEstimationPointToPlane` over the `_tukey`-built `TukeyLoss` `RobustKernel` across the `RegistrationPolicy.voxel_schedule`-derived voxel/correspondence pairs and the `multiscale_iterations` criteria list, then folds the tensor result through the one `RegistrationResult._from_tensor` projector that reads `.transformation`/`.fitness`/`.inlier_rmse` and folds the fitness against the source point count into the inlier estimate rather than zeroing it; the colored path runs the tensor `icp` under `TransformationEstimationForColoredICP(colored_lambda_geometric, _tukey())` through the same `_from_tensor` fold; the `KISS_MATCHER` bootstrap binds the full `KISSMatcherConfig` gain constructor (`voxel_size`/`use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`) from the policy, runs the `match` + `prune_and_solve` decomposition over the `float64 (3, n)` transposed source/target positions (the array overload `match`/`prune_and_solve`/`solve` carry, NOT the `float32 (3, 1)` sequence form `estimate` documents, so the array path threads the decomposition that keeps every stage accessor populated), reads `RegistrationSolution.rotation`/`.translation` into a 4x4 transform, folds `get_num_final_inliers` into the inlier count and `get_num_rotation_inliers` into the rotation-consistent inlier count, threads the per-stage `get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time` into the timing receipt, and gates every readout on `solution.valid`; the `OPEN3D_FGR` bootstrap downsamples and estimates normals on both legacy clouds in one comprehension, computes `compute_fpfh_feature` descriptors, runs `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`, and folds the correspondence-set length into the inlier count; the VGICP path runs `small_gicp.align` with `registration_type='VGICP'` against the target positions building the internal Gaussian voxel map, reading `RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations` (the iteration count riding the timing receipt), folding the inlier ratio into the fitness; the multiway path folds every-cloud-against-first `_bootstrap` solutions through one `Block.of_seq(session[1:]).mapi(...)` over the `_edge` projector into fully-decided `(PoseGraphNode, PoseGraphEdge)` pairs — the per-edge domain math (the cloud(i+1)->cloud(0) pose, the `evaluate_registration` fitness scoring the `uncertain` flag against the `edge_uncertain_below_fitness` floor rather than a hardcoded `False`) living in the fold so the `PoseGraph` boundary bind is a pure append over already-decided edges, never a `for`-loop accumulating with index arithmetic and parallel `nodes`/`edges` interleave — runs `global_optimization`, then re-evaluates the optimized session-final pose through `evaluate_registration` so the multiway receipt carries the real fitness/inlier-RMSE/correspondence-count instead of the placeholder `1.0`/`0.0`/`0`, reading every optimized node `.pose` into the per-cloud transform tuple.
- Receipt: emission rides the `@receipted(_REDACTION)` AOP aspect over the inner `RegistrationResult._emit` (the decorator rail the receipts owner declares and the sibling `ReconReceipt._emit`/compute `handoff.md` `_emit` establish), never an inline `Signals.emit` threaded through the body; the aspect harvests `RegistrationResult.contribute` on exit, which returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port streams through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("geometry.scan.registration", ("emitted", mode, facts))` minting the `fact` case at `emitted`, never a four-positional `Receipt.of("emitted", owner, subject, facts)` call the receipts owner names deleted nor a single bare `Receipt` against the `Iterable[Receipt]` port. The facts are produced once through `RegistrationResult.facts` (parity with the sibling `DeviationBand.facts`/`ReconReceipt.facts`) carrying the mode, the `BootstrapEngine` (where the arm bootstraps), the fitness, the inlier RMSE, the inlier count, the rotation-consistent inlier count, and the per-stage timings spread as `t.<stage>` fact keys as NATIVE `float`/`int` slots — the `observability/receipts#RECEIPT` `EventDict` is `dict[str, object]` and its `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them without a `str()`/`repr()` coerce, so the pre-`repr`/`str`-formatted fact map is the deleted form the receipts owner rejects; the bounded `span_facts` scalars (mode, engine, the inlier count) are the only subset the `geometry.register` span widens with, the full fitness/RMSE/timing ledger riding the receipt facts alone. `RegistrationResult.graduates(evidence_key)` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=_SUBJECT)`, gating TWO residual keys against the compute owner's per-key ceiling fold — the measured `inlier_rmse` against `_RMSE_CEILING` and the `misfit` residual `1.0 - fitness` against `_MISFIT_CEILING` — so a transform whose alignment residual exceeds the RMSE ceiling OR whose fitness drops below the minimum (a coarse `GLOBAL`/`KISS_MATCHER` pose minting a `0.0` placeholder RMSE no longer graduates on the vacuous ceiling alone, because its inlier-ratio misfit must clear the floor too) is an `Error(BoundaryFault)` on the rail rather than a graduated handoff. The misfit is expressed as the upper-bounded residual `1 - fitness` against a misfit ceiling rather than a fitness lower bound, so the quality floor rides the compute owner's residual-over-ceiling `_admit` direction unchanged — no second admission direction minted here. The subject is typed as the compute-owned `GeometrySubject` `"registration-transform"` literal (imported from `rasm.compute.graduation.handoff`, never a bare `str`, so an unlisted literal fails at the type boundary); this owner is the CONSUMER of the already-declared subject and the supplier of its measured/ceiling ledger, the residual-over-ceiling fold itself the one admission gate the compute owner owns.
- Packages: `kiss_matcher` (`KISSMatcher`/`KISSMatcherConfig` gain constructor incl. `voxel_size`/`use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`/`match`/`prune_and_solve`/`RegistrationSolution.rotation`/`.translation`/`.valid`/`get_num_final_inliers`/`get_num_rotation_inliers`/`get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time`), `open3d` (`t.geometry.PointCloud.to_legacy`/`t.pipelines.registration.multi_scale_icp`/`icp`/`ICPConvergenceCriteria`/`TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP`/`robust_kernel.RobustKernel`/`RobustKernelMethod.TukeyLoss`/`utility.DoubleVector`/`geometry.PointCloud.voxel_down_sample`/`estimate_normals`/`KDTreeSearchParamHybrid`/`pipelines.registration.compute_fpfh_feature`/`registration_fgr_based_on_feature_matching`/`evaluate_registration`/`PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`/`global_optimization`/`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`), `small_gicp` (`align`/`RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations`), `numpy` (`asarray`/`eye`/`ravel`/`reshape` over the open3d/kiss-matcher transform arrays — `np.eye(4)` the catalogued identity creator `numpy.md` row [08], `np.ravel`/`np.reshape` the catalogued shape ops rows [01]/[02], never the uncatalogued `np.identity`/`ndarray.flatten`), `expression` (`Block.of_seq`/`Block.mapi` the per-edge multiway pair fold over the non-reference clouds, `Ok`/`Error` the span-arm `match` on the rail, `Map.empty` the keep-all `Redaction.classified` table), `msgspec` (`Struct`/`field`/`gc=False` on the leaf receipt), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` for the one `geometry.register` span), runtime (`RuntimeRail`/`boundary`/`Receipt`/`Redaction`/`receipted`/`ContentKey`/`LanePolicy`, the `ReceiptContributor` port `RegistrationResult.contribute` satisfies structurally), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`). All three compiled registration packages import function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy; none is module-top.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import sys
import numpy as np
from enum import StrEnum
from typing import TYPE_CHECKING, Final, assert_never

from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct, field
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis

if TYPE_CHECKING:
    import open3d as o3d  # type-only: the runtime import is function-local per boundary so the runtime module loads clean


# --- [TYPES] ----------------------------------------------------------------------------


class RegistrationMode(StrEnum):
    GLOBAL = "global"
    MULTISCALE = "multiscale"
    COLORED_ICP = "colored-icp"
    VGICP = "vgicp"
    MULTIWAY = "multiway"


class BootstrapEngine(StrEnum):
    KISS_MATCHER = "kiss-matcher"
    OPEN3D_FGR = "open3d-fgr"


# at least two clouds: the pairwise modes read session[0]/session[1], MULTIWAY needs >=2 to form one edge.
type RegistrationSession = tuple["o3d.t.geometry.PointCloud", "o3d.t.geometry.PointCloud", *tuple["o3d.t.geometry.PointCloud", ...]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: GeometrySubject = "registration-transform"
_KISS_FLOOR: Final[tuple[int, int]] = (3, 13)
_RMSE_CEILING: Final[float] = 0.01
_MISFIT_CEILING: Final[float] = 0.7
# registration facts carry no secret field, so the @receipted egress binds the keep-all redaction.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_TRACER: Final[trace.Tracer] = trace.get_tracer("geometry.scan.registration")


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
    num_threads: int = 8
    edge_uncertain_below_fitness: float = 0.5
    preference_loop_closure: float = 0.25

    @property
    def voxel_schedule(self) -> tuple[tuple[float, float], ...]:
        return ((self.voxel * 4, self.max_correspondence), (self.voxel, self.voxel))


class RegistrationResult(Struct, frozen=True, gc=False):
    mode: RegistrationMode
    engine: BootstrapEngine | None
    transform: tuple[float, ...]
    poses: tuple[tuple[float, ...], ...]
    fitness: float
    inlier_rmse: float
    inliers: int
    rotation_inliers: int = 0
    timings: dict[str, float] = field(default_factory=dict)

    @staticmethod
    def of(
        mode: RegistrationMode, transform: np.ndarray, fitness: float, inlier_rmse: float, inliers: int,
        *, engine: BootstrapEngine | None = None, poses: tuple[tuple[float, ...], ...] = (),
        rotation_inliers: int = 0, timings: dict[str, float] | None = None,
    ) -> RegistrationResult:
        flat = tuple(np.ravel(np.asarray(transform)))  # the catalogued shape op owns the row-major flatten
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), float(inlier_rmse),
            int(inliers), int(rotation_inliers), timings or {},
        )

    @staticmethod
    def _from_tensor(
        mode: RegistrationMode, reg: "o3d.t.pipelines.registration.RegistrationResult", source: "o3d.t.geometry.PointCloud"
    ) -> RegistrationResult:
        # one fold for every tensor-backend arm (MULTISCALE/COLORED_ICP): the open3d `.fitness` is the
        # matched-source fraction, so `fitness * |source|` is the inlier estimate rather than zeroing it.
        return RegistrationResult.of(
            mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse,
            int(reg.fitness * len(source.point.positions.numpy())),
        )

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: RegistrationResult) -> RegistrationResult:
        return receipt  # the @receipted aspect harvests `contribute` and emits on exit; egress is the decorator rail

    @property
    def span_facts(self) -> dict[str, str | int]:
        return {"mode": self.mode.value, "engine": self.engine.value if self.engine else "", "inliers": self.inliers}

    def facts(self) -> dict[str, object]:
        # native float/int slots: the `observability/receipts#RECEIPT` `EventDict` is `dict[str, object]`
        # and its `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them without a
        # `str()`/`repr()` coerce — pre-formatting here is the deleted form the receipts owner rejects.
        return {
            **self.span_facts, "fitness": self.fitness, "inlier_rmse": self.inlier_rmse,
            "rotation_inliers": self.rotation_inliers,
        } | {f"t.{stage}": seconds for stage, seconds in self.timings.items()}

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints the `fact` case at `emitted`, never a four-positional call against the port.
        return (Receipt.of("geometry.scan.registration", ("emitted", self.mode.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt.graduates(
            "geometry.scan.registration", HandoffAxis(geometry=_SUBJECT), evidence_key,
            {"inlier_rmse": self.inlier_rmse, "misfit": 1.0 - self.fitness},
            {"inlier_rmse": _RMSE_CEILING, "misfit": _MISFIT_CEILING},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class ScanRegistration(Struct, frozen=True):
    policy: RegistrationPolicy = RegistrationPolicy()
    lane: LanePolicy | None = None

    def register(self, session: RegistrationSession, mode: RegistrationMode) -> RuntimeRail[RegistrationResult]:
        # span-then-fence: the `geometry.register` span widens with the bounded `mode` scalar behind
        # `is_recording()`, then `boundary` runs the kernel eagerly inside the live `with` so an open3d/
        # kiss-matcher raise records on the open span through the faults `_convert` weave; the `Ok` arm
        # widens with `receipt.span_facts` and the `Error` arm leaves status to the fence's `_convert`.
        with _TRACER.start_as_current_span("geometry.register") as span:
            if span.is_recording():
                span.set_attributes({"mode": mode.value})
            rail = boundary(f"scan.{mode}", lambda: RegistrationResult._emit(self._dispatch(session, mode)))
            match rail:
                case Ok(receipt):
                    if span.is_recording():
                        span.set_attributes(receipt.span_facts)
                    span.set_status(Status(StatusCode.OK))
                case Error(_):
                    pass
            return rail

    def _dispatch(self, session: RegistrationSession, mode: RegistrationMode) -> RegistrationResult:
        import open3d as o3d  # noqa: PLC0415

        source, target = session[0], session[1]
        reg_t = o3d.t.pipelines.registration
        match mode:
            case RegistrationMode.VGICP:
                import small_gicp  # noqa: PLC0415

                src = source.point.positions.numpy()
                result = small_gicp.align(
                    target.point.positions.numpy(), src, registration_type="VGICP",
                    downsampling_resolution=self.policy.voxel, num_threads=self.policy.num_threads,
                )
                return RegistrationResult.of(
                    mode, result.T_target_source, result.num_inliers / max(len(src), 1), result.error, result.num_inliers,
                    timings={"iterations": float(result.iterations)},
                )
            case RegistrationMode.MULTISCALE:
                voxels, corrs = zip(*self.policy.voxel_schedule)
                reg = reg_t.multi_scale_icp(
                    source, target, o3d.utility.DoubleVector(voxels),
                    [reg_t.ICPConvergenceCriteria(max_iteration=it) for it in self.policy.multiscale_iterations],
                    o3d.utility.DoubleVector(corrs), estimation_method=reg_t.TransformationEstimationPointToPlane(self._tukey()),
                )
                return RegistrationResult._from_tensor(mode, reg, source)
            case RegistrationMode.COLORED_ICP:
                colored = reg_t.TransformationEstimationForColoredICP(self.policy.colored_lambda_geometric, self._tukey())
                reg = reg_t.icp(source, target, self.policy.max_correspondence, estimation_method=colored)
                return RegistrationResult._from_tensor(mode, reg, source)
            case RegistrationMode.GLOBAL:
                return self._bootstrap(source, target, self._engine())
            case RegistrationMode.MULTIWAY:
                return self._multiway(session)
            case unreachable:
                assert_never(unreachable)

    def _engine(self) -> BootstrapEngine:
        return BootstrapEngine.KISS_MATCHER if sys.version_info < _KISS_FLOOR else BootstrapEngine.OPEN3D_FGR

    def _tukey(self) -> "o3d.t.pipelines.registration.robust_kernel.RobustKernel":
        import open3d as o3d  # noqa: PLC0415

        rk = o3d.t.pipelines.registration.robust_kernel
        return rk.RobustKernel(rk.RobustKernelMethod.TukeyLoss, self.policy.tukey_k)

    def _bootstrap(self, source: "o3d.t.geometry.PointCloud", target: "o3d.t.geometry.PointCloud", engine: BootstrapEngine) -> RegistrationResult:
        match engine:
            case BootstrapEngine.KISS_MATCHER:
                import kiss_matcher  # noqa: PLC0415

                config = kiss_matcher.KISSMatcherConfig(
                    voxel_size=self.policy.voxel, use_quatro=self.policy.use_quatro, thr_linearity=self.policy.thr_linearity,
                    num_max_corr=self.policy.num_max_corr, robin_noise_bound_gain=self.policy.robin_noise_bound_gain,
                    solver_noise_bound_gain=self.policy.solver_noise_bound_gain,
                )
                matcher = kiss_matcher.KISSMatcher(config)
                # the `float64 (3, n)` array overload is `match`/`prune_and_solve`/`solve`'s, NOT `estimate`'s
                # `float32 (3, 1)` sequence form; the decomposition keeps every stage-timing accessor populated.
                src = np.asarray(source.point.positions.numpy(), dtype=np.float64).T
                src_matched, tgt_matched = matcher.match(src, np.asarray(target.point.positions.numpy(), dtype=np.float64).T)
                solution = matcher.prune_and_solve(src_matched, tgt_matched)
                inliers = matcher.get_num_final_inliers() if solution.valid else 0
                return RegistrationResult.of(
                    RegistrationMode.GLOBAL, self._homogeneous(np.asarray(solution.rotation), np.asarray(solution.translation)),
                    float(inliers) / max(src.shape[1], 1), 0.0, inliers,
                    engine=BootstrapEngine.KISS_MATCHER, rotation_inliers=matcher.get_num_rotation_inliers() if solution.valid else 0,
                    timings={
                        "extraction": matcher.get_extraction_time(), "matching": matcher.get_matching_time(),
                        "rejection": matcher.get_rejection_time(), "solver": matcher.get_solver_time(),
                        "processing": matcher.get_processing_time(),
                    },
                )
            case BootstrapEngine.OPEN3D_FGR:
                import open3d as o3d  # noqa: PLC0415

                reg = o3d.pipelines.registration
                search = o3d.geometry.KDTreeSearchParamHybrid(self.policy.voxel * 5, 100)
                normals = o3d.geometry.KDTreeSearchParamHybrid(self.policy.voxel * 2, 30)
                # `estimate_normals` mutates in place and returns `None`, so the downsample + normal-estimate fold
                # threads the in-place conditioning inside the one comprehension (`or cloud` yields the conditioned
                # cloud past the `None` return) rather than a bare statement loop accumulating over `down`.
                down = tuple(
                    cloud.estimate_normals(normals) or cloud
                    for cloud in (s.to_legacy().voxel_down_sample(self.policy.voxel) for s in (source, target))
                )
                features = tuple(reg.compute_fpfh_feature(cloud, search) for cloud in down)
                result = reg.registration_fgr_based_on_feature_matching(down[0], down[1], features[0], features[1])
                return RegistrationResult.of(
                    RegistrationMode.GLOBAL, np.asarray(result.transformation), result.fitness, result.inlier_rmse,
                    len(result.correspondence_set), engine=BootstrapEngine.OPEN3D_FGR,
                )
            case unreachable:
                assert_never(unreachable)

    def _multiway(self, session: RegistrationSession) -> RegistrationResult:
        import open3d as o3d  # noqa: PLC0415

        reg = o3d.pipelines.registration
        engine = self._engine()  # one engine-selection read; every pairwise edge solves on the same bootstrap engine
        legacy = tuple(cloud.to_legacy() for cloud in session)
        # the per-edge domain math (the cloud(i+1)->cloud(0) pose, the measured `uncertain` fitness gate) folds once
        # through one `Block.mapi` over the non-reference clouds into fully-formed `(node, edge)` pairs, so the
        # `PoseGraph` boundary bind is a pure append over already-decided edges, never a domain accumulation with
        # index arithmetic and parallel `nodes`/`edges` interleave; `i` is the edge index into `session[1:]`.
        pairs = Block.of_seq(session[1:]).mapi(lambda i, cloud: self._edge(reg, legacy, i, self._bootstrap(cloud, session[0], engine)))
        graph = reg.PoseGraph()
        graph.nodes.append(reg.PoseGraphNode(np.eye(4)))
        for node, edge in pairs:
            graph.nodes.append(node)
            graph.edges.append(edge)
        reg.global_optimization(
            graph, reg.GlobalOptimizationLevenbergMarquardt(), reg.GlobalOptimizationConvergenceCriteria(),
            # keyword-bound: the positional order is `(max_correspondence_distance, edge_prune_threshold,
            # preference_loop_closure, reference_node)`, so the policy loop-closure gain MUST name its slot or
            # it lands in `edge_prune_threshold` and disables loop closure; node 0 (identity root) is fixed.
            reg.GlobalOptimizationOption(
                max_correspondence_distance=self.policy.max_correspondence,
                preference_loop_closure=self.policy.preference_loop_closure, reference_node=0,
            ),
        )
        poses = tuple(np.asarray(node.pose) for node in graph.nodes)
        final = reg.evaluate_registration(legacy[-1], legacy[0], self.policy.max_correspondence, poses[-1])
        return RegistrationResult.of(
            RegistrationMode.MULTIWAY, poses[-1], final.fitness, final.inlier_rmse, len(final.correspondence_set),
            engine=engine, poses=tuple(tuple(np.ravel(pose)) for pose in poses),
        )

    def _edge(
        self, reg: "o3d.pipelines.registration", legacy: tuple["o3d.geometry.PointCloud", ...], i: int, solution: RegistrationResult
    ) -> tuple["o3d.pipelines.registration.PoseGraphNode", "o3d.pipelines.registration.PoseGraphEdge"]:
        # the `uncertain` flag is the measured `evaluate_registration` fitness against the policy floor, not a
        # hardcoded `False`; the pose maps cloud(i+1)->cloud(0), the `PoseGraphNode` absolute-pose convention.
        pose = np.reshape(np.asarray(solution.transform), (4, 4))
        fitness = reg.evaluate_registration(legacy[i + 1], legacy[0], self.policy.max_correspondence, pose).fitness
        node = reg.PoseGraphNode(pose)
        edge = reg.PoseGraphEdge(0, i + 1, pose, uncertain=fitness < self.policy.edge_uncertain_below_fitness)
        return node, edge

    def _homogeneous(self, rotation: np.ndarray, translation: np.ndarray) -> np.ndarray:
        transform = np.eye(4)  # the catalogued identity creator; never the uncatalogued `np.identity`
        transform[:3, :3] = rotation
        transform[:3, 3] = translation
        return transform
```

## [03]-[RESEARCH]

- [TENSOR_MULTISCALE]: the `t.pipelines.registration.multi_scale_icp(source, target, voxel_sizes, criteria_list, max_correspondence_distances, init_source_to_target=eye, estimation_method=...)` signature is folder-`.api`-confirmed (`open3d.md` row [02]) — `voxel_sizes` and `max_correspondence_distances` are `o3d.utility.DoubleVector`, `criteria_list` is a `list[ICPConvergenceCriteria]` (one per scale, NOT an `IntVector` of iteration counts), and `init_source_to_target` defaults to identity so the arm passes `estimation_method=plane` by keyword; the `TransformationEstimationForColoredICP(lambda_geometric, kernel)` arity (`open3d.md` row [04]) and the tensor `icp(source, target, max_correspondence_distance, estimation_method=...)` keyword (`open3d.md` row [01]) are catalogue-confirmed. The only live-run residual is the per-scale `ICPConvergenceCriteria` relative-fitness/relative-rmse tuning, an owner-local heuristic.
- [GLOBAL_KISS_ARITY]: the global solve threads the `match(src, tgt) -> (src_matched, tgt_matched)` keypoint stage plus the `prune_and_solve(src_matched, tgt_matched) -> RegistrationSolution` ROBIN-prune-and-GNC-solve stage (`kiss-matcher.md#80-82`/`#107`-`#108`), the catalogue-confirmed `float64 (3, n)` array overload — the catalogue bins that array form with `match`/`prune_and_solve`/`solve`, while `estimate` is the `float32 (3, 1)` *sequence* form, so the array path threads the documented decomposition rather than a `float64 (3, n)` `estimate` overload the catalogue does not grant; the decomposition keeps every stage accessor populated (`match` fills `get_extraction_time`/`get_matching_time`, `prune_and_solve` fills `get_rejection_time`/`get_solver_time`, `get_processing_time` the total) and reuses the matched correspondences per `kiss-matcher.md#108`. The full `KISSMatcherConfig(voxel_size=..., use_quatro=..., thr_linearity=..., num_max_corr=..., robin_noise_bound_gain=..., solver_noise_bound_gain=...)` gain constructor (`kiss-matcher.md#32`), the `RegistrationSolution.rotation`/`.translation`/`.valid` readouts (`kiss-matcher.md#51-55`), and the `get_num_final_inliers`/`get_num_rotation_inliers`/`get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time` receipt accessors (`kiss-matcher.md#96-100`) confirm against the branch `kiss-matcher` catalogue on the cp312 companion. `global_optimization` is the catalogued multiway driver (`open3d.md` row [08], "pose graph plus options") and `PoseGraph` the catalogued node/edge graph (row [07], "multiway registration node/edge graph"); the construction surface the arm folds the graph through — `PoseGraphNode`/`PoseGraphEdge`/`PoseGraphNode.pose`, the `GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption` config triple, the legacy `RegistrationResult.correspondence_set` the FGR/`evaluate_registration` arms read, and the tensor `t.geometry.PointCloud.to_legacy`/`.point.positions`/`.numpy()` accessors the dispatch arms call — are real open3d 0.19 members the catalog formerly enumerated only implicitly under those two rows and now carries as explicit `open3d.md` rows in sections 02/03 (the catalog-completeness gap is closed, never a phantom citation); the `GlobalOptimizationOption` slots are keyword-bound at the call so the policy loop-closure gain cannot mismap into `edge_prune_threshold`.
- [MULTIWAY_EVALUATION]: the `registration.evaluate_registration(source, target, max_correspondence_distance, transformation)` fitness/inlier-RMSE/correspondence readout (`open3d.md` row [07], same `RegistrationResult` family as row [01]) is the evidence source the multiway arm reads per edge and on the optimized session-final pose, so the `PoseGraphEdge` `uncertain` flag and the multiway receipt carry measured alignment quality rather than placeholder constants; the per-edge `edge_uncertain_below_fitness` floor and the keyword-bound `GlobalOptimizationOption(max_correspondence_distance=..., preference_loop_closure=..., reference_node=0)` arguments are owner-local `RegistrationPolicy` heuristics the live run tunes, not catalogue dependencies — the keyword binding load-bearing because the positional order interleaves `edge_prune_threshold` between the two policy gains.
- [GLOBAL_FGR_FALLBACK]: the `>=3.13` fallback bootstrap composes `geometry.PointCloud.voxel_down_sample` (`open3d.md` row [01]) + `estimate_normals` over `KDTreeSearchParamHybrid` (`open3d.md` rows [03]/[17]) + `pipelines.registration.compute_fpfh_feature` (`open3d.md` row [06]) + `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`/`.correspondence_set` (`open3d.md` row [05], same `RegistrationResult` family as row [01]); this is the multi-library woven coarse-pose rail the `[KISS_MATCHER_FALLBACK_FGR]` deferred USAGE card (geometry, [BLOCKED], reference only) serves — the dispatch arm is authored, the per-interpreter selection it backs lands with that card. Note `open3d` itself ships cp38-cp312 packages only (`open3d.md#168`), so the FGR fallback is exercisable only where an `open3d` package resolves; the `_engine` floor is the structural selector the card finalizes against a live multi-interpreter run.
- [GICP_ALIGN_ARITY]: the `small_gicp.align(target_points, source_points, registration_type='VGICP', downsampling_resolution=..., num_threads=...)` raw-array overload (`small-gicp.md` row [01] + the entrypoint scope note that the raw-array overload carries the full `ICP`/`PLANE_ICP`/`GICP`/`VGICP` set and builds the internal Gaussian voxel map for `VGICP`) and the `RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations` readouts (`small-gicp.md` rows [07]-[09]) confirm against the branch `small-gicp` catalogue; the source-count inlier-ratio fitness is the owner-local fold.
- [TRANSFORM_ARRAYS]: the 4x4 transform plumbing composes only `.api`-confirmed numpy members — `np.asarray` the no-copy intake (`numpy.md` conversion row [10]), `np.eye(4)` the identity creator (`numpy.md` creation row [08]) the `_homogeneous` rotation/translation assembly and the multiway root node read, `np.ravel` the row-major flatten (`numpy.md` shape row [02]) the `RegistrationResult.of` transform/pose serialization, and `np.reshape(a, (4, 4))` the per-edge pose recovery (`numpy.md` shape row [01]); the uncatalogued `np.identity`/`ndarray.flatten`/`ndarray.reshape(...)` member forms are the deleted spellings, the catalogued functional shape ops owning every transform reshape so no phantom member rides the receipt.
- [SPAN_AND_EGRESS]: the `register` entry weaves one `geometry.register` OTel span around the `boundary` fence (the sibling `reconstruction.md`/`deviation.md` and compute `graduation/handoff.md#GRADUATION` span-then-fence pattern) over the `.api`-confirmed `trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` surface (`opentelemetry-api.md` trace family rows [05]/[06] + the SDK-free API contract), the bounded `span_facts` scalars the only `set_attributes` payload behind the `is_recording()` gate and the full fitness/RMSE/timing ledger riding the receipt facts alone per the `Span.set_attributes` scalar contract. Emission rides the `@receipted(_REDACTION)`-decorated `RegistrationResult._emit` over the inner identity body, the `observability/receipts#RECEIPT` decorator rail that harvests `contribute` and routes the stream through the canonical `Signals.emit` fold on exit — the `Redaction(classified=Map.empty())` keep-all table (registration facts carry no secret field) the same `_REDACTION` shape the receipts/handoff owners hold, the `Contributing[P, R: ReceiptContributor]` bound preserving the concrete `RegistrationResult` subtype so the `Ok(receipt)` arm reads `receipt.span_facts` without a static erasure to the bare port. The span/aspect spellings are settled against the receipts and handoff owners.

## [04]-[UPSTREAM]

- [FALLBACK_CARD]: the `_engine` engine-selection read and the `_bootstrap_fgr` arm are the structure the deferred USAGE card `[KISS_MATCHER_FALLBACK_FGR]` (geometry, [BLOCKED], reference only) finalizes — on a `>=3.13` interpreter without `kiss-matcher` resolves, the `GLOBAL` mode degrades to the `open3d` Fast Global Registration path that mints the coarse pose seeding the fine modes and multiway edges. The arm body is authored here as the dispatch row (so the page is transcription-complete); the card owns the per-interpreter package-resolution selection and its live multi-interpreter verification, not authored as landed selection logic here.
- [GRADUATION_SUBJECT]: the `registration-transform` `GeometrySubject` this owner graduates is already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union, so the `_SUBJECT` constant imports the literal from `rasm.compute.graduation.handoff` rather than minting a bare `str` — an unlisted literal fails at the `GeometrySubject` type boundary, the compute owner owning the union. `RegistrationResult.graduates` routes a two-key measured ledger (`inlier_rmse` and the `misfit` residual `1 - fitness`) through the one compute `GraduationReceipt.graduates` admission fold against the `_RMSE_CEILING`/`_MISFIT_CEILING` residual bars — the per-key residual-over-ceiling gate the compute owner owns, not a local admission body — so a transform whose RMSE exceeds the ceiling or whose fitness falls below the floor (a coarse `GLOBAL` bootstrap with a `0.0` placeholder RMSE included) is an `Error(BoundaryFault)` rather than a graduated handoff. The misfit residual rides the compute owner's existing upper-bound `_admit` direction, so no new literal, no new gate, and no second admission direction are minted; this page is a CONSUMER of the already-declared subject and the compute admission rail, supplying only its measured/ceiling ledger, never authoring the compute interior.
