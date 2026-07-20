# [RASM_IDEAS]

Forward pool of higher-order kernel concepts grounded in the robust-geometry domain and the monorepo geometry-flow. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

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

[KERNEL_SIGNAL_FABRIC]-[ACTIVE]: Kernel observability substrate — one `Domain` signal owner turning the typed-receipt corpus into hook facts, metrics, spans, and op-cost evidence with zero OpenTelemetry reference.
- Capability: receipt-as-truth telemetry-as-tap for the whole kernel — a typed hook registry (`rasm.rasm.<domain>.<point>` points, veto/observe/replay modalities, subscriber faults isolated onto the `Fin` rail), a receipt-to-instrument projection minting `rasm.kernel.<measure>` UCUM instruments through `IMeterFactory.Create(MeterOptions)`, an `ActivitySource` span band per sub-domain namespace, a band-2400 `GeometryFault` counter partitioned by fault case, and a uniform op-cost capsule (elapsed, allocations, iterations) captured at the operation runtime — the kernel arm the branch `InstrumentFan` merges.
- Shape: one new page `libs/csharp/Rasm/.planning/Domain/telemetry.md` (`Domain/Telemetry.cs`) owning registry, instruments, and cost capsule; `libs/csharp/Rasm/.planning/Domain/rails.md` threading-law extension for tap points on the `Op` rail; `libs/csharp/Rasm/.planning/Analysis/query.md` `Env` gains the telemetry-sink field its growth law already sanctions.
- Unlocks: kernel-grain dashboards, span-profile correlation, and tenant cost attribution at the app strata without one emit-call scattered into domain code; every `SampleReceipt`/`BooleanReceipt`/`SolveReceipt` fact stream becomes a metric channel for free.
- Anchors: `System.Diagnostics.Metrics` BCL inbox surface (`libs/csharp/.api/api-diagnostics-metrics.md` — `IMeterFactory.Create(MeterOptions)`, tagged `Counter`/`Histogram` writes), `Analysis/query.md` growth law "a telemetry sink is one field on `Env`", `Domain/rails.md` `Op` threading law, the branch wire law (`rasm.<domain>.<measure>` UCUM, no pre-baked suffixes).
- Tension: app-neutrality is absolute — registry, meter factory, and sink enter through `Env` and composition, never a process-global static; two apps composing the kernel must never fight over one registry slot.

[BENCH_CLAIM_LEDGER]-[QUEUED]: Typed benchmark-claim registry — every speed-gated lane in the corpus becomes an enumerable `BenchClaim` row the corpus gate proves.
- Capability: each vectorized-versus-scalar speed claim (Hausdorff `TensorPrimitives.Max` reduction, parametric tensor reductions, future SIMD lanes) minted as a typed row — claim `Op` key, vectorized lane, reference lane, admission threshold — so benchmark coverage is a fold over the ledger, never a prose hunt.
- Shape: a `BenchClaim` vocabulary and ledger band on `libs/csharp/Rasm/.planning/Domain/telemetry.md`; claim rows threaded at `libs/csharp/Rasm/.planning/Processing/decimate.md`, `libs/csharp/Rasm/.planning/Parametric/curve.md`, and `libs/csharp/Rasm/.planning/Parametric/surface.md` gated lanes.
- Unlocks: the app-stratum BenchmarkDotNet corpus gate enumerates kernel claims mechanically; an unproven speed claim becomes a visible ledger defect.
- Anchors: `Processing/decimate.md` corpus-gate reference-row language, `Parametric/curve.md`/`surface.md` benchmark-gated tensor reductions, branch `BenchmarkReceipt` families.

[COLUMNAR_WIRE_SCHEMA]-[QUEUED]: Columnar schema identity and lossless evidence serialization on the one encoding owner — kernel wires become lake- and Flight-ready without the kernel touching a storage client.
- Capability: `EncodedGeometry` descriptors gain a schema-identity band — `ContentHash`-derived schema id, field/dtype/stride/null semantics — so upper strata map wires onto Arrow record batches and Parquet zero-copy; receipt evidence carrying 106-bit `ddouble` fields serializes losslessly through `DDoubleJsonConverter` and `DoubleDoubleIOExpand` binary round-trips.
- Shape: a schema-descriptor and serialization band on `libs/csharp/Rasm/.planning/Drawing/pack.md` extending the settled dtype-strided arena and `View<T>` tensor seam.
- Unlocks: geometry and receipt egress into the estate storage plane (Persistence Arrow/Parquet/Flight adapters) with one schema authority; support bundles carry exact numeric evidence, never rounded doubles.
- Anchors: `Drawing/pack.md` `[TENSOR_RESIDENCY_SEAM]` descriptors + `EncodingKind`, `Domain/identity.md` seed-zero `ContentHash`, `libs/csharp/Rasm/.api/api-doubledouble.md` converter and binary I/O rows.

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

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
