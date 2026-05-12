using System.Collections.Frozen;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Domain;

public enum GeometryKind { Unknown = 0, Curve = 1, Polyline = 2, Mesh = 3, SubD = 4, Surface = 5, BrepGeneral = 10, BrepBox = 11, BrepSphere = 12, BrepCylinder = 13, BrepCone = 14, BrepTorus = 15, BrepPlane = 16, Line = 20, Sphere = 21, Box = 22, BoundingBox = 23, Cylinder = 24, Cone = 25, Torus = 26, Plane = 27 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, IsoU, IsoV, Silhouette, SubCurve, Draft }
[StructLayout(LayoutKind.Auto)]
internal readonly record struct FaceProjection {
    private FaceProjection(Brep brep, int faceIndex, bool reversed) { Brep = brep; FaceIndex = faceIndex; Reversed = reversed; }
    internal Brep Brep { get; }
    internal int FaceIndex { get; }
    internal bool Reversed { get; }
    internal static FaceProjection From(BrepFace face) => new(brep: face.DuplicateFace(duplicateMeshes: false), faceIndex: face.FaceIndex, reversed: face.OrientationIsReversed);
    internal Unit Dispose() =>
        fun(static (Brep brep) => { brep.Dispose(); return Unit.Default; })(Brep);
}
[StructLayout(LayoutKind.Auto)]
internal readonly record struct CurveProjection {
    internal CurveProjection(Curve curve, CurveFeature feature, ComponentIndexType type, int index) : this(curve: curve, feature: feature, source: new ComponentIndex(type: type, index: index)) { }
    internal CurveProjection(Curve curve, CurveFeature feature, ComponentIndex source) { Curve = curve; Feature = feature; Source = source; }
    internal Curve Curve { get; }
    internal CurveFeature Feature { get; }
    internal ComponentIndex Source { get; }
    internal Unit Dispose() =>
        fun(static (Curve curve) => { curve.Dispose(); return Unit.Default; })(Curve);
}
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
internal static class GeometryKinds {
    internal static GeometryKind Kind(object geometry, Context context) =>
        geometry switch {
            Brep brep => KindOfBrep(brep: brep, context: context),
            Surface surface => KindOfSurface(surface: surface, context: context, brep: false),
            Mesh => GeometryKind.Mesh,
            SubD => GeometryKind.SubD,
            Curve curve => curve.TryGetPolyline(polyline: out Polyline _) ? GeometryKind.Polyline : GeometryKind.Curve,
            Polyline => GeometryKind.Polyline,
            Line => GeometryKind.Line,
            Sphere => GeometryKind.Sphere,
            Box => GeometryKind.Box,
            BoundingBox => GeometryKind.BoundingBox,
            _ => GeometryKind.Unknown,
        };
    internal static GeometryKind KindOfBrep(Brep brep, Context context) =>
        brep switch {
            { IsSurface: true } single => KindOfSurface(surface: single.Surfaces[0], context: context, brep: true),
            Brep candidate when candidate.IsBox(tolerance: context.Absolute.Value) => GeometryKind.BrepBox,
            _ => GeometryKind.BrepGeneral,
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
}
[StructLayout(LayoutKind.Auto)]
internal readonly record struct Stats {
    private Stats(int count, double minimum, double maximum, double mean, double variance, double rms) {
        Count = count;
        Minimum = minimum;
        Maximum = maximum;
        Mean = mean;
        Variance = variance;
        Rms = rms;
    }
    internal int Count { get; }
    internal double Minimum { get; }
    internal double Maximum { get; }
    internal double Mean { get; }
    internal double Variance { get; }
    internal double Rms { get; }
    internal static Fin<Stats> From(Seq<double> values, Op key) =>
        values.Fold(
            initialState: (Count: 0, Mean: 0.0, M2: 0.0, SumSquares: 0.0, Minimum: double.PositiveInfinity, Maximum: double.NegativeInfinity, AllFinite: true),
            f: static (state, value) => (Count: state.Count + 1, Delta: value - state.Mean, Square: value * value) switch {
                (int count, double delta, double square) => (
                    Count: count, Mean: state.Mean + (delta / count), M2: state.M2 + (delta * (value - (state.Mean + (delta / count)))), SumSquares: state.SumSquares + square, Minimum: Math.Min(val1: state.Minimum, val2: value), Maximum: Math.Max(val1: state.Maximum, val2: value), AllFinite: state.AllFinite && RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: square)),
            }) switch {
                (0, _, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                (_, _, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                (int count, double mean, double m2, double sumSquares, double minimum, double maximum, _) => Fin.Succ(new Stats(
                    count: count, minimum: minimum, maximum: maximum, mean: mean, variance: Math.Max(val1: 0.0, val2: m2 / count), rms: Math.Sqrt(d: sumSquares / count))),
            };
}
internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: +1);
    internal static Seq<TItem> Minima<TItem>(this Seq<TItem> items, Func<TItem, double> projection, double tolerance) =>
        Extrema(items: items, projection: projection, tolerance: tolerance, direction: -1);
    private static Seq<TItem> Extrema<TItem>(Seq<TItem> items, Func<TItem, double> projection, double tolerance, int direction) =>
        items.Fold(
            initialState: (Best: direction > 0 ? double.NegativeInfinity : double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection, Direction: (double)direction),
            f: static (state, item) => state.Projection(arg: item) switch {
                double score when state.Direction * score > (state.Direction * state.Best) + state.Tolerance => state with { Best = score, Hits = Seq(item) },
                double score when state.Direction * score >= (state.Direction * state.Best) - state.Tolerance => state with { Best = state.Direction * score > state.Direction * state.Best ? score : state.Best, Hits = item.Cons(state.Hits) },
                _ => state,
            }).Hits.Rev();
}

// --- [GEOMETRY_CLASSIFIER] --------------------------------------------------------------
// Single SmartEnum that owns "which geometry types have a native bounding box and how to
// compute it". Bounds.cs / Topology.cs delegate to this instead of duplicating the type list.
// Lookup is type-keyed (Lazy Map built once from Items) — no predicate-based search, hence no
// closure captures. The Native catch-all covers any GeometryBase subclass not enumerated.
[SmartEnum<int>]
internal sealed partial class GeometryClassifier {
    private delegate Fin<BoundingBox> BoundsFn(object geometry, Op key);
    public static readonly GeometryClassifier Point3d = new(key: 0, type: typeof(Point3d),
        bounds: static (g, key) => g switch {
            Point3d { IsValid: true } p => Fin.Succ(new BoundingBox(min: p, max: p)),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier BoundingBox = new(key: 1, type: typeof(BoundingBox),
        bounds: static (g, key) => g switch {
            BoundingBox { IsValid: true } b => Fin.Succ(b),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Box = new(key: 2, type: typeof(Box),
        bounds: static (g, key) => g switch {
            Box { IsValid: true } b => Fin.Succ(b.BoundingBox),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Sphere = new(key: 3, type: typeof(Sphere),
        bounds: static (g, key) => g switch {
            Sphere { IsValid: true } s => Fin.Succ(s.BoundingBox),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Line = new(key: 4, type: typeof(Line),
        bounds: static (g, key) => g switch {
            Line { IsValid: true } l => Fin.Succ(l.BoundingBox),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Polyline = new(key: 5, type: typeof(Polyline),
        bounds: static (g, key) => g switch {
            Polyline p => Fin.Succ(p.BoundingBox),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Circle = new(key: 6, type: typeof(Circle),
        bounds: static (g, key) => g switch {
            Circle c => Fin.Succ(c.BoundingBox),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Arc = new(key: 7, type: typeof(Arc),
        bounds: static (g, key) => g switch {
            Arc { IsValid: true } a => Optional(a.ToNurbsCurve())
                .ToFin(key.InvalidResult())
                .Map(static curve => { using NurbsCurve disposable = curve; return disposable.GetBoundingBox(accurate: true); }),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Cylinder = new(key: 8, type: typeof(Cylinder),
        bounds: static (g, key) => g switch {
            Cylinder { IsValid: true } c => Optional(c.ToBrep(capBottom: true, capTop: true))
                .ToFin(key.InvalidResult())
                .Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Cone = new(key: 9, type: typeof(Cone),
        bounds: static (g, key) => g switch {
            Cone { IsValid: true } c => Optional(c.ToBrep(capBottom: true))
                .ToFin(key.InvalidResult())
                .Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Torus = new(key: 10, type: typeof(Torus),
        bounds: static (g, key) => g switch {
            Torus { IsValid: true } t => Optional(t.ToBrep())
                .ToFin(key.InvalidResult())
                .Map(static brep => { using Brep disposable = brep; return disposable.GetBoundingBox(accurate: true); }),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    public static readonly GeometryClassifier Native = new(key: 11, type: typeof(GeometryBase),
        bounds: static (g, key) => g switch {
            GeometryBase { IsValid: true } b => Fin.Succ(b.GetBoundingBox(accurate: true)),
            _ => Fin.Fail<BoundingBox>(key.InvalidInput()),
        });
    internal Type Type { get; }
    private BoundsFn Bounds { get; }
    // FrozenDictionary on `Type` — three structural reasons:
    // (1) `Type` keys lack a usable LanguageExt trait: the v5 reflection-based Ord/Hashable
    //     resolvers walk every loaded assembly via `Module.GetDefinedTypes()` and one of the Rhino
    //     assemblies throws during enumeration, poisoning Map<Type,_> and HashMap<Type,_>.
    // (2) Referencing the items by name avoids the SmartEnum `Items` collection, whose `Lazy`
    //     backing field lives in the auto-generated partial; cross-partial field-initialiser
    //     ordering is implementation-defined, so `_lookups` may still be null at this point.
    // (3) FrozenDictionary is the .NET 9 optimal read-only hash dictionary — eager construction at
    //     cctor, allocation-free lookups, no Lazy indirection, no per-call overhead.
    // The dictionary-initialiser indexer form tolerates the typeof(int) collision between Index and
    // Integer (Integer wins by declaration order, matching the typeof(int) special case in `For`).
    private static readonly FrozenDictionary<Type, GeometryClassifier> Lookup = BuildLookup();
    [BoundaryAdapter]
    private static FrozenDictionary<Type, GeometryClassifier> BuildLookup() =>
        new Dictionary<Type, GeometryClassifier> {
            [Point3d.Type] = Point3d,
            [BoundingBox.Type] = BoundingBox,
            [Box.Type] = Box,
            [Sphere.Type] = Sphere,
            [Line.Type] = Line,
            [Polyline.Type] = Polyline,
            [Circle.Type] = Circle,
            [Arc.Type] = Arc,
            [Cylinder.Type] = Cylinder,
            [Cone.Type] = Cone,
            [Torus.Type] = Torus,
            [Native.Type] = Native,
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
        (For(type: type).IsSome || type == typeof(object)) && (includeSphere || type != typeof(Sphere));
}
