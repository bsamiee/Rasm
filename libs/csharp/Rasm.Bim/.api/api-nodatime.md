# [RASM_BIM_API_NODATIME]

Full surface and stacking: `libs/csharp/.api/api-nodatime.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.TimeZones`, `NodaTime.Extensions`, `NodaTime.Calendars`, `NodaTime.Xml`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- calendar types: `CalendarSystem`, `YearMonth`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit resolver, ambiguous mapping, skipped mapping
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns, `NodaTime.Text.CompositePatternBuilder` for format-dispatched multi-pattern parse
- interop surfaces: `NodaTime.Extensions` BCL/`TimeProvider` bridges (`ToClock`, `GetCurrentInstant`, `ToInstant`), `NodaTime.Xml` settings, `NodaTime.Text.TypeConverterSettings`
- wire adapters (separate packages, both manifest-pinned): `NodaTime.Serialization.SystemTextJson` (STJ converter registration) and `NodaTime.Serialization.Protobuf` (protobuf `Timestamp`/`Duration` mapping) — the value types here are authored canonical and serialized through these adapters, never a hand-rolled JSON/proto converter

[INTEGRATION_STACK]:
- clock seam: a single injected `System.TimeProvider` is the one elapsed-timing root; `TimeProviderExtensions.ToClock` projects it into a NodaTime `IClock` so receipt timestamps (`Instant`) and delay/timeout work share one fake-able clock under test — no parallel `SystemClock` direct read in domain code, no second injected clock type.
- wire seam: an `Instant`/`ZonedDateTime`/`Duration` authored canonical serializes through `NodaTime.Serialization.SystemTextJson` (registered once on the STJ options the Bim wire codec owns) and `NodaTime.Serialization.Protobuf` (the protobuf `Timestamp`/`Duration` field mapping), so the same receipt fact crosses the JSON and protobuf boundaries with identity-preserving round-trip rather than a per-boundary converter.
- hash seam: a persisted clock fact that participates in a snapshot fingerprint formats through an invariant `InstantPattern.ExtendedIso`/`PeriodPattern.Roundtrip` BEFORE the `System.IO.Hashing` `XxHash3.Append` so the identity hash is stable across cultures and machines — a culture-ambient `ToString()` does make the fingerprint non-deterministic.
- text seam: `CompositePatternBuilder` owns multi-format intake (e.g. an external feed mixing RFC-3339 and extended-ISO) as one format-dispatched parser, not a try/catch ladder over sibling `IPattern<T>` instances.

[LOCAL_ADMISSION]:
- Receipts store semantic instants, not local wall-clock values.
- Calendar and time-zone values stay explicit wherever persisted or exported.
- Local date-times require an explicit zone and resolver before they become instants.
- Text persistence uses invariant or roundtrip patterns, not culture-ambient formatting.
- Elapsed timing and delays remain `TimeProvider` work; calendar truth remains NodaTime work.

[RAIL_LAW]:
- Package: `NodaTime`
- Owns: semantic timestamps and calendar values
- Accept: receipts store instants and zones
- Reject: local DateTime vocabulary
