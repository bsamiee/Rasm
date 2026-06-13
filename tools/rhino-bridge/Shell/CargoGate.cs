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

// Ownership: D-4's forwarding table as data — one declaration. Host-owned family roots resolve in
// the default ALC (single host type identity; Rhino already loaded them); bridge-owned names
// (Contract + the StreamJsonRpc closure) resolve in the shell ALC instance (single seam identity —
// the IBridgeCargo cast across the collectible boundary depends on it); everything else is
// cargo-first, which is what makes per-swap LanguageExt/Thinktecture copies hot-swappable.
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

// Ownership: the per-swap cargo resolution scope. Load consults HostAssemblyTable: parent-first
// for bridge-owned names, default-ALC fallthrough for host-owned, cargo-first otherwise.
internal sealed class CargoLoadContext(string cargoAssemblyPath, int generation) : AssemblyLoadContext(name: $"Rasm.Bridge.Cargo#{generation}", isCollectible: true) {
    private readonly AssemblyDependencyResolver resolver = new(componentAssemblyPath: cargoAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName) =>
        HostAssemblyTable.OwnerOf(simpleName: assemblyName.Name ?? string.Empty) switch {
            AssemblyOwner.Host => null,
            AssemblyOwner.Shell => HostAssemblyTable.ShellContext.LoadFromAssemblyName(assemblyName: assemblyName),
            _ => resolver.ResolveAssemblyToPath(assemblyName: assemblyName) is { } path ? LoadFromAssemblyPath(assemblyPath: path) : null,
        };

    protected override nint LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName: unmanagedDllName) is { } path ? LoadUnmanagedDllFromPath(unmanagedDllPath: path) : nint.Zero;
}

// Ownership: the D-3 collectible-ALC owner. Token-gated single ownership (Lock + generation);
// Swap runs on the idle frame and hash equality short-circuits the swap, never the LoadCargoAsync
// call (the reuse is evidenced by the cargo.reused fact). Unload reports the WeakReference +
// bounded-GC confirmation honestly and NEVER blocks on an unconfirmed unload: per the probe-0a
// verdict the recovery for a pinned cargo ALC is a SUPERVISED HOST RECYCLE decided workstation-side
// from UnloadReceipt.Confirmed=false — the fallback lives behind the same IBridgeCargo seam and
// never leaks into the Contract.
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
            // The manifest is the per-session carrier: SessionId sources every in-host stamp and
            // ReportDir roots the spool/capture writers, so activation hands it to the cargo ctor
            // (CargoManifest resolves shell-ALC-first — one type identity across the seam).
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
        WeakReference probe = Release(lease: lease);
        int retries = 0;
        while (probe.IsAlive && retries < GcRetryBudget) {
            retries++;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        // D-3d: an attached debugger pins collectible ALCs by design; the receipt carries the gate
        // so the supervisor can discount an unconfirmed unload instead of recycling the host.
        return new UnloadReceipt(
            Confirmed: !probe.IsAlive,
            DebuggerAttached: debugger,
            GcRetries: retries,
            ElapsedMs: Stopwatch.GetElapsedTime(startingTimestamp: started).TotalMilliseconds);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Release(CargoLease lease) {
        // D-2 precondition: cargo disposal drains scenario scopes + host-event detachers BEFORE the
        // ALC unload begins. NoInlining keeps no caller-frame strong reference alive so the
        // WeakReference probe is the only observer left.
        try {
            lease.Cargo.Dispose();
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            // A throwing cargo Dispose must not block the unload attempt; the leak (if any) is
            // reported by the receipt, never by an exception on the unload rail.
            Debug.WriteLine(message: $"cargo dispose threw: {error.Message}");
        }
        WeakReference probe = new(target: lease.Context);
        lease.Context.Unload();
        return probe;
    }

    private static void PublishUnload(UnloadReceipt receipt, Action<BridgeEvent> publish) =>
        publish(Fact(key: receipt.Confirmed ? "cargo.unload.confirmed" : "cargo.unload.leaked", value: $"gcRetries={receipt.GcRetries} elapsedMs={receipt.ElapsedMs:F0} debugger={receipt.DebuggerAttached}"));

    private static BridgeEvent.FactCase Fact(string key, string value) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) { Stamp = default };
}
