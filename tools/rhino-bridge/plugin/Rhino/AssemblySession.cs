namespace Rasm.RhinoBridge.Rhino;

// --- [SERVICES] -------------------------------------------------------------------------
internal sealed class BridgeSessions : IDisposable {
    private readonly Dictionary<string, BridgeAssemblySession> sessions = new(StringComparer.Ordinal);
    internal BridgeDoctor Doctor(RhinoDoc? document) =>
        new(
            RhinoName: RhinoApp.Name,
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoPid: Environment.ProcessId,
            ActiveDocument: document is not null,
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
    internal BridgeRunReport Run(BridgeRunRequest request, RhinoDoc? document, int timeoutMs) {
        ArgumentNullException.ThrowIfNull(request);
        return sessions.TryGetValue(key: request.SessionId, value: out BridgeAssemblySession? session) switch {
            true => session.Run(request: request, document: document, timeoutMs: timeoutMs),
            false => new(Status: BridgeWire.Failed, SessionId: request.SessionId, DurationMs: 0, Probes: [], Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded.")),
        };
    }
    internal BridgeUnloadReport Unload(BridgeUnloadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        if (!sessions.TryGetValue(key: request.SessionId, value: out BridgeAssemblySession? session)) {
            return new(Status: BridgeWire.Failed, SessionId: request.SessionId, UnloadRequested: false, Unloaded: false, Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded."));
        }
        BridgeUnloadReport report = session.Unload();
        _ = report.Unloaded && sessions.Remove(key: request.SessionId);
        return report;
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
            BridgeAssemblySession session = BridgeAssemblySession.Load(sessionId: sessionId, assemblyPath: path, workspaceRoot: workspaceRoot);
            sessions.Add(key: session.SessionId, value: session);
            return session.LoadReport();
        } catch (Exception error) when (error is BadImageFormatException or FileLoadException or FileNotFoundException or InvalidOperationException or ReflectionTypeLoadException) {
            return new(Status: BridgeWire.Failed, SessionId: sessionId, AssemblyName: null, Location: path, PdbPath: null, Probes: [], Fault: LoadFault(error: error));
        }
    }
    private static BridgeAssemblyReport[] Assemblies() {
        Assembly[] required = [typeof(RhinoApp).Assembly, typeof(IRhinoBridgeProbe).Assembly, typeof(BridgeWire).Assembly, typeof(Host).Assembly];
        return [.. required
            .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
            .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .Select(static assembly => Loaded(assembly: assembly, required: true))];
    }
    private static BridgeAssemblyReport Loaded(Assembly assembly, bool required) =>
        new(Name: assembly.GetName().Name ?? "unknown", Status: BridgeWire.Ok, Required: required, Version: assembly.GetName().Version?.ToString(), Location: assembly.Location, Fault: null);
    private static BridgeFault LoadFault(Exception error) {
        ReflectionTypeLoadException? loader = LoaderError(error: error);
        return BridgeFault.FromException(category: "load", error: error) with {
            Causes = loader?.LoaderExceptions.Select(static item => item is null ? null : BridgeFault.FromException(category: "loader", error: item)).OfType<BridgeFault>().ToArray(),
        };
    }
    private static ReflectionTypeLoadException? LoaderError(Exception error) =>
        error switch {
            ReflectionTypeLoadException loader => loader,
            { InnerException: ReflectionTypeLoadException loader } => loader,
            _ => null,
        };
}

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
            Assembly[] closure = loadContext.LoadClosure(root: loadedAssembly, workspaceRoot: workspaceRoot);
            ProbeEntry[] probes = Discover(assemblies: closure);
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
    internal BridgeRunReport Run(BridgeRunRequest request, RhinoDoc? document, int timeoutMs) {
        Stopwatch timer = Stopwatch.StartNew();
        ProbeEntry[] selected = Select(probe: request.Probe);
        using CancellationTokenSource cancellation = new(delay: TimeSpan.FromMilliseconds(Math.Clamp(value: timeoutMs, min: 1, max: 300000)));
        BridgeProbeReport[] reports = selected.Length switch {
            0 => [],
            _ => [.. selected.Select(entry => RunProbe(entry: entry, request: request, document: document, cancellation: cancellation.Token))],
        };
        timer.Stop();
        BridgeFault? fault = reports.Length switch {
            0 => BridgeFault.MessageOnly(category: "probe", message: request.Probe is string id ? $"Probe '{id}' was not found in session '{SessionId}'." : $"No IRhinoBridgeProbe implementations were found in session '{SessionId}'."),
            _ when reports.Any(static report => !string.Equals(report.Status, BridgeWire.Ok, StringComparison.Ordinal)) => BridgeFault.MessageOnly(category: "probe", message: "One or more Rhino bridge probes failed."),
            _ => null,
        };
        return new(Status: fault is null ? BridgeWire.Ok : BridgeWire.Failed, SessionId: SessionId, DurationMs: (int)timer.ElapsedMilliseconds, Probes: reports, Fault: fault);
    }
    internal BridgeUnloadReport Unload() {
        WeakReference? reference = Release();
        bool unloaded = reference is null || Collect(reference: reference);
        return new(
            Status: unloaded ? BridgeWire.Ok : BridgeWire.Failed,
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
    private BridgeProbeReport RunProbe(ProbeEntry entry, BridgeRunRequest request, RhinoDoc? document, CancellationToken cancellation) {
        Stopwatch timer = Stopwatch.StartNew();
        using StringWriter output = new(CultureInfo.InvariantCulture);
        try {
            IRhinoBridgeProbe probe = (IRhinoBridgeProbe)Activator.CreateInstance(entry.Type)!;
            JsonElement args = request.Arguments.ValueKind is JsonValueKind.Undefined ? JsonSerializer.SerializeToElement(new { }, BridgeWire.CompactJson) : request.Arguments;
            RhinoBridgeProbeContext probeContext = new(Document: document, WorkspaceRoot: workspaceRoot, Arguments: args, Output: output, Cancellation: cancellation);
            RhinoBridgeProbeResult result = probe.Run(context: probeContext);
            timer.Stop();
            (string captured, bool truncated) = Capture(output: output);
            return new(Id: entry.Descriptor.Id, TypeName: entry.Descriptor.TypeName, Status: result.Status, DurationMs: (int)timer.ElapsedMilliseconds, Diagnostics: result.Diagnostics, Output: captured, OutputTruncated: truncated, Summary: result.Summary, Fault: null);
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
    private static (string Output, bool Truncated) Capture(StringWriter output) {
        string text = output.ToString();
        return text.Length <= OutputLimit ? (text, false) : (text[..OutputLimit], true);
    }
    private static ProbeEntry[] Discover(IEnumerable<Assembly> assemblies) {
        try {
            return [.. assemblies
                .SelectMany(static assembly => assembly.GetTypes())
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
    [MethodImpl(MethodImplOptions.NoInlining)]
    private WeakReference? Release() {
        WeakReference? reference = context is BridgeLoadContext active ? new WeakReference(active, trackResurrection: false) : null;
        entries = [];
        assembly = null;
        context?.Unload();
        context = null;
        return reference;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Collect(WeakReference reference) {
        for (int index = 0; index < 10 && reference.IsAlive; index++) {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        return !reference.IsAlive;
    }
    private sealed record ProbeEntry(Type Type, BridgeProbeDescriptor Descriptor);
}

internal sealed class BridgeLoadContext : AssemblyLoadContext {
    private readonly AssemblyDependencyResolver resolver;
    private readonly Dictionary<string, Assembly> shared;
    internal BridgeLoadContext(string assemblyPath) : base(isCollectible: true) {
        resolver = new AssemblyDependencyResolver(assemblyPath);
        shared = SharedAssemblies();
    }
    internal static string HostRoot => Path.GetDirectoryName(typeof(RhinoApp).Assembly.Location) ?? string.Empty;
    internal static bool IsHostAssembly(Assembly assembly, string hostRoot) =>
        !string.IsNullOrWhiteSpace(assembly.Location)
        && !string.IsNullOrWhiteSpace(hostRoot)
        && Path.GetFullPath(assembly.Location).StartsWith(value: hostRoot, comparisonType: StringComparison.Ordinal);
    internal Assembly[] LoadClosure(Assembly root, string workspaceRoot) {
        Dictionary<string, Assembly> closure = new(StringComparer.Ordinal);
        Queue<Assembly> pending = new();
        AddClosureAssembly(assembly: root, closure: closure, pending: pending);
        while (pending.TryDequeue(out Assembly? current)) {
            foreach (AssemblyName reference in current.GetReferencedAssemblies()) {
                Assembly? loaded = Load(assemblyName: reference);
                if (loaded is not null && IsClosureAssembly(assembly: loaded, workspaceRoot: workspaceRoot)) {
                    AddClosureAssembly(assembly: loaded, closure: closure, pending: pending);
                }
            }
        }
        return [.. closure.Values];
    }
    protected override Assembly? Load(AssemblyName assemblyName) =>
        assemblyName.Name switch {
            string name when shared.TryGetValue(key: name, value: out Assembly? assembly) => assembly,
            _ when resolver.ResolveAssemblyToPath(assemblyName) is string path => LoadFromAssemblyPath(path),
            _ => null,
        };
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is string path ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
    private static void AddClosureAssembly(Assembly assembly, Dictionary<string, Assembly> closure, Queue<Assembly> pending) {
        string? name = assembly.GetName().Name;
        if (!string.IsNullOrWhiteSpace(name) && closure.TryAdd(key: name, value: assembly)) {
            pending.Enqueue(item: assembly);
        }
    }
    private bool IsClosureAssembly(Assembly assembly, string workspaceRoot) =>
        AssemblyLoadContext.GetLoadContext(assembly) == this && IsUnder(path: assembly.Location, root: workspaceRoot);
    private static Dictionary<string, Assembly> SharedAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => IsHostAssembly(assembly: assembly, hostRoot: HostRoot))
            .Concat([typeof(IRhinoBridgeProbe).Assembly, typeof(BridgeWire).Assembly, typeof(RhinoApp).Assembly])
            .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
            .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    private static bool IsUnder(string path, string root) =>
        !string.IsNullOrWhiteSpace(path)
        && !string.IsNullOrWhiteSpace(root)
        && Path.GetRelativePath(relativeTo: Path.GetFullPath(root), path: Path.GetFullPath(path)) is string relative
        && !relative.StartsWith(value: "..", comparisonType: StringComparison.Ordinal)
        && !Path.IsPathRooted(relative);
}
