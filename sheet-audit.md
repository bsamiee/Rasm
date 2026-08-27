# 1. Use generated value-object projections

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:386`
```csharp
    public Fin<RevisionIndex> Next(Op? key = null) => Of(value: Advance(held: Value), key: key);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:476`
```csharp
    public static readonly TitleField Revision = new(key: "revision", read: static (block, _) => block.Revision.Map(static r => r.Index.Value).IfNone(string.Empty));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1241`
```csharp
    public string Text => Switch(indexed: static row => row.Index.Value.ToString(CultureInfo.InvariantCulture), named: static row => row.Name.Value);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:386`
```csharp
    public Fin<RevisionIndex> Next(Op? key = null) => Of(value: Advance(held: ToValue()), key: key);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:476`
```csharp
    public static readonly TitleField Revision = new(key: "revision", read: static (block, _) => block.Revision.Map(static r => r.Index.ToValue()).IfNone(string.Empty));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1241`
```csharp
    public string Text => Switch(indexed: static row => row.Index.ToValue().ToString(CultureInfo.InvariantCulture), named: static row => row.Name.ToValue());
```

## Why
`[ValueObject<T>]` keeps its generated key private by default and publishes `ToValue()` as the supported projection. These `.Value` reads target members the declarations do not generate.

## Change
Project `RevisionIndex`, `AciIndex`, and `StyleName` through their generated `ToValue()` members.

## Delta
`LOC: +0; symbols: +0`

# 2. Collapse the rung roster protocol

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:71`
```csharp
internal interface IRungRoster<TSelf> where TSelf : class, IRungRoster<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    static abstract double[] Logs { get; }
}
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:83`
```csharp
    internal static TSelf Nearest<TSelf>(double magnitude) where TSelf : class, IRungRoster<TSelf> =>
        TSelf.Items[NearestIndex(logs: TSelf.Logs, magnitude: magnitude)];
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1124`
```csharp
public sealed partial class LineWidth : IRungRoster<LineWidth> {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1137`
```csharp
    public static double[] Logs => LogLadder.Value;
    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Width.Millimeters)).ToArray());
    public static Fin<LineWidth> For(Length width, Op? key = null) =>
        width.Millimeters > 0.0 && double.IsFinite(width.Millimeters)
            ? Fin.Succ(RungLadder.Nearest<LineWidth>(magnitude: width.Millimeters))
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1299`
```csharp
public sealed partial class TextHeight : IRungRoster<TextHeight> {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1321`
```csharp
    public static double[] Logs => LogLadder.Value;
    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Height.Millimeters)).ToArray());
    public static Fin<TextHeight> For(Length height, Op? key = null) =>
        height.Millimeters > 0.0 && double.IsFinite(height.Millimeters)
            ? Fin.Succ(RungLadder.Nearest<TextHeight>(magnitude: height.Millimeters))
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:71`
```csharp
// IRungRoster DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:83`
```csharp
// RungLadder.Nearest DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1124`
```csharp
public sealed partial class LineWidth {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1137`
```csharp
    // LineWidth.Logs DELETED
    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Width.Millimeters)).ToArray());
    public static Fin<LineWidth> For(Length width, Op? key = null) =>
        width.Millimeters > 0.0 && double.IsFinite(width.Millimeters)
            ? Fin.Succ(Items[RungLadder.NearestIndex(logs: LogLadder.Value, magnitude: width.Millimeters)])
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1299`
```csharp
public sealed partial class TextHeight {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1321`
```csharp
    // TextHeight.Logs DELETED
    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Height.Millimeters)).ToArray());
    public static Fin<TextHeight> For(Length height, Op? key = null) =>
        height.Millimeters > 0.0 && double.IsFinite(height.Millimeters)
            ? Fin.Succ(Items[RungLadder.NearestIndex(logs: LogLadder.Value, magnitude: height.Millimeters)])
```

## Why
Only two owners implement the protocol, both already receive generated `Items` and own a private log cache, while `ScaleLadder` calls `NearestIndex` directly. The interface, generic forwarder, and public log projections add four symbols without abstracting varying behavior.

## Change
Keep `RungLadder.NearestIndex` as the one snap algorithm and let each roster index its generated items with its private cached logs.

## Delta
`LOC: -8; symbols: -4`

# 3. Fail the extent freeze at the declared seat

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:236`
```csharp
    private static readonly Lazy<FrozenDictionary<(SheetSeries Series, int Index), (Length Width, Length Height)>> Ladder =
        new(static () => toSeq(SheetSeries.Items)
            .Bind(static series => Range(series.Bounds.Floor, series.Bounds.Ceiling - series.Bounds.Floor + 1).ToSeq().Map(index => (Series: series, Index: index)))
            .Choose(static seat => seat.Series.Extent(seat.Index, Op.Of()).ToOption().Map(extent => (Seat: seat, Extent: extent)))
            .ToFrozenDictionary(static row => row.Seat, static row => row.Extent));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:236`
```csharp
    private static readonly Lazy<FrozenDictionary<(SheetSeries Series, int Index), (Length Width, Length Height)>> Ladder =
        new(static () => toSeq(SheetSeries.Items)
            .Bind(static series => Range(series.Bounds.Floor, series.Bounds.Ceiling - series.Bounds.Floor + 1).ToSeq().Map(index => (Series: series, Index: index)))
            .ToFrozenDictionary(static seat => seat, static seat => seat.Series.Extent(seat.Index, Op.Of()).ThrowIfFail()));
```

## Why
`Choose(...ToOption())` converts a failed declared extent into an absent dictionary row, so the later lookup loses the originating fault. Every enumerated seat is roster truth and must either freeze or fail where it is derived.

## Change
Freeze each declared seat directly and unwrap only at the static-initialization boundary.

## Delta
`LOC: -1; symbols: +0`

# 4. Call the notation renderer directly

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:469`
```csharp
    public static readonly TitleField Scale = new(key: "scale", read: static (block, standard) => block.Scale.Render(ScaleNotation.For(standard)));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:556`
```csharp
    public string Render(ScaleNotation notation) => notation.Render(this);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:587`
```csharp
    [UseDelegateFromConstructor] internal partial string Render(DrawingScale scale);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:469`
```csharp
    public static readonly TitleField Scale = new(key: "scale", read: static (block, standard) => ScaleNotation.For(standard).Render(block.Scale));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:556`
```csharp
    // DrawingScale.Render DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:587`
```csharp
    [UseDelegateFromConstructor] public partial string Render(DrawingScale scale);
```

## Why
The notation row owns the render delegate. `DrawingScale.Render` only reverses that call and makes the semantic owner resolve through an extra hop.

## Change
Publish the notation renderer, call it directly, and remove the forwarding scale member.

## Ripples
At `libs/dotnet/Rasm.Rhino/.planning/Exchange/publish.md:174`, replace `scale.Render(notation: ScaleNotation.For(Standard))` with `ScaleNotation.For(Standard).Render(scale)`; at `libs/dotnet/Rasm.AppUi/.planning/Render/drafting.md:988`, replace `ratio.Render(ScaleNotation.For(sheet.Size.Standard))` with `ScaleNotation.For(sheet.Size.Standard).Render(ratio)`. Update the owned-surface spelling at `libs/dotnet/Rasm.Rhino/.planning/Exchange/publish.md:148` from `DrawingScale.Render(ScaleNotation.For(standard))` to `ScaleNotation.For(standard).Render(scale)`.

## Delta
`LOC: -1; symbols: -1`

# 5. Remove the impossible empty-ladder fallback

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:655`
```csharp
    public DrawingScale Nearest(DrawingScale scale) =>
        Frozen.Value[this] is var ladder && ladder.Members.IsEmpty
            ? scale
            : ladder.Members[RungLadder.NearestIndex(logs: ladder.Logs, magnitude: scale.Ratio)];
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:655`
```csharp
    public DrawingScale Nearest(DrawingScale scale) =>
        Members[RungLadder.NearestIndex(logs: Frozen.Value[this].Logs, magnitude: scale.Ratio)];
```

## Why
Every closed `ScaleLadder` row constructs at least one rung before `Frozen` becomes visible. Returning an arbitrary unpreferred input for an impossible empty row weakens the ladder contract with success-shaped fallback data.

## Change
Index the frozen roster directly so a broken future row fails at its owner.

## Delta
`LOC: -2; symbols: +0`

# 6. Inline the single-use scale power fold

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:629`
```csharp
    public static readonly ScaleLadder Iso5455 = new(key: "iso-5455", standard: SheetStandard.Iso, decades: 5, ceiling: 10000, rungs: static () =>
        toSeq(new[] { 1, 2, 5 }).Bind(static mantissa => Powers(seed: mantissa, count: Iso5455!.Decades))
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:650`
```csharp
    private static Seq<int> Powers(int seed, int count) =>
        Range(0, count).Fold(Seq<int>(), (held, _) => held.Add(held.Last.Map(static last => last * 10).IfNone(seed)));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:629`
```csharp
    public static readonly ScaleLadder Iso5455 = new(key: "iso-5455", standard: SheetStandard.Iso, decades: 5, ceiling: 10000, rungs: static () =>
        toSeq(new[] { 1, 2, 5 }).Bind(static mantissa => Range(0, Iso5455!.Decades).Fold(Seq<int>(), (held, _) => held.Add(held.Last.Map(static last => last * 10).IfNone(mantissa))))
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:650`
```csharp
// ScaleLadder.Powers DELETED
```

## Why
`Powers` is a private one-call helper whose policy inputs and sole use already belong to the `Iso5455` formula row.

## Change
Keep the state fold beside the single formula that uses it and delete the helper hop.

## Delta
`LOC: -2; symbols: -1`

# 7. Fold admitted naming fields without re-looking them up

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:856`
```csharp
    internal string Render(Seq<(NamingField Field, string Value)> fields) =>
        Sequence.Map(static (field, index) => (field, index)).Fold(string.Empty, (held, pair) =>
            string.Concat(held, pair.Item2 == 0 || Fused.Contains(pair.Item2 - 1) ? string.Empty : Delimiter, Value(fields, pair.Item1)));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:866`
```csharp
    private static string Value(Seq<(NamingField Field, string Value)> fields, NamingField field) => fields.Find(pair => pair.Field.Equals(field)).Map(static pair => pair.Value).IfNone(string.Empty);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:856`
```csharp
    internal string Render(Seq<(NamingField Field, string Value)> fields) =>
        fields.Map(static (pair, index) => (pair.Value, Index: index)).Fold(string.Empty, (held, pair) =>
            string.Concat(held, pair.Index == 0 || Fused.Contains(pair.Index - 1) ? string.Empty : Delimiter, pair.Value));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:866`
```csharp
// NamingStandard.Value DELETED
```

## Why
`SheetNumber` stores the successful `Admit` result, whose fields are already in the standard's sequence. Re-walking `Sequence` and searching the admitted run for every position repeats an invariant already discharged at intake.

## Change
Fold the ordered admitted values directly and delete the private lookup helper.

## Delta
`LOC: -1; symbols: -1`

# 8. Generate the precision cases as an ad-hoc union

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1483`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DrawingPrecisionForm {
    private DrawingPrecisionForm() { }
    public sealed record Places : DrawingPrecisionForm { internal Places(int count) => Count = count; public int Count { get; } }
    public sealed record Fraction : DrawingPrecisionForm { internal Fraction(int denominator) => Denominator = denominator; public int Denominator { get; } }
}
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1502`
```csharp
    private static DrawingPrecisionForm Decimal(double resolved) => new DrawingPrecisionForm.Places(count: Math.Max(0, (int)Math.Ceiling(-Math.Log10(resolved))));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1504`
```csharp
    private static DrawingPrecisionForm Fractional(double resolvedFeet) =>
        new DrawingPrecisionForm.Fraction(denominator: Denominators.Find(rung => 1.0 / rung <= resolvedFeet * 12.0).IfNone(Denominators[^1]));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1483`
```csharp
// DrawingPrecisionForm.Places DELETED
// DrawingPrecisionForm.Fraction DELETED
[Union<int, int>(T1Name = "Places", T2Name = "Fraction")]
public readonly partial struct DrawingPrecisionForm;
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1502`
```csharp
    private static DrawingPrecisionForm Decimal(double resolved) => DrawingPrecisionForm.Places(Math.Max(0, (int)Math.Ceiling(-Math.Log10(resolved))));
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1504`
```csharp
    private static DrawingPrecisionForm Fractional(double resolvedFeet) =>
        DrawingPrecisionForm.Fraction(Denominators.Find(rung => 1.0 / rung <= resolvedFeet * 12.0).IfNone(Denominators[^1]));
```

## Why
The two cases carry one `int` each and no case-specific behavior. Thinktecture's named ad-hoc union generates their construction and exhaustive dispatch without two nested records, two constructors, and two payload properties.

## Change
Declare the two payload slots on `[Union<int, int>]` and use the generated named case factories.

## Ripples
At `libs/dotnet/Rasm.Rhino/.planning/Annotation/style.md:706-708`, change the generated `Switch` arms to `places: static count => count` and `fraction: static denominator => (int)Math.Log2(denominator)` because ad-hoc-union arms receive the payload directly.

## Delta
`LOC: -4; symbols: -2`

# 9. Make drawing precision a total projection

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1513`
```csharp
public readonly record struct DrawingPrecision(DrawingScale Scale, DrawingUnits Units) {
    private static Length PaperQuantum => LineWidth.W025.Width;
    public Length Quantum => PaperQuantum * ((double)Scale.Model / Scale.Paper);
    public Fin<DrawingPrecisionForm> Form(Op? key = null) =>
        Quantum.As(Units.Unit) is var resolved && resolved > 0.0 && double.IsFinite(resolved)
            ? Fin.Succ(Units.Form(resolved: resolved))
            : Fin.Fail<DrawingPrecisionForm>(new KernelFault.OutOfRange(Label: nameof(Quantum), Scalar: resolved, Requirement: "a positive finite model quantum", Key: Some(key.OrDefault())));
    public static DrawingPrecision Of(DrawingScale scale, DrawingUnits units) => new(Scale: scale, Units: units);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1513`
```csharp
public readonly record struct DrawingPrecision(DrawingScale Scale, DrawingUnits Units) {
    private static Length PaperQuantum => LineWidth.W025.Width;
    public Length Quantum => PaperQuantum * ((double)Scale.Model / Scale.Paper);
    public DrawingPrecisionForm Form() => Units.Form(resolved: Quantum.As(Units.Unit));
    // DrawingPrecision.Of DELETED
}
```

## Why
`DrawingScale` admits positive integer terms, every `DrawingUnits` row carries a real `LengthUnit`, and `PaperQuantum` is a fixed positive finite rung. Their derived quantum cannot reach the failure arm, so rechecking it and accepting an unused `Op` misrepresent a total calculation as fallible. The positional record constructor already mints the value.

## Change
Return the precision form directly, remove the redundant factory, and construct the record through its existing positional constructor.

## Ripples
At `libs/dotnet/Rasm.Rhino/.planning/Annotation/style.md:703`, replace the `from resolution in DrawingPrecision.Of(...).Form(key: op)` clause with `let resolution = new DrawingPrecision(Scale: scale, Units: DrawingUnits.For(standard: size.Standard)).Form()`. At `libs/dotnet/Rasm.Rhino/.planning/Document/session.md:1314`, construct `new DrawingPrecision(Scale: admission.First, Units: admission.Second)`. Remove `DrawingPrecision.Of` from the package surface at `libs/dotnet/Rasm.Rhino/.planning/Annotation/style.md:340`, and rewrite the derivation spelling at `libs/dotnet/Rasm.Rhino/.planning/Document/session.md:1153` to `new DrawingPrecision(scale, units).Form()`.

## Delta
`LOC: -4; symbols: -1`

# 10. Seal sheet-size admission at its factories

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:169`
```csharp
public abstract partial record SheetSize : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:171`
```csharp
    public sealed record Rostered : SheetSize {
        internal Rostered(SheetSeries series, int index) => (Series, Index) = (series, index);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:176`
```csharp
    public sealed record Custom : SheetSize {
        internal Custom(Length width, Length height, SheetStandard standard) => (Width, Height, Standard) = (width, height, standard);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:245`
```csharp
    public bool IsValid => ValidityClaim.All(Width > Length.Zero, Height > Length.Zero);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:169`
```csharp
public abstract partial record SheetSize {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:171`
```csharp
    public sealed record Rostered : SheetSize {
        private Rostered(SheetSeries series, int index) => (Series, Index) = (series, index);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:176`
```csharp
    public sealed record Custom : SheetSize {
        private Custom(Length width, Length height, SheetStandard standard) => (Width, Height, Standard) = (width, height, standard);
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:245`
```csharp
    // SheetSize.IsValid DELETED
```

## Why
Both cases are constructed only by `SheetSize.Of`, which already admits the series bounds or positive finite extents. Internal case constructors leave a bypass that forces downstream validity checks; private constructors make the admitted invariant structural and retire the weaker repeated predicate.

## Change
Make both case constructors private, remove `IValidityEvidence`, and delete the post-construction validity projection.

## Ripples
At `libs/dotnet/Rasm.Rhino/.planning/Display/modes.md:988-991`, reduce the `capture` arm to the session, target, and mode checks; delete `row.Extent.Match(Some: static size => size.IsValid, None: static () => true)` because every present `SheetSize` is admitted.

## Delta
`LOC: -1; symbols: -1`

# 11. Remove the weaker title-block recheck

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:407`
```csharp
public sealed record TitleBlock : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:431`
```csharp
    public bool IsValid => ValidityClaim.All(Sheet >= 1, SheetCount >= Sheet, Owner.Length > 0, Title.Length > 0);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:407`
```csharp
public sealed record TitleBlock {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:431`
```csharp
    // TitleBlock.IsValid DELETED
```

## Why
The constructor is private and `TitleBlock.Of` already accumulates the stronger admission: owner, project, client, title, drawn-by, sheet ordinal, and sheet count. `IsValid` repeats only a weaker subset and can never reject an admitted instance.

## Change
Remove the evidence interface and its redundant post-construction predicate.

## Delta
`LOC: -1; symbols: -1`

# 12. Stop revalidating an admitted plot policy

## From
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1618`
```csharp
public sealed record PlotPolicy : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1634`
```csharp
    public bool IsValid => Size.IsValid;
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1640`
```csharp
        return (
                size.IsValid ? Validation<Error, SheetSize>.Success(size) : Validation<Error, SheetSize>.Fail(op.InvalidInput()),
                ScaleLadder.For(size.Standard).Admits(scale)
                    ? Validation<Error, DrawingScale>.Success(scale)
                    : Validation<Error, DrawingScale>.Fail(new KernelFault.InvalidValue(Label: nameof(scale), Requirement: "a rung of the standard's scale ladder", Key: Some(op))),
                LineGroup.For(size: size, key: op).ToValidation(),
                PdfTrait.Law.Admit(conformance).ToValidation())
            .Apply((admittedSize, admittedScale, group, traits) => new PlotPolicy(
                size: admittedSize, orientation: orientation, frame: SheetFrame.For(admittedSize.Standard), scale: admittedScale, group: group,
```

## To
`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1618`
```csharp
public sealed record PlotPolicy {
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1634`
```csharp
    // PlotPolicy.IsValid DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/sheet.md:1640`
```csharp
        return (
                ScaleLadder.For(size.Standard).Admits(scale)
                    ? Validation<Error, DrawingScale>.Success(scale)
                    : Validation<Error, DrawingScale>.Fail(new KernelFault.InvalidValue(Label: nameof(scale), Requirement: "a rung of the standard's scale ladder", Key: Some(op))),
                LineGroup.For(size: size, key: op).ToValidation(),
                PdfTrait.Law.Admit(conformance).ToValidation())
            .Apply((admittedScale, group, traits) => new PlotPolicy(
                size: size, orientation: orientation, frame: SheetFrame.For(size.Standard), scale: admittedScale, group: group,
```

## Why
`SheetSize` is admitted once by its private case factories after the preceding move. Re-lifting its weaker `IsValid` predicate into the applicative adds a gate that cannot fail, and `PlotPolicy.IsValid` then exposes only that same subset while ignoring the scale, group, and conformance admissions that actually define the policy.

## Change
Remove the redundant sheet gate from the `Validation` fan-in, construct from the admitted size directly, and delete the misleading post-construction validity surface.

## Ripples
At `libs/dotnet/Rasm.Fabrication/.planning/Documentation/projection.md:158`, delete the leading `!plot.IsValid ||` clause from `ProjectionPolicy.ValidateFactoryArguments`; `PlotPolicy` is privately constructed only after all of its gates succeed.

## Delta
`LOC: -3; symbols: -1`
