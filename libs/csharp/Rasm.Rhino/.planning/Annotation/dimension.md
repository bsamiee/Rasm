# [RASM_RHINO_ANNOTATION_DIMENSION]

`DimensionSpec` admits dimension construction once, `DimAdjust` refits only its matching host kind, and `Dimensions.Commit` folds every mutation through the shared drafting spine. Reads compose the canonical annotation snapshot with named definition-point roles, effective style evidence, viewport-resolved text placement, and explicit custody for exploded native geometry.

## [01]-[INDEX]

- [02]-[ADMISSION]: generated value owners and the closed construction family.
- [03]-[REFIT]: kind-safe geometry adjustment and dimension pose.
- [04]-[MUTATION]: placement, adjustment, text recomputation, and override mutation.
- [05]-[PROJECTION]: the family row table, detached state, named display evidence, formatted text, and leased pieces.

## [02]-[ADMISSION]

- Owner: `DimFrame` enforces plane and coplanar horizontal-axis invariants through one generated construction gate; its perpendicularity gate is ANGLE-class, taken from the kernel `Context` this branch's `[BOUNDARY]` rail already carries — a length-class epsilon in that slot demands bit-exact perpendicularity, and a raw `RhinoDoc` in a public value-object factory re-derives a tolerance the `Context` owns.
- Owner: `DimensionSpec` carries one payload per host construction form and admits every raw geometric value before native construction.
- Law: `AngularExtension` carries extension-point behavior as a value consumed by the line-pair constructor.
- Boundary: `DimensionSpec.Mint` captures the native constructor family through the one `Op.Catch` funnel, so a throwing constructor lands as the keyed `InvalidResult` carrying the caught detail.
- Boundary: a curve-driven construction takes a `GeometryHandle` and reads its native inside one `DraftBorrow` scope, matching the custody every exploded product already crosses on — a raw `Curve` in a public payload names no owner and no lifetime.
- Growth: a construction form lands as one `DimensionSpec` case and one total dispatch arm.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Globalization;
using LanguageExt.UnsafeValueAccess;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

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

[ComplexValueObject]
public sealed partial class DimFrame {
    public Plane Plane { get; }
    public Option<Vector3d> Horizontal { get; }
    public AngleTolerance Tolerance { get; }

    // `IsPerpendicularTo`'s second argument is an ANGLE tolerance in radians: a zero-length epsilon there demands
    // bit-exact perpendicularity and refuses every hand-built frame, so the gate takes the kernel `Context`'s own
    // angle-class owner, which admits its radian interval at its own gate, never a length-class epsilon.
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Plane plane,
        ref Option<Vector3d> horizontal,
        ref AngleTolerance tolerance) =>
        validationError = plane.IsValid
            && horizontal.ForAll(axis => axis.IsValid && !axis.IsZero && axis.IsPerpendicularTo(plane.Normal, tolerance.Value))
            ? null
            : new ValidationError(message: "Dimension frame is invalid.");

    public static Fin<DimFrame> Of(
        Plane plane, Context context, Option<Vector3d> horizontal = default, Op? key = null) {
        Op op = key.OrDefault();
        return from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from frame in Admission.Admitted(
                   fault: Validate(plane, horizontal, domain.Angle, out DimFrame? admitted),
                   value: admitted,
                   refusal: op.InvalidInput())
               select frame;
    }

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
    public sealed record MarkOn(DimFrame Frame, GeometryHandle Source, double Parameter) : DimensionSpec;

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
                // The curve lives only inside its handle's lease, so the domain gate and the host construction share
                // ONE borrow scope: a range check outside it reads a native the scope has already closed.
                markOn: static (ctx, spec) => spec.Source.Typed<Curve, Dimension>(key: ctx.Op, project: curve =>
                    from _ in guard(spec.Parameter >= curve.Domain.Min && spec.Parameter <= curve.Domain.Max,
                        ctx.Op.InvalidInput()).ToFin()
                    from mark in Fin.Succ<Dimension>(value: Centermark.Create(
                        dimStyle: ctx.Style, plane: spec.Frame.Plane, curve: curve, curveParameter: spec.Parameter))
                    select mark)))
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
            from source in key.Need(value: spec.Source)
            from parameter in key.AcceptInput(value: spec.Parameter)
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
            from linear in ctx.Op.Need(ctx.Geometry as LinearDimension)
            from _ in ctx.Op.Catch(() => linear.SetLocations(
                extensionLine1End: fit.Ext1End, extensionLine2End: fit.Ext2End, pointOnDimensionLine: fit.OnDimLine))
            select unit,
        angularVertex: static (ctx, fit) =>
            from angular in ctx.Op.Need(ctx.Geometry as AngularDimension)
            from _ in ctx.Op.Confirm(success: angular.AdjustFromPoints(
                plane: fit.Plane,
                centerpoint: fit.Center,
                defpoint1: fit.Def1,
                defpoint2: fit.Def2,
                dimlinepoint: fit.Line))
            select unit,
        angularSpread: static (ctx, fit) =>
            from angular in ctx.Op.Need(ctx.Geometry as AngularDimension)
            from _ in ctx.Op.Confirm(success: angular.AdjustFromPoints(
                plane: fit.Plane,
                extpoint1: fit.Ext1,
                extpoint2: fit.Ext2,
                dirpoint1: fit.Dir1,
                dirpoint2: fit.Dir2,
                dimlinepoint: fit.Line))
            select unit,
        radial: static (ctx, fit) =>
            from radial in ctx.Op.Need(ctx.Geometry as RadialDimension)
            from _ in ctx.Op.Confirm(success: radial.AdjustFromPoints(
                plane: fit.Plane,
                centerpoint: fit.Center,
                radiuspoint: fit.RadiusPoint,
                dimlinepoint: fit.Line,
                rotationInPlane: fit.RotationRadians))
            select unit,
        ordinate: static (ctx, fit) =>
            from ordinate in ctx.Op.Need(ctx.Geometry as OrdinateDimension)
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
            from mark in ctx.Op.Need(ctx.Geometry as Centermark)
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
        Admission.Admitted(
            fault: Validate(textPosition, textRotation, textPoint, plainUserText, distanceScale, detail, out DimPose? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());

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
    public sealed record Restate(TableTarget Target, Option<ModelUnit> Units = default) : DimOp;
    public sealed record Redisplay(TableTarget Target, LengthDisplayRow Display, LengthChannel Channel) : DimOp;
    public sealed record Restyle(TableTarget Target, StylePatch Patch) : DimOp;
    public sealed record Unstyle(TableTarget Target) : DimOp;

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        place: static (ctx, edit) =>
            from style in edit.Style.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
            from minted in edit.Spec.Mint(style: style, op: ctx.Op)
            from receipt in new Lease<Dimension>.Owned(Value: minted).Use(owned =>
                from _ in edit.Overrides.Traverse(patch => patch.Overlay(annotation: owned, key: ctx.Op).Map(static _ => unit)).As()
                from id in ctx.Op.Catch(() => ResourceId.Admit(ctx.Document.Objects.Add(
                    geometry: owned, attributes: edit.Attributes.ValueUnsafe(),
                    history: null, reference: false), ctx.Op))
                from placed in DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id), key: ctx.Op)
                select placed)
            select receipt,
        adjust: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Adjusted,
            (dimension, key) => edit.Fit.Apply(geometry: dimension, op: key)),
        repose: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Adjusted,
            (dimension, key) => edit.Pose.Apply(geometry: dimension, key: key)),
        restate: static (ctx, edit) => Amended(ctx.Document, edit.Target, ctx.Op, DraftSlot.Reformulated,
            (dimension, key) => key.Catch(() => dimension.UpdateDimensionText(
                dimension.DimensionStyle,
                edit.Units.Map(static unit => unit.System).IfNone(ctx.Document.ModelUnitSystem)))),
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
            change: (annotation, key) => key.Need(annotation as Dimension)
                .Bind(dimension => change(dimension, key)));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Dimensions {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<DimOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key),
            op: Op.Of(name: nameof(Dimensions)));

    public static Fin<DimAnswer> Ask(DocumentSession session, DimAsk request) {
        Op op = Op.Of(name: nameof(Dimensions));
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[PROJECTION]

- Owner: `DimFamily` is the measuring-family row table — one row per host dimension class carrying its probe, its `DimKindFacts` read, and its `DimSkeleton` read, so a family is one row and no reader re-spells the native type test; `DimKindFacts` stays the payload vocabulary the rows construct.
- Owner: `DimState` composes `TextState` instead of repeating annotation identity, frame, content, formatting, mask, and style fields, then adds the resolved family, dimension value, pose, effective setting rows, and per-kind facts.
- Owner: `DimPointRole` labels every constructor and display point, so arity never becomes positional consumer knowledge.
- Owner: `DimAsk` closes state, display, formatted value, viewport text transform, and exploded-piece custody under one request family; every scale it carries is the shared `DraftScale`, admitted at its own gate rather than re-guarded per arm.
- Law: a state read proves `AnnotationKind.Measures` before resolving a family, so a non-measuring annotation refuses at the kind rather than at a probe scan that reads like a missing implementation.
- Boundary: exploded geometry crosses through `DraftCustody.Crossed`, the namespace's one detach fold, and `DimAnswer.Pieces` owns the detached handles until disposal.
- Boundary: `IDisposable` rides the `DimAnswer` union with a total `Dispose` switch, so the `Fin<DimAnswer>` every ask returns is `using`-scopable whatever case it carries; a disposer on the one carrying case leaves the union un-scopable and leaks the pieces.
- Boundary: piece disposal attempts every handle, and the accumulated cleanup fault parks on the answer's own `Faults` cell — a disposer carries no rail outward and a throw there replaces the primary exception mid-unwind.

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
}

// The two payload shapes every `DimFamily` column constructs, so they precede the row table that returns them.
public readonly record struct DimPoint(DimPointRole Role, Point3d Value);

public sealed record DimSkeleton(
    Seq<DimPoint> Points,
    Seq<Line> Lines,
    Seq<Arc> Arcs,
    Arr<Point3d> TextBox) : IDetachedDocumentResult;

// One row per measuring family carrying every behaviour that used to be a separate `switch` over the native type:
// the family probe, the per-kind fact read, and the point/line/arc skeleton read. A new family is one row; a
// dispatcher reads the row instead of re-spelling the type test, and a family the host gives no display readers says
// so in its own skeleton column instead of falling through a shared default nothing declares.
[SmartEnum<int>]
public sealed partial class DimFamily {
    public static readonly DimFamily Linear = new(
        key: 0,
        probe: static geometry => geometry is LinearDimension,
        facts: static (geometry, key) => key.Catch(() =>
            from linear in key.Need(geometry as LinearDimension)
            select (DimKindFacts)new DimKindFacts.Linear(linear.DistanceBetweenArrowTips)),
        skeleton: static (geometry, scale, key) =>
            from linear in key.Need(geometry as LinearDimension)
            from points in key.Catch(() => linear.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Extension1, a), new DimPoint(DimPointRole.Extension2, b),
                    new DimPoint(DimPointRole.Arrow1, c), new DimPoint(DimPointRole.Arrow2, d),
                    new DimPoint(DimPointRole.DimensionLine, e), new DimPoint(DimPointRole.Text, f)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in key.Catch(() => linear.GetDisplayLines(linear.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(key.InvalidResult()))
            from box in TextBox(linear.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box));

    public static readonly DimFamily Angular = new(
        key: 1,
        probe: static geometry => geometry is AngularDimension,
        facts: static (geometry, key) => key.Catch(() =>
            from angular in key.Need(geometry as AngularDimension)
            select (DimKindFacts)new DimKindFacts.Angular(
                StyleValue.Of(angular.AngleFormat), angular.AngleResolution, angular.AngleRoundoff,
                StyleValue.Of(angular.AngleZeroSuppression))),
        skeleton: static (geometry, scale, key) =>
            from angular in key.Need(geometry as AngularDimension)
            from points in key.Catch(() => angular.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f, out Point3d g)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Definition1, b),
                    new DimPoint(DimPointRole.Definition2, c), new DimPoint(DimPointRole.DimensionLine, d),
                    new DimPoint(DimPointRole.Arrow1, e), new DimPoint(DimPointRole.Arrow2, f), new DimPoint(DimPointRole.Text, g)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from display in key.Catch(() => angular.GetDisplayLines(angular.DimensionStyle, scale.Value, out Line[] lines, out Arc[] arcs)
                ? Fin.Succ(value: (Lines: toSeq(lines), Arcs: toSeq(arcs)))
                : Fin.Fail<(Seq<Line> Lines, Seq<Arc> Arcs)>(key.InvalidResult()))
            from box in TextBox(angular.GetTextRectangle, key)
            select new DimSkeleton(points, display.Lines, display.Arcs, box));

    public static readonly DimFamily Radial = new(
        key: 2,
        probe: static geometry => geometry is RadialDimension,
        facts: static (geometry, key) => key.Catch(() =>
            from radial in key.Need(geometry as RadialDimension)
            select (DimKindFacts)new DimKindFacts.Radial(
                StyleValue.Of(radial.LeaderTextHorizontalAlignment),
                StyleValue.Of(radial.LeaderArrowType),
                radial.LeaderArrowSize,
                ResourceId.Maybe(radial.LeaderArrowBlockId),
                StyleValue.Of(radial.LeaderCurveStyle))),
        skeleton: static (geometry, scale, key) =>
            from radial in key.Need(geometry as RadialDimension)
            from points in key.Catch(() => radial.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Radius, b),
                    new DimPoint(DimPointRole.DimensionLine, c), new DimPoint(DimPointRole.Knee, d)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in key.Catch(() => radial.GetDisplayLines(radial.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(key.InvalidResult()))
            from box in TextBox(radial.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box));

    public static readonly DimFamily Ordinate = new(
        key: 3,
        probe: static geometry => geometry is OrdinateDimension,
        facts: static (geometry, key) => key.Catch(() =>
            from ordinate in key.Need(geometry as OrdinateDimension)
            from axis in key.AcceptValidated<OrdinateAxis>(candidate: (int)ordinate.Direction)
            select (DimKindFacts)new DimKindFacts.Ordinate(axis, ordinate.KinkOffset1, ordinate.KinkOffset2)),
        skeleton: static (geometry, scale, key) =>
            from ordinate in key.Need(geometry as OrdinateDimension)
            from points in key.Catch(() => ordinate.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Definition1, a), new DimPoint(DimPointRole.Leader, b),
                    new DimPoint(DimPointRole.Kink1, c), new DimPoint(DimPointRole.Kink2, d), new DimPoint(DimPointRole.Text, e)))
                : Fin.Fail<Seq<DimPoint>>(key.InvalidResult()))
            from lines in key.Catch(() => ordinate.GetDisplayLines(ordinate.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(key.InvalidResult()))
            from box in TextBox(ordinate.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box));

    // `Centermark` publishes `AdjustFromPoints` and `Radius` alone — no point, display-line, or text-rectangle
    // reader — so its skeleton column is the declared refusal rather than an unstated fall-through.
    public static readonly DimFamily Mark = new(
        key: 4,
        probe: static geometry => geometry is Centermark,
        facts: static (geometry, key) => key.Catch(() =>
            from mark in key.Need(geometry as Centermark)
            select (DimKindFacts)new DimKindFacts.Mark(mark.Radius)),
        skeleton: static (geometry, _, key) => Fin.Fail<DimSkeleton>(error: key.Unsupported(
            valueType: geometry.GetType(), outputType: typeof(DimSkeleton))));

    [UseDelegateFromConstructor]
    internal partial bool Probe(Dimension geometry);

    [UseDelegateFromConstructor]
    internal partial Fin<DimKindFacts> Facts(Dimension geometry, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<DimSkeleton> Skeleton(Dimension geometry, DraftScale scale, Op key);

    internal static Fin<DimFamily> Of(Dimension geometry, Op key) =>
        toSeq(Items).Find(row => row.Probe(geometry: geometry))
            .ToFin(Fail: key.Unsupported(valueType: geometry.GetType(), outputType: typeof(DimFamily)));

    private delegate bool TextRectProbe(out Point3d[] corners);

    private static Fin<Arr<Point3d>> TextBox(TextRectProbe probe, Op key) => key.Catch(() =>
        probe(out Point3d[] corners) ? Fin.Succ(value: toArr(corners)) : Fin.Fail<Arr<Point3d>>(key.InvalidResult()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DimState(
    TextState Annotation,
    AnnotationKind Kind,
    DimFamily Family,
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAsk {
    private DimAsk() { }
    public sealed record State(TableTarget Target) : DimAsk;
    public sealed record Skeleton(TableTarget Target, DraftScale Scale) : DimAsk;
    public sealed record ValueText(TableTarget Target, Option<ModelUnit> Units = default) : DimAsk;
    public sealed record TextTransform(TableTarget Target, ViewportInfo Viewport, DraftScale Scale, TextFacing Facing) : DimAsk;
    public sealed record Pieces(TableTarget Target) : DimAsk;

    internal Fin<DimAnswer> Answer(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        state: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from annotation in TextState.Of(native: dimension.Native, key: ctx.Op)
            from kind in ctx.Op.AcceptValidated<AnnotationKind>(candidate: (int)dimension.Geometry.AnnotationType)
            from _ in guard(kind.Measures, ctx.Op.InvalidResult(detail: kind.Key.ToString(CultureInfo.InvariantCulture))).ToFin()
            from family in DimFamily.Of(geometry: dimension.Geometry, key: ctx.Op)
            from facts in family.Facts(geometry: dimension.Geometry, key: ctx.Op)
            from settings in toSeq(StyleField.Items)
                .TraverseM(field => ctx.Op.Catch(() => field.Read(
                    style: dimension.Geometry.DimensionStyle,
                    key: ctx.Op)).Map(value => new StyleSetting(Field: field, Value: value)))
                .As()
            select (DimAnswer)new DimAnswer.State(new DimState(
                annotation, kind, family, dimension.Geometry.NumericValue,
                Op.Text(dimension.Geometry.PlainUserText),
                dimension.Geometry.TextPosition, dimension.Geometry.TextRotation, dimension.Geometry.UseDefaultTextPoint,
                ResourceId.Maybe(dimension.Geometry.DetailMeasured),
                dimension.Geometry.DistanceScale, dimension.Geometry.DimensionScale,
                dimension.Geometry.DimensionStyle.DimensionScaleValue, settings, facts)),
        skeleton: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from family in DimFamily.Of(geometry: dimension.Geometry, key: ctx.Op)
            from skeleton in family.Skeleton(geometry: dimension.Geometry, scale: ask.Scale, key: ctx.Op)
            select (DimAnswer)new DimAnswer.Skeleton(skeleton),
        valueText: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            let units = ask.Units.Map(static unit => unit.System).IfNone(ctx.Document.ModelUnitSystem)
            from text in ctx.Op.Catch(() => dimension.Geometry switch {
                AngularDimension angular => ctx.Op.AcceptText(angular.GetAngleDisplayText(angular.DimensionStyle)),
                LinearDimension linear => ctx.Op.AcceptText(linear.GetDistanceDisplayText(units, linear.DimensionStyle)),
                RadialDimension radial => ctx.Op.AcceptText(radial.GetDistanceDisplayText(units, radial.DimensionStyle)),
                OrdinateDimension ordinate => ctx.Op.AcceptText(ordinate.GetDistanceDisplayText(units, ordinate.DimensionStyle)),
                var unknown => Fin.Fail<string>(error: ctx.Op.Unsupported(
                    valueType: unknown.GetType(), outputType: typeof(string))),
            })
            select (DimAnswer)new DimAnswer.Formatted(text),
        textTransform: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from transform in ctx.Op.Catch(() => Fin.Succ(value: dimension.Geometry.GetTextTransform(
                viewport: ask.Viewport, style: dimension.Geometry.DimensionStyle,
                textScale: ask.Scale.Value, drawForward: ask.Facing.Key)))
            select (DimAnswer)new DimAnswer.Transformed(transform),
        pieces: static (ctx, ask) =>
            from dimension in Resolved(ctx.Document, ask.Target, ctx.Op)
            from products in ctx.Op.Catch(() => Optional(dimension.Geometry.Explode())
                .Map(static values => toSeq(values)).ToFin(Fail: ctx.Op.InvalidResult()))
            from handles in DraftCustody.Crossed(products: products, op: ctx.Op)
            select (DimAnswer)new DimAnswer.Pieces(handles));

    private static Fin<(AnnotationObjectBase Native, Dimension Geometry)> Resolved(
        RhinoDoc document,
        TableTarget target,
        Op key) =>
        from annotation in TextAsk.Single(document: document, target: target, key: key)
        from dimension in key.Need(annotation.AnnotationGeometry as Dimension)
        select (annotation, dimension);
}

// `IDisposable` rides the UNION, never the one carrying case: `Fin<DimAnswer>` is what `Dimensions.Ask` returns, so a
// case-local disposer leaves the answer un-`using`-able and the exploded pieces leak at every call site that folds it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAnswer : IDetachedDocumentResult, IDisposable {
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());

    private DimAnswer() { }

    public sealed record State(DimState Snapshot) : DimAnswer;
    public sealed record Skeleton(DimSkeleton Value) : DimAnswer;
    public sealed record Formatted(string Text) : DimAnswer;
    public sealed record Transformed(Transform Value) : DimAnswer;
    public sealed record Pieces(Seq<GeometryHandle> Products) : DimAnswer;

    public Seq<Error> Faults => faults.Value;

    // Disposal carries no rail outward, so a refused release parks on this answer's evidence cell; throwing here
    // replaces the primary exception mid-`using`-unwind, which `libs/csharp/.planning/RULINGS.md` forecloses.
    public void Dispose() => _ = Switch(
        state: static _ => unit,
        skeleton: static _ => unit,
        formatted: static _ => unit,
        transformed: static _ => unit,
        pieces: row => DraftCustody.Release(values: row.Products, op: Op.Of(name: nameof(DimAnswer))).Match(
            Succ: static _ => unit,
            Fail: fault => ignore(faults.Swap(rows => rows.Add(fault)))));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
