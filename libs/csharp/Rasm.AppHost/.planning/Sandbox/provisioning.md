# [APPHOST_PROVISIONING_AND_UPDATE]

Rasm.AppHost owns post-fetch updates through one `UpdateManager` state machine: download, supply-chain admission, staging, drain, restart, and phase receipts. `UpdateChannel` rows carry the release ring and its downgrade policy while the feed binds from configuration at boot. `FleetRoll` walks `MembershipView.Serving` through health-gated canary, blue-green, or linear waves selected by a `FlagVerdict`, under one `DistributedLock` so two nodes never drive overlapping waves. `UpdateCheck(ReleaseIdentity)` remains the outbound detect leg; this page owns every later phase.

Settled composition: `SupplyChainGate`/`AdmissionSubject` from Sandbox/admission#SUPPLY_CHAIN_GATE; `Lifecycle`, `DrainRow`, `DrainBand`, `DrainConductor.Drain`, and `DrainReceipt` from Runtime/lifecycle#DRAIN_CONDUCTOR; `DeadlineClass` and `ClockPolicy` from Runtime/time; `RolloutSegment` and `FlagVerdict` from Runtime/features#FLAG_DEFINITION; `MembershipView`/`MemberRecord` and `DistributedLock`/`LeaseKey`/`FencedRuntime`/`CoordinationSink`/`FenceHolding`/`CoordinationFault` from Wire/coordination; `WireHealth.Evaluate` and `HealthReport` from Observability/health#WIRE_HEALTH; `LatencySpine.Seal` and `ILatencyContext`/`CheckpointToken`/`ILatencyDataExporter` from Observability/telemetry#SIGNAL_GOVERNANCE; `InstrumentSet` and `AppHostSlot` from Observability/instruments; `ReceiptKind` from Observability/instruments#RECEIPT_PROJECTION; `TelemetryDomain` from Observability/telemetry#SIGNAL_GOVERNANCE; `InstrumentSpec`/`InstrumentKind`/`MeasureForm`/`Buckets` from `Rasm/Domain/instrument`, `BoardPack`/`PanelSpec`/`Objective` from `Rasm/Domain/objective`, and `TelemetryContributorPort` from `Rasm/Domain/telemetry#CONTRIBUTE`, so this page declares rows and mints no instrument vocabulary; `ReceiptSinkPort`/`TelemetrySource`/`TenantContext`/`CorrelationId` from Rasm/Domain/frame; `Fault`/`FaultBand`/`Op`/`IValidityEvidence` from Rasm/Domain/rails. Velopack owns the release lifecycle, Thinktecture the vocabularies, LanguageExt the rails.

## [01]-[INDEX]

- [02]-[UPDATE_RAIL]: Post-fetch state machine over one outcome family, its fault band, per-phase receipt, and generated instruments.
- [03]-[CHANNEL_AXIS]: Three feed rows binding explicit channel and downgrade policy onto options.
- [04]-[ROLLOVER_DRAIN]: Drain-before-swap handshake and the canary/blue-green/linear-wave `RollStrategy` axis over a lock-held, health-gated fleet wave.

## [02]-[UPDATE_RAIL]

- Owner: `UpdateOutcome` `[Union]` the terminal disposition and the SOLE authority for the phase it reports; `UpdatePhase` `[SmartEnum<string>]` the five wire-stable post-fetch phase keys the outcome projects onto; `UpdateFault` `[Union]` the fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Update`; `Code` derive SEALED); `UpdateReceipt` per-transition evidence implementing the kernel `IValidityEvidence` fold; `UpdateMetrics` the source-gen instrument partial under the `CounterAttribute`/`HistogramAttribute` generator, carrying the `TelemetryContributorPort` that DECLARES the family it mints; `UpdateRail` the boundary capsule owning the `UpdateManager` handle and the staged-pending probe.
- Cases: `UpdateOutcome` = `InFlight(Target)` | `StagedPending(Target)` | `Restarted(Target)` | `RolledBack(Option<string> Prior, generated FaultObservation Fault)` | `Declined(Reason)`; five phase rows — detected, downloading, staged, rolling-over, rolled-back; `UpdateFault` = DownloadBroken | StagePending | RolloverRejected | DowngradeBlocked | AdmissionRejected | FeedUnbound.
- Entry: `Stage(UpdateInfo found, IProgress<int> progress, CancellationToken token)` returns `IO<UpdateReceipt>` — carries the download-and-stage effect, forecloses a blocked downgrade and a release already staged for restart before transfer, and rails a broken transfer onto `DownloadBroken`; `Rollover(VelopackAsset asset, DrainThread drain)` returns `IO<UpdateReceipt>` — carries the drain-gated restart effect and rails a refused handoff onto `RolloverRejected`; `Resume(DrainThread drain)` returns `IO<UpdateReceipt>` — re-enters a staged-pending release after a process bounce, declining when nothing is pending; `UpdateMetrics.Port(string version)` is the declaration-only contributor port the composition root hands `InstrumentFan.Mount` at Runtime/modules, carrying this family on the `Published` column so governance, the view predicate, and the board read rows nobody re-binds.
- Law: phase and outcome were TWO projections of one transition and the receipt carried both, so the `IsValid` fold existed to prove they agreed; `UpdateOutcome.Phase` makes the disagreement unrepresentable — outcome decides, phase projects, and the coherence claims delete with the second column. `InFlight` names the exact incoherence the prior `Downloading` mint carried, a `StagedPending` outcome staking a staged claim on a transfer still running.
- Law: every phase transition mints ONE `UpdateReceipt` fanned to `ReceiptSinkPort.Send` under `ReceiptKind.Update` and the `TelemetrySource.AppHost` package row — sending the phase key as the receipt kind gave one wire shape five kinds, which no fan arm is writable against.
- Law: `IsDowngrade` is the SHIPPED flag and the hand `target.Version < CurrentVersion` comparison it replaced was strictly narrower — Velopack's flag also covers the LATERAL move, a re-published build at the same version, which a strict less-than reads as a forward roll and stages without the policy check; the `Downgrade` receipt column deletes with that re-derivation, having been written from the flag at four sites and read at none, and the fact it duplicated rides `UpdateFault.DowngradeBlocked` on the one arm where it decides anything.
- Law: ONE mint per instrument name. Generated partials bind these three handles on the meter this rail is handed, so the `AppHostMeasure` rows that once re-declared the same three names on the same meter were the second bind `Observability/instruments#RECEIPT_PROJECTION` names a forked stream; the family joins governance through `Port`'s `Published` column instead, which is the declaration surface a contributor that mounts nothing was given.
- Law: `Prior` rides `Option<string>` because `manager.CurrentVersion` is a foreign nullable and `?? string.Empty` made a missing installed version indistinguishable from a real empty one on the receipt every rollback reads; the probe is taken BEFORE the transfer and threaded into the terminal fold, so all three rollback arms carry the version a rollback returns to and the `None` two of them hard-spelled — an absence indistinguishable from a host with no installed version — has no seat.
- Law: three fault cases had no producing arm while the state diagram drew every edge — `DownloadUpdatesAsync` was awaited raw inside `IO.liftAsync` so a broken transfer threw out of the rail unhandled, `Rollover` carried no failure arm at all, and `StagePending` named a conflict nothing tested. Both transfer edges now rail through `Op.Catch`, and the third rides the `Foreclosed` fold's second arm, which refuses a transfer over a release already awaiting restart — the one act that overwrites the package `Resume` holds.
- Receipt: `UpdateReceipt` — the outcome, its projected phase, channel key, target version, delta count, `Instant`, elapsed `Duration`, and correlation id; a rollback lowers its live `UpdateFault` once through `FaultWire.Observe`, so the generated message carries typed recovery and bounded exact causes without serializing a LanguageExt error graph; the `Deltas >= 0` claim deletes because an array length never violates it.
- Packages: Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Microsoft.Extensions.Telemetry.Abstractions, Rasm (kernel `IValidityEvidence`/`ValidityClaim`/`Op`), BCL inbox
- Growth: one outcome case is one `Phase` arm and one `UpdateFault` case; a new phase key is one row the outcome roster projects onto; one instrument is one strongly-typed metric-attribute factory beside one `Published` row on `Port`, and one board tile is one `PanelSpec` on that port's pack; zero new surface.
- Boundary: `UpdateRail` is the named boundary capsule for the statement carve-out — the `UpdateManager` ctor, the awaited download, and the terminal `ApplyUpdatesAndRestart` carry language-owned statement forms while every other member stays expression-shaped; the rail composes `UpdateManager` directly with no rename adapter and the `UpdateChannel` axis is the only added vocabulary; `VelopackApp.Build()...Run()` is the process-entry bootstrap owned at the app root, never a rail fence, so `VelopackHook` registration stays there; `ApplyUpdatesAndRestart` takes `found.TargetFullRelease` as its `VelopackAsset`, never the `UpdateInfo`, and on success the call never returns because the host process is replaced — the rolled-over receipt mints and fans before it; `UpdateManager` publishes no disposal member at the catalogued surface, so the rail holds it for its own lifetime and implements no `IDisposable` it cannot honour; the feed url is the boot-resolved `FeedBinding`, never a row literal, so the rail dials only a proven configuration value; an inline `meter.CreateCounter` call is the deleted form — every spine instrument is a generated factory whose name and tag set are declaration facts; the `Target` fold reads `VelopackAsset.Version` through `ToString`, the single version-stamp seam; `UpdateReceipt` rides the suite wire law as one `AppHostWireContext` `[JsonSerializable]` row and `UpdateOutcome` carries the `[JsonDerivedType]` roster that registration demands, since a polymorphic member on a registered wire shape without one cannot round-trip its cases; `vpk`-side notarization and SBOM emission are build-time signing concerns and carry no rail fence, but `SupplyChainGate.Admit` verifies a downloaded release before staging as `AdmissionSubject.Release`, never a skipped step or a second gate; the page is host-local and crosses no browser or peer TS wire — `UpdateReceipt` and `FleetRollReceipt` reconstruct in TS as the `ReceiptHeaderWire` beside an AppHost family arm the corpus still owes, so this page authors no second wire shape.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UpdateFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Update;
    private UpdateFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record DownloadBroken(string Target, Error Cause) : UpdateFault($"{Target}: {Cause.Message}"), ICausedFault;
    [FaultCase(1)]
    public sealed partial record StagePending : UpdateFault { public StagePending(string detail) : base(detail) { } }
    [FaultCase(2)]
    public sealed partial record RolloverRejected(string Target, Error Cause) : UpdateFault($"{Target}: {Cause.Message}"), ICausedFault;
    [FaultCase(3)]
    public sealed partial record DowngradeBlocked : UpdateFault { public DowngradeBlocked(string detail) : base(detail) { } }
    [FaultCase(4)]
    public sealed partial record AdmissionRejected(string Target, Error Cause) : UpdateFault($"{Target}: {Cause.Message}"), ICausedFault;
    [FaultCase(5)]
    public sealed partial record FeedUnbound : UpdateFault { public FeedUnbound(string key) : base($"{key}: unset or not absolute") { } }
}

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UpdatePhase {
    public static readonly UpdatePhase Detected = new("detected");
    public static readonly UpdatePhase Downloading = new("downloading");
    public static readonly UpdatePhase Staged = new("staged");
    public static readonly UpdatePhase RollingOver = new("rolling-over");
    public static readonly UpdatePhase RolledBack = new("rolled-back");
}

// Outcome is the ONE authority and phase PROJECTS off it, so a receipt cannot record a state the machine
// cannot reach — a Staged phase carrying RolledBack, or a RolledBack phase claiming a restart. `InFlight`
// exists because the transfer transition previously reported `StagedPending`, staking a staged claim on bytes
// still moving; the advance arm that mapped phase to phase deletes with the second authority, since nothing
// advances a phase any more — the outcome names it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonDerivedType(typeof(InFlight), typeDiscriminator: "in-flight")]
[JsonDerivedType(typeof(StagedPending), typeDiscriminator: "staged-pending")]
[JsonDerivedType(typeof(Restarted), typeDiscriminator: "restarted")]
[JsonDerivedType(typeof(RolledBack), typeDiscriminator: "rolled-back")]
[JsonDerivedType(typeof(Declined), typeDiscriminator: "declined")]
public abstract partial record UpdateOutcome {
    private UpdateOutcome() { }
    public sealed record InFlight(string Target) : UpdateOutcome;
    public sealed record StagedPending(string Target) : UpdateOutcome;
    public sealed record Restarted(string Target) : UpdateOutcome;
    public sealed record RolledBack(
        Option<string> Prior,
        Rasm.Contracts.Fault.V1.FaultObservation Fault) : UpdateOutcome;
    public sealed record Declined(string Reason) : UpdateOutcome;

    public UpdatePhase Phase => Switch(
        inFlight: static _ => UpdatePhase.Downloading,
        stagedPending: static _ => UpdatePhase.Staged,
        restarted: static _ => UpdatePhase.RollingOver,
        rolledBack: static _ => UpdatePhase.RolledBack,
        declined: static _ => UpdatePhase.Detected);

    public static UpdateOutcome Reverted(Option<string> prior, Error fault) =>
        new RolledBack(prior, FaultWire.Observe(fault));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record UpdateReceipt(
    UpdateOutcome Outcome,
    string Channel,
    string Target,
    int Deltas,
    Instant At,
    Duration Elapsed,
    CorrelationId Correlation) : IValidityEvidence {
    public UpdatePhase Phase => Outcome.Phase;

    [JsonIgnore]
    public bool IsValid => ValidityClaim.All(
        Elapsed >= Duration.Zero,
        !string.IsNullOrEmpty(Channel)).Holds;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// Generated partials are the ONE mint of this family — a second bind on the same meter mints a second stream
// per name, so the `AppHostMeasure` twins of these three rows are the deleted form and governance joins through
// `Port` instead. The tag roster spells `AppHostSlot.Channel`'s own key rather than the C# type name: an attribute
// takes no computed value, and a `nameof(UpdateChannel)` dimension exported a key the governance view predicate
// never admits. Every attribute literal here is the generator's copy of a name the `Port` rows below derive.
public static partial class UpdateMetrics {
    [Counter("rasm.apphost.channel", Name = "rasm.apphost.update.staged")]
    public static partial StagedMetric Staged(Meter meter);

    [Counter("rasm.apphost.channel", Name = "rasm.apphost.update.rollback")]
    public static partial RollbackMetric Rollback(Meter meter);

    [Histogram("rasm.apphost.channel", Name = "rasm.apphost.update.rollover.duration")]
    public static partial RolloverDurationMetric RolloverDuration(Meter meter);

    // PUBLISHED, never mounted: the handles above already exist on the meter this rail is handed, so the port
    // declares the family for `SignalGovernance.Rostered`, the view predicate, and the board without asking any
    // root to bind a second handle. `Declared` is the proof surface either column feeds, which is why the tiles
    // these rows carry admit against the very names the partials mint.
    public static TelemetryContributorPort Port(string version) =>
        new(Scope: TelemetrySource.AppHost, Version: version, Instruments: Seq<InstrumentSpec>(),
            Published: Seq(StagedRow, RollbackRow, RolloverRow), Board: Some(Tiles));

    private static readonly InstrumentSpec StagedRow = InstrumentSpec.Create(
        TelemetryDomain.AppHost.Measure("update.staged"), InstrumentKind.Count, MeasureForm.Whole, "{release}",
        "releases staged for restart by update channel", Seq(AppHostSlot.Channel.Key), None, None, None);

    private static readonly InstrumentSpec RollbackRow = InstrumentSpec.Create(
        TelemetryDomain.AppHost.Measure("update.rollback"), InstrumentKind.Count, MeasureForm.Whole, "{release}",
        "update transitions rolled back by update channel", Seq(AppHostSlot.Channel.Key), None, None, None);

    private static readonly InstrumentSpec RolloverRow = InstrumentSpec.Create(
        TelemetryDomain.AppHost.Measure("update.rollover.duration"), InstrumentKind.Distribution, MeasureForm.Real,
        Buckets.Seconds, "drain-gated update rollover duration per channel", Seq(AppHostSlot.Channel.Key),
        Some(Buckets.CadenceSeconds), None, None);

    // Widgets stay absent so `PanelKind.For` derives each from its own row's measurement shape, exactly as the
    // sibling pack derives its tiles; the provenance key is this family's own, never the catalog roster's.
    private static readonly BoardPack Tiles = new(
        Wire: TelemetryDomain.AppHost.Measure("update"),
        Panels: Seq(
            new PanelSpec("updates staged", StagedRow.Name, StagedRow.Dimensions, None),
            new PanelSpec("update rollbacks", RollbackRow.Name, RollbackRow.Dimensions, None),
            new PanelSpec("update rollover", RolloverRow.Name, RolloverRow.Dimensions, None)),
        Objectives: Seq<Objective>());
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
public sealed class UpdateRail {
    readonly UpdateManager manager;
    readonly UpdateChannel channel;
    readonly Lifecycle host;
    readonly ReceiptSinkPort sink;
    readonly SupplyChainGate.Runtime gate;
    readonly StagedMetric staged;
    readonly RollbackMetric rollback;
    readonly RolloverDurationMetric rolloverDuration;

    // Binding arrives already resolved and validated, so the rail never reads configuration and never dials an
    // unproven host: a refused feed stopped at boot with a named key, and the ring travels ON the binding
    // rather than beside it, which is why no second channel parameter exists to disagree with it.
    public UpdateRail(FeedBinding feed, Lifecycle host, ReceiptSinkPort sink, SupplyChainGate.Runtime gate, Meter meter) {
        this.channel = feed.Channel;
        this.host = host;
        this.sink = sink;
        this.gate = gate;
        this.manager = new UpdateManager(feed.Feed.AbsoluteUri, new UpdateOptions {
            ExplicitChannel = channel.ExplicitChannel,
            AllowVersionDowngrade = channel.AllowVersionDowngrade,
        });
        this.staged = UpdateMetrics.Staged(meter);
        this.rollback = UpdateMetrics.Rollback(meter);
        this.rolloverDuration = UpdateMetrics.RolloverDuration(meter);
    }

    public Option<VelopackAsset> Pending => Optional(manager.UpdatePendingRestart);

    // Absence rides the Option: `?? string.Empty` made a host whose installed version cannot be read
    // indistinguishable from one whose version is genuinely blank, on the one column every rollback reads.
    Option<string> Prior => Optional(manager.CurrentVersion).Map(static version => version.ToString());

    // Both foreclosures decide before a byte moves and settle on one rollback receipt: a ring that forbids the
    // downgrade Velopack flagged, and a release already staged for restart — transferring over that one
    // overwrites the very package `Resume` is holding, which is what makes `StagePending` a reachable edge
    // rather than a drawn one. Order is deliberate: the ring refuses a downgrade even when nothing is pending.
    Option<UpdateFault> Foreclosed(UpdateInfo found) =>
        found.IsDowngrade && !channel.AllowVersionDowngrade
            ? Some((UpdateFault)new UpdateFault.DowngradeBlocked(Target(found)))
            : Pending.Map(static asset => (UpdateFault)new UpdateFault.StagePending(Target(asset)));

    public IO<UpdateReceipt> Stage(UpdateInfo found, IProgress<int> progress, CancellationToken token) =>
        Foreclosed(found).Match(
            Some: cause =>
              from blocked in Mint(Target(found), found.DeltasToTarget.Length, Duration.Zero,
                  UpdateOutcome.Reverted(Prior, cause))
              from _counted in IO.lift(() => rollback.Add(1, channel.Key))
              select blocked,
            None: () =>
              from start in IO.lift(() => host.Clocks.Now)
              // `Prior` is PROBED before the transfer and threaded into the terminal fold, because a static
              // fold reaches no manager — which is why two of the three rollback arms once published
              // `Prior = None` on the one column every rollback reader consults.
              from prior in IO.lift(() => Prior)
              from _moving in Mint(Target(found), found.DeltasToTarget.Length, Duration.Zero, new UpdateOutcome.InFlight(Target(found)))
              // Broken transfers are a DRAWN edge on this machine's own diagram and were unreachable: the await
              // sat bare inside `IO.liftAsync`, so a refused download escaped the rail as an exception and the
              // `DownloadBroken` case had no producer at all.
              from transferred in IO.liftAsync(async () =>
                  await Op.Of().Catch(async execution => {
                      await manager.DownloadUpdatesAsync(found, progress.Report, execution).ConfigureAwait(false);
                      return Fin.Succ(unit);
                  }, token))
              from admitted in transferred.Match(
                  Succ: _ => SupplyChainGate.Admit(gate, new AdmissionSubject.Release(found.TargetFullRelease, channel), token),
                  Fail: error => IO.pure<Validation<Error, SupplyChainReceipt>>(Fail<Error, SupplyChainReceipt>(error)))
              from finish in IO.lift(() => host.Clocks.Now)
              from receipt in Mint(Target(found), found.DeltasToTarget.Length, finish - start,
                  Settled(transferred, admitted, Target(found), prior))
              from _metered in IO.lift(() => receipt.Outcome is UpdateOutcome.StagedPending
                  ? staged.Add(1, channel.Key)
                  : rollback.Add(1, channel.Key))
              select receipt);

    public IO<UpdateReceipt> Rollover(VelopackAsset asset, DrainThread drain) =>
        from drained in host.Drain(
            DrainRows(), drain.Latency, drain.Checkpoint, drain.Instruments,
            DeadlineClass.DrainCooperative.Allotted)
        // Seal is the drain band's export act: the frozen ledger leaves through the exporter BEFORE the process
        // is replaced, so the last drain's checkpoints are evidence the successor reads, never lost memory.
        from _sealed in LatencySpine.Seal(drain.Exporter, drain.Latency)
        from _timed in IO.lift(() => rolloverDuration.Record((host.Clocks.Now - drained.At).TotalSeconds, channel.Key))
        from rolling in Mint(Target(asset), 0, drained.Elapsed, new UpdateOutcome.Restarted(Target(asset)))
        // This handoff never returns on success; a REFUSED handoff is the diagram's other unreachable edge and
        // rails here rather than throwing past a receipt that already claimed a restart.
        from handed in IO.lift(() => Op.Of().Catch(() => {
            manager.ApplyUpdatesAndRestart(asset);
            return Fin.Succ(unit);
        }, token: host.Spine.Token))
        from settled in handed.Match(
            Succ: _ => IO.pure(rolling),
            Fail: error => Mint(Target(asset), 0, drained.Elapsed,
                UpdateOutcome.Reverted(Prior, new UpdateFault.RolloverRejected(Target(asset), error))))
        select settled;

    public IO<UpdateReceipt> Resume(DrainThread drain) =>
        Pending.Match(
            Some: asset => Rollover(asset, drain),
            None: () => Mint(string.Empty, 0, Duration.Zero, new UpdateOutcome.Declined(nameof(Pending))));

    Seq<DrainRow> DrainRows() => [new(nameof(UpdateRail), DrainBand.Stores, 0, static _ => IO.pure(unit))];

    static UpdateOutcome Settled(
        Fin<Unit> transferred, Validation<Error, SupplyChainReceipt> admitted, string target, Option<string> prior) =>
        transferred.Match(
            Succ: _ => admitted.Match(
                Succ: _ => (UpdateOutcome)new UpdateOutcome.StagedPending(target),
                Fail: faults => UpdateOutcome.Reverted(prior, new UpdateFault.AdmissionRejected(target, faults))),
            Fail: error => UpdateOutcome.Reverted(prior, new UpdateFault.DownloadBroken(target, error)));

    IO<UpdateReceipt> Mint(string target, int deltas, Duration elapsed, UpdateOutcome outcome) =>
        from at in IO.lift(() => host.Clocks.Now)
        let receipt = new UpdateReceipt(outcome, channel.Key, target, deltas, at, elapsed, host.CorrelationId)
        from _fanned in sink.Send(host.CorrelationId, TenantContext.Current, TelemetrySource.AppHost,
            ReceiptKind.Update.Key, JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))
        select receipt;

    static string Target(UpdateInfo found) => Target(found.TargetFullRelease);

    static string Target(VelopackAsset asset) => asset.Version.ToString();
}
```

```mermaid
stateDiagram-v2
    accTitle: Update outcome state
    accDescr: Downloaded releases stage, roll over, or end in rollback, with every edge produced by an arm on the rail.
    [*] --> Detected
    Detected --> Downloading : Stage
    Detected --> RolledBack : DowngradeBlocked
    Detected --> RolledBack : StagePending
    Downloading --> Staged : admitted
    Downloading --> RolledBack : DownloadBroken
    Downloading --> RolledBack : AdmissionRejected
    Staged --> RollingOver : Rollover
    Staged --> Staged : Pending
    RollingOver --> [*] : Restarted
    RollingOver --> RolledBack : RolloverRejected
    RolledBack --> [*]
```

## [03]-[CHANNEL_AXIS]

- Owner: `UpdateChannel` `[SmartEnum<string>]` three release-ring rows under the `ComparerAccessors.StringOrdinal` accessor, carrying the explicit-channel string and the downgrade-allow column as INSTANCE columns; `FeedBinding` the config-resolved per-channel feed the rail dials.
- Cases: 3 channel rows — stable, beta, canary.
- Entry: `FeedBinding.Of(UpdateChannel channel, IConfiguration configuration)` returns `Fin<FeedBinding>` — the boot-time resolve of that channel's feed key through the ranked `ConfigSource` chain, refusing an unset or non-absolute value on the typed rail under the channel's own name.
- Law: this roster is the FOLDER'S PRECEDENT for a `[SmartEnum]` carrying its policy as instance columns — the row answers `ExplicitChannel` and `AllowVersionDowngrade` itself, so no parallel per-ring policy table and no key-to-policy `Switch` exists to drift from it. `Sandbox/solver#SOLVER_KIND` and `[04]`'s `RollStrategy` read the same shape.
- Law: the resolved row's `ExplicitChannel` seats `UpdateOptions.ExplicitChannel` and its `AllowVersionDowngrade` seats `UpdateOptions.AllowVersionDowngrade`, while the ctor url comes from the `FeedBinding` the composition resolved — those two declared columns and one bound value are the whole update-options surface the rail writes; `MaximumDeltasBeforeFallback` stays unset so the full-package fallback governs; canary alone admits a downgrade so a forward-rolled canary build reverts to its prior pin.
- Receipt: the channel key stamps `UpdateReceipt.Channel` and keys the `AddView` cardinality cap on every update instrument; a refused binding rides the boot `Fin` rail naming the channel and its key, never a receipt.
- Packages: Velopack, Microsoft.Extensions.Configuration, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one channel row carries one explicit-channel string, one downgrade column, and one config key its binding reads; a ring split lands as one row and one key, never a second axis; zero new surface.
- Boundary: the axis owns the release-RING decision and never the feed VALUE — a feed URI is a deploy fact differing per environment, per tenant, and per air-gapped mirror, so a literal frozen into a settled row is the deleted form twice over: it asserts a value no surface was read for, and it forecloses the offline and mirror cases the supply-chain gate exists to serve; the feed binds from the `Runtime/config#POLICY_VALUES` ranked source chain and validates at boot, so an unset or unreachable feed refuses on the typed rail under a named channel instead of surfacing later as a network fault from a dead host; the detect-leg `ReleaseIdentity.Feed` is the outbound poll URI of the `UpdateCheck` hop, a distinct value the axis never reads — and the identity-to-ring resolve that once sat here DELETES, because the ring the rail runs on is the one its `FeedBinding` carries and a second read of the same fact off a detect-leg record is a mirror with no producer; `ExplicitChannel` is the Velopack channel-suffix selector pinning which release set the manager resolves; `AllowVersionDowngrade` is the downgrade-policy column the rail reads before any transfer, never a per-call flag; the `AddView` rows at signal-governance cap update-instrument cardinality on the channel key so three channels cap at three series per instrument.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UpdateChannel {
    public static readonly UpdateChannel Stable = new("stable", explicitChannel: "stable", allowVersionDowngrade: false);
    public static readonly UpdateChannel Beta = new("beta", explicitChannel: "beta", allowVersionDowngrade: false);
    public static readonly UpdateChannel Canary = new("canary", explicitChannel: "canary", allowVersionDowngrade: true);

    public string ExplicitChannel { get; }
    public bool AllowVersionDowngrade { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
// Feeds BIND, they do not declare. One key per ring under the section root the config grammar mints from
// `nameof`, its suffix being the ring's own key, so a new ring reaches configuration with zero edit here. Boot
// resolves every ring the composition will dial: an absent key and a value that is not an absolute URI both
// refuse on the typed rail naming the ring AND the key an operator must set. A frozen literal could report
// neither — it dials whatever it was born with, and a host that stops resolving surfaces mid-transfer as a
// network fault with no configuration to correct.
public sealed record FeedBinding(UpdateChannel Channel, Uri Feed) {
    public const string Section = nameof(FeedBinding);

    public static string KeyOf(UpdateChannel channel) => $"{Section}:{channel.Key}";

    public static Fin<FeedBinding> Of(UpdateChannel channel, IConfiguration configuration) =>
        Uri.TryCreate(configuration[KeyOf(channel)], UriKind.Absolute, out Uri? feed)
            ? Fin.Succ(new FeedBinding(channel, feed))
            : Fin.Fail<FeedBinding>(new UpdateFault.FeedUnbound(KeyOf(channel)));
}
```

## [04]-[ROLLOVER_DRAIN]

- Owner: `DrainThread` the composition-supplied record carrying the conductor's telemetry tail; `RollVerdict` `[SmartEnum<string>]` the three wave verdicts; `RollStrategy` `[SmartEnum<string>]` the progressive-delivery axis with delegate-backed cohort planning over the features owner's `RolloutSegment` band, the `From(FlagVerdict)` verdict seat, and the shared advance fold; `FleetRuntime` the fleet-conductor dependency capsule; `FleetRollReceipt` the per-node wave evidence; `RollAnnotationWire` the per-wave deploy annotation; `FleetRoll` the lock-held wave conductor walking `MembershipView.Serving`.
- Cases: three roll strategies — `Canary` rolls the probe cohort its own band answers, then expands over the remainder, `BlueGreen` swaps a parallel half-fleet cohort on a health-pass, `LinearWave` advances fixed-percentage increments with a bake window between waves; `RollVerdict` = advanced | held | rolled-back.
- Entry: `Conduct(UpdateRail rail, VelopackAsset staged, DrainThread drain)` and `ConductPending(...)` DELETE — both were one-line forwardings onto `rail.Rollover` and `rail.Resume`, and `Resume` already matches the pending asset the guard re-tested; `Roll(FleetRuntime fleet, MembershipView membership, UpdateChannel channel, RollStrategy strategy, Op key)` returns `IO<Validation<Error, Seq<FleetRollReceipt>>>` — refuses an empty serving set under the caller's `Op` before anything is acquired, takes `LeaseKey.Lock(FleetRoll.Section)` for the rollover section, plans the cohorts from `strategy.Plan(membership.Serving)`, rolls each cohort under the fence, waits on the post-roll `WireHealth.Evaluate` serving probe, and bakes the strategy's inter-wave dwell before the next cohort admits.
- Law: one conductor per fleet is now STRUCTURAL — waves run inside `DistributedLock.Guard` over `LeaseKey.Lock(FleetRoll.Section)`, the fleet-wide `rollover-drain` name held as a page constant, so a second node contending the section reads `CoordinationFault.LockHeld` and rolls nothing, and a conductor whose lease lapses mid-wave surfaces `FenceRejected` instead of driving cohorts against another node's. Keying that lease on the caller's `Op` was the exclusion in name only: two conductors entering under two member names took two disjoint leases over one fleet.
- Law: one clock and one correlation thread the fleet wave. `FleetRoll` took an `IClock` beside the rail's `ClockPolicy` and minted a fresh correlation per node, which is exactly why the claim that a rollover receipt references its drain by correlation id never held; `FleetRuntime` carries both, so every per-node receipt and every wave annotation joins on the conductor's own root.
- Law: the wave verdict is a ROW rather than a bool beside three string constants. `Advances` answered a bool and the annotation fold re-derived a three-way disposition from it and a rollback scan, spelling three untyped tokens the annotation carried as `const string`; `RollVerdict` is one typed answer both the advance decision and the annotation read.
- Law: `Remaining` rides `Option<int>` on the annotation. Folding an empty cohort's absent tail to `0` published "fleet complete" on every dashboard reading it, and the `Remaining >= 0` receipt claim tells the two apart nowhere — absence on a registered wire shape rides the option-shaped carrier the suite's `OmitAbsent` modifier omits.
- Law: `BlueGreen` and `LinearWave` shared one byte-identical plan lambda differing only in their band and bake columns, so the fold lands ONCE as `Banded` and the two rows carry their columns; `Canary`'s lead-then-remainder plan is genuinely different and stays its own, reading the same band column through the same `Cohort` projection so no plan on this axis ignores the exposure it was handed. `RollPlan` deletes with them — it copied `Bake` into `BakeWindow` and existed to carry a value the strategy row already answers.
- Law: the annotation carries its rows TYPED. Its `Channel`, `Strategy`, and `Verdict` columns were hand-projected to strings by a seven-member mint, and the suite wire law already generates that projection for every `[SmartEnum<string>]` — a transcription beside a converter is the twin, whether hand-written or generated.
- Law: cohort planning composes the features owner's `[0,100)` value object, so a wave width and a flag rollout segment are ONE percentage vocabulary the estate validates once; a page-local `Width(count, percent)` helper is the ad-hoc percentage-rollout computation `Runtime/features#FLAG_DEFINITION` names deleted, and it silently admitted out-of-band literals the value object's factory refuses at type init.
- Receipt: each cohort node mints one `FleetRollReceipt` — wave index, strategy key, node id, the node's terminal `UpdateOutcome`, post-roll serving status, and the live nodes-remaining countdown (the un-rolled tail of the current cohort and every later cohort, never the constant fleet count) — fanned beside the per-node `UpdateReceipt`; each wave folds one `RollAnnotationWire` under `ReceiptKind.Roll`, the deploy annotation the estate dashboard timeline marks every fleet wave from, so a rollback annotates as loudly as an advance and a parallel deploy-event sender is the deleted form.
- Packages: Velopack, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (kernel `IValidityEvidence`/`ValidityClaim`/`InstrumentSet`/`Op`), BCL inbox
- Growth: one drain participant row per update-sensitive subsystem registered through `DrainParticipantPort` at its declared band; a new progressive strategy is one `RollStrategy` row with its plan delegate and policy columns, never a second roll state machine; a wave-width retune is the row's `RolloutSegment` band; zero new surface.
- Boundary: drain-before-swap is the law — `ApplyUpdatesAndRestart` is never reached until `DrainConductor.Drain` settles, so the replaced process leaves no half-flushed store write or in-flight hop; the cooperative and forced budgets are the conductor's OWN `DeadlineClass` rows and no longer travel on the thread record, because the fold reads them itself and two call sites carrying them disagree; the latency context, checkpoint token, instrument set, and ledger exporter arrive on the `DrainThread` from the composition root, so this page consumes the drain fold and declares none of its telemetry threads; the staged asset is `UpdatePendingRestart` read at composition, so a rollover after a process bounce resumes from the staged phase without re-staging; the rollover is the single restart path and the bare `ApplyUpdatesAndExit` and `WaitExitThenApplyUpdates` forms are deleted because the drain-gated restart owns the handoff; `RollStrategy` is one row on the existing `FleetRoll`, not a parallel conductor — a second roll state machine or a strategy-specific scheduler beside `ScheduleEntry.Spread` is the rejected form, and the `ScheduleEntry.Spread` fleet-spread seed stays the wave-pacing cadence the strategy `Bake` reads; the bake dwell rides the injected clock-driven delegate on the runtime capsule, never an ambient `Task.Delay`, so a `LinearWave` bakes its window and a `Canary` holds its probe deterministically under the same `TimeProvider` the spine injects; `FleetRoll` consumes `MembershipView.Serving` as fleet membership and the `WireHealth` serving projection as the recovery gate; each node rolls through the same `rail.Rollover`, and the first unrecovered node halts the fleet.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// Drain thread the composition root owns, travelling as ONE record because every rollover call site needs
// all four and the conductor's own signature proves it: the latency context it marks against, the checkpoint
// token bounding the flush, the instrument set the band distribution writes, and the exporter the seal drains
// to arrive from `Observability/telemetry#SIGNAL_GOVERNANCE` and `Observability/instruments#INSTRUMENT_CATALOG`,
// never minted here. Cooperative and forced deadlines LEFT this record when the conductor took them onto its
// own `DeadlineClass` rows — a budget travelling beside the fold that owns it is a second value to disagree.
public sealed record DrainThread(
    ILatencyContext Latency,
    CheckpointToken Checkpoint,
    InstrumentSet Instruments,
    ILatencyDataExporter Exporter);

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RollVerdict {
    public static readonly RollVerdict Advanced = new("advanced");
    public static readonly RollVerdict Held = new("held");
    public static readonly RollVerdict RolledBack = new("rolled-back");
}

// Each row carries one cohort plan, one exposure band, and one inter-wave dwell. `Plan` folds the roster into
// one ordered cohort sequence per strategy shape, and `Verdict` grades the cohort that ran. A second
// roll state machine beside this axis is the rejected form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RollStrategy {
    public static readonly RollStrategy Canary = new("canary", wave: RolloutSegment.Create(1), bake: Duration.FromSeconds(120), plan: Led);
    public static readonly RollStrategy BlueGreen = new("blue-green", wave: RolloutSegment.Create(50), bake: Duration.Zero, plan: Banded);
    public static readonly RollStrategy LinearWave = new("linear-wave", wave: RolloutSegment.Create(25), bake: Duration.FromSeconds(300), plan: Banded);

    // Unknown variants and an absent features rail both fall to Canary, the narrowest exposure, because an
    // unresolved verdict must never widen a fleet rollout past the safest wave.
    public static readonly RollStrategy Default = Canary;

    // Exposure bands are the features owner's own [0,100) value object, so a wave width and a flag rollout
    // segment are ONE percentage vocabulary the estate validates once. Every plan reads the band it is handed,
    // canary included: a zero band answers a zero cohort width, which would fold the probe into the remainder
    // and roll the whole fleet in one wave, so canary's 1 is the column that keeps the probe a probe.
    public RolloutSegment Wave { get; }
    public Duration Bake { get; }

    // Features decides WHICH strategy a wave runs and this page decides what the strategy does, so a
    // verdict's variant keys the row exactly as `ModelRoute.From` keys a model route off the same seam.
    public static RollStrategy From(FlagVerdict verdict) =>
        TryGet((string)verdict.Variant, out RollStrategy? row) ? row : Default;

    [UseDelegateFromConstructor]
    public partial Seq<Seq<MemberRecord>> Plan(Seq<MemberRecord> nodes, RolloutSegment wave);

    public Seq<Seq<MemberRecord>> Plan(Seq<MemberRecord> nodes) => Plan(nodes, Wave);

    // One verdict, three causes, read by both the advance decision and the wave annotation: a rollback anywhere
    // in the cohort holds the wave loudly, a full restart-and-serving cohort advances, and everything else —
    // staged-pending, declined, or a node that came back NotServing — holds.
    public RollVerdict Verdict(Seq<FleetRollReceipt> cohort) =>
        cohort.Exists(static row => row.Outcome is UpdateOutcome.RolledBack) ? RollVerdict.RolledBack
        : cohort.ForAll(static row => row.Outcome is UpdateOutcome.Restarted && row.Serving == ServingStatus.Serving) ? RollVerdict.Advanced
        : RollVerdict.Held;

    // Canary is a PROBE plus the remainder, and the probe's width is the band's own `Cohort` projection: the
    // segment floors a nonzero percentage to one node, so a five-node fleet probes one and a three-hundred-node
    // fleet probes three under the same declared column. Discarding `wave` here froze the probe at a hardcoded
    // head and made this row's band decorative on the one strategy whose exposure it decides.
    static Seq<Seq<MemberRecord>> Led(Seq<MemberRecord> nodes, RolloutSegment wave) =>
        Seq(nodes.Take(wave.Cohort(nodes.Count)).ToSeq(), nodes.Skip(wave.Cohort(nodes.Count)).ToSeq())
            .Filter(static cohort => !cohort.IsEmpty);

    // BlueGreen and LinearWave differed by their band and their dwell alone, so the plan lands ONCE and the two
    // rows carry their columns; two byte-identical lambdas is the per-instance body the density law names.
    static Seq<Seq<MemberRecord>> Banded(Seq<MemberRecord> nodes, RolloutSegment wave) =>
        nodes.IsEmpty ? [] : toSeq(nodes.Chunk(int.Max(1, wave.Cohort(nodes.Count))).Select(static cohort => toSeq(cohort)));
}

// --- [SERVICES] -----------------------------------------------------------------------------
// Fleet conductors thread their whole dependency set on ONE record: the lock runtime and its receipt fan, the roll,
// probe, and bake closures the composition supplies, and the single clock and correlation every receipt on the
// wave stamps. Threading nine loose parameters is what let a fresh `Correlation.Mint()` per node stand beside
// this rail's own root while the receipt line claimed the two joined.
public sealed record FleetRuntime(
    FencedRuntime Fence,
    CoordinationSink Coordination,
    Func<MemberRecord, IO<UpdateReceipt>> RollNode,
    Func<MemberRecord, IO<HealthReport>> Probe,
    Func<Duration, IO<Unit>> Bake,
    ReceiptSinkPort Sink,
    ClockPolicy Clocks,
    CorrelationId Correlation,
    TenantContext Tenant);

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct FleetRollReceipt(
    int Wave,
    RollStrategy Strategy,
    int NodeId,
    UpdateOutcome Outcome,
    ServingStatus Serving,
    int Remaining,
    Instant At) : IValidityEvidence {
    // Remaining counts DOWN across the whole plan, so a negative value or a wave index below zero is a fold
    // that mis-counted its own tail rather than a fleet state.
    [JsonIgnore]
    public bool IsValid => ValidityClaim.All(Wave >= 0, Remaining >= 0).Holds;
}

// One record per wave folded off the roll receipts, HLC-stamped on the receipt fan under `ReceiptKind.Roll` so
// this estate's dashboard timeline marks every fleet wave beside stack deploys. Its rows ride TYPED: the suite
// wire law generates the key projection for every smart-enum, so the seven-member hand mint that stringified
// them was a transcription of a converter that already exists. `Remaining` tails the list carrying `= default`
// because an empty cohort has no live tail to report and a zero there reads as a completed fleet on every
// dashboard — the `OmitAbsent` modifier drops the absent slot at write, and a slot without a default reads
// back wire-required under `RespectRequiredConstructorParameters`.
public readonly record struct RollAnnotationWire(
    int Wave,
    UpdateChannel Channel,
    RollStrategy Strategy,
    RollVerdict Verdict,
    int HostCount,
    Instant At,
    Option<int> Remaining = default);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class FleetRoll {
    // Sections are FLEET-WIDE names, so this one is a page constant on the one keyed-registry key owner rather
    // than the caller's `Op`: keying the lock on the calling member handed two conductors entering under two
    // member names two disjoint "exclusive" leases and let both drive one fleet. Caller `Op` keys this fold's
    // own fault attribution and never the section, which is what keeps it from widening the lock space.
    public const string Section = "rollover-drain";

    // Waves run INSIDE the fenced section: acquisition names the rollover lock on the one keyed-registry
    // key owner, the guard brackets every cohort with the lease read, and a contended acquire rolls nothing.
    // Two nodes driving overlapping waves was foreclosed in prose and by nothing else.
    public static IO<Validation<Error, Seq<FleetRollReceipt>>> Roll(
        FleetRuntime fleet, MembershipView membership, UpdateChannel channel, RollStrategy strategy, Op key) =>
        // Waves over an empty serving set advanced no node and answered an empty receipt sequence, which every
        // reader takes for a completed fleet — the same absent-tail confusion the `Remaining` law names one
        // record above. Refusal rides the caller's own `Op`, adopted onto this page's fault family.
        membership.Serving.IsEmpty
            ? IO.pure(Fail<Error, Seq<FleetRollReceipt>>(
                CoordinationFault.Of(key.InvalidInput(nameof(MembershipView.Serving)))))
            : DistributedLock.Acquire(fleet.Fence, fleet.Coordination, LeaseKey.Lock(Section)).Bind(acquired => acquired.Match(
                Succ: held => DistributedLock
                    .Guard(fleet.Fence, held, Wave(fleet, strategy.Plan(membership.Serving), 0, channel, strategy))
                    .Bind(rolled => DistributedLock.Release(fleet.Fence, fleet.Coordination, held).Map(_ => rolled)),
                Fail: faults => IO.pure(Fail<Error, Seq<FleetRollReceipt>>(faults))));

    static IO<Seq<FleetRollReceipt>> Wave(
        FleetRuntime fleet, Seq<Seq<MemberRecord>> cohorts, int index, UpdateChannel channel, RollStrategy strategy) =>
        cohorts.Head.Match(
            Some: cohort => cohort.Map(static (node, slot) => (Node: node, Slot: slot))
                .TraverseM(pair =>
                    from rolled in fleet.RollNode(pair.Node)
                    from report in fleet.Probe(pair.Node)
                    // Remaining counts down live — the un-rolled tail of this cohort plus every later cohort.
                    let receipt = new FleetRollReceipt(index, strategy, pair.Node.NodeId, rolled.Outcome, Serving(report),
                        cohort.Count - pair.Slot - 1 + cohorts.Tail.Sum(static rest => rest.Count), fleet.Clocks.Now)
                    from _fanned in fleet.Sink.Send(fleet.Correlation, fleet.Tenant, TelemetrySource.AppHost,
                        ReceiptKind.Roll.Key, JsonSerializer.SerializeToElement(receipt, SuiteContracts.Host))
                    select receipt)
                .As()
                .Bind(here =>
                    from _annotated in fleet.Sink.Send(fleet.Correlation, fleet.Tenant, TelemetrySource.AppHost,
                        ReceiptKind.Roll.Key, JsonSerializer.SerializeToElement(
                            new RollAnnotationWire(index, channel, strategy, strategy.Verdict(here), here.Count,
                                fleet.Clocks.Now, here.Last.Map(static row => row.Remaining)),
                            SuiteContracts.Host))
                    from rest in strategy.Verdict(here) == RollVerdict.Advanced && !cohorts.Tail.IsEmpty
                        ? (strategy.Bake > Duration.Zero ? fleet.Bake(strategy.Bake) : IO.pure(unit))
                            .Bind(_ => Wave(fleet, cohorts.Tail, index + 1, channel, strategy))
                        : IO.pure(Seq<FleetRollReceipt>())
                    select here + rest),
            None: static () => IO.pure(Seq<FleetRollReceipt>()));

    static ServingStatus Serving(HealthReport report) =>
        report.Status == HealthStatus.Unhealthy ? ServingStatus.NotServing : ServingStatus.Serving;
}
```

```mermaid
sequenceDiagram
    accTitle: Drain-gated restart handoff
    accDescr: UpdateRail drains the node, seals the latency ledger, records rollover, and hands restart to Velopack.
    participant Rail as UpdateRail
    participant Drain as DrainConductor
    participant Velopack as UpdateManager
    Rail->>Drain: Drain(rows, latency, checkpoint, instruments)
    Drain-->>Rail: DrainReceipt
    Rail->>Rail: Mint(Restarted) -> ReceiptSinkPort
    Rail->>Velopack: ApplyUpdatesAndRestart(staged)
    Note over Velopack: process replaced, call never returns
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
