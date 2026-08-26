using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ---------------------------------------------------------------------------

[Union]
internal abstract partial record SupervisorVerb {
    private SupervisorVerb() { }
    internal sealed record Verify(ScenarioSelection Selection) : SupervisorVerb;
    internal sealed record Status : SupervisorVerb;
    internal sealed record Quit : SupervisorVerb;

    public string Key => Switch(
        verify: static _ => "verify",
        status: static _ => "status",
        quit: static _ => "quit");

    public SessionPhase EntryPhase => Switch(
        verify: static _ => SessionPhase.Launch,
        status: static _ => SessionPhase.Status,
        quit: static _ => SessionPhase.QuitAe);
}

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record SupervisorRuntime(
    Atom<Option<Lease.Token>> Held, Atom<Option<int>> LiveHostPid, TimeProvider Clock, SessionPolicy Policy,
    string ArtifactRoot, string PayloadRoot, string LeasePath, string JournalPath,
    string AutosaveDirectory, string DiagnosticReportsDirectory, Seq<string> CrashBaseline, BundleInfo Bundle, CancellationToken Root);

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Verbs {
    internal const int UsageExitCode = 2;

    internal static string Help() =>
        new JsonObject {
            ["tool"] = "rasm-bridge-supervisor",
            ["stdout"] = "one SessionEnvelope JSON document",
            ["verbs"] = new JsonArray(
                new JsonObject { ["verb"] = "verify", ["args"] = new JsonArray(new JsonObject { ["name"] = "selection", ["shape"] = "json union: $type in [all|themes|names]" }) },
                new JsonObject { ["verb"] = "status", ["args"] = new JsonArray() },
                new JsonObject { ["verb"] = "quit", ["args"] = new JsonArray() }),
            ["exitCodes"] = new JsonObject(PhaseStatus.Items
                .Select(static status => KeyValuePair.Create<string, JsonNode?>(status.Key, status.ExitCode))
                .Append(KeyValuePair.Create<string, JsonNode?>("usage", UsageExitCode))),
        }.ToJsonString();

    internal static Fin<SupervisorVerb> Parse(string[] argv) {
        ArgumentNullException.ThrowIfNull(argv);
        return argv switch {
            ["verify", { } selection] => Selection(selection).Map(SupervisorVerb (admitted) => new SupervisorVerb.Verify(admitted)),
            ["status"] => new SupervisorVerb.Status(),
            ["quit"] => new SupervisorVerb.Quit(),
            _ => Error.New("unrecognized invocation: verify <selection-json> | status | quit"),
        };
    }

    private static Fin<ScenarioSelection> Selection(string raw) {
        try {
            return Optional(JsonSerializer.Deserialize(raw, BridgeJsonContext.Default.ScenarioSelection)).ToFin(Error.New("selection decoded to null"));
        } catch (JsonException decode) {
            return Error.New($"selection is not a ScenarioSelection union document: {decode.Message}");
        }
    }
}

// --- [ENTRY] ---------------------------------------------------------------------------

internal static class Program {
    private const string ReportsRootMetadata = "RasmBridgeReportsRoot";
    private const string PayloadDirectory = "payload";

    internal static async Task<int> Main(string[] args) {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help") {
            await Console.Out.WriteLineAsync(Verbs.Help()).ConfigureAwait(false);
            return 0;
        }
        Fin<SupervisorVerb> parsed = Verbs.Parse(args);
        if (parsed is not Fin<SupervisorVerb>.Succ(SupervisorVerb verb)) {
            Diagnose("argv.rejected", parsed.Match(static _ => string.Empty, static error => error.Message));
            await Console.Out.WriteLineAsync(Verbs.Help()).ConfigureAwait(false);
            return Verbs.UsageExitCode;
        }
        using CancellationTokenSource interrupt = new();
        string reportsRoot = ReportsRoot();
        Fin<BundleInfo> discovery = BundleInfo.Discover(SessionPolicy.Default.ToolDeadline);
        if (discovery is not Fin<BundleInfo>.Succ(BundleInfo bundle)) {
            return await CompleteAsync(Rejected(verb, discovery.Match(static _ => string.Empty, static error => error.Message), reportsRoot)).ConfigureAwait(false);
        }
        SupervisorRuntime runtime = Compose(reportsRoot, bundle, interrupt.Token);
        using PosixSignalRegistration sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx => Quench(ctx, interrupt, runtime));
        using PosixSignalRegistration sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx => Quench(ctx, interrupt, runtime));
        void OnExit(object? sender, EventArgs args) => _ = Shutdown.Drive(runtime, "process-exit");
        AppDomain.CurrentDomain.ProcessExit += OnExit;
        SessionEnvelope envelope;
        try {
            envelope = await new SessionRun(verb, runtime).RunAsync().ConfigureAwait(false);
        } catch (Exception failure) when (failure is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            envelope = Rejected(verb, $"{failure.GetType().Name}: {failure.Message}", runtime.ArtifactRoot);
        }
        AppDomain.CurrentDomain.ProcessExit -= OnExit;
        return await CompleteAsync(envelope).ConfigureAwait(false);
    }

    private static SessionEnvelope Rejected(SupervisorVerb verb, string detail, string reportDir) =>
        SessionFold.Run(
            Guid.NewGuid().ToString("n"), verb,
            new SessionState.Faulted(new BridgeFault.LaunchFailed(detail), verb.EntryPhase, Seq<ScenarioOutcome>()),
            Seq<BridgeEvent>(), (0L, 0L), reportDir);

    private static void Quench(PosixSignalContext ctx, CancellationTokenSource interrupt, SupervisorRuntime runtime) {
        _ = Shutdown.Drive(runtime, ctx.Signal.ToString());
        if (!interrupt.IsCancellationRequested) {
            interrupt.Cancel();
        }
    }

    private static SupervisorRuntime Compose(string reportsRoot, BundleInfo bundle, CancellationToken root) {
        string diagnosticReports = Path.Combine(BundleInfo.UserLibraryDirectory, "Logs", "DiagnosticReports");
        return new SupervisorRuntime(
            Held: Atom(Option<Lease.Token>.None),
            LiveHostPid: Atom(Option<int>.None),
            Clock: TimeProvider.System,
            Policy: SessionPolicy.Default,
            ArtifactRoot: reportsRoot,
            PayloadRoot: Path.Combine(AppContext.BaseDirectory, PayloadDirectory),
            LeasePath: Lease.CanonicalPath,
            JournalPath: QuitJournal.CanonicalPath,
            AutosaveDirectory: Path.Combine(BundleInfo.UserLibraryDirectory, "Autosave Information"),
            DiagnosticReportsDirectory: diagnosticReports,
            CrashBaseline: Reconcile.Reports(diagnosticReports, bundle.CrashReportPattern),
            Bundle: bundle,
            Root: root);
    }

    private static string ReportsRoot() =>
        typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(static metadata => string.Equals(metadata.Key, ReportsRootMetadata, StringComparison.Ordinal))
            .Value is { Length: > 0 } configured
                ? configured
                : throw new InvalidOperationException($"assembly metadata '{ReportsRootMetadata}' has no path");

    private static async Task<int> CompleteAsync(SessionEnvelope envelope) {
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(envelope, BridgeJsonContext.Default.SessionEnvelope)).ConfigureAwait(false);
        Diagnose("session.terminal", detail: null, envelope);
        return envelope.ExitCode;
    }

    private static void Diagnose(string @event, string? detail, SessionEnvelope? envelope = null) =>
        Console.Error.WriteLine(new JsonObject {
            ["event"] = @event,
            ["detail"] = detail,
            ["runId"] = envelope?.RunId,
            ["status"] = envelope?.Status.Overall.Key,
            ["exit"] = envelope?.ExitCode,
        }.ToJsonString());
}
