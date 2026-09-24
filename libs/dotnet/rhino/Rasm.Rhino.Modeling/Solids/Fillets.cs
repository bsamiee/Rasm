using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RadiusMethod {
    public sealed record Constant(double Start, double End) : RadiusMethod;

    public sealed record Profiled(Seq<(double Parameter, double Distance)> Rows) : RadiusMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilletShape {
    public sealed record RationalArc(double Radius) : FilletShape;

    public sealed record NonRational(double Radius, int Degree, double TanSlider, double InnerSlider) : FilletShape;

    public sealed record G2Blend(double Radius) : FilletShape;

    public sealed record Chamfer(double Radius0, double Radius1) : FilletShape;
}

public sealed record FilletResult(Seq<Brep> Fillets, Seq<Brep> OutBreps0, Seq<Brep> OutBreps1);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SectionFilletProfile {
    public sealed record RationalArcs() : SectionFilletProfile;

    public sealed record CubicArcs() : SectionFilletProfile;

    public sealed record QuarticArcs() : SectionFilletProfile;

    public sealed record QuinticArcs() : SectionFilletProfile;

    public sealed record NonRationalCubic(double TanSlider) : SectionFilletProfile;

    public sealed record NonRationalQuartic(double TanSlider, double InnerSlider) : SectionFilletProfile;

    public sealed record NonRationalQuintic(double TanSlider, double InnerSlider) : SectionFilletProfile;

    public sealed record G2ChordalQuintic() : SectionFilletProfile;
}

public sealed record SectionFilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1);

public sealed record BlendEdge(BrepFace Face, BrepEdge Edge, Interval Domain, bool Reverse, BlendContinuity Continuity);

public sealed record BlendStation(BrepFace Face, BrepEdge Edge, double T, bool Reverse, BlendContinuity Continuity);

public sealed record OffsetResult(Seq<Brep> Offsets, Seq<Brep> Blends, Seq<Brep> Walls);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PipeMethod {
    public sealed record Constant(double Radius) : PipeMethod;

    public sealed record Variable(Seq<(double Parameter, double Radius)> Rows) : PipeMethod;

    public sealed record Thick(double Radius0, double Radius1) : PipeMethod;

    public sealed record ThickVariable(Seq<(double Parameter, double Radius0, double Radius1)> Rows) : PipeMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidFillets {
    // --- [LIMITS]
    private static readonly Fin<Limits<int>> RailDegree = Limits.AtLeast(2).AtMost(10, nameof(RailDegree));

    private static readonly Fin<Limits<double>> FilletSlider = Limits.AtLeast(-0.95).AtMost(0.95, nameof(FilletSlider));

    // --- [FILLETS]
    public static IO<Seq<Brep>> FilletEdges(Brep brep, Seq<(int Edge, RadiusMethod Method)> edges, BlendType blend, RailType rail, bool setback, double tolerance, double angleTolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!edges.IsEmpty, nameof(edges)))
        let indices = edges.Map(static row => row.Edge)
        from listed in IO.lift(() => Answers.InRange(indices, brep.Edges.Count, nameof(Brep.Edges)))
        let constants = edges
            .Traverse(static row => row.Method.Switch(constant: static constant => Some((constant.Start, constant.End)), profiled: static _ => None))
            .As()
        from breps in constants.Match(
            Some: radii => GeometryResults.Acquire(
                () => Brep.CreateFilletEdges(brep, indices, radii.Map(static radius => radius.Start), radii.Map(static radius => radius.End), blend, rail, setback, tolerance, angleTolerance),
                nameof(Brep.CreateFilletEdges)),
            None: () =>
                from rows in IO.lift(() => edges.TraverseM(row => from distances in Distances(brep.Edges[row.Edge].Domain, row.Method) select (row.Edge, Distances: distances)).As())
                from variable in GeometryResults.Acquire(() => Brep.CreateFilletEdgesVariableRadius(brep, indices, rows.ToDictionary(static row => row.Edge, static row => (IList<BrepEdgeFilletDistance>)[.. row.Distances]), blend, rail, setback, tolerance, angleTolerance), nameof(Brep.CreateFilletEdgesVariableRadius))
                select variable)
        select breps;

    public static IO<FilletResult> FaceFillet(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) =>
        from settings in Settings(shape, trim, extend, acrossTangents, tolerance)
        from result in IO.lift(() => Refused.Unless(Brep.CreateFilletSurface(face0, uv0, face1, uv1, settings, out Brep.FilletSurfaceResults results), nameof(Brep.CreateFilletSurface))
            .Map(_ => new FilletResult(toSeq(results.Fillets), toSeq(results.OutBreps0), toSeq(results.OutBreps1))))
        select result;

    public static IO<FilletResult> FaceCurveFillet(BrepFace face, Point2d uv, Curve curve, double t, FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) =>
        from inside in IO.lift(() => OutOfDomain.Unless(curve.Domain, t, nameof(Brep.CreateFilletSurfaceCurve)))
        from settings in Settings(shape, trim, extend, acrossTangents, tolerance)
        from result in IO.lift(() => Refused.Unless(Brep.CreateFilletSurfaceCurve(face, uv, curve, t, settings, out Brep.FilletSurfaceResults results), nameof(Brep.CreateFilletSurfaceCurve))
            .Map(_ => new FilletResult(toSeq(results.Fillets), toSeq(results.OutBreps0), toSeq(results.OutBreps1))))
        select result;

    public static IO<SectionFilletResult> SectionFillet(BrepFace faceA, Point2d uvA, BrepFace faceB, Point2d uvB, double radius, int railDegree, SectionFilletProfile profile, bool trim, bool extend, double tolerance) =>
        from valid in IO.lift(() =>
            from rail in RailDegree.Bind(limits => limits.Check(railDegree, nameof(railDegree)))
            from rounded in Limits.Above(0.0).Check(radius, nameof(radius))
            select (RailDegree: rail, Radius: rounded))
        from answer in IO.lift(() => {
            List<Brep> trimmedA = [];
            List<Brep> trimmedB = [];
            List<Brep> fillets = [];
            Fin<Unit> made = profile.Switch(
                (FaceA: faceA, UvA: uvA, FaceB: faceB, UvB: uvB, valid.Radius, Tolerance: tolerance, TrimmedA: trimmedA, TrimmedB: trimmedB, valid.RailDegree, Trim: trim, Extend: extend, Fillets: fillets),
                rationalArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateRationalArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateRationalArcsFilletSrf)),
                cubicArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf)),
                quarticArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf)),
                quinticArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf)),
                nonRationalCubic: static (s, cubic) =>
                    from tan in Slider(cubic.TanSlider, nameof(SectionFilletProfile.NonRationalCubic.TanSlider))
                    from created in Refused.Unless(SurfaceFilletBase.CreateNonRationalCubicFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, tan, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalCubicFilletSrf))
                    select created,
                nonRationalQuartic: static (s, quartic) =>
                    from tan in Slider(quartic.TanSlider, nameof(SectionFilletProfile.NonRationalQuartic.TanSlider))
                    from inner in Slider(quartic.InnerSlider, nameof(SectionFilletProfile.NonRationalQuartic.InnerSlider))
                    from created in Refused.Unless(SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, tan, inner, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuarticFilletSrf))
                    select created,
                nonRationalQuintic: static (s, quintic) =>
                    from tan in Slider(quintic.TanSlider, nameof(SectionFilletProfile.NonRationalQuintic.TanSlider))
                    from inner in Slider(quintic.InnerSlider, nameof(SectionFilletProfile.NonRationalQuintic.InnerSlider))
                    from created in Refused.Unless(SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, tan, inner, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuinticFilletSrf))
                    select created,
                g2ChordalQuintic: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.RailDegree, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf)));
            return (Made: made, Result: new SectionFilletResult(toSeq(fillets), toSeq(trimmedA), toSeq(trimmedB)));
        })
        from made in GeometryOps.OnFailure(
            IO.lift(answer.Made),
            Disposal.Release(answer.Result.Trimmed0 + answer.Result.Trimmed1 + answer.Result.Fillets))
        select answer.Result;

    private static Fin<double> Slider(double slider, string member) =>
        FilletSlider.Bind(limits => limits.Check(slider, member));

    private static Fin<Seq<BrepEdgeFilletDistance>> Distances(Interval domain, RadiusMethod method) =>
        method.Switch(
            domain,
            constant: static (edge, constant) => Seq(new BrepEdgeFilletDistance(edge.Min, constant.Start), new BrepEdgeFilletDistance(edge.Max, constant.End)),
            profiled: static (edge, profiled) => profiled.Rows.TraverseM(row =>
                from inside in OutOfDomain.Unless(edge, row.Parameter, nameof(BrepEdge.Domain))
                select new BrepEdgeFilletDistance(row.Parameter, row.Distance)).As());

    private static IO<Brep.FilletSurfaceSettings> Settings(FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) =>
        from settings in IO.lift(() => shape.Switch(
            (Trim: trim, Extend: extend, Tolerance: tolerance),
            rationalArc: static (settings, arc) =>
                from radius in Limits.Above(0.0).Check(arc.Radius, nameof(FilletShape.RationalArc.Radius))
                select Brep.FilletSurfaceSettings.CreateRationalArcSettings(radius, settings.Tolerance, settings.Trim, settings.Extend),
            nonRational: static (settings, rounded) =>
                from radius in Limits.Above(0.0).Check(rounded.Radius, nameof(FilletShape.NonRational.Radius))
                from degree in CurveConstruction.ArcDegree.Bind(limits => limits.Check(rounded.Degree, nameof(FilletShape.NonRational.Degree)))
                from tan in Slider(rounded.TanSlider, nameof(FilletShape.NonRational.TanSlider))
                from inner in Slider(rounded.InnerSlider, nameof(FilletShape.NonRational.InnerSlider))
                select Brep.FilletSurfaceSettings.CreateNonRationalSettings(radius, settings.Tolerance, degree, tan, inner, settings.Trim, settings.Extend),
            g2Blend: static (settings, blend) =>
                from radius in Limits.Above(0.0).Check(blend.Radius, nameof(FilletShape.G2Blend.Radius))
                select Brep.FilletSurfaceSettings.CreateG2BlendSettings(radius, settings.Tolerance, settings.Trim, settings.Extend),
            chamfer: static (settings, chamfer) =>
                from first in Limits.Above(0.0).Check(chamfer.Radius0, nameof(FilletShape.Chamfer.Radius0))
                from second in Limits.Above(0.0).Check(chamfer.Radius1, nameof(FilletShape.Chamfer.Radius1))
                select Brep.FilletSurfaceSettings.CreateChamferSettings(first, second, settings.Tolerance, settings.Trim, settings.Extend)))
        from across in IO.lift(() => settings.ContinueAcrossTangentFaces = acrossTangents)
        select settings;

    // --- [BLENDS]
    public static IO<Seq<Brep>> BlendSurface(BlendEdge side0, BlendEdge side1) =>
        GeometryResults.Acquire(() => Brep.CreateBlendSurface(side0.Face, side0.Edge, side0.Domain, side0.Reverse, side0.Continuity, side1.Face, side1.Edge, side1.Domain, side1.Reverse, side1.Continuity), nameof(Brep.CreateBlendSurface));

    public static IO<Curve> BlendSection(BlendStation side0, BlendStation side1) =>
        IO.lift(() => Missing.Unless(Brep.CreateBlendShape(side0.Face, side0.Edge, side0.T, side0.Reverse, side0.Continuity, side1.Face, side1.Edge, side1.T, side1.Reverse, side1.Continuity), nameof(Brep.CreateBlendShape)));

    // --- [OFFSETS]
    public static IO<OffsetResult> OffsetSolid(Brep brep, double distance, bool solid, bool extend, bool shrink, double tolerance) =>
        from answer in IO.lift(() => (Offsets: Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerance, out Brep[] blends, out Brep[] walls), Blends: blends, Walls: walls))
        from offsets in GeometryResults.Acquire(() => answer.Offsets, nameof(Brep.CreateOffsetBrep))
        select new OffsetResult(offsets, toSeq(answer.Blends), toSeq(answer.Walls));

    public static IO<Brep> FaceOffset(BrepFace face, double distance, double tolerance, bool bothSides, bool createSolid) =>
        IO.lift(() => Missing.Unless(Brep.CreateFromOffsetFace(face, distance, tolerance, bothSides, createSolid), nameof(Brep.CreateFromOffsetFace)));

    public static IO<Seq<Brep>> Shell(Brep brep, Seq<int> facesToRemove, double distance, double tolerance) =>
        from filled in IO.lift(() => Invalid.Unless(!facesToRemove.IsEmpty, nameof(facesToRemove)))
        from listed in IO.lift(() => Answers.InRange(facesToRemove, brep.Faces.Count, nameof(Brep.Faces)))
        from shells in GeometryResults.Acquire(() => Brep.CreateShell(brep, facesToRemove, distance, tolerance), nameof(Brep.CreateShell))
        select shells;

    public static IO<Seq<Brep>> Pipe(Curve rail, PipeMethod method, bool localBlending, PipeCapMode cap, bool fitRail, double tolerance, double angleTolerance) =>
        method.Switch(
            (Rail: rail, LocalBlending: localBlending, Cap: cap, FitRail: fitRail, Tolerance: tolerance, Angle: angleTolerance),
            constant: static (pipe, constant) =>
                from positive in IO.lift(() => Limits.Above(0.0).Check(constant.Radius, nameof(PipeMethod.Constant.Radius)))
                from breps in GeometryResults.Acquire(() => Brep.CreatePipe(pipe.Rail, positive, pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle), nameof(Brep.CreatePipe))
                select breps,
            variable: static (pipe, variable) =>
                from rows in IO.lift(() =>
                    from filled in Invalid.Unless(!variable.Rows.IsEmpty, nameof(PipeMethod.Variable.Rows))
                    from listed in variable.Rows.TraverseM(row =>
                        from inside in OutOfDomain.Unless(pipe.Rail.Domain, row.Parameter, nameof(Brep.CreatePipe))
                        from positive in Limits.Above(0.0).Check(row.Radius, nameof(PipeMethod.Variable.Rows))
                        select (row.Parameter, Radius: positive)).As()
                    select listed)
                from breps in GeometryResults.Acquire(() => Brep.CreatePipe(pipe.Rail, rows.Map(static row => row.Parameter), rows.Map(static row => row.Radius), pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle), nameof(Brep.CreatePipe))
                select breps,
            thick: static (pipe, thick) =>
                from radii in IO.lift(() =>
                    from inner in Limits.Above(0.0).Check(thick.Radius0, nameof(PipeMethod.Thick.Radius0))
                    from outer in Limits.Above(0.0).Check(thick.Radius1, nameof(PipeMethod.Thick.Radius1))
                    select (Radius0: inner, Radius1: outer))
                from breps in GeometryResults.Acquire(() => Brep.CreateThickPipe(pipe.Rail, radii.Radius0, radii.Radius1, pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle), nameof(Brep.CreateThickPipe))
                select breps,
            thickVariable: static (pipe, variable) =>
                from rows in IO.lift(() =>
                    from filled in Invalid.Unless(!variable.Rows.IsEmpty, nameof(PipeMethod.ThickVariable.Rows))
                    from listed in variable.Rows.TraverseM(row =>
                        from inside in OutOfDomain.Unless(pipe.Rail.Domain, row.Parameter, nameof(Brep.CreateThickPipe))
                        from inner in Limits.Above(0.0).Check(row.Radius0, nameof(PipeMethod.ThickVariable.Rows))
                        from outer in Limits.Above(0.0).Check(row.Radius1, nameof(PipeMethod.ThickVariable.Rows))
                        select (row.Parameter, Radius0: inner, Radius1: outer)).As()
                    select listed)
                from breps in GeometryResults.Acquire(() => Brep.CreateThickPipe(pipe.Rail, rows.Map(static row => row.Parameter), rows.Map(static row => row.Radius0), rows.Map(static row => row.Radius1), pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle), nameof(Brep.CreateThickPipe))
                select breps);
}
