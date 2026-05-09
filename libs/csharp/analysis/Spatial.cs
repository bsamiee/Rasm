using Core;
using Core.Domain;
using Core.Runtime;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

// --- [SPATIAL] ---------------------------------------------------------------------------------

public sealed class SpatialIndex : IDisposable {
    private readonly RTree tree;
    private bool disposed;
    private SpatialIndex(RTree tree) =>
        this.tree = tree;
    public static Validation<Error, SpatialIndex> Points(params ReadOnlySpan<Point3d> points) =>
        ValidatePoints(points: points)
            .Bind(static (Point3d[] values) => Optional(RTree.CreateFromPointArray(points: values) ?? new RTree())
                .ToFin(Query.SpatialIndexKey.InvalidResult()))
            .Map(static (RTree tree) => new SpatialIndex(tree: tree))
            .ToValidation();
    public static Validation<Error, SpatialIndex> PointCloud(PointCloud cloud) =>
        Optional(cloud)
            .ToFin(ValidationFault.MissingGeometry())
            .Bind(static (PointCloud candidate) => candidate.IsValid switch {
                true => Optional(RTree.CreatePointCloudTree(cloud: candidate) ?? new RTree())
                    .ToFin(Query.SpatialIndexKey.InvalidResult()),
                false => Fin.Fail<RTree>(Query.SpatialIndexKey.InvalidInput()),
            })
            .Map(static (RTree tree) => new SpatialIndex(tree: tree))
            .ToValidation();
    public static Validation<Error, SpatialIndex> Bounds<TGeometry>(
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
                                false => Fin.Fail<RTree>(Query.SpatialIndexKey.InvalidResult()),
                            })))
            .Map(static (RTree tree) => new SpatialIndex(tree: tree))
            .ToValidation();
    public static Validation<Error, SpatialIndex> MeshFaces(Mesh mesh) =>
        Optional(mesh)
            .ToFin(ValidationFault.MissingGeometry())
            .Bind(static (Mesh candidate) => candidate.IsValid switch {
                true => Optional(RTree.CreateMeshFaceTree(mesh: candidate) ?? new RTree())
                    .ToFin(Query.SpatialIndexKey.InvalidResult()),
                false => Fin.Fail<RTree>(Query.SpatialIndexKey.InvalidInput()),
            })
            .Map(static (RTree tree) => new SpatialIndex(tree: tree))
            .ToValidation();
    public Eff<AnalysisRuntime, Seq<SpatialHit>> Search(BoundingBox box) =>
        Ready()
            .Bind((RTree active) => box.IsValid switch {
                true => Search(
                    tree: active,
                    shape: box),
                false => Fin.Fail<Seq<SpatialHit>>(Query.SpatialIndexKey.InvalidInput()),
            })
            .ToEff();
    public Eff<AnalysisRuntime, Seq<SpatialHit>> Search(Sphere sphere) =>
        Ready()
            .Bind((RTree active) => sphere.IsValid switch {
                true => Search(
                    tree: active,
                    shape: sphere),
                false => Fin.Fail<Seq<SpatialHit>>(Query.SpatialIndexKey.InvalidInput()),
            })
            .ToEff();
    public Eff<AnalysisRuntime, Seq<SpatialPair>> Overlaps(SpatialIndex other, double tolerance = 0.0) =>
        (
            Ready(),
            Optional(other)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(static (SpatialIndex index) => index.Ready()),
            RhinoMath.IsValidDouble(x: tolerance) && tolerance >= 0.0
                ? Fin.Succ(tolerance)
                : Fin.Fail<double>(Query.SpatialIndexKey.InvalidInput())
        ).Apply(static (RTree left, RTree right, double modelTolerance) => (Left: left, Right: right, Tolerance: modelTolerance))
        .As()
        .Bind(static ((RTree Left, RTree Right, double Tolerance) state) => OverlapPairs(state: state))
        .ToEff();
    private static Fin<Seq<SpatialPair>> OverlapPairs((RTree Left, RTree Right, double Tolerance) state) {
        SearchState search = new(capacity: state.Left.Count * state.Right.Count);
        return RTree.SearchOverlaps(
            treeA: state.Left,
            treeB: state.Right,
            tolerance: state.Tolerance,
            callback: CollectPair(search: search)) switch {
                true => Fin.Succ(search.Pairs()),
                false => Fin.Fail<Seq<SpatialPair>>(Query.SpatialIndexKey.InvalidResult()),
            };
    }
    public static Eff<AnalysisRuntime, Seq<SpatialPair>> KNearest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        int count) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            count switch {
                > 0 => Fin.Succ(count),
                _ => Fin.Fail<int>(Query.SpatialIndexKey.InvalidInput()),
            }
        ).Apply(static (Point3d[] hay, Point3d[] query, int k) => (Hay: hay, Query: query, Count: k))
        .As()
        .Bind(static ((Point3d[] Hay, Point3d[] Query, int Count) state) => PointPairs(
            values: RTree.Point3dKNeighbors(
                hayPoints: state.Hay,
                needlePts: state.Query,
                amount: state.Count)))
        .ToEff();
    public static Eff<AnalysisRuntime, Seq<SpatialPair>> Closest(
        ReadOnlySpan<Point3d> points,
        ReadOnlySpan<Point3d> needles,
        double limitDistance) =>
        (
            ValidatePoints(points: points),
            ValidatePoints(points: needles),
            RhinoMath.IsValidDouble(x: limitDistance) && limitDistance > 0.0
                ? Fin.Succ(limitDistance)
                : Fin.Fail<double>(Query.SpatialIndexKey.InvalidInput())
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
            false => DisposeTree(),
            true => true,
        };
    // BOUNDARY ADAPTER — IDisposable.Dispose() must call tree.Dispose() (void) and return a bool to drive
    // the assignment-expression form of the public Dispose member; the helper preserves the void→bool bridge.
    private bool DisposeTree() {
        tree.Dispose();
        return true;
    }
    private Fin<RTree> Ready() =>
        disposed switch {
            true => Fin.Fail<RTree>(Query.SpatialIndexKey.InvalidInput()),
            false => Fin.Succ(tree),
        };
    private static Fin<Point3d[]> ValidatePoints(ReadOnlySpan<Point3d> points) =>
        toSeq(points.ToArray())
            .Map(static (Point3d point) => point switch {
                Point3d candidate when candidate.IsValid => Fin.Succ(candidate),
                _ => Fin.Fail<Point3d>(Query.SpatialIndexKey.InvalidInput()),
            })
            .TraverseFin()
            .Map(static (Seq<Point3d> values) => values.ToArray());
    private static Fin<BoundingBox[]> ValidateBounds<TGeometry>(
        ReadOnlySpan<TGeometry> items) where TGeometry : GeometryBase =>
        toSeq(items.ToArray())
            .Map(static (TGeometry geometry) => Optional(geometry)
                .ToFin(ValidationFault.MissingGeometry())
                .Bind(static (TGeometry candidate) => (candidate.IsValid, candidate.GetBoundingBox(accurate: true)) switch {
                    (true, BoundingBox box) when box.IsValid => Fin.Succ(box),
                    _ => Fin.Fail<BoundingBox>(Query.SpatialIndexKey.InvalidInput()),
                }))
            .TraverseFin()
            .Map(static (Seq<BoundingBox> boxes) => boxes.ToArray());
    private static Fin<Seq<SpatialHit>> Search(RTree tree, BoundingBox shape) {
        SearchState search = new(capacity: tree.Count);
        return tree.Search(
            box: shape,
            callback: CollectHit(search: search)) switch {
                true => Fin.Succ(search.Hits()),
                false => Fin.Fail<Seq<SpatialHit>>(Query.SpatialIndexKey.InvalidResult()),
            };
    }
    private static Fin<Seq<SpatialHit>> Search(RTree tree, Sphere shape) {
        SearchState search = new(capacity: tree.Count);
        return tree.Search(
            sphere: shape,
            callback: CollectHit(search: search)) switch {
                true => Fin.Succ(search.Hits()),
                false => Fin.Fail<Seq<SpatialHit>>(Query.SpatialIndexKey.InvalidResult()),
            };
    }
    private static EventHandler<RTreeEventArgs> CollectHit(SearchState search) =>
        (object? _, RTreeEventArgs args) => search.Add(id: args.Id);
    private static EventHandler<RTreeEventArgs> CollectPair(SearchState search) =>
        (object? _, RTreeEventArgs args) => search.Add(a: args.Id, b: args.IdB);
    private static Fin<Seq<SpatialPair>> PointPairs(IEnumerable<int[]> values) =>
        Optional(values)
            .ToFin(Query.SpatialIndexKey.InvalidResult())
            .Map(static (IEnumerable<int[]> rows) => rows
                .SelectMany(static (int[] ids, int needle) => ids
                    .Select((int source) => new SpatialPair(A: needle, B: source)))
                .OrderBy(static (SpatialPair pair) => pair.A)
                .ThenBy(static (SpatialPair pair) => pair.B)
                .Aggregate(
                    seed: Seq<SpatialPair>(),
                    func: static (Seq<SpatialPair> current, SpatialPair pair) => current.Add(pair)));
    private sealed class SearchState {
        private readonly int[] ids;
        private readonly SpatialPair[] pairs;
        private int hitCount;
        private int pairCount;
        internal SearchState(int capacity) {
            ids = new int[capacity];
            pairs = new SpatialPair[capacity];
        }
        internal void Add(int id) =>
            hitCount = (hitCount < ids.Length) switch {
                true => AddHit(id: id, count: hitCount),
                false => hitCount,
            };
        internal void Add(int a, int b) =>
            pairCount = (pairCount < pairs.Length) switch {
                true => AddPair(pair: new SpatialPair(A: a, B: b), count: pairCount),
                false => pairCount,
            };
        internal Seq<SpatialHit> Hits() =>
            ids
                .Take(count: hitCount)
                .Order()
                .Select(static (int id) => new SpatialHit(Id: id))
                .Aggregate(
                    seed: Seq<SpatialHit>(),
                    func: static (Seq<SpatialHit> current, SpatialHit hit) => current.Add(hit));
        internal Seq<SpatialPair> Pairs() =>
            pairs
                .Take(count: pairCount)
                .OrderBy(static (SpatialPair pair) => pair.A)
                .ThenBy(static (SpatialPair pair) => pair.B)
                .Aggregate(
                    seed: Seq<SpatialPair>(),
                    func: static (Seq<SpatialPair> current, SpatialPair pair) => current.Add(pair));
        private int AddHit(int id, int count) {
            ids[count] = id;
            return count + 1;
        }
        private int AddPair(SpatialPair pair, int count) {
            pairs[count] = pair;
            return count + 1;
        }
    }
}
