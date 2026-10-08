using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ArcSlider : System.Numerics.IMinMaxValue<ArcSlider> {
    public static ArcSlider MinValue { get; } = new(-0.9);
    public static ArcSlider MaxValue { get; } = new(0.9);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record NonRationalArc {
    public (int Degree, double TanSlider, double InnerSlider) Arguments =>
        Switch<(int Degree, double TanSlider, double InnerSlider)>(
            cubic: static arc => (3, arc.Tangent, ArcSlider.Neutral),
            quartic: static arc => (4, arc.Tangent, arc.Inner),
            quintic: static arc => (5, arc.Tangent, arc.Inner));

    public sealed record Cubic(ArcSlider Tangent) : NonRationalArc;
    public sealed record Quartic(ArcSlider Tangent, ArcSlider Inner) : NonRationalArc;
    public sealed record Quintic(ArcSlider Tangent, ArcSlider Inner) : NonRationalArc;
}

[Union]
public abstract partial record FilletSection {
    public sealed record Rational : FilletSection;
    public sealed record NonRational(NonRationalArc Arc) : FilletSection;
    public sealed record G2Blend : FilletSection;
}

[Union]
public abstract partial record FilletProfile {
    public sealed record Round(double Radius, FilletSection Section) : FilletProfile;
    public sealed record Chamfer(double Radius0, double Radius1) : FilletProfile;
}

[Union]
public abstract partial record EdgeRadius {
    public sealed record Ends(double Start, double End) : EdgeRadius;
    public sealed record Stations(Seq<BrepEdgeFilletDistance> Distances) : EdgeRadius;
}

public sealed record FilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fillets {
    // --- [EDGE_FILLETS]
    public static IO<Seq<Brep>> CreateFilletEdges(Brep brep, Map<int, EdgeRadius> edges, BlendType blendType, RailType railType, bool setbackFillets, Tolerances tolerances) =>
        toSeq(edges.Values).Traverse(static radius => radius.Switch<Option<EdgeRadius.Ends>>(ends: static ends => ends, stations: static _ => None)).As().Match(
            Some: ends => Copies.AcquireNonEmpty(
                () => Brep.CreateFilletEdges(brep, edges.Keys, ends.Map(static row => row.Start), ends.Map(static row => row.End), blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle),
                nameof(Brep.CreateFilletEdges)),
            None: Copies.AcquireNonEmpty(
                () => Copies.InRange(toSeq(edges.Keys), brep.Edges.Count, nameof(Brep.Edges)).Map(_ => Brep.CreateFilletEdgesVariableRadius(
                    brep,
                    edges.Keys,
                    edges.ToDictionary(static row => row.Key, row => (IList<BrepEdgeFilletDistance>)[.. row.Value.Switch(
                        brep.Edges[row.Key].Domain,
                        ends: static (domain, ends) => Seq(new BrepEdgeFilletDistance(domain.T0, ends.Start), new BrepEdgeFilletDistance(domain.T1, ends.End)),
                        stations: static (_, stations) => stations.Distances)]),
                    blendType,
                    railType,
                    setbackFillets,
                    tolerances.Absolute,
                    tolerances.Angle)),
                nameof(Brep.CreateFilletEdgesVariableRadius)));

    // --- [SURFACE_FILLETS]
    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile profile, bool trim, bool extend, bool continueAcrossTangentFaces, Tolerances tolerances) =>
        Filleted(() => {
            Brep.FilletSurfaceSettings settings = Settings(profile, trim, extend, tolerances);
            settings.ContinueAcrossTangentFaces = continueAcrossTangentFaces;
            return (
                nameof(Brep.CreateFilletSurface),
                Brep.CreateFilletSurface(face0, uv0, face1, uv1, settings, out Brep.FilletSurfaceResults? results),
                toSeq<Brep?>(results?.Fillets),
                toSeq<Brep?>(results?.OutBreps0),
                toSeq<Brep?>(results?.OutBreps1));
        });

    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile.Round round, int railDegree, bool trim, bool extend, Tolerances tolerances) =>
        Filleted(() => {
            List<Brep> fillets = [];
            List<Brep> trimmed0 = [];
            List<Brep> trimmed1 = [];
            (string member, bool accepted) = round.Section.Switch(
                (Face0: face0, Uv0: uv0, Face1: face1, Uv1: uv1, round.Radius, tolerances.Absolute, Trimmed0: trimmed0, Trimmed1: trimmed1, RailDegree: railDegree, Trim: trim, Extend: extend, Fillets: fillets),
                rational: static (s, _) => (
                    nameof(SurfaceFilletBase.CreateRationalArcsFilletSrf),
                    SurfaceFilletBase.CreateRationalArcsFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Trimmed0, s.Trimmed1, s.RailDegree, s.Trim, s.Extend, s.Fillets)),
                nonRational: static (s, section) => section.Arc.Switch(
                    s,
                    cubic: static (s, arc) => (
                        nameof(SurfaceFilletBase.CreateNonRationalCubicFilletSrf),
                        SurfaceFilletBase.CreateNonRationalCubicFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Trimmed0, s.Trimmed1, s.RailDegree, arc.Tangent, s.Trim, s.Extend, s.Fillets)),
                    quartic: static (s, arc) => (
                        nameof(SurfaceFilletBase.CreateNonRationalQuarticFilletSrf),
                        SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Trimmed0, s.Trimmed1, s.RailDegree, arc.Tangent, arc.Inner, s.Trim, s.Extend, s.Fillets)),
                    quintic: static (s, arc) => (
                        nameof(SurfaceFilletBase.CreateNonRationalQuinticFilletSrf),
                        SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Trimmed0, s.Trimmed1, s.RailDegree, arc.Tangent, arc.Inner, s.Trim, s.Extend, s.Fillets))),
                g2Blend: static (s, _) => (
                    nameof(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf),
                    SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Trimmed0, s.Trimmed1, s.RailDegree, s.Trim, s.Extend, s.Fillets)));
            return (member, accepted, toSeq<Brep?>(fillets), toSeq<Brep?>(trimmed0), toSeq<Brep?>(trimmed1));
        });

    public static IO<FilletResult> CreateFilletSurfaceCurve(BrepFace face, Point2d uv, Curve curve, double t, FilletProfile.Round round, bool trim, bool extend, Tolerances tolerances) =>
        Filleted(() => (
            nameof(Brep.CreateFilletSurfaceCurve),
            Brep.CreateFilletSurfaceCurve(face, uv, curve, t, Settings(round, trim, extend, tolerances), out Brep.FilletSurfaceResults? results),
            toSeq<Brep?>(results?.Fillets),
            toSeq<Brep?>(results?.OutBreps0),
            toSeq<Brep?>(results?.OutBreps1)));

    public static IO<FilletResult> FilletSurfaceToRail(Curve rail, BrepFace faceWithCurve, BrepFace secondFace, Point2d uv, int railDegree, NonRationalArc arc, int bezierSurfaces, bool extend, FilletSurfaceSplitType splitType, Tolerances tolerances) =>
        Filleted(() => {
            List<Brep> fillets = [];
            List<Brep> breps0 = [];
            List<Brep> breps1 = [];
            (int degree, double tanSlider, double innerSlider) = arc.Arguments;
            bool accepted = rail.FilletSurfaceToRail(faceWithCurve, secondFace, uv.X, uv.Y, railDegree, degree, [tanSlider, innerSlider], bezierSurfaces, extend, splitType, tolerances.Absolute, fillets, breps0, breps1, out _);
            return (nameof(Curve.FilletSurfaceToRail), accepted, toSeq<Brep?>(fillets), toSeq<Brep?>(breps0), toSeq<Brep?>(breps1));
        });

    private static Brep.FilletSurfaceSettings Settings(FilletProfile profile, bool trim, bool extend, Tolerances tolerances) =>
        profile.Switch(
            (Trim: trim, Extend: extend, tolerances.Absolute),
            round: static (s, round) => round.Section.Switch(
                (s.Trim, s.Extend, s.Absolute, round.Radius),
                rational: static (s, _) => Brep.FilletSurfaceSettings.CreateRationalArcSettings(s.Radius, s.Absolute, s.Trim, s.Extend),
                nonRational: static (s, section) => section.Arc.Arguments switch {
                    var (degree, tanSlider, innerSlider) => Brep.FilletSurfaceSettings.CreateNonRationalSettings(s.Radius, s.Absolute, degree, tanSlider, innerSlider, s.Trim, s.Extend),
                },
                g2Blend: static (s, _) => Brep.FilletSurfaceSettings.CreateG2BlendSettings(s.Radius, s.Absolute, s.Trim, s.Extend)),
            chamfer: static (s, chamfer) => Brep.FilletSurfaceSettings.CreateChamferSettings(chamfer.Radius0, chamfer.Radius1, s.Absolute, s.Trim, s.Extend));

    private static IO<FilletResult> Filleted(Func<(string Member, bool Accepted, Seq<Brep?> Fillets, Seq<Brep?> Trimmed0, Seq<Brep?> Trimmed1)> fillet) =>
        Copies.Owned(
            IO.lift(fillet),
            static made => Refused.Unless(made.Accepted, made.Member).Bind(_ =>
                (Measurements.Valid(made.Fillets, made.Member).ToValidation(), Measurements.Valid(made.Trimmed0, made.Member).ToValidation(), Measurements.Valid(made.Trimmed1, made.Member).ToValidation())
                    .Apply(static (fillets, trimmed0, trimmed1) => new FilletResult(fillets, trimmed0, trimmed1))
                    .As()
                    .ToFin()),
            static made => DisposalOps.Release(Conversions.Rows([.. made.Fillets, .. made.Trimmed0, .. made.Trimmed1])));
}
