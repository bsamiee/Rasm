using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [SPATIAL] ---------------------------------------------------------------------------------

public sealed class Tree : IDisposable {
    private readonly RTree tree;
    private bool disposed;
    private Tree(RTree tree) =>
        this.tree = tree;
    public static Validation<Error, Tree> Points(params ReadOnlySpan<Point3d> points) =>
        ValidatePoints(points: points)
            .Bind(static (Point3d[] values) => Optional(RTree.CreateFromPointArray(points: values) ?? new RTree())
                .ToFin(Query.TreeKey.InvalidResult()))
            .Map(static (RTree tree) => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> PointCloud(PointCloud cloud) =>
        Optional(cloud)
            .ToFin(ValidationFault.MissingGeometry())
            .Bind(static (PointCloud candidate) => candidate.IsValid switch {
                true => Optional(RTree.CreatePointCloudTree(cloud: candidate) ?? new RTree())
                    .ToFin(Query.TreeKey.InvalidResult()),
                false => Fin.Fail<RTree>(Query.TreeKey.InvalidInput()),
            })
            .Map(static (RTree tree) => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> Bounds<TGeometry>(
        params ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        ValidateBounds(items: items)
            .Bind(static (BoundingBox[] boxes) => boxes
                .Select(static (BoundingBox box, int index) => (Box: box, Index: index))
                .Aggregate(
                    seed: Fin.Succ(new RTree()),
                    func: static (Fin<RTree> current, (BoundingBox Box, int Index) item) => current.Bind((RTree tree) => tree.Insert(
                            box: item.Box,
                            elementId: item.Index) switch {
                                true => Fin.Succ(tree),
                                false => Fin.Fail<RTree>(Query.TreeKey.InvalidResult()),
                            })))
            .Map(static (RTree tree) => new Tree(tree: tree))
            .ToValidation();
    public static Validation<Error, Tree> MeshFaces(Mesh mesh) =>
        Optional(mesh)
            .ToFin(ValidationFault.MissingGeometry())
            .Bind(static (Mesh candidate) => candidate.IsValid switch {
                true => Optional(RTree.CreateMeshFaceTree(mesh: candidate) ?? new RTree())
                    .ToFin(Query.TreeKey.InvalidResult()),
                false => Fin.Fail<RTree>(Query.TreeKey.InvalidInput()),
            })
            .Map(static (RTree tree) => new Tree(tree: tree))
            .ToValidation();
    public Eff<Context, Seq<Hit>> Search(BoundingBox box) =>
        Ready()
            .Bind((RTree active) => box.IsValid switch {
                true => Search(
                    tree: active,
                    shape: box),
                false => Fin.Fail<Seq<Hit>>(Query.TreeKey.InvalidInput()),
            })
            .ToEff();
    public Eff<Context, Seq<Hit>> Search(Sphere sphere) =>
        Ready()
            .Bind((RTree active) => sphere.IsValid switch {
                true => Search(
                    tree: active,
                    shape: sphere),
                false => Fin.Fail<Seq<Hit>>(Query.TreeKey.InvalidInput()),
            })
            .ToEff();
    public Eff<Context, Seq<Couple>> Overlaps(Tree other, double tolerance = 0.0) =>
        (
            Ready(),
            Optional(other)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(static (Tree index) => index.Ready()),
            RhinoMath.IsValidDouble(x: tolerance) && tolerance >= 0.0
                ? Fin.Succ(tolerance)
                : Fin.Fail<double>(Query.TreeKey.InvalidInput())
        ).Apply(static (RTree left, RTree right, double modelTolerance) => (Left: left, Right: right, Tolerance: modelTolerance))
        .As()
        .Bind(static ((RTree Left, RTree Right, double Tolerance) state) => OverlapPairs(state: state))
        .ToEff();
    private static Fin<Seq<Couple>> OverlapPairs((RTree Left, RTree Right, double Tolerance) state) {
        Atom<Seq<Couple>> atom = Atom(value: Seq<Couple>());
        return RTree.SearchOverlaps(
            treeA: state.Left,
            treeB: state.Right,
            tolerance: state.Tolerance,
            callback: (object? _, RTreeEventArgs args) => atom.Swap((Seq<Couple> current) => current.Add(new Couple(A: args.Id, B: args.IdB)))) switch {
                true => Fin.Succ(SortedPairs(pairs: atom.Value)),
                false => Fin.Fail<Seq<Couple>>(Query.TreeKey.InvalidResult()),
            };
    }
    private static Seq<Couple> SortedPairs(Seq<Couple> pairs) =>
        toSeq(pairs.OrderBy(static (Couple p) => p.A).ThenBy(static (Couple p) => p.B));
    public static Eff<Context, Seq<Couple>> KNearest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        int count) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            count switch {
                > 0 => Fin.Succ(count),
                _ => Fin.Fail<int>(Query.TreeKey.InvalidInput()),
            }
        ).Apply(static (Point3d[] hay, Point3d[] query, int k) => (Hay: hay, Query: query, Count: k))
        .As()
        .Bind(static ((Point3d[] Hay, Point3d[] Query, int Count) state) => PointPairs(
            values: RTree.Point3dKNeighbors(
                hayPoints: state.Hay,
                needlePts: state.Query,
                amount: state.Count)))
        .ToEff();
    public static Eff<Context, Seq<Couple>> Closest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        double limitDistance) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            RhinoMath.IsValidDouble(x: limitDistance) && limitDistance > 0.0
                ? Fin.Succ(limitDistance)
                : Fin.Fail<double>(Query.TreeKey.InvalidInput())
        ).Apply(static (Point3d[] hay, Point3d[] query, double distance) => (Hay: hay, Query: query, Distance: distance))
        .As()
        .Bind(static ((Point3d[] Hay, Point3d[] Query, double Distance) state) => PointPairs(
            values: RTree.Point3dClosestPoints(
                hayPoints: state.Hay,
                needlePts: state.Query,
                limitDistance: state.Distance)))
        .ToEff();
    public void Dispose() =>
        disposed = disposed switch {
            false => fun((RTree t) => { t.Dispose(); return true; })(tree),
            true => true,
        };
    private Fin<RTree> Ready() =>
        disposed switch {
            true => Fin.Fail<RTree>(Query.TreeKey.InvalidInput()),
            false => Fin.Succ(tree),
        };
    private static Fin<Point3d[]> ValidatePoints(ReadOnlySpan<Point3d> points) =>
        toSeq(points.ToArray())
            .TraverseM(static (Point3d point) => point switch {
                Point3d candidate when candidate.IsValid => Fin.Succ(candidate),
                _ => Fin.Fail<Point3d>(Query.TreeKey.InvalidInput()),
            })
            .As()
            .Map(static (Seq<Point3d> values) => values.ToArray());
    private static Fin<BoundingBox[]> ValidateBounds<TGeometry>(
        ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        toSeq(items.ToArray())
            .TraverseM(static (TGeometry geometry) => Optional(geometry)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(static (TGeometry candidate) => (candidate.IsValid, candidate.GetBoundingBox(accurate: true)) switch {
                    (true, BoundingBox box) when box.IsValid => Fin.Succ(box),
                    _ => Fin.Fail<BoundingBox>(Query.TreeKey.InvalidInput()),
                }))
            .As()
            .Map(static (Seq<BoundingBox> boxes) => boxes.ToArray());
    private static Fin<Seq<Hit>> Search(RTree tree, BoundingBox shape) {
        Atom<Seq<int>> atom = Atom(value: Seq<int>());
        return tree.Search(
            box: shape,
            callback: (object? _, RTreeEventArgs args) => atom.Swap((Seq<int> current) => current.Add(args.Id))) switch {
                true => Fin.Succ(SortedHits(ids: atom.Value)),
                false => Fin.Fail<Seq<Hit>>(Query.TreeKey.InvalidResult()),
            };
    }
    private static Fin<Seq<Hit>> Search(RTree tree, Sphere shape) {
        Atom<Seq<int>> atom = Atom(value: Seq<int>());
        return tree.Search(
            sphere: shape,
            callback: (object? _, RTreeEventArgs args) => atom.Swap((Seq<int> current) => current.Add(args.Id))) switch {
                true => Fin.Succ(SortedHits(ids: atom.Value)),
                false => Fin.Fail<Seq<Hit>>(Query.TreeKey.InvalidResult()),
            };
    }
    private static Seq<Hit> SortedHits(Seq<int> ids) =>
        toSeq(ids.Order().Select(static (int id) => new Hit(Id: id)));
    private static Fin<Seq<Couple>> PointPairs(IEnumerable<int[]> values) =>
        Optional(values)
            .ToFin(Query.TreeKey.InvalidResult())
            .Map(static (IEnumerable<int[]> rows) => rows
                .SelectMany(static (int[] ids, int needle) => ids
                    .Select((int source) => new Couple(A: needle, B: source)))
                .OrderBy(static (Couple pair) => pair.A)
                .ThenBy(static (Couple pair) => pair.B)
                .Aggregate(
                    seed: Seq<Couple>(),
                    func: static (Seq<Couple> current, Couple pair) => current.Add(pair)));
}
