using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PointWeight : System.Numerics.IMinMaxValue<PointWeight> {
    public static PointWeight MinValue { get; } = new(double.BitIncrement(0d));
    public static PointWeight MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RebuildPointCount : System.Numerics.IMinMaxValue<RebuildPointCount> {
    public static RebuildPointCount MinValue { get; } = new(2);
    public static RebuildPointCount MaxValue { get; } = new(1000);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceEdit {
    public sealed record Rebuild : SurfaceEdit {
        private Rebuild(SurfaceDegree u, SurfaceDegree v, RebuildPointCount uCount, RebuildPointCount vCount) => (U, V, UCount, VCount) = (u, v, uCount, vCount);

        public SurfaceDegree U { get; }

        public SurfaceDegree V { get; }

        public RebuildPointCount UCount { get; }

        public RebuildPointCount VCount { get; }

        public static Fin<Rebuild> Validated(SurfaceDegree u, SurfaceDegree v, RebuildPointCount uCount, RebuildPointCount vCount) =>
            (int)uCount > u && (int)vCount > v
                ? new Rebuild(u, v, uCount, vCount)
                : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"rebuild counts {uCount} by {vCount} need to exceed degrees {u} by {v}"));
    }

    public sealed record RebuildOneDirection(SurfaceDirection Direction, int Count, LoftType Loft) : SurfaceEdit;
    public sealed record Fit(SurfaceDegree U, SurfaceDegree V, double Tolerance) : SurfaceEdit;
    public sealed record SubDFriendly : SurfaceEdit;
    public sealed record Offset(double Distance) : SurfaceEdit;
    public sealed record VariableOffset((double UMinVMin, double UMinVMax, double UMaxVMin, double UMaxVMax) Corners, Seq<(Point2d Parameter, double Distance)> Interior) : SurfaceEdit;
    public sealed record Smooth(SmoothOptions Options) : SurfaceEdit;
    public sealed record ExtendEdge(SurfaceEdge Edge, double Length, bool SmoothExtension) : SurfaceEdit;
    public sealed record ExtendDomain(SurfaceDirection Direction, Interval Interval) : SurfaceEdit;
    public sealed record Trim(Interval U, Interval V) : SurfaceEdit;
    public sealed record Transpose : SurfaceEdit;
    public sealed record Reverse(SurfaceDirection Direction) : SurfaceEdit;
    public sealed record SetDomain(SurfaceDirection Direction, Interval Domain) : SurfaceEdit;
    public sealed record Periodic(SurfaceDirection Direction, bool SmoothSeam) : SurfaceEdit;

    public IO<EditStep<Surface, int>> Step(Tolerances tolerances, Surface surface, Action<IDisposable> retain) =>
        Switch(
            (Surface: surface, Tolerances: tolerances, Retain: retain),
            rebuild: static (at, of) => Copies.Acquire(nameof(Surface.Rebuild), _ => Edits.Replaced<Surface, int>(at.Surface.Rebuild(of.U, of.V, of.UCount, of.VCount)), at.Retain),
            rebuildOneDirection: static (at, of) => Copies.Acquire(nameof(Surface.RebuildOneDirection), _ =>
                Edits.Replaced<Surface, int>(at.Surface.RebuildOneDirection((int)of.Direction, of.Count, of.Loft, at.Tolerances.Absolute)), at.Retain),
            fit: static (at, of) => Copies.Acquire(nameof(Surface.Fit), _ => Edits.Replaced<Surface, int>(at.Surface.Fit(of.U, of.V, of.Tolerance)), at.Retain),
            subDFriendly: static (at, _) => Copies.Acquire(nameof(NurbsSurface.CreateSubDFriendly), _ => Edits.Replaced<Surface, int>(NurbsSurface.CreateSubDFriendly(at.Surface)), at.Retain),
            offset: static (at, of) => Copies.Acquire(nameof(Surface.Offset), _ => Edits.Replaced<Surface, int>(at.Surface.Offset(of.Distance, at.Tolerances.Absolute)), at.Retain),
            variableOffset: static (at, of) => Copies.Acquire(nameof(Surface.VariableOffset), _ => Edits.Replaced<Surface, int>(of.Interior.IsEmpty
                ? at.Surface.VariableOffset(of.Corners.UMinVMin, of.Corners.UMinVMax, of.Corners.UMaxVMin, of.Corners.UMaxVMax, at.Tolerances.Absolute)
                : at.Surface.VariableOffset(
                    of.Corners.UMinVMin, of.Corners.UMinVMax, of.Corners.UMaxVMin, of.Corners.UMaxVMax,
                    of.Interior.Map(static row => row.Parameter), of.Interior.Map(static row => row.Distance), at.Tolerances.Absolute)), at.Retain),
            smooth: static (at, of) => of.Options.Coordinates switch {
                var (system, plane) => Copies.Acquire(nameof(Surface.Smooth), _ => Edits.Replaced<Surface, int>(
                    at.Surface.Smooth(of.Options.Factor, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)), at.Retain),
            },
            extendEdge: static (at, of) => Copies.Acquire(nameof(Surface.Extend), _ => Edits.Replaced<Surface, int>(at.Surface.Extend(of.Edge, of.Length, of.SmoothExtension)), at.Retain),
            extendDomain: static (at, of) => Copies.Acquire(nameof(Surface.Extend), _ => Edits.Accepted<Surface, int>(at.Surface, at.Surface.Extend((int)of.Direction, of.Interval)), at.Retain),
            trim: static (at, of) => Copies.Acquire(nameof(Surface.Trim), _ => Edits.Replaced<Surface, int>(at.Surface.Trim(of.U, of.V)), at.Retain),
            transpose: static (at, _) => Copies.Acquire(nameof(Surface.Transpose), _ => Edits.Accepted<Surface, int>(at.Surface, at.Surface.Transpose(inPlace: true) is not null), at.Retain),
            reverse: static (at, of) => Copies.Acquire(nameof(Surface.Reverse), _ =>
                Edits.Accepted<Surface, int>(at.Surface, at.Surface.Reverse((int)of.Direction, inPlace: true) is not null), at.Retain),
            setDomain: static (at, of) => Copies.Acquire(nameof(Surface.SetDomain), _ => Edits.Accepted<Surface, int>(at.Surface, at.Surface.SetDomain((int)of.Direction, of.Domain)), at.Retain),
            periodic: static (at, of) => Copies.Acquire(nameof(Surface.CreatePeriodicSurface), _ =>
                Edits.Replaced<Surface, int>(Surface.CreatePeriodicSurface(at.Surface, (int)of.Direction, of.SmoothSeam)), at.Retain));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NurbsSurfaceEdit {
    public sealed record IncreaseDegree(SurfaceDirection Direction, SurfaceDegree Degree) : NurbsSurfaceEdit;
    public sealed record MakeRational : NurbsSurfaceEdit;
    public sealed record MakeNonRational : NurbsSurfaceEdit;
    public sealed record MakeUniform(Option<SurfaceDirection> Direction) : NurbsSurfaceEdit;
    public sealed record InsertKnot(SurfaceDirection Direction, double Value, int Multiplicity) : NurbsSurfaceEdit;
    public sealed record RemoveKnots(SurfaceDirection Direction, int Start, int End) : NurbsSurfaceEdit;
    public sealed record RemoveKnotsAt(SurfaceDirection Direction, Point2d At) : NurbsSurfaceEdit;
    public sealed record SetPoint(int U, int V, Point3d Point, PointWeight Weight) : NurbsSurfaceEdit;
    public sealed record UniformKnots(SurfaceDirection Direction, double Spacing) : NurbsSurfaceEdit;
    public sealed record PeriodicKnots(SurfaceDirection Direction, double Spacing) : NurbsSurfaceEdit;
    public sealed record RemoveMultipleKnots(SurfaceDirection Direction, int Minimum, int Maximum) : NurbsSurfaceEdit;
    public sealed record MatchToCurve(SurfaceEdge Side, Curve Target, double MaxEndDistance, double MaxInteriorDistance, int MaxLevel) : NurbsSurfaceEdit;

    public IO<EditStep<NurbsSurface, int>> Step(Tolerances tolerances, NurbsSurface nurbs, Action<IDisposable> retain) =>
        Switch(
            (Nurbs: nurbs, Tolerances: tolerances, Retain: retain),
            increaseDegree: static (at, of) => of.Direction is SurfaceDirection.U
                ? Copies.Acquire(nameof(NurbsSurface.IncreaseDegreeU), _ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.IncreaseDegreeU(of.Degree)), at.Retain)
                : Copies.Acquire(nameof(NurbsSurface.IncreaseDegreeV), _ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.IncreaseDegreeV(of.Degree)), at.Retain),
            makeRational: static (at, _) => Copies.Acquire(nameof(NurbsSurface.MakeRational), _ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeRational()), at.Retain),
            makeNonRational: static (at, _) => Copies.Acquire(nameof(NurbsSurface.MakeNonRational), _ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeNonRational()), at.Retain),
            makeUniform: static (at, of) => Copies.Acquire(nameof(NurbsSurface.MakeUniform), _ =>
                Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeUniform(of.Direction.Match(static direction => (int)direction, 2))), at.Retain),
            insertKnot: static (at, of) => Copies.Acquire(nameof(NurbsSurfaceKnotList.InsertKnot), _ =>
                Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).InsertKnot(of.Value, of.Multiplicity)), at.Retain),
            removeKnots: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.Start, Knots(at.Nurbs, of.Direction).Count, nameof(NurbsSurfaceKnotList.RemoveKnots)),
                        Conversions.Within(of.End - 1, Knots(at.Nurbs, of.Direction).Count, nameof(NurbsSurfaceKnotList.RemoveKnots)))
                    .Apply(static (_, _) => unit).As())
                >> Copies.Acquire(nameof(NurbsSurfaceKnotList.RemoveKnots), _ =>
                    Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveKnots(of.Start, of.End)), at.Retain),
            removeKnotsAt: static (at, of) => Copies.Acquire(nameof(NurbsSurfaceKnotList.RemoveKnotsAt), _ =>
                Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveKnotsAt(of.At.X, of.At.Y)), at.Retain),
            setPoint: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.U, at.Nurbs.Points.CountU, nameof(NurbsSurfacePointList.SetPoint)), Conversions.Within(of.V, at.Nurbs.Points.CountV, nameof(NurbsSurfacePointList.SetPoint)))
                    .Apply(static (_, _) => unit).As())
                >> Copies.Acquire(nameof(NurbsSurfacePointList.SetPoint), _ =>
                    Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.Points.SetPoint(of.U, of.V, of.Point, of.Weight)), at.Retain),
            uniformKnots: static (at, of) => Copies.Acquire(nameof(NurbsSurfaceKnotList.CreateUniformKnots), _ =>
                Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).CreateUniformKnots(of.Spacing)), at.Retain),
            periodicKnots: static (at, of) => Copies.Acquire(nameof(NurbsSurfaceKnotList.CreatePeriodicKnots), _ =>
                Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).CreatePeriodicKnots(of.Spacing)), at.Retain),
            removeMultipleKnots: static (at, of) => Copies.Acquire(nameof(NurbsSurfaceKnotList.RemoveMultipleKnots), _ =>
                Edits.Count(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveMultipleKnots(of.Minimum, of.Maximum, at.Tolerances.Absolute)), at.Retain),
            matchToCurve: static (at, of) => Copies.Acquire(nameof(NurbsSurface.MatchToCurve), _ =>
                Edits.Replaced<NurbsSurface, int>(at.Nurbs.MatchToCurve(of.Side, of.Target, of.MaxEndDistance, of.MaxInteriorDistance, at.Tolerances.Absolute, of.MaxLevel)), at.Retain));

    private static NurbsSurfaceKnotList Knots(NurbsSurface nurbs, SurfaceDirection direction) => direction is SurfaceDirection.U ? nurbs.KnotsU : nurbs.KnotsV;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEdits {
    // --- [EDITS]
    public static IO<(Surface Result, Seq<(SurfaceEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Surface source, Seq<SurfaceEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (surface, edit, held) => edit.Step(tolerances, surface, held), static _ => IO.pure(unit), retain);

    public static IO<(NurbsSurface Result, Seq<(NurbsSurfaceEdit Edit, EditResult<int> Result)> Results)> Edit(
        Tolerances tolerances, NurbsSurface source, Seq<NurbsSurfaceEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (nurbs, edit, held) => edit.Step(tolerances, nurbs, held), static _ => IO.pure(unit), retain);
}
