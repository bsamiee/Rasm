# [RASM_RHINO_ANNOTATION_DIMENSION]

`DimensionSpec` admits dimension construction once, `DimAdjust` refits only its matching host kind, and `Dimensions.Commit` folds every mutation through the shared drafting spine. Reads compose the canonical annotation snapshot with named definition-point roles, effective style evidence, viewport-resolved text placement, and explicit custody for exploded native geometry.

## [01]-[INDEX]

- [02]-[ADMISSION]: generated value owners and the closed construction family.
- [03]-[REFIT]: kind-safe geometry adjustment and dimension pose.
- [04]-[MUTATION]: placement, adjustment, text recomputation, and override mutation.
- [05]-[PROJECTION]: detached state, named display evidence, formatted text, and leased pieces.

## [02]-[ADMISSION]

- Owner: `DimFrame` enforces plane and coplanar horizontal-axis invariants through one generated construction gate.
- Owner: `DimensionSpec` carries one payload per host construction form and admits every raw geometric value before native construction.
- Policy: `AngularExtension` carries extension-point behavior as a value consumed by the line-pair constructor.
- Boundary: `DimensionSpec.Mint` captures the native constructor family through the one `Op.Catch` funnel, so a throwing constructor lands as the keyed `InvalidResult` carrying the caught detail.
- Growth: a construction form lands as one `DimensionSpec` case and one total dispatch arm.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RadialKind {
    public static readonly RadialKind Radius = new(key: (int)AnnotationType.Radius);
    public static readonly RadialKind Diameter = new(key: (int)AnnotationType.Diameter);

    internal AnnotationType Host => (AnnotationType)Key;
}

[SmartEnum<int>]
public sealed partial class OrdinateAxis {
    public static readonly OrdinateAxis X = new(key: (int)OrdinateDimension.MeasuredDirection.Xaxis);
    public static readonly OrdinateAxis Y = new(key: (int)OrdinateDimension.MeasuredDirection.Yaxis);

    internal OrdinateDimension.MeasuredDirection Host => (OrdinateDimension.MeasuredDirection)Key;
}

[SmartEnum<bool>]
public sealed partial class AngularExtension {
    public static readonly AngularExtension Retain = new(key: false);
    public static readonly AngularExtension Rebuild = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class TextFacing {
    public static readonly TextFacing Native = new(key: false);
    public static readonly TextFacing Forward = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class TextPointMode {
    public static readonly TextPointMode Positioned = new(key: false);
    public static readonly TextPointMode Automatic = new(key: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetailEdit {
    private DetailEdit() { }
    public sealed record Attach(ResourceId Detail) : DetailEdit;
    public sealed record Detach : DetailEdit;

    internal Fin<Unit> Apply(Dimension dimension, Op key) => Switch(
        (Dimension: dimension, Op: key),
        attach: static (context, edit) => context.Op.Catch(() => { context.Dimension.DetailMeasured = edit.Detail.Value; }),
        detach: static (context, _) => context.Op.Catch(() => { context.Dimension.DetailMeasured = Guid.Empty; }));
}

[SmartEnum<int>]
public sealed partial class DimensionKind {
    public static readonly DimensionKind Aligned = new(key: (int)AnnotationType.Aligned);
    public static readonly DimensionKind Rotated = new(key: (int)AnnotationType.Rotated);
    public static readonly DimensionKind Angular = new(key: (int)AnnotationType.Angular);
    public static readonly DimensionKind Angular3pt = new(key: (int)AnnotationType.Angular3pt);
    public static readonly DimensionKind Radius = new(key: (int)AnnotationType.Radius);
    public static readonly DimensionKind Diameter = new(key: (int)AnnotationType.Diameter);
    public static readonly DimensionKind Ordinate = new(key: (int)AnnotationType.Ordinate);
    public static readonly DimensionKind Centermark = new(key: (int)AnnotationType.Centermark);
}

[ComplexValueObject]
public sealed partial class DimFrame {
    public Plane Plane { get; }
    public Option<Vector3d> Horizontal { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Plane plane, ref Option<Vector3d> horizontal) =>
        validationError = plane.IsValid && horizontal.ForAll(axis =>
            axis.IsValid && !axis.IsZero && axis.IsPerpendicularTo(plane.Normal, RhinoMath.ZeroTolerance))
            ? null
            : new ValidationError(message: "Dimension frame is invalid.");

    public static Fin<DimFrame> Of(Plane plane, Option<Vector3d> horizontal = default, Op? key = null) =>
        Validate(plane, horizontal, out DimFrame? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<DimFrame>(error: key.OrDefault().InvalidInput());

    internal Vector3d Reference => Horizontal.IfNone(noneValue: Plane.XAxis);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionSpec {
    private DimensionSpec() { }
    public sealed record Aligned(DimFrame Frame, Point3d From, Point3d To, Point3d Line) : DimensionSpec;
    public sealed record Rotated(DimFrame Frame, Point3d From, Point3d To, Point3d Line, double RotationRadians) : DimensionSpec;
    public sealed record AngularVertex(DimFrame Frame, Point3d Center, Point3d Def1, Point3d Def2, Point3d Line) : DimensionSpec;
    public sealed record AngularSpread(DimFrame Frame, Point3d Ext1, Point3d Ext2, Point3d Dir1, Point3d Dir2, Point3d Line) : DimensionSpec;
    public sealed record AngularLines(Line SideA, Point3d OnA, Line SideB, Point3d OnB, Point3d OnArc, AngularExtension Extension) : DimensionSpec;
    public sealed record AngularArc(Arc Value, double Offset) : DimensionSpec;
    public sealed record Radial(DimFrame Frame, RadialKind Kind, Point3d Center, Point3d RadiusPoint, Point3d Line) : DimensionSpec;
    public sealed record Ordinate(DimFrame Frame, OrdinateAxis Axis, Point3d Base, Point3d Def, Point3d Leader, double Kink1, double Kink2) : DimensionSpec;
    public sealed record MarkAt(DimFrame Frame, Point3d Center, double Radius) : DimensionSpec;
    public sealed record MarkOn(DimFrame Frame, Curve Source, double Parameter) : DimensionSpec;

    internal Fin<Dimension> Mint(DimensionStyle style, Op op) =>
        from _ in Admit(op)
        from minted in op.Catch(() => Switch(
                (Style: style, Op: op),
                aligned: static (ctx, spec) => Fin.Succ<Dimension>(value: LinearDimension.Create(
                    dimtype: AnnotationType.Aligned, dimStyle: ctx.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    defpoint1: spec.From, defpoint2: spec.To, dimlinepoint: spec.Line, rotationInPlane: 0.0)),
                rotated: static (ctx, spec) => Fin.Succ<Dimension>(value: LinearDimension.Create(
                    dimtype: AnnotationType.Rotated, dimStyle: ctx.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    defpoint1: spec.From, defpoint2: spec.To, dimlinepoint: spec.Line, rotationInPlane: spec.RotationRadians)),
                angularVertex: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    centerpoint: spec.Center, defpoint1: spec.Def1, defpoint2: spec.Def2, dimlinepoint: spec.Line)),
                angularSpread: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    extpoint1: spec.Ext1, extpoint2: spec.Ext2, dirpoint1: spec.Dir1, dirpoint2: spec.Dir2, dimlinepoint: spec.Line)),
                angularLines: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx.Style, line1: spec.SideA, pointOnLine1: spec.OnA, line2: spec.SideB, pointOnLine2: spec.OnB,
                    pointOnAngularDimensionArc: spec.OnArc, bSetExtensionPoints: spec.Extension.Key)),
                angularArc: static (ctx, spec) => {
                    AngularDimension built = new(arc: spec.Value, offset: spec.Offset) { ParentDimensionStyle = ctx.Style };
                    return Fin.Succ<Dimension>(value: built);
                },
                radial: static (ctx, spec) => Fin.Succ<Dimension>(value: RadialDimension.Create(
                    dimStyle: ctx.Style, dimtype: spec.Kind.Host, plane: spec.Frame.Plane,
                    centerpoint: spec.Center, radiuspoint: spec.RadiusPoint, dimlinepoint: spec.Line)),
                ordinate: static (ctx, spec) => Fin.Succ<Dimension>(value: OrdinateDimension.Create(
                    dimStyle: ctx.Style, plane: spec.Frame.Plane, direction: spec.Axis.Host,
                    basepoint: spec.Base, defpoint: spec.Def, leaderpoint: spec.Leader,
                    kinkoffset1: spec.Kink1, kinkoffset2: spec.Kink2)),
                markAt: static (ctx, spec) => Fin.Succ<Dimension>(value: Centermark.Create(
                    dimStyle: ctx.Style, plane: spec.Frame.Plane, centerPoint: spec.Center, radius: spec.Radius)),
                markOn: static (ctx, spec) => Fin.Succ<Dimension>(value: Centermark.Create(
                    dimStyle: ctx.Style, plane: spec.Frame.Plane, curve: spec.Source, curveParameter: spec.Parameter))))
        select minted;

    private Fin<Unit> Admit(Op op) => Switch(
        op,
        aligned: static (key, spec) => FramePoints(key, spec.Frame, spec.From, spec.To, spec.Line),
        rotated: static (key, spec) => FramePoints(key, spec.Frame, spec.From, spec.To, spec.Line)
            .Bind(_ => key.AcceptInput(value: spec.RotationRadians)).Map(static _ => unit),
        angularVertex: static (key, spec) => FramePoints(key, spec.Frame, spec.Center, spec.Def1, spec.Def2, spec.Line),
        angularSpread: static (key, spec) => FramePoints(key, spec.Frame, spec.Ext1, spec.Ext2, spec.Dir1, spec.Dir2, spec.Line),
        angularLines: static (key, spec) =>
            from lines in Seq(spec.SideA, spec.SideB).TraverseM(line => key.AcceptInput(value: line)).As()
            from points in Points(key, spec.OnA, spec.OnB, spec.OnArc)
            from extension in key.Need(value: spec.Extension)
            select unit,
        angularArc: static (key, spec) =>
            from arc in key.AcceptInput(value: spec.Value)
            from offset in key.AcceptInput(value: spec.Offset)
            select unit,
        radial: static (key, spec) =>
            from frame in FramePoints(key, spec.Frame, spec.Center, spec.RadiusPoint, spec.Line)
            from kind in key.Need(value: spec.Kind)
            select unit,
        ordinate: static (key, spec) =>
            from frame in FramePoints(key, spec.Frame, spec.Base, spec.Def, spec.Leader)
            from axis in key.Need(value: spec.Axis)
            from kinks in Seq(spec.Kink1, spec.Kink2).TraverseM(kink => key.AcceptInput(value: kink)).As()
            select unit,
        markAt: static (key, spec) => FramePoints(key, spec.Frame, spec.Center)
            .Bind(_ => key.Positive(value: spec.Radius)).Map(static _ => unit),
        markOn: static (key, spec) =>
            from frame in key.Need(value: spec.Frame)
            from sourceValue in key.Need(value: spec.Source)
            from source in key.AcceptInput(value: sourceValue)
            from parameter in key.AcceptInput(value: spec.Parameter)
            from _ in guard(parameter >= source.Domain.Min && parameter <= source.Domain.Max, key.InvalidInput()).ToFin()
            select unit);

    private static Fin<Unit> FramePoints(Op key, DimFrame? frame, params ReadOnlySpan<Point3d> points) =>
        key.Need(value: frame).Bind(_ => Points(key, points));

    private static Fin<Unit> Points(Op key, params ReadOnlySpan<Point3d> points) =>
        LanguageExt.Iterable<Point3d>.FromSpan(points).ToSeq()
            .TraverseM(point => key.AcceptInput(value: point)).As().Map(static _ => unit);
}
```

## [03]-[REFIT]

- Owner: `DimAdjust` refits each measuring kind through its native geometry contract and rejects a mismatched target before mutation.
- Owner: `DimPose` carries dimension-only text placement, measurement scale, and detail binding through one generated aggregate gate.
- Boundary: refit and pose act on the duplicate supplied by `TextOp.Reworked`, so a rejected native edit never mutates document-owned geometry.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAdjust {
    private DimAdjust() { }
    public sealed record Linear(Point2d Ext1End, Point2d Ext2End, Point2d OnDimLine) : DimAdjust;
    public sealed record AngularVertex(Plane Plane, Point3d Center, Point3d Def1, Point3d Def2, Point3d Line) : DimAdjust;
    public sealed record AngularSpread(Plane Plane, Point3d Ext1, Point3d Ext2, Point3d Dir1, Point3d Dir2, Point3d Line) : DimAdjust;
    public sealed record Radial(Plane Plane, Point3d Center, Point3d RadiusPoint, Point3d Line, double RotationRadians) : DimAdjust;
    public sealed record Ordinate(Plane Plane, OrdinateAxis Axis, Point3d Base, Point3d Def, Point3d Leader, double Kink1, double Kink2) : DimAdjust;
    public sealed record Mark(Plane Plane, Point3d Center) : DimAdjust;

    internal Fin<Unit> Apply(Dimension geometry, Op op) =>
        Admit(op).Bind(_ => Switch(
            (Geometry: geometry, Op: op),
            linear: static (ctx, fit) =>
            from linear in Optional(ctx.Geometry as LinearDimension).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Catch(() => linear.SetLocations(
                extensionLine1End: fit.Ext1End, extensionLine2End: fit.Ext2End, pointOnDimensionLine: fit.OnDimLine))
            select unit,
        angularVertex: static (ctx, fit) =>
            from angular in Optional(ctx.Geometry as AngularDimension).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Confirm(success: angular.AdjustFromPoints(
                plane: fit.Plane,
                centerpoint: fit.Center,
                defpoint1: fit.Def1,
                defpoint2: fit.Def2,
                dimlinepoint: fit.Line))
            select unit,
        angularSpread: static (ctx, fit) =>
            from angular in Optional(ctx.Geometry as AngularDimension).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Confirm(success: angular.AdjustFromPoints(
                plane: fit.Plane,
                extpoint1: fit.Ext1,
                extpoint2: fit.Ext2,
                dirpoint1: fit.Dir1,
                dirpoint2: fit.Dir2,
                dimlinepoint: fit.Line))
            select unit,
        radial: static (ctx, fit) =>
            from radial in Optional(ctx.Geometry as RadialDimension).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Confirm(success: radial.AdjustFromPoints(
                plane: fit.Plane,
                centerpoint: fit.Center,
                radiuspoint: fit.RadiusPoint,
                dimlinepoint: fit.Line,
                rotationInPlane: fit.RotationRadians))
            select unit,
        ordinate: static (ctx, fit) =>
            from ordinate in Optional(ctx.Geometry as OrdinateDimension).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Confirm(success: ordinate.AdjustFromPoints(
                plane: fit.Plane,
                direction: fit.Axis.Host,
                basepoint: fit.Base,
                defpoint: fit.Def,
                leaderpoint: fit.Leader,
                kinkoffset1: fit.Kink1,
                kinkoffset2: fit.Kink2))
            select unit,
        mark: static (ctx, fit) =>
            from mark in Optional(ctx.Geometry as Centermark).ToFin(Fail: ctx.Op.InvalidInput())
            from _ in ctx.Op.Confirm(success: mark.AdjustFromPoints(
                plane: fit.Plane, centerPoint: fit.Center))
            select unit));

    private Fin<Unit> Admit(Op op) => Switch(
        op,
        linear: static (key, fit) => Point2s(key, fit.Ext1End, fit.Ext2End, fit.OnDimLine),
        angularVertex: static (key, fit) => PlanePoints(key, fit.Plane, fit.Center, fit.Def1, fit.Def2, fit.Line),
        angularSpread: static (key, fit) => PlanePoints(key, fit.Plane, fit.Ext1, fit.Ext2, fit.Dir1, fit.Dir2, fit.Line),
        radial: static (key, fit) => PlanePoints(key, fit.Plane, fit.Center, fit.RadiusPoint, fit.Line)
            .Bind(_ => key.AcceptInput(value: fit.RotationRadians)).Map(static _ => unit),
        ordinate: static (key, fit) =>
            from points in PlanePoints(key, fit.Plane, fit.Base, fit.Def, fit.Leader)
            from axis in key.Need(value: fit.Axis)
            from kinks in Seq(fit.Kink1, fit.Kink2).TraverseM(kink => key.AcceptInput(value: kink)).As()
            select unit,
        mark: static (key, fit) => PlanePoints(key, fit.Plane, fit.Center));

    private static Fin<Unit> Point2s(Op key, params ReadOnlySpan<Point2d> points) =>
        LanguageExt.Iterable<Point2d>.FromSpan(points).ToSeq()
            .TraverseM(point => key.AcceptInput(value: point)).As().Map(static _ => unit);

    private static Fin<Unit> PlanePoints(Op key, Plane plane, params ReadOnlySpan<Point3d> points) =>
        key.AcceptInput(value: plane)
            .Bind(_ => LanguageExt.Iterable<Point3d>.FromSpan(points).ToSeq()
                .TraverseM(point => key.AcceptInput(value: point)).As())
            .Map(static _ => unit);
}

[ComplexValueObject]
public sealed partial class DimPose {
    public Option<Point2d> TextPosition { get; }
    public Option<double> TextRotation { get; }
    public Option<TextPointMode> TextPoint { get; }
    public Option<string> PlainUserText { get; }
    public Option<double> DistanceScale { get; }
    public Option<DetailEdit> Detail { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Option<Point2d> textPosition, ref Option<double> textRotation,
        ref Option<TextPointMode> textPoint, ref Option<string> plainUserText, ref Option<double> distanceScale,
        ref Option<DetailEdit> detail) {
        bool any = textPosition.IsSome || textRotation.IsSome || textPoint.IsSome
            || plainUserText.IsSome || distanceScale.IsSome || detail.IsSome;
        bool valid = textPosition.ForAll(static point => point.IsValid)
            && textRotation.ForAll(double.IsFinite)
            && textPoint.ForAll(static mode => mode is not null)
            && distanceScale.ForAll(static scale => double.IsFinite(scale) && scale > 0.0)
            && detail.ForAll(static edit => edit is not null);
        validationError = any && valid ? null : new ValidationError(message: "Dimension pose is empty or invalid.");
    }

    public static Fin<DimPose> Of(
        Option<Point2d> textPosition = default, Option<double> textRotation = default,
        Option<TextPointMode> textPoint = default, Option<string> plainUserText = default,
        Option<double> distanceScale = default, Option<DetailEdit> detail = default, Op? key = null) =>
        Validate(textPosition, textRotation, textPoint, plainUserText, distanceScale, detail, out DimPose? admitted) is null
            && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<DimPose>(error: key.OrDefault().InvalidInput());

    internal Fin<Unit> Apply(Dimension geometry, Op key) =>
        from _ in key.Catch(() => {
            TextPosition.Iter(position => geometry.TextPosition = position);
            TextRotation.Iter(rotation => geometry.TextRotation = rotation);
            TextPoint.Iter(mode => geometry.UseDefaultTextPoint = mode.Key);
            PlainUserText.Iter(text => geometry.PlainUserText = text);
            DistanceScale.Iter(scale => geometry.DistanceScale = scale);
        })
        from __ in Detail.Traverse(edit => edit.Apply(dimension: geometry, key: key)).As()
        select unit;
}
```

## [04]-[MUTATION]

- Owner: `DimOp` is the complete dimension mutation program consumed by `Dimensions.Commit`.
- Law: style changes compose `StylePatch`; dimension-specific state remains inside `DimPose`.
- Law: `LengthChannel` couples length-display selection with the matching zero-suppression reset.
- Entry: `Dimensions.Commit` preserves the frozen wire and accepts the shared `DraftPlan<DimOp>` policy owner.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LengthChannel {
    public static readonly LengthChannel Primary = new(apply: static (dimension, display) =>
        Op.Side(() => dimension.SetDimensionLengthDisplayWithZeroSuppressionReset(display)));
    public static readonly LengthChannel Alternate = new(apply: static (dimension, display) =>
        Op.Side(() => dimension.SetAltDimensionLengthDisplayWithZeroSuppressionReset(display)));

    [UseDelegateFromConstructor]
    internal partial Unit Apply(Dimension dimension, DimensionStyle.LengthDisplay display);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimOp {
    private DimOp() { }
    public sealed record Place(DimensionSpec Spec, ResourceRef Style, Option<StylePatch> Overrides = default, Option<ObjectAttributes> Attributes = default) : DimOp;
    public sealed record Adjust(TableTarget Target, DimAdjust Fit) : DimOp;
    public sealed record Repose(TableTarget Target, DimPose Pose) : DimOp;
    public sealed record Restate(TableTarget Target, Option<UnitSystem> Units = default) : DimOp;
    public sealed record Redisplay(TableTarget Target, LengthDisplayRow Display, LengthChannel Channel) : DimOp;
    public sealed record Restyle(TableTarget Target, StylePatch Patch) : DimOp;
    public sealed record Unstyle(TableTarget Target) : DimOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        place: static (ctx, edit) =>
            from style in edit.Style.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
            from minted in edit.Spec.Mint(style: style, op: ctx.Op)
            from _ in edit.Overrides.Traverse(patch => patch.Overlay(annotation: minted, key: ctx.Op).Map(static _ => unit)).As()
            from id in ctx.Op.Catch(() => ResourceId.Admit(ctx.Document.Objects.Add(
                geometry: minted, attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                history: null, reference: false), ctx.Op))
            from receipt in DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id))
            select receipt,
        adjust: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Adjusted,
            (dimension, key) => edit.Fit.Apply(geometry: dimension, op: key)),
        repose: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Adjusted,
            (dimension, key) => edit.Pose.Apply(geometry: dimension, key: key)),
        restate: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Reformulated,
            (dimension, key) => key.Catch(() =>
                dimension.UpdateDimensionText(dimension.DimensionStyle, edit.Units.IfNone(ctx.Document.ModelUnitSystem)))),
        redisplay: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Restyled,
            (dimension, key) => key.Catch(() => edit.Channel.Apply(dimension, edit.Display.Host))),
        restyle: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Restyled,
            (dimension, key) => edit.Patch.Overlay(annotation: dimension, key: key).Map(static _ => unit)),
        unstyle: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Restyled,
            static (dimension, key) => key.Confirm(success: dimension.ClearPropertyOverrides())));

    private static Fin<DraftReceipt> Amended(
        RhinoDoc document,
        TableTarget target,
        Op op,
        DraftSlot slot,
        Func<Dimension, Op, Fin<Unit>> change) =>
        TextOp.Reworked(document: document, target: target, op: op, slot: slot,
            change: (annotation, key) => Optional(annotation as Dimension).ToFin(Fail: key.InvalidInput())
                .Bind(dimension => change(dimension, key)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Dimensions {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<DimOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());

    public static Fin<DimAnswer> Ask(DocumentSession session, DimAsk request) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[PROJECTION]

- Owner: `DimState` composes `TextState` instead of repeating annotation identity, frame, content, formatting, mask, and style fields, then adds dimension value, pose, effective setting rows, and per-kind facts.
- Owner: `DimPointRole` labels every constructor and display point, so arity never becomes positional consumer knowledge.
- Owner: `DimAsk` closes state, display, formatted value, viewport text transform, and exploded-piece custody under one request family.
- Boundary: exploded geometry crosses through one compensating batch, every raw product releases on every outcome, and `DimAnswer.Pieces` owns the detached handles until disposal.
- Boundary: piece disposal attempts every handle and raises the accumulated cleanup fault only after the full custody run settles.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DimPointRole {
    public static readonly DimPointRole Extension1 = new(key: 0);
    public static readonly DimPointRole Extension2 = new(key: 1);
    public static readonly DimPointRole Arrow1 = new(key: 2);
    public static readonly DimPointRole Arrow2 = new(key: 3);
    public static readonly DimPointRole DimensionLine = new(key: 4);
    public static readonly DimPointRole Text = new(key: 5);
    public static readonly DimPointRole Center = new(key: 6);
    public static readonly DimPointRole Definition1 = new(key: 7);
    public static readonly DimPointRole Definition2 = new(key: 8);
    public static readonly DimPointRole Radius = new(key: 9);
    public static readonly DimPointRole Knee = new(key: 10);
    public static readonly DimPointRole Leader = new(key: 11);
    public static readonly DimPointRole Kink1 = new(key: 12);
    public static readonly DimPointRole Kink2 = new(key: 13);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimKindFacts {
    private DimKindFacts() { }
    public sealed record Linear(double ArrowTipSpan) : DimKindFacts;
    public sealed record Angular(StyleValue Format, int Resolution, double Roundoff, StyleValue ZeroSuppression) : DimKindFacts;
    public sealed record Radial(StyleValue TextAlignment, StyleValue Arrow, double ArrowSize, Option<ResourceId> ArrowBlock, StyleValue Curve) : DimKindFacts;
    public sealed record Ordinate(OrdinateAxis Axis, double Kink1, double Kink2) : DimKindFacts;
    public sealed record Mark(double Radius) : DimKindFacts;

    internal static Fin<DimKindFacts> Of(Dimension geometry, Op key) => key.Catch(() => geometry switch {
        LinearDimension linear => Fin.Succ<DimKindFacts>(value: new Linear(linear.DistanceBetweenArrowTips)),
        AngularDimension angular => Fin.Succ<DimKindFacts>(value: new Angular(
            StyleValue.Of(angular.AngleFormat), angular.AngleResolution, angular.AngleRoundoff, StyleValue.Of(angular.AngleZeroSuppression))),
        RadialDimension radial => Fin.Succ<DimKindFacts>(value: new Radial(
            StyleValue.Of(radial.LeaderTextHorizontalAlignment),
            StyleValue.Of(radial.LeaderArrowType),
            radial.LeaderArrowSize,
            ResourceId.Maybe(radial.LeaderArrowBlockId),
            StyleValue.Of(radial.LeaderCurveStyle))),
        OrdinateDimension ordinate => key.AcceptValidated<OrdinateAxis>(candidate: (int)ordinate.Direction)
            .Map(axis => (DimKindFacts)new Ordinate(axis, ordinate.KinkOffset1, ordinate.KinkOffset2)),
        Centermark mark => Fin.Succ<DimKindFacts>(value: new Mark(mark.Radius)),
        var unknown => Fin.Fail<DimKindFacts>(error: key.Unsupported(
            valueType: unknown.GetType(), outputType: typeof(DimKindFacts))),
    });
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct DimPoint(DimPointRole Role, Point3d Value);

public sealed record DimState(
    TextState Annotation,
    DimensionKind Kind,
    double NumericValue,
    Option<string> PlainUserText,
    Point2d TextPosition,
    double TextRotation,
    bool UseDefaultTextPoint,
    Option<ResourceId> DetailMeasured,
    double DistanceScale,
    double DimensionScale,
    double StyleScaleValue,
    Seq<StyleSetting> EffectiveStyle,
    DimKindFacts Facts) : IDetachedDocumentResult;

public sealed record DimSkeleton(
    Seq<DimPoint> Points,
    Seq<Line> Lines,
    Seq<Arc> Arcs,
    Arr<Point3d> TextBox) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAsk {
    private DimAsk() { }
    public sealed record State(TableTarget Target) : DimAsk;
    public sealed record Skeleton(TableTarget Target, double Scale = 1.0) : DimAsk;
    public sealed record ValueText(TableTarget Target, Option<UnitSystem> Units = default) : DimAsk;
    public sealed record TextTransform(TableTarget Target, ViewportInfo Viewport, double Scale, TextFacing Facing) : DimAsk;
    public sealed record Pieces(TableTarget Target) : DimAsk;

    internal Fin<DimAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from annotation in TextState.Of(native: dimension.Native, key: ctx.Op)
            from kind in ctx.Op.AcceptValidated<DimensionKind>(candidate: (int)dimension.Geometry.AnnotationType)
            from facts in DimKindFacts.Of(dimension.Geometry, ctx.Op)
            from settings in toSeq(StyleField.Items)
                .TraverseM(field => ctx.Op.Catch(() => field.Read(
                    style: dimension.Geometry.DimensionStyle,
                    key: ctx.Op)).Map(value => new StyleSetting(Field: field, Value: value)))
                .As()
            select (DimAnswer)new DimAnswer.State(new DimState(
                annotation, kind, dimension.Geometry.NumericValue,
                Optional(dimension.Geometry.PlainUserText).Filter(static text => text.Length > 0),
                dimension.Geometry.TextPosition, dimension.Geometry.TextRotation, dimension.Geometry.UseDefaultTextPoint,
                ResourceId.Maybe(dimension.Geometry.DetailMeasured),
                dimension.Geometry.DistanceScale, dimension.Geometry.DimensionScale,
                dimension.Geometry.DimensionStyle.DimensionScaleValue, settings, facts)),
        skeleton: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from scale in ctx.Op.Positive(value: ask.Scale)
            from skeleton in Skeletal(dimension.Geometry, scale, ctx.Op)
            select (DimAnswer)new DimAnswer.Skeleton(skeleton),
        valueText: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from text in ctx.Op.Catch(() => dimension.Geometry switch {
                AngularDimension angular => ctx.Op.AcceptText(angular.GetAngleDisplayText(angular.DimensionStyle)),
                LinearDimension linear => ctx.Op.AcceptText(linear.GetDistanceDisplayText(ask.Units.IfNone(ctx.Document.ModelUnitSystem), linear.DimensionStyle)),
                RadialDimension radial => ctx.Op.AcceptText(radial.GetDistanceDisplayText(ask.Units.IfNone(ctx.Document.ModelUnitSystem), radial.DimensionStyle)),
                OrdinateDimension ordinate => ctx.Op.AcceptText(ordinate.GetDistanceDisplayText(ask.Units.IfNone(ctx.Document.ModelUnitSystem), ordinate.DimensionStyle)),
                var unknown => Fin.Fail<string>(error: ctx.Op.Unsupported(
                    valueType: unknown.GetType(), outputType: typeof(string))),
            })
            select (DimAnswer)new DimAnswer.Formatted(text),
        textTransform: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from scale in ctx.Op.Positive(value: ask.Scale)
            from transform in ctx.Op.Catch(() => Fin.Succ(value: dimension.Geometry.GetTextTransform(
                viewport: ask.Viewport, style: dimension.Geometry.DimensionStyle, textScale: scale, drawForward: ask.Facing.Key)))
            select (DimAnswer)new DimAnswer.Transformed(transform),
        pieces: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from products in ctx.Op.Catch(() => Optional(dimension.Geometry.Explode())
                .Map(static values => toSeq(values)).ToFin(Fail: ctx.Op.InvalidResult()))
            from handles in Crossed(products: products, key: ctx.Op)
            select (DimAnswer)new DimAnswer.Pieces(handles));

    private static Fin<Seq<GeometryHandle>> Crossed(Seq<GeometryBase> products, Op key) =>
        DocumentCommit.Compensated(
            source: products,
            land: product => GeometryCrossing.Cross(source: product, mode: CrossingMode.Detach, key: key),
            rollback: landed => Release(values: landed, key: key),
            release: sources => Release(values: sources, key: key));

    internal static Fin<Unit> Release<T>(Seq<T> values, Op key) where T : class, IDisposable =>
        values.Fold(
            Fin.Succ(value: unit),
            (state, value) => (value is null ? Fin.Succ(value: unit) : key.Catch(value.Dispose)).Match(
                Succ: _ => state,
                Fail: error => state.Match(
                    Succ: _ => Fin.Fail<Unit>(error: error),
                    Fail: prior => Fin.Fail<Unit>(error: prior + error))));

    private static Fin<(AnnotationObjectBase Native, Dimension Geometry)> Resolved(
        RhinoDoc document,
        TableTarget target,
        Op key) =>
        from annotation in TextAsk.Single(document: document, target: target, key: key)
        from dimension in Optional(annotation.AnnotationGeometry as Dimension).ToFin(Fail: key.InvalidInput())
        select (annotation, dimension);

    private static Fin<DimSkeleton> Skeletal(Dimension geometry, double scale, Op key) => geometry switch {
        LinearDimension linear =>
            from points in key.Catch(() => linear.Get3dPoints(out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Extension1, a), new DimPoint(DimPointRole.Extension2, b),
                    new DimPoint(DimPointRole.Arrow1, c), new DimPoint(DimPointRole.Arrow2, d),
                    new DimPoint(DimPointRole.DimensionLine, e), new DimPoint(DimPointRole.Text, f)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in DisplayLines(linear, scale, key)
            from box in TextBox(linear.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        AngularDimension angular =>
            from points in key.Catch(() => angular.Get3dPoints(out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f, out Point3d g)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Definition1, b),
                    new DimPoint(DimPointRole.Definition2, c), new DimPoint(DimPointRole.DimensionLine, d),
                    new DimPoint(DimPointRole.Arrow1, e), new DimPoint(DimPointRole.Arrow2, f), new DimPoint(DimPointRole.Text, g)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from display in key.Catch(() => angular.GetDisplayLines(angular.DimensionStyle, scale, out Line[] lines, out Arc[] arcs)
                ? Fin.Succ(value: (Lines: toSeq(lines), Arcs: toSeq(arcs)))
                : Fin.Fail<(Seq<Line> Lines, Seq<Arc> Arcs)>(key.InvalidResult()))
            from box in TextBox(angular.GetTextRectangle, key)
            select new DimSkeleton(points, display.Lines, display.Arcs, box),
        RadialDimension radial =>
            from points in key.Catch(() => radial.Get3dPoints(out Point3d a, out Point3d b, out Point3d c, out Point3d d)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Radius, b),
                    new DimPoint(DimPointRole.DimensionLine, c), new DimPoint(DimPointRole.Knee, d)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in DisplayLines(radial, scale, key)
            from box in TextBox(radial.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        OrdinateDimension ordinate =>
            from points in key.Catch(() => ordinate.Get3dPoints(out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Definition1, a), new DimPoint(DimPointRole.Leader, b),
                    new DimPoint(DimPointRole.Kink1, c), new DimPoint(DimPointRole.Kink2, d), new DimPoint(DimPointRole.Text, e)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in DisplayLines(ordinate, scale, key)
            from box in TextBox(ordinate.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        var unknown => Fin.Fail<DimSkeleton>(error: key.Unsupported(
            valueType: unknown.GetType(), outputType: typeof(DimSkeleton))),
    };

    private static Fin<Seq<Line>> DisplayLines(Dimension geometry, double scale, Op key) => key.Catch(() => geometry switch {
        LinearDimension linear when linear.GetDisplayLines(linear.DimensionStyle, scale, out IEnumerable<Line> linearLines) =>
            Fin.Succ(value: toSeq(linearLines)),
        RadialDimension radial when radial.GetDisplayLines(radial.DimensionStyle, scale, out IEnumerable<Line> radialLines) =>
            Fin.Succ(value: toSeq(radialLines)),
        OrdinateDimension ordinate when ordinate.GetDisplayLines(ordinate.DimensionStyle, scale, out IEnumerable<Line> ordinateLines) =>
            Fin.Succ(value: toSeq(ordinateLines)),
        _ => Fin.Fail<Seq<Line>>(error: key.InvalidResult()),
    });

    private delegate bool TextRectProbe(out Point3d[] corners);

    private static Fin<Arr<Point3d>> TextBox(TextRectProbe probe, Op key) => key.Catch(() =>
        probe(out Point3d[] corners) ? Fin.Succ(value: toArr(corners)) : Fin.Fail<Arr<Point3d>>(key.InvalidResult()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAnswer : IDetachedDocumentResult {
    private DimAnswer() { }
    public sealed record State(DimState Snapshot) : DimAnswer;
    public sealed record Skeleton(DimSkeleton Value) : DimAnswer;
    public sealed record Formatted(string Text) : DimAnswer;
    public sealed record Transformed(Transform Value) : DimAnswer;
    public sealed record Pieces(Seq<GeometryHandle> Products) : DimAnswer, IDisposable {
        public void Dispose() => DimAsk.Release(values: Products, key: Op.Of()).ThrowIfFail();
    }
}
```
