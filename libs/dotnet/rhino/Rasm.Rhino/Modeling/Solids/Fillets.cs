using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ArcDegree {
    Cubic,
    Quartic,
    Quintic,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NonRationalArc {
    public sealed record Cubic(double Tangent) : NonRationalArc;
    public sealed record Quartic(double Tangent, double Inner) : NonRationalArc;
    public sealed record Quintic(double Tangent, double Inner) : NonRationalArc;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ArcSection {
    public sealed record Rational : ArcSection;
    public sealed record NonRational(NonRationalArc Arc) : ArcSection;

    internal (int Degree, Seq<double> Sliders) Arguments => Switch(
        rational: static _ => (2, Seq<double>()),
        nonRational: static section => section.Arc.Switch(
            cubic: static arc => (3, Seq(arc.Tangent, 0d)),
            quartic: static arc => (4, Seq(arc.Tangent, arc.Inner)),
            quintic: static arc => (5, Seq(arc.Tangent, arc.Inner))));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletSection {
    public sealed record Arc(ArcSection Section) : FilletSection;
    public sealed record Lines : FilletSection;
    public sealed record Arcs(ArcDegree Degree) : FilletSection;
    public sealed record G2ChordalQuintic : FilletSection;

    public IO<Seq<Brep>> Emit(FilletEngine engine, int railDegree, bool extend, Action<IDisposable> retain) =>
        Switch(
            (Engine: engine, RailDegree: railDegree, Extend: extend, Retain: retain),
            arc: static (s, arc) => arc.Section.Switch(
                s,
                rational: static (s, _) => Emitted(nameof(SurfaceFilletBase.RationalArcs), fillets => s.Engine.RationalArcs(s.RailDegree, s.Extend, fillets), s.Retain),
                nonRational: static (s, section) => section.Arc.Switch(
                    s,
                    cubic: static (s, arc) => Emitted(nameof(SurfaceFilletBase.NonRationalCubic),
                        fillets => s.Engine.NonRationalCubic(s.RailDegree, arc.Tangent, s.Extend, fillets), s.Retain),
                    quartic: static (s, arc) => Emitted(nameof(SurfaceFilletBase.NonRationalQuartic),
                        fillets => s.Engine.NonRationalQuartic(s.RailDegree, arc.Tangent, arc.Inner, s.Extend, fillets), s.Retain),
                    quintic: static (s, arc) => Emitted(nameof(SurfaceFilletBase.NonRationalQuintic),
                        fillets => s.Engine.NonRationalQuintic(s.RailDegree, arc.Tangent, arc.Inner, s.Extend, fillets), s.Retain))),
            lines: static (s, _) => Emitted(nameof(SurfaceFilletBase.Lines), fillets => s.Engine.Lines(s.RailDegree, s.Extend, fillets), s.Retain),
            arcs: static (s, arcs) => arcs.Degree switch {
                ArcDegree.Cubic => Emitted(nameof(SurfaceFilletBase.NonRationalCubicArcs), fillets => s.Engine.NonRationalCubicArcs(s.RailDegree, s.Extend, fillets), s.Retain),
                ArcDegree.Quartic => Emitted(nameof(SurfaceFilletBase.NonRationalQuarticArcs), fillets => s.Engine.NonRationalQuarticArcs(s.RailDegree, s.Extend, fillets), s.Retain),
                ArcDegree.Quintic => Emitted(nameof(SurfaceFilletBase.NonRationalQuinticArcs), fillets => s.Engine.NonRationalQuinticArcs(s.RailDegree, s.Extend, fillets), s.Retain),
            },
            g2ChordalQuintic: static (s, _) => Emitted(nameof(SurfaceFilletBase.G2ChordalQuintic), fillets => s.Engine.G2ChordalQuintic(s.RailDegree, s.Extend, fillets), s.Retain));

    private static IO<Seq<Brep>> Emitted(string member, Func<List<Brep>, bool> emit, Action<IDisposable> retain) =>
        Copies.Acquire(member, _ => new List<Brep>() switch {
            var fillets => (Answer.Accepted(emit(fillets)), Answer.Many(fillets)).Apply(static (_, made) => made).As(),
        }, retain);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletContact {
    public sealed record Faces(BrepFace A, Point2d UvA, BrepFace B, Point2d UvB, double Radius, bool AllowExtend) : FilletContact;
    public sealed record Chamfer(BrepFace A, Point2d UvA, BrepFace B, Point2d UvB, double Radius0, double Radius1, bool AllowExtend) : FilletContact;
    public sealed record Rail(BrepFace RailFace, global::Rhino.Geometry.Curve Rail3d, BrepFace Face, Point2d Uv) : FilletContact;
    public sealed record Curve(BrepFace Face, Point2d Uv, global::Rhino.Geometry.Curve Rail3d, double T, double Radius) : FilletContact;

    public IO<FilletEngine> Engine(Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(SurfaceFilletBase.IsInitialized), _ => Initialized(Switch(
            tolerances.Absolute,
            faces: static (tolerance, faces) => new FilletEngine(faces.A, faces.UvA, faces.B, faces.UvB, faces.Radius, faces.AllowExtend, tolerance),
            chamfer: static (tolerance, chamfer) => new FilletEngine(chamfer.A, chamfer.UvA, chamfer.B, chamfer.UvB, chamfer.Radius0, chamfer.Radius1, chamfer.AllowExtend, tolerance),
            rail: static (tolerance, rail) => new FilletEngine(rail.RailFace, rail.Rail3d, rail.Face, rail.Uv, tolerance),
            curve: static (tolerance, curve) => new FilletEngine(curve.Face, curve.Uv, curve.Rail3d, curve.T, curve.Radius, tolerance))), retain);

    private static Answer<FilletEngine> Initialized(FilletEngine engine) =>
        (Answer.Accepted(engine.IsInitialized()), Answer.One(engine)).Apply(static (_, held) => held).As();
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveAlignment {
    public sealed record Ignore(int BezierSurfaces) : CurveAlignment;
    public sealed record Match : CurveAlignment;
    public sealed record MatchToTolerance : CurveAlignment;

    internal (int AlignToCurve, int BezierSurfaces) Arguments => Switch(
        ignore: static ignore => (0, ignore.BezierSurfaces),
        match: static _ => (1, 0),
        matchToTolerance: static _ => (2, 0));
}

public sealed record SurfaceToCurveFit(double CurveDeviation, double FaceDeviation, double FaceAngle, double BezierAngle, double BezierCurvature);

public sealed record SurfaceToRailFit(double Deviation0, double Deviation1, double FaceAngle0, double FaceAngle1, double BezierAngle, double BezierCurvature);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EdgeRadius {
    public sealed record Ends(double Start, double End) : EdgeRadius;
    public sealed record Stations(Seq<BrepEdgeFilletDistance> Distances) : EdgeRadius;
}

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

    public IO<Unit> ChangeRadius(double radius) =>
        IO.lift(() => Refused.Unless(ChangeFilletRadius(radius), nameof(ChangeFilletRadius)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fillets {
    // --- [EDGE_FILLETS]
    public static IO<Seq<Brep>> CreateFilletEdges(Tolerances tolerances, Brep brep, Map<int, EdgeRadius> edges, BlendType blendType, RailType railType, bool setbackFillets, Action<IDisposable> retain) =>
        IO.lift(() => Copies.Within(brep, ComponentIndexType.BrepEdge, toSeq(edges.Keys)))
        >> toSeq(edges.Values).Traverse(static radius => radius.Switch(ends: static ends => Some(ends), stations: static _ => Option<EdgeRadius.Ends>.None)).As().Match(
            Some: ends => Copies.Acquire(nameof(Brep.CreateFilletEdges), _ => Answer.NonEmpty(
                Brep.CreateFilletEdges(brep, edges.Keys, from radius in ends select radius.Start, from radius in ends select radius.End,
                    blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle)), retain),
            None: () => Copies.Acquire(nameof(Brep.CreateFilletEdgesVariableRadius), _ => Answer.NonEmpty(
                Brep.CreateFilletEdgesVariableRadius(brep, edges.Keys, edges.ToDictionary(static row => row.Key, row => (IList<BrepEdgeFilletDistance>)[.. row.Value.Switch(
                        (Brep: brep, Index: row.Key),
                        ends: static (edge, ends) => edge.Brep.Edges[edge.Index].Domain switch {
                            var domain => Seq(new BrepEdgeFilletDistance(domain.T0, ends.Start), new BrepEdgeFilletDistance(domain.T1, ends.End)),
                        },
                        stations: static (_, stations) => stations.Distances)]),
                    blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle)), retain));

    // --- [SURFACE_FILLETS]
    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, Brep.FilletSurfaceSettings settings, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateFilletSurface), _ => Filleted(
            Answer.Accepted(Brep.CreateFilletSurface(face0, uv0, face1, uv1, settings, out Brep.FilletSurfaceResults? results)), results?.Fillets, results?.OutBreps0, results?.OutBreps1), retain);

    public static IO<(Seq<Brep> Fillets, SurfaceToCurveFit Fit)> FilletSurfaceToCurve(Tolerances tolerances, Curve curve, BrepFace face, double t, Point2d uv, double radius, CurveAlignment alignment, int railDegree, ArcSection section, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.FilletSurfaceToCurve), _ => (Fillets: new List<Brep>(), Alignment: alignment.Arguments, Arc: section.Arguments) switch {
            var made => (Answer.Accepted(curve.FilletSurfaceToCurve(face, t, uv.X, uv.Y, radius, made.Alignment.AlignToCurve, railDegree, made.Arc.Degree, made.Arc.Sliders,
                        made.Alignment.BezierSurfaces, tolerances.Absolute, made.Fillets, out double[] fit)), Answer.Many(made.Fillets))
                .Apply((_, fillets) => (Fillets: fillets, Fit: new SurfaceToCurveFit(fit[0], fit[1], fit[2], fit[3], fit[4]))).As(),
        }, retain);

    public static IO<(FilletResult Products, SurfaceToRailFit Fit)> FilletSurfaceToRail(Tolerances tolerances, Curve rail, BrepFace faceWithCurve, BrepFace secondFace, Point2d uv, int railDegree, ArcSection section, int bezierSurfaces, bool extend, FilletSurfaceSplitType splitType, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.FilletSurfaceToRail), _ => (Fillets: new List<Brep>(), Trimmed0: new List<Brep>(), Trimmed1: new List<Brep>(), Arc: section.Arguments) switch {
            var made => Filleted(
                    Answer.Accepted(rail.FilletSurfaceToRail(faceWithCurve, secondFace, uv.X, uv.Y, railDegree, made.Arc.Degree, made.Arc.Sliders, bezierSurfaces, extend, splitType, tolerances.Absolute,
                        made.Fillets, made.Trimmed0, made.Trimmed1, out double[] fit)),
                    made.Fillets, made.Trimmed0, made.Trimmed1)
                .Map(products => (Products: products, Fit: new SurfaceToRailFit(fit[0], fit[1], fit[2], fit[3], fit[4], fit[5]))),
        }, retain);

    private static Answer<FilletResult> Filleted(Answer<Unit> verdict, IEnumerable<Brep>? fillets, IEnumerable<Brep>? trimmed0, IEnumerable<Brep>? trimmed1) =>
        (verdict, Answer.Many(fillets ?? []), Answer.Many(trimmed0 ?? []), Answer.Many(trimmed1 ?? []))
            .Apply(static (_, made, first, second) => new FilletResult(made, first, second)).As();
}
