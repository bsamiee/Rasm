# [RASM_APPHOST_API_NODATIME_STJ]

Full surface and stacking: `libs/csharp/.api/api-nodatime-stj.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[SETTINGS_LIFECYCLE]:

- `NodaJsonSettings` is mutable, not thread-safe. Construct, optionally mutate per-type converter properties (set a property to `null` to suppress that type), call `ConfigureForNodaTime`, discard.
- Default constructor binds `DateTimeZoneProviders.Tzdb`; the provider overload accepts any `IDateTimeZoneProvider`.
- `AddConverters` insertion order: Instant, Interval, LocalDate, LocalDateTime, LocalTime, AnnualDate, YearMonth, DateInterval, Offset, DateTimeZone, Duration, Period, OffsetDateTime, OffsetDate, OffsetTime, ZonedDateTime. First converter wins per type; no double-registration.

[CONVERTER_BASE_HIERARCHY]:

- `NodaConverterBase<T>` is the base for all built-in converters; it handles nullable-struct bridging, `CanConvert` (sealed-class vs. assignable check), and `ReadAsPropertyName` delegation.
- `NodaPatternConverter<T>` extends the base with an `IPattern<T>` and an optional post-parse `Action<T>` validator (used for ISO calendar enforcement on `OffsetDate`, `OffsetDateTime`, `ZonedDateTime`).
- `DelegatingConverterBase<T>` is independent of `NodaConverterBase<T>`; use it when a `[JsonConverterAttribute]`-decorated property requires a distinct type that wraps a reusable singleton converter instance.

[PRECEDENCE_LAW]:

- STJ resolves options-level `Converters` list before consulting `TypeInfoResolver` metadata. First converter in the list that claims the type wins.
- In `SuiteContracts.Wire`: `ThinktectureJsonConverterFactory` is added first (factory, position 0); `ConfigureForNodaTime` appends NodaTime per-type converters after it.
- `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` declines types carrying `[JsonConverter]`; no NodaTime type carries that attribute.
- NodaTime converters own `Instant`, `LocalDate`, `LocalDateTime`, `LocalTime`, `AnnualDate`, `YearMonth`, `DateInterval`, `Offset`, `DateTimeZone`, `Duration`, `Period`, `OffsetDateTime`, `OffsetDate`, `OffsetTime`, `ZonedDateTime`, `Interval` — the converter list wins for each; the source-gen `JsonTypeInfo` for those types is never reached.
- Thinktecture factory owns all value-objects, smart-enums, and keyed-unions; NodaTime converters own the semantic-time types. The two factories partition the type space — no collision is possible.

[STACK]:

- wire merge: `SuiteContracts.Wire` is the one `JsonSerializerOptions` merge per app root; `ConfigureForNodaTime` appends the NodaTime per-type converters after the `ThinktectureJsonConverterFactory` at position 0, and the merged options carry every `[JsonSerializable]` context — a standalone options owner is the deleted form.
- clock seam: NodaTime owns the semantic-time currency (`Instant`/`Duration`/`ZonedDateTime`); every deadline and schedule crosses the wire through these converters, never a raw `DateTime`/`TimeSpan` — `ClockPolicy` produces the `Instant`, and this catalog serializes it.
- type partition: the Thinktecture factory owns value-objects/smart-enums/keyed-unions and NodaTime owns the semantic-time types; the two factories partition the type space in the one merge, so no third converter list is minted.

[RAIL_LAW]:

- Package: `NodaTime.Serialization.SystemTextJson`
- Owns: STJ converter registration for NodaTime semantic-time types
- Accept: `ConfigureForNodaTime` as the sole registration call; position after `TypeInfoResolver` binding in `Wire` is the precedence law
- Reject: manual converter list construction, hand-rolled NodaTime converters, duplicate registration alongside `ConfigureForNodaTime`, or a standalone options owner outside the `Wire` merge
