using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public abstract partial record SpatialProbe {
    private SpatialProbe() { }
    public sealed record NearestCase(int Count) : SpatialProbe {
        internal override Fin<SpatialProbe> Validate(Op key) => guard(Count > 0, key.InvalidInput()).ToFin().Map(_ => (SpatialProbe)this);
        internal override IEnumerable<int[]> Neighbors(Point3d[] hay, Point3d[] needles) => RTree.Point3dKNeighbors(hayPoints: hay, needlePts: needles, amount: Count);
    }
    public sealed record WithinCase(double Distance) : SpatialProbe {
        internal override Fin<SpatialProbe> Validate(Op key) => guard(RhinoMath.IsValidDouble(x: Distance) && Distance > 0.0, key.InvalidInput()).ToFin().Map(_ => (SpatialProbe)this);
        internal override IEnumerable<int[]> Neighbors(Point3d[] hay, Point3d[] needles) => RTree.Point3dClosestPoints(hayPoints: hay, needlePts: needles, limitDistance: Distance);
    }
    public static SpatialProbe Nearest(int count) => new NearestCase(Count: count);
    public static SpatialProbe Within(double distance) => new WithinCase(Distance: distance);
    internal abstract Fin<SpatialProbe> Validate(Op key);
    internal abstract IEnumerable<int[]> Neighbors(Point3d[] hay, Point3d[] needles);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpatialHit(int Id) { public bool IsValid => Id >= 0; }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpatialPair(int A, int B) { public bool IsValid => A >= 0 && B >= 0 && A != B; }

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class Tree : IDisposable {
    internal static readonly Op Key = Op.Of(name: nameof(Tree));
    private readonly RTree tree;
    private bool disposed;
    private Tree(RTree tree) => this.tree = tree;
    public static Validation<Error, Tree> Points(params ReadOnlySpan<Point3d> points) =>
        ValidatePoints(points: points)
            .Bind(static values => Optional(RTree.CreateFromPointArray(points: values)).ToFin(Key.InvalidResult()))
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> PointCloud(PointCloud cloud) =>
        FromValid(input: cloud, isValid: static c => c.IsValid, create: static c => RTree.CreatePointCloudTree(cloud: c));
    public static Validation<Error, Tree> MeshFaces(Mesh mesh) =>
        FromValid(input: mesh, isValid: static m => m.IsValid, create: static m => RTree.CreateMeshFaceTree(mesh: m));
    public static Validation<Error, Tree> FromBounds<TGeometry>(params ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        ValidateBounds(items: items)
            .Bind(static boxes => toSeq(boxes)
                .Map(static (box, index) => (Box: box, Index: index))
                .Fold(
                    initialState: Fin.Succ(new RTree()),
                    f: static (current, item) => current.Bind(tree => tree.Insert(box: item.Box, elementId: item.Index) switch {
                        true => Fin.Succ(tree),
                        false => new Lease<RTree>.Owned(Value: tree).Use(static _ => Fin.Fail<RTree>(Key.InvalidResult())),
                    })))
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    private static Validation<Error, Tree> FromValid<TInput>(TInput input, Func<TInput, bool> isValid, Func<TInput, RTree?> create) where TInput : class =>
        Optional(input)
            .ToFin(new Fault.MissingGeometry())
            .Bind(candidate => isValid(arg: candidate) switch {
                true => Optional(create(arg: candidate)).ToFin(Key.InvalidResult()),
                false => Fin.Fail<RTree>(Key.InvalidInput()),
            })
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    internal Eff<Env, Seq<SpatialHit>> Hits(BoundingBox box) =>
        RunSearch(shape: box, run: static (index, bounds, callback) => index.Search(box: bounds, callback: callback), isValid: static b => b.IsValid);
    internal Eff<Env, Seq<SpatialHit>> Hits(Sphere sphere) =>
        RunSearch(shape: sphere, run: static (index, ball, callback) => index.Search(sphere: ball, callback: callback), isValid: static s => s.IsValid);
    private Eff<Env, Seq<SpatialHit>> RunSearch<TShape>(TShape shape, Func<RTree, TShape, EventHandler<RTreeEventArgs>, bool> run, Func<TShape, bool> isValid) =>
        from runtime in Env.EnvAsks
        from active in Ready().ToEff()
        from result in (isValid(arg: shape)
            ? Search(tree: active, shape: shape, run: run, cancel: runtime.Cancellation)
            : Fin.Fail<Seq<SpatialHit>>(Key.InvalidInput())).ToEff()
        select result;
    internal Eff<Env, Seq<SpatialPair>> OverlapPairsWith(Tree other, double tolerance = 0.0) =>
        from runtime in Env.EnvAsks
        from state in (
            Ready(),
            Optional(other)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static index => index.Ready()),
            guard(RhinoMath.IsValidDouble(x: tolerance) && tolerance >= 0.0, Key.InvalidInput()).ToFin().Map(_ => tolerance)
        ).Apply(static (left, right, modelTolerance) => (Left: left, Right: right, Tolerance: modelTolerance)).As().ToEff()
        from pairs in OverlapPairs(state: state, cancel: runtime.Cancellation).ToEff()
        select pairs;
    private static Fin<Seq<SpatialPair>> OverlapPairs((RTree Left, RTree Right, double Tolerance) state, CancellationToken cancel) =>
        // BOUNDARY ADAPTER — Rhino RTree.SearchOverlaps uses a mutating callback delegate, single-threaded per search.
        BoundaryRTreeSearch<SpatialPair>(
            run: buffer => RTree.SearchOverlaps(treeA: state.Left, treeB: state.Right, tolerance: state.Tolerance,
                callback: (_, args) => { args.Cancel = cancel.IsCancellationRequested; buffer.Add(item: new SpatialPair(A: args.Id, B: args.IdB)); }),
            sort: SortedPairs,
            cancel: cancel);
    private static Fin<Seq<TItem>> BoundaryRTreeSearch<TItem>(Func<List<TItem>, bool> run, Func<List<TItem>, Seq<TItem>> sort, CancellationToken cancel) {
        List<TItem> buffer = [];
        return run(arg: buffer) switch {
            true when cancel.IsCancellationRequested => Fin.Fail<Seq<TItem>>(new Fault.Cancelled()),
            true => Fin.Succ(sort(arg: buffer)),
            false when cancel.IsCancellationRequested => Fin.Fail<Seq<TItem>>(new Fault.Cancelled()),
            false => Fin.Fail<Seq<TItem>>(Key.InvalidResult()),
        };
    }
    public void Dispose() =>
        disposed = disposed || new Lease<RTree>.Owned(Value: tree).Use(static _ => true);
    private Fin<RTree> Ready() =>
        guard(!disposed, Key.InvalidInput()).ToFin().Map(_ => tree);
    internal static Fin<Point3d[]> ValidatePoints(ReadOnlySpan<Point3d> points) =>
        Seq(points)
            .TraverseM(static point => point switch {
                Point3d candidate when candidate.IsValid => Fin.Succ(candidate),
                _ => Fin.Fail<Point3d>(Key.InvalidInput()),
            })
            .As()
            .Map(static values => values.ToArray());
    private static Fin<BoundingBox[]> ValidateBounds<TGeometry>(ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        Seq(items)
            .TraverseM(static geometry => Optional(geometry)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static candidate => guard(candidate.IsValid, Key.InvalidInput()).ToFin().Bind(_ => candidate.BoundsOf(op: Key).Bind(static box => guard(box.IsValid, Key.InvalidInput()).ToFin().Map(_ => box)))))
            .As()
            .Map(static boxes => boxes.ToArray());
    private static Fin<Seq<SpatialHit>> Search<TShape>(RTree tree, TShape shape, Func<RTree, TShape, EventHandler<RTreeEventArgs>, bool> run, CancellationToken cancel) =>
        // BOUNDARY ADAPTER — Rhino RTree.Search uses a mutating callback delegate, single-threaded per search.
        BoundaryRTreeSearch<int>(
            run: buffer => run(arg1: tree, arg2: shape, arg3: (_, args) => { args.Cancel = cancel.IsCancellationRequested; buffer.Add(item: args.Id); }),
            sort: SortedHits,
            cancel: cancel)
        .Map(static ids => ids.Map(static id => new SpatialHit(Id: id)));
    private static Seq<int> SortedHits(List<int> buffer) {
        buffer.Sort();
        return toSeq(buffer);
    }
    private static Seq<SpatialPair> SortedPairs(List<SpatialPair> buffer) {
        buffer.Sort(comparison: static (left, right) => (left.A, right.A) switch {
            (int a, int b) when a != b => a.CompareTo(value: b),
            _ => left.B.CompareTo(value: right.B),
        });
        return toSeq(buffer);
    }
    internal static Fin<Seq<SpatialPair>> PointPairs(IEnumerable<int[]> values) => Optional(values)
            .ToFin(Key.InvalidResult())
            .Map(static rows => SortedPairs(buffer: [.. rows.SelectMany(static (ids, needle) => ids.Select(source => new SpatialPair(A: needle, B: source)))]));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<Unit, TOut> SpatialSearch<TOut>(Tree index, BoundingBox box, Op key) where TOut : notnull => SpatialSearch<TOut, BoundingBox>(index: index, shape: box, key: key, search: static (tree, bounds) => tree.Hits(box: bounds));
    internal static Operation<Unit, TOut> SpatialSearch<TOut>(Tree index, Sphere sphere, Op key) where TOut : notnull => SpatialSearch<TOut, Sphere>(index: index, shape: sphere, key: key, search: static (tree, ball) => tree.Hits(sphere: ball));
    private static Operation<Unit, TOut> SpatialSearch<TOut, TShape>(Tree index, TShape shape, Op key, Func<Tree, TShape, Eff<Env, Seq<SpatialHit>>> search) where TOut : notnull =>
        Operation<Unit, SpatialHit>.Build(key: key, requiresContext: true, state: (Index: index, Shape: shape, Search: search, Key: key),
            evaluator: static (state, _) =>
                from tree in Optional(state.Index).ToFin(state.Key.InvalidInput()).ToEff()
                from hits in state.Search(arg1: tree, arg2: state.Shape)
                from output in new AnalysisOutput<SpatialHit>(state.Key).Many(values: hits).ToEff()
                select output)
            .As<Unit, TOut>(key: key);

    internal static Operation<Unit, TOut> SpatialOverlaps<TOut>(Tree left, Tree right, double tolerance, Op key) where TOut : notnull =>
        Operation<Unit, SpatialPair>.Build(key: key, requiresContext: true, state: (Left: left, Right: right, Tolerance: tolerance, Key: key),
            evaluator: static (state, _) =>
                from tree in Optional(state.Left).ToFin(state.Key.InvalidInput()).ToEff()
                from pairs in tree.OverlapPairsWith(other: state.Right, tolerance: state.Tolerance)
                from output in new AnalysisOutput<SpatialPair>(state.Key).Many(values: pairs).ToEff()
                select output)
            .As<Unit, TOut>(key: key);

    internal static Operation<Unit, TOut> SpatialPointPairs<TOut>(Seq<Point3d> points, Seq<Point3d> needles, SpatialProbe probe, Op key) where TOut : notnull =>
        Operation<Unit, SpatialPair>.Build(key: key, state: (Points: points, Needles: needles, Probe: probe, Key: key),
            evaluator: static (state, _) =>
                from runtime in Env.EnvAsks
                from validated in (
                    Tree.ValidatePoints(points: state.Points.ToArray()),
                    Tree.ValidatePoints(points: state.Needles.ToArray()),
                    Optional(state.Probe).ToFin(state.Key.InvalidInput()).Bind(probe => probe.Validate(key: state.Key))
                ).Apply(static (hay, needles, valid) => (Hay: hay, Needles: needles, Probe: valid)).As().ToEff()
                from notCancelled in (runtime.Cancellation.IsCancellationRequested ? Fin.Fail<Unit>(new Fault.Cancelled()) : Fin.Succ(unit)).ToEff()
                from pairs in Tree.PointPairs(values: validated.Probe.Neighbors(hay: validated.Hay, needles: validated.Needles)).ToEff()
                from output in new AnalysisOutput<SpatialPair>(state.Key).Many(values: pairs).ToEff()
                select output)
            .As<Unit, TOut>(key: key);
}
