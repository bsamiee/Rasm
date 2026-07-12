# [APPHOST_TIME_AND_DEADLINES]

One temporal law serves the whole suite: `TimeProvider` owns elapsed measurement, NodaTime `IClock` owns semantic instants, and one injected `ClockPolicy` record pairs them — consumer capsules bind the pair at construction. `DeadlineClass` is the nine-row bound vocabulary that every duration literal in the four packages traces to, `SchedulePort` is the suite's single scheduler — Cronos cron rows and fixed-period rows carry every scheduled concern, with maintenance-lease policy values deciding cross-process ownership — and `FencingToken` is the decoded CARRIER of the store-issued lease generation riding that maintenance lease — the Persistence store's row-CAS predicate is the authoritative fence, the monotone single-writer correctness proof a timeout alone cannot give, and AppHost mints no token of its own. BCL temporal shapes cross only at the admission seam, receipts stamp `Instant` and `Duration`, and the test row swaps deterministic fakes through the same record.

## [01]-[INDEX]

- [01]-[CLOCK_SPLIT]: One injected clock pair; elapsed versus semantic time with sentinel admission.
- [02]-[DEADLINE_TAXONOMY]: Nine deadline rows; every suite duration literal traces here.
- [03]-[SCHEDULE_PORT]: The suite scheduler with cron and period rows and lease values.
- [04]-[FENCING_TOKEN]: Decoded store-issued token carrier; the store's CAS predicate is the fence.

## [02]-[CLOCK_SPLIT]

- Owner: `ClockPolicy`
- Entry: `public static Option<Instant> Admit(DateTimeOffset raw)` — `Option<T>` carries absence; a platform sentinel never travels inward.
- Packages: NodaTime, Microsoft.Extensions.TimeProvider.Testing, NodaTime.Testing, BCL inbox
- Growth: a new foreign temporal representation lands as one policy value on `ClockPolicy` — an admission overload, a persisted text pattern, or an exported zone-projection formatter; zero new surface.
- Boundary: `ClockPolicy` is an APP-stratum record and NEVER crosses the strata DAG downward — a `ClockPolicy` parameter on an AEC-DOMAIN or APP-PLATFORM signature is the named inversion (the deleted form the Bim Exchange rails carried): below the app root, signatures thread the NEUTRAL primitives the pair wraps — NodaTime `IClock` for a semantic stamp, BCL `TimeProvider` for a monotonic mark/elapsed pair, or a plain `Instant` value — and the app composition supplies them off this one record (`ClockPolicy.System.Clock`/`Time`); app-side siblings receive `ClockPolicy` through composition and stamp TTL, retention, lease, and elapsed evidence from it — `DateTime.UtcNow`, `DateTime.Now`, and direct `Stopwatch` call sites are the deleted patterns; `InstantPattern.ExtendedIso` and `PeriodPattern.Roundtrip` are the only persisted temporal grammars, invariant-culture only; `Exported` is the one cross-boundary stamp formatter — `OffsetDateTimePattern.Rfc3339` carries the offset stamp and `ZonedDateTimePattern` carries the user-facing zoned stamp, bound once through `WithZoneProvider(DateTimeZoneProviders.Tzdb)` and `WithResolver(Resolvers.StrictResolver)` so a hand-built zoned formatter is the deleted pattern; `ZoneShift` reads the offset transition at an instant through `GetZoneInterval`, the only DST-evidence surface for receipt windows that straddle a transition; the test row constructs the same record from `FakeTimeProvider` and `FakeClock`, so `Advance`, `SetUtcNow`, `AutoAdvanceAmount`, and `FromUtc` drive schedule, drain, and retry specs deterministically with zero test-only production surface.

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

## [03]-[DEADLINE_TAXONOMY]

- Owner: `DeadlineClass`
- Cases: startup, ready-probe, health-probe, drain-cooperative, drain-forced, hop-attempt, hop-total, support-window, cache-ttl
- Entry: `public DeadlineReceipt Receipt(DeadlineClass row, long mark, Option<Duration> allotted = default)` — pure value; the outcome derives from measurement, never from a caller flag.
- Receipt: `DeadlineReceipt` — class, allotted `Duration`, consumed `Duration`, outcome, `Instant` stamp.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new bound is one `DeadlineClass` row; profile variance stays one policy value through `Resolve`; zero new surface.
- Boundary: every duration bound in the suite traces to a row here or to a policy row on its owning page — a bare `TimeSpan` literal anywhere else is the named defect; profile variance enters `Resolve` as one override-table swap at the composition root, and consumers read the frozen table, never the raw rows; the cancellation spine, hop registry, drain conductor, and cache lanes consume these rows as values — drain-cooperative escalates to drain-forced and every other miss is forced; the cooperative allotment is the telemetry-flush budget — a ForceFlush during plugin unload runs inside drain-cooperative, and an overrun escalates through the `Escalation` arc to drain-forced, the terminal forced-flush bound past which the drain conductor abandons in-flight export, so the flush latency is one `escalatesTo` arc, never a separate timer.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

## [04]-[SCHEDULE_PORT]

- Owner: `ScheduleEntry`
- Cases: `OccurrenceSpec.Cron(CronExpression Expression)` | `OccurrenceSpec.Every(Duration Period)` | `OccurrenceSpec.Annual(AnnualDate Date, LocalTime At, DateTimeZone Zone)`
- Entry: `public static IO<(Fin<Unit> Outcome, DeadlineReceipt Deadline)> Run(ClockPolicy clocks, ScheduleEntry entry, Option<Duration> allotted = default)` — `IO<T>` carries the deferred occurrence run; the work outcome rides `Fin<Unit>` beside the deadline receipt.
- Receipt: every occurrence run yields its `DeadlineReceipt` paired with the work outcome, and `Heartbeat` folds a not-met receipt into `SupportTrigger.WatchdogTimeout` carrying the firing `ScheduleEntry` — the watchdog signal the support owner consumes, composed from the receipt with no watchdog service type.
- Packages: Cronos, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing
- Growth: a new scheduled concern is one `ScheduleEntry` row registered by its consumer, a new occurrence grammar is one case on `OccurrenceSpec`, a new fleet cadence is one `CronCadence` row, and a fleet-spread concern is one `ScheduleEntry.Spread` call; zero new surface.
- Boundary: this port is the suite's only scheduler — per-package timer loops, host idle hooks, pg_cron, Quartz, Hangfire, and NCrontab are the deleted patterns; occurrence math consumes and emits UTC instants with zone projection confined inside the occurrence call; cron rows persist as expression text and rebuild through `TryParse` at composition, so `CronFormatException` never crosses the configuration boundary; second-resolution rows admit through `CronFormat.IncludeSeconds` and a fleet-spread cron row admits through `OccurrenceSpec.Fleet`, which routes the cadence keyword to the matching `{Yearly,Weekly,Monthly,Daily,Hourly,EveryMinute}WithJitter(int jitterSeed)` template and falls through to the four-arg `CronExpression.TryParse(expression, format, jitterSeed, out)` for a literal expression carrying an `H` field, while `EverySecond` carries no jitter template so a second-resolution fleet row is the rejected case; `ScheduleEntry.Spread` derives that `jitterSeed` from the schedule key through `XxHash3.HashToUInt64` over the UTF-8 key bytes, folded to `int`, so fleet spreading of a shared cron row is one cross-process-stable schedule-key-derived seed value rather than the per-process-randomized `string.GetHashCode`, and a process restart re-parses the identical spread expression, never a hand-edited expression; the `Annual` case carries a calendar-recurring `AnnualDate` resolved through `InYear(...).At(...).InZoneStrictly` so a once-a-year rollup row maps its local wall-time to a UTC instant under the strict resolver, never a hand-built leap-day branch; `Missed` detects a skipped occurrence through `GetPreviousOccurrence` against the last-fired stamp, `Window` audits a bounded ascending backfill through `GetOccurrences`, and `WindowDescending` reads the most-recent-first audit through `GetOccurrencesDescending`, so a missed-occurrence sweep reads occurrence history rather than tracking a running counter; `Span` reports the calendar gap between two stamps through `Period.Between` over zoned local date-times — the only calendar-difference surface a retention or lease-age report reads, never a `Duration`-to-days division; `OccurrenceSpec.Identical` compares two specs through `CronExpression.Equals` on the cron arm so a reload that re-parses an unchanged expression text is recognized as the same schedule and never re-registers; lease release has two distinct values — handoff-on-drain releases immediately on the drain conductor's signal, crash-reclaim waits `CrashStaleness` past the holder's last stamp, and `CrashStaleness` exceeds the drain-cooperative plus drain-forced sum so a draining holder is never reclaimed mid-drain; a watchdog is a heartbeat row plus a deadline class, never a service type; the `Heartbeat` crossing into Observability/bundles#SUPPORT_CAPTURE binds the wire-stable schedule `Key` and `DeadlineReceipt` as the cross-owner contract — the live `ScheduleEntry` with its `Func<IO<Unit>> Work` closure stays process-local and never rides into the support owner's manifest or any serialized trigger, so the seam couples to the schedule-key contract, not the scheduler's delegate-bearing interior.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CronCadence {
    public static readonly CronCadence Yearly = new("@yearly", CronExpression.YearlyWithJitter);
    public static readonly CronCadence Weekly = new("@weekly", CronExpression.WeeklyWithJitter);
    public static readonly CronCadence Monthly = new("@monthly", CronExpression.MonthlyWithJitter);
    public static readonly CronCadence Daily = new("@daily", CronExpression.DailyWithJitter);
    public static readonly CronCadence Hourly = new("@hourly", CronExpression.HourlyWithJitter);
    public static readonly CronCadence EveryMinute = new("@every_minute", CronExpression.EveryMinuteWithJitter);

    private readonly Func<int, CronExpression> jitter;

    public CronExpression WithJitter(int jitterSeed) => jitter(jitterSeed);
}

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

    public static Fin<OccurrenceSpec> Fleet(string template, CronFormat format, int jitterSeed) =>
        CronCadence.TryGet(template, out var cadence)
            ? Fin.Succ<OccurrenceSpec>(new Cron(cadence.WithJitter(jitterSeed)))
            : CronExpression.TryParse(template, format, jitterSeed, out var parsed)
                ? Fin.Succ<OccurrenceSpec>(new Cron(parsed!))
                : Fin.Fail<OccurrenceSpec>(Error.New($"<invalid-fleet:{template}>"));

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
    Func<IO<Unit>> Work) {
    public static int Seed(string key) =>
        unchecked((int)XxHash3.HashToUInt64(System.Text.Encoding.UTF8.GetBytes(key)));

    public static Fin<ScheduleEntry> Spread(
        string key,
        string template,
        CronFormat format,
        DeadlineClass deadline,
        Option<LeasePolicy> lease,
        Func<IO<Unit>> work) =>
        OccurrenceSpec.Fleet(template, format, Seed(key))
            .Map(spec => new ScheduleEntry(key, spec, deadline, lease, work));
}

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

The watchdog spine composes end to end without a watchdog service type: `Run` emits the heartbeat row's paired `(Fin<Unit>, DeadlineReceipt)`, `Heartbeat` folds a not-met receipt into `SupportTrigger.WatchdogTimeout` carrying the same `ScheduleEntry`, and the support owner consumes that trigger; a missed occurrence is detected through `Missed`, which reads `GetPreviousOccurrence` against the last-fired stamp, an ascending backfill audit reads the bounded `GetOccurrences` window, and a most-recent-first audit reads `WindowDescending` over `GetOccurrencesDescending`. Six-field second-resolution rows admit through `CronFormat.IncludeSeconds`, a fleet-spread cron row admits through `OccurrenceSpec.Fleet` over the `CronCadence` template table seeded by `ScheduleEntry.Spread` from the schedule key through `XxHash3.HashToUInt64`, and a calendar-recurring rollup admits through the `Annual` case whose `AnnualDate` resolves under the strict zone resolver. `Span` reports the calendar gap between two stamps through `Period.Between`, and `OccurrenceSpec.Identical` proves schedule identity across a reload through `CronExpression.Equals`.

Consumers register rows, never ports — the registered set at composition:

| [INDEX] | [CONSUMER_ROW]            | [SPEC]                    | [DEADLINE]        | [LEASE]           |
| :-----: | :------------------------ | :------------------------ | :---------------- | :---------------- |
|  [01]   | persistence-maintenance   | config-sourced cron       | consumer-declared | maintenance-lease |
|  [02]   | support-scheduled-capture | config-sourced cron       | support-window    | none              |
|  [03]   | bundle-retention-eviction | support-owned cadence row | support-window    | none              |
|  [04]   | compute-model-warmup      | consumer-declared         | consumer-declared | none              |
|  [05]   | watchdog-heartbeat        | `Every` 3 × health-probe  | health-probe      | none              |
|  [06]   | fleet-rollup              | `@yearly`+jitter          | support-window    | none              |

The heartbeat period is one policy value fixed at 3 × the health-probe row; one heartbeat row exists per watched child or peer. Maintenance work executes only while the registering process holds the maintenance lease; `LeasePolicy.Maintenance` carries the reclamation value both release routes share. The fleet-rollup row registers through `ScheduleEntry.Spread` so its `@yearly`+jitter occurrence carries a deterministic `XxHash3`-derived seed off the row key — every fleet node computes the identical seed from the shared key, the `H` field distributes the nodes across the cadence window, and each rollup run emits its `DeadlineReceipt` through the same `Run` path under the support-window deadline with no fleet-specific instrument; a not-met rollup folds through `Heartbeat` into `SupportTrigger.WatchdogTimeout` like every other watched row.

## [05]-[FENCING_TOKEN]

- Owner: `FencingToken` `[ValueObject<ulong>]` — the decoded carrier of the store-issued lease generation; `LeaseElection` the ONE Persistence PORT adapter acquiring, renewing, and fencing through the coordination op-union.
- Entry: `Acquire(LeaseElection.Runtime runtime, string leaseKey)` returns `Fin<FencingToken>` — the adapter calls the Persistence coordination op-union with wire-stable primitives (lease key, holder id, staleness millis) and DECODES the store-issued `LeaseToken` generation into the carrier; `Fence(runtime, leaseKey, held)` routes a guarded write's token through the store's row-CAS predicate — the authoritative check; `Admits(FencingToken incoming)` survives ONLY as a documented client-side pre-check that short-circuits an obviously-stale retry before the round-trip, never a substitute for the store's verdict.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new fenced resource carries the same decoded token through the same `Fence` adapter, never a second token type; a new election driver acquires through the same `Acquire`; zero new surface.
- Boundary: the fence is store-validated or it is not fencing — a per-process token mint, an in-memory latest-token cell, and an in-memory fence validation are the DELETED forms (two processes minting independent sequences is zero cross-process safety); the store issues the strictly-increasing generation as its fenced-lease column and validates every guarded write with `WHERE token >= held` (the Kleppmann reject-lower AT the resource), so a paused-then-resumed stale holder's late write rejects store-side and surfaces through the adapter as the decoded `LeaseFenced` fault — a registry-banded `Wire/coordination#DISTRIBUTED_LOCK` `CoordinationFault` case the composition-root delegate binding constructs at the seam, never a bare `Error.New` minted here; requests cross as wire-stable primitives and results decode from Persistence-owned types — no AppHost interface or type crosses down; the election reuses the `LeasePolicy.Maintenance` `CrashStaleness` window as the lease timeout but the store-validated token is the correctness proof the timeout alone cannot give; the token crosses the wire as the same decimal-string width as the HLC `Logical` half so the op-log cursor and the fence read one monotone identity; the maintenance-lease election at SCHEDULE_PORT, the Sandbox/provisioning#ROLLOVER_DRAIN `FleetRoll` conductor election, and the sidecar Wire/companion#PROCESS_MODALITY write-forward each acquire through this one rail — the store (`Rasm.Persistence` `ONE_FENCED_LEASE_STORE`) fences every token, aligned to a sibling branch, never coupled; a held lease without a store-validated token is the rejected form.

```csharp signature
[ValueObject<ulong>(
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
public readonly partial struct FencingToken {
    public static readonly FencingToken Zero = Create(0UL);

    // Client-side PRE-CHECK only: short-circuits an obviously-stale retry before the round-trip.
    // The authoritative fence is the store's row-CAS predicate (WHERE token >= held) — always consulted.
    public bool Admits(FencingToken incoming) => incoming >= this;
}

// The ONE Persistence PORT adapter for lease custody: wire-stable primitives cross down, the
// store-issued generation decodes back. AppHost mints nothing; the delegates bind the store's
// coordination op-union at the composition root, their failures the decoded registry-banded
// CoordinationFault cases (LeaseFenced on a rejected guarded write) constructed at that binding.
public static class LeaseElection {
    public sealed record Runtime(
        Func<string, LeasePolicy, Fin<(ulong Generation, Instant Deadline)>> AcquireLease,
        Func<string, LeasePolicy, ulong, Fin<(ulong Generation, Instant Deadline)>> RenewLease,
        Func<string, ulong, Fin<Unit>> GuardWrite,
        Func<string, ulong, Fin<Unit>> ReleaseLease,
        LeasePolicy Lease);

    public static Fin<FencingToken> Acquire(Runtime runtime, string leaseKey) =>
        runtime.AcquireLease(leaseKey, runtime.Lease).Map(static grant => FencingToken.Create(grant.Generation));

    public static Fin<FencingToken> Renew(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.RenewLease(leaseKey, runtime.Lease, (ulong)held).Map(static grant => FencingToken.Create(grant.Generation));

    public static Fin<Unit> Fence(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.GuardWrite(leaseKey, (ulong)held);

    public static Fin<Unit> Release(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.ReleaseLease(leaseKey, (ulong)held);
}
```
