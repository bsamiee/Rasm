# `faults.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/faults.md`

Authority: current disk state; `CLAUDE.md`; monorepo, .NET, and `Rasm` planning law; the C# stack doctrine; the checked-in LanguageExt and Thinktecture API catalogs; `Domain/results.md`'s generated `[FaultCase]` substrate; and every direct producer or consumer named below. This is an audit only. No spec-sheet, API catalog, consumer, test, or validation surface was changed.

Apply the moves in order. Each code replacement is bounded to at most ten authored lines; repeated case and message rows are split by owner group.

## Corrected outcome

Delete all six target category rosters. Their cases become direct `GeometryFault` identities or, where the producer has violated a successful operation's result contract, the producer returns its resolved `Op.InvalidResult()` instead. `NativeEngine` and `EncodingStage` also disappear from their consumer pages.

The corrected union has **62 direct leaves**, compact ordinals **0 through 61**, and binds `FaultBand.Geometry = new(2350, 62, ...)`. Its interval is **[2350, 2412)**, disjoint from `Component` **[2300, 2341)** and `Appearance` **[2450, 2453)**.

Target nonblank authored C# lines: **155 -> 144, net -11**.

Target authored declarations: six roster types, thirty-nine roster fields, and `AbandonWitness.Done` disappear (**-46**); replacing 28 leaves with 62 adds 34 leaf declarations; net target surface **-12 declarations**. `NativeEngine` (type + row) and `EncodingStage` (type + seven rows) remove another ten declarations outside the target. Seven exhausted private helpers also disappear: `Nurbs.Construction`, `Curves.Fault`, `Surfaces.Fault`, `Subdivision.Fault`, `Development.Fault`, `Panelization.Fault`, and `Patterning.Fault`. Guaranteed campaign surface change: **-29 authored declarations**.

The 63-case draft fails at one precise boundary: `KindMismatch` has no domain producer, only six impossible closed-result fallbacks, so the final union has 62 leaves. A narrower 57-case draft also fails because it retains a cancellation category mirror, collapses three distinct chain failures into one, discards the substrate cause, uses nonexistent `Length`, `NameId`, `EncodingDType`, and `OffsetKind` payloads, and describes current nested records with obsolete `[UnionCase<...>]` syntax.

## Governing constraints

- `GeometryFault` stays the one direct `FaultBand.Geometry` family. Each leaf remains a nested sealed partial record with one explicit `[FaultCase]` ordinal.
- Thinktecture supplies the closed union and exhaustive generated `Switch`; LanguageExt supplies `Error`, `Fin`, `Validation<Error, T>`, `Option`, and cause aggregation.
- A direct case survives only where the caller can recover or diagnose differently from the evidence at that site. A successful operation returning the wrong result case is `key.InvalidResult()`, not a fabricated geometry fault.
- A contextual fault created from another `Error` preserves it through `ICausedFault`; no replacement may discard the source failure.
- No compatibility aliases, string dispatch, category mirrors, helper shells, or second identity registry are introduced.
- Recovery identity is unchanged: `SkeletonStalled`, `CollapseStalled`, `ConstraintUnrecoverable`, and `RemeshStalled` remain the only transient leaves; all replacement leaves inherit terminal posture exactly as their source cases did. The four direct cancellation leaves remain terminal like `RunAbandoned`; native/stage/engine mirror deletion changes identity granularity, not retriability. `ArrangementSubdivisionFailed` remains terminal and preserves its original error through `ICausedFault`, matching the prior `Error.Many` posture where the contextual terminal member forced the aggregate terminal.

## Ordered moves

### 1. Remove dead imports; add the only new carrier import

Location: `faults.md`, first C# fence, imports.

From:

```csharp
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Threading;
```

To:

```csharp
using Rhino.Geometry;
```

Keep `using System;`: the direct tessellation projection carries `Type` and the payload-limit message reads `Array.MaxLength`. Keep the existing package imports used by leaf payloads. The four deleted BCL namespaces have no use in the current fence and no use after the roster deletion.

Effect: target LOC **-3**.

### 2. Delete the false spatial mismatch and classify closed-result violations

Location: `faults.md`, `KindMismatch`; `Meshing/arrangement.md` `Field`; `Meshing/intersect.md` `Bvh`, `OverlapPairs`, and `Ray`; `Processing/remesh.md` `Source.Of` and `Project`.

From:

```csharp
[FaultCase(2)] public sealed partial record KindMismatch(SpatialKind Index, QueryKind Query) : GeometryFault;
```

To:

```csharp
```

Every one of the six direct constructions follows a resolved `SpatialOp` with a statically known result case. Replace each fallback with the operation key already in scope:

```csharp
Fin.Fail<T>(key.InvalidResult())
```

Remove `static` from the five `Bind` lambdas in `arrangement.md`, `intersect.md`, and `remesh.md` that must capture `key`; `Source.Of` already uses a capturing lambda. These are not mismatches between admissible spatial kinds: `Spatial.Apply(Build)` promises `SpatialAnswer.Index`, while each `SpatialOp.Query` arm constructs the one `QueryResult` case selected by its request. A different closed result is a broken successful operation result. No `KindMismatch` producer remains, so retaining the leaf would reserve a dead identity.

Effect: target LOC **-2** and target declarations **-1**. Consumer LOC is unchanged; six fabricated domain faults become the existing operation-result fault.

### 3. Correct topology absence and the impossible prior-table read

Location: `faults.md`, ordinal 5 leaf and message; `Spatial/reconciliation.md`; `Spatial/naming.md` `Survive` call chain.

From:

```csharp
[FaultCase(5)] public sealed partial record HashMismatch(TopoName Name, EntityKind Kind) : GeometryFault;
```

To:

```csharp
[FaultCase(7)] public sealed partial record TopologyContentMissing(TopoName Name, EntityKind Kind) : GeometryFault;
```

From:

```csharp
_ => Validation.Fail<Error, NameAddress>(new GeometryFault.HashMismatch(entry.Name, entry.Kind)),
```

To:

```csharp
_ => Validation.Fail<Error, NameAddress>(new GeometryFault.TopologyContentMissing(entry.Name, entry.Kind)),
```

`Addresses` computes the prior entry's canonical content key and tests membership in the rebuilt live set. It has no competing hash value; “mismatch” invents evidence. The real outcome is missing topology content, with the existing `TopoName` and `EntityKind` retained.

Separately, `NameTable.Resolve` returning a name whose `Entries` row is absent is an impossible result from an admitted table. Thread the existing `Op key` through `Step` and `Survive` and replace:

```csharp
prior.Entries.Find(name)
    .ToFin(new GeometryFault.NameCollision(name, entity.Kind))
```

with:

```csharp
prior.Entries.Find(name).ToFin(key.InvalidResult())
```

That branch is neither a collision nor missing rebuilt content; it is a broken successful table result.

Keep ordinal 6 `NameCollision`: `Step` has a separate, real constructor when two entities claim the same admitted name. After this replacement that duplicate-claim branch is the sole `NameCollision` producer.

### 4. Delete `AbandonWitness`; make cancellation identities direct

Location: `faults.md`, `AbandonWitness` and `RunAbandoned`; `Meshing/arrangement.md` `Operand`, `Opened`, `Cancelled`, managed checkpoints, and native status arm; `Rasm/RULINGS.md` abandonment row.

From:

```csharp
[FaultCase(3)] public sealed partial record RunAbandoned(
    Kind Kind, UnitInterval Progress, AbandonWitness Witness) : GeometryFault;
```

To:

```csharp
[FaultCase(2)] public sealed partial record SubdivisionCancelled(int Operand, UnitInterval Progress) : GeometryFault;
[FaultCase(3)] public sealed partial record ClassificationCancelled(UnitInterval Progress) : GeometryFault;
[FaultCase(4)] public sealed partial record WeldCancelled(UnitInterval Progress) : GeometryFault;
[FaultCase(5)] public sealed partial record NativeBooleanCancelled(UnitInterval Progress) : GeometryFault;
```

Delete `AbandonWitness`, its five rows, and `Done`. Its canonical managed progress duplicates the `RunAbandoned.Progress` payload, while `NativeCancelled.Done = None` proves the roster cannot own the actual native progress.

Replace `Operand.Stage` with the fact its row actually contributes:

```csharp
public static readonly Operand A = new(key: 0, progress: UnitInterval.Create(value: 0.00), static cut => cut.FaceB);
public static readonly Operand B = new(key: 1, progress: UnitInterval.Create(value: 0.25), static cut => cut.FaceA);
public UnitInterval Progress { get; }
```

Reshape the two governance helpers around the already-created direct fault:

```csharp
static Option<Error> Opened(GeometryFault fault, UnitInterval progress, ArrangementPolicy policy) {
    policy.Progress.Iter(sink => sink.Report(progress.Value));
    return Cancelled(fault, policy);
}
static Option<Error> Cancelled(GeometryFault fault, ArrangementPolicy policy) =>
    policy.Cancel.IsCancellationRequested ? Some<Error>(fault) : None;
```

At subdivision, construct `SubdivisionCancelled(side.Key, side.Progress)` once and reuse it for the opening and per-face token reads. Classification and weld construct their direct cases with `0.50` and `0.75`. The native status arm keeps its measured, clamped progress and constructs `NativeBooleanCancelled`.

Proof: these are four distinct cancellation sites and diagnostics; none needs a stage category after its case is the identity. The always-`Kind.Mesh` payload and five string keys disappear. Update the settled ruling to name the four direct cancellation leaves and remove the obsolete roster claim.

### 5. Replace arrangement categories without losing the source fault

Location: `faults.md`, `DegenerateArrangement`; `Meshing/arrangement.md` volumetric route, `Arrange`, `FaceBuild`, `Field`, and native status arms.

From:

```csharp
[FaultCase(12)] public sealed partial record DegenerateArrangement(
    int CellCount, ArrangementWitness ManifoldWitness, Option<int> Native = default) : GeometryFault;
```

To:

```csharp
[FaultCase(14)] public sealed partial record CellComplexScaleExceeded(long Faces, Dimension Ceiling) : GeometryFault;
[FaultCase(15)] public sealed partial record ArrangementSubdivisionFailed(int Operand, int Face, Error Cause) : GeometryFault, ICausedFault;
[FaultCase(16)] public sealed partial record NativeOperandRejected(int Operand, int Status) : GeometryFault;
[FaultCase(17)] public sealed partial record NativeBooleanFailed(int Status) : GeometryFault;
```

Exact producer mapping:

- widen `Gate`'s success to `(BooleanRoute Route, long Faces)` so its already-computed face census reaches `Volumetric`; the over-ceiling `CellComplex` route constructs `CellComplexScaleExceeded(gate.Faces, policy.ScaleCeiling)` instead of recomputing the census or discarding `Dimension` through `.Value`;
- `FaceBuild` substrate `MapFail`: `ArrangementSubdivisionFailed(side.Key, faceId, fail)`; the operand is required because both soups number faces from zero, while the positional `Cause` satisfies `ICausedFault` and `Fault.Inner` preserves the original error;
- native raise failure: `NativeOperandRejected(i, raisedStatus)`;
- native non-cancellation status: `NativeBooleanFailed(status)`.

Three arrangement branches are impossible successful results:

```csharp
result is IntersectResult.Chains chains ? Fin.Succ(chains.Table) : Fin.Fail<CrossTable>(key.InvalidResult())
```

```csharp
answer is SpatialAnswer.Index built ? ... : Fin.Fail<SpatialAnswer>(key.InvalidResult())
```

The first replacement also removes the current unresolved `ArrangementWitness.TableUnavailable` spelling; the roster declares `LatticeUnavailable`, and neither label should survive because the condition is a wrong result case. Delete `ArrangementWitness` and its six rows. A two-field `ArrangementSubdivisionFailed(int Face, Error Cause)` is also rejected because face ordinals collide across the two operand soups.

### 6. Type the constraint budget and replace tessellation categories

Location: `faults.md`, `ConstraintUnrecoverable`, `DegenerateTessellation`; every corresponding site in `Meshing/delaunay.md`.

From:

```csharp
[FaultCase(13)] public sealed partial record ConstraintUnrecoverable(int Constraint, int Budget) : GeometryFault;
```

To:

```csharp
[FaultCase(18)] public sealed partial record ConstraintUnrecoverable(int Constraint, Dimension Budget) : GeometryFault {
    public override Retriability Retriability => Retriability.Transient;
}
```

Pass `Policy.MaxRecoverySteiner` or `Policy.MaxFlipPasses` directly at all six sites; remove `.Value` only from the fault argument, not from loop arithmetic.

Replace the category-bearing leaf in four bounded blocks:

```csharp
[FaultCase(19)] public sealed partial record WalkExitedHull(int Simplex) : GeometryFault;
[FaultCase(20)] public sealed partial record WalkLimitReached(int Simplex, int Limit) : GeometryFault;
[FaultCase(21)] public sealed partial record EmptyCavity(int Simplex) : GeometryFault;
[FaultCase(22)] public sealed partial record CoplanarInsertionUnsupported(int Simplex) : GeometryFault;
```

```csharp
[FaultCase(23)] public sealed partial record ConstraintCrossingMissing(int A, int B) : GeometryFault;
[FaultCase(24)] public sealed partial record BlockingFaceMissing(int A, int B) : GeometryFault;
[FaultCase(25)] public sealed partial record FlipLimitReached(int Edges, Dimension Limit) : GeometryFault;
```

```csharp
[FaultCase(26)] public sealed partial record BisectorUndefined(int Site) : GeometryFault;
[FaultCase(27)] public sealed partial record VertexUnrepresentable(int Vertex) : GeometryFault;
[FaultCase(28)] public sealed partial record UnsupportedTessellationProjection(TessellationKind Kind, Type Output) : GeometryFault;
```

```csharp
[FaultCase(29)] public sealed partial record DualRequiresExplicitVertex(int Simplex) : GeometryFault;
[FaultCase(30)] public sealed partial record CollinearTriangle(int Simplex) : GeometryFault;
[FaultCase(31)] public sealed partial record CircumcenterInvalid(int Simplex) : GeometryFault;
```

Exact evidence corrections:

- `WalkLimitReached(current, Store.SimplexCount)` carries the bound that was spent.
- `ConstraintCrossingMissing(a, b)` and `BlockingFaceMissing(a, b)` carry the constrained endpoints, not unrelated `Store.LastLive()` state.
- `FlipLimitReached(settled.BudgetExhaustedEdges, Policy.MaxFlipsPerEdge)` carries count and typed policy limit.
- `VertexUnrepresentable` in `Triangles` receives `vs[i]`, not surrounding simplex `s`.
- the four wrong-kind projection calls retain a domain refusal, because a caller invoked a projection unsupported by the live `TessellationKind`; pass `Kind` plus `typeof(ValueTuple<Arr<Point3d>, Arr<(int, int, int)>>)`, `typeof(DualGraph)`, `typeof(Arr<BoundedCell>)`, or `typeof(MeshSpace)` respectively.
- remaining rows pass the existing simplex/site value at the condition that failed.

Delete `TessellationWitness` and all thirteen rows. These are real geometry outcomes, not impossible completed results; converting them to `InvalidResult` would erase actionable geometric evidence.

### 7. Delete the one-row native-engine mirror

Location: `faults.md`, `NativeAssetMissing`; `Meshing/arrangement.md`, `NativeEngine` and `Gate`.

From:

```csharp
[FaultCase(15)] public sealed partial record NativeAssetMissing(
    NativeEngine Engine, string Rid, long Ceiling) : GeometryFault;
```

To:

```csharp
[FaultCase(32)] public sealed partial record ManifoldLibraryUnavailable(
    string RuntimeIdentifier, long Faces, Dimension ManagedCeiling) : GeometryFault;
```

Delete `NativeEngine` and `ManifoldC`. `Gate` already has `faces`; construct `ManifoldLibraryUnavailable(RuntimeInformation.RuntimeIdentifier, faces, policy.ScaleCeiling)`. The dependency is fixed by the only native lane; a one-row enum contributes neither choice nor behavior, and the measured census plus typed managed ceiling must not collapse into one ambiguous `long`.

### 8. Separate chain failures; refuse impossible intersection and section results

Location: `faults.md`, `IntersectionFault` and `SectionFault`; `Meshing/intersect.md` `ChainWalk`; `Meshing/slice.md` `Fold`, `Nest`, `SliceStack.Of`.

From:

```csharp
[FaultCase(16)] public sealed partial record IntersectionFault(
    PrimitiveKind A, PrimitiveKind B, Option<int> Junction = default) : GeometryFault;
```

To:

```csharp
[FaultCase(33)] public sealed partial record NonManifoldIntersection(PrimitiveKind A, PrimitiveKind B, int Junction) : GeometryFault;
[FaultCase(34)] public sealed partial record MissingIntersectionVertex(PrimitiveKind A, PrimitiveKind B, int Vertex) : GeometryFault;
[FaultCase(35)] public sealed partial record IncompleteIntersectionWalk(PrimitiveKind A, PrimitiveKind B, int From, int To) : GeometryFault;
```

Map the degree breach directly. Change `Corners` from `Option<Polyline>` to `Fin<Polyline>`, pass `a`/`b`, and construct `MissingIntersectionVertex(a, b, slot)` at the exact failed `corner(slot)` lookup; `run[0].Source` is not proof of which vertex was absent. On incomplete coverage, destructure `graph.Edges.First()` and pass both endpoints to `IncompleteIntersectionWalk(a, b, edge.Source, edge.Target)`. Preserve `PrimitiveKind`; replacing it with `int A, int B` would erase the domain type.

From:

```csharp
[FaultCase(17)] public sealed partial record SectionFault(int Layer, double Elevation, int OpenChains) : GeometryFault;
```

To:

```csharp
[FaultCase(36)] public sealed partial record OpenSection(int Layer, double Elevation, int Chains) : GeometryFault;
[FaultCase(37)] public sealed partial record InvalidSectionNesting(int Layer, double Elevation, int Contours) : GeometryFault;
```

`SealPosture.Admit` maps open chains to `OpenSection`; `Nest` maps containment contradiction/cycle/multi-parent to `InvalidSectionNesting(layer, elevation, n)`. Keep `double Elevation`: there is no `Length` carrier in this namespace or current call shape.

Two branches are operation-contract violations:

- `Intersection.Apply` returning a non-`Chains` result for `PlaneMesh` -> `key.InvalidResult()`;
- `SliceStack.Of` finding a cycle after `Nest` proved the forest -> `key.InvalidResult()`.

### 9. Rename single-outcome leaves and remove sentinel evidence

Location: `faults.md`; `Solving/fit.md`, `Drawing/view.md`, `Drawing/hatch.md`, and `Processing/decimate.md`.

From:

```csharp
[FaultCase(0)] public sealed partial record DegenerateInput(Kind Kind, Option<int> Index, string Witness) : GeometryFault;
[FaultCase(18)] public sealed partial record FitFault(UnitInterval Inliers, UnitInterval Floor) : GeometryFault;
[FaultCase(20)] public sealed partial record ProjectionFault(EdgeKind Kind, int Segment) : GeometryFault;
[FaultCase(21)] public sealed partial record HatchFault(HatchPattern Pattern, int Region, string Witness) : GeometryFault;
[FaultCase(22)] public sealed partial record DecimationFault(int FaceBudget, int Achieved) : GeometryFault;
```

To, without changing the admitted payload types that remain:

```csharp
[FaultCase(0)] public sealed partial record DegenerateInput(Kind Kind, Option<int> Index, string Detail) : GeometryFault;
[FaultCase(38)] public sealed partial record InsufficientInliers(UnitInterval Inliers, UnitInterval Floor) : GeometryFault;
[FaultCase(43)] public sealed partial record EmptyProjection : GeometryFault { }
[FaultCase(44)] public sealed partial record HatchFailed(HatchPattern Pattern, int Region, string Detail) : GeometryFault;
[FaultCase(45)] public sealed partial record FaceBudgetMissed(int FaceBudget, int Achieved) : GeometryFault;
```

Producer mapping:

- rename `DegenerateInput.Witness` to `Detail`; every current construction is positional and the only property read is the target message, so no consumer call shape changes. The string is presentation detail, not an identity witness.
- `FitFault` -> `InsufficientInliers`; do not replace the two validated fractions with `int` or `Tolerance`.
- no drawable edges -> stateless `EmptyProjection`; delete fake `EdgeKind.Silhouette` and `-1`.
- `HatchFault` -> `HatchFailed`, positional `Witness` -> `Detail`; all four sites already carry the same pattern, region, and diagnostic shape.
- actual decimation budget miss -> `FaceBudgetMissed(budget, store.Live)`.
- section drawing receiving the wrong intersection result -> `key.InvalidResult()`.
- Hausdorff sampling with `filled == 0` -> `key.InvalidResult()`; `DecimationFault(0, lod.FaceCount)` fabricates a budget that was never tested.

Keep ordinal 46 `RemeshStalled` and its transient override unchanged apart from compaction.

At `Solving/fit.md` `MinimalSphere`, delete the contextual aggregate relabel:

```csharp
.MapFail(cause => new GeometryFault.DegenerateInput(Kind.Sphere, draw[0], "coplanar-sample") + cause)
```

The composed matrix solve already returns the exact typed solve failure and the fitting page's own boundary requires composed faults to surface unchanged. Deleting the row preserves the original error and retriability directly; wrapping it in an additional terminal `DegenerateInput` forces an `Error.Many` terminal posture and duplicates the solve identity. No new caused leaf or helper replaces it.

### 10. Split parameterization failures by actual outcome

Location: `faults.md`, `ParameterizationFault`; `Processing/flatten.md` five producers.

From:

```csharp
[FaultCase(19)] public sealed partial record ParameterizationFault(
    Option<ChartId> Chart, double Distortion) : GeometryFault;
```

To:

```csharp
[FaultCase(39)] public sealed partial record InvalidChartBoundary(int Loops, Option<int> Vertices) : GeometryFault;
[FaultCase(40)] public sealed partial record IncompleteParameterizationSpectrum(int Expected, int Actual) : GeometryFault;
[FaultCase(41)] public sealed partial record ParameterizationUnconverged(Option<double> Residual, int Iterations) : GeometryFault;
[FaultCase(42)] public sealed partial record FlippedChart(ChartId Chart, double MaxConformal) : GeometryFault;
```

Exact mapping:

- `FlattenLscm` with no loops -> `InvalidChartBoundary(0, None)`; `MeshDec.Disk` supplies `Loops.Length` and `Some(Loops[0].Length)` only when exactly one loop exists.
- insufficient eigenpairs -> `IncompleteParameterizationSpectrum(GaugeModes + 1, pairs.Count)`.
- spent ARAP fold -> preserve `state.Residual` and `state.Iterations`; do not coerce absence to `0.0`.
- flipped output -> `FlippedChart(ChartId.Create(store.Chart[flipped]), distortion.MaxConformal)`; `MinimumDeterminant` would name evidence the producer does not calculate.

### 11. Delete `EncodingStage`; derive redundant dtype facts from the channel

Location: `faults.md`, `EncodingFault`; `Drawing/pack.md`, `EncodingStage` and ten fault constructions.

From:

```csharp
[FaultCase(24)] public sealed partial record EncodingFault(
    EncodingChannel Channel, ChannelDtype Dtype, EncodingStage Stage,
    Option<double> Expected = default, Option<double> Actual = default) : GeometryFault;
```

To:

```csharp
[FaultCase(47)] public sealed partial record ChannelWidthMismatch(EncodingChannel Channel, int Actual) : GeometryFault;
[FaultCase(48)] public sealed partial record DuplicateEncodingChannel(EncodingChannel Channel) : GeometryFault;
[FaultCase(49)] public sealed partial record ChannelArityMismatch(EncodingChannel Channel, long Expected, long Actual) : GeometryFault;
```

```csharp
[FaultCase(50)] public sealed partial record UnboundEncodingChannel(EncodingChannel Channel) : GeometryFault;
[FaultCase(51)] public sealed partial record EncodingPayloadTooLarge(long Bytes) : GeometryFault;
[FaultCase(52)] public sealed partial record EncodingRoundTripExceeded(EncodingChannel Channel, double Error) : GeometryFault;
[FaultCase(53)] public sealed partial record MissingEncodingChannel(EncodingChannel Channel) : GeometryFault;
```

Delete `EncodingStage` and its seven rows. Map the ten constructors directly: one width, one duplicate, three arity, one unbound, one extent, one round-trip, and two missing-channel sites.

For arity, compare `(long)count * channel.Arity` against `raw.LongLength`/`block.Count` and pass the same `long` facts, preventing the existing `int` multiplication from overflowing before the error path. In `EncodedStore.Reserve`, likewise replace the byte count:

```csharp
new(count, new byte[channels.Fold(0, static (acc, c) => acc + (count * c.Arity * c.Dtype.Width))],
    new EncodingChannelDescriptor[channels.Count]);
```

with:

```csharp
new(count, new byte[checked((int)channels.Fold(0L,
        (extent, channel) => extent + ((long)count * channel.Arity * channel.Dtype.Width)))],
    new EncodingChannelDescriptor[channels.Count]);
```

`Extent` has already proved that sum does not exceed `Array.MaxLength`; the long fold removes overflow, the checked narrowing prevents the allocator from observing a wrapped intermediate, and removing `static` repairs the current illegal capture of `count`. `ChannelWidthMismatch` derives expected width from `Channel.Dtype.Width`; round-trip derives the tolerance from `Channel.Dtype.Tolerance`; payload extent derives its ceiling from `Array.MaxLength`. Repeating `ChannelDtype` or optional numeric slots would preserve the deleted parallel axis.

### 12. Delete parametric stage/carrier axes; classify request and result failures correctly

Location: `faults.md`, `ParametricFault`, `ParametricStage`, `ParametricCarrier`; `Parametric/nurbs.md`, `curve.md`, and `surface.md`.

From:

```csharp
[FaultCase(25)] public sealed partial record ParametricFault(ParametricStage Stage, ParametricCarrier Carrier, string Witness) : GeometryFault;
```

To, as the four genuine algorithmic outcomes:

```csharp
[FaultCase(54)] public sealed partial record InvalidKnotVector(int Degree, int KnotCount, string Detail) : GeometryFault;
[FaultCase(55)] public sealed partial record LengthInversionUnconverged(double Target) : GeometryFault;
[FaultCase(56)] public sealed partial record CurveProjectionUnconverged(Point3d Probe) : GeometryFault;
[FaultCase(57)] public sealed partial record OffsetUnconverged(Kind Carrier, double Deviation) : GeometryFault;
```

Exact mapping:

- `KnotVector.Of` passes `degree`, `raw.Length`, and its existing detail.
- Brent exhaustion passes `target`; Newton exhaustion passes `probe`.
- curve and normal-surface refinement exhaustion pass `Kind.Curve` or `Kind.Surface` and the measured deviation.
- NURBS control/weight/point admission maps to `DegenerateInput(Kind.Curve|Surface, offendingIndex, detail)`; compute the first bad weight/point index instead of retaining a carrier label with no location.
- open fill loop, out-of-domain parameter, invalid station plan, non-finite split parameter, and invalid geodesic plan are request refusals -> resolved `key.InvalidInput()`.
- empty station batch, non-curve refit, and non-curve offset fit are impossible completed results -> resolved `key.InvalidResult()`.

Delete both roster types and all eleven rows. Delete `nurbs.md`'s `Construction<T>` and `curve.md`/`surface.md`'s generic `Fault<T>` helpers after their last calls move; no replacement helper is added. The prior audit's `OffsetKind` payload is rejected because no such owner exists.

### 13. Delete `DevelopmentStage`; keep only domain outcomes

Location: `faults.md`, `DevelopmentFault` and `DevelopmentStage`; `Parametric/subdivide.md`, `develop.md`, `panelize.md`, and `patternmap.md`.

From:

```csharp
[FaultCase(26)] public sealed partial record DevelopmentFault(DevelopmentStage Stage, Option<int> Panel, string Witness, Option<double> Measure = default) : GeometryFault;
```

To, as three direct outcomes:

```csharp
[FaultCase(58)] public sealed partial record NoDevelopableStrips : GeometryFault { }
[FaultCase(59)] public sealed partial record StripIsometryExceeded(int Strip, double Distortion, Tolerance Limit) : GeometryFault;
[FaultCase(60)] public sealed partial record PanelPlanarityExceeded(int Panel, double Deviation, Tolerance Limit) : GeometryFault;
```

Exact mapping:

- `StripCount(field) == 0` -> `NoDevelopableStrips`.
- an unrolled strip over the band -> `StripIsometryExceeded(strip, (double)unrolled.Witness, policy.Isometry)`.
- final panel planarity over the band first resolves `WorstPanel(final.Field).ToFin(key.InvalidResult())`, then constructs `PanelPlanarityExceeded(panel, final.Band.Maximum.To(), policy.Planarity)`; absence is not a valid offender payload.

All other former stage faults are not domain outcomes:

- invalid subdivision, strip, panel, pattern-plan, or pattern-policy values -> each resolved operation's `InvalidInput()`;
- wrong `SurfaceResult`, missing quad result, wrong pullback result, absent nearest vertex, malformed cell wall, degenerate panel frame, and empty produced strip field -> `key.InvalidResult()` at the producing operation.

Delete `DevelopmentStage` and its four rows. Delete the now-unused `Subdivision.Fault<T>`, `Development.Fault<T>`, `Panelization.Fault<T>`, and `Patterning.Fault<T>` helpers. Retain no `SubdivisionFailed` or `PatternMappingFailed` leaf: each current producer is admission failure, not an independent recovery identity.

### 14. Install compact ordinals and exhaustive messages

Location: `faults.md`, all `[FaultCase]` attributes and `Message`.

From: the current 28 ordinals `0..27` and the current 28-arm generated `Switch`.

To: the following declaration order is authoritative:

```text
 0 DegenerateInput                       21 EmptyCavity
 1 IndexMismatch                         22 CoplanarInsertionUnsupported
 2 SubdivisionCancelled                  23 ConstraintCrossingMissing
 3 ClassificationCancelled               24 BlockingFaceMissing
 4 WeldCancelled                         25 FlipLimitReached
 5 NativeBooleanCancelled                26 BisectorUndefined
 6 NameCollision                         27 VertexUnrepresentable
 7 TopologyContentMissing                28 UnsupportedTessellationProjection
 8 UnrepairableMesh                      29 DualRequiresExplicitVertex
 9 OverConstrained                       30 CollinearTriangle
10 SingularSystem                        31 CircumcenterInvalid
11 DegenerateOffset                      32 ManifoldLibraryUnavailable
12 SkeletonStalled                       33 NonManifoldIntersection
13 CollapseStalled                       34 MissingIntersectionVertex
14 CellComplexScaleExceeded              35 IncompleteIntersectionWalk
15 ArrangementSubdivisionFailed          36 OpenSection
16 NativeOperandRejected                 37 InvalidSectionNesting
17 NativeBooleanFailed                   38 InsufficientInliers
18 ConstraintUnrecoverable               39 InvalidChartBoundary
19 WalkExitedHull                        40 IncompleteParameterizationSpectrum
20 WalkLimitReached                      41 ParameterizationUnconverged
```

```text
42 FlippedChart                           52 EncodingRoundTripExceeded
43 EmptyProjection                        53 MissingEncodingChannel
44 HatchFailed                            54 InvalidKnotVector
45 FaceBudgetMissed                       55 LengthInversionUnconverged
46 RemeshStalled                          56 CurveProjectionUnconverged
47 ChannelWidthMismatch                   57 OffsetUnconverged
48 DuplicateEncodingChannel               58 NoDevelopableStrips
49 ChannelArityMismatch                   59 StripIsometryExceeded
50 UnboundEncodingChannel                 60 PanelPlanarityExceeded
51 EncodingPayloadTooLarge                61 CotangentQuality
```

Rewrite `Message` as the generated exhaustive `Switch`, one expression arm per row. Every arm renders only its typed payload; `ArrangementSubdivisionFailed` may render `Cause.Message`, width and round-trip arms derive their expected value through `Channel`, and `EncodingPayloadTooLarge` renders `Array.MaxLength`. Do not add a dictionary, formatter helper, case registry, partial switch, or text parser.

To, in bounded blocks inside the one `Switch`:

```csharp
degenerateInput: static f => $"Degenerate {f.Kind} input at {f.Index}: {f.Detail}.",
indexMismatch: static f => $"{f.Kind} index count mismatch: expected {f.Expected}, actual {f.Actual}.",
subdivisionCancelled: static f => $"Arrangement operand {f.Operand} subdivision cancelled at {f.Progress}.",
classificationCancelled: static f => $"Arrangement classification cancelled at {f.Progress}.",
weldCancelled: static f => $"Arrangement weld cancelled at {f.Progress}.",
nativeBooleanCancelled: static f => $"Native boolean cancelled at {f.Progress}.",
nameCollision: static f => $"Topology name {f.Name} collides for {f.Kind}.",
```

```csharp
topologyContentMissing: static f => $"Topology content for {f.Name} ({f.Kind}) is absent from the rebuild.",
unrepairableMesh: static f => $"Mesh repair stopped at {f.Stage}; remaining={f.Remaining}, budget={f.Budget}.",
overConstrained: static f => $"Constraint system has {f.RedundantRows} redundant rows at residual {f.Residual:R}.",
singularSystem: static f => $"Constraint system rank {f.Rank} is singular for {f.Parameters} parameters.",
degenerateOffset: static f => $"Offset wavefront degenerated at vertex {f.WavefrontVertex}, time={f.Time}.",
skeletonStalled: static f => $"Skeleton propagation stalled with {f.PendingEvents} pending events, time={f.Time}.",
collapseStalled: static f => $"Collapse stalled at iteration {f.Iteration}, residual={f.Residual:R}.",
cellComplexScaleExceeded: static f => $"Cell complex has {f.Faces} faces; managed ceiling={f.Ceiling}.",
```

```csharp
arrangementSubdivisionFailed: static f => $"Arrangement operand {f.Operand} face {f.Face} subdivision failed: {f.Cause.Message}",
nativeOperandRejected: static f => $"Native boolean operand {f.Operand} was rejected with status {f.Status}.",
nativeBooleanFailed: static f => $"Native boolean failed with status {f.Status}.",
constraintUnrecoverable: static f => $"Constraint {f.Constraint} exhausted recovery budget {f.Budget}.",
walkExitedHull: static f => $"Tessellation walk exited the hull from simplex {f.Simplex}.",
walkLimitReached: static f => $"Tessellation walk from simplex {f.Simplex} reached limit {f.Limit}.",
emptyCavity: static f => $"Tessellation cavity at simplex {f.Simplex} has no boundary.",
coplanarInsertionUnsupported: static f => $"Coplanar insertion at simplex {f.Simplex} is unsupported in tetrahedralization.",
```

```csharp
constraintCrossingMissing: static f => $"Constraint edge ({f.A}, {f.B}) has no crossing or on-segment vertex.",
blockingFaceMissing: static f => $"Constraint edge ({f.A}, {f.B}) has no blocking tetrahedron face.",
flipLimitReached: static f => $"{f.Edges} tessellation edges exceeded flip limit {f.Limit}.",
bisectorUndefined: static f => $"Voronoi bisector at site {f.Site} is numerically undefined.",
vertexUnrepresentable: static f => $"Tessellation vertex {f.Vertex} cannot be represented explicitly.",
unsupportedTessellationProjection: static f => $"{f.Kind} cannot project to {f.Output.Name}.",
dualRequiresExplicitVertex: static f => $"Dual simplex {f.Simplex} contains an implicit vertex.",
collinearTriangle: static f => $"Dual simplex {f.Simplex} is collinear.",
```

```csharp
circumcenterInvalid: static f => $"Dual simplex {f.Simplex} produced an invalid circumcenter.",
manifoldLibraryUnavailable: static f => $"Manifold library is unavailable for {f.RuntimeIdentifier}; faces={f.Faces}, managed ceiling={f.ManagedCeiling}.",
nonManifoldIntersection: static f => $"Intersection between {f.A} and {f.B} branches at vertex {f.Junction}.",
missingIntersectionVertex: static f => $"Intersection between {f.A} and {f.B} is missing vertex {f.Vertex}.",
incompleteIntersectionWalk: static f => $"Intersection between {f.A} and {f.B} left edge ({f.From}, {f.To}) unvisited.",
openSection: static f => $"Section layer {f.Layer} at elevation {f.Elevation:R} has {f.Chains} open chains.",
invalidSectionNesting: static f => $"Section layer {f.Layer} at elevation {f.Elevation:R} has invalid nesting across {f.Contours} contours.",
insufficientInliers: static f => $"Fit inlier fraction {f.Inliers} is below floor {f.Floor}.",
```

```csharp
invalidChartBoundary: static f => $"Chart boundary is invalid; loops={f.Loops}, vertices={f.Vertices}.",
incompleteParameterizationSpectrum: static f => $"Parameterization spectrum has {f.Actual} modes; expected {f.Expected}.",
parameterizationUnconverged: static f => $"Parameterization did not converge after {f.Iterations} iterations; residual={f.Residual}.",
flippedChart: static f => $"Chart {f.Chart} contains a flipped face; maximum conformal distortion={f.MaxConformal:R}.",
emptyProjection: static _ => "Projection produced no drawable edges.",
hatchFailed: static f => $"Hatch {f.Pattern} region {f.Region} failed: {f.Detail}.",
faceBudgetMissed: static f => $"Decimation achieved {f.Achieved} faces against budget {f.FaceBudget}.",
remeshStalled: static f => $"Remeshing stalled after {f.Iterations} iterations; target={f.TargetLength}, achieved={f.Achieved}.",
```

```csharp
channelWidthMismatch: static f => $"Encoding channel {f.Channel} requires width {f.Channel.Dtype.Width}; actual={f.Actual}.",
duplicateEncodingChannel: static f => $"Encoding channel {f.Channel} is duplicated.",
channelArityMismatch: static f => $"Encoding channel {f.Channel} requires {f.Expected} values; actual={f.Actual}.",
unboundEncodingChannel: static f => $"Encoding channel {f.Channel} has no bound lane.",
encodingPayloadTooLarge: static f => $"Encoding payload has {f.Bytes} bytes; maximum={Array.MaxLength}.",
encodingRoundTripExceeded: static f => $"Encoding channel {f.Channel} round-trip error {f.Error:R} exceeds {f.Channel.Dtype.Tolerance:R}.",
missingEncodingChannel: static f => $"Encoding channel {f.Channel} is missing.",
invalidKnotVector: static f => $"Knot vector degree {f.Degree}, count {f.KnotCount} is invalid: {f.Detail}.",
```

```csharp
lengthInversionUnconverged: static f => $"Length inversion did not converge at target {f.Target:R}.",
curveProjectionUnconverged: static f => $"Curve projection did not converge for point {f.Probe}.",
offsetUnconverged: static f => $"{f.Carrier} offset did not converge; deviation={f.Deviation:R}.",
noDevelopableStrips: static _ => "Development produced no developable strips.",
stripIsometryExceeded: static f => $"Strip {f.Strip} distortion {f.Distortion:R} exceeds {f.Limit}.",
panelPlanarityExceeded: static f => $"Panel {f.Panel} deviation {f.Deviation:R} exceeds {f.Limit}.",
cotangentQuality: static f => $"Face {f.Face} cotangent aspect ratio {f.Ratio} exceeds ceiling {f.Ceiling}.");
```

Accounting proof for the target fence, using the final one-line leaf/message form: imports header/usings 12 + namespace 1 + errors header/root 5 + leaves 62 + `Message` header/arms 63 + closing brace 1 = **144 nonblank authored C# lines**.

### 15. Resize and relocate the sole geometry band

Location: `libs/dotnet/Rasm/.planning/Domain/results.md`, `FaultBand.Geometry`, implementation ripple only.

From:

```csharp
public static readonly FaultBand Geometry = new(2400, 28, BandKind.Fault, TelemetrySource.Kernel);
```

To:

```csharp
public static readonly FaultBand Geometry = new(2350, 62, BandKind.Fault, TelemetrySource.Kernel);
```

Disjointness proof:

- `Component`: `[2300, 2341)`
- `Geometry`: `[2350, 2412)`
- `Appearance`: `[2450, 2453)`

Keeping base 2400 would overlap `Appearance` at 2450. Moving only the unshipped Geometry band preserves both neighbors and leaves explicit gaps. The span equals the direct-leaf count, and ordinals fill `0..61`; no band literal or ordinal mirror is added anywhere else.

### 16. Delete obsolete rosters only after direct call-site migration

Location: `faults.md` types section; `Drawing/pack.md`; `Meshing/arrangement.md`.

Delete in the final cleanup:

```text
ParametricStage
DevelopmentStage
ParametricCarrier
TessellationWitness
ArrangementWitness
AbandonWitness
EncodingStage
NativeEngine
```

After these deletions `faults.md` has no `[TYPES]` section; remove the empty separator. Deletion is last so every producer moves directly from old identity to final identity with no compatibility interval.

Update the target card at the same time.

From:

```text
- [02]-[FAULT_BAND]: `GeometryFault` and the typed stage, carrier, and witness vocabularies its leaves compose.
- Owner: `GeometryFault` the direct `FaultBand.Geometry` union; `ParametricStage`, `DevelopmentStage`, `ParametricCarrier`, and the witness rosters type its payloads.
- Cases: the 28 direct leaves in the fence, each at its compact generated ordinal.
```

To:

```text
- [02]-[FAULT_BAND]: `GeometryFault` and the typed evidence its direct leaves carry.
- Owner: `GeometryFault` the direct `FaultBand.Geometry` union.
- Cases: the 62 direct leaves in the fence, each at its compact generated ordinal.
```

The existing growth line remains structurally correct unchanged: one new leaf still compacts `0..N-1` and changes the sole band span in the same edit.

### 17. Repair every prose, card, diagram, and density ripple

Location: every consumer page edited by moves 2 through 13; no page outside this closed list names a deleted symbol or old leaf after the code replacements:

```text
Spatial/naming.md                 Spatial/reconciliation.md
Meshing/arrangement.md            Meshing/delaunay.md
Meshing/intersect.md              Meshing/slice.md
Solving/fit.md                    Processing/flatten.md
Processing/decimate.md            Processing/remesh.md
Drawing/view.md                   Drawing/hatch.md
Drawing/pack.md                   Parametric/nurbs.md
Parametric/curve.md               Parametric/surface.md
Parametric/subdivide.md           Parametric/develop.md
Parametric/panelize.md            Parametric/patternmap.md
Domain/results.md                 Rasm/RULINGS.md
```

From -> To: replace only the following identity-bearing prose and diagram labels; do not add a migration narrative or a leaf roster.

```text
KindMismatch                     -> Op.InvalidResult
HashMismatch                     -> TopologyContentMissing
RunAbandoned / AbandonWitness    -> the four direct cancellation cases
DegenerateArrangement / ArrangementWitness
                                 -> the four direct arrangement cases or Op.InvalidResult
DegenerateTessellation / TessellationWitness
                                 -> the thirteen direct tessellation cases
NativeAssetMissing / NativeEngine
                                 -> ManifoldLibraryUnavailable
IntersectionFault               -> the three direct intersection cases or Op.InvalidResult
SectionFault                    -> OpenSection / InvalidSectionNesting / Op.InvalidResult
FitFault                        -> InsufficientInliers
ParameterizationFault           -> the four direct parameterization cases
ProjectionFault                 -> EmptyProjection / Op.InvalidResult
HatchFault                      -> HatchFailed
DecimationFault                 -> FaceBudgetMissed / Op.InvalidResult
EncodingFault / EncodingStage   -> the seven direct encoding cases
ParametricFault / ParametricStage / ParametricCarrier
                                 -> the four direct parametric outcomes or Op.InvalidInput/InvalidResult
DevelopmentFault / DevelopmentStage
                                 -> the three direct development outcomes or Op.InvalidInput/InvalidResult
```

For `Rasm/RULINGS.md`, replace the obsolete single `RunAbandoned` row with the four direct cancellation identities while preserving its existing distinction from direct effect-token cancellation. For `Domain/results.md`, change only the `Geometry` band row. In density tables, replace old case counts only where the table counts the changed owner; never insert the 62-leaf roster as a table or diagram. A final repository search after implementation must find none of the eight deleted roster names, `GeometryFault.KindMismatch`, or any retired category-bearing leaf name listed above.
