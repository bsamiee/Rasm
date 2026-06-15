# [APPHOST_TIME_AND_DEADLINES]

One temporal law serves the whole suite: `TimeProvider` owns elapsed measurement, NodaTime `IClock` owns semantic instants, and one injected `ClockPolicy` record pairs them — consumer capsules bind the pair at construction. `DeadlineClass` is the nine-row bound vocabulary that every duration literal in the four packages traces to, and `SchedulePort` is the suite's single scheduler — Cronos cron rows and fixed-period rows carry every scheduled concern, with maintenance-lease policy values deciding cross-process ownership. BCL temporal shapes cross only at the admission seam, receipts stamp `Instant` and `Duration`, and the test row swaps deterministic fakes through the same record.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                    |
| :-----: | ----------------- | ------------------------------------------------------------------------- |
|   [1]   | CLOCK_SPLIT       | One injected clock pair; elapsed versus semantic time; sentinel admission |
|   [2]   | DEADLINE_TAXONOMY | Nine deadline rows; every suite duration literal traces here              |
|   [3]   | SCHEDULE_PORT     | The suite scheduler; cron and period rows; lease values                   |

## [2]-[CLOCK_SPLIT]

- Owner: `ClockPolicy`
- Entry: `public static Option<Instant> Admit(DateTimeOffset raw)` — `Option<T>` carries absence; a platform sentinel never travels inward.
- Packages: NodaTime, Microsoft.Extensions.TimeProvider.Testing, NodaTime.Testing, BCL inbox
- Growth: a new foreign temporal representation lands as one policy value on `ClockPolicy` — an admission overload, a persisted text pattern, or an exported zone-projection formatter; zero new surface.
- Boundary: siblings receive `ClockPolicy` through composition and stamp TTL, retention, lease, and elapsed evidence from it — `DateTime.UtcNow`, `DateTime.Now`, and direct `Stopwatch` call sites are the deleted patterns; `InstantPattern.ExtendedIso` and `PeriodPattern.Roundtrip` are the only persisted temporal grammars, invariant-culture only; `Exported` is the one cross-boundary stamp formatter — `OffsetDateTimePattern.Rfc3339` carries the offset stamp and `ZonedDateTimePattern` carries the user-facing zoned stamp, bound once through `WithZoneProvider(DateTimeZoneProviders.Tzdb)` and `WithResolver(Resolvers.StrictResolver)` so a hand-built zoned formatter is the deleted pattern; `ZoneShift` reads the offset transition at an instant through `GetZoneInterval`, the only DST-evidence surface for receipt windows that straddle a transition; the test row constructs the same record from `FakeTimeProvider` and `FakeClock`, so `Advance`, `SetUtcNow`, `AutoAdvanceAmount`, and `FromUtc` drive schedule, drain, and retry specs deterministically with zero test-only production surface.

```csharp signature
public sealed record ClockPolicy(TimeProvider Time, IClock Clock) {
    public static readonly ClockPolicy System = new(TimeProvider.System, SystemClock.Instance);

    public Instant Now => Clock.GetCurrentInstant();

    public long Mark() => Time.GetTimestamp();

    public Duration Elapsed(long mark) => Time.GetElapsedTime(mark).ToDuration();

    public static Option<Instant> Admit(DateTimeOffset raw) =>
        raw == default ? None : Optional(raw.ToInstant());

    public static Option<Instant> Admit(DateTime raw) =>
        raw == DateTime.MinValue ? None : Optional(new DateTimeOffset(raw, TimeSpan.Zero).ToInstant());

    public static Interval Window(Instant at, Duration radius) => new(at - radius, at + radius);

    public static string Persisted(Instant value) => InstantPattern.ExtendedIso.Format(value);

    public static string Persisted(Period value) => PeriodPattern.Roundtrip.Format(value);

    private static readonly ZonedDateTimePattern Zoned =
        ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFFo<G> z", DateTimeZoneProviders.Tzdb)
            .WithZoneProvider(DateTimeZoneProviders.Tzdb)
            .WithResolver(Resolvers.StrictResolver);

    public static string Exported(Instant value, Offset offset) =>
        OffsetDateTimePattern.Rfc3339.Format(value.WithOffset(offset));

    public static string Exported(Instant value, DateTimeZone zone) =>
        Zoned.Format(value.InZone(zone));

    public static ZoneInterval ZoneShift(Instant at, DateTimeZone zone) => zone.GetZoneInterval(at);
}
```

## [3]-[DEADLINE_TAXONOMY]

- Owner: `DeadlineClass`
- Cases: startup, ready-probe, health-probe, drain-cooperative, drain-forced, hop-attempt, hop-total, support-window, cache-ttl
- Entry: `public DeadlineReceipt Receipt(DeadlineClass row, long mark, Option<Duration> allotted = default)` — pure value; the outcome derives from measurement, never from a caller flag.
- Receipt: `DeadlineReceipt` — class, allotted `Duration`, consumed `Duration`, outcome, `Instant` stamp.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new bound is one `DeadlineClass` row; profile variance stays one policy value through `Resolve`; zero new surface.
- Boundary: every duration bound in the suite traces to a row here or to a policy row on its owning page — a bare `TimeSpan` literal anywhere else is the named defect; profile variance enters `Resolve` as one override-table swap at the composition root, and consumers read the frozen table, never the raw rows; the cancellation spine, hop registry, drain conductor, and cache lanes consume these rows as values — drain-cooperative escalates to drain-forced and every other miss is forced; the cooperative allotment is the telemetry-flush budget — a ForceFlush during plugin unload runs inside drain-cooperative, and an overrun escalates through the `Escalation` arc to drain-forced, the terminal forced-flush bound past which the drain conductor abandons in-flight export, so the flush latency is one `escalatesTo` arc, never a separate timer.

```csharp signature
public sealed class TimeKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TimeKeyPolicy, string>]
[KeyMemberComparer<TimeKeyPolicy, string>]
public sealed partial class DeadlineClass {
    public static readonly DeadlineClass Startup = new("startup", allotted: Duration.FromSeconds(30), escalatesTo: null);
    public static readonly DeadlineClass ReadyProbe = new("ready-probe", allotted: Duration.FromSeconds(5), escalatesTo: null);
    public static readonly DeadlineClass HealthProbe = new("health-probe", allotted: Duration.FromSeconds(5), escalatesTo: null);
    public static readonly DeadlineClass DrainCooperative = new("drain-cooperative", allotted: Duration.FromSeconds(20), escalatesTo: "drain-forced");
    public static readonly DeadlineClass DrainForced = new("drain-forced", allotted: Duration.FromSeconds(5), escalatesTo: null);
    public static readonly DeadlineClass HopAttempt = new("hop-attempt", allotted: Duration.FromSeconds(10), escalatesTo: null);
    public static readonly DeadlineClass HopTotal = new("hop-total", allotted: Duration.FromSeconds(30), escalatesTo: null);
    public static readonly DeadlineClass SupportWindow = new("support-window", allotted: Duration.FromSeconds(120), escalatesTo: null);
    public static readonly DeadlineClass CacheTtl = new("cache-ttl", allotted: Duration.FromMinutes(5), escalatesTo: null);

    private readonly string? escalatesTo;

    public Duration Allotted { get; }

    public Option<DeadlineClass> Escalation =>
        Optional(escalatesTo).Bind(static key => TryGet(key, out var row) ? Optional(row) : None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TimeKeyPolicy, string>]
[KeyMemberComparer<TimeKeyPolicy, string>]
public sealed partial class DeadlineOutcome {
    public static readonly DeadlineOutcome Met = new("met");
    public static readonly DeadlineOutcome Escalated = new("escalated");
    public static readonly DeadlineOutcome Forced = new("forced");
}

public readonly record struct DeadlineReceipt(DeadlineClass Class, Duration Allotted, Duration Consumed, DeadlineOutcome Outcome, Instant At) {
    public static DeadlineReceipt From(DeadlineClass row, Duration allotted, Duration consumed, Instant at) =>
        new(
            Class: row,
            Allotted: allotted,
            Consumed: consumed,
            Outcome: consumed <= allotted ? DeadlineOutcome.Met
                : row.Escalation.IsSome ? DeadlineOutcome.Escalated
                : DeadlineOutcome.Forced,
            At: at);
}

public static class DeadlineOps {
    public static FrozenDictionary<DeadlineClass, Duration> Resolve(HashMap<DeadlineClass, Duration> overrides) =>
        DeadlineClass.Items.ToFrozenDictionary(static row => row, row => overrides.Find(row).IfNone(row.Allotted));

    extension(ClockPolicy clocks) {
        public DeadlineReceipt Receipt(DeadlineClass row, long mark, Option<Duration> allotted = default) =>
            DeadlineReceipt.From(row, allotted.IfNone(row.Allotted), clocks.Elapsed(mark), clocks.Now);
    }
}
```

## [4]-[SCHEDULE_PORT]

- Owner: `ScheduleEntry`
- Cases: `OccurrenceSpec.Cron(CronExpression Expression)` | `OccurrenceSpec.Every(Duration Period)` | `OccurrenceSpec.Annual(AnnualDate Date, LocalTime At, DateTimeZone Zone)`
- Entry: `public static IO<(Fin<Unit> Outcome, DeadlineReceipt Deadline)> Run(ClockPolicy clocks, ScheduleEntry entry, Option<Duration> allotted = default)` — `IO<T>` carries the deferred occurrence run; the work outcome rides `Fin<Unit>` beside the deadline receipt.
- Receipt: every occurrence run yields its `DeadlineReceipt` paired with the work outcome, and `Heartbeat` folds a not-met receipt into `SupportTrigger.WatchdogTimeout` carrying the firing `ScheduleEntry` — the watchdog signal the support owner consumes, composed from the receipt with no watchdog service type.
- Packages: Cronos, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new scheduled concern is one `ScheduleEntry` row registered by its consumer, and a new occurrence grammar is one case on `OccurrenceSpec`; zero new surface.
- Boundary: this port is the suite's only scheduler — per-package timer loops, host idle hooks, pg_cron, Quartz, Hangfire, and NCrontab are the deleted patterns; occurrence math consumes and emits UTC instants with zone projection confined inside the occurrence call; cron rows persist as expression text and rebuild through `TryParse` at composition, so `CronFormatException` never crosses the configuration boundary; second-resolution rows admit through `CronFormat.IncludeSeconds` and a fleet-spread cron row admits through the `YearlyWithJitter` template family, while `H` fields hash deterministically from the four-arg `TryParse` jitter-seed integer, so fleet spreading of a shared cron row is one schedule-key-derived seed value, never a hand-edited expression; the `Annual` case carries a calendar-recurring `AnnualDate` resolved through `InYear(...).At(...).InZoneStrictly` so a once-a-year rollup row maps its local wall-time to a UTC instant under the strict resolver, never a hand-built leap-day branch; `Missed` detects a skipped occurrence through `GetPreviousOccurrence` against the last-fired stamp, `Window` audits a bounded ascending backfill through `GetOccurrences`, and `WindowDescending` reads the most-recent-first audit through `GetOccurrencesDescending`, so a missed-occurrence sweep reads occurrence history rather than tracking a running counter; `Span` reports the calendar gap between two stamps through `Period.Between` over zoned local date-times — the only calendar-difference surface a retention or lease-age report reads, never a `Duration`-to-days division; `OccurrenceSpec.Identical` compares two specs through `CronExpression.Equals` on the cron arm so a reload that re-parses an unchanged expression text is recognized as the same schedule and never re-registers; lease release has two distinct values — handoff-on-drain releases immediately on the drain conductor's signal, crash-reclaim waits `CrashStaleness` past the holder's last stamp, and `CrashStaleness` exceeds the drain-cooperative plus drain-forced sum so a draining holder is never reclaimed mid-drain; a watchdog is a heartbeat row plus a deadline class, never a service type.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OccurrenceSpec {
    private OccurrenceSpec() { }

    public sealed record Cron(CronExpression Expression) : OccurrenceSpec;

    public sealed record Every(Duration Period) : OccurrenceSpec;

    public sealed record Annual(AnnualDate Date, LocalTime At, DateTimeZone Zone) : OccurrenceSpec;

    public static Fin<OccurrenceSpec> Admit(string expression, CronFormat format, Option<int> jitterSeed = default) =>
        (jitterSeed is { IsSome: true, Case: int seed }
            ? CronExpression.TryParse(expression, format, seed, out var parsed)
            : CronExpression.TryParse(expression, format, out parsed))
            ? Fin.Succ<OccurrenceSpec>(new Cron(parsed!))
            : Fin.Fail<OccurrenceSpec>(Error.New($"<invalid-cron:{expression}>"));

    public bool Identical(OccurrenceSpec other) =>
        (this, other) switch {
            (Cron a, Cron b) => a.Expression.Equals(b.Expression),
            (Every a, Every b) => a.Period == b.Period,
            (Annual a, Annual b) => a.Date == b.Date && a.At == b.At && a.Zone.Equals(b.Zone),
            _ => false,
        };
}

public sealed record LeasePolicy(Duration CrashStaleness) {
    public static readonly LeasePolicy Maintenance = new(CrashStaleness: Duration.FromSeconds(120));
}

public sealed record ScheduleEntry(
    string Key,
    OccurrenceSpec Spec,
    DeadlineClass Deadline,
    Option<LeasePolicy> Lease,
    Func<IO<Unit>> Work);

public static class SchedulePort {
    public static Option<Instant> Next(ScheduleEntry entry, Instant after) =>
        entry.Spec.Switch(
            cron:  static (s, c) => Optional(c.Expression.GetNextOccurrence(s.ToDateTimeOffset(), TimeZoneInfo.Utc))
                .Map(static next => next.ToInstant()),
            every: static (s, e) => Optional(s + e.Period),
            annual: static (s, a) => AnnualNext(s, a.Date, a.At, a.Zone),
            state: after);

    public static IO<(Fin<Unit> Outcome, DeadlineReceipt Deadline)> Run(ClockPolicy clocks, ScheduleEntry entry, Option<Duration> allotted = default) =>
        IO.lift(clocks.Mark).Bind(mark =>
            (entry.Work()
                .Timeout(allotted.IfNone(entry.Deadline.Allotted).ToTimeSpan())
                .Map(static _ => Fin.Succ(unit))
              | @catch<IO, Fin<Unit>>(static _ => true, static error => pure<IO, Fin<Unit>>(Fin.Fail<Unit>(error))))
            .As()
            .Map(outcome => (outcome, clocks.Receipt(entry.Deadline, mark, allotted))));

    public static Option<Instant> Missed(ScheduleEntry entry, Instant lastFired, Instant now) =>
        entry.Spec.Switch(
            cron: static (s, c) =>
                Optional(c.Expression.GetPreviousOccurrence(s.Now.ToDateTimeOffset(), TimeZoneInfo.Utc))
                    .Map(prev => (Prev: prev.ToInstant(), s.LastFired))
                    .Filter(static pair => pair.Prev > pair.LastFired)
                    .Map(static pair => pair.Prev),
            every: static (s, e) =>
                s.LastFired + e.Period <= s.Now ? Some(s.LastFired + e.Period) : None,
            annual: static (s, a) =>
                AnnualWindow(s.LastFired, s.Now, a.Date, a.At, a.Zone).LastOrNone(),
            state: (LastFired: lastFired, Now: now));

    public static Seq<Instant> Window(ScheduleEntry entry, Instant from, Instant to) =>
        entry.Spec.Switch(
            cron: static (s, c) =>
                c.Expression.GetOccurrences(s.From.ToDateTimeOffset(), s.To.ToDateTimeOffset(), TimeZoneInfo.Utc)
                    .Map(static at => at.ToInstant())
                    .ToSeq(),
            every: static (s, e) =>
                Range(0, (int)((s.To - s.From).TotalSeconds / e.Period.TotalSeconds))
                    .Map(step => s.From + e.Period * step)
                    .ToSeq(),
            annual: static (s, a) => AnnualWindow(s.From, s.To, a.Date, a.At, a.Zone),
            state: (From: from, To: to));

    public static Seq<Instant> WindowDescending(ScheduleEntry entry, Instant from, Instant to) =>
        entry.Spec.Switch(
            cron: static (s, c) =>
                c.Expression.GetOccurrencesDescending(s.From.ToDateTimeOffset(), s.To.ToDateTimeOffset(), TimeZoneInfo.Utc)
                    .Map(static at => at.ToInstant())
                    .ToSeq(),
            every: static (s, e) => Window(s.Entry, s.From, s.To).Rev(),
            annual: static (s, a) => AnnualWindow(s.From, s.To, a.Date, a.At, a.Zone).Rev(),
            state: (Entry: entry, From: from, To: to));

    public static Period Span(Instant from, Instant to, DateTimeZone zone) =>
        Period.Between(from.InZone(zone).LocalDateTime, to.InZone(zone).LocalDateTime);

    static Option<Instant> AnnualNext(Instant after, AnnualDate date, LocalTime at, DateTimeZone zone) =>
        Range(0, 2)
            .Map(step => date.InYear(after.InZone(zone).Year + step).At(at).InZoneStrictly(zone).ToInstant())
            .Filter(candidate => candidate > after)
            .HeadOrNone();

    static Seq<Instant> AnnualWindow(Instant from, Instant to, AnnualDate date, LocalTime at, DateTimeZone zone) =>
        Range(from.InZone(zone).Year, to.InZone(zone).Year - from.InZone(zone).Year + 1)
            .Map(year => date.InYear(year).At(at).InZoneStrictly(zone).ToInstant())
            .Filter(candidate => candidate >= from && candidate < to)
            .ToSeq();

    public static Option<SupportTrigger> Heartbeat(CorrelationId correlation, ScheduleEntry entry, DeadlineReceipt receipt) =>
        receipt.Outcome == DeadlineOutcome.Met
            ? None
            : Some<SupportTrigger>(new SupportTrigger.WatchdogTimeout(correlation, entry));
}
```

The watchdog spine composes end to end without a watchdog service type: `Run` emits the heartbeat row's paired `(Fin<Unit>, DeadlineReceipt)`, `Heartbeat` folds a not-met receipt into `SupportTrigger.WatchdogTimeout` carrying the same `ScheduleEntry`, and the support owner consumes that trigger; a missed occurrence is detected through `Missed`, which reads `GetPreviousOccurrence` against the last-fired stamp, an ascending backfill audit reads the bounded `GetOccurrences` window, and a most-recent-first audit reads `WindowDescending` over `GetOccurrencesDescending`. Six-field second-resolution rows admit through `CronFormat.IncludeSeconds`, a fleet-spread cron row admits through the `YearlyWithJitter` template family seeded from the schedule key, and a calendar-recurring rollup admits through the `Annual` case whose `AnnualDate` resolves under the strict zone resolver. `Span` reports the calendar gap between two stamps through `Period.Between`, and `OccurrenceSpec.Identical` proves schedule identity across a reload through `CronExpression.Equals`.

Consumers register rows, never ports — the registered set at composition:

| [INDEX] | [CONSUMER_ROW]            | [SPEC]                    | [DEADLINE]        | [LEASE]           |
| :-----: | ------------------------- | ------------------------- | :---------------- | :---------------- |
|   [1]   | persistence-maintenance   | config-sourced cron       | consumer-declared | maintenance-lease |
|   [2]   | support-scheduled-capture | config-sourced cron       | support-window    | none              |
|   [3]   | bundle-retention-eviction | support-owned cadence row | support-window    | none              |
|   [4]   | compute-model-warmup      | consumer-declared         | consumer-declared | none              |
|   [5]   | watchdog-heartbeat        | `Every` 3 × health-probe  | health-probe      | none              |

The heartbeat period is one policy value fixed at 3 × the health-probe row; one heartbeat row exists per watched child or peer. Maintenance work executes only while the registering process holds the maintenance lease; `LeasePolicy.Maintenance` carries the reclamation value both release routes share.
