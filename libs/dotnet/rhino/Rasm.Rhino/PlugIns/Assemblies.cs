using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto;
using Rhino.Runtime;

namespace Rasm.Rhino.PlugIns;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class AssemblyLoading {
    // --- [NATIVE_LIBRARIES]
    private static nint Library(string frameworks, string name) =>
        DisposalOps.Within(static () => new RiskyAction(nameof(NativeLibrary.TryLoad)),
            _ => IO.lift(() => NativeLibrary.TryLoad(Path.Join(frameworks, $"lib{name}.dylib"), out nint handle) ? handle : 0)).Run();

    // --- [INITIALIZATION]
    public static readonly Unit Initialized = Initialize(typeof(AssemblyLoading).Assembly);

    private static Unit Initialize(Assembly copy) {
        Platform.Instance.LoadAssembly(copy);
        return (from context in Optional(AssemblyLoadContext.GetLoadContext(copy))
                from folder in Optional(Path.GetDirectoryName(copy.Location))
                from executables in Optional(Path.GetDirectoryName(Environment.ProcessPath))
                select (Context: context, Folder: folder, Frameworks: Path.Join(executables, "..", "Frameworks")))
            .Iter(static held => held.Context.ResolvingUnmanagedDll += (importer, name) =>
                string.Equals(Path.GetDirectoryName(importer.Location), held.Folder, StringComparison.Ordinal) ? Library(held.Frameworks, name) : 0);
    }
}
