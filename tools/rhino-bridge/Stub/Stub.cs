using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Bridge.Stub;

// --- [SERVICES] ------------------------------------------------------------------------

file sealed class ShellLoadContext(string shellAssemblyPath) : AssemblyLoadContext(name: "Rasm.Bridge.Shell", isCollectible: false) {
    private readonly AssemblyDependencyResolver resolver = new(componentAssemblyPath: shellAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName) =>
        resolver.ResolveAssemblyToPath(assemblyName: assemblyName) is { } path ? LoadFromAssemblyPath(assemblyPath: path) : null;

    protected override nint LoadUnmanagedDll(string unmanagedDllName) =>
        resolver.ResolveUnmanagedDllToPath(unmanagedDllName: unmanagedDllName) is { } path ? LoadUnmanagedDllFromPath(unmanagedDllPath: path) : nint.Zero;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

file static class ShellSeam {
    private const string EndpointHomeName = ".rasm";
    private const string EndpointFileName = "rhino-bridge-rbx.json";
    private const string ShellAssemblyFile = "Rasm.Bridge.Shell.dll";
    private const string ShellEntryMethod = "Start";
    private const string ShellEntryType = "Rasm.Bridge.Shell.ShellHost";

    internal static object? Activate() {
        string deployDir = Path.GetDirectoryName(path: typeof(ShellLoadContext).Assembly.Location) ?? string.Empty;
        string shellPath = Path.Combine(path1: deployDir, path2: ShellAssemblyFile);
        try {
            return File.Exists(path: shellPath)
                ? Start(deployDir: deployDir, shellPath: shellPath)
                : Poison(fault: $"shell assembly absent at '{shellPath}'");
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException
            or ArgumentException or BadImageFormatException or ReflectionTypeLoadException or TypeLoadException
            or MissingMemberException or TargetInvocationException or NotSupportedException) {
            return Poison(fault: error.GetBaseException().Message);
        }
    }

    private static object? Start(string deployDir, string shellPath) {
        ShellLoadContext context = new(shellAssemblyPath: shellPath);
        Assembly shell = context.LoadFromAssemblyPath(assemblyPath: shellPath);
        Type entry = shell.GetType(name: ShellEntryType, throwOnError: true)!;
        MethodInfo start = entry.GetMethod(name: ShellEntryMethod, bindingAttr: BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMethodException(className: ShellEntryType, methodName: ShellEntryMethod);
        return start.Invoke(obj: null, parameters: [deployDir, Environment.ProcessId]);
    }

    private static object? Poison(string fault) {
        try {
            string directory = Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: EndpointHomeName);
            _ = Directory.CreateDirectory(path: directory);
            using Process host = Process.GetCurrentProcess();
            using FileStream stream = new(
                path: Path.Combine(path1: directory, path2: EndpointFileName),
                mode: FileMode.Create, access: FileAccess.Write, share: FileShare.Read);
            using Utf8JsonWriter writer = new(utf8Json: stream);
            writer.WriteStartObject();
            writer.WriteString(propertyName: "pipeName", value: string.Empty);
            writer.WriteNumber(propertyName: "rhinoPid", value: Environment.ProcessId);
            writer.WriteNumber(propertyName: "rhinoStartedAtUnixMs", value: new DateTimeOffset(dateTime: host.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds());
            writer.WriteNumber(propertyName: "contractGeneration", value: 0);
            writer.WriteString(propertyName: "shellVersion", value: string.Empty);
            writer.WriteString(propertyName: "rhinoVersion", value: RhinoApp.Version.ToString());
            writer.WriteString(propertyName: "fault", value: fault);
            writer.WriteEndObject();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            RhinoApp.WriteLine(message: $"[rasm-bridge] poisoned endpoint write failed: {error.Message}; fault was: {fault}");
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
