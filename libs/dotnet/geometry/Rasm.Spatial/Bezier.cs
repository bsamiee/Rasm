using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using SuperLinq;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CubicBezier(Vector3D<double> Start, Vector3D<double> StartControl, Vector3D<double> EndControl, Vector3D<double> End);

public sealed record HermiteStation(Vector3D<double> Point, Vector3D<double> In, Vector3D<double> Out);

public sealed record HermiteSpline {
    private HermiteSpline(Seq<HermiteStation> stations, bool closed) =>
        Spans = toSeq((closed ? stations + Seq(stations[0]) : stations).AsIterable().Window(2, static pair =>
            new CubicBezier(pair[0].Point, pair[0].Point + (pair[0].Out / 3d), pair[1].Point - (pair[1].In / 3d), pair[1].Point))).Strict();
    public Seq<CubicBezier> Spans { get; }

    public static Fin<HermiteSpline> Through(Seq<HermiteStation> stations) => SpatialFault.Unless(stations.Count >= 2, SpatialRule.Count, stations.Count).Map(_ => new HermiteSpline(stations, closed: false));

    public static Fin<HermiteSpline> Tcb(Seq<Vector3D<double>> points, double tension, double continuity, double bias, bool closed) =>
        (SpatialFault.Unless(points.Count >= 2, SpatialRule.Count, points.Count), SpatialFault.Unless(double.IsFinite(tension) && double.IsFinite(continuity) && double.IsFinite(bias), SpatialRule.Range, (tension, continuity, bias)))
            .Apply((_, _) => (Before: (1d - tension) * (1d + bias) / 2d, After: (1d - tension) * (1d - bias) / 2d) switch {
                var lean => new HermiteSpline(toSeq((Seq(closed ? points[^1] : points[0] + (points[0] - points[1])) + points + Seq(closed ? points[0] : points[^1] + (points[^1] - points[^2])))
                    .AsIterable().Window(3, span => (Before: span[1] - span[0], After: span[2] - span[1]) switch {
                        var chords => new HermiteStation(span[1], (lean.Before * (1d - continuity) * chords.Before) + (lean.After * (1d + continuity) * chords.After), (lean.Before * (1d + continuity) * chords.Before) + (lean.After * (1d - continuity) * chords.After)),
                    })).Strict(), closed),
            }).As();
}

[ValueObject<double>(SkipFactoryMethods = true)]
public sealed partial class ConicRho {
    public static Fin<ConicRho> Validated(double value) => SpatialFault.Unless(value is > 0d and < 1d, SpatialRule.Range, value).Map(_ => new ConicRho(value));
}

public sealed record RationalQuadratic {
    private RationalQuadratic(Vector3D<double> start, Vector3D<double> apex, Vector3D<double> end, double weight) => (Start, Apex, End, Weight) = (start, apex, end, weight);
    public Vector3D<double> Start { get; }
    public Vector3D<double> Apex { get; }
    public Vector3D<double> End { get; }
    public double Weight { get; }

    public static RationalQuadratic Rho(Vector3D<double> start, Vector3D<double> apex, Vector3D<double> end, ConicRho rho) => new(start, apex, end, (double)rho / (1d - (double)rho));

    public static RationalQuadratic Hyperbola(double a, double b, ParameterSpan u) => double.Cosh(u.Length / 2d) switch { var weight => new(Branch(a, b, u.Start), Branch(a, b, (u.Start + u.End) / 2d) / weight, Branch(a, b, u.End), weight) };

    private static Vector3D<double> Branch(double a, double b, double t) => new(a * double.Cosh(t), b * double.Sinh(t), 0d);
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class HomogeneousGrid {
    public Matrix<double> X { get; }
    public Matrix<double> Y { get; }
    public Matrix<double> Z { get; }
    public Matrix<double> W { get; }

    public static Fin<HomogeneousGrid> Validated(Matrix<double> x, Matrix<double> y, Matrix<double> z, Matrix<double> w) =>
        SpatialFault.Unless(x.RowCount >= 2 && x.ColumnCount >= 2 && Seq(y, z, w).ForAll(component => component.RowCount == x.RowCount && component.ColumnCount == x.ColumnCount), SpatialRule.Shape, Seq(x, y, z, w).Map(static grid => (grid.RowCount, grid.ColumnCount)))
            .Map(_ => new HomogeneousGrid(x, y, z, w));

    internal static HomogeneousGrid Paraboloid(double curvatureU, double curvatureV) =>
        new(Matrix<double>.Build.DenseOfRowArrays([0d, 0d, 0d], [1d, 0d, 0d], [0d, 0d, 0d]), Matrix<double>.Build.DenseOfRowArrays([0d, 1d, 0d], [0d, 0d, 0d], [0d, 0d, 0d]),
            Matrix<double>.Build.DenseOfRowArrays([0d, 0d, curvatureV], [0d, 0d, 0d], [curvatureU, 0d, 0d]), Matrix<double>.Build.DenseOfRowArrays([1d, 0d, 0d], [0d, 0d, 0d], [0d, 0d, 0d]));

    internal HomogeneousGrid Bernstein(ParameterSpan u, ParameterSpan v) =>
        (Rows: Basis(X.RowCount - 1, u), Columns: Basis(X.ColumnCount - 1, v)) switch { var basis => Seq(X, Y, Z, W).Map(component => basis.Rows * component.TransposeAndMultiply(basis.Columns)) switch { var net => new(net[0], net[1], net[2], net[3]) } };

    private static Matrix<double> Basis(int degree, ParameterSpan span) =>
        Matrix<double>.Build.Dense(degree + 1, degree + 1, (row, power) => SpecialFunctions.Binomial(row, power) / SpecialFunctions.Binomial(degree, power))
        * Matrix<double>.Build.Dense(degree + 1, degree + 1, (power, column) => power <= column ? SpecialFunctions.Binomial(column, power) * double.Pow(span.Start, column - power) * double.Pow(span.Length, power) : 0d);
}

public sealed record PolynomialPatch {
    private PolynomialPatch(HomogeneousGrid power, ParameterSpan u, ParameterSpan v) => (Power, U, V, Net) = (power, u, v, power.Bernstein(u, v));
    public HomogeneousGrid Power { get; }
    public ParameterSpan U { get; }
    public ParameterSpan V { get; }
    public HomogeneousGrid Net { get; }

    public static PolynomialPatch Of(HomogeneousGrid power, ParameterSpan u, ParameterSpan v) => new(power, u, v);
    public static PolynomialPatch Paraboloid(double curvatureU, double curvatureV, ParameterSpan u, ParameterSpan v) => new(HomogeneousGrid.Paraboloid(curvatureU, curvatureV), u, v);
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class BezierVolume {
    public int UDegree { get; }
    public int VDegree { get; }
    public int WDegree { get; }
    public Seq<Vector3D<double>> Controls { get; }

    public Vector3D<double> At(Vector3D<double> uvw) =>
        Controls.Zip(Seq(Bernstein(VDegree, uvw.Y), Bernstein(WDegree, uvw.Z)).Fold(Bernstein(UDegree, uvw.X), static (tensor, axis) => from weight in axis from held in tensor select weight * held))
            .Fold(new Vector3D<double>(0d, 0d, 0d), static (sum, term) => sum + (term.Second * term.First));

    public static Fin<BezierVolume> Validated(int uDegree, int vDegree, int wDegree, Seq<Vector3D<double>> controls) =>
        (SpatialFault.Unless(uDegree >= 1 && vDegree >= 1 && wDegree >= 1, SpatialRule.Range, (uDegree, vDegree, wDegree)), SpatialFault.Unless(controls.Count == (uDegree + 1L) * (vDegree + 1L) * (wDegree + 1L), SpatialRule.Count, controls.Count))
            .Apply((_, _) => new BezierVolume(uDegree, vDegree, wDegree, controls)).As();

    private static Seq<double> Bernstein(int degree, double at) => toSeq(Range(0, degree + 1)).Map(index => SpecialFunctions.Binomial(degree, index) * double.Pow(at, index) * double.Pow(1d - at, degree - index)).Strict();
}
