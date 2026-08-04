# [CSHARP_TESTING_API_NODATIME_TESTING]

`NodaTime.Testing` ships the semantic-time doubles NodaTime itself declares no fake for: `FakeClock` substitutes `IClock` under programmable advance, the zone family scripts DST transitions a tzdb lookup cannot produce on demand, and the construction extensions spell `Duration` and `LocalDate` literals inline. It pairs with `timeprovider-testing.md` rather than competing: `FakeTimeProvider` drives the BCL monotonic and timer plane, `FakeClock` drives the `Instant` a `ZonedClock` reads, and one spec composes both off a single policy record.

## [01]-[PACKAGE_SURFACE]

- package: `NodaTime.Testing`
- license: `Apache-2.0`
- namespaces: `NodaTime.Testing`, `NodaTime.Testing.Extensions`, `NodaTime.Testing.TimeZones`
- asset: `lib/netstandard2.0/NodaTime.Testing.dll` — the sole shipped target framework; every consumer TFM binds it
- rail: evidence — deterministic semantic time, scripted zone transitions, and literal temporal construction; a suite-owned harness row (`PrivateAssets="all"`), never centrally injected

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                       | [KIND]    | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :-------- | :------------------------------------------------------------------------ |
|  [01]   | `FakeClock`                    | clock     | `IClock` under manual advance and per-read auto-advance; thread-safe      |
|  [02]   | `SingleTransitionDateTimeZone` | zone      | one-transition `DateTimeZone`; early and late intervals exposed           |
|  [03]   | `MultiTransitionDateTimeZone`  | zone      | scripted transition sequence; interval and transition collections exposed |
|  [04]   | `FakeDateTimeZoneSource`       | provider  | builder-defined `IDateTimeZoneSource` over caller-supplied zones          |
|  [05]   | `DurationConstruction`         | extension | `Duration` literals off numeric receivers across every unit               |
|  [06]   | `LocalDateConstruction`        | extension | `LocalDate` literals off a day receiver, one member per calendar month    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: clock control — `NodaTime.Testing`

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `new FakeClock(Instant)`                                                      | ctor     | start instant, auto-advance zero                               |
|  [02]   | `new FakeClock(Instant, Duration)`                                            | ctor     | start instant with a per-read auto-advance                     |
|  [03]   | `FakeClock.FromUtc(int, int, int)`                                            | static   | midnight UTC of the given ISO year, month, day                 |
|  [04]   | `FakeClock.FromUtc(int, int, int, int, int, int)`                             | static   | the given UTC date and time of day in the ISO calendar         |
|  [05]   | `Advance(Duration)`                                                           | instance | moves the clock by a duration; negative values move it back    |
|  [06]   | `AdvanceNanoseconds(long)` … `AdvanceDays(int)`                               | instance | unit wrappers over `Advance`; days are standard 24-hour days   |
|  [07]   | `Reset(Instant)`                                                              | instance | rebases the clock; `AutoAdvance` survives unchanged            |
|  [08]   | `AutoAdvance { get; set; }`                                                   | property | every `GetCurrentInstant()` advances by this; defaults to zero |
|  [09]   | `GetCurrentInstant()`                                                         | instance | the read; repeated calls return one value until time is moved  |

[ENTRYPOINT_SCOPE]: scripted zones — `NodaTime.Testing.TimeZones`

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `new SingleTransitionDateTimeZone(Instant, int, int)`               | ctor     | transition point plus whole-hour offsets           |
|  [02]   | `new SingleTransitionDateTimeZone(Instant, Offset, Offset)`         | ctor     | transition point plus explicit offsets             |
|  [03]   | `new SingleTransitionDateTimeZone(Instant, Offset, Offset, string)` | ctor     | the same with a caller-chosen zone id              |
|  [04]   | `SingleTransitionDateTimeZone.EarlyInterval` / `.LateInterval`      | property | the two `ZoneInterval` sides of the transition     |
|  [05]   | `SingleTransitionDateTimeZone.Transition`                           | property | the transition instant — the early interval's end  |
|  [06]   | `new MultiTransitionDateTimeZone.Builder(...)`                      | ctor     | first offset, first saving offset, and first name  |
|  [07]   | `Builder.Add(Instant, int[, int[, string]])`                        | instance | appends one transition; standard, saving, and name |
|  [08]   | `Builder.Build()`                                                   | instance | mints the zone                                     |
|  [09]   | `MultiTransitionDateTimeZone.Intervals` / `.Transitions`            | property | the ordered interval and instant collections       |
|  [10]   | `GetZoneInterval(Instant)`                                          | instance | the `DateTimeZone` override both zones carry       |
|  [11]   | `new FakeDateTimeZoneSource.Builder()`                              | ctor     | opens the source; `Zones`/`BclIdsToZoneIds` mutate |
|  [12]   | `Builder.Add(DateTimeZone)` then `Builder.Build()`                  | instance | seats a zone, then mints the source                |
|  [13]   | `FakeDateTimeZoneSource.ToProvider()`                               | instance | wraps the source as an `IDateTimeZoneProvider`     |
|  [14]   | `ForId(string)` / `GetIds()` / `GetSystemDefaultId()`               | instance | the `IDateTimeZoneSource` resolution surface       |

[ENTRYPOINT_SCOPE]: literal construction — `NodaTime.Testing.Extensions`, `DurationConstruction` and `LocalDateConstruction` extending their numeric and day receivers

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `Days\|Hours(int\|double)`                             | static  | `Duration` off a whole-unit count     |
|  [02]   | `Minutes\|Seconds\|Milliseconds\|Ticks\|Nanoseconds(int\|long\|double)` | static | `Duration` off a sub-hour count |
|  [03]   | `January(int day, int year)` … `December`              | static  | `LocalDate`, one member per ISO month |

## [04]-[IMPLEMENTATION_LAW]

[DETERMINISM]: the clock moves only through `Advance*`, `Reset`, and `AutoAdvance`, so a temporal proof is a pure function of the advance sequence. `AutoAdvance` makes the read itself the mutation — a spec asserting over repeated reads states the expected drift or leaves the property at zero.

[STACKING]:
- `NodaTime` (`libs/csharp/.api/api-nodatime.md`): `FakeClock` implements `IClock.GetCurrentInstant()` and substitutes `SystemClock.Instance`; `FakeDateTimeZoneSource.ToProvider()` substitutes `DateTimeZoneProviders.Tzdb`, so a DST-straddling receipt window proves against a scripted transition rather than tzdb contents.
- `timeprovider-testing.md`: `FakeTimeProvider` owns the BCL `TimeProvider` plane — monotonic marks, elapsed pairs, and timer firing — and `FakeClock` owns the semantic `Instant`; a spec advancing one and reading the other proves nothing, so paired advances move both.
- `xunit.v3` (`xunit-v3.md`): plain construction inside `[Fact]` bodies; the doubles hold no fixture requirement and no disposal contract.

[LOCAL_ADMISSION]:
- Zone-sensitive proofs resolve against a scripted `SingleTransitionDateTimeZone` or `MultiTransitionDateTimeZone`, never against tzdb contents a data release re-bases.
- Production surfaces read time through injected `IClock` and `TimeProvider` contracts; a test-only clock type reaching production is the named defect.
- Construction extensions serve spec literals alone — production duration and date construction stays on the NodaTime factories.

[RAIL_LAW]:
- Package: `NodaTime.Testing`
- Owns: programmable semantic time, scripted zone transitions, and literal temporal construction inside C# specs.
- Accept: injected `FakeClock` with explicit advances; builder-defined zone sources; extension literals in spec bodies.
- Reject: tzdb-dependent zone assertions, wall-clock reads, a second fake-clock implementation, or advancing one time plane while asserting the other.
