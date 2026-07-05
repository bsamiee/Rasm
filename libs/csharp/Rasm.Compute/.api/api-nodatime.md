# [RASM_COMPUTE_API_NODATIME]

`NodaTime` supplies semantic instants, local calendar values, offsets, zones, periods,
durations, intervals, clocks, resolvers, and text patterns for AppHost receipts,
health snapshots, support windows, and persisted clock facts. Version `3.3.2` is
Apache-2.0 and ships `lib/net8.0`+`lib/netstandard2.0`; the workspace `net10.0`
consumer binds `lib/net8.0`, which is the build that carries the `NodaTime.HighPerformance`
`Instant64`/`Duration64` `long`-backed tier and the generic-math operator interfaces
(`IAdditionOperators`/`IComparisonOperators`/`IMinMaxValue`) those compact types implement.
The Protobuf and System.Text.Json wire seams live in the sibling `NodaTime.Serialization.*`
packages (`.api/api-nodatime-protobuf.md`); this overlay narrows the core temporal owner to the Compute delta; the substrate canonical member catalog is `libs/csharp/.api/api-nodatime.md`.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-nodatime.md`
- the time-value/zone/clock/pattern type rosters and the instant, resolver, and text call-shape tables live on the substrate catalog — this overlay never re-states them
- rail: time

## [02]-[COMPUTE_BINDINGS]

[COMPUTE_BINDINGS]:
- `Instant.Plus(Duration)`/`Minus(Duration)` (operator `+`/`-` twins) own the hot-path deadline math interior to the receipt boundary; `Instant.MaxValue` is the never-expire latch the `Model/sessions` idle-unload sweep reads, and `Duration.Zero` the identity elapsed span (zero delay / no-op TTL).
- `Instant.ToDateTimeUtc()` → `DateTime` (`Kind=Utc`) is the one BCL crossing at the gRPC `CallOptions.WithDeadline` edge (`Runtime/transport#CALL_POLICY`); the semantic `Instant` stays the interior truth on both sides of the call.
- `NodaTime.HighPerformance.Instant64`/`Duration64` — `long`-tick value types bridged through `Instant64.FromInstant`/`.ToInstant()` and `Duration64.FromDuration`/`.ToDuration()`, implementing the generic-math operator interfaces — keep a hot-path accumulation in the compact tier and cross to `Instant`/`Duration` only at the receipt boundary; the compact tier is an interior arithmetic accelerator, never the wire shape.
- `Provenance.At` age (the `Analysis/assessment` bounded-retry backoff) is `clock.GetCurrentInstant() - row.Provenance.At` — `Duration` comparison against the `RetryPolicy` backoff schedule, never a `DateTime` subtraction.

## [03]-[IMPLEMENTATION_LAW]


[TIME_TOPOLOGY]:
- namespaces: `NodaTime`, `NodaTime.Text`, `NodaTime.Text.Patterns`, `NodaTime.TimeZones`, `NodaTime.TimeZones.Cldr`, `NodaTime.Xml`, `NodaTime.HighPerformance`
- instant types: `Instant`, `OffsetDateTime`, `ZonedDateTime`
- local types: `LocalDate`, `LocalTime`, `LocalDateTime`, `AnnualDate`
- calendar types: `CalendarSystem`, `YearMonth`, `IsoDayOfWeek`, calendar eras
- duration types: `Duration`, `Period`, `PeriodBuilder`, `DateInterval`, `Interval`
- zone providers: `DateTimeZoneProviders.Tzdb`, `DateTimeZoneProviders.Bcl`
- zone mapping: strict, lenient, explicit resolver, ambiguous mapping, skipped mapping
- text patterns: `InstantPattern`, local-date patterns, zoned-date-time patterns
- interop surfaces: BCL conversion extensions, `NodaTime.Xml.XmlSerializationSettings`, `NodaTime.Text.TypeConverterSettings` (both expose a `DateTimeZoneProvider` admission seam)
- compact surfaces: `NodaTime.HighPerformance.Instant64`/`Duration64` — `long`-tick value types bridged to the semantic tier through `Instant64.FromInstant`/`.ToInstant()` and `Duration64.FromDuration`/`.ToDuration()`; they implement the generic-math operator interfaces, so a hot-path accumulation stays in the compact tier and crosses to `Instant`/`Duration` only at the receipt boundary. The semantic `Instant`/`Duration` stay the persisted and exported truth; the compact tier is an interior arithmetic accelerator, never the wire shape.

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
