# 1. Remove the unused Prelude import

`libs/dotnet/Rasm/.planning/Numerics/faults.md:32`
From
```csharp
using static LanguageExt.Prelude;
```
To
```csharp
// using static LanguageExt.Prelude DELETED
```
Why
The fence calls no Prelude member; `Option<T>` is available through `using LanguageExt`.

Change
Delete the static Prelude import.

Delta
−1 LOC; 0 symbols, members, or types.

# 2. Collapse arrangement cancellation into one staged fault

`libs/dotnet/Rasm/.planning/Numerics/faults.md:44-47,120-123`
From
```csharp
[FaultCase(2)] public sealed partial record SubdivisionCancelled(int Operand, UnitInterval Progress) : GeometryFault;
[FaultCase(3)] public sealed partial record ClassificationCancelled(UnitInterval Progress) : GeometryFault;
[FaultCase(4)] public sealed partial record WeldCancelled(UnitInterval Progress) : GeometryFault;
[FaultCase(5)] public sealed partial record NativeBooleanCancelled(UnitInterval Progress) : GeometryFault;
subdivisionCancelled: static fault => $"Arrangement operand {fault.Operand} subdivision cancelled at {fault.Progress}.",
classificationCancelled: static fault => $"Arrangement classification cancelled at {fault.Progress}.",
weldCancelled: static fault => $"Arrangement weld cancelled at {fault.Progress}.",
nativeBooleanCancelled: static fault => $"Native boolean cancelled at {fault.Progress}.",
```
To
```csharp
[FaultCase(2)] public sealed partial record ArrangementCancelled(ArrangementStage Stage, Option<int> Operand, UnitInterval Progress) : GeometryFault;
arrangementCancelled: static fault => $"Arrangement {fault.Stage} cancelled at {fault.Progress}; operand={fault.Operand}.",
```
Why
The four leaves differ only by one arrangement-stage discriminant and the subdivision arm's optional operand. Separate case types duplicate one cancellation capability, while `Native` is an ambiguous implementation adjective. A typed `ArrangementStage` preserves exhaustive stage recovery and removes three fault identities and three generated dispatch arms.

Change
Replace the four cases with `ArrangementCancelled`, add `ArrangementStage.Subdivision`, `Classification`, `Weld`, and `Manifold` at the arrangement owner, and compact the final fault ordinals to `0..59`.

Delta
−6 LOC in this sheet and −5 LOC project-wide; −2 types net and −2 fault-payload members, with 4 enum members added.

Ripples
`libs/dotnet/Rasm/.planning/Meshing/arrangement.md`: add `ArrangementStage`, replace all four cancellation constructions with the staged case, and replace their prose and diagram roster. `libs/dotnet/Rasm/.planning/Domain/results.md`: set `FaultBand.Geometry` span to `60` after the skeleton split below and keep the compact final ordinals aligned.

# 3. Remove the constant entity-kind payload from primitive-count mismatch

`libs/dotnet/Rasm/.planning/Numerics/faults.md:43,119`
From
```csharp
[FaultCase(1)] public sealed partial record IndexMismatch(EntityKind Kind, int Expected, int Actual) : GeometryFault;
indexMismatch: static fault => $"{fault.Kind} index count mismatch: expected {fault.Expected}, actual {fault.Actual}.",
```
To
```csharp
[FaultCase(1)] public sealed partial record PrimitiveCountMismatch(int Expected, int Actual) : GeometryFault;
primitiveCountMismatch: static fault => $"Spatial primitive count mismatch: expected {fault.Expected}, actual {fault.Actual}.",
```
Why
Both producers compare a caller-supplied primitive column with `SpatialIndex.Primitives` and pass the literal `EntityKind.Face`, even though the index admits generic bounding-box primitives. The literal is neither true for every index nor independent failure evidence.

Change
Rename the case for the checked cardinality and delete the constant `Kind` member.

Delta
0 LOC; −1 payload member and 0 types.

Ripples
`libs/dotnet/Rasm/.planning/Spatial/index.md`: replace both `GeometryFault.IndexMismatch(EntityKind.Face, expected, actual)` constructions with `GeometryFault.PrimitiveCountMismatch(expected, actual)` and rename the fault in the owner prose and diagrams.

# 4. Name inconsistent constraint systems by the failed predicate

`libs/dotnet/Rasm/.planning/Numerics/faults.md:54,127`
From
```csharp
[FaultCase(9)] public sealed partial record OverConstrained(int RedundantRows, double Residual) : GeometryFault;
overConstrained: static fault => $"Constraint system has {fault.RedundantRows} redundant rows at residual {fault.Residual:R}.",
```
To
```csharp
[FaultCase(6)] public sealed partial record InconsistentConstraints(int DependentRows, double Residual) : GeometryFault;
inconsistentConstraints: static fault => $"Constraint system has {fault.DependentRows} dependent rows at residual {fault.Residual:R}.",
```
Why
An overdetermined system may be consistent. This branch fails only when the witness reports dependent rows and the residual remains above tolerance, so the current name misclassifies the refusal and `RedundantRows` overstates what `DofReport.Deficiency` proves.

Change
Rename the case and row-count member while retaining the evidence shape and assigning its compact final ordinal.

Delta
0 LOC; 0 symbols, members, or types.

Ripples
`libs/dotnet/Rasm/.planning/Solving/solver.md`: construct `GeometryFault.InconsistentConstraints(report.Deficiency, norm)` and replace `OverConstrained` in the entry contract and diagram.

# 5. Separate curve-skeleton, contraction, and straight-skeleton failures

`libs/dotnet/Rasm/.planning/Numerics/faults.md:58-59,130-131`
From
```csharp
[FaultCase(12)] public sealed partial record SkeletonStalled(int PendingEvents, Option<double> Time = default) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
[FaultCase(13)] public sealed partial record CollapseStalled(int Iteration, double Residual) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
skeletonStalled: static fault => $"Skeleton propagation stalled with {fault.PendingEvents} pending events, time={fault.Time}.",
collapseStalled: static fault => $"Collapse stalled at iteration {fault.Iteration}, residual={fault.Residual:R}.",
```
To
```csharp
[FaultCase(9)] public sealed partial record CurveSkeletonStalled(int RemainingFaces) : GeometryFault;
[FaultCase(10)] public sealed partial record ContractionUnconverged(int Iteration, double Residual) : GeometryFault;
[FaultCase(11)] public sealed partial record StraightSkeletonUnconverged(int PendingEvents, double Time) : GeometryFault;
curveSkeletonStalled: static fault => $"Curve-skeleton surgery stalled with {fault.RemainingFaces} faces remaining.",
contractionUnconverged: static fault => $"Mesh contraction did not converge after {fault.Iteration} iterations; residual={fault.Residual:R}.",
straightSkeletonUnconverged: static fault => $"Straight-skeleton propagation did not converge; pending={fault.PendingEvents}, time={fault.Time:R}.",
```
Why
The current cases conflate three algorithms and mislabel their evidence: curve-skeleton surgery passes remaining faces as pending events, contraction passes an area ratio or solve residual, and straight-skeleton propagation passes queued events and event time. All are deterministic under fixed input and policy, so replay cannot repair them.

Change
Give each algorithm one truthful case, route all three straight-skeleton budget or same-time exits through `StraightSkeletonUnconverged`, and inherit terminal retriability from `Fault`.

Delta
+2 LOC; +1 type and −1 member net because two retriability overrides are deleted.

Ripples
`libs/dotnet/Rasm/.planning/Meshing/skeleton.md`: replace `CollapseStalled` with `ContractionUnconverged`, replace the surgery-queue `SkeletonStalled(liveFaces)` with `CurveSkeletonStalled(liveFaces)`, and update prose and diagrams. `libs/dotnet/Rasm/.planning/Meshing/offset.md`: replace both budget exits and the same-time-cycle exit with `StraightSkeletonUnconverged(queue.Count, eventTime)` and update prose and diagrams.

# 6. Carry Manifold's typed status vocabulary directly

`libs/dotnet/Rasm/.planning/Numerics/faults.md:63-64,134-135`
From
```csharp
[FaultCase(16)] public sealed partial record NativeOperandRejected(int Operand, int Status) : GeometryFault;
[FaultCase(17)] public sealed partial record NativeBooleanFailed(int Status) : GeometryFault;
nativeOperandRejected: static fault => $"Native boolean operand {fault.Operand} was rejected with status {fault.Status}.",
nativeBooleanFailed: static fault => $"Native boolean failed with status {fault.Status}.",
```
To
```csharp
[FaultCase(14)] public sealed partial record ManifoldOperandRejected(int Operand, ManifoldError Status) : GeometryFault;
[FaultCase(15)] public sealed partial record ManifoldBooleanRejected(ManifoldError Status) : GeometryFault;
manifoldOperandRejected: static fault => $"Manifold operand {fault.Operand} was rejected with status {fault.Status}.",
manifoldBooleanRejected: static fault => $"Manifold boolean was rejected with status {fault.Status}.",
```
Why
`Native` is ambiguous in a package that also binds Rhino, and raw integers erase the admitted `ManifoldError` vocabulary. The provider enum makes status recovery typed and prevents unrelated integers from constructing either case.

Change
Rename both provider cases, replace their `int` statuses with `ManifoldError`, and assign their compact final ordinals.

Delta
0 LOC; 0 symbols, members, or types.

Ripples
`libs/dotnet/Rasm/.planning/Meshing/arrangement.md`: type `manifold_status` and its local status values as `ManifoldError`, replace magic zero checks with `MANIFOLD_NO_ERROR`, rename both fault constructions, and update the fault roster and diagram.

# 7. Name deterministic constraint-recovery exhaustion precisely

`libs/dotnet/Rasm/.planning/Numerics/faults.md:65,136`
From
```csharp
[FaultCase(18)] public sealed partial record ConstraintUnrecoverable(int Constraint, Dimension Budget) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
constraintUnrecoverable: static fault => $"Constraint {fault.Constraint} exhausted recovery budget {fault.Budget}.",
```
To
```csharp
[FaultCase(16)] public sealed partial record ConstraintRecoveryExhausted(int Constraint, Dimension Budget) : GeometryFault;
constraintRecoveryExhausted: static fault => $"Constraint {fault.Constraint} exhausted recovery budget {fault.Budget}.",
```
Why
`Unrecoverable` claims an absolute result while the payload proves only exhaustion of the configured recovery budget. Every producer is a deterministic constrained-tessellation pass, so redriving the same input and budget repeats the same outcome.

Change
Rename the case to the bounded failure it proves, delete its retriability override, and assign its compact final ordinal.

Delta
0 LOC; −1 override member and 0 types.

Ripples
`libs/dotnet/Rasm/.planning/Meshing/delaunay.md`: replace all `GeometryFault.ConstraintUnrecoverable` constructions and prose or diagram references with `GeometryFault.ConstraintRecoveryExhausted`.

# 8. Name remeshing non-convergence and make it terminal

`libs/dotnet/Rasm/.planning/Numerics/faults.md:98,164`
From
```csharp
[FaultCase(46)] public sealed partial record RemeshStalled(PositiveMagnitude TargetLength, Option<double> Achieved, int Iterations) : GeometryFault { public override Retriability Retriability => Retriability.Transient; }
remeshStalled: static fault => $"Remeshing stalled after {fault.Iterations} iterations; target={fault.TargetLength}, achieved={fault.Achieved}.",
```
To
```csharp
[FaultCase(44)] public sealed partial record RemeshUnconverged(PositiveMagnitude TargetLength, Option<double> Achieved, int Iterations) : GeometryFault;
remeshUnconverged: static fault => $"Remeshing did not converge after {fault.Iterations} iterations; target={fault.TargetLength}, achieved={fault.Achieved}.",
```
Why
The producer reports either an iteration-budget exit above the convergence band or a run with no measured pass; neither proves that progress stalled. Both outcomes are deterministic under the same mesh, target, and policy, so replay is not recovery.

Change
Rename the case to the proved convergence verdict, delete the retriability override, and assign its compact final ordinal.

Delta
0 LOC; −1 override member and 0 types.

Ripples
`libs/dotnet/Rasm/.planning/Processing/remesh.md`: rename both constructions and every prose or diagram reference to `GeometryFault.RemeshUnconverged`, and remove the transient-retry claim.

# 9. Name decimation budget failure as an exceeded limit

`libs/dotnet/Rasm/.planning/Numerics/faults.md:97,163`
From
```csharp
[FaultCase(45)] public sealed partial record FaceBudgetMissed(int FaceBudget, int Achieved) : GeometryFault;
faceBudgetMissed: static fault => $"Decimation achieved {fault.Achieved} faces against budget {fault.FaceBudget}.",
```
To
```csharp
[FaultCase(43)] public sealed partial record FaceBudgetExceeded(int Budget, int Actual) : GeometryFault;
faceBudgetExceeded: static fault => $"Decimation produced {fault.Actual} faces; budget={fault.Budget}.",
```
Why
The producer fails only when the live face count remains above the requested budget. `Missed` and `Achieved` obscure that ordered predicate; `Exceeded`, `Budget`, and `Actual` state it directly.

Change
Rename the case and its payload members to the measured limit violation and assign its compact final ordinal.

Delta
0 LOC; 0 symbols, members, or types.

Ripples
`libs/dotnet/Rasm/.planning/Processing/decimate.md`: rename the construction and every prose or diagram reference to `GeometryFault.FaceBudgetExceeded`.

# 10. Name the Rhino face-aspect-ratio predicate correctly

`libs/dotnet/Rasm/.planning/Numerics/faults.md:115,179`
From
```csharp
[FaultCase(61)] public sealed partial record CotangentQuality(int Face, PositiveMagnitude Ratio, PositiveMagnitude Ceiling) : GeometryFault;
cotangentQuality: static fault => $"Face {fault.Face} cotangent aspect ratio {fault.Ratio} exceeds ceiling {fault.Ceiling}.");
```
To
```csharp
[FaultCase(59)] public sealed partial record FaceAspectRatioExceeded(int Face, PositiveMagnitude AspectRatio, PositiveMagnitude Ceiling) : GeometryFault;
faceAspectRatioExceeded: static fault => $"Face {fault.Face} aspect ratio {fault.AspectRatio} exceeds ceiling {fault.Ceiling}.");
```
Why
The sole producer reads `Mesh.Faces.GetFaceAspectRatio`; it does not inspect cotangent weights. The current case and message therefore report a different metric from the one that failed.

Change
Rename the case and measured member to the actual Rhino face-aspect-ratio predicate and assign its compact final ordinal.

Delta
0 LOC; 0 symbols, members, or types.

Ripples
`libs/dotnet/Rasm/.planning/Meshing/mesh.md`: rename the construction and every prose or diagram reference to `GeometryFault.FaceAspectRatioExceeded`.

# 11. Name the offset geometry kind without a carrier abstraction

`libs/dotnet/Rasm/.planning/Numerics/faults.md:111,175`
From
```csharp
[FaultCase(57)] public sealed partial record OffsetUnconverged(Kind Carrier, double Deviation) : GeometryFault;
offsetUnconverged: static fault => $"{fault.Carrier} offset did not converge; deviation={fault.Deviation:R}.",
```
To
```csharp
[FaultCase(55)] public sealed partial record OffsetUnconverged(Kind Kind, double Deviation) : GeometryFault;
offsetUnconverged: static fault => $"{fault.Kind} offset did not converge; deviation={fault.Deviation:R}.",
```
Why
The payload is the geometry `Kind` discriminant (`Curve` or `Surface`), not a carrier. Naming it directly removes an abstraction label and keeps the value one hop from its semantics.

Change
Rename the positional member from `Carrier` to `Kind` and assign its compact final ordinal.

Delta
0 LOC; 0 symbols, members, or types.
