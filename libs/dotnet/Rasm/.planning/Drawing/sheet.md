# [RASM_SHEET]

`Rasm.Drawing` owns the branch's ONE drawing-standards catalog: the publishing bodies as a keyed vocabulary, the sheet series whose extents DERIVE from each standard's own formula, the frame a standard draws around a sheet — binding margin, zone module, title-block rectangle and field roster — the preferred scale ladders and the ratio value they publish, the sheet-identity and layer-name grammars of the naming standards with their host projections, the line-width, line-type, and plot-style tables, the lettering ladder and the annotation proportions that derive from it, the units-and-precision rule a scale implies, and the plot policy that binds those values into one issued sheet. Every one of those facts was hand-rolled or hardcoded per consumer — the census (`.claude/scratch/dotnet-solution/census/sheet/*.md`) counted a duplicated fifteen-row extent roster, eight authorities over one plot weight, seven bare lettering literals, four unowned grammars, and a `1:50` that existed only in a comment — and this owner is where each becomes one row, one formula, or one grammar with its consumers untouched or loudly broken.

Composition is downward: `ModelUnit`/`UnitSystem` from `Domain/context` carry every unit projection, `Dimension`/`PositiveMagnitude`/`UnitInterval`/`VectorAngle`/`EpsilonPolicy` from `Numerics/atoms` carry every count, magnitude, fraction, bearing, and floor, `PerceptualColor` from `Numerics/atoms` carries every plot colour, `Op`/`Fault`/`Fin`/`Validation` from `Domain/results` carry every admission, and UnitsNet `Length` carries every extent and width INSIDE the owner — kernel measures still LEAVE as bare `double` through `In(ModelUnit)` per the branch ruling. Every literal on this page names the standard clause it transcribes; a value the standard derives is a formula row here and never a table.

## [01]-[INDEX]

- [02]-[STANDARD]: `SheetStandard`, `PublishingBody`, `RungLadder` — the publishing bodies, the standard-per-concern discriminant every roster keys on with its deferral index, and the one preferred-number snap every ladder reads.
- [03]-[SERIES]: `SheetSeries`, `SheetSize`, `SheetOrientation`, `SheetMargin` — algorithmic and declared extent series, the admitted size, orientation, and the binding-aware margin quad.
- [04]-[FRAME]: `SheetFrame`, `FrameBand`, `ZoneGrid`, `ZoneRef`, `RevisionIndex`, `Revision`, `TitleBlock`, `TitleField`, `TitleBlockLayout`, `SheetOfGrammar` — the standard's extent-banded frame, zone designators, revision letters, and title-block field roster.
- [05]-[SCALE]: `DrawingScale`, `ScaleNotation`, `ScaleLadder` — the ratio value, its notations, and the preferred ladders.
- [06]-[IDENTITY]: `NamingStandard`, `NamingField`, `DisciplineDesignator`, `SheetType`, `ContainerType`, `ContainerRole`, `SheetNumber` — sheet-identity grammars and their designator vocabularies.
- [07]-[LAYER]: `LayerStandard`, `LayerField`, `LayerStatus`, `LayerName`, `HostLayerScheme` — layer-naming grammars, the admitted name, and the host projections.
- [08]-[LINEWORK]: `PenCode`, `LineWidth`, `LineGroup`, `LineType`, `PlotPosture`, `AciIndex`, `StyleName`, `PlotStyleKey`, `PlotStyle`, `PlotStyleTable` — the ISO 9175-1 pen vocabulary, the width ladder, the line groups per standard, ISO line types with derived rhythms, and the typed pen table.
- [09]-[LETTERING]: `TextHeight`, `LetteringForm`, `DraftingMetrics`, `Terminator`, `DatumDesignator`, `DatumRegime`, `GeometricCharacteristic`, `ZoneModifier`, `SymbolSet` — the height ladder, the GD&T characteristic and modifier vocabularies per publishing standard, and every lettering and annotation proportion that derives from h.
- [10]-[UNITS]: `DrawingUnits`, `DrawingPrecisionForm`, `DrawingPrecision`, `ProjectionAngle`, `NorthPosture` — the declared drawing unit, the shape its precision publishes, the scale-implied quantum, the first/third-angle projection convention, and the north convention.
- [11]-[PLOT]: `PlotResolution`, `PdfTrait`, `LayerEmission`, `IssuePosture`, `PlotPolicy` — the issued-sheet policy the host PDF and print policies compose, and the issuing convention per standard it reads its defaults from.

## [02]-[STANDARD]

- Owner: `RungLadder` — the ONE preferred-number snap, log-distance minimum over a roster's frozen log column, read by `LineWidth`, `TextHeight`, and `ScaleLadder` each over its own private log cache; `PublishingBody` `[SmartEnum<string>]` — the four bodies whose documents this page transcribes; `SheetStandard` `[SmartEnum<string>]` — the discriminant every roster on this page keys on, one row per standards FAMILY a drawing set is issued under: `Iso` (ISO 216 sizes, ISO 5457 frame, ISO 7200 title block, ISO 5455 scales, ISO 128 linework, ISO 3098 lettering, ISO 129 dimensioning), `Ansi` (ASME Y14.1 sheets and frames, ASME Y14.2 linework, ASME Y14.5, US NCS naming), `Arch` (the US architectural sheet series under the same NCS/AIA naming and ASME linework), `Jis` (JIS P 0138 B-series, JIS Z 8311 frame, JIS Z 8313 lettering, ISO-aligned linework).
- Cases: `Iso` · `Ansi` · `Arch` · `Jis` — the wire keys `iso`/`ansi`/`arch`/`jis` every consuming surface and exported document reads, ordinal-compared, never culture-folded.
- Law: the standard is the SOURCE discriminant and nothing else — a `SheetSize` row, a frame row, a scale ladder, a lettering ladder, and a plot style each carry a `Standard` column and derive their `For(standard)` index through `SheetStandard.Index` behind an accessor-backed `Lazy`, so no roster restates the standard's own membership.
- Law: `Defers` names the standard a family falls to where it publishes no convention of its own (JIS falls to ISO, ARCH to ANSI, the two roots to themselves), and `Index` resolves that hop ONCE at roster freeze — eight `For` reads on this page share the one law, and an unrostered root raises at static init rather than per read.
- Law: `Unit` is the paper unit the standard PUBLISHES its extents in (millimetres for ISO and JIS, inches for ANSI and ARCH); a consumer reads a sheet in any admitted regime through `SheetSize.In`, so the publication unit is provenance and never a second unit constant.
- Growth: a new standards family is one `SheetStandard` row naming the standard it defers to, plus one row on each concern roster that differs; a family sharing another's linework or lettering carries no row on that roster and `Index` resolves its `For` read to the row it defers to.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[KeyMemberEqualityComparer]`), System.Collections.Frozen (`FrozenDictionary`), System.Numerics.Tensors (`TensorPrimitives.Subtract`, `IndexOfMinMagnitude`), `Domain/context` (`UnitSystem`).
- Boundary: the AEC analysis-discipline vocabulary (`Rasm.Element` `Discipline`) and the IFC semantic classification (`Rasm.Element` `Classification`) are NOT drafting vocabularies — `electrical` and `fire` are live tokens in both senses — and nothing on this page composes them; the drafting designators live at `[06]`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics.Tensors;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PublishingBody {
    public static readonly PublishingBody Iso = new(key: "iso");
    public static readonly PublishingBody Asme = new(key: "asme");
    public static readonly PublishingBody Ncs = new(key: "ncs");
    public static readonly PublishingBody Jis = new(key: "jis");
    public static readonly PublishingBody Bsi = new(key: "bsi");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetStandard {
    public static readonly SheetStandard Iso = new(key: "iso", body: PublishingBody.Iso, unit: UnitSystem.Millimeters, defers: static () => Iso);
    public static readonly SheetStandard Ansi = new(key: "ansi", body: PublishingBody.Asme, unit: UnitSystem.Inches, defers: static () => Ansi);
    public static readonly SheetStandard Arch = new(key: "arch", body: PublishingBody.Ncs, unit: UnitSystem.Inches, defers: static () => Ansi);
    public static readonly SheetStandard Jis = new(key: "jis", body: PublishingBody.Jis, unit: UnitSystem.Millimeters, defers: static () => Iso);

    public PublishingBody Body { get; }
    public UnitSystem Unit { get; }
    private readonly Func<SheetStandard> defers;
    public SheetStandard Defers => defers();

    internal static FrozenDictionary<SheetStandard, TRow> Index<TRow>(IEnumerable<TRow> rows, Func<TRow, SheetStandard> column) where TRow : notnull {
        Dictionary<SheetStandard, TRow> own = rows.ToDictionary(column, static row => row);
        return Items.ToFrozenDictionary(static standard => standard, standard => own.TryGetValue(standard, out TRow? row) ? row : own[standard.Defers]);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RungLadder {
    private const int StackRungs = 64;
    internal static int NearestIndex(ReadOnlySpan<double> logs, double magnitude) {
        Span<double> distance = logs.Length <= StackRungs ? stackalloc double[logs.Length] : new double[logs.Length];
        TensorPrimitives.Subtract(logs, Math.Log(magnitude), distance);
        return TensorPrimitives.IndexOfMinMagnitude<double>(distance);
    }
}
```

## [03]-[SERIES]

- Owner: `SheetSeries` `[SmartEnum<string>]` — one row per extent series, each carrying its standard, its wire prefix, its index range, and the ONE `Extent(index)` derivation: the ISO A/B/C and JIS B series are FORMULAS (a root extent and the halving rule ISO 216 §5 states — the longer side halves and each dimension rounds DOWN to the whole millimetre), the ANSI and ARCH series are declared rosters (ASME Y14.1 Table 1; the US architectural series ASME Y14.1 does not define — provenance stated as the AIA/NCS convention); `SheetSize` `[Union]` — a rostered `(Series, Index)` pair or a `Custom(Length, Length, SheetStandard)` extent a caller carries, with `Width`/`Height` DERIVED, `Standard` total on both arms, `Key` the wire spelling, `In(ModelUnit)` the one unit projection, and `[ObjectFactory<string>]` admitting the wire key grammar (`a3`, `b1`, `c4`, `ansi-b`, `arch-d`, `jis-b4`, `custom-iso-210x297mm`); `SheetOrientation` `[SmartEnum<string>]` — `Portrait` or `Landscape` as ROWS whose `Extent(size)` column swaps the published pair, so no size row exists twice; `SheetMargin` `[ComplexValueObject]` — the binding-aware margin quad in millimetres, `Left` being the binding edge.
- Cases: `IsoA` root 841 × 1189 (ISO 216 §5.1: A0 area 1 m², aspect 1:√2, rounded to the millimetre) · `IsoB` root 1000 × 1414 (§5.2: geometric mean of A(n) and A(n−1); B0 short side is exactly 1000) · `IsoC` root 917 × 1297 (§5.3: geometric mean of A(n) and B(n)) · `JisB` root 1030 × 1456 (JIS P 0138: B0 area 1.5 m²) — all four halve by `Halved`, indices 0-10 (ISO 216 publishes A0-A10, B0-B10, C0-C10; JIS B0-B10) · `Ansi` A 8.5 × 11, B 11 × 17, C 17 × 22, D 22 × 34, E 34 × 44 in (ASME Y14.1 Table 1, letters A-E; F 28 × 40 in is the one non-doubling row and is carried) · `Arch` A 9 × 12, B 12 × 18, C 18 × 24, D 24 × 36, E 36 × 48, E1 30 × 42 in (US architectural series, 3:4 aspect; no ASME table publishes it, so the row states the AIA convention as its provenance).
- Entry: `SheetSize.Of` is ONE entrypoint discriminating on input SHAPE — `Of(series, index, key)` mints a rostered size (an index outside the series range refuses), `Of(width, height, standard, key)` admits a caller extent under the standard it is issued against (both extents positive, finite), and `Of(width, height, unit, standard, key)` admits a host triple through the millimetre base; `SheetSize.Validate(string)` / the generated `IParsable` `Parse` — the `[ObjectFactory<string>]` admission — read a wire key; `size.In(unit, key)` projects the portrait pair into any admitted regime; `orientation.Extent(size)` reads the pair oriented; `SheetFrame.For(size.Standard).Margin(size)` reads the standard's own frame margins (`[04]`) so a `PageFrame` composes them rather than re-authoring an inset.
- Auto: `Halved(index)` derives every ISO and JIS extent — `(w, h)` at index n is `(floor(h_{n−1} / 2), w_{n−1})` off the root — so A4 = 210 × 297 and B5 = 176 × 250 are computed, never stored, and a new index is nothing; the ISO 216 rounding is DOWN to the whole millimetre for every halving (A1's 594.5 → 594) while the ROOT rounds to the NEAREST millimetre (A0's 840.9 → 841), which is why the root is data and the halving is the formula.
- Law: `Key` is the ONE wire spelling — series prefix followed by index for the halving series (`a3`, `b1`, `c4`, `jis-b4`), series prefix followed by its own suffix for the declared series (`ansi-b`, `arch-d`); a `Custom` extent spells `custom-{standard}-{width}x{height}mm` at round-trip precision, so two customs differing at any bit key apart and the standard survives the wire, and it never collides with a rostered key because no series prefix begins with `custom`.
- Law: `In` refuses typed rather than scaling silently — an unadmitted unit regime fails at `ModelUnit`, so a sheet extent never crosses a boundary carrying a scale nobody admitted; the projection composes `ModelUnit.ScaleTo` off the millimetre base, and printer points (`UnitSystem.PrinterPoints`) are one more admitted regime, never a `72/25.4` constant at a consumer.
- Packages: UnitsNet (`Length.FromMillimeters`/`FromInches`, `Millimeters`, `Inches`, `operator *(Length, double)`, `operator <=`), Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ComplexValueObject]`, `[ObjectFactory<string>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Fin`, `Option`, `Range`, `ThrowIfFail`), System.Collections.Frozen (`FrozenDictionary`), `Domain/context` (`ModelUnit`, `UnitSystem`), `Domain/results` (`Op`, `Fault`, `ValidityClaim`).
- Growth: a new halving series is one row naming its root; a new declared series is one row naming its table; a new size on a declared series is one row on that table; a new unit regime is nothing.
- Boundary: the AppUi twin roster (`Rasm.AppUi/.planning/Render/drafting.md:45-60`, fifteen rows character-identical), the Rhino free struct (`Rasm.Rhino/.planning/Exchange/sheets.md:929` `SheetSize(LengthUnit, double, double)`), the AppUi points constant (`drafting.md:71-75`), the AppUi free-token page roster (`Document/export.md:1248`), and the AppUi centimetre report pair (`export.md:217`) all DELETE and read this owner — `Custom` is the one caller-override arm the Rhino struct needed. NAMED LOSS: the fifteen per-size static members (`SheetSize.A3`) — a consumer spells `SheetSize.Of(SheetSeries.IsoA, 3)` or parses `a3`; the sizes a series admits are its `Range` and never a hand roster.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate Fin<(Length Width, Length Height)> ExtentRule(int index, Op key);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetSeries {
    public static readonly SheetSeries IsoA = new(key: "iso-a", standard: SheetStandard.Iso, prefix: "a", bounds: (0, 10), suffixes: Seq<string>(),
        extent: Halving(root: Mm(841, 1189)));
    public static readonly SheetSeries IsoB = new(key: "iso-b", standard: SheetStandard.Iso, prefix: "b", bounds: (0, 10), suffixes: Seq<string>(),
        extent: Halving(root: Mm(1000, 1414)));
    public static readonly SheetSeries IsoC = new(key: "iso-c", standard: SheetStandard.Iso, prefix: "c", bounds: (0, 10), suffixes: Seq<string>(),
        extent: Halving(root: Mm(917, 1297)));
    public static readonly SheetSeries JisB = new(key: "jis-b", standard: SheetStandard.Jis, prefix: "jis-b", bounds: (0, 10), suffixes: Seq<string>(),
        extent: Halving(root: Mm(1030, 1456)));
    public static readonly SheetSeries Ansi = new(key: "ansi", standard: SheetStandard.Ansi, prefix: "ansi-", bounds: (0, 5), suffixes: Seq("a", "b", "c", "d", "e", "f"),
        extent: Declared(In(8.5, 11), In(11, 17), In(17, 22), In(22, 34), In(34, 44), In(28, 40)));
    public static readonly SheetSeries Arch = new(key: "arch", standard: SheetStandard.Arch, prefix: "arch-", bounds: (0, 5), suffixes: Seq("a", "b", "c", "d", "e", "e1"),
        extent: Declared(In(9, 12), In(12, 18), In(18, 24), In(24, 36), In(36, 48), In(30, 42)));

    public SheetStandard Standard { get; }
    public string Prefix { get; }
    public (int Floor, int Ceiling) Bounds { get; }
    public Seq<string> Suffixes { get; }
    internal ExtentRule Extent { get; }

    internal Length WidthAt(int index) => SheetSize.Of(series: this, index: index).ThrowIfFail().Width;

    internal string Spell(int index) => Suffixes.IsEmpty ? Prefix + index.ToString(CultureInfo.InvariantCulture) : Prefix + Suffixes[index];
    internal Fin<int> Index(string suffix, Op key) =>
        Suffixes.IsEmpty
            ? int.TryParse(suffix, NumberStyles.None, CultureInfo.InvariantCulture, out int index) && index >= Bounds.Floor && index <= Bounds.Ceiling
                ? Fin.Succ(index)
                : Fin.Fail<int>(key.InvalidInput())
            : Suffixes.Map(static (suffix, index) => (suffix, index)).Find(pair => string.Equals(pair.Item1, suffix, StringComparison.Ordinal))
                .Map(static pair => pair.Item2).ToFin(key.InvalidInput());

    private static (Length Width, Length Height) Mm(double width, double height) => (Length.FromMillimeters(width), Length.FromMillimeters(height));
    private static (Length Width, Length Height) In(double width, double height) => (Length.FromInches(width), Length.FromInches(height));
    private static ExtentRule Halving((Length Width, Length Height) root) => (index, key) =>
        index < 0 ? Fin.Fail<(Length, Length)>(key.InvalidInput())
        : Fin.Succ(Enumerable.Range(0, index).Aggregate(root, static (held, _) =>
            (Width: Length.FromMillimeters(Math.Floor(held.Height.Millimeters / 2.0)), Height: held.Width)));
    private static ExtentRule Declared(params (Length Width, Length Height)[] table) => (index, key) =>
        index >= 0 && index < table.Length ? Fin.Succ(table[index]) : Fin.Fail<(Length, Length)>(key.InvalidInput());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetOrientation {
    public static readonly SheetOrientation Portrait = new(key: "portrait", extent: static size => (size.Width, size.Height));
    public static readonly SheetOrientation Landscape = new(key: "landscape", extent: static size => (size.Height, size.Width));
    [UseDelegateFromConstructor] public partial (Length Width, Length Height) Extent(SheetSize size);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>]
public abstract partial record SheetSize : IValidityEvidence {
    private SheetSize() { }
    public sealed record Rostered : SheetSize {
        internal Rostered(SheetSeries series, int index) => (Series, Index) = (series, index);
        public SheetSeries Series { get; }
        public int Index { get; }
    }
    public sealed record Custom : SheetSize {
        internal Custom(Length width, Length height, SheetStandard standard) => (Width, Height, Standard) = (width, height, standard);
        public Length Width { get; }
        public Length Height { get; }
        public SheetStandard Standard { get; }
    }

    public static Fin<SheetSize> Of(SheetSeries series, int index, Op? key = null) =>
        index >= series.Bounds.Floor && index <= series.Bounds.Ceiling
            ? Fin.Succ<SheetSize>(new Rostered(series: series, index: index))
            : Fin.Fail<SheetSize>(key.OrDefault().InvalidInput());
    public static Fin<SheetSize> Of(Length width, Length height, SheetStandard standard, Op? key = null) =>
        width > Length.Zero && height > Length.Zero && double.IsFinite(width.Millimeters) && double.IsFinite(height.Millimeters)
            ? Fin.Succ<SheetSize>(new Custom(width: width, height: height, standard: standard))
            : Fin.Fail<SheetSize>(key.OrDefault().InvalidInput());
    public static Fin<SheetSize> Of(double width, double height, ModelUnit unit, SheetStandard standard, Op? key = null) {
        Op op = key.OrDefault();
        return from scale in MillimetreScale(unit: unit, key: op)
               from admitted in Of(width: Length.FromMillimeters(width * scale.From), height: Length.FromMillimeters(height * scale.From), standard: standard, key: op)
               select admitted;
    }
    internal static Length Unbounded => Length.FromMillimeters(double.PositiveInfinity);
    internal static Fin<(double From, double Into)> MillimetreScale(ModelUnit unit, Op key) =>
        from millimetres in ModelUnit.Of(value: UnitSystem.Millimeters, key: key)
        from inward in unit.ScaleTo(target: millimetres, key: key)
        from outward in millimetres.ScaleTo(target: unit, key: key)
        select (inward, outward);

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out SheetSize? item) {
        item = null;
        Op key = Op.Of();
        Fin<SheetSize> parsed = Optional(value).ToFin(key.InvalidInput()).Bind(text =>
            text.StartsWith("custom-", StringComparison.Ordinal)
                ? CustomOf(text: text.AsSpan(7), key: key)
                : ByPrefixLength.Value
                    .Find(row => text.StartsWith(row.Prefix, StringComparison.Ordinal))
                    .ToFin(key.InvalidInput())
                    .Bind(row => row.Index(suffix: text[row.Prefix.Length..], key: key).Bind(index => Of(series: row, index: index, key: key))));
        return parsed.Match(
            Succ: size => { item = size; return null; },
            Fail: static _ => new ValidationError(message: "SheetSize requires a rostered key (a3, ansi-b, arch-d, jis-b4) or custom-{standard}-{w}x{h}mm."));
    }
    private static readonly Lazy<Seq<SheetSeries>> ByPrefixLength =
        new(static () => toSeq(toSeq(SheetSeries.Items).OrderByDescending(static row => row.Prefix.Length)).Strict());
    private static Fin<SheetSize> CustomOf(ReadOnlySpan<char> text, Op key) {
        int dash = text.IndexOf('-');
        int cross = text.IndexOf('x');
        return dash > 0 && cross > dash && text.EndsWith("mm", StringComparison.Ordinal)
            && SheetStandard.TryGet(text[..dash].ToString(), out SheetStandard? standard)
            && double.TryParse(text[(dash + 1)..cross], NumberStyles.Float, CultureInfo.InvariantCulture, out double width)
            && double.TryParse(text[(cross + 1)..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out double height)
            ? Of(width: Length.FromMillimeters(width), height: Length.FromMillimeters(height), standard: standard, key: key)
            : Fin.Fail<SheetSize>(key.InvalidInput());
    }

    public Length Width => Extent.Width;
    public Length Height => Extent.Height;
    private (Length Width, Length Height) Extent => Switch(
        rostered: static row => Ladder.Value[(row.Series, row.Index)],
        custom: static row => (row.Width, row.Height));
    private static readonly Lazy<FrozenDictionary<(SheetSeries Series, int Index), (Length Width, Length Height)>> Ladder =
        new(static () => toSeq(SheetSeries.Items)
            .Bind(static series => Range(series.Bounds.Floor, series.Bounds.Ceiling - series.Bounds.Floor + 1).ToSeq().Map(index => (Series: series, Index: index)))
            .ToFrozenDictionary(static seat => seat, static seat => seat.Series.Extent(seat.Index, Op.Of()).ThrowIfFail()));
    public SheetStandard Standard => Switch(rostered: static row => row.Series.Standard, custom: static row => row.Standard);
    public string Key => Switch(
        rostered: static row => row.Series.Spell(row.Index),
        custom: static row => string.Create(CultureInfo.InvariantCulture, $"custom-{row.Standard.Key}-{row.Width.Millimeters:R}x{row.Height.Millimeters:R}mm"));
    public bool IsValid => ValidityClaim.All(Width > Length.Zero, Height > Length.Zero);

    public Fin<(double Width, double Height)> In(ModelUnit unit, Op? key = null) =>
        MillimetreScale(unit: unit, key: key.OrDefault()).Map(scale => (Width.Millimeters * scale.Into, Height.Millimeters * scale.Into));
}

[ComplexValueObject]
public sealed partial class SheetMargin {
    public Length Left { get; }
    public Length Top { get; }
    public Length Right { get; }
    public Length Bottom { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Length left, ref Length top, ref Length right, ref Length bottom) =>
        validationError = ValidityClaim.All(
            ValidityClaim.Nonnegative(left.Millimeters), ValidityClaim.Nonnegative(top.Millimeters),
            ValidityClaim.Nonnegative(right.Millimeters), ValidityClaim.Nonnegative(bottom.Millimeters))
            ? null
            : new ValidationError(message: "SheetMargin requires finite non-negative insets.");

    public Fin<(double Left, double Top, double Right, double Bottom)> In(ModelUnit unit, Op? key = null) =>
        SheetSize.MillimetreScale(unit: unit, key: key.OrDefault())
            .Map(scale => (Left.Millimeters * scale.Into, Top.Millimeters * scale.Into, Right.Millimeters * scale.Into, Bottom.Millimeters * scale.Into));
}
```

## [04]-[FRAME]

- Owner: `SheetFrame` `[SmartEnum<string>]` — one row per standard carrying the frame geometry the standard publishes, banded by extent: binding and free margins, the zone module (ISO 5457 §5.3: 50 mm reference-grid divisions, letters down the short edge and numbers along the long edge), the centring-mark tick length, and the title-block rectangle; `FrameBand` — one extent regime of that geometry; `ZoneGrid` — the derived division counts for a size under a frame; `ZoneRef` `[ObjectFactory<string>]` — the zone designator (`B3`) a callout cites; `TitleBlockLayout` `[SmartEnum<string>]` — the ISO 7200 / ASME Y14.1 / JIS Z 8311 title-block rectangle and its field pitch; `TitleField` `[SmartEnum<string>]` — the ISO 7200 data-field roster, each row carrying its `Read` over the typed `TitleBlock`; `TitleBlock` — the typed record the field roster reads (typed scale, typed sheet number, typed revision, never free strings for facts another owner types); `SheetOfGrammar` — the `n/m` versus `n OF m` sheet-count spelling per standard.
- Cases: `SheetFrame.Iso` one band — binding 20 mm, other edges 10 mm, module 50 × 50 mm, ticks 5 mm, block 180 × 55 mm anchored bottom-right (ISO 5457 §5.2-5.4, ISO 7200 §5) · `SheetFrame.Ansi` two bands — margins 0.5 in throughout, no reference grid to ANSI C, then the 4.25 × 5.5 in module on D and above (ASME Y14.1 Fig 1-3), block 6.5 × 2.5 in · `SheetFrame.Arch` one band — binding 1.5 in, other edges 0.5 in, module 4.25 × 5.5 in, block 6.5 × 2.5 in (NCS UDS Module 02 sheet frame) · `SheetFrame.Jis` one band matching ISO with a 170 × 50 mm block (JIS Z 8311).
- Entry: `SheetFrame.For(standard)` (the `SheetStandard.Index` read); `frame.Margin(size, key)` and `frame.Zones(size, orientation, key)` → `ZoneGrid`, both folding the frame's own extent bands; `frame.Block` → the block rectangle; `ZoneRef.Of(column, row, key)` / `Validate("B3")`; `TitleBlockLayout.For(standard).Rows`/`.Pitch`; `field.Read(block, standard)`; `SheetOfGrammar.For(standard).Render(n, m)`; `TitleBlock.Of(…)` and `Revision.Of(index, date, description)`.
- Auto: zone counts derive from the extent over the band's module (A0 → 24 × 16, A1 → 16 × 12, A2 → 12 × 8, A3 → 8 × 6, A4 → 6 × 4 — computed, never a table); the zone designator is `{letter}{number}` with letters from the top and numbers from the left; the block anchors bottom-right inside the frame; the field pitch derives from the block height and the row count.
- Law: frame geometry BANDS by extent where the standard publishes one — ASME Y14.1 zones the D and E sheets on a module the A-C sheets carry none of — so `Margin` and `Zones` fold the band roster and an extent past the last band, or a band publishing no reference grid, REFUSES typed rather than answering a fabricated grid.
- Law: `TitleBlock.Scale` is a `DrawingScale`, `TitleBlock.Number` a `SheetNumber`, and `Revision.Index` a `RevisionIndex` over the ASME Y14.35 §4.3 letter sequence — a title block cannot claim a scale the projection does not use, a number no grammar admits, or a revision letter the sequence skips (the AppUi block carried all three as free strings); `Of` is the ONE mint and the ctor is private, so `Sheet`/`SheetCount` admit against each other rather than guarding at a later read.
- Packages: UnitsNet (`Length`), NodaTime (`LocalDate`, `LocalDatePattern.Iso`), Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[ComplexValueObject]`, `[ValueObject]`, `[ObjectFactory<string>]`), LanguageExt.Core (`Fin`, `Option`, `Validation` applicative), System.Collections.Frozen (`FrozenDictionary`), Rasm.Domain (`Op`, `Fault`, `ValidityClaim`, `AcceptValidated`).
- Growth: a new frame convention is one `SheetFrame` row and a size-dependent one is one more band on it; a new title-block field is one `TitleField` row reading the record; a new block layout is one row; a family sharing another's frame, layout, or sheet-count spelling carries no row at all.
- Boundary: the AppUi `TitleBlockStandard`/`TitleField`/`TitleBlock` (`Render/drafting.md:28-42,86-124`) delete and compose these rows — its eleven-field delegate roster is the richest form and survives here as the `TitleField` shape; the drafting SURFACE that strokes the frame stays the consumer's (`DraftEmit.TitleLayout` reads `SheetFrame`, `ZoneGrid`, `TitleBlockLayout`, and the fields, and draws). `Interaction/chrome` `PageFrame.Sheet` composes `SheetMargin` in place of its own `PageMargin`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct FrameBand(Length Ceiling, Length Binding, Length Edge, Option<(Length X, Length Y)> Module);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetFrame {
    public static readonly SheetFrame Iso = new(key: "iso", standard: SheetStandard.Iso, tick: Length.FromMillimeters(5),
        block: (Length.FromMillimeters(180), Length.FromMillimeters(55)),
        bands: Seq(new FrameBand(Ceiling: SheetSize.Unbounded, Binding: Length.FromMillimeters(20), Edge: Length.FromMillimeters(10),
            Module: Some((Length.FromMillimeters(50), Length.FromMillimeters(50))))));
    public static readonly SheetFrame Ansi = new(key: "ansi", standard: SheetStandard.Ansi, tick: Length.FromInches(0.125),
        block: (Length.FromInches(6.5), Length.FromInches(2.5)),
        bands: Seq(
            new FrameBand(Ceiling: Length.FromInches(17), Binding: Length.FromInches(0.5), Edge: Length.FromInches(0.5), Module: None),
            new FrameBand(Ceiling: SheetSize.Unbounded, Binding: Length.FromInches(0.5), Edge: Length.FromInches(0.5),
                Module: Some((Length.FromInches(4.25), Length.FromInches(5.5))))));
    public static readonly SheetFrame Arch = new(key: "arch", standard: SheetStandard.Arch, tick: Length.FromInches(0.125),
        block: (Length.FromInches(6.5), Length.FromInches(2.5)),
        bands: Seq(new FrameBand(Ceiling: SheetSize.Unbounded, Binding: Length.FromInches(1.5), Edge: Length.FromInches(0.5),
            Module: Some((Length.FromInches(4.25), Length.FromInches(5.5))))));
    public static readonly SheetFrame Jis = new(key: "jis", standard: SheetStandard.Jis, tick: Length.FromMillimeters(5),
        block: (Length.FromMillimeters(170), Length.FromMillimeters(50)),
        bands: Seq(new FrameBand(Ceiling: SheetSize.Unbounded, Binding: Length.FromMillimeters(20), Edge: Length.FromMillimeters(10),
            Module: Some((Length.FromMillimeters(50), Length.FromMillimeters(50))))));

    public SheetStandard Standard { get; }
    public Length Tick { get; }
    public (Length Width, Length Height) Block { get; }
    public Seq<FrameBand> Bands { get; }

    public static SheetFrame For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, SheetFrame>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));

    public Fin<SheetMargin> Margin(SheetSize size, Op? key = null) {
        Op op = key.OrDefault();
        return Band(size: size, key: op).Bind(band =>
            op.AcceptValidated<SheetMargin>(SheetMargin.Validate(band.Binding, band.Edge, band.Edge, band.Edge, out SheetMargin? margin), margin));
    }
    public Fin<ZoneGrid> Zones(SheetSize size, SheetOrientation orientation, Op? key = null) {
        Op op = key.OrDefault();
        (Length width, Length height) = orientation.Extent(size);
        return from band in Band(size: size, key: op)
               from module in band.Module.ToFin(new KernelFault.InvalidValue(Label: nameof(ZoneGrid), Requirement: "a standard drawing a reference grid at this extent", Key: Some(op)))
               select new ZoneGrid(Columns: Math.Max(1, (int)Math.Floor(width / module.X)), Rows: Math.Max(1, (int)Math.Floor(height / module.Y)),
                   ModuleX: module.X, ModuleY: module.Y);
    }
    private Fin<FrameBand> Band(SheetSize size, Op key) =>
        Bands.Find(band => size.Width <= band.Ceiling)
            .ToFin(new KernelFault.InvalidValue(Label: nameof(SheetFrame), Requirement: $"a frame band covering the '{size.Key}' extent", Key: Some(key)));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ZoneGrid(int Columns, int Rows, Length ModuleX, Length ModuleY) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Columns >= 1, Rows >= 1, ModuleX > Length.Zero, ModuleY > Length.Zero);
    public Fin<ZoneRef> At(int column, int row, Op key) =>
        column >= 1 && column <= Columns && row >= 1 && row <= Rows
            ? ZoneRef.Of(column: column, row: row, key: key)
            : Fin.Fail<ZoneRef>(key.InvalidInput());
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class ZoneRef {
    public int Column { get; }
    public int Row { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int column, ref int row) =>
        validationError = column >= 1 && row >= 1 && row <= 26 ? null : new ValidationError(message: "ZoneRef requires a positive column and a row A-Z.");
    public static Fin<ZoneRef> Of(int column, int row, Op key) => key.AcceptValidated<ZoneRef>(Validate(column, row, out ZoneRef? zone), zone);
    public string Text => string.Create(CultureInfo.InvariantCulture, $"{(char)('A' + Row - 1)}{Column}");
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out ZoneRef? item) {
        item = null;
        return value is [var letter, .. var digits] && letter is >= 'A' and <= 'Z'
            && int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int column)
            && Validate(column, letter - 'A' + 1, out item) is null
            ? null
            : new ValidationError(message: "ZoneRef requires the {letter}{number} grammar, e.g. B3.");
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RevisionIndex {
    private const string Alphabet = "ABCDEFGHJKLMNPRTUVWY";
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToUpperInvariant();
        validationError = value.Length > 0 && value.All(static letter => Alphabet.Contains(letter, StringComparison.Ordinal))
            ? null
            : new ValidationError(message: "RevisionIndex admits the ASME Y14.35 letters A-Y (I, O, Q, S, X, Z excluded), extending as AA, AB.");
    }
    public static Fin<RevisionIndex> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<RevisionIndex>(Validate(value, out RevisionIndex? index), index);
    public Fin<RevisionIndex> Next(Op? key = null) => Of(value: Advance(held: ToValue()), key: key);
    private static string Advance(string held) =>
        held.Length == 0 ? Alphabet[..1]
        : Alphabet.IndexOf(held[^1], StringComparison.Ordinal) + 1 is int seat && seat < Alphabet.Length
            ? string.Concat(held[..^1], Alphabet.AsSpan(seat, 1))
            : string.Concat(Advance(held: held[..^1]), Alphabet.AsSpan(0, 1));
}

[ComplexValueObject]
public sealed partial class Revision {
    public RevisionIndex Index { get; }
    public LocalDate Date { get; }
    public string Description { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref RevisionIndex index, ref LocalDate date, ref string description) {
        description = description.Trim();
        validationError = description.Length > 0 ? null : new ValidationError(message: "Revision requires a description.");
    }
    public static Fin<Revision> Of(RevisionIndex index, LocalDate date, string description, Op? key = null) =>
        key.OrDefault().AcceptValidated<Revision>(Validate(index, date, description, out Revision? revision), revision);
}

public sealed record TitleBlock {
    private TitleBlock(string owner, string project, string client, string title, Option<string> supplement,
        SheetNumber number, DisciplineDesignator discipline, DrawingScale scale, DrawingUnits units,
        LocalDate date, Option<Revision> revision, string drawn, Option<string> checkedBy, Option<string> approvedBy,
        int sheet, int sheetCount) =>
        (Owner, Project, Client, Title, Supplement, Number, Discipline, Scale, Units, Date, Revision, Drawn, Checked, Approved, Sheet, SheetCount) =
        (owner, project, client, title, supplement, number, discipline, scale, units, date, revision, drawn, checkedBy, approvedBy, sheet, sheetCount);

    public string Owner { get; }
    public string Project { get; }
    public string Client { get; }
    public string Title { get; }
    public Option<string> Supplement { get; }
    public SheetNumber Number { get; }
    public DisciplineDesignator Discipline { get; }
    public DrawingScale Scale { get; }
    public DrawingUnits Units { get; }
    public LocalDate Date { get; }
    public Option<Revision> Revision { get; }
    public string Drawn { get; }
    public Option<string> Checked { get; }
    public Option<string> Approved { get; }
    public int Sheet { get; }
    public int SheetCount { get; }

    public static Fin<TitleBlock> Of(string owner, string project, string client, string title, Option<string> supplement,
        SheetNumber number, DisciplineDesignator discipline, DrawingScale scale, DrawingUnits units,
        LocalDate date, Option<Revision> revision, string drawn, Option<string> checkedBy, Option<string> approvedBy,
        int sheet, int sheetCount, Op? key = null) {
        Op op = key.OrDefault();
        return (
                Entry(owner, nameof(Owner), op), Entry(project, nameof(Project), op), Entry(client, nameof(Client), op),
                Entry(title, nameof(Title), op), Entry(drawn, nameof(Drawn), op),
                Ordinal(sheet, nameof(Sheet), sheet >= 1, "a one-based sheet ordinal", op),
                Ordinal(sheetCount, nameof(SheetCount), sheetCount >= sheet, "a count at least the sheet ordinal", op))
            .Apply((admittedOwner, admittedProject, admittedClient, admittedTitle, admittedDrawn, admittedSheet, admittedCount) =>
                new TitleBlock(owner: admittedOwner, project: admittedProject, client: admittedClient, title: admittedTitle, supplement: supplement,
                    number: number, discipline: discipline, scale: scale, units: units, date: date, revision: revision,
                    drawn: admittedDrawn, checkedBy: checkedBy, approvedBy: approvedBy, sheet: admittedSheet, sheetCount: admittedCount))
            .As().ToFin();
    }
    private static Validation<Error, string> Entry(string value, string label, Op key) =>
        value.Trim() is { Length: > 0 } trimmed
            ? Validation<Error, string>.Success(trimmed)
            : Validation<Error, string>.Fail(new KernelFault.InvalidValue(Label: label, Requirement: "a non-blank entry", Key: Some(key)));
    private static Validation<Error, int> Ordinal(int value, string label, bool admits, string requirement, Op key) =>
        admits
            ? Validation<Error, int>.Success(value)
            : Validation<Error, int>.Fail(new KernelFault.OutOfRange(Label: label, Scalar: value, Requirement: requirement, Key: Some(key)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TitleField {
    public static readonly TitleField Owner = new(key: "owner", read: static (block, _) => block.Owner);
    public static readonly TitleField Number = new(key: "number", read: static (block, _) => block.Number.Text);
    public static readonly TitleField Title = new(key: "title", read: static (block, _) => block.Title);
    public static readonly TitleField Supplement = new(key: "supplement", read: static (block, _) => block.Supplement.IfNone(string.Empty));
    public static readonly TitleField Project = new(key: "project", read: static (block, _) => block.Project);
    public static readonly TitleField Client = new(key: "client", read: static (block, _) => block.Client);
    public static readonly TitleField Discipline = new(key: "discipline", read: static (block, _) => block.Discipline.Title);
    public static readonly TitleField Scale = new(key: "scale", read: static (block, standard) => ScaleNotation.For(standard).Render(block.Scale));
    public static readonly TitleField Units = new(key: "units", read: static (block, _) => block.Units.Key);
    public static readonly TitleField Date = new(key: "date", read: static (block, _) => LocalDatePattern.Iso.Format(block.Date));
    public static readonly TitleField Drawn = new(key: "drawn", read: static (block, _) => block.Drawn);
    public static readonly TitleField Checked = new(key: "checked", read: static (block, _) => block.Checked.IfNone(string.Empty));
    public static readonly TitleField Approved = new(key: "approved", read: static (block, _) => block.Approved.IfNone(string.Empty));
    public static readonly TitleField Sheet = new(key: "sheet", read: static (block, standard) => SheetOfGrammar.For(standard).Render(block.Sheet, block.SheetCount));
    public static readonly TitleField Revision = new(key: "revision", read: static (block, _) => block.Revision.Map(static r => r.Index.ToValue()).IfNone(string.Empty));

    [UseDelegateFromConstructor] public partial string Read(TitleBlock block, SheetStandard standard);
    public string LabelKey => string.Concat("sheet.field.", Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TitleBlockLayout {
    public static readonly TitleBlockLayout Iso = new(key: "iso", standard: SheetStandard.Iso, inset: Length.FromMillimeters(3), header: Length.FromMillimeters(8),
        rows: Seq(TitleField.Owner, TitleField.Title, TitleField.Number, TitleField.Scale, TitleField.Date, TitleField.Drawn, TitleField.Approved, TitleField.Revision, TitleField.Sheet));
    public static readonly TitleBlockLayout Ansi = new(key: "ansi", standard: SheetStandard.Ansi, inset: Length.FromInches(0.125), header: Length.FromInches(0.3),
        rows: Seq(TitleField.Owner, TitleField.Title, TitleField.Number, TitleField.Scale, TitleField.Date, TitleField.Drawn, TitleField.Approved, TitleField.Revision, TitleField.Sheet));
    public static readonly TitleBlockLayout Arch = new(key: "arch", standard: SheetStandard.Arch, inset: Length.FromInches(0.125), header: Length.FromInches(0.3),
        rows: Seq(TitleField.Owner, TitleField.Project, TitleField.Client, TitleField.Title, TitleField.Number, TitleField.Discipline, TitleField.Scale, TitleField.Date, TitleField.Drawn, TitleField.Checked, TitleField.Revision, TitleField.Sheet));
    public static readonly TitleBlockLayout Jis = new(key: "jis", standard: SheetStandard.Jis, inset: Length.FromMillimeters(3), header: Length.FromMillimeters(8),
        rows: Seq(TitleField.Owner, TitleField.Title, TitleField.Number, TitleField.Scale, TitleField.Date, TitleField.Drawn, TitleField.Approved, TitleField.Sheet));

    public SheetStandard Standard { get; }
    public Length Inset { get; }
    public Length Header { get; }
    public Seq<TitleField> Rows { get; }
    public static TitleBlockLayout For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, TitleBlockLayout>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
    public Length Pitch => (SheetFrame.For(Standard).Block.Height - Header) / Math.Max(1, Rows.Count - 1);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetOfGrammar {
    public static readonly SheetOfGrammar Slash = new(key: "slash", standard: SheetStandard.Iso, prefix: string.Empty, separator: "/");
    public static readonly SheetOfGrammar Of = new(key: "of", standard: SheetStandard.Ansi, prefix: "SHEET ", separator: " OF ");
    public static readonly SheetOfGrammar ArchOf = new(key: "arch-of", standard: SheetStandard.Arch, prefix: string.Empty, separator: " OF ");
    public SheetStandard Standard { get; }
    public string Prefix { get; }
    public string Separator { get; }
    public string Render(int sheet, int count) => string.Create(CultureInfo.InvariantCulture, $"{Prefix}{sheet}{Separator}{count}");
    public static SheetOfGrammar For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, SheetOfGrammar>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
}
```

## [05]-[SCALE]

- Owner: `DrawingScale` `[ComplexValueObject]` `[ObjectFactory<string>]` — the paper-to-model ratio as a reduced positive integer pair, admitted from any notation the standards publish and rendered through a `ScaleNotation` row; `ScaleNotation` `[SmartEnum<string>]` — `Ratio` (`1:50`, ISO 5455 §4), `Architectural` (`1/4" = 1'-0"`, US architectural convention), `Engineering` (`1" = 20'`, US engineering convention); `ScaleLadder` `[SmartEnum<string>]` — the preferred ladders: `Iso5455` is a FORMULA (the {1, 2, 5} × 10ⁿ series over reductions 1:2 … 1:10000 and enlargements 2:1 … 50:1, ISO 5455 Table 1), `Architectural` and `Engineering` are declared rosters with their upstream named; `Members` derive lazily and `Nearest(scale)` snaps a free ratio onto the ladder.
- Entry: `DrawingScale.Of(paper, model, key)` reduces and admits; `DrawingScale.Admit(text, key)` parses every notation (`1:50`, `50:1`, `1/4"=1'-0"`, `1"=20'`, `3/32"=1'`) and hands back the ADMITTING notation beside the value; `DrawingScale.Validate(text)` is the same read through the `[ObjectFactory<string>]` plane; `ScaleNotation.For(standard).Render(scale)`; `scale.Ratio` (paper / model as `double`); `ScaleLadder.For(standard)`, `ladder.Members`, `ladder.Nearest(scale)`, `ladder.Admits(scale)`.
- Auto: the ISO ladder GENERATES its members — mantissa {1, 2, 5} across `Decades` integer decades up to `Ceiling` — so `1:20000` is those two columns moving together and never a row; the imperial rosters carry their fraction spellings as data because their ratios (1:96, 1:48, 1:120) are not a mantissa series; every ladder freezes ONCE in ratio order beside its membership set and its log column, so `Members` publishes in ladder order and `Admits`/`Nearest` never scan.
- Law: full scale is `1:1`; a reduction keeps `Paper = 1`, an enlargement keeps `Model = 1`; a ratio that reduces to neither (`3:2`) is admitted as its reduced pair — the ladder decides preferredness, the value never does.
- Law: the notation that ADMITTED a text is carried out with the value, so a title block renders back the spelling its drawing set was issued in; the imperial rows discriminate on which side the text pins (architectural pins one foot of model, engineering one inch of paper), and both quantize on the AIA 1/64-in drafting denominator, which is therefore named once and never a bare `64`.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, `[ObjectFactory<string>]`, `[SmartEnum]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Fin`, `Option`, `Seq.Choose`, `Range`), System.Collections.Frozen (`FrozenDictionary`, `FrozenSet`), System.Numerics.BigInteger (`GreatestCommonDivisor`), Rasm.Domain (`Op`, `Fault`).
- Growth: a notation is one row with its parse and render delegates; a ladder is one row (a formula or a table); a family sharing another's notation or ladder carries no row and falls to the standard it defers to.
- Boundary: AppUi `ProjectionBasis.Scale`/`FrameEdit.Scale(double)`/`TitleBlock.Scale(string)` and Rhino `SheetScale`/`CaptureScale`/`VectorScale`/`StyleField.DimensionScale` all take or read `DrawingScale` — Rhino's `SheetScale.NamedCase` and `DetailViewObject.ScaleFormat` stay the host lowering (a host operator-typed spelling is host grammar), reading `Render`/`Validate` here; the annotation multiplier `DraftScale` (a pattern scale) is DISTINCT and stays.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Drawing;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class DrawingScale {
    public int Paper { get; }
    public int Model { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int paper, ref int model) {
        if (paper <= 0 || model <= 0) { validationError = new ValidationError(message: "DrawingScale requires positive paper and model terms."); return; }
        int divisor = (int)System.Numerics.BigInteger.GreatestCommonDivisor(paper, model);
        (paper, model) = (paper / divisor, model / divisor);
    }
    public static Fin<DrawingScale> Of(int paper, int model, Op? key = null) =>
        key.OrDefault().AcceptValidated<DrawingScale>(Validate(paper, model, out DrawingScale? scale), scale);
    public double Ratio => (double)Paper / Model;
    public bool IsReduction => Paper < Model;
    public static Fin<(DrawingScale Scale, ScaleNotation Notation)> Admit(string text, Op? key = null) =>
        Admitted(text).ToFin(key.OrDefault().InvalidInput());
    private static Option<(DrawingScale Scale, ScaleNotation Notation)> Admitted(string? value) =>
        Optional(value).Bind(text => toSeq(ScaleNotation.Items).Choose(row => row.Parse(text.Trim()).Map(scale => (Scale: scale, Notation: row))).Head);
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out DrawingScale? item) {
        item = null;
        Option<(DrawingScale Scale, ScaleNotation Notation)> parsed = Admitted(value);
        return parsed.Match(
            Some: pair => { item = pair.Scale; return null; },
            None: static () => new ValidationError(message: "DrawingScale requires a ratio (1:50), an architectural (1/4\"=1'-0\"), or an engineering (1\"=20') spelling."));
    }
}

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScaleNotation {
    public static readonly ScaleNotation Ratio = new(key: "ratio", standard: SheetStandard.Iso,
        parse: static text => text.Split(':') is [var p, var m] && int.TryParse(p, NumberStyles.None, CultureInfo.InvariantCulture, out int paper) && int.TryParse(m, NumberStyles.None, CultureInfo.InvariantCulture, out int model)
            ? DrawingScale.Of(paper: paper, model: model).ToOption() : None,
        render: static scale => string.Create(CultureInfo.InvariantCulture, $"{scale.Paper}:{scale.Model}"));
    public static readonly ScaleNotation Architectural = new(key: "architectural", standard: SheetStandard.Arch,
        parse: static text => Imperial(text: text).Filter(static pair => pair.ModelInches == InchesPerFoot).Bind(static pair => Quantize(pair)),
        render: static scale => string.Create(CultureInfo.InvariantCulture, $"{Vulgar(inches: InchesPerFoot * scale.Paper / scale.Model)}\" = 1'-0\""));
    public static readonly ScaleNotation Engineering = new(key: "engineering", standard: SheetStandard.Ansi,
        parse: static text => Imperial(text: text).Filter(static pair => pair.PaperInches == 1.0).Bind(static pair => Quantize(pair)),
        render: static scale => string.Create(CultureInfo.InvariantCulture, $"1\" = {scale.Model / (double)scale.Paper / InchesPerFoot:0.###}'"));

    public SheetStandard Standard { get; }
    [UseDelegateFromConstructor] internal partial Option<DrawingScale> Parse(string text);
    [UseDelegateFromConstructor] public partial string Render(DrawingScale scale);
    public static ScaleNotation For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, ScaleNotation>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));

    private const int Sixtyfourths = 64;
    private const double InchesPerFoot = 12.0;

    private static Option<(double PaperInches, double ModelInches)> Imperial(string text) {
        string[] sides = text.Replace(" ", string.Empty).Split('=');
        return sides is [var paperText, var modelText] && paperText.EndsWith('"') && Fraction(paperText[..^1]) is { IsSome: true, Case: double paperInches }
            && ModelInches(modelText) is { IsSome: true, Case: double modelInches }
            ? Some((paperInches, modelInches))
            : None;
    }
    private static Option<DrawingScale> Quantize((double PaperInches, double ModelInches) pair) =>
        DrawingScale.Of(paper: (int)Math.Round(pair.PaperInches * Sixtyfourths), model: (int)Math.Round(pair.ModelInches * Sixtyfourths)).ToOption();
    private static Option<double> Fraction(string text) =>
        text.Split('/') is [var n, var d] && double.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out double num) && double.TryParse(d, NumberStyles.Float, CultureInfo.InvariantCulture, out double den) && den > 0
            ? Some(num / den)
            : double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double whole) ? Some(whole) : None;
    private static Option<double> ModelInches(string text) {
        int feet = text.IndexOf('\'');
        if (feet < 0) { return None; }
        Option<double> ft = Fraction(text[..feet]);
        string tail = text[(feet + 1)..].TrimStart('-');
        Option<double> inches = tail.Length == 0 ? Some(0.0) : tail.EndsWith('"') ? Fraction(tail[..^1]) : None;
        return ft.Bind(f => inches.Map(i => (f * InchesPerFoot) + i));
    }
    private static string Vulgar(double inches) {
        int quantized = (int)Math.Round(inches * Sixtyfourths);
        (int whole, int part) = (quantized / Sixtyfourths, quantized % Sixtyfourths);
        if (part == 0) { return whole.ToString(CultureInfo.InvariantCulture); }
        int divisor = (int)System.Numerics.BigInteger.GreatestCommonDivisor(part, Sixtyfourths);
        string fraction = string.Create(CultureInfo.InvariantCulture, $"{part / divisor}/{Sixtyfourths / divisor}");
        return whole == 0 ? fraction : string.Concat(whole.ToString(CultureInfo.InvariantCulture), " ", fraction);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScaleLadder {
    public static readonly ScaleLadder Iso5455 = new(key: "iso-5455", standard: SheetStandard.Iso, decades: 5, ceiling: 10000, rungs: static () =>
        toSeq(new[] { 1, 2, 5 }).Bind(static mantissa => Range(0, Iso5455!.Decades).Fold(Seq<int>(), (held, _) => held.Add(held.Last.Map(static last => last * 10).IfNone(mantissa))))
            .Filter(static n => n >= 2 && n <= Iso5455!.Ceiling).Map(static n => DrawingScale.Of(paper: 1, model: n).ThrowIfFail())
            + Seq(DrawingScale.Of(1, 1).ThrowIfFail())
            + toSeq(new[] { 2, 5, 10, 20, 50 }).Map(static n => DrawingScale.Of(paper: n, model: 1).ThrowIfFail()));
    public static readonly ScaleLadder Architectural = new(key: "architectural", standard: SheetStandard.Arch, decades: 1, ceiling: 128, rungs: static () =>
        toSeq(new[] { 128, 96, 64, 48, 32, 24, 16, 12, 8, 4 }).Map(static n => DrawingScale.Of(paper: 1, model: n).ThrowIfFail()));
    public static readonly ScaleLadder Engineering = new(key: "engineering", standard: SheetStandard.Ansi, decades: 1, ceiling: 720, rungs: static () =>
        toSeq(new[] { 10, 20, 30, 40, 50, 60 }).Map(static feet => DrawingScale.Of(paper: 1, model: feet * 12).ThrowIfFail()));

    public SheetStandard Standard { get; }
    public int Decades { get; }
    public int Ceiling { get; }
    [UseDelegateFromConstructor] internal partial Seq<DrawingScale> Rungs();

    private static readonly Lazy<FrozenDictionary<ScaleLadder, (Seq<DrawingScale> Members, FrozenSet<DrawingScale> Held, double[] Logs)>> Frozen =
        new(static () => Items.ToFrozenDictionary(static row => row, static row => Freeze(rungs: row.Rungs())));
    private static (Seq<DrawingScale> Members, FrozenSet<DrawingScale> Held, double[] Logs) Freeze(Seq<DrawingScale> rungs) {
        Seq<DrawingScale> ordered = toSeq(rungs.OrderBy(static row => row.Ratio)).Strict();
        return (ordered, ordered.ToFrozenSet(), ordered.Map(static row => Math.Log(row.Ratio)).ToArray());
    }

    public Seq<DrawingScale> Members => Frozen.Value[this].Members;
    public bool Admits(DrawingScale scale) => Frozen.Value[this].Held.Contains(scale);
    public DrawingScale Nearest(DrawingScale scale) =>
        Members[RungLadder.NearestIndex(logs: Frozen.Value[this].Logs, magnitude: scale.Ratio)];
    public static ScaleLadder For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, ScaleLadder>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
}
```

## [06]-[IDENTITY]

- Owner: `ContainerType` `[SmartEnum<string>]` and `ContainerRole` `[SmartEnum<string>]` — the BS EN ISO 19650-2 UK annex information-type and role vocabularies the container identifier's two-letter fields draw from; `NamingStandard` `[SmartEnum<string>]` — the sheet-identity grammars: `Ncs` (US NCS Uniform Drawing System Module 01: `{Discipline}-{SheetType}{Sequence}`, e.g. `A-101`), `Iso19650` (BS EN ISO 19650-2 UK national annex container identifier: `{Project}-{Originator}-{Volume}-{Level}-{Type}-{Role}-{Number}`, e.g. `PRJ-ORG-Z1-01-DR-A-0001`), `Simple` (the solution's own `{Prefix}{Sequence}` for sets no client standard governs); `NamingField` `[SmartEnum<string>]` — the field vocabulary the grammars sequence; `DisciplineDesignator` `[SmartEnum<string>]` — the NCS/AIA discipline letters with their titles; `SheetType` `[SmartEnum<int>]` — the NCS sheet-type digits 0-9; `SheetNumber` `[ComplexValueObject]` — the admitted identity: a standard with its ordered field values, rendered by the standard's own `Format` and parsed by its own `Parse`.
- Cases: `DisciplineDesignator` keys the NCS UDS Module 01 Table 1 discipline letter and carries its published `Title` · `SheetType` keys the Module 01 §4.2 designator digit 0-9 and carries its published `Title`, digits 7 and 8 being the standard's own user-defined slots · `ContainerType` keys the UK annex information-type code (`DR`, `M3`, `SP`, `CR`…) and `ContainerRole` its role letter (`A`, `S`, `M`…), so the two length-2 fields of a container identifier stop being positionally interchangeable strings; the fence spells all four rosters.
- Entry: `SheetNumber.Of(standard, fields, key)` admits the field values against the standard's grammar through `Validation` (every field checked, every refusal reported, and an ABSENT field refusing apart from a present-but-invalid one); `SheetNumber.Parse(standard, text, key)`; `number.Text` renders through the standard's own sequence, delimiter, and fused seats; `SheetNumber.Ncs(discipline, type, sequence)` and `Iso19650(project, originator, volume, level, type, role, number)` are the typed mints over the two published grammars.
- Law: `Delimiter` and `Fused` are the whole rendering vocabulary — one shared `Render` joins the standard's own sequence and runs the seats a standard fuses (NCS runs its sheet-type digit into the sequence number), so a format body spelling `"-"` beside a declared delimiter is the deleted form; `CaseRule` applies ONCE at admission, mirroring `LayerName`.
- Law: a raw string carries no standard discriminant — `A-101` parses under NCS and `A-25-M-Doors` under BS 1192 while both are hyphenated — so parsing takes the standard EXPLICITLY and no `[ObjectFactory<string>]` plane exists on `SheetNumber`; a persisted number stores its standard key beside its text.
- Law: the drafting `DisciplineDesignator` is NOT `Rasm.Element` `Discipline` (an analysis vocabulary) — the `[NOT]` line both pages carry; `electrical` and `fire` are tokens in both and never one row.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Validation` applicative, `Range`), System.Buffers (`SearchValues`, `MemoryExtensions.LastIndexOfAnyExcept`), Rasm.Domain.
- Growth: a naming standard is one row carrying its field sequence, delimiter, fused seats, case rule, `Admit`, and `Parse` — the render DERIVES from those columns, so no row carries a format body; a discipline, sheet type, container type, or role is one row.
- Boundary: AppUi `TitleBlock.DrawingNumber` (free string) and `DisciplineKey` (free key), Rhino `NumberRule(string Template, …)` and its `%pagenumber%` expansion, and the export-file stem all take `SheetNumber`; Rhino's `%name%` stamp token grammar (`publish.md:157-165`) is the host lowering that renders `TitleField` reads and stays.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamingField {
    public static readonly NamingField Discipline = new(key: "discipline");
    public static readonly NamingField SheetType = new(key: "sheet-type");
    public static readonly NamingField Sequence = new(key: "sequence");
    public static readonly NamingField Project = new(key: "project");
    public static readonly NamingField Originator = new(key: "originator");
    public static readonly NamingField Volume = new(key: "volume");
    public static readonly NamingField Level = new(key: "level");
    public static readonly NamingField Type = new(key: "type");
    public static readonly NamingField Role = new(key: "role");
    public static readonly NamingField Number = new(key: "number");
    public static readonly NamingField Description = new(key: "description");
    public static readonly NamingField Prefix = new(key: "prefix");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DisciplineDesignator {
    public static readonly DisciplineDesignator General = new(key: "G", title: "General");
    public static readonly DisciplineDesignator Hazardous = new(key: "H", title: "Hazardous Materials");
    public static readonly DisciplineDesignator Survey = new(key: "V", title: "Survey / Mapping");
    public static readonly DisciplineDesignator Geotechnical = new(key: "B", title: "Geotechnical");
    public static readonly DisciplineDesignator Civil = new(key: "C", title: "Civil");
    public static readonly DisciplineDesignator Landscape = new(key: "L", title: "Landscape");
    public static readonly DisciplineDesignator Structural = new(key: "S", title: "Structural");
    public static readonly DisciplineDesignator Architectural = new(key: "A", title: "Architectural");
    public static readonly DisciplineDesignator Interiors = new(key: "I", title: "Interiors");
    public static readonly DisciplineDesignator Equipment = new(key: "Q", title: "Equipment");
    public static readonly DisciplineDesignator FireProtection = new(key: "F", title: "Fire Protection");
    public static readonly DisciplineDesignator Plumbing = new(key: "P", title: "Plumbing");
    public static readonly DisciplineDesignator Process = new(key: "D", title: "Process");
    public static readonly DisciplineDesignator Mechanical = new(key: "M", title: "Mechanical");
    public static readonly DisciplineDesignator Electrical = new(key: "E", title: "Electrical");
    public static readonly DisciplineDesignator DistributedEnergy = new(key: "W", title: "Distributed Energy");
    public static readonly DisciplineDesignator Telecommunications = new(key: "T", title: "Telecommunications");
    public static readonly DisciplineDesignator Resource = new(key: "R", title: "Resource");
    public static readonly DisciplineDesignator Other = new(key: "X", title: "Other Disciplines");
    public static readonly DisciplineDesignator Contractor = new(key: "Z", title: "Contractor / Shop Drawings");
    public string Title { get; }
}

[SmartEnum<int>]
public sealed partial class SheetType {
    public static readonly SheetType General = new(key: 0, title: "General");
    public static readonly SheetType Plans = new(key: 1, title: "Plans");
    public static readonly SheetType Elevations = new(key: 2, title: "Elevations");
    public static readonly SheetType Sections = new(key: 3, title: "Sections");
    public static readonly SheetType LargeScale = new(key: 4, title: "Large-Scale Views");
    public static readonly SheetType Details = new(key: 5, title: "Details");
    public static readonly SheetType Schedules = new(key: 6, title: "Schedules and Diagrams");
    public static readonly SheetType UserSeven = new(key: 7, title: "User Defined");
    public static readonly SheetType UserEight = new(key: 8, title: "User Defined");
    public static readonly SheetType ThreeDimensional = new(key: 9, title: "3D Representations");
    public string Title { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContainerType {
    public static readonly ContainerType Animation = new(key: "AF", title: "Animation file");
    public static readonly ContainerType Combined = new(key: "CM", title: "Combined model");
    public static readonly ContainerType Correspondence = new(key: "CO", title: "Correspondence");
    public static readonly ContainerType CostPlan = new(key: "CP", title: "Cost plan");
    public static readonly ContainerType ClashReport = new(key: "CR", title: "Clash report");
    public static readonly ContainerType Database = new(key: "DB", title: "Database");
    public static readonly ContainerType Drawing = new(key: "DR", title: "Drawing");
    public static readonly ContainerType FileNote = new(key: "FN", title: "File note");
    public static readonly ContainerType HealthSafety = new(key: "HS", title: "Health and safety");
    public static readonly ContainerType Exchange = new(key: "IE", title: "Information exchange file");
    public static readonly ContainerType ModelTwo = new(key: "M2", title: "2D model");
    public static readonly ContainerType ModelThree = new(key: "M3", title: "3D model");
    public static readonly ContainerType Minutes = new(key: "MI", title: "Minutes and action notes");
    public static readonly ContainerType Rendition = new(key: "MR", title: "Model rendition file");
    public static readonly ContainerType Presentation = new(key: "PP", title: "Presentation");
    public static readonly ContainerType Programme = new(key: "PR", title: "Programme");
    public static readonly ContainerType RoomData = new(key: "RD", title: "Room data sheet");
    public static readonly ContainerType Request = new(key: "RI", title: "Request for information");
    public static readonly ContainerType Report = new(key: "RP", title: "Report");
    public static readonly ContainerType Accommodation = new(key: "SA", title: "Schedule of accommodation");
    public static readonly ContainerType Schedule = new(key: "SH", title: "Schedule");
    public static readonly ContainerType Snagging = new(key: "SN", title: "Snagging list");
    public static readonly ContainerType Specification = new(key: "SP", title: "Specification");
    public static readonly ContainerType Survey = new(key: "SU", title: "Survey");
    public static readonly ContainerType Visualisation = new(key: "VS", title: "Visualisation");
    public string Title { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContainerRole {
    public static readonly ContainerRole Architect = new(key: "A", title: "Architect");
    public static readonly ContainerRole BuildingSurveyor = new(key: "B", title: "Building surveyor");
    public static readonly ContainerRole Civil = new(key: "C", title: "Civil engineer");
    public static readonly ContainerRole Drainage = new(key: "D", title: "Drainage / highways engineer");
    public static readonly ContainerRole Electrical = new(key: "E", title: "Electrical engineer");
    public static readonly ContainerRole Facilities = new(key: "F", title: "Facilities manager");
    public static readonly ContainerRole LandSurveyor = new(key: "G", title: "Geographical and land surveyor");
    public static readonly ContainerRole Ventilation = new(key: "H", title: "Heating and ventilation designer");
    public static readonly ContainerRole Interiors = new(key: "I", title: "Interior designer");
    public static readonly ContainerRole Client = new(key: "K", title: "Client");
    public static readonly ContainerRole Landscape = new(key: "L", title: "Landscape architect");
    public static readonly ContainerRole Mechanical = new(key: "M", title: "Mechanical engineer");
    public static readonly ContainerRole PublicHealth = new(key: "P", title: "Public health engineer");
    public static readonly ContainerRole QuantitySurveyor = new(key: "Q", title: "Quantity surveyor");
    public static readonly ContainerRole Structural = new(key: "S", title: "Structural engineer");
    public static readonly ContainerRole Planner = new(key: "T", title: "Town and country planner");
    public static readonly ContainerRole Contractor = new(key: "W", title: "Contractor");
    public static readonly ContainerRole Subcontractor = new(key: "X", title: "Subcontractor");
    public static readonly ContainerRole Specialist = new(key: "Y", title: "Specialist designer");
    public static readonly ContainerRole General = new(key: "Z", title: "General (non-disciplinary)");
    public string Title { get; }
}

internal delegate Validation<Error, Seq<(NamingField Field, string Value)>> IdentityAdmit(Seq<(NamingField Field, string Value)> fields, Op key);
internal delegate Option<Seq<(NamingField Field, string Value)>> IdentityParse(string text);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamingStandard {
    public static readonly NamingStandard Ncs = new(key: "ncs", body: PublishingBody.Ncs, delimiter: "-", fused: Seq(1),
        sequence: Seq(NamingField.Discipline, NamingField.SheetType, NamingField.Sequence),
        caseRule: static text => text.ToUpperInvariant(),
        admit: static (fields, key) => (
                Field(fields, NamingField.Discipline, static v => DisciplineDesignator.TryGet(v, out _), key, "an NCS discipline designator"),
                Field(fields, NamingField.SheetType, static v => v.Length == 1 && char.IsAsciiDigit(v[0]), key, "one sheet-type digit"),
                Field(fields, NamingField.Sequence, static v => v.Length == 2 && v.All(char.IsAsciiDigit), key, "a two-digit sequence"))
            .Apply(static (a, b, c) => Seq(a, b, c)).As(),
        parse: static text => text.Split('-') is [var d, var rest] && rest.Length == 3
            ? Some(Seq((NamingField.Discipline, d), (NamingField.SheetType, rest[..1]), (NamingField.Sequence, rest[1..])))
            : None);
    public static readonly NamingStandard Iso19650 = new(key: "iso-19650", body: PublishingBody.Bsi, delimiter: "-", fused: Seq<int>(),
        sequence: Seq(NamingField.Project, NamingField.Originator, NamingField.Volume, NamingField.Level, NamingField.Type, NamingField.Role, NamingField.Number),
        caseRule: static text => text.ToUpperInvariant(),
        admit: static (fields, key) => (
                Field(fields, NamingField.Project, static v => v.Length is >= 2 and <= 6, key, "a 2-6 character project code"),
                Field(fields, NamingField.Originator, static v => v.Length is >= 3 and <= 6, key, "a 3-6 character originator code"),
                Field(fields, NamingField.Volume, static v => v.Length is >= 1 and <= 3, key, "a 1-3 character volume/system code"),
                Field(fields, NamingField.Level, static v => v.Length == 2, key, "a two-character level/location code"),
                Field(fields, NamingField.Type, static v => ContainerType.TryGet(v, out _), key, "a UK annex information-type code (DR, M3, SP…)"),
                Field(fields, NamingField.Role, static v => ContainerRole.TryGet(v, out _), key, "a UK annex role code (A, S, M…)"),
                Field(fields, NamingField.Number, static v => v.Length is >= 4 and <= 6 && v.All(char.IsAsciiDigit), key, "a 4-6 digit number"))
            .Apply(static (a, b, c, d, e, f, g) => Seq(a, b, c, d, e, f, g)).As(),
        parse: static text => text.Split('-') is { Length: 7 } parts
            ? Some(toSeq(Iso19650!.Sequence).Zip(toSeq(parts)).Map(static pair => (pair.Item1, pair.Item2)))
            : None);
    public static readonly NamingStandard Simple = new(key: "simple", body: PublishingBody.Iso, delimiter: string.Empty, fused: Seq<int>(),
        sequence: Seq(NamingField.Prefix, NamingField.Sequence),
        caseRule: static text => text,
        admit: static (fields, key) => (
                Field(fields, NamingField.Prefix, static v => v.Length >= 1, key, "a non-empty prefix"),
                Field(fields, NamingField.Sequence, static v => v.Length >= 1 && v.All(char.IsAsciiDigit), key, "a digit sequence"))
            .Apply(static (a, b) => Seq(a, b)).As(),
        parse: static text => text.AsSpan().LastIndexOfAnyExcept(Digits) is int last && last >= 0 && last < text.Length - 1
            ? Some(Seq((NamingField.Prefix, text[..(last + 1)]), (NamingField.Sequence, text[(last + 1)..])))
            : None);

    public PublishingBody Body { get; }
    public string Delimiter { get; }
    public Seq<int> Fused { get; }
    public Seq<NamingField> Sequence { get; }
    [UseDelegateFromConstructor] internal partial string CaseRule(string text);
    internal IdentityAdmit Admit { get; }
    internal IdentityParse Parse { get; }

    private static readonly SearchValues<char> Digits = SearchValues.Create("0123456789");

    internal string Render(Seq<(NamingField Field, string Value)> fields) =>
        fields.Map(static (pair, index) => (pair.Value, Index: index)).Fold(string.Empty, (held, pair) =>
            string.Concat(held, pair.Index == 0 || Fused.Contains(pair.Index - 1) ? string.Empty : Delimiter, pair.Value));

    private static Validation<Error, (NamingField Field, string Value)> Field(Seq<(NamingField Field, string Value)> fields, NamingField field, Func<string, bool> admits, Op key, string requirement) =>
        fields.Find(pair => pair.Field.Equals(field)).Match(
            None: () => Validation<Error, (NamingField, string)>.Fail(new KernelFault.InvalidValue(Label: field.Key, Requirement: "a value this standard's sequence requires", Key: Some(key))),
            Some: pair => admits(pair.Value)
                ? Validation<Error, (NamingField, string)>.Success(pair)
                : Validation<Error, (NamingField, string)>.Fail(new KernelFault.InvalidValue(Label: field.Key, Requirement: requirement, Key: Some(key))));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SheetNumber {
    public NamingStandard Standard { get; }
    public Seq<(NamingField Field, string Value)> Fields { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref NamingStandard standard, ref Seq<(NamingField Field, string Value)> fields) =>
        validationError = fields.Map(static pair => pair.Field).Equals(standard.Sequence) ? null : new ValidationError(message: "SheetNumber fields must follow the standard's own sequence.");

    public static Fin<SheetNumber> Of(NamingStandard standard, Seq<(NamingField Field, string Value)> fields, Op? key = null) {
        Op op = key.OrDefault();
        Seq<(NamingField Field, string Value)> cased = fields.Map(pair => (pair.Field, standard.CaseRule(pair.Value)));
        return standard.Admit(cased, op).ToFin().Bind(admitted => op.AcceptValidated<SheetNumber>(Validate(standard, admitted, out SheetNumber? number), number));
    }
    public static Fin<SheetNumber> Parse(NamingStandard standard, string text, Op? key = null) =>
        standard.Parse(text).ToFin(key.OrDefault().InvalidInput()).Bind(fields => Of(standard: standard, fields: fields, key: key));
    public static Fin<SheetNumber> Ncs(DisciplineDesignator discipline, SheetType type, int sequence, Op? key = null) =>
        Of(NamingStandard.Ncs, Seq((NamingField.Discipline, discipline.Key), (NamingField.SheetType, type.Key.ToString(CultureInfo.InvariantCulture)), (NamingField.Sequence, sequence.ToString("00", CultureInfo.InvariantCulture))), key);
    public static Fin<SheetNumber> Iso19650(string project, string originator, string volume, string level, ContainerType type, ContainerRole role, int number, Op? key = null) =>
        Of(NamingStandard.Iso19650, Seq((NamingField.Project, project), (NamingField.Originator, originator), (NamingField.Volume, volume), (NamingField.Level, level), (NamingField.Type, type.Key), (NamingField.Role, role.Key), (NamingField.Number, number.ToString("0000", CultureInfo.InvariantCulture))), key);
    public string Text => Standard.Render(Fields);
}
```

## [07]-[LAYER]

- Owner: `LayerStandard` `[SmartEnum<string>]` — the layer-naming grammars as rows, each carrying its field sequence, delimiter, case rule, `Parse`, and `Format`: `Ncs` (US NCS / AIA CAD Layer Guidelines: `{Discipline}-{Major}[-{Minor}[-{Minor}]][-{Status}]`, hyphen, uppercase, discipline 1-2 letters, major and minor four letters, status one letter), `Iso13567` (ISO 13567-2: fixed-position fields — agent 2, element 6, presentation 2, status 1, sector 4, phase 1, projection 1, scale 1, work package 2, user-definable — hyphens as fillers, no delimiter), `Bs1192` (BS 1192:2007 §… `{Role}-{Classification}-{Presentation}-{Description}`, hyphen), `House` (the branch's own `draft-{style}[-part-{n}]` scheme absorbed as ONE row); `LayerField` `[SmartEnum<string>]` — the field vocabulary; `LayerName` `[ComplexValueObject]` — the admitted name, standard + ordered field values, `Text` rendered by the standard; `HostLayerScheme` `[SmartEnum<string>]` — the host projections: `RhinoPath` (the standard's fields become `::`-nested segments — discipline over major over minor — never a flat name with a foreign delimiter), `AutoCadFlat` (the standard's own formatted text, since DWG layer names ARE the standard's grammar), `IfcPresentation` (`IfcPresentationLayerAssignment.Name` = the formatted text), `Pdf` (an optional-content-group name).
- Entry: `LayerName.Of(standard, fields, key)` (Validation over every field); `LayerName.Parse(standard, text, key)`; `name.Text`; `HostLayerScheme.X.Project(name)`; `LayerStandard.Ncs.Status` rows (N new, E existing, D demolish, F future, T temporary, M to be moved, X abandoned).
- Law: parsing takes the standard EXPLICITLY (a hyphenated raw string is ambiguous across NCS and BS 1192 — the same reason `SheetNumber` carries none), and the Rhino `::` path is a PROJECTION of the fields, never the storage form; a consumer that stored `Parent::Child` re-parses through `RhinoPath.Unproject`.
- Law: the solution's `draft-{style}` scheme is a `LayerStandard` row and never a string interpolation at a consumer; its part ordinal is a FIELD (`Part`) rather than a `-part-{n}` suffix an interpolation cannot parse back.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Validation`), Rasm.Domain.
- Growth: a naming standard is one row naming its sequence and, where fixed-position, its slot widths; a field is one row; a host projection is one row.
- Boundary: the four unrelated senses of "layer" — CAD layer (this owner), per-element visibility override (AppUi `VisibilityOverride`), analysis result layer (AppUi `ResultLayer`), IFC material ply (`Rasm.Element` `MaterialLayer`) — are NOT one concept and this owner names none of the other three; Rhino `LayerName`/`LayerPath` (`Document/layers.md:38-93`), Rhino `DwgWriteCase.FullLayerPath`/`ColorMethod` decisions, AppUi `Role(style)`/`-part-{p}`/`"draft-annotation"` all compose this owner and delete their grammar; the layer-state facet store and per-view overrides stay at Rhino.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Thinktecture;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerField {
    public static readonly LayerField Discipline = new(key: "discipline");
    public static readonly LayerField Major = new(key: "major");
    public static readonly LayerField Minor = new(key: "minor");
    public static readonly LayerField MinorTwo = new(key: "minor-2");
    public static readonly LayerField Status = new(key: "status");
    public static readonly LayerField Agent = new(key: "agent");
    public static readonly LayerField Element = new(key: "element");
    public static readonly LayerField Presentation = new(key: "presentation");
    public static readonly LayerField Sector = new(key: "sector");
    public static readonly LayerField Phase = new(key: "phase");
    public static readonly LayerField Projection = new(key: "projection");
    public static readonly LayerField Scale = new(key: "scale");
    public static readonly LayerField WorkPackage = new(key: "work-package");
    public static readonly LayerField User = new(key: "user");
    public static readonly LayerField Role = new(key: "role");
    public static readonly LayerField Classification = new(key: "classification");
    public static readonly LayerField Description = new(key: "description");
    public static readonly LayerField Prefix = new(key: "prefix");
    public static readonly LayerField Style = new(key: "style");
    public static readonly LayerField Part = new(key: "part");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerStatus {
    public static readonly LayerStatus New = new(key: "N", title: "New work");
    public static readonly LayerStatus Existing = new(key: "E", title: "Existing to remain");
    public static readonly LayerStatus Demolish = new(key: "D", title: "Existing to demolish");
    public static readonly LayerStatus Future = new(key: "F", title: "Future work");
    public static readonly LayerStatus Temporary = new(key: "T", title: "Temporary work");
    public static readonly LayerStatus Move = new(key: "M", title: "Items to be moved");
    public static readonly LayerStatus Abandoned = new(key: "X", title: "Abandoned");
    public string Title { get; }
}

internal delegate Validation<Error, Seq<(LayerField Field, string Value)>> LayerAdmit(Seq<(LayerField Field, string Value)> fields, Op key);
internal delegate Option<Seq<(LayerField Field, string Value)>> LayerParse(string text);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerStandard {
    public static readonly LayerStandard Ncs = new(key: "ncs", body: PublishingBody.Ncs, delimiter: "-", caseRule: static text => text.ToUpperInvariant(),
        sequence: Seq(LayerField.Discipline, LayerField.Major, LayerField.Minor, LayerField.MinorTwo, LayerField.Status),
        required: Seq(LayerField.Discipline, LayerField.Major), slots: Seq<(LayerField, int)>(),
        admit: static (fields, key) => Fields(fields, key,
            (LayerField.Discipline, static v => v.Length is 1 or 2 && DisciplineDesignator.TryGet(v[..1], out _), "an NCS discipline designator with an optional level-2 letter"),
            (LayerField.Major, static v => v.Length == 4 && v.All(char.IsAsciiLetterOrDigit), "a four-character major group"),
            (LayerField.Minor, static v => v.Length == 4 && v.All(char.IsAsciiLetterOrDigit), "a four-character minor group"),
            (LayerField.MinorTwo, static v => v.Length == 4 && v.All(char.IsAsciiLetterOrDigit), "a four-character second minor group"),
            (LayerField.Status, static v => LayerStatus.TryGet(v, out _), "an NCS status letter")),
        parse: static text => text.Split('-') switch {
            [var d, var major, .. var rest] when rest.Length <= 3 => Some(Seq((LayerField.Discipline, d), (LayerField.Major, major))
                + toSeq(rest).Zip(Seq(LayerField.Minor, LayerField.MinorTwo, LayerField.Status)).Map(static pair =>
                    (pair.Item1.Length == 1 ? LayerField.Status : pair.Item2, pair.Item1))),
            _ => None,
        });
    public static readonly LayerStandard Iso13567 = new(key: "iso-13567", body: PublishingBody.Iso, delimiter: string.Empty, caseRule: static text => text.ToUpperInvariant(),
        sequence: Seq(LayerField.Agent, LayerField.Element, LayerField.Presentation, LayerField.Status, LayerField.Sector, LayerField.Phase, LayerField.Projection, LayerField.Scale, LayerField.WorkPackage),
        required: Seq(LayerField.Agent, LayerField.Element, LayerField.Presentation),
        slots: Seq((LayerField.Agent, 2), (LayerField.Element, 6), (LayerField.Presentation, 2), (LayerField.Status, 1), (LayerField.Sector, 4),
            (LayerField.Phase, 1), (LayerField.Projection, 1), (LayerField.Scale, 1), (LayerField.WorkPackage, 2)),
        admit: static (fields, key) => Fields(fields, key,
            (LayerField.Agent, static v => v.Length == 2, "a two-character agent code"),
            (LayerField.Element, static v => v.Length == 6, "a six-character element code"),
            (LayerField.Presentation, static v => v.Length == 2, "a two-character presentation code"),
            (LayerField.Status, static v => v.Length == 1, "one status character"),
            (LayerField.Sector, static v => v.Length == 4, "a four-character sector code"),
            (LayerField.Phase, static v => v.Length == 1, "one phase character"),
            (LayerField.Projection, static v => v.Length == 1, "one projection character"),
            (LayerField.Scale, static v => v.Length == 1, "one scale character"),
            (LayerField.WorkPackage, static v => v.Length == 2, "a two-character work-package code")),
        parse: static text => Iso13567!.Positional(text: text));
    public static readonly LayerStandard Bs1192 = new(key: "bs-1192", body: PublishingBody.Bsi, delimiter: "-", caseRule: static text => text,
        sequence: Seq(LayerField.Role, LayerField.Classification, LayerField.Presentation, LayerField.Description),
        required: Seq(LayerField.Role, LayerField.Classification, LayerField.Presentation), slots: Seq<(LayerField, int)>(),
        admit: static (fields, key) => Fields(fields, key,
            (LayerField.Role, static v => v.Length is 1 or 2, "a one- or two-character role code"),
            (LayerField.Classification, static v => v.Length >= 1, "a classification code (Uniclass)"),
            (LayerField.Presentation, static v => v.Length is 1 or 2, "a one- or two-character presentation code"),
            (LayerField.Description, static v => v.Length >= 1, "a description")),
        parse: static text => text.Split('-', 4) is { Length: >= 3 } parts
            ? Some(toSeq(Seq(LayerField.Role, LayerField.Classification, LayerField.Presentation, LayerField.Description).Take(parts.Length)).Zip(toSeq(parts)).Map(static pair => (pair.Item1, pair.Item2)))
            : None);
    public static readonly LayerStandard House = new(key: "house", body: PublishingBody.Iso, delimiter: "-", caseRule: static text => text.ToLowerInvariant(),
        sequence: Seq(LayerField.Prefix, LayerField.Style, LayerField.Part),
        required: Seq(LayerField.Prefix, LayerField.Style), slots: Seq<(LayerField, int)>(),
        admit: static (fields, key) => Fields(fields, key,
            (LayerField.Prefix, static v => v.Length >= 1 && !v.Contains('-'), "a hyphen-free prefix"),
            (LayerField.Style, static v => v.Length >= 1 && !v.Contains('-'), "a hyphen-free style key"),
            (LayerField.Part, static v => v.All(char.IsAsciiDigit) && v.Length >= 1, "a part ordinal")),
        parse: static text => text.Split('-') switch {
            [var prefix, var style] => Some(Seq((LayerField.Prefix, prefix), (LayerField.Style, style))),
            [var prefix, var style, var part] when part.All(char.IsAsciiDigit) => Some(Seq((LayerField.Prefix, prefix), (LayerField.Style, style), (LayerField.Part, part))),
            _ => None,
        });

    public PublishingBody Body { get; }
    public string Delimiter { get; }
    public Seq<LayerField> Sequence { get; }
    public Seq<LayerField> Required { get; }
    public Seq<LayerField> Optional => Sequence.Filter(field => !Required.Contains(field));
    public Seq<(LayerField Field, int Width)> Slots { get; }
    [UseDelegateFromConstructor] internal partial string CaseRule(string text);
    internal LayerAdmit Admit { get; }
    internal LayerParse Parse { get; }

    private static Validation<Error, Seq<(LayerField Field, string Value)>> Fields(Seq<(LayerField Field, string Value)> fields, Op key, params (LayerField Field, Func<string, bool> Admits, string Requirement)[] rules) =>
        fields.Traverse(pair => toSeq(rules).Find(rule => rule.Field.Equals(pair.Field)).Match(
            Some: rule => rule.Admits(pair.Value)
                ? Validation<Error, (LayerField, string)>.Success(pair)
                : Validation<Error, (LayerField, string)>.Fail(new KernelFault.InvalidValue(Label: pair.Field.Key, Requirement: rule.Requirement, Key: Some(key))),
            None: () => Validation<Error, (LayerField, string)>.Fail(new KernelFault.InvalidValue(Label: pair.Field.Key, Requirement: "a field this standard sequences", Key: Some(key))))).As();
    internal string Render(Seq<(LayerField Field, string Value)> fields) =>
        Slots.IsEmpty
            ? string.Join(Delimiter, fields.Map(static pair => pair.Value))
            : Slots.Fold(string.Empty, (held, slot) => string.Concat(held,
                fields.Find(pair => pair.Field.Equals(slot.Field)).Map(pair => pair.Value.PadRight(slot.Width, '-')).IfNone(new string('-', slot.Width))));

    private Option<Seq<(LayerField Field, string Value)>> Positional(string text) =>
        Slots.Fold(Some((At: 0, Rows: Seq<(LayerField, string)>())), (held, slot) => held.Bind(state =>
                state.At >= text.Length ? Some(state)
                : state.At + slot.Width <= text.Length ? Some((state.At + slot.Width, state.Rows.Add((slot.Field, text.Substring(state.At, slot.Width)))))
                : Option<(int At, Seq<(LayerField, string)> Rows)>.None))
            .Map(static state => state.Rows.Filter(static row => row.Item2.Trim('-').Length > 0));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class LayerName {
    public LayerStandard Standard { get; }
    public Seq<(LayerField Field, string Value)> Fields { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref LayerStandard standard, ref Seq<(LayerField Field, string Value)> fields) {
        Seq<(LayerField Field, string Value)> ordered = standard.Sequence.Choose(field => fields.Find(pair => pair.Field.Equals(field)));
        validationError = ordered.Count == fields.Count && standard.Required.ForAll(field => ordered.Exists(pair => pair.Field.Equals(field)))
            ? null
            : new ValidationError(message: "LayerName requires every required field of its standard, in its sequence, and no field outside it.");
        fields = ordered;
    }
    public static Fin<LayerName> Of(LayerStandard standard, Seq<(LayerField Field, string Value)> fields, Op? key = null) {
        Op op = key.OrDefault();
        Seq<(LayerField Field, string Value)> cased = fields.Map(pair => (pair.Field, standard.CaseRule(pair.Value)));
        return standard.Admit(cased, op).ToFin().Bind(admitted => op.AcceptValidated<LayerName>(Validate(standard, admitted, out LayerName? name), name));
    }
    public static Fin<LayerName> Parse(LayerStandard standard, string text, Op? key = null) =>
        standard.Parse(text).ToFin(key.OrDefault().InvalidInput()).Bind(fields => Of(standard: standard, fields: fields, key: key));
    public string Text => Standard.Render(Fields);
    public Option<string> Read(LayerField field) => Fields.Find(pair => pair.Field.Equals(field)).Map(static pair => pair.Value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostLayerScheme {
    public static readonly HostLayerScheme RhinoPath = new(key: "rhino-path", project: static name => name.Fields.Map(static pair => pair.Value), separator: Some("::"));
    public static readonly HostLayerScheme AutoCadFlat = new(key: "autocad-flat", project: static name => Seq(name.Text), separator: None);
    public static readonly HostLayerScheme IfcPresentation = new(key: "ifc-presentation", project: static name => Seq(name.Text), separator: None);
    public static readonly HostLayerScheme Pdf = new(key: "pdf-ocg", project: static name => Seq(name.Text), separator: None);

    [UseDelegateFromConstructor] public partial Seq<string> Project(LayerName name);
    public Option<string> Separator { get; }
    public string Path(LayerName name) => Separator.Match(Some: mark => string.Join(mark, Project(name)), None: () => string.Concat(Project(name)));
    public Fin<LayerName> Unproject(LayerStandard standard, string path, Op? key = null) =>
        LayerName.Parse(standard: standard,
            text: Separator.Match(Some: mark => string.Join(standard.Delimiter, path.Split(mark, StringSplitOptions.None)), None: () => path), key: key);
}
```

## [08]-[LINEWORK]

- Owner: `PenCode` `[SmartEnum<string>]` — the ISO 9175-1 Table 1 nib vocabulary, one row per nib colour carrying the ink a legend or CTB seed plots and the nib diameter it draws; `LineWidth` `[SmartEnum<string>]` — the ISO 128-2 / ISO 128-24 width ladder (0.13, 0.18, 0.25, 0.35, 0.5, 0.7, 1.0, 1.4, 2.0 mm — a √2 progression the standard publishes as R20-rounded values, so the rows are DATA with the law named and the `PenCode` row as a column); `LineGroup` `[SmartEnum<string>]` — the ISO 128-24 line groups (a group is the wide/narrow pair 2:1 apart: 0.25 (0.13/0.25), 0.35, 0.5, 0.7, 1.0, 1.4, 2.0), selected by sheet extent per standard through `LineGroup.For(size)`; `LineType` `[SmartEnum<string>]` — the ISO 128-2 basic line types 01-15 with their element proportions in MULTIPLES OF THE LINE WIDTH d (dash 12d, gap 3d, dot ≤ 0.5d, long dash 24d, short dash 6d — ISO 128-2 Table 3), so `Rhythm(width)` DERIVES the dash-and-gap pattern for any width and no dash literal exists; `PlotPosture` `[SmartEnum<string>]` — colour, grayscale, monochrome plot postures (the AppUi `PlotColor` shape); `AciIndex` `[ValueObject<int>]`, `StyleName` `[ValueObject<string>]`, and `PlotStyleKey` `[Union]` — the two plot-style key regimes as ONE closed family; `PlotStyle` `[ComplexValueObject]` — one pen row (key, width, plot colour, screening) and `PlotStyleTable` `[Union]` — colour-dependent (`Ctb`, keyed by `AciIndex`) or named (`Stb`, keyed by `StyleName`), the two AutoCAD plot-style regimes every CAD host reads.
- Entry: `LineWidth.For(width, key)` snaps a free width to the ladder and REFUSES a non-positive one; `LineWidth.For(pen)` reads the width a nib draws; `LineGroup.For(size, key)`; `type.Rhythm(width)` → the drawn-and-gap pairs; `PlotStyleKey.Of(index)` / `Of(name)`; `PlotStyle.Of(key, width, screening, colour)`; `PlotStyleTable.Style(key, op)`; `PlotPosture.Ink(colour)`.
- Law: a bare millimetre plot weight — Rhino's eight authorities and AppUi's `EdgeStyle.weight` floats — reads a `LineWidth` row; `-1.0` "by parent" is a CASE at the host, never a sentinel on this ladder, so `For` REFUSES a non-positive width instead of snapping it to a hairline.
- Law: the plot-style key carries its own REGIME — an `AciIndex` reads a CTB and a `StyleName` an STB — so a table asked in the other regime refuses typed where an option-shaped answer would read exactly like a missing entry; the ISO 9175-1 ink of the width's own pen is the style's colour wherever a table states none.
- Law: `LineType` rhythms derive from the width — the AppUi `3 mm dash / 2 mm gap` shared by hidden and centre lines is the defect this closes: ISO 128-2 type 02 (dashed) and type 04 (long-dashed dotted) emit distinguishable patterns at every width; elements ride as drawn-and-gap PAIRS, so no sign on a `double` carries the drawn-versus-gap discriminant.
- Packages: UnitsNet (`Length`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Option`, `HashMap`), System.Buffers (`SearchValues`), System.Collections.Frozen (`FrozenDictionary`), `Numerics/atoms` (`PerceptualColor.OfRgb`, `UnitInterval`, `Band.Count`).
- Growth: a nib is one `PenCode` row; a width is one row naming its pen; a line type is one row of proportions; a line-group ladder is one rung roster per standard; a plot-style key regime is one union case.
- Boundary: the AutoCAD Colour Index, plot-style names, and screening percentages are plot-style data here, typed as `AciIndex`, `StyleName`, and `UnitInterval`; the CAD host's participation switch (`PrintWidthPolicy`), by-layer/by-object inheritance rosters, and per-viewport overrides stay at Rhino; AppUi `EdgeStyle` reads `LineWidth` + `LineType` rows and `PlotColor` becomes `PlotPosture`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PenCode {
    public static readonly PenCode Violet = new(key: "violet", ink: Rgb(148, 0, 211));
    public static readonly PenCode Red = new(key: "red", ink: Rgb(255, 0, 0));
    public static readonly PenCode White = new(key: "white", ink: Rgb(255, 255, 255));
    public static readonly PenCode Yellow = new(key: "yellow", ink: Rgb(255, 255, 0));
    public static readonly PenCode Brown = new(key: "brown", ink: Rgb(165, 42, 42));
    public static readonly PenCode Blue = new(key: "blue", ink: Rgb(0, 0, 255));
    public static readonly PenCode Orange = new(key: "orange", ink: Rgb(255, 165, 0));
    public static readonly PenCode Green = new(key: "green", ink: Rgb(0, 128, 0));
    public static readonly PenCode Grey = new(key: "grey", ink: Rgb(128, 128, 128));
    public PerceptualColor Ink { get; }
    public Length NibDiameter => LineWidth.For(pen: this).Width;
    private static PerceptualColor Rgb(byte red, byte green, byte blue) => PerceptualColor.OfRgb(red: red, green: green, blue: blue).ThrowIfFail();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineWidth {
    public static readonly LineWidth W013 = new(key: "0.13", width: Length.FromMillimeters(0.13), pen: PenCode.Violet);
    public static readonly LineWidth W018 = new(key: "0.18", width: Length.FromMillimeters(0.18), pen: PenCode.Red);
    public static readonly LineWidth W025 = new(key: "0.25", width: Length.FromMillimeters(0.25), pen: PenCode.White);
    public static readonly LineWidth W035 = new(key: "0.35", width: Length.FromMillimeters(0.35), pen: PenCode.Yellow);
    public static readonly LineWidth W050 = new(key: "0.5", width: Length.FromMillimeters(0.5), pen: PenCode.Brown);
    public static readonly LineWidth W070 = new(key: "0.7", width: Length.FromMillimeters(0.7), pen: PenCode.Blue);
    public static readonly LineWidth W100 = new(key: "1.0", width: Length.FromMillimeters(1.0), pen: PenCode.Orange);
    public static readonly LineWidth W140 = new(key: "1.4", width: Length.FromMillimeters(1.4), pen: PenCode.Green);
    public static readonly LineWidth W200 = new(key: "2.0", width: Length.FromMillimeters(2.0), pen: PenCode.Grey);
    public Length Width { get; }
    public PenCode Pen { get; }

    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Width.Millimeters)).ToArray());
    public static Fin<LineWidth> For(Length width, Op? key = null) =>
        width.Millimeters > 0.0 && double.IsFinite(width.Millimeters)
            ? Fin.Succ(Items[RungLadder.NearestIndex(logs: LogLadder.Value, magnitude: width.Millimeters)])
            : Fin.Fail<LineWidth>(new KernelFault.OutOfRange(Label: nameof(LineWidth), Scalar: width.Millimeters, Requirement: "a positive finite paper width", Key: Some(key.OrDefault())));
    public static LineWidth For(PenCode pen) => ByPen.Value[pen];
    private static readonly Lazy<FrozenDictionary<PenCode, LineWidth>> ByPen =
        new(static () => Items.ToFrozenDictionary(static row => row.Pen, static row => row));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineGroup {
    public static readonly LineGroup G025 = new(key: "0.25", wide: LineWidth.W025, narrow: LineWidth.W013);
    public static readonly LineGroup G035 = new(key: "0.35", wide: LineWidth.W035, narrow: LineWidth.W018);
    public static readonly LineGroup G050 = new(key: "0.5", wide: LineWidth.W050, narrow: LineWidth.W025);
    public static readonly LineGroup G070 = new(key: "0.7", wide: LineWidth.W070, narrow: LineWidth.W035);
    public static readonly LineGroup G100 = new(key: "1.0", wide: LineWidth.W100, narrow: LineWidth.W050);
    public static readonly LineGroup G140 = new(key: "1.4", wide: LineWidth.W140, narrow: LineWidth.W070);
    public static readonly LineGroup G200 = new(key: "2.0", wide: LineWidth.W200, narrow: LineWidth.W100);
    public LineWidth Wide { get; }
    public LineWidth Narrow { get; }

    private static readonly Lazy<FrozenDictionary<SheetStandard, Seq<(Length Ceiling, LineGroup Group)>>> Ladders =
        new(static () => SheetStandard.Index(
            rows: Seq(
                (Standard: SheetStandard.Iso, Rungs: Seq((Ceiling: SheetSeries.IsoA.WidthAt(4), Group: G025), (SheetSeries.IsoA.WidthAt(2), G035), (SheetSize.Unbounded, G050))),
                (Standard: SheetStandard.Ansi, Rungs: Seq((Ceiling: SheetSize.Unbounded, Group: G070)))),
            column: static row => row.Standard)
            .ToFrozenDictionary(static pair => pair.Key, static pair => pair.Value.Rungs));

    public static Fin<LineGroup> For(SheetSize size, Op? key = null) =>
        Ladders.Value[size.Standard].Find(rung => size.Width <= rung.Ceiling).Map(static rung => rung.Group)
            .ToFin(new KernelFault.InvalidValue(Label: nameof(LineGroup), Requirement: $"a line-group rung covering the '{size.Key}' extent", Key: Some(key.OrDefault())));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LineType {
    public static readonly LineType Continuous = new(key: "01", elements: Seq<(double, double)>());
    public static readonly LineType Dashed = new(key: "02", elements: Seq((12.0, 3.0)));
    public static readonly LineType DashedSpaced = new(key: "03", elements: Seq((12.0, 18.0)));
    public static readonly LineType LongDashedDotted = new(key: "04", elements: Seq((24.0, 3.0), (0.5, 3.0)));
    public static readonly LineType LongDashedDoubleDotted = new(key: "05", elements: Seq((24.0, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public static readonly LineType LongDashedTripleDotted = new(key: "06", elements: Seq((24.0, 3.0), (0.5, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public static readonly LineType Dotted = new(key: "07", elements: Seq((0.5, 3.0)));
    public static readonly LineType LongDashedShortDashed = new(key: "08", elements: Seq((24.0, 3.0), (6.0, 3.0)));
    public static readonly LineType LongDashedDoubleShortDashed = new(key: "09", elements: Seq((24.0, 3.0), (6.0, 3.0), (6.0, 3.0)));
    public static readonly LineType DashedDotted = new(key: "10", elements: Seq((12.0, 3.0), (0.5, 3.0)));
    public static readonly LineType DoubleDashedDotted = new(key: "11", elements: Seq((12.0, 3.0), (12.0, 3.0), (0.5, 3.0)));
    public static readonly LineType DashedDoubleDotted = new(key: "12", elements: Seq((12.0, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public static readonly LineType DoubleDashedDoubleDotted = new(key: "13", elements: Seq((12.0, 3.0), (12.0, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public static readonly LineType DashedTripleDotted = new(key: "14", elements: Seq((12.0, 3.0), (0.5, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public static readonly LineType DoubleDashedTripleDotted = new(key: "15", elements: Seq((12.0, 3.0), (12.0, 3.0), (0.5, 3.0), (0.5, 3.0), (0.5, 3.0)));
    public Seq<(double Drawn, double Gap)> Elements { get; }
    public bool IsContinuous => Elements.IsEmpty;
    public Seq<(Length Drawn, Length Gap)> Rhythm(LineWidth width) =>
        Elements.Map(pair => (Drawn: width.Width * pair.Drawn, Gap: width.Width * pair.Gap));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlotPosture {
    public static readonly PlotPosture Colour = new(key: "colour", ink: static colour => Fin.Succ(colour));
    public static readonly PlotPosture Grayscale = new(key: "grayscale",
        ink: static colour => PerceptualColor.Achromatic(lightness: colour.Lightness, alpha: colour.Alpha));
    public static readonly PlotPosture Monochrome = new(key: "monochrome", ink: static _ => Fin.Succ(Ink0));
    private static readonly PerceptualColor Ink0 = PerceptualColor.Of(lightness: 0.0, opponentA: 0.0, opponentB: 0.0).ThrowIfFail();
    [UseDelegateFromConstructor] public partial Fin<PerceptualColor> Ink(PerceptualColor colour);
}

[ValueObject<int>]
public sealed partial class AciIndex {
    private const int PaletteSize = 255;
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Band.Count.Admits(value) && value <= PaletteSize
            ? null
            : new ValidationError(message: "AciIndex admits an AutoCAD Colour Index 1-255; ByBlock and ByLayer are host inheritance cases.");
    public static Fin<AciIndex> Of(int value, Op? key = null) =>
        key.OrDefault().AcceptValidated<AciIndex>(Validate(value, out AciIndex? index), index);
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StyleName {
    private static readonly SearchValues<char> Reserved = SearchValues.Create("<>/\\\":;?*|=`,");
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = value.Length > 0 && !value.AsSpan().ContainsAny(Reserved)
            ? null
            : new ValidationError(message: "StyleName requires a non-blank name free of the AutoCAD reserved glyphs.");
    }
    public static Fin<StyleName> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<StyleName>(Validate(value, out StyleName? name), name);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotStyleKey {
    private PlotStyleKey() { }
    public sealed record Indexed : PlotStyleKey { internal Indexed(AciIndex index) => Index = index; public AciIndex Index { get; } }
    public sealed record Named : PlotStyleKey { internal Named(StyleName name) => Name = name; public StyleName Name { get; } }
    public static Fin<PlotStyleKey> Of(int index, Op? key = null) => AciIndex.Of(value: index, key: key).Map(static seat => (PlotStyleKey)new Indexed(index: seat));
    public static Fin<PlotStyleKey> Of(string name, Op? key = null) => StyleName.Of(value: name, key: key).Map(static seat => (PlotStyleKey)new Named(name: seat));
    public string Text => Switch(indexed: static row => row.Index.ToValue().ToString(CultureInfo.InvariantCulture), named: static row => row.Name.ToValue());
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class PlotStyle {
    public PlotStyleKey Key { get; }
    public LineWidth Width { get; }
    public PerceptualColor Colour { get; }
    public UnitInterval Screening { get; }
    public static Fin<PlotStyle> Of(PlotStyleKey key, LineWidth width, UnitInterval screening, Option<PerceptualColor> colour = default, Op? op = null) =>
        op.OrDefault().AcceptValidated<PlotStyle>(Validate(key, width, colour.IfNone(width.Pen.Ink), screening, out PlotStyle? style), style);
}

[Union]
public abstract partial record PlotStyleTable {
    private PlotStyleTable() { }
    public sealed record Ctb(HashMap<AciIndex, PlotStyle> ByIndex) : PlotStyleTable;
    public sealed record Stb(HashMap<StyleName, PlotStyle> ByName) : PlotStyleTable;
    public Fin<PlotStyle> Style(PlotStyleKey key, Op? op = null) {
        Op seat = op.OrDefault();
        return (this, key) switch {
            (Ctb table, PlotStyleKey.Indexed row) => table.ByIndex.Find(row.Index).ToFin(Missing(key: key, seat: seat)),
            (Stb table, PlotStyleKey.Named row) => table.ByName.Find(row.Name).ToFin(Missing(key: key, seat: seat)),
            (Ctb, PlotStyleKey.Named) or (Stb, PlotStyleKey.Indexed) =>
                Fin.Fail<PlotStyle>(new KernelFault.InvalidValue(Label: nameof(PlotStyleTable), Requirement: "a key of the table's own regime", Key: Some(seat))),
            _ => Fin.Fail<PlotStyle>(seat.InvalidInput()),
        };
    }
    private static Error Missing(PlotStyleKey key, Op seat) =>
        new KernelFault.InvalidValue(Label: nameof(PlotStyle), Requirement: $"a style seated at '{key.Text}'", Key: Some(seat));
}
```

## [09]-[LETTERING]

- Owner: `TextHeight` `[SmartEnum<string>]` — the ISO 3098-1 lettering-height ladder (1.8, 2.5, 3.5, 5, 7, 10, 14, 20 mm — the √2 series from 1.8 rounded to R20 preferred numbers, ISO 3098-1 §5), with `For(size)` folding the standard's own minimum-height rungs (ISO 3098-1 §5.2 / ASME Y14.2 §4: 2.5 mm minimum to A2, 3.5 mm above; 0.12 in on ANSI A-C, 0.16 in on D-E) and `For(Length)` snapping a free height through the shared rung ladder; `LetteringForm` `[SmartEnum<string>]` — Type A (line width h/14) and Type B (h/10), vertical or 15° italic, each carrying its own pitch and lower-case ratios (ISO 3098-1 §4, Table 1); `DraftingMetrics` — every proportion ISO 3098-1 Table 1 and the annotation standards derive from h and d, lettering and annotation being ONE member set under two subsection labels; `Terminator` `[SmartEnum<string>]` — the ISO 129-1 dimension terminators (closed/open arrowhead at 15° or 30°, oblique stroke, dot, origin circle) sized as multiples of d; `DatumRegime` `[SmartEnum<string>]` and `ZoneModifier` `[SmartEnum<string>]` — the tri-valued datum dependence each characteristic carries and the diameter and material-condition glyphs its tolerance-zone compartment takes.
- Entry: `TextHeight.For(size, key)`, `TextHeight.For(height, key)`; `form.Metrics(height)` → `DraftingMetrics` with `CentreMark(diameter)`; `Terminator.X.Size(width)`; `DatumDesignator.Of(primary, secondary, key)` / `Validate("A-B")`; `SymbolSet.For(standard)` with `Admits(characteristic)` and `Rows`; `ZoneModifier.Items` for the compartment glyphs.
- Law: a bare lettering millimetre literal reads a `TextHeight` row — the seven AppUi literals (`3d` × 5, `2.5d`, `6d`) and the Rhino `StyleField.TextHeight` schema row without a value are what this closes; the GD&T frame height is `2h` DERIVED, never `6d` beside `3d`; every proportion names the clause it transcribes or states the solution convention it is, and a proportion whose standard sizes it off another feature takes that feature as an ARGUMENT rather than dropping the axis.
- Law: datum dependence is TRI-valued — a form tolerance takes no datum, an orientation or location tolerance requires one, and a profile tolerance is legally either — so `DatumRegime` rows carry the admission and a `bool` stranding the third case is the deleted form.
- Packages: UnitsNet (`Length`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Option`), System.Collections.Frozen (`FrozenDictionary`), `Numerics/atoms` (`VectorAngle` — radian value object; degree literals enter through `double.DegreesToRadians`).
- Growth: a height is one row; a form is one row carrying its own ratios; a proportion is one derived member; a datum regime, a characteristic, or a zone modifier is one row carrying its consequence.
- Boundary: the drafting FACE (the letterform family a host installs) resolves at the host — Rhino `FaceQuery` and AppUi's shaping owner — under a `LetteringForm` row; this page names the standard's form and never a font file; the OS UI roster (`TypeRole`) is not a drafting face and a plotted sheet never reads it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextHeight {
    public static readonly TextHeight H18 = new(key: "1.8", height: Length.FromMillimeters(1.8));
    public static readonly TextHeight H25 = new(key: "2.5", height: Length.FromMillimeters(2.5));
    public static readonly TextHeight H35 = new(key: "3.5", height: Length.FromMillimeters(3.5));
    public static readonly TextHeight H50 = new(key: "5", height: Length.FromMillimeters(5));
    public static readonly TextHeight H70 = new(key: "7", height: Length.FromMillimeters(7));
    public static readonly TextHeight H100 = new(key: "10", height: Length.FromMillimeters(10));
    public static readonly TextHeight H140 = new(key: "14", height: Length.FromMillimeters(14));
    public static readonly TextHeight H200 = new(key: "20", height: Length.FromMillimeters(20));
    public Length Height { get; }

    private static readonly Lazy<FrozenDictionary<SheetStandard, Seq<(Length Ceiling, TextHeight Floor)>>> Floors =
        new(static () => SheetStandard.Index(
            rows: Seq(
                (Standard: SheetStandard.Iso, Rungs: Seq((Ceiling: SheetSeries.IsoA.WidthAt(2), Floor: H25), (SheetSize.Unbounded, H35))),
                (Standard: SheetStandard.Ansi, Rungs: Seq((Ceiling: SheetSeries.Ansi.WidthAt(2), Floor: H35), (SheetSize.Unbounded, H50)))),
            column: static row => row.Standard)
            .ToFrozenDictionary(static pair => pair.Key, static pair => pair.Value.Rungs));
    public static Fin<TextHeight> For(SheetSize size, Op? key = null) =>
        Floors.Value[size.Standard].Find(rung => size.Width <= rung.Ceiling).Map(static rung => rung.Floor)
            .ToFin(new KernelFault.InvalidValue(Label: nameof(TextHeight), Requirement: $"a lettering floor covering the '{size.Key}' extent", Key: Some(key.OrDefault())));

    private static readonly Lazy<double[]> LogLadder = new(static () => Items.Select(static row => Math.Log(row.Height.Millimeters)).ToArray());
    public static Fin<TextHeight> For(Length height, Op? key = null) =>
        height.Millimeters > 0.0 && double.IsFinite(height.Millimeters)
            ? Fin.Succ(Items[RungLadder.NearestIndex(logs: LogLadder.Value, magnitude: height.Millimeters)])
            : Fin.Fail<TextHeight>(new KernelFault.OutOfRange(Label: nameof(TextHeight), Scalar: height.Millimeters, Requirement: "a positive finite lettering height", Key: Some(key.OrDefault())));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LetteringForm {
    public static readonly LetteringForm TypeA = new(key: "a-vertical", widthRatio: 1.0 / 14.0, pitchRatio: 1.5, lowerCaseRatio: 10.0 / 14.0, slant: VectorAngle.Create(value: 0.0));
    public static readonly LetteringForm TypeAItalic = new(key: "a-italic", widthRatio: 1.0 / 14.0, pitchRatio: 1.5, lowerCaseRatio: 10.0 / 14.0, slant: VectorAngle.Create(value: double.DegreesToRadians(15)));
    public static readonly LetteringForm TypeB = new(key: "b-vertical", widthRatio: 1.0 / 10.0, pitchRatio: 1.4, lowerCaseRatio: 7.0 / 10.0, slant: VectorAngle.Create(value: 0.0));
    public static readonly LetteringForm TypeBItalic = new(key: "b-italic", widthRatio: 1.0 / 10.0, pitchRatio: 1.4, lowerCaseRatio: 7.0 / 10.0, slant: VectorAngle.Create(value: double.DegreesToRadians(15)));
    public double WidthRatio { get; }
    public double PitchRatio { get; }
    public double LowerCaseRatio { get; }
    public VectorAngle Slant { get; }
    public DraftingMetrics Metrics(TextHeight height) => new(Height: height, Form: this);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DraftingMetrics(TextHeight Height, LetteringForm Form) {
    // --- [LETTERING]
    public Length LineWidth => Height.Height * Form.WidthRatio;
    public Length CharacterSpacing => LineWidth * 2.0;
    public Length LinePitch => Height.Height * Form.PitchRatio;
    public Length WordSpacing => LineWidth * 6.0;
    public Length LowerCaseHeight => Height.Height * Form.LowerCaseRatio;

    // --- [ANNOTATION]
    public Length FrameHeight => Height.Height * 2.0;
    public Length FramePad => Height.Height * 0.5;
    public Length ItemReferenceDiameter => Height.Height * 2.0;
    public Length ExtensionGap => LineWidth * 3.0;
    public Length ExtensionOvershoot => LineWidth * 8.0;
    public Length CentreMarkGap => LineWidth * 3.0;
    public Length SurfaceTextureLeg => Height.Height * 1.4;
    public Length SurfaceTextureLongLeg => Height.Height * 3.0;
    public Length CentreMark(Length diameter) => diameter * 0.18 is var arm && arm > LineWidth * 1.5 ? arm : LineWidth * 1.5;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Terminator {
    public static readonly Terminator ClosedArrow = new(key: "closed-arrow", lengthRatio: 10.0, angle: VectorAngle.Create(value: double.DegreesToRadians(15)));
    public static readonly Terminator OpenArrow = new(key: "open-arrow", lengthRatio: 10.0, angle: VectorAngle.Create(value: double.DegreesToRadians(30)));
    public static readonly Terminator ObliqueStroke = new(key: "oblique", lengthRatio: 10.0, angle: VectorAngle.Create(value: double.DegreesToRadians(45)));
    public static readonly Terminator Dot = new(key: "dot", lengthRatio: 5.0, angle: VectorAngle.Create(value: 0.0));
    public static readonly Terminator OriginCircle = new(key: "origin", lengthRatio: 5.0, angle: VectorAngle.Create(value: 0.0));
    public double LengthRatio { get; }
    public VectorAngle Angle { get; }
    public Length Size(LineWidth width) => width.Width * LengthRatio;
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class DatumDesignator {
    public char Primary { get; }
    public Option<char> Secondary { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref char primary, ref Option<char> secondary) =>
        validationError = Letter(primary) && secondary.Map(Letter).IfNone(true)
            ? null
            : new ValidationError(message: "DatumDesignator admits one ISO 5459 letter or a common-datum pair (A-B); I, O, Q excluded.");
    private static bool Letter(char glyph) => glyph is (>= 'A' and <= 'Z') and not 'I' and not 'O' and not 'Q';
    public static Fin<DatumDesignator> Of(char primary, Option<char> secondary = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<DatumDesignator>(Validate(primary, secondary, out DatumDesignator? datum), datum);
    public bool IsCommon => Secondary.IsSome;
    public string Text => Secondary.Match(
        Some: second => string.Create(CultureInfo.InvariantCulture, $"{Primary}-{second}"),
        None: () => Primary.ToString(CultureInfo.InvariantCulture));
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out DatumDesignator? item) {
        item = null;
        return value switch {
            [var only] => Validate(only, Option<char>.None, out item),
            [var first, '-', var second] => Validate(first, Some(second), out item),
            _ => new ValidationError(message: "DatumDesignator admits 'A' or a common-datum pair 'A-B'."),
        };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DatumRegime {
    public static readonly DatumRegime Free = new(key: "free", admits: static datum => datum.IsNone);
    public static readonly DatumRegime Optional = new(key: "optional", admits: static _ => true);
    public static readonly DatumRegime Required = new(key: "required", admits: static datum => datum.IsSome);
    [UseDelegateFromConstructor] public partial bool Admits(Option<DatumDesignator> datum);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ZoneModifier {
    public static readonly ZoneModifier Diametral = new(key: "diametral", glyph: '⌀', leading: true);
    public static readonly ZoneModifier Maximum = new(key: "maximum-material", glyph: 'Ⓜ', leading: false);
    public static readonly ZoneModifier Least = new(key: "least-material", glyph: 'Ⓛ', leading: false);
    public static readonly ZoneModifier FreeState = new(key: "free-state", glyph: 'Ⓕ', leading: false);
    public char Glyph { get; }
    public bool Leading { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometricCharacteristic {
    public static readonly GeometricCharacteristic Straightness = new(key: "straightness", glyph: '⏤', datums: DatumRegime.Free);
    public static readonly GeometricCharacteristic Flatness = new(key: "flatness", glyph: '⏥', datums: DatumRegime.Free);
    public static readonly GeometricCharacteristic Circularity = new(key: "circularity", glyph: '○', datums: DatumRegime.Free);
    public static readonly GeometricCharacteristic Cylindricity = new(key: "cylindricity", glyph: '⌭', datums: DatumRegime.Free);
    public static readonly GeometricCharacteristic ProfileLine = new(key: "profile-line", glyph: '⌒', datums: DatumRegime.Optional);
    public static readonly GeometricCharacteristic ProfileSurface = new(key: "profile-surface", glyph: '⌓', datums: DatumRegime.Optional);
    public static readonly GeometricCharacteristic Angularity = new(key: "angularity", glyph: '∠', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic Perpendicularity = new(key: "perpendicularity", glyph: '⟂', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic Parallelism = new(key: "parallelism", glyph: '∥', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic Position = new(key: "position", glyph: '⌖', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic Concentricity = new(key: "concentricity", glyph: '◎', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic Symmetry = new(key: "symmetry", glyph: '⌯', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic CircularRunout = new(key: "circular-runout", glyph: '↗', datums: DatumRegime.Required);
    public static readonly GeometricCharacteristic TotalRunout = new(key: "total-runout", glyph: '⌰', datums: DatumRegime.Required);
    public char Glyph { get; }
    public DatumRegime Datums { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolSet {
    public static readonly SymbolSet Iso1101 = new(key: "iso-1101", standard: SheetStandard.Iso,
        admits: static _ => true);
    public static readonly SymbolSet AsmeY145 = new(key: "asme-y14.5", standard: SheetStandard.Ansi,
        admits: static row => row != GeometricCharacteristic.Concentricity && row != GeometricCharacteristic.Symmetry);
    public SheetStandard Standard { get; }
    [UseDelegateFromConstructor] public partial bool Admits(GeometricCharacteristic characteristic);
    public Seq<GeometricCharacteristic> Rows => Admitted.Value[this];
    private static readonly Lazy<FrozenDictionary<SymbolSet, Seq<GeometricCharacteristic>>> Admitted =
        new(static () => Items.ToFrozenDictionary(static row => row, static row => toSeq(GeometricCharacteristic.Items).Filter(row.Admits).Strict()));
    public static SymbolSet For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, SymbolSet>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
}
```

## [10]-[UNITS]

- Owner: `DrawingUnits` `[SmartEnum<string>]` — the unit a sheet DECLARES it is drawn in (millimetres, metres, inches, feet-and-inches), keyed to the standard family; `DrawingPrecisionForm` `[Union<int, int>]` — the SHAPE a unit publishes its precision in, the `Places` decimal count or the `Fraction` inch denominator as two named slots; `DrawingPrecision` — the precision a scale implies: the smallest feature a plot resolves is the narrowest usable width of the `LineWidth` ladder on paper (ISO 128-24), so the model quantum is that width `× scale.Model / scale.Paper` and the unit's own row shapes it — a 1:100 plan dimensions to the centimetre, a 1:5 detail to the tenth of a millimetre, and a feet-and-inches drawing to an inch denominator off the published ladder, all by construction; `NorthPosture` `[SmartEnum<string>]` — project north versus true north as rows whose `Rotation(declination)` column answers the plan rotation, the declination read off `Rasm.Element` `GeoReference.RotationRadians` (the model's own `IfcGeometricRepresentationContext.TrueNorth`) and never hand-authored beside it.
- Entry: `DrawingUnits.For(standard)`; `ProjectionAngle.For(standard)` with `QuadrantSign`; `new DrawingPrecision(scale, units)` → `Quantum` and `Form()`; `NorthPosture.X.Rotation(VectorAngle declination)`.
- Law: three unit authorities exist — the MODEL's (`Rasm.Element` `UnitScheme`), the SHEET's (this row) and the USER's readout locale (AppUi `LocalePolicy`) — and a drawing STATES the sheet's; a title block reads `DrawingUnits`, never the locale.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union<T1, T2>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Seq`), System.Collections.Frozen (`FrozenDictionary`), `Numerics/atoms` (`VectorAngle`), UnitsNet (`Length.As`).
- Growth: a unit is one row naming its standard, whether it is that standard's PREFERRED spelling, and the precision shape it publishes; a precision shape is one union case.
- Boundary: the model geometric tolerance (`Rasm.Element` `Header.Tolerance`, a content-hash quantization grid) and the readout precision (`MeasureRole.decimals`) are DISTINCT and stay; AppUi `DraftPolicy.Declination` and Rhino `SunPlace.NorthAngle` delete for the one `GeoReference` read.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[Union<int, int>(T1Name = "Places", T2Name = "Fraction")]
public readonly partial struct DrawingPrecisionForm;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DrawingUnits {
    public static readonly DrawingUnits Millimetres = new(key: "mm", unit: LengthUnit.Millimeter, standard: SheetStandard.Iso, preferred: true, form: Decimal);
    public static readonly DrawingUnits Metres = new(key: "m", unit: LengthUnit.Meter, standard: SheetStandard.Iso, preferred: false, form: Decimal);
    public static readonly DrawingUnits Inches = new(key: "in", unit: LengthUnit.Inch, standard: SheetStandard.Ansi, preferred: true, form: Decimal);
    public static readonly DrawingUnits FeetInches = new(key: "ft-in", unit: LengthUnit.Foot, standard: SheetStandard.Arch, preferred: true, form: Fractional);
    public LengthUnit Unit { get; }
    public SheetStandard Standard { get; }
    public bool Preferred { get; }
    [UseDelegateFromConstructor] public partial DrawingPrecisionForm Form(double resolved);

    private static DrawingPrecisionForm Decimal(double resolved) => DrawingPrecisionForm.CreatePlaces(Math.Max(0, (int)Math.Ceiling(-Math.Log10(resolved))));
    private static readonly Seq<int> Denominators = Seq(2, 4, 8, 16, 32, 64);
    private static DrawingPrecisionForm Fractional(double resolvedFeet) =>
        DrawingPrecisionForm.CreateFraction(Denominators.Find(rung => 1.0 / rung <= resolvedFeet * 12.0).IfNone(Denominators[^1]));

    public static DrawingUnits For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, DrawingUnits>> ByStandard =
        new(static () => SheetStandard.Index(Items.Where(static row => row.Preferred), static row => row.Standard));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DrawingPrecision(DrawingScale Scale, DrawingUnits Units) {
    private static Length PaperQuantum => LineWidth.W025.Width;
    public Length Quantum => PaperQuantum * ((double)Scale.Model / Scale.Paper);
    public DrawingPrecisionForm Form() => Units.Form(resolved: Quantum.As(Units.Unit));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProjectionAngle {
    public static readonly ProjectionAngle First = new(key: "first-angle", standard: SheetStandard.Iso, quadrantSign: -1);
    public static readonly ProjectionAngle Third = new(key: "third-angle", standard: SheetStandard.Ansi, quadrantSign: +1);
    public SheetStandard Standard { get; }
    public int QuadrantSign { get; }
    public static ProjectionAngle For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, ProjectionAngle>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NorthPosture {
    public static readonly NorthPosture Project = new(key: "project", rotation: static _ => VectorAngle.Create(value: 0.0));
    public static readonly NorthPosture True = new(key: "true", rotation: static declination => declination);
    [UseDelegateFromConstructor] public partial VectorAngle Rotation(VectorAngle declination);
}
```

## [11]-[PLOT]

- Owner: `PlotResolution` `[SmartEnum<string>]` — the output-class resolutions (review 150 dpi, plot 300 dpi, archive 600 dpi) so no `300.0` literal is the one default every frameless request inherits; `PdfTrait` `[SmartEnum<string>]` realizing `ICapability<PdfTrait>` — PDF/A-2b, PDF/A-3, and PDF/UA as COMBINABLE conformance claims under one `CapabilityLaw` (ISO 19005-2/-3 and ISO 14289-1 are orthogonal and one file is routinely both); `LayerEmission` `[SmartEnum<string>]` — how layers cross to PDF (flattened, or optional-content groups per `HostLayerScheme.Pdf`); `IssuePosture` `[SmartEnum<string>]` — one row per issuing convention carrying every default an issued sheet takes; `PlotPolicy` — the issued-sheet policy binding size, orientation, frame, scale, line group, plot-style table, posture, resolution, layer emission, and PDF conformance into ONE admitted value the host PDF and print policies compose.
- Entry: `PlotPolicy.Of(size, orientation, scale, posture, resolution, emission, conformance, styles, key)` — `Validation` over every admissible column, with the frame and the line group DERIVED from the size inside the mint; `PlotPolicy.Issue(size, key)` — the size's own standard's `IssuePosture` row.
- Law: the plot posture and the PDF colour target bind at ONE value — AppUi's `PlotColor.Target` and `PdfPolicy.Color` were read separately for one emitted sheet; here `Posture` decides and the host colour target derives.
- Law: `Frame` and `Group` DERIVE from the size inside the mint and the ctor is private, so a frame from one standard beside a size from another is unrepresentable rather than guarded at a later read; an absent plot-style table rides the option, because an empty table resolves every pen to nothing while claiming to be one.
- Law: issued-sheet defaults are ROWS — orientation, scale, posture, resolution, emission, and conformance all read the standard's `IssuePosture` — so a new issuing convention is one row and never six literals inside a body.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Validation`), System.Collections.Frozen (`FrozenDictionary`), `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`), Rasm.Domain.
- Growth: a resolution class, a conformance trait, an emission mode, or an issuing convention is one row; a policy column is one field with one admission clause.
- Boundary: Rhino `PdfPolicy` (`Exchange/publish.md`), AppUi `PdfPolicy`/`PdfExport` (`Document/export.md`) and their `PrintPlan`/`VisualExport` arms stay plural per stratum (ruled) and COMPOSE `PlotPolicy` for the values they once carried as literals; PDFsharp hardening (permissions, tagging), Skia page begin, and the Rhino `FilePdf` custom-page gate are host mechanics and stay.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Drawing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlotResolution {
    public static readonly PlotResolution Review = new(key: "review", dpi: Dimension.Create(150));
    public static readonly PlotResolution Plot = new(key: "plot", dpi: Dimension.Create(300));
    public static readonly PlotResolution Archive = new(key: "archive", dpi: Dimension.Create(600));
    public Dimension Dpi { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PdfTrait : ICapability<PdfTrait> {
    public static readonly PdfTrait ArchivalA2b = new(key: "pdf-a-2b", rank: 0);
    public static readonly PdfTrait ArchivalA3 = new(key: "pdf-a-3", rank: 1);
    public static readonly PdfTrait Accessible = new(key: "pdf-ua", rank: 2);
    public int Rank { get; }
    public static CapabilityLaw<PdfTrait> Law => law.Value;
    private static readonly Lazy<CapabilityLaw<PdfTrait>> law =
        new(static () => CapabilityLaw<PdfTrait>.Forbidden(Seq(CapabilitySet<PdfTrait>.Of(ArchivalA2b, ArchivalA3))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerEmission {
    public static readonly LayerEmission Flattened = new(key: "flattened");
    public static readonly LayerEmission OptionalContent = new(key: "optional-content");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IssuePosture {
    public static readonly IssuePosture Iso = new(key: "iso", standard: SheetStandard.Iso, orientation: SheetOrientation.Landscape,
        scale: DrawingScale.Of(paper: 1, model: 100).ThrowIfFail(), posture: PlotPosture.Colour, resolution: PlotResolution.Plot,
        emission: LayerEmission.OptionalContent, conformance: CapabilitySet<PdfTrait>.Of(PdfTrait.ArchivalA2b));
    public static readonly IssuePosture Ansi = new(key: "ansi", standard: SheetStandard.Ansi, orientation: SheetOrientation.Landscape,
        scale: DrawingScale.Of(paper: 1, model: 120).ThrowIfFail(), posture: PlotPosture.Colour, resolution: PlotResolution.Plot,
        emission: LayerEmission.OptionalContent, conformance: CapabilitySet<PdfTrait>.Of(PdfTrait.ArchivalA2b));
    public static readonly IssuePosture Arch = new(key: "arch", standard: SheetStandard.Arch, orientation: SheetOrientation.Landscape,
        scale: DrawingScale.Of(paper: 1, model: 48).ThrowIfFail(), posture: PlotPosture.Colour, resolution: PlotResolution.Plot,
        emission: LayerEmission.OptionalContent, conformance: CapabilitySet<PdfTrait>.Of(PdfTrait.ArchivalA2b));
    public SheetStandard Standard { get; }
    public SheetOrientation Orientation { get; }
    public DrawingScale Scale { get; }
    public PlotPosture Posture { get; }
    public PlotResolution Resolution { get; }
    public LayerEmission Emission { get; }
    public CapabilitySet<PdfTrait> Conformance { get; }
    public static IssuePosture For(SheetStandard standard) => ByStandard.Value[standard];
    private static readonly Lazy<FrozenDictionary<SheetStandard, IssuePosture>> ByStandard =
        new(static () => SheetStandard.Index(Items, static row => row.Standard));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlotPolicy {
    private PlotPolicy(SheetSize size, SheetOrientation orientation, SheetFrame frame, DrawingScale scale, LineGroup group,
        Option<PlotStyleTable> styles, PlotPosture posture, PlotResolution resolution, LayerEmission emission, CapabilitySet<PdfTrait> conformance) =>
        (Size, Orientation, Frame, Scale, Group, Styles, Posture, Resolution, Emission, Conformance) =
        (size, orientation, frame, scale, group, styles, posture, resolution, emission, conformance);

    public SheetSize Size { get; }
    public SheetOrientation Orientation { get; }
    public SheetFrame Frame { get; }
    public DrawingScale Scale { get; }
    public LineGroup Group { get; }
    public Option<PlotStyleTable> Styles { get; }
    public PlotPosture Posture { get; }
    public PlotResolution Resolution { get; }
    public LayerEmission Emission { get; }
    public CapabilitySet<PdfTrait> Conformance { get; }

    public static Fin<PlotPolicy> Of(SheetSize size, SheetOrientation orientation, DrawingScale scale, PlotPosture posture,
        PlotResolution resolution, LayerEmission emission, CapabilitySet<PdfTrait> conformance,
        Option<PlotStyleTable> styles = default, Op? key = null) {
        Op op = key.OrDefault();
        return (
                ScaleLadder.For(size.Standard).Admits(scale)
                    ? Validation<Error, DrawingScale>.Success(scale)
                    : Validation<Error, DrawingScale>.Fail(new KernelFault.InvalidValue(Label: nameof(scale), Requirement: "a rung of the standard's scale ladder", Key: Some(op))),
                LineGroup.For(size: size, key: op).ToValidation(),
                PdfTrait.Law.Admit(conformance).ToValidation())
            .Apply((admittedScale, group, traits) => new PlotPolicy(
                size: size, orientation: orientation, frame: SheetFrame.For(size.Standard), scale: admittedScale, group: group,
                styles: styles, posture: posture, resolution: resolution, emission: emission, conformance: traits))
            .As().ToFin();
    }
    public static Fin<PlotPolicy> Issue(SheetSize size, Op? key = null) {
        IssuePosture convention = IssuePosture.For(size.Standard);
        return Of(size: size, orientation: convention.Orientation, scale: ScaleLadder.For(size.Standard).Nearest(convention.Scale),
            posture: convention.Posture, resolution: convention.Resolution, emission: convention.Emission,
            conformance: convention.Conformance, styles: None, key: key);
    }
}
```

## [12]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
