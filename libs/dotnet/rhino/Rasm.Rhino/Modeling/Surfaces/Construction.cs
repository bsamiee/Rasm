using System.Globalization;
using MathNet.Numerics;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SurfaceDirection { U = 0, V = 1 }

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SurfaceDegree : System.Numerics.IMinMaxValue<SurfaceDegree> {
    public static SurfaceDegree MinValue { get; } = new(1);
    public static SurfaceDegree MaxValue { get; } = new(11);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<IsoStatus>(SkipIParsable = true, SkipIComparable = true)]
public readonly partial struct SurfaceEdge {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref IsoStatus value) =>
        validationError = value is IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North
            ? null
            : new ValidationError($"{value} names no surface edge, which is {nameof(IsoStatus.West)}, {nameof(IsoStatus.South)}, {nameof(IsoStatus.East)} or {nameof(IsoStatus.North)}");
}

[ComplexValueObject]
public sealed partial class PointGrid {
    public Seq<Point3d> Points { get; }

    public int UCount { get; }

    public int VCount => Points.Count / UCount;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Point3d> points, ref int uCount) =>
        validationError = uCount >= 2 && points.Count / uCount >= 2 && points.Count % uCount is 0
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{points.Count} points form no grid of at least 2 by 2 with {uCount} in U"));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PointSurface {
    public sealed record Through(bool UClosed, bool VClosed) : PointSurface;

    public sealed record ControlPoints : PointSurface;
}

[ComplexValueObject(SkipToString = true)]
public sealed partial class HermiteGrid {
    public AscendingParameters U { get; }

    public AscendingParameters V { get; }

    public Seq<(Point3d Point, Vector3d UTangent, Vector3d VTangent, Vector3d Twist)> Stations { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref AscendingParameters u, ref AscendingParameters v, ref Seq<(Point3d Point, Vector3d UTangent, Vector3d VTangent, Vector3d Twist)> stations) =>
        validationError = stations.Count == u.Values.Count * v.Values.Count
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{stations.Count} Hermite stations fill no {u.Values.Count} by {v.Values.Count} grid"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceForm {
    public sealed record Paraboloid(Plane Frame, double CurvatureU, double CurvatureV, Interval U, Interval V) : SurfaceForm;

    public sealed record Hyperboloid(Plane Frame, double Waist, double Axial, Interval S) : SurfaceForm;

    public sealed record Polynomial : SurfaceForm {
        private Polynomial(Seq<Seq<Point4d>> coefficients, Interval u, Interval v) => (Coefficients, U, V) = (coefficients, u, v);

        public Seq<Seq<Point4d>> Coefficients { get; }

        public Interval U { get; }

        public Interval V { get; }

        public static Fin<Polynomial> Validated(Seq<Seq<Point4d>> coefficients, Interval u, Interval v) =>
            from uDegree in SurfaceDegree.Validated(coefficients.Count - 1)
            from vDegree in Callbacks.Each(coefficients, static (row, index) => SurfaceDegree.Validated(row.Count - 1).MapFail(error => Error.New(ErrorCodes.ValidationFailed,
                string.Create(CultureInfo.InvariantCulture, $"polynomial coefficient row {index}: {error.Message}"))))
            from rectangular in guard(vDegree.Distinct().Count is 1, Error.New(ErrorCodes.ValidationFailed, "polynomial coefficient rows differ in length")).ToFin()
            from spanned in guard(u.IsIncreasing && v.IsIncreasing, Error.New(ErrorCodes.ValidationFailed, $"polynomial domains {u} and {v} are not increasing")).ToFin()
            select new Polynomial(coefficients, u, v);

        internal Seq<Seq<Point4d>> Net => Bernstein(Shifted(Coefficients, U, V));

        private static Seq<Seq<Point4d>> Shifted(Seq<Seq<Point4d>> a, Interval u, Interval v) =>
            Grid(a, (k, l) => Sum(Range(k, a.Count - k), i => Sum(Range(l, a[0].Count - l), j =>
                a[i][j] * (SpecialFunctions.Binomial(i, k) * Math.Pow(u.T0, i - k) * Math.Pow(u.Length, k) * SpecialFunctions.Binomial(j, l) * Math.Pow(v.T0, j - l) * Math.Pow(v.Length, l)))));

        private static Seq<Seq<Point4d>> Bernstein(Seq<Seq<Point4d>> b) =>
            Grid(b, (p, q) => Sum(Range(0, p + 1), k => Sum(Range(0, q + 1), l =>
                b[k][l] * (SpecialFunctions.Binomial(p, k) / SpecialFunctions.Binomial(b.Count - 1, k) * SpecialFunctions.Binomial(q, l) / SpecialFunctions.Binomial(b[0].Count - 1, l)))));

        private static Seq<Seq<Point4d>> Grid(Seq<Seq<Point4d>> shape, Func<int, int, Point4d> entry) =>
            toSeq(Range(0, shape.Count)).Map(row => toSeq(Range(0, shape[0].Count)).Map(column => entry(row, column)).Strict()).Strict();

        private static Point4d Sum(IEnumerable<int> indices, Func<int, Point4d> term) =>
            toSeq(indices).Fold(new Point4d(0d, 0d, 0d, 0d), (sum, index) => sum + term(index));
    }

    public IO<Surface> ToSurface(Action<IDisposable> retain) =>
        Switch(
            retain,
            paraboloid: static (held, paraboloid) => DisposalOps.Within<IDisposable, Surface>(temporary =>
                from a in Parabola((x, z) => paraboloid.Frame.PointAt(x, paraboloid.V.T0, z + (paraboloid.CurvatureV * paraboloid.V.T0 * paraboloid.V.T0)), paraboloid.CurvatureU, paraboloid.U, temporary)
                from b in Parabola((y, z) => paraboloid.Frame.PointAt(0d, y, z), paraboloid.CurvatureV, paraboloid.V, temporary)
                from sum in Copies.Acquire(nameof(SumSurface.Create), _ => Answer.One<Surface>(SumSurface.Create(a, b)), held)
                select sum, DisposalOps.Release),
            hyperboloid: static (held, hyperboloid) => Copies.Acquire(nameof(RevSurface.Create), _ => Answer.One<Surface>(RevSurface.Create(
                new Line(
                    hyperboloid.Frame.PointAt(hyperboloid.Waist, hyperboloid.Waist * hyperboloid.S.T0, hyperboloid.Axial * hyperboloid.S.T0),
                    hyperboloid.Frame.PointAt(hyperboloid.Waist, hyperboloid.Waist * hyperboloid.S.T1, hyperboloid.Axial * hyperboloid.S.T1)),
                new Line(hyperboloid.Frame.Origin, hyperboloid.Frame.ZAxis, 1d))), held),
            polynomial: static (held, polynomial) => polynomial.Net switch {
                var net => Copies.Acquire(nameof(NurbsSurface.CreateFromPoints), _ =>
                    NurbsSurface.CreateFromPoints(net.Bind(static row => row.Map(static point => new Point3d(point))), net.Count, net[0].Count, net.Count - 1, net[0].Count - 1) switch {
                        var surface => (
                                Answer.Accepted(surface is not null
                                    && (net.ForAll(static row => row.ForAll(static point => point.W is 1d)) || surface.MakeRational())
                                    && net.Map((row, u) => row.Map((point, v) => surface.Points.SetPoint(u, v, point)).ForAll(static set => set)).ForAll(static set => set)
                                    && surface.SetDomain((int)SurfaceDirection.U, polynomial.U)
                                    && surface.SetDomain((int)SurfaceDirection.V, polynomial.V)),
                                Answer.One<Surface>(surface))
                            .Apply(static (_, built) => built).As(),
                    }, held),
            });

    private static IO<NurbsCurve> Parabola(Func<double, double, Point3d> at, double curvature, Interval span, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(BezierCurve.ToNurbsCurve), _ => Answer.Successor(
            new BezierCurve([
                at(span.T0, curvature * span.T0 * span.T0), at((span.T0 + span.T1) / 2d, curvature * span.T0 * span.T1), at(span.T1, curvature * span.T1 * span.T1),
            ]),
            static bezier => Answer.One(bezier.ToNurbsCurve())), retain);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceConstruction {
    // --- [FROM_POINTS]
    public static IO<NurbsSurface> FromPoints(PointGrid grid, SurfaceDegree uDegree, SurfaceDegree vDegree, PointSurface method, Action<IDisposable> retain) =>
        method.Switch(
            (Grid: grid, U: uDegree, V: vDegree, Retain: retain),
            through: static (at, through) => Fitted(at.Grid, at.U, at.V, nameof(NurbsSurface.CreateThroughPoints))
                >> Copies.Acquire(nameof(NurbsSurface.CreateThroughPoints), _ => Answer.One(
                    NurbsSurface.CreateThroughPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V, through.UClosed, through.VClosed)), at.Retain),
            controlPoints: static (at, _) => Fitted(at.Grid, at.U, at.V, nameof(NurbsSurface.CreateFromPoints))
                >> Copies.Acquire(nameof(NurbsSurface.CreateFromPoints), _ => Answer.One(
                    NurbsSurface.CreateFromPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V)), at.Retain));

    private static IO<Unit> Fitted(PointGrid grid, SurfaceDegree uDegree, SurfaceDegree vDegree, string member) =>
        IO.lift(() => (Conversions.Within(uDegree, grid.UCount, member), Conversions.Within(vDegree, grid.VCount, member)).Apply(static (_, _) => unit).As());

    // --- [HERMITE]
    public static IO<NurbsSurface> Hermite(HermiteGrid grid, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(HermiteSurface.ToNurbsSurface), _ => Answer.Successor(new HermiteSurface(grid.U.Values.Count, grid.V.Values.Count), hermite => Filled(hermite, grid)), retain);

    private static Answer<NurbsSurface> Filled(HermiteSurface hermite, HermiteGrid grid) {
        _ = grid.U.Values.Iter(hermite.SetUParameterAt);
        _ = grid.V.Values.Iter(hermite.SetVParameterAt);
        _ = grid.Stations.Iter((index, station) => {
            (int u, int v) = (index / grid.V.Values.Count, index % grid.V.Values.Count);
            hermite.SetPointAt(u, v, station.Point);
            hermite.SetUTangentAt(u, v, station.UTangent);
            hermite.SetVTangentAt(u, v, station.VTangent);
            hermite.SetTwistAt(u, v, station.Twist);
        });
        return Answer.One(hermite.ToNurbsSurface());
    }

    // --- [REFIT]
    public static IO<(NurbsSurface A, NurbsSurface B)> MakeCompatible(Surface a, Surface b, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(NurbsSurface.MakeCompatible), _ =>
            (Answer.Accepted(NurbsSurface.MakeCompatible(a, b, out NurbsSurface first, out NurbsSurface second)), Answer.One(first), Answer.One(second))
                .Apply(static (_, compatibleA, compatibleB) => (A: compatibleA, B: compatibleB)).As(), retain);
}
