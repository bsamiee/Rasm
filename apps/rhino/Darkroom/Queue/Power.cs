using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Rasm.Rhino;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]

namespace Darkroom.Queue;

// --- [TYPES] ---------------------------------------------------------------------------
[Flags]
internal enum ExecutionState : uint {
    None = 0,
    SystemRequired = 1u << 0,
    DisplayRequired = 1u << 1,
    Continuous = 1u << 31,
}

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class SafeNativeMethods {
    // --- [POWER]
    [LibraryImport("kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    internal static partial ExecutionState SetThreadExecutionState(ExecutionState flags);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MachinePower {
    // --- [HOLD]
    public static IO<T> HeldAwake<T>(IO<T> run) =>
        OperatingSystem.IsWindows()
            ? HeldAwakeOnWindows(run)
            : (from caffeinate in Started("caffeinate", ["-i", "-w", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)])
               from value in run.Finally(IO.lift(caffeinate.Kill))
               select value).Bracket();

    [SupportedOSPlatform("windows")]
    private static IO<T> HeldAwakeOnWindows<T>(IO<T> run) =>
        IO.lift(static () => SafeNativeMethods.SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired) switch {
            var prior => RhinoFault.Unless(prior is not ExecutionState.None).Map(_ => prior),
        }).OnUiThread().Bind(prior => run.Finally(IO.lift(() => RhinoFault.Unless(SafeNativeMethods.SetThreadExecutionState(prior) is not ExecutionState.None)).OnUiThread()));

    // --- [SHUTDOWN]
    public static IO<Unit> ShutDown =>
        OperatingSystem.IsWindows()
            ? (from shutdown in Started("shutdown", ["/s", "/t", "60"])
               from exited in IO.liftAsync(async environment => {
                   await shutdown.WaitForExitAsync(environment.Token).ConfigureAwait(false);
                   return unit;
               })
               from code in IO.lift(() => RhinoFault.Unless(shutdown.ExitCode is 0))
               select unit).Bracket()
            : Started("osascript", ["-e", "delay 60", "-e", """tell application "System Events" to shut down"""]).Map(static _ => unit).Bracket();

    private static IO<Process> Started(string program, Seq<string> arguments) =>
        unitIO.Bind(_ => Products.Made(Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true })));
}
