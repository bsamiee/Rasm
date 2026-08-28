# [APPHOST_HEALTH_AND_DEGRADATION]

Capability health and the usable-failure degradation ladder for every Rasm.AppHost process: one contributor row family folds package probes into a wire-neutral snapshot through a single `ProbeSource` union, a `DriverProbe`-keyed arm binds every admitted backing-service health check (Postgres, cache transit, NATS, Kafka, Redis, upstream HTTP, disk, allocation) onto its degradation rule through one shared driver instance, the five-level `DegradationLevel` ladder carries one retained `Faculty` set per rung, an alert roster derives from the grading table and sweeps every committed reading, and a wire-health mapping projects the registry onto the standard wire health service. Microsoft.Extensions.Diagnostics.HealthChecks supplies probe mechanics, the AspNetCore.HealthChecks.NpgSql/Nats/Redis/Kafka/Uris/System family supplies the concrete backing-service probes, ResourceMonitoring publishes utilization ratios and container ceilings on the Windows and Linux hosts it mints a snapshot provider for while the BCL process counters carry every other host, Thinktecture owns the vocabularies, LanguageExt and NodaTime carry the fold carriers and stamps; every consumer reads one level value.

Settled composition: `CapabilitySet<TCapability>`, `ICapability<TSelf>`, and `CapabilityLaw<TCapability>` arrive from `Rasm/Domain/validation#CAPABILITY`; `Transition<TState>` and `Cell.Step` from `Rasm/Domain/results#TRANSITION`; `Stat<Scalar>`, `SampleMoment`, `Scalar`, and `MomentNormalizer` from `Rasm/Domain/stats#MOMENTS`; `MonotonicTimeline` and `MonotonicStamp` from `Rasm/Parametric/projections#TIMELINE`; `CorrelationId` from `Rasm/Domain/frame#SOURCE` and `AlertSeverity` with its rank, hold, tone, urgency, and escalation-walk columns from `Rasm/Domain/objective#BURN` — the routing vocabulary the deploy plane's contact rows key on, so no severity ladder mints here; `TelemetrySource` from `Rasm/Domain/frame#SOURCE` and `HlcStamp` from `#STAMP`; `EventType` and `EventSource` from `Rasm/Domain/event#EVENT_GRAMMAR`. `AppHostPoint`, `AppHostFact`, and `HookSet` arrive from Observability/hooks#HOOK_ROSTER; `AppHostMeasure.HealthLevel` and the composition `InstrumentSet` from Observability/instruments#INSTRUMENT_CATALOG — `DegradationCell` writes that gauge on every reading it commits and mints no instrument of its own; `DomainEvent` and `Topic.Health` from Wire/topics#TOPIC_FABRIC. `ClockPolicy` with its `MonotonicTimeline Line` and `DeadlineClass` arrive from Runtime/time#CLOCK_POLICY and `#DEADLINE_TAXONOMY`; `EnergyCell` and `FidelityScale` from Runtime/profiles#POWER_AND_FIDELITY as the thermal-and-power half of the ONE pressure grade; `CapabilityRegistry` and `DiscoveryQuery` from Agent/capability#DISCOVERY_FOLD; `SuiteContracts.Host` from Runtime/ports#WIRE_LAW. `Slo.Specs` compiles error-budget burn onto the deploy plane from the `HostInstruments` board pack, so this engine alerts on health evidence alone.

## [01]-[INDEX]

- [02]-[HEALTH_FOLD]: Probe sources, the graded pressure axes, the utilization capsule, and one snapshot fold.
- [03]-[DEGRADATION_LADDER]: Faculty ladder, derivation fold with hysteresis, the fanning cell, and the command-availability wire.
- [04]-[WIRE_HEALTH]: Tag-predicate wire mapping, the out-of-band filtered evaluation, and the inbound set-degradation route.
- [05]-[ALERT_ENGINE]: Declarative alert rules over the degradation reading with hysteresis, escalation, and versioning.
- [06]-[TS_PROJECTION]: Health snapshot, degradation level, command availability, and alert wire shapes.

## [02]-[HEALTH_FOLD]

- Owner: `ContributorTag` `[SmartEnum<string>]` realizing kernel `ICapability<ContributorTag>` — the routing vocabulary a row declares and a rule keys on; `ProbeSource` `[Union]` — the closed family naming WHERE a probe reads and carrying its name, failure status, tag set, probe delegate, collection window, and service-registration arm as case columns; `HealthContributorRow` — the four-column registration record and the `IHealthCheck`; `DriverProbe` `[SmartEnum<string>]` — the backing-service axis binding each dependency kind to its tag and failure status; `MonitorFeature` `[SmartEnum<string>]` realizing `ICapability<MonitorFeature>` — the resource-monitor option axis; `PressureSource` `[Union]` — where a utilization ratio comes from, carrying the instrument pair its arm reads AND the `CapabilityLaw<MonitorFeature>` corners that arm admits; `PressureAxis` `[SmartEnum<string>]` — the graded-axis roster, each row carrying the reading projection and the render it stamps; `Band` — one axis's degraded and unhealthy ceilings; `PressurePolicy` — the source, cadence, ceiling roster, container quota, and admitted monitor features; `UtilizationCell` — the boundary capsule holding the listener seat, the observed pair, and the differencing marks; `HealthSnapshot` with nested `Entry` — the only health shape interiors read.
- Cases: four `ContributorTag` rows — `Host`, `Remote`, `Store`, `Pressure` — key the derivation rules and the wire predicates; three `ProbeSource` cases — `Gauge` over the utilization capsule and the energy cell, `Peer` over a peer process's wire health service, `Driver` over any admitted package check; three `PressureSource` rows — `Container` and `Host` over the ResourceMonitoring meter's limit-relative and process-relative gauge pairs, `Process` over the BCL counters; three `PressureAxis` rows — `Cpu`, `Memory`, `Fidelity`; two `MonitorFeature` rows — `CgroupV2` and `DiskIo`; eight `DriverProbe` rows tracking the LANDED Persistence sink roster — `Postgres`/`Cache`/`Redis` (Store), `Nats`/`Kafka`/`Upstream` (Remote), `Disk`/`Allocations` (Pressure).
- Entry: `HealthContributorRow.Of(ProbeSource source, Duration cadence)` is the ONE row construction; `builder.Register(params ReadOnlySpan<HealthContributorRow> rows)` folds every row's service registration and its `HealthCheckRegistration` in one pass; `UtilizationCell.Of(PressureSource, MonotonicTimeline)` is the one construction, mounting the meter arm's listener as it builds; `UtilizationCell.Read()` is the one utilization pull returning the typed result; `PressurePolicy.Container(ResourceQuotaProvider)` admits the container arm against its own corner law; `report.Snapshot(Instant at, CorrelationId correlation)` is the pure report fold.
- Auto: `Register` seats BOTH halves of every row — `ProbeSource.Mount` registers the resource monitor a `Gauge` source needs and returns the collection untouched for every other case, then the row's own `Registration` lands its `FailureStatus`, `Tags`, `Timeout`, `Delay`, and `Period` as registration policy rather than probe-local exception handling; the `Driver` case adapts ANY admitted package check — `NpgSqlHealthCheck` over the pooled `NpgsqlDataSource`, the `Cache` row's L2-transit probe over the ONE `IDistributedCache` the `Runtime/resources#CACHE_PORT` rides (through `Microsoft.Extensions.Caching.StackExchangeRedis` when the deploy binds redis — the raw `IConnectionMultiplexer` driver is PRUNED Persistence-side, so a direct-multiplexer probe is the deleted form), `NatsHealthCheck` over the pooled `INatsConnection`, `KafkaHealthCheck` over its sink `ProducerConfig`, `RedisHealthCheck` over its sink connection, `UriHealthCheck` over a service-discovery `AddUrlGroup`, and `DiskStorageHealthCheck`/`ProcessAllocatedMemoryHealthCheck` over the BCL counters — through one synthetic `HealthCheckContext` seating the row's own failure status, so the packages enter as rows rather than parallel `Add*` registration faces; the monitor arm shapes options through the standard builder — `UseZeroToOneRangeForMetrics` and `UseZeroToOneRangeForLinuxMetrics` together pin every platform's series to the zero-to-one axis the ceilings compare against, while `UseLinuxCalculationV2` and `EnableSystemDiskIoMetrics` read the admitted `MonitorFeature` set; the probe's own cadence is the pull cadence, so `Delay` seats one sampling interval and the source's `Span` answers the collection window, and no publisher window exists to alias against them.
- Output: `HealthSnapshot` stamped with `Instant` and `CorrelationId`; `HealthReport` never crosses the fold.
- Packages: Rasm, Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.ResourceMonitoring, Microsoft.Extensions.Options, AspNetCore.HealthChecks.NpgSql, AspNetCore.HealthChecks.Nats, AspNetCore.HealthChecks.Redis, AspNetCore.HealthChecks.Kafka, AspNetCore.HealthChecks.Uris, AspNetCore.HealthChecks.System, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one capability probe is one `ProbeSource` case with its five columns, breaking every fold arm at compile time; one backing-service dependency is one `DriverProbe` row binding its tag and failure status; one utilization authority is one `PressureSource` case with its own corner law; one graded axis is one `PressureAxis` row and one `Band` on `PressurePolicy.Ceilings`, so a thermal, power, or IO retune is a ceiling edit and never a parallel policy; one routing facet is one `ContributorTag` row.
- Boundary: package health types stop at this boundary — interiors read `HealthSnapshot` and one level value; a `Driver` case binds the SAME pooled `NpgsqlDataSource`, the one L2 `IDistributedCache` transit, and the pooled `INatsConnection` the production path owns, so a probe shares connection pressure with live traffic and never opens a second out-of-pool connection, and its tag routes a faulted dependency onto an EXISTING degradation rule (`Store` -> `ReadOnly`, `Remote` -> `ReducedRemote`, `Pressure` -> `Degraded`) with zero added `Rule`; the roster is seed DATA tracking the landed Persistence egress sink roster, so the probe axis never drifts beside the roster it probes, and WHICH rows a deployment registers is the composition's own argument list at Runtime/modules#MODULE_LEDGER rather than a `Default` column no fold read — a per-row registration flag and the registration argument that overrides it are two answers to one question; the `Disk`/`Allocations` probes are the discrete hard-ceiling complement to the continuous `Gauge` reading, not a second utilization source — they grade an absolute breach the windowed ratio does not express, both projecting into the one `Pressure`-tagged contributor set; `Peer` cases read a peer process over its wire health service, so cross-process health is a read, never shared state; the `Gauge` case folds CPU, memory, and the `Runtime/profiles#POWER_AND_FIDELITY` fidelity grade onto ONE `Pressure`-tagged row, and `FidelityScale.Grade` is what makes that one row honest — it already folds thermal ahead of power state, so a thermal axis beside the fidelity axis grades one measurement twice and the branch RULINGS `[02]` thermal-and-power clause becomes a structural fact rather than a convention; the fidelity axis inverts against the roster's own top row so pressure rises as fidelity falls, and an unread power authority grades `Balanced` at profiles, one rung below the degraded ceiling, so absence never escalates a health rule; an unreadable source grades `Degraded` with its captured cause as the entry detail, because a void probe's one evidence surface is the result it returns and a zero reading grades healthy. `PressureSource` seats the three arms this branch serves — the `Process` row over BCL counters, `Host` and `Container` over the meter where the package registers a snapshot source; `Container` reads `container.cpu.limit.utilization`/`container.memory.limit.utilization` because `UseLinuxCalculationV2` publishes no `process.cpu.utilization` at all and the non-V2 process gauge scales against the cpu REQUEST, so a container arm reading the process pair grades the wrong ceiling or nothing — and that conditional roster IS the `Host` row's corner law: every `MonitorFeature` set holding `CgroupV2` is barred there, which is the legal-corner statement no bool pair carries, while the `Process` row admits the empty set alone because its arm registers no meter for a feature to shape; the two range flips are one invariant across two platform halves rather than a policy — the Linux flip defaults on and the cross-platform flip defaults OFF, so a Windows host left unflipped emits `[0, 100]` against ratio ceilings and suspends itself at one percent load, and both are pinned in the arm rather than exposed as features; `ResourceQuota` carries the `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings that `ResourceQuotaProvider.GetResourceQuota()` supplies to `PressurePolicy.Quota` as the ceiling evidence the grade detail stamps, never a re-derived second ratio, and that provider registers on every Linux host but only inside a Windows job object; the meter arm's observable instruments deliver nothing until asked, so `Read` drives the listener's own observation and a listener merely started is the silently-dead form the pull deletes; listener mount rides `Of` because a separable attach step left an unlistened meter cell constructible, and a metered `Read` refuses until BOTH named instruments have published, so the package's own no-snapshot-source-outside-Windows-and-Linux return surfaces as a `Degraded` entry carrying its cause; the counter arm refuses on the SAME terms — a span the monotonic timeline has not advanced and a GC memory ceiling that resolves to zero each DECLINE the transition, so the cell keeps its last admitted pair and the read reports absence, where the prior form committed a fabricated zero share and graded a saturated host healthy; NAMED LOSS on the tag column — `HealthReport` hands tags back as the strings the registration wrote, so the snapshot re-admits them once through `ContributorTag.Admit` and a tag no row names cannot key a rule and does not cross, leaving a foreign package's own tags on its own `HealthCheckRegistration` where its own reader takes them.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Thinktecture;
using Sample = (LanguageExt.Option<Rasm.AppHost.Observability.Utilization> Usage, Rasm.MonotonicStamp At, NodaTime.Duration Cpu);

namespace Rasm.AppHost.Observability;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContributorTag : ICapability<ContributorTag> {
    public static readonly ContributorTag Host = new("host", rank: 0);
    public static readonly ContributorTag Remote = new("remote", rank: 1);
    public static readonly ContributorTag Store = new("store", rank: 2);
    public static readonly ContributorTag Pressure = new("pressure", rank: 3);

    public int Rank { get; }

    public static CapabilitySet<ContributorTag> Admit(IEnumerable<string> keys) =>
        CapabilitySet<ContributorTag>.Of([.. keys.Select(static key => TryGet(out ContributorTag? row) ? row : null).OfType<ContributorTag>()]);

    static IReadOnlyList<ContributorTag> ICapability<ContributorTag>.Items => Items;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MonitorFeature : ICapability<MonitorFeature> {
    public static readonly MonitorFeature CgroupV2 = new("cgroup-v2", rank: 0);
    public static readonly MonitorFeature DiskIo = new("disk-io", rank: 1);

    public int Rank { get; }

    static IReadOnlyList<MonitorFeature> ICapability<MonitorFeature>.Items => Items;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DriverProbe {
    public static readonly DriverProbe Postgres = new("npgsql", ContributorTag.Store, HealthStatus.Unhealthy);
    public static readonly DriverProbe Cache = new("cache-l2", ContributorTag.Store, HealthStatus.Unhealthy);
    public static readonly DriverProbe Redis = new("redis", ContributorTag.Store, HealthStatus.Unhealthy);
    public static readonly DriverProbe Nats = new("nats", ContributorTag.Remote, HealthStatus.Unhealthy);
    public static readonly DriverProbe Kafka = new("kafka", ContributorTag.Remote, HealthStatus.Unhealthy);
    public static readonly DriverProbe Upstream = new("uris", ContributorTag.Remote, HealthStatus.Unhealthy);
    public static readonly DriverProbe Disk = new("diskstorage", ContributorTag.Pressure, HealthStatus.Degraded);
    public static readonly DriverProbe Allocations = new("process_allocated_memory", ContributorTag.Pressure, HealthStatus.Degraded);

    public ContributorTag Tag { get; }
    public HealthStatus Failing { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PressureAxis {
    public static readonly PressureAxis Cpu = new("cpu", read: static row => row.Usage.CpuRatio, render: "P0");
    public static readonly PressureAxis Memory = new("memory", read: static row => row.Usage.MemoryRatio, render: "P0");
    public static readonly PressureAxis Fidelity = new(
        "fidelity", read: static row => FidelityScale.Burst.FidelityTier - row.Fidelity.FidelityTier, render: "0");

    [UseDelegateFromConstructor]
    public partial double Read(PressureReading reading);

    public string Render { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressureSource {
    private PressureSource() { }

    public abstract string Key { get; }
    public abstract CapabilityLaw<MonitorFeature> Law { get; }

    public sealed record Metered(string Cpu, string Memory, CapabilityLaw<MonitorFeature> Corners) : PressureSource {
        public override string Key => Cpu;
        public override CapabilityLaw<MonitorFeature> Law => Corners;
    }

    public sealed record Runtime : PressureSource {
        public override string Key => "dotnet.process.cpu";
        public override CapabilityLaw<MonitorFeature> Law => new(Seq(CapabilitySet<MonitorFeature>.None));
    }

    public static readonly PressureSource Container = new Metered(
        "container.cpu.limit.utilization", "container.memory.limit.utilization", CapabilityLaw<MonitorFeature>.Open);

    public static readonly PressureSource Host = new Metered(
        "process.cpu.utilization", "dotnet.process.memory.virtual.utilization",
        CapabilityLaw<MonitorFeature>.Forbidden(Seq(
            CapabilitySet<MonitorFeature>.Of(MonitorFeature.CgroupV2))));

    public static readonly PressureSource Process = new Runtime();

    public IServiceCollection Register(IServiceCollection services, CapabilitySet<MonitorFeature> features) =>
        Switch(
            state: (Services: services, Features: features),
            metered: static (bound, _) => bound.Services
                .AddResourceMonitoring()
                .Configure<ResourceMonitoringOptions>(options => {
                    options.UseZeroToOneRangeForMetrics = true;
                    options.UseZeroToOneRangeForLinuxMetrics = true;
                    options.UseLinuxCalculationV2 = bound.Features.Admits(MonitorFeature.CgroupV2);
                    options.EnableSystemDiskIoMetrics = bound.Features.Admits(MonitorFeature.DiskIo);
                }),
            runtime: static (bound, _) => bound.Services);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProbeSource {
    private ProbeSource() { }

    public abstract string Name { get; }
    public abstract HealthStatus Failing { get; }
    public abstract CapabilitySet<ContributorTag> Tags { get; }
    public abstract Func<CancellationToken, ValueTask<HealthCheckResult>> Probe { get; }
    public abstract Duration Span(Duration cadence);
    public abstract IServiceCollection Mount(IServiceCollection services);

    public sealed record Gauge(UtilizationCell Cell, EnergyCell Energy, PressurePolicy Policy) : ProbeSource {
        public override string Name => nameof(Gauge);
        public override HealthStatus Failing => HealthStatus.Degraded;
        public override CapabilitySet<ContributorTag> Tags => CapabilitySet<ContributorTag>.Of(ContributorTag.Pressure);
        public override Func<CancellationToken, ValueTask<HealthCheckResult>> Probe =>
            _ => ValueTask.FromResult(Cell.Read().Match(
                Succ: usage => Policy.Grade(new PressureReading(usage, Energy.Read())),
                Fail: fault => new HealthCheckResult(HealthStatus.Degraded, fault.Message)));
        public override Duration Span(Duration cadence) => Policy.Window;
        public override IServiceCollection Mount(IServiceCollection services) =>
            Policy.Source.Register(services, Policy.Features);
    }

    public sealed record Peer(string Service, ContributorTag Facet, Func<CancellationToken, ValueTask<HealthCheckResult>> Read) : ProbeSource {
        public override string Name => Service;
        public override HealthStatus Failing => HealthStatus.Unhealthy;
        public override CapabilitySet<ContributorTag> Tags => CapabilitySet<ContributorTag>.Of(ContributorTag.Remote, Facet);
        public override Func<CancellationToken, ValueTask<HealthCheckResult>> Probe => Read;
        public override Duration Span(Duration cadence) => cadence;
        public override IServiceCollection Mount(IServiceCollection services) => services;
    }

    public sealed record Driver(DriverProbe Row, IHealthCheck Check) : ProbeSource {
        private readonly HealthCheckContext seated = new() {
            Registration = new HealthCheckRegistration(
                Row.Key, Check, Row.Failing,
                HealthContributorRow.Keys(CapabilitySet<ContributorTag>.Of(Row.Tag)),
                DeadlineClass.HealthProbe.Allotted.ToTimeSpan()),
        };

        public override string Name => Row.Key;
        public override HealthStatus Failing => Row.Failing;
        public override CapabilitySet<ContributorTag> Tags => CapabilitySet<ContributorTag>.Of(Row.Tag);
        public override Func<CancellationToken, ValueTask<HealthCheckResult>> Probe =>
            token => new ValueTask<HealthCheckResult>(Check.CheckHealthAsync(seated, token));
        public override Duration Span(Duration cadence) => cadence;
        public override IServiceCollection Mount(IServiceCollection services) => services;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Utilization(double CpuRatio, double MemoryRatio);

public readonly record struct PressureReading(Utilization Usage, FidelityScale Fidelity);

public readonly record struct Band(double Degraded, double Unhealthy) {
    public HealthStatus Grade(double value) =>
        value >= Unhealthy ? HealthStatus.Unhealthy
        : value >= Degraded ? HealthStatus.Degraded
        : HealthStatus.Healthy;
}

public sealed record PressurePolicy(
    PressureSource Source,
    Duration Window,
    Duration Sampling,
    Seq<(PressureAxis Axis, Band Band)> Ceilings,
    Option<ResourceQuota> Quota,
    CapabilitySet<MonitorFeature> Features) {
    public static readonly PressurePolicy Canonical = new(
        Source: PressureSource.Process,
        Window: Duration.FromSeconds(5), Sampling: Duration.FromSeconds(1),
        Ceilings: Seq(
            (PressureAxis.Cpu, new Band(Degraded: 0.80d, Unhealthy: 0.92d)),
            (PressureAxis.Memory, new Band(Degraded: 0.85d, Unhealthy: 0.95d)),
            (PressureAxis.Fidelity, new Band(
                Degraded: FidelityScale.Burst.FidelityTier - FidelityScale.Sustained.FidelityTier,
                Unhealthy: FidelityScale.Burst.FidelityTier - FidelityScale.Conserve.FidelityTier))),
        Quota: None,
        Features: CapabilitySet<MonitorFeature>.None);

    public static Fin<PressurePolicy> Container(ResourceQuotaProvider quotas) =>
        Admit(Canonical with {
            Source = PressureSource.Container,
            Quota = Optional(quotas.GetResourceQuota()),
            Features = CapabilitySet<MonitorFeature>.All,
        });

    public static Fin<PressurePolicy> Admit(PressurePolicy held) =>
        held.Source.Law.Admit(held.Features).Map(_ => held);

    public HealthCheckResult Grade(PressureReading reading) =>
        Graded(Ceilings.Fold(
            (Status: HealthStatus.Healthy, Read: Seq<string>()),
            (held, row) => Folded(held, row, row.Axis.Read(reading))));

    static (HealthStatus Status, Seq<string> Read) Folded(
        (HealthStatus Status, Seq<string> Read) held, (PressureAxis Axis, Band Band) row, double value) =>
        (row.Band.Grade(value) is var status && status < held.Status ? status : held.Status,
         held.Read.Add($"{row.Axis.Key} {value.ToString(row.Axis.Render)}"));

    HealthCheckResult Graded((HealthStatus Status, Seq<string> Read) fold) =>
        new(fold.Status, $"{Source.Key} {string.Join(' ', fold.Read)}{Limits}");

    string Limits => Quota.Match(
        Some: static quota => $" limit {quota.MaxCpuInCores:0.##}cpu {quota.MaxMemoryInBytes}By baseline {quota.BaselineCpuInCores:0.##}cpu {quota.BaselineMemoryInBytes}By",
        None: static () => string.Empty);
}

public sealed record HealthContributorRow(
    ProbeSource Source,
    DeadlineClass Timeout,
    Duration Delay,
    Duration Period) : IHealthCheck {
    public static HealthContributorRow Of(ProbeSource source, Duration cadence) =>
        new(Source: source, Timeout: DeadlineClass.HealthProbe, Delay: cadence, Period: source.Span(cadence));

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Source.Probe(cancellationToken).AsTask();

    public static FrozenSet<string> Keys(CapabilitySet<ContributorTag> tags) =>
        tags.Held.OrderBy(static row => row.Rank).Select(static row => row.Key).ToFrozenSet(StringComparer.Ordinal);

    public HealthCheckRegistration Registration =>
        new(Source.Name, _ => this, Source.Failing, Keys(Source.Tags), Timeout.Allotted.ToTimeSpan()) {
            Delay = Delay.ToTimeSpan(),
            Period = Period.ToTimeSpan(),
        };
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
        CapabilitySet<ContributorTag> Tags,
        Option<string> Detail = default);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class UtilizationCell : IDisposable {
    public const string Meter = "Microsoft.Extensions.Diagnostics.ResourceMonitoring";
    private static readonly Error Unobserved =
        new KernelFault.InvalidValue(Label: nameof(UtilizationCell), Requirement: "a published utilization measurement");
    private static readonly Error Unmeasured =
        new KernelFault.InvalidValue(Label: nameof(UtilizationCell), Requirement: "an advanced sample span and a resolved memory ceiling");

    private readonly PressureSource source;
    private readonly MonotonicTimeline line;
    private readonly MeterListener listener = new();
    private readonly Atom<(Option<double> Cpu, Option<double> Memory)> observed = Atom((Option<double>.None, Option<double>.None));
    private readonly Atom<Sample> counted;

    private UtilizationCell(PressureSource source, MonotonicTimeline line, MonotonicStamp seed) {
        this.source = source;
        this.line = line;
        this.key = key;
        counted = Atom<Sample>((None, seed, Duration.FromTimeSpan(Environment.CpuUsage.TotalTime)));
    }

    public static Fin<UtilizationCell> Of(PressureSource source, MonotonicTimeline line) =>
        line.Capture()
            .Map(seed => new UtilizationCell(source, line, seed))
            .Map(static held => held.source.Switch(
                metered: static row => cell.Listening(row),
                runtime: static _ => cell));

    public Fin<Utilization> Read() => source.Switch(
        state: this,
        metered: static (held, _) => Try.lift(() => Fin.Succ(held.Pulled())).Run().Bind(static inner => inner).Bind(static usage => usage.ToFin(Unobserved)),
        runtime: static (held, _) => held.Counted());

    public void Dispose() => listener.Dispose();

    Option<Utilization> Pulled() {
        listener.RecordObservableInstruments();
        (Option<double> Cpu, Option<double> Memory) held = observed.Value;
        return held.Cpu.Bind(cpu => held.Memory.Map(memory => new Utilization(cpu, memory)));
    }

    Fin<Utilization> Counted() =>
        line.Capture(key).Bind(now => Settled(Cell.Step(
            counted,
            held => Differenced(held, now, Duration.FromTimeSpan(Environment.CpuUsage.TotalTime)),
            Unmeasured)));

    static Fin<Utilization> Settled(Transition<Sample> moved) =>
        moved is Transition<Sample>.Committed row
            ? row.State.Usage.ToFin(Unmeasured)
            : Fin.Fail<Utilization>(Unmeasured);

    Option<Sample> Differenced(Sample prior, MonotonicStamp now, Duration cpu) =>
        from span in line.Elapsed(prior.At, now, key).ToOption().Filter(static held => held > TimeSpan.Zero)
        from ceiling in Some(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes).Filter(static bytes => bytes > 0L)
        select ((Option<Utilization>)Some(new Utilization(
                    double.Min(1d, (cpu - prior.Cpu).TotalSeconds / (span.TotalSeconds * Environment.ProcessorCount)),
                    double.Min(1d, Environment.WorkingSet / (double)ceiling))),
                now, cpu);

    UtilizationCell Listening(PressureSource.Metered row) {
        listener.InstrumentPublished = (instrument, active) => {
            if (instrument.Meter.Name == Meter && (instrument.Name == row.Cpu || instrument.Name == row.Memory))
                active.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
            ignore(observed.Swap(current => instrument.Name == row.Cpu
                ? (Some(measurement), current.Memory)
                : (current.Cpu, Some(measurement)))));
        listener.Start();
        return this;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class HealthSurface {
    extension(IHealthChecksBuilder builder) {
        public IHealthChecksBuilder Register(params ReadOnlySpan<HealthContributorRow> rows) =>
            Iterable<HealthContributorRow>.FromSpan(rows).Fold(builder, static (admitted, row) =>
                (row.Source.Mount(admitted.Services), admitted.Add(row.Registration)).Item2);
    }
    extension(HealthReport report) {
        public HealthSnapshot Snapshot(Instant at, CorrelationId correlation) =>
            new(report.Status, at, correlation,
                report.Entries.AsIterable().Map(static entry => new HealthSnapshot.Entry(
                    entry.Key,
                    entry.Value.Status,
                    Duration.FromTimeSpan(entry.Value.Duration),
                    ContributorTag.Admit(entry.Value.Tags),
                    Optional(entry.Value.Description))).ToSeq());
    }
}
```

## [03]-[DEGRADATION_LADDER]

- Owner: `Faculty` `[SmartEnum<string>]` realizes kernel `ICapability<Faculty>`; `CommandAccess` `[SmartEnum<string>]` carries the per-level default verdict; `CommandVerdict` `[Union]` is the three-case domain answer; `DegradationLevel` `[SmartEnum<string>]` carries rank, posture, retained set, and generated enum value; `DegradationPolicy` is the derivation table; `DegradationCell` owns the coherent reading atom; `CommandAvailability` projects deviations directly onto generated `Availability.CommandAvailability` and its verdict oneof.
- Cases: `Full(0)`, `ReducedRemote(1)`, `LocalOnly(2)`, `ReadOnly(3)`, `Suspended(4)` in severity order; six `Faculty` rows form the retained sets; three `CommandAccess` postures — `All`, `Reads`, `None`; three `CommandVerdict` cases — `Available`, `Gated(reason)`, `Withheld(level, reason)`.
- Entry: `Derive(DegradationState state, HealthSnapshot snapshot)` folds rules with escalation-immediate, recovery-hysteresis semantics; `Force(Option<DegradationLevel> forced)` is the single override entrypoint; `Cascade(Option<DegradationLevel> parent)` admits a parent-forced level as a derivation floor; `Read()` returns the coherent reading; `CommandAvailability.Of(CapabilityRegistry, DegradationState, Instant)` returns the generated protobuf carrier.
- Auto: `DegradationCell` registers as the `IHealthCheckPublisher` and owns one `Atom<DegradationReading>` — `PublishAsync` snapshots the `HealthReport` and folds `Derive` in the SAME swap, so the published snapshot and the level it produced are one atomic transition and a reader can never observe a fresh level against a stale snapshot or the reverse; `HealthCheckPublisherOptions` binds `Delay` and `Period` from `DegradationPolicy.Canonical` and `Timeout` from `DeadlineClass.HealthProbe`; `OperatorOverride` projects onto `Force` at the composition root — forced beats derived, release re-derives; `Force` and `Cascade` swap the `State` slot of the reading while preserving the last snapshot so the override is coherent with the evidence it overrides; every committed reading — derived, forced, cascaded alike — fires the `Observability/hooks#HOOK_ROSTER` `Degradation` replay row through the fact's own projected seat, so the held window carries the trajectory an attaching panel reads, the cell writes the `AppHostMeasure.HealthLevel` gauge on that same commit, and the alert sweep folds the same value.
- Output: `DegradationReading` carries the latest `HealthSnapshot` and the `DegradationState` (derived level, forced input, cascade floor, recovery streak, dwell anchor); a `Level` change reaches the lifecycle as the degraded or recovered step through `Lifecycle.DegradationTap`.
- Growth: one `Rule` row or one `Faculty` case absorbs a new degradation driver; a new rung is one `DegradationLevel` row naming the ONE faculty it forfeits and the posture it publishes; a new pressure-read consumer reads the one `DegradationReading` value, never a second cell — zero new surface.
- Boundary: this ladder runs as a monotone forfeiture chain — each rung derives its retained set from the rung above by dropping exactly one `Faculty`, so a rung that both drops and re-admits a capability is unspellable and no membership list is restated; degradation is process-local, peer-health-informed, and parent-cascade-floored — a peer level never propagates as this process's level, but a parent process's forced level enters `Derive` as a floor through `Cascade`, never as shared state; the snapshot and the derived level are one `DegradationReading` atom so the `Runtime/laneguard#LANE_GUARD` governor reads a coherent `(snapshot, level)` pressure value for its adaptive-concurrency and load-shed decisions, and a governor reading a stale snapshot against a fresh level is the race the single atom forecloses; `LocalOnly` is the host-absent fold — `Faculty.HostDocument` leaves the retained set and document sources yield absence; the container-limit pressure signal enters the rank algebra as data, not a new rule — a `PressurePolicy.Container` row grades against `ResourceQuota` so the `Pressure`-tagged row escalates on the cgroup limit and the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules carry that limit-relative status into `Derive` with the same retained-set hysteresis; the fan's own refusal rides the `Fin` out of `Force`, `Cascade`, and `PublishAsync`, so a dispatcher whose point never seated fails the publish rather than dropping every transition an attaching panel exists to replay; cross-process cascade splits at a boundary the snapshot fold preserves — the READ stays the owner here and the WRITE lands at `Wire/companion#DEGRADATION_CASCADE`, which calls `Cascade` with the observed parent level mutating only the `State` slot, release passing `None` so the cell re-derives off its own snapshots and the cascade floor never escalates below local pressure. NAMED LOSS on the availability carrier: the per-descriptor `false` row. Peer decoders answer an absent command off the level's OWN posture (`typescript core state/evidence#AVAILABILITY_LATTICE` `Availability.admits`), so the whole-catalog transcription was the level restated once per descriptor; what crosses now is exactly the complement of that posture — an admitting level sends its refusals, a withholding one its exceptions — and the answer gains a reason where a boolean carried none, `CommandAccess` being the .NET owner of the posture the peer's own row table had been mirroring with no producer to read it.

```csharp
using Google.Protobuf.WellKnownTypes;
using NodaTime.Serialization.Protobuf;
// Contracts are retired from this logic.

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class Faculty : ICapability<Faculty> {
    public static readonly Faculty HostDocument = new("host-document", rank: 0);
    public static readonly Faculty RemoteCompute = new("remote-compute", rank: 1);
    public static readonly Faculty LocalCompute = new("local-compute", rank: 2);
    public static readonly Faculty StoreWrite = new("store-write", rank: 3);
    public static readonly Faculty StoreRead = new("store-read", rank: 4);
    public static readonly Faculty TelemetryExport = new("telemetry-export", rank: 5);

    public int Rank { get; }

    static IReadOnlyList<Faculty> ICapability<Faculty>.Items => Items;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommandAccess {
    public static readonly CommandAccess All = new(
        "all", admitting: true, verdict: static _ => new CommandVerdict.Available());
    public static readonly CommandAccess Reads = new(
        "reads", admitting: false, verdict: static level => new CommandVerdict.Gated(level.Key));
    public static readonly CommandAccess None = new(
        "none", admitting: false, verdict: static level => new CommandVerdict.Withheld(level, level.Key));

    [UseDelegateFromConstructor]
    public partial CommandVerdict Verdict(DegradationLevel level);

    public bool Admitting { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandVerdict {
    private CommandVerdict() { }
    public sealed record Available : CommandVerdict;
    public sealed record Gated(string Reason) : CommandVerdict;
    public sealed record Withheld(DegradationLevel Level, string Reason) : CommandVerdict;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DegradationLevel {
    public static readonly DegradationLevel Full = new(
        "full", rank: 0, access: CommandAccess.All, retains: CapabilitySet<Faculty>.All,
        wire: Control.DegradationLevel.Full);
    public static readonly DegradationLevel ReducedRemote = new(
        "reduced-remote", rank: 1, access: CommandAccess.All, retains: Full.Retains.Without(Faculty.RemoteCompute),
        wire: Control.DegradationLevel.ReducedRemote);
    public static readonly DegradationLevel LocalOnly = new(
        "local-only", rank: 2, access: CommandAccess.All, retains: ReducedRemote.Retains.Without(Faculty.HostDocument),
        wire: Control.DegradationLevel.LocalOnly);
    public static readonly DegradationLevel ReadOnly = new(
        "read-only", rank: 3, access: CommandAccess.Reads, retains: LocalOnly.Retains.Without(Faculty.StoreWrite),
        wire: Control.DegradationLevel.ReadOnly);
    public static readonly DegradationLevel Suspended = new(
        "suspended", rank: 4, access: CommandAccess.None, retains: ReadOnly.Retains.Without(Faculty.LocalCompute),
        wire: Control.DegradationLevel.Suspended);

    public int Rank { get; }
    public CommandAccess Access { get; }
    public CapabilitySet<Faculty> Retains { get; }
    public Control.DegradationLevel Wire { get; }

    static readonly FrozenDictionary<Control.DegradationLevel, DegradationLevel> ByWire =
        Items.ToFrozenDictionary(static row => row.Wire);

    public static Option<DegradationLevel> OfWire(Control.DegradationLevel wire) =>
        ByWire.TryGetValue(wire, out var row) ? Optional(row) : None;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DegradationState(
    DegradationLevel Derived,
    int Streak,
    Option<DegradationLevel> Forced = default,
    Option<DegradationLevel> Cascade = default,
    Option<Instant> Since = default) {
    public static readonly DegradationState Boot = new(DegradationLevel.Full, Streak: 0, Forced: None, Cascade: None, Since: None);
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
    public readonly record struct Rule(ContributorTag Tag, HealthStatus Trigger, DegradationLevel Outcome);

    public static readonly DegradationPolicy Canonical = new(
        Rules: [
            new Rule(ContributorTag.Remote, HealthStatus.Unhealthy, DegradationLevel.ReducedRemote),
            new Rule(ContributorTag.Host, HealthStatus.Unhealthy, DegradationLevel.LocalOnly),
            new Rule(ContributorTag.Store, HealthStatus.Unhealthy, DegradationLevel.ReadOnly),
            new Rule(ContributorTag.Pressure, HealthStatus.Degraded, DegradationLevel.ReducedRemote),
            new Rule(ContributorTag.Pressure, HealthStatus.Unhealthy, DegradationLevel.Suspended),
        ],
        ConsecutiveHealthy: 3,
        MinimumDwell: Duration.FromSeconds(60),
        PublishDelay: Duration.FromSeconds(5),
        PublishPeriod: Duration.FromSeconds(30));

    public DegradationState Derive(DegradationState state, HealthSnapshot snapshot) =>
        (Candidate: Candidate(snapshot), Rank: state.Derived.Rank) switch {
            var fold when fold.Candidate.Rank > fold.Rank =>
                new DegradationState(fold.Candidate, Streak: 0, Forced: state.Forced, Cascade: state.Cascade, Since: Optional(snapshot.At)),
            var fold when fold.Candidate.Rank == fold.Rank => state with { Streak = 0 },
            var fold when state.Streak + 1 >= ConsecutiveHealthy
                && state.Since.Map(since => snapshot.At - since >= MinimumDwell).IfNone(true) =>
                new DegradationState(fold.Candidate, Streak: 0, Forced: state.Forced, Cascade: state.Cascade, Since: Optional(snapshot.At)),
            _ => state with { Streak = state.Streak + 1 },
        };

    private DegradationLevel Candidate(HealthSnapshot snapshot) =>
        Rules.Fold(DegradationLevel.Full, (worst, rule) =>
            rule.Outcome.Rank > worst.Rank
                && snapshot.Entries.Exists(entry => entry.Status == rule.Trigger && entry.Tags.Admits(rule.Tag))
                ? rule.Outcome
                : worst);
}

public readonly record struct DegradationReading(HealthSnapshot Snapshot, DegradationState State) {
    public static DegradationReading Boot(Instant at, CorrelationId correlation) =>
        new(new HealthSnapshot(HealthStatus.Healthy, at, correlation, []), DegradationState.Boot);
    public DegradationLevel Level => State.Level;
}

public static class CommandAvailability {
    public static Host.CommandAvailability Of(CapabilityRegistry registry, DegradationState state, Instant since) =>
        Deviating(state.Level, since,
            permitted: toSet(registry.Discover(new DiscoveryQuery.Permitting(state.Level)).Map(static row => row.Descriptor)),
            catalog: toSet(registry.Discover(new DiscoveryQuery.All()).Map(static row => row.Descriptor)));

    static Host.CommandAvailability Deviating(
        DegradationLevel level, Instant since, Set<string> permitted, Set<string> catalog) =>
        level.Access.Admitting
            ? Crossing(level, since, catalog - permitted, new CommandVerdict.Withheld(level, level.Key))
            : Crossing(level, since, permitted, new CommandVerdict.Available());

    static Host.CommandAvailability Crossing(
        DegradationLevel level, Instant since, Set<string> rows, CommandVerdict verdict) {
        Host.CommandAvailability wire = new() { Level = level.Wire, Since = since.ToTimestamp() };
        rows.Iter(descriptor => wire.Commands.Add(descriptor, Verdict(verdict)));
        return wire;
    }

    static Host.CommandVerdictWire Verdict(CommandVerdict verdict) => verdict.Switch(
        available: static _ => new Host.CommandVerdictWire { Available = new Empty() },
        gated: static row => new Host.CommandVerdictWire {
            Gated = new Host.CommandVerdictWire.Types.Gated { Reason = row.Reason },
        },
        withheld: static row => new Host.CommandVerdictWire {
            Withheld = new Host.CommandVerdictWire.Types.Withheld {
                Level = row.Level.Wire,
                Reason = row.Reason,
            },
        });
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DegradationCell(
    DegradationPolicy policy,
    IClock clock,
    CorrelationId correlation,
    InstrumentSet signals,
    HookSet<AppHostPoint, AppHostFact, TelemetrySource> hooks) : IHealthCheckPublisher {
    private readonly Atom<DegradationReading> cell = Atom(DegradationReading.Boot(clock.GetCurrentInstant(), correlation));

    public DegradationReading Read() => cell.Value;
    public DegradationState State => cell.Value.State;
    public DegradationLevel Level => cell.Value.Level;

    public Fin<DegradationState> Force(Option<DegradationLevel> forced) =>
        Fired(cell.Swap(reading => reading with { State = reading.State with { Forced = forced } })).Map(static held => held.State);

    public Fin<DegradationState> Cascade(Option<DegradationLevel> parent) =>
        Fired(cell.Swap(reading => reading with { State = reading.State with { Cascade = parent } })).Map(static held => held.State);

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken) =>
        Folded(report.Snapshot(clock.GetCurrentInstant(), correlation)).Match(
            Succ: static _ => Task.CompletedTask,
            Fail: static fault => Task.FromException(fault.ToException()));

    Fin<DegradationReading> Folded(HealthSnapshot snapshot) =>
        Fired(cell.Swap(reading => new DegradationReading(snapshot, policy.Derive(reading.State, snapshot))));

    Fin<DegradationReading> Fired(DegradationReading reading) =>
        signals.Level(AppHostMeasure.HealthLevel.Row, reading.Level.Rank)
            .Bind(_ => hooks.Fire(at: AppHostPoint.Degradation, fact: new AppHostFact.Degradation(reading)))
            .Map(_ => reading);
}
```

## [04]-[WIRE_HEALTH]

- Owner: `WireHealthRow` binds one wire service name to one tag predicate; `WireHealth` attaches the filtered evaluation and the app-root wire registration.
- Entry: `Register(IServiceCollection services, params ReadOnlySpan<WireHealthRow> rows)` is the app-root pin folding every row onto the standard wire health service; `Evaluate(HealthCheckService, WireHealthRow, IClock, CorrelationId, CancellationToken)` is the out-of-band read of the SAME row for a probe that speaks no gRPC.
- Auto: `Register` composes `AddGrpcHealthChecks(Action<GrpcHealthChecksOptions>)` — each row lands as one `GrpcHealthChecksOptions.Services` mapping through `ServiceMappingCollection.Map(string name, Func<HealthCheckMapContext, bool> predicate)`, the predicate reading the row's tag key against `HealthCheckMapContext.Tags` — and the endpoint serves through `MapGrpcHealthChecksService()` at the wire host, so the `grpc.health.v1` protocol answers per-service off the one registry; healthy and degraded project to the serving wire state, unhealthy to not-serving — degraded keeps serving because the level, not the wire, carries usable failure.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, Grpc.AspNetCore.HealthChecks, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one wire row per served service name — one `Map` mapping and zero new surface; the empty-name `Overall` row is the default service every client without a service name reads.
- Boundary: the wire predicate is the ONE place a `ContributorTag` renders back to text, because the framework hands its map context and its registration filter raw strings; set-degradation is the service-modality inbound route — the verb admits the wire enum through `DegradationLevel.OfWire`, the roster-derived inverse of the row's own `Wire` column, mapping `Unspecified` and any unlisted ordinal to `None` so `Force` re-derives rather than forcing a phantom level, and lands on `Force`; one override path serves operator config, wire verbs, and release; `MapGrpcHealthChecksService` is the one wire-health endpoint — a hand-rolled `Grpc.Health.V1.Health.HealthBase` override beside it is the deleted form; `Evaluate` drives the registry live and returns a `HealthSnapshot`, so an HTTP liveness route, an operator verb, and a CLI check read the row set rather than the `DegradationCell`'s published reading — the cell stays the CADENCED truth every interior consumes and `Evaluate` is the on-demand probe path, so a caller wanting the settled level reads `DegradationCell.Read()` and never re-evaluates to derive one.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record WireHealthRow(string Service, Option<ContributorTag> Tag) {
    public static readonly WireHealthRow Overall = new(string.Empty, None);
    public bool Admits(IEnumerable<string> tags) => Tag.Map(row => tags.Contains(row.Key)).IfNone(true);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class WireHealth {
    public static IServiceCollection Register(IServiceCollection services, params ReadOnlySpan<WireHealthRow> rows) {
        Seq<WireHealthRow> set = Iterable<WireHealthRow>.FromSpan(rows).ToSeq();
        return services.AddGrpcHealthChecks(options =>
            ignore(set.Iter(row => options.Services.Map(row.Service, context => row.Admits(context.Tags)))));
    }

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

- Owner: `HealthSignal` `[Union]` the closed selector naming which reading a rule watches; `AlertCondition` `[Union]` the declarative condition family (threshold, anomaly band, forecast band) carrying its wire key and window depth as case columns; `AlertTransition` `[SmartEnum<string>]` the three transition keys; `AlertRule` the versioned rule record carrying hysteresis and debounce; `AlertPolicy` the rule roster derived from the grading table; `AlertState` the per-rule firing-state value and `AlertCell` the keyed holder that carries it between readings; `AlertEngine` the evaluate-and-escalate surface over the continuous `DegradationReading` stream, with `AlertEngine.Runtime` carrying the dispatcher the delivery arm fires through.
- Cases: `HealthSignal` = Overall | Tagged | Named | Level — the aggregate status rank, the worst rank among entries carrying one tag, one named contributor's rank, and the derived degradation rank; `AlertCondition` = Threshold | AnomalyBand | ForecastBand — Threshold fires on a value crossing a bound, AnomalyBand on a value outside a rolling mean ± k·sigma band, ForecastBand on a value outside a linear-trend forecast band; severity rows are the kernel `page`/`ticket` pair alone.
- Entry: `Sweep(AlertEngine.Runtime runtime, AlertCell cell, DegradationReading reading, Instant at)` returns `IO<Seq<Alert>>` — the composition's one stream entry, folding the whole roster against one reading and committing each rule's advanced state; `Observe(Runtime, AlertRule, AlertState, DegradationReading, Instant)` is the per-rule leg it folds — it resolves `rule.Signal` through `Project`, delegates to the pure value-fold `Evaluate`, and fires each transition on `IO`; `Evaluate(AlertRule, AlertState, double, Instant)` returns `(AlertState State, Option<Alert> Fired)`; `Backtest(AlertRule, Seq<(Instant, double)>)` returns `Seq<Alert>` by replaying that pure fold without a delivery runtime. One fold, two front doors: the live stream resolves and delivers, while the back-test feeds historical values and only returns evidence.
- Auto: the threshold condition fires only after the value holds past the rule's dwell so a momentary spike does not fire, and recovers only after the value clears the hysteresis band so a value oscillating at the bound does not flap; the dwell is the rule's own debounce raised to its severity's `Hold` column, so a ticketing rule inherits the deploy plane's flap suppression without restating it; the anomaly band summarizes the held window through the kernel `Stat<Scalar>` Welford fold and tests the incoming value against it, so the sample never contributes to the baseline that judges it and an empty or all-sentinel window refuses rather than answering a fabricated zero mean; the forecast band fits the window through the kernel packed covariance and evaluates one step past its end, so a slow drift toward a limit fires before the limit is crossed; a firing alert escalates through the severity rank walk if it stays fired past the escalation dwell and the top row escalates to itself, so the ladder grows by a severity row and never by an arm here; a recovered alert reports the severity it held and resets the escalation; the rule version stamps every alert so a rule edit is auditable and a back-test pins the rule version it ran against.
- Output: `Alert` — rule id, rule version, severity, condition key, the firing value, the transition row, and `Instant`; the delivery arm fires it at `AppHostPoint.Alert`, and `Alert.Event(source, stamp)` is its projection onto `Topic.Health` as `rasm.apphost.alert.<transition>` — the composition's observe tap publishes it there, so the outbound fan reaches every alert without a second transport and the live span's `traceparent` joins it to the snapshot that fired it. An alert names a `HealthSignal` and no instrument — a counter here mints the second grader the Boundary below and the folder ruling both forbid.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one condition shape is one `AlertCondition` case breaking every evaluate arm; one watched reading is one `HealthSignal` case breaking `Project`; one grading rule mints its own alert row through the same derivation, so the roster grows where the grader does and never beside it; a rule edit is one new `AlertRule` version, never a mutated rule; a routing posture is a kernel `AlertSeverity` row, never a case here; zero new surface.
- Boundary: the alert engine is the only declarative-alerting owner — an ad hoc threshold check, a per-metric alarm, and a parallel alert store are the deleted forms; the sweep seats at `Runtime/modules#MODULE_LEDGER` on the health publish cadence beside `DegradationCell`, reading the cell's own committed value, so the engine and the governor grade one observation of one moment and a `HealthSnapshot`-only entry is the stale-read shape that pair exists to foreclose; `Project` resolves each `HealthSignal` case onto its OWN rank axis — the status cases onto the healthy-through-unhealthy ladder, `Level` onto the five-rung degradation rank — so a bound is authored against the case the rule names and a bound ported across cases is the drift the two axes make visible, while an unmatched selector yields `Option<double>.None` so `Observe` holds the prior state and delivers nothing; the severity vocabulary is the kernel's two-row routing axis — this page carries no ladder, and the rank-ordered incident escalation rides the row's `Rank` and `Escalated` columns; error-budget burn is the SEPARATE concern the kernel `Slo.Specs` compiles onto the deploy plane from an `Objective`, so a burn arm here mints the second metric source this boundary forbids; the alert engine and degradation ladder remain distinct — degradation is the host's capability state, while alerting is user-facing notification over continuous queries; hysteresis is the condition's recovery band and dwell is the rule's minimum breach hold, so sampling frequency cannot change a rule's firing threshold; only live `Observe` fires the dispatcher, so `Backtest` cannot notify operators while replaying history — and `Backtest` seeds `AlertState.Clear` per replay rather than reading the cell, so a historical run can neither observe nor disturb live firing state; rule versioning makes a rule edit a new immutable version so each alert identifies the rule that fired it; the cell is the roster's own custody and holds nothing for a rule the roster no longer carries, so a retired rule's dwell cannot resurrect it; the delivery column is the composition's dispatcher rather than a `Func<>` provider, because delivery is the composed observe tap over `AppHostPoint.Alert` and not a per-call effect, so no composition root can hand this engine a notifier the bus never sees.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HealthSignal {
    private HealthSignal() { }
    public sealed record Overall : HealthSignal;
    public sealed record Tagged(ContributorTag Tag) : HealthSignal;
    public sealed record Named(string Contributor) : HealthSignal;
    public sealed record Level : HealthSignal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AlertCondition {
    private AlertCondition() { }

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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AlertRule(
    string RuleId,
    int Version,
    HealthSignal Signal,
    AlertCondition Condition,
    AlertSeverity Severity,
    Duration Debounce,
    Duration EscalationDwell) {
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

public readonly record struct Alert(
    string RuleId,
    int Version,
    AlertSeverity Severity,
    string Condition,
    double Value,
    AlertTransition Transition,
    Instant At) {
    public Fin<DomainEvent> Event(EventSource source, HlcStamp stamp) =>
        DomainEvent.Of(
            Topic.Health, EventType.Of(TelemetryDomain.AppHost.Key, "alert", Transition.Key), source,
            $"{RuleId}:{Version}:{Transition.Key}:{ClockPolicy.Persisted(At)}",
            JsonSerializer.SerializeToElement(this, SuiteContracts.Host), DataClassification.Operational, stamp);
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class AlertPolicy {
    static readonly Duration Escalation = Duration.FromMinutes(15);

    public static Seq<AlertRule> Canonical =>
        DegradationPolicy.Canonical.Rules
            .Map(static rule => Row(
                $"apphost.health.{rule.Tag.Key}.{rule.Outcome.Key}",
                new HealthSignal.Tagged(rule.Tag),
                AlertEngine.Rank(rule.Trigger) - 0.5d,
                rule.Outcome.Retains.Admits(Faculty.StoreWrite) ? AlertSeverity.Ticket : AlertSeverity.Page))
            .Add(Row(
                "apphost.health.level",
                new HealthSignal.Level(),
                DegradationLevel.ReducedRemote.Rank - 0.5d,
                AlertSeverity.Page))
            .Strict();

    static AlertRule Row(string id, HealthSignal signal, double bound, AlertSeverity severity) =>
        new(RuleId: id,
            Version: 1,
            Signal: signal,
            Condition: new AlertCondition.Threshold(Bound: bound, Above: true, Hysteresis: 0.5d),
            Severity: severity,
            Debounce: DegradationPolicy.Canonical.MinimumDwell,
            EscalationDwell: Escalation);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class AlertCell(Seq<AlertRule> rules) {
    readonly AtomHashMap<string, AlertState> states = AtomHashMap<string, AlertState>();

    public Seq<AlertRule> Rules { get; } = rules;

    public AlertState Held(string ruleId) => states.Find(ruleId).IfNone(AlertState.Clear);

    public Unit Commit(string ruleId, AlertState state) => states.SwapKey(ruleId, _ => Some(state));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AlertEngine {
    public sealed record Runtime(HookSet<AppHostPoint, AppHostFact, TelemetrySource> Hooks);

    public static IO<Seq<Alert>> Sweep(Runtime runtime, AlertCell cell, DegradationReading reading, Instant at) =>
        cell.Rules
            .TraverseM(rule => Observe(runtime, rule, cell.Held(rule.RuleId), reading, at)
                .Map(step => (cell.Commit(rule.RuleId, step.State), step.Fired).Item2))
            .As()
            .Map(static fired => fired.Somes());

    public static double Rank(HealthStatus status) => status switch {
        HealthStatus.Healthy => 1d,
        HealthStatus.Degraded => 2d,
        _ => 3d,
    };

    public static Option<double> Project(AlertRule rule, DegradationReading reading) => rule.Signal.Switch(
        state: reading,
        overall: static (read, _) => Some(Rank(read.Snapshot.Status)),
        tagged: static (read, row) => read.Snapshot.Entries
            .Filter(entry => entry.Tags.Admits(row.Tag))
            .Fold(Option<double>.None, static (worst, entry) => Some(double.Max(worst.IfNone(0d), Rank(entry.Status)))),
        named: static (read, row) => read.Snapshot.Entries
            .Find(entry => entry.Name == row.Contributor)
            .Map(static entry => Rank(entry.Status)),
        level: static (read, _) => Some((double)read.Level.Rank));

    public static IO<(AlertState State, Option<Alert> Fired)> Observe(
        Runtime runtime, AlertRule rule, AlertState state, DegradationReading reading, Instant at) =>
        Project(rule, reading).Match(
            Some: value => Deliver(runtime, Evaluate(rule, state, value, at, runtime.Key)),
            None: () => IO.pure((state, Option<Alert>.None)));

    public static (AlertState State, Option<Alert> Fired) Evaluate(
        AlertRule rule, AlertState state, double value, Instant at) {
        var window = (state.Window.Add(value) is var w && w.Count > rule.Condition.Depth ? w.Tail : w).Strict();
        var breached = Breached(rule.Condition, value, state.Window, state.Firing);
        Option<Instant> breachedSince = breached && !state.Firing
            ? state.BreachedSince.Match(Some: static since => Some(since), None: () => Some(at))
            : None;
        return (breached, state.Firing) switch {
            (true, false) when breachedSince.Map(since => at - since >= rule.Dwell).IfNone(false) =>
                (state with { Firing = true, Current = rule.Severity, BreachedSince = None, FiredSince = Some(at), Window = window },
                 Some(Mint(rule, rule.Severity, value, AlertTransition.Fired, at))),
            (true, true) when state.FiredSince.Map(since => at - since >= rule.EscalationDwell).IfNone(false) && state.Current.Rank < state.Current.Escalated.Rank =>
                (state with { Current = state.Current.Escalated, FiredSince = Some(at), Window = window },
                 Some(Mint(rule, state.Current.Escalated, value, AlertTransition.Escalated, at))),
            (false, true) =>
                (AlertState.Clear with { Window = window },
                 Some(Mint(rule, state.Current, value, AlertTransition.Recovered, at))),
            _ => (state with { BreachedSince = breachedSince, Window = window }, None),
        };
    }

    public static Seq<Alert> Backtest(AlertRule rule, Seq<(Instant At, double Value)> history) =>
        history.Fold((State: AlertState.Clear, Fired: Seq<Alert>()), (acc, sample) =>
            Threaded(acc, Evaluate(rule, acc.State, sample.Value, sample.At))).Fired;

    static (AlertState State, Seq<Alert> Fired) Threaded(
        (AlertState State, Seq<Alert> Fired) held, (AlertState State, Option<Alert> Fired) step) =>
        (step.State, step.Fired.Match(Some: held.Fired.Add, None: () => held.Fired));

    static Alert Mint(AlertRule rule, AlertSeverity severity, double value, AlertTransition transition, Instant at) =>
        new(rule.RuleId, rule.Version, severity, rule.Condition.Key, value, transition, at);

    static IO<(AlertState State, Option<Alert> Fired)> Deliver(
        Runtime runtime,
        (AlertState State, Option<Alert> Fired) step) =>
        step.Fired.Match(
            Some: alert => IO.lift(() => runtime.Hooks.Fire(
                    at: AppHostPoint.Alert, fact: new AppHostFact.Alert(alert)))
                .Map(_ => step),
            None: () => IO.pure(step));

    static bool Breached(AlertCondition condition, double value, Seq<double> baseline, bool firing) => condition.Switch(
        state: (Value: value, Baseline: baseline, Firing: firing),
        threshold: static (read, t) => read.Firing
            ? (t.Above ? read.Value >= t.Bound - t.Hysteresis : read.Value <= t.Bound + t.Hysteresis)
            : (t.Above ? read.Value > t.Bound : read.Value < t.Bound),
        anomalyBand: static (read, a) => read.Baseline.Count >= a.Depth
            && Stat<Scalar>.Of(read.Baseline.Map(static v => (Scalar)v), read.Key)
                .Map(stat => double.Abs(read.Value - stat.Mean) > a.Sigma * stat.Deviation(MomentNormalizer.Population))
                .IfFail(false),
        forecastBand: static (read, f) => read.Baseline.Count >= f.Depth
            && Forecast(read.Baseline, read.Key)
                .Map(fit => double.Abs(read.Value - fit) > f.EnvelopeWidth)
                .IfNone(false));

    static Option<double> Forecast(Seq<double> baseline) =>
        SampleMoment.Of(baseline.Map(static (value, index) => Seq((double)index, value)))
            .ToOption()
            .Map(fit => fit.Mean[1] + Slope(fit) * (baseline.Count - fit.Mean[0]));

    static double Slope(SampleMoment fit) => fit[0, 0] > 0d ? fit[0, 1] / fit[0, 0] : 0d;
}
```

## [06]-[TS_PROJECTION]

- Owner: `HealthSnapshotWire`, `DegradationWire`, and `AlertWire` transcribe host-local dashboard records; generated `CommandAvailability` and `CommandVerdictWire` carry the health availability contract, including level, deviations from the level posture, and dwell anchor.
- Growth: one faculty key row or alert field extends its local record; availability fields, verdict cases, and degradation enum values extend the protobuf schema once.
- Boundary: host-local snapshot and alert JSON keep extended-ISO instants, ISO-8601 durations, and smart-enum keys. Generated availability uses protobuf `Timestamp`, the compute degradation enum, and the command-verdict oneof; a command absent from the map takes the level's own posture at the decoder, so only deviations cross. `DegradationWire` remains the host-local state emission, with `cascade` distinct from operator `forced`; rank and retained faculties derive locally and never cross.

```ts
export {
  CommandAvailabilitySchema,
  CommandVerdictWireSchema,
} from "@rasm\/contracts/rasm/contracts/availability/availability_pb";
export type { CommandAvailability, CommandVerdictWire } from "@rasm\/contracts/rasm/contracts/availability/availability_pb";
export {
  DegradationLevel,
  DegradationLevelSchema,
} from "@rasm\/contracts/rasm/contracts/compute/control_pb";

type HealthStatusWire = "healthy" | "degraded" | "unhealthy";

type FacultyKey =
  | "host-document" | "remote-compute" | "local-compute"
  | "store-write" | "store-read" | "telemetry-export";

type ContributorTagKey = "host" | "remote" | "store" | "pressure";

type DegradationLevelKey =
  | "full" | "reduced-remote" | "local-only" | "read-only" | "suspended";

interface HealthEntryWire {
  readonly name: string;
  readonly status: HealthStatusWire;
  readonly elapsed: string;
  readonly tags: readonly ContributorTagKey[];
  readonly detail?: string;
}

interface HealthSnapshotWire {
  readonly status: HealthStatusWire;
  readonly at: string;
  readonly correlation: string;
  readonly entries: readonly HealthEntryWire[];
}

interface DegradationWire {
  readonly derived: DegradationLevelKey;
  readonly forced?: DegradationLevelKey;
  readonly cascade?: DegradationLevelKey;
  readonly streak: number;
  readonly since?: string;
  readonly floor: DegradationLevelKey;
  readonly level: DegradationLevelKey;
}

type AlertSeverityKey = "page" | "ticket";

type AlertConditionKey = "threshold" | "anomaly-band" | "forecast-band";

type AlertTransitionKey = "fired" | "recovered" | "escalated";

interface AlertWire {
  readonly ruleId: string;
  readonly version: number;
  readonly severity: AlertSeverityKey;
  readonly condition: AlertConditionKey;
  readonly value: number;
  readonly transition: AlertTransitionKey;
  readonly at: string;
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
