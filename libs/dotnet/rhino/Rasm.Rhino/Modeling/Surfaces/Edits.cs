using Rasm.RhinoCommon.Document.Units;
using Rhino.Geometry.Collections;

namespace Rasm.RhinoCommon.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record SurfaceEdit {
    public sealed record Rebuild(int U, int V, int UCount, int VCount) : SurfaceEdit;
    public sealed record RebuildOneDirection(SurfaceDirection Direction, int Count, LoftType Loft) : SurfaceEdit;
    public sealed record Fit(int U, int V, double Tolerance) : SurfaceEdit;
    public sealed record SubDFriendly : SurfaceEdit;
    public sealed record Offset(double Distance) : SurfaceEdit;
    public sealed record VariableOffset((double UMinVMin, double UMinVMax, double UMaxVMin, double UMaxVMax) Corners, Seq<(Point2d Parameter, double Distance)> Interior) : SurfaceEdit;
    public sealed record Smooth(SmoothOptions Options) : SurfaceEdit;
    public sealed record ExtendEdge(IsoStatus Edge, double Length, bool SmoothExtension) : SurfaceEdit;
    public sealed record ExtendDomain(SurfaceDirection Direction, Interval Interval) : SurfaceEdit;
    public sealed record Trim(Interval U, Interval V) : SurfaceEdit;
    public sealed record Transpose : SurfaceEdit;
    public sealed record Reverse(SurfaceDirection Direction) : SurfaceEdit;
    public sealed record SetDomain(SurfaceDirection Direction, Interval Domain) : SurfaceEdit;
    public sealed record Periodic(SurfaceDirection Direction, bool SmoothSeam) : SurfaceEdit;

    public IO<EditStep<Surface, int>> Step(Tolerances tolerances, Surface surface) =>
        Switch(
            (Surface: surface, Tolerances: tolerances),
            rebuild: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.Rebuild(of.U, of.V, of.UCount, of.VCount)),
            rebuildOneDirection: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.RebuildOneDirection((int)of.Direction, of.Count, of.Loft, at.Tolerances.Absolute)),
            fit: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.Fit(of.U, of.V, of.Tolerance)),
            subDFriendly: static (at, _) => Edits.Replaced<Surface, int>(NurbsSurface.CreateSubDFriendly(at.Surface)),
            offset: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.Offset(of.Distance, at.Tolerances.Absolute)),
            variableOffset: static (at, of) => Edits.Replaced<Surface, int>(of.Interior.IsEmpty
                ? at.Surface.VariableOffset(of.Corners.UMinVMin, of.Corners.UMinVMax, of.Corners.UMaxVMin, of.Corners.UMaxVMax, at.Tolerances.Absolute)
                : at.Surface.VariableOffset(
                    of.Corners.UMinVMin, of.Corners.UMinVMax, of.Corners.UMaxVMin, of.Corners.UMaxVMax,
                    of.Interior.Map(static row => row.Parameter), of.Interior.Map(static row => row.Distance), at.Tolerances.Absolute)),
            smooth: static (at, of) => of.Options.Coordinates switch {
                var (system, plane) => Edits.Replaced<Surface, int>(at.Surface.Smooth(of.Options.Factor, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)),
            },
            extendEdge: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.Extend(of.Edge, of.Length, of.SmoothExtension)),
            extendDomain: static (at, of) => Edits.Accepted<Surface, int>(at.Surface, at.Surface.Extend((int)of.Direction, of.Interval)),
            trim: static (at, of) => Edits.Replaced<Surface, int>(at.Surface.Trim(of.U, of.V)),
            transpose: static (at, _) => Edits.Accepted<Surface, int>(at.Surface, at.Surface.Transpose(inPlace: true) is not null),
            reverse: static (at, of) => Edits.Accepted<Surface, int>(at.Surface, at.Surface.Reverse((int)of.Direction, inPlace: true) is not null),
            setDomain: static (at, of) => Edits.Accepted<Surface, int>(at.Surface, at.Surface.SetDomain((int)of.Direction, of.Domain)),
            periodic: static (at, of) => Edits.Replaced<Surface, int>(Surface.CreatePeriodicSurface(at.Surface, (int)of.Direction, of.SmoothSeam)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NurbsSurfaceEdit {
    public sealed record IncreaseDegree(SurfaceDirection Direction, int Degree) : NurbsSurfaceEdit;
    public sealed record MakeRational : NurbsSurfaceEdit;
    public sealed record MakeNonRational : NurbsSurfaceEdit;
    public sealed record MakeUniform(Option<SurfaceDirection> Direction) : NurbsSurfaceEdit;
    public sealed record InsertKnot(SurfaceDirection Direction, double Value, int Multiplicity) : NurbsSurfaceEdit;
    public sealed record RemoveKnots(SurfaceDirection Direction, int Start, int End) : NurbsSurfaceEdit;
    public sealed record RemoveKnotsAt(SurfaceDirection Direction, Point2d At) : NurbsSurfaceEdit;
    public sealed record SetPoint(int U, int V, Point3d Point, double Weight) : NurbsSurfaceEdit;
    public sealed record UniformKnots(SurfaceDirection Direction, double Spacing) : NurbsSurfaceEdit;
    public sealed record PeriodicKnots(SurfaceDirection Direction, double Spacing) : NurbsSurfaceEdit;
    public sealed record RemoveMultipleKnots(SurfaceDirection Direction, int Minimum, int Maximum) : NurbsSurfaceEdit;
    public sealed record MatchToCurve(IsoStatus Side, Curve Target, double MaxEndDistance, double MaxInteriorDistance, int MaxLevel) : NurbsSurfaceEdit;

    public IO<EditStep<NurbsSurface, int>> Step(Tolerances tolerances, NurbsSurface nurbs) =>
        Switch(
            (Nurbs: nurbs, Tolerances: tolerances),
            increaseDegree: static (at, of) => of.Direction is SurfaceDirection.U
                ? Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.IncreaseDegreeU(of.Degree))
                : Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.IncreaseDegreeV(of.Degree)),
            makeRational: static (at, _) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeRational()),
            makeNonRational: static (at, _) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeNonRational()),
            makeUniform: static (at, of) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.MakeUniform(of.Direction.Match(static direction => (int)direction, 2))),
            insertKnot: static (at, of) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).InsertKnot(of.Value, of.Multiplicity)),
            removeKnots: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.Start, Knots(at.Nurbs, of.Direction).Count), Conversions.Within(of.End - 1, Knots(at.Nurbs, of.Direction).Count))
                        .Apply(static (_, _) => unit).As())
                    .Bind(_ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveKnots(of.Start, of.End))),
            removeKnotsAt: static (at, of) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveKnotsAt(of.At.X, of.At.Y)),
            setPoint: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.U, at.Nurbs.Points.CountU), Conversions.Within(of.V, at.Nurbs.Points.CountV)).Apply(static (_, _) => unit).As())
                    .Bind(_ => Edits.Accepted<NurbsSurface, int>(at.Nurbs, at.Nurbs.Points.SetPoint(of.U, of.V, of.Point, of.Weight))),
            uniformKnots: static (at, of) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).CreateUniformKnots(of.Spacing)),
            periodicKnots: static (at, of) => Edits.Accepted<NurbsSurface, int>(at.Nurbs, Knots(at.Nurbs, of.Direction).CreatePeriodicKnots(of.Spacing)),
            removeMultipleKnots: static (at, of) => Edits.Count(at.Nurbs, Knots(at.Nurbs, of.Direction).RemoveMultipleKnots(of.Minimum, of.Maximum, at.Tolerances.Absolute)),
            matchToCurve: static (at, of) => Edits.Replaced<NurbsSurface, int>(at.Nurbs.MatchToCurve(of.Side, of.Target, of.MaxEndDistance, of.MaxInteriorDistance, at.Tolerances.Absolute, of.MaxLevel)));

    private static NurbsSurfaceKnotList Knots(NurbsSurface nurbs, SurfaceDirection direction) => direction is SurfaceDirection.U ? nurbs.KnotsU : nurbs.KnotsV;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceEdits {
    // --- [EDITS]
    public static IO<(Surface Result, Seq<(SurfaceEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Surface source, Seq<SurfaceEdit> edits) =>
        Edits.Apply(source, edits, (surface, edit) => edit.Step(tolerances, surface), static _ => IO.pure(unit));

    public static IO<(NurbsSurface Result, Seq<(NurbsSurfaceEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, NurbsSurface source, Seq<NurbsSurfaceEdit> edits) =>
        Edits.Apply(source, edits, (nurbs, edit) => edit.Step(tolerances, nurbs), static _ => IO.pure(unit));
}
