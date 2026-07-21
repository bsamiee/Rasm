# [RASM_RHINO_ANNOTATION_STYLE]

`StyleField` is the drafting-schema authority: each row admits one exact payload family, reads and writes one catalogued `DimensionStyle.Field` pairing, and feeds the same patch fold into table styles and per-annotation overrides.

Document spine component address `ResourceRef` resolves every Annotation table through its per-table `ResourceLens<T>` row, while `DraftPlan`, `DraftSpine`, and `DraftReceipt` carry every drafting mutation through the Document grant, the shared `DocumentCommit.Sealed` envelope, and the detached fact rail.

## [01]-[INDEX]

- [02]-[ADDRESS_AND_VOCAB]: the `TableGrip<T>` revision law over the Document-owned `ResourceRef`/`ResourceLens<T>` address, plus explicit-value length-display rows.
- [03]-[FIELD_SCHEMA]: `StyleAxis`, `StyleValue`, exact-family `StyleField` rows, and the `StylePatch` fold.
- [04]-[STYLE_RAIL]: `StyleOp`, `DraftPlan<StyleOp>`, and the `Styles.Commit` entry over the shared spine.
- [05]-[ASK_FAMILY]: `StyleAsk`/`StyleAnswer` — snapshot, built-in census, swatch lease, and name minting.
- [06]-[SPINE_AND_RECEIPTS]: `DraftSpine`, `DraftSlot`, `DraftBody`, and the `DraftReceipt` monoid shared by every Annotation rail.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[ADDRESS_AND_VOCAB]

- Owner: `TableGrip<TComponent>` extends the Document spine's `ResourceRef`/`ResourceLens<TComponent>` component address (tables.md) with the table's index, duplicate, and modify rows and owns the one duplicate-then-`Modify` revision law every component table walks; the address family, its `ResourceId`/`ResourceName`/`ResourceIndex` scalars, and the sentinel projectors live on the Document spine, never re-declared here.
- Law: each Annotation table contributes one `ResourceLens<T>` row — style, linetype, hatch, and section each declare exactly one — and no rail mints a second address family.
- Law: the kernel `Op.AcceptValidated` receiver rows are the one host-enum admission bridge — every `[SmartEnum]` keyed on a host value admits through its generated `Validate` via the owning raw-shape row, so no vocabulary mints a private `Of`/`TryGet` wrapper and no folder carries a local bridge.
- Law: `ColorBoundary` owns the `PerceptualColor`↔`System.Drawing.Color` round trip; `TagBag` owns preflighted replacement, owner-derived snapshots, and compensating replay over the per-table user-string method groups; `TargetResolution.Only<TNative>` owns exactly-one object resolution with the typed cast probe.
- Law: `LengthDisplayRow` keys each host value explicitly, including the host spelling `Millmeters`.
- Boundary: resolution reads live per call inside the owning operation — tables mutate under commands, so no resolved component is cached on a value.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Specialized;
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
public sealed record TableGrip<TComponent>(
    ResourceLens<TComponent> Lens,
    DraftComponentKind Kind,
    Func<RhinoDoc, TComponent, int> Index,
    Func<TComponent, TComponent> Duplicate,
    Func<RhinoDoc, TComponent, int, bool, bool> Modify) where TComponent : class {
    internal Fin<DraftReceipt> Revised(
        ResourceRef target, RhinoDoc document, DraftSlot slot, WriteMode mode, Op op, Func<TComponent, Op, Fin<Unit>> revise) =>
        from live in target.Resolve(document: document, lens: Lens, key: op)
        let index = Index(document, live)
        from copy in op.Catch(() => Fin.Succ(value: Duplicate(live)))
        from _ in revise(copy, op)
        from __ in op.Confirm(success: Modify(document, copy, index, mode.QuietWrite))
        from receipt in DraftReceipt.Component(slot: slot, componentKind: Kind, index: ResourceIndex.Create(index))
        select receipt;
}

public static class ColorBoundary {
    extension(System.Drawing.Color color) {
        internal Fin<PerceptualColor> Admitted(Op key) =>
            PerceptualColor.OfRgb(red: color.R, green: color.G, blue: color.B, alpha: color.A, key: key);
    }

    extension(PerceptualColor color) {
        internal System.Drawing.Color Sys() =>
            color.ToRgb() switch {
                var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha, red, green, blue),
            };
    }
}

public static class TagBag {
    internal static Fin<Unit> Apply(HashMap<string, string> tags, Func<string, string, bool> set, Action clear, Op key) =>
        from admitted in toSeq(tags).TraverseM(pair =>
            from name in key.AcceptText(value: pair.Key)
            from value in key.AcceptText(value: pair.Value)
            select (Name: name, Value: value)).As()
        from original in Snapshot(set: set, key: key)
        from _ in Replay(tags: admitted, set: set, clear: clear, key: key).BindFail(primary =>
            Replay(
                tags: toSeq(original).Map(static pair => (Name: pair.Key, Value: pair.Value)),
                set: set,
                clear: clear,
                key: key).Match(
                    Succ: _ => Fin.Fail<Unit>(error: primary),
                    Fail: rollback => Fin.Fail<Unit>(error: primary + rollback)))
        select unit;

    internal static HashMap<string, string> Read(NameValueCollection native) =>
        toSeq(native.AllKeys)
            .Choose(name => Optional(name).Bind(tag => Optional(native[tag]).Map(value => (Key: tag, Value: value))))
            .Fold(HashMap<string, string>(), static (state, pair) => state.AddOrUpdate(key: pair.Key, value: pair.Value));

    private static Fin<HashMap<string, string>> Snapshot(Func<string, string, bool> set, Op key) => set.Target switch {
        DimensionStyle style => key.Catch(() => Fin.Succ(value: Read(style.GetUserStrings()))),
        HatchPattern pattern => key.Catch(() => Fin.Succ(value: Read(pattern.GetUserStrings()))),
        Linetype linetype => key.Catch(() => Fin.Succ(value: Read(linetype.GetUserStrings()))),
        _ => Fin.Fail<HashMap<string, string>>(error: key.MissingContext()),
    };

    private static Fin<Unit> Replay(
        Seq<(string Name, string Value)> tags,
        Func<string, string, bool> set,
        Action clear,
        Op key) =>
        from _ in key.Catch(clear)
        from __ in tags.Traverse(pair => key.Confirm(success: set(pair.Name, pair.Value)).ToValidation()).As().ToFin()
        select unit;
}

public static class TargetResolution {
    extension(TableTarget target) {
        internal Fin<(Guid Id, TNative Native)> Only<TNative>(RhinoDoc document, Op key) where TNative : RhinoObject =>
            from ids in target.Resolve(document: document, key: key)
            from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: key.InvalidInput()) }
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())
            from typed in Optional(native as TNative).ToFin(Fail: key.InvalidInput())
            select (id, typed);
    }
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

- Owner: `StyleField` is the keyed schema; each row carries its axis plus exact read, admission, and write delegates, while `StyleEdit` is the sole admitted field/payload pair.
- Law: enum payloads carry their CLR enum family beside the value; each `Pick<TEnum>` row accepts only its exact family and a declared member before any host cast.
- Law: `StylePatch` applies pre-admitted edits in order, stops on the first refused host write, and mints annotation overrides through `Overlay`.
- Law: color/plot-source `Field` cases from `ExtLineColorSource` through `DimLinePlotWeight_mm`, plus `MaskFlags`, `SignedOrdinate`, and `UnitSystem`, carry no CLR property on `DimensionStyle`; `Name` and `Index` cannot inherit from a parent. `StyleField` excludes every non-property case, and the override census reports schema rows alone.
- Law: each host setter marks its own override field, while `MaskOffset` binds `Field.MaskBorder`.
- Law: `Field.LeaderContentAngle` is a shared slot — `LeaderContentAngleType` reads it as `GetInt` and `LeaderTextRotationRadians`/`LeaderTextRotationDegrees` as `GetDouble`, while `Field.LeaderContentAngleStyle` binds no accessor — so the schema carries exactly one row for the field, the angle-style enum, and the rotation double stays off-schema; a second row keyed on the same field value is a duplicate-key fault at vocabulary materialization.
- Law: `ToleranceZeroSuppress` is an inert host stub — its getter returns the constant `ZeroSuppression.None`, its setter body is empty, and no `Field` case backs it — so the tolerance axis excludes it and no patch can claim tolerance zero suppression.
- Law: `Overlay` duplicates a nil-id child against the annotation's bound style, applies the patch, and attaches through `SetOverrideDimStyle`.
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleValue {
    private StyleValue() { }
    public sealed record Real(double Value) : StyleValue;
    public sealed record Whole(int Value) : StyleValue;
    public sealed record Choice : StyleValue {
        internal Choice(Enum value) => Value = value;
        public Type Family => Value.GetType();
        public Enum Value { get; }
    }
    public sealed record Flag(bool Value) : StyleValue;
    public sealed record Script(string Value) : StyleValue;
    public sealed record Tint(PerceptualColor Value) : StyleValue;
    public sealed record Anchor(Option<ResourceId> Value) : StyleValue;
    public sealed record Face(Font Value) : StyleValue;
    public sealed record Glyph(char Value) : StyleValue;

    public static StyleValue Of<TEnum>(TEnum value) where TEnum : struct, Enum =>
        new Choice(value: value);
}

[SmartEnum<int>]
public sealed partial class StyleField {
    // --- [ARROW]
    public static readonly StyleField ArrowType1 = Pick(DimensionStyle.Field.ArrowType1, StyleAxis.Arrow, static s => s.ArrowType1, static (s, v) => s.ArrowType1 = v);
    public static readonly StyleField ArrowType2 = Pick(DimensionStyle.Field.ArrowType2, StyleAxis.Arrow, static s => s.ArrowType2, static (s, v) => s.ArrowType2 = v);
    public static readonly StyleField LeaderArrowType = Pick(DimensionStyle.Field.LeaderArrowType, StyleAxis.Arrow, static s => s.LeaderArrowType, static (s, v) => s.LeaderArrowType = v);
    public static readonly StyleField ArrowLength = Real(DimensionStyle.Field.Arrowsize, StyleAxis.Arrow, static s => s.ArrowLength, static (s, v) => s.ArrowLength = v);
    public static readonly StyleField LeaderArrowLength = Real(DimensionStyle.Field.LeaderArrowsize, StyleAxis.Arrow, static s => s.LeaderArrowLength, static (s, v) => s.LeaderArrowLength = v);
    public static readonly StyleField ArrowBlockId1 = Anchor(DimensionStyle.Field.ArrowBlockId1, StyleAxis.Arrow, static s => s.ArrowBlockId1, static (s, v) => s.ArrowBlockId1 = v);
    public static readonly StyleField ArrowBlockId2 = Anchor(DimensionStyle.Field.ArrowBlockId2, StyleAxis.Arrow, static s => s.ArrowBlockId2, static (s, v) => s.ArrowBlockId2 = v);
    public static readonly StyleField LeaderArrowBlockId = Anchor(DimensionStyle.Field.LeaderArrowBlock, StyleAxis.Arrow, static s => s.LeaderArrowBlockId, static (s, v) => s.LeaderArrowBlockId = v);
    public static readonly StyleField ClippingArrowType1 = Pick(DimensionStyle.Field.ClippingArrowType1, StyleAxis.Arrow, static s => s.ClippingArrowType1, static (s, v) => s.ClippingArrowType1 = v);
    public static readonly StyleField ClippingArrowType2 = Pick(DimensionStyle.Field.ClippingArrowType2, StyleAxis.Arrow, static s => s.ClippingArrowType2, static (s, v) => s.ClippingArrowType2 = v);
    public static readonly StyleField ClippingArrowLength = Real(DimensionStyle.Field.ClippingArrowSize, StyleAxis.Arrow, static s => s.ClippingArrowLength, static (s, v) => s.ClippingArrowLength = v);
    public static readonly StyleField FitArrow = Pick(DimensionStyle.Field.ArrowFit, StyleAxis.Arrow, static s => s.FitArrow, static (s, v) => s.FitArrow = v);
    public static readonly StyleField SuppressArrow1 = Flag(DimensionStyle.Field.SuppressArrow1, StyleAxis.Arrow, static s => s.SuppressArrow1, static (s, v) => s.SuppressArrow1 = v);
    public static readonly StyleField SuppressArrow2 = Flag(DimensionStyle.Field.SuppressArrow2, StyleAxis.Arrow, static s => s.SuppressArrow2, static (s, v) => s.SuppressArrow2 = v);
    // --- [TEXT]
    public static readonly StyleField TextHeight = Real(DimensionStyle.Field.TextHeight, StyleAxis.Text, static s => s.TextHeight, static (s, v) => s.TextHeight = v);
    public static readonly StyleField TextGap = Real(DimensionStyle.Field.TextGap, StyleAxis.Text, static s => s.TextGap, static (s, v) => s.TextGap = v);
    public static readonly StyleField TextRotation = Real(DimensionStyle.Field.TextRotation, StyleAxis.Text, static s => s.TextRotation, static (s, v) => s.TextRotation = v);
    public static readonly StyleField TypeFace = Face(DimensionStyle.Field.Font, StyleAxis.Text, static s => s.Font, static (s, v) => s.Font = v);
    public static readonly StyleField TextVerticalAlignment = Pick(DimensionStyle.Field.TextVerticalAlignment, StyleAxis.Text, static s => s.TextVerticalAlignment, static (s, v) => s.TextVerticalAlignment = v);
    public static readonly StyleField TextHorizontalAlignment = Pick(DimensionStyle.Field.TextHorizontalAlignment, StyleAxis.Text, static s => s.TextHorizontalAlignment, static (s, v) => s.TextHorizontalAlignment = v);
    public static readonly StyleField TextOrientation = Pick(DimensionStyle.Field.TextOrientation, StyleAxis.Text, static s => s.TextOrientation, static (s, v) => s.TextOrientation = v);
    public static readonly StyleField LeaderTextOrientation = Pick(DimensionStyle.Field.LeaderTextOrientation, StyleAxis.Text, static s => s.LeaderTextOrientation, static (s, v) => s.LeaderTextOrientation = v);
    public static readonly StyleField DimTextOrientation = Pick(DimensionStyle.Field.DimTextOrientation, StyleAxis.Text, static s => s.DimTextOrientation, static (s, v) => s.DimTextOrientation = v);
    public static readonly StyleField DimRadialTextOrientation = Pick(DimensionStyle.Field.DimRadialTextOrientation, StyleAxis.Text, static s => s.DimRadialTextOrientation, static (s, v) => s.DimRadialTextOrientation = v);
    public static readonly StyleField DimTextLocation = Pick(DimensionStyle.Field.DimTextLocation, StyleAxis.Text, static s => s.DimTextLocation, static (s, v) => s.DimTextLocation = v);
    public static readonly StyleField DimRadialTextLocation = Pick(DimensionStyle.Field.DimRadialTextLocation, StyleAxis.Text, static s => s.DimRadialTextLocation, static (s, v) => s.DimRadialTextLocation = v);
    public static readonly StyleField DimTextAngleType = Pick(DimensionStyle.Field.DimTextAngleStyle, StyleAxis.Text, static s => s.DimTextAngleType, static (s, v) => s.DimTextAngleType = v);
    public static readonly StyleField DimRadialTextAngleType = Pick(DimensionStyle.Field.DimRadialTextAngleStyle, StyleAxis.Text, static s => s.DimRadialTextAngleType, static (s, v) => s.DimRadialTextAngleType = v);
    public static readonly StyleField FitText = Pick(DimensionStyle.Field.TextFit, StyleAxis.Text, static s => s.FitText, static (s, v) => s.FitText = v);
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
    public static readonly StyleField DimensionLengthDisplay = Pick(DimensionStyle.Field.DimensionLengthDisplay, StyleAxis.Length, static s => s.DimensionLengthDisplay, static (s, v) => s.DimensionLengthDisplay = v);
    public static readonly StyleField AlternateDimensionLengthDisplay = Pick(DimensionStyle.Field.AlternateDimensionLengthDisplay, StyleAxis.Length, static s => s.AlternateDimensionLengthDisplay, static (s, v) => s.AlternateDimensionLengthDisplay = v);
    public static readonly StyleField AngleFormat = Pick(DimensionStyle.Field.AngleFormat, StyleAxis.Length, static s => s.AngleFormat, static (s, v) => s.AngleFormat = v);
    public static readonly StyleField Roundoff = Real(DimensionStyle.Field.Round, StyleAxis.Length, static s => s.Roundoff, static (s, v) => s.Roundoff = v);
    public static readonly StyleField AlternateRoundoff = Real(DimensionStyle.Field.AltRound, StyleAxis.Length, static s => s.AlternateRoundoff, static (s, v) => s.AlternateRoundoff = v);
    public static readonly StyleField AngularRoundoff = Real(DimensionStyle.Field.AngularRound, StyleAxis.Length, static s => s.AngularRoundoff, static (s, v) => s.AngularRoundoff = v);
    public static readonly StyleField ZeroSuppress = Pick(DimensionStyle.Field.ZeroSuppress, StyleAxis.Length, static s => s.ZeroSuppress, static (s, v) => s.ZeroSuppress = v);
    public static readonly StyleField AlternateZeroSuppress = Pick(DimensionStyle.Field.AltZeroSuppress, StyleAxis.Length, static s => s.AlternateZeroSuppress, static (s, v) => s.AlternateZeroSuppress = v);
    public static readonly StyleField AngleZeroSuppress = Pick(DimensionStyle.Field.AngleZeroSuppress, StyleAxis.Length, static s => s.AngleZeroSuppress, static (s, v) => s.AngleZeroSuppress = v);
    public static readonly StyleField Prefix = Script(DimensionStyle.Field.Prefix, StyleAxis.Length, static s => s.Prefix, static (s, v) => s.Prefix = v);
    public static readonly StyleField Suffix = Script(DimensionStyle.Field.Suffix, StyleAxis.Length, static s => s.Suffix, static (s, v) => s.Suffix = v);
    public static readonly StyleField AlternatePrefix = Script(DimensionStyle.Field.AlternatePrefix, StyleAxis.Length, static s => s.AlternatePrefix, static (s, v) => s.AlternatePrefix = v);
    public static readonly StyleField AlternateSuffix = Script(DimensionStyle.Field.AlternateSuffix, StyleAxis.Length, static s => s.AlternateSuffix, static (s, v) => s.AlternateSuffix = v);
    public static readonly StyleField StackFractionFormat = Pick(DimensionStyle.Field.StackFormat, StyleAxis.Length, static s => s.StackFractionFormat, static (s, v) => s.StackFractionFormat = v);
    public static readonly StyleField StackHeightScale = Real(DimensionStyle.Field.StackTextheightScale, StyleAxis.Length, static s => s.StackHeightScale, static (s, v) => s.StackHeightScale = v);
    public static readonly StyleField AlternateUnitsDisplay = Flag(DimensionStyle.Field.Alternate, StyleAxis.Length, static s => s.AlternateUnitsDisplay, static (s, v) => s.AlternateUnitsDisplay = v);
    public static readonly StyleField AlternateBelowLine = Flag(DimensionStyle.Field.AltBelow, StyleAxis.Length, static s => s.AlternateBelowLine, static (s, v) => s.AlternateBelowLine = v);
    // --- [TOLERANCE]
    public static readonly StyleField ToleranceFormat = Pick(DimensionStyle.Field.ToleranceFormat, StyleAxis.Tolerance, static s => s.ToleranceFormat, static (s, v) => s.ToleranceFormat = v);
    public static readonly StyleField ToleranceResolution = Whole(DimensionStyle.Field.ToleranceResolution, StyleAxis.Tolerance, static s => s.ToleranceResolution, static (s, v) => s.ToleranceResolution = v);
    public static readonly StyleField AlternateToleranceResolution = Whole(DimensionStyle.Field.AltToleranceResolution, StyleAxis.Tolerance, static s => s.AlternateToleranceResolution, static (s, v) => s.AlternateToleranceResolution = v);
    public static readonly StyleField ToleranceHeightScale = Real(DimensionStyle.Field.ToleranceHeightScale, StyleAxis.Tolerance, static s => s.ToleranceHeightScale, static (s, v) => s.ToleranceHeightScale = v);
    public static readonly StyleField ToleranceUpperValue = Real(DimensionStyle.Field.ToleranceUpperValue, StyleAxis.Tolerance, static s => s.ToleranceUpperValue, static (s, v) => s.ToleranceUpperValue = v);
    public static readonly StyleField ToleranceLowerValue = Real(DimensionStyle.Field.ToleranceLowerValue, StyleAxis.Tolerance, static s => s.ToleranceLowerValue, static (s, v) => s.ToleranceLowerValue = v);
    // --- [MASK]
    public static readonly StyleField DrawTextMask = Flag(DimensionStyle.Field.DrawMask, StyleAxis.Mask, static s => s.DrawTextMask, static (s, v) => s.DrawTextMask = v);
    public static readonly StyleField MaskColor = Tint(DimensionStyle.Field.MaskColor, StyleAxis.Mask, static s => s.MaskColor, static (s, v) => s.MaskColor = v);
    public static readonly StyleField MaskColorSource = Pick(DimensionStyle.Field.MaskColorSource, StyleAxis.Mask, static s => s.MaskColorSource, static (s, v) => s.MaskColorSource = v);
    public static readonly StyleField MaskFrameType = Pick(DimensionStyle.Field.MaskFrameType, StyleAxis.Mask, static s => s.MaskFrameType, static (s, v) => s.MaskFrameType = v);
    public static readonly StyleField MaskOffset = Real(DimensionStyle.Field.MaskBorder, StyleAxis.Mask, static s => s.MaskOffset, static (s, v) => s.MaskOffset = v);
    // --- [LAYOUT]
    public static readonly StyleField BaselineSpacing = Real(DimensionStyle.Field.BaselineSpacing, StyleAxis.Layout, static s => s.BaselineSpacing, static (s, v) => s.BaselineSpacing = v);
    public static readonly StyleField DimensionScale = Real(DimensionStyle.Field.DimensionScale, StyleAxis.Layout, static s => s.DimensionScale, static (s, v) => s.DimensionScale = v);
    public static readonly StyleField CentermarkSize = Real(DimensionStyle.Field.Centermark, StyleAxis.Layout, static s => s.CentermarkSize, static (s, v) => s.CentermarkSize = v);
    public static readonly StyleField CenterMarkType = Pick(DimensionStyle.Field.CentermarkStyle, StyleAxis.Layout, static s => s.CenterMarkType, static (s, v) => s.CenterMarkType = v);
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
    public static readonly StyleField LeaderContentAngleType = Pick(DimensionStyle.Field.LeaderContentAngle, StyleAxis.Leader, static s => s.LeaderContentAngleType, static (s, v) => s.LeaderContentAngleType = v);
    public static readonly StyleField LeaderCurveType = Pick(DimensionStyle.Field.LeaderCurveType, StyleAxis.Leader, static s => s.LeaderCurveType, static (s, v) => s.LeaderCurveType = v);
    public static readonly StyleField LeaderTextVerticalAlignment = Pick(DimensionStyle.Field.LeaderTextVerticalAlignment, StyleAxis.Leader, static s => s.LeaderTextVerticalAlignment, static (s, v) => s.LeaderTextVerticalAlignment = v);
    public static readonly StyleField LeaderTextHorizontalAlignment = Pick(DimensionStyle.Field.LeaderTextHorizontalAlignment, StyleAxis.Leader, static s => s.LeaderTextHorizontalAlignment, static (s, v) => s.LeaderTextHorizontalAlignment = v);

    public StyleAxis Axis { get; }

    internal DimensionStyle.Field Host => (DimensionStyle.Field)Key;

    [UseDelegateFromConstructor]
    internal partial bool Accepts(StyleValue value);

    [UseDelegateFromConstructor]
    internal partial Fin<StyleValue> Read(DimensionStyle style, Op key);

    [UseDelegateFromConstructor]
    internal partial Unit Write(DimensionStyle style, StyleValue value);

    private static StyleField Real(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, double> get, Action<DimensionStyle, double> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Real(Value: value)),
            static value => ((StyleValue.Real)value).Value,
            static value => value is StyleValue.Real scalar && double.IsFinite(scalar.Value));

    private static StyleField Whole(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, int> get, Action<DimensionStyle, int> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Whole(Value: value)),
            static value => ((StyleValue.Whole)value).Value,
            static value => value is StyleValue.Whole);

    private static StyleField Pick<TEnum>(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, TEnum> get, Action<DimensionStyle, TEnum> set)
        where TEnum : struct, Enum =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: StyleValue.Of(value)),
            static value => (TEnum)((StyleValue.Choice)value).Value,
            static value => value is StyleValue.Choice { Value: TEnum member }
                && Enum.IsDefined(member));

    private static StyleField Flag(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, bool> get, Action<DimensionStyle, bool> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Flag(Value: value)),
            static value => ((StyleValue.Flag)value).Value,
            static value => value is StyleValue.Flag);

    private static StyleField Script(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, string> get, Action<DimensionStyle, string> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Script(Value: value)),
            static value => ((StyleValue.Script)value).Value,
            static value => value is StyleValue.Script script && script.Value is not null);

    private static StyleField Tint(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, System.Drawing.Color> get, Action<DimensionStyle, System.Drawing.Color> set) =>
        Of(field, axis, get, set,
            static (value, key) => value.Admitted(key).Map(static color => (StyleValue)new StyleValue.Tint(Value: color)),
            static value => ((StyleValue.Tint)value).Value.Sys(),
            static value => value is StyleValue.Tint tint && tint.Value is not null);

    private static StyleField Anchor(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Guid> get, Action<DimensionStyle, Guid> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Anchor(
                Value: Optional(value).Filter(static id => id != Guid.Empty).Map(ResourceId.Create))),
            static value => ((StyleValue.Anchor)value).Value.Map(static id => id.Value).IfNone(Guid.Empty),
            static value => value is StyleValue.Anchor);

    private static StyleField Face(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Font> get, Action<DimensionStyle, Font> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Face(Value: value)),
            static value => ((StyleValue.Face)value).Value,
            static value => value is StyleValue.Face face && face.Value is not null);

    private static StyleField Glyph(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, char> get, Action<DimensionStyle, char> set) =>
        Of(field, axis, get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Glyph(Value: value)),
            static value => ((StyleValue.Glyph)value).Value,
            static value => value is StyleValue.Glyph);

    private static StyleField Of<T>(
        DimensionStyle.Field field,
        StyleAxis axis,
        Func<DimensionStyle, T> get,
        Action<DimensionStyle, T> set,
        Func<T, Op, Fin<StyleValue>> wrap,
        Func<StyleValue, T> unwrap,
        Func<StyleValue, bool> accepts) =>
        new(
            key: (int)field,
            axis: axis,
            accepts: accepts,
            read: (style, key) =>
                from value in wrap(get(style), key)
                from _ in guard(accepts(value), key.InvalidResult()).ToFin()
                select value,
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

    public static Fin<StyleEdit> Of(StyleField? field, StyleValue? value, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedField in op.Need(value: field)
               from admittedValue in op.Need(value: value)
               from _ in guard(admittedField.Accepts(value: admittedValue), op.InvalidInput()).ToFin()
               select new StyleEdit(field: admittedField, value: admittedValue);
    }
}

public sealed record StylePatch {
    private StylePatch(Seq<StyleEdit> edits) => Edits = edits;

    public Seq<StyleEdit> Edits { get; }

    public static Fin<StylePatch> Of(params ReadOnlySpan<StyleEdit> edits) {
        Op op = Op.Of(name: nameof(StylePatch));
        Seq<StyleEdit> run = LanguageExt.Iterable<StyleEdit>.FromSpan(edits).ToSeq();
        return from admitted in run.TraverseM(edit => op.Need(value: edit)).As()
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new StylePatch(edits: admitted);
    }

    public Seq<StyleField> Marked => Edits.Map(static edit => edit.Field).Distinct();

    internal Fin<Unit> Apply(DimensionStyle style, Op key) =>
        Edits.TraverseM(edit => key.Catch(() => edit.Field.Write(style: style, value: edit.Value))).As().Map(static _ => unit);

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

- Owner: `StyleOp` `[Union]` — the mutation verbs over the `DimStyleTable`: authoring, patch amendment, whole-setting copy, override clearing, reverse absorption, reparenting, current selection, deletion, length scaling, the paper/model scale faces, and the user-string bag; `DraftPlan<StyleOp>` — the admitted commit plan; `Styles` — the `Commit`/`Ask` entry pair.
- Law: an amendment never mutates the resolved live component — every write duplicates it, applies its change to the copy, and lands through `DimStyleTable.Modify` by index inside the shared undo bracket.
- Law: `Author` refuses an existing name, shapes the detached style completely, and performs one terminal `Add`; a parent payload makes the authored style a child whose patch-marked fields alone override the parent through `ParentId`.
- Law: `DraftPlan<TOp>.Of` admits its mode and every operation before the shared commit spine can enter a document grant.
- Law: `Absorb` is the one reverse projection — `DimStyleTable.Modify(style, annotation)` folds a live annotation's per-instance overrides back onto the style, its `ModifyType` outcome inspected before the write counts: `Modify` and `Override` land as receipt facts, `NotSaved` is a typed refusal.
- Law: `Copy` projects every source setting through `DimensionStyle.CopyFrom` while preserving the target name, id, and index; `StyleTagEdit` closes set, delete, and clear under one mutation case without a sentinel key.
- Law: reclamation is not a case — unused-style reclaim is the document rail's `TableOp.Reclaim(TableKind.DimStyles)` row, and re-spelling it here splits one host member across two owners.
- Growth: a new style verb is one case with its arm; the spine, the receipt, and every consumer read it with zero new surface.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WriteMode {
    public static readonly WriteMode Quiet = new(key: 0, quietWrite: true);
    public static readonly WriteMode Notifying = new(key: 1, quietWrite: false);

    internal bool QuietWrite { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleTagEdit {
    private StyleTagEdit() { }
    public sealed record Set(StyleTag Tag) : StyleTagEdit;
    public sealed record Delete(StyleTagKey Key) : StyleTagEdit;
    public sealed record Clear : StyleTagEdit;

    internal Fin<Unit> Apply(DimensionStyle style, Op op) => Switch(
        (Style: style, Op: op),
        set: static (context, edit) =>
            from tag in context.Op.Need(value: edit.Tag)
            from _ in context.Op.Confirm(success: context.Style.SetUserString(key: tag.Key.Value, value: tag.Value))
            select unit,
        delete: static (context, edit) =>
            from key in context.Op.Need(value: edit.Key)
            from _ in context.Op.Confirm(success: context.Style.DeleteUserString(key: key.Value))
            select unit,
        clear: static (context, _) => context.Op.Catch(context.Style.DeleteAllUserStrings));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleOp {
    private StyleOp() { }
    public sealed record Author(ResourceName Name, StylePatch Patch, WriteMode Mode, Option<ResourceId> Parent = default) : StyleOp;
    public sealed record Amend(ResourceRef Target, StylePatch Patch, WriteMode Mode) : StyleOp;
    public sealed record Copy(ResourceRef Target, ResourceRef Source, WriteMode Mode) : StyleOp;
    public sealed record ClearOverrides(ResourceRef Target, Seq<StyleField> Fields, WriteMode Mode) : StyleOp;
    public sealed record Absorb(ResourceRef Target, TableTarget Annotation) : StyleOp;
    public sealed record Reparent(ResourceRef Target, Option<ResourceId> Parent, WriteMode Mode) : StyleOp;
    public sealed record SetCurrent(ResourceRef Target, WriteMode Mode) : StyleOp;
    public sealed record Delete(ResourceRef Target, WriteMode Mode) : StyleOp;
    public sealed record ScaleLengths(ResourceRef Target, double Factor, WriteMode Mode) : StyleOp;
    public sealed record PageScale(ResourceRef Target, double LeftMillimeters, double RightMillimeters, WriteMode Mode) : StyleOp;
    public sealed record Tag(ResourceRef Target, StyleTagEdit Edit, WriteMode Mode) : StyleOp;

    internal static readonly ResourceLens<DimensionStyle> Lens = new(
        ById: static (document, id) => document.DimStyles.Find(styleId: id, ignoreDeleted: true),
        ByName: static (document, name) => document.DimStyles.FindName(name: name),
        ByIndex: static (document, index) => document.DimStyles.FindIndex(index: index));

    internal static readonly TableGrip<DimensionStyle> Grip = new(
        Lens, DraftComponentKind.Style,
        Index: static (_, style) => style.Index,
        Duplicate: static style => style.Duplicate(),
        Modify: static (document, copy, index, quiet) => document.DimStyles.Modify(newSettings: copy, dimstyleIndex: index, quiet: quiet));

    internal Fin<DraftReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            author: static (context, edit) =>
                from _ in guard(context.Document.DimStyles.FindName(name: edit.Name.Value) is null, context.Op.InvalidInput()).ToFin()
                from shaped in context.Op.Catch(() => Fin.Succ(value: new DimensionStyle { Name = edit.Name.Value }))
                from __ in edit.Parent.Traverse(parent => context.Op.Catch(() => shaped.ParentId = parent.Value)).As()
                from ___ in edit.Patch.Apply(style: shaped, key: context.Op)
                from index in context.Op.Catch(() => ResourceIndex.Admit(
                    context.Document.DimStyles.Add(dimstyle: shaped, reference: false), context.Op))
                from receipt in DraftReceipt.Component(slot: DraftSlot.Authored, componentKind: DraftComponentKind.Style, index: index)
                select receipt,
            amend: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                    revise: (style, key) => edit.Patch.Apply(style: style, key: key)),
            copy: static (context, edit) =>
                from source in edit.Source.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from receipt in Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode,
                    op: context.Op, revise: (style, key) => key.Catch(() => style.CopyFrom(source)))
                select receipt,
            clearOverrides: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                    revise: (style, key) => edit.Fields.IsEmpty
                        ? key.Catch(style.ClearAllFieldOverrides)
                        : edit.Fields.TraverseM(field => key.Catch(() => style.ClearFieldOverride(field: field.Host))).As().Map(static _ => unit)),
            absorb: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from row in edit.Annotation.Only<AnnotationObjectBase>(document: context.Document, key: context.Op)
                from annotation in Optional(row.Native.AnnotationGeometry).ToFin(Fail: context.Op.InvalidInput())
                from outcome in context.Op.Catch(() => context.Document.DimStyles.Modify(dimstyle: style, annotation: annotation) switch {
                    ModifyType.Modify or ModifyType.Override => Fin.Succ(value: style.Index),
                    var refused => Fin.Fail<int>(error: context.Op.InvalidResult(detail: refused.ToString())),
                })
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Absorbed, componentKind: DraftComponentKind.Style, index: ResourceIndex.Create(outcome))
                select receipt,
            reparent: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Reparented, mode: edit.Mode, op: context.Op,
                    revise: (style, key) => key.Catch(() =>
                        style.ParentId = edit.Parent.Map(static parent => parent.Value).IfNone(noneValue: Guid.Empty))),
            setCurrent: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.SetCurrent(index: style.Index, quiet: edit.Mode.QuietWrite))
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Current, componentKind: DraftComponentKind.Style, index: ResourceIndex.Create(style.Index))
                select receipt,
            delete: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.Delete(index: style.Index, quiet: edit.Mode.QuietWrite))
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Deleted, componentKind: DraftComponentKind.Style, index: ResourceIndex.Create(style.Index))
                select receipt,
            scaleLengths: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Scaled, mode: edit.Mode, op: context.Op,
                    revise: (style, key) => key.Positive(value: edit.Factor)
                        .Bind(_ => key.Catch(() => style.ScaleLengthValues(scale: edit.Factor)))),
            pageScale: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Scaled, mode: edit.Mode, op: context.Op,
                    revise: (style, key) =>
                        from _ in key.Positive(value: edit.LeftMillimeters)
                        from __ in key.Positive(value: edit.RightMillimeters)
                        from ___ in key.Catch(() => {
                            style.ScaleLeftLengthMillimeters = edit.LeftMillimeters;
                            style.ScaleRightLengthMillimeters = edit.RightMillimeters;
                        })
                        select unit),
            tag: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, mode: edit.Mode, op: context.Op,
                    revise: (copy, key) => edit.Edit.Apply(style: copy, op: key)));
}

// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DraftMode {
    public static readonly DraftMode Recorded = new(key: 0, redraw: RedrawPolicy.Deferred, recordsUndo: true);
    public static readonly DraftMode Immediate = new(key: 1, redraw: RedrawPolicy.Immediate, recordsUndo: true);
    public static readonly DraftMode Unrecorded = new(key: 2, redraw: RedrawPolicy.Deferred, recordsUndo: false);

    internal RedrawPolicy Redraw { get; }
    internal bool RecordsUndo { get; }
}

public sealed record DraftPlan<TOp> where TOp : class {
    private DraftPlan(string name, DraftMode mode, Seq<TOp> operations) { Name = name; Mode = mode; Operations = operations; }

    public string Name { get; }
    public DraftMode Mode { get; }
    public Seq<TOp> Operations { get; }

    public static Fin<DraftPlan<TOp>> Of(string name, DraftMode mode, params ReadOnlySpan<TOp> operations) {
        Op op = Op.Of(name: nameof(DraftPlan<TOp>));
        Seq<TOp> run = LanguageExt.Iterable<TOp>.FromSpan(operations).ToSeq();
        return from label in op.AcceptText(value: name)
               from admittedMode in op.AcceptInput(value: mode)
               from admittedRun in run.TraverseM(operation => op.AcceptInput(value: operation)).As()
               from _ in guard(!admittedRun.IsEmpty, op.InvalidInput()).ToFin()
               select new DraftPlan<TOp>(name: label, mode: admittedMode, operations: admittedRun);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Styles {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<StyleOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key), op: Op.Of());

    public static Fin<StyleAnswer> Ask(DocumentSession session, StyleAsk request) {
        Op op = Op.Of();
        return from admitted in op.AcceptInput(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document, op: op), key: op, needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[ASK_FAMILY]

- Owner: `StyleAsk` `[Union]` — the catalog-backed read requests: whole-state snapshot, built-in-style census, swatch render, and default-or-rooted name minting; `StyleAnswer` `[Union]` — one typed result case per request; `StyleSetting` — one `(field, value)` read fact; `StyleTag` — one admitted user-string fact; `StyleSnapshot` — the one-pass definition read: identity, parentage, override census over schema rows, config projection, current-selection state, rendered length units, and user strings.
- Law: the snapshot's config projection is the schema fold — every verified `StyleField` row's `Read` delegate answers one `StyleSetting`, so a consumer never re-reads those host properties.
- Law: the swatch crosses as an owned lease — `CreatePreviewBitmap` acquires a native bitmap, the answer wraps it in `Lease<Bitmap>.Owned`, and the caller's disposal is the only release; a bare bitmap field is the deleted form.
- Law: `PreviewBudget` bounds each dimension and their overflow-safe pixel product before bitmap allocation.
- Law: the override census reads `IsFieldOverriden` (host single-`d` spelling) per schema row; `HasFieldOverrides` answers presence before the per-row sweep so an unoverridden style costs one probe.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAsk {
    private StyleAsk() { }
    public sealed record Snapshot(ResourceRef Target) : StyleAsk;
    public sealed record BuiltIns : StyleAsk;
    public sealed record Swatch(ResourceRef Target, PreviewSpec Preview) : StyleAsk;
    public sealed record MintName(Option<ResourceName> Root = default) : StyleAsk;

    internal Fin<StyleAnswer> Answer(RhinoDoc document, Op op) =>
        Switch(
            context: (Document: document, Op: op),
            snapshot: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from state in StyleSnapshot.Of(style: style, document: ctx.Document, key: ctx.Op)
                select (StyleAnswer)new StyleAnswer.State(Snapshot: state),
            builtIns: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<StyleAnswer>(value: new StyleAnswer.Rows(
                Styles: toSeq(ctx.Document.DimStyles.BuiltInStyles)
                    .Map(static style => new StyleRow(
                        Key: ResourceId.Create(style.Id),
                        Name: ResourceName.Create(style.Name),
                        Index: ResourceIndex.Create(style.Index))),
                CurrentId: ResourceId.Create(ctx.Document.DimStyles.CurrentId)))),
            swatch: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx.Document, lens: StyleOp.Lens, key: ctx.Op)
                from bitmap in ctx.Op.Catch(() => Optional(style.CreatePreviewBitmap(
                        width: ask.Preview.Width,
                        height: ask.Preview.Height,
                        transparent: ask.Preview.Surface.UsesTransparency))
                    .ToFin(Fail: ctx.Op.InvalidResult()))
                select (StyleAnswer)new StyleAnswer.Rendered(Swatch: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
            mintName: static (ctx, ask) =>
                from minted in ctx.Op.Catch(() => ctx.Op.AcceptText(value: ask.Root.Match(
                    Some: root => ctx.Document.DimStyles.GetUnusedStyleName(rootName: root.Value),
                    None: () => ctx.Document.DimStyles.GetUnusedStyleName())))
                select (StyleAnswer)new StyleAnswer.Minted(Name: ResourceName.Create(minted)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAnswer : IDetachedDocumentResult {
    private StyleAnswer() { }
    public sealed record State(StyleSnapshot Snapshot) : StyleAnswer;
    public sealed record Rows(Seq<StyleRow> Styles, ResourceId CurrentId) : StyleAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Swatch) : StyleAnswer;
    public sealed record Minted(ResourceName Name) : StyleAnswer;
}

// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PreviewSurface {
    public static readonly PreviewSurface Opaque = new(usesTransparency: false);
    public static readonly PreviewSurface Transparent = new(usesTransparency: true);
    internal bool UsesTransparency { get; }
}

[ComplexValueObject]
public sealed partial class PreviewBudget {
    public int MaxWidth { get; }
    public int MaxHeight { get; }
    public long MaxPixels { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref int maxWidth, ref int maxHeight, ref long maxPixels) {
        if (maxWidth <= 0 || maxHeight <= 0 || maxPixels <= 0)
            validationError = new ValidationError("Preview budget must be positive.");
    }
}

[ComplexValueObject]
public sealed partial class PreviewSpec {
    public int Width { get; }
    public int Height { get; }
    public PreviewSurface Surface { get; }
    public PreviewBudget Budget { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int width, ref int height, ref PreviewSurface surface, ref PreviewBudget budget) {
        if (width <= 0 || height <= 0 || surface is null || budget is null
            || width > budget.MaxWidth || height > budget.MaxHeight || width > budget.MaxPixels / height)
            validationError = new ValidationError("Preview dimensions exceed the admitted budget.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class StyleTagKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) validationError = new ValidationError("Style tag key is required.");
    }
}

[ComplexValueObject]
public sealed partial class StyleTag {
    public StyleTagKey Key { get; }
    public string Value { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref StyleTagKey key, ref string value) {
        if (key is null || value is null) validationError = new ValidationError("Style tag is incomplete.");
    }
}

public readonly record struct StyleSetting(StyleField Field, StyleValue Value);
public readonly record struct StyleRow(ResourceId Key, ResourceName Name, ResourceIndex Index);

public sealed record StyleSnapshot(
    ResourceId Key,
    ResourceIndex Index,
    ResourceName Name,
    Option<ResourceId> Parent,
    ResourceId Root,
    bool IsChild,
    bool HasOverrides,
    Seq<StyleField> Overridden,
    Seq<StyleSetting> Settings,
    Seq<StyleTag> Tags,
    int TagCount,
    bool Current,
    double ScaleValue,
    UnitSystem LengthUnit,
    UnitSystem AlternateLengthUnit) : IDetachedDocumentResult {
    public static Fin<StyleSnapshot> Of(DimensionStyle style, RhinoDoc document, Op key) =>
        from active in Optional(style).ToFin(Fail: key.InvalidInput())
        from settings in toSeq(StyleField.Items)
            .TraverseM(row => key.Catch(() => row.Read(style: active, key: key))
                .Map(value => new StyleSetting(Field: row, Value: value)))
            .As()
        from root in key.Catch(() => Optional(document.DimStyles.FindRoot(styleId: active.Id, ignoreDeleted: true))
            .ToFin(Fail: key.InvalidResult()))
        from snapshot in key.Catch(() => Fin.Succ(value: new StyleSnapshot(
            Key: ResourceId.Create(active.Id),
            Index: ResourceIndex.Create(active.Index),
            Name: ResourceName.Create(active.Name),
            Parent: ResourceId.Maybe(active.ParentId),
            Root: ResourceId.Create(root.Id),
            IsChild: active.IsChild,
            HasOverrides: active.HasFieldOverrides,
            Overridden: active.HasFieldOverrides
                ? toSeq(StyleField.Items).Filter(row => active.IsFieldOverriden(field: row.Host))
                : Seq<StyleField>(),
            Settings: settings,
            Tags: toSeq(TagBag.Read(active.GetUserStrings())).Map(static pair =>
                StyleTag.Create(key: StyleTagKey.Create(pair.Key), value: pair.Value)),
            TagCount: active.UserStringCount,
            Current: document.DimStyles.CurrentId == active.Id,
            ScaleValue: active.DimensionScaleValue,
            LengthUnit: active.DimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber),
            AlternateLengthUnit: active.AlternateDimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber))))
        select snapshot;
}
```

## [06]-[SPINE_AND_RECEIPTS]

- Owner: `DraftSpine` — the one Annotation commit entry: it derives its needs through `SessionNeed.Mutation`, demands once, and commits through the Document spine's `DocumentCommit.Sealed` envelope with the `DraftReceipt` fold and undo-serial stamp as its carrier; `DraftSlot` `[SmartEnum<int>]` — the consequence vocabulary with its admitted body kinds; `DraftBody` `[Union]` — the typed fact payloads; `DraftFact` — one validated slot/body pairing; `DraftReceipt` — the compositional fact stream shared by every Annotation rail.
- Law: the spine is the one commit entry for the namespace — style, text, dimension, hatch, linetype, and section commits share it verbatim, so undo, redraw, and grant semantics cannot drift between drafting rails; a rail re-spelling the demand/envelope sequence, or opening `UndoBracket.Begin` beside `Sealed`, is the deleted form.
- Law: `DocumentCommit.Compensated` is the one compensating-transaction fold — land each element, roll back every landed key on the first refusal, settle source custody through its release policy on every outcome, preserve the initiating fault, and append rollback and release faults in order; a rail re-typing this fold or spelling a caller-local release cascade beside it is the deleted form.
- Law: one fact stream, kind-discriminated — `DraftSlot` declares every legal payload kind, `DraftFact.Of` rejects an illegal cross-product, and every projection is a `Choose` over the stream.
- Growth: a new consequence class is one slot row or one body case; every rail and every projection gains it for free.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DraftBodyKind {
    public static readonly DraftBodyKind Component = new();
    public static readonly DraftBodyKind Object = new();
    public static readonly DraftBodyKind Tally = new();
    public static readonly DraftBodyKind Path = new();
    public static readonly DraftBodyKind Record = new();
}

[SmartEnum]
public sealed partial class DraftComponentKind {
    public static readonly DraftComponentKind Style = new();
    public static readonly DraftComponentKind Section = new();
    public static readonly DraftComponentKind Hatch = new();
    public static readonly DraftComponentKind Linetype = new();
}

[SmartEnum<int>]
public sealed partial class DraftSlot {
    public static readonly DraftSlot Authored = new(key: 0, bodies: Seq(DraftBodyKind.Component));
    public static readonly DraftSlot Amended = new(key: 1, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Object));
    public static readonly DraftSlot Absorbed = new(key: 2, bodies: Seq(DraftBodyKind.Component));
    public static readonly DraftSlot Reparented = new(key: 3, bodies: Seq(DraftBodyKind.Component));
    public static readonly DraftSlot Current = new(key: 4, bodies: Seq(DraftBodyKind.Component));
    public static readonly DraftSlot Deleted = new(key: 5, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Object));
    public static readonly DraftSlot Revived = new(key: 6, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Object));
    public static readonly DraftSlot Scaled = new(key: 7, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Object));
    public static readonly DraftSlot Renamed = new(key: 8, bodies: Seq(DraftBodyKind.Component));
    public static readonly DraftSlot Imported = new(key: 9, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Tally, DraftBodyKind.Path));
    public static readonly DraftSlot Exported = new(key: 10, bodies: Seq(DraftBodyKind.Tally, DraftBodyKind.Path));
    public static readonly DraftSlot Loaded = new(key: 11, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Tally));
    public static readonly DraftSlot Placed = new(key: 12, bodies: Seq(DraftBodyKind.Object));
    public static readonly DraftSlot Adjusted = new(key: 13, bodies: Seq(DraftBodyKind.Object));
    public static readonly DraftSlot Restyled = new(key: 14, bodies: Seq(DraftBodyKind.Object));
    public static readonly DraftSlot Reflowed = new(key: 15, bodies: Seq(DraftBodyKind.Object));
    public static readonly DraftSlot Reformulated = new(key: 16, bodies: Seq(DraftBodyKind.Object));
    public static readonly DraftSlot Bound = new(key: 17, bodies: Seq(DraftBodyKind.Component, DraftBodyKind.Object));
    public static readonly DraftSlot Undo = new(key: 18, bodies: Seq(DraftBodyKind.Record));

    internal Seq<DraftBodyKind> Bodies { get; }
    internal bool Accepts(DraftBodyKind body) => Bodies.Contains(body);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DraftBody {
    private DraftBody() { }
    public sealed record Component(DraftComponentKind ComponentKind, ResourceIndex Index) : DraftBody;
    public sealed record Object(ResourceId Id) : DraftBody;
    public sealed record Tally(DraftCount Count) : DraftBody;
    public sealed record Path(DraftPath Value) : DraftBody;
    public sealed record Record(UndoSerial Serial) : DraftBody;

    internal DraftBodyKind Kind => this switch {
        Component _ => DraftBodyKind.Component,
        Object _ => DraftBodyKind.Object,
        Tally _ => DraftBodyKind.Tally,
        Path _ => DraftBodyKind.Path,
        Record _ => DraftBodyKind.Record,
    };
}

// --- [MODELS] -------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class DraftCount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 0) validationError = new ValidationError("Draft count cannot be negative.");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class DraftPath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        if (value.Length == 0) validationError = new ValidationError("Draft path is required.");
    }
}

[ValueObject<uint>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class UndoSerial {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) {
        if (value == 0u) validationError = new ValidationError("Undo serial must be positive.");
    }
}

public sealed record DraftFact {
    private DraftFact(DraftSlot slot, DraftBody body) { Slot = slot; Body = body; }
    public DraftSlot Slot { get; }
    public DraftBody Body { get; }

    internal static Fin<DraftFact> Of(DraftSlot slot, DraftBody body, Op key) =>
        from admittedSlot in Optional(slot).ToFin(Fail: key.InvalidInput())
        from admittedBody in Optional(body).ToFin(Fail: key.InvalidInput())
        from _ in guard(admittedSlot.Accepts(admittedBody.Kind), key.InvalidInput()).ToFin()
        select new DraftFact(slot: admittedSlot, body: admittedBody);

    internal static DraftFact Undo(UndoSerial serial) => new(
        slot: DraftSlot.Undo,
        body: new DraftBody.Record(Serial: serial));
}

public readonly record struct DraftReceipt : IDetachedDocumentResult {
    private readonly Seq<DraftFact> facts;

    private DraftReceipt(Seq<DraftFact> facts) => this.facts = facts;

    public static DraftReceipt Empty { get; } = new(facts: Seq<DraftFact>());

    public Seq<DraftFact> Facts => facts;

    public DraftReceipt Contribute(DraftReceipt contribution) =>
        new(facts: facts + contribution.facts);

    public static Fin<DraftReceipt> Component(DraftSlot slot, DraftComponentKind componentKind, ResourceIndex index) =>
        Of(slot: slot, body: new DraftBody.Component(ComponentKind: componentKind, Index: index));

    public static Fin<DraftReceipt> Objects(DraftSlot slot, Seq<ResourceId> ids) =>
        ids.Distinct()
            .Traverse(id => Of(slot: slot, body: new DraftBody.Object(Id: id)).ToValidation())
            .As()
            .ToFin()
            .Map(static receipts => receipts.Fold(Empty, static (state, next) => state.Contribute(next)));

    public static Fin<DraftReceipt> Tally(DraftSlot slot, DraftCount count) =>
        Of(slot: slot, body: new DraftBody.Tally(Count: count));

    public static Fin<DraftReceipt> Path(DraftSlot slot, DraftPath path) =>
        Of(slot: slot, body: new DraftBody.Path(Value: path));

    public static DraftReceipt UndoRecord(UndoSerial serial) =>
        new(facts: Seq(DraftFact.Undo(serial: serial)));

    private static Fin<DraftReceipt> Of(DraftSlot slot, DraftBody body) {
        Op op = Op.Of();
        return DraftFact.Of(slot: slot, body: body, key: op)
            .Map(fact => new DraftReceipt(facts: Seq(fact)));
    }

    public Seq<ResourceIndex> Components(DraftSlot slot, DraftComponentKind componentKind) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(fact => fact.Body is DraftBody.Component body && body.ComponentKind == componentKind
                ? Some(body.Index)
                : Option<ResourceIndex>.None);

    public Seq<ResourceId> Ids(DraftSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is DraftBody.Object body ? Some(body.Id) : Option<ResourceId>.None);

    public Seq<DraftCount> Tallies(DraftSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is DraftBody.Tally body ? Some(body.Count) : Option<DraftCount>.None);

    public int FactCount(DraftSlot slot) => facts.Count(fact => fact.Slot == slot);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class DraftSpine {
    internal static Fin<DraftReceipt> Commit<TOp>(
        DocumentSession session, DraftPlan<TOp> plan,
        Func<RhinoDoc, TOp, Op, Fin<DraftReceipt>> apply, Op op) where TOp : class =>
        session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: plan.Name,
                recordsUndo: plan.Mode.RecordsUndo,
                redraw: plan.Mode.Redraw,
                run: () => plan.Operations.TraverseM(operation => apply(document, operation, op)).As()
                    .Map(static receipts => receipts.Fold(DraftReceipt.Empty, static (state, next) => state.Contribute(next))),
                stamp: static (receipt, serial) => serial > 0u
                    ? receipt.Contribute(DraftReceipt.UndoRecord(serial: UndoSerial.Create(serial)))
                    : receipt,
                op: op),
            key: op,
            needs: SessionNeed.Mutation(undo: plan.Mode.RecordsUndo, redraw: plan.Mode.Redraw).ToArray());
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]            | [FORM]                                                | [ENTRY]                               |
| :-----: | :----------------- | :----------------- | :---------------------------------------------------- | :------------------------------------ |
|  [01]   | table revision     | `TableGrip<T>`     | Document-owned lens + index/duplicate/modify rows     | `Revised(target, ...)`                |
|  [02]   | color boundary     | `ColorBoundary`    | `PerceptualColor`/`System.Drawing.Color` round trip   | `Admitted` / `Sys`                    |
|  [03]   | user-string bag    | `TagBag`           | compensated whole-bag replacement                     | `Apply` / `Read`                      |
|  [04]   | object singleton   | `TargetResolution` | exactly-one id + typed cast probe on `TableTarget`    | `Only<TNative>`                       |
|  [05]   | unit vocabulary    | `LengthDisplayRow` | rows keyed on explicit host values, metric column     | `Host` projection                     |
|  [06]   | config schema      | `StyleField`       | one row per catalog-proven property/`Field` pairing   | `Read` / `Write`                      |
|  [07]   | edit currency      | `StylePatch`       | exact-family edit run with table and override folds   | `Apply` / `Overlay`                   |
|  [08]   | style mutations    | `StyleOp`          | one flat `[Union]`, duplicate-then-`Modify` law       | `Styles.Commit`                       |
|  [09]   | style reads        | `StyleAsk`         | closed request/answer family                           | `Styles.Ask`                          |
|  [10]   | commit entry       | `DraftSpine`       | `Sealed` composition over the `DraftReceipt` fold     | `Commit`                              |
|  [11]   | receipts           | `DraftReceipt`     | `DraftFact` stream + slot projections                 | `Components` / `Ids` / `Tallies`      |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
