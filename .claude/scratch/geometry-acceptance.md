# [GEOMETRY_ACCEPTANCE]

Acceptance verification for the geometry campaign close. Four `[05]` dry-run traces walked end-to-end across the landed kernel pages; generic cross-folder symbol-resolution sweep in flight. Disposition per finding: FIXED / SKIPPED-COLLISION / PASS.

## [01]-[DRY_RUN_VERDICTS]

| [TRACE] | [VERDICT] | [SPINE] |
| :-----: | :-------: | :------ |
| (a) Generation SpineRef | PASS | `ContentHash.Of` (`Domain/identity.md:31`) + `Encode.Apply` (`Drawing/pack.md:205`) → `curve.md` `StationPlan.[T0,T1]`/`StationField` RMF (Wang double-reflection, `FrameDefect` witness) → `surface.md` geodesic+isoline UV pullback → `subdivide.md:15` → `panelize.md:15` → `patternmap.md:156` `InstanceStream.Mapped` per-instance SoA frames |
| (b) Fabrication toolpath | PASS | `offset.md` `JoinType`/`OffsetOp` family + `Medial`→`ClearanceNode(At, Radius, NearestEdge)` → `slice.md` `SliceStack` parent-forest nesting → `[V8]` chain `fields.md:287` `SignedDistanceFromMeshCase` → `reconstruct.md` GWN → `index.md` BVH `SpatialQuery.Winding` → `develop.md` `DevelopOp.Unroll` + `DevelopmentReceipt.MaxIsometry` witness → `delaunay.md:741` `LowerHull` (`[V11]` tier) |
| (c) Documentation HLR | PASS | `view.md:171` `View.Apply(ViewOp)` plan+section duality → `DrawingProjection` visible/hidden × silhouette/crease/boundary/intersection, successor-link chaining (`ProjectedSegment.Next`), E1 killed by construction (`view.md:424`) → `intersect.md` `PlaneMesh`/`SegmentSegment`/`SegmentTriangle` → `index.md` `Build`/`Overlap`/`Range`/`Winding` |
| (d) Reality capture | PASS | `cloud.md:84` `VectorCloud.Cluster` → `neighbors.md:360` `OrientNormals` (Prim MST) → `fit.md` MLESAC `FitKind.Plane`/`Cylinder` over `NeighborSource.StaticCase` kd-tree → `repair.md:209` `Heal.Repair` predicate-gated (`Predicate.Orient2D`, `predicates.md:171`) → `decimate.md:249` boundary link-condition + `BoundaryPenalty` + `HausdorffCeiling` → `flatten.md:194` `ChartAtlas` under ELIMINATE-BOUNDARY-ROWS Dirichlet (`MeshDec.Reduced`, `flatten.md:474`) → `naming.md:58` `NamingOp.Track` re-anchor over the `[REANCHOR_INJECTIVITY]` harness |

## [02]-[TRACE_NOTES]

- (a)/3 — PASS, attribution note: the brief's "isoline families ... `fields.md` `SampleDetailed`" link over-attributes. `surface.md` isolines are pure NURBS `IsoCurve` extraction and never cite `fields.md`; `SampleDetailed` exists on its owner (`fields.md:373`) with no dangling consumer. Brief-side over-attribution, not a corpus defect — no edit warranted on any page.
- (a)/3 — visibility seam: `surface.md` (`Rasm.Parametric`) cites `geodesics.md` internal members (`EnsureGeodesicDistances` `geodesics.md:107`, `PropagateWindows` `geodesics.md:231`) in signature-pinned kernel comments; the public geodesic entries are `HeatGeodesicAt`/`GeodesicTangentAt`/`VectorHeatAt` (`geodesics.md:16,782`). Cross-assembly composition requires `InternalsVisibleTo` or routing through the public entries. Comment-tier citation, not a broken code seam — recorded for the redteam, not edited.
- (b)/4 — naming convention: `fields.md:287` physically declares `SignedDistanceFromMeshCase` (the page's `-Case` suffix convention for `ScalarField` union arms); consumers cite the canonical case name. Not a mismatch.
- (c)/5 — `HiddenLineResult` is prose-named at `view.md:426` as the Fabrication-side thin projection (FAB:47 route); the geometry-side declared carrier is `DrawingProjection` (`view.md:119`). Owner split is correct by stratum — no kernel edit.

## [03]-[SYMBOL_SWEEP]

Two halves, all nine folders, load-bearing citations only (entries, receipts, fault ctors, seam contracts).

Half 1 — Numerics/Spatial/Solving/Analysis: [CLEAN]. All fault-payload discriminants (`EntityKind`→`naming.md:37`, `HealStage`→`repair.md:47`, `ChartId`→`flatten.md:46`, `PrimitiveKind`→`intersect.md:44`, `EdgeKind`→`view.md:58`), all frozen seam delegations (`GeodesicKernel.*At`, `SegmentKernel.*At`, `DecAssembly.HodgeVectorAt`, `SurfaceProjection.ShapeOperator`), all fault ctors (`DegenerateInput` 2400 … `FitFault` 2428) resolve. Solving-absorption clean: zero stale `Rasm.Processing` solver/fit citations; inbound consumers (`Domain/stats.md`, `Spatial/cloud.md`) cite `Solving/fit`.

Half 2 — Meshing/Processing/Parametric/Domain/Drawing: one failure class, now closed.

| [FINDING] | [DISPOSITION] |
| :-------- | :-----------: |
| Phantom type `EncodeOp` — owner `Spatial/reconciliation.md` declares `EncodeForm` `[Union]` (case `Parametric`, lines 51/56) and `ReconcileOp.Encode(EncodeForm)` (line 45); no `EncodeOp` exists anywhere. Six consumer sites: `Parametric/curve.md:5`, `curve.md:283`, `nurbs.md:17`, `nurbs.md:414`, `surface.md:5`, `surface.md:245`. Same pages spell `EncodeForm.Parametric` correctly elsewhere (`nurbs.md:5,438`) — owner right, consumers drifted. | FIXED — six one-token `EncodeOp`→`EncodeForm` edits at the consumers; fences balanced pre-edit, no collision; `rg EncodeOp` over the corpus now returns zero |

Advisories carried to the redteam (symbols resolve; accessibility is the question, not existence):
- `Solving/solver.md:677` uses `svd.U.At(i,k)`; `Matrix.At(int,int)` is `internal` at `Numerics/matrix.md:195` — cross-assembly visibility.
- `Drawing/view.md` calls `MeshFeaturePolicy.Of(...)`; declared `internal` at `Processing/segment.md:207`.
- `surface.md` kernel comments cite `internal` `geodesics.md` members (`EnsureGeodesicDistances:107`, `PropagateWindows:231`); public entries `HeatGeodesicAt`/`GeodesicTangentAt`/`VectorHeatAt` exist (`geodesics.md:16,782`).
- Missing-`using` gaps with correct owners: `Processing/register.md:25-26` (Domain vocabulary, 19 sites), same pattern `segment.md`/`geodesics.md`; `Parametric/develop.md`/`panelize.md` cite owners in Packages prose without the import.
- `Parametric/locate.md:86` prose shorthand `Query.Operation<TGeometry,TOut>()` — no `Query` type in Analysis; the real forward (`AnalysisQuery.LocationCase → Location.Operation()`) is spelled correctly elsewhere on the page.

## [04]-[VERDICT]

The geometry corpus is internally closed. All four `[05]` acceptance dry-runs compose end-to-end from landed fences with zero missing owners: every entry the traces reach (`ContentHash.Of`, `Encode.Apply`, `Parametric.Apply`/`Stations`, `Surfaces.Apply`, `Subdivision.Apply`, `Panelize.Apply`, `Pattern.Apply`, `Offsetting.Apply`/`Medial`, `Slicing.Apply`, `Development.Apply`/`Unroll`, `LowerHull`, `View.Apply`, `VectorCloud.Cluster`, `Fit.Apply`, `Heal.Repair`, `Simplify.Apply`, `Flatten.Apply`, `Naming.Apply`) is declared on its owning page with the signature its consumers cite, and every composed cross-folder contract — the `[V8]` SDF chain, the `[V9]` kd-tree lane, the `[V3]` Section seam, the `[V2]` identity seam, the `[V11]` hull tier — resolves hop by hop. The one phantom the sweep surfaced (`EncodeOp`, six Parametric sites) was a naming drift against a correctly-declared owner and is fixed; no seam was structurally broken. Residual risk is confined to the accessibility tier — three `internal` members consumed cross-assembly — which is an `InternalsVisibleTo`/public-promotion decision for the redteam, not a reachability gap. Every declared entry is reachable; every composed contract is real.
