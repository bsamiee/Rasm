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

// Ownership: capability rows own their probes, so live checks, recorded host hazards, and
// requires-lattice degradation stay in one vocabulary instead of parallel probe tables.
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
        // BOUNDARY ADAPTER: throwing probes fail their own capability row, never the next scenario.
        try {
            (PhaseStatus outcome, string receipt) = probe(host);
            return new CapabilityEntry(Key: Key, Outcome: outcome, Receipt: receipt);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return new CapabilityEntry(Key: Key, Outcome: PhaseStatus.Failed, Receipt: $"probe threw {error.GetType().Name}: {error.Message}");
        }
    }
}

// --- [BOUNDARY] -----------------------------------------------------------------------------

// Ownership: the per-scenario scratch redirect. Live scenarios root their File3dm/PDF/OBJ writes at
// Path.GetTempPath(); pointing the OS temp vars at a <reportDir>/scratch/<scenario> tree for the
// scenario bracket keeps those artifacts under the retained report dir (DrainScopes never touches
// the filesystem) instead of leaking into the system temp tree. Dispose restores the prior env once.
// Root is the honest artifact root in both states: the redirected scratch tree when live, else the
// OS temp tree the un-redirected scenario actually writes to — never a reportDir the env never aimed at.
internal readonly record struct ScratchRedirect(string Root, bool Redirected, string? PriorTmpDir, string? PriorTmp, string? PriorTemp) : IDisposable {
    private static readonly string[] Keys = ["TMPDIR", "TMP", "TEMP"];

    internal static ScratchRedirect Open(string reportDir, string scenario) {
        // BOUNDARY ADAPTER: a filesystem fault degrades to an un-redirected scratch rooted at the
        // report dir — the scenario stays live and Dispose is inert, never a raw throw across the rail.
        string root = Path.Combine(path1: reportDir, path2: Spool.ScratchDirectory, path3: scenario);
        try {
            _ = Directory.CreateDirectory(path: root);
            ScratchRedirect redirect = new(
                Root: root,
                Redirected: true,
                PriorTmpDir: Environment.GetEnvironmentVariable(variable: "TMPDIR"),
                PriorTmp: Environment.GetEnvironmentVariable(variable: "TMP"),
                PriorTemp: Environment.GetEnvironmentVariable(variable: "TEMP"));
            foreach (string key in Keys) {
                Environment.SetEnvironmentVariable(variable: key, value: root);
            }
            return redirect;
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException) {
            return new ScratchRedirect(Root: Path.GetTempPath(), Redirected: false, PriorTmpDir: null, PriorTmp: null, PriorTemp: null);
        }
    }

    public void Dispose() {
        if (!Redirected) {
            return;
        }
        Environment.SetEnvironmentVariable(variable: "TMPDIR", value: PriorTmpDir);
        Environment.SetEnvironmentVariable(variable: "TMP", value: PriorTmp);
        Environment.SetEnvironmentVariable(variable: "TEMP", value: PriorTemp);
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the collectible cargo host owns scenario discovery, capability probing, requires
// admission, host-drift attribution, and scenario IO. Every event is both spooled for crash
// evidence and relayed through the shell-owned publisher; CargoManifest supplies the session stamp
// and artifact root. Cargo statics stay absent outside Gh2Lane and the per-run Capture hook.
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
        // Unload requires the ambient SDK hook, scenario scopes, and GH2 lane references to be clear.
        Capture.Hook = null;
        lock (sync) {
            lane?.Dispose();
            lane = null;
        }
    }

    // --- [PROBES]

    internal static (PhaseStatus Outcome, string Receipt) ProbeEventPipe() {
        // This probe asserts the current host pid, not a version or prior verdict.
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
        string path = Path.Combine(path1: manifest.ReportDir, path2: Spool.Gh2Directory, path3: "probe", path4: "gh2-render.png");
        return AcquireLane().Bind(f: live => live.DrawCanvas(path: path)) switch {
            Fin<CaptureFile>.Succ(CaptureFile file) => (PhaseStatus.Ok,
                string.Create(provider: CultureInfo.InvariantCulture, $"DrawToBitmap {file.Width}x{file.Height} in {Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds:F0}ms via the ctor-never-Show editor")),
            Fin<CaptureFile>.Fail(Error error) => (PhaseStatus.Unsupported, $"render lane unavailable: {error.Message}"),
            _ => (PhaseStatus.Failed, "render probe unresolved"),
        };
    }

    // --- [BRACKET]

    private static ScenarioReceipt Receipt(ScenarioEntry scenario, PhaseStatus status, double duration, BridgeFault? fault) =>
        new(Scenario: scenario.Name, Status: status, DurationMs: duration, Fault: fault) {
            ScenarioStatus = status,
            CertificatePath = string.Empty,
            ArtifactRefs = [],
            ReferenceResults = [],
            FirstScenarioFailure = string.Empty,
        };

    private ScenarioReceipt Refuse(ScenarioEntry scenario, CapabilityEntry gap, Action<BridgeEvent> publish, long started) {
        // Unmet capability requirements refuse before entrypoint invocation.
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        BridgeFault fault = new BridgeFault.CapabilityAbsent(Capability: gap.Key, ProbeReceipt: gap.Receipt);
        double duration = Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds;
        Emit(spool: spool, publish: publish, evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Unsupported, DurationMs: duration, Fault: fault) { Stamp = NextStamp(scenario: scenario.Name) });
        return Receipt(scenario: scenario, status: PhaseStatus.Unsupported, duration: duration, fault: fault);
    }

    private ScenarioReceipt Vanish(ScenarioEntry scenario, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        Emit(spool: spool, publish: publish, evt: Fact(key: "scenario.missing", value: $"'{scenario.Name}' not present in staged assemblies carrying [RhinoScenario]", scenario: scenario.Name));
        double duration = Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds;
        Emit(spool: spool, publish: publish, evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Failed, DurationMs: duration, Fault: null) { Stamp = NextStamp(scenario: scenario.Name) });
        return Receipt(scenario: scenario, status: PhaseStatus.Failed, duration: duration, fault: null);
    }

    private ScenarioReceipt Execute(ScenarioEntry scenario, MethodInfo entry, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(reportDir: manifest.ReportDir, scenario: scenario.Name);
        using ScratchRedirect scratch = ScratchRedirect.Open(reportDir: manifest.ReportDir, scenario: scenario.Name);
        void emit(BridgeEvent evt) => Emit(spool: spool, publish: publish, evt: evt);
        void fact(string key, object? value) => emit(Fact(key: key, value: value, scenario: scenario.Name));
        ScenarioContext? context = null;
        PhaseStatus status = PhaseStatus.Failed;
        BridgeFault? fault = null;
        bool commandCaptureWasEnabled = false;
        bool commandCaptureAdmitted = false;
        string commandCaptureFailure = string.Empty;
        try {
            // Both command surfaces clear per scenario: the capture buffer AND the persistent history
            // window, so a warm host never carries prior scenarios' lines into command.*.tail evidence.
            commandCaptureWasEnabled = RhinoApp.CommandWindowCaptureEnabled;
            RhinoApp.CommandWindowCaptureEnabled = true;
            _ = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
            RhinoApp.ClearCommandHistoryWindow();
            commandCaptureAdmitted = true;
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            commandCaptureFailure = $"{error.GetType().Name}: {error.Message}";
        }
        try {
            // Provision a host ActiveDoc when absent: RhinoDoc.Create registers it (CreateHeadless would not) and the host owns its lifetime — the quit scrub closes it, never disposed here, hence CA2000 is suppressed.
            RhinoDoc? prior = RhinoDoc.ActiveDoc;
#pragma warning disable CA2000
            RhinoDoc? resolved = prior ?? RhinoDoc.Create(modelTemplateFileName: null);
#pragma warning restore CA2000
            if (resolved is { } doc) {
                fact(key: "scenario.doc.source", value: prior is null ? "created" : "active");
                fact(key: "scratch.root", value: scratch.Root);
                fact(key: "scratch.redirected", value: scratch.Redirected);
                context = new ScenarioContext(doc: doc, sink: fact, scenario: scenario.Name);
                Capture.Hook = label => doc.Views.ActiveView is { } view
                    ? Shoot(spool: spool, view: view, scenario: scenario.Name, label: label, onFailure: false, emit: emit, fact: fact)
                    : Fin.Fail<CaptureReceipt>(error: Error.New(message: "Capture.Snapshot: no active viewport"));
                (status, fault) = Invoke(entry: entry, context: context, fact: fact);
                if (context.FactCount == 0) {
                    fact(key: "facts.empty", value: "scenario emitted zero facts");
                }
                if (status != PhaseStatus.Ok) {
                    AutoCapture(spool: spool, context: context, scenario: scenario, emit: emit, fact: fact);
                }
            } else {
                fact(key: "scenario.doc.absent", value: "RhinoDoc.Create(null) returned null; host cannot provision an active document");
            }
        } finally {
            // Footer facts ride the same write-through spool as per-event crash evidence.
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
        return Receipt(scenario: scenario, status: status, duration: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds, fault: fault);
    }

    private (PhaseStatus Status, BridgeFault? Fault) Invoke(MethodInfo entry, ScenarioContext context, Action<string, object?> fact) {
        // BOUNDARY ADAPTER: reflective dispatch maps loader drift to HostDrift; scenario throws
        // stay scenario facts and never escape the cargo rail.
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
        // Failure captures are evidence only: realized view scopes shoot viewports, gh2.* shoots canvas.
        if (context.RealizedView is { } view) {
            _ = Shoot(spool: spool, view: view, scenario: scenario.Name, label: null, onFailure: true, emit: emit, fact: fact);
        }
        if (scenario.Requires.Any(predicate: static key => key.StartsWith(value: "gh2.", comparisonType: StringComparison.Ordinal))) {
            string path = Path.Combine(path1: manifest.ReportDir, path2: Spool.Gh2Directory, path3: scenario.Name, path4: "failure.png");
            Fin<CaptureFile> shot = AcquireLane().Bind(f: live => live.DrawCanvas(path: path));
            if (shot is Fin<CaptureFile>.Succ(CaptureFile file)) {
                ArtifactRef artifact = Spool.IndexFile(
                    reportDir: manifest.ReportDir, path: file.Path, scenario: scenario.Name,
                    role: EvidenceRole.Gh2CanvasManifest, mediaType: "image/png",
                    retention: ArtifactRetentionClass.Forensic, onFailure: true);
                emit(new BridgeEvent.CaptureCase(Path: file.Path, Width: file.Width, Height: file.Height, OnFailure: true) {
                    Stamp = NextStamp(scenario: scenario.Name),
                    Artifact = artifact,
                    Capture = new CaptureArtifact(
                        Artifact: artifact, Width: file.Width, Height: file.Height, OnFailure: true,
                        Label: "gh2-failure", Frame: string.Create(provider: CultureInfo.InvariantCulture, $"{file.Width}x{file.Height}"), Camera: "gh2.canvas", NonBlank: true),
                });
            } else if (shot is Fin<CaptureFile>.Fail(Error error)) {
                fact("capture.canvas.failed", error.Message);
            }
        }
    }

    private Fin<CaptureReceipt> Shoot(Spool spool, RhinoView view, string scenario, string? label, bool onFailure, Action<BridgeEvent> emit, Action<string, object?> fact) {
        // Capture metadata travels with the shot so empty images are diagnosable.
        Fin<BridgeEvent.CaptureCase> shot = spool.Capture(view: view, label: label, onFailure: onFailure);
        if (shot is Fin<BridgeEvent.CaptureCase>.Succ(BridgeEvent.CaptureCase capture)) {
            emit(capture with { Stamp = NextStamp(scenario: scenario) });
            RhinoViewport viewport = view.ActiveViewport;
            fact("capture.camera.location", viewport.CameraLocation);
            fact("capture.camera.target", viewport.CameraTarget);
            fact("capture.frame", string.Create(provider: CultureInfo.InvariantCulture, $"{capture.Width}x{capture.Height}"));
            fact("capture.objects", view.Document.Objects.Count);
            return Fin.Succ(value: new CaptureReceipt(
                Path: capture.Path, Width: capture.Width, Height: capture.Height,
                OnFailure: capture.OnFailure, Artifact: capture.Artifact));
        }
        if (shot is Fin<BridgeEvent.CaptureCase>.Fail(Error error)) {
            fact("capture.failed", error.Message);
            return Fin.Fail<CaptureReceipt>(error: error);
        }
        return Fin.Fail<CaptureReceipt>(error: Error.New(message: "capture unresolved"));
    }

    // --- [DISCOVERY]

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
        // BOUNDARY ADAPTER: GH2 load failures project to the typed render capability rail.
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
        // BOUNDARY ADAPTER: partially loadable assemblies still yield usable types and loader facts.
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
        null => JsonSerializer.SerializeToElement(value: (string?)null),
        JsonElement element => element.Clone(),
        JsonDocument document => document.RootElement.Clone(),
        bool flag => JsonSerializer.SerializeToElement(value: flag, jsonTypeInfo: BridgeJsonContext.Default.Boolean),
        int number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Int32),
        long number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Int64),
        double number => JsonSerializer.SerializeToElement(value: number, jsonTypeInfo: BridgeJsonContext.Default.Double),
        string text => JsonSerializer.SerializeToElement(value: text, jsonTypeInfo: BridgeJsonContext.Default.String),
        IFormattable formattable => JsonSerializer.SerializeToElement(value: formattable.ToString(format: null, formatProvider: CultureInfo.InvariantCulture), jsonTypeInfo: BridgeJsonContext.Default.String),
        _ => SerializeUnknown(value: value),
    };

    private static JsonElement SerializeUnknown(object value) {
        try {
            return JsonSerializer.SerializeToElement(value: value, inputType: value.GetType());
        } catch (NotSupportedException) {
            return JsonSerializer.SerializeToElement(value: value.ToString() ?? string.Empty, jsonTypeInfo: BridgeJsonContext.Default.String);
        }
    }
}
