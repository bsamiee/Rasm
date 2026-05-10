using System.Runtime.InteropServices;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Thinktecture;
using static LanguageExt.Prelude;
namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------

[Union]
public partial record Shape {
    public const string Accepted = "GeometryBase, Point3d, Box, BoundingBox, Line, Polyline, Plane, Sphere, Cylinder, Cone, Torus, Circle, Arc";

    public sealed record Native(GeometryBase Geometry) : Shape;
    public sealed record Point(Rhino.Geometry.Point3d Value) : Shape;
    public sealed record Box(Rhino.Geometry.Box Value) : Shape;
    public sealed record BoundingBox(Rhino.Geometry.BoundingBox Value) : Shape;
    public sealed record Line(Rhino.Geometry.Line Value) : Shape;
    public sealed record Polyline(Rhino.Geometry.Polyline Value) : Shape;
    public sealed record Plane(Rhino.Geometry.Plane Value) : Shape;
    public sealed record Sphere(Rhino.Geometry.Sphere Value) : Shape;
    public sealed record Cylinder(Rhino.Geometry.Cylinder Value) : Shape;
    public sealed record Cone(Rhino.Geometry.Cone Value) : Shape;
    public sealed record Torus(Rhino.Geometry.Torus Value) : Shape;
    public sealed record Circle(Rhino.Geometry.Circle Value) : Shape;
    public sealed record Arc(Rhino.Geometry.Arc Value) : Shape;
    public Fin<Shape> Validate() =>
        ValidateWith(key: new Op(name: nameof(Shape)));
    internal Fin<Shape> ValidateWith(Op key) =>
        key.RequireValid(value: Inner).Bind(static value => From(value: value).ToFin(Error.New(message: "Shape payload is not supported.")));
    public object Inner =>
        Switch<object>(
            native: static n => n.Geometry,
            point: static point => point.Value,
            box: static b => b.Value,
            boundingBox: static bbox => bbox.Value,
            line: static line => line.Value,
            polyline: static polyline => polyline.Value,
            plane: static plane => plane.Value,
            sphere: static sphere => sphere.Value,
            cylinder: static cylinder => cylinder.Value,
            cone: static cone => cone.Value,
            torus: static torus => torus.Value,
            circle: static circle => circle.Value,
            arc: static arc => arc.Value);
    public static Option<Shape> From(object value) =>
        value switch {
            GeometryBase geometry => Some<Shape>(new Native(Geometry: geometry)),
            Rhino.Geometry.Point3d point => Some<Shape>(new Point(Value: point)),
            Rhino.Geometry.Box box => Some<Shape>(new Box(Value: box)),
            Rhino.Geometry.BoundingBox bbox => Some<Shape>(new BoundingBox(Value: bbox)),
            Rhino.Geometry.Line line => Some<Shape>(new Line(Value: line)),
            Rhino.Geometry.Polyline polyline => Some<Shape>(new Polyline(Value: polyline)),
            Rhino.Geometry.Plane plane => Some<Shape>(new Plane(Value: plane)),
            Rhino.Geometry.Sphere sphere => Some<Shape>(new Sphere(Value: sphere)),
            Rhino.Geometry.Cylinder cylinder => Some<Shape>(new Cylinder(Value: cylinder)),
            Rhino.Geometry.Cone cone => Some<Shape>(new Cone(Value: cone)),
            Rhino.Geometry.Torus torus => Some<Shape>(new Torus(Value: torus)),
            Rhino.Geometry.Circle circle => Some<Shape>(new Circle(Value: circle)),
            Rhino.Geometry.Arc arc => Some<Shape>(new Arc(Value: arc)),
            _ => None,
        };
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
        condition switch {
            true => Fin.Succ(value),
            false => Fin.Fail<TValue>(key.InvalidResult()),
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
                        double s when s >= acc.Best - acc.Tolerance => acc with { Best = Math.Max(val1: acc.Best, val2: s), Hits = acc.Hits.Add(item) },
                        _ => acc,
                    })
            .Hits;
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
                        double s when s <= acc.Best + acc.Tolerance => acc with { Best = Math.Min(val1: acc.Best, val2: s), Hits = acc.Hits.Add(item) },
                        _ => acc,
                    })
            .Hits;
}
