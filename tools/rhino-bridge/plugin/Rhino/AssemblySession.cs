namespace Rasm.RhinoBridge.Rhino;

// --- [SERVICES] -------------------------------------------------------------------------
internal sealed class BridgeSessionTable : IDisposable {
    private readonly Dictionary<string, Entry> sessions = new(StringComparer.Ordinal);
    internal BridgeReport.Doctor Doctor(RhinoDoc? document) =>
        new(
            Status: PhaseStatus.Ok,
            Fault: null,
            RhinoName: RhinoApp.Name,
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoPid: Environment.ProcessId,
            ActiveDocument: document is not null,
            ModelAbsoluteTolerance: document?.ModelAbsoluteTolerance,
            Assemblies: [.. LoadContext.SharedAssemblies().Values
                .OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal)
                .Select(static assembly => LoadedReport(assembly: assembly, required: true))],
            Sessions: [.. sessions.Values.Where(static entry => entry.IsLoaded).Select(static entry => entry.Report())]);
    internal BridgeReport.Load Load(BridgeLoadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        string path = Path.GetFullPath(request.AssemblyPath);
        string packages = PackageCacheRoot(workspaceRoot: request.WorkspaceRoot, packageCacheRoot: request.PackageCacheRoot);
        return File.Exists(path) switch {
            false => new(Status: PhaseStatus.Failed, Fault: BridgeFault.MessageOnly(category: "load", message: $"Assembly does not exist: {path}"), SessionId: null, AssemblyName: null, Location: path, PdbPath: null, WorkspaceRoot: request.WorkspaceRoot, PackageCacheRoot: packages, Assemblies: []),
            true => LoadExisting(path: path, workspaceRoot: request.WorkspaceRoot, packageCacheRoot: packages),
        };
    }
    internal BridgeReport.Unload Unload(BridgeUnloadRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        if (!sessions.TryGetValue(key: request.SessionId, value: out Entry? entry)) {
            return new(Status: PhaseStatus.Failed, Fault: BridgeFault.MessageOnly(category: "session", message: $"Bridge session '{request.SessionId}' is not loaded."), SessionId: request.SessionId, UnloadRequested: false, Unloaded: false);
        }
        BridgeReport.Unload report = entry.Unload();
        if (report.Unloaded) {
            _ = sessions.Remove(key: request.SessionId);
        }
        return report;
    }
    public void Dispose() {
        foreach (Entry entry in sessions.Values) {
            _ = entry.Unload();
        }
        sessions.Clear();
    }
    private BridgeReport.Load LoadExisting(string path, string workspaceRoot, string packageCacheRoot) {
        string sessionId = Guid.NewGuid().ToString(format: "N");
        // BOUNDARY ADAPTER — ALC load surfaces a wide native exception family; collapse into a Failed report with causes.
        try {
            Entry entry = Entry.Load(sessionId: sessionId, assemblyPath: path, workspaceRoot: workspaceRoot, packageCacheRoot: packageCacheRoot);
            sessions.Add(key: entry.SessionId, value: entry);
            return entry.LoadReport();
        } catch (Exception error) when (error is BadImageFormatException or FileLoadException or FileNotFoundException or InvalidOperationException or ReflectionTypeLoadException) {
            return new(Status: PhaseStatus.Failed, Fault: LoadFault(error: error), SessionId: sessionId, AssemblyName: null, Location: path, PdbPath: null, WorkspaceRoot: workspaceRoot, PackageCacheRoot: packageCacheRoot, Assemblies: []);
        }
    }
    private static string PackageCacheRoot(string workspaceRoot, string? packageCacheRoot) =>
        string.IsNullOrWhiteSpace(value: packageCacheRoot) ? Path.Combine(path1: workspaceRoot, path2: ".cache/nuget/packages") : Path.GetFullPath(path: packageCacheRoot);
    internal static BridgeAssemblyReport LoadedReport(Assembly assembly, bool required) =>
        new(Name: assembly.GetName().Name ?? "unknown", Status: PhaseStatus.Ok, Required: required, Version: assembly.GetName().Version?.ToString(), InformationalVersion: assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion, Location: assembly.Location, Fault: null);
    private static BridgeFault LoadFault(Exception error) =>
        BridgeFault.FromException(category: "load", error: error) with {
            Causes = error switch {
                ReflectionTypeLoadException current => [.. current.LoaderExceptions.OfType<Exception>().Select(static loader => BridgeFault.FromException(category: "loader", error: loader))],
                { InnerException: ReflectionTypeLoadException nested } => [.. nested.LoaderExceptions.OfType<Exception>().Select(static loader => BridgeFault.FromException(category: "loader", error: loader))],
                _ => null,
            },
        };

    private sealed class Entry {
        private readonly string location;
        private readonly string workspaceRoot;
        private readonly string packageCacheRoot;
        private LoadContext? context;
        private Assembly? assembly;
        private BridgeAssemblyReport[] closure;
        private Entry(string sessionId, string location, string workspaceRoot, string packageCacheRoot, LoadContext context, Assembly assembly, BridgeAssemblyReport[] closure) {
            SessionId = sessionId;
            this.location = location;
            this.workspaceRoot = workspaceRoot;
            this.packageCacheRoot = packageCacheRoot;
            this.context = context;
            this.assembly = assembly;
            this.closure = closure;
        }
        internal string SessionId { get; }
        internal bool IsLoaded => context is not null && assembly is not null;
        internal static Entry Load(string sessionId, string assemblyPath, string workspaceRoot, string packageCacheRoot) {
            LoadContext loadContext = new(assemblyPath: assemblyPath, packageCacheRoot: packageCacheRoot);
            // BOUNDARY ADAPTER — partial-load rollback requires synchronous Unload on the same thread.
            try {
                Assembly loadedAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                Assembly[] loadedClosure = loadContext.LoadClosure(root: loadedAssembly);
                string rootName = loadedAssembly.GetName().Name ?? string.Empty;
                BridgeAssemblyReport[] reports = [.. loadedClosure.Select(item => LoadedReport(assembly: item, required: string.Equals(a: item.GetName().Name, b: rootName, comparisonType: StringComparison.Ordinal)))];
                return new(sessionId: sessionId, location: assemblyPath, workspaceRoot: workspaceRoot, packageCacheRoot: packageCacheRoot, context: loadContext, assembly: loadedAssembly, closure: reports);
            } catch {
                loadContext.Unload();
                throw;
            }
        }
        internal BridgeSessionReport Report() =>
            new(SessionId: SessionId, AssemblyName: assembly?.GetName().Name ?? "unloaded", Location: location, Status: IsLoaded ? PhaseStatus.Ok : PhaseStatus.Skipped);
        internal BridgeReport.Load LoadReport() =>
            new(
                Status: PhaseStatus.Ok,
                Fault: null,
                SessionId: SessionId,
                AssemblyName: assembly?.GetName().FullName,
                Location: location,
                PdbPath: Path.ChangeExtension(path: location, extension: ".pdb") is string pdb && File.Exists(pdb) ? pdb : null,
                WorkspaceRoot: workspaceRoot,
                PackageCacheRoot: packageCacheRoot,
                Assemblies: closure);
        internal BridgeReport.Unload Unload() =>
            (IsLoaded, Release()) switch {
                (false, _) => new(Status: PhaseStatus.Failed, Fault: BridgeFault.MessageOnly(category: "unload", message: $"Session '{SessionId}' is already unloaded."), SessionId: SessionId, UnloadRequested: false, Unloaded: false),
                (_, WeakReference reference) when Collect(reference: reference) => new(Status: PhaseStatus.Ok, Fault: null, SessionId: SessionId, UnloadRequested: true, Unloaded: true),
                _ => new(Status: PhaseStatus.Failed, Fault: BridgeFault.MessageOnly(category: "unload", message: $"Session '{SessionId}' requested unload, but loaded code still has live references."), SessionId: SessionId, UnloadRequested: true, Unloaded: false),
            };
        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference Release() {
            WeakReference reference = new(target: context!, trackResurrection: false);
            closure = [];
            assembly = null;
            context?.Unload();
            context = null;
            return reference;
        }
        // Why: Collectible ALC unloads require finalizer drain + reference release; 3 cycles
        // is sufficient for synchronous Rhino plugin contexts. If a session leaks, the
        // BridgeReport.Unload.Unloaded=false signals the agent; manual restart resolves.
        // BOUNDARY ADAPTER — GC drain loop is the canonical .NET unloadability pattern.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Collect(WeakReference reference) {
            int attempts = 0;
            while (reference.IsAlive && attempts++ < 3) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return !reference.IsAlive;
        }
    }

    internal sealed class LoadContext : AssemblyLoadContext {
        private readonly AssemblyDependencyResolver resolver;
        private readonly Dictionary<string, Assembly> shared;
        private readonly Dictionary<string, string> packageAssets;
        private readonly Seq<Func<AssemblyName, Assembly?>> resolvers;
        internal LoadContext(string assemblyPath, string packageCacheRoot) : base(isCollectible: true) {
            resolver = new AssemblyDependencyResolver(assemblyPath);
            shared = SharedAssemblies();
            packageAssets = PackageAssets(assemblyPath: assemblyPath, packageCacheRoot: packageCacheRoot);
            // Cascade: resolver (plugin-folder + RID fallback) → packageAssets (NuGet cache, covers macos hostpolicy gap) → shared (host identity).
            resolvers = [
                name => resolver.ResolveAssemblyToPath(name) is string p ? LoadFromAssemblyPath(p) : null,
                name => name.Name is string n && packageAssets.TryGetValue(key: n, value: out string? p) ? LoadFromAssemblyPath(p) : null,
                name => name.Name is string n && shared.TryGetValue(key: n, value: out Assembly? a) ? a : null,
            ];
            Unloading += _ => {
                shared.Clear();
                packageAssets.Clear();
            };
        }
        internal static string HostRoot => Path.GetDirectoryName(typeof(RhinoApp).Assembly.Location) ?? string.Empty;
        internal Assembly[] LoadClosure(Assembly root) {
            Dictionary<string, Assembly> loaded = new(StringComparer.Ordinal);
            Queue<Assembly> pending = new();
            AddClosure(assembly: root, loaded: loaded, pending: pending);
            while (pending.TryDequeue(result: out Assembly? current)) {
                foreach (AssemblyName reference in current.GetReferencedAssemblies()) {
                    Assembly? resolved = Load(assemblyName: reference);
                    if (resolved is not null && GetLoadContext(assembly: resolved) == this) {
                        AddClosure(assembly: resolved, loaded: loaded, pending: pending);
                    }
                }
            }
            return [.. loaded.Values];
        }
        protected override Assembly? Load(AssemblyName assemblyName) =>
            resolvers.Select(selector: fn => fn(assemblyName)).FirstOrDefault(static result => result is not null);
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) =>
            resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is string path ? LoadUnmanagedDllFromPath(path) : IntPtr.Zero;
        private static void AddClosure(Assembly assembly, Dictionary<string, Assembly> loaded, Queue<Assembly> pending) {
            string? name = assembly.GetName().Name;
            if (!string.IsNullOrWhiteSpace(value: name) && loaded.TryAdd(key: name, value: assembly)) {
                pending.Enqueue(item: assembly);
            }
        }
        internal static Dictionary<string, Assembly> SharedAssemblies() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(static assembly => IsUnder(path: assembly.Location, root: HostRoot))
                .Concat([typeof(BridgeWire).Assembly, typeof(RhinoApp).Assembly])
                .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
                .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
                .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);
        private static Dictionary<string, string> PackageAssets(string assemblyPath, string packageCacheRoot) {
            string deps = Path.ChangeExtension(path: assemblyPath, extension: ".deps.json");
            string packages = Path.GetFullPath(path: packageCacheRoot);
            if (!File.Exists(path: deps) || !Directory.Exists(path: packages)) {
                return new(StringComparer.Ordinal);
            }
            // BOUNDARY ADAPTER — .deps.json may be missing fields on partial restore.
            try {
                using JsonDocument document = JsonDocument.Parse(json: File.ReadAllText(path: deps, encoding: Encoding.UTF8));
                return document.RootElement.GetProperty(propertyName: "targets").EnumerateObject()
                    .SelectMany(target => target.Value.EnumerateObject())
                    .SelectMany(library => PackageRuntimeAssets(library: library, packages: packages))
                    .GroupBy(static item => item.Name, StringComparer.Ordinal)
                    .ToDictionary(static group => group.Key, static group => group.First().Path, StringComparer.Ordinal);
            } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or ArgumentException) {
                return new(StringComparer.Ordinal);
            }
        }
        private static IEnumerable<(string Name, string Path)> PackageRuntimeAssets(JsonProperty library, string packages) {
            string[] package = library.Name.Split(separator: '/', count: 2);
            return package is [string id, string version]
                && library.Value.TryGetProperty(propertyName: "runtime", value: out JsonElement runtime)
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
}
