# [H1][RHINO_ANALYSIS_HANDOFF]
>**Dictum:** *Archive capability guides direction; modern typed analysis owns implementation.*

<br>

[IMPORTANT] This handoff prepares the next session for an ultra-deep archive-vs-modern analysis pass. The archive is a concept catalogue, not source code authority.

[IMPORTANT] Treat this file as the analysis capability roadmap. Each session must update the status ledger after implementation so future sessions continue from the same source of truth.

---
## [1][CURRENT_TRUTH]
>**Dictum:** *Start from current worktree evidence, not prior-session memory.*

<br>

**Worktree:** current branch is `main`; target files already have unstaged edits from prior implementation passes. Preserve them unless the user explicitly asks to revert.

**Modern analysis LOC:** `libs/csharp/analysis/*.cs` is 1,706 LOC.

| [INDEX] | [FILE] | [LOC] | [ROLE] |
| :-----: | ------ | ----: | ------ |
| **1** | `libs/csharp/analysis/Analyze.cs` | 131 | Execution surface, scoped context, null/error rails. |
| **2** | `libs/csharp/analysis/Query.cs` | 427 | Query descriptor algebra, bounded aspect vocabulary, mass builders, compact analysis summaries. |
| **3** | `libs/csharp/analysis/Measure.cs` | 136 | Bounds, scalar measures, mass-property projections. |
| **4** | `libs/csharp/analysis/Locate.cs` | 493 | Point/frame/normal/curvature/location queries, normalized-length curvature profiles. |
| **5** | `libs/csharp/analysis/Extract.cs` | 261 | Extraction, read-only topology, primitives, mesh diagnostics. |
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
- Compact aspect vocabulary through `Bounds`, `Measure`, `Location`, `MassKind`, and static `Query` factories.

**Modern risks:**

- `Query.Cast` and `Retype` are guarded but still rely on runtime type erasure.
- `Measure.cs` has repetitive mass projection arms.
- `Locate.cs` and `Query.cs` exceed the `coding-csharp` scrutiny threshold; density is acceptable only when it removes real duplication.
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
| **2** | Curvature profiles | Modernized via raw normalized-length profiles and compact summaries. | Keep; do not reselect unless API semantics change. |
| **3** | Surface quality | Partially present through curvature, domain, continuity, mass outputs. | Defer score API until quality semantics have lower-level evidence beyond curvature summaries. |
| **4** | Curve fairness | Missing as aggregate quality concept. | Defer; derive from curvature profile later. |
| **5** | Mesh FEA quality | Partially present via `Query.MeshCheck` and validation. | Refine diagnostics before inventing quality scores. |
| **6** | Shape conformance | Partially present through primitive extraction, curvature summaries, and mass/measure. | Defer; needs typed residual semantics, not archive-style score objects. |
| **7** | Primitive extraction | Modernized for circle, arc, ellipse, polyline, plane, cylinder, sphere, cone, torus, and box. | Keep conformance projections deferred until residual semantics are typed. |
| **8** | Feature detection | Mostly absent. | Defer; avoid broad feature system. |
| **9** | Intersection pairs | Modernized in `Intersect.cs`. | Keep refining pair coverage and outputs. |
| **10** | Intersection classification | Modernized as compact `IntersectionKind` projection over existing native pair outputs. | Keep; do not reselect unless classification vocabulary changes. |
| **11** | Near-miss and stability | Missing. | Defer; introduces sampling and tolerance policy knobs. |
| **12** | Read-only topology | Partially present via naked edges, components, manifold checks. | Candidate after analysis branch is stable. |
| **13** | Topology healing | Absent. | Reject for `analysis`; mutating repair belongs elsewhere. |
| **14** | Spatial indexing | Absent. | Future focused module only if collection-level querying is approved. |
| **15** | Morphology, fields, transforms | Mostly absent. | Out of current analysis branch. |

---
## [5][ROADMAP_LEDGER]
>**Dictum:** *Roadmap status connects focused passes into one coherent implementation arc.*

<br>

[IMPORTANT] Update this table at the end of every future session. Do not rely on chat history as the capability source of truth.

| [ORDER] | [CAPABILITY] | [STATUS] | [OWNER] | [DONE_WHEN] |
| :-----: | ------------ | :------: | ------- | ----------- |
| **1** | Analysis contract stabilization | DONE | `Analyze.cs`, `Query.cs`, `Operation.cs` | Public API compatibility is preserved; null query, null geometry, unsupported output, invalid input, and invalid result states stay on typed rails; erased casts report the owning operation key; result aggregation preserves order while accumulating candidate errors; future capability can extend existing descriptors without new facade/API families. |
| **2** | Curvature profile foundation | DONE | `Query.cs`, `Locate.cs` | Raw curve/surface profiles use normalized-length curve sampling; compact summaries expose curve `Magnitude` and surface `Gaussian` then `Mean`; invalid count and unsupported outputs stay on typed rails; managed gates pass and Rhino runtime specs compile. |
| **3** | Mesh diagnostics | DONE | `Extract.cs`, `GeometryValidation.cs` | Mesh diagnostics stay query-based; mesh-intersection-like operations use Rhino's native coefficient multiplied by model absolute tolerance; managed tests do not require Rhino native runtime; Rhino runtime coverage locks the native coefficient behavior when runtime tests are available. |
| **4** | Primitive vocabulary | DONE | `Extract.cs` | `Query.Primitive<TGeometry,TOut>()` covers Rhino-native curve primitives `Circle`, `Arc`, `Ellipse`, and `Polyline`; surface primitives `Plane`, `Cylinder`, `Sphere`, `Cone`, and `Torus`; and Brep `Box`, with unsupported outputs remaining typed failures and conformance vocabulary still deferred. |
| **5** | Intersection semantics | DONE | `Intersect.cs`, `Query.cs` | Existing pair outputs support compact `IntersectionKind` classification without a parallel subsystem; point, overlap, mesh-plane, and mesh-mesh classifications have managed/API coverage and Rhino runtime specs compile. |
| **6** | Read-only topology diagnostics | OPEN | `Extract.cs`, `GeometryValidation.cs` | Boundary, adjacency, non-manifold, and component diagnostics are query outputs, not repair operations. |
| **7** | Conformance vocabulary | DEFERRED | `Measure.cs`, `Extract.cs`, possible focused type | Primitive vocabulary and curvature summaries can support typed conformance projections. |
| **8** | Quality workflows | DEFERRED | Existing analysis files first | Quality/fairness workflows compose lower-level primitives without archive-style request objects. |
| **9** | Spatial indexing | DEFERRED | New file only if approved | Collection-level `RTree` capability has a focused owner and no broad clustering system. |
| **10** | Topology healing | REJECTED | Not `analysis` | Mutating repair belongs outside read-only analysis. |
| **11** | Morphology, fields, transforms | REJECTED | Not current branch | Separate future capability areas, not blockers for analysis. |

**Session rule:** select 1-3 `OPEN` or `PARTIAL` roadmap items per pass. Finish or advance those items, then update `STATUS`, `DONE_WHEN`, and deferred pick-up criteria so future sessions add capability for real foundation reasons.

---
## [6][INTEGRATION_STRATEGY]
>**Dictum:** *Build higher-order capability from lower-level query primitives.*

<br>

[IMPORTANT] Finish the `analysis` branch of capability before opening topology, spatial, morphology, fields, or transforms. Each session should select 1-3 roadmap items, not the whole backlog.

**Preferred integration order:**

1. **Stabilize existing analysis primitives.** Ensure query rails, subtype dispatch, Rhino tolerance policy, output validation, section markers, and managed/Rhino test lanes are clean.
2. **Extend existing analysis vocabulary.** Add capability through `Bounds`, `Measure`, `Location`, `Extract`, or `Intersect` when the ownership is natural.
3. **Add one focused type only when vocabulary needs it.** A small canonical result type is acceptable for a real aggregate concept, such as a compact curvature profile summary.
4. **Compose higher-order calls later.** Quality/conformance APIs should be projections over existing lower-level queries, not large copied subsystems.

**Capability branches:**

| [ORDER] | [BRANCH] | [OWNER] | [DIRECTION] |
| :-----: | -------- | ------- | ----------- |
| **1** | `analysis-curvature-profiles` | `Query.cs`, `Locate.cs` | Completed; keep as foundation for conformance and quality projections. |
| **2** | `analysis-mesh-diagnostics` | `Extract.cs`, `GeometryValidation.cs` | Expose Rhino-native `MeshCheckParameters` meaning and validation evidence without custom score heuristics. |
| **3** | `analysis-primitive-vocabulary` | `Extract.cs` | Extend `Primitive<TGeometry,TOut>()` with native `TryGet*` primitives before conformance scoring. |
| **4** | `analysis-intersection-semantics` | `Intersect.cs`, `Query.cs` | Completed; extend only when a new native pair or classification vocabulary is selected. |
| **5** | `analysis-topology-readonly` | `Extract.cs`, `GeometryValidation.cs` | Add read-only topology outputs only after core analysis is stable. |
| **6** | `analysis-spatial-index` | New file only if approved | Collection-level `RTree` does not cleanly belong in current files; keep it future and focused. |

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
| **4** | Mesh diagnostics | [Mesh](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.mesh) | `Check`, `GetSelfIntersections`, `GetNakedEdges`, `IsManifold`, `IsSolid`. |
| **5** | Intersections | [Intersection](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.intersect.intersection) | `CurveBrep`, `CurveCurve`, `MeshPlane`, `MeshMesh`, mesh tolerance coefficient. |
| **6** | Area mass | [AreaMassProperties](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.areamassproperties) | Area, centroid, error, radii, principal moments. |
| **7** | Volume mass | [VolumeMassProperties](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.volumemassproperties) | Volume, centroid, error, radii, principal moments. |
| **8** | Spatial indexing | [RTree](https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.rtree) | Future collection-level spatial branch only. |

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
- `Query.Intersect<Curve,Curve,IntersectionKind>()` as an additive typed descriptor.
- Unsupported intersection classification pairs as typed failures preserving `Intersect` vocabulary.

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
- Source coverage for point and overlap intersection classification, including mesh-derived overlap classifications.
- Representative mass properties.

**Gate truth on this handoff update:**

- `pnpm check:cs` passed Debug/Release builds, format/analyzer posture, and managed tests: `Analysis.Tests` 32, `Core.Tests` 10, `Foundation.CsAnalyzer.Tests` 85.
- `dotnet build tests/rhino/Rhino.Tests.csproj --configuration Release --no-restore` passed.
- `CONFIGURATIONS=Release RASM_RHINO_TESTS=require scripts/check-cs.sh` built and passed managed tests, then failed at the runtime guard.

**Known caveat:** forced Rhino runtime execution fails with: `check-cs: Rhino runtime tests require a Rhino.Testing net10.0 asset. Current dependency graph resolves below net10.0 and crashes the RhinoWIP testhost on macOS.` This is a Rhino.Testing execution caveat; it does not reopen implemented analysis capabilities whose managed gates pass and runtime specs compile.

---
## [10][NEXT_SESSION_PROMPT]
>**Dictum:** *The next prompt should force evidence-first selection and focused implementation.*

<br>

```markdown
# Objective

Perform an evidence-first archive-vs-modern capability review, then advance 1-3 roadmap items from `handoff.md` in the modern C# analysis pipeline.

Compare `.archive/libs/rhino/` against current `libs/csharp/`, focusing on:

- `libs/csharp/analysis/`
- only `libs/csharp/core/Domain/` files that feed, validate, constrain, duplicate, or shape analysis behavior

Use the archive as a capability and vocabulary catalogue only. Do not port code, request/result architecture, config tables, folder patterns, or helper layers.

The goal is to integrate archive value into the modern lower-level foundation across multiple sessions. Use `handoff.md` as the roadmap/source of truth: select 1-3 actionable `OPEN` or `PARTIAL` items, implement them properly, then update the roadmap ledger.

## Required Context

1. Read `AGENTS.md`, `CLAUDE.md`, and `handoff.md`.
2. Load `coding-csharp` plus foundation references `validation.md` and `patterns.md`; load `effects.md`, `transforms.md`, and `testing.md` when relevant.
3. Check `git status --short` and preserve existing worktree changes.
4. Fully inspect current `libs/csharp/analysis/*.cs`.
5. Inspect relevant `libs/csharp/core/Domain/*.cs`.
6. Inspect `.archive/libs/rhino/analysis/` deeply, then use `.archive/libs/rhino/extraction/`, `intersection/`, `topology/`, and `spatial/` only for concepts relevant to analysis.
7. Research current Rhino 9/WIP primary docs before making Rhino-specific claims or edits.
8. Establish current baseline: LOC, public analysis surface, managed tests, Rhino runtime tests, and quality gates.

## Subagent Workflow

Use focused parallel subagents after initial orientation:

1. Archive analysis catalogue and deferred opportunities.
2. Modern analysis public API, ergonomics, compatibility, and downstream usage.
3. Modern analysis implementation density, casts, duplication, and `coding-csharp` fit.
4. Core context, validation, requirement, native operand, and result rails.
5. Rhino 9/WIP API audit from primary docs.
6. Tests, gates, runtime caveats, and behavior-preservation risks.

Each agent must return file references, evidence, proposed improvements ranked by value/risk, API impact, test impact, and functionality-preservation risk. Wait for all agents before finalizing an implementation plan.

## Discovery Focus

Build an old-vs-new map and classify each archive concept:

- `already modernized`
- `partially present`
- `missing and valuable now`
- `valuable but defer`
- `obsolete`
- `reject`

Pay special attention to:

- read-only topology diagnostics
- primitive/conformance vocabulary
- quality/fairness projections over existing summaries
- near-miss or stability checks
- extraction descriptors and feature detection
- do not reselect completed curvature or intersection classification work unless new evidence changes API ownership

For each `missing and valuable now` item, state the modern owner, best Rhino-native API, public API impact, LOC impact, tests required, behavior risk, and why it should be implemented now.

## Roadmap Rules

Use `handoff.md` as a living todo/source of truth:

- start from the roadmap ledger
- choose 1-3 `OPEN` or `PARTIAL` items for the pass
- mark chosen items `ACTIVE` in the plan
- implement only those selected items
- update the ledger before final response
- add newly discovered items only when evidence proves they belong

## Selection Rules

Select only 1-3 improvements per session. A selected improvement must:

- fit existing `analysis` or `core/Domain` ownership
- improve correctness, capability, or API coherence
- use Rhino-native APIs where available
- preserve typed ROP behavior
- be testable without broad new infrastructure
- avoid public API migration unless clearly necessary

Defer anything requiring a new subsystem, broad descriptor migration, helper-file proliferation, speculative scoring, topology healing, or separate single/batch APIs.

## Implementation Rules

- Prefer refining existing files.
- Add a new file only when existing ownership becomes less coherent.
- Add no helper/util/wrapper files.
- Add no knob-heavy options or config objects.
- Collapse repeated rails or traversal shapes only when doing so improves correctness and readability.
- Keep high-level capabilities as projections over lower-level query primitives.
- Preserve public source compatibility unless the user explicitly accepts a breaking change.

Before editing, explicitly check:

- null query and null geometry stay inside typed rails
- public API compatibility against baseline
- unsafe query/result retyping
- native tuple operand validation for `Plane`, `Line`, and other Rhino structs used by selected work
- Rhino partial-result semantics for relevant intersection APIs
- mesh tolerance policy for mesh-intersection-like operations
- Rhino subclass support
- managed tests do not require Rhino native runtime

## Validation

Run relevant gates:

- `bash -n scripts/check-cs.sh`
- `scripts/check-cs.sh --self-test`
- targeted managed `dotnet test` commands for impacted projects
- `pnpm check:cs`
- Rhino runtime tests when environment supports them

If Rhino runtime is blocked, report the exact blocker and which managed gates still passed.

## Deliverables

Return:

1. Archive concept catalogue.
2. Old-vs-new capability map.
3. Selected 1-3 improvements and why.
4. Deferred and rejected concepts with reasons.
5. Files changed and why each change belongs there.
6. Public API before vs after.
7. Baseline LOC vs final LOC for touched areas.
8. Rhino 9/WIP evidence with primary-source links.
9. Tests and gates run, including runtime caveats.
10. `handoff.md` roadmap updates made.
11. Remaining risks and next capability branch.
```

---
## [11][OPEN_DECISIONS]
>**Dictum:** *Future work should choose capability branch before editing.*

<br>

**Best immediate candidates:**

1. **Read-only topology diagnostics:** Add one focused query at a time after bounded result shapes are chosen for boundary loops, adjacency, and non-manifold evidence.
2. **Conformance vocabulary:** Start when primitive extraction, curvature summaries, and mass/measure can support typed residual projections without score-first APIs.
3. **Quality/fairness workflows:** Start only when they compose lower-level primitives without archive request/result objects.

**Deferred pick-up criteria:**

- **Curvature summaries:** Do not reselect unless public summary semantics change or runtime execution reveals a concrete defect.
- **Intersection classification:** Do not reselect unless a new native pair, result shape, or classification vocabulary is selected.
- **Read-only topology diagnostics:** Pick up when each diagnostic has a bounded typed result shape and tests can cover boundary, adjacency, and non-manifold cases without repair behavior.
- **Conformance vocabulary:** Pick up when primitive extraction and curvature summaries support typed residual projections; do not use archive `object? IdealPrimitive` or score-first APIs.
- **Quality/fairness workflows:** Pick up when they can be projections over primitives, curvature profiles, mesh diagnostics, and topology diagnostics.
- **Spatial indexing:** Pick up only on a separately approved collection-level branch with a focused `RTree` owner.
- **Feature/pattern detection and near-miss stability:** Pick up only after the lower-level extraction, intersection, and topology rails expose enough stable evidence to avoid broad workflow facades.

**Next recommended capability branch:** `analysis-topology-readonly`.

**Reject inside `analysis`:**

- Topology healing.
- Mutating repair workflows.
- Archive request/result/config architecture.
- Broad folder scaffolds.
- Wrapper modules and compatibility layers.
