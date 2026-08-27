# 1. Remove the unused parameterization capability layer

From

Lines 57-78
```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParamTrait : ICapability<ParamTrait> {
    public static readonly ParamTrait Conformal      = new("conformal", rank: 0);
    public static readonly ParamTrait AreaPreserving = new("area-preserving", rank: 1);
    public static readonly ParamTrait FreeBoundary   = new("free-boundary", rank: 2);
    public static readonly ParamTrait Iterative      = new("iterative", rank: 3);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ParamKind {
    public static readonly ParamKind Harmonic = new("harmonic", traits: CapabilitySet<ParamTrait>.Of(ParamTrait.Conformal));
    public static readonly ParamKind Lscm     = new("lscm", traits: CapabilitySet<ParamTrait>.Of(ParamTrait.Conformal, ParamTrait.FreeBoundary));
    public static readonly ParamKind Arap     = new("arap", traits: CapabilitySet<ParamTrait>.Of(ParamTrait.AreaPreserving, ParamTrait.FreeBoundary, ParamTrait.Iterative));
    public static readonly ParamKind Bff      = new("bff", traits: CapabilitySet<ParamTrait>.Of(ParamTrait.Conformal, ParamTrait.FreeBoundary));

    public CapabilitySet<ParamTrait> Traits { get; }
}
```

Lines 200-200
```csharp
public sealed record ChartAtlas(MeshSpace Source, CapabilitySet<ParamTrait> Traits, Seq<UvIsland> Islands, Seq<FeatureEdge> Cuts, Distortion Distortion) {
```

Lines 257-273
```csharp
    public ParamKind Kind =>
        Switch(
            harmonic: static _ => ParamKind.Harmonic,
            lscm:     static _ => ParamKind.Lscm,
            arap:     static _ => ParamKind.Arap,
            bff:      static _ => ParamKind.Bff);

    public MeshSpace Chart =>
        Switch(
            harmonic: static h => h.Chart, lscm: static l => l.Chart,
            arap:     static a => a.Chart, bff:  static b => b.Chart);

    public ParamPolicy Policy =>
        Switch(
            harmonic: static h => h.Policy, lscm: static l => l.Policy,
            arap:     static a => a.Policy, bff:  static b => b.Policy);
```

Lines 282-291
```csharp
    public static Fin<ChartAtlas> Apply(ParamOp op, Op? key = null) {
        Op token = key.OrDefault();
        return MeshDec.Of(op.Chart, op.Policy, token).Bind(dec =>
            op.Switch(
                state: (Dec: dec, Key: token),
                harmonic: static (s, h) => FlattenHarmonic(h, s.Dec, s.Key),
                lscm:     static (s, l) => FlattenLscm(s.Dec, l.Policy, s.Key),
                arap:     static (s, a) => FlattenArap(s.Dec, a.Policy, s.Key),
                bff:      static (s, b) => FlattenBff(b, s.Dec, s.Key))
            .Bind(solved => Assemble(solved, op, dec, token)));
    }
```

To

Lines 57-78
```csharp
// ParamTrait and ParamKind DELETED
```

Lines 200-200
```csharp
public sealed record ChartAtlas(MeshSpace Source, Seq<UvIsland> Islands, Seq<FeatureEdge> Cuts, Distortion Distortion) {
```

Lines 257-273
```csharp
// ParamOp.Kind, ParamOp.Chart, and ParamOp.Policy DELETED
```

Lines 282-291
```csharp
    public static Fin<ChartAtlas> Apply(ParamOp op, Op? key = null) {
        Op token = key.OrDefault();
        (MeshSpace chart, ParamPolicy policy) = op.Map(
            harmonic: static value => (value.Chart, value.Policy),
            lscm: static value => (value.Chart, value.Policy),
            arap: static value => (value.Chart, value.Policy),
            bff: static value => (value.Chart, value.Policy));
        return MeshDec.Of(chart, policy, token).Bind(dec =>
            op.Switch(
                state: (Dec: dec, Key: token),
                harmonic: static (s, value) => FlattenHarmonic(value, s.Dec, s.Key),
                lscm: static (s, value) => FlattenLscm(s.Dec, value.Policy, s.Key),
                arap: static (s, value) => FlattenArap(s.Dec, value.Policy, s.Key),
                bff: static (s, value) => FlattenBff(value, s.Dec, s.Key))
            .Bind(solved => Assemble(solved, chart, policy, dec, token)));
    }
```

Why

No consumer reads `ChartAtlas.Traits`, `ParamTrait`, or `ParamKind`. The rows also state guarantees the returned evidence does not prove: a harmonic solve is not generally conformal, and ARAP is not generally area preserving. The three `ParamOp` properties each repeat the same exhaustive union fold only to support one call site.

Change

Delete both smart enums and the atlas trait column. Project `(chart, policy)` once with the generated exhaustive `Map`, retain the generated `Switch` for execution, and pass the admitted values directly into assembly. Remove the trait claims from the owner/law/packages/growth prose and the `ParamKind → ChartAtlas` diagram edge.

Delta

Code-fence LOC: -39 removed, +6 added, net -33. Module surface: -2 types, -14 members, +0 types, +0 members, net -16 symbols.

# 2. Remove the impossible successful-atlas flag

From

Lines 187-198
```csharp
public sealed record Distortion(
    double MaxConformal,
    double MeanConformal,
    double MaxArea,
    double MinArea,
    double MeanArea,
    double MaxQuasiConformal,
    int Iterations,
    Option<double> Residual,
    Option<int> FactorNonZeros,
    Option<double> SpectralGap,
    bool FlipFreeBijective);
```

Lines 393-398
```csharp
        Seq<UvIsland> islands = Islands(store, dec);
        int flipped = store.Flip.Span.IndexOf(true);
        Distortion distortion = Fold(store, dec, solved, flipped);
        return flipped < 0
            ? Fin.Succ(new ChartAtlas(op.Chart, op.Kind.Traits, islands, dec.Cuts, distortion))
            : Fin.Fail<ChartAtlas>(new GeometryFault.FlippedChart(ChartId.Create(store.Chart[flipped]), distortion.MaxConformal));
```

Lines 424-432
```csharp
    static Distortion Fold(ChartStore store, MeshDec dec, Solved solved, int flipped) {
        int n = dec.FaceCount;
        ReadOnlySpan<double> c = store.Conformal.Span, a = store.Area.Span, q = store.QuasiConformal.Span;
        return new Distortion(
            MaxConformal: TensorPrimitives.Max(c), MeanConformal: TensorPrimitives.Sum(c) / n,
            MaxArea: TensorPrimitives.Max(a), MinArea: TensorPrimitives.Min(a), MeanArea: TensorPrimitives.Sum(a) / n,
            MaxQuasiConformal: TensorPrimitives.MaxMagnitude(q),
            Iterations: solved.Iterations, Residual: solved.Residual, FactorNonZeros: solved.FactorNonZeros,
            SpectralGap: solved.SpectralGap, FlipFreeBijective: flipped < 0);
    }
```

To

Lines 187-198
```csharp
public sealed record Distortion(
    double MaxConformal,
    double MeanConformal,
    double MaxArea,
    double MinArea,
    double MeanArea,
    double MaxQuasiConformal,
    int Iterations,
    Option<double> Residual,
    Option<int> FactorNonZeros,
    Option<double> SpectralGap);
```

Lines 393-398
```csharp
        Seq<UvIsland> islands = Islands(store, dec);
        int flipped = store.Flip.Span.IndexOf(true);
        Distortion distortion = Fold(store, dec, solved);
        return flipped < 0
            ? Fin.Succ(new ChartAtlas(op.Chart, islands, dec.Cuts, distortion))
            : Fin.Fail<ChartAtlas>(new GeometryFault.FlippedChart(ChartId.Create(store.Chart[flipped]), distortion.MaxConformal));
```

Lines 424-432
```csharp
    static Distortion Fold(ChartStore store, MeshDec dec, Solved solved) {
        int n = dec.FaceCount;
        ReadOnlySpan<double> c = store.Conformal.Span, a = store.Area.Span, q = store.QuasiConformal.Span;
        return new Distortion(
            MaxConformal: TensorPrimitives.Max(c), MeanConformal: TensorPrimitives.Sum(c) / n,
            MaxArea: TensorPrimitives.Max(a), MinArea: TensorPrimitives.Min(a), MeanArea: TensorPrimitives.Sum(a) / n,
            MaxQuasiConformal: TensorPrimitives.MaxMagnitude(q),
            Iterations: solved.Iterations, Residual: solved.Residual, FactorNonZeros: solved.FactorNonZeros,
            SpectralGap: solved.SpectralGap);
    }
```

Why

`Assemble` returns `Fin.Fail<ChartAtlas>` for every flipped chart, so every obtainable `ChartAtlas` necessarily carries `FlipFreeBijective == true`. The boolean is an impossible-state column and downstream checks can never observe its false arm.

Change

Keep the exact `Orient2D` gate as the admission decision, but remove its redundant copy from `Distortion` and stop threading `flipped` into `Fold`. Rewrite the flip law and output prose so bijectivity is stated as atlas admission, not a stored boolean.

Delta

Code-fence LOC: -1 target, -1 ripple, net -2. Module surface: -1 generated record member, +0, net -1 symbol.

Ripples

- `libs/dotnet/Rasm.Fabrication/.planning/Nesting/nfp.md:708-710`: remove `!atlas.Result.FlipFreeBijective` and correct the two remaining reads to `atlas.Distortion.MaxArea` and `atlas.Distortion.MinArea`.
- `libs/dotnet/Rasm.Fabrication/.planning/Forming/tube.md:1029-1034`: correct `Some(unrolled.Atlas.Result)` to `Some(unrolled.Atlas.Distortion)`; `ChartAtlas` has no `Result` member.
- `libs/dotnet/Rasm.Fabrication/.planning/Forming/tube.md:1299-1306`: remove `.Bool(row.FlipFreeBijective)` from the canonical distortion encoding; the field was constant for every encodable atlas.

# 3. Replace the public scratch wrapper with method-owned buffers

From

Lines 116-146
```csharp
public sealed class ChartStore : IDisposable {
    readonly MemoryOwner<double> u, v;
    readonly MemoryOwner<int> chart;
    readonly MemoryOwner<double> conformal, area, quasiConformal;
    readonly MemoryOwner<bool> flip, degenerate;

    ChartStore(int vertices, int faces) {
        u = MemoryOwner<double>.Allocate(vertices, AllocationMode.Clear);
        v = MemoryOwner<double>.Allocate(vertices, AllocationMode.Clear);
        chart = MemoryOwner<int>.Allocate(faces, AllocationMode.Clear);
        conformal = MemoryOwner<double>.Allocate(faces, AllocationMode.Clear);
        area = MemoryOwner<double>.Allocate(faces, AllocationMode.Clear);
        quasiConformal = MemoryOwner<double>.Allocate(faces, AllocationMode.Clear);
        flip = MemoryOwner<bool>.Allocate(faces, AllocationMode.Clear);
        degenerate = MemoryOwner<bool>.Allocate(faces, AllocationMode.Clear);
    }

    public static ChartStore Allocate(int vertices, int faces) => new(vertices, faces);

    public Memory<double> U => u.Memory;
    public Memory<double> V => v.Memory;
    public Span<int> Chart => chart.Span;
    public Memory<double> Conformal => conformal.Memory;
    public Memory<double> Area => area.Memory;
    public Memory<double> QuasiConformal => quasiConformal.Memory;
    public Memory<bool> Flip => flip.Memory;
    public Memory<bool> Degenerate => degenerate.Memory;
    public Point2d At(int vertex) => new(u.Span[vertex], v.Span[vertex]);

    public void Dispose() { u.Dispose(); v.Dispose(); chart.Dispose(); conformal.Dispose(); area.Dispose(); quasiConformal.Dispose(); flip.Dispose(); degenerate.Dispose(); }
}
```

Lines 382-399
```csharp
    static Fin<ChartAtlas> Assemble(Solved solved, ParamOp op, MeshDec dec, Op key) {
        using ChartStore store = ChartStore.Allocate(dec.VertexCount, dec.FaceCount);
        solved.U.CopyTo(store.U.Span);
        solved.V.CopyTo(store.V.Span);
        ParallelHelper.For(0, dec.FaceCount,
            new DistortionPass(dec, store.U, store.V, store.Conformal, store.Area, store.QuasiConformal, store.Flip, store.Degenerate, dec.AreaFloor, dec.CollapseFloor),
            op.Policy.ParallelFloor.Value);
        int degenerate = store.Degenerate.Span.IndexOf(true);
        if (degenerate >= 0) {
            return Fin.Fail<ChartAtlas>(new GeometryFault.DegenerateInput(Kind.Mesh, degenerate, "parameterization: degenerate reference triangle"));
        }
        Seq<UvIsland> islands = Islands(store, dec);
        int flipped = store.Flip.Span.IndexOf(true);
        Distortion distortion = Fold(store, dec, solved, flipped);
        return flipped < 0
            ? Fin.Succ(new ChartAtlas(op.Chart, op.Kind.Traits, islands, dec.Cuts, distortion))
            : Fin.Fail<ChartAtlas>(new GeometryFault.FlippedChart(ChartId.Create(store.Chart[flipped]), distortion.MaxConformal));
    }
```

Lines 401-401
```csharp
    readonly struct DistortionPass(MeshDec dec, ReadOnlyMemory<double> u, ReadOnlyMemory<double> v, Memory<double> conformal, Memory<double> area, Memory<double> quasi, Memory<bool> flip, Memory<bool> degenerate, double areaFloor, double collapseFloor) : IAction {
```

Lines 424-426
```csharp
    static Distortion Fold(ChartStore store, MeshDec dec, Solved solved, int flipped) {
        int n = dec.FaceCount;
        ReadOnlySpan<double> c = store.Conformal.Span, a = store.Area.Span, q = store.QuasiConformal.Span;
```

Lines 435-457
```csharp
    static Seq<UvIsland> Islands(ChartStore store, MeshDec dec) {
        AdjacencyGraph<int, SEdge<int>> dual = new(allowParallelEdges: false);
        dual.AddVertexRange(Enumerable.Range(0, dec.FaceCount));
        foreach (((int u, int v), int faceA, int faceB) in dec.InteriorEdges()) {
            if (!dec.IsCutEdge(u, v)) dual.AddEdge(new SEdge<int>(faceA, faceB));
        }
        Dictionary<int, int> label = new(dec.FaceCount);
        int count = dual.WeaklyConnectedComponents(label);
        List<int>[] vertices = new List<int>[count];
        List<(int A, int B, int C)>[] faces = new List<(int A, int B, int C)>[count];
        IndexSet[] seen = new IndexSet[count];
        for (int chart = 0; chart < count; chart++) { vertices[chart] = []; faces[chart] = []; seen[chart] = []; }
        for (int f = 0; f < dec.FaceCount; f++) {
            int chart = label[f];
            store.Chart[f] = chart;
            (int a, int b, int c) = dec.Face(f);
            faces[chart].Add((a, b, c));
            if (seen[chart].Add(a)) vertices[chart].Add(a);
            if (seen[chart].Add(b)) vertices[chart].Add(b);
            if (seen[chart].Add(c)) vertices[chart].Add(c);
        }
        return toSeq(Enumerable.Range(0, count).Select(chart =>
            new UvIsland(ChartId.Create(chart), toArray(vertices[chart]), toArray(faces[chart]), toArray(vertices[chart].Select(store.At))))).Strict();
    }
```

To

Lines 116-146
```csharp
// ChartStore DELETED
```

Lines 382-399
```csharp
    static Fin<ChartAtlas> Assemble(Solved solved, MeshSpace chartSpace, ParamPolicy policy, MeshDec dec, Op key) {
        int vertices = dec.VertexCount, faces = dec.FaceCount;
        using MemoryOwner<double> planes = MemoryOwner<double>.Allocate((2 * vertices) + (3 * faces), AllocationMode.Clear);
        using MemoryOwner<bool> verdicts = MemoryOwner<bool>.Allocate(2 * faces, AllocationMode.Clear);
        Memory<double> u = planes.Memory.Slice(0, vertices), v = planes.Memory.Slice(vertices, vertices);
        Memory<double> conformal = planes.Memory.Slice(2 * vertices, faces);
        Memory<double> area = planes.Memory.Slice((2 * vertices) + faces, faces);
        Memory<double> quasi = planes.Memory.Slice((2 * vertices) + (2 * faces), faces);
        Memory<bool> flip = verdicts.Memory.Slice(0, faces), degenerate = verdicts.Memory.Slice(faces, faces);
        solved.U.CopyTo(u.Span);
        solved.V.CopyTo(v.Span);
        ParallelHelper.For(0, dec.FaceCount,
            new DistortionPass(dec, u, v, conformal, area, quasi, flip, degenerate, dec.CollapseFloor),
            policy.ParallelFloor.Value);
        int invalid = degenerate.Span.IndexOf(true);
        if (invalid >= 0) return Fin.Fail<ChartAtlas>(new GeometryFault.DegenerateInput(Kind.Mesh, invalid, "parameterization: degenerate reference triangle"));
        int flipped = flip.Span.IndexOf(true);
        (Seq<UvIsland> islands, ChartId flippedChart) = Islands(u, v, dec, Math.Max(flipped, 0));
        Distortion distortion = Fold(conformal.Span, area.Span, quasi.Span, dec.FaceCount, solved);
        return flipped < 0
            ? Fin.Succ(new ChartAtlas(chartSpace, islands, dec.Cuts, distortion))
            : Fin.Fail<ChartAtlas>(new GeometryFault.FlippedChart(flippedChart, distortion.MaxConformal));
    }
```

Lines 401-401
```csharp
    readonly struct DistortionPass(MeshDec dec, ReadOnlyMemory<double> u, ReadOnlyMemory<double> v, Memory<double> conformal, Memory<double> area, Memory<double> quasi, Memory<bool> flip, Memory<bool> degenerate, double collapseFloor) : IAction {
```

Lines 424-426
```csharp
    static Distortion Fold(ReadOnlySpan<double> c, ReadOnlySpan<double> a, ReadOnlySpan<double> q, int n, Solved solved) {
```

Lines 435-457
```csharp
    static (Seq<UvIsland> Islands, ChartId Probe) Islands(
        ReadOnlyMemory<double> u, ReadOnlyMemory<double> v, MeshDec dec, int probeFace) {
        AdjacencyGraph<int, SEdge<int>> dual = new(allowParallelEdges: false);
        dual.AddVertexRange(Enumerable.Range(0, dec.FaceCount));
        foreach (((int a, int b), int left, int right) in dec.InteriorEdges()) {
            if (!dec.IsCutEdge(a, b)) dual.AddEdge(new SEdge<int>(left, right));
        }
        Dictionary<int, int> components = new(dec.FaceCount);
        int count = dual.WeaklyConnectedComponents(components);
        List<int>[] vertices = new List<int>[count];
        List<(int A, int B, int C)>[] faces = new List<(int A, int B, int C)>[count];
        IndexSet[] seen = new IndexSet[count];
        for (int id = 0; id < count; id++) { vertices[id] = []; faces[id] = []; seen[id] = []; }
        for (int face = 0; face < dec.FaceCount; face++) {
            int id = components[face];
            (int a, int b, int c) = dec.Face(face);
            faces[id].Add((a, b, c));
            if (seen[id].Add(a)) vertices[id].Add(a);
            if (seen[id].Add(b)) vertices[id].Add(b);
            if (seen[id].Add(c)) vertices[id].Add(c);
        }
        Seq<UvIsland> islands = toSeq(Enumerable.Range(0, count).Select(id => new UvIsland(
            ChartId.Create(id), toArray(vertices[id]), toArray(faces[id]),
            toArray(vertices[id].Select(vertex => new Point2d(u.Span[vertex], v.Span[vertex])))))).Strict();
        return (islands, ChartId.Create(components[probeFace]));
    }
```

Why

`ChartStore` is a public, single-call allocation wrapper whose members only forward eight `MemoryOwner<T>` buffers. It adds a public type, a factory, eight projections, an index projection, and a hand-written disposal sweep without owning any invariant. Its chart plane also duplicates the face-to-component dictionary solely to recover one flipped face's label. `DistortionPass.areaFloor` is stored but never read because `MeshDec.Jacobian` already reads `AreaFloor`.

Change

Lease one partitioned numeric plane and one partitioned verdict plane directly in `Assemble`, pass their `Memory`/`Span` slices to the existing pass, fold, and island builder, and let each `using` own disposal. Return the probed component id from the QuikGraph label dictionary instead of mirroring every label into another pooled plane. Change `Fold` to take its three spans, construct UV points at the final island projection, and remove `ChartStore` from the scratch exemption prose.

Delta

Code-fence LOC: -31 wrapper lines, +11 across direct ownership and adjusted callers, net -20. Module surface: -1 type and -20 members/fields, +0, net -21 symbols. Runtime scratch: -8 independent owners and -1 mirrored `int` plane, +2 partitioned owners, net -6 owners and one face-count plane.

# 4. Inline the single-use island-chain adapter

From

Lines 166-184
```csharp
        UvIsland self = this;
        return Cycles.Of(successor, op)
            .Bind(loops => loops.TraverseM(loop => self.ChainOf(loop, local, weld)).As())
            .Bind(chains => op.AcceptValue(value: chains.Strict()));
    }

    Fin<Chain> ChainOf(Seq<int> loop, Dictionary<int, int> local, double weld) {
        Polyline points = new();
        foreach (int at in loop) {
            Point2d uv = Uv[local[at]];
            Point3d next = new(uv.X, uv.Y, 0.0);
            if (points.Count == 0 || points[^1].DistanceTo(next) > weld) points.Add(next);
        }
        if (points.Count < 3) {
            return Fin.Fail<Chain>(new GeometryFault.DegenerateInput(Kind.Mesh, loop[0], "island-boundary: loop collapsed under weld"));
        }
        points.Add(points[0]);
        return Fin.Succ(new Chain(points));
    }
```

To

Lines 166-184
```csharp
        return Cycles.Of(successor, op)
            .Bind(loops => loops.TraverseM(loop => {
                Polyline points = new();
                foreach (int at in loop) {
                    Point2d uv = Uv[local[at]];
                    Point3d point = new(uv.X, uv.Y, 0.0);
                    if (points.Count == 0 || points[^1].DistanceTo(point) > weld) points.Add(point);
                }
                if (points.Count < 3) {
                    return Fin.Fail<Chain>(new GeometryFault.DegenerateInput(Kind.Mesh, loop[0], "island-boundary: loop collapsed under weld"));
                }
                points.Add(points[0]);
                return Fin.Succ(new Chain(points));
            }).As())
            .Bind(chains => op.AcceptValue(value: chains.Strict()));
    }
```

Why

`ChainOf` has one call site and no independent invariant or reusable abstraction. It forces an extra captured `self` solely to forward the island state into a private method.

Change

Place the chain construction directly in the `TraverseM` arm so the boundary walk and its typed refusal remain one operation.

Delta

Code-fence LOC: 19 to 17, net -2. Module surface: -1 method, +0, net -1 symbol.

# 5. Replace transient numeric records with tuple aliases

From

Lines 47-50
```csharp
using EdgeKeySet = System.Collections.Generic.HashSet<(int, int)>;
using IndexSet = System.Collections.Generic.HashSet<int>;
using Dimension = Rasm.Numerics.Dimension;
```

Lines 328-349
```csharp
                Atom<Fin<ArapState>> cell = Atom(value: Fin.Succ(new ArapState(seed.U, seed.V, 0, Option<double>.None)));
                Transition<Fin<ArapState>> driven = Cell.Converge(
                    cell: cell,
                    step: state => Some(state.Bind(active => Settled(active.Residual) ? Fin.Succ(active) : Step(active))),
                    settled: state => state.Match(Succ: active => Settled(active.Residual), Fail: static _ => true),
                    budget: policy.MaxIterations,
                    declined: key.InvalidResult());
                return driven.Current.Bind(state => Settled(state.Residual)
                    ? Fin.Succ(new Solved(state.U, state.V, state.Iterations, state.Residual, Some(system.FactorNonZeros), Option<double>.None))
                    : Fin.Fail<Solved>(new GeometryFault.ParameterizationUnconverged(state.Residual, state.Iterations)));

                bool Settled(Option<double> residual) => residual.Exists(value => value <= tolerance);

                Fin<ArapState> Step(ArapState state) =>
                    dec.LocalRotations(state.U, state.V, key).Bind(rotations => {
                        dec.RotatedGradient(rotations, axis: 0, sink: gradientU.Memory);
                        dec.RotatedGradient(rotations, axis: 1, sink: gradientV.Memory);
                        return from solvedU in system.SolveWith(gradientU.Memory, k => state.U[gauge[k]], key)
                               from solvedV in system.SolveWith(gradientV.Memory, k => state.V[gauge[k]], key)
                               let nextU = system.Scatter(gauge, k => state.U[gauge[k]], solvedU)
                               let nextV = system.Scatter(gauge, k => state.V[gauge[k]], solvedV)
                               select new ArapState(nextU, nextV, state.Iterations + 1, Some(MaxDelta(state.U, nextU, state.V, nextV, scratch.Memory)));
                    });
```

Lines 488-492
```csharp
file readonly record struct Solved(double[] U, double[] V, int Iterations, Option<double> Residual, Option<int> FactorNonZeros, Option<double> SpectralGap);

file readonly record struct ArapState(double[] U, double[] V, int Iterations, Option<double> Residual);

file readonly record struct Matrix2(double M00, double M01, double M10, double M11);
```

To

Lines 47-50
```csharp
using EdgeKeySet = System.Collections.Generic.HashSet<(int, int)>;
using IndexSet = System.Collections.Generic.HashSet<int>;
using Dimension = Rasm.Numerics.Dimension;
using Matrix2 = (double M00, double M01, double M10, double M11);
using Solved = (double[] U, double[] V, int Iterations, LanguageExt.Option<double> Residual, LanguageExt.Option<int> FactorNonZeros, LanguageExt.Option<double> SpectralGap);
```

Lines 328-349
```csharp
                Atom<Fin<Solved>> cell = Atom(value: Fin.Succ(seed));
                Transition<Fin<Solved>> driven = Cell.Converge(
                    cell: cell,
                    step: state => Some(state.Bind(active => Settled(active.Residual) ? Fin.Succ(active) : Step(active))),
                    settled: state => state.Match(Succ: active => Settled(active.Residual), Fail: static _ => true),
                    budget: policy.MaxIterations,
                    declined: key.InvalidResult());
                return driven.Current.Bind(state => Settled(state.Residual)
                    ? Fin.Succ(state)
                    : Fin.Fail<Solved>(new GeometryFault.ParameterizationUnconverged(state.Residual, state.Iterations)));

                bool Settled(Option<double> residual) => residual.Exists(value => value <= tolerance);

                Fin<Solved> Step(Solved state) =>
                    dec.LocalRotations(state.U, state.V, key).Bind(rotations => {
                        dec.RotatedGradient(rotations, axis: 0, sink: gradientU.Memory);
                        dec.RotatedGradient(rotations, axis: 1, sink: gradientV.Memory);
                        return from solvedU in system.SolveWith(gradientU.Memory, k => state.U[gauge[k]], key)
                               from solvedV in system.SolveWith(gradientV.Memory, k => state.V[gauge[k]], key)
                               let nextU = system.Scatter(gauge, k => state.U[gauge[k]], solvedU)
                               let nextV = system.Scatter(gauge, k => state.V[gauge[k]], solvedV)
                               select new Solved(nextU, nextV, state.Iterations + 1, Some(MaxDelta(state.U, nextU, state.V, nextV, scratch.Memory)), Some(system.FactorNonZeros), Option<double>.None);
                    });
```

Lines 488-492
```csharp
// Solved, ArapState, and Matrix2 DELETED
```

Why

`ArapState` duplicates the first four columns of `Solved`, while `Solved` and `Matrix2` are file-only dense signatures with no invariant, identity, or behavior. Their three record declarations mint three module types and fourteen generated properties for shapes that C# tuple aliases carry without introducing runtime or module identity. The initial LSCM seed already has the full solved signature, and the first ARAP step can populate its factor evidence.

Change

Alias the two dense signatures, drive `Cell.Converge` over `Fin<Solved>`, return its terminal success directly, and have each step preserve the full solved shape. Existing named element access and `new Solved(...)`/`new Matrix2(...)` constructions continue to bind through the aliases.

Delta

Code-fence LOC: -3 declarations, +2 aliases, net -1. Module surface: -3 types and -14 generated properties, +0 types and +0 members, net -17 symbols.

# 6. Collapse both boundary-constrained modes onto one solve

From

Lines 252-255
```csharp
    public sealed record Harmonic(MeshSpace Chart, Option<Polyline> Boundary, ParamPolicy Policy) : ParamOp;
    public sealed record Lscm(MeshSpace Chart, ParamPolicy Policy) : ParamOp;
    public sealed record Arap(MeshSpace Chart, ParamPolicy Policy) : ParamOp;
    public sealed record Bff(MeshSpace Chart, Option<Arr<double>> TargetCurvature, ParamPolicy Policy) : ParamOp;
```

Lines 286-290
```csharp
                harmonic: static (s, h) => FlattenHarmonic(h, s.Dec, s.Key),
                lscm:     static (s, l) => FlattenLscm(s.Dec, l.Policy, s.Key),
                arap:     static (s, a) => FlattenArap(s.Dec, a.Policy, s.Key),
                bff:      static (s, b) => FlattenBff(b, s.Dec, s.Key))
```

Lines 295-307
```csharp
    static Fin<Solved> FlattenHarmonic(ParamOp.Harmonic op, MeshDec dec, Op key) =>
        dec.Disk().Bind(loop => Pins(op.Boundary, loop.Length).Bind(pinned =>
            dec.Reduced(loop, key).Bind(system =>
                from solvedU in system.Solve(k => pinned[k].X, key)
                from solvedV in system.Solve(k => pinned[k].Y, key)
                select Scattered(system, loop, pinned, solvedU, solvedV, iterations: 1))));

    static Fin<Point2d[]> Pins(Option<Polyline> boundary, int count) =>
        boundary.Match(
            Some: b => b.Count >= 2 && b.Length > 0.0
                ? Fin.Succ(Resample(b, count))
                : Fin.Fail<Point2d[]>(new GeometryFault.DegenerateInput(Kind.Curve, b.Count, "harmonic pin: degenerate boundary polyline")),
            None: () => Fin.Succ(UnitCircle(count)));
```

Lines 354-374
```csharp
    static Fin<Solved> FlattenBff(ParamOp.Bff op, MeshDec dec, Op key) =>
        dec.Disk().Bind(loop => {
            Arr<double> target = op.TargetCurvature.IfNone(() => new Arr<double>([.. Enumerable.Repeat(2.0 * Math.PI / loop.Length, loop.Length)]));
            return target.Count != loop.Length || !target.ForAll(static t => ValidityClaim.Finite(value: t))
                ? Fin.Fail<Solved>(new GeometryFault.DegenerateInput(Kind.Mesh, target.Count, "bff turning prescription: finite, one row per boundary vertex"))
                : dec.Reduced(loop, key).Bind(system => {
                    Point2d[] curve = dec.IntegrateBoundary(loop, target);
                    return from solvedU in system.Solve(k => curve[k].X, key)
                           from solvedV in system.Solve(k => curve[k].Y, key)
                           select Scattered(system, loop, curve, solvedU, solvedV, iterations: 1);
                });
        });

    static Solved Scattered(ReducedSystem system, int[] loop, Point2d[] pinned, Arr<double> solvedU, Arr<double> solvedV, int iterations) {
        double[] u = system.Scatter(loop, k => pinned[k].X, solvedU);
        double[] v = system.Scatter(loop, k => pinned[k].Y, solvedV);
        return new Solved(u, v, iterations, Residual: Option<double>.None, FactorNonZeros: Some(system.FactorNonZeros), SpectralGap: Option<double>.None);
    }
```

To

Lines 252-255
```csharp
    public sealed record Harmonic(MeshSpace Chart, Option<Polyline> Boundary, ParamPolicy Policy) : ParamOp;
    public sealed record Lscm(MeshSpace Chart, ParamPolicy Policy) : ParamOp;
    public sealed record Arap(MeshSpace Chart, ParamPolicy Policy) : ParamOp;
    public sealed record Turning(MeshSpace Chart, Option<Arr<double>> TurningAngles, ParamPolicy Policy) : ParamOp;
```

Lines 286-290
```csharp
                harmonic: static (s, value) => FlattenHarmonic(value, s.Dec, s.Key),
                lscm: static (s, value) => FlattenLscm(s.Dec, value.Policy, s.Key),
                arap: static (s, value) => FlattenArap(s.Dec, value.Policy, s.Key),
                turning: static (s, value) => FlattenTurning(value, s.Dec, s.Key))
```

Lines 295-307
```csharp
    static Fin<Solved> FlattenHarmonic(ParamOp.Harmonic op, MeshDec dec, Op key) =>
        dec.Disk().Bind(loop => op.Boundary.Match(
            Some: boundary => boundary.Count >= 2 && boundary.Length > 0.0
                ? Fin.Succ(Resample(boundary, loop.Length))
                : Fin.Fail<Point2d[]>(new GeometryFault.DegenerateInput(Kind.Curve, boundary.Count, "harmonic pin: degenerate boundary polyline")),
            None: () => Fin.Succ(UnitCircle(loop.Length)))
            .Bind(pinned => FlattenBoundary(dec, loop, pinned, key)));

// Pins DELETED
```

Lines 354-374
```csharp
    static Fin<Solved> FlattenTurning(ParamOp.Turning op, MeshDec dec, Op key) =>
        dec.Disk().Bind(loop => {
            Arr<double> turning = op.TurningAngles.IfNone(() =>
                new Arr<double>([.. Enumerable.Repeat(2.0 * Math.PI / loop.Length, loop.Length)]));
            return turning.Count != loop.Length || !turning.ForAll(static angle => ValidityClaim.Finite(value: angle))
                ? Fin.Fail<Solved>(new GeometryFault.DegenerateInput(Kind.Mesh, turning.Count, "boundary turning: finite angle per boundary vertex"))
                : FlattenBoundary(dec, loop, dec.IntegrateBoundary(loop, turning), key);
        });

    static Fin<Solved> FlattenBoundary(MeshDec dec, int[] loop, Point2d[] pinned, Op key) =>
        dec.Reduced(loop, key).Bind(system =>
            from solvedU in system.Solve(k => pinned[k].X, key)
            from solvedV in system.Solve(k => pinned[k].Y, key)
            select new Solved(
                system.Scatter(loop, k => pinned[k].X, solvedU),
                system.Scatter(loop, k => pinned[k].Y, solvedV),
                1, None, Some(system.FactorNonZeros), None));

// Scattered DELETED
```

Why

`Bff` claims Boundary First Flattening, but the body performs a harmonic Dirichlet solve over a polygon integrated from turning angles; it implements neither BFF's boundary scale-factor solve nor its Hilbert-transform reconstruction. The harmonic and turning-angle arms then duplicate the same reduced U/V back-solve through the one-call `Scattered` adapter, while `Pins` only forwards one `Option.Match`.

Change

Rename `ParamOp.Bff` to `ParamOp.Turning`, `TargetCurvature` to `TurningAngles`, and `FlattenBff` to `FlattenTurning`. Fold the optional harmonic boundary at its only use, send both boundary-producing arms through one `FlattenBoundary` solve that retains each `LinearSolution`, and delete `Pins` and `Scattered`. Update the union `Map`/`Switch`, owner/cases/growth/packages prose, and diagram label to the truthful turning-angle modality.

Delta

Code-fence LOC: 34 to 28, net -6. Module surface: -2 methods, +1 method, net -1 symbol; renamed union members are surface-neutral.

# 7. Preserve matrix evidence through one reduced solve

From

Lines 187-198
```csharp
public sealed record Distortion(
    double MaxConformal,
    double MeanConformal,
    double MaxArea,
    double MinArea,
    double MeanArea,
    double MaxQuasiConformal,
    int Iterations,
    Option<double> Residual,
    Option<int> FactorNonZeros,
    Option<double> SpectralGap,
    bool FlipFreeBijective);
```

Lines 311-318
```csharp
    static Fin<Solved> FlattenLscm(MeshDec dec, ParamPolicy policy, Op key) =>
        dec.Loops.Length == 0
            ? Fin.Fail<Solved>(new GeometryFault.InvalidChartBoundary(0, None))
            : SparseMatrix.FromTriplets(Dimension.Create(2 * dec.VertexCount), Dimension.Create(2 * dec.VertexCount), dec.ConformalTriplets(), key)
                .Bind(conformal => conformal.SmallestEigenpairsDetailed(k: GaugeModes + 1, tolerance: policy.ResidualTolerance.Value, budget: policy.EigenBudget, key: key))
                .Bind(eigen => eigen.PairsIn(expected: EigenOrder.Ascending, key: key).Bind(pairs => pairs.Count > GaugeModes
                    ? Fin.Succ(SplitComplex(dec, pairs[GaugeModes], eigen.Evidence.Iterations.IfNone(0)))
                    : Fin.Fail<Solved>(new GeometryFault.IncompleteParameterizationSpectrum(GaugeModes + 1, pairs.Count))));
```

Lines 328-349
```csharp
                Atom<Fin<ArapState>> cell = Atom(value: Fin.Succ(new ArapState(seed.U, seed.V, 0, Option<double>.None)));
                Transition<Fin<ArapState>> driven = Cell.Converge(
                    cell: cell,
                    step: state => Some(state.Bind(active => Settled(active.Residual) ? Fin.Succ(active) : Step(active))),
                    settled: state => state.Match(Succ: active => Settled(active.Residual), Fail: static _ => true),
                    budget: policy.MaxIterations,
                    declined: key.InvalidResult());
                return driven.Current.Bind(state => Settled(state.Residual)
                    ? Fin.Succ(new Solved(state.U, state.V, state.Iterations, state.Residual, Some(system.FactorNonZeros), Option<double>.None))
                    : Fin.Fail<Solved>(new GeometryFault.ParameterizationUnconverged(state.Residual, state.Iterations)));

                bool Settled(Option<double> residual) => residual.Exists(value => value <= tolerance);

                Fin<ArapState> Step(ArapState state) =>
                    dec.LocalRotations(state.U, state.V, key).Bind(rotations => {
                        dec.RotatedGradient(rotations, axis: 0, sink: gradientU.Memory);
                        dec.RotatedGradient(rotations, axis: 1, sink: gradientV.Memory);
                        return from solvedU in system.SolveWith(gradientU.Memory, k => state.U[gauge[k]], key)
                               from solvedV in system.SolveWith(gradientV.Memory, k => state.V[gauge[k]], key)
                               let nextU = system.Scatter(gauge, k => state.U[gauge[k]], solvedU)
                               let nextV = system.Scatter(gauge, k => state.V[gauge[k]], solvedV)
                               select new ArapState(nextU, nextV, state.Iterations + 1, Some(MaxDelta(state.U, nextU, state.V, nextV, scratch.Memory)));
                    });
```

Lines 367-379
```csharp
    static Solved Scattered(ReducedSystem system, int[] loop, Point2d[] pinned, Arr<double> solvedU, Arr<double> solvedV, int iterations) {
        double[] u = system.Scatter(loop, k => pinned[k].X, solvedU);
        double[] v = system.Scatter(loop, k => pinned[k].Y, solvedV);
        return new Solved(u, v, iterations, Residual: Option<double>.None, FactorNonZeros: Some(system.FactorNonZeros), SpectralGap: Option<double>.None);
    }

    static Solved SplitComplex(MeshDec dec, (double Eigenvalue, Arr<double> Eigenvector) pair, int iterations) {
        int n = dec.VertexCount;
        double[] u = new double[n];
        double[] v = new double[n];
        for (int i = 0; i < n; i++) { u[i] = pair.Eigenvector[i]; v[i] = pair.Eigenvector[n + i]; }
        return new Solved(u, v, iterations, Residual: Option<double>.None, FactorNonZeros: Option<int>.None, SpectralGap: Some(pair.Eigenvalue));
    }
```

Lines 427-432
```csharp
        return new Distortion(
            MaxConformal: TensorPrimitives.Max(c), MeanConformal: TensorPrimitives.Sum(c) / n,
            MaxArea: TensorPrimitives.Max(a), MinArea: TensorPrimitives.Min(a), MeanArea: TensorPrimitives.Sum(a) / n,
            MaxQuasiConformal: TensorPrimitives.MaxMagnitude(q),
            Iterations: solved.Iterations, Residual: solved.Residual, FactorNonZeros: solved.FactorNonZeros,
            SpectralGap: solved.SpectralGap, FlipFreeBijective: flipped < 0);
```

Lines 488-490
```csharp
file readonly record struct Solved(double[] U, double[] V, int Iterations, Option<double> Residual, Option<int> FactorNonZeros, Option<double> SpectralGap);

file readonly record struct ArapState(double[] U, double[] V, int Iterations, Option<double> Residual);
```

Lines 525-537
```csharp
    public Fin<Arr<double>> Solve(Func<int, double> pinnedValue, Op key) {
        double[] rhs = new double[InteriorCount];
        foreach ((int i, int slot, double w) in Couplings) rhs[i] += w * pinnedValue(slot);
        return Factor.SolveDetailed(new Arr<double>(rhs), key).Map(static solve => solve.Solution);
    }

    public Fin<Arr<double>> SolveWith(Memory<double> source, Func<int, double> pinnedValue, Op key) {
        double[] rhs = new double[InteriorCount];
        ReadOnlySpan<double> plane = source.Span;
        for (int vertex = 0; vertex < Map.Length; vertex++) { if (Map[vertex] >= 0) rhs[Map[vertex]] = plane[vertex]; }
        foreach ((int i, int slot, double w) in Couplings) rhs[i] += w * pinnedValue(slot);
        return Factor.SolveDetailed(new Arr<double>(rhs), key).Map(static solve => solve.Solution);
    }
```

To

Lines 187-198
```csharp
public sealed record Distortion(
    double MaxConformal,
    double MeanConformal,
    double MaxArea,
    double MinArea,
    double MeanArea,
    double MaxQuasiConformal,
    int Iterations,
    double SolveResidual,
    Option<double> ConvergenceDelta,
    Option<int> FactorNonZeros,
    Option<double> LscmEigenvalue);
```

Lines 311-318
```csharp
    static Fin<Solved> FlattenLscm(MeshDec dec, ParamPolicy policy, Op key) =>
        dec.Loops.Length == 0
            ? Fin.Fail<Solved>(new GeometryFault.InvalidChartBoundary(0, None))
            : SparseMatrix.FromTriplets(Dimension.Create(2 * dec.VertexCount), Dimension.Create(2 * dec.VertexCount), dec.ConformalTriplets(), key)
                .Bind(conformal => conformal.SmallestEigenpairsDetailed(k: GaugeModes + 1, tolerance: policy.ResidualTolerance.Value, budget: policy.EigenBudget, key: key))
                .Bind(eigen => eigen.PairsIn(expected: EigenOrder.Ascending, key: key).Bind(pairs => pairs.Count > GaugeModes
                    ? Fin.Succ(SplitComplex(dec, pairs[GaugeModes], eigen.Evidence.Iterations.IfNone(0), eigen.MaxResidual))
                    : Fin.Fail<Solved>(new GeometryFault.IncompleteParameterizationSpectrum(GaugeModes + 1, pairs.Count))));
```

Lines 328-349
```csharp
                Atom<Fin<Solved>> cell = Atom(value: Fin.Succ(seed));
                Transition<Fin<Solved>> driven = Cell.Converge(
                    cell: cell,
                    step: state => Some(state.Bind(active => Settled(active.Delta) ? Fin.Succ(active) : Step(active))),
                    settled: state => state.Match(Succ: active => Settled(active.Delta), Fail: static _ => true),
                    budget: policy.MaxIterations,
                    declined: key.InvalidResult());
                return driven.Current.Bind(state => Settled(state.Delta)
                    ? Fin.Succ(state)
                    : Fin.Fail<Solved>(new GeometryFault.ParameterizationUnconverged(state.Delta, state.Iterations)));

                bool Settled(Option<double> delta) => delta.Exists(value => value <= tolerance);

                Fin<Solved> Step(Solved state) =>
                    dec.LocalRotations(state.U, state.V, key).Bind(rotations => {
                        dec.RotatedGradient(rotations, axis: 0, sink: gradientU.Memory);
                        dec.RotatedGradient(rotations, axis: 1, sink: gradientV.Memory);
                        return from solvedU in system.Solve(k => state.U[gauge[k]], key, source: Some<ReadOnlyMemory<double>>(gradientU.Memory))
                               from solvedV in system.Solve(k => state.V[gauge[k]], key, source: Some<ReadOnlyMemory<double>>(gradientV.Memory))
                               let nextU = system.Scatter(gauge, k => state.U[gauge[k]], solvedU.Solution)
                               let nextV = system.Scatter(gauge, k => state.V[gauge[k]], solvedV.Solution)
                               select new Solved(nextU, nextV, state.Iterations + 1,
                                   Math.Max(solvedU.Residual, solvedV.Residual), Some(system.FactorNonZeros), None,
                                   Some(MaxDelta(state.U, nextU, state.V, nextV, scratch.Memory)));
                    });
```

Lines 367-379
```csharp
    static Fin<Solved> FlattenBoundary(MeshDec dec, int[] loop, Point2d[] pinned, Op key) =>
        dec.Reduced(loop, key).Bind(system =>
            from solvedU in system.Solve(k => pinned[k].X, key)
            from solvedV in system.Solve(k => pinned[k].Y, key)
            select new Solved(
                system.Scatter(loop, k => pinned[k].X, solvedU.Solution),
                system.Scatter(loop, k => pinned[k].Y, solvedV.Solution),
                1, Math.Max(solvedU.Residual, solvedV.Residual), Some(system.FactorNonZeros), None, None));

    static Solved SplitComplex(
        MeshDec dec, (double Eigenvalue, Arr<double> Eigenvector) pair, int iterations, double residual) {
        int n = dec.VertexCount;
        double[] u = new double[n];
        double[] v = new double[n];
        for (int i = 0; i < n; i++) { u[i] = pair.Eigenvector[i]; v[i] = pair.Eigenvector[n + i]; }
        return new Solved(u, v, iterations, residual, None, Some(pair.Eigenvalue), None);
    }
```

Lines 427-432
```csharp
        return new Distortion(
            MaxConformal: TensorPrimitives.Max(c), MeanConformal: TensorPrimitives.Sum(c) / n,
            MaxArea: TensorPrimitives.Max(a), MinArea: TensorPrimitives.Min(a), MeanArea: TensorPrimitives.Sum(a) / n,
            MaxQuasiConformal: TensorPrimitives.MaxMagnitude(q),
            Iterations: solved.Iterations, SolveResidual: solved.Residual, ConvergenceDelta: solved.Delta,
            FactorNonZeros: solved.FactorNonZeros, LscmEigenvalue: solved.LscmEigenvalue);
```

Lines 47-50
```csharp
using Solved = (double[] U, double[] V, int Iterations, double Residual, LanguageExt.Option<int> FactorNonZeros, LanguageExt.Option<double> LscmEigenvalue, LanguageExt.Option<double> Delta);
```

Lines 488-490
```csharp
// Solved and ArapState DELETED
```

Lines 525-537
```csharp
    public Fin<LinearSolution> Solve(
        Func<int, double> pinnedValue, Op key, Option<ReadOnlyMemory<double>> source = default) {
        double[] rhs = new double[InteriorCount];
        source.Iter(memory => {
            ReadOnlySpan<double> plane = memory.Span;
            for (int vertex = 0; vertex < Map.Length; vertex++)
                if (Map[vertex] >= 0) rhs[Map[vertex]] = plane[vertex];
        });
        foreach ((int i, int slot, double w) in Couplings) rhs[i] += w * pinnedValue(slot);
        return Factor.SolveDetailed(new Arr<double>(rhs), key);
    }
```

Why

Both reduced-system entries map `LinearSolution` immediately to its vector, violating the package ruling that every matrix solution is read whole: the true residual and route evidence disappear, and the atlas then publishes `None` for direct and eigen residuals the matrix owner measured. The existing `Distortion.Residual` further mixes ARAP iterate delta with solver residual, two unrelated quantities. `Solve` and `SolveWith` also duplicate one factor path solely for optional source seeding.

Change

Return `LinearSolution` intact from one source-optional solve entry, scatter only its `Solution`, and retain the maximum U/V true residual. Carry eigensolver `MaxResidual` through the same solved tuple. Separate the universally measured `SolveResidual` from ARAP's optional `ConvergenceDelta`, and rename the falsely claimed `SpectralGap` to `LscmEigenvalue`, because the code stores λ₃ itself and never subtracts adjacent eigenvalues. Rewrite the output law accordingly.

Delta

Code-fence LOC: -2 across the reduced solve entries, solved-state wrappers, and result projection. Module surface: -1 method, +1 truthful result member, net 0 members. Carrier states: -1 false residual absence and -1 mixed-unit column; +1 total solve residual and +1 optional convergence delta.

Ripples

- `libs/dotnet/Rasm.Fabrication/.planning/Forming/tube.md:1299-1306`: replace `.Maybe(row.Residual, ...)` with `.Double(row.SolveResidual).Maybe(row.ConvergenceDelta, ...)`, rename `.Maybe(row.SpectralGap, ...)` to `.Maybe(row.LscmEigenvalue, ...)`, and apply the flip-flag deletion from task 2.

# 8. Inline ARAP delta reduction into existing scratch

From

Lines 322-328
```csharp
            int[] gauge = [dec.Anchor];
            double tolerance = policy.ResidualTolerance.Value;
            return dec.Reduced(gauge, key).Bind(system => {
                using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(dec.VertexCount, AllocationMode.Clear);
                using MemoryOwner<double> gradientU = MemoryOwner<double>.Allocate(dec.VertexCount, AllocationMode.Clear);
                using MemoryOwner<double> gradientV = MemoryOwner<double>.Allocate(dec.VertexCount, AllocationMode.Clear);
```

Lines 345-349
```csharp
                        return from solvedU in system.SolveWith(gradientU.Memory, k => state.U[gauge[k]], key)
                               from solvedV in system.SolveWith(gradientV.Memory, k => state.V[gauge[k]], key)
                               let nextU = system.Scatter(gauge, k => state.U[gauge[k]], solvedU)
                               let nextV = system.Scatter(gauge, k => state.V[gauge[k]], solvedV)
                               select new ArapState(nextU, nextV, state.Iterations + 1, Some(MaxDelta(state.U, nextU, state.V, nextV, scratch.Memory)));
```

Lines 578-578
```csharp
    public int Anchor => Loops.Length > 0 ? Loops[0][0] : 0;
```

Lines 479-486
```csharp
    static double MaxDelta(double[] u, double[] nextU, double[] v, double[] nextV, Memory<double> scratch) {
        Span<double> plane = scratch.Span;
        TensorPrimitives.Subtract(nextU, u, plane[..u.Length]);
        double du = TensorPrimitives.MaxMagnitude(plane[..u.Length]);
        TensorPrimitives.Subtract(nextV, v, plane[..v.Length]);
        return Math.Max(du, TensorPrimitives.MaxMagnitude(plane[..v.Length]));
    }
```

To

Lines 322-328
```csharp
            int[] gauge = [dec.Loops[0][0]];
            double tolerance = policy.ResidualTolerance.Value;
            return dec.Reduced(gauge, key).Bind(system => {
                using MemoryOwner<double> gradientU = MemoryOwner<double>.Allocate(dec.VertexCount, AllocationMode.Clear);
                using MemoryOwner<double> gradientV = MemoryOwner<double>.Allocate(dec.VertexCount, AllocationMode.Clear);
```

Lines 345-349
```csharp
                        return (from solvedU in system.Solve(k => state.U[gauge[k]], key, Some<ReadOnlyMemory<double>>(gradientU.Memory))
                                from solvedV in system.Solve(k => state.V[gauge[k]], key, Some<ReadOnlyMemory<double>>(gradientV.Memory))
                                select (U: solvedU, V: solvedV)).Map(solved => {
                            double[] nextU = system.Scatter(gauge, k => state.U[gauge[k]], solved.U.Solution);
                            double[] nextV = system.Scatter(gauge, k => state.V[gauge[k]], solved.V.Solution);
                            Span<double> delta = gradientU.Span;
                            TensorPrimitives.Subtract(nextU, state.U, delta);
                            double largest = TensorPrimitives.MaxMagnitude(delta);
                            TensorPrimitives.Subtract(nextV, state.V, delta);
                            largest = Math.Max(largest, TensorPrimitives.MaxMagnitude(delta));
                            return new Solved(nextU, nextV, state.Iterations + 1,
                                Math.Max(solved.U.Residual, solved.V.Residual), Some(system.FactorNonZeros), None, Some(largest));
                        });
```

Lines 578-578
```csharp
// MeshDec.Anchor DELETED
```

Lines 479-486
```csharp
// MaxDelta DELETED
```

Why

The U-gradient buffer is dead after both factor solves and has the exact vertex-count shape the convergence delta needs, so a third pooled buffer exists only to serve the one-call `MaxDelta` helper. `MeshDec.Anchor` is another one-call wrapper whose fallback is unreachable: a successful LSCM seed has already proved at least one boundary loop before ARAP selects the gauge.

Change

Reuse `gradientU` for both subtraction passes directly in `Step`, delete `MaxDelta`, and read the already-proved first loop vertex at the call site. Preserve the typed unconverged exit and the same exact gauge value.

Delta

Code-fence LOC: -1 scratch lease, -7 helper lines, +5 call-site lines, -1 property, net -4. Module surface: -1 method and -1 property, +0, net -2 symbols. Runtime scratch: -1 vertex-count pooled plane.

# 9. Let using declarations own mesh-edit disposal

From

Lines 210-225
```csharp
    public Fin<MeshSpace> ToMesh(Op? key = null) {
        MeshEdit edit = MeshEdit.Of(Source);
        try {
            Dictionary<(int, int, int), int> faceAt = new(edit.FaceCount);
            for (int f = 0; f < edit.FaceCount; f++) { faceAt[Cyclic(edit.Face(f))] = f; }
            foreach (UvIsland island in Islands) {
                Dictionary<int, int> at = new(island.Vertices.Count);
                for (int i = 0; i < island.Vertices.Count; i++) { at[island.Vertices[i]] = i; }
                foreach ((int a, int b, int c) in island.Faces) {
                    edit.SetCornerUv(faceAt[Cyclic((a, b, c))], island.Uv[at[a]], island.Uv[at[b]], island.Uv[at[c]]);
                }
            }
            return edit.ToSpace(key.OrDefault());
        }
        finally { edit.Dispose(); }
    }
```

Lines 230-244
```csharp
    public Fin<MeshSpace> ToTextureMesh(Op? key = null) {
        List<Point3d> vertices = new();
        List<(int A, int B, int C)> faces = new();
        foreach (UvIsland island in Islands) {
            Dictionary<int, int> remap = new(island.Vertices.Count);
            for (int i = 0; i < island.Vertices.Count; i++) {
                remap[island.Vertices[i]] = vertices.Count;
                vertices.Add(new Point3d(island.Uv[i].X, island.Uv[i].Y, 0.0));
            }
            foreach ((int a, int b, int c) in island.Faces) faces.Add((remap[a], remap[b], remap[c]));
        }
        MeshEdit edit = MeshEdit.Of(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(faces), Source.Tolerance);
        try { return edit.ToSpace(key.OrDefault()); }
        finally { edit.Dispose(); }
    }
```

To

Lines 210-225
```csharp
    public Fin<MeshSpace> ToMesh(Op? key = null) {
        using MeshEdit edit = MeshEdit.Of(Source);
        Dictionary<(int, int, int), int> faceAt = new(edit.FaceCount);
        for (int f = 0; f < edit.FaceCount; f++) { faceAt[Cyclic(edit.Face(f))] = f; }
        foreach (UvIsland island in Islands) {
            Dictionary<int, int> at = new(island.Vertices.Count);
            for (int i = 0; i < island.Vertices.Count; i++) { at[island.Vertices[i]] = i; }
            foreach ((int a, int b, int c) in island.Faces)
                edit.SetCornerUv(faceAt[Cyclic((a, b, c))], island.Uv[at[a]], island.Uv[at[b]], island.Uv[at[c]]);
        }
        return edit.ToSpace(key.OrDefault());
    }
```

Lines 230-244
```csharp
    public Fin<MeshSpace> ToTextureMesh(Op? key = null) {
        List<Point3d> vertices = new();
        List<(int A, int B, int C)> faces = new();
        foreach (UvIsland island in Islands) {
            Dictionary<int, int> remap = new(island.Vertices.Count);
            for (int i = 0; i < island.Vertices.Count; i++) {
                remap[island.Vertices[i]] = vertices.Count;
                vertices.Add(new Point3d(island.Uv[i].X, island.Uv[i].Y, 0.0));
            }
            foreach ((int a, int b, int c) in island.Faces) faces.Add((remap[a], remap[b], remap[c]));
        }
        using MeshEdit edit = MeshEdit.Of(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(faces), Source.Tolerance);
        return edit.ToSpace(key.OrDefault());
    }
```

Why

`MeshEdit` implements `IDisposable`; both methods hand-write a `try/finally` that does nothing beyond the disposal a using declaration already guarantees on success, failure, and early return.

Change

Declare each edit lease with `using`, keep the mutation bodies unchanged, and return the frozen `MeshSpace` directly.

Delta

Code-fence LOC: 31 to 27, net -4. Module surface: unchanged.

# 10. Traverse optional policy admissions once

From

Lines 98-112
```csharp
    public static Fin<ParamPolicy> Of(
        Option<double> residualTolerance = default, Option<double> creaseDihedral = default,
        Option<Dimension> maxIterations = default, Option<Dimension> eigenBudget = default,
        Option<Dimension> parallelFloor = default, Op? key = null) {
        Op op = key.OrDefault();
        return from residual in residualTolerance.Match(
                   Some: value => op.AcceptValidated<PositiveMagnitude>(candidate: value),
                   None: () => Fin.Succ(Canonical.ResidualTolerance))
               from crease in creaseDihedral.Match(
                   Some: value => op.AcceptValidated<VectorAngle>(candidate: value),
                   None: () => Fin.Succ(Canonical.CreaseDihedral))
               from _ in guard(crease.Value < Math.PI, op.InvalidInput())
               select new ParamPolicy(residual, maxIterations.IfNone(Canonical.MaxIterations),
                   eigenBudget.IfNone(Canonical.EigenBudget), crease, parallelFloor.IfNone(Canonical.ParallelFloor));
    }
```

To

Lines 98-112
```csharp
    public static Fin<ParamPolicy> Of(
        Option<double> residualTolerance = default, Option<double> creaseDihedral = default,
        Option<Dimension> maxIterations = default, Option<Dimension> eigenBudget = default,
        Option<Dimension> parallelFloor = default, Op? key = null) {
        Op op = key.OrDefault();
        return from residual in residualTolerance.TraverseM(
                   value => op.AcceptValidated<PositiveMagnitude>(candidate: value)).As()
               from crease in creaseDihedral.TraverseM(
                   value => op.AcceptValidated<VectorAngle>(candidate: value)).As()
               let angle = crease.IfNone(Canonical.CreaseDihedral)
               from _ in guard(angle.Value < Math.PI, op.InvalidInput())
               select new ParamPolicy(residual.IfNone(Canonical.ResidualTolerance),
                   maxIterations.IfNone(Canonical.MaxIterations), eigenBudget.IfNone(Canonical.EigenBudget),
                   angle, parallelFloor.IfNone(Canonical.ParallelFloor));
    }
```

Why

The two `Option.Match` pairs hand-write the same absence-total effect inversion. LanguageExt already defines that composition: `Option.TraverseM` leaves absence untouched, runs generated value-object admission only for a supplied raw value, and preserves the one `Fin` carrier.

Change

Traverse each optional raw scalar through `AcceptValidated`, re-anchor once with `.As()`, default only after admission, and retain the dependent half-turn guard on the selected crease angle. The `Dimension` options remain trusted generated values and are not revalidated.

Delta

Code-fence LOC: 15 to 14, net -1. Module surface: unchanged.

# 11. Remove unused cycle context and refusal wrappers

From

Lines 167-167
```csharp
        return Cycles.Of(successor, op)
```

Lines 494-519
```csharp
file static class Cycles {
    internal static Fin<Seq<Seq<int>>> Of(Dictionary<int, int> successor, Op key) {
        Seq<Seq<int>> loops = [];
        IndexSet seen = new(successor.Count);
        foreach (int seed in successor.Keys.OrderBy(static k => k)) {
            if (!seen.Add(seed)) continue;
            Seq<int> loop = Seq(seed);
            int at = seed;
            while (true) {
                if (!successor.TryGetValue(at, out int step)) return Open(at);
                if (step == seed) break;
                if (!seen.Add(step)) return Merged(step);
                loop = loop.Add(step);
                at = step;
            }
            loops = loops.Add(loop);
        }
        return loops.IsEmpty ? Empty() : Fin.Succ(loops);

        static Fin<Seq<Seq<int>>> Open(int at) =>
            Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, at, "boundary: open half-edge chain"));
        static Fin<Seq<Seq<int>>> Merged(int at) =>
            Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, at, "boundary: two half-edges share one head"));
        static Fin<Seq<Seq<int>>> Empty() =>
            Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "boundary: no closed loop"));
    }
}
```

Lines 575-576
```csharp
        from loops in BoundaryLoops(native, key)
        select new MeshDec(snapshot.Calculus, native, chart.Tolerance, loops, cuts, cutEdges);
```

Lines 764-774
```csharp
    static Fin<int[][]> BoundaryLoops(Mesh mesh, Op key) {
        EdgeKeySet directed = new(3 * mesh.Faces.Count);
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces.GetFace(f);
            directed.Add((face.A, face.B)); directed.Add((face.B, face.C)); directed.Add((face.C, face.A));
        }
        Dictionary<int, int> next = new();
        foreach ((int u, int v) in directed) { if (!directed.Contains((v, u))) next[u] = v; }
        return next.Count == 0
            ? Fin.Succ(System.Array.Empty<int[]>())
            : Cycles.Of(next, key).Map(static loops => loops.Map(static loop => loop.ToArray()).ToArray());
```

To

Lines 167-167
```csharp
        return Cycles.Of(successor)
```

Lines 494-519
```csharp
file static class Cycles {
    internal static Fin<Seq<Seq<int>>> Of(Dictionary<int, int> successor) {
        Seq<Seq<int>> loops = [];
        IndexSet seen = new(successor.Count);
        foreach (int seed in successor.Keys.OrderBy(static value => value)) {
            if (!seen.Add(seed)) continue;
            Seq<int> loop = Seq(seed);
            int at = seed;
            while (true) {
                if (!successor.TryGetValue(at, out int step))
                    return Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, at, "boundary: open half-edge chain"));
                if (step == seed) break;
                if (!seen.Add(step))
                    return Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, step, "boundary: two half-edges share one head"));
                loop = loop.Add(step);
                at = step;
            }
            loops = loops.Add(loop);
        }
        return loops.IsEmpty
            ? Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "boundary: no closed loop"))
            : Fin.Succ(loops);
    }
}
```

Lines 575-576
```csharp
        from loops in BoundaryLoops(native)
        select new MeshDec(snapshot.Calculus, native, chart.Tolerance, loops, cuts, cutEdges);
```

Lines 764-774
```csharp
    static Fin<int[][]> BoundaryLoops(Mesh mesh) {
        EdgeKeySet directed = new(3 * mesh.Faces.Count);
        for (int face = 0; face < mesh.Faces.Count; face++) {
            MeshFace row = mesh.Faces.GetFace(face);
            directed.Add((row.A, row.B)); directed.Add((row.B, row.C)); directed.Add((row.C, row.A));
        }
        Dictionary<int, int> next = new();
        foreach ((int u, int v) in directed) if (!directed.Contains((v, u))) next[u] = v;
        return next.Count == 0
            ? Fin.Succ(System.Array.Empty<int[]>())
            : Cycles.Of(next).Map(static loops => loops.Map(static loop => loop.ToArray()).ToArray());
```

Why

`Cycles.Of` never reads its `Op`, so both callers thread context into a pure walker that mints fixed geometry faults. `BoundaryLoops` consequently carries an `Op` solely to forward it. The three local refusal functions each wrap one return and hide the failing branch without sharing behavior.

Change

Remove the unused `Op` from `Cycles.Of` and `BoundaryLoops`, update both callers, and construct each typed refusal at the branch that detects it. Keep the shared cycle walker because both island egress and mesh admission consume its ordered-loop algorithm.

Delta

Code-fence LOC: 42 to 37, net -5. Module surface: -2 parameters; local surface: -3 functions; +0, net -5 symbols.
