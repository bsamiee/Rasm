# [RASM_TASKLOG]

`Rasm` open and closed work distilled from `IDEAS.md` and design-page RESEARCH residuals. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[BOOLEAN_RECEIPT_PROVENANCE]-[QUEUED]: The arrangement boolean receipt carries the source-attribution channel the native ABI already hands over.
- Capability: every boolean product attributes its faces to their originating operand through the run/original-id/face-id provenance family, so a downstream consumer joins products back to sources without geometric matching.
- Shape: provenance columns on `Rasm/.planning/Meshing/arrangement.md` `BooleanReceipt`, filled from the `api-manifold.md` `[RUN_PROVENANCE]` members (landed this pass), sized through the `merge_length` read.
- Unlocks: source-keyed material and semantic carry-through across booleans — the only attribution channel the boundary offers, currently dropped at the receipt.
- Anchors: `libs/csharp/Rasm/.api/api-manifold.md` `[RUN_PROVENANCE]` block; `Meshing/arrangement.md` the receipt owner and its `meshgl64` extraction fold.

[ARRANGEMENT_CANCEL_THREAD]-[QUEUED]: Governance for the arrangement fold and the native scale lane.
- Capability: cancellation and progress govern the whole arrangement fold under the synchronous-rail law, and the native tier answers that same governance with its abandonment lowering onto the typed fault rail beside its asset-absence sibling.
- Shape: an execution-governance band on `libs/csharp/Rasm/.planning/Meshing/arrangement.md` extending `ArrangementPolicy` across subdivision, classification, and weld; the tier-3 lane binds `manifold_execution_context`, `manifold_execution_context_cancel`, `manifold_execution_context_progress`, and `manifold_with_context`, lowering `MANIFOLD_CANCELLED` beside `NativeAssetMissing`.
- Unlocks: `[NATIVE_LANE_GOVERNANCE]` — governable million-face booleans.
- Anchors: `libs/csharp/Rasm/.api/api-manifold.md` context rows 09-13, `Meshing/arrangement.md` `ScaleCeiling` gate and fault taxonomy.

[VORONOI_COMPLEX_BAND]-[QUEUED]: Voronoi cell decomposition over cluster clouds.
- Capability: cells, adjacency edges, and per-cell measures computed from the admitted dual constructors over `VectorCloud` clusters, with a typed receipt (cell/edge counts, unbounded-cell handling, tolerance evidence).
- Shape: a Voronoi-complex band on `libs/csharp/Rasm/.planning/Spatial/cloud.md` beside the hull rail's Delaunay fold.
- Unlocks: `[CLOUD_VORONOI_FIELD]` — density and territory evidence per point.
- Anchors: `libs/csharp/Rasm/.api/api-miconvexhull.md` `VoronoiMesh.Create` overload family with `PlaneDistanceTolerance`.

[SIBSON_FIELD_ROW]-[QUEUED]: Natural-neighbor fitted-field row.
- Capability: Sibson natural-neighbor interpolation fitted from scattered samples via the Voronoi complex, minted as a reconstruction payload and carried as one fitted `ScalarField` case with its receipt.
- Shape: a fitting row on `libs/csharp/Rasm/.planning/Meshing/reconstruct.md`; one fitted case on `libs/csharp/Rasm/.planning/Spatial/fields.md` per the fitted-payload law.
- Unlocks: `[CLOUD_VORONOI_FIELD]` — exact-support scattered interpolation beside the RBF/MLS family.
- Anchors: `Spatial/fields.md` reconstruction case family, `Meshing/reconstruct.md` policy-dispatched entry.

[FLOW_MORSE_DECOMPOSITION]-[QUEUED]: Morse graph and separatrix band on the flow owner.
- Capability: facet-transition digraph over the traced field, recurrent sets as strongly connected components, condensation into a Morse graph, fixed-point classification by local linearization, and separatrices traced from saddle eigendirections through the settled dense-output event localization.
- Shape: a topology band on `libs/csharp/Rasm/.planning/Processing/flow.md` composing `StronglyConnectedComponents` and `CondensateStronglyConnected`.
- Unlocks: `[FLOW_TOPOLOGY_ATLAS]` — qualitative field skeletons for panelize and patternmap guidance.
- Anchors: `libs/csharp/.api/api-quikgraph.md` SCC/condensation rows, `Processing/flow.md` tracer.

[ARRANGEMENT_PROGRESS_TAP]-[QUEUED]: Overlay execution governance surfaces through the operation runtime.
- Capability: a long-running exact overlay reports progress and honors cancellation through the same `Env` capsule every other kernel operation charges, so an interactive consumer reads one governance surface.
- Shape: an `IProgress<double>`/`CancellationToken` passthrough from the `Analysis/query.md` `Env` cost capsule onto the `Meshing/arrangement.md` `ArrangementPolicy` `Cancel`/`Progress` columns.
- Unlocks: viewport and Fabrication overlay consumers gain live progress without a second governance channel.
- Anchors: `Meshing/arrangement.md` execution-governance band; `Analysis/query.md` `[03]-[OPERATION_RUNTIME]` `OpCost` capsule.
- Atomic: one passthrough wiring on two settled pages.

[QUANTITY_MEASURE_BAND]-[QUEUED]: Quantity-typed projections on the metrology owner.
- Capability: mass-property, bounds, and conformance results project as unit-carrying quantities derived from the executing `Context`'s `ModelUnit` — length/area/volume/mass identities, comparison and rescale unit-safe by construction.
- Shape: a quantity-projection band on `libs/csharp/Rasm/.planning/Analysis/measure.md` over the context bridge.
- Unlocks: `[UNIT_CARRIED_MEASURES]` — typed takeoff and display quantities at the app strata.
- Anchors: `Analysis/measure.md` metrology owner, `Domain/context.md` bridge band.

[DUAL_JACOBIAN_FLOOR]-[QUEUED]: Dual-number forward mode and the auto-Jacobian adapter.
- Capability: a dual scalar with the generic-math operator surface residual kernels need, and an `ILmModel` adapter deriving exact Jacobians from residual code — hand-coded Jacobian drift retired where models opt in.
- Shape: the forward-mode floor on new page `libs/csharp/Rasm/.planning/Solving/sensitivity.md` (`Solving/Sensitivity.cs`); an auto-Jacobian admission row on `libs/csharp/Rasm/.planning/Solving/solver.md`.
- Unlocks: `[SENSITIVITY_RAIL]` — gradient-native solving.
- Anchors: `Solving/solver.md` `ILmModel` residual+Jacobian floor, `Numerics/matrix.md` solve family.

[ADJOINT_SENSITIVITY]-[QUEUED]: Adjoint sensitivity maps with typed receipts.
- Capability: adjoint accumulation over the converged system returning d(solution)/d(parameter) maps, island-aware, with a sensitivity receipt (parameter set, conditioning evidence, residual norms).
- Shape: the adjoint band on `libs/csharp/Rasm/.planning/Solving/sensitivity.md` over the forward-mode floor and the island decomposition.
- Unlocks: `[SENSITIVITY_RAIL]` — parametric design optimization loops.
- Anchors: `Solving/solver.md` island fold, `Numerics/matrix.md` factorization reuse.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BLEND_PATH_AXIS_SPLIT]-[COMPLETE]: `Numerics/atoms.md` `[02]-[SCALAR_FLOOR]` carries `BlendPath` as a two-case `[Union]` — a rectangular row takes no traversal and a polar row carries `HueSpan` in its own payload, so the illegal pairing is unspellable — over eight space rows each naming the `RgbProfile` row whose `DynamicRange` its interpolation runs under (`Jzazbz`/`Jzczhz`/`Ictcp` at the PQ reference white); `PerceptualColor.Mix`/`Ramp` dispatch through one seam, and `Theme/tokens.md` with `Appearance/texture.md` moved to the new shape in the same pass.
[ENV_TELEMETRY_FIELD]-[COMPLETE]: `Env.EnvAsks`/`Asks`/`Taps` ride settled fences at `Analysis/query.md` `[03]-[OPERATION_RUNTIME]`, each composing the catalogued `Eff.runtime<Env>()`; the card's `[03]-[OPERATION_RUNTIME_RESEARCH]` section name never existed on the page.
[OP_COST_CAPSULE]-[COMPLETE]: `Operation.Apply` at `Analysis/query.md` `[03]-[OPERATION_RUNTIME]` opens `CostMark.Start()` ahead of the fold and charges the `OpCost` capsule through the `Env` tap on both exits, the fail exit publishing the fault beside the cost.
[BENCH_CLAIM_ROWS]-[COMPLETE]: landed as `Domain/telemetry.md` `[08]-[BENCH_LEDGER]` with the four registered rows — `Simplify.HausdorffClaim`, `Parametric.FrameDefectClaim`, `Surfaces.CurvatureSummaryClaim`, `Flatten.DistortionClaim`.
[CAPSULE_EXTENSION_MINTS]-[COMPLETE]: landed as `Domain/telemetry.md` `[03]-[CAUSAL_FRAME]`, `[04]-[INSTRUMENT_MECHANISM]`, and `[06]-[SLO_ALGEBRA]` — causal frame with `TelemetrySource`, `InstrumentSpec` absorbing `InstrumentRow`, and the burn/objective/severity/panel algebra; `NodaTime` joined the kernel manifest touch-point set for the envelope instants and objective windows.
[EVIDENCE_SERIALIZATION]-[COMPLETE]: `Drawing/pack.md` `[03]-[SCHEMA_AND_EVIDENCE]` seats `PackWireContext` as the kernel evidence resolver and `EvidenceWire.Json` as one options identity sealed by `JsonSerializerOptions.MakeReadOnly()` at type init, with `DDoubleJsonConverter` registered options-level so the exact hi/lo codec outranks any generated `ddouble` contract.
[PACK_SCHEMA_IDENTITY]-[COMPLETE]: `Drawing/pack.md` `[03]-[SCHEMA_AND_EVIDENCE]` derives the `ContentHash` id from kind and field rows, validates id recomputation, stride/null rows, and the declaration roster, then gates `Describes` on valid schema and geometry carriers.
[SIGNAL_TAP_OWNER]-[COMPLETE]: `SpanBand` settled at `Domain/telemetry.md` `[05]-[SIGNAL_TAP]` — one `ActivitySource` per `KernelDomain` row, `Traced` the rail-valued bracket — with the activity family catalogued at `libs/csharp/.api/api-diagnostics-activity.md`.
[MODELUNIT_UNITSNET_BRIDGE]-[COMPLETE]: `[UNIT_BRIDGE]` band on `Domain/context.md` `ModelUnit` — `Convert` over guarded `UnitConverter.TryConvert` and the cached `Converter<TQuantity>` hot-path row, `ScaleTo` kept the one scale owner.
[QUANTILE_SEAM_AWARENESS]-[COMPLETE]: three-formed scope clause on the `Domain/stats.md` Boundary line naming `Rasm.Compute` `StreamMonitor.Quantile` as operational owner beside the AppUi exact nearest-rank sibling.
[FIT_DRAWS_DETERMINISTIC]-[COMPLETE]: `Solving/fit.md` mints ONE `Deterministic.Stream` at `Apply`; candidate shuffle and minimal-set draws are `NextBelow` reads of that threaded state; `System.Random` residue zero.
[FLAT_INDEX_DELEGATION]-[COMPLETE]: `SymmetricMatrix.FlatIndex` promoted `internal`; `Domain/stats.md` `SampleMoment` indexer and `Solving/solver.md` `Lm.PackedIndex` (checked `long` widening kept at the call boundary) both delegate.
[NEWELL_FOLD_COLLAPSE]-[COMPLETE]: `Numerics/atoms.md` `VectorFrame.NewellNormal` is the one inexact polygon-normal fold; `Spatial/neighbors.md` and `Parametric/panelize.md` compose it; `Numerics/predicates.md` keeps the exact-carrier bar.
[MOMENT_HANDOFF_PROSE]-[COMPLETE]: `Domain/stats.md` `[MOMENT_OWNERSHIP]` seam edge states the packed-triangle no-repack `SymmetricMatrix.Of` handoff.
[PROJECT_SEQ_COLLAPSE]-[COMPLETE]: `Spatial/cloud.md` `[03]` sequence arms route through `AtomProjection.Values`; the private `ProjectSeq` helper is deleted.
[GEOMETRY_MEASURES_OWNER]-[COMPLETE]: `Analysis/measure.md` seats the `GeometryMeasures` cluster at the full Option-field `MassProperty` concept; `Rasm.Bim` `Semantics/properties.md` and `Planning/cost.md` bind the kernel owner through `QuantityDerivation.Derive`.
[HATCH_SYNTHESIS]-[COMPLETE]: landed as `Drawing/hatch.md` — `Hatching.Apply` folds `HatchOp` (`Regions`/`Projection`) through overlay-normalized region loops, exact crossing-parity courses, and the `Patterning` motif orbit into the successor-linked SoA `HatchResult` wire; `GeometryFault.HatchFault` 2437; `Rasm.Fabrication` `Documentation/projection.md` carries the per-view hatch rows.
