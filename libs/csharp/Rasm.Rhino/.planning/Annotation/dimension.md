# [RASM_RHINO_ANNOTATION_DIMENSION]

Dimension rail (`Rasm.Rhino.Annotation`). One `DimensionSpec` union carries ten cases spanning eleven host construction forms: aligned and rotated linear, four angular forms, parameterized radial radius/diameter, ordinate, and two centermark seeds. Each case owns only the frame and payload its host constructor consumes; line-pair and arc-offset angular cases carry no discarded `DimFrame`. `DimAdjust` owns per-kind re-fit, and `DimAsk` resolves definition points, display lines/arcs, text rectangles where the host exposes them, formatted value text, and complete catalogued per-kind facts. Per-instance restyling composes `StylePatch.Overlay`; dimension-only pose remains `DimPose`. `ForceText` stays absent because the host marks it obsolete and text fit routes through `TextFit`/`ArrowFit`.

## [01]-[INDEX]

- [02]-[CONSTRUCTION]: `RadialKind`, `OrdinateAxis`, `DimFrame`, and the `DimensionSpec` union with its `Mint` dispatch.
- [03]-[ADJUST]: the `DimAdjust` re-fit union over the per-kind `AdjustFromPoints`/`SetLocations` members.
- [04]-[DIMENSION_RAIL]: `DimPose`, `DimOp`, `DimTransaction`, and the `Dimensions` entry pair.
- [05]-[ASK_FAMILY]: `DimAsk`/`DimAnswer` — state with per-kind facts, display skeleton, value text, and explosion.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[CONSTRUCTION]

- Owner: `RadialKind` `[SmartEnum<int>]` — radius versus diameter keyed on explicit `AnnotationType` byte values; `OrdinateAxis` `[SmartEnum<int>]` — measured direction keyed on `MeasuredDirection`; `DimFrame` — a dimension plane with horizontal reference defaulting to its x-axis; `DimensionSpec` `[Union]` — ten cases carrying only constructor-consumed payload, with the radial case generating its two host forms.
- Law: the spec discriminates the host construction form — a caller states measurement intent and never selects among the three style-object `AngularDimension.Create` shapes, the arc-offset constructor, or the two `Centermark.Create` shapes; the duplicate angular overload taking a style id collapses because `ResourceRef` already resolves the style object.
- Law: the arc-offset case binds its style as parent — every `Create` sets `ParentDimensionStyle` on the minted geometry, so the constructor-built arc form assigns the same member; `SetOverrideDimStyle` demands a nil-id marked override style and refuses a table style, so it never carries base-style binding.
- Law: `AnnotationType` keys on explicit byte values `0..11` — `Angular3pt = 11` is the three-point angular the vertex case constructs and `Angular = 2` the extension-point form, so the two angular display behaviors stay distinct cases, never one case with a flag.
- Law: minted geometry is detached — `Mint` answers a `Dimension` no document owns; placement is the rail's `Place` verb inside the shared spine, and a spec never touches a table.
- Growth: a new host construction form is one case with its arm; `Mint`, the rail, and every consumer read it with zero new surface.

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

public readonly record struct DimFrame(Plane Plane, Option<Vector3d> Horizontal) {
    public static Fin<DimFrame> Of(Plane plane, Option<Vector3d> horizontal = default, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(plane.IsValid, op.InvalidInput()).ToFin()
               from __ in horizontal.Match(Some: axis => op.AcceptInput(value: axis).Map(static _ => unit), None: () => Fin.Succ(value: unit))
               select new DimFrame(Plane: plane, Horizontal: horizontal);
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
    public sealed record AngularLines(Line SideA, Point3d OnA, Line SideB, Point3d OnB, Point3d OnArc, bool SetExtensionPoints = true) : DimensionSpec;
    public sealed record AngularArc(Arc Value, double Offset) : DimensionSpec;
    public sealed record Radial(DimFrame Frame, RadialKind Kind, Point3d Center, Point3d RadiusPoint, Point3d Line) : DimensionSpec;
    public sealed record Ordinate(DimFrame Frame, OrdinateAxis Axis, Point3d Base, Point3d Def, Point3d Leader, double Kink1, double Kink2) : DimensionSpec;
    public sealed record MarkAt(DimFrame Frame, Point3d Center, double Radius) : DimensionSpec;
    public sealed record MarkOn(DimFrame Frame, Curve Source, double Parameter) : DimensionSpec;

    internal Fin<Dimension> Mint(DimensionStyle style, Op op) =>
        Switch(
            (Style: style, Op: op),
            aligned: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: LinearDimension.Create(
                dimtype: AnnotationType.Aligned, dimStyle: context.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                defpoint1: spec.From, defpoint2: spec.To, dimlinepoint: spec.Line, rotationInPlane: 0.0))),
            rotated: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: LinearDimension.Create(
                dimtype: AnnotationType.Rotated, dimStyle: context.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                defpoint1: spec.From, defpoint2: spec.To, dimlinepoint: spec.Line, rotationInPlane: spec.RotationRadians))),
            angularVertex: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: AngularDimension.Create(
                dimStyle: context.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                centerpoint: spec.Center, defpoint1: spec.Def1, defpoint2: spec.Def2, dimlinepoint: spec.Line))),
            angularSpread: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: AngularDimension.Create(
                dimStyle: context.Style, plane: spec.Frame.Plane, horizontal: spec.Frame.Reference,
                extpoint1: spec.Ext1, extpoint2: spec.Ext2, dirpoint1: spec.Dir1, dirpoint2: spec.Dir2, dimlinepoint: spec.Line))),
            angularLines: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: AngularDimension.Create(
                dimStyle: context.Style, line1: spec.SideA, pointOnLine1: spec.OnA, line2: spec.SideB, pointOnLine2: spec.OnB,
                pointOnAngularDimensionArc: spec.OnArc, bSetExtensionPoints: spec.SetExtensionPoints))),
            angularArc: static (context, spec) => context.Op.Catch(() => {
                AngularDimension built = new(arc: spec.Value, offset: spec.Offset);
                built.ParentDimensionStyle = context.Style;
                return Fin.Succ<Dimension>(value: built);
            }),
            radial: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: RadialDimension.Create(
                dimStyle: context.Style, dimtype: spec.Kind.Host, plane: spec.Frame.Plane,
                centerpoint: spec.Center, radiuspoint: spec.RadiusPoint, dimlinepoint: spec.Line))),
            ordinate: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: OrdinateDimension.Create(
                dimStyle: context.Style, plane: spec.Frame.Plane, direction: spec.Axis.Host,
                basepoint: spec.Base, defpoint: spec.Def, leaderpoint: spec.Leader,
                kinkoffset1: spec.Kink1, kinkoffset2: spec.Kink2))),
            markAt: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: Centermark.Create(
                dimStyle: context.Style, plane: spec.Frame.Plane, centerPoint: spec.Center, radius: spec.Radius))),
            markOn: static (context, spec) => context.Op.Catch(() => Fin.Succ<Dimension>(value: Centermark.Create(
                dimStyle: context.Style, plane: spec.Frame.Plane, curve: spec.Source, curveParameter: spec.Parameter))));
}
```

## [03]-[ADJUST]

- Owner: `DimAdjust` `[Union]` — the per-kind re-fit vocabulary: linear definition-point relocation through `SetLocations`, the angular vertex and extension-point `AdjustFromPoints` pair, the radial re-fit with in-plane rotation, the ordinate re-fit, and the centermark recenter.
- Law: an adjust case demands its kind — the arm casts the live geometry to its subtype and a mismatched target is a typed refusal, so a linear re-fit can never silently no-op against an angular dimension.
- Law: the re-fit runs on the duplicate inside the amend kernel — the host `AdjustFromPoints` members mutate in place and return `bool`, so `Confirm` gates the `Replace` and a refused fit leaves the document untouched.

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
        Switch(
            (Geometry: geometry, Op: op),
            linear: static (context, fit) =>
                from linear in Optional(context.Geometry as LinearDimension).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Catch(() => {
                    linear.SetLocations(extensionLine1End: fit.Ext1End, extensionLine2End: fit.Ext2End, pointOnDimensionLine: fit.OnDimLine);
                    return Fin.Succ(value: unit);
                })
                select unit,
            angularVertex: static (context, fit) =>
                from angular in Optional(context.Geometry as AngularDimension).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: angular.AdjustFromPoints(
                    plane: fit.Plane, centerpoint: fit.Center, defpoint1: fit.Def1, defpoint2: fit.Def2, dimlinepoint: fit.Line))
                select unit,
            angularSpread: static (context, fit) =>
                from angular in Optional(context.Geometry as AngularDimension).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: angular.AdjustFromPoints(
                    plane: fit.Plane, extpoint1: fit.Ext1, extpoint2: fit.Ext2, dirpoint1: fit.Dir1, dirpoint2: fit.Dir2, dimlinepoint: fit.Line))
                select unit,
            radial: static (context, fit) =>
                from radial in Optional(context.Geometry as RadialDimension).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: radial.AdjustFromPoints(
                    plane: fit.Plane, centerpoint: fit.Center, radiuspoint: fit.RadiusPoint, dimlinepoint: fit.Line, rotationInPlane: fit.RotationRadians))
                select unit,
            ordinate: static (context, fit) =>
                from ordinate in Optional(context.Geometry as OrdinateDimension).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: ordinate.AdjustFromPoints(
                    plane: fit.Plane, direction: fit.Axis.Host, basepoint: fit.Base, defpoint: fit.Def, leaderpoint: fit.Leader,
                    kinkoffset1: fit.Kink1, kinkoffset2: fit.Kink2))
                select unit,
            mark: static (context, fit) =>
                from mark in Optional(context.Geometry as Centermark).ToFin(Fail: context.Op.InvalidInput())
                from _ in context.Op.Confirm(success: mark.AdjustFromPoints(plane: fit.Plane, centerPoint: fit.Center))
                select unit);
}
```

## [04]-[DIMENSION_RAIL]

- Owner: `DimPose` — the dimension-only instance state one payload carries: text position and rotation, the default-text-point grant, plain user text, distance scale, and the measured-detail binding; `DimOp` `[Union]` — placement, re-fit, pose, text recomputation, length-display swap, and the restyle/unstyle pair over the style algebra; `DimTransaction` — the commit plan; `Dimensions` — the `Commit`/`Ask` entry pair.
- Law: per-instance styling rides `StylePatch.Overlay` — the ~52 flat override properties on `Dimension` are host convenience faces over the override style, so one field algebra serves style authoring, text annotations, and dimensions; a `DimPose` field duplicating a `StyleField` row is the deleted form.
- Law: `Redisplay` couples display and suppression — `SetDimensionLengthDisplayWithZeroSuppressionReset` (and its alternate sibling) resets zero suppression coherently with the display swap, so a length-display change never lands as two edits that can half-apply.
- Law: `Restate` recomputes after a regime change — `UpdateDimensionText(style, units)` runs when the style or unit regime moved under a placed dimension, its unit defaulting to the document model unit and overridable per call.
- Law: every mutation walks the duplicate-then-`Replace` kernel this namespace shares — `TextOp.Reworked` narrows to `Dimension` per verb, so a dimension verb aimed at a text annotation is a typed refusal.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DimPose(
    Option<Point2d> TextPosition = default,
    Option<double> TextRotation = default,
    Option<bool> UseDefaultTextPoint = default,
    Option<string> PlainUserText = default,
    Option<double> DistanceScale = default,
    Option<Guid> DetailMeasured = default) {
    internal Fin<Unit> Apply(Dimension geometry, Op key) =>
        from _ in guard(
            TextPosition.IsSome || TextRotation.IsSome || UseDefaultTextPoint.IsSome ||
            PlainUserText.IsSome || DistanceScale.IsSome || DetailMeasured.IsSome,
            key.InvalidInput()).ToFin()
        from __ in TextPosition.Match(
            Some: position => key.AcceptInput(value: position).Map(static _ => unit),
            None: () => Fin.Succ(value: unit))
        from ___ in TextRotation.Match(
            Some: rotation => key.AcceptInput(value: rotation).Map(static _ => unit),
            None: () => Fin.Succ(value: unit))
        from ____ in DistanceScale.Match(
            Some: scale => key.Positive(value: scale).Map(static _ => unit),
            None: () => Fin.Succ(value: unit))
        from _____ in DetailMeasured.Match(
            Some: detail => guard(detail != Guid.Empty, key.InvalidInput()).ToFin(),
            None: () => Fin.Succ(value: unit))
        from applied in key.Catch(() => {
            _ = TextPosition.Iter(at => geometry.TextPosition = at);
            _ = TextRotation.Iter(angle => geometry.TextRotation = angle);
            _ = UseDefaultTextPoint.Iter(grant => geometry.UseDefaultTextPoint = grant);
            _ = PlainUserText.Iter(text => geometry.PlainUserText = text);
            _ = DistanceScale.Iter(scale => geometry.DistanceScale = scale);
            _ = DetailMeasured.Iter(detail => geometry.DetailMeasured = detail);
            return Fin.Succ(value: unit);
        })
        select applied;
}

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LengthChannel {
    public static readonly LengthChannel Primary = new(
        apply: static (dimension, display) => {
            dimension.SetDimensionLengthDisplayWithZeroSuppressionReset(ld: display);
            return unit;
        });
    public static readonly LengthChannel Alternate = new(
        apply: static (dimension, display) => {
            dimension.SetAltDimensionLengthDisplayWithZeroSuppressionReset(ld: display);
            return unit;
        });

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

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            place: static (context, edit) =>
                from style in edit.Style.Resolve(document: context.Document, lens: StyleOp.Lens, key: context.Op)
                from minted in edit.Spec.Mint(style: style, op: context.Op)
                from _ in edit.Overrides.Match(
                    Some: patch => patch.Overlay(annotation: minted, key: context.Op).Map(static _ => unit),
                    None: () => Fin.Succ(value: unit))
                from id in context.Op.Catch(() => context.Document.Objects.Add(
                        geometry: minted,
                        attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                        history: null,
                        reference: false) is var added && added != Guid.Empty
                    ? Fin.Succ(value: added)
                    : Fin.Fail<Guid>(error: context.Op.InvalidResult()))
                select DraftReceipt.Objects(slot: DraftSlot.Placed, ids: Seq(id)),
            adjust: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Adjusted,
                    change: (dimension, key) => edit.Fit.Apply(geometry: dimension, op: key)),
            repose: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Adjusted,
                    change: (dimension, key) => edit.Pose.Apply(geometry: dimension, key: key)),
            restate: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Reformulated,
                    change: (dimension, key) => key.Catch(() => {
                        dimension.UpdateDimensionText(
                            style: dimension.DimensionStyle,
                            units: edit.Units.IfNone(noneValue: context.Document.ModelUnitSystem));
                        return Fin.Succ(value: unit);
                    })),
            redisplay: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: (dimension, key) =>
                        from channel in Optional(edit.Channel).ToFin(Fail: key.InvalidInput())
                        from display in Optional(edit.Display).ToFin(Fail: key.InvalidInput())
                        from applied in key.Catch(() => Fin.Succ(value: channel.Apply(dimension: dimension, display: display.Host)))
                        select applied),
            restyle: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: (dimension, key) => edit.Patch.Overlay(annotation: dimension, key: key).Map(static _ => unit)),
            unstyle: static (context, edit) =>
                Amended(document: context.Document, target: edit.Target, op: context.Op, slot: DraftSlot.Restyled,
                    change: static (dimension, key) => key.Confirm(success: dimension.ClearPropertyOverrides())));

    private static Fin<DraftReceipt> Amended(
        RhinoDoc document, TableTarget target, Op op, DraftSlot slot,
        Func<Dimension, Op, Fin<Unit>> change) =>
        TextOp.Reworked(document: document, target: target, op: op, slot: slot,
            change: (annotation, key) => Optional(annotation as Dimension).ToFin(Fail: key.InvalidInput())
                .Bind(dimension => change(dimension, key)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DimTransaction(string Name, Seq<DimOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static DimTransaction Batch(string name, params ReadOnlySpan<DimOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}
```

## [05]-[ASK_FAMILY]

- Owner: `DimKindFacts` `[Union]` — the per-kind evidence: the linear aligned discriminant and arrow-tip span, the angular format quartet, the radial diameter discriminant and leader quintet presence, the ordinate axis and kink offsets, the centermark radius; `DimState` — the one-pass dimension read: kind, measured value, user text and field-token text, style binding, override presence, pose, and the kind facts; `DimSkeleton` — the display resolution: definition points, display lines, display arcs, and the text rectangle; `DimAsk`/`DimAnswer` — the typed request/result pairs including formatted value text and explosion.
- Law: display resolution is per-kind but one answer — `Get3dPoints` arities differ (six linear, seven angular, four radial, five ordinate), `GetDisplayLines` splits lines-only from the angular lines-plus-arcs form, and `GetTextRectangle` answers the text frame on every measuring kind; the skeleton folds all three into one shape so a consumer never learns the arity table, and `Centermark` carries none of the three, so a mark skeleton is a typed refusal.
- Law: value text is the host formatter — `GetDistanceDisplayText(units, style)` for measuring kinds and `GetAngleDisplayText(style)` for angular, so formatted output honors resolution, suppression, prefix, and suffix without a local re-derivation.
- Law: explosion detaches — `Explode()` answers constituent curve and text geometry the caller owns; nothing document-bound rides the answer.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimKindFacts {
    private DimKindFacts() { }
    public sealed record LinearFacts(bool Aligned, double ArrowTipSpan) : DimKindFacts;
    public sealed record AngularFacts(
        DimensionStyle.AngleDisplayFormat Format,
        int Resolution,
        double Roundoff,
        DimensionStyle.ZeroSuppression ZeroSuppression) : DimKindFacts;
    public sealed record RadialFacts(
        bool IsDiameter,
        TextHorizontalAlignment TextAlignment,
        DimensionStyle.ArrowType ArrowType,
        double ArrowSize,
        Guid ArrowBlockId,
        DimensionStyle.LeaderCurveStyle CurveStyle) : DimKindFacts;
    public sealed record OrdinateFacts(OrdinateDimension.MeasuredDirection Axis, double Kink1, double Kink2) : DimKindFacts;
    public sealed record MarkFacts(double Radius) : DimKindFacts;

    internal static Fin<DimKindFacts> Of(Dimension geometry, Op key) =>
        key.Catch(() => geometry switch {
            LinearDimension linear => Fin.Succ<DimKindFacts>(value: new LinearFacts(
                Aligned: linear.Aligned, ArrowTipSpan: linear.DistanceBetweenArrowTips)),
            AngularDimension angular => Fin.Succ<DimKindFacts>(value: new AngularFacts(
                Format: angular.AngleFormat,
                Resolution: angular.AngleResolution,
                Roundoff: angular.AngleRoundoff,
                ZeroSuppression: angular.AngleZeroSuppression)),
            RadialDimension radial => Fin.Succ<DimKindFacts>(value: new RadialFacts(
                IsDiameter: radial.IsDiameterDimension,
                TextAlignment: radial.LeaderTextHorizontalAlignment,
                ArrowType: radial.LeaderArrowType,
                ArrowSize: radial.LeaderArrowSize,
                ArrowBlockId: radial.LeaderArrowBlockId,
                CurveStyle: radial.LeaderCurveStyle)),
            OrdinateDimension ordinate => Fin.Succ<DimKindFacts>(value: new OrdinateFacts(
                Axis: ordinate.Direction, Kink1: ordinate.KinkOffset1, Kink2: ordinate.KinkOffset2)),
            Centermark mark => Fin.Succ<DimKindFacts>(value: new MarkFacts(Radius: mark.Radius)),
            var unmapped => Fin.Fail<DimKindFacts>(error: key.Unsupported(geometryType: unmapped.GetType(), outputType: typeof(DimKindFacts))),
        });
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DimState(
    Guid Key,
    AnnotationType Kind,
    double NumericValue,
    Option<string> PlainUserText,
    Option<string> TextWithFields,
    Guid StyleId,
    bool HasPropertyOverrides,
    Point2d TextPosition,
    double TextRotation,
    bool UseDefaultTextPoint,
    Option<Guid> DetailMeasured,
    double DistanceScale,
    DimKindFacts Facts) : IDetachedDocumentResult;

public sealed record DimSkeleton(
    Seq<Point3d> Points,
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
    public sealed record Pieces(TableTarget Target) : DimAsk;

    internal Fin<DimAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            state: static (ctx, ask) =>
                from dimension in Resolved(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from facts in DimKindFacts.Of(geometry: dimension.Geometry, key: ctx.Op)
                select (DimAnswer)new DimAnswer.State(Snapshot: new DimState(
                    Key: dimension.Id,
                    Kind: dimension.Geometry.AnnotationType,
                    NumericValue: dimension.Geometry.NumericValue,
                    PlainUserText: Optional(dimension.Geometry.PlainUserText).Filter(static text => text.Length > 0),
                    TextWithFields: Optional(dimension.Geometry.PlainTextWithFields).Filter(static text => text.Length > 0),
                    StyleId: dimension.Geometry.DimensionStyleId,
                    HasPropertyOverrides: dimension.Geometry.HasPropertyOverrides,
                    TextPosition: dimension.Geometry.TextPosition,
                    TextRotation: dimension.Geometry.TextRotation,
                    UseDefaultTextPoint: dimension.Geometry.UseDefaultTextPoint,
                    DetailMeasured: Optional(dimension.Geometry.DetailMeasured).Filter(static detail => detail != Guid.Empty),
                    DistanceScale: dimension.Geometry.DistanceScale,
                    Facts: facts)),
            skeleton: static (ctx, ask) =>
                from dimension in Resolved(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from resolved in Skeletal(geometry: dimension.Geometry, scale: ask.Scale, key: ctx.Op)
                select (DimAnswer)new DimAnswer.Resolved(Skeleton: resolved),
            valueText: static (ctx, ask) =>
                from dimension in Resolved(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from text in ctx.Op.Catch(() => dimension.Geometry switch {
                    AngularDimension angular => ctx.Op.AcceptText(value: angular.GetAngleDisplayText(dimStyle: angular.DimensionStyle)),
                    LinearDimension linear => ctx.Op.AcceptText(value: linear.GetDistanceDisplayText(
                        units: ask.Units.IfNone(noneValue: ctx.Document.ModelUnitSystem), dimStyle: linear.DimensionStyle)),
                    RadialDimension radial => ctx.Op.AcceptText(value: radial.GetDistanceDisplayText(
                        units: ask.Units.IfNone(noneValue: ctx.Document.ModelUnitSystem), dimStyle: radial.DimensionStyle)),
                    OrdinateDimension ordinate => ctx.Op.AcceptText(value: ordinate.GetDistanceDisplayText(
                        units: ask.Units.IfNone(noneValue: ctx.Document.ModelUnitSystem), dimStyle: ordinate.DimensionStyle)),
                    var unmapped => Fin.Fail<string>(error: ctx.Op.Unsupported(geometryType: unmapped.GetType(), outputType: typeof(string))),
                })
                select (DimAnswer)new DimAnswer.Formatted(Text: text),
            pieces: static (ctx, ask) =>
                from dimension in Resolved(document: ctx.Document, target: ask.Target, key: ctx.Op)
                from products in ctx.Op.Catch(() => Optional(dimension.Geometry.Explode())
                    .Map(static pieces => toSeq(pieces))
                    .ToFin(Fail: ctx.Op.InvalidResult()))
                select (DimAnswer)new DimAnswer.Pieces(Products: products));

    private static Fin<(Guid Id, Dimension Geometry)> Resolved(RhinoDoc document, TableTarget target, Op key) =>
        from annotation in TextAsk.Single(document: document, target: target, key: key)
        from dimension in Optional(annotation.AnnotationGeometry as Dimension).ToFin(Fail: key.InvalidInput())
        select (annotation.Id, dimension);

    private static Fin<DimSkeleton> Skeletal(Dimension geometry, double scale, Op key) =>
        from factor in key.Positive(value: scale)
        from skeleton in geometry switch {
            LinearDimension linear => Linear(linear, factor, key),
            AngularDimension angular => Angular(angular, factor, key),
            RadialDimension radial => Radial(radial, factor, key),
            OrdinateDimension ordinate => Ordinate(ordinate, factor, key),
            var unmapped => Fin.Fail<DimSkeleton>(error: key.Unsupported(geometryType: unmapped.GetType(), outputType: typeof(DimSkeleton))),
        }
        select skeleton;

    private static Fin<DimSkeleton> Linear(LinearDimension geometry, double scale, Op key) =>
        from points in key.Catch(() => geometry.Get3dPoints(
                out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f)
            ? Fin.Succ(value: Seq(a, b, c, d, e, f))
            : Fin.Fail<Seq<Point3d>>(error: key.InvalidResult()))
        from lines in key.Catch(() => geometry.GetDisplayLines(
                style: geometry.DimensionStyle, scale: scale, lines: out IEnumerable<Line> resolved)
            ? Fin.Succ(value: toSeq(resolved))
            : Fin.Fail<Seq<Line>>(error: key.InvalidResult()))
        from box in TextBox(geometry.GetTextRectangle, key)
        select new DimSkeleton(Points: points, Lines: lines, Arcs: Seq<Arc>(), TextBox: box);

    private static Fin<DimSkeleton> Angular(AngularDimension geometry, double scale, Op key) =>
        from points in key.Catch(() => geometry.Get3dPoints(
                out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e, out Point3d f, out Point3d g)
            ? Fin.Succ(value: Seq(a, b, c, d, e, f, g))
            : Fin.Fail<Seq<Point3d>>(error: key.InvalidResult()))
        from display in key.Catch(() => geometry.GetDisplayLines(
                style: geometry.DimensionStyle, scale: scale, lines: out Line[] lines, arcs: out Arc[] arcs)
            ? Fin.Succ(value: (Lines: toSeq(lines), Arcs: toSeq(arcs)))
            : Fin.Fail<(Seq<Line> Lines, Seq<Arc> Arcs)>(error: key.InvalidResult()))
        from box in TextBox(geometry.GetTextRectangle, key)
        select new DimSkeleton(Points: points, Lines: display.Lines, Arcs: display.Arcs, TextBox: box);

    private static Fin<DimSkeleton> Radial(RadialDimension geometry, double scale, Op key) =>
        from points in key.Catch(() => geometry.Get3dPoints(out Point3d a, out Point3d b, out Point3d c, out Point3d d)
            ? Fin.Succ(value: Seq(a, b, c, d))
            : Fin.Fail<Seq<Point3d>>(error: key.InvalidResult()))
        from lines in key.Catch(() => geometry.GetDisplayLines(
                style: geometry.DimensionStyle, scale: scale, lines: out IEnumerable<Line> resolved)
            ? Fin.Succ(value: toSeq(resolved))
            : Fin.Fail<Seq<Line>>(error: key.InvalidResult()))
        from box in TextBox(geometry.GetTextRectangle, key)
        select new DimSkeleton(Points: points, Lines: lines, Arcs: Seq<Arc>(), TextBox: box);

    private static Fin<DimSkeleton> Ordinate(OrdinateDimension geometry, double scale, Op key) =>
        from points in key.Catch(() => geometry.Get3dPoints(
                out Point3d a, out Point3d b, out Point3d c, out Point3d d, out Point3d e)
            ? Fin.Succ(value: Seq(a, b, c, d, e))
            : Fin.Fail<Seq<Point3d>>(error: key.InvalidResult()))
        from lines in key.Catch(() => geometry.GetDisplayLines(
                style: geometry.DimensionStyle, scale: scale, lines: out IEnumerable<Line> resolved)
            ? Fin.Succ(value: toSeq(resolved))
            : Fin.Fail<Seq<Line>>(error: key.InvalidResult()))
        from box in TextBox(geometry.GetTextRectangle, key)
        select new DimSkeleton(Points: points, Lines: lines, Arcs: Seq<Arc>(), TextBox: box);

    private delegate bool TextRectProbe(out Point3d[] corners);

    private static Fin<Arr<Point3d>> TextBox(TextRectProbe probe, Op key) =>
        key.Catch(() => probe(out Point3d[] corners)
            ? Fin.Succ(value: toArr(corners))
            : Fin.Fail<Arr<Point3d>>(error: key.InvalidResult()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DimAnswer : IDetachedDocumentResult {
    private DimAnswer() { }
    public sealed record State(DimState Snapshot) : DimAnswer;
    public sealed record Resolved(DimSkeleton Skeleton) : DimAnswer;
    public sealed record Formatted(string Text) : DimAnswer;
    public sealed record Pieces(Seq<GeometryBase> Products) : DimAnswer;
}

public static class Dimensions {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DimTransaction plan) {
        Op op = Op.Of();
        return from active in Optional(plan).ToFin(Fail: op.InvalidInput())
               from _ in guard(!active.Operations.IsEmpty, op.InvalidInput()).ToFin()
               from receipt in DraftSpine.Commit(
                   session: session, name: active.Name, redraw: active.Redraw, recording: active.UndoRecorded,
                   run: document => active.Operations
                       .TraverseM(operation => operation.Apply(document: document, op: op)).As()
                       .Map(static receipts => receipts.Fold(DraftReceipt.Empty, static (state, value) => state + value)),
                   op: op)
               select receipt;
    }

    public static Fin<DimAnswer> Ask(DocumentSession session, DimAsk request) {
        Op op = Op.Of();
        return from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in session.Demand(
                   use: document => active.Answer(document: document, op: op),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]         | [FORM]                                                   | [ENTRY]                |
| :-----: | :------------------- | :-------------- | :-------------------------------------------------------- | :---------------------- |
|  [01]   | construction         | `DimensionSpec` | ten cases spanning eleven host forms, one `Mint` dispatch | `DimOp.Place`           |
|  [02]   | re-fit               | `DimAdjust`     | per-kind `AdjustFromPoints`/`SetLocations`, kind-checked  | `DimOp.Adjust`          |
|  [03]   | instance pose        | `DimPose`       | dimension-only state as one optional-field payload        | `DimOp.Repose`          |
|  [04]   | per-instance style   | `StylePatch`    | `Overlay` composition — flat property roster never re-listed | `DimOp.Restyle`      |
|  [05]   | dimension mutations  | `DimOp`         | one flat `[Union]` over the shared amend kernel           | `Dimensions.Commit`     |
|  [06]   | display resolution   | `DimSkeleton`   | points + lines + arcs + text box, arity table internalized | `DimAsk.Skeleton`      |
|  [07]   | dimension reads      | `DimAsk`        | state with kind facts, skeleton, value text, explosion    | `Dimensions.Ask`        |
