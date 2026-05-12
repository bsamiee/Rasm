namespace Rasm.Analysis;

public sealed class Tree : IDisposable {
    private static readonly Op Key = Op.Of(name: nameof(Tree));
    private readonly RTree tree;
    private bool disposed;
    private Tree(RTree tree) => this.tree = tree;
    public static Validation<Error, Tree> Points(params ReadOnlySpan<Point3d> points) =>
        ValidatePoints(points: points)
            .Bind(static values => Optional(RTree.CreateFromPointArray(points: values))
                .ToFin(Key.InvalidResult()))
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> PointCloud(PointCloud cloud) => Optional(cloud)
            .ToFin(new Fault.MissingGeometry())
            .Bind(static candidate => candidate.IsValid switch {
                true => Optional(RTree.CreatePointCloudTree(cloud: candidate))
                    .ToFin(Key.InvalidResult()),
                false => Fin.Fail<RTree>(Key.InvalidInput()),
            })
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> Bounds<TGeometry>(
        params ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        ValidateBounds(items: items)
            .Bind(static boxes => boxes
                .Select(static (box, index) => (Box: box, Index: index))
                .Aggregate(
                    seed: Fin.Succ(new RTree()), func: static (current, item) => current.Bind(tree => tree.Insert(
                            box: item.Box, elementId: item.Index) switch {
                                true => Fin.Succ(tree),
                                false => Fin.Fail<RTree>(Key.InvalidResult()),
                            })))
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> MeshFaces(Mesh mesh) => Optional(mesh)
            .ToFin(new Fault.MissingGeometry())
            .Bind(static candidate => candidate.IsValid switch {
                true => Optional(RTree.CreateMeshFaceTree(mesh: candidate))
                    .ToFin(Key.InvalidResult()),
                false => Fin.Fail<RTree>(Key.InvalidInput()),
            })
            .Map(static tree => new Tree(tree: tree))
            .ToValidation();
    public Eff<Context, Seq<Hit>> Search(BoundingBox box) =>
        Ready().Bind(active => box.IsValid switch { true => Search(tree: active, shape: box), false => Fin.Fail<Seq<Hit>>(Key.InvalidInput()) });
    public Eff<Context, Seq<Hit>> Search(Sphere sphere) =>
        Ready().Bind(active => sphere.IsValid switch { true => Search(tree: active, shape: sphere), false => Fin.Fail<Seq<Hit>>(Key.InvalidInput()) });
    public Eff<Context, Seq<Couple>> Overlaps(Tree other, double tolerance = 0.0) =>
        (
            Ready(),
            Optional(other)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static index => index.Ready()),
            RhinoMath.IsValidDouble(x: tolerance) && tolerance >= 0.0
                ? Fin.Succ(tolerance)
                : Fin.Fail<double>(Key.InvalidInput())
        ).Apply(static (left, right, modelTolerance) => (Left: left, Right: right, Tolerance: modelTolerance))
        .As()
        .Bind(static state => OverlapPairs(state: state));
    private static Fin<Seq<Couple>> OverlapPairs((RTree Left, RTree Right, double Tolerance) state) {
        // BOUNDARY ADAPTER — Rhino RTree.SearchOverlaps uses a mutating callback delegate.
        Atom<Seq<Couple>> atom = Atom(value: Seq<Couple>());
        return RTree.SearchOverlaps(
            treeA: state.Left,
            treeB: state.Right,
            tolerance: state.Tolerance,
            callback: (_, args) => atom.Swap(current => new Couple(A: args.Id, B: args.IdB).Cons(current))) switch {
                true => Fin.Succ(toSeq(atom.Value.OrderBy(static p => p.A).ThenBy(static p => p.B))),
                false => Fin.Fail<Seq<Couple>>(Key.InvalidResult()),
            };
    }
    public static Eff<Context, Seq<Couple>> KNearest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        int count) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            count switch {
                > 0 => Fin.Succ(count),
                _ => Fin.Fail<int>(Key.InvalidInput()),
            }
        ).Apply(static (hay, query, k) => (Hay: hay, Query: query, Count: k))
        .As()
        .Bind(static state => PointPairs(values: RTree.Point3dKNeighbors(hayPoints: state.Hay, needlePts: state.Query, amount: state.Count)));
    public static Eff<Context, Seq<Couple>> Closest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        double limitDistance) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            RhinoMath.IsValidDouble(x: limitDistance) && limitDistance > 0.0
                ? Fin.Succ(limitDistance)
                : Fin.Fail<double>(Key.InvalidInput())
        ).Apply(static (hay, query, distance) => (Hay: hay, Query: query, Distance: distance))
        .As()
        .Bind(static state => PointPairs(values: RTree.Point3dClosestPoints(hayPoints: state.Hay, needlePts: state.Query, limitDistance: state.Distance)));
    public void Dispose() =>
        disposed = disposed switch {
            false => fun((RTree t) => { t.Dispose(); return true; })(tree),
            true => true,
        };
    private Fin<RTree> Ready() =>
        disposed switch {
            true => Fin.Fail<RTree>(Key.InvalidInput()),
            false => Fin.Succ(tree),
        };
    private static Fin<Point3d[]> ValidatePoints(ReadOnlySpan<Point3d> points) =>
        toSeq(points.ToArray())
            .TraverseM(static point => point switch {
                Point3d candidate when candidate.IsValid => Fin.Succ(candidate),
                _ => Fin.Fail<Point3d>(Key.InvalidInput()),
            })
            .As()
            .Map(static values => values.ToArray());
    private static Fin<BoundingBox[]> ValidateBounds<TGeometry>(
        ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        toSeq(items.ToArray())
            .TraverseM(static geometry => Optional(geometry)
                .ToFin(new Fault.MissingGeometry())
                .Bind(static candidate => candidate.IsValid
                    ? candidate.Bounds(op: Key).Bind(static box => box.IsValid ? Fin.Succ(box) : Fin.Fail<BoundingBox>(Key.InvalidInput()))
                    : Fin.Fail<BoundingBox>(Key.InvalidInput())))
            .As()
            .Map(static boxes => boxes.ToArray());
    private static Fin<Seq<Hit>> Search(RTree tree, BoundingBox shape) {
        Atom<Seq<int>> atom = Atom(value: Seq<int>());
        return tree.Search(
            box: shape,
            callback: (_, args) => atom.Swap(current => args.Id.Cons(current))) switch {
                true => Fin.Succ(SortedHits(ids: atom.Value)),
                false => Fin.Fail<Seq<Hit>>(Key.InvalidResult()),
            };
    }
    private static Fin<Seq<Hit>> Search(RTree tree, Sphere shape) {
        Atom<Seq<int>> atom = Atom(value: Seq<int>());
        return tree.Search(
            sphere: shape,
            callback: (_, args) => atom.Swap(current => args.Id.Cons(current))) switch {
                true => Fin.Succ(SortedHits(ids: atom.Value)),
                false => Fin.Fail<Seq<Hit>>(Key.InvalidResult()),
            };
    }
    private static Seq<Hit> SortedHits(Seq<int> ids) => toSeq(ids.Order().Select(static id => new Hit(Id: id)));
    private static Fin<Seq<Couple>> PointPairs(IEnumerable<int[]> values) => Optional(values)
            .ToFin(Key.InvalidResult())
            .Map(static rows => toSeq(rows
                .SelectMany(static (ids, needle) => ids.Select(source => new Couple(A: needle, B: source)))
                .OrderBy(static pair => pair.A)
                .ThenBy(static pair => pair.B)));
}
