# [APPHOST_HOST_PROFILES]

Rasm.AppHost boots every process from one supplied `ConsumptionProfile` row: a composition root states `tenancy`, `topology`, `host`, `lifecycle`, `isolation`, and `providers`, `Resolve` admits that row against the crossings this branch answers, and every boot fact folds out of the axis values — server GC, ReadyToRun, module scan, single-instance, co-hosted assets, ship vehicle, OTLP export, builder construction, lifetime attach. `Boot` turns the resolved record into a configured Generic Host builder, one identity fold derives per-user roots and telemetry resource attributes from it, and one power-and-fidelity fold reads the live power state and thermal budget to scale compute fidelity on a battery- or thermally-constrained host.

`RecoveryObjective` rides the host descriptor and the topology row as the declared `(Rpo, Rto)` window and projects onto `ResolvedProfile`, so `Rasm.Persistence/Version/recovery` reads the DR target as settled vocabulary and never mints it locally. This page owns the six-axis roster, the host and provider descriptor shapes, the axis-refusal rail, the per-modality DR objective, the boot-attach delegate rows, the resource-identity fold, and the energy-aware fidelity scaling over Microsoft.Extensions.Hosting, Thinktecture-generated vocabulary, LanguageExt rails, NodaTime instants, the OpenTelemetry resource seam, and the per-platform power-state native reads.

## [01]-[INDEX]

- [02]-[PROFILE_AXIS]: Six-axis consumption roster, descriptor shapes, axis refusal, one resolved record.
- [03]-[LIFETIME_ADAPTERS]: Builder selection, lifetime delegates, `HostOptions` policy, and hook projection.
- [04]-[RESOURCE_IDENTITY]: Per-user roots including the durable queue root, and the resource triple behind one detector.
- [05]-[POWER_AND_FIDELITY]: Power-state and thermal-budget reads; energy-aware compute-fidelity scaling.

## [02]-[PROFILE_AXIS]

- Owner: `ProfileAxis` names the six-axis roster; `Tenancy`, `DeploymentTopology`, `LifecycleOwner`, and `Isolation` close their vocabularies; `HostDescriptor` and `ProviderDescriptor` fix the two open axes' descriptor shape over `DescriptorLifetime`, the span-and-ender pair both families answer `lifetime` with; `ConsumptionProfile` carries the supplied row, `RecoveryObjective` its `(Rpo, Rto)` durability column, and `ResolvedProfile` the only profile artifact siblings consume.
- Cases: `tenancy` = none | single | multi; `topology` = in-host | sidecar | companion | service | edge | cli; `lifecycle` = caller-owned | package-owned; `isolation` = in-proc | thread | process | wasm | remote; `host` and `providers` carry descriptor rows this branch supplies through `HostRows` and `ProviderRows`, each row answering `Fits`, `Tenancy`, `Lifetime`, and `Degrade` beside its family's extension columns — `ShipVehicle`, `HostAttach`, `HostSurface`, `RecoveryObjective`, and the five capability booleans for a host, `Supplies` and `Reach` for a provider; `HostAttach` = Foreign | AppRoot | Quiet | Managed; `HostSurface` = Embedded | Windowed | Offscreen | None; `RuntimeAttachment` = Isolated | Integrating; `ProfileFault` = Text | AttachmentRejected | RootUnresolved | AxisUnsupported in the 1100 code band.
- Entry: `Fin<ResolvedProfile> Resolve(ConsumptionProfile profile, string applicationName, string environmentName, string contentRoot, string serviceVersion, IClock clock, Option<RuntimeAttachment> attachment = default)` — `Admit` gates the axis values first, so `Fin` aborts on axis refusal, attachment rejection, and root rejection.
- Auto: one supplied row replaces every bootstrap program — a host descriptor overrides its topology row's `Vehicle`, `Attach`, `Surface`, and `Durability` columns while an unhosted profile reads the topology row, so `ServerGc`, `ReadyToRun`, `ModuleScan`, `SingleInstance`, `CoHostedAssets`, `LocalStore`, `HostDocument`, and `OtlpExport` fold from axis values with no key roster between them; raw axis keys admit through each vocabulary's generated `Validate` against `ProfileFault`.
- Receipt: `Canonical()` emits the six axis rows in roster order under an ordinal provider-key sort and `CanonicalJson()` renders them as the one UTF-8 `canonical-json` preimage — the byte-deriving input the `consumption-profile` corpus contract freezes, so the three branches diff one string rather than three rosters.
- Packages: Microsoft.Extensions.Hosting, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one host integration is one `HostRows` descriptor row and one bound port is one `ProviderRows` row, each answering its family's whole coordinate set at zero new surface, so a row minted short of one is the deleted form; a new closed-axis value is one member on its owning vocabulary, and a new axis is one `ProfileAxis` row beside one `ConsumptionProfile` column, both settling at the corpus roster first.
- Boundary: every open-axis row answers the consumption-descriptor coordinates in this branch's casing — `Fits` the selection sentence a composition root picks the row on, `Tenancy` the MECHANISM the row separates tenants by and never a `Tenancy` roster value, `Lifetime` a survival span paired with the `LifecycleOwner` that ends it, `Degrade` derived from the capability columns already expressing each forfeit; `Admit` rides each family's lead because every row in it answers alike — a host through `ProfileBoot.Boot` over its own `Attach` delegate pair, a provider through the `ConsumptionProfile.Providers` seat — and a residual states only what no column carries, which is why the provider family declares none and the host family spends one on `test-harness` alone; axis values stay data — a compile-time assumption, an ambient global, a build flag, and a package branching on which product hosts it are the four deleted forms, so a host integration lands as a descriptor row and never as a closed case; `Admit` refuses an unservable axis value with `ProfileFault.AxisUnsupported` carrying `AxisEvidence` that names the axis, so silent degradation and a narrowed public surface never happen; in-host topology carrying no host descriptor refuses on the `host` axis because a consuming application supplies its own row; `isolation` refuses where no bound provider supplies the crossing's capability; `RuntimeAttachment.Integrating` admits only where the resolved row carries `SingleInstance`, so a shared store root reaches exactly one live instance; `RecoveryObjective` is the branch's one DR-target declaration — `Rasm.Persistence` IMPORTS the type through the `Runtime ⇄ Rasm.Persistence/Version/recovery # [IMPORT]: RecoveryObjective` seam and a composition root threads `ResolvedProfile.Recovery` in as the value, so a Persistence-local `(Rpo, Rto)` record and a host-band-keyed RPO/RTO table there are both the deleted form; `ResolvedProfile` itself never crosses, because a one-line accessor over `.Recovery` is a forwarding wrapper rather than a port; grading lives with the observation, so `Rasm.Persistence/Store/schema` `RecoveryWindow.Gauged` is the ONE gauge folding each `RecoveryAxis` row's measured half against the declared column, and a `Meets*` predicate on this struct is the deleted second grader blind to the unmeasured half; column values stay app-root publish and composition facts — DATAS tuning knobs enter only behind a losing benchmark claim, the `SingleInstance` value is probed through the discovery manifest, a `CoHostedAssets` host serves the built TS bundle same-origin from its app root through `UseStaticFiles(StaticFileOptions)` with `FileProvider`/`RequestPath` off the selected bundle root — `MapStaticAssets` is foreclosed because it resolves a BUILD-emitted static-web-asset manifest and this column selects its bundle at RUNTIME, so a tree the .NET build never enumerated is absent from that manifest and answers 404 — which makes the column's invariant a provider question rather than a build one: a `CoHostedAssets: true` row whose selected root resolves no readable directory is a boot-time refusal, never a per-request miss, with cross-origin headers held as designed growth; and the test-harness row composes FakeTimeProvider, FakeClock, in-memory configuration, instant deadline overrides, and LeakTrackingObjectPool over provider-validation proof.

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
    // Unreachable by construction — `Admit` refuses in-host carrying no host descriptor and `ResolvedProfile.Recovery`
    // prefers the host row's column, so this cell reads the `Relaxed` window both in-host host rows select.
    public static readonly DeploymentTopology InHost = new("in-host", serverGc: false, vehicle: ShipVehicle.Yak, attach: HostAttach.AppRoot, surface: HostSurface.Windowed, durability: RecoveryObjective.Relaxed);
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

// Host descriptors and topology rows DECLARE this `(Rpo, Rto)` window and project it onto `ResolvedProfile`.
// This struct is the branch's ONE declaration: `Rasm.Persistence` imports the type for its `Version/recovery`
// gauge and its `Store/schema` admission parameter, the same S2-over-S1 crossing `Rasm.Materials` takes for
// `BenchmarkGate`, so a Persistence-local `(Rpo, Rto)` record is a twin rather than a port shape. Declaring and
// GRADING split: a `Meets*` predicate here takes a bare `Duration` that cannot spell the unmeasured half, and
// `Store/schema` `RecoveryWindow.Gauged` already folds every `RecoveryAxis` row into one `RecoveryReading`.
public readonly record struct RecoveryObjective(Duration Rpo, Duration Rto) {
    public static readonly RecoveryObjective Strict = new(Duration.FromMinutes(1), Duration.FromMinutes(15));
    public static readonly RecoveryObjective Standard = new(Duration.FromMinutes(5), Duration.FromMinutes(30));
    public static readonly RecoveryObjective Relaxed = new(Duration.FromMinutes(15), Duration.FromHours(1));
    public static readonly RecoveryObjective Instant = new(Duration.Zero, Duration.Zero);
}

// One shape carries the lifetime coordinate for both open-axis families, because stating a span without its
// ender is half an answer: a reader learns how long what entered lasts and never who tears it down. Ender
// spells the closed `lifecycle` vocabulary, so neither family mints a second word for caller and package.
public readonly record struct DescriptorLifetime(string Survives, LifecycleOwner Ender);

// Admission answers alike for every host row and rides this sentence instead of a column: `ProfileBoot.Boot`
// composes the row's own `Attach.CreateBuilder`/`Attach.AttachLifetime` pair, so a column beside `Attach`
// restates a member the row already carries. `Tenancy` names the MECHANISM a row separates tenants by and
// never a `Tenancy` roster value — that closed axis is a profile column, and re-spelling it here forks one
// vocabulary into two. `Residual` states only the forfeit no capability column already expresses.
[ComplexValueObject]
[ValidationError<ProfileFault>]
public sealed partial class HostDescriptor {
    public string Key { get; }
    public string Fits { get; }
    public string Tenancy { get; }
    public DescriptorLifetime Lifetime { get; }
    public string Residual { get; }
    public ShipVehicle Vehicle { get; }
    public HostAttach Attach { get; }
    public HostSurface Surface { get; }
    public RecoveryObjective Durability { get; }
    public bool Document { get; }
    public bool LocalStore { get; }
    public bool ModuleScan { get; }
    public bool SingleInstance { get; }
    public bool CoHostedAssets { get; }

    // Forfeits DERIVE from the capability columns rather than restating them: a false column IS the forfeit,
    // so one fact keeps one owner and no row claims a capability its own column denies. Residual rides the
    // same fold as a pinned row whose held flag is its emptiness, so an unspent one drops out with the rest.
    public Seq<string> Degrade => Forfeits.Filter(static row => !row.Held).Map(static row => row.Column);

    private Seq<(bool Held, string Column)> Forfeits => [
        (Document, nameof(Document)),
        (LocalStore, nameof(LocalStore)),
        (ModuleScan, nameof(ModuleScan)),
        (SingleInstance, nameof(SingleInstance)),
        (CoHostedAssets, nameof(CoHostedAssets)),
        (Residual.Length == 0, Residual),
    ];
}

// Admission answers alike for every provider row and rides this sentence: a composition root seats the row on
// `ConsumptionProfile.Providers`, which `Grants` folds into the capability set `Supplies` is read against, so
// no row carries an entry of its own. No provider row spends a degradation residual either, so this family
// states none and `Degrade` derives from `Reach` alone.
[ComplexValueObject]
[ValidationError<ProfileFault>]
public sealed partial class ProviderDescriptor {
    public string Key { get; }
    public string Fits { get; }
    public string Tenancy { get; }
    public DescriptorLifetime Lifetime { get; }
    public Capability Supplies { get; }
    // Reach is the degradation coordinate: a remote-reaching provider drops out of the retained set the
    // moment DegradationLevel stops retaining RemoteCompute, while an in-proc row survives every level.
    public Isolation Reach { get; }

    // Forfeit IS the crossing capability the reach demands, so this derives off `Isolation.Needs` and mints
    // nothing: an in-proc or thread row needs no crossing, forfeits none, and answers empty.
    public Seq<string> Degrade => Reach.Needs.Map(static needed => needed.Key).ToSeq();
}

// Rows this branch supplies for the OPEN axes. A consumer embedding the estate inside its own product
// mints its own row against the same shape; nothing here is a closed set a package may switch over.
public static class HostRows {
    public static readonly HostDescriptor Rhino = HostDescriptor.Create(
        key: "rhino",
        fits: "a Rhino instance loads this estate through its own plug-in loader",
        tenancy: "one document session per process, scoped by the launching user's profile root",
        lifetime: new("until the host unloads the plug-in load context", LifecycleOwner.CallerOwned),
        residual: "",
        vehicle: ShipVehicle.Yak, attach: HostAttach.Foreign, surface: HostSurface.Embedded, durability: RecoveryObjective.Relaxed,
        document: true, localStore: true, moduleScan: true, singleInstance: false, coHostedAssets: false);

    public static readonly HostDescriptor Gh2 = HostDescriptor.Create(
        key: "gh2",
        fits: "a Grasshopper2 editor loads this estate beside its own solution graph",
        tenancy: "one document session per process, scoped by the launching user's profile root",
        lifetime: new("until the host unloads the plug-in load context", LifecycleOwner.CallerOwned),
        residual: "",
        vehicle: ShipVehicle.Yak, attach: HostAttach.Foreign, surface: HostSurface.Embedded, durability: RecoveryObjective.Relaxed,
        document: true, localStore: true, moduleScan: true, singleInstance: false, coHostedAssets: false);

    public static readonly HostDescriptor DesktopShell = HostDescriptor.Create(
        key: "desktop-shell",
        fits: "this estate ships and launches as its own desktop bundle",
        tenancy: "one live instance per user, held by the single-instance discovery manifest",
        lifetime: new("until the launched process exits", LifecycleOwner.PackageOwned),
        residual: "",
        vehicle: ShipVehicle.DesktopBundle, attach: HostAttach.AppRoot, surface: HostSurface.Windowed, durability: RecoveryObjective.Standard,
        document: false, localStore: true, moduleScan: true, singleInstance: true, coHostedAssets: false);

    public static readonly HostDescriptor WebAppRoot = HostDescriptor.Create(
        key: "web-app-root",
        fits: "an application root composes this estate and serves the built bundle same-origin",
        tenancy: "a request-scoped TenantContext adopted per ingress carrier",
        lifetime: new("until the application root stops the Generic Host", LifecycleOwner.CallerOwned),
        residual: "",
        vehicle: ShipVehicle.Oci, attach: HostAttach.AppRoot, surface: HostSurface.None, durability: RecoveryObjective.Strict,
        document: false, localStore: false, moduleScan: false, singleInstance: false, coHostedAssets: true);

    public static readonly HostDescriptor TestHarness = HostDescriptor.Create(
        key: "test-harness",
        fits: "a test assembly composes this estate against fake time and in-memory configuration",
        tenancy: "one composition per test scope, sharing no root across scopes",
        lifetime: new("until the fixture disposes the composition", LifecycleOwner.CallerOwned),
        residual: "wall-clock progression, since FakeTimeProvider and FakeClock advance only where a test drives them",
        vehicle: ShipVehicle.Folder, attach: HostAttach.AppRoot, surface: HostSurface.Offscreen, durability: RecoveryObjective.Instant,
        document: false, localStore: false, moduleScan: true, singleInstance: false, coHostedAssets: false);
}

public static class ProviderRows {
    public static readonly ProviderDescriptor OtlpCollector = ProviderDescriptor.Create(
        key: "otlp-collector",
        fits: "a deployment exports the four signals to a collector endpoint",
        tenancy: "resource attributes stamp the emitter, and the queue root scopes by host key",
        lifetime: new("until the durable queue drains its last batch", LifecycleOwner.PackageOwned),
        supplies: Capability.TelemetryExport, reach: Isolation.Remote);

    public static readonly ProviderDescriptor RemoteSolver = ProviderDescriptor.Create(
        key: "remote-solver",
        fits: "solve capacity lives off-box behind the outbound hop",
        tenancy: "a per-tenant Budget debits at the brokered mediation gate",
        lifetime: new("until the calling scope cancels or the hop deadline elapses", LifecycleOwner.CallerOwned),
        supplies: Capability.RemoteCompute, reach: Isolation.Remote);

    public static readonly ProviderDescriptor LocalSolver = ProviderDescriptor.Create(
        key: "local-solver",
        fits: "solve capacity runs on this machine inside a spawned child",
        tenancy: "one grant scope per spawned child, holding no ambient host authority",
        lifetime: new("until the sandbox kill rail converges the child", LifecycleOwner.PackageOwned),
        supplies: Capability.LocalCompute, reach: Isolation.Process);

    public static readonly ProviderDescriptor DocumentBridge = ProviderDescriptor.Create(
        key: "document-bridge",
        fits: "a live host document backs this estate's reads and writes",
        tenancy: "one host document every caller in the process shares",
        lifetime: new("until the host closes the document", LifecycleOwner.CallerOwned),
        supplies: Capability.HostDocument, reach: Isolation.InProc);

    public static readonly ProviderDescriptor StoreReader = ProviderDescriptor.Create(
        key: "store-reader",
        fits: "a resolved local store root answers reads",
        tenancy: "one per-user store root the resolved profile fixed",
        lifetime: new("until the composition root disposes the reader", LifecycleOwner.PackageOwned),
        supplies: Capability.StoreRead, reach: Isolation.InProc);

    public static readonly ProviderDescriptor StoreWriter = ProviderDescriptor.Create(
        key: "store-writer",
        fits: "a resolved local store root accepts writes",
        tenancy: "one per-user store root the resolved profile fixed",
        lifetime: new("until the composition root disposes the writer", LifecycleOwner.PackageOwned),
        supplies: Capability.StoreWrite, reach: Isolation.InProc);
}

// `Host` tails the positional list carrying `= default`: this profile rides `PhaseReceipt` across the suite wire,
// whose `OmitAbsent` modifier drops an absent `Option<T>` at write, so a slot without a default reads back
// wire-required under `RespectRequiredConstructorParameters` and fails the decode of its own emission.
public sealed record ConsumptionProfile(
    Tenancy Tenancy,
    DeploymentTopology Topology,
    LifecycleOwner Lifecycle,
    Isolation Isolation,
    Seq<ProviderDescriptor> Providers,
    Option<HostDescriptor> Host = default) {
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

    // Roster order under UTF-8 fixes the `canonical-json` PREIMAGE this corpus contract freezes as a vector, so
    // fixtures derive bytes from this member rather than from a reader's transcription of the roster, and each
    // branch proving parity compares one string. Values escape through `JsonEncodedText`, so a descriptor key
    // carrying a quote or a control character renders as an admissible literal; NON-ASCII does not survive the
    // crossing, because this encoder emits `\uXXXX` where the peer branches' encoders emit raw UTF-8 — the
    // printable-ASCII bound `tests/contracts/MANIFEST.md` `[02.10]` states is what makes the three renders one
    // string. Serializing a dictionary is deleted, because property order there belongs to the collection rather
    // than the roster and drifts on rehash.
    public string CanonicalJson() =>
        $"{{{string.Join(',', Canonical().Select(static row =>
            $"\"{JsonEncodedText.Encode(row.Key).Value}\":\"{JsonEncodedText.Encode(row.Value).Value}\""))}}}";
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
- Auto: Boot composes the resolved `HostAttach` row's `CreateBuilder` and `AttachLifetime` delegates with `HostOptions` — startup and shutdown timeouts, concurrent start and stop, `BackgroundServiceExceptionBehavior.StopHost` — deleting per-host bootstrap programs; the `Managed` row registers `AddSystemd` for the Linux-server backend, and `MirrorService` rides the existing `Lifecycle.Subscribe` fold so every committed transition fires its service-state mirror through one subscriber seat, never a per-callsite emission; `WatchdogTick` derives the keep-alive period from the manager's own deadline and the schedule-port heartbeat row runs `Watchdog` on it, never a second timer; `Reloaded` brackets a reload fold in the manager's reload window; `Aborted` flattens a `HostAbortedException` into the boot-fault trigger value with no second state machine.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Hosting.Systemd, Microsoft.Extensions.Options, NodaTime, BCL inbox
- Receipt: `ServiceNotify` projects each `RuntimePhase` transition to its `ServiceState` sd_notify mirror through one table lookup, so a new host modality inherits the mirror as one row; `Watchdog` emits the `WatchdogPing` keep-alive payload and `Reloaded` the `RELOADING=1`/`STATUS=`/`READY=1` window on the same `ISystemdNotifier`; every emission returns `Fin<Unit>`, so a torn-down notify socket lands as a refusal on the caller's rail rather than an exception crossing a phase commit.
- Growth: one `HostAttach` row — a key beside two static delegate targets bound through the row constructor — extends the lifetime surface with zero new surface; one `ServiceNotify` row binds a new phase-to-state mirror without leaving the fold; a new sd_notify assertion is one payload mint through the `ServiceState(string)` ctor, never a package change, because the package names `Ready` and `Stopping` alone.
- Boundary: a `CoHostedAssets` host crosses in through `external` — its builder is constructed at the web app root, where ASP.NET Core enters as a shared-framework asset only, and the static-file middleware seats there under the column's gate ahead of endpoint routing; the host registers `ConsoleLifetime` as the default `IHostLifetime` on every builder path including the empty builder, and that default is the ALC-COLLECTIBILITY blocker rather than a console nuisance — `ConsoleLifetime` holds three `PosixSignalRegistration` values (SIGINT, SIGQUIT, SIGTERM), each handing a delegate to a process-global native handler table that roots the lifetime, its service provider, and the whole plugin load context, and its `StopAsync` releases none of them because only `Dispose` unregisters — so a `Foreign` attach swaps in the no-op `DetachedLifetime` through `Detached` to keep those roots unplanted and host-attach trigger injection drives phases; teardown is terminal at `await ((IAsyncDisposable)host).DisposeAsync()`, which also drains the `BackgroundService` tasks the synchronous `Dispose` never awaits, and `await host.StopAsync()` alone releases none of the registrations, since `ConsoleLifetime.StopAsync` is documented to do nothing and only `Dispose` unregisters — the two facts are INDEPENDENT and neither substitutes for the other: the lifetime swap keeps the process-global roots unplanted, so a detached host's load context collects even undisposed, while disposal releases whatever a composition root planted anyway; `AddSystemd` is the one service-manager registration — `SystemdHelpers.IsSystemdService` gates the live `ISystemdNotifier.Notify` emission so the notify socket is written only under systemd on the Linux-server backend; `MirrorService` registers one `Lifecycle.Subscribe` observer at the composition root for the `Managed` row, so `Emit` fires on every committed `PhaseReceipt` — `ServiceState.Ready` mirrors the ready transition and `ServiceState.Stopping` mirrors the draining transition, the two payloads the package names — and it subscribes on the shielded phase hook point, so a dead socket parks as isolated evidence instead of unwinding through the CAS commit; the service-manager liveness keep-alive rides the schedule-port heartbeat row through `Watchdog` writing the `WatchdogPing` payload, its PERIOD derived by `WatchdogTick` as half the manager's `WATCHDOG_USEC` deadline under the unset-or-equal `WATCHDOG_PID` guard, and an absent `WATCHDOG_USEC` registers no heartbeat row at all — the manager expects no keep-alive there, and a fixed fallback period is the fabricated-measurement form; the watchdog carries a UNIT-side obligation the fence states because the default is a trap — systemd's `WatchdogSignal=` defaults to SIGABRT, which the CoreCLR PAL fully absorbs, so a missed deadline hangs the unit in `deactivating` for the whole `TimeoutStopSec` before the SIGKILL fallback (`Result=watchdog`, witnessed), and the unit therefore declares `WatchdogSignal=SIGKILL` or an explicit SIGABRT disposition so a missed deadline kills promptly; the reload window is `Reloaded`, whose `RELOADING=1` — carrying the mandatory `MONOTONIC_USEC` stamp a bare assertion cannot omit — opens and re-sent `READY=1` closes the `Type=notify-reload` handshake the unit declares beside `ReloadSignal=` (default SIGHUP), so `ExecReload=kill -HUP $MAINPID` is the deleted unit form — asynchronous, unorderable, and carrying no completion notification — while launchd publishes NO reload facility of any kind and its macOS trigger is the operator command `launchctl kill SIGHUP <domain>/<label>`, no plist key declaring it; `HostAbortedException` during build projects through `Aborted` to a boot-fault trigger value consumed by the transition entrypoint, never a second state machine.

```csharp signature
public static class ProfileBoot {
    // Package names `Ready` and `Stopping` and nothing else; every further sd_notify assertion mints through its
    // public `ServiceState(string)` ctor, so this protocol vocabulary reaches whole with no package gap.
    public static readonly ServiceState WatchdogPing = new("WATCHDOG=1");

    // Watchdog rides the environment, never the package: `Microsoft.Extensions.Hosting.Systemd` reads
    // `NOTIFY_SOCKET` and `LISTEN_PID` and nothing else, so these two names read directly, exactly as this
    // suite's socket-activation adapter reads its own pair, and never through an absent libsystemd binding.
    public const string WatchdogUsecVariable = "WATCHDOG_USEC";
    public const string WatchdogPidVariable = "WATCHDOG_PID";

    public static HostApplicationBuilder CreateApp(HostApplicationBuilderSettings settings) => Host.CreateApplicationBuilder(settings);

    public static HostApplicationBuilder CreateEmpty(HostApplicationBuilderSettings settings) => Host.CreateEmptyApplicationBuilder(settings);

    public static IHostApplicationBuilder Inherit(IHostApplicationBuilder builder) => builder;

    public static IHostApplicationBuilder Detached(IHostApplicationBuilder builder) =>
        (builder.Services.Replace(ServiceDescriptor.Describe(typeof(IHostLifetime), typeof(DetachedLifetime), ServiceLifetime.Singleton)), builder).Item2;

    public static IHostApplicationBuilder Quiet(IHostApplicationBuilder builder) =>
        (builder.Services.Configure<ConsoleLifetimeOptions>(static options => options.SuppressStatusMessages = true), builder).Item2;

    public static IHostApplicationBuilder Service(IHostApplicationBuilder builder) =>
        (builder.Services.AddSystemd(), builder).Item2;

    // MONOTONIC_USEC beside RELOADING=1 is a HARD REQUIREMENT, witnessed on systemd 260: a bare RELOADING=1 is
    // SILENTLY DISCARDED, so `systemctl reload` blocks to `TimeoutStartSec` and fails (rc=1) while the service
    // survives, where the stamped pair reloads in rc=0. `Type=notify-reload` holds the unit in `reloading` until a
    // re-sent READY=1 closes the window. `Stopwatch.GetTimestamp()` already samples CLOCK_MONOTONIC on Linux, so
    // that stamp converts through `Stopwatch.Frequency` into microseconds with no P/Invoke.
    public static ServiceState Reloading() =>
        new($"RELOADING=1\nMONOTONIC_USEC={(Stopwatch.GetTimestamp() * 1_000_000L / Stopwatch.Frequency).ToString(CultureInfo.InvariantCulture)}");

    public static ServiceState Status(string text) => new($"STATUS={text}");

    public static Option<ServiceState> ServiceNotify(RuntimePhase phase) =>
        phase == RuntimePhase.Ready ? Some(ServiceState.Ready)
        : phase == RuntimePhase.Draining ? Some(ServiceState.Stopping)
        : None;

    // Every emission returns the rail. The write is a UNIX datagram send on a socket the manager owns, so a
    // torn-down socket throws, and this arm runs under a lifecycle subscriber and a heartbeat occurrence — both
    // places where an escaping exception crosses a CAS commit or kills a scheduled row. `IsEnabled` answers only
    // whether `NOTIFY_SOCKET` was exported, never whether the peer is still listening.
    public static Fin<Unit> Notify(ISystemdNotifier notifier, ServiceState state) =>
        notifier.IsEnabled
            ? Try.lift(fun(() => notifier.Notify(state))).Run()
            : Fin.Succ(unit);

    public static Fin<Unit> Emit(ISystemdNotifier notifier, RuntimePhase phase) =>
        ServiceNotify(phase).Match(
            Some: state => Notify(notifier, state),
            None: static () => Fin.Succ(unit));

    public static Fin<Unit> Watchdog(ISystemdNotifier notifier) => Notify(notifier, WatchdogPing);

    // sd_watchdog_enabled(3) protocol law: the manager expects keep-alives when `$WATCHDOG_USEC` is set AND
    // `$WATCHDOG_PID` is UNSET OR names this process — the opposite polarity from the socket-activation guard,
    // which requires `$LISTEN_PID` to EQUAL the pid and disqualifies on absence. Reading the two alike disables a
    // watchdog on every unit that sets `WatchdogSec=` without exporting a pid; a plain unit always exports it
    // equal to MainPID (witnessed), so the unset arm serves the sd_notify-from-elsewhere case the law admits.
    // Ticks run at half the deadline, and the manager restarts its countdown from EACH notification, so a tick
    // late by under that half-margin still lands inside the window. An absent deadline yields NO period: the
    // manager expects nothing there, so no heartbeat row registers, where a fallback column would arm a
    // keep-alive against a watcher that never watches.
    public static Option<Duration> WatchdogTick() =>
        Optional(Environment.GetEnvironmentVariable(WatchdogUsecVariable))
            .Filter(static _ => Optional(Environment.GetEnvironmentVariable(WatchdogPidVariable))
                .Filter(static owner => owner.Length > 0)
                .Match(
                    Some: static owner => int.TryParse(owner, CultureInfo.InvariantCulture, out var pid) && pid == Environment.ProcessId,
                    None: static () => true))
            .Bind(static declared => long.TryParse(declared, CultureInfo.InvariantCulture, out var usec) && usec > 0L
                ? Some(Duration.FromNanoseconds(usec * 500L))
                : None);

    // Window brackets the WHOLE fold including its refusal, so a rejected re-validation still closes this
    // handshake and no unit parks in `reloading` until its manager's own reload timeout cuts it down.
    public static Fin<ReloadOutcome> Reloaded(ISystemdNotifier notifier, Func<Fin<ReloadOutcome>> reload) =>
        from opened in Notify(notifier, Reloading())
        from outcome in reload()
        from stated in Notify(notifier, Status($"reload:{outcome.Key}"))
        from closed in Notify(notifier, ServiceState.Ready)
        select outcome;

    // Observers subscribe on the shielded phase hook point, so this socket write parks as isolated evidence
    // rather than unwinding out of whichever CAS commit published the receipt.
    public static PhaseSubscription MirrorService(Lifecycle lifecycle, ISystemdNotifier notifier) =>
        lifecycle.Subscribe(receipt => Emit(notifier, receipt.To));

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
|  [11]   | `WatchdogPing` via `Watchdog`               | keep-alive at the `WatchdogTick` half-deadline       |
|  [12]   | `Reloading`/`Status` via `Reloaded`         | reload window around one `ReloadOutcome`, then ready |

## [04]-[RESOURCE_IDENTITY]

- Owner: `ProfileIdentity` — per-user root computation and the telemetry resource triple; `ProfileRoots` is the path artifact carried inside the resolved record, splitting the data base from the config base and carrying the durable OTLP queue root beside the store and support roots; `QueueRootVariable` the deploy coordinate for that queue, spelled off the `Runtime/config#SOURCE_AXIS` `ConfigSource.EnvPrefix`; `HostResourceDetector` the one `IResourceDetector` carrying both the resolved record and its composition-supplied extra rows.
- Entry: `ImmutableArray<KeyValuePair<string, object>> ResourceAttributes(ResolvedProfile resolved, params ReadOnlySpan<KeyValuePair<string, object>> extra)` — pure projection over the resolved record; `string InstanceId(ResolvedProfile resolved)` the one per-process instance spelling the resource row and the boot log enricher share; `new HostResourceDetector(resolved, extra)` is the detector `Observability/telemetry#SIGNAL_GOVERNANCE` `ResourceIdentity.Compose` seats BEHIND the enriching contrib chain and AHEAD of the deployment-override detector.
- Auto: identity derives from the resolved record before any provider construction, and the detector's `Detect` returns that projection through `new Resource(IEnumerable<KeyValuePair<string, object>>)`, so `ConfigureResource` admits ONE resource feed and a per-call attribute push at each provider is the deleted form; SEAT ORDER is the whole precedence law and the only one: the builder folds every seated source left to right through `Resource.Merge`, which awards each colliding key to the incoming resource with no distinction between an attribute list and a detector, so the enriching host, os, process, runtime, and container detectors seat FIRST and lose every collision with the mint, the mint seats next, and the deploy-plane environment-variable detector tails and outranks all of it; the triple assembles from the `TelemetryDomain` namespace const and the resolved record alone, so a branch-wide namespace rename moves every resource, instrument, and dimension together; rasm-owned resource dimensions read their `TelemetryDomain` row rather than a literal, so each one resolves the roster the conformance gate proves against; the queue root folds the deploy-declared durable volume ahead of the local-disk evidence, so a containerized service arms its offline queue on the path a deployment mounted while a desktop host arms on its own base and a host owning neither opens none, and store residence and queue residence stay two answers on every arm — a companion scopes both under its own segment, an integrating instance keeps its queue off the shared store root it attached to, and every queue scopes by host key so two co-resident processes under one mount stay apart.
- Packages: OpenTelemetry, Rasm, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one attribute row or one root policy value per new identity fact, or one sibling `IResourceDetector` composed through `ResourceIdentity.Compose`; zero new surface.
- Boundary: roots are per-user paths off TWO platform bases — `LocalApplicationData` carries the store, support, and queue roots because those are data, and `ApplicationData` carries the config root alone, since the two collapse on darwin but diverge on linux (`$XDG_DATA_HOME` versus `$XDG_CONFIG_HOME`) and roam versus stay local on windows, so a single base lands a document store, a crash marker, and a durable queue in a CONFIG directory on exactly the service and edge rows that only ever run on linux; a `LocalStore` host stores under the data base, companion topology scopes its own companion store, and every other row runs scratch-only; Persistence consumes the resolved record and derives no path; host-document identity enters as one extra attribute row where the descriptor carries `Document`; the resource triple is `service.namespace` `rasm`, `service.name` the `TelemetryDomain.Qualify` render of the application row, and `service.instance.id` as pid joined with the start instant — the qualified name is load-bearing because a metrics store maps a subset of resource attributes onto series labels, so a store dropping `service.namespace` still separates this estate's emitters from a foreign `service.name`, and the qualifier rather than a local concatenation owns it so an already-prefixed or PascalCase application id lands one dotted lowercase spelling instead of two; `deployment.environment.name` is the live semconv spelling and the bare `deployment.environment` key is the deprecated form no exporter re-introduces; `QueueRoot` is the ONLY durable-telemetry path any composition reads — an offline queue rooted at a container layer loses its tail on the next reschedule, a queue rooted at a shared store root corrupts on a second live instance, and a queue rooted at a base two co-resident processes share lets each drain the other's batches, so every arm answers residence here rather than at a consumer and `QueueRootVariable` is the one coordinate a deployment sets to declare the volume that survives it; deriving queue residence from `LocalStore` alone is the deleted form, because that column answers where a document store lives and disarms durable buffering on exactly the service and edge rows that always export; `HostResourceDetector` is the one resource-discovery seam and a hand-pushed attribute list at a provider builder is the deleted pattern, its `Admitted` narrowing scoped to the ONE collision seat order cannot answer — two rows inside a single detector's own attribute list, where no merge runs — because no pre-build narrowing defends a key the merge fold itself overwrites afterwards, which is exactly the case the prior whole-list scan missed.

```csharp signature
public sealed record ProfileRoots(string AppRoot, string ConfigRoot, Option<string> StoreRoot, string SupportRoot, Option<string> QueueRoot);

public static class ProfileIdentity {
    // Durable-telemetry disk is a DEPLOYMENT fact under the one config env prefix its owner declares, read raw
    // because roots resolve before any configuration source mounts. Containerized roots resolve a per-user
    // base into an image layer a reschedule erases and no in-process probe tells that apart from a mounted
    // volume, so the deploy plane names the surviving path or the composition opens no queue and reports none.
    public const string QueueRootVariable = ConfigSource.EnvPrefix + "TELEMETRY_QUEUE_ROOT";

    // TWO bases, because the platform answers twice and only one answer is right per root kind. Darwin collapses
    // `ApplicationData` and `LocalApplicationData` onto `~/Library/Application Support`; linux DIVERGES them onto
    // `$XDG_CONFIG_HOME` and `$XDG_DATA_HOME`; windows roams the first over a network profile and keeps the
    // second local. A document store, a durable OTLP queue, and a crash marker are data and take the local base
    // on every row — load-bearing on the service and edge topologies, which run linux exclusively — while user
    // settings are config and take the roaming one. One base for both was the darwin-shaped assumption.
    public static Fin<ProfileRoots> Roots(ConsumptionProfile profile, string applicationName, Option<RuntimeAttachment> attachment) =>
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) switch {
            ({ Length: > 0 } data, { Length: > 0 } config) =>
                Fin.Succ(Folded(profile, Path.Join(data, applicationName), Path.Join(config, applicationName), attachment)),
            ({ Length: > 0 }, _) =>
                Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.ApplicationData))),
            _ => Fin.Fail<ProfileRoots>(new ProfileFault.RootUnresolved(nameof(Environment.SpecialFolder.LocalApplicationData))),
        };

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

    // ONE collision this guard answers and one only: two rows inside a single detector's own attribute list,
    // where no merge runs and a duplicate key is an ambiguous `Resource`. Cross-detector precedence is SEAT
    // ORDER — `Resource.Merge` awards a colliding key to the incoming resource, so the enrich chain seats ahead
    // of this detector and loses, and the deploy-plane environment detector seats behind it and wins. The owned
    // key set derives from the minted rows themselves as a set lookup, so a new identity row closes over its own
    // key with no second roster to edit and no per-row scan of the whole mint.
    static ImmutableArray<KeyValuePair<string, object>> Admitted(
        ImmutableArray<KeyValuePair<string, object>> minted, ImmutableArray<KeyValuePair<string, object>> extra) =>
        minted.AddRange(extra.ExceptBy(minted.Select(static held => held.Key), static row => row.Key, StringComparer.Ordinal));

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
    static ProfileRoots Folded(ConsumptionProfile profile, string baseRoot, string configRoot, Option<RuntimeAttachment> attachment) =>
        (profile.Topology == DeploymentTopology.Companion, profile.LocalStore, attachment.Case) switch {
            (true, _, _) => Rooted(profile, baseRoot, configRoot, Some(Path.Join(baseRoot, "companion")), Some(Path.Join(baseRoot, "companion"))),
            (_, true, RuntimeAttachment.Integrating link) => Rooted(profile, baseRoot, configRoot, Some(link.SharedStoreRoot), Some(baseRoot)),
            (_, true, _) => Rooted(profile, baseRoot, configRoot, Some(Path.Join(baseRoot, "store")), Some(baseRoot)),
            _ => Rooted(profile, baseRoot, configRoot, None, None),
        };

    // Deploy coordinate OUTRANKS the local answer, so a service or edge row — the topologies that always
    // export and own no local store — arms its queue on the volume a deployment mounted rather than on the
    // one column that answers a document-store question. Both answers then scope by host key, so a parent and
    // its co-resident companion never lease and drain each other's batches under one mounted directory.
    static ProfileRoots Rooted(ConsumptionProfile profile, string baseRoot, string configRoot, Option<string> store, Option<string> local) {
        Option<string> deployed = Optional(Environment.GetEnvironmentVariable(QueueRootVariable))
            .Filter(static declared => declared.Length > 0);
        return new(baseRoot, configRoot, store, Path.Join(baseRoot, "support"),
            (deployed.IsSome ? deployed : local).Map(root => Path.Join(root, "otlp", profile.HostKey)));
    }
}
```

## [05]-[POWER_AND_FIDELITY]

- Owner: `PowerState` `[SmartEnum<string>]` the host power-source axis under the `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `ThermalPressure` `[SmartEnum<int>]` the thermal-budget ladder whose generated key IS the rank; `PowerReading` the probed triple carrying thermal as an OPTIONAL half; `PowerAuthority` `[SmartEnum<string>]` the platform row owning the read; `FidelityScale` the compute-fidelity policy record graded from one reading; `PowerCell` the atom-backed capsule holding the last ADMITTED reading; `PowerProbe` the delegate targets the authority rows bind, holding each platform's key spellings as named consts.
- Cases: 3 power rows — plugged, battery, low-battery; 4 thermal rows — nominal(0), fair(1), serious(2), critical(3); 4 authority rows — `Darwin` over IOKit power sources and `NSProcessInfo.thermalState`, `Windows` over `GetSystemPowerStatus` with NO thermal answer, `Linux` over the power-supply and thermal sysfs classes, `Absent` for every remaining platform; 4 `FidelityScale` grades spanning burst through conserve.
- Entry: `PowerAuthority.Platform` selects the row the running platform owns and `Read()` returns `Fin<PowerReading>`; `PowerReading.Of(PowerState, Option<ThermalPressure>, double)` returns `Fin<PowerReading>` — the one construction route, admitting the charge fraction finite and inside `[0, 1]` so no platform read's raw double reaches a ceiling comparison; `FidelityScale.Grade(PowerReading)` is the total projection into the profile the compute scheduler reads; `PowerCell.Refresh()` re-probes and returns the cell, so the health `Gauge` probe is the one sampling site and `PowerCell.Thermal` reads the rank `PressurePolicy.Grade` folds beside CPU and memory.
- Auto: a plugged host at nominal thermal pressure grades to the full burst profile; a low-battery or critical-thermal host grades to the sustained profile that caps parallelism and lowers the compute fidelity tier so the device stays within its energy and thermal budget; a reading whose authority measures power but publishes no thermal grades on the power arms alone rather than on a manufactured `Nominal`; a refused read holds the prior reading, and a cell that never admitted one grades `Balanced` — bursting on absent evidence is the fabricated full-charge grade the authority rows exist to refuse; the power state feeds the resource-pressure health contributor as one extra grade input so a thermally-throttled host degrades through the existing degradation rail, never a parallel power alarm.
- Receipt: `FidelityScale` carries the parallelism cap, the fidelity tier, and the sustained flag the compute scheduler reads; a power-state transition logs through one `SpineLog` event in the 1000-1099 EVENT stride (`FaultBand.SpineEvents`).
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one power row absorbs a new power source; one thermal row absorbs a new pressure level; one `PowerAuthority` row with its `PowerProbe` target absorbs a new platform authority; a new fidelity profile is one `FidelityScale` grade arm, never a parallel scaling owner; zero new surface.
- Boundary: the power-and-fidelity fold is the only energy-awareness owner — a per-solve battery check, an ad hoc thermal poll, and a parallel power monitor are the deleted forms; the fidelity scale is data the Compute scheduler reads to bound its `CpuBudget` and lane parallelism, so the host owns the power-state truth and the compute scheduler consumes the fidelity grade, never re-reading the power state; platform variance rides the `PowerAuthority` roster rather than a runtime `if` inside the probe, and a row whose read has not landed REFUSES — a synthesized plugged-at-nominal-at-full-charge triple is indistinguishable from a measured one at every consumer, which is why absence crosses as a typed refusal the cell holds against; THERMAL absence is a second axis of the same law and the reason the column is optional — windows publishes no user-mode thermal-pressure surface at all (WMI leaves `Win32_TemperatureProbe.CurrentReading` unpopulated by documented design, `MSAcpi_ThermalZoneTemperature` is an ACPI-driver class reporting a motherboard zone in tenths of Kelvin where the platform exposes one, `IOCTL_THERMAL_QUERY_INFORMATION` is a kernel DDI, and `EFFECTIVE_POWER_MODE` is a power-policy ladder that never escalates on heat), and a linux thermal zone publishing `temp` without trip points yields no ladder, so both refuse the half rather than grade `Nominal`; every native read is a hand-declared interop or file read — no managed package reports AC, battery, or thermal state on any RID, `Microsoft.Extensions.Diagnostics.ResourceMonitoring` owns process, container, disk, and network utilization and none of this, and macOS publishes no SMC surface so `NSProcessInfo.thermalState` IS the sanctioned darwin ladder; the capsule holds one atom and a `MeterListener` seat beside it is dead apparatus, because power and thermal state reach the process by native probe alone and publish no meter — the `UtilizationCell` listener is the resource-monitoring path and this cell never twins it; the power state enters the resource-pressure grade as a third input beside CPU and memory so a thermally-throttled host degrades on the same `Pressure`-tagged rule, never a new degradation level.

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
// int keys and a Rank column beside them is a second ordering that drifts. The four keys are also the four
// IMPLICIT ordinals `NSProcessInfoThermalState` declares — the enum names no explicit values, so the darwin
// probe casts the raw ordinal straight onto a key rather than mapping through a table nobody can verify.
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
// Thermal is OPTIONAL because measuring power and measuring heat are two capabilities and a platform can own
// one without the other: the windows arm reads AC state and charge and has no user-mode thermal surface at all,
// and a linux zone publishing `temp` with no trip points yields no ladder. A total column would force those arms
// to publish `Nominal`, which no consumer can tell from a measured nominal — the same fabricated-measurement
// shape the authority roster exists to refuse, one field over.
public readonly record struct PowerReading {
    private PowerReading(PowerState power, Option<ThermalPressure> thermal, double battery) =>
        (Power, Thermal, BatteryFraction) = (power, thermal, battery);

    public PowerState Power { get; }

    public Option<ThermalPressure> Thermal { get; }

    public double BatteryFraction { get; }

    public static Fin<PowerReading> Of(PowerState power, Option<ThermalPressure> thermal, double battery) =>
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

    // Thermal grades FIRST where it was measured, because heat is the ceiling a plugged host still hits; where
    // its authority published none, this fold drops straight to the power arms, so an unthermal platform grades
    // on evidence it genuinely holds rather than on a nominal it never read.
    public static FidelityScale Grade(PowerReading reading) =>
        reading.Thermal.Case switch {
            ThermalPressure heat when heat.Key >= ThermalPressure.Critical.Key => Conserve,
            ThermalPressure heat when heat.Key >= ThermalPressure.Serious.Key => Sustained,
            _ => Powered(reading),
        };

    static FidelityScale Powered(PowerReading reading) =>
        reading.Power == PowerState.LowBattery
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

public sealed class PowerCell(PowerAuthority authority) {
    private readonly Atom<Option<PowerReading>> cell = Atom(Option<PowerReading>.None);

    // TWO absences collapse here and both grade nominal: no reading admitted yet, and a reading whose authority
    // publishes no thermal half. The health grade escalates on measured heat alone, so an unmeasured host never
    // escalates on evidence nobody took; unread fidelity grades Balanced so it never bursts on the same absence.
    // Asymmetry is deliberate — absence must not escalate a health rule and must not unlock burst either.
    public ThermalPressure Thermal => cell.Value.Bind(static held => held.Thermal).IfNone(ThermalPressure.Nominal);

    public FidelityScale Read() => cell.Value.Map(FidelityScale.Grade).IfNone(FidelityScale.Balanced);

    // Refused probe HOLDS the last admitted reading: dropping back to absence lets one transient failure
    // grade a critically throttled host as unconstrained until the next successful read.
    public PowerCell Refresh() =>
        (ignore(cell.Swap(prior => authority.Read().Match(
            Succ: static reading => Some(reading),
            Fail: _ => prior))), this).Item2;
}

// Every key string is a named const off its platform's own header or ABI file, never an inline literal, so a
// spelling drifts in one place and a reader diffs the const against the source document. The `Absent` row states
// what a landing owes rather than a blank refusal, so a held reading's cause names its owing authority.
public static class PowerProbe {
    // IOKit `ps/IOPowerSources.h` + `ps/IOPSKeys.h`. `IOPSGetProvidingPowerSourceType` returns exactly one of
    // three strings; the description dictionary carries the state, capacity, and presence keys. Charge is
    // DERIVED — the header states clients divide current by max, so no fraction key exists to read, and both
    // sides are `CFNumber kCFNumberIntType`. `Time to Empty`/`Time to Full Charge` never enter: they read zero
    // on AC, which is an unmeasured value indistinguishable from a measured one, and the only sentinel-carrying
    // remaining-time source is `IOPSGetTimeRemainingEstimate` (-1 unknown, -2 unlimited), which this fold wants
    // for nothing. Thermal casts the raw `NSProcessInfoThermalState` ordinal straight onto a `ThermalPressure`
    // key: the enum declares no explicit values, so its four cases ARE 0-3 in declaration order.
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

    // `/sys/class/power_supply/<name>/` and `/sys/class/thermal/`, from the kernel sysfs ABI. `capacity` is the
    // one percent-native node (0-100 direct); the µAh/µWh `charge_*`/`energy_*` family is a class property the
    // ABI file does not carry and any of it may be skipped, so a ratio keyed to it reads a node that may not
    // exist. `present` INVERTS the usual convention — its absence means present — so a missing node is true here
    // and absence everywhere else. Thermal derives two ways and refuses if neither resolves: trip points wherever
    // one zone publishes them (both trip nodes are Optional), else its cooling-device ratio, whose three nodes
    // stay Required on every conforming host. A zone publishing `temp` alone yields NO ladder and grades nothing.
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

    // `Is Present && !Is Charging` is the DISCHARGE predicate the low band needs — `Power Source State` alone
    // reads `AC Power` on a charging laptop, which is true and useless here. `AC Power` and `UPS Power` both
    // grade plugged: a UPS host is on wall power through a buffer, and that buffer's own depletion is a
    // deployment alarm rather than a compute-fidelity input.
    static PowerState DarwinState(DarwinPower.Source battery) =>
        battery.State != BatteryPowerValue ? PowerState.Plugged
        : battery.Present && !battery.Charging && DarwinCharge(battery) < FidelityScale.BatteryReserve ? PowerState.LowBattery
        : PowerState.Battery;

    static double DarwinCharge(DarwinPower.Source battery) =>
        battery.MaxCapacity > 0 ? (double)battery.CurrentCapacity / battery.MaxCapacity : 0d;

    // `GetSystemPowerStatus` over `SYSTEM_POWER_STATUS` (winbase.h, Kernel32.dll) is the whole windows answer and
    // it is a raw interop declaration by necessity: no managed package on any RID reports AC or battery state.
    // `BatteryLifePercent` reads 255 for unknown and `ACLineStatus` 255 for unknown, so both sentinels refuse
    // rather than round to a number; `BatteryFlag` bit 4 is the critical band that narrows Battery to LowBattery.
    // Thermal is NONE by platform, not by omission — see the section Boundary for the four refuted candidates.
    public static Fin<PowerReading> Windows() =>
        WindowsPower.Status().Match(
            Some: status => PowerReading.Of(WindowsState(status), None, status.BatteryLifePercent / 100d),
            None: static () => Unresolved(PowerAuthority.Windows.Key, "a GetSystemPowerStatus read reporting a known AC state and charge"));

    // `BatteryFlag` is a FLAG WORD, not an enum — it reads 0 when the battery is neither charging nor at a named
    // band — so the low band tests bits 2 (low) and 4 (critical) rather than comparing the whole value.
    static PowerState WindowsState(WindowsPower.SystemPowerStatus status) =>
        status.ACLineStatus == WindowsPower.AcOnline ? PowerState.Plugged
        : (status.BatteryFlag & (WindowsPower.BatteryLow | WindowsPower.BatteryCritical)) != 0 ? PowerState.LowBattery
        : PowerState.Battery;

    public static Fin<PowerReading> Linux() =>
        LinuxPower.Battery().Match(
            Some: battery => PowerReading.Of(LinuxState(battery), LinuxPower.Thermal(), battery.Capacity / 100d),
            None: static () => Unresolved(PowerAuthority.Linux.Key, $"a {PowerSupplyRoot} row publishing {CapacityNode}"));

    // AC is read off the MAINS side, never inferred from the battery row: a host with no battery at all still
    // answers plugged through an `online` mains supply, and a battery reading `Discharging` on a machine whose
    // mains row is online is a charge-cycle artifact rather than an unplugged host.
    static PowerState LinuxState(LinuxPower.Supply battery) =>
        !LinuxPower.OnMains() && battery.Present && battery.Status == DischargingStatus
            ? (battery.Capacity < FidelityScale.BatteryReserve * 100 ? PowerState.LowBattery : PowerState.Battery)
            : PowerState.Plugged;

    public static Fin<PowerReading> Absent() =>
        Unresolved(PowerAuthority.Absent.Key, "a platform authority reporting battery charge and thermal pressure");

    static Fin<PowerReading> Unresolved(string authority, string requirement) =>
        Fin.Fail<PowerReading>(new ProfileFault.Text($"power-authority:{authority} requires {requirement}"));
}

// --- [NATIVE_SEAMS] -------------------------------------------------------------------------

// One shim per platform authority, each exposing TYPED reads alone so the fold above never touches a handle, a
// CoreFoundation type ref, or a struct layout. Each read returns `Option`, so an absent surface is absence at the
// seam rather than a zero the grade cannot tell from a measurement.
[SupportedOSPlatform("macos")]
public static partial class DarwinPower {
    private const string IOKit = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string ObjC = "/usr/lib/libobjc.A.dylib";
    private const uint Utf8Encoding = 0x08000100;
    private const nint IntType = 9;

    public readonly record struct Source(string State, int CurrentCapacity, int MaxCapacity, bool Charging, bool Present);

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

    // First `Type == InternalBattery` row decodes whole. Blob and list are COPIES this seam owns and releases on
    // every path; each description dictionary is a GET no caller may release.
    public static Option<Source> Battery() {
        IntPtr blob = IOPSCopyPowerSourcesInfo();
        if (blob == IntPtr.Zero) { return None; }
        IntPtr list = IOPSCopyPowerSourcesList(blob);
        try {
            for (nint index = 0; list != IntPtr.Zero && index < CFArrayGetCount(list); index++) {
                IntPtr row = IOPSGetPowerSourceDescription(blob, CFArrayGetValueAtIndex(list, index));
                if (row != IntPtr.Zero && Text(row, PowerProbe.SourceTypeKey) == PowerProbe.InternalBatteryType) {
                    return Some(new Source(
                        State: Text(row, PowerProbe.PowerSourceStateKey),
                        CurrentCapacity: Number(row, PowerProbe.CurrentCapacityKey),
                        MaxCapacity: Number(row, PowerProbe.MaxCapacityKey),
                        Charging: Flag(row, PowerProbe.IsChargingKey),
                        Present: Flag(row, PowerProbe.IsPresentKey)));
                }
            }
            return None;
        }
        finally {
            if (list != IntPtr.Zero) { CFRelease(list); }
            CFRelease(blob);
        }
    }

    // `NSProcessInfo.processInfo.thermalState` — the sanctioned macOS pressure ladder and the ONLY one: no public
    // SMC API exists, `pmset -g therm` records nothing on a live device, and no SDK header declares an AppleSMC
    // key surface. Ordinals cast straight onto a key because that ObjC enum declares no explicit values. Inside
    // this suite's Rhino host bundle, managed `NSProcessInfo.ThermalState` reads the same value with no interop;
    // outside it `Microsoft.macOS.dll` is absent, so this arm stays the portable one and that managed member
    // stays a Rhino-hosted option, never the default.
    public static Option<ThermalPressure> Thermal() =>
        objc_getClass("NSProcessInfo") is var cls && cls != IntPtr.Zero
            && ThermalPressure.TryGet((int)SendLong(Send(cls, sel_registerName("processInfo")), sel_registerName("thermalState")), out var row)
            ? Some(row)
            : None;

    // Keys cross as CFStrings this seam mints and releases; values are GETs it never owns. An unreadable key
    // yields the type's own absence — empty text, zero, false — which every caller above tests explicitly rather
    // than folding into a reading.
    private static IntPtr Value(IntPtr dictionary, string key) {
        IntPtr name = CFStringCreateWithCString(IntPtr.Zero, key, Utf8Encoding);
        try { return CFDictionaryGetValue(dictionary, name); }
        finally { CFRelease(name); }
    }

    private static string Text(IntPtr dictionary, string key) {
        Span<byte> buffer = stackalloc byte[128];
        return Value(dictionary, key) is var held && held != IntPtr.Zero
            && CFStringGetCString(held, buffer, buffer.Length, Utf8Encoding)
            && buffer.IndexOf((byte)0) is var terminator && terminator >= 0
            ? Encoding.UTF8.GetString(buffer[..terminator])
            : string.Empty;
    }

    private static int Number(IntPtr dictionary, string key) =>
        Value(dictionary, key) is var held && held != IntPtr.Zero && CFNumberGetValue(held, IntType, out var value) ? value : 0;

    private static bool Flag(IntPtr dictionary, string key) =>
        Value(dictionary, key) is var held && held != IntPtr.Zero && CFBooleanGetValue(held);
}

public static partial class WindowsPower {
    public const byte AcOffline = 0;
    public const byte AcOnline = 1;
    public const byte AcUnknown = 255;
    public const byte BatteryLow = 2;
    public const byte BatteryCritical = 4;
    public const byte PercentUnknown = 255;

    // winbase.h `SYSTEM_POWER_STATUS`, field-for-field. `BatteryLifeTime`/`BatteryFullLifeTime` read -1 on AC and
    // never enter the reading; `SystemStatusFlag` is the Windows 10 battery-saver bit, formerly `Reserved1`.
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

    // BOTH sentinels refuse. A 255 charge is documented unknown, and rounding it to 2.55 or clamping it to 1.0
    // hands the grade a full battery it never measured; a 255 AC state is the same absence one field over.
    [SupportedOSPlatform("windows")]
    public static Option<SystemPowerStatus> Status() =>
        GetSystemPowerStatus(out var status)
            && status.BatteryLifePercent != PercentUnknown
            && status.ACLineStatus != AcUnknown
            ? Some(status)
            : None;
}

// Pure BCL: every read is a text file under a kernel-published ABI path, so this seam declares no interop and the
// whole platform arm is `File.ReadAllText` over node names the ABI file fixes.
[SupportedOSPlatform("linux")]
public static class LinuxPower {
    // Cooling bands are the SHAPE of the fallback ladder: a normalized throttle ratio has no natural steps, so the
    // three fractions state where the ladder climbs and a fourth level is one row, never a new derivation.
    private static readonly Seq<(double Floor, ThermalPressure Level)> CoolingBands =
        Seq((0.9d, ThermalPressure.Critical), (0.6d, ThermalPressure.Serious), (0.3d, ThermalPressure.Fair));

    private static readonly Seq<(string Trip, ThermalPressure Level)> TripBands =
        Seq(("critical", ThermalPressure.Critical), ("hot", ThermalPressure.Serious), ("passive", ThermalPressure.Fair));

    public readonly record struct Supply(string Status, int Capacity, bool Present);

    // `present` INVERTS the usual convention: the ABI states an absent node means the battery IS present, so the
    // default is true and only an explicit `0` reads absent. `capacity` is the one percent-native node and a row
    // that skips it is skipped whole — the µAh/µWh family is not in the ABI file and may be absent on any device.
    public static Option<Supply> Battery() =>
        Rows().Filter(static row => Node(row, PowerProbe.TypeNode) == PowerProbe.BatterySupply)
            .Map(static row => (Row: row, Capacity: Reading(row, PowerProbe.CapacityNode)))
            .Filter(static pair => pair.Capacity.IsSome)
            .Map(static pair => new Supply(
                Status: Node(pair.Row, PowerProbe.StatusNode),
                Capacity: (int)pair.Capacity.IfNone(0),
                Present: Reading(pair.Row, PowerProbe.PresentNode).Map(static held => held != 0).IfNone(true)))
            .Head;

    // AC reads off the MAINS side. `online` admits 0 offline, 1 online fixed, 2 online programmable, so any
    // non-zero is powered; a host with no battery at all still answers here.
    public static bool OnMains() =>
        Rows().Filter(static row => Node(row, PowerProbe.TypeNode) != PowerProbe.BatterySupply)
            .Exists(static row => Reading(row, PowerProbe.OnlineNode).Exists(static held => held != 0));

    // TWO derivations, in order, then a refusal. Trip points give the natural three-step escalation, but both trip
    // nodes are ABI-OPTIONAL, so a zone publishing `temp` alone yields nothing here; its cooling-device ratio is
    // that fallback every conforming host publishes, since `type`, `cur_state`, and `max_state` stay Required.
    // Grading a bare temperature against a hardcoded ceiling is deleted — that ceiling varies per silicon.
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

    // Trip pairs are INDEXED, never named: `trip_point_Y_type` names its band and `trip_point_Y_temp` carries
    // that band's threshold, so a band resolves by scanning one zone's own indices rather than a fixed slot.
    static Option<long> Trip(string zone, string kind) =>
        toSeq(Directory.EnumerateFiles(zone, "trip_point_*_type"))
            .Filter(path => Text(path) == kind)
            .Bind(static path => Reading(Path.GetDirectoryName(path)!, Path.GetFileName(path).Replace("_type", "_temp")).ToSeq())
            .Head;

    static Seq<string> Rows() => Children(PowerProbe.PowerSupplyRoot);

    static Seq<string> Zones() => Children(PowerProbe.ThermalRoot).Filter(static path => Path.GetFileName(path).StartsWith("thermal_zone", StringComparison.Ordinal));

    static Seq<string> Devices() => Children(PowerProbe.ThermalRoot).Filter(static path => Path.GetFileName(path).StartsWith("cooling_device", StringComparison.Ordinal));

    static Seq<string> Children(string root) =>
        Directory.Exists(root) ? toSeq(Directory.EnumerateDirectories(root)) : Seq<string>();

    static string Node(string directory, string node) => Text(Path.Join(directory, node));

    static Option<long> Reading(string directory, string node) =>
        long.TryParse(Node(directory, node), CultureInfo.InvariantCulture, out var value) ? Some(value) : None;

    // Unreadable nodes are ABSENCE, never zero: a permission fault, a racing hot-unplug, and a driver skipping
    // its property all land here, and every caller above distinguishes empty text from a parsed value.
    static string Text(string path) =>
        Try.lift(() => File.ReadAllText(path).Trim()).Run().IfFail(string.Empty);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
