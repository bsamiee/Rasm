# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process from one supplied `ConsumptionProfile` row: a composition root states `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, and `providers`, `Resolve` admits that row against the crossings this branch answers, and every boot fact folds out of the axis values.

`Boot` turns the resolved record into a configured Generic Host builder, one identity fold derives per-user roots and telemetry resource attributes from it, and one power-and-fidelity fold reads the live power state and thermal budget to scale compute fidelity on a constrained host.

`RecoveryObjective` rides the host descriptor and the topology row as the declared `(Rpo, Rto)` window and projects onto `ResolvedProfile`, so `Rasm.Persistence/Version/recovery` reads the DR target as settled vocabulary.

This page owns the six-axis roster, the two descriptor shapes, the axis-refusal channel, the boot-attach delegate rows, the resource-identity fold, and the energy-aware fidelity scaling.

Settled kernel composition: `ICapability<TSelf>`, `CapabilitySet<TCapability>`, and `CapabilityLaw<TCapability>` arrive from `Rasm/Domain/validation#CAPABILITY`; `Transition<TState>` and `Cell.Step` from `Rasm/Domain/results#TRANSITION`; `FaultBand` and `Fault` from `#FAULT_BAND`; `TelemetrySource` from `Rasm/Domain/frame#SOURCE`; `HookTap` and `HookSet` from `Rasm/Domain/hooks#HOOKS`.

Settled folder composition: `Faculty` — the retained-capability vocabulary a provider row supplies and an isolation crossing demands — arrives from Observability/health#DEGRADATION_LADDER, and `FidelityScale` travels back the other way as the thermal-and-power half of that page's ONE pressure grade.

`AppHostPoint` and `AppHostFact` arrive from Observability/hooks#HOOK_ROSTER; `RuntimePhase`, `PhaseTrigger`, `FaultSource`, `TerminationKind`, and `PhaseCommit` from Runtime/lifecycle#PHASE_FAMILY and `#FAULT_SPINE`; `ClockPolicy` from Runtime/time#CLOCK_POLICY; `ConfigSource.EnvPrefix` and `ReloadOutcome` from Runtime/config#SOURCE_AXIS and `#OPTIONS_ADMISSION`; `DumpPolicy` from Observability/bundles#SUPPORT_CAPTURE; `TelemetryDomain` from Observability/telemetry#SIGNAL_GOVERNANCE.

## [01]-[INDEX]

- [02]-[PROFILE_AXIS]: Six-axis consumption roster, descriptor shapes, axis refusal, one resolved record.
- [03]-[LIFETIME_ADAPTERS]: Builder selection, lifetime delegates, `HostOptions` policy, and hook projection.
- [04]-[RESOURCE_IDENTITY]: Per-user roots including the durable queue root, and the resource triple behind one detector.
- [05]-[POWER_AND_FIDELITY]: Power-state and thermal-budget reads; energy-aware compute-fidelity scaling.

## [02]-[PROFILE_AXIS]

- Owner: `ProfileAxis` names the six-axis roster; `Tenancy`, `DeploymentTopology`, `LifecycleOwner`, and `Isolation` close their vocabularies; `HostCapability` is the host-integration capability vocabulary both descriptor families and every boot fold read through `CapabilitySet<HostCapability>`; `HostDescriptor` and `ProviderDescriptor` fix the two open axes' descriptor shape over `DescriptorLifetime`, the span-and-ender pair both families answer `lifetime` with; `ConsumptionProfile` carries the supplied row, `RecoveryObjective` its `(Rpo, Rto)` durability column, and `ResolvedProfile` the only profile artifact siblings consume.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `host` and `providers` carry descriptor rows this branch supplies through `HostRows` and `ProviderRows`, each row answering `Fits`, `Tenancy`, and `Lifetime` beside its family's extension columns — `ShipVehicle`, `HostAttach`, `HostSurface`, `RecoveryObjective`, and one `CapabilitySet<HostCapability>` for a host, `Supplies` and `Reach` for a provider; `HostCapability` = host-document | local-store | module-scan | single-instance | co-hosted-assets; `HostAttach` = Foreign | AppRoot | Quiet | Managed; `HostSurface` = Embedded | Windowed | Offscreen | None; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = AttachmentRejected | RootUnresolved | AxisUnsupported | NotifyRefused in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Admit` accumulates both axis refusals first, so `Fin` aborts carrying every unservable axis, then attachment rejection and root rejection sequence behind it.
- Auto: one supplied row replaces every bootstrap program — a host descriptor overrides its topology row's `Vehicle`, `Attach`, `Surface`, and `Durability` columns while an unhosted profile reads the topology row, so `ServerGc`, `ReadyToRun`, `OtlpExport`, and every `HostCapability` membership fold from axis values with no key roster between them; raw axis keys admit through each vocabulary's generated `Validate` against `ProfileFault`.
- Auto: `Canonical()` emits the six axis rows in roster order under an ordinal provider-key sort and `CanonicalJson()` renders them as the one UTF-8 `canonical-json` preimage — the byte-deriving input the `consumption-profile` corpus contract freezes, so the three branches diff one string rather than three rosters.
- Packages: Rasm, Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, Generator.Equals, NodaTime, BCL inbox
- Growth: one host integration is one `HostRows` descriptor row and one bound port is one `ProviderRows` row, each answering its family's whole coordinate set at zero new surface, so a row minted short of one refuses at its own factory; a new host capability is one `HostCapability` row every descriptor either holds or forfeits by silence; a new closed-axis value is one member on its owning vocabulary, and a new axis is one `ProfileAxis` row beside one `ConsumptionProfile` column, both settling at the corpus roster first.
- Boundary: every open-axis row answers the consumption-descriptor coordinates in this branch's casing — `Fits` the selection sentence a composition root picks the row on, `Tenancy` the MECHANISM the row separates tenants by and never a `Tenancy` roster value, `Lifetime` a survival span paired with the `LifecycleOwner` that ends it; a forfeit is the COMPLEMENT of the held set and stores nowhere, so `HostCapability.Items` minus `Held` answers it at any reader and a stored degradation column is a second copy of one fact; `Admit` rides each family's lead because every row in it answers alike — a host through `ProfileBoot.Boot` over its own `Attach` delegate pair, a provider through the `ConsumptionProfile.Providers` seat — and a residual states only what no capability row carries, which is why the provider family declares none and the host family spends one on `test-harness` alone; axis values stay data — a compile-time assumption, an ambient global, a build flag, and a package branching on which product hosts it are the four deleted forms, so a host integration lands as a descriptor row and never as a closed case; `Admit` refuses BOTH unservable axes in one pass with `ProfileFault.AxisUnsupported` carrying `AxisEvidence` that names the axis, because a composition root restating one axis only to be refused on the second is the sequenced form this accumulation deletes; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; `isolation` refuses where no bound provider supplies the crossing's `Faculty`; a profile stating no provider row reads the branch's own `ProviderRows.Default` as its bound set — an unstated providers axis means whatever this branch supplies, never nothing, so an app root NARROWS by naming rows and never widens past them, while the canonical render carries the SUPPLIED column so the three-branch preimage never forks on a default this branch seats; `RuntimeAttachment.Integrating` admits only where the resolved row holds `SingleInstance`, so a shared store root reaches exactly one live instance; `RecoveryObjective` is the branch's one DR-target declaration — `Rasm.Persistence` IMPORTS the type through the `Runtime ⇄ Rasm.Persistence/Version/recovery # [IMPORT]: RecoveryObjective` boundary and a composition root threads `ResolvedProfile.Recovery` in as the value, so a Persistence-local `(Rpo, Rto)` record and a host-band-keyed RPO/RTO table there are both the deleted form; `ResolvedProfile` itself never crosses, because a one-line accessor over `.Recovery` is a forwarding wrapper rather than a port; grading lives with the observation, so `Rasm.Persistence/Store/schema` `RecoveryWindow.Gauged` is the ONE gauge folding each `RecoveryAxis` row's measured half against the declared column, and a `Meets*` predicate on this struct is the deleted second grader blind to the unmeasured half; column values stay app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the `SingleInstance` value is probed through the discovery manifest, a `CoHostedAssets` host serves the built TS bundle same-origin from its app root through `UseStaticFiles(StaticFileOptions)` with `FileProvider`/`RequestPath` off the selected bundle root — `MapStaticAssets` is foreclosed because it resolves a BUILD-emitted static-web-asset manifest and this column selects its bundle at RUNTIME, so a tree the .NET build never enumerated is absent from that manifest and answers 404 — which makes the column's invariant a provider question rather than a build one: a row holding `CoHostedAssets` whose selected root resolves no readable directory is a boot-time refusal, never a per-request miss, with cross-origin headers held as designed growth; and the test-harness row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof.
- Boundary: `HostCapability` and `Faculty` are two vocabularies over one word and the discriminant is WHO decides — a `HostCapability` set is fixed by the integration and read once at boot, so a host either was built to back a document or was not, while a `Faculty` set is graded per health reading and narrows as a live process degrades; the two meet at exactly one point, where a `DocumentBridge` provider supplies `Faculty.HostDocument` on a host row that holds `HostCapability.Document`, and collapsing them lets a degradation reading retract a build-time fact.

Each `isolation` value names the crossing that answers it; an unbound capability refuses on the `isolation` axis rather than degrading to a weaker crossing:

| [INDEX] | [ISOLATION] | [CROSSING_OWNER]                   | [ADMISSION]                 |
| :-----: | :---------- | :--------------------------------- | :-------------------------- |
|  [01]   | `in-proc`   | `Runtime/laneguard#LANE_GUARD`     | always served               |
|  [02]   | `thread`    | `Runtime/laneguard#LANE_GUARD`     | always served               |
|  [03]   | `process`   | `Wire/companion#PROCESS_MODALITY`  | `Faculty.LocalCompute` row  |
|  [04]   | `wasm`      | `Sandbox/isolation#ISOLATION_AXIS` | `Faculty.LocalCompute` row  |
|  [05]   | `remote`    | `Wire/outbound#HOP_AXIS`           | `Faculty.RemoteCompute` row |

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using System.Text.Json;
using Generator.Equals;
using Thinktecture;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileAxis {
    public static readonly ProfileAxis Tenancy = new("tenancy");
    public static readonly ProfileAxis Topology = new("topology");
    public static readonly ProfileAxis Host = new("host");
    public static readonly ProfileAxis Lifecycle = new("lifecycle");
    public static readonly ProfileAxis Isolation = new("isolation");
    public static readonly ProfileAxis Providers = new("providers");
}

[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Tenancy {
    public static readonly Tenancy None = new("none");
    public static readonly Tenancy Single = new("single");
    public static readonly Tenancy Multi = new("multi");
}

[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LifecycleOwner {
    public static readonly LifecycleOwner CallerOwned = new("caller-owned");
    public static readonly LifecycleOwner PackageOwned = new("package-owned");
}

[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Isolation {
    public static readonly Isolation InProc = new("in-proc", needs: None);
    public static readonly Isolation Thread = new("thread", needs: None);
    public static readonly Isolation Process = new("process", needs: Some(Faculty.LocalCompute));
    public static readonly Isolation Wasm = new("wasm", needs: Some(Faculty.LocalCompute));
    public static readonly Isolation Remote = new("remote", needs: Some(Faculty.RemoteCompute));

    public Option<Faculty> Needs { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShipVehicle {
    public static readonly ShipVehicle Yak = new("yak", readyToRun: false);
    public static readonly ShipVehicle DesktopBundle = new("desktop-bundle", readyToRun: true);
    public static readonly ShipVehicle Oci = new("oci", readyToRun: false);
    public static readonly ShipVehicle Folder = new("folder", readyToRun: false);

    public bool ReadyToRun { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostSurface {
    public static readonly HostSurface Embedded = new("embedded");
    public static readonly HostSurface Windowed = new("windowed");
    public static readonly HostSurface Offscreen = new("offscreen");
    public static readonly HostSurface None = new("none");
}

[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeploymentTopology {
    public static readonly DeploymentTopology InHost = new("in-host", serverGc: false, vehicle: ShipVehicle.Yak, attach: HostAttach.AppRoot, surface: HostSurface.Windowed, durability: RecoveryObjective.Relaxed);
    public static readonly DeploymentTopology Sidecar = new("sidecar", serverGc: true, vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.Quiet, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard);
    public static readonly DeploymentTopology Companion = new("companion", serverGc: true, vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.Quiet, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard);
    public static readonly DeploymentTopology Service = new("service", serverGc: true, vehicle: ShipVehicle.Oci, attach: HostAttach.Managed, surface: HostSurface.None, durability: RecoveryObjective.Strict);
    public static readonly DeploymentTopology Edge = new("edge", serverGc: true, vehicle: ShipVehicle.Oci, attach: HostAttach.Managed, surface: HostSurface.None, durability: RecoveryObjective.Strict);
    public static readonly DeploymentTopology Cli = new("cli", serverGc: false, vehicle: ShipVehicle.Folder, attach: HostAttach.AppRoot, surface: HostSurface.None, durability: RecoveryObjective.Relaxed);

    public bool ServerGc { get; }
    public ShipVehicle Vehicle { get; }
    public HostAttach Attach { get; }
    public HostSurface Surface { get; }
    public RecoveryObjective Durability { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HostCapability : ICapability<HostCapability> {
    public static readonly HostCapability Document = new("host-document", rank: 0);
    public static readonly HostCapability LocalStore = new("local-store", rank: 1);
    public static readonly HostCapability ModuleScan = new("module-scan", rank: 2);
    public static readonly HostCapability SingleInstance = new("single-instance", rank: 3);
    public static readonly HostCapability CoHostedAssets = new("co-hosted-assets", rank: 4);

    public int Rank { get; }

    public static CapabilitySet<HostCapability> Unhosted => CapabilitySet<HostCapability>.Of(ModuleScan);

    static IReadOnlyList<HostCapability> ICapability<HostCapability>.Items => Items;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RuntimeAttachment {
    private RuntimeAttachment() { }
    public sealed record Isolated : RuntimeAttachment;
    public sealed record Integrating(string SharedStoreRoot) : RuntimeAttachment;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record AxisEvidence(ProfileAxis Axis, string Value, string Reason) {
    public string Detail => $"{Axis.Key}={Value}:{Reason}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Profile;
    private ProfileFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record AttachmentRejected : ProfileFault { public AttachmentRejected(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record RootUnresolved : ProfileFault { public RootUnresolved(string detail) : base(detail) { } }

    [FaultCase(2)]
    public sealed partial record AxisUnsupported : ProfileFault {
        public AxisUnsupported(AxisEvidence evidence) : base(evidence.Detail) => Evidence = evidence;

        public AxisEvidence Evidence { get; }
    }

    [FaultCase(3)]
    public sealed partial record NotifyRefused : ProfileFault, ICausedFault {
        public NotifyRefused(string state, Error cause) : base($"{state}:{cause.Message}") => Cause = cause;

        public Error Cause { get; }

    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RecoveryObjective(Duration Rpo, Duration Rto) {
    public static readonly RecoveryObjective Strict = new(Duration.FromMinutes(1), Duration.FromMinutes(15));
    public static readonly RecoveryObjective Standard = new(Duration.FromMinutes(5), Duration.FromMinutes(30));
    public static readonly RecoveryObjective Relaxed = new(Duration.FromMinutes(15), Duration.FromHours(1));
    public static readonly RecoveryObjective Instant = new(Duration.Zero, Duration.Zero);
}

public readonly record struct DescriptorLifetime(string Survives, LifecycleOwner Ender);

[ComplexValueObject]
[ValidationError]
public sealed partial class HostDescriptor {
    public string Key { get; }
    public string Fits { get; }
    public string Tenancy { get; }
    public DescriptorLifetime Lifetime { get; }
    public Option<string> Residual { get; }
    public ShipVehicle Vehicle { get; }
    public HostAttach Attach { get; }
    public HostSurface Surface { get; }
    public RecoveryObjective Durability { get; }
    public CapabilitySet<HostCapability> Held { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? error, ref string key, ref string fits, ref string tenancy, ref DescriptorLifetime lifetime,
        ref Option<string> residual, ref ShipVehicle vehicle, ref HostAttach attach, ref HostSurface surface,
        ref RecoveryObjective durability, ref CapabilitySet<HostCapability> held) =>
        error = Descriptors.Coordinates(nameof(HostDescriptor), key, fits, tenancy, lifetime);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ProviderDescriptor {
    public string Key { get; }
    public string Fits { get; }
    public string Tenancy { get; }
    public DescriptorLifetime Lifetime { get; }
    public Faculty Supplies { get; }
    public Isolation Reach { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? error, ref string key, ref string fits, ref string tenancy,
        ref DescriptorLifetime lifetime, ref Faculty supplies, ref Isolation reach) =>
        error = Descriptors.Coordinates(nameof(ProviderDescriptor), key, fits, tenancy, lifetime);
}

static class Descriptors {
    public static ValidationError? Coordinates(string family, string key, string fits, string tenancy, DescriptorLifetime lifetime) =>
        string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(fits)
        || string.IsNullOrWhiteSpace(tenancy) || string.IsNullOrWhiteSpace(lifetime.Survives)
            ? new ValidationError($"{family} requires key, fits, tenancy, and a lifetime span.")
            : null;
}

[Equatable]
public sealed partial record ConsumptionProfile(
    Tenancy Tenancy,
    DeploymentTopology Topology,
    LifecycleOwner Lifecycle,
    Isolation Isolation,
    [property: UnorderedEquality] Seq<ProviderDescriptor> Providers,
    Option<HostDescriptor> Host = default) {
    public Seq<ProviderDescriptor> Bound => Providers.IsEmpty ? ProviderRows.Default : Providers;
    public CapabilitySet<Faculty> Grants => CapabilitySet<Faculty>.Of([.. Bound.Map(static row => row.Supplies)]);
    public CapabilitySet<HostCapability> Held => Host.Map(static host => host.Held).IfNone(HostCapability.Unhosted);

    public ShipVehicle Vehicle => Host.Map(static host => host.Vehicle).IfNone(Topology.Vehicle);
    public HostAttach Attach => Host.Map(static host => host.Attach).IfNone(Topology.Attach);
    public HostSurface Surface => Host.Map(static host => host.Surface).IfNone(Topology.Surface);
    public RecoveryObjective Recovery => Host.Map(static host => host.Durability).IfNone(Topology.Durability);
    public bool ServerGc => Topology.ServerGc;
    public bool ReadyToRun => Vehicle.ReadyToRun;
    public bool OtlpExport => Supplies(Faculty.TelemetryExport);
    public string HostKey => Host.Map(static host => host.Key).IfNone("none");

    public bool Holds(HostCapability capability) => Held.Admits(capability);
    public bool Supplies(Faculty faculty) => Grants.Admits(faculty);

    public ImmutableArray<KeyValuePair<string, string>> Canonical() => [
        new(ProfileAxis.Tenancy.Key, Tenancy.Key),
        new(ProfileAxis.Topology.Key, Topology.Key),
        new(ProfileAxis.Host.Key, HostKey),
        new(ProfileAxis.Lifecycle.Key, Lifecycle.Key),
        new(ProfileAxis.Isolation.Key, Isolation.Key),
        new(ProfileAxis.Providers.Key, string.Join(',', Providers.Map(static row => row.Key).Order(StringComparer.Ordinal))),
    ];

    public string CanonicalJson() =>
        $"{{{string.Join(',', Canonical().Select(static row =>
            $"\"{JsonEncodedText.Encode(row.Key).Value}\":\"{JsonEncodedText.Encode(row.Value).Value}\""))}}}";
}

public sealed record ResolvedProfile(ConsumptionProfile Profile, string ApplicationName, string EnvironmentName, string ContentRoot, string ServiceVersion, ProfileRoots Roots, Option<RuntimeAttachment> Attachment, int ProcessId, Instant StartInstant) {
    public RecoveryObjective Recovery => Profile.Recovery;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class HostRows {
    public static readonly HostDescriptor Rhino = HostDescriptor.Create(
        key: "rhino",
        fits: "a Rhino instance loads this assembly through its own plug-in loader",
        tenancy: "one document session per process, scoped by the launching user's profile root",
        lifetime: new("until the host unloads the plug-in load context", LifecycleOwner.CallerOwned),
        residual: None,
        vehicle: ShipVehicle.Yak, attach: HostAttach.Foreign, surface: HostSurface.Embedded, durability: RecoveryObjective.Relaxed,
        held: CapabilitySet<HostCapability>.Of(HostCapability.Document, HostCapability.LocalStore, HostCapability.ModuleScan));

    public static readonly HostDescriptor Gh2 = HostDescriptor.Create(
        key: "gh2",
        fits: "a Grasshopper2 editor loads this assembly beside its own solution graph",
        tenancy: "one document session per process, scoped by the launching user's profile root",
        lifetime: new("until the host unloads the plug-in load context", LifecycleOwner.CallerOwned),
        residual: None,
        vehicle: ShipVehicle.Yak, attach: HostAttach.Foreign, surface: HostSurface.Embedded, durability: RecoveryObjective.Relaxed,
        held: CapabilitySet<HostCapability>.Of(HostCapability.Document, HostCapability.LocalStore, HostCapability.ModuleScan));

    public static readonly HostDescriptor DesktopShell = HostDescriptor.Create(
        key: "desktop-shell",
        fits: "this solution ships and launches as its own desktop bundle",
        tenancy: "one live instance per user, held by the single-instance discovery manifest",
        lifetime: new("until the launched process exits", LifecycleOwner.PackageOwned),
        residual: None,
        vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.AppRoot, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard,
        held: CapabilitySet<HostCapability>.Of(HostCapability.LocalStore, HostCapability.ModuleScan, HostCapability.SingleInstance));

    public static readonly HostDescriptor WebAppRoot = HostDescriptor.Create(
        key: "web-app-root",
        fits: "an application root composes this solution and serves the built bundle same-origin",
        tenancy: "a request-scoped TenantContext adopted per ingress carrier",
        lifetime: new("until the application root stops the Generic Host", LifecycleOwner.CallerOwned),
        residual: None,
        vehicle: ShipVehicle.Oci, attach: HostAttach.AppRoot, surface: HostSurface.None, durability: RecoveryObjective.Strict,
        held: CapabilitySet<HostCapability>.Of(HostCapability.CoHostedAssets));

    public static readonly HostDescriptor TestHarness = HostDescriptor.Create(
        key: "test-harness",
        fits: "a test assembly composes this solution against fake time and in-memory configuration",
        tenancy: "one composition per test scope, sharing no root across scopes",
        lifetime: new("until the fixture disposes the composition", LifecycleOwner.CallerOwned),
        residual: Some("wall-clock progression, since FakeTimeProvider and FakeClock advance only where a test drives them"),
        vehicle: ShipVehicle.Folder, attach: HostAttach.AppRoot, surface: HostSurface.Offscreen, durability: RecoveryObjective.Instant,
        held: CapabilitySet<HostCapability>.Of(HostCapability.ModuleScan));
}

public static class ProviderRows {
    public static readonly ProviderDescriptor OtlpCollector = ProviderDescriptor.Create(
        key: "otlp-collector",
        fits: "a deployment exports the four signals to a collector endpoint",
        tenancy: "resource attributes stamp the emitter, and the queue root scopes by host key",
        lifetime: new("until the durable queue drains its last batch", LifecycleOwner.PackageOwned),
        supplies: Faculty.TelemetryExport, reach: Isolation.Remote);

    public static readonly ProviderDescriptor RemoteSolver = ProviderDescriptor.Create(
        key: "remote-solver",
        fits: "solve capacity lives off-box behind the outbound hop",
        tenancy: "a per-tenant Budget debits at the brokered mediation gate",
        lifetime: new("until the calling scope cancels or the hop deadline elapses", LifecycleOwner.CallerOwned),
        supplies: Faculty.RemoteCompute, reach: Isolation.Remote);

    public static readonly ProviderDescriptor LocalSolver = ProviderDescriptor.Create(
        key: "local-solver",
        fits: "solve capacity runs on this machine inside a spawned child",
        tenancy: "one grant scope per spawned child, holding no ambient host authority",
        lifetime: new("until the sandbox kill path drains the child", LifecycleOwner.PackageOwned),
        supplies: Faculty.LocalCompute, reach: Isolation.Process);

    public static readonly ProviderDescriptor DocumentBridge = ProviderDescriptor.Create(
        key: "document-bridge",
        fits: "a live host document backs this solution's reads and writes",
        tenancy: "one host document every caller in the process shares",
        lifetime: new("until the host closes the document", LifecycleOwner.CallerOwned),
        supplies: Faculty.HostDocument, reach: Isolation.InProc);

    public static readonly ProviderDescriptor StoreReader = ProviderDescriptor.Create(
        key: "store-reader",
        fits: "a resolved local store root answers reads",
        tenancy: "one per-user store root the resolved profile fixed",
        lifetime: new("until the composition root disposes the reader", LifecycleOwner.PackageOwned),
        supplies: Faculty.StoreRead, reach: Isolation.InProc);

    public static readonly ProviderDescriptor StoreWriter = ProviderDescriptor.Create(
        key: "store-writer",
        fits: "a resolved local store root accepts writes",
        tenancy: "one per-user store root the resolved profile fixed",
        lifetime: new("until the composition root disposes the writer", LifecycleOwner.PackageOwned),
        supplies: Faculty.StoreWrite, reach: Isolation.InProc);

    public static Seq<ProviderDescriptor> Default =>
        Seq(OtlpCollector, RemoteSolver, LocalSolver, DocumentBridge, StoreReader, StoreWriter);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProfileSurface {
    public static Validation<Error, ConsumptionProfile> Admit(ConsumptionProfile profile) =>
        (Hosted(profile), Crossing(profile)).Apply(static (_, _) => unit).Map(_ => profile).As();

    public static Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default) =>
        from row in Admit(profile).Match(
            Succ: Fin.Succ,
            Fail: static faults => Fin.Fail<ConsumptionProfile>(Error.Many(faults.Map(static fault => (Error)fault).ToSeq())))
        from admitted in attachment.IsSome && !row.Holds(HostCapability.SingleInstance)
            ? Fin.Fail<Option<RuntimeAttachment>>(new ProfileFault.AttachmentRejected(row.HostKey))
            : Fin.Succ(attachment)
        from roots in ProfileIdentity.Roots(row, applicationName, admitted)
        select new ResolvedProfile(row, applicationName, environmentName, contentRoot, serviceVersion, roots, admitted, Environment.ProcessId, clock.GetCurrentInstant());

    static Validation<Error, Unit> Hosted(ConsumptionProfile profile) =>
        profile.Topology != DeploymentTopology.InHost || profile.Host.IsSome
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new ProfileFault.AxisUnsupported(
                new AxisEvidence(ProfileAxis.Host, "none", "in-host topology carries no host descriptor row")));

    static Validation<Error, Unit> Crossing(ConsumptionProfile profile) =>
        profile.Isolation.Needs.Filter(needed => !profile.Supplies(needed)).Match(
            None: static () => Validation<Error, Unit>.Success(unit),
            Some: needed => Validation<Error, Unit>.Fail(new ProfileFault.AxisUnsupported(
                new AxisEvidence(ProfileAxis.Isolation, profile.Isolation.Key, needed.Key))));
}
```

## [03]-[LIFETIME_ADAPTERS]

- Owner: `ProfileBoot` — builder selection, lifetime-adapter delegate rows, and `HostOptions` policy as one fold; `BootVariable` the environment coordinates that resolve before any configuration source mounts; `WatchdogEnrollment` the service-manager keep-alive registration.
- Entry: `IHostApplicationBuilder Boot(ResolvedProfile resolved, Duration startupDeadline, Duration shutdownDeadline, Option<IHostApplicationBuilder> external = default)` — total over every row; both deadline values arrive from the deadline vocabulary.
- Auto: Boot composes the resolved `HostAttach` row's `CreateBuilder` and `AttachLifetime` delegates with `HostOptions` — startup and shutdown timeouts, concurrent start and stop, `BackgroundServiceExceptionBehavior.StopHost` — deleting per-host bootstrap programs; the `Managed` row registers `AddSystemd` for the Linux-server backend, and `MirrorService` is a hook tap on the shielded phase point so every committed transition fires its service-state mirror through one subscriber seat, never a per-callsite emission; `Enrolled` derives the keep-alive period from the manager's own deadline and the schedule-port heartbeat row runs `Watchdog` on it, never a second timer; `Reloaded` brackets a reload fold in the manager's reload window; `Aborted` flattens a `HostAbortedException` into the boot-fault trigger value with no second state machine.
- Packages: Rasm, Microsoft.Extensions.Hosting, Microsoft.Extensions.Hosting.Systemd, Microsoft.Extensions.Options, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, BCL inbox
- Auto: `ServiceNotify` projects each `RuntimePhase` through the roster's own generated total `Map` to its `ServiceState` sd_notify mirror, so a new phase row breaks this projection at compile time rather than falling silently through a ternary tail; `Watchdog` emits the `WatchdogPing` keep-alive payload and `Reloaded` the `RELOADING=1`/`STATUS=`/`READY=1` window on the same `ISystemdNotifier`; every emission returns `Fin<Unit>` carrying `ProfileFault.NotifyRefused`, so a torn-down notify socket lands as a banded refusal on the caller's result rather than an exception crossing a phase commit.
- Growth: one `HostAttach` row — a key beside two static delegate targets bound through the row constructor — extends the lifetime surface with zero new surface; one phase-to-state mirror is one arm on the generated `Map`; a new sd_notify assertion is one payload mint through the `ServiceState(string)` ctor, never a package change, because the package names `Ready` and `Stopping` alone; one further pre-mount environment coordinate is one `BootVariable` row.
- Boundary: a host holding `CoHostedAssets` crosses in through `external` — its builder is constructed at the web app root, where ASP.NET Core enters as a shared-framework asset only, and the static-file middleware seats there under the capability's gate ahead of endpoint routing; the host registers `ConsoleLifetime` as the default `IHostLifetime` on every builder path including the empty builder, and that default is the ALC-COLLECTIBILITY blocker rather than a console nuisance — `ConsoleLifetime` holds three `PosixSignalRegistration` values (SIGINT, SIGQUIT, SIGTERM), each handing a delegate to a process-global native handler table that roots the lifetime, its service provider, and the whole plugin load context, and its `StopAsync` releases none of them because only `Dispose` unregisters — so a `Foreign` attach swaps in the no-op `DetachedLifetime` through `Detached` to keep those roots unplanted and host-attach trigger injection drives phases; teardown is terminal at `await ((IAsyncDisposable)host).DisposeAsync()`, which also drains the `BackgroundService` tasks the synchronous `Dispose` never awaits, and `await host.StopAsync()` alone releases none of the registrations, since `ConsoleLifetime.StopAsync` is documented to do nothing and only `Dispose` unregisters — the two facts are INDEPENDENT and neither substitutes for the other: the lifetime swap keeps the process-global roots unplanted, so a detached host's load context collects even undisposed, while disposal releases whatever a composition root planted anyway; `AddSystemd` is the one service-manager registration — `SystemdHelpers.IsSystemdService` gates the live `ISystemdNotifier.Notify` emission so the notify socket is written only under systemd on the Linux-server backend; `MirrorService` is a `HookTap` scoped to `AppHostPoint.Phase` that the composition root mounts for the `Managed` row, so `Emit` fires on every committed `PhaseCommit` — `ServiceState.Ready` mirrors the ready transition and `ServiceState.Stopping` mirrors the draining transition, the two payloads the package names — and the dispatcher's own isolation parks a dead socket as evidence instead of unwinding through the CAS commit; the service-manager liveness keep-alive rides the schedule-port heartbeat row through `Watchdog` writing the `WatchdogPing` payload, its PERIOD derived by `Enrolled` as half the manager's `WATCHDOG_USEC` deadline under the unset-or-equal `WATCHDOG_PID` guard, and an absent `WATCHDOG_USEC` registers no heartbeat row at all — the manager expects no keep-alive there, and a fixed fallback period is the fabricated-measurement form; the miss half is NOT re-derived here, because `Runtime/time#SCHEDULE_PORT` `Heartbeat` already folds a not-met `GaugedSpan<DeadlineClass>` into `SupportTrigger.Timed` under the watchdog kind carrying the firing row, so the enrollment answers only what that fold cannot — the dump completeness a hang deserves, which is why `DumpPolicy.Escalated` rides this row alone while every other trigger captures a process still answering its own probes; the watchdog carries a UNIT-side obligation the fence states because the default is a trap — systemd's `WatchdogSignal=` defaults to SIGABRT, which the CoreCLR PAL fully absorbs, so a missed deadline hangs the unit in `deactivating` for the whole `TimeoutStopSec` before the SIGKILL fallback (`Result=watchdog`, witnessed), and the unit therefore declares `WatchdogSignal=SIGKILL` or an explicit SIGABRT disposition so a missed deadline kills promptly; the reload window is `Reloaded`, whose `RELOADING=1` — carrying the mandatory `MONOTONIC_USEC` stamp a bare assertion cannot omit — opens and re-sent `READY=1` closes the `Type=notify-reload` handshake the unit declares beside `ReloadSignal=` (default SIGHUP), so `ExecReload=kill -HUP $MAINPID` is the deleted unit form — asynchronous, unorderable, and carrying no completion notification — while launchd publishes NO reload facility of any kind and its macOS trigger is the operator command `launchctl kill SIGHUP <domain>/<label>`, no plist key declaring it; `HostAbortedException` during build projects through `Aborted` to a boot-fault trigger value consumed by the transition entrypoint, never a second state machine.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting.Systemd;
using Thinktecture;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BootVariable {
    public static readonly BootVariable QueueRoot = new(ConfigSource.EnvPrefix + "TELEMETRY_QUEUE_ROOT");
    public static readonly BootVariable WatchdogDeadline = new("WATCHDOG_USEC");
    public static readonly BootVariable WatchdogOwner = new("WATCHDOG_PID");
    public static readonly BootVariable ListenOwner = new("LISTEN_PID");
    public static readonly BootVariable ListenCount = new("LISTEN_FDS");
    public static readonly BootVariable ListenNames = new("LISTEN_FDNAMES");

    public Option<string> Read() => Optional(Environment.GetEnvironmentVariable(Key)).Filter(static held => held.Length > 0);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WatchdogEnrollment(Duration Period, DumpPolicy Stalled);

// --- [COMPOSITION] ---------------------------------------------------------------------
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

    public static ServiceState Reloading(ClockPolicy clocks) =>
        new($"RELOADING=1\nMONOTONIC_USEC={(clocks.Time.GetTimestamp() * 1_000_000L / clocks.Time.TimestampFrequency).ToString(CultureInfo.InvariantCulture)}");

    public static ServiceState Status(string text) => new($"STATUS={text}");

    public static Option<ServiceState> ServiceNotify(RuntimePhase phase) => phase.Map(
        boot: Option<ServiceState>.None,
        ready: Some(ServiceState.Ready),
        running: Option<ServiceState>.None,
        degraded: Option<ServiceState>.None,
        draining: Some(ServiceState.Stopping),
        unloaded: Option<ServiceState>.None,
        faulted: Option<ServiceState>.None,
        supportCapture: Option<ServiceState>.None);

    public static Fin<Unit> Notify(ISystemdNotifier notifier, ServiceState state) =>
        notifier.IsEnabled
            ? Op.Of().Catch(() => Fin.Succ(fun(() => notifier.Notify(state))()))
                .MapFail(cause => new ProfileFault.NotifyRefused(state.ToString(), cause))
            : Fin.Succ(unit);

    public static Fin<Unit> Emit(ISystemdNotifier notifier, RuntimePhase phase) =>
        ServiceNotify(phase).Match(
            Some: state => Notify(notifier, state),
            None: static () => Fin.Succ(unit));

    public static Fin<Unit> Watchdog(ISystemdNotifier notifier) => Notify(notifier, WatchdogPing);

    public static Option<WatchdogEnrollment> Enrolled() =>
        BootVariable.WatchdogDeadline.Read()
            .Filter(static _ => BootVariable.WatchdogOwner.Read().Match(
                Some: static owner => int.TryParse(owner, CultureInfo.InvariantCulture, out var pid) && pid == Environment.ProcessId,
                None: static () => true))
            .Bind(static declared => long.TryParse(declared, CultureInfo.InvariantCulture, out var usec) && usec > 0L
                ? Some(new WatchdogEnrollment(Duration.FromNanoseconds(usec * 500L), DumpPolicy.Escalated))
                : None);

    public static Fin<ReloadOutcome> Reloaded(ISystemdNotifier notifier, ClockPolicy clocks, Func<Fin<ReloadOutcome>> reload) =>
        from opened in Notify(notifier, Reloading(clocks))
        from outcome in reload()
        from stated in Notify(notifier, Status($"reload:{outcome.Key}"))
        from closed in Notify(notifier, ServiceState.Ready)
        select outcome;

    public static HookTap<AppHostPoint, AppHostFact, TelemetrySource> MirrorService(ISystemdNotifier notifier) =>
        new(Name: Op.Of(nameof(MirrorService)),
            Observe: fact => fact.Switch(
                phase: row => Emit(notifier, row.Commit.To),
                command: static _ => Fin.Succ(unit),
                outcome: static _ => Fin.Succ(unit),
                delivery: static _ => Fin.Succ(unit),
                degradation: static _ => Fin.Succ(unit),
                alert: static _ => Fin.Succ(unit),
                binding: static _ => Fin.Succ(unit),
                profile: static _ => Fin.Succ(unit),
                coordination: static _ => Fin.Succ(unit),
                companion: static _ => Fin.Succ(unit)),
            Scope: Some(Seq(AppHostPoint.Phase)),
            Owner: Some(TelemetrySource.AppHost));

    public static PhaseTrigger Aborted(HostAbortedException abort) =>
        new PhaseTrigger.FaultCommitted(new FaultSource.Unhandled(
            FaultWire.Observe(Error.New(abort.Message, (Exception)abort)), TerminationKind.Terminating));

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

| [INDEX] | [SIGNAL]                                    | [PROJECTION]                                         |
| :-----: | :------------------------------------------ | :--------------------------------------------------- |
|  [01]   | `IHostedLifecycleService.StartingAsync`     | boot                                                 |
|  [02]   | `IHostedLifecycleService.StartedAsync`      | ready                                                |
|  [03]   | `IHostApplicationLifetime` started token    | running                                              |
|  [04]   | `IHostedLifecycleService.StoppingAsync`     | draining                                             |
|  [05]   | `IHostApplicationLifetime` stopping token   | draining                                             |
|  [06]   | `IHostedLifecycleService.StoppedAsync`      | unloaded                                             |
|  [07]   | `IHostApplicationLifetime` stopped token    | unloaded                                             |
|  [08]   | `HostAbortedException` during build         | faulted                                              |
|  [09]   | `ServiceState.Ready` via `ServiceNotify`    | sd_notify mirror of the ready commit                 |
|  [10]   | `ServiceState.Stopping` via `ServiceNotify` | sd_notify mirror of the draining commit              |
|  [11]   | `WatchdogPing` via `Watchdog`               | keep-alive at the `Enrolled` half-deadline           |
|  [12]   | `Reloading`/`Status` via `Reloaded`         | reload window around one `ReloadOutcome`, then ready |

## [04]-[RESOURCE_IDENTITY]

- Owner: `ProfileIdentity` — per-user root computation and the telemetry resource triple; `ProfileRoots` is the path artifact carried inside the resolved record, splitting the data base from the config base and carrying the durable OTLP queue root beside the store and support roots; `HostResourceDetector` the one `IResourceDetector` carrying both the resolved record and its composition-supplied extra rows.
- Entry: `ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` — pure projection over the resolved record; `string InstanceId(ResolvedProfile resolved)` the one per-process instance spelling the resource row and the boot log enricher share; `new HostResourceDetector(resolved, extra)` is the detector `Observability/telemetry#SIGNAL_GOVERNANCE` `ResourceIdentity.Compose` seats BEHIND the enriching contrib chain and AHEAD of the deployment-override detector.
- Auto: identity derives from the resolved record before any provider construction, and the detector's `Detect` returns that projection through `new Resource(IEnumerable<KeyValuePair<string, object>>)`, so `ConfigureResource` admits ONE resource feed and a per-call attribute push at each provider is the deleted form; SEAT ORDER is the whole precedence law and the only one: the builder folds every seated source left to right through `Resource.Merge`, which awards each colliding key to the incoming resource with no distinction between an attribute list and a detector, so the enriching host, os, process, runtime, and container detectors seat FIRST and lose every collision with the mint, the mint seats next, and the deploy-plane environment-variable detector tails and outranks all of it; the triple assembles from the `TelemetryDomain` namespace const and the resolved record alone, so a branch-wide namespace rename moves every resource, instrument, and dimension together; rasm-owned resource dimensions read their `TelemetryDomain` row rather than a literal, so each one resolves the roster the conformance gate proves against; the queue root folds the deploy-declared durable volume ahead of the local-disk evidence, so a containerized service arms its offline queue on the path a deployment mounted while a desktop host arms on its own base and a host owning neither opens none, and store placement and queue placement stay two answers on every arm — a companion scopes both under its own segment, an integrating instance keeps its queue off the shared store root it attached to, and every queue scopes by host key so two co-resident processes under one mount stay apart.
- Packages: Rasm, OpenTelemetry, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one attribute row or one root policy value per new identity fact, or one sibling `IResourceDetector` composed through `ResourceIdentity.Compose`; zero new surface.
- Boundary: roots are per-user paths off TWO platform bases — `LocalApplicationData` carries the store, support, and queue roots because those are data, and `ApplicationData` carries the config root alone, since the two collapse on darwin but diverge on linux (`$XDG_DATA_HOME` versus `$XDG_CONFIG_HOME`) and roam versus stay local on windows, so a single base lands a document store, a crash marker, and a durable queue in a CONFIG directory on exactly the service and edge rows that only ever run on linux; a host holding `LocalStore` stores under the data base, companion topology scopes its own companion store, and every other row runs scratch-only; Persistence consumes the resolved record and derives no path; host-document identity enters as one extra attribute row where the descriptor holds `HostCapability.Document`; the resource triple is `service.namespace` `rasm`, `service.name` the `TelemetryDomain.Qualify` render of the application row, and `service.instance.id` as pid joined with the start instant — the qualified name is load-bearing because a metrics store maps a subset of resource attributes onto series labels, so a store dropping `service.namespace` still separates this deployment's emitters from a foreign `service.name`, and the qualifier rather than a local concatenation owns it so an already-prefixed or PascalCase application id lands one dotted lowercase spelling instead of two; `deployment.environment.name` is the live semconv spelling and the bare `deployment.environment` key is the deprecated form no exporter re-introduces; `QueueRoot` is the ONLY durable-telemetry path any composition reads — an offline queue rooted at a container layer loses its tail on the next reschedule, a queue rooted at a shared store root corrupts on a second live instance, and a queue rooted at a base two co-resident processes share lets each drain the other's batches, so every arm answers placement here rather than at a consumer and `BootVariable.QueueRoot` is the one coordinate a deployment sets to declare the volume that survives it; deriving queue placement from `LocalStore` alone is the deleted form, because that capability answers where a document store lives and disarms durable buffering on exactly the service and edge rows that always export; `HostResourceDetector` is the one resource-discovery boundary and a hand-pushed attribute list at a provider builder is the deleted pattern, its `Admitted` narrowing scoped to the ONE collision seat order cannot answer — two rows inside a single detector's own attribute list, where no merge runs — because no pre-build narrowing defends a key the merge fold itself overwrites afterwards, which is exactly the case the prior whole-list scan missed.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using NodaTime.Text;
using OpenTelemetry.Resources;

namespace Rasm.AppHost.Runtime;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ProfileRoots(string AppRoot, string ConfigRoot, Option<string> StoreRoot, string SupportRoot, Option<string> QueueRoot);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProfileIdentity {
    public static Fin<ProfileRoots> Roots(ConsumptionProfile profile, string applicationName, Option<RuntimeAttachment> attachment) =>
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) switch {
            ({ Length: > 0 } data, { Length: > 0 } config) =>
                Fin.Succ(Folded(profile, Path.Join(data, applicationName), Path.Join(config, applicationName), attachment)),
            ({ Length: > 0 }, _) =>
                Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.ApplicationData))),
            _ => Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.LocalApplicationData))),
        };

    public static string InstanceId(ResolvedProfile resolved) =>
        $"{resolved.ProcessId}:{InstantPattern.ExtendedIso.Format(resolved.StartInstant)}";

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

    static ImmutableArray<KeyValuePair<string, object>> Admitted(
        ImmutableArray<KeyValuePair<string, object>> minted, ImmutableArray<KeyValuePair<string, object>> extra) =>
        minted.AddRange(extra.ExceptBy(minted.Select(static held => held.Key), static row => row.Key, StringComparer.Ordinal));

    public sealed record HostResourceDetector(ResolvedProfile Resolved, ImmutableArray<KeyValuePair<string, object>> Extra) : IResourceDetector {
        public Resource Detect() => new(ResourceAttributes(Resolved, Extra.AsSpan()));
    }

    static ProfileRoots Folded(ConsumptionProfile profile, string baseRoot, string configRoot, Option<string> attachment) =>
        (profile.Topology == DeploymentTopology.Companion, profile.Holds(HostCapability.LocalStore), Shared(attachment)) switch {
            (true, _, _) => Rooted(profile, baseRoot, configRoot, Some(Path.Join(baseRoot, "companion")), Some(Path.Join(baseRoot, "companion"))),
            (_, true, { IsSome: true } shared) => Rooted(profile, baseRoot, configRoot, shared, Some(baseRoot)),
            (_, true, _) => Rooted(profile, baseRoot, configRoot, Some(Path.Join(baseRoot, "store")), Some(baseRoot)),
            _ => Rooted(profile, baseRoot, configRoot, None, None),
        };

    static Option<string> Shared(Option<RuntimeAttachment> attachment) =>
        attachment.Bind(static held => held.Switch(
            isolated: static _ => Option<string>.None,
            integrating: static link => Some(link.SharedStoreRoot)));

    static ProfileRoots Rooted(ConsumptionProfile profile, string baseRoot, string configRoot, Option<string> store, Option<string> local) =>
        new(baseRoot, configRoot, store, Path.Join(baseRoot, "support"),
            (BootVariable.QueueRoot.Read() | local).Map(root => Path.Join(root, "otlp", profile.HostKey)));
}
```

## [05]-[POWER_AND_FIDELITY]

- Owner: `PowerState` `[SmartEnum<string>]` the host power-source axis; `ThermalPressure` `[SmartEnum<int>]` the thermal-budget ladder whose generated key IS the rank; `PowerReading` the probed triple carrying thermal as an OPTIONAL half; `PowerAuthority` `[SmartEnum<string>]` the platform row owning the read; `FidelityScale` `[SmartEnum<int>]` the compute-fidelity ladder keyed by its own tier and graded from one reading; `EnergyCell` the atom-backed capsule holding the last ADMITTED reading; `PowerProbe` the delegate targets the authority rows bind, holding each platform's key spellings as named consts.
- Cases: 3 power rows — plugged, battery, low-battery; 4 thermal rows — nominal(0), fair(1), serious(2), critical(3); 4 authority rows — `Darwin` over IOKit power sources and `NSProcessInfo.thermalState`, `Windows` over `GetSystemPowerStatus` with NO thermal answer, `Linux` over the power-supply and thermal sysfs classes, `Absent` for every remaining platform; 4 `FidelityScale` grades spanning burst(3) through conserve(0).
- Entry: `PowerAuthority.Platform` selects the row the running platform owns and `Read()` returns `Fin<PowerReading>`; `PowerReading.Of(PowerState, Option<ThermalPressure>, double)` returns `Fin<PowerReading>` — the one construction route, admitting the charge fraction finite and inside `[0, 1]` so no platform read's raw double reaches a ceiling comparison; `FidelityScale.Grade(PowerReading)` is the total projection into the row the compute scheduler and the health pressure axis both read; `EnergyCell.Refresh()` re-probes and returns the `Transition` verdict, so the health `Gauge` probe is the one sampling site and a refused probe is a case a caller reads rather than a silence.
- Auto: a plugged host at nominal thermal pressure grades to the full burst row; a low-battery or critical-thermal host grades to the sustained row that caps parallelism and lowers the fidelity tier so the device stays within its energy and thermal budget; a reading whose authority measures power but publishes no thermal grades on the power arms alone rather than on a manufactured `Nominal`; a refused read holds the prior reading, and a cell that never admitted one grades `Balanced` — bursting on absent evidence is the fabricated full-charge grade the authority rows exist to refuse; the graded row feeds `Observability/health#HEALTH_FOLD`'s `PressureAxis.Fidelity` as one input beside CPU and memory, so a thermally-throttled host degrades through the existing degradation ladder, never a parallel power alarm.
- Auto: the graded `FidelityScale` row IS the evidence — `PressureAxis.Fidelity` reads its tier against a `Band` derived from this roster's own rows, and the degradation reading that fold produces already crosses `AppHostPoint.Degradation` and writes `AppHostMeasure.HealthLevel`, so a second spine event for one transition gives one measurement two publishers.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one power row absorbs a new power source; one thermal row absorbs a new pressure level; one `PowerAuthority` row with its `PowerProbe` target absorbs a new platform authority; a new fidelity row is one `FidelityScale` member carrying its cap and tier, never a parallel scaling owner; zero new surface.
- Boundary: the power-and-fidelity fold is the only energy-awareness owner — a per-solve battery check, an ad hoc thermal poll, and a parallel power monitor are the deleted forms; the fidelity row is data the Compute scheduler reads to bound its `CpuBudget` and lane parallelism, so the host owns the power-state truth and the compute scheduler consumes the graded row, never re-reading the power state; `FidelityScale` is a keyed roster rather than four minted records because its four members are a closed ladder whose key IS the tier every consumer compares on, so a ceiling stated as a tier difference reads off the roster and a fifth grade lands as one row; the SUSTAINED flag deletes with that key — it was a stored restatement of "this row sits at or below the sustained tier", derivable at any reader and a second answer the moment a tier moves; platform variance rides the `PowerAuthority` roster rather than a runtime `if` inside the probe, and a row whose read has not landed REFUSES — a synthesized plugged-at-nominal-at-full-charge triple is indistinguishable from a measured one at every consumer, which is why absence crosses as a typed refusal the cell holds against; THERMAL absence is a second axis of the same law and the reason the column is optional — windows publishes no user-mode thermal-pressure surface at all (WMI leaves `Win32_TemperatureProbe.CurrentReading` unpopulated by documented design, `MSAcpi_ThermalZoneTemperature` is an ACPI-driver class reporting a motherboard zone in tenths of Kelvin where the platform exposes one, `IOCTL_THERMAL_QUERY_INFORMATION` is a kernel DDI, and `EFFECTIVE_POWER_MODE` is a power-policy ladder that never escalates on heat), and a linux thermal zone publishing `temp` without trip points yields no ladder, so both refuse the half rather than grade `Nominal`; every native read is a hand-declared interop or file read — no managed package reports AC, battery, or thermal state on any RID, `Microsoft.Extensions.Diagnostics.ResourceMonitoring` owns process, container, disk, and network utilization and none of this, and macOS publishes no SMC surface so `NSProcessInfo.thermalState` IS the sanctioned darwin ladder; the capsule holds one atom and a `MeterListener` seat beside it is dead apparatus, because power and thermal state reach the process by native probe alone and publish no meter — the `UtilizationCell` listener is the resource-monitoring path and this cell never twins it; thermal grades INSIDE the fidelity fold and nowhere else, so the capsule publishes no thermal accessor of its own — a rank read beside the graded row is the second grader the branch RULINGS `[02]` thermal-and-power clause names, and the one health axis reads the row that already folded heat ahead of power.
- Boundary: the darwin adapter admits a CoreFoundation GET through one `Handle` before any read, so the three-way `IntPtr.Zero` ladder the four value readers each carried collapses to one admission whose absence the `Option` carries; the COPY handles the adapter owns stay raw inside the release-bounded scan, because a lazily projected sequence escaping that scope reads memory the `finally` already freed.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Thinktecture;

namespace Rasm.AppHost.Runtime;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PowerState {
    public static readonly PowerState Plugged = new("plugged");
    public static readonly PowerState Battery = new("battery");
    public static readonly PowerState LowBattery = new("low-battery");
}

[SmartEnum<int>(
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.Default<int>, int>]
[KeyMemberComparer<ComparerAccessors.Default<int>, int>]
public sealed partial class ThermalPressure {
    public static readonly ThermalPressure Nominal = new(0);
    public static readonly ThermalPressure Fair = new(1);
    public static readonly ThermalPressure Serious = new(2);
    public static readonly ThermalPressure Critical = new(3);
}

[SmartEnum<int>(
    KeyMemberName = "FidelityTier",
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.Default<int>, int>]
[KeyMemberComparer<ComparerAccessors.Default<int>, int>]
public sealed partial class FidelityScale {
    public const double BatteryReserve = 0.2d;

    public static readonly FidelityScale Conserve = new(0, parallelismCap: 1);
    public static readonly FidelityScale Sustained = new(1, parallelismCap: int.Max(1, Environment.ProcessorCount / 2));
    public static readonly FidelityScale Balanced = new(2, parallelismCap: Environment.ProcessorCount);
    public static readonly FidelityScale Burst = new(3, parallelismCap: int.MaxValue);

    public int ParallelismCap { get; }

    public static FidelityScale Grade(PowerReading reading) =>
        (reading.Thermal.Case, reading.Power, reading.BatteryFraction) switch {
            (ThermalPressure heat, _, _) when heat >= ThermalPressure.Critical => Conserve,
            (ThermalPressure heat, _, _) when heat >= ThermalPressure.Serious => Sustained,
            (_, var power, _) when power == PowerState.LowBattery => Sustained,
            (_, var power, < BatteryReserve) when power == PowerState.Battery => Sustained,
            (_, var power, _) when power == PowerState.Battery => Balanced,
            _ => Burst,
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PowerAuthority {
    public static readonly PowerAuthority Darwin = new("darwin", read: PowerProbe.Darwin);
    public static readonly PowerAuthority Windows = new("windows", read: PowerProbe.Windows);
    public static readonly PowerAuthority Linux = new("linux", read: PowerProbe.Linux);
    public static readonly PowerAuthority Absent = new("absent", read: PowerProbe.Absent);

    [UseDelegateFromConstructor]
    public partial Fin<PowerReading> Read();

    public static PowerAuthority Platform =>
        OperatingSystem.IsMacOS() ? Darwin
        : OperatingSystem.IsWindows() ? Windows
        : OperatingSystem.IsLinux() ? Linux
        : Absent;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PowerReading {
    private PowerReading(PowerState power, Option<ThermalPressure> thermal, double battery) =>
        (Power, Thermal, BatteryFraction) = (power, thermal, battery);

    public PowerState Power { get; }

    public Option<ThermalPressure> Thermal { get; }

    public double BatteryFraction { get; }

    public static Fin<PowerReading> Of(PowerState power, Option<ThermalPressure> thermal, double battery) =>
        double.IsFinite(battery) && battery is >= 0d and <= 1d
            ? Fin.Succ(new PowerReading(power, thermal, battery))
            : Fin.Fail<PowerReading>(new ProfileFault.AttachmentRejected($"power-reading:battery-fraction {battery} outside [0,1]"));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EnergyCell(PowerAuthority authority) {
    private readonly Atom<Option<PowerReading>> cell = Atom(Option<PowerReading>.None);

    public FidelityScale Read() => cell.Value.Map(FidelityScale.Grade).IfNone(FidelityScale.Balanced);

    public Transition<Option<PowerReading>> Refresh() =>
        authority.Read().Match(
            Succ: reading => Cell.Step(cell, _ => Some(Some(reading)), new ProfileFault.AttachmentRejected($"power-authority:{authority.Key} declined")),
            Fail: cause => new Transition<Option<PowerReading>>.Refused(cell.Value, cause));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PowerProbe {
    public const string AcPowerValue = "AC Power";
    public const string BatteryPowerValue = "Battery Power";
    public const string UpsPowerValue = "UPS Power";
    public const string PowerSourceStateKey = "Power Source State";
    public const string CurrentCapacityKey = "Current Capacity";
    public const string MaxCapacityKey = "Max Capacity";
    public const string IsChargingKey = "Is Charging";
    public const string IsPresentKey = "Is Present";
    public const string SourceTypeKey = "Type";
    public const string InternalBatteryType = "InternalBattery";

    public const string PowerSupplyRoot = "/sys/class/power_supply";
    public const string ThermalRoot = "/sys/class/thermal";
    public const string TypeNode = "type";
    public const string OnlineNode = "online";
    public const string StatusNode = "status";
    public const string PresentNode = "present";
    public const string CapacityNode = "capacity";
    public const string TempNode = "temp";
    public const string BatterySupply = "Battery";
    public const string DischargingStatus = "Discharging";

    public static Fin<PowerReading> Darwin() =>
        DarwinPower.Battery().Match(
            Some: battery => PowerReading.Of(DarwinState(battery), DarwinPower.Thermal(), DarwinCharge(battery)),
            None: static () => Unresolved(PowerAuthority.Darwin.Key, $"an IOKit power source of {SourceTypeKey} {InternalBatteryType}"));

    static PowerState DarwinState(DarwinPower.Source battery) =>
        battery.State != BatteryPowerValue ? PowerState.Plugged
        : battery.Present && !battery.Charging && DarwinCharge(battery) < FidelityScale.BatteryReserve ? PowerState.LowBattery
        : PowerState.Battery;

    static double DarwinCharge(DarwinPower.Source battery) =>
        battery.MaxCapacity > 0 ? (double)battery.CurrentCapacity / battery.MaxCapacity : 0d;

    public static Fin<PowerReading> Windows() =>
        WindowsPower.Status().Match(
            Some: status => PowerReading.Of(WindowsState(status), None, status.BatteryLifePercent / 100d),
            None: static () => Unresolved(PowerAuthority.Windows.Key, "a GetSystemPowerStatus read reporting a known AC state and charge"));

    static PowerState WindowsState(WindowsPower.SystemPowerStatus status) =>
        status.ACLineStatus == WindowsPower.AcOnline ? PowerState.Plugged
        : (status.BatteryFlag & (WindowsPower.BatteryLow | WindowsPower.BatteryCritical)) != 0 ? PowerState.LowBattery
        : PowerState.Battery;

    public static Fin<PowerReading> Linux() =>
        LinuxPower.Battery().Match(
            Some: battery => PowerReading.Of(LinuxState(battery), LinuxPower.Thermal(), battery.Capacity / 100d),
            None: static () => Unresolved(PowerAuthority.Linux.Key, $"a {PowerSupplyRoot} row publishing {CapacityNode}"));

    static PowerState LinuxState(LinuxPower.Supply battery) =>
        !LinuxPower.OnMains() && battery.Present && battery.Status == DischargingStatus
            ? (battery.Capacity < FidelityScale.BatteryReserve * 100 ? PowerState.LowBattery : PowerState.Battery)
            : PowerState.Plugged;

    public static Fin<PowerReading> Absent() =>
        Unresolved(PowerAuthority.Absent.Key, "a platform authority reporting battery charge and thermal pressure");

    static Fin<PowerReading> Unresolved(string authority, string requirement) =>
        Fin.Fail<PowerReading>(new ProfileFault.AttachmentRejected($"power-authority:{authority} requires {requirement}"));
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[SupportedOSPlatform("macos")]
public static partial class DarwinPower {
    private const string IOKit = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string ObjC = "/usr/lib/libobjc.A.dylib";
    private const uint Utf8Encoding = 0x08000100;
    private const nint IntType = 9;

    public readonly record struct Source(string State, int CurrentCapacity, int MaxCapacity, bool Charging, bool Present);

    private readonly record struct Handle(IntPtr Address) {
        public static Option<Handle> Of(IntPtr address) => address != IntPtr.Zero ? Some(new Handle(address)) : None;
    }

    [LibraryImport(IOKit)] private static partial IntPtr IOPSCopyPowerSourcesInfo();
    [LibraryImport(IOKit)] private static partial IntPtr IOPSCopyPowerSourcesList(IntPtr blob);
    [LibraryImport(IOKit)] private static partial IntPtr IOPSGetPowerSourceDescription(IntPtr blob, IntPtr source);
    [LibraryImport(CoreFoundation)] private static partial IntPtr CFDictionaryGetValue(IntPtr dictionary, IntPtr key);
    [LibraryImport(CoreFoundation)] private static partial IntPtr CFArrayGetValueAtIndex(IntPtr array, nint index);
    [LibraryImport(CoreFoundation)] private static partial nint CFArrayGetCount(IntPtr array);
    [LibraryImport(CoreFoundation)] private static partial void CFRelease(IntPtr reference);
    [LibraryImport(CoreFoundation, StringMarshalling = StringMarshalling.Utf8)] private static partial IntPtr CFStringCreateWithCString(IntPtr allocator, string value, uint encoding);
    [LibraryImport(CoreFoundation)] [return: MarshalAs(UnmanagedType.U1)] private static partial bool CFStringGetCString(IntPtr text, Span<byte> buffer, nint size, uint encoding);
    [LibraryImport(CoreFoundation)] [return: MarshalAs(UnmanagedType.U1)] private static partial bool CFNumberGetValue(IntPtr number, nint type, out int value);
    [LibraryImport(CoreFoundation)] [return: MarshalAs(UnmanagedType.U1)] private static partial bool CFBooleanGetValue(IntPtr boolean);
    [LibraryImport(ObjC, StringMarshalling = StringMarshalling.Utf8)] private static partial IntPtr objc_getClass(string name);
    [LibraryImport(ObjC, StringMarshalling = StringMarshalling.Utf8)] private static partial IntPtr sel_registerName(string name);
    [LibraryImport(ObjC, EntryPoint = "objc_msgSend")] private static partial IntPtr Send(IntPtr receiver, IntPtr selector);
    [LibraryImport(ObjC, EntryPoint = "objc_msgSend")] private static partial long SendLong(IntPtr receiver, IntPtr selector);

    public static Option<Source> Battery() {
        IntPtr blob = IOPSCopyPowerSourcesInfo();
        if (blob == IntPtr.Zero) { return None; }
        IntPtr list = IOPSCopyPowerSourcesList(blob);
        try {
            for (nint index = 0; list != IntPtr.Zero && index < CFArrayGetCount(list); index++) {
                Option<Source> row = Handle.Of(IOPSGetPowerSourceDescription(blob, CFArrayGetValueAtIndex(list, index)))
                    .Filter(static held => Text(held, PowerProbe.SourceTypeKey).Exists(static kind => kind == PowerProbe.InternalBatteryType))
                    .Map(static held => new Source(
                        State: Text(held, PowerProbe.PowerSourceStateKey).IfNone(string.Empty),
                        CurrentCapacity: Number(held, PowerProbe.CurrentCapacityKey).IfNone(0),
                        MaxCapacity: Number(held, PowerProbe.MaxCapacityKey).IfNone(0),
                        Charging: Flag(held, PowerProbe.IsChargingKey).IfNone(false),
                        Present: Flag(held, PowerProbe.IsPresentKey).IfNone(false)));
                if (row.IsSome) { return row; }
            }
            return None;
        }
        finally {
            if (list != IntPtr.Zero) { CFRelease(list); }
            CFRelease(blob);
        }
    }

    public static Option<ThermalPressure> Thermal() =>
        Handle.Of(objc_getClass("NSProcessInfo"))
            .Bind(static cls => ThermalPressure.TryGet((int)SendLong(Send(cls.Address, sel_registerName("processInfo")), sel_registerName("thermalState")), out var row)
                ? Some(row)
                : None);

    private static Option<Handle> Value(Handle dictionary, string key) {
        IntPtr name = CFStringCreateWithCString(IntPtr.Zero, key, Utf8Encoding);
        try { return Handle.Of(CFDictionaryGetValue(dictionary.Address, name)); }
        finally { CFRelease(name); }
    }

    private static Option<string> Text(Handle dictionary, string key) {
        Span<byte> buffer = stackalloc byte[128];
        return Value(dictionary, key) is { Case: Handle held }
            && CFStringGetCString(held.Address, buffer, buffer.Length, Utf8Encoding)
            && buffer.IndexOf((byte)0) is var terminator && terminator >= 0
            ? Some(Encoding.UTF8.GetString(buffer[..terminator]))
            : None;
    }

    private static Option<int> Number(Handle dictionary, string key) =>
        Value(dictionary, key).Bind(static held => CFNumberGetValue(held.Address, IntType, out var value) ? Some(value) : None);

    private static Option<bool> Flag(Handle dictionary, string key) =>
        Value(dictionary, key).Map(static held => CFBooleanGetValue(held.Address));
}

public static partial class WindowsPower {
    public const byte AcOffline = 0;
    public const byte AcOnline = 1;
    public const byte AcUnknown = 255;
    public const byte BatteryLow = 2;
    public const byte BatteryCritical = 4;
    public const byte PercentUnknown = 255;

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemPowerStatus {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemPowerStatus(out SystemPowerStatus status);

    [SupportedOSPlatform("windows")]
    public static Option<SystemPowerStatus> Status() =>
        GetSystemPowerStatus(out var status)
            && status.BatteryLifePercent != PercentUnknown
            && status.ACLineStatus != AcUnknown
            ? Some(status)
            : None;
}

[SupportedOSPlatform("linux")]
public static class LinuxPower {
    private static readonly Seq<(double Floor, ThermalPressure Level)> CoolingBands =
        Seq((0.9d, ThermalPressure.Critical), (0.6d, ThermalPressure.Serious), (0.3d, ThermalPressure.Fair));

    private static readonly Seq<(string Trip, ThermalPressure Level)> TripBands =
        Seq(("critical", ThermalPressure.Critical), ("hot", ThermalPressure.Serious), ("passive", ThermalPressure.Fair));

    public readonly record struct Supply(string Status, int Capacity, bool Present);

    public static Option<Supply> Battery() =>
        Rows().Filter(static row => Node(row, PowerProbe.TypeNode).Exists(static kind => kind == PowerProbe.BatterySupply))
            .Map(static row => (Row: row, Capacity: Reading(row, PowerProbe.CapacityNode)))
            .Filter(static pair => pair.Capacity.IsSome)
            .Map(static pair => new Supply(
                Status: Node(pair.Row, PowerProbe.StatusNode).IfNone(string.Empty),
                Capacity: (int)pair.Capacity.IfNone(0),
                Present: Reading(pair.Row, PowerProbe.PresentNode).Map(static held => held != 0).IfNone(true)))
            .Head;

    public static bool OnMains() =>
        Rows().Filter(static row => !Node(row, PowerProbe.TypeNode).Exists(static kind => kind == PowerProbe.BatterySupply))
            .Exists(static row => Reading(row, PowerProbe.OnlineNode).Exists(static held => held != 0));

    public static Option<ThermalPressure> Thermal() => Tripped() | Cooled();

    static Option<ThermalPressure> Tripped() =>
        Optional(Zones().Map(static zone => (Zone: zone, Temp: Reading(zone, PowerProbe.TempNode)))
            .Filter(static pair => pair.Temp.IsSome)
            .Bind(static pair => TripBands.Filter(band => Trip(pair.Zone, band.Trip).Exists(point => pair.Temp.Exists(temp => temp >= point)))
                .Map(static band => band.Level))
            .MaxBy(static level => level.Key));

    static Option<ThermalPressure> Cooled() =>
        Optional(Devices().Map(static device => (Current: Reading(device, "cur_state"), Max: Reading(device, "max_state")))
            .Filter(static pair => pair.Current.IsSome && pair.Max.Exists(static max => max > 0))
            .Map(static pair => pair.Current.IfNone(0) / (double)pair.Max.IfNone(1))
            .Bind(static ratio => CoolingBands.Filter(band => ratio >= band.Floor).Map(static band => band.Level))
            .MaxBy(static level => level.Key));

    static Option<long> Trip(string zone, string kind) =>
        toSeq(Directory.EnumerateFiles(zone, "trip_point_*_type"))
            .Filter(path => Text(path).Exists(band => band == kind))
            .Bind(static path => Reading(Path.GetDirectoryName(path)!, Path.GetFileName(path).Replace("_type", "_temp")).ToSeq())
            .Head;

    static Seq<string> Rows() => Children(PowerProbe.PowerSupplyRoot);

    static Seq<string> Zones() => Children(PowerProbe.ThermalRoot).Filter(static path => Path.GetFileName(path).StartsWith("thermal_zone", StringComparison.Ordinal));

    static Seq<string> Devices() => Children(PowerProbe.ThermalRoot).Filter(static path => Path.GetFileName(path).StartsWith("cooling_device", StringComparison.Ordinal));

    static Seq<string> Children(string root) =>
        Directory.Exists(root) ? toSeq(Directory.EnumerateDirectories(root)) : Seq<string>();

    static Option<string> Node(string directory, string node) => Text(Path.Join(directory, node));

    static Option<long> Reading(string directory, string node) =>
        Node(directory, node).Bind(static text => long.TryParse(text, CultureInfo.InvariantCulture, out var value) ? Some(value) : None);

    static Option<string> Text(string path) =>
        Op.Of().Catch(() => Fin.Succ(File.ReadAllText(path).Trim())).ToOption().Filter(static text => text.Length > 0);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
