using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ---------------------------------------------------------------------------

[Union]
internal abstract partial record SessionState {
    private SessionState() { }
    internal sealed record Idle(BundleInfo Bundle) : SessionState;
    internal sealed record Connecting(LiveHost Host) : SessionState;
    internal sealed record Negotiating(LiveHost Host) : SessionState;
    internal sealed record Ready(LiveHost Host) : SessionState;
    internal sealed record Loading(LiveHost Host) : SessionState;
    internal sealed record Running(LiveHost Host, LoadedCargo Cargo, Seq<ScenarioOutcome> Done, Seq<ScenarioEntry> Remaining) : SessionState;
    internal sealed record Unloading(LiveHost Host, LoadedCargo Cargo) : SessionState;
    internal sealed record Quitting(LiveHost Host) : SessionState;
    internal sealed record Faulted(BridgeFault Fault, SessionPhase At, Seq<ScenarioOutcome> Done) : SessionState;

    public HostFingerprint Fingerprint => Switch(
        idle: static _ => default,
        connecting: static state => state.Host.Fingerprint,
        negotiating: static state => state.Host.Fingerprint,
        ready: static state => state.Host.Fingerprint,
        loading: static state => state.Host.Fingerprint,
        running: static state => state.Host.Fingerprint,
        unloading: static state => state.Host.Fingerprint,
        quitting: static state => state.Host.Fingerprint,
        faulted: static _ => default);

    public CapabilityEntry[] Capabilities => Switch(
        idle: static _ => [],
        connecting: static _ => [],
        negotiating: static _ => [],
        ready: static _ => [],
        loading: static _ => [],
        running: static state => state.Cargo.Capabilities,
        unloading: static state => state.Cargo.Capabilities,
        quitting: static _ => [],
        faulted: static _ => []);

    public Seq<ScenarioOutcome> Outcomes => Switch(
        idle: static _ => Seq<ScenarioOutcome>(),
        connecting: static _ => Seq<ScenarioOutcome>(),
        negotiating: static _ => Seq<ScenarioOutcome>(),
        ready: static _ => Seq<ScenarioOutcome>(),
        loading: static _ => Seq<ScenarioOutcome>(),
        running: static state => state.Done + state.Remaining.Map(static entry => new ScenarioOutcome(entry.Name, PhaseStatus.Skipped, 0.0, Fault: null)),
        unloading: static _ => Seq<ScenarioOutcome>(),
        quitting: static _ => Seq<ScenarioOutcome>(),
        faulted: static state => state.Done);

    public Option<BridgeFault> Refusal => Switch(
        idle: static _ => Option<BridgeFault>.None,
        connecting: static _ => Option<BridgeFault>.None,
        negotiating: static _ => Option<BridgeFault>.None,
        ready: static _ => Option<BridgeFault>.None,
        loading: static _ => Option<BridgeFault>.None,
        running: static _ => Option<BridgeFault>.None,
        unloading: static _ => Option<BridgeFault>.None,
        quitting: static _ => Option<BridgeFault>.None,
        faulted: static state => Some(state.Fault));
}

[Union]
internal abstract partial record SessionSignal {
    private SessionSignal() { }
    internal sealed record HostExited(int Pid, long AtUnixMs, Option<CrashFact> Report) : SessionSignal;
    internal sealed record DeadlineHit(SessionPhase Phase, TimeSpan Elapsed) : SessionSignal;
}

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record SessionPolicy(
    TimeSpan ConnectDeadline, TimeSpan HelloDeadline, TimeSpan LoadDeadline, TimeSpan UnloadDeadline,
    TimeSpan QuitRungDeadline, TimeSpan SessionDeadline, TimeSpan ScenarioDefaultBudget,
    TimeSpan WatchPoll, TimeSpan JournalSlack, TimeSpan ToolDeadline) {

    public static readonly SessionPolicy Default = new(
        ConnectDeadline: TimeSpan.FromSeconds(90),
        HelloDeadline: TimeSpan.FromSeconds(10),
        LoadDeadline: TimeSpan.FromSeconds(60),
        UnloadDeadline: TimeSpan.FromSeconds(15),
        QuitRungDeadline: TimeSpan.FromSeconds(15),
        SessionDeadline: TimeSpan.FromSeconds(540),
        ScenarioDefaultBudget: TimeSpan.FromSeconds(30),
        WatchPoll: TimeSpan.FromMilliseconds(250),
        JournalSlack: TimeSpan.FromSeconds(120),
        ToolDeadline: TimeSpan.FromSeconds(15));

    public Option<TimeSpan> DeadlineFor(SessionState state) {
        ArgumentNullException.ThrowIfNull(state);
        return state.Switch(
            state: this,
            idle: static (_, _) => Option<TimeSpan>.None,
            connecting: static (policy, _) => Some(policy.ConnectDeadline),
            negotiating: static (policy, _) => Some(policy.HelloDeadline),
            ready: static (_, _) => Option<TimeSpan>.None,
            loading: static (policy, _) => Some(policy.LoadDeadline),
            running: static (policy, _) => Some(policy.ScenarioDefaultBudget),
            unloading: static (policy, _) => Some(policy.UnloadDeadline),
            quitting: static (policy, _) => Some(policy.QuitRungDeadline),
            faulted: static (_, _) => Option<TimeSpan>.None);
    }

    public TimeSpan ExecuteBudget(ScenarioEntry[] selected) {
        ArgumentNullException.ThrowIfNull(selected);
        double totalMs = selected.Sum(entry => entry.BudgetMs > 0 ? entry.BudgetMs : ScenarioDefaultBudget.TotalMilliseconds);
        return totalMs > 0.0 ? TimeSpan.FromMilliseconds(Math.Min(totalMs, SessionDeadline.TotalMilliseconds)) : ScenarioDefaultBudget;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class SessionRun {
    private readonly SupervisorVerb verb;
    private readonly SupervisorRuntime runtime;
    private readonly Guid sessionId = Guid.NewGuid();
    private readonly List<BridgeEvent> stream = [];
    private readonly string runId;
    private readonly string reportDir;
    private long sequence;

    internal SessionRun(SupervisorVerb verb, SupervisorRuntime runtime) {
        ArgumentNullException.ThrowIfNull(verb);
        ArgumentNullException.ThrowIfNull(runtime);
        this.verb = verb;
        this.runtime = runtime;
        runId = sessionId.ToString("n");
        reportDir = Path.Combine(runtime.ArtifactRoot, runId);
    }

    internal async Task<SessionEnvelope> RunAsync() {
        _ = Directory.CreateDirectory(reportDir);
        Fin<Lease.Token> claimed = Lease.Acquire(runtime.LeasePath, sessionId, runtime.Clock, stream.Add);
        if (claimed is not Fin<Lease.Token>.Succ(Lease.Token lease)) {
            return Fold(Fault(new BridgeFault.LaunchFailed(Detail(claimed)), verb.EntryPhase));
        }
        _ = runtime.Held.Swap(_ => Some(lease));
        try {
            return await verb.Switch(
                state: this,
                verify: static (run, verify) => run.VerifyAsync(verify),
                status: static (run, _) => run.StatusAsync(),
                quit: static (run, _) => run.QuitAsync()).ConfigureAwait(false);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Fold(Fault(new BridgeFault.LaunchFailed($"{error.GetType().Name}: {error.Message}"), verb.EntryPhase));
        } finally {
            _ = runtime.Held.Swap(_ => Option<Lease.Token>.None);
            _ = Lease.Release(lease);
        }
    }

    // --- [VERBS]

    private Task<SessionEnvelope> StatusAsync() =>
        WithHostAsync(SessionPhase.Status, async (connection, live, machine) => {
            LiveHost negotiated = await NegotiateAsync(connection, live, machine).ConfigureAwait(false);
            Phase(SessionPhase.Hello, PhaseStatus.Ok);
            stream.Add(Fact("status.endpoint", negotiated.Endpoint.PipeName));
            return new SessionState.Ready(negotiated);
        });

    private Task<SessionEnvelope> VerifyAsync(SupervisorVerb.Verify verify) =>
        WithHostAsync(SessionPhase.Connect, async (connection, live, machine) => {
            LiveHost retire = live;
            try {
                LiveHost negotiated = await NegotiateAsync(connection, live, machine).ConfigureAwait(false);
                retire = negotiated;
                Phase(SessionPhase.Hello, PhaseStatus.Ok);
                Fin<CargoManifest> staged = Evidence.Stage(runtime.PayloadRoot, sessionId, reportDir, Path.Combine(reportDir, ReportLayout.StageDirectory));
                if (staged is not Fin<CargoManifest>.Succ(CargoManifest manifest)) {
                    return Fault(new BridgeFault.LaunchFailed(Detail(staged)), SessionPhase.Stage);
                }
                Phase(SessionPhase.Stage, PhaseStatus.Ok);
                LoadedCargo cargo = await machine.RunPhaseAsync(SessionPhase.Load, new SessionState.Loading(negotiated),
                    ct => connection.Shell.LoadCargoAsync(manifest, ct)).ConfigureAwait(false);
                Phase(SessionPhase.Load, PhaseStatus.Ok, cargo.LoadMs);
                ScenarioEntry[] chosen = verify.Selection.Filter(cargo.Scenarios);
                ScenarioOutcome[] outcomes = await machine.RunPhaseAsync(SessionPhase.Execute,
                    new SessionState.Running(negotiated, cargo, Seq<ScenarioOutcome>(), toSeq(chosen)),
                    ct => connection.Shell.RunAsync(verify.Selection, ct),
                    runtime.Policy.ExecuteBudget(chosen)).ConfigureAwait(false);
                UnloadOutcome unload = await machine.RunPhaseAsync(SessionPhase.Unload, new SessionState.Unloading(negotiated, cargo),
                    ct => connection.Shell.UnloadCargoAsync(ct)).ConfigureAwait(false);
                stream.Add(Fact(unload.ReleaseRequested ? "cargo.release.requested" : "cargo.release.empty",
                    string.Create(CultureInfo.InvariantCulture, $"elapsedMs={unload.ElapsedMs:F0}")));
                Phase(SessionPhase.Unload, PhaseStatus.Ok, unload.ElapsedMs);
                return new SessionState.Running(negotiated, cargo, toSeq(outcomes), Seq<ScenarioEntry>());
            } finally {
                await RetireAsync(connection, retire, machine).ConfigureAwait(false);
            }
        });

    private Task<SessionEnvelope> QuitAsync() {
        Fin<LiveHost> admitted = Endpoint.ReadLive();
        if (admitted is not Fin<LiveHost>.Succ(LiveHost host)) {
            stream.Add(Fact("quit.no-host", Detail(admitted)));
            return Task.FromResult(Fold(new SessionState.Idle(runtime.Bundle)));
        }
        return WithConnectionAsync(host, SessionPhase.QuitAe, async (connection, live, machine) => {
            LiveHost negotiated = await NegotiateAsync(connection, live, machine).ConfigureAwait(false);
            await machine.QuiesceAsync(negotiated, sessionId, connection.Shell.PrepareQuitAsync, stream.Add).ConfigureAwait(false);
            PhaseStatus outcome = QuitLadder.Run(runtime, negotiated, sessionId, stream.Add);
            return outcome == PhaseStatus.Ok
                ? new SessionState.Quitting(negotiated)
                : Fault(new BridgeFault.ExecuteDeadline(SessionPhase.QuitKill.Key, runtime.Policy.QuitRungDeadline.TotalMilliseconds), SessionPhase.QuitKill);
        });
    }

    // --- [LADDER]

    private static async Task<LiveHost> NegotiateAsync(SupervisorConnection connection, LiveHost live, SessionMachine machine) {
        HostFingerprint fingerprint = await machine.RunPhaseAsync(SessionPhase.Hello, new SessionState.Negotiating(live),
            ct => connection.Shell.HelloAsync(Environment.ProcessId, ct)).ConfigureAwait(false);
        return live with { Fingerprint = fingerprint };
    }

    private async Task RetireAsync(SupervisorConnection connection, LiveHost host, SessionMachine machine) {
        try {
            await machine.QuiesceAsync(host, sessionId, connection.Shell.PrepareQuitAsync, stream.Add).ConfigureAwait(false);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            stream.Add(Fact("quit.prepare.failed", $"{error.GetType().Name}: {error.Message}"));
        } finally {
            _ = QuitLadder.Run(runtime, host, sessionId, stream.Add);
        }
    }

    private Task<SessionEnvelope> WithHostAsync(SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionMachine, Task<SessionState>> body) {
        stream.AddRange(Reconcile.Sweep(runtime, sessionId));
        Fin<LiveHost> reused = Endpoint.ReadLive();
        Fin<LiveHost> host = reused.IsSucc ? reused : LaunchAndPoll();
        return host.Match(
            live => WithConnectionAsync(live, connectPhase, body),
            error => Task.FromResult(Fold(Fault(new BridgeFault.ConnectFailed(error.Message, 0.0), SessionPhase.Connect))));
    }

    private Fin<LiveHost> LaunchAndPoll() {
        long started = Environment.TickCount64;
        stream.AddRange(Reconcile.ClearRecovery(runtime, sessionId));
        Fin<Unit> launched = runtime.Bundle.Launch(runtime.Policy.ToolDeadline);
        if (launched.IsFail) {
            BridgeFault.LaunchFailed launchFault = new(Detail(launched));
            Phase(SessionPhase.Launch, launchFault.Status, Elapsed(started), launchFault);
            return Error.New(launchFault.Prescription);
        }
        Phase(SessionPhase.Launch, PhaseStatus.Ok, Elapsed(started));
        Fin<LiveHost> polled = Poll.Until(static () => Endpoint.ReadLive().ToOption(), runtime.Policy.ConnectDeadline, runtime.Policy.WatchPoll, runtime.Root);
        if (polled is Fin<LiveHost>.Succ(LiveHost live)) {
            Phase(SessionPhase.Connect, PhaseStatus.Ok, Elapsed(started));
            return live;
        }
        BridgeFault.ConnectFailed connectFault = new("endpoint did not appear before the connect deadline", Elapsed(started));
        Phase(SessionPhase.Connect, connectFault.Status, connectFault.ElapsedMs, connectFault);
        return Error.New(connectFault.Prescription);
    }

    private async Task<SessionEnvelope> WithConnectionAsync(LiveHost host, SessionPhase connectPhase, Func<SupervisorConnection, LiveHost, SessionMachine, Task<SessionState>> body) {
        SessionState final;
        using SessionMachine machine = SessionMachine.Open(host, runtime);
        _ = runtime.LiveHostPid.Swap(_ => Some(host.Pid));
        try {
            SupervisorConnection connection = await SupervisorConnection.ConnectAsync(host.Endpoint.PipeName, runtime.Policy.ConnectDeadline, runtime.Root).ConfigureAwait(false);
            await using (connection.ConfigureAwait(false)) {
                try {
                    final = await body(connection, host, machine).ConfigureAwait(false);
                } finally {
                    stream.AddRange(connection.Events);
                }
            }
        } catch (SessionMachine.PhaseFaultedException faulted) {
            final = Fault(faulted.Faulted.Fault, faulted.Faulted.At);
        } catch (Exception error) when (error is RemoteRpcException or JsonException or IOException or TimeoutException or ObjectDisposedException or OperationCanceledException) {
            final = Fault(FaultOf(error), connectPhase);
        }
        return Fold(final);
    }

    // --- [FOLD]

    private SessionState.Faulted Fault(BridgeFault fault, SessionPhase at) {
        Phase(at, fault.Status, fault: fault);
        return new SessionState.Faulted(fault, at, Seq<ScenarioOutcome>());
    }

    private SessionEnvelope Fold(SessionState final) =>
        SessionFold.Run(runId, verb, final, toSeq(stream), Evidence.SpoolTail(reportDir), reportDir);

    private EventStamp Next() => new(sessionId, Interlocked.Increment(ref sequence), runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds(), Scenario: null);

    private void Phase(SessionPhase phase, PhaseStatus status, double durationMs = 0.0, BridgeFault? fault = null) =>
        stream.Add(new BridgeEvent.PhaseCase(phase, status, durationMs, fault) { Stamp = Next() });

    private BridgeEvent.FactCase Fact(string key, string value) => BridgeEvent.Fact(key, value, Next());

    private static string Detail<T>(Fin<T> result) => result.Match(static _ => string.Empty, static error => error.Message);

    private static double Elapsed(long started) => Environment.TickCount64 - started;

    private static BridgeFault FaultOf(Exception error) =>
        error switch {
            RemoteInvocationException { DeserializedErrorData: BridgeFault fault } => fault,
            RemoteInvocationException { DeserializedErrorData: JsonElement element } when RemoteFault(element).Case is BridgeFault fault => fault,
            RemoteMethodNotFoundException missing => new BridgeFault.CapabilityAbsent("rpc.method", missing.Message),
            OperationCanceledException => new BridgeFault.ConnectFailed("supervisor interrupted", 0.0),
            TimeoutException or IOException or ObjectDisposedException or JsonException => new BridgeFault.ConnectFailed(error.Message, 0.0),
            _ => new BridgeFault.LaunchFailed(error.Message),
        };

    private static Option<BridgeFault> RemoteFault(JsonElement element) {
        try {
            return Optional(JsonSerializer.Deserialize(element.GetRawText(), BridgeJsonContext.Default.BridgeFault));
        } catch (JsonException) {
            return Option<BridgeFault>.None;
        }
    }
}

internal sealed class SessionMachine : IDisposable {
    private readonly Atom<SessionState> cursor;
    private readonly SessionPolicy policy;
    private readonly TimeProvider clock;
    private readonly CancellationToken root;
    private readonly HostWatch watch;
    private readonly TaskCompletionSource<SessionState.Faulted> faultedGate = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private SessionMachine(LiveHost host, SupervisorRuntime runtime) {
        cursor = Atom((SessionState)new SessionState.Connecting(host));
        policy = runtime.Policy;
        clock = runtime.Clock;
        root = runtime.Root;
        cursor.Change += Observe;
        watch = HostWatch.Attach(host.Pid, () => Raise(new SessionSignal.HostExited(
            host.Pid,
            clock.GetUtcNow().ToUnixTimeMilliseconds(),
            Evidence.IpsDiff(runtime.CrashBaseline, runtime.DiagnosticReportsDirectory, runtime.Bundle))), policy.WatchPoll);
    }

    internal static SessionMachine Open(LiveHost host, SupervisorRuntime runtime) => new(host, runtime);

    internal async Task<T> RunPhaseAsync<T>(SessionPhase phase, SessionState phaseState, Func<CancellationToken, Task<T>> rpc, Option<TimeSpan> deadline = default) {
        ArgumentNullException.ThrowIfNull(rpc);
        _ = cursor.Swap(_ => phaseState);
        TimeSpan window = deadline.IfNone(() => policy.DeadlineFor(phaseState).IfNone(policy.SessionDeadline));
        using CancellationTokenSource scope = CancellationTokenSource.CreateLinkedTokenSource(root);
        await using ConfiguredAsyncDisposable tripped = scope.Token.Register(() => Raise(new SessionSignal.DeadlineHit(phase, window))).ConfigureAwait(false);
        scope.CancelAfter(window);
        Task<T> work = rpc(scope.Token);
        Task done = await Task.WhenAny(work, faultedGate.Task).ConfigureAwait(false);
        if (ReferenceEquals(done, faultedGate.Task)) {
            await scope.CancelAsync().ConfigureAwait(false);
            throw new PhaseFaultedException(await faultedGate.Task.ConfigureAwait(false));
        }
        return await work.ConfigureAwait(false);
    }

    internal Task QuiesceAsync(LiveHost host, Guid sessionId, Func<CancellationToken, Task<QuitScrub>> prepare, Action<BridgeEvent> publish) {
        _ = cursor.Swap(_ => new SessionState.Quitting(host));
        return QuitPrepare.RunAsync(prepare, policy.QuitRungDeadline, clock, sessionId, publish, root);
    }

    public void Dispose() {
        cursor.Change -= Observe;
        watch.Dispose();
        _ = faultedGate.TrySetCanceled();
    }

    private void Observe(SessionState state) {
        if (state is SessionState.Faulted faulted) {
            _ = faultedGate.TrySetResult(faulted);
        }
    }

    private void Raise(SessionSignal signal) => _ = cursor.Swap(state => SessionDispatch.Apply(state, signal, policy));

    internal sealed class PhaseFaultedException : Exception {
        internal PhaseFaultedException(SessionState.Faulted faulted) : base(faulted.Fault.Prescription) => Faulted = faulted;
        internal PhaseFaultedException() : this("phase faulted") { }
        internal PhaseFaultedException(string message) : this(new SessionState.Faulted(new BridgeFault.LaunchFailed(message), SessionPhase.Connect, Seq<ScenarioOutcome>())) { }
        internal PhaseFaultedException(string message, Exception innerException) : base(message, innerException) =>
            Faulted = new SessionState.Faulted(new BridgeFault.LaunchFailed(message), SessionPhase.Connect, Seq<ScenarioOutcome>());

        internal SessionState.Faulted Faulted { get; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class SessionDispatch {
    private const string CargoLoad = "cargo.load";
    private const string CargoRelease = "cargo.release";

    internal static SessionState Apply(SessionState state, SessionSignal signal, SessionPolicy policy) {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(policy);
        return state.Switch(
            state: (Signal: signal, Policy: policy),
            idle: static (_, current) => current,
            connecting: static (ctx, current) => HostPhase(current, ctx.Signal, SessionPhase.Connect, ctx.Policy.ConnectDeadline),
            negotiating: static (ctx, current) => HostPhase(current, ctx.Signal, SessionPhase.Hello, ctx.Policy.HelloDeadline),
            ready: static (_, current) => current,
            loading: static (ctx, current) => CargoPhase(current, CargoLoad, Seq<ScenarioOutcome>(), ctx.Signal, SessionPhase.Load, ctx.Policy.LoadDeadline),
            running: static (ctx, current) => CargoPhase(current, InFlight(current), current.Done, ctx.Signal, SessionPhase.Execute, ctx.Policy.ScenarioDefaultBudget),
            unloading: static (ctx, current) => CargoPhase(current, CargoRelease, Seq<ScenarioOutcome>(), ctx.Signal, SessionPhase.Unload, ctx.Policy.UnloadDeadline),
            quitting: static (_, current) => current,
            faulted: static (_, current) => current);
    }

    private static SessionState HostPhase(SessionState current, SessionSignal signal, SessionPhase phase, TimeSpan deadline) =>
        signal.Switch(
            state: (Current: current, Phase: phase, Deadline: deadline),
            hostExited: static (ctx, exited) => new SessionState.Faulted(new BridgeFault.LaunchFailed(Exited(exited.Pid, ctx.Phase)), ctx.Phase, Seq<ScenarioOutcome>()),
            deadlineHit: static (ctx, hit) => hit.Elapsed >= ctx.Deadline
                ? new SessionState.Faulted(new BridgeFault.ConnectFailed(Deadline(ctx.Phase, hit.Elapsed), hit.Elapsed.TotalMilliseconds), ctx.Phase, Seq<ScenarioOutcome>())
                : ctx.Current);

    private static SessionState CargoPhase(SessionState current, string scenario, Seq<ScenarioOutcome> done, SessionSignal signal, SessionPhase phase, TimeSpan deadline) =>
        signal.Switch(
            state: (Current: current, Scenario: scenario, Done: done, Phase: phase, Deadline: deadline),
            hostExited: static (ctx, exited) => new SessionState.Faulted(
                new BridgeFault.RhinoCrash(exited.Report.IfNone(new CrashFact(string.Empty, "unknown", "unknown", string.Empty)) with { Detail = Crashed(exited.Pid, ctx.Scenario) }, ctx.Scenario),
                ctx.Phase, ctx.Done),
            deadlineHit: static (ctx, hit) => hit.Elapsed >= ctx.Deadline
                ? new SessionState.Faulted(new BridgeFault.ExecuteDeadline(ctx.Scenario, hit.Elapsed.TotalMilliseconds), ctx.Phase, ctx.Done)
                : ctx.Current);

    private static string Crashed(int pid, string scenario) =>
        string.Create(CultureInfo.InvariantCulture, $"host pid {pid} exited inside '{scenario}'");

    private static string Deadline(SessionPhase phase, TimeSpan elapsed) =>
        string.Create(CultureInfo.InvariantCulture, $"{phase.Key} deadline after {elapsed.TotalMilliseconds:F0}ms");

    private static string Exited(int pid, SessionPhase during) =>
        string.Create(CultureInfo.InvariantCulture, $"host pid {pid} exited during {during.Key}");

    private static string InFlight(SessionState.Running running) =>
        running.Remaining.Head.Map(static entry => entry.Name).IfNone("session");
}

internal static class SessionFold {
    private const int FirstFailureCap = 256;

    internal static SessionEnvelope Run(string runId, SupervisorVerb verb, SessionState final, Seq<BridgeEvent> stream, (long Count, long LastSequence) spoolTail, string reportDir) {
        ArgumentNullException.ThrowIfNull(verb);
        ArgumentNullException.ThrowIfNull(final);
        Seq<BridgeEvent> ordered = toSeq(stream.OrderBy(static evt => evt.Stamp.AtUnixMs).ThenBy(static evt => evt.Stamp.Sequence));
        Seq<ScenarioOutcome> outcomes = final.Outcomes;
        Option<BridgeFault> fault = final.Refusal;
        Seq<BridgeEvent.PhaseCase> phases = ordered.Choose(static evt => evt is BridgeEvent.PhaseCase phase ? Some(phase) : Option<BridgeEvent.PhaseCase>.None);
        Seq<BridgeEvent.PhaseCase> sessionPhases = phases.Filter(static phase => phase.Stamp.Scenario is null);
        Seq<BridgeEvent> evidence = ordered.Filter(static evt => evt.IsEvidence);
        long relayed = evidence.Filter(static evt => evt.Stamp.Scenario is { Length: > 0 }).Count;
        SpoolSummary spool = new(spoolTail.Count, relayed, spoolTail.LastSequence);
        PhaseStatus sessionStatus = (sessionPhases.Map(static phase => phase.Status) + fault.Map(static f => f.Status).ToSeq())
            .Fold(PhaseStatus.Ok, static (accumulator, observed) => accumulator.Worst(observed));
        sessionStatus = spool.Diverged && sessionStatus == PhaseStatus.Ok ? PhaseStatus.Degraded : sessionStatus;
        PhaseStatus scenarioStatus = outcomes.Map(static outcome => outcome.Status).Fold(PhaseStatus.Ok, static (accumulator, observed) => accumulator.Worst(observed));
        (string firstSessionFault, SessionPhase? faultPhase) = FirstSessionFault(final, sessionPhases);
        string firstScenarioFailure = FirstScenarioFailure(outcomes);
        Seq<BridgeEvent> carried = spool.Diverged ? evidence + Seq<BridgeEvent>(Divergence(runId, ordered, spool)) : evidence;
        double duration = ordered.Head.Case is BridgeEvent head && ordered.Last.Case is BridgeEvent tail ? tail.Stamp.AtUnixMs - head.Stamp.AtUnixMs : 0.0;
        ArtifactRef[] artifacts = Evidence.ArtifactRefs(reportDir);
        SessionEnvelope envelope = new(
            runId, verb.Key, new StatusBreakdown(scenarioStatus, sessionStatus, scenarioStatus.Worst(sessionStatus)), duration, reportDir,
            final.Fingerprint, final.Capabilities, [.. outcomes], [.. phases], [.. carried], artifacts, Counts(carried, artifacts), spool,
            firstScenarioFailure.Length > 0 ? firstScenarioFailure : firstSessionFault,
            firstScenarioFailure.Length > 0 ? SessionPhase.Execute : faultPhase, fault.Match(static BridgeFault? (f) => f, static () => null));
        _ = Evidence.WriteCertificate(reportDir, envelope);
        return envelope;
    }

    private static BridgeEvent.FactCase Divergence(string runId, Seq<BridgeEvent> ordered, SpoolSummary spool) {
        Guid sessionId = ordered.Head.Case is BridgeEvent head ? head.Stamp.SessionId : Guid.TryParse(runId, out Guid parsed) ? parsed : Guid.Empty;
        (long lastSequence, long atUnixMs) = ordered.Last.Case is BridgeEvent tail ? (Math.Max(tail.Stamp.Sequence, spool.LastSequence), tail.Stamp.AtUnixMs) : (spool.LastSequence, 0L);
        return BridgeEvent.Fact(
            "evidence.divergence",
            new JsonObject { ["spool"] = spool.DurableEvents, ["relayed"] = spool.RelayedEvents, ["spoolLastSequence"] = spool.LastSequence },
            new EventStamp(sessionId, lastSequence + 1, atUnixMs, Scenario: null));
    }

    private static (string Failure, SessionPhase? Phase) FirstSessionFault(SessionState final, Seq<BridgeEvent.PhaseCase> phases) =>
        final is SessionState.Faulted faulted
            ? (Truncate(faulted.Fault.Prescription), faulted.At)
            : phases.Filter(static phase => phase.Status.ExitCode != 0).Head.Case is BridgeEvent.PhaseCase firstPhase
                ? (Truncate(firstPhase.Fault?.Prescription ?? $"{firstPhase.Phase.Key} {firstPhase.Status.Key}"), firstPhase.Phase)
                : (string.Empty, null);

    private static string FirstScenarioFailure(Seq<ScenarioOutcome> outcomes) =>
        outcomes.Filter(static outcome => outcome.Status.ExitCode != 0).Head.Case is ScenarioOutcome first
            ? Truncate(first.Fault?.Prescription ?? $"{first.Scenario} {first.Status.Key}")
            : string.Empty;

    private static EvidenceCounts Counts(Seq<BridgeEvent> evidence, ArtifactRef[] artifacts) {
        Seq<EvidenceRole> roles = evidence.Choose(static evt => evt is BridgeEvent.FactCase fact ? Some(EvidenceRole.OfFactKey(fact.Key)) : Option<EvidenceRole>.None);
        return new EvidenceCounts(
            Facts: roles.Count,
            Assertions: roles.Filter(static role => role == EvidenceRole.Assertion).Count,
            Manifests: roles.Filter(static role => role.IsManifest).Count,
            Captures: evidence.Filter(static evt => evt is BridgeEvent.CaptureCase).Count,
            Artifacts: artifacts.Length);
    }

    private static string Truncate(string text) => text.Length <= FirstFailureCap ? text : text[..FirstFailureCap];
}
