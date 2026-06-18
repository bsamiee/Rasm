using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: argv admits into SupervisorVerb and collapses to one SessionEnvelope at Main. Verify
// is shaped by ScenarioSelection plus closure manifest, not flags or sibling verbs.
[Union]
internal abstract partial record SupervisorVerb {
    private SupervisorVerb() { }
    internal sealed record Verify(ScenarioSelection Selection, string ClosureManifest, string EvidenceMode) : SupervisorVerb;
    internal sealed record Status : SupervisorVerb;
    // The direct supervisor does not own redeploy — Assay owns the stable operator spelling and the real
    // package cycle. The verb is admitted only to report itself UNSUPPORTED (exit 3); PackagePath is no
    // longer silently dropped into a constant exit-1 trap but surfaced verbatim in the unsupported receipt.
    internal sealed record Redeploy(string PackagePath) : SupervisorVerb;
    internal sealed record Quit : SupervisorVerb;

    public string Key => Switch(
        verify: static _ => "verify",
        status: static _ => "status",
        redeploy: static _ => "redeploy",
        quit: static _ => "quit");

    public SessionPhase EntryPhase => Switch(
        verify: static _ => SessionPhase.Launch,
        status: static _ => SessionPhase.Status,
        redeploy: static _ => SessionPhase.Install,
        quit: static _ => SessionPhase.QuitAe);
}

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: composition-edge runtime surface; policy rows and storage paths are composed once. The
// Lease and LiveHostPid cells commit the acquired teardown state so the signal-edge shutdown owner
// releases the lease and kills the orphaned host synchronously without reaching a blocked-await finally.
internal sealed record SupervisorRuntime(
    Atom<Option<LeaseToken>> Lease, Atom<Option<int>> LiveHostPid, TimeProvider Clock, SessionPolicy Policy,
    string ArtifactRoot, string LeasePath, string JournalPath, BundleInfo Bundle, CancellationToken Root);

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: verb admission, derived help, and per-verb session pipelines.
internal static class Verbs {
    internal const int UsageExitCode = 2;

    internal static string Help() {
        JsonObject document = new() {
            ["tool"] = "rasm-bridge-supervisor",
            ["stdout"] = "one SessionEnvelope JSON document",
            ["verbs"] = new JsonArray([.. Cases().Select(selector: static @case => VerbNode(@case: @case))]),
            ["exitCodes"] = new JsonObject(properties: PhaseStatus.Items
                .Select(selector: static status => KeyValuePair.Create<string, JsonNode?>(key: status.Key, value: status.ExitCode))
                .Append(element: KeyValuePair.Create<string, JsonNode?>(key: "usage", value: UsageExitCode))),
        };
        return document.ToJsonString();
    }

    internal static Fin<SupervisorVerb> Parse(string[] argv) {
        ArgumentNullException.ThrowIfNull(argument: argv);
        return argv switch {
            ["verify", { } selection, { } manifest] => Selection(raw: selection)
                .Map(f: SupervisorVerb (admitted) => new SupervisorVerb.Verify(Selection: admitted, ClosureManifest: manifest, EvidenceMode: "verify")),
            ["verify", { } selection, { } manifest, { } evidenceMode] when evidenceMode is "verify" or "author" => Selection(raw: selection)
                .Map(f: SupervisorVerb (admitted) => new SupervisorVerb.Verify(Selection: admitted, ClosureManifest: manifest, EvidenceMode: evidenceMode)),
            ["status"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Status()),
            ["redeploy", { } package] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Redeploy(PackagePath: package)),
            ["quit"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Quit()),
            _ => Fin.Fail<SupervisorVerb>(error: Error.New(message: "unrecognized invocation: verify <selection-json> <closure-manifest> [verify|author] | status | redeploy <package> | quit")),
        };
    }

    private static IEnumerable<Type> Cases() =>
        typeof(SupervisorVerb).GetNestedTypes(bindingAttr: BindingFlags.Public | BindingFlags.NonPublic)
            .Where(predicate: static candidate => candidate.IsSealed && candidate.IsSubclassOf(c: typeof(SupervisorVerb)));

    private static Fin<ScenarioSelection> Selection(string raw) {
        try {
            return JsonSerializer.Deserialize(json: raw, jsonTypeInfo: BridgeJsonContext.Default.ScenarioSelection) is { } admitted
                ? Fin.Succ(value: admitted)
                : Fin.Fail<ScenarioSelection>(error: Error.New(message: "selection decoded to null"));
        } catch (JsonException decode) {
            return Fin.Fail<ScenarioSelection>(error: Error.New(message: $"selection is not a ScenarioSelection union document: {decode.Message}"));
        }
    }

    private static string Shape(Type parameter) =>
        parameter.GetCustomAttributes<JsonDerivedTypeAttribute>().Select(selector: static derived => derived.TypeDiscriminator?.ToString()).ToArray() is { Length: > 0 } discriminants
            ? $"json union: $type in [{string.Join(separator: '|', value: discriminants)}]"
            : parameter == typeof(string) ? "string" : parameter.Name;

    private static JsonObject ParameterNode(ParameterInfo parameter) => new() {
        ["name"] = JsonNamingPolicy.CamelCase.ConvertName(name: parameter.Name ?? string.Empty),
        ["shape"] = Shape(parameter: parameter.ParameterType),
    };

    private static JsonObject VerbNode(Type @case) => new() {
        ["verb"] = JsonNamingPolicy.CamelCase.ConvertName(name: @case.Name),
        ["args"] = new JsonArray([.. @case.GetConstructors()[0].GetParameters().Select(selector: static parameter => (JsonNode)ParameterNode(parameter: parameter))]),
    };
}

// --- [ENTRY] ------------------------------------------------------------------------------

// Ownership: terminal process collapse; stdout carries one envelope, stderr diagnostics, and
// status rows derive exit codes.
internal static class Program {
    internal static async Task<int> Main(string[] args) {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help") {
            await Console.Out.WriteLineAsync(value: Verbs.Help()).ConfigureAwait(false);
            return 0;
        }
        Fin<SupervisorVerb> parsed = Verbs.Parse(argv: args);
        if (parsed is not Fin<SupervisorVerb>.Succ(SupervisorVerb verb)) {
            Diagnose(@event: "argv.rejected", detail: parsed is Fin<SupervisorVerb>.Fail(Error rejection) ? rejection : null);
            await Console.Out.WriteLineAsync(value: Verbs.Help()).ConfigureAwait(false);
            return Verbs.UsageExitCode;
        }
        using CancellationTokenSource interrupt = new();
        SupervisorRuntime runtime = Compose(root: interrupt.Token);
        // assay kills the supervisor group SIGTERM -> 1.0s -> SIGKILL; a blocked await never reaches the
        // RunAsync finally in that window, so the signal callback drives lease release, endpoint poison,
        // and orphan-host kill synchronously and idempotently. ProcessExit is the parent-death backstop;
        // SIGKILL stays uncatchable, covered by the dead-pid lease reclaim on the next acquire.
        using PosixSignalRegistration sigterm = PosixSignalRegistration.Create(signal: PosixSignal.SIGTERM, handler: ctx => Quench(ctx: ctx, interrupt: interrupt, runtime: runtime));
        using PosixSignalRegistration sigint = PosixSignalRegistration.Create(signal: PosixSignal.SIGINT, handler: ctx => Quench(ctx: ctx, interrupt: interrupt, runtime: runtime));
        void OnExit(object? sender, EventArgs args) => _ = Shutdown.Drive(runtime: runtime, reason: "process-exit");
        EventHandler onExit = OnExit;
        AppDomain.CurrentDomain.ProcessExit += onExit;
        SessionEnvelope envelope;
        try {
            envelope = await SessionKernel.RunAsync(verb: verb, runtime: runtime).ConfigureAwait(false);
        } catch (Exception failure) when (failure is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            envelope = SessionFold.Run(
                runId: Guid.NewGuid().ToString(format: "n"), verb: verb,
                final: new SessionState.Faulted(
                    Fault: new BridgeFault.LaunchFailed(Detail: $"{failure.GetType().Name}: {failure.Message}"),
                    At: verb.EntryPhase, Done: Seq<ScenarioReceipt>()),
                stream: Seq<BridgeEvent>(), spoolTail: (0L, 0L), reportDir: runtime.ArtifactRoot);
        }
        // A normal terminal envelope means RunAsync already released the lease; detach the exit backstop
        // so it cannot re-fire against teardown state after the process completes cleanly.
        AppDomain.CurrentDomain.ProcessExit -= onExit;
        await Console.Out.WriteLineAsync(value: JsonSerializer.Serialize(value: envelope, jsonTypeInfo: BridgeJsonContext.Default.SessionEnvelope)).ConfigureAwait(false);
        Diagnose(@event: "session.terminal", detail: null, envelope: envelope);
        return envelope.Status.ExitCode;
    }

    // The signal callback runs inside the ~1s SIGTERM grace: drive teardown synchronously first, then
    // cancel the interrupt so any in-flight await that can still unwind does, and let .NET terminate.
    private static void Quench(PosixSignalContext ctx, CancellationTokenSource interrupt, SupervisorRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: ctx);
        _ = Shutdown.Drive(runtime: runtime, reason: ctx.Signal.ToString());
        if (!interrupt.IsCancellationRequested)
            interrupt.Cancel();
    }

    private static SupervisorRuntime Compose(CancellationToken root) {
        // Discovery misses degrade to the narrowed path stem so status can name the precondition.
        string appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH") ?? "/Applications/RhinoWIP.app";
        string stem = Path.GetFileNameWithoutExtension(path: appPath);
        BundleInfo bundle = BundleInfo.Discover(toolDeadline: SessionPolicy.Default.ToolDeadline) is Fin<BundleInfo>.Succ(BundleInfo discovered)
            ? discovered
            : new BundleInfo(AppPath: appPath, CFBundleName: stem, CFBundleExecutable: stem, CFBundleVersion: string.Empty);
        return new SupervisorRuntime(
            Lease: Atom(value: Option<LeaseToken>.None),
            LiveHostPid: Atom(value: Option<int>.None),
            Clock: TimeProvider.System,
            Root: root,
            Policy: SessionPolicy.Default,
            ArtifactRoot: Path.Combine(Environment.CurrentDirectory, ".artifacts", "assay", "bridge"),
            LeasePath: Lease.CanonicalPath,
            JournalPath: QuitJournal.CanonicalPath,
            Bundle: bundle);
    }

    private static void Diagnose(string @event, Error? detail, SessionEnvelope? envelope = null) =>
        Console.Error.WriteLine(value: new JsonObject {
            ["event"] = @event,
            ["detail"] = detail?.Message,
            ["runId"] = envelope?.RunId,
            ["status"] = envelope?.Status.Key,
            ["exit"] = envelope?.Status.ExitCode,
        }.ToJsonString());
}
