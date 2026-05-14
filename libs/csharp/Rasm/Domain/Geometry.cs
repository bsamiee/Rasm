using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum Primitive { None, Line, Polyline, Circle, Arc, Ellipse, Plane, Sphere, Cylinder, Cone, Torus, Box, BoundingBox }
public enum Closure { Unknown, Open, Closed }
public enum Solidity { Unknown, Open, Solid }
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
public enum SolidOrientation { Unknown = 0, Outward = 1, Inward = -1 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, Iso, Silhouette, SubCurve, Draft }
[BoundaryAdapter]
public interface ITopologyProjection {
    public ComponentIndex Source { get; }
    public Unit Dispose();
    public bool SameAs(ITopologyProjection other);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurveProjection(Curve Curve, CurveFeature Feature, ComponentIndex Source) : ITopologyProjection {
    internal CurveProjection(Curve curve, CurveFeature feature, ComponentIndexType type, int index) : this(Curve: curve, Feature: feature, Source: new ComponentIndex(type: type, index: index)) { }
    public Unit Dispose() { Curve.Dispose(); return Unit.Default; }
    public bool SameAs(ITopologyProjection other) => other is CurveProjection c && ReferenceEquals(objA: Curve, objB: c.Curve);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveSelector(CurveFeature Feature, Option<Vector3d> Direction = default, Option<double> Angle = default, Option<int> Index = default, Option<double> Normalized = default, Option<IsoStatus> Iso = default);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FaceProjection : ITopologyProjection {
    private FaceProjection(Brep brep, int faceIndex, bool reversed) { Brep = brep; FaceIndex = faceIndex; Reversed = reversed; }
    public Brep Brep { get; }
    public int FaceIndex { get; }
    public bool Reversed { get; }
    public ComponentIndex Source => new(type: ComponentIndexType.BrepFace, index: FaceIndex);
    public static FaceProjection From(BrepFace face) { ArgumentNullException.ThrowIfNull(argument: face); return new(brep: face.DuplicateFace(duplicateMeshes: false), faceIndex: face.FaceIndex, reversed: face.OrientationIsReversed); }
    public Unit Dispose() { Brep.Dispose(); return Unit.Default; }
    public bool SameAs(ITopologyProjection other) => other is FaceProjection f && ReferenceEquals(objA: Brep, objB: f.Brep);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceProjection : ITopologyProjection {
    private static readonly Op Key = Op.Of(name: nameof(MeshFaceProjection));
    private MeshFaceProjection(Mesh mesh, int face) { Mesh = mesh; Face = face; }
    public Mesh Mesh { get; }
    public int Face { get; }
    public ComponentIndex Source => new(type: ComponentIndexType.MeshFace, index: Face);
    public Unit Dispose() => Unit.Default;
    public bool SameAs(ITopologyProjection other) => other is MeshFaceProjection m && ReferenceEquals(objA: Mesh, objB: m.Mesh) && Face == m.Face;
    public static Fin<MeshFaceProjection> Create(Mesh? mesh, int face) =>
        Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => (native.Faces.Count, face) switch {
            ( <= 0, _) => Fin.Fail<MeshFaceProjection>(Key.InvalidResult()),
            (int count, int index) when index >= 0 && index < count => Fin.Succ(new MeshFaceProjection(mesh: native, face: index)),
            _ => Fin.Fail<MeshFaceProjection>(Key.InvalidInput()),
        });
    public Fin<Seq<Point3d>> Vertices =>
        Create(mesh: Mesh, face: Face).Bind(projection => projection.Mesh.Faces.GetFaceVertices(faceIndex: projection.Face, a: out Point3f a, b: out Point3f b, c: out Point3f c, d: out Point3f d) switch {
            true when projection.Mesh.Faces[projection.Face].IsQuad => Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d),
            true => Seq((Point3d)a, (Point3d)b, (Point3d)c),
            false => Fin.Fail<Seq<Point3d>>(Key.InvalidResult()),
        });
    public Fin<Vector3d> Normal => Isolated().Bind(static mesh => {
        Fin<Vector3d> result = (mesh.FaceNormals.ComputeFaceNormals(), mesh.FaceNormals.UnitizeFaceNormals(), mesh.FaceNormals.Count) switch {
            (true, true, > 0) => Fin.Succ((Vector3d)mesh.FaceNormals[0]),
            _ => Fin.Fail<Vector3d>(Key.InvalidResult()),
        };
        mesh.Dispose();
        return result;
    });
    public Fin<Point3d> Center => Create(mesh: Mesh, face: Face).Bind(static projection => projection.Mesh.Faces.GetFaceCenter(faceIndex: projection.Face) switch {
        Point3d point when point.IsValid => Fin.Succ(point),
        _ => Fin.Fail<Point3d>(Key.InvalidResult()),
    });
    public Fin<Mesh> Isolated() => Create(mesh: Mesh, face: Face).Bind(projection => {
        Mesh? result = Rhino.Geometry.Mesh.CreateFromFilteredFaceList(original: projection.Mesh, inclusion: Enumerable.Range(start: 0, count: projection.Mesh.Faces.Count).Select(index => index == projection.Face));
        try {
            Fin<Mesh> output = (result is { IsValid: true }, result?.FaceNormals.ComputeFaceNormals(), result?.FaceNormals.UnitizeFaceNormals()) switch {
                (true, true, true) => Fin.Succ(result),
                _ => Fin.Fail<Mesh>(Key.InvalidResult()),
            };
            result = output.IsSucc ? null : result;
            return output;
        } finally { result?.Dispose(); }
    });
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IntersectionHit(Option<Curve> Curve, Option<Point3d> Point, Option<Point3d> PointB, Option<Interval> OverlapA, Option<Interval> OverlapB, IntersectionKind Kind) {
    public static IntersectionHit At(Point3d point) => new(Curve: Option<Curve>.None, Point: Some(point), PointB: Option<Point3d>.None, OverlapA: Option<Interval>.None, OverlapB: Option<Interval>.None, Kind: IntersectionKind.Point);
    public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new(Curve: Some(curve), Point: Option<Point3d>.None, PointB: Option<Point3d>.None, OverlapA: Option<Interval>.None, OverlapB: Option<Interval>.None, Kind: kind);
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB) => new(Curve: Option<Curve>.None, Point: Some(start), PointB: Some(end), OverlapA: Some(overlapA), OverlapB: Some(overlapB), Kind: IntersectionKind.Overlap);
    public static IntersectionHit Tag(IntersectionKind kind) => new(Curve: Option<Curve>.None, Point: Option<Point3d>.None, PointB: Option<Point3d>.None, OverlapA: Option<Interval>.None, OverlapB: Option<Interval>.None, Kind: kind);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance);

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point = new(key: 0, type: typeof(Point3d), topology: Topology.Point, primitive: Primitive.None, nominalClosure: Closure.Open, nominalSolidity: Solidity.Open);
    public static readonly Kind Line = new(key: 1, type: typeof(Line), topology: Topology.Curve, primitive: Primitive.Line, nominalClosure: Closure.Open, nominalSolidity: Solidity.Open);
    public static readonly Kind Polyline = new(key: 2, type: typeof(Polyline), topology: Topology.Curve, primitive: Primitive.Polyline, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Open);
    public static readonly Kind Circle = new(key: 3, type: typeof(Circle), topology: Topology.Curve, primitive: Primitive.Circle, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Open);
    public static readonly Kind Arc = new(key: 4, type: typeof(Arc), topology: Topology.Curve, primitive: Primitive.Arc, nominalClosure: Closure.Open, nominalSolidity: Solidity.Open);
    public static readonly Kind Ellipse = new(key: 5, type: typeof(Ellipse), topology: Topology.Curve, primitive: Primitive.Ellipse, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Open);
    public static readonly Kind Curve = new(key: 6, type: typeof(Curve), topology: Topology.Curve, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Open);
    public static readonly Kind Surface = new(key: 7, type: typeof(Surface), topology: Topology.Surface, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Open);
    public static readonly Kind Plane = new(key: 8, type: typeof(Plane), topology: Topology.Surface, primitive: Primitive.Plane, nominalClosure: Closure.Open, nominalSolidity: Solidity.Open);
    public static readonly Kind Sphere = new(key: 9, type: typeof(Sphere), topology: Topology.Surface, primitive: Primitive.Sphere, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Solid);
    public static readonly Kind Cylinder = new(key: 10, type: typeof(Cylinder), topology: Topology.Surface, primitive: Primitive.Cylinder, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind Cone = new(key: 11, type: typeof(Cone), topology: Topology.Surface, primitive: Primitive.Cone, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind Torus = new(key: 12, type: typeof(Torus), topology: Topology.Surface, primitive: Primitive.Torus, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Solid);
    public static readonly Kind Brep = new(key: 13, type: typeof(Brep), topology: Topology.Brep, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind Box = new(key: 14, type: typeof(Box), topology: Topology.Brep, primitive: Primitive.Box, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Solid);
    public static readonly Kind BBox = new(key: 15, type: typeof(BoundingBox), topology: Topology.Brep, primitive: Primitive.BoundingBox, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Solid);
    public static readonly Kind Mesh = new(key: 16, type: typeof(Mesh), topology: Topology.Mesh, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind SubD = new(key: 17, type: typeof(SubD), topology: Topology.SubD, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind Cloud = new(key: 18, type: typeof(PointCloud), topology: Topology.PointCloud, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Open);
    public static readonly Kind Extrusion = new(key: 19, type: typeof(Extrusion), topology: Topology.Extrusion, primitive: Primitive.None, nominalClosure: Closure.Unknown, nominalSolidity: Solidity.Unknown);
    public static readonly Kind Hatch = new(key: 20, type: typeof(Hatch), topology: Topology.Hatch, primitive: Primitive.None, nominalClosure: Closure.Closed, nominalSolidity: Solidity.Open);
    public Type Type { get; }
    public Topology Topology { get; }
    public Primitive Primitive { get; }
    public Closure NominalClosure { get; }
    public Solidity NominalSolidity { get; }
}
[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MassKind {
    public static readonly MassKind None = new(key: 0, label: nameof(None), requirement: Requirement.None,
        aggregate: static (_, _, _, _, _, _) => Fin.Fail<IDisposable>(new Fault.ComputationFailed(Label: nameof(None))));
    public static readonly MassKind Length = new(key: 1, label: nameof(Length), requirement: Requirement.CurveLength,
        aggregate: LengthAggregate);
    public static readonly MassKind Area = new(key: 2, label: nameof(Area), requirement: Requirement.AreaMass,
        aggregate: static (geometry, ctx, firstMoments, secondMoments, productMoments, op) => WeightedAggregate<AreaMassProperties>(
            geometry: geometry, ctx: ctx, mass: Area!, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op,
            sum: static (summands, weights) => AreaMassProperties.WeightedSum(summands: summands, weights: weights)));
    public static readonly MassKind Volume = new(key: 3, label: nameof(Volume), requirement: Requirement.VolumeMass,
        aggregate: static (geometry, ctx, firstMoments, secondMoments, productMoments, op) => WeightedAggregate<VolumeMassProperties>(
            geometry: geometry, ctx: ctx, mass: Volume!, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op,
            sum: static (summands, weights) => VolumeMassProperties.WeightedSum(summands: summands, weights: weights)));
    public string Label { get; }
    internal Requirement Requirement { get; }
    internal Func<IEnumerable<GeometryBase>, Context, bool, bool, bool, Op, Fin<IDisposable>> Aggregate { get; }
    private static Fin<IDisposable> LengthAggregate(IEnumerable<GeometryBase> geometry, Context ctx, bool firstMoments, bool secondMoments, bool productMoments, Op op) =>
        toSeq(geometry) switch {
            Seq<GeometryBase> items when items.ForAll(static item => item is Curve) => Optional(LengthMassProperties.Compute(
                    curves: items.AsIterable().Cast<Curve>(), length: true, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments))
                .ToFin(Fail: new Fault.ComputationFailed(Label: nameof(LengthMassProperties))).Map(static p => (IDisposable)p),
            Seq<GeometryBase> items => WeightedAggregate<LengthMassProperties>(
                geometry: items.AsIterable(), ctx: ctx, mass: Length, firstMoments: firstMoments, secondMoments: secondMoments, productMoments: productMoments, op: op,
                sum: static (summands, weights) => LengthMassProperties.WeightedSum(summands: summands, weights: weights)),
        };
    private static Fin<IDisposable> WeightedAggregate<TMass>(
        IEnumerable<GeometryBase> geometry,
        Context ctx,
        MassKind mass,
        bool firstMoments,
        bool secondMoments,
        bool productMoments,
        Op op,
        Func<IEnumerable<TMass>, IEnumerable<double>, TMass?> sum) where TMass : class, IDisposable =>
        toSeq(geometry).Fold(
            initialState: Fin.Succ(Seq<IDisposable>()),
            f: (state, item) => state.Bind(owned =>
                Dispatch.ResolveTagged(table: Dispatch.MassPropertiesTable, source: item, tag: mass, args: (ctx, firstMoments, secondMoments, productMoments), op: op)
                    .Match(
                        Succ: resource => Fin.Succ(resource.Cons(owned)),
                        Fail: error => (owned.Iter(static r => r.Dispose()), Fin.Fail<Seq<IDisposable>>(error)).Item2)))
            .Bind(owned => {
                Fin<IDisposable> result = Optional(sum(
                        owned.AsIterable().Cast<TMass>(),
                        Enumerable.Repeat(element: 1.0, count: owned.Count)))
                    .ToFin(Fail: new Fault.ComputationFailed(Label: typeof(TMass).Name))
                    .Map(static p => (IDisposable)p);
                _ = owned.Iter(static r => r.Dispose());
                return result;
            });
}

// --- [CONSTANTS] --------------------------------------------------------------------------
// FrozenDictionary on Type keys: LanguageExt v5 Map/HashMap<Type,_> trips a Rhino reflection enumeration bug.
[BoundaryAdapter]
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = Kind.Items.ToFrozenDictionary(keySelector: static k => k.Type);
    internal static Option<Kind> For(Type type) => type == typeof(Point)
        ? Some(Kind.Point)
        : Optional(ByType.GetValueOrDefault(key: type)) | (InheritsBase(type: type) is Type bt ? Optional(ByType.GetValueOrDefault(key: bt)) : Option<Kind>.None);
    internal static Type? InheritsBase(Type type) => type.BaseType is Type b ? (ByType.ContainsKey(key: b) ? b : InheritsBase(type: b)) : null;
}
[BoundaryAdapter]
internal static class Dispatch {
    internal static readonly FrozenDictionary<(Type Source, Type Target), Func<object, (Context Ctx, Op Op), Fin<object>>> CoercionTable = new Dictionary<(Type, Type), Func<object, (Context, Op), Fin<object>>> {
        [(typeof(Curve), typeof(Line))] = static (g, a) => ((Curve)g).IsLinear(tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(new Line(from: ((Curve)g).PointAtStart, to: ((Curve)g).PointAtEnd)) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Curve), typeof(Polyline))] = static (g, a) => ((Curve)g).TryGetPolyline(polyline: out Polyline poly) ? Fin.Succ<object>(poly) : Fin.Fail<object>(error: a.Item2.InvalidResult()),
        [(typeof(Curve), typeof(Circle))] = static (g, a) => ((Curve)g).TryGetCircle(circle: out Circle c, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(c) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Curve), typeof(Arc))] = static (g, a) => ((Curve)g).TryGetArc(arc: out Arc r, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(r) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Curve), typeof(Ellipse))] = static (g, a) => ((Curve)g).TryGetEllipse(ellipse: out Ellipse e, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(e) : Fin.Fail<object>(error: a.Item2.InvalidResult()),
        [(typeof(Surface), typeof(Plane))] = static (g, a) => ((Surface)g).TryGetPlane(plane: out Plane p, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(p) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Surface), typeof(Sphere))] = static (g, a) => ((Surface)g).TryGetSphere(sphere: out Sphere s, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(s) : Fin.Fail<object>(error: a.Item2.InvalidResult()),
        [(typeof(Surface), typeof(Cylinder))] = static (g, a) => ((Surface)g).TryGetCylinder(cylinder: out Cylinder c, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(c) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Surface), typeof(Cone))] = static (g, a) => ((Surface)g).TryGetCone(cone: out Cone c, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(c) : Fin.Fail<object>(error: a.Item2.InvalidResult()), [(typeof(Surface), typeof(Torus))] = static (g, a) => ((Surface)g).TryGetTorus(torus: out Torus t, tolerance: a.Item1.Absolute.Value) ? Fin.Succ<object>(t) : Fin.Fail<object>(error: a.Item2.InvalidResult()),
        [(typeof(Brep), typeof(Box))] = static (g, a) => ((Brep)g).IsBox(tolerance: a.Item1.Absolute.Value) && ((Brep)g).Faces[0].UnderlyingSurface().TryGetPlane(plane: out Plane plane, tolerance: a.Item1.Absolute.Value) && ((Brep)g).GetBoundingBox(plane: plane, worldBox: out Box box) is { IsValid: true } ? Fin.Succ<object>(box) : Fin.Fail<object>(error: a.Item2.InvalidResult()),
    }.ToFrozenDictionary();
    internal static Fin<TTarget> Coerce<TTarget>(object? source, Context ctx, Op op) => Optional(source)
        .Bind(s => LookupPair(table: CoercionTable, left: s.GetType(), right: typeof(TTarget)).Map(fn => (Source: s, Fn: fn)))
        .Match(
            Some: pair => pair.Fn(arg1: pair.Source, arg2: (ctx, op)).Map(static v => (TTarget)v),
            None: () => Fin.Fail<TTarget>(error: source is null ? op.InvalidInput() : op.Unsupported(geometryType: source.GetType(), outputType: typeof(TTarget))));
    internal static bool SupportsCoercion(Type source, Type target) => LookupPair(table: CoercionTable, left: source, right: target).IsSome;
    internal static readonly FrozenDictionary<Type, Func<object, Op, Fin<BoundingBox>>> BoundsTable = new Dictionary<Type, Func<object, Op, Fin<BoundingBox>>> {
        [typeof(BoundingBox)] = static (g, op) => ((BoundingBox)g).IsValid ? Fin.Succ((BoundingBox)g) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Box)] = static (g, op) => ((Box)g).IsValid ? Fin.Succ(((Box)g).BoundingBox) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Sphere)] = static (g, op) => ((Sphere)g).IsValid ? Fin.Succ(((Sphere)g).BoundingBox) : Fin.Fail<BoundingBox>(error: op.InvalidInput()),
        [typeof(Plane)] = static (_, op) => Fin.Fail<BoundingBox>(error: op.Unsupported(geometryType: typeof(Plane), outputType: typeof(BoundingBox))), [typeof(Line)] = static (g, op) => ((Line)g).IsValid ? Fin.Succ(((Line)g).BoundingBox) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Polyline)] = static (g, _) => Fin.Succ(((Polyline)g).BoundingBox),
        [typeof(Point3d)] = static (g, op) => ((Point3d)g).IsValid ? Fin.Succ(new BoundingBox(min: (Point3d)g, max: (Point3d)g)) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Circle)] = static (g, _) => Fin.Succ(((Circle)g).BoundingBox),
        [typeof(Arc)] = static (g, op) => ((Arc)g).IsValid ? Fin.Succ(((Arc)g).BoundingBox()) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Ellipse)] = static (g, op) => Optional(((Ellipse)g).ToNurbsCurve()).ToFin(Fail: op.InvalidResult()).Map(static c => { using NurbsCurve d = c; return d.GetBoundingBox(accurate: true); }),
        [typeof(Cylinder)] = static (g, op) => ((Cylinder)g).IsValid ? Optional(((Cylinder)g).ToBrep(capBottom: true, capTop: true)).ToFin(Fail: op.InvalidResult()).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(Cone)] = static (g, op) => ((Cone)g).IsValid ? Optional(((Cone)g).ToBrep(capBottom: true)).ToFin(Fail: op.InvalidResult()).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }) : Fin.Fail<BoundingBox>(error: op.InvalidInput()),
        [typeof(Torus)] = static (g, op) => ((Torus)g).IsValid ? Optional(((Torus)g).ToBrep()).ToFin(Fail: op.InvalidResult()).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }) : Fin.Fail<BoundingBox>(error: op.InvalidInput()), [typeof(GeometryBase)] = static (g, op) => g is GeometryBase { IsValid: true } gb ? Fin.Succ(gb.GetBoundingBox(accurate: true)) : Fin.Fail<BoundingBox>(error: op.InvalidInput()),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (Context Ctx, Op Op), Fin<Seq<Point3d>>>> VerticesTable = new Dictionary<Type, Func<object, (Context, Op), Fin<Seq<Point3d>>>> {
        [typeof(Point3d)] = static (g, _) => Fin.Succ(Seq((Point3d)g)), [typeof(Point)] = static (g, _) => Fin.Succ(Seq(((Point)g).Location)), [typeof(Line)] = static (g, _) => Fin.Succ(Seq(((Line)g).From, ((Line)g).To)), [typeof(Polyline)] = static (g, _) => Fin.Succ(toSeq((Polyline)g)),
        [typeof(BoundingBox)] = static (g, _) => Fin.Succ(toSeq(((BoundingBox)g).GetCorners())), [typeof(Box)] = static (g, _) => Fin.Succ(toSeq(((Box)g).GetCorners())), [typeof(Curve)] = static (g, _) => Fin.Succ(((Curve)g).TryGetPolyline(polyline: out Polyline poly) ? toSeq(poly) : Seq(((Curve)g).PointAtStart, ((Curve)g).PointAtEnd)),
        [typeof(Brep)] = static (g, _) => Fin.Succ(toSeq(((Brep)g).DuplicateVertices())), [typeof(Mesh)] = static (g, _) => Fin.Succ(toSeq(((Mesh)g).Vertices.ToPoint3dArray())), [typeof(PointCloud)] = static (g, _) => Fin.Succ(toSeq(((PointCloud)g).GetPoints())),
        [typeof(SubD)] = static (g, _) => Fin.Succ(toSeq(LanguageExt.List.unfold(state: (SubDVertex?)((SubD)g).Vertices.First, unfolder: static v => v switch { SubDVertex sv => Some((sv.ControlNetPoint, (SubDVertex?)sv.Next)), _ => None }))),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (Context Ctx, Op Op), Fin<Point3d>>> CentroidTable = new Dictionary<Type, Func<object, (Context, Op), Fin<Point3d>>> {
        [typeof(Point3d)] = static (g, _) => Fin.Succ((Point3d)g), [typeof(Point)] = static (g, _) => Fin.Succ(((Point)g).Location), [typeof(Line)] = static (g, _) => Fin.Succ(((Line)g).PointAt(t: 0.5)), [typeof(Polyline)] = static (g, _) => Fin.Succ(((Polyline)g).CenterPoint()), [typeof(BoundingBox)] = static (g, _) => Fin.Succ(((BoundingBox)g).Center), [typeof(Box)] = static (g, _) => Fin.Succ(((Box)g).Center),
        [typeof(Curve)] = static (g, args) => (((Curve)g).IsClosed, ((Curve)g).TryGetPlane(plane: out Plane _, tolerance: args.Item1.Absolute.Value)) switch { (true, true) => Optional(AreaMassProperties.Compute(closedPlanarCurve: (Curve)g, planarTolerance: args.Item1.Absolute.Value)).ToFin(Fail: args.Item2.InvalidResult()).Map(static m => { using AreaMassProperties d = m; return d.Centroid; }), _ => Optional(LengthMassProperties.Compute(curve: (Curve)g)).ToFin(Fail: args.Item2.InvalidResult()).Map(static m => { using LengthMassProperties d = m; return d.Centroid; }) },
        [typeof(Brep)] = static (g, args) => MassCentroid(source: g, isSolid: ((Brep)g).IsSolid, ctx: args.Item1, op: args.Item2), [typeof(Mesh)] = static (g, args) => MassCentroid(source: g, isSolid: ((Mesh)g).IsSolid, ctx: args.Item1, op: args.Item2), [typeof(Surface)] = static (g, args) => MassCentroid(source: g, isSolid: ((Surface)g).IsSolid, ctx: args.Item1, op: args.Item2),
        [typeof(SubD)] = static (g, args) => Optional(((SubD)g).ToBrep(options: SubDToBrepOptions.Default)).ToFin(Fail: args.Item2.InvalidResult()).Bind(brep => { using Brep d = brep; return MassCentroid(source: d, isSolid: d.IsSolid, ctx: args.Item1, op: args.Item2); }),
    }.ToFrozenDictionary();
    private static Fin<Point3d> MassCentroid(object source, bool isSolid, Context ctx, Op op) =>
        ResolveTagged(table: MassPropertiesTable, source: source, tag: isSolid ? MassKind.Volume : MassKind.Area, args: (ctx, true, false, false), op: op)
            .Bind(disposable => { using IDisposable owned = disposable; return owned switch { LengthMassProperties l => Fin.Succ(l.Centroid), AreaMassProperties a => Fin.Succ(a.Centroid), VolumeMassProperties v => Fin.Succ(v.Centroid), _ => Fin.Fail<Point3d>(op.InvalidResult()) }; });
    internal static readonly FrozenDictionary<Type, Func<object, (Point3d Target, Context Ctx, Op Op), Fin<ClosestHit>>> ClosestTable = new Dictionary<Type, Func<object, (Point3d, Context, Op), Fin<ClosestHit>>> {
        [typeof(Line)] = static (g, args) => Fin.Succ(new ClosestHit(Point: ((Line)g).ClosestPoint(testPoint: args.Item1, limitToFiniteSegment: true), Distance: None, Normal: None, Component: None, MeshPoint: None)), [typeof(Polyline)] = static (g, args) => Fin.Succ(new ClosestHit(Point: ((Polyline)g).ClosestPoint(testPoint: args.Item1), Distance: None, Normal: None, Component: None, MeshPoint: None)),
        [typeof(Curve)] = static (g, args) => ((Curve)g).ClosestPoint(testPoint: args.Item1, t: out double p) ? Fin.Succ(new ClosestHit(Point: ((Curve)g).PointAt(t: p), Distance: Some(args.Item1.DistanceTo(other: ((Curve)g).PointAt(t: p))), Normal: None, Component: None, MeshPoint: None)) : Fin.Fail<ClosestHit>(error: args.Item3.InvalidResult()), [typeof(Surface)] = static (g, args) => ((Surface)g).ClosestPoint(testPoint: args.Item1, u: out double u, v: out double v) ? Fin.Succ(new ClosestHit(Point: ((Surface)g).PointAt(u: u, v: v), Distance: Some(args.Item1.DistanceTo(other: ((Surface)g).PointAt(u: u, v: v))), Normal: Some(((Surface)g).NormalAt(u: u, v: v)), Component: None, MeshPoint: None)) : Fin.Fail<ClosestHit>(error: args.Item3.InvalidResult()),
        [typeof(Brep)] = static (g, args) => ((Brep)g).ClosestPoint(testPoint: args.Item1, closestPoint: out Point3d pt, ci: out ComponentIndex ci, s: out double _, t: out double _, maximumDistance: 0.0, normal: out Vector3d normal) ? Fin.Succ(new ClosestHit(Point: pt, Distance: Some(args.Item1.DistanceTo(other: pt)), Normal: Some(normal), Component: Some(ci), MeshPoint: None)) : Fin.Fail<ClosestHit>(error: args.Item3.InvalidResult()), [typeof(Mesh)] = static (g, args) => Optional(((Mesh)g).ClosestMeshPoint(testPoint: args.Item1, maximumDistance: 0.0)).ToFin(Fail: args.Item3.InvalidResult()).Map(mp => new ClosestHit(Point: mp.Point, Distance: Some(args.Item1.DistanceTo(other: mp.Point)), Normal: Some(((Mesh)g).NormalAt(meshPoint: mp)), Component: None, MeshPoint: Some(mp))),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, Op, Fin<Seq<GeometryBase>>>> ComponentsTable = new Dictionary<Type, Func<object, Op, Fin<Seq<GeometryBase>>>> {
        [typeof(Brep)] = static (g, _) => Fin.Succ(toSeq((((Brep)g).GetConnectedComponents() switch { Brep[] arr when arr.Length > 0 => arr, _ => Brep.SplitDisjointPieces(brep: (Brep)g) }).Cast<GeometryBase>())),
        [typeof(Mesh)] = static (g, _) => Fin.Succ(toSeq(((Mesh)g).SplitDisjointPieces().Cast<GeometryBase>())),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, Op, Fin<Seq<Interval>>>> DomainsTable = new Dictionary<Type, Func<object, Op, Fin<Seq<Interval>>>> {
        [typeof(Curve)] = static (g, _) => Fin.Succ(Seq(((Curve)g).Domain)),
        [typeof(Surface)] = static (g, _) => Fin.Succ(Seq(((Surface)g).Domain(direction: 0), ((Surface)g).Domain(direction: 1))),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (IsoStatus Iso, double N, Op Op), Fin<Seq<Curve>>>> IsoCurvesTable = new Dictionary<Type, Func<object, (IsoStatus, double, Op), Fin<Seq<Curve>>>> {
        [typeof(Surface)] = static (g, args) => IsoSeq(surface: (Surface)g, iso: args.Item1, normalized: args.Item2, op: args.Item3),
        [typeof(Brep)] = static (g, args) => toSeq(((Brep)g).Faces).TraverseM(face => IsoSeq(surface: face, iso: args.Item1, normalized: args.Item2, op: args.Item3).Map(static seq => (IEnumerable<Curve>)seq)).As().Map(static nested => nested.Bind(static cs => toSeq(cs))),
    }.ToFrozenDictionary();
    private static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso: iso)).ToFin(Fail: op.InvalidResult()).Map(static c => Seq(c)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(direction: iso == IsoStatus.X ? 0 : 1) is { IsValid: true } d => surface is BrepFace face ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(direction: iso == IsoStatus.X ? 0 : 1, constantParameter: d.ParameterAt(normalizedParameter: normalized)))) : Optional(surface.IsoCurve(iso, d.ParameterAt(normalizedParameter: normalized))).ToFin(Fail: op.InvalidResult()).Map(static c => Seq(c)),
        _ => Fin.Fail<Seq<Curve>>(error: op.InvalidInput()),
    };
    internal static readonly FrozenDictionary<Type, Func<object, Op, Fin<Seq<Point3d>>>> ControlPointsTable = new Dictionary<Type, Func<object, Op, Fin<Seq<Point3d>>>> {
        [typeof(Curve)] = static (g, op) => ((Curve)g) is NurbsCurve nc ? Fin.Succ(toSeq(Enumerable.Range(start: 0, count: nc.Points.Count).Select(i => nc.Points[i].Location))) : Optional(((Curve)g).ToNurbsCurve()).ToFin(Fail: op.InvalidResult()).Map(static c => { using NurbsCurve d = c; return toSeq(Enumerable.Range(start: 0, count: d.Points.Count).Select(i => d.Points[i].Location)); }),
        [typeof(Surface)] = static (g, op) => ((Surface)g) is NurbsSurface ns ? Fin.Succ(toSeq(Enumerable.Range(start: 0, count: ns.Points.CountU).SelectMany(u => Enumerable.Range(start: 0, count: ns.Points.CountV).Select(v => ns.Points.GetControlPoint(u: u, v: v).Location)))) : Optional(((Surface)g).ToNurbsSurface()).ToFin(Fail: op.InvalidResult()).Map(static s => { using NurbsSurface d = s; return toSeq(Enumerable.Range(start: 0, count: d.Points.CountU).SelectMany(u => Enumerable.Range(start: 0, count: d.Points.CountV).Select(v => d.Points.GetControlPoint(u: u, v: v).Location))); }),
        [typeof(Brep)] = static (g, op) => toSeq(((Brep)g).Faces).TraverseM(face => Optional(face.ToNurbsSurface()).ToFin(Fail: op.InvalidResult()).Map(static s => { using NurbsSurface d = s; return toSeq(Enumerable.Range(start: 0, count: d.Points.CountU).SelectMany(u => Enumerable.Range(start: 0, count: d.Points.CountV).Select(v => d.Points.GetControlPoint(u: u, v: v).Location))); })).As().Map(static nested => nested.Bind(static pts => pts)),
    }.ToFrozenDictionary();
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> CurveOrPrimitiveInput = static (g, args) =>
        (g is Curve c ? c : g switch {
            Line l => (Curve?)new LineCurve(line: l),
            Polyline p => p.ToPolylineCurve(),
            Circle ci => new ArcCurve(circle: ci),
            Arc a => new ArcCurve(arc: a),
            _ => null,
        }) switch {
            Curve native => ((args.Item1.Feature is CurveFeature.Segment or CurveFeature.SubCurve)
                ? Optional(args.Item1.Feature == CurveFeature.SubCurve ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                    Option<Curve[]> opt when opt.Case is Curve[] arr && arr.Length > 0 =>
                        Fin.Succ(toSeq(arr.Select((cc, i) => new CurveProjection(curve: cc, feature: args.Item1.Feature, type: ComponentIndexType.PolycurveSegment, index: i)))),
                    _ => Optional(native.DuplicateCurve()).ToFin(Fail: args.Item3.InvalidResult())
                        .Map(d => Seq(new CurveProjection(curve: d, feature: args.Item1.Feature, type: ComponentIndexType.PolycurveSegment, index: 0))),
                }
                : Optional(native.DuplicateCurve()).ToFin(Fail: args.Item3.InvalidResult())
                    .Map(d => Seq(new CurveProjection(curve: d, feature: args.Item1.Feature, type: ComponentIndexType.NoType, index: 0))))
            .Map(seq => (g is not Curve) switch {
                true => fun(static (Curve n, Seq<CurveProjection> s) => { n.Dispose(); return s; })(native, seq),
                false => seq,
            }),
            _ => Fin.Fail<Seq<CurveProjection>>(error: args.Item3.InvalidResult()),
        };
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> BrepEdgeHandler = static (g, args) => Fin.Succ(toSeq(((Brep)g).Edges).Where(e => (args.Item1.Feature, e.Valence) switch { (CurveFeature.Edge, _) => true, (CurveFeature.Interior, EdgeAdjacency.Interior) => true, (CurveFeature.NonManifold, EdgeAdjacency.NonManifold) => true, (CurveFeature.NakedOuter, EdgeAdjacency.Naked) => toSeq(e.TrimIndices()).Exists(t => e.Brep.Trims[t].Loop.LoopType == BrepLoopType.Outer), (CurveFeature.NakedInner, EdgeAdjacency.Naked) => toSeq(e.TrimIndices()).Exists(t => e.Brep.Trims[t].Loop.LoopType == BrepLoopType.Inner), (CurveFeature.Boundary, EdgeAdjacency.Naked) => true, _ => false }).Bind(e => Optional(e.DuplicateCurve()).Map(c => new CurveProjection(curve: c, feature: args.Item1.Feature, type: ComponentIndexType.BrepEdge, index: e.EdgeIndex)).ToSeq()));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> BrepFaceBoundaryHandler = static (g, args) =>
        Optional(((BrepFace)g).DuplicateFace(duplicateMeshes: false))
            .ToFin(Fail: args.Item3.InvalidResult())
            .Bind(faceBrep => {
                using Brep owned = faceBrep;
                return Optional(owned.DuplicateNakedEdgeCurves(true, true))
                    .ToFin(Fail: args.Item3.InvalidResult())
                    .Map(curves => toSeq(curves.Select(curve => new CurveProjection(
                        Curve: curve,
                        Feature: CurveFeature.Boundary,
                        Source: new ComponentIndex(type: ComponentIndexType.BrepFace, index: ((BrepFace)g).FaceIndex)))));
            });
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> MeshEdgeHandler = static (g, args) => Fin.Succ(toSeq(Enumerable.Range(start: 0, count: ((Mesh)g).TopologyEdges.Count)).Where(i => (args.Item1.Feature, ((Mesh)g).TopologyEdges.GetConnectedFaces(topologyEdgeIndex: i).Length) switch { (CurveFeature.Edge, _) => true, (CurveFeature.Boundary, 1) => true, (CurveFeature.Interior, 2) => true, (CurveFeature.NonManifold, > 2) => true, _ => false }).Map(i => new CurveProjection(curve: ((Mesh)g).TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), feature: args.Item1.Feature, type: ComponentIndexType.MeshTopologyEdge, index: i)));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> MeshNakedEdgeHandler = static (g, args) => Fin.Succ(toSeq(Optional(((Mesh)g).GetNakedEdges()).IfNone(static () => [])).Map((poly, i) => new CurveProjection(curve: poly.ToPolylineCurve(), feature: args.Item1.Feature, type: ComponentIndexType.NoType, index: i)));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> BrepLoopHandler = static (g, args) => Fin.Succ(toSeq(((Brep)g).Loops).Where(l => (args.Item1.Feature, l.LoopType) switch { (CurveFeature.OuterLoop, BrepLoopType.Outer) => true, (CurveFeature.InnerLoop, BrepLoopType.Inner) => true, _ => false }).Bind(l => Optional(l.To3dCurve()).Map(c => new CurveProjection(curve: c, feature: args.Item1.Feature, type: ComponentIndexType.BrepLoop, index: l.LoopIndex)).ToSeq()));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> IsoHandler = static (g, args) => g switch { Brep b => toSeq(b.Faces).TraverseM(f => IsoSeq(surface: f, iso: args.Item1.Iso.IfNone(static () => IsoStatus.X), normalized: args.Item1.Normalized.IfNone(static () => 0.5), op: args.Item3).Map(s => s.Map(c => new CurveProjection(Curve: c, Feature: args.Item1.Feature, Source: new ComponentIndex(type: ComponentIndexType.BrepFace, index: f.FaceIndex))))).As().Map(static n => n.Bind(static s => s)), Surface s => IsoSeq(surface: s, iso: args.Item1.Iso.IfNone(static () => IsoStatus.X), normalized: args.Item1.Normalized.IfNone(static () => 0.5), op: args.Item3).Map(seq => seq.Map(c => new CurveProjection(Curve: c, Feature: args.Item1.Feature, Source: new ComponentIndex(type: ComponentIndexType.NoType, index: 0)))), _ => Fin.Fail<Seq<CurveProjection>>(error: args.Item3.Unsupported(geometryType: g.GetType(), outputType: typeof(Curve))) };
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> SilhouetteHandler = static (g, args) => args.Item4.IsCancellationRequested switch {
        true => Fin.Fail<Seq<CurveProjection>>(error: new Fault.Cancelled()),
        false => g switch { GeometryBase native when args.Item1.Direction.IfNone(static () => Vector3d.ZAxis) is { IsValid: true } dir && !dir.IsTiny() => Optional((args.Item1.Feature == CurveFeature.Draft ? Some(args.Item1.Angle.IfNone(static () => 0.0)) : None).Case switch { double angle => Silhouette.ComputeDraftCurve(geometry: native, draftAngle: angle, pullDirection: dir, tolerance: args.Item2.Absolute.Value, angleToleranceRadians: args.Item2.Angle.Value, cancelToken: args.Item4), _ => Silhouette.Compute(geometry: native, silhouetteType: SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, parallelCameraDirection: dir, tolerance: args.Item2.Absolute.Value, angleToleranceRadians: args.Item2.Angle.Value, clippingPlanes: [], cancelToken: args.Item4) }).ToFin(Fail: args.Item4.IsCancellationRequested ? (Error)new Fault.Cancelled() : args.Item3.InvalidResult()).Map(arr => toSeq(arr).Map(sil => new CurveProjection(Curve: sil.Curve, Feature: args.Item1.Feature, Source: sil.GeometryComponentIndex))), _ => Fin.Fail<Seq<CurveProjection>>(error: args.Item3.Unsupported(geometryType: g.GetType(), outputType: typeof(Curve))) },
    };
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> SurfaceBoundaryHandler = static (g, args) => Seq(IsoStatus.South, IsoStatus.East, IsoStatus.North, IsoStatus.West).TraverseM(iso => Optional(((Surface)g).IsoCurve(iso: iso)).ToFin(Fail: args.Item3.InvalidResult())).As().Map(seq => seq.Map((c, i) => new CurveProjection(curve: c, feature: CurveFeature.Boundary, type: ComponentIndexType.NoType, index: i)));
    private static readonly Func<object, (CurveSelector, Context, Op, CancellationToken), Fin<Seq<CurveProjection>>> SubdEdgeHandler = static (g, args) => { _ = ((SubD)g).UpdateSurfaceMeshCache(lazyUpdate: true); return Fin.Succ(toSeq(((SubD)g).DuplicateEdgeCurves().Select((c, i) => new CurveProjection(curve: c, feature: args.Item1.Feature, type: ComponentIndexType.SubdEdge, index: i)))); };
    private static readonly (Type Geometry, CurveFeature Feature, Func<object, (CurveSelector Sel, Context Ctx, Op Op, CancellationToken Cancel), Fin<Seq<CurveProjection>>> Handler)[] CurveCapabilities = [
        (typeof(Curve), CurveFeature.Input, CurveOrPrimitiveInput), (typeof(Curve), CurveFeature.Boundary, CurveOrPrimitiveInput), (typeof(Curve), CurveFeature.Segment, CurveOrPrimitiveInput), (typeof(Curve), CurveFeature.SubCurve, CurveOrPrimitiveInput),
        (typeof(Line), CurveFeature.Input, CurveOrPrimitiveInput), (typeof(Polyline), CurveFeature.Input, CurveOrPrimitiveInput), (typeof(Polyline), CurveFeature.Segment, CurveOrPrimitiveInput), (typeof(Circle), CurveFeature.Input, CurveOrPrimitiveInput), (typeof(Arc), CurveFeature.Input, CurveOrPrimitiveInput),
        (typeof(Brep), CurveFeature.Edge, BrepEdgeHandler), (typeof(Brep), CurveFeature.Boundary, BrepEdgeHandler), (typeof(Brep), CurveFeature.NakedOuter, BrepEdgeHandler), (typeof(Brep), CurveFeature.NakedInner, BrepEdgeHandler), (typeof(Brep), CurveFeature.Interior, BrepEdgeHandler), (typeof(Brep), CurveFeature.NonManifold, BrepEdgeHandler),
        (typeof(BrepFace), CurveFeature.Boundary, BrepFaceBoundaryHandler),
        (typeof(Mesh), CurveFeature.Edge, MeshEdgeHandler), (typeof(Mesh), CurveFeature.Boundary, MeshEdgeHandler), (typeof(Mesh), CurveFeature.NakedOuter, MeshNakedEdgeHandler), (typeof(Mesh), CurveFeature.Interior, MeshEdgeHandler), (typeof(Mesh), CurveFeature.NonManifold, MeshEdgeHandler),
        (typeof(Brep), CurveFeature.OuterLoop, BrepLoopHandler), (typeof(Brep), CurveFeature.InnerLoop, BrepLoopHandler), (typeof(Brep), CurveFeature.Iso, IsoHandler), (typeof(Surface), CurveFeature.Iso, IsoHandler),
        (typeof(Brep), CurveFeature.Silhouette, SilhouetteHandler), (typeof(Mesh), CurveFeature.Silhouette, SilhouetteHandler), (typeof(Surface), CurveFeature.Silhouette, SilhouetteHandler), (typeof(SubD), CurveFeature.Silhouette, SilhouetteHandler), (typeof(Brep), CurveFeature.Draft, SilhouetteHandler), (typeof(Mesh), CurveFeature.Draft, SilhouetteHandler), (typeof(Surface), CurveFeature.Draft, SilhouetteHandler), (typeof(SubD), CurveFeature.Draft, SilhouetteHandler),
        (typeof(Surface), CurveFeature.Boundary, SurfaceBoundaryHandler), (typeof(SubD), CurveFeature.Edge, SubdEdgeHandler), (typeof(SubD), CurveFeature.Segment, SubdEdgeHandler),
    ];
    internal static readonly FrozenDictionary<(Type Geometry, CurveFeature Feature), Func<object, (CurveSelector Sel, Context Ctx, Op Op, CancellationToken Cancel), Fin<Seq<CurveProjection>>>> CurvesTable =
        CurveCapabilities.ToFrozenDictionary(keySelector: static row => (row.Geometry, row.Feature), elementSelector: static row => row.Handler);
    private static IntersectionResult.Hits EventHits(CurveIntersections? hits) =>
        new(Values: toSeq(Optional(hits).ToSeq().Bind(static h => h)).Map(static hit => hit switch {
            { IsPoint: true } => IntersectionHit.At(point: hit.PointA),
            { IsOverlap: true } => IntersectionHit.Overlap(start: hit.PointA, end: hit.PointA2, overlapA: hit.OverlapA, overlapB: hit.OverlapB),
            _ => IntersectionHit.Tag(kind: IntersectionKind.Unknown),
        }));
    private static IntersectionResult.Hits Hits(Curve[]? curves, Point3d[]? points, IntersectionKind curveKind) =>
        new(Values:
            toSeq(curves ?? []).Map(curve => IntersectionHit.Along(curve: curve, kind: curveKind))
            + toSeq(points ?? []).Map(static point => IntersectionHit.At(point: point)));
    private static Fin<IntersectionResult> SolvedHits(bool solved, Curve[]? curves, Point3d[]? points, IntersectionKind curveKind, (Context Ctx, Op Op, CancellationToken Cancel, IProgress<double>? Progress) args) =>
        (solved, args.Cancel.IsCancellationRequested) switch {
            (true, _) => Fin.Succ((IntersectionResult)Hits(curves: curves, points: points, curveKind: curveKind)),
            (false, true) => Fin.Fail<IntersectionResult>(error: new Fault.Cancelled()),
            _ => Fin.Fail<IntersectionResult>(error: args.Op.InvalidResult()),
        };
    internal static readonly FrozenDictionary<(Type, Type), Func<object, object, (Context Ctx, Op Op, CancellationToken Cancel, IProgress<double>? Progress), Fin<IntersectionResult>>> IntersectTable = new Dictionary<(Type, Type), Func<object, object, (Context, Op, CancellationToken, IProgress<double>?), Fin<IntersectionResult>>> {
        [(typeof(Line), typeof(Plane))] = static (a, b, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Values: Intersection.LinePlane(line: (Line)a, plane: (Plane)b, lineParameter: out double t) ? Seq(((Line)a).PointAt(t: t)) : Seq<Point3d>())), [(typeof(Plane), typeof(Plane))] = static (a, b, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Lines(Values: Intersection.PlanePlane(planeA: (Plane)a, planeB: (Plane)b, intersectionLine: out Line line) ? Seq(line) : Seq<Line>())),
        [(typeof(Line), typeof(Circle))] = static (a, b, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Values: Intersection.LineCircle(line: (Line)a, circle: (Circle)b, t1: out double _, point1: out Point3d p1, t2: out double _, point2: out Point3d p2) switch { LineCircleIntersection.Single => Seq(p1), LineCircleIntersection.Multiple => Seq(p1, p2), _ => Seq<Point3d>() })), [(typeof(Line), typeof(Sphere))] = static (a, b, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Values: Intersection.LineSphere(line: (Line)a, sphere: (Sphere)b, intersectionPoint1: out Point3d p1, intersectionPoint2: out Point3d p2) switch { LineSphereIntersection.Single => Seq(p1), LineSphereIntersection.Multiple => Seq(p1, p2), _ => Seq<Point3d>() })),
        [(typeof(Line), typeof(BoundingBox))] = static (a, b, args) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Values: Intersection.LineBox(line: (Line)a, box: (BoundingBox)b, tolerance: args.Item1.Absolute.Value, lineParameters: out Interval iv) ? Seq(iv) : Seq<Interval>())), [(typeof(Line), typeof(Box))] = static (a, b, args) => Fin.Succ((IntersectionResult)new IntersectionResult.Intervals(Values: Intersection.LineBox(line: (Line)a, box: (Box)b, tolerance: args.Item1.Absolute.Value, lineParameters: out Interval iv) ? Seq(iv) : Seq<Interval>())),
        [(typeof(Curve), typeof(Curve))] = static (a, b, args) => args.Item3.IsCancellationRequested ? Fin.Fail<IntersectionResult>(error: new Fault.Cancelled()) : new Func<Fin<IntersectionResult>>(() => { using CurveIntersections? hits = Intersection.CurveCurve(curveA: (Curve)a, curveB: (Curve)b, tolerance: args.Item1.Absolute.Value, overlapTolerance: args.Item1.Absolute.Value); return args.Item3.IsCancellationRequested ? Fin.Fail<IntersectionResult>(error: new Fault.Cancelled()) : Fin.Succ((IntersectionResult)EventHits(hits: hits)); })(), [(typeof(Curve), typeof(Plane))] = static (a, b, args) => { using CurveIntersections? hits = Intersection.CurvePlane(curve: (Curve)a, plane: (Plane)b, tolerance: args.Item1.Absolute.Value); return Fin.Succ((IntersectionResult)EventHits(hits: hits)); },
        [(typeof(Curve), typeof(Line))] = static (a, b, args) => { using CurveIntersections? hits = Intersection.CurveLine(curve: (Curve)a, line: (Line)b, tolerance: args.Item1.Absolute.Value, overlapTolerance: args.Item1.Absolute.Value); return Fin.Succ((IntersectionResult)EventHits(hits: hits)); }, [(typeof(Curve), typeof(Surface))] = static (a, b, args) => { using CurveIntersections? hits = Intersection.CurveSurface(curve: (Curve)a, surface: (Surface)b, tolerance: args.Item1.Absolute.Value, overlapTolerance: args.Item1.Absolute.Value); return Fin.Succ((IntersectionResult)EventHits(hits: hits)); },
        [(typeof(Curve), typeof(Brep))] = static (a, b, args) => SolvedHits(solved: Intersection.CurveBrep(curve: (Curve)a, brep: (Brep)b, tolerance: args.Item1.Absolute.Value, overlapCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Overlap, args: args),
        [(typeof(Curve), typeof(BrepFace))] = static (a, b, args) => SolvedHits(solved: Intersection.CurveBrepFace(curve: (Curve)a, face: (BrepFace)b, tolerance: args.Item1.Absolute.Value, overlapCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Overlap, args: args),
        [(typeof(Surface), typeof(Surface))] = static (a, b, args) => SolvedHits(solved: Intersection.SurfaceSurface(surfaceA: (Surface)a, surfaceB: (Surface)b, tolerance: args.Item1.Absolute.Value, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Curve, args: args),
        [(typeof(Brep), typeof(Plane))] = static (a, b, args) => SolvedHits(solved: Intersection.BrepPlane(brep: (Brep)a, plane: (Plane)b, tolerance: args.Item1.Absolute.Value, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Curve, args: args),
        [(typeof(Brep), typeof(Surface))] = static (a, b, args) => SolvedHits(solved: Intersection.BrepSurface(brep: (Brep)a, surface: (Surface)b, tolerance: args.Item1.Absolute.Value, joinCurves: true, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Curve, args: args),
        [(typeof(Brep), typeof(Brep))] = static (a, b, args) => SolvedHits(solved: Intersection.BrepBrep(brepA: (Brep)a, brepB: (Brep)b, tolerance: args.Item1.Absolute.Value, joinCurves: true, intersectionCurves: out Curve[] curves, intersectionPoints: out Point3d[] points), curves: curves, points: points, curveKind: IntersectionKind.Curve, args: args),
        [(typeof(Mesh), typeof(Line))] = static (a, b, _) => Fin.Succ((IntersectionResult)new IntersectionResult.Points(Values: toSeq(Intersection.MeshLineSorted(mesh: (Mesh)a, line: (Line)b, faceIds: out int[] _) ?? []))),
        [(typeof(Mesh), typeof(Plane))] = static (a, b, args) => { using MeshIntersectionCache cache = new(); Polyline[]? polylines = Intersection.MeshPlane(mesh: (Mesh)a, cache: cache, plane: (Plane)b, tolerance: args.Item1.Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient, overlaps: true); return Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(Values: toSeq(Optional(polylines).ToSeq().Bind(static h => h)).Map(static p => (Curve: p, Kind: IntersectionKind.Unknown)))); },
        [(typeof(Mesh), typeof(Mesh))] = static (a, b, args) => { using TextLog textLog = new(); return Intersection.MeshMesh(meshes: [(Mesh)a, (Mesh)b], tolerance: args.Item1.Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient, intersections: out Polyline[] ints, overlapsPolylines: true, overlapsPolylinesResult: out Polyline[] olap, overlapsMesh: false, overlapsMeshResult: out Mesh _, textLog: textLog, cancel: args.Item3, progress: args.Item4) switch { true => Fin.Succ((IntersectionResult)new IntersectionResult.Polylines(Values: toSeq(Optional(ints).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Curve)) + toSeq(Optional(olap).ToSeq().Bind(static p => p)).Map(static p => (Curve: p, Kind: IntersectionKind.Overlap)))), false when args.Item3.IsCancellationRequested => Fin.Fail<IntersectionResult>(error: new Fault.Cancelled()), false => Fin.Fail<IntersectionResult>(error: args.Item2.InvalidResult()) }; },
    }.ToFrozenDictionary();
    private static Option<Kind> ShapedAs(object value, Context ctx, Kind kind, Func<Surface, double, bool> probe) =>
        value switch {
            Brep { IsSurface: true } b when probe(arg1: b.Surfaces[0], arg2: ctx.Absolute.Value) => Some(kind),
            Surface s when probe(arg1: s, arg2: ctx.Absolute.Value) => Some(kind),
            _ => Option<Kind>.None,
        };
    // Brep{IsSurface:true} reports the primitive Kind, except Plane which reports Kind.Surface — preserves the Brep-vs-Surface asymmetry.
    internal static readonly Seq<Func<object, Context, Option<Kind>>> KindPredicates = Seq<Func<object, Context, Option<Kind>>>(
        static (v, ctx) => v is Brep b && b.IsBox(tolerance: ctx.Absolute.Value) ? Some(Kind.Box) : Option<Kind>.None,
        static (v, ctx) => v is Curve c && c.IsLinear(tolerance: ctx.Absolute.Value) ? Some(Kind.Line) : Option<Kind>.None,
        static (v, ctx) => v is Curve c && c.TryGetCircle(circle: out Circle _, tolerance: ctx.Absolute.Value) ? Some(Kind.Circle) : Option<Kind>.None,
        static (v, ctx) => v is Curve c && c.TryGetArc(arc: out Arc _, tolerance: ctx.Absolute.Value) ? Some(Kind.Arc) : Option<Kind>.None,
        static (v, ctx) => v is Curve c && c.TryGetEllipse(ellipse: out Ellipse _, tolerance: ctx.Absolute.Value) ? Some(Kind.Ellipse) : Option<Kind>.None,
        static (v, _) => v is Curve c && c.TryGetPolyline(polyline: out Polyline _) ? Some(Kind.Polyline) : Option<Kind>.None,
        static (v, ctx) => v switch {
            Brep { IsSurface: true } b when b.Surfaces[0].TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) => Some(Kind.Surface),
            Surface s when s.TryGetPlane(plane: out Plane _, tolerance: ctx.Absolute.Value) => Some(Kind.Plane),
            _ => Option<Kind>.None,
        },
        static (v, ctx) => ShapedAs(value: v, ctx: ctx, kind: Kind.Sphere, probe: static (s, t) => s.TryGetSphere(sphere: out Sphere _, tolerance: t)),
        static (v, ctx) => ShapedAs(value: v, ctx: ctx, kind: Kind.Cylinder, probe: static (s, t) => s.TryGetCylinder(cylinder: out Cylinder _, tolerance: t)),
        static (v, ctx) => ShapedAs(value: v, ctx: ctx, kind: Kind.Cone, probe: static (s, t) => s.TryGetCone(cone: out Cone _, tolerance: t)),
        static (v, ctx) => ShapedAs(value: v, ctx: ctx, kind: Kind.Torus, probe: static (s, t) => s.TryGetTorus(torus: out Torus _, tolerance: t)));
    internal static readonly FrozenDictionary<(Type Geometry, MassKind Mass), Func<object, (Context Ctx, bool FirstMoments, bool SecondMoments, bool ProductMoments), Fin<IDisposable>>> MassPropertiesTable = new Dictionary<(Type, MassKind), Func<object, (Context, bool, bool, bool), Fin<IDisposable>>> {
        [(typeof(Curve), MassKind.Length)] = static (g, args) => Optional(LengthMassProperties.Compute(curve: (Curve)g, length: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(LengthMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Curve), MassKind.Area)] = static (g, args) => Optional(AreaMassProperties.Compute(closedPlanarCurve: (Curve)g, planarTolerance: args.Item1.Absolute.Value)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(AreaMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Mesh), MassKind.Area)] = static (g, args) => Optional(AreaMassProperties.Compute(mesh: (Mesh)g, area: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(AreaMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Brep), MassKind.Area)] = static (g, args) => Optional(AreaMassProperties.Compute(brep: (Brep)g, area: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4, relativeTolerance: args.Item1.Relative.Value, absoluteTolerance: args.Item1.Absolute.Value)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(AreaMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Surface), MassKind.Area)] = static (g, args) => Optional(AreaMassProperties.Compute(surface: (Surface)g, area: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(AreaMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Mesh), MassKind.Volume)] = static (g, args) => Optional(VolumeMassProperties.Compute(mesh: (Mesh)g, volume: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(VolumeMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Brep), MassKind.Volume)] = static (g, args) => Optional(VolumeMassProperties.Compute(brep: (Brep)g, volume: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4, relativeTolerance: args.Item1.Relative.Value, absoluteTolerance: args.Item1.Absolute.Value)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(VolumeMassProperties))).Map(static p => (IDisposable)p),
        [(typeof(Surface), MassKind.Volume)] = static (g, args) => Optional(VolumeMassProperties.Compute(surface: (Surface)g, volume: true, firstMoments: args.Item2, secondMoments: args.Item3, productMoments: args.Item4)).ToFin(Fail: new Fault.ComputationFailed(Label: nameof(VolumeMassProperties))).Map(static p => (IDisposable)p),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (Context Ctx, Op Op), Fin<double>>> LengthTable = new Dictionary<Type, Func<object, (Context, Op), Fin<double>>> {
        [typeof(Line)] = static (g, args) => ((Line)g).IsValid ? Fin.Succ(((Line)g).Length) : Fin.Fail<double>(error: args.Item2.InvalidInput()),
        [typeof(Polyline)] = static (g, args) => ((Polyline)g).IsValid ? Fin.Succ(((Polyline)g).Length) : Fin.Fail<double>(error: args.Item2.InvalidInput()),
        [typeof(Curve)] = static (g, args) => ((Curve)g).GetLength(fractionalTolerance: args.Item1.Relative.Value) switch {
            double l when RhinoMath.IsValidDouble(x: l) && l >= 0.0 => Fin.Succ(l),
            _ => Fin.Fail<double>(error: args.Item2.InvalidResult()),
        },
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (Point3d Target, Context Ctx, Op Op), Fin<bool>>> ContainsTable = new Dictionary<Type, Func<object, (Point3d, Context, Op), Fin<bool>>> {
        [typeof(Brep)] = static (g, args) => Fin.Succ(((Brep)g).IsPointInside(point: args.Item1, tolerance: args.Item2.Absolute.Value, strictlyIn: false)),
        [typeof(Mesh)] = static (g, args) => Fin.Succ(((Mesh)g).IsPointInside(point: args.Item1, tolerance: args.Item2.Absolute.Value, strictlyIn: false)),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, Op, Fin<SolidOrientation>>> SolidOrientationTable = new Dictionary<Type, Func<object, Op, Fin<SolidOrientation>>> {
        [typeof(Brep)] = static (g, _) => Fin.Succ((SolidOrientation)(int)((Brep)g).SolidOrientation),
        [typeof(Mesh)] = static (g, _) => Fin.Succ((SolidOrientation)((Mesh)g).SolidOrientation()),
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<Type, Func<object, (Context Ctx, Op Op), Fin<Seq<FaceProjection>>>> FacesTable = new Dictionary<Type, Func<object, (Context, Op), Fin<Seq<FaceProjection>>>> {
        [typeof(Brep)] = static (g, _) => Fin.Succ(toSeq(((Brep)g).Faces.Cast<BrepFace>().Select(static f => FaceProjection.From(face: f)))),
        [typeof(BrepFace)] = static (g, _) => Fin.Succ(Seq(FaceProjection.From(face: (BrepFace)g))),
        [typeof(GeometryBase)] = static (g, args) => g switch {
            Brep brep => Fin.Succ(toSeq(brep.Faces.Cast<BrepFace>().Select(static f => FaceProjection.From(face: f)))),
            GeometryBase { HasBrepForm: true } gb => Optional(Brep.TryConvertBrep(geometry: gb)).ToFin(Fail: args.Item2.InvalidResult())
                .Map(static b => { using Brep owned = b; return toSeq(owned.Faces.Cast<BrepFace>().Select(static f => FaceProjection.From(face: f))); }),
            _ => Fin.Fail<Seq<FaceProjection>>(error: args.Item2.Unsupported(geometryType: g.GetType(), outputType: typeof(Seq<FaceProjection>))),
        },
    }.ToFrozenDictionary();
    internal static readonly FrozenDictionary<(Type, Type), Func<object, object, (int Count, Context Ctx, Op Op), Fin<Seq<ResidualSample>>>> ConformanceTable = new Dictionary<(Type, Type), Func<object, object, (int, Context, Op), Fin<Seq<ResidualSample>>>> {
        [(typeof(Curve), typeof(Line))] = static (g, p, args) => SampleCurveAgainst(curve: (Curve)g, primitive: (Line)p, count: args.Item1, ctx: args.Item2, op: args.Item3, distance: static (line, pt) => pt.DistanceTo(other: line.ClosestPoint(testPoint: pt, limitToFiniteSegment: false))),
        [(typeof(Curve), typeof(Circle))] = static (g, p, args) => SampleCurveAgainst(curve: (Curve)g, primitive: (Circle)p, count: args.Item1, ctx: args.Item2, op: args.Item3, distance: static (circle, pt) => pt.DistanceTo(other: circle.ClosestPoint(testPoint: pt))),
        [(typeof(Curve), typeof(Arc))] = static (g, p, args) => SampleCurveAgainst(curve: (Curve)g, primitive: (Arc)p, count: args.Item1, ctx: args.Item2, op: args.Item3, distance: static (arc, pt) => pt.DistanceTo(other: arc.ClosestPoint(testPoint: pt))),
        [(typeof(Surface), typeof(Plane))] = static (g, p, args) => SampleSurfaceAgainst(surface: (Surface)g, primitive: (Plane)p, count: args.Item1, ctx: args.Item2, op: args.Item3, distance: static (plane, pt) => Math.Abs(value: plane.DistanceTo(testPoint: pt))),
        [(typeof(Surface), typeof(Sphere))] = static (g, p, args) => SampleSurfaceAgainst(surface: (Surface)g, primitive: (Sphere)p, count: args.Item1, ctx: args.Item2, op: args.Item3, distance: static (sphere, pt) => pt.DistanceTo(other: sphere.ClosestPoint(testPoint: pt))),
    }.ToFrozenDictionary();
    internal static Fin<Seq<double>> Fractions(int count, Op op) => count switch {
        1 => Fin.Succ(Seq(0.5)),
        > 1 => Fin.Succ(toSeq(Enumerable.Range(start: 0, count: count).Select(i => i / (count - 1.0)))),
        _ => Fin.Fail<Seq<double>>(error: op.InvalidInput()),
    };
    private static Seq<ResidualSample> Residuals<TPrimitive>(Seq<Point3d> points, TPrimitive primitive, Context ctx, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        toSeq(points.AsIterable().Select((p, i) => distance(arg1: primitive, arg2: p) switch {
            double d => new ResidualSample(Index: i, Location: p, Distance: d, Tolerance: ctx.Absolute.Value, WithinTolerance: d <= ctx.Absolute.Value),
        }));
    private static Fin<Seq<ResidualSample>> SampleCurveAgainst<TPrimitive>(Curve curve, TPrimitive primitive, int count, Context ctx, Op op, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        Fractions(count: count, op: op)
            .Bind(fs => Optional(curve.NormalizedLengthParameters(s: [.. fs.AsIterable()], absoluteTolerance: ctx.Absolute.Value, fractionalTolerance: ctx.Relative.Value)).ToFin(Fail: op.InvalidResult()).Map(ps => Residuals(points: toSeq(ps).Map(curve.PointAt), primitive: primitive, ctx: ctx, distance: distance)));
    private static Fin<Seq<ResidualSample>> SampleSurfaceAgainst<TPrimitive>(Surface surface, TPrimitive primitive, int count, Context ctx, Op op, Func<TPrimitive, Point3d, double> distance) where TPrimitive : notnull =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            ( { IsValid: true } u, { IsValid: true } v) => Fractions(count: count, op: op)
                .Map(fractions => Residuals(points: fractions.Map(f => u.ParameterAt(normalizedParameter: f)).Bind(pu => fractions.Map(f => v.ParameterAt(normalizedParameter: f)).Map(pv => surface.PointAt(u: pu, v: pv))), primitive: primitive, ctx: ctx, distance: distance)),
            _ => Fin.Fail<Seq<ResidualSample>>(error: op.InvalidInput()),
        };
    internal static Option<TVal> Lookup<TVal>(FrozenDictionary<Type, TVal> table, Type key) =>
        Optional(table.GetValueOrDefault(key: key))
        | (KindLookup.InheritsBase(type: key) is Type bt ? Optional(table.GetValueOrDefault(key: bt)) : Option<TVal>.None)
        | (typeof(GeometryBase).IsAssignableFrom(c: key) ? Optional(table.GetValueOrDefault(key: typeof(GeometryBase))) : Option<TVal>.None);
    internal static Option<TVal> LookupTagged<TTag, TVal>(FrozenDictionary<(Type, TTag), TVal> table, Type key, TTag tag) where TTag : notnull =>
        Optional(table.GetValueOrDefault(key: (key, tag)))
        | (KindLookup.InheritsBase(type: key) is Type bt ? Optional(table.GetValueOrDefault(key: (bt, tag))) : Option<TVal>.None);
    internal static Option<TVal> LookupPair<TVal>(FrozenDictionary<(Type, Type), TVal> table, Type left, Type right) {
        Option<Type> leftBase = Optional(KindLookup.InheritsBase(type: left));
        Option<Type> rightBase = Optional(KindLookup.InheritsBase(type: right));
        return Optional(table.GetValueOrDefault(key: (left, right)))
            | leftBase.Bind(b => Optional(table.GetValueOrDefault(key: (b, right))))
            | rightBase.Bind(b => Optional(table.GetValueOrDefault(key: (left, b))))
            | leftBase.Bind(lb => rightBase.Bind(rb => Optional(table.GetValueOrDefault(key: (lb, rb)))));
    }
    internal static Fin<TOut> Resolve<TOut, TArgs>(FrozenDictionary<Type, Func<object, TArgs, Fin<TOut>>> table, object? source, TArgs args, Op op) =>
        Optional(source).Bind(s => Lookup(table: table, key: s.GetType()).Map(fn => (Source: s, Fn: fn))).Match(
            Some: hit => hit.Fn(arg1: hit.Source, arg2: args),
            None: () => Fin.Fail<TOut>(error: source is null ? op.InvalidInput() : op.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))));
    internal static bool Supports<TValue>(FrozenDictionary<Type, TValue> table, Type source) => Lookup(table: table, key: source).IsSome;
    internal static Fin<TOut> ResolveTagged<TOut, TTag, TArgs>(FrozenDictionary<(Type, TTag), Func<object, TArgs, Fin<TOut>>> table, object? source, TTag tag, TArgs args, Op op) where TTag : notnull =>
        Optional(source).Bind(s => LookupTagged(table: table, key: s.GetType(), tag: tag).Map(fn => (Source: s, Fn: fn))).Match(
            Some: hit => hit.Fn(arg1: hit.Source, arg2: args),
            None: () => Fin.Fail<TOut>(error: source is null ? op.InvalidInput() : op.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))));
    internal static Fin<TOut> ResolvePair<TOut, TArgs>(FrozenDictionary<(Type, Type), Func<object, object, TArgs, Fin<TOut>>> table, object? left, object? right, TArgs args, Op op) =>
        Optional(left).Bind(l => Optional(right).Map(r => (Left: l, Right: r))).Match(
            Some: pair => LookupPair(table: table, left: pair.Left.GetType(), right: pair.Right.GetType()).Match(
                Some: fn => fn(arg1: pair.Left, arg2: pair.Right, arg3: args),
                None: () => Fin.Fail<TOut>(error: op.Unsupported(geometryType: pair.Left.GetType(), outputType: pair.Right.GetType()))),
            None: () => Fin.Fail<TOut>(error: op.InvalidInput()));
    internal static Fin<TOut> ResolveUnorderedPair<TOut, TArgs>(FrozenDictionary<(Type, Type), Func<object, object, TArgs, Fin<TOut>>> table, object? left, object? right, TArgs args, Op op) =>
        Optional(left).Bind(l => Optional(right).Map(r => (Left: l, Right: r))).Match(
            Some: pair => (LookupPair(table: table, left: pair.Left.GetType(), right: pair.Right.GetType()).Map(fn => (Source: (L: pair.Left, R: pair.Right), Fn: fn))
                | LookupPair(table: table, left: pair.Right.GetType(), right: pair.Left.GetType()).Map(fn => (Source: (L: pair.Right, R: pair.Left), Fn: fn))).Match(
                    Some: hit => hit.Fn(arg1: hit.Source.L, arg2: hit.Source.R, arg3: args),
                    None: () => Fin.Fail<TOut>(error: op.Unsupported(geometryType: pair.Left.GetType(), outputType: pair.Right.GetType()))),
            None: () => Fin.Fail<TOut>(error: op.InvalidInput()));
    internal static bool SupportsPair<TValue>(FrozenDictionary<(Type, Type), TValue> table, Type left, Type right) => LookupPair(table: table, left: left, right: right).IsSome;
    internal static bool SupportsUnorderedPair<TValue>(FrozenDictionary<(Type, Type), TValue> table, Type left, Type right) =>
        SupportsPair(table: table, left: left, right: right) || SupportsPair(table: table, left: right, right: left);
    internal static readonly FrozenDictionary<Type, Func<object, bool>> ValidityTable = new Dictionary<Type, Func<object, bool>> {
        [typeof(GeometryBase)] = static g => ((GeometryBase)g).IsValid,
        [typeof(double)] = static d => RhinoMath.IsValidDouble(x: (double)d),
        [typeof(bool)] = static _ => true, [typeof(int)] = static _ => true, [typeof(SurfaceCurvature)] = static _ => true, [typeof(MeshCheckParameters)] = static _ => true, [typeof(Kind)] = static _ => true,
        [typeof(MeshPoint)] = static m => ((MeshPoint)m).Point.IsValid,
        [typeof(ComponentIndex)] = static c => (ComponentIndex)c is { ComponentIndexType: not ComponentIndexType.InvalidType } ci && ci.Index >= 0,
        [typeof(IntersectionHit)] = static h => (IntersectionHit)h is { Kind: not IntersectionKind.Unknown } hit
            && hit.Point.Map(static point => point.IsValid).IfNone(true)
            && hit.PointB.Map(static point => point.IsValid).IfNone(true)
            && hit.OverlapA.Map(static interval => interval.IsValid).IfNone(true)
            && hit.OverlapB.Map(static interval => interval.IsValid).IfNone(true)
            && hit.Curve.Map(static curve => curve.IsValid).IfNone(true),
        [typeof(ValueTuple<double, Vector3d>)] = static t => (ValueTuple<double, Vector3d>)t is var tup && RhinoMath.IsValidDouble(x: tup.Item1) && tup.Item2.IsValid,
        [typeof(Point2d)] = static p => ((Point2d)p).IsValid, [typeof(Point3d)] = static p => ((Point3d)p).IsValid, [typeof(Vector3d)] = static v => ((Vector3d)v).IsValid, [typeof(Plane)] = static p => ((Plane)p).IsValid,
        [typeof(BoundingBox)] = static b => ((BoundingBox)b).IsValid, [typeof(Box)] = static b => ((Box)b).IsValid, [typeof(Sphere)] = static s => ((Sphere)s).IsValid,
        [typeof(Cylinder)] = static c => ((Cylinder)c).IsValid, [typeof(Cone)] = static c => ((Cone)c).IsValid, [typeof(Torus)] = static t => ((Torus)t).IsValid,
        [typeof(Arc)] = static a => ((Arc)a).IsValid, [typeof(Circle)] = static c => ((Circle)c).IsValid, [typeof(Ellipse)] = static e => ((Ellipse)e).IsValid,
        [typeof(Rectangle3d)] = static r => ((Rectangle3d)r).IsValid, [typeof(Interval)] = static i => ((Interval)i).IsValid,
        [typeof(Line)] = static l => ((Line)l).IsValid, [typeof(Polyline)] = static p => ((Polyline)p).IsValid,
    }.ToFrozenDictionary();
    internal static Option<bool> ValidityOf(object source) => Lookup(table: ValidityTable, key: source.GetType()).Map(predicate => predicate(arg: source));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[BoundaryAdapter]
public static class KindRole {
    public static Option<Kind> AsKind(this Type type) => KindLookup.For(type: type);
    public static bool SupportsBounds(this Type type, bool includeSphere) => (Dispatch.Supports(table: Dispatch.BoundsTable, source: type) || type == typeof(object)) && (includeSphere || type != typeof(Sphere)) && type != typeof(Plane);
    public static bool InputCurve(this CurveFeature feature) => feature is CurveFeature.Input or CurveFeature.Segment or CurveFeature.SubCurve or CurveFeature.Boundary;
    public static bool InputBoundary(this CurveFeature feature) => feature is CurveFeature.Input or CurveFeature.Boundary;
    public static bool IsGeometryBaseDerived(this Kind kind) => typeof(GeometryBase).IsAssignableFrom(c: kind?.Type);
    public static Fin<Kind> Kind(this object value, Context ctx) =>
        (Dispatch.KindPredicates.Choose(predicate => predicate(arg1: value, arg2: ctx)).Head | KindLookup.For(type: value?.GetType() ?? typeof(object)))
            .ToFin(Fail: Op.Of(name: nameof(Kind)).InvalidInput());
    public static Fin<BoundingBox> Bounds(this object value, Op op) => Dispatch.Resolve(table: Dispatch.BoundsTable, source: value, args: op, op: op);
    public static Fin<Seq<Point3d>> Vertices(this Kind kind, object value, Context ctx, Op op) => Dispatch.Resolve(table: Dispatch.VerticesTable, source: value, args: (ctx, op), op: op);
    public static Fin<Point3d> Centroid(this Kind kind, object value, Context ctx, Op op) => Dispatch.Resolve(table: Dispatch.CentroidTable, source: value, args: (ctx, op), op: op);
    public static Fin<ClosestHit> Closest(this Kind kind, object value, Point3d target, Context ctx, Op op) =>
        target.IsValid ? Dispatch.Resolve(table: Dispatch.ClosestTable, source: value, args: (target, ctx, op), op: op) : Fin.Fail<ClosestHit>(error: op.InvalidInput());
    public static Fin<Seq<CurveProjection>> Curves(this Kind kind, object value, CurveSelector selector, Context ctx, Op op, CancellationToken cancel = default) =>
        Dispatch.ResolveTagged(table: Dispatch.CurvesTable, source: value, tag: selector.Feature, args: (selector, ctx, op, cancel), op: op);
    public static Fin<Seq<GeometryBase>> Components(this Kind kind, object value, Context ctx, Op op) => Dispatch.Resolve(table: Dispatch.ComponentsTable, source: value, args: op, op: op);
    public static Fin<Seq<Interval>> Domains(this Kind kind, object value, Op op) => Dispatch.Resolve(table: Dispatch.DomainsTable, source: value, args: op, op: op);
    public static Fin<Seq<Curve>> IsoCurves(this Kind kind, object value, IsoStatus direction, double normalized, Op op) => Dispatch.Resolve(table: Dispatch.IsoCurvesTable, source: value, args: (direction, normalized, op), op: op);
    public static Fin<Seq<Point3d>> ControlPoints(this Kind kind, object value, Op op) => Dispatch.Resolve(table: Dispatch.ControlPointsTable, source: value, args: op, op: op);
    public static Closure ClosureOf(this Kind kind, object value, Context ctx) => (kind?.Topology, value) switch { (Topology.Curve, Curve c) => c.IsClosed ? Closure.Closed : Closure.Open, (Topology.Brep, Brep b) => b.IsSolid ? Closure.Closed : Closure.Open, (Topology.Mesh, Mesh m) => m.IsClosed ? Closure.Closed : Closure.Open, _ => kind?.NominalClosure ?? Closure.Unknown };
    public static Solidity SolidityOf(this Kind kind, object value, Context ctx) => (kind?.Topology, value) switch { (Topology.Brep, Brep b) => b.IsSolid ? Solidity.Solid : Solidity.Open, (Topology.Mesh, Mesh m) => m.IsSolid ? Solidity.Solid : Solidity.Open, (Topology.Surface, Surface s) => s.IsSolid ? Solidity.Solid : Solidity.Open, _ => kind?.NominalSolidity ?? Solidity.Unknown };
    public static Fin<IntersectionResult> Intersect(this Kind a, Kind b, object valueA, object valueB, Context ctx, Op op, IProgress<double>? progress = null, CancellationToken cancel = default) =>
        Dispatch.ResolveUnorderedPair(table: Dispatch.IntersectTable, left: valueA, right: valueB, args: (ctx, op, cancel, progress), op: op);
    public static Fin<bool> Contains(this Kind kind, object value, Point3d target, Context ctx, Op op) =>
        target.IsValid ? Dispatch.Resolve(table: Dispatch.ContainsTable, source: value, args: (target, ctx, op), op: op) : Fin.Fail<bool>(error: op.InvalidInput());
    public static Fin<TTarget> Coerce<TTarget>(this Kind kind, object value, Context ctx, Op op) => Dispatch.Coerce<TTarget>(source: value, ctx: ctx, op: op);
    public static Fin<SolidOrientation> SolidOrientation(this Kind kind, object value, Op op) => Dispatch.Resolve(table: Dispatch.SolidOrientationTable, source: value, args: op, op: op);
    public static Fin<double> Length(this Kind kind, object value, Context ctx, Op op) => Dispatch.Resolve(table: Dispatch.LengthTable, source: value, args: (ctx, op), op: op);
    public static Fin<Seq<FaceProjection>> Faces(this Kind kind, object value, Context ctx, Op op) => Dispatch.Resolve(table: Dispatch.FacesTable, source: value, args: (ctx, op), op: op);
    public static Fin<Seq<ResidualSample>> Conformance(this Kind kindG, Kind kindP, object geometry, object primitive, int count, Context ctx, Op op) =>
        Dispatch.ResolvePair(table: Dispatch.ConformanceTable, left: geometry, right: primitive, args: (count, ctx, op), op: op);
}
[BoundaryAdapter]
public static class MassKindRole {
    public static Eff<Env, IDisposable> Compute(this MassKind mass, object value, Op op, bool firstMoments = false, bool secondMoments = false, bool productMoments = false) =>
        from context in Env.Asks
        from result in Dispatch.ResolveTagged(table: Dispatch.MassPropertiesTable, source: value, tag: mass, args: (context, firstMoments, secondMoments, productMoments), op: op).ToEff()
        select result;
}

// --- [COMPOSITION] ------------------------------------------------------------------------
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult;
    public sealed record Lines(Seq<Line> Values) : IntersectionResult;
    public sealed record Circles(Seq<Circle> Values) : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values) : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values) : IntersectionResult;
    public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult;
    public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
}
