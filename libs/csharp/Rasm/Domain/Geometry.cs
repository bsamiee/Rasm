using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using static LanguageExt.Prelude;
namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct Shape {
    private Shape(object inner) => Inner = inner;
    public object Inner { get; }
    public const string Accepted = "Rhino/GH geometry convertible through native RhinoCommon or GH2 brokers";
    internal Fin<Shape> ValidateWith(Op key) =>
        key.RequireValid(value: Inner)
            .Map(static value => new Shape(inner: value));
    public static Fin<Shape> Create(object? value) =>
        Optional(value)
            .ToFin(new Op(name: nameof(Shape)).InvalidInput())
            .Bind(static raw => new Op(name: nameof(Shape))
                .RequireValid(value: raw)
                .Map(static valid => new Shape(inner: valid)));
}

public enum GeometryKind { Unknown = 0, Curve = 1, Polyline = 2, Mesh = 3, SubD = 4, Surface = 5, BrepGeneral = 10, BrepBox = 11, BrepSphere = 12, BrepCylinder = 13, BrepCone = 14, BrepTorus = 15, BrepPlane = 16, Line = 20, Sphere = 21, Box = 22, BoundingBox = 23, Cylinder = 24, Cone = 25, Torus = 26, Plane = 27 }
public enum CurveFeature { Input, Segment, Edge, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoop, InnerLoop, IsoU, IsoV, Silhouette, SubCurve, Draft }

[StructLayout(LayoutKind.Auto)]
internal readonly record struct FaceProjection {
    private FaceProjection(Brep brep, int faceIndex, bool reversed) { Brep = brep; FaceIndex = faceIndex; Reversed = reversed; }
    internal Brep Brep { get; }
    internal int FaceIndex { get; }
    internal bool Reversed { get; }
    internal static FaceProjection From(BrepFace face) =>
        new(brep: face.DuplicateFace(duplicateMeshes: false), faceIndex: face.FaceIndex, reversed: face.OrientationIsReversed);
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

// --- [OPERATIONS] ----------------------------------------------------------------------

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
            Shape shape => shape.ValidateWith(key: key).Map(static valid => (TValue)(object)valid),
            IntersectionEvent intersection => key.Require(condition:
                (intersection.IsPoint || intersection.IsOverlap)
                && intersection.PointA.IsValid
                && intersection.PointB.IsValid, value: value),
            ValueTuple<double, Vector3d> principal => key.Require(condition: RhinoMath.IsValidDouble(x: principal.Item1) && principal.Item2.IsValid, value: value),
            double scalar => key.Require(condition: RhinoMath.IsValidDouble(x: scalar), value: value),
            bool or int or Enum => Fin.Succ(value),
            _ => Fin.Fail<TValue>(key.InvalidResult()),
        };
    private static Fin<TValue> Require<TValue>(this Op key, bool condition, TValue value) =>
        condition ? Fin.Succ(value) : Fin.Fail<TValue>(key.InvalidResult());
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
            f: static (acc, value) => (Count: acc.Count + 1, Delta: value - acc.Mean, Square: value * value) switch {
                (int count, double delta, double square) => (
                    Count: count,
                    Mean: acc.Mean + (delta / count),
                    M2: acc.M2 + (delta * (value - (acc.Mean + (delta / count)))),
                    SumSquares: acc.SumSquares + square,
                    Minimum: Math.Min(val1: acc.Minimum, val2: value),
                    Maximum: Math.Max(val1: acc.Maximum, val2: value),
                    AllFinite: acc.AllFinite && RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: square)),
            }) switch {
                (0, _, _, _, _, _, _) => Fin.Fail<Stats>(key.InvalidResult()),
                (_, _, _, _, _, _, false) => Fin.Fail<Stats>(key.InvalidResult()),
                (int count, double mean, double m2, double sumSquares, double minimum, double maximum, _) => Fin.Succ(new Stats(
                    count: count,
                    minimum: minimum,
                    maximum: maximum,
                    mean: mean,
                    variance: Math.Max(val1: 0.0, val2: m2 / count),
                    rms: Math.Sqrt(d: sumSquares / count))),
            };
}

internal static class FoldExtensions {
    internal static Seq<TItem> Maxima<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.NegativeInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection),
                f: static (acc, item) =>
                    acc.Projection(arg: item) switch {
                        double s when s > acc.Best + acc.Tolerance => acc with { Best = s, Hits = Seq(item) },
                        double s when s >= acc.Best - acc.Tolerance => acc with { Best = Math.Max(val1: acc.Best, val2: s), Hits = item.Cons(acc.Hits) },
                        _ => acc,
                    })
            .Hits.Rev();
    internal static Seq<TItem> Minima<TItem>(
        this Seq<TItem> items,
        Func<TItem, double> projection,
        double tolerance) =>
        items
            .Fold(
                initialState: (Best: double.PositiveInfinity, Hits: Seq<TItem>(), Tolerance: tolerance, Projection: projection),
                f: static (acc, item) =>
                    acc.Projection(arg: item) switch {
                        double s when s < acc.Best - acc.Tolerance => acc with { Best = s, Hits = Seq(item) },
                        double s when s <= acc.Best + acc.Tolerance => acc with { Best = Math.Min(val1: acc.Best, val2: s), Hits = item.Cons(acc.Hits) },
                        _ => acc,
                    })
            .Hits.Rev();
}
