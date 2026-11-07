using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Rhino;

// --- [COMPOSITION] ---------------------------------------------------------------------

var path = Environment.GetEnvironmentVariable("RHINO_TEST_ASSEMBLY");
var arguments = JsonSerializer.Deserialize<string[]>(Environment.GetEnvironmentVariable("RHINO_TEST_ARGUMENTS"));
var tests = new Suite(path, AssemblyLoadContext.GetLoadContext(typeof(RhinoApp).Assembly)).LoadFromAssemblyPath(path);
typeof(Assembly).GetMethod("SetEntryAssembly", [typeof(Assembly)]).Invoke(null, [tests]);
new Thread(() => {
    try { if (tests.EntryPoint.Invoke(null, new object[] { arguments }) is Task task) task.GetAwaiter().GetResult(); }
    finally { RhinoApp.InvokeOnUiThread(new Action(() => RhinoApp.Exit(false))); }
}) { IsBackground = true }.Start();

// --- [SERVICES] ------------------------------------------------------------------------

sealed class Suite(string path, AssemblyLoadContext rhino) : AssemblyLoadContext(path) {
    readonly AssemblyDependencyResolver resolver = new(path);
    protected override Assembly Load(AssemblyName name) => resolver.ResolveAssemblyToPath(name) is string file ? LoadFromAssemblyPath(file) : rhino.LoadFromAssemblyName(name);
    protected override IntPtr LoadUnmanagedDll(string name) => resolver.ResolveUnmanagedDllToPath(name) is string file ? LoadUnmanagedDllFromPath(file) : IntPtr.Zero;
}
