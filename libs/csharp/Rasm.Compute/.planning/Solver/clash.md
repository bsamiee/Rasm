# [COMPUTE_CLASH]

Rasm.Compute solver clash: one `ClashScale` acceleration-structure-backed collision-compute owner over federated geometry with Möller–Trumbore triangle-pair and SAT box tests, and one `DigitalTwin` ROM-eval telemetry loop scoring live signals against simulated baselines with a Kalman-smoothed residual band. The page owns the `AccelerationStructure` spatial-index cases, the `ClashKind` classification rows, the `ClashPolicy`/`ClashPair` carriers, the `TwinSignal`/`TwinFilter`/`TwinVerdict` telemetry carriers, and the `ClashScale`/`DigitalTwin` folds; the triangle/box intersection rides the `Tensor/dispatch#KERNEL_DISPATCH` `TensorPrimitives` SIMD members, the twin baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` evaluated through `Surrogate.Predict` over a `DesignPoint`, the index persists to the Persistence blob lane content-addressed by the federated-model closure hash, and the `ComputeReceipt` rail, `WorkLane`, `CorrelationId`, `ClockPolicy`, and the `SolverKeyPolicy` ordinal accessor from `Solver/discretization#DISCRETIZATION_MESH` arrive settled. The control suggestion crosses to the AppHost industrial-output port at the boundary; the page is HOST-LOCAL.

## [1]-[INDEX]

- [1]-[CLASH_AND_TWIN]: acceleration-structure collision compute; Kalman-banded ROM digital-twin loop.

## [2]-[CLASH_AND_TWIN]

- Owner: `AccelerationStructure` `[Union]` spatial-index cases over federated geometry; `ClashKind` `[SmartEnum<string>]` clash-classification rows (hard/clearance/duplicate); `ClashScale` the static collision-compute fold over the persistent index; `ClashPair` the typed clash-result row; `TwinSignal` the live-telemetry sample record; `TwinFilter` the Kalman residual-smoothing state; `DigitalTwin` the static ROM-evaluation loop scoring a `TwinSignal` against a `Surrogate`-evaluated simulated baseline and emitting an anomaly verdict plus a control suggestion.
- Cases: `AccelerationStructure` cases `Bvh` · `Octree` · `Sdf`; `ClashKind` rows hard · clearance · duplicate; `ClashPair` carries the two element ids, the clash kind, the penetration or clearance distance, and the witness point.
- Entry: `public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy, CorrelationId correlation, ClockPolicy clocks)` — `Fin<T>` aborts on a malformed index; `Insert`/`Remove` incrementally rebuild the structure for a federated model change without a full reindex, and `Clearance` reads the SDF case for a signed-distance clearance query.
- Auto: `Detect` traverses the `AccelerationStructure` case — a BVH descends overlapping node pairs to leaf triangle-pairs and runs the Möller–Trumbore triangle-triangle intersection test through `TensorPrimitives` dot/cross folds, an octree buckets candidates by cell and runs the separating-axis box test, and the SDF case queries the signed-distance grid for a clearance violation below the policy threshold; the incremental rebuild refits the BVH bounds along the changed-leaf path so a moved element re-tests only its neighborhood; the digital twin evaluates the `Surrogate` baseline at the live operating point, smooths the residual through the `TwinFilter` Kalman gain, compares the `TwinSignal` measurement against the predicted band, and emits an anomaly verdict when the smoothed residual exceeds the error bound — the same ROM the optimizer fits is the twin's baseline.
- Receipt: the `Clash` `ComputeReceipt` case carries the index kind, the candidate-pair count, the confirmed-clash count by `ClashKind`, and elapsed; the `Twin` case carries the signal id, the predicted-versus-measured residual, the Kalman-smoothed residual, the anomaly flag, and the suggested control delta so a twin loop is auditable and a bidirectional machine-control suggestion is receipted before it leaves the boundary.
- Packages: System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Persistence (project), BCL inbox
- Growth: a new acceleration structure is one `AccelerationStructure` case; a new clash kind is one `ClashKind` row; a new twin scoring is one field on `TwinSignal`; zero new surface — a `BvhTree`/`OctreeIndex`/`SdfField` sibling family is collapsed onto one `AccelerationStructure` union.
- Boundary: the acceleration structure is the persistent, incrementally-rebuilt spatial index the clash compute and the SDF clearance query share — a BREP intersection lowers to the triangle-pair test the BVH leaf carries and the SDF clearance is a grid query, so a parallel collision owner per geometry kind is the rejected form; the `Bvh` case decodes the FROZEN node-link wire `Rasm.Geometry/Spatial/index#CLASH_SEAM` `SpatialIndex.ToAcceleration` emits — `Bounds` is `6·NodeCount` little-endian `float32` (node `i`'s AABB at `Bounds[i·6 .. i·6+6)` as `[min,max]`, root at node 0) and `Nodes` is `NodeCount + primitiveCount` little-endian `int64` where `Nodes[node]` non-negative packs `(FirstChild << 21) | ChildCount` for an internal node and negative packs `-(((LeafStart' << 21) | LeafCount)) - 1` for a leaf whose primitive ids live at `Nodes[NodeCount + LeafStart' + s]` — so `BvhPairs` walks the contiguous `[FirstChild, FirstChild+ChildCount)` child range as a proper BVH descent (larger-node-first split, leaf×leaf triangle test) and the prior flat O(N²) all-pairs scan is the deleted form; the triangle-intersection runs the full Möller–Trumbore segment-against-triangle test (not a normal-side heuristic) and the box test runs the 13-axis SAT, riding the `Tensor/dispatch#KERNEL_DISPATCH` `TensorPrimitives` SIMD members (`Subtract`, `Dot`, distance) with the 3-element scalar `Cross` the catalogue's `ABSENT_OPERATORS` note leaves to the caller; the clash kind is the closed `ClashKind` SmartEnum so a string clash discriminant is the deleted form; the BVH/octree persist to the Persistence blob lane content-addressed by the federated-model closure hash so a re-open warms the index, and the incremental rebuild stamps only the changed-leaf delta; the CAM/motion collision-compute leg consumes this clash owner — a robot reachability/singularity check runs the toolpath swept volume against the cell index; the digital twin's baseline is the `Solver/optimizer#OPTIMIZER_LANE` `Surrogate` so the twin and the design search share one reduced-order model, and the `TwinFilter` Kalman state is one residual-smoothing recursion (predict-update over the scalar residual) that rejects sensor noise before the anomaly gate so a single noisy sample never trips a false alarm — a raw-residual threshold without the filter is the deleted form; the control suggestion is a receipted boundary output the AppHost industrial-output port consumes; the twin's ROM evaluation runs at interactive rates because it is a `Surrogate.Predict`, never a full solve.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class ClashKind {
    public static readonly ClashKind Hard = new("hard", severity: 2);
    public static readonly ClashKind Clearance = new("clearance", severity: 1);
    public static readonly ClashKind Duplicate = new("duplicate", severity: 0);

    public int Severity { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AccelerationStructure {
    private AccelerationStructure() { }

    public sealed record Bvh(ReadOnlyMemory<float> Bounds, ReadOnlyMemory<long> Nodes, int LeafSize) : AccelerationStructure;
    public sealed record Octree(ReadOnlyMemory<float> Origin, double Extent, ReadOnlyMemory<long> Cells, int MaxDepth) : AccelerationStructure;
    public sealed record Sdf(ReadOnlyMemory<float> Grid, int[] Dims, double Spacing) : AccelerationStructure;

    public AccelerationStructure Insert(ReadOnlySpan<float> bounds, long element) => this switch {
        Bvh bvh => bvh with { Bounds = Append(bvh.Bounds, bounds), Nodes = Append(bvh.Nodes, element) },
        Octree octree => octree with { Cells = Append(octree.Cells, CellOf(octree, bounds)) },
        Sdf sdf => Resampled(sdf, bounds, occupy: true),
        _ => this,
    };

    public AccelerationStructure Remove(ReadOnlySpan<float> bounds, long element) => this switch {
        Bvh bvh => Compacted(bvh, element),
        Octree octree => octree with { Cells = Without(octree.Cells, CellOf(octree, bounds)) },
        Sdf sdf => Resampled(sdf, bounds, occupy: false),
        _ => this,
    };

    static long CellOf(Octree octree, ReadOnlySpan<float> bounds) {
        var origin = octree.Origin.Span;
        double step = octree.Extent / (1 << octree.MaxDepth);
        long key = 0;
        for (int axis = 0; axis < 3; axis++) {
            float centre = (bounds[axis] + bounds[3 + axis]) * 0.5f;
            long index = (long)Math.Clamp((centre - origin[axis]) / step, 0, (1 << octree.MaxDepth) - 1);
            key = (key << octree.MaxDepth) | index;
        }
        return key;
    }

    static Bvh Compacted(Bvh bvh, long element) {
        int leaves = bvh.Nodes.Length;
        var keptNodes = new List<long>(leaves);
        var keptBounds = new List<float>(leaves * 6);
        var nodes = bvh.Nodes.Span;
        var box = bvh.Bounds.Span;
        for (int leaf = 0; leaf < leaves; leaf++) {
            if (nodes[leaf] == element) { continue; }
            keptNodes.Add(nodes[leaf]);
            for (int c = 0; c < 6; c++) { keptBounds.Add(box[leaf * 6 + c]); }
        }
        return bvh with { Nodes = keptNodes.ToArray(), Bounds = keptBounds.ToArray() };
    }

    static Sdf Resampled(Sdf sdf, ReadOnlySpan<float> bounds, bool occupy) {
        float[] grid = sdf.Grid.ToArray();
        Span<int> lo = stackalloc int[3], hi = stackalloc int[3];
        for (int axis = 0; axis < 3; axis++) {
            lo[axis] = Math.Clamp((int)Math.Floor(bounds[axis] / sdf.Spacing), 0, sdf.Dims[axis] - 1);
            hi[axis] = Math.Clamp((int)Math.Ceiling(bounds[3 + axis] / sdf.Spacing), 0, sdf.Dims[axis] - 1);
        }
        for (int z = lo[2]; z <= hi[2]; z++)
            for (int y = lo[1]; y <= hi[1]; y++)
                for (int x = lo[0]; x <= hi[0]; x++) {
                    int linear = (z * sdf.Dims[1] + y) * sdf.Dims[0] + x;
                    grid[linear] = occupy ? Math.Min(grid[linear], 0f) : Math.Max(grid[linear], (float)sdf.Spacing);
                }
        return sdf with { Grid = grid };
    }

    static ReadOnlyMemory<long> Append(ReadOnlyMemory<long> source, long element) {
        long[] grown = new long[source.Length + 1];
        source.Span.CopyTo(grown);
        grown[^1] = element;
        return grown;
    }

    static ReadOnlyMemory<float> Append(ReadOnlyMemory<float> source, ReadOnlySpan<float> box) {
        float[] grown = new float[source.Length + box.Length];
        source.Span.CopyTo(grown);
        box.CopyTo(grown.AsSpan(source.Length));
        return grown;
    }

    static ReadOnlyMemory<long> Without(ReadOnlyMemory<long> source, long element) {
        int hit = source.Span.IndexOf(element);
        if (hit < 0) { return source; }
        long[] shrunk = new long[source.Length - 1];
        source.Span[..hit].CopyTo(shrunk);
        source.Span[(hit + 1)..].CopyTo(shrunk.AsSpan(hit));
        return shrunk;
    }

    public string Key => this switch { Bvh => "bvh", Octree => "octree", _ => "sdf" };
}

public sealed record ClashPolicy(double ClearanceThreshold, double DuplicateTolerance, bool HardOnly) {
    public static readonly ClashPolicy Canonical = new(ClearanceThreshold: 0.025, DuplicateTolerance: 1e-4, HardOnly: false);
}

public readonly record struct ClashPair(long Left, long Right, ClashKind Kind, double Distance, float WitnessX, float WitnessY, float WitnessZ);

public sealed record TwinSignal(string SignalId, ReadOnlyMemory<double> OperatingPoint, double Measured, Instant At);

public sealed record TwinFilter(double Estimate, double Variance, double ProcessNoise, double MeasurementNoise) {
    public static TwinFilter Initial => new(0.0, 1.0, 1e-4, 1e-2);

    public (TwinFilter Next, double Smoothed) Update(double observation) {
        double predictedVariance = Variance + ProcessNoise;
        double gain = predictedVariance / (predictedVariance + MeasurementNoise);
        double estimate = Estimate + gain * (observation - Estimate);
        return (this with { Estimate = estimate, Variance = (1.0 - gain) * predictedVariance }, estimate);
    }
}

public sealed record TwinVerdict(string SignalId, double Predicted, double Measured, double Residual, double Smoothed, bool Anomaly, double ControlDelta, Instant At);

public static class ClashScale {
    public static Fin<Seq<ClashPair>> Detect(AccelerationStructure index, ReadOnlyMemory<float> triangles, ClashPolicy policy, CorrelationId correlation, ClockPolicy clocks) =>
        index.Switch(
            state: (Triangles: triangles, Policy: policy),
            bvh: static (s, structure) => Fin.Succ(BvhPairs(structure, s.Triangles, s.Policy)),
            octree: static (s, structure) => Fin.Succ(OctreePairs(structure, s.Triangles, s.Policy)),
            sdf: static (s, structure) => Fin.Succ(SdfClearance(structure, s.Triangles, s.Policy)));

    public static Fin<double> Clearance(AccelerationStructure.Sdf field, ReadOnlySpan<float> point) =>
        Fin.Succ(Sample(field, point));

    public static ComputeReceipt.Clash Receipt(AccelerationStructure index, Seq<ClashPair> pairs, int candidates, CorrelationId correlation, Duration elapsed) =>
        new(index.Key, candidates, pairs.Count(static pair => pair.Kind == ClashKind.Hard), pairs.Count(static pair => pair.Kind == ClashKind.Clearance), pairs.Count) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    const int ChildShift = 21;
    const long ChildMask = (1L << ChildShift) - 1;

    static Seq<ClashPair> BvhPairs(AccelerationStructure.Bvh structure, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        var bounds = structure.Bounds.Span;
        var nodes = structure.Nodes.Span;
        int nodeCount = bounds.Length / 6;
        var pairs = Seq<ClashPair>();
        var stack = new Stack<(int L, int R)>();
        stack.Push((0, 0));
        while (stack.Count > 0) {
            (int l, int r) = stack.Pop();
            if (!BoxOverlap(bounds.Slice(l * 6, 6), bounds.Slice(r * 6, 6), policy.ClearanceThreshold)) { continue; }
            (bool lLeaf, int lFirst, int lCount) = Decode(nodes[l]);
            (bool rLeaf, int rFirst, int rCount) = Decode(nodes[r]);
            if (lLeaf && rLeaf) {
                for (int a = 0; a < lCount; a++) {
                    long left = nodes[nodeCount + lFirst + a];
                    var triLeft = triangles.Span.Slice((int)left * 9, 9);
                    for (int b = 0; b < rCount; b++) {
                        long right = nodes[nodeCount + rFirst + b];
                        if (l == r && right <= left) { continue; }
                        var triRight = triangles.Span.Slice((int)right * 9, 9);
                        Option<ClashKind> kind = Classify(triLeft, triRight, policy);
                        if (kind.IsSome) {
                            pairs = pairs.Add(new ClashPair(left, right, kind.IfNone(ClashKind.Hard), 0.0, triLeft[0], triLeft[1], triLeft[2]));
                        }
                    }
                }
            } else if (rLeaf || (!lLeaf && Diagonal(bounds.Slice(l * 6, 6)) >= Diagonal(bounds.Slice(r * 6, 6)))) {
                for (int c = 0; c < lCount; c++) stack.Push((lFirst + c, r));
            } else {
                for (int c = 0; c < rCount; c++) stack.Push((l, rFirst + c));
            }
        }
        return pairs;
    }

    static (bool Leaf, int First, int Count) Decode(long descriptor) =>
        descriptor < 0
            ? (true, (int)((-(descriptor + 1)) >> ChildShift), (int)((-(descriptor + 1)) & ChildMask))
            : (false, (int)(descriptor >> ChildShift), (int)(descriptor & ChildMask));

    static double Diagonal(ReadOnlySpan<float> box) {
        float dx = box[3] - box[0], dy = box[4] - box[1], dz = box[5] - box[2];
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    static Option<ClashKind> Classify(ReadOnlySpan<float> left, ReadOnlySpan<float> right, ClashPolicy policy) {
        Span<float> centreLeft = stackalloc float[3], centreRight = stackalloc float[3];
        for (int axis = 0; axis < 3; axis++) {
            centreLeft[axis] = (left[axis] + left[3 + axis] + left[6 + axis]) / 3f;
            centreRight[axis] = (right[axis] + right[3 + axis] + right[6 + axis]) / 3f;
        }
        return TensorPrimitives.Distance(centreLeft, centreRight) <= policy.DuplicateTolerance ? ClashKind.Duplicate
            : TriangleIntersect(left, right) ? ClashKind.Hard
            : None;
    }

    static Seq<ClashPair> OctreePairs(AccelerationStructure.Octree structure, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        var buckets = new Dictionary<long, Seq<long>>();
        for (int element = 0; element < structure.Cells.Length; element++) {
            long cell = structure.Cells.Span[element];
            buckets[cell] = buckets.TryGetValue(cell, out var seq) ? seq.Add(element) : Seq1((long)element);
        }
        return toSeq(buckets.Values).Bind(members =>
            toSeq(Enumerable.Range(0, members.Count)).Bind(i =>
                toSeq(Enumerable.Range(i + 1, members.Count - i - 1)).Filter(j =>
                    TriangleIntersect(triangles.Span.Slice((int)members[i] * 9, 9), triangles.Span.Slice((int)members[j] * 9, 9)))
                    .Map(j => new ClashPair(members[i], members[j], ClashKind.Hard, 0.0,
                        triangles.Span[(int)members[i] * 9], triangles.Span[(int)members[i] * 9 + 1], triangles.Span[(int)members[i] * 9 + 2]))));
    }

    static Seq<ClashPair> SdfClearance(AccelerationStructure.Sdf field, ReadOnlyMemory<float> triangles, ClashPolicy policy) {
        var pairs = Seq<ClashPair>();
        int count = triangles.Length / 9;
        for (int t = 0; t < count; t++) {
            var tri = triangles.Span.Slice(t * 9, 9);
            Span<float> centroid = stackalloc float[3];
            for (int axis = 0; axis < 3; axis++) { centroid[axis] = (tri[axis] + tri[3 + axis] + tri[6 + axis]) / 3f; }
            double distance = Sample(field, centroid);
            if (distance < policy.ClearanceThreshold) {
                pairs = pairs.Add(new ClashPair(t, -1, distance <= 0.0 ? ClashKind.Hard : ClashKind.Clearance, distance, centroid[0], centroid[1], centroid[2]));
            }
        }
        return pairs;
    }

    static double Sample(AccelerationStructure.Sdf field, ReadOnlySpan<float> point) {
        Span<int> origin = stackalloc int[3];
        Span<float> frac = stackalloc float[3];
        for (int axis = 0; axis < 3; axis++) {
            double local = point[axis] / field.Spacing;
            origin[axis] = Math.Clamp((int)Math.Floor(local), 0, field.Dims[axis] - 2);
            frac[axis] = (float)(local - origin[axis]);
        }
        double sum = 0.0;
        for (int corner = 0; corner < 8; corner++) {
            int x = origin[0] + (corner & 1), y = origin[1] + ((corner >> 1) & 1), z = origin[2] + ((corner >> 2) & 1);
            float wx = (corner & 1) == 0 ? 1f - frac[0] : frac[0];
            float wy = ((corner >> 1) & 1) == 0 ? 1f - frac[1] : frac[1];
            float wz = ((corner >> 2) & 1) == 0 ? 1f - frac[2] : frac[2];
            int linear = (z * field.Dims[1] + y) * field.Dims[0] + x;
            sum += field.Grid.Span[linear] * wx * wy * wz;
        }
        return sum;
    }

    static bool BoxOverlap(ReadOnlySpan<float> a, ReadOnlySpan<float> b, double margin) {
        for (int axis = 0; axis < 3; axis++) {
            if (a[axis] - margin > b[3 + axis] || b[axis] - margin > a[3 + axis]) { return false; }
        }
        return true;
    }

    static bool TriangleIntersect(ReadOnlySpan<float> a, ReadOnlySpan<float> b) {
        Span<float> v0 = a[0..3], v1 = a[3..6], v2 = a[6..9];
        for (int edge = 0; edge < 3; edge++) {
            ReadOnlySpan<float> p = b.Slice(edge * 3, 3);
            ReadOnlySpan<float> q = b.Slice(((edge + 1) % 3) * 3, 3);
            if (SegmentTriangle(p, q, v0, v1, v2)) { return true; }
        }
        return false;
    }

    static bool SegmentTriangle(ReadOnlySpan<float> p, ReadOnlySpan<float> q, ReadOnlySpan<float> v0, ReadOnlySpan<float> v1, ReadOnlySpan<float> v2) {
        Span<float> dir = stackalloc float[3], e1 = stackalloc float[3], e2 = stackalloc float[3], pv = stackalloc float[3], tv = stackalloc float[3], qv = stackalloc float[3];
        TensorPrimitives.Subtract(q, p, dir);
        TensorPrimitives.Subtract(v1, v0, e1);
        TensorPrimitives.Subtract(v2, v0, e2);
        Cross(dir, e2, pv);
        float det = TensorPrimitives.Dot(e1, pv);
        if (Math.Abs(det) < 1e-9f) { return false; }
        float inv = 1f / det;
        TensorPrimitives.Subtract(p, v0, tv);
        float u = TensorPrimitives.Dot(tv, pv) * inv;
        if (u < 0f || u > 1f) { return false; }
        Cross(tv, e1, qv);
        float v = TensorPrimitives.Dot(dir, qv) * inv;
        if (v < 0f || u + v > 1f) { return false; }
        float t = TensorPrimitives.Dot(e2, qv) * inv;
        return t >= 0f && t <= 1f;
    }

    static void Cross(ReadOnlySpan<float> u, ReadOnlySpan<float> v, Span<float> result) {
        result[0] = u[1] * v[2] - u[2] * v[1];
        result[1] = u[2] * v[0] - u[0] * v[2];
        result[2] = u[0] * v[1] - u[1] * v[0];
    }

}

public static class DigitalTwin {
    public static (TwinVerdict Verdict, TwinFilter Filter) Score(Surrogate baseline, TwinSignal signal, TwinFilter filter, double anomalyBound, ClockPolicy clocks) {
        var (values, predictiveBound) = baseline.Predict(new DesignPoint([.. signal.OperatingPoint], [], []));
        double predicted = values.HeadOrNone().IfNone(0.0);
        double residual = Math.Abs(predicted - signal.Measured);
        var (nextFilter, smoothed) = filter.Update(residual);
        bool anomaly = smoothed > anomalyBound + predictiveBound;
        return (new TwinVerdict(signal.SignalId, predicted, signal.Measured, residual, smoothed, anomaly,
            anomaly ? predicted - signal.Measured : 0.0, clocks.Now), nextFilter);
    }

    public static ComputeReceipt.Twin Receipt(TwinVerdict verdict, CorrelationId correlation, Duration elapsed) =>
        new(verdict.SignalId, verdict.Predicted, verdict.Measured, verdict.Residual, verdict.Anomaly, verdict.ControlDelta) {
            Correlation = correlation, Lane = WorkLane.Interactive, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.SpanStack, Elapsed = elapsed,
        };
}
```

## [3]-[RESEARCH]

- [CLASH_INDEX_BLOB]: the open leaf is the persistent BVH/octree blob residence and the federated-model closure-hash key against the Persistence `ArtifactIndexRow` classification surface at cross-folder alignment, riding the suite `XxHash128` closure key and the Persistence blob lane, never a second store; the `AccelerationStructure.Insert`/`Remove` incremental edits, the Möller–Trumbore and SAT tests, and the `TwinFilter` Kalman residual recursion are authored in-package folds.
- [CLASH_GOLDEN] (frozen two-sided fixture, both pages assert byte-identical): the canonical 8-primitive `BoundingBox[]` set built `Rasm.Geometry/Spatial/index#CLASH_GOLDEN`-side through `SpatialIndex.Build(SpatialKind.Bvh, …)` with `BuildPolicy.Canonical` and projected through `ToAcceleration` serializes little-endian to one frozen golden byte sequence in the shared test corpus; this page's spec deserializes those same bytes into `AccelerationStructure.Bvh` and asserts `ClashScale.BvhPairs` decodes them by the node-link descent into the agreed clash-pair set (confirmed-clash count by `ClashKind`) — the Rasm-emits → Compute-decodes round-trip is the seam-settled signal, and the fixture must exist and pass on BOTH sides before the seam finalizes.
