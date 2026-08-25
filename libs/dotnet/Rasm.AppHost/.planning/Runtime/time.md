# [APPHOST_TIME_AND_DEADLINES]

One temporal law serves the whole suite: kernel `MonotonicTimeline` owns elapsed measurement, NodaTime `IClock` owns semantic instants, and one injected `ClockPolicy` record binds both — consumer capsules take the record at construction. `DeadlineClass` is the bound deadline vocabulary every duration literal in the four packages traces to and the gauge roster each kernel span reads. `SchedulePort` is the suite's single scheduler — Cronos cron rows, fixed-period rows, and calendar-recurring rows carry every scheduled concern under one occurrence rail, each entry carrying its own `RedrivePolicy` — and `FencingToken` is the decoded CARRIER of the store-issued lease generation riding the maintenance lease, where the Persistence store's row-CAS predicate is the authoritative fence and AppHost mints no token of its own. The test row swaps deterministic fakes through the same record.

## [01]-[INDEX]

- [02]-[CLOCK_SPLIT]: One injected clock triple; the monotonic timeline, the semantic clock, and the persisted grammars.
- [03]-[DEADLINE_TAXONOMY]: Deadline rows as gauge lanes; every suite duration literal traces here.
- [04]-[SCHEDULE_PORT]: The suite scheduler with cron, period, and annual rows, redrive curves, and lease values.
- [05]-[FENCING_TOKEN]: Decoded store-issued token carrier and the one acquire-renew-guard-release lease algebra over it.

## [02]-[CLOCK_SPLIT]

- Owner: `ClockPolicy` — the one injected triple binding the kernel `MonotonicTimeline`, the NodaTime `IClock`, and the `TimeProvider` both derive from.
- Entry: `public static Fin<ClockPolicy> Of(TimeProvider time, IClock clock, Op? key = null)` — the timeline admits its provider's timestamp frequency once, so a provider that cannot measure refuses at composition rather than at the first span.
- Packages: Rasm (kernel `MonotonicTimeline`), NodaTime, Microsoft.Extensions.TimeProvider.Testing, NodaTime.Testing, BCL inbox
- Growth: a new persisted temporal grammar is one policy value on `ClockPolicy`; a new gauged concern is one `IGaugeLane` roster at its owning page, never a second span type; zero new surface.
- Boundary: `ClockPolicy` is an APP-stratum record and NEVER crosses the strata DAG downward — a `ClockPolicy` parameter on an AEC-DOMAIN or APP-PLATFORM signature is the named inversion (the deleted form the Bim Exchange rails carried): below the app root a monotonic mark/elapsed pair threads as kernel `MonotonicTimeline`, constructed once off `TimeProvider`; a semantic stamp threads as `IClock`/`Instant`; a bounded latency reading threads as `MonotonicTimeline.Gauged` — and the app composition supplies each off this one record; app-side siblings receive `ClockPolicy` through composition and stamp TTL, retention, lease, and elapsed evidence from it, so `DateTime.UtcNow`, `DateTime.Now`, a direct `Stopwatch`, and a raw `TimeProvider.GetTimestamp`/`GetElapsedTime` pair are the deleted patterns; `InstantPattern.ExtendedIso` and `PeriodPattern.Roundtrip` are the only persisted temporal grammars, invariant-culture only, and they are the one pair every durable stamp and every span text rebuilds through; NAMED LOSS — the `DateTimeOffset`/`DateTime` sentinel admission, the `Interval` radius window, the RFC-3339 and zoned export formatters, and the `GetZoneInterval` DST probe all delete unread, so a foreign BCL temporal shape and a zone-projected export each land their admission at the boundary that first reads one rather than sitting here as an unreached seam; the test row constructs the same record from `FakeTimeProvider` and `FakeClock`, so `Advance`, `SetUtcNow`, `AutoAdvanceAmount`, and `FromUtc` drive schedule, drain, and retry specs deterministically with zero test-only production surface.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record ClockPolicy(TimeProvider Time, IClock Clock, MonotonicTimeline Line) {
    public static readonly Fin<ClockPolicy> System = Of(time: TimeProvider.System, clock: SystemClock.Instance);

    public static Fin<ClockPolicy> Of(TimeProvider time, IClock clock, Op? key = null) =>
        MonotonicTimeline.Of(provider: time, key: key)
            .Map(line => new ClockPolicy(Time: time, Clock: clock, Line: line));

    public Instant Now => Clock.GetCurrentInstant();

    public static string Persisted(Instant value) => InstantPattern.ExtendedIso.Format(value);

    public static string Persisted(Period value) => PeriodPattern.Roundtrip.Format(value);
}
```

## [03]-[DEADLINE_TAXONOMY]

- Owner: `DeadlineClass` `[SmartEnum<string>]` realizing kernel `IGaugeLane<DeadlineClass>` — the bound vocabulary AND the gauge roster; `DeadlineOutcome` the derived three-valued escalation verdict; `DeadlineOps` the one gauged bracket.
- Cases: startup, ready-probe, health-probe, drain-cooperative, drain-forced, hop-attempt, hop-total, lane-attempt, lane-fold, support-window, otlp-drain, cache-ttl; outcomes met | escalated | forced.
- Entry: `public Fin<(Fin<T> Value, GaugedSpan<DeadlineClass> Span)> Gauged<T>(DeadlineClass row, Op work, Func<Fin<T>> body)` on `ClockPolicy` — the body's own verdict rides INSIDE the pair so a refused body still returns its crossing, and only a broken capture fails the outer rail.
- Packages: Rasm (kernel `IGaugeLane`/`GaugedSpan`/`MonotonicTimeline`), Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: a new bound is one `DeadlineClass` row and the gauge lane it becomes by declaration; zero new surface.
- Boundary: every duration bound in the suite traces to a row here or to a policy row on its owning page — a bare `TimeSpan` literal anywhere else is the named defect; the row IS the lane, so `Gauged` reads `lane.Bound` and carries no bound parameter and two call sites gauging one deadline cannot disagree; the outcome derives from the span's own `Breached` plus the presence of an escalation arc and never from a caller flag, so a stored breach flag is the deleted form; NAMED LOSS — the profile-override table and its `Resolve` fold delete unread, so profile variance now enters as a row edit at this owner rather than a frozen dictionary no consumer resolved; the cancellation spine, hop registry, work-lane governor, drain conductor, and cache lanes consume these rows as values — drain-cooperative escalates to drain-forced and every other miss is forced; the two lane rows are the in-process axis the transport rows never covered, so a `LanePolicy` reaching for `hop-attempt` prices an in-process fold on a socket's budget and is the substitution this pair deletes, while the interactive-versus-fold split is the lane's own rank and never a profile override; the cooperative allotment is the telemetry-flush budget — a ForceFlush during plugin unload runs inside drain-cooperative and an overrun escalates through the `Escalation` arc to drain-forced, the terminal forced-flush bound past which the drain conductor abandons in-flight export, so the flush latency is one `escalatesTo` arc and never a separate timer.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeadlineClass : IGaugeLane<DeadlineClass> {
    public static readonly DeadlineClass Startup = new("startup", Duration.FromSeconds(30), escalatesTo: None);
    public static readonly DeadlineClass ReadyProbe = new("ready-probe", Duration.FromSeconds(5), escalatesTo: None);
    public static readonly DeadlineClass HealthProbe = new("health-probe", Duration.FromSeconds(5), escalatesTo: None);
    public static readonly DeadlineClass DrainCooperative = new("drain-cooperative", Duration.FromSeconds(20), escalatesTo: Some("drain-forced"));
    public static readonly DeadlineClass DrainForced = new("drain-forced", Duration.FromSeconds(5), escalatesTo: None);
    public static readonly DeadlineClass HopAttempt = new("hop-attempt", Duration.FromSeconds(10), escalatesTo: None);
    public static readonly DeadlineClass HopTotal = new("hop-total", Duration.FromSeconds(30), escalatesTo: None);
    public static readonly DeadlineClass LaneAttempt = new("lane-attempt", Duration.FromSeconds(30), escalatesTo: None);
    public static readonly DeadlineClass LaneFold = new("lane-fold", Duration.FromMinutes(10), escalatesTo: None);
    public static readonly DeadlineClass SupportWindow = new("support-window", Duration.FromSeconds(120), escalatesTo: None);
    public static readonly DeadlineClass OtlpDrain = new("otlp-drain", Duration.FromSeconds(5), escalatesTo: None);
    public static readonly DeadlineClass CacheTtl = new("cache-ttl", Duration.FromMinutes(5), escalatesTo: None);

    private readonly Option<string> escalatesTo;

    public Duration Allotted { get; }
    public TimeSpan Bound => Allotted.ToTimeSpan();

    public Option<DeadlineClass> Escalation =>
        escalatesTo.Bind(static key => TryGet(key, out DeadlineClass? row) ? Optional(row) : None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeadlineOutcome {
    public static readonly DeadlineOutcome Met = new("met");
    public static readonly DeadlineOutcome Escalated = new("escalated");
    public static readonly DeadlineOutcome Forced = new("forced");

    public static DeadlineOutcome Of(GaugedSpan<DeadlineClass> span) =>
        !span.Breached ? Met
        : span.Lane.Escalation.IsSome ? Escalated
        : Forced;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DeadlineOps {
    extension(ClockPolicy clocks) {
        public Fin<(Fin<T> Value, GaugedSpan<DeadlineClass> Span)> Gauged<T>(DeadlineClass row, Op work, Func<Fin<T>> body) =>
            clocks.Line.Gauged<T, DeadlineClass>(lane: row, work: work, body: body, key: work);
    }
}
```

## [04]-[SCHEDULE_PORT]

- Owner: `ScheduleEntry` the registered row; `OccurrenceSpec` the three occurrence grammars; `CronCadence` the fleet template table; `LeasePolicy` the reclamation value; `TimeFault` the refusal family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` binds zero-based case identity to `FaultBand.HostSchedule`; `Code` derives SEALED); `SchedulePort` the fold.
- Cases: `OccurrenceSpec.Cron(CronExpression Expression)` | `OccurrenceSpec.Every(Duration Period)` | `OccurrenceSpec.Annual(AnnualDate Date, LocalTime At, DateTimeZone Zone)`; `TimeFault` = CronRejected | WindowExhausted | OccurrenceTimedOut.
- Entry: `public static IO<(Fin<Unit> Outcome, GaugedSpan<DeadlineClass> Span)> Run(ClockPolicy clocks, ScheduleEntry entry, Op? key = null)` — `IO<T>` carries the deferred occurrence run; the work outcome rides beside its gauged span, and the redrive curve runs inside the gauge so a re-driven occurrence reports the whole crossing.
- Packages: Rasm (kernel `ContentHash`/`RedrivePolicy`/`Redrive`/`Retriability`/`FaultBand`), Cronos, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new scheduled concern is one `ScheduleEntry` row registered by its consumer, a new occurrence grammar is one case on `OccurrenceSpec`, a new fleet cadence is one `CronCadence` row, and a new refusal is one `TimeFault` case carrying its own retriability; zero new surface.
- Boundary: this port is the suite's only scheduler — per-package timer loops, host idle hooks, pg_cron, Quartz, Hangfire, and NCrontab are the deleted patterns; occurrence math consumes and emits UTC instants with zone projection confined inside the occurrence call; cron rows persist as expression text and rebuild through `Admit` at composition, so `CronFormatException` never crosses the configuration boundary and a refusal is the banded `TimeFault.CronRejected` rather than a bare `Error.New`; `Admit` is the ONE admission over that concern and discriminates on its input — a `CronCadence` keyword with a seed resolves the matching `{Yearly,Weekly,Monthly,Daily,Hourly,EveryMinute}WithJitter(int)` template, anything else parses through `CronExpression.TryParse` under the declared `CronFormat` with the seed when one is supplied, so second-resolution rows admit through `CronFormat.IncludeSeconds` and `EverySecond` carries no jitter template, which makes a second-resolution fleet row the rejected case at the package rail rather than at a hand branch; `ScheduleEntry.Spread` derives that seed from the schedule key through the kernel `ContentHash` framed writer, so fleet spreading of a shared cron row is one cross-process-stable schedule-key-derived value rather than the per-process-randomized `string.GetHashCode`, the folder's third hash algorithm deletes with it, and a process restart re-parses the identical spread expression; NAMED LOSS — the seed VALUE changes once at this landing and every fleet node moves together, which is exactly the fleet property the row exists to hold; the `Annual` case carries a calendar-recurring `AnnualDate` resolved through `InYear(...).At(...).InZoneStrictly` so a once-a-year rollup maps its local wall-time to a UTC instant under the strict resolver, never a hand-built leap-day branch; `Missed` is the ONE occurrence-window read — every unfired occurrence in the half-open window, ascending, bounded by the entry's own redrive bound and refusing above it as typed `WindowExhausted`, so an outbox sweep reads occurrence history rather than tracking a running counter and a hundred-day second-resolution window reports exhaustion instead of overflowing a narrowed `int` range; NAMED LOSS — the descending audit and the `Period.Between` calendar-gap report delete unread, so a most-recent-first read reverses the bounded ascending answer and a calendar-difference report lands at the owner that first needs one; the occurrence deadline is the retriability seam — a `Timeout` expiry re-codes to the transient `OccurrenceTimedOut` so `Redrive.Run` re-drives it on the entry's own curve while every other refusal stays terminal, and a catch-all predicate that swallowed the timeout beside a genuine fault is the deleted form; lease release has two distinct values — handoff-on-drain releases immediately on the drain conductor's signal, crash-reclaim waits `CrashStaleness` past the holder's last stamp, and `LeasePolicy.Outlasts` proves that window outlasts the drain-cooperative plus drain-forced sum so a draining holder is never reclaimed mid-drain, forced at the `Runtime/modules#MODULE_LEDGER` `coordination-seat` row where the policy meets those bounds, so a refused proof stops boot on the typed rail rather than standing unread; a watchdog is a heartbeat row plus a deadline class; the `Heartbeat` crossing into `Observability/bundles#CAPTURE_PIPELINE` binds the wire-stable schedule `Key` and measured `GaugedSpan<DeadlineClass>` as the cross-owner contract, and the trigger's `Timed` case carries both while the live `ScheduleEntry` closure stays process-local.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

    [UseDelegateFromConstructor] public partial CronExpression WithJitter(int jitterSeed);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OccurrenceSpec {
    private OccurrenceSpec() { }

    public sealed record Cron(CronExpression Expression) : OccurrenceSpec;

    public sealed record Every(Duration Period) : OccurrenceSpec;

    public sealed record Annual(AnnualDate Date, LocalTime At, DateTimeZone Zone) : OccurrenceSpec;

    public static Fin<OccurrenceSpec> Admit(string text, CronFormat format, Option<int> jitterSeed = default) =>
        (Templated(text: text, jitterSeed: jitterSeed) | Parsed(text: text, format: format, jitterSeed: jitterSeed))
            .Map(static expression => (OccurrenceSpec)new Cron(Expression: expression))
            .ToFin(new TimeFault.CronRejected(Text: text));

    static Option<CronExpression> Templated(string text, Option<int> jitterSeed) =>
        from seed in jitterSeed
        from cadence in CronCadence.TryGet(text, out CronCadence? row) ? Optional(row) : None
        select cadence.WithJitter(jitterSeed: seed);

    static Option<CronExpression> Parsed(string text, CronFormat format, Option<int> jitterSeed) =>
        jitterSeed.Match(
            Some: seed => CronExpression.TryParse(text, format, seed, out CronExpression? seeded) ? Optional(seeded) : None,
            None: () => CronExpression.TryParse(text, format, out CronExpression? plain) ? Optional(plain) : None);
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimeFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostSchedule;
    private TimeFault() { }

    [FaultCase(0)]
    public sealed partial record CronRejected(string Text) : TimeFault {
        public override string Message => $"Cron expression '{Text}' did not parse.";
    }

    [FaultCase(1)]
    public sealed partial record WindowExhausted(string Key, int Bound) : TimeFault {
        public override string Message => string.Create(provider: CultureInfo.InvariantCulture,
            $"Schedule '{Key}' holds more than {Bound} unfired occurrences in the read window.");
    }

    [FaultCase(2)]
    public sealed partial record OccurrenceTimedOut(string Key, Duration Allotted) : TimeFault {
        public override string Message => $"Schedule '{Key}' occurrence exceeded its {Allotted} allotment.";
        public override Retriability Retriability => Retriability.Transient;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LeasePolicy(Duration CrashStaleness) {
    public static readonly LeasePolicy Maintenance = new(CrashStaleness: Duration.FromSeconds(120));

    public static Unit Outlasts => OutlastsProof.Value;

    private static readonly Lazy<Unit> OutlastsProof = new(static () =>
        Maintenance.CrashStaleness > DeadlineClass.DrainCooperative.Allotted + DeadlineClass.DrainForced.Allotted
            ? unit
            : throw new InvalidOperationException($"{nameof(LeasePolicy)}.{nameof(Maintenance)}"),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

public sealed record ScheduleEntry(
    string Key,
    OccurrenceSpec Spec,
    DeadlineClass Deadline,
    Option<LeasePolicy> Lease,
    RedrivePolicy Redrive,
    Func<IO<Unit>> Work) {
    public static int Seed(string key) =>
        unchecked((int)ContentHash.Half(
            digest: ContentHash.Of(key, static (text, writer) => writer.String(text)),
            lane: Lane.Low));

    public static Fin<ScheduleEntry> Spread(
        string key,
        string template,
        CronFormat format,
        DeadlineClass deadline,
        Option<LeasePolicy> lease,
        RedrivePolicy redrive,
        Func<IO<Unit>> work) =>
        OccurrenceSpec.Admit(text: template, format: format, jitterSeed: Some(Seed(key: key)))
            .Map(spec => new ScheduleEntry(key, spec, deadline, lease, redrive, work));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SchedulePort {
    const int AnnualLookahead = 2;

    public static Option<Instant> Next(ScheduleEntry entry, Instant after) =>
        entry.Spec.Switch(
            cron:  static (s, c) => Optional(c.Expression.GetNextOccurrence(s.ToDateTimeOffset(), TimeZoneInfo.Utc))
                .Map(static next => next.ToInstant()),
            every: static (s, e) => Optional(s + e.Period),
            annual: static (s, a) => AnnualNext(s, a.Date, a.At, a.Zone),
            state: after);

    public static IO<(Fin<Unit> Outcome, GaugedSpan<DeadlineClass> Span)> Run(ClockPolicy clocks, ScheduleEntry entry, Op? key = null) =>
        IO.lift(() => clocks.Gauged<Unit>(
                row: entry.Deadline,
                work: key.OrDefault(),
                body: () => Op.Of().Catch(() => Fin.Succ(Redrive.Run(policy: entry.Redrive, work: Occurrence(entry: entry)).Run()))))
          .Bind(static gauged => gauged.Match(
              Succ: static measured => IO.pure((measured.Value, measured.Span)),
              Fail: IO.fail<(Fin<Unit> Outcome, GaugedSpan<DeadlineClass> Span)>));

    static IO<Unit> Occurrence(ScheduleEntry entry) =>
        entry.Work()
            .Timeout(entry.Deadline.Allotted.ToTimeSpan())
            .Catch(static error => error.Is(Errors.TimedOut) || error.Is(Errors.Cancelled),
                   _ => IO.fail<Unit>(new TimeFault.OccurrenceTimedOut(Key: entry.Key, Allotted: entry.Deadline.Allotted)));

    public static Fin<Seq<Instant>> Missed(ScheduleEntry entry, Instant lastFired, Instant now) =>
        Bounded(key: entry.Key, bound: entry.Redrive.Bound, occurrences: entry.Spec.Switch<(Instant From, Instant To, int Bound), IEnumerable<Instant>>(
            cron: static (s, c) => c.Expression
                .GetOccurrences(s.From.ToDateTimeOffset(), s.To.ToDateTimeOffset(), TimeZoneInfo.Utc)
                .Map(static at => at.ToInstant()),
            every: static (s, e) => Range(1, Capped(Counted(s.From, s.To, e.Period), s.Bound))
                .Map(step => s.From + e.Period * step),
            annual: static (s, a) => AnnualWindow(s.From, s.To, a.Date, a.At, a.Zone),
            state: (From: lastFired, To: now, Bound: entry.Redrive.Bound)));

    public static Option<SupportTrigger> Heartbeat(CorrelationId correlation, ScheduleEntry entry, GaugedSpan<DeadlineClass> span) =>
        DeadlineOutcome.Of(span) == DeadlineOutcome.Met
            ? None
            : Some<SupportTrigger>(new SupportTrigger.Timed(
                correlation, SupportTriggerKind.WatchdogTimeout, entry, span));

    static Fin<Seq<Instant>> Bounded(string key, int bound, IEnumerable<Instant> occurrences) =>
        toSeq(occurrences.Take(bound + 1)).Strict() switch {
            var taken when taken.Count > bound => Fin.Fail<Seq<Instant>>(new TimeFault.WindowExhausted(Key: key, Bound: bound)),
            var taken => Fin.Succ(taken),
        };

    static Int128 Counted(Instant from, Instant to, Duration period) =>
        period > Duration.Zero ? (to - from).ToInt128Nanoseconds() / period.ToInt128Nanoseconds() : Int128.Zero;

    static int Capped(Int128 steps, int bound) => (int)Int128.Clamp(steps, Int128.Zero, bound + 1);

    static Option<Instant> AnnualNext(Instant after, AnnualDate date, LocalTime at, DateTimeZone zone) =>
        Range(0, AnnualLookahead).ToSeq()
            .Map(step => date.InYear(after.InZone(zone).Year + step).At(at).InZoneStrictly(zone).ToInstant())
            .Filter(candidate => candidate > after)
            .Head;

    static Seq<Instant> AnnualWindow(Instant from, Instant to, AnnualDate date, LocalTime at, DateTimeZone zone) =>
        Range(from.InZone(zone).Year, to.InZone(zone).Year - from.InZone(zone).Year + 1).ToSeq()
            .Map(year => date.InYear(year).At(at).InZoneStrictly(zone).ToInstant())
            .Filter(candidate => candidate > from && candidate <= to);
}
```

The watchdog spine composes end to end without a watchdog service type: `Run` returns the heartbeat row's paired work outcome and gauged span, `Heartbeat` folds a missed bound into `SupportTrigger.Timed` under the watchdog kind carrying the same `ScheduleEntry` and span, and the support owner consumes that trigger. The heartbeat period is one policy value fixed at three times the health-probe row, and one heartbeat row exists per watched child or peer. Maintenance work executes only while the registering process holds the maintenance lease; `LeasePolicy.Maintenance` carries the reclamation value both release routes share. A fleet-spread row registers through `ScheduleEntry.Spread` so its cadence carries a deterministic `ContentHash`-derived seed off the row key — every fleet node computes the identical seed from the shared key, the `H` field distributes the nodes across the cadence window, and each run returns its gauged span through the same `Run` path with no fleet-specific instrument.

## [05]-[FENCING_TOKEN]

- Owner: `FencingToken` `[ValueObject<ulong>]` — the decoded carrier of the store-issued lease generation; `LeaseElection` the ONE Persistence PORT adapter acquiring, renewing, and fencing through the coordination op-union; `FenceVerb` the four-row lease-transition roster, each row binding its own store arrow as a delegate column; `FenceHolding<TKey>`/`FenceStep<TKey>` the held state and its transition; `FencedRuntime` the one dependency capsule every fenced holder threads; `FencedLease<TKey>` the algebra over all of them.
- Law: this fence DETECTS and each guarded write REJECTS — `Fence` binds the coordination `LeaseGuard` READ case, which validates the held generation against the lease row and mutates nothing, so a passing guard proves the lease stood at the read instant and proves nothing about the write that follows; the authoritative rejection is the `WHERE token >= held` predicate inside each guarded write's own statement, re-evaluated by the engine against the committed row version, and `Wire/coordination#DISTRIBUTED_LOCK` `DistributedLock.Guard` brackets its critical section with that read on both sides to DETECT a stolen lock rather than to admit the section — a guard evaluated apart from the write it protects passes on a generation another writer already moved.
- Entry: `Acquire(LeaseElection.Runtime runtime, string leaseKey)` returns `Fin<FencingToken>` — the adapter calls the Persistence coordination op-union with wire-stable primitives (lease key, holder id, staleness millis) and DECODES the store-issued `LeaseToken` generation into the carrier; `Fence(runtime, leaseKey, held)` reads the lease row through `LeaseGuard` and answers whether the held generation still stands; `Admits(FencingToken incoming)` survives ONLY as a documented client-side pre-check that short-circuits an obviously-stale retry before the round-trip, never a substitute for the store's verdict.
- Entry: `FencedLease<TKey>.Acquire(runtime, key, correlation)` mints a holding, `Fenced(runtime, holding, verb)` applies one post-acquisition verb, and `Guard(runtime, holding, section)` brackets a critical section with the guard read on both sides — each answering `IO<Fin<FenceStep<TKey>>>` over the namespaced key its caller holds.
- Law: RENEW RE-ISSUES. `LeaseElection.Renew` decodes a fresh generation, so the verb column answers `Fin<Option<FencingToken>>` and the holding advances onto what the store issued — a verb answering `Fin<Unit>` leaves every holder fencing on the generation it acquired with while the store has already moved past it, which is a stale-token rejection the holder cannot explain.
- Law: `TKey` is the caller's own namespaced key value object under `Thinktecture.IConvertible<string>`, so the store's lease key has exactly one author per namespace and a bare interpolation cannot reach the adapter; `Wire/coordination#ROLE_ELECTION` and `#DISTRIBUTED_LOCK` are two `LeaseKey` namespaces over this one fold, never two algebras.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: a new fenced resource carries the same decoded token through the same `Fence` adapter, never a second token type; a new election driver acquires through the same `Acquire`; a new lease transition is one `FenceVerb` row with its own store arrow, breaking every reader that folds the roster; zero new surface.
- Boundary: the fence is store-validated or it is not fencing — a per-process token mint, an in-memory latest-token cell, and an in-memory fence validation are the DELETED forms (two processes minting independent sequences is zero cross-process safety); the store issues the strictly-increasing generation as its fenced-lease column, so a generation of zero names no issued lease and the factory refuses it rather than seating a genesis row that reads as a held fence to every comparison — NAMED LOSS, the `Zero` row deletes and an unacquired holder carries `Option<FencingToken>` instead; a paused-then-resumed stale holder's late write rejects store-side and surfaces as `Wire/coordination#DISTRIBUTED_LOCK` `CoordinationFault.FenceRejected(key, cause)` carrying the store's verdict as its inner cause — the Persistence-band `LeaseFenced` stays store-side per the two-formed-pair law, never a bare `Error.New` minted here; requests cross as wire-stable primitives and results decode from Persistence-owned types — no AppHost interface or type crosses down; the election reuses the `LeasePolicy.Maintenance` `CrashStaleness` window as the lease timeout but the store-validated token is the correctness proof the timeout alone cannot give; the token crosses the wire as the same decimal-string width as the HLC `Logical` half so the op-log cursor and the fence read one monotone identity; the maintenance-lease election at SCHEDULE_PORT, the `Sandbox/provisioning#ROLLOVER_DRAIN` conductor election, and the sidecar `Wire/companion#PROCESS_MODALITY` write-forward each acquire through this one rail — the store (`Rasm.Persistence` `ONE_FENCED_LEASE_STORE`) fences every token, aligned to a sibling branch, never coupled; a held lease without a store-validated token is the rejected form; `FencedLease` is the ONE acquire-renew-guard-release fold on the spine and every fenced holder threads `FencedRuntime` — a per-consumer election runtime, a per-consumer lock runtime, and a second holding record over these same four store arrows are the deleted forms, and the verb a transition carries is the roster row that performed it rather than a string its caller remembered to pass.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FenceVerb {
    public static readonly FenceVerb Acquire = new("acquire",
        static (runtime, key, _) => LeaseElection.Acquire(runtime, key).Map(Some));
    public static readonly FenceVerb Renew = new("renew",
        static (runtime, key, held) => Held(held, token => LeaseElection.Renew(runtime, key, token).Map(Some)));
    public static readonly FenceVerb Guard = new("guard",
        static (runtime, key, held) => Held(held, token => LeaseElection.Fence(runtime, key, token).Map(static _ => Option<FencingToken>.None)));
    public static readonly FenceVerb Release = new("release",
        static (runtime, key, held) => Held(held, token => LeaseElection.Release(runtime, key, token).Map(static _ => Option<FencingToken>.None)));

    [UseDelegateFromConstructor]
    public partial Fin<Option<FencingToken>> Fence(LeaseElection.Runtime runtime, string leaseKey, Option<FencingToken> held);

    static Fin<Option<FencingToken>> Held(Option<FencingToken> held, Func<FencingToken, Fin<Option<FencingToken>>> arm) =>
        held.Match(Some: arm, None: static () => Fin.Fail<Option<FencingToken>>(
            new KernelFault.InvalidValue(Label: nameof(FencingToken), Requirement: "a held generation")));
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<ulong>(
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
public readonly partial struct FencingToken {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref ulong value) =>
        validationError = value == 0UL
            ? new ValidationError(message: "FencingToken requires a store-issued generation above zero.")
            : null;

    public bool Admits(FencingToken incoming) => incoming >= this;
}

public sealed record FenceHolding<TKey>(TKey Key, int Holder, FencingToken Token, Instant LeaseDeadline, CorrelationId Correlation)
    where TKey : notnull;

public readonly record struct FenceStep<TKey>(FenceHolding<TKey> Holding, FenceVerb Verb) where TKey : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public static class LeaseElection {
    public sealed record Runtime(
        Func<string, LeasePolicy, Fin<(ulong Generation, Instant Deadline)>> AcquireLease,
        Func<string, LeasePolicy, ulong, Fin<(ulong Generation, Instant Deadline)>> RenewLease,
        Func<string, ulong, Fin<Unit>> GuardWrite,
        Func<string, ulong, Fin<Unit>> ReleaseLease,
        LeasePolicy Lease);

    // --- [OPERATIONS]
    public static Fin<FencingToken> Acquire(Runtime runtime, string leaseKey) =>
        runtime.AcquireLease(leaseKey, runtime.Lease).Bind(static grant => Decoded(grant.Generation));

    public static Fin<FencingToken> Renew(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.RenewLease(leaseKey, runtime.Lease, (ulong)held).Bind(static grant => Decoded(grant.Generation));

    public static Fin<Unit> Fence(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.GuardWrite(leaseKey, (ulong)held);

    public static Fin<Unit> Release(Runtime runtime, string leaseKey, FencingToken held) =>
        runtime.ReleaseLease(leaseKey, (ulong)held);

    static Fin<FencingToken> Decoded(ulong generation) =>
        Op.Of().AcceptValidated<FencingToken, ulong>(generation);
}

public sealed record FencedRuntime(
    int NodeId,
    LeaseElection.Runtime Lease,
    ClockPolicy Clocks,
    Duration Staleness);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FencedLease<TKey> where TKey : notnull, Thinktecture.IConvertible<string> {
    public static IO<Fin<FenceStep<TKey>>> Acquire(FencedRuntime runtime, TKey key, CorrelationId correlation) =>
        Run(runtime, FenceVerb.Acquire, key, None, correlation);

    public static IO<Fin<FenceStep<TKey>>> Fenced(FencedRuntime runtime, FenceHolding<TKey> holding, FenceVerb verb) =>
        Run(runtime, verb, holding.Key, Some(holding), holding.Correlation);

    public static IO<Fin<A>> Guard<A>(FencedRuntime runtime, FenceHolding<TKey> holding, IO<A> section) =>
        from opened in Fenced(runtime, holding, FenceVerb.Guard)
        from ran in opened.Match(Succ: _ => section.Map(Fin.Succ), Fail: error => IO.pure(Fin.Fail<A>(error)))
        from closed in Fenced(runtime, holding, FenceVerb.Guard)
        select from value in ran from _ in closed select value;

    static IO<Fin<FenceStep<TKey>>> Run(
        FencedRuntime runtime, FenceVerb verb, TKey key, Option<FenceHolding<TKey>> prior, CorrelationId correlation) =>
        IO.lift(() => verb.Fence(runtime.Lease, key.ToValue(), prior.Map(static held => held.Token))
            .Bind(issued => issued.Match(Some: Some, None: () => prior.Map(static held => held.Token)).Match(
                Some: token => Fin.Succ(new FenceStep<TKey>(
                    new FenceHolding<TKey>(
                        Key: key,
                        Holder: runtime.NodeId,
                        Token: token,
                        LeaseDeadline: issued.Match(
                            Some: _ => runtime.Clocks.Now + runtime.Staleness,
                            None: () => prior.Map(static held => held.LeaseDeadline).IfNone(runtime.Clocks.Now)),
                        Correlation: correlation),
                    verb)),
                None: () => Fin.Fail<FenceStep<TKey>>(new KernelFault.InvalidValue(
                    Label: nameof(FencingToken), Requirement: "a held or store-issued generation")))));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
