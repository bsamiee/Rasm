using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor.Tests;

// --- [MODELS] ----------------------------------------------------------------------------

internal static class SessionGens {
    public static readonly Guid Sid = Guid.Parse(input: "6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f");
    public static readonly string ReportDir = Directory.CreateTempSubdirectory(prefix: "rbx-spec-").FullName;
    public static readonly BundleInfo Bundle = new(AppPath: "/Applications/RhinoWIP.app", CFBundleName: "RhinoWIP", CFBundleExecutable: "Rhinoceros", CFBundleVersion: "9.0.26153");
    public static readonly HostFingerprint Fingerprint = new(BundleVersion: "9.0.26153.12416", RhinoCommonVersion: "9.0.26153.12416", Grasshopper2Version: "2.0.0", RuntimeVersion: "10.0.2");
    public static readonly EndpointRecord Endpoint = EndpointRecord.Create(pipeName: "rbx-spec", rhinoPid: 4242, rhinoStartedAtUnixMs: 1_765_432_000_000, contractVersion: 1, shellVersion: "1.0.0", rhinoVersion: "9.0.26153", fault: "");
    public static readonly LiveHost Host = new(Pid: 4242, StartedAtUnixMs: 1_765_432_000_000, Endpoint: Endpoint, Fingerprint: Fingerprint);
    public static readonly Handshake Ours = new(ContractVersion: 1, SenderVersion: "supervisor", Capabilities: [], Fingerprint: null, Endpoint: null);
    public static readonly Handshake Peer = new(ContractVersion: 1, SenderVersion: "shell", Capabilities: [new CapabilityEntry(Key: "rpc.streamjsonrpc", Outcome: PhaseStatus.Ok, Receipt: "2.25.25")], Fingerprint: Fingerprint, Endpoint: Endpoint);
    public static readonly CargoManifest Manifest = new(SessionId: Sid, ReportDir: "/tmp/rbx", ContentHash: "xx64:abc", StagePath: "/tmp/stage", HostPlugins: [], BuiltAgainst: Fingerprint, ScenarioAssemblies: ["Rasm.Rhino.Tests.dll"]);
    public static readonly CargoReceipt Cargo = new(ContentHash: "xx64:abc", SwapMs: 100.0, Scenarios: [], Capabilities: [new CapabilityEntry(Key: "gh2.dataflow", Outcome: PhaseStatus.Unsupported, Receipt: "0b render-only")]);
    public static readonly SessionState.Ready Ready = new(Host: Host, Peer: Peer);
    public static readonly SessionState.Running Running = new(Host: Host, Cargo: Cargo, Done: Seq(value: Receipt(name: "blocks.baseline", status: PhaseStatus.Ok)), Remaining: Seq(value: Entry(name: "blocks.next")));
    public static readonly SessionState.Quitting Quitting = new(Host: Host, Rung: SessionPhase.QuitAe, RungStartedMs: 1_765_432_100_000);
    public static readonly SessionState.Faulted Faulted = new(Fault: new BridgeFault.BusyHeld(HolderPid: 777, AgeSeconds: 12.0), At: SessionPhase.Connect, Done: Seq<ScenarioReceipt>());

    public static SessionState[] NonTerminal => [
        new SessionState.Idle(Bundle: Bundle),
        new SessionState.Connecting(Host: Host, PollsRemaining: 360),
        new SessionState.Negotiating(Host: Host, Ours: Ours),
        new SessionState.Loading(Host: Host, Peer: Peer, Manifest: Manifest),
        Running,
        Quitting,
        Faulted,
    ];

    public static ScenarioEntry Entry(string name) => new(Theme: "blocks", Name: name, Requires: [], BudgetMs: 30_000);

    public static BridgeEvent.FactCase Fact(long sequence, string key, string? scenario = null) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: 1.0, jsonTypeInfo: BridgeJsonContext.Default.Double)) { Stamp = Stamp(sequence: sequence, scenario: scenario) };

    public static SessionEnvelope Fold(SessionState final, Seq<BridgeEvent> stream = default, (long Count, long LastSequence) spoolTail = default) =>
        SessionFold.Run(runId: Sid.ToString(format: "n"), verb: new SupervisorVerb.Status(), final: final, stream: stream, spoolTail: spoolTail, reportDir: ReportDir);

    public static BridgeEvent.PhaseCase Phase(long sequence, SessionPhase phase, PhaseStatus status, BridgeFault? fault = null) =>
        new(Phase: phase, Status: status, DurationMs: 5.0, Fault: fault) { Stamp = Stamp(sequence: sequence) };

    public static ScenarioReceipt Receipt(string name, PhaseStatus status, BridgeFault? fault = null) =>
        new(Scenario: name, Status: status, DurationMs: 1.0, Fault: fault);

    public static EventStamp Stamp(long sequence, string? scenario = null) =>
        new(SessionId: Sid, Sequence: sequence, AtUnixMs: 1_765_432_100_000 + sequence, Scenario: scenario);
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public sealed class PolicyLaws {
    [Fact]
    public void EveryNonTerminalStateCarriesADeadlineRow() {
        Assert.All(collection: SessionGens.NonTerminal, action: static state =>
            Assert.True(condition: SessionPolicy.Default.DeadlineFor(state: state).IsSome));
        Assert.True(condition: SessionPolicy.Default.DeadlineFor(state: SessionGens.Ready).IsNone);
        Assert.True(condition: SessionPolicy.Default.DeadlineFor(state: new SessionState.Terminal(Envelope: SessionGens.Fold(final: SessionGens.Faulted))).IsNone);
    }

    [Fact]
    public void ExecuteBudgetSumsPerScenarioBudgetsNotTheirMax() {
        ScenarioEntry[] batch = [SessionGens.Entry(name: "blocks.a"), SessionGens.Entry(name: "blocks.b"), SessionGens.Entry(name: "blocks.c")];
        TimeSpan budget = SessionPolicy.Default.ExecuteBudget(selected: batch);
        Assert.Equal(expected: TimeSpan.FromSeconds(value: 90), actual: budget);
        Assert.NotEqual(expected: TimeSpan.FromSeconds(value: 30), actual: budget);
    }

    [Fact]
    public void ExecuteBudgetSubstitutesTheDefaultForUnsetBudgets() =>
        Assert.Equal(
            expected: TimeSpan.FromSeconds(value: 60),
            actual: SessionPolicy.Default.ExecuteBudget(selected: [
                new ScenarioEntry(Theme: "blocks", Name: "blocks.a", Requires: [], BudgetMs: 0),
                new ScenarioEntry(Theme: "blocks", Name: "blocks.b", Requires: [], BudgetMs: -5),
            ]));

    [Fact]
    public void ExecuteBudgetClampsAtTheSessionDeadline() =>
        Assert.Equal(
            expected: SessionPolicy.Default.SessionDeadline,
            actual: SessionPolicy.Default.ExecuteBudget(selected: [.. Enumerable.Range(start: 0, count: 30)
                .Select(selector: static index => new ScenarioEntry(
                    Theme: "blocks",
                    Name: string.Create(provider: System.Globalization.CultureInfo.InvariantCulture, $"blocks.s{index}"),
                    Requires: [], BudgetMs: 30_000))]));

    [Fact]
    public void ExecuteBudgetOfAnEmptySelectionIsTheDefault() =>
        Assert.Equal(expected: SessionPolicy.Default.ScenarioDefaultBudget, actual: SessionPolicy.Default.ExecuteBudget(selected: []));
}

public sealed class DispatchLaws {
    private static readonly SessionPolicy Policy = SessionPolicy.Default;
    private static readonly SessionSignal Exit = new SessionSignal.HostExited(Pid: 4242, AtUnixMs: 1_765_432_200_000);
    private static readonly SessionSignal Silent = new SessionSignal.HeartbeatSilent(SilentFor: TimeSpan.FromMinutes(value: 2));
    private static readonly SessionSignal Overrun = new SessionSignal.DeadlineHit(Phase: SessionPhase.Execute, Elapsed: TimeSpan.FromHours(value: 2));

    private static SessionSignal[] Signals => [
        new SessionSignal.HostExited(Pid: 4242, AtUnixMs: 1L),
        new SessionSignal.HeartbeatSilent(SilentFor: TimeSpan.FromMinutes(value: 2)),
        new SessionSignal.DeadlineHit(Phase: SessionPhase.Execute, Elapsed: TimeSpan.FromHours(value: 2)),
    ];

    [Fact]
    public void EveryNonTerminalStateReachesFaultedWithinTheLadderDepth() =>
        Assert.All(collection: SessionGens.NonTerminal, action: static state =>
            Assert.IsType<SessionState.Faulted>(@object: Enumerable.Range(start: 0, count: 3)
                .Aggregate(seed: state, func: static (cursor, _) => SessionDispatch.Apply(state: cursor, signal: Overrun, policy: Policy))));

    [Fact]
    public void PrematureDeadlineAndSilenceSignalsHoldTheState() {
        SessionSignal earlyDeadline = new SessionSignal.DeadlineHit(Phase: SessionPhase.Execute, Elapsed: TimeSpan.FromMilliseconds(value: 1));
        SessionSignal earlySilence = new SessionSignal.HeartbeatSilent(SilentFor: TimeSpan.FromMilliseconds(value: 1));
        Assert.All(collection: SessionGens.NonTerminal, action: state => {
            Assert.Same(expected: state, actual: SessionDispatch.Apply(state: state, signal: earlyDeadline, policy: Policy));
            Assert.Same(expected: state, actual: SessionDispatch.Apply(state: state, signal: earlySilence, policy: Policy));
        });
    }

    [Fact]
    public void IdleIgnoresHostSignals() {
        SessionState idle = new SessionState.Idle(Bundle: SessionGens.Bundle);
        Assert.Same(expected: idle, actual: SessionDispatch.Apply(state: idle, signal: Exit, policy: Policy));
        Assert.Same(expected: idle, actual: SessionDispatch.Apply(state: idle, signal: Silent, policy: Policy));
    }

    [Fact]
    public void ConnectingHostExitMapsToLaunchFailed() {
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(@object: SessionDispatch.Apply(
            state: new SessionState.Connecting(Host: SessionGens.Host, PollsRemaining: 360), signal: Exit, policy: Policy));
        _ = Assert.IsType<BridgeFault.LaunchFailed>(@object: faulted.Fault);
        Assert.Same(expected: SessionPhase.Connect, actual: faulted.At);
    }

    [Fact]
    public void ConnectingSilenceReadsDialogSuspected() =>
        _ = Assert.IsType<BridgeFault.DialogSuspected>(@object: Assert.IsType<SessionState.Faulted>(@object: SessionDispatch.Apply(
            state: new SessionState.Connecting(Host: SessionGens.Host, PollsRemaining: 360), signal: Silent, policy: Policy)).Fault);

    [Fact]
    public void RunningHostExitRecordsTheCrashAgainstTheInFlightScenario() {
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(@object: SessionDispatch.Apply(state: SessionGens.Running, signal: Exit, policy: Policy));
        BridgeFault.RhinoCrash crash = Assert.IsType<BridgeFault.RhinoCrash>(@object: faulted.Fault);
        Assert.Equal(expected: "blocks.next", actual: crash.Scenario);
        Assert.Same(expected: SessionPhase.Execute, actual: faulted.At);
        Assert.Equal(expected: SessionGens.Running.Done, actual: faulted.Done);
    }

    [Fact]
    public void RunningSilenceReadsUiWedgedNamingTheScenario() {
        BridgeFault.UiWedged wedged = Assert.IsType<BridgeFault.UiWedged>(@object: Assert.IsType<SessionState.Faulted>(
            @object: SessionDispatch.Apply(state: SessionGens.Running, signal: Silent, policy: Policy)).Fault);
        Assert.Equal(expected: "blocks.next", actual: wedged.Scenario);
    }

    [Fact]
    public void RunningDeadlineReadsExecuteDeadline() =>
        _ = Assert.IsType<BridgeFault.ExecuteDeadline>(@object: Assert.IsType<SessionState.Faulted>(
            @object: SessionDispatch.Apply(state: SessionGens.Running, signal: Overrun, policy: Policy)).Fault);

    [Fact]
    public void QuitLadderEscalatesAeForceKillThenWedges() {
        SessionState.Quitting force = Assert.IsType<SessionState.Quitting>(@object: SessionDispatch.Apply(state: SessionGens.Quitting, signal: Overrun, policy: Policy));
        Assert.Same(expected: SessionPhase.QuitForce, actual: force.Rung);
        SessionState.Quitting kill = Assert.IsType<SessionState.Quitting>(@object: SessionDispatch.Apply(state: force, signal: Overrun, policy: Policy));
        Assert.Same(expected: SessionPhase.QuitKill, actual: kill.Rung);
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(@object: SessionDispatch.Apply(state: kill, signal: Overrun, policy: Policy));
        BridgeFault.UiWedged wedged = Assert.IsType<BridgeFault.UiWedged>(@object: faulted.Fault);
        Assert.Equal(expected: SessionPhase.QuitKill.Key, actual: wedged.Scenario);
    }

    [Fact]
    public void QuitHostExitConfirmsTheRungClean() =>
        Assert.Same(expected: SessionGens.Quitting, actual: SessionDispatch.Apply(state: SessionGens.Quitting, signal: Exit, policy: Policy));

    [Fact]
    public void FaultedAndTerminalAbsorbEverySignal() {
        SessionState terminal = new SessionState.Terminal(Envelope: SessionGens.Fold(final: SessionGens.Faulted));
        Assert.All(collection: Signals, action: signal => {
            Assert.Same(expected: SessionGens.Faulted, actual: SessionDispatch.Apply(state: SessionGens.Faulted, signal: signal, policy: Policy));
            Assert.Same(expected: terminal, actual: SessionDispatch.Apply(state: terminal, signal: signal, policy: Policy));
        });
    }
}

public sealed class FoldLaws {
    [Fact]
    public void CleanRunFoldsOkWithAnEmptyFirstFailurePair() {
        SessionEnvelope envelope = SessionGens.Fold(
            final: SessionGens.Running with { Remaining = Seq<ScenarioEntry>() },
            stream: Seq<BridgeEvent>(a: SessionGens.Phase(sequence: 1, phase: SessionPhase.Connect, status: PhaseStatus.Ok), b: SessionGens.Phase(sequence: 2, phase: SessionPhase.Hello, status: PhaseStatus.Ok)));
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Status);
        Assert.Equal(expected: string.Empty, actual: envelope.FirstFailure);
        Assert.Null(@object: envelope.FirstFaultPhase);
        Assert.Equal(expected: 0, actual: envelope.Status.ExitCode);
    }

    [Fact]
    public void AllSkippedFoldsOkAtTheRootWhileReceiptsReadSkipped() {
        SessionEnvelope envelope = SessionGens.Fold(final: SessionGens.Running with {
            Done = Seq(a: SessionGens.Receipt(name: "a", status: PhaseStatus.Skipped), b: SessionGens.Receipt(name: "b", status: PhaseStatus.Skipped)),
            Remaining = Seq<ScenarioEntry>(),
        });
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Status);
        Assert.All(collection: envelope.Scenarios, action: static receipt => Assert.Same(expected: PhaseStatus.Skipped, actual: receipt.Status));
    }

    [Fact]
    public void RemainingScenariosFoldSkippedIntoTheReceipts() {
        SessionEnvelope envelope = SessionGens.Fold(final: SessionGens.Running);
        Assert.Equal(expected: 2, actual: envelope.Scenarios.Length);
        Assert.Equal(expected: "blocks.next", actual: envelope.Scenarios[1].Scenario);
        Assert.Same(expected: PhaseStatus.Skipped, actual: envelope.Scenarios[1].Status);
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Status);
    }

    [Fact]
    public void SessionFaultOutranksPhaseEvidence() {
        SessionEnvelope envelope = SessionGens.Fold(
            final: SessionGens.Faulted,
            stream: Seq<BridgeEvent>(value: SessionGens.Phase(sequence: 1, phase: SessionPhase.Launch, status: PhaseStatus.Failed, fault: new BridgeFault.LaunchFailed(Detail: "phase-level"))));
        Assert.Equal(expected: SessionGens.Faulted.Fault.Prescription, actual: envelope.FirstFailure);
        Assert.Same(expected: SessionPhase.Connect, actual: envelope.FirstFaultPhase);
        Assert.Same(expected: PhaseStatus.Busy, actual: envelope.Status);
        Assert.Equal(expected: 5, actual: envelope.Status.ExitCode);
    }

    [Fact]
    public void FirstFailingPhaseWinsInWireOrder() {
        BridgeFault loadFault = new BridgeFault.NugetLockDrift(Detail: "NU1004 Rasm.Bridge.Contract");
        SessionEnvelope envelope = SessionGens.Fold(
            final: SessionGens.Running with { Done = Seq<ScenarioReceipt>(), Remaining = Seq<ScenarioEntry>() },
            stream: Seq<BridgeEvent>(
                a: SessionGens.Phase(sequence: 3, phase: SessionPhase.Execute, status: PhaseStatus.Failed, fault: new BridgeFault.ExecuteDeadline(Scenario: "late", ElapsedMs: 1.0)),
                b: SessionGens.Phase(sequence: 1, phase: SessionPhase.Connect, status: PhaseStatus.Ok),
                c: SessionGens.Phase(sequence: 2, phase: SessionPhase.Load, status: PhaseStatus.Failed, fault: loadFault)));
        Assert.Same(expected: SessionPhase.Load, actual: envelope.FirstFaultPhase);
        Assert.Equal(expected: loadFault.Prescription, actual: envelope.FirstFailure);
    }

    [Fact]
    public void FirstFailureTruncatesAtTheWireCap() {
        SessionEnvelope envelope = SessionGens.Fold(final: new SessionState.Faulted(
            Fault: new BridgeFault.LaunchFailed(Detail: new string(c: 'x', count: 512)), At: SessionPhase.Launch, Done: Seq<ScenarioReceipt>()));
        Assert.Equal(expected: 256, actual: envelope.FirstFailure.Length);
    }

    [Fact]
    public void StatusFoldIsOrderIndependentForEveryStatusPair() =>
        Assert.All(
            collection: PhaseStatus.Items.SelectMany(collectionSelector: static _ => PhaseStatus.Items, resultSelector: static (left, right) => (left, right)),
            action: static pair => Assert.Same(
                expected: SessionGens.Fold(
                    final: SessionGens.Running with { Done = Seq<ScenarioReceipt>(), Remaining = Seq<ScenarioEntry>() },
                    stream: Seq<BridgeEvent>(a: SessionGens.Phase(sequence: 1, phase: SessionPhase.Load, status: pair.left), b: SessionGens.Phase(sequence: 2, phase: SessionPhase.Execute, status: pair.right))).Status,
                actual: SessionGens.Fold(
                    final: SessionGens.Running with { Done = Seq<ScenarioReceipt>(), Remaining = Seq<ScenarioEntry>() },
                    stream: Seq<BridgeEvent>(a: SessionGens.Phase(sequence: 1, phase: SessionPhase.Load, status: pair.right), b: SessionGens.Phase(sequence: 2, phase: SessionPhase.Execute, status: pair.left))).Status));

    [Fact]
    public void EnvelopeEvidenceCarriesOnlyFactAndCaptureCases() {
        SessionEnvelope envelope = SessionGens.Fold(
            final: SessionGens.Faulted,
            stream: Seq<BridgeEvent>(
                SessionGens.Fact(sequence: 1, key: "cargo.swapMs"),
                new BridgeEvent.CaptureCase(Path: "/tmp/rbx/a.png", Width: 1280, Height: 720, OnFailure: true) { Stamp = SessionGens.Stamp(sequence: 2) },
                SessionGens.Phase(sequence: 3, phase: SessionPhase.Execute, status: PhaseStatus.Ok),
                new BridgeEvent.ProgressCase(Done: 1, Total: 2) { Stamp = SessionGens.Stamp(sequence: 4) },
                new BridgeEvent.HostExceptionCase(Report: "swallowed") { Stamp = SessionGens.Stamp(sequence: 5) }),
            spoolTail: (0L, 5L));
        Assert.Equal(expected: 2, actual: envelope.Evidence.Length);
        Assert.All(collection: envelope.Evidence, action: static evt => Assert.True(condition: evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase));
    }

    [Fact]
    public void SpoolDivergenceEmitsTheReconciliationFact() {
        Seq<BridgeEvent> stream = Seq<BridgeEvent>(a: SessionGens.Fact(sequence: 1, key: "a", scenario: "probe"), b: SessionGens.Fact(sequence: 2, key: "b", scenario: "probe"));
        SessionEnvelope diverged = SessionGens.Fold(final: SessionGens.Faulted, stream: stream, spoolTail: (4L, 2L));
        BridgeEvent.FactCase reconciliation = Assert.IsType<BridgeEvent.FactCase>(@object: diverged.Evidence[^1]);
        Assert.Equal(expected: "evidence.divergence", actual: reconciliation.Key);
        Assert.Equal(expected: 4L, actual: reconciliation.Value.GetProperty(propertyName: "spool").GetInt64());
        Assert.Equal(expected: 2L, actual: reconciliation.Value.GetProperty(propertyName: "relayed").GetInt64());
        Assert.Equal(expected: 3L, actual: reconciliation.Stamp.Sequence);
        SessionEnvelope reconciled = SessionGens.Fold(final: SessionGens.Faulted, stream: stream, spoolTail: (2L, 2L));
        Assert.DoesNotContain(collection: reconciled.Evidence, filter: static evt => evt is BridgeEvent.FactCase { Key: "evidence.divergence" });
    }

    [Fact]
    public void RelayOnlyLifecycleFactsDoNotTriggerSpoolDivergence() {
        Seq<BridgeEvent> stream = Seq<BridgeEvent>(
            a: SessionGens.Fact(sequence: 1, key: "cargo.swapped"),
            b: SessionGens.Fact(sequence: 2, key: "blocks.fact", scenario: "blocks.baseline"));
        SessionEnvelope reconciled = SessionGens.Fold(final: SessionGens.Faulted, stream: stream, spoolTail: (1L, 2L));
        Assert.DoesNotContain(collection: reconciled.Evidence, filter: static evt => evt is BridgeEvent.FactCase { Key: "evidence.divergence" });
    }

    [Fact]
    public void TerminalFoldIsIdempotent() {
        SessionEnvelope envelope = SessionGens.Fold(final: SessionGens.Faulted);
        Assert.Same(expected: envelope, actual: SessionGens.Fold(final: new SessionState.Terminal(Envelope: envelope)));
    }

    [Fact]
    public void ExitCodeTaxonomyPassesThrough() {
        Assert.All(
            collection: ((BridgeFault Fault, PhaseStatus Status, int Exit)[])[
                (new BridgeFault.CapabilityAbsent(Capability: "gh2.dataflow", ProbeReceipt: "0b"), PhaseStatus.Unsupported, 3),
                (new BridgeFault.BusyHeld(HolderPid: 777, AgeSeconds: 1.0), PhaseStatus.Busy, 5),
                (new BridgeFault.ExecuteDeadline(Scenario: "a", ElapsedMs: 1.0), PhaseStatus.Timeout, 5),
                (new BridgeFault.LaunchFailed(Detail: "gone"), PhaseStatus.Failed, 1),
            ],
            action: static row => {
                SessionEnvelope envelope = SessionGens.Fold(final: new SessionState.Faulted(Fault: row.Fault, At: SessionPhase.Launch, Done: Seq<ScenarioReceipt>()));
                Assert.Same(expected: row.Status, actual: envelope.Status);
                Assert.Equal(expected: row.Exit, actual: envelope.Status.ExitCode);
            });
    }

    [Fact]
    public void HostAndCapabilitiesProjectFromTheFinalState() {
        SessionEnvelope ready = SessionGens.Fold(final: SessionGens.Ready);
        Assert.Equal(expected: SessionGens.Fingerprint, actual: ready.Host);
        Assert.Equal(expected: SessionGens.Peer.Capabilities, actual: ready.Capabilities);
        SessionEnvelope running = SessionGens.Fold(final: SessionGens.Running);
        Assert.Equal(expected: SessionGens.Fingerprint, actual: running.Host);
        Assert.Equal(expected: SessionGens.Cargo.Capabilities, actual: running.Capabilities);
        SessionEnvelope faulted = SessionGens.Fold(final: SessionGens.Faulted);
        Assert.Equal(expected: new HostFingerprint(BundleVersion: string.Empty, RhinoCommonVersion: string.Empty, Grasshopper2Version: string.Empty, RuntimeVersion: string.Empty), actual: faulted.Host);
        Assert.Empty(collection: faulted.Capabilities);
    }
}

// The reference lifecycle: author emits candidates under the theme root, review + rename promotes,
// an unpromoted root degrades instead of failing, and only reviewed+matched rows certify.
public sealed class ReferenceLifecycleLaws {
    private const string Scenario = "blocks.Baseline";

    [Fact]
    public void AuthorModeWritesTheCandidateUnderTheThemeRoot() {
        string root = Directory.CreateTempSubdirectory(prefix: "rbx-refs-").FullName;
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Author, root: root, stream: ReferenceStream());
        string candidate = Path.Combine(root, "blocks", "Baseline.candidate.reference.json");
        Assert.True(condition: File.Exists(path: candidate));
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Candidate, actual: row.Admission);
        Assert.Equal(expected: candidate, actual: row.ReferencePath);
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Scenarios[0].ScenarioStatus);
    }

    [Fact]
    public void VerifyOverAnEmptyRootDegradesAsUnpromoted() {
        string root = Directory.CreateTempSubdirectory(prefix: "rbx-refs-").FullName;
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Verify, root: root, stream: ReferenceStream());
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Unpromoted, actual: row.Admission);
        Assert.Same(expected: PhaseStatus.Degraded, actual: envelope.Scenarios[0].ScenarioStatus);
        Assert.Same(expected: PhaseStatus.Degraded, actual: envelope.Status);
        Assert.Equal(expected: 2, actual: envelope.Status.ExitCode);
    }

    [Fact]
    public void ReviewedMatchingReferenceCertifiesTheScenario() {
        string root = PromotedRoot(expected: 42.0);
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Verify, root: root, stream: ReferenceStream());
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Matched, actual: row.Admission);
        Assert.True(condition: row.Matched);
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Scenarios[0].ScenarioStatus);
        Assert.Same(expected: PhaseStatus.Ok, actual: envelope.Status);
    }

    [Fact]
    public void ReviewedMismatchFailsTheScenarioWithTheDetail() {
        string root = PromotedRoot(expected: 43.0);
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Verify, root: root, stream: ReferenceStream());
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Mismatch, actual: row.Admission);
        Assert.Same(expected: PhaseStatus.Failed, actual: envelope.Scenarios[0].ScenarioStatus);
        Assert.Equal(expected: "reference.mismatch", actual: envelope.Scenarios[0].FirstScenarioFailure);
    }

    [Fact]
    public void CandidateFilesNeverSatisfyVerify() {
        string root = Directory.CreateTempSubdirectory(prefix: "rbx-refs-").FullName;
        _ = Directory.CreateDirectory(path: Path.Combine(root, "blocks"));
        File.WriteAllText(
            path: Path.Combine(root, "blocks", "Baseline.candidate.reference.json"),
            contents: JsonSerializer.Serialize(value: ReviewedRows(expected: 42.0, admission: ReferenceAdmission.Candidate), jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidenceArray));
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Verify, root: root, stream: ReferenceStream());
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Unpromoted, actual: row.Admission);
        Assert.NotSame(expected: PhaseStatus.Ok, actual: envelope.Scenarios[0].ScenarioStatus);
    }

    [Fact]
    public void SilentScenarioOverAPromotedRootFailsAsMissing() {
        string root = PromotedRoot(expected: 42.0);
        SessionEnvelope envelope = FoldVerify(mode: EvidenceMode.Verify, root: root, stream: Seq<BridgeEvent>());
        ReferenceEvidenceResult row = Assert.Single(collection: envelope.Scenarios[0].ReferenceResults);
        Assert.Same(expected: ReferenceAdmission.Missing, actual: row.Admission);
        Assert.Same(expected: PhaseStatus.Failed, actual: envelope.Scenarios[0].ScenarioStatus);
    }

    private static SessionEnvelope FoldVerify(EvidenceMode mode, string root, Seq<BridgeEvent> stream) =>
        SessionFold.Run(
            runId: SessionGens.Sid.ToString(format: "n"),
            verb: new SupervisorVerb.Verify(Selection: new ScenarioSelection.AllCase(), ClosureManifest: "closure.json", EvidenceMode: mode),
            final: SessionGens.Running with { Done = Seq(value: SessionGens.Receipt(name: Scenario, status: PhaseStatus.Ok)), Remaining = Seq<ScenarioEntry>() },
            stream: stream,
            spoolTail: default,
            reportDir: Directory.CreateTempSubdirectory(prefix: "rbx-report-").FullName,
            evidenceMode: mode,
            referenceRoots: [new ReferenceRoot(Assembly: "Rasm.Scenarios.dll", Theme: "", Path: root)]);

    private static Seq<BridgeEvent> ReferenceStream() {
        using JsonDocument payload = JsonDocument.Parse(json: """{"name":"volume","actual":42.0,"tolerance":{"mode":"exact","absolute":0,"relative":0}}""");
        return Seq<BridgeEvent>(value: new BridgeEvent.FactCase(Key: "reference.volume", Value: payload.RootElement.Clone()) {
            Stamp = SessionGens.Stamp(sequence: 1, scenario: Scenario),
        });
    }

    private static ReferenceEvidence[] ReviewedRows(double expected, ReferenceAdmission admission) => [
        new ReferenceEvidence(
            Name: new EvidenceName(Key: "volume"), Class: EvidenceClass.CertifiedReference,
            Expected: JsonSerializer.SerializeToElement(value: expected, jsonTypeInfo: BridgeJsonContext.Default.Double),
            Tolerance: new ReferenceTolerance(Mode: "exact", Absolute: 0.0, Relative: 0.0),
            Admission: admission, ReviewedBy: "spec", ReviewedAt: "2026-01-01T00:00:00Z"),
    ];

    private static string PromotedRoot(double expected) {
        string root = Directory.CreateTempSubdirectory(prefix: "rbx-refs-").FullName;
        _ = Directory.CreateDirectory(path: Path.Combine(root, "blocks"));
        File.WriteAllText(
            path: Path.Combine(root, "blocks", "Baseline.reference.json"),
            contents: JsonSerializer.Serialize(value: ReviewedRows(expected: expected, admission: ReferenceAdmission.Reviewed), jsonTypeInfo: BridgeJsonContext.Default.ReferenceEvidenceArray));
        return root;
    }
}

public sealed class VerbLaws {
    [Fact]
    public void ParseAdmitsEveryVerbShape() {
        SupervisorVerb.Verify verify = Assert.IsType<SupervisorVerb.Verify>(@object: Succ(argv: ["verify", """{"$type":"themes","themes":["blocks"]}""", "/tmp/closure.json"]));
        ScenarioSelection.ThemesCase themes = Assert.IsType<ScenarioSelection.ThemesCase>(@object: verify.Selection);
        Assert.Equal(expected: ["blocks"], actual: themes.Themes);
        Assert.Equal(expected: "/tmp/closure.json", actual: verify.ClosureManifest);
        Assert.Same(expected: EvidenceMode.Verify, actual: verify.EvidenceMode);
        Assert.Same(expected: EvidenceMode.Author, actual: Assert.IsType<SupervisorVerb.Verify>(
            @object: Succ(argv: ["verify", """{"$type":"all"}""", "/tmp/closure.json", "author"])).EvidenceMode);
        _ = Assert.IsType<SupervisorVerb.Status>(@object: Succ(argv: ["status"]));
        Assert.Equal(expected: "/tmp/p.yak", actual: Assert.IsType<SupervisorVerb.Redeploy>(@object: Succ(argv: ["redeploy", "/tmp/p.yak"])).PackagePath);
        _ = Assert.IsType<SupervisorVerb.Quit>(@object: Succ(argv: ["quit"]));
    }

    [Fact]
    public void ParseRejectsUnknownShapes() {
        Assert.All(
            collection: (string[][])[
                ["launch"], ["verify"], ["verify", "not-json", "/tmp/closure.json"],
                ["verify", """{"$type":"all"}""", "/tmp/closure.json", "chaos"], ["redeploy"], []],
            action: static argv => Assert.IsType<Fin<SupervisorVerb>.Fail>(@object: Verbs.Parse(argv: argv)));
    }

    [Fact]
    public void HelpDerivesFromTheUnionMetadata() {
        using JsonDocument help = JsonDocument.Parse(json: Verbs.Help());
        string[] verbs = [.. help.RootElement.GetProperty(propertyName: "verbs").EnumerateArray().Select(selector: static verb => verb.GetProperty(propertyName: "verb").GetString() ?? string.Empty)];
        Assert.Equal(expected: ["verify", "status", "redeploy", "quit"], actual: verbs);
        string selectionShape = help.RootElement.GetProperty(propertyName: "verbs")[0].GetProperty(propertyName: "args")[0].GetProperty(propertyName: "shape").GetString() ?? string.Empty;
        Assert.Contains(expectedSubstring: "all|themes|names", actualString: selectionShape, comparisonType: StringComparison.Ordinal);
        string evidenceShape = help.RootElement.GetProperty(propertyName: "verbs")[0].GetProperty(propertyName: "args")[2].GetProperty(propertyName: "shape").GetString() ?? string.Empty;
        Assert.Contains(expectedSubstring: "verify|author", actualString: evidenceShape, comparisonType: StringComparison.Ordinal);
        JsonElement exitCodes = help.RootElement.GetProperty(propertyName: "exitCodes");
        Assert.All(collection: PhaseStatus.Items, action: status =>
            Assert.Equal(expected: status.ExitCode, actual: exitCodes.GetProperty(propertyName: status.Key).GetInt32()));
        Assert.Equal(expected: Verbs.UsageExitCode, actual: exitCodes.GetProperty(propertyName: "usage").GetInt32());
    }

    [Fact]
    public void VerbProjectionsRouteKeysAndEntryPhases() {
        Assert.Equal(expected: "verify", actual: Succ(argv: ["verify", """{"$type":"all"}""", "/tmp/closure.json"]).Key);
        Assert.Same(expected: SessionPhase.Launch, actual: Succ(argv: ["verify", """{"$type":"all"}""", "/tmp/closure.json"]).EntryPhase);
        Assert.Same(expected: SessionPhase.Status, actual: new SupervisorVerb.Status().EntryPhase);
        Assert.Same(expected: SessionPhase.Install, actual: new SupervisorVerb.Redeploy(PackagePath: "/tmp/p.yak").EntryPhase);
        Assert.Same(expected: SessionPhase.QuitAe, actual: new SupervisorVerb.Quit().EntryPhase);
    }

    private static SupervisorVerb Succ(string[] argv) =>
        Assert.IsType<Fin<SupervisorVerb>.Succ>(@object: Verbs.Parse(argv: argv)).Value;
}
