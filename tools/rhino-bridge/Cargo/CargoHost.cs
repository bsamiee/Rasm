using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Rasm.Bridge.Contract;
using Rasm.TestKit.Scenarios;
using Rhino;
using Rhino.Display;
using Rhino.Runtime;

namespace Rasm.Bridge.Cargo;

// --- [TYPES] --------------------------------------------------------------------------------

// Ownership: the M1 capability vocabulary — one row per optional lane, each row carrying its
// probe as a constructor delegate (the row owns its behavior; no parallel probe table). A live
// row asserts the lane's actual behavior against the running host, never a version string.
// Recorded Phase-0 verdicts ride their rows verbatim (0a hot-swap pinned, 0b dataflow blocked):
// re-deriving 0b live would block the UI-thread probe bracket the rows run inside — the exact
// deadlock the recorded verdict documents. Requires-keys without a row degrade through the
// lattice in CargoHost.Run, never here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class HostCapability {
    public static readonly HostCapability CargoHotswap = new(
        key: "cargo.hotswap",
        probe: static _ => (PhaseStatus.Unsupported,
            "probe 0a: collectible-ALC unload stays pinned in the live host; recovery is a supervised host recycle behind the same IBridgeCargo seam"));
    public static readonly HostCapability EventPipe = new(key: "eventpipe", probe: static _ => CargoHost.ProbeEventPipe());
    public static readonly HostCapability ExceptionTap = new(key: "exception.tap", probe: static _ => CargoHost.ProbeExceptionTap());
    public static readonly HostCapability Gh2Dataflow = new(
        key: "gh2.dataflow",
        probe: static _ => (PhaseStatus.Unsupported,
            "probe 0b: Start(SolutionMode.Headless) is blocked under the execute lane on this build; GH2 stays render-only"));
    public static readonly HostCapability Gh2Render = new(key: "gh2.render", probe: static host => host.ProbeRender());

    private readonly Func<CargoHost, (PhaseStatus Outcome, string Receipt)> probe;

    internal CapabilityEntry Probe(CargoHost host) {
        // BOUNDARY ADAPTER — a throwing probe is attributed to its own row (tolerance M1), never
        // to the first scenario after it.
        try {
            (PhaseStatus outcome, string receipt) = probe(host);
            return new CapabilityEntry(Key: Key, Outcome: outcome, Receipt: receipt);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return new CapabilityEntry(Key: Key, Outcome: PhaseStatus.Failed, Receipt: $"probe threw {error.GetType().Name}: {error.Message}");
        }
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the IBridgeCargo implementation activated by CargoGate inside the collectible ALC —
// in-host post-load discovery over staged assemblies carrying [RhinoScenario], the capability probe bracket,
// the requires-lattice, the host-drift floor, and the scenario IO bracket (acquire context ->
// invoke entrypoint -> release; the finally rung flushes the spool footer). Every event is
// double-folded: cargo-stamped into the crash-durable spool AND relayed through the shell-owned
// publish delegate, which re-stamps the session-global sequence while preserving the Scenario
// slot. CargoManifest arrives at activation: SessionId feeds the stamps, ReportDir roots the
// spool and capture writers. Statics are banned in cargo outside Gh2Lane; the SDK's Capture hook
// is bound per run and cleared on both the run bracket and Dispose (D-2 precondition).
public sealed class CargoHost : IBridgeCargo {
    private const string ProbeSlot = "probe";
    private const int CommandEvidenceTail = 4096;
    private const string ScenarioAttributeType = "Rasm.TestKit.Scenarios.RhinoScenarioAttribute";

    private readonly Lock sync = new();
    private readonly CargoManifest manifest;
    private Seq<(ScenarioEntry Entry, MethodInfo Method)> corpus;
    private Seq<CapabilityEntry> capabilities;
    private Seq<(string Key, string Value)> discoveryFacts;
    private Gh2Lane? lane;
    private long sequence;
    private bool scanned;

    public CargoHost(CargoManifest manifest) {
        ArgumentNullException.ThrowIfNull(argument: manifest);
        this.manifest = manifest;
    }

    public ScenarioEntry[] Discover() => [.. Scan().Map(f: static row => row.Entry)];

    public CapabilityEntry[] Probe(Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(argument: publish);
        long started = Stopwatch.GetTimestamp();
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: ProbeSlot);
        CapabilityEntry[] report = [.. HostCapability.Items.Select(selector: row => Probed(row: row, spool: spool, publish: publish))];
        foreach ((string key, string value) in discoveryFacts) {
            Emit(spool: spool, publish: publish, evt: Fact(key: key, value: value, scenario: ProbeSlot));
        }
        lock (sync) {
            capabilities = toSeq(report);
        }
        double wallMs = Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds;
        Emit(spool: spool, publish: publish, evt: Fact(key: "probe.wallMs", value: wallMs, scenario: ProbeSlot));
        Emit(spool: spool, publish: publish, evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.Probe, Status: PhaseStatus.Ok, DurationMs: wallMs, Fault: null) { Stamp = NextStamp(scenario: ProbeSlot) });
        return report;
    }

    public ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(argument: publish);
        long started = Stopwatch.GetTimestamp();
        object? candidate = Scan()
            .Filter(f: row => string.Equals(a: row.Entry.Name, b: scenario.Name, comparisonType: StringComparison.Ordinal))
            .Map(f: static row => row.Method)
            .Head.Case;
        return FirstUnmet(requires: scenario.Requires) is { } gap
            ? Refuse(scenario: scenario, gap: gap, publish: publish, started: started)
            : candidate is MethodInfo entry
                ? Execute(scenario: scenario, entry: entry, publish: publish, started: started)
                : Vanish(scenario: scenario, publish: publish, started: started);
    }

    public void Dispose() {
        // D-2 unload precondition: the ambient SDK hook is cleared (the one cargo-reachable
        // static outside Gh2Lane), per-run scopes were drained by each run bracket, and the lane
        // drops its GH2 references (the editor singleton is host-owned and survives the swap).
        Capture.Hook = null;
        lock (sync) {
            lane?.Dispose();
            lane = null;
        }
    }

    // --- [PROBES] -----------------------------------------------------------------------------

    internal static (PhaseStatus Outcome, string Receipt) ProbeEventPipe() {
        // Behavior assertion, not a recorded verdict: the diagnostic socket for THIS host pid
        // either listens or it does not (probe 0c corroborated live each session).
        string socketRoot = Path.GetTempPath();
        string pattern = string.Create(provider: CultureInfo.InvariantCulture, $"dotnet-diagnostic-{Environment.ProcessId}-*");
        return Directory.EnumerateFiles(path: socketRoot, searchPattern: pattern).Any()
            ? (PhaseStatus.Ok, $"diagnostic socket live under {socketRoot} (probe 0c: EventPipe admitted; MDNC rides the supervisor)")
            : (PhaseStatus.Unsupported, $"no {pattern} socket under {socketRoot}: EventPipe disabled on this host");
    }

    internal static (PhaseStatus Outcome, string Receipt) ProbeExceptionTap() {
        static void Tap(string source, Exception ex) {
        }
        HostUtils.OnExceptionReport += Tap;
        HostUtils.OnExceptionReport -= Tap;
        return (PhaseStatus.Ok, "OnExceptionReport subscription took; the shell tap is the live wire");
    }

    internal (PhaseStatus Outcome, string Receipt) ProbeRender() {
        long started = Stopwatch.GetTimestamp();
        return AcquireLane().Bind(f: live => live.DrawCanvas(path: Path.Combine(path1: manifest.ReportDir, path2: "probe.gh2-render.png"))) switch {
            Fin<CaptureFile>.Succ(CaptureFile file) => (PhaseStatus.Ok,
                string.Create(provider: CultureInfo.InvariantCulture, $"DrawToBitmap {file.Width}x{file.Height} in {Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds:F0}ms via the ctor-never-Show editor")),
            Fin<CaptureFile>.Fail(Error error) => (PhaseStatus.Unsupported, $"render lane unavailable: {error.Message}"),
            _ => (PhaseStatus.Failed, "render probe unresolved"),
        };
    }

    // --- [BRACKET] ----------------------------------------------------------------------------

    private ScenarioReceipt Refuse(ScenarioEntry scenario, CapabilityEntry gap, Action<BridgeEvent> publish, long started) {
        // Requires-lattice (tolerance M4): unmet capability -> Unsupported with the probe receipt
        // attached; the entrypoint is never invoked.
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        BridgeFault fault = new BridgeFault.CapabilityAbsent(Capability: gap.Key, ProbeReceipt: gap.Receipt);
        double duration = Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds;
        Emit(spool: spool, publish: publish, evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Unsupported, DurationMs: duration, Fault: fault) { Stamp = NextStamp(scenario: scenario.Name) });
        return new ScenarioReceipt(Scenario: scenario.Name, Status: PhaseStatus.Unsupported, DurationMs: duration, Fault: fault);
    }

    private ScenarioReceipt Vanish(ScenarioEntry scenario, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        Emit(spool: spool, publish: publish, evt: Fact(key: "scenario.missing", value: $"'{scenario.Name}' not present in staged assemblies carrying [RhinoScenario]", scenario: scenario.Name));
        double duration = Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds;
        Emit(spool: spool, publish: publish, evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Failed, DurationMs: duration, Fault: null) { Stamp = NextStamp(scenario: scenario.Name) });
        return new ScenarioReceipt(Scenario: scenario.Name, Status: PhaseStatus.Failed, DurationMs: duration, Fault: null);
    }

    private ScenarioReceipt Execute(ScenarioEntry scenario, MethodInfo entry, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        void emit(BridgeEvent evt) => Emit(spool: spool, publish: publish, evt: evt);
        void fact(string key, object? value) => emit(Fact(key: key, value: value, scenario: scenario.Name));
        ScenarioContext? context = null;
        PhaseStatus status = PhaseStatus.Failed;
        BridgeFault? fault = null;
        bool commandCaptureWasEnabled = false;
        bool commandCaptureAdmitted = false;
        string commandCaptureFailure = string.Empty;
        try {
            commandCaptureWasEnabled = RhinoApp.CommandWindowCaptureEnabled;
            RhinoApp.CommandWindowCaptureEnabled = true;
            _ = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
            commandCaptureAdmitted = true;
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            commandCaptureFailure = $"{error.GetType().Name}: {error.Message}";
        }
        try {
            if (RhinoDoc.ActiveDoc is { } doc) {
                context = new ScenarioContext(doc: doc, sink: fact);
                Capture.Hook = label => doc.Views.ActiveView is { } view
                    ? Shoot(spool: spool, view: view, scenario: scenario.Name, label: label, onFailure: false, emit: emit, fact: fact)
                    : Fin.Fail<string>(error: Error.New(message: "Capture.Snapshot: no active viewport"));
                (status, fault) = Invoke(entry: entry, context: context, fact: fact);
                if (context.FactCount == 0) {
                    fact(key: "facts.empty", value: "scenario emitted zero facts");
                }
                if (status != PhaseStatus.Ok) {
                    AutoCapture(spool: spool, context: context, scenario: scenario, emit: emit, fact: fact);
                }
            } else {
                fact(key: "scenario.doc.absent", value: "no active document in the host");
            }
        } finally {
            // Port obligation 7 (belt): the footer rides the same WriteThrough spool; the
            // per-line append above is the structural crash-durability.
            Capture.Hook = null;
            if (context is { } scenarioContext) {
                int leaked = scenarioContext.DrainScopes();
                if (leaked > 0) {
                    fact(key: "scope.leaked", value: leaked);
                }
            }
            if (spool.Failures > 0) {
                publish(Fact(key: "spool.degraded", value: spool.Failures, scenario: scenario.Name));
            }
            try {
                string[] captured = commandCaptureAdmitted ? RhinoApp.CapturedCommandWindowStrings(clearBuffer: true) : [];
                string capturedText = string.IsNullOrEmpty(value: commandCaptureFailure)
                    ? string.Join(separator: Environment.NewLine, values: captured)
                    : commandCaptureFailure;
                string history = RhinoApp.CommandHistoryWindowText ?? string.Empty;
                fact(key: "command.capture.enabled", value: commandCaptureAdmitted);
                fact(key: "command.capture.count", value: captured.Length);
                fact(key: "command.capture.tail", value: capturedText.Length <= CommandEvidenceTail ? capturedText : capturedText[^CommandEvidenceTail..]);
                fact(key: "command.history.length", value: history.Length);
                fact(key: "command.history.tail", value: history.Length <= CommandEvidenceTail ? history : history[^CommandEvidenceTail..]);
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                string detail = $"{error.GetType().Name}: {error.Message}";
                fact(key: "command.capture.enabled", value: false);
                fact(key: "command.capture.count", value: 0);
                fact(key: "command.capture.tail", value: detail.Length <= CommandEvidenceTail ? detail : detail[^CommandEvidenceTail..]);
                fact(key: "command.history.length", value: 0);
                fact(key: "command.history.tail", value: string.Empty);
            } finally {
                if (commandCaptureAdmitted) {
                    try {
                        RhinoApp.CommandWindowCaptureEnabled = commandCaptureWasEnabled;
                    } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                        fact(key: "command.capture.restore.failed", value: $"{error.GetType().Name}: {error.Message}");
                    }
                }
            }
            emit(new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: status, DurationMs: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds, Fault: fault) { Stamp = NextStamp(scenario: scenario.Name) });
        }
        return new ScenarioReceipt(Scenario: scenario.Name, Status: status, DurationMs: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds, Fault: fault);
    }

    private (PhaseStatus Status, BridgeFault? Fault) Invoke(MethodInfo entry, ScenarioContext context, Action<string, object?> fact) {
        // BOUNDARY ADAPTER — the tool's one reflective dispatch. The host-drift floor (M4d):
        // loader exceptions at invoke classify as HostDrift carrying both fingerprints; every
        // other throw is the scenario's own failure, captured as facts, never re-thrown.
        try {
            return entry.Invoke(obj: null, parameters: [context]) switch {
                Fin<Unit>.Succ => (PhaseStatus.Ok, null),
                Fin<Unit>.Fail(Error failure) => Failed(fact: fact, key: "scenario.failure", detail: failure.Message),
                _ => Failed(fact: fact, key: "scenario.entry.shape", detail: $"'{entry.Name}' returned a non-Fin<Unit> value; the entrypoint shape is static Fin<Unit> (ScenarioContext)"),
            };
        } catch (Exception error) when (DriftMember(error: error) is { } member) {
            fact("scenario.drift", member);
            return (PhaseStatus.Failed, new BridgeFault.HostDrift(MissingMember: member, BuiltAgainst: manifest.BuiltAgainst, Running: RunningFingerprint()));
        } catch (TargetInvocationException wrapped) when (wrapped.InnerException is { } inner) {
            return Failed(fact: fact, key: "scenario.exception", detail: $"{inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Failed(fact: fact, key: "scenario.exception", detail: $"{error.GetType().Name}: {error.Message}");
        }
    }

    private void AutoCapture(Spool spool, ScenarioContext context, ScenarioEntry scenario, Action<BridgeEvent> emit, Action<string, object?> fact) {
        // Capture triggers (features F3, INCLUSIVE): a live DocumentScope with a realized
        // viewport shoots the viewport; a gh2.* requirement shoots the canvas. Captures are
        // evidence, never oracles.
        if (context.RealizedView is { } view) {
            _ = Shoot(spool: spool, view: view, scenario: scenario.Name, label: null, onFailure: true, emit: emit, fact: fact);
        }
        if (scenario.Requires.Any(predicate: static key => key.StartsWith(value: "gh2.", comparisonType: StringComparison.Ordinal))) {
            string path = Path.Combine(path1: manifest.ReportDir, path2: scenario.Name + ".canvas.png");
            Fin<CaptureFile> shot = AcquireLane().Bind(f: live => live.DrawCanvas(path: path));
            if (shot is Fin<CaptureFile>.Succ(CaptureFile file)) {
                emit(new BridgeEvent.CaptureCase(Path: file.Path, Width: file.Width, Height: file.Height, OnFailure: true) { Stamp = NextStamp(scenario: scenario.Name) });
            } else if (shot is Fin<CaptureFile>.Fail(Error error)) {
                fact("capture.canvas.failed", error.Message);
            }
        }
    }

    private Fin<string> Shoot(Spool spool, RhinoView view, string scenario, string? label, bool onFailure, Action<BridgeEvent> emit, Action<string, object?> fact) {
        // The capture-metadata convention: camera, frame, and object-count facts ride WITH the
        // capture event so an empty image is diagnosable without re-shooting.
        Fin<BridgeEvent.CaptureCase> shot = spool.Capture(view: view, label: label, onFailure: onFailure);
        if (shot is Fin<BridgeEvent.CaptureCase>.Succ(BridgeEvent.CaptureCase capture)) {
            emit(capture with { Stamp = NextStamp(scenario: scenario) });
            RhinoViewport viewport = view.ActiveViewport;
            fact("capture.camera.location", viewport.CameraLocation);
            fact("capture.camera.target", viewport.CameraTarget);
            fact("capture.frame", string.Create(provider: CultureInfo.InvariantCulture, $"{capture.Width}x{capture.Height}"));
            fact("capture.objects", view.Document.Objects.Count);
            return Fin.Succ(value: capture.Path);
        }
        if (shot is Fin<BridgeEvent.CaptureCase>.Fail(Error error)) {
            fact("capture.failed", error.Message);
            return Fin.Fail<string>(error: error);
        }
        return Fin.Fail<string>(error: Error.New(message: "capture unresolved"));
    }

    // --- [DISCOVERY] --------------------------------------------------------------------------

    private Seq<(ScenarioEntry Entry, MethodInfo Method)> Scan() {
        lock (sync) {
            if (!scanned) {
                AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly: typeof(CargoHost).Assembly) ?? AssemblyLoadContext.Default;
                List<(ScenarioEntry Entry, MethodInfo Method)> entries = [];
                List<(string Key, string Value)> facts = [];
                foreach (string path in ScenarioAssemblyPaths().Order(comparer: StringComparer.Ordinal)) {
                    Assembly? assembly = null;
                    try {
                        assembly = context.LoadFromAssemblyPath(assemblyPath: path);
                    } catch (Exception error) when (error is BadImageFormatException or FileLoadException or FileNotFoundException) {
                        facts.Add(item: ("discovery.assembly.load.failed", $"{Path.GetFileName(path)}: {error.GetType().Name}: {error.Message}"));
                    }
                    if (assembly is null) {
                        continue;
                    }
                    int before = entries.Count;
                    foreach (Type type in TypesOf(assembly: assembly, facts: facts)) {
                        MethodInfo[] methods;
                        try {
                            methods = type.GetMethods(bindingAttr: BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        } catch (Exception error) when (error is TypeLoadException or FileLoadException or FileNotFoundException or NotSupportedException) {
                            facts.Add(item: ("discovery.type.methods.failed", $"{type.FullName}: {error.GetType().Name}: {error.Message}"));
                            continue;
                        }
                        foreach (MethodInfo method in methods) {
                            try {
                                if (EntryOf(method: method) is { } entry) {
                                    entries.Add(item: (entry, method));
                                }
                            } catch (Exception error) when (error is TypeLoadException or FileLoadException or FileNotFoundException or CustomAttributeFormatException) {
                                facts.Add(item: ("discovery.method.attribute.failed", $"{type.FullName}.{method.Name}: {error.GetType().Name}: {error.Message}"));
                            }
                        }
                    }
                    int added = entries.Count - before;
                    if (added > 0) {
                        facts.Add(item: (
                            "discovery.assembly.scenarios",
                            string.Create(provider: CultureInfo.InvariantCulture, $"{assembly.GetName().Name}:{added}")));
                    }
                }
                corpus = toSeq(entries);
                discoveryFacts = toSeq(facts);
                scanned = true;
            }
            return corpus;
        }
    }

    private CapabilityEntry? FirstUnmet(string[] requires) {
        Seq<CapabilityEntry> granted;
        lock (sync) {
            granted = capabilities;
        }
        return requires
            .Select(selector: key => granted.Filter(f: entry => string.Equals(a: entry.Key, b: key, comparisonType: StringComparison.Ordinal)).Head.Case is CapabilityEntry row
                ? row
                : new CapabilityEntry(Key: key, Outcome: PhaseStatus.Unsupported, Receipt: "no capability row on this build"))
            .Where(predicate: static entry => entry.Outcome != PhaseStatus.Ok)
            .Cast<CapabilityEntry?>()
            .FirstOrDefault();
    }

    private Fin<Gh2Lane> AcquireLane() {
        // BOUNDARY ADAPTER — Gh2Lane JITs against Grasshopper2; an absent or unloaded GH2
        // surfaces at this callsite as a loader exception, projected typed (proof 10: every
        // LoadPlugIn outcome reads as the same capability absence).
        lock (sync) {
            try {
                lane ??= Gh2Lane.Acquire();
                return Fin.Succ(value: lane);
            } catch (Exception error) when (error is TypeLoadException or TypeInitializationException or FileNotFoundException or FileLoadException or MissingMethodException or InvalidOperationException) {
                return Fin.Fail<Gh2Lane>(error: Error.New(message: $"{error.GetType().Name}: {error.Message}"));
            }
        }
    }

    private CapabilityEntry Probed(HostCapability row, Spool spool, Action<BridgeEvent> publish) {
        CapabilityEntry entry = row.Probe(host: this);
        Emit(spool: spool, publish: publish, evt: Fact(key: $"capability.{entry.Key}", value: $"{entry.Outcome.Key}: {entry.Receipt}", scenario: ProbeSlot));
        return entry;
    }

    private BridgeEvent.FactCase Fact(string key, object? value, string? scenario) =>
        new(Key: key, Value: Json(value: value)) { Stamp = NextStamp(scenario: scenario) };

    private EventStamp NextStamp(string? scenario) => new(
        SessionId: manifest.SessionId,
        Sequence: Interlocked.Increment(location: ref sequence),
        AtUnixMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Scenario: scenario);

    private static (PhaseStatus Status, BridgeFault? Fault) Failed(Action<string, object?> fact, string key, string detail) {
        fact(key, detail);
        return (PhaseStatus.Failed, null);
    }

    private static string? DriftMember(Exception error) => error switch {
        MissingMethodException or MissingFieldException or TypeLoadException => error.Message,
        TargetInvocationException { InnerException: { } inner } when inner is MissingMethodException or MissingFieldException or TypeLoadException => inner.Message,
        _ => null,
    };

    private static void Emit(Spool spool, Action<BridgeEvent> publish, BridgeEvent evt) {
        spool.Append(evt: evt);
        publish(evt);
    }

    private static ScenarioEntry? EntryOf(MethodInfo method) {
        CustomAttributeData? marker = method.CustomAttributes.FirstOrDefault(predicate: static attribute =>
            string.Equals(a: attribute.AttributeType.FullName, b: ScenarioAttributeType, comparisonType: StringComparison.Ordinal));
        if (marker is null || marker.ConstructorArguments.Count == 0 || marker.ConstructorArguments[0].Value is not string theme) {
            return null;
        }
        string[] requires = [.. marker.NamedArguments
            .Where(predicate: static argument => string.Equals(a: argument.MemberName, b: nameof(RhinoScenarioAttribute.Requires), comparisonType: StringComparison.Ordinal))
            .SelectMany(selector: static argument => argument.TypedValue.Value is IEnumerable<CustomAttributeTypedArgument> values
                ? values.Select(selector: static value => value.Value).OfType<string>()
                : [])];
        int budgetMs = marker.NamedArguments
            .Where(predicate: static argument => string.Equals(a: argument.MemberName, b: nameof(RhinoScenarioAttribute.BudgetMs), comparisonType: StringComparison.Ordinal))
            .Select(selector: static argument => argument.TypedValue.Value is int value ? value : 0)
            .FirstOrDefault();
        return new ScenarioEntry(Theme: theme, Name: $"{theme}.{method.Name}", Requires: requires, BudgetMs: budgetMs);
    }

    private static IEnumerable<Type> TypesOf(Assembly assembly, List<(string Key, string Value)> facts) {
        // BOUNDARY ADAPTER — a partially-loadable scenario assembly (regime-A skew) yields its
        // loadable types; the loader casualties are facts so selection never collapses to a
        // false zero-match with no diagnostic trail.
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException partial) {
            foreach (Exception? error in partial.LoaderExceptions) {
                if (error is not null) {
                    facts.Add(item: ("discovery.type.load.failed", $"{assembly.GetName().Name}: {error.GetType().Name}: {error.Message}"));
                }
            }
            return partial.Types.Where(predicate: static type => type is not null)!;
        }
    }

    private IEnumerable<string> ScenarioAssemblyPaths() =>
        manifest.ScenarioAssemblies is { Length: > 0 } rows
            ? rows
                .Select(selector: name => Path.Combine(path1: manifest.StagePath, path2: name))
                .Where(predicate: File.Exists)
            : Directory.EnumerateFiles(path: manifest.StagePath, searchPattern: "*.Tests.dll");

    private static HostFingerprint RunningFingerprint() => new(
        BundleVersion: RhinoApp.Version.ToString(),
        RhinoCommonVersion: typeof(RhinoApp).Assembly.GetName().Version?.ToString() ?? string.Empty,
        Grasshopper2Version: AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(predicate: static assembly => string.Equals(a: assembly.GetName().Name, b: "Grasshopper2", comparisonType: StringComparison.Ordinal))
            ?.GetName().Version?.ToString() ?? string.Empty,
        RuntimeVersion: Environment.Version.ToString());

    private static JsonElement Json(object? value) => value switch {
        null => JsonSerializer.SerializeToElement(value: "null", jsonTypeInfo: BridgeJsonContext.Default.String),
        bool flag => JsonSerializer.SerializeToElement(value: flag, jsonTypeInfo: BridgeJsonContext.Default.Boolean),
        int number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Int32),
        long number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Int64),
        double number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Double),
        string text => JsonSerializer.SerializeToElement(value: text, jsonTypeInfo: BridgeJsonContext.Default.String),
        IFormattable formattable => JsonSerializer.SerializeToElement(value: formattable.ToString(format: null, formatProvider: CultureInfo.InvariantCulture), jsonTypeInfo: BridgeJsonContext.Default.String),
        _ => JsonSerializer.SerializeToElement(value: value.ToString() ?? string.Empty, jsonTypeInfo: BridgeJsonContext.Default.String),
    };
}
