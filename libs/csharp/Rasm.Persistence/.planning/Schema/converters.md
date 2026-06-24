# [PERSISTENCE_SCHEMA_CONVERTERS]

The codec axis of every store the suite opens. `ConverterRail` mounts the converter-admission convention, the culture-pinned snake-case naming, the full NodaTime temporal vocabulary, the keyed-column width budget, the per-profile concurrency token, and the compiled-model freeze — all as data folded once at `ConfigureConventions` and `OnConfiguring`, never per entity and never per column. Two closed families carry the variation: `TemporalCodec` is the `[SmartEnum<string>]` data table of every persisted clock type with its parse/format pattern and its native PG store types (scalar plus range plus multirange), owning its own type index and pair-pattern factory; `ConcurrencyToken` is the `[SmartEnum<string>]` whose generated `Get` resolves the `StoreProfile.ConcurrencyToken` string column to one row, binding the EF token shape per posture at entity-config time. Naming is not an axis: snake-case is one suite-wide day-zero policy applied inside `Compose`, because a second casing on a live schema is a full-rename migration, never a runtime row swap. The spine is the two EF Core providers, `EFCore.NamingConventions`, `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`, and the Npgsql NodaTime plugin — every converter admits at model build, every temporal value crosses through one typed `ValueConverter`, and no `DateTime` sentinel ever reaches a store.

## [01]-[INDEX]

- [01]-[CONVERTER_RAIL]: converter-admission convention, culture-pinned snake-case naming, temporal codec table, keyed width budget, per-profile concurrency token, and compiled-model freeze.

## [02]-[CONVERTER_RAIL]

Converter admission is a CODEC_AXIS column, not a static surface. Generated domain types cross through one convention (`UseThinktectureValueConverters`); temporal types cross through one pre-convention registration (`ConfigureConventions`); the concurrency token resolves the profile's string column to one row at entity config. The interior never sees a `HasConversion` call, a `DateTime`, or a provider-shaped value — every conversion is admitted once at model finalization and survives the compiled-model freeze byte-identically.

- Owner: `ConverterRail` static composition surface owning the two admission folds (snake-case naming plus Thinktecture admission on the options leg, temporal pre-conventions on the model-config leg) plus the entity-leg token projection and the compiled-model mount; `TemporalCodec` `[SmartEnum<string>]` the frozen clock-type table owning its `Of<T>`/`Of(Type)` type index, its `ConverterType` projection, its scalar/range/multirange PG store columns, and the `PairFor` endpoint-pair factory; `ConcurrencyToken` `[SmartEnum<string>]` the per-posture token shape whose generated `Get` resolves the `StoreProfile.ConcurrencyToken` string column; `PatternConverter<T>` the one generic NodaTime text converter every temporal column closes; `PairPattern<T,TPart>` the `IPattern<T>` endpoint-pair implementation for the range types NodaTime gives no text pattern.
- Cases: `TemporalCodec` carries `Instant`, `LocalDate`, `LocalTime`, `LocalDateTime`, `OffsetDateTime`, `OffsetDate`, `OffsetTime`, `ZonedDateTime`, `Offset`, `Duration`, `Period`, `AnnualDate`, `YearMonth`, and the composed `Interval`/`DateInterval` endpoint-pair rows — the full `api-nodatime` value vocabulary, each row carrying its parse-capable `IPattern<T>`, its scalar PG type, and (for the range rows) its range and multirange PG types; `ConcurrencyToken` carries `xmin` (pg system column), `version` (integer counter), and `none`.
- Entry: `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options)` is the one options-delegate tail every profile folds in — culture-pinned `UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture)` plus Thinktecture admission; `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled)` adds the frozen-model mount onto that tail; `public static ModelConfigurationBuilder Conventions(ModelConfigurationBuilder conventions, bool nativeTemporal)` registers every `TemporalCodec` row's text converter once across all entities on legs without native NodaTime mapping, and is a no-op on the pg leg (`nativeTemporal: true`) where the plugin maps natively; `public static EntityTypeBuilder Token(string token, EntityTypeBuilder entity)` resolves the profile's `ConcurrencyToken` string through the generated `Get` and binds the row onto an aggregate root; `public static string Format<T>(T value)` and `public static Fin<T> Parse<T>(string text)` round-trip a temporal CLR value through its codec row.
- Packages: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`, `EFCore.NamingConventions`, `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `NodaTime`, `LanguageExt.Core`
- Growth: one `TemporalCodec` row per new clock type (its pattern, scalar PG type, and range/multirange PG types are columns); one `ConcurrencyToken` row per new optimistic-concurrency posture; a new generated domain owner costs zero converter code on the same `UseThinktectureValueConverters` convention; a casing change is a day-zero suite decision in `Compose`, never a row; zero new surface.
- Boundary: every admission below is data on a closed family. The interior code never re-validates, never branches on provider, and never owns a converter class.

Generated-type admission rides one convention. `UseThinktectureValueConverters(ConverterAdmission)` installs the model-building convention so every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column maps through `ThinktectureValueConverterFactory.Create<T,TKey>` at finalization with zero hand-written converter classes; a single declared property narrows through `PropertyBuilder.HasThinktectureValueConverter`, a complex-type member through `ComplexTypePropertyBuilderExtensions`, and a primitive-collection element through `PrimitiveCollectionBuilderExtensions` — a per-column conversion is one builder call, never a converter class, and a hand-written `HasConversion` for a generated type is the deleted form.

Key-column width is a real strategy, not a default. `Configuration` is a `sealed class` with `init`-only members and no record `with`, so `ConverterAdmission` is an object initializer — `new Configuration { SmartEnums = SmartEnumConfiguration.Default, KeyedValueObjects = new KeyedValueObjectConfiguration { MaxLengthStrategy = new FixedKeyedValueObjectMaxLengthStrategy(KeyWidth) } }` — pinning the smart-enum leg to `SmartEnumConfiguration.Default` (columns bound to the widest string-keyed item) and flipping only the keyed-value-object leg to a declared `varchar(KeyWidth)`, because `Configuration.Default` leaves `KeyedValueObjects = KeyedValueObjectConfiguration.NoMaxLength` (unbounded) and an unbounded key column fractures the schema fingerprint. `Configuration.NoMaxLength` is the rejected unbounded form; `FixedKeyedValueObjectMaxLengthStrategy(KeyWidth)` is the bounded selection, and its `overwriteExistingMaxLength` ctor flag stays default-`false` so an explicit per-property `HasMaxLength` still wins. `UseConstructorForRead` stays the `Configuration.Default` value (`true`), so a keyed value object reads back through its primary constructor, not its static factory.

Naming is one suite-wide policy, not a vocabulary. `Compose` applies `UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture)` directly — the explicit invariant culture is load-bearing because the `EFCore.NamingConventions` rewriters are culture-aware and an ambient Turkish-I locale fractures the schema fingerprint. The four other casings (`camel`/`lower`/`upper`/`upper-snake`) are not modeled: a second casing on a live schema is a full-rename migration, never a runtime row swap, so a five-row casing enum would model a day-zero constant as an axis nothing selects. A custom prefix scheme, were one ever needed, composes through one `INameRewriter` on the same convention seam, not a parallel options pass; hand-written provider naming patches are the deleted form.

Temporal columns cross through one typed converter, registered once. `TemporalCodec` is the frozen table over the `api-nodatime` value vocabulary; each row carries a parse-capable `IPattern<T>` and its PG store columns, and `Conventions(builder, nativeTemporal: false)` folds every row into `ModelConfigurationBuilder.Properties(row.ClrType).HaveConversion(row.ConverterType)` — `HaveConversion(Type)` taking the closed `PatternConverter<T>` type per `row.ClrType` — so a single `PatternConverter<T> : ValueConverter<T, string>` admits every temporal column of that type across every entity with zero per-column calls — the BOUNDARY_ADMISSION law applied to time. The fold is provider-gated: `ModelConfigurationBuilder` is provider-blind, so a model-wide `HaveConversion` to text would clobber the native pg mappings the plugin installs; the pg leg therefore passes `nativeTemporal: true` and the convention is a no-op there, while the sqlite and duckdb legs pass `false` and take the text converter — one fold, one flag, no per-provider converter class. `PatternConverter<T>` resolves its `IPattern<T>` once at type-close from the `TemporalCodec.Of<T>` index (the only boxing point, at the typed boundary, never per value) and round-trips through `IPattern<T>.Format`/`IPattern<T>.Parse`. The converter read path is the EF-forced statement seam — a parse failure at materialization is store corruption, not domain absence, so it throws through `ParseResult<T>.GetValueOrThrow()` inside the converter; the parse-as-rail surface is `ConverterRail.Parse<T>`, which lifts `ParseResult<T>.Success == false` into a typed `SchemaFault` for the call sites that read store text outside the model. The pg profile additionally rides the Npgsql NodaTime plugin (`Store/profiles#PROFILE_AXIS` `UseNodaTime()`) so a row's scalar `PgType` — `Instant`/`OffsetDateTime`/`ZonedDateTime` to `timestamptz`, `LocalDate`/`OffsetDate`/`AnnualDate`/`YearMonth` to `date`, `LocalTime` to `time`, `OffsetTime` to `timetz`, `Offset`/`Duration`/`Period` to `interval`, `Interval`/`DateInterval` to `tstzrange`/`daterange` — maps natively, and the text converter applies only on the sqlite leg through the same row, so a `Duration` p95 statistic, a `LocalDate` calendar column, and an `OffsetTime` wall-time column persist as native PG temporals and as ISO text under one vocabulary, never a BCL `DateTime` column.

The range rows carry the multirange store type too. The Npgsql NodaTime plugin maps `Interval`/`DateInterval` collections through `IntervalMultirangeMapping`/`DateIntervalMultirangeMapping` to `tstzmultirange`/`datemultirange`, so the two endpoint-pair rows carry a third `PgMultirange` column beside their scalar `PgType` and `PgRange`: a single validity window stores as a range, a disjoint validity set (the `RangeAgg`/`RangeIntersectAgg` time-travel algebra the `api-npgsql-ef-nodatime` `DbFunctions` projections build) stores as a multirange, both native on the pg leg and both slash-joined ISO pairs on the sqlite leg through the same `PatternConverter<Interval>`/`PatternConverter<DateInterval>`. A validity-window column forced to a scalar range when the domain carries a disjoint set is the deleted form, because intersecting two coverage sets on a scalar range loses every gap.

The pattern statics are exact. `Instant` rides `InstantPattern.ExtendedIso`, `LocalDate` `LocalDatePattern.Iso`, `LocalTime` `LocalTimePattern.ExtendedIso`, `LocalDateTime` `LocalDateTimePattern.ExtendedIso`, `OffsetDateTime` `OffsetDateTimePattern.ExtendedIso`, `OffsetDate` `OffsetDatePattern.GeneralIso`, `OffsetTime` `OffsetTimePattern.ExtendedIso`, `Offset` `OffsetPattern.GeneralInvariantWithZ`, `Duration` `DurationPattern.Roundtrip` (sign- and sub-second-preserving "o"), `Period` `PeriodPattern.Roundtrip` (unit-preserving), `AnnualDate` `AnnualDatePattern.Iso`, and `YearMonth` `YearMonthPattern.Iso` — all parse-capable invariant singletons. `ZonedDateTime` is the one type whose ISO statics (`GeneralFormatOnlyIso`/`ExtendedFormatOnlyIso`) cannot read back, so its row rides `ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z '('o<g>')'", DateTimeZoneProviders.Tzdb)` — the embedded `z '('o<g>')'` zone-plus-offset is what makes the round-trip unambiguous across DST transitions, where a bare `z` parse is the deleted ambiguous form. `Interval` and `DateInterval` have no NodaTime text pattern, so their rows compose an endpoint pair — the `Interval` row formats `{Start}/{End}` from `InstantPattern.ExtendedIso` and the `DateInterval` row from `LocalDatePattern.Iso` — so an interval persists as a slash-joined ISO pair on the sqlite leg while the pg leg rides the native range mapping, and a single-instant sentinel standing in for a window is the deleted form.

Concurrency tokens resolve the profile's string column once. `StoreProfile.ConcurrencyToken` is a `string` (`xmin`/`version`/`none`), and `ConcurrencyToken` is the `[SmartEnum<string>]` whose generated `Get` is the one resolution site — `ConverterRail.Token(profile.ConcurrencyToken, entity)` parses that string to its row and binds it, so the string-to-shape mapping lives here once rather than re-spelled at every entity config, and an unknown token surfaces as the smart-enum's `UnknownSmartEnumIdentifierException` (a `KeyNotFoundException`) at the model-build boundary, not a silent miss. The `xmin` row binds the system column through `entity.Property<uint>("xmin").IsRowVersion().HasColumnName("xmin")` — the Npgsql `xid`/`xmin` row-version convention recognizes a `uint` `OnAddOrUpdate` concurrency token and binds the system column, so no first-party `UseXminAsConcurrencyToken` extension is required (none exists in the provider). The `version` row maps an `int` integer column configured `.IsConcurrencyToken().IsRowVersion()` bumped per write. The `none` row is the entity-config no-op for snapshot and blob profiles. The token is purely an entity-level configuration concern — it touches `EntityTypeBuilder`, never `DbContextOptionsBuilder`, so there is no options leg; `Bind` dispatches the three postures through one generated `Switch`, never a hand-coded per-context branch, and the provider-exception fault projection that lifts a violated token into `StoreFault.Concurrency` belongs to `Query/transaction#SQLSTATE_CLASSIFIER`, which reads the same `StoreProfile.ConcurrencyToken` string.

The compiled-model freeze is drop-in. `Compose(options, compiled)` feeds `compiled` — the `dotnet ef dbcontext optimize` codegen output — to `UseModel` on the same naming-plus-Thinktecture tail, and `Schema/migration#MIGRATION_LAW` calls exactly this two-arg form to mount the frozen model. The codegen's internal `IModelRuntimeInitializer.Initialize` bakes the invariant snake-case rewrites and every admitted converter into the frozen model before emission, so a fresh-built model and a compiled model emit byte-identical column names and migration SQL, identical `ModelSnapshot` bytes, and one fingerprint: the fast path is drop-in beside the convention with no dual naming path, no post-compile fixup, and no per-column rename, and a hand-applied naming patch on the compiled model is the deleted form.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------

// Endpoint-pair IPattern for the two range types NodaTime gives no text pattern: `{Start}/{End}`.
internal sealed class PairPattern<T, TPart>(IPattern<TPart> part, Func<T, (TPart Start, TPart End)> split, Func<TPart, TPart, T> join) : IPattern<T> {
    public string Format(T value) => split(value) is var (start, end) ? $"{part.Format(start)}/{part.Format(end)}" : string.Empty;

    public ParseResult<T> Parse(string text) =>
        text.Split('/', 2) is [var lhs, var rhs] && part.Parse(lhs) is { Success: true } start && part.Parse(rhs) is { Success: true } end
            ? ParseResult<T>.ForValue(join(start.Value, end.Value))
            : ParseResult<T>.ForException(() => new FormatException($"<interval-pair:{text}>"));

    public StringBuilder AppendFormat(T value, StringBuilder builder) => builder.Append(Format(value));
}

// --- [MODELS] ----------------------------------------------------------------------------

[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class ConcurrencyToken {
    public static readonly ConcurrencyToken Xmin = new("xmin");
    public static readonly ConcurrencyToken Version = new("version");
    public static readonly ConcurrencyToken None = new("none");

    // Entity-level only — the token touches `EntityTypeBuilder`, never the options builder. The
    // generated `Get(profile.ConcurrencyToken)` is the one string-to-row resolution site.
    public EntityTypeBuilder Bind(EntityTypeBuilder entity) => Switch(
        state: entity,
        xmin: static e => { e.Property<uint>("xmin").IsRowVersion().HasColumnName("xmin"); return e; },
        version: static e => { e.Property<int>("version").IsConcurrencyToken().IsRowVersion(); return e; },
        none: static e => e);
}

[SmartEnum<string>]
[ValidationError<SchemaFault>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class TemporalCodec {
    public static readonly TemporalCodec Instant = new("instant", typeof(Instant), InstantPattern.ExtendedIso, "timestamptz");
    public static readonly TemporalCodec LocalDate = new("local-date", typeof(LocalDate), LocalDatePattern.Iso, "date");
    public static readonly TemporalCodec LocalTime = new("local-time", typeof(LocalTime), LocalTimePattern.ExtendedIso, "time");
    public static readonly TemporalCodec LocalDateTime = new("local-date-time", typeof(LocalDateTime), LocalDateTimePattern.ExtendedIso, "timestamp");
    public static readonly TemporalCodec OffsetDateTime = new("offset-date-time", typeof(OffsetDateTime), OffsetDateTimePattern.ExtendedIso, "timestamptz");
    public static readonly TemporalCodec OffsetDate = new("offset-date", typeof(OffsetDate), OffsetDatePattern.GeneralIso, "date");
    public static readonly TemporalCodec OffsetTime = new("offset-time", typeof(OffsetTime), OffsetTimePattern.ExtendedIso, "timetz");
    public static readonly TemporalCodec ZonedDateTime = new("zoned-date-time", typeof(ZonedDateTime),
        ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z '('o<g>')'", DateTimeZoneProviders.Tzdb), "timestamptz");
    public static readonly TemporalCodec Offset = new("offset", typeof(Offset), OffsetPattern.GeneralInvariantWithZ, "interval");
    public static readonly TemporalCodec Duration = new("duration", typeof(Duration), DurationPattern.Roundtrip, "interval");
    public static readonly TemporalCodec Period = new("period", typeof(Period), PeriodPattern.Roundtrip, "interval");
    public static readonly TemporalCodec AnnualDate = new("annual-date", typeof(AnnualDate), AnnualDatePattern.Iso, "date");
    public static readonly TemporalCodec YearMonth = new("year-month", typeof(YearMonth), YearMonthPattern.Iso, "date");
    public static readonly TemporalCodec Interval = new("interval", typeof(Interval),
        PairFor(InstantPattern.ExtendedIso, static i => (i.Start, i.End), static (s, e) => new Interval(s, e)), "tstzrange", "tstzrange", "tstzmultirange");
    public static readonly TemporalCodec DateInterval = new("date-interval", typeof(DateInterval),
        PairFor(LocalDatePattern.Iso, static d => (d.Start, d.End), static (s, e) => new DateInterval(s, e)), "daterange", "daterange", "datemultirange");

    // Derived index projects from `Items` through a lazy accessor, never an eager static initializer
    // that races the generated `Items` materialization.
    private static readonly Lazy<FrozenDictionary<Type, TemporalCodec>> ByClr =
        new(static () => Items.ToFrozenDictionary(static codec => codec.ClrType));

    private readonly object pattern;

    public Type ClrType { get; }
    public string PgType { get; }
    // Range/multirange store types are present only on the two endpoint-pair rows; scalar rows leave them None.
    public Option<string> PgRange { get; }
    public Option<string> PgMultirange { get; }
    public Type ConverterType => typeof(PatternConverter<>).MakeGenericType(ClrType);

    private TemporalCodec(string key, Type clrType, object pattern, string pgType, string? pgRange = null, string? pgMultirange = null) : this(key) =>
        (ClrType, this.pattern, PgType, PgRange, PgMultirange) = (clrType, pattern, pgType, Optional(pgRange), Optional(pgMultirange));

    public IPattern<T> Pattern<T>() => (IPattern<T>)pattern;

    public static TemporalCodec Of<T>() => ByClr.Value[typeof(T)];

    public static Fin<TemporalCodec> Of(Type clr) =>
        ByClr.Value.TryGetValue(clr, out var codec) ? Fin.Succ(codec) : Fin.Fail<TemporalCodec>(SchemaFault.Create($"<no-codec:{clr.Name}>"));

    private static IPattern<T> PairFor<T, TPart>(IPattern<TPart> part, Func<T, (TPart, TPart)> split, Func<TPart, TPart, T> join) =>
        new PairPattern<T, TPart>(part, split, join);
}

// The read path is the EF-forced statement seam: a parse failure at materialization is store
// corruption, so it throws; `ConverterRail.Parse<T>` is the rail-shaped sibling for non-model reads.
public sealed class PatternConverter<T>() : ValueConverter<T, string>(
    static value => Pattern.Format(value),
    static text => Pattern.Parse(text).GetValueOrThrow()) {
    private static readonly IPattern<T> Pattern = TemporalCodec.Of<T>().Pattern<T>();
}

// --- [COMPOSITION] -----------------------------------------------------------------------

public static class ConverterRail {
    private const int KeyWidth = 64;

    // `Configuration` is a sealed class with init-only members and no record `with`: object initializer only.
    private static readonly Configuration ConverterAdmission = new() {
        SmartEnums = SmartEnumConfiguration.Default,
        KeyedValueObjects = new KeyedValueObjectConfiguration { MaxLengthStrategy = new FixedKeyedValueObjectMaxLengthStrategy(KeyWidth) },
    };

    // Snake-case is one suite-wide day-zero policy; the invariant culture pins the rewriter against an ambient Turkish-I.
    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options) =>
        options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture).UseThinktectureValueConverters(ConverterAdmission);

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled) =>
        Compose(options).UseModel(compiled);

    // `ModelConfigurationBuilder` is provider-blind, so the text converter is folded ONLY on legs without
    // native NodaTime mapping (sqlite/duckdb). On pg the plugin owns timestamptz/date/interval/range natively
    // and a model-wide `HaveConversion` to text would clobber it — so the pg leg passes `nativeTemporal: true`.
    public static ModelConfigurationBuilder Conventions(ModelConfigurationBuilder conventions, bool nativeTemporal) =>
        nativeTemporal
            ? conventions
            : TemporalCodec.Items.Fold(conventions, static (builder, codec) => {
                builder.Properties(codec.ClrType).HaveConversion(codec.ConverterType);
                return builder;
            });

    // The profile carries `ConcurrencyToken` as a string; `Get` is the one resolution to the row.
    public static EntityTypeBuilder Token(string token, EntityTypeBuilder entity) => ConcurrencyToken.Get(token).Bind(entity);

    public static string Format<T>(T value) => TemporalCodec.Of<T>().Pattern<T>().Format(value);

    public static Fin<T> Parse<T>(string text) =>
        TemporalCodec.Of(typeof(T)).Bind(codec =>
            codec.Pattern<T>().Parse(text) is { Success: true } parsed
                ? Fin.Succ(parsed.Value)
                : Fin.Fail<T>(SchemaFault.Create($"<temporal-parse:{typeof(T).Name}:{text}>")));
}
```
