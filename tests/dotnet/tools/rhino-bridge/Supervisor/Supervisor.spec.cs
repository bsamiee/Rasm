using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Rasm.Bridge.Contract;
using Rasm.TestKit;

namespace Rasm.Bridge.Supervisor.Tests;

// --- [MODELS] --------------------------------------------------------------------------

internal static class SessionGens {
    public static readonly Guid Sid = Guid.Parse("6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f");
    public static readonly BundleInfo Bundle = new(Path.Combine(BundleInfo.ApplicationsDirectory, "RhinoBETA.app"), "RhinoBETA", "Rhinoceros", "9.0.26237");
    public static readonly HostFingerprint Fingerprint = new("9.0.26237.15344", "9.0.26237.15344", "2.0.0", "10.0.2");
    public static readonly EndpointRecord.Live Endpoint = new("rbx-spec", 4242, 1_765_432_000_000, "9.0.26237");
    public static readonly LiveHost Host = new(4242, 1_765_432_000_000, Endpoint, Fingerprint);
    public static readonly LoadedCargo Cargo = new("xx64:abc", 100.0, [], [new CapabilityEntry("gh2.render", PhaseStatus.Ok, "DrawToBitmap")]);
    public static readonly SessionState.Ready Ready = new(Host);
    public static readonly SessionState.Running Running = new(Host, Cargo, Seq(Outcome("blocks.baseline", PhaseStatus.Ok)), Seq(Entry("blocks.next")));
    public static readonly SessionState.Quitting Quitting = new(Host);
    public static readonly SessionState.Faulted Faulted = new(new BridgeFault.BusyHeld(777, 12.0), SessionPhase.Connect, Seq<ScenarioOutcome>());

    public static SessionState[] Phased => [
        new SessionState.Connecting(Host),
        new SessionState.Negotiating(Host),
        new SessionState.Loading(Host),
        Running,
        new SessionState.Unloading(Host, Cargo),
    ];

    public static SessionState[] Absorbing => [new SessionState.Idle(Bundle), Ready, Quitting, Faulted];

    public static ScenarioEntry Entry(string name) => new("blocks", name, [], 30_000);

    public static BridgeEvent.FactCase Fact(long sequence, string key, string? scenario = null) =>
        new(key, JsonSerializer.SerializeToElement(1.0, BridgeJsonContext.Default.Double)) { Stamp = Stamp(sequence, scenario) };

    public static SessionEnvelope Fold(SessionState final, Seq<BridgeEvent> stream = default, (long Count, long LastSequence) spoolTail = default) {
        using TestDirectory report = TestDirectory.Create("rbx-spec-");
        return SessionFold.Run(Sid.ToString("n"), new SupervisorVerb.Status(), final, stream, spoolTail, report.Root.FullName);
    }

    public static BridgeEvent.PhaseCase Phase(long sequence, SessionPhase phase, PhaseStatus status, BridgeFault? fault = null) =>
        new(phase, status, 5.0, fault) { Stamp = Stamp(sequence) };

    public static ScenarioOutcome Outcome(string name, PhaseStatus status, BridgeFault? fault = null) => new(name, status, 1.0, fault);

    public static EventStamp Stamp(long sequence, string? scenario = null) => new(Sid, sequence, 1_765_432_100_000 + sequence, scenario);

    public static SupervisorRuntime Runtime(TestDirectory scratch, SessionPolicy policy, BundleInfo? bundle = null) => new(
        Held: Atom(Option<Lease.Token>.None),
        LiveHostPid: Atom(Option<int>.None),
        Clock: TimeProvider.System,
        Policy: policy,
        ArtifactRoot: scratch.CreateDirectory("reports").FullName,
        PayloadRoot: scratch.CreateDirectory("payload").FullName,
        LeasePath: scratch.File("gate.lease").FullName,
        JournalPath: scratch.File("quits.jsonl").FullName,
        AutosaveDirectory: scratch.CreateDirectory("autosave").FullName,
        DiagnosticReportsDirectory: scratch.CreateDirectory("reports-diag").FullName,
        CrashBaseline: Seq<string>(),
        Bundle: bundle ?? Bundle,
        Root: CancellationToken.None);

    public static int Spawn() {
        using Process sleeper = Process.Start(new ProcessStartInfo("/bin/sleep", "300") { UseShellExecute = false })!;
        return sleeper.Id;
    }

    public static LiveHost StandIn(int pid) {
        long started = Posix.StartedAtUnixMs(pid).IfNone(0L);
        return new LiveHost(pid, started, new EndpointRecord.Live(string.Create(CultureInfo.InvariantCulture, $"rbx-gate-{pid}"), pid, started, "gate"), default);
    }

    public static void AwaitDeath(int pid) {
        long until = Environment.TickCount64 + 5_000;
        while (Posix.Alive(pid) && Environment.TickCount64 < until) {
            Thread.Sleep(50);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PolicyLaws {
    [Fact]
    public void PhasedStatesCarryADeadlineRowAndAbsorbingStatesCarryNone() {
        Assert.All(SessionGens.Phased.Append(SessionGens.Quitting), static state => Assert.True(SessionPolicy.Default.DeadlineFor(state).IsSome));
        Assert.All(SessionGens.Absorbing.Where(static state => state is not SessionState.Quitting), static state => Assert.True(SessionPolicy.Default.DeadlineFor(state).IsNone));
    }

    [Fact]
    public void ExecuteBudgetSumsPerScenarioBudgetsNotTheirMax() =>
        Assert.Equal(TimeSpan.FromSeconds(90), SessionPolicy.Default.ExecuteBudget([SessionGens.Entry("a"), SessionGens.Entry("b"), SessionGens.Entry("c")]));

    [Fact]
    public void ExecuteBudgetSubstitutesTheDefaultForUnsetBudgets() =>
        Assert.Equal(TimeSpan.FromSeconds(60), SessionPolicy.Default.ExecuteBudget([
            new ScenarioEntry("blocks", "blocks.a", [], 0),
            new ScenarioEntry("blocks", "blocks.b", [], -5),
        ]));

    [Fact]
    public void ExecuteBudgetClampsAtTheSessionDeadline() =>
        Assert.Equal(SessionPolicy.Default.SessionDeadline, SessionPolicy.Default.ExecuteBudget([.. Enumerable.Range(0, 30)
            .Select(static index => new ScenarioEntry("blocks", string.Create(CultureInfo.InvariantCulture, $"blocks.s{index}"), [], 30_000))]));

    [Fact]
    public void ExecuteBudgetOfAnEmptySelectionIsTheDefault() =>
        Assert.Equal(SessionPolicy.Default.ScenarioDefaultBudget, SessionPolicy.Default.ExecuteBudget([]));
}

public sealed class DispatchLaws {
    private static readonly SessionPolicy Policy = SessionPolicy.Default;
    private static readonly SessionSignal Exit = new SessionSignal.HostExited(4242, 1_765_432_200_000, Option<CrashFact>.None);
    private static readonly SessionSignal Overrun = new SessionSignal.DeadlineHit(SessionPhase.Execute, TimeSpan.FromHours(2));

    [Fact]
    public void EveryPhasedStateFaultsOnOneOverrun() =>
        Assert.All(SessionGens.Phased, static state => Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(state, Overrun, Policy)));

    [Fact]
    public void PrematureDeadlinesHoldTheState() {
        SessionSignal early = new SessionSignal.DeadlineHit(SessionPhase.Execute, TimeSpan.FromMilliseconds(1));
        Assert.All(SessionGens.Phased.Concat(SessionGens.Absorbing), state => Assert.Same(state, SessionDispatch.Apply(state, early, Policy)));
    }

    [Fact]
    public void AbsorbingStatesIgnoreEverySignal() =>
        Assert.All(SessionGens.Absorbing, static state => {
            Assert.Same(state, SessionDispatch.Apply(state, Exit, Policy));
            Assert.Same(state, SessionDispatch.Apply(state, Overrun, Policy));
        });

    [Fact]
    public void ConnectingHostExitMapsToLaunchFailed() {
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(new SessionState.Connecting(SessionGens.Host), Exit, Policy));
        BridgeFault.LaunchFailed fault = Assert.IsType<BridgeFault.LaunchFailed>(faulted.Fault);
        Assert.Contains("during connect", fault.Detail, StringComparison.Ordinal);
        Assert.Same(SessionPhase.Connect, faulted.At);
    }

    [Fact]
    public void ConnectingDeadlineMapsToConnectFailed() =>
        _ = Assert.IsType<BridgeFault.ConnectFailed>(Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(
            new SessionState.Connecting(SessionGens.Host), new SessionSignal.DeadlineHit(SessionPhase.Connect, Policy.ConnectDeadline), Policy)).Fault);

    [Fact]
    public void RunningHostExitRecordsTheCrashAgainstTheInFlightScenario() {
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(SessionGens.Running, Exit, Policy));
        BridgeFault.RhinoCrash crash = Assert.IsType<BridgeFault.RhinoCrash>(faulted.Fault);
        Assert.Equal("blocks.next", crash.Scenario);
        Assert.Same(SessionPhase.Execute, faulted.At);
        Assert.Equal(SessionGens.Running.Done, faulted.Done);
    }

    [Fact]
    public void RunningDeadlineReadsExecuteDeadline() =>
        _ = Assert.IsType<BridgeFault.ExecuteDeadline>(Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(SessionGens.Running, Overrun, Policy)).Fault);

    [Fact]
    public void RunningHostExitCarriesTheCrashReportWhenOneLanded() {
        CrashFact report = new("/tmp/Rhinoceros-2026-08-26.ips", "com.apple.render", "EXC_BAD_ACCESS", string.Empty);
        SessionSignal exit = new SessionSignal.HostExited(4242, 1_765_432_200_000, Some(report));
        BridgeFault.RhinoCrash crash = Assert.IsType<BridgeFault.RhinoCrash>(Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(SessionGens.Running, exit, Policy)).Fault);
        Assert.Equal(report.IpsPath, crash.Crash.IpsPath);
        Assert.Equal(report.CrashThread, crash.Crash.CrashThread);
        Assert.Equal(report.ExceptionType, crash.Crash.ExceptionType);
        Assert.Contains("blocks.next", crash.Crash.Detail, StringComparison.Ordinal);
    }
}

public sealed class FoldLaws {
    [Fact]
    public void CleanRunFoldsOkWithAnEmptyFirstFailure() {
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Running with { Remaining = Seq<ScenarioEntry>() },
            Seq<BridgeEvent>(SessionGens.Phase(1, SessionPhase.Connect, PhaseStatus.Ok), SessionGens.Phase(2, SessionPhase.Hello, PhaseStatus.Ok)));
        Assert.Same(PhaseStatus.Ok, envelope.Status.Overall);
        Assert.Equal(string.Empty, envelope.FirstFailure);
        Assert.Null(envelope.FaultPhase);
        Assert.Equal(0, envelope.ExitCode);
    }

    [Fact]
    public void SkippedOutcomesNeverDisplaceTheStatusAccumulator() {
        SessionEnvelope envelope = SessionGens.Fold(SessionGens.Running with {
            Done = Seq(SessionGens.Outcome("a", PhaseStatus.Skipped), SessionGens.Outcome("b", PhaseStatus.Skipped)),
            Remaining = Seq<ScenarioEntry>(),
        });
        Assert.Same(PhaseStatus.Ok, envelope.Status.Overall);
        Assert.All(envelope.Scenarios, static outcome => Assert.Same(PhaseStatus.Skipped, outcome.Status));
    }

    [Fact]
    public void RemainingScenariosFoldSkippedIntoTheOutcomes() {
        SessionEnvelope envelope = SessionGens.Fold(SessionGens.Running);
        Assert.Equal(2, envelope.Scenarios.Length);
        Assert.Equal("blocks.next", envelope.Scenarios[1].Scenario);
        Assert.Same(PhaseStatus.Skipped, envelope.Scenarios[1].Status);
    }

    [Fact]
    public void SessionFaultOutranksPhaseEvidence() {
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Faulted,
            Seq<BridgeEvent>(SessionGens.Phase(1, SessionPhase.Launch, PhaseStatus.Failed, new BridgeFault.LaunchFailed("phase-level"))));
        Assert.Equal(SessionGens.Faulted.Fault.Prescription, envelope.FirstFailure);
        Assert.Same(SessionPhase.Connect, envelope.FaultPhase);
        Assert.Same(PhaseStatus.Busy, envelope.Status.Overall);
        Assert.Equal(5, envelope.ExitCode);
    }

    [Fact]
    public void FirstFailingPhaseWinsInWireOrder() {
        BridgeFault loadFault = new BridgeFault.CargoRecycleRequired("a", "b");
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Running with { Done = Seq<ScenarioOutcome>(), Remaining = Seq<ScenarioEntry>() },
            Seq<BridgeEvent>(
                SessionGens.Phase(3, SessionPhase.Execute, PhaseStatus.Failed, new BridgeFault.ExecuteDeadline("late", 1.0)),
                SessionGens.Phase(1, SessionPhase.Connect, PhaseStatus.Ok),
                SessionGens.Phase(2, SessionPhase.Load, PhaseStatus.Failed, loadFault)));
        Assert.Same(SessionPhase.Load, envelope.FaultPhase);
        Assert.Equal(loadFault.Prescription, envelope.FirstFailure);
    }

    [Fact]
    public void ScenarioFailureOutranksSessionPhaseFailure() {
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Running with { Done = Seq(SessionGens.Outcome("blocks.a", PhaseStatus.Failed)), Remaining = Seq<ScenarioEntry>() },
            Seq<BridgeEvent>(SessionGens.Phase(1, SessionPhase.Unload, PhaseStatus.Failed)));
        Assert.Equal("blocks.a failed", envelope.FirstFailure);
        Assert.Same(SessionPhase.Execute, envelope.FaultPhase);
    }

    [Fact]
    public void FirstFailureTruncatesAtTheWireCap() =>
        Assert.Equal(256, SessionGens.Fold(new SessionState.Faulted(new BridgeFault.LaunchFailed(new string('x', 512)), SessionPhase.Launch, Seq<ScenarioOutcome>())).FirstFailure.Length);

    [Fact]
    public void StatusFoldIsOrderIndependentForEveryStatusPair() =>
        Assert.All(
            PhaseStatus.Items.SelectMany(static _ => PhaseStatus.Items, static (left, right) => (left, right)),
            static pair => Assert.Same(Fold(pair.left, pair.right), Fold(pair.right, pair.left)));

    private static PhaseStatus Fold(PhaseStatus first, PhaseStatus second) =>
        SessionGens.Fold(
            SessionGens.Running with { Done = Seq<ScenarioOutcome>(), Remaining = Seq<ScenarioEntry>() },
            Seq<BridgeEvent>(SessionGens.Phase(1, SessionPhase.Load, first), SessionGens.Phase(2, SessionPhase.Execute, second))).Status.Overall;

    [Fact]
    public void EnvelopeEvidenceCarriesOnlyFactAndCaptureCases() {
        ArtifactRef artifact = new("captures/s/a.png", EvidenceRole.Capture, "image/png", 1L, "00", "s", OnFailure: false);
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Faulted,
            Seq<BridgeEvent>(
                SessionGens.Fact(1, "cargo.swapMs"),
                new BridgeEvent.CaptureCase(artifact, 1280, 720, "a", "top") { Stamp = SessionGens.Stamp(2) },
                SessionGens.Phase(3, SessionPhase.Execute, PhaseStatus.Ok)),
            (0L, 3L));
        Assert.Equal(2, envelope.Evidence.Length);
        Assert.All(envelope.Evidence, static evt => Assert.True(evt.IsEvidence));
        Assert.Equal(1, envelope.Counts.Captures);
    }

    [Fact]
    public void CountsClassifyFactKeysThroughTheRoleVocabulary() {
        SessionEnvelope envelope = SessionGens.Fold(
            SessionGens.Faulted,
            Seq<BridgeEvent>(SessionGens.Fact(1, "case.volume.status"), SessionGens.Fact(2, "manifest.object.blocks"), SessionGens.Fact(3, "manifest.gh2.canvas"), SessionGens.Fact(4, "cargo.loaded")));
        Assert.Equal(new EvidenceCounts(4, 1, 2, 0, 0), envelope.Counts);
    }

    [Fact]
    public void SpoolDivergenceEmitsTheReconciliationFactAndDegrades() {
        Seq<BridgeEvent> stream = Seq<BridgeEvent>(SessionGens.Fact(1, "a", "probe"), SessionGens.Fact(2, "b", "probe"));
        SessionEnvelope diverged = SessionGens.Fold(SessionGens.Running with { Done = Seq<ScenarioOutcome>(), Remaining = Seq<ScenarioEntry>() }, stream, (4L, 2L));
        BridgeEvent.FactCase reconciliation = Assert.IsType<BridgeEvent.FactCase>(diverged.Evidence[^1]);
        Assert.Equal("evidence.divergence", reconciliation.Key);
        Assert.Equal(4L, reconciliation.Value.GetProperty("spool").GetInt64());
        Assert.Equal(2L, reconciliation.Value.GetProperty("relayed").GetInt64());
        Assert.Equal(3L, reconciliation.Stamp.Sequence);
        Assert.Same(PhaseStatus.Degraded, diverged.Status.Session);
        SessionEnvelope reconciled = SessionGens.Fold(SessionGens.Faulted, stream, (2L, 2L));
        Assert.DoesNotContain(reconciled.Evidence, static evt => evt is BridgeEvent.FactCase { Key: "evidence.divergence" });
    }

    [Fact]
    public void RelayOnlyLifecycleFactsDoNotTriggerSpoolDivergence() {
        Seq<BridgeEvent> stream = Seq<BridgeEvent>(SessionGens.Fact(1, "cargo.loaded"), SessionGens.Fact(2, "blocks.fact", "blocks.baseline"));
        Assert.DoesNotContain(SessionGens.Fold(SessionGens.Faulted, stream, (1L, 2L)).Evidence, static evt => evt is BridgeEvent.FactCase { Key: "evidence.divergence" });
    }

    [Fact]
    public void ExitCodeTaxonomyPassesThrough() =>
        Assert.All(
            ((BridgeFault Fault, PhaseStatus Status, int Exit)[])[
                (new BridgeFault.CapabilityAbsent("gh2.render", "absent"), PhaseStatus.Unsupported, 3),
                (new BridgeFault.BusyHeld(777, 1.0), PhaseStatus.Busy, 5),
                (new BridgeFault.ExecuteDeadline("a", 1.0), PhaseStatus.Timeout, 5),
                (new BridgeFault.LaunchFailed("gone"), PhaseStatus.Failed, 1),
            ],
            static row => {
                SessionEnvelope envelope = SessionGens.Fold(new SessionState.Faulted(row.Fault, SessionPhase.Launch, Seq<ScenarioOutcome>()));
                Assert.Same(row.Status, envelope.Status.Overall);
                Assert.Equal(row.Exit, envelope.ExitCode);
            });

    [Fact]
    public void HostAndCapabilitiesProjectFromTheFinalState() {
        SessionEnvelope ready = SessionGens.Fold(SessionGens.Ready);
        Assert.Equal(SessionGens.Fingerprint, ready.Host);
        Assert.Empty(ready.Capabilities);
        SessionEnvelope running = SessionGens.Fold(SessionGens.Running);
        Assert.Equal(SessionGens.Cargo.Capabilities, running.Capabilities);
        SessionEnvelope faulted = SessionGens.Fold(SessionGens.Faulted);
        Assert.Equal(default, faulted.Host);
        Assert.Empty(faulted.Capabilities);
    }

    [Fact]
    public void CertificateIsTheEnvelopeAndIndexesOnlyEvidenceDirectories() {
        using TestDirectory report = TestDirectory.Create("rbx-cert-");
        File.WriteAllText(report.CreateDirectory("captures/s").FullName + "/a.png", "png");
        File.WriteAllText(report.CreateDirectory("events").FullName + "/s.jsonl", "{}");
        File.WriteAllText(report.CreateDirectory("stage").FullName + "/x.dll", "dll");
        SessionEnvelope envelope = SessionFold.Run(SessionGens.Sid.ToString("n"), new SupervisorVerb.Status(), SessionGens.Ready, Seq<BridgeEvent>(), (0L, 0L), report.Root.FullName);
        SessionEnvelope written = JsonSerializer.Deserialize(File.ReadAllText(ReportLayout.Certificate(report.Root.FullName)), BridgeJsonContext.Default.SessionEnvelope)!;
        Assert.Equal(envelope.RunId, written.RunId);
        Assert.Equal(["captures/s/a.png", "events/s.jsonl"], envelope.Artifacts.Select(static artifact => artifact.Path), StringComparer.Ordinal);
        Assert.Equal([EvidenceRole.Capture, EvidenceRole.Spool], envelope.Artifacts.Select(static artifact => artifact.Role));
        Assert.Equal("s", envelope.Artifacts[0].Scenario);
    }
}

public sealed class VerbLaws {
    [Fact]
    public void ParseAdmitsEveryVerbShape() {
        SupervisorVerb.Verify verify = Assert.IsType<SupervisorVerb.Verify>(Succ(["verify", """{"$type":"themes","themes":["blocks"]}"""]));
        Assert.Equal(["blocks"], Assert.IsType<ScenarioSelection.ThemesCase>(verify.Selection).Themes);
        _ = Assert.IsType<SupervisorVerb.Status>(Succ(["status"]));
        _ = Assert.IsType<SupervisorVerb.Quit>(Succ(["quit"]));
    }

    [Fact]
    public void ParseRejectsUnknownShapes() =>
        Assert.All(
            (string[][])[["launch"], ["verify"], ["verify", "not-json"], ["verify", """{"$type":"all"}""", "extra"], []],
            static argv => Assert.True(Verbs.Parse(argv).IsFail));

    [Fact]
    public void HelpNamesEveryVerbAndExitCode() {
        using JsonDocument help = JsonDocument.Parse(Verbs.Help());
        Assert.Equal(["verify", "status", "quit"], help.RootElement.GetProperty("verbs").EnumerateArray().Select(static verb => verb.GetProperty("verb").GetString() ?? string.Empty), StringComparer.Ordinal);
        JsonElement exitCodes = help.RootElement.GetProperty("exitCodes");
        Assert.All(PhaseStatus.Items, status => Assert.Equal(status.ExitCode, exitCodes.GetProperty(status.Key).GetInt32()));
        Assert.Equal(Verbs.UsageExitCode, exitCodes.GetProperty("usage").GetInt32());
    }

    [Fact]
    public void VerbProjectionsRouteKeysAndEntryPhases() {
        Assert.Equal("verify", Succ(["verify", """{"$type":"all"}"""]).Key);
        Assert.Same(SessionPhase.Launch, Succ(["verify", """{"$type":"all"}"""]).EntryPhase);
        Assert.Same(SessionPhase.Status, new SupervisorVerb.Status().EntryPhase);
        Assert.Same(SessionPhase.QuitAe, new SupervisorVerb.Quit().EntryPhase);
    }

    private static SupervisorVerb Succ(string[] argv) => Spec.SuccValue(Verbs.Parse(argv), "verb");
}

public sealed class HostControlLaws {
    private static readonly SessionPolicy Fast = SessionPolicy.Default with { QuitRungDeadline = TimeSpan.FromSeconds(2) };

    [Fact]
    public void BogusBundleLaunchFailsTyped() {
        BundleInfo bogus = new(Path.Combine(Path.GetTempPath(), "rbx-gate-bogus.app"), "Bogus", "Bogus", "0.0");
        Fin<Unit> launched = bogus.Launch(TimeSpan.FromSeconds(10));
        Assert.True(launched.IsFail);
        Assert.Same(PhaseStatus.Failed, new BridgeFault.LaunchFailed(launched.Match(static _ => string.Empty, static error => error.Message)).Status);
    }

    [Fact]
    public void Kill9MidConnectIsObservedByKqueueAndDispatchedAsLaunchFailed() {
        int child = SessionGens.Spawn();
        SessionSignal? seen = null;
        using ManualResetEventSlim raised = new(initialState: false);
        using HostWatch watch = HostWatch.Attach(child, () => { seen = new SessionSignal.HostExited(child, 0L, Option<CrashFact>.None); raised.Set(); }, Fast.WatchPoll);
        Assert.True(Posix.Kill(child));
        Assert.True(raised.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Equal("kqueue", watch.Mode);
        SessionState.Faulted faulted = Assert.IsType<SessionState.Faulted>(SessionDispatch.Apply(new SessionState.Connecting(SessionGens.StandIn(child)), seen!, Fast));
        Assert.Contains("during connect", Assert.IsType<BridgeFault.LaunchFailed>(faulted.Fault).Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void DeadPidAndRecycledPidEndpointsAreRejected() {
        int dead = SessionGens.Spawn();
        _ = Posix.Kill(dead);
        SessionGens.AwaitDeath(dead);
        Fin<LiveHost> deadAdmit = LiveHost.Admit(new EndpointRecord.Live("rbx-gate-dead", dead, 1L, "gate"), default);
        Spec.Fail(deadAdmit, error => Assert.Contains("not alive", error.Message, StringComparison.Ordinal));
        Fin<LiveHost> drifted = LiveHost.Admit(new EndpointRecord.Live("rbx-gate-drift", Environment.ProcessId, 1L, "gate"), default);
        Spec.Fail(drifted, error => Assert.Contains("recycled", error.Message, StringComparison.Ordinal));
        Fin<LiveHost> self = LiveHost.Admit(new EndpointRecord.Live("rbx-gate-self", Environment.ProcessId, Posix.StartedAtUnixMs(Environment.ProcessId).IfNone(0L), "gate"), default);
        Spec.Succ(self, host => Assert.Equal(Environment.ProcessId, host.Pid));
    }

    [Fact]
    public void DeadLeaseIsReclaimedWithTheReclaimFact() {
        using TestDirectory scratch = TestDirectory.Create("rbx-lease-");
        string path = scratch.File("dead.lease").FullName;
        File.WriteAllText(path, """{"holderPid":4999999,"holderStartedAtUnixMs":1,"acquiredAtUnixMs":1}""");
        List<BridgeEvent> published = [];
        Fin<Lease.Token> token = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, published.Add);
        Spec.Succ(token, held => _ = Lease.Release(held));
        Assert.Contains(published, static evt => evt is BridgeEvent.FactCase { Key: "lease.reclaimed" });
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void SecondSupervisorIsBusyWithExitFive() {
        using TestDirectory scratch = TestDirectory.Create("rbx-lease-");
        string path = scratch.File("busy.lease").FullName;
        Fin<Lease.Token> first = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, static _ => { });
        Fin<Lease.Token> second = Lease.Acquire(path, Guid.NewGuid(), TimeProvider.System, static _ => { });
        Spec.Fail(second, error => {
            Assert.Equal(PhaseStatus.Busy.ExitCode, error.Code);
            Assert.Contains(Environment.ProcessId.ToString(CultureInfo.InvariantCulture), error.Message, StringComparison.Ordinal);
        });
        Spec.Succ(first, winner => _ = Lease.Release(winner));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void QuitLadderEscalatesAeForceKillAndJournalsEachRung() {
        using TestDirectory scratch = TestDirectory.Create("rbx-quit-");
        SupervisorRuntime runtime = SessionGens.Runtime(scratch, Fast);
        int child = SessionGens.Spawn();
        LiveHost host = SessionGens.StandIn(child);
        List<BridgeEvent> published = [];
        PhaseStatus outcome = QuitLadder.Run(runtime, host, Guid.NewGuid(), published.Add);
        Seq<QuitJournal.Entry> entries = QuitJournal.Read(runtime.JournalPath);
        Assert.Same(PhaseStatus.Ok, outcome);
        Assert.False(Posix.Alive(child));
        Assert.Equal(["quit.ae:failed", "quit.force:failed", "quit.kill:ok"],
            published.OfType<BridgeEvent.PhaseCase>().Select(static phase => $"{phase.Phase.Key}:{phase.Status.Key}"), StringComparer.Ordinal);
        Assert.Equal(["quit.ae", "quit.force", "quit.kill"], entries.Map(static entry => entry.Rung), StringComparer.Ordinal);
        Assert.All(entries, entry => Assert.Equal(child, entry.Pid));
    }

    [Fact]
    public async Task QuitPrepareReassertsAfterStallTypesResidueAndAcceptsFirstCleanPassAsync() {
        TimeSpan bound = Fast.QuitRungDeadline;
        CancellationToken ct = TestContext.Current.CancellationToken;
        static QuitScrub Clean(int marked) => new(marked, marked, 0, new Gh2Scrub.NotLoaded(), []);
        int attempts = 0;
        async Task<QuitScrub> StallThenCleanAsync(CancellationToken token) {
            attempts++;
            if (attempts == 1) {
                await Task.Delay(bound + TimeSpan.FromSeconds(2), TimeProvider.System, token).ConfigureAwait(false);
            }
            return Clean(1);
        }
        List<BridgeEvent> retried = [];
        await QuitPrepare.RunAsync(StallThenCleanAsync, bound, TimeProvider.System, Guid.NewGuid(), retried.Add, ct).ConfigureAwait(true);
        Assert.Equal(2, attempts);
        Assert.Contains(retried, static evt => evt is BridgeEvent.FactCase { Key: "quit.prepared" });
        List<BridgeEvent> dirty = [];
        await QuitPrepare.RunAsync(_ => Task.FromResult(new QuitScrub(1, 1, 1, new Gh2Scrub.NotLoaded(), ["/tmp/x.3dm"])), bound, TimeProvider.System, Guid.NewGuid(), dirty.Add, ct).ConfigureAwait(true);
        _ = Assert.Single(dirty, static evt => evt is BridgeEvent.FactCase { Key: "quit.prepare.incomplete" });
        Assert.DoesNotContain(dirty, static evt => evt is BridgeEvent.FactCase { Key: "quit.prepared" });
        List<BridgeEvent> gh2Dirty = [];
        await QuitPrepare.RunAsync(_ => Task.FromResult(new QuitScrub(1, 1, 0, new Gh2Scrub.Scrubbed(1, 1, 1), [])), bound, TimeProvider.System, Guid.NewGuid(), gh2Dirty.Add, ct).ConfigureAwait(true);
        _ = Assert.Single(gh2Dirty, static evt => evt is BridgeEvent.FactCase { Key: "quit.prepare.incomplete" });
        List<BridgeEvent> clean = [];
        await QuitPrepare.RunAsync(_ => Task.FromResult(Clean(2)), bound, TimeProvider.System, Guid.NewGuid(), clean.Add, ct).ConfigureAwait(true);
        BridgeEvent.FactCase prepared = Assert.IsType<BridgeEvent.FactCase>(Assert.Single(clean));
        Assert.Equal("quit.prepared", prepared.Key);
        Assert.Equal(2, prepared.Value.GetProperty("documents").GetInt32());
    }

    [Fact]
    public void ReconcileClearsOnlyJournalWindowedMarkers() {
        using TestDirectory scratch = TestDirectory.Create("rbx-reconcile-");
        SupervisorRuntime runtime = SessionGens.Runtime(scratch, Fast);
        string windowed = Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker + ".rhl");
        string foreign = Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker);
        File.WriteAllText(windowed, "gate");
        File.WriteAllText(foreign, "gate");
        long mark = new DateTimeOffset(File.GetLastWriteTimeUtc(windowed)).ToUnixTimeMilliseconds();
        _ = QuitJournal.Append(runtime.JournalPath, new QuitJournal.Entry(1, mark - 60_000, mark + 60_000, "quit.kill", "rbx-gate"));
        File.SetLastWriteTimeUtc(foreign, DateTime.UtcNow.AddDays(-2));
        Seq<BridgeEvent> facts = Reconcile.Sweep(runtime, Guid.NewGuid());
        Assert.Contains(facts, evt => evt is BridgeEvent.FactCase { Key: "reconcile.cleared" } fact && string.Equals(fact.Value.GetProperty("path").GetString(), windowed, StringComparison.Ordinal));
        Assert.Contains(facts, evt => evt is BridgeEvent.FactCase { Key: "reconcile.skipped.foreign" } fact && string.Equals(fact.Value.GetProperty("path").GetString(), foreign, StringComparison.Ordinal));
        Assert.False(File.Exists(windowed));
        Assert.True(File.Exists(foreign));
    }

    [Fact]
    public void RecoveryClearRemovesTheAutosaveLockAndCrashReportsBeforeLaunch() {
        using TestDirectory scratch = TestDirectory.Create("rbx-recovery-");
        SupervisorRuntime runtime = SessionGens.Runtime(scratch, Fast);
        string lockFile = Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker + ".rhl");
        string report = Path.Combine(runtime.DiagnosticReportsDirectory, "Rhinoceros-2026-08-26.ips");
        File.WriteAllText(lockFile, "lock");
        File.WriteAllText(report, "crash");
        Seq<BridgeEvent> facts = Reconcile.ClearRecovery(runtime, Guid.NewGuid());
        Assert.Equal(2, facts.Count(static evt => evt is BridgeEvent.FactCase { Key: "recovery.cleared" }));
        Assert.False(File.Exists(lockFile));
        Assert.False(File.Exists(report));
    }

    [Fact]
    public void StagingIsContentKeyedAndLaysOutCargoRootPlusScenarios() {
        using TestDirectory scratch = TestDirectory.Create("rbx-stage-");
        string cargo = scratch.CreateDirectory("payload/cargo").FullName;
        string scenarios = scratch.CreateDirectory("payload/scenarios").FullName;
        File.WriteAllText(Path.Combine(cargo, CargoManifest.AssemblyFile), "alpha");
        File.WriteAllText(Path.Combine(cargo, "Rasm.Bridge.Cargo.deps.json"), "cargo");
        File.WriteAllText(Path.Combine(scenarios, "Rasm.Scenarios.dll"), "beta");
        string payload = Path.Combine(scratch.Root.FullName, "payload");
        string stage = scratch.File(ReportLayout.StageDirectory).FullName;
        Guid session = Guid.NewGuid();
        CargoManifest staged = Spec.SuccValue(Evidence.Stage(payload, session, scratch.Root.FullName, stage), "stage");
        CargoManifest restaged = Spec.SuccValue(Evidence.Stage(payload, session, scratch.Root.FullName, stage), "restage");
        Assert.Equal(staged.ContentHash, restaged.ContentHash);
        Assert.True(File.Exists(Path.Combine(staged.StagePath, CargoManifest.AssemblyFile)));
        Assert.True(File.Exists(Path.Combine(staged.StagePath, CargoManifest.ScenariosDirectory, "Rasm.Scenarios.dll")));
        File.WriteAllText(Path.Combine(scenarios, "Rasm.Scenarios.dll"), "gamma");
        CargoManifest changed = Spec.SuccValue(Evidence.Stage(payload, session, scratch.Root.FullName, stage), "changed");
        Assert.NotEqual(staged.ContentHash, changed.ContentHash, StringComparer.Ordinal);
        File.Delete(Path.Combine(scenarios, "Rasm.Scenarios.dll"));
        Spec.Fail(Evidence.Stage(payload, session, scratch.Root.FullName, stage), error => Assert.Contains("scenario assembly", error.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void SpoolHarvestKeepsEveryWholeLineAndDropsTheTruncatedTail() {
        using TestDirectory scratch = TestDirectory.Create("rbx-spool-");
        BridgeEvent fact = new BridgeEvent.FactCase("gate.fact", JsonDocument.Parse("1").RootElement.Clone()) { Stamp = new EventStamp(SessionGens.Sid, 7, 1, "gate") };
        string line = JsonSerializer.Serialize(fact, BridgeJsonContext.Default.BridgeEvent);
        _ = scratch.CreateDirectory(ReportLayout.EventsDirectory);
        File.WriteAllText(ReportLayout.Spool(scratch.Root.FullName, "gate"), line + "\n" + line + "\n" + line[..(line.Length / 2)]);
        Assert.Equal(2, Evidence.HarvestSpool(scratch.Root.FullName, "gate").Count);
        Assert.Equal((2L, 7L), Evidence.SpoolTail(scratch.Root.FullName));
    }

    [Fact]
    public void IpsParserReadsTheFaultingThreadAndExceptionType() {
        using TestDirectory scratch = TestDirectory.Create("rbx-ips-");
        string body = """{"faultingThread":1,"threads":[{"name":"main"},{"queue":"com.apple.render"}],"exception":{"type":"EXC_BAD_ACCESS"}}""";
        FileInfo report = scratch.File("Rhinoceros-2026-08-26-000000.ips");
        File.WriteAllText(report.FullName, "{\"app_name\":\"Rhinoceros\"}\n" + body);
        CrashFact parsed = Spec.SuccValue(Evidence.IpsDiff(Seq<string>(), scratch.Root.FullName, SessionGens.Bundle).ToFin(Error.New("no report")), "ips");
        Assert.Equal("com.apple.render", parsed.CrashThread);
        Assert.Equal("EXC_BAD_ACCESS", parsed.ExceptionType);
        Assert.Equal(report.FullName, parsed.IpsPath);
        Spec.None(Evidence.IpsDiff(Seq(report.FullName), scratch.Root.FullName, SessionGens.Bundle));
    }
}
