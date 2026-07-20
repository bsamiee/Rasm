# [RASM_TASKLOG]

`Rasm` open and closed work distilled from `IDEAS.md` and design-page RESEARCH residuals. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[SIGNAL_TAP_OWNER]-[ACTIVE]: Author the kernel signal owner — hook registry, instrument projection, and fault-band metrics on one new `Domain` page.
- Capability: `libs/csharp/Rasm/.planning/Domain/telemetry.md` (`Domain/Telemetry.cs`) — typed hook registry with `rasm.rasm.<domain>.<point>` points and veto/observe/replay modalities, subscriber-fault isolation onto the `Fin` rail, receipt-fact taps, `rasm.kernel.<measure>` UCUM instruments minted through `IMeterFactory.Create(MeterOptions)`, per-namespace `ActivitySource` rows, and one band-2400 fault counter partitioned by `GeometryFault` case.
- Shape: registry and instruments are instance-owned and composition-entered — no static registry, no ambient meter; instruments project receipt facts, never scatter emit calls into domain kernels.
- Unlocks: `[KERNEL_SIGNAL_FABRIC]` — the kernel arm the branch `InstrumentFan` merges.
- Anchors: `libs/csharp/.api/api-diagnostics-metrics.md` mint/create/write rows, `Domain/rails.md` fault-band and threading law, HANDOFF hook-rail law.

[ENV_TELEMETRY_FIELD]-[QUEUED]: Thread the telemetry sink through the operation runtime.
- Capability: `Env` gains the telemetry-sink field beside `Context`/`Progress`/`Cancellation`; `Analyze` scope builders accept it; `Domain/rails.md` threading law names the sink capsule so synchronous rails below the `Eff` floor stay explicit-parameter.
- Shape: edits to `libs/csharp/Rasm/.planning/Analysis/query.md` (host-frozen `Env` record extension per its growth law) and `libs/csharp/Rasm/.planning/Domain/rails.md`.
- Unlocks: `[KERNEL_SIGNAL_FABRIC]` runtime carriage without a second rail.
- Anchors: `Analysis/query.md` growth row "a telemetry sink is one field on `Env`", positional Grasshopper construction noted as the frozen-shape ripple to re-verify.
- Atomic: one `Env` field with its scope-builder and law rows.

[OP_COST_CAPSULE]-[QUEUED]: Uniform op-cost evidence captured at the operation runtime.
- Capability: elapsed time, allocated bytes, and iteration counts captured once at `Operation.Apply`/`Prepare` and projected as a cost fact per `Op` key through the signal tap — the kernel-side billing-truth feed app strata attribute to tenants.
- Shape: a cost-capsule band on `libs/csharp/Rasm/.planning/Analysis/query.md` and its projection row on `libs/csharp/Rasm/.planning/Domain/telemetry.md`.
- Unlocks: `[KERNEL_SIGNAL_FABRIC]` cost-attribution channel.
- Anchors: `Analysis/query.md` `Apply` effect fold, `Domain/rails.md` `Op` value law.

[BENCH_CLAIM_ROWS]-[QUEUED]: Mint the `BenchClaim` vocabulary and thread the three settled speed-gated lanes.
- Capability: `BenchClaim` typed row (claim `Op` key, vectorized lane, reference lane, admission threshold) with its ledger fold on `libs/csharp/Rasm/.planning/Domain/telemetry.md`; claims threaded at the `libs/csharp/Rasm/.planning/Processing/decimate.md` Hausdorff `TensorPrimitives.Max` lane and the `libs/csharp/Rasm/.planning/Parametric/curve.md` / `libs/csharp/Rasm/.planning/Parametric/surface.md` tensor-reduction lanes, then one sweep for unregistered vectorized claims.
- Shape: ledger rows beside the instrument bands — one signal owner, two evidence families.
- Unlocks: `[BENCH_CLAIM_LEDGER]` — mechanical corpus-gate enumeration.
- Anchors: `Processing/decimate.md` corpus-gate reference-row language.

[PACK_SCHEMA_IDENTITY]-[QUEUED]: Schema-identity descriptors on the encoding owner.
- Capability: `EncodedGeometry` descriptor band gains `ContentHash`-derived schema id, field names, dtype/stride, and null semantics so a consumer maps any kernel wire onto a columnar batch without out-of-band knowledge.
- Shape: extension of the settled descriptor and `View<T>` seam on `libs/csharp/Rasm/.planning/Drawing/pack.md`.
- Unlocks: `[COLUMNAR_WIRE_SCHEMA]` — the Persistence Arrow adapter reads one schema authority.
- Anchors: `Drawing/pack.md` `[TENSOR_RESIDENCY_SEAM]`, `Domain/identity.md` deterministic derivation surface.

[EVIDENCE_SERIALIZATION]-[QUEUED]: Lossless 106-bit evidence serialization rows.
- Capability: receipt fields carrying `ddouble` evidence serialize through `DDoubleJsonConverter` on `JsonSerializerOptions` and round-trip binary through `DoubleDoubleIOExpand` — exact hi/lo preserved in support bundles and federation payloads.
- Shape: a serialization band on `libs/csharp/Rasm/.planning/Drawing/pack.md` beside `EncodingKind`.
- Unlocks: `[COLUMNAR_WIRE_SCHEMA]` evidence egress without precision loss.
- Anchors: `libs/csharp/Rasm/.api/api-doubledouble.md` converter and binary I/O detail rows.
- Atomic: two serialization rows on the settled encoding page.

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

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
