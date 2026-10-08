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
[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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
    AnnotationType AnnotationType, string DisplayText, bool HasMeasurableTextFields, double NumericValue,
    Option<string> PlainUserText, Option<string> TextFormula, Point2d TextPosition, double TextRotation, bool UseDefaultTextPoint,
    Option<Guid> DetailMeasured, double DistanceScale, double DimensionScale, Guid DimensionStyleId, DimensionTypeState TypeState);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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
public static partial class Dimensions {
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
        IO.lift(() => ToState(dimension, owner.DisplayText, owner.HasMeasurableTextFields));

    [MapPropertyFromSource(nameof(DimensionState.TypeState), Use = nameof(TypeState))]
    private static partial DimensionState ToState(Dimension dimension, string displayText, bool hasMeasurableTextFields);

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

    // --- [TEXT]
    public static IO<Unit> UpdateDimensionText(RhinoDoc doc, Dimension dimension, LengthUnit units) =>
        (from style in use(DimensionStyles.Effective(doc, dimension, None))
         from updated in IO.lift(() => dimension.UpdateDimensionText(style, units))
         select updated).Bracket();

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
        (from info in use(() => new ViewportInfo(viewport))
         select dimension.GetTextTransform(info, style, textScale, drawForward: false)).Bracket();

    // --- [DISPLAY]
    public static IO<DimensionDisplayGeometry> DisplayGeometry(Dimension dimension, DimensionStyle style, double scale) =>
        IO.lift(Fin<DimensionDisplayGeometry> () => dimension switch {
            LinearDimension linear =>
                !linear.Get3dPoints(out Point3d extension1, out Point3d extension2, out Point3d arrow1, out Point3d arrow2, out Point3d dimensionLine, out Point3d text)
                    ? new Refused(nameof(LinearDimension.Get3dPoints))
                : !linear.GetTextRectangle(out Point3d[] corners) ? new Refused(nameof(LinearDimension.GetTextRectangle))
                : !linear.GetDisplayLines(style, scale, out IEnumerable<Line> lines) ? new Refused(nameof(LinearDimension.GetDisplayLines))
                : new DimensionDisplayGeometry.Linear(extension1, extension2, arrow1, arrow2, dimensionLine, text, toSeq(lines), toSeq(corners)),
            AngularDimension angular =>
                !angular.Get3dPoints(out Point3d center, out Point3d definition1, out Point3d definition2, out Point3d arrow1, out Point3d arrow2, out Point3d dimensionLine, out Point3d text)
                    ? new Refused(nameof(AngularDimension.Get3dPoints))
                : !angular.GetTextRectangle(out Point3d[] corners) ? new Refused(nameof(AngularDimension.GetTextRectangle))
                : !angular.GetDisplayLines(style, scale, out Line[] lines, out Arc[] arcs) ? new Refused(nameof(AngularDimension.GetDisplayLines))
                : new DimensionDisplayGeometry.Angular(
                    center, definition1, definition2, arrow1, arrow2, dimensionLine, text,
                    toSeq(arcs).Filter(static arc => arc.IsValid).Strict(), toSeq(lines).Filter(static line => line.IsValid).Strict(), toSeq(corners)),
            RadialDimension radial =>
                !radial.Get3dPoints(out Point3d center, out Point3d radiusPoint, out Point3d dimensionLine, out Point3d knee) ? new Refused(nameof(RadialDimension.Get3dPoints))
                : !radial.GetTextRectangle(out Point3d[] corners) ? new Refused(nameof(RadialDimension.GetTextRectangle))
                : !radial.GetDisplayLines(style, scale, out IEnumerable<Line> lines) ? new Refused(nameof(RadialDimension.GetDisplayLines))
                : new DimensionDisplayGeometry.Radial(center, radiusPoint, dimensionLine, knee, toSeq(lines), toSeq(corners)),
            OrdinateDimension ordinate =>
                !ordinate.Get3dPoints(out Point3d basePoint, out Point3d definition, out Point3d leader, out Point3d kink1, out Point3d kink2) ? new Refused(nameof(OrdinateDimension.Get3dPoints))
                : !ordinate.GetTextRectangle(out Point3d[] corners) ? new Refused(nameof(OrdinateDimension.GetTextRectangle))
                : !ordinate.GetDisplayLines(style, scale, out IEnumerable<Line> lines) ? new Refused(nameof(OrdinateDimension.GetDisplayLines))
                : new DimensionDisplayGeometry.Ordinate(basePoint, definition, leader, kink1, kink2, toSeq(lines), toSeq(corners)),
            Centermark => new MissingMember(typeof(Centermark), nameof(LinearDimension.GetDisplayLines)),
            _ => throw new UnreachableException(),
        });

    // --- [PIECES]
    public static IO<T> Explode<T>(Dimension dimension, Func<Seq<GeometryBase>, IO<T>> body) =>
        Copies.AcquireNonEmpty(dimension.Explode, nameof(Dimension.Explode)).Bracket(Use: pieces => IO.pure(pieces).Bind(body), Fin: DisposalOps.Release);
}
