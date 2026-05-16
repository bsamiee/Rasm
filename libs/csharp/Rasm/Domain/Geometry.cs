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
        internal TResult Project<TResult>(Func<T, TResult> project) { using T owned = Value; return project(arg: owned); }
    }
    public sealed record Borrowed(T Value) : Lease<T>;
    internal TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => owned.Project(project: use), borrowed: static (use, borrowed) => use(arg: borrowed.Value));
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
        : typeof(T) == typeof(Brep) && Value is BrepFace face
            ? faceBrep.Case switch {
                Lease<Brep> lease => Some((T)(object)lease.Resource),
                _ => Optional(face.DuplicateFace(duplicateMeshes: false)).Map(brep => { faceBrep = new Lease<Brep>.Owned(Value: brep); return (T)(object)brep; }),
            }
            : Option<T>.None;
    public static TopologyProjection Of(Curve curve, CurveFeature feature, ComponentIndex source) { ArgumentNullException.ThrowIfNull(curve); return new(value: new Lease<GeometryBase>.Owned(Value: curve), feature: feature, source: source); }
    public static TopologyProjection Of(BrepFace face, bool copy = false) {
        ArgumentNullException.ThrowIfNull(face);
        return new(value: copy ? new Lease<GeometryBase>.Owned(face.DuplicateFace(false)) : (Lease<GeometryBase>)new Lease<GeometryBase>.Borrowed(face), feature: CurveFeature.Input, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), reversed: face.OrientationIsReversed);
    }
    public static Fin<TopologyProjection> OfMesh(Mesh? mesh, ComponentIndex source) =>
        Optional(mesh).ToFin(Key.InvalidInput()).Bind(native => source switch {
            { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < native.Faces.Count =>
                Fin.Succ<TopologyProjection>(new(value: new Lease<GeometryBase>.Borrowed(Value: native), feature: CurveFeature.Input, source: source)),
            { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < native.Ngons.Count =>
                Fin.Succ<TopologyProjection>(new(value: new Lease<GeometryBase>.Borrowed(Value: native), feature: CurveFeature.Input, source: source)),
            _ => Fin.Fail<TopologyProjection>(Key.InvalidInput()),
        });
    public static Fin<TopologyProjection> OfMesh(Mesh? mesh, MeshNgon polygon) =>
        Optional(mesh).ToFin(Key.InvalidInput()).Bind(native =>
            Optional(polygon.BoundaryVertexIndexList()).Filter(static vertices => vertices.Length >= 3).ToFin(Key.InvalidResult())
                .Bind(_ => MeshComponentIndex(mesh: native, polygon: polygon))
                .Bind(component => OfMesh(mesh: native, source: component)));
    public Unit Dispose() {
        _ = value.Dispose();
        return faceBrep.Iter(static owned => owned.Dispose());
    }
    public TopologyProjection DetachFrom(GeometryBase source) {
        ArgumentNullException.ThrowIfNull(source);
        return (Value, source) switch {
            (BrepFace face, _) when ReferenceEquals(face.Brep, source) => Of(face, copy: true),
            (Mesh mesh, Mesh owner) when ReferenceEquals(mesh, owner) && (Source switch { { ComponentIndexType: ComponentIndexType.MeshFace, Index: int i } => i >= 0 && i < mesh.Faces.Count, { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int i } => i >= 0 && i < mesh.Ngons.Count, _ => false }) =>
                new(value: new Lease<GeometryBase>.Owned(Value: mesh.DuplicateMesh()), feature: Feature, source: Source, reversed: Reversed),
            _ => this,
        };
    }
    public bool SameAs(TopologyProjection? other) => other switch { TopologyProjection p => ReferenceEquals(Value, p.Value) && Source.Equals(p.Source), _ => false };
    public bool Transfers(Type outputType) {
        ArgumentNullException.ThrowIfNull(outputType);
        return outputType.IsAssignableFrom(typeof(TopologyProjection))
            || (Value is Curve curve && outputType.IsInstanceOfType(curve))
            || (Value is Brep or BrepFace && outputType.IsAssignableFrom(typeof(Brep)));
    }
    public Fin<Seq<Point3d>> Vertices => (Value, Source) switch {
        (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face }) when face >= 0 && face < mesh.Faces.Count =>
            mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch { true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)), true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)), false => Fin.Fail<Seq<Point3d>>(Key.InvalidResult()) },
        (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon }) when ngon >= 0 && ngon < mesh.Ngons.Count =>
            Optional(mesh.Ngons[ngon].BoundaryVertexIndexList()).ToFin(Key.InvalidResult()).Bind(indices => toSeq(indices).TraverseM(i => i <= int.MaxValue && (int)i < mesh.Vertices.Count ? Fin.Succ((Point3d)mesh.Vertices[(int)i]) : Fin.Fail<Point3d>(Key.InvalidResult())).As()),
        _ => Fin.Fail<Seq<Point3d>>(Key.InvalidInput()),
    };
    public Fin<Vector3d> Normal => (Value, Source) switch {
        (Mesh mesh, { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face }) when face >= 0 && face < mesh.Faces.Count =>
            (mesh.FaceNormals.Count > face || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count > face
                ? NormalOf(new Vector3d(mesh.FaceNormals[face]))
                : Fin.Fail<Vector3d>(Key.InvalidResult()),
        (Mesh, { ComponentIndexType: ComponentIndexType.MeshNgon }) => Vertices.Bind(vertices => vertices.Count switch {
            >= 3 => NormalOf(vertices.Map((point, index) => Vector3d.CrossProduct(point - vertices[0], vertices[(index + 1) % vertices.Count] - vertices[0])).Fold(Vector3d.Zero, static (sum, v) => sum + v)),
            _ => Fin.Fail<Vector3d>(Key.InvalidResult()),
        }),
        _ => Fin.Fail<Vector3d>(Key.InvalidInput()),
    };
    private static Fin<Vector3d> NormalOf(Vector3d candidate) =>
        candidate.IsValid && !candidate.IsTiny() ? Fin.Succ(candidate / candidate.Length) : Fin.Fail<Vector3d>(Key.InvalidResult());
    private static Fin<ComponentIndex> MeshComponentIndex(Mesh mesh, MeshNgon polygon) =>
        Optional(polygon.FaceIndexList()).ToFin(Key.InvalidResult()).Bind(faces => faces switch {
            uint[] values when values.Length == 1 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) < 0 => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshFace, (int)values[0])),
            uint[] values when values.Length > 0 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) is >= 0 and int ngon => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshNgon, ngon)),
            _ => Fin.Fail<ComponentIndex>(Key.InvalidInput()),
        });
    internal static Fin<Seq<TValue>> Project<TValue>(Seq<TopologyProjection> all, Seq<TopologyProjection> chosen, Func<Seq<TopologyProjection>, Fin<Seq<TValue>>> project) {
        Fin<Seq<TValue>> result = project(chosen);
        _ = all.Filter(v => !result.IsSucc || !chosen.Exists(c => c.SameAs(v) && c.Transfers(typeof(TValue)))).Iter(static v => v.Dispose());
        return result;
    }
    public Fin<Point3d> Centroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => Centroid(context: ctx, key: Key));
    public Fin<Plane> FrameAtCentroid(Context? context) => Optional(context).ToFin(Key.MissingContext()).Bind(ctx => FrameAtCentroid(context: ctx, key: Key));
    internal Fin<Point3d> Centroid(Context context, Op key) =>
        As<Brep>().ToFin(key.InvalidResult()).Bind(brep => Optional(AreaMassProperties.Compute(brep, true, true, false, false, context.Fractional, context.Absolute.Value)).ToFin(key.InvalidResult()).Map(static mass => new Lease<AreaMassProperties>.Owned(mass).Use(static d => d.Centroid)));
    internal Fin<Plane> FrameAtCentroid(Context context, Op key) =>
        Face(key).Bind(brepFace => Centroid(context, key).Bind(centroid =>
            (brepFace.ClosestPointOnFace(centroid, out double u, out double v, 0.0), brepFace.FrameAt(u, v, out Plane frame), brepFace.NormalAt(u, v)) switch {
                (true, true, Vector3d normal) when frame.IsValid && normal.IsValid && !normal.IsTiny() =>
                    Fin.Succ((frame.ZAxis * (Reversed ? -normal : normal)) >= 0.0 ? frame : new Plane(frame.Origin, frame.XAxis, -frame.YAxis)),
                _ => Fin.Fail<Plane>(key.InvalidResult()),
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
public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<double> Parameter, Option<Point2d> Uv, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint) {
    internal static ClosestHit At(Point3d target, Point3d point, Option<double> parameter = default, Option<Point2d> uv = default, Option<Vector3d> normal = default, Option<ComponentIndex> component = default, Option<MeshPoint> meshPoint = default) =>
        new(Point: point, Distance: Some(target.DistanceTo(other: point)), Parameter: parameter, Uv: uv, Normal: normal, Component: component, MeshPoint: meshPoint);
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
    internal bool CanDecomposeFaces => Topology is Topology.Brep or Topology.Surface or Topology.Extrusion;
    internal bool CanReadEdges =>
        Type == typeof(Line) || Type == typeof(Polyline) || Type == typeof(BoundingBox) || Type == typeof(Box) || Topology is Topology.Brep or Topology.Mesh or Topology.SubD;
    internal bool CanPrincipal => Topology is Topology.Curve or Topology.Brep or Topology.Mesh or Topology.Surface or Topology.Extrusion;
    internal bool CanCoerceTo(Type target) =>
        target.IsAssignableFrom(Type)
        || (target == typeof(Box) && Type == typeof(Brep))
        || (target == typeof(Curve) && Topology == Topology.Curve)
        || (CurvePrimitives.Contains(target) && Type == typeof(Curve))
        || (SurfacePrimitives.Contains(target) && (Type == typeof(Brep) || Type == typeof(Surface)))
        || (target == typeof(Brep) && BrepSources.Contains(Type));
    private static readonly FrozenSet<Type> CurvePrimitives = new[] { typeof(Line), typeof(Circle), typeof(Arc), typeof(Ellipse), typeof(Polyline) }.ToFrozenSet();
    private static readonly FrozenSet<Type> SurfacePrimitives = new[] { typeof(Plane), typeof(Sphere), typeof(Cylinder), typeof(Cone), typeof(Torus) }.ToFrozenSet();
    private static readonly FrozenSet<Type> BrepSources = new[] { typeof(Brep), typeof(Surface), typeof(Box), typeof(BoundingBox), typeof(Sphere), typeof(Cylinder), typeof(Cone), typeof(Torus), typeof(Extrusion) }.ToFrozenSet();
}

// --- [CONSTANTS] --------------------------------------------------------------------------
[BoundaryAdapter]
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = Kind.Items.ToFrozenDictionary(static k => k.Type);
    internal static readonly FrozenDictionary<Rhino.DocObjects.ObjectType, Kind> ByObjectType = new Dictionary<Rhino.DocObjects.ObjectType, Kind> { [Rhino.DocObjects.ObjectType.Point] = Kind.Point, [Rhino.DocObjects.ObjectType.Curve] = Kind.Curve, [Rhino.DocObjects.ObjectType.Surface] = Kind.Surface, [Rhino.DocObjects.ObjectType.Brep] = Kind.Brep, [Rhino.DocObjects.ObjectType.Mesh] = Kind.Mesh, [Rhino.DocObjects.ObjectType.SubD] = Kind.SubD, [Rhino.DocObjects.ObjectType.PointSet] = Kind.PointCloud, [Rhino.DocObjects.ObjectType.Hatch] = Kind.Hatch, [Rhino.DocObjects.ObjectType.Extrusion] = Kind.Extrusion }.ToFrozenDictionary();
    internal static Option<Kind> Resolve(Type type) => type == typeof(Point) ? Some(Kind.Point) : Optional(ByType.GetValueOrDefault(type)) | (InheritsBase(type) is Type bt ? Optional(ByType.GetValueOrDefault(bt)) : Option<Kind>.None);
    internal static Type? InheritsBase(Type type) => type.BaseType is Type b ? (ByType.ContainsKey(b) ? b : InheritsBase(b)) : null;
}

// --- [SERVICES] ---------------------------------------------------------------------------
[BoundaryAdapter]
internal static class GeometryKernel {
    private static bool Universal(Type type) => type == typeof(object) || type == typeof(GeometryBase);
    internal static bool CanBound(Type source, bool includeSphere) => Universal(source) || typeof(GeometryBase).IsAssignableFrom(source) || KindLookup.Resolve(source).Map(kind => kind.CanBound(includeSphere: includeSphere)).IfNone(false);
    internal static bool CanKind(Type source) => Universal(source) || KindLookup.Resolve(source).IsSome;
    internal static bool CanDecomposeFaces(Type type) => Universal(type) || typeof(BrepFace).IsAssignableFrom(c: type) || KindLookup.Resolve(type).Map(static kind => kind.CanDecomposeFaces).IfNone(false);
    internal static bool CanReadVertices(Type type) => Universal(type) || KindLookup.Resolve(type).Map(static kind => kind.CanReadVertices).IfNone(false);
    internal static bool CanReadControlPoints(Type type) => Universal(type) || KindLookup.Resolve(type).Map(static kind => kind.CanReadControlPoints).IfNone(false);
    internal static bool CanReadEdges(Type type) => Universal(type) || KindLookup.Resolve(type).Map(static kind => kind.CanReadEdges).IfNone(false);
    internal static bool CanPrincipal(Type type) => Universal(type) || KindLookup.Resolve(type).Map(static kind => kind.CanPrincipal).IfNone(false);
    internal static bool CanCurveForm(Type type) => Universal(type) || typeof(Curve).IsAssignableFrom(c: type) || KindLookup.Resolve(type).Map(static kind => kind.Topology == Topology.Curve).IfNone(false);
    internal static bool CanCoerce(Type source, Type target) => Universal(source) || KindLookup.Resolve(source).Map(kind => kind.CanCoerceTo(target: target)).IfNone(target.IsAssignableFrom(source));
    public static Fin<Kind> KindOf(this object geometry, Context context) {
        Op key = Op.Of(name: nameof(Kind));
        return Optional(geometry).ToFin(key.InvalidInput()).Bind(g =>
            (InferredKind(geometry: g, context: context, key: key) | NativeKind(geometry: g) | KindLookup.Resolve(g.GetType()))
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
            _ => KindLookup.Resolve(typeof(TTarget)).Bind(kind => PrimitiveOf(kind, s, context, Op.Of(name: nameof(CoerceTo)))).ToFin(op.Unsupported(s.GetType(), typeof(TTarget))).Map(static v => (TTarget)v),
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
            ? Optional(KindLookup.ByObjectType.GetValueOrDefault(native.ObjectType)) | (native.HasBrepForm ? Some(Kind.Brep) : Option<Kind>.None)
            : Option<Kind>.None;
    private static Option<object> PrimitiveOf(Kind kind, object source, Context context, Op op) =>
        (kind.Type, source) switch {
            (Type t, Point point) when t == typeof(Point3d) => Some((object)point.Location),
            (Type t, Brep brep) when t == typeof(Box) => brep.IsBox(context.Absolute.Value) && brep.Faces[0].UnderlyingSurface().TryGetPlane(out Plane plane, context.Absolute.Value) && new Box(plane, brep) is { IsValid: true } box ? Some((object)box) : Option<object>.None,
            (Type t, object value) when t == typeof(Curve) => CurveForm(source: value, op: op).ToOption().Map(static lease => (object)lease.Resource),
            (Type t, Curve curve) when t == typeof(Line) && curve.IsLinear(context.Absolute.Value) => Some((object)new Line(curve.PointAtStart, curve.PointAtEnd)),
            (Type t, Curve curve) when t == typeof(Circle) && curve.TryGetCircle(out Circle value, context.Absolute.Value) => Some((object)value),
            (Type t, Curve curve) when t == typeof(Arc) && curve.TryGetArc(out Arc value, context.Absolute.Value) => Some((object)value),
            (Type t, Curve curve) when t == typeof(Ellipse) && curve.TryGetEllipse(out Ellipse value, context.Absolute.Value) => Some((object)value),
            (Type t, Curve curve) when t == typeof(Polyline) && curve.TryGetPolyline(out Polyline value) => Some((object)value),
            (Type t, Brep { IsSurface: true } brep) when t == typeof(Plane) || t == typeof(Sphere) || t == typeof(Cylinder) || t == typeof(Cone) || t == typeof(Torus) => PrimitiveOf(kind, brep.Surfaces[0], context, op),
            (Type t, Surface surface) when t == typeof(Plane) && surface.TryGetPlane(out Plane value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Sphere) && surface.TryGetSphere(out Sphere value, context.Absolute.Value) => Some((object)value),
            (Type t, Surface surface) when t == typeof(Cylinder) && surface.TryGetCylinder(out Cylinder value, context.Absolute.Value) => Some((object)value),
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
        Fractions(count: count, op: key).Bind(fractions =>
            Optional(curve.NormalizedLengthParameters([.. fractions.AsIterable()], context.Absolute.Value, context.Fractional)).ToFin(key.InvalidResult()).Map(static p => toSeq(p)));
    internal static Fin<Point2d> SurfaceUv(Surface surface, Point2d uv, Context context, Op key) =>
        (uv.IsValid, surface.Domain(0), surface.Domain(1)) switch {
            (true, Interval u, Interval v) when u.IsValid && v.IsValid && u.IncludesParameter(uv.X) && v.IncludesParameter(uv.Y)
                && (surface is not BrepFace face || face.IsPointOnFace(uv.X, uv.Y, context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
    internal static Fin<Seq<Point2d>> SurfaceSampleUv(Surface surface, int resolution, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model =>
        Optional(surface).ToFin(key.InvalidInput()).Bind(native => (native.Domain(0), native.Domain(1)) switch {
            (Interval u, Interval v) when u.IsValid && v.IsValid =>
                Fractions(count: resolution, op: key)
                    .Map(fractions => fractions.Bind(uf => fractions.Map(vf => new Point2d(u.ParameterAt(uf), v.ParameterAt(vf)))))
                    .Bind(samples => native is BrepFace face
                        ? OnFaceUv(face, samples, model.Absolute.Value) switch { Seq<Point2d> valid when !valid.IsEmpty => Fin.Succ(valid), _ => Fin.Fail<Seq<Point2d>>(key.InvalidResult()) }
                        : Fin.Succ(samples)),
            _ => Fin.Fail<Seq<Point2d>>(key.InvalidInput()),
        }));
    private static Seq<Point2d> OnFaceUv(BrepFace face, Seq<Point2d> samples, double tolerance) =>
        samples.Choose(uv => face.IsPointOnFace(u: uv.X, v: uv.Y, tolerance: tolerance) != PointFaceRelation.Exterior ? Some(uv)
            : face.ClosestPointOnFace(testPoint: face.PointAt(u: uv.X, v: uv.Y), u: out double fu, v: out double fv, maximumDistance: 0.0)
                && face.IsPointOnFace(u: fu, v: fv, tolerance: tolerance) != PointFaceRelation.Exterior ? Some(new Point2d(fu, fv)) : Option<Point2d>.None);
    internal static Fin<Seq<Point3d>> SurfaceSamplePoints(Surface surface, int resolution, Context context, Op key) =>
        SurfaceSampleUv(surface: surface, resolution: resolution, context: context, key: key)
            .Map(uvs => uvs.Map(uv => surface.PointAt(u: uv.X, v: uv.Y)));
    internal static Fin<ClosestHit> ClosestOf(object? geometry, Point3d target, Op key) =>
        from _ in guard(target.IsValid, key.InvalidInput())
        from g in Optional(geometry).ToFin(key.InvalidInput())
        from hit in g switch {
            Line line => Fin.Succ(ClosestHit.At(target: target, point: line.ClosestPoint(testPoint: target, limitToFiniteSegment: true))),
            Polyline polyline => Fin.Succ(ClosestHit.At(target: target, point: polyline.ClosestPoint(testPoint: target))),
            Curve curve when curve.ClosestPoint(testPoint: target, t: out double parameter) =>
                Fin.Succ(ClosestHit.At(target: target, point: curve.PointAt(t: parameter), parameter: Some(parameter))),
            BrepFace face when face.ClosestPointOnFace(testPoint: target, u: out double u, v: out double v, maximumDistance: 0.0) =>
                Fin.Succ(ClosestHit.At(target: target, point: face.PointAt(u: u, v: v), uv: Some(new Point2d(x: u, y: v)), normal: Some(face.NormalAt(u: u, v: v)),
                    component: face.FaceIndex >= 0 ? Some(new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex)) : Option<ComponentIndex>.None)),
            Surface surface when surface.ClosestPoint(testPoint: target, u: out double u, v: out double v) =>
                Fin.Succ(ClosestHit.At(target: target, point: surface.PointAt(u: u, v: v), uv: Some(new Point2d(x: u, y: v)), normal: Some(surface.NormalAt(u: u, v: v)))),
            Brep brep when brep.ClosestPoint(target, out Point3d point, out ComponentIndex component, out double u, out double v, 0.0, out Vector3d normal) =>
                Fin.Succ(ClosestHit.At(target: target, point: point, uv: Some(new Point2d(x: u, y: v)), normal: Some(normal), component: Some(component))),
            Mesh mesh => Optional(mesh.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0)).ToFin(key.InvalidResult())
                .Map(meshPoint => ClosestHit.At(target: target, point: meshPoint.Point, normal: Some(mesh.NormalAt(meshPoint: meshPoint)), meshPoint: Some(meshPoint))),
            Curve or BrepFace or Surface or Brep => Fin.Fail<ClosestHit>(key.InvalidInput()),
            _ => Fin.Fail<ClosestHit>(key.Unsupported(g.GetType(), typeof(ClosestHit))),
        }
        select hit;
}
