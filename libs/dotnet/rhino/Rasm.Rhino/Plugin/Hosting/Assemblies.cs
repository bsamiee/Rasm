using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto;
using Rhino.Runtime;

namespace Rasm.Rhino.Plugin.Hosting;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class Assemblies {
    // --- [NATIVE_LIBRARIES]
    private static nint Library(string frameworks, string name) =>
        (from spy in use(static () => new RiskyAction(nameof(NativeLibrary.TryLoad)))
         from library in IO.lift(() => NativeLibrary.TryLoad(Path.Join(frameworks, $"lib{name}.dylib"), out nint handle) ? handle : 0)
         select library).Bracket().Run();

    // --- [INITIALIZATION]
    public static readonly Unit Initialized = Initialize(typeof(Assemblies).Assembly);

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
