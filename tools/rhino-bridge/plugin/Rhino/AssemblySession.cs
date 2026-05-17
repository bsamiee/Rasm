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
            ModelAbsoluteTolerance: document?.ModelAbsoluteTolerance,
            Assemblies: Assemblies(),
            Sessions: [.. sessions.Values.Where(static session => session.IsLoaded).Select(static session => session.Report())]);
    internal BridgeLoadReport Load(BridgeLoadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        string path = Path.GetFullPath(request.AssemblyPath);
        return File.Exists(path) switch {
            false => new(Status: BridgeWire.Failed, SessionId: null, AssemblyName: null, Location: path, PdbPath: null, Assemblies: [], Fault: BridgeFault.MessageOnly(category: "load", message: $"Assembly does not exist: {path}")),
            true => LoadExisting(path: path, workspaceRoot: request.WorkspaceRoot),
        };
    }
    internal BridgeUnloadReport Unload(BridgeUnloadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        if (!sessions.TryGetValue(key: request.SessionId, value: out BridgeAssemblySession? session)) {
            return new(Status: BridgeWire.Failed, SessionId: request.SessionId, UnloadRequested: false, Unloaded: false, Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded."));
        }
        BridgeUnloadReport report = session.Unload();
        if (report.Unloaded) {
            _ = sessions.Remove(key: request.SessionId);
        }
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
            return new(Status: BridgeWire.Failed, SessionId: sessionId, AssemblyName: null, Location: path, PdbPath: null, Assemblies: [], Fault: LoadFault(error: error));
        }
    }
    private static BridgeAssemblyReport[] Assemblies() {
        Assembly[] required = [typeof(RhinoApp).Assembly, typeof(BridgeWire).Assembly, typeof(Host).Assembly];
        return [.. required
            .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
            .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal)
            .Select(static assembly => Loaded(assembly: assembly, required: true))];
    }
    internal static BridgeAssemblyReport Loaded(Assembly assembly, bool required) =>
        new(Name: assembly.GetName().Name ?? "unknown", Status: BridgeWire.Ok, Required: required, Version: assembly.GetName().Version?.ToString(), InformationalVersion: assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion, Location: assembly.Location, Fault: null);
    private static BridgeFault LoadFault(Exception error) {
        ReflectionTypeLoadException? loader = error switch {
            ReflectionTypeLoadException current => current,
            { InnerException: ReflectionTypeLoadException current } => current,
            _ => null,
        };
        return BridgeFault.FromException(category: "load", error: error) with {
            Causes = loader?.LoaderExceptions.Select(static item => item is null ? null : BridgeFault.FromException(category: "loader", error: item)).OfType<BridgeFault>().ToArray(),
        };
    }
}

internal sealed class BridgeAssemblySession {
    private readonly string location;
    private BridgeLoadContext? context;
    private Assembly? assembly;
    private BridgeAssemblyReport[] closure;
    private bool unloadRequested;
    private BridgeAssemblySession(string sessionId, string location, BridgeLoadContext context, Assembly assembly, BridgeAssemblyReport[] closure) {
        SessionId = sessionId;
        this.location = location;
        this.context = context;
        this.assembly = assembly;
        this.closure = closure;
    }
    internal string SessionId { get; }
    internal bool IsLoaded => context is not null && assembly is not null;
    internal static BridgeAssemblySession Load(string sessionId, string assemblyPath, string workspaceRoot) {
        BridgeLoadContext loadContext = new(assemblyPath: assemblyPath, workspaceRoot: workspaceRoot);
        try {
            Assembly loadedAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            Assembly[] loadedClosure = loadContext.LoadClosure(root: loadedAssembly);
            string rootName = loadedAssembly.GetName().Name ?? string.Empty;
            BridgeAssemblyReport[] reports = [.. loadedClosure.Select(item => BridgeSessions.Loaded(assembly: item, required: string.Equals(a: item.GetName().Name, b: rootName, comparisonType: StringComparison.Ordinal)))];
            return new(sessionId: sessionId, location: assemblyPath, context: loadContext, assembly: loadedAssembly, closure: reports);
        } catch {
            loadContext.Unload();
            throw;
        }
    }
    internal BridgeSessionReport Report() =>
        new(SessionId: SessionId, AssemblyName: assembly?.GetName().Name ?? "unloaded", Location: location, Status: IsLoaded ? BridgeWire.Ok : BridgeWire.Skipped);
    internal BridgeLoadReport LoadReport() =>
        new(
            Status: BridgeWire.Ok,
            SessionId: SessionId,
            AssemblyName: assembly?.GetName().FullName,
            Location: location,
            PdbPath: Path.ChangeExtension(path: location, extension: ".pdb") is string pdb && File.Exists(pdb) ? pdb : null,
            Assemblies: closure,
            Fault: null);
    internal BridgeUnloadReport Unload() {
        if (unloadRequested) {
            return new(
                Status: BridgeWire.Failed,
                SessionId: SessionId,
                UnloadRequested: true,
                Unloaded: false,
                Fault: BridgeFault.MessageOnly(category: "unload", message: $"Session '{SessionId}' already requested unload, but loaded code still has live references."));
        }
        WeakReference? reference = Release();
        unloadRequested = true;
        bool unloaded = reference is not null && Collect(reference: reference);
        return new(
            Status: unloaded ? BridgeWire.Ok : BridgeWire.Failed,
            SessionId: SessionId,
            UnloadRequested: reference is not null,
            Unloaded: unloaded,
            Fault: unloaded ? null : BridgeFault.MessageOnly(category: "unload", message: $"Session '{SessionId}' requested unload, but loaded code still has live references."));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private WeakReference? Release() {
        WeakReference? reference = context is BridgeLoadContext active ? new WeakReference(target: active, trackResurrection: false) : null;
        closure = [];
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
}

internal sealed class BridgeLoadContext : AssemblyLoadContext {
    private readonly AssemblyDependencyResolver resolver;
    private readonly Dictionary<string, Assembly> shared;
    private readonly Dictionary<string, string> packageAssets;
    internal BridgeLoadContext(string assemblyPath, string workspaceRoot) : base(isCollectible: true) {
        resolver = new AssemblyDependencyResolver(assemblyPath);
        shared = SharedAssemblies();
        packageAssets = PackageAssets(assemblyPath: assemblyPath, workspaceRoot: workspaceRoot);
    }
    internal static string HostRoot => Path.GetDirectoryName(typeof(RhinoApp).Assembly.Location) ?? string.Empty;
    internal Assembly[] LoadClosure(Assembly root) {
        Dictionary<string, Assembly> loaded = new(StringComparer.Ordinal);
        Queue<Assembly> pending = new();
        AddClosureAssembly(assembly: root, loaded: loaded, pending: pending);
        while (pending.TryDequeue(result: out Assembly? current)) {
            foreach (AssemblyName reference in current.GetReferencedAssemblies()) {
                Assembly? resolved = Load(assemblyName: reference);
                if (resolved is not null && IsClosureAssembly(assembly: resolved)) {
                    AddClosureAssembly(assembly: resolved, loaded: loaded, pending: pending);
                }
            }
        }
        return [.. loaded.Values];
    }
    protected override Assembly? Load(AssemblyName assemblyName) =>
        assemblyName.Name switch {
            string name when shared.TryGetValue(key: name, value: out Assembly? assembly) => assembly,
            string name when packageAssets.TryGetValue(key: name, value: out string? path) => LoadFromAssemblyPath(path),
            _ when resolver.ResolveAssemblyToPath(assemblyName) is string path => LoadFromAssemblyPath(path),
            _ => null,
        };
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is string path ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
    private static void AddClosureAssembly(Assembly assembly, Dictionary<string, Assembly> loaded, Queue<Assembly> pending) {
        string? name = assembly.GetName().Name;
        if (!string.IsNullOrWhiteSpace(value: name) && loaded.TryAdd(key: name, value: assembly)) {
            pending.Enqueue(item: assembly);
        }
    }
    private bool IsClosureAssembly(Assembly assembly) =>
        AssemblyLoadContext.GetLoadContext(assembly: assembly) == this;
    private static Dictionary<string, Assembly> SharedAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(static assembly => IsUnder(path: assembly.Location, root: HostRoot))
            .Concat([typeof(BridgeWire).Assembly, typeof(RhinoApp).Assembly])
            .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
            .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
    private static Dictionary<string, string> PackageAssets(string assemblyPath, string workspaceRoot) {
        string deps = Path.ChangeExtension(path: assemblyPath, extension: ".deps.json");
        string packages = Path.Combine(path1: workspaceRoot, path2: ".cache/nuget/packages");
        if (!File.Exists(path: deps) || !Directory.Exists(path: packages)) {
            return new(StringComparer.Ordinal);
        }
        try {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json: File.ReadAllText(path: deps, encoding: Encoding.UTF8));
            return document.RootElement.GetProperty(propertyName: "targets").EnumerateObject()
                .SelectMany(target => target.Value.EnumerateObject())
                .SelectMany(library => PackageRuntimeAssets(library: library, packages: packages))
                .GroupBy(static item => item.Name, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => group.First().Path, StringComparer.Ordinal);
        } catch (Exception error) when (error is IOException or System.Text.Json.JsonException or InvalidOperationException or ArgumentException) {
            return new(StringComparer.Ordinal);
        }
    }
    private static IEnumerable<(string Name, string Path)> PackageRuntimeAssets(System.Text.Json.JsonProperty library, string packages) {
        string[] package = library.Name.Split(separator: '/', count: 2);
        return package is [string id, string version]
            && library.Value.TryGetProperty(propertyName: "runtime", value: out System.Text.Json.JsonElement runtime)
            && Directory.EnumerateDirectories(path: packages).FirstOrDefault(path => string.Equals(a: Path.GetFileName(path: path), b: id, comparisonType: StringComparison.OrdinalIgnoreCase)) is string packageRoot
            ? runtime.EnumerateObject()
                .Select(asset => Path.Combine(path1: packageRoot, path2: version, path3: asset.Name))
                .Where(File.Exists)
                .Select(static path => (Name: Path.GetFileNameWithoutExtension(path: path), Path: path))
            : [];
    }
    private static bool IsUnder(string path, string root) =>
        !string.IsNullOrWhiteSpace(value: path)
        && !string.IsNullOrWhiteSpace(value: root)
        && Path.GetRelativePath(relativeTo: Path.GetFullPath(root), path: Path.GetFullPath(path)) is string relative
        && !relative.StartsWith(value: "..", comparisonType: StringComparison.Ordinal)
        && !Path.IsPathRooted(path: relative);
}
