using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Rhino;

// --- [COMPOSITION] ---------------------------------------------------------------------

string[] command = JsonSerializer.Deserialize<string[]>(Environment.GetEnvironmentVariable("RHINO_TEST_COMMAND") ?? throw new InvalidOperationException("RHINO_TEST_COMMAND is unset"))
    ?? throw new InvalidOperationException("RHINO_TEST_COMMAND holds no command");
(string path, string[] arguments) = command[1] == "exec" ? (command[2], command[3..]) : (Path.ChangeExtension(command[0], ".dll"), command[1..]);
Assembly tests = new Suite(path, AssemblyLoadContext.GetLoadContext(typeof(RhinoApp).Assembly) ?? throw new InvalidOperationException("RhinoCommon loaded outside a load context")).LoadFromAssemblyPath(path);
MethodInfo entry = tests.EntryPoint ?? throw new InvalidOperationException($"{path} declares no entry point");
(typeof(Assembly).GetMethod("SetEntryAssembly", new[] { typeof(Assembly) }) ?? throw new InvalidOperationException("The host runtime states no Assembly.SetEntryAssembly")).Invoke(null, new object[] { tests });
new Thread(() => {
    try { if (entry.Invoke(null, new object[] { arguments }) is Task task) task.GetAwaiter().GetResult(); } finally { RhinoApp.InvokeOnUiThread(new Action(() => RhinoApp.Exit(false))); }
}) { IsBackground = true }.Start();

// --- [SERVICES] ------------------------------------------------------------------------

sealed class Suite : AssemblyLoadContext {
    readonly AssemblyDependencyResolver resolver;
    readonly AssemblyLoadContext rhino;
    readonly string host = Path.GetDirectoryName(typeof(RhinoApp).Assembly.Location) ?? throw new InvalidOperationException("RhinoCommon loaded from no folder");
    public Suite(string path, AssemblyLoadContext rhino) : base(path) => (resolver, this.rhino) = (new AssemblyDependencyResolver(path), rhino);
    protected override Assembly Load(AssemblyName name) => !File.Exists(Path.Combine(host, $"{name.Name}.dll")) && resolver.ResolveAssemblyToPath(name) is string file ? LoadFromAssemblyPath(file) : rhino.LoadFromAssemblyName(name);
    protected override IntPtr LoadUnmanagedDll(string name) => resolver.ResolveUnmanagedDllToPath(name) is string file ? LoadUnmanagedDllFromPath(file) : IntPtr.Zero;
}
