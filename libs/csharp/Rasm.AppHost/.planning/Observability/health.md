# [APPHOST_HEALTH_AND_DEGRADATION]

Capability health and the usable-failure degradation rail for every Rasm.AppHost process: a health-contributor row family folds package probes into one wire-neutral snapshot, a `DriverProbe`-keyed adapter binds every admitted backing-service health check (Postgres, Redis, Kafka, upstream HTTP, disk, allocation) onto its degradation rule through one shared driver instance, the five-level DegradationLevel vocabulary carries one retained-capability set per row, and a wire-health mapping projects the registry onto the standard wire health service. Microsoft.Extensions.Diagnostics.HealthChecks supplies probe mechanics, the AspNetCore.HealthChecks.NpgSql/Redis/Kafka/Uris/System family supplies the concrete backing-service probes, ResourceMonitoring publishes utilization ratios and container ceilings on the Windows and Linux hosts it mints a snapshot provider for while the BCL process counters carry every other host, Thinktecture owns the vocabularies, LanguageExt and NodaTime carry the fold rails and stamps; every consumer reads one level value.

Settled composition: `CorrelationId` arrives from `Rasm/Domain/telemetry#CAUSAL_FRAME`, and `AlertSeverity` with its rank, hold, tone, urgency, and escalation-walk columns from `Rasm/Domain/telemetry#SLO_ALGEBRA` — the routing vocabulary the deploy plane's contact rows key on, so no severity ladder mints here. `ThermalPressure` and the live `PowerCell` reading arrive from `Runtime/profiles#POWER_AND_FIDELITY` as the third pressure input beside CPU and memory, and `Slo.Specs` compiles error-budget burn onto the deploy plane from the `HostInstruments` board pack at Observability/instruments#INSTRUMENT_CATALOG, so this engine alerts on health evidence alone.

## [01]-[INDEX]

- [02]-[HEALTH_FOLD]: Contributor rows, resource pressure, peer reads, and one snapshot fold.
- [03]-[DEGRADATION_RAIL]: Level vocabulary, retained capabilities, derivation fold, and hysteresis.
- [04]-[WIRE_HEALTH]: Tag-predicate wire mapping, the out-of-band filtered evaluation, and the inbound set-degradation route.
- [05]-[ALERT_ENGINE]: Declarative alert rules over the degradation reading with hysteresis, escalation, and versioning.
- [06]-[TS_PROJECTION]: Health snapshot, degradation level, and alert wire shapes.

## [02]-[HEALTH_FOLD]

- Owner: `HealthContributorRow` is the probe row and the `IHealthCheck`; `DriverProbe` `[SmartEnum<string>]` is the backing-service probe axis carrying each dependency kind's contributor tag and failure status; `PressureSource` `[Union]` names where a utilization ratio comes from and carries the instrument pair its arm reads; `PressurePolicy` grades utilization and thermal pressure with a `ResourceQuota` container-limit column; `UtilizationCell` is the boundary capsule holding the last reading, the listener seat, and the counter mark; `HealthSnapshot` with nested `Entry` is the only health shape interiors read.
- Cases: tag consts `Host`, `Remote`, `Store`, `Pressure` key the derivation rules and the wire predicates; three `PressureSource` rows — `Container` and `Host` over the ResourceMonitoring meter's limit-relative and process-relative gauge pairs, `Process` over the BCL counters; eight `DriverProbe` rows tracking the LANDED Persistence sink roster — `Postgres`/`Cache` (Store), `Nats`/`Upstream` (Remote), `Disk`/`Allocations` (Pressure) as DEFAULT probes, `Kafka`/`Redis` as deploy-gated sink-tracking rows registering only when their sink is bound — bind each admitted health-check package to its degradation rule; `Gauge`, `Peer`, and `Driver` are the canonical row factories and `Monitor` is the resource-monitoring registration fold.
- Entry: `Register(params ReadOnlySpan<HealthContributorRow> rows)` composes registrations; `UtilizationCell.Of(PressureSource, IClock)` is the one construction, mounting the meter arm's listener as it builds; `UtilizationCell.Read()` is the one utilization pull returning the typed rail; `Snapshot(Instant at, CorrelationId correlation)` is the pure report fold.
- Auto: rows project into `HealthCheckRegistration` — `FailureStatus`, `Tags`, `Timeout`, `Delay`, `Period` are registration policy, never probe-local exception handling; `Driver(DriverProbe, cadence, IHealthCheck)` adapts ANY admitted package check — `NpgSqlHealthCheck` over the pooled `NpgsqlDataSource`, the `Cache` row's L2-transit probe over the ONE `IDistributedCache` the `Runtime/resources#CACHE_PORT` rides (through `Microsoft.Extensions.Caching.StackExchangeRedis` when the deploy binds redis — the raw `IConnectionMultiplexer` driver is PRUNED Persistence-side, so a direct-multiplexer probe is the deleted form), `NatsHealthCheck` over the pooled `INatsConnection` (the spine's landed NATS egress sink, exactly as the npgsql row binds its data source), the deploy-gated `KafkaHealthCheck` over its sink `ProducerConfig` and `RedisHealthCheck` over its sink connection, `UriHealthCheck` over a service-discovery `AddUrlGroup`, and `DiskStorageHealthCheck`/`ProcessAllocatedMemoryHealthCheck` over the BCL counters — into one contributor row whose synthetic `HealthCheckContext` seats the `DriverProbe.FailureStatus` the package check stamps, so the packages enter as rows through one adapter rather than parallel `Add*` registration faces; `Monitor` registers the meter for a `Metered` source alone and shapes it through the options rail — `UseZeroToOneRangeForMetrics` and `UseZeroToOneRangeForLinuxMetrics` together pin every platform's series to the zero-to-one axis the ceilings compare against, `UseLinuxCalculationV2` rides `CgroupV2`, `EnableSystemDiskIoMetrics` rides `DiskIoMetrics`; the probe's own cadence is the pull cadence, so `Delay` seats one sampling interval and `Period` the collection window and no publisher window exists to alias against them.
- Receipt: `HealthSnapshot` stamped with `Instant` and `CorrelationId`; `HealthReport` never crosses the fold.
- Packages: Rasm, Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.ResourceMonitoring, Microsoft.Extensions.Options, AspNetCore.HealthChecks.NpgSql, AspNetCore.HealthChecks.Nats, AspNetCore.HealthChecks.Redis, AspNetCore.HealthChecks.Kafka, AspNetCore.HealthChecks.Uris, AspNetCore.HealthChecks.System, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one contributor row per new capability probe — sibling packages extend the same `Register` span through the health port registration set, zero new surface; a new backing-service dependency is one `DriverProbe` row binding its tag and failure status, never a new factory; a new utilization authority is one `PressureSource` case breaking every read arm; container-limit grading is one `ResourceQuota` value flip on `PressurePolicy.Quota`, a sampling retune is one `Sampling` value, and a thermal retune is one `ThermalPressure` row on the `ThermalDegraded`/`ThermalUnhealthy` columns, never a parallel policy.
- Boundary: package health types stop at this seam — interiors read `HealthSnapshot` and one level value; a `Driver` row binds the SAME pooled `NpgsqlDataSource`, the one L2 `IDistributedCache` transit, and the pooled `INatsConnection` the production path owns, so a probe shares connection pressure with live traffic and never opens a second out-of-pool connection or invents a parallel connection vocabulary — the roster is seed DATA tracking the landed Persistence egress sink roster (NATS the default spine anchor; kafka/redis deploy-gated sink rows, never default probes), so the probe axis never drifts beside the roster it probes, and its tag routes a faulted dependency onto an EXISTING degradation rule (`Store` -> `ReadOnly`, `Remote` -> `ReducedRemote`, `Pressure` -> `Degraded`) with zero added `Rule`; the `Disk`/`Allocations` probes are the discrete hard-ceiling complement to the continuous `UtilizationCell` gauge, not a second utilization source — they grade an absolute breach the windowed ratio does not express, both projecting into the one `Pressure`-tagged contributor set; `Peer` rows read a peer process over its wire health service, so cross-process health is a read, never shared state; `Gauge` folds the `Runtime/profiles#POWER_AND_FIDELITY` thermal rank as a third input beside CPU and memory so a thermally-throttled host escalates on the SAME `Pressure`-tagged row and the degradation rules gain nothing — a second thermal probe row, a thermal-only degradation level, and a power alarm beside the rail are the three deleted forms; an unreadable source grades `Degraded` with its captured cause as the entry detail, because a void probe's one evidence surface is the result it returns and a zero reading grades healthy. `PressureSource` seats the three arms this branch serves — the `Process` row over BCL counters, `Host` and `Container` over the meter where the package registers a snapshot source; `Container` reads `container.cpu.limit.utilization`/`container.memory.limit.utilization` because `UseLinuxCalculationV2` publishes no `process.cpu.utilization` at all and the non-V2 process gauge scales against the cpu REQUEST, so a container arm reading the process pair grades the wrong ceiling or nothing; the two range flips are one invariant across two platform halves — the Linux flip defaults on and the cross-platform flip defaults OFF, so a Windows host left unflipped emits `[0, 100]` against ratio ceilings and suspends itself at one percent load; `ResourceQuota` carries the `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings that `ResourceQuotaProvider.GetResourceQuota()` supplies to `PressurePolicy.Quota` as the ceiling evidence the grade detail stamps, never a re-derived second ratio, and that provider registers on every Linux host but only inside a Windows job object; the meter arm's observable instruments deliver nothing until asked, so `Read` drives the listener's own observation and a listener merely started is the silently-dead form the pull deletes; listener mount rides `Of` because a separable attach step left an unlistened meter cell constructible, and both the unlistened cell and the listened-but-unpublished one answered a zero ratio, which grades a saturated host healthy — a metered `Read` now refuses until BOTH named instruments have published, so the package's own no-snapshot-source-outside-Windows-and-Linux return surfaces as a `Degraded` entry carrying its cause.

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

    // Utilization and thermal pressure are one grade, not two probes: a cgroup-throttled host and a
    // thermally-throttled host both escalate the ONE Pressure-tagged row the degradation rules already key on.
    // Probe cadence IS sampling cadence — Delay seats one interval so the first read differences a real span.
    public static HealthContributorRow Gauge(UtilizationCell cell, PowerCell power, PressurePolicy policy) => new(
        Name: nameof(Gauge),
        Probe: _ => ValueTask.FromResult(cell.Read().Match(
            Succ: usage => policy.Grade(usage, power.Refresh().Thermal),
            Fail: fault => HealthCheckResult.Degraded(fault.Message))),
        FailureStatus: HealthStatus.Degraded,
        Tags: TagSet(Pressure),
        Timeout: DeadlineClass.HealthProbe,
        Delay: policy.Sampling,
        Period: policy.Window);

    // Builder overload and ConfigureMonitor rows drive the retired push model, so shape rides the
    // standard options rail; a Process source needs no meter at all, which is why the arm registers nothing.
    public static IServiceCollection Monitor(IServiceCollection services, PressurePolicy policy) =>
        policy.Source.Switch(
            state: (Services: services, Policy: policy),
            metered: static (bound, _) => bound.Services
                .AddResourceMonitoring()
                .Configure<ResourceMonitoringOptions>(options => {
                    options.UseZeroToOneRangeForMetrics = true;
                    options.UseZeroToOneRangeForLinuxMetrics = true;
                    options.UseLinuxCalculationV2 = bound.Policy.CgroupV2;
                    options.EnableSystemDiskIoMetrics = bound.Policy.DiskIoMetrics;
                }),
            runtime: static (bound, _) => bound.Services);

    public static HealthContributorRow Peer(string name, string tag, Duration cadence, Func<CancellationToken, ValueTask<HealthCheckResult>> probe) => new(
        Name: name,
        Probe: probe,
        FailureStatus: HealthStatus.Unhealthy,
        Tags: TagSet(Remote, tag),
        Timeout: DeadlineClass.HealthProbe,
        Delay: cadence,
        Period: cadence);

    // One driver-probe adapter serves every admitted Xabaril IHealthCheck (Npgsql/Redis/Kafka/Uris/System):
    // becomes one contributor row carrying the DriverProbe row's tag and failure status, never a parallel
    // AddNpgSql/AddRedis/AddKafka/AddUrlGroup registration scatter. A synthetic HealthCheckContext seats
    // whichever failure status the package check stamps, and every shared driver instance — pooled
    // NpgsqlDataSource, one L2 IDistributedCache transit, pooled INatsConnection — is the production path's.
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

// Backing-service probes take one axis: each row per dependency kind carries the contributor tag and failure
// status that route a faulted dependency onto its existing degradation rule — Postgres/Redis to Store
// (-> ReadOnly), Kafka/upstream HTTP to Remote (-> ReducedRemote), disk/allocation ceilings to Pressure
// (-> Degraded). Adding a backing-service kind is one row; the degradation rules are untouched.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DriverProbe {
    // This roster TRACKS the landed Persistence sink roster — a sink admitted Persistence-side lands
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

// Where a ratio comes from is the row, so the instrument pair travels with the arm that reads it. The meter
// arm carries two names because the roster is conditional: cgroup-v2 calculation publishes the container
// pair and no process gauge, while the process pair scales against the cpu request rather than the limit.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressureSource {
    private PressureSource() { }

    // Key names the CPU series each arm actually grades, so a health entry's detail states which authority
    // produced the ratio an operator is reading. Host is process-relative on Windows alone — the Linux
    // provider feeds the process memory gauge from the container memory-limit ratio, one series under two names.
    public abstract string Key { get; }

    public sealed record Metered(string Cpu, string Memory) : PressureSource { public override string Key => Cpu; }
    public sealed record Runtime : PressureSource { public override string Key => "dotnet.process.cpu"; }

    public static readonly PressureSource Container =
        new Metered("container.cpu.limit.utilization", "container.memory.limit.utilization");
    public static readonly PressureSource Host =
        new Metered("process.cpu.utilization", "dotnet.process.memory.virtual.utilization");
    public static readonly PressureSource Process = new Runtime();
}

public sealed record PressurePolicy(
    PressureSource Source,
    Duration Window,
    Duration Sampling,
    double CpuDegraded,
    double CpuUnhealthy,
    double MemoryDegraded,
    double MemoryUnhealthy,
    ThermalPressure ThermalDegraded,
    ThermalPressure ThermalUnhealthy,
    Option<ResourceQuota> Quota,
    bool CgroupV2,
    bool DiskIoMetrics) {
    // BCL counters take the canonical seat as the only arm every host serves; a
    // Windows or Linux root that wants the smoothed meter ratio flips Source with `with`.
    public static readonly PressurePolicy Canonical = new(
        Source: PressureSource.Process,
        Window: Duration.FromSeconds(5), Sampling: Duration.FromSeconds(1),
        CpuDegraded: 0.80d, CpuUnhealthy: 0.92d, MemoryDegraded: 0.85d, MemoryUnhealthy: 0.95d,
        ThermalDegraded: ThermalPressure.Serious, ThermalUnhealthy: ThermalPressure.Critical,
        Quota: None, CgroupV2: false, DiskIoMetrics: false);

    // Quota provider registers on every Linux host and only inside a Windows job object, so taking it as the
    // argument makes the container arm unconstructible exactly where its ceilings do not resolve.
    public static PressurePolicy Container(ResourceQuotaProvider quotas) =>
        Canonical with {
            Source = PressureSource.Container,
            Quota = Optional(quotas.GetResourceQuota()),
            CgroupV2 = true,
            DiskIoMetrics = true,
        };

    // Thermal rank IS the generated smart-enum key, so the joint tuple compares three scalars on one axis
    // each and no ceiling is restated per arm.
    public HealthCheckResult Grade(Utilization usage, ThermalPressure thermal) =>
        (Cpu: usage.CpuRatio, Memory: usage.MemoryRatio, Thermal: thermal.Key) switch {
            var load when load.Cpu >= CpuUnhealthy || load.Memory >= MemoryUnhealthy || load.Thermal >= ThermalUnhealthy.Key =>
                HealthCheckResult.Unhealthy(Detail(load)),
            var load when load.Cpu >= CpuDegraded || load.Memory >= MemoryDegraded || load.Thermal >= ThermalDegraded.Key =>
                HealthCheckResult.Degraded(Detail(load)),
            _ => HealthCheckResult.Healthy(),
        };

    string Detail((double Cpu, double Memory, int Thermal) load) =>
        $"{Source.Key} cpu {load.Cpu:P0} memory {load.Memory:P0} thermal {load.Thermal}{Ceilings}";

    // Quota is the DETAIL's evidence and never a second grade: the container arm's meter ratios already resolve
    // against the cgroup limit, so a ratio re-derived off these numbers would grade one saturation under two
    // authorities that disagree the moment either read lags. A host resolving no provider — darwin, or a Windows
    // process outside a job object — stamps the ratios bare, so an absent ceiling reads absent, never unbounded.
    string Ceilings => Quota.Match(
        Some: static quota => $" limit {quota.MaxCpuInCores:0.##}cpu {quota.MaxMemoryInBytes}By baseline {quota.BaselineCpuInCores:0.##}cpu {quota.BaselineMemoryInBytes}By",
        None: static () => string.Empty);
}

// Named boundary capsule for the statement carve-out: listener wiring and the observation pull carry
// language-owned statement forms, and every read leaves on the typed rail.
public sealed class UtilizationCell : IDisposable {
    public const string Meter = "Microsoft.Extensions.Diagnostics.ResourceMonitoring";
    private static readonly Error Unobserved =
        new Fault.InvalidValue(Label: nameof(UtilizationCell), Requirement: "a published utilization measurement");

    private readonly PressureSource source;
    private readonly IClock clock;
    private readonly MeterListener listener = new();
    private readonly Atom<(Option<double> Cpu, Option<double> Memory)> observed = Atom((Option<double>.None, Option<double>.None));
    private readonly Atom<(Utilization Usage, Instant At, Duration Cpu)> counted;

    private UtilizationCell(PressureSource source, IClock clock) {
        this.source = source;
        this.clock = clock;
        counted = Atom((new Utilization(0d, 0d), clock.GetCurrentInstant(), Duration.FromTimeSpan(Environment.CpuUsage.TotalTime)));
    }

    // Listener wiring rides construction, so a Metered cell cannot exist unlistened; a separable attach step
    // leaves an unattached cell constructible, and its zero ratio grades a saturated host healthy.
    public static UtilizationCell Of(PressureSource source, IClock clock) =>
        source.Switch(
            state: new UtilizationCell(source, clock),
            metered: static (held, row) => held.Listening(row),
            runtime: static (held, _) => held);

    // Start only publishes instruments; an observable delivers nothing until the listener observes it, and
    // that call aggregates every throwing callback, so the pull is a captured boundary crossing.
    public Fin<Utilization> Read() => source.Switch(
        state: this,
        metered: static (held, _) => Try.lift(held.Pulled).Run().Bind(static usage => usage.ToFin(Unobserved)),
        runtime: static (held, _) => Try.lift(held.Counted).Run());

    public void Dispose() => listener.Dispose();

    // BOTH axes must publish before a ratio pair exists: the resource-monitoring package registers no
    // snapshot source outside Windows and Linux, so an arm that never observes refuses with its cause
    // rather than reading zero-and-healthy on a saturated host.
    Option<Utilization> Pulled() {
        listener.RecordObservableInstruments();
        (Option<double> Cpu, Option<double> Memory) held = observed.Value;
        return held.Cpu.Bind(cpu => held.Memory.Map(memory => new Utilization(cpu, memory)));
    }

    // Clock and counter read OUTSIDE the swap so a CAS retry re-folds identical inputs; reading either
    // inside would stamp a different span per attempt and the committed ratio would not be the sampled one.
    Utilization Counted() =>
        Sampled(clock.GetCurrentInstant(), Duration.FromTimeSpan(Environment.CpuUsage.TotalTime));

    // Empty spans report the prior ratio and an unreported memory ceiling reports zero share.
    Utilization Sampled(Instant now, Duration cpu) =>
        counted.Swap(prior => Differenced(prior, now, cpu)).Usage;

    static (Utilization Usage, Instant At, Duration Cpu) Differenced(
        (Utilization Usage, Instant At, Duration Cpu) prior, Instant now, Duration cpu) =>
        ((now - prior.At).TotalSeconds is var span && span > 0d
            ? new Utilization(
                double.Min(1d, (cpu - prior.Cpu).TotalSeconds / (span * Environment.ProcessorCount)),
                GC.GetGCMemoryInfo().TotalAvailableMemoryBytes is var ceiling && ceiling > 0L
                    ? double.Min(1d, Environment.WorkingSet / (double)ceiling)
                    : 0d)
            : prior.Usage,
         now, cpu);

    UtilizationCell Listening(PressureSource.Metered row) {
        listener.InstrumentPublished = (instrument, active) => {
            if (instrument.Meter.Name == Meter && (instrument.Name == row.Cpu || instrument.Name == row.Memory))
                active.EnableMeasurementEvents(instrument);
        };
        // Publication filter admits the two named instruments alone, so the callback's two arms are total.
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
            ignore(observed.Swap(current => instrument.Name == row.Cpu
                ? (Some(measurement), current.Memory)
                : (current.Cpu, Some(measurement)))));
        listener.Start();
        return this;
    }
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

- Owner: `Capability` and `DegradationLevel` vocabularies under the shipped `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `DegradationPolicy` with nested `Rule` rows is the derivation table; `DegradationState` is the fold receipt; `DegradationReading` is the coherent `(snapshot, state)` pair; `DegradationCell` is the boundary capsule owning the one atom cell, the publisher seam, and the hook rail every committed reading fans through.
- Cases: `Full(0)`, `ReducedRemote(1)`, `LocalOnly(2)`, `ReadOnly(3)`, `Suspended(4)` in severity order; six `Capability` keys form the retained sets.
- Entry: `Derive(DegradationState state, HealthSnapshot snapshot)` folds rules with escalation-immediate, recovery-hysteresis semantics; `Force(Option<DegradationLevel> forced)` is the single override entrypoint; `Cascade(Option<DegradationLevel> parent)` admits a parent-forced level as a derivation floor; `Read()` returns the one `DegradationReading` carrying the snapshot that produced the level and the derived `DegradationState` in one coherent value.
- Auto: `DegradationCell` registers as the `IHealthCheckPublisher` and owns one `Atom<DegradationReading>` — `PublishAsync` snapshots the `HealthReport` and folds `Derive` in the SAME swap, so the published snapshot and the level it produced are one atomic transition and a reader can never observe a fresh level against a stale snapshot or the reverse; `HealthCheckPublisherOptions` binds `Delay` and `Period` from `DegradationPolicy.Canonical` and `Timeout` from `DeadlineClass.HealthProbe`; `OperatorOverride` projects onto `Force` at the composition root — forced beats derived, release re-derives; `Force` and `Cascade` swap the `State` slot of the reading while preserving the last snapshot so the override is coherent with the evidence it overrides; every committed reading — derived, forced, cascaded alike — fans the `Observability/hooks#HOOK_RAIL` `Degradation` replay row through the rail's own `Degraded` member, so the held window carries the trajectory an attaching panel reads rather than whichever single arm remembered to publish.
- Receipt: `DegradationReading` carries the latest `HealthSnapshot` and the `DegradationState` (derived level, forced input, cascade floor, recovery streak, dwell anchor); a `Level` change rides the lifecycle transition receipt as the degraded trigger.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `Rule` row or one `Capability` case absorbs a new degradation driver; a new level is one `DegradationLevel` item; a new pressure-read consumer reads the one `DegradationReading` value, never a second cell — zero new surface.
- Boundary: degradation is process-local, peer-health-informed, and parent-cascade-floored — a peer level never propagates as this process's level, but a parent process's forced level enters `Derive` as a floor through `Cascade`, never as shared state; the snapshot and the derived level are one `DegradationReading` atom so the `Runtime/laneguard#LANE_GUARD` governor reads a coherent `(snapshot, level)` pressure value for its adaptive-concurrency and load-shed decisions — the prior two-surface read (a `HealthSnapshot.Snapshot` independent of a `DegradationState.Derive`) is the collapsed form, and a governor reading a stale snapshot against a fresh level is the race the single atom forecloses; `LocalOnly` is the host-absent fold: `Capability.HostDocument` gates off and document sources yield absence; the container-limit pressure signal enters the rank algebra as data, not a new rule — a `PressurePolicy.Container` row grades against `ResourceQuota` so the `Pressure`-tagged `Gauge` row escalates on the cgroup limit, and the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules carry that limit-relative status into `Derive` with the same retained-set hysteresis, so a container-throttled process degrades and recovers on its own limit with zero added `Rule` row. Cross-process cascade splits at a seam the snapshot fold preserves: the READ — this process's own `DegradationReading` — stays the owner here; the WRITE — a parent fanning its level to a child over the control hop — lands at `Wire/companion#DEGRADATION_CASCADE`, which calls `Cascade` with the observed parent level mutating only the `State` slot; release passes `None` and the cell re-derives off its own snapshots, the cascade floor never escalating below local pressure, so folding the snapshot into the cell preserves the cascade-floor and hysteresis fields and never merges the parent-cascade owner.

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

// One coherent pressure cell holds both halves: its atom carries the snapshot that produced the level and the
// derived state together, so a governor's load-shed/bulkhead-resize read is race-free — it can
// never see a fresh level against a stale snapshot. Force/Cascade swap only the State slot.
public readonly record struct DegradationReading(HealthSnapshot Snapshot, DegradationState State) {
    public static DegradationReading Boot(Instant at, CorrelationId correlation) =>
        new(new HealthSnapshot(HealthStatus.Healthy, at, correlation, []), DegradationState.Boot);
    public DegradationLevel Level => State.Level;
}

public sealed class DegradationCell(DegradationPolicy policy, IClock clock, CorrelationId correlation, HookRail rail) : IHealthCheckPublisher {
    private readonly Atom<DegradationReading> cell = Atom(DegradationReading.Boot(clock.GetCurrentInstant(), correlation));
    public DegradationReading Read() => cell.Value;
    public DegradationState State => cell.Value.State;
    public DegradationLevel Level => cell.Value.Level;

    public DegradationState Force(Option<DegradationLevel> forced) =>
        Fired(cell.Swap(reading => reading with { State = reading.State with { Forced = forced } })).State;

    public DegradationState Cascade(Option<DegradationLevel> parent) =>
        Fired(cell.Swap(reading => reading with { State = reading.State with { Cascade = parent } })).State;

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken) =>
        Task.FromResult(Folded(report.Snapshot(clock.GetCurrentInstant(), correlation)));

    // Snapshot mints once outside the cell: a swap fold re-runs on every CAS retry, so a clock read
    // inside would stamp a later instant per attempt and the published evidence would not be the graded one.
    DegradationState Folded(HealthSnapshot snapshot) =>
        Fired(cell.Swap(reading => new DegradationReading(snapshot, policy.Derive(reading.State, snapshot)))).State;

    // Every COMMITTED reading fans the replay row off the swap's own return — a prior-value read beside the swap
    // races the operator path and misses exactly the override transitions a late panel attaches to reconstruct.
    DegradationReading Fired(DegradationReading reading) => (rail.Degraded(reading), reading).Item2;
}
```

## [04]-[WIRE_HEALTH]

- Owner: `WireHealthRow` binds one wire service name to one tag predicate; `WireHealth` attaches the filtered evaluation and the app-root wire registration.
- Entry: `Register(IServiceCollection services, params ReadOnlySpan<WireHealthRow> rows)` is the app-root pin folding every row onto the standard wire health service; `Evaluate(HealthCheckService, WireHealthRow, IClock, CorrelationId, CancellationToken)` is the out-of-band read of the SAME row for a probe that speaks no gRPC.
- Auto: `Register` composes `AddGrpcHealthChecks(Action<GrpcHealthChecksOptions>)` — each row lands as one `GrpcHealthChecksOptions.Services` mapping through `ServiceMappingCollection.Map(string name, Func<HealthCheckMapContext, bool> predicate)`, the predicate reading the row's tag against `HealthCheckMapContext.Tags` — and the endpoint serves through `MapGrpcHealthChecksService()` at the wire host, so the `grpc.health.v1` protocol answers per-service off the one registry; healthy and degraded project to the serving wire state, unhealthy to not-serving — degraded keeps serving because the level, not the wire, carries usable failure.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, Grpc.AspNetCore.HealthChecks, LanguageExt.Core
- Growth: one wire row per served service name — one `Map` mapping and zero new surface; the empty-name `Overall` row is the default service every client without a service name reads.
- Boundary: set-degradation is the service-modality inbound route — the verb admits its level key through `DegradationLevel.TryGet`, mapping an unknown key to `None` so `Force` re-derives rather than forcing a phantom level, and lands on `Force`; one override rail serves operator config, wire verbs, and release; `MapGrpcHealthChecksService` is the one wire-health endpoint — a hand-rolled `Grpc.Health.V1.Health.HealthBase` override beside it is the deleted form; `Evaluate` drives the registry live and returns a `HealthSnapshot`, so an HTTP liveness route, an operator verb, and a CLI check read the row set rather than the `DegradationCell`'s published reading — the cell stays the CADENCED truth every interior consumes and `Evaluate` is the on-demand probe path, so a caller wanting the settled level reads `DegradationCell.Read()` and never re-evaluates to derive one.

```csharp signature
public sealed record WireHealthRow(string Service, Option<string> Tag) {
    public static readonly WireHealthRow Overall = new(string.Empty, None);
    public bool Admits(IEnumerable<string> tags) => Tag.Map(tag => tags.Contains(tag)).IfNone(true);
}

public static class WireHealth {
    // App-root pin: every row folds onto GrpcHealthChecksOptions.Services as one Map mapping, and
    // MapGrpcHealthChecksService() serves grpc.health.v1 at the wire host, so the health fold stays
    // sole truth and the wire reads it filtered, never a second health surface.
    public static IServiceCollection Register(IServiceCollection services, params ReadOnlySpan<WireHealthRow> rows) {
        Seq<WireHealthRow> set = rows.ToArray().ToSeq();
        return services.AddGrpcHealthChecks(options =>
            ignore(set.Iter(row => options.Services.Map(row.Service, context => row.Admits(context.Tags)))));
    }

    // One row drives BOTH filtered reads: the gRPC mapping above and this out-of-band evaluation, so a
    // liveness or readiness probe outside grpc.health.v1 admits exactly what its wire service admits.
    // CheckHealthAsync takes a nullable predicate, so Overall passes None and evaluates the whole
    // registry rather than allocating an all-admitting closure per probe.
    public static IO<HealthSnapshot> Evaluate(
        HealthCheckService service, WireHealthRow row, IClock clock, CorrelationId correlation, CancellationToken token) =>
        IO.liftAsync(async () =>
            (await service.CheckHealthAsync(
                row.Tag.Match(
                    Some: _ => (Func<HealthCheckRegistration, bool>?)(registration => row.Admits(registration.Tags)),
                    None: static () => null),
                token).ConfigureAwait(false))
            .Snapshot(clock.GetCurrentInstant(), correlation));
}
```

## [05]-[ALERT_ENGINE]

- Owner: `HealthSignal` `[Union]` the closed selector naming which reading a rule watches; `AlertCondition` `[Union]` the declarative condition family (threshold, anomaly, forecast-band) carrying its wire key and window depth as case columns; `AlertTransition` `[SmartEnum<string>]` the three transition keys; `AlertRule` the versioned rule record carrying hysteresis and debounce; `AlertState` the per-rule firing-state cell; `AlertEngine` the static evaluate-and-escalate surface over the continuous `DegradationReading` stream.
- Cases: `HealthSignal` = Overall | Tagged | Named | Level — the aggregate status rank, the worst rank among entries carrying one tag, one named contributor's rank, and the derived degradation rank; `AlertCondition` = Threshold | AnomalyBand | ForecastBand — Threshold fires on a value crossing a bound, AnomalyBand on a value outside a rolling mean ± k·sigma band, ForecastBand on a value outside a linear-trend forecast envelope; severity rows are the kernel `page`/`ticket` pair alone.
- Entry: `Observe(AlertEngine.Runtime runtime, AlertRule rule, AlertState state, DegradationReading reading, Instant at)` is the stream entry — it resolves `rule.Signal` through `AlertEngine.Project`, delegates to the pure value-fold `Evaluate`, and delivers each transition receipt on `IO`; `Evaluate(AlertRule rule, AlertState state, double value, Instant at, CorrelationId correlation)` returns `(AlertState State, Option<AlertReceipt> Fired)`; `Backtest(AlertRule rule, Seq<(Instant At, double Value)> history, CorrelationId correlation)` returns `Seq<AlertReceipt>` by replaying that pure fold without a delivery runtime. One fold, two front doors: the live stream resolves and delivers, while the back-test feeds historical values and only returns evidence.
- Auto: the threshold condition fires only after the value holds past the rule's dwell so a momentary spike does not fire, and recovers only after the value clears the hysteresis band so a value oscillating at the bound does not flap; the dwell is the rule's own debounce raised to its severity's `Hold` column, so a ticketing rule inherits the deploy plane's flap suppression without restating it; the anomaly band folds the held window's mean and deviation in one pass and tests the incoming value against them, so the sample never contributes to the baseline that judges it; the forecast band fits the window by ordinary least squares and evaluates one step past its end, so a slow drift toward a limit fires before the limit is crossed; a firing alert escalates through the severity rank walk if it stays fired past the escalation dwell and the top row escalates to itself, so the ladder grows by a severity row and never by an arm here; a recovered alert reports the severity it held and resets the escalation; the rule version stamps every receipt so a rule edit is auditable and a back-test pins the rule version it ran against.
- Receipt: `AlertReceipt` — rule id, rule version, severity, condition key, the firing value, the transition row, `Instant`, and the correlation the producing reading carried, so an alert joins the snapshot that fired it rather than rooting a fresh causal frame; `Observe` invokes the runtime delivery delegate exactly once for each transition, and the app root binds that delegate to `ReceiptSinkPort.Send` and the outbound fan.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one condition shape is one `AlertCondition` case breaking every evaluate arm; one watched reading is one `HealthSignal` case breaking `Project`; a rule edit is one new `AlertRule` version, never a mutated rule; a routing posture is a kernel `AlertSeverity` row, never a case here; zero new surface.
- Boundary: the alert engine is the only declarative-alerting owner — an ad hoc threshold check, a per-metric alarm, and a parallel alert store are the deleted forms; the engine evaluates over the continuous `DegradationReading` the `[03]` cell already publishes, so it reads the coherent snapshot-and-level pair the governor reads and can never grade a fresh level against a stale snapshot, and a `HealthSnapshot`-only entry is the stale-read shape that pair exists to foreclose; `AlertEngine.Project` resolves each `HealthSignal` case onto its OWN rank axis — the status cases onto the healthy-through-unhealthy ladder, `Level` onto the five-row degradation rank — so a bound is authored against the case the rule names and a bound ported across cases is the drift the two axes make visible, while an unmatched selector yields `Option<double>.None` so `Observe` holds the prior state and delivers nothing; the severity vocabulary is the kernel's two-row routing axis — this page carries no ladder, and the rank-ordered incident escalation rides the row's `Rank` and `Escalated` columns; error-budget burn is the SEPARATE concern the kernel `Slo.Specs` compiles onto the deploy plane from an `Objective`, so a burn arm here mints the second metric source this boundary forbids; the alert engine and degradation rail remain distinct — degradation is the host's capability state, while alerting is user-facing notification over continuous queries; hysteresis is the condition's recovery band and dwell is the rule's minimum breach hold, so sampling frequency cannot change a rule's firing threshold; only live `Observe` invokes delivery, so `Backtest` cannot notify operators while replaying history; rule versioning makes a rule edit a new immutable version so each receipt identifies the rule that fired it.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HealthSignal {
    private HealthSignal() { }
    public sealed record Overall : HealthSignal;
    public sealed record Tagged(string Tag) : HealthSignal;
    public sealed record Named(string Contributor) : HealthSignal;
    public sealed record Level : HealthSignal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AlertCondition {
    private AlertCondition() { }

    // Wire key and window depth are case columns, so the TS literal union, the ring size, and the band's own
    // sufficiency gate read one declaration rather than static switches restating the same three rows. A band
    // floors at two samples: one point carries no deviation and no slope, so a degenerate window stays inert
    // instead of firing on every value against a zero baseline.
    public abstract string Key { get; }
    public abstract int Depth { get; }

    public sealed record Threshold(double Bound, bool Above, double Hysteresis) : AlertCondition {
        public override string Key => "threshold";
        public override int Depth => 1;
    }
    public sealed record AnomalyBand(double Sigma, int Window) : AlertCondition {
        public override string Key => "anomaly-band";
        public override int Depth => int.Max(Window, 2);
    }
    public sealed record ForecastBand(double EnvelopeWidth, int Window) : AlertCondition {
        public override string Key => "forecast-band";
        public override int Depth => int.Max(Window, 2);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlertTransition {
    public static readonly AlertTransition Fired = new("fired");
    public static readonly AlertTransition Recovered = new("recovered");
    public static readonly AlertTransition Escalated = new("escalated");
}

public sealed record AlertRule(
    string RuleId,
    int Version,
    HealthSignal Signal,
    AlertCondition Condition,
    AlertSeverity Severity,
    Duration Debounce,
    Duration EscalationDwell) {
    // Severity rows fix the hold a rule may raise but never undercut, so a ticketing rule
    // inherits the deploy plane's flap suppression with no constant restated here.
    public Duration Dwell => Debounce > Severity.Hold ? Debounce : Severity.Hold;
}

public readonly record struct AlertState(
    bool Firing,
    AlertSeverity Current,
    Option<Instant> BreachedSince,
    Option<Instant> FiredSince,
    Seq<double> Window) {
    public static readonly AlertState Clear = new(false, AlertSeverity.Ticket, None, None, []);
}

public readonly record struct AlertReceipt(
    string RuleId,
    int Version,
    AlertSeverity Severity,
    string Condition,
    double Value,
    AlertTransition Transition,
    Instant At,
    CorrelationId Correlation);

public static class AlertEngine {
    public sealed record Runtime(Func<AlertReceipt, IO<Unit>> Deliver);

    // Enum values outside the roster reach this arm, so the floor takes the WORST rank: an unmapped status is
    // absent evidence, and ranking it healthier than healthy silences the rule that watches it.
    static double Rank(HealthStatus status) => status switch {
        HealthStatus.Healthy => 1d,
        HealthStatus.Degraded => 2d,
        _ => 3d,
    };

    public static Option<double> Project(AlertRule rule, DegradationReading reading) => rule.Signal.Switch(
        state: reading,
        overall: static (read, _) => Some(Rank(read.Snapshot.Status)),
        tagged: static (read, row) => read.Snapshot.Entries
            .Filter(entry => entry.Tags.Contains(row.Tag))
            .Fold(Option<double>.None, static (worst, entry) => Some(double.Max(worst.IfNone(0d), Rank(entry.Status)))),
        named: static (read, row) => read.Snapshot.Entries
            .Find(entry => entry.Name == row.Contributor)
            .Map(static entry => Rank(entry.Status)),
        level: static (read, _) => Some((double)read.Level.Rank));

    public static IO<(AlertState State, Option<AlertReceipt> Fired)> Observe(Runtime runtime, AlertRule rule, AlertState state, DegradationReading reading, Instant at) =>
        Project(rule, reading).Match(
            Some: value => Deliver(runtime, Evaluate(rule, state, value, at, reading.Snapshot.Correlation)),
            None: () => IO.pure((state, Option<AlertReceipt>.None)));

    public static (AlertState State, Option<AlertReceipt> Fired) Evaluate(AlertRule rule, AlertState state, double value, Instant at, CorrelationId correlation) {
        // Held window carries the BASELINE and the tested value stays out-of-sample: a point folded into its own
        // mean pulls the mean toward itself and inflates the deviation it is measured against.
        var window = (state.Window.Add(value) is var w && w.Count > rule.Condition.Depth ? w.Tail : w).Strict();
        var breached = Breached(rule.Condition, value, state.Window, state.Firing);
        Option<Instant> breachedSince = breached && !state.Firing
            ? state.BreachedSince.Match(Some: static since => Some(since), None: () => Some(at))
            : None;
        return (breached, state.Firing) switch {
            (true, false) when breachedSince.Map(since => at - since >= rule.Dwell).IfNone(false) =>
                (state with { Firing = true, Current = rule.Severity, BreachedSince = None, FiredSince = Some(at), Window = window },
                 Some(Mint(rule, rule.Severity, value, AlertTransition.Fired, at, correlation))),
            // Top-row escalation returns itself, so the rank walk is the whole guard and no ceiling literal exists.
            (true, true) when state.FiredSince.Map(since => at - since >= rule.EscalationDwell).IfNone(false) && state.Current.Rank < state.Current.Escalated.Rank =>
                (state with { Current = state.Current.Escalated, FiredSince = Some(at), Window = window },
                 Some(Mint(rule, state.Current.Escalated, value, AlertTransition.Escalated, at, correlation))),
            (false, true) =>
                (AlertState.Clear with { Window = window },
                 Some(Mint(rule, state.Current, value, AlertTransition.Recovered, at, correlation))),
            _ => (state with { BreachedSince = breachedSince, Window = window }, None),
        };
    }

    public static Seq<AlertReceipt> Backtest(AlertRule rule, Seq<(Instant At, double Value)> history, CorrelationId correlation) =>
        history.Fold((State: AlertState.Clear, Fired: Seq<AlertReceipt>()), (acc, sample) =>
            Threaded(acc, Evaluate(rule, acc.State, sample.Value, sample.At, correlation))).Fired;

    static (AlertState State, Seq<AlertReceipt> Fired) Threaded(
        (AlertState State, Seq<AlertReceipt> Fired) held, (AlertState State, Option<AlertReceipt> Fired) step) =>
        (step.State, step.Fired.Match(Some: held.Fired.Add, None: () => held.Fired));

    static AlertReceipt Mint(AlertRule rule, AlertSeverity severity, double value, AlertTransition transition, Instant at, CorrelationId correlation) =>
        new(rule.RuleId, rule.Version, severity, rule.Condition.Key, value, transition, at, correlation);

    static IO<(AlertState State, Option<AlertReceipt> Fired)> Deliver(
        Runtime runtime,
        (AlertState State, Option<AlertReceipt> Fired) step) =>
        step.Fired.Match(
            Some: receipt => IO.lift(() => runtime.Deliver(receipt))
                .Bind(static delivery => delivery)
                .Map(_ => step),
            None: () => IO.pure(step));

    static bool Breached(AlertCondition condition, double value, Seq<double> baseline, bool firing) => condition.Switch(
        state: (Value: value, Baseline: baseline, Firing: firing),
        threshold: static (read, t) => read.Firing
            ? (t.Above ? read.Value >= t.Bound - t.Hysteresis : read.Value <= t.Bound + t.Hysteresis)
            : (t.Above ? read.Value > t.Bound : read.Value < t.Bound),
        anomalyBand: static (read, a) => read.Baseline.Count >= a.Depth
            && Moments(read.Baseline) is var m && double.Abs(read.Value - m.Mean) > a.Sigma * m.Sigma,
        forecastBand: static (read, f) => read.Baseline.Count >= f.Depth
            && double.Abs(read.Value - Forecast(read.Baseline)) > f.EnvelopeWidth);

    // One pass carries both moments: recomputing the mean inside the deviation fold is quadratic in the
    // window and the ring is walked once per sample on every rule.
    static (double Mean, double Sigma) Moments(Seq<double> baseline) =>
        baseline.Fold((Sum: 0d, Squares: 0d), static (held, value) => (held.Sum + value, held.Squares + value * value))
            is var totals && baseline.Count is var n && n > 0
            ? (totals.Sum / n, double.Sqrt(double.Max(0d, totals.Squares / n - totals.Sum / n * (totals.Sum / n))))
            : (0d, 0d);

    // Ordinary least squares over the baseline's index axis, evaluated one step past its end, so the tested
    // value is genuinely out-of-sample and a drift toward a limit fires before the limit is crossed; a
    // two-point slope through the endpoints is the naive form that reads noise as trend.
    static double Forecast(Seq<double> baseline) {
        double count = baseline.Count;
        double meanX = (count - 1d) / 2d;
        double meanY = baseline.Fold(0d, static (sum, value) => sum + value) / count;
        (double Cov, double Var) fit = baseline
            .Map((value, index) => (X: index - meanX, Y: value - meanY))
            .Fold((Cov: 0d, Var: 0d), static (held, row) => (held.Cov + row.X * row.Y, held.Var + row.X * row.X));
        return meanY + (fit.Var > 0d ? fit.Cov / fit.Var : 0d) * (count - meanX);
    }
}
```

## [06]-[TS_PROJECTION]

- Owner: `HealthSnapshotWire`, `DegradationWire`, `CommandAvailabilityWire`, and `AlertReceiptWire` transcribe the snapshot, level, command-availability, and alert records the dashboard ingests; `CommandAvailabilityWire` is the ONE frozen name for the health/availability wire — the `DegradationLevel` command-availability projection (the level and the per-command verdict the `Agent/capability#DISCOVERY_FOLD` `Permitting` fold derives) the TS `state/evidence` `Availability` lattice decodes, its level roster mirroring the `DegradationLevel` rows one-to-one at the decode seam.
- Packages: BCL inbox
- Growth: one capability key row, one alert field, or one field on an owning wire record, zero new surface.
- Boundary: instants cross as extended-ISO text and elapsed spans as ISO-8601 duration text; level, cascade, capability, transition, and severity keys are the smart-enum string keys, status crosses as the camel-case enum name, never ordinals; `AlertSeverityKey` is the kernel routing pair the deploy plane's contact rows already key on, so the decode seam admits exactly the two rows the C# vocabulary carries and a four-tier ladder crossing this wire is the drift the collapse deleted; `DegradationWire` transcribes `DegradationState`'s own emission — the stored slots beside the `Floor` and `Level` projections the record already computes — registered at Runtime/ports#WIRE_LAW, so `rank` and `retains` never cross: both derive from the frozen `DegradationLevelKey` roster the decode seam already mirrors, and a wire field no C# record emits is the phantom this shape deletes; `cascade` is the parent-floored level a child reports, distinct from `forced` operator override.

```ts signature
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
  readonly derived: DegradationLevelKey;
  readonly forced: DegradationLevelKey | null;
  readonly cascade: DegradationLevelKey | null;
  readonly streak: number;
  readonly since: string | null;
  readonly floor: DegradationLevelKey;
  readonly level: DegradationLevelKey;
}

interface CommandAvailabilityWire {
  readonly level: DegradationLevelKey;
  readonly commands: Readonly<Record<string, boolean>>;
  readonly since: string;
}

type AlertSeverityKey = "page" | "ticket";

type AlertConditionKey = "threshold" | "anomaly-band" | "forecast-band";

type AlertTransitionKey = "fired" | "recovered" | "escalated";

interface AlertReceiptWire {
  readonly ruleId: string;
  readonly version: number;
  readonly severity: AlertSeverityKey;
  readonly condition: AlertConditionKey;
  readonly value: number;
  readonly transition: AlertTransitionKey;
  readonly at: string;
  readonly correlation: string;
}
```

## [07]-[RESEARCH]

(none)
