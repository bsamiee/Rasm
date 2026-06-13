using System.Globalization;
using System.Text.Json;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

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
    internal sealed record Ready(LiveHost Host, Handshake Peer) : SessionState;
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

// Ownership: every duration, cadence, budget, and retention value in the tool. One deadline row per
// state; shell and Python derive these values instead of restating them.
internal sealed record SessionPolicy(
    Schedule Connect, Schedule QuitLadder,
    TimeSpan ReconcileDeadline, TimeSpan LaunchDeadline, TimeSpan ConnectDeadline, TimeSpan HelloDeadline,
    TimeSpan LoadDeadline, TimeSpan QuitRungDeadline, TimeSpan FaultDeadline, TimeSpan SessionDeadline,
    TimeSpan ScenarioDefaultBudget, TimeSpan HeartbeatWindow, TimeSpan WatchPoll, TimeSpan JournalSlack,
    TimeSpan ToolDeadline, TimeSpan ForensicsDeadline,
    int RestartBudget, TimeSpan FailureRetention, bool PruneGreenRuns) {

    // Schedule intersection makes the bounded side authoritative; union would keep the unbounded
    // cadence alive.
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
        // WatchPoll is both kqueue wake granularity and degraded PID-poll cadence.
        WatchPoll: TimeSpan.FromMilliseconds(value: 250),
        // macOS writes .ips asynchronously after kill; slack retains late crash markers for the
        // supervised window.
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
            ready: static (_, _) => Option<TimeSpan>.None,
            loading: static (policy, _) => Some(value: policy.LoadDeadline),
            running: static (policy, _) => Some(value: policy.ScenarioDefaultBudget),
            quitting: static (policy, _) => Some(value: policy.QuitRungDeadline),
            faulted: static (policy, _) => Some(value: policy.FaultDeadline),
            terminal: static (_, _) => Option<TimeSpan>.None);
    }
}

// Ownership: live host sessions. This is the executable bridge lifecycle that admits one verb,
// owns one lease, opens one RPC pipe, folds every event into one SessionEnvelope, and emits exactly
// one terminal value for Assay.
internal static class SessionKernel {
    internal static SessionEnvelope Run(SupervisorVerb verb, SupervisorRuntime runtime) =>
        new SessionRun(verb: verb, runtime: runtime).Run();

    private sealed class SessionRun {
        private const string EndpointFileName = "rhino-bridge-rbx.json";

        private readonly SupervisorVerb verb;
        private readonly SupervisorRuntime runtime;
        private readonly Guid sessionId = Guid.NewGuid();
        private readonly List<BridgeEvent> stream = [];
        private readonly string runId;
        private readonly string reportDir;
        private long sequence;

        internal SessionRun(SupervisorVerb verb, SupervisorRuntime runtime) {
            ArgumentNullException.ThrowIfNull(argument: verb);
            ArgumentNullException.ThrowIfNull(argument: runtime);
            this.verb = verb;
            this.runtime = runtime;
            runId = sessionId.ToString(format: "n");
            reportDir = Path.Combine(path1: runtime.ArtifactRoot, path2: runId);
        }

        [Obsolete]
        internal SessionEnvelope Run() {
            _ = Directory.CreateDirectory(path: reportDir);
            Fin<LeaseToken> claimed = Lease.Acquire(path: runtime.LeasePath, sessionId: sessionId, clock: runtime.Clock, publish: Publish);
            if (claimed is not Fin<LeaseToken>.Succ(LeaseToken lease)) {
                BridgeFault fault = new BridgeFault.LaunchFailed(
                    Detail: claimed is Fin<LeaseToken>.Fail(Error error) ? error.Message : "lease acquisition failed");
                Phase(phase: verb.EntryPhase, status: fault.Status, fault: fault);
                return Fold(final: Faulted(fault: fault, at: verb.EntryPhase), spoolTail: (0L, 0L));
            }
            try {
                return verb switch {
                    SupervisorVerb.Doctor => Doctor(),
                    SupervisorVerb.Verify verify => Verify(verify: verify),
                    SupervisorVerb.Quit => Quit(),
                    SupervisorVerb.Redeploy => Fold(
                        final: Faulted(new BridgeFault.RedeployIncomplete(FailingCheck: "redeploy.supervisor"), SessionPhase.Install),
                        spoolTail: (0L, 0L)),
                    _ => throw new InvalidOperationException(message: "unknown supervisor verb"),
                };
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                BridgeFault fault = new BridgeFault.LaunchFailed(Detail: $"{error.GetType().Name}: {error.Message}");
                Phase(phase: verb.EntryPhase, status: fault.Status, fault: fault);
                return Fold(final: Faulted(fault: fault, at: verb.EntryPhase), spoolTail: (0L, 0L));
            } finally {
                _ = Lease.Release(token: lease);
            }
        }

        private SessionEnvelope Doctor() =>
            WithHost(connectPhase: SessionPhase.Doctor, body: (connection, live) => {
                Handshake peer = connection.Hello();
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                stream.Add(item: Fact("doctor.endpoint", peer.Endpoint?.PipeName ?? live.Endpoint.PipeName));
                return new SessionProjection(
                    Final: new SessionState.Ready(Host: negotiated, Peer: peer),
                    SpoolTail: (0L, 0L));
            });

        [Obsolete]
        private SessionEnvelope Verify(SupervisorVerb.Verify verify) =>
            WithHost(connectPhase: SessionPhase.Connect, body: (connection, live) => {
                Handshake peer = connection.Hello();
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                if (peer.ContractVersion < Handshake.CurrentVersion) {
                    BridgeFault fault = new BridgeFault.ShellSkew(ShellContract: peer.ContractVersion, SupervisorContract: Handshake.CurrentVersion);
                    Phase(SessionPhase.Hello, fault.Status, fault: fault);
                    return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.Hello), SpoolTail: (0L, 0L));
                }
                Phase(SessionPhase.Hello, PhaseStatus.Ok);
                Fin<CargoManifest> staged = Evidence.Stage(
                    closureManifest: verify.ClosureManifest, sessionId: sessionId, reportDir: reportDir,
                    refsRoot: Path.Combine(path1: reportDir, path2: "refs"));
                if (staged is not Fin<CargoManifest>.Succ(CargoManifest manifest)) {
                    BridgeFault fault = new BridgeFault.NugetLockDrift(
                        Detail: staged is Fin<CargoManifest>.Fail(Error error) ? error.Message : "closure staging unresolved");
                    Phase(SessionPhase.Stage, fault.Status, fault: fault);
                    return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.Stage), SpoolTail: (0L, 0L));
                }
                Phase(SessionPhase.Stage, PhaseStatus.Ok);
                CargoReceipt cargo = connection.Load(manifest: manifest);
                Phase(SessionPhase.Load, PhaseStatus.Ok, durationMs: cargo.SwapMs);
                ScenarioReceipt[] receipts = connection.Run(selection: verify.Selection);
                UnloadReceipt unload = connection.Unload();
                stream.Add(item: Fact(
                    unload.Confirmed ? "cargo.unload.confirmed" : "cargo.unload.leaked",
                    $"gcRetries={unload.GcRetries};elapsedMs={unload.ElapsedMs:F0};debugger={unload.DebuggerAttached}"));
                Phase(SessionPhase.Unload, PhaseStatus.Ok, durationMs: unload.ElapsedMs);
                connection.PrepareQuit();
                connection.Dispose();
                _ = QuitLadder.Run(host: negotiated, sessionId: sessionId, publish: stream.Add).Run(runtime);
                return new SessionProjection(
                    Final: new SessionState.Running(
                        Host: negotiated, Cargo: cargo, Done: toSeq(receipts), Remaining: Seq<ScenarioEntry>(),
                        RestartBudget: runtime.Policy.RestartBudget),
                    SpoolTail: SpoolTail(receipts: receipts));
            });

        [Obsolete]
        private SessionEnvelope Quit() {
            Fin<LiveHost> admitted = ReadLiveEndpoint();
            if (admitted is not Fin<LiveHost>.Succ(LiveHost host)) {
                stream.Add(item: Fact("quit.no-host", admitted is Fin<LiveHost>.Fail(Error error) ? error.Message : "no live endpoint"));
                return Fold(final: new SessionState.Idle(Bundle: runtime.Bundle), spoolTail: (0L, 0L));
            }
            return WithConnection(host: host, connectPhase: SessionPhase.QuitAe, body: (connection, live) => {
                Handshake peer = connection.Hello();
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                connection.PrepareQuit();
                connection.Dispose();
                Fin<PhaseStatus> outcome = QuitLadder.Run(host: negotiated, sessionId: sessionId, publish: stream.Add).Run(runtime);
                if (outcome is Fin<PhaseStatus>.Succ(PhaseStatus status) && status == PhaseStatus.Ok) {
                    return new SessionProjection(
                        Final: new SessionState.Quitting(
                            Host: negotiated, Rung: SessionPhase.QuitAe,
                            RungStartedMs: runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds()),
                        SpoolTail: (0L, 0L));
                }
                BridgeFault fault = new BridgeFault.UiWedged(
                    SilentForMs: runtime.Policy.QuitRungDeadline.TotalMilliseconds, Scenario: SessionPhase.QuitKill.Key);
                Phase(SessionPhase.QuitKill, fault.Status, durationMs: runtime.Policy.QuitRungDeadline.TotalMilliseconds, fault: fault);
                return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.QuitKill), SpoolTail: (0L, 0L));
            });
        }

        private void ReconcileHost() =>
            stream.AddRange(collection: Reconcile.Run(bundle: runtime.Bundle, sessionId: sessionId).Run(runtime).IfFail(Seq<BridgeEvent>()).AsEnumerable());

        private Fin<LiveHost> EnsureHost() =>
            ReadLiveEndpoint() is Fin<LiveHost>.Succ(LiveHost live)
                ? Fin.Succ(value: live)
                : LaunchAndPoll();

        [Obsolete]
        private SessionEnvelope WithHost(SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionProjection> body) {
            ReconcileHost();
            return EnsureHost() switch {
                Fin<LiveHost>.Succ(LiveHost host) => WithConnection(host: host, connectPhase: connectPhase, body: body),
                Fin<LiveHost>.Fail(Error error) => Fault(phase: SessionPhase.Connect, detail: error.Message),
                _ => Fault(phase: SessionPhase.Connect, detail: "host admission unresolved"),
            };
        }

        private Fin<LiveHost> LaunchAndPoll() {
            long started = Environment.TickCount64;
            Fin<Unit> launched = runtime.Bundle.Launch(toolDeadline: runtime.Policy.ToolDeadline);
            if (launched is Fin<Unit>.Fail(Error launchError)) {
                BridgeFault fault = new BridgeFault.LaunchFailed(Detail: launchError.Message);
                Phase(SessionPhase.Launch, fault.Status, durationMs: Elapsed(started: started), fault: fault);
                return Fin.Fail<LiveHost>(error: Error.New(message: fault.Prescription));
            }
            Phase(SessionPhase.Launch, PhaseStatus.Ok, durationMs: Elapsed(started: started));
            long until = Environment.TickCount64 + (long)runtime.Policy.ConnectDeadline.TotalMilliseconds;
            while (Environment.TickCount64 < until && !runtime.Root.IsCancellationRequested) {
                if (ReadLiveEndpoint() is Fin<LiveHost>.Succ(LiveHost live)) {
                    Phase(SessionPhase.Connect, PhaseStatus.Ok, durationMs: Elapsed(started: started));
                    return Fin.Succ(value: live);
                }
                Thread.Sleep(timeout: runtime.Policy.WatchPoll);
            }
            BridgeFault fault = new BridgeFault.ConnectFailed(
                Detail: "endpoint did not appear before connect deadline", ElapsedMs: Elapsed(started: started));
            Phase(SessionPhase.Connect, fault.Status, durationMs: fault.ElapsedMs, fault: fault);
            return Fin.Fail<LiveHost>(error: Error.New(message: fault.Prescription));
        }

        [Obsolete]
        private SessionEnvelope WithConnection(LiveHost host, SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionProjection> body) {
            SupervisorConnection? connection = null;
            SessionProjection projection;
            try {
                connection = SupervisorConnection.Connect(pipeName: host.Endpoint.PipeName, timeout: runtime.Policy.ConnectDeadline);
                projection = body(connection, host);
            } catch (Exception error) when (error is RemoteMethodNotFoundException or RemoteInvocationException or ConnectionLostException
                or IOException or TimeoutException or ObjectDisposedException) {
                BridgeFault fault = FaultOf(error: error);
                Phase(connectPhase, fault.Status, fault: fault);
                projection = new SessionProjection(Final: Faulted(fault: fault, at: connectPhase), SpoolTail: (0L, 0L));
            } finally {
                if (connection is not null) {
                    stream.AddRange(collection: connection.Events);
                    connection.Dispose();
                }
            }
            return Fold(final: projection.Final, spoolTail: projection.SpoolTail);
        }

        private SessionEnvelope Fault(SessionPhase phase, string detail) {
            BridgeFault fault = new BridgeFault.ConnectFailed(Detail: detail, ElapsedMs: 0.0);
            return Fold(final: Faulted(fault: fault, at: phase), spoolTail: (0L, 0L));
        }

        private SessionEnvelope Fold(SessionState final, (long Count, long LastSequence) spoolTail) =>
            SessionFold.Run(runId: runId, verb: verb, final: final, stream: toSeq(stream), spoolTail: spoolTail, reportDir: reportDir);

        private void Publish(BridgeEvent evt) => stream.Add(item: evt);

        private EventStamp Next(string? scenario = null) => new(
            SessionId: sessionId,
            Sequence: Interlocked.Increment(location: ref sequence),
            AtUnixMs: runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds(),
            Scenario: scenario);

        private void Phase(SessionPhase phase, PhaseStatus status, double durationMs = 0.0, BridgeFault? fault = null) =>
            stream.Add(item: new BridgeEvent.PhaseCase(Phase: phase, Status: status, DurationMs: durationMs, Fault: fault) { Stamp = Next() });

        private BridgeEvent.FactCase Fact(string key, string value) =>
            new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) {
                Stamp = Next(),
            };

        private static SessionState.Faulted Faulted(BridgeFault fault, SessionPhase at) =>
            new(Fault: fault, At: at, Done: Seq<ScenarioReceipt>());

        private (long Count, long LastSequence) SpoolTail(ScenarioReceipt[] receipts) {
            Seq<BridgeEvent> harvested = toSeq(receipts.Select(selector: static receipt => receipt.Scenario).Prepend("probe"))
                .Bind(f: scenario => Evidence.HarvestSpool(reportDir: reportDir, scenario: scenario));
            return (harvested.Count, harvested.Map(f: static evt => evt.Stamp.Sequence)
                .Fold(initialState: 0L, f: static (max, observed) => Math.Max(val1: max, val2: observed)));
        }

        private static Fin<LiveHost> ReadLiveEndpoint() {
            try {
                string path = Path.Combine(
                    path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile),
                    path2: ".rasm", path3: EndpointFileName);
                if (!File.Exists(path: path)) {
                    return Fin.Fail<LiveHost>(error: Error.New(message: $"endpoint absent at '{path}'"));
                }
                using JsonDocument raw = JsonDocument.Parse(json: File.ReadAllText(path: path));
                if (raw.RootElement.TryGetProperty(propertyName: "fault", value: out JsonElement poisoned)) {
                    return Fin.Fail<LiveHost>(error: Error.New(message: $"poisoned endpoint: {poisoned.GetString()}"));
                }
                EndpointRecord? endpoint = raw.RootElement.Deserialize(jsonTypeInfo: BridgeJsonContext.Default.EndpointRecord);
                return endpoint is null
                    ? Fin.Fail<LiveHost>(error: Error.New(message: $"endpoint decoded to null: '{path}'"))
                    : LiveHost.Admit(endpoint: endpoint, fingerprint: new HostFingerprint(
                        BundleVersion: endpoint.RhinoVersion,
                        RhinoCommonVersion: string.Empty,
                        Grasshopper2Version: string.Empty,
                        RuntimeVersion: string.Empty));
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
                return Fin.Fail<LiveHost>(error: Error.New(message: $"endpoint read failed: {error.Message}"));
            }
        }

        private static BridgeFault FaultOf(Exception error) =>
            error switch {
                RemoteInvocationException remote when RemoteFault(remote: remote) is { } fault => fault,
                RemoteMethodNotFoundException missing => new BridgeFault.CapabilityAbsent(Capability: "rpc.method", ProbeReceipt: missing.Message),
                TimeoutException timeout => new BridgeFault.ConnectFailed(Detail: timeout.Message, ElapsedMs: 0.0),
                IOException io => new BridgeFault.ConnectFailed(Detail: io.Message, ElapsedMs: 0.0),
                ObjectDisposedException disposed => new BridgeFault.ConnectFailed(Detail: disposed.Message, ElapsedMs: 0.0),
                _ => new BridgeFault.LaunchFailed(Detail: error.Message),
            };

        private static BridgeFault? RemoteFault(RemoteInvocationException remote) {
            if (remote.DeserializedErrorData is BridgeFault direct) {
                return direct;
            }
            if (remote.DeserializedErrorData is JsonElement element) {
                try {
                    return JsonSerializer.Deserialize(json: element.GetRawText(), jsonTypeInfo: BridgeJsonContext.Default.BridgeFault);
                } catch (JsonException) {
                    return null;
                }
            }
            return null;
        }

        private static double Elapsed(long started) => Environment.TickCount64 - started;
    }

    private sealed record SessionProjection(SessionState Final, (long Count, long LastSequence) SpoolTail);
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
            connecting: static (ctx, current) => HostPhase(
                host: current.Host, current: current, signal: ctx.Signal, policy: ctx.Policy,
                phase: SessionPhase.Connect, deadline: ctx.Policy.ConnectDeadline),
            negotiating: static (ctx, current) => HostPhase(
                host: current.Host, current: current, signal: ctx.Signal, policy: ctx.Policy,
                phase: SessionPhase.Hello, deadline: ctx.Policy.HelloDeadline),
            ready: static (_, current) => current,
            loading: static (ctx, current) => CargoPhase(
                host: current.Host, current: current, scenario: CargoLoad, done: Seq<ScenarioReceipt>(),
                signal: ctx.Signal, policy: ctx.Policy, phase: SessionPhase.Load, deadline: ctx.Policy.LoadDeadline),
            running: static (ctx, current) => CargoPhase(
                host: current.Host, current: current, scenario: InFlight(running: current), done: current.Done,
                signal: ctx.Signal, policy: ctx.Policy, phase: SessionPhase.Execute, deadline: ctx.Policy.ScenarioDefaultBudget),
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

    private static SessionState HostPhase(LiveHost host, SessionState current, SessionSignal signal,
        SessionPolicy policy, SessionPhase phase, TimeSpan deadline) =>
        signal switch {
            SessionSignal.HostExited exited =>
                Fault(fault: new BridgeFault.LaunchFailed(Detail: Exited(pid: exited.Pid, during: phase)), at: phase),
            SessionSignal.HeartbeatSilent silent when silent.SilentFor >= policy.HeartbeatWindow =>
                Fault(fault: new BridgeFault.DialogSuspected(SilentForMs: silent.SilentFor.TotalMilliseconds), at: phase),
            SessionSignal.DeadlineHit hit when hit.Elapsed >= deadline =>
                Fault(fault: new BridgeFault.ConnectFailed(Detail: Deadline(phase: phase, elapsed: hit.Elapsed), ElapsedMs: hit.Elapsed.TotalMilliseconds), at: phase),
            SessionSignal.ShutdownStarted shutdown =>
                Quit(host: host, atUnixMs: shutdown.AtUnixMs),
            _ => current,
        };

    private static SessionState CargoPhase(LiveHost host, SessionState current, string scenario,
        Seq<ScenarioReceipt> done, SessionSignal signal, SessionPolicy policy, SessionPhase phase, TimeSpan deadline) =>
        signal switch {
            SessionSignal.HostExited exited =>
                Fault(fault: new BridgeFault.RhinoCrash(Crash: Crash(pid: exited.Pid, scenario: scenario), Scenario: scenario), at: phase, done: done),
            SessionSignal.HeartbeatSilent silent when silent.SilentFor >= policy.HeartbeatWindow =>
                Fault(fault: new BridgeFault.UiWedged(SilentForMs: silent.SilentFor.TotalMilliseconds, Scenario: scenario), at: phase, done: done),
            SessionSignal.DeadlineHit hit when hit.Elapsed >= deadline =>
                Fault(fault: new BridgeFault.ExecuteDeadline(Scenario: scenario, ElapsedMs: hit.Elapsed.TotalMilliseconds), at: phase, done: done),
            SessionSignal.ShutdownStarted shutdown =>
                Quit(host: host, atUnixMs: shutdown.AtUnixMs),
            _ => current,
        };

    private static SessionState.Quitting Quit(LiveHost host, long atUnixMs) =>
        new(Host: host, Rung: SessionPhase.QuitAe, RungStartedMs: atUnixMs);

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
            SessionState.Ready ready => ready.Peer.Capabilities,
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
            SessionState.Ready ready => ready.Host.Fingerprint,
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
