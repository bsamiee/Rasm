using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Supervisor;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: argv -> SupervisorVerb [Union] -> Eff<SupervisorRuntime, SessionEnvelope> -> ONE
// terminal collapse at Main. `verify` is modality-polymorphic on input shape: assay passes a
// ScenarioSelection union case + the staged closure manifest path; no flags, no modes. Key and
// EntryPhase are derived projections — help, report routing, and fault attribution all read this
// single declaration.
[Union]
internal abstract partial record SupervisorVerb {
    private SupervisorVerb() { }
    internal sealed record Verify(ScenarioSelection Selection, string ClosureManifest) : SupervisorVerb;
    internal sealed record Doctor : SupervisorVerb;
    internal sealed record Redeploy(string PackagePath) : SupervisorVerb;
    internal sealed record Quit : SupervisorVerb;

    public string Key => Switch(
        verify: static _ => "verify",
        doctor: static _ => "doctor",
        redeploy: static _ => "redeploy",
        quit: static _ => "quit");

    public SessionPhase EntryPhase => Switch(
        verify: static _ => SessionPhase.Launch,
        doctor: static _ => SessionPhase.Doctor,
        redeploy: static _ => SessionPhase.Install,
        quit: static _ => SessionPhase.QuitAe);
}

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: the composition-edge dependency surface, constructed once at Main. Schedule values
// come from Policy rows, never call-site literals; the ~/.rasm lease + quit-journal paths are
// composed here once so fault-injection harnesses can re-root them.
internal sealed record SupervisorRuntime(
    Atom<Option<LeaseToken>> Lease, TimeProvider Clock, SessionPolicy Policy,
    string ArtifactRoot, string LeasePath, string JournalPath, BundleInfo Bundle, CancellationToken Root);

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: the verb surface — argv admission, runtime-rendered help, and the per-verb session
// pipelines. Help derives from the SupervisorVerb union metadata and the PhaseStatus rows at
// runtime: one declaration, zero generated artifacts to drift.
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
                .Map(f: SupervisorVerb (admitted) => new SupervisorVerb.Verify(Selection: admitted, ClosureManifest: manifest)),
            ["doctor"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Doctor()),
            ["redeploy", { } package] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Redeploy(PackagePath: package)),
            ["quit"] => Fin.Succ<SupervisorVerb>(value: new SupervisorVerb.Quit()),
            _ => Fin.Fail<SupervisorVerb>(error: Error.New(message: "unrecognized invocation: verify <selection-json> <closure-manifest> | doctor | redeploy <package> | quit")),
        };
    }

    internal static Eff<SupervisorRuntime, SessionEnvelope> Session(SupervisorVerb verb) {
        ArgumentNullException.ThrowIfNull(argument: verb);
        return Eff<SupervisorRuntime, SessionEnvelope>.Lift(f: runtime => Fin.Succ(value: SessionKernel.Run(verb: verb, runtime: runtime)));
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

// Ownership: the one terminal collapse. stdout carries exactly one JSON document (the assay
// decode contract); structlog-style diagnostics ride stderr; the exit code derives from
// PhaseStatus rows.
internal static class Program {
    internal static int Main(string[] args) {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help") {
            Console.Out.WriteLine(value: Verbs.Help());
            return 0;
        }
        Fin<SupervisorVerb> parsed = Verbs.Parse(argv: args);
        if (parsed is not Fin<SupervisorVerb>.Succ(SupervisorVerb verb)) {
            Diagnose(@event: "argv.rejected", detail: parsed is Fin<SupervisorVerb>.Fail(Error rejection) ? rejection : null);
            Console.Out.WriteLine(value: Verbs.Help());
            return Verbs.UsageExitCode;
        }
        using CancellationTokenSource interrupt = new();
        SupervisorRuntime runtime = Compose(root: interrupt.Token);
        Fin<SessionEnvelope> outcome = Verbs.Session(verb: verb).Run(runtime);
        SessionEnvelope envelope = outcome switch {
            Fin<SessionEnvelope>.Succ(SessionEnvelope folded) => folded,
            Fin<SessionEnvelope>.Fail(Error failure) => SessionFold.Run(
                runId: Guid.NewGuid().ToString(format: "n"), verb: verb,
                final: new SessionState.Faulted(
                    Fault: new BridgeFault.LaunchFailed(Detail: failure.Message),
                    At: verb.EntryPhase, Done: Seq<ScenarioReceipt>()),
                stream: Seq<BridgeEvent>(), spoolTail: (0L, 0L), reportDir: runtime.ArtifactRoot),
            _ => throw new InvalidOperationException(message: "Fin produced neither value nor error"),
        };
        Console.Out.WriteLine(value: JsonSerializer.Serialize(value: envelope, jsonTypeInfo: BridgeJsonContext.Default.SessionEnvelope));
        Diagnose(@event: "session.terminal", detail: null, envelope: envelope);
        return envelope.Status.ExitCode;
    }

    private static SupervisorRuntime Compose(CancellationToken root) {
        // CFBundle discovery (env narrows, never required); a discovery miss degrades to the
        // path-derived stem so doctor can still report the named missing precondition.
        string appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH") ?? "/Applications/RhinoWIP.app";
        string stem = Path.GetFileNameWithoutExtension(path: appPath);
        BundleInfo bundle = BundleInfo.Discover(toolDeadline: SessionPolicy.Default.ToolDeadline) is Fin<BundleInfo>.Succ(BundleInfo discovered)
            ? discovered
            : new BundleInfo(AppPath: appPath, CFBundleName: stem, CFBundleExecutable: stem, CFBundleVersion: string.Empty);
        return new SupervisorRuntime(
            Lease: Atom(value: Option<LeaseToken>.None),
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
