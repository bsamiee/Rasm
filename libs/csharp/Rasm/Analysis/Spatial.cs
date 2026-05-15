namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Probe {
    public sealed record Nearest(int Count) : Probe;
    public sealed record Within(double Distance) : Probe;
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class Tree : IDisposable {
    internal static readonly Op Key = Op.Of(name: nameof(Tree));
    private readonly RTree tree;
    private bool disposed;
    private Tree(RTree tree) => this.tree = tree;
    public static Validation<Error, Tree> Points(params ReadOnlySpan<Point3d> points) =>
        ValidatePoints(points: points)
            .Bind(static values => Optional(RTree.CreateFromPointArray(points: values))
                .ToFin(Key.InvalidResult()))
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> PointCloud(PointCloud cloud) =>
        FromValid(input: cloud, isValid: static c => c.IsValid, create: static c => RTree.CreatePointCloudTree(cloud: c));
    public static Validation<Error, Tree> MeshFaces(Mesh mesh) =>
        FromValid(input: mesh, isValid: static m => m.IsValid, create: static m => RTree.CreateMeshFaceTree(mesh: m));
    public static Validation<Error, Tree> Bounds<TGeometry>(
        params ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        ValidateBounds(items: items)
            .Bind(static boxes => boxes
                .Select(static (box, index) => (Box: box, Index: index))
                .Aggregate(
                    seed: Fin.Succ(new RTree()), func: static (current, item) => current.Bind(tree => tree.Insert(
                            box: item.Box, elementId: item.Index) switch {
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
    public Eff<Env, Seq<Hit>> Search(BoundingBox box) =>
        from runtime in Env.EnvAsks
        from active in Ready().ToEff()
        from result in (box.IsValid
            ? Search(tree: active, shape: box, run: static (index, bounds, callback) => index.Search(box: bounds, callback: callback), cancel: runtime.Cancellation)
            : Fin.Fail<Seq<Hit>>(Key.InvalidInput())).ToEff()
        select result;
    public Eff<Env, Seq<Hit>> Search(Sphere sphere) =>
        from runtime in Env.EnvAsks
        from active in Ready().ToEff()
        from result in (sphere.IsValid
            ? Search(tree: active, shape: sphere, run: static (index, ball, callback) => index.Search(sphere: ball, callback: callback), cancel: runtime.Cancellation)
            : Fin.Fail<Seq<Hit>>(Key.InvalidInput())).ToEff()
        select result;
    public Eff<Env, Seq<Couple>> Overlaps(Tree other, double tolerance = 0.0) =>
        from runtime in Env.EnvAsks
        from state in (
            Ready(),
            Optional(other)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static index => index.Ready()),
            RhinoMath.IsValidDouble(x: tolerance) && tolerance >= 0.0
                ? Fin.Succ(tolerance)
                : Fin.Fail<double>(Key.InvalidInput())
        ).Apply(static (left, right, modelTolerance) => (Left: left, Right: right, Tolerance: modelTolerance)).As().ToEff()
        from pairs in OverlapPairs(state: state, cancel: runtime.Cancellation).ToEff()
        select pairs;
    private static Fin<Seq<Couple>> OverlapPairs((RTree Left, RTree Right, double Tolerance) state, CancellationToken cancel) {
        // BOUNDARY ADAPTER — Rhino RTree.SearchOverlaps uses a mutating callback delegate, single-threaded per search.
        List<Couple> buffer = [];
        return RTree.SearchOverlaps(
            treeA: state.Left,
            treeB: state.Right,
            tolerance: state.Tolerance,
            callback: (_, args) => { args.Cancel = cancel.IsCancellationRequested; buffer.Add(item: new Couple(A: args.Id, B: args.IdB)); }) switch {
                true when cancel.IsCancellationRequested => Fin.Fail<Seq<Couple>>(new Fault.Cancelled()),
                true => Fin.Succ(SortedCouples(buffer: buffer)),
                false when cancel.IsCancellationRequested => Fin.Fail<Seq<Couple>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Couple>>(Key.InvalidResult()),
            };
    }
    public void Dispose() =>
        disposed = disposed switch {
            false => new Lease<RTree>.Owned(Value: tree).Use(static _ => true),
            true => true,
        };
    private Fin<RTree> Ready() =>
        disposed switch {
            true => Fin.Fail<RTree>(Key.InvalidInput()),
            false => Fin.Succ(tree),
        };
    internal static Fin<Point3d[]> ValidatePoints(ReadOnlySpan<Point3d> points) =>
        Seq(points)
            .TraverseM(static point => point switch {
                Point3d candidate when candidate.IsValid => Fin.Succ(candidate),
                _ => Fin.Fail<Point3d>(Key.InvalidInput()),
            })
            .As()
            .Map(static values => values.ToArray());
    private static Fin<BoundingBox[]> ValidateBounds<TGeometry>(
        ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        Seq(items)
            .TraverseM(static geometry => Optional(geometry)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static candidate => candidate.IsValid
                    ? candidate.Bounds(op: Key).Bind(static box => box.IsValid ? Fin.Succ(box) : Fin.Fail<BoundingBox>(Key.InvalidInput()))
                    : Fin.Fail<BoundingBox>(Key.InvalidInput())))
            .As()
            .Map(static boxes => boxes.ToArray());
    private static Fin<Seq<Hit>> Search<TShape>(RTree tree, TShape shape, Func<RTree, TShape, EventHandler<RTreeEventArgs>, bool> run, CancellationToken cancel) {
        // BOUNDARY ADAPTER — Rhino RTree.Search uses a mutating callback delegate, single-threaded per search.
        List<int> buffer = [];
        return run(
            arg1: tree,
            arg2: shape,
            arg3: (_, args) => { args.Cancel = cancel.IsCancellationRequested; buffer.Add(item: args.Id); }) switch {
                true when cancel.IsCancellationRequested => Fin.Fail<Seq<Hit>>(new Fault.Cancelled()),
                true => Fin.Succ(SortedHits(buffer: buffer)),
                false when cancel.IsCancellationRequested => Fin.Fail<Seq<Hit>>(new Fault.Cancelled()),
                false => Fin.Fail<Seq<Hit>>(Key.InvalidResult()),
            };
    }
    private static Seq<Hit> SortedHits(List<int> buffer) {
        buffer.Sort();
        return toSeq(buffer.Select(static id => new Hit(Id: id)));
    }
    private static Seq<Couple> SortedCouples(List<Couple> buffer) {
        buffer.Sort(comparison: static (left, right) => (left.A, right.A) switch {
            (int a, int b) when a != b => a.CompareTo(value: b),
            _ => left.B.CompareTo(value: right.B),
        });
        return toSeq(buffer);
    }
    internal static Fin<Seq<Couple>> PointPairs(IEnumerable<int[]> values) => Optional(values)
            .ToFin(Key.InvalidResult())
            .Map(static rows => SortedCouples(buffer: [.. rows.SelectMany(static (ids, needle) => ids.Select(source => new Couple(A: needle, B: source)))]));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Spatial {
    public static Eff<Context, Seq<Couple>> NearestPoints(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        Probe probe) =>
        (Tree.ValidatePoints(points: points), Tree.ValidatePoints(points: needles), ValidateProbe(probe: probe))
            .Apply(static (hay, operation, valid) => (Hay: hay, Operation: operation, Probe: valid))
            .As()
            .Bind(static state => Tree.PointPairs(values: Neighbors(state: state)));
    private static IEnumerable<int[]> Neighbors((Point3d[] Hay, Point3d[] Operation, Probe Probe) state) => state.Probe switch {
        Probe.Nearest k => RTree.Point3dKNeighbors(hayPoints: state.Hay, needlePts: state.Operation, amount: k.Count),
        Probe.Within w => RTree.Point3dClosestPoints(hayPoints: state.Hay, needlePts: state.Operation, limitDistance: w.Distance),
        _ => [],
    };
    private static Fin<Probe> ValidateProbe(Probe probe) => probe switch {
        Probe.Nearest { Count: > 0 } => Fin.Succ(probe),
        Probe.Within w when RhinoMath.IsValidDouble(x: w.Distance) && w.Distance > 0.0 => Fin.Succ(probe),
        _ => Fin.Fail<Probe>(Tree.Key.InvalidInput()),
    };
}
