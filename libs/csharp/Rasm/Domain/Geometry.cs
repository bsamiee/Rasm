using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
public enum SolidOrientation { Unknown = 0, Outward = 1, Inward = -1 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, Iso, Silhouette, SubCurve, Draft }
internal enum ProjectionOwnership { Dispose, Transfer }
[Union]
internal abstract partial record Lease<T> where T : class, IDisposable {
    private Lease() { }
    public sealed record Owned(T Value) : Lease<T> {
        internal TResult Project<TResult>(Func<T, TResult> project) {
            using T owned = Value;
            return project(arg: owned);
        }
    }
    public sealed record Borrowed(T Value) : Lease<T>;
    internal TResult Use<TResult>(Func<T, TResult> project) =>
        Switch(
            state: project,
            owned: static (use, owned) => owned.Project(project: use),
            borrowed: static (use, borrowed) => use(arg: borrowed.Value));
}
[BoundaryAdapter]
public sealed record TopologyProjection {
    private static readonly Op Key = Op.Of(name: nameof(TopologyProjection));
    public GeometryBase Value { get; }
    public CurveFeature Feature { get; }
    public ComponentIndex Source { get; }
    public bool Owns { get; }
    public bool Reversed { get; }
    private TopologyProjection(GeometryBase value, CurveFeature feature, ComponentIndex source, bool owns, bool reversed = false) { Value = value; Feature = feature; Source = source; Owns = owns; Reversed = reversed; }
    public int FaceIndex => Source.Index;
    public Option<T> As<T>() where T : class => Value is T match ? Some(match) : Option<T>.None;
    internal Fin<T> OnMeshFace<T>(Func<Mesh, int, Fin<T>> use) where T : notnull => (Value, Source) switch { (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index }) when index >= 0 && index < mesh.Faces.Count => use(mesh, index), _ => Fin.Fail<T>(Key.InvalidInput()) };
    public static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndex source) { ArgumentNullException.ThrowIfNull(curve); return new(value: curve, feature: feature, source: source, owns: true); }
    internal static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndexType type, int index) => FromCurve(curve, feature, new ComponentIndex(type, index));
    public static TopologyProjection FaceFrom(BrepFace face) { ArgumentNullException.ThrowIfNull(face); return new(value: face.DuplicateFace(duplicateMeshes: false), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), owns: true, reversed: face.OrientationIsReversed); }
    public static Fin<TopologyProjection> MeshFace(Mesh? mesh, int face) => Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => (native.Faces.Count, face) switch { ( <= 0, _) => Fin.Fail<TopologyProjection>(Key.InvalidResult()), (int count, int index) when index >= 0 && index < count => Fin.Succ<TopologyProjection>(new(value: native, feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.MeshFace, index), owns: false)), _ => Fin.Fail<TopologyProjection>(Key.InvalidInput()) });
    public Unit Dispose() => Optional((Owns, Value) switch { (true, IDisposable disposable) => disposable, _ => null }).Iter(static d => d.Dispose());
    public bool SameAs(TopologyProjection? other) => other switch { TopologyProjection p => ReferenceEquals(Value, p.Value) && Source.Equals(p.Source), _ => false };
    public bool Transfers(Type outputType) => Owns && ((Value is Curve && outputType == typeof(Curve)) || (Value is Brep && outputType == typeof(Brep)));
    public Fin<Seq<Point3d>> Vertices => OnMeshFace<Seq<Point3d>>(static (mesh, face) => mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch { true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)), true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)), false => Fin.Fail<Seq<Point3d>>(Key.InvalidResult()) });
    public Fin<Vector3d> Normal => OnMeshFace<Vector3d>(static (mesh, face) =>
        FaceNormal(mesh.FaceNormals.Count > face ? Some(mesh.FaceNormals[face]) : Option<Vector3f>.None).Match(
            Succ: Fin.Succ,
            Fail: _ => (mesh.FaceNormals.ComputeFaceNormals() && mesh.FaceNormals.Count > face) switch {
                true => FaceNormal(Some(mesh.FaceNormals[face])),
                false => Fin.Fail<Vector3d>(Key.InvalidResult()),
            }));
    private static Fin<Vector3d> FaceNormal(Option<Vector3f> source) =>
        source.Case switch {
            Vector3f faceNormal when new Vector3d(faceNormal) is Vector3d normal && normal.IsValid && !normal.IsTiny() => Fin.Succ(normal),
            _ => Fin.Fail<Vector3d>(Key.InvalidResult()),
        };
    internal static Fin<Seq<TValue>> Project<TValue>(
        Seq<TopologyProjection> all,
        Seq<TopologyProjection> chosen,
        ProjectionOwnership ownership,
        Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
        Fin<Seq<TValue>> result = project(arg: chosen);
        bool keep = ownership == ProjectionOwnership.Transfer && result.IsSucc;
        _ = all.Filter(value => !keep || !chosen.Exists(c => c.SameAs(other: value))).Iter(static v => v.Dispose());
        return result;
    }
    public Fin<Point3d> Centroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => Centroid(context: ctx, key: Key));
    public Fin<Plane> FrameAtCentroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => FrameAtCentroid(context: ctx, key: Key));
    internal Fin<Point3d> Centroid(Context context, Op key) =>
        this switch {
            { Value: Brep brep } => Optional(AreaMassProperties.Compute(brep: brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value))
                .ToFin(key.InvalidResult())
                .Map(static mass => new Lease<AreaMassProperties>.Owned(Value: mass).Use(static disposable => disposable.Centroid)),
            _ => Fin.Fail<Point3d>(key.InvalidInput()),
        };
    internal Fin<Plane> FrameAtCentroid(Context context, Op key) =>
        this switch {
            { Value: Brep brep, Reversed: bool reversed } => Centroid(context: context, key: key)
                .Bind(centroid => {
                    BrepFace brepFace = brep.Faces[0];
                    return brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: 0.0) switch {
                        true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                            (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() => Fin.Succ((frame.ZAxis * (reversed ? -normal : normal)) switch {
                                >= 0.0 => frame,
                                _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                            }),
                            _ => Fin.Fail<Plane>(key.InvalidResult()),
                        },
                        false => Fin.Fail<Plane>(key.InvalidResult()),
                    };
                }),
            _ => Fin.Fail<Plane>(key.InvalidInput()),
        };
    internal Fin<Seq<Interval>> Domains(Op key) =>
        this switch {
            { Value: Brep brep } => brep.Faces[0] switch {
                BrepFace brepFace => (brepFace.Domain(direction: 0), brepFace.Domain(direction: 1)) switch {
                    (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
                    _ => Fin.Fail<Seq<Interval>>(key.InvalidResult()),
                },
            },
            _ => Fin.Fail<Seq<Interval>>(key.InvalidInput()),
        };
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveSelector(CurveFeature Feature, Option<Vector3d> Direction = default, Option<double> Angle = default, Option<int> Index = default, Option<double> Normalized = default, Option<IsoStatus> Iso = default);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Hit(int Id);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Couple(int A, int B);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);
[BoundaryAdapter]
[Union]
public abstract partial record IntersectionHit {
    private IntersectionHit() { }
    public sealed record PointCase(Point3d Point) : IntersectionHit;
    public sealed record CurveCase(Curve Curve, IntersectionKind CurveKind) : IntersectionHit;
    public sealed record OverlapCase(Point3d Start, Point3d End, Interval OverlapA, Interval OverlapB, Option<Curve> Curve) : IntersectionHit;
    public IntersectionKind Kind => Switch(pointCase: static _ => IntersectionKind.Point, curveCase: static c => c.CurveKind, overlapCase: static _ => IntersectionKind.Overlap);
    public Seq<Curve> Curves => Switch(pointCase: static _ => Seq<Curve>(), curveCase: static c => Seq(c.Curve), overlapCase: static o => o.Curve.ToSeq());
    public Seq<Point3d> Points => Switch(pointCase: static p => Seq(p.Point), curveCase: static _ => Seq<Point3d>(), overlapCase: static o => Seq(o.Start, o.End));
    public Seq<Interval> Intervals => Switch(pointCase: static _ => Seq<Interval>(), curveCase: static _ => Seq<Interval>(), overlapCase: static o => Seq(o.OverlapA, o.OverlapB));
    internal Unit Dispose() => Curves.Iter(static curve => curve.Dispose());
    public static IntersectionHit At(Point3d point) => new PointCase(point);
    public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new CurveCase(curve, kind);
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB, Option<Curve> curve = default) => new OverlapCase(start, end, overlapA, overlapB, curve);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance);
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult; public sealed record Lines(Seq<Line> Values) : IntersectionResult; public sealed record Circles(Seq<Circle> Values) : IntersectionResult; public sealed record Points(Seq<Point3d> Values) : IntersectionResult; public sealed record Intervals(Seq<Interval> Values) : IntersectionResult; public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult; public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
}
// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point = new(0, typeof(Point3d), Topology.Point), Line = new(1, typeof(Line), Topology.Curve), Polyline = new(2, typeof(Polyline), Topology.Curve), Circle = new(3, typeof(Circle), Topology.Curve), Arc = new(4, typeof(Arc), Topology.Curve), Ellipse = new(5, typeof(Ellipse), Topology.Curve);
    public static readonly Kind Curve = new(6, typeof(Curve), Topology.Curve), Surface = new(7, typeof(Surface), Topology.Surface), Plane = new(8, typeof(Plane), Topology.Surface), Sphere = new(9, typeof(Sphere), Topology.Surface), Cylinder = new(10, typeof(Cylinder), Topology.Surface), Cone = new(11, typeof(Cone), Topology.Surface), Torus = new(12, typeof(Torus), Topology.Surface);
    public static readonly Kind Brep = new(13, typeof(Brep), Topology.Brep), Box = new(14, typeof(Box), Topology.Brep), BoundingBox = new(15, typeof(BoundingBox), Topology.Brep), Mesh = new(16, typeof(Mesh), Topology.Mesh), SubD = new(17, typeof(SubD), Topology.SubD), PointCloud = new(18, typeof(PointCloud), Topology.PointCloud), Extrusion = new(19, typeof(Extrusion), Topology.Extrusion), Hatch = new(20, typeof(Hatch), Topology.Hatch);
    public Type Type { get; }
    public Topology Topology { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
[BoundaryAdapter]
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = Kind.Items.ToFrozenDictionary(static k => k.Type);
    internal static Option<Kind> Resolve(Type type) => type == typeof(Point) ? Some(Kind.Point) : Optional(ByType.GetValueOrDefault(type)) | (InheritsBase(type) is Type bt ? Optional(ByType.GetValueOrDefault(bt)) : Option<Kind>.None);
    internal static Type? InheritsBase(Type type) => type.BaseType is Type b ? (ByType.ContainsKey(b) ? b : InheritsBase(b)) : null;
}

// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class GeometryKernel {
    internal static bool CanBound(Type source, bool includeSphere) =>
        source != typeof(Plane) && (includeSphere || source != typeof(Sphere)) && (source == typeof(object) || source == typeof(GeometryBase) || typeof(GeometryBase).IsAssignableFrom(source) || KindLookup.Resolve(source).IsSome);
    internal static bool CanKind(Type source) => source == typeof(object) || source == typeof(GeometryBase) || KindLookup.Resolve(source).IsSome;
    internal static bool CanDecomposeFaces(Type type) =>
        type == typeof(object) || type == typeof(GeometryBase) || typeof(BrepFace).IsAssignableFrom(type) || typeof(Brep).IsAssignableFrom(type) || typeof(Surface).IsAssignableFrom(type) || typeof(Extrusion).IsAssignableFrom(type) || typeof(SubD).IsAssignableFrom(type);
    internal static bool CanReadVertices(Type type) =>
        type == typeof(object) || type == typeof(GeometryBase) || type == typeof(Point3d) || type == typeof(Line) || type == typeof(Polyline) || type == typeof(BoundingBox) || type == typeof(Box) || typeof(Point).IsAssignableFrom(type) || typeof(Curve).IsAssignableFrom(type) || typeof(Brep).IsAssignableFrom(type) || typeof(Mesh).IsAssignableFrom(type) || typeof(PointCloud).IsAssignableFrom(type) || typeof(SubD).IsAssignableFrom(type) || typeof(Surface).IsAssignableFrom(type) || typeof(Extrusion).IsAssignableFrom(type);
    internal static bool CanReadControlPoints(Type type) =>
        type == typeof(object) || typeof(Curve).IsAssignableFrom(type) || typeof(Surface).IsAssignableFrom(type) || typeof(Brep).IsAssignableFrom(type);
    internal static bool CanProjectCurves(Type type, Option<CurveFeature> feature = default) =>
        type == typeof(object)
        || (feature.Case switch {
            CurveFeature f => (type, f) switch {
                (Type t, CurveFeature.Input) when t == typeof(Line) || t == typeof(Circle) || t == typeof(Arc) || typeof(Curve).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Segment or CurveFeature.SubCurve) when t == typeof(Line) || t == typeof(Polyline) || t == typeof(Circle) || t == typeof(Arc) || typeof(Curve).IsAssignableFrom(t) || typeof(SubD).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Boundary) when t == typeof(Line) || t == typeof(Polyline) || t == typeof(Circle) || t == typeof(Arc) || typeof(Curve).IsAssignableFrom(t) || typeof(Brep).IsAssignableFrom(t) || typeof(BrepFace).IsAssignableFrom(t) || typeof(Mesh).IsAssignableFrom(t) || typeof(Surface).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Edge or CurveFeature.Interior or CurveFeature.NonManifold or CurveFeature.NakedOuter) when typeof(Brep).IsAssignableFrom(t) || typeof(Mesh).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.NakedInner or CurveFeature.OuterLoop or CurveFeature.InnerLoop) when typeof(Brep).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Iso) when typeof(Brep).IsAssignableFrom(t) || typeof(Surface).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Silhouette or CurveFeature.Draft) when typeof(Brep).IsAssignableFrom(t) || typeof(Mesh).IsAssignableFrom(t) || typeof(Extrusion).IsAssignableFrom(t) || typeof(Surface).IsAssignableFrom(t) || typeof(SubD).IsAssignableFrom(t) => true,
                (Type t, CurveFeature.Edge) when typeof(SubD).IsAssignableFrom(t) => true,
                _ => false,
            },
            _ => typeof(Curve).IsAssignableFrom(type) || type == typeof(Line) || type == typeof(Polyline) || type == typeof(Circle) || type == typeof(Arc) || typeof(Brep).IsAssignableFrom(type) || typeof(BrepFace).IsAssignableFrom(type) || typeof(Mesh).IsAssignableFrom(type) || typeof(Surface).IsAssignableFrom(type) || typeof(Extrusion).IsAssignableFrom(type) || typeof(SubD).IsAssignableFrom(type),
        });
    internal static bool CanCoerce(Type source, Type target) =>
        source == typeof(object) || source == typeof(GeometryBase) || target.IsAssignableFrom(source)
        || (target == typeof(Box) && typeof(Brep).IsAssignableFrom(source))
        || ((target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Ellipse) || target == typeof(Polyline)) && typeof(Curve).IsAssignableFrom(source))
        || ((target == typeof(Plane) || target == typeof(Sphere) || target == typeof(Cylinder) || target == typeof(Cone) || target == typeof(Torus)) && (typeof(Brep).IsAssignableFrom(source) || typeof(Surface).IsAssignableFrom(source)))
        || (target == typeof(Brep) && typeof(Extrusion).IsAssignableFrom(source));
    public static Fin<Kind> Kind(this object geometry, Context context) =>
        Optional(geometry).ToFin(Op.Of(name: nameof(Kind)).InvalidInput()).Bind(g =>
            (InferredKind(geometry: g, context: context) | KindLookup.Resolve(g.GetType()))
            .ToFin(Op.Of(name: nameof(Kind)).InvalidInput()));
    public static Fin<BoundingBox> Bounds(this object geometry, Op op) =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => OpAcceptance.ValidityOf(source: g).Case switch {
            false => Fin.Fail<BoundingBox>(op.InvalidInput()),
            true => g switch {
                BoundingBox box => Fin.Succ(box),
                Box box => Fin.Succ(box.BoundingBox),
                Sphere sphere => Fin.Succ(sphere.BoundingBox),
                Line line => Fin.Succ(line.BoundingBox),
                Polyline polyline => Fin.Succ(polyline.BoundingBox),
                Circle circle => Fin.Succ(circle.BoundingBox),
                Arc arc => Fin.Succ(arc.BoundingBox()),
                Point3d point => Fin.Succ(new BoundingBox(point, point)),
                Plane => Fin.Fail<BoundingBox>(op.Unsupported(typeof(Plane), typeof(BoundingBox))),
                Ellipse ellipse => Optional(ellipse.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static c => new Lease<Curve>.Owned(Value: c).Use(static d => d.GetBoundingBox(accurate: true))),
                Cylinder cylinder => Optional(cylinder.ToBrep(capBottom: true, capTop: true)).ToFin(op.InvalidResult()).Map(static b => new Lease<Brep>.Owned(Value: b).Use(static d => d.GetBoundingBox(accurate: true))),
                Cone cone => Optional(cone.ToBrep(capBottom: true)).ToFin(op.InvalidResult()).Map(static b => new Lease<Brep>.Owned(Value: b).Use(static d => d.GetBoundingBox(accurate: true))),
                Torus torus => Optional(torus.ToBrep()).ToFin(op.InvalidResult()).Map(static b => new Lease<Brep>.Owned(Value: b).Use(static d => d.GetBoundingBox(accurate: true))),
                GeometryBase native => native.IsValid ? Fin.Succ(native.GetBoundingBox(accurate: true)) : Fin.Fail<BoundingBox>(op.InvalidInput()),
                _ => Fin.Fail<BoundingBox>(op.Unsupported(g.GetType(), typeof(BoundingBox))),
            },
            _ => Fin.Fail<BoundingBox>(op.InvalidInput()),
        });
    public static Fin<TTarget> Coerce<TTarget>(object? source, Context context, Op op) where TTarget : notnull =>
        Optional(source).ToFin(op.InvalidInput()).Bind(s => s switch {
            TTarget target => op.AcceptValue(target),
            _ => CoerceValue(source: s, target: typeof(TTarget), context: context).ToFin(op.Unsupported(s.GetType(), typeof(TTarget))).Map(static v => (TTarget)v),
        });
    private static Option<Kind> InferredKind(object geometry, Context context) =>
        geometry switch {
            Brep brep => BoxOf(brep: brep, context: context).Map(static _ => global::Rasm.Domain.Kind.Box)
                | PlaneFromBrep(brep: brep, context: context).Map(static _ => global::Rasm.Domain.Kind.Surface)
                | PrimitiveSurfaceKind(surface: brep is { IsSurface: true } ? brep.Surfaces[0] : null, context: context),
            Curve curve => LineOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Line)
                | CircleOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Circle)
                | ArcOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Arc)
                | EllipseOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Ellipse)
                | PolylineOf(curve: curve).Map(static _ => global::Rasm.Domain.Kind.Polyline),
            Surface surface => PrimitiveSurfaceKind(surface: surface, context: context),
            _ => Option<Kind>.None,
        };
    private static Option<Kind> PrimitiveSurfaceKind(Surface? surface, Context context) =>
        Optional(surface).Bind(s =>
            PlaneOf(surface: s, context: context).Map(static _ => global::Rasm.Domain.Kind.Plane)
            | SphereOf(surface: s, context: context).Map(static _ => global::Rasm.Domain.Kind.Sphere)
            | CylinderOf(surface: s, context: context).Map(static _ => global::Rasm.Domain.Kind.Cylinder)
            | ConeOf(surface: s, context: context).Map(static _ => global::Rasm.Domain.Kind.Cone)
            | TorusOf(surface: s, context: context).Map(static _ => global::Rasm.Domain.Kind.Torus));
    private static Option<object> CoerceValue(object source, Type target, Context context) =>
        (source, target) switch {
            (Brep brep, Type t) when t == typeof(Box) => BoxOf(brep: brep, context: context).Map(static v => (object)v),
            (Curve curve, Type t) when t == typeof(Line) => LineOf(curve: curve, context: context).Map(static v => (object)v),
            (Curve curve, Type t) when t == typeof(Circle) => CircleOf(curve: curve, context: context).Map(static v => (object)v),
            (Curve curve, Type t) when t == typeof(Arc) => ArcOf(curve: curve, context: context).Map(static v => (object)v),
            (Curve curve, Type t) when t == typeof(Ellipse) => EllipseOf(curve: curve, context: context).Map(static v => (object)v),
            (Curve curve, Type t) when t == typeof(Polyline) => PolylineOf(curve: curve).Map(static v => (object)v),
            (Brep brep, Type t) when t == typeof(Plane) => PlaneFromBrep(brep: brep, context: context).Map(static v => (object)v),
            (Surface surface, Type t) when t == typeof(Plane) => PlaneOf(surface: surface, context: context).Map(static v => (object)v),
            (Brep brep, Type t) when t == typeof(Sphere) => brep is { IsSurface: true } ? SphereOf(surface: brep.Surfaces[0], context: context).Map(static v => (object)v) : Option<object>.None,
            (Surface surface, Type t) when t == typeof(Sphere) => SphereOf(surface: surface, context: context).Map(static v => (object)v),
            (Brep brep, Type t) when t == typeof(Cylinder) => brep is { IsSurface: true } ? CylinderOf(surface: brep.Surfaces[0], context: context).Map(static v => (object)v) : Option<object>.None,
            (Surface surface, Type t) when t == typeof(Cylinder) => CylinderOf(surface: surface, context: context).Map(static v => (object)v),
            (Brep brep, Type t) when t == typeof(Cone) => brep is { IsSurface: true } ? ConeOf(surface: brep.Surfaces[0], context: context).Map(static v => (object)v) : Option<object>.None,
            (Surface surface, Type t) when t == typeof(Cone) => ConeOf(surface: surface, context: context).Map(static v => (object)v),
            (Brep brep, Type t) when t == typeof(Torus) => brep is { IsSurface: true } ? TorusOf(surface: brep.Surfaces[0], context: context).Map(static v => (object)v) : Option<object>.None,
            (Surface surface, Type t) when t == typeof(Torus) => TorusOf(surface: surface, context: context).Map(static v => (object)v),
            (Extrusion extrusion, Type t) when t == typeof(Brep) => Optional(extrusion.ToBrep()).Map(static v => (object)v),
            _ => Option<object>.None,
        };
    private static Option<Box> BoxOf(Brep brep, Context context) =>
        brep.IsBox(context.Absolute.Value) && brep.Faces[0].UnderlyingSurface().TryGetPlane(out Plane plane, context.Absolute.Value) && new Box(plane, brep) is { IsValid: true } box ? Some(box) : Option<Box>.None;
    private static Option<Line> LineOf(Curve curve, Context context) => curve.IsLinear(context.Absolute.Value) ? Some(new Line(curve.PointAtStart, curve.PointAtEnd)) : Option<Line>.None;
    private static Option<Circle> CircleOf(Curve curve, Context context) => curve.TryGetCircle(out Circle value, context.Absolute.Value) ? Some(value) : Option<Circle>.None;
    private static Option<Arc> ArcOf(Curve curve, Context context) => curve.TryGetArc(out Arc value, context.Absolute.Value) ? Some(value) : Option<Arc>.None;
    private static Option<Ellipse> EllipseOf(Curve curve, Context context) => curve.TryGetEllipse(out Ellipse value, context.Absolute.Value) ? Some(value) : Option<Ellipse>.None;
    private static Option<Polyline> PolylineOf(Curve curve) => curve.TryGetPolyline(out Polyline value) ? Some(value) : Option<Polyline>.None;
    private static Option<Plane> PlaneFromBrep(Brep brep, Context context) => brep is { IsSurface: true } ? PlaneOf(surface: brep.Surfaces[0], context: context) : Option<Plane>.None;
    private static Option<Plane> PlaneOf(Surface surface, Context context) => surface.TryGetPlane(out Plane value, context.Absolute.Value) ? Some(value) : Option<Plane>.None;
    private static Option<Sphere> SphereOf(Surface surface, Context context) => surface.TryGetSphere(out Sphere value, context.Absolute.Value) ? Some(value) : Option<Sphere>.None;
    private static Option<Cylinder> CylinderOf(Surface surface, Context context) => surface.TryGetCylinder(out Cylinder value, context.Absolute.Value) ? Some(value) : Option<Cylinder>.None;
    private static Option<Cone> ConeOf(Surface surface, Context context) => surface.TryGetCone(out Cone value, context.Absolute.Value) ? Some(value) : Option<Cone>.None;
    private static Option<Torus> TorusOf(Surface surface, Context context) => surface.TryGetTorus(out Torus value, context.Absolute.Value) ? Some(value) : Option<Torus>.None;
    public static Fin<Seq<double>> Fractions(int count, Op op) => count switch { 1 => Fin.Succ(Seq(0.5)), > 1 => Fin.Succ(toSeq(Enumerable.Range(0, count).Select(i => i / (count - 1.0)))), _ => Fin.Fail<Seq<double>>(op.InvalidInput()) };
}
