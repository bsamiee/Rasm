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
            : DisposalOps.Within<Process, T>(retain =>
                Started("caffeinate", ["-i", "-w", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)], retain).Bind(_ => run),
                static processes => Callbacks.Each(processes.Rev().Map(static process => Callbacks.Each(Seq(IO.lift(() => process.Kill()), IO.lift(process.Dispose))).Map(unit)))
                    .Map(static _ => unit));

    [SupportedOSPlatform("windows")]
    private static IO<T> HeldAwakeOnWindows<T>(IO<T> run) =>
        DisposalOps.Within(
            IO.lift(static () => SafeNativeMethods.SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired) switch {
                var prior => Refused.Unless(prior is not ExecutionState.None, prior, nameof(SafeNativeMethods.SetThreadExecutionState)),
            }).OnUiThread(),
            _ => run,
            static flags => IO.lift(() => Refused.Unless(SafeNativeMethods.SetThreadExecutionState(flags) is not ExecutionState.None, nameof(SafeNativeMethods.SetThreadExecutionState))).OnUiThread());

    // --- [SHUTDOWN]
    public static IO<Unit> ShutDown =>
        OperatingSystem.IsWindows()
            ? DisposalOps.Within<Process, Unit>(retain =>
                from shutdown in Started("shutdown", ["/s", "/t", "60"], retain)
                from exited in IO.liftAsync(async environment => {
                    await shutdown.WaitForExitAsync(environment.Token).ConfigureAwait(false);
                    return unit;
                })
                from code in IO.lift(() => Refused.Unless(shutdown.ExitCode is 0, nameof(Process.ExitCode)))
                select unit, DisposalOps.Release)
            : DisposalOps.Within<Process, Unit>(retain =>
                Started("osascript", ["-e", "delay 60", "-e", """tell application "System Events" to shut down"""], retain).Map(static _ => unit), DisposalOps.Release);

    private static IO<Process> Started(string program, Seq<string> arguments, Action<Process> retain) =>
        DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true }), nameof(Process.Start))), retain);
}
