# [APPHOST_HEALTH_AND_DEGRADATION]

Capability health and the usable-failure degradation rail for every Rasm.AppHost process: a health-contributor row family folds package probes into one wire-neutral snapshot, a `DriverProbe`-keyed adapter binds every admitted backing-service health check (Postgres, Redis, Kafka, upstream HTTP, disk, allocation) onto its degradation rule through one shared driver instance, the five-level DegradationLevel vocabulary carries one retained-capability set per row, and a wire-health mapping projects the registry onto the standard wire health service. Microsoft.Extensions.Diagnostics.HealthChecks supplies probe mechanics, the AspNetCore.HealthChecks.NpgSql/Redis/Kafka/Uris/System family supplies the concrete backing-service probes, ResourceMonitoring publishes CPU/memory utilization and container limits through its OTel observable instruments and ResourceQuotaProvider, Thinktecture owns the vocabularies, LanguageExt and NodaTime carry the fold rails and stamps; every consumer reads one level value.

## [01]-[INDEX]

- [01]-[HEALTH_FOLD]: Contributor rows, resource pressure, peer reads, and one snapshot fold.
- [02]-[DEGRADATION_RAIL]: Level vocabulary, retained capabilities, derivation fold, and hysteresis.
- [03]-[WIRE_HEALTH]: Tag-predicate wire mapping and the inbound set-degradation route.
- [04]-[ALERT_ENGINE]: Declarative alert rules over continuous queries with hysteresis, escalation, and versioning.
- [05]-[TS_PROJECTION]: Health snapshot, degradation level, and alert wire shapes.

## [02]-[HEALTH_FOLD]

- Owner: `HealthContributorRow` is the probe row and the `IHealthCheck`; `DriverProbe` `[SmartEnum<string>]` is the backing-service probe axis carrying each dependency kind's contributor tag and failure status; `PressurePolicy` grades utilization with a `ResourceQuota` container-limit column; `UtilizationCell` is the `MeterListener`-backed boundary capsule that records the ResourceMonitoring observable instruments and grades on read; `HealthSnapshot` with nested `Entry` is the only health shape interiors read.
- Cases: tag consts `Host`, `Remote`, `Store`, `Pressure` key the derivation rules and the wire predicates; instrument-name consts `CpuInstrument` and `MemoryInstrument` key the meter subscription; eight `DriverProbe` rows tracking the LANDED Persistence sink roster — `Postgres`/`Cache` (Store), `Nats`/`Upstream` (Remote), `Disk`/`Allocations` (Pressure) as DEFAULT probes, `Kafka`/`Redis` as deploy-gated sink-tracking rows registering only when their sink is bound — bind each admitted health-check package to its degradation rule; `Gauge`, `Peer`, and `Driver` are the canonical row factories and `Monitor` is the resource-monitoring registration fold.
- Entry: `Register(params ReadOnlySpan<HealthContributorRow> rows)` composes registrations; `Snapshot(Instant at, CorrelationId correlation)` is the pure report fold.
- Auto: rows project into `HealthCheckRegistration` — `FailureStatus`, `Tags`, `Timeout`, `Delay`, `Period` are registration policy, never probe-local exception handling; `Driver(DriverProbe, cadence, IHealthCheck)` adapts ANY admitted package check — `NpgSqlHealthCheck` over the pooled `NpgsqlDataSource`, the `Cache` row's L2-transit probe over the ONE `IDistributedCache` the `Runtime/resources#CACHE_PORT` rides (through `Microsoft.Extensions.Caching.StackExchangeRedis` when the deploy binds redis — the raw `IConnectionMultiplexer` driver is PRUNED Persistence-side, so a direct-multiplexer probe is the deleted form), `NatsHealthCheck` over the pooled `INatsConnection` (the spine's landed NATS egress sink, exactly as the npgsql row binds its data source), the deploy-gated `KafkaHealthCheck` over its sink `ProducerConfig` and `RedisHealthCheck` over its sink connection, `UriHealthCheck` over a service-discovery `AddUrlGroup`, and `DiskStorageHealthCheck`/`ProcessAllocatedMemoryHealthCheck` over the BCL counters — into one contributor row whose synthetic `HealthCheckContext` seats the `DriverProbe.FailureStatus` the package check stamps, so the packages enter as rows through one adapter rather than parallel `Add*` registration faces; `Monitor` folds the policy onto `AddResourceMonitoring`, binding `CollectionWindow` to `PressurePolicy.Canonical.Window`, `SamplingInterval` to `PressurePolicy.Canonical.Sampling`, and `PublishingWindow`, `CpuConsumptionRefreshInterval`, and `MemoryConsumptionRefreshInterval`, setting `UseLinuxCalculationV2` from `CgroupV2` and `UseZeroToOneRangeForLinuxMetrics` so CPU rides the normalized zero-to-one range the grade reads, and turning on `EnableSystemDiskIoMetrics` from `DiskIoMetrics` for the disk-I/O instruments; `UtilizationCell.Attach` opens a `MeterListener` on the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, enables `process.cpu.utilization` and `dotnet.process.memory.virtual.ratio`, and records each observed double into the atom, the publishing window bounding every derived signal's reaction time.
- Receipt: `HealthSnapshot` stamped with `Instant` and `CorrelationId`; `HealthReport` never crosses the fold.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.ResourceMonitoring, AspNetCore.HealthChecks.NpgSql, AspNetCore.HealthChecks.Nats, AspNetCore.HealthChecks.Redis, AspNetCore.HealthChecks.Kafka, AspNetCore.HealthChecks.Uris, AspNetCore.HealthChecks.System, NodaTime, LanguageExt.Core
- Growth: one contributor row per new capability probe — sibling packages extend the same `Register` span through the health port registration set, zero new surface; a new backing-service dependency is one `DriverProbe` row binding its tag and failure status, never a new factory; container-limit grading is one `ResourceQuota` value flip on `PressurePolicy.Quota`, a sampling retune is one `Sampling` value, and a new utilization signal is one enabled instrument name on `UtilizationCell.Attach`, never a parallel policy.
- Boundary: package health types stop at this seam — interiors read `HealthSnapshot` and one level value; a `Driver` row binds the SAME pooled `NpgsqlDataSource`, the one L2 `IDistributedCache` transit, and the pooled `INatsConnection` the production path owns, so a probe shares connection pressure with live traffic and never opens a second out-of-pool connection or invents a parallel connection vocabulary — the roster is seed DATA tracking the landed Persistence egress sink roster (NATS the default spine anchor; kafka/redis deploy-gated sink rows, never default probes), so the probe axis never drifts beside the roster it probes, and its tag routes a faulted dependency onto an EXISTING degradation rule (`Store` -> `ReadOnly`, `Remote` -> `ReducedRemote`, `Pressure` -> `Degraded`) with zero added `Rule`; the `Disk`/`Allocations` probes are the discrete hard-ceiling complement to the continuous `UtilizationCell` gauge, not a second utilization source — they grade an absolute breach the windowed ratio does not express, both projecting into the one `Pressure`-tagged contributor set; `Peer` rows read a peer process over its wire health service, so cross-process health is a read, never shared state; `Gauge` grades against the container limit when `PressurePolicy.Quota` carries a `ResourceQuota` — the OCI `headless` and `web` rows set `CgroupV2` so `Monitor` turns on `UseLinuxCalculationV2` and `UseZeroToOneRangeForLinuxMetrics`, folding the cgroup `MaxCpuInCores` and `MaxMemoryInBytes` ceilings into the same zero-to-one ratio axis the meter publishes, so a cgroup-throttled process degrades on the limit it actually runs under, never the host total; `ResourceQuota` carries the `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings, and whether the `process.cpu.utilization`/`dotnet.process.memory.virtual.ratio` instruments arrive already quota-normalized under that flag or require a `ResourceQuotaProvider.GetResourceQuota()` recompute rides `[MEMORY_RATIO_SEMANTIC]`; a bare host-ratio grade on a container row is the deleted form. Utilization crosses as observable-instrument readings and container limits as `ResourceQuota`, so the meter subscription is the one pull path.

```csharp signature
public sealed record HealthContributorRow(
    string Name,
    Func<CancellationToken, ValueTask<HealthCheckResult>> Probe,
    HealthStatus FailureStatus,
    FrozenSet<string> Tags,
    DeadlineClass Timeout,
    Duration Delay,
    Duration Period) : IHealthCheck {
    public const string Host = "host";
    public const string Remote = "remote";
    public const string Store = "store";
    public const string Pressure = "pressure";

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Probe(cancellationToken).AsTask();
    public static FrozenSet<string> TagSet(params ReadOnlySpan<string> tags) =>
        tags.ToArray().ToFrozenSet(StringComparer.Ordinal);

    public static HealthContributorRow Gauge(UtilizationCell cell, PressurePolicy policy) => new(
        Name: nameof(Gauge),
        Probe: _ => ValueTask.FromResult(policy.Grade(cell.Read())),
        FailureStatus: HealthStatus.Degraded,
        Tags: TagSet(Pressure),
        Timeout: DeadlineClass.HealthProbe,
        Delay: policy.Window,
        Period: policy.Window);

    public static IServiceCollection Monitor(IServiceCollection services, PressurePolicy policy) =>
        services.AddResourceMonitoring(options => {
            options.CollectionWindow = policy.Window.ToTimeSpan();
            options.SamplingInterval = policy.Sampling.ToTimeSpan();
            options.PublishingWindow = policy.Window.ToTimeSpan();
            options.CpuConsumptionRefreshInterval = policy.Window.ToTimeSpan();
            options.MemoryConsumptionRefreshInterval = policy.Window.ToTimeSpan();
            options.EnableSystemDiskIoMetrics = policy.DiskIoMetrics;
            options.UseLinuxCalculationV2 = policy.CgroupV2;
            options.UseZeroToOneRangeForLinuxMetrics = true;
        });

    public static HealthContributorRow Peer(string name, string tag, Duration cadence, Func<CancellationToken, ValueTask<HealthCheckResult>> probe) => new(
        Name: name,
        Probe: probe,
        FailureStatus: HealthStatus.Unhealthy,
        Tags: TagSet(Remote, tag),
        Timeout: DeadlineClass.HealthProbe,
        Delay: cadence,
        Period: cadence);

    // The one driver-probe adapter: any admitted Xabaril IHealthCheck (Npgsql/Redis/Kafka/Uris/System)
    // becomes one contributor row carrying the DriverProbe row's tag and failure status, never a parallel
    // AddNpgSql/AddRedis/AddKafka/AddUrlGroup registration scatter. The synthetic HealthCheckContext seats
    // the failure status the package check stamps; the shared driver instance (pooled NpgsqlDataSource,
    // the one L2 IDistributedCache transit, the pooled INatsConnection) is the one the production path owns.
    public static HealthContributorRow Driver(DriverProbe probe, Duration cadence, IHealthCheck check) {
        var context = new HealthCheckContext {
            Registration = new HealthCheckRegistration(probe.Key, check, probe.FailureStatus, TagSet(probe.Tag), DeadlineClass.HealthProbe.Allotted.ToTimeSpan()),
        };
        return new(
            Name: probe.Key,
            Probe: ct => new ValueTask<HealthCheckResult>(check.CheckHealthAsync(context, ct)),
            FailureStatus: probe.FailureStatus,
            Tags: TagSet(probe.Tag),
            Timeout: DeadlineClass.HealthProbe,
            Delay: cadence,
            Period: cadence);
    }
}

// The backing-service probe axis: one row per dependency kind carries the contributor tag and failure
// status that route a faulted dependency onto its existing degradation rule — Postgres/Redis to Store
// (-> ReadOnly), Kafka/upstream HTTP to Remote (-> ReducedRemote), disk/allocation ceilings to Pressure
// (-> Degraded). Adding a backing-service kind is one row; the degradation rules are untouched.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DriverProbe {
    // The roster TRACKS the landed Persistence sink roster — a sink admitted Persistence-side lands
    // here as one row. Default rows probe unconditionally; a sink-tracking row (Default: false) is
    // deploy-gated and registers only when its sink is bound at the composition root.
    public static readonly DriverProbe Postgres = new("npgsql", HealthContributorRow.Store, HealthStatus.Unhealthy, defaultRow: true);
    public static readonly DriverProbe Cache = new("cache-l2", HealthContributorRow.Store, HealthStatus.Unhealthy, defaultRow: true);
    public static readonly DriverProbe Nats = new("nats", HealthContributorRow.Remote, HealthStatus.Unhealthy, defaultRow: true);
    public static readonly DriverProbe Kafka = new("kafka", HealthContributorRow.Remote, HealthStatus.Unhealthy, defaultRow: false);
    public static readonly DriverProbe Redis = new("redis", HealthContributorRow.Store, HealthStatus.Unhealthy, defaultRow: false);
    public static readonly DriverProbe Upstream = new("uris", HealthContributorRow.Remote, HealthStatus.Unhealthy, defaultRow: true);
    public static readonly DriverProbe Disk = new("diskstorage", HealthContributorRow.Pressure, HealthStatus.Degraded, defaultRow: true);
    public static readonly DriverProbe Allocations = new("process_allocated_memory", HealthContributorRow.Pressure, HealthStatus.Degraded, defaultRow: true);

    public string Tag { get; }
    public HealthStatus FailureStatus { get; }
    public bool Default { get; }
}

public readonly record struct Utilization(double CpuRatio, double MemoryRatio);

public sealed record PressurePolicy(
    Duration Window,
    Duration Sampling,
    double CpuDegraded,
    double CpuUnhealthy,
    double MemoryDegraded,
    double MemoryUnhealthy,
    Option<ResourceQuota> Quota,
    bool CgroupV2,
    bool DiskIoMetrics) {
    public static readonly PressurePolicy Canonical = new(
        Window: Duration.FromSeconds(5), Sampling: Duration.FromSeconds(1),
        CpuDegraded: 0.80d, CpuUnhealthy: 0.92d, MemoryDegraded: 0.85d, MemoryUnhealthy: 0.95d,
        Quota: None, CgroupV2: false, DiskIoMetrics: false);

    public static PressurePolicy Container(ResourceQuota quota) =>
        Canonical with { Quota = Optional(quota), CgroupV2 = true, DiskIoMetrics = true };

    public HealthCheckResult Grade(Utilization usage) =>
        (Cpu: usage.CpuRatio, Memory: usage.MemoryRatio) switch {
            var load when load.Cpu >= CpuUnhealthy || load.Memory >= MemoryUnhealthy =>
                HealthCheckResult.Unhealthy($"cpu {load.Cpu:P0} memory {load.Memory:P0}"),
            var load when load.Cpu >= CpuDegraded || load.Memory >= MemoryDegraded =>
                HealthCheckResult.Degraded($"cpu {load.Cpu:P0} memory {load.Memory:P0}"),
            _ => HealthCheckResult.Healthy(),
        };
}

public sealed class UtilizationCell : IDisposable {
    public const string Meter = "Microsoft.Extensions.Diagnostics.ResourceMonitoring";
    public const string CpuInstrument = "process.cpu.utilization";
    public const string MemoryInstrument = "dotnet.process.memory.virtual.ratio";
    private readonly Atom<Utilization> cell = Atom(new Utilization(0d, 0d));
    private readonly MeterListener listener = new();

    public Utilization Read() => cell.Value;

    public UtilizationCell Attach() {
        listener.InstrumentPublished = (instrument, active) => {
            if (instrument.Meter.Name == Meter && instrument.Name is CpuInstrument or MemoryInstrument)
                active.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
            cell.Swap(current => instrument.Name switch {
                CpuInstrument => current with { CpuRatio = measurement },
                MemoryInstrument => current with { MemoryRatio = measurement },
                _ => current,
            }));
        listener.Start();
        return this;
    }

    public void Dispose() => listener.Dispose();
}

public sealed record HealthSnapshot(
    HealthStatus Status,
    Instant At,
    CorrelationId Correlation,
    Seq<HealthSnapshot.Entry> Entries) {
    public readonly record struct Entry(
        string Name,
        HealthStatus Status,
        Duration Elapsed,
        FrozenSet<string> Tags,
        Option<string> Detail);
}

public static class HealthSurface {
    extension(IHealthChecksBuilder builder) {
        public IHealthChecksBuilder Register(params ReadOnlySpan<HealthContributorRow> rows) =>
            Iterable<HealthContributorRow>.FromSpan(rows).Fold(builder, static (admitted, row) =>
                admitted.Add(new HealthCheckRegistration(row.Name, _ => row, row.FailureStatus, row.Tags, row.Timeout.Allotted.ToTimeSpan()) {
                    Delay = row.Delay.ToTimeSpan(),
                    Period = row.Period.ToTimeSpan(),
                }));
    }
    extension(HealthReport report) {
        public HealthSnapshot Snapshot(Instant at, CorrelationId correlation) =>
            new(report.Status, at, correlation,
                report.Entries.AsIterable().Map(static entry => new HealthSnapshot.Entry(
                    entry.Key,
                    entry.Value.Status,
                    Duration.FromTimeSpan(entry.Value.Duration),
                    HealthContributorRow.TagSet([.. entry.Value.Tags]),
                    Optional(entry.Value.Description))).ToSeq());
    }
}
```

## [03]-[DEGRADATION_RAIL]

- Owner: `Capability` and `DegradationLevel` vocabularies under the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `DegradationPolicy` with nested `Rule` rows is the derivation table; `DegradationState` is the fold receipt; `DegradationReading` is the coherent `(snapshot, state)` pair; `DegradationCell` is the boundary capsule owning the one atom cell and the publisher seam.
- Cases: `Full(0)`, `ReducedRemote(1)`, `LocalOnly(2)`, `ReadOnly(3)`, `Suspended(4)` in severity order; six `Capability` keys form the retained sets.
- Entry: `Derive(DegradationState state, HealthSnapshot snapshot)` folds rules with escalation-immediate, recovery-hysteresis semantics; `Force(Option<DegradationLevel> forced)` is the single override entrypoint; `Cascade(Option<DegradationLevel> parent)` admits a parent-forced level as a derivation floor; `Read()` returns the one `DegradationReading` carrying the snapshot that produced the level and the derived `DegradationState` in one coherent value.
- Auto: `DegradationCell` registers as the `IHealthCheckPublisher` and owns one `Atom<DegradationReading>` — `PublishAsync` snapshots the `HealthReport` and folds `Derive` in the SAME swap, so the published snapshot and the level it produced are one atomic transition and a reader can never observe a fresh level against a stale snapshot or the reverse; `HealthCheckPublisherOptions` binds `Delay` and `Period` from `DegradationPolicy.Canonical` and `Timeout` from `DeadlineClass.HealthProbe`; `OperatorOverride` projects onto `Force` at the composition root — forced beats derived, release re-derives; `Force` and `Cascade` swap the `State` slot of the reading while preserving the last snapshot so the override is coherent with the evidence it overrides.
- Receipt: `DegradationReading` carries the latest `HealthSnapshot` and the `DegradationState` (derived level, forced input, cascade floor, recovery streak, dwell anchor); a `Level` change rides the lifecycle transition receipt as the degraded trigger.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `Rule` row or one `Capability` case absorbs a new degradation driver; a new level is one `DegradationLevel` item; a new pressure-read consumer reads the one `DegradationReading` value, never a second cell — zero new surface.
- Boundary: degradation is process-local, peer-health-informed, and parent-cascade-floored — a peer level never propagates as this process's level, but a parent process's forced level enters `Derive` as a floor through `Cascade`, never as shared state; the snapshot and the derived level are one `DegradationReading` atom so the `Runtime/laneguard#LANE_GUARD` governor reads a coherent `(snapshot, level)` pressure value for its adaptive-concurrency and load-shed decisions — the prior two-surface read (a `HealthSnapshot.Snapshot` independent of a `DegradationState.Derive`) is the collapsed form, and a governor reading a stale snapshot against a fresh level is the race the single atom forecloses; `LocalOnly` is the host-absent fold: `Capability.HostDocument` gates off and document sources yield absence; the container-limit pressure signal enters the rank algebra as data, not a new rule — a `PressurePolicy.Container` row grades against `ResourceQuota` so the `Pressure`-tagged `Gauge` row escalates on the cgroup limit, and the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules carry that limit-relative status into `Derive` with the same retained-set hysteresis, so a container-throttled process degrades and recovers on its own limit with zero added `Rule` row. The cross-process cascade is a seam-split the snapshot fold preserves: the READ — this process's own `DegradationReading` — stays the owner here; the WRITE — a parent fanning its level to a child over the control hop — lands at `Wire/companion#DEGRADATION_CASCADE`, which calls `Cascade` with the observed parent level mutating only the `State` slot; release passes `None` and the cell re-derives off its own snapshots, the cascade floor never escalating below local pressure, so folding the snapshot into the cell preserves the cascade-floor and hysteresis fields and never merges the parent-cascade owner.

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class Capability {
    public static readonly Capability HostDocument = new("host-document");
    public static readonly Capability RemoteCompute = new("remote-compute");
    public static readonly Capability LocalCompute = new("local-compute");
    public static readonly Capability StoreWrite = new("store-write");
    public static readonly Capability StoreRead = new("store-read");
    public static readonly Capability TelemetryExport = new("telemetry-export");

    public static FrozenSet<Capability> Set(params ReadOnlySpan<Capability> items) => items.ToArray().ToFrozenSet();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DegradationLevel {
    public static readonly DegradationLevel Full = new("full", rank: 0, retains: Capability.Set(Capability.HostDocument, Capability.RemoteCompute, Capability.LocalCompute, Capability.StoreWrite, Capability.StoreRead, Capability.TelemetryExport));
    public static readonly DegradationLevel ReducedRemote = new("reduced-remote", rank: 1, retains: Capability.Set(Capability.HostDocument, Capability.LocalCompute, Capability.StoreWrite, Capability.StoreRead, Capability.TelemetryExport));
    public static readonly DegradationLevel LocalOnly = new("local-only", rank: 2, retains: Capability.Set(Capability.LocalCompute, Capability.StoreWrite, Capability.StoreRead, Capability.TelemetryExport));
    public static readonly DegradationLevel ReadOnly = new("read-only", rank: 3, retains: Capability.Set(Capability.LocalCompute, Capability.StoreRead, Capability.TelemetryExport));
    public static readonly DegradationLevel Suspended = new("suspended", rank: 4, retains: Capability.Set(Capability.StoreRead, Capability.TelemetryExport));
    public int Rank { get; }
    public FrozenSet<Capability> Retains { get; }
    public bool Permits(Capability capability) => Retains.Contains(capability);
}

public readonly record struct DegradationState(
    DegradationLevel Derived,
    Option<DegradationLevel> Forced,
    Option<DegradationLevel> Cascade,
    int Streak,
    Option<Instant> Since) {
    public static readonly DegradationState Boot = new(DegradationLevel.Full, None, None, Streak: 0, Since: None);
    public DegradationLevel Floor =>
        Cascade.Match(parent => parent.Rank > Derived.Rank ? parent : Derived, () => Derived);
    public DegradationLevel Level => Forced.IfNone(Floor);
}

public sealed record DegradationPolicy(
    Seq<DegradationPolicy.Rule> Rules,
    int ConsecutiveHealthy,
    Duration MinimumDwell,
    Duration PublishDelay,
    Duration PublishPeriod) {
    public readonly record struct Rule(string Tag, HealthStatus Trigger, DegradationLevel Outcome);

    public static readonly DegradationPolicy Canonical = new(
        Rules: [
            new Rule(HealthContributorRow.Remote, HealthStatus.Unhealthy, DegradationLevel.ReducedRemote),
            new Rule(HealthContributorRow.Host, HealthStatus.Unhealthy, DegradationLevel.LocalOnly),
            new Rule(HealthContributorRow.Store, HealthStatus.Unhealthy, DegradationLevel.ReadOnly),
            new Rule(HealthContributorRow.Pressure, HealthStatus.Degraded, DegradationLevel.ReducedRemote),
            new Rule(HealthContributorRow.Pressure, HealthStatus.Unhealthy, DegradationLevel.Suspended),
        ],
        ConsecutiveHealthy: 3,
        MinimumDwell: Duration.FromSeconds(60),
        PublishDelay: Duration.FromSeconds(5),
        PublishPeriod: Duration.FromSeconds(30));

    public DegradationState Derive(DegradationState state, HealthSnapshot snapshot) =>
        (Candidate: Candidate(snapshot), Rank: state.Derived.Rank) switch {
            var fold when fold.Candidate.Rank > fold.Rank =>
                new DegradationState(fold.Candidate, state.Forced, state.Cascade, Streak: 0, Since: Optional(snapshot.At)),
            var fold when fold.Candidate.Rank == fold.Rank => state with { Streak = 0 },
            var fold when state.Streak + 1 >= ConsecutiveHealthy
                && state.Since.Map(since => snapshot.At - since >= MinimumDwell).IfNone(true) =>
                new DegradationState(fold.Candidate, state.Forced, state.Cascade, Streak: 0, Since: Optional(snapshot.At)),
            _ => state with { Streak = state.Streak + 1 },
        };

    private DegradationLevel Candidate(HealthSnapshot snapshot) =>
        Rules.Fold(DegradationLevel.Full, (worst, rule) =>
            rule.Outcome.Rank > worst.Rank
                && snapshot.Entries.Exists(entry => entry.Status == rule.Trigger && entry.Tags.Contains(rule.Tag))
                ? rule.Outcome
                : worst);
}

// The coherent pressure cell: one atom carries the snapshot that produced the level and the
// derived state together, so a governor's load-shed/bulkhead-resize read is race-free — it can
// never see a fresh level against a stale snapshot. Force/Cascade swap only the State slot.
public readonly record struct DegradationReading(HealthSnapshot Snapshot, DegradationState State) {
    public static DegradationReading Boot(Instant at, CorrelationId correlation) =>
        new(new HealthSnapshot(HealthStatus.Healthy, at, correlation, []), DegradationState.Boot);
    public DegradationLevel Level => State.Level;
}

public sealed class DegradationCell(DegradationPolicy policy, IClock clock, CorrelationId correlation) : IHealthCheckPublisher {
    private readonly Atom<DegradationReading> cell = Atom(DegradationReading.Boot(clock.GetCurrentInstant(), correlation));
    public DegradationReading Read() => cell.Value;
    public DegradationState State => cell.Value.State;
    public DegradationLevel Level => cell.Value.Level;

    public DegradationState Force(Option<DegradationLevel> forced) =>
        cell.Swap(reading => reading with { State = reading.State with { Forced = forced } }).State;

    public DegradationState Cascade(Option<DegradationLevel> parent) =>
        cell.Swap(reading => reading with { State = reading.State with { Cascade = parent } }).State;

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken) =>
        Task.FromResult(cell.Swap(reading =>
            report.Snapshot(clock.GetCurrentInstant(), correlation) is var snapshot
                ? new DegradationReading(snapshot, policy.Derive(reading.State, snapshot))
                : reading).State);
}
```

## [04]-[WIRE_HEALTH]

- Owner: `WireHealthRow` binds one wire service name to one tag predicate; `WireHealth` attaches the filtered evaluation.
- Entry: `Evaluate(HealthCheckService service, CancellationToken token)` runs the tag-filtered registry sweep behind one row.
- Auto: app roots register the `grpc.health.v1` service behind the app-root pin and feed it the row predicate; healthy and degraded project to the serving wire state, unhealthy to not-serving — degraded keeps serving because the level, not the wire, carries usable failure.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, LanguageExt.Core
- Growth: one wire row per served service name, zero new surface.
- Boundary: set-degradation is the service-modality inbound route — the verb admits its level key through `DegradationLevel.TryGet`, mapping an unknown key to `None` so `Force` re-derives rather than forcing a phantom level, and lands on `Force`; one override rail serves operator config, wire verbs, and release.

```csharp signature
public sealed record WireHealthRow(string Service, Option<string> Tag) {
    public static readonly WireHealthRow Overall = new(string.Empty, None);
    public bool Admits(IEnumerable<string> tags) => Tag.Map(tag => tags.Contains(tag)).IfNone(true);
}

public static class WireHealth {
    extension(WireHealthRow row) {
        public Task<HealthReport> Evaluate(HealthCheckService service, CancellationToken token) =>
            service.CheckHealthAsync(registration => row.Admits(registration.Tags), token);
    }
}
```

## [05]-[ALERT_ENGINE]

- Owner: `AlertSeverity` `[SmartEnum<int>]` the rank-ordered severity ladder; `AlertCondition` `[Union]` the declarative condition family (threshold, anomaly, forecast-band); `AlertRule` the versioned rule record carrying hysteresis and debounce; `AlertState` the per-rule firing-state cell; `AlertEngine` the static evaluate-and-escalate surface over the continuous health snapshot stream.
- Cases: 4 severity rows — info(0), warning(1), error(2), critical(3); `AlertCondition` = Threshold | AnomalyBand | ForecastBand — Threshold fires on a value crossing a bound, AnomalyBand on a value outside a rolling mean ± k·sigma band, ForecastBand on a value outside a linear-trend forecast envelope.
- Entry: `Observe(AlertEngine.Runtime runtime, AlertRule rule, AlertState state, HealthSnapshot snapshot, Instant at)` is the health-stream entry — it resolves `rule.Metric` to a continuous value off the snapshot through `AlertEngine.Project` and delegates to the value-fold `Evaluate(AlertEngine.Runtime runtime, AlertRule rule, AlertState state, double value, Instant at)`; both return `(AlertState State, Option<AlertReceipt> Fired)` — the fold runs one sample through the rule's condition with hysteresis and debounce, returning the next firing state and an alert receipt only on a confirmed transition; `Backtest(AlertEngine.Runtime runtime, AlertRule rule, Seq<(Instant At, double Value)> history)` returns `Seq<AlertReceipt>` — replays the same value-fold over historical samples so a new rule's firing behavior is proven against past data before it goes live. One fold, two front doors: the live health stream resolves its value through `Project`, the back-test feeds the raw historical value.
- Auto: the threshold condition fires only after the value holds past the rule's debounce window so a momentary spike does not fire, and recovers only after the value clears the hysteresis band so a value oscillating at the bound does not flap; the anomaly band tracks a rolling mean and standard deviation over the rule's window so a value outside k sigma fires without a hand-set threshold; the forecast band fits a linear trend over the window and fires on a value outside the prediction envelope so a slow drift toward a limit fires before the limit is crossed; a firing alert escalates its severity if it stays fired past the escalation dwell so a persistent warning becomes an error becomes critical, and a recovered alert resets the escalation; the rule version stamps every receipt so a rule edit is auditable and a back-test pins the rule version it ran against.
- Receipt: `AlertReceipt` — rule id, rule version, severity, condition kind, the firing value, the transition (fired/recovered/escalated), `Instant`, correlation id; fanned through `ReceiptSinkPort.Send` and routed to the delivery fan-out for multi-channel notification.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one severity is one `AlertSeverity` row; one condition shape is one `AlertCondition` case breaking every evaluate arm; a rule edit is one new `AlertRule` version, never a mutated rule; zero new surface.
- Boundary: the alert engine is the only declarative-alerting owner — an ad hoc threshold check, a per-metric alarm, and a parallel alert store are the deleted forms; the engine evaluates over the continuous `HealthSnapshot` stream the health fold already produces so alerting reads the existing health signal, never a second metric source — `AlertEngine.Project(AlertRule, HealthSnapshot)` is the named-metric resolution that turns the rule's `Metric` string into a continuous double off the snapshot the fold consumes: the reserved metric keys `status` (the overall snapshot status rank), `pressure` (the `Pressure`-tagged entry status rank, the same signal `UtilizationCell` feeds), and a per-entry `{name}.status` route project the matching `HealthSnapshot.Entry` status onto a 0..3 rank, and an unmatched metric yields `Option<double>.None` so `Observe` holds the prior state and fires no spurious alert rather than fabricating a value — so the engine rides the existing health stream end to end with no hand-fed double and no second metric pipeline; the alert engine and the degradation rail are distinct concerns — degradation is the host's own capability state derived from health, while alerting is the user-facing notification product over arbitrary continuous queries, so a degradation transition is one alert input but an alert never forces a degradation level; the hysteresis and debounce reuse the same consecutive-confirm and minimum-dwell semantics the degradation derivation uses so the host-health alerting shape and the dashboard alerting share one anti-flap law; a fired alert routes to the delivery fan-out so multi-channel alert delivery is the existing outbound delivery, never a second sender; rule versioning makes a rule edit a new immutable version so an alert receipt traces to the exact rule that fired it, and a back-test proves a rule against history before it fires on live data.

```csharp signature
[SmartEnum<int>]
public sealed partial class AlertSeverity {
    public static readonly AlertSeverity Info = new(0);
    public static readonly AlertSeverity Warning = new(1);
    public static readonly AlertSeverity Error = new(2);
    public static readonly AlertSeverity Critical = new(3);

    public AlertSeverity Escalated => Value >= Critical.Value ? Critical : FromValue(Value + 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AlertCondition {
    private AlertCondition() { }
    public sealed record Threshold(double Bound, bool Above, double Hysteresis) : AlertCondition;
    public sealed record AnomalyBand(double Sigma, int Window) : AlertCondition;
    public sealed record ForecastBand(double EnvelopeWidth, int Window) : AlertCondition;
}

public sealed record AlertRule(
    string RuleId,
    int Version,
    string Metric,
    AlertCondition Condition,
    AlertSeverity Severity,
    Duration Debounce,
    Duration EscalationDwell);

public readonly record struct AlertState(
    bool Firing,
    AlertSeverity Current,
    int ConfirmStreak,
    Option<Instant> FiredSince,
    Seq<double> Window) {
    public static readonly AlertState Clear = new(false, AlertSeverity.Info, 0, None, []);
}

public readonly record struct AlertReceipt(
    string RuleId,
    int Version,
    AlertSeverity Severity,
    string Condition,
    double Value,
    string Transition,
    Instant At,
    CorrelationId Correlation) {
    public const string Fired = "fired";
    public const string Recovered = "recovered";
    public const string Escalated = "escalated";
}

public static class AlertEngine {
    public const string StatusMetric = "status";
    public const string PressureMetric = "pressure";
    public const string EntryStatusSuffix = ".status";

    public sealed record Runtime(int ConfirmCount, Func<AlertReceipt, IO<Unit>> Deliver, ClockPolicy Clocks);

    static double Rank(HealthStatus status) => status switch {
        HealthStatus.Unhealthy => 3d,
        HealthStatus.Degraded => 2d,
        HealthStatus.Healthy => 1d,
        _ => 0d,
    };

    public static Option<double> Project(AlertRule rule, HealthSnapshot snapshot) =>
        rule.Metric switch {
            StatusMetric => Some(Rank(snapshot.Status)),
            PressureMetric => snapshot.Entries
                .Find(entry => entry.Tags.Contains(HealthContributorRow.Pressure))
                .Map(entry => Rank(entry.Status)),
            var key when key.EndsWith(EntryStatusSuffix, StringComparison.Ordinal) =>
                snapshot.Entries
                    .Find(entry => entry.Name == key[..^EntryStatusSuffix.Length])
                    .Map(entry => Rank(entry.Status)),
            _ => None,
        };

    public static (AlertState State, Option<AlertReceipt> Fired) Observe(Runtime runtime, AlertRule rule, AlertState state, HealthSnapshot snapshot, Instant at) =>
        Project(rule, snapshot).Match(
            Some: value => Evaluate(runtime, rule, state, value, at),
            None: () => (state, Option<AlertReceipt>.None));

    public static (AlertState State, Option<AlertReceipt> Fired) Evaluate(Runtime runtime, AlertRule rule, AlertState state, double value, Instant at) {
        var window = (state.Window.Add(value) is var w && w.Count > Window(rule.Condition) ? w.Tail : w).Strict();
        var breached = Breached(rule.Condition, value, window, state.Firing);
        var streak = breached ? state.ConfirmStreak + 1 : 0;
        return (breached, state.Firing) switch {
            (true, false) when streak >= runtime.ConfirmCount =>
                (state with { Firing = true, Current = rule.Severity, ConfirmStreak = 0, FiredSince = Some(at), Window = window },
                 Some(new AlertReceipt(rule.RuleId, rule.Version, rule.Severity, Kind(rule.Condition), value, AlertReceipt.Fired, at, Correlation.Mint()))),
            (true, true) when state.FiredSince.Map(since => at - since >= rule.EscalationDwell).IfNone(false) && state.Current.Value < AlertSeverity.Critical.Value =>
                (state with { Current = state.Current.Escalated, FiredSince = Some(at), Window = window },
                 Some(new AlertReceipt(rule.RuleId, rule.Version, state.Current.Escalated, Kind(rule.Condition), value, AlertReceipt.Escalated, at, Correlation.Mint()))),
            (false, true) =>
                (AlertState.Clear with { Window = window },
                 Some(new AlertReceipt(rule.RuleId, rule.Version, rule.Severity, Kind(rule.Condition), value, AlertReceipt.Recovered, at, Correlation.Mint()))),
            _ => (state with { ConfirmStreak = streak, Window = window }, None),
        };
    }

    public static Seq<AlertReceipt> Backtest(Runtime runtime, AlertRule rule, Seq<(Instant At, double Value)> history) =>
        history.Fold((State: AlertState.Clear, Fired: Seq<AlertReceipt>()), (acc, sample) =>
            Evaluate(runtime, rule, acc.State, sample.Value, sample.At) is var step
                ? (step.State, step.Fired.Match(Some: acc.Fired.Add, None: () => acc.Fired))
                : acc).Fired;

    static bool Breached(AlertCondition condition, double value, Seq<double> window, bool firing) => condition.Switch(
        threshold: t => firing
            ? (t.Above ? value >= t.Bound - t.Hysteresis : value <= t.Bound + t.Hysteresis)
            : (t.Above ? value > t.Bound : value < t.Bound),
        anomalyBand: a => window.Count >= a.Window && Math.Abs(value - Mean(window)) > a.Sigma * Sigma(window),
        forecastBand: f => window.Count >= f.Window && Math.Abs(value - Forecast(window)) > f.EnvelopeWidth);

    static int Window(AlertCondition condition) => condition.Switch(
        threshold: static _ => 1, anomalyBand: static a => a.Window, forecastBand: static f => f.Window);

    static string Kind(AlertCondition condition) => condition.Switch(
        threshold: static _ => "threshold", anomalyBand: static _ => "anomaly-band", forecastBand: static _ => "forecast-band");

    static double Mean(Seq<double> window) => window.Average();
    static double Sigma(Seq<double> window) => Math.Sqrt(window.Map(v => Math.Pow(v - Mean(window), 2)).Average());
    static double Forecast(Seq<double> window) => window.Last + (window.Last - window.Head) / Math.Max(window.Count - 1, 1);
}
```

## [06]-[TS_PROJECTION]

- Owner: `HealthSnapshotWire`, `DegradationWire`, `CommandAvailabilityWire`, and `AlertReceiptWire` transcribe the snapshot, level, command-availability, and alert records the dashboard ingests; `CommandAvailabilityWire` is the ONE frozen name for the health/availability wire — the `DegradationLevel` command-availability projection (the level plus the per-command verdict the `Agent/capability#DISCOVERY_FOLD` `Permitting` fold derives) the TS `state/evidence` `Availability` lattice decodes, its level roster mirroring the `DegradationLevel` rows one-to-one at the decode seam.
- Packages: BCL inbox
- Growth: one capability key row, one alert field, or one field on an owning wire record, zero new surface.
- Boundary: instants cross as extended-ISO text and elapsed spans as ISO-8601 duration text; level, cascade, capability, and severity keys are the smart-enum string and int keys, status crosses as the camel-case enum name, never ordinals; `cascade` is the parent-floored level a child reports, distinct from `forced` operator override; the alert condition crosses as a literal-discriminated union on the condition kind and the alert transition crosses as the fired/recovered/escalated literal.

```ts contract
type HealthStatusWire = "healthy" | "degraded" | "unhealthy";

type CapabilityKey =
  | "host-document" | "remote-compute" | "local-compute"
  | "store-write" | "store-read" | "telemetry-export";

type DegradationLevelKey =
  | "full" | "reduced-remote" | "local-only" | "read-only" | "suspended";

interface HealthEntryWire {
  readonly name: string;
  readonly status: HealthStatusWire;
  readonly elapsed: string;
  readonly tags: readonly string[];
  readonly detail: string | null;
}

interface HealthSnapshotWire {
  readonly status: HealthStatusWire;
  readonly at: string;
  readonly correlation: string;
  readonly entries: readonly HealthEntryWire[];
}

interface DegradationWire {
  readonly level: DegradationLevelKey;
  readonly rank: number;
  readonly retains: readonly CapabilityKey[];
  readonly forced: DegradationLevelKey | null;
  readonly cascade: DegradationLevelKey | null;
  readonly since: string | null;
}

interface CommandAvailabilityWire {
  readonly level: DegradationLevelKey;
  readonly commands: Readonly<Record<string, boolean>>;
  readonly since: string;
}

type AlertSeverityKey = "info" | "warning" | "error" | "critical";

interface AlertReceiptWire {
  readonly ruleId: string;
  readonly version: number;
  readonly severity: AlertSeverityKey;
  readonly condition: "threshold" | "anomaly-band" | "forecast-band";
  readonly value: number;
  readonly transition: "fired" | "recovered" | "escalated";
  readonly at: string;
  readonly correlation: string;
}
```

## [07]-[RESEARCH]

- [WIRE_REGISTRATION]: the `grpc.health.v1` wire-service registration surface behind the app-root pin, with the default status-to-serving projection; the tag-predicate evaluation rides the confirmed `HealthCheckService.CheckHealthAsync(Func<HealthCheckRegistration, bool>?, CancellationToken)` overload.
- [MEMORY_RATIO_SEMANTIC]: `dotnet.process.memory.virtual.ratio` is read as the memory-pressure ratio the `Gauge` grade compares against `MemoryDegraded`/`MemoryUnhealthy`; whether this instrument and `process.cpu.utilization` report over the cgroup `MaxMemoryInBytes`/`MaxCpuInCores` ceilings or over the host total under `UseLinuxCalculationV2`+`UseZeroToOneRangeForLinuxMetrics` settles whether a `Container`-row grade rides the meter as published or threads a `ResourceQuotaProvider.GetResourceQuota()`-sourced `ResourceQuota` into a quota-relative recompute and the provider-registration surface that supplies `PressurePolicy.Quota` to the runtime — resolved against the ResourceMonitoring metric exporter and `ResourceQuotaProvider` registration before the container-row grade finalizes.
