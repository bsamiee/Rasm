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

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NonRationalArc {
    public (int Degree, double TanSlider, double InnerSlider) Arguments =>
        Switch(cubic: static arc => (3, arc.Tangent, ArcSlider.Neutral), quartic: static arc => (4, arc.Tangent, arc.Inner), quintic: static arc => (5, arc.Tangent, arc.Inner));

    public sealed record Cubic(ArcSlider Tangent) : NonRationalArc;
    public sealed record Quartic(ArcSlider Tangent, ArcSlider Inner) : NonRationalArc;
    public sealed record Quintic(ArcSlider Tangent, ArcSlider Inner) : NonRationalArc;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletSection {
    public sealed record Rational : FilletSection;
    public sealed record NonRational(NonRationalArc Arc) : FilletSection;
    public sealed record G2Blend : FilletSection;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record FilletProfile {
    public sealed record Round(double Radius, FilletSection Section) : FilletProfile;
    public sealed record Chamfer(double Radius0, double Radius1) : FilletProfile;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EdgeRadius {
    public sealed record Ends(double Start, double End) : EdgeRadius;
    public sealed record Stations(Seq<BrepEdgeFilletDistance> Distances) : EdgeRadius;
}

public sealed record FilletResult(Seq<Brep> Fillets, Seq<Brep> Trimmed0, Seq<Brep> Trimmed1);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fillets {
    // --- [EDGE_FILLETS]
    public static IO<Seq<Brep>> CreateFilletEdges(Brep brep, Map<int, EdgeRadius> edges, BlendType blendType, RailType railType, bool setbackFillets, Tolerances tolerances) =>
        toSeq(edges.Values).Traverse(static radius => radius.Switch(ends: static ends => Some(ends), stations: static _ => Option<EdgeRadius.Ends>.None)).As().Match(
            Some: ends => Copies.AcquireNonEmpty(
                () => Brep.CreateFilletEdges(brep, edges.Keys, from radius in ends select radius.Start, from radius in ends select radius.End,
                    blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle),
                nameof(Brep.CreateFilletEdges)),
            None:
                from indices in IO.lift(() => Copies.InRange(toSeq(edges.Filter(static radius => radius is EdgeRadius.Ends).Keys), brep.Edges.Count, nameof(Brep.Edges)))
                let distances = edges.ToDictionary(static row => row.Key, row => (IList<BrepEdgeFilletDistance>)[.. row.Value.Switch(
                    (Brep: brep, Index: row.Key),
                    ends: static (edge, ends) => edge.Brep.Edges[edge.Index].Domain switch {
                        var domain => Seq(new BrepEdgeFilletDistance(domain.T0, ends.Start), new BrepEdgeFilletDistance(domain.T1, ends.End)),
                    },
                    stations: static (_, stations) => stations.Distances)])
                from products in Copies.AcquireNonEmpty(
                    () => Brep.CreateFilletEdgesVariableRadius(brep, edges.Keys, distances, blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle),
                    nameof(Brep.CreateFilletEdgesVariableRadius))
                select products);

    // --- [SURFACE_FILLETS]
    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile profile, bool trim, bool extend, bool continueAcrossTangentFaces, Tolerances tolerances) =>
        Filleted(IO.lift(() => {
            Brep.FilletSurfaceSettings settings = Settings(profile, trim, extend, tolerances);
            settings.ContinueAcrossTangentFaces = continueAcrossTangentFaces;
            return (nameof(Brep.CreateFilletSurface), Brep.CreateFilletSurface(face0, uv0, face1, uv1, settings, out Brep.FilletSurfaceResults? results),
                toSeq<Brep?>(results?.Fillets), toSeq<Brep?>(results?.OutBreps0), toSeq<Brep?>(results?.OutBreps1));
        }));

    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile.Round round, int railDegree, bool trim, bool extend, Tolerances tolerances) =>
        Filleted(
            from products in IO.lift(static () => (Fillets: new List<Brep>(), Trimmed0: new List<Brep>(), Trimmed1: new List<Brep>()))
            let answer = round.Section.Switch(
                (Face0: face0, Uv0: uv0, Face1: face1, Uv1: uv1, round.Radius, tolerances.Absolute, Products: products, RailDegree: railDegree, Trim: trim, Extend: extend),
                rational: static (s, _) => (Member: nameof(SurfaceFilletBase.CreateRationalArcsFilletSrf), Accepted:
                    SurfaceFilletBase.CreateRationalArcsFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, s.Trim, s.Extend, s.Products.Fillets)),
                nonRational: static (s, section) => section.Arc.Switch(
                    s,
                    cubic: static (s, arc) => (Member: nameof(SurfaceFilletBase.CreateNonRationalCubicFilletSrf), Accepted:
                        SurfaceFilletBase.CreateNonRationalCubicFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, arc.Tangent, s.Trim, s.Extend, s.Products.Fillets)),
                    quartic: static (s, arc) => (Member: nameof(SurfaceFilletBase.CreateNonRationalQuarticFilletSrf), Accepted:
                        SurfaceFilletBase.CreateNonRationalQuarticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, arc.Tangent, arc.Inner, s.Trim, s.Extend, s.Products.Fillets)),
                    quintic: static (s, arc) => (Member: nameof(SurfaceFilletBase.CreateNonRationalQuinticFilletSrf), Accepted:
                        SurfaceFilletBase.CreateNonRationalQuinticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, arc.Tangent, arc.Inner, s.Trim, s.Extend, s.Products.Fillets))),
                g2Blend: static (s, _) => (Member: nameof(SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf), Accepted:
                    SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, s.Trim, s.Extend, s.Products.Fillets)))
            select (answer.Member, answer.Accepted, toSeq<Brep?>(products.Fillets), toSeq<Brep?>(products.Trimmed0), toSeq<Brep?>(products.Trimmed1)));

    public static IO<FilletResult> CreateFilletSurfaceCurve(BrepFace face, Point2d uv, Curve curve, double t, FilletProfile.Round round, bool trim, bool extend, Tolerances tolerances) =>
        Filleted(IO.lift(() => (
            nameof(Brep.CreateFilletSurfaceCurve), Brep.CreateFilletSurfaceCurve(face, uv, curve, t, Settings(round, trim, extend, tolerances), out Brep.FilletSurfaceResults? results),
            toSeq<Brep?>(results?.Fillets), toSeq<Brep?>(results?.OutBreps0), toSeq<Brep?>(results?.OutBreps1))));

    public static IO<(Seq<Brep> Fillets, Seq<double> FitResults)> FilletSurfaceToCurve(Curve curve, BrepFace face, double t, Point2d uv, double radius, int alignToCurve, int railDegree, Option<NonRationalArc> arc, int bezierSurfaces, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => {
                List<Brep> fillets = [];
                (int degree, double tanSlider, double innerSlider) = arc.Map(static value => value.Arguments).IfNone((2, ArcSlider.Neutral, ArcSlider.Neutral));
                return (Accepted: curve.FilletSurfaceToCurve(face, t, uv.X, uv.Y, radius, alignToCurve, railDegree, degree, [tanSlider, innerSlider], bezierSurfaces, tolerances.Absolute, fillets, out double[] fitResults),
                    Fillets: toSeq<Brep?>(fillets.AsEnumerable()), FitResults: toSeq(fitResults));
            }),
            static Fin<(Seq<Brep> Fillets, Seq<double> FitResults)> (made) => made.Accepted
                ? Measurements.Valid(made.Fillets, nameof(Curve.FilletSurfaceToCurve)).Map(fillets => (Fillets: fillets, made.FitResults))
                : new Refused(nameof(Curve.FilletSurfaceToCurve)),
            static made => DisposalOps.Release(Conversions.Rows(made.Fillets)));

    public static IO<FilletResult> FilletSurfaceToRail(Curve rail, BrepFace faceWithCurve, BrepFace secondFace, Point2d uv, int railDegree, NonRationalArc arc, int bezierSurfaces, bool extend, FilletSurfaceSplitType splitType, Tolerances tolerances) =>
        Filleted(
            from products in IO.lift(static () => (Fillets: new List<Brep>(), Trimmed0: new List<Brep>(), Trimmed1: new List<Brep>()))
            let args = arc.Arguments
            let accepted = rail.FilletSurfaceToRail(faceWithCurve, secondFace, uv.X, uv.Y, railDegree, args.Degree, [args.TanSlider, args.InnerSlider], bezierSurfaces, extend, splitType, tolerances.Absolute, products.Fillets, products.Trimmed0, products.Trimmed1, out _)
            select (nameof(Curve.FilletSurfaceToRail), accepted, toSeq<Brep?>(products.Fillets), toSeq<Brep?>(products.Trimmed0), toSeq<Brep?>(products.Trimmed1)));

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

    private static IO<FilletResult> Filleted(IO<(string Member, bool Accepted, Seq<Brep?> Fillets, Seq<Brep?> Trimmed0, Seq<Brep?> Trimmed1)> fillet) =>
        Copies.Owned(fillet,
            static Fin<FilletResult> (made) => made.Accepted
                ? (Measurements.Valid(made.Fillets, made.Member).ToValidation(), Measurements.Valid(made.Trimmed0, made.Member).ToValidation(), Measurements.Valid(made.Trimmed1, made.Member).ToValidation())
                    .Apply(static (fillets, trimmed0, trimmed1) => new FilletResult(fillets, trimmed0, trimmed1)).As().ToFin()
                : new Refused(made.Member),
            static made => DisposalOps.Release(Conversions.Rows([.. made.Fillets, .. made.Trimmed0, .. made.Trimmed1])));
}
