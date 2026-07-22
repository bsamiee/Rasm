# [APPUI_LOCALIZATION_CULTURE]

One locale law serves every AppUi surface: `LocaleRow` is the culture axis — tag, string-table source, typed flow direction, script-shaping policy, and format policy — while plural cardinality belongs to each message pattern rather than to the locale. `ResolvedLocale` binds `CultureInfo`, NodaTime display patterns, `CompositeFormat`, and one ICU `MessageFormatter`; `LocaleRuntime` propagates a complete candidate before publishing it, so failed propagation cannot expose mixed culture. `MirrorPolicy`, `ShapedAnnotation`, and `LiveCaption` share the typed asset, shaping, motion, and scheduler vocabularies rather than recreating string keys, bidi booleans, or cadence literals.

## [01]-[INDEX]

- [01]-[LOCALE_AXIS]: Culture rows: tag, source, typed flow, shaping, and format columns; message-level plural routes.
- [02]-[STRING_TABLES]: Inbox resx vocabulary, nameof-derived keys, `PropertyGrid` bridge.
- [03]-[CULTURE_COMPOSITION]: Resolve fold, atomic switch, pattern and format binding.
- [04]-[RTL_MIRRORING]: Flow application at surface root, icon mirroring exemption.

## [02]-[LOCALE_AXIS]

- Owner: `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `PluralRoute` `[SmartEnum<string>]` ICU-route policy rows; `LocaleRow` `[SmartEnum<string>]` culture axis.
- Cases: en, qps-ploc, qps-plocm — `En` is the shipped default; `PseudoLtr` proves expansion and `PseudoRtl` independently proves mirrored layout.
- Entry: `public partial string Source(string key, CultureInfo strings)` — the per-row string-table source column.
- Auto: generated `Items` and key lookup under the single comparer; `Source` rides `[UseDelegateFromConstructor]`. The `PluralResx` column is the per-row ICU-pattern source — it folds the `.one`/`.other`/`.few`/`.many`/`.zero`/`.two` satellite keys into one `{count, plural, …}` ICU pattern string the `MessageFormatter` resolves, so the plural grammar of a locale is CLDR data the engine reads, never a row-coded suffix branch.
- Packages: Jeffijoe.MessageFormat, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new shipped language is one `LocaleRow` row plus one satellite resx set; a locale whose CLDR plural rule the engine does not ship is one `Pluralizer` delegate registered onto `MessageFormatter.CardinalPluralizers`/`OrdinalPluralizers`, never a `Plural` dispatch arm; zero new surface.
- Boundary: `Flow` and `Shaping` are authoritative row columns — flow is never inferred from a culture tag, and `qps-ploc` remains left-to-right while `qps-plocm` proves mirroring independently; per-surface culture variance enters through `LocalePolicy`, never a second axis; script coverage remains ranked `FontChain` data while the locale carries the HarfBuzz segment policy that selects those faces; plural and select grammar lives in the full ICU pattern stored at the resx base key, and `PluralRoute` remains the closed validation vocabulary for cardinal and ordinal pattern inventories rather than a locale column.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PluralRoute {
    public static readonly PluralRoute Cardinal = new("cardinal", keyword: "plural");
    public static readonly PluralRoute Ordinal = new("ordinal", keyword: "selectordinal");

    public string Keyword { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class LocaleRow {
    public static readonly LocaleRow En = new("en", FlowDirection.LeftToRight, "en-US", new RunSpec(Direction.LeftToRight, Script.Latin, Language.Default, ClusterLevel.MonotoneGraphemes), source: LocaleStrings.Find, pluralResx: LocaleStrings.Pattern);
    public static readonly LocaleRow PseudoLtr = new("qps-ploc", FlowDirection.LeftToRight, "en-US", new RunSpec(Direction.LeftToRight, Script.Latin, Language.Default, ClusterLevel.MonotoneGraphemes), source: LocaleStrings.Expand, pluralResx: LocaleStrings.Pattern);
    public static readonly LocaleRow PseudoRtl = new("qps-plocm", FlowDirection.RightToLeft, "en-US", new RunSpec(Direction.RightToLeft, Script.Latin, Language.Default, ClusterLevel.MonotoneGraphemes), source: LocaleStrings.Expand, pluralResx: LocaleStrings.Pattern);

    public FlowDirection Flow { get; }

    public string FormatTag { get; }

    public RunSpec Shaping { get; }

    [UseDelegateFromConstructor]
    public partial string Source(string key, CultureInfo strings);

    [UseDelegateFromConstructor]
    public partial MessagePattern PluralResx(string key, PluralRoute route, CultureInfo strings);
}
```

## [03]-[STRING_TABLES]

- Owner: `LocaleStrings` static string-table surface; `PluralCategory` `[SmartEnum<string>]` the CLDR category axis.
- Entry: `public static string Find(string key, CultureInfo strings)` performs satellite lookup with a visible missing-key marker; `public static MessagePattern Pattern(string key, PluralRoute route, CultureInfo strings)` returns the full ICU pattern at the base key together with its typed route and any category seed rows used by authoring validation.
- Packages: Jeffijoe.MessageFormat, bodong.Avalonia.PropertyGrid, BCL inbox
- Growth: a new translatable surface is one resx key row per shipped locale row; a plural surface is the same base key plus its present CLDR-category satellites (`.one`/`.other`/`.few`/`.many`/`.zero`/`.two`); zero new surface.
- Boundary: inbox `ResourceManager` owns satellite, parent, and neutral fallback. The composition root supplies the public `ILocalizationService` implementation to `Propagate`; the unverified `LocalizationService.Default` accessor is absent. A base resx value contains the complete ICU message, so exact `=n` branches, offsets, nested `select`, escaping, cardinal plural, and ordinal plural remain engine-owned. `PluralCategory` satellites survive as seed data on `MessagePattern` for authoring and proof, never as a runtime reconstruction of the grammar.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PluralCategory {
    public static readonly PluralCategory Zero = new("zero");
    public static readonly PluralCategory One = new("one");
    public static readonly PluralCategory Two = new("two");
    public static readonly PluralCategory Few = new("few");
    public static readonly PluralCategory Many = new("many");
    public static readonly PluralCategory Other = new("other");
}

public readonly record struct MessagePattern(string Source, PluralRoute Route, Seq<(PluralCategory Category, string Seed)> Seeds) {
    // The route participates in admission: the stored ICU pattern must carry the requested route's
    // keyword, so a cardinal request cannot silently format an ordinal grammar and vice versa.
    public Fin<MessagePattern> Admitted(string key) =>
        Source.Contains(Route.Keyword, StringComparison.Ordinal)
            ? Fin.Succ(this)
            : Fin.Fail<MessagePattern>(new LocaleFault.FormatRejected($"{key}: pattern lacks the {Route.Key} '{Route.Keyword}' route"));
}

public static class LocaleStrings {
    public const string BaseName = "Rasm.AppUi.Strings";

    public static readonly ResourceManager Table = new(BaseName, typeof(LocaleStrings).Assembly);

    public static string Key(string owner, string member) => $"{owner}.{member}";

    public static string Find(string key, CultureInfo strings) => Table.GetString(key, strings) ?? $"[missing:{key}]";

    public static string Expand(string key, CultureInfo strings) => $"[!! {Find(key, strings)} !!]";

    public static MessagePattern Pattern(string key, PluralRoute route, CultureInfo strings) =>
        new(
            Source: Find(key, strings),
            Route: route,
            Seeds: toSeq(PluralCategory.Items)
                .Choose(category => Optional(Table.GetString($"{key}.{category.Key}", strings)).Map(seed => (category, seed))));
}
```

## [04]-[CULTURE_COMPOSITION]

- Owner: `LocalePolicy` user-settings options record; `ResolvedLocale` resolve product; `LocaleRuntime` apply-then-publish locale cell carrying the composition-bound `Propagate` rail.
- Entry: `public Fin<Unit> Apply(LocalePolicy policy)` — `Fin` aborts on unresolved tag, zone, culture, pattern, or propagation failure; the complete candidate propagates before the atom publishes it.
- Auto: `Republish` is the whole options-monitor bridge — `OptionsAdmission.Observe` wires it under the transition reload class, so a culture switch is an options reload, not a second driver; the resolved record binds one cached `MessageFormatter(useCache: true, culture: Formats, customValueFormatter: …)` per culture so each ICU pattern compiles once and reuses across calls, `LocaleValueFormatter` riding the ctor as the one typed-value coercion hook, and a locale swap mints a fresh formatter rather than mutating the live one.
- Receipt: `ReloadReceipt` per culture switch from the options monitor stream — section, transition class, `ReloadOutcome`, `Instant`, correlation.
- Packages: Jeffijoe.MessageFormat, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new display grammar is one pattern value on `ResolvedLocale`, a new format edge is one expression-bodied projection on the same record, and a locale whose CLDR rule the engine lacks is one `Pluralizer` registered on the formatter's `CardinalPluralizers`/`OrdinalPluralizers`; zero new surface.
- Boundary: ambient process culture remains absent. `ResolvedLocale.Resolve`, `Plural`, and `Message` trap culture and formatter exceptions onto `Fin`; `LocaleValueFormatter` implements date, time, and number hooks over NodaTime and the resolved format culture. `Propagate` reaches Semi resources and the injected PropertyGrid localization service before `Cell.Swap`, so a failure leaves the published generation unchanged. Full ICU patterns own plural, ordinal, gender, nested selection, exact branches, and offsets; call-site grammar branching is rejected.

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
    LocalTimePattern Time,
    DurationPattern Elapsed,
    IMessageFormatter Formatter) {
    public const string TimestampText = "G";
    public const string DateText = "D";
    public const string ElapsedText = "H:mm:ss";

    public static Fin<ResolvedLocale> Resolve(LocaleRow row, DateTimeZone zone, Option<string> formatTag) =>
        Try.lift(() => Compose(row, zone, CultureInfo.GetCultureInfo(formatTag.IfNone(row.FormatTag)))).Run()
            .MapFail(error => new LocaleFault.FormatRejected(error.Message));

    public string Label(string key) => Row.Source(key, Strings);

    public Fin<string> Plural(string key, long count, PluralRoute route) =>
        Row.PluralResx(key, route, Strings).Admitted(key)
            .Bind(pattern => Format(() => pattern.Source, ("count", count)));

    public Fin<string> Message(string key, params (string Name, object? Value)[] args) =>
        Format(() => Row.Source(key, Strings), args);

    public string Stamp(Instant value) => Timestamp.Format(value.InZone(Zone));

    public string Day(LocalDate value) => Date.Format(value);

    public string Clock(LocalTime value) => Time.Format(value);

    public string Span(Duration value) => Elapsed.Format(value);

    public string Text(CompositeFormat format, params object?[] args) => string.Format(Formats, format, args);

    public string Quantity(IFormattable value) => value.ToString(null, Formats);

    private Fin<string> Format(Func<string> pattern, params (string Name, object? Value)[] args) =>
        Try.lift(() => Formatter.FormatMessage(
            pattern(),
            args.Fold(new Dictionary<string, object?>(StringComparer.Ordinal), static (map, arg) => { map[arg.Name] = arg.Value; return map; }),
            Formats)).Run().MapFail(error => new LocaleFault.FormatRejected(error.Message));

    private static ResolvedLocale Compose(LocaleRow row, DateTimeZone zone, CultureInfo formats) =>
        (ZonedDateTimePattern.CreateWithInvariantCulture(TimestampText, DateTimeZoneProviders.Tzdb).WithCulture(formats),
         LocalDatePattern.CreateWithInvariantCulture(DateText).WithCulture(formats)) switch {
            var (timestamp, date) => new(
                Row: row,
                Strings: CultureInfo.GetCultureInfo(row.Key),
                Formats: formats,
                Zone: zone,
                Timestamp: timestamp,
                Date: date,
                Time: LocalTimePattern.ExtendedIso.WithCulture(formats),
                Elapsed: DurationPattern.CreateWithInvariantCulture(ElapsedText).WithCulture(formats),
                Formatter: new MessageFormatter(useCache: true, culture: formats, customValueFormatter: new LocaleValueFormatter(timestamp, date, LocalTimePattern.ExtendedIso.WithCulture(formats), zone))),
        };
}

// The one typed-value coercion hook: NodaTime arguments format through the resolved display patterns and
// IFormattable quantities through Formats, so ICU pattern arguments never open a second format path.
public sealed class LocaleValueFormatter(ZonedDateTimePattern timestamp, LocalDatePattern date, LocalTimePattern time, DateTimeZone zone) : CustomValueFormatter {
    public override bool TryFormatDate(CultureInfo culture, object? value, string? style, out string? formatted) =>
        (formatted = value switch {
            Instant instant => timestamp.Format(instant.InZone(zone)),
            LocalDate day => date.Format(day),
            _ => null,
        }) is not null;

    public override bool TryFormatTime(CultureInfo culture, object? value, string? style, out string? formatted) =>
        (formatted = value switch {
            LocalTime clock => time.Format(clock),
            Instant instant => time.Format(instant.InZone(zone).TimeOfDay),
            _ => null,
        }) is not null;

    public override bool TryFormatNumber(CultureInfo culture, object? value, string? style, out string? formatted) =>
        (formatted = value is IFormattable quantity ? quantity.ToString(style, culture) : null) is not null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LocaleFault : Expected {
    private LocaleFault(string detail, int code) : base(detail, code) { }
    public sealed record TagUnresolved(string Tag)
        : LocaleFault($"locale/tag: {Tag}", AppUiFaultBand.Locale.Code(0));
    public sealed record ZoneUnresolved(string Zone)
        : LocaleFault($"locale/zone: {Zone}", AppUiFaultBand.Locale.Code(1));
    public sealed record CaptionModelAbsent(string Model)
        : LocaleFault($"locale/caption-model: {Model}", AppUiFaultBand.Locale.Code(2));
    public sealed record FormatRejected(string Detail)
        : LocaleFault($"locale/format: {Detail}", AppUiFaultBand.Locale.Code(3));
    public sealed record PropagationRejected(string Detail)
        : LocaleFault($"locale/propagate: {Detail}", AppUiFaultBand.Locale.Code(4));
    public sealed record CaptionRejected(string Detail)
        : LocaleFault($"locale/caption: {Detail}", AppUiFaultBand.Locale.Code(5));
}

public sealed class LocaleRuntime(
    Atom<ResolvedLocale> cell,
    IDateTimeZoneProvider zones,
    Func<ResolvedLocale, Fin<Unit>> propagate) {
    public Atom<ResolvedLocale> Cell { get; } = cell;

    public IDateTimeZoneProvider Zones { get; } = zones;

    public Func<ResolvedLocale, Fin<Unit>> Propagate { get; } = propagate;

    public static Fin<LocaleRuntime> Boot(LocalePolicy policy, IDateTimeZoneProvider zones, Func<ResolvedLocale, Fin<Unit>> propagate) =>
        from resolved in Compose(policy, zones)
        from _ in propagate(resolved)
        select new LocaleRuntime(Atom(resolved), zones, propagate);

    public ResolvedLocale Current => Cell.Value;

    public Fin<Unit> Apply(LocalePolicy policy) =>
        from resolved in Compose(policy, Zones)
        from _ in Propagate(resolved)
        select ignore(Cell.Swap(_ => resolved));

    public ReloadOutcome Republish(LocalePolicy policy) =>
        Apply(policy) is { IsFail: true, Case: Error error }
            ? new ReloadOutcome.Rejected(LocalePolicy.Section, ConfigError.Create(error.Message))
            : new ReloadOutcome.Applied(LocalePolicy.Section);

    private static Fin<ResolvedLocale> Compose(LocalePolicy policy, IDateTimeZoneProvider zones) =>
        (RowFor(policy.Tag), Optional(zones.GetZoneOrNull(policy.Zone))) switch {
            ({ IsSome: true, Case: LocaleRow row }, { IsSome: true, Case: DateTimeZone zone }) =>
                ResolvedLocale.Resolve(row, zone, policy.FormatTag),
            ({ IsSome: false }, _) => Fin<ResolvedLocale>.Fail(new LocaleFault.TagUnresolved(policy.Tag)),
            _ => Fin<ResolvedLocale>.Fail(new LocaleFault.ZoneUnresolved(policy.Zone)),
        };

    private static Option<LocaleRow> RowFor(string tag) =>
        LocaleRow.TryGet(tag, out LocaleRow row) ? Optional(row) : None;
}
```

```mermaid
---
title: Locale resolution ownership
config:
  layout: elk
  htmlLabels: true
  markdownAutoWrap: false
  deterministicIds: true
  elk:
    nodePlacementStrategy: NETWORK_SIMPLEX
    considerModelOrder: NODES_AND_EDGES
  flowchart:
    curve: linear
    defaultRenderer: elk
    padding: 25
---
flowchart LR
    accTitle: Locale resolution ownership
    accDescr: Locale policy selects and transactionally publishes one resolved locale while locale data feeds full message formatting, directional asset mirroring, and reload evidence.
    LocalePolicy --> LocaleRuntime
    LocaleRuntime --> ResolvedLocale
    LocaleRuntime --> ReloadOutcome
    LocaleRow --> ResolvedLocale
    ResolvedLocale --> LocaleStrings
    LocaleRow --> MirrorPolicy
```

## [05]-[RTL_MIRRORING]

- Owner: `MirrorPolicy` directional-row policy record; `ShapedAnnotation` the complex-script 3D-annotation shaping projection; `CaptionSource` · `LiveCaption` the live-caption-and-translation owner.
- Entry: `public bool Mirrors(AssetKey iconKey, LocaleRow row)` uses the asset vocabulary; `ShapedAnnotation` carries the locale's complete `RunSpec`; `public IObservable<Fin<CaptionSegment>> Stream()` returns exhaustive VAD-segmented, globally timestamped, language- and confidence-bearing caption evidence on the locale fault rail.
- Packages: Avalonia, System.Reactive, Whisper.net, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a direction-sensitive glyph is one key row on `Directional`; a new caption source is one `CaptionSource` case; zero new surface.
- Boundary: the surface root inherits the row's typed `FlowDirection`, and only `AssetKey` values in `MirrorPolicy.Directional` rejoin that flow. `ShapedAnnotation` passes the row's `RunSpec` and its `TypographyRole` to typography, so annotation feature tags stay role-owned and one reconciled feature sequence reaches shaping. `CaptionEngine.Load` traps model admission and retains both model factories; each subscription admits one `CaptionSession` on `Fin` with one recognizer and one stateful VAD processor, so concurrent subscribers share immutable model handles but never mutable recognition state. `DetectSpeechNoResetAsync` preserves cross-window VAD state, each speech span slices the loss-bearing PCM window before transcription, and `CaptionSegment` retains stream-global start and end, detected language, probability, no-speech probability, token count, and the resolved dwell duration. `Stream` lifts session-construction failure and terminal Rx failure into `LocaleFault.CaptionRejected`, so `OnError` never becomes a second fault rail. The translated source derives its target as `LocaleRow.En`; `MotionPacing.Serial` delays and concatenates every derived caption while `ObserveOn` marshals that lossless stream, so no pacing operator discards audio ingress.

```csharp signature
public sealed record MirrorPolicy(Seq<AssetKey> Directional) {
    public static readonly MirrorPolicy Default = new(Seq(AssetKeys.NavBack, AssetKeys.NavForward));

    public bool Mirrors(AssetKey iconKey, LocaleRow row) =>
        row.Flow == FlowDirection.RightToLeft && Directional.Contains(iconKey);
}

public readonly record struct ShapedAnnotation(string Text, RunSpec Spec, TypographyRole Role) {
    public static ShapedAnnotation For(string key, ResolvedLocale locale) => Of(locale.Label(key), locale.Row);

    // Feature tags stay role-owned: shaping traverses Role.Features through the composition-bound
    // admission, and HarfBuzz applies script-required forms (rlig) from the RunSpec script itself, so a
    // locale-local feature vocabulary never forks the typography policy axis.
    public static ShapedAnnotation Of(string text, LocaleRow row) => new(text, row.Shaping, TypographyRole.Caption);
}

// LiveCaption realizes on Whisper.net — one owner for streaming transcription, Silero VAD segmentation,
// and the built-in translate-to-English task. Translated binds the English target; broad-target MT is a
// growth row on a named consumer, never a second engine here.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptionSource {
    private CaptionSource() { }
    public sealed record Live(IObservable<ReadOnlyMemory<float>> Pcm16k, Option<string> Language, LocaleRow Target) : CaptionSource;
    public sealed record Translated(IObservable<ReadOnlyMemory<float>> Pcm16k) : CaptionSource; // WithTranslate: English target
}

public sealed class CaptionEngine(WhisperFactory factory, WhisperVadFactory vadFactory) : IDisposable {
    public WhisperFactory Factory { get; } = factory;

    public WhisperVadFactory VadFactory { get; } = vadFactory;

    public static Fin<CaptionEngine> Load(string modelPath, string vadModelPath) =>
        File.Exists(modelPath) && File.Exists(vadModelPath)
            ? Try.lift(() => WhisperVadFactory.FromPath(vadModelPath) switch {
                    var vadFactory => new CaptionEngine(
                        WhisperFactory.FromPath(modelPath),
                        vadFactory),
                })
                .Run().MapFail(error => new LocaleFault.CaptionModelAbsent(error.Message))
            : Fin.Fail<CaptionEngine>(new LocaleFault.CaptionModelAbsent(File.Exists(modelPath) ? vadModelPath : modelPath));

    // Model weights arrive offline through WhisperGgmlDownloader.Default.GetGgmlModelAsync /
    // GetGgmlSileroVadModelAsync at provisioning, never at caption time.
    public Fin<CaptionSession> Session(CaptionSource source) =>
        Try.lift(() => new CaptionSession(
            source.Switch(
                state: Factory,
                live: static (factory, l) => l.Language.Match(
                    Some: language => factory.CreateBuilder().WithLanguage(language).Build(),
                    None: () => factory.CreateBuilder().WithLanguageDetection().Build()),
                translated: static (factory, _) => factory.CreateBuilder().WithLanguageDetection().WithTranslate().Build()),
            VadFactory.CreateBuilder().Build()))
            .Run().MapFail(error => new LocaleFault.CaptionRejected(error.Message));

    public void Dispose() { VadFactory.Dispose(); Factory.Dispose(); }
}

public sealed class CaptionSession(WhisperProcessor processor, WhisperVadProcessor vad) : IDisposable {
    public WhisperProcessor Processor { get; } = processor;

    public WhisperVadProcessor Vad { get; } = vad;

    public void Dispose() { Vad.Dispose(); Processor.Dispose(); }
}

public readonly record struct CaptionSegment(
    ShapedAnnotation Annotation,
    TimeSpan Start,
    TimeSpan End,
    string Language,
    float Probability,
    float NoSpeechProbability,
    int TokenCount,
    Duration Dwell);

public readonly record struct CaptionWindow(ReadOnlyMemory<float> Samples, TimeSpan Offset);

public sealed class LiveCaption(CaptionEngine engine, CaptionSource source, MotionToken dwell, IScheduler scheduler) {
    public CaptionEngine Engine { get; } = engine;

    public CaptionSource Source { get; } = source;

    public MotionToken Dwell { get; } = dwell;

    public IScheduler Scheduler { get; } = scheduler;

    // One processor retains recognition context for the subscription, and Concat serializes the processor
    // with the stateful VAD while every source window remains present in stream order.
    public IObservable<Fin<CaptionSegment>> Stream() =>
        Observable.Defer(() => Engine.Session(Source).Match(
            Succ: session => Observable.Using(
                () => session,
                Pipeline)
                .Select(static segment => Fin.Succ(segment))
                .Catch<Fin<CaptionSegment>, Exception>(error => Observable.Return(
                    Fin.Fail<CaptionSegment>(new LocaleFault.CaptionRejected(error.Message)))),
            Fail: error => Observable.Return(Fin.Fail<CaptionSegment>(error))));

    private IObservable<CaptionSegment> Pipeline(CaptionSession session) =>
        Dwell.Gate(
            MotionPacing.Serial,
            Pcm(Source).Scan(
                (NextOffset: 0L, Window: new CaptionWindow(ReadOnlyMemory<float>.Empty, TimeSpan.Zero)),
                static (state, samples) => (
                    NextOffset: state.NextOffset + samples.Length,
                    Window: new CaptionWindow(samples, TimeSpan.FromSeconds(state.NextOffset / 16_000d))))
                .Select(state => Observable.FromAsync(async ct =>
                    Speech(state.Window, await session.Vad.DetectSpeechNoResetAsync(state.Window.Samples, ct).ConfigureAwait(false))))
                .Concat()
                .SelectMany(static windows => windows)
                .Select(window => Observable.FromAsync(ct => Transcribe(session.Processor, window, Target(Source), Dwell.Duration, ct)))
                .Concat()
                .SelectMany(static segments => segments),
            Scheduler)
            .Where(static segment => !string.IsNullOrWhiteSpace(segment.Annotation.Text))
            .ObserveOn(Scheduler);

    static Seq<CaptionWindow> Speech(CaptionWindow window, IReadOnlyList<VadSegmentData> speech) =>
        toSeq(speech).Choose(span =>
            (Start: Math.Clamp((int)Math.Floor(span.Start.TotalSeconds * 16_000d), 0, window.Samples.Length),
             End: Math.Clamp((int)Math.Ceiling(span.End.TotalSeconds * 16_000d), 0, window.Samples.Length)) switch {
                var bounds when bounds.End > bounds.Start => Some(new CaptionWindow(
                    window.Samples.Slice(bounds.Start, bounds.End - bounds.Start),
                    window.Offset + span.Start)),
                _ => None,
            });

    static async Task<Seq<CaptionSegment>> Transcribe(
        WhisperProcessor processor,
        CaptionWindow window,
        LocaleRow target,
        Duration dwell,
        CancellationToken cancellationToken) =>
        toSeq(await processor.ProcessAsync(window.Samples, cancellationToken)
            .Select(segment => new CaptionSegment(
                ShapedAnnotation.Of(segment.Text, target),
                window.Offset + segment.Start,
                window.Offset + segment.End,
                segment.Language,
                segment.Probability,
                segment.NoSpeechProbability,
                segment.Tokens.Length,
                dwell))
            .ToArrayAsync(cancellationToken));

    static IObservable<ReadOnlyMemory<float>> Pcm(CaptionSource source) =>
        source.Switch(live: static l => l.Pcm16k, translated: static t => t.Pcm16k);

    static LocaleRow Target(CaptionSource source) =>
        source.Switch(live: static value => value.Target, translated: static _ => LocaleRow.En);
}
```

## [06]-[RESEARCH]

- [PSEUDO_LOCALE]: qps-ploc satellite resx resolution through the ResourceManager fallback fold on ICU-backed globalization.
- [BROAD_TARGET_MT]: machine translation past the Whisper.net translate-to-English arm is a growth row — it re-opens only when a consumer names a non-English caption target; the recognizer, VAD, model-download, segment, streaming, and translate members are VERIFIED against `.api/api-whisper-net.md` and the caption owner is settled.
