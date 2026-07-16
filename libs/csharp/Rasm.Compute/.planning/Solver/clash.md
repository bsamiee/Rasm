# [COMPUTE_CLASH]

Rasm.Compute solver clash confirms collisions and scores live telemetry: `ClashScale` is the narrow-phase collision-confirmation fold over the geometry-owned node-link broad-phase wire, `DigitalTwin` the ROM-eval loop scoring a live signal against a `Surrogate` baseline through a Kalman-smoothed innovation z-score and a two-sided CUSUM drift gate. `AccelerationStructure` `[Union]` is the DECODED node-link wire `Rasm/Spatial/index#CLASH_SEAM` `SpatialIndex.ToAcceleration` emits — `ClashScale` reads it, descends the federated self-clash tree, and runs the complete two-direction Möller–Trumbore narrow phase, never building or refitting the index (`SpatialIndex.Build`/`Refit` owns that; a Compute-local rebuild is the rejected double-owner form). Owned surface: the `AccelerationStructure` decoded cases, the `ClashKind` rows, the `ClashPolicy`/`ClashPair` and `TwinSignal`/`TwinPolicy`/`TwinFilter`/`TwinVerdict` carriers, and the `ClashScale`/`DigitalTwin` folds.

Narrow-phase triangle and closest-distance work rides `System.Numerics.Vector3` hardware vectors over a `MemoryMarshal.Cast<float, Vector3>` view of the federated triangle wire; the twin baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` evaluated through `Surrogate.Predict` over a `DesignPoint`, so the twin and the design search share one reduced-order model. `ComputeReceipt`, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. Candidate `ClashPair` sets feed the `Model/inference#INFERENCE_MODES` `ClashScore` false-positive filter; a twin control suggestion crosses to the AppHost `Wire/livewire#WRITE_BACK` outbound write-back as a receipted `ExternalValue`. Page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[CLASH_AND_TWIN]: node-link narrow-phase collision confirmation and clearance descent; Kalman/CUSUM ROM digital-twin loop.

## [02]-[CLASH_AND_TWIN]

- Owner: `AccelerationStructure` `[Union]` the DECODED node-link wire (`Bvh`/`Octree`, both the per-node AABB `Bounds` plus packed `Nodes` descriptor `SpatialIndex.ToAcceleration` emits); `ClashKind` `[SmartEnum<string>]` classification rows under `ComparerAccessors.StringOrdinal`; `ClashPolicy`/`ClashPair` the detection policy and clash-result row; `TwinSignal`/`TwinPolicy`/`TwinFilter`/`TwinVerdict` the live sample, gate policy, Kalman+CUSUM state, and verdict; `ClashScale` the narrow-phase fold (self-clash plus clearance descent); `DigitalTwin` the ROM loop over a `TwinSignal` against a `Surrogate` baseline.
- Cases: `AccelerationStructure` `Bvh` · `Octree` — one node-link traversal serves both, the broad-phase kernels differing only in `ChildCount` (binary node `== 2`, octree cell `∈ [1,8]`), never in the descent; `ClashKind` hard · clearance · duplicate; `ClashPair` carries the two primitive ids, the kind, the signed separation (negative interference depth, positive clearance gap), and the `Vector3` witness point.
- Entry: `public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy)` — `Fin<T>` aborts through `Admit` on a malformed wire (mis-aligned bounds/triangle buffer, out-of-range child/leaf range, out-of-range leaf primitive id); `Clearance(index, triangles, Vector3 point)` and `SweptClearance(index, triangles, path)` descend the same tree for the point-to-scene nearest-surface distance and the CAM/motion swept-volume minimum clearance. Detection is a pure geometry fold, so the `CorrelationId`/`ClockPolicy` receipt tail enters only at `Receipt`, never as a dead entry param. Incremental edits are the geometry `SpatialIndex.Refit` seam — a moved element re-bounds and re-projects through `ToAcceleration` unchanged, so a Compute-local `Insert`/`Remove` index rebuild is the rejected double-owner form.
- Auto: `Detect` walks the contiguous `[FirstChild, FirstChild+ChildCount)` child range as one hierarchical descent — a BVH node and an octree cell traverse identically, so the prior parallel `BvhPairs`/`OctreePairs` bodies collapse to one `NodeLinkPairs` fold and the Morton-cell decode that mis-read the node-link array as a per-element cell map is the deleted form; each overlapping leaf-pair runs the complete two-direction Möller–Trumbore test plus the Ericson closest distance and bands by `ClashPolicy`. `DigitalTwin` scores the `Surrogate` residual through the `TwinFilter` Kalman+CUSUM recursion into a verdict plus a control suggestion.
- Receipt: the `Clash` `ComputeReceipt` case carries the index kind, candidate-pair count, confirmed hard-clash and clearance-violation counts, and total pairs; the `Twin` case carries the signal id, predicted-versus-measured residual, Kalman-smoothed residual, anomaly flag, and suggested control delta, so a twin loop is auditable and a machine-control suggestion is receipted before it leaves the boundary.
- Packages: System.Numerics (`Vector3`), System.Runtime.InteropServices (`MemoryMarshal`), CommunityToolkit.HighPerformance (`ArrayPoolBufferWriter`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new clash kind is one `ClashKind` row; a new twin scoring channel is one field on `TwinSignal`/`TwinVerdict`; a new broad-phase kernel that still emits the node-link wire reuses `NodeLinkPairs` untouched; zero new surface — a `BvhTree`/`OctreeIndex`/`SdfField` sibling family collapses onto the one decoded `AccelerationStructure` wire and the one `NodeLinkPairs` traversal.
- Boundary: `AccelerationStructure` is the DECODED, read-only node-link wire — the geometry `SpatialIndex` owns `Build`/`Refit`/persistence (its immutable `NodeStore` is the record Persistence content-addresses), so a Compute-local index-mutation surface is the rejected double-owner form; `Bvh` and `Octree` carry the identical `(Bounds, Nodes)` wire `NodeLinkProjection` emits and `Decode` matches that seam projection bit-for-bit, so `NodeLinkPairs` descends both kinds identically — the frozen `ToAcceleration` round-trip is builder-agnostic and seam-settled `Rasm/Spatial/index#CLASH_GOLDEN`, and a flat O(N²) scan or a per-kind traversal is rejected. Narrow phase is the COMPLETE two-direction Möller–Trumbore (`SegmentTriangle` over every edge, the pierce point as witness) plus the Ericson tri-tri closest distance (`SegmentSegment` over the nine edge pairs, `PointTriangle` over the six vertex queries) feeding a real signed `Separation` — a single-direction edge test, a hardcoded penetration, or a vertex-0 witness is the rejected fake. Kernel reads the federated triangle wire through `MemoryMarshal.Cast<float, Vector3>` on `System.Numerics.Vector3` hardware vectors, so a `TensorPrimitives` call over a 3-element span is the rejected anti-idiom; `ClashKind` is the closed SmartEnum so a string discriminant is rejected, and `ClashPolicy.HardOnly` gates the lower bands. `Detect` returns a real `Fin` because `Admit` validates the wire alignment and the leaf primitive-id range before the descent, lifting a malformed index to `ComputeFault.ModelRejected` rather than a silent corruption or an unguarded `IndexOutOfRange`. Clearance descent (`NearestTriangle` with AABB-distance pruning) serves the CAM/motion reachability and swept-volume check by querying the cell index for the minimum surface distance, never a re-detected pair set. Candidate `ClashPair` sets are the broad-phase result `Solver/constitutive#CONSTITUTIVE` `ContactEnforcement` consumes (contact never re-detects pairs) and the feature vector `Model/inference#INFERENCE_MODES` `ClashScore` scores for false-positive filtering. `DigitalTwin` shares the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate`, its `TwinFilter` Kalman recursion rejecting sensor noise and its two-sided CUSUM catching a slow drift a single-sample threshold misses — a raw-residual threshold without the filter is rejected, and the gate fires only on a normalized innovation past the surrogate's predictive band or an accumulated CUSUM drift; the ROM runs at interactive rates because it is a `Surrogate.Predict`, never a full solve. Control suggestion is a receipted boundary output the AppHost `Wire/livewire#WRITE_BACK` commits as an `ExternalValue` with a `HopReceipt` (or rolls back on rejection); clash and twin evidence stays the typed `ClashPair`/`TwinVerdict` plus the `ComputeReceipt.Clash`/`Twin` cases, and Compute mints no quality plan, store, or second integrity engine.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard = new("hard", severity: 2);
    public static readonly ClashKind Clearance = new("clearance", severity: 1);
    public static readonly ClashKind Duplicate = new("duplicate", severity: 0);

    public int Severity { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccelerationStructure {
    private AccelerationStructure() { }

    // The DECODED node-link wire `Rasm/Spatial/index#CLASH_SEAM` `ToAcceleration` emits: both cases carry the
    // identical per-node AABB `Bounds` and packed `Nodes` descriptor; the `int` column (`LeafSize`/`MaxDepth`) is
    // build provenance the seam carries, never read by the traversal.
    public sealed record Bvh(ReadOnlyMemory<float> Bounds, ReadOnlyMemory<long> Nodes, int LeafSize) : AccelerationStructure;
    public sealed record Octree(ReadOnlyMemory<float> Bounds, ReadOnlyMemory<long> Nodes, int MaxDepth) : AccelerationStructure;

    public string Key => Switch(bvh: static _ => "bvh", octree: static _ => "octree");

    public (ReadOnlyMemory<float> Bounds, ReadOnlyMemory<long> Nodes) NodeLink =>
        Switch(bvh: static b => (b.Bounds, b.Nodes), octree: static o => (o.Bounds, o.Nodes));
}

public sealed record ClashPolicy(double ClearanceThreshold, double DuplicateTolerance, bool HardOnly) {
    public static readonly ClashPolicy Canonical = new(ClearanceThreshold: 0.025, DuplicateTolerance: 1e-4, HardOnly: false);
}

public readonly record struct ClashPair(long Left, long Right, ClashKind Kind, double Separation, Vector3 Witness);

public sealed record TwinSignal(string SignalId, ImmutableArray<double> OperatingPoint, double Measured, Instant At);

public sealed record TwinPolicy(double SigmaBound, double DriftThreshold, double ControlGain) {
    public static readonly TwinPolicy Canonical = new(SigmaBound: 3.0, DriftThreshold: 5.0, ControlGain: 1.0);
}

public sealed record TwinFilter(double Estimate, double Variance, double ProcessNoise, double MeasurementNoise, double CusumHigh, double CusumLow, double DriftSlack) {
    public static TwinFilter Initial => new(0.0, 1.0, 1e-4, 1e-2, 0.0, 0.0, 0.5);

    // Predict-update Kalman over the scalar residual PLUS a two-sided CUSUM over the innovation-normalized residual:
    // the normalized innovation is the z-score the spike gate reads; the CUSUM resets to zero on a fired drift alarm.
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

public static class ClashScale {
    public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy) =>
        Admit(index, triangles).Map(_ => index.Switch(
            state: (Triangles: triangles, Policy: policy),
            bvh: static (s, b) => NodeLinkPairs(b.Bounds, b.Nodes, s.Triangles, s.Policy),
            octree: static (s, o) => NodeLinkPairs(o.Bounds, o.Nodes, s.Triangles, s.Policy)));

    public static Fin<double> Clearance(AccelerationStructure index, ReadOnlyMemory<float> triangles, Vector3 point) =>
        Admit(index, triangles).Map(_ => index.Switch(
            state: (Triangles: triangles, Point: point),
            bvh: static (s, b) => NearestTriangle(b.Bounds.Span, b.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(s.Triangles.Span), s.Point),
            octree: static (s, o) => NearestTriangle(o.Bounds.Span, o.Nodes.Span, MemoryMarshal.Cast<float, Vector3>(s.Triangles.Span), s.Point)));

    public static Fin<(double Clearance, Vector3 At)> SweptClearance(AccelerationStructure index, ReadOnlyMemory<float> triangles, ReadOnlyMemory<float> path) =>
        Admit(index, triangles).Map(_ => {
            var (boundsMem, nodesMem) = index.NodeLink;
            ReadOnlySpan<float> bounds = boundsMem.Span;
            ReadOnlySpan<long> nodes = nodesMem.Span;
            ReadOnlySpan<Vector3> verts = MemoryMarshal.Cast<float, Vector3>(triangles.Span);
            ReadOnlySpan<Vector3> samples = MemoryMarshal.Cast<float, Vector3>(path.Span);
            double best = double.MaxValue;
            Vector3 at = default;
            for (int i = 0; i < samples.Length; i++) {
                double distance = NearestTriangle(bounds, nodes, verts, samples[i]);
                if (distance < best) { best = distance; at = samples[i]; }
            }
            return (best, at);
        });

    // The ray entry `Analysis/daylight` shadow/obstruction composes: one Möller–Trumbore cast over the SAME
    // node-link traversal `Detect`/`Clearance` walk — a daylight-local BVH walk beside it is the rejected form.
    // `Admit` gates the wire and `% 9` alignment; a zero-length `direction` rails `ModelRejected` before the
    // `Vector3.Normalize` NaN.
    public static Fin<bool> Occluded(AccelerationStructure index, ReadOnlyMemory<float> triangles, Vector3 origin, Vector3 direction, float maxDistance) =>
        Admit(index, triangles).Bind(_ => {
            if (direction.LengthSquared() < 1e-24f)
                return Fin.Fail<bool>(new ComputeFault.ModelRejected($"<clash-ray-degenerate:{direction.LengthSquared()}>"));
            var (boundsMem, nodesMem) = index.NodeLink;
            return Fin.Succ(RayHit(boundsMem.Span, nodesMem.Span, MemoryMarshal.Cast<float, Vector3>(triangles.Span), origin, Vector3.Normalize(direction), maxDistance));
        });

    // Node-link ray walk: slab-test each AABB, descend on a hit, Möller–Trumbore the leaf triangles; first hit
    // within range answers true — occlusion needs no nearest-hit ordering.
    static bool RayHit(ReadOnlySpan<float> bounds, ReadOnlySpan<long> nodes, ReadOnlySpan<Vector3> verts, Vector3 origin, Vector3 direction, float maxDistance) { /* the
        stack-based traversal over Decode(nodes[i]) with the shared slab and Möller–Trumbore kernels */ }

    public static ComputeReceipt.Clash Receipt(AccelerationStructure index, Seq<ClashPair> pairs, int candidates, CorrelationId correlation, Duration elapsed) =>
        new(index.Key, candidates, pairs.Count(static pair => pair.Kind == ClashKind.Hard), pairs.Count(static pair => pair.Kind == ClashKind.Clearance), pairs.Count) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    const int ChildShift = 21;
    const long ChildMask = (1L << ChildShift) - 1;

    static Fin<Unit> Admit(AccelerationStructure index, ReadOnlyMemory<float> triangles) {
        var (boundsMem, nodesMem) = index.NodeLink;
        if (boundsMem.Length % 6 != 0 || boundsMem.Length == 0)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-bounds-malformed:{boundsMem.Length}>"));
        if (triangles.Length % 9 != 0)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-triangles-unaligned:{triangles.Length}>"));
        int nodeCount = boundsMem.Length / 6, triCount = triangles.Length / 9;
        if (nodesMem.Length < nodeCount)
            return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-node-block-short:{nodesMem.Length}<{nodeCount}>"));
        var nodes = nodesMem.Span;
        for (int node = 0; node < nodeCount; node++) {
            (bool leaf, int first, int count) = Decode(nodes[node]);
            if (!leaf) {
                if (first < 0 || first + count > nodeCount)
                    return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-child-range-oob:{first}+{count}/{nodeCount}>"));
                continue;
            }
            if (first < 0 || nodeCount + first + count > nodes.Length)
                return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-leaf-range-oob:{first}+{count}>"));
            for (int s = 0; s < count; s++) {
                long primitive = nodes[nodeCount + first + s];
                if (primitive < 0 || primitive >= triCount)
                    return Fin.Fail<Unit>(new ComputeFault.ModelRejected($"<clash-leaf-primitive-oob:{primitive}/{triCount}>"));
            }
        }
        return Fin.Succ(unit);
    }

    static Seq<ClashPair> NodeLinkPairs(ReadOnlyMemory<float> boundsMem, ReadOnlyMemory<long> nodesMem, ReadOnlyMemory<float> trianglesMem, ClashPolicy policy) {
        ReadOnlySpan<float> bounds = boundsMem.Span;
        ReadOnlySpan<long> nodes = nodesMem.Span;
        ReadOnlySpan<Vector3> verts = MemoryMarshal.Cast<float, Vector3>(trianglesMem.Span);
        int nodeCount = bounds.Length / 6;
        using var sink = new ArrayPoolBufferWriter<ClashPair>();
        var stack = new Stack<(int L, int R)>();
        stack.Push((0, 0));
        while (stack.Count > 0) {
            (int l, int r) = stack.Pop();
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
                        foreach (ClashPair pair in Classify(left, right, triLeft, triRight, policy)) {
                            sink.GetSpan(1)[0] = pair;
                            sink.Advance(1);
                        }
                    }
                }
            } else if (rLeaf || (!lLeaf && Diagonal(bounds.Slice(l * 6, 6)) >= Diagonal(bounds.Slice(r * 6, 6)))) {
                for (int c = 0; c < lCount; c++) { stack.Push((lFirst + c, r)); }
            } else {
                for (int c = 0; c < rCount; c++) { stack.Push((l, rFirst + c)); }
            }
        }
        return toSeq(sink.WrittenSpan.ToArray());
    }

    // Duplicate is checked first: a near-coincident pair (parallel normals, near-zero separation) is a duplicate
    // even when it overlaps (signed <= 0), which overlap alone would band Hard. Its band is the tight
    // `DuplicateTolerance`, not the looser `ClearanceThreshold`; `HardOnly` suppresses both lower bands.
    static Option<ClashPair> Classify(long left, long right, (Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b, ClashPolicy policy) {
        (double signed, Vector3 witness) = Separation(a, b);
        return !policy.HardOnly && signed <= policy.DuplicateTolerance && Coincident(a, b)
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
            if (hitA.IsSome) { return (-Penetration(a, b), hitA.IfNone(Vector3.Zero)); }
            Option<Vector3> hitB = SegmentTriangle(vb[e], vb[e + 1], a.A, a.B, a.C);
            if (hitB.IsSome) { return (-Penetration(a, b), hitB.IfNone(Vector3.Zero)); }
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

    // Coincident planes: unit normals parallel within ~2.5° (the duplicate-plane test; the separation magnitude
    // is the call-site `DuplicateTolerance` gate, never re-read here).
    static bool Coincident((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b) {
        Vector3 na = Vector3.Cross(a.B - a.A, a.C - a.A), nb = Vector3.Cross(b.B - b.A, b.C - b.A);
        float la = na.Length(), lb = nb.Length();
        return la >= 1e-12f && lb >= 1e-12f && MathF.Abs(Vector3.Dot(na, nb) / (la * lb)) >= 0.999f;
    }

    static float Penetration((Vector3 A, Vector3 B, Vector3 C) a, (Vector3 A, Vector3 B, Vector3 C) b) {
        Vector3 na = Vector3.Cross(a.B - a.A, a.C - a.A), nb = Vector3.Cross(b.B - b.A, b.C - b.A);
        float la = na.Length(), lb = nb.Length();
        if (la < 1e-12f || lb < 1e-12f) { return 0f; }
        na /= la;
        nb /= lb;
        float depth = 0f;
        depth = MathF.Max(depth, VertexDepth(a.A, b, nb));
        depth = MathF.Max(depth, VertexDepth(a.B, b, nb));
        depth = MathF.Max(depth, VertexDepth(a.C, b, nb));
        depth = MathF.Max(depth, VertexDepth(b.A, a, na));
        depth = MathF.Max(depth, VertexDepth(b.B, a, na));
        depth = MathF.Max(depth, VertexDepth(b.C, a, na));
        return depth;
    }

    static float VertexDepth(Vector3 point, (Vector3 A, Vector3 B, Vector3 C) tri, Vector3 normal) {
        float plane = MathF.Abs(Vector3.Dot(normal, point - tri.A));
        (float closest, _) = PointTriangle(point, tri.A, tri.B, tri.C);
        return closest <= plane + 1e-5f ? plane : 0f;
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
        var stack = new Stack<int>();
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

public static class DigitalTwin {
    public static (TwinVerdict Verdict, TwinFilter Filter) Score(Surrogate baseline, TwinSignal signal, TwinFilter filter, TwinPolicy policy, ClockPolicy clocks) {
        var (values, predictiveBound) = baseline.Predict(new DesignPoint(signal.OperatingPoint, [], []));
        double predicted = values.Head.IfNone(0.0);
        double residual = predicted - signal.Measured;
        var (next, smoothed, normalized, drift) = filter.Update(Math.Abs(residual), policy.DriftThreshold);
        bool spike = Math.Abs(normalized) > policy.SigmaBound && smoothed > predictiveBound;
        bool anomaly = spike || drift;
        double control = anomaly ? policy.ControlGain * residual : 0.0;
        return (new TwinVerdict(signal.SignalId, predicted, signal.Measured, residual, smoothed, normalized, anomaly, drift, control, clocks.Now), next);
    }

    public static ComputeReceipt.Twin Receipt(TwinVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new(verdict.SignalId, verdict.Predicted, verdict.Measured, verdict.Residual, verdict.Anomaly, verdict.ControlDelta) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.SpanStack, Elapsed = elapsed,
        };
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [CLASH_GEOMETRY_SOURCE]-[OPEN]: what live federated-geometry source the consumer hands to `Detect` — the broad-phase build and refit are the geometry `Rasm/Spatial/index#CLASH_SEAM` `SpatialIndex` seam via `ToAcceleration`, never re-minted here, so only the live source at the federation call site is open; consumer wiring at that call site.
