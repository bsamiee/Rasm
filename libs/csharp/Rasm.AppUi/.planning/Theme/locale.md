# [APPUI_LOCALIZATION_CULTURE]

One locale law serves every AppUi surface: `LocaleRow` is the culture axis — tag, string-table source, flow direction, format policy, plural route — `ResolvedLocale` is the single resolve product binding `CultureInfo` composition, the NodaTime display patterns, the `CompositeFormat` rail, and the one ICU `MessageFormatter` engine, and `LocaleStrings` is the inbox-resx string vocabulary whose nameof-derived keys the command table, the screen catalog, and the PropertyGrid `LocalizationService` bridge all share. `LocaleRuntime` swaps the resolved set atomically from the user-settings `LocalePolicy` section, and `MirrorPolicy` owns retained-layout mirroring with the icon exemption law. The package spine is BCL inbox globalization and resources, the `Jeffijoe.MessageFormat` ICU CLDR plural/select engine, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [01]-[LOCALE_AXIS]: Culture rows: tag, source, flow, format, plural-route columns.
- [02]-[STRING_TABLES]: Inbox resx vocabulary, nameof-derived keys, `PropertyGrid` bridge.
- [03]-[CULTURE_COMPOSITION]: Resolve fold, atomic switch, pattern and format binding.
- [04]-[RTL_MIRRORING]: Flow application at surface root, icon mirroring exemption.

## [02]-[LOCALE_AXIS]

- Owner: `LocaleKeyPolicy` single ordinal-ignore-case key accessor; `LocaleRow` `[SmartEnum<string>]` culture axis.
- Cases: en, qps-ploc — En is the shipped default; Pseudo is the conformance row proving string expansion and mirrored layout in headless evidence.
- Entry: `public partial string Source(string key, CultureInfo strings)` — the per-row string-table source column.
- Auto: generated `Items` and key lookup under the single comparer; `Source` rides `[UseDelegateFromConstructor]`. The `PluralResx` column is the per-row ICU-pattern source — it folds the `.one`/`.other`/`.few`/`.many`/`.zero`/`.two` satellite keys into one `{count, plural, …}` ICU pattern string the `MessageFormatter` resolves, so the plural grammar of a locale is CLDR data the engine reads, never a row-coded suffix branch.
- Packages: Jeffijoe.MessageFormat, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new shipped language is one `LocaleRow` row plus one satellite resx set; a locale whose CLDR plural rule the engine does not ship is one `Pluralizer` delegate registered onto `MessageFormatter.CardinalPluralizers`/`OrdinalPluralizers`, never a `Plural` dispatch arm; zero new surface.
- Boundary: `RightToLeft` is the authoritative flow column — flow is never derived from culture data, so the pseudo row proves mirroring on every platform; per-surface culture variance enters as the `LocalePolicy` construction source, never a second axis — embedded panel surfaces fold the host language probe, an `AppearanceSettings.LanguageIdentifier` LCID read through `CultureInfo.GetCultureInfo(int)` behind a `HostAttachPort` delegate, to a row key with unmatched tags folding to `En` at the probe edge, and standalone surfaces carry the user-chosen user-settings value; script coverage for non-Latin rows arrives as ranked family values on the `FontChain` rows, so a locale row never carries font data; `qps-ploc` constructs on ICU-backed globalization as `qps-Ploc` with parent `qps`, and its satellite resolution rides the research row; the plural grammar is the ICU `PluralFormatter` over the row's `PluralResx` pattern — `PluralRoute` names the CLDR category set (`cardinal-*`/`ordinal-*`) the `MessageFormatter` selects from, so a row carries the route discriminant only and the grammatical branch is engine-resolved CLDR data, never an inline `count == 1 ?` arm.

```csharp signature
public sealed class LocaleKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<LocaleKeyPolicy, string>]
[KeyMemberComparer<LocaleKeyPolicy, string>]
public sealed partial class LocaleRow {
    public static readonly LocaleRow En = new("en", rightToLeft: false, formatTag: "en-US", pluralRoute: Cardinal, source: LocaleStrings.Find, pluralResx: LocaleStrings.Pattern);
    public static readonly LocaleRow Pseudo = new("qps-ploc", rightToLeft: true, formatTag: "en-US", pluralRoute: Cardinal, source: LocaleStrings.Expand, pluralResx: LocaleStrings.Pattern);

    public const string Cardinal = "cardinal";
    public const string Ordinal = "ordinal";

    public bool RightToLeft { get; }

    public string FormatTag { get; }

    public string PluralRoute { get; }

    [UseDelegateFromConstructor]
    public partial string Source(string key, CultureInfo strings);

    [UseDelegateFromConstructor]
    public partial string PluralResx(string key, string route, CultureInfo strings);
}
```

## [03]-[STRING_TABLES]

- Owner: `LocaleStrings` static string-table surface; `PluralCategory` `[SmartEnum<string>]` the CLDR category axis.
- Entry: `public static string Find(string key, CultureInfo strings)` — satellite lookup; a missing key returns the key itself as the visible marker; `public static string Pattern(string key, string route, CultureInfo strings)` — folds the present CLDR-category satellite keys for one base key into a single `{count, <route>, …}` ICU pattern the `MessageFormatter` interprets.
- Packages: Jeffijoe.MessageFormat, bodong.Avalonia.PropertyGrid, BCL inbox
- Growth: a new translatable surface is one resx key row per shipped locale row; a plural surface is the same base key plus its present CLDR-category satellites (`.one`/`.other`/`.few`/`.many`/`.zero`/`.two`); zero new surface.
- Boundary: inbox `ResourceManager` owns the fallback fold — satellite culture, then parent, then neutral — so a hand-rolled chain walk and per-screen resx managers are the deleted patterns; keys compose through `Key` from nameof-derived owner and member symbols — the command intent key doubles as its label key and the screen catalog `Title` cell arrives as a formed key, so call-site string-literal keys are the deleted form; the bodong bridge implements `PropertyModels.Localization.ILocalizationService` — its string indexer rides `Find` — registered through `LocalizationService.Default.AddExtraService` with `SelectCulture` keeping the grid in step on every locale swap, so PropertyGrid display names and built-in strings ride this one vocabulary; the inspector color editor's swatch names resolve through `ColorHelper.ToDisplayName` so a named-color label rides the same resolved string vocabulary rather than a hardcoded color-name table; Microsoft.Extensions.Localization is the named-rejected owner shape and a translation-service abstraction is the rejected form — string tables are data rows; `Expand` brackets resolved text so truncation and clipping surface in pseudo-row headless sweeps; `Pattern` assembles the ICU pattern from the resx-authored per-category strings (`{cardinal|ordinal}` route -> `plural`/`selectordinal` keyword, each present `PluralCategory` -> one `<cat> {…}` branch with the bracketed `#` value position the resx string carries), so the localizable artifact is the per-category resx text and the assembled pattern feeds the one `MessageFormatter` — a `count == 1 ? OneSuffix : OtherSuffix` suffix branch anywhere is the deleted form, and a locale's exact category set is engine-resolved CLDR data, not a fixed one/other pair.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<LocaleKeyPolicy, string>]
[KeyMemberComparer<LocaleKeyPolicy, string>]
public sealed partial class PluralCategory {
    public static readonly PluralCategory Zero = new("zero");
    public static readonly PluralCategory One = new("one");
    public static readonly PluralCategory Two = new("two");
    public static readonly PluralCategory Few = new("few");
    public static readonly PluralCategory Many = new("many");
    public static readonly PluralCategory Other = new("other");
}

public static class LocaleStrings {
    public const string BaseName = "Rasm.AppUi.Strings";

    public static readonly ResourceManager Table = new(BaseName, typeof(LocaleStrings).Assembly);

    public static string Key(string owner, string member) => $"{owner}.{member}";

    public static string Find(string key, CultureInfo strings) => Table.GetString(key, strings) ?? key;

    public static string Expand(string key, CultureInfo strings) => $"[!! {Find(key, strings)} !!]";

    public static string Pattern(string key, string route, CultureInfo strings) =>
        toSeq(PluralCategory.Items)
            .Choose(cat => Optional(Table.GetString($"{key}.{cat.Key}", strings)).Map(text => (Cat: cat.Key, Text: text)))
            .Match(
                Empty: () => Find(key, strings),
                Seq: present => $"{{count, {Keyword(route)}, {string.Join(" ", present.Map(static p => $"{p.Cat} {{{p.Text}}}"))}}}");

    static string Keyword(string route) => route == LocaleRow.Ordinal ? "selectordinal" : "plural";
}
```

## [04]-[CULTURE_COMPOSITION]

- Owner: `LocalePolicy` user-settings options record; `ResolvedLocale` resolve product; `LocaleRuntime` atomic locale cell.
- Entry: `public Fin<Unit> Apply(LocalePolicy policy)` — `Fin` aborts on an unresolved tag or zone; one swap publishes the full resolved set.
- Auto: `Republish` is the whole options-monitor bridge — `OptionsAdmission.Observe` wires it under the transition reload class, so a culture switch is an options reload, not a second driver; the resolved record binds one cached `MessageFormatter(useCache: true, culture: Formats)` per culture so each ICU pattern compiles once and reuses across calls, and a locale swap mints a fresh formatter rather than mutating the live one.
- Receipt: `ReloadReceipt` per culture switch from the options monitor stream — section, transition class, `ReloadOutcome`, `Instant`, correlation.
- Packages: Jeffijoe.MessageFormat, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new display grammar is one pattern value on `ResolvedLocale`, a new format edge is one expression-bodied projection on the same record, and a locale whose CLDR rule the engine lacks is one `Pluralizer` registered on the formatter's `CardinalPluralizers`/`OrdinalPluralizers`; zero new surface.
- Boundary: ambient culture is structurally absent — `CultureInfo.CurrentCulture`, `CurrentUICulture`, and default-thread culture writes never appear because the host process owns process culture on embedded rows; consumers read `Current` and pass `Strings`/`Formats` explicitly, and one swap publishes strings, patterns, formats, the formatter, and flow together so no frame observes a mixed-culture state — per-control culture refresh handlers are the deleted pattern; persisted temporal grammars stay invariant through the `ClockPolicy` patterns, the resolved record owns only the user-facing display edge, and `ZonedDateTime` projection is confined inside `Stamp`; `CompositeFormat` is the runtime-format path for positional substitution and `Plural`/`Select` route through the one ICU `MessageFormatter` — quantities format through the `IFormattable` edge with `Formats` (the same culture the inspector quantity admission receives) and the formatter's `CustomValueFormatter` is the one typed-value coercion hook so a `{when, date, long}` or `{qty, number}` argument in a pattern respects `Formats` without a parallel path; a rejected `LocalePolicy` write keeps prior values live and cross-process propagation rides the op-log cursor consequence; the three temporal pattern texts are execution-proven — `G` formats the zoned timestamp, `D` the full date, `H:mm:ss` the duration; the plural and gender route is the live ICU `MessageFormatter` over the row's assembled `PluralResx` pattern — the engine selects the grammatically-correct CLDR branch (`zero`/`one`/`two`/`few`/`many`/`other`, `=n` exact) for the active culture, so the suffix-branch `Plural` is DROPPED root-up and a `count == 1 ?` ternary anywhere outside an ICU pattern is the deleted form.

```csharp signature
public sealed record LocalePolicy(string Tag, string Zone, Option<string> FormatTag) {
    public const string Section = nameof(LocalePolicy);

    public static readonly LocalePolicy Default = new(Tag: "en", Zone: "Etc/UTC", FormatTag: None);
}

public sealed record ResolvedLocale(
    LocaleRow Row,
    CultureInfo Strings,
    CultureInfo Formats,
    DateTimeZone Zone,
    ZonedDateTimePattern Timestamp,
    LocalDatePattern Date,
    DurationPattern Elapsed,
    IMessageFormatter Formatter) {
    public const string TimestampText = "G";
    public const string DateText = "D";
    public const string ElapsedText = "H:mm:ss";

    public static ResolvedLocale Resolve(LocaleRow row, DateTimeZone zone, Option<string> formatTag = default) =>
        Compose(row, zone, CultureInfo.GetCultureInfo(formatTag.IfNone(row.FormatTag)));

    public string Label(string key) => Row.Source(key, Strings);

    public string Plural(string key, long count) =>
        Formatter.FormatMessage(Row.PluralResx(key, Row.PluralRoute, Strings), new Dictionary<string, object?> { ["count"] = count }, Formats);

    public string Select(string key, string branch, params (string Name, object? Value)[] args) =>
        Formatter.FormatMessage(Row.Source(key, Strings), args.Fold(new Dictionary<string, object?>(StringComparer.Ordinal), static (map, arg) => { map[arg.Name] = arg.Value; return map; }), Formats);

    public string Stamp(Instant value) => Timestamp.Format(value.InZone(Zone));

    public string Day(LocalDate value) => Date.Format(value);

    public string Span(Duration value) => Elapsed.Format(value);

    public string Text(CompositeFormat format, params object?[] args) => string.Format(Formats, format, args);

    public string Quantity(IFormattable value) => value.ToString(null, Formats);

    private static ResolvedLocale Compose(LocaleRow row, DateTimeZone zone, CultureInfo formats) =>
        new(
            Row: row,
            Strings: CultureInfo.GetCultureInfo(row.Key),
            Formats: formats,
            Zone: zone,
            Timestamp: ZonedDateTimePattern.CreateWithInvariantCulture(TimestampText, DateTimeZoneProviders.Tzdb).WithCulture(formats),
            Date: LocalDatePattern.CreateWithInvariantCulture(DateText).WithCulture(formats),
            Elapsed: DurationPattern.CreateWithInvariantCulture(ElapsedText).WithCulture(formats),
            Formatter: new MessageFormatter(useCache: true, culture: formats));
}

public sealed record LocaleRuntime(Atom<ResolvedLocale> Cell, IDateTimeZoneProvider Zones) {
    public static Fin<LocaleRuntime> Boot(LocalePolicy policy, IDateTimeZoneProvider zones) =>
        Compose(policy, zones).Map(resolved => new LocaleRuntime(Atom(resolved), zones));

    public ResolvedLocale Current => Cell.Value;

    public Fin<Unit> Apply(LocalePolicy policy) =>
        Compose(policy, Zones).Map(resolved => ignore(Cell.Swap(_ => resolved)));

    public ReloadOutcome Republish(LocalePolicy policy) =>
        Apply(policy) is { IsFail: true, Case: Error error }
            ? new ReloadOutcome.Rejected(LocalePolicy.Section, ConfigError.Create(error.Message))
            : new ReloadOutcome.Applied(LocalePolicy.Section);

    private static Fin<ResolvedLocale> Compose(LocalePolicy policy, IDateTimeZoneProvider zones) =>
        (RowFor(policy.Tag), Optional(zones.GetZoneOrNull(policy.Zone))) switch {
            ({ IsSome: true, Case: LocaleRow row }, { IsSome: true, Case: DateTimeZone zone }) =>
                Fin<ResolvedLocale>.Succ(ResolvedLocale.Resolve(row, zone, policy.FormatTag)),
            ({ IsSome: false }, _) => Fin<ResolvedLocale>.Fail(Error.New($"unresolved locale tag: {policy.Tag}")),
            _ => Fin<ResolvedLocale>.Fail(Error.New($"unresolved zone id: {policy.Zone}")),
        };

    private static Option<LocaleRow> RowFor(string tag) =>
        LocaleRow.Items.AsIterable().Find(item => LocaleKeyPolicy.EqualityComparer.Equals(item.Key, tag));
}
```

```mermaid
flowchart LR
    LocalePolicy --> LocaleRuntime
    LocaleRuntime --> ResolvedLocale
    LocaleRuntime --> ReloadOutcome
    LocaleRow --> ResolvedLocale
    ResolvedLocale --> LocaleStrings
    LocaleRow --> MirrorPolicy
```

## [05]-[RTL_MIRRORING]

- Owner: `MirrorPolicy` directional-row policy record; `ShapedAnnotation` the complex-script 3D-annotation shaping projection; `CaptionSource` · `LiveCaption` the live-caption-and-translation owner.
- Entry: `public bool Mirrors(string iconKey, LocaleRow row)` — pure predicate; the icon presenter's flow pin folds over it; `public static ShapedAnnotation For(string key, ResolvedLocale locale)` — projects a 3D-annotation string into its shaping features and flow; `public IObservable<ShapedAnnotation> Stream(ResolvedLocale locale)` — the live-caption stream optionally translated into the target locale.
- Packages: Avalonia, System.Reactive, LanguageExt.Core
- Growth: a direction-sensitive glyph is one key row on `Directional`; a new caption source is one `CaptionSource` case; zero new surface.
- Boundary: the mount transaction writes the active row's flow once to the surface root's inherited `Visual.FlowDirection` (`LeftToRight` | `RightToLeft`) and layout mirroring arrives by inheritance — per-control flow flips are the deleted pattern; every icon presenter pins `LeftToRight` structurally — the exemption — and only `Directional` rows re-join the root flow, so logos, status glyphs, and brand marks never mirror; 3D-annotation complex-script shaping rides `ShapedAnnotation` — a viewport label, dimension text, or scene annotation carries its locale's bidi flow and complex-script feature tags (RTL ligatures for Arabic/Hebrew, contextual alternates for Indic) which feed the typography `ShapingSurface.DrawLabel` HarfBuzz rail so a 3D annotation shapes through the one shaping owner exactly as retained text does and a per-annotation glyph loop is the deleted form; drafting title-blocks are locale-aware through the drafting page's `TitleBlock.Fields(ResolvedLocale)` so the ISO/ANSI/JIS field labels and the date resolve through this locale vocabulary — the title-block standard owns layout, the locale owns the field text, and a hardcoded title-block label is the deleted form; live caption and translation ride `LiveCaption` — a spoken or streamed utterance source projects into shaped annotations, the `Translated` source folds each utterance through the composition-bound `Translate` delegate into the target locale before shaping so a live session captions in the viewer's language, with the speech-recognition and translation engine member spellings research-gated; Skia-side bidi and complex-script ordering stay with the shaping rail and this cluster owns retained-layout mirroring plus the annotation-shaping and caption projection; caret travel and text alignment arrive from the inherited flow, never per-control settings.

```csharp signature
public sealed record MirrorPolicy(Seq<string> Directional) {
    public static readonly MirrorPolicy Default = new(Seq("nav.back", "nav.forward"));

    public bool Mirrors(string iconKey, LocaleRow row) =>
        row.RightToLeft && Directional.Exists(key => string.Equals(key, iconKey, StringComparison.Ordinal));
}

public readonly record struct ShapedAnnotation(string Text, bool RightToLeft, Seq<string> Features) {
    public static ShapedAnnotation For(string key, ResolvedLocale locale) =>
        new(locale.Label(key), locale.Row.RightToLeft, locale.Row.RightToLeft ? Seq("calt", "liga", "rlig") : Seq("calt", "liga"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptionSource {
    private CaptionSource() { }
    public sealed record Live(IObservable<string> Utterances) : CaptionSource;
    public sealed record Translated(IObservable<string> Utterances, Func<string, LocaleRow, IO<string>> Translate) : CaptionSource;
}

public sealed record LiveCaption(CaptionSource Source, LocaleRow Target, double DwellSeconds) {
    public IObservable<ShapedAnnotation> Stream(ResolvedLocale locale) =>
        Source.Switch(
            state: (Target: Target, Locale: locale),
            live: static (ctx, l) => l.Utterances.Select(text => new ShapedAnnotation(text, ctx.Locale.Row.RightToLeft, Seq("calt", "liga"))),
            translated: static (ctx, t) => t.Utterances.Select(text => t.Translate(text, ctx.Target).Run())
                .Select(translated => new ShapedAnnotation(translated, ctx.Target.RightToLeft, ctx.Target.RightToLeft ? Seq("calt", "liga", "rlig") : Seq("calt", "liga"))));
}
```

## [06]-[RESEARCH]

- [PSEUDO_LOCALE]: qps-ploc satellite resx resolution through the ResourceManager fallback fold on ICU-backed globalization.
- [LIVE_TRANSLATE]: the speech-recognition utterance source and the machine-translation engine the `LiveCaption.Translated` `Translate` delegate binds — the recognizer producing the utterance stream and the translation service producing the target-locale text, resolved at implementation against an admitted speech-and-translation package bound through a composition delegate; the `ShapedAnnotation` complex-script projection, the `CaptionSource` union, and the caption stream are settled, the recognizer-and-translator member spellings are the unverified surface.
