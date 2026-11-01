using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Darkroom.Chain;
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

    // --- [ARGUMENTS]
    [LibraryImport("/usr/lib/libSystem.B.dylib")]
    [SupportedOSPlatform("macos")]
    private static partial nint _NSGetArgv();

    [LibraryImport("/usr/lib/libSystem.B.dylib")]
    [SupportedOSPlatform("macos")]
    private static partial nint _NSGetArgc();

    [SupportedOSPlatform("macos")]
    internal static string[] Arguments() =>
        [.. Enumerable.Range(0, Marshal.ReadInt32(_NSGetArgc())).Select(static index => Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(Marshal.ReadIntPtr(_NSGetArgv()), index * nint.Size))!)];

    // --- [MEMORY]
    [LibraryImport("/usr/lib/libSystem.B.dylib", SetLastError = true)]
    [SupportedOSPlatform("macos")]
    private static partial int getrusage(int who, Span<long> usage);

    [SupportedOSPlatform("macos")]
    internal static Fin<long> PeakResident() {
        Span<long> usage = stackalloc long[18];
        (int status, long peak) = (getrusage(0, usage), usage[4]);
        return DarkroomFault.Unless(status is 0, DarkroomReason.Failed, Marshal.GetLastPInvokeError()).Map(_ => peak);
    }
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
            var prior => DarkroomFault.Unless(prior is not ExecutionState.None, DarkroomReason.Failed, prior).Map(_ => prior),
        }).OnUiThread().Bind(prior => run.Finally(IO.lift(() => SafeNativeMethods.SetThreadExecutionState(prior) switch {
            var restored => DarkroomFault.Unless(restored is not ExecutionState.None, DarkroomReason.Failed, restored),
        }).OnUiThread()));

    // --- [SHUTDOWN]
    public static IO<Unit> ShutDown =>
        OperatingSystem.IsWindows()
            ? (from shutdown in Started("shutdown", ["/s", "/t", "60"])
               from exited in IO.liftAsync(environment => shutdown.WaitForExitAsync(environment.Token).ToUnit())
               from code in IO.lift(() => DarkroomFault.Unless(shutdown.ExitCode is 0, DarkroomReason.Failed, shutdown.ExitCode))
               select unit).Bracket()
            : Started("osascript", ["-e", "delay 60", "-e", """tell application "System Events" to shut down"""]).Map(static _ => unit).Bracket();

    // --- [PROCESSES]
    private static IO<Process> Started(string program, Seq<string> arguments) =>
        Products.Of(() => Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true }));

    internal static IO<Unit> Exited(int processId) =>
        (from process in Products.Of(() => Process.GetProcessById(processId))
         from waited in IO.liftAsync(environment => process.WaitForExitAsync(environment.Token).ToUnit())
         select waited).Bracket()
            .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.pure(unit));
}
