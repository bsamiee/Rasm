# [COMPUTE_CLASH]

Rasm.Compute solver clash confirms collisions and scores live telemetry: `ClashScale` is the narrow-phase collision-confirmation fold over the geometry-owned node-link broad-phase wire, and `DigitalTwin` scores a live signal against a `Surrogate` baseline through a Kalman innovation and two-sided CUSUM. `AccelerationStructure` is one decoded wire record parameterized by `AccelerationKind`; `Spatial.Apply(SpatialOp.Wire)` remains the sole builder/refitter.

Narrow-phase triangle and closest-distance work rides `System.Numerics.Vector3` hardware vectors over a `MemoryMarshal.Cast<float, Vector3>` view of the federated triangle wire; the twin baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` evaluated through `Surrogate.Predict` over a `DesignPoint`, so the twin and the design search share one reduced-order model. `ComputeReceipt`, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. Candidate `ClashPair` sets feed the `Model/inference#INFERENCE_MODES` `ClashScore` false-positive filter; a twin control suggestion crosses to the AppHost `Wire/livewire#WRITE_BACK` outbound write-back as a receipted `ExternalValue`. Page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[CLASH_AND_TWIN]: node-link narrow-phase collision confirmation and clearance descent; Kalman/CUSUM ROM digital-twin loop.

## [02]-[CLASH_AND_TWIN]

- Owner: `AccelerationKind` `[SmartEnum<string>]` carries BVH/octree child arity; `AccelerationStructure` carries one decoded `(Bounds, Nodes, BuildParameter)` wire; `ClashKind` classifies hard · clearance · duplicate; `ClashScale` owns admission, traversal, intersection, and clearance; `DigitalTwin` owns validated Kalman/CUSUM scoring.
- Cases: `AccelerationKind` bvh · octree; `ClashKind` hard · clearance · duplicate; `ClashPair.Separation` is `0` for a confirmed triangle-surface intersection and positive for clearance because the decoded surface wire carries no volumetric penetration domain.
- Entry: `public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy)` — `Fin<T>` aborts through `Admit` on a malformed wire (mis-aligned bounds/triangle buffer, out-of-range child/leaf range, out-of-range leaf primitive id); `Clearance(index, triangles, Vector3 point)` and `SweptClearance(index, triangles, path)` descend the same tree for the point-to-scene nearest-surface distance and the CAM/motion swept-volume minimum clearance — each path leg additionally ray-tests the surface so a crossing between samples reports zero clearance instead of the two positive endpoint distances. Detection is a pure geometry fold, so the `CorrelationId`/`ClockPolicy` receipt tail enters only at `Receipt`, never as a dead entry param. Incremental edits are the kernel `SpatialOp.Refit` seam — a moved element re-bounds there and re-projects through `SpatialOp.Wire` unchanged, so a Compute-local `Insert`/`Remove` index rebuild is the rejected double-owner form.
- Auto: `Detect` walks the contiguous `[FirstChild, FirstChild+ChildCount)` child range as one hierarchical descent — a BVH node and an octree cell traverse identically, so the prior parallel `BvhPairs`/`OctreePairs` bodies collapse to one `NodeLinkPairs` fold and the Morton-cell decode that mis-read the node-link array as a per-element cell map is the deleted form; each overlapping leaf-pair runs the complete two-direction Möller–Trumbore test plus the Ericson closest distance and bands by `ClashPolicy`. `DigitalTwin` scores the `Surrogate` residual through the `TwinFilter` Kalman+CUSUM recursion into a verdict plus a control suggestion.
- Receipt: the `Clash` `ComputeReceipt` case carries the index kind, candidate-pair count, confirmed hard-clash and clearance-violation counts, and total pairs; the `Twin` case carries the signal id, predicted-versus-measured residual, Kalman-smoothed residual, anomaly flag, and suggested control delta, so a twin loop is auditable and a machine-control suggestion is receipted before it leaves the boundary.
- Packages: System.Numerics (`Vector3`), System.Runtime.InteropServices (`MemoryMarshal`), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new clash kind is one `ClashKind` row; a new twin scoring channel is one field on `TwinSignal`/`TwinVerdict`; a new broad-phase kernel that still emits the node-link wire reuses `NodeLinkPairs` untouched; zero new surface — a `BvhTree`/`OctreeIndex`/`SdfField` sibling family collapses onto the one decoded `AccelerationStructure` wire and the one `NodeLinkPairs` traversal.
- Boundary: `AccelerationStructure` is the decoded read-only node-link wire, and `AccelerationKind` validates only the builder-specific child arity. `Admit` verifies finite ordered boxes, finite nondegenerate triangles, leaf primitive ranges, child ranges, root reachability, and acyclicity before traversal. `NodeLinkPairs` canonicalizes and deduplicates node pairs; equal internal nodes expand upper-triangular child pairs. `Separation` reports surface intersection as `0` and clearance as positive; volumetric penetration depth requires a solid-domain carrier absent from this wire. `DigitalTwin.Score` faults malformed signals, filters, policies, or surrogate outputs; signed residuals feed both CUSUM directions, and corrective control opposes the smoothed residual.

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

public sealed record ClashPolicy(double ClearanceThreshold, double DuplicateTolerance, bool HardOnly) {
    public static readonly ClashPolicy Canonical = new(ClearanceThreshold: 0.025, DuplicateTolerance: 1e-4, HardOnly: false);
}

public readonly record struct ClashPair(long Left, long Right, ClashKind Kind, double Separation, Vector3 Witness);

public sealed record TwinSignal(string SignalId, ImmutableArray<double> OperatingPoint, double Measured, Instant At) {
    public bool Invalid => string.IsNullOrWhiteSpace(SignalId) || OperatingPoint.IsDefaultOrEmpty || !OperatingPoint.All(double.IsFinite) || !double.IsFinite(Measured);
}

public sealed record TwinPolicy(double SigmaBound, double DriftThreshold, double ControlGain) {
    public static readonly TwinPolicy Canonical = new(SigmaBound: 3.0, DriftThreshold: 5.0, ControlGain: 1.0);

    public bool Invalid => !double.IsFinite(SigmaBound) || SigmaBound <= 0.0 || !double.IsFinite(DriftThreshold) || DriftThreshold <= 0.0 || !double.IsFinite(ControlGain) || ControlGain < 0.0;
}

public sealed record TwinFilter(double Estimate, double Variance, double ProcessNoise, double MeasurementNoise, double CusumHigh, double CusumLow, double DriftSlack) {
    public static TwinFilter Initial => new(0.0, 1.0, 1e-4, 1e-2, 0.0, 0.0, 0.5);

    public bool Invalid => !double.IsFinite(Estimate) || !double.IsFinite(Variance) || Variance <= 0.0
        || !double.IsFinite(ProcessNoise) || ProcessNoise < 0.0 || !double.IsFinite(MeasurementNoise) || MeasurementNoise <= 0.0
        || !double.IsFinite(CusumHigh) || CusumHigh < 0.0 || !double.IsFinite(CusumLow) || CusumLow > 0.0
        || !double.IsFinite(DriftSlack) || DriftSlack < 0.0;

    public (TwinFilter Next, double Smoothed, double Normalized, bool Drift) Update(double observation, double driftThreshold) {
        double predictedVariance = Variance + ProcessNoise;
        double innovation = observation - Estimate;
        double innovationVariance = predictedVariance + MeasurementNoise;
        double gain = predictedVariance / innovationVariance;
        double estimate = Estimate + gain * innovation;
        double normalized = innovation / Math.Sqrt(innovationVariance);
        double high = Math.Max(0.0, CusumHigh + normalized - DriftSlack);
        double low = Math.Min(0.0, CusumLow + normalized + DriftSlack);
        bool drift = high > driftThreshold || -low > driftThreshold;
        return (this with {
            Estimate = estimate,
            Variance = (1.0 - gain) * predictedVariance,
            CusumHigh = drift ? 0.0 : high,
            CusumLow = drift ? 0.0 : low,
        }, estimate, normalized, drift);
    }
}

public sealed record TwinVerdict(string SignalId, double Predicted, double Measured, double Residual, double Smoothed, double Normalized, bool Anomaly, bool Drift, double ControlDelta, Instant At);

// --- [OPERATIONS] ------------------------------------------------------------------------------------------

public static class ClashScale {
    public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) =>
        Admit(index, triangles, policy).Map(_ => NodeLinkPairs(index.Bounds, index.Nodes, triangles, policy));

    public static Fin<double> Clearance(AccelerationStructure index, ReadOnlyMemory<float> triangles, Vector3 point) =>
        Admit(index, triangles, ClashPolicy.Canonical).Bind(_ => Finite(point)
            ? Fin.Succ(NearestTriangle(index.Bounds.Span, index.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(triangles.Span), point))
            : Fin.Fail<double>(ComputeFault.Create("<clash-point-nonfinite>")));

    public static Fin<(double Clearance, Vector3 At)> SweptClearance(AccelerationStructure index, ReadOnlyMemory<float> triangles, ReadOnlyMemory<float> path) =>
        Admit(index, triangles, ClashPolicy.Canonical).Bind(_ => path.IsEmpty || path.Length % 3 != 0 || !path.Span.ToArray().All(float.IsFinite)
            ? Fin.Fail<(double Clearance, Vector3 At)>(ComputeFault.Create("<clash-path-malformed>"))
            : Fin.Succ(Sweep(index, triangles, path)));

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
            // A crossing between samples is a zero-clearance fact the vertex distances miss: two positive endpoints straddling a wall
            Vector3 leg = samples[i + 1] - samples[i];
            float span = leg.Length();
            if (span > 1e-9f && RayHit(bounds, nodes, verts, samples[i], leg / span, span)) { return (0.0, samples[i]); }
        }
        return (best, at);
    }

    public static Fin<bool> Occluded(AccelerationStructure index, ReadOnlyMemory<float> triangles, Vector3 origin, Vector3 direction, float maxDistance) =>
        Admit(index, triangles, ClashPolicy.Canonical).Bind(_ => {
            if (!Finite(origin) || !Finite(direction) || !float.IsFinite(maxDistance) || maxDistance <= 0f || direction.LengthSquared() < 1e-24f)
                return Fin.Fail<bool>(new ComputeFault.ModelRejected($"<clash-ray-degenerate:{direction.LengthSquared()}>"));
            return Fin.Succ(RayHit(index.Bounds.Span, index.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(triangles.Span), origin, Vector3.Normalize(direction), maxDistance));
        });

    static bool RayHit(ReadOnlySpan<float> bounds, ReadOnlySpan<long> nodes, ReadOnlySpan<Vector3> verts, Vector3 origin, Vector3 direction, float maxDistance) {
        int nodeCount = bounds.Length / 6;
        Vector3 far = origin + direction * maxDistance;
        Stack<int> stack = new();
        stack.Push(0);
        while (stack.Count > 0) {
            int node = stack.Pop();
            if (!RaySlab(bounds.Slice(node * 6, 6), origin, direction, maxDistance)) { continue; }
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (leaf) {
                for (int s = 0; s < count; s++) {
                    int tri = (int)nodes[nodeCount + first + s];
                    if (SegmentTriangle(origin, far, verts[3 * tri], verts[3 * tri + 1], verts[3 * tri + 2]).IsSome) { return true; }
                }
            } else {
                for (int c = 0; c < count; c++) { stack.Push(first + c); }
            }
        }
        return false;
    }

    static bool RaySlab(ReadOnlySpan<float> box, Vector3 origin, Vector3 direction, float maxDistance) {
        Span<float> o = [origin.X, origin.Y, origin.Z];
        Span<float> d = [direction.X, direction.Y, direction.Z];
        float tNear = 0f, tFar = maxDistance;
        for (int axis = 0; axis < 3; axis++) {
            if (MathF.Abs(d[axis]) < 1e-12f) {
                if (o[axis] < box[axis] || o[axis] > box[3 + axis]) { return false; }
                continue;
            }
            float inv = 1f / d[axis];
            float t1 = (box[axis] - o[axis]) * inv, t2 = (box[3 + axis] - o[axis]) * inv;
            if (t1 > t2) { (t1, t2) = (t2, t1); }
            tNear = MathF.Max(tNear, t1);
            tFar = MathF.Min(tFar, t2);
            if (tNear > tFar) { return false; }
        }
        return true;
    }

    public static ComputeReceipt.Clash Receipt(AccelerationStructure index, Seq<ClashPair> pairs, int candidates, CorrelationId correlation, Duration elapsed) =>
        new(index.Kind.Key, candidates, pairs.Count(static pair => pair.Kind == ClashKind.Hard), pairs.Count(static pair => pair.Kind == ClashKind.Clearance), pairs.Count) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    const int ChildShift = 21;
    const long ChildMask = (1L << ChildShift) - 1;

    static Fin<Unit> Admit(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        ReadOnlyMemory<float> boundsMem = index.Bounds;
        ReadOnlyMemory<long> nodesMem = index.Nodes;
        if (!double.IsFinite(policy.ClearanceThreshold) || policy.ClearanceThreshold < 0.0 || !double.IsFinite(policy.DuplicateTolerance) || policy.DuplicateTolerance < 0.0 || policy.DuplicateTolerance > policy.ClearanceThreshold)
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
        for (int node = 0; node < nodeCount; node++) {
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (!leaf) {
                if (count < index.Kind.MinimumChildren || count > index.Kind.MaximumChildren || first < 0 || first + count > nodeCount || (first <= node && node < first + count))
                    return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-child-range-oob:{first}+{count}/{nodeCount}>"));
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

    static Seq<ClashPair> NodeLinkPairs(ReadOnlyMemory<float> boundsMem, ReadOnlyMemory<long> nodesMem, ReadOnlyMemory<float> trianglesMem, ClashPolicy policy) {
        ReadOnlySpan<float> bounds = boundsMem.Span;
        ReadOnlySpan<long> nodes = nodesMem.Span;
        ReadOnlySpan<Vector3> verts = MemoryMarshal.Cast<float, Vector3>(trianglesMem.Span);
        int nodeCount = bounds.Length / 6;
        using ArrayPoolBufferWriter<ClashPair> sink = new();
        Stack<(int L, int R)> stack = new();
        HashSet<(int L, int R)> visited = [];
        stack.Push((0, 0));
        while (stack.Count > 0) {
            (int l, int r) = stack.Pop();
            if (l > r) { (l, r) = (r, l); }
            if (!visited.Add((l, r))) { continue; }
            if (!BoxOverlap(bounds.Slice(l * 6, 6), bounds.Slice(r * 6, 6), policy.ClearanceThreshold)) { continue; }
            (bool lLeaf, int lFirst, int lCount) = Decode(nodes[l]);
            (bool rLeaf, int rFirst, int rCount) = Decode(nodes[r]);
            if (lLeaf && rLeaf) {
                for (int a = 0; a < lCount; a++) {
                    int left = (int)nodes[nodeCount + lFirst + a];
                    (Vector3, Vector3, Vector3) triLeft = (verts[3 * left], verts[3 * left + 1], verts[3 * left + 2]);
                    for (int b = 0; b < rCount; b++) {
                        int right = (int)nodes[nodeCount + rFirst + b];
                        if (l == r && right <= left) { continue; }
                        (Vector3, Vector3, Vector3) triRight = (verts[3 * right], verts[3 * right + 1], verts[3 * right + 2]);
                        if (Classify(left, right, triLeft, triRight, policy) is { IsSome: true, Case: ClashPair pair }) {
                            sink.GetSpan(1)[0] = pair;
                            sink.Advance(1);
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
        return toSeq(sink.WrittenSpan.ToArray());
    }

    static Option<ClashPair> Classify(long left, long right, (Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) {
        (double signed, Vector3 witness) = Separation(a, b);
        return !policy.HardOnly && signed <= policy.DuplicateTolerance && Coincident(a, b, policy.DuplicateTolerance)
            ? Some(new ClashPair(left, right, ClashKind.Duplicate, signed, witness))
            : signed <= 0.0
                ? Some(new ClashPair(left, right, ClashKind.Hard, signed, witness))
                : policy.HardOnly || signed > policy.ClearanceThreshold
                    ? Option<ClashPair>.None
                    : Some(new ClashPair(left, right, ClashKind.Clearance, signed, witness));
    }

    static (double Signed, Vector3 Witness) Separation((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b) {
        Span<Vector3> va = [a.A, a.B, a.C, a.A];
        Span<Vector3> vb = [b.A, b.B, b.C, b.A];
        for (int e = 0; e < 3; e++) {
            Option<Vector3> hitA = SegmentTriangle(va[e], va[e + 1], b.A, b.B, b.C);
            if (hitA.IsSome) { return (0.0, hitA.IfNone(Vector3.Zero)); }
            Option<Vector3> hitB = SegmentTriangle(vb[e], vb[e + 1], a.A, a.B, a.C);
            if (hitB.IsSome) { return (0.0, hitB.IfNone(Vector3.Zero)); }
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
        return (best.Distance, best.Witness);
    }

    static (float Distance, Vector3 Witness) Closer((float Distance, Vector3 Witness) x, (float Distance, Vector3 Witness) y) =>
        y.Distance < x.Distance ? y : x;

    static bool Coincident((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, double tolerance) {
        Vector3 na = Vector3.Cross(a.B - a.A, a.C - a.A), nb = Vector3.Cross(b.B - b.A, b.C - b.A);
        float la = na.Length(), lb = nb.Length();
        if (la < 1e-12f || lb < 1e-12f || MathF.Abs(Vector3.Dot(na, nb) / (la * lb)) < 0.999f) { return false; }
        float limit = (float)tolerance;
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
        int nodeCount = bounds.Length / 6;
        double best = double.MaxValue;
        Stack<int> stack = new();
        stack.Push(0);
        while (stack.Count > 0) {
            int node = stack.Pop();
            if (BoxDistance(bounds.Slice(node * 6, 6), point) >= best) { continue; }
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (leaf) {
                for (int s = 0; s < count; s++) {
                    int tri = (int)nodes[nodeCount + first + s];
                    (float distance, _) = PointTriangle(point, verts[3 * tri], verts[3 * tri + 1], verts[3 * tri + 2]);
                    if (distance < best) { best = distance; }
                }
            } else {
                for (int c = 0; c < count; c++) { stack.Push(first + c); }
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

// --- [TWIN] ------------------------------------------------------------------------------------------------

public static class DigitalTwin {
    public static Fin<(TwinVerdict Verdict, TwinFilter Filter)> Score(Surrogate baseline, TwinSignal signal, TwinFilter filter, TwinPolicy policy, ClockPolicy clocks) {
        if (signal.Invalid || filter.Invalid || policy.Invalid) { return Fin.Fail<(TwinVerdict, TwinFilter)>(ComputeFault.Create("<twin-invalid-input>")); }
        return baseline.Predict(new DesignPoint(signal.OperatingPoint, [], [])).Bind(prediction => {
            if (prediction.Values.Count != 1 || !prediction.Values.ForAll(double.IsFinite) || !double.IsFinite(prediction.Bound) || prediction.Bound < 0.0) {
                return Fin.Fail<(TwinVerdict, TwinFilter)>(ComputeFault.Create("<twin-surrogate-shape>"));
            }
            double predicted = prediction.Values[0];
            double residual = predicted - signal.Measured;
            (TwinFilter next, double smoothed, double normalized, bool drift) = filter.Update(residual, policy.DriftThreshold);
            bool spike = Math.Abs(normalized) > policy.SigmaBound && Math.Abs(smoothed) > prediction.Bound;
            bool anomaly = spike || drift;
            double control = anomaly ? -policy.ControlGain * smoothed : 0.0;
            return Fin.Succ((new TwinVerdict(signal.SignalId, predicted, signal.Measured, residual, smoothed, normalized, anomaly, drift, control, clocks.Now), next));
        });
    }

    public static ComputeReceipt.Twin Receipt(TwinVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new(verdict.SignalId, verdict.Predicted, verdict.Measured, verdict.Residual, verdict.Anomaly, verdict.ControlDelta) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.SpanStack, Elapsed = elapsed,
        };
}
```
