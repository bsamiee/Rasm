# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration over an N-cloud session, not a fixed pair. `ScanRegistration` is one registration owner discriminating by `RegistrationMode` row over a `RegistrationSession` — the `tuple[PointCloud, PointCloud, *tuple[PointCloud, ...]]` of two-or-more `o3d.t.geometry.PointCloud` clouds (the pairwise modes read the first two, the multiway mode folds all N), the `>=2` arity carried in the type so a length-1 session is a boundary type error, never a runtime `IndexError` — so the pair is the degenerate `N==2` case of the session, never a parallel surface. The modes: the initialization-free global bootstrap (`kiss-matcher` Faster-PFH/ROBIN/GNC where available, otherwise `open3d` FGR-over-FPFH, no initial pose), coarse-to-fine `multi_scale_icp` over the open3d tensor backend with a Tukey-robust point-to-plane estimator, colored point-to-plane ICP, the `small-gicp` VGICP parallel-multithreaded fine-refinement speed path, and N-cloud multiway pose-graph optimization, with the robust point-to-plane estimator and voxel/correspondence schedule as the shared pre-step. The `GLOBAL` coarse pose seeds the fine modes and every multiway pairwise edge.

The entry is `async` and no ICP, RANSAC, or pose-graph solve runs on the event loop: `register` composes the graduation `evidence_run` weave (`EvidenceScope.SCAN_REGISTRATION` the seed row — no page-local tracer mint, no hand-authored span/`_ok` pair) around `lane.offload(_register_kernel, ...)`, the module-level picklable kernel the runtime per-subinterpreter hop carries. The registration transform graduates through the geometry-minted `rasm.geometry.graduation` spine as the `GeometrySubject.REGISTRATION_TRANSFORM` member — `graduates()` returns the local `GeometryHandoff` carrier whose `wire()` projection is the compute crossing, and the RMSE/misfit ceilings are `RegistrationPolicy` rows, never module `Final`s. Reconstruction and mesh repair route the `mesh/` sub-domain.

## [01]-[INDEX]

- [01]-[REGISTRATION]: mode-discriminated registration over an N-cloud session, the global bootstrap (`kiss-matcher` primary / `open3d` FGR fallback), the N-cloud multiway pose-graph, and the shared estimation pre-step — the entry an `async` composition of the graduation `evidence_run` weave over `lane.offload` — the weave's harvest the one receipt egress — and the `GeometryHandoff` graduation carrier with policy-row ceilings.

## [02]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` row over a `RegistrationSession` (the `type RegistrationSession = tuple[PointCloud, PointCloud, *tuple[PointCloud, ...]]` PEP-646 alias whose `>=2` arity the type carries so a length-1 session fails at the boundary; the pairwise modes read `session[0]`/`session[1]`, the multiway mode folds the whole tuple); `RegistrationPolicy` the frozen tuning carrier (voxel, max-correspondence, Tukey scale, the multiscale iteration schedule, the colored geometric-weight, the `kiss-matcher` solver gains `use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`, the multiway edge-uncertainty fitness floor and loop-closure preference, AND the graduation ceilings `rmse_ceiling`/`misfit_ceiling` — every bar a policy row, never a module `Final`) with a derived `voxel_schedule` property; `RegistrationResult` the typed `gc=False` receipt carrying the mode, the chosen `BootstrapEngine`, the 4x4 transform (the session-final node pose for multiway), the per-cloud transform tuple, the fitness, the inlier RMSE, the inlier count, the rotation-consistent inlier count, and the per-stage timings, with a `RegistrationResult.of` factory that ravels the transform once and defaults the single-pose tuple and a `RegistrationResult._from_tensor` projector the two tensor arms share — the receipt conforms structurally to the runtime-checkable `ReceiptContributor` Protocol the weave's harvest reads; `BootstrapEngine` the global-coarse-pose vocabulary (`KISS_MATCHER` | `OPEN3D_FGR`) the engine selection selects.
- Cases: `RegistrationMode` rows `GLOBAL` (the initialization-free coarse pose — `kiss-matcher` Faster-PFH/ROBIN/GNC where available, otherwise `open3d` FGR-over-FPFH, no initial pose) · `MULTISCALE` (coarse-to-fine `t.pipelines.multi_scale_icp` with a Tukey-robust point-to-plane estimator) · `COLORED_ICP` (colored point-to-plane over the open3d tensor backend) · `VGICP` (the `small-gicp` voxelized parallel fine-refinement speed path) · `MULTIWAY` (N-cloud pose-graph optimization over a multi-station session) — matched by `match`/`assert_never`, each binding the engine and estimator that owns it.
- Entry: `ScanRegistration.register` is `async` — it admits a `RegistrationSession` and a mode and returns `RuntimeRail[RegistrationResult]` by composing `evidence_run(EvidenceScope.SCAN_REGISTRATION, f"register.{mode}", partial(self.lane.offload, _register_kernel, session, mode, self.policy))`: the graduation weave opens the seeded span, `async_boundary` fences the offload, the weave's `_flat` absorbs the lane's already-fenced rail un-nested, and the weave's harvest emits the structurally-conforming `RegistrationResult.contribute` stream exactly once on the cleared `Ok` while an open3d/kiss-matcher raise stays an `Error(BoundaryFault)` recorded on the live span. `_register_kernel` is module-level and picklable — the lane imports neither the kernel nor any compiled package. The `GLOBAL` arm threads the private `_bootstrap(source, target, engine, policy)` coarse pose (no initial pose required, the `BootstrapEngine` resolved once by `_engine` and passed in, never re-read per call) that seeds the fine modes and every multiway edge; the `MULTIWAY` arm threads `_multiway`, which reads `_engine` once and folds each session cloud's pairwise `_bootstrap(cloud, session[0], engine, policy)` solution into a `PoseGraph` and runs `global_optimization`; the two tensor arms bind the shared Tukey `RobustKernel` through `_tukey` and fold the open3d tensor result through the one `RegistrationResult._from_tensor` projector rather than re-spelling the construction per arm.
- Auto: the `_engine` selector resolves the active worker lane once: `BootstrapEngine.KISS_MATCHER` when `kiss-matcher` resolves, otherwise `BootstrapEngine.OPEN3D_FGR`; the tensor path runs `t.pipelines.registration.multi_scale_icp` with `TransformationEstimationPointToPlane` over the `_tukey`-built `TukeyLoss` `RobustKernel` across the `RegistrationPolicy.voxel_schedule`-derived voxel/correspondence pairs and the `multiscale_iterations` criteria list (a `list[ICPConvergenceCriteria]`, one per scale, never an `IntVector`), then folds the tensor result through the one `RegistrationResult._from_tensor` projector that reads `.transformation`/`.fitness`/`.inlier_rmse` and folds the fitness against the source point count into the inlier estimate rather than zeroing it; the colored path runs the tensor `icp` under `TransformationEstimationForColoredICP(colored_lambda_geometric, _tukey(policy))` through the same `_from_tensor` fold; the `KISS_MATCHER` bootstrap binds the full `KISSMatcherConfig` gain constructor from the policy, runs the `match` + `prune_and_solve` decomposition over the `float64 (3, n)` transposed source/target positions (the array overload `match`/`prune_and_solve`/`solve` carry, NOT the `float32 (3, 1)` sequence form `estimate` documents, so the array path threads the decomposition that keeps every stage accessor populated), reads `RegistrationSolution.rotation`/`.translation` into a 4x4 transform, folds `get_num_final_inliers` into the inlier count and `get_num_rotation_inliers` into the rotation-consistent inlier count, threads the per-stage `get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time` into the timing receipt, and gates every readout on `solution.valid`; the `OPEN3D_FGR` bootstrap downsamples and estimates normals on both legacy clouds in one comprehension, computes `compute_fpfh_feature` descriptors, runs `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`, and folds the correspondence-set length into the inlier count; the VGICP path runs `small_gicp.align` with `registration_type='VGICP'` against the target positions building the internal Gaussian voxel map, reading `RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations`, folding the inlier ratio into the fitness; the multiway path folds every-cloud-against-first `_bootstrap` solutions through one `Block.of_seq(session[1:]).mapi(...)` over the `_edge` projector into fully-decided `(PoseGraphNode, PoseGraphEdge)` pairs — the per-edge domain math (the cloud(i+1)->cloud(0) pose, the `evaluate_registration` fitness scoring the `uncertain` flag against the `edge_uncertain_below_fitness` floor rather than a hardcoded `False`) living in the fold so the `PoseGraph` boundary bind is a pure append over already-decided edges — runs `global_optimization` with the keyword-bound `GlobalOptimizationOption`, then re-evaluates the optimized session-final pose through `evaluate_registration` so the multiway receipt carries the real fitness/inlier-RMSE/correspondence-count instead of placeholders, reading every optimized node `.pose` into the per-cloud transform tuple.
- Receipt: emission is the graduation weave's harvest — `evidence_run` reads the structurally-conforming `RegistrationResult` on the cleared `Ok` and emits its `contribute` stream exactly once, never an inline `Signals.emit` and never a page-local `@receipted` leg; `contribute` returns the one-element `tuple[Receipt, ...]` through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("geometry.scan.registration", ("emitted", mode, facts))` minting the `fact` case, never a four-positional call. The facts are produced once through `RegistrationResult.facts` carrying the mode, the `BootstrapEngine`, the fitness, the inlier RMSE, the inlier and rotation-consistent inlier counts, and the per-stage timings spread as `t.<stage>` fact keys as NATIVE `float`/`int` slots the receipts `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce. `RegistrationResult.graduates(evidence_key, policy)` returns the local `GeometryHandoff` carrier — `GeometryHandoff.of(GeometrySubject.REGISTRATION_TRANSFORM, key, measured, ceilings)` with TWO measured keys, the `inlier_rmse` against `policy.rmse_ceiling` and the `misfit` residual `1.0 - fitness` against `policy.misfit_ceiling`, so a transform whose alignment residual exceeds the RMSE ceiling OR whose fitness drops below the floor (a coarse `GLOBAL` pose minting a `0.0` placeholder RMSE no longer clears on the vacuous key alone) breaches the carrier's residual-over-ceiling `admitted` verdict rather than crossing clean. The misfit is expressed as the upper-bounded residual `1 - fitness` so the quality floor rides the graduation owner's one `_admit` direction unchanged — no second admission direction minted here; the crossing to compute is the carrier's `wire()` data, never an import.
- Packages: `kiss_matcher` (`KISSMatcher`/`KISSMatcherConfig` gain constructor incl. `voxel_size`/`use_quatro`/`thr_linearity`/`num_max_corr`/`robin_noise_bound_gain`/`solver_noise_bound_gain`/`match`/`prune_and_solve`/`RegistrationSolution.rotation`/`.translation`/`.valid`/`get_num_final_inliers`/`get_num_rotation_inliers`/`get_extraction_time`/`get_matching_time`/`get_rejection_time`/`get_solver_time`/`get_processing_time`), `open3d` (`t.geometry.PointCloud.to_legacy`/`t.pipelines.registration.multi_scale_icp`/`icp`/`ICPConvergenceCriteria`/`TransformationEstimationPointToPlane`/`TransformationEstimationForColoredICP`/`robust_kernel.RobustKernel`/`RobustKernelMethod.TukeyLoss`/`utility.DoubleVector`/`geometry.PointCloud.voxel_down_sample`/`estimate_normals`/`KDTreeSearchParamHybrid`/`pipelines.registration.compute_fpfh_feature`/`registration_fgr_based_on_feature_matching`/`evaluate_registration`/`PoseGraph`/`PoseGraphNode`/`PoseGraphEdge`/`global_optimization`/`GlobalOptimizationLevenbergMarquardt`/`GlobalOptimizationConvergenceCriteria`/`GlobalOptimizationOption`), `small_gicp` (`align`/`RegistrationResult.T_target_source`/`.num_inliers`/`.error`/`.iterations`), `numpy` (`asarray`/`eye`/`ravel`/`reshape` over the open3d/kiss-matcher transform arrays — never the uncatalogued `np.identity`/`ndarray.flatten`), `expression` (`Block.of_seq`/`Block.mapi` the per-edge multiway pair fold), `msgspec` (`Struct`/`field`/`gc=False` on the leaf receipt), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine — the span, fence, and harvest this page composes instead of minting), runtime (`RuntimeRail`, `LanePolicy.offload` the per-subinterpreter hop, `ContentKey` from `rasm.runtime.identity`, `Receipt`). All three compiled registration packages import function-local at boundary scope under `# noqa: PLC0415`; none is module-top.
- Growth: a new registration engine is one `RegistrationMode` row plus one kernel arm; a new bootstrap backend is one `BootstrapEngine` member plus one `_bootstrap` arm; a stricter graduation bar is a `RegistrationPolicy` ceiling value the caller passes; the RANSAC feature-matching global arm (`registration_ransac_based_on_feature_matching`) is the named next `BootstrapEngine` row when a scene defeats both standing engines; zero new surface.
- Boundary: the cleaned input cloud is `scan/ingestion.md#INGESTION`'s product (a same-folder seam); deviation against a reference is `scan/deviation.md#DEVIATION`; surface reconstruction is `scan/reconstruction.md#RECONSTRUCTION`; no mesh repair, no tessellation, no durable store. The deleted forms: a sync `register` blocking the event loop on an ICP/pose-graph solve where the lane offload isolates it; a `lane: LanePolicy | None` accepted yet never composed; a page-local `trace.get_tracer` mint, hand-authored span/`_ok` pair, or `@receipted` `_emit` leg re-emitting what the weave's harvest already emits; a module-`Final` RMSE/misfit ceiling where the policy rows carry the bars; a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff`; a pairwise-only `register(source, target)` beside a session surface; a hardcoded `uncertain=False` pose-graph edge; a placeholder `1.0`/`0.0`/`0` multiway receipt; a positional `GlobalOptimizationOption` call that lands the loop-closure gain in `edge_prune_threshold`; a four-positional `Receipt.of` or a `str()`-coerced facts map; and the uncatalogued `np.identity`/`ndarray.flatten` spellings.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from typing import TYPE_CHECKING, assert_never

import numpy as np
from expression.collections import Block
from msgspec import Struct, field

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt

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
    rmse_ceiling: float = 0.01  # the graduation alignment bar; a policy row, never a module Final
    misfit_ceiling: float = 0.7  # the graduation 1-fitness bar

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
        mode: RegistrationMode,
        transform: np.ndarray,
        fitness: float,
        inlier_rmse: float,
        inliers: int,
        *,
        engine: BootstrapEngine | None = None,
        poses: tuple[tuple[float, ...], ...] = (),
        rotation_inliers: int = 0,
        timings: dict[str, float] | None = None,
    ) -> "RegistrationResult":
        flat = tuple(np.ravel(np.asarray(transform)))  # the catalogued shape op owns the row-major flatten
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), float(inlier_rmse), int(inliers), int(rotation_inliers), timings or {}
        )

    @staticmethod
    def _from_tensor(
        mode: RegistrationMode, reg: "o3d.t.pipelines.registration.RegistrationResult", source: "o3d.t.geometry.PointCloud"
    ) -> "RegistrationResult":
        # one fold for every tensor-backend arm (MULTISCALE/COLORED_ICP): the open3d `.fitness` is the
        # matched-source fraction, so `fitness * |source|` is the inlier estimate rather than zeroing it.
        return RegistrationResult.of(
            mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse, int(reg.fitness * source.point.positions.shape[0])
        )

    def facts(self) -> dict[str, object]:
        # native float/int slots: the receipts EventDict serializes them without a str()/repr() coerce.
        return {
            "mode": self.mode.value,
            "engine": self.engine.value if self.engine else "",
            "fitness": self.fitness,
            "inlier_rmse": self.inlier_rmse,
            "inliers": self.inliers,
            "rotation_inliers": self.rotation_inliers,
        } | {f"t.{stage}": seconds for stage, seconds in self.timings.items()}

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract minting the `fact` case.
        return (Receipt.of("geometry.scan.registration", ("emitted", self.mode.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey, policy: RegistrationPolicy) -> GeometryHandoff:
        # two measured keys against two policy-row ceilings: a coarse pose with a 0.0 placeholder RMSE
        # no longer clears on the vacuous key alone — its inlier-ratio misfit must clear the floor too.
        return GeometryHandoff.of(
            GeometrySubject.REGISTRATION_TRANSFORM,
            evidence_key,
            {"inlier_rmse": self.inlier_rmse, "misfit": 1.0 - self.fitness},
            {"inlier_rmse": policy.rmse_ceiling, "misfit": policy.misfit_ceiling},
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _engine() -> BootstrapEngine:
    return BootstrapEngine.KISS_MATCHER if find_spec("kiss_matcher") is not None else BootstrapEngine.OPEN3D_FGR


def _tukey(policy: RegistrationPolicy) -> "o3d.t.pipelines.registration.robust_kernel.RobustKernel":
    import open3d as o3d  # noqa: PLC0415

    rk = o3d.t.pipelines.registration.robust_kernel
    return rk.RobustKernel(rk.RobustKernelMethod.TukeyLoss, policy.tukey_k)


def _homogeneous(rotation: np.ndarray, translation: np.ndarray) -> np.ndarray:
    transform = np.eye(4)  # the catalogued identity creator; never the uncatalogued `np.identity`
    transform[:3, :3] = rotation
    transform[:3, 3] = translation
    return transform


def _bootstrap(
    source: "o3d.t.geometry.PointCloud", target: "o3d.t.geometry.PointCloud", engine: BootstrapEngine, policy: RegistrationPolicy
) -> RegistrationResult:
    match engine:
        case BootstrapEngine.KISS_MATCHER:
            import kiss_matcher  # noqa: PLC0415

            config = kiss_matcher.KISSMatcherConfig(
                voxel_size=policy.voxel,
                use_quatro=policy.use_quatro,
                thr_linearity=policy.thr_linearity,
                num_max_corr=policy.num_max_corr,
                robin_noise_bound_gain=policy.robin_noise_bound_gain,
                solver_noise_bound_gain=policy.solver_noise_bound_gain,
            )
            matcher = kiss_matcher.KISSMatcher(config)
            # the `float64 (3, n)` array overload is `match`/`prune_and_solve`/`solve`'s, NOT `estimate`'s
            # `float32 (3, 1)` sequence form; the decomposition keeps every stage-timing accessor populated.
            src = np.asarray(source.point.positions.numpy(), dtype=np.float64).T
            src_matched, tgt_matched = matcher.match(src, np.asarray(target.point.positions.numpy(), dtype=np.float64).T)
            solution = matcher.prune_and_solve(src_matched, tgt_matched)
            inliers = matcher.get_num_final_inliers() if solution.valid else 0
            return RegistrationResult.of(
                RegistrationMode.GLOBAL,
                _homogeneous(np.asarray(solution.rotation), np.asarray(solution.translation)),
                float(inliers) / max(src.shape[1], 1),
                0.0,
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
            import open3d as o3d  # noqa: PLC0415

            reg = o3d.pipelines.registration
            search = o3d.geometry.KDTreeSearchParamHybrid(policy.voxel * 5, 100)
            normals = o3d.geometry.KDTreeSearchParamHybrid(policy.voxel * 2, 30)
            # `estimate_normals` mutates in place and returns `None`, so the downsample + normal-estimate
            # fold threads the in-place conditioning inside the one comprehension (`or cloud` yields the
            # conditioned cloud past the `None` return) rather than a bare statement loop.
            down = tuple(
                cloud.estimate_normals(normals) or cloud
                for cloud in (s.to_legacy().voxel_down_sample(policy.voxel) for s in (source, target))
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


def _edge(
    reg: "o3d.pipelines.registration",
    legacy: tuple["o3d.geometry.PointCloud", ...],
    i: int,
    solution: RegistrationResult,
    policy: RegistrationPolicy,
) -> tuple["o3d.pipelines.registration.PoseGraphNode", "o3d.pipelines.registration.PoseGraphEdge"]:
    # the `uncertain` flag is the measured `evaluate_registration` fitness against the policy floor, not a
    # hardcoded `False`; the pose maps cloud(i+1)->cloud(0) — the `PoseGraphNode` absolute-pose convention —
    # so the edge declares (source=i+1, target=0): `PoseGraphEdge` expects the source->target transform, and
    # an (0, i+1) edge carrying this un-inverted pose is the deleted direction-flip defect.
    pose = np.reshape(np.asarray(solution.transform), (4, 4))
    fitness = reg.evaluate_registration(legacy[i + 1], legacy[0], policy.max_correspondence, pose).fitness
    node = reg.PoseGraphNode(pose)
    edge = reg.PoseGraphEdge(i + 1, 0, pose, uncertain=fitness < policy.edge_uncertain_below_fitness)
    return node, edge


def _multiway(session: RegistrationSession, policy: RegistrationPolicy) -> RegistrationResult:
    import open3d as o3d  # noqa: PLC0415

    reg = o3d.pipelines.registration
    engine = _engine()  # one engine-selection read; every pairwise edge solves on the same bootstrap engine
    legacy = tuple(cloud.to_legacy() for cloud in session)
    # the per-edge domain math folds once through one `Block.mapi` over the non-reference clouds into
    # fully-formed `(node, edge)` pairs, so the `PoseGraph` boundary bind is a pure append over
    # already-decided edges, never a domain accumulation with index arithmetic.
    pairs = Block.of_seq(session[1:]).mapi(lambda i, cloud: _edge(reg, legacy, i, _bootstrap(cloud, session[0], engine, policy), policy))
    graph = reg.PoseGraph()
    graph.nodes.append(reg.PoseGraphNode(np.eye(4)))
    for node, edge in pairs:
        graph.nodes.append(node)
        graph.edges.append(edge)
    reg.global_optimization(
        graph,
        reg.GlobalOptimizationLevenbergMarquardt(),
        reg.GlobalOptimizationConvergenceCriteria(),
        # keyword-bound: the positional order interleaves `edge_prune_threshold` between the two policy
        # gains, so the loop-closure preference MUST name its slot; node 0 (identity root) is fixed.
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


def _register_kernel(session: RegistrationSession, mode: RegistrationMode, policy: RegistrationPolicy) -> RegistrationResult:
    # the module-level picklable kernel the lane offloads; a raise here converts through the lane's
    # async_boundary onto the rail the graduation weave records on the live span.
    import open3d as o3d  # noqa: PLC0415

    source, target = session[0], session[1]
    reg_t = o3d.t.pipelines.registration
    match mode:
        case RegistrationMode.VGICP:
            import small_gicp  # noqa: PLC0415

            src = source.point.positions.numpy()
            result = small_gicp.align(
                target.point.positions.numpy(),
                src,
                registration_type="VGICP",
                downsampling_resolution=policy.voxel,
                num_threads=policy.num_threads,
            )
            return RegistrationResult.of(
                mode,
                result.T_target_source,
                result.num_inliers / max(len(src), 1),
                result.error,
                result.num_inliers,
                timings={"iterations": float(result.iterations)},
            )
        case RegistrationMode.MULTISCALE:
            voxels, corrs = zip(*policy.voxel_schedule)
            reg = reg_t.multi_scale_icp(
                source,
                target,
                o3d.utility.DoubleVector(voxels),
                [reg_t.ICPConvergenceCriteria(max_iteration=it) for it in policy.multiscale_iterations],
                o3d.utility.DoubleVector(corrs),
                estimation_method=reg_t.TransformationEstimationPointToPlane(_tukey(policy)),
            )
            return RegistrationResult._from_tensor(mode, reg, source)
        case RegistrationMode.COLORED_ICP:
            colored = reg_t.TransformationEstimationForColoredICP(policy.colored_lambda_geometric, _tukey(policy))
            reg = reg_t.icp(source, target, policy.max_correspondence, estimation_method=colored)
            return RegistrationResult._from_tensor(mode, reg, source)
        case RegistrationMode.GLOBAL:
            return _bootstrap(source, target, _engine(), policy)
        case RegistrationMode.MULTIWAY:
            return _multiway(session, policy)
        case unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanRegistration(Struct, frozen=True):
    lane: LanePolicy
    policy: RegistrationPolicy = RegistrationPolicy()

    async def register(self, session: RegistrationSession, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        # the one composed entry: the graduation weave (seeded span + fence + harvest) wraps the lane
        # offload — `partial` keeps the dispatch a coroutine function the weave's modality probe reads,
        # the weave's `_flat` absorbs the lane's already-fenced rail un-nested, and its harvest emits
        # the conforming `RegistrationResult` exactly once; no page-local receipt leg exists.
        return await evidence_run(
            EvidenceScope.SCAN_REGISTRATION, f"register.{mode}", partial(self.lane.offload, _register_kernel, session, mode, self.policy)
        )
```
