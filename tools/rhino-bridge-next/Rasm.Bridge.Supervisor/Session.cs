using System.Globalization;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: the session has ONE owner. Cases carry their evidence; no boolean phase flags, no
// nullable payload bags. Transitions are one total state-threaded Switch (SessionDispatch); the
// envelope is the fold of whatever evidence exists when the session leaves the machine.
[Union]
internal abstract partial record SessionState {
    private SessionState() { }
    internal sealed record Idle(BundleInfo Bundle) : SessionState;
    internal sealed record Reconciling(BundleInfo Bundle, Seq<string> MarkersCleared) : SessionState;
    internal sealed record Launching(BundleInfo Bundle, long LaunchedAtMs) : SessionState;
    internal sealed record Connecting(LiveHost Host, int PollsRemaining) : SessionState;
    internal sealed record Negotiating(LiveHost Host, Handshake Ours) : SessionState;
    internal sealed record Loading(LiveHost Host, Handshake Peer, CargoManifest Manifest) : SessionState;
    internal sealed record Running(LiveHost Host, CargoReceipt Cargo, Seq<ScenarioReceipt> Done,
                                 Seq<ScenarioEntry> Remaining, int RestartBudget) : SessionState;
    internal sealed record Quitting(LiveHost Host, SessionPhase Rung, long RungStartedMs) : SessionState;
    internal sealed record Faulted(BridgeFault Fault, SessionPhase At, Seq<ScenarioReceipt> Done) : SessionState;
    internal sealed record Terminal(SessionEnvelope Envelope) : SessionState;
}

// Ownership: the closed signal vocabulary the watchers produce (supervisor-private; never wire).
[Union]
internal abstract partial record SessionSignal {
    private SessionSignal() { }
    internal sealed record HostExited(int Pid, long AtUnixMs) : SessionSignal;
    internal sealed record HeartbeatSilent(TimeSpan SilentFor) : SessionSignal;
    internal sealed record ShutdownStarted(long AtUnixMs) : SessionSignal;
    internal sealed record RpcCompleted(SessionPhase Phase, PhaseStatus Status) : SessionSignal;
    internal sealed record DeadlineHit(SessionPhase Phase, TimeSpan Elapsed) : SessionSignal;
}

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: EVERY duration, cadence, budget, and retention number in the tool. One authoritative
// deadline row per state; shell and python derive, never re-declare. Probe-0d sizing: warm doctor
// round-trip (connect cost folded in) min 726 / median ~775 / max 792 ms, cold launch (open ->
// endpoint) ~4.6 s; the verdict-path warm cost was unmeasurable on the probe build (execute lane
// blocked), so the Load and Execute rows stay generous, operator-tunable rows rather than
// measurement-tight ones.
internal sealed record SessionPolicy(
    Schedule Connect, Schedule QuitLadder,
    TimeSpan ReconcileDeadline, TimeSpan LaunchDeadline, TimeSpan ConnectDeadline, TimeSpan HelloDeadline,
    TimeSpan LoadDeadline, TimeSpan QuitRungDeadline, TimeSpan FaultDeadline, TimeSpan SessionDeadline,
    TimeSpan ScenarioDefaultBudget, TimeSpan HeartbeatWindow, TimeSpan WatchPoll, TimeSpan JournalSlack,
    TimeSpan ToolDeadline, TimeSpan ForensicsDeadline,
    int RestartBudget, TimeSpan FailureRetention, bool PruneGreenRuns) {

    // Connect composes by INTERSECTION: spaced alone recurs forever and a schedule union runs
    // while EITHER side continues, so only `&` makes upto(90s) the terminating bound.
    public static readonly SessionPolicy Default = new(
        Connect: Schedule.spaced(space: TimeSpan.FromMilliseconds(value: 250)) & Schedule.upto(max: TimeSpan.FromSeconds(value: 90)),
        QuitLadder: Schedule.spaced(space: TimeSpan.FromSeconds(value: 15)) | Schedule.recurs(times: 2),
        ReconcileDeadline: TimeSpan.FromSeconds(value: 10),
        LaunchDeadline: TimeSpan.FromSeconds(value: 30),
        ConnectDeadline: TimeSpan.FromSeconds(value: 90),
        HelloDeadline: TimeSpan.FromSeconds(value: 10),
        LoadDeadline: TimeSpan.FromSeconds(value: 60),
        QuitRungDeadline: TimeSpan.FromSeconds(value: 15),
        FaultDeadline: TimeSpan.FromSeconds(value: 5),
        SessionDeadline: TimeSpan.FromMinutes(value: 10),
        ScenarioDefaultBudget: TimeSpan.FromSeconds(value: 30),
        HeartbeatWindow: TimeSpan.FromSeconds(value: 10),
        // WatchPoll is both the kqueue wake granularity and the degraded PID-poll cadence (D-7:
        // 250 ms polling is the swap-in fallback, a policy value, never a rewrite).
        WatchPoll: TimeSpan.FromMilliseconds(value: 250),
        // macOS writes .ips ASYNC after a kill: a marker stamped shortly after a journaled
        // retirement still belongs to the supervised instance's [launch, kill] window.
        JournalSlack: TimeSpan.FromSeconds(value: 120),
        ToolDeadline: TimeSpan.FromSeconds(value: 15),
        ForensicsDeadline: TimeSpan.FromSeconds(value: 120),
        RestartBudget: 1,
        FailureRetention: TimeSpan.FromDays(value: 7),
        PruneGreenRuns: true);

    public Option<TimeSpan> DeadlineFor(SessionState state) {
        ArgumentNullException.ThrowIfNull(argument: state);
        return state.Switch(
            state: this,
            idle: static (policy, _) => Some(value: policy.SessionDeadline),
            reconciling: static (policy, _) => Some(value: policy.ReconcileDeadline),
            launching: static (policy, _) => Some(value: policy.LaunchDeadline),
            connecting: static (policy, _) => Some(value: policy.ConnectDeadline),
            negotiating: static (policy, _) => Some(value: policy.HelloDeadline),
            loading: static (policy, _) => Some(value: policy.LoadDeadline),
            running: static (policy, _) => Some(value: policy.ScenarioDefaultBudget),
            quitting: static (policy, _) => Some(value: policy.QuitRungDeadline),
            faulted: static (policy, _) => Some(value: policy.FaultDeadline),
            terminal: static (_, _) => Option<TimeSpan>.None);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: the one total transition algebra over state x signal. Stuck-state law: every
// non-terminal state exits on DeadlineHit (Quitting escalates rung-by-rung, everything else
// faults), so no external event can wedge the machine and Faulted always reaches Terminal through
// the fold. RpcCompleted outcomes carry typed payloads richer than the signal, so the session
// pipeline folds them directly; the signal row exists for quit-rung escalation (closed:false
// FAILS its rung). Host relaunch under the restart budget is runtime choreography over the prior
// Running state — the pure machine records the crash.
internal static class SessionDispatch {
    internal static SessionState Apply(SessionState state, SessionSignal signal, SessionPolicy policy) {
        ArgumentNullException.ThrowIfNull(argument: state);
        ArgumentNullException.ThrowIfNull(argument: signal);
        ArgumentNullException.ThrowIfNull(argument: policy);
        return state.Switch(
            state: (Signal: signal, Policy: policy),
            idle: static (ctx, current) => ctx.Signal switch {
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.SessionDeadline =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Deadline(phase: SessionPhase.Reconcile, elapsed: hit.Elapsed)), at: SessionPhase.Reconcile),
                _ => current,
            },
            reconciling: static (ctx, current) => ctx.Signal switch {
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.ReconcileDeadline =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Deadline(phase: SessionPhase.Reconcile, elapsed: hit.Elapsed)), at: SessionPhase.Reconcile),
                _ => current,
            },
            launching: static (ctx, current) => ctx.Signal switch {
                SessionSignal.HostExited exited =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Exited(pid: exited.Pid, during: SessionPhase.Launch)), at: SessionPhase.Launch),
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Fault(fault: new BridgeFault.DialogSuspected(SilentForMs: silent.SilentFor.TotalMilliseconds), at: SessionPhase.Launch),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.LaunchDeadline =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Deadline(phase: SessionPhase.Launch, elapsed: hit.Elapsed)), at: SessionPhase.Launch),
                _ => current,
            },
            connecting: static (ctx, current) => ctx.Signal switch {
                SessionSignal.HostExited exited =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Exited(pid: exited.Pid, during: SessionPhase.Connect)), at: SessionPhase.Connect),
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Fault(fault: new BridgeFault.DialogSuspected(SilentForMs: silent.SilentFor.TotalMilliseconds), at: SessionPhase.Connect),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.ConnectDeadline =>
                    Fault(fault: new BridgeFault.ConnectFailed(Detail: Deadline(phase: SessionPhase.Connect, elapsed: hit.Elapsed), ElapsedMs: hit.Elapsed.TotalMilliseconds), at: SessionPhase.Connect),
                SessionSignal.ShutdownStarted shutdown =>
                    new SessionState.Quitting(Host: current.Host, Rung: SessionPhase.QuitAe, RungStartedMs: shutdown.AtUnixMs),
                _ => current,
            },
            negotiating: static (ctx, current) => ctx.Signal switch {
                SessionSignal.HostExited exited =>
                    Fault(fault: new BridgeFault.LaunchFailed(Detail: Exited(pid: exited.Pid, during: SessionPhase.Hello)), at: SessionPhase.Hello),
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Fault(fault: new BridgeFault.DialogSuspected(SilentForMs: silent.SilentFor.TotalMilliseconds), at: SessionPhase.Hello),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.HelloDeadline =>
                    Fault(fault: new BridgeFault.ConnectFailed(Detail: Deadline(phase: SessionPhase.Hello, elapsed: hit.Elapsed), ElapsedMs: hit.Elapsed.TotalMilliseconds), at: SessionPhase.Hello),
                SessionSignal.ShutdownStarted shutdown =>
                    new SessionState.Quitting(Host: current.Host, Rung: SessionPhase.QuitAe, RungStartedMs: shutdown.AtUnixMs),
                _ => current,
            },
            loading: static (ctx, current) => ctx.Signal switch {
                SessionSignal.HostExited exited =>
                    Fault(fault: new BridgeFault.RhinoCrash(Crash: Crash(pid: exited.Pid, scenario: CargoLoad), Scenario: CargoLoad), at: SessionPhase.Load),
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Fault(fault: new BridgeFault.UiWedged(SilentForMs: silent.SilentFor.TotalMilliseconds, Scenario: CargoLoad), at: SessionPhase.Load),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.LoadDeadline =>
                    Fault(fault: new BridgeFault.ExecuteDeadline(Scenario: CargoLoad, ElapsedMs: hit.Elapsed.TotalMilliseconds), at: SessionPhase.Load),
                SessionSignal.ShutdownStarted shutdown =>
                    new SessionState.Quitting(Host: current.Host, Rung: SessionPhase.QuitAe, RungStartedMs: shutdown.AtUnixMs),
                _ => current,
            },
            running: static (ctx, current) => ctx.Signal switch {
                SessionSignal.HostExited exited =>
                    Fault(fault: new BridgeFault.RhinoCrash(Crash: Crash(pid: exited.Pid, scenario: InFlight(running: current)), Scenario: InFlight(running: current)), at: SessionPhase.Execute, done: current.Done),
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Fault(fault: new BridgeFault.UiWedged(SilentForMs: silent.SilentFor.TotalMilliseconds, Scenario: InFlight(running: current)), at: SessionPhase.Execute, done: current.Done),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.ScenarioDefaultBudget =>
                    Fault(fault: new BridgeFault.ExecuteDeadline(Scenario: InFlight(running: current), ElapsedMs: hit.Elapsed.TotalMilliseconds), at: SessionPhase.Execute, done: current.Done),
                SessionSignal.ShutdownStarted shutdown =>
                    new SessionState.Quitting(Host: current.Host, Rung: SessionPhase.QuitAe, RungStartedMs: shutdown.AtUnixMs),
                _ => current,
            },
            quitting: static (ctx, current) => ctx.Signal switch {
                // Rung confirmed CLEAN: the ladder effect observes the exit and completes the fold.
                SessionSignal.HostExited => current,
                SessionSignal.HeartbeatSilent silent when silent.SilentFor >= ctx.Policy.HeartbeatWindow =>
                    Escalate(quitting: current, observedMs: silent.SilentFor.TotalMilliseconds),
                SessionSignal.DeadlineHit hit when hit.Elapsed >= ctx.Policy.QuitRungDeadline =>
                    Escalate(quitting: current, observedMs: hit.Elapsed.TotalMilliseconds),
                SessionSignal.RpcCompleted completed when completed.Status.ExitCode != 0 =>
                    Escalate(quitting: current, observedMs: 0.0),
                _ => current,
            },
            faulted: static (_, current) => current,
            terminal: static (_, current) => current);
    }

    private const string CargoLoad = "cargo.load";

    // The ladder order ae -> force -> kill is law; a deadline past the kill rung means the host
    // survived SIGKILL observation, which reads as a wedge, never a clean rung.
    private static SessionState Escalate(SessionState.Quitting quitting, double observedMs) {
        Option<SessionPhase> next = quitting.Rung == SessionPhase.QuitAe ? Some(value: SessionPhase.QuitForce)
            : quitting.Rung == SessionPhase.QuitForce ? Some(value: SessionPhase.QuitKill)
            : Option<SessionPhase>.None;
        SessionState wedged = Fault(fault: new BridgeFault.UiWedged(SilentForMs: observedMs, Scenario: SessionPhase.QuitKill.Key), at: SessionPhase.QuitKill);
        return next.Case is SessionPhase rung
            ? quitting with { Rung = rung, RungStartedMs = quitting.RungStartedMs + (long)observedMs }
            : wedged;
    }

    private static SessionState.Faulted Fault(BridgeFault fault, SessionPhase at, Seq<ScenarioReceipt> done = default) =>
        new(Fault: fault, At: at, Done: done);

    private static CrashFact Crash(int pid, string scenario) =>
        new(IpsPath: string.Empty, CrashThread: "unknown", ExceptionType: "unknown",
            Detail: string.Create(provider: CultureInfo.InvariantCulture, $"host pid {pid} exited inside '{scenario}'"));

    private static string Deadline(SessionPhase phase, TimeSpan elapsed) =>
        string.Create(provider: CultureInfo.InvariantCulture, $"{phase.Key} deadline after {elapsed.TotalMilliseconds:F0}ms");

    private static string Exited(int pid, SessionPhase during) =>
        string.Create(provider: CultureInfo.InvariantCulture, $"host pid {pid} exited during {during.Key}");

    private static string InFlight(SessionState.Running running) =>
        running.Remaining.Head.Case is ScenarioEntry entry ? entry.Name : "session";
}

// Ownership: the terminal fold — SessionEnvelope fields are fold RESULTS over the event stream +
// receipts. Status = Worst over phase events and receipts seeded Ok (rank ties keep the
// accumulator, so an all-skipped run reads ok at the root while every receipt reads skipped — the
// skip signal is receipt-level, never the root). FirstFailure + FirstFaultPhase = the first-non-ok
// projection in wire order, with the session fault outranking phase evidence. Evidence carries
// fact/capture cases only (phase/progress history lives in receipts + the on-disk spool). Spool
// reconciliation compares the relayed in-host tally against the spool tail and emits an
// `evidence.divergence` fact on mismatch.
internal static class SessionFold {
    private const int FirstFailureCap = 256;

    internal static PhaseStatus Worst(PhaseStatus left, PhaseStatus right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        return left.Worst(other: right);
    }

    internal static SessionEnvelope Run(string runId, SupervisorVerb verb, SessionState final,
        Seq<BridgeEvent> stream, (long Count, long LastSequence) spoolTail, string reportDir) {
        ArgumentNullException.ThrowIfNull(argument: verb);
        ArgumentNullException.ThrowIfNull(argument: final);
        if (final is SessionState.Terminal terminal)
            return terminal.Envelope;
        Seq<BridgeEvent> ordered = toSeq(value: stream.OrderBy(keySelector: static evt => evt.Stamp.AtUnixMs).ThenBy(keySelector: static evt => evt.Stamp.Sequence));
        Seq<ScenarioReceipt> receipts = Receipts(final: final);
        BridgeFault? fault = final is SessionState.Faulted faulted ? faulted.Fault : null;
        Seq<BridgeEvent.PhaseCase> phases = ordered.Choose(selector: static evt => evt is BridgeEvent.PhaseCase phase ? Some(value: phase) : Option<BridgeEvent.PhaseCase>.None);
        PhaseStatus status = (phases.Map(f: static phase => phase.Status) + receipts.Map(f: static receipt => receipt.Status) + (fault is null ? Seq<PhaseStatus>() : Seq(value: fault.Status)))
            .Fold(initialState: PhaseStatus.Ok, f: static (accumulator, observed) => accumulator.Worst(other: observed));
        (string firstFailure, SessionPhase? firstPhase) = FirstNonOk(final: final, fault: fault, phases: phases, receipts: receipts);
        long relayed = ordered.Filter(f: evt => evt.Stamp.Sequence <= spoolTail.LastSequence).Count;
        Seq<BridgeEvent> evidence = ordered.Filter(f: static evt => evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase);
        Seq<BridgeEvent> carried = spoolTail.Count == relayed
            ? evidence
            : evidence + Seq<BridgeEvent>(value: Divergence(runId: runId, ordered: ordered, spoolTail: spoolTail, relayed: relayed));
        double duration = ordered.Head.Case is BridgeEvent head && ordered.Last.Case is BridgeEvent tail
            ? tail.Stamp.AtUnixMs - head.Stamp.AtUnixMs
            : 0.0;
        return new SessionEnvelope(
            RunId: runId, Verb: verb.Key, Status: status, DurationMs: duration, ReportDir: reportDir,
            Host: Host(final: final), Capabilities: Capabilities(final: final), Scenarios: [.. receipts],
            Evidence: [.. carried], FirstFailure: firstFailure, FirstFaultPhase: firstPhase, Fault: fault);
    }

    private static CapabilityEntry[] Capabilities(SessionState final) =>
        final switch {
            SessionState.Running running => running.Cargo.Capabilities,
            SessionState.Loading loading => loading.Peer.Capabilities,
            _ => [],
        };

    private static BridgeEvent.FactCase Divergence(string runId, Seq<BridgeEvent> ordered, (long Count, long LastSequence) spoolTail, long relayed) {
        Guid sessionId = ordered.Head.Case is BridgeEvent head ? head.Stamp.SessionId
            : Guid.TryParse(input: runId, result: out Guid parsed) ? parsed : Guid.Empty;
        (long lastSequence, long atUnixMs) = ordered.Last.Case is BridgeEvent tail
            ? (Math.Max(val1: tail.Stamp.Sequence, val2: spoolTail.LastSequence), tail.Stamp.AtUnixMs)
            : (spoolTail.LastSequence, 0L);
        using JsonDocument value = JsonDocument.Parse(json: string.Create(
            provider: CultureInfo.InvariantCulture,
            $"{{\"spool\":{spoolTail.Count},\"relayed\":{relayed},\"spoolLastSequence\":{spoolTail.LastSequence}}}"));
        return new BridgeEvent.FactCase(Key: "evidence.divergence", Value: value.RootElement.Clone()) {
            Stamp = new EventStamp(SessionId: sessionId, Sequence: lastSequence + 1, AtUnixMs: atUnixMs, Scenario: null),
        };
    }

    private static (string Failure, SessionPhase? Phase) FirstNonOk(SessionState final, BridgeFault? fault,
        Seq<BridgeEvent.PhaseCase> phases, Seq<ScenarioReceipt> receipts) =>
        fault is not null && final is SessionState.Faulted faulted
            ? (Truncate(text: fault.Prescription), faulted.At)
            : phases.Filter(f: static phase => phase.Status.ExitCode != 0).Head.Case is BridgeEvent.PhaseCase firstPhase
                ? (Truncate(text: firstPhase.Fault?.Prescription ?? $"{firstPhase.Phase.Key} {firstPhase.Status.Key}"), firstPhase.Phase)
                : receipts.Filter(f: static receipt => receipt.Status.ExitCode != 0).Head.Case is ScenarioReceipt firstReceipt
                    ? (Truncate(text: firstReceipt.Fault?.Prescription ?? $"{firstReceipt.Scenario} {firstReceipt.Status.Key}"), SessionPhase.Execute)
                    : (string.Empty, null);

    private static HostFingerprint Host(SessionState final) =>
        final switch {
            SessionState.Connecting connecting => connecting.Host.Fingerprint,
            SessionState.Negotiating negotiating => negotiating.Host.Fingerprint,
            SessionState.Loading loading => loading.Host.Fingerprint,
            SessionState.Running running => running.Host.Fingerprint,
            SessionState.Quitting quitting => quitting.Host.Fingerprint,
            _ => new HostFingerprint(BundleVersion: string.Empty, RhinoCommonVersion: string.Empty, Grasshopper2Version: string.Empty, RuntimeVersion: string.Empty),
        };

    private static Seq<ScenarioReceipt> Receipts(SessionState final) =>
        final switch {
            SessionState.Running running => running.Done + running.Remaining.Map(f: static entry =>
                new ScenarioReceipt(Scenario: entry.Name, Status: PhaseStatus.Skipped, DurationMs: 0.0, Fault: null)),
            SessionState.Faulted faulted => faulted.Done,
            _ => Seq<ScenarioReceipt>(),
        };

    private static string Truncate(string text) =>
        text.Length <= FirstFailureCap ? text : text[..FirstFailureCap];
}
