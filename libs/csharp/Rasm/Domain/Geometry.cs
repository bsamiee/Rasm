using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
public enum CurveFeature { Input = 0, Segment = 1, Edge = 2, Boundary = 3, NakedOuter = 4, NakedInner = 5, Interior = 6, NonManifold = 7, OuterLoop = 8, InnerLoop = 9, Iso = 10, Silhouette = 11, SubCurve = 12, Draft = 13 }
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
    internal T Resource => Switch(owned: static owned => owned.Value, borrowed: static borrowed => borrowed.Value);
    internal Unit Dispose() => Switch(owned: static owned => { owned.Value.Dispose(); return unit; }, borrowed: static _ => unit);
}
[BoundaryAdapter]
public sealed record TopologyProjection {
    private static readonly Op Key = Op.Of(name: nameof(TopologyProjection));
    private readonly Lease<GeometryBase> value;
    private Option<Lease<Brep>> faceBrep;
    public GeometryBase Value => value.Resource;
    public CurveFeature Feature { get; }
    public ComponentIndex Source { get; }
    public bool Reversed { get; }
    private TopologyProjection(Lease<GeometryBase> value, CurveFeature feature, ComponentIndex source, bool reversed = false) { this.value = value; Feature = feature; Source = source; Reversed = reversed; }
    public int FaceIndex => Source.Index;
    public Option<T> As<T>() where T : class =>
        Value is T match ? Some(match)
        : (typeof(T), Value, faceBrep.Case) switch {
            (Type t, BrepFace, Lease<Brep> lease) when t == typeof(Brep) => Some((T)(object)lease.Resource),
            (Type t, BrepFace face, _) when t == typeof(Brep) => Optional(face.DuplicateFace(duplicateMeshes: false)).Map(brep => {
                faceBrep = new Lease<Brep>.Owned(Value: brep);
                return (T)(object)brep;
            }),
            _ => Option<T>.None,
        };
    internal Fin<T> OnMeshFace<T>(Func<Mesh, int, Fin<T>> use) where T : notnull => (Value, Source) switch { (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index }) when index >= 0 && index < mesh.Faces.Count => use(mesh, index), _ => Fin.Fail<T>(Key.InvalidInput()) };
    public static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndex source) { ArgumentNullException.ThrowIfNull(curve); return new(value: new Lease<GeometryBase>.Owned(Value: curve), feature: feature, source: source); }
    internal static TopologyProjection FromCurve(Curve curve, CurveFeature feature, ComponentIndexType type, int index) => FromCurve(curve, feature, new ComponentIndex(type, index));
    public static TopologyProjection FaceFrom(BrepFace face) { ArgumentNullException.ThrowIfNull(face); return new(value: new Lease<GeometryBase>.Borrowed(Value: face), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), reversed: face.OrientationIsReversed); }
    internal static TopologyProjection FaceCopyFrom(BrepFace face) { ArgumentNullException.ThrowIfNull(face); return new(value: new Lease<GeometryBase>.Owned(Value: face.DuplicateFace(duplicateMeshes: false)), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), reversed: face.OrientationIsReversed); }
    public static Fin<TopologyProjection> MeshFace(Mesh? mesh, int face) => Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => (native.Faces.Count, face) switch { ( <= 0, _) => Fin.Fail<TopologyProjection>(Key.InvalidResult()), (int count, int index) when index >= 0 && index < count => Fin.Succ<TopologyProjection>(new(value: new Lease<GeometryBase>.Borrowed(Value: native), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.MeshFace, index))), _ => Fin.Fail<TopologyProjection>(Key.InvalidInput()) });
    public Unit Dispose() {
        _ = value.Dispose();
        return faceBrep.Iter(static owned => owned.Dispose());
    }
    public TopologyProjection DetachFrom(GeometryBase source) {
        ArgumentNullException.ThrowIfNull(source);
        return (Value, source, Source) switch {
            (BrepFace face, _, _) when ReferenceEquals(objA: face.Brep, objB: source) => FaceCopyFrom(face),
            (Mesh mesh, Mesh owner, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face }) when ReferenceEquals(objA: mesh, objB: owner) && face >= 0 && face < mesh.Faces.Count =>
                new(value: new Lease<GeometryBase>.Owned(Value: mesh.DuplicateMesh()), feature: Feature, source: Source, reversed: Reversed),
            _ => this,
        };
    }
    public bool SameAs(TopologyProjection? other) => other switch { TopologyProjection p => ReferenceEquals(Value, p.Value) && Source.Equals(p.Source), _ => false };
    public bool Transfers(Type outputType) {
        ArgumentNullException.ThrowIfNull(outputType);
        return outputType.IsAssignableFrom(typeof(TopologyProjection))
            || (Value is Curve curve && outputType.IsAssignableFrom(curve.GetType()))
            || (Value is Brep or BrepFace && outputType.IsAssignableFrom(typeof(Brep)));
    }
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
        Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
        Fin<Seq<TValue>> result = project(arg: chosen);
        Type outputType = typeof(TValue);
        _ = all.Filter(value => !result.IsSucc || !chosen.Exists(c => c.SameAs(other: value) && c.Transfers(outputType: outputType))).Iter(static v => v.Dispose());
        return result;
    }
    public Fin<Point3d> Centroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => Centroid(context: ctx, key: Key));
    public Fin<Plane> FrameAtCentroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => FrameAtCentroid(context: ctx, key: Key));
    internal Fin<Point3d> Centroid(Context context, Op key) =>
        As<Brep>().ToFin(key.InvalidResult()).Bind(brep =>
            Optional(AreaMassProperties.Compute(brep: brep, area: true, firstMoments: true, secondMoments: false, productMoments: false, relativeTolerance: context.Fractional, absoluteTolerance: context.Absolute.Value))
                .ToFin(key.InvalidResult())
                .Map(static mass => new Lease<AreaMassProperties>.Owned(Value: mass).Use(static disposable => disposable.Centroid)));
    internal Fin<Plane> FrameAtCentroid(Context context, Op key) =>
        Face(key: key).Bind(brepFace =>
            Centroid(context: context, key: key)
                .Bind(centroid =>
                    brepFace.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: 0.0) switch {
                        true => (brepFace.FrameAt(u: u, v: v, frame: out Plane frame), brepFace.NormalAt(u: u, v: v)) switch {
                            (true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() => Fin.Succ((frame.ZAxis * (Reversed ? -normal : normal)) switch {
                                >= 0.0 => frame,
                                _ => new Plane(frame.Origin, frame.XAxis, -frame.YAxis),
                            }),
                            _ => Fin.Fail<Plane>(key.InvalidResult()),
                        },
                        false => Fin.Fail<Plane>(key.InvalidResult()),
                    }));
    internal Fin<Seq<Interval>> Domains(Op key) =>
        Face(key: key).Bind(brepFace => (brepFace.Domain(direction: 0), brepFace.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(u, v)),
            _ => Fin.Fail<Seq<Interval>>(key.InvalidResult()),
        });
    private Fin<BrepFace> Face(Op key) =>
        Value switch {
            BrepFace face => Fin.Succ(face),
            Brep { Faces.Count: > 0 } brep => Fin.Succ(brep.Faces[0]),
            _ => Fin.Fail<BrepFace>(key.InvalidInput()),
        };
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint) {
    internal static ClosestHit At(Point3d target, Point3d point, Option<Vector3d> normal = default, Option<ComponentIndex> component = default, Option<MeshPoint> meshPoint = default) =>
        new(Point: point, Distance: Some(target.DistanceTo(other: point)), Normal: normal, Component: component, MeshPoint: meshPoint);
}
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
    internal bool IsValid => Switch(
        pointCase: static p => p.Point.IsValid,
        curveCase: static c => c.CurveKind != IntersectionKind.Unknown && c.Curve.IsValid,
        overlapCase: static o => o.Start.IsValid && o.End.IsValid && o.OverlapA.IsValid && o.OverlapB.IsValid && o.Curve.Map(static c => c.IsValid).IfNone(true));
    internal Unit Dispose() => Curves.Iter(static curve => curve.Dispose());
    public static IntersectionHit At(Point3d point) => new PointCase(point);
    public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new CurveCase(curve, kind);
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB, Option<Curve> curve = default) => new OverlapCase(start, end, overlapA, overlapB, curve);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ResidualSample(int Index, Point3d Location, double Distance, double Tolerance, bool WithinTolerance);
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult; public sealed record Lines(Seq<Line> Values) : IntersectionResult; public sealed record Circles(Seq<Circle> Values) : IntersectionResult; public sealed record Points(Seq<Point3d> Values) : IntersectionResult; public sealed record Intervals(Seq<Interval> Values) : IntersectionResult; public sealed record Polylines(Seq<(Polyline Curve, IntersectionKind Kind)> Values) : IntersectionResult; public sealed record Hits(Seq<IntersectionHit> Values) : IntersectionResult;
    internal Fin<Seq<TOut>> Project<TOut>(Op key) => Switch(
        state: key,
        curves: static (k, c) => UniformAs<Curve, TOut>(values: c.Values, key: k, caseType: typeof(Curves), tag: IntersectionKind.Curve),
        lines: static (k, l) => UniformAs<Line, TOut>(values: l.Values, key: k, caseType: typeof(Lines), tag: IntersectionKind.Curve),
        circles: static (k, c) => UniformAs<Circle, TOut>(values: c.Values, key: k, caseType: typeof(Circles), tag: IntersectionKind.Curve),
        points: static (k, p) => UniformAs<Point3d, TOut>(values: p.Values, key: k, caseType: typeof(Points), tag: IntersectionKind.Point),
        intervals: static (k, i) => UniformAs<Interval, TOut>(values: i.Values, key: k, caseType: typeof(Intervals), tag: IntersectionKind.Overlap),
        polylines: static (k, p) => typeof(TOut) switch {
            Type t when t == typeof(Polyline) => k.AcceptResults<Polyline, TOut>(values: p.Values.Map(static x => x.Curve)),
            Type t when t == typeof(IntersectionKind) => k.AcceptResults<IntersectionKind, TOut>(values: p.Values.Map(static x => x.Kind)),
            _ => Fin.Fail<Seq<TOut>>(k.Unsupported(geometryType: typeof(Polylines), outputType: typeof(TOut))),
        },
        hits: static (k, h) => HitsAs<TOut>(hits: h.Values, key: k));
    private static Fin<Seq<TOut>> HitsAs<TOut>(Seq<IntersectionHit> hits, Op key) => typeof(TOut) switch {
        Type t when t == typeof(IntersectionHit) => key.AcceptResults<IntersectionHit, TOut>(values: hits),
        Type t when t == typeof(Curve) => key.AcceptResults<Curve, TOut>(values: hits.Bind(static value => value.Curves)),
        Type t when t == typeof(Point3d) => DropHitCurves(hits: hits, result: key.AcceptResults<Point3d, TOut>(values: hits.Bind(static value => value.Points))),
        Type t when t == typeof(Interval) => DropHitCurves(hits: hits, result: key.AcceptResults<Interval, TOut>(values: hits.Bind(static value => value.Intervals))),
        Type t when t == typeof(IntersectionKind) => DropHitCurves(hits: hits, result: key.AcceptResults<IntersectionKind, TOut>(values: hits.Map(static value => value.Kind))),
        _ => DropHitCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(Hits), outputType: typeof(TOut)))),
    };
    private static Fin<Seq<TOut>> DropHitCurves<TOut>(Seq<IntersectionHit> hits, Fin<Seq<TOut>> result) {
        _ = hits.Iter(static value => value.Dispose());
        return result;
    }
    private static Fin<Seq<TOut>> UniformAs<TNative, TOut>(Seq<TNative> values, Op key, Type caseType, IntersectionKind tag) where TNative : notnull => typeof(TOut) switch {
        Type t when t == typeof(TNative) => key.AcceptResults<TNative, TOut>(values: values),
        Type t when t == typeof(IntersectionKind) => key.AcceptResults<IntersectionKind, TOut>(values: toSeq(Enumerable.Repeat(element: tag, count: values.Count))),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: caseType, outputType: typeof(TOut))),
    };
}
// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point = new(0, typeof(Point3d), Topology.Point), Line = new(1, typeof(Line), Topology.Curve), Polyline = new(2, typeof(Polyline), Topology.Curve), Circle = new(3, typeof(Circle), Topology.Curve), Arc = new(4, typeof(Arc), Topology.Curve), Ellipse = new(5, typeof(Ellipse), Topology.Curve);
    public static readonly Kind Curve = new(6, typeof(Curve), Topology.Curve), Surface = new(7, typeof(Surface), Topology.Surface), Plane = new(8, typeof(Plane), Topology.Surface), Sphere = new(9, typeof(Sphere), Topology.Surface), Cylinder = new(10, typeof(Cylinder), Topology.Surface), Cone = new(11, typeof(Cone), Topology.Surface), Torus = new(12, typeof(Torus), Topology.Surface);
    public static readonly Kind Brep = new(13, typeof(Brep), Topology.Brep), Box = new(14, typeof(Box), Topology.Brep), BoundingBox = new(15, typeof(BoundingBox), Topology.Brep), Mesh = new(16, typeof(Mesh), Topology.Mesh), SubD = new(17, typeof(SubD), Topology.SubD), PointCloud = new(18, typeof(PointCloud), Topology.PointCloud), Extrusion = new(19, typeof(Extrusion), Topology.Extrusion), Hatch = new(20, typeof(Hatch), Topology.Hatch);
    public Type Type { get; }
    public Topology Topology { get; }
    internal bool CanBound(bool includeSphere) => Type != typeof(Plane) && (includeSphere || Type != typeof(Sphere));
    internal bool CanReadVertices =>
        Type == typeof(Point3d) || Type == typeof(Line) || Type == typeof(Polyline) || Topology is Topology.Point or Topology.Curve or Topology.Brep or Topology.Mesh or Topology.PointCloud or Topology.SubD or Topology.Surface or Topology.Extrusion;
    internal bool CanReadControlPoints => Topology is Topology.Curve or Topology.Surface or Topology.Brep;
    internal bool CanCoerceTo(Type target) =>
        target.IsAssignableFrom(Type)
        || (target == typeof(Point3d) && Type == typeof(Point3d))
        || (target == typeof(Box) && Type == typeof(Brep))
        || (target == typeof(Curve) && Topology == Topology.Curve)
        || ((target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Ellipse) || target == typeof(Polyline)) && Type == typeof(Curve))
        || ((target == typeof(Plane) || target == typeof(Sphere) || target == typeof(Cylinder) || target == typeof(Cone) || target == typeof(Torus)) && (Type == typeof(Brep) || Type == typeof(Surface)))
        || (target == typeof(Brep) && (Type == typeof(Brep) || Type == typeof(Surface) || Type == typeof(Box) || Type == typeof(BoundingBox) || Type == typeof(Sphere) || Type == typeof(Cylinder) || Type == typeof(Cone) || Type == typeof(Torus) || Type == typeof(Extrusion)));
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
        source == typeof(object) || source == typeof(GeometryBase) || typeof(GeometryBase).IsAssignableFrom(source) || KindLookup.Resolve(source).Map(kind => kind.CanBound(includeSphere: includeSphere)).IfNone(false);
    internal static bool CanKind(Type source) => source == typeof(object) || source == typeof(GeometryBase) || KindLookup.Resolve(source).IsSome;
    internal static bool CanDecomposeFaces(Type type) =>
        type == typeof(object) || type == typeof(GeometryBase) || typeof(BrepFace).IsAssignableFrom(c: type) || typeof(Brep).IsAssignableFrom(c: type) || typeof(Surface).IsAssignableFrom(c: type) || typeof(Extrusion).IsAssignableFrom(c: type);
    internal static bool CanReadVertices(Type type) =>
        type == typeof(object) || type == typeof(GeometryBase) || KindLookup.Resolve(type).Map(static kind => kind.CanReadVertices).IfNone(false);
    internal static bool CanReadControlPoints(Type type) =>
        type == typeof(object) || type == typeof(GeometryBase) || KindLookup.Resolve(type).Map(static kind => kind.CanReadControlPoints).IfNone(false);
    internal static bool CanCoerce(Type source, Type target) =>
        source == typeof(object) || source == typeof(GeometryBase) || KindLookup.Resolve(source).Map(kind => kind.CanCoerceTo(target: target)).IfNone(target.IsAssignableFrom(source));
    public static Fin<Kind> Kind(this object geometry, Context context) =>
        Optional(geometry).ToFin(Op.Of(name: nameof(Kind)).InvalidInput()).Bind(g =>
            (InferredKind(geometry: g, context: context) | NativeKind(geometry: g) | KindLookup.Resolve(g.GetType()))
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
                | PlaneFromBrep(brep: brep, context: context).Map(static _ => global::Rasm.Domain.Kind.Plane)
                | PrimitiveSurfaceKind(surface: brep is { IsSurface: true } ? brep.Surfaces[0] : null, context: context),
            Curve curve => LineOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Line)
                | CircleOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Circle)
                | ArcOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Arc)
                | EllipseOf(curve: curve, context: context).Map(static _ => global::Rasm.Domain.Kind.Ellipse)
                | PolylineOf(curve: curve).Map(static _ => global::Rasm.Domain.Kind.Polyline),
            Surface surface => PrimitiveSurfaceKind(surface: surface, context: context),
            _ => Option<Kind>.None,
        };
    private static Option<Kind> NativeKind(object geometry) =>
        geometry switch {
            GeometryBase native => native.ObjectType switch {
                Rhino.DocObjects.ObjectType.Point => Some(global::Rasm.Domain.Kind.Point),
                Rhino.DocObjects.ObjectType.Curve => Some(global::Rasm.Domain.Kind.Curve),
                Rhino.DocObjects.ObjectType.Surface => Some(global::Rasm.Domain.Kind.Surface),
                Rhino.DocObjects.ObjectType.Brep => Some(global::Rasm.Domain.Kind.Brep),
                Rhino.DocObjects.ObjectType.Mesh => Some(global::Rasm.Domain.Kind.Mesh),
                Rhino.DocObjects.ObjectType.SubD => Some(global::Rasm.Domain.Kind.SubD),
                Rhino.DocObjects.ObjectType.PointSet => Some(global::Rasm.Domain.Kind.PointCloud),
                Rhino.DocObjects.ObjectType.Hatch => Some(global::Rasm.Domain.Kind.Hatch),
                Rhino.DocObjects.ObjectType.Extrusion => Some(global::Rasm.Domain.Kind.Extrusion),
                _ when native.HasBrepForm => Some(global::Rasm.Domain.Kind.Brep),
                _ => Option<Kind>.None,
            },
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
            (Point point, Type t) when t == typeof(Point3d) => Some((object)point.Location),
            (Brep brep, Type t) when t == typeof(Box) => BoxOf(brep: brep, context: context).Map(static v => (object)v),
            (object value, Type t) when t == typeof(Curve) => CurveForm(source: value, op: Op.Of(name: nameof(Coerce))).ToOption().Map(static lease => (object)lease.Resource),
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
            (object value, Type t) when t == typeof(Brep) => BrepForm(source: value, op: Op.Of(name: nameof(Coerce))).ToOption().Map(static lease => (object)lease.Resource),
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
    internal static Fin<Lease<Curve>> CurveForm(object? source, Op op) =>
        Optional(source).ToFin(op.InvalidInput()).Bind(value => value switch {
            Curve curve => Fin.Succ<Lease<Curve>>(new Lease<Curve>.Borrowed(Value: curve)),
            Line line when line.IsValid => Fin.Succ<Lease<Curve>>(new Lease<Curve>.Owned(Value: new LineCurve(line))),
            Polyline polyline when polyline.IsValid => Optional(polyline.ToPolylineCurve()).ToFin(op.InvalidResult()).Map(static curve => (Lease<Curve>)new Lease<Curve>.Owned(Value: curve)),
            Circle circle when circle.IsValid => Fin.Succ<Lease<Curve>>(new Lease<Curve>.Owned(Value: new ArcCurve(circle))),
            Arc arc when arc.IsValid => Fin.Succ<Lease<Curve>>(new Lease<Curve>.Owned(Value: new ArcCurve(arc))),
            Ellipse ellipse when ellipse.IsValid => Optional(ellipse.ToNurbsCurve()).ToFin(op.InvalidResult()).Map(static curve => (Lease<Curve>)new Lease<Curve>.Owned(Value: curve)),
            _ => Fin.Fail<Lease<Curve>>(op.Unsupported(value.GetType(), typeof(Curve))),
        });
    internal static Fin<Lease<Brep>> BrepForm(object? source, Op op) =>
        Optional(source).ToFin(op.InvalidInput()).Bind(value => value switch {
            Brep brep => Fin.Succ<Lease<Brep>>(new Lease<Brep>.Borrowed(Value: brep)),
            GeometryBase { HasBrepForm: true } native => Optional(Brep.TryConvertBrep(native)).ToFin(op.InvalidResult()).Map(brep => ReferenceEquals(native, brep) ? (Lease<Brep>)new Lease<Brep>.Borrowed(Value: brep) : new Lease<Brep>.Owned(Value: brep)),
            Box box => Optional(box.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            BoundingBox box => Optional(box.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            Sphere sphere => Optional(sphere.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            Cylinder cylinder => Optional(cylinder.ToBrep(capBottom: true, capTop: true)).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            Cone cone => Optional(cone.ToBrep(capBottom: true)).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            Torus torus => Optional(torus.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            Extrusion extrusion => Optional(extrusion.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            _ => Fin.Fail<Lease<Brep>>(op.Unsupported(value.GetType(), typeof(Brep))),
        });
    public static Fin<Seq<double>> Fractions(int count, Op op) => count switch { 1 => Fin.Succ(Seq(0.5)), > 1 => Fin.Succ(toSeq(Enumerable.Range(0, count).Select(i => i / (count - 1.0)))), _ => Fin.Fail<Seq<double>>(op.InvalidInput()) };
    internal static Fin<Seq<double>> CurveSampleParameters(Curve curve, int count, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model =>
            Optional(curve).ToFin(key.InvalidInput()).Bind(native =>
                Fractions(count: count, op: key)
                    .Bind(fractions => Optional(native.NormalizedLengthParameters(s: [.. fractions.AsIterable()], absoluteTolerance: model.Absolute.Value, fractionalTolerance: model.Fractional))
                        .ToFin(key.InvalidResult())
                        .Map(static parameters => toSeq(parameters)))));
    internal static Fin<Point2d> SurfaceUv(Surface surface, Point2d uv, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model =>
            Optional(surface).ToFin(key.InvalidInput()).Bind(native =>
                (uv.IsValid, native.Domain(direction: 0), native.Domain(direction: 1)) switch {
                    (true, Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(t: uv.X) && v.IncludesParameter(t: uv.Y)
                        && (native is not BrepFace face || face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: model.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
                    _ => Fin.Fail<Point2d>(key.InvalidInput()),
                }));
    internal static Fin<Seq<Point2d>> SurfaceSampleUv(Surface surface, int resolution, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model =>
            Optional(surface).ToFin(key.InvalidInput()).Bind(native =>
                (native.Domain(direction: 0), native.Domain(direction: 1)) switch {
                    (Interval u, Interval v) when u.IsValid && v.IsValid =>
                        Fractions(count: resolution, op: key)
                            .Map(fractions => fractions.Bind(uf => fractions.Map(vf => new Point2d(x: u.ParameterAt(uf), y: v.ParameterAt(vf)))))
                            .Bind(samples => native switch {
                                BrepFace face => samples.Filter(uv => face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: model.Absolute.Value) != PointFaceRelation.Exterior) switch {
                                    Seq<Point2d> valid when !valid.IsEmpty => Fin.Succ(valid),
                                    _ => samples.Choose(uv => face.ClosestPointOnFace(testPoint: face.PointAt(u: uv.X, v: uv.Y), u: out double fu, v: out double fv, maximumDistance: 0.0) && face.IsPointOnFace(u: fu, v: fv, tolerance: model.Absolute.Value) != PointFaceRelation.Exterior ? Some(new Point2d(fu, fv)) : Option<Point2d>.None) switch {
                                        Seq<Point2d> valid when !valid.IsEmpty => Fin.Succ(valid),
                                        _ => Fin.Fail<Seq<Point2d>>(key.InvalidResult()),
                                    },
                                },
                                _ => Fin.Succ(samples),
                            }),
                    _ => Fin.Fail<Seq<Point2d>>(key.InvalidInput()),
                }));
    internal static Fin<Seq<Point3d>> SurfaceSamplePoints(Surface surface, int resolution, Context context, Op key) =>
        SurfaceSampleUv(surface: surface, resolution: resolution, context: context, key: key)
            .Map(uvs => uvs.Map(uv => surface.PointAt(u: uv.X, v: uv.Y)));
    internal static Fin<ClosestHit> Closest(object? geometry, Point3d target, Op key) =>
        from _ in guard(target.IsValid, key.InvalidInput())
        from g in Optional(geometry).ToFin(key.InvalidInput())
        from hit in g switch {
            Line line => line.ClosestPoint(testPoint: target, limitToFiniteSegment: true) switch {
                Point3d point => Fin.Succ(ClosestHit.At(target: target, point: point)),
            },
            Polyline polyline => polyline.ClosestPoint(testPoint: target) switch {
                Point3d point => Fin.Succ(ClosestHit.At(target: target, point: point)),
            },
            Curve curve => curve.ClosestPoint(testPoint: target, t: out double parameter) switch {
                true => curve.PointAt(t: parameter) switch {
                    Point3d point => Fin.Succ(ClosestHit.At(target: target, point: point)),
                },
                false => Fin.Fail<ClosestHit>(key.InvalidInput()),
            },
            BrepFace face => face.ClosestPointOnFace(testPoint: target, u: out double u, v: out double v, maximumDistance: 0.0) switch {
                true => face.PointAt(u: u, v: v) switch {
                    Point3d point => Fin.Succ(ClosestHit.At(target: target, point: point, normal: Some(face.NormalAt(u: u, v: v)), component: face.FaceIndex switch { >= 0 => Some(new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex)), _ => Option<ComponentIndex>.None })),
                },
                false => Fin.Fail<ClosestHit>(key.InvalidInput()),
            },
            Surface surface => surface.ClosestPoint(testPoint: target, u: out double u, v: out double v) switch {
                true => surface.PointAt(u: u, v: v) switch {
                    Point3d point => Fin.Succ(ClosestHit.At(target: target, point: point, normal: Some(surface.NormalAt(u: u, v: v)))),
                },
                false => Fin.Fail<ClosestHit>(key.InvalidInput()),
            },
            Brep brep => brep.ClosestPoint(target, out Point3d point, out ComponentIndex component, out double _, out double _, 0.0, out Vector3d normal) switch {
                true => Fin.Succ(ClosestHit.At(target: target, point: point, normal: Some(normal), component: Some(component))),
                false => Fin.Fail<ClosestHit>(key.InvalidInput()),
            },
            Mesh mesh => Optional(mesh.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0)).ToFin(key.InvalidResult()).Map(meshPoint => ClosestHit.At(target: target, point: meshPoint.Point, normal: Some(mesh.NormalAt(meshPoint: meshPoint)), meshPoint: Some(meshPoint))),
            _ => Fin.Fail<ClosestHit>(key.Unsupported(g.GetType(), typeof(ClosestHit))),
        }
        select hit;
}
