using System.Diagnostics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionAdjustment {
    public sealed record Linear(Point2d ExtensionLine1End, Point2d ExtensionLine2End, Point2d PointOnDimensionLine) : DimensionAdjustment;

    public sealed record AngularVertex(Plane Plane, Point3d CenterPoint, Point3d DefPoint1, Point3d DefPoint2, Point3d DimLinePoint) : DimensionAdjustment;

    public sealed record AngularSpread(Plane Plane, Point3d ExtPoint1, Point3d ExtPoint2, Point3d DirPoint1, Point3d DirPoint2, Point3d DimLinePoint) : DimensionAdjustment;

    public sealed record Radial(Plane Plane, Point3d CenterPoint, Point3d RadiusPoint, Point3d DimLinePoint) : DimensionAdjustment;

    public sealed record Ordinate(Plane Plane, OrdinateDimension.MeasuredDirection Direction, Point3d BasePoint, Point3d DefPoint, Point3d LeaderPoint, double KinkOffset1, double KinkOffset2) : DimensionAdjustment;

    public sealed record Centermark(Plane Plane, Point3d CenterPoint) : DimensionAdjustment;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionForm {
    public sealed record Linear(AnnotationType Kind, Plane Plane, Vector3d Horizontal, Point3d DefPoint1, Point3d DefPoint2, Point3d DimLinePoint, double RotationInPlane) : DimensionForm;

    public sealed record AngularVertex(Vector3d Horizontal, DimensionAdjustment.AngularVertex Adjustment) : DimensionForm;

    public sealed record AngularSpread(Vector3d Horizontal, DimensionAdjustment.AngularSpread Adjustment) : DimensionForm;

    public sealed record AngularLines(Line Line1, Point3d PointOnLine1, Line Line2, Point3d PointOnLine2, Point3d PointOnArc, bool SetExtensionPoints) : DimensionForm;

    public sealed record AngularArc(Arc Arc, double Offset) : DimensionForm;

    public sealed record Radial(AnnotationType Kind, DimensionAdjustment.Radial Adjustment) : DimensionForm;

    public sealed record Ordinate(DimensionAdjustment.Ordinate Adjustment) : DimensionForm;

    public sealed record CentermarkAt(double Radius, DimensionAdjustment.Centermark Adjustment) : DimensionForm;

    public sealed record CentermarkOn(Plane Plane, Curve Curve, double CurveParameter) : DimensionForm;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionTypeState {
    public sealed record Linear(double DistanceBetweenArrowTips, bool Aligned) : DimensionTypeState;

    public sealed record Angular(DimensionStyle.AngleDisplayFormat AngleFormat, int AngleResolution, double AngleRoundoff, DimensionStyle.ZeroSuppression AngleZeroSuppression) : DimensionTypeState;

    public sealed record Radial(bool IsDiameterDimension, TextHorizontalAlignment LeaderTextHorizontalAlignment, DimensionStyle.ArrowType LeaderArrowType, double LeaderArrowSize, Guid LeaderArrowBlockId, DimensionStyle.LeaderCurveStyle LeaderCurveStyle) : DimensionTypeState;

    public sealed record Ordinate(OrdinateDimension.MeasuredDirection Direction, double KinkOffset1, double KinkOffset2) : DimensionTypeState;

    public sealed record Centermark(double Radius) : DimensionTypeState;
}

public sealed record DimensionState(
    AnnotationType AnnotationType,
    string DisplayText,
    bool HasMeasurableTextFields,
    double NumericValue,
    string PlainUserText,
    string TextFormula,
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
    public sealed record Linear(Point3d ExtensionLine1End, Point3d ExtensionLine2End, Point3d Arrowhead1End, Point3d Arrowhead2End, Point3d DimLinePoint, Point3d TextPoint, Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry;

    public sealed record Angular(Point3d CenterPoint, Point3d DefPoint1, Point3d DefPoint2, Point3d ArrowPoint1, Point3d ArrowPoint2, Point3d DimLinePoint, Point3d TextPoint, Seq<Line> Lines, Seq<Arc> Arcs, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry;

    public sealed record Radial(Point3d CenterPoint, Point3d RadiusPoint, Point3d DimLinePoint, Point3d KneePoint, Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry;

    public sealed record Ordinate(Point3d BasePoint, Point3d DefPoint, Point3d LeaderPoint, Point3d KinkPoint1, Point3d KinkPoint2, Seq<Line> Lines, Seq<Point3d> TextRectangle) : DimensionDisplayGeometry;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MissingMember(Type Subject, string Member) : Expected("{Subject} has no {Member}", ErrorOps.Code<MissingMember>());

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Dimensions {
    // --- [PLACEMENT]
    public static IO<Guid> Place(RhinoDoc doc, DimensionForm form, DimensionStyle style, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        Disposal.Using(
            form.Switch(
                style,
                linear: static (parent, linear) =>
                    IO.lift(() => Missing.Unless<Dimension>(LinearDimension.Create(linear.Kind, parent, linear.Plane, linear.Horizontal, linear.DefPoint1, linear.DefPoint2, linear.DimLinePoint, linear.RotationInPlane), nameof(LinearDimension.Create))),
                angularVertex: static (parent, vertex) => IO.lift(() => Missing.Unless<Dimension>(
                    AngularDimension.Create(parent, vertex.Adjustment.Plane, vertex.Horizontal, vertex.Adjustment.CenterPoint, vertex.Adjustment.DefPoint1, vertex.Adjustment.DefPoint2, vertex.Adjustment.DimLinePoint),
                    nameof(AngularDimension.Create))),
                angularSpread: static (parent, spread) => IO.lift(() => Missing.Unless<Dimension>(
                    AngularDimension.Create(
                        parent, spread.Adjustment.Plane, spread.Horizontal, spread.Adjustment.ExtPoint1, spread.Adjustment.ExtPoint2, spread.Adjustment.DirPoint1, spread.Adjustment.DirPoint2, spread.Adjustment.DimLinePoint),
                    nameof(AngularDimension.Create))),
                angularLines: static (parent, lines) =>
                    IO.lift(() => Missing.Unless<Dimension>(AngularDimension.Create(parent, lines.Line1, lines.PointOnLine1, lines.Line2, lines.PointOnLine2, lines.PointOnArc, lines.SetExtensionPoints), nameof(AngularDimension.Create))),
                angularArc: static (parent, arc) => IO.lift<Dimension>(() => new AngularDimension(arc.Arc, arc.Offset) { ParentDimensionStyle = parent }),
                radial: static (parent, radial) => IO.lift(() => Missing.Unless<Dimension>(
                    RadialDimension.Create(parent, radial.Kind, radial.Adjustment.Plane, radial.Adjustment.CenterPoint, radial.Adjustment.RadiusPoint, radial.Adjustment.DimLinePoint),
                    nameof(RadialDimension.Create))),
                ordinate: static (parent, ordinate) => IO.lift(() => Missing.Unless<Dimension>(
                    OrdinateDimension.Create(
                        parent,
                        ordinate.Adjustment.Plane,
                        ordinate.Adjustment.Direction,
                        ordinate.Adjustment.BasePoint,
                        ordinate.Adjustment.DefPoint,
                        ordinate.Adjustment.LeaderPoint,
                        ordinate.Adjustment.KinkOffset1,
                        ordinate.Adjustment.KinkOffset2),
                    nameof(OrdinateDimension.Create))),
                centermarkAt: static (parent, mark) =>
                    IO.lift(() => Missing.Unless<Dimension>(Centermark.Create(parent, mark.Adjustment.Plane, mark.Adjustment.CenterPoint, mark.Radius), nameof(Centermark.Create))),
                centermarkOn: static (parent, mark) =>
                    from parameter in IO.lift(() => Invalid.Unless(mark.Curve.Domain.IncludesParameter(mark.CurveParameter), nameof(Curve.Domain)))
                    from created in IO.lift(() => Missing.Unless<Dimension>(Centermark.Create(parent, mark.Plane, mark.Curve, mark.CurveParameter), nameof(Centermark.Create)))
                    select created),
            dimension =>
                from ids in TableOps.Apply(doc, new TableOp.Add(Seq(new GeometryPair(dimension, attributes)), history, reference))
                from id in IO.lift(() => ids.Head.ToFin(new Missing(nameof(TableOps.Apply))))
                select id);

    // --- [EDITS]
    public static IO<Unit> Adjust(RhinoDoc doc, Guid id, DimensionAdjustment adjustment) =>
        adjustment.Switch(
            (Doc: doc, Id: id),
            linear: static (target, linear) => RhinoObjects.ReplaceGeometry<LinearDimension>(target.Doc, target.Id, dimension =>
                IO.lift(() => dimension.SetLocations(linear.ExtensionLine1End, linear.ExtensionLine2End, linear.PointOnDimensionLine))),
            angularVertex: static (target, vertex) => RhinoObjects.ReplaceGeometry<AngularDimension>(target.Doc, target.Id, dimension =>
                IO.lift(() => Refused.Unless(dimension.AdjustFromPoints(vertex.Plane, vertex.CenterPoint, vertex.DefPoint1, vertex.DefPoint2, vertex.DimLinePoint), nameof(AngularDimension.AdjustFromPoints)))),
            angularSpread: static (target, spread) => RhinoObjects.ReplaceGeometry<AngularDimension>(target.Doc, target.Id, dimension =>
                IO.lift(() => Refused.Unless(dimension.AdjustFromPoints(spread.Plane, spread.ExtPoint1, spread.ExtPoint2, spread.DirPoint1, spread.DirPoint2, spread.DimLinePoint), nameof(AngularDimension.AdjustFromPoints)))),
            radial: static (target, radial) => RhinoObjects.ReplaceGeometry<RadialDimension>(target.Doc, target.Id, dimension =>
                IO.lift(() => Refused.Unless(dimension.AdjustFromPoints(radial.Plane, radial.CenterPoint, radial.RadiusPoint, radial.DimLinePoint, rotationInPlane: 0.0), nameof(RadialDimension.AdjustFromPoints)))),
            ordinate: static (target, ordinate) => RhinoObjects.ReplaceGeometry<OrdinateDimension>(target.Doc, target.Id, dimension =>
                IO.lift(() => Refused.Unless(dimension.AdjustFromPoints(ordinate.Plane, ordinate.Direction, ordinate.BasePoint, ordinate.DefPoint, ordinate.LeaderPoint, ordinate.KinkOffset1, ordinate.KinkOffset2), nameof(OrdinateDimension.AdjustFromPoints)))),
            centermark: static (target, mark) => RhinoObjects.ReplaceGeometry<Centermark>(target.Doc, target.Id, dimension =>
                IO.lift(() => Refused.Unless(dimension.AdjustFromPoints(mark.Plane, mark.CenterPoint), nameof(Centermark.AdjustFromPoints)))));

    public static IO<Unit> UpdateDimensionText(RhinoDoc doc, Guid id, LengthUnit units) =>
        RhinoObjects.ReplaceGeometry<Dimension>(doc, id, dimension =>
            Disposal.Using(() => dimension.DimensionStyle, style => IO.lift(() => dimension.UpdateDimensionText(style, units))));

    // --- [READS]
    public static IO<DimensionState> State(RhinoDoc doc, Guid id) =>
        from resolved in Queries.Resolve<AnnotationObjectBase, Dimension>(doc, id)
        from typeState in IO.lift(Fin<DimensionTypeState> () => resolved.Geometry switch {
            LinearDimension linear => TypeState(linear),
            AngularDimension angular => TypeState(angular),
            RadialDimension radial => TypeState(radial),
            OrdinateDimension ordinate => TypeState(ordinate),
            Centermark mark => TypeState(mark),
            _ => throw new UnreachableException(),
        })
        from state in IO.lift(() => Project(resolved.Geometry, resolved.Object.DisplayText, resolved.Object.HasMeasurableTextFields, typeState))
        select state;

    private static partial DimensionTypeState.Linear TypeState(LinearDimension linear);

    private static partial DimensionTypeState.Angular TypeState(AngularDimension angular);

    private static partial DimensionTypeState.Radial TypeState(RadialDimension radial);

    private static partial DimensionTypeState.Ordinate TypeState(OrdinateDimension ordinate);

    private static partial DimensionTypeState.Centermark TypeState(Centermark mark);

    [MapProperty(nameof(Dimension.PlainUserText), nameof(DimensionState.PlainUserText), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(Dimension.TextFormula), nameof(DimensionState.TextFormula), SuppressNullMismatchDiagnostic = true)]
    private static partial DimensionState Project(Dimension dimension, string displayText, bool hasMeasurableTextFields, DimensionTypeState typeState);

    public static IO<DimensionDisplayGeometry> DisplayGeometry(RhinoDoc doc, Guid id, double scale) =>
        from resolved in Queries.Resolve<RhinoObject, Dimension>(doc, id)
        from display in Disposal.Using(() => resolved.Geometry.DimensionStyle, style => IO.lift(resolved.Geometry switch {
            LinearDimension linear => Framed(
                    linear.Get3dPoints(out Point3d extension1, out Point3d extension2, out Point3d arrow1, out Point3d arrow2, out Point3d dimLine, out Point3d text),
                    linear.GetTextRectangle(out Point3d[] corners),
                    linear.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map(_ => (DimensionDisplayGeometry)new DimensionDisplayGeometry.Linear(extension1, extension2, arrow1, arrow2, dimLine, text, toSeq(lines), toSeq(corners))),
            AngularDimension angular => Framed(
                    angular.Get3dPoints(out Point3d center, out Point3d def1, out Point3d def2, out Point3d arrow1, out Point3d arrow2, out Point3d dimLine, out Point3d text),
                    angular.GetTextRectangle(out Point3d[] corners),
                    angular.GetDisplayLines(style, scale, out Line[] lines, out Arc[] arcs))
                .Map(_ => (DimensionDisplayGeometry)new DimensionDisplayGeometry.Angular(
                    center, def1, def2, arrow1, arrow2, dimLine, text, toSeq(lines).Filter(static line => line.IsValid), toSeq(arcs).Filter(static arc => arc.IsValid), toSeq(corners))),
            RadialDimension radial => Framed(
                    radial.Get3dPoints(out Point3d center, out Point3d radiusPoint, out Point3d dimLine, out Point3d knee),
                    radial.GetTextRectangle(out Point3d[] corners),
                    radial.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map(_ => (DimensionDisplayGeometry)new DimensionDisplayGeometry.Radial(center, radiusPoint, dimLine, knee, toSeq(lines), toSeq(corners))),
            OrdinateDimension ordinate => Framed(
                    ordinate.Get3dPoints(out Point3d basePoint, out Point3d def, out Point3d leader, out Point3d kink1, out Point3d kink2),
                    ordinate.GetTextRectangle(out Point3d[] corners),
                    ordinate.GetDisplayLines(style, scale, out IEnumerable<Line> lines))
                .Map(_ => (DimensionDisplayGeometry)new DimensionDisplayGeometry.Ordinate(basePoint, def, leader, kink1, kink2, toSeq(lines), toSeq(corners))),
            Centermark => new MissingMember(typeof(Centermark), nameof(LinearDimension.Get3dPoints)),
            _ => throw new UnreachableException(),
        }))
        select display;

    private static Fin<Unit> Framed(bool pointed, bool framed, bool drawn) =>
        from points in Refused.Unless(pointed, nameof(LinearDimension.Get3dPoints))
        from rectangle in Refused.Unless(framed, nameof(LinearDimension.GetTextRectangle))
        from lines in Refused.Unless(drawn, nameof(LinearDimension.GetDisplayLines))
        select unit;

    public static IO<string> DisplayText(RhinoDoc doc, Guid id, LengthUnit units) =>
        from resolved in Queries.Resolve<RhinoObject, Dimension>(doc, id)
        from text in Disposal.Using(() => resolved.Geometry.DimensionStyle, style => IO.lift<string>(resolved.Geometry switch {
            LinearDimension linear => linear.GetDistanceDisplayText(units, style),
            AngularDimension angular => angular.GetAngleDisplayText(style),
            RadialDimension radial => radial.GetDistanceDisplayText(units, style),
            OrdinateDimension ordinate => ordinate.GetDistanceDisplayText(units, style),
            Centermark => new MissingMember(typeof(Centermark), nameof(LinearDimension.GetDistanceDisplayText)),
            _ => throw new UnreachableException(),
        }))
        select text;

    public static IO<Transform> TextTransform(RhinoDoc doc, Guid id, Option<(ViewportTarget Target, bool DrawForward)> viewport, double textScale) =>
        from resolved in Queries.Resolve<RhinoObject, AnnotationBase>(doc, id)
        from transform in Disposal.Using(() => resolved.Geometry.DimensionStyle, style => viewport.Match(
            Some: view =>
                from row in Viewports.ResolveViewport(doc, view.Target)
                from projected in Disposal.Using(() => new ViewportInfo(row.Viewport), info => IO.lift<Transform>(resolved.Geometry switch {
                    Dimension dimension => dimension.GetTextTransform(info, style, textScale, view.DrawForward),
                    TextEntity text => text.GetTextTransform(info, textScale, style),
                    Leader => new MissingMember(typeof(Leader), nameof(TextEntity.GetTextTransform)),
                    _ => throw new UnreachableException(),
                }))
                select projected,
            None: () => IO.lift<Transform>(resolved.Geometry switch {
                TextEntity text => text.GetTextTransform(textScale, style),
                Dimension or Leader => new MissingMember(resolved.Geometry.GetType(), nameof(TextEntity.GetTextTransform)),
                _ => throw new UnreachableException(),
            })))
        select transform;
}
