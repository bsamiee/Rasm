# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process through one host-variance axis: eight string-keyed `HostProfile` rows carry every modality difference as column values and delegate bindings, one `Resolve` fold materializes the `ResolvedProfile` record siblings consume, one `Boot` fold turns that record into a configured Generic Host builder, and one identity fold derives per-user roots and telemetry resource attributes from the same record. The page owns the profile axis, the lifetime-adapter delegate rows, and the resource-identity fold over Microsoft.Extensions.Hosting, Thinktecture-generated vocabulary, LanguageExt rails, NodaTime instants, and the OpenTelemetry resource seam.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     |
| :-----: | :---------------- | :------------------------------------------------------------------------- |
|   [1]   | PROFILE_AXIS      | Eight rows resolve every modality variance into one record                 |
|   [2]   | LIFETIME_ADAPTERS | Builder selection, lifetime delegates, HostOptions policy, hook projection |
|   [3]   | RESOURCE_IDENTITY | Per-user roots and telemetry resource identity                             |

## [2]-[PROFILE_AXIS]

- Owner: `HostProfile` — one `[SmartEnum<string>]` host-variance axis; `ResolvedProfile` is the only profile artifact siblings consume.
- Cases: rhino-plugin, gh2-plugin, standalone-desktop, companion-process, sidecar, headless-service, web-service, test-host; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = Text | AttachmentRejected | RootUnresolved in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(HostProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Fin` aborts on attachment and root rejection.
- Auto: one Resolve fold replaces eight bootstrap programs — column values, attachment legality, per-user roots, and process identity land in one record; raw profile keys admit through the generated `Validate` against `ProfileFault`.
- Packages: Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one profile row — key, seven column values, two delegate bindings — absorbs a new host modality with zero new surface; standalone-integrating stays the `Integrating` field, never a ninth row.
- Boundary: column values are app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the standalone single-instance value is probed through the discovery manifest, the web row serves the built TS bundle same-origin from its app root with cross-origin headers held as designed growth, and the test row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof.

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

[SmartEnum<string>]
[ValidationError<ProfileFault>]
[KeyMemberEqualityComparer<HostProfileKeyPolicy, string>]
[KeyMemberComparer<HostProfileKeyPolicy, string>]
public sealed partial class HostProfile {
    public static readonly HostProfile RhinoPlugin = new("rhino-plugin", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Yak, createBuilder: ProfileBoot.CreateEmpty, attachLifetime: ProfileBoot.Detached);
    public static readonly HostProfile Gh2Plugin = new("gh2-plugin", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Yak, createBuilder: ProfileBoot.CreateEmpty, attachLifetime: ProfileBoot.Detached);
    public static readonly HostProfile StandaloneDesktop = new("standalone-desktop", serverGc: false, readyToRun: true, moduleScan: true, otlpExport: false, singleInstance: true, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);
    public static readonly HostProfile CompanionProcess = new("companion-process", serverGc: true, readyToRun: true, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Quiet);
    public static readonly HostProfile Sidecar = new("sidecar", serverGc: true, readyToRun: true, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.DesktopBundle, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Quiet);
    public static readonly HostProfile HeadlessService = new("headless-service", serverGc: true, readyToRun: false, moduleScan: true, otlpExport: true, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Oci, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Service);
    public static readonly HostProfile WebService = new("web-service", serverGc: true, readyToRun: false, moduleScan: false, otlpExport: true, singleInstance: false, coHostedAssets: true, vehicle: ShipVehicle.Oci, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);
    public static readonly HostProfile TestHost = new("test-host", serverGc: false, readyToRun: false, moduleScan: true, otlpExport: false, singleInstance: false, coHostedAssets: false, vehicle: ShipVehicle.Folder, createBuilder: ProfileBoot.CreateApp, attachLifetime: ProfileBoot.Inherit);

    public bool ServerGc { get; }
    public bool ReadyToRun { get; }
    public bool ModuleScan { get; }
    public bool OtlpExport { get; }
    public bool SingleInstance { get; }
    public bool CoHostedAssets { get; }
    public ShipVehicle Vehicle { get; }

    [UseDelegateFromConstructor]
    public partial HostApplicationBuilder CreateBuilder(HostApplicationBuilderSettings settings);

    [UseDelegateFromConstructor]
    public partial IHostApplicationBuilder AttachLifetime(IHostApplicationBuilder builder);
}

public sealed record ResolvedProfile(HostProfile Profile, string ApplicationName, string EnvironmentName, string ContentRoot, string ServiceVersion, ProfileRoots Roots, Option<RuntimeAttachment> Attachment, int ProcessId, Instant StartInstant);

public static class ProfileSurface {
    public static Fin<ResolvedProfile> Resolve(HostProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default) =>
        from admitted in attachment.IsSome && profile != HostProfile.StandaloneDesktop
            ? Fin.Fail<Option<RuntimeAttachment>>(new ProfileFault.AttachmentRejected(profile.Key))
            : Fin.Succ(attachment)
        from roots in ProfileIdentity.Roots(profile, applicationName, admitted)
        select new ResolvedProfile(profile, applicationName, environmentName, contentRoot, serviceVersion, roots, admitted, Environment.ProcessId, clock.GetCurrentInstant());
}
```

## [3]-[LIFETIME_ADAPTERS]

- Owner: `ProfileBoot` — builder selection, lifetime-adapter delegate rows, and `HostOptions` policy as one fold.
- Entry: `IHostApplicationBuilder Boot(ResolvedProfile resolved, Duration startupDeadline, Duration shutdownDeadline, Option<IHostApplicationBuilder> external = default)` — total over every row; both deadline values arrive from the deadline vocabulary.
- Auto: Boot composes the row's `CreateBuilder` and `AttachLifetime` delegates with `HostOptions` — startup and shutdown timeouts, concurrent start and stop, `BackgroundServiceExceptionBehavior.StopHost` — deleting per-host bootstrap programs; the `Service` row gates `AddSystemd`/`AddWindowsService` on the matching probe, and `MirrorService` rides the existing `Lifecycle.Subscribe` fold so every committed transition fires its service-state mirror through one subscriber seat, never a per-callsite emission; `Aborted` flattens a `HostAbortedException` into the boot-fault trigger value with no second state machine.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Hosting.Systemd, Microsoft.Extensions.Hosting.WindowsServices, Microsoft.Extensions.Options, NodaTime
- Receipt: `ServiceNotify` projects each `RuntimePhase` transition to its `ServiceState` sd_notify mirror through one table lookup, so a new host modality inherits the mirror as one row; `Aborted` yields a `PhaseTrigger.FaultCommitted` value carrying `FaultSource.Unhandled` evidence with `Terminating: true`.
- Growth: one adapter row — a static delegate target bound through the row constructor — extends the lifetime surface with zero new surface; one `ServiceNotify` row binds a new phase-to-state mirror without leaving the fold.
- Boundary: the web row crosses in through `external` — its builder is constructed at the web app root, where ASP.NET Core enters as a shared-framework asset only; the host registers `ConsoleLifetime` as the default `IHostLifetime` on every builder path including the empty builder, so plugin rows swap in the no-op `DetachedLifetime` through `Detached` and host-attach trigger injection drives phases; `AddSystemd` and `AddWindowsService` coexist because each registration is environment-gated, the windows registration carries the SCM name through `WindowsServiceLifetimeOptions.ServiceName` bound to the resolved `IHostEnvironment.ApplicationName` so the service control manager registers under the canonical application identity rather than the executable stem, and `SystemdHelpers.IsSystemdService`/`WindowsServiceHelpers.IsWindowsService` gate the live `ISystemdNotifier.Notify` emission so the notify socket is written only under a service manager; `MirrorService` registers one `Lifecycle.Subscribe` observer at the composition root for service rows, so `Emit` fires on every committed `PhaseReceipt` — `ServiceState.Ready` mirrors the ready transition and `ServiceState.Stopping` mirrors the draining transition, the two confirmed notify payloads — and the service-manager liveness keep-alive rides the schedule-port heartbeat row rather than a second timer; `HostAbortedException` during build projects through `Aborted` to a boot-fault trigger value consumed by the transition entrypoint, never a second state machine.

```csharp signature
public static class ProfileBoot {
    public static HostApplicationBuilder CreateApp(HostApplicationBuilderSettings settings) => Host.CreateApplicationBuilder(settings);

    public static HostApplicationBuilder CreateEmpty(HostApplicationBuilderSettings settings) => Host.CreateEmptyApplicationBuilder(settings);

    public static IHostApplicationBuilder Inherit(IHostApplicationBuilder builder) => builder;

    public static IHostApplicationBuilder Detached(IHostApplicationBuilder builder) =>
        (builder.Services.Replace(ServiceDescriptor.Describe(typeof(IHostLifetime), typeof(DetachedLifetime), ServiceLifetime.Singleton)), builder).Item2;

    public static IHostApplicationBuilder Quiet(IHostApplicationBuilder builder) =>
        (builder.Services.Configure<ConsoleLifetimeOptions>(static options => options.SuppressStatusMessages = true), builder).Item2;

    public static IHostApplicationBuilder Service(IHostApplicationBuilder builder) =>
        (builder.Services
            .AddSystemd()
            .AddWindowsService(options => options.ServiceName = builder.Environment.ApplicationName), builder).Item2;

    public static Option<ServiceState> ServiceNotify(RuntimePhase phase) =>
        phase == RuntimePhase.Ready ? Some(ServiceState.Ready)
        : phase == RuntimePhase.Draining ? Some(ServiceState.Stopping)
        : None;

    public static Unit Emit(ISystemdNotifier notifier, RuntimePhase phase) =>
        ServiceNotify(phase).Match(
            Some: state => { if (notifier.IsEnabled) notifier.Notify(state); return unit; },
            None: static () => unit);

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
|   [1]   | `IHostedLifecycleService.StartingAsync`     | boot                                    |
|   [2]   | `IHostedLifecycleService.StartedAsync`      | ready                                   |
|   [3]   | `IHostApplicationLifetime` started token    | running                                 |
|   [4]   | `IHostedLifecycleService.StoppingAsync`     | draining                                |
|   [5]   | `IHostApplicationLifetime` stopping token   | draining                                |
|   [6]   | `IHostedLifecycleService.StoppedAsync`      | unloaded                                |
|   [7]   | `IHostApplicationLifetime` stopped token    | unloaded                                |
|   [8]   | `HostAbortedException` during build         | faulted                                 |
|   [9]   | `ServiceState.Ready` via `ServiceNotify`    | sd_notify mirror of the ready commit    |
|  [10]   | `ServiceState.Stopping` via `ServiceNotify` | sd_notify mirror of the draining commit |

## [4]-[RESOURCE_IDENTITY]

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

## [5]-[RESEARCH]

- [PLUGIN_HOST]: Generic Host boot and unload inside the RhinoWIP plugin load context without process exit; the `Detached` lifetime swap and host-attach trigger injection are the settled mechanics, the unverified surface is the load-context teardown sequence under live host eviction.
- [WEB_ROOT]: static-asset spellings at the web app root under the Microsoft.AspNetCore.App shared framework; `CoHostedAssets` selects the co-hosted-bundle column, the unverified surface is the static-file middleware registration at the app root.
- [WATCHDOG_PING]: the systemd watchdog liveness channel beyond the `ServiceState.Ready`/`ServiceState.Stopping` payloads — the periodic keep-alive notify rides the schedule-port heartbeat row, the unverified surface is the watchdog-keepalive notify payload on `ISystemdNotifier`.
