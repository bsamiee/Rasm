# [H1][RHINO_ANALYSIS_HANDOFF]
>**Dictum:** *Archive capability guides direction; modern typed analysis owns implementation.*

<br>

[IMPORTANT] This handoff records the completed archive-vs-modern analysis finalization pass. The archive is a concept catalogue, not source code authority.

[IMPORTANT] Treat this file as the analysis capability roadmap. Each session must update the status ledger after implementation so future sessions continue from the same source of truth.

---
## [1][CURRENT_TRUTH]
>**Dictum:** *Start from current worktree evidence, not prior-session memory.*

<br>

**Worktree:** current branch is `main`; current pass leaves unstaged implementation edits in analysis, tests, and this handoff. Preserve them unless the user explicitly asks to revert.

**Modern analysis LOC:** `libs/csharp/analysis/*.cs` is 2,123 LOC.

| [INDEX] | [FILE] | [LOC] | [ROLE] |
| :-----: | ------ | ----: | ------ |
| **1** | `libs/csharp/analysis/Analyze.cs` | 131 | Execution surface, scoped context, null/error rails. |
| **2** | `libs/csharp/analysis/Query.cs` | 459 | Query descriptor algebra, bounded aspect vocabulary, mass builders, compact analysis summaries, conformance residual/profile vocabulary. |
| **3** | `libs/csharp/analysis/Measure.cs` | 345 | Bounds, scalar measures, mass-property projections, conformance residual and profile projections. |
| **4** | `libs/csharp/analysis/Locate.cs` | 545 | Point/frame/normal/curvature/location queries, normalized-length curvature profiles, shared explicit scalar projections. |
| **5** | `libs/csharp/analysis/Extract.cs` | 385 | Extraction, read-only topology, primitives, mesh diagnostics, mesh check count evidence. |
| **6** | `libs/csharp/analysis/Intersect.cs` | 258 | Pair intersections and pair validation/output rails. |

**Relevant core LOC:** `libs/csharp/core/Domain/*.cs` is 761 LOC.

| [INDEX] | [FILE] | [LOC] | [ROLE] |
| :-----: | ------ | ----: | ------ |
| **1** | `libs/csharp/core/Domain/GeometryContext.cs` | 204 | Tolerances, units, context construction, mesh-intersection tolerance. |
| **2** | `libs/csharp/core/Domain/GeometryValidation.cs` | 453 | Geometry readiness masks, native operand validation, Rhino-native checks. |
| **3** | `libs/csharp/core/Domain/Operation.cs` | 104 | Operation keys, typed faults, output validation. |

**Archive LOC:** `.archive/libs/rhino/**/*.cs` is 10,744 LOC. Archive `analysis` alone is 1,521 LOC across `Analysis.cs`, `AnalysisConfig.cs`, `AnalysisCore.cs`, and `AnalysisCompute.cs`.

**Status terms:** `OPEN` means not started; `ACTIVE` means selected for current pass; `PARTIAL` means started but incomplete; `DONE` means implemented and validated by configured build, managed, and compile gates; `DEFERRED` means valuable later; `REJECTED` means excluded from modern `analysis`. Rhino runtime testhost blockers are recorded as execution caveats, not implementation incompleteness.

---
## [2][MODERN_PARADIGM]
>**Dictum:** *Modern code is a compact typed query algebra, not a folder-per-capability framework.*

<br>

The modern approach centers on `Query<TGeometry,TOut>` descriptors and `Analyze.Run` execution. Callers choose a query, pass one or more inputs through `params ReadOnlySpan<TGeometry>`, and receive `Validation<Error, Seq<TOut>>`.

**Modern strengths:**

- One coherent public execution style: `Analyze.Run(...)` and `Analyze.In(...).Run(...)`.
- No separate single/batch API split.
- Typed rails for null query, null geometry, missing context, invalid context, unsupported output, invalid input, and invalid result.
- Context-owned validation via `GeometryRequirement`, `GeometryCheck`, and `GeometryContext`.
- Rhino-native first implementation: `IsValidWithLog`, `GetBoundingBox`, `Mesh.Check`, `Surface.IsSolid`, `LengthMassProperties`, `AreaMassProperties`, `VolumeMassProperties`, `Intersection.*`, curve/surface evaluation APIs.
- Compact aspect vocabulary through `Bounds`, `Measure`, `Location`, `Topology`, `MassKind`, `CurvatureScalar`, `MeshCheckCount`, `ConformanceResidual`, `ResidualProfile`, and static `Query` factories.

**Modern risks:**

- `Query.Cast` and `Retype` are guarded but still rely on runtime type erasure.
- `Measure.cs` has repetitive mass projection arms and now carries conformance residual/profile pressure; future conformance work should extend explicit pair semantics only when downstream needs are concrete.
- `Locate.cs` and `Query.cs` exceed the `coding-csharp` scrutiny threshold; `Extract.cs` now carries mesh/topology pressure above the 350 LOC scrutiny signal; aggregate quality/fairness work should still avoid score-first APIs or repeated projection shapes.
- `MassKind.None` is a public invalid state used to satisfy enum shape rules; it must stay an explicit typed invalid-input rail.
- `GeometryResult.Many` preserves output order and accumulated failures through prepend-then-reverse aggregation.
- Rhino runtime tests are not part of default `pnpm check:cs` execution when the Rhino.Testing asset guard blocks runtime execution.

---
## [3][ARCHIVE_PARADIGM]
>**Dictum:** *Archive code is broad domain vocabulary with heavier architecture and weaker type rails.*

<br>

The archive follows a repeated capability-folder model. Documentation conflicts between an older 3-file pattern and the actual 4-file pattern, but the implementation is clearly organized around:

| [INDEX] | [FILE_KIND] | [ARCHIVE_ROLE] |
| :-----: | ----------- | -------------- |
| **1** | `X.cs` | Public static facade plus nested request/result records. |
| **2** | `XConfig.cs` | Metadata, validation modes, operation maps, constants. |
| **3** | `XCore.cs` | `Result<T>` orchestration and `UnifiedOperation` dispatch. |
| **4** | `XCompute.cs` | Raw Rhino and math algorithms. |

**Archive strengths:**

- Strong CAD/geometry vocabulary: quality, fairness, conformance, near-miss, stability, topology diagnosis, feature extraction, spatial indexing.
- Domain-facing results: `SurfaceQualityResult`, `CurveFairnessResult`, `MeshQualityResult`, `CurvatureProfileResult`, `ShapeConformanceResult`.
- Clear user workflows: analyze surface quality, analyze curve fairness, analyze mesh for FEA, classify intersections, find near misses, diagnose topology.
- Performance-minded techniques: `FrozenDictionary`, `ArrayPool`, `RTree`, and algorithm-specific dispatch tables.

**Archive weaknesses under current standards:**

- Object-heavy request/result architecture with broad public record proliferation.
- Runtime casts such as `(Result<T>)(object)` in orchestration.
- `object?` primitives in conformance outputs.
- Separate batch APIs and flags such as diagnostics, parallelism, and accumulate-errors knobs.
- Imperative compute files with loops, mutable arrays, null sentinels, and `try/finally`.
- Capability folder scaffolding that would create file bloat if copied into modern `libs/csharp`.

---
## [4][CAPABILITY_MAP]
>**Dictum:** *Classify archive value before selecting work.*

<br>

| [INDEX] | [ARCHIVE_CONCEPT] | [MODERN_STATUS] | [NEXT_DECISION] |
| :-----: | ----------------- | --------------- | --------------- |
| **1** | Curvature at a point | Modernized in `Location.CurvatureAt`. | Keep. |
| **2** | Curvature profiles | Modernized via raw normalized-length profiles, compact summaries, and explicit scalar projections with scalar/profile parity coverage. | Keep; fairness can now derive from scalar profile evidence. |
| **3** | Surface quality | Closed as evidence-only through explicit Gaussian/Mean scalar profiles, domain, continuity, mass outputs, topology evidence, primitives, mesh diagnostics, and residual profiles. | Reject score-first workflow APIs; future quality must add compact typed evidence only. |
| **4** | Curve fairness | Closed as evidence-only through curvature magnitude scalar streams and compact `CurvatureProfile` summaries. | Reject fairness scores; defer only approved typed inflection or bending evidence. |
| **5** | Mesh FEA quality | Modernized as Rhino-native `MeshCheckParameters` plus typed `MeshCheckCount` projections and topology parity evidence. | Reject aggregate FEA scores; defer face-level metric streams until typed semantics are approved. |
| **6** | Shape conformance | Modernized for explicit primitive-pair residual and compact residual-profile evidence over curve-line and surface-plane pairs. | Reject auto-detect and score APIs; keep additional pairs and max-sample evidence deferred until explicit typed semantics are approved. |
| **7** | Primitive extraction | Modernized for circle, arc, ellipse, polyline, plane, cylinder, sphere, cone, torus, and box. | Keep as conformance foundation; further residual primitive pairs stay deferred until explicit pair semantics are selected. |
| **8** | Feature detection | Mostly absent. | Reject broad workflow systems; defer only narrow descriptor-first native-backed evidence. |
| **9** | Intersection pairs | Modernized in `Intersect.cs`. | Keep refining pair coverage and outputs. |
| **10** | Intersection classification | Modernized as compact `IntersectionKind` projection over existing native pair outputs. | Keep; do not reselect unless classification vocabulary changes. |
| **11** | Near-miss and stability | Missing. | Defer only with native-backed descriptor semantics and explicit tolerance policy. |
| **12** | Read-only topology | Modernized as bounded `Topology` descriptor projections over boundary, adjacency, and non-manifold evidence. | Keep; richer topology result objects stay deferred until lower-level evidence demands them. |
| **13** | Topology healing | Absent. | Reject for `analysis`; mutating repair belongs elsewhere. |
| **14** | Spatial indexing | Absent. | Defer to a focused collection-level `RTree` owner; do not fold into scalar analysis files. |
| **15** | Morphology, fields, transforms | Mostly absent. | Out of current analysis branch. |

---
## [5][ROADMAP_LEDGER]
>**Dictum:** *Roadmap status connects focused passes into one coherent implementation arc.*

<br>

[IMPORTANT] Update this table at the end of every future session. Do not rely on chat history as the capability source of truth.

| [ORDER] | [CAPABILITY] | [STATUS] | [OWNER] | [DONE_WHEN] |
| :-----: | ------------ | :------: | ------- | ----------- |
| **1** | Analysis contract stabilization | DONE | `Analyze.cs`, `Query.cs`, `Operation.cs` | Public API compatibility is preserved; null query, null geometry, unsupported output, invalid input, and invalid result states stay on typed rails; erased casts report the owning operation key; result aggregation preserves order while accumulating candidate errors; future capability can extend existing descriptors without new facade/API families. |
| **2** | Curvature profile foundation | DONE | `Query.cs`, `Locate.cs` | Raw curve/surface profiles use normalized-length curve sampling; compact summaries expose curve `Magnitude` and surface `Gaussian` then `Mean`; explicit scalar projection supports curve `Magnitude` and surface `Gaussian`/`Mean` double streams; invalid count and unsupported scalar/output combinations stay on typed rails; managed gates pass and Rhino runtime specs compile. |
| **3** | Mesh diagnostics | DONE | `Extract.cs`, `GeometryValidation.cs` | Mesh diagnostics stay query-based; `Query.MeshCheck` remains canonical; typed `MeshCheckCount` projections expose bounded native `MeshCheckParameters` count evidence; mesh-intersection-like operations use Rhino's native coefficient multiplied by model absolute tolerance; managed tests do not require Rhino native runtime; Rhino runtime coverage locks native coefficient, mesh count, and topology parity behavior when runtime tests are available. |
| **4** | Primitive vocabulary | DONE | `Extract.cs` | `Query.Primitive<TGeometry,TOut>()` covers Rhino-native curve primitives `Circle`, `Arc`, `Ellipse`, and `Polyline`; surface primitives `Plane`, `Cylinder`, `Sphere`, `Cone`, and `Torus`; and Brep `Box`, with unsupported outputs remaining typed failures and conformance vocabulary still deferred. |
| **5** | Intersection semantics | DONE | `Intersect.cs`, `Query.cs` | Existing pair outputs support compact `IntersectionKind` classification without a parallel subsystem; point, overlap, mesh-plane, and mesh-mesh classifications have managed/API coverage and Rhino runtime specs compile. |
| **6** | Read-only topology diagnostics | DONE | `Query.cs`, `Extract.cs` | `Topology.Boundary`, `Topology.Adjacency`, and `Topology.NonManifold` are additive descriptor-based query outputs; boundary projects existing native naked-edge curves/polylines; adjacency and non-manifold project Rhino `ComponentIndex` evidence for Brep edges and mesh topology edges; boolean `Topology.NonManifold` returns `true` when non-manifold evidence exists; unsupported outputs and null geometry stay on typed rails; managed gates pass and Rhino runtime specs compile. |
| **7** | Conformance vocabulary | DONE | `Query.cs`, `Measure.cs` | Additive `ConformanceResidual`, `Conformance.Distance`, `Conformance.Rms`, `Conformance.WithinTolerance`, and `Query.Conformance<TGeometry,TPrimitive,TOut>(Conformance)` expose explicit curve-line and surface-plane residual evidence; raw distance streams, RMS projections, and model-tolerance booleans stay descriptor-first; invalid residual/count and unsupported outputs fail on typed rails before input execution; null geometry remains a managed typed rail; public source compatibility is preserved; managed gates pass and Rhino runtime specs compile. |
| **8** | Conformance residual profile | DONE | `Query.cs`, `Measure.cs` | Additive `ResidualProfile`, `ConformanceResidual.Profile`, and `Conformance.Profile(int)` expose compact count, min, max, mean, variance, RMS, tolerance, and within-tolerance evidence over existing curve-line and surface-plane residual streams; `Distance`, `Rms`, and `WithinTolerance` stay source-compatible; invalid counts, default descriptors, unsupported outputs, null geometry, and invalid native operands stay on typed rails; managed gates pass and Rhino runtime specs compile. |
| **9** | Quality workflows | DONE | `Query.cs`, `Locate.cs`, `Extract.cs`, `Measure.cs` | Quality and fairness are closed as evidence-only analysis: explicit curvature scalar projections, typed mesh check count projections, topology diagnostics, primitive vocabulary, conformance residual streams, and conformance residual profiles compose the approved lower-level surface without archive request/result objects; aggregate quality/fairness/FEA scores, auto-detect conformance workflows, and broad feature/pattern workflow APIs are rejected for `analysis`; remaining future work is blocked behind explicit typed evidence semantics. |
| **10** | Spatial indexing | DEFERRED | New file only if approved | Collection-level `RTree` capability needs a focused owner, range/proximity semantics, lifecycle rules, and tests; current `Analyze.Run` stays per-input and no broad clustering system is approved. |
| **11** | Topology healing | REJECTED | Not `analysis` | Mutating repair belongs outside read-only analysis. |
| **12** | Morphology, fields, transforms | REJECTED | Not current branch | Separate future capability areas, not blockers for analysis. |

**Session rule:** select 1-5 cohesive `OPEN` or `PARTIAL` roadmap items per pass when the work remains tightly bounded. Prefer fewer items when API risk, LOC pressure, runtime coverage, or test complexity grows. Finish or advance those items, then update `STATUS`, `DONE_WHEN`, and deferred pick-up criteria so future sessions add capability for real foundation reasons.

---
## [6][INTEGRATION_STRATEGY]
>**Dictum:** *Build higher-order capability from lower-level query primitives.*

<br>

[IMPORTANT] Next pass should choose a new approved capability branch, not reopen completed foundations or rejected areas. `analysis-spatial-index` is the next candidate only if a focused collection-level `RTree` owner, range/proximity semantics, lifecycle rules, and tests are approved first.

**Preferred integration order:**

1. **Stabilize existing analysis primitives.** Ensure query rails, subtype dispatch, Rhino tolerance policy, output validation, section markers, and managed/Rhino test lanes are clean.
2. **Extend existing analysis vocabulary.** Add capability through `Bounds`, `Measure`, `Location`, `Extract`, or `Intersect` when the ownership is natural.
3. **Add one focused type only when vocabulary needs it.** A small canonical result type is acceptable for a real aggregate concept, such as a compact curvature profile summary.
4. **Compose higher-order calls later.** Quality/conformance APIs should be projections over existing lower-level queries, not large copied subsystems.

**Capability branches:**

| [ORDER] | [BRANCH] | [OWNER] | [DIRECTION] |
| :-----: | -------- | ------- | ----------- |
| **1** | `analysis-curvature-profiles` | `Query.cs`, `Locate.cs` | Completed; keep as foundation for conformance and quality projections. |
| **2** | `analysis-mesh-diagnostics` | `Extract.cs`, `GeometryValidation.cs` | Completed; expose Rhino-native `MeshCheckParameters` and typed `MeshCheckCount` evidence without custom score heuristics. |
| **3** | `analysis-primitive-vocabulary` | `Extract.cs` | Extend `Primitive<TGeometry,TOut>()` with native `TryGet*` primitives before conformance scoring. |
| **4** | `analysis-intersection-semantics` | `Intersect.cs`, `Query.cs` | Completed; extend only when a new native pair or classification vocabulary is selected. |
| **5** | `analysis-topology-readonly` | `Query.cs`, `Extract.cs` | Completed; keep as foundation for quality and conformance projections. |
| **6** | `analysis-quality-projections` | `Query.cs`, `Locate.cs`, `Extract.cs` | Completed as evidence-only quality/fairness foundation; explicit curvature scalar projections, scalar/profile parity, and mesh check count evidence stay score-free. |
| **7** | `analysis-conformance-residual-vocabulary` | `Query.cs`, `Measure.cs` | Completed; explicit primitive-pair `Distance`, `Rms`, and `WithinTolerance` residual evidence exists for curve-line and surface-plane inputs without score APIs. |
| **8** | `analysis-conformance-residual-profile` | `Query.cs`, `Measure.cs` | Completed; compact residual profiles project count, min, max, mean, variance, RMS, tolerance, and within-tolerance evidence over existing residual streams without max-location evidence or score APIs. |
| **9** | `analysis-deferred-finalization` | `Query.cs`, `Locate.cs`, `Extract.cs`, `Measure.cs` | Completed; remaining analysis backlog is closed as `DONE`, `REJECTED`, or explicitly blocked `DEFERRED` with concrete pick-up criteria. |
| **10** | `analysis-spatial-index` | New file only if approved | Next candidate only if a collection-level `RTree` owner, range/proximity semantics, lifecycle rules, and tests are approved. |

---
## [7][QUALITY_RULES]
>**Dictum:** *Small capability must still meet the full architecture bar.*

<br>

[CRITICAL]:
- NEVER copy archive code, folder patterns, result types, request objects, or config architecture.
- NEVER add helper/util/wrapper files for one-caller logic.
- NEVER create single/batch splits, diagnostics knobs, parallelism knobs, or accumulate-error flags.
- NEVER hand-roll Rhino behavior when Rhino 9/WIP provides a native API.

[IMPORTANT]:
- ALWAYS keep `core` and `analysis` as one capability pipeline.
- ALWAYS keep public analysis usage descriptor-first and rail-returning.
- ALWAYS validate null query, null geometry, native operands, context, and output shape.
- ALWAYS separate managed contract tests from Rhino runtime tests.

**`coding-csharp` fit checks:**

- Use `Fin<T>` for synchronous fallible execution and `Validation<Error,T>` for accumulated public failures.
- Prefer discriminant-driven projection and folds over broad imperative branching.
- Keep one canonical type per concept.
- Use explicit declarations and named domain call arguments.
- Treat 350-400 LOC files as scrutiny signals, not automatic split triggers.
- Add files only when cohesion improves more than locality suffers.

---
## [8][RHINO_API_EVIDENCE]
>**Dictum:** *Primary Rhino APIs should own geometry semantics.*

<br>

Use current Rhino 9/WIP primary docs before making Rhino-specific claims or changes:

| [INDEX] | [API_AREA] | [PRIMARY_DOC] | [MODERN_USE] |
| :-----: | ---------- | ------------- | ------------ |
| **1** | Geometry validity and bounds | [GeometryBase](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.geometrybase) | `IsValidWithLog`, `GetBoundingBox`. |
| **2** | Curve evaluation and profiles | [Curve](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.curve) | `LengthParameter`, `NormalizedLengthParameter`, `CurvatureAt`, `FrameAt`, `DerivativeAt`. |
| **3** | Surface evaluation | [Surface](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.surface) | `FrameAt`, `NormalAt`, `CurvatureAt`, `ShortPath`, `IsSolid`, `Domain`. |
| **4** | Mesh diagnostics | [Mesh](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.mesh), [MeshCheckParameters](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.meshcheckparameters) | `Check`, typed `MeshCheckCount` projections, `GetSelfIntersections`, `GetNakedEdges`, `IsManifold`, `IsSolid`. |
| **5** | Intersections | [Intersection](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.intersect.intersection) | `CurveBrep`, `CurveCurve`, `MeshPlane`, `MeshMesh`, mesh tolerance coefficient. |
| **6** | Area mass | [AreaMassProperties](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.areamassproperties) | Area, centroid, error, radii, principal moments. |
| **7** | Volume mass | [VolumeMassProperties](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.volumemassproperties) | Volume, centroid, error, radii, principal moments. |
| **8** | Spatial indexing | [RTree](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtree) | Future collection-level spatial branch only. |
| **9** | Mesh topology graph | [MeshTopologyEdgeList](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.collections.meshtopologyedgelist), [MeshTopologyVertexList](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.collections.meshtopologyvertexlist) | Read-only adjacency and non-manifold component evidence. |
| **10** | Brep topology evidence | [BrepEdge](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.brepedge), [ComponentIndexType](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.componentindextype) | Read-only edge valence and component-index outputs. |
| **11** | Curve-line residual evidence | [Curve](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.curve), [Line](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.line) | `NormalizedLengthParameter`, `PointAt`, and `ClosestPoint` drive explicit conformance distance samples and compact residual profiles. |
| **12** | Surface-plane residual evidence | [Surface](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.surface), [Plane](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.plane) | `Domain`, `PointAt`, and `DistanceTo` drive explicit conformance distance samples and compact residual profiles. |

---
## [9][TEST_AND_GATE_TRUTH]
>**Dictum:** *Managed tests prove contracts; Rhino runtime tests prove native behavior.*

<br>

**Default gate:** `pnpm check:cs` runs restore, Debug/Release build, formatting verification, unused-code diagnostics, and managed tests. It does not necessarily execute Rhino runtime tests.

**Managed coverage now includes:**

- Null query and null geometry typed rails.
- Scoped context failures.
- Property factory compatibility.
- `Analyze.Scope` constructor non-public behavior.
- Tuple validation and native operand validation.
- `GeometryRequirement` inclusion.
- `GeometryResult` order and invalid candidate accumulation.
- `MassKind.None` as a single typed invalid-input failure before input execution.
- Unsupported query diagnostics preserving the owning operation vocabulary.
- Unsupported primitive outputs as typed failures.
- Invalid `Location.CurvatureProfile(count: 0)` as one typed failure before input execution.
- Unsupported curvature summary outputs as typed failures.
- Additive `Location.CurvatureProfile(count, CurvatureScalar)` factory coverage.
- Supported scalar descriptor construction for curve `Magnitude` and surface `Gaussian`/`Mean`.
- Default `Query.Locate<Curve,double>(Location.CurvatureProfile(count))` remains unsupported before input execution.
- Invalid explicit scalar curvature counts fail once before input execution.
- Unsupported explicit scalar combinations, including curve `Gaussian` and surface `Magnitude`, preserve `CurvatureAt` vocabulary before input execution.
- `Query.Intersect<Curve,Curve,IntersectionKind>()` as an additive typed descriptor.
- Unsupported intersection classification pairs as typed failures preserving `Intersect` vocabulary.
- `Query.Topology<TGeometry,TOut>(Topology)` as an additive typed descriptor.
- Unsupported topology outputs preserve `Topology` vocabulary before input execution.
- Null geometry through topology descriptors stays on typed rails.
- `Query.MeshCheckCount(MeshCheckCount)` as an additive typed descriptor.
- Invalid `MeshCheckCount.None` as one typed failure before input execution.
- Null geometry through mesh check count descriptors stays on typed rails.
- `Query.Conformance<TGeometry,TPrimitive,TOut>(Conformance)` as an additive typed descriptor.
- Invalid conformance residual and count values as one typed failure before input execution.
- Unsupported conformance outputs preserve `Conformance` operation vocabulary before input execution.
- Null geometry through conformance descriptors stays on managed typed rails without Rhino native runtime.
- `Query.Conformance<TGeometry,TPrimitive,ResidualProfile>(Conformance.Profile(count))` as an additive typed descriptor.
- Invalid conformance profile counts and unsupported profile outputs fail once before input execution.
- Null geometry through conformance profile descriptors stays on managed typed rails without Rhino native runtime.

**Rhino runtime coverage exists for:**

- Curve/surface/Brep/mesh analysis behavior.
- Rhino subtype dispatch such as `LineCurve` and `PlaneSurface`.
- Curvature/profile sampling.
- Surface/Brep volume readiness.
- Mesh diagnostics, naked edges, naked point status, self-intersections.
- Rhino-native primitive extraction for curve and surface primitives through `Query.Primitive<TGeometry,TOut>()`.
- Mesh intersection tolerance policy: model absolute tolerance times Rhino's mesh intersection tolerance coefficient.
- Intersection pair behavior and invalid native operands.
- Source coverage for normalized-length curve curvature profiles.
- Source coverage for curve `Magnitude` summaries and surface `Gaussian` then `Mean` summaries.
- Source coverage for explicit curve `Magnitude` scalar samples matching native `Curve.CurvatureAt(...).Length`.
- Source coverage for explicit surface `Gaussian` and `Mean` scalar samples matching native `Surface.CurvatureAt`.
- Source coverage that curve scalar streams match `CurvatureProfile` `Count`, `Minimum`, `Maximum`, `Mean`, and `Variance`.
- Source coverage for `count: 1` scalar/profile parity with zero variance.
- Source coverage for non-flat surface Gaussian/Mean scalar/profile parity against native `Surface.CurvatureAt`.
- Source coverage for point and overlap intersection classification, including mesh-derived overlap classifications.
- Source coverage for `Topology.Boundary`, `Topology.Adjacency`, and `Topology.NonManifold` mesh descriptors.
- Source coverage that boolean `Topology.NonManifold` is `true` when non-manifold evidence exists and `false` for a closed-good mesh case.
- Source coverage for mesh topology `ComponentIndexType.MeshTopologyEdge` evidence.
- Source coverage that `MeshCheckCount.NakedEdges` matches native `MeshCheckParameters.NakedEdgeCount` for an open mesh.
- Source coverage that closed-good mesh naked and non-manifold mesh check counts are zero.
- Source coverage that `MeshCheckCount.NonManifoldEdges` matches `Topology.NonManifold` component evidence.
- Source coverage for curve-line conformance residuals with exact and offset line cases, including raw distance, RMS, and tolerance projections.
- Source coverage for surface-plane conformance residuals with exact and offset plane cases, including raw distance, RMS, and tolerance projections.
- Source coverage for curve-line residual profiles with exact and offset line cases, including count, minimum, maximum, mean, variance, RMS, tolerance, and within-tolerance evidence.
- Source coverage for surface-plane residual profiles with exact and offset plane cases, including `count: 2` projecting four UV samples.
- Source coverage for conformance native operand failures through invalid `Line` and `Plane` operands.
- Representative mass properties.

**Gate truth on this handoff update:**

- `bash -n scripts/check-cs.sh` passed.
- `scripts/check-cs.sh --self-test` passed.
- `dotnet test tests/csharp/analysis/Analysis.Tests.csproj --configuration Release --no-restore` passed: `Analysis.Tests` 44.
- `dotnet build tests/rhino/Rhino.Tests.csproj --configuration Release --no-restore` passed.
- `pnpm check:cs` passed Debug/Release builds, format/analyzer posture, and managed tests: `Analysis.Tests` 44, `Core.Tests` 10, `Foundation.CsAnalyzer.Tests` 85.
- `RASM_RHINO_TESTS=1 pnpm check:cs` passed managed gates and skipped Rhino runtime execution at the asset guard: `Analysis.Tests` 44, `Core.Tests` 10, `Foundation.CsAnalyzer.Tests` 85; `Rhino.Testing` resolves below `net10.0` and crashes the RhinoWIP testhost on macOS unless `RASM_RHINO_TESTS_FORCE=1` is set.

**Known caveat:** forced Rhino runtime execution fails with: `check-cs: Rhino runtime tests require a Rhino.Testing net10.0 asset. Current dependency graph resolves below net10.0 and crashes the RhinoWIP testhost on macOS.` This is a Rhino.Testing execution caveat; it does not reopen implemented analysis capabilities whose managed gates pass and runtime specs compile.

---
## [10][NEXT_SESSION_PROMPT]
>**Dictum:** *The next prompt should force evidence-first selection and focused implementation.*

<br>

```markdown
# Objective

Read `AGENTS.md`, `CLAUDE.md`, and `handoff.md` first. Treat `handoff.md` as the living roadmap/source of truth, not background context.

Load and follow `coding-csharp` from `/Users/bardiasamiee/.codex/skills/coding-csharp/SKILL.md`.
Always load its foundation references: `validation.md` and `patterns.md`.
When updating `handoff.md`, follow the repo Markdown/documentation standards from `AGENTS.md`.

## Goal

Select the next branch from current `handoff.md`. The recommended branch is `analysis-spatial-index` only if a focused collection-level `RTree` owner is approved.

Do not reopen roadmap items already marked `DONE` or `REJECTED`. Do not add score-first quality/fairness/FEA APIs, archive workflow facades, auto-detect conformance, topology healing, broad feature/pattern systems, or spatial APIs without approved collection-level semantics.

## Spatial Owner Criteria

Approve `analysis-spatial-index` only when the plan defines:
- A focused collection-level owner that does not bloat `Extract.cs`, `Measure.cs`, or scalar query files.
- Exact range/proximity/overlap semantics over Rhino `RTree`.
- Typed rail behavior for empty collections, null members, invalid query geometry, and unsupported outputs.
- Lifecycle rules for disposable `RTree` instances.
- Managed/API tests plus Rhino runtime specs that compile when runtime execution is blocked.

## Scope

- Modern code: `libs/csharp/analysis/` and only relevant `libs/csharp/core/Domain/`.
- Archive input: `.archive/libs/rhino/`, used only as a capability/vocabulary catalogue.
- Do not port archive code, request/result architecture, config tables, folder patterns, wrappers, helper layers, compatibility facades, broad result bags, auto-detect workflows, or score-first workflows.

## Required Workflow

1. Read `AGENTS.md`, `CLAUDE.md`, and `handoff.md`.
2. Check `git status --short` and preserve existing worktree changes.
3. Establish current baseline: LOC, public API, tests, gates, and Rhino runtime caveats.
4. Verify current Rhino 9/WIP `RTree` primary docs before selecting implementation.
5. Produce a concrete implementation plan before edits with:
   - exact files,
   - code-shape snippets,
   - API impact,
   - test impact,
   - value/risk ranking,
   - why collection-level ownership is approved or still blocked.
6. Implement only selected `ACTIVE` items.
7. Update `handoff.md` before final response:
   - roadmap status ledger,
   - `DONE_WHEN` criteria,
   - test/gate truth and runtime caveats,
   - next recommended capability branch.
8. Validate with relevant C# gates and impacted tests. Run Rhino runtime tests when supported; if blocked, report the exact blocker.

## Implementation Rules

- Preserve modern typed query/ROP architecture.
- Keep `core` and `analysis` as one coherent capability pipeline.
- Prefer refining existing files over adding files.
- Add a file only when a focused collection-level owner improves cohesion.
- Use Rhino 9/WIP native APIs directly wherever available.
- Prefer compact canonical typed shapes when evidence needs more than scalar outputs.
- Avoid single/batch API splits, knobs, config objects, compatibility wrappers, helper files, broad rewrites, and broad result bags.
- Preserve public source compatibility unless explicitly justified and approved.
- Do not implement topology healing, mutating repair workflows, morphology, fields, transforms, or score APIs.

## Validation

Run relevant gates:

- `bash -n scripts/check-cs.sh`
- `scripts/check-cs.sh --self-test`
- `dotnet test tests/csharp/analysis/Analysis.Tests.csproj --configuration Release --no-restore`
- `dotnet test tests/csharp/core/Core.Tests.csproj --configuration Release --no-restore` when core is touched
- `dotnet build tests/rhino/Rhino.Tests.csproj --configuration Release --no-restore`
- `pnpm check:cs`
- `RASM_RHINO_TESTS=1 pnpm check:cs`

If Rhino runtime is blocked, report the exact blocker and which managed gates still passed.

## Deliver

1. Selected `ACTIVE` items and why.
2. Files changed and why each belongs there.
3. Public API before vs after.
4. Baseline LOC vs final LOC.
5. Rhino 9/WIP evidence with primary-source links.
6. Tests and quality gates run, including runtime caveats.
7. `handoff.md` roadmap updates made.
8. Remaining execution caveats and next recommended capability branch.
```

---
## [11][OPEN_DECISIONS]
>**Dictum:** *Future work should choose capability branch before editing.*

<br>

**Best immediate branch:** `analysis-spatial-index`, only if a focused collection-level owner is approved.

**Closed decisions:**

1. **Quality evidence projections:** DONE as lower-level evidence from scalar profiles, mesh count evidence, topology diagnostics, primitive extraction, and conformance residual profiles.
2. **Curve fairness aggregates:** REJECTED as score-first workflow APIs; current fairness evidence is curvature magnitude streams and compact summaries.
3. **Mesh FEA aggregate quality:** REJECTED as aggregate score semantics; face-level metric streams remain blocked until a compact typed shape is approved.
4. **Auto-detect conformance:** REJECTED because archive `object?` primitive/result workflows conflict with explicit typed pairs.
5. **Feature/pattern workflow detection:** REJECTED as broad analysis facade; only narrow descriptor-first native evidence remains viable.
6. **Topology healing:** REJECTED because mutating repair belongs outside read-only `analysis`.

**Deferred pick-up criteria:**

- **Curvature summaries:** Do not reselect unless public summary or scalar projection semantics change, or runtime execution reveals a concrete defect.
- **Intersection classification:** Do not reselect unless a new native pair, result shape, or classification vocabulary is selected.
- **Read-only topology diagnostics:** Do not reselect unless richer loop, adjacency, or Brep non-manifold result shapes are approved from concrete downstream needs.
- **Conformance residual profile:** Do not reselect unless profile semantics change, runtime execution reveals a concrete defect, or a compact typed max-location/sample-evidence shape is explicitly approved.
- **Additional conformance primitive pairs:** Pick up curve-arc/circle and surface cylinder/sphere/cone/torus residuals only after explicit pair semantics are selected and Rhino-native APIs are confirmed.
- **Quality/fairness workflows:** Do not reselect score APIs; pick up only compact typed evidence projections over primitives, scalar curvature profiles, mesh check count evidence, mesh diagnostics, topology diagnostics, and residual profiles.
- **Mesh FEA streams:** Pick up face-level aspect, skew, or Jacobian evidence only after typed semantics and Rhino-native/runtime parity expectations are approved.
- **Spatial indexing:** Pick up only on a separately approved collection-level branch with a focused `RTree` owner, range/proximity semantics, lifecycle rules, and tests.
- **Feature/pattern detection and near-miss stability:** Pick up only narrow native-backed descriptors after extraction, intersection, topology, and tolerance rails expose enough stable evidence to avoid broad workflow facades.

**Finalization result:** `analysis-deferred-finalization` closed every remaining `PARTIAL` or `DEFERRED` analysis item as `DONE`, `REJECTED`, or explicitly blocked `DEFERRED` with owner, blocker, and pick-up criterion.

**Next recommended capability branch:** `analysis-spatial-index`, only after focused collection-level `RTree` ownership is approved.

**Reject inside `analysis`:**

- Topology healing.
- Mutating repair workflows.
- Archive request/result/config architecture.
- Broad folder scaffolds.
- Wrapper modules and compatibility layers.
- Aggregate conformance, quality, or fairness score APIs without approved typed evidence semantics.
