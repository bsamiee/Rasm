# [APPHOST_TIME_AND_DEADLINES]

One temporal law serves the whole suite: `TimeProvider` owns elapsed measurement, NodaTime `IClock` owns semantic instants, and one injected `ClockPolicy` record pairs them — consumer capsules bind the pair at construction. `DeadlineClass` is the nine-row bound vocabulary that every duration literal in the four packages traces to, and `SchedulePort` is the suite's single scheduler — Cronos cron rows and fixed-period rows carry every scheduled concern, with maintenance-lease policy values deciding cross-process ownership. BCL temporal shapes cross only at the admission seam, receipts stamp `Instant` and `Duration`, and the test row swaps deterministic fakes through the same record.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                              |
| :-----: | ----------------- | ------------------------------------------------------------------- |
|   [1]   | CLOCK_SPLIT       | One injected clock pair; elapsed versus semantic time; sentinel admission |
|   [2]   | DEADLINE_TAXONOMY | Nine deadline rows; every suite duration literal traces here        |
|   [3]   | SCHEDULE_PORT     | The suite scheduler; cron and period rows; lease values             |

## [2]-[CLOCK_SPLIT]

- Owner: `ClockPolicy`
- Entry: `public static Option<Instant> Admit(DateTimeOffset raw)` — `Option<T>` carries absence; a platform sentinel never travels inward.
- Packages: NodaTime, Microsoft.Extensions.TimeProvider.Testing, NodaTime.Testing, BCL inbox
- Growth: a new foreign temporal representation lands as one policy value on `ClockPolicy` — an admission overload or a persisted text pattern; zero new surface.
- Boundary: siblings receive `ClockPolicy` through composition and stamp TTL, retention, lease, and elapsed evidence from it — `DateTime.UtcNow`, `DateTime.Now`, and direct `Stopwatch` call sites are the deleted patterns; `InstantPattern.ExtendedIso` and `PeriodPattern.Roundtrip` are the only persisted temporal grammars, invariant-culture only; `OffsetDateTime` carries exported stamps and `ZonedDateTime` appears only at user-facing edges through an explicit resolver; the test row constructs the same record from `FakeTimeProvider` and `FakeClock`, so `Advance`, `SetUtcNow`, `AutoAdvanceAmount`, and `FromUtc` drive schedule, drain, and retry specs deterministically with zero test-only production surface.

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
}
```

## [3]-[DEADLINE_TAXONOMY]

- Owner: `DeadlineClass`
- Cases: startup, ready-probe, health-probe, drain-cooperative, drain-forced, hop-attempt, hop-total, support-window, cache-ttl
- Entry: `public DeadlineReceipt Receipt(DeadlineClass row, long mark, Option<Duration> allotted = default)` — pure value; the outcome derives from measurement, never from a caller flag.
- Receipt: `DeadlineReceipt` — class, allotted `Duration`, consumed `Duration`, outcome, `Instant` stamp.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new bound is one `DeadlineClass` row; profile variance stays one policy value through `Resolve`; zero new surface.
- Boundary: every duration bound in the suite traces to a row here or to a policy row on its owning page — a bare `TimeSpan` literal anywhere else is the named defect; profile variance enters `Resolve` as one override-table swap at the composition root, and consumers read the frozen table, never the raw rows; the cancellation spine, hop registry, drain conductor, and cache lanes consume these rows as values — drain-cooperative escalates to drain-forced and every other miss is forced.

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
- Cases: `OccurrenceSpec.Cron(CronExpression Expression)` | `OccurrenceSpec.Every(Duration Period)`
- Entry: `public static IO<(Fin<Unit> Outcome, DeadlineReceipt Deadline)> Run(ClockPolicy clocks, ScheduleEntry entry, Option<Duration> allotted = default)` — `IO<T>` carries the deferred occurrence run; the work outcome rides `Fin<Unit>` beside the deadline receipt.
- Receipt: every occurrence run yields its `DeadlineReceipt` paired with the work outcome — a miss past the allotted bound is the watchdog signal consumed by the support owner.
- Packages: Cronos, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new scheduled concern is one `ScheduleEntry` row registered by its consumer, and a new occurrence grammar is one case on `OccurrenceSpec`; zero new surface.
- Boundary: this port is the suite's only scheduler — per-package timer loops, host idle hooks, pg_cron, Quartz, Hangfire, and NCrontab are the deleted patterns; occurrence math consumes and emits UTC instants with zone projection confined inside the occurrence call; cron rows persist as expression text and rebuild through `TryParse` at composition, so `CronFormatException` never crosses the configuration boundary; lease release has two distinct values — handoff-on-drain releases immediately on the drain conductor's signal, crash-reclaim waits `CrashStaleness` past the holder's last stamp, and `CrashStaleness` exceeds the drain-cooperative plus drain-forced sum so a draining holder is never reclaimed mid-drain; a watchdog is a heartbeat row plus a deadline class, never a service type.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OccurrenceSpec {
    private OccurrenceSpec() { }

    public sealed record Cron(CronExpression Expression) : OccurrenceSpec;

    public sealed record Every(Duration Period) : OccurrenceSpec;

    public static Fin<OccurrenceSpec> Admit(string expression, CronFormat format) =>
        CronExpression.TryParse(expression, format, out var parsed)
            ? Fin.Succ<OccurrenceSpec>(new Cron(parsed!))
            : Fin.Fail<OccurrenceSpec>(Error.New($"<invalid-cron:{expression}>"));
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
            state: after);

    public static IO<(Fin<Unit> Outcome, DeadlineReceipt Deadline)> Run(ClockPolicy clocks, ScheduleEntry entry, Option<Duration> allotted = default) =>
        IO.lift(clocks.Mark).Bind(mark =>
            (entry.Work()
                .Timeout(allotted.IfNone(entry.Deadline.Allotted).ToTimeSpan())
                .Map(static _ => Fin.Succ(unit))
              | @catch<IO, Fin<Unit>>(static _ => true, static error => pure<IO, Fin<Unit>>(Fin.Fail<Unit>(error))))
            .As()
            .Map(outcome => (outcome, clocks.Receipt(entry.Deadline, mark, allotted))));
}
```

Consumers register rows, never ports — the registered set at composition:

| [INDEX] | [CONSUMER_ROW]            | [SPEC]              | [DEADLINE]        | [LEASE]           |
| :-----: | ------------------------- | ------------------- | :---------------- | :---------------- |
|   [1]   | persistence-maintenance   | config-sourced cron | consumer-declared | maintenance-lease |
|   [2]   | support-scheduled-capture | config-sourced cron | support-window    | none              |
|   [3]   | bundle-retention-eviction | support-owned cadence row | support-window | none           |
|   [4]   | compute-model-warmup      | consumer-declared   | consumer-declared | none              |
|   [5]   | watchdog-heartbeat        | `Every` 15 s        | health-probe      | none              |

The heartbeat period is one policy value fixed at 3 × the health-probe row; one heartbeat row exists per watched child or peer, and a run whose receipt outcome is not met is the watchdog-timeout consequence at its consuming owner. Maintenance work executes only while the registering process holds the maintenance lease; `LeasePolicy.Maintenance` carries the reclamation value both release routes share.

## [5]-[RESEARCH]

| [INDEX] | [ITEM]                                                                          | [PROOF]                                                                                                  | [GATE]            |
| :-----: | ------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ----------------- |
|   [1]   | ForceFlush completion latency inside the drain-cooperative allotment during plugin unload | libs/csharp/Rasm.AppHost/scenarios/drain-deadlines.verify.csx measuring drain receipt elapsed against the row under live RhinoWIP | DEADLINE_TAXONOMY |
|   [2]   | Cronos jitter-seed parameter shape for key-derived `H` schedule rows            | uv run python -m tools.assay api query cronos CronExpression.Parse                                        | SCHEDULE_PORT     |
