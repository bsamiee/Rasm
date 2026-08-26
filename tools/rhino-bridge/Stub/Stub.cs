using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Bridge.Stub;

// --- [SERVICES] ------------------------------------------------------------------------

file sealed class ShellLoadContext(string shellAssemblyPath) : AssemblyLoadContext("Rasm.Bridge.Shell", isCollectible: false) {
    private readonly AssemblyDependencyResolver resolver = new(shellAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName) =>
        resolver.ResolveAssemblyToPath(assemblyName) is { } path ? LoadFromAssemblyPath(path) : null;

    protected override nint LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path ? LoadUnmanagedDllFromPath(path) : nint.Zero;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

file static class ShellSeam {
    private const string ShellAssemblyFile = "Rasm.Bridge.Shell.dll";
    private const string ShellEntryType = "Rasm.Bridge.Shell.ShellHost";
    private const string ShellEntryMethod = "Start";

    internal static object? Activate() {
        string deployDir = Path.GetDirectoryName(typeof(ShellLoadContext).Assembly.Location) ?? string.Empty;
        string shellPath = Path.Combine(deployDir, ShellAssemblyFile);
        try {
            return File.Exists(shellPath) ? Start(shellPath) : Poison($"shell assembly absent at '{shellPath}'");
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException
            or ArgumentException or BadImageFormatException or ReflectionTypeLoadException or TypeLoadException
            or MissingMemberException or TargetInvocationException or NotSupportedException) {
            return Poison(error.GetBaseException().Message);
        }
    }

    private static object? Start(string shellPath) {
        Assembly shell = new ShellLoadContext(shellPath).LoadFromAssemblyPath(shellPath);
        MethodInfo start = shell.GetType(ShellEntryType, throwOnError: true)!.GetMethod(ShellEntryMethod, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMethodException(ShellEntryType, ShellEntryMethod);
        return start.Invoke(null, [Environment.ProcessId]);
    }

    private static object? Poison(string fault) {
        try {
            string home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
            _ = Directory.CreateDirectory(home);
            using Process host = Process.GetCurrentProcess();
            using FileStream stream = new(Path.Combine(home, "rhino-bridge-rbx.json"), FileMode.Create, FileAccess.Write, FileShare.Read);
            using Utf8JsonWriter writer = new(stream);
            writer.WriteStartObject();
            writer.WriteString("$type", "poisoned");
            writer.WriteNumber("rhinoPid", Environment.ProcessId);
            writer.WriteNumber("rhinoStartedAtUnixMs", new DateTimeOffset(host.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds());
            writer.WriteString("rhinoVersion", RhinoApp.Version.ToString());
            writer.WriteString("fault", fault);
            writer.WriteEndObject();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            RhinoApp.WriteLine($"[rasm-bridge] poisoned endpoint write failed: {error.Message}; fault was: {fault}");
        }
        return null;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class RasmBridgePlugin : PlugIn {
    private object? shell;

    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.Idle += StartOnIdle;
        return LoadReturnCode.Success;
    }

    protected override void OnShutdown() {
        RhinoApp.Idle -= StartOnIdle;
        (shell as IDisposable)?.Dispose();
        shell = null;
    }

    private void StartOnIdle(object? sender, EventArgs args) {
        RhinoApp.Idle -= StartOnIdle;
        shell = ShellSeam.Activate();
    }
}
