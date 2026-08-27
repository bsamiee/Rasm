# [RASM_RHINO_ANNOTATION_DIMENSION]

`DimensionSpec` admits dimension construction once, `DimAdjust` refits only its matching host kind, and `Dimensions.Commit` folds every mutation through the shared drafting spine. Reads compose the canonical annotation snapshot with named definition-point roles, effective style evidence, viewport-resolved text placement, and explicit custody for exploded native geometry.

## [01]-[INDEX]

- [02]-[ADMISSION]: the shared definition-point products, generated value owners, and the closed construction family.
- [03]-[REFIT]: kind-safe geometry adjustment and dimension pose.
- [04]-[MUTATION]: placement, adjustment, text recomputation, and override mutation.
- [05]-[PROJECTION]: the family row table, detached state, named display evidence, formatted text, and leased pieces.

## [02]-[ADMISSION]

- Owner: `SpanPoints`, `VertexPoints`, `SpreadPoints`, `RadialPoints`, and `OrdinatePoints` are the definition-point products construction and refit SHARE — each admits its own run through the kernel's span receiver, so a spec arm and a fit arm over one measuring family cannot drift in arity or in the order they name their points.
- Owner: `DimFrame` enforces plane and coplanar horizontal-axis invariants through one generated construction gate; its perpendicularity gate is ANGLE-class and reads the kernel `Context`'s `Orientation` lane, so the radian interval the host member takes comes from the branch's one tolerance vocabulary rather than a page-local epsilon owner.
- Owner: `DimensionSpec` carries one payload per host construction form and admits every raw geometric value before native construction.
- Law: `AngularExtension` carries extension-point behavior as a value consumed by the line-pair constructor.
- Law: admission ACCUMULATES — a frame refused on its plane and its horizontal axis alike reports both clauses through `DraftFault`'s own semigroup, so a caller fixing one defect does not discover the next on the following attempt.
- Boundary: `DimensionSpec.Mint` captures the native constructor family through the one `Try.lift` funnel, so a throwing constructor lands as the keyed `InvalidResult` carrying the caught detail.
- Boundary: a curve-driven construction takes a `GeometryHandle` and reads its native inside one `DraftBorrow` scope, matching the custody every exploded product already crosses on — a raw `Curve` in a public payload names no owner and no lifetime.
- Packages: `Domain/context` (`Context.For`, `Tolerance`, `ToleranceLane.Orientation`), `Domain/validation` (`Acceptance.Rows` span receiver, `FactoryBridge.Accept`), `Document/session.md` (`DraftFault`), `Annotation/style.md` (`DraftBorrow`, `DraftScale`); RhinoCommon `LinearDimension`/`AngularDimension`/`RadialDimension`/`OrdinateDimension`/`Centermark` per `.api/api-rhinocommon-annotation.md`.
- Growth: a construction form lands as one `DimensionSpec` case and one total dispatch arm; a new point roster is one product both families read.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Display;
using Rhino.Geometry;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
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

    internal Fin<Unit> Apply(Dimension dimension) => Switch(
        dimension,
        attach: static (context, edit) => Try.lift(() =>
            Fin.Succ(value: HostEdge.Side(() => context.DetailMeasured = edit.Detail.Value))).Run().Bind(static inner => inner),
        detach: static (context, _) => Try.lift(() =>
            Fin.Succ(value: HostEdge.Side(() => context.DetailMeasured = Guid.Empty))).Run().Bind(static inner => inner));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SpanPoints(Point3d From, Point3d To, Point3d Line) {
    internal Fin<Unit> Admit() => Acceptance.Rows(From, To, Line).Map(static _ => unit);
}

public readonly record struct VertexPoints(Point3d Center, Point3d Def1, Point3d Def2, Point3d Line) {
    internal Fin<Unit> Admit() => Acceptance.Rows(Center, Def1, Def2, Line).Map(static _ => unit);
}

public readonly record struct SpreadPoints(Point3d Ext1, Point3d Ext2, Point3d Dir1, Point3d Dir2, Point3d Line) {
    internal Fin<Unit> Admit() => Acceptance.Rows(Ext1, Ext2, Dir1, Dir2, Line).Map(static _ => unit);
}

public readonly record struct RadialPoints(Point3d Center, Point3d RadiusPoint, Point3d Line) {
    internal Fin<Unit> Admit() => Acceptance.Rows(Center, RadiusPoint, Line).Map(static _ => unit);
}

public readonly record struct OrdinatePoints(Point3d Base, Point3d Def, Point3d Leader, double Kink1, double Kink2) {
    internal Fin<Unit> Admit() =>
        from _ in Acceptance.Rows(Base, Def, Leader)
        from __ in Acceptance.Rows(Kink1, Kink2)
        select unit;
}

[ComplexValueObject]
[ValidationError]
public sealed partial class DimFrame {
    public Plane Plane { get; }
    public Option<Vector3d> Horizontal { get; }
    public Tolerance Tolerance { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Plane plane,
        ref Option<Vector3d> horizontal,
        ref Tolerance tolerance) {
        (Plane frame, Option<Vector3d> axis, Tolerance gate) = (plane, horizontal, tolerance);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (!frame.IsValid, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Plane) }))),
            (axis.Exists(static value => !value.IsValid || value.IsZero), static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Horizontal) }))),
            (!axis.ForAll(value => value.IsPerpendicularTo(frame.Normal, gate.Value)), () => new ValidationClause(string.Join(" | ", new object?[] { nameof(Horizontal), $"perpendicular to the frame normal within the {gate.Lane.Key} lane" })))));
    }

    public static Fin<DimFrame> Of(
        Plane plane, Context context, Option<Vector3d> horizontal = default) {
        return from domain in Optional(context).ToFin(Fail: new KernelFault.MissingContext())
               from frame in FactoryBridge.Accept<DimFrame>(
                   fault: Validate(plane, horizontal, domain.For(lane: ToleranceLane.Orientation), out DimFrame? admitted),
                   admitted: admitted)
               select frame;
    }

    internal Vector3d Reference => Horizontal.IfNone(noneValue: Plane.XAxis);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimensionSpec {
    private DimensionSpec() { }
    public sealed record Aligned(DimFrame Frame, SpanPoints Points) : DimensionSpec;
    public sealed record Rotated(DimFrame Frame, SpanPoints Points, double RotationRadians) : DimensionSpec;
    public sealed record AngularVertex(DimFrame Frame, VertexPoints Points) : DimensionSpec;
    public sealed record AngularSpread(DimFrame Frame, SpreadPoints Points) : DimensionSpec;
    public sealed record AngularLines(Line SideA, Point3d OnA, Line SideB, Point3d OnB, Point3d OnArc, AngularExtension Extension) : DimensionSpec;
    public sealed record AngularArc(Arc Value, double Offset) : DimensionSpec;
    public sealed record Radial(DimFrame Frame, RadialKind Kind, RadialPoints Points) : DimensionSpec;
    public sealed record Ordinate(DimFrame Frame, OrdinateAxis Axis, OrdinatePoints Points) : DimensionSpec;
    public sealed record MarkAt(DimFrame Frame, Point3d Center, double Radius) : DimensionSpec;
    public sealed record MarkOn(DimFrame Frame, GeometryHandle Source, double Parameter) : DimensionSpec;

    internal Fin<Dimension> Mint(DimensionStyle style) =>
        from _ in Admit()
        from minted in Try.lift(() => Switch(
                style,
                aligned: static (ctx, spec) => Fin.Succ<Dimension>(value: LinearDimension.Create(
                    dimtype: AnnotationType.Aligned, dimStyle: ctx, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    defpoint1: spec.Points.From, defpoint2: spec.Points.To, dimlinepoint: spec.Points.Line, rotationInPlane: 0.0)),
                rotated: static (ctx, spec) => Fin.Succ<Dimension>(value: LinearDimension.Create(
                    dimtype: AnnotationType.Rotated, dimStyle: ctx, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    defpoint1: spec.Points.From, defpoint2: spec.Points.To, dimlinepoint: spec.Points.Line,
                    rotationInPlane: spec.RotationRadians)),
                angularVertex: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    centerpoint: spec.Points.Center, defpoint1: spec.Points.Def1, defpoint2: spec.Points.Def2,
                    dimlinepoint: spec.Points.Line)),
                angularSpread: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                    extpoint1: spec.Points.Ext1, extpoint2: spec.Points.Ext2, dirpoint1: spec.Points.Dir1,
                    dirpoint2: spec.Points.Dir2, dimlinepoint: spec.Points.Line)),
                angularLines: static (ctx, spec) => Fin.Succ<Dimension>(value: AngularDimension.Create(
                    dimStyle: ctx, line1: spec.SideA, pointOnLine1: spec.OnA, line2: spec.SideB, pointOnLine2: spec.OnB,
                    pointOnAngularDimensionArc: spec.OnArc, bSetExtensionPoints: spec.Extension.Key)),
                angularArc: static (ctx, spec) => Fin.Succ<Dimension>(
                    value: new AngularDimension(arc: spec.Value, offset: spec.Offset) { ParentDimensionStyle = ctx }),
                radial: static (ctx, spec) => Fin.Succ<Dimension>(value: RadialDimension.Create(
                    dimStyle: ctx, dimtype: spec.Kind.Host, plane: spec.Frame.Plane,
                    centerpoint: spec.Points.Center, radiuspoint: spec.Points.RadiusPoint, dimlinepoint: spec.Points.Line)),
                ordinate: static (ctx, spec) => Fin.Succ<Dimension>(value: OrdinateDimension.Create(
                    dimStyle: ctx, plane: spec.Frame.Plane, direction: spec.Axis.Host,
                    basepoint: spec.Points.Base, defpoint: spec.Points.Def, leaderpoint: spec.Points.Leader,
                    kinkoffset1: spec.Points.Kink1, kinkoffset2: spec.Points.Kink2)),
                markAt: static (ctx, spec) => Fin.Succ<Dimension>(value: Centermark.Create(
                    dimStyle: ctx, plane: spec.Frame.Plane, centerPoint: spec.Center, radius: spec.Radius)),
                markOn: static (ctx, spec) => spec.Source.Typed<Curve, Dimension>(project: curve =>
                    from _ in guard(spec.Parameter >= curve.Domain.Min && spec.Parameter <= curve.Domain.Max,
                        new KernelFault.InvalidInput()).ToFin()
                    from mark in Fin.Succ<Dimension>(value: Centermark.Create(
                        dimStyle: ctx, plane: spec.Frame.Plane, curve: curve, curveParameter: spec.Parameter))
                    select mark))).Run().Bind(static inner => inner)
        select minted;

    private Fin<Unit> Admit() => Switch(aligned: static (key, spec) => spec.Points.Admit(),
        rotated: static (key, spec) => spec.Points.Admit().Bind(_ => Acceptance.Rows(spec.RotationRadians)).Map(static _ => unit),
        angularVertex: static (key, spec) => spec.Points.Admit(),
        angularSpread: static (key, spec) => spec.Points.Admit(),
        angularLines: static (key, spec) =>
            from lines in Acceptance.Rows(spec.SideA, spec.SideB)
            from points in Acceptance.Rows(spec.OnA, spec.OnB, spec.OnArc)
            select unit,
        angularArc: static (key, spec) =>
            from arc in Acceptance.Rows(spec.Value)
            from offset in Acceptance.Rows(spec.Offset)
            select unit,
        radial: static (key, spec) => spec.Points.Admit(),
        ordinate: static (key, spec) => spec.Points.Admit(),
        markAt: static (key, spec) => Acceptance.Rows(spec.Center).Bind(_ => Admit.Positive(value: spec.Radius)).Map(static _ => unit),
        markOn: static (key, spec) => Acceptance.Rows(spec.Parameter).Map(static _ => unit));
}
```

## [03]-[REFIT]

- Owner: `DimAdjust` refits each measuring kind through its native geometry contract and rejects a mismatched target before mutation, carrying the same point products the construction family names.
- Owner: `DimPose` carries dimension-only text placement, measurement scale, and detail binding through one generated aggregate gate whose six columns accumulate.
- Law: `DimAdjust.Linear` is the one arm carrying its own roster — the host refits a linear dimension from PLANE-space `Point2d` triples where every other family takes world points — and the divergence is the host member's, named here rather than smoothed over.
- Law: a pose gate reports every violated column, so a caller correcting a non-finite rotation is not then told its scale is non-positive; the two `is not null` re-checks the gate once carried are the boundary's job and the boundary already did it.
- Boundary: refit and pose act on the duplicate supplied by `TextOp.Reworked`, so a rejected native edit never mutates document-owned geometry.
- Packages: `Domain/validation` (`Acceptance.Rows`, `FactoryBridge.Accept`), `Document/session.md` (`DraftFault`); RhinoCommon `AdjustFromPoints`/`SetLocations` per `.api/api-rhinocommon-annotation.md`.
- Growth: a refit form is one `DimAdjust` case reading one point product; a pose column is one member and one clause.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAdjust {
    private DimAdjust() { }
    public sealed record Linear(Point2d Ext1End, Point2d Ext2End, Point2d OnDimLine) : DimAdjust;
    public sealed record AngularVertex(Plane Plane, VertexPoints Points) : DimAdjust;
    public sealed record AngularSpread(Plane Plane, SpreadPoints Points) : DimAdjust;
    public sealed record Radial(Plane Plane, RadialPoints Points, double RotationRadians) : DimAdjust;
    public sealed record Ordinate(Plane Plane, OrdinateAxis Axis, OrdinatePoints Points) : DimAdjust;
    public sealed record Mark(Plane Plane, Point3d Center) : DimAdjust;

    internal Fin<Unit> Apply(Dimension geometry) =>
        Admit().Bind(_ => Switch(
            geometry,
            linear: static (ctx, fit) =>
                from linear in Admit.Need(ctx as LinearDimension)
                from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(() => linear.SetLocations(
                    extensionLine1End: fit.Ext1End, extensionLine2End: fit.Ext2End, pointOnDimensionLine: fit.OnDimLine)))).Run().Bind(static inner => inner)
                select unit,
            angularVertex: static (ctx, fit) =>
                from angular in Admit.Need(ctx as AngularDimension)
                from _ in Admit.Confirm(success: angular.AdjustFromPoints(
                    plane: fit.Plane,
                    centerpoint: fit.Points.Center,
                    defpoint1: fit.Points.Def1,
                    defpoint2: fit.Points.Def2,
                    dimlinepoint: fit.Points.Line))
                select unit,
            angularSpread: static (ctx, fit) =>
                from angular in Admit.Need(ctx as AngularDimension)
                from _ in Admit.Confirm(success: angular.AdjustFromPoints(
                    plane: fit.Plane,
                    extpoint1: fit.Points.Ext1,
                    extpoint2: fit.Points.Ext2,
                    dirpoint1: fit.Points.Dir1,
                    dirpoint2: fit.Points.Dir2,
                    dimlinepoint: fit.Points.Line))
                select unit,
            radial: static (ctx, fit) =>
                from radial in Admit.Need(ctx as RadialDimension)
                from _ in Admit.Confirm(success: radial.AdjustFromPoints(
                    plane: fit.Plane,
                    centerpoint: fit.Points.Center,
                    radiuspoint: fit.Points.RadiusPoint,
                    dimlinepoint: fit.Points.Line,
                    rotationInPlane: fit.RotationRadians))
                select unit,
            ordinate: static (ctx, fit) =>
                from ordinate in Admit.Need(ctx as OrdinateDimension)
                from _ in Admit.Confirm(success: ordinate.AdjustFromPoints(
                    plane: fit.Plane,
                    direction: fit.Axis.Host,
                    basepoint: fit.Points.Base,
                    defpoint: fit.Points.Def,
                    leaderpoint: fit.Points.Leader,
                    kinkoffset1: fit.Points.Kink1,
                    kinkoffset2: fit.Points.Kink2))
                select unit,
            mark: static (ctx, fit) =>
                from mark in Admit.Need(ctx as Centermark)
                from _ in Admit.Confirm(success: mark.AdjustFromPoints(
                    plane: fit.Plane, centerPoint: fit.Center))
                select unit));

    private Fin<Unit> Admit() => Switch(linear: static (key, fit) => Acceptance.Rows(fit.Ext1End, fit.Ext2End, fit.OnDimLine).Map(static _ => unit),
        angularVertex: static (key, fit) => Acceptance.Rows(fit.Plane).Bind(_ => fit.Points.Admit()),
        angularSpread: static (key, fit) => Acceptance.Rows(fit.Plane).Bind(_ => fit.Points.Admit()),
        radial: static (key, fit) =>
            from plane in Acceptance.Rows(fit.Plane)
            from points in fit.Points.Admit()
            from rotation in Acceptance.Rows(fit.RotationRadians)
            select unit,
        ordinate: static (key, fit) => Acceptance.Rows(fit.Plane).Bind(_ => fit.Points.Admit()),
        mark: static (key, fit) => Acceptance.Rows(fit.Plane).Bind(_ => Acceptance.Rows(fit.Center)).Map(static _ => unit));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class DimPose {
    public Option<Point2d> TextPosition { get; }
    public Option<double> TextRotation { get; }
    public Option<TextPointMode> TextPoint { get; }
    public Option<string> PlainUserText { get; }
    public Option<DraftScale> DistanceScale { get; }
    public Option<DetailEdit> Detail { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Option<Point2d> textPosition, ref Option<double> textRotation,
        ref Option<TextPointMode> textPoint, ref Option<string> plainUserText, ref Option<DraftScale> distanceScale,
        ref Option<DetailEdit> detail) {
        (Option<Point2d> position, Option<double> rotation) = (textPosition, textRotation);
        bool empty = position.IsNone && rotation.IsNone && textPoint.IsNone
            && plainUserText.IsNone && distanceScale.IsNone && detail.IsNone;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (empty, static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(DimPose) }))),
            (position.Exists(static point => !point.IsValid), static () => new ValidationClause(string.Join(" | ", new object?[] { nameof(TextPosition) }))),
            (rotation.Exists(static value => !double.IsFinite(value)), () => new ValidationClause(string.Join(" | ", new object?[] { nameof(TextRotation), rotation.IfNone(double.NaN), "a finite radian rotation" })))));
    }

    public static Fin<DimPose> Of(
        Option<Point2d> textPosition = default, Option<double> textRotation = default,
        Option<TextPointMode> textPoint = default, Option<string> plainUserText = default,
        Option<DraftScale> distanceScale = default, Option<DetailEdit> detail = default) =>
        FactoryBridge.Accept<DimPose>(
            fault: Validate(textPosition, textRotation, textPoint, plainUserText, distanceScale, detail, out DimPose? admitted),
            admitted: admitted);

    internal Fin<Unit> Apply(Dimension geometry) =>
        from _ in Try.lift(() => Fin.Succ(value: HostEdge.Side(() => {
            TextPosition.Iter(position => geometry.TextPosition = position);
            TextRotation.Iter(rotation => geometry.TextRotation = rotation);
            TextPoint.Iter(mode => geometry.UseDefaultTextPoint = mode.Key);
            PlainUserText.Iter(text => geometry.PlainUserText = text);
            DistanceScale.Iter(scale => geometry.DistanceScale = scale.Value);
        }))).Run().Bind(static inner => inner)
        from __ in Detail.Traverse(edit => edit.Apply(dimension: geometry)).As()
        select unit;
}
```

## [04]-[MUTATION]

- Owner: `DimOp` is the complete dimension mutation program consumed by `Dimensions.Commit`.
- Law: per-annotation style changes ride the namespace's shared `AnnotationStyleOp` — overlay a `StylePatch` or clear every property override — so the dimension and text families carry one case each over one owner rather than two byte-identical pairs; dimension-specific state remains inside `DimPose`.
- Law: `LengthChannel` couples length-display selection with the matching zero-suppression reset as a row column — the page's rung-3 exemplar: the channel IS the behaviour, so no arm re-tests which of the two host members to call.
- Law: absence never crosses as `null` — an optional attribute set and the unused history slot both project through the kernel's one host-slot spelling, so the boundary states which argument the host reads as "use the document's own".
- Entry: `Dimensions.Commit` preserves the frozen wire and accepts the shared `DraftPlan<DimOp>` policy owner.
- Packages: `Annotation/style.md` (`AnnotationStyleOp`, `StylePatch`, `DraftPlan`, `DraftSpine`), `Document/tables.md` (`TableTarget`, `ResourceRef`), `Domain/results` (`HostEdge.Slot`, `Lease<T>`); RhinoCommon `ObjectTable.Add` per `.api/api-rhinocommon-document.md`.
- Growth: a dimension verb is one case with its arm; the spine and every consumer read it unchanged.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LengthChannel {
    public static readonly LengthChannel Primary = new(apply: static (dimension, display) =>
        HostEdge.Side(() => dimension.SetDimensionLengthDisplayWithZeroSuppressionReset(display)));
    public static readonly LengthChannel Alternate = new(apply: static (dimension, display) =>
        HostEdge.Side(() => dimension.SetAltDimensionLengthDisplayWithZeroSuppressionReset(display)));

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
    public sealed record Style(TableTarget Target, AnnotationStyleOp Edit) : DimOp;

    internal Fin<Unit> Apply(RhinoDoc document) => Switch(
        document,
        place: static (ctx, edit) =>
            from style in edit.Style.Resolve(document: ctx, lens: StyleOp.Lens)
            from minted in edit.Spec.Mint(style: style)
            from _ in new Lease<Dimension>.Owned(Value: minted).Use(owned =>
                from _ in edit.Overrides.Traverse(patch => patch.Overlay(annotation: owned).Map(static _ => unit)).As()
                from __ in Try.lift(() => ResourceId.Admit(ctx.Objects.Add(
                    geometry: owned,
                    attributes: HostEdge.Slot(edit.Attributes),
                    history: HostEdge.Slot(Option<HistoryRecord>.None),
                    reference: false))).Run().Bind(static inner => inner)
                select unit)
            select unit,
        adjust: static (ctx, edit) => Amended(ctx, edit.Target,
            (dimension, key) => edit.Fit.Apply(geometry: dimension)),
        repose: static (ctx, edit) => Amended(ctx, edit.Target,
            (dimension, key) => edit.Pose.Apply(geometry: dimension)),
        restate: static (ctx, edit) => Amended(ctx, edit.Target,
            (dimension, key) => Try.lift(() => Fin.Succ(value: HostEdge.Side(() => dimension.UpdateDimensionText(
                dimension.DimensionStyle,
                edit.Units.Map(static unit => unit.System).IfNone(ctx.ModelUnitSystem))))).Run().Bind(static inner => inner)),
        redisplay: static (ctx, edit) => Amended(ctx, edit.Target,
            (dimension, key) => Try.lift(() => Fin.Succ(value: edit.Channel.Apply(dimension, edit.Display.Host))).Run().Bind(static inner => inner)),
        style: static (ctx, edit) => Amended(ctx, edit.Target,
            (dimension, key) => edit.Edit.Apply(annotation: dimension)));

    private static Fin<Unit> Amended(
        RhinoDoc document,
        TableTarget target,
        Func<Dimension, Fin<Unit>> change) =>
        TextOp.Reworked(document: document, target: target,
            change: (annotation, key) => Admit.Need(annotation as Dimension)
                .Bind(dimension => change(dimension)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Dimensions {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<DimOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document));

    public static Fin<DimAnswer> Ask(DocumentSession session, DimAsk request) {
        return from admitted in Acceptance.Input(value: request)
               from answer in Admit.Demand(
                   use: document => admitted.Answer(document: document), needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[PROJECTION]

- Owner: `DimFamily` is the measuring-family row table — one row per host dimension class carrying its probe, its `DimKindFacts` read, its `DimSkeleton` read, and its formatted-value read, so a family is one row and NO reader re-spells the native type test; `DimKindFacts` stays the payload vocabulary the rows construct.
- Owner: `DimState` composes `TextState` instead of repeating annotation identity, frame, content, formatting, mask, and style fields, then adds the resolved family, dimension value, pose, effective setting rows, and per-kind facts.
- Owner: `DimPointRole` labels every constructor and display point, so arity never becomes positional consumer knowledge.
- Owner: `DimAsk` closes state, display, formatted value, viewport text transform, and exploded-piece custody under one request family; every scale it carries is the shared `DraftScale`, admitted at its own gate rather than re-guarded per arm.
- Law: the row table is TOTAL over the host hierarchy on every axis it publishes — points, display lines, and formatted text alike — so a family the host gives no reader for says so in its own column, and a `var unknown` catch-all beside the table re-spells the type test the table exists to delete.
- Law: a state read proves `AnnotationKind.Measures` before resolving a family, so a non-measuring annotation refuses at the kind rather than at a probe scan that reads like a missing implementation.
- Law: the read side names the text-point axis with the SAME row the write side does — an asymmetric raw `bool` on one half of one concept is the discarded discriminant, not a simpler shape.
- Boundary: exploded geometry crosses through `DraftCrossing.Crossed`, the namespace's one detach fold, and `DimAnswer.Pieces` owns the detached handles until the caller folds `Release`.
- Boundary: custody release rides the RESULT, not a disposer. A `void Dispose` swallows its cleanup fault or replaces a primary exception mid-unwind, so `DimAnswer.Release` returns `Fin<Unit>` and the accumulated release fault reaches the caller typed. NAMED LOSS: `using`-scopability of the answer — bought back by a fault a consumer reads, where the cell it used to park on had no reader anywhere; witness — the `pieces` arm folds kernel `Custody.Dispose` and every other case answers success.
- Packages: `Annotation/style.md` (`DraftCrossing`, `DraftScale`, `StyleField`, `StyleSetting`, `StyleValue`), `Domain/results` (`Custody`, `Try.lift`, `KernelFault.Unsupported`); RhinoCommon `Get3dPoints`/`GetDisplayLines`/`GetTextRectangle`/`GetAngleDisplayText`/`GetDistanceDisplayText`/`GetTextTransform`/`Explode` per `.api/api-rhinocommon-annotation.md`.
- Growth: a measuring family is one `DimFamily` row answering every column; a read is one `DimAsk` case with its `DimAnswer` twin.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

public readonly record struct DimPoint(DimPointRole Role, Point3d Value);

public sealed record DimSkeleton(
    Seq<DimPoint> Points,
    Seq<Line> Lines,
    Seq<Arc> Arcs,
    Arr<Point3d> TextBox) : IDetachedDocumentResult;

[SmartEnum<int>]
public sealed partial class DimFamily {
    public static readonly DimFamily Linear = new(
        key: 0,
        probe: static geometry => geometry is LinearDimension,
        facts: static (geometry, key) => Try.lift(() =>
            from linear in Admit.Need(geometry as LinearDimension)
            select (DimKindFacts)new DimKindFacts.Linear(linear.DistanceBetweenArrowTips)).Run().Bind(static inner => inner),
        skeleton: static (geometry, scale, key) =>
            from linear in Admit.Need(geometry as LinearDimension)
            from points in Try.lift(() => linear.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Extension1, a), new DimPoint(DimPointRole.Extension2, b),
                    new DimPoint(DimPointRole.Arrow1, c), new DimPoint(DimPointRole.Arrow2, d),
                    new DimPoint(DimPointRole.DimensionLine, e), new DimPoint(DimPointRole.Text, f)))
                : Fin.Fail<Seq<DimPoint>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from lines in Try.lift(() => linear.GetDisplayLines(linear.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from box in TextBox(linear.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        text: static (geometry, units, key) =>
            from linear in Admit.Need(geometry as LinearDimension)
            from value in Try.lift(() => Acceptance.Text(linear.GetDistanceDisplayText(units, linear.DimensionStyle))).Run().Bind(static inner => inner)
            select value);

    public static readonly DimFamily Angular = new(
        key: 1,
        probe: static geometry => geometry is AngularDimension,
        facts: static (geometry, key) => Try.lift(() =>
            from angular in Admit.Need(geometry as AngularDimension)
            select (DimKindFacts)new DimKindFacts.Angular(
                StyleValue.Of(angular.AngleFormat), angular.AngleResolution, angular.AngleRoundoff,
                StyleValue.Of(angular.AngleZeroSuppression))).Run().Bind(static inner => inner),
        skeleton: static (geometry, scale, key) =>
            from angular in Admit.Need(geometry as AngularDimension)
            from points in Try.lift(() => angular.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f, out Point3d g)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Definition1, b),
                    new DimPoint(DimPointRole.Definition2, c), new DimPoint(DimPointRole.DimensionLine, d),
                    new DimPoint(DimPointRole.Arrow1, e), new DimPoint(DimPointRole.Arrow2, f), new DimPoint(DimPointRole.Text, g)))
                : Fin.Fail<Seq<DimPoint>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from display in Try.lift(() => angular.GetDisplayLines(angular.DimensionStyle, scale.Value, out Line[] lines, out Arc[] arcs)
                ? Fin.Succ(value: (Lines: toSeq(lines), Arcs: toSeq(arcs)))
                : Fin.Fail<(Seq<Line> Lines, Seq<Arc> Arcs)>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from box in TextBox(angular.GetTextRectangle, key)
            select new DimSkeleton(points, display.Lines, display.Arcs, box),
        text: static (geometry, _, key) =>
            from angular in Admit.Need(geometry as AngularDimension)
            from value in Try.lift(() => Acceptance.Text(angular.GetAngleDisplayText(angular.DimensionStyle))).Run().Bind(static inner => inner)
            select value);

    public static readonly DimFamily Radial = new(
        key: 2,
        probe: static geometry => geometry is RadialDimension,
        facts: static (geometry, key) => Try.lift(() =>
            from radial in Admit.Need(geometry as RadialDimension)
            select (DimKindFacts)new DimKindFacts.Radial(
                StyleValue.Of(radial.LeaderTextHorizontalAlignment),
                StyleValue.Of(radial.LeaderArrowType),
                radial.LeaderArrowSize,
                ResourceId.Maybe(radial.LeaderArrowBlockId),
                StyleValue.Of(radial.LeaderCurveStyle))).Run().Bind(static inner => inner),
        skeleton: static (geometry, scale, key) =>
            from radial in Admit.Need(geometry as RadialDimension)
            from points in Try.lift(() => radial.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Center, a), new DimPoint(DimPointRole.Radius, b),
                    new DimPoint(DimPointRole.DimensionLine, c), new DimPoint(DimPointRole.Knee, d)))
                : Fin.Fail<Seq<DimPoint>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from lines in Try.lift(() => radial.GetDisplayLines(radial.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from box in TextBox(radial.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        text: static (geometry, units, key) =>
            from radial in Admit.Need(geometry as RadialDimension)
            from value in Try.lift(() => Acceptance.Text(radial.GetDistanceDisplayText(units, radial.DimensionStyle))).Run().Bind(static inner => inner)
            select value);

    public static readonly DimFamily Ordinate = new(
        key: 3,
        probe: static geometry => geometry is OrdinateDimension,
        facts: static (geometry, key) => Try.lift(() =>
            from ordinate in Admit.Need(geometry as OrdinateDimension)
            from axis in FactoryBridge.Accept<OrdinateAxis>(candidate: (int)ordinate.Direction)
            select (DimKindFacts)new DimKindFacts.Ordinate(axis, ordinate.KinkOffset1, ordinate.KinkOffset2)).Run().Bind(static inner => inner),
        skeleton: static (geometry, scale, key) =>
            from ordinate in Admit.Need(geometry as OrdinateDimension)
            from points in Try.lift(() => ordinate.Get3dPoints(
                    out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e)
                ? Fin.Succ(value: Seq(new DimPoint(DimPointRole.Definition1, a), new DimPoint(DimPointRole.Leader, b),
                    new DimPoint(DimPointRole.Kink1, c), new DimPoint(DimPointRole.Kink2, d), new DimPoint(DimPointRole.Text, e)))
                : Fin.Fail<Seq<DimPoint>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from lines in Try.lift(() => ordinate.GetDisplayLines(ordinate.DimensionStyle, scale.Value, out IEnumerable<Line> rows)
                ? Fin.Succ(value: toSeq(rows))
                : Fin.Fail<Seq<Line>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from box in TextBox(ordinate.GetTextRectangle, key)
            select new DimSkeleton(points, lines, Seq<Arc>(), box),
        text: static (geometry, units, key) =>
            from ordinate in Admit.Need(geometry as OrdinateDimension)
            from value in Try.lift(() => Acceptance.Text(ordinate.GetDistanceDisplayText(units, ordinate.DimensionStyle))).Run().Bind(static inner => inner)
            select value);

    public static readonly DimFamily Mark = new(
        key: 4,
        probe: static geometry => geometry is Centermark,
        facts: static (geometry, key) => Try.lift(() =>
            from mark in Admit.Need(geometry as Centermark)
            select (DimKindFacts)new DimKindFacts.Mark(mark.Radius)).Run().Bind(static inner => inner),
        skeleton: static (geometry, _, key) => Fin.Fail<DimSkeleton>(error: new KernelFault.Unsupported(
            valueType: geometry.GetType(), OutputType: typeof(DimSkeleton))),
        text: static (geometry, _, key) => Fin.Fail<string>(error: new KernelFault.Unsupported(
            valueType: geometry.GetType(), OutputType: typeof(string))));

    [UseDelegateFromConstructor]
    internal partial bool Probe(Dimension geometry);

    [UseDelegateFromConstructor]
    internal partial Fin<DimKindFacts> Facts(Dimension geometry);

    [UseDelegateFromConstructor]
    internal partial Fin<DimSkeleton> Skeleton(Dimension geometry, DraftScale scale);

    [UseDelegateFromConstructor]
    internal partial Fin<string> Text(Dimension geometry, UnitSystem units);

    internal static Fin<DimFamily> Of(Dimension geometry) =>
        toSeq(Items).Find(row => Admit.Probe(geometry: geometry))
            .ToFin(Fail: new KernelFault.Unsupported(valueType: geometry.GetType(), OutputType: typeof(DimFamily)));

    private delegate bool TextRectProbe(out Point3d[] corners);

    private static Fin<Arr<Point3d>> TextBox(TextRectProbe probe) => Try.lift(() =>
        probe(out Point3d[] corners) ? Fin.Succ(value: toArray(corners)) : Fin.Fail<Arr<Point3d>>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DimState(
    TextState Annotation,
    AnnotationKind Kind,
    DimFamily Family,
    double NumericValue,
    Option<string> PlainUserText,
    Point2d TextPosition,
    double TextRotation,
    TextPointMode TextPoint,
    Option<ResourceId> DetailMeasured,
    DraftScale DistanceScale,
    double DimensionScale,
    double StyleScaleValue,
    Seq<StyleSetting> EffectiveStyle,
    DimKindFacts Facts) : IDetachedDocumentResult;

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAsk {
    private DimAsk() { }
    public sealed record State(TableTarget Target) : DimAsk;
    public sealed record Skeleton(TableTarget Target, DraftScale Scale) : DimAsk;
    public sealed record ValueText(TableTarget Target, Option<ModelUnit> Units = default) : DimAsk;
    public sealed record TextTransform(TableTarget Target, ViewportInfo Viewport, DraftScale Scale, TextFacing Facing) : DimAsk;
    public sealed record Pieces(TableTarget Target) : DimAsk;

    internal Fin<DimAnswer> Answer(RhinoDoc document) => Switch(
        document,
        state: static (ctx, ask) =>
            from dimension in Resolved(ctx, ask.Target)
            from annotation in TextState.Of(native: dimension.Native)
            from kind in FactoryBridge.Accept<AnnotationKind>(candidate: (int)dimension.Geometry.AnnotationType)
            from _ in guard(kind.Measures, new KernelFault.InvalidResult(Detail: Some(kind.Key.ToString(CultureInfo.InvariantCulture))))
            from family in DimFamily.Of(geometry: dimension.Geometry)
            from facts in family.Facts(geometry: dimension.Geometry)
            from mode in FactoryBridge.Accept<TextPointMode>(candidate: dimension.Geometry.UseDefaultTextPoint)
            from scale in FactoryBridge.Accept<DraftScale>(candidate: dimension.Geometry.DistanceScale)
            from settings in toSeq(StyleField.Items)
                .TraverseM(field => field.Read(style: dimension.Geometry.DimensionStyle)
                    .Map(value => new StyleSetting(Field: field, Value: value)))
                .As()
            select (DimAnswer)new DimAnswer.State(new DimState(
                annotation, kind, family, dimension.Geometry.NumericValue,
                HostEdge.Text(dimension.Geometry.PlainUserText),
                dimension.Geometry.TextPosition, dimension.Geometry.TextRotation, mode,
                ResourceId.Maybe(dimension.Geometry.DetailMeasured),
                scale, dimension.Geometry.DimensionScale,
                dimension.Geometry.DimensionStyle.DimensionScaleValue, settings, facts)),
        skeleton: static (ctx, ask) =>
            from dimension in Resolved(ctx, ask.Target)
            from family in DimFamily.Of(geometry: dimension.Geometry)
            from skeleton in family.Skeleton(geometry: dimension.Geometry, scale: ask.Scale)
            select (DimAnswer)new DimAnswer.Skeleton(skeleton),
        valueText: static (ctx, ask) =>
            from dimension in Resolved(ctx, ask.Target)
            from family in DimFamily.Of(geometry: dimension.Geometry)
            from text in family.Text(
                geometry: dimension.Geometry,
                units: ask.Units.Map(static unit => unit.System).IfNone(ctx.ModelUnitSystem))
            select (DimAnswer)new DimAnswer.Formatted(text),
        textTransform: static (ctx, ask) =>
            from dimension in Resolved(ctx, ask.Target)
            from transform in Try.lift(() => Fin.Succ(value: dimension.Geometry.GetTextTransform(
                viewport: ask.Viewport, style: dimension.Geometry.DimensionStyle,
                textScale: ask.Scale.Value, drawForward: ask.Facing.Key))).Run().Bind(static inner => inner)
            select (DimAnswer)new DimAnswer.Transformed(transform),
        pieces: static (ctx, ask) =>
            from dimension in Resolved(ctx, ask.Target)
            from products in Try.lift(() => Optional(dimension.Geometry.Explode())
                .Map(static values => toSeq(values)).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            from handles in DraftCrossing.Crossed(products: products)
            select (DimAnswer)new DimAnswer.Pieces(handles));

    private static Fin<(AnnotationObjectBase Native, Dimension Geometry)> Resolved(
        RhinoDoc document,
        TableTarget target) =>
        from annotation in TextAsk.Single(document: document, target: target)
        from dimension in Admit.Need(annotation.AnnotationGeometry as Dimension)
        select (annotation, dimension);
}

[Union(MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAnswer : IDetachedDocumentResult {
    private DimAnswer() { }
    public sealed record State(DimState Snapshot) : DimAnswer;
    public sealed record Skeleton(DimSkeleton Value) : DimAnswer;
    public sealed record Formatted(string Text) : DimAnswer;
    public sealed record Transformed(Transform Value) : DimAnswer;
    public sealed record Pieces(Seq<GeometryHandle> Products) : DimAnswer;

    public Fin<Unit> Release() => SwitchPartially(
        @default: static _ => Fin.Succ(value: unit),
        pieces: static (row) => Custody.Dispose(held: row.Products));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
