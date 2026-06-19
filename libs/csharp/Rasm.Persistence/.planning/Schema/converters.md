# [PERSISTENCE_SCHEMA_CONVERTERS]

Converter and naming registration for every store the suite opens. `ConverterRail` is the single registration row admitting every generated domain owner, the snake-case naming policy, the frozen NodaTime sqlite pattern table, and the `xmin`/integer concurrency token. The page spine is the two EF Core providers, EFCore.NamingConventions, Thinktecture.Runtime.Extensions.EntityFrameworkCore10, and the NodaTime provider plugin — every converter mounts once at model build, never per entity.

## [01]-[INDEX]

- [01]-[CONVERTER_RAIL]: converter and naming registration, sqlite pattern table, concurrency token, and compiled mount.

## [02]-[CONVERTER_RAIL]

- Owner: `ConverterRail` static composition surface owning the converter and naming registration, the frozen `SqlitePatterns` NodaTime round-trip table, the concurrency-token rows, and the compiled-model mount.
- Entry: `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options)` is the one registration row every profile's options delegate folds in; `public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled)` mounts the frozen model; `public static string SqliteText<T>(T value)` and `public static Fin<T> SqliteValue<T>(string text)` round-trip a temporal CLR value through the keyed pattern table.
- Auto: `ThinktectureValueConverterFactory` converters cover every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column behind `UseThinktectureValueConverters` — zero hand-written converter classes; a single declared property converts through `HasThinktectureValueConverter`, a complex-type property through `ComplexTypePropertyBuilderExtensions`, and a primitive-collection element through `PrimitiveCollectionBuilderExtensions`, so a per-column conversion is one builder call, never a converter class.
- Packages: Thinktecture.Runtime.Extensions.EntityFrameworkCore10, EFCore.NamingConventions, Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime, NodaTime, LanguageExt.Core
- Growth: one policy value per naming override; a new generated domain owner costs zero converter code on the same registration row; a sqlite temporal column is one `SqlitePatterns` row; zero new surface.
- Boundary: `UseSnakeCaseNamingConvention` is the single naming policy — the `CamelCase`, `LowerCase`, `UpperCase`, and `UpperSnakeCase` rewriters are the named rejected conventions because a second casing fractures the schema fingerprint, and hand-written provider naming patches and per-entity converter classes are the deleted patterns. Key-column width rides the converter registration's `Configuration` value — `SmartEnumConfiguration` bounds smart-enum columns and `KeyedValueObjectConfiguration` bounds keyed value-object columns to a declared max-length and `Configuration.NoMaxLength` is the rejected unbounded form; `Configuration.Default` is the bounded selection. Pg temporal columns ride the profile row's `UseNodaTime` option mapping `Instant` to `timestamptz`, `LocalDate` to `date`, `LocalTime` to `time`, `OffsetTime` to `timetz`, `Duration`/`Period` to `interval`, and `Interval`/`DateInterval` to range/multirange; sqlite temporal columns persist NodaTime pattern text under the same convention so no `DateTime` sentinel reaches a store — `SqlitePatterns` is the frozen pattern table the sqlite converter rows trace to, keyed by CLR type, so a `Duration` median or p95 statistic rides `DurationPattern.Roundtrip` (the round-tripping "o" pattern preserving sign and sub-second), a date-only column rides `LocalDatePattern.Iso`, a time-only column rides `LocalTimePattern.ExtendedIso` (full sub-second precision), instants ride `InstantPattern.ExtendedIso`, local timestamps ride `LocalDateTimePattern.ExtendedIso`, offset timestamps ride `OffsetDateTimePattern.ExtendedIso`, and a period column rides `PeriodPattern.Roundtrip` (unit-preserving round-trip), so each temporal column round-trips as ISO text rather than fall back to a BCL `DateTime` column; `ZonedDateTime` is the one type whose pattern statics are format-only, so a zoned sqlite column rides `ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z", DateTimeZoneProviders.Tzdb)` to be parse-capable — the bare `GeneralFormatOnlyIso`/`ExtendedFormatOnlyIso` statics cannot read back. A hand-written `DateTime`-typed sqlite temporal column is the deleted form. Concurrency tokens are schema facts declared here — the pg row maps the system `xmin` column through `UseXminAsConcurrencyToken()` and the sqlite row carries an integer version column bumped per write — while the provider-exception fault projection belongs to the query rail. The compiled-model mount is the two-argument `Compose(options, compiled)` overload feeding `UseModel`, where `compiled` is the `dotnet ef dbcontext optimize` codegen output whose internal `IModelRuntimeInitializer.Initialize` step bakes the `UseSnakeCaseNamingConvention` rewrites into the frozen model before emission, so a fresh-built model and a compiled model emit byte-identical column names and migration SQL — the compiled-model fast path is drop-in beside the convention with no dual naming path, no post-compile fixup, and no per-column rename, and a hand-applied naming patch on the compiled model is the deleted form.

```csharp signature
public static class ConverterRail {
    public static readonly FrozenDictionary<Type, IPattern<object>> SqlitePatterns = new Dictionary<Type, IPattern<object>> {
        [typeof(Instant)] = Boxed(InstantPattern.ExtendedIso),
        [typeof(LocalDate)] = Boxed(LocalDatePattern.Iso),
        [typeof(LocalTime)] = Boxed(LocalTimePattern.ExtendedIso),
        [typeof(LocalDateTime)] = Boxed(LocalDateTimePattern.ExtendedIso),
        [typeof(OffsetDateTime)] = Boxed(OffsetDateTimePattern.ExtendedIso),
        [typeof(ZonedDateTime)] = Boxed(ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z", DateTimeZoneProviders.Tzdb)),
        [typeof(Duration)] = Boxed(DurationPattern.Roundtrip),
        [typeof(Period)] = Boxed(PeriodPattern.Roundtrip),
    }.ToFrozenDictionary();

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options) =>
        options.UseSnakeCaseNamingConvention().UseThinktectureValueConverters(Configuration.Default);

    public static DbContextOptionsBuilder Compose(DbContextOptionsBuilder options, IModel compiled) =>
        Compose(options).UseModel(compiled);

    public static string SqliteText<T>(T value) where T : struct =>
        SqlitePatterns[typeof(T)].Format(value);

    public static Fin<T> SqliteValue<T>(string text) where T : struct =>
        SqlitePatterns[typeof(T)].Parse(text) is { Success: true, Value: T parsed }
            ? Fin.Succ(parsed)
            : Fin.Fail<T>(SchemaFault.Create($"<temporal-parse:{typeof(T).Name}:{text}>"));

    private static IPattern<object> Boxed<T>(IPattern<T> pattern) =>
        new BoxedPattern<T>(pattern);

    private sealed record BoxedPattern<T>(IPattern<T> Inner) : IPattern<object> {
        public ParseResult<object> Parse(string text) => Inner.Parse(text) switch {
            { Success: true, Value: T value } => ParseResult<object>.ForValue(value!),
            var failed => ParseResult<object>.ForException(() => failed.Exception),
        };
        public string Format(object value) => Inner.Format((T)value);
        public StringBuilder AppendFormat(object value, StringBuilder builder) => Inner.AppendFormat((T)value, builder);
    }
}
```

