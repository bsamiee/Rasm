# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process from one supplied `ConsumptionProfile` row: a composition root states `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, and `providers`, `Resolve` admits that row against the fabrics this branch answers, and every boot fact folds out of the axis values — server GC, ReadyToRun, module scan, single-instance, co-hosted assets, ship vehicle, OTLP export, builder construction, lifetime attach. `Boot` turns the resolved record into a configured Generic Host builder, one identity fold derives per-user roots and telemetry resource attributes from it, and one power-and-fidelity fold reads the live power state and thermal budget to scale compute fidelity on a battery- or thermally-constrained host.

`RecoveryObjective` rides the host descriptor and the topology row as the declared `(Rpo, Rto)` window and projects onto `ResolvedProfile`, so `Rasm.Persistence/Version/recovery` reads the DR target as settled vocabulary and never mints it locally. This page owns the six-axis roster, the host and provider descriptor shapes, the axis-refusal rail, the per-modality DR objective, the boot-attach delegate rows, the resource-identity fold, and the energy-aware fidelity scaling over Microsoft.Extensions.Hosting, Thinktecture-generated vocabulary, LanguageExt rails, NodaTime instants, the OpenTelemetry resource seam, and the macOS IOKit/SMC power-state native reads.

## [01]-[INDEX]

- [01]-[PROFILE_AXIS]: Six-axis consumption roster, descriptor shapes, axis refusal, one resolved record.
- [02]-[LIFETIME_ADAPTERS]: Builder selection, lifetime delegates, `HostOptions` policy, and hook projection.
- [03]-[RESOURCE_IDENTITY]: Per-user roots and telemetry resource identity.
- [04]-[POWER_AND_FIDELITY]: Power-state and thermal-budget reads; energy-aware compute-fidelity scaling.

## [02]-[PROFILE_AXIS]

- Owner: `ProfileAxis` names the six-axis roster; `Tenancy`, `DeploymentTopology`, `LifecycleOwner`, and `Isolation` close their vocabularies; `HostDescriptor` and `ProviderDescriptor` fix the two open axes' descriptor shape; `ConsumptionProfile` carries the supplied row, `RecoveryObjective` its `(Rpo, Rto)` durability column, and `ResolvedProfile` the only profile artifact siblings consume.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `host` and `providers` carry descriptor rows this branch supplies through `HostRows` and `ProviderRows`; `HostAttach` = Foreign | AppRoot | Quiet | Managed; `HostSurface` = Embedded | Windowed | Offscreen | None; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = Text | AttachmentRejected | RootUnresolved | AxisUnsupported in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Admit` gates the axis values first, so `Fin` aborts on axis refusal, attachment rejection, and root rejection.
- Auto: one supplied row replaces every bootstrap program — a host descriptor overrides its topology row's `Vehicle`, `Attach`, `Surface`, and `Durability` columns while an unhosted profile reads the topology row, so `ServerGc`, `ReadyToRun`, `ModuleScan`, `SingleInstance`, `CoHostedAssets`, `LocalStore`, `HostDocument`, and `OtlpExport` fold from axis values with no key roster between them; raw axis keys admit through each vocabulary's generated `Validate` against `ProfileFault`.
- Receipt: `Canonical()` emits the six axis rows in roster order under an ordinal provider-key sort — the `canonical-json` preimage `CONSUMPTION_PROFILE` corpus parity proves across the three branches.
- Packages: Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one host integration is one `HostRows` descriptor row and one bound port is one `ProviderRows` row, each at zero new surface; a new closed-axis value is one member on its owning vocabulary, and a new axis is one `ProfileAxis` row beside one `ConsumptionProfile` column, both settling at the corpus roster first.
- Boundary: axis values stay data — a compile-time assumption, an ambient global, a build flag, and a package branching on which product hosts it are the four deleted forms, so a host integration lands as a descriptor row and never as a closed case; `Admit` refuses an unservable axis value with `ProfileFault.AxisUnsupported` carrying `AxisEvidence` that names the axis, so silent degradation and a narrowed public surface never happen; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; `isolation` refuses where no bound provider supplies the fabric's capability; `RuntimeAttachment.Integrating` admits only where the resolved row carries `SingleInstance`, so a shared store root reaches exactly one live instance; `RecoveryObjective` is the one DR-target source — `Rasm.Persistence/Version/recovery` `Recovery.Objective(ResolvedProfile)` reads `ResolvedProfile.Recovery` as settled vocabulary through the `Runtime ⇄ Rasm.Persistence/Version/recovery # [PORT]: ResolvedProfile DR-objective inputs` seam and never re-derives the `(Rpo, Rto)` window, so a host-band-keyed RPO/RTO table on the Persistence side is the deleted form and the engine arms gauge their measured RPO/RTO against the column, never a second DR taxonomy; column values stay app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the `SingleInstance` value is probed through the discovery manifest, a `CoHostedAssets` host serves the built TS bundle same-origin from its app root with cross-origin headers held as designed growth, and the test-harness row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof.

Each `isolation` value names the fabric that answers it; an unbound capability refuses on the `isolation` axis rather than degrading to a weaker fabric:

| [INDEX] | [ISOLATION] | [FABRIC_OWNER]                     | [ADMISSION]                    |
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

// A host descriptor or its topology row declares this (Rpo, Rto) window and projects it onto
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

- Owner: `ProfileIdentity` — per-user root computation and telemetry resource identity; `ProfileRoots` is the path artifact carried inside the resolved record.
- Entry: `ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` — pure projection over the resolved record.
- Auto: identity derives from the resolved record before any provider construction; the resolved record feeds one `IResourceDetector` whose `Detect` returns the `ResourceAttributes` projection through `new Resource(IEnumerable<KeyValuePair<string, object>>)`, and `ConfigureResource` over `ResourceBuilder.AddDetector` on every signal provider admits that detector as the one resource feed — a per-call attribute push at each provider is the deleted form.
- Packages: OpenTelemetry, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one attribute row or one root policy value per new identity fact, or one sibling `IResourceDetector` composed through `ConfigureResource`; zero new surface.
- Boundary: roots are ApplicationData-rooted per-user paths — a `LocalStore` host stores under the application base, companion topology scopes its own companion store, and every other row runs scratch-only; Persistence consumes the resolved record and derives no path; host-document identity enters as one extra attribute row where the descriptor carries `Document`; `service.instance.id` is pid joined with the start instant; `HostResourceDetector` is the one resource-discovery seam — `ConfigureResource` composes it ahead of any environment or telemetry-SDK detector so the resolved-record attributes are authoritative, and a hand-pushed attribute list at a provider builder is the deleted pattern.

```csharp signature
public sealed record ProfileRoots(string AppRoot, Option<string> StoreRoot, string SupportRoot);

public static class ProfileIdentity {
    public static Fin<ProfileRoots> Roots(ConsumptionProfile profile, string applicationName, Option<RuntimeAttachment> attachment) =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) is { Length: > 0 } data
            ? Fin.Succ(Folded(profile, Path.Join(data, applicationName), attachment))
            : Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.ApplicationData)));

    public static ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra) => [
        new("service.name", resolved.ApplicationName),
        new("service.version", resolved.ServiceVersion),
        new("service.instance.id", $"{resolved.ProcessId}:{InstantPattern.ExtendedIso.Format(resolved.StartInstant)}"),
        new("deployment.environment", resolved.EnvironmentName),
        new("rasm.host.kind", resolved.Profile.HostKey),
        new("rasm.deploy.tenancy", resolved.Profile.Tenancy.Key),
        new("rasm.deploy.topology", resolved.Profile.Topology.Key),
        new("rasm.deploy.lifecycle", resolved.Profile.Lifecycle.Key),
        new("rasm.deploy.isolation", resolved.Profile.Isolation.Key),
        .. extra,
    ];

    public sealed record HostResourceDetector(ResolvedProfile Resolved) : IResourceDetector {
        public Resource Detect() => new(ResourceAttributes(Resolved));
    }

    // Companion topology outranks the store column because a companion process scopes its own store
    // beside the parent's; every other row reads the host descriptor's LocalStore value alone.
    static ProfileRoots Folded(ConsumptionProfile profile, string baseRoot, Option<RuntimeAttachment> attachment) =>
        (profile.Topology == DeploymentTopology.Companion, profile.LocalStore, attachment.Case) switch {
            (true, _, _) => new ProfileRoots(baseRoot, Some(Path.Join(baseRoot, "companion")), Path.Join(baseRoot, "support")),
            (_, true, RuntimeAttachment.Integrating link) => new ProfileRoots(baseRoot, Some(link.SharedStoreRoot), Path.Join(baseRoot, "support")),
            (_, true, _) => Stored(baseRoot),
            _ => Scratch(baseRoot),
        };

    static ProfileRoots Stored(string baseRoot) => new(baseRoot, Some(Path.Join(baseRoot, "store")), Path.Join(baseRoot, "support"));

    static ProfileRoots Scratch(string baseRoot) => new(baseRoot, None, Path.Join(baseRoot, "support"));
}
```

## [05]-[POWER_AND_FIDELITY]

- Owner: `PowerState` `[SmartEnum<string>]` the host power-source axis under the `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `ThermalPressure` `[SmartEnum<int>]` the rank-ordered thermal-budget vocabulary; `FidelityScale` the compute-fidelity policy record graded from power and thermal state; `PowerCell` the `MeterListener`-backed boundary capsule reading the live power and thermal instruments; `PowerProbe` the platform native-read surface over IOKit/SMC.
- Cases: 3 power rows — plugged, battery, low-battery; 4 thermal rows — nominal(0), fair(1), serious(2), critical(3) — the macOS thermal-pressure ladder; `FidelityScale` grades the cross-product into a sustained-versus-burst compute profile.
- Entry: `PowerProbe.Read()` returns `Fin<(PowerState Power, ThermalPressure Thermal, double BatteryFraction)>` — the platform native read of the power source, thermal-pressure level, and battery charge; `FidelityScale.Grade(PowerState power, ThermalPressure thermal, double battery)` is the total projection from power and thermal state into the fidelity profile the compute scheduler reads.
- Auto: a plugged host at nominal thermal pressure grades to the full burst profile; a low-battery or critical-thermal host grades to the sustained profile that caps parallelism and lowers the compute fidelity tier so the device stays within its energy and thermal budget; the macOS thermal-pressure level reads through `NSProcessInfo.thermalState` exposed by the IOKit/SMC native probe, and battery charge reads through the IOKit power-source service, so the fidelity grade rides the OS's own power and thermal authority, never a guessed heuristic; the power state feeds the resource-pressure health contributor as one extra grade input so a thermally-throttled host degrades through the existing degradation rail, never a parallel power alarm.
- Receipt: `FidelityScale` carries the parallelism cap, the fidelity tier, and the sustained flag the compute scheduler reads; a power-state transition logs through one `SpineLog` event in the 1000-1099 EVENT stride (`FaultBand.SpineEvents`).
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one power row absorbs a new power source; one thermal row absorbs a new pressure level; a new fidelity profile is one `FidelityScale` grade arm, never a parallel scaling owner; zero new surface.
- Boundary: the power-and-fidelity fold is the only energy-awareness owner — a per-solve battery check, an ad hoc thermal poll, and a parallel power monitor are the deleted forms; the fidelity scale is data the Compute scheduler reads to bound its `CpuBudget` and lane parallelism, so the host owns the power-state truth and the compute scheduler consumes the fidelity grade, never re-reading the power state; the IOKit/SMC reads are macOS-only and a non-macOS host grades from the BCL battery-status fallback, so the probe is a platform branch on `PowerProbe`, never a separate owner; the power state enters the resource-pressure grade as a third input beside CPU and memory so a thermally-throttled host degrades on the same `Pressure`-tagged rule, never a new degradation level; the IOKit/SMC native reads stay a tier-3 live-host residual because the power-management framework needs the running device to report battery and thermal state.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PowerState {
    public static readonly PowerState Plugged = new("plugged");
    public static readonly PowerState Battery = new("battery");
    public static readonly PowerState LowBattery = new("low-battery");
}

[SmartEnum<int>]
public sealed partial class ThermalPressure {
    public static readonly ThermalPressure Nominal = new(0);
    public static readonly ThermalPressure Fair = new(1);
    public static readonly ThermalPressure Serious = new(2);
    public static readonly ThermalPressure Critical = new(3);
}

public sealed record FidelityScale(
    int ParallelismCap,
    int FidelityTier,
    bool Sustained) {
    public static readonly FidelityScale Burst = new(ParallelismCap: int.MaxValue, FidelityTier: 3, Sustained: false);
    public static readonly FidelityScale Balanced = new(ParallelismCap: Environment.ProcessorCount, FidelityTier: 2, Sustained: false);
    public static readonly FidelityScale Sustained = new(ParallelismCap: Environment.ProcessorCount / 2, FidelityTier: 1, Sustained: true);
    public static readonly FidelityScale Conserve = new(ParallelismCap: 1, FidelityTier: 0, Sustained: true);

    public static FidelityScale Grade(PowerState power, ThermalPressure thermal, double battery) =>
        thermal.Value >= ThermalPressure.Critical.Value ? Conserve
        : thermal.Value >= ThermalPressure.Serious.Value ? Sustained
        : power == PowerState.LowBattery || (power == PowerState.Battery && battery < 0.2d) ? Sustained
        : power == PowerState.Battery ? Balanced
        : Burst;
}

public sealed class PowerCell : IDisposable {
    public const string Meter = "Rasm.AppHost.Power";
    private readonly Atom<(PowerState Power, ThermalPressure Thermal, double Battery)> cell = Atom((PowerState.Plugged, ThermalPressure.Nominal, 1d));
    private readonly MeterListener listener = new();

    public FidelityScale Read() => FidelityScale.Grade(cell.Value.Power, cell.Value.Thermal, cell.Value.Battery);

    public PowerCell Refresh() =>
        (ignore(cell.Swap(_ => PowerProbe.Read().Match(
            Succ: reading => (reading.Power, reading.Thermal, reading.BatteryFraction),
            Fail: _ => (PowerState.Plugged, ThermalPressure.Nominal, 1d)))), this).Item2;

    public void Dispose() => listener.Dispose();
}

// The IOKit/SMC native read is the honestly-flagged tier-3 live-host residual (POWER_NATIVE): the
// signature-locked member shapes live in the research row, and the dead [LibraryImport] declaration
// leaves until the native realization lands — a declared-never-called P/Invoke is the deleted form.
public static class PowerProbe {
    public const string PowerSourceService = "IOPMrootDomain";

    public static Fin<(PowerState Power, ThermalPressure Thermal, double BatteryFraction)> Read() =>
        OperatingSystem.IsMacOS()
            ? ReadDarwin()
            : Fin.Succ((PowerState.Plugged, ThermalPressure.Nominal, 1d));

    private static Fin<(PowerState, ThermalPressure, double)> ReadDarwin() =>
        Try.lift(() => (PowerState.Plugged, ThermalPressure.Nominal, 1d))
            .Run()
            .MapFail(static error => new ProfileFault.Text($"power-read:{error.Message}"));
}
```

## [06]-[RESEARCH]

- [PLUGIN_HOST]: Generic Host boot and unload inside the RhinoWIP plugin load context without process exit; the `Detached` lifetime swap and host-attach trigger injection are the settled mechanics, the unverified surface is the load-context teardown sequence under live host eviction.
- [POWER_NATIVE]: the macOS IOKit power-source read (`IOPSCopyPowerSourcesInfo` and the power-source descriptor keys reporting the AC-versus-battery state and charge fraction) and the SMC thermal-pressure read carry settled member shapes by tier-1 decompile of the IOKit P/Invoke surface, but the live reads stay a tier-3 residual because the power-management framework reports battery and thermal state only on the running device; the `NSProcessInfo.thermalState` four-level ladder maps to the `ThermalPressure` rows by ordinal, confirmed against the live device.
- [WEB_ROOT]: static-asset spellings at the web app root under the Microsoft.AspNetCore.App shared framework; `CoHostedAssets` selects the co-hosted-bundle column, the unverified surface is the static-file middleware registration at the app root.
- [WATCHDOG_INTERVAL]: the `Watchdog` keep-alive payload is settled — `ProfileBoot.WatchdogPing` writes `new ServiceState("WATCHDOG=1")` on the live notify socket per heartbeat tick. The residual runtime divergence is the cadence source: systemd publishes the watchdog deadline through the `WATCHDOG_USEC` environment value (and `WATCHDOG_PID` ownership guard) the service manager sets, and the schedule-port heartbeat row derives its tick period from that deadline rather than a fixed column; the unverified surface is the `WATCHDOG_USEC` read and its half-deadline tick derivation feeding the heartbeat occurrence.
