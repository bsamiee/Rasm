using System.Reflection;
using System.Runtime.Loader;

namespace Rasm.TestKit;

// --- [SERVICES] -------------------------------------------------------------------------
// Resolve Private=false RhinoWIP host assemblies from RHINO_WIP_APP_PATH (or newest /Applications/Rhino*.app) at load time.
public static class HostBundle {
    private static int registered;
    public static int RegistrationCount => Volatile.Read(location: ref registered);
    public static void Register() {
        if (Interlocked.Exchange(location1: ref registered, value: 1) == 0) {
            AssemblyLoadContext.Default.Resolving += static (context, name) => Resolve(context: context, name: name);
        }
    }
    private static Assembly? Resolve(AssemblyLoadContext context, AssemblyName name) =>
        // BOUNDARY ADAPTER — native loader callback; probe the RhinoWIP bundle for Private=false host assemblies.
        name.Name is string assembly
            ? Candidates(assembly: assembly).Where(File.Exists).Select(context.LoadFromAssemblyPath).FirstOrDefault()
            : null;
    private static IEnumerable<string> Candidates(string assembly) {
        string resources = Path.Combine(path1: BundlePath(), path2: "Contents/Frameworks/RhCore.framework/Versions/Current/Resources");
        yield return Path.Combine(path1: resources, path2: $"{assembly}.dll");
        yield return Path.Combine(paths: [resources, "ManagedPlugIns", "Grasshopper2Plugin.rhp", $"{assembly}.dll"]);
    }
    private static string BundlePath() =>
        Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH") switch {
            string env when Directory.Exists(path: env) => env,
            _ => Newest() ?? "/Applications/RhinoWIP.app",
        };
    private static string? Newest() =>
        Directory.Exists(path: "/Applications")
            ? Directory.EnumerateDirectories(path: "/Applications", searchPattern: "Rhino*.app")
                .OrderBy(keySelector: static directory => directory.Contains(value: "WIP", comparisonType: StringComparison.Ordinal) ? 1 : 0)
                .ThenBy(keySelector: static directory => directory, comparer: StringComparer.Ordinal)
                .LastOrDefault()
            : null;
}
