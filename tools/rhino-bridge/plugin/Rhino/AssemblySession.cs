namespace Rasm.RhinoBridge.Rhino;

// --- [SERVICES] -------------------------------------------------------------------------
internal sealed class BridgeSessions : IDisposable {
    private static readonly BridgeAssemblyInventory[] AssemblyInventory = [
        new(Name: "RhinoCommon", Required: true),
        new(Name: "Rasm.RhinoBridge.Contracts", Required: true),
        new(Name: "Rasm.RhinoBridge.Plugin", Required: true),
        new(Name: "Grasshopper2", Required: false),
        new(Name: "GrasshopperIO", Required: false),
        new(Name: "Rasm", Required: false),
        new(Name: "Rasm.Grasshopper", Required: false),
        new(Name: "Radyab", Required: false),
    ];
    private readonly Dictionary<string, BridgeAssemblySession> sessions = new(StringComparer.Ordinal);
    internal BridgeDoctor Doctor() =>
        new(
            RhinoName: RhinoApp.Name,
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoPid: Environment.ProcessId,
            ActiveDocument: RhinoDoc.ActiveDoc is not null,
            Assemblies: Assemblies(),
            Sessions: sessions.Values.Select(static session => session.Report()).ToArray());
    internal BridgeLoadReport Load(BridgeLoadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        string path = Path.GetFullPath(request.AssemblyPath);
        return File.Exists(path) switch {
            false => new(Status: BridgeWire.Failed, SessionId: null, AssemblyName: null, Location: path, PdbPath: null, Probes: [], Fault: BridgeFault.MessageOnly(category: "load", message: $"Assembly does not exist: {path}")),
            true => LoadExisting(path: path, workspaceRoot: request.WorkspaceRoot),
        };
    }
    internal BridgeRunReport Run(BridgeRunRequest request, int timeoutMs) {
        ArgumentNullException.ThrowIfNull(request);
        return sessions.TryGetValue(key: request.SessionId, value: out BridgeAssemblySession? session) switch {
            true => session.Run(request: request, timeoutMs: timeoutMs),
            false => new(Status: BridgeWire.Failed, SessionId: request.SessionId, DurationMs: 0, Probes: [], Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded.")),
        };
    }
    internal BridgeUnloadReport Unload(BridgeUnloadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        return sessions.Remove(key: request.SessionId, value: out BridgeAssemblySession? session) switch {
            true => session.Unload(),
            false => new(Status: BridgeWire.Failed, SessionId: request.SessionId, UnloadRequested: false, Unloaded: false, Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded.")),
        };
    }
    public void Dispose() {
        foreach (BridgeAssemblySession session in sessions.Values) {
            _ = session.Unload();
        }
        sessions.Clear();
    }
    private BridgeLoadReport LoadExisting(string path, string workspaceRoot) {
        string sessionId = Guid.NewGuid().ToString(format: "N");
        try {
            return BridgeAssemblySession.Load(sessionId: sessionId, assemblyPath: path, workspaceRoot: workspaceRoot) switch {
                BridgeAssemblySession session => Add(session: session),
                _ => throw new InvalidOperationException("Bridge session loader returned no session."),
            };
        } catch (Exception error) when (error is BadImageFormatException or FileLoadException or FileNotFoundException or InvalidOperationException or ReflectionTypeLoadException) {
            return new(Status: BridgeWire.Failed, SessionId: sessionId, AssemblyName: null, Location: path, PdbPath: null, Probes: [], Fault: LoadFault(error: error));
        }
    }
    private BridgeLoadReport Add(BridgeAssemblySession session) {
        sessions.Add(key: session.SessionId, value: session);
        return session.LoadReport();
    }
    private static BridgeAssemblyReport[] Assemblies() =>
        [.. AssemblyInventory
            .Select(static item => LoadedAssembly(name: item.Name) switch {
                Assembly assembly => Loaded(item: item, assembly: assembly),
                _ => new BridgeAssemblyReport(Name: item.Name, Status: BridgeWire.NotLoaded, Required: item.Required, Version: null, Location: null, Fault: item.Required ? BridgeFault.MessageOnly(category: "assembly", message: $"{item.Name} assembly is not loaded.") : null),
            })];
    private static Assembly? LoadedAssembly(string name) =>
        string.Equals(name, "Rasm.RhinoBridge.Plugin", StringComparison.Ordinal)
            ? typeof(Host).Assembly
            : AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, name, StringComparison.Ordinal));
    private static BridgeAssemblyReport Loaded(BridgeAssemblyInventory item, Assembly assembly) =>
        new(Name: item.Name, Status: BridgeWire.Ok, Required: item.Required, Version: assembly.GetName().Version?.ToString(), Location: assembly.Location, Fault: null);
    private static BridgeFault LoadFault(Exception error) =>
        BridgeFault.FromException(category: "load", error: error) with {
            Causes = error is ReflectionTypeLoadException rtl
                ? rtl.LoaderExceptions.Select(static loader => loader is null ? null : BridgeFault.FromException(category: "loader", error: loader)).OfType<BridgeFault>().ToArray()
                : null,
        };
}

internal sealed record BridgeAssemblyInventory(string Name, bool Required);

internal sealed class BridgeAssemblySession {
    private const string TypeDiscoveryFailed = "Target assembly type discovery failed.";
    private const int OutputLimit = 32768;
    private readonly string workspaceRoot;
    private readonly string location;
    private BridgeLoadContext? context;
    private Assembly? assembly;
    private ProbeEntry[] entries;
    private BridgeAssemblySession(string sessionId, string location, string workspaceRoot, BridgeLoadContext context, Assembly assembly, ProbeEntry[] entries) {
        SessionId = sessionId;
        this.location = location;
        this.workspaceRoot = workspaceRoot;
        this.context = context;
        this.assembly = assembly;
        this.entries = entries;
    }
    internal string SessionId { get; }
    internal static BridgeAssemblySession Load(string sessionId, string assemblyPath, string workspaceRoot) {
        BridgeLoadContext loadContext = new(assemblyPath: assemblyPath);
        try {
            Assembly loadedAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            ProbeEntry[] probes = Discover(assembly: loadedAssembly);
            return new(sessionId: sessionId, location: assemblyPath, workspaceRoot: workspaceRoot, context: loadContext, assembly: loadedAssembly, entries: probes);
        } catch {
            loadContext.Unload();
            throw;
        }
    }
    internal BridgeSessionReport Report() =>
        new(SessionId: SessionId, AssemblyName: assembly?.GetName().Name ?? "unloaded", Location: location, Probes: [.. entries.Select(static entry => entry.Descriptor)]);
    internal BridgeLoadReport LoadReport() =>
        new(
            Status: BridgeWire.Ok,
            SessionId: SessionId,
            AssemblyName: assembly?.GetName().FullName,
            Location: location,
            PdbPath: Path.ChangeExtension(path: location, extension: ".pdb") is string pdb && File.Exists(pdb) ? pdb : null,
            Probes: [.. entries.Select(static entry => entry.Descriptor)],
            Fault: null);
    internal BridgeRunReport Run(BridgeRunRequest request, int timeoutMs) {
        Stopwatch timer = Stopwatch.StartNew();
        ProbeEntry[] selected = Select(probe: request.Probe);
        using CancellationTokenSource cancellation = new(delay: TimeSpan.FromMilliseconds(Math.Clamp(value: timeoutMs, min: 1, max: 300000)));
        BridgeProbeReport[] reports = selected.Length switch {
            0 => [],
            _ => [.. selected.Select(entry => RunProbe(entry: entry, request: request, cancellation: cancellation.Token))],
        };
        timer.Stop();
        BridgeFault? fault = reports.Length switch {
            0 => BridgeFault.MessageOnly(category: "probe", message: request.Probe is string id ? $"Probe '{id}' was not found in session '{SessionId}'." : $"No IRhinoBridgeProbe implementations were found in session '{SessionId}'."),
            _ when reports.Any(static report => !string.Equals(report.Status, BridgeWire.Ok, StringComparison.Ordinal)) => BridgeFault.MessageOnly(category: "probe", message: "One or more Rhino bridge probes failed."),
            _ => null,
        };
        return new(
            Status: fault is null ? BridgeWire.Ok : BridgeWire.Failed,
            SessionId: SessionId,
            DurationMs: (int)timer.ElapsedMilliseconds,
            Probes: reports,
            Fault: fault);
    }
    internal BridgeUnloadReport Unload() {
        WeakReference? reference = context is BridgeLoadContext active ? new WeakReference(active, trackResurrection: false) : null;
        entries = [];
        assembly = null;
        context?.Unload();
        context = null;
        bool unloaded = reference is null || Collect(reference: reference);
        return new(
            Status: BridgeWire.Ok,
            SessionId: SessionId,
            UnloadRequested: reference is not null,
            Unloaded: unloaded,
            Fault: unloaded ? null : BridgeFault.MessageOnly(category: "unload", message: $"Session '{SessionId}' requested unload, but loaded code still has live references."));
    }
    private ProbeEntry[] Select(string? probe) =>
        probe switch {
            string id => [.. entries.Where(entry => string.Equals(entry.Descriptor.Id, id, StringComparison.Ordinal) || string.Equals(entry.Descriptor.TypeName, id, StringComparison.Ordinal))],
            _ => entries,
        };
    private BridgeProbeReport RunProbe(ProbeEntry entry, BridgeRunRequest request, CancellationToken cancellation) {
        Stopwatch timer = Stopwatch.StartNew();
        using StringWriter output = new(CultureInfo.InvariantCulture);
        try {
            IRhinoBridgeProbe probe = (IRhinoBridgeProbe)Activator.CreateInstance(entry.Type)!;
            JsonElement args = request.Arguments.ValueKind is JsonValueKind.Undefined ? JsonSerializer.SerializeToElement(new { }, BridgeWire.CompactJson) : request.Arguments;
            RhinoBridgeProbeContext probeContext = new(Document: RhinoDoc.ActiveDoc, WorkspaceRoot: workspaceRoot, Arguments: args, Output: output, Cancellation: cancellation);
            RhinoBridgeProbeResult result = probe.Run(context: probeContext);
            timer.Stop();
            (string captured, bool truncated) = Capture(output: output);
            return new(
                Id: entry.Descriptor.Id,
                TypeName: entry.Descriptor.TypeName,
                Status: result.Status,
                DurationMs: (int)timer.ElapsedMilliseconds,
                Diagnostics: [.. result.Diagnostics.Select(ToDiagnostic)],
                Output: captured,
                OutputTruncated: truncated,
                Summary: result.Summary,
                Fault: null);
        } catch (OperationCanceledException error) {
            timer.Stop();
            return Failure(entry: entry, status: BridgeWire.Timeout, timer: timer, output: output, fault: BridgeFault.FromException(category: "timeout", error: error));
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            timer.Stop();
            return Failure(entry: entry, status: BridgeWire.Failed, timer: timer, output: output, fault: BridgeFault.FromException(category: "probe", error: error));
        }
    }
    private static BridgeProbeReport Failure(ProbeEntry entry, string status, Stopwatch timer, StringWriter output, BridgeFault fault) {
        (string captured, bool truncated) = Capture(output: output);
        return new(Id: entry.Descriptor.Id, TypeName: entry.Descriptor.TypeName, Status: status, DurationMs: (int)timer.ElapsedMilliseconds, Diagnostics: [], Output: captured, OutputTruncated: truncated, Summary: null, Fault: fault);
    }
    private static BridgeDiagnostic ToDiagnostic(RhinoBridgeDiagnostic diagnostic) =>
        new(Severity: diagnostic.Severity, Message: diagnostic.Message, Source: diagnostic.Source, Code: diagnostic.Code, File: diagnostic.File, Line: diagnostic.Line, Column: diagnostic.Column, Category: diagnostic.Category);
    private static (string Output, bool Truncated) Capture(StringWriter output) {
        string text = output.ToString();
        return text.Length <= OutputLimit ? (text, false) : (text[..OutputLimit], true);
    }
    private static ProbeEntry[] Discover(Assembly assembly) {
        try {
            return [.. assembly.GetTypes()
                .Where(static type => typeof(IRhinoBridgeProbe).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
                .Select(static type => new ProbeEntry(Type: type, Descriptor: Describe(type: type)))];
        } catch (ReflectionTypeLoadException error) {
            throw new InvalidOperationException(message: LoaderMessage(error: error), innerException: error);
        }
    }
    private static string LoaderMessage(ReflectionTypeLoadException error) =>
        string.Join(separator: Environment.NewLine, values: error.LoaderExceptions.Select(static loader => loader?.Message).OfType<string>().Prepend(TypeDiscoveryFailed));
    private static BridgeProbeDescriptor Describe(Type type) =>
        Activator.CreateInstance(type) switch {
            IRhinoBridgeProbe probe => new(Id: probe.Id, TypeName: type.FullName ?? type.Name, AssemblyName: type.Assembly.GetName().Name ?? "unknown"),
            _ => throw new InvalidOperationException($"Probe type '{type.FullName}' does not expose a public parameterless constructor."),
        };
    private static bool Collect(WeakReference reference) {
        for (int i = 0; i < 4 && reference.IsAlive; i++) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        return !reference.IsAlive;
    }
}

internal sealed record ProbeEntry(Type Type, BridgeProbeDescriptor Descriptor);

internal sealed class BridgeLoadContext : AssemblyLoadContext {
    private readonly AssemblyDependencyResolver resolver;
    internal BridgeLoadContext(string assemblyPath) : base(isCollectible: true) => resolver = new AssemblyDependencyResolver(assemblyPath);
    protected override Assembly? Load(AssemblyName assemblyName) =>
        assemblyName.Name switch {
            "Rasm.RhinoBridge.Contracts" => typeof(IRhinoBridgeProbe).Assembly,
            "RhinoCommon" => typeof(RhinoApp).Assembly,
            "Grasshopper2" or "GrasshopperIO" or "Eto" when HostAssembly(assemblyName.Name) is Assembly hostAssembly => hostAssembly,
            string _ when resolver.ResolveAssemblyToPath(assemblyName) is string path => LoadFromAssemblyPath(path),
            _ => null,
        };
    private static Assembly? HostAssembly(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, name, StringComparison.Ordinal));
}
