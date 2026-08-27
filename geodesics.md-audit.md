# 1. Use computation values as memo keys

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:46`

```csharp
[StructLayout(LayoutKind.Auto)] internal readonly record struct FrameBundleKey();
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:74`

```csharp
internal readonly record struct GeodesicKey(Seq<int> Sources);
[StructLayout(LayoutKind.Auto)] internal readonly record struct McfKey(double TimeStep, int Iterations);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:239`

```csharp
internal readonly record struct WindowPropagation(WindowField Field, double[] VertexDistance);
[StructLayout(LayoutKind.Auto)] internal readonly record struct WindowFieldKey(int Source, WindowPropagationPolicy Policy);
[StructLayout(LayoutKind.Auto)] internal readonly record struct ConeAngleKey();
[StructLayout(LayoutKind.Auto)] internal readonly record struct IntrinsicFaceIndexKey();
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:780`

```csharp
[StructLayout(LayoutKind.Auto)] internal readonly record struct VectorHeatKey(double Time, Seq<(int Vertex, Vector3d Direction)> Sources);
```

## To

```csharp
// FrameBundleKey DELETED
// GeodesicKey DELETED
// McfKey DELETED
// WindowFieldKey DELETED
// ConeAngleKey DELETED
// IntrinsicFaceIndexKey DELETED
// VectorHeatKey DELETED
```

```csharp
space.Cache.Memoized(probe: unit, compute: () => Fin.Succ(Compute(mesh: space.Native)));
space.Cache.Memoized(probe: ordered, compute: /* heat-distance solve */);
space.Cache.Memoized(probe: (timeStep, iterations), compute: /* mean-curvature solve */);
space.Cache.Memoized(probe: (source, policy), compute: /* window propagation */);
space.Cache.Memoized(probe: unit, compute: () => Fin.Succ(ConeAnglesOf(imesh: imesh)));
space.Cache.Memoized(probe: unit, compute: /* intrinsic-face index */);
space.Cache.Memoized(probe: (time, ordered), compute: /* vector-heat solve */);
```

## Why

`LaplacianCache.Memoized` already partitions entries by both `TKey` and result type. The seven records add names and generated positional members around values that already have structural equality: `Unit`, `Seq<T>`, and value tuples are the actual cache identities.

## Change

Pass each computation's admitted value product directly to `Memoized`; use `unit` only for parameterless computations, whose different result types keep their slots distinct. Delete every cache-key wrapper and its constructor calls.

## Delta

Code-fence LOC: `-7`. Source-declared module surface: `-7` types and `-7` positional members; `0` replacement types or members.

## Ripples

No consumer owns a key type. Calls to `FrameBundle.Of` in `libs/dotnet/Rasm/.planning/Processing/segment.md` follow the operation-parameter removal in task 2.

# 2. Remove dead trace extent and optional-operation ceremony

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:197`

```csharp
public readonly record struct GeodesicTracePolicy(PositiveMagnitude TraceLengthFactor, Dimension MaxSteps, UnitInterval VertexSnap, Option<Set<int>> Barrier) {
    public static readonly GeodesicTracePolicy Default = new(TraceLengthFactor: PositiveMagnitude.Create(value: 64.0), MaxSteps: Dimension.Create(value: 4096), VertexSnap: UnitInterval.Create(value: 1.0e-6), Barrier: Option<Set<int>>.None);
    public static Fin<GeodesicTracePolicy> Of(double traceLengthFactor, int maxSteps, double vertexSnap, Option<Set<int>> barrier = default, Op? key = null) =>
        key.OrDefault() switch {
            Op op => from factor in op.AcceptValidated<PositiveMagnitude>(candidate: traceLengthFactor)
                     from steps in op.AcceptValidated<Dimension>(candidate: maxSteps)
                     from snap in op.AcceptValidated<UnitInterval>(candidate: vertexSnap)
                     select new GeodesicTracePolicy(TraceLengthFactor: factor, MaxSteps: steps, VertexSnap: snap, Barrier: barrier),
        };
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:209`

```csharp
public readonly record struct WindowPropagationPolicy(Dimension MaxWindowsPerEdge, Dimension BacktraceMaxHops, PositiveMagnitude SaddleAngleThreshold, bool ReportCutLocus) {
    public static readonly WindowPropagationPolicy Default = new(MaxWindowsPerEdge: Dimension.Create(value: 512), BacktraceMaxHops: Dimension.Create(value: 4096), SaddleAngleThreshold: PositiveMagnitude.Create(value: Math.Tau), ReportCutLocus: false);
    public static Fin<WindowPropagationPolicy> Of(int maxWindowsPerEdge, int backtraceMaxHops, double saddleAngleThreshold, bool reportCutLocus, Op? key = null) =>
        key.OrDefault() switch {
            Op op => from windows in op.AcceptValidated<Dimension>(candidate: maxWindowsPerEdge)
                     from hops in op.AcceptValidated<Dimension>(candidate: backtraceMaxHops)
                     from saddle in op.AcceptValidated<PositiveMagnitude>(candidate: saddleAngleThreshold)
                     select new WindowPropagationPolicy(MaxWindowsPerEdge: windows, BacktraceMaxHops: hops, SaddleAngleThreshold: saddle, ReportCutLocus: reportCutLocus),
        };
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:49`

```csharp
internal static Fin<FrameBundle> Of(MeshSpace space, Op key) =>
    space.Cache.Memoized(probe: new FrameBundleKey(), compute: () => Fin.Succ(Compute(mesh: space.Native)));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:943`

```csharp
private static Fin<double[]> ConeAngles(MeshSpace space, IntrinsicMesh imesh, Op key) =>
    space.Cache.Memoized(probe: new ConeAngleKey(), compute: () => Fin.Succ(ConeAnglesOf(imesh: imesh)));
```

## To

```csharp
public readonly record struct GeodesicTracePolicy(Dimension MaxSteps, UnitInterval VertexSnap, Option<Set<int>> Barrier) {
    public static readonly GeodesicTracePolicy Default = new(MaxSteps: Dimension.Create(value: 4096), VertexSnap: UnitInterval.Create(value: 1.0e-6), Barrier: None);
    public static Fin<GeodesicTracePolicy> Of(int maxSteps, double vertexSnap, Op key, Option<Set<int>> barrier = default) =>
        from steps in key.AcceptValidated<Dimension>(candidate: maxSteps)
        from snap in key.AcceptValidated<UnitInterval>(candidate: vertexSnap)
        select new GeodesicTracePolicy(MaxSteps: steps, VertexSnap: snap, Barrier: barrier);
}

public readonly record struct WindowPropagationPolicy(Dimension MaxWindowsPerEdge, Dimension BacktraceMaxHops, PositiveMagnitude SaddleAngleThreshold, bool ReportCutLocus) {
    public static readonly WindowPropagationPolicy Default = new(MaxWindowsPerEdge: Dimension.Create(value: 512), BacktraceMaxHops: Dimension.Create(value: 4096), SaddleAngleThreshold: PositiveMagnitude.Create(value: Math.Tau), ReportCutLocus: false);
    public static Fin<WindowPropagationPolicy> Of(int maxWindowsPerEdge, int backtraceMaxHops, double saddleAngleThreshold, bool reportCutLocus, Op key) =>
        from windows in key.AcceptValidated<Dimension>(candidate: maxWindowsPerEdge)
        from hops in key.AcceptValidated<Dimension>(candidate: backtraceMaxHops)
        from saddle in key.AcceptValidated<PositiveMagnitude>(candidate: saddleAngleThreshold)
        select new WindowPropagationPolicy(MaxWindowsPerEdge: windows, BacktraceMaxHops: hops, SaddleAngleThreshold: saddle, ReportCutLocus: reportCutLocus);
}

internal static Fin<FrameBundle> Of(MeshSpace space) =>
    space.Cache.Memoized(probe: unit, compute: () => Fin.Succ(Compute(mesh: space.Native)));

private static Fin<double[]> ConeAngles(MeshSpace space, IntrinsicMesh imesh) =>
    space.Cache.Memoized(probe: unit, compute: () => Fin.Succ(ConeAnglesOf(imesh: imesh)));
```

## Why

The exact-exp near-source branch returns before tracing, so every live trace request is `seat.Chord`; `TraceLengthFactor` never controls an execution. The policy factories are kernel admissions and already receive validation through `Op`, while the frame and cone memos cannot emit operation-specific failures.

## Change

Delete `TraceLengthFactor` and always pass `seat.Chord` to the walk. Require `Op` directly in both composite factories, remove the one-arm `OrDefault` switches, and remove unused `Op` parameters from the two infallible memo probes.

## Delta

Code-fence LOC: `-8`. Source-declared module surface: `-1` policy member; `0` types and methods net. Four parameters and two one-arm switches are removed.

## Ripples

Update policy construction sites to omit `traceLengthFactor`. Change `FrameBundle.Of(space: space, key: key)` to `FrameBundle.Of(space: space)` in `libs/dotnet/Rasm/.planning/Processing/segment.md` and throughout the target; no compatibility overloads remain.

# 3. Inline the one-call heat solve

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:120`

```csharp
internal static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
    int n = space.Native.Vertices.Count;
    Seq<int> ordered = toSeq(sources.AsIterable().Distinct().Order());
    return ordered.IsEmpty || ordered.Exists(i => i < 0 || i >= n)
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : space.Cache.Memoized(probe: new GeodesicKey(Sources: ordered),
            compute: () => from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
                           from _ in guard(!imesh.HasFlips, key.Unsupported(inputType: typeof(IntrinsicMesh), outputType: typeof(Arr<double>)))
                           from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                           from distances in ComputeHeatGeodesic(space: space, laplacian: laplacian, sources: ordered, key: key)
                           select distances);
}
private static Fin<Arr<double>> ComputeHeatGeodesic(MeshSpace space, SparseLaplacian laplacian, Seq<int> sources, Op key) {
    int n = space.Native.Vertices.Count;
    double h = space.Cache.MeanEdgeLength;
    if (h <= EpsilonPolicy.ZeroTolerance) return Fin.Fail<Arr<double>>(key.InvalidResult());
    double t = h * h;
    return from heatFactor in space.Cache.ScalarHeatCholesky(time: t, key: key)
           from delta in Fin.Succ(DecAssembly.SourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
           from u in Solved(heatFactor.SolveDetailed(rhs: delta, key: key), key: key)
           from gradient in Fin.Succ(DecAssembly.FaceGradients(mesh: space.Native, u: u))
           from divergence in Fin.Succ(DecAssembly.Divergence(mesh: space.Native, gradients: gradient))
           from phi in Solved(laplacian.Stiffness.SingularSolveDetailed(rhs: divergence, gauge: GaugePolicy.Pinned(indices: [.. sources], mass: Some(laplacian.MassLumped), shift: GaugeShift.MinZero), context: space.Tolerance, key: key), key: key)
           select phi;
}
```

## To

```csharp
internal static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
    int n = space.Native.Vertices.Count;
    Seq<int> ordered = toSeq(sources.AsIterable().Distinct().Order());
    double h = space.Cache.MeanEdgeLength;
    return ordered.IsEmpty || ordered.Exists(i => i < 0 || i >= n)
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : h <= EpsilonPolicy.ZeroTolerance
            ? Fin.Fail<Arr<double>>(key.InvalidResult())
            : space.Cache.Memoized(probe: ordered,
                compute: () => from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
                               from _ in guard(!imesh.HasFlips, key.Unsupported(inputType: typeof(IntrinsicMesh), outputType: typeof(Arr<double>)))
                               from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                               from heat in space.Cache.ScalarHeatCholesky(time: h * h, key: key)
                               let delta = DecAssembly.SourceDelta(n: n, sources: ordered, mass: laplacian.MassLumped)
                               from u in Solved(heat.SolveDetailed(rhs: delta, key: key), key: key)
                               let gradient = DecAssembly.FaceGradients(mesh: space.Native, u: u)
                               let divergence = DecAssembly.Divergence(mesh: space.Native, gradients: gradient)
                               from distance in Solved(laplacian.Stiffness.SingularSolveDetailed(rhs: divergence, gauge: GaugePolicy.Pinned(indices: [.. ordered], mass: Some(laplacian.MassLumped), shift: GaugeShift.MinZero), context: space.Tolerance, key: key), key: key)
                               select distance);
}

// ComputeHeatGeodesic DELETED
```

## Why

`ComputeHeatGeodesic` has one caller and only splits one cache computation. `SourceDelta`, `FaceGradients`, and `Divergence` are pure projections, so lifting them into successful `Fin` values adds effect syntax without failure semantics.

## Change

Move the scale guard and heat pipeline into the memo computation, keep only actual factorization and solve failures in the LanguageExt query, and bind pure DEC projections with `let`.

## Delta

Code-fence LOC: `-5`. Source-declared module surface: `-1` method; `0` types or members added.

# 4. Collapse mean-curvature stages into one monadic fold

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:147`

```csharp
internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key) =>
    from displacements in EnsureMcfDisplacements(space: space, timeStep: timeStep, iterations: iterations, key: key)
    from value in MeshProbe.ScalarOn(space: space, sample: sample, perVertex: displacements, key: key)
    select value;
private static Fin<Arr<double>> EnsureMcfDisplacements(MeshSpace space, double timeStep, int iterations, Op key) =>
    !double.IsFinite(x: timeStep) || timeStep <= 0.0 || iterations < 1
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : space.Cache.Memoized(probe: new McfKey(TimeStep: timeStep, Iterations: iterations),
            compute: () => space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                .Bind(laplacian => from system in MeshKernel.AssembleMassStiffnessSystem(laplacian: laplacian, stiffnessScale: timeStep, key: key)
                                   from factor in CholeskySparse.Of(symmetric: system, key: key)
                                   from final in IterateMcf(space: space, mass: laplacian.MassLumped, system: factor, iterations: iterations, key: key)
                                   select ComputeDisplacements(original: space.Native, smoothed: final)));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:160`

```csharp
return toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
    Fin.Succ(coordinates),
    (state, _) => state.Bind(current => {
        double[][] rhs = [new double[n], new double[n], new double[n]];
        for (int axis = 0; axis < rhs.Length; axis++) TensorPrimitives.Multiply<double>(weights, current[axis], rhs[axis]);
        return toSeq(rhs).TraverseM(axis => Solved(system.SolveDetailed(rhs: new Arr<double>(axis), key: key), key: key).Map(solution => solution.AsIterable().ToArray())).As().Map(axes => axes.AsIterable().ToArray());
    }));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:173`

```csharp
private static Arr<double> ComputeDisplacements(Mesh original, double[][] smoothed) {
    int n = original.Vertices.Count;
    double[] magnitude = new double[n];
    for (int i = 0; i < n; i++) {
        Point3d before = original.Vertices[index: i];
        magnitude[i] = new Vector3d(x: smoothed[0][i] - before.X, y: smoothed[1][i] - before.Y, z: smoothed[2][i] - before.Z).Length;
    }
    return new Arr<double>(magnitude);
}
```

## To

```csharp
internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key) {
    if (!double.IsFinite(x: timeStep) || timeStep <= 0.0 || iterations < 1)
        return Fin.Fail<double>(key.InvalidInput());
    return from displacement in space.Cache.Memoized(probe: (timeStep, iterations), compute: () =>
               from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
               from system in MeshKernel.AssembleMassStiffnessSystem(laplacian: laplacian, stiffnessScale: timeStep, key: key)
               from factor in CholeskySparse.Of(symmetric: system, key: key)
               from displacement in IterateMcf(space: space, mass: laplacian.MassLumped, system: factor, iterations: iterations, key: key)
               select displacement)
           from value in MeshProbe.ScalarOn(space: space, sample: sample, perVertex: displacement, key: key)
           select value;
}

// EnsureMcfDisplacements DELETED
// ComputeDisplacements DELETED
```

```csharp
private static Fin<Arr<double>> IterateMcf(MeshSpace space, Arr<double> mass, CholeskySparse system, int iterations, Op key) {
    int n = space.Native.Vertices.Count;
    double[][] coordinates = [new double[n], new double[n], new double[n]];
    for (int i = 0; i < n; i++) {
        Point3d v = space.Native.Vertices[index: i];
        coordinates[0][i] = v.X; coordinates[1][i] = v.Y; coordinates[2][i] = v.Z;
    }
    double[] weights = [.. mass.AsIterable()];
    return toSeq(Enumerable.Range(start: 0, count: iterations))
    .FoldM(coordinates, (current, _) => {
        double[][] rhs = [new double[n], new double[n], new double[n]];
        for (int axis = 0; axis < rhs.Length; axis++)
            TensorPrimitives.Multiply<double>(weights, current[axis], rhs[axis]);
        return toSeq(rhs).TraverseM(axis => Solved(system.SolveDetailed(rhs: new Arr<double>(axis), key: key), key: key)
            .Map(solution => solution.AsIterable().ToArray())).As().Map(axes => axes.AsIterable().ToArray());
    }).As()
    .Map(smoothed => {
        double[] magnitude = new double[n];
        for (int i = 0; i < n; i++) {
            Point3d before = space.Native.Vertices[index: i];
            magnitude[i] = new Vector3d(
                x: smoothed[0][i] - before.X,
                y: smoothed[1][i] - before.Y,
                z: smoothed[2][i] - before.Z).Length;
        }
        return new Arr<double>(magnitude);
    });
}
```

## Why

The cache, iteration, and magnitude projection form one operation. `EnsureMcfDisplacements` and `ComputeDisplacements` are single-call partitions, while `Fold(Fin.Succ(seed), state.Bind(...))` manually reconstructs the `FoldM` sequencing LanguageExt already provides. A LINQ projection over every vertex would add iterator and collection materialization to the numerical hot path, so the existing array loop remains inside the retained iteration kernel.

## Change

Own validation, memoization, and sampling in the public operation; make `IterateMcf` return the final displacement field directly, preserve its array projection loop, and express only the effectful recurrence with `FoldM` plus the existing `TraverseM` axis solve.

## Delta

Code-fence LOC: `-6`. Source-declared module surface: `-2` methods; `0` types or members added. The retained `IterateMcf` changes result type from `Fin<double[][]>` to `Fin<Arr<double>>` without adding a method or carrier.

# 5. Remove unused and one-call probe helpers

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:77`

```csharp
internal static double SearchDistance(MeshSpace space) =>
    Math.Max(val1: space.Tolerance.Absolute.Value, val2: space.Cache.MeanEdgeLength);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:86`

```csharp
internal static Fin<double> ScalarOn(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) =>
    ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => key.AcceptValue(value: FaceValue(face: face, weights: weights, perVertex: perVertex)));
internal static Fin<Vector3d> VectorOn(MeshSpace space, Point3d sample, Arr<Vector3d> perVertex, Op key) =>
    ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => key.AcceptValue(value: BarycentricVector(face: face, weights: weights, at: vertex => perVertex[index: vertex])));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:94`

```csharp
internal static double FaceValue(MeshFace face, double[] weights, Arr<double> perVertex) {
    double value = (weights[0] * perVertex[index: face.A]) + (weights[1] * perVertex[index: face.B]) + (weights[2] * perVertex[index: face.C]);
    return face.IsQuad ? value + (weights[3] * perVertex[index: face.D]) : value;
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:813`

```csharp
for (int s = 0; s < sources.Count; s++) {
    (int v, Vector3d direction) = sources[index: s];
    if (v < 0 || v >= n) continue;
    if (frames.Tangent(direction: direction, vertex: v).Case is not Complex seated) continue;
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:957`

```csharp
private static double SafeVertexDistance(double[] vertexDistance, int vertex) =>
    vertex >= 0 && vertex < vertexDistance.Length ? vertexDistance[vertex] : double.PositiveInfinity;
```

## To

```csharp
// MeshProbe.SearchDistance DELETED
// MeshProbe.VectorOn DELETED
// MeshProbe.FaceValue DELETED
// SafeVertexDistance DELETED
```

```csharp
MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample,
    maximumDistance: Math.Max(space.Tolerance.Absolute.Value, space.Cache.MeanEdgeLength));

internal static Fin<double> ScalarOn(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) =>
    ClosestFace(space, sample, key, (_, face, weights, _) => {
        double value = (weights[0] * perVertex[face.A]) + (weights[1] * perVertex[face.B]) + (weights[2] * perVertex[face.C]);
        return key.AcceptValue(face.IsQuad ? value + (weights[3] * perVertex[face.D]) : value);
    });

for (int s = 0; s < sources.Count; s++) {
    (int v, Vector3d direction) = sources[s];
    if (frames.Tangent(direction, v).Case is not Complex seated) continue;

double distance = (weights[0] * wave.VertexDistance[face.A])
    + (weights[1] * wave.VertexDistance[face.B])
    + (weights[2] * wave.VertexDistance[face.C]);
```

## Why

`VectorOn` has no consumer. The other three methods each serve one expression. Source vertices are admitted before vector-heat encoding, and Rhino and intrinsic face owners already guarantee the vertex indices used by interpolation; their interior range checks duplicate ownership.

## Change

Delete the unused sampler, inline the search bound and scalar interpolation at their only consumers, index admitted source and face vertices directly, and retain `BarycentricVector`, whose multiple vector/complex consumers justify it.

## Delta

Code-fence LOC: `-5`. Source-declared module surface: `-4` methods; `0` types or members added.

# 6. Collapse vector-heat staging and its private carrier

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:789`

```csharp
private static Fin<Complex[]> EnsureVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
    int n = space.Native.Vertices.Count;
    Seq<(int Vertex, Vector3d Direction)> ordered = toSeq(sources.AsIterable()
        .OrderBy(static s => s.Vertex).ThenBy(static s => s.Direction.X).ThenBy(static s => s.Direction.Y).ThenBy(static s => s.Direction.Z));
    return ordered.IsEmpty || !double.IsFinite(x: time) || time <= 0.0 || ordered.Exists(s => s.Vertex < 0 || s.Vertex >= n || !s.Direction.IsValid || s.Direction.IsTiny())
        ? Fin.Fail<Complex[]>(key.InvalidInput())
        : space.Cache.Memoized(probe: new VectorHeatKey(Time: time, Sources: ordered),
            compute: () => ComputeVectorHeat(space: space, sources: ordered, time: time, key: key));
}
private static Fin<Complex[]> ComputeVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
    int n = space.Native.Vertices.Count;
    return from frames in FrameBundle.Of(space: space, key: key)
           from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
           from connectionFactor in space.Cache.ConnectionCholesky(symmetry: 1, time: time, edgeAdjustment: Option<Arr<double>>.None, key: key)
           from scalarFactor in space.Cache.ScalarHeatCholesky(time: time, key: key)
           let rhs = EncodeVectorHeatSources(n: n, sources: sources, frames: frames, mass: laplacian.MassLumped)
           from direction in Solved(connectionFactor.SolveDetailed(rhs: rhs.StackedDirection, key: key), key: key)
           from magnitude in Solved(scalarFactor.SolveDetailed(rhs: rhs.Magnitude, key: key), key: key)
           from indicator in Solved(scalarFactor.SolveDetailed(rhs: rhs.Indicator, key: key), key: key)
           select RecoverVectorHeat(n: n, direction: direction, magnitude: magnitude, indicator: indicator);
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:810`

```csharp
private sealed record VectorHeatRhs(Arr<double> StackedDirection, Arr<double> Magnitude, Arr<double> Indicator);
private static VectorHeatRhs EncodeVectorHeatSources(int n, Seq<(int Vertex, Vector3d Direction)> sources, FrameBundle frames, Arr<double> mass) {
```

## To

```csharp
// ComputeVectorHeat DELETED
// VectorHeatRhs DELETED
```

```csharp
private static Fin<Complex[]> EnsureVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
    int n = space.Native.Vertices.Count;
    Seq<(int Vertex, Vector3d Direction)> ordered = toSeq(sources.AsIterable()
        .OrderBy(static s => s.Vertex).ThenBy(static s => s.Direction.X).ThenBy(static s => s.Direction.Y).ThenBy(static s => s.Direction.Z));
    return ordered.IsEmpty || !double.IsFinite(x: time) || time <= 0.0
            || ordered.Exists(s => s.Vertex < 0 || s.Vertex >= n || !s.Direction.IsValid || s.Direction.IsTiny())
        ? Fin.Fail<Complex[]>(key.InvalidInput())
        : space.Cache.Memoized(probe: (time, ordered), compute: () =>
            from frames in FrameBundle.Of(space: space)
            from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
            from connection in space.Cache.ConnectionCholesky(symmetry: 1, time: time, edgeAdjustment: None, key: key)
            from heat in space.Cache.ScalarHeatCholesky(time: time, key: key)
            let rhs = EncodeVectorHeatSources(n: n, sources: ordered, frames: frames, mass: laplacian.MassLumped)
            from direction in Solved(connection.SolveDetailed(rhs: rhs.StackedDirection, key: key), key: key)
            from magnitude in Solved(heat.SolveDetailed(rhs: rhs.Magnitude, key: key), key: key)
            from indicator in Solved(heat.SolveDetailed(rhs: rhs.Indicator, key: key), key: key)
            select RecoverVectorHeat(n: n, direction: direction, magnitude: magnitude, indicator: indicator));
}

private static (Arr<double> StackedDirection, Arr<double> Magnitude, Arr<double> Indicator) EncodeVectorHeatSources(
    int n, Seq<(int Vertex, Vector3d Direction)> sources, FrameBundle frames, Arr<double> mass) {
```

## Why

`ComputeVectorHeat` is a one-call continuation of the cache miss and owns no independent admission or algorithm. `VectorHeatRhs` is a private three-array transfer product with no invariant or behavior; named tuple elements preserve every use-site meaning without minting a type.

## Change

Move the solve query into `EnsureVectorHeat`'s memo computation, return the three encoded right-hand sides as a named tuple, and delete the helper method and private record. Keep `EncodeVectorHeatSources` and `RecoverVectorHeat` as the substantial statement kernels around the package-owned solves.

## Delta

Code-fence LOC: `-3`. Source-declared module surface: `-1` method, `-1` type, and `-3` positional members; `0` replacement types or members.

# 7. Inline trivial log-map helpers

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:870`

```csharp
private static Fin<Vector3d> TransportedLog(Vector3d transported, double scale, Op key) {
    Vector3d unit = transported;
    return unit.IsValid && unit.Unitize() ? key.AcceptValue(value: scale * unit) : Fin.Fail<Vector3d>(key.InvalidResult());
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:945`

```csharp
private static Fin<HashMap<(int A, int B, int C), int>> IntrinsicFaceIndex(MeshSpace space, IntrinsicMesh imesh, Op key) =>
    space.Cache.Memoized(probe: new IntrinsicFaceIndexKey(), compute: () => Fin.Succ(toHashMap(
        imesh.LiveFaceIndices()
            .Select(f => (Key: SortedTriple(face: imesh.Triangles[index: f]!.Value), Face: f))
            .DistinctBy(static row => row.Key))));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:955`

```csharp
private static int IntrinsicFaceOfVertices(HashMap<(int A, int B, int C), int> index, IntrinsicMesh imesh, int a, int b, int c) =>
    index.Find(key: SortedTriple(face: (a, b, c))).IfNone(() => FirstLiveFaceAt(imesh: imesh, vertex: a));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:965`

```csharp
private static LogMapTrace ZeroTrace(TangentLogMapAlgorithm algorithm, int source, int degenerateVertices) => new(
    Algorithm: algorithm, SourceVertex: source, MaxMagnitudeResidual: Option<double>.None, HeatTime: Option<double>.None,
    PathFaces: [], CrossedEdges: [], TracedLength: 0.0, PathRelativeResidual: 0.0, SegmentCount: 0, EdgeCrossingCount: 0, VertexPassCount: 0,
    DegenerateVertexCount: degenerateVertices, StopKind: Some(GeodesicStopKind.AtSource));
```

## To

```csharp
// TransportedLog DELETED
// IntrinsicFaceIndex DELETED
// IntrinsicFaceOfVertices DELETED
// ZeroTrace DELETED
```

```csharp
let unit = transported
from vector in unit.IsValid && unit.Unitize()
    ? key.AcceptValue(distance * unit)
    : Fin.Fail<Vector3d>(key.InvalidResult())

from faceIndex in space.Cache.Memoized(probe: unit, compute: () => Fin.Succ(toHashMap(
    imesh.LiveFaceIndices()
        .Select(f => (Key: SortedTriple(imesh.Triangles[f]!.Value), Face: f))
        .DistinctBy(static row => row.Key))))
let intrinsicFace = faceIndex.Find(SortedTriple((face.A, face.B, face.C)))
    .IfNone(() => FirstLiveFaceAt(imesh, face.A))

let trace = new LogMapTrace(
    Algorithm: TangentLogMapAlgorithm.ExactStraightestExp, SourceVertex: source,
    MaxMagnitudeResidual: None, HeatTime: None, PathFaces: [], CrossedEdges: [],
    TracedLength: 0.0, PathRelativeResidual: 0.0, SegmentCount: 0, EdgeCrossingCount: 0, VertexPassCount: 0,
    DegenerateVertexCount: frames.DegenerateVertexCount, StopKind: Some(GeodesicStopKind.AtSource))
```

## Why

Each helper has one call and no independently reusable rule. They hide a two-branch normalization, a cache expression, one map lookup, or one record literal while increasing the kernel's method surface.

## Change

Inline the normalization into vector-heat log projection, inline the memoized face index and lookup into exact-window projection, and construct the near-source trace at the branch that owns it.

## Delta

Code-fence LOC: `-8`. Source-declared module surface: `-4` methods; `0` types or members added.

# 8. Store only independent propagation state

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:223`

```csharp
internal readonly record struct WindowField(
    int SourceVertex, Seq<GeodesicWindow> Windows, Arr<int> EdgeOffsets,
    int OcclusionClampCount, int PseudosourceCount, int CutLocusCount, int DroppedWindowCount, int PopCount, int PopBudget) {
    internal static WindowField Empty(int source) => new(
        SourceVertex: source, Windows: [], EdgeOffsets: [0],
        OcclusionClampCount: 0, PseudosourceCount: 0, CutLocusCount: 0, DroppedWindowCount: 0, PopCount: 0, PopBudget: 0);
    internal int EdgeCount => Math.Max(val1: 0, val2: EdgeOffsets.Count - 1);
    internal int PopBudgetRemaining => Math.Max(val1: 0, val2: PopBudget - PopCount);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:239`

```csharp
internal readonly record struct WindowPropagation(WindowField Field, double[] VertexDistance);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:294`

```csharp
return Fin.Succ(new WindowPropagation(
    Field: new WindowField(
        SourceVertex: source, Windows: toSeq(Enumerable.Range(start: 0, count: perEdge.Length).SelectMany(e => perEdge[e])), EdgeOffsets: new Arr<int>(offsets),
        OcclusionClampCount: census.Clamps, PseudosourceCount: census.Pseudosources, CutLocusCount: cutLocus,
        DroppedWindowCount: census.Drops, PopCount: pops, PopBudget: popBudget),
    VertexDistance: vertexDistance));
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:379`

```csharp
private static int CountCutLocus(IntrinsicMesh imesh, List<GeodesicWindow>[] perEdge) =>
    Enumerable.Range(start: 0, count: perEdge.Length).Count(e => perEdge[e].Count >= 2 && Disagrees(
        windows: perEdge[e], band: EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: imesh.EdgeAt(index: e).Length)));
private static bool Disagrees(List<GeodesicWindow> windows, double band) =>
    windows.GroupBy(static window => window.Pseudosource)
        .Select(static group => group.Min(static window => Math.Min(val1: window.Sigma + window.D0, val2: window.Sigma + window.D1)))
        .ToArray() switch {
            { Length: >= 2 } reaches => reaches.Max() - reaches.Min() > band,
            _ => false,
        };
```

## To

```csharp
// WindowPropagation DELETED
// Disagrees DELETED

internal readonly record struct WindowField(
    Seq<GeodesicWindow> Windows, Arr<int> EdgeOffsets,
    int OcclusionClampCount, int PseudosourceCount, int CutLocusCount, int DroppedWindowCount, int PopBudgetRemaining) {
    // WindowField.Empty DELETED
    internal int EdgeCount => Math.Max(0, EdgeOffsets.Count - 1);
```

```csharp
internal static Fin<(WindowField Field, double[] VertexDistance)> PropagateWindows(
    IntrinsicMesh imesh, int source, WindowPropagationPolicy policy, double[] coneAngle, Op key) {

return Fin.Succ((
    Field: new WindowField(
        Windows: toSeq(Enumerable.Range(0, perEdge.Length).SelectMany(e => perEdge[e])), EdgeOffsets: new Arr<int>(offsets),
        OcclusionClampCount: census.Clamps, PseudosourceCount: census.Pseudosources, CutLocusCount: cutLocus,
        DroppedWindowCount: census.Drops, PopBudgetRemaining: Math.Max(0, popBudget - pops)),
    VertexDistance: vertexDistance));
```

```csharp
private static int CountCutLocus(IntrinsicMesh imesh, List<GeodesicWindow>[] perEdge) =>
    Enumerable.Range(start: 0, count: perEdge.Length).Count(e => {
        if (perEdge[e].Count < 2) return false;
        double[] reaches = perEdge[e]
            .GroupBy(static window => window.Pseudosource)
            .Select(static group => group.Min(static window => Math.Min(window.Sigma + window.D0, window.Sigma + window.D1)))
            .ToArray();
        double band = EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, imesh.EdgeAt(index: e).Length);
        return reaches is { Length: >= 2 } && reaches.Max() - reaches.Min() > band;
    });
```

## Why

`WindowPropagation` is a one-boundary two-value transfer with no behavior. `SourceVertex` is never read, `Empty` is never called, and no consumer needs both completed pop count and budget. `Disagrees` is a generic-named one-call predicate whose only semantics are the cut-locus fold that calls it.

## Change

Return propagation as a named tuple, delete unused source and factory state, compute the remaining budget once at construction, and inline the disagreement reduction into `CountCutLocus`. Keep `WindowField` because its CSR offsets and `At` reader form the shared forward/backtrace state owner.

## Delta

Code-fence LOC: `-6`. Source-declared module surface: `-1` type, `-1` method, and `-6` members (`WindowPropagation.Field`, `WindowPropagation.VertexDistance`, `WindowField.SourceVertex`, `PopCount`, `PopBudget`, and `Empty`); one positional `PopBudgetRemaining` replaces the computed member, for `-5` members net.

# 9. Canonicalize log-map and terminal vocabularies

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:739`

```csharp
[SmartEnum<int>]
public sealed partial class TangentLogMapAlgorithm {
    public static readonly TangentLogMapAlgorithm VectorHeatApproximate = new(key: 0);
    public static readonly TangentLogMapAlgorithm ExactStraightestExp = new(key: 1);
    public static readonly TangentLogMapAlgorithm ExactWindowPropagation = new(key: 2);
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:746`

```csharp
[SmartEnum<int>]
public sealed partial class GeodesicStopKind {
    public static readonly GeodesicStopKind LengthReached = new(key: 0);
    public static readonly GeodesicStopKind BoundaryHit = new(key: 1);
    public static readonly GeodesicStopKind IterationCap = new(key: 2);
    public static readonly GeodesicStopKind BarrierHit = new(key: 3);
    public static readonly GeodesicStopKind StopVertex = new(key: 4);
    public static readonly GeodesicStopKind DegenerateChart = new(key: 5);
    public static readonly GeodesicStopKind AtSource = new(key: 6);
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:758`

```csharp
public readonly record struct LogMapTrace(
    TangentLogMapAlgorithm Algorithm, int SourceVertex,
    Option<double> MaxMagnitudeResidual, Option<double> HeatTime, Arr<int> PathFaces, Arr<int> CrossedEdges,
    double TracedLength, double PathRelativeResidual, int SegmentCount, int EdgeCrossingCount, int VertexPassCount,
    int DegenerateVertexCount = 0, Option<GeodesicStopKind> StopKind = default, int WindowCount = 0, int OcclusionClampCount = 0,
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:771`

```csharp
ValidityClaim.CountExactly(count: PathFaces.Count, expected: SegmentCount),
ValidityClaim.CountExactly(count: CrossedEdges.Count, expected: EdgeCrossingCount),
PathFaces.ForAll(static face => face >= 0) && CrossedEdges.ForAll(static edge => edge >= 0),
!StopKind.IsSome || SegmentCount == 0 || SegmentCount == EdgeCrossingCount + VertexPassCount + 1);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:777`

```csharp
[StructLayout(LayoutKind.Auto)] public readonly record struct TangentLogMapResult(Vector3d Tangent, LogMapTrace Trace);
```

## To

```csharp
[SmartEnum<int>]
public sealed partial class LogMapAlgorithm {
    public static readonly LogMapAlgorithm VectorHeat = new(key: 0);
    public static readonly LogMapAlgorithm Straightest = new(key: 1);
    public static readonly LogMapAlgorithm WindowPropagation = new(key: 2);
}

public enum GeodesicStop { LengthReached, BoundaryHit, IterationCap, BarrierHit, TargetReached, DegenerateChart, AtSource }
```

```csharp
public readonly record struct LogMapTrace(
    LogMapAlgorithm Algorithm, int SourceVertex,
    Option<double> MagnitudeResidual, Option<double> HeatTime, Arr<int> Faces, Arr<int> Edges,
    double Length, double RelativeResidual, int VertexPassCount,
    int DegenerateVertexCount = 0, Option<GeodesicStop> Stop = default,
    int WindowCount = 0, int OcclusionClampCount = 0,
```

```csharp
Faces.ForAll(static face => face >= 0) && Edges.ForAll(static edge => edge >= 0),
!Stop.IsSome || Faces.IsEmpty || Faces.Count == Edges.Count + VertexPassCount + 1);
```

```csharp
[StructLayout(LayoutKind.Auto)] public readonly record struct LogMapResult(Vector3d Vector, LogMapTrace Trace);
```

## Why

The algorithm is a fixed behavioral vocabulary and its generated exhaustive `Switch` has value. Stop values are only stored and compared; a Thinktecture key, lookup, conversion, and dispatch surface is unused. `SegmentCount` and `EdgeCrossingCount` duplicate the two arrays, while `Tangent`, `Exact`, `Kind`, `StopVertex`, `MaxMagnitudeResidual`, `PathFaces`, and `PathRelativeResidual` add imprecise or redundant wording.

## Change

Keep the generated algorithm owner but rename it and its rows to domain terms. Replace the passive terminal SmartEnum with a plain enum, rename `StopVertex` to `TargetReached`, derive both counts from `Faces` and `Edges`, and rename the trace fields, result, and entrypoints to `LogMapResult`, `LogMapAt`, `StraightestLogMapAt`, and `WindowLogMapAt` without aliases.

## Delta

Code-fence LOC: `-10`. Source-declared module surface: `-2` stored trace members, `0` types net; the seven terminal values remain one closed vocabulary. Generated surface: the terminal SmartEnum's `Items`, `Get`, `TryGet`, `ToValue`, `Switch`, `Map`, parse, format, comparison, and conversion families disappear; the algorithm SmartEnum and its exhaustive `Switch` remain.

## Ripples

Rename algorithm/result/entrypoint references in `libs/dotnet/Rasm/.planning/Spatial/fields.md`, `libs/dotnet/Rasm/.planning/Parametric/patternmap.md`, `libs/dotnet/Rasm/.planning/Processing/extract.md`, and `libs/dotnet/Rasm.Materials/.planning/Component/masonry.md`. Rename stop references and consume `WalkTrace.Edges.Count` in `libs/dotnet/Rasm/.planning/Meshing/mesh.md`; update the owning cards and Mermaid labels in those pages with no compatibility names.

# 10. Collapse walk and backtrace evidence onto measured state

## From

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:395`

```csharp
internal readonly record struct ExpTrace(Vector3d SeatedWorldDir, double TracedLength, Arr<int> PathFaces, Arr<int> CrossedEdges, int EdgeCrossingCount, int VertexPassCount, GeodesicStopKind Stop, Arr<(int CutEdge, double U)> Crossings, double EndX, double EndY, int ArrivalFace);
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:414`

```csharp
int face = startFace; double traversed = 0.0; GeodesicStopKind stop = GeodesicStopKind.IterationCap;
int edgeCrossings = 0; int vertexPasses = 0;
double endX = qx; double endY = qy; int arrivalFace = startFace;
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:446`

```csharp
int edgeIndex = imesh.IndexOfEdge(lo: ea, hi: eb);
if (edgeIndex >= 0 && policy.Barrier.Exists(barrier => barrier.Contains(edgeIndex))) {
    traversed += tHit; endX = qx + (tHit * dx); endY = qy + (tHit * dy); arrivalFace = face; stop = GeodesicStopKind.BarrierHit; break;
}
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:522`

```csharp
internal readonly record struct BvpTrace(Option<Vector3d> WorldLogDir, double TracedLength, double FieldDistance, Arr<int> PathFaces, Arr<int> CrossedEdges, int EdgeCrossingCount, int VertexPassCount, GeodesicStopKind Stop);
internal static Option<BvpTrace> BacktraceGeodesicToSource(IntrinsicMesh imesh, Mesh mesh, FrameBundle frames, WindowField field, double targetDistance, int source, int targetFace, double[] targetWeights, double[] coneAngles, WindowPropagationPolicy policy) {
```

`libs/dotnet/Rasm/.planning/Processing/geodesics.md:541`

```csharp
let forward = WalkChart(imesh: imesh, startFace: seat.StartFace, va: seat.Va, vb: seat.Vb, vc: seat.Vc, seatAngle: seat.ChartAngle, seatedWorldDir: seat.WorldDir, traceLength: target.ChartDistance, coneAngles: coneAngles, mode: GeodesicWalkMode.Straightest, stopAtVertex: -1, policy: GeodesicTracePolicy.Default)
select new BvpTrace(WorldLogDir: Some(target.ChartDistance * forward.SeatedWorldDir), TracedLength: target.ChartDistance, FieldDistance: fieldDistance,
    PathFaces: forward.PathFaces, CrossedEdges: forward.CrossedEdges, EdgeCrossingCount: forward.EdgeCrossingCount, VertexPassCount: forward.VertexPassCount, Stop: forward.Stop);
```

## To

```csharp
// BvpTrace DELETED

internal readonly record struct WalkTrace(
    Vector3d InitialDirection, double Length, Arr<int> Faces, Arr<int> Edges,
    int VertexPassCount, GeodesicStop Stop, Arr<(int CutEdge, double U)> Crossings);
```

```csharp
int face = startFace; double traversed = 0.0; GeodesicStop stop = GeodesicStop.IterationCap;
int vertexPasses = 0;
```

```csharp
int edgeIndex = imesh.IndexOfEdge(lo: ea, hi: eb);
if (edgeIndex >= 0 && policy.Barrier.Exists(barrier => barrier.Contains(edgeIndex))) {
    stop = GeodesicStop.BarrierHit;
    break;
}
```

```csharp
internal static Option<(Option<Vector3d> Vector, double FieldDistance, Option<WalkTrace> Walk)> BacktraceGeodesicToSource(
    IntrinsicMesh imesh, Mesh mesh, FrameBundle frames, WindowField field,
    int source, int targetFace, double[] targetWeights, double[] coneAngles, WindowPropagationPolicy policy) {

let forward = WalkChart(/* admitted chart seat */, traceLength: target.ChartDistance, /* existing policy */)
let vector = forward.Stop == GeodesicStop.LengthReached
    ? Some(fieldDistance * forward.InitialDirection)
    : None
select (Vector: vector, FieldDistance: fieldDistance, Walk: Some(forward));
```

## Why

`EndX`, `EndY`, and `ArrivalFace` are never read; edge count duplicates `Edges.Count`. `BvpTrace` then copies the walk's path, counts, length, and stop into a second record, and `targetDistance` is unused. Renaming that transfer to another record would preserve a type with no invariant. The barrier branch also adds `tHit` after the loop has already added it, while the direct leg publishes the requested chart length and a direction even when the walk stopped early.

## Change

Retain only independent walk evidence, derive edge count from the array, and return backtrace as a named tuple carrying an optional walk instead of copying it into a second type. Remove the unused parameter and dead coordinates, remove the second barrier increment, use measured `WalkTrace.Length`, and publish a log vector only after `LengthReached`; project arrays and stop from `Walk` when building `LogMapTrace`, with absence representing the unconfirmed iteration-cap case.

## Delta

Code-fence LOC: `-15`. Source-declared module surface: `-1` type, `-12` stored record members, and `-1` parameter; `0` replacement module-level types or members. Three dead locals and one duplicate distance update are removed.

## Ripples

In `libs/dotnet/Rasm/.planning/Meshing/mesh.md`, rename `ExpTrace` to `WalkTrace`, `SeatedWorldDir` to `InitialDirection`, `TracedLength` to `Length`, `PathFaces` to `Faces`, and `CrossedEdges` to `Edges`; derive crossing count from `Edges.Count` and do not add compatibility projections.
