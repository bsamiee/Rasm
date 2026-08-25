using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
    Atom<Option<LeaseToken>> Lease, Atom<Option<int>> LiveHostPid, TimeProvider Clock, SessionPolicy Policy,
    string ArtifactRoot, string CargoSourceRoot, string LeasePath, string JournalPath, BundleInfo Bundle, CancellationToken Root);

// --- [OPERATIONS] ----------------------------------------------------------------------

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
            ["verify", { } selection] => Selection(raw: selection)
                .Map(f: SupervisorVerb (admitted) => new SupervisorVerb.Verify(Selection: admitted)),
            ["status"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Status()),
            ["quit"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Quit()),
            _ => Fin.Fail<SupervisorVerb>(error: Error.New(message: "unrecognized invocation: verify <selection-json> | status | quit")),
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
            : parameter == typeof(string) ? "string" : SmartEnumShape(parameter: parameter) ?? parameter.Name;

    private static string? SmartEnumShape(Type parameter) =>
        parameter.GetProperty(name: "Items", bindingAttr: BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null) is System.Collections.IEnumerable items
            ? $"key in [{string.Join(separator: '|', values: items.Cast<object>().Select(selector: static item => item.ToString() ?? string.Empty))}]"
            : null;

    private static JsonObject ParameterNode(ParameterInfo parameter) => new() {
        ["name"] = JsonNamingPolicy.CamelCase.ConvertName(name: parameter.Name ?? string.Empty),
        ["shape"] = Shape(parameter: parameter.ParameterType),
    };

    private static JsonObject VerbNode(Type @case) => new() {
        ["verb"] = JsonNamingPolicy.CamelCase.ConvertName(name: @case.Name),
        ["args"] = new JsonArray([.. @case.GetConstructors()[0].GetParameters().Select(selector: static parameter => (JsonNode)ParameterNode(parameter: parameter))]),
    };
}

// --- [ENTRY] ---------------------------------------------------------------------------

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
                    At: verb.EntryPhase, Done: Seq<ScenarioOutcome>()),
                stream: Seq<BridgeEvent>(), spoolTail: (0L, 0L), reportDir: runtime.ArtifactRoot);
        }
        AppDomain.CurrentDomain.ProcessExit -= onExit;
        await Console.Out.WriteLineAsync(value: JsonSerializer.Serialize(value: envelope, jsonTypeInfo: BridgeJsonContext.Default.SessionEnvelope)).ConfigureAwait(false);
        Diagnose(@event: "session.terminal", detail: null, envelope: envelope);
        return envelope.Status.ExitCode;
    }

    private static void Quench(PosixSignalContext ctx, CancellationTokenSource interrupt, SupervisorRuntime runtime) {
        ArgumentNullException.ThrowIfNull(argument: ctx);
        _ = Shutdown.Drive(runtime: runtime, reason: ctx.Signal.ToString());
        if (!interrupt.IsCancellationRequested)
            interrupt.Cancel();
    }

    private static SupervisorRuntime Compose(CancellationToken root) {
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
            ArtifactRoot: Path.Combine(Environment.CurrentDirectory, ".artifacts", "dotnet", "bridge"),
            CargoSourceRoot: AppContext.BaseDirectory,
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
