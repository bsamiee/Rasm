# [APPHOST_PROVISIONING_AND_UPDATE]

Rasm.AppHost owns the post-fetch update concern: a `UpdateManager`-borne state machine that downloads a found release, admits it through an offline supply-chain gate, stages it, drains the node, rolls it over through `ApplyUpdatesAndRestart`, and mints a typed `UpdateReceipt` on every phase, plus a three-row `UpdateChannel` vocabulary carrying feed routing and downgrade policy, plus a supply-chain admit gate proving the downloaded artifact's Sigstore signature and SemVer-contract before a byte is staged, plus a fleet-wide rolling-update conductor that walks the attached-peer roster in a `RollStrategy`-shaped (canary, blue-green, linear-wave) health-gated wave. The page owns the update rail, the channel axis, the supply-chain admit gate, the progressive-rollout strategy axis, and the rollover-drain handshake that runs `DrainConductor` before the restart hands the process to Velopack and the `FleetRoll` fleet conductor that paces the strategy-shaped wave over `PeerRoster`. The `UpdateCheck(ReleaseIdentity)` outbound hop stays the detect leg at outbound-resilience; everything after a release is found composes here over Velopack, the `Sigstore`/`NuGet.Versioning` admit gate, the `DrainConductor` fold, `ReceiptSinkPort`, and the generated metric attributes.

## [01]-[INDEX]

- [01]-[UPDATE_RAIL]: Post-fetch state machine, fault band, per-phase receipt, and generated instruments.
- [02]-[CHANNEL_AXIS]: Three feed rows binding explicit channel and downgrade policy onto options.
- [03]-[SUPPLY_CHAIN_GATE]: Offline Sigstore signature + SLSA provenance and SemVer-contract admission run before any release stages.
- [04]-[ROLLOVER_DRAIN]: Drain-before-swap handshake and the canary/blue-green/linear-wave `RollStrategy` axis over a health-gated fleet-wide wave.

## [02]-[UPDATE_RAIL]

- Owner: `UpdatePhase` `[SmartEnum<string>]` five post-fetch phases under the `UpdateKeyPolicy` ordinal accessor; `UpdateOutcome` `[Union]` terminal disposition; `UpdateFault` `[Union]` fault family in the 1300 band; `UpdateReceipt` per-phase evidence record; `UpdateMetrics` source-gen instrument partial under the `CounterAttribute`/`HistogramAttribute` generator; `UpdateRail` boundary capsule owning the `UpdateManager` handle and the staged-pending probe.
- Cases: 5 phase rows — detected, downloading, staged, rolling-over, rolled-back; outcomes restarted | staged-pending | rolled-back | declined; `UpdateFault` = Text | DownloadBroken | StagePending | RolloverRejected | DowngradeBlocked.
- Entry: `IO<UpdateReceipt> Stage(UpdateInfo found, IProgress<int> progress, CancellationToken token)` carries the download-and-stage effect and forecloses a blocked downgrade before transfer; `IO<UpdateReceipt> Rollover(VelopackAsset asset, Duration cooperative, Duration forced)` carries the drain-gated restart effect; `IO<UpdateReceipt> Resume(Duration cooperative, Duration forced)` re-enters a staged-pending release after a process bounce.
- Auto: every phase commit mints one `UpdateReceipt` fanned to `ReceiptSinkPort.Send` under the `Rasm.AppHost` package key; the generated counter rises per staged and per rollback phase and the generated histogram records the rollover span; `IsUpdatePendingRestart` is read at boot so a staged-but-unrestarted release re-enters the rail at the staged phase without a second download.
- Receipt: `UpdateReceipt` — phase, channel key, target version, prior version, downgrade flag, delta count, `Instant`, elapsed `Duration`, outcome, correlation id.
- Packages: Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Microsoft.Extensions.Telemetry.Abstractions, BCL inbox.
- Growth: one phase row plus its `Next` arm, or one outcome case, or one fault case breaking every dispatch site at compile time; one instrument is one strongly-typed metric-attribute factory; zero new surface.
- Boundary: `UpdateRail` is the named boundary capsule for the statement carve-out — the `UpdateManager` ctor, the awaited download, and the terminal `ApplyUpdatesAndRestart` carry language-owned statement forms while every other member stays expression-shaped; the rail composes `UpdateManager` directly with no rename adapter — the `UpdateChannel` axis is the only added vocabulary; `VelopackApp.Build()...Run()` is the process-entry bootstrap owned at the app root, never a rail fence, so `VelopackHook` registration stays at the app root and never enters this page; `ApplyUpdatesAndRestart` takes `found.TargetFullRelease` as its `VelopackAsset`, never the `UpdateInfo`, and the call never returns because the host process is replaced — the rolled-over receipt mints and fans before the call; `found.IsDowngrade` against the channel's `AllowVersionDowngrade` column forecloses a disallowed downgrade as `DowngradeBlocked` before any byte transfers; an inline `meter.CreateCounter` call is the deleted form — every spine instrument is a generated factory whose name and tag set are declaration facts and whose generated metric type exposes the strongly-typed `Add`/`Record` over the channel-key tag; the `Target` fold reads `VelopackAsset.Version` (a `SemanticVersion`) through `ToString`, the single version-stamp seam; `UpdateReceipt` rides the suite wire law as one `AppHostWireContext` `[JsonSerializable]` row; `vpk`-side notarization and SBOM emission are build-time signing concerns and carry no rail fence, but the RUNTIME admission verify of a downloaded release — proving the artifact's Sigstore signature and SemVer-contract before it stages — is the `#SUPPLY_CHAIN_GATE` `SupplyChainGate.Admit` boundary capsule the `Stage` fold composes, never a skipped step; the page is host-local and crosses no browser or peer TS wire — `UpdateReceipt` and `FleetRollReceipt` reconstruct in TS solely through the existing `ReceiptEnvelopeWire` at Runtime/ports#TS_PROJECTION, so the page authors no `TS_PROJECTION` cluster and adds no second wire shape.

```csharp signature
public sealed class UpdateKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<UpdateKeyPolicy, string>]
[KeyMemberComparer<UpdateKeyPolicy, string>]
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
    public sealed record Text : UpdateFault { public Text(string detail) : base(detail, 1300) { } }
    public sealed record DownloadBroken : UpdateFault { public DownloadBroken(string detail) : base(detail, 1301) { } }
    public sealed record StagePending : UpdateFault { public StagePending(string detail) : base(detail, 1302) { } }
    public sealed record RolloverRejected : UpdateFault { public RolloverRejected(string detail) : base(detail, 1303) { } }
    public sealed record DowngradeBlocked : UpdateFault { public DowngradeBlocked(string detail) : base(detail, 1304) { } }
    public sealed record AdmissionRejected : UpdateFault { public AdmissionRejected(string detail) : base(detail, 1305) { } }
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
        this.manager = new UpdateManager(channel.Feed, new UpdateOptions {
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
        found.IsDowngrade && !channel.AllowVersionDowngrade
            ? from blocked in Mint(UpdatePhase.RolledBack, Target(found.TargetFullRelease), found.IsDowngrade, found.DeltasToTarget.Length, Duration.Zero, new UpdateOutcome.RolledBack(Prior, new UpdateFault.DowngradeBlocked(Target(found.TargetFullRelease))))
              from _ in IO.lift(() => rollback.Add(1, channel.Key))
              select blocked
            : from start in IO.lift(() => host.Clock.GetCurrentInstant())
              from downloading in Mint(UpdatePhase.Downloading, Target(found.TargetFullRelease), found.IsDowngrade, found.DeltasToTarget.Length, Duration.Zero, new UpdateOutcome.StagedPending(Target(found.TargetFullRelease)))
              from done in IO.liftAsync(async () => {
                  await manager.DownloadUpdatesAsync(found, progress.Report, token).ConfigureAwait(false);
                  return unit;
              })
              // Supply-chain admit gate: the downloaded artifact is verified BEFORE it is staged, so a
              // forged or out-of-contract release never reaches ApplyUpdatesAndRestart — a failed admit
              // mints a RolledBack receipt carrying the supply-chain fault rather than staging the bytes.
              from admitted in SupplyChainGate.Admit(gate, found.TargetFullRelease, channel, token)
              from finish in IO.lift(() => host.Clock.GetCurrentInstant())
              from receipt in admitted.Match(
                  Succ: _ => Mint(UpdatePhase.Staged, Target(found.TargetFullRelease), found.IsDowngrade, found.DeltasToTarget.Length, finish - start, new UpdateOutcome.StagedPending(Target(found.TargetFullRelease))),
                  Fail: faults => Mint(UpdatePhase.RolledBack, Target(found.TargetFullRelease), found.IsDowngrade, found.DeltasToTarget.Length, finish - start, new UpdateOutcome.RolledBack(Prior, new UpdateFault.AdmissionRejected(faults.Head.Message))))
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

    static string Target(VelopackAsset asset) => asset.Version.ToString();
}
```

```mermaid
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

- Owner: `UpdateChannel` `[SmartEnum<string>]` three feed rows under the `UpdateKeyPolicy` ordinal accessor, carrying the feed URI, explicit-channel string, and downgrade-allow column.
- Cases: 3 channel rows — stable, beta, canary.
- Entry: `UpdateChannel.From(ReleaseIdentity installed)` resolves the row from the detect-leg identity's channel string under the ordinal accessor.
- Auto: the resolved row's `Feed` seats the `UpdateManager` ctor url, its `ExplicitChannel` seats `UpdateOptions.ExplicitChannel`, and its `AllowVersionDowngrade` seats `UpdateOptions.AllowVersionDowngrade` — the three columns are the only update-options surface the rail writes; `MaximumDeltasBeforeFallback` stays unset so the full-package fallback governs; canary alone admits a downgrade so a forward-rolled canary build reverts to its prior pin.
- Receipt: the channel key stamps `UpdateReceipt.Channel` and keys the `AddView` cardinality cap on every update instrument.
- Packages: Velopack, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: one channel row carries one feed URI, one explicit-channel string, and one downgrade column; a ring split lands as one row, never a second axis; zero new surface.
- Boundary: the axis owns the feed-routing decision — each row carries its own authoritative `Feed` URI, and `From` resolves the row from `ReleaseIdentity.Channel` under the ordinal accessor; the detect-leg `ReleaseIdentity.Feed` is the outbound poll URI of the `UpdateCheck` hop, a distinct value the axis never reads; `ExplicitChannel` is the Velopack channel-suffix selector that pins which release set the manager resolves; `AllowVersionDowngrade` is the downgrade-policy column the rail reads before any transfer, never a per-call flag; the `AddView` rows at signal-governance cap update-instrument cardinality on the channel key so three channels cap at three series per instrument.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<UpdateKeyPolicy, string>]
[KeyMemberComparer<UpdateKeyPolicy, string>]
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

## [04]-[SUPPLY_CHAIN_GATE]

- Owner: `SupplyChainFault` `[Union]` fault family in the 1320 band; `SupplyChainReceipt` the admit-evidence record; `TrustPolicy` the per-channel expected-signer plus version-contract policy; `SupplyChainGate` the static admit surface whose `Admit` is the named statement carve-out, with the nested `Runtime` binding the one offline `SigstoreVerifier`, the policy resolver, and the staging directory.
- Cases: `SupplyChainFault` = Text | BundleMissing | SignatureRejected | ProvenanceUnbound | VersionIncompatible | TrustRootUnavailable — one case per admit-rejection cause.
- Entry: `Admit(SupplyChainGate.Runtime gate, VelopackAsset asset, UpdateChannel channel, CancellationToken token)` returns `IO<Validation<SupplyChainFault, SupplyChainReceipt>>` — loads the cosign bundle sitting beside the downloaded artifact, verifies its Sigstore signature and SLSA provenance offline against the pinned trust root through `SigstoreVerifier.TryVerifyDigestAsync` over the artifact's `SHA256` digest, and checks the artifact `Version` against the channel's `VersionRange` contract; the signature leg and the version leg accumulate applicatively so a release that is both forged AND out-of-contract reports both faults in one pass.
- Auto: the `Admit` runs BEFORE `UpdateRail.Stage` commits the staged phase, so a forged or out-of-contract release never reaches `ApplyUpdatesAndRestart` — the `Stage` fold branches on the admit `Validation`, minting `RolledBack` on a fault and `Staged` only on success; the trust anchor is the offline `FileTrustRootProvider(pinnedTrustedRootJson)` so the verify path performs NO network call and the gate is hermetic; `SigstoreVerifier.TryVerifyDigestAsync` is the non-throwing ROP mirror returning `(bool Success, VerificationResult? Result)` — a `VerificationException` never escapes the domain — and reuses the artifact's already-computed `SHA256` rather than re-reading the package stream; the expected signer is the `VerificationPolicy.CertificateIdentity` built once via `CertificateIdentity.ForGitHubActions(owner, repository)`, so an empty-identity verify that asserts only cryptographic integrity is the rejected form; the DSSE/in-toto provenance leg reads `VerificationResult.Statement` (`InTotoStatement`) and binds its `Subject` digest to the admitted artifact so one verify proves signature AND build provenance; the version leg parses the artifact's Velopack `SemanticVersion` through `NuGetVersion.TryParse(asset.Version.ToString())` and decides with `VersionRange.Satisfies`, the real SemVer-2.0 contract check `System.Version` cannot express, with a parse failure on either boundary failing closed as `VersionIncompatible`.
- Receipt: `SupplyChainReceipt` — verified signer SAN, in-toto predicate type, admitted version string, `Instant`; the verified signer is the trusted-publisher principal the `Agent/capability#GRANT_BROKER` may treat as a privileged artifact source, and the receipt rides the `UpdateReceipt` correlation, never a parallel admit instrument.
- Packages: Sigstore, NuGet.Versioning, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one verify threshold is one `VerificationPolicy` column (`TransparencyLogThreshold`, `RequireSignedCertificateTimestamps`); one channel's expected signer is one `TrustPolicy` row; a managed-key (non-Fulcio) feed is the `VerificationPolicy.PublicKey` column; zero new surface.
- Boundary: the gate is the suite's only supply-chain admit owner — a `System.Version`-based semver check, a hand-split `lower-upper` range string, a throwing `Parse` in the admission fold, an unsigned-release install, and a network-bound verify on an air-gapped node are the deleted forms; the gate is the precondition the `Sandbox/isolation` plugin loader's third-party-artifact admission shares — both the self-update release and a downloaded plugin/companion artifact verify through this one `Admit`, never two verify paths; `vpk`-side build-time notarization is distinct — this is the runtime admission verify of what was actually downloaded, so the build signs and this gate proves; the `TufTrustRootProvider` network anchor is admitted only on a connected node and rides the `Wire/outbound` `Polly.Core` pipeline, while the `FileTrustRootProvider` removes that dependency for a hermetic gate, so the trust-root fetch is the only outbound leg and the verify itself is offline; the version leg admits only the version/range/comparer surface — package-graph resolution and framework compatibility stay out of scope, the contract is one `VersionRange.Satisfies` membership test, and `FindBestMatch` selects the newest in-range candidate when a feed offers several.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
public abstract partial record SupplyChainFault : Expected, IValidationError<SupplyChainFault> {
    private SupplyChainFault(string detail, int code) : base(detail, code, None) { }
    public static SupplyChainFault Create(string message) => new Text(message);
    public sealed record Text : SupplyChainFault { public Text(string detail) : base(detail, 1320) { } }
    public sealed record BundleMissing : SupplyChainFault { public BundleMissing(string detail) : base(detail, 1321) { } }
    public sealed record SignatureRejected : SupplyChainFault { public SignatureRejected(string detail) : base(detail, 1322) { } }
    public sealed record ProvenanceUnbound : SupplyChainFault { public ProvenanceUnbound(string detail) : base(detail, 1323) { } }
    public sealed record VersionIncompatible : SupplyChainFault { public VersionIncompatible(string detail) : base(detail, 1324) { } }
    public sealed record TrustRootUnavailable : SupplyChainFault { public TrustRootUnavailable(string detail) : base(detail, 1325) { } }
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct SupplyChainReceipt(string Signer, string Provenance, string Version, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class SupplyChainGate {
    public sealed record TrustPolicy(VerificationPolicy Verification, VersionRange ContractRange);
    public sealed record Runtime(SigstoreVerifier Verifier, Func<UpdateChannel, TrustPolicy> PolicyOf, DirectoryInfo Staging);

    public static IO<Validation<SupplyChainFault, SupplyChainReceipt>> Admit(Runtime gate, VelopackAsset asset, UpdateChannel channel, CancellationToken token) =>
        gate.PolicyOf(channel) is var policy && Bundle(gate.Staging, asset) is { IsSome: true } bundleFile
            ? from loaded in IO.liftAsync(async () => await SigstoreBundle.LoadAsync(bundleFile.ValueUnsafe(), token))
              from verified in IO.liftAsync(async () => await gate.Verifier.TryVerifyDigestAsync(
                  Convert.FromHexString(asset.SHA256), HashAlgorithmType.Sha256, loaded, policy.Verification, token))
              from at in IO.lift(() => DateTimeOffset.UtcNow)
              select (Signature(verified, asset), Version(policy.ContractRange, asset))
                  .Apply(static (signer, version) => new SupplyChainReceipt(
                      signer.Signer.SubjectAlternativeName, signer.Provenance, version.ToNormalizedString(), Instant.FromDateTimeOffset(at)))
                  .As()
            : IO.pure<Validation<SupplyChainFault, SupplyChainReceipt>>(Fail(new SupplyChainFault.BundleMissing(asset.FileName)));

    // Signature leg: a passing TryVerify carries a VerifiedIdentity AND the decoded in-toto SLSA statement;
    // the provenance Subject binds the attested artifact, so signature and build-provenance pass as one.
    static Validation<SupplyChainFault, (VerifiedIdentity Signer, string Provenance)> Signature((bool Success, VerificationResult? Result) verified, VelopackAsset asset) =>
        verified is { Success: true, Result.SignerIdentity: { } signer }
            ? verified.Result.Statement is { PredicateType: { } predicate }
                ? Success<SupplyChainFault, (VerifiedIdentity, string)>((signer, predicate))
                : Fail<SupplyChainFault, (VerifiedIdentity, string)>(new SupplyChainFault.ProvenanceUnbound(asset.FileName))
            : Fail<SupplyChainFault, (VerifiedIdentity, string)>(new SupplyChainFault.SignatureRejected(verified.Result?.FailureReason ?? asset.FileName));

    // Version leg: parse the Velopack SemanticVersion through NuGetVersion (real SemVer-2.0) and decide with
    // VersionRange.Satisfies — a parse failure or an out-of-contract version fails closed, matching the rail's posture.
    static Validation<SupplyChainFault, NuGetVersion> Version(VersionRange contract, VelopackAsset asset) =>
        NuGetVersion.TryParse(asset.Version.ToString(), out var version) && contract.Satisfies(version)
            ? Success<SupplyChainFault, NuGetVersion>(version)
            : Fail<SupplyChainFault, NuGetVersion>(new SupplyChainFault.VersionIncompatible($"{asset.Version} ∉ {contract.PrettyPrint()}"));

    static Option<FileInfo> Bundle(DirectoryInfo staging, VelopackAsset asset) =>
        new FileInfo(Path.Combine(staging.FullName, $"{asset.FileName}.sigstore.json")) is { Exists: true } file ? Some(file) : None;
}
```

## [05]-[ROLLOVER_DRAIN]

- Owner: `RolloverDrain` static surface composing `DrainConductor.Drain` ahead of `UpdateRail.Rollover` so a node empties before its process is replaced; `RollStrategy` `[SmartEnum<string>]` the progressive-delivery axis (canary, blue-green, linear-wave) with a delegate-backed `Next(cohort, health)` plan arm per strategy; `RollPlan` the per-wave cohort projection; `FleetRoll` the fleet-wide rolling-update conductor walking `PeerRoster.Attached` in strategy-shaped health-gated waves; `FleetRollReceipt` the per-wave fleet-progress projection riding the existing receipt stream.
- Cases: two conduct paths on the local node — `Conduct` for a staged asset, `ConductPending` for a post-bounce resume; three roll strategies — `Canary` rolls a single-node probe then expands the cohort on a health-hold, `BlueGreen` swaps a parallel half-fleet cohort on a health-pass, `LinearWave` advances fixed N% increments with a bake window between waves; one fleet conduct — `FleetRoll.Roll` paces the wave across the roster, the `RollStrategy` row shaping each next cohort off the prior cohort's recovered serving status.
- Entry: `IO<UpdateReceipt> Conduct(UpdateRail rail, VelopackAsset staged, DeadlineClass cooperative, DeadlineClass forced)` — `IO` carries the drain-then-restart effect; the drain receipt seats the rollover; `IO<Seq<FleetRollReceipt>> Roll(PeerRoster roster, RollStrategy strategy, Func<RosterEntry, IO<UpdateReceipt>> rollNode, Func<RosterEntry, IO<HealthReport>> probe, Func<Duration, IO<Unit>> bake, ReceiptSinkPort sink, IClock clock, TenantContext tenant)` plans the cohorts from `RollStrategy.Plan(roster.Attached)`, rolls each cohort, waits on the post-roll `WireHealth.Evaluate` serving probe, then bakes the strategy's inter-wave dwell through the injected clock-driven `bake` delegate before the next cohort admits.
- Auto: the conductor's first act is the draining transition, so inbound admission ceases before the staged release rolls over; the cooperative and forced budgets arrive from the `DrainCooperative` and `DrainForced` deadline rows; the rollover histogram records the span from drain settle to restart handoff; on a fleet node the parent's drain registration fans the signal to the child over the local-ipc hop before the parent itself rolls over; `RollStrategy.Plan` folds `PeerRoster.Attached` into the ordered cohort sequence the strategy shape dictates — `Canary` plans a 1-node lead cohort then the remainder, `BlueGreen` plans a single half-fleet cohort, `LinearWave` plans equal `WavePercent`-sized cohorts with a `BakeWindow` dwell — and `FleetRoll.Roll` rolls each cohort, folds the `probe` serving status, and halts the wave on a `NotServing` node before the next cohort so a node that fails to recover stops the rollout; the canary health-hold reuses the existing `HealthSnapshot`/`DegradationLevel` gate through the `probe`, never a new probe owner; the strategy row also threads the targeting plane — a `Runtime/features#FLAG_VERDICT` verdict selects which `RollStrategy` row a wave runs so progressive binary rollout and feature rollout share one targeting plane.
- Receipt: the `RollingOver` `UpdateReceipt` carries the `DrainReceipt.Elapsed` as its drain span and the rollover outcome; a straggled drain step does not abort the rollover — the restart proceeds and the straggler surfaces on the drain receipt the rollover receipt references by correlation id; each `FleetRoll` cohort mints one `FleetRollReceipt` — wave index, the `RollStrategy` key, node pid, the node's terminal `UpdateOutcome`, post-roll serving status, nodes-remaining — fanned through the existing receipt stream beside the per-node `UpdateReceipt`, never a parallel fleet instrument.
- Packages: Velopack, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions.
- Growth: one drain participant row per update-sensitive subsystem registered through `DrainParticipantPort` at its declared band; a new progressive strategy is one `RollStrategy` row with its `Plan`/`Next` arm, never a second roll state machine; a wave-width retune is the `LinearWave` `WavePercent` column; zero new surface.
- Boundary: drain-before-swap is the law — `ApplyUpdatesAndRestart` is never reached until `DrainConductor.Drain` settles, so the replaced process leaves no half-flushed store write or in-flight hop; the cooperative and forced deadline values are the `DrainCooperative`/`DrainForced` rows, never an inline literal; the staged asset is `UpdatePendingRestart` read at composition, so a rollover after a process bounce resumes from the staged phase without re-staging; the rollover is the single restart path — the bare `ApplyUpdatesAndExit` and `WaitExitThenApplyUpdates` forms are deleted because the drain-gated restart owns the handoff; `RollStrategy` is one row on the existing `FleetRoll`, not a parallel conductor — a second roll state machine or a strategy-specific scheduler beside `ScheduleEntry.Spread` is the rejected form, and the `ScheduleEntry.Spread` fleet-spread seed stays the wave-pacing cadence the strategy `BakeWindow` reads, never a new scheduler; `FleetRoll` is a conductor over the existing owners — it consumes `PeerRoster.Attached` from Wire/companion#PROCESS_MODALITY as the wave membership and `WireHealthRow` from Observability/health#WIRE_HEALTH as the per-node recovery gate, both as settled vocabulary, and each node's actual roll is the same `RolloverDrain.Conduct` the local node runs dialed over the control hop, so the fleet wave never re-implements the drain-gated restart; the wave halts on the first unrecovered node so a bad release never rolls the whole fleet; `FencingToken` conductor election keeps one conductor per fleet so two nodes never drive overlapping waves.

```csharp signature
public static class RolloverDrain {
    public static IO<UpdateReceipt> Conduct(UpdateRail rail, VelopackAsset staged, DeadlineClass cooperative, DeadlineClass forced) =>
        rail.Rollover(staged, cooperative.Allotted, forced.Allotted);

    public static IO<UpdateReceipt> ConductPending(UpdateRail rail, DeadlineClass cooperative, DeadlineClass forced) =>
        rail.PendingRestart
            ? rail.Resume(cooperative.Allotted, forced.Allotted)
            : IO.fail<UpdateReceipt>(new UpdateFault.StagePending(nameof(rail.PendingRestart)));
}

// The progressive-delivery axis: one row per strategy carries its cohort plan, the recover-and-advance
// predicate, and the inter-wave bake window. Plan folds the roster into the ordered cohort sequence the
// strategy shape dictates; Next admits the following cohort only on a held health-pass. A second roll
// state machine beside this axis is the rejected form.
public sealed record RollPlan(Seq<Seq<RosterEntry>> Cohorts, Duration BakeWindow);

[SmartEnum<string>]
[KeyMemberEqualityComparer<UpdateKeyPolicy, string>]
[KeyMemberComparer<UpdateKeyPolicy, string>]
public sealed partial class RollStrategy {
    public static readonly RollStrategy Canary = new("canary", wavePercent: 0, bake: Duration.FromSeconds(120),
        plan: static nodes => nodes.IsEmpty ? [] : Seq(nodes.Take(1).ToSeq()).Add(nodes.Skip(1).ToSeq()).Filter(static c => !c.IsEmpty));
    public static readonly RollStrategy BlueGreen = new("blue-green", wavePercent: 50, bake: Duration.Zero,
        plan: static nodes => nodes.IsEmpty ? [] : Chunk(nodes, int.Max((nodes.Count + 1) / 2, 1)));
    public static readonly RollStrategy LinearWave = new("linear-wave", wavePercent: 25, bake: Duration.FromSeconds(300),
        plan: static nodes => nodes.IsEmpty ? [] : Chunk(nodes, int.Max(nodes.Count / 4, 1)));

    public int WavePercent { get; }
    public Duration Bake { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Seq<RosterEntry>> Cohorts(Seq<RosterEntry> nodes);

    public RollPlan Plan(Seq<RosterEntry> nodes) => new(Cohorts(nodes), Bake);

    // A health-pass admits the next cohort; a NotServing node in the just-rolled cohort halts the wave.
    public bool Advances(Seq<FleetRollReceipt> cohort) => cohort.ForAll(static row => row.Serving == ServingStatus.Serving);

    static Seq<Seq<RosterEntry>> Chunk(Seq<RosterEntry> nodes, int width) =>
        nodes.AsEnumerable().Chunk(width).Select(static c => c.ToSeq()).ToSeq();
}

public readonly record struct FleetRollReceipt(
    int Wave,
    string Strategy,
    int Pid,
    UpdateOutcome Outcome,
    ServingStatus Serving,
    int Remaining,
    Instant At);

public static class FleetRoll {
    // The bake delay rides the injected clock-driven `bake` delegate (the SchedulePort cadence the strategy
    // BakeWindow reads, test-fakeable through the same TimeProvider the spine injects), never an ambient
    // Task.Delay — so a LinearWave bakes its 300s window and a Canary holds its 120s probe between cohorts.
    public static IO<Seq<FleetRollReceipt>> Roll(
        PeerRoster roster,
        RollStrategy strategy,
        Func<RosterEntry, IO<UpdateReceipt>> rollNode,
        Func<RosterEntry, IO<HealthReport>> probe,
        Func<Duration, IO<Unit>> bake,
        ReceiptSinkPort sink,
        IClock clock,
        TenantContext tenant) =>
        strategy.Plan(roster.Attached) is var plan
            ? Wave(plan.Cohorts, 0, strategy, plan, roster, rollNode, probe, bake, sink, clock, tenant)
            : IO.pure(Seq<FleetRollReceipt>());

    static IO<Seq<FleetRollReceipt>> Wave(
        Seq<Seq<RosterEntry>> cohorts, int index, RollStrategy strategy, RollPlan plan, PeerRoster roster,
        Func<RosterEntry, IO<UpdateReceipt>> rollNode, Func<RosterEntry, IO<HealthReport>> probe,
        Func<Duration, IO<Unit>> bake, ReceiptSinkPort sink, IClock clock, TenantContext tenant) =>
        cohorts.IsEmpty
            ? IO.pure(Seq<FleetRollReceipt>())
            : cohorts.Head.Value
                .TraverseM(node =>
                    from rolled in rollNode(node)
                    from report in probe(node)
                    from at in IO.lift(() => clock.GetCurrentInstant())
                    let receipt = new FleetRollReceipt(index, strategy.Key, node.Pid, rolled.Outcome, Serving(report), roster.Attached.Count, at)
                    from _ in sink.Send(Correlation.Mint(), tenant, TelemetrySource.AppHost.Key, nameof(FleetRoll), JsonSerializer.SerializeToElement(receipt, AppHostWireContext.Default.FleetRollReceipt))
                    select receipt)
                .As()
                .Bind(here => strategy.Advances(here) && !cohorts.Tail.IsEmpty
                    // health-pass with a following cohort: bake the inter-wave dwell, then advance.
                    ? (plan.BakeWindow > Duration.Zero ? bake(plan.BakeWindow) : IO.pure(unit))
                        .Bind(_ => Wave(cohorts.Tail, index + 1, strategy, plan, roster, rollNode, probe, bake, sink, clock, tenant))
                        .Map(rest => here + rest)
                    : IO.pure(here));

    static ServingStatus Serving(HealthReport report) =>
        report.Status == HealthStatus.Unhealthy ? ServingStatus.NotServing : ServingStatus.Serving;
}
```

```mermaid
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

## [06]-[RESEARCH]

- [STAGED_FEED]: the production feed URIs per channel replace the placeholder authority on the `UpdateChannel` rows once the release-feed host is provisioned.
- [TRUST_ANCHOR]: the pinned `trusted_root.json` the `FileTrustRootProvider` loads and the expected `CertificateIdentity.ForGitHubActions(owner, repository)` signer settle against the actual release-signing identity once the `vpk` build pipeline publishes the cosign bundle (`*.sigstore.json`) beside each release asset; the `SupplyChainGate.Runtime.Staging` directory is the Velopack packages dir the `UpdateManager` downloads into, where `Bundle` resolves the per-asset bundle file. The connected-node `TufTrustRootProvider` with a `CustomTrustedRoot` + local cache is the online anchor variant; the air-gapped node pins the offline `FileTrustRootProvider` so the verify path is fully hermetic.
- [CONTRACT_RANGE]: the per-channel `VersionRange` contract on `TrustPolicy` settles against the host's plugin/release compatibility window — `stable` admits the broadest stable range, `canary` admits the floating prerelease range — resolved once the release-versioning policy is fixed; `VersionRange.FindBestMatch` selects the newest in-range release when a feed offers several candidates.
