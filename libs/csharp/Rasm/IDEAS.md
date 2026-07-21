# [RASM_IDEAS]

Forward pool of higher-order kernel concepts grounded in the robust-geometry domain and the monorepo geometry-flow. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[NATIVE_LANE_GOVERNANCE]-[QUEUED]: Cancellable, progress-reporting arrangement fold — the tier-3 `manifoldc` lane binds execution contexts and the managed lane honors the runtime token end to end.
- Capability: million-face booleans become governable — the arrangement fold threads `CancellationToken` and progress per the synchronous-rail law, and the native scale lane binds `manifold_execution_context`, `manifold_execution_context_cancel`, `manifold_execution_context_progress`, and `manifold_with_context`, mapping `MANIFOLD_CANCELLED` onto the typed fault rail.
- Shape: an execution-governance band on `libs/csharp/Rasm/.planning/Meshing/arrangement.md` extending `ArrangementPolicy` and the tier-3 gate.
- Unlocks: interactive hosts cancel a runaway boolean typed instead of abandoning a thread; long solves report progress through the same `IProgress<double>` the analysis runtime already carries.
- Anchors: `libs/csharp/Rasm/.api/api-manifold.md` execution-context rows, `Meshing/arrangement.md` `ScaleCeiling` gate, `Domain/rails.md` explicit-`CancellationToken` law, `Analysis/query.md` `Env` progress capsule.

[CLOUD_VORONOI_FIELD]-[QUEUED]: Spatial Voronoi complex and natural-neighbor interpolation — cluster clouds gain cell decomposition and Sibson scattered-data fields.
- Capability: 3D Voronoi cell decomposition over `VectorCloud` clusters (cells, adjacency, cell measures) through the admitted dual constructors, and a natural-neighbor (Sibson) fitted-field row turning scattered samples into a `ScalarField` with typed receipts — the interpolation family the RBF/MLS rows do not cover.
- Shape: a Voronoi-complex band on `libs/csharp/Rasm/.planning/Spatial/cloud.md`; a natural-neighbor fitting row on `libs/csharp/Rasm/.planning/Meshing/reconstruct.md` minting the fitted payload; one fitted case on `libs/csharp/Rasm/.planning/Spatial/fields.md`.
- Unlocks: density and territory evidence per cluster point, exact-support scattered interpolation for survey and scan data, and a cell substrate for downstream fracture and packing work.
- Anchors: `libs/csharp/Rasm/.api/api-miconvexhull.md` `VoronoiMesh.Create`/`Triangulation.CreateVoronoi` overload family, `Spatial/cloud.md` hull rail already composing the Delaunay fold, `fields.md` fitted-payload law (reconstruct mints, fields carries).

[FLOW_TOPOLOGY_ATLAS]-[QUEUED]: Vector-field topology — Morse decomposition, recurrent sets, and separatrices over the settled dense-output tracer.
- Capability: the flow owner gains a topology band — fixed-point and periodic-orbit detection, recurrent-set extraction as strongly connected components of the facet-transition digraph, condensation into a Morse graph, and separatrix tracing seeded from saddle eigendirections through the existing event-localized tracer.
- Shape: a topology band on `libs/csharp/Rasm/.planning/Processing/flow.md` composing the graph substrate (`StronglyConnectedComponents`, `CondensateStronglyConnected`) and the dense-output integrator.
- Unlocks: qualitative field understanding for panelization and pattern guidance — direction fields ship with their singularity/separatrix skeleton, not just traced lines.
- Anchors: `libs/csharp/.api/api-quikgraph.md` SCC and condensation rows, `Processing/flow.md` dense-output event localization, `Processing/segment.md` cross-field singularity vocabulary.

[UNIT_CARRIED_MEASURES]-[QUEUED]: Quantity-typed metrology — measures leave the kernel carrying unit identity derived from the model context, never bare doubles.
- Capability: `ModelUnit` bridges onto the admitted units substrate so every mass-property, bounds, and conformance result projects as a quantity with unit identity (`length`/`area`/`volume`/`mass`), cross-context rescale rides `UnitConverter.Convert`, and consumers render or compare without hand-scaling.
- Shape: a unit-bridge band on `libs/csharp/Rasm/.planning/Domain/context.md` (`ModelUnit` to `LengthUnit` projection, conversion row); a quantity-projection band on `libs/csharp/Rasm/.planning/Analysis/measure.md`.
- Unlocks: unit-safe display at the app strata, cost and takeoff pipelines consuming typed quantities, and cross-document comparisons that cannot silently mix unit regimes.
- Anchors: `libs/csharp/.api/api-unitsnet.md` `UnitConverter` conversion family, `Domain/context.md` `ModelUnit` meters-per-unit evidence and `ScaleTo`, `Analysis/measure.md` metrology owner.

[SENSITIVITY_RAIL]-[QUEUED]: Solving differentiation rail — dual-number forward mode and adjoint sensitivities make the solver tier gradient-native.
- Capability: a dual-number scalar with the operator surface the residual kernels need, an `ILmModel` adapter deriving exact Jacobians from residual code (retiring hand-coded Jacobian drift), and adjoint accumulation returning d(solution)/d(parameter) sensitivity maps with typed receipts.
- Shape: one new page `libs/csharp/Rasm/.planning/Solving/sensitivity.md` (`Solving/Sensitivity.cs`) beside the λ-ladder functor; an auto-Jacobian admission row on `libs/csharp/Rasm/.planning/Solving/solver.md`.
- Unlocks: gradient-based parametric design optimization, constraint-solver conditioning evidence, and sensitivity-aware fitting across the fit and register owners.
- Anchors: `Solving/solver.md` `ILmModel` residual+Jacobian floor and island decomposition, `Numerics/matrix.md` solve family, generic-math operator patterns already ruling the numeric floor.

[DRAWING_HATCH_PLANE]-[QUEUED]: Hatch synthesis over the exact region complex — drawing regions gain pattern fills at kernel exactness.
- Capability: hatch-pattern families (parallel, crosshatch, staggered rows with angle/spacing/origin policy) generated as line families and clipped exactly to the region cells `DrawingProjection.Fill` mints, per-region pattern policy, SoA polyline wire for sheet consumers.
- Shape: one new page `libs/csharp/Rasm/.planning/Drawing/hatch.md` (`Drawing/Hatch.cs`) composing the arrangement planar overlay and the intersect crossing lattice.
- Unlocks: fabrication documentation and drafting sheets receive filled drawings from the kernel wire — no host hatch round-trip, no approximate clipping.
- Anchors: `Drawing/view.md` region fill routing `ArrangementOp.PlanarOverlay`, `Meshing/intersect.md` segment crossings, `Parametric/patternmap.md` wallpaper-group vocabulary for pattern symmetry rows.

[TRIANGULAR_ADDRESS_OWNER]-[QUEUED]: One triangular-addressing mint — every packed-upper producer delegates to the promoted floor owner.
- Capability: packed-upper index arithmetic becomes single-owner; a layout edit lands in one member and every spectral and solver consumer follows by construction instead of by three hand-kept formula copies.
- Shape: a visibility-and-delegation band across `libs/csharp/Rasm/.planning/Numerics/matrix.md` (`FlatIndex` promoted `internal`), `libs/csharp/Rasm/.planning/Domain/stats.md` (`SampleMoment` indexer delegates), and `libs/csharp/Rasm/.planning/Solving/solver.md` (`Lm.PackedIndex` delegates, checked `long` widening kept at its call boundary).
- Unlocks: the packed-upper layout contract enforces itself; `Processing/register.md` normal-equation scatter and `Analysis/select.md` spectral reads inherit the one mint.
- Anchors: the folder `RULINGS.md` packed-upper rows; the one-assembly internal-reach law; `Solving/solver.md`'s own mirror-law boundary naming the drift defect.

[POLYGON_NORMAL_OWNER]-[QUEUED]: One inexact polygon-normal owner — the Newell area-vector fold homes on the numerics floor for every ring, panel, and frame consumer.
- Capability: orientation-true polygon normals become one composed floor capability; the exact axis carrier and the inexact vector stay two owners by decision.
- Shape: one floor member on `libs/csharp/Rasm/.planning/Numerics/atoms.md`; composition edits on `libs/csharp/Rasm/.planning/Spatial/neighbors.md` (local `NewellNormal` deleted) and `libs/csharp/Rasm/.planning/Parametric/panelize.md`; a derives-from note on `libs/csharp/Rasm/.planning/Numerics/predicates.md`.
- Unlocks: ring-case Bishop seeding, panel plane fits, and the remesh flip gate read one fold; `Spatial/cloud.md`'s fitted-plane ring normal keeps its winding guard as the deliberate divergent method.
- Anchors: the folder `RULINGS.md` Newell-owner row; the cured `DominantAxis` five-page collapse as precedent; `predicates.md`'s exact-carrier boundary.

[CAUSAL_FRAME_CAPSULE]-[QUEUED]: Cross-stratum causal-frame primitives join the kernel signal capsule — every stratum emits receipts through kernel-owned identity, tenancy, and envelope types.
- Capability: receipt emission stops splitting into two paradigms — the neutral-`Guid`/`string` twins and the L3 value objects collapse onto one kernel-owned causal frame every package names legally.
- Shape: `CorrelationId`, `TenantId`/`TenantContext`, `ReceiptEnvelope`, and the `ReceiptSinkPort` record (its emit delegate app-root-bound) mint on `libs/csharp/Rasm/.planning/Domain/telemetry.md` beside `TelemetryContributorPort`.
- Unlocks: `Rasm.Fabrication` and every app-platform peer name the receipt seam without a strata inversion; the OTel, HLC, and baggage lacing stays at `Rasm.AppHost` `SignalGovernance` untouched.
- Anchors: `libs/csharp/.planning/RULINGS.md` causal-frame homing row; the settled `TelemetryContributorPort` kernel move as the proven vehicle; `Domain/telemetry.md` capsule law.
- Ripple: `[CAPSULE_EXTENSION_MINTS]` decomposes this with `[INSTRUMENT_SPEC_CAPSULE]` and `[BURN_RATE_CAPSULE]`.

[INSTRUMENT_SPEC_CAPSULE]-[QUEUED]: Instrument-shape specs and their kind roster become one kernel capsule member — per-folder bind factories retire.
- Capability: the counter, level, advised-histogram, and keyed-levels bind bodies mint once; every sink descriptor composes the kernel spec instead of re-typing its own spec record and kind vocabulary.
- Shape: an `InstrumentSpec` record and kind roster with per-kind bind delegates on `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[03]`, beside `InstrumentRow` and `Buckets`.
- Unlocks: `Rasm.AppUi` and `Rasm.Compute` delete their twin spec records; `Rasm.AppHost` instrument rows adopt the factories and drop inline lambdas.
- Anchors: `libs/csharp/.planning/RULINGS.md` instrument-spec collapse row; kernel `InstrumentRow`/`Buckets`/`LevelCells` already homed.
- Ripple: `[CAPSULE_EXTENSION_MINTS]`.

[BURN_RATE_CAPSULE]-[QUEUED]: Multiwindow burn-rate SLO algebra joins the kernel capsule — one carrier for windows, objective, and the burn fold.
- Capability: the canonical fast/slow burn windows and the burn computation exist once; a factor or objective change lands at one owner and every alerting sink follows.
- Shape: a burn-window/objective carrier and fold on `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[03]` beside the cost and bench capsules.
- Unlocks: `Rasm.AppUi` viewport tiles, `Rasm.Compute` IaC rule rows, and `Rasm.AppHost` health rules compose one algebra; the hand-typed window constants delete.
- Anchors: `libs/csharp/.planning/RULINGS.md` burn-rate carrier row; the SRE multiwindow discipline both sinks already encode.
- Ripple: `[CAPSULE_EXTENSION_MINTS]`.

[KERNEL_SIGNAL_FABRIC]-[BLOCKED]: Analysis-runtime charge completes the kernel signal fabric — runtime capability reads join the settled spans, metrics, hooks, cost evidence, and bench ledger.
- Capability: the analysis-runtime capability read over `Eff.runtime<RT>()` — the last research remnant of the signal fabric restored to settled fences, so analysis effects read their capability from the runtime record.
- Shape: settled `[03]` fences on `libs/csharp/Rasm/.planning/Analysis/query.md` (runtime charge).
- Unlocks: analysis pipelines read runtime capability without service location — the last unsettled band of the observability theme closes.
- Anchors: `libs/csharp/.api/api-languageext.md` runtime rows; the settled span, metric, hook, cost, and bench bands on `Domain/telemetry.md`.
- Arms: `api-languageext.md` admits `public static Eff<RT, RT> runtime<RT>()`; until the row exists the charge stays research, never settled fences.

[COLUMNAR_WIRE_SCHEMA]-[BLOCKED]: Frozen JSON identity completes the pack wire — `EvidenceWire.Json` becomes one static sealed options identity beside the settled schema and exact binary blocks.
- Capability: one static read-only `JsonSerializerOptions` identity for `EvidenceWire.Json`, joining the validated `PackSchema` identity and the exact binary `EvidenceWire` blocks `[03]-[SCHEMA_AND_EVIDENCE]` already settles.
- Shape: one static identity row on `libs/csharp/Rasm/.planning/Drawing/pack.md` `[03]-[SCHEMA_AND_EVIDENCE]`.
- Unlocks: wire consumers verify and dedupe against a sealed contract identity — no per-call options graph, no cold metadata cache, no post-seal mutation path.
- Anchors: `libs/csharp/.api/api-system-text-json.md` options rows; `Drawing/pack.md` schema-and-evidence law; the contract-identity precedent that resolver, converter, and options instances are stable identities.
- Arms: `api-system-text-json.md` admits `public void JsonSerializerOptions.MakeReadOnly()`; until that row exists the frozen JSON identity stays research.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BENCH_CLAIM_LEDGER]-[COMPLETE]: landed as `Domain/telemetry.md` `[05]` (`BenchClaim`/`BenchLedger`) with claim rows at `Processing/decimate.md` `Simplify.HausdorffClaim`, `Parametric/curve.md` `Parametric.FrameDefectClaim`, `Parametric/surface.md` `Surfaces.CurvatureSummaryClaim`, and `Processing/flatten.md` `Flatten.DistortionClaim`.
