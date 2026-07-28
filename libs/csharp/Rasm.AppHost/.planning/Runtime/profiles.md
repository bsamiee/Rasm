# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process from one supplied `ConsumptionProfile` row: a composition root states `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, and `providers`, `Resolve` admits that row against the crossings this branch answers, and every boot fact folds out of the axis values — server GC, ReadyToRun, module scan, single-instance, co-hosted assets, ship vehicle, OTLP export, builder construction, lifetime attach. `Boot` turns the resolved record into a configured Generic Host builder, one identity fold derives per-user roots and telemetry resource attributes from it, and one power-and-fidelity fold reads the live power state and thermal budget to scale compute fidelity on a battery- or thermally-constrained host.

`RecoveryObjective` rides the host descriptor and the topology row as the declared `(Rpo, Rto)` window and projects onto `ResolvedProfile`, so `Rasm.Persistence/Version/recovery` reads the DR target as settled vocabulary and never mints it locally. This page owns the six-axis roster, the host and provider descriptor shapes, the axis-refusal rail, the per-modality DR objective, the boot-attach delegate rows, the resource-identity fold, and the energy-aware fidelity scaling over Microsoft.Extensions.Hosting, Thinktecture-generated vocabulary, LanguageExt rails, NodaTime instants, the OpenTelemetry resource seam, and the macOS IOKit/SMC power-state native reads.

## [01]-[INDEX]

- [02]-[PROFILE_AXIS]: Six-axis consumption roster, descriptor shapes, axis refusal, one resolved record.
- [03]-[LIFETIME_ADAPTERS]: Builder selection, lifetime delegates, `HostOptions` policy, and hook projection.
- [04]-[RESOURCE_IDENTITY]: Per-user roots including the durable queue root, and the resource triple behind one detector.
- [05]-[POWER_AND_FIDELITY]: Power-state and thermal-budget reads; energy-aware compute-fidelity scaling.

## [02]-[PROFILE_AXIS]

- Owner: `ProfileAxis` names the six-axis roster; `Tenancy`, `DeploymentTopology`, `LifecycleOwner`, and `Isolation` close their vocabularies; `HostDescriptor` and `ProviderDescriptor` fix the two open axes' descriptor shape; `ConsumptionProfile` carries the supplied row, `RecoveryObjective` its `(Rpo, Rto)` durability column, and `ResolvedProfile` the only profile artifact siblings consume.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `host` and `providers` carry descriptor rows this branch supplies through `HostRows` and `ProviderRows`; `HostAttach` = Foreign | AppRoot | Quiet | Managed; `HostSurface` = Embedded | Windowed | Offscreen | None; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = Text | AttachmentRejected | RootUnresolved | AxisUnsupported in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Admit` gates the axis values first, so `Fin` aborts on axis refusal, attachment rejection, and root rejection.
- Auto: one supplied row replaces every bootstrap program — a host descriptor overrides its topology row's `Vehicle`, `Attach`, `Surface`, and `Durability` columns while an unhosted profile reads the topology row, so `ServerGc`, `ReadyToRun`, `ModuleScan`, `SingleInstance`, `CoHostedAssets`, `LocalStore`, `HostDocument`, and `OtlpExport` fold from axis values with no key roster between them; raw axis keys admit through each vocabulary's generated `Validate` against `ProfileFault`.
- Receipt: `Canonical()` emits the six axis rows in roster order under an ordinal provider-key sort — the `canonical-json` preimage `CONSUMPTION_PROFILE` corpus parity proves across the three branches.
- Packages: Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one host integration is one `HostRows` descriptor row and one bound port is one `ProviderRows` row, each at zero new surface; a new closed-axis value is one member on its owning vocabulary, and a new axis is one `ProfileAxis` row beside one `ConsumptionProfile` column, both settling at the corpus roster first.
- Boundary: axis values stay data — a compile-time assumption, an ambient global, a build flag, and a package branching on which product hosts it are the four deleted forms, so a host integration lands as a descriptor row and never as a closed case; `Admit` refuses an unservable axis value with `ProfileFault.AxisUnsupported` carrying `AxisEvidence` that names the axis, so silent degradation and a narrowed public surface never happen; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; `isolation` refuses where no bound provider supplies the crossing's capability; `RuntimeAttachment.Integrating` admits only where the resolved row carries `SingleInstance`, so a shared store root reaches exactly one live instance; `RecoveryObjective` is the one DR-target source — `Rasm.Persistence/Version/recovery` `Recovery.Objective(ResolvedProfile)` reads `ResolvedProfile.Recovery` as settled vocabulary through the `Runtime ⇄ Rasm.Persistence/Version/recovery # [PORT]: ResolvedProfile DR-objective inputs` seam and never re-derives the `(Rpo, Rto)` window, so a host-band-keyed RPO/RTO table on the Persistence side is the deleted form and the engine arms gauge their measured RPO/RTO against the column, never a second DR taxonomy; column values stay app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the `SingleInstance` value is probed through the discovery manifest, a `CoHostedAssets` host serves the built TS bundle same-origin from its app root with cross-origin headers held as designed growth, and the test-harness row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof.

Each `isolation` value names the crossing that answers it; an unbound capability refuses on the `isolation` axis rather than degrading to a weaker crossing:

| [INDEX] | [ISOLATION] | [CROSSING_OWNER]                   | [ADMISSION]                    |
| :-----: | :---------- | :--------------------------------- | :----------------------------- |
|  [01]   | `in-proc`   | `Runtime/laneguard#LANE_GUARD`     | always served                  |
|  [02]   | `thread`    | `Runtime/laneguard#LANE_GUARD`     | always served                  |
|  [03]   | `process`   | `Wire/companion#PROCESS_MODALITY`  | `Capability.LocalCompute` row  |
|  [04]   | `wasm`      | `Sandbox/isolation#ISOLATION_AXIS` | `Capability.LocalCompute` row  |
|  [05]   | `remote`    | `Wire/outbound#HOP_AXIS`           | `Capability.RemoteCompute` row |

```csharp signature

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileAxis {
    public static readonly ProfileAxis Tenancy = new("tenancy", closed: true);
    public static readonly ProfileAxis Topology = new("topology", closed: true);
    public static readonly ProfileAxis Host = new("host", closed: false);
    public static readonly ProfileAxis Lifecycle = new("lifecycle", closed: true);
    public static readonly ProfileAxis Isolation = new("isolation", closed: true);
    public static readonly ProfileAxis Providers = new("providers", closed: false);

    // Closed marks the axes whose value set the corpus roster fixes; an open axis fixes the descriptor
    // shape alone, so a row minted here is capability this branch supplies, never a corpus vocabulary.
    public bool Closed { get; }
}

[SmartEnum<string>]
[ValidationError<ProfileFault>]
public sealed partial class Tenancy {
    public static readonly Tenancy None = new("none");
    public static readonly Tenancy Single = new("single");
    public static readonly Tenancy Multi = new("multi");
}

[SmartEnum<string>]
[ValidationError<ProfileFault>]
public sealed partial class LifecycleOwner {
    public static readonly LifecycleOwner CallerOwned = new("caller-owned");
    public static readonly LifecycleOwner PackageOwned = new("package-owned");
}

[SmartEnum<string>]
[ValidationError<ProfileFault>]
public sealed partial class Isolation {
    public static readonly Isolation InProc = new("in-proc", needs: None);
    public static readonly Isolation Thread = new("thread", needs: None);
    public static readonly Isolation Process = new("process", needs: Some(Capability.LocalCompute));
    public static readonly Isolation Wasm = new("wasm", needs: Some(Capability.LocalCompute));
    public static readonly Isolation Remote = new("remote", needs: Some(Capability.RemoteCompute));

    public Option<Capability> Needs { get; }
}

[SmartEnum<string>]
public sealed partial class ShipVehicle {
    public static readonly ShipVehicle Yak = new("yak", readyToRun: false);
    public static readonly ShipVehicle DesktopBundle = new("desktop-bundle", readyToRun: true);
    public static readonly ShipVehicle Oci = new("oci", readyToRun: false);
    public static readonly ShipVehicle Folder = new("folder", readyToRun: false);

    // Ahead-of-time compilation buys start-up latency on a locally launched bundle alone; a long-lived
    // container and a host-loaded plugin assembly both pay the size for a warm-up they never repeat.
    public bool ReadyToRun { get; }
}

[SmartEnum<string>]
public sealed partial class HostAttach {
    public static readonly HostAttach Foreign = new("foreign", createBuilder: ProfileBoot.CreateEmpty, attachLifetime: ProfileBoot.Detached);
    public static readonly HostAttach AppRoot = new("app-root", createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);
    public static readonly HostAttach Quiet = new("quiet", createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Quiet);
    public static readonly HostAttach Managed = new("managed", createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Service);

    [UseDelegateFromConstructor]
    public partial HostApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings);

    [UseDelegateFromConstructor]
    public partial IHostApplicationBuilder AttachLifetime(IHostApplicationBuilder builder);
}

[SmartEnum<string>]
public sealed partial class HostSurface {
    public static readonly HostSurface Embedded = new("embedded");
    public static readonly HostSurface Windowed = new("windowed");
    public static readonly HostSurface Offscreen = new("offscreen");
    public static readonly HostSurface None = new("none");
}

[SmartEnum<string>]
[ValidationError<ProfileFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeploymentTopology {
    public static readonly DeploymentTopology InHost = new("in-host", serverGc: false, vehicle: ShipVehicle.Yak, attach: HostAttach.AppRoot, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard);
    public static readonly DeploymentTopology Sidecar = new("sidecar", serverGc: true, vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.Quiet, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard);
    public static readonly DeploymentTopology Companion = new("companion", serverGc: true, vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.Quiet, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard);
    public static readonly DeploymentTopology Service = new("service", serverGc: true, vehicle: ShipVehicle.Oci, attach: HostAttach.Managed, surface: HostSurface.None, durability: RecoveryObjective.Strict);
    public static readonly DeploymentTopology Edge = new("edge", serverGc: true, vehicle: ShipVehicle.Oci, attach: HostAttach.Managed, surface: HostSurface.None, durability: RecoveryObjective.Strict);
    public static readonly DeploymentTopology Cli = new("cli", serverGc: false, vehicle: ShipVehicle.Folder, attach: HostAttach.AppRoot, surface: HostSurface.None, durability: RecoveryObjective.Relaxed);

    // Four columns state what an UNHOSTED profile inherits; a host descriptor overrides each of them,
    // so in-host values sit here only as the shape a consumer-supplied descriptor is measured against.
    public bool ServerGc { get; }
    public ShipVehicle Vehicle { get; }
    public HostAttach Attach { get; }
    public HostSurface Surface { get; }
    public RecoveryObjective Durability { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuntimeAttachment {
    private RuntimeAttachment() { }
    public sealed record Isolated : RuntimeAttachment;
    public sealed record Integrating(string SharedStoreRoot) : RuntimeAttachment;
}

// Refusal evidence names the AXIS, so a consumer reads which of the six the composition root must
// restate; a detail string alone forces the caller to parse prose back into an axis coordinate.
public sealed record AxisEvidence(ProfileAxis Axis, string Value, string Reason) {
    public string Detail => $"{Axis.Key}={Value}:{Reason}";
}

[Union]
public abstract partial record ProfileFault : Expected, IValidationError<ProfileFault> {
    private ProfileFault(string detail, int code) : base(detail, code, None) { }

    public static ProfileFault Create(string message) => new Text(message);

    public sealed record Text : ProfileFault { public Text(string detail) : base(detail, FaultBand.Profile.Code(0)) { } }
    public sealed record AttachmentRejected : ProfileFault { public AttachmentRejected(string detail) : base(detail, FaultBand.Profile.Code(1)) { } }
    public sealed record RootUnresolved : ProfileFault { public RootUnresolved(string detail) : base(detail, FaultBand.Profile.Code(2)) { } }

    public sealed record AxisUnsupported : ProfileFault {
        public AxisUnsupported(AxisEvidence evidence) : base(evidence.Detail, FaultBand.Profile.Code(3)) => Evidence = evidence;

        public AxisEvidence Evidence { get; }
    }
}

// Host descriptors and topology rows declare this (Rpo, Rto) window and project it onto
// ResolvedProfile; Rasm.Persistence/Version/recovery gauges its measured RPO/RTO against the column.
public readonly record struct RecoveryObjective(Duration Rpo, Duration Rto) {
    public static readonly RecoveryObjective Strict = new(Duration.FromMinutes(1), Duration.FromMinutes(15));
    public static readonly RecoveryObjective Standard = new(Duration.FromMinutes(5), Duration.FromMinutes(30));
    public static readonly RecoveryObjective Relaxed = new(Duration.FromMinutes(15), Duration.FromHours(1));
    public static readonly RecoveryObjective Instant = new(Duration.Zero, Duration.Zero);

    public bool MeetsRpo(Duration measured) => measured <= Rpo;
    public bool MeetsRto(Duration measured) => measured <= Rto;
}

[ComplexValueObject]
[ValidationError<ProfileFault>]
public sealed partial class HostDescriptor {
    public string Key { get; }
    public ShipVehicle Vehicle { get; }
    public HostAttach Attach { get; }
    public HostSurface Surface { get; }
    public RecoveryObjective Durability { get; }
    public bool Document { get; }
    public bool LocalStore { get; }
    public bool ModuleScan { get; }
    public bool SingleInstance { get; }
    public bool CoHostedAssets { get; }
}

[ComplexValueObject]
[ValidationError<ProfileFault>]
public sealed partial class ProviderDescriptor {
    public string Key { get; }
    public Capability Supplies { get; }
    // Reach is the degradation coordinate: a remote-reaching provider drops out of the retained set the
    // moment DegradationLevel stops retaining RemoteCompute, while an in-proc row survives every level.
    public Isolation Reach { get; }
}

// Rows this branch supplies for the OPEN axes. A consumer embedding the estate inside its own product
// mints its own row against the same shape; nothing here is a closed set a package may switch over.
public static class HostRows {
    public static readonly HostDescriptor Rhino = HostDescriptor.Create("rhino", ShipVehicle.Yak, HostAttach.Foreign, HostSurface.Embedded, RecoveryObjective.Relaxed, document: true, localStore: true, moduleScan: true, singleInstance: false, coHostedAssets: false);
    public static readonly HostDescriptor Gh2 = HostDescriptor.Create("gh2", ShipVehicle.Yak, HostAttach.Foreign, HostSurface.Embedded, RecoveryObjective.Relaxed, document: true, localStore: true, moduleScan: true, singleInstance: false, coHostedAssets: false);
    public static readonly HostDescriptor DesktopShell = HostDescriptor.Create("desktop-shell", ShipVehicle.DesktopBundle, HostAttach.AppRoot, HostSurface.Windowed, RecoveryObjective.Standard, document: false, localStore: true, moduleScan: true, singleInstance: true, coHostedAssets: false);
    public static readonly HostDescriptor WebAppRoot = HostDescriptor.Create("web-app-root", ShipVehicle.Oci, HostAttach.AppRoot, HostSurface.None, RecoveryObjective.Strict, document: false, localStore: false, moduleScan: false, singleInstance: false, coHostedAssets: true);
    public static readonly HostDescriptor TestHarness = HostDescriptor.Create("test-harness", ShipVehicle.Folder, HostAttach.AppRoot, HostSurface.Offscreen, RecoveryObjective.Instant, document: false, localStore: false, moduleScan: true, singleInstance: false, coHostedAssets: false);
}

public static class ProviderRows {
    public static readonly ProviderDescriptor OtlpCollector = ProviderDescriptor.Create("otlp-collector", Capability.TelemetryExport, Isolation.Remote);
    public static readonly ProviderDescriptor RemoteSolver = ProviderDescriptor.Create("remote-solver", Capability.RemoteCompute, Isolation.Remote);
    public static readonly ProviderDescriptor LocalSolver = ProviderDescriptor.Create("local-solver", Capability.LocalCompute, Isolation.Process);
    public static readonly ProviderDescriptor DocumentBridge = ProviderDescriptor.Create("document-bridge", Capability.HostDocument, Isolation.InProc);
    public static readonly ProviderDescriptor StoreReader = ProviderDescriptor.Create("store-reader", Capability.StoreRead, Isolation.InProc);
    public static readonly ProviderDescriptor StoreWriter = ProviderDescriptor.Create("store-writer", Capability.StoreWrite, Isolation.InProc);
}

public sealed record ConsumptionProfile(
    Tenancy Tenancy,
    DeploymentTopology Topology,
    Option<HostDescriptor> Host,
    LifecycleOwner Lifecycle,
    Isolation Isolation,
    Seq<ProviderDescriptor> Providers) {
    public FrozenSet<Capability> Grants { get; } = Providers.Map(static row => row.Supplies).ToFrozenSet();

    public ShipVehicle Vehicle => Host.Map(static host => host.Vehicle).IfNone(Topology.Vehicle);
    public HostAttach Attach => Host.Map(static host => host.Attach).IfNone(Topology.Attach);
    public HostSurface Surface => Host.Map(static host => host.Surface).IfNone(Topology.Surface);
    public RecoveryObjective Recovery => Host.Map(static host => host.Durability).IfNone(Topology.Durability);
    public bool ServerGc => Topology.ServerGc;
    public bool ReadyToRun => Vehicle.ReadyToRun;
    public bool ModuleScan => Host.Map(static host => host.ModuleScan).IfNone(true);
    public bool SingleInstance => Host.Map(static host => host.SingleInstance).IfNone(false);
    public bool CoHostedAssets => Host.Map(static host => host.CoHostedAssets).IfNone(false);
    public bool LocalStore => Host.Map(static host => host.LocalStore).IfNone(false);
    public bool HostDocument => Host.Map(static host => host.Document).IfNone(false);
    public bool OtlpExport => Supplies(Capability.TelemetryExport);
    public string HostKey => Host.Map(static host => host.Key).IfNone("none");

    public bool Supplies(Capability capability) => Grants.Contains(capability);

    // Six rows in roster order under an ordinal provider-key sort: the canonical-json preimage the
    // corpus parity reads, so a set literal reordered at the composition root re-serializes identically.
    public ImmutableArray<KeyValuePair<string, string>> Canonical() => [
        new(ProfileAxis.Tenancy.Key, Tenancy.Key),
        new(ProfileAxis.Topology.Key, Topology.Key),
        new(ProfileAxis.Host.Key, HostKey),
        new(ProfileAxis.Lifecycle.Key, Lifecycle.Key),
        new(ProfileAxis.Isolation.Key, Isolation.Key),
        new(ProfileAxis.Providers.Key, string.Join(',', Providers.Map(static row => row.Key).Order(StringComparer.Ordinal))),
    ];
}

public sealed record ResolvedProfile(ConsumptionProfile Profile, string ApplicationName, string EnvironmentName, string ContentRoot, string ServiceVersion, ProfileRoots Roots, Option<RuntimeAttachment> Attachment, int ProcessId, Instant StartInstant) {
    public RecoveryObjective Recovery => Profile.Recovery;
}

public static class ProfileSurface {
    public static Fin<ConsumptionProfile> Admit(ConsumptionProfile profile) =>
        (profile.Topology == DeploymentTopology.InHost && profile.Host.IsNone, profile.Isolation.Needs) switch {
            (true, _) => Fin.Fail<ConsumptionProfile>(new ProfileFault.AxisUnsupported(
                new AxisEvidence(ProfileAxis.Host, "none", "in-host topology carries no host descriptor row"))),
            (_, { IsSome: true, Case: Capability needed }) when !profile.Supplies(needed) => Fin.Fail<ConsumptionProfile>(
                new ProfileFault.AxisUnsupported(new AxisEvidence(ProfileAxis.Isolation, profile.Isolation.Key, needed.Key))),
            _ => Fin.Succ(profile),
        };

    public static Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default) =>
        from row in Admit(profile)
        from admitted in attachment.IsSome && !row.SingleInstance
            ? Fin.Fail<Option<RuntimeAttachment>>(new ProfileFault.AttachmentRejected(row.HostKey))
            : Fin.Succ(attachment)
        from roots in ProfileIdentity.Roots(row, applicationName, admitted)
        select new ResolvedProfile(row, applicationName, environmentName, contentRoot, serviceVersion, roots, admitted, Environment.ProcessId, clock.GetCurrentInstant());
}
```

## [03]-[LIFETIME_ADAPTERS]

- Owner: `ProfileBoot` — builder selection, lifetime-adapter delegate rows, and `HostOptions` policy as one fold.
- Entry: `IHostApplicationBuilder Boot(ResolvedProfile resolved, Duration startupDeadline, Duration shutdownDeadline, Option<IHostApplicationBuilder> external = default)` — total over every row; both deadline values arrive from the deadline vocabulary.
- Auto: Boot composes the resolved `HostAttach` row's `CreateBuilder` and `AttachLifetime` delegates with `HostOptions` — startup and shutdown timeouts, concurrent start and stop, `BackgroundServiceExceptionBehavior.StopHost` — deleting per-host bootstrap programs; the `Managed` row registers `AddSystemd` for the Linux-server backend, and `MirrorService` rides the existing `Lifecycle.Subscribe` fold so every committed transition fires its service-state mirror through one subscriber seat, never a per-callsite emission; `Watchdog` rides the schedule-port heartbeat row as the keep-alive notify, never a second timer; `Aborted` flattens a `HostAbortedException` into the boot-fault trigger value with no second state machine.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Hosting.Systemd, Microsoft.Extensions.Options, NodaTime
- Receipt: `ServiceNotify` projects each `RuntimePhase` transition to its `ServiceState` sd_notify mirror through one table lookup, so a new host modality inherits the mirror as one row; `Watchdog` emits the `WatchdogPing` keep-alive payload on the same `ISystemdNotifier`, gated on the live notify socket; `Aborted` yields a `PhaseTrigger.FaultCommitted` value carrying `FaultSource.Unhandled` evidence with `Terminating: true`.
- Growth: one `HostAttach` row — a key beside two static delegate targets bound through the row constructor — extends the lifetime surface with zero new surface; one `ServiceNotify` row binds a new phase-to-state mirror without leaving the fold; the keep-alive notify stays one `Watchdog` emission bound to the existing schedule-port heartbeat, never a new port.
- Boundary: a `CoHostedAssets` host crosses in through `external` — its builder is constructed at the web app root, where ASP.NET Core enters as a shared-framework asset only; the host registers `ConsoleLifetime` as the default `IHostLifetime` on every builder path including the empty builder, so a `Foreign` attach swaps in the no-op `DetachedLifetime` through `Detached` and host-attach trigger injection drives phases; `AddSystemd` is the one service-manager registration — `SystemdHelpers.IsSystemdService` gates the live `ISystemdNotifier.Notify` emission so the notify socket is written only under systemd on the Linux-server backend; `MirrorService` registers one `Lifecycle.Subscribe` observer at the composition root for the `Managed` row, so `Emit` fires on every committed `PhaseReceipt` — `ServiceState.Ready` mirrors the ready transition and `ServiceState.Stopping` mirrors the draining transition, the two confirmed notify payloads — and the service-manager liveness keep-alive rides the schedule-port heartbeat row through `Watchdog`, which writes the `WatchdogPing` payload (`new ServiceState("WATCHDOG=1")`) on each heartbeat tick under the same notify-socket gate; `HostAbortedException` during build projects through `Aborted` to a boot-fault trigger value consumed by the transition entrypoint, never a second state machine.

```csharp signature
public static class ProfileBoot {
    public static readonly ServiceState WatchdogPing = new("WATCHDOG=1");

    public static HostApplicationBuilder CreateApp(HostApplicationBuilderSettings settings) => Host.CreateApplicationBuilder(settings);

    public static HostApplicationBuilder CreateEmpty(HostApplicationBuilderSettings settings) => Host.CreateEmptyApplicationBuilder(settings);

    public static IHostApplicationBuilder Inherit(IHostApplicationBuilder builder) => builder;

    public static IHostApplicationBuilder Detached(IHostApplicationBuilder builder) =>
        (builder.Services.Replace(ServiceDescriptor.Describe(typeof(IHostLifetime), typeof(DetachedLifetime), ServiceLifetime.Singleton)), builder).Item2;

    public static IHostApplicationBuilder Quiet(IHostApplicationBuilder builder) =>
        (builder.Services.Configure<ConsoleLifetimeOptions>(static options => options.SuppressStatusMessages = true), builder).Item2;

    public static IHostApplicationBuilder Service(IHostApplicationBuilder builder) =>
        (builder.Services.AddSystemd(), builder).Item2;

    public static Option<ServiceState> ServiceNotify(RuntimePhase phase) =>
        phase == RuntimePhase.Ready ? Some(ServiceState.Ready)
        : phase == RuntimePhase.Draining ? Some(ServiceState.Stopping)
        : None;

    public static Unit Emit(ISystemdNotifier notifier, RuntimePhase phase) =>
        ServiceNotify(phase).Match(
            Some: state => { if (notifier.IsEnabled) notifier.Notify(state); return unit; },
            None: static () => unit);

    public static Unit Watchdog(ISystemdNotifier notifier) {
        if (notifier.IsEnabled) notifier.Notify(WatchdogPing);
        return unit;
    }

    public static PhaseSubscription MirrorService(Lifecycle lifecycle, ISystemdNotifier notifier) =>
        lifecycle.Subscribe(receipt => ignore(Emit(notifier, receipt.To)));

    public static PhaseTrigger Aborted(HostAbortedException abort) =>
        new PhaseTrigger.FaultCommitted(new FaultSource.Unhandled(Error.New(abort), Terminating: true));

    public static IHostApplicationBuilder Boot(ResolvedProfile resolved, Duration startupDeadline, Duration shutdownDeadline, Option<IHostApplicationBuilder> external = default) =>
        Tuned(
            resolved.Profile.Attach.AttachLifetime(external.IfNone(() => resolved.Profile.Attach.CreateBuilder(new HostApplicationBuilderSettings {
                ApplicationName = resolved.ApplicationName,
                EnvironmentName = resolved.EnvironmentName,
                ContentRootPath = resolved.ContentRoot,
            }))),
            startupDeadline,
            shutdownDeadline);

    static IHostApplicationBuilder Tuned(IHostApplicationBuilder builder, Duration startup, Duration shutdown) =>
        (builder.Services.Configure<HostOptions>(options => {
            options.StartupTimeout = startup.ToTimeSpan();
            options.ShutdownTimeout = shutdown.ToTimeSpan();
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
        }), builder).Item2;

    private sealed class DetachedLifetime : IHostLifetime {
        public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

Lifetime signals project into phase-transition trigger values consumed by the transition entrypoint as one vocabulary:

| [INDEX] | [SIGNAL]                                    | [PROJECTION]                            |
| :-----: | :------------------------------------------ | :-------------------------------------- |
|  [01]   | `IHostedLifecycleService.StartingAsync`     | boot                                    |
|  [02]   | `IHostedLifecycleService.StartedAsync`      | ready                                   |
|  [03]   | `IHostApplicationLifetime` started token    | running                                 |
|  [04]   | `IHostedLifecycleService.StoppingAsync`     | draining                                |
|  [05]   | `IHostApplicationLifetime` stopping token   | draining                                |
|  [06]   | `IHostedLifecycleService.StoppedAsync`      | unloaded                                |
|  [07]   | `IHostApplicationLifetime` stopped token    | unloaded                                |
|  [08]   | `HostAbortedException` during build         | faulted                                 |
|  [09]   | `ServiceState.Ready` via `ServiceNotify`    | sd_notify mirror of the ready commit    |
|  [10]   | `ServiceState.Stopping` via `ServiceNotify` | sd_notify mirror of the draining commit |
|  [11]   | `WatchdogPing` via `Watchdog`               | sd_notify keep-alive on each heartbeat  |

## [04]-[RESOURCE_IDENTITY]

- Owner: `ProfileIdentity` — per-user root computation and the telemetry resource triple; `ProfileRoots` is the path artifact carried inside the resolved record, carrying the durable OTLP queue root beside the store and support roots; `QueueRootVariable` the deploy coordinate for that queue, spelled off the `Runtime/config#SOURCE_AXIS` `ConfigSource.EnvPrefix`; `HostResourceDetector` the one `IResourceDetector` carrying both the resolved record and its composition-supplied extra rows.
- Entry: `ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` — pure projection over the resolved record; `string InstanceId(ResolvedProfile resolved)` the one per-process instance spelling the resource row and the boot log enricher share; `new HostResourceDetector(resolved, extra)` is the detector `Observability/telemetry#SIGNAL_GOVERNANCE` `ResourceIdentity.Compose` seats ahead of the contrib detector chain.
- Auto: identity derives from the resolved record before any provider construction, and the detector's `Detect` returns that projection through `new Resource(IEnumerable<KeyValuePair<string, object>>)`, so `ConfigureResource` admits ONE resource feed and a per-call attribute push at each provider is the deleted form; the triple assembles from the `TelemetryDomain` namespace const and the resolved record alone, so a branch-wide namespace rename moves every resource, instrument, and dimension together; rasm-owned resource dimensions read their `TelemetryDomain` row rather than a literal, so each one resolves the roster the conformance gate proves against; the queue root folds the deploy-declared durable volume ahead of the local-disk evidence, so a containerized service arms its offline queue on the path a deployment mounted while a desktop host arms on its own base and a host owning neither opens none, and store residence and queue residence stay two answers on every arm — a companion scopes both under its own segment, an integrating instance keeps its queue off the shared store root it attached to, and every queue scopes by host key so two co-resident processes under one mount stay apart.
- Packages: OpenTelemetry, Rasm, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one attribute row or one root policy value per new identity fact, or one sibling `IResourceDetector` composed through `ResourceIdentity.Compose`; zero new surface.
- Boundary: roots are ApplicationData-rooted per-user paths — a `LocalStore` host stores under the application base, companion topology scopes its own companion store, and every other row runs scratch-only; Persistence consumes the resolved record and derives no path; host-document identity enters as one extra attribute row where the descriptor carries `Document`; the resource triple is `service.namespace` `rasm`, `service.name` the `TelemetryDomain.Qualify` render of the application row, and `service.instance.id` as pid joined with the start instant — the qualified name is load-bearing because a metrics store maps a subset of resource attributes onto series labels, so a store dropping `service.namespace` still separates this estate's emitters from a foreign `service.name`, and the qualifier rather than a local concatenation owns it so an already-prefixed or PascalCase application id lands one dotted lowercase spelling instead of two; `deployment.environment.name` is the live semconv spelling and the bare `deployment.environment` key is the deprecated form no exporter re-introduces; `QueueRoot` is the ONLY durable-telemetry path any composition reads — an offline queue rooted at a container layer loses its tail on the next reschedule, a queue rooted at a shared store root corrupts on a second live instance, and a queue rooted at a base two co-resident processes share lets each drain the other's batches, so every arm answers residence here rather than at a consumer and `QueueRootVariable` is the one coordinate a deployment sets to declare the volume that survives it; deriving queue residence from `LocalStore` alone is the deleted form, because that column answers where a document store lives and disarms durable buffering on exactly the service and edge rows that always export; `HostResourceDetector` is the one resource-discovery seam and a hand-pushed attribute list at a provider builder is the deleted pattern.

```csharp signature
public sealed record ProfileRoots(string AppRoot, Option<string> StoreRoot, string SupportRoot, Option<string> QueueRoot);

public static class ProfileIdentity {
    // Durable-telemetry disk is a DEPLOYMENT fact under the one config env prefix its owner declares, read raw
    // because roots resolve before any configuration source mounts. Containerized roots resolve a per-user
    // base into an image layer a reschedule erases and no in-process probe tells that apart from a mounted
    // volume, so the deploy plane names the surviving path or the composition opens no queue and reports none.
    public const string QueueRootVariable = ConfigSource.EnvPrefix + "TELEMETRY_QUEUE_ROOT";

    public static Fin<ProfileRoots> Roots(ConsumptionProfile profile, string applicationName, Option<RuntimeAttachment> attachment) =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) is { Length: > 0 } data
            ? Fin.Succ(Folded(profile, Path.Join(data, applicationName), attachment))
            : Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.ApplicationData)));

    // ONE spelling of the per-process instance identity: the resource row carries it and the static log
    // enricher stamps that same row onto every record, so a restart-lineage question answers identically from
    // a metric series and from a log line and neither plane derives it a second time.
    public static string InstanceId(ResolvedProfile resolved) =>
        $"{resolved.ProcessId}:{InstantPattern.ExtendedIso.Format(resolved.StartInstant)}";

    // Triple heads the array so a truncating collector keeps identity; every rasm-owned row spells its
    // TelemetryDomain member, so SignalGovernance.Rostered proves these keys against the same roster it
    // proves instrument names against and a literal drifting off the roster has no spelling here.
    public static ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra) => Admitted([
        new("service.namespace", TelemetryDomain.Namespace),
        new("service.name", TelemetryDomain.Qualify(resolved.ApplicationName)),
        new("service.version", resolved.ServiceVersion),
        new("service.instance.id", InstanceId(resolved)),
        new("deployment.environment.name", resolved.EnvironmentName),
        new(TelemetryDomain.Host.Measure("kind"), resolved.Profile.HostKey),
        new(TelemetryDomain.Deploy.Measure("tenancy"), resolved.Profile.Tenancy.Key),
        new(TelemetryDomain.Deploy.Measure("topology"), resolved.Profile.Topology.Key),
        new(TelemetryDomain.Deploy.Measure("lifecycle"), resolved.Profile.Lifecycle.Key),
        new(TelemetryDomain.Deploy.Measure("isolation"), resolved.Profile.Isolation.Key),
    ], [.. extra]);

    // Extra rows NARROW to keys the mint does not own: merge order is precedence at every consumer, so a plugin
    // discriminator carrying `service.name` or `service.instance.id` displaces the identity every series and
    // every log record joins on, and whichever row lands last wins silently. The owned key set derives from the
    // minted rows themselves, so a new identity row closes over its own key with no second roster to edit.
    static ImmutableArray<KeyValuePair<string, object>> Admitted(
        ImmutableArray<KeyValuePair<string, object>> minted, ImmutableArray<KeyValuePair<string, object>> extra) =>
        minted.AddRange(extra.Where(row => !minted.Any(held => string.Equals(held.Key, row.Key, StringComparison.Ordinal))));

    // Extra rows ride the detector rather than a second push site, so one Detect call carries the whole
    // resource and a composition adding a fact never widens the provider-side seam.
    public sealed record HostResourceDetector(ResolvedProfile Resolved, ImmutableArray<KeyValuePair<string, object>> Extra) : IResourceDetector {
        public Resource Detect() => new(ResourceAttributes(Resolved, Extra.AsSpan()));
    }

    // Store residence and LOCAL queue residence are two independent columns every arm answers, because the
    // base root is per-USER and per-application, never per-process. A companion runs beside its parent under
    // that one root, so both of its directories scope under the companion segment: a queue left at the
    // parent's path gives two live processes one blob directory, where each leases and drains the other's
    // batches through its own endpoint. The integrating arm inverts the pair — its STORE is the shared root it
    // attached to while its queue stays under its own base, since a shared store root is reached by whichever
    // instance attached to it. A host owning no local disk offers no local answer and takes the deploy one.
    static ProfileRoots Folded(ConsumptionProfile profile, string baseRoot, Option<RuntimeAttachment> attachment) =>
        (profile.Topology == DeploymentTopology.Companion, profile.LocalStore, attachment.Case) switch {
            (true, _, _) => Rooted(profile, baseRoot, Some(Path.Join(baseRoot, "companion")), Some(Path.Join(baseRoot, "companion"))),
            (_, true, RuntimeAttachment.Integrating link) => Rooted(profile, baseRoot, Some(link.SharedStoreRoot), Some(baseRoot)),
            (_, true, _) => Rooted(profile, baseRoot, Some(Path.Join(baseRoot, "store")), Some(baseRoot)),
            _ => Rooted(profile, baseRoot, None, None),
        };

    // Deploy coordinate OUTRANKS the local answer, so a service or edge row — the topologies that always
    // export and own no local store — arms its queue on the volume a deployment mounted rather than on the
    // one column that answers a document-store question. Both answers then scope by host key, so a parent and
    // its co-resident companion never lease and drain each other's batches under one mounted directory.
    static ProfileRoots Rooted(ConsumptionProfile profile, string baseRoot, Option<string> store, Option<string> local) {
        Option<string> deployed = Optional(Environment.GetEnvironmentVariable(QueueRootVariable))
            .Filter(static declared => declared.Length > 0);
        return new(baseRoot, store, Path.Join(baseRoot, "support"),
            (deployed.IsSome ? deployed : local).Map(root => Path.Join(root, "otlp", profile.HostKey)));
    }
}
```

## [05]-[POWER_AND_FIDELITY]

- Owner: `PowerState` `[SmartEnum<string>]` the host power-source axis under the `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `ThermalPressure` `[SmartEnum<int>]` the thermal-budget ladder whose generated key IS the rank; `PowerReading` the probed triple; `PowerAuthority` `[SmartEnum<string>]` the platform row owning the read; `FidelityScale` the compute-fidelity policy record graded from one reading; `PowerCell` the atom-backed capsule holding the last ADMITTED reading; `PowerProbe` the delegate targets the authority rows bind.
- Cases: 3 power rows — plugged, battery, low-battery; 4 thermal rows — nominal(0), fair(1), serious(2), critical(3) — the macOS thermal-pressure ladder; 2 authority rows — `Darwin` over IOKit and `NSProcessInfo.thermalState`, `Absent` for every platform whose authority has not landed; 4 `FidelityScale` grades spanning burst through conserve.
- Entry: `PowerAuthority.Platform` selects the row the running platform owns and `Read()` returns `Fin<PowerReading>`; `PowerReading.Of(PowerState, ThermalPressure, double)` returns `Fin<PowerReading>` — the one construction route, admitting the charge fraction finite and inside `[0, 1]` so no platform read's raw double reaches a ceiling comparison; `FidelityScale.Grade(PowerReading)` is the total projection into the profile the compute scheduler reads; `PowerCell.Refresh()` re-probes and returns the cell, so the health `Gauge` probe is the one sampling site and `PowerCell.Thermal` reads the rank `PressurePolicy.Grade` folds beside CPU and memory.
- Auto: a plugged host at nominal thermal pressure grades to the full burst profile; a low-battery or critical-thermal host grades to the sustained profile that caps parallelism and lowers the compute fidelity tier so the device stays within its energy and thermal budget; a refused read holds the prior reading, and a cell that never admitted one grades `Balanced` — bursting on absent evidence is the fabricated full-charge grade the authority rows exist to refuse; the power state feeds the resource-pressure health contributor as one extra grade input so a thermally-throttled host degrades through the existing degradation rail, never a parallel power alarm.
- Receipt: `FidelityScale` carries the parallelism cap, the fidelity tier, and the sustained flag the compute scheduler reads; a power-state transition logs through one `SpineLog` event in the 1000-1099 EVENT stride (`FaultBand.SpineEvents`).
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one power row absorbs a new power source; one thermal row absorbs a new pressure level; one `PowerAuthority` row with its `PowerProbe` target absorbs a new platform authority; a new fidelity profile is one `FidelityScale` grade arm, never a parallel scaling owner; zero new surface.
- Boundary: the power-and-fidelity fold is the only energy-awareness owner — a per-solve battery check, an ad hoc thermal poll, and a parallel power monitor are the deleted forms; the fidelity scale is data the Compute scheduler reads to bound its `CpuBudget` and lane parallelism, so the host owns the power-state truth and the compute scheduler consumes the fidelity grade, never re-reading the power state; platform variance rides the `PowerAuthority` roster rather than a runtime `if` inside the probe, and a row whose read has not landed REFUSES — a synthesized plugged-at-nominal-at-full-charge triple is indistinguishable from a measured one at every consumer, which is why absence crosses as a typed refusal the cell holds against; the capsule holds one atom and a `MeterListener` seat beside it is dead apparatus, because power and thermal state reach the process by native probe alone and publish no meter — the `UtilizationCell` listener is the resource-monitoring path and this cell never twins it; the power state enters the resource-pressure grade as a third input beside CPU and memory so a thermally-throttled host degrades on the same `Pressure`-tagged rule, never a new degradation level.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PowerState {
    public static readonly PowerState Plugged = new("plugged");
    public static readonly PowerState Battery = new("battery");
    public static readonly PowerState LowBattery = new("low-battery");
}

// Generated key IS the rank: the macOS ladder orders nominal through critical, so every ceiling compares
// int keys and a Rank column beside them is a second ordering that drifts.
[SmartEnum<int>]
public sealed partial class ThermalPressure {
    public static readonly ThermalPressure Nominal = new(0);
    public static readonly ThermalPressure Fair = new(1);
    public static readonly ThermalPressure Serious = new(2);
    public static readonly ThermalPressure Critical = new(3);
}

// Charge admits at the PROBE boundary, never at the grade: a non-finite or out-of-band fraction compares false
// against every ceiling, so an unadmitted reading grades a nearly-flat battery as burst budget and no consumer
// tells that from a measured full charge. `Of` is the one construction route, so a platform authority landing
// its native read hands a raw double to the gate rather than to `FidelityScale.Grade`, which stays policy alone.
public readonly record struct PowerReading {
    private PowerReading(PowerState power, ThermalPressure thermal, double battery) =>
        (Power, Thermal, BatteryFraction) = (power, thermal, battery);

    public PowerState Power { get; }

    public ThermalPressure Thermal { get; }

    public double BatteryFraction { get; }

    public static Fin<PowerReading> Of(PowerState power, ThermalPressure thermal, double battery) =>
        double.IsFinite(battery) && battery is >= 0d and <= 1d
            ? Fin.Succ(new PowerReading(power, thermal, battery))
            : Fin.Fail<PowerReading>(new ProfileFault.Text($"power-reading:battery-fraction {battery} outside [0,1]"));
}

public sealed record FidelityScale(
    int ParallelismCap,
    int FidelityTier,
    bool Sustained) {
    // Reserve is the battery share below which a discharging host stops treating charge as spare budget.
    public const double BatteryReserve = 0.2d;

    public static readonly FidelityScale Burst = new(ParallelismCap: int.MaxValue, FidelityTier: 3, Sustained: false);
    public static readonly FidelityScale Balanced = new(ParallelismCap: Environment.ProcessorCount, FidelityTier: 2, Sustained: false);
    // Halved cap floors at one: a single-core host resolves to zero permits and starves every lane.
    public static readonly FidelityScale Sustained = new(ParallelismCap: int.Max(1, Environment.ProcessorCount / 2), FidelityTier: 1, Sustained: true);
    public static readonly FidelityScale Conserve = new(ParallelismCap: 1, FidelityTier: 0, Sustained: true);

    public static FidelityScale Grade(PowerReading reading) =>
        reading.Thermal.Key >= ThermalPressure.Critical.Key ? Conserve
        : reading.Thermal.Key >= ThermalPressure.Serious.Key ? Sustained
        : reading.Power == PowerState.LowBattery
            || (reading.Power == PowerState.Battery && reading.BatteryFraction < BatteryReserve) ? Sustained
        : reading.Power == PowerState.Battery ? Balanced
        : Burst;
}

// Platform variance is a row, so a host whose authority has not landed refuses instead of synthesizing a
// reading no consumer can tell from a measured one.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PowerAuthority {
    public static readonly PowerAuthority Darwin = new("darwin", read: PowerProbe.Darwin);
    public static readonly PowerAuthority Absent = new("absent", read: PowerProbe.Absent);

    [UseDelegateFromConstructor]
    public partial Fin<PowerReading> Read();

    public static PowerAuthority Platform => OperatingSystem.IsMacOS() ? Darwin : Absent;
}

public sealed class PowerCell(PowerAuthority authority) {
    private readonly Atom<Option<PowerReading>> cell = Atom(Option<PowerReading>.None);

    // Unread thermals report nominal so an unmeasured host never escalates the pressure grade on evidence
    // nobody took; unread fidelity grades Balanced so it never bursts on the same absence.
    public ThermalPressure Thermal => cell.Value.Map(static held => held.Thermal).IfNone(ThermalPressure.Nominal);

    public FidelityScale Read() => cell.Value.Map(FidelityScale.Grade).IfNone(FidelityScale.Balanced);

    // Refused probe HOLDS the last admitted reading: dropping back to absence lets one transient failure
    // grade a critically throttled host as unconstrained until the next successful read.
    public PowerCell Refresh() =>
        (ignore(cell.Swap(prior => authority.Read().Match(
            Succ: static reading => Some(reading),
            Fail: _ => prior))), this).Item2;
}

// Each row states what its read owes rather than a shared blank refusal, so a held reading's cause names
// its owing authority; IOKit/SMC shapes stay the POWER_NATIVE residual and a declared-never-called
// [LibraryImport] beside them is the deleted form.
public static class PowerProbe {
    public static Fin<PowerReading> Darwin() =>
        Unresolved(PowerAuthority.Darwin.Key, "the IOKit power-source read and the NSProcessInfo thermal-state ladder");

    public static Fin<PowerReading> Absent() =>
        Unresolved(PowerAuthority.Absent.Key, "a platform authority reporting battery charge and thermal pressure");

    static Fin<PowerReading> Unresolved(string authority, string requirement) =>
        Fin.Fail<PowerReading>(new ProfileFault.Text($"power-authority:{authority} requires {requirement}"));
}
```

## [06]-[RESEARCH]

- [PLUGIN_HOST]-[BLOCKED]: which load-context teardown sequence a Generic Host boot-and-unload survives inside the RhinoWIP plugin ALC without process exit, given the settled `Detached` lifetime swap and host-attach trigger injection; verify on a live host bring-up through `tools.assay bridge`.
- [POWER_NATIVE]-[BLOCKED]: which values `IOPSCopyPowerSourcesInfo` and the SMC thermal read return for AC-versus-battery state, charge fraction, and pressure level, given member shapes settled by tier-1 IOKit P/Invoke decompile and a `NSProcessInfo.thermalState` ladder mapping ordinal-wise onto `ThermalPressure`; verify on the running device, which alone reports battery and thermal state.
- [POWER_AUTHORITY_ROSTER]-[OPEN]: which Windows and Linux surfaces report AC-versus-battery state, charge fraction, and thermal pressure — the `GetSystemPowerStatus` struct fields against the `/sys/class/power_supply` and `/sys/class/thermal` node set — so each lands as one `PowerAuthority` row beside `Darwin` and `Absent` narrows to the platforms that genuinely publish none; verify the Windows struct on the assay member rail and the sysfs node set on a live Linux host.
- [WEB_ROOT]-[OPEN]: which static-file middleware registration serves the co-hosted bundle `CoHostedAssets` selects, at the web app root under the Microsoft.AspNetCore.App shared framework; verify on `libs/csharp/Rasm.AppHost/.api/`, then decompile the shared-framework static-files surface on the assay member rail.
- [WATCHDOG_INTERVAL]-[OPEN]: which member reads `WATCHDOG_USEC` (with its `WATCHDOG_PID` ownership guard) so the schedule-port heartbeat row derives its tick period as a half-deadline rather than a fixed column, the payload itself settled at `ProfileBoot.WatchdogPing`; verify on `libs/csharp/Rasm.AppHost/.api/api-hosting-lifetimes.md`, then decompile `Microsoft.Extensions.Hosting.Systemd` on the assay member rail.
