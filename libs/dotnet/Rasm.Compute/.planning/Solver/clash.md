# [COMPUTE_CLASH]

Rasm.Compute solver clash confirms collisions and scores live telemetry: `ClashScale` is the narrow-phase collision-confirmation fold over the geometry-owned node-link broad-phase wire, and `DigitalTwin` scores a live signal against a `Surrogate` baseline through the injected `Stats/estimator` changepoint detector over a bounded residual window. `AccelerationStructure` is one decoded wire record parameterized by `AccelerationKind`; `Spatial.Apply(SpatialOp.Wire)` remains the sole builder/refitter.

Narrow-phase triangle and closest-distance work rides `System.Numerics.Vector3` hardware vectors over a `MemoryMarshal.Cast<float, Vector3>` view of the federated triangle wire; the twin baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` evaluated through `Surrogate.Predict` over a `DesignPoint`, so the twin and the design search share one reduced-order model. `DigitalTwin.Update` closes the twin loop's model end: the `Stats/signal#SIGNAL_LANE` `MeasuredMode` set calibrates the FE stiffness/mass parameter vector through the `Tensor/blas#DENSE_ALGEBRA` `LevenbergMarquardt.Minimize` black-box arm, so an FE model that never reconciles with measured dynamics stops being the twin's baseline. `ComputeReceipt`, `WorkLane`, `CorrelationId`, NodaTime `IClock` for the semantic stamp with kernel `MonotonicTimeline` for the elapsed span (the app-stratum `ClockPolicy` never descends to a twin lane), and the Thinktecture `ComparerAccessors.StringOrdinal` accessor arrive settled. Candidate `ClashPair` sets feed the `Model/run#RUN_MODES` `ClashScore` false-positive filter; a twin control suggestion crosses to the AppHost `Wire/livewire#WRITE_BACK` outbound write-back as a receipted `ExternalValue`. Page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[CLASH_AND_TWIN]: node-link narrow-phase collision confirmation and clearance descent; detector-composed ROM digital-twin loop; measured-mode FE model updating.

## [02]-[CLASH_AND_TWIN]

- Owner: `AccelerationKind` `[SmartEnum<string>]` carries BVH/octree child arity; `AccelerationStructure` carries one decoded `(Bounds, Nodes, BuildParameter)` wire; `ClashKind` classifies hard · clearance · duplicate; `ClashScale` owns admission, traversal, intersection, and clearance; `DigitalTwin` owns residual-window scoring composed over the Stats detector and the `Update` FE-updating fold with its `ModelUpdatePolicy`/`UpdateVerdict` carriers; `TwinLoop` owns typed `SensorReading<TwinSignal>` admission, revisioned atomic held-window scoring, anomaly control through the injected `Runtime/board#HOOK_POINTS` veto fold, and edge-consuming recalibration cadence.
- Law: `ClashKind` has a NAMED cross-seam counterpart — Bim `Model/systems.md` carries a two-row `{HARD, CLEARANCE}` roster under an IgnoreCase comparer for the BCF wire while this roster is Ordinal lower-case with the third `duplicate` row; NO wire crossing exists today (no `libs/contracts/manifest.json` row), so the divergence is lawful parity, and a future crossing keys on the BCF spelling AT THE BOUNDARY with this roster's keys never leaving the package — a shared-key assumption across the two comparers mis-keys silently, which is why this line exists.
- Cases: `AccelerationKind` bvh · octree; `ClashKind` hard · clearance · duplicate, admitted through `ClashPolicy.Admitted` (empty means every kind) rather than a bool knob suppressing two classifier arms; `ClashPair` carries a NON-NEGATIVE `Clearance` beside an `Intersecting` discriminant — a confirmed triangle-surface intersection is `Intersecting` at zero clearance and a clearance band is a positive distance, because the decoded surface wire carries no volumetric penetration domain and a signed scalar standing in for both reads a deeper overlap as a wider gap.
- Entry: `AdmittedScene.Of(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy)` runs the complete `Admit` traversal ONCE and mints the private-ctor scene evidence every query reads — `Fin<T>` aborts on a malformed wire (mis-aligned bounds/triangle buffer, out-of-range child/leaf range, out-of-range leaf primitive id, a node two parents claim), and a per-query re-validation is the deleted quadratic form; `Detect(scene)` returns the `ClashSurvey` of confirmed pairs beside its candidate count and truncation flag, `Clearance(scene, Vector3 point)` and `SweptClearance(scene, path)` descend the same tree for the point-to-scene nearest-surface distance and the CAM/motion swept-volume minimum clearance — each path leg also ray-tests the surface, so a crossing between samples reports zero clearance AT THE CROSSING POINT rather than at an endpoint the tool merely passed through — and `Occluded(scene, origin, direction, maxDistance)` answers the ray test over the same first-hit descent. Detection is a pure geometry fold, so the `CorrelationId`/`IClock` receipt tail enters only at `Receipt`, never as a dead entry param. Incremental edits are the kernel `SpatialOp.Refit` seam — a moved element re-bounds there and re-projects through `SpatialOp.Wire` unchanged, so a Compute-local `Insert`/`Remove` index rebuild is the rejected double-owner form. `TwinLoop.Of(baseline, detector, policy, suggested, clock, archive)` is the validated mint — an invalid `TwinLoopPolicy` fails before any held state constructs, and the optional archive pair (per-segment sink factory + `Runtime/archive#HDF_ARCHIVE` policy) arms the durable observation tier — and `TwinLoop.Ingest(SensorReading<TwinSignal> reading)` is the typed telemetry entry answering `IO<Fin<TwinVerdict>>`, weighting each admitted reading by its own `sampledrate` denominator; a caller enqueues each decoded message envelope onto `WorkLane.CaptureIngest`, and the lane dispatch routes admitted signals here. `TwinLoop.ClaimRecalibration()` (`IO<Fin<bool>>` — a due claim also SEALS the accumulated observation segment through the ONE `ArchiveSession` as a create-only container, so the cadence edge is the archive boundary and a month-long twin is a segment series a re-fit reads whole) consumes each scoring-cadence edge once before composition drives `Update` with `Stats/signal` `Transform.Modal` measured modes.
- Auto: `Detect` walks the contiguous `[FirstChild, FirstChild+ChildCount)` child range as one hierarchical descent — a BVH node and an octree cell traverse identically, so the prior parallel `BvhPairs`/`OctreePairs` bodies collapse to one `NodeLinkPairs` fold and the Morton-cell decode that mis-read the node-link array as a per-element cell map is the deleted form; each overlapping leaf-pair runs the complete two-direction Möller–Trumbore test and the Ericson closest distance and bands by `ClashPolicy`. `DigitalTwin` pushes the `Surrogate` residual onto the estimator-owned bounded `ResidualWindow` and reads the injected detector's last-row score and change flag into a verdict and a control suggestion — one anomaly owner, twin-local control only.
- Receipt: the `Clash` `ComputeReceipt` case carries the index kind, candidate-pair count, confirmed hard-clash and clearance-violation counts, and total pairs, projected from the `ClashSurvey` so the counts and the candidate total come from ONE traversal; the survey's truncation flag rides the carrier beside them, the same way the optimizer's exact evidence rides its own result rather than claiming a slot the receipt owner does not declare. The `Twin` case carries the signal id, predicted-versus-measured residual, detector anomaly flag, and suggested control delta, so a twin loop is auditable and a machine-control suggestion is receipted before it leaves the boundary.
- Packages: PureHDF (reached ONLY through the `Runtime/archive#HDF_ARCHIVE` `ArchiveSession` capsule — this page declares slots and attributes and opens no container of its own), Generator.Equals (`[Equatable]` structural equality and the `Inequalities` diff on `UpdateVerdict`/`ModelUpdatePolicy`), System.Numerics (`Vector3`), System.Runtime.InteropServices (`MemoryMarshal`), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter`), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll` the span finiteness gate and `SumOfSquares` the MAC denominator — the complex MAC numerator is a phase-weighted fold no pair reduce covers), MathNet.Numerics (`Vector<double>`/`Matrix<double>` the LM contract carries), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Schedule` the commit-retry policy, `IO` the archive and retry rails), NodaTime, Rasm (project, kernel signal capsule, `Transition`/`Cell` the one lock-free transition verdict), BCL inbox.
- Growth: a new clash kind is one `ClashKind` row and one `Kind` precedence arm, admissible to any caller through the same set column; a new twin scoring channel is one field on `TwinSignal`/`TwinVerdict`; a new updating residual term (mode-shape components, static deflections) is one weighted row pair on the `Update` stacked residual with its `ModelUpdatePolicy` weight column; a new narrow-phase band edge or coplanarity threshold is one `ClashPolicy` column; a new broad-phase kernel that still emits the node-link wire reuses `NodeLinkPairs` untouched; zero new surface — a `BvhTree`/`OctreeIndex`/`SdfField` sibling family collapses onto the one decoded `AccelerationStructure` wire and the one `NodeLinkPairs` traversal, and a standalone `ModelUpdater` service collapses onto `DigitalTwin.Update`.
- Boundary: `AccelerationStructure` is the decoded read-only node-link wire, and `AccelerationKind` validates only the builder-specific child arity. `Admit` verifies finite ordered boxes, finite nondegenerate triangles, leaf primitive ranges, child ranges, root reachability, acyclicity, and SINGLE PARENTAGE before traversal — the last proves the wire is a tree, which is exactly what makes each unordered node pair reachable by one descent path and lets `NodeLinkPairs` drop the visited set it otherwise pays per query; an acyclic wire whose child ranges overlap is a DAG that would re-expand a shared subtree, so admission refuses it by name rather than a traversal absorbing it silently.
- Boundary: `NodeLinkPairs` canonicalizes node pairs and expands equal internal nodes upper-triangular; the pair sink stops at `ClashPolicy.MaxPairs` and the survey publishes the truncation, because an unbounded sink over a degenerate scene is the memory failure a clash run must degrade around rather than die on. `Clearance` is non-negative and `Intersecting` is its own column; volumetric penetration depth requires a solid-domain carrier absent from this wire.
- Boundary: this page is the FLOAT production-plane clash tier — the third rail beside the kernel's two intersection owners, and the kernel ruling's voice extends unchanged: `Analysis/relations` owns host-native intersection, `Meshing/intersect` the predicate-exact tier, and the consumer's tolerance source decides. A `ClashPolicy` band over a federated float triangle wire lands here; a verdict that must survive degeneracy — shared vertex, coplanar contact, exact straddle — adjudicates at the `Rasm/Meshing/intersect#INTERSECTION` exact lattice, and a host-parametric NURBS/Brep pair at `Rasm/Analysis/relations#INTERSECTION_TABLE`; a float separation never upgrades into an exact claim. Composition already runs where it fits without redesign — the broad phase IS the kernel `Rasm/Spatial/index#SPATIAL_INDEX` structure (`Spatial.Apply(SpatialOp.Wire)` builds, `SpatialOp.Refit` re-bounds, this page only decodes and traverses) — while the narrow phase stays float-native BY DESIGN: the triangle wire is quantized to float at the federation boundary, so the `System.Numerics.Vector3` hardware lanes are the honest arithmetic for it and folding the kernel `IntersectOp` exact narrow phase over it would certify precision the wire no longer carries; the tier discriminant is the wire's own dtype, never a preference.
- Boundary: `RayFirstHit` and `NearestTriangle` run per path sample and per query point, so their descent stacks are process-static `[ThreadStatic]` scratch grown once per thread and the slab test indexes the `Vector3` components directly — a per-call `Stack<int>` and a per-axis span rebuild are the allocation the hot loop exists without. One ray descent serves both the occlusion predicate and the sweep witness, so a bool-returning twin walking the same tree for less evidence has nowhere to reappear.
- Boundary: `AccelerationStructure` carries the C#-only clash branch golden `tests/dotnet/README.md` `[09]-[SNAPSHOTS]` registers, its bytes frozen at the `Rasm/Spatial/index#SPATIAL_INDEX` producer — this decoder asserts the `Decode` and leaf-tail round-trip over that producer's pinned descriptors and never re-freezes a second vector; clash pairs stay OUT of golden scope, since `NodeLinkPairs` classifies triangles under a `ClashPolicy` and the golden pins neither a triangle wire nor a policy row.
- Boundary: `DigitalTwin.Score` faults malformed signals, windows, policies, surrogate outputs, and non-Anomaly detector carriers; changepoint state, thresholds, and anomaly classification live on the injected Stats detector, and corrective control opposes the raw residual only on a flagged change. `TwinLoop.Ingest` snapshots state, runs detector scoring outside the atom, and commits only against the sampled revision through the kernel `Cell.Step` declining transition — the verdict IS the outcome, so no ticket column reconstructs it from state both racers can read. A competing commit spends one attempt of the `Schedule.recurs` budget and exhaustion surfaces the typed contention fault the last attempt raised; the retry is guarded on that fault ALONE, so a malformed signal or a detector refusal fails once instead of burning the whole budget on a refusal no re-read can change. The control callback runs only after the winning commit, so no foreign callback executes under state custody. Recalibration cadence is an EDGE over the scored counter (`Scored − Recalibrated >= RecalibrateEvery`), never a modulo against it — a modulo silently skips every boundary a burst of ingests jumps over and fires twice where a claim and a commit interleave on the same count.
- Boundary: `Update` composes the settled ends — measured modes from `Stats/signal` `Transform.Modal`, computed modes from the caller's modal oracle over `Solver/contract` `SolveLane`, the fit from `Tensor/blas` `LevenbergMarquardt.Minimize` — pairing one-to-one greedy by complex MAC (magnitude AND phase) under the MAC floor so a spurious FDD peak never calibrates a parameter and no computed mode pairs twice; pairs join on INDEX at every consumer, because a float frequency is a lookup key only until two modes agree to the last bit. The modal oracle crosses the full FE solve, outside hyper-dual reach, so the central-difference Jacobian authored here is the black-box arm's legitimate ingress; the oracle call budget is a `ModelUpdatePolicy` column and its exhaustion returns the PARTIAL verdict marked unconverged rather than an open-ended descent over a solve that costs minutes per probe. The updated verdict rides the existing `ComputeReceipt.Fit` case (`Family` `model-update`, `Quality` the paired-MAC mean), never a new receipt surface.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AccelerationKind {
    public static readonly AccelerationKind Bvh = new("bvh", minimumChildren: 2, maximumChildren: 2);
    public static readonly AccelerationKind Octree = new("octree", minimumChildren: 1, maximumChildren: 8);

    public int MinimumChildren { get; }
    public int MaximumChildren { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard = new("hard", severity: 2);
    public static readonly ClashKind Clearance = new("clearance", severity: 1);
    public static readonly ClashKind Duplicate = new("duplicate", severity: 0);

    public int Severity { get; }
}

public sealed record AccelerationStructure(AccelerationKind Kind, ReadOnlyMemory<float> Bounds, ReadOnlyMemory<long> Nodes, int BuildParameter);

public sealed record ClashPolicy(double ClearanceThreshold, double DuplicateTolerance, double CoplanarCosine, Seq<ClashKind> Admitted, int MaxPairs) {
    public static readonly ClashPolicy Canonical = new(ClearanceThreshold: 0.025, DuplicateTolerance: 1e-4, CoplanarCosine: 0.999, Admitted: Seq<ClashKind>(), MaxPairs: 1 << 20);

    public bool Admits(ClashKind kind) => Admitted.IsEmpty || Admitted.Contains(kind);

    public bool Invalid =>
        !double.IsFinite(ClearanceThreshold) || ClearanceThreshold < 0.0
        || !double.IsFinite(DuplicateTolerance) || DuplicateTolerance < 0.0 || DuplicateTolerance > ClearanceThreshold
        || !double.IsFinite(CoplanarCosine) || CoplanarCosine is <= 0.0 or > 1.0
        || MaxPairs < 1;
}

public readonly record struct ClashPair(long Left, long Right, ClashKind Kind, double Clearance, bool Intersecting, Vector3 Witness);

public readonly record struct ClashSurvey(Seq<ClashPair> Pairs, int Candidates, bool Truncated);

public sealed record TwinSignal(string SignalId, ImmutableArray<double> OperatingPoint, double Measured, Instant At) {
    public bool Invalid => string.IsNullOrWhiteSpace(SignalId) || OperatingPoint.IsDefaultOrEmpty || !OperatingPoint.All(double.IsFinite) || !double.IsFinite(Measured);
}

public sealed record TwinPolicy(double ControlGain, int WindowCapacity) {
    public static readonly TwinPolicy Canonical = new(ControlGain: 1.0, WindowCapacity: 64);

    public bool Invalid => !double.IsFinite(ControlGain) || ControlGain < 0.0;
}


public sealed record TwinVerdict(string SignalId, double Predicted, double Measured, double Residual, double Score, bool Anomaly, double ControlDelta, Instant At);

[Equatable]
public sealed partial record ModelUpdatePolicy([property: OrderedEquality] ImmutableArray<(double Lower, double Upper)> Bounds, double FrequencyWeight, double MacWeight, double MacFloor, int MaxOracleCalls, LmPolicy Descent) {
    public static readonly ModelUpdatePolicy Canonical = new([], FrequencyWeight: 1.0, MacWeight: 0.5, MacFloor: 0.6, MaxOracleCalls: 512, LmPolicy.Canonical);

    public bool Invalid => !double.IsFinite(FrequencyWeight) || FrequencyWeight <= 0.0 || !double.IsFinite(MacWeight) || MacWeight < 0.0
        || !double.IsFinite(MacFloor) || MacFloor is < 0.0 or > 1.0 || MaxOracleCalls < 1
        || Bounds.Any(static b => !double.IsFinite(b.Lower) || !double.IsFinite(b.Upper) || b.Lower >= b.Upper);
}

public readonly record struct ModePair(int MeasuredIndex, int ComputedIndex, double MeasuredHz, double ComputedHz, double Mac);

[Equatable]
public sealed partial record UpdateVerdict([property: OrderedEquality] ImmutableArray<double> Parameters, double Residual, int Iterations, bool Converged, bool Exhausted, int OracleCalls, Seq<ModePair> Pairs, Seq<double> Unpaired, Instant At);

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class AdmittedScene {
    AdmittedScene(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        Index = index; Triangles = triangles; Policy = policy;
    }

    public AccelerationStructure Index { get; }
    public ReadOnlyMemory<float> Triangles { get; }
    public ClashPolicy Policy { get; }

    public static Fin<AdmittedScene> Of(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) =>
        ClashScale.Admit(index, triangles, policy).Map(_ => new AdmittedScene(index, triangles, policy));
}

public static class ClashScale {
    public static ClashSurvey Detect(AdmittedScene scene) =>
        NodeLinkPairs(scene.Index.Bounds, scene.Index.Nodes, scene.Triangles, scene.Policy);

    public static Fin<double> Clearance(AdmittedScene scene, Vector3 point) =>
        Finite(point)
            ? Fin.Succ(NearestTriangle(scene.Index.Bounds.Span, scene.Index.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(scene.Triangles.Span), point))
            : Fin.Fail<double>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(3L))));

    public static Fin<(double Clearance, Vector3 At)> SweptClearance(AdmittedScene scene, ReadOnlyMemory<float> path) =>
        path.IsEmpty || path.Length % 3 != 0 || !TensorPrimitives.IsFiniteAll<float>(path.Span)
            ? Fin.Fail<(double Clearance, Vector3 At)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(path.Length, 3L))))
            : Fin.Succ(Sweep(scene.Index, scene.Triangles, path));

    static (double Clearance, Vector3 At) Sweep(AccelerationStructure index, ReadOnlyMemory<float> triangles, ReadOnlyMemory<float> path) {
        ReadOnlySpan<float> bounds = index.Bounds.Span;
        ReadOnlySpan<long> nodes = index.Nodes.Span;
        ReadOnlySpan<Vector3> verts = MemoryMarshal.Cast<float, Vector3>(triangles.Span);
        ReadOnlySpan<Vector3> samples = MemoryMarshal.Cast<float, Vector3>(path.Span);
        double best = double.MaxValue;
        Vector3 at = default;
        for (int i = 0; i < samples.Length; i++) {
            double distance = NearestTriangle(bounds, nodes, verts, samples[i]);
            if (distance < best) { best = distance; at = samples[i]; }
            if (i + 1 >= samples.Length) { continue; }
            Vector3 leg = samples[i + 1] - samples[i];
            float span = leg.Length();
            if (span <= 1e-9f) { continue; }
            Option<Vector3> crossing = RayFirstHit(bounds, nodes, verts, samples[i], leg / span, span);
            if (crossing.Case is Vector3 hit) { return (0.0, hit); }
        }
        return (best, at);
    }

    public static Fin<bool> Occluded(AdmittedScene scene, Vector3 origin, Vector3 direction, float maxDistance) =>
        !Finite(origin) || !Finite(direction) || !float.IsFinite(maxDistance) || maxDistance <= 0f || direction.LengthSquared() < 1e-24f
            ? Fin.Fail<bool>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Scalar(direction.LengthSquared()))))
            : Fin.Succ(RayFirstHit(scene.Index.Bounds.Span, scene.Index.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(scene.Triangles.Span), origin, Vector3.Normalize(direction), maxDistance).IsSome);

    static Option<Vector3> RayFirstHit(ReadOnlySpan<float> bounds, ReadOnlySpan<long> nodes, ReadOnlySpan<Vector3> verts, Vector3 origin, Vector3 direction, float maxDistance) {
        int nodeCount = bounds.Length / 6, top = 0;
        Vector3 far = origin + direction * maxDistance;
        int[] stack = Descent(nodeCount);
        stack[top++] = 0;
        while (top > 0) {
            int node = stack[--top];
            if (!RaySlab(bounds.Slice(node * 6, 6), origin, direction, maxDistance)) { continue; }
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (leaf) {
                for (int s = 0; s < count; s++) {
                    int tri = (int)nodes[nodeCount + first + s];
                    Option<Vector3> hit = SegmentTriangle(origin, far, verts[3 * tri], verts[3 * tri + 1], verts[3 * tri + 2]);
                    if (hit.IsSome) { return hit; }
                }
            } else {
                for (int c = 0; c < count; c++) { stack[top++] = first + c; }
            }
        }
        return Option<Vector3>.None;
    }

    [ThreadStatic] private static int[]? descent;

    static int[] Descent(int nodeCount) =>
        descent is { } held && held.Length >= nodeCount ? held : descent = new int[Math.Max(64, nodeCount)];

    static bool RaySlab(ReadOnlySpan<float> box, Vector3 origin, Vector3 direction, float maxDistance) {
        float tNear = 0f, tFar = maxDistance;
        for (int axis = 0; axis < 3; axis++) {
            float o = Component(origin, axis), d = Component(direction, axis);
            if (MathF.Abs(d) < 1e-12f) {
                if (o < box[axis] || o > box[3 + axis]) { return false; }
                continue;
            }
            float inv = 1f / d;
            float t1 = (box[axis] - o) * inv, t2 = (box[3 + axis] - o) * inv;
            if (t1 > t2) { (t1, t2) = (t2, t1); }
            tNear = MathF.Max(tNear, t1);
            tFar = MathF.Min(tFar, t2);
            if (tNear > tFar) { return false; }
        }
        return true;
    }

    static float Component(Vector3 value, int axis) => axis switch { 0 => value.X, 1 => value.Y, _ => value.Z };

    public static ComputeReceipt.Clash Receipt(AccelerationStructure index, ClashSurvey survey, CorrelationId correlation, Duration elapsed) =>
        new(index.Kind, survey.Candidates,
            survey.Pairs.Count(static pair => pair.Kind == ClashKind.Hard),
            survey.Pairs.Count(static pair => pair.Kind == ClashKind.Clearance),
            survey.Pairs.Count) {
            Truncated = survey.Truncated,
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Interactive, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    const int ChildShift = 21;
    const long ChildMask = (1L << ChildShift) - 1;

    internal static Fin<Unit> Admit(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        ReadOnlyMemory<float> boundsMem = index.Bounds;
        ReadOnlyMemory<long> nodesMem = index.Nodes;
        if (policy.Invalid)
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
        if (index.BuildParameter <= 0)
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(index.BuildParameter))));
        if (boundsMem.Length % 6 != 0 || boundsMem.Length == 0)
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(boundsMem.Length, 6L))));
        if (triangles.Length == 0 || triangles.Length % 9 != 0)
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Alignment(triangles.Length, 9L))));
        int nodeCount = boundsMem.Length / 6, triCount = triangles.Length / 9;
        if (nodesMem.Length < nodeCount)
            return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(nodesMem.Length, nodeCount))));
        ReadOnlySpan<float> bounds = boundsMem.Span;
        ReadOnlySpan<long> nodes = nodesMem.Span;
        ReadOnlySpan<Vector3> vertices = MemoryMarshal.Cast<float, Vector3>(triangles.Span);
        for (int node = 0; node < nodeCount; node++) {
            ReadOnlySpan<float> box = bounds.Slice(node * 6, 6);
            if (!TensorPrimitives.IsFiniteAll<float>(box) || box[0] > box[3] || box[1] > box[4] || box[2] > box[5])
                return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Index(node, nodeCount))));
        }
        for (int triangle = 0; triangle < triCount; triangle++) {
            Vector3 a = vertices[3 * triangle], b = vertices[3 * triangle + 1], c = vertices[3 * triangle + 2];
            if (!Finite(a) || !Finite(b) || !Finite(c) || Vector3.Cross(b - a, c - a).LengthSquared() <= 1e-24f)
                return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Index(triangle, triCount))));
        }
        byte[] claimed = new byte[nodeCount];
        for (int node = 0; node < nodeCount; node++) {
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (!leaf) {
                if (count < index.Kind.MinimumChildren || count > index.Kind.MaximumChildren || first < 0 || first + count > nodeCount || (first <= node && node < first + count))
                    return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Extent(first, count, nodeCount))));
                for (int child = 0; child < count; child++) {
                    if (first + child == 0 || claimed[first + child] != 0)
                        return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Index(first + child, nodeCount))));
                    claimed[first + child] = 1;
                }
                continue;
            }
            if (count <= 0 || first < 0 || nodeCount + first + count > nodes.Length)
                return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Extent(first, count, nodes.Length - nodeCount))));
            for (int s = 0; s < count; s++) {
                long primitive = nodes[nodeCount + first + s];
                if (primitive < 0 || primitive >= triCount)
                    return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Range(RangeRequirement.WithinBounds, new ScalarEvidence.Interval(primitive, 0d, triCount - 1d))));
            }
        }
        return Acyclic(nodes, nodeCount);
    }

    static Fin<Unit> Acyclic(ReadOnlySpan<long> nodes, int nodeCount) {
        byte[] state = new byte[nodeCount];
        Stack<(int Node, bool Exit)> stack = new();
        stack.Push((0, false));
        while (stack.TryPop(out (int Node, bool Exit) frame)) {
            if (frame.Exit) { state[frame.Node] = 2; continue; }
            if (state[frame.Node] == 1) { return Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Cycle(new GraphWitness.Node(frame.Node)))); }
            if (state[frame.Node] == 2) { continue; }
            state[frame.Node] = 1;
            stack.Push((frame.Node, true));
            (bool leaf, int first, int count) = Decode(nodes[frame.Node]);
            if (!leaf) {
                for (int child = count - 1; child >= 0; child--) { stack.Push((first + child, false)); }
            }
        }
        return state.Any(static value => value == 0)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Reachable, new ContractEvidence.None())))
            : Fin.Succ(unit);
    }

    static bool Finite(Vector3 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    static ClashSurvey NodeLinkPairs(ReadOnlyMemory<float> boundsMem, ReadOnlyMemory<long> nodesMem, ReadOnlyMemory<float> trianglesMem, ClashPolicy policy) {
        ReadOnlySpan<float> bounds = boundsMem.Span;
        ReadOnlySpan<long> nodes = nodesMem.Span;
        ReadOnlySpan<Vector3> verts = MemoryMarshal.Cast<float, Vector3>(trianglesMem.Span);
        int nodeCount = bounds.Length / 6, candidates = 0, written = 0;
        bool truncated = false;
        using ArrayPoolBufferWriter<ClashPair> sink = new();
        Stack<(int L, int R)> stack = new();
        stack.Push((0, 0));
        while (stack.Count > 0 && !truncated) {
            (int l, int r) = stack.Pop();
            if (l > r) { (l, r) = (r, l); }
            if (!BoxOverlap(bounds.Slice(l * 6, 6), bounds.Slice(r * 6, 6), policy.ClearanceThreshold)) { continue; }
            (bool lLeaf, int lFirst, int lCount) = Decode(nodes[l]);
            (bool rLeaf, int rFirst, int rCount) = Decode(nodes[r]);
            if (lLeaf && rLeaf) {
                for (int a = 0; a < lCount && !truncated; a++) {
                    int left = (int)nodes[nodeCount + lFirst + a];
                    (Vector3, Vector3, Vector3) triLeft = (verts[3 * left], verts[3 * left + 1], verts[3 * left + 2]);
                    for (int b = 0; b < rCount; b++) {
                        int right = (int)nodes[nodeCount + rFirst + b];
                        if (l == r && right <= left) { continue; }
                        candidates++;
                        (Vector3, Vector3, Vector3) triRight = (verts[3 * right], verts[3 * right + 1], verts[3 * right + 2]);
                        if (Classify(left, right, triLeft, triRight, policy) is { IsSome: true, Case: ClashPair pair }) {
                            sink.GetSpan(1)[0] = pair;
                            sink.Advance(1);
                            if (++written >= policy.MaxPairs) { truncated = true; break; }
                        }
                    }
                }
            } else if (l == r) {
                for (int leftChild = 0; leftChild < lCount; leftChild++) {
                    for (int rightChild = leftChild; rightChild < lCount; rightChild++) { stack.Push((lFirst + leftChild, lFirst + rightChild)); }
                }
            } else if (rLeaf || (!lLeaf && Diagonal(bounds.Slice(l * 6, 6)) >= Diagonal(bounds.Slice(r * 6, 6)))) {
                for (int c = 0; c < lCount; c++) { stack.Push((lFirst + c, r)); }
            } else {
                for (int c = 0; c < rCount; c++) { stack.Push((l, rFirst + c)); }
            }
        }
        return new ClashSurvey(toSeq(sink.WrittenSpan.ToArray()), candidates, truncated);
    }

    static Option<ClashPair> Classify(long left, long right, (Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) {
        (double clearance, bool intersecting, Vector3 witness) = Separation(a, b);
        return Kind(clearance, intersecting, a, b, policy)
            .Filter(policy.Admits)
            .Map(kind => kind == ClashKind.Hard
                ? new ClashPair(left, right, kind, 0.0, true, witness)
                : new ClashPair(left, right, kind, clearance, intersecting, witness));
    }

    static Option<ClashKind> Kind(double clearance, bool intersecting, (Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) =>
        clearance <= policy.DuplicateTolerance && Coincident(a, b, policy) ? Some(ClashKind.Duplicate)
        : intersecting ? Some(ClashKind.Hard)
        : clearance <= policy.ClearanceThreshold ? Some(ClashKind.Clearance)
        : Option<ClashKind>.None;

    static (double Clearance, bool Intersecting, Vector3 Witness) Separation((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b) {
        Span<Vector3> va = [a.A, a.B, a.C, a.A];
        Span<Vector3> vb = [b.A, b.B, b.C, b.A];
        for (int e = 0; e < 3; e++) {
            Option<Vector3> hitA = SegmentTriangle(va[e], va[e + 1], b.A, b.B, b.C);
            if (hitA.IsSome) { return (0.0, true, hitA.IfNone(Vector3.Zero)); }
            Option<Vector3> hitB = SegmentTriangle(vb[e], vb[e + 1], a.A, a.B, a.C);
            if (hitB.IsSome) { return (0.0, true, hitB.IfNone(Vector3.Zero)); }
        }
        (float Distance, Vector3 Witness) best = (float.MaxValue, default);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                best = Closer(best, SegmentSegment(va[i], va[i + 1], vb[j], vb[j + 1]));
        best = Closer(best, PointTriangle(a.A, b.A, b.B, b.C));
        best = Closer(best, PointTriangle(a.B, b.A, b.B, b.C));
        best = Closer(best, PointTriangle(a.C, b.A, b.B, b.C));
        best = Closer(best, PointTriangle(b.A, a.A, a.B, a.C));
        best = Closer(best, PointTriangle(b.B, a.A, a.B, a.C));
        best = Closer(best, PointTriangle(b.C, a.A, a.B, a.C));
        return (best.Distance, false, best.Witness);
    }

    static (float Distance, Vector3 Witness) Closer((float Distance, Vector3 Witness) x, (float Distance, Vector3 Witness) y) =>
        y.Distance < x.Distance ? y : x;

    static bool Coincident((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) {
        Vector3 na = Vector3.Cross(a.B - a.A, a.C - a.A), nb = Vector3.Cross(b.B - b.A, b.C - b.A);
        float la = na.Length(), lb = nb.Length();
        if (la < 1e-12f || lb < 1e-12f || MathF.Abs(Vector3.Dot(na, nb) / (la * lb)) < (float)policy.CoplanarCosine) { return false; }
        float limit = (float)policy.DuplicateTolerance;
        return PointTriangle(a.A, b.A, b.B, b.C).Distance <= limit
            && PointTriangle(a.B, b.A, b.B, b.C).Distance <= limit
            && PointTriangle(a.C, b.A, b.B, b.C).Distance <= limit
            && PointTriangle(b.A, a.A, a.B, a.C).Distance <= limit
            && PointTriangle(b.B, a.A, a.B, a.C).Distance <= limit
            && PointTriangle(b.C, a.A, a.B, a.C).Distance <= limit;
    }

    static Option<Vector3> SegmentTriangle(Vector3 p, Vector3 q, Vector3 v0, Vector3 v1, Vector3 v2) {
        Vector3 dir = q - p, e1 = v1 - v0, e2 = v2 - v0, pv = Vector3.Cross(dir, e2);
        float det = Vector3.Dot(e1, pv);
        if (MathF.Abs(det) < 1e-9f) { return Option<Vector3>.None; }
        float inv = 1f / det;
        Vector3 tv = p - v0;
        float u = Vector3.Dot(tv, pv) * inv;
        if (u < 0f || u > 1f) { return Option<Vector3>.None; }
        Vector3 qv = Vector3.Cross(tv, e1);
        float v = Vector3.Dot(dir, qv) * inv;
        if (v < 0f || u + v > 1f) { return Option<Vector3>.None; }
        float t = Vector3.Dot(e2, qv) * inv;
        return t >= 0f && t <= 1f ? Some(p + t * dir) : Option<Vector3>.None;
    }

    static (float Distance, Vector3 Closest) PointTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
        Vector3 ab = b - a, ac = c - a, ap = p - a;
        float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) { return (Vector3.Distance(p, a), a); }
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) { return (Vector3.Distance(p, b), b); }
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f) { Vector3 q = a + d1 / (d1 - d3) * ab; return (Vector3.Distance(p, q), q); }
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) { return (Vector3.Distance(p, c), c); }
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f) { Vector3 q = a + d2 / (d2 - d6) * ac; return (Vector3.Distance(p, q), q); }
        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && d4 - d3 >= 0f && d5 - d6 >= 0f) { Vector3 q = b + (d4 - d3) / (d4 - d3 + (d5 - d6)) * (c - b); return (Vector3.Distance(p, q), q); }
        float denom = 1f / (va + vb + vc);
        Vector3 r = a + ab * (vb * denom) + ac * (vc * denom);
        return (Vector3.Distance(p, r), r);
    }

    static (float Distance, Vector3 Witness) SegmentSegment(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2) {
        Vector3 d1 = q1 - p1, d2 = q2 - p2, r = p1 - p2;
        float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
        float s, t;
        if (a <= 1e-12f && e <= 1e-12f) { s = 0f; t = 0f; }
        else if (a <= 1e-12f) { s = 0f; t = Math.Clamp(f / e, 0f, 1f); }
        else {
            float c = Vector3.Dot(d1, r);
            if (e <= 1e-12f) { t = 0f; s = Math.Clamp(-c / a, 0f, 1f); }
            else {
                float b = Vector3.Dot(d1, d2), denom = a * e - b * b;
                s = denom > 1e-12f ? Math.Clamp((b * f - c * e) / denom, 0f, 1f) : 0f;
                t = (b * s + f) / e;
                if (t < 0f) { t = 0f; s = Math.Clamp(-c / a, 0f, 1f); }
                else if (t > 1f) { t = 1f; s = Math.Clamp((b - c) / a, 0f, 1f); }
            }
        }
        Vector3 c1 = p1 + d1 * s, c2 = p2 + d2 * t;
        return (Vector3.Distance(c1, c2), (c1 + c2) * 0.5f);
    }

    static double NearestTriangle(ReadOnlySpan<float> bounds, ReadOnlySpan<long> nodes, ReadOnlySpan<Vector3> verts, Vector3 point) {
        int nodeCount = bounds.Length / 6, top = 0;
        double best = double.MaxValue;
        int[] stack = Descent(nodeCount);
        stack[top++] = 0;
        while (top > 0) {
            int node = stack[--top];
            if (BoxDistance(bounds.Slice(node * 6, 6), point) >= best) { continue; }
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (leaf) {
                for (int s = 0; s < count; s++) {
                    int tri = (int)nodes[nodeCount + first + s];
                    (float distance, _) = PointTriangle(point, verts[3 * tri], verts[3 * tri + 1], verts[3 * tri + 2]);
                    if (distance < best) { best = distance; }
                }
            } else {
                for (int c = 0; c < count; c++) { stack[top++] = first + c; }
            }
        }
        return best;
    }

    static (bool Leaf, int First, int Count) Decode(long descriptor) =>
        descriptor < 0
            ? (true, (int)((-(descriptor + 1)) >> ChildShift), (int)((-(descriptor + 1)) & ChildMask))
            : (false, (int)(descriptor >> ChildShift), (int)(descriptor & ChildMask));

    static double Diagonal(ReadOnlySpan<float> box) {
        float dx = box[3] - box[0], dy = box[4] - box[1], dz = box[5] - box[2];
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    static double BoxDistance(ReadOnlySpan<float> box, Vector3 p) {
        float dx = MathF.Max(0f, MathF.Max(box[0] - p.X, p.X - box[3]));
        float dy = MathF.Max(0f, MathF.Max(box[1] - p.Y, p.Y - box[4]));
        float dz = MathF.Max(0f, MathF.Max(box[2] - p.Z, p.Z - box[5]));
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    static bool BoxOverlap(ReadOnlySpan<float> a, ReadOnlySpan<float> b, double margin) {
        for (int axis = 0; axis < 3; axis++) {
            if (a[axis] - margin > b[3 + axis] || b[axis] - margin > a[3 + axis]) { return false; }
        }
        return true;
    }
}

// --- [TWIN] ----------------------------------------------------------------------------

public static class DigitalTwin {
    public static Fin<(TwinVerdict Verdict, ResidualWindow Window)> Score(
        Surrogate baseline, TwinSignal signal, ResidualWindow window, Func<Matrix<double>, Fin<Prediction>> detector, TwinPolicy policy, IClock clock) {
        if (signal.Invalid || policy.Invalid) { return Fin.Fail<(TwinVerdict, ResidualWindow)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))); }
        return baseline.Predict(new DesignPoint(signal.OperatingPoint, [], [])).Bind(prediction => {
            if (prediction.Values.Count != 1 || !prediction.Values.ForAll(double.IsFinite) || !double.IsFinite(prediction.Bound) || prediction.Bound < 0.0) {
                return Fin.Fail<(TwinVerdict, ResidualWindow)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(prediction.Values.Count, 1L))));
            }
            double predicted = prediction.Values[0];
            double residual = predicted - signal.Measured;
            if (!double.IsFinite(residual)) { return Fin.Fail<(TwinVerdict, ResidualWindow)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Value(residual)))); }
            ResidualWindow next = window.Push(residual);
            return detector(next.Evidence).Bind(outcome => outcome is Prediction.Anomaly anomaly && anomaly.Scores.Count == next.Count
                ? Fin.Succ((new TwinVerdict(
                    signal.SignalId, predicted, signal.Measured, residual,
                    anomaly.Scores[anomaly.Scores.Count - 1], anomaly.Changes[^1],
                    anomaly.Changes[^1] ? -policy.ControlGain * residual : 0.0,
                    clock.GetCurrentInstant()), next))
                : Fin.Fail<(TwinVerdict, ResidualWindow)>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.None()))));
        });
    }

    public static ComputeReceipt.Twin Receipt(TwinVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new(verdict.SignalId, verdict.Predicted, verdict.Measured, verdict.Residual, verdict.Anomaly, verdict.ControlDelta) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Interactive, Substrate.CpuTensor, AllocationClass.SpanStack, elapsed),
        };

    public static Fin<UpdateVerdict> Update(
        Func<ImmutableArray<double>, Fin<Seq<(double FrequencyHz, ReadOnlyMemory<double> Shape)>>> modalOracle,
        Seq<MeasuredMode> measured,
        ImmutableArray<double> seed,
        ModelUpdatePolicy policy,
        IClock clock) {
        if (policy.Invalid || measured.IsEmpty || seed.IsDefaultOrEmpty || !seed.All(double.IsFinite)
            || (!policy.Bounds.IsDefaultOrEmpty && policy.Bounds.Length != seed.Length)) {
            return Fin.Fail<UpdateVerdict>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
        }
        Func<Vector<double>, Vector<double>> boxed = parameters => Boxed(parameters, policy.Bounds);
        int rows = 2 * measured.Count, calls = 0;
        Option<Error> probeFault = None;
        Vector<double> lastResidual = Vector<double>.Build.Dense(rows);
        Matrix<double> lastJacobian = Matrix<double>.Build.Dense(rows, seed.Length);
        bool Spent => probeFault.IsSome || calls >= policy.MaxOracleCalls;
        Fin<Vector<double>> Residual(Vector<double> parameters) {
            if (Spent) { return Fin.Succ(lastResidual); }
            calls++;
            return modalOracle([.. boxed(parameters)])
                .Bind(computed => Stacked(measured, computed, policy))
                .Map(stacked => lastResidual = stacked);
        }
        return Residual(Vector<double>.Build.DenseOfArray([.. seed])).Bind(_ =>
            LevenbergMarquardt.Minimize(
                p => Residual(p).Match(Succ: r => r, Fail: e => { probeFault = probeFault | Some(e); return lastResidual; }),
                p => Jacobian(Residual, p, rows).Match(Succ: j => lastJacobian = j, Fail: e => { probeFault = probeFault | Some(e); return lastJacobian; }),
                Vector<double>.Build.DenseOfArray([.. seed]),
                policy.Descent)
            .Bind(fit => probeFault.Case is Error fault
                ? Fin.Fail<UpdateVerdict>(fault)
                : modalOracle([.. boxed(fit.Iterate)]).Bind(computed =>
                    Pairs(measured, computed, policy).Map(indexed => {
                        Seq<ModePair> pairs = indexed.Map(pair =>
                            new ModePair(pair.MeasuredIndex, pair.ComputedIndex, measured[pair.MeasuredIndex].FrequencyHz, computed[pair.ComputedIndex].FrequencyHz, pair.Mac));
                        Set<int> paired = toSet(pairs.Map(static pair => pair.MeasuredIndex));
                        Seq<double> unpaired = measured
                            .Map(static (mode, index) => (Index: index, mode.FrequencyHz))
                            .Filter(row => !paired.Contains(row.Index))
                            .Map(static row => row.FrequencyHz);
                        return new UpdateVerdict([.. boxed(fit.Iterate)], fit.Residual, fit.Steps,
                            fit.Termination is SolveTermination.Converged && calls < policy.MaxOracleCalls,
                            calls >= policy.MaxOracleCalls, calls,
                            pairs, unpaired, clock.GetCurrentInstant());
                    }))));
    }

    static Vector<double> Boxed(Vector<double> parameters, ImmutableArray<(double Lower, double Upper)> bounds) =>
        bounds.IsDefaultOrEmpty
            ? parameters
            : Vector<double>.Build.Dense(parameters.Count, index => Math.Clamp(parameters[index], bounds[index].Lower, bounds[index].Upper));

    public static ComputeReceipt.Fit Receipt(UpdateVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new("model-update", "levenberg-marquardt", verdict.Parameters.Length, verdict.Iterations, verdict.Residual, verdict.Converged,
            verdict.Pairs.IsEmpty ? 0.0 : verdict.Pairs.Sum(static pair => pair.Mac) / verdict.Pairs.Count, "mac-mean", verdict.Pairs.Count) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static Fin<Vector<double>> Stacked(Seq<MeasuredMode> measured, Seq<(double FrequencyHz, ReadOnlyMemory<double> Shape)> computed, ModelUpdatePolicy policy) =>
        Pairs(measured, computed, policy).Map(pairs => {
            double[] stacked = new double[2 * measured.Count];
            HashMap<int, (int ComputedIndex, double Mac)> byMeasured =
                pairs.Fold(HashMap<int, (int, double)>(), static (map, row) => map.AddOrUpdate(row.MeasuredIndex, (row.ComputedIndex, row.Mac)));
            for (int index = 0; index < measured.Count; index++) {
                Option<(int ComputedIndex, double Mac)> pair = byMeasured.Find(index);
                double measuredHz = measured[index].FrequencyHz;
                stacked[2 * index] = policy.FrequencyWeight * pair.Match(
                    Some: row => (computed[row.ComputedIndex].FrequencyHz - measuredHz) / Math.Max(1e-9, measuredHz),
                    None: () => 1.0);
                stacked[2 * index + 1] = policy.MacWeight * pair.Match(Some: static row => 1.0 - row.Mac, None: () => 1.0);
            }
            return Vector<double>.Build.DenseOfArray(stacked);
        });

    static Fin<Seq<(int MeasuredIndex, int ComputedIndex, double Mac)>> Pairs(Seq<MeasuredMode> measured, Seq<(double FrequencyHz, ReadOnlyMemory<double> Shape)> computed, ModelUpdatePolicy policy) {
        Seq<(int Measured, int Computed)> mismatched = toSeq(
            from mode in measured
            from candidate in computed
            where candidate.Shape.Length != mode.ShapeMagnitude.Length
            select (Measured: mode.ShapeMagnitude.Length, Computed: candidate.Shape.Length));
        if (!mismatched.IsEmpty) {
            return Fin.Fail<Seq<(int, int, double)>>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Shape(
                ShapeRequirement.Arity,
                new ShapeEvidence.Count(mismatched.Head.Computed, mismatched.Head.Measured))));
        }
        Seq<(int MeasuredIndex, int ComputedIndex, double Mac)> ranked = toSeq(
            from m in Enumerable.Range(0, measured.Count)
            from c in Enumerable.Range(0, computed.Count)
            let mac = Mac(measured[m], computed[c].Shape.Span)
            where mac >= policy.MacFloor
            orderby mac descending
            select (m, c, mac));
        return Fin.Succ(ranked
            .Fold((Measured: Set<int>(), Computed: Set<int>(), Pairs: Seq<(int, int, double)>()),
                static (taken, row) => taken.Measured.Contains(row.MeasuredIndex) || taken.Computed.Contains(row.ComputedIndex)
                    ? taken
                    : (taken.Measured.Add(row.MeasuredIndex), taken.Computed.Add(row.ComputedIndex), taken.Pairs.Add(row)))
            .Pairs);
    }

    static double Mac(MeasuredMode mode, ReadOnlySpan<double> computed) {
        ReadOnlySpan<double> magnitude = mode.ShapeMagnitude.Span, phase = mode.ShapePhase.Span;
        double re = 0.0, im = 0.0, measuredNorm = 0.0;
        for (int channel = 0; channel < magnitude.Length; channel++) {
            re += magnitude[channel] * Math.Cos(phase[channel]) * computed[channel];
            im -= magnitude[channel] * Math.Sin(phase[channel]) * computed[channel];
            measuredNorm += magnitude[channel] * magnitude[channel];
        }
        double denominator = measuredNorm * TensorPrimitives.SumOfSquares(computed);
        return denominator < 1e-300 ? 0.0 : (re * re + im * im) / denominator;
    }

    static Fin<Matrix<double>> Jacobian(Func<Vector<double>, Fin<Vector<double>>> residual, Vector<double> at, int rows) =>
        toSeq(Enumerable.Range(0, at.Count)).Fold(
            Fin.Succ(Matrix<double>.Build.Dense(rows, at.Count)),
            (acc, column) => acc.Bind(jacobian => {
                double step = 1e-6 * Math.Max(1.0, Math.Abs(at[column]));
                Vector<double> forward = at.Clone(); forward[column] += step;
                Vector<double> backward = at.Clone(); backward[column] -= step;
                return (from f in residual(forward) from b in residual(backward) select (f - b) / (2.0 * step))
                    .Map(delta => { jacobian.SetColumn(column, delta); return jacobian; });
            }));
}

// --- [TWIN_LOOP] -----------------------------------------------------------------------

public sealed record TwinLoopPolicy(TwinPolicy Scoring, int RecalibrateEvery, int CommitAttempts) {
    public static readonly TwinLoopPolicy Canonical = new(TwinPolicy.Canonical, RecalibrateEvery: 256, CommitAttempts: 8);

    public bool Invalid => Scoring.Invalid || RecalibrateEvery < 8 || CommitAttempts < 1;
}

public sealed class TwinLoop {
    private readonly record struct TwinState(ResidualWindow Window, Seq<(double Residual, bool Anomaly)> Segment, long Scored, long Recalibrated, bool Claimed, long Revision);

    private readonly Surrogate baseline;
    private readonly Func<Matrix<double>, Fin<Prediction>> detector;
    private readonly TwinLoopPolicy policy;
    private readonly Func<TwinVerdict, Fin<TwinVerdict>> suggested;
    private readonly IClock clock;
    private readonly Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive;
    private readonly Atom<TwinState> held;

    private TwinLoop(Surrogate baseline, Func<Matrix<double>, Fin<Prediction>> detector, TwinLoopPolicy policy, WindowCapacity capacity, Func<TwinVerdict, Fin<TwinVerdict>> suggested, IClock clock, Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive) {
        (this.baseline, this.detector, this.policy, this.suggested, this.clock, this.archive) = (baseline, detector, policy, suggested, clock, archive);
        held = Atom(new TwinState(ResidualWindow.Of(capacity), Seq<(double, bool)>(), Scored: 0L, Recalibrated: 0L, Claimed: false, Revision: 0L));
    }

    public static Fin<TwinLoop> Of(
        Surrogate baseline,
        Func<Matrix<double>, Fin<Prediction>> detector,
        TwinLoopPolicy policy,
        Func<TwinVerdict, Fin<TwinVerdict>> suggested,
        IClock clock,
        Option<(Func<Stream> Sink, HdfArchivePolicy Policy)> archive = default) =>
        policy.Invalid
            ? Fin.Fail<TwinLoop>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
            : WindowCapacity.From(policy.Scoring.WindowCapacity)
                .Map(capacity => new TwinLoop(baseline, detector, policy, capacity, suggested, clock, archive));

    public IO<Fin<bool>> ClaimRecalibration() {
        TwinState next = held.Swap(state => {
            bool due = state.Scored - state.Recalibrated >= policy.RecalibrateEvery;
            return state with {
                Recalibrated = due ? state.Scored : state.Recalibrated,
                Segment = due ? Seq<(double, bool)>() : state.Segment,
                Claimed = due,
            };
        });
        return !next.Claimed || archive.IsNone
            ? IO.pure(Fin.Succ(next.Claimed))
            : Seal(next).Map(static sealed_ => sealed_.Map(static _ => true));
    }

    private IO<Fin<Unit>> Seal(TwinState claimed) =>
        archive.Match(
            None: () => IO.pure(Fin.Succ(unit)),
            Some: capability => {
                double[] residuals = [.. claimed.Segment.Map(static row => row.Residual)];
                byte[] anomalies = [.. claimed.Segment.Map(static row => row.Anomaly ? (byte)1 : (byte)0)];
                ChunkGrid grid = ChunkGrid.Derive([residuals.Length], components: 1, targetChunkElements: policy.RecalibrateEvery);
                ArchiveSlot<double> residualSlot = new("residuals", grid);
                ArchiveSlot<byte> anomalySlot = new("anomalies", grid);
                return ArchiveSession.Write(
                    capability.Sink(), capability.Policy,
                    Seq<IArchiveSlot>(residualSlot, anomalySlot),
                    Seq(("scored", (ArchiveAttribute)new ArchiveAttribute.Whole(claimed.Scored)),
                        ("recalibrate-every", new ArchiveAttribute.Whole(policy.RecalibrateEvery)),
                        ("sealed-at", new ArchiveAttribute.Whole(clock.GetCurrentInstant().ToUnixTimeTicks()))),
                    session =>
                        IO.pure(from residualCursor in session.Cursor(residualSlot)
                                from anomalyCursor in session.Cursor(anomalySlot)
                                from _residuals in residualCursor.Write(residuals)
                                from _anomalies in anomalyCursor.Write(anomalies)
                                select unit));
            });

    public static Fin<TwinSignal> Admit(SensorReading<TwinSignal> reading) =>
        !reading.Data.Invalid
            ? Fin.Succ(reading.Data)
            : Fin.Fail<TwinSignal>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(reading.Envelope.Id))));

    public IO<Fin<TwinVerdict>> Ingest(SensorReading<TwinSignal> reading) =>
        Admit(reading).Match(
            Succ: signal => Score(signal, reading.Sampled),
            Fail: static error => IO.pure(Fin.Fail<TwinVerdict>(error)));

    private IO<Fin<TwinVerdict>> Score(TwinSignal signal, int weight) =>
        IO.lift(() => Attempt(signal, weight))
            .Bind(static settled => settled.Match(Succ: IO.pure, Fail: IO.fail<TwinVerdict>))
            .RetryWhile(Schedule.recurs(Math.Max(1, policy.CommitAttempts) - 1), static error => error is ComputeFault.RetryOwnerConflict)
            .Try()
            .Map(settled => settled.Bind(won => won.Anomaly ? suggested(won) : Fin.Succ(won)));

    private Fin<TwinVerdict> Attempt(TwinSignal signal, int weight) {
        TwinState snapshot = held.Value;
        return DigitalTwin.Score(baseline, signal, snapshot.Window, detector, policy.Scoring, clock).Bind(scored =>
            Cell.Step(
                held,
                state => state.Revision == snapshot.Revision
                    ? Some(state with {
                        Window = scored.Window,
                        Segment = state.Segment.Add((scored.Window.Residuals.Last, scored.Verdict.Anomaly)),
                        Scored = state.Scored + weight,
                        Claimed = false,
                        Revision = state.Revision + 1L,
                    })
                    : Option<TwinState>.None,
                new ComputeFault.RetryOwnerConflict($"<twin-contention:{snapshot.Revision}>")) switch {
                Transition<TwinState>.Committed => Fin.Succ(scored.Verdict),
                Transition<TwinState>.Refused refused => Fin.Fail<TwinVerdict>(refused.Cause),
                var other => Fin.Fail<TwinVerdict>(new ComputeFault.RetryOwnerConflict($"<twin-contention:{other.Current.Revision}>")),
            });
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
