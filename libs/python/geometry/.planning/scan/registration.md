# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration over an N-cloud session, not a fixed pair. `ScanRegistration` is one registration owner discriminating by `RegistrationMode` row over a `RegistrationSession` — a tuple of one-or-more `o3d.t.geometry.PointCloud` clouds (the pairwise modes read the first two, the multiway mode folds all N) — so the pair is the degenerate `N==2` case of the session, never a parallel surface. The modes: the initialization-free global bootstrap (Faster-PFH keypoints, ROBIN outlier pruning, GNC/Quatro solving — no initial pose, the `kiss-matcher` primary on the `<3.13` companion band, the `open3d` Fast Global Registration fallback on `>=3.13` where no `kiss-matcher` wheel resolves), coarse-to-fine `multi_scale_icp` over the open3d tensor backend with a Tukey-robust point-to-plane estimator, colored point-to-plane ICP, the `small-gicp` VGICP parallel-multithreaded fine-refinement speed path (a Forge-companion-lane gated enrichment row, never the spine), and N-cloud multiway pose-graph optimization, with the robust point-to-plane estimator and voxel/correspondence schedule as the shared pre-step. The `GLOBAL` coarse pose seeds the fine modes and every multiway pairwise edge. The registration transform graduates via the compute `graduation/handoff.md#GRADUATION` `HandoffAxis` geometry case as the `registration-transform` `GeometrySubject`; reconstruction and mesh repair route the `mesh-utility` sub-domain.

## [01]-[INDEX]

- [01]-[REGISTRATION]: mode-discriminated registration over an N-cloud session, the interpreter-gated global bootstrap (`kiss-matcher` primary / `open3d` FGR fallback), the N-cloud multiway pose-graph, and the shared estimation pre-step.

## [02]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` row over a `RegistrationSession` (a `tuple[o3d.t.geometry.PointCloud, ...]` of length `>=2`; the pairwise modes read `session[0]`/`session[1]`, the multiway mode folds the whole tuple); `RegistrationPolicy` the frozen tuning carrier (voxel, max-correspondence, Tukey scale, the multiscale iteration schedule, the colored geometric-weight, and the `kiss-matcher` solver gains `use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain` plus the multiway edge-uncertainty fitness floor and loop-closure preference) with a derived `voxel_schedule` property, parity with the sibling `reconstruction.md` `ReconPolicy` value object rather than loose scalars on the service; `RegistrationResult` the typed receipt carrying the mode, the chosen `BootstrapEngine`, the 4x4 transform (the session-final node pose for multiway), the per-cloud transform tuple (one identity-rooted pose per session cloud, length-1 for the pairwise modes), the fitness, the inlier RMSE, the inlier count, the rotation-consistent inlier count, and the per-stage timings, with a `RegistrationResult.of` factory that flattens the transform once and defaults the single-pose tuple so the six arms construct through one keyword fold rather than six near-identical positional constructions; `BootstrapEngine` the global-coarse-pose vocabulary (`KISS_MATCHER` | `OPEN3D_FGR`) the interpreter floor selects.
- Cases: `RegistrationMode` rows `GLOBAL` (the initialization-free coarse pose — `kiss-matcher` Faster-PFH/ROBIN/GNC on `<3.13`, `open3d` FGR-over-FPFH on `>=3.13`, no initial pose) · `MULTISCALE` (coarse-to-fine `t.pipelines.multi_scale_icp` with a Tukey-robust point-to-plane estimator) · `COLORED_ICP` (colored point-to-plane over the open3d tensor backend) · `VGICP` (the `small-gicp` voxelized parallel fine-refinement speed path) · `MULTIWAY` (N-cloud pose-graph optimization over a multi-station session) — matched by `match`/`assert_never`, each binding the engine and estimator that owns it.
- Entry: `ScanRegistration.register` admits a `RegistrationSession` and a mode, runs the mode's pipeline through one `match`, and returns a `RuntimeRail[RegistrationResult]`; the `GLOBAL` arm threads the private `_bootstrap` coarse pose (no initial pose required, the `BootstrapEngine` selected once by the `_engine` interpreter-floor read) that seeds the fine modes and every multiway edge; the `MULTIWAY` arm threads the private `_multiway` that folds each session cloud's pairwise `_bootstrap(cloud, session[0])` solution (source the i-th cloud, target the reference cloud, so the solved pose maps cloud(i)->cloud(0) in the same direction `evaluate_registration` and the `PoseGraphNode` absolute-pose convention assert) into a `PoseGraph` and runs `global_optimization`; the shared Tukey robust point-to-plane estimator binds once above the `match`.
- Auto: the `_engine` floor reads `sys.version_info` once against the `_KISS_FLOOR` `(3, 13)` anchor — below it selects `BootstrapEngine.KISS_MATCHER`, otherwise `BootstrapEngine.OPEN3D_FGR` (the `[KISS_MATCHER_FALLBACK_FGR]` deferred USAGE card, [BLOCKED], reference only — the FGR arm body is authored as the dispatch row but the per-interpreter selection it serves lands with that card); the tensor path runs `t.pipelines.registration.multi_scale_icp` with `TransformationEstimationPointToPlane` over a `TukeyLoss` `RobustKernel` across the `RegistrationPolicy.voxel_schedule`-derived voxel/correspondence pairs and the `multiscale_iterations` criteria list, reading `.transformation`/`.fitness`/`.inlier_rmse` and folding the fitness against the source point count into the inlier estimate rather than zeroing it; the colored path runs the tensor `icp` under `TransformationEstimationForColoredICP(colored_lambda_geometric, kernel)` reading the same triple; the `KISS_MATCHER` bootstrap binds the full `KISSMatcherConfig` gain constructor (`voxel_size`/`use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`) from the policy, runs `KISSMatcher.estimate` over the `float64 (3, n)` transposed source/target positions, reads `RegistrationSolution.rotation`/`.translation` into a 4x4 transform, folds `get_num_final_inliers` into the inlier count and `get_num_rotation_inliers` into the rotation-consistent inlier count, threads the per-stage `get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time` into the timing receipt, and gates every readout on `solution.valid`; the `OPEN3D_FGR` bootstrap downsamples and estimates normals on both legacy clouds in one comprehension, computes `compute_fpfh_feature` descriptors, runs `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`, and folds the correspondence-set length into the inlier count; the VGICP path runs `small_gicp.align` with `registration_type='VGICP'` against the target positions building the internal Gaussian voxel map, reading `RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations` (the iteration count riding the timing receipt), folding the inlier ratio into the fitness; the multiway path folds every-cloud-against-first `_bootstrap` solutions into `PoseGraphNode`/`PoseGraphEdge`, scores each edge with `evaluate_registration` so the edge `uncertain` flag is the measured fitness against the `edge_uncertain_below_fitness` floor rather than a hardcoded `False`, runs `global_optimization`, then re-evaluates the optimized session-final pose through `evaluate_registration` so the multiway receipt carries the real fitness/inlier-RMSE/correspondence-count instead of the placeholder `1.0`/`0.0`/`0`, reading every optimized node `.pose` into the per-cloud transform tuple.
- Receipt: `RegistrationResult.contribute` emits one `emitted`-phase `Receipt.of` row carrying the mode, the `BootstrapEngine` (where the arm bootstraps), the fitness, the inlier RMSE, the inlier count, the rotation-consistent inlier count, and the per-stage timings spread as `t.<stage>` fact keys; `RegistrationResult.graduates(evidence_key)` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=_SUBJECT)`, gating TWO residual keys against the compute owner's per-key ceiling fold — the measured `inlier_rmse` against `_RMSE_CEILING` and the `misfit` residual `1.0 - fitness` against `_MISFIT_CEILING` — so a transform whose alignment residual exceeds the RMSE ceiling OR whose fitness drops below the minimum (a coarse `GLOBAL`/`KISS_MATCHER` pose minting a `0.0` placeholder RMSE no longer graduates on the vacuous ceiling alone, because its inlier-ratio misfit must clear the floor too) is an `Error(BoundaryFault)` on the rail rather than a graduated handoff. The misfit is expressed as the upper-bounded residual `1 - fitness` against a misfit ceiling rather than a fitness lower bound, so the quality floor rides the compute owner's residual-over-ceiling `_admit` direction unchanged — no second admission direction minted here. The subject is typed as the compute-owned `GeometrySubject` `"registration-transform"` literal (imported from `rasm.compute.graduation.handoff`, never a bare `str`, so an unlisted literal fails at the type boundary); this owner is the CONSUMER of the already-declared subject and the supplier of its measured/ceiling ledger, the residual-over-ceiling fold itself the one admission gate the compute owner owns.
- Packages: `kiss_matcher` (`KISSMatcher`/`KISSMatcherConfig` gain constructor incl. `voxel_size`/`use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`/`estimate`/`RegistrationSolution.rotation`/`.translation`/`.valid`/`get_num_final_inliers`/`get_num_rotation_inliers`/`get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time`), `open3d` (`t.geometry.PointCloud.to_legacy`/`t.pipelines.registration.multi_scale_icp`/`icp`/`ICPConvergenceCriteria`/`TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP`/`robust_kernel.RobustKernel`/`RobustKernelMethod.TukeyLoss`/`utility.DoubleVector`/`geometry.PointCloud.voxel_down_sample`/`estimate_normals`/`KDTreeSearchParamHybrid`/`pipelines.registration.compute_fpfh_feature`/`registration_fgr_based_on_feature_matching`/`evaluate_registration`/`PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`/`global_optimization`/`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`), `small_gicp` (`align`/`RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations`), `numpy`, `msgspec` (`Struct`/`field`), runtime (`RuntimeRail`/`boundary`/`Receipt`/`ContentKey`/`LanePolicy`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`). All three compiled registration packages import function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy; none is module-top.
- Growth: a new registration algorithm is one `RegistrationMode` row plus one dispatch arm building through `RegistrationResult.of`; a new robust kernel is one estimator bind; a new tuning axis is one `RegistrationPolicy` field; a new global-bootstrap engine is one `BootstrapEngine` row plus one `_bootstrap` arm plus one `_engine` floor read (the `kiss-matcher` `use_quatro` degenerate-scene solver is one `RegistrationPolicy` field threaded into `KISSMatcherConfig`, not a new engine); the ICP/global-bootstrap loops (the multi-second `_dispatch` kernels) hand across the optional `LanePolicy` lane field through the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter CPU-offload variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`) as ONE `offload(kernel, *args)` hand-off call — `LanePolicy` is the imported lane field so the seam exists and the lane never imports the kernel; zero new surface, no parallel per-algorithm class family.
- Boundary: no IFC tessellation (that is `tessellation`); robust mesh repair and reconstruction cleanup route `mesh-utility`; scan-vs-model deviation routes the `deviation.md#DEVIATION` sibling on the registered pose; raw-scan cleaning is the `ingestion.md#INGESTION` precondition, never re-run here; no durable store; no Rhino/GH mutation. `small-gicp` is a gated enrichment row (Forge-companion-lane, `python_version<'3.15'`), never the spine — it accelerates fine VGICP refinement the cp315-clean spine cannot reach at the multi-threaded scan scale, and the spine never depends on it. A `get_icp`/`get_vgicp`/`register_pair`/`register_multiway` family, a stringly-typed mode dispatch, a fixed-pair signature that cannot fold N clouds, a hand-rolled FPFH/GNC/GICP kernel where the packages own it, selecting the open3d FGR arm on a `<3.13` interpreter where `kiss-matcher` carries a wheel, and a weaker local registration reimplementation are the deleted forms.

```python signature
import sys
import numpy as np
from enum import StrEnum
from typing import assert_never

from msgspec import Struct, field

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.lanes import LanePolicy
from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis

import open3d as o3d


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: GeometrySubject = "registration-transform"
_KISS_FLOOR: tuple[int, int] = (3, 13)
_RMSE_CEILING: float = 0.01
_MISFIT_CEILING: float = 0.7


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


class RegistrationResult(Struct, frozen=True):
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
        mode: RegistrationMode, transform: "np.ndarray", fitness: float, inlier_rmse: float, inliers: int,
        *, engine: BootstrapEngine | None = None, poses: tuple[tuple[float, ...], ...] = (),
        rotation_inliers: int = 0, timings: dict[str, float] | None = None,
    ) -> "RegistrationResult":
        flat = tuple(np.asarray(transform).flatten())
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), float(inlier_rmse),
            int(inliers), int(rotation_inliers), timings or {},
        )

    def contribute(self) -> Receipt:
        facts = {
            "mode": self.mode.value, "engine": self.engine.value if self.engine else "",
            "fitness": repr(self.fitness), "inlier_rmse": repr(self.inlier_rmse),
            "inliers": str(self.inliers), "rotation_inliers": str(self.rotation_inliers),
        } | {f"t.{stage}": repr(seconds) for stage, seconds in self.timings.items()}
        return Receipt.of("emitted", "geometry.scan.registration", self.mode.value, facts)

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

    def register(self, session: tuple["o3d.t.geometry.PointCloud", ...], mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        return boundary(f"scan.{mode}", lambda: self._dispatch(session, mode))

    def _dispatch(self, session: tuple["o3d.t.geometry.PointCloud", ...], mode: RegistrationMode) -> RegistrationResult:
        source, target = session[0], session[1]
        tukey = o3d.t.pipelines.registration.robust_kernel.RobustKernel(
            o3d.t.pipelines.registration.robust_kernel.RobustKernelMethod.TukeyLoss, self.policy.tukey_k
        )
        plane = o3d.t.pipelines.registration.TransformationEstimationPointToPlane(tukey)
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
                reg = o3d.t.pipelines.registration.multi_scale_icp(
                    source, target, o3d.utility.DoubleVector(voxels),
                    [o3d.t.pipelines.registration.ICPConvergenceCriteria(max_iteration=it) for it in self.policy.multiscale_iterations],
                    o3d.utility.DoubleVector(corrs), estimation_method=plane,
                )
                return RegistrationResult.of(
                    mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse, int(reg.fitness * len(source.point.positions.numpy())),
                )
            case RegistrationMode.COLORED_ICP:
                colored = o3d.t.pipelines.registration.TransformationEstimationForColoredICP(self.policy.colored_lambda_geometric, tukey)
                reg = o3d.t.pipelines.registration.icp(source, target, self.policy.max_correspondence, estimation_method=colored)
                return RegistrationResult.of(
                    mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse, int(reg.fitness * len(source.point.positions.numpy())),
                )
            case RegistrationMode.GLOBAL:
                return self._bootstrap(source, target)
            case RegistrationMode.MULTIWAY:
                return self._multiway(session)
            case unreachable:
                assert_never(unreachable)

    def _engine(self) -> BootstrapEngine:
        return BootstrapEngine.KISS_MATCHER if sys.version_info < _KISS_FLOOR else BootstrapEngine.OPEN3D_FGR

    def _bootstrap(self, source: "o3d.t.geometry.PointCloud", target: "o3d.t.geometry.PointCloud") -> RegistrationResult:
        match self._engine():
            case BootstrapEngine.KISS_MATCHER:
                import kiss_matcher  # noqa: PLC0415

                config = kiss_matcher.KISSMatcherConfig(
                    voxel_size=self.policy.voxel, use_quatro=self.policy.use_quatro, thr_linearity=self.policy.thr_linearity,
                    num_max_corr=self.policy.num_max_corr, robin_noise_bound_gain=self.policy.robin_noise_bound_gain,
                    solver_noise_bound_gain=self.policy.solver_noise_bound_gain,
                )
                matcher = kiss_matcher.KISSMatcher(config)
                src = source.point.positions.numpy().astype(np.float64).T
                solution = matcher.estimate(src, target.point.positions.numpy().astype(np.float64).T)
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
                reg = o3d.pipelines.registration
                search = o3d.geometry.KDTreeSearchParamHybrid(self.policy.voxel * 5, 100)
                normals = o3d.geometry.KDTreeSearchParamHybrid(self.policy.voxel * 2, 30)
                down = tuple(cloud.to_legacy().voxel_down_sample(self.policy.voxel) for cloud in (source, target))
                for cloud in down:
                    cloud.estimate_normals(normals)
                features = tuple(reg.compute_fpfh_feature(cloud, search) for cloud in down)
                result = reg.registration_fgr_based_on_feature_matching(down[0], down[1], features[0], features[1])
                return RegistrationResult.of(
                    RegistrationMode.GLOBAL, np.asarray(result.transformation), result.fitness, result.inlier_rmse,
                    len(result.correspondence_set), engine=BootstrapEngine.OPEN3D_FGR,
                )
            case unreachable:
                assert_never(unreachable)

    def _multiway(self, session: tuple["o3d.t.geometry.PointCloud", ...]) -> RegistrationResult:
        reg = o3d.pipelines.registration
        graph = reg.PoseGraph()
        graph.nodes.append(reg.PoseGraphNode(np.identity(4)))
        legacy = tuple(cloud.to_legacy() for cloud in session)
        for i, edge in enumerate(self._bootstrap(cloud, session[0]) for cloud in session[1:]):
            pose = np.asarray(edge.transform).reshape(4, 4)
            evaluation = reg.evaluate_registration(legacy[i + 1], legacy[0], self.policy.max_correspondence, pose)
            graph.nodes.append(reg.PoseGraphNode(pose))
            graph.edges.append(reg.PoseGraphEdge(0, i + 1, pose, uncertain=evaluation.fitness < self.policy.edge_uncertain_below_fitness))
        reg.global_optimization(
            graph, reg.GlobalOptimizationLevenbergMarquardt(), reg.GlobalOptimizationConvergenceCriteria(),
            reg.GlobalOptimizationOption(self.policy.max_correspondence, self.policy.preference_loop_closure, 0),
        )
        poses = tuple(np.asarray(node.pose) for node in graph.nodes)
        final = reg.evaluate_registration(legacy[-1], legacy[0], self.policy.max_correspondence, poses[-1])
        return RegistrationResult.of(
            RegistrationMode.MULTIWAY, poses[-1], final.fitness, final.inlier_rmse, len(final.correspondence_set),
            engine=self._engine(), poses=tuple(tuple(pose.flatten()) for pose in poses),
        )

    def _homogeneous(self, rotation: "np.ndarray", translation: "np.ndarray") -> "np.ndarray":
        transform = np.identity(4)
        transform[:3, :3] = rotation
        transform[:3, 3] = translation
        return transform
```

## [03]-[RESEARCH]

- [TENSOR_MULTISCALE]: the `t.pipelines.registration.multi_scale_icp(source, target, voxel_sizes, criteria_list, max_correspondence_distances, init_source_to_target=eye, estimation_method=...)` signature is folder-`.api`-confirmed (`open3d.md` row [02]) — `voxel_sizes` and `max_correspondence_distances` are `o3d.utility.DoubleVector`, `criteria_list` is a `list[ICPConvergenceCriteria]` (one per scale, NOT an `IntVector` of iteration counts), and `init_source_to_target` defaults to identity so the arm passes `estimation_method=plane` by keyword; the `TransformationEstimationForColoredICP(lambda_geometric, kernel)` arity (`open3d.md` row [04]) and the tensor `icp(source, target, max_correspondence_distance, estimation_method=...)` keyword (`open3d.md` row [01]) are catalogue-confirmed. The only live-run residual is the per-scale `ICPConvergenceCriteria` relative-fitness/relative-rmse tuning, an owner-local heuristic.
- [GLOBAL_KISS_ARITY]: the `KISSMatcher.estimate(src, tgt)` `float64 (3, n)` array overload (`kiss-matcher.md#75`/`#106`, distinct from the `float32 (3, 1)` sequence form), the full `KISSMatcherConfig(voxel_size=..., use_quatro=..., thr_linearity=..., num_max_corr=..., robin_noise_bound_gain=..., solver_noise_bound_gain=...)` gain constructor (`kiss-matcher.md#32`), the `RegistrationSolution.rotation`/`.translation`/`.valid` readouts (`kiss-matcher.md#51-55`), and the `get_num_final_inliers`/`get_num_rotation_inliers`/`get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time` receipt accessors (`kiss-matcher.md#96-100`) confirm against the branch `kiss-matcher` catalogue on the cp312 companion. The `open3d` multiway triple (`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`), `PoseGraphNode.pose` 4x4 readout, and `global_optimization` (`open3d.md` row [08]) confirm against the branch `open3d` catalogue.
- [MULTIWAY_EVALUATION]: the `registration.evaluate_registration(source, target, max_correspondence_distance, transformation)` fitness/inlier-RMSE/correspondence readout (`open3d.md` row [07], same `RegistrationResult` family as row [01]) is the evidence source the multiway arm reads per edge and on the optimized session-final pose, so the `PoseGraphEdge` `uncertain` flag and the multiway receipt carry measured alignment quality rather than placeholder constants; the per-edge `edge_uncertain_below_fitness` floor and the `preference_loop_closure`/`max_correspondence` `GlobalOptimizationOption` arguments are owner-local `RegistrationPolicy` heuristics the live run tunes, not catalogue dependencies.
- [GLOBAL_FGR_FALLBACK]: the `>=3.13` fallback bootstrap composes `geometry.PointCloud.voxel_down_sample` (`open3d.md` row [01]) + `estimate_normals` over `KDTreeSearchParamHybrid` (`open3d.md` rows [03]/[17]) + `pipelines.registration.compute_fpfh_feature` (`open3d.md` row [06]) + `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`/`.correspondence_set` (`open3d.md` row [05], same `RegistrationResult` family as row [01]); this is the multi-library woven coarse-pose rail the `[KISS_MATCHER_FALLBACK_FGR]` deferred USAGE card (geometry, [BLOCKED], reference only) serves — the dispatch arm is authored, the per-interpreter selection it backs lands with that card. Note `open3d` itself ships cp38-cp312 wheels only (`open3d.md#138`), so the FGR fallback is exercisable only where an `open3d` wheel resolves; the `_engine` floor is the structural selector the card finalizes against a live multi-interpreter run.
- [GICP_ALIGN_ARITY]: the `small_gicp.align(target_points, source_points, registration_type='VGICP', downsampling_resolution=..., num_threads=...)` raw-array overload (`small-gicp.md` row [01] + the entrypoint scope note that the raw-array overload carries the full `ICP`/`PLANE_ICP`/`GICP`/`VGICP` set and builds the internal Gaussian voxel map for `VGICP`) and the `RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations` readouts (`small-gicp.md` rows [07]-[09]) confirm against the branch `small-gicp` catalogue; the source-count inlier-ratio fitness is the owner-local fold.

## [04]-[UPSTREAM]

- [GATED_FLOORS]: `kiss-matcher 1.0.2` ships cp38-cp312 wheels (no cp313+, no abi3 — `kiss-matcher.md#12`/`#122`), so it carries marker `python_version<'3.13'` and is the `GLOBAL` primary only on the companion band; `small-gicp 1.0.1` ships cp310-cp314 wheels (no cp315 load path — `small-gicp.md#13`/`#111`), marker `python_version<'3.15'`, a Forge-companion-lane GATED ENRICHMENT row by rail policy, NEVER the spine; `open3d 0.19.0` ships cp38-cp312 wheels (no cp313+ — `open3d.md#138`), marker `python_version<'3.15'` companion band, the source of both the tensor ICP modes and the FGR fallback. All three import function-local under `# noqa: PLC0415` at boundary scope per the manifest import policy; this page shares the `open3d` admission delta with `ingestion.md`/`reconstruction.md`/`deviation.md` (one `open3d` row under `python_version<'3.15'` covers all four), the `kiss-matcher` row under `python_version<'3.13'`, and the `small-gicp` row under `python_version<'3.15'` on the Forge companion lane. The page records the deltas; the orchestrator carries the central manifest rows.
- [FALLBACK_CARD]: the `_engine` interpreter-floor read and the `_bootstrap_fgr` arm are the structure the deferred USAGE card `[KISS_MATCHER_FALLBACK_FGR]` (geometry, [BLOCKED], reference only) finalizes — on a `>=3.13` interpreter where no `kiss-matcher` wheel resolves, the `GLOBAL` mode degrades to the `open3d` Fast Global Registration path that mints the coarse pose seeding the fine modes and multiway edges. The arm body is authored here as the dispatch row (so the page is transcription-complete); the card owns the per-interpreter wheel-resolution selection and its live multi-interpreter verification, not authored as landed selection logic here.
- [GRADUATION_SUBJECT]: the `registration-transform` `GeometrySubject` this owner graduates is already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union, so the `_SUBJECT` constant imports the literal from `rasm.compute.graduation.handoff` rather than minting a bare `str` — an unlisted literal fails at the `GeometrySubject` type boundary, the compute owner owning the union. `RegistrationResult.graduates` routes a two-key measured ledger (`inlier_rmse` and the `misfit` residual `1 - fitness`) through the one compute `GraduationReceipt.graduates` admission fold against the `_RMSE_CEILING`/`_MISFIT_CEILING` residual bars — the per-key residual-over-ceiling gate the compute owner owns, not a local admission body — so a transform whose RMSE exceeds the ceiling or whose fitness falls below the floor (a coarse `GLOBAL` bootstrap with a `0.0` placeholder RMSE included) is an `Error(BoundaryFault)` rather than a graduated handoff. The misfit residual rides the compute owner's existing upper-bound `_admit` direction, so no new literal, no new gate, and no second admission direction are minted; this page is a CONSUMER of the already-declared subject and the compute admission rail, supplying only its measured/ceiling ledger, never authoring the compute interior.
- [OFFLOAD_LANE]: the `_dispatch` ICP/bootstrap/multiway kernels are heavy geometry CPU kernels the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant absorbs through `anyio.to_interpreter.run_sync` under one `CapacityLimiter` — the no-pickle PEP-734 hop the lanes owner confirms (`lanes.md` ENTRYPOINTS `LanePolicy.offload`), degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner explicitly rejects as the process-pool serialization tax. The optional `lane: LanePolicy | None` field on `ScanRegistration` is the imported seam, the hand-off a uniform one `offload(kernel, *args)` growth on the Growth bullet shared with `ingestion.md#INGESTION`, `reconstruction.md#RECONSTRUCTION`, `daemon.md#DAEMON`, and `repair.md#MESH`, sequence-after the runtime lane, never a second concurrency surface minted in geometry.
