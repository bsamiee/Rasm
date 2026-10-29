using System.Collections.Generic;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ArcDegree {
    public static readonly ArcDegree Cubic = new(
        3, static (engine, rail, tangent, _, extend, fillets) => engine.NonRationalCubic(rail, tangent, extend, fillets), static (engine, rail, extend, fillets) => engine.NonRationalCubicArcs(rail, extend, fillets));
    public static readonly ArcDegree Quartic = new(
        4, static (engine, rail, tangent, inner, extend, fillets) => engine.NonRationalQuartic(rail, tangent, inner, extend, fillets), static (engine, rail, extend, fillets) => engine.NonRationalQuarticArcs(rail, extend, fillets));
    public static readonly ArcDegree Quintic = new(
        5, static (engine, rail, tangent, inner, extend, fillets) => engine.NonRationalQuintic(rail, tangent, inner, extend, fillets), static (engine, rail, extend, fillets) => engine.NonRationalQuinticArcs(rail, extend, fillets));

    public int Degree { get; }

    [UseDelegateFromConstructor]
    internal partial bool Emit(SurfaceFilletBase engine, int railDegree, double tangent, double inner, bool extend, List<Brep> fillets);

    [UseDelegateFromConstructor]
    internal partial bool Arcs(SurfaceFilletBase engine, int railDegree, bool extend, List<Brep> fillets);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ArcSection {
    private ArcSection(int degree, Seq<double> sliders) => (Degree, Sliders) = (degree, sliders);

    public int Degree { get; }

    public Seq<double> Sliders { get; }

    public sealed record Rational() : ArcSection(2, []);

    public sealed record NonRational(ArcDegree Arc, double Tangent, double Inner) : ArcSection(Arc.Degree, [Tangent, Inner]);

    public sealed record NonRationalArcs(ArcDegree Arc) : ArcSection(Arc.Degree, []);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletSection {
    public sealed record Arc(ArcSection Section) : FilletSection;

    public sealed record Lines() : FilletSection;

    public sealed record G2() : FilletSection;

    internal bool Emit(SurfaceFilletBase engine, int railDegree, bool extend, List<Brep> fillets) =>
        Switch(
            (Engine: engine, RailDegree: railDegree, Extend: extend, Fillets: fillets),
            arc: static (at, arc) => arc.Section.Switch(
                at,
                rational: static (at, _) => at.Engine.RationalArcs(at.RailDegree, at.Extend, at.Fillets),
                nonRational: static (at, section) => section.Arc.Emit(at.Engine, at.RailDegree, section.Tangent, section.Inner, at.Extend, at.Fillets),
                nonRationalArcs: static (at, section) => section.Arc.Arcs(at.Engine, at.RailDegree, at.Extend, at.Fillets)),
            lines: static (at, _) => at.Engine.Lines(at.RailDegree, at.Extend, at.Fillets),
            g2: static (at, _) => at.Engine.G2ChordalQuintic(at.RailDegree, at.Extend, at.Fillets));

    internal Fin<Brep.FilletSurfaceSettings> Settings(double radius, double tolerance, bool trim, bool extend) =>
        Switch(
            (Radius: radius, Tolerance: tolerance, Trim: trim, Extend: extend),
            arc: static (at, arc) => arc.Section.Switch(
                at,
                rational: static (at, _) => Fin.Succ(Brep.FilletSurfaceSettings.CreateRationalArcSettings(at.Radius, at.Tolerance, at.Trim, at.Extend)),
                nonRational: static (at, section) =>
                    Brep.FilletSurfaceSettings.CreateNonRationalSettings(at.Radius, at.Tolerance, section.Arc.Degree, section.Tangent, section.Inner, at.Trim, at.Extend),
                nonRationalArcs: static (_, _) => new RhinoFault(nameof(ArcSection.NonRationalArcs), RhinoReason.Unsupported)),
            lines: static (_, _) => new RhinoFault(nameof(Lines), RhinoReason.Unsupported),
            g2: static (at, _) => Brep.FilletSurfaceSettings.CreateG2BlendSettings(at.Radius, at.Tolerance, at.Trim, at.Extend));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletContact {
    public sealed record Faces(BrepFace A, Point2d UvA, BrepFace B, Point2d UvB, double Radius, bool AllowExtend) : FilletContact;
    public sealed record Chamfer(BrepFace A, Point2d UvA, BrepFace B, Point2d UvB, double Radius0, double Radius1, bool AllowExtend) : FilletContact;
    public sealed record Rail(BrepFace RailFace, Curve Rail3d, BrepFace Face, Point2d Uv) : FilletContact;
    public sealed record FaceCurve(BrepFace Face, Point2d Uv, Curve Rail3d, double T, double Radius) : FilletContact;

    public IO<FilletEngine> Engine(Tolerances tolerances) =>
        from engine in Products.Of(() => Switch(
            tolerances.Absolute,
            faces: static (tolerance, faces) => new FilletEngine(faces.A, faces.UvA, faces.B, faces.UvB, faces.Radius, faces.AllowExtend, tolerance),
            chamfer: static (tolerance, chamfer) => new FilletEngine(chamfer.A, chamfer.UvA, chamfer.B, chamfer.UvB, chamfer.Radius0, chamfer.Radius1, chamfer.AllowExtend, tolerance),
            rail: static (tolerance, rail) => new FilletEngine(rail.RailFace, rail.Rail3d, rail.Face, rail.Uv, tolerance),
            faceCurve: static (tolerance, faceCurve) => new FilletEngine(faceCurve.Face, faceCurve.Uv, faceCurve.Rail3d, faceCurve.T, faceCurve.Radius, tolerance)))
        from initialized in Products.Accepted(engine.IsInitialized)
        select engine;

    internal Fin<(bool Accepted, Brep.FilletSurfaceResults? Results)> Surface(FilletSection section, Tolerances tolerances, bool trim, bool extend, bool tangentChain) =>
        Switch(
            (Section: section, Tolerance: tolerances.Absolute, Trim: trim, Extend: extend, Chain: tangentChain),
            faces: static (at, faces) => Chained(at.Section.Settings(faces.Radius, at.Tolerance, at.Trim, at.Extend), at.Chain).Map(settings =>
                (Accepted: Brep.CreateFilletSurface(faces.A, faces.UvA, faces.B, faces.UvB, settings, out Brep.FilletSurfaceResults results), Results: (Brep.FilletSurfaceResults?)results)),
            chamfer: static (at, chamfer) => Chained(at.Section is FilletSection.Lines
                    ? Brep.FilletSurfaceSettings.CreateChamferSettings(chamfer.Radius0, chamfer.Radius1, at.Tolerance, at.Trim, at.Extend)
                    : new RhinoFault(nameof(Chamfer), RhinoReason.Unsupported), at.Chain).Map(settings =>
                (Accepted: Brep.CreateFilletSurface(chamfer.A, chamfer.UvA, chamfer.B, chamfer.UvB, settings, out Brep.FilletSurfaceResults results), Results: (Brep.FilletSurfaceResults?)results)),
            rail: static (_, _) => new RhinoFault(nameof(Rail), RhinoReason.Unsupported),
            faceCurve: static (at, faceCurve) => Chained(at.Section.Settings(faceCurve.Radius, at.Tolerance, at.Trim, at.Extend), at.Chain).Map(settings =>
                (Accepted: Brep.CreateFilletSurfaceCurve(faceCurve.Face, faceCurve.Uv, faceCurve.Rail3d, faceCurve.T, settings, out Brep.FilletSurfaceResults results), Results: (Brep.FilletSurfaceResults?)results)));

    private static Fin<Brep.FilletSurfaceSettings> Chained(Fin<Brep.FilletSurfaceSettings> settings, bool tangentChain) =>
        settings.Map(held => {
            held.ContinueAcrossTangentFaces = tangentChain;
            return held;
        });
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveAlignment {
    private CurveAlignment(int alignToCurve, int bezierSurfaces) => (AlignToCurve, BezierSurfaces) = (alignToCurve, bezierSurfaces);

    public int AlignToCurve { get; }

    public int BezierSurfaces { get; }

    public sealed record Ignore(int Surfaces) : CurveAlignment(0, Surfaces);

    public sealed record Match() : CurveAlignment(1, 0);

    public sealed record MatchToTolerance() : CurveAlignment(2, 0);
}

public sealed record SurfaceToCurveFit(double CurveDeviation, double FaceDeviation, double FaceAngle, double BezierAngle, double BezierCurvature);

public sealed record SurfaceToRailFit(double Deviation0, double Deviation1, double FaceAngle0, double FaceAngle1, double BezierAngle, double BezierCurvature);

public sealed record FilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FilletEngine : SurfaceFilletBase {
    internal FilletEngine(BrepFace faceA, Point2d uvA, BrepFace faceB, Point2d uvB, double radius, bool allowExtend, double tolerance)
        : base(faceA, uvA, faceB, uvB, radius, allowExtend, tolerance) { }

    internal FilletEngine(BrepFace faceA, Point2d uvA, BrepFace faceB, Point2d uvB, double radius0, double radius1, bool allowExtend, double tolerance)
        : base(faceA, uvA, faceB, uvB, radius0, radius1, allowExtend, tolerance) { }

    internal FilletEngine(BrepFace railFace, Curve rail, BrepFace face, Point2d uv, double tolerance)
        : base(railFace, rail, face, uv, tolerance) { }

    internal FilletEngine(BrepFace face, Point2d uv, Curve rail, double t, double radius, double tolerance)
        : base(face, uv, rail, t, radius, tolerance) { }

    public IO<Unit> ChangeRadius(double radius) => Products.Accepted(() => ChangeFilletRadius(radius));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fillets {
    // --- [EDGES]
    public static IO<Seq<Brep>> Edges(Tolerances tolerances, Brep brep, Map<int, Law> radii, Spacing sampling, BlendType blendType, RailType railType, bool setbackFillets) =>
        from held in IO.lift(() => brep.Within(ComponentIndexType.BrepEdge, toSeq(radii.Keys)))
        from filleted in toSeq(radii.Values).ForAll(static radius => radius is Law.Constant or Law.Linear)
            ? Products.Many(() => Brep.CreateFilletEdges(brep, held, radii.Values.Select(static radius => radius.At(0d)), radii.Values.Select(static radius => radius.At(1d)),
                blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle))
            : from distances in IO.lift(() => toSeq(radii.Keys).Traverse(edge => radii[edge] switch {
                var radius => (radius is Law.Through through ? new Spacing.Lengths(through.Keys.Map(static key => key.At)) : sampling).Along(tolerances, brep.Edges[edge])
                    .Map(stations => (Edge: edge, Distances: stations.Map(station => new BrepEdgeFilletDistance(station.Parameter, radius.At(station.Normalized))).Strict())),
            }).As())
              from variable in Products.Many(() => Brep.CreateFilletEdgesVariableRadius(brep, held,
                  distances.AsEnumerable().ToDictionary(static row => row.Edge, static IList<BrepEdgeFilletDistance> (row) => [.. row.Distances]),
                  blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle))
              select variable
        select filleted;

    // --- [SURFACES]
    public static IO<FilletResult> Surface(Tolerances tolerances, FilletContact contact, FilletSection section, bool trim, bool extend, bool tangentChain) =>
        from answered in IO.lift(() => contact.Surface(section, tolerances, trim, extend, tangentChain))
        from result in Decoded(answered.Accepted, answered.Results?.Fillets, answered.Results?.OutBreps0, answered.Results?.OutBreps1, nameof(Brep.CreateFilletSurface))
        select result;

    public static IO<FilletResult> Engine(Tolerances tolerances, FilletContact contact, FilletSection section, int railDegree, bool extend, bool trim) =>
        from engine in contact.Engine(tolerances)
        from answered in IO.lift(() => (Fillets: new List<Brep>(), First: new List<Brep>(), Second: new List<Brep>()) switch {
            var lists => (Accepted: section.Emit(engine, railDegree, extend, lists.Fillets) && (!trim || engine.TrimBreps(extend, lists.First, lists.Second)), Lists: lists),
        })
        from result in Decoded(answered.Accepted, answered.Lists.Fillets, answered.Lists.First, answered.Lists.Second, nameof(SurfaceFilletBase))
        select result;

    // --- [RAILS]
    public static IO<(Seq<Brep> Fillets, SurfaceToCurveFit Fit)> ToCurve(Tolerances tolerances, Curve curve, BrepFace face, double t, Point2d uv, double radius, CurveAlignment alignment, int railDegree, ArcSection section) =>
        from fillets in IO.lift(static () => new List<Brep>())
        from answered in IO.lift(() => (
            Accepted: curve.FilletSurfaceToCurve(face, t, uv.X, uv.Y, radius, alignment.AlignToCurve, railDegree, section.Degree, section.Sliders, alignment.BezierSurfaces, tolerances.Absolute, fillets, out double[] fit),
            Fit: fit))
        from result in Decoded(answered.Accepted, fillets, first: null, second: null, nameof(Curve.FilletSurfaceToCurve))
        from fit in IO.lift(() => answered.Fit is [var curveDeviation, var faceDeviation, var faceAngle, var bezierAngle, var bezierCurvature, ..]
            ? Fin.Succ(new SurfaceToCurveFit(curveDeviation, faceDeviation, faceAngle, bezierAngle, bezierCurvature))
            : new RhinoFault(nameof(Curve.FilletSurfaceToCurve), RhinoReason.Missing))
        select (result.Fillets, fit);

    public static IO<(FilletResult Products, SurfaceToRailFit Fit)> ToRail(
        Tolerances tolerances, Curve rail, BrepFace faceWithCurve, BrepFace secondFace, Point2d uv, int railDegree, ArcSection.NonRational section, int bezierSurfaces, bool extend, FilletSurfaceSplitType splitType) =>
        from lists in IO.lift(static () => (Fillets: new List<Brep>(), First: new List<Brep>(), Second: new List<Brep>()))
        from answered in IO.lift(() => (
            Accepted: rail.FilletSurfaceToRail(faceWithCurve, secondFace, uv.X, uv.Y, railDegree, section.Degree, section.Sliders, bezierSurfaces, extend, splitType, tolerances.Absolute,
                lists.Fillets, lists.First, lists.Second, out double[] fit),
            Fit: fit))
        from result in Decoded(answered.Accepted, lists.Fillets, lists.First, lists.Second, nameof(Curve.FilletSurfaceToRail))
        from fit in IO.lift(() => answered.Fit is [var deviation0, var deviation1, var faceAngle0, var faceAngle1, var bezierAngle, var bezierCurvature, ..]
            ? Fin.Succ(new SurfaceToRailFit(deviation0, deviation1, faceAngle0, faceAngle1, bezierAngle, bezierCurvature))
            : new RhinoFault(nameof(Curve.FilletSurfaceToRail), RhinoReason.Missing))
        select (result, fit);

    // --- [DECODING]
    private static IO<FilletResult> Decoded(bool accepted, IEnumerable<Brep>? fillets, IEnumerable<Brep>? first, IEnumerable<Brep>? second, string member) =>
        from trimmed0 in Products.Many(() => first ?? [], required: false, member)
        from trimmed1 in Products.Many(() => second ?? [], required: false, member)
        from made in Products.Many(() => fillets ?? [], required: false, member)
        from verdict in Products.Accepted(() => accepted, member)
        select new FilletResult(made, trimmed0, trimmed1);
}
