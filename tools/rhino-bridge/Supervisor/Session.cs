using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: session state is one closed owner; cases carry evidence and dispatch owns transitions.
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

// Ownership: supervisor-private watcher signals, never wire payloads.
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

// Ownership: duration, cadence, budget, and retention policy; other surfaces derive from these rows.
internal sealed record SessionPolicy(
    TimeSpan ReconcileDeadline, TimeSpan LaunchDeadline, TimeSpan ConnectDeadline, TimeSpan HelloDeadline,
    TimeSpan LoadDeadline, TimeSpan QuitRungDeadline, TimeSpan FaultDeadline, TimeSpan SessionDeadline,
    TimeSpan ScenarioDefaultBudget, TimeSpan HeartbeatWindow, TimeSpan WatchPoll, TimeSpan JournalSlack,
    TimeSpan ToolDeadline, TimeSpan ForensicsDeadline,
    int RestartBudget, TimeSpan FailureRetention, bool PruneGreenRuns) {

    public static readonly SessionPolicy Default = new(
        ReconcileDeadline: TimeSpan.FromSeconds(value: 10),
        LaunchDeadline: TimeSpan.FromSeconds(value: 30),
        ConnectDeadline: TimeSpan.FromSeconds(value: 90),
        HelloDeadline: TimeSpan.FromSeconds(value: 10),
        LoadDeadline: TimeSpan.FromSeconds(value: 60),
        QuitRungDeadline: TimeSpan.FromSeconds(value: 15),
        FaultDeadline: TimeSpan.FromSeconds(value: 5),
        // SessionDeadline sits strictly below assay's 600s scenario timeout so the supervisor always
        // emits a typed terminal envelope (DeadlineHit -> Faulted) before assay's SIGTERM/SIGKILL ladder
        // lands; a 600s == 600s tie would race and the operator could get a synthetic stderr envelope.
        SessionDeadline: TimeSpan.FromSeconds(value: 540),
        ScenarioDefaultBudget: TimeSpan.FromSeconds(value: 30),
        HeartbeatWindow: TimeSpan.FromSeconds(value: 10),
        // WatchPoll is both kqueue wake cadence and degraded PID-poll cadence.
        WatchPoll: TimeSpan.FromMilliseconds(value: 250),
        // Slack retains macOS crash markers written after the supervised kill window.
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

// Ownership: executable bridge lifecycle from verb admission through lease, RPC, event fold, and
// terminal envelope.
internal static class SessionKernel {
    internal static Task<SessionEnvelope> RunAsync(SupervisorVerb verb, SupervisorRuntime runtime) =>
        new SessionRun(verb: verb, runtime: runtime).RunAsync();

    private sealed class SessionRun {
        // The cursor's Negotiating arm only needs supervisor-side handshake evidence; the wire-side
        // capabilities ride SupervisorConnection.HelloAsync, so this is the algebra's placeholder peer.
        private static readonly Handshake SupervisorHandshake = new(
            ContractVersion: Handshake.CurrentVersion, SenderVersion: "supervisor",
            Capabilities: [], Fingerprint: null, Endpoint: null);

        private readonly SupervisorVerb verb;
        private readonly SupervisorRuntime runtime;
        private readonly Guid sessionId = Guid.NewGuid();
        private readonly List<BridgeEvent> stream = [];
        private readonly string runId;
        private readonly string reportDir;
        private ReferenceRoot[] referenceRoots = [];
        private string evidenceMode = "verify";
        private long sequence;

        internal SessionRun(SupervisorVerb verb, SupervisorRuntime runtime) {
            ArgumentNullException.ThrowIfNull(argument: verb);
            ArgumentNullException.ThrowIfNull(argument: runtime);
            this.verb = verb;
            this.runtime = runtime;
            runId = sessionId.ToString(format: "n");
            reportDir = Path.Combine(path1: runtime.ArtifactRoot, path2: runId);
        }

        internal async Task<SessionEnvelope> RunAsync() {
            _ = Directory.CreateDirectory(path: reportDir);
            Fin<LeaseToken> claimed = Lease.Acquire(path: runtime.LeasePath, sessionId: sessionId, clock: runtime.Clock, publish: Publish);
            if (claimed is not Fin<LeaseToken>.Succ(LeaseToken lease)) {
                BridgeFault fault = new BridgeFault.LaunchFailed(
                    Detail: claimed is Fin<LeaseToken>.Fail(Error error) ? error.Message : "lease acquisition failed");
                Phase(phase: verb.EntryPhase, status: fault.Status, fault: fault);
                return Fold(final: Faulted(fault: fault, at: verb.EntryPhase), spoolTail: (0L, 0L));
            }
            // Commit the claim into the runtime cell so the signal-edge shutdown owner releases it
            // idempotently inside the SIGTERM callback when the finally cannot be reached in time.
            _ = runtime.Lease.Swap(f: _ => Some(value: lease));
            try {
                return verb switch {
                    SupervisorVerb.Status => await StatusAsync().ConfigureAwait(false),
                    SupervisorVerb.Verify verify => await VerifyAsync(verify: verify).ConfigureAwait(false),
                    SupervisorVerb.Quit => await QuitAsync().ConfigureAwait(false),
                    SupervisorVerb.Redeploy redeploy => Fold(
                        final: Faulted(new BridgeFault.CapabilityAbsent(Capability: "redeploy.supervisor", ProbeReceipt: $"the direct supervisor does not own redeploy; Assay owns the package cycle for '{redeploy.PackagePath}'"), SessionPhase.Install),
                        spoolTail: Evidence.SpoolTail(reportDir: reportDir)),
                    _ => throw new InvalidOperationException(message: "unknown supervisor verb"),
                };
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                BridgeFault fault = new BridgeFault.LaunchFailed(Detail: $"{error.GetType().Name}: {error.Message}");
                Phase(phase: verb.EntryPhase, status: fault.Status, fault: fault);
                return Fold(final: Faulted(fault: fault, at: verb.EntryPhase), spoolTail: (0L, 0L));
            } finally {
                // Clearing the cell before release makes the signal-edge shutdown owner's SwapMaybe a
                // no-op, so the lease is released exactly once whether the finally or the callback wins.
                _ = runtime.Lease.Swap(f: _ => Option<LeaseToken>.None);
                _ = Lease.Release(token: lease);
            }
        }

        private Task<SessionEnvelope> StatusAsync() =>
            WithHostAsync(connectPhase: SessionPhase.Status, body: async (connection, live, machine) => {
                Handshake peer = await machine.RunPhaseAsync(phase: SessionPhase.Hello, phaseState: new SessionState.Negotiating(Host: live, Ours: SupervisorHandshake),
                    rpc: ct => connection.HelloAsync(ct: ct)).ConfigureAwait(false);
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                stream.Add(item: Fact("status.endpoint", peer.Endpoint?.PipeName ?? live.Endpoint.PipeName));
                return new SessionProjection(
                    Final: new SessionState.Ready(Host: negotiated, Peer: peer),
                    SpoolTail: (0L, 0L));
            });

        private Task<SessionEnvelope> VerifyAsync(SupervisorVerb.Verify verify) =>
            WithHostAsync(connectPhase: SessionPhase.Connect, body: async (connection, live, machine) => {
                Handshake peer = await machine.RunPhaseAsync(phase: SessionPhase.Hello, phaseState: new SessionState.Negotiating(Host: live, Ours: SupervisorHandshake),
                    rpc: ct => connection.HelloAsync(ct: ct)).ConfigureAwait(false);
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                machine.Track(host: negotiated);
                if (peer.ContractVersion < Handshake.CurrentVersion) {
                    BridgeFault fault = new BridgeFault.ShellSkew(ShellContract: peer.ContractVersion, SupervisorContract: Handshake.CurrentVersion);
                    Phase(SessionPhase.Hello, fault.Status, fault: fault);
                    return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.Hello), SpoolTail: Evidence.SpoolTail(reportDir: reportDir));
                }
                CapabilityEntry shellContent = peer.Capabilities.FirstOrDefault(predicate: static entry =>
                    string.Equals(a: entry.Key, b: Handshake.ShellContentCapability, comparisonType: StringComparison.Ordinal));
                if (shellContent.Key is null || shellContent.Outcome != PhaseStatus.Ok) {
                    BridgeFault fault = new BridgeFault.HostDrift(
                        MissingMember: shellContent.Key is null ? Handshake.ShellContentCapability : $"{Handshake.ShellContentCapability}:{shellContent.Receipt}",
                        BuiltAgainst: live.Fingerprint,
                        Running: negotiated.Fingerprint);
                    Phase(SessionPhase.Hello, fault.Status, fault: fault);
                    return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.Hello), SpoolTail: Evidence.SpoolTail(reportDir: reportDir));
                }
                Phase(SessionPhase.Hello, PhaseStatus.Ok);
                Fin<CargoManifest> staged = Evidence.Stage(
                    closureManifest: verify.ClosureManifest, sessionId: sessionId, reportDir: reportDir,
                    refsRoot: Path.Combine(path1: reportDir, path2: "refs"));
                if (staged is not Fin<CargoManifest>.Succ(CargoManifest manifest)) {
                    BridgeFault fault = new BridgeFault.NugetLockDrift(
                        Detail: staged is Fin<CargoManifest>.Fail(Error error) ? error.Message : "closure staging unresolved");
                    Phase(SessionPhase.Stage, fault.Status, fault: fault);
                    return new SessionProjection(Final: Faulted(fault: fault, at: SessionPhase.Stage), SpoolTail: Evidence.SpoolTail(reportDir: reportDir));
                }
                referenceRoots = manifest.ReferenceRoots;
                evidenceMode = verify.EvidenceMode;
                Phase(SessionPhase.Stage, PhaseStatus.Ok);
                CargoReceipt cargo = await machine.RunPhaseAsync(phase: SessionPhase.Load, phaseState: new SessionState.Loading(Host: negotiated, Peer: peer, Manifest: manifest),
                    rpc: ct => connection.LoadAsync(manifest: manifest, ct: ct)).ConfigureAwait(false);
                Phase(SessionPhase.Load, PhaseStatus.Ok, durationMs: cargo.SwapMs);
                // Execute rides the per-scenario budget supervisor-side: a wedged synchronous UI invoke
                // cannot self-cancel, so the running-phase deadline (entry budget or default) plus the host
                // watch fold a DeadlineHit/UiWedged into Faulted and recover the host through the quit ladder.
                Seq<ScenarioEntry> selected = toSeq(value: cargo.Scenarios);
                ScenarioReceipt[] receipts = await machine.RunPhaseAsync(
                    phase: SessionPhase.Execute,
                    phaseState: new SessionState.Running(Host: negotiated, Cargo: cargo, Done: Seq<ScenarioReceipt>(), Remaining: selected, RestartBudget: runtime.Policy.RestartBudget),
                    deadline: ExecuteBudget(selected: selected),
                    rpc: ct => connection.RunAsync(selection: verify.Selection, ct: ct)).ConfigureAwait(false);
                UnloadReceipt unload = await machine.RunPhaseAsync(phase: SessionPhase.Unload, phaseState: new SessionState.Loading(Host: negotiated, Peer: peer, Manifest: manifest),
                    rpc: ct => connection.UnloadAsync(ct: ct)).ConfigureAwait(false);
                stream.Add(item: Fact(
                    unload.Confirmed ? "cargo.unload.confirmed" : "cargo.unload.leaked",
                    string.Create(
                        provider: CultureInfo.InvariantCulture,
                        $"gcRetries={unload.GcRetries};elapsedMs={unload.ElapsedMs:F0};debugger={unload.DebuggerAttached}")));
                // After-leak recycle: the WeakReference-unconfirmed ALC is recovered by the quit ladder
                // (supervised host recycle), never a forced in-host unload. The gcdump is best-effort
                // forensics — GcDump projects a collect timeout or non-zero exit to None, so the recycle
                // fact stays honest about whether the dump landed rather than implying a dump always exists.
                if (!unload.Confirmed && !unload.DebuggerAttached) {
                    Option<string> gcdump = Evidence.GcDump(pid: negotiated.Pid, reportDir: reportDir, deadline: runtime.Policy.ForensicsDeadline);
                    stream.Add(item: gcdump.Case is string captured
                        ? Fact("cargo.gcdump", captured)
                        : Fact("cargo.gcdump.unavailable", string.Create(provider: CultureInfo.InvariantCulture,
                            $"dotnet-gcdump collect did not produce an artifact within {runtime.Policy.ForensicsDeadline.TotalMilliseconds:F0}ms")));
                    stream.Add(item: Fact("cargo.recycle.after-leak", gcdump.Case is string dump ? $"quit-ladder;gcdump={dump}" : "quit-ladder;gcdump=unavailable"));
                }
                Phase(SessionPhase.Unload, PhaseStatus.Ok, durationMs: unload.ElapsedMs);
                await machine.QuiesceAsync(host: negotiated, sessionId: sessionId, prepare: ct => connection.PrepareQuitAsync(ct: ct), publish: stream.Add).ConfigureAwait(false);
                _ = QuitLadder.Run(host: negotiated, sessionId: sessionId, publish: stream.Add).Run(runtime);
                return new SessionProjection(
                    Final: new SessionState.Running(
                        Host: negotiated, Cargo: cargo, Done: toSeq(receipts), Remaining: Seq<ScenarioEntry>(),
                        RestartBudget: runtime.Policy.RestartBudget),
                    SpoolTail: SpoolTail(receipts: receipts));
            });

        private Task<SessionEnvelope> QuitAsync() {
            Fin<LiveHost> admitted = ReadLiveEndpoint();
            if (admitted is not Fin<LiveHost>.Succ(LiveHost host)) {
                stream.Add(item: Fact("quit.no-host", admitted is Fin<LiveHost>.Fail(Error error) ? error.Message : "no live endpoint"));
                return Task.FromResult(Fold(final: new SessionState.Idle(Bundle: runtime.Bundle), spoolTail: (0L, 0L)));
            }
            return WithConnectionAsync(host: host, connectPhase: SessionPhase.QuitAe, body: async (connection, live, machine) => {
                Handshake peer = await machine.RunPhaseAsync(phase: SessionPhase.Hello, phaseState: new SessionState.Negotiating(Host: live, Ours: SupervisorHandshake),
                    rpc: ct => connection.HelloAsync(ct: ct)).ConfigureAwait(false);
                LiveHost negotiated = live with { Fingerprint = peer.Fingerprint ?? live.Fingerprint, Endpoint = peer.Endpoint ?? live.Endpoint };
                machine.Track(host: negotiated);
                await machine.QuiesceAsync(host: negotiated, sessionId: sessionId, prepare: ct => connection.PrepareQuitAsync(ct: ct), publish: stream.Add).ConfigureAwait(false);
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

        private TimeSpan ExecuteBudget(Seq<ScenarioEntry> selected) {
            // The running-phase deadline is the worst per-scenario budget across the selection (each
            // scenario's BudgetMs when positive, else the policy default), so one wedged scenario in a
            // batch still trips a supervisor-side DeadlineHit instead of waiting out the session budget.
            long maxBudgetMs = selected.Map(f: static entry => entry.BudgetMs).Filter(f: static ms => ms > 0)
                .Fold(initialState: 0L, f: static (max, ms) => Math.Max(val1: max, val2: ms));
            return maxBudgetMs > 0 ? TimeSpan.FromMilliseconds(value: maxBudgetMs) : runtime.Policy.ScenarioDefaultBudget;
        }

        private void ReconcileHost() =>
            stream.AddRange(collection: Reconcile.Run(bundle: runtime.Bundle, sessionId: sessionId).Run(runtime).IfFail(Seq<BridgeEvent>()).AsEnumerable());

        private Fin<LiveHost> EnsureHost() =>
            ReadLiveEndpoint() is Fin<LiveHost>.Succ(LiveHost live)
                ? Fin.Succ(value: live)
                : LaunchAndPoll();

        private Task<SessionEnvelope> WithHostAsync(SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionMachine, Task<SessionProjection>> body) {
            ReconcileHost();
            return EnsureHost() switch {
                Fin<LiveHost>.Succ(LiveHost host) => WithConnectionAsync(host: host, connectPhase: connectPhase, body: body),
                Fin<LiveHost>.Fail(Error error) => Task.FromResult(Fault(phase: SessionPhase.Connect, detail: error.Message)),
                _ => Task.FromResult(Fault(phase: SessionPhase.Connect, detail: "host admission unresolved")),
            };
        }

        private Fin<LiveHost> LaunchAndPoll() {
            long started = Environment.TickCount64;
            // Launch-edge recovery clear precedes only an actual launch; host reuse never wedges on a dialog.
            stream.AddRange(collection: Reconcile.ClearRecovery(bundle: runtime.Bundle, sessionId: sessionId).Run(runtime).IfFail(Seq<BridgeEvent>()).AsEnumerable());
            Fin<Unit> launched = runtime.Bundle.Launch(toolDeadline: runtime.Policy.ToolDeadline);
            if (launched is Fin<Unit>.Fail(Error launchError)) {
                BridgeFault.LaunchFailed launchFault = new(Detail: launchError.Message);
                Phase(SessionPhase.Launch, launchFault.Status, durationMs: Elapsed(started: started), fault: launchFault);
                return Fin.Fail<LiveHost>(error: Error.New(message: launchFault.Prescription));
            }
            Phase(SessionPhase.Launch, PhaseStatus.Ok, durationMs: Elapsed(started: started));
            if (Poll.Until(probe: static () => ReadLiveEndpoint() is Fin<LiveHost>.Succ(LiveHost found) ? Some(value: found) : Option<LiveHost>.None,
                    deadline: runtime.Policy.ConnectDeadline, cadence: runtime.Policy.WatchPoll, ct: runtime.Root)
                is Fin<LiveHost>.Succ(LiveHost live)) {
                Phase(SessionPhase.Connect, PhaseStatus.Ok, durationMs: Elapsed(started: started));
                return Fin.Succ(value: live);
            }
            BridgeFault.ConnectFailed connectFault = new(
                Detail: "endpoint did not appear before connect deadline", ElapsedMs: Elapsed(started: started));
            Phase(SessionPhase.Connect, connectFault.Status, durationMs: connectFault.ElapsedMs, fault: connectFault);
            return Fin.Fail<LiveHost>(error: Error.New(message: connectFault.Prescription));
        }

        private async Task<SessionEnvelope> WithConnectionAsync(LiveHost host, SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionMachine, Task<SessionProjection>> body) {
            SessionProjection projection;
            // The machine seats the connect-phase cursor and attaches the host watch over the whole
            // connection lifetime; every RPC then folds host-exit, heartbeat-silence, and per-phase
            // deadline signals through SessionDispatch.Apply, so a wedged or exited host trips a typed
            // Faulted state supervisor-side instead of blocking on the raw RPC await.
            using SessionMachine machine = SessionMachine.Open(host: host, runtime: runtime);
            // Commit the live host pid so the signal-edge shutdown owner can kill the orphan synchronously.
            _ = runtime.LiveHostPid.Swap(f: _ => Some(value: host.Pid));
            try {
                SupervisorConnection connection = await SupervisorConnection
                    .ConnectAsync(pipeName: host.Endpoint.PipeName, timeout: runtime.Policy.ConnectDeadline, ct: runtime.Root)
                    .ConfigureAwait(false);
                await using (connection.ConfigureAwait(false)) {
                    try {
                        projection = await body(connection, host, machine).ConfigureAwait(false);
                    } finally {
                        stream.AddRange(collection: connection.Events);
                    }
                }
            } catch (SessionMachine.PhaseFaulted faulted) {
                Phase(faulted.At, faulted.Fault.Status, fault: faulted.Fault);
                projection = new SessionProjection(Final: Faulted(fault: faulted.Fault, at: faulted.At), SpoolTail: Evidence.SpoolTail(reportDir: reportDir));
            } catch (Exception error) when (error is RemoteRpcException or JsonException
                or IOException or TimeoutException or ObjectDisposedException) {
                BridgeFault fault = FaultOf(error: error);
                Phase(connectPhase, fault.Status, fault: fault);
                projection = new SessionProjection(Final: Faulted(fault: fault, at: connectPhase), SpoolTail: Evidence.SpoolTail(reportDir: reportDir));
            }
            return Fold(final: projection.Final, spoolTail: projection.SpoolTail);
        }

        private SessionEnvelope Fault(SessionPhase phase, string detail) {
            BridgeFault fault = new BridgeFault.ConnectFailed(Detail: detail, ElapsedMs: 0.0);
            return Fold(final: Faulted(fault: fault, at: phase), spoolTail: (0L, 0L));
        }

        private SessionEnvelope Fold(SessionState final, (long Count, long LastSequence) spoolTail) =>
            SessionFold.Run(
                runId: runId, verb: verb, final: final, stream: toSeq(stream), spoolTail: spoolTail, reportDir: reportDir,
                evidenceMode: evidenceMode, referenceRoots: referenceRoots);

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
                .Bind(f: scenario => Evidence.HarvestSpool(reportDir: reportDir, scenario: scenario))
                .Filter(f: static evt => evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase);
            return (harvested.Count, harvested.Map(f: static evt => evt.Stamp.Sequence)
                .Fold(initialState: 0L, f: static (max, observed) => Math.Max(val1: max, val2: observed)));
        }

        private static Fin<LiveHost> ReadLiveEndpoint() {
            try {
                string path = EndpointRecord.EndpointPath;
                if (!File.Exists(path: path)) {
                    return Fin.Fail<LiveHost>(error: Error.New(message: $"endpoint absent at '{path}'"));
                }
                using JsonDocument raw = JsonDocument.Parse(json: File.ReadAllText(path: path));
                if (raw.RootElement.TryGetProperty(propertyName: "fault", value: out JsonElement poisoned) && poisoned.GetString() is { Length: > 0 } faultMessage) {
                    return Fin.Fail<LiveHost>(error: Error.New(message: $"poisoned endpoint: {faultMessage}"));
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
                JsonException malformed => new BridgeFault.ConnectFailed(Detail: malformed.Message, ElapsedMs: 0.0),
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

    // Ownership: the live composition of the session-state algebra. One cursor cell folds every raised
    // SessionSignal through SessionDispatch.Apply, the host watch is the subscription that survives the
    // whole connection, and each Phase arms a linked per-phase deadline so a wedged or exited host folds
    // to Faulted instead of blocking the RPC await. The pure machine stays the deep owner; this only
    // composes it at the live seam.
    private sealed class SessionMachine : IDisposable {
        private readonly Atom<SessionState> cursor;
        private readonly SessionPolicy policy;
        private readonly TimeProvider clock;
        private readonly CancellationToken root;
        private readonly HostWatch watch;
        private readonly TaskCompletionSource<SessionState.Faulted> faultedGate =
            new(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

        private SessionMachine(Atom<SessionState> cursor, SessionPolicy policy, TimeProvider clock, int pid, CancellationToken root) {
            this.cursor = cursor;
            this.policy = policy;
            this.clock = clock;
            this.root = root;
            cursor.Change += Observe;
            watch = HostWatch.Attach(pid: pid, raise: Raise, poll: policy.WatchPoll, clock: clock);
        }

        internal static SessionMachine Open(LiveHost host, SupervisorRuntime runtime) =>
            new(cursor: Atom(value: (SessionState)new SessionState.Connecting(Host: host, PollsRemaining: 0)),
                policy: runtime.Policy, clock: runtime.Clock, pid: host.Pid, root: runtime.Root);

        internal void Track(LiveHost host) => _ = cursor.Swap(f: _ => new SessionState.Connecting(Host: host, PollsRemaining: 0));

        // One RPC under a per-phase deadline: the cursor seats the phase state, a linked CTS cancels the
        // await at the phase deadline while a DeadlineHit folds the cursor to Faulted, and the host watch's
        // HostExited/HeartbeatSilent fold the same way. The await races the faulted gate; whichever
        // resolves first wins, and a faulted cursor cancels the in-flight RPC and throws the typed fault.
        internal async Task<T> RunPhaseAsync<T>(SessionPhase phase, SessionState phaseState, Func<CancellationToken, Task<T>> rpc, TimeSpan? deadline = null) {
            ArgumentNullException.ThrowIfNull(argument: rpc);
            _ = cursor.Swap(f: _ => phaseState);
            TimeSpan window = deadline ?? policy.DeadlineFor(state: phaseState).IfNone(policy.SessionDeadline);
            using CancellationTokenSource scope = CancellationTokenSource.CreateLinkedTokenSource(root);
            await using ConfiguredAsyncDisposable tripped = scope.Token.Register(callback: () =>
                Raise(new SessionSignal.DeadlineHit(Phase: phase, Elapsed: window))).ConfigureAwait(false);
            if (window != Timeout.InfiniteTimeSpan)
                scope.CancelAfter(delay: window);
            Task<T> work = rpc(scope.Token);
            Task done = await Task.WhenAny(work, faultedGate.Task).ConfigureAwait(false);
            if (ReferenceEquals(objA: done, objB: faultedGate.Task)) {
                await scope.CancelAsync().ConfigureAwait(false);
                throw new PhaseFaulted(faulted: await faultedGate.Task.ConfigureAwait(false));
            }
            return await work.ConfigureAwait(false);
        }

        // PrepareQuit under a bounded scope: PrepareQuitAsync was uncancellable, so a wedged GH2 reflective
        // scrub blocked the supervisor with no escalation, and a stalled-then-swallowed scrub slid a dirty
        // host straight into the `terminate` AE — the AppKit "Save changes?" sheet that wedges the rung and
        // forces the SIGKILL crash. QuitPrepare.RunAsync bounds each scrub attempt at the quit-rung deadline and
        // composes the receipt: a residual-dirty or incomplete scrub is retried once (the re-assert is the
        // AE-rung precondition) and the outcome is published as a typed fact, so a not-clean host reaching
        // `terminate` is evidence rather than a silent slide. The bound is preserved so a wedged GH2 scrub
        // cannot block forever; the caller's quit ladder still recycles the host either way.
        internal Task QuiesceAsync(LiveHost host, Guid sessionId, Func<CancellationToken, Task<QuitPrepareReceipt>> prepare, Action<BridgeEvent> publish) {
            _ = cursor.Swap(f: _ => new SessionState.Quitting(Host: host, Rung: SessionPhase.QuitAe, RungStartedMs: clock.GetUtcNow().ToUnixTimeMilliseconds()));
            return QuitPrepare.RunAsync(prepare: prepare, deadline: policy.QuitRungDeadline, clock: clock, sessionId: sessionId, publish: publish, root: root);
        }

        public void Dispose() {
            cursor.Change -= Observe;
            watch.Dispose();
            _ = faultedGate.TrySetCanceled();
        }

        private void Observe(SessionState state) {
            if (state is SessionState.Faulted faulted)
                _ = faultedGate.TrySetResult(result: faulted);
        }

        private void Raise(SessionSignal signal) => _ = cursor.Swap(f: state => SessionDispatch.Apply(state: state, signal: signal, policy: policy));

        // The faulted-gate sentinel crosses the body's straight-line awaits back to the connection seam,
        // where it converts to the projection — the one boundary throw the live path raises by design,
        // analogous to a cancellation token tripping an await. The carrying ctor pins the typed fault;
        // the conventional ctors exist only to satisfy the exception-shape contract.
        internal sealed class PhaseFaulted : Exception {
            internal PhaseFaulted(SessionState.Faulted faulted) : base(message: faulted.Fault.Prescription) {
                Fault = faulted.Fault;
                At = faulted.At;
            }

            internal PhaseFaulted() : this(faulted: new SessionState.Faulted(Fault: new BridgeFault.LaunchFailed(Detail: "phase faulted"), At: SessionPhase.Connect, Done: Seq<ScenarioReceipt>())) { }
            internal PhaseFaulted(string message) : this(faulted: new SessionState.Faulted(Fault: new BridgeFault.LaunchFailed(Detail: message), At: SessionPhase.Connect, Done: Seq<ScenarioReceipt>())) { }
            internal PhaseFaulted(string message, Exception innerException) : base(message: message, innerException: innerException) {
                Fault = new BridgeFault.LaunchFailed(Detail: message);
                At = SessionPhase.Connect;
            }

            internal BridgeFault Fault { get; }
            internal SessionPhase At { get; }
        }
    }

    private sealed record SessionProjection(SessionState Final, (long Count, long LastSequence) SpoolTail);
}

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: total transition algebra over state and signal. Deadlines force non-terminal exit,
// quit signals escalate rung-by-rung, and runtime relaunch choreography remains outside the pure
// machine.
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
                // The ladder effect observes clean rung exit and completes the fold.
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

    // Rung order is AE, force, kill; surviving the kill observation reads as a wedge.
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

// Ownership: terminal envelope fold over event stream and receipts. Status uses Worst, first
// failure follows wire order, evidence carries facts/captures only, and spool reconciliation only
// compares scenario-scoped in-host evidence.
internal static class SessionFold {
    private const int FirstFailureCap = 256;

    internal static PhaseStatus Worst(PhaseStatus left, PhaseStatus right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        return left.Worst(other: right);
    }

    internal static SessionEnvelope Run(string runId, SupervisorVerb verb, SessionState final,
        Seq<BridgeEvent> stream, (long Count, long LastSequence) spoolTail, string reportDir,
        string evidenceMode = "verify", ReferenceRoot[]? referenceRoots = null) {
        ArgumentNullException.ThrowIfNull(argument: verb);
        ArgumentNullException.ThrowIfNull(argument: final);
        if (final is SessionState.Terminal terminal)
            return terminal.Envelope;
        Seq<BridgeEvent> ordered = toSeq(value: stream.OrderBy(keySelector: static evt => evt.Stamp.AtUnixMs).ThenBy(keySelector: static evt => evt.Stamp.Sequence));
        Seq<ScenarioReceipt> receipts = Receipts(final: final);
        BridgeFault? fault = final is SessionState.Faulted faulted ? faulted.Fault : null;
        Seq<BridgeEvent.PhaseCase> phases = ordered.Choose(selector: static evt => evt is BridgeEvent.PhaseCase phase ? Some(value: phase) : Option<BridgeEvent.PhaseCase>.None);
        Seq<BridgeEvent.PhaseCase> sessionPhases = phases.Filter(f: static phase => phase.Stamp.Scenario is null);
        long relayed = ordered.Filter(f: static evt =>
            (evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase)
            && evt.Stamp.Scenario is { Length: > 0 }).Count;
        SpoolSummary spool = new(DurableEvents: spoolTail.Count, RelayedEvents: relayed, LastSequence: spoolTail.LastSequence, Diverged: spoolTail.Count > relayed, Failures: 0);
        PhaseStatus sessionStatus = (sessionPhases.Map(f: static phase => phase.Status) + (fault is null ? Seq<PhaseStatus>() : Seq(value: fault.Status)))
            .Fold(initialState: PhaseStatus.Ok, f: static (accumulator, observed) => accumulator.Worst(other: observed));
        if (spool.Diverged && sessionStatus == PhaseStatus.Ok) {
            sessionStatus = PhaseStatus.Degraded;
        }
        (string firstSessionFault, SessionPhase? firstPhase) = FirstSessionFault(final: final, fault: fault, phases: sessionPhases);
        Seq<BridgeEvent> evidence = ordered.Filter(f: static evt => evt is BridgeEvent.FactCase or BridgeEvent.CaptureCase);
        // Divergence means durable spool evidence outlived relay delivery.
        Seq<BridgeEvent> carried = spoolTail.Count <= relayed
            ? evidence
            : evidence + Seq<BridgeEvent>(value: Divergence(runId: runId, ordered: ordered, spoolTail: spoolTail, relayed: relayed));
        double duration = ordered.Head.Case is BridgeEvent head && ordered.Last.Case is BridgeEvent tail
            ? tail.Stamp.AtUnixMs - head.Stamp.AtUnixMs
            : 0.0;
        ReferenceEvidenceResult[] references = verb is SupervisorVerb.Verify
            ? Evidence.ReferenceResults(evidenceMode: evidenceMode, roots: referenceRoots ?? [], receipts: receipts, evidence: carried, reportDir: reportDir)
            : [];
        receipts = AttachReferences(receipts: receipts, references: references);
        PhaseStatus scenarioStatus = receipts.Map(f: static receipt => receipt.ScenarioStatus)
            .Fold(initialState: PhaseStatus.Ok, f: static (accumulator, observed) => accumulator.Worst(other: observed));
        PhaseStatus status = scenarioStatus.Worst(other: sessionStatus);
        (string firstScenarioFailure, _) = FirstScenarioFailure(receipts: receipts);
        string firstFailure = firstScenarioFailure.Length > 0 ? firstScenarioFailure : firstSessionFault;
        Evidence.EnsureReportFiles(reportDir: reportDir, receipts: receipts);
        ArtifactRef[] artifacts = Evidence.ArtifactRefs(reportDir: reportDir);
        EvidenceCounts counts = Counts(evidence: carried, artifacts: artifacts, references: references);
        ScenarioCounts scenarioCounts = ScenarioCountsOf(receipts: receipts);
        PhaseReceipt[] phaseReceipts = [.. phases.Map(f: static phase => new PhaseReceipt(Phase: phase.Phase, Status: phase.Status, DurationMs: phase.DurationMs, Fault: phase.Fault))];
        EvidenceCertificate certificate = new(
            RunId: runId, Scenario: "session",
            Status: new StatusBreakdown(ScenarioStatus: scenarioStatus, SessionStatus: sessionStatus, OverallStatus: status),
            Classes: Classes(counts: counts), Counts: counts, Artifacts: artifacts, References: references,
            ObjectManifests: [], GeometryManifests: [], ViewportManifests: [], Gh2CanvasManifests: [],
            ScratchManifests: [], Phases: phaseReceipts,
            FirstFault: firstFailure.Length > 0 ? new FaultSummary(Phase: firstPhase, Fault: fault, Message: firstFailure) : null);
        string certificatePath = Evidence.WriteCertificate(reportDir: reportDir, certificate: certificate);
        return new SessionEnvelope(
            RunId: runId, Verb: verb.Key, Status: status, DurationMs: duration, ReportDir: reportDir,
            Host: Host(final: final), Capabilities: Capabilities(final: final), Scenarios: [.. receipts],
            Evidence: [.. carried], FirstFailure: firstFailure, FirstFaultPhase: firstPhase, Fault: fault) {
            ScenarioStatus = scenarioStatus,
            SessionStatus = sessionStatus,
            PhaseReceipts = phaseReceipts,
            FirstScenarioFailure = firstScenarioFailure,
            FirstSessionFault = firstSessionFault,
            CertificatePath = certificatePath,
            ArtifactRefs = artifacts,
            EvidenceCounts = counts,
            ScenarioCounts = scenarioCounts,
            Spool = spool,
        };
    }

    private static CapabilityEntry[] Capabilities(SessionState final) =>
        final switch {
            SessionState.Running running => running.Cargo.Capabilities,
            SessionState.Ready ready => ready.Peer.Capabilities,
            SessionState.Loading loading => loading.Peer.Capabilities,
            _ => [],
        };

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct EvidenceDivergence(
        [property: JsonPropertyName("spool")] long Spool,
        [property: JsonPropertyName("relayed")] long Relayed,
        [property: JsonPropertyName("spoolLastSequence")] long SpoolLastSequence);

    private static BridgeEvent.FactCase Divergence(string runId, Seq<BridgeEvent> ordered, (long Count, long LastSequence) spoolTail, long relayed) {
        Guid sessionId = ordered.Head.Case is BridgeEvent head ? head.Stamp.SessionId
            : Guid.TryParse(input: runId, result: out Guid parsed) ? parsed : Guid.Empty;
        (long lastSequence, long atUnixMs) = ordered.Last.Case is BridgeEvent tail
            ? (Math.Max(val1: tail.Stamp.Sequence, val2: spoolTail.LastSequence), tail.Stamp.AtUnixMs)
            : (spoolTail.LastSequence, 0L);
        return new BridgeEvent.FactCase(
            Key: "evidence.divergence",
            Value: JsonSerializer.SerializeToElement(value: new EvidenceDivergence(Spool: spoolTail.Count, Relayed: relayed, SpoolLastSequence: spoolTail.LastSequence))) {
            Stamp = new EventStamp(SessionId: sessionId, Sequence: lastSequence + 1, AtUnixMs: atUnixMs, Scenario: null),
        };
    }

    private static (string Failure, SessionPhase? Phase) FirstSessionFault(SessionState final, BridgeFault? fault,
        Seq<BridgeEvent.PhaseCase> phases) =>
        fault is not null && final is SessionState.Faulted faulted
            ? (Truncate(text: fault.Prescription), faulted.At)
            : phases.Filter(f: static phase => phase.Status.ExitCode != 0).Head.Case is BridgeEvent.PhaseCase firstPhase
                ? (Truncate(text: firstPhase.Fault?.Prescription ?? $"{firstPhase.Phase.Key} {firstPhase.Status.Key}"), firstPhase.Phase)
                : (string.Empty, null);

    private static (string Failure, SessionPhase? Phase) FirstScenarioFailure(Seq<ScenarioReceipt> receipts) =>
        receipts.Filter(f: static receipt => receipt.ScenarioStatus.ExitCode != 0).Head.Case is ScenarioReceipt firstReceipt
            ? (Truncate(text: firstReceipt.FirstScenarioFailure.Length > 0
                ? firstReceipt.FirstScenarioFailure
                : firstReceipt.Fault?.Prescription ?? $"{firstReceipt.Scenario} {firstReceipt.ScenarioStatus.Key}"), SessionPhase.Execute)
            : (string.Empty, null);

    private static Seq<ScenarioReceipt> AttachReferences(Seq<ScenarioReceipt> receipts, ReferenceEvidenceResult[] references) =>
        receipts.Map(f: receipt => {
            ReferenceEvidenceResult[] rows = [.. references.Where(predicate: result => string.Equals(a: result.Scenario, b: receipt.Scenario, comparisonType: StringComparison.Ordinal))];
            ReferenceEvidenceResult? firstFailure = rows.FirstOrDefault(predicate: static result =>
                !result.Matched && result.Admission != ReferenceAdmission.Candidate);
            return receipt with {
                ScenarioStatus = firstFailure is null ? receipt.ScenarioStatus : PhaseStatus.Failed,
                ReferenceResults = rows,
                FirstScenarioFailure = firstFailure is null ? receipt.FirstScenarioFailure : firstFailure.Detail,
            };
        });

    private static EvidenceCounts Counts(Seq<BridgeEvent> evidence, ArtifactRef[] artifacts, ReferenceEvidenceResult[] references) {
        Seq<BridgeEvent.FactCase> factRows = evidence.Choose(selector: static evt => evt is BridgeEvent.FactCase fact ? Some(value: fact) : Option<BridgeEvent.FactCase>.None);
        int facts = factRows.Count;
        int captures = evidence.Filter(f: static evt => evt is BridgeEvent.CaptureCase).Count;
        int objectManifests = factRows.Filter(f: static fact => fact.Key.StartsWith(value: "manifest.object.", comparisonType: StringComparison.Ordinal)).Count;
        int geometryManifests = factRows.Filter(f: static fact => fact.Key.StartsWith(value: "manifest.geometry.", comparisonType: StringComparison.Ordinal)).Count;
        int viewportManifests = factRows.Filter(f: static fact => fact.Key.StartsWith(value: "manifest.viewport.", comparisonType: StringComparison.Ordinal)).Count;
        int gh2Manifests = factRows.Filter(f: static fact => fact.Key.StartsWith(value: "manifest.gh2.", comparisonType: StringComparison.Ordinal)).Count;
        int scratchManifests = artifacts.Count(predicate: static artifact => artifact.Role == EvidenceRole.Scratch);
        return new EvidenceCounts(
            Facts: facts, Assertions: factRows.Filter(f: static fact => fact.Key.StartsWith(value: "case.", comparisonType: StringComparison.Ordinal)).Count,
            References: references.Length,
            ReferenceMatches: references.Count(predicate: static reference => reference.Matched && reference.Admission == ReferenceAdmission.Matched),
            ReferenceFailures: references.Count(predicate: static reference => !reference.Matched && reference.Admission != ReferenceAdmission.Candidate),
            Captures: captures, Artifacts: artifacts.Length,
            ObjectManifests: objectManifests + artifacts.Count(predicate: static artifact => artifact.Role == EvidenceRole.ObjectManifest),
            GeometryManifests: geometryManifests + artifacts.Count(predicate: static artifact => artifact.Role == EvidenceRole.GeometryManifest),
            ViewportManifests: viewportManifests + artifacts.Count(predicate: static artifact => artifact.Role == EvidenceRole.ViewportManifest),
            Gh2CanvasManifests: gh2Manifests + artifacts.Count(predicate: static artifact => artifact.Role == EvidenceRole.Gh2CanvasManifest),
            ScratchManifests: scratchManifests);
    }

    private static EvidenceClass[] Classes(EvidenceCounts counts) =>
        [.. new[] {
            EvidenceClass.Smoke,
            counts.Facts > 0 ? EvidenceClass.Semantic : null,
            counts.ObjectManifests + counts.GeometryManifests > 0 ? EvidenceClass.Geometry : null,
            counts.Captures > 0 || counts.Gh2CanvasManifests > 0 ? EvidenceClass.Visual : null,
            counts.References > 0 ? EvidenceClass.CertifiedReference : null,
        }.OfType<EvidenceClass>()];

    private static ScenarioCounts ScenarioCountsOf(Seq<ScenarioReceipt> receipts) =>
        new(
            Total: receipts.Count,
            Ok: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Ok).Count,
            Failed: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Failed).Count,
            Skipped: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Skipped).Count,
            Unsupported: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Unsupported).Count,
            Timeout: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Timeout).Count,
            Busy: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Busy).Count,
            Degraded: receipts.Filter(f: static receipt => receipt.ScenarioStatus == PhaseStatus.Degraded).Count);

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
                new ScenarioReceipt(Scenario: entry.Name, Status: PhaseStatus.Skipped, DurationMs: 0.0, Fault: null) {
                    ScenarioStatus = PhaseStatus.Skipped,
                    CertificatePath = string.Empty,
                    ArtifactRefs = [],
                    ReferenceResults = [],
                    FirstScenarioFailure = string.Empty,
                }),
            SessionState.Faulted faulted => faulted.Done,
            _ => Seq<ScenarioReceipt>(),
        };

    private static string Truncate(string text) =>
        text.Length <= FirstFailureCap ? text : text[..FirstFailureCap];
}
