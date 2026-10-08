using System.Diagnostics;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record DimensionTypeState {
    public sealed record Linear() : DimensionTypeState;

    public sealed record Angular(
        DimensionStyle.AngleDisplayFormat AngleFormat, int AngleResolution, double AngleRoundoff, DimensionStyle.ZeroSuppression AngleZeroSuppression) : DimensionTypeState;

    public sealed record Radial(
        TextHorizontalAlignment LeaderTextHorizontalAlignment, DimensionStyle.ArrowType LeaderArrowType, double LeaderArrowSize, Option<Guid> LeaderArrowBlockId,
        DimensionStyle.LeaderCurveStyle LeaderCurveStyle) : DimensionTypeState;

    public sealed record Ordinate(OrdinateDimension.MeasuredDirection Direction, double KinkOffset1, double KinkOffset2) : DimensionTypeState;

    public sealed record CenterMark(double Radius) : DimensionTypeState;
}

public sealed record DimensionState(
    AnnotationType AnnotationType,
    string DisplayText,
    bool HasMeasurableTextFields,
    double NumericValue,
    Option<string> PlainUserText,
    Option<string> TextFormula,
    Point2d TextPosition,
    double TextRotation,
    bool UseDefaultTextPoint,
    Option<Guid> DetailMeasured,
    double DistanceScale,
    double DimensionScale,
    Guid DimensionStyleId,
    DimensionTypeState TypeState);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionDisplayGeometry {
    private DimensionDisplayGeometry(Seq<Line> lines, Seq<Point3d> textRectangle) => (Lines, TextRectangle) = (lines, textRectangle);

    public Seq<Line> Lines { get; }

    public Seq<Point3d> TextRectangle { get; }

    public sealed record Linear(
        Point3d ExtensionLine1End, Point3d ExtensionLine2End, Point3d Arrowhead1End, Point3d Arrowhead2End, Point3d DimensionLinePoint, Point3d TextPoint,
        Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry(Lines, TextRectangle);

    public sealed record Angular(
        Point3d CenterPoint, Point3d DefPoint1, Point3d DefPoint2, Point3d ArrowPoint1, Point3d ArrowPoint2, Point3d DimlinePoint, Point3d TextPoint, Seq<Arc> Arcs,
        Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry(Lines, TextRectangle);

    public sealed record Radial(
        Point3d CenterPoint, Point3d RadiusPoint, Point3d DimlinePoint, Point3d KneePoint,
        Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry(Lines, TextRectangle);

    public sealed record Ordinate(
        Point3d BasePoint, Point3d DefPoint, Point3d LeaderPoint, Point3d KinkPoint1, Point3d KinkPoint2,
        Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry(Lines, TextRectangle);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class DimensionMapper {
    [MapPropertyFromSource(nameof(DimensionState.TypeState), Use = nameof(TypeState))]
    internal static partial DimensionState ToState(Dimension dimension, string displayText, bool hasMeasurableTextFields);

    private static partial DimensionTypeState.Angular ToState(AngularDimension angular);

    private static partial DimensionTypeState.Radial ToState(RadialDimension radial);

    private static partial DimensionTypeState.Ordinate ToState(OrdinateDimension ordinate);

    private static partial DimensionTypeState.CenterMark ToState(Centermark centermark);

    private static DimensionTypeState TypeState(Dimension dimension) =>
        dimension switch {
            LinearDimension => new DimensionTypeState.Linear(),
            AngularDimension angular => ToState(angular),
            RadialDimension radial => ToState(radial),
            OrdinateDimension ordinate => ToState(ordinate),
            Centermark centermark => ToState(centermark),
            _ => throw new UnreachableException(),
        };
}

public static class Dimensions {
    // --- [CREATION]
    public static IO<T> Create<T>(
        RhinoDoc doc, Option<ComponentRef<DimensionStyle>> style, Option<Func<DimensionStyle, IO<Unit>>> overrides, Func<DimensionStyle, T?> create, string member)
        where T : Dimension =>
        from parent in style.Match(
            Some: address => TableOps.Find(doc.DimStyles, address, includeDeleted: false),
            None: () => IO.lift(() => doc.DimStyles.Current))
        from dimension in Copies.Acquire(() => create(parent), member)
        from overridden in DisposalOps.OnFailure(overrides.Traverse(edit => DimensionStyles.SetOverride(doc, dimension, edit)).As(), IO.lift(dimension.Dispose))
        select dimension;

    // --- [READS]
    public static IO<DimensionState> State(DimensionObject owner, Dimension dimension) =>
        IO.lift(() => DimensionMapper.ToState(dimension, owner.DisplayText, owner.HasMeasurableTextFields));

    // --- [TEXT]
    public static IO<Unit> UpdateDimensionText(RhinoDoc doc, Dimension dimension, LengthUnit units) =>
        DimensionStyles.Effective(doc, dimension, None, style => IO.lift(() => dimension.UpdateDimensionText(style, units)));

    public static IO<string> ValueText(Dimension dimension, DimensionStyle style, LengthUnit units) =>
        IO.lift(Fin<string> () => dimension switch {
            LinearDimension linear => linear.GetDistanceDisplayText(units, style),
            AngularDimension angular => angular.GetAngleDisplayText(style),
            RadialDimension radial => radial.GetDistanceDisplayText(units, style),
            OrdinateDimension ordinate => ordinate.GetDistanceDisplayText(units, style),
            Centermark => new MissingMember(typeof(Centermark), nameof(LinearDimension.GetDistanceDisplayText)),
            _ => throw new UnreachableException(),
        });

    public static IO<Transform> GetTextTransform(Dimension dimension, DimensionStyle style, RhinoViewport viewport, double textScale) =>
        use(() => new ViewportInfo(viewport)).Bind(info => IO.lift(() => dimension.GetTextTransform(info, style, textScale, drawForward: false))).Bracket();

    // --- [DISPLAY]
    public static IO<DimensionDisplayGeometry> DisplayGeometry(Dimension dimension, DimensionStyle style, double scale) =>
        IO.lift(Fin<DimensionDisplayGeometry> () => dimension switch {
            LinearDimension linear => Framed(
                    linear.Get3dPoints(out Point3d extension1, out Point3d extension2, out Point3d arrow1, out Point3d arrow2, out Point3d dimensionLine, out Point3d text),
                    linear.GetTextRectangle(out Point3d[] corners),
                    linear.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map<DimensionDisplayGeometry>(_ => new DimensionDisplayGeometry.Linear(extension1, extension2, arrow1, arrow2, dimensionLine, text, toSeq(lines), toSeq(corners))),
            AngularDimension angular => Framed(
                    angular.Get3dPoints(out Point3d center, out Point3d definition1, out Point3d definition2, out Point3d arrow1, out Point3d arrow2, out Point3d dimensionLine, out Point3d text),
                    angular.GetTextRectangle(out Point3d[] corners),
                    angular.GetDisplayLines(style, scale, out Line[] lines, out Arc[] arcs))
                .Map<DimensionDisplayGeometry>(_ => new DimensionDisplayGeometry.Angular(
                    center, definition1, definition2, arrow1, arrow2, dimensionLine, text,
                    toSeq(arcs).Filter(static arc => arc.IsValid), toSeq(lines).Filter(static line => line.IsValid), toSeq(corners))),
            RadialDimension radial => Framed(
                    radial.Get3dPoints(out Point3d center, out Point3d radiusPoint, out Point3d dimensionLine, out Point3d knee),
                    radial.GetTextRectangle(out Point3d[] corners),
                    radial.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map<DimensionDisplayGeometry>(_ => new DimensionDisplayGeometry.Radial(center, radiusPoint, dimensionLine, knee, toSeq(lines), toSeq(corners))),
            OrdinateDimension ordinate => Framed(
                    ordinate.Get3dPoints(out Point3d basePoint, out Point3d definition, out Point3d leader, out Point3d kink1, out Point3d kink2),
                    ordinate.GetTextRectangle(out Point3d[] corners),
                    ordinate.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map<DimensionDisplayGeometry>(_ => new DimensionDisplayGeometry.Ordinate(basePoint, definition, leader, kink1, kink2, toSeq(lines), toSeq(corners))),
            Centermark => new MissingMember(typeof(Centermark), nameof(LinearDimension.GetDisplayLines)),
            _ => throw new UnreachableException(),
        });

    private static Fin<Unit> Framed(bool pointed, bool framed, bool drawn) =>
        from points in Refused.Unless(pointed, nameof(LinearDimension.Get3dPoints))
        from rectangle in Refused.Unless(framed, nameof(LinearDimension.GetTextRectangle))
        from lines in Refused.Unless(drawn, nameof(LinearDimension.GetDisplayLines))
        select unit;

    // --- [PIECES]
    public static IO<A> Explode<A>(Dimension dimension, Func<Seq<GeometryBase>, IO<A>> body) =>
        Copies.AcquireNonEmpty(() => dimension.Explode(), nameof(Dimension.Explode)).Bracket(Use: body, Fin: DisposalOps.Release);
}
