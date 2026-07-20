# [PY_COMPUTE_IDEAS]

Forward pool of higher-order folder concepts grounded in the numeric-science domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[COMPUTE_HOOK_RAIL]-[QUEUED]: compute registers its typed hook vocabulary on the runtime registry and gains metrics through the built-in taps.
- Capability: `rasm.compute.<domain>.<point>` hook rows — solve dispatch enter/exit, graduation admit and reject, study design evaluation, jit compile — fired as closed msgspec payload facts under the telemetry-as-tap law, with `tap_metrics` projecting each payload's numeric measures onto `rasm.compute.<domain>.<measure>` UCUM rows and `tap_receipts` streaming the same fact, so metric, receipt, and log lines cannot disagree.
- Shape: one new page `libs/python/compute/.planning/graduation/observability.md` owning the point-row table, the payload `Struct` family, and the measure-mapping rows; producers fire through the hub binding, apps attach subscribers at composition, and a colliding registration refuses structurally, so two apps composed on this library never fight over points.
- Unlocks: dashboard-visible solve, graduation, and study metrics with zero emit-calls in domain code; veto points for admission policy; replay for late-attaching diagnostic subscribers; real capability deepening the two-page graduation folder.
- Anchors: runtime `observability/hooks#HOOKS` `HookPoint`/`Hooks.fire`/`tap_metrics`/`tap_receipts` — a new point is one registered row, zero runtime edits; the `evidence_run` binding on `graduation/handoff#EVIDENCE_WEAVE`; branch `msgspec` catalog `Struct` rows.

[SOLVE_RESOURCE_LEDGER]-[QUEUED]: every measured solve carries its true resource cost, and tenant attribution joins it at the backend.
- Capability: `ResourceUsage` fact band — user and system CPU seconds, peak RSS, wallclock — sampled around each measured kernel through one `psutil.Process.oneshot()` block and folded as facts on the evidence weave, so the backend joins solve cost to the `rasm.tenant` baggage the runtime promotes.
- Shape: one `ResourceUsage` owner and one sampling fold on `libs/python/compute/.planning/graduation/observability.md`; every module-level measured kernel contributes the band through the shared fold with zero per-solver code.
- Unlocks: cost attribution per solve, per study evaluation, and per tenant; capacity ruling over the worker fleet grounded in measured CPU and RSS rather than wallclock alone.
- Anchors: branch `psutil` catalog `Process.oneshot()`, `Process.cpu_times() -> pcputimes`, `Process.memory_info() -> pmem`; the `measured` weave on `graduation/handoff#EVIDENCE_WEAVE`; tenant-baggage promotion stays a runtime concern, never a compute import.
- Tension: sampling runs inside worker lanes reading the worker's own process handle; cross-process aggregation stays the runtime lanes owner's.

[SOLVE_PROFILE_PARITY]-[QUEUED]: engine-native profile evidence lands in the one receipt band, mirroring the estate query-profile parity law.
- Capability: profile band on `SolverReceipt` and `JitEvidence` harvesting what each engine already measures — numba `CPUDispatcher` `signatures`, `parallel_diagnostics`, and `inspect_types`/`inspect_asm` extents beside XLA lowered-IR statistics off the captured IR — so a slow solve explains itself from the receipt, never from an external profiler attach.
- Shape: one profile `Struct` beside `JitEvidence` on `libs/python/compute/.planning/numerics/jit.md` and one optional profile slot on `libs/python/compute/.planning/solvers/receipt.md`, folded by the measured kernels that already own the engine handles.
- Unlocks: per-route compile-and-execute profiles on dashboards beside the bench fabric; solver-route regression diagnosis without a profiler re-run.
- Anchors: folder `numba` catalog dispatcher inspect surface; `numerics/jit#JIT` `Capture` lowered-IR rows; the estate parity precedent folding engine profiles into one receipt profile band.
- Tension: continuous span-profile correlation stays the runtime composition root's; this band is engine-native evidence only.

[MESH_GENERATION_ROUTE]-[QUEUED]: compute generates its own simulation meshes instead of only reading them.
- Capability: gmsh-backed unstructured 1/2/3-D mesh generation with physical groups, size fields, and boundary input, landing as a generation arm on `MeshExchange` beside read and write — generated meshes flow straight into the `CTOR` element table, weak-form assembly, and the meshio round-trip.
- Shape: one `generate` route on `libs/python/compute/.planning/solvers/mesh.md` mapping gmsh physical groups onto the named-set round-trip and gmsh element types onto `ElementKind`.
- Unlocks: end-to-end FEM studies from boundary description to graduated solve evidence with no external meshing step; design loops that re-mesh per iteration.
- Anchors: `solvers/mesh#EXCHANGE` promote/recover round-trip; the `CTOR` table; candidate `gmsh` (pypi-verified, adjacent to `scikit-fem`) through the admission lane.
- Tension: geometry tessellation, registration, and topology stay the geometry branch's; FEM discretization of a simulation domain is compute's charter — the arm consumes boundary input as data and imports no geometry kernel.

[STUDY_DESIGN_FAMILIES]-[QUEUED]: classical experiment designs join the study spine as method rows.
- Capability: factorial, fractional-factorial, Box-Behnken, central-composite, and Plackett-Burman design generation as `StudyMethod` rows whose `indices` fold returns `{}` — sampling-only members on the existing axis admitting the multi-output objective shape.
- Shape: new method rows and their design folds on `libs/python/compute/.planning/experiments/study.md`, marginals resolved through the same per-axis parameterization the SALib channel reads.
- Unlocks: response-surface and screening studies over solver routes; `RunHistory` comparison across design families under provably equal content keys.
- Anchors: `experiments/study#STUDY` `StudyMethod` axis and sample-grid spine; candidate `pyDOE3` (pypi-verified, adjacent to `salib`) through the admission lane.

[CONVEX_BACKEND_FAMILY]-[QUEUED]: the conic backend axis completes and the power cone becomes a first-class constraint row.
- Capability: `PowCone3D` membership joins the cone-constraint family, and SCS, HiGHS, and ProxSuite land as selectable solve-backend rows beside the Clarabel arm — every backend recovering the same primal/dual pair the KKT certificate grades, so backend choice never weakens the proof.
- Shape: one cone row and the backend rows on `libs/python/compute/.planning/optimization/convex.md`, backend selection one policy value on the existing solve dispatch.
- Unlocks: geometric-mean and power-utility programs; LP-heavy conic programs on HiGHS; real-time QP re-solves on ProxSuite under DPP warm starts.
- Anchors: folder `cvxpy` catalog `PowCone3D`, multi-backend `solve`, and the `ARGS`/`DUAL_VALUE` recovery laws; `optimization/convex#CONVEX` dual-certificate fold; candidates `scs`, `highspy`, `proxsuite` (pypi-verified) through the admission lane.

[DIFFERENTIAL_STIFF_POLICY]-[QUEUED]: the stiff route gains its implicit floor and bounded step control.
- Capability: `ImplicitEuler` joins the solver table as the order-1 DIRK floor beneath the Kvaerno family, and `ClipStepSizeController` lands as a wrapping-controller policy row bounding any adaptive controller's steps to a range.
- Shape: one solver row and one controller policy row on `libs/python/compute/.planning/solvers/differential.md`, both resolved off the gated `dfx` carrier like every sibling row.
- Unlocks: robust integration of severely stiff problems where high-order SDIRK stages fail; step clamping across event-adjacent integration windows.
- Anchors: folder `diffrax` catalog `ImplicitEuler` (implicit DIRK) and `ClipStepSizeController` (wrapping controller); the optimistix/lineax implicit-step seam the Kvaerno rows already thread.

[INFERENCE_SCALE_SCORING]-[QUEUED]: posterior scoring stays tractable at large draw counts.
- Capability: subsampled PSIS-LOO — `loo_subsample` with `update_subsample` refinement — as a scoring row beside the full `loo` fold, selected by draw-count policy so a large posterior grades predictive fit without the full pointwise matrix.
- Shape: one scoring row and its policy threshold on `libs/python/compute/.planning/experiments/inference.md`; `ELPDData.kind` discriminates the subsampled result on read.
- Unlocks: convergence-plus-fit graduation on posteriors whose full LOO cost is prohibitive.
- Anchors: folder `arviz` catalog LOO family (`loo_subsample`/`update_subsample`, `ELPDData.kind`); `experiments/inference#BAYESIAN` residual table.

[STUDY_BENCH_PROJECTION]-[QUEUED]: study measurements join the runtime bench fabric.
- Capability: `Measured` wallclock and speedup project onto the runtime benchmark-receipt family — one bench contribution per measured design evaluation, subject-keyed per objective.
- Shape: one projection fold beside the `Measured` owner mapping wallclock onto the bench duration measure and the speedup ratio onto a bench fact, riding the runtime bench receipt and `domain="bench"` rows with zero runtime edits under the bench growth law.
- Unlocks: DOE evaluation cost lands on dashboards beside every other bench subject; regression tracking over study runs without receipt post-processing.
- Anchors: `experiments/study#STUDY` `MeasurementMode`/`Measured`; runtime `observability/profiles#BENCH` `BenchmarkReceipt`/`Bench.run`; the `rasm.bench.*` rows on runtime `observability/metrics#METRIC`.
- Tension: solver receipts hold no benchmark authority and graduation admits no Python-only benchmark conclusion — the projection is observability evidence only, never a handoff verdict; `RESULT` mode contributes nothing.

[EVIDENCE_TRACE_LINKS]-[BLOCKED]: `SpanContext` decode on the `GeometryHandoff` wire when geometry lands trace links.
- Capability: the graduation decode admits the optional serialized W3C `traceparent` string beside the `ContentKey` (absent = no link), decodes it to a `SpanContext`, and folds it as a `Link` on the consuming evidence span.
- Shape: one decode arm on the compute side of the `GeometryHandoff` wire and one `Link` fold at the `evidence_run` span open, on `libs/python/compute/.planning/graduation/handoff.md`.
- Unlocks: backend trace click-through from compute evidence spans to the upstream geometry producer trace.
- Anchors: the graduation `HandoffAxis` spine; `Link`/`Span.add_link` on the branch `opentelemetry-api` catalogue.
- Tension: blocker question — does `GeometryHandoff.wire()` widen to carry the serialized W3C `traceparent` string beside the `ContentKey`? Resolution route: the branch ruling `[HANDOFF_TRACE_WIRE]` on `libs/python/.planning/IDEAS.md` owns the wire representation; that frozen-wire change is geometry's to land, and this decode co-ships it.
- Ripple: `geometry` `[EVIDENCE_TRACE_LINKS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
