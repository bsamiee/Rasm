# [RASM_RHINO_ANNOTATION_STYLE]

Drafting-style rail (`Rasm.Rhino.Annotation`). One `StyleField` schema table carries every catalog-proven property/`DimensionStyle.Field` pairing through its axis, payload kind, and read/write delegates, so one patch fold drives the verified configuration surface. `StylePatch` authors styles, amends them through `DimStyleTable`, and mints per-annotation override styles for the text and dimension rails through one `Overlay` seam. `ResourceRef` is the one address union every Annotation table rail resolves through a per-table lens, `DraftSpine` is the one commit kernel every rail's transaction walks, and `DraftReceipt` is the one fact stream every rail folds. Field inheritance is per-field law: a child style holds values only for fields its patch marked, `SetFieldOverride`/`ClearFieldOverride`/`IsFieldOverriden` (host single-`d` spelling) address the same `Field` vocabulary the schema rows key on, and `DimStyleTable.Modify(style, annotation)` is the sole reverse projection from a live annotation back onto a style. Reclamation stays the document rail's `TableOp.Reclaim(TableKind.DimStyles)` row; a per-enum wrapper type, shared-style in-place mutation, and second address union per resource table are deleted forms.

## [01]-[INDEX]

- [02]-[ADDRESS_AND_VOCAB]: `ResourceRef`, `ResourceLens<T>`, and the `LengthDisplayRow` unit vocabulary keyed on explicit host values.
- [03]-[FIELD_SCHEMA]: `StyleAxis`, `ValueKind`, `StyleValue`, the `StyleField` row table, and the `StylePatch` fold with its `Overlay` override-mint seam.
- [04]-[STYLE_RAIL]: `StyleOp`, `StyleTransaction`, and the `Styles.Commit` entry over the shared spine.
- [05]-[ASK_FAMILY]: `StyleAsk`/`StyleAnswer` — snapshot, built-in census, swatch lease, and name minting.
- [06]-[SPINE_AND_RECEIPTS]: `DraftSpine`, `DraftSlot`, `DraftBody`, and the `DraftReceipt` monoid shared by every Annotation rail.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[ADDRESS_AND_VOCAB]

- Owner: `ResourceRef` `[Union]` — `ById`, `ByName`, `ByIndex` — the one table-component address for styles, hatch patterns, linetypes, and section styles; `ResourceLens<TComponent>` — the per-table resolution triple each page mints once as a static row; `LengthDisplayRow` `[SmartEnum<int>]` — the `DimensionStyle.LengthDisplay` vocabulary keyed on explicit host values with a metric column.
- Law: one address union serves every Annotation table — a page supplies its lens, `Resolve` answers the live component, and every arm treats an unresolved address as typed absence; a `StyleRef`/`PatternRef`/`LinetypeRef` sibling per table is the deleted form.
- Law: `LengthDisplay` keys on the explicit host VALUE, never declaration position — the host declares the cases out of value order (`0,3,4,5,6,7,1,8,2,9`), and `Millmeters = 3` is the genuine host spelling; a row keyed by declaration ordinal silently transposes feet and millimeter displays.
- Boundary: resolution reads live per call inside the owning operation — tables mutate under commands, so no resolved component is cached on a value.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
public sealed record ResourceLens<TComponent>(
    Func<RhinoDoc, Guid, TComponent?> ById,
    Func<RhinoDoc, string, TComponent?> ByName,
    Func<RhinoDoc, int, TComponent?> ByIndex) where TComponent : class;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResourceRef : IDetachedDocumentResult {
    private ResourceRef() { }
    public sealed record ById(Guid Value) : ResourceRef;
    public sealed record ByName(string Value) : ResourceRef;
    public sealed record ByIndex(int Value) : ResourceRef;

    public static Fin<ResourceRef> Of(Guid id) =>
        id != Guid.Empty
            ? Fin.Succ<ResourceRef>(value: new ById(Value: id))
            : Fin.Fail<ResourceRef>(error: Op.Of(name: nameof(ResourceRef)).InvalidInput());

    public static Fin<ResourceRef> Of(string name) =>
        Op.Of(name: nameof(ResourceRef)).AcceptText(value: name).Map(static valid => (ResourceRef)new ByName(Value: valid));

    public static Fin<ResourceRef> Of(int index) =>
        index >= 0
            ? Fin.Succ<ResourceRef>(value: new ByIndex(Value: index))
            : Fin.Fail<ResourceRef>(error: Op.Of(name: nameof(ResourceRef)).InvalidInput());

    internal Fin<TComponent> Resolve<TComponent>(RhinoDoc document, ResourceLens<TComponent> lens, Op key) where TComponent : class =>
        Switch(
            state: (Document: document, Lens: lens, Op: key),
            byId: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ById(ctx.Document, address.Value)).ToFin(Fail: ctx.Op.MissingContext())),
            byName: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ByName(ctx.Document, address.Value)).ToFin(Fail: ctx.Op.MissingContext())),
            byIndex: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ByIndex(ctx.Document, address.Value)).ToFin(Fail: ctx.Op.MissingContext())));
}

[SmartEnum<int>]
public sealed partial class LengthDisplayRow {
    public static readonly LengthDisplayRow ModelUnits = new(key: (int)DimensionStyle.LengthDisplay.ModelUnits, metric: false);
    public static readonly LengthDisplayRow InchesFractional = new(key: (int)DimensionStyle.LengthDisplay.InchesFractional, metric: false);
    public static readonly LengthDisplayRow FeetAndInches = new(key: (int)DimensionStyle.LengthDisplay.FeetAndInches, metric: false);
    public static readonly LengthDisplayRow Millimeters = new(key: (int)DimensionStyle.LengthDisplay.Millmeters, metric: true);
    public static readonly LengthDisplayRow Centimeters = new(key: (int)DimensionStyle.LengthDisplay.Centimeters, metric: true);
    public static readonly LengthDisplayRow Meters = new(key: (int)DimensionStyle.LengthDisplay.Meters, metric: true);
    public static readonly LengthDisplayRow Kilometers = new(key: (int)DimensionStyle.LengthDisplay.Kilometers, metric: true);
    public static readonly LengthDisplayRow InchesDecimal = new(key: (int)DimensionStyle.LengthDisplay.InchesDecimal, metric: false);
    public static readonly LengthDisplayRow FeetDecimal = new(key: (int)DimensionStyle.LengthDisplay.FeetDecimal, metric: false);
    public static readonly LengthDisplayRow Miles = new(key: (int)DimensionStyle.LengthDisplay.Miles, metric: false);

    public bool Metric { get; }

    internal DimensionStyle.LengthDisplay Host => (DimensionStyle.LengthDisplay)Key;
}
```

## [03]-[FIELD_SCHEMA]

- Owner: `StyleAxis` `[SmartEnum<int>]` — seven config axes; `ValueKind` keyless `[SmartEnum]` — the payload-shape vocabulary; `StyleValue` `[Union]` — the typed payload carriers; `StyleField` `[SmartEnum<int>]` — one row per catalog-proven property/`DimensionStyle.Field` pairing, keyed on the explicit host field value and carrying axis, kind, and read/write delegates minted through one generic factory; `StyleEdit` — one kind-checked field assignment; `StylePatch` — the ordered edit run with the table fold and per-annotation `Overlay` mint.
- Law: the schema is the one write surface — a patch applies through row delegates, and enum-valued fields ride `Whole` payloads whose host cast lives inside the row; a call site naming a `DimensionStyle` property directly bypasses the census and is the deleted form.
- Law: kind agreement is admission — `StyleEdit.Of` refuses a payload whose kind differs from the row's column, so `Write` never sees a mismatched case and the cast inside a row delegate is total.
- Law: color/plot-source `Field` cases from `ExtLineColorSource` through `DimLinePlotWeight_mm`, plus `MaskFlags`, `SignedOrdinate`, and `UnitSystem`, carry no CLR property on `DimensionStyle`; `Name` and `Index` cannot inherit from a parent. `StyleField` excludes every non-property case, and the override census reports schema rows alone.
- Law: every host setter routes `ON_DimStyle_Set*` with `setOverride: true`, so a row's `Write` is also its override marking — the schema needs no second marking pass, and `MaskOffset` pairs `Field.MaskBorder`, the host's name for the mask-offset slot.
- Law: `Field.LeaderContentAngle` is a shared slot — `LeaderContentAngleType` reads it as `GetInt` and `LeaderTextRotationRadians`/`LeaderTextRotationDegrees` as `GetDouble`, while `Field.LeaderContentAngleStyle` binds no accessor — so the schema carries exactly one row for the field, the angle-style enum, and the rotation double stays off-schema; a second row keyed on the same field value is a duplicate-key fault at vocabulary materialization.
- Law: `ToleranceZeroSuppress` is an inert host stub — its getter returns the constant `ZeroSuppression.None`, its setter body is empty, and no `Field` case backs it — so the tolerance axis excludes it and no patch can claim tolerance zero suppression.
- Law: `Overlay` is the one per-annotation override mint — `SetOverrideDimStyle` demands a nil-id style outside the table with its differing fields marked, so the mint runs `Duplicate(newName, newId, newParentId)` with an empty id re-parented to the annotation's bound style, the patch setters mark their own fields, and the child attaches; the text and dimension rails compose it, and a shared style mutated in place to customize one annotation is the deleted form.
- Growth: a catalog-proven host config pairing is one row minted through its payload adapter; every patch, snapshot, and census gains it without another operation surface.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StyleAxis {
    public static readonly StyleAxis Arrow = new(key: 0);
    public static readonly StyleAxis Text = new(key: 1);
    public static readonly StyleAxis Length = new(key: 2);
    public static readonly StyleAxis Tolerance = new(key: 3);
    public static readonly StyleAxis Mask = new(key: 4);
    public static readonly StyleAxis Layout = new(key: 5);
    public static readonly StyleAxis Leader = new(key: 6);
}

[SmartEnum]
public sealed partial class ValueKind {
    public static readonly ValueKind Real = new();
    public static readonly ValueKind Whole = new();
    public static readonly ValueKind Flag = new();
    public static readonly ValueKind Script = new();
    public static readonly ValueKind Tint = new();
    public static readonly ValueKind Anchor = new();
    public static readonly ValueKind Face = new();
    public static readonly ValueKind Glyph = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleValue {
    private StyleValue() { }
    public sealed record Real(double Value) : StyleValue;
    public sealed record Whole(int Value) : StyleValue;
    public sealed record Flag(bool Value) : StyleValue;
    public sealed record Script(string Value) : StyleValue;
    public sealed record Tint(PerceptualColor Value) : StyleValue;
    public sealed record Anchor(Guid Value) : StyleValue;
    public sealed record Face(Font Value) : StyleValue;
    public sealed record Glyph(char Value) : StyleValue;

    internal ValueKind Kind => Switch(
        real: static _ => ValueKind.Real,
        whole: static _ => ValueKind.Whole,
        flag: static _ => ValueKind.Flag,
        script: static _ => ValueKind.Script,
        tint: static _ => ValueKind.Tint,
        anchor: static _ => ValueKind.Anchor,
        face: static _ => ValueKind.Face,
        glyph: static _ => ValueKind.Glyph);
}

[SmartEnum<int>]
public sealed partial class StyleField {
    // --- [ARROW]
    public static readonly StyleField ArrowType1 = Pick(DimensionStyle.Field.ArrowType1, StyleAxis.Arrow, static s => (int)s.ArrowType1, static (s, v) => s.ArrowType1 = (DimensionStyle.ArrowType)v);
    public static readonly StyleField ArrowType2 = Pick(DimensionStyle.Field.ArrowType2, StyleAxis.Arrow, static s => (int)s.ArrowType2, static (s, v) => s.ArrowType2 = (DimensionStyle.ArrowType)v);
    public static readonly StyleField LeaderArrowType = Pick(DimensionStyle.Field.LeaderArrowType, StyleAxis.Arrow, static s => (int)s.LeaderArrowType, static (s, v) => s.LeaderArrowType = (DimensionStyle.ArrowType)v);
    public static readonly StyleField ArrowLength = Real(DimensionStyle.Field.Arrowsize, StyleAxis.Arrow, static s => s.ArrowLength, static (s, v) => s.ArrowLength = v);
    public static readonly StyleField LeaderArrowLength = Real(DimensionStyle.Field.LeaderArrowsize, StyleAxis.Arrow, static s => s.LeaderArrowLength, static (s, v) => s.LeaderArrowLength = v);
    public static readonly StyleField ArrowBlockId1 = Anchor(DimensionStyle.Field.ArrowBlockId1, StyleAxis.Arrow, static s => s.ArrowBlockId1, static (s, v) => s.ArrowBlockId1 = v);
    public static readonly StyleField ArrowBlockId2 = Anchor(DimensionStyle.Field.ArrowBlockId2, StyleAxis.Arrow, static s => s.ArrowBlockId2, static (s, v) => s.ArrowBlockId2 = v);
    public static readonly StyleField LeaderArrowBlockId = Anchor(DimensionStyle.Field.LeaderArrowBlock, StyleAxis.Arrow, static s => s.LeaderArrowBlockId, static (s, v) => s.LeaderArrowBlockId = v);
    public static readonly StyleField ClippingArrowType1 = Pick(DimensionStyle.Field.ClippingArrowType1, StyleAxis.Arrow, static s => (int)s.ClippingArrowType1, static (s, v) => s.ClippingArrowType1 = (DimensionStyle.ClippingArrowType)v);
    public static readonly StyleField ClippingArrowType2 = Pick(DimensionStyle.Field.ClippingArrowType2, StyleAxis.Arrow, static s => (int)s.ClippingArrowType2, static (s, v) => s.ClippingArrowType2 = (DimensionStyle.ClippingArrowType)v);
    public static readonly StyleField ClippingArrowLength = Real(DimensionStyle.Field.ClippingArrowSize, StyleAxis.Arrow, static s => s.ClippingArrowLength, static (s, v) => s.ClippingArrowLength = v);
    public static readonly StyleField FitArrow = Pick(DimensionStyle.Field.ArrowFit, StyleAxis.Arrow, static s => (int)s.FitArrow, static (s, v) => s.FitArrow = (DimensionStyle.ArrowFit)v);
    public static readonly StyleField SuppressArrow1 = Flag(DimensionStyle.Field.SuppressArrow1, StyleAxis.Arrow, static s => s.SuppressArrow1, static (s, v) => s.SuppressArrow1 = v);
    public static readonly StyleField SuppressArrow2 = Flag(DimensionStyle.Field.SuppressArrow2, StyleAxis.Arrow, static s => s.SuppressArrow2, static (s, v) => s.SuppressArrow2 = v);
    // --- [TEXT]
    public static readonly StyleField TextHeight = Real(DimensionStyle.Field.TextHeight, StyleAxis.Text, static s => s.TextHeight, static (s, v) => s.TextHeight = v);
    public static readonly StyleField TextGap = Real(DimensionStyle.Field.TextGap, StyleAxis.Text, static s => s.TextGap, static (s, v) => s.TextGap = v);
    public static readonly StyleField TextRotation = Real(DimensionStyle.Field.TextRotation, StyleAxis.Text, static s => s.TextRotation, static (s, v) => s.TextRotation = v);
    public static readonly StyleField TypeFace = Face(DimensionStyle.Field.Font, StyleAxis.Text, static s => s.Font, static (s, v) => s.Font = v);
    public static readonly StyleField TextVerticalAlignment = Pick(DimensionStyle.Field.TextVerticalAlignment, StyleAxis.Text, static s => (int)s.TextVerticalAlignment, static (s, v) => s.TextVerticalAlignment = (TextVerticalAlignment)v);
    public static readonly StyleField TextHorizontalAlignment = Pick(DimensionStyle.Field.TextHorizontalAlignment, StyleAxis.Text, static s => (int)s.TextHorizontalAlignment, static (s, v) => s.TextHorizontalAlignment = (TextHorizontalAlignment)v);
    public static readonly StyleField TextOrientation = Pick(DimensionStyle.Field.TextOrientation, StyleAxis.Text, static s => (int)s.TextOrientation, static (s, v) => s.TextOrientation = (TextOrientation)v);
    public static readonly StyleField LeaderTextOrientation = Pick(DimensionStyle.Field.LeaderTextOrientation, StyleAxis.Text, static s => (int)s.LeaderTextOrientation, static (s, v) => s.LeaderTextOrientation = (TextOrientation)v);
    public static readonly StyleField DimTextOrientation = Pick(DimensionStyle.Field.DimTextOrientation, StyleAxis.Text, static s => (int)s.DimTextOrientation, static (s, v) => s.DimTextOrientation = (TextOrientation)v);
    public static readonly StyleField DimRadialTextOrientation = Pick(DimensionStyle.Field.DimRadialTextOrientation, StyleAxis.Text, static s => (int)s.DimRadialTextOrientation, static (s, v) => s.DimRadialTextOrientation = (TextOrientation)v);
    public static readonly StyleField DimTextLocation = Pick(DimensionStyle.Field.DimTextLocation, StyleAxis.Text, static s => (int)s.DimTextLocation, static (s, v) => s.DimTextLocation = (DimensionStyle.TextLocation)v);
    public static readonly StyleField DimRadialTextLocation = Pick(DimensionStyle.Field.DimRadialTextLocation, StyleAxis.Text, static s => (int)s.DimRadialTextLocation, static (s, v) => s.DimRadialTextLocation = (DimensionStyle.TextLocation)v);
    public static readonly StyleField DimTextAngleType = Pick(DimensionStyle.Field.DimTextAngleStyle, StyleAxis.Text, static s => (int)s.DimTextAngleType, static (s, v) => s.DimTextAngleType = (DimensionStyle.LeaderContentAngleStyle)v);
    public static readonly StyleField DimRadialTextAngleType = Pick(DimensionStyle.Field.DimRadialTextAngleStyle, StyleAxis.Text, static s => (int)s.DimRadialTextAngleType, static (s, v) => s.DimRadialTextAngleType = (DimensionStyle.LeaderContentAngleStyle)v);
    public static readonly StyleField FitText = Pick(DimensionStyle.Field.TextFit, StyleAxis.Text, static s => (int)s.FitText, static (s, v) => s.FitText = (DimensionStyle.TextFit)v);
    public static readonly StyleField UseKerning = Flag(DimensionStyle.Field.Kerning, StyleAxis.Text, static s => s.UseKerning, static (s, v) => s.UseKerning = v);
    public static readonly StyleField TextUnderlined = Flag(DimensionStyle.Field.TextUnderlined, StyleAxis.Text, static s => s.TextUnderlined, static (s, v) => s.TextUnderlined = v);
    public static readonly StyleField LineSpaceScale = Real(DimensionStyle.Field.LineSpaceScale, StyleAxis.Text, static s => s.LineSpaceScale, static (s, v) => s.LineSpaceScale = v);
    public static readonly StyleField DrawForward = Flag(DimensionStyle.Field.DrawForward, StyleAxis.Text, static s => s.DrawForward, static (s, v) => s.DrawForward = v);
    public static readonly StyleField DecimalSeparator = Glyph(DimensionStyle.Field.DecimalSeparator, StyleAxis.Text, static s => s.DecimalSeparator, static (s, v) => s.DecimalSeparator = v);
    // --- [LENGTH]
    public static readonly StyleField LengthFactor = Real(DimensionStyle.Field.LengthFactor, StyleAxis.Length, static s => s.LengthFactor, static (s, v) => s.LengthFactor = v);
    public static readonly StyleField AlternateLengthFactor = Real(DimensionStyle.Field.AlternateLengthFactor, StyleAxis.Length, static s => s.AlternateLengthFactor, static (s, v) => s.AlternateLengthFactor = v);
    public static readonly StyleField LengthResolution = Whole(DimensionStyle.Field.LengthResolution, StyleAxis.Length, static s => s.LengthResolution, static (s, v) => s.LengthResolution = v);
    public static readonly StyleField AlternateLengthResolution = Whole(DimensionStyle.Field.AlternateLengthResolution, StyleAxis.Length, static s => s.AlternateLengthResolution, static (s, v) => s.AlternateLengthResolution = v);
    public static readonly StyleField AngleResolution = Whole(DimensionStyle.Field.AngleResolution, StyleAxis.Length, static s => s.AngleResolution, static (s, v) => s.AngleResolution = v);
    public static readonly StyleField DimensionLengthDisplay = Pick(DimensionStyle.Field.DimensionLengthDisplay, StyleAxis.Length, static s => (int)s.DimensionLengthDisplay, static (s, v) => s.DimensionLengthDisplay = (DimensionStyle.LengthDisplay)v);
    public static readonly StyleField AlternateDimensionLengthDisplay = Pick(DimensionStyle.Field.AlternateDimensionLengthDisplay, StyleAxis.Length, static s => (int)s.AlternateDimensionLengthDisplay, static (s, v) => s.AlternateDimensionLengthDisplay = (DimensionStyle.LengthDisplay)v);
    public static readonly StyleField AngleFormat = Pick(DimensionStyle.Field.AngleFormat, StyleAxis.Length, static s => (int)s.AngleFormat, static (s, v) => s.AngleFormat = (DimensionStyle.AngleDisplayFormat)v);
    public static readonly StyleField Roundoff = Real(DimensionStyle.Field.Round, StyleAxis.Length, static s => s.Roundoff, static (s, v) => s.Roundoff = v);
    public static readonly StyleField AlternateRoundoff = Real(DimensionStyle.Field.AltRound, StyleAxis.Length, static s => s.AlternateRoundoff, static (s, v) => s.AlternateRoundoff = v);
    public static readonly StyleField AngularRoundoff = Real(DimensionStyle.Field.AngularRound, StyleAxis.Length, static s => s.AngularRoundoff, static (s, v) => s.AngularRoundoff = v);
    public static readonly StyleField ZeroSuppress = Pick(DimensionStyle.Field.ZeroSuppress, StyleAxis.Length, static s => (int)s.ZeroSuppress, static (s, v) => s.ZeroSuppress = (DimensionStyle.ZeroSuppression)v);
    public static readonly StyleField AlternateZeroSuppress = Pick(DimensionStyle.Field.AltZeroSuppress, StyleAxis.Length, static s => (int)s.AlternateZeroSuppress, static (s, v) => s.AlternateZeroSuppress = (DimensionStyle.ZeroSuppression)v);
    public static readonly StyleField AngleZeroSuppress = Pick(DimensionStyle.Field.AngleZeroSuppress, StyleAxis.Length, static s => (int)s.AngleZeroSuppress, static (s, v) => s.AngleZeroSuppress = (DimensionStyle.ZeroSuppression)v);
    public static readonly StyleField Prefix = Script(DimensionStyle.Field.Prefix, StyleAxis.Length, static s => s.Prefix, static (s, v) => s.Prefix = v);
    public static readonly StyleField Suffix = Script(DimensionStyle.Field.Suffix, StyleAxis.Length, static s => s.Suffix, static (s, v) => s.Suffix = v);
    public static readonly StyleField AlternatePrefix = Script(DimensionStyle.Field.AlternatePrefix, StyleAxis.Length, static s => s.AlternatePrefix, static (s, v) => s.AlternatePrefix = v);
    public static readonly StyleField AlternateSuffix = Script(DimensionStyle.Field.AlternateSuffix, StyleAxis.Length, static s => s.AlternateSuffix, static (s, v) => s.AlternateSuffix = v);
    public static readonly StyleField StackFractionFormat = Pick(DimensionStyle.Field.StackFormat, StyleAxis.Length, static s => (int)s.StackFractionFormat, static (s, v) => s.StackFractionFormat = (DimensionStyle.StackDisplayFormat)v);
    public static readonly StyleField StackHeightScale = Real(DimensionStyle.Field.StackTextheightScale, StyleAxis.Length, static s => s.StackHeightScale, static (s, v) => s.StackHeightScale = v);
    public static readonly StyleField AlternateUnitsDisplay = Flag(DimensionStyle.Field.Alternate, StyleAxis.Length, static s => s.AlternateUnitsDisplay, static (s, v) => s.AlternateUnitsDisplay = v);
    public static readonly StyleField AlternateBelowLine = Flag(DimensionStyle.Field.AltBelow, StyleAxis.Length, static s => s.AlternateBelowLine, static (s, v) => s.AlternateBelowLine = v);
    // --- [TOLERANCE]
    public static readonly StyleField ToleranceFormat = Pick(DimensionStyle.Field.ToleranceFormat, StyleAxis.Tolerance, static s => (int)s.ToleranceFormat, static (s, v) => s.ToleranceFormat = (DimensionStyle.ToleranceDisplayFormat)v);
    public static readonly StyleField ToleranceResolution = Whole(DimensionStyle.Field.ToleranceResolution, StyleAxis.Tolerance, static s => s.ToleranceResolution, static (s, v) => s.ToleranceResolution = v);
    public static readonly StyleField AlternateToleranceResolution = Whole(DimensionStyle.Field.AltToleranceResolution, StyleAxis.Tolerance, static s => s.AlternateToleranceResolution, static (s, v) => s.AlternateToleranceResolution = v);
    public static readonly StyleField ToleranceHeightScale = Real(DimensionStyle.Field.ToleranceHeightScale, StyleAxis.Tolerance, static s => s.ToleranceHeightScale, static (s, v) => s.ToleranceHeightScale = v);
    public static readonly StyleField ToleranceUpperValue = Real(DimensionStyle.Field.ToleranceUpperValue, StyleAxis.Tolerance, static s => s.ToleranceUpperValue, static (s, v) => s.ToleranceUpperValue = v);
    public static readonly StyleField ToleranceLowerValue = Real(DimensionStyle.Field.ToleranceLowerValue, StyleAxis.Tolerance, static s => s.ToleranceLowerValue, static (s, v) => s.ToleranceLowerValue = v);
    // --- [MASK]
    public static readonly StyleField DrawTextMask = Flag(DimensionStyle.Field.DrawMask, StyleAxis.Mask, static s => s.DrawTextMask, static (s, v) => s.DrawTextMask = v);
    public static readonly StyleField MaskColor = Tint(DimensionStyle.Field.MaskColor, StyleAxis.Mask, static s => s.MaskColor, static (s, v) => s.MaskColor = v);
    public static readonly StyleField MaskColorSource = Pick(DimensionStyle.Field.MaskColorSource, StyleAxis.Mask, static s => (int)s.MaskColorSource, static (s, v) => s.MaskColorSource = (DimensionStyle.MaskType)v);
    public static readonly StyleField MaskFrameType = Pick(DimensionStyle.Field.MaskFrameType, StyleAxis.Mask, static s => (int)s.MaskFrameType, static (s, v) => s.MaskFrameType = (DimensionStyle.MaskFrame)v);
    public static readonly StyleField MaskOffset = Real(DimensionStyle.Field.MaskBorder, StyleAxis.Mask, static s => s.MaskOffset, static (s, v) => s.MaskOffset = v);
    // --- [LAYOUT]
    public static readonly StyleField BaselineSpacing = Real(DimensionStyle.Field.BaselineSpacing, StyleAxis.Layout, static s => s.BaselineSpacing, static (s, v) => s.BaselineSpacing = v);
    public static readonly StyleField DimensionScale = Real(DimensionStyle.Field.DimensionScale, StyleAxis.Layout, static s => s.DimensionScale, static (s, v) => s.DimensionScale = v);
    public static readonly StyleField CentermarkSize = Real(DimensionStyle.Field.Centermark, StyleAxis.Layout, static s => s.CentermarkSize, static (s, v) => s.CentermarkSize = v);
    public static readonly StyleField CenterMarkType = Pick(DimensionStyle.Field.CentermarkStyle, StyleAxis.Layout, static s => (int)s.CenterMarkType, static (s, v) => s.CenterMarkType = (DimensionStyle.CenterMarkStyle)v);
    public static readonly StyleField ExtensionLineExtension = Real(DimensionStyle.Field.ExtensionLineExtension, StyleAxis.Layout, static s => s.ExtensionLineExtension, static (s, v) => s.ExtensionLineExtension = v);
    public static readonly StyleField ExtensionLineOffset = Real(DimensionStyle.Field.ExtensionLineOffset, StyleAxis.Layout, static s => s.ExtensionLineOffset, static (s, v) => s.ExtensionLineOffset = v);
    public static readonly StyleField DimensionLineExtension = Real(DimensionStyle.Field.DimensionLineExtension, StyleAxis.Layout, static s => s.DimensionLineExtension, static (s, v) => s.DimensionLineExtension = v);
    public static readonly StyleField SuppressExtension1 = Flag(DimensionStyle.Field.SuppressExtension1, StyleAxis.Layout, static s => s.SuppressExtension1, static (s, v) => s.SuppressExtension1 = v);
    public static readonly StyleField SuppressExtension2 = Flag(DimensionStyle.Field.SuppressExtension2, StyleAxis.Layout, static s => s.SuppressExtension2, static (s, v) => s.SuppressExtension2 = v);
    public static readonly StyleField FixedExtensionOn = Flag(DimensionStyle.Field.FixedExtensionOn, StyleAxis.Layout, static s => s.FixedExtensionOn, static (s, v) => s.FixedExtensionOn = v);
    public static readonly StyleField FixedExtensionLength = Real(DimensionStyle.Field.FixedExtensionLength, StyleAxis.Layout, static s => s.FixedExtensionLength, static (s, v) => s.FixedExtensionLength = v);
    public static readonly StyleField ForceDimensionLineBetweenExtensionLines = Flag(DimensionStyle.Field.ForceDimLine, StyleAxis.Layout, static s => s.ForceDimensionLineBetweenExtensionLines, static (s, v) => s.ForceDimensionLineBetweenExtensionLines = v);
    public static readonly StyleField TextMoveLeader = Whole(DimensionStyle.Field.TextmoveLeader, StyleAxis.Layout, static s => s.TextMoveLeader, static (s, v) => s.TextMoveLeader = v);
    public static readonly StyleField ArcLengthSymbol = Whole(DimensionStyle.Field.ArclengthSymbol, StyleAxis.Layout, static s => s.ArcLengthSymbol, static (s, v) => s.ArcLengthSymbol = v);
    // --- [LEADER]
    public static readonly StyleField LeaderHasLanding = Flag(DimensionStyle.Field.LeaderHasLanding, StyleAxis.Leader, static s => s.LeaderHasLanding, static (s, v) => s.LeaderHasLanding = v);
    public static readonly StyleField LeaderLandingLength = Real(DimensionStyle.Field.LeaderLandingLength, StyleAxis.Leader, static s => s.LeaderLandingLength, static (s, v) => s.LeaderLandingLength = v);
    public static readonly StyleField LeaderContentAngleType = Pick(DimensionStyle.Field.LeaderContentAngle, StyleAxis.Leader, static s => (int)s.LeaderContentAngleType, static (s, v) => s.LeaderContentAngleType = (DimensionStyle.LeaderContentAngleStyle)v);
    public static readonly StyleField LeaderCurveType = Pick(DimensionStyle.Field.LeaderCurveType, StyleAxis.Leader, static s => (int)s.LeaderCurveType, static (s, v) => s.LeaderCurveType = (DimensionStyle.LeaderCurveStyle)v);
    public static readonly StyleField LeaderTextVerticalAlignment = Pick(DimensionStyle.Field.LeaderTextVerticalAlignment, StyleAxis.Leader, static s => (int)s.LeaderTextVerticalAlignment, static (s, v) => s.LeaderTextVerticalAlignment = (TextVerticalAlignment)v);
    public static readonly StyleField LeaderTextHorizontalAlignment = Pick(DimensionStyle.Field.LeaderTextHorizontalAlignment, StyleAxis.Leader, static s => (int)s.LeaderTextHorizontalAlignment, static (s, v) => s.LeaderTextHorizontalAlignment = (TextHorizontalAlignment)v);

    public StyleAxis Axis { get; }
    public ValueKind Kind { get; }

    internal DimensionStyle.Field Host => (DimensionStyle.Field)Key;

    [UseDelegateFromConstructor]
    internal partial Fin<StyleValue> Read(DimensionStyle style, Op key);

    [UseDelegateFromConstructor]
    internal partial Unit Write(DimensionStyle style, StyleValue value);

    private static StyleField Real(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, double> get, Action<DimensionStyle, double> set) =>
        Of(field, axis, ValueKind.Real, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Real(Value: value)), static value => ((StyleValue.Real)value).Value);

    private static StyleField Whole(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, int> get, Action<DimensionStyle, int> set) =>
        Of(field, axis, ValueKind.Whole, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Whole(Value: value)), static value => ((StyleValue.Whole)value).Value);

    private static StyleField Pick(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, int> get, Action<DimensionStyle, int> set) =>
        Whole(field: field, axis: axis, get: get, set: set);

    private static StyleField Flag(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, bool> get, Action<DimensionStyle, bool> set) =>
        Of(field, axis, ValueKind.Flag, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Flag(Value: value)), static value => ((StyleValue.Flag)value).Value);

    private static StyleField Script(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, string> get, Action<DimensionStyle, string> set) =>
        Of(field, axis, ValueKind.Script, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Script(Value: value)), static value => ((StyleValue.Script)value).Value);

    private static StyleField Tint(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, System.Drawing.Color> get, Action<DimensionStyle, System.Drawing.Color> set) =>
        Of(field, axis, ValueKind.Tint, get, set,
            static (value, key) => PerceptualColor.OfRgb(
                red: value.R,
                green: value.G,
                blue: value.B,
                alpha: value.A / 255.0,
                key: key).Map(static color => (StyleValue)new StyleValue.Tint(Value: color)),
            static value => {
                (byte red, byte green, byte blue, double alpha) = ((StyleValue.Tint)value).Value.ToRgb();
                return System.Drawing.Color.FromArgb((byte)Math.Round(alpha * 255.0), red, green, blue);
            });

    private static StyleField Anchor(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Guid> get, Action<DimensionStyle, Guid> set) =>
        Of(field, axis, ValueKind.Anchor, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Anchor(Value: value)), static value => ((StyleValue.Anchor)value).Value);

    private static StyleField Face(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Font> get, Action<DimensionStyle, Font> set) =>
        Of(field, axis, ValueKind.Face, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Face(Value: value)), static value => ((StyleValue.Face)value).Value);

    private static StyleField Glyph(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, char> get, Action<DimensionStyle, char> set) =>
        Of(field, axis, ValueKind.Glyph, get, set, static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Glyph(Value: value)), static value => ((StyleValue.Glyph)value).Value);

    private static StyleField Of<T>(
        DimensionStyle.Field field,
        StyleAxis axis,
        ValueKind kind,
        Func<DimensionStyle, T> get,
        Action<DimensionStyle, T> set,
        Func<T, Op, Fin<StyleValue>> wrap,
        Func<StyleValue, T> unwrap) =>
        new(
            key: (int)field,
            axis: axis,
            kind: kind,
            read: (style, key) => wrap(get(style), key),
            write: (style, value) => {
                set(style, unwrap(value));
                return unit;
            });
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record StyleEdit {
    private StyleEdit(StyleField field, StyleValue value) {
        Field = field;
        Value = value;
    }

    public StyleField Field { get; }
    public StyleValue Value { get; }

    public static Fin<StyleEdit> Of(StyleField field, StyleValue value, Op? key = null) {
        Op op = key.OrDefault();
        return from row in Optional(field).ToFin(Fail: op.InvalidInput())
               from payload in Optional(value).ToFin(Fail: op.InvalidInput())
               from _ in guard(row.Kind == payload.Kind, op.InvalidInput()).ToFin()
               select new StyleEdit(field: row, value: payload);
    }
}

public sealed record StylePatch(Seq<StyleEdit> Edits) {
    public static Fin<StylePatch> Of(params ReadOnlySpan<StyleEdit> edits) {
        Op op = Op.Of();
        return toSeq(edits.ToArray())
            .TraverseM(edit => Optional(edit).ToFin(Fail: op.InvalidInput())).As()
            .Bind(admitted => guard(!admitted.IsEmpty, op.InvalidInput()).ToFin().Map(_ => new StylePatch(Edits: admitted)));
    }

    public Seq<StyleField> Marked => Edits.Map(static edit => edit.Field).Distinct();

    internal Fin<Unit> Apply(DimensionStyle style, Op key) =>
        Edits.TraverseM(edit => key.Catch(() => Fin.Succ(value: edit.Field.Write(style: style, value: edit.Value)))).As()
            .Map(static _ => unit);

    internal Fin<DimensionStyle> Overlay(AnnotationBase annotation, Op key) =>
        from effective in Optional(annotation.DimensionStyle).ToFin(Fail: key.MissingContext())
        from child in key.Catch(() => Fin.Succ(value: effective.Duplicate(
            newName: string.Empty, newId: Guid.Empty, newParentId: annotation.DimensionStyleId)))
        from _ in Apply(style: child, key: key)
        from attached in key.Confirm(success: annotation.SetOverrideDimStyle(overrideStyle: child))
        select child;
}
```

## [04]-[STYLE_RAIL]

- Owner: `StyleOp` `[Union]` — the mutation verbs over the `DimStyleTable`: authoring, patch amendment, override clearing, reverse absorption, reparenting, current selection, deletion, length scaling, the paper/model scale faces, and the user-string bag; `StyleTransaction` — the commit plan; `Styles` — the `Commit`/`Ask` entry pair.
- Law: an amendment never mutates the resolved live component — every write duplicates it, applies its change to the copy, and lands through `DimStyleTable.Modify` by index inside the shared undo bracket.
- Law: `Author` refuses an existing name typed — `FindName` probes inside the arm, and a caller wanting a fresh name composes the `MintName` ask; a parent payload makes the authored style a child whose patch-marked fields alone override the parent through `ParentId`.
- Law: `Absorb` is the one reverse projection — `DimStyleTable.Modify(style, annotation)` folds a live annotation's per-instance overrides back onto the style, its `ModifyType` outcome inspected before the write counts: `Modify` and `Override` land as receipt facts, `NotSaved` is a typed refusal.
- Law: reclamation is not a case — unused-style reclaim is the document rail's `TableOp.Reclaim(TableKind.DimStyles)` row, and re-spelling it here splits one host member across two owners.
- Growth: a new style verb is one case with its arm; the spine, the receipt, and every consumer read it with zero new surface.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleOp {
    private StyleOp() { }
    public sealed record Author(string Name, StylePatch Patch, Option<Guid> Parent = default) : StyleOp;
    public sealed record Amend(ResourceRef Target, StylePatch Patch, bool Quiet = true) : StyleOp;
    public sealed record ClearOverrides(ResourceRef Target, Seq<StyleField> Fields, bool Quiet = true) : StyleOp;
    public sealed record Absorb(ResourceRef Target, TableTarget Annotation) : StyleOp;
    public sealed record Reparent(ResourceRef Target, Option<Guid> Parent, bool Quiet = true) : StyleOp;
    public sealed record SetCurrent(ResourceRef Target, bool Quiet = true) : StyleOp;
    public sealed record Delete(ResourceRef Target, bool Quiet = true) : StyleOp;
    public sealed record ScaleLengths(ResourceRef Target, double Factor, bool Quiet = true) : StyleOp;
    public sealed record PageScale(ResourceRef Target, double LeftMillimeters, double RightMillimeters, bool Quiet = true) : StyleOp;
    public sealed record Tag(ResourceRef Target, string Key, Option<string> Value = default) : StyleOp;

    internal static readonly ResourceLens<DimensionStyle> Lens = new(
        ById: static (document, id) => document.DimStyles.Find(styleId: id, ignoreDeleted: true),
        ByName: static (document, name) => document.DimStyles.FindName(name: name),
        ByIndex: static (document, index) => document.DimStyles.FindIndex(index: index));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            author: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in guard(context.Document.DimStyles.FindName(name: name) is null, context.Op.InvalidInput()).ToFin()
                from index in context.Op.Catch(() => context.Document.DimStyles.Add(name: name) is var added && added >= 0
                    ? Fin.Succ(value: added)
                    : Fin.Fail<int>(error: context.Op.InvalidResult()))
                from fresh in Optional(context.Document.DimStyles.FindIndex(index: index)).ToFin(Fail: context.Op.InvalidResult())
                from receipt in context.Op.Catch(() => {
                    DimensionStyle shaped = fresh.Duplicate();
                    _ = edit.Parent.Iter(parent => shaped.ParentId = parent);
                    return edit.Patch.Apply(style: shaped, key: context.Op)
                        .Bind(_ => context.Op.Confirm(success: context.Document.DimStyles.Modify(newSettings: shaped, dimstyleIndex: index, quiet: true)))
                        .Map(_ => DraftReceipt.Component(slot: DraftSlot.Authored, index: index));
                })
                select receipt,
            amend: static (context, edit) =>
                Revised(target: edit.Target, document: context.Document, quiet: edit.Quiet, op: context.Op,
                    revise: (style, key) => edit.Patch.Apply(style: style, key: key), slot: DraftSlot.Amended),
            clearOverrides: static (context, edit) =>
                Revised(target: edit.Target, document: context.Document, quiet: edit.Quiet, op: context.Op,
                    revise: (style, key) => key.Catch(() => {
                        _ = edit.Fields.IsEmpty
                            ? fun(style.ClearAllFieldOverrides)()
                            : ignore(edit.Fields.Iter(field => style.ClearFieldOverride(field: field.Host)));
                        return Fin.Succ(value: unit);
                    }), slot: DraftSlot.Amended),
            absorb: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from ids in edit.Annotation.Resolve(document: context.Document, key: context.Op)
                from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: context.Op.InvalidInput()) }
                from native in Optional(context.Document.Objects.FindId(id)).ToFin(Fail: context.Op.MissingContext())
                from annotation in Optional((native as AnnotationObjectBase)?.AnnotationGeometry).ToFin(Fail: context.Op.InvalidInput())
                from outcome in context.Op.Catch(() => context.Document.DimStyles.Modify(dimstyle: style, annotation: annotation) switch {
                    ModifyType.Modify or ModifyType.Override => Fin.Succ(value: style.Index),
                    var refused => Fin.Fail<int>(error: context.Op.InvalidResult(detail: refused.ToString())),
                })
                select DraftReceipt.Component(slot: DraftSlot.Absorbed, index: outcome),
            reparent: static (context, edit) =>
                Revised(target: edit.Target, document: context.Document, quiet: edit.Quiet, op: context.Op,
                    revise: (style, key) => key.Catch(() => {
                        style.ParentId = edit.Parent.IfNone(noneValue: Guid.Empty);
                        return Fin.Succ(value: unit);
                    }), slot: DraftSlot.Reparented),
            setCurrent: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.SetCurrent(index: style.Index, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Current, index: style.Index),
            delete: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.Delete(index: style.Index, quiet: edit.Quiet))
                select DraftReceipt.Component(slot: DraftSlot.Deleted, index: style.Index),
            scaleLengths: static (context, edit) =>
                Revised(target: edit.Target, document: context.Document, quiet: edit.Quiet, op: context.Op,
                    revise: (style, key) =>
                        from _ in key.Positive(value: edit.Factor)
                        from __ in key.Catch(() => { style.ScaleLengthValues(scale: edit.Factor); return Fin.Succ(value: unit); })
                        select unit, slot: DraftSlot.Scaled),
            pageScale: static (context, edit) =>
                Revised(target: edit.Target, document: context.Document, quiet: edit.Quiet, op: context.Op,
                    revise: (style, key) =>
                        from _ in key.Positive(value: edit.LeftMillimeters)
                        from __ in key.Positive(value: edit.RightMillimeters)
                        from ___ in key.Catch(() => {
                            style.ScaleLeftLengthMillimeters = edit.LeftMillimeters;
                            style.ScaleRightLengthMillimeters = edit.RightMillimeters;
                            return Fin.Succ(value: unit);
                        })
                        select unit, slot: DraftSlot.Scaled),
            tag: static (context, edit) =>
                from key in context.Op.AcceptText(value: edit.Key)
                from receipt in Revised(
                    target: edit.Target,
                    document: context.Document,
                    quiet: true,
                    op: context.Op,
                    revise: (copy, op) => edit.Value.Match(
                        Some: value => op.Confirm(success: copy.SetUserString(key: key, value: value)),
                        None: () => op.Confirm(success: copy.DeleteUserString(key: key))),
                    slot: DraftSlot.Amended)
                select receipt);

    private static Fin<DraftReceipt> Revised(
        ResourceRef target, RhinoDoc document, bool quiet, Op op,
        Func<DimensionStyle, Op, Fin<Unit>> revise, DraftSlot slot) =>
        from style in target.Resolve(document: document, lens: Lens, key: op)
        from copy in op.Catch(() => Fin.Succ(value: style.Duplicate()))
        from _ in revise(copy, op)
        from __ in op.Confirm(success: document.DimStyles.Modify(newSettings: copy, dimstyleIndex: style.Index, quiet: quiet))
        select DraftReceipt.Component(slot: slot, index: style.Index);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record StyleTransaction(string Name, Seq<StyleOp> Operations, RedrawPolicy Redraw, bool UndoRecorded = true) {
    public static StyleTransaction Batch(string name, params ReadOnlySpan<StyleOp> operations) =>
        new(Name: name, Operations: toSeq(operations.ToArray()), Redraw: RedrawPolicy.Deferred);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Styles {
    public static Fin<DraftReceipt> Commit(DocumentSession session, StyleTransaction plan) {
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

    public static Fin<StyleAnswer> Ask(DocumentSession session, StyleAsk request) {
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

## [05]-[ASK_FAMILY]

- Owner: `StyleAsk` `[Union]` — the catalog-backed read requests: whole-state snapshot, built-in-style census, swatch render, and unused-name minting; `StyleAnswer` `[Union]` — one typed result case per request; `StyleSetting` — one `(field, value)` read fact; `StyleSnapshot` — the one-pass definition read: identity, parentage, override census over schema rows, config projection, current-selection state, and rendered length units.
- Law: the snapshot's config projection is the schema fold — every verified `StyleField` row's `Read` delegate answers one `StyleSetting`, so a consumer never re-reads those host properties.
- Law: the swatch crosses as an owned lease — `CreatePreviewBitmap` acquires a native bitmap, the answer wraps it in `Lease<Bitmap>.Owned`, and the caller's disposal is the only release; a bare bitmap field is the deleted form.
- Law: the override census reads `IsFieldOverriden` (host single-`d` spelling) per schema row; `HasFieldOverrides` answers presence before the per-row sweep so an unoverridden style costs one probe.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAsk {
    private StyleAsk() { }
    public sealed record Snapshot(ResourceRef Target) : StyleAsk;
    public sealed record BuiltIns : StyleAsk;
    public sealed record Swatch(ResourceRef Target, System.Drawing.Size Size, bool Transparent = false) : StyleAsk;
    public sealed record MintName(string Root) : StyleAsk;

    internal Fin<StyleAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            snapshot: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from state in StyleSnapshot.Of(style: style, document: ctx.Document, key: ctx.Op)
                select (StyleAnswer)new StyleAnswer.State(Snapshot: state),
            builtIns: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<StyleAnswer>(value: new StyleAnswer.Rows(
                Styles: toSeq(ctx.Document.DimStyles.BuiltInStyles)
                    .Map(static style => (style.Id, style.Name, style.Index)),
                CurrentId: ctx.Document.DimStyles.CurrentId))),
            swatch: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from bitmap in ctx.Op.Catch(() => Optional(style.CreatePreviewBitmap(
                        width: ask.Size.Width, height: ask.Size.Height, transparent: ask.Transparent))
                    .ToFin(Fail: ctx.Op.InvalidResult()))
                select (StyleAnswer)new StyleAnswer.Rendered(Swatch: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
            mintName: static (ctx, ask) =>
                from root in ctx.Op.AcceptText(value: ask.Root)
                from minted in ctx.Op.AcceptText(value: ctx.Document.DimStyles.GetUnusedStyleName(rootName: root))
                select (StyleAnswer)new StyleAnswer.Minted(Name: minted));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAnswer : IDetachedDocumentResult {
    private StyleAnswer() { }
    public sealed record State(StyleSnapshot Snapshot) : StyleAnswer;
    public sealed record Rows(Seq<(Guid Key, string Name, int Index)> Styles, Guid CurrentId) : StyleAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Swatch) : StyleAnswer;
    public sealed record Minted(string Name) : StyleAnswer;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct StyleSetting(StyleField Field, StyleValue Value);

public sealed record StyleSnapshot(
    Guid Key,
    int Index,
    string Name,
    Option<Guid> Parent,
    bool IsChild,
    bool HasOverrides,
    Seq<StyleField> Overridden,
    Seq<StyleSetting> Settings,
    bool Current,
    UnitSystem LengthUnit,
    UnitSystem AlternateLengthUnit) : IDetachedDocumentResult {
    public static Fin<StyleSnapshot> Of(DimensionStyle style, RhinoDoc document, Op key) =>
        from active in Optional(style).ToFin(Fail: key.InvalidInput())
        from settings in toSeq(StyleField.Items)
            .TraverseM(row => key.Catch(() => row.Read(style: active, key: key))
                .Map(value => new StyleSetting(Field: row, Value: value)))
            .As()
        from snapshot in key.Catch(() => Fin.Succ(value: new StyleSnapshot(
            Key: active.Id,
            Index: active.Index,
            Name: active.Name,
            Parent: Optional(active.ParentId).Filter(static parent => parent != Guid.Empty),
            IsChild: active.IsChild,
            HasOverrides: active.HasFieldOverrides,
            Overridden: active.HasFieldOverrides
                ? toSeq(StyleField.Items).Filter(row => active.IsFieldOverriden(field: row.Host))
                : Seq<StyleField>(),
            Settings: settings,
            Current: document.DimStyles.CurrentId == active.Id,
            LengthUnit: active.DimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber),
            AlternateLengthUnit: active.AlternateDimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber))))
        select snapshot;
}
```

## [06]-[SPINE_AND_RECEIPTS]

- Owner: `DraftSpine` — the one commit kernel every Annotation rail walks: grant proof, redraw suppression with restoration on every exit, the shared `UndoBracket`, the receipt stamp, and the post-success redraw; `DraftSlot` `[SmartEnum<int>]` — the consequence vocabulary spanning every Annotation rail; `DraftBody` `[Union]` — the fact payloads; `DraftFact` — one slot-keyed fact; `DraftReceipt` — the additive fold with slot-keyed projections, the same fact-stream form the document and block rails carry.
- Law: the spine is the one bracket owner for the namespace — style, text, dimension, hatch, linetype, and section commits share this kernel verbatim, so undo, redraw, and grant semantics cannot drift between drafting rails; a rail re-spelling the demand/bracket/restore sequence is the deleted form.
- Law: one fact stream, kind-discriminated — component indexes, placed object ids, tallies, interchange paths, and undo serials are `DraftBody` cases on one record, never parallel receipt types; every projection is a `Choose` over the stream.
- Growth: a new consequence class is one slot row or one body case; every rail and every projection gains it for free.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DraftSlot {
    public static readonly DraftSlot Authored = new(key: 0);
    public static readonly DraftSlot Amended = new(key: 1);
    public static readonly DraftSlot Absorbed = new(key: 2);
    public static readonly DraftSlot Reparented = new(key: 3);
    public static readonly DraftSlot Current = new(key: 4);
    public static readonly DraftSlot Deleted = new(key: 5);
    public static readonly DraftSlot Revived = new(key: 6);
    public static readonly DraftSlot Scaled = new(key: 7);
    public static readonly DraftSlot Renamed = new(key: 8);
    public static readonly DraftSlot Imported = new(key: 9);
    public static readonly DraftSlot Exported = new(key: 10);
    public static readonly DraftSlot Loaded = new(key: 11);
    public static readonly DraftSlot Placed = new(key: 12);
    public static readonly DraftSlot Adjusted = new(key: 13);
    public static readonly DraftSlot Restyled = new(key: 14);
    public static readonly DraftSlot Reflowed = new(key: 15);
    public static readonly DraftSlot Reformulated = new(key: 16);
    public static readonly DraftSlot Bound = new(key: 17);
    public static readonly DraftSlot Undo = new(key: 18);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DraftBody {
    private DraftBody() { }
    public sealed record Component(int Index) : DraftBody;
    public sealed record Object(Guid Id) : DraftBody;
    public sealed record Tally(int Count) : DraftBody;
    public sealed record Path(string Value) : DraftBody;
    public sealed record Record(uint Serial) : DraftBody;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct DraftFact(DraftSlot Slot, DraftBody Body);

public readonly record struct DraftReceipt : IDetachedDocumentResult {
    private readonly Seq<DraftFact> facts;

    private DraftReceipt(Seq<DraftFact> facts) => this.facts = facts;

    public static DraftReceipt Empty { get; } = new(facts: Seq<DraftFact>());

    public Seq<DraftFact> Facts => facts;

    public static DraftReceipt operator +(DraftReceipt left, DraftReceipt right) =>
        new(facts: left.facts + right.facts);

    public static DraftReceipt Component(DraftSlot slot, int index) =>
        index >= 0 ? new(facts: Seq(new DraftFact(Slot: slot, Body: new DraftBody.Component(Index: index)))) : Empty;

    public static DraftReceipt Objects(DraftSlot slot, Seq<Guid> ids) =>
        new(facts: ids.Distinct().Filter(static id => id != Guid.Empty)
            .Map(id => new DraftFact(Slot: slot, Body: new DraftBody.Object(Id: id))));

    public static DraftReceipt Tally(DraftSlot slot, int count) =>
        count >= 0 ? new(facts: Seq(new DraftFact(Slot: slot, Body: new DraftBody.Tally(Count: count)))) : Empty;

    public static DraftReceipt Path(DraftSlot slot, string path) =>
        new(facts: Seq(new DraftFact(Slot: slot, Body: new DraftBody.Path(Value: path))));

    public static DraftReceipt UndoRecords(Seq<uint> serials) =>
        new(facts: serials.Filter(static serial => serial > 0u)
            .Map(serial => new DraftFact(Slot: DraftSlot.Undo, Body: new DraftBody.Record(Serial: serial))));

    public Seq<int> Components(DraftSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is DraftBody.Component body ? Some(body.Index) : Option<int>.None);

    public Seq<Guid> Ids(DraftSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is DraftBody.Object body ? Some(body.Id) : Option<Guid>.None);

    public Seq<int> Tallies(DraftSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is DraftBody.Tally body ? Some(body.Count) : Option<int>.None);

    public int FactCount(DraftSlot slot) => facts.Count(fact => fact.Slot == slot);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class DraftSpine {
    internal static Fin<DraftReceipt> Commit(
        DocumentSession session, string name, RedrawPolicy redraw, bool recording,
        Func<RhinoDoc, Fin<DraftReceipt>> run, Op op) =>
        from label in op.AcceptText(value: name)
        let needs = Seq(SessionNeed.Mutate)
            + (recording ? Seq(SessionNeed.Undo) : Seq<SessionNeed>())
            + (redraw.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>())
        from receipt in session.Demand(
            use: document => Run(document: document, name: label, redraw: redraw, recording: recording, run: run, op: op),
            key: op,
            needs: needs.ToArray())
        select receipt;

    private static Fin<DraftReceipt> Run(
        RhinoDoc document, string name, RedrawPolicy redraw, bool recording,
        Func<RhinoDoc, Fin<DraftReceipt>> run, Op op) =>
        from receipt in op.Catch(() => {
            bool priorRedraw = document.Views.RedrawEnabled;
            Fin<Unit> suppressed = op.Catch(() => {
                _ = Op.SideWhen(redraw.Suppress, () =>
                    document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false));
                return Fin.Succ(value: unit);
            });
            Fin<DraftReceipt> outcome = suppressed.Bind(_ => op.Catch(() => {
                using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recording);
                Fin<DraftReceipt> folded = guard(undo.Admitted, op.InvalidResult()).ToFin()
                    .Bind(_ => op.Catch(() => run(document)));
                return undo.Seal(
                    outcome: folded,
                    stamp: static (receipt, serial) => serial > 0u ? receipt + DraftReceipt.UndoRecords(serials: Seq(serial)) : receipt,
                    key: op);
            }));
            Fin<Unit> restored = op.Catch(() => {
                _ = Op.SideWhen(redraw.Suppress, () =>
                    document.Views.EnableRedraw(enable: priorRedraw, redrawDocument: false, redrawLayers: false));
                return Fin.Succ(value: unit);
            });
            return (outcome, restored).Apply(static (folded, _) => folded).As();
        })
        from _ in redraw.Enabled
            ? op.Catch(() => { document.Views.Redraw(deferred: redraw.Defers); return Fin.Succ(value: unit); })
            : Fin.Succ(value: unit)
        select receipt;
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]            | [FORM]                                              | [ENTRY]                               |
| :-----: | :--------------- | :----------------- | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | resource address | `ResourceRef`      | one union: id, name, index over a per-table lens    | `Of` / `Resolve(document, lens, key)` |
|  [02]   | unit vocabulary  | `LengthDisplayRow` | rows keyed on explicit host values, metric column   | `Host` projection                     |
|  [03]   | config schema    | `StyleField`       | one row per catalog-proven property/`Field` pairing | `Read` / `Write`                      |
|  [04]   | edit currency    | `StylePatch`       | kind-checked edit run with table and override folds | `Apply` / `Overlay`                   |
|  [05]   | style mutations  | `StyleOp`          | one flat `[Union]`, duplicate-then-`Modify` law     | `Styles.Commit`                       |
|  [06]   | style reads      | `StyleAsk`         | snapshot, built-ins, swatch lease, name mint        | `Styles.Ask`                          |
|  [07]   | commit kernel    | `DraftSpine`       | demand + undo bracket + redraw compensation         | `Commit(session, name, ...)`          |
|  [08]   | receipts         | `DraftReceipt`     | `DraftFact` stream + slot projections               | `Components` / `Ids` / `Tallies`      |
