# [APPHOST_PROVISIONING_AND_UPDATE]

Rasm.AppHost owns post-fetch updates through one `UpdateManager` state machine: download, supply-chain admission, staging, drain, restart, and phase receipts. `UpdateChannel` rows carry feed and downgrade policy. `FleetRoll` walks `MembershipView.Serving` through health-gated canary, blue-green, or linear waves. `UpdateCheck(ReleaseIdentity)` remains the outbound detect leg; this page owns every later phase over Velopack, `SupplyChainGate`, `DrainConductor`, `ReceiptSinkPort`, and generated metrics.

## [01]-[INDEX]

- [01]-[UPDATE_RAIL]: Post-fetch state machine, fault band, per-phase receipt, and generated instruments.
- [02]-[CHANNEL_AXIS]: Three feed rows binding explicit channel and downgrade policy onto options.
- [03]-[ROLLOVER_DRAIN]: Drain-before-swap handshake and the canary/blue-green/linear-wave `RollStrategy` axis over a health-gated fleet-wide wave.

## [02]-[UPDATE_RAIL]

- Owner: `UpdatePhase` `[SmartEnum<string>]` five post-fetch phases under the `ComparerAccessors.StringOrdinal` accessor; `UpdateOutcome` `[Union]` terminal disposition; `UpdateFault` `[Union]` fault family deriving its codes through `FaultBand.Update`; `UpdateReceipt` per-phase evidence record; `UpdateMetrics` source-gen instrument partial under the `CounterAttribute`/`HistogramAttribute` generator; `UpdateRail` boundary capsule owning the `UpdateManager` handle and the staged-pending probe.
- Cases: 5 phase rows — detected, downloading, staged, rolling-over, rolled-back; outcomes restarted | staged-pending | rolled-back | declined; `UpdateFault` = Text | DownloadBroken | StagePending | RolloverRejected | DowngradeBlocked.
- Entry: `IO<UpdateReceipt> Stage(UpdateInfo found, IProgress<int> progress, CancellationToken token)` carries the download-and-stage effect and forecloses a blocked downgrade before transfer; `IO<UpdateReceipt> Rollover(VelopackAsset asset, Duration cooperative, Duration forced)` carries the drain-gated restart effect; `IO<UpdateReceipt> Resume(Duration cooperative, Duration forced)` re-enters a staged-pending release after a process bounce.
- Auto: every phase commit mints one `UpdateReceipt` fanned to `ReceiptSinkPort.Send` under the `Rasm.AppHost` package key; the generated counter rises per staged and per rollback phase and the generated histogram records the rollover span; `IsUpdatePendingRestart` is read at boot so a staged-but-unrestarted release re-enters the rail at the staged phase without a second download.
- Receipt: `UpdateReceipt` — phase, channel key, target version, prior version, downgrade flag, delta count, `Instant`, elapsed `Duration`, outcome, correlation id.
- Packages: Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Microsoft.Extensions.Telemetry.Abstractions, BCL inbox.
- Growth: one phase row and its `Next` arm, one outcome case, or one fault case breaks every dispatch site at compile time; one instrument is one strongly-typed metric-attribute factory; zero new surface.
- Boundary: `UpdateRail` is the named boundary capsule for the statement carve-out — the `UpdateManager` ctor, the awaited download, and the terminal `ApplyUpdatesAndRestart` carry language-owned statement forms while every other member stays expression-shaped; the rail composes `UpdateManager` directly with no rename adapter — the `UpdateChannel` axis is the only added vocabulary; `VelopackApp.Build()...Run()` is the process-entry bootstrap owned at the app root, never a rail fence, so `VelopackHook` registration stays at the app root and never enters this page; `ApplyUpdatesAndRestart` takes `found.TargetFullRelease` as its `VelopackAsset`, never the `UpdateInfo`, and the call never returns because the host process is replaced — the rolled-over receipt mints and fans before the call; `found.IsDowngrade` against the channel's `AllowVersionDowngrade` column forecloses a disallowed downgrade as `DowngradeBlocked` before any byte transfers; an inline `meter.CreateCounter` call is the deleted form — every spine instrument is a generated factory whose name and tag set are declaration facts and whose generated metric type exposes the strongly-typed `Add`/`Record` over the channel-key tag; the `Target` fold reads `VelopackAsset.Version` (a `SemanticVersion`) through `ToString`, the single version-stamp seam; `UpdateReceipt` rides the suite wire law as one `AppHostWireContext` `[JsonSerializable]` row; `vpk`-side notarization and SBOM emission are build-time signing concerns and carry no rail fence, but `SupplyChainGate.Admit` verifies a downloaded release before staging as `AdmissionSubject.Release`, never a skipped step or second gate; the page is host-local and crosses no browser or peer TS wire — `UpdateReceipt` and `FleetRollReceipt` reconstruct in TS solely through `ReceiptEnvelopeWire`, so the page authors no second wire shape.

```csharp signature

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

[Union]
public abstract partial record UpdateOutcome {
    private UpdateOutcome() { }
    public sealed record Restarted(string Target) : UpdateOutcome;
    public sealed record StagedPending(string Target) : UpdateOutcome;
    public sealed record RolledBack(string Prior, UpdateFault Cause) : UpdateOutcome;
    public sealed record Declined(string Reason) : UpdateOutcome;
}

[Union]
public abstract partial record UpdateFault : Expected, IValidationError<UpdateFault> {
    private UpdateFault(string detail, int code) : base(detail, code, None) { }
    public static UpdateFault Create(string message) => new Text(message);
    public sealed record Text : UpdateFault { public Text(string detail) : base(detail, FaultBand.Update.Code(0)) { } }
    public sealed record DownloadBroken : UpdateFault { public DownloadBroken(string detail) : base(detail, FaultBand.Update.Code(1)) { } }
    public sealed record StagePending : UpdateFault { public StagePending(string detail) : base(detail, FaultBand.Update.Code(2)) { } }
    public sealed record RolloverRejected : UpdateFault { public RolloverRejected(string detail) : base(detail, FaultBand.Update.Code(3)) { } }
    public sealed record DowngradeBlocked : UpdateFault { public DowngradeBlocked(string detail) : base(detail, FaultBand.Update.Code(4)) { } }
    public sealed record AdmissionRejected : UpdateFault { public AdmissionRejected(string detail) : base(detail, FaultBand.Update.Code(5)) { } }
}

public sealed record UpdateReceipt(
    UpdatePhase Phase,
    string Channel,
    string Target,
    string Prior,
    bool Downgrade,
    int Deltas,
    Instant At,
    Duration Elapsed,
    UpdateOutcome Outcome,
    CorrelationId Correlation);

public static partial class UpdateMetrics {
    [Counter(nameof(UpdateChannel), Name = "rasm.apphost.update.staged")]
    public static partial StagedMetric Staged(Meter meter);

    [Counter(nameof(UpdateChannel), Name = "rasm.apphost.update.rollback")]
    public static partial RollbackMetric Rollback(Meter meter);

    [Histogram(nameof(UpdateChannel), Name = "rasm.apphost.update.rollover.duration")]
    public static partial RolloverDurationMetric RolloverDuration(Meter meter);
}

public sealed class UpdateRail {
    readonly UpdateManager manager;
    readonly UpdateChannel channel;
    readonly Lifecycle host;
    readonly ReceiptSinkPort sink;
    readonly SupplyChainGate.Runtime gate;
    readonly StagedMetric staged;
    readonly RollbackMetric rollback;
    readonly RolloverDurationMetric rolloverDuration;

    public UpdateRail(UpdateChannel channel, Lifecycle host, ReceiptSinkPort sink, SupplyChainGate.Runtime gate, Meter meter) {
        this.channel = channel;
        this.host = host;
        this.sink = sink;
        this.gate = gate;
        this.manager = new UpdateManager(channel.Feed.AbsoluteUri, new UpdateOptions {
            ExplicitChannel = channel.ExplicitChannel,
            AllowVersionDowngrade = channel.AllowVersionDowngrade,
        });
        this.staged = UpdateMetrics.Staged(meter);
        this.rollback = UpdateMetrics.Rollback(meter);
        this.rolloverDuration = UpdateMetrics.RolloverDuration(meter);
    }

    public bool PendingRestart => manager.IsUpdatePendingRestart;
    public Option<VelopackAsset> Pending => Optional(manager.UpdatePendingRestart);
    string Prior => manager.CurrentVersion?.ToString() ?? string.Empty;

    public IO<UpdateReceipt> Stage(UpdateInfo found, IProgress<int> progress, CancellationToken token) =>
        Downgrade(found.TargetFullRelease) && !channel.AllowVersionDowngrade
            ? from blocked in Mint(UpdatePhase.RolledBack, Target(found.TargetFullRelease), Downgrade(found.TargetFullRelease), found.DeltasToTarget.Length, Duration.Zero, new UpdateOutcome.RolledBack(Prior, new UpdateFault.DowngradeBlocked(Target(found.TargetFullRelease))))
              from _ in IO.lift(() => rollback.Add(1, channel.Key))
              select blocked
            : from start in IO.lift(() => host.Clock.GetCurrentInstant())
              from downloading in Mint(UpdatePhase.Downloading, Target(found.TargetFullRelease), Downgrade(found.TargetFullRelease), found.DeltasToTarget.Length, Duration.Zero, new UpdateOutcome.StagedPending(Target(found.TargetFullRelease)))
              from done in IO.liftAsync(async () => {
                  await manager.DownloadUpdatesAsync(found, progress.Report, token).ConfigureAwait(false);
                  return unit;
              })
              // SupplyChainGate verifies the download BEFORE it stages, so a forged or
              // out-of-contract release never reaches ApplyUpdatesAndRestart — a failed admit mints a
              // RolledBack receipt carrying the supply-chain fault rather than staging the bytes.
              from admitted in SupplyChainGate.Admit(gate, new AdmissionSubject.Release(found.TargetFullRelease, channel), token)
              from finish in IO.lift(() => host.Clock.GetCurrentInstant())
              from receipt in admitted.Match(
                  Succ: _ => Mint(UpdatePhase.Staged, Target(found.TargetFullRelease), Downgrade(found.TargetFullRelease), found.DeltasToTarget.Length, finish - start, new UpdateOutcome.StagedPending(Target(found.TargetFullRelease))),
                  Fail: faults => Mint(UpdatePhase.RolledBack, Target(found.TargetFullRelease), Downgrade(found.TargetFullRelease), found.DeltasToTarget.Length, finish - start, new UpdateOutcome.RolledBack(Prior, new UpdateFault.AdmissionRejected(string.Join("; ", faults.Map(static f => f.Message))))))
              from _ in IO.lift(() => admitted.IsSuccess ? staged.Add(1, channel.Key) : rollback.Add(1, channel.Key))
              select receipt;

    public IO<UpdateReceipt> Rollover(VelopackAsset asset, Duration cooperative, Duration forced) =>
        from drained in host.Drain(DrainRows(), cooperative, forced)
        from rolling in Mint(UpdatePhase.RollingOver, Target(asset), false, 0, drained.Elapsed, new UpdateOutcome.Restarted(Target(asset)))
        from _ in IO.lift(() => rolloverDuration.Record((host.Clock.GetCurrentInstant() - drained.At).TotalSeconds, channel.Key))
        from applied in IO.lift(fun(() => manager.ApplyUpdatesAndRestart(asset)))
        select rolling;

    public IO<UpdateReceipt> Resume(Duration cooperative, Duration forced) =>
        Pending.Match(
            Some: asset => Rollover(asset, cooperative, forced),
            None: () => Mint(UpdatePhase.Detected, string.Empty, false, 0, Duration.Zero, new UpdateOutcome.Declined(nameof(UpdatePhase.Detected))));

    Seq<(string Name, DrainBand Band, int Rank, Func<CancellationToken, IO<Unit>> Drain)> DrainRows() =>
        [(nameof(UpdateRail), DrainBand.Stores, 0, static _ => IO.pure(unit))];

    IO<UpdateReceipt> Mint(UpdatePhase phase, string target, bool downgrade, int deltas, Duration elapsed, UpdateOutcome outcome) =>
        from at in IO.lift(() => host.Clock.GetCurrentInstant())
        let receipt = new UpdateReceipt(phase, channel.Key, target, Prior, downgrade, deltas, at, elapsed, outcome, host.CorrelationId)
        from _ in sink.Send(host.CorrelationId, TenantContext.Current, TelemetrySource.AppHost.Key, phase.Key, JsonSerializer.SerializeToElement(receipt, AppHostWireContext.Default.UpdateReceipt))
        select receipt;

    // Derived off the two catalogued members (VelopackAsset.Version, UpdateManager.CurrentVersion) —
    // never an uncatalogued UpdateInfo flag.
    bool Downgrade(VelopackAsset target) => manager.CurrentVersion is { } current && target.Version < current;

    static string Target(VelopackAsset asset) => asset.Version.ToString();
}
```

```mermaid
accTitle: Update phase state
accDescr: Downloaded releases stage, roll over, or end in rollback.
stateDiagram-v2
    [*] --> Detected
    Detected --> Downloading : Stage
    Detected --> RolledBack : DowngradeBlocked
    Downloading --> Staged : DownloadComplete
    Downloading --> RolledBack : DownloadBroken
    Staged --> RollingOver : Rollover
    Staged --> Staged : PendingRestart
    RollingOver --> [*] : Restarted
    RollingOver --> RolledBack : RolloverRejected
    RolledBack --> [*]
```

## [03]-[CHANNEL_AXIS]

- Owner: `UpdateChannel` `[SmartEnum<string>]` three feed rows under the `ComparerAccessors.StringOrdinal` accessor, carrying the feed URI, explicit-channel string, and downgrade-allow column.
- Cases: 3 channel rows — stable, beta, canary.
- Entry: `UpdateChannel.From(ReleaseIdentity installed)` resolves the row from the detect-leg identity's channel string under the ordinal accessor.
- Auto: the resolved row's `Feed` seats the `UpdateManager` ctor url, its `ExplicitChannel` seats `UpdateOptions.ExplicitChannel`, and its `AllowVersionDowngrade` seats `UpdateOptions.AllowVersionDowngrade` — the three columns are the only update-options surface the rail writes; `MaximumDeltasBeforeFallback` stays unset so the full-package fallback governs; canary alone admits a downgrade so a forward-rolled canary build reverts to its prior pin.
- Receipt: the channel key stamps `UpdateReceipt.Channel` and keys the `AddView` cardinality cap on every update instrument.
- Packages: Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: one channel row carries one feed URI, one explicit-channel string, and one downgrade column; a ring split lands as one row, never a second axis; zero new surface.
- Boundary: the axis owns the feed-routing decision — each row carries its own authoritative `Feed` URI, and `From` resolves the row from `ReleaseIdentity.Channel` under the ordinal accessor; the detect-leg `ReleaseIdentity.Feed` is the outbound poll URI of the `UpdateCheck` hop, a distinct value the axis never reads; `ExplicitChannel` is the Velopack channel-suffix selector that pins which release set the manager resolves; `AllowVersionDowngrade` is the downgrade-policy column the rail reads before any transfer, never a per-call flag; the `AddView` rows at signal-governance cap update-instrument cardinality on the channel key so three channels cap at three series per instrument.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UpdateChannel {
    public static readonly UpdateChannel Stable = new("stable", new Uri("https://updates.rasm.app/stable"), explicitChannel: "stable", allowVersionDowngrade: false);
    public static readonly UpdateChannel Beta = new("beta", new Uri("https://updates.rasm.app/beta"), explicitChannel: "beta", allowVersionDowngrade: false);
    public static readonly UpdateChannel Canary = new("canary", new Uri("https://updates.rasm.app/canary"), explicitChannel: "canary", allowVersionDowngrade: true);

    public Uri Feed { get; }
    public string ExplicitChannel { get; }
    public bool AllowVersionDowngrade { get; }

    public static UpdateChannel From(ReleaseIdentity installed) =>
        TryGet(installed.Channel, out var row) ? row : Stable;
}
```

## [04]-[ROLLOVER_DRAIN]

- Owner: `RolloverDrain` static surface composing `DrainConductor.Drain` ahead of `UpdateRail.Rollover` so a node empties before its process is replaced; `RollStrategy` `[SmartEnum<string>]` the progressive-delivery axis (canary, blue-green, linear-wave) with delegate-backed cohort planning and the shared outcome-and-health advance fold; `RollPlan` the per-wave cohort projection; `FleetRoll` the fleet-wide rolling-update conductor walking `MembershipView.Serving` in strategy-shaped health-gated waves; `FleetRollReceipt` the per-wave fleet-progress projection riding the existing receipt stream; `RollAnnotationWire` the per-wave deploy-annotation record folded off the roll receipts, joining the `AppHostWireContext` roster and fanned under `InstrumentFan.RollKind` so the estate dashboard timeline marks every fleet wave.
- Cases: two conduct paths on the local node — `Conduct` for a staged asset, `ConductPending` for a post-bounce resume; three roll strategies — `Canary` rolls a single-node probe then expands the cohort on a health-hold, `BlueGreen` swaps a parallel half-fleet cohort on a health-pass, `LinearWave` advances fixed N% increments with a bake window between waves; one fleet conduct — `FleetRoll.Roll` paces the wave across the cluster serving set, the `RollStrategy` row shaping each next cohort off the prior cohort's recovered serving status.
- Entry: `IO<UpdateReceipt> Conduct(UpdateRail rail, VelopackAsset staged, DeadlineClass cooperative, DeadlineClass forced)` — `IO` carries the drain-then-restart effect; the drain receipt seats the rollover; `IO<Seq<FleetRollReceipt>> Roll(MembershipView membership, UpdateChannel channel, RollStrategy strategy, Func<MemberRecord, IO<UpdateReceipt>> rollNode, Func<MemberRecord, IO<HealthReport>> probe, Func<Duration, IO<Unit>> bake, ReceiptSinkPort sink, IClock clock, TenantContext tenant)` plans the cohorts from `RollStrategy.Plan(membership.Serving)`, rolls each cohort, waits on the post-roll `WireHealth.Evaluate` serving probe, then bakes the strategy's inter-wave dwell through the injected clock-driven `bake` delegate before the next cohort admits.
- Auto: the conductor's first act is the draining transition, so inbound admission ceases before the staged release rolls over; the cooperative and forced budgets arrive from the `DrainCooperative` and `DrainForced` deadline rows; the rollover histogram records the span from drain settle to restart handoff; on a fleet node the parent's drain registration fans the signal to the child over the local-ipc hop before the parent itself rolls over; `RollStrategy.Plan` folds `MembershipView.Serving` into the ordered cohort sequence the strategy shape dictates — `Canary` plans a 1-node lead cohort then the remainder, while `BlueGreen` and `LinearWave` derive every cohort width from their `WavePercent` columns and apply the row's `BakeWindow` dwell — and `FleetRoll.Roll` rolls each cohort, commits its annotation, then advances only when every node carries `UpdateOutcome.Restarted` and returns `Serving`; rollback, declined, staged-pending, and `NotServing` evidence hold the wave before bake or recursion; the canary health-hold reuses the existing `HealthSnapshot`/`DegradationLevel` gate through the `probe`, never a new probe owner; the strategy row also threads the targeting plane — a feature verdict selects which `RollStrategy` row a wave runs so progressive binary rollout and feature rollout share one targeting plane.
- Receipt: the `RollingOver` `UpdateReceipt` carries the `DrainReceipt.Elapsed` as its drain span and the rollover outcome; a straggled drain step does not abort the rollover — the restart proceeds and the straggler surfaces on the drain receipt the rollover receipt references by correlation id; each `FleetRoll` cohort mints one `FleetRollReceipt` — wave index, the `RollStrategy` key, node id, the node's terminal `UpdateOutcome`, post-roll serving status, and the live nodes-remaining countdown (the un-rolled tail of the current cohort and every later cohort, never the constant fleet count) — fanned through the existing receipt stream beside the per-node `UpdateReceipt`, never a parallel fleet instrument; each wave folds one `RollAnnotationWire` — wave, channel, strategy, advanced/held/rolled-back verdict, host count, live remaining, instant — fanned under `InstrumentFan.RollKind`, the deploy-annotation the TypeScript iac timeline rail ingests through the existing `ReceiptEnvelopeWire`.
- Packages: Velopack, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions.
- Growth: one drain participant row per update-sensitive subsystem registered through `DrainParticipantPort` at its declared band; a new progressive strategy is one `RollStrategy` row with its plan delegate and policy columns, never a second roll state machine; a wave-width retune is the row's `WavePercent` column; zero new surface.
- Boundary: drain-before-swap is the law — `ApplyUpdatesAndRestart` is never reached until `DrainConductor.Drain` settles, so the replaced process leaves no half-flushed store write or in-flight hop; the cooperative and forced deadline values are the `DrainCooperative`/`DrainForced` rows, never an inline literal; the staged asset is `UpdatePendingRestart` read at composition, so a rollover after a process bounce resumes from the staged phase without re-staging; the rollover is the single restart path — the bare `ApplyUpdatesAndExit` and `WaitExitThenApplyUpdates` forms are deleted because the drain-gated restart owns the handoff; `RollStrategy` is one row on the existing `FleetRoll`, not a parallel conductor — a second roll state machine or a strategy-specific scheduler beside `ScheduleEntry.Spread` is the rejected form, and the `ScheduleEntry.Spread` fleet-spread seed stays the wave-pacing cadence the strategy `BakeWindow` reads, never a new scheduler; `FleetRoll` consumes `MembershipView.Serving` as fleet membership and `WireHealthRow` as the recovery gate; each node rolls through the same `RolloverDrain.Conduct`, and the first unrecovered node halts the fleet; fenced conductor election keeps one conductor per fleet so two nodes never drive overlapping waves.

```csharp signature
public static class RolloverDrain {
    public static IO<UpdateReceipt> Conduct(UpdateRail rail, VelopackAsset staged, DeadlineClass cooperative, DeadlineClass forced) =>
        rail.Rollover(staged, cooperative.Allotted, forced.Allotted);

    public static IO<UpdateReceipt> ConductPending(UpdateRail rail, DeadlineClass cooperative, DeadlineClass forced) =>
        rail.PendingRestart
            ? rail.Resume(cooperative.Allotted, forced.Allotted)
            : IO.fail<UpdateReceipt>(new UpdateFault.StagePending(nameof(rail.PendingRestart)));
}

// RollStrategy carries one cohort plan, recover-and-advance
// predicate, and the inter-wave bake window. Plan folds the roster into the ordered cohort sequence the
// strategy shape dictates; Advances admits the following cohort only on a held health-pass. A second roll
// state machine beside this axis is the rejected form.
public sealed record RollPlan(Seq<Seq<MemberRecord>> Cohorts, Duration BakeWindow);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RollStrategy {
    public static readonly RollStrategy Canary = new("canary", wavePercent: 0, bake: Duration.FromSeconds(120),
        plan: static (nodes, _) => nodes.IsEmpty ? [] : Seq(nodes.Take(1).ToSeq()).Add(nodes.Skip(1).ToSeq()).Filter(static c => !c.IsEmpty));
    public static readonly RollStrategy BlueGreen = new("blue-green", wavePercent: 50, bake: Duration.Zero,
        plan: static (nodes, percent) => nodes.IsEmpty ? [] : Chunk(nodes, Width(nodes.Count, percent)));
    public static readonly RollStrategy LinearWave = new("linear-wave", wavePercent: 25, bake: Duration.FromSeconds(300),
        plan: static (nodes, percent) => nodes.IsEmpty ? [] : Chunk(nodes, Width(nodes.Count, percent)));

    public int WavePercent { get; }
    public Duration Bake { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Seq<MemberRecord>> Cohorts(Seq<MemberRecord> nodes, int wavePercent);

    public RollPlan Plan(Seq<MemberRecord> nodes) => new(Cohorts(nodes, WavePercent), Bake);

    // A health-pass advances only a cohort whose nodes restarted and returned to service; rollback,
    // staged-pending, declined, or NotServing evidence holds the wave after its annotation commits.
    public bool Advances(Seq<FleetRollReceipt> cohort) => cohort.ForAll(static row =>
        row.Outcome is UpdateOutcome.Restarted && row.Serving == ServingStatus.Serving);

    static Seq<Seq<MemberRecord>> Chunk(Seq<MemberRecord> nodes, int width) =>
        nodes.AsEnumerable().Chunk(width).Select(static c => c.ToSeq()).ToSeq();

    static int Width(int count, int percent) => int.Max((int)Math.Ceiling(count * percent / 100d), 1);
}

public readonly record struct FleetRollReceipt(
    int Wave,
    string Strategy,
    int NodeId,
    UpdateOutcome Outcome,
    ServingStatus Serving,
    int Remaining,
    Instant At);

// RollAnnotationWire folds one record per wave off the roll receipts — wave,
// channel, strategy, verdict, host count, live remaining, instant — HLC-stamped on the receipt fan
// under InstrumentFan.RollKind so the estate dashboard timeline marks every fleet wave beside stack
// deploys, and a rollback annotates as loudly as an advance; a parallel deploy-event sender is the
// deleted form.
public readonly record struct RollAnnotationWire(
    int Wave,
    string Channel,
    string Strategy,
    string Verdict,
    int HostCount,
    int Remaining,
    Instant At) {
    public const string Advanced = "advanced";
    public const string Held = "held";
    public const string RolledBack = "rolled-back";

    public static RollAnnotationWire From(UpdateChannel channel, RollStrategy strategy, int wave, Seq<FleetRollReceipt> cohort, bool advanced, Instant at) =>
        new(wave, channel.Key, strategy.Key,
            cohort.Exists(static row => row.Outcome is UpdateOutcome.RolledBack) ? RolledBack : advanced ? Advanced : Held,
            cohort.Count,
            cohort.Last.Map(static row => row.Remaining).IfNone(0),
            at);
}

public static class FleetRoll {
    // Bake delay rides the injected clock-driven `bake` delegate (the SchedulePort cadence the strategy
    // BakeWindow reads, test-fakeable through the same TimeProvider the spine injects), never an ambient
    // Task.Delay — so a LinearWave bakes its 300s window and a Canary holds its 120s probe between cohorts.
    public static IO<Seq<FleetRollReceipt>> Roll(
        MembershipView membership,
        UpdateChannel channel,
        RollStrategy strategy,
        Func<MemberRecord, IO<UpdateReceipt>> rollNode,
        Func<MemberRecord, IO<HealthReport>> probe,
        Func<Duration, IO<Unit>> bake,
        ReceiptSinkPort sink,
        IClock clock,
        TenantContext tenant) =>
        strategy.Plan(membership.Serving) is var plan
            ? Wave(plan.Cohorts, 0, channel, strategy, plan, rollNode, probe, bake, sink, clock, tenant)
            : IO.pure(Seq<FleetRollReceipt>());

    static IO<Seq<FleetRollReceipt>> Wave(
        Seq<Seq<MemberRecord>> cohorts, int index, UpdateChannel channel, RollStrategy strategy, RollPlan plan,
        Func<MemberRecord, IO<UpdateReceipt>> rollNode, Func<MemberRecord, IO<HealthReport>> probe,
        Func<Duration, IO<Unit>> bake, ReceiptSinkPort sink, IClock clock, TenantContext tenant) =>
        cohorts.IsEmpty
            ? IO.pure(Seq<FleetRollReceipt>())
            : cohorts.Head.Value.AsEnumerable().Select(static (node, slot) => (node, slot)).ToSeq()
                .TraverseM(pair =>
                    from rolled in rollNode(pair.node)
                    from report in probe(pair.node)
                    from at in IO.lift(() => clock.GetCurrentInstant())
                    // Remaining counts down live — the un-rolled tail of this cohort plus every later
                    // cohort — never the constant fleet serving count.
                    let receipt = new FleetRollReceipt(index, strategy.Key, pair.node.NodeId, rolled.Outcome, Serving(report),
                        cohorts.Head.Value.Count - pair.slot - 1 + cohorts.Tail.Map(static c => c.Count).Sum(), at)
                    from _ in sink.Send(Correlation.Mint(), tenant, TelemetrySource.AppHost.Key, nameof(FleetRoll),
                        JsonSerializer.SerializeToElement(receipt, AppHostWireContext.Default.FleetRollReceipt))
                    select receipt)
                .As()
                .Bind(here =>
                    from at in IO.lift(() => clock.GetCurrentInstant())
                    let advanced = strategy.Advances(here)
                    // One annotation per wave — the deploy-timeline record the RollKind fan arm projects.
                    from _ in sink.Send(Correlation.Mint(), tenant, TelemetrySource.AppHost.Key, InstrumentFan.RollKind,
                        JsonSerializer.SerializeToElement(RollAnnotationWire.From(channel, strategy, index, here, advanced, at), AppHostWireContext.Default.RollAnnotationWire))
                    from rest in advanced && !cohorts.Tail.IsEmpty
                        // health-pass with a following cohort: bake the inter-wave dwell, then advance.
                        ? (plan.BakeWindow > Duration.Zero ? bake(plan.BakeWindow) : IO.pure(unit))
                            .Bind(__ => Wave(cohorts.Tail, index + 1, channel, strategy, plan, rollNode, probe, bake, sink, clock, tenant))
                        : IO.pure(Seq<FleetRollReceipt>())
                    select here + rest);

    static ServingStatus Serving(HealthReport report) =>
        report.Status == HealthStatus.Unhealthy ? ServingStatus.NotServing : ServingStatus.Serving;
}
```

```mermaid
accTitle: Drain-gated restart handoff
accDescr: UpdateRail drains the node, records rollover, and hands restart to Velopack.
sequenceDiagram
    participant Rail as UpdateRail
    participant Drain as DrainConductor
    participant Velopack as UpdateManager
    Rail->>Drain: Drain(rows, cooperative, forced)
    Drain-->>Rail: DrainReceipt
    Rail->>Rail: Mint(RollingOver) -> ReceiptSinkPort
    Rail->>Velopack: ApplyUpdatesAndRestart(staged)
    Note over Velopack: process replaced, call never returns
```

## [05]-[RESEARCH]

- [STAGED_FEED]-[BLOCKED]: Which production feed URI belongs to each `UpdateChannel` row?; verify against release-feed host provisioning.
