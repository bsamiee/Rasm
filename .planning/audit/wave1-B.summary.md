# Wave 1 - Agent B - `partial class Query` Audit

## Architectural Shape (current state)

The `Analysis.Query` partial class is a **typed-aspect dispatcher** spanning seven files (3,243 LOC). Public surface is a constellation of generic entrypoints (`Bounds<>`, `Measure<>`, `Locate<>`, `Faces<>`, `Topology<>`, `Conformance<>`, `Deviation<>`, `Intersect<>`, `Vertices<>`, `Edges`, `Outlines`, `Iso`, `Primitive<>`, `Kind<>`, `SolidOrientation<>`, `IsPointInside<>`, `Components<>`, `MeshCheck`, `MeshCheckCount`, `MeshFaceMetric`, `SelfIntersections`, `Domain<>`, `Segments<>`, `EdgeMidpoints<>`, `NakedEdges<>`, `SpatialMidpoint<>`, `UniqueCorners`, `BoundingCorners<>`, `WorldCardinalPoints`, `Quadrants<>`, `FaceFrame<>`) that each dispatch via tuple-pattern switch on `(typeof(TGeometry), typeof(TOut), aspect.Case)`. Each public entrypoint drives down through 1–3 levels of private generic builders to a `Query<TGeometry,TOut>.Build` call that produces a `Query<,>` record carrying `(OperationKey, GeometryRequirement, RequiresContext, Fin<Unit> Ready, Func<TGeom,Eff<RT,Seq<TOut>>>)`. The host record at `Query.cs:17` is itself a 5-field hand-rolled reader-effect; runtime composition happens inside `Analyze.Program<TGeometry,TOut>` (`Analyze.cs:81`) with a divide-and-conquer span fold and a `RuntimeOrSentinel` fallback that synthesizes an uninitialized `AnalysisRuntime` via `RuntimeHelpers.GetUninitializedObject` when no document context is provided. Aspect unions (`Bounds`, `Measure`, `Location`, `Faces`) are Thinktecture `[Union]` records; `Topology`, `Conformance`, `Deviation` are `readonly record struct` discriminants that mirror enum cases. The whole algebra reads as one giant case-table with strong typing but considerable structural redundancy.

## Top-3 Collapse Candidates

| Rank | Cluster (finding) | Members | Est. LOC |
| ---- | ------------------- | -------- | --------- |
| 1 | **Aspect-dispatch unification** (`B-098`) — Bounds/Measure/Location/Faces/Topology/Conformance/Deviation/Intersect all repeat the same `(TGeom,TOut,Aspect)→Build` switch pattern | 8 entrypoints + supporting per-aspect builders | **-150** |
| 2 | **RepoQuery-style collapse of Vertices/EdgeMidpoints/DecomposeFaces** (`B-046`) — three near-identical `switch on TGeometry` projection dispatchers | `Vertices<>`, `EdgeMidpoints<>`, `DecomposeFaces<>` | **-65** |
| 3 | **CurvatureProfileReady fold** (`B-100`) — 7 arms × 12-15 LOC each for `(Curve|Surface) × (None|Magnitude|Gaussian|Mean)` × (Vector3d|Point3d|CurvatureProfile|double|SurfaceCurvature) | `CurvatureProfileReady`, `CurveScalarProfile`, `CurveCurvatures`, `SurfaceCurvatures`, `SurfaceScalars`, `SurfaceScalarProfile` | **-65** |

Honorable mentions: `B-053` (Intersect Pair/PairEvents/PairCurvePoint/PairPolylines collapse, **-50 LOC**), `B-061` (LengthMass/AreaMass/VolumeMass/Mass collapse, **-50 LOC**), `B-088` (MeshCheckCount enum→getter dispatch table, **-30 LOC**).

## Cross-Partial Duplication Map

| Helper / concept | Where defined | Where re-implemented | Verdict |
| ---------------------------- | -------------- | --------------------- | -------- |
| `CurveAtNormalizedValue` | `Query.cs:371` | `CurveEdgeMidpoint` (`Extract.cs:82`), `EdgeCurveMidpoints` fold (`Extract.cs:579`) | `B-036` collapse `CurveEdgeMidpoint` to call `CurveAtNormalized<>` directly |
| `CurveAtNormalized<>` | `Query.cs:383` | `Mid` (`Locate.cs:179`), `TangentAtMiddle` (`Locate.cs:209`) | KEEP — multi-call canonical primitive |
| Surface-shape classification (Plane/Sphere/Cylinder/Cone/Torus) | `KindOfBrep` (`Extract.cs:566`) | `Kind<>` Surface arm (`Extract.cs:251-258`) | `B-042`/`B-083` extract `SurfaceKind(Surface, context)` shared primitive |
| Brep-primitive guard (Sphere/Cylinder/Cone/Torus → reject) | `BrepEdgeMidpoints` (`Extract.cs:96-100`) | `BrepVertices` (`Extract.cs:349-353`) | `B-041` shared `GuardAgainstPrimitive` |
| BoundingBox extraction from polymorphic input | `BoundingBoxOf` (`Measure.cs:45`), `ExtractBounds` (`Measure.cs:555`) | Both extract from `GeometryBase\|Line\|Polyline\|BoundingBox\|Box\|Sphere` | `B-044`/`B-045`/`B-047` consolidate to one |
| `DecomposeFaces`/`SelectFaces` pipeline | `Extract.cs:611,628` | `Faces<>()` (`Extract.cs:594`), `FaceFrame<>()` (`Locate.cs:611`) | `B-031`/`B-032` shared `Faces<TGeom,TFaceOut>(aspect, project)` primitive |
| Stats fold (Count/Min/Max/Mean/Variance/Rms) | `Core.Runtime.FoldExtensions.StatsOf` | `Profile()` (`Locate.cs:419`), `ResidualProfileDistances` (`Measure.cs:466`), `ResidualRmsDistances` (`Measure.cs:444`), `ResidualWithinToleranceDistances` (`Measure.cs:455`) | `B-011`/`B-012`/`B-013`/`B-014` adopt existing `StatsOf` |
| `Choose`/`Dot` (extreme-along-direction) | `Locate.cs:62-86` | Single-cluster, but `Dot` reimplements `Vector3d operator*` | `B-020`/`B-021`/`B-022` use `Curve.ExtremeParameters` + `Vector3d operator*` |
| `Mass` (Optional→ToFin pattern) | `Query.cs:543` | Used 4× inside Query.cs Mass-builder closures | KEEP (helper retained) but `MassFault` errors should fold into `OperationFault` |

## What's Working (do not touch)

The `Analyze.From()` / `Analyze.In()` boundary is clean: it ingests a `RhinoDoc?` or `UnitSystem` and produces a `Scope` carrying a typed `AnalysisRuntime`; consumers thread the runtime through `Eff<AnalysisRuntime,T>.Asks` rather than reaching for `RhinoDoc.ActiveDoc`. The `OperationKey`-keyed error rail is consistent — every public entrypoint produces `Query<>.Reject(key, fault)` on type-mismatch and propagates the same `Unsupported`/`InvalidInput`/`InvalidResult`/`MissingContext` codes through `Core.Domain.OperationFault`. `Core.Runtime.RhinoMatch.RequireValid` provides exhaustive value-validity checks that downstream `One`/`Many` builders correctly invoke. Aspect unions are Thinktecture `[Union]` records (good — generated dispatch). `Validation<Error,T>` parallel-accumulates input errors at `Analyze.Run`, `Eff<RT,T>` carries effectful pipelines internally, and `Fin<T>` is the synchronous fallible rail at the leaf — no library-mixing within a file. Zero `var` keyword usage, zero `if`/`else`/`for`/`foreach`/`while` outside annotated boundary adapters, zero `try`/`catch`/`throw` in domain code, zero `.Result`/`.Wait()`. Named parameters are present at every domain call site. The discipline at the type level is exemplary; the redundancy is at the structural-pattern level.

## Findings Index

100 findings emitted. Counts by category: `helper-spam` 47 · `collapse-candidate` 22 · `surface` 11 · `local-reimplementation` 8 · `cross-partial-duplication` 7 · `loc-reduction` 3 · `cosmetic` 1 · `boundary-leak` 1 · `csp-violation` 0.

Total estimated LOC reduction across all findings (sum of negative `estimated_loc_delta`): **-1,221 LOC** (~38% of the 3,243-line algebra). Realized reduction will be lower because of `depends_on` overlap; conservative non-overlapping estimate is **-650 LOC**.
