# [APPHOST_HEALTH_AND_DEGRADATION]

Capability health and the usable-failure degradation rail for every Rasm.AppHost process: a health-contributor row
family folds package probes into one wire-neutral snapshot, the five-level DegradationLevel vocabulary carries one
retained-capability set per row, and a wire-health mapping projects the registry onto the standard wire health
service. Microsoft.Extensions.Diagnostics.HealthChecks and ResourceMonitoring supply probe mechanics, Thinktecture
owns the vocabularies, LanguageExt and NodaTime carry the fold rails and stamps; every consumer reads one level value.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                               |
| :-----: | ---------------- | -------------------------------------------------------------------- |
|   [1]   | HEALTH_FOLD      | Contributor rows, resource pressure, peer reads, one snapshot fold   |
|   [2]   | DEGRADATION_RAIL | Level vocabulary, retained capabilities, derivation fold, hysteresis |
|   [3]   | WIRE_HEALTH      | Tag-predicate wire mapping and the inbound set-degradation route     |
|   [4]   | TS_PROJECTION    | Health snapshot and degradation level wire shapes                    |

## [2]-[HEALTH_FOLD]

- Owner: `HealthContributorRow` is the probe row and the `IHealthCheck`; `PressurePolicy` grades utilization with container-limit columns; `HealthSnapshot` with nested `Entry` is the only health shape interiors read.
- Cases: tag consts `Host`, `Remote`, `Store`, `Pressure` key the derivation rules and the wire predicates; `Gauge` and `Peer` are the canonical row factories.
- Entry: `Register(params ReadOnlySpan<HealthContributorRow> rows)` composes registrations; `Snapshot(Instant at, CorrelationId correlation)` is the pure report fold.
- Auto: rows project into `HealthCheckRegistration` — `FailureStatus`, `Tags`, `Timeout`, `Delay`, `Period` are registration policy, never probe-local exception handling; `AddResourceMonitoring` plus `ConfigureMonitor` align `CollectionWindow` with `PressurePolicy.Canonical.Window`, bind `PublishingWindow`, and set `UseLinuxCalculationV2` and `UseZeroToOneRangeForLinuxMetrics` on cgroup-v2 hosts with `EnableSystemDiskIoMetrics` for the disk-I/O instruments.
- Receipt: `HealthSnapshot` stamped with `Instant` and `CorrelationId`; `HealthReport` never crosses the fold.
- Packages: Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.ResourceMonitoring, NodaTime, LanguageExt.Core
- Growth: one contributor row per new capability probe — sibling packages extend the same `Register` span through the health port registration set, zero new surface; container-limit grading is one `Quota` column flip on `PressurePolicy`, never a parallel policy.
- Boundary: package health types stop at this seam — interiors read `HealthSnapshot` and one level value; `Peer` rows read a peer process over its wire health service, so cross-process health is a read, never shared state; `Gauge` grades against the container limit when the `ResourceUtilization` snapshot carries `SystemResources` — the OCI `headless` and `web` rows fold guaranteed-versus-maximum CPU units and memory bytes into the same percentage axis, so a cgroup-throttled process degrades on the limit it actually runs under, never the host total; `ResourceQuota` carries the baseline-and-maximum quota the limit grading reads, and a bare host-percentage grade on a container row is the deleted form.

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

    public static HealthContributorRow Gauge(IResourceMonitor monitor, PressurePolicy policy) => new(
        Name: nameof(Gauge),
        Probe: _ => ValueTask.FromResult(policy.Grade(monitor.GetUtilization(policy.Window.ToTimeSpan()))),
        FailureStatus: HealthStatus.Degraded,
        Tags: TagSet(Pressure),
        Timeout: DeadlineClass.HealthProbe,
        Delay: policy.Window,
        Period: policy.Window);

    public static IResourceMonitorBuilder Monitor(IResourceMonitorBuilder builder, PressurePolicy policy) =>
        builder.ConfigureMonitor(options => {
            options.CollectionWindow = policy.Window.ToTimeSpan();
            options.PublishingWindow = policy.Window.ToTimeSpan();
            options.EnableSystemDiskIoMetrics = policy.DiskIoMetrics;
            options.UseLinuxCalculationV2 = policy.CgroupV2;
            options.UseZeroToOneRangeForLinuxMetrics = false;
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

public sealed record PressurePolicy(
    Duration Window,
    double CpuDegraded,
    double CpuUnhealthy,
    double MemoryDegraded,
    double MemoryUnhealthy,
    bool Quota,
    bool CgroupV2,
    bool DiskIoMetrics) {
    public static readonly PressurePolicy Canonical = new(
        Window: Duration.FromSeconds(5), CpuDegraded: 80d, CpuUnhealthy: 92d, MemoryDegraded: 85d, MemoryUnhealthy: 95d,
        Quota: false, CgroupV2: false, DiskIoMetrics: false);

    public static readonly PressurePolicy Container = Canonical with { Quota = true, CgroupV2 = true, DiskIoMetrics = true };

    public HealthCheckResult Grade(ResourceUtilization usage) =>
        (Cpu: usage.CpuUsedPercentage, Memory: usage.MemoryUsedPercentage, Limit: Quota ? Optional(usage.SystemResources) : None) switch {
            var load when load.Cpu >= CpuUnhealthy || load.Memory >= MemoryUnhealthy =>
                HealthCheckResult.Unhealthy($"cpu {load.Cpu:F0} memory {load.Memory:F0}"),
            var load when load.Cpu >= CpuDegraded || load.Memory >= MemoryDegraded =>
                HealthCheckResult.Degraded($"cpu {load.Cpu:F0} memory {load.Memory:F0}"),
            _ => HealthCheckResult.Healthy(),
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
- Entry: `Derive(DegradationState state, HealthSnapshot snapshot)` folds rules with escalation-immediate, recovery-hysteresis semantics; `Force(Option<DegradationLevel> forced)` is the single override entrypoint.
- Auto: `DegradationCell` registers as the `IHealthCheckPublisher`; `HealthCheckPublisherOptions` binds `Delay` and `Period` from `DegradationPolicy.Canonical` and `Timeout` from `DeadlineClass.HealthProbe`; `OperatorOverride` projects onto `Force` at the composition root — forced beats derived, release re-derives.
- Receipt: `DegradationState` carries derived level, forced input, recovery streak, and dwell anchor; a `Level` change rides the lifecycle transition receipt as the degraded trigger.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `Rule` row or one `Capability` case absorbs a new degradation driver; a new level is one `DegradationLevel` item — zero new surface.
- Boundary: degradation is process-local and peer-health-informed — a peer level never propagates as this process's level; `LocalOnly` is the host-absent fold: `Capability.HostDocument` gates off and document sources yield absence; the container-limit pressure signal enters the rank algebra as data, not a new rule — a `PressurePolicy.Container` row grades against `SystemResources` so the `Pressure`-tagged `Gauge` row escalates on the cgroup limit, and the existing `Pressure`-Degraded and `Pressure`-Unhealthy rules carry that limit-relative status into `Derive` with the same retained-set hysteresis, so a container-throttled process degrades and recovers on its own limit with zero added `Rule` row.

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
    int Streak,
    Option<Instant> Since) {
    public static readonly DegradationState Boot = new(DegradationLevel.Full, None, Streak: 0, Since: None);
    public DegradationLevel Level => Forced.IfNone(Derived);
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
                new DegradationState(fold.Candidate, state.Forced, Streak: 0, Since: Optional(snapshot.At)),
            var fold when fold.Candidate.Rank == fold.Rank => state with { Streak = 0 },
            var fold when state.Streak + 1 >= ConsecutiveHealthy
                && state.Since.Map(since => snapshot.At - since >= MinimumDwell).IfNone(true) =>
                new DegradationState(fold.Candidate, state.Forced, Streak: 0, Since: Optional(snapshot.At)),
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
- Boundary: instants cross as extended-ISO text and elapsed spans as ISO-8601 duration text; level and capability keys are the smart-enum string keys, status crosses as the camel-case enum name, never ordinals.

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
  readonly since: string | null;
}
```

## [6]-[RESEARCH]

- [WIRE_REGISTRATION]: wire health service registration surface and tag-predicate mapping overload behind the app-root pin, with the default status-to-serving projection.
- [CONTAINER_LIMIT_READ]: the `SystemResources` guaranteed-versus-maximum CPU-unit and memory-byte property spellings and the `ResourceUtilization` used-byte accessor the `Quota`-flagged `Grade` folds against the container limit; the `ResourceQuota` baseline-and-maximum quota accessor; the limit-relative escalation arithmetic resolves from these accessors, never a host-total percentage on a container row.
