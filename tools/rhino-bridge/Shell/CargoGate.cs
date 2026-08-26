using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class CargoLoadContext : AssemblyLoadContext {
    private static readonly AssemblyLoadContext ShellContext =
        GetLoadContext(typeof(CargoLoadContext).Assembly) ?? Default;

    private readonly AssemblyDependencyResolver resolver;
    private readonly string stagePath;

    internal CargoLoadContext(string cargoAssemblyPath, int generation)
        : base(string.Create(CultureInfo.InvariantCulture, $"Rasm.Bridge.Cargo#{generation}"), isCollectible: true) {
        resolver = new AssemblyDependencyResolver(cargoAssemblyPath);
        stagePath = Path.GetDirectoryName(cargoAssemblyPath)
            ?? throw new InvalidOperationException($"cargo entry has no parent directory: '{cargoAssemblyPath}'");
    }

    protected override Assembly? Load(AssemblyName assemblyName) =>
        Loaded(Default, assemblyName) is not null
            ? null
            : Loaded(ShellContext, assemblyName)
                ?? Staged(assemblyName.Name + ".dll")
                ?? Staged(Path.Combine(CargoManifest.ScenariosDirectory, assemblyName.Name + ".dll"));

    protected override nint LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path ? LoadUnmanagedDllFromPath(path) : nint.Zero;

    private Assembly? Staged(string relative) {
        string path = Path.Combine(stagePath, relative);
        return File.Exists(path) ? LoadFromAssemblyPath(path) : null;
    }

    private static Assembly? Loaded(AssemblyLoadContext context, AssemblyName assemblyName) =>
        context.Assemblies.FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName.Name, StringComparison.Ordinal));
}

internal sealed class CargoGate : IDisposable {
    private const string CargoEntryType = "Rasm.Bridge.Cargo.CargoHost";

    private readonly Lock sync = new();
    private CargoLease? current;
    private int generation;

    private sealed record CargoLease(string ContentHash, CargoLoadContext Context, IBridgeCargo Cargo);

    internal IBridgeCargo? Current {
        get {
            lock (sync) {
                return current?.Cargo;
            }
        }
    }

    internal static LocalRpcException Refuse(BridgeFault fault) =>
        new(fault.Prescription) {
            ErrorCode = BridgeFault.RpcErrorCode,
            ErrorData = JsonSerializer.SerializeToElement(fault, BridgeJsonContext.Default.BridgeFault),
        };

    internal LoadedCargo Load(CargoManifest manifest, HostFingerprint running, Action<BridgeEvent> publish) {
        lock (sync) {
            long started = Stopwatch.GetTimestamp();
            if (current is { } active && !string.Equals(active.ContentHash, manifest.ContentHash, StringComparison.Ordinal)) {
                throw Refuse(new BridgeFault.CargoRecycleRequired(active.ContentHash, manifest.ContentHash));
            }
            bool reused = current is not null;
            CargoLease lease = current ??= Activate(manifest, running);
            publish(BridgeEvent.Fact(reused ? "cargo.reused" : "cargo.loaded", manifest.ContentHash));
            return new LoadedCargo(
                manifest.ContentHash,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                lease.Cargo.Discover(),
                lease.Cargo.Probe(publish));
        }
    }

    internal UnloadOutcome Unload() {
        lock (sync) {
            if (current is not { } lease) {
                return new UnloadOutcome(ReleaseRequested: false, ElapsedMs: 0.0);
            }
            current = null;
            long started = Stopwatch.GetTimestamp();
            Release(lease);
            return new UnloadOutcome(ReleaseRequested: true, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }

    public void Dispose() => _ = Unload();

    private CargoLease Activate(CargoManifest manifest, HostFingerprint running) {
        string entryPath = Path.Combine(manifest.StagePath, CargoManifest.AssemblyFile);
        generation++;
        CargoLoadContext context = new(entryPath, generation);
        try {
            Type entry = context.LoadFromAssemblyPath(entryPath).GetType(CargoEntryType, throwOnError: true)!;
            IBridgeCargo cargo = (IBridgeCargo)Activator.CreateInstance(entry, manifest, running)!;
            return new CargoLease(manifest.ContentHash, context, cargo);
        } catch {
            context.Unload();
            throw;
        }
    }

    private static void Release(CargoLease lease) {
        try {
            lease.Cargo.Dispose();
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            Debug.WriteLine($"cargo dispose threw: {error.Message}");
        }
        lease.Context.Unload();
    }
}
