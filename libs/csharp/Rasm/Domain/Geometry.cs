using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

public enum GeometryKind { Unknown = 0, Curve = 1, Polyline = 2, Mesh = 3, SubD = 4, Surface = 5, BrepGeneral = 10, BrepBox = 11, BrepSphere = 12, BrepCylinder = 13, BrepCone = 14, BrepTorus = 15, BrepPlane = 16, Line = 20, Sphere = 21, Box = 22, BoundingBox = 23, Cylinder = 24, Cone = 25, Torus = 26, Plane = 27 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, IsoU, IsoV, Silhouette, SubCurve, Draft }

internal static class Validity {
    internal static Fin<TValue> RequireValid<TValue>(this Op key, TValue value) =>
        value switch {
            Point2d point => key.Require(condition: point.IsValid, value: value),
            Point3d point => key.Require(condition: point.IsValid, value: value),
            Vector3d vector => key.Require(condition: vector.IsValid, value: value),
            Plane plane => key.Require(condition: plane.IsValid, value: value),
            BoundingBox box => key.Require(condition: box.IsValid, value: value),
            Box box => key.Require(condition: box.IsValid, value: value),
            Sphere sphere => key.Require(condition: sphere.IsValid, value: value),
            Cylinder cylinder => key.Require(condition: cylinder.IsValid, value: value),
            Cone cone => key.Require(condition: cone.IsValid, value: value),
            Torus torus => key.Require(condition: torus.IsValid, value: value),
            Arc arc => key.Require(condition: arc.IsValid, value: value),
            Circle circle => key.Require(condition: circle.IsValid, value: value),
            Ellipse ellipse => key.Require(condition: ellipse.IsValid, value: value),
            Rectangle3d rectangle => key.Require(condition: rectangle.IsValid, value: value),
            Interval interval => key.Require(condition: interval.IsValid, value: value),
            Line line => key.Require(condition: line.IsValid, value: value),
            Polyline polyline => key.Require(condition: polyline.IsValid, value: value),
            GeometryBase geometry => key.Require(condition: geometry.IsValid, value: value),
            SurfaceCurvature => Fin.Succ(value),
            MeshCheckParameters => Fin.Succ(value),
            MeshPoint meshPoint => key.Require(condition: meshPoint.Point.IsValid, value: value),
            ComponentIndex component => key.Require(condition: component.ComponentIndexType != ComponentIndexType.InvalidType && component.Index >= 0, value: value),
            IntersectionEvent intersection => key.Require(condition:
                (intersection.IsPoint || intersection.IsOverlap)
                && intersection.PointA.IsValid
                && intersection.PointB.IsValid, value: value),
            ValueTuple<double, Vector3d> principal => key.Require(condition: RhinoMath.IsValidDouble(x: principal.Item1) && principal.Item2.IsValid, value: value),
            double scalar => key.Require(condition: RhinoMath.IsValidDouble(x: scalar), value: value),
            bool or int or Enum => Fin.Succ(value),
            _ => Fin.Fail<TValue>(key.InvalidResult()),
        };
    private static Fin<TValue> Require<TValue>(this Op key, bool condition, TValue value) => condition ? Fin.Succ(value) : Fin.Fail<TValue>(key.InvalidResult());
}

// --- [GEOMETRY_CLASSIFIER] --------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class GeometryClassifier {
    private delegate Fin<BoundingBox> BoundsFn(object geometry, Op key);
    private delegate Fin<object> ExtractFn(object geometry, Context context, Op key);
    public static readonly GeometryClassifier Point3d = new(key: 0, type: typeof(Point3d), kind: GeometryKind.Unknown, extractFrom: Option<Type>.None,
        bounds: static (g, key) => g switch { Point3d { IsValid: true } p => Fin.Succ(new BoundingBox(min: p, max: p)), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Option<ExtractFn>.None);
    public static readonly GeometryClassifier BoundingBox = new(key: 1, type: typeof(BoundingBox), kind: GeometryKind.BoundingBox, extractFrom: Option<Type>.None,
        bounds: static (g, key) => g switch { BoundingBox { IsValid: true } b => Fin.Succ(b), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Option<ExtractFn>.None);
    public static readonly GeometryClassifier Box = new(key: 2, type: typeof(Box), kind: GeometryKind.Box, extractFrom: Some(typeof(Brep)),
        bounds: static (g, key) => g switch { Box { IsValid: true } b => Fin.Succ(b.BoundingBox), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Brep brep when brep.IsBox(tolerance: c.Absolute.Value) && brep.GetBoundingBox(plane: Rhino.Geometry.Plane.WorldXY, worldBox: out Box value) is { IsValid: true } => Fin.Succ<object>(value), Brep => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Box))) }));
    public static readonly GeometryClassifier Sphere = new(key: 3, type: typeof(Sphere), kind: GeometryKind.Sphere, extractFrom: Some(typeof(Surface)),
        bounds: static (g, key) => g switch { Sphere { IsValid: true } s => Fin.Succ(s.BoundingBox), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Surface s when s.TryGetSphere(sphere: out Sphere value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Surface => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Sphere))) }));
    public static readonly GeometryClassifier Line = new(key: 4, type: typeof(Line), kind: GeometryKind.Line, extractFrom: Option<Type>.None,
        bounds: static (g, key) => g switch { Line { IsValid: true } l => Fin.Succ(l.BoundingBox), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Option<ExtractFn>.None);
    public static readonly GeometryClassifier Polyline = new(key: 5, type: typeof(Polyline), kind: GeometryKind.Polyline, extractFrom: Some(typeof(Curve)),
        bounds: static (g, key) => g switch { Polyline p => Fin.Succ(p.BoundingBox), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, _, key) => g switch { Curve curve when curve.TryGetPolyline(polyline: out Polyline value) => Fin.Succ<object>(value), Curve => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Polyline))) }));
    public static readonly GeometryClassifier Circle = new(key: 6, type: typeof(Circle), kind: GeometryKind.Unknown, extractFrom: Some(typeof(Curve)),
        bounds: static (g, key) => g switch { Circle c => Fin.Succ(c.BoundingBox), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Curve curve when curve.TryGetCircle(circle: out Circle value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Curve => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Circle))) }));
    public static readonly GeometryClassifier Arc = new(key: 7, type: typeof(Arc), kind: GeometryKind.Unknown, extractFrom: Some(typeof(Curve)),
        bounds: static (g, key) => g switch { Arc { IsValid: true } a => Optional(a.ToNurbsCurve()).ToFin(key.InvalidResult()).Map(static curve => { using NurbsCurve disposable = curve; return disposable.GetBoundingBox(accurate: true); }), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Curve curve when curve.TryGetArc(arc: out Arc value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Curve => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Arc))) }));
    public static readonly GeometryClassifier Cylinder = new(key: 8, type: typeof(Cylinder), kind: GeometryKind.Cylinder, extractFrom: Some(typeof(Surface)),
        bounds: static (g, key) => g switch { Cylinder { IsValid: true } c => Optional(c.ToBrep(capBottom: true, capTop: true)).ToFin(key.InvalidResult()).Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Surface s when s.TryGetCylinder(cylinder: out Cylinder value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Surface => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Cylinder))) }));
    public static readonly GeometryClassifier Cone = new(key: 9, type: typeof(Cone), kind: GeometryKind.Cone, extractFrom: Some(typeof(Surface)),
        bounds: static (g, key) => g switch { Cone { IsValid: true } c => Optional(c.ToBrep(capBottom: true)).ToFin(key.InvalidResult()).Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Surface s when s.TryGetCone(cone: out Cone value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Surface => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Cone))) }));
    public static readonly GeometryClassifier Torus = new(key: 10, type: typeof(Torus), kind: GeometryKind.Torus, extractFrom: Some(typeof(Surface)),
        bounds: static (g, key) => g switch { Torus { IsValid: true } t => Optional(t.ToBrep()).ToFin(key.InvalidResult()).Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Surface s when s.TryGetTorus(torus: out Torus value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Surface => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Torus))) }));
    public static readonly GeometryClassifier Native = new(key: 11, type: typeof(GeometryBase), kind: GeometryKind.Unknown, extractFrom: Option<Type>.None,
        bounds: static (g, key) => g switch { GeometryBase { IsValid: true } b => Fin.Succ(b.GetBoundingBox(accurate: true)), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Option<ExtractFn>.None);
    public static readonly GeometryClassifier Ellipse = new(key: 12, type: typeof(Ellipse), kind: GeometryKind.Unknown, extractFrom: Some(typeof(Curve)),
        bounds: static (g, key) => g switch { Ellipse e => Optional(e.ToNurbsCurve()).ToFin(key.InvalidResult()).Map(static curve => { using NurbsCurve disposable = curve; return disposable.GetBoundingBox(accurate: true); }), _ => Fin.Fail<BoundingBox>(key.InvalidInput()) },
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Curve curve when curve.TryGetEllipse(ellipse: out Ellipse value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Curve => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Ellipse))) }));
    public static readonly GeometryClassifier Plane = new(key: 13, type: typeof(Plane), kind: GeometryKind.Plane, extractFrom: Some(typeof(Surface)),
        bounds: static (g, key) => Fin.Fail<BoundingBox>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(BoundingBox))),
        extract: Some<ExtractFn>(static (g, c, key) => g switch { Surface s when s.TryGetPlane(plane: out Plane value, tolerance: c.Absolute.Value) => Fin.Succ<object>(value), Surface => Fin.Fail<object>(key.InvalidResult()), _ => Fin.Fail<object>(key.Unsupported(geometryType: g.GetType(), outputType: typeof(Plane))) }));
    internal Type Type { get; }
    public GeometryKind Kind { get; }
    public Option<Type> ExtractFrom { get; }
    private BoundsFn Bounds { get; }
    private Option<ExtractFn> Extract { get; }
    // FrozenDictionary because LanguageExt v5 Map/HashMap<Type,_> trips a Rhino reflection enumeration bug.
    private static readonly FrozenDictionary<Type, GeometryClassifier> Lookup = BuildLookup();
    [BoundaryAdapter]
    private static FrozenDictionary<Type, GeometryClassifier> BuildLookup() =>
        new Dictionary<Type, GeometryClassifier> {
            [Point3d.Type] = Point3d, [BoundingBox.Type] = BoundingBox, [Box.Type] = Box, [Sphere.Type] = Sphere,
            [Line.Type] = Line, [Polyline.Type] = Polyline, [Circle.Type] = Circle, [Arc.Type] = Arc,
            [Cylinder.Type] = Cylinder, [Cone.Type] = Cone, [Torus.Type] = Torus, [Native.Type] = Native,
            [Ellipse.Type] = Ellipse, [Plane.Type] = Plane,
        }.ToFrozenDictionary();
    internal static Option<GeometryClassifier> For(Type type) =>
        Optional(Lookup.GetValueOrDefault(type)) | (typeof(GeometryBase).IsAssignableFrom(c: type) ? Some(Native) : Option<GeometryClassifier>.None);
    internal static Option<GeometryClassifier> Match(object geometry) =>
        For(type: geometry.GetType());
    internal static Fin<BoundingBox> BoundingBoxOf(object geometry, Op key, Type outputType) =>
        Match(geometry: geometry).Case switch {
            GeometryClassifier classifier => classifier.Bounds(geometry: geometry, key: key),
            _ => Fin.Fail<BoundingBox>(key.Unsupported(geometryType: geometry.GetType(), outputType: outputType)),
        };
    internal static bool SupportsBounds(Type type, bool includeSphere) =>
        (For(type: type).IsSome || type == typeof(object)) && (includeSphere || type != typeof(Sphere)) && type != typeof(Plane);
    internal static GeometryKind KindOf(object geometry, Context context) =>
        geometry switch {
            Brep { IsSurface: true } single => KindOfSurface(surface: single.Surfaces[0], context: context, brep: true),
            Brep candidate when candidate.IsBox(tolerance: context.Absolute.Value) => GeometryKind.BrepBox,
            Brep => GeometryKind.BrepGeneral,
            Surface surface => KindOfSurface(surface: surface, context: context, brep: false),
            Mesh => GeometryKind.Mesh,
            SubD => GeometryKind.SubD,
            Curve curve => curve.TryGetPolyline(polyline: out Polyline _) ? GeometryKind.Polyline : GeometryKind.Curve,
            _ => For(type: geometry.GetType()).Case switch {
                GeometryClassifier classifier => classifier.Kind,
                _ => GeometryKind.Unknown,
            },
        };
    private static GeometryKind KindOfSurface(Surface surface, Context context, bool brep) =>
        surface switch {
            Surface s when s.TryGetPlane(plane: out Plane _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepPlane : GeometryKind.Plane,
            Surface s when s.TryGetSphere(sphere: out Sphere _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepSphere : GeometryKind.Sphere,
            Surface s when s.TryGetCylinder(cylinder: out Cylinder _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepCylinder : GeometryKind.Cylinder,
            Surface s when s.TryGetCone(cone: out Cone _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepCone : GeometryKind.Cone,
            Surface s when s.TryGetTorus(torus: out Torus _, tolerance: context.Absolute.Value) => brep ? GeometryKind.BrepTorus : GeometryKind.Torus,
            _ => brep ? GeometryKind.BrepGeneral : GeometryKind.Surface,
        };
    internal Fin<object> ExtractPrimitive(object geometry, Context context, Op key) =>
        Extract.Case switch {
            ExtractFn fn => fn(geometry: geometry, context: context, key: key),
            _ => Fin.Fail<object>(key.Unsupported(geometryType: geometry.GetType(), outputType: Type)),
        };
}
