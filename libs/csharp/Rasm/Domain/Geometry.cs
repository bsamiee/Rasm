using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

// --- [TYPES] ------------------------------------------------------------------------------
public enum Topology { Unknown, Point, Curve, Surface, Brep, Mesh, SubD, PointCloud, Hatch, Extrusion }
public enum Primitive { None, Line, Polyline, Circle, Arc, Ellipse, Plane, Sphere, Cylinder, Cone, Torus, Box, BoundingBox }
public enum Closure { Unknown, Open, Closed }
public enum Solidity { Unknown, Open, Solid }
public enum GeometryKind { Unknown = 0, Curve = 1, Polyline = 2, Mesh = 3, SubD = 4, Surface = 5, BrepGeneral = 10, BrepBox = 11, BrepSphere = 12, BrepCylinder = 13, BrepCone = 14, BrepTorus = 15, BrepPlane = 16, Line = 20, Sphere = 21, Box = 22, BoundingBox = 23, Cylinder = 24, Cone = 25, Torus = 26, Plane = 27 }
public enum IntersectionKind { Unknown = 0, Point = 1, Overlap = 2, Curve = 3 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, IsoU, IsoV, Silhouette, SubCurve, Draft }

[BoundaryAdapter]
public interface ITopologyProjection {
    public ComponentIndex Source { get; }
    public Unit Dispose();
    public bool SameAs(ITopologyProjection other);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurveProjection(Curve Curve, CurveFeature Feature, ComponentIndex Source) : ITopologyProjection {
    internal CurveProjection(Curve curve, CurveFeature feature, ComponentIndexType type, int index)
        : this(Curve: curve, Feature: feature, Source: new ComponentIndex(type: type, index: index)) { }
    public Unit Dispose() => fun(static (Curve curve) => { curve.Dispose(); return Unit.Default; })(Curve);
    public bool SameAs(ITopologyProjection other) => other is CurveProjection c && ReferenceEquals(objA: Curve, objB: c.Curve);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ClosestHit(Point3d Point, Option<double> Distance, Option<Vector3d> Normal, Option<ComponentIndex> Component, Option<MeshPoint> MeshPoint);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CurveSelector(CurveFeature Feature, Option<Vector3d> Direction, Option<double> Angle, Option<int> Index, Option<double> Normalized);

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

// --- [CONSTANTS] --------------------------------------------------------------------------
// FrozenDictionary because LanguageExt v5 Map/HashMap<Type,_> trips a Rhino reflection enumeration bug.
[BoundaryAdapter]
internal static class KindLookup {
    private static readonly FrozenDictionary<Type, Kind> ByType = Kind.Items.ToFrozenDictionary(keySelector: static k => k.Type);
    internal static Option<Kind> For(Type type) =>
        Optional(ByType.GetValueOrDefault(key: type)) | (
            typeof(Extrusion).IsAssignableFrom(c: type) ? Some(Kind.Extrusion) :
            typeof(Brep).IsAssignableFrom(c: type) ? Some(Kind.Brep) :
            typeof(Surface).IsAssignableFrom(c: type) ? Some(Kind.Surface) :
            typeof(Curve).IsAssignableFrom(c: type) ? Some(Kind.Curve) :
            typeof(Mesh).IsAssignableFrom(c: type) ? Some(Kind.Mesh) :
            typeof(SubD).IsAssignableFrom(c: type) ? Some(Kind.SubD) :
            typeof(PointCloud).IsAssignableFrom(c: type) ? Some(Kind.Cloud) :
            typeof(Hatch).IsAssignableFrom(c: type) ? Some(Kind.Hatch) :
            Option<Kind>.None);
}

[BoundaryAdapter]
internal static class Coercion {
    internal static bool Supports(Type geometryType, Type targetType) =>
        Table.ContainsKey(key: (geometryType, targetType))
        || (typeof(Curve).IsAssignableFrom(c: geometryType) && Table.ContainsKey(key: (typeof(Curve), targetType)))
        || (typeof(Surface).IsAssignableFrom(c: geometryType) && Table.ContainsKey(key: (typeof(Surface), targetType)))
        || (typeof(Brep).IsAssignableFrom(c: geometryType) && Table.ContainsKey(key: (typeof(Brep), targetType)));
    private static readonly FrozenDictionary<(Type Source, Type Target), Func<Context, object, Op, Fin<object>>> Table =
        new Dictionary<(Type, Type), Func<Context, object, Op, Fin<object>>> {
            [(typeof(Curve), typeof(Line))] = static (_, g, op) => ((Curve)g).TryGetPolyline(polyline: out Polyline poly) && poly.Count == 2 ? Fin.Succ<object>(new Line(from: poly[0], to: poly[1])) : ((Curve)g).IsLinear() ? Fin.Succ<object>(new Line(from: ((Curve)g).PointAtStart, to: ((Curve)g).PointAtEnd)) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Curve), typeof(Polyline))] = static (_, g, op) => ((Curve)g).TryGetPolyline(polyline: out Polyline poly) ? Fin.Succ<object>(poly) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Curve), typeof(Circle))] = static (ctx, g, op) => ((Curve)g).TryGetCircle(circle: out Circle circle, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(circle) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Curve), typeof(Arc))] = static (ctx, g, op) => ((Curve)g).TryGetArc(arc: out Arc arc, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(arc) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Curve), typeof(Ellipse))] = static (ctx, g, op) => ((Curve)g).TryGetEllipse(ellipse: out Ellipse ellipse, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(ellipse) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Surface), typeof(Plane))] = static (ctx, g, op) => ((Surface)g).TryGetPlane(plane: out Plane plane, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(plane) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Surface), typeof(Sphere))] = static (ctx, g, op) => ((Surface)g).TryGetSphere(sphere: out Sphere sphere, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(sphere) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Surface), typeof(Cylinder))] = static (ctx, g, op) => ((Surface)g).TryGetCylinder(cylinder: out Cylinder cyl, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(cyl) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Surface), typeof(Cone))] = static (ctx, g, op) => ((Surface)g).TryGetCone(cone: out Cone cone, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(cone) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Surface), typeof(Torus))] = static (ctx, g, op) => ((Surface)g).TryGetTorus(torus: out Torus torus, tolerance: ctx.Absolute.Value) ? Fin.Succ<object>(torus) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
            [(typeof(Brep), typeof(Box))] = static (ctx, g, op) => ((Brep)g).IsBox(tolerance: ctx.Absolute.Value) && ((Brep)g).GetBoundingBox(plane: Rhino.Geometry.Plane.WorldXY, worldBox: out Box box) is { IsValid: true } ? Fin.Succ<object>(box) : Fin.Fail<object>(error: new Fault.InvalidResult(Key: op)),
        }.ToFrozenDictionary();
    internal static Fin<TTarget> Of<TTarget>(object source, Context ctx, Op op) {
        ArgumentNullException.ThrowIfNull(argument: source);
        return ResolveLookup(sourceType: source.GetType(), targetType: typeof(TTarget)) switch {
            (Type s, Type t) when Table.GetValueOrDefault(key: (s, t)) is Func<Context, object, Op, Fin<object>> fn => fn(arg1: ctx, arg2: source, arg3: op).Map(static (object v) => (TTarget)v),
            _ => Fin.Fail<TTarget>(error: new Fault.Unsupported(Key: op, GeometryType: source.GetType(), OutputType: typeof(TTarget))),
        };
    }
    private static (Type Source, Type Target) ResolveLookup(Type sourceType, Type targetType) =>
        Table.ContainsKey(key: (sourceType, targetType)) switch {
            true => (sourceType, targetType),
            false => (typeof(Curve).IsAssignableFrom(c: sourceType), typeof(Surface).IsAssignableFrom(c: sourceType), typeof(Brep).IsAssignableFrom(c: sourceType)) switch {
                (true, _, _) when Table.ContainsKey(key: (typeof(Curve), targetType)) => (typeof(Curve), targetType),
                (_, true, _) when Table.ContainsKey(key: (typeof(Surface), targetType)) => (typeof(Surface), targetType),
                (_, _, true) when Table.ContainsKey(key: (typeof(Brep), targetType)) => (typeof(Brep), targetType),
                _ => (sourceType, targetType),
            },
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
[BoundaryAdapter]
public static class KindRole {
    [BoundaryAdapter]
    public static Option<Kind> AsKind(this Type type) => KindLookup.For(type: type);
    [BoundaryAdapter]
    public static bool SupportsBounds(this Type type, bool includeSphere) =>
        (KindLookup.For(type: type).IsSome || type == typeof(object))
        && (includeSphere || type != typeof(Sphere))
        && type != typeof(Plane);
    [BoundaryAdapter]
    public static bool InputCurve(this CurveFeature feature) =>
        feature is CurveFeature.Input or CurveFeature.Segment or CurveFeature.SubCurve or CurveFeature.Boundary;
    [BoundaryAdapter]
    public static bool InputBoundary(this CurveFeature feature) =>
        feature is CurveFeature.Input or CurveFeature.Boundary;
    [BoundaryAdapter]
    public static Fin<Kind> Kind(this object value, Context ctx) {
        ArgumentNullException.ThrowIfNull(argument: value);
        ArgumentNullException.ThrowIfNull(argument: ctx);
        return value switch {
            Brep brep when brep.IsBox(tolerance: ctx.Absolute.Value) => Fin.Succ(Rasm.Domain.Kind.Box),
            Brep { IsSurface: true } single => SurfaceClassification(s: single.Surfaces[0], absolute: ctx.Absolute.Value, brep: true),
            Surface s => SurfaceClassification(s: s, absolute: ctx.Absolute.Value, brep: false),
            Curve curve when curve.TryGetPolyline(polyline: out Polyline _) => Fin.Succ(Rasm.Domain.Kind.Polyline),
            Curve curve when curve.TryGetCircle(circle: out Circle _, tolerance: ctx.Absolute.Value) => Fin.Succ(Rasm.Domain.Kind.Circle),
            Curve curve when curve.TryGetArc(arc: out Arc _, tolerance: ctx.Absolute.Value) => Fin.Succ(Rasm.Domain.Kind.Arc),
            Curve curve when curve.TryGetEllipse(ellipse: out Ellipse _, tolerance: ctx.Absolute.Value) => Fin.Succ(Rasm.Domain.Kind.Ellipse),
            Curve curve when curve.IsLinear(tolerance: ctx.Absolute.Value) => Fin.Succ(Rasm.Domain.Kind.Line),
            _ => KindLookup.For(type: value.GetType()).ToFin(Fail: new Fault.InvalidInput(Key: Op.Of(name: "Kind"))),
        };
    }
    [BoundaryAdapter]
    public static Fin<BoundingBox> Bounds(this object value, Op op) {
        ArgumentNullException.ThrowIfNull(argument: value);
        return value switch {
            BoundingBox { IsValid: true } bb => Fin.Succ(bb),
            Box { IsValid: true } b => Fin.Succ(b.BoundingBox),
            Sphere { IsValid: true } s => Fin.Succ(s.BoundingBox),
            Plane => Fin.Fail<BoundingBox>(error: new Fault.Unsupported(Key: op, GeometryType: value.GetType(), OutputType: typeof(BoundingBox))),
            Line { IsValid: true } l => Fin.Succ(l.BoundingBox),
            Polyline p => Fin.Succ(p.BoundingBox),
            Point3d { IsValid: true } pt => Fin.Succ(new BoundingBox(min: pt, max: pt)),
            Circle c => Fin.Succ(c.BoundingBox),
            Arc { IsValid: true } a => Optional(a.ToNurbsCurve()).ToFin(Fail: new Fault.InvalidResult(Key: op)).Map(static c => { using NurbsCurve d = c; return d.GetBoundingBox(accurate: true); }),
            Ellipse e => Optional(e.ToNurbsCurve()).ToFin(Fail: new Fault.InvalidResult(Key: op)).Map(static c => { using NurbsCurve d = c; return d.GetBoundingBox(accurate: true); }),
            Cylinder { IsValid: true } cyl => Optional(cyl.ToBrep(capBottom: true, capTop: true)).ToFin(Fail: new Fault.InvalidResult(Key: op)).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }),
            Cone { IsValid: true } co => Optional(co.ToBrep(capBottom: true)).ToFin(Fail: new Fault.InvalidResult(Key: op)).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }),
            Torus { IsValid: true } t => Optional(t.ToBrep()).ToFin(Fail: new Fault.InvalidResult(Key: op)).Map(static b => { using Brep d = b; return d.GetBoundingBox(accurate: true); }),
            GeometryBase { IsValid: true } gb => Fin.Succ(gb.GetBoundingBox(accurate: true)),
            _ => Fin.Fail<BoundingBox>(error: new Fault.InvalidInput(Key: op)),
        };
    }
    [BoundaryAdapter]
    public static Fin<TTarget> Coerce<TTarget>(this Kind kind, object value, Context ctx, Op op) {
        ArgumentNullException.ThrowIfNull(argument: kind);
        ArgumentNullException.ThrowIfNull(argument: value);
        return Coercion.Of<TTarget>(source: value, ctx: ctx, op: op);
    }
    [BoundaryAdapter]
    internal static Fin<Kind> SurfaceClassification(Surface s, double absolute, bool brep) =>
        s.TryGetPlane(plane: out Plane _, tolerance: absolute) ? Fin.Succ(brep ? Rasm.Domain.Kind.Surface : Rasm.Domain.Kind.Plane) :
        s.TryGetSphere(sphere: out Sphere _, tolerance: absolute) ? Fin.Succ(Rasm.Domain.Kind.Sphere) :
        s.TryGetCylinder(cylinder: out Cylinder _, tolerance: absolute) ? Fin.Succ(Rasm.Domain.Kind.Cylinder) :
        s.TryGetCone(cone: out Cone _, tolerance: absolute) ? Fin.Succ(Rasm.Domain.Kind.Cone) :
        s.TryGetTorus(torus: out Torus _, tolerance: absolute) ? Fin.Succ(Rasm.Domain.Kind.Torus) :
        Fin.Succ(brep ? Rasm.Domain.Kind.Brep : Rasm.Domain.Kind.Surface);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
[Union]
public partial record IntersectionResult {
    public sealed record Curves(Seq<Curve> Values) : IntersectionResult;
    public sealed record Lines(Seq<Line> Values) : IntersectionResult;
    public sealed record Circles(Seq<Circle> Values) : IntersectionResult;
    public sealed record Points(Seq<Point3d> Values) : IntersectionResult;
    public sealed record Intervals(Seq<Interval> Values) : IntersectionResult;
    public sealed record Polylines(Seq<Polyline> Values, Seq<IntersectionKind> Kinds) : IntersectionResult;
    public sealed record Events(Seq<IntersectionEvent> Values) : IntersectionResult;
    public sealed record Mixed(Seq<Curve> CurveValues, Seq<Point3d> PointValues) : IntersectionResult;
}
