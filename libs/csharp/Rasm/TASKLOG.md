# [RASM_TASKLOG]

`Rasm` open and closed work distilled from `IDEAS.md` and design-page RESEARCH residuals. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[ARRANGEMENT_CANCEL_THREAD]-[QUEUED]: Governance for the arrangement fold and the native scale lane.
- Capability: `CancellationToken` and progress threaded through subdivision, classification, and weld per the synchronous-rail law; the tier-3 lane binds `manifold_execution_context`, `manifold_execution_context_cancel`, `manifold_execution_context_progress`, and `manifold_with_context`, with `MANIFOLD_CANCELLED` lowering onto the typed fault rail beside `NativeAssetMissing`.
- Shape: an execution-governance band on `libs/csharp/Rasm/.planning/Meshing/arrangement.md` extending `ArrangementPolicy`.
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

[MODELUNIT_UNITSNET_BRIDGE]-[QUEUED]: Unit-identity bridge on the context substrate.
- Capability: `ModelUnit` projects onto the admitted units vocabulary and dynamic conversion rides `UnitConverter.Convert(QuantityValue, Enum, Enum)` with the cached-delegate row for hot paths; cross-context rescale keeps `ScaleTo` as the one scale owner.
- Shape: a unit-bridge band on `libs/csharp/Rasm/.planning/Domain/context.md`.
- Unlocks: `[UNIT_CARRIED_MEASURES]` — quantity projections read one bridge.
- Anchors: `libs/csharp/.api/api-unitsnet.md` conversion rows 01-04, `Domain/context.md` `ModelUnit` meters-per-unit evidence.
- Atomic: one bridge band on the settled context page.

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

[HATCH_SYNTHESIS]-[QUEUED]: Hatch-pattern synthesis clipped to the exact region complex.
- Capability: pattern families (parallel, crosshatch, staggered; angle/spacing/origin policy rows) generated as line families and clipped exactly to `DrawingProjection.Fill` region cells through the intersect crossing lattice, per-region policy, SoA polyline wire output.
- Shape: new page `libs/csharp/Rasm/.planning/Drawing/hatch.md` (`Drawing/Hatch.cs`) composing arrangement overlay cells and segment crossings.
- Unlocks: `[DRAWING_HATCH_PLANE]` — filled sheet drawings from the kernel wire.
- Anchors: `Drawing/view.md` `Fill` routing `ArrangementOp.PlanarOverlay`, `Meshing/intersect.md` `SegmentSegment` crossings, `Parametric/patternmap.md` symmetry vocabulary.

[CAPSULE_EXTENSION_MINTS]-[QUEUED]: Mint the four capsule extensions on the kernel signal owner — causal frame, package self-identity, instrument spec, burn-rate algebra.
- Capability: the signal capsule carries every cross-stratum signal primitive the strata law homes at L1, so no emitting package types a twin.
- Shape: four member bands on `libs/csharp/Rasm/.planning/Domain/telemetry.md` — `CorrelationId`/`TenantId`/`TenantContext`/`ReceiptEnvelope`/`ReceiptSinkPort` beside `TelemetryContributorPort`; the minted `TelemetrySource` package-identity rows beside the causal-frame band, foreign-source rows staying AppHost; `InstrumentSpec` with its kind roster and bind delegates beside `InstrumentRow`; the burn-window/objective carrier and burn fold beside `OpCost`.
- Unlocks: IDEAS.md `[CAUSAL_FRAME_CAPSULE]`, `[INSTRUMENT_SPEC_CAPSULE]`, and `[BURN_RATE_CAPSULE]` — the folder compose tasks arm the moment these members land.
- Anchors: `Domain/telemetry.md` capsule dependency law (BCL, LanguageExt, Thinktecture only); `libs/csharp/.planning/RULINGS.md` homing and collapse rows.
- Ripple: precedes `Rasm.AppHost` `[CAPSULE_PORT_RETARGET]`, `Rasm.AppUi` `[SIGNAL_CAPSULE_COMPOSE]`, `Rasm.Compute` `[SIGNAL_CAPSULE_COMPOSE]`, `Rasm.Persistence` `[RECEIPT_PORT_KERNEL_TYPES]`, `Rasm.Fabrication` `[RECEIPT_PORT_KERNEL_TYPES]`.

[QUANTILE_SEAM_AWARENESS]-[QUEUED]: Kernel quantile prose scopes its anticipated sketch to geometry samples.
- Capability: the stats page's anticipated P² policy row names its geometry-sample scope and the realized operational sibling, so a sweep never reads the two as one owner.
- Shape: one clause on `libs/csharp/Rasm/.planning/Domain/stats.md` scoping the `Distribution.Of` sketch row and citing the Compute `StreamMonitor.Quantile` operational owner.
- Unlocks: the branch three-formed quantile refusal holds with zero mutually-unaware prose.
- Anchors: `libs/csharp/.planning/RULINGS.md` streaming-quantile row; `Rasm.Compute` `Stats/monitor.md` P² lane.
- Ripple: mirrors `Rasm.Compute` `[QUANTILE_SEAM_AWARENESS]`.
- Atomic: one clause.

[FLAT_INDEX_DELEGATION]-[QUEUED]: Collapse the three packed-upper formula copies onto the promoted `FlatIndex` owner.
- Capability: one member mints triangular addressing; producers delegate, and layout drift becomes unrepresentable.
- Shape: `libs/csharp/Rasm/.planning/Numerics/matrix.md` `FlatIndex` `private` to `internal`; `libs/csharp/Rasm/.planning/Domain/stats.md` indexer delegates; `libs/csharp/Rasm/.planning/Solving/solver.md` `Lm.PackedIndex` delegates with its checked `long` widening kept at the call boundary.
- Unlocks: IDEAS.md `[TRIANGULAR_ADDRESS_OWNER]` — drift-proof addressing across spectral and solver consumers.
- Anchors: `RULINGS.md` packed-upper rows; `solver.md` mirror-law boundary.
- Atomic: three page edits, zero behavior change.

[NEWELL_FOLD_COLLAPSE]-[QUEUED]: Land the floor Newell owner and compose it at every ring and panel site.
- Capability: one orientation-true fold serves every inexact polygon-normal read.
- Shape: one member on `libs/csharp/Rasm/.planning/Numerics/atoms.md`; `libs/csharp/Rasm/.planning/Spatial/neighbors.md` deletes its local `NewellNormal` and composes; `libs/csharp/Rasm/.planning/Parametric/panelize.md` composes; `libs/csharp/Rasm/.planning/Numerics/predicates.md` gains the derives-from note on its exact carrier.
- Unlocks: IDEAS.md `[POLYGON_NORMAL_OWNER]` — the floor owner realized.
- Anchors: `predicates.md` Newell fence as the reference fold; `RULINGS.md` Newell-owner row.

[MOMENT_HANDOFF_PROSE]-[QUEUED]: State the no-repack covariance handoff at the moment owner.
- Capability: consumer prose matches the layout contract — a direct packed-triangle handoff, never an unpack.
- Shape: one sentence repair in `libs/csharp/Rasm/.planning/Domain/stats.md` `[MOMENT_OWNERSHIP]` — `CloudKernel` hands the packed upper triangle straight to `SymmetricMatrix.Of` under the shared layout, no repack.
- Unlocks: IDEAS.md `[TRIANGULAR_ADDRESS_OWNER]` — prose stops contradicting the contract the delegation enforces.
- Anchors: `RULINGS.md` packed-upper layout row; `Spatial/cloud.md` `CovarianceOf` fence.
- Atomic: one sentence.

[PROJECT_SEQ_COLLAPSE]-[QUEUED]: Route the cloud sequence projections through the one sanctioned dispatch site.
- Capability: type-directed projection dispatch keeps its single owner; consumer-local reflection branches stay deleted forms.
- Shape: `libs/csharp/Rasm/.planning/Spatial/cloud.md` `[03]` — the `Seq<Vector3d>`/`Seq<double>`/`Seq<Plane>` arms route through `AtomProjection.Values<TItem, TOut>` and the private `ProjectSeq` helper deletes.
- Unlocks: `Numerics/atoms.md` `[05]` one-dispatch-site law holds corpus-wide with zero adapters.
- Anchors: `atoms.md` `Values` sequence-acceptance case; the `Output`-gate equivalence already proven at the call site.
- Atomic: one dispatch rewrite, one helper deletion.

[ENV_TELEMETRY_FIELD]-[BLOCKED]: `Env.Telemetry` projections settle on the analysis runtime.
- Capability: `EnvAsks`/`Asks`/`Taps` projections over the complete `Env.Telemetry` target — runtime capability reads with no service location.
- Shape: settled projection fences on `libs/csharp/Rasm/.planning/Analysis/query.md` `[03]-[OPERATION_RUNTIME_RESEARCH]`.
- Unlocks: `[KERNEL_SIGNAL_FABRIC]` — analysis pipelines read telemetry capability through the runtime record.
- Anchors: `Analysis/query.md` `Env` capsule; `libs/csharp/.api/api-languageext.md` runtime rows.
- Arms: `api-languageext.md` admits `public static Eff<RT, RT> runtime<RT>()`; until then the projections stay research.

[OP_COST_CAPSULE]-[BLOCKED]: Two-exit cost charge settles onto `Operation.Apply`.
- Capability: the two-exit charge composing the settled `[04]` `CostMark`/`OpCost` capsule onto `Operation.Apply` — success and failure exits both charge cost evidence.
- Shape: a settled `Operation.Apply` fence on `libs/csharp/Rasm/.planning/Analysis/query.md`.
- Unlocks: `[KERNEL_SIGNAL_FABRIC]` — cost evidence rides every operation exit.
- Anchors: `Domain/telemetry.md` `[04]` cost capsule; `libs/csharp/.api/api-languageext.md` runtime rows.
- Arms: `api-languageext.md` admits `public static Eff<RT, RT> runtime<RT>()`; until then the charge stays research code.

[EVIDENCE_SERIALIZATION]-[BLOCKED]: Frozen JSON identity settles beside the exact binary evidence blocks.
- Capability: the frozen `DDoubleJsonConverter` options identity joining the settled count-prefixed exact hi/lo `EvidenceWire.WriteBlock`/`ReadBlock` binary blocks.
- Shape: one static sealed identity row on `libs/csharp/Rasm/.planning/Drawing/pack.md`.
- Unlocks: `[COLUMNAR_WIRE_SCHEMA]` — wire consumers verify against a sealed contract identity.
- Anchors: `Drawing/pack.md` schema-and-evidence law; `libs/csharp/.api/api-system-text-json.md` options rows.
- Arms: `api-system-text-json.md` admits `public void JsonSerializerOptions.MakeReadOnly()`; until then the frozen identity stays research.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BENCH_CLAIM_ROWS]-[COMPLETE]: landed as `Domain/telemetry.md` `[05]-[BENCH_LEDGER]` with the four registered rows — `Simplify.HausdorffClaim`, `Parametric.FrameDefectClaim`, `Surfaces.CurvatureSummaryClaim`, `Flatten.DistortionClaim`.
[PACK_SCHEMA_IDENTITY]-[COMPLETE]: `Drawing/pack.md` `[03]-[SCHEMA_AND_EVIDENCE]` derives the `ContentHash` id from kind and field rows, validates id recomputation, stride/null rows, and the declaration roster, then gates `Describes` on valid schema and geometry carriers.
[SIGNAL_TAP_OWNER]-[COMPLETE]: `SpanBand` settled at `Domain/telemetry.md` `[04]-[SIGNAL_TAP]` — one `ActivitySource` per `KernelDomain` row, `Traced` the rail-valued bracket — with the activity family catalogued at `libs/csharp/.api/api-diagnostics-activity.md`.
