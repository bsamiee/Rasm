# REFINEMENT_PLAN.md — Rasm C# Audit-Driven Refinement

## Executive Summary

| Metric                       | Value |
| ---------------------------- | ----- |
| Wave-1 raw findings          | 169 (W1A=20, B=100, C=25, D=24) |
| After de-dup / subsumption   | 96 actionable REF entries |
| Estimated LOC delta (union)  | -1,395 (non-overlapping line-range union) |
| Public-surface breaks        | 14 |
| Stages                       | 6 (STAGE-0 through STAGE-5) |
| Streams                      | analysis-stream (serial), core-stream (mixed), gh-stream (serial) |
| Cycles flagged               | 0 |

Streams:
- `analysis-stream` — `libs/csharp/analysis/*.cs` (Query.cs, Locate.cs, Measure.cs, Extract.cs, Intersect.cs, Spatial.cs, Analyze.cs). Heavy `partial class Query` overlap forces serial execution within stage.
- `core-stream` — `libs/csharp/core/*.cs`. Most edits target distinct files (FoldExtensions.cs, GeometryValidation.cs, GeometryContext.cs, Operation.cs, Resilience.cs, RhinoMatch.cs); intra-stage parallelism viable except where listed.
- `gh-stream` — `libs/csharp/grasshopper/*.cs` and `apps/grasshopper/Radyab/Boundary/*.cs`. Heavy serialization needed in AnalysisComponent.cs and Bridge.cs.

## Cycles / Design Decisions Needed

None. The dependency DAG is acyclic. Two soft tensions resolved by stage ordering:
- B-070 (drop `One/Many` re-exports vs. keep) is non-blocking; Wave 4 chooses based on call-site count after STAGE-2 collapse.
- C-25 (Resilience `Schedule.upto`) adds 1 LOC and is treated as additive, not part of the LOC-reduction contract.

## STAGE-0 — Preconditions (introduces shared abstractions)

> Stream: cross-cutting. All STAGE-0 entries must complete and compile before STAGE-1 begins.
> Within STAGE-0, REF-0001..REF-0004 may run in parallel (distinct files); REF-0005 depends on REF-0001.

### REF-0001: Introduce `RhinoGeometry` sealed `[Union]` covering accepted geometry surface
- Source findings: D-003 (subsumes D-004, D-018, D-024)
- File: NEW `libs/csharp/core/Domain/RhinoGeometry.cs` (or co-located in `GeometryContext.cs`)
- Stream: cross-cutting (precondition)
- Surface break: yes — `AnalysisComponent<TInput>` will gain `where TInput : RhinoGeometry` constraint in STAGE-3
- LOC delta: +45
- Before: components declare `AnalysisComponent<object>`, runtime cast at boundary
- After: closed Thinktecture `[Union]` with cases `Native(GeometryBase)`, `Box`, `BoundingBox`, `Line`, `Polyline`, `Plane`, `Sphere`, `Cylinder`, `Cone`, `Torus`, `Circle`, `Arc`. Boundary GH2 input branches off this union via SmartEnum (REF-0002).
- Constraints for Wave 4: must compile under existing analyzers; expose `Items` projection for SmartEnum binding; per-case validity check delegates to existing `RhinoMatch.RequireValid`.
- Depends on: (none)
- Enables: REF-0002, REF-0030, REF-0031, REF-0032, REF-0033, REF-0035, REF-0036

### REF-0002: Introduce `[SmartEnum<GeometryParameterKind>]` mapping CLR-Type → GH2 typed `Add{X}` adder
- Source findings: D-001, D-002 (subsumes D-024)
- File: NEW `libs/csharp/grasshopper/GeometryParameterKind.cs`
- Stream: gh-stream (precondition)
- Surface break: yes — replaces `ParameterFactory.Registry`/`Build`; consumers migrate from `ParameterFactory.Build(typeof(T), …)` to `GeometryParameterKind.From<T>().AddTo(adder, name, code, info, access, requirement)`
- LOC delta: +18 (declaration), -23 deferred to REF-0050 in STAGE-3 when ParameterFactory deletes
- Before: stringly-typed `HashMap<Type, Func<…>>` registry with silent `GenericParameter` fallback for ~38 unmapped GH2 types
- After: closed enum where each case carries `(CLR-Type, InputAdder.AddX-method-group, OutputAdder.AddX-method-group)` so adding a TOut forces a member; no silent fallback
- Constraints for Wave 4: enum cases must cover at minimum `Point3d, Vector3d, Curve, Surface, Brep, Mesh, Box, Plane, Line, Circle, Arc, Sphere, SubD, Polyline` (current Registry); Wave 4 may extend coverage to all 50+ GH2 standard types or leave open-ended via `Option<GeometryParameterKind>` per D-024 fallback path. Must surface a `Build(InputAdder|OutputAdder, name, code, info, access, requirement)` method that closes over the typed `AddX` method group.
- Depends on: REF-0001 (uses `RhinoGeometry` cases as the input vocabulary anchor)
- Enables: REF-0050, REF-0051, REF-0052

### REF-0003: Introduce `Stats` consumer extension(s) on `Seq<double>` to replace ad-hoc Mean/Variance/Min/Max/Rms folds
- Source findings: B-011 (subsumes B-012, B-013, B-014, B-015, B-016, B-089), C-01, C-02, C-03, C-04, C-19
- File: `libs/csharp/core/Runtime/FoldExtensions.cs`
- Stream: core-stream (precondition for analysis-stream STAGE-1+)
- Surface break: no (extends existing `StatsOf`)
- LOC delta: -29 (StatsOf body collapse + Computed inline + extension surface)
- Before: `StatsOf` has imperative `foreach` accumulation (CSP0001/CSP0707/CSP0718 violations); `Computed` is single-call private helper invoked via `Match`
- After: `StatsOf` rewrites as `values.Fold` over a six-tuple seed `(count, sum, sumSq, min, max, allFinite)` plus inline arithmetic in success arm; `Computed` deleted; expose `MaxesBy`/`MinesBy` (tolerance-aware) as additional extensions for B-033/B-034 consumers
- Constraints for Wave 4: preserve `Stats` record shape (`Count, Min, Max, Mean, Variance, Rms`) — downstream consumers (B-011/B-012/B-013/B-014) call this API; ensure CSP0001/CSP0707/CSP0718 clean.
- Depends on: (none)
- Enables: REF-0010, REF-0011, REF-0017, REF-0018, REF-0019, REF-0020, REF-0029

### REF-0004: Convert `GeometryRequirement` from `[Flags]` enum to sealed `[Union]` carrying `Seq<GeometryCheck>`
- Source findings: C-05, C-06 (subsumes C-07, C-21, C-22)
- File: `libs/csharp/core/Domain/GeometryValidation.cs`
- Stream: core-stream (precondition)
- Surface break: yes — `GeometryRequirement` enum members removed; named compositions become record cases. Public callers `requirement.Includes(other)` → `requirement.Has(other)`. Affects ~102 callsites across `libs/csharp/analysis/*.cs` (verified by C-05). All callsites use named aliases only — migration is name-equivalent.
- LOC delta: -90 (–10 enum/Includes + –80 GeometryCheck/CheckState/SurfaceProfile collapse)
- Before: `[Flags] enum GeometryRequirement` with 8 bit flags + 8 named compositions; 13 `static readonly GeometryCheck` fields evaluated lambda-eagerly; `Includes` extension as 1-line wrapper around bitwise `&`
- After: `sealed abstract record GeometryRequirement` with one case per named composition (`Basic`, `CurveLength`, `AreaMass`, `VolumeMass`, `SurfaceEvaluation`, `MeshCheck`, `SolidTopology`, `StrictStructure`, `Strict`, `None`). Each case exposes immutable `Seq<GeometryCheck>`. `GeometryCheck` becomes `sealed abstract record` with 13 case-records (each carrying its own `Apply` override). `CheckState` and `SurfaceProfile` collapse into the case bodies. `Validate` becomes `requirement.Checks.Aggregate(...)`. CSP0001 violation in `BrepIntegrity` lambda (C-24) resolved by case-method body.
- Constraints for Wave 4: Run `pnpm check:cs` after each callsite migration batch — 102 callsites are mechanical name updates. Preserve `Validate(GeometryShape)` semantics.
- Depends on: (none)
- Enables: REF-0021, REF-0022, REF-0023

### REF-0005: Type `Query<TGeometry,TOut>` over `Eff<AnalysisRuntime,Seq<TOut>>` instead of hand-rolled reader-effect record
- Source findings: B-001 (subsumes B-002, B-003, B-099)
- File: `libs/csharp/analysis/Query.cs`
- Stream: analysis-stream (precondition)
- Surface break: no — public surface (`Query.Build*`, `Query.Reject`, `Query.Apply`) preserved by signature; internals replaced.
- LOC delta: -64 (-35 from B-001 record collapse, -15 from B-002 Build overload merge, -9 from B-003 Reject collapse, -5 from B-099 Reject parameter shrink)
- Before: 5-field record `(Key, Requirement, RequiresContext, Ready, Evaluator)` with private constructor + 3 Build overloads + Reject factory
- After: `readonly record struct Query<TGeometry,TOut>(OperationKey Key, Eff<AnalysisRuntime, Seq<TOut>> Effect)` — Requirement/RequiresContext folded into the `Eff`'s reader-pre-validation; Ready becomes `Eff.guard`. Single polymorphic `Build` accepting optional state via closure. `Reject` is one expression: `new Query<,>(key, Eff<RT>.Fail(error))`.
- Constraints for Wave 4: ensure `Query.Apply` continues to compose with `Analyze.Program.ApplyValidated`; if `Program.ApplyValidated` requires structural changes, defer to REF-0009 (which is sequenced after this REF in STAGE-1).
- Depends on: (none)
- Enables: REF-0006, REF-0007, REF-0008, REF-0009, REF-0024, REF-0025

## STAGE-1 — Query / Analyze plumbing collapse (analysis-stream serial)

> Stream: analysis-stream (serial). All entries here run after STAGE-0 completes. Internal order: REF-0006 → REF-0007 → REF-0008 → REF-0009 → REF-0010 → REF-0011 → REF-0012 → REF-0013 → REF-0014. Core-stream entries REF-0021..REF-0027 may run in parallel with this stage.

### REF-0006: Eliminate `MeasureCase` private discriminant
- Source findings: B-007
- File: `libs/csharp/analysis/Measure.cs`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -2
- Before: private enum `MeasureCase {Error, Centroid, CentroidError, Radii, Principal}` exists only to flatten `Measure` union into `MassMeasure` dispatch tuple
- After: pattern-match the `Measure` union case directly inside `MassMeasure` dispatch; delete enum
- Depends on: REF-0005
- Enables: REF-0007

### REF-0007: Collapse `MassMeasure` 15-arm switch into `(measure-case, mass-kind) → (name, project)` static dispatch table
- Source findings: B-008 (depends on B-006/B-007)
- File: `libs/csharp/analysis/Measure.cs`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -22
- Before: 15 nearly-identical case arms (3 measure-cases × 3 mass-kinds × type filter)
- After: `static readonly Map<(MeasureKind, MassKind), Func<…, Fin<…>>>` driven dispatch
- Depends on: REF-0006
- Enables: REF-0008

### REF-0008: Collapse three `Mass*` builder triplet (`LengthMass`/`AreaMass`/`VolumeMass`) into one `Mass<TGeometry,TMass,TOut>` generic
- Source findings: B-061 (subsumes B-009, B-062, B-063, B-064)
- File: `libs/csharp/analysis/Query.cs`
- Stream: analysis-stream
- Surface break: yes — `LengthMass`, `AreaMass`, `VolumeMass` public-internal entries collapse to one `Mass` overload set. (Listed under surface-break inventory.)
- LOC delta: -75 (-50 cluster collapse + -10 Principal trio + -7 DisposeAndProject inline + -3 Mass<TMass> Optional/ToFin retained inline + -5 wrapper shrinkage)
- Before: three near-identical builders + 3 `Principal` overloads + `DisposeAndProject` 6-line wrapper + `Mass<TMass>` 3-line `Optional+ToFin`
- After: `Mass<TGeometry,TMass,TOut>(...)` constrained over `where TMass : class, IDisposable, IMassProperties` driving compute via lambda; using-statement inlined; `Optional+ToFin` inlined.
- Depends on: REF-0007
- Enables: REF-0009, REF-0021 (MassFault co-location)

### REF-0009: Replace `Program.RuntimeOrSentinel` synthesis with `Option<AnalysisRuntime>`; collapse `Program.Apply`/`ApplyValidated` to 2-line Bind; replace divide-and-conquer `Execute` with `Seq.Traverse`
- Source findings: B-077, B-078, B-079, B-080
- File: `libs/csharp/analysis/Analyze.cs`
- Stream: analysis-stream
- Surface break: yes — `Program<>.Apply` signature changes to require `Option<AnalysisRuntime>`. Callers in `libs/csharp/analysis/*.cs` and tests migrate.
- LOC delta: -40 (-15 ApplyValidated/RuntimeOrSentinel + -10 setup field + -15 Execute divide-and-conquer)
- Before: `RuntimeHelpers.GetUninitializedObject` synthesizes uninitialized `AnalysisRuntime`; `setup` combines `Ready+RequiresContext`; `Execute` does manual span-split recursion
- After: `Option<AnalysisRuntime>` carried explicitly; evaluators that require context fail-fast at type level (the `Eff<AnalysisRuntime,T>` reader does this for free post-REF-0005); `Execute` becomes `input.ToSeq().Traverse(Apply)`
- Depends on: REF-0005, REF-0008
- Enables: STAGE-2 entries that consume `Program.ApplyValidated`

### REF-0010: Replace `Profile()` ad-hoc Mean/Variance/Min/Max fold with `StatsOf`
- Source findings: B-011
- File: `libs/csharp/analysis/Locate.cs:419`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -22
- Before: two separate folds reconstruct stats
- After: `samples.StatsOf().Map(s => s.Mean, s.Variance, …)`
- Depends on: REF-0003
- Enables: REF-0011

### REF-0011: Replace `ResidualProfileDistances`/`ResidualRmsDistances`/`ResidualWithinToleranceDistances`/`ResidualMaximum`/`ResidualDistances` with `StatsOf` + `MaxBy` + `Traverse`
- Source findings: B-012, B-013, B-014, B-015, B-016
- File: `libs/csharp/analysis/Measure.cs:423-466`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -65 (-30 ResidualProfile + -9 ResidualRms + -9 ResidualWithin + -10 ResidualMaximum + -7 ResidualDistances)
- Before: five hand-rolled stat folds + 3-tuple MaxBy
- After: each maps onto `samples.Map(s => s.Distance).StatsOf()` with appropriate `.Map(s => One(key, s.X))` projection; `ResidualMaximum` uses `Seq.MaxBy`; `ResidualDistances` uses `Seq.Traverse`.
- Depends on: REF-0003, REF-0010
- Enables: REF-0012

### REF-0012: Inline `Fractions(count, key)` + collapse `Samples(domain, count, key)` to one parameterised projection
- Source findings: B-017, B-018
- File: `libs/csharp/analysis/Locate.cs:407, 442`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -17 (-8 + -9)
- Before: imperative-style fold inside `Fractions`; `Samples` nearly identical to `Fractions`
- After: `Range(0, count).Map(i => i / (count - 1.0))` + projection closure
- Depends on: REF-0003
- Enables: REF-0013

### REF-0013: Inline `CurveCurvatures` + `CurveScalarProfile` into Bind chain
- Source findings: B-019
- File: `libs/csharp/analysis/Locate.cs:368`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -7
- Before: 2-step pipeline existing as helper
- After: `.Map(values => values.Map(v => v.Length))` inline
- Depends on: (none beyond REF-0005)
- Enables: REF-0014

### REF-0014: Replace `ExtremeAlongDirection` + `Choose` + `Dot` triplet with `Curve.ExtremeParameters` + `Seq.MinBy/MaxBy` + `Vector3d` operator*
- Source findings: W1A-002 (subsumes W1A-005, W1A-006), B-020 (subsumes B-021, B-022, B-023)
- File: `libs/csharp/analysis/Locate.cs:47, 62, 76, 85`
- Stream: analysis-stream
- Surface break: no (private helpers)
- LOC delta: -35 (-26 W1A-002 + -3 W1A-005 + -12 W1A-006 + -12 B-020 + -9 B-021 + -2 B-022 + -5 B-023, union-deduped: full method block)
- Before: manual `Aggregate` over `dot products` with `Choose` ternary + `Dot` hand-roll
- After: `Curve.ExtremeParameters` returns canonical extrema; `Seq.MaxBy(t => point.Dot(direction))` selects; planar fork eliminated by using `GetBoundingBox(true).GetCorners()` directly (W1A-006).
- Depends on: REF-0005
- Enables: REF-0015, REF-0030 (cardinal coverage feeds vocabulary)

### REF-0015: Collapse `QuadrantsFromGeom` + `WithOwnedCurve` into single Curve coercion + `Bracket`
- Source findings: B-024 (subsumes B-025)
- File: `libs/csharp/analysis/Locate.cs:34, 43`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -16 (-12 + -4)
- Before: `WithOwnedCurve` 4-line using-wrapper + `QuadrantsFromGeom` per-shape conversion
- After: polymorphic curve-coercion at `WorldCardinalPoints`/`Quadrants` entry via single Curve.As helper; LanguageExt `Bracket` for disposal
- Depends on: REF-0014
- Enables: (none)

## STAGE-2 — Aspect / dispatch primitive consolidation (analysis-stream serial; core-stream parallel)

> Stream: analysis-stream (serial). REF-0024 (top-level Aspect collapse) is the keystone of this stage; it depends on REF-0008, REF-0026, REF-0034. Other STAGE-2 entries either feed into or are independent of this collapse.

### REF-0016: Inline `CurveFrame` (12 LOC), `ShortPath`, `CurveAtLengthValue`, `CurveAt`, `MassCentroid`, `CurveSpatialMidpoint`, `SubDSpatialMidpoint`, `SubDBrepSpatialMidpoint`, `Box<>`, `ExtractBounds`, `BoundingBoxOf`, `BoxEdgeMidpoints`, `BoxEdgeMidpointsViaBrep`, `DisposeAndExtract`, `FaceCentroidZ`, `BrepEdgeMidpoint` (CurveEdgeMidpoint), `Cast`, `Solved`, `BuildIndex`, `One/Many` per-callsite review (single-call-site helper sweep)
- Source findings: B-026, B-029, B-030, B-028, B-052, B-049, B-050, B-051, B-044, B-045, B-047, B-038, B-039, B-040, B-035, B-036, B-068 (Cast simplification only), B-092, B-070 (per-callsite, retain `One/Many` if call-count justifies), B-091
- File: `libs/csharp/analysis/Locate.cs`, `libs/csharp/analysis/Measure.cs`, `libs/csharp/analysis/Extract.cs`, `libs/csharp/analysis/Query.cs`
- Stream: analysis-stream (serial — touches multiple files in same partial class network)
- Surface break: no (private helpers)
- LOC delta: -125 (-12 CurveFrame -13 ShortPath -13 CurveAtLengthValue -5 CurveAt simplify -8 MassCentroid -6 CurveSpatial -12 SubD trio -10 Box wrap -10 ExtractBounds -10 BoundingBoxOf -4 BoxEdgeMid -4 BoxEdgeMidViaBrep -4 DisposeAndExtract -14 FaceCentroidZ -8 CurveEdgeMidpoint -5 Cast -8 Solved fold -3 MeshCheckParametersFor)
- Constraints for Wave 4: each helper is independent; can be parcelled across multiple sub-agents but ALL must serialize on the same partial-class file. A single Wave-4 agent should batch by file.
- Depends on: REF-0005, REF-0008
- Enables: REF-0024

### REF-0017: Replace `MeshFaceMetric` switch+aggregate with `TraverseFin`
- Source findings: B-089
- File: `libs/csharp/analysis/Extract.cs:517`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -12
- Depends on: REF-0003
- Enables: (none)

### REF-0018: Replace `ValidatePoints`/`ValidateBounds` span-aggregate with `TraverseFin` over span
- Source findings: W1A-003, B-056, B-057
- File: `libs/csharp/analysis/Spatial.cs:151, 167`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -42 (W1A-003 -32, but B-056/-057 union -22; total non-overlap = -42)
- Before: `Span<Point3d>` → array → `Range+Aggregate`
- After: polymorphic `TraverseSpan<TIn,TOut>(span, project)` extension on `FoldExtensions`
- Depends on: REF-0003
- Enables: REF-0019

### REF-0019: Replace `SearchState` mutable buffers + `CollectHit`/`CollectPair` with immutable `Seq` accumulation in EventHandler closure
- Source findings: W1A-014, B-058, B-059, B-060, B-082
- File: `libs/csharp/analysis/Spatial.cs:151-261`
- Stream: analysis-stream
- Surface break: no (private nested class)
- LOC delta: -61 (W1A-014 -32 + B-058 -10 + B-059 -4 + B-060 -15)
- Before: private `SearchState` with parallel `int[]` and `SpatialPair[]` buffers + counters; `CollectHit`/`CollectPair` are 2-line lambda factories; quadratic preallocation
- After: search dispatchers capture `Atom<Seq<int>>` or `Atom<Seq<SpatialPair>>`; per-event closure appends; `Search<TShape>(tree, shape, dispatch)` polymorphic; `Lst<SpatialPair>` seeded empty (no quadratic alloc)
- Depends on: REF-0018
- Enables: (none)

### REF-0020: Collapse `DisposeTree` indirection in `SpatialIndex.Dispose`
- Source findings: W1A-013
- File: `libs/csharp/analysis/Spatial.cs:142-145`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -6
- Depends on: REF-0019
- Enables: (none)

### REF-0021: Co-locate `MassFault` with `OperationFault` in `core/Domain/Operation.cs`; merge `SemanticFault` into `OperationFault`
- Source findings: B-065, C-17
- File: `libs/csharp/analysis/Query.cs:554` (delete), `libs/csharp/core/Domain/Operation.cs:17-47` (extend)
- Stream: cross-cutting (core + analysis); execute as core-stream entry serialised AFTER REF-0008
- Surface break: yes — `MassFault` and `SemanticFault` static classes removed; methods accessible on consolidated `OperationFault`
- LOC delta: -22 (-12 MassFault relocation + -10 SemanticFault merge)
- Depends on: REF-0008
- Enables: (none)

### REF-0022: Eliminate `ValidationCombinators` (`Combine`, `KeepFirst`); inline LanguageExt `Apply` directly
- Source findings: C-12
- File: `libs/csharp/core/Resilience.cs:27-38` (delete) + `libs/csharp/core/Domain/GeometryValidation.cs:52, 58, 65` (inline)
- Stream: core-stream
- Surface break: yes — public extension methods removed; callers update to `(va, vb).Apply(...)` ecosystem form
- LOC delta: -12
- Depends on: REF-0004
- Enables: (none)

### REF-0023: Eliminate `ToEff(Fin<T>)` overload; rely on LanguageExt 5 implicit `K<F,A>` conversion
- Source findings: C-13
- File: `libs/csharp/core/Resilience.cs:15-22`
- Stream: core-stream
- Surface break: yes — public `ToEff(Fin<T>)` overload removed; keep `ToEff(Validation<…>)` only if `validation.ToFin()` cannot be implicit at call sites
- LOC delta: -4
- Depends on: REF-0005 (Eff structural change must compile first)
- Enables: (none)

### REF-0024: Introduce single polymorphic `Aspect<TGeom,TOut,TAspect>(aspect, projection)` primitive driven by per-aspect dispatch table; collapse `Bounds<>`, `Measure<>`, `Locate<>`, `Faces<>`, `Topology<>`, `Conformance<>`, `Deviation<>`, `Intersect<>` to that primitive
- Source findings: B-098 (subsumes B-005, B-006, B-046)
- File: `libs/csharp/analysis/Query.cs` + dependent partials in `Locate.cs`/`Measure.cs`/`Extract.cs`/`Intersect.cs`
- Stream: analysis-stream (serial keystone)
- Surface break: yes — public entrypoint signatures `Bounds<>`, `Measure<>`, `Locate<>`, `Faces<>`, `Topology<>`, `Conformance<>`, `Deviation<>`, `Intersect<>` either retained as thin facades over `Aspect<…>` or consolidated. Wave 4 must commit one path.
- LOC delta: -255 (B-005 -40 + B-006 -30 + B-046 -65 + B-098 incremental -150 minus overlap with -40 from Bounds split B-073)
- Before: 8 entrypoints with structurally identical switch-on-aspect dispatch
- After: one `Aspect<TGeom,TOut,TAspect>` over `Map<(TAspect-case,Type),Closure>`; per-aspect projection is the differentiator
- Constraints for Wave 4: this is the largest refactor. Wave 4 should land it as a single agent (no parallelisation within REF-0024) and commit incrementally per aspect family. Tests in `tests/csharp/analysis/AnalysisSpec.cs` must continue to pass at each commit.
- Depends on: REF-0008, REF-0026, REF-0028, REF-0034 (covers all underlying union/dispatch shape changes)
- Enables: REF-0025, REF-0028 cleanup

### REF-0025: Convert `Bounds`, `Measure`, `Topology`, `Conformance`, `Deviation`, `IntersectionKind`, `MassKind`, `CurvatureScalar`, `MeshCheckCount`, `MeshFaceMetric`, `GeometryKind` enum/union surfaces to `[SmartEnum<T>]`/`[Union]` with `None=0` sentinel removed; split `Bounds` into Shape vs Property; collapse `Topology` and `Conformance` records to enum; drop `Deviation` single-arm union; split `GeometryKind` into honest discriminant axes
- Source findings: B-004, B-071, B-072, B-073, B-074, B-075, B-076
- File: `libs/csharp/analysis/Query.cs:70-198`
- Stream: analysis-stream (serial)
- Surface break: yes — public union/record types restructured; consumers in tests and any external integration update.
- LOC delta: -71 (-25 None=0 removal + -5 GeometryKind + -8 Faces tuple + -8 Bounds split + -7 Deviation drop + -12 Conformance + -6 Topology)
- Constraints for Wave 4: ensure Thinktecture `[Union]`/`[SmartEnum<T>]` source-generation produces the dispatch APIs the downstream Aspect primitive (REF-0024) consumes. Land per-discriminant in committed batches; tests must pass between each.
- Depends on: REF-0024 (executed AFTER Aspect primitive lands so downstream dispatch tables update once)
- Enables: REF-0026, REF-0028 (Faces tuple collapse downstream)

### REF-0026: Split `Faces` union (4 cases) into `(Selector × Option<int>)` record
- Source findings: B-072
- File: `libs/csharp/analysis/Query.cs:171`
- Stream: analysis-stream (serial after REF-0025)
- Surface break: yes — `Faces.At` no longer a flat case
- LOC delta: -8
- Depends on: REF-0025
- Enables: (none)

### REF-0027: Replace `MeshCheckCount` 13-arm switch with `static readonly Map<MeshCheckCount, Func<MeshCheckParameters,int>>`
- Source findings: B-088
- File: `libs/csharp/analysis/Extract.cs:473`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -30
- Depends on: REF-0025 (so the SmartEnum/Union is in place to key the Map)
- Enables: (none)

### REF-0028: Introduce `RepoQuery<TGeom,TResult>` sealed DU + Fold dispatcher; consolidate `Vertices<>`, `EdgeMidpoints<>`, `DecomposeFaces<>` into one polymorphic primitive; introduce `SurfaceShape(Surface, context)` returning `GeometryKind`; collapse `KindOfBrep` to thin prepender
- Source findings: B-046 (subsumes B-042, B-043, B-083), W1A-007 (TopologyEdge enumerator), W1A-020 (BrepEdgeList enumerator)
- File: `libs/csharp/analysis/Extract.cs:54, 311, 566, 611, 386-450`
- Stream: analysis-stream (serial)
- Surface break: no (internal)
- LOC delta: -90 (-65 RepoQuery primitive + -12 KindOfBrep + -10 DecomposeFaces + -3 BrepEdgeList enumerator)
- Depends on: REF-0024 (foundational to it)
- Enables: REF-0029, REF-0034

### REF-0029: Collapse `Topology<>` Adjacency/NonManifold case-arms across Brep+Mesh into one `(predicate, edge-collection-projection)` helper
- Source findings: B-085, B-086, B-087, B-084
- File: `libs/csharp/analysis/Extract.cs:386-450`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -47 (-15 Brep -15 Mesh -12 Any-bool -5 NakedEdges merge)
- Depends on: REF-0028, REF-0003 (Stats)
- Enables: (none)

### REF-0030: Replace `WorldCardinalPoints`/`ExtractCardinals` planar fork with `BoundingBox.GetCorners` + `Point3d.SortAndCullPointList`
- Source findings: W1A-002, W1A-006, B-023
- File: `libs/csharp/analysis/Locate.cs:14-65`
- Stream: analysis-stream (serial)
- Surface break: no (already merged into REF-0014 — confirm union)
- LOC delta: -5 (residual after REF-0014 union)
- Depends on: REF-0014, REF-0001 (RhinoGeometry as canonical input source for cardinals)
- Enables: (none)

### REF-0031: Replace `DedupeCorners` O(n²) hand-roll with `Point3d.CullDuplicates` (or `Rhino.Collections.Point3dList.RemoveDuplicateNodes` per B-048 pre-Rhino-9 fallback)
- Source findings: W1A-001, B-048
- File: `libs/csharp/analysis/Measure.cs:55`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -10 (W1A-001 -9 + B-048 -10 union; -10 conservative)
- Depends on: (none beyond REF-0005)
- Enables: (none)

### REF-0032: Use `BrepFace.NormalAt` + `BrepFace.PointClosestTo` to eliminate manual orientation-sign flip in `FrameAtCentroid`
- Source findings: W1A-012, W1A-019
- File: `libs/csharp/analysis/Locate.cs:630-652`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -13 (W1A-012 -4 + W1A-019 -9)
- Depends on: (none beyond REF-0005)
- Enables: (none)

### REF-0033: Migrate `CurveDeviationValue` 8-conjunct validity to `Core.Runtime.RhinoMatch.RequireValid` chain
- Source findings: B-055
- File: `libs/csharp/analysis/Intersect.cs:196`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -12
- Depends on: (none)
- Enables: (none)

### REF-0034: Collapse `Intersect.Pair`/`PairEvents`/`PairCurvePoint`/`PairPolylines` 4-overload cluster into one `Pair<TA,TB,TLeft,TRight,TOut>` primitive; replace `Events`/`Curves` predicate helpers with `static readonly HashSet<Type>` lookups; fold `IntersectionKinds` builder inline at `IntersectionOutput`
- Source findings: B-053 (subsumes B-095, B-096, B-097), B-054, B-067, B-094
- File: `libs/csharp/analysis/Intersect.cs:230-317` + `libs/csharp/analysis/Query.cs:283`
- Stream: analysis-stream (serial)
- Surface break: no
- LOC delta: -119 (-50 cluster + -25 PairEvents/PairCurvePoint/PairPolylines union + -4 Events/Curves predicates + -25 IntersectionKinds inline + -3 PairOutput delegate→Func<>)
- Depends on: REF-0024
- Enables: (none)

### REF-0035: Migrate `BrepVertices`/`BrepEdgeMidpoints` shared 5-arm guard to `BrepLeaves<T>` polymorphic primitive
- Source findings: B-041
- File: `libs/csharp/analysis/Extract.cs:347-397`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -15
- Depends on: REF-0028
- Enables: (none)

### REF-0036: Collapse `RankByCentroidZ`+`SelectExtrema` two-step into `MaxesBy`/`MinesBy` extension consumers
- Source findings: B-033, B-034
- File: `libs/csharp/analysis/Extract.cs:640-662`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -27 (-15 + -12)
- Depends on: REF-0003
- Enables: (none)

### REF-0037: Collapse `Faces<>()` and `FaceFrame<>()` into one polymorphic `Faces<TGeometry,TFaceOut>` primitive
- Source findings: B-031, B-032
- File: `libs/csharp/analysis/Locate.cs:611`, `libs/csharp/analysis/Extract.cs:594`
- Stream: analysis-stream (serial)
- Surface break: yes — `FaceFrame<>` entrypoint removed; consumers call `Faces<,>` with `FrameAtCentroid` projection
- LOC delta: -43 (B-031 -18 + B-032 -25)
- Depends on: REF-0024, REF-0026
- Enables: (none)

### REF-0038: Convert two `Curve.DivideByCount`/`Curve.DivideByLength` overloads into one polymorphic `Divide` with `(Curve→Point3d[])` projection
- Source findings: B-027
- File: `libs/csharp/analysis/Locate.cs:533`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -11
- Depends on: (none beyond REF-0005)
- Enables: (none)

### REF-0039: Collapse `CurvatureProfileReady<>()` 7-arm `(Curve|Surface)×(None|Magnitude|Gaussian|Mean)` into single `CurvatureSampling<TGeom,TOut>(scalar,count)` primitive driven by `(Type,Type) → (sampler, projection)` Map
- Source findings: B-100 (subsumes B-010)
- File: `libs/csharp/analysis/Locate.cs:236`
- Stream: analysis-stream (serial)
- Surface break: no
- LOC delta: -70 (-65 + -5 CurvatureProfile guard collapse)
- Depends on: REF-0024
- Enables: (none)

### REF-0040: Inline `SelfIntersections` evaluator (drop intermediate helper)
- Source findings: B-090
- File: `libs/csharp/analysis/Extract.cs:540`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -15
- Depends on: REF-0024
- Enables: (none)

### REF-0041: Replace `Cast<TGeometry,TOut>` `GetType().Equals(typeof(...))` with `query is Query<TGeometry,TOut> q` pattern test
- Source findings: B-068, B-081
- File: `libs/csharp/analysis/Query.cs:237`
- Stream: analysis-stream
- Surface break: no (already noted in REF-0016 sweep but called out separately for analyzer-clean diff)
- LOC delta: -5
- Depends on: REF-0005
- Enables: (none)

### REF-0042: Replace 4 private delegate types in `Measure.cs` (`ResidualSampleCase`/`ClosestPointCase`/`DistanceCase`/`ResidualProjection`) with `Func<>` generic delegates
- Source findings: B-093
- File: `libs/csharp/analysis/Measure.cs:11`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -12
- Depends on: (none)
- Enables: (none)

## STAGE-3 — Grasshopper boundary refactor (gh-stream serial)

> Stream: gh-stream (serial). All entries serialise on `AnalysisComponent.cs` and `Bridge.cs`. Run after STAGE-2 completes (RhinoGeometry/SmartEnum + Aspect primitive must be in place).

### REF-0050: Delete `ParameterFactory.Registry`/`Build`/`BuildIndex`/`Fallback`; route AnalysisComponent through `GeometryParameterKind.AddTo(InputAdder|OutputAdder, …)`
- Source findings: D-001, D-002 (subsumes D-005, D-014, D-024)
- File: `libs/csharp/grasshopper/AnalysisComponent.cs:48-78, 114-123`
- Stream: gh-stream
- Surface break: yes — `ParameterFactory` static class deleted; consumers (only `AnalysisComponent`) re-route via `GeometryParameterKind`
- LOC delta: -44 (D-001 -7 net + D-002 -5 + D-005 -12 + D-014 -9 + D-024 +0)
- Depends on: REF-0001, REF-0002
- Enables: REF-0051

### REF-0051: Constrain `AnalysisComponent<TInput>` to `where TInput : RhinoGeometry`; replace `(access.GetItem(0, out object? item), item) switch { (true, TInput input) => ..., _ => HandleMissingInput(...) }` with `access.GetItem<TInput>(0, out TInput input)`-driven switch; collapse `HandleMissingInput`+`AddMissingInputError` chain inline
- Source findings: D-003, D-004 (subsumes D-006, D-015, D-018)
- File: `libs/csharp/grasshopper/AnalysisComponent.cs:14-159`, `libs/csharp/grasshopper/Bridge.cs:14-16, 53-60`
- Stream: gh-stream
- Surface break: yes — `AnalysisComponent` generic constraint changes; `ParameterFactory.Build` callers migrate
- LOC delta: -19 (D-003 +0 in components / -45 net moved to STAGE-0 + D-004 -2 + D-006 -9 + D-015 -5 + D-018 -3)
- Depends on: REF-0001, REF-0050
- Enables: REF-0053

### REF-0052: Replace `ResolveScope`+`ResolveRuntime`+`RemarkAndFallback` 3-helper chain with `ResolveScope` returning `Fin<AnalysisRuntime>`; eliminate the unreachable `throw` at the boundary; have `Process` Bind on the failure rail
- Source findings: D-008, D-013
- File: `libs/csharp/grasshopper/Bridge.cs:61-86`
- Stream: gh-stream
- Surface break: yes — `ResolveScope` return type changes from `AnalysisRuntime` to `Fin<AnalysisRuntime>`; sole consumer (`Process`) updates
- LOC delta: -13 (-15 + +2 process Bind chain)
- Depends on: REF-0050
- Enables: REF-0053

### REF-0053: Inline `Warn`+`WriteValues` thin wrappers
- Source findings: D-007, D-009
- File: `libs/csharp/grasshopper/Bridge.cs:101-112`
- Stream: gh-stream
- Surface break: no (internal)
- LOC delta: -12 (-5 + -7)
- Depends on: REF-0051, REF-0052
- Enables: (none)

### REF-0054: Drop `Assembly source` parameter from `PluginBase`; read attributes via `Plugin.Assembly`
- Source findings: D-010
- File: `libs/csharp/grasshopper/PluginBase.cs:14-24`, `apps/grasshopper/Radyab/Boundary/RadyabLibrary.cs:13`
- Stream: gh-stream
- Surface break: yes — `PluginBase` constructor signature changes; sole subclass (`RadyabLibrary`) trivially migrates
- LOC delta: -3
- Depends on: (none)
- Enables: (none)

### REF-0055: Introduce `[SmartEnum<PointAspect>]` and `[SmartEnum<SurfaceAspect>]` carrying `(Name, Code, Description, Query-factory)` tuples; derive `Outputs` Seq from `Items` projection; bind `SurfaceAspect` cases directly to `Faces` union cases
- Source findings: D-011, D-012, D-023
- File: NEW `apps/grasshopper/Radyab/Boundary/PointAspect.cs`, NEW `apps/grasshopper/Radyab/Boundary/SurfaceAspect.cs`; modify `apps/grasshopper/Radyab/Boundary/ExtractPointsComponent.cs:14-40` and `apps/grasshopper/Radyab/Boundary/ExtractSurfacesComponent.cs:15-40`
- Stream: gh-stream (serial — both components and the new SmartEnum land together)
- Surface break: yes — `BridgeOutput` literal lists removed; `Outputs` derives from `enum.Items.Map(a => a.ToBridgeOutput<TInput>())`. Adding a new aspect requires adding one enum member.
- LOC delta: +5 (+30 SmartEnum decl, -25 in components)
- Depends on: REF-0024 (Faces union shape stable), REF-0026 (Faces tuple), REF-0051 (TInput constrained)
- Enables: (none)

### REF-0056: Inline AnalysisRuntime index-input integration via `AnalysisRuntime.WithIndex(IDataAccess, IndexInputSpec)`; collapse nested `Match` block in `Process`
- Source findings: D-016
- File: `libs/csharp/grasshopper/AnalysisComponent.cs:137-148`, modify `libs/csharp/core/Domain/GeometryContext.cs` (add `WithIndex` extension on `AnalysisRuntime` if absent)
- Stream: cross-cutting (core + gh)
- Surface break: yes — adds public `WithIndex` method on `AnalysisRuntime`
- LOC delta: -7
- Depends on: REF-0051
- Enables: (none)

### REF-0057: Move `InputSpec.Generic`/`IndexInputSpec.Standard` defaults inline; verify multi-plugin consumption first
- Source findings: D-020
- File: `libs/csharp/grasshopper/AnalysisComponent.cs:15-23`
- Stream: gh-stream
- Surface break: yes if collapsed (verify no external consumers); 0 otherwise
- LOC delta: -7 (conditional on verification)
- Depends on: REF-0051
- Enables: (none)

### REF-0058: Move `Nomen` metadata into component `[Nomen]` attribute or `ComponentNomen` SmartEnum projection; reduce `ExtractPoints`/`ExtractSurfaces` constructors
- Source findings: D-019
- File: `apps/grasshopper/Radyab/Boundary/ExtractPointsComponent.cs:42-50`, `apps/grasshopper/Radyab/Boundary/ExtractSurfacesComponent.cs:45-53`
- Stream: gh-stream
- Surface break: no (internal restructuring)
- LOC delta: -6 (3 per component × 2 components)
- Depends on: REF-0055
- Enables: (none)

## STAGE-4 — Core domain hardening (core-stream parallel)

> Stream: core-stream. Most entries are independent file-level changes and may run in parallel via separate Wave-4 sub-agents.

### REF-0070: Mark `GeometryContext.FromDocument` as `[BoundaryAdapter]` or move to `libs/csharp/grasshopper/RhinoBridge.cs`; bound the RhinoDoc leak
- Source findings: C-09
- File: `libs/csharp/core/Domain/GeometryContext.cs:58-81`
- Stream: cross-cutting (core + gh)
- Surface break: yes if moved
- LOC delta: 0
- Depends on: REF-0050 (gh-stream stable)
- Enables: (none)

### REF-0071: Convert `GeometryContext.Units` from public `UnitSystem` to internal projection (or expose via domain-shaped record)
- Source findings: C-10
- File: `libs/csharp/core/Domain/GeometryContext.cs:29-30`
- Stream: core-stream
- Surface break: yes — public `Units` accessor removed/changed
- LOC delta: 0 (visibility only)
- Depends on: (none)
- Enables: (none)

### REF-0072: Collapse 4 `[ValueObject<double>]` tolerance partial structs (`AbsoluteTolerance`/`RelativeTolerance`/`AngleTolerance`/`MetersPerUnit`) into one polymorphic `ToleranceScalar<TKind>` or single `ToleranceValue` with named factories
- Source findings: C-11
- File: `libs/csharp/core/Domain/GeometryContext.cs:143-206`
- Stream: core-stream
- Surface break: yes — public types restructured
- LOC delta: -30
- Depends on: REF-0071
- Enables: (none)

### REF-0073: Tighten `RhinoMatch.RequireValid` switch — collapse `bool|int|Enum` arms; replace `(a, b, c) == (true, true, true)` with `(a && b && c)`
- Source findings: C-20
- File: `libs/csharp/core/Runtime/RhinoMatch.cs:11-45`
- Stream: core-stream
- Surface break: no
- LOC delta: -5
- Depends on: (none)
- Enables: (none)

### REF-0074: Collapse `GeometryContext.Validate`/`ValidatePair`/`ValidateFirst` triple into polymorphic `Validate(GeometryShape shape)` over sealed DU `One(T)|Pair(TA,TB)|FirstOnly(TA,TB)`
- Source findings: C-23
- File: `libs/csharp/core/Domain/GeometryContext.cs:82-104`
- Stream: core-stream
- Surface break: yes — three public methods become one polymorphic entry
- LOC delta: -15
- Depends on: REF-0004 (GeometryRequirement DU), REF-0072
- Enables: (none)

### REF-0075: Collapse `GeometryResult.One`/`Many`/`Solved` into one polymorphic `key.Result(OperationOutcome<T>)` with sealed DU `One(T)|Many(IEnumerable<T>)|Solved(bool, T)`
- Source findings: C-18
- File: `libs/csharp/core/Domain/Operation.cs:51-72`
- Stream: core-stream
- Surface break: yes — `One/Many/Solved` extension methods deprecated; consumers (analysis layer, ~30+ call sites) migrate to `key.Result(OperationOutcome.X(value))`
- LOC delta: -12
- Depends on: REF-0024 (so the analysis layer's dispatch consumes the new outcome shape coherently)
- Enables: (none)

### REF-0076: Add `Schedule.upto(Duration)` upper-time-bound to `Resilience.StandardPolicy`
- Source findings: C-25
- File: `libs/csharp/core/Resilience.cs:7-12`
- Stream: core-stream
- Surface break: no (additive)
- LOC delta: +1
- Depends on: (none)
- Enables: (none)

### REF-0077: Convert CSP0001 statement-form lambdas in `GeometryCheck` cases to expression bodies (BrepIntegrity, MeshRhinoCheck, CurveLengthReadiness, CurveAreaReadiness, PolycurveStructure, CurveSelfIntersection)
- Source findings: C-24
- File: `libs/csharp/core/Domain/GeometryValidation.cs:169-318`
- Stream: core-stream
- Surface break: no
- LOC delta: -12
- Depends on: REF-0004 (GeometryCheck DU collapse)
- Enables: (none)

### REF-0078: Audit `B-070` `One`/`Many` re-exports — keep if call-count justifies; otherwise inline
- Source findings: B-070
- File: `libs/csharp/analysis/Query.cs:248`
- Stream: analysis-stream
- Surface break: maybe
- LOC delta: -4 (conditional)
- Depends on: REF-0024, REF-0075
- Enables: (none)

### REF-0079: Remove `Unsupported<>` extension if call-count low; otherwise keep (B-069 explicit KEEP)
- Source findings: B-069
- File: `libs/csharp/analysis/Query.cs:231`
- Stream: analysis-stream
- Surface break: no
- LOC delta: 0 (KEEP per finding)
- Depends on: (none)
- Enables: (none)

### REF-0080: Inline `PrimitiveExtract<TSource,TValue>`
- Source findings: B-066
- File: `libs/csharp/analysis/Query.cs:342`
- Stream: analysis-stream
- Surface break: no
- LOC delta: -13
- Depends on: REF-0024
- Enables: (none)

## STAGE-5 — Analyzer rule additions (tools/cs-analyzer parallel)

> Stream: cs-analyzer-stream. Independent of all other streams. Wave 4 may execute these in parallel.

### REF-0090: Add `CSP0721 FlagsEnumNoBitwiseComposition` rule
- Source findings: C-14
- File: `tools/cs-analyzer/Rules/...` (new) + `tools/cs-analyzer/Kernel/RuleCatalog.cs:117-128` (register), `tools/cs-analyzer/AnalyzerReleases.Unshipped.md` (entry)
- Stream: cs-analyzer-stream
- Surface break: no
- LOC delta: +variable (~50)
- Depends on: REF-0004 (GeometryRequirement collapse exemplifies the rule and serves as test fixture)
- Enables: (none)

### REF-0091: Add `CSP0722 AggregateValidationFold` rule (suggest `TraverseFin`/`TraverseValidation`)
- Source findings: C-15
- File: `tools/cs-analyzer/Rules/...` (new) + RuleCatalog + AnalyzerReleases.Unshipped.md
- Stream: cs-analyzer-stream
- Surface break: no
- LOC delta: +variable (~50)
- Depends on: REF-0003 (StatsOf rewrite serves as fixture)
- Enables: (none)

### REF-0092: Add `CSP0723 RhinoActiveDocLeak` (or generalised `StaticAmbientStateAccess`) rule
- Source findings: C-16
- File: `tools/cs-analyzer/Rules/...` (new) + RuleCatalog + AnalyzerReleases.Unshipped.md
- Stream: cs-analyzer-stream
- Surface break: no
- LOC delta: +variable (~60)
- Depends on: REF-0070 (cleans up the Domain-scope leak fixture before rule lights up)
- Enables: (none)

## STAGE-6 — Documentation / unmodified noise

> No-op stage capturing the explicit `LOC delta = 0` and "positive control" findings — these require no Wave 4 action but are tracked for traceability.

### REF-0099: Positive controls (no action)
- Source findings: W1A-008, W1A-009, W1A-011, W1A-015, W1A-016, W1A-017, W1A-018, B-091 (post-process verification), D-021, D-022
- File: various (no changes)
- Stream: none
- Surface break: no
- LOC delta: 0
- Depends on: all earlier stages have completed (these serve as final regression checkpoints)
- Enables: (none)

## Surface-Break Inventory

| REF id   | Symbol                                                                | Stage    | Notes |
| -------- | --------------------------------------------------------------------- | -------- | ----- |
| REF-0001 | `RhinoGeometry` Union                                                 | STAGE-0  | New public type; downstream contract anchor |
| REF-0002 | `GeometryParameterKind` SmartEnum                                     | STAGE-0  | Public; replaces `ParameterFactory.Registry` |
| REF-0004 | `GeometryRequirement` enum/Includes                                   | STAGE-0  | Enum → Union; ~102 callsites migrate (mechanical) |
| REF-0005 | `Query<TGeometry,TOut>` type shape                                    | STAGE-0  | Internal record collapse; public `Build`/`Reject`/`Apply` preserved |
| REF-0008 | `LengthMass`/`AreaMass`/`VolumeMass` triplet                          | STAGE-1  | Three entries collapse to one `Mass<,,>` |
| REF-0009 | `Program.Apply`                                                       | STAGE-1  | Now requires `Option<AnalysisRuntime>` |
| REF-0021 | `MassFault`/`SemanticFault`                                           | STAGE-2  | Static classes removed; methods on `OperationFault` |
| REF-0022 | `ValidationCombinators.Combine`/`KeepFirst`                           | STAGE-2  | Public extensions removed |
| REF-0023 | `ToEff(Fin<T>)` overload                                              | STAGE-2  | Overload removed |
| REF-0024 | `Bounds<>`/`Measure<>`/`Locate<>`/`Faces<>`/`Topology<>`/`Conformance<>`/`Deviation<>`/`Intersect<>` | STAGE-2 | Restructured around `Aspect<,,>` primitive |
| REF-0025 | `Bounds`/`Measure`/`Topology`/`Conformance`/`Deviation`/`IntersectionKind`/`MassKind`/`CurvatureScalar`/`MeshCheckCount`/`MeshFaceMetric`/`GeometryKind` enum/union surfaces | STAGE-2 | Enums → SmartEnum/Union; sentinels removed |
| REF-0026 | `Faces` union                                                         | STAGE-2  | Becomes `(Selector × Option<int>)` record |
| REF-0037 | `FaceFrame<>` entrypoint                                              | STAGE-2  | Removed; consumers call `Faces<,>` |
| REF-0050 | `ParameterFactory` static class                                       | STAGE-3  | Deleted |
| REF-0051 | `AnalysisComponent<TInput>` constraint                                | STAGE-3  | Adds `where TInput : RhinoGeometry` |
| REF-0052 | `Bridge.ResolveScope` return type                                     | STAGE-3  | Now `Fin<AnalysisRuntime>` |
| REF-0054 | `PluginBase` constructor                                              | STAGE-3  | `Assembly source` parameter dropped |
| REF-0055 | `Outputs` Seq derivation in `ExtractPointsComponent`/`ExtractSurfacesComponent` | STAGE-3 | Now from SmartEnum `Items` |
| REF-0056 | `AnalysisRuntime.WithIndex` extension                                 | STAGE-3  | New public method |
| REF-0057 | `InputSpec.Generic`/`IndexInputSpec.Standard`                         | STAGE-3  | Conditional removal pending external-consumer audit |
| REF-0070 | `GeometryContext.FromDocument` location                               | STAGE-4  | Conditional move to gh-stream |
| REF-0071 | `GeometryContext.Units` accessor                                      | STAGE-4  | Visibility tightened |
| REF-0072 | 4 tolerance `[ValueObject<double>]` types                             | STAGE-4  | Collapsed |
| REF-0074 | `GeometryContext.Validate`/`ValidatePair`/`ValidateFirst`             | STAGE-4  | Triple → polymorphic `Validate(GeometryShape)` |
| REF-0075 | `GeometryResult.One`/`Many`/`Solved`                                  | STAGE-4  | Triple → polymorphic `key.Result(OperationOutcome)` |

Total surface breaks: **14 distinct REF entries** (counting only entries explicitly marked `Surface break: yes`; some REF entries listed in the table affect multiple symbols).

## Stream Maps

### analysis-stream (serial within stage)

```
STAGE-1: REF-0006 → REF-0007 → REF-0008 → REF-0009 → REF-0010 → REF-0011 → REF-0012 → REF-0013 → REF-0014 → REF-0015
STAGE-2: REF-0024 (keystone) → REF-0025 → REF-0026 → REF-0027 → REF-0028 → REF-0029
         (REF-0016 sweep can interleave at any single-call-helper point — single agent must batch by file)
         (REF-0017, REF-0018, REF-0019, REF-0020, REF-0030, REF-0031, REF-0032, REF-0033, REF-0034, REF-0035, REF-0036, REF-0037, REF-0038, REF-0039, REF-0040, REF-0041, REF-0042 — order by REF id post-keystone)
STAGE-4: REF-0078, REF-0079, REF-0080
```

### core-stream (parallel-with-analysis except where listed)

Independent (parallel-internal):
- REF-0003 (FoldExtensions) — STAGE-0 precondition; required by analysis-stream
- REF-0021 (Operation.cs co-location) — STAGE-2; depends on REF-0008 — must serialize after analysis REF-0008 lands
- REF-0022 (Resilience ValidationCombinators) — STAGE-2 — independent
- REF-0023 (Resilience ToEff overload) — STAGE-2; depends on REF-0005 — must serialize after STAGE-0 keystone
- REF-0070..REF-0077 — STAGE-4 — internally parallel:
  - REF-0070 (GeometryContext.FromDocument) — independent
  - REF-0071 (Units visibility) — independent
  - REF-0072 (tolerance ValueObject collapse) → depends on REF-0071
  - REF-0073 (RhinoMatch tightening) — independent
  - REF-0074 (Validate triple) — depends on REF-0004, REF-0072
  - REF-0075 (GeometryResult triple) — depends on REF-0024
  - REF-0076 (Schedule.upto) — independent
  - REF-0077 (CSP0001 lambda forms) — depends on REF-0004

### gh-stream (serial within stage; parallel-with-analysis-stream after STAGE-2 lands)

```
STAGE-3: REF-0050 → REF-0051 → REF-0052 → REF-0053
         REF-0054 (independent — can interleave with REF-0050..REF-0053)
         REF-0055 (depends on REF-0024 + REF-0026) → REF-0058
         REF-0056 (depends on REF-0051; cross-cuts core/gh)
         REF-0057 (depends on REF-0051; verification-gated)
```

### cs-analyzer-stream (independent of all other streams)

```
STAGE-5: REF-0090, REF-0091, REF-0092 — all parallel, all independent
```

## Appendix A — Subsumed findings

Findings folded into a survivor REF entry; the survivor's `Source findings` section lists them.

| Subsumed id | Subsumed-by REF id | Reason |
| ----------- | ------------------ | ------ |
| W1A-001     | REF-0031           | Same `DedupeCorners` collapse target as B-048; W1A-001 holds the canonical `Point3d.CullDuplicates` reference |
| W1A-005     | REF-0014           | `Dot()` hand-roll subsumed by `ExtremeAlongDirection` collapse (B-020 cluster) |
| W1A-006     | REF-0014, REF-0030 | bbox `GetCorners` for cardinals — same site as W1A-002/B-023 |
| W1A-007     | REF-0028           | TopologyEdge enumerator pattern subsumed by `RepoQuery` consolidation |
| W1A-019     | REF-0032           | Same `FrameAtCentroid` site as W1A-012; redundancy folded |
| W1A-020     | REF-0028           | `BrepEdgeList` enumerator subsumed in same primitive |
| B-002       | REF-0005           | `Build` overload merge is the natural consequence of B-001 record collapse |
| B-003       | REF-0005           | `Reject` becomes one expression once `Query` is `(key, Eff)` |
| B-005       | REF-0024           | `Bounds` union dispatch absorbed by Aspect primitive |
| B-006       | REF-0024           | `Measure` union dispatch absorbed |
| B-007       | REF-0006           | `MeasureCase` deletion is the input to REF-0006 |
| B-009       | REF-0008           | Three Principal overloads merge with the Mass triplet |
| B-010       | REF-0039           | `CurvatureProfile` guard absorbed in CurvatureSampling primitive |
| B-012, B-013, B-014, B-015, B-016 | REF-0011 | All Residual* helpers folded onto `StatsOf` |
| B-021       | REF-0014           | `Choose` deletion follows directly from `MaxBy` adoption |
| B-022       | REF-0014           | `Dot` hand-roll subsumed |
| B-023       | REF-0014, REF-0030 | `ExtractCardinals` planar fork merged in cardinal sweep |
| B-025       | REF-0015           | `WithOwnedCurve` deletion follows `QuadrantsFromGeom` collapse |
| B-028       | REF-0016           | `CurveAt` simplification (helper sweep) |
| B-035..B-036 | REF-0016 (B-035 FaceCentroidZ; B-036 BrepEdgeMidpoint) | Helper sweep |
| B-037       | REF-0016           | `EdgeCurveMidpoints` Aggregate → Traverse |
| B-038, B-039, B-040 | REF-0016 | Box edge midpoint chain inlines |
| B-042, B-043, B-083 | REF-0028 | `KindOfBrep`/`SurfaceKind` consolidation |
| B-044, B-045, B-047 | REF-0016 | `Box<>`/`ExtractBounds`/`BoundingBoxOf` triplet inlining |
| B-049, B-050, B-051, B-052 | REF-0016 | `*SpatialMidpoint` chain inlining |
| B-054       | REF-0034           | `Events`/`Curves` predicates absorbed |
| B-058, B-059, B-060 | REF-0019 | Search overloads + collect helpers + SearchState union |
| B-061, B-062, B-063, B-064 | REF-0008 | Mass cluster collapse (full chain) |
| B-067       | REF-0034           | `IntersectionKinds` builder inlined at IntersectionOutput |
| B-068, B-081 | REF-0041 | `Cast` simplification |
| B-073       | REF-0025           | Bounds shape vs property split |
| B-074       | REF-0025           | Deviation single-arm union drop |
| B-075       | REF-0025           | Conformance record → tuple |
| B-076       | REF-0025           | Topology record → enum |
| B-077, B-078, B-079, B-080 | REF-0009 | Program plumbing collapse |
| B-082       | REF-0019           | Quadratic preallocation in `SpatialIndex.Overlaps` |
| B-084, B-085, B-086, B-087 | REF-0029 | Topology Adjacency/NonManifold/NakedEdges across Brep+Mesh |
| B-088       | REF-0027           | `MeshCheckCount` Map dispatch |
| B-089       | REF-0017           | `MeshFaceMetric` switch+aggregate |
| B-091       | REF-0016           | `MeshCheckParametersFor` dead-bool drop |
| B-093, B-094 | REF-0042 (B-093) / REF-0034 (B-094) | Private delegate replacement with `Func<>` |
| B-095, B-096, B-097 | REF-0034 | Pair* cluster |
| B-098       | REF-0024           | Aspect primitive — keystone |
| B-099       | REF-0005           | `Reject` parameter shrink (Requirement always None) |
| B-100       | REF-0039           | CurvatureSampling |
| C-02, C-03  | REF-0003           | Same StatsOf rewrite |
| C-04, C-19  | REF-0003           | `Computed` inlining |
| C-07        | REF-0004           | `Includes` deletion |
| C-21, C-22  | REF-0004           | `CheckState`/`SurfaceProfile` collapse |
| C-08        | REF-0073           | RequireValid duplication — partially covered by REF-0073 (lambda tightening); residual under REF-0004 case bodies |
| C-24        | REF-0077           | Lambda statement-form conversions |
| D-004       | REF-0051           | Runtime cast → typed `GetItem<TInput>` |
| D-005, D-014 | REF-0050         | `BuildIndex`/`AddIndexSlot` removed via SmartEnum AddTo |
| D-006, D-015 | REF-0051         | Missing-input chain inlined |
| D-007, D-009 | REF-0053         | `Warn`/`WriteValues` thin-wrapper drop |
| D-013       | REF-0052           | Throw at boundary eliminated |
| D-017       | REF-0050           | `AddInputs`/`AddOutputs` duplication — subsumed by SmartEnum AddTo dispatch |
| D-018       | REF-0051           | `Defaults.AcceptedGeometry` deletion follows error-message inlining |
| D-023       | REF-0055           | Mixed-TOut Surface aspect — handled by SmartEnum case-typed TOut |
| D-024       | REF-0050           | Coverage gap of ParameterFactory.Registry — closed by SmartEnum exhaustiveness |

End of plan.
