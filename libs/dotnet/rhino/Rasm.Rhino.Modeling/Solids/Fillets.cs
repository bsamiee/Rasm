using Rasm.Rhino.Document;

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

public sealed record FilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1) {
    public static FilletResult From(Brep.FilletSurfaceResults results) =>
        new(Answers.Present(results.Fillets), Answers.Present(results.OutBreps0), Answers.Present(results.OutBreps1));
}

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

public sealed record RailFilletOptions(int RailDegree, int ArcDegree, double Slider0, double Slider1, int BezierSurfaces, bool Extend, FilletSurfaceSplitType Split);

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
    private static readonly Limits<double> RailSlider = Limits.AtLeast(-1.0).AtMost(1.0);

    // --- [FILLETS]
    public static IO<Seq<Brep>> FilletEdges(Brep brep, Seq<(int Edge, RadiusMethod Method)> edges, BlendType blend, RailType rail, bool setback, double tolerance, double angleTolerance) =>
        from listed in IO.lift(() =>
            (Invalid.Unless(!edges.IsEmpty, nameof(edges)), Answers.Unique(edges.Map(static row => row.Edge), nameof(edges)).ToFin(), GeometryResults.InRange(edges.Map(static row => row.Edge), brep.Edges.Count, nameof(Brep.Edges)))
                .Apply((_, _, _) => edges.Map(static row => row.Edge))
                .As())
        from breps in edges.Traverse(static row => row.Method.Switch(constant: static constant => Some((constant.Start, constant.End)), profiled: static _ => None)).As().Match(
            Some: radii => GeometryResults.Acquire(
                () => Brep.CreateFilletEdges(brep, listed, radii.Map(static radius => radius.Start), radii.Map(static radius => radius.End), blend, rail, setback, tolerance, angleTolerance),
                nameof(Brep.CreateFilletEdges),
                emptyFails: false),
            None: () => GeometryResults.Acquire(
                () =>
                    from rows in edges.Traverse(row => from distances in Distances(brep.Edges[row.Edge].Domain, row.Method) select (row.Edge, Distances: distances)).As()
                    select Brep.CreateFilletEdgesVariableRadius(
                        brep,
                        listed,
                        rows.ToDictionary(static row => row.Edge, static row => (IList<BrepEdgeFilletDistance>)[.. row.Distances]),
                        blend,
                        rail,
                        setback,
                        tolerance,
                        angleTolerance),
                nameof(Brep.CreateFilletEdgesVariableRadius),
                emptyFails: false))
        select breps;

    public static IO<FilletResult> FaceFillet(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) =>
        IO.lift(() => Refused.Unless(Brep.CreateFilletSurface(face0, uv0, face1, uv1, Settings(shape, trim, extend, acrossTangents, tolerance), out Brep.FilletSurfaceResults results), results, nameof(Brep.CreateFilletSurface))
            .Map(FilletResult.From));

    public static IO<FilletResult> FaceCurveFillet(BrepFace face, Point2d uv, Curve curve, double t, FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) =>
        IO.lift(() => OutOfDomain.Unless(curve.Domain, t, nameof(Brep.CreateFilletSurfaceCurve))
            .Bind(_ => Refused.Unless(Brep.CreateFilletSurfaceCurve(face, uv, curve, t, Settings(shape, trim, extend, acrossTangents, tolerance), out Brep.FilletSurfaceResults results), results, nameof(Brep.CreateFilletSurfaceCurve)))
            .Map(FilletResult.From));

    public static IO<FilletResult> SectionFillet(BrepFace faceA, Point2d uvA, BrepFace faceB, Point2d uvB, double radius, int railDegree, SectionFilletProfile profile, bool trim, bool extend, double tolerance) =>
        from call in IO.lift(() => (FaceA: faceA, UvA: uvA, FaceB: faceB, UvB: uvB, Radius: radius, Tolerance: tolerance, TrimmedA: new List<Brep>(), TrimmedB: new List<Brep>(), Rail: railDegree, Trim: trim, Extend: extend, Fillets: new List<Brep>()))
        from made in IO.lift(() => profile.Switch(
                call,
                rationalArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateRationalArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateRationalArcsFilletSrf)),
                cubicArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalCubicArcsFilletSrf)),
                quarticArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuarticArcsFilletSrf)),
                quinticArcs: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateNonRationalQuinticArcsFilletSrf)),
                nonRationalCubic: static (s, cubic) => Refused.Unless(
                    SurfaceFilletBase.CreateNonRationalCubicFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, cubic.TanSlider, s.Trim, s.Extend, s.Fillets),
                    nameof(SurfaceFilletBase.CreateNonRationalCubicFilletSrf)),
                nonRationalQuartic: static (s, quartic) => Refused.Unless(
                    SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, quartic.TanSlider, quartic.InnerSlider, s.Trim, s.Extend, s.Fillets),
                    nameof(SurfaceFilletBase.CreateNonRationalQuarticFilletSrf)),
                nonRationalQuintic: static (s, quintic) => Refused.Unless(
                    SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, quintic.TanSlider, quintic.InnerSlider, s.Trim, s.Extend, s.Fillets),
                    nameof(SurfaceFilletBase.CreateNonRationalQuinticFilletSrf)),
                g2ChordalQuintic: static (s, _) => Refused.Unless(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(s.FaceA, s.UvA, s.FaceB, s.UvB, s.Radius, s.Tolerance, s.TrimmedA, s.TrimmedB, s.Rail, s.Trim, s.Extend, s.Fillets), nameof(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf))))
        select new FilletResult(toSeq(call.Fillets), toSeq(call.TrimmedA), toSeq(call.TrimmedB));

    public static IO<(FilletResult Fillet, Seq<double> Fit)> RailFillet(Curve rail, BrepFace first, BrepFace second, double u, double v, RailFilletOptions options, double tolerance) =>
        from sliders in IO.lift(() =>
            (Invalid.Unless(options.RailDegree is 3 or 5, nameof(RailFilletOptions.RailDegree)), RailSlider.Check(options.Slider0, nameof(RailFilletOptions.Slider0)), RailSlider.Check(options.Slider1, nameof(RailFilletOptions.Slider1)))
                .Apply(static (_, slider0, slider1) => (double[])[slider0, slider1])
                .As())
        from answer in IO.lift(() => {
            List<Brep> fillets = [];
            List<Brep> trimmed0 = [];
            List<Brep> trimmed1 = [];
            bool accepted = rail.FilletSurfaceToRail(first, second, u, v, options.RailDegree, options.ArcDegree, sliders, options.BezierSurfaces, options.Extend, options.Split, tolerance, fillets, trimmed0, trimmed1, out double[] fit);
            return (Accepted: accepted, Fillet: new FilletResult(toSeq(fillets), toSeq(trimmed0), toSeq(trimmed1)), Fit: toSeq(fit));
        })
        from accepted in DisposalOps.OnFailure(
            IO.lift(() => Refused.Unless(answer.Accepted, nameof(Curve.FilletSurfaceToRail))),
            DisposalOps.Release(answer.Fillet.Fillets + answer.Fillet.Trimmed0 + answer.Fillet.Trimmed1))
        select (answer.Fillet, answer.Fit);

    private static Fin<Seq<BrepEdgeFilletDistance>> Distances(Interval domain, RadiusMethod method) =>
        method.Switch(
            domain,
            constant: static (edge, constant) => Seq(new BrepEdgeFilletDistance(edge.Min, constant.Start), new BrepEdgeFilletDistance(edge.Max, constant.End)),
            profiled: static (edge, profiled) => profiled.Rows.Traverse(station =>
                    OutOfDomain.Unless(edge, station.Parameter, nameof(BrepEdge.Domain)).Map(_ => new BrepEdgeFilletDistance(station.Parameter, station.Distance)))
                .As());

    private static Brep.FilletSurfaceSettings Settings(FilletShape shape, bool trim, bool extend, bool acrossTangents, double tolerance) {
        Brep.FilletSurfaceSettings settings = shape.Switch(
            (Trim: trim, Extend: extend, Tolerance: tolerance),
            rationalArc: static (s, arc) => Brep.FilletSurfaceSettings.CreateRationalArcSettings(arc.Radius, s.Tolerance, s.Trim, s.Extend),
            nonRational: static (s, arc) => Brep.FilletSurfaceSettings.CreateNonRationalSettings(arc.Radius, s.Tolerance, arc.Degree, arc.TanSlider, arc.InnerSlider, s.Trim, s.Extend),
            g2Blend: static (s, blend) => Brep.FilletSurfaceSettings.CreateG2BlendSettings(blend.Radius, s.Tolerance, s.Trim, s.Extend),
            chamfer: static (s, chamfer) => Brep.FilletSurfaceSettings.CreateChamferSettings(chamfer.Radius0, chamfer.Radius1, s.Tolerance, s.Trim, s.Extend));
        settings.ContinueAcrossTangentFaces = acrossTangents;
        return settings;
    }

    // --- [BLENDS]
    public static IO<Seq<Brep>> BlendSurface(BlendEdge side0, BlendEdge side1) =>
        GeometryResults.Acquire(
            () => Brep.CreateBlendSurface(side0.Face, side0.Edge, side0.Domain, side0.Reverse, side0.Continuity, side1.Face, side1.Edge, side1.Domain, side1.Reverse, side1.Continuity),
            nameof(Brep.CreateBlendSurface),
            emptyFails: false);

    public static IO<Curve> BlendSection(BlendStation side0, BlendStation side1) =>
        GeometryResults.Acquire(
            () => Brep.CreateBlendShape(side0.Face, side0.Edge, side0.T, side0.Reverse, side0.Continuity, side1.Face, side1.Edge, side1.T, side1.Reverse, side1.Continuity),
            nameof(Brep.CreateBlendShape));

    // --- [OFFSETS]
    public static IO<OffsetResult> OffsetSolid(Brep brep, double distance, bool solid, bool extend, bool shrink, double tolerance) =>
        from answer in IO.lift(() => (Offsets: Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerance, out Brep[] blends, out Brep[] walls), Blends: Answers.Present(blends), Walls: Answers.Present(walls)))
        from offsets in DisposalOps.OnFailure(GeometryResults.Acquire(() => answer.Offsets, nameof(Brep.CreateOffsetBrep), emptyFails: true), DisposalOps.Release(answer.Blends + answer.Walls))
        select new OffsetResult(offsets, answer.Blends, answer.Walls);

    public static IO<Brep> FaceOffset(BrepFace face, double distance, double tolerance, bool bothSides, bool createSolid) =>
        GeometryResults.Acquire(() => Brep.CreateFromOffsetFace(face, distance, tolerance, bothSides, createSolid), nameof(Brep.CreateFromOffsetFace));

    public static IO<Seq<Brep>> Shell(Brep brep, Seq<int> facesToRemove, double distance, double tolerance) =>
        GeometryResults.Acquire(
            () => (Invalid.Unless(!facesToRemove.IsEmpty, nameof(facesToRemove)), GeometryResults.InRange(facesToRemove, brep.Faces.Count, nameof(Brep.Faces)))
                .Apply((_, _) => Brep.CreateShell(brep, facesToRemove, distance, tolerance))
                .As(),
            nameof(Brep.CreateShell),
            emptyFails: false);

    public static IO<Seq<Brep>> Pipe(Curve rail, PipeMethod method, bool localBlending, PipeCapMode cap, bool fitRail, double tolerance, double angleTolerance) =>
        method.Switch(
            (Rail: rail, LocalBlending: localBlending, Cap: cap, FitRail: fitRail, Tolerance: tolerance, Angle: angleTolerance),
            constant: static (pipe, constant) => GeometryResults.Acquire(
                () => Brep.CreatePipe(pipe.Rail, constant.Radius, pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle),
                nameof(Brep.CreatePipe),
                emptyFails: true),
            variable: static (pipe, variable) => GeometryResults.Acquire(
                () => Invalid.Unless(!variable.Rows.IsEmpty, nameof(PipeMethod.Variable.Rows)).Map(_ => Brep.CreatePipe(
                    pipe.Rail,
                    variable.Rows.Map(row => pipe.Rail.Domain.NormalizedParameterAt(row.Parameter)),
                    variable.Rows.Map(static row => row.Radius),
                    pipe.LocalBlending,
                    pipe.Cap,
                    pipe.FitRail,
                    pipe.Tolerance,
                    pipe.Angle)),
                nameof(Brep.CreatePipe),
                emptyFails: true),
            thick: static (pipe, thick) => GeometryResults.Acquire(
                () => Brep.CreateThickPipe(pipe.Rail, thick.Radius0, thick.Radius1, pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerance, pipe.Angle),
                nameof(Brep.CreateThickPipe),
                emptyFails: true),
            thickVariable: static (pipe, variable) => GeometryResults.Acquire(
                () => Invalid.Unless(!variable.Rows.IsEmpty, nameof(PipeMethod.ThickVariable.Rows)).Map(_ => Brep.CreateThickPipe(
                    pipe.Rail,
                    variable.Rows.Map(row => pipe.Rail.Domain.NormalizedParameterAt(row.Parameter)),
                    variable.Rows.Map(static row => row.Radius0),
                    variable.Rows.Map(static row => row.Radius1),
                    pipe.LocalBlending,
                    pipe.Cap,
                    pipe.FitRail,
                    pipe.Tolerance,
                    pipe.Angle)),
                nameof(Brep.CreateThickPipe),
                emptyFails: true));
}
