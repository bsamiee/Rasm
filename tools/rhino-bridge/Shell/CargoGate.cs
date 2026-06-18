using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Rasm.Bridge.Contract;

namespace Rasm.Bridge.Shell;

// --- [TYPES] --------------------------------------------------------------------------------

internal enum AssemblyOwner {
    Host,
    Shell,
    Cargo,
}

// --- [TABLES] -------------------------------------------------------------------------------

// Ownership: assembly ownership table. Host families stay in the default ALC, bridge families stay
// in the shell ALC, and everything else resolves cargo-first for per-swap dependency isolation.
internal static class HostAssemblyTable {
    internal static readonly FrozenSet<string> HostOwned = new[] {
        "RhinoCommon", "Rhino.UI", "Rhino.Runtime.Code", "RhinoCodePlatform.Rhino3D",
        "Grasshopper2", "GrasshopperIO", "Eto", "Microsoft.macOS", "System.Drawing.Common",
    }.ToFrozenSet(comparer: StringComparer.Ordinal);

    internal static readonly FrozenSet<string> ShellOwned = new[] {
        "Rasm.Bridge.Contract", "Rasm.Bridge.Shell", "StreamJsonRpc",
        "Nerdbank.Streams", "Nerdbank.MessagePack", "PolyType",
        "Microsoft.VisualStudio.Threading", "Microsoft.VisualStudio.Validation",
        "Newtonsoft.Json", "MessagePack", "Microsoft.NET.StringTools", "System.IO.Pipelines",
    }.ToFrozenSet(comparer: StringComparer.Ordinal);

    internal static AssemblyLoadContext ShellContext { get; } =
        AssemblyLoadContext.GetLoadContext(assembly: typeof(HostAssemblyTable).Assembly) ?? AssemblyLoadContext.Default;

    internal static Assembly? ShellAssembly(AssemblyName assemblyName) =>
        string.Equals(a: assemblyName.Name, b: typeof(CargoManifest).Assembly.GetName().Name, comparisonType: StringComparison.Ordinal)
            ? typeof(CargoManifest).Assembly
            : string.Equals(a: assemblyName.Name, b: typeof(HostAssemblyTable).Assembly.GetName().Name, comparisonType: StringComparison.Ordinal)
                ? typeof(HostAssemblyTable).Assembly
                : ShellContext.Assemblies.FirstOrDefault(predicate: assembly => AssemblyName.ReferenceMatchesDefinition(reference: assemblyName, definition: assembly.GetName()));

    internal static AssemblyOwner OwnerOf(string simpleName) {
        string probe = simpleName;
        while (probe.Length > 0) {
            if (HostOwned.Contains(item: probe)) {
                return AssemblyOwner.Host;
            }
            if (ShellOwned.Contains(item: probe)) {
                return AssemblyOwner.Shell;
            }
            int cut = probe.LastIndexOf(value: '.');
            if (cut <= 0) {
                break;
            }
            probe = probe[..cut];
        }
        return AssemblyOwner.Cargo;
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: per-swap cargo resolution scope over the assembly ownership table.
internal sealed class CargoLoadContext(string cargoAssemblyPath, int generation) : AssemblyLoadContext(name: string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Rasm.Bridge.Cargo#{generation}"), isCollectible: true) {
    private readonly AssemblyDependencyResolver resolver = new(componentAssemblyPath: cargoAssemblyPath);
    private readonly string stagePath = Path.GetDirectoryName(path: cargoAssemblyPath) ?? ".";

    protected override Assembly? Load(AssemblyName assemblyName) =>
        HostAssemblyTable.OwnerOf(simpleName: assemblyName.Name ?? string.Empty) switch {
            AssemblyOwner.Host => null,
            AssemblyOwner.Shell => HostAssemblyTable.ShellAssembly(assemblyName: assemblyName)
                ?? HostAssemblyTable.ShellContext.LoadFromAssemblyName(assemblyName: assemblyName),
            _ => resolver.ResolveAssemblyToPath(assemblyName: assemblyName) is { } path
                ? LoadFromAssemblyPath(assemblyPath: path)
                : Path.Combine(path1: stagePath, path2: assemblyName.Name + ".dll") is { } staged && File.Exists(path: staged)
                    ? LoadFromAssemblyPath(assemblyPath: staged)
                    : null,
        };

    protected override nint LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName: unmanagedDllName) is { } path ? LoadUnmanagedDllFromPath(unmanagedDllPath: path) : nint.Zero;
}

// Ownership: collectible cargo ALC lifecycle. Hash equality short-circuits swaps inside the gate,
// while unload reports WeakReference confirmation honestly so workstation policy decides recycle.
internal sealed class CargoGate : IDisposable {
    private const string CargoAssemblyFile = "Rasm.Bridge.Cargo.dll";
    private const string CargoEntryType = "Rasm.Bridge.Cargo.CargoHost";
    private const int GcRetryBudget = 10;

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

    internal CargoReceipt Swap(CargoManifest manifest, Action<BridgeEvent> publish) {
        lock (sync) {
            long started = Stopwatch.GetTimestamp();
            bool reused = current is { } live && string.Equals(a: live.ContentHash, b: manifest.ContentHash, comparisonType: StringComparison.Ordinal);
            if (!reused) {
                if (current is { } stale) {
                    current = null;
                    PublishUnload(receipt: UnloadKernel(lease: stale), publish: publish);
                }
                current = Activate(manifest: manifest);
            }
            CargoLease lease = current!;
            publish(Fact(key: reused ? "cargo.reused" : "cargo.swapped", value: manifest.ContentHash));
            return new CargoReceipt(
                ContentHash: manifest.ContentHash,
                SwapMs: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds,
                Scenarios: lease.Cargo.Discover(),
                Capabilities: lease.Cargo.Probe(publish: publish));
        }
    }

    internal UnloadReceipt Unload() {
        lock (sync) {
            if (current is not { } lease) {
                return new UnloadReceipt(Confirmed: true, DebuggerAttached: Debugger.IsAttached, GcRetries: 0, ElapsedMs: 0.0);
            }
            current = null;
            return UnloadKernel(lease: lease);
        }
    }

    public void Dispose() {
        lock (sync) {
            if (current is { } lease) {
                current = null;
                _ = UnloadKernel(lease: lease);
            }
        }
    }

    private CargoLease Activate(CargoManifest manifest) {
        string entryPath = Path.Combine(path1: manifest.StagePath, path2: CargoAssemblyFile);
        generation++;
        CargoLoadContext context = new(cargoAssemblyPath: entryPath, generation: generation);
        try {
            Assembly assembly = context.LoadFromAssemblyPath(assemblyPath: entryPath);
            Type entry = assembly.GetType(name: CargoEntryType, throwOnError: true)!;
            // Manifest identity crosses shell-ALC-first so cargo stamps and writes to the session root.
            IBridgeCargo cargo = (IBridgeCargo)Activator.CreateInstance(type: entry, args: [manifest])!;
            return new CargoLease(ContentHash: manifest.ContentHash, Context: context, Cargo: cargo);
        } catch {
            context.Unload();
            throw;
        }
    }

    private static UnloadReceipt UnloadKernel(CargoLease lease) {
        long started = Stopwatch.GetTimestamp();
        bool debugger = Debugger.IsAttached;
        // Release sheds its own frame (NoInlining) before the WeakReference probe so no caller local
        // pins the context; a leak-free ALC then collects within the retry budget because each cycle's
        // GC.Collect plus finalizer drain reclaims the unreferenced load context, while a genuine leak
        // (a host-rooted reference into cargo) stays alive past the budget and reports Confirmed=false.
        WeakReference probe = Release(lease: lease);
        int retries = 0;
        while (probe.IsAlive && retries < GcRetryBudget) {
            retries++;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        // Debugger-pinned collectible ALCs report unconfirmed unload without forcing recycle.
        return new UnloadReceipt(
            Confirmed: !probe.IsAlive,
            DebuggerAttached: debugger,
            GcRetries: retries,
            ElapsedMs: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Release(CargoLease lease) {
        // NoInlining keeps caller frames from retaining the ALC after cargo disposal drains hooks.
        try {
            lease.Cargo.Dispose();
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            // Dispose failures cannot block unload; the receipt reports any remaining leak.
            Debug.WriteLine(message: $"cargo dispose threw: {error.Message}");
        }
        WeakReference probe = new(target: lease.Context);
        lease.Context.Unload();
        return probe;
    }

    private static void PublishUnload(UnloadReceipt receipt, Action<BridgeEvent> publish) =>
        publish(Fact(key: receipt.Confirmed ? "cargo.unload.confirmed" : "cargo.unload.leaked", value: string.Create(System.Globalization.CultureInfo.InvariantCulture, $"gcRetries={receipt.GcRetries} elapsedMs={receipt.ElapsedMs:F0} debugger={receipt.DebuggerAttached}")));

    private static BridgeEvent.FactCase Fact(string key, string value) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) { Stamp = default };
}
