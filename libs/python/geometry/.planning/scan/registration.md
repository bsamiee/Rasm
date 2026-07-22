# [PY_GEOMETRY_SCAN_REGISTRATION]

Point-cloud and 3D-scan registration over an N-cloud session, not a fixed pair: `ScanRegistration` discriminates every alignment strategy by `RegistrationMode` over a `RegistrationSession`, the two-or-more-cloud tuple whose `>=2` arity the PEP-646 type carries statically and `register` re-proves at runtime, so a length-1 session refuses typed at the boundary, never as a worker-side `IndexError`. Sessions carry the `scan/ingestion.md#INGESTION` `Cloud` array carrier, never a live `open3d` cloud — a tensor point cloud is a pybind11 handle no pickler carries, so the arrays cross the process seam whole and each kernel arm re-inflates through `Cloud.tensor()`/`legacy()` where its own native solve begins. Pairwise modes read the first two clouds and the multiway mode folds all N, so a pair is the degenerate `N==2` case of the session, never a parallel surface. Nearest-neighbor registration and pose-graph alignment are this owner's charter — `mesh/spatial.md#SPATIAL` composes `open3d`/`small-gicp` as query libraries but owns no registration capability.

`register` runs `async`, so no ICP, RANSAC, or pose-graph solve touches the event loop: it composes the graduation `evidence_run` weave (seeded `EvidenceScope.SCAN_REGISTRATION`, no page-local tracer or span/`_ok` pair) around the `lane.offload` crossing on `Kernel.of(_register_kernel, KernelTrait.HOSTILE)` — the `open3d`/`small-gicp`/`kiss-matcher` band holds process-global native state and imports under no isolated subinterpreter, so the module-level kernel ships `REFERENCE` onto the warm process pool and the lane imports neither the kernel nor any compiled package. Each transform graduates through the `rasm.geometry.graduation` spine as `GeometrySubject.REGISTRATION_TRANSFORM`; `graduates()` returns the local `GeometryHandoff` whose `wire()` projection is the compute crossing, and alignment ceilings ride `RegistrationPolicy` rows.

## [01]-[INDEX]

- [01]-[REGISTRATION]: mode-discriminated alignment over an N-cloud session — global bootstrap, coarse-to-fine and colored ICP, VGICP fine-refinement, and multiway pose-graph — behind one `async` graduation-weave entry.

## [02]-[REGISTRATION]

- Owner: `ScanRegistration` — the frozen owner discriminating by `RegistrationMode` over a `RegistrationSession`; `RegistrationPolicy` is the one tuning carrier for every voxel/correspondence/Tukey/solver-gain/multiway bar including the graduation ceilings, with a derived `voxel_schedule`; `RegistrationResult` the `gc=False` receipt whose `of` factory ravels the transform once and defaults the single-pose tuple, sharing one `_from_tensor` projector across the tensor arms and conforming structurally to the `ReceiptContributor` the weave's harvest reads; `BootstrapEngine` (`KISS_MATCHER` | `OPEN3D_FGR`) the global-coarse-pose vocabulary.
- Cases: `RegistrationMode` rows — `GLOBAL` (initialization-free coarse pose, no initial pose), `MULTISCALE` (coarse-to-fine tensor ICP, Tukey-robust point-to-plane), `COLORED_ICP` (colored point-to-plane), `VGICP` (`small-gicp` voxelized parallel fine-refinement speed path), `MULTIWAY` (N-cloud pose-graph). `GLOBAL`'s coarse pose seeds every fine mode and every multiway pairwise edge; each arm binds the engine and estimator that owns it.
- Entry: `register` admits a session and a mode and returns `RuntimeRail[RegistrationResult]`. Its weave opens the seeded span, `async_boundary` fences the offload, `_flat` absorbs the lane's already-fenced rail un-nested, and the harvest emits the conforming result once on the cleared `Ok` while an `open3d`/`kiss-matcher` raise stays an `Error(BoundaryFault)` on the live span. The kernel takes the lane conduit's pickled tap as its trailing offload arg and beats the graduation `GeometryPulse.REGISTRATION` point through `pulsed` — one solve-start beat per mode, one per multiway edge — so a `Hooks` tap streams convergence progress under the lane's lossy drop law.
- Auto: `_engine` resolves the bootstrap backend once per worker lane — `KISS_MATCHER` when `kiss_matcher` resolves, else `OPEN3D_FGR` — and every arm (`GLOBAL`, each `MULTIWAY` edge) reuses that one decision; the tensor arms share the `_tukey` robust kernel and the `_from_tensor` projector rather than re-reading the `open3d` result per arm.
- Receipt: emission is the weave's harvest — the conforming `RegistrationResult.contribute` streams once on the cleared `Ok`, never an inline emit or page-local `@receipted` leg. `graduates` measures two keys against two policy ceilings, `inlier_rmse` against `rmse_ceiling` and the `1 - fitness` misfit against `misfit_ceiling`, so a coarse `GLOBAL` pose minting a `0.0` placeholder RMSE cannot clear on the vacuous key alone — its inlier-ratio misfit must clear the floor too. That misfit rides the graduation owner's single `_admit` residual-over-ceiling direction, so no second admission direction is minted here.
- Bench: `bench` rides the graduation `bench_seam` fold over the whole `register` crossing — arity re-proof, offload, solver, weave — cloud-size-parameterized: the subject keys the exact `RegistrationMode` row and the source point count as `rasm.geometry.scan.registration.<mode>.p<points>`, so a latency row compares like-for-like across scan densities; latency and throughput rows per arm, zero instrument rows, graduation's `bench_terminal` wrapping the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Packages: `kiss_matcher`, `open3d`, `small_gicp` (the three compiled registration backends, each imported function-local at boundary scope under `# noqa: PLC0415`, never module-top), `numpy` (transform assembly via `np.eye`/`np.ravel`/`np.reshape`, never the uncatalogued `np.identity`/`ndarray.flatten`), `expression` (`Block.mapi` the per-edge multiway fold), `msgspec`, and the geometry graduation spine and runtime rails per the fence imports.
- Growth: a new registration engine is one `RegistrationMode` row and one kernel arm; a new bootstrap backend is one `BootstrapEngine` member and one `_bootstrap` arm; a stricter graduation bar is a `RegistrationPolicy` ceiling the caller passes. `registration_ransac_based_on_feature_matching` is the named next `BootstrapEngine` row when a scene defeats both standing engines.
- Boundary: the cleaned input `Cloud` is `scan/ingestion.md#INGESTION`'s product and carrier mint; deviation against a reference is `scan/deviation.md#DEVIATION`; surface reconstruction is `scan/reconstruction.md#RECONSTRUCTION`. No mesh repair, tessellation, or durable store here.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from queue import Queue
from typing import TYPE_CHECKING, assert_never

import numpy as np
from expression import Error
from expression.collections import Block
from msgspec import Struct, field

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometryPulse, GeometrySubject, PulseBeat, bench_seam, evidence_run
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    import open3d as o3d  # type-only; the runtime import is function-local so the module loads clean

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


# >=2 Cloud carriers: pairwise modes read [0]/[1], MULTIWAY needs two for one edge; arrays cross, o3d rebuilds worker-side
type RegistrationSession = tuple[Cloud, Cloud, *tuple[Cloud, ...]]


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
    rmse_ceiling: float = 0.01  # policy-row alignment bar, never a module Final
    misfit_ceiling: float = 0.7  # the 1-fitness bar

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
        flat = tuple(np.ravel(np.asarray(transform)))  # the catalogued row-major flatten
        return RegistrationResult(
            mode, engine, flat, poses or (flat,), float(fitness), float(inlier_rmse), int(inliers), int(rotation_inliers), timings or {}
        )

    @staticmethod
    def _from_tensor(
        mode: RegistrationMode, reg: "o3d.t.pipelines.registration.RegistrationResult", source: "o3d.t.geometry.PointCloud"
    ) -> "RegistrationResult":
        # open3d `.fitness` is the matched-source fraction; `fitness * |source|` is the inlier estimate
        return RegistrationResult.of(
            mode, reg.transformation.numpy(), reg.fitness, reg.inlier_rmse, int(reg.fitness * source.point.positions.shape[0])
        )

    def facts(self) -> dict[str, object]:
        # native float/int slots the receipts renderer serializes without a str()/repr() coerce
        return {
            "mode": self.mode.value,
            "engine": self.engine.value if self.engine else "",
            "fitness": self.fitness,
            "inlier_rmse": self.inlier_rmse,
            "inliers": self.inliers,
            "rotation_inliers": self.rotation_inliers,
        } | {f"t.{stage}": seconds for stage, seconds in self.timings.items()}

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.registration", ("emitted", self.mode.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey, policy: RegistrationPolicy) -> GeometryHandoff:
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
    transform = np.eye(4)  # catalogued identity creator, never `np.identity`
    transform[:3, :3] = rotation
    transform[:3, 3] = translation
    return transform


def _bootstrap(source: Cloud, target: Cloud, engine: BootstrapEngine, policy: RegistrationPolicy) -> RegistrationResult:
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
    import open3d as o3d  # noqa: PLC0415

    reg = o3d.pipelines.registration
    engine = _engine()  # one engine read; every edge solves on the same bootstrap engine
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
    session: RegistrationSession, mode: RegistrationMode, policy: RegistrationPolicy, tap: "Queue[PulseFact | None]"
) -> RegistrationResult:
    # module-level HOSTILE kernel: Cloud carriers cross the process seam as arrays, each arm re-inflates the o3d form
    # its solver needs, and a raise converts through the lane's async_boundary onto the rail
    import open3d as o3d  # noqa: PLC0415

    source, target = session[0], session[1]
    pulsed(tap, GeometryPulse.REGISTRATION, PulseBeat(stage=f"solve.{mode.value}", done=0, total=1))  # solve-start beat before the native arm
    reg_t = o3d.t.pipelines.registration
    match mode:
        case RegistrationMode.VGICP:
            import small_gicp  # noqa: PLC0415

            # small_gicp.align consumes bare (N, 3) arrays, so the carrier feeds it with no o3d rebuild at all
            result = small_gicp.align(
                target.positions,
                source.positions,
                registration_type="VGICP",
                downsampling_resolution=policy.voxel,
                num_threads=policy.num_threads,
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
                estimation_method=reg_t.TransformationEstimationPointToPlane(_tukey(policy)),
            )
            return RegistrationResult._from_tensor(mode, reg, src_t)
        case RegistrationMode.COLORED_ICP:
            colored = reg_t.TransformationEstimationForColoredICP(policy.colored_lambda_geometric, _tukey(policy))
            src_t = source.tensor()
            reg = reg_t.icp(src_t, target.tensor(), policy.max_correspondence, estimation_method=colored)
            return RegistrationResult._from_tensor(mode, reg, src_t)
        case RegistrationMode.GLOBAL:
            return _bootstrap(source, target, _engine(), policy)
        case RegistrationMode.MULTIWAY:
            return _multiway(session, policy, tap)
        case unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanRegistration(Struct, frozen=True):
    lane: LanePolicy
    policy: RegistrationPolicy = RegistrationPolicy()

    async def register(self, session: RegistrationSession, mode: RegistrationMode) -> "RuntimeRail[RegistrationResult]":
        async def dispatch() -> "RuntimeRail[RegistrationResult]":
            # PEP-646 arity is static evidence only, so the two-cloud minimum re-proves HERE at runtime — INSIDE the
            # evidence span, so a short session lands on the SCAN_REGISTRATION receipt as this typed refusal
            # instead of an unwitnessed early return or a worker-side IndexError.
            if len(session) < 2:
                return Error(BoundaryFault(config=(f"scan.registration.{mode}", f"session-arity:{len(session)}<2")))
            # HOSTILE is the declared trait because the compiled registration band imports under no isolated subinterpreter;
            # the trailing tap is the lane conduit's pickled proxy the kernel's pulse beats write through.
            return await self.lane.offload(Kernel.of(_register_kernel, KernelTrait.HOSTILE), session, mode, self.policy, self.lane.pulses.tap)

        return await evidence_run(EvidenceScope.SCAN_REGISTRATION, f"register.{mode}", dispatch)

    def bench(self, session: RegistrationSession, mode: RegistrationMode, *, rounds: int = 32, warmup: int = 4) -> BenchmarkReceipt:
        # cloud-size-parameterized macro-bench: the subject keys the exact mode row and the source point count, so
        # a latency row compares like-for-like across scan densities; each round drives the whole register crossing
        # — arity re-proof, offload, solver, weave — never an in-kernel probe (the pulse boundary).
        return bench_seam(f"{EvidenceScope.SCAN_REGISTRATION.value}.{mode}.p{len(session[0])}", partial(self.register, session, mode), rounds=rounds, warmup=warmup)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
