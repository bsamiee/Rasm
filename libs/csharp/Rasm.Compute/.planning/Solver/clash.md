# [COMPUTE_CLASH]

Rasm.Compute solver clash confirms collisions and scores live telemetry: `ClashScale` is the narrow-phase collision-confirmation fold over the geometry-owned node-link broad-phase wire, and `DigitalTwin` scores a live signal against a `Surrogate` baseline through the injected `Stats/estimator` changepoint detector over a bounded residual window. `AccelerationStructure` is one decoded wire record parameterized by `AccelerationKind`; `Spatial.Apply(SpatialOp.Wire)` remains the sole builder/refitter.

Narrow-phase triangle and closest-distance work rides `System.Numerics.Vector3` hardware vectors over a `MemoryMarshal.Cast<float, Vector3>` view of the federated triangle wire; the twin baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` evaluated through `Surrogate.Predict` over a `DesignPoint`, so the twin and the design search share one reduced-order model. `DigitalTwin.Update` closes the twin loop's model end: the `Stats/signal#SIGNAL_LANE` `MeasuredMode` set calibrates the FE stiffness/mass parameter vector through the `Tensor/blas#DENSE_ALGEBRA` `LevenbergMarquardt.Minimize` black-box arm, so an FE model that never reconciles with measured dynamics stops being the twin's baseline. `ComputeReceipt`, `WorkLane`, `CorrelationId`, NodaTime `IClock` (the App-owned `ClockPolicy` stays at composition), and the Thinktecture `ComparerAccessors.StringOrdinal` accessor arrive settled. Candidate `ClashPair` sets feed the `Model/inference#INFERENCE_MODES` `ClashScore` false-positive filter; a twin control suggestion crosses to the AppHost `Wire/livewire#WRITE_BACK` outbound write-back as a receipted `ExternalValue`. Page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[CLASH_AND_TWIN]: node-link narrow-phase collision confirmation and clearance descent; detector-composed ROM digital-twin loop; measured-mode FE model updating.

## [02]-[CLASH_AND_TWIN]

- Owner: `AccelerationKind` `[SmartEnum<string>]` carries BVH/octree child arity; `AccelerationStructure` carries one decoded `(Bounds, Nodes, BuildParameter)` wire; `ClashKind` classifies hard · clearance · duplicate; `ClashScale` owns admission, traversal, intersection, and clearance; `DigitalTwin` owns residual-window scoring composed over the Stats detector and the `Update` FE-updating fold with its `ModelUpdatePolicy`/`UpdateVerdict` carriers; `TwinLoop` owns typed `SensorEnvelope<TwinSignal>` admission, revisioned atomic held-window scoring, anomaly control through the injected `Runtime/receipts#HOOK_POINTS` veto fold, and edge-consuming recalibration cadence.
- Cases: `AccelerationKind` bvh · octree; `ClashKind` hard · clearance · duplicate; `ClashPair` carries a NON-NEGATIVE `Clearance` beside an `Intersecting` discriminant — a confirmed triangle-surface intersection is `Intersecting` at zero clearance and a clearance band is a positive distance, because the decoded surface wire carries no volumetric penetration domain and a signed scalar standing in for both reads a deeper overlap as a wider gap.
- Entry: `AdmittedScene.Of(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy)` runs the complete `Admit` traversal ONCE and mints the private-ctor scene evidence every query reads — `Fin<T>` aborts on a malformed wire (mis-aligned bounds/triangle buffer, out-of-range child/leaf range, out-of-range leaf primitive id, a node two parents claim), and a per-query re-validation is the deleted quadratic form; `Detect(scene)` returns the `ClashSurvey` of confirmed pairs beside its candidate count and truncation flag, `Clearance(scene, Vector3 point)` and `SweptClearance(scene, path)` descend the same tree for the point-to-scene nearest-surface distance and the CAM/motion swept-volume minimum clearance — each path leg also ray-tests the surface, so a crossing between samples reports zero clearance AT THE CROSSING POINT rather than at an endpoint the tool merely passed through — and `Occluded(scene, origin, direction, maxDistance)` answers the ray test over the same first-hit descent. Detection is a pure geometry fold, so the `CorrelationId`/`IClock` receipt tail enters only at `Receipt`, never as a dead entry param. Incremental edits are the kernel `SpatialOp.Refit` seam — a moved element re-bounds there and re-projects through `SpatialOp.Wire` unchanged, so a Compute-local `Insert`/`Remove` index rebuild is the rejected double-owner form. `TwinLoop.Of(baseline, detector, policy, suggested, clock)` is the validated mint — an invalid `TwinLoopPolicy` fails before any held state constructs — and `TwinLoop.Ingest(SensorEnvelope<TwinSignal> envelope)` is the typed telemetry entry; a caller enqueues each decoded envelope onto `WorkLane.CaptureIngest`, and the lane dispatch routes admitted signals here. `TwinLoop.ClaimRecalibration()` consumes each scoring-cadence edge once before composition drives `Update` with `Stats/signal` `Transform.Modal` measured modes.
- Auto: `Detect` walks the contiguous `[FirstChild, FirstChild+ChildCount)` child range as one hierarchical descent — a BVH node and an octree cell traverse identically, so the prior parallel `BvhPairs`/`OctreePairs` bodies collapse to one `NodeLinkPairs` fold and the Morton-cell decode that mis-read the node-link array as a per-element cell map is the deleted form; each overlapping leaf-pair runs the complete two-direction Möller–Trumbore test and the Ericson closest distance and bands by `ClashPolicy`. `DigitalTwin` pushes the `Surrogate` residual onto the bounded `TwinWindow` and reads the injected `Stats/estimator#ESTIMATOR_LANE` detector's last-row score and change flag into a verdict and a control suggestion — one anomaly owner, twin-local control only.
- Receipt: the `Clash` `ComputeReceipt` case carries the index kind, candidate-pair count, confirmed hard-clash and clearance-violation counts, and total pairs, projected from the `ClashSurvey` so the counts and the candidate total come from ONE traversal; the survey's truncation flag rides the carrier beside them, the same way the optimizer's exact evidence rides its own result rather than claiming a slot the receipt owner does not declare. The `Twin` case carries the signal id, predicted-versus-measured residual, detector anomaly flag, and suggested control delta, so a twin loop is auditable and a machine-control suggestion is receipted before it leaves the boundary.
- Packages: System.Numerics (`Vector3`), System.Runtime.InteropServices (`MemoryMarshal`), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter`), System.Numerics.Tensors (`TensorPrimitives.Dot`/`SumOfSquares` in the MAC), MathNet.Numerics (`Vector<double>`/`Matrix<double>` the LM contract carries), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, kernel signal capsule), BCL inbox.
- Growth: a new clash kind is one `ClashKind` row; a new twin scoring channel is one field on `TwinSignal`/`TwinVerdict`; a new updating residual term (mode-shape components, static deflections) is one weighted row pair on the `Update` stacked residual with its `ModelUpdatePolicy` weight column; a new narrow-phase band edge or coplanarity threshold is one `ClashPolicy` column; a new broad-phase kernel that still emits the node-link wire reuses `NodeLinkPairs` untouched; zero new surface — a `BvhTree`/`OctreeIndex`/`SdfField` sibling family collapses onto the one decoded `AccelerationStructure` wire and the one `NodeLinkPairs` traversal, and a standalone `ModelUpdater` service collapses onto `DigitalTwin.Update`.
- Boundary: `AccelerationStructure` is the decoded read-only node-link wire, and `AccelerationKind` validates only the builder-specific child arity. `Admit` verifies finite ordered boxes, finite nondegenerate triangles, leaf primitive ranges, child ranges, root reachability, acyclicity, and SINGLE PARENTAGE before traversal — the last proves the wire is a tree, which is exactly what makes each unordered node pair reachable by one descent path and lets `NodeLinkPairs` drop the visited set it otherwise pays per query; an acyclic wire whose child ranges overlap is a DAG that would re-expand a shared subtree, so admission refuses it by name rather than a traversal absorbing it silently.
- Boundary: `NodeLinkPairs` canonicalizes node pairs and expands equal internal nodes upper-triangular; the pair sink stops at `ClashPolicy.MaxPairs` and the survey publishes the truncation, because an unbounded sink over a degenerate scene is the memory failure a clash run must degrade around rather than die on. `Clearance` is non-negative and `Intersecting` is its own column; volumetric penetration depth requires a solid-domain carrier absent from this wire.
- Boundary: `RayFirstHit` and `NearestTriangle` run per path sample and per query point, so their descent stacks are process-static `[ThreadStatic]` scratch grown once per thread and the slab test indexes the `Vector3` components directly — a per-call `Stack<int>` and a per-axis span rebuild are the allocation the hot loop exists without. One ray descent serves both the occlusion predicate and the sweep witness, so a bool-returning twin walking the same tree for less evidence has nowhere to reappear.
- Boundary: `AccelerationStructure` carries the C#-only clash branch golden `tests/csharp/README.md` `[09]-[SNAPSHOTS]` registers, its bytes frozen at the `Rasm/Spatial/index#SPATIAL_INDEX` producer — this decoder asserts the `Decode` and leaf-tail round-trip over that producer's pinned descriptors and never re-freezes a second vector; clash pairs stay OUT of golden scope, since `NodeLinkPairs` classifies triangles under a `ClashPolicy` and the golden pins neither a triangle wire nor a policy row.
- Boundary: `DigitalTwin.Score` faults malformed signals, windows, policies, surrogate outputs, and non-Anomaly detector carriers; changepoint state, thresholds, and anomaly classification live on the injected Stats detector, and corrective control opposes the raw residual only on a flagged change. `TwinLoop.Ingest` snapshots state, runs detector scoring outside the atom, and commits only against the sampled revision; a competing commit spends one policy-owned rescore attempt, exhaustion returns typed contention, and the control callback runs only after the winning commit, so no foreign callback executes under state custody. Recalibration cadence is an EDGE over the scored counter (`Scored − Recalibrated >= RecalibrateEvery`), never a modulo against it — a modulo silently skips every boundary a burst of ingests jumps over and fires twice where a claim and a commit interleave on the same count.
- Boundary: `Update` composes the settled ends — measured modes from `Stats/signal` `Transform.Modal`, computed modes from the caller's modal oracle over `Solver/contract` `SolveLane`, the fit from `Tensor/blas` `LevenbergMarquardt.Minimize` — pairing one-to-one greedy by complex MAC (magnitude AND phase) under the MAC floor so a spurious FDD peak never calibrates a parameter and no computed mode pairs twice; pairs join on INDEX at every consumer, because a float frequency is a lookup key only until two modes agree to the last bit. The modal oracle crosses the full FE solve, outside hyper-dual reach, so the central-difference Jacobian authored here is the black-box arm's legitimate ingress; the oracle call budget is a `ModelUpdatePolicy` column and its exhaustion returns the PARTIAL verdict marked unconverged rather than an open-ended descent over a solve that costs minutes per probe. The updated verdict rides the existing `ComputeReceipt.Fit` case (`Family` `model-update`, `Quality` the paired-MAC mean), never a new receipt surface.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------------------------

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

// Every narrow-phase threshold is a COLUMN: the clearance band, the duplicate tolerance, the coplanarity cosine a
// duplicate pair must clear, and the pair ceiling a degenerate scene stops at. A literal inside the classifier is a
// policy no caller can retune and no receipt can report.
public sealed record ClashPolicy(double ClearanceThreshold, double DuplicateTolerance, double CoplanarCosine, bool HardOnly, int MaxPairs) {
    public static readonly ClashPolicy Canonical = new(ClearanceThreshold: 0.025, DuplicateTolerance: 1e-4, CoplanarCosine: 0.999, HardOnly: false, MaxPairs: 1 << 20);

    public bool Invalid =>
        !double.IsFinite(ClearanceThreshold) || ClearanceThreshold < 0.0
        || !double.IsFinite(DuplicateTolerance) || DuplicateTolerance < 0.0 || DuplicateTolerance > ClearanceThreshold
        || !double.IsFinite(CoplanarCosine) || CoplanarCosine is <= 0.0 or > 1.0
        || MaxPairs < 1;
}

// `Clearance` is the NON-NEGATIVE surface gap and `Intersecting` its own discriminant, so a confirmed overlap and a
// zero-gap touch stay distinguishable and no consumer reads a negative distance as a penetration depth this wire
// cannot measure.
public readonly record struct ClashPair(long Left, long Right, ClashKind Kind, double Clearance, bool Intersecting, Vector3 Witness);

// One traversal answers the pairs, what it examined, and whether the ceiling cut it short — so the receipt reads
// three facts off one fold instead of a caller re-counting candidates it never saw.
public readonly record struct ClashSurvey(Seq<ClashPair> Pairs, int Candidates, bool Truncated);

public sealed record TwinSignal(string SignalId, ImmutableArray<double> OperatingPoint, double Measured, Instant At) {
    public bool Invalid => string.IsNullOrWhiteSpace(SignalId) || OperatingPoint.IsDefaultOrEmpty || !OperatingPoint.All(double.IsFinite) || !double.IsFinite(Measured);
}

public sealed record TwinPolicy(double ControlGain, int WindowCapacity) {
    public static readonly TwinPolicy Canonical = new(ControlGain: 1.0, WindowCapacity: 64);

    public bool Invalid => !double.IsFinite(ControlGain) || ControlGain < 0.0 || WindowCapacity < 8;
}

// Residual evidence window: the twin holds recent residuals ONLY — smoothing, changepoint state, thresholds,
// and anomaly classification are the `Stats/estimator#ESTIMATOR_LANE` detector's (`TemporalSpec` Cusum /
// BayesianOnline / CorrelatedResidual over the fitted `EstimatorModel.Detector`); a twin-local Kalman/CUSUM
// recursion is the deleted double-owner form.
public sealed record TwinWindow(Seq<double> Residuals, int Capacity) {
    public static TwinWindow Of(TwinPolicy policy) => new(Seq<double>(), policy.WindowCapacity);

    public bool Invalid => Capacity < 8 || Residuals.Exists(static value => !double.IsFinite(value));

    public TwinWindow Push(double residual) =>
        this with { Residuals = (Residuals.Count >= Capacity ? Residuals.Tail : Residuals).Add(residual) };
}

public sealed record TwinVerdict(string SignalId, double Predicted, double Measured, double Residual, double Score, bool Anomaly, double ControlDelta, Instant At);

// FE-updating policy: parameter box, measured-vs-computed weighting, the MAC floor below which a measured mode
// refuses to pair — an unpaired mode is a residual fact, never a silently dropped row — and the oracle call budget.
// Each modal probe crosses a full FE solve, so the budget is the difference between a bounded calibration and a
// descent that spends an afternoon; exhaustion returns the partial verdict rather than a fault with no parameters.
public sealed record ModelUpdatePolicy(ImmutableArray<(double Lower, double Upper)> Bounds, double FrequencyWeight, double MacWeight, double MacFloor, int MaxOracleCalls, LmPolicy Descent) {
    public static readonly ModelUpdatePolicy Canonical = new([], FrequencyWeight: 1.0, MacWeight: 0.5, MacFloor: 0.6, MaxOracleCalls: 512, LmPolicy.Canonical);

    public bool Invalid => !double.IsFinite(FrequencyWeight) || FrequencyWeight <= 0.0 || !double.IsFinite(MacWeight) || MacWeight < 0.0
        || !double.IsFinite(MacFloor) || MacFloor is < 0.0 or > 1.0 || MaxOracleCalls < 1
        || Bounds.Any(static b => !double.IsFinite(b.Lower) || !double.IsFinite(b.Upper) || b.Lower >= b.Upper);
}

// Pairs join on INDEX everywhere the fold consumes them and carry their frequencies as EVIDENCE the verdict retains:
// the computed mode set is transient, so a reader needs both frequencies here, while every join upstream keys the
// index — a float frequency is a stable key only until two modes agree to the last bit.
public readonly record struct ModePair(int MeasuredIndex, int ComputedIndex, double MeasuredHz, double ComputedHz, double Mac);

// `Exhausted` marks the PARTIAL verdict a spent oracle budget returns: the parameters are the last projected iterate
// and `Converged` is false, so a reader tells a converged fit from a truncated one without re-deriving the call count.
public sealed record UpdateVerdict(ImmutableArray<double> Parameters, double Residual, int Iterations, bool Converged, bool Exhausted, int OracleCalls, Seq<ModePair> Pairs, Seq<double> Unpaired, Instant At);

// --- [OPERATIONS] ------------------------------------------------------------------------------------------

// Admission evidence carrier: the private-ctor scene mints from ONE full Admit traversal, and every query reads
// it — a per-query re-validation of the same wire (the prior Detect/Clearance/SweptClearance/Occluded shape,
// each re-walking admission) is the deleted quadratic form; a mutated wire re-admits by minting a new scene.
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
            : Fin.Fail<double>(ComputeFault.Create("<clash-point-nonfinite>"));

    public static Fin<(double Clearance, Vector3 At)> SweptClearance(AdmittedScene scene, ReadOnlyMemory<float> path) =>
        path.IsEmpty || path.Length % 3 != 0 || !path.Span.ToArray().All(float.IsFinite)
            ? Fin.Fail<(double Clearance, Vector3 At)>(ComputeFault.Create("<clash-path-malformed>"))
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
            // Crossings between samples carry a zero-clearance fact the vertex distances miss: two positive endpoints straddling a wall
            Vector3 leg = samples[i + 1] - samples[i];
            float span = leg.Length();
            if (span <= 1e-9f) { continue; }
            // The WITNESS is the crossing itself, not the sample that preceded it: a tool centre that clears at both
            // endpoints and buries mid-leg is reported where it buried, so an operator reads the offending station.
            Option<Vector3> crossing = RayFirstHit(bounds, nodes, verts, samples[i], leg / span, span);
            if (crossing.Case is Vector3 hit) { return (0.0, hit); }
        }
        return (best, at);
    }

    public static Fin<bool> Occluded(AdmittedScene scene, Vector3 origin, Vector3 direction, float maxDistance) =>
        !Finite(origin) || !Finite(direction) || !float.IsFinite(maxDistance) || maxDistance <= 0f || direction.LengthSquared() < 1e-24f
            ? Fin.Fail<bool>(new ComputeFault.ModelRejected($"<clash-ray-degenerate:{direction.LengthSquared()}>"))
            : Fin.Succ(RayFirstHit(scene.Index.Bounds.Span, scene.Index.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(scene.Triangles.Span), origin, Vector3.Normalize(direction), maxDistance).IsSome);

    // ONE ray descent answers both the occlusion predicate and the swept-clearance witness: the hit POSITION is what
    // the sweep needs, and a bool-returning twin beside it would walk the same tree for strictly less evidence.
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

    // Descent scratch: the ray and nearest-point queries run per path sample and per probe point, so the stack is
    // per-thread process-static and grows to the tree's node count once. An admitted wire is a TREE, so a descent
    // never holds more frames than it has nodes and the buffer needs no per-push bound check.
    [ThreadStatic] private static int[]? descent;

    static int[] Descent(int nodeCount) =>
        descent is { } held && held.Length >= nodeCount ? held : descent = new int[Math.Max(64, nodeCount)];

    // Component reads index the vectors directly — a per-call `Span<float>` rebuild of the origin and direction was
    // three stores and a bounds check per axis on the hottest loop in the page.
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
        new(index.Kind.Key, survey.Candidates,
            survey.Pairs.Count(static pair => pair.Kind == ClashKind.Hard),
            survey.Pairs.Count(static pair => pair.Kind == ClashKind.Clearance),
            survey.Pairs.Count) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Interactive, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    const int ChildShift = 21;
    const long ChildMask = (1L << ChildShift) - 1;

    internal static Fin<Unit> Admit(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        ReadOnlyMemory<float> boundsMem = index.Bounds;
        ReadOnlyMemory<long> nodesMem = index.Nodes;
        if (policy.Invalid)
            return Fin.Fail<Unit>(ComputeFault.Create("<clash-policy-invalid>"));
        if (index.BuildParameter <= 0)
            return Fin.Fail<Unit>(ComputeFault.Create("<clash-build-parameter-invalid>"));
        if (boundsMem.Length % 6 != 0 || boundsMem.Length == 0)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-bounds-malformed:{boundsMem.Length}>"));
        if (triangles.Length == 0 || triangles.Length % 9 != 0)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-triangles-unaligned:{triangles.Length}>"));
        int nodeCount = boundsMem.Length / 6, triCount = triangles.Length / 9;
        if (nodesMem.Length < nodeCount)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-node-block-short:{nodesMem.Length}<{nodeCount}>"));
        ReadOnlySpan<float> bounds = boundsMem.Span;
        ReadOnlySpan<long> nodes = nodesMem.Span;
        ReadOnlySpan<Vector3> vertices = MemoryMarshal.Cast<float, Vector3>(triangles.Span);
        for (int node = 0; node < nodeCount; node++) {
            ReadOnlySpan<float> box = bounds.Slice(node * 6, 6);
            if (!box.ToArray().All(float.IsFinite) || box[0] > box[3] || box[1] > box[4] || box[2] > box[5])
                return Fin.Fail<Unit>(ComputeFault.Create($"<clash-box-invalid:{node}>"));
        }
        for (int triangle = 0; triangle < triCount; triangle++) {
            Vector3 a = vertices[3 * triangle], b = vertices[3 * triangle + 1], c = vertices[3 * triangle + 2];
            if (!Finite(a) || !Finite(b) || !Finite(c) || Vector3.Cross(b - a, c - a).LengthSquared() <= 1e-24f)
                return Fin.Fail<Unit>(ComputeFault.Create($"<clash-triangle-invalid:{triangle}>"));
        }
        // SINGLE PARENTAGE is what makes the wire a TREE rather than merely an acyclic graph, and the tree is what
        // makes each unordered node pair reachable by exactly ONE descent path — so `NodeLinkPairs` needs no visited
        // set and a shared subtree can never re-expand its pairs. Proving it here costs one pass; absorbing it in the
        // traversal costs a hash set per query.
        byte[] claimed = new byte[nodeCount];
        for (int node = 0; node < nodeCount; node++) {
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (!leaf) {
                if (count < index.Kind.MinimumChildren || count > index.Kind.MaximumChildren || first < 0 || first + count > nodeCount || (first <= node && node < first + count))
                    return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-child-range-oob:{first}+{count}/{nodeCount}>"));
                for (int child = 0; child < count; child++) {
                    if (first + child == 0 || claimed[first + child] != 0)
                        return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-node-multiply-claimed:{first + child}>"));
                    claimed[first + child] = 1;
                }
                continue;
            }
            if (count <= 0 || first < 0 || nodeCount + first + count > nodes.Length)
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-leaf-range-oob:{first}+{count}>"));
            for (int s = 0; s < count; s++) {
                long primitive = nodes[nodeCount + first + s];
                if (primitive < 0 || primitive >= triCount)
                    return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-leaf-primitive-oob:{primitive}/{triCount}>"));
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
            if (state[frame.Node] == 1) { return Fin.Fail<Unit>(ComputeFault.Create($"<clash-cycle:{frame.Node}>")); }
            if (state[frame.Node] == 2) { continue; }
            state[frame.Node] = 1;
            stack.Push((frame.Node, true));
            (bool leaf, int first, int count) = Decode(nodes[frame.Node]);
            if (!leaf) {
                for (int child = count - 1; child >= 0; child--) { stack.Push((first + child, false)); }
            }
        }
        return state.Any(static value => value == 0)
            ? Fin.Fail<Unit>(ComputeFault.Create("<clash-unreachable-node>"))
            : Fin.Succ(unit);
    }

    static bool Finite(Vector3 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    // No visited set: `Admit` proves single parentage, so every unordered node pair is generated by exactly one
    // descent path and the deduplicating hash the traversal used to carry was insurance against a wire admission
    // now refuses outright. `Candidates` counts the triangle pairs the narrow phase actually classified, so the
    // receipt's candidate slot is measured rather than estimated from the leaf arity.
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
                            // The ceiling stops the SINK, never the classification already paid for: a degenerate
                            // scene degrades to a bounded, flagged answer instead of exhausting the pool.
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

    // Intersection is the DISCRIMINANT, clearance the magnitude: a confirmed overlap bands hard at zero clearance, a
    // coplanar touching pair inside the duplicate tolerance bands duplicate, and a positive gap inside the clearance
    // threshold bands clearance. Reading a single signed scalar for both collapsed "deeper overlap" onto "wider gap".
    static Option<ClashPair> Classify(long left, long right, (Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) {
        (double clearance, bool intersecting, Vector3 witness) = Separation(a, b);
        return !policy.HardOnly && clearance <= policy.DuplicateTolerance && Coincident(a, b, policy)
            ? Some(new ClashPair(left, right, ClashKind.Duplicate, clearance, intersecting, witness))
            : intersecting
                ? Some(new ClashPair(left, right, ClashKind.Hard, 0.0, true, witness))
                : policy.HardOnly || clearance > policy.ClearanceThreshold
                    ? Option<ClashPair>.None
                    : Some(new ClashPair(left, right, ClashKind.Clearance, clearance, false, witness));
    }

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
        // Coplanarity is a POLICY cosine: a model authored at coarse tessellation needs a looser parallel test than a
        // machined assembly, and a literal here made that a rebuild rather than a column.
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

    // Shares the ray descent's per-thread scratch: the two queries run sequentially inside `Sweep`, never nested, so
    // one buffer per thread serves both and no query allocates.
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

    // Decode inverts the producer pack on both arms, and every leaf read indexes nodes[nodeCount + First + s] — the exact counterpart of the
    // producer's TAIL-RELATIVE tail - count, so First is a tail offset and never a node index.
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

// --- [TWIN] ------------------------------------------------------------------------------------------------

public static class DigitalTwin {
    // Fitted Stats/estimator partial application supplies the detector (EstimatorFold.Predict over a
    // Detector-kind EstimatorModel); the twin pushes the residual, hands the window as detector evidence, and
    // reads the LAST row's score and change flag — one anomaly owner, twin-local control projection downstream.
    public static Fin<(TwinVerdict Verdict, TwinWindow Window)> Score(
        Surrogate baseline, TwinSignal signal, TwinWindow window, Func<Matrix<double>, Fin<Prediction>> detector, TwinPolicy policy, IClock clock) {
        if (signal.Invalid || window.Invalid || policy.Invalid) { return Fin.Fail<(TwinVerdict, TwinWindow)>(ComputeFault.Create("<twin-invalid-input>")); }
        return baseline.Predict(new DesignPoint(signal.OperatingPoint, [], [])).Bind(prediction => {
            if (prediction.Values.Count != 1 || !prediction.Values.ForAll(double.IsFinite) || !double.IsFinite(prediction.Bound) || prediction.Bound < 0.0) {
                return Fin.Fail<(TwinVerdict, TwinWindow)>(ComputeFault.Create("<twin-surrogate-shape>"));
            }
            double predicted = prediction.Values[0];
            double residual = predicted - signal.Measured;
            TwinWindow next = window.Push(residual);
            Matrix<double> evidence = Matrix<double>.Build.Dense(next.Residuals.Count, 1, (row, _) => next.Residuals[row]);
            return detector(evidence).Bind(outcome => outcome is Prediction.Anomaly anomaly && anomaly.Scores.Count == next.Residuals.Count
                ? Fin.Succ((new TwinVerdict(
                    signal.SignalId, predicted, signal.Measured, residual,
                    anomaly.Scores[anomaly.Scores.Count - 1], anomaly.Changes[^1],
                    anomaly.Changes[^1] ? -policy.ControlGain * residual : 0.0,
                    clock.GetCurrentInstant()), next))
                : Fin.Fail<(TwinVerdict, TwinWindow)>(ComputeFault.Create("<twin-detector-carrier>")));
        });
    }

    public static ComputeReceipt.Twin Receipt(TwinVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new(verdict.SignalId, verdict.Predicted, verdict.Measured, verdict.Residual, verdict.Anomaly, verdict.ControlDelta) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Interactive, Substrate.CpuTensor, AllocationClass.SpanStack, elapsed),
        };

    // FE model updating: measured modes calibrate the stiffness/mass parameter vector through the blas
    // LevenbergMarquardt black-box arm — the modal oracle crosses the full FE solve, outside hyper-dual reach, so the
    // Jacobian is the central-difference column sweep this owner authors. Pairing is one-to-one greedy by complex MAC (magnitude AND phase) under the MAC floor;
    // Residual stacks weighted frequency errors over paired-mode MAC deficits.
    public static Fin<UpdateVerdict> Update(
        Func<ImmutableArray<double>, Fin<Seq<(double FrequencyHz, ReadOnlyMemory<double> Shape)>>> modalOracle,
        Seq<MeasuredMode> measured,
        ImmutableArray<double> seed,
        ModelUpdatePolicy policy,
        IClock clock) {
        if (policy.Invalid || measured.IsEmpty || seed.IsDefaultOrEmpty || !seed.All(double.IsFinite)
            || (!policy.Bounds.IsDefaultOrEmpty && policy.Bounds.Length != seed.Length)) {
            return Fin.Fail<UpdateVerdict>(ComputeFault.Create("<twin-update-invalid-input>"));
        }
        // Every oracle evaluation runs INSIDE the declared box: proposals project through Boxed before the
        // oracle, and the fitted verdict carries the projected parameters — an out-of-box iterate is
        // structurally unreachable while empty Bounds remain the honest unbounded case.
        Func<Vector<double>, Vector<double>> boxed = parameters => Boxed(parameters, policy.Bounds);
        int rows = 2 * measured.Count, calls = 0;
        Error? probeFault = null;
        Vector<double> lastResidual = Vector<double>.Build.Dense(rows);
        Matrix<double> lastJacobian = Matrix<double>.Build.Dense(rows, seed.Length);
        // The oracle budget and the probe fault are the SAME latch: once either trips, every further callback
        // replays the last ADMITTED residual and Jacobian, so LM sees no improvement, terminates on its own
        // stall rule, and consumes neither a fabricated zero column nor a MaxValue cost as evidence — and the
        // outer Bind reads the latch before any verdict projects.
        bool Spent => probeFault is not null || calls >= policy.MaxOracleCalls;
        Fin<Vector<double>> Residual(Vector<double> parameters) {
            if (Spent) { return Fin.Succ(lastResidual); }
            calls++;
            return modalOracle([.. boxed(parameters)])
                .Bind(computed => Stacked(measured, computed, policy))
                .Map(stacked => lastResidual = stacked);
        }
        return Residual(Vector<double>.Build.DenseOfArray([.. seed])).Bind(_ =>
            LevenbergMarquardt.Minimize(
                p => Residual(p).Match(Succ: r => r, Fail: e => { probeFault ??= e; return lastResidual; }),
                p => Jacobian(Residual, p, rows).Match(Succ: j => lastJacobian = j, Fail: e => { probeFault ??= e; return lastJacobian; }),
                Vector<double>.Build.DenseOfArray([.. seed]),
                policy.Descent)
            .Bind(fit => probeFault is { } fault
                ? Fin.Fail<UpdateVerdict>(fault)
                // The final pairing reads the fitted parameters through ONE more oracle call, which the budget
                // admits ahead of the descent rather than counting against it — a verdict with no pairing evidence
                // is not a partial answer, it is no answer.
                : modalOracle([.. boxed(fit.Parameters)]).Bind(computed =>
                    Pairs(measured, computed, policy).Map(indexed => {
                        Seq<ModePair> pairs = indexed.Map(pair =>
                            new ModePair(pair.MeasuredIndex, pair.ComputedIndex, measured[pair.MeasuredIndex].FrequencyHz, computed[pair.ComputedIndex].FrequencyHz, pair.Mac));
                        Seq<double> unpaired = measured
                            .Map(static (mode, index) => (Index: index, mode.FrequencyHz))
                            .Filter(row => !pairs.Exists(pair => pair.MeasuredIndex == row.Index))
                            .Map(static row => row.FrequencyHz);
                        return new UpdateVerdict([.. boxed(fit.Parameters)], fit.Residual, fit.Iterations,
                            fit.Converged && calls < policy.MaxOracleCalls, calls >= policy.MaxOracleCalls, calls,
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
            // Residual assembly keys the MEASURED INDEX the pairing returned — a float-frequency lookup silently
            // mis-assigned every mode of a symmetric structure, where two measured peaks share a frequency to the
            // last bit and the first row claimed both.
            for (int index = 0; index < measured.Count; index++) {              // an unpaired mode contributes the full unit deficit on both rows
                Option<(int MeasuredIndex, int ComputedIndex, double Mac)> pair = pairs.Find(row => row.MeasuredIndex == index);
                double measuredHz = measured[index].FrequencyHz;
                stacked[2 * index] = policy.FrequencyWeight * pair.Match(
                    Some: row => (computed[row.ComputedIndex].FrequencyHz - measuredHz) / Math.Max(1e-9, measuredHz),
                    None: () => 1.0);
                stacked[2 * index + 1] = policy.MacWeight * pair.Match(Some: static row => 1.0 - row.Mac, None: () => 1.0);
            }
            return Vector<double>.Build.DenseOfArray(stacked);
        });

    // ONE-TO-ONE correspondence over the COMPLETE retained shape evidence: candidate (measured, computed) pairs
    // rank by complex MAC — magnitude AND phase, |φmᴴφc|²/(‖φm‖²·‖φc‖²) — and greedily assign so no computed
    // mode pairs twice; a channel-width mismatch is a typed rejection, never a truncation, and both unmatched
    // sides survive as explicit evidence (unpaired measured rows carry the unit deficit; extra computed modes
    // stay unclaimed).
    static Fin<Seq<(int MeasuredIndex, int ComputedIndex, double Mac)>> Pairs(Seq<MeasuredMode> measured, Seq<(double FrequencyHz, ReadOnlyMemory<double> Shape)> computed, ModelUpdatePolicy policy) {
        if (measured.Exists(mode => computed.Exists(candidate => candidate.Shape.Length != mode.ShapeMagnitude.Length))) {
            return Fin.Fail<Seq<(int, int, double)>>(ComputeFault.Create("<twin-update-shape-width>"));
        }
        Seq<(int MeasuredIndex, int ComputedIndex, double Mac)> ranked = toSeq(
            from m in Enumerable.Range(0, measured.Count)
            from c in Enumerable.Range(0, computed.Count)
            let mac = Mac(measured[m], computed[c].Shape.Span)
            where mac >= policy.MacFloor
            orderby mac descending
            select (m, c, mac));
        HashSet<int> takenMeasured = [];
        HashSet<int> takenComputed = [];
        // The correspondence IS the index pair — every downstream join reads it, so the frequencies never become a
        // key and two modes agreeing to the last bit stay two modes.
        Seq<(int, int, double)> pairs = Seq<(int, int, double)>();
        foreach ((int m, int c, double mac) in ranked) {
            if (takenMeasured.Contains(m) || takenComputed.Contains(c)) { continue; }
            takenMeasured.Add(m); takenComputed.Add(c);
            pairs = pairs.Add((m, c, mac));
        }
        return Fin.Succ(pairs);
    }

    // Complex MAC over the full measured shape: φm = mag·e^{iθ} against the real computed φc —
    // |φmᴴφc|² = (Σ mag·cosθ·c)² + (Σ mag·sinθ·c)².
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

    // Every column must settle: a failed forward or backward probe fails the WHOLE Jacobian — a pre-zeroed
    // column reaching the optimizer as fabricated zero sensitivity is the deleted form.
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

// --- [TWIN_LOOP] -------------------------------------------------------------------------------------------

public sealed record TwinLoopPolicy(TwinPolicy Scoring, int RecalibrateEvery, int CommitAttempts) {
    public static readonly TwinLoopPolicy Canonical = new(TwinPolicy.Canonical, RecalibrateEvery: 256, CommitAttempts: 8);

    public bool Invalid => Scoring.Invalid || RecalibrateEvery < 8 || CommitAttempts < 1;
}

// Capture-lane twin loop: lane dispatch calls Ingest after a verified broker adapter enqueues; this owner
// serializes signal admission, held-window scoring, anomaly control veto, and recalibration cadence.
// Admission-once: the loop exists only over a valid policy — Of refuses before any held state mints, so an
// Atom<TwinState> over an invalid window capacity is unconstructible and Ingest never re-validates per call.
public sealed class TwinLoop {
    private readonly record struct TwinState(TwinWindow Window, long Scored, long Recalibrated, bool Claimed, long Revision, long Commit);

    private readonly Surrogate baseline;
    private readonly Func<Matrix<double>, Fin<Prediction>> detector;
    private readonly TwinLoopPolicy policy;
    private readonly Func<TwinVerdict, Fin<TwinVerdict>> suggested;
    private readonly IClock clock;
    private readonly Atom<TwinState> held;
    private long tickets;

    private TwinLoop(Surrogate baseline, Func<Matrix<double>, Fin<Prediction>> detector, TwinLoopPolicy policy, Func<TwinVerdict, Fin<TwinVerdict>> suggested, IClock clock) {
        (this.baseline, this.detector, this.policy, this.suggested, this.clock) = (baseline, detector, policy, suggested, clock);
        held = Atom(new TwinState(TwinWindow.Of(policy.Scoring), Scored: 0L, Recalibrated: 0L, Claimed: false, Revision: 0L, Commit: 0L));
    }

    public static Fin<TwinLoop> Of(
        Surrogate baseline,
        Func<Matrix<double>, Fin<Prediction>> detector,
        TwinLoopPolicy policy,
        Func<TwinVerdict, Fin<TwinVerdict>> suggested,
        IClock clock) =>
        policy.Invalid
            ? Fin.Fail<TwinLoop>(ComputeFault.Create("<twin-loop-policy>"))
            : Fin.Succ(new TwinLoop(baseline, detector, policy, suggested, clock));

    // Edge-consuming cadence over the ELAPSED count, never a modulo against it: a modulo fires only on the exact
    // multiple, so a burst that carries the counter past a boundary skips that recalibration outright, and the same
    // count re-probed by a second claimant reads due twice. The distance predicate consumes each interval once and
    // survives any ingest burst — the first claimant past the interval advances the mark to the current count.
    public bool ClaimRecalibration() {
        TwinState next = held.Swap(state => {
            bool due = state.Scored - state.Recalibrated >= policy.RecalibrateEvery;
            return state with { Recalibrated = due ? state.Scored : state.Recalibrated, Claimed = due };
        });
        return next.Claimed;
    }

    public static Fin<TwinSignal> Admit(SensorEnvelope<TwinSignal> envelope) =>
        !envelope.Data.Invalid
            ? Fin.Succ(envelope.Data)
            : Fin.Fail<TwinSignal>(ComputeFault.Create($"<twin-envelope-data:{envelope.Event.Id}>"));

    public Fin<TwinVerdict> Ingest(SensorEnvelope<TwinSignal> envelope) =>
        Admit(envelope).Bind(Score);

    private Fin<TwinVerdict> Score(TwinSignal signal) =>
        Range(0, policy.CommitAttempts).Fold(
            Fin.Succ(Option<TwinVerdict>.None),
            (settled, _) => settled.Bind(verdict => verdict.IsSome ? Fin.Succ(verdict) : TryCommit(signal)))
        .Bind(verdict => verdict.Match(
            Some: won => won.Anomaly ? suggested(won) : Fin.Succ(won),
            None: () => Fin.Fail<TwinVerdict>(ComputeFault.Create("<twin-contention>"))));

    private Fin<Option<TwinVerdict>> TryCommit(TwinSignal signal) {
        TwinState snapshot = held.Value;
        return DigitalTwin.Score(baseline, signal, snapshot.Window, detector, policy.Scoring, clock).Map(scored => {
            long ticket = Interlocked.Increment(ref tickets);
            TwinState committed = held.Swap(state => state.Revision == snapshot.Revision
                ? state with {
                    Window = scored.Window,
                    Scored = state.Scored + 1L,
                    Claimed = false,
                    Revision = state.Revision + 1L,
                    Commit = ticket,
                }
                : state);
            return committed.Commit == ticket ? Some(scored.Verdict) : Option<TwinVerdict>.None;
        });
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
