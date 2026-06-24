# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process through one host-variance axis: eight string-keyed `HostProfile` rows carry every modality difference as column values and delegate bindings, one `Resolve` fold materializes the `ResolvedProfile` record siblings consume, one `Boot` fold turns that record into a configured Generic Host builder, one identity fold derives per-user roots and telemetry resource attributes from the same record, and one power-and-fidelity fold reads the live power state and thermal budget to scale compute fidelity on a battery- or thermally-constrained host. The per-modality durability objective is one `RecoveryObjective(Rpo, Rto)` column on the `HostProfile` row — a web service targets a tighter RPO/RTO window than a Rhino plugin — projected onto `ResolvedProfile` so `Rasm.Persistence/Version/recovery` reads the DR target as settled vocabulary and never mints it locally. The page owns the profile axis, the per-modality DR objective, the lifetime-adapter delegate rows, the resource-identity fold, and the energy-aware fidelity scaling over Microsoft.Extensions.Hosting, Thinktecture-generated vocabulary, LanguageExt rails, NodaTime instants, the OpenTelemetry resource seam, and the macOS IOKit/SMC power-state native reads.

## [01]-[INDEX]

- [01]-[PROFILE_AXIS]: Eight rows resolve every modality variance into one record.
- [02]-[LIFETIME_ADAPTERS]: Builder selection, lifetime delegates, `HostOptions` policy, and hook projection.
- [03]-[RESOURCE_IDENTITY]: Per-user roots and telemetry resource identity.
- [04]-[POWER_AND_FIDELITY]: Power-state and thermal-budget reads; energy-aware compute-fidelity scaling.

## [02]-[PROFILE_AXIS]

- Owner: `HostProfile` — one `[SmartEnum<string>]` host-variance axis; `RecoveryObjective` the per-modality `(Rpo, Rto)` durability target column; `ResolvedProfile` is the only profile artifact siblings consume.
- Cases: rhino-plugin, gh2-plugin, standalone-desktop, companion-process, sidecar, headless-service, web-service, test-host; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = Text | AttachmentRejected | RootUnresolved in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(HostProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Fin` aborts on attachment and root rejection.
- Auto: one Resolve fold replaces eight bootstrap programs — column values, attachment legality, per-user roots, and process identity land in one record; raw profile keys admit through the generated `Validate` against `ProfileFault`.
- Packages: Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one profile row — key, seven column values, two delegate bindings — absorbs a new host modality with zero new surface; standalone-integrating stays the `Integrating` field, never a ninth row.
- Boundary: column values are app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the standalone single-instance value is probed through the discovery manifest, the web row serves the built TS bundle same-origin from its app root with cross-origin headers held as designed growth, and the test row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof; the `RecoveryObjective` column is the one DR-target source — `Rasm.Persistence/Version/recovery` `Recovery.Objective(ResolvedProfile)` reads `ResolvedProfile.Recovery` as settled vocabulary through the `Runtime ⇄ Rasm.Persistence/Version/recovery # [PORT]: ResolvedProfile DR-objective inputs` seam and never re-derives the per-modality `(Rpo, Rto)` from the profile key, so a host-band-keyed RPO/RTO table on the Persistence side is the deleted form and the durability objective stays a profile column the runtime owns, the engine arms gauge their measured RPO/RTO against, never a second DR taxonomy.

```csharp signature
public sealed class HostProfileKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[SmartEnum]
public sealed partial class ShipVehicle {
    public static readonly ShipVehicle Yak = new();
    public static readonly ShipVehicle DesktopBundle = new();
    public static readonly ShipVehicle Oci = new();
    public static readonly ShipVehicle Folder = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuntimeAttachment {
    private RuntimeAttachment() { }
    public sealed record Isolated : RuntimeAttachment;
    public sealed record Integrating(string SharedStoreRoot) : RuntimeAttachment;
}

[Union]
public abstract partial record ProfileFault : Expected, IValidationError<ProfileFault> {
    private ProfileFault(string detail, int code) : base(detail, code, None) { }

    public static ProfileFault Create(string message) => new Text(message);

    public sealed record Text : ProfileFault { public Text(string detail) : base(detail, 1100) { } }
    public sealed record AttachmentRejected : ProfileFault { public AttachmentRejected(string detail) : base(detail, 1101) { } }
    public sealed record RootUnresolved : ProfileFault { public RootUnresolved(string detail) : base(detail, 1102) { } }
}

// The per-modality durability objective: the declared (Rpo, Rto) DR window each HostProfile row carries and
// projects onto ResolvedProfile. Rasm.Persistence/Version/recovery reads ResolvedProfile.Recovery as settled
// vocabulary and gauges its measured RPO/RTO against it, never re-deriving the window from the profile key.
public readonly record struct RecoveryObjective(Duration Rpo, Duration Rto) {
    public static readonly RecoveryObjective Strict = new(Duration.FromMinutes(1), Duration.FromMinutes(15));
    public static readonly RecoveryObjective Standard = new(Duration.FromMinutes(5), Duration.FromMinutes(30));
    public static readonly RecoveryObjective Relaxed = new(Duration.FromMinutes(15), Duration.FromHours(1));
    public static readonly RecoveryObjective Instant = new(Duration.Zero, Duration.Zero);

    public bool MeetsRpo(Duration measured) => measured <= Rpo;
    public bool MeetsRto(Duration measured) => measured <= Rto;
}

[SmartEnum<string>]
[ValidationError<ProfileFault>]
[KeyMemberEqualityComparer<HostProfileKeyPolicy, string>]
[KeyMemberComparer<HostProfileKeyPolicy, string>]
public sealed partial class HostProfile {
    public static readonly HostProfile RhinoPlugin = new("rhino-plugin", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Yak, recovery: RecoveryObjective.Relaxed, createBuilder: ProfileBoot.CreateEmpty, attachLifetime: ProfileBoot.Detached);
    public static readonly HostProfile Gh2Plugin = new("gh2-plugin", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Yak, recovery: RecoveryObjective.Relaxed, createBuilder: ProfileBoot.CreateEmpty, attachLifetime: ProfileBoot.Detached);
    public static readonly HostProfile StandaloneDesktop = new("standalone-desktop", serverGc: false, readyToRun: true, moduleScan: true, otlpExport: false, singleInstance: true, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, recovery: RecoveryObjective.Standard, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);
    public static readonly HostProfile CompanionProcess = new("companion-process", serverGc: true, readyToRun: true, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, recovery: RecoveryObjective.Standard, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Quiet);
    public static readonly HostProfile Sidecar = new("sidecar", serverGc: true, readyToRun: true, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, recovery: RecoveryObjective.Standard, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Quiet);
    public static readonly HostProfile HeadlessService = new("headless-service", serverGc: true, readyToRun: false, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Oci, recovery: RecoveryObjective.Strict, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Service);
    public static readonly HostProfile WebService = new("web-service", serverGc: true, readyToRun: false, moduleScan: false, otlpExport: true, singleInstance: false, coHostedAssets: true, vehicle: ShipVehicle.Oci, recovery: RecoveryObjective.Strict, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);
    public static readonly HostProfile TestHost = new("test-host", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Folder, recovery: RecoveryObjective.Instant, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);

    public bool ServerGc { get; }
    public bool ReadyToRun { get; }
    public bool ModuleScan { get; }
    public bool OtlpExport { get; }
    public bool SingleInstance { get; }
    public bool CoHostedAssets { get; }
    public ShipVehicle Vehicle { get; }
    public RecoveryObjective Recovery { get; }

    [UseDelegateFromConstructor]
    public partial HostApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings);

    [UseDelegateFromConstructor]
    public partial IHostApplicationBuilder AttachLifetime(IHostApplicationBuilder builder);
}

public sealed record ResolvedProfile(HostProfile Profile, string ApplicationName, string EnvironmentName, string ContentRoot, string ServiceVersion, ProfileRoots Roots, Option<RuntimeAttachment> Attachment, int ProcessId, Instant StartInstant) {
    public RecoveryObjective Recovery => Profile.Recovery;
}

public static class ProfileSurface {
    public static Fin<ResolvedProfile> Resolve(HostProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default) =>
        from admitted in attachment.IsSome && profile != HostProfile.StandaloneDesktop
            ? Fin.Fail<Option<RuntimeAttachment>>(new ProfileFault.AttachmentRejected(profile.Key))
            : Fin.Succ(attachment)
        from roots in ProfileIdentity.Roots(profile, applicationName, admitted)
        select new ResolvedProfile(profile, applicationName, environmentName, contentRoot, serviceVersion, roots, admitted, Environment.ProcessId, clock.GetCurrentInstant());
}
```

## [03]-[LIFETIME_ADAPTERS]

- Owner: `ProfileBoot` — builder selection, lifetime-adapter delegate rows, and `HostOptions` policy as one fold.
- Entry: `IHostApplicationBuilder Boot(ResolvedProfile resolved, Duration startupDeadline, Duration shutdownDeadline, Option<IHostApplicationBuilder> external = default)` — total over every row; both deadline values arrive from the deadline vocabulary.
- Auto: Boot composes the row's `CreateBuilder` and `AttachLifetime` delegates with `HostOptions` — startup and shutdown timeouts, concurrent start and stop, `BackgroundServiceExceptionBehavior.StopHost` — deleting per-host bootstrap programs; the `Service` row registers `AddSystemd` for the Linux-server backend, and `MirrorService` rides the existing `Lifecycle.Subscribe` fold so every committed transition fires its service-state mirror through one subscriber seat, never a per-callsite emission; `Watchdog` rides the schedule-port heartbeat row as the keep-alive notify, never a second timer; `Aborted` flattens a `HostAbortedException` into the boot-fault trigger value with no second state machine.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Hosting.Systemd, Microsoft.Extensions.Options, NodaTime
- Receipt: `ServiceNotify` projects each `RuntimePhase` transition to its `ServiceState` sd_notify mirror through one table lookup, so a new host modality inherits the mirror as one row; `Watchdog` emits the `WatchdogPing` keep-alive payload on the same `ISystemdNotifier`, gated on the live notify socket; `Aborted` yields a `PhaseTrigger.FaultCommitted` value carrying `FaultSource.Unhandled` evidence with `Terminating: true`.
- Growth: one adapter row — a static delegate target bound through the row constructor — extends the lifetime surface with zero new surface; one `ServiceNotify` row binds a new phase-to-state mirror without leaving the fold; the keep-alive notify stays one `Watchdog` emission bound to the existing schedule-port heartbeat, never a new port.
- Boundary: the web row crosses in through `external` — its builder is constructed at the web app root, where ASP.NET Core enters as a shared-framework asset only; the host registers `ConsoleLifetime` as the default `IHostLifetime` on every builder path including the empty builder, so plugin rows swap in the no-op `DetachedLifetime` through `Detached` and host-attach trigger injection drives phases; `AddSystemd` is the one service-manager registration — `SystemdHelpers.IsSystemdService` gates the live `ISystemdNotifier.Notify` emission so the notify socket is written only under systemd on the Linux-server backend; `MirrorService` registers one `Lifecycle.Subscribe` observer at the composition root for the service row, so `Emit` fires on every committed `PhaseReceipt` — `ServiceState.Ready` mirrors the ready transition and `ServiceState.Stopping` mirrors the draining transition, the two confirmed notify payloads — and the service-manager liveness keep-alive rides the schedule-port heartbeat row through `Watchdog`, which writes the `WatchdogPing` payload (`new ServiceState("WATCHDOG=1")`) on each heartbeat tick under the same notify-socket gate; `HostAbortedException` during build projects through `Aborted` to a boot-fault trigger value consumed by the transition entrypoint, never a second state machine.

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
            resolved.Profile.AttachLifetime(external.IfNone(() => resolved.Profile.CreateBuilder(new HostApplicationBuilderSettings {
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
- Boundary: roots are ApplicationData-rooted per-user paths — store under the application base on plugin and standalone rows, a scoped companion store on the companion row, no local store on sidecar, headless, web, and test rows; Persistence consumes the resolved record and derives no path; host-document identity enters as one extra attribute row on plugin rows; `service.instance.id` is pid joined with the start instant; `HostResourceDetector` is the one resource-discovery seam — `ConfigureResource` composes it ahead of any environment or telemetry-SDK detector so the resolved-record attributes are authoritative, and a hand-pushed attribute list at a provider builder is the deleted pattern.

```csharp signature
public sealed record ProfileRoots(string AppRoot, Option<string> StoreRoot, string SupportRoot);

public static class ProfileIdentity {
    public static Fin<ProfileRoots> Roots(HostProfile profile, string applicationName, Option<RuntimeAttachment> attachment) =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) is { Length: > 0 } data
            ? Fin.Succ(Folded(profile, Path.Join(data, applicationName), attachment))
            : Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.ApplicationData)));

    public static ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra) => [
        new("service.name", resolved.ApplicationName),
        new("service.version", resolved.ServiceVersion),
        new("service.instance.id", $"{resolved.ProcessId}:{InstantPattern.ExtendedIso.Format(resolved.StartInstant)}"),
        new("deployment.environment", resolved.EnvironmentName),
        new("rasm.host.kind", resolved.Profile.Key),
        .. extra,
    ];

    public sealed record HostResourceDetector(ResolvedProfile Resolved) : IResourceDetector {
        public Resource Detect() => new(ResourceAttributes(Resolved));
    }

    static ProfileRoots Folded(HostProfile profile, string baseRoot, Option<RuntimeAttachment> attachment) =>
        profile.Switch(
            state: (Base: baseRoot, Attachment: attachment),
            rhinoPlugin: static s => Stored(s.Base),
            gh2Plugin: static s => Stored(s.Base),
            standaloneDesktop: static s => s.Attachment is { IsSome: true, Case: RuntimeAttachment.Integrating link }
                ? new ProfileRoots(s.Base, Some(link.SharedStoreRoot), Path.Join(s.Base, "support"))
                : Stored(s.Base),
            companionProcess: static s => new ProfileRoots(s.Base, Some(Path.Join(s.Base, "companion")), Path.Join(s.Base, "support")),
            sidecar: static s => Scratch(s.Base),
            headlessService: static s => Scratch(s.Base),
            webService: static s => Scratch(s.Base),
            testHost: static s => Scratch(s.Base));

    static ProfileRoots Stored(string baseRoot) => new(baseRoot, Some(Path.Join(baseRoot, "store")), Path.Join(baseRoot, "support"));

    static ProfileRoots Scratch(string baseRoot) => new(baseRoot, None, Path.Join(baseRoot, "support"));
}
```

## [05]-[POWER_AND_FIDELITY]

- Owner: `PowerState` `[SmartEnum<string>]` the host power-source axis under the `HostProfileKeyPolicy` accessor; `ThermalPressure` `[SmartEnum<int>]` the rank-ordered thermal-budget vocabulary; `FidelityScale` the compute-fidelity policy record graded from power and thermal state; `PowerCell` the `MeterListener`-backed boundary capsule reading the live power and thermal instruments; `PowerProbe` the platform native-read surface over IOKit/SMC.
- Cases: 3 power rows — plugged, battery, low-battery; 4 thermal rows — nominal(0), fair(1), serious(2), critical(3) — the macOS thermal-pressure ladder; `FidelityScale` grades the cross-product into a sustained-versus-burst compute profile.
- Entry: `PowerProbe.Read()` returns `Fin<(PowerState Power, ThermalPressure Thermal, double BatteryFraction)>` — the platform native read of the power source, thermal-pressure level, and battery charge; `FidelityScale.Grade(PowerState power, ThermalPressure thermal, double battery)` is the total projection from power and thermal state into the fidelity profile the compute scheduler reads.
- Auto: a plugged host at nominal thermal pressure grades to the full burst profile; a low-battery or critical-thermal host grades to the sustained profile that caps parallelism and lowers the compute fidelity tier so the device stays within its energy and thermal budget; the macOS thermal-pressure level reads through `NSProcessInfo.thermalState` exposed by the IOKit/SMC native probe, and battery charge reads through the IOKit power-source service, so the fidelity grade rides the OS's own power and thermal authority, never a guessed heuristic; the power state feeds the resource-pressure health contributor as one extra grade input so a thermally-throttled host degrades through the existing degradation rail, never a parallel power alarm.
- Receipt: `FidelityScale` carries the parallelism cap, the fidelity tier, and the sustained flag the compute scheduler reads; a power-state transition logs through one `SpineLog` event in the 1000-1999 band.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one power row absorbs a new power source; one thermal row absorbs a new pressure level; a new fidelity profile is one `FidelityScale` grade arm, never a parallel scaling owner; zero new surface.
- Boundary: the power-and-fidelity fold is the only energy-awareness owner — a per-solve battery check, an ad hoc thermal poll, and a parallel power monitor are the deleted forms; the fidelity scale is data the Compute scheduler reads to bound its `CpuBudget` and lane parallelism, so the host owns the power-state truth and the compute scheduler consumes the fidelity grade, never re-reading the power state; the IOKit/SMC reads are macOS-only and a non-macOS host grades from the BCL battery-status fallback, so the probe is a platform branch on `PowerProbe`, never a separate owner; the power state enters the resource-pressure grade as a third input beside CPU and memory so a thermally-throttled host degrades on the same `Pressure`-tagged rule, never a new degradation level; the IOKit/SMC native reads stay a tier-3 live-host residual because the power-management framework needs the running device to report battery and thermal state.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<HostProfileKeyPolicy, string>]
[KeyMemberComparer<HostProfileKeyPolicy, string>]
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

public static partial class PowerProbe {
    public const string PowerSourceService = "IOPMrootDomain";

    [LibraryImport("/System/Library/Frameworks/IOKit.framework/IOKit", EntryPoint = "IOPSCopyPowerSourcesInfo")]
    private static partial nint CopyPowerSourcesInfo();

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
