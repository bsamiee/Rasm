using System.Collections.Frozen;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Nodes;
using Eto.Drawing;
using Rasm.Bridge.Contract;
using Rasm.ScenarioKit;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using GhDocument = Grasshopper2.Doc.Document;

namespace Rasm.Bridge.Cargo;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed partial class CargoHost : IBridgeCargo {
    [SmartEnum<string>]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
    private sealed partial class HostCapability {
        public static readonly HostCapability RhinoDocument = new("rhino.document", static host => host.ProbeDocument());
        public static readonly HostCapability Gh2Editor = new("gh2.editor", static host => host.ProbeEditor());
        public static readonly HostCapability Gh2Render = new("gh2.render", static host => host.ProbeRender());
        public static readonly HostCapability Gh2Solve = new("gh2.solve", static host => host.ProbeSolve());

        private readonly Func<CargoHost, (PhaseStatus Outcome, string Detail)> probe;

        internal CapabilityEntry Probe(CargoHost host) {
            try {
                (PhaseStatus outcome, string detail) = probe(host);
                return new CapabilityEntry(Key, outcome, detail);
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                return new CapabilityEntry(Key, PhaseStatus.Failed, $"probe threw {error.GetType().Name}: {error.Message}");
            }
        }
    }

    private sealed record Gh2Baseline(FrozenSet<Guid> Objects, bool Modified);
    private sealed record HostBaseline(FrozenSet<Guid> RhinoObjects, bool RhinoModified, IReadOnlyDictionary<GhDocument, Gh2Baseline> Gh2Documents);

    private readonly record struct CleanupTally(int Added, int Removed, int Missing, int Residual) {
        internal bool Restored => Missing == 0 && Residual == 0;
        internal JsonObject ToJson() => new() { ["added"] = Added, ["removed"] = Removed, ["missing"] = Missing, ["residual"] = Residual };
    }

    private sealed record CleanupReport(CleanupTally Rhino, CleanupTally Gh2Documents, CleanupTally Gh2Objects) {
        internal bool Restored => Rhino.Restored && Gh2Documents.Restored && Gh2Objects.Restored;
        internal JsonObject ToJson() => new() { ["rhino"] = Rhino.ToJson(), ["gh2Documents"] = Gh2Documents.ToJson(), ["gh2Objects"] = Gh2Objects.ToJson() };
    }

    private readonly Lock sync = new();
    private readonly CargoManifest manifest;
    private readonly HostFingerprint running;
    private Seq<(ScenarioEntry Entry, MethodInfo Method)> corpus;
    private Seq<CapabilityEntry> capabilities;
    private Seq<(string Key, string Value)> discoveryFacts;
    private Gh2Lane? lane;
    private uint documentSerial;
    private long sequence;
    private bool scanned;

    public CargoHost(CargoManifest manifest, HostFingerprint running) {
        ArgumentNullException.ThrowIfNull(manifest);
        this.manifest = manifest;
        this.running = running;
    }

    public ScenarioEntry[] Discover() => [.. Scan().Map(static row => row.Entry)];

    public CapabilityEntry[] Probe(Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(publish);
        long started = Stopwatch.GetTimestamp();
        using Spool spool = new(manifest.ReportDir, ReportLayout.ProbeSlot);
        void emit(BridgeEvent evt) => Emit(spool, publish, evt);
        CapabilityEntry[] report = [.. HostCapability.Items.Select(row => row.Probe(this))];
        foreach (CapabilityEntry entry in report) {
            emit(Fact($"capability.{entry.Key}", $"{entry.Outcome.Key}: {entry.Detail}", ReportLayout.ProbeSlot));
        }
        _ = Scan();
        foreach ((string key, string value) in discoveryFacts) {
            emit(Fact(key, value, ReportLayout.ProbeSlot));
        }
        lock (sync) {
            capabilities = toSeq(report);
        }
        double wallMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        emit(new BridgeEvent.PhaseCase(SessionPhase.Probe, PhaseStatus.Ok, wallMs, Fault: null) { Stamp = NextStamp(ReportLayout.ProbeSlot) });
        return report;
    }

    public ScenarioOutcome Run(ScenarioEntry scenario, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(publish);
        long started = Stopwatch.GetTimestamp();
        MethodInfo entry = Scan()
            .Find(row => string.Equals(row.Entry.Name, scenario.Name, StringComparison.Ordinal))
            .Map(static row => row.Method)
            .IfNone(() => throw new InvalidOperationException($"'{scenario.Name}' is absent from the discovered corpus"));
        return FirstUnmet(scenario.Requires).Match(
            Some: gap => Refuse(scenario, gap, publish, started),
            None: () => Execute(scenario, entry, publish, started));
    }

    public void Dispose() {
        lock (sync) {
            lane = null;
            documentSerial = 0;
        }
    }

    // --- [PROBES]

    private (PhaseStatus Outcome, string Detail) ProbeDocument() =>
        AcquireDocument().Match(
            Succ: static doc => (PhaseStatus.Ok, $"'{Title(doc)}' view '{doc.Views.ActiveView.ActiveViewport.Name}' objects={doc.Objects.Count}"),
            Fail: static error => (PhaseStatus.Unsupported, error.Message));

    private (PhaseStatus Outcome, string Detail) ProbeEditor() =>
        AcquireLane().Match(
            Succ: static live => (PhaseStatus.Ok, string.Create(CultureInfo.InvariantCulture,
                $"Grasshopper2 {Gh2Lane.Version} hidden editor; plugins loaded={live.PluginsLoaded} failed={live.PluginsFailed} proxies={Gh2Lane.Registered}")),
            Fail: static error => (PhaseStatus.Unsupported, error.Message));

    private (PhaseStatus Outcome, string Detail) ProbeRender() {
        long started = Stopwatch.GetTimestamp();
        string path = Path.Combine(manifest.ReportDir, ReportLayout.Gh2Directory, ReportLayout.ProbeSlot, "gh2-render.png");
        return AcquireLane().Bind(live => live.DrawCanvas(path)).Match(
            Succ: file => (PhaseStatus.Ok, string.Create(CultureInfo.InvariantCulture,
                $"DrawToBitmap {file.Width}x{file.Height} -> {new FileInfo(file.Path).Length} bytes in {Stopwatch.GetElapsedTime(started).TotalMilliseconds:F0}ms")),
            Fail: static error => (PhaseStatus.Unsupported, error.Message));
    }

    private (PhaseStatus Outcome, string Detail) ProbeSolve() {
        long started = Stopwatch.GetTimestamp();
        return AcquireLane().Bind(static _ => Gh2Document.Isolated()).Bind(doc => {
            Fin<(Gh2Solve Solve, double[] Sum)> solved =
                from a in doc.Number(new PointF(0, 0), 2.5)
                from b in doc.Number(new PointF(0, 60), 4.0)
                from add in doc.Node(Gh2Object.Addition, new PointF(160, 30))
                from wireA in Gh2Document.Wire(a, add.Parameters.Input(0))
                from wireB in Gh2Document.Wire(b, add.Parameters.Input(1))
                from solve in doc.Solve()
                from sum in Gh2Document.Output<double>(add.Parameters.Output(0))
                select (solve, sum);
            doc.Close();
            return solved;
        }).Match(
            Succ: row => row.Sum is [6.5]
                ? (PhaseStatus.Ok, string.Create(CultureInfo.InvariantCulture, $"headless Addition solved 2.5 + 4 = 6.5 over {row.Solve.Computable} computables in {Stopwatch.GetElapsedTime(started).TotalMilliseconds:F0}ms"))
                : (PhaseStatus.Failed, $"headless Addition yielded [{string.Join(", ", row.Sum)}] instead of [6.5]"),
            Fail: static error => (PhaseStatus.Unsupported, error.Message));
    }

    private Fin<Gh2Lane> AcquireLane() {
        lock (sync) {
            try {
                lane ??= Gh2Lane.Acquire();
                return lane;
            } catch (Exception error) when (error is TypeLoadException or TypeInitializationException or FileNotFoundException or FileLoadException or MissingMethodException or InvalidOperationException) {
                return Error.New($"{error.GetType().Name}: {error.Message}");
            }
        }
    }

    private Fin<RhinoDoc> AcquireDocument() {
        lock (sync) {
            try {
                string? template = Template();
                if (documentSerial == 0) {
                    documentSerial = Mint(template);
                }
                return RhinoDoc.FromRuntimeSerialNumber(documentSerial) switch {
                    null => Error.New($"no Rhino document could be acquired; template='{template ?? "none"}'"),
                    { Views.ActiveView: null } doc => Error.New($"Rhino document '{Title(doc)}' has no active view; template='{template ?? "none"}'"),
                    { } doc => doc,
                };
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                return Error.New($"Rhino document acquisition threw {error.GetType().Name}: {error.Message}");
            }
        }
    }

    private static uint Mint(string? template) {
        uint existing = Serial(RhinoDoc.ActiveDoc ?? RhinoDoc.OpenDocuments().FirstOrDefault());
        if (existing != 0) {
            return existing;
        }
        using RhinoDoc created = RhinoDoc.Create(template);
        return Serial(created);
    }

    private static uint Serial(RhinoDoc? doc) => doc switch {
        null => 0u,
        { } live => live.RuntimeSerialNumber,
    };

    private static string? Template() =>
        FileSettings.TemplateFile is { Length: > 0 } pinned && File.Exists(pinned)
            ? pinned
            : FileSettings.TemplateFolder is { Length: > 0 } folder && Directory.Exists(folder)
                ? Directory.EnumerateFiles(folder, "*.3dm").Order(StringComparer.Ordinal).FirstOrDefault()
                : null;

    private static string Title(RhinoDoc doc) => doc.Name is { Length: > 0 } name ? name : "untitled";

    // --- [BRACKET]

    private ScenarioOutcome Refuse(ScenarioEntry scenario, CapabilityEntry gap, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(manifest.ReportDir, scenario.Name);
        BridgeFault fault = new BridgeFault.CapabilityAbsent(gap.Key, gap.Detail);
        double duration = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        Emit(spool, publish, new BridgeEvent.PhaseCase(SessionPhase.Execute, PhaseStatus.Unsupported, duration, fault) { Stamp = NextStamp(scenario.Name) });
        return new ScenarioOutcome(scenario.Name, PhaseStatus.Unsupported, duration, fault);
    }

    private ScenarioOutcome Execute(ScenarioEntry scenario, MethodInfo entry, Action<BridgeEvent> publish, long started) {
        using Spool spool = new(manifest.ReportDir, scenario.Name);
        void emit(BridgeEvent evt) => Emit(spool, publish, evt);
        void fact(string key, object? value) => emit(Fact(key, value, scenario.Name));
        PhaseStatus status = PhaseStatus.Failed;
        BridgeFault? fault = null;
        ScenarioContext? context = null;
        HostBaseline? baseline = null;
        RhinoDoc? host = null;
        try {
            Fin<(RhinoDoc Doc, DirectoryInfo Scratch)> staged = AcquireDocument().Bind(doc => PrepareScratch(scenario.Name).Map(scratch => (doc, scratch)));
            if (staged is Fin<(RhinoDoc Doc, DirectoryInfo Scratch)>.Fail(Error refused)) {
                fact("scenario.stage.failed", refused.Message);
            } else if (staged is Fin<(RhinoDoc Doc, DirectoryInfo Scratch)>.Succ((RhinoDoc doc, DirectoryInfo directory))) {
                host = doc;
                baseline = CaptureContent(doc);
                fact("scratch.root", directory.FullName);
                context = new ScenarioContext(
                    doc, directory, scenario.Name, fact,
                    captureRhino: label => doc.Views.ActiveView is { } view
                        ? Shoot(spool, view, scenario.Name, label.Key, onFailure: false, emit, fact)
                        : Error.New("CaptureSnapshot: no active viewport"),
                    captureGrasshopper: label => ShootGrasshopper(scenario.Name, label.Key, onFailure: false, emit, fact));
                (status, fault) = Invoke(entry, context, fact);
                if (context.FactCount == 0) {
                    fact("facts.empty", "scenario emitted zero facts");
                }
                if (status != PhaseStatus.Ok) {
                    AutoCapture(spool, context, scenario, emit, fact);
                }
            }
        } finally {
            if (context is { } scenarioContext && scenarioContext.DrainScopes() is > 0 and int leaked) {
                fact("scope.leaked", leaked);
            }
            if (baseline is { } captured && host is { } doc) {
                status = Restore(doc, captured, status, fact);
            }
            if (spool.Failures > 0) {
                publish(Fact("spool.degraded", spool.Failures, scenario.Name));
            }
            emit(new BridgeEvent.PhaseCase(SessionPhase.Execute, status, Stopwatch.GetElapsedTime(started).TotalMilliseconds, fault) { Stamp = NextStamp(scenario.Name) });
        }
        return new ScenarioOutcome(scenario.Name, status, Stopwatch.GetElapsedTime(started).TotalMilliseconds, fault);
    }

    private Fin<DirectoryInfo> PrepareScratch(string scenario) {
        try {
            return Directory.CreateDirectory(ReportLayout.Scratch(manifest.ReportDir, scenario));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or NotSupportedException) {
            return Error.New($"scenario scratch directory could not be created: {error.Message}");
        }
    }

    private (PhaseStatus Status, BridgeFault? Fault) Invoke(MethodInfo entry, ScenarioContext context, Action<string, object?> fact) {
        try {
            return entry.Invoke(null, [context]) switch {
                Fin<Unit>.Succ => (PhaseStatus.Ok, null),
                Fin<Unit>.Fail(Error failure) => Failed(fact, "scenario.failure", failure.Message),
                _ => Failed(fact, "scenario.entry.shape", $"'{entry.Name}' returned a non-Fin<Unit> value; the entrypoint shape is static Fin<Unit> (ScenarioContext)"),
            };
        } catch (Exception error) when (DriftMember(error) is { } member) {
            fact("scenario.drift", member);
            return (PhaseStatus.Failed, new BridgeFault.HostDrift(member, running));
        } catch (TargetInvocationException wrapped) when (wrapped.InnerException is { } inner) {
            return Failed(fact, "scenario.exception", $"{inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Failed(fact, "scenario.exception", $"{error.GetType().Name}: {error.Message}");
        }
    }

    private static PhaseStatus Restore(RhinoDoc doc, HostBaseline baseline, PhaseStatus status, Action<string, object?> fact) {
        try {
            CleanupReport cleanup = RestoreContent(doc, baseline);
            fact("cleanup", cleanup.ToJson());
            if (cleanup.Restored) {
                return status;
            }
            fact("scenario.cleanup.failed", "host content diverged from its pre-scenario baseline");
            return PhaseStatus.Failed;
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            fact("scenario.cleanup.exception", $"{error.GetType().Name}: {error.Message}\n{error.StackTrace}");
            return PhaseStatus.Failed;
        }
    }

    private void AutoCapture(Spool spool, ScenarioContext context, ScenarioEntry scenario, Action<BridgeEvent> emit, Action<string, object?> fact) {
        if (context.RealizedView is { } view) {
            _ = Shoot(spool, view, scenario.Name, "failure", onFailure: true, emit, fact);
        }
        if (scenario.Requires.Any(static key => key.StartsWith("gh2.", StringComparison.Ordinal))) {
            _ = ShootGrasshopper(scenario.Name, "failure", onFailure: true, emit, fact);
        }
    }

    private Fin<Snapshot> ShootGrasshopper(string scenario, string label, bool onFailure, Action<BridgeEvent> emit, Action<string, object?> fact) {
        string stem = Spool.Sanitize(label);
        string path = Path.Combine(manifest.ReportDir, ReportLayout.Gh2Directory, scenario, $"{stem}.png");
        return AcquireLane().Bind(live => live.DrawCanvas(path)).BiMap(
            Succ: file => {
                ArtifactRef artifact = ArtifactRef.Index(manifest.ReportDir, file.Path, EvidenceRole.Capture, scenario, onFailure);
                emit(new BridgeEvent.CaptureCase(artifact, file.Width, file.Height, stem, "gh2.canvas") { Stamp = NextStamp(scenario) });
                fact("capture.gh2.frame", string.Create(CultureInfo.InvariantCulture, $"{file.Width}x{file.Height}"));
                return new Snapshot(file.Path, file.Width, file.Height, onFailure, artifact);
            },
            Fail: error => {
                fact("capture.gh2.failed", error.Message);
                return error;
            });
    }

    private Fin<Snapshot> Shoot(Spool spool, RhinoView view, string scenario, string label, bool onFailure, Action<BridgeEvent> emit, Action<string, object?> fact) =>
        spool.Capture(view, label, onFailure).BiMap(
            Succ: capture => {
                emit(capture with { Stamp = NextStamp(scenario) });
                RhinoViewport viewport = view.ActiveViewport;
                fact("capture.camera.location", viewport.CameraLocation);
                fact("capture.camera.target", viewport.CameraTarget);
                fact("capture.frame", string.Create(CultureInfo.InvariantCulture, $"{capture.Width}x{capture.Height}"));
                fact("capture.objects", view.Document.Objects.Count);
                return new Snapshot(Path.Combine(manifest.ReportDir, capture.Artifact.Path), capture.Width, capture.Height, onFailure, capture.Artifact);
            },
            Fail: error => {
                fact("capture.failed", error.Message);
                return error;
            });

    // --- [BASELINE]

    private static HostBaseline CaptureContent(RhinoDoc doc) =>
        new(
            RhinoDocumentScope.ObjectIds(doc),
            doc.Modified,
            GhDocument.AllDocuments.ToDictionary(
                static document => document,
                static document => new Gh2Baseline(document.Objects.Forwards.Select(static item => item.InstanceId).ToFrozenSet(), document.Modified)));

    private static CleanupReport RestoreContent(RhinoDoc doc, HostBaseline baseline) {
        Guid[] rhinoAdded = [.. RhinoDocumentScope.ObjectIds(doc).Where(id => !baseline.RhinoObjects.Contains(id) && doc.Objects.FindId(id) is not null)];
        int rhinoMissing = baseline.RhinoObjects.Count(id => doc.Objects.FindId(id) is null);
        _ = doc.Objects.Delete(rhinoAdded, quiet: true);
        int rhinoResidual = rhinoAdded.Count(id => doc.Objects.FindId(id) is not null);
        int rhinoRemoved = rhinoAdded.Length - rhinoResidual;
        if (!baseline.RhinoModified) {
            doc.Modified = false;
        }
        GhDocument[] gh2Current = [.. GhDocument.AllDocuments];
        CleanupTally documents = default;
        CleanupTally objects = default;
        foreach ((GhDocument document, Gh2Baseline prior) in baseline.Gh2Documents) {
            if (!gh2Current.Contains(document)) {
                documents = documents with { Missing = documents.Missing + 1 };
                objects = objects with { Missing = objects.Missing + prior.Objects.Count };
                continue;
            }
            Guid[] current = [.. document.Objects.Forwards.Select(static item => item.InstanceId)];
            Guid[] added = [.. current.Where(id => !prior.Objects.Contains(id))];
            int removed = added.Count(document.Objects.Remove);
            int residual = added.Count(id => document.Objects.Forwards.Any(item => item.InstanceId == id));
            objects = new CleanupTally(
                objects.Added + added.Length, objects.Removed + removed,
                objects.Missing + prior.Objects.Count(id => !current.Contains(id)), objects.Residual + residual);
            if (!prior.Modified) {
                document.Unmodify();
            }
        }
        foreach (GhDocument created in gh2Current.Where(document => !baseline.Gh2Documents.ContainsKey(document))) {
            objects = objects with { Added = objects.Added + created.Objects.Count };
            created.Unmodify();
            created.Close();
            documents = GhDocument.AllDocuments.Contains(created)
                ? documents with { Added = documents.Added + 1, Residual = documents.Residual + 1 }
                : documents with { Added = documents.Added + 1, Removed = documents.Removed + 1 };
        }
        return new CleanupReport(new CleanupTally(rhinoAdded.Length, rhinoRemoved, rhinoMissing, rhinoResidual), documents, objects);
    }

    // --- [DISCOVERY]

    private Seq<(ScenarioEntry Entry, MethodInfo Method)> Scan() {
        lock (sync) {
            if (!scanned) {
                AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(typeof(CargoHost).Assembly) ?? AssemblyLoadContext.Default;
                List<(ScenarioEntry Entry, MethodInfo Method)> entries = [];
                List<(string Key, string Value)> facts = [];
                foreach (string path in ScenarioAssemblyPaths()) {
                    Assembly assembly;
                    try {
                        assembly = context.LoadFromAssemblyPath(path);
                    } catch (Exception error) when (error is BadImageFormatException or FileLoadException or FileNotFoundException) {
                        facts.Add(("discovery.assembly.load.failed", $"{Path.GetFileName(path)}: {error.GetType().Name}: {error.Message}"));
                        continue;
                    }
                    int before = entries.Count;
                    foreach (Type type in LoadableTypes(assembly, error => facts.Add(("discovery.type.load.failed", $"{assembly.GetName().Name}: {error.GetType().Name}: {error.Message}")))) {
                        try {
                            entries.AddRange(toSeq(type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                                .Choose(static method => Optional(method.GetCustomAttribute<RhinoScenarioAttribute>())
                                    .Map(marker => (Entry: new ScenarioEntry(marker.Theme, $"{marker.Theme}.{method.Name}", marker.Requires, marker.BudgetMs), Method: method))));
                        } catch (Exception error) when (error is TypeLoadException or FileLoadException or FileNotFoundException or NotSupportedException or CustomAttributeFormatException) {
                            facts.Add(("discovery.type.methods.failed", $"{type.FullName}: {error.GetType().Name}: {error.Message}"));
                        }
                    }
                    facts.Add(("discovery.assembly.scenarios", string.Create(CultureInfo.InvariantCulture, $"{assembly.GetName().Name}:{entries.Count - before}")));
                }
                corpus = toSeq(entries);
                discoveryFacts = toSeq(facts);
                scanned = true;
            }
            return corpus;
        }
    }

    private IEnumerable<string> ScenarioAssemblyPaths() {
        string directory = Path.Combine(manifest.StagePath, CargoManifest.ScenariosDirectory);
        return Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, "*.dll").Order(StringComparer.Ordinal)
            : [];
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly, Action<Exception> onLoaderFault) {
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException partial) {
            foreach (Exception? error in partial.LoaderExceptions) {
                if (error is not null) {
                    onLoaderFault(error);
                }
            }
            return partial.Types.Where(static type => type is not null)!;
        }
    }

    private Option<CapabilityEntry> FirstUnmet(string[] requires) {
        Seq<CapabilityEntry> granted;
        lock (sync) {
            granted = capabilities;
        }
        return toSeq(requires)
            .Map(key => granted.Find(entry => string.Equals(entry.Key, key, StringComparison.Ordinal))
                .IfNone(new CapabilityEntry(key, PhaseStatus.Unsupported, "no capability row on this host")))
            .Find(static entry => entry.Outcome != PhaseStatus.Ok);
    }

    // --- [EVIDENCE]

    private BridgeEvent.FactCase Fact(string key, object? value, string scenario) =>
        BridgeEvent.Fact(key, Json(value), NextStamp(scenario));

    private EventStamp NextStamp(string? scenario) => new(
        manifest.SessionId, Interlocked.Increment(ref sequence), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), scenario);

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
        spool.Append(evt);
        publish(evt);
    }

    private static JsonElement Json(object? value) => value switch {
        null => JsonSerializer.SerializeToElement(value: (string?)null),
        JsonElement element => element.Clone(),
        JsonNode node => JsonSerializer.SerializeToElement(node, BridgeJsonContext.Default.JsonNode),
        bool flag => JsonSerializer.SerializeToElement(flag, BridgeJsonContext.Default.Boolean),
        int number => JsonSerializer.SerializeToElement(number, BridgeJsonContext.Default.Int32),
        long number => JsonSerializer.SerializeToElement(number, BridgeJsonContext.Default.Int64),
        double number => JsonSerializer.SerializeToElement(number, BridgeJsonContext.Default.Double),
        string text => JsonSerializer.SerializeToElement(text, BridgeJsonContext.Default.String),
        IFormattable formattable => JsonSerializer.SerializeToElement(formattable.ToString(format: null, CultureInfo.InvariantCulture), BridgeJsonContext.Default.String),
        _ => SerializeUnknown(value),
    };

    private static JsonElement SerializeUnknown(object value) {
        try {
            return JsonSerializer.SerializeToElement(value, value.GetType());
        } catch (NotSupportedException) {
            return JsonSerializer.SerializeToElement(value.ToString() ?? string.Empty, BridgeJsonContext.Default.String);
        }
    }
}
