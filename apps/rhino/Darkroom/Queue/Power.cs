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
        [.. toSeq(Enumerable.Range(0, Marshal.ReadInt32(_NSGetArgc()))).Choose(static index => Optional(Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(Marshal.ReadIntPtr(_NSGetArgv()), index * nint.Size))))];

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

internal static class MachinePower {

    // --- [HOLDS]

    public static IO<T> HeldAwake<T>(IO<T> run) =>
        OperatingSystem.IsWindows()
            ? HeldAwakeOnWindows(run)
            : (from caffeinate in use(Started("caffeinate", ["-i", "-w", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)]), static process => {
                using (process) process.Kill();
            })
               from value in run
               select value).Bracket();

    [SupportedOSPlatform("windows")]
    private static IO<T> HeldAwakeOnWindows<T>(IO<T> run) =>
        (from prior in IO.lift(static () => SafeNativeMethods.SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired) switch {
            var previous => DarkroomFault.Unless(previous is not ExecutionState.None, DarkroomReason.Failed, previous).Map(_ => previous),
        }).OnUiThread()
         from released in Products.Released(IO.lift(() => SafeNativeMethods.SetThreadExecutionState(prior) switch {
             var restored => DarkroomFault.Unless(restored is not ExecutionState.None, DarkroomReason.Failed, restored),
         }).OnUiThread())
         from value in run
         select value).Bracket();

    // --- [SHUTDOWN]

    public static IO<Unit> ShutDown =>
        OperatingSystem.IsWindows()
            ? (from shutdown in use(Started("shutdown", ["/s", "/t", "60"]))
               from exited in IO.liftAsync(environment => shutdown.WaitForExitAsync(environment.Token).ToUnit())
               from code in IO.lift(DarkroomFault.Unless(shutdown.ExitCode is 0, DarkroomReason.Failed, shutdown.ExitCode))
               select unit).Bracket()
            : use(Started("osascript", ["-e", "delay 60", "-e", """tell application "System Events" to shut down"""])).Map(static _ => unit).Bracket();

    // --- [PROCESSES]

    private static IO<Process> Started(string program, Seq<string> arguments) =>
        IO.lift(() => Conversions.Required(Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true })));

    internal static IO<Unit> Exited(int processId) =>
        (from process in Products.Of(() => Process.GetProcessById(processId))
         from waited in IO.liftAsync(environment => process.WaitForExitAsync(environment.Token).ToUnit())
         select waited).Bracket()
            | @catch(static error => error.HasException<ArgumentException>(), static _ => IO.pure(unit));
}
