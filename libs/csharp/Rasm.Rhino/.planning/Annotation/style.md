# [RASM_RHINO_ANNOTATION_STYLE]

`StyleField` is the drafting-schema authority: each row admits one exact payload family, reads and writes one catalogued `DimensionStyle.Field` pairing, and feeds the same patch fold into table styles and per-annotation overrides.

Document spine component address `ResourceRef` resolves every Annotation table through its per-table `ResourceLens<T>` row, while `DraftPlan`, `DraftSpine`, and `DraftReceipt` carry every drafting mutation through the Document grant, the shared `DocumentCommit.Sealed` envelope, and the detached fact rail.

## [01]-[INDEX]

- [02]-[ADDRESS_AND_VOCAB]: the `TableGrip<T>` revision law over the Document-owned `ResourceRef`/`ResourceLens<T>` address, the `DraftCustody` custody fold and `DraftBorrow` input seam, the shared drafting scalars, and explicit-value length-display rows.
- [03]-[FIELD_SCHEMA]: `StyleAxis`, `StyleValue`, exact-family `StyleField` rows with flags-aware pick admission, and the `StylePatch` fold that re-derives an override child from its construction parent.
- [04]-[STYLE_RAIL]: `StyleOp`, `DraftPlan<StyleOp>`, and the `Styles.Commit` entry over the shared spine.
- [05]-[ASK_FAMILY]: `StyleAsk`/`StyleAnswer` — snapshot, built-in census, swatch lease, and name minting.
- [06]-[SPINE_AND_RECEIPTS]: `DraftSpine`, `DraftSlot`, `DraftBody`, and the `DraftReceipt` monoid shared by every Annotation rail.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[ADDRESS_AND_VOCAB]

- Owner: `TableGrip<TComponent>` extends the Document spine's `ResourceRef`/`ResourceLens<TComponent>` component address (tables.md) with the table's index, duplicate, and modify rows and owns the one duplicate-then-`Modify` revision law every component table walks; the address family, its `ResourceId`/`ResourceName`/`ResourceIndex` scalars, and the sentinel projectors live on the Document spine, never re-declared here.
- Law: each Annotation table contributes one `ResourceLens<T>` row — style, linetype, hatch, and section each declare exactly one — and no rail mints a second address family.
- Law: the kernel `Op.AcceptValidated` receiver rows are the one host-enum admission bridge — every `[SmartEnum]` keyed on a host value admits through its generated `Validate` via the owning raw-shape row, so no vocabulary mints a private `Of`/`TryGet` wrapper and no folder carries a local bridge.
- Law: `DraftCustody` is the namespace's ONE native-custody fold — `Release` accumulates every disposer fault, `Failed` folds a primary fault with its cleanup, and `Crossed` lands a raw host batch on `GeometryHandle` custody through the Document crossing with both source and landed release policies — so no rail declares a second copy and every mint-then-write and detach path in Annotation settles through it.
- Law: `DraftScale`, `DraftAngle`, and `DraftWeight` are the namespace's drafting quantity owners — pattern and boundary scales, radian rotations, and millimetre plot weights admit once here and compose from every drafting page, so no page re-mints a scalar owner for a host property another already owns and no rail publishes the same quantity as a bare `double`.
- Law: `DraftBorrow` is the one input-custody seam — a `GeometryHandle` argument projects its live native inside one lease scope through `Typed`, and a handle spread nests one scope per member, so a public drafting payload names custody and never a raw `Curve` or `Brep`.
- Law: `TableGrip.Revised` releases its duplicate on all three paths — revise refusal, `Modify` refusal, and success — because `Modify` copies settings into the table row and leaves the duplicate the caller's; the released-on-refusal-only shape leaks one native per successful amendment.
- Law: `ColorBoundary` owns the `PerceptualColor`↔`System.Drawing.Color` round trip; `TagSurface` binds one tagged component's three re-published user-string members and `TagBag` owns preflighted replacement and compensating replay over it — the host keeps that surface `internal` on `CommonObject`, so the seam is the argument and never a reflected delegate receiver; `TargetResolution.Only<TNative>` owns exactly-one object resolution with the typed cast probe.
- Law: `LengthDisplayRow` keys each host value explicitly, including the host spelling `Millmeters`.
- Boundary: resolution reads live per call inside the owning operation — tables mutate under commands, so no resolved component is cached on a value.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Specialized;
using System.Globalization;
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] --------------------------------------------------------------------------------
public static class DraftCustody {
    internal static Fin<Unit> Release<T>(Seq<T> values, Op op) where T : class, IDisposable =>
        values.Traverse(value => op.Catch(() => Fin.Succ(value: Op.Side(value.Dispose))).ToValidation())
            .As().ToFin().Map(static _ => unit);

    internal static Fin<TValue> Failed<TValue, TResource>(Error primary, Seq<TResource> values, Op op)
        where TResource : class, IDisposable =>
        Release(values: values, op: op).Match(
            Succ: _ => Fin.Fail<TValue>(error: primary),
            Fail: cleanup => Fin.Fail<TValue>(error: primary + cleanup));

    internal static Fin<Seq<GeometryHandle>> Crossed<TGeometry>(Seq<TGeometry> products, Op op)
        where TGeometry : GeometryBase =>
        DocumentCommit.Compensated(
            source: products,
            land: product => GeometryCrossing.Cross(source: product, mode: CrossingMode.Detach, key: op),
            rollback: landed => Release(values: landed, op: op),
            release: sources => Release(values: sources, op: op));
}

public sealed record TableGrip<TComponent>(
    ResourceLens<TComponent> Lens,
    DraftComponentKind Kind,
    Func<RhinoDoc, TComponent, int> Index,
    Func<TComponent, TComponent> Duplicate,
    Func<RhinoDoc, TComponent, int, bool, bool> Modify) where TComponent : class, IDisposable {
    // `Modify` copies the duplicate's settings into the table row and leaves the native this rail's own, so success
    // releases it exactly as both refusal legs do.
    internal Fin<DraftReceipt> Revised(
        ResourceRef target, RhinoDoc document, DraftSlot slot, HostInteraction interaction, Op op, Func<TComponent, Op, Fin<Unit>> revise) =>
        from live in target.Resolve(document: document, lens: Lens, key: op)
        let index = Index(document, live)
        from copy in op.Catch(() => Fin.Succ(value: Duplicate(live)))
        from _ in revise(copy, op)
            .BindFail(primary => DraftCustody.Failed<Unit, TComponent>(primary: primary, values: Seq(copy), op: op))
        from __ in op.Confirm(success: Modify(document, copy, index, interaction.IsQuiet))
            .BindFail(primary => DraftCustody.Failed<Unit, TComponent>(primary: primary, values: Seq(copy), op: op))
        from ___ in DraftCustody.Release(values: Seq(copy), op: op)
        from receipt in DraftReceipt.Component(
            slot: slot, componentKind: Kind, index: ResourceIndex.Create(index), key: op)
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

// `CommonObject` keeps the user-string surface `internal` and every tagged component re-publishes it, so no base
// member and no interface spans the family. `TagSurface` is that missing seam: the OWNER binds its own three
// members once at the call site, and the snapshot reader is an argument rather than a reflected delegate `Target`
// — a static method group has a null `Target`, and a type-test roster silently strands a fourth tagged component.
public readonly record struct TagSurface(
    Func<NameValueCollection> Read,
    Func<string, string, bool> Set,
    Action Clear);

public static class TagBag {
    internal static Fin<Unit> Apply(HashMap<string, string> tags, TagSurface owner, Op key) =>
        from admitted in toSeq(tags.AsIterable()).Traverse(pair =>
            (from name in key.AcceptText(value: pair.Key)
             from value in key.AcceptText(value: pair.Value)
             select (Name: name, Value: value)).ToValidation()).As().ToFin()
        from original in key.Catch(() => Fin.Succ(value: TagOp.Snapshot(owner.Read())))
        from _ in Replay(tags: admitted, owner: owner, key: key).BindFail(primary =>
            Replay(
                tags: toSeq(original).Map(static pair => (Name: pair.Key, Value: pair.Value)),
                owner: owner,
                key: key).Match(
                    Succ: _ => Fin.Fail<Unit>(error: primary),
                    Fail: rollback => Fin.Fail<Unit>(error: primary + rollback)))
        select unit;

    private static Fin<Unit> Replay(Seq<(string Name, string Value)> tags, TagSurface owner, Op key) =>
        from _ in key.Catch(owner.Clear)
        from __ in tags.Traverse(pair => key.Confirm(success: owner.Set(pair.Name, pair.Value)).ToValidation()).As().ToFin()
        select unit;
}

public static class TargetResolution {
    extension(TableTarget target) {
        internal Fin<(Guid Id, TNative Native)> Only<TNative>(RhinoDoc document, Op key) where TNative : RhinoObject =>
            from ids in target.Resolve(document: document, key: key)
            from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: key.InvalidInput()) }
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: key.MissingContext())
            from typed in key.Need(native as TNative)
            select (id, typed);
    }
}

// A handle's native lives only inside its lease scope, so a spread nests one scope per member and hands the whole
// borrowed run to one continuation — flattening the run out of the scopes publishes natives the leases already closed.
public static class DraftBorrow {
    extension(GeometryHandle handle) {
        internal Fin<TResult> Typed<TNative, TResult>(Op key, Func<TNative, Fin<TResult>> project)
            where TNative : GeometryBase =>
            handle.With(key: key, project: native => Optional(native as TNative)
                .ToFin(Fail: key.InvalidInput())
                .Bind(project));
    }

    extension(Seq<GeometryHandle> handles) {
        internal Fin<TResult> Typed<TNative, TResult>(Op key, Func<Seq<TNative>, Fin<TResult>> project)
            where TNative : GeometryBase =>
            handles.Head.Match(
                Some: head => head.Typed<TNative, TResult>(key: key, project: native =>
                    handles.Tail.Typed<TNative, TResult>(key: key, project: rest => project(Seq(native) + rest))),
                None: () => project(Seq<TNative>()));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class DraftScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value <= 0.0) validationError = new ValidationError("Draft scale must be finite and positive.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class DraftAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value)) validationError = new ValidationError("Draft angle must be finite.");
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class DraftWeight {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) {
        if (!double.IsFinite(value) || value < 0.0) validationError = new ValidationError("Draft plot weight must be finite and non-negative.");
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

- Owner: `StyleField` is the keyed schema; each row carries its axis with exact read, admission, and write delegates, while `StyleEdit` is the sole admitted field/payload pair.
- Law: enum payloads carry their CLR enum family beside the value; each `Pick<TEnum>` row accepts only its exact family and a declared member before any host cast.
- Law: `StylePatch.Of` accumulates its edit admission while `Apply` stops on the first refused host write, and `Overlay` mints annotation overrides — the two folds answer different questions, so they carry different failure algebras.
- Law: color/plot-source `Field` cases from `ExtLineColorSource` through `DimLinePlotWeight_mm`, with `MaskFlags`, `SignedOrdinate`, and `UnitSystem`, carry no CLR property on `DimensionStyle`; `Name` and `Index` cannot inherit from a parent. `StyleField` excludes every non-property case, and the override census reports schema rows alone.
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
            static value => value is StyleValue.Choice { Value: TEnum member } && Admits(member));

    // `DimensionStyle.ZeroSuppression` and every other `[Flags]` host row admit composites `Enum.IsDefined` refuses,
    // so the gate runs a mask-subset test on flag-typed rows and stays exact-membership on the closed ones.
    private static bool Admits<TEnum>(TEnum member) where TEnum : struct, Enum =>
        typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false)
            ? Enum.GetValues<TEnum>().Aggregate(default(TEnum), Or) is var mask
                && (ToBits(member) & ~ToBits(mask)) is 0UL
            : Enum.IsDefined(member);

    private static TEnum Or<TEnum>(TEnum left, TEnum right) where TEnum : struct, Enum =>
        (TEnum)Enum.ToObject(typeof(TEnum), ToBits(left) | ToBits(right));

    private static ulong ToBits<TEnum>(TEnum member) where TEnum : struct, Enum =>
        Convert.ToUInt64(member, CultureInfo.InvariantCulture);

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
        return from admitted in run.Traverse(edit => op.Need(value: edit).ToValidation()).As().ToFin()
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new StylePatch(edits: admitted);
    }

    internal Fin<Unit> Apply(DimensionStyle style, Op key) =>
        Edits.TraverseM(edit => key.Catch(() => edit.Field.Write(style: style, value: edit.Value))).As().Map(static _ => unit);

    // `DimensionStyle` is the EFFECTIVE style — already carrying any prior override — so seeding the child from it
    // compounds each successive `Restyle`; the construction base is `ParentDimensionStyle`, which re-derives clean.
    internal Fin<DimensionStyle> Overlay(AnnotationBase annotation, Op key) =>
        from parent in Optional(annotation.ParentDimensionStyle).ToFin(Fail: key.MissingContext())
        from child in key.Catch(() => Fin.Succ(value: parent.Duplicate(
            newName: string.Empty, newId: Guid.Empty, newParentId: annotation.DimensionStyleId)))
        from _ in Apply(style: child, key: key)
            .BindFail(primary => DraftCustody.Failed<Unit, DimensionStyle>(primary: primary, values: Seq(child), op: key))
        from attached in key.Confirm(success: annotation.SetOverrideDimStyle(overrideStyle: child))
            .BindFail(primary => DraftCustody.Failed<Unit, DimensionStyle>(primary: primary, values: Seq(child), op: key))
        select child;
}
```

## [04]-[STYLE_RAIL]

- Owner: `StyleOp` `[Union]` — the mutation verbs over the `DimStyleTable`: authoring, patch amendment, whole-setting copy, override clearing, reverse absorption, reparenting, current selection, deletion, length scaling, the paper/model scale faces, and the user-string bag; `DraftPlan<StyleOp>` — the admitted commit plan; `Styles` — the `Commit`/`Ask` entry pair.
- Law: an amendment never mutates the resolved live component — every write duplicates it, applies its change to the copy, and lands through `DimStyleTable.Modify` by index inside the shared undo bracket.
- Law: `Author` refuses an existing name, shapes the detached style, and performs one terminal `Add`; a parent payload makes the authored style a child whose patch-marked fields alone override the parent through `ParentId`.
- Law: `DraftPlan<TOp>.Of` admits its mode and every operation before the shared commit spine can enter a document grant.
- Law: every plural ADMISSION fold in the namespace accumulates — `Traverse` onto `Validation`, then back to `Fin` — so a rejected batch reports its whole refusal set; the fail-fast `TraverseM` shape is reserved for plural HOST WRITES, where a later write must never run after an earlier one refused.
- Law: `Absorb` is the one reverse projection — `DimStyleTable.Modify(style, annotation)` folds a live annotation's per-instance overrides back onto the style, its `ModifyType` outcome inspected before the write counts: `Modify` and `Override` land as receipt facts, `NotSaved` is a typed refusal.
- Law: `Copy` projects every source setting through `DimensionStyle.CopyFrom` while preserving the target name, id, and index; `StyleTagEdit` closes set, delete, and clear under one mutation case without a sentinel key.
- Law: reclamation is not a case — unused-style reclaim is the document rail's `TableOp.Reclaim(TableKind.DimStyles)` row, and re-spelling it here splits one host member across two owners.
- Law: the write posture is the spine's `HostInteraction`, carried by every drafting op case in the namespace — style, linetype, hatch, and section alike. The axis is exactly quiet-versus-interactive and the spine already owns it, so a folder-local two-row vocabulary over the same host `quiet` boolean was one concept with two names that could disagree.
- Growth: a new style verb is one case with its arm; the spine, the receipt, and every consumer read it with zero new surface.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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
    public sealed record Author(ResourceName Name, StylePatch Patch, HostInteraction Interaction, Option<ResourceId> Parent = default) : StyleOp;
    public sealed record Amend(ResourceRef Target, StylePatch Patch, HostInteraction Interaction) : StyleOp;
    public sealed record Copy(ResourceRef Target, ResourceRef Source, HostInteraction Interaction) : StyleOp;
    public sealed record ClearOverrides(ResourceRef Target, Seq<StyleField> Fields, HostInteraction Interaction) : StyleOp;
    public sealed record Absorb(ResourceRef Target, TableTarget Annotation) : StyleOp;
    public sealed record Reparent(ResourceRef Target, Option<ResourceId> Parent, HostInteraction Interaction) : StyleOp;
    public sealed record SetCurrent(ResourceRef Target, HostInteraction Interaction) : StyleOp;
    public sealed record Delete(ResourceRef Target, HostInteraction Interaction) : StyleOp;
    public sealed record ScaleLengths(ResourceRef Target, double Factor, HostInteraction Interaction) : StyleOp;
    public sealed record PageScale(ResourceRef Target, double LeftMillimeters, double RightMillimeters, HostInteraction Interaction) : StyleOp;
    public sealed record Tag(ResourceRef Target, StyleTagEdit Edit, HostInteraction Interaction) : StyleOp;

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
                from receipt in new Lease<DimensionStyle>.Owned(Value: shaped).Use(owned =>
                    from __ in edit.Parent.Traverse(parent => context.Op.Catch(() => owned.ParentId = parent.Value)).As()
                    from ___ in edit.Patch.Apply(style: owned, key: context.Op)
                    from index in context.Op.Catch(() => ResourceIndex.Admit(
                        context.Document.DimStyles.Add(dimstyle: owned, reference: false), context.Op))
                    from authored in DraftReceipt.Component(
                        slot: DraftSlot.Authored, componentKind: DraftComponentKind.Style, index: index, key: context.Op)
                    select authored)
                select receipt,
            amend: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, interaction: edit.Interaction, op: context.Op,
                    revise: (style, key) => edit.Patch.Apply(style: style, key: key)),
            copy: static (context, edit) =>
                from source in edit.Source.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from receipt in Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, interaction: edit.Interaction,
                    op: context.Op, revise: (style, key) => key.Catch(() => style.CopyFrom(source)))
                select receipt,
            clearOverrides: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, interaction: edit.Interaction, op: context.Op,
                    revise: (style, key) => edit.Fields.IsEmpty
                        ? key.Catch(style.ClearAllFieldOverrides)
                        : edit.Fields.TraverseM(field => key.Catch(() => style.ClearFieldOverride(field: field.Host))).As().Map(static _ => unit)),
            absorb: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from row in edit.Annotation.Only<AnnotationObjectBase>(document: context.Document, key: context.Op)
                from annotation in context.Op.Need(row.Native.AnnotationGeometry)
                from outcome in context.Op.Catch(() => context.Document.DimStyles.Modify(dimstyle: style, annotation: annotation) switch {
                    ModifyType.Modify or ModifyType.Override => Fin.Succ(value: style.Index),
                    var refused => Fin.Fail<int>(error: context.Op.InvalidResult(detail: refused.ToString())),
                })
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Absorbed, componentKind: DraftComponentKind.Style,
                    index: ResourceIndex.Create(outcome), key: context.Op)
                select receipt,
            reparent: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Reparented, interaction: edit.Interaction, op: context.Op,
                    revise: (style, key) => key.Catch(() =>
                        style.ParentId = edit.Parent.Map(static parent => parent.Value).IfNone(noneValue: Guid.Empty))),
            setCurrent: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.SetCurrent(index: style.Index, quiet: edit.Interaction.IsQuiet))
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Current, componentKind: DraftComponentKind.Style,
                    index: ResourceIndex.Create(style.Index), key: context.Op)
                select receipt,
            delete: static (context, edit) =>
                from style in edit.Target.Resolve(document: context.Document, lens: Lens, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.DimStyles.Delete(index: style.Index, quiet: edit.Interaction.IsQuiet))
                from receipt in DraftReceipt.Component(
                    slot: DraftSlot.Deleted, componentKind: DraftComponentKind.Style,
                    index: ResourceIndex.Create(style.Index), key: context.Op)
                select receipt,
            scaleLengths: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Scaled, interaction: edit.Interaction, op: context.Op,
                    revise: (style, key) => key.Positive(value: edit.Factor)
                        .Bind(_ => key.Catch(() => style.ScaleLengthValues(scale: edit.Factor)))),
            pageScale: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Scaled, interaction: edit.Interaction, op: context.Op,
                    revise: (style, key) =>
                        from _ in key.Positive(value: edit.LeftMillimeters)
                        from __ in key.Positive(value: edit.RightMillimeters)
                        from ___ in key.Catch(() => {
                            style.ScaleLeftLengthMillimeters = edit.LeftMillimeters;
                            style.ScaleRightLengthMillimeters = edit.RightMillimeters;
                        })
                        select unit),
            tag: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context.Document, slot: DraftSlot.Amended, interaction: edit.Interaction, op: context.Op,
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
               from admittedRun in run
                   .Traverse(operation => op.AcceptInput(value: operation).ToValidation())
                   .As()
                   .ToFin()
               from _ in guard(!admittedRun.IsEmpty, op.InvalidInput()).ToFin()
               select new DraftPlan<TOp>(name: label, mode: admittedMode, operations: admittedRun);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Styles {
    public static Fin<DraftReceipt> Commit(DocumentSession session, DraftPlan<StyleOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document, op: key),
            op: Op.Of(name: nameof(Styles)));

    public static Fin<StyleAnswer> Ask(DocumentSession session, StyleAsk request) {
        Op op = Op.Of(name: nameof(Styles));
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
- Boundary: `CreatePreviewBitmap` renders through the host and reaches this page only inside `Styles.Ask`'s `DocumentSession.Demand`, which resolves every body on the command thread — so the preview needs no second crossing and none is spelled, exactly as the block-preview rail is bound. A preview reached outside a demand has no affinity at all, and this page publishes no such route.
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
    ModelUnit LengthUnit,
    ModelUnit AlternateLengthUnit) : IDetachedDocumentResult {
    public static Fin<StyleSnapshot> Of(DimensionStyle style, RhinoDoc document, Op key) =>
        from active in key.Need(style)
        from settings in toSeq(StyleField.Items)
            .TraverseM(row => key.Catch(() => row.Read(style: active, key: key))
                .Map(value => new StyleSetting(Field: row, Value: value)))
            .As()
        from root in key.Catch(() => Optional(document.DimStyles.FindRoot(styleId: active.Id, ignoreDeleted: true))
            .ToFin(Fail: key.InvalidResult()))
        from lengthUnit in ModelUnit.Of(
            value: active.DimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber), key: key)
        from alternateLengthUnit in ModelUnit.Of(
            value: active.AlternateDimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber), key: key)
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
            Tags: toSeq(TagOp.Snapshot(active.GetUserStrings())).Map(static pair =>
                StyleTag.Create(key: StyleTagKey.Create(pair.Key), value: pair.Value)),
            TagCount: active.UserStringCount,
            Current: document.DimStyles.CurrentId == active.Id,
            ScaleValue: active.DimensionScaleValue,
            LengthUnit: lengthUnit,
            AlternateLengthUnit: alternateLengthUnit)))
        select snapshot;
}
```

## [06]-[SPINE_AND_RECEIPTS]

- Owner: `DraftSpine` — the one Annotation commit entry: it derives its needs through `SessionNeed.Mutation`, demands once, and commits through the Document spine's `DocumentCommit.Sealed` envelope with the `DraftReceipt` fold and undo-serial stamp as its carrier; `DraftSlot` `[SmartEnum<int>]` — the consequence vocabulary, each row carrying the body predicate it admits; `DraftBody` `[Union]` — the typed fact payloads; `DraftFacts` — the folder's mint surface as an extension block. `DraftFact` and `DraftReceipt` are ALIASES of the Document spine's `Fact<TSlot, TBody>`/`FactStream<TSlot, TBody>` closed over this folder's two vocabularies.
- Law: the spine is the one commit entry for the namespace — style, text, dimension, hatch, linetype, and section commits share it verbatim, so undo, redraw, and grant semantics cannot drift between drafting rails; a rail re-spelling the demand/envelope sequence, or opening `UndoBracket.Begin` beside `Sealed`, is the deleted form.
- Law: `DocumentCommit.Compensated` is the one compensating-transaction fold — land each element, roll back every landed key on the first refusal, settle source custody through its release policy on every outcome, preserve the initiating fault, and append rollback and release faults in order; a rail re-typing this fold or spelling a caller-local release cascade beside it is the deleted form.
- Law: the stream MACHINERY is not this folder's — accumulation, the `Admits` cross-product gate, the undo-stamp projection, and `Project<T>` live once on the Document spine's `FactStream<TSlot, TBody>`, and this folder contributes exactly its slot vocabulary and its body union; a folder-local receipt, fact, gate, or projection beside the owner is the deleted form, and the same two declarations are all a third mutation folder needs to join.
- Law: one fact stream — each `DraftSlot` row carries its own generated `Admits` predicate over `DraftBody`, so a slot cannot exist without declaring the bodies it emits, the stream factory refuses an illegal pairing with the slot named in the fault detail, and a body kind never doubles as a second vocabulary beside the union it discriminates.
- Law: `Project<T>(slot, select)` is the receipt's one reader — a caller selects the demanded body case instead of growing a typed accessor per body.
- Law: every receipt mint takes the operating key, so a fact carries the provenance of the arm that produced it rather than an anonymous root minted at the factory.
- Law: the undo scalar is the spine's `UndoSerial` — the commit envelope mints it, every folder receipt carries it, and `Maybe` is the one zero projector, so an unrecorded program contributes no undo fact instead of one asserting record zero. A folder-local twin over the same `uint` is the forked form: two owners of one invariant drift the moment either changes.
- Growth: a new consequence class is one slot row with its predicate or one body case; every rail and every projection gains it for free.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DraftComponentKind {
    public static readonly DraftComponentKind Style = new();
    public static readonly DraftComponentKind Section = new();
    public static readonly DraftComponentKind Hatch = new();
    public static readonly DraftComponentKind Linetype = new();
}

// This folder's whole contribution to the shared stream: a keyed slot vocabulary and a body union. The
// accumulation, the gate, the undo projection, and the slot-keyed reader live once on the Document spine's
// `FactStream<TSlot, TBody>`, which this page closes as `DraftReceipt`.
[SmartEnum<int>]
public sealed partial class DraftSlot : IFactSlot<DraftBody> {
    public static readonly DraftSlot Authored = new(key: 0, admits: Rowed);
    public static readonly DraftSlot Amended = new(key: 1, admits: Touched);
    public static readonly DraftSlot Absorbed = new(key: 2, admits: Rowed);
    public static readonly DraftSlot Reparented = new(key: 3, admits: Rowed);
    public static readonly DraftSlot Current = new(key: 4, admits: Rowed);
    public static readonly DraftSlot Deleted = new(key: 5, admits: Touched);
    public static readonly DraftSlot Revived = new(key: 6, admits: Touched);
    public static readonly DraftSlot Scaled = new(key: 7, admits: Touched);
    public static readonly DraftSlot Renamed = new(key: 8, admits: Rowed);
    public static readonly DraftSlot Imported = new(key: 9, admits: Filed);
    public static readonly DraftSlot Exported = new(key: 10, admits: Shipped);
    public static readonly DraftSlot Loaded = new(key: 11, admits: Stocked);
    public static readonly DraftSlot Placed = new(key: 12, admits: Instanced);
    public static readonly DraftSlot Adjusted = new(key: 13, admits: Instanced);
    public static readonly DraftSlot Restyled = new(key: 14, admits: Instanced);
    public static readonly DraftSlot Reflowed = new(key: 15, admits: Instanced);
    public static readonly DraftSlot Reformulated = new(key: 16, admits: Instanced);
    public static readonly DraftSlot Bound = new(key: 17, admits: Touched);
    public static readonly DraftSlot Undo = new(key: 18, admits: Stamped);

    // The cross product a receipt may express: one predicate row per slot, so a new slot cannot compile without
    // declaring which bodies it emits and a mismatched pairing refuses at the stream factory naming the slot.
    [UseDelegateFromConstructor]
    public partial bool Admits(DraftBody body);

    private static bool Rowed(DraftBody body) => body is DraftBody.Component;
    private static bool Instanced(DraftBody body) => body is DraftBody.Object;
    private static bool Touched(DraftBody body) => body is DraftBody.Component or DraftBody.Object;
    private static bool Filed(DraftBody body) => body is DraftBody.Component or DraftBody.Tally or DraftBody.Path;
    private static bool Shipped(DraftBody body) => body is DraftBody.Tally or DraftBody.Path;
    private static bool Stocked(DraftBody body) => body is DraftBody.Component or DraftBody.Tally;
    private static bool Stamped(DraftBody body) => body is DraftBody.Record;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DraftBody {
    private DraftBody() { }
    public sealed record Component(DraftComponentKind ComponentKind, ResourceIndex Index) : DraftBody;
    public sealed record Object(ResourceId Id) : DraftBody;
    public sealed record Tally(DraftCount Count) : DraftBody;
    public sealed record Path(DraftPath Value) : DraftBody;
    public sealed record Record(UndoSerial Serial) : DraftBody;
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

// --- [EXPORTS] ------------------------------------------------------------------------------
// The folder's receipt IS the spine's stream closed over this folder's two vocabularies; the aliases carry the
// domain names call sites already read, and the project-level alias rows publish them namespace-wide, so no
// consumer spells the instantiation and no folder-local receipt type exists to drift from the owner.
global using DraftFact = Rasm.Rhino.Document.Fact<Rasm.Rhino.Annotation.DraftSlot, Rasm.Rhino.Annotation.DraftBody>;
global using DraftReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Annotation.DraftSlot, Rasm.Rhino.Annotation.DraftBody>;

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The folder's mint surface rides an extension block over the closed instantiation, so `DraftReceipt.Component`
// reads exactly as it did while the accumulation and the gate stay on the one owner.
public static class DraftFacts {
    extension(DraftReceipt) {
        public static Fin<DraftReceipt> Component(DraftSlot slot, DraftComponentKind componentKind, ResourceIndex index, Op key) =>
            DraftReceipt.Of(slot: slot, body: new DraftBody.Component(ComponentKind: componentKind, Index: index), key: key);

        public static Fin<DraftReceipt> Objects(DraftSlot slot, Seq<ResourceId> ids, Op key) =>
            DraftReceipt.All(
                slot: slot,
                bodies: ids.Distinct().Map(static id => (DraftBody)new DraftBody.Object(Id: id)),
                key: key);

        public static Fin<DraftReceipt> Tally(DraftSlot slot, DraftCount count, Op key) =>
            DraftReceipt.Of(slot: slot, body: new DraftBody.Tally(Count: count), key: key);

        public static Fin<DraftReceipt> Path(DraftSlot slot, DraftPath path, Op key) =>
            DraftReceipt.Of(slot: slot, body: new DraftBody.Path(Value: path), key: key);
    }
}

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
                    .Map(static receipts => receipts.Fold(DraftReceipt.Empty, static (state, next) => state + next)),
                stamp: static (receipt, serial) => receipt.Stamped(
                    slot: DraftSlot.Undo,
                    record: static stamped => new DraftBody.Record(Serial: stamped),
                    serial: serial),
                op: op),
            key: op,
            needs: SessionNeed.Mutation(undo: plan.Mode.RecordsUndo, redraw: plan.Mode.Redraw).ToArray());
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]                                 | [FORM]                                        | [ENTRY]                     |
| :-----: | :--------------- | :-------------------------------------- | :-------------------------------------------- | :-------------------------- |
|  [01]   | native custody   | `DraftCustody`                          | release fold, failure fold, detach crossing   | `Release` / `Crossed`       |
|  [02]   | table revision   | `TableGrip<T>`                          | Document lens + index/duplicate/modify rows   | `Revised(target, ...)`      |
|  [03]   | color boundary   | `ColorBoundary`                         | `PerceptualColor`/`Color` round trip          | `Admitted` / `Sys`          |
|  [04]   | user-string bag  | `TagBag`                                | compensated whole-bag replacement             | `Apply`                     |
|  [05]   | object singleton | `TargetResolution`                      | exactly-one id + typed cast on `TableTarget`  | `Only<TNative>`             |
|  [06]   | input custody    | `DraftBorrow`                           | nested lease scopes over a handle or a run    | `Typed<TNative, TOut>`      |
|  [07]   | drafting scalars | `DraftScale`/`DraftAngle`/`DraftWeight` | positive, radian, and millimetre owners       | `Create` / `Value`          |
|  [08]   | unit vocabulary  | `LengthDisplayRow`                      | rows keyed on host values, metric column      | `Host` projection           |
|  [09]   | config schema    | `StyleField`                            | one row per proven property/`Field` pairing   | `Read` / `Write`            |
|  [10]   | edit currency    | `StylePatch`                            | exact-family run, table and override folds    | `Apply` / `Overlay`         |
|  [11]   | style mutations  | `StyleOp`                               | flat `[Union]`, duplicate-then-`Modify` law   | `Styles.Commit`             |
|  [12]   | style reads      | `StyleAsk`                              | closed request/answer family                  | `Styles.Ask`                |
|  [13]   | commit entry     | `DraftSpine`                            | `Sealed` over the `DraftReceipt` fold         | `Commit`                    |
|  [14]   | receipts         | `DraftSlot`/`DraftBody`                 | spine `FactStream` closed on two vocabularies | `DraftFacts` / `Project<T>` |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
