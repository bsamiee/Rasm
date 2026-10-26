using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NonRationalArc {
    public (int Degree, double TanSlider, double InnerSlider) Arguments =>
        Switch(cubic: static arc => (3, arc.Tangent, 0d), quartic: static arc => (4, arc.Tangent, arc.Inner), quintic: static arc => (5, arc.Tangent, arc.Inner));

    public sealed record Cubic(double Tangent) : NonRationalArc;
    public sealed record Quartic(double Tangent, double Inner) : NonRationalArc;
    public sealed record Quintic(double Tangent, double Inner) : NonRationalArc;
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
    public static IO<Seq<Brep>> CreateFilletEdges(Brep brep, Map<int, EdgeRadius> edges, BlendType blendType, RailType railType, bool setbackFillets, Tolerances tolerances, Action<IDisposable> retain) =>
        IO.lift(() => Copies.Within(brep, ComponentIndexType.BrepEdge, toSeq(edges.Keys)))
        >> toSeq(edges.Values).Traverse(static radius => radius.Switch(ends: static ends => Some(ends), stations: static _ => Option<EdgeRadius.Ends>.None)).As().Match(
            Some: ends => Copies.AcquireNonEmpty(
                () => Brep.CreateFilletEdges(brep, edges.Keys, from radius in ends select radius.Start, from radius in ends select radius.End,
                    blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle),
                nameof(Brep.CreateFilletEdges), retain),
            None: () => Copies.AcquireNonEmpty(
                () => Brep.CreateFilletEdgesVariableRadius(brep, edges.Keys, edges.ToDictionary(static row => row.Key, row => (IList<BrepEdgeFilletDistance>)[.. row.Value.Switch(
                        (Brep: brep, Index: row.Key),
                        ends: static (edge, ends) => edge.Brep.Edges[edge.Index].Domain switch {
                            var domain => Seq(new BrepEdgeFilletDistance(domain.T0, ends.Start), new BrepEdgeFilletDistance(domain.T1, ends.End)),
                        },
                        stations: static (_, stations) => stations.Distances)]),
                    blendType, railType, setbackFillets, tolerances.Absolute, tolerances.Angle),
                nameof(Brep.CreateFilletEdgesVariableRadius), retain));

    // --- [SURFACE_FILLETS]
    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile profile, bool trim, bool extend, bool continueAcrossTangentFaces, Tolerances tolerances, Action<IDisposable> retain) =>
        Filleted(() => {
            Brep.FilletSurfaceSettings settings = Settings(profile, trim, extend, tolerances);
            settings.ContinueAcrossTangentFaces = continueAcrossTangentFaces;
            return (nameof(Brep.CreateFilletSurface), Brep.CreateFilletSurface(face0, uv0, face1, uv1, settings, out Brep.FilletSurfaceResults? results),
                results);
        }, retain);

    public static IO<FilletResult> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, FilletProfile.Round round, int railDegree, bool trim, bool extend, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Owned(
            static () => (Fillets: new List<Brep>(), Trimmed0: new List<Brep>(), Trimmed1: new List<Brep>()),
            products => IO.lift(() => {
                (string member, bool accepted) = round.Section.Switch(
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
                        SurfaceFilletBase.CreateG2ChordalQuinticFilletSrf(s.Face0, s.Uv0, s.Face1, s.Uv1, s.Radius, s.Absolute, s.Products.Trimmed0, s.Products.Trimmed1, s.RailDegree, s.Trim, s.Extend, s.Products.Fillets)));
                return Result((member, accepted, [.. products.Fillets], [.. products.Trimmed0], [.. products.Trimmed1]));
            }),
            static products => DisposalOps.Release(Conversions.Rows(products.Fillets) + Conversions.Rows(products.Trimmed0) + Conversions.Rows(products.Trimmed1)), retain);

    public static IO<FilletResult> CreateFilletSurfaceCurve(BrepFace face, Point2d uv, Curve curve, double t, FilletProfile.Round round, bool trim, bool extend, Tolerances tolerances, Action<IDisposable> retain) =>
        Filleted(() => (
            nameof(Brep.CreateFilletSurfaceCurve), Brep.CreateFilletSurfaceCurve(face, uv, curve, t, Settings(round, trim, extend, tolerances), out Brep.FilletSurfaceResults? results),
            results), retain);

    public static IO<(Seq<Brep> Fillets, Seq<double> FitResults)> FilletSurfaceToCurve(Curve curve, BrepFace face, double t, Point2d uv, double radius, int alignToCurve, int railDegree, Option<NonRationalArc> arc, int bezierSurfaces, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Owned(
            static () => new List<Brep>(),
            fillets => IO.lift(Fin<(Seq<Brep> Fillets, Seq<double> FitResults)> () => {
                (int degree, double tanSlider, double innerSlider) = arc.Map(static value => value.Arguments).IfNone((2, 0d, 0d));
                return curve.FilletSurfaceToCurve(face, t, uv.X, uv.Y, radius, alignToCurve, railDegree, degree, [tanSlider, innerSlider], bezierSurfaces, tolerances.Absolute, fillets, out double[] fitResults)
                    ? Measurements.Valid([.. fillets], nameof(Curve.FilletSurfaceToCurve)).Map(products => (Fillets: products, FitResults: toSeq(fitResults)))
                    : new Refused(nameof(Curve.FilletSurfaceToCurve), Refused.Declined);
            }),
            static fillets => DisposalOps.Release(Conversions.Rows(fillets)), retain);

    public static IO<(FilletResult Products, Seq<double> FitResults)> FilletSurfaceToRail(Curve rail, BrepFace faceWithCurve, BrepFace secondFace, Point2d uv, int railDegree, Option<NonRationalArc> arc, int bezierSurfaces, bool extend, FilletSurfaceSplitType splitType, Tolerances tolerances, Action<IDisposable> retain) =>
        Copies.Owned(
            static () => (Fillets: new List<Brep>(), Trimmed0: new List<Brep>(), Trimmed1: new List<Brep>()),
            products => IO.lift(() => {
                (int degree, double tanSlider, double innerSlider) = arc.Map(static value => value.Arguments).IfNone((2, 0d, 0d));
                bool accepted = rail.FilletSurfaceToRail(faceWithCurve, secondFace, uv.X, uv.Y, railDegree, degree, [tanSlider, innerSlider], bezierSurfaces, extend, splitType, tolerances.Absolute, products.Fillets, products.Trimmed0, products.Trimmed1, out double[] fitResults);
                return Result((nameof(Curve.FilletSurfaceToRail), accepted, [.. products.Fillets], [.. products.Trimmed0], [.. products.Trimmed1]))
                    .Map(fillets => (Products: fillets, FitResults: toSeq(fitResults)));
            }),
            static products => DisposalOps.Release(Conversions.Rows(products.Fillets) + Conversions.Rows(products.Trimmed0) + Conversions.Rows(products.Trimmed1)), retain);

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

    private static IO<FilletResult> Filleted(Func<(string Member, bool Accepted, Brep.FilletSurfaceResults? Results)> fillet, Action<IDisposable> retain) =>
        Copies.Owned(fillet,
            static made => Result((made.Member, made.Accepted, [.. toSeq(made.Results?.Fillets)], [.. toSeq(made.Results?.OutBreps0)], [.. toSeq(made.Results?.OutBreps1)])),
            static made => DisposalOps.Release(Conversions.Rows(made.Results?.Fillets) + Conversions.Rows(made.Results?.OutBreps0) + Conversions.Rows(made.Results?.OutBreps1)), retain);

    private static Fin<FilletResult> Result((string Member, bool Accepted, Seq<Brep?> Fillets, Seq<Brep?> Trimmed0, Seq<Brep?> Trimmed1) made) =>
        made.Accepted
            ? (Measurements.Valid(made.Fillets, made.Member), Measurements.Valid(made.Trimmed0, made.Member), Measurements.Valid(made.Trimmed1, made.Member))
                .Apply(static (fillets, trimmed0, trimmed1) => new FilletResult(fillets, trimmed0, trimmed1)).As()
            : new Refused(made.Member, Refused.Declined);
}
