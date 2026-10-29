using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate Curve? CatenaryCall(out Point3d apex, out double parameter, out double length, out double deviation);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CatenaryCurve(Curve Curve, Point3d Apex, double Parameter, double Length, double MaxDeviation);

public sealed record NurbsCurveFit(NurbsCurve Curve, Option<Line> MaximumSeparation, Option<(double Source, double Fit)> SeparationParameters);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Ribbon {
    public sealed record Ruled(Curve Offset, Seq<Curve> CrossSections, Seq<Surface> RuledSurfaces) : Ribbon;

    public sealed record Swept(Curve Offset, Seq<Curve> Rails, Seq<Curve> CrossSections, Seq<Brep> BrepSurfaces) : Ribbon;

    public Seq<GeometryBase> Parts => Switch<Seq<GeometryBase>>(
        ruled: static ribbon => [ribbon.Offset, .. ribbon.CrossSections, .. ribbon.RuledSurfaces],
        swept: static ribbon => [ribbon.Offset, .. ribbon.Rails, .. ribbon.CrossSections, .. ribbon.BrepSurfaces]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveConstruction {
    // --- [CATENARIES]
    public static IO<CatenaryCurve> CreateCatenaryCurve(CatenaryCall call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        from answered in IO.lift(() => (Curve: call(out Point3d apex, out double parameter, out double length, out double deviation), Apex: apex, Parameter: parameter, Length: length, Deviation: deviation))
        from curve in Products.Of(() => answered.Curve, member)
        select new CatenaryCurve(curve, answered.Apex, answered.Parameter, answered.Length, answered.Deviation);

    // --- [FITS]
    public static IO<NurbsCurveFit> CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions) =>
        from answered in IO.lift(() => (
            Curve: Curve.CreateNurbsCurveFit(curve, domain, rebuildOptions, out Line separation, out double source, out double fit), Separation: separation, Source: source, Fit: fit))
        from fitted in Products.Of(() => answered.Curve, nameof(Curve.CreateNurbsCurveFit))
        select new NurbsCurveFit(fitted, Conversions.Present(answered.Separation),
            from sourceAt in Conversions.Present(answered.Source)
            from fitAt in Conversions.Present(answered.Fit)
            select (Source: sourceAt, Fit: fitAt));

    // --- [RIBBONS]
    public static IO<Ribbon> RibbonOffset(Curve curve, RibbonOffsetParameters parameters) =>
        parameters.RibbonSurfaceGenerationMethod is RibbonOffsetSurfaceMethod.None
            ? from answered in IO.lift(() => (
                  Offset: curve.RibbonOffset(parameters.OffsetDistance, parameters.BlendRadius, parameters.OffsetLocation, parameters.OffsetPlaneVector3d, parameters.OffsetTolerance,
                      out Curve[] sections, out Surface[] surfaces), Sections: sections, Surfaces: surfaces))
              from offset in Products.Of(() => answered.Offset, nameof(Curve.RibbonOffset))
              from sections in Products.Many(() => answered.Sections, required: false, nameof(Curve.RibbonOffset))
              from surfaces in Products.Many(() => answered.Surfaces, required: false, nameof(Curve.RibbonOffset))
              select (Ribbon)new Ribbon.Ruled(offset, sections, surfaces)
            : from answered in IO.lift(() => (Offset: curve.RibbonOffset(parameters, out Curve[] rails, out Curve[] sections, out Brep[] breps), Rails: rails, Sections: sections, Breps: breps))
              from offset in Products.Of(() => answered.Offset, nameof(Curve.RibbonOffset))
              from rails in Products.Many(() => answered.Rails, required: false, nameof(Curve.RibbonOffset))
              from sections in Products.Many(() => answered.Sections, required: false, nameof(Curve.RibbonOffset))
              from breps in Products.Many(() => answered.Breps, required: false, nameof(Curve.RibbonOffset))
              select (Ribbon)new Ribbon.Swept(offset, rails, sections, breps);

    // --- [CHAMFERS]
    public static Step<Curve> Chamfer(Tolerances tolerances, double before, double after) =>
        Steps.Derived<Curve>(curve =>
            from seamed in IO.lift(() => CurveEvaluation.Kinks(tolerances, curve, Continuity.G1_continuous) switch {
                var kinks when curve.IsClosed && kinks.Exists(t => t >= curve.Domain.T1) => Seam(curve, kinks.Filter(t => t < curve.Domain.T1).Last.IfNone(curve.Domain.T0))
                    .Bind(t => RhinoFault.Unless(curve.ChangeClosedCurveSeam(t), RhinoReason.Declined, t)),
                _ => unit,
            })
            from cuts in IO.lift(() => Cuts(curve, CurveEvaluation.Kinks(tolerances, curve, Continuity.G1_continuous).Filter(t => t > curve.Domain.T0 && t < curve.Domain.T1), before, after))
            from chamfered in cuts.IsEmpty ? IO.pure(curve) : Cut(tolerances, curve, cuts)
            select chamfered);

    private static Fin<double> Seam(Curve curve, double last) =>
        ((curve.GetLength(new Interval(curve.Domain.T0, last)) + curve.GetLength()) / 2d) switch {
            var middle => RhinoFault.Unless(curve.LengthParameter(middle, out double t), RhinoReason.Outside, middle).Map(_ => t),
        };

    private static Fin<Seq<(double Start, double End)>> Cuts(Curve curve, Seq<double> corners, double before, double after) =>
        (Total: curve.GetLength(), Lengths: corners.Map(corner => curve.GetLength(new Interval(curve.Domain.T0, corner)) switch {
            var at => (Start: at - before, End: at + after),
        }).Strict()) switch {
            var measured => Products.Each(measured.Lengths, (span, index) =>
                span.Start > (index > 0 ? measured.Lengths[index - 1].End : 0d) && span.End < (index < measured.Lengths.Count - 1 ? measured.Lengths[index + 1].Start : measured.Total)
                    ? (RhinoFault.Unless(curve.LengthParameter(span.Start, out double start), RhinoReason.Outside, span.Start, index).Map(_ => start),
                        RhinoFault.Unless(curve.LengthParameter(span.End, out double end), RhinoReason.Outside, span.End, index).Map(_ => end))
                        .Apply(static (start, end) => (Start: start, End: end)).As()
                    : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"chamfer ({span.Start}, {span.End})"), RhinoReason.Outside, index)),
        };

    private static IO<Curve> Cut(Tolerances tolerances, Curve curve, Seq<(double Start, double End)> cuts) =>
        from pieces in Products.Many(() => curve.Split(cuts.Bind(static cut => Seq(cut.Start, cut.End))))
        from lines in Products.Many(() => cuts.Map(cut => new LineCurve(curve.PointAt(cut.Start), curve.PointAt(cut.End))))
        let kept = pieces.Filter(piece => !cuts.Exists(cut => cut.Start < piece.Domain.Mid && piece.Domain.Mid < cut.End))
        from joined in Products.Many(() => Curve.JoinCurves(kept + lines.Map(static line => (Curve)line), tolerances.Absolute, preserveDirection: true))
        from single in IO.lift(() => RhinoFault.Unless(joined.Count is 1, RhinoReason.Declined, joined.Count).Map(_ => joined[0]))
        select single;
}
