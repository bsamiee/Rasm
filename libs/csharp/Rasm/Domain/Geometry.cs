using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Topology {
    public static readonly Topology Unknown = new(0), Point = new(1), Curve = new(2), Surface = new(3), Brep = new(4), Mesh = new(5), SubD = new(6), PointCloud = new(7), Hatch = new(8), Extrusion = new(9);
}
[SmartEnum<int>]
public sealed partial class IntersectionKind {
    public static readonly IntersectionKind Unknown = new(0), Point = new(1), Overlap = new(2), Curve = new(3);
}
public enum IntersectionTangency { Unknown = 0, Transversal = 1, Tangent = 2 }
public enum CurveFeature { Input = 0, Segment = 1, Edge = 2, Boundary = 3, NakedOuter = 4, NakedInner = 5, Interior = 6, NonManifold = 7, OuterLoop = 8, InnerLoop = 9, Iso = 10, Silhouette = 11, SubCurve = 12, Draft = 13 }
[Union]
public partial record CurveForm {
    public sealed record LineCase(Line Value) : CurveForm;
    public sealed record CircleCase(Circle Value) : CurveForm;
    public sealed record ArcCase(Arc Value) : CurveForm;
    public sealed record EllipseCase(Ellipse Value) : CurveForm;
    public sealed record PolylineCase(Polyline Value, bool IsClosed) : CurveForm;
    public sealed record NurbsCase(int Degree, bool IsClosed, bool IsPlanar, bool IsPeriodic, int SpanCount, int Dimension) : CurveForm;
}

[Union]
internal abstract partial record Lease<T> where T : class, IDisposable {
    private Lease() { }
    public sealed record Owned(T Value) : Lease<T> {
        internal TResult Project<TResult>(Func<T, TResult> project) { using T owned = Value; return project(arg: owned); }
        internal TResult Project<TState, TResult>(TState state, Func<TState, T, TResult> project) { using T owned = Value; return project(arg1: state, arg2: owned); }
    }
    public sealed record Borrowed(T Value) : Lease<T>;
    internal TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => owned.Project(project: use), borrowed: static (use, borrowed) => use(arg: borrowed.Value));
    internal TResult Use<TState, TResult>(TState state, Func<TState, T, TResult> project) =>
        Switch(state: (State: state, Project: project), owned: static (use, owned) => owned.Project(state: use.State, project: use.Project), borrowed: static (use, borrowed) => use.Project(arg1: use.State, arg2: borrowed.Value));
    internal T Resource => Switch(owned: static owned => owned.Value, borrowed: static borrowed => borrowed.Value);
    internal Unit Dispose() => Switch(owned: static owned => { owned.Value.Dispose(); return unit; }, borrowed: static _ => unit);
}
[BoundaryAdapter]
[Union]
public abstract partial record IntersectionHit {
    private IntersectionHit() { }
    public sealed record PointCase(Point3d Point, IntersectionTangency Tangency = IntersectionTangency.Unknown) : IntersectionHit;
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
    internal static bool CanProjectTo(Type output) =>
        output == typeof(IntersectionHit) || output == typeof(Curve) || output == typeof(Point3d) || output == typeof(Interval) || output == typeof(IntersectionKind) || output == typeof(IntersectionTangency);
    internal static Fin<Seq<TOut>> Project<TOut>(Seq<IntersectionHit> hits, Op key) => typeof(TOut) switch {
        Type t when t == typeof(IntersectionHit) => key.AcceptResults<IntersectionHit, TOut>(values: hits),
        Type t when t == typeof(Curve) => key.AcceptResults<Curve, TOut>(values: hits.Bind(static value => value.Curves)),
        Type t when t == typeof(Point3d) => DropCurves(hits: hits, result: key.AcceptResults<Point3d, TOut>(values: hits.Bind(static value => value.Points))),
        Type t when t == typeof(Interval) => DropCurves(hits: hits, result: key.AcceptResults<Interval, TOut>(values: hits.Bind(static value => value.Intervals))),
        Type t when t == typeof(IntersectionKind) => DropCurves(hits: hits, result: key.AcceptResults<IntersectionKind, TOut>(values: hits.Map(static value => value.Kind))),
        Type t when t == typeof(IntersectionTangency) => DropCurves(hits: hits, result: key.AcceptResults<IntersectionTangency, TOut>(values: hits.Map(static value => value is IntersectionHit.PointCase pc ? pc.Tangency : IntersectionTangency.Unknown))),
        _ => DropCurves(hits: hits, result: Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(IntersectionHit), outputType: typeof(TOut)))),
    };
    private static Fin<Seq<TOut>> DropCurves<TOut>(Seq<IntersectionHit> hits, Fin<Seq<TOut>> result) {
        _ = hits.Iter(static value => value.Dispose());
        return result;
    }
    public static IntersectionHit At(Point3d point, IntersectionTangency tangency = IntersectionTangency.Unknown) => new PointCase(point, tangency);
    public static IntersectionHit Along(Curve curve, IntersectionKind kind) => new CurveCase(curve, kind);
    public static IntersectionHit Overlap(Point3d start, Point3d end, Interval overlapA, Interval overlapB, Option<Curve> curve = default) => new OverlapCase(start, end, overlapA, overlapB, curve);
}

[SmartEnum<int>]
public sealed partial class Kind {
    public static readonly Kind Point = new(0, typeof(Point3d), Topology.Point), Line = new(1, typeof(Line), Topology.Curve), Polyline = new(2, typeof(Polyline), Topology.Curve), Circle = new(3, typeof(Circle), Topology.Curve), Arc = new(4, typeof(Arc), Topology.Curve), Ellipse = new(5, typeof(Ellipse), Topology.Curve);
    public static readonly Kind Curve = new(6, typeof(Curve), Topology.Curve), Surface = new(7, typeof(Surface), Topology.Surface), Plane = new(8, typeof(Plane), Topology.Surface), Sphere = new(9, typeof(Sphere), Topology.Surface), Cylinder = new(10, typeof(Cylinder), Topology.Surface), Cone = new(11, typeof(Cone), Topology.Surface), Torus = new(12, typeof(Torus), Topology.Surface);
    public static readonly Kind Brep = new(13, typeof(Brep), Topology.Brep), Box = new(14, typeof(Box), Topology.Brep), BoundingBox = new(15, typeof(BoundingBox), Topology.Brep), Mesh = new(16, typeof(Mesh), Topology.Mesh), SubD = new(17, typeof(SubD), Topology.SubD), PointCloud = new(18, typeof(PointCloud), Topology.PointCloud), Extrusion = new(19, typeof(Extrusion), Topology.Extrusion), Hatch = new(20, typeof(Hatch), Topology.Hatch);
    public Type Type { get; }
    public Topology Topology { get; }
    internal bool CanBound(bool includeSphere) => Type != typeof(Plane) && (includeSphere || Type != typeof(Sphere));
    internal bool CanReadVertices =>
        Type == typeof(Point3d) || Type == typeof(Curve) || Type == typeof(Line) || Type == typeof(Polyline) || Type == typeof(Arc) || TopologyVertexReadable.Contains(Topology);
    internal bool CanReadControlPoints => TopologyControlReadable.Contains(Topology);
    internal bool CanReadEdges =>
        Type == typeof(Line) || Type == typeof(Polyline) || Type == typeof(BoundingBox) || Type == typeof(Box) || TopologyEdgeReadable.Contains(Topology);
    internal bool CanPrincipal => TopologyPrincipal.Contains(Topology);
    internal bool CanCoerceTo(Type target) =>
        target.IsAssignableFrom(Type)
        || (target == typeof(Box) && Type == typeof(Brep))
        || (target == typeof(Curve) && Topology == Topology.Curve)
        || (CurvePrimitives.Contains(target) && Type == typeof(Curve))
        || (SurfacePrimitives.Contains(target) && (Type == typeof(Brep) || Type == typeof(Surface)))
        || (target == typeof(Brep) && BrepSources.Contains(Type));
    private static readonly FrozenSet<Type> CurvePrimitives = new[] { typeof(Line), typeof(Circle), typeof(Arc), typeof(Ellipse), typeof(Polyline) }.ToFrozenSet();
    private static readonly FrozenSet<Type> SurfacePrimitives = new[] { typeof(Plane), typeof(Sphere), typeof(Cylinder), typeof(Cone), typeof(Torus) }.ToFrozenSet();
    private static readonly FrozenSet<Type> BrepSources = new[] { typeof(Brep), typeof(Surface), typeof(Box), typeof(BoundingBox), typeof(Sphere), typeof(Cylinder), typeof(Cone), typeof(Torus), typeof(Extrusion), typeof(SubD) }.ToFrozenSet();
    private static readonly FrozenSet<Topology> TopologyVertexReadable = new[] { Topology.Point, Topology.Brep, Topology.Mesh, Topology.PointCloud, Topology.SubD, Topology.Extrusion }.ToFrozenSet();
    private static readonly FrozenSet<Topology> TopologyControlReadable = new[] { Topology.Curve, Topology.Surface, Topology.Brep }.ToFrozenSet();
    private static readonly FrozenSet<Topology> TopologyEdgeReadable = new[] { Topology.Brep, Topology.Mesh, Topology.SubD }.ToFrozenSet();
    private static readonly FrozenSet<Topology> TopologyPrincipal = new[] { Topology.Curve, Topology.Brep, Topology.Mesh, Topology.Surface, Topology.Extrusion }.ToFrozenSet();
    private static readonly FrozenDictionary<Type, Kind> ByType = Items.ToFrozenDictionary(static k => k.Type);
    internal static readonly FrozenDictionary<Rhino.DocObjects.ObjectType, Kind> ByObjectType = new (Rhino.DocObjects.ObjectType Key, Kind Value)[] { (Rhino.DocObjects.ObjectType.Point, Point), (Rhino.DocObjects.ObjectType.Curve, Curve), (Rhino.DocObjects.ObjectType.Surface, Surface), (Rhino.DocObjects.ObjectType.Brep, Brep), (Rhino.DocObjects.ObjectType.Mesh, Mesh), (Rhino.DocObjects.ObjectType.SubD, SubD), (Rhino.DocObjects.ObjectType.PointSet, PointCloud), (Rhino.DocObjects.ObjectType.Hatch, Hatch), (Rhino.DocObjects.ObjectType.Extrusion, Extrusion) }.ToFrozenDictionary(keySelector: static p => p.Key, elementSelector: static p => p.Value);
    private static Type? InheritsBase(Type type) => type.BaseType is Type b ? (ByType.ContainsKey(key: b) ? b : InheritsBase(type: b)) : null;
    public static Option<Kind> Of(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type == typeof(Rhino.Geometry.Point) ? Some(Point) : Optional(ByType.GetValueOrDefault(key: type)) | (InheritsBase(type: type) is Type bt ? Optional(ByType.GetValueOrDefault(key: bt)) : Option<Kind>.None);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RayQuery(Ray3d Ray, int MaxReflections = 1) {
    public static RayQuery Of(Ray3d ray, int maxReflections = 1) => new(Ray: ray, MaxReflections: maxReflections);
    internal bool IsValid => Ray.Position.IsValid && Ray.Direction.IsValid && !Ray.Direction.IsTiny() && MaxReflections is >= 1 and <= 1000;
}
[BoundaryAdapter]
public sealed record TopologyProjection {
    private static readonly Op Key = Op.Of(name: nameof(TopologyProjection));
    private readonly Lease<GeometryBase> value;
    private readonly bool detachedSingleFace;
    private Option<Lease<Brep>> faceBrep;
    public GeometryBase Value => value.Resource;
    public CurveFeature Feature { get; }
    public ComponentIndex Source { get; }
    public bool Reversed { get; }
    private TopologyProjection(Lease<GeometryBase> value, CurveFeature feature, ComponentIndex source, bool reversed = false, bool detachedSingleFace = false) { this.value = value; this.detachedSingleFace = detachedSingleFace; Feature = feature; Source = source; Reversed = reversed; }
    internal bool HasValidSource => (Value, Source) switch {
        (Curve { IsValid: true }, _) => true,
        (Brep { IsValid: true, Faces.Count: int count }, { ComponentIndexType: ComponentIndexType.BrepFace, Index: int f }) => f >= 0 && (f < count || (detachedSingleFace && count == 1)),
        (BrepFace { IsValid: true } face, { ComponentIndexType: ComponentIndexType.BrepFace, Index: int f }) => f >= 0 && f == face.FaceIndex,
        (Mesh { IsValid: true } mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int f }) => f >= 0 && f < mesh.Faces.Count,
        (Mesh { IsValid: true } mesh, { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int n }) => n >= 0 && n < mesh.Ngons.Count,
        _ => false,
    };
    public Option<T> As<T>() where T : class =>
        Value is T match ? Some(match)
        : typeof(T) == typeof(BrepFace) && Value is Brep { Faces.Count: > 0 } brep && Source is { ComponentIndexType: ComponentIndexType.BrepFace, Index: int faceIndex } ? faceIndex switch {
            >= 0 when faceIndex < brep.Faces.Count => Some((T)(object)brep.Faces[faceIndex]),
            >= 0 when detachedSingleFace && brep.Faces.Count == 1 => Some((T)(object)brep.Faces[0]),
            _ => Option<T>.None,
        }
        : typeof(T) == typeof(Brep) && Value is BrepFace face
            ? faceBrep.Case switch {
                Lease<Brep> lease => Some((T)(object)lease.Resource),
                _ => Optional(face.DuplicateFace(duplicateMeshes: false)).Map(brep => { faceBrep = new Lease<Brep>.Owned(Value: brep); return (T)(object)brep; }),
            }
            : Option<T>.None;
    public static TopologyProjection Of(Curve curve, CurveFeature feature, ComponentIndex source) { ArgumentNullException.ThrowIfNull(curve); return new(value: new Lease<GeometryBase>.Owned(Value: curve), feature: feature, source: source); }
    public static TopologyProjection Of(BrepFace face, bool copy = false) {
        ArgumentNullException.ThrowIfNull(face);
        return new(value: copy ? new Lease<GeometryBase>.Owned(face.DuplicateFace(false)) : (Lease<GeometryBase>)new Lease<GeometryBase>.Borrowed(face), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), reversed: face.OrientationIsReversed, detachedSingleFace: copy);
    }
    public static Fin<TopologyProjection> FromMesh(Mesh? mesh, ComponentIndex source) =>
        Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => new TopologyProjection(value: new Lease<GeometryBase>.Borrowed(Value: native), feature: CurveFeature.Input, source: source) switch { { HasValidSource: true } projection => Fin.Succ(projection), _ => Fin.Fail<TopologyProjection>(Key.InvalidInput()) });
    public Unit Dispose() {
        _ = value.Dispose();
        return faceBrep.Iter(static owned => owned.Dispose());
    }
    public TopologyProjection DetachFrom(GeometryBase source) {
        ArgumentNullException.ThrowIfNull(source);
        return (Value, source) switch {
            (BrepFace face, _) when ReferenceEquals(face.Brep, source) => Of(face, copy: true),
            (Mesh mesh, Mesh owner) when ReferenceEquals(mesh, owner) && HasValidSource =>
                new(value: new Lease<GeometryBase>.Owned(Value: mesh.DuplicateMesh()), feature: Feature, source: Source, reversed: Reversed),
            _ => this,
        };
    }
    private bool SameAs(TopologyProjection? other) => other switch { TopologyProjection p => ReferenceEquals(Value, p.Value) && Source.Equals(p.Source), _ => false };
    public bool Transfers(Type outputType) {
        ArgumentNullException.ThrowIfNull(outputType);
        return outputType.IsAssignableFrom(typeof(TopologyProjection))
            || (Value is Curve curve && outputType.IsInstanceOfType(curve))
            || (Value is Brep or BrepFace && outputType.IsAssignableFrom(typeof(Brep)));
    }
    public bool Transfers(object? output) =>
        output switch {
            null => false,
            TopologyProjection projection => SameAs(other: projection),
            GeometryBase geometry => ReferenceEquals(objA: Value, objB: geometry) || (Value, geometry) switch {
                (Brep brep, BrepFace face) => ReferenceEquals(objA: brep, objB: face.Brep),
                (BrepFace face, Brep brep) => ReferenceEquals(objA: face.Brep, objB: brep),
                (BrepFace source, BrepFace face) => ReferenceEquals(objA: source.Brep, objB: face.Brep),
                _ => false,
            },
            _ => false,
        };
    internal static Fin<Seq<TValue>> Project<TValue>(Seq<TopologyProjection> all, Seq<TopologyProjection> chosen, Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
        Fin<Seq<TValue>> result = project(chosen);
        _ = all.Filter(v => !result.IsSucc || !chosen.Exists(c => c.SameAs(v) && c.Transfers(typeof(TValue)))).Iter(static v => v.Dispose());
        return result;
    }
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<double> Parameter, Option<Point2d> Uv, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint) {
    internal static ClosestHit At(Point3d target, Point3d point, Option<double> parameter = default, Option<Point2d> uv = default, Option<Vector3d> normal = default, Option<ComponentIndex> component = default, Option<MeshPoint> meshPoint = default) =>
        new(Point: point, Distance: Some(target.DistanceTo(other: point)), Parameter: parameter, Uv: uv, Normal: normal, Component: component, MeshPoint: meshPoint);
    internal bool IsValid => Point.IsValid
        && Distance.IsSome
        && Distance.Map(static d => RhinoMath.IsValidDouble(d) && d >= 0.0).IfNone(true)
        && Parameter.Map(static t => RhinoMath.IsValidDouble(t)).IfNone(true)
        && Uv.Map(static uv => uv.IsValid).IfNone(true)
        && Normal.Map(static n => n.IsValid && n.Length > RhinoMath.ZeroTolerance).IfNone(true)
        && Component.Map(static c => c is { ComponentIndexType: not ComponentIndexType.InvalidType } && c.Index >= 0).IfNone(true)
        && MeshPoint.Map(static m => m.Point.IsValid).IfNone(true);
    internal static bool CanProjectTo(Type output, bool parameterMode = false) =>
        output == typeof(ClosestHit) || output == typeof(double) || output == typeof(Point2d) || output == typeof(ComponentIndex) || output == typeof(MeshPoint)
        || (!parameterMode && (output == typeof(Point3d) || output == typeof(Vector3d)));
    internal Fin<Seq<TOut>> Project<TOut>(Op key, bool parameterMode = false) => typeof(TOut) switch {
        Type t when t == typeof(Point3d) && !parameterMode => key.AcceptResults<Point3d, TOut>(values: Seq(Point)),
        Type t when t == typeof(ClosestHit) => key.AcceptResults<ClosestHit, TOut>(values: Seq(this)),
        Type t when t == typeof(double) => (parameterMode ? Parameter : Distance).ToFin(Fail: key.InvalidResult()).Bind(value => key.AcceptResults<double, TOut>(values: Seq(value))),
        Type t when t == typeof(Point2d) => Uv.ToFin(Fail: key.InvalidResult()).Bind(uv => key.AcceptResults<Point2d, TOut>(values: Seq(uv))),
        Type t when t == typeof(Vector3d) && !parameterMode => Normal.ToFin(Fail: key.InvalidResult()).Bind(value => key.AcceptResults<Vector3d, TOut>(values: Seq(value))),
        Type t when t == typeof(ComponentIndex) => Component.ToFin(Fail: key.InvalidResult()).Bind(component => key.AcceptResults<ComponentIndex, TOut>(values: Seq(component))),
        Type t when t == typeof(MeshPoint) => MeshPoint.ToFin(Fail: key.InvalidResult()).Bind(meshPoint => key.AcceptResults<MeshPoint, TOut>(values: Seq(meshPoint))),
        _ => Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
    };
    internal Fin<double> SignedDistanceFrom(Point3d sample, Op key) {
        ClosestHit hit = this;
        return hit.Distance.ToFin(Fail: key.InvalidResult()).Bind(distance => hit.Normal.ToFin(Fail: key.InvalidResult()).Map(normal => ((sample - hit.Point) * normal) >= 0.0 ? distance : -distance));
    }
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Hit(int Id);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct Couple(int A, int B);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct CurveDeviation(double MinimumDistance, Point3d MinimumA, Point3d MinimumB, double MaximumDistance, Point3d MaximumA, Point3d MaximumB, double Tolerance, bool WithinTolerance);

// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class GeometryKernel {
    private static bool Universal(Type type) => type == typeof(object) || type == typeof(GeometryBase);
    internal static bool Can(Type type, Func<Kind, bool> predicate) {
        ArgumentNullException.ThrowIfNull(argument: type);
        ArgumentNullException.ThrowIfNull(argument: predicate);
        return Universal(type) || Kind.Of(type).Map(predicate).IfNone(false);
    }
    internal static bool CanBound(Type source, bool includeSphere) => Universal(source) || typeof(GeometryBase).IsAssignableFrom(source) || Kind.Of(source).Map(kind => kind.CanBound(includeSphere: includeSphere)).IfNone(false);
    internal static bool CanCurveForm(Type type) => typeof(Curve).IsAssignableFrom(c: type) || Can(type: type, predicate: static kind => kind.Topology == Topology.Curve);
    internal static bool CanSurfaceForm(Type type) => Universal(type: type) || typeof(Surface).IsAssignableFrom(c: type) || Can(type: type, predicate: static kind => kind.Topology == Topology.Surface);
    internal static bool CanCoerce(Type source, Type target) => Universal(source) || Kind.Of(source).Map(kind => kind.CanCoerceTo(target: target)).IfNone(target.IsAssignableFrom(source));
    internal static bool CanDecomposeFaces(Type type) => typeof(BrepFace).IsAssignableFrom(c: type) || Can(type: type, predicate: static kind => kind.CanCoerceTo(target: typeof(Brep)));
    internal static bool CanEvaluateTopology(Type type) => typeof(Mesh).IsAssignableFrom(c: type) || typeof(Brep).IsAssignableFrom(c: type) || Can(type: type, predicate: static kind => kind.Topology == Topology.Mesh || kind.Topology == Topology.Brep || kind.CanCoerceTo(target: typeof(Brep)));
    internal static bool CanEvaluateSolidTopology(Type type) => typeof(Mesh).IsAssignableFrom(c: type) || typeof(Brep).IsAssignableFrom(c: type) || Can(type: type, predicate: static kind => kind.Topology == Topology.Mesh || kind.Topology == Topology.Brep || kind.Type == typeof(Box) || kind.Type == typeof(BoundingBox) || kind.Topology == Topology.Extrusion || kind.Topology == Topology.SubD);
    internal static bool CanClosest(Type type) => Universal(type: type) || typeof(Brep).IsAssignableFrom(type) || typeof(Mesh).IsAssignableFrom(type) || type == typeof(Box) || type == typeof(BoundingBox) || CanCurveForm(type: type) || CanSurfaceForm(type: type);
    internal static bool CanClosestNormal(Type type) => Universal(type: type) || type == typeof(Plane) || typeof(Surface).IsAssignableFrom(c: type) || typeof(BrepFace).IsAssignableFrom(c: type) || typeof(Brep).IsAssignableFrom(c: type) || typeof(Mesh).IsAssignableFrom(c: type);
    internal static bool CanReadVertices(Type type) => Can(type: type, predicate: static kind => kind.CanReadVertices);
    internal static bool CanSamplePoints(Type type) => CanCurveForm(type: type) || CanSurfaceForm(type: type) || CanReadVertices(type: type);
    public static Fin<Kind> KindOf(this object geometry, Context context) {
        Op key = Op.Of(name: nameof(Kind));
        return Optional(geometry).ToFin(key.InvalidInput()).Bind(g =>
            (InferredKind(geometry: g, context: context, key: key) | NativeKind(geometry: g) | Kind.Of(g.GetType()))
            .ToFin(key.InvalidInput()));
    }
    public static Fin<BoundingBox> BoundsOf(this object geometry, Op op) =>
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
                Ellipse => CurveForm(source: g, op: op).Map(static lease => lease.Use(static d => d.GetBoundingBox(accurate: true))),
                Cylinder or Cone or Torus => BrepForm(source: g, op: op).Map(static lease => lease.Use(static d => d.GetBoundingBox(accurate: true))),
                GeometryBase native => native.IsValid ? Fin.Succ(native.GetBoundingBox(accurate: true)) : Fin.Fail<BoundingBox>(op.InvalidInput()),
                _ => Fin.Fail<BoundingBox>(op.Unsupported(g.GetType(), typeof(BoundingBox))),
            },
            _ => Fin.Fail<BoundingBox>(op.InvalidInput()),
        });
    public static Fin<TTarget> CoerceTo<TTarget>(object? source, Context context, Op op) where TTarget : notnull =>
        Optional(source).ToFin(op.InvalidInput()).Bind(s => s switch {
            TTarget target => op.AcceptValue(target),
            _ => Kind.Of(typeof(TTarget)).Bind(kind => PrimitiveOf(kind, s, context, Op.Of(name: nameof(CoerceTo)))).ToFin(op.Unsupported(s.GetType(), typeof(TTarget))).Map(static v => (TTarget)v),
        });
    private static Option<Kind> InferredKind(object geometry, Context context, Op key) =>
        (geometry switch {
            Brep => Seq(Kind.Box, Kind.Plane, Kind.Sphere, Kind.Cylinder, Kind.Cone, Kind.Torus),
            Curve => Seq(Kind.Line, Kind.Circle, Kind.Arc, Kind.Ellipse, Kind.Polyline),
            Surface => Seq(Kind.Plane, Kind.Sphere, Kind.Cylinder, Kind.Cone, Kind.Torus),
            _ => Seq<Kind>(),
        }).Choose(kind => PrimitiveOf(kind, geometry, context, key).Map(_ => kind)).Head;
    private static Option<Kind> NativeKind(object geometry) =>
        geometry is GeometryBase native
            ? Optional(Kind.ByObjectType.GetValueOrDefault(native.ObjectType)) | (native.HasBrepForm ? Some(Kind.Brep) : Option<Kind>.None)
            : Option<Kind>.None;
    private static Option<object> PrimitiveOf(Kind kind, object source, Context context, Op op) =>
        (kind.Type, source) switch {
            (Type t, Point point) when t == typeof(Point3d) => Some((object)point.Location),
            (Type t, Brep brep) when t == typeof(Box) => brep.IsBox(context.Absolute.Value) && brep.Faces[0].UnderlyingSurface().TryGetPlane(out Plane plane, context.Absolute.Value) && new Box(plane, brep) is { IsValid: true } box ? Some((object)box) : Option<object>.None,
            (Type t, object value) when t == typeof(Curve) => CurveForm(source: value, op: op).ToOption().Map(static lease => (object)lease.Resource),
            (Type t, Curve curve) when t == typeof(Line) || t == typeof(Circle) || t == typeof(Arc) || t == typeof(Ellipse) || t == typeof(Polyline) => CurveFormOf(curve: curve, context: context, op: op).ToOption().Bind(form => (t, form) switch {
                (Type output, CurveForm.LineCase line) when output == typeof(Line) => Some((object)line.Value),
                (Type output, CurveForm.CircleCase circle) when output == typeof(Circle) => Some((object)circle.Value),
                (Type output, CurveForm.ArcCase arc) when output == typeof(Arc) => Some((object)arc.Value),
                (Type output, CurveForm.EllipseCase ellipse) when output == typeof(Ellipse) => Some((object)ellipse.Value),
                (Type output, CurveForm.PolylineCase polyline) when output == typeof(Polyline) => Some((object)polyline.Value),
                _ => Option<object>.None,
            }),
            (Type t, Brep { IsSurface: true, Faces.Count: > 0 } brep) when t == typeof(Plane) || t == typeof(Sphere) || t == typeof(Cylinder) || t == typeof(Cone) || t == typeof(Torus) => PrimitiveOf(kind, brep.Faces[0], context, op),
            (Type t, Surface surface) when t == typeof(Plane) && surface.TryGetPlane(out Plane value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Sphere) && surface.TryGetSphere(out Sphere value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Cylinder) && surface.TryGetFiniteCylinder(out Cylinder value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Cone) && surface.TryGetCone(out Cone value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Torus) && surface.TryGetTorus(out Torus value, context.Absolute.Value) => Some((object)value),
            (Type t, object value) when t == typeof(Brep) => BrepForm(source: value, op: op).ToOption().Map(static lease => (object)lease.Resource),
            _ => Option<object>.None,
        };
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
    internal static Fin<Lease<Surface>> SurfaceForm(object? source, Op op) =>
        Optional(source).ToFin(op.InvalidInput()).Bind(value => value switch {
            Surface surface => Fin.Succ<Lease<Surface>>(new Lease<Surface>.Borrowed(Value: surface)),
            Plane plane when plane.IsValid => Fin.Succ<Lease<Surface>>(new Lease<Surface>.Owned(Value: new PlaneSurface(plane))),
            Sphere sphere when sphere.IsValid => Optional(sphere.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static surface => (Lease<Surface>)new Lease<Surface>.Owned(Value: surface)),
            Cylinder cylinder when cylinder.IsValid => Optional(cylinder.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static surface => (Lease<Surface>)new Lease<Surface>.Owned(Value: surface)),
            Cone cone when cone.IsValid => Optional(cone.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static surface => (Lease<Surface>)new Lease<Surface>.Owned(Value: surface)),
            Torus torus when torus.IsValid => Optional(torus.ToNurbsSurface()).ToFin(op.InvalidResult()).Map(static surface => (Lease<Surface>)new Lease<Surface>.Owned(Value: surface)),
            Brep { IsSurface: true, Faces.Count: > 0 } brep => Fin.Succ<Lease<Surface>>(new Lease<Surface>.Borrowed(Value: brep.Faces[0])),
            _ => Fin.Fail<Lease<Surface>>(op.Unsupported(value.GetType(), typeof(Surface))),
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
            SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Map(static brep => (Lease<Brep>)new Lease<Brep>.Owned(Value: brep)),
            _ => Fin.Fail<Lease<Brep>>(op.Unsupported(value.GetType(), typeof(Brep))),
        });
    private static Fin<Seq<double>> Fractions(int count, Op op) => count switch { 1 => Fin.Succ(Seq(0.5)), > 1 => Fin.Succ(toSeq(Enumerable.Range(0, count).Select(i => i / (count - 1.0)))), _ => Fin.Fail<Seq<double>>(op.InvalidInput()) };
    internal static Fin<Seq<double>> CurveSampleParameters(Curve curve, int count, Context context, Op key) =>
        Fractions(count: count, op: key).Bind(fractions =>
            Optional(curve.NormalizedLengthParameters([.. fractions.AsIterable()], context.Absolute.Value, context.Fractional)).ToFin(key.InvalidResult()).Map(static p => toSeq(p)));
    internal static Fin<CurveForm> CurveFormOf(Curve curve, Context context, Op op) =>
        Fin.Succ<CurveForm>(curve switch {
            _ when curve.IsLinear(tolerance: context.Absolute.Value) => new CurveForm.LineCase(Value: new Line(from: curve.PointAtStart, to: curve.PointAtEnd)),
            _ when curve.TryGetCircle(circle: out Circle c, tolerance: context.Absolute.Value) => new CurveForm.CircleCase(Value: c),
            _ when curve.TryGetArc(arc: out Arc a, tolerance: context.Absolute.Value) => new CurveForm.ArcCase(Value: a),
            _ when curve.TryGetEllipse(ellipse: out Ellipse e, tolerance: context.Absolute.Value) => new CurveForm.EllipseCase(Value: e),
            _ when curve.TryGetPolyline(polyline: out Polyline p) => new CurveForm.PolylineCase(Value: p, IsClosed: curve.IsClosed),
            _ => new CurveForm.NurbsCase(Degree: curve.Degree, IsClosed: curve.IsClosed, IsPlanar: curve.IsPlanar(tolerance: context.Absolute.Value), IsPeriodic: curve.IsPeriodic, SpanCount: curve.SpanCount, Dimension: curve.Dimension),
        });
    internal static Fin<Seq<Point3d>> VerticesOf(object? source, Op key) =>
        Optional(source).ToFin(key.InvalidInput()).Bind(value => value switch {
            Point3d point => Fin.Succ(Seq(point)),
            Point point => Fin.Succ(Seq(point.Location)),
            Line line => Fin.Succ(Seq(line.From, line.To)),
            Arc arc => Fin.Succ(Seq(arc.StartPoint, arc.EndPoint)),
            Polyline polyline => Fin.Succ(toSeq(polyline)),
            BoundingBox box => Fin.Succ(toSeq(box.GetCorners())),
            Box box => Fin.Succ(toSeq(box.GetCorners())),
            Curve curve when curve.TryGetPolyline(polyline: out Polyline poly) => Fin.Succ(toSeq(poly)),
            object curveLike when CanCurveForm(type: curveLike.GetType()) => CurveForm(source: curveLike, op: key).Bind(lease => lease.Use(curve => VerticesOf(source: curve, key: key))),
            Brep brep => Fin.Succ(toSeq(brep.DuplicateVertices())),
            Mesh mesh => Fin.Succ(toSeq(mesh.Vertices.ToPoint3dArray())),
            PointCloud cloud => Fin.Succ(toSeq(cloud.GetPoints())),
            SubD subd => Fin.Succ(toSeq(LanguageExt.List.unfold((SubDVertex?)subd.Vertices.First, static vertex => vertex switch { SubDVertex current => Some((current.ControlNetPoint, (SubDVertex?)current.Next)), _ => None }))),
            GeometryBase { HasBrepForm: true } native => BrepForm(source: native, op: key).Bind(lease => lease.Use(brep => VerticesOf(source: brep, key: key))),
            _ => Fin.Fail<Seq<Point3d>>(key.Unsupported(value.GetType(), typeof(Point3d))),
        });
    internal static Fin<Seq<Point3d>> SamplePoints(object? source, int count, Context context, Op key) =>
        guard(count > 0, key.InvalidInput()).Bind(_ => Optional(source).ToFin(key.InvalidInput()).Bind(value => value switch {
            Curve curve => CurveSampleParameters(curve: curve, count: count, context: context, key: key).Map(parameters => parameters.Map(curve.PointAt)),
            object curveLike when CanCurveForm(type: curveLike.GetType()) => CurveForm(source: curveLike, op: key).Bind(lease => lease.Use(curve => SamplePoints(source: curve, count: count, context: context, key: key))),
            Surface surface => SurfaceSamplePoints(surface: surface, count: count, context: context, key: key),
            object surfaceLike when CanSurfaceForm(type: surfaceLike.GetType()) => SurfaceForm(source: surfaceLike, op: key).Bind(lease => lease.Use(surface => SurfaceSamplePoints(surface: surface, count: count, context: context, key: key))),
            object vertexLike when CanReadVertices(type: vertexLike.GetType()) => VerticesOf(source: vertexLike, key: key),
            _ => Fin.Fail<Seq<Point3d>>(key.Unsupported(value.GetType(), typeof(Point3d))),
        }));
    internal static Fin<Point2d> SurfaceUv(Surface surface, Point2d uv, Context context, Op key) =>
        (uv.IsValid, surface.Domain(0), surface.Domain(1)) switch {
            (true, Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(uv.X) && v.IncludesParameter(uv.Y)
                && (surface is not BrepFace face || face.IsPointOnFace(uv.X, uv.Y, context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
    internal static Fin<Seq<Point2d>> SurfaceSampleUv(Surface surface, int count, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model =>
        Optional(surface).ToFin(key.InvalidInput()).Bind(native => (native.Domain(0), native.Domain(1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid =>
                Fractions(count: count, op: key)
                    .Map(fractions => fractions.Bind(uf => fractions.Map(vf => new Point2d(u.ParameterAt(uf), v.ParameterAt(vf)))))
                    .Bind(samples => (native, model.Absolute.Value) switch {
                        (BrepFace face, double tolerance) => samples.Choose(uv =>
                            face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: tolerance) != PointFaceRelation.Exterior ? Some(uv)
                            : face.ClosestPointOnFace(testPoint: face.PointAt(u: uv.X, v: uv.Y), u: out double fu, v: out double fv, maximumDistance: 0.0)
                                && face.IsPointOnFace(u: fu, v: fv, tolerance: tolerance) != PointFaceRelation.Exterior ? Some(new Point2d(fu, fv)) : Option<Point2d>.None) switch {
                                    Seq<Point2d> valid when !valid.IsEmpty => Fin.Succ(valid),
                                    _ => Fin.Fail<Seq<Point2d>>(key.InvalidResult()),
                                },
                        _ => Fin.Succ(samples),
                    }),
            _ => Fin.Fail<Seq<Point2d>>(key.InvalidInput()),
        }));
    internal static Fin<Seq<Point3d>> SurfaceSamplePoints(Surface surface, int count, Context context, Op key) =>
        SurfaceSampleUv(surface: surface, count: count, context: context, key: key)
            .Map(uvs => uvs.Map(uv => surface.PointAt(u: uv.X, v: uv.Y)));
    internal static Fin<Vector3d> NormalAt(Surface surface, Point2d uv, Op key) =>
        surface.NormalAt(u: uv.X, v: uv.Y) switch {
            Vector3d normal when normal.IsValid && !normal.IsTiny() => Fin.Succ(surface is BrepFace { OrientationIsReversed: true } ? -normal : normal),
            _ => Fin.Fail<Vector3d>(key.InvalidResult()),
        };
    internal static Fin<Plane> FrameAt(Surface surface, Point2d uv, Op key) =>
        (surface.FrameAt(u: uv.X, v: uv.Y, frame: out Plane frame), frame) switch {
            (true, { IsValid: true } native) => NormalAt(surface: surface, uv: uv, key: key).Bind(normal =>
                Fin.Succ((native.ZAxis * normal) >= 0.0 ? native : new Plane(origin: native.Origin, xDirection: native.XAxis, yDirection: -native.YAxis))),
            _ => Fin.Fail<Plane>(key.InvalidResult()),
        };
    internal static Fin<ClosestHit> ClosestOf(object? geometry, Point3d target, Op key) =>
        from _ in guard(target.IsValid, key.InvalidInput())
        from g in Optional(geometry).ToFin(key.InvalidInput())
        from hit in g switch {
            Line line => Fin.Succ(ClosestHit.At(target: target, point: line.ClosestPoint(testPoint: target, limitToFiniteSegment: true), parameter: Some(Math.Clamp(line.ClosestParameter(testPoint: target), 0.0, 1.0)))),
            Polyline polyline => Fin.Succ(ClosestHit.At(target: target, point: polyline.ClosestPoint(testPoint: target), parameter: Some(polyline.ClosestParameter(testPoint: target)))),
            Plane plane when plane.ClosestParameter(testPoint: target, s: out double s, t: out double t) => Fin.Succ(ClosestHit.At(target: target, point: plane.PointAt(u: s, v: t), uv: Some(new Point2d(x: s, y: t)), normal: Some(plane.Normal))),
            Sphere sphere => Fin.Succ(ClosestHit.At(target: target, point: sphere.ClosestPoint(testPoint: target))),
            Box box => Fin.Succ(ClosestHit.At(target: target, point: box.ClosestPoint(target, false))),
            BoundingBox box => Fin.Succ(ClosestHit.At(target: target, point: box.ClosestPoint(target, false))),
            Curve curve when curve.ClosestPoint(testPoint: target, t: out double parameter) =>
                Fin.Succ(ClosestHit.At(target: target, point: curve.PointAt(t: parameter), parameter: Some(parameter))),
            BrepFace face when face.ClosestPointOnFace(testPoint: target, u: out double u, v: out double v, maximumDistance: 0.0) =>
                NormalAt(surface: face, uv: new Point2d(x: u, y: v), key: key).Map(normal =>
                    ClosestHit.At(target: target, point: face.PointAt(u: u, v: v), uv: Some(new Point2d(x: u, y: v)), normal: Some(normal),
                        component: face.FaceIndex >= 0 ? Some(new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex)) : Option<ComponentIndex>.None)),
            Surface surface when surface.ClosestPoint(testPoint: target, u: out double u, v: out double v) =>
                NormalAt(surface: surface, uv: new Point2d(x: u, y: v), key: key).Map(normal =>
                    ClosestHit.At(target: target, point: surface.PointAt(u: u, v: v), uv: Some(new Point2d(x: u, y: v)), normal: Some(normal))),
            Brep brep when brep.ClosestPoint(target, out Point3d point, out ComponentIndex component, out double u, out double v, 0.0, out Vector3d _) =>
                component switch {
                    { ComponentIndexType: ComponentIndexType.BrepFace, Index: int faceIndex } when faceIndex >= 0 && faceIndex < brep.Faces.Count =>
                        NormalAt(surface: brep.Faces[faceIndex], uv: new Point2d(x: u, y: v), key: key).Map(oriented =>
                            ClosestHit.At(target: target, point: point, uv: Some(new Point2d(x: u, y: v)), normal: Some(oriented), component: Some(component))),
                    { ComponentIndexType: ComponentIndexType.BrepEdge } =>
                        Fin.Succ(ClosestHit.At(target: target, point: point, parameter: Some(u), component: Some(component))),
                    _ => Fin.Succ(ClosestHit.At(target: target, point: point, component: Some(component))),
                },
            Mesh mesh => Optional(mesh.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0)).ToFin(key.InvalidResult())
                .Map(meshPoint => ClosestHit.At(target: target, point: meshPoint.Point, normal: Some(mesh.NormalAt(meshPoint: meshPoint)), component: Some(meshPoint.ComponentIndex), meshPoint: Some(meshPoint))),
            object curveLike when CanCurveForm(type: curveLike.GetType()) =>
                CurveForm(source: curveLike, op: key).Bind(lease => lease.Use(curve => ClosestOf(geometry: curve, target: target, key: key))),
            object surfaceLike when CanSurfaceForm(type: surfaceLike.GetType()) =>
                SurfaceForm(source: surfaceLike, op: key).Bind(lease => lease.Use(surface => ClosestOf(geometry: surface, target: target, key: key))),
            Curve or BrepFace or Surface or Brep => Fin.Fail<ClosestHit>(key.InvalidInput()),
            _ => Fin.Fail<ClosestHit>(key.Unsupported(g.GetType(), typeof(ClosestHit))),
        }
        select hit;
}
