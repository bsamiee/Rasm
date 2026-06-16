# [APPHOST_HEALTH_AND_DEGRADATION]

Capability health and the usable-failure degradation rail for every Rasm.AppHost process: a health-contributor row family folds package probes into one wire-neutral snapshot, the five-level DegradationLevel vocabulary carries one retained-capability set per row, and a wire-health mapping projects the registry onto the standard wire health service. Microsoft.Extensions.Diagnostics.HealthChecks supplies probe mechanics, ResourceMonitoring publishes CPU/memory utilization and container limits through its OTel observable instruments and ResourceQuotaProvider, Thinktecture owns the vocabularies, LanguageExt and NodaTime carry the fold rails and stamps; every consumer reads one level value.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                               |
| :-----: | ---------------- | -------------------------------------------------------------------- |
|   [1]   | HEALTH_FOLD      | Contributor rows, resource pressure, peer reads, one snapshot fold   |
|   [2]   | DEGRADATION_RAIL | Level vocabulary, retained capabilities, derivation fold, hysteresis |
|   [3]   | WIRE_HEALTH      | Tag-predicate wire mapping and the inbound set-degradation route     |
|   [4]   | TS_PROJECTION    | Health snapshot and degradation level wire shapes                    |

## [2]-[HEALTH_FOLD]

- Owner: `HealthContributorRow` is the probe row and the `IHealthCheck`; `PressurePolicy` grades utilization with a `ResourceQuota` container-limit column; `UtilizationCell` is the `MeterListener`-backed boundary capsule that records the ResourceMonitoring observable instruments and grades on read; `HealthSnapshot` with nested `Entry` is the only health shape interiors read.
- Cases: tag consts `Host`, `Remote`, `Store`, `Pressure` key the derivation rules and the wire predicates; instrument-name consts `CpuInstrument` and `MemoryInstrument` key the meter subscription; `Gauge` and `Peer` are the canonical row factories and `Monitor` is the resource-monitoring registration fold.
- Entry: `Register(params ReadOnlySpan<HealthContributorRow> rows)` composes registrations; `Snapshot(Instant at, CorrelationId correlation)` is the pure report fold.
- Auto: rows project into `HealthCheckRegistration` — `FailureStatus`, `Tags`, `Timeout`, `Delay`, `Period` are registration policy, never probe-local exception handling; `Monitor` folds the policy onto `AddResourceMonitoring`, binding `CollectionWindow` to `PressurePolicy.Canonical.Window`, `SamplingInterval` to `PressurePolicy.Canonical.Sampling`, and `PublishingWindow`, `CpuConsumptionRefreshInterval`, and `MemoryConsumptionRefreshInterval`, setting `UseLinuxCalculationV2` from `CgroupV2` and `UseZeroToOneRangeForLinuxMetrics` so CPU rides the normalized zero-to-one range the grade reads, and turning on `EnableSystemDiskIoMetrics` from `DiskIoMetrics` for the disk-I/O instruments; `UtilizationCell.Attach` opens a `MeterListener` on the meter `Microsoft.Extensions.Diagnostics.ResourceMonitoring`, enables `process.cpu.utilization` and `dotnet.process.memory.virtual.ratio`, and records each observed double into the atom, the publishing window bounding every derived signal's reaction time.
- Receipt: `HealthSnapshot` stamped with `Instant` and `CorrelationId`; `HealthReport` never crosses the fold.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.ResourceMonitoring, NodaTime, LanguageExt.Core
- Growth: one contributor row per new capability probe — sibling packages extend the same `Register` span through the health port registration set, zero new surface; container-limit grading is one `ResourceQuota` value flip on `PressurePolicy.Quota`, a sampling retune is one `Sampling` value, and a new utilization signal is one enabled instrument name on `UtilizationCell.Attach`, never a parallel policy.
- Boundary: package health types stop at this seam — interiors read `HealthSnapshot` and one level value; `Peer` rows read a peer process over its wire health service, so cross-process health is a read, never shared state; `Gauge` grades against the container limit when `PressurePolicy.Quota` carries a `ResourceQuota` — the OCI `headless` and `web` rows set `CgroupV2` so `Monitor` turns on `UseLinuxCalculationV2` and `UseZeroToOneRangeForLinuxMetrics`, folding the cgroup `MaxCpuInCores` and `MaxMemoryInBytes` ceilings into the same zero-to-one ratio axis the meter publishes, so a cgroup-throttled process degrades on the limit it actually runs under, never the host total; `ResourceQuota` carries the `MaxMemoryInBytes`/`MaxCpuInCores` and `BaselineMemoryInBytes`/`BaselineCpuInCores` ceilings, and whether the `process.cpu.utilization`/`dotnet.process.memory.virtual.ratio` instruments arrive already quota-normalized under that flag or require a `ResourceQuotaProvider.GetResourceQuota()` recompute rides `[MEMORY_RATIO_SEMANTIC]`; a bare host-ratio grade on a container row is the deleted form. The obsolete `IResourceMonitor`/`ResourceUtilization`/`SystemResources`/`IResourceUtilizationPublisher`/`ISnapshotProvider` pull path is the deleted form: utilization crosses as observable-instrument readings, container limits as `ResourceQuota`, never the windowed-snapshot interface.

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

## [3]-[DEGRADATION_RAIL]

- Owner: `Capability` and `DegradationLevel` vocabularies under one `HealthKeyPolicy` comparer accessor; `DegradationPolicy` with nested `Rule` rows is the derivation table; `DegradationState` is the fold receipt; `DegradationCell` is the boundary capsule owning the atom cell and the publisher seam.
- Cases: `Full(0)`, `ReducedRemote(1)`, `LocalOnly(2)`, `ReadOnly(3)`, `Suspended(4)` in severity order; six `Capability` keys form the retained sets.
- Entry: `Derive(DegradationState state, HealthSnapshot snapshot)` folds rules with escalation-immediate, recovery-hysteresis semantics; `Force(Option<DegradationLevel> forced)` is the single override entrypoint; `Cascade(Option<DegradationLevel> parent)` admits a parent-forced level as a derivation floor.
- Auto: `DegradationCell` registers as the `IHealthCheckPublisher`; `HealthCheckPublisherOptions` binds `Delay` and `Period` from `DegradationPolicy.Canonical` and `Timeout` from `DeadlineClass.HealthProbe`; `OperatorOverride` projects onto `Force` at the composition root — forced beats derived, release re-derives.
- Receipt: `DegradationState` carries derived level, forced input, cascade floor, recovery streak, and dwell anchor; a `Level` change rides the lifecycle transition receipt as the degraded trigger.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `Rule` row or one `Capability` case absorbs a new degradation driver; a new level is one `DegradationLevel` item — zero new surface.
- Boundary: degradation is process-local, peer-health-informed, and parent-cascade-floored — a peer level never propagates as this process's level, but a parent process's forced level enters `Derive` as a floor through `Cascade`, never as shared state; `LocalOnly` is the host-absent fold: `Capability.HostDocument` gates off and document sources yield absence; the container-limit pressure signal enters the rank algebra as data, not a new rule — a `PressurePolicy.Container` row grades against `ResourceQuota` so the `Pressure`-tagged `Gauge` row escalates on the cgroup limit, and the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules carry that limit-relative status into `Derive` with the same retained-set hysteresis, so a container-throttled process degrades and recovers on its own limit with zero added `Rule` row. The cross-process cascade is a seam-split: the READ — this process's own derived `Level` value — stays the owner here; the WRITE — a parent fanning its level to a child over the control hop — lands at `companion-sidecar#DEGRADATION_CASCADE`, which calls `Cascade` with the observed parent level; release passes `None` and the cell re-derives off its own snapshots, the cascade floor never escalating below local pressure.

```csharp signature
public sealed class HealthKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;
    public static IEqualityComparer<string> EqualityComparer => Policy;
    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<HealthKeyPolicy, string>]
[KeyMemberComparer<HealthKeyPolicy, string>]
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
[KeyMemberEqualityComparer<HealthKeyPolicy, string>]
[KeyMemberComparer<HealthKeyPolicy, string>]
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

public sealed class DegradationCell(DegradationPolicy policy, IClock clock, CorrelationId correlation) : IHealthCheckPublisher {
    private readonly Atom<DegradationState> cell = Atom(DegradationState.Boot);
    public DegradationState State => cell.Value;
    public DegradationLevel Level => cell.Value.Level;

    public DegradationState Force(Option<DegradationLevel> forced) =>
        cell.Swap(state => state with { Forced = forced });

    public DegradationState Cascade(Option<DegradationLevel> parent) =>
        cell.Swap(state => state with { Cascade = parent });

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken) =>
        Task.FromResult(cell.Swap(state => policy.Derive(state, report.Snapshot(clock.GetCurrentInstant(), correlation))));
}
```

## [4]-[WIRE_HEALTH]

- Owner: `WireHealthRow` binds one wire service name to one tag predicate; `WireHealth` attaches the filtered evaluation.
- Entry: `Evaluate(HealthCheckService service, CancellationToken token)` runs the tag-filtered registry sweep behind one row.
- Auto: app roots register the `grpc.health.v1` service behind the app-root pin and feed it the row predicate; healthy and degraded project to the serving wire state, unhealthy to not-serving — degraded keeps serving because the level, not the wire, carries usable failure.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, LanguageExt.Core
- Growth: one wire row per served service name, zero new surface.
- Boundary: set-degradation is the service-modality inbound route — the verb admits its level key through `DegradationLevel.Validate` and lands on `Force`; one override rail serves operator config, wire verbs, and release.

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

## [5]-[TS_PROJECTION]

- Owner: `HealthSnapshotWire` and `DegradationWire` transcribe the snapshot and level records the dashboard ingests.
- Packages: BCL inbox
- Growth: one capability key row or one field on an owning wire record, zero new surface.
- Boundary: instants cross as extended-ISO text and elapsed spans as ISO-8601 duration text; level, cascade, and capability keys are the smart-enum string keys, status crosses as the camel-case enum name, never ordinals; `cascade` is the parent-floored level a child reports, distinct from `forced` operator override.

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
```

## [6]-[RESEARCH]

- [WIRE_REGISTRATION]: the `grpc.health.v1` wire-service registration surface behind the app-root pin, with the default status-to-serving projection; the tag-predicate evaluation rides the confirmed `HealthCheckService.CheckHealthAsync(Func<HealthCheckRegistration, bool>?, CancellationToken)` overload.
- [MEMORY_RATIO_SEMANTIC]: `dotnet.process.memory.virtual.ratio` is read as the memory-pressure ratio the `Gauge` grade compares against `MemoryDegraded`/`MemoryUnhealthy`; whether this instrument and `process.cpu.utilization` report over the cgroup `MaxMemoryInBytes`/`MaxCpuInCores` ceilings or over the host total under `UseLinuxCalculationV2`+`UseZeroToOneRangeForLinuxMetrics` settles whether a `Container`-row grade rides the meter as published or threads a `ResourceQuotaProvider.GetResourceQuota()`-sourced `ResourceQuota` into a quota-relative recompute and the provider-registration surface that supplies `PressurePolicy.Quota` to the runtime — resolved against the ResourceMonitoring metric exporter and `ResourceQuotaProvider` registration before the container-row grade finalizes.
