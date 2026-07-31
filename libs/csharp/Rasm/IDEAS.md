# [RASM_IDEAS]

Forward pool of higher-order kernel concepts grounded in the robust-geometry domain and the monorepo geometry-flow. `[1]-[OPEN]` carries live ideas; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[PERCEPTUAL_COLOUR_DEPTH]-[QUEUED]: Perceptual colour states itself against a DECLARED viewing condition, so appearance is measured rather than assumed.
- Capability: difference, contrast, and tonal reads resolve under the condition a surface presents in — adapting luminance, surround, and background — so a chromatic-adaptation-aware appearance model adjudicates two colours where a fixed opponent metric assumes an average observer at one white; the interpolation-space and reference-lightness axes are settled, and the viewing condition is the one that remains.
- Shape: `libs/csharp/Rasm/.planning/Numerics/atoms.md` `[02]-[SCALAR_FLOOR]` — a viewing-condition column beside the working-space roster binding the `Configuration` cam slot, which makes the appearance spaces statable as interpolation and difference targets off that column rather than unrostered for want of conditions.
- Unlocks: an accessibility gate and a chart legend read contrast and difference under the surround they render in, so a dark-surround overlay stops borrowing a bright-office verdict.
- Anchors: `libs/csharp/.api/api-unicolour.md` `CamConfiguration` rows and the `Configuration` cam slot; `Hct` pins its own conditions internally, the standing proof that the condition is separable from the space it parameterizes.
- Tension: the genuine bet — the appearance model under declared conditions is the correct answer and also the one whose inputs no consumer measures, so the column lands only where a consumer states its own conditions, and an unstated condition keeps the fixed metric rather than inheriting a default that fabricates a surround.

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

[UV_ISLAND_PACKING_OWNER]-[QUEUED]: One 2D irregular-packing algebra — UV-island layout and true-shape placement home at the kernel lattice both S2 peers reach.
- Capability: irregular 2D packing/placement becomes one kernel owner — no-fit-polygon construction, candidate placement scoring, and atlas layout over exact ring booleans — so the two S2 peers that each need it compose one algebra instead of one owning and one unreachable.
- Shape: a packing owner under `libs/csharp/Rasm/.planning/Meshing/` composing `ArrangementOp.PlanarOverlay` exact ring booleans, `OffsetOp.Minkowski` support-vertex convolution (the no-fit-polygon construction itself), and `Meshing/delaunay.md` `LowerHull`.
- Unlocks: `Processing/flatten.md` `ChartAtlas` gains a packing consumer at its own stratum, and `Rasm.Fabrication` `Nesting/nfp.md` true-shape placement re-seats as composition over the kernel algebra.
- Anchors: `docs/laws/scars.md` `[STRATA_TWIN]` seating law; `Meshing/arrangement.md` `PlanarOverlay`; `Meshing/offset.md` `Minkowski`; `Rasm.Fabrication/.planning/Nesting/nfp.md` `NoFitPolygon`.
- Ripple: `Rasm.Fabrication` `Nesting/nfp.md` re-seats as consumer; `Rasm.Materials` atlas packing composes the same owner.

[MATRIX_TRANSFORM_BAND]-[QUEUED]: FFT and separable convolution on the numeric floor — the admitted transform namespaces gain one kernel composition band.
- Capability: spectral and separable convolution becomes a floor capability composed from the admitted numerics substrate, so mip/filter folds, field morphology, and image-space kernels read one transform owner instead of hand-rolled loops or uncomposed package surface.
- Shape: a `MatrixKernel`-adjacent transform band on `libs/csharp/Rasm/.planning/Numerics/matrix.md` composing MathNet `IntegralTransforms` and `Interpolation`.
- Unlocks: `Rasm.Materials` mip and filter folds and `Rasm.Fabrication` field-morphology arms compose one kernel transform band; kernel image-space work gains its FFT floor.
- Anchors: `libs/csharp/.api/api-mathnet-numerics.md` `IntegralTransforms`/`Interpolation` rows; `Numerics/matrix.md` `MatrixKernel` funnel law.
- Ripple: `Rasm.Materials` `Raster/filter.md` and `Rasm.Fabrication` morphology consumers follow the band.

[OPTIONAL_KEY_LEVEL_FAMILY]-[QUEUED]: Pulled level families reach the absent-tenant arm — a keyed cell entry carries an optional key, so a possibly-unpartitioned dimension projects untagged rather than vanishing.
- Capability: absent-dimension law reaches the PULLED plane — a level family whose one dimension may be absent projects an untagged measurement, so partitioned and unpartitioned compositions report the same series and neither mints a sentinel key nor drops the measure.
- Shape: the keyed cell store, its write, and its keyed reader on `libs/csharp/Rasm/.planning/Domain/telemetry.md` `[04]-[INSTRUMENT_MECHANISM]`, with the `Levels` bind arm reading the optional key.
- Unlocks: every per-tenant census gauge in the estate reports under both tenancy modes; a contributor stops choosing between a sentinel dimension and a missing series.
- Anchors: the settled `TenantContext.Partitions`/`Tags` absent-tenant arm on `[03]-[CAUSAL_FRAME]`; the `Levels` kind beside its keyed reader on `[04]-[INSTRUMENT_MECHANISM]`; `libs/csharp/.api/api-diagnostics-metrics.md` `Measurement<T>` rows carrying the untagged construction.
- Tension: the keyed store's map key widens, so whether structural equality over a full tag set lands in the same edit or stays a second axis is the open bet.
- Ripple: mirrors `Rasm.Persistence` `[UNPARTITIONED_USAGE_SERIES]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[GEOMETRY_BATCH_PRODUCER]-[DROPPED]: wrong owner — S0 references no sibling, so the kernel reaches neither `LakeGeneration` nor `Apache.Arrow` without pushing a columnar dependency onto every package above it, and `Encode.Apply` yields ONE `EncodedGeometry` per op where a lake generation needs a corpus; landed as `Rasm.Compute` `[GEOMETRY_LAKE_EGRESS]` keying its generation on this page's `PackSchema.SchemaId`.
[KERNEL_SIGNAL_FABRIC]-[COMPLETE]: the arming row is `libs/csharp/.api/api-languageext.md` `[07]` `Eff.runtime<RT>() -> Eff<RT, RT>`, and the charge already rides settled fences — `Env.EnvAsks`/`Asks`/`Taps` and the two-exit `Operation.Apply` cost charge at `Analysis/query.md` `[03]-[OPERATION_RUNTIME]` — so the signal plane carries no research remnant.
[BENCH_CLAIM_LEDGER]-[COMPLETE]: landed as `Domain/telemetry.md` `[08]-[BENCH_LEDGER]` (`BenchClaim`/`BenchLedger`) with claim rows at `Processing/decimate.md` `Simplify.HausdorffClaim`, `Parametric/curve.md` `Parametric.FrameDefectClaim`, `Parametric/surface.md` `Surfaces.CurvatureSummaryClaim`, and `Processing/flatten.md` `Flatten.DistortionClaim`.
[COLUMNAR_WIRE_SCHEMA]-[COMPLETE]: landed as `Drawing/pack.md` `[03]-[SCHEMA_AND_EVIDENCE]` — `PackWireContext` supplies the resolver, `EvidenceWire.Json` seals through `JsonSerializerOptions.MakeReadOnly()` at type init with `DDoubleJsonConverter` registered options-level, and the context folds into `Rasm.AppHost/Runtime/ports#WIRE_LAW` `SuiteContracts.Wire` as one argument.
[CAUSAL_FRAME_CAPSULE]-[COMPLETE]: landed as `Domain/telemetry.md` `[03]-[CAUSAL_FRAME]` — `TelemetrySource`, `CorrelationId`, `TenantId`/`TenantContext`, `ReceiptEnvelope`, and `ReceiptSinkPort`; tenancy stamps the kernel `AsyncLocal` slot and the BCL `Activity` store, and the OTel baggage store registers as one composition `TenantMirror` row.
[INSTRUMENT_SPEC_CAPSULE]-[COMPLETE]: landed as `Domain/telemetry.md` `[04]-[INSTRUMENT_MECHANISM]` — `InstrumentSpec` absorbed `InstrumentRow` whole, `InstrumentKind` x `MeasureForm` derives every bind from one generic body, and `InstrumentSet.Write` folds the three write verbs onto one typed rail.
[BURN_RATE_CAPSULE]-[COMPLETE]: landed as `Domain/telemetry.md` `[06]-[SLO_ALGEBRA]` — `Sli`, `Objective`, the four-row `BurnRow` table, `AlertSeverity` over page and ticket, `AlertSpec`, and the eight-row `PanelKind`.
[TRIANGULAR_ADDRESS_OWNER]-[COMPLETE]: `SymmetricMatrix.FlatIndex` promoted `internal` is the one triangular-addressing mint; `Domain/stats.md` `SampleMoment` and `Solving/solver.md` `Lm.PackedIndex` delegate, the solver keeping its checked `long` widening at the call boundary.
[DRAWING_HATCH_PLANE]-[COMPLETE]: landed as `Drawing/hatch.md` — `HatchPattern` rows (`Parallel`/`Crosshatch`/`Staggered`/`Motif`) carry rhythm as row data, courses clip by exact winding parity over `DrawingProjection.Fill` loops, and the motif arm composes `Patterning.Apply`'s wallpaper vocabulary; realized by `TASKLOG` `[HATCH_SYNTHESIS]`.
[POLYGON_NORMAL_OWNER]-[COMPLETE]: `Numerics/atoms.md` `VectorFrame.NewellNormal` is the one inexact polygon-normal fold; `Spatial/neighbors.md` and `Parametric/panelize.md` compose it, `Numerics/predicates.md` keeps the exact-carrier bar, and `Spatial/cloud.md`'s fitted-plane ring normal stays the deliberate divergent method.
