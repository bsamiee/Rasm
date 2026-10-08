using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace Rasm.Rhino.Render.Queue;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MachinePower {
    // --- [HOLD]
    public static IO<T> HeldAwake<T>(IO<T> run) =>
        OperatingSystem.IsWindows()
            ? Executing(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired)
                .Bracket(Use: _ => IO.pure(unit).Bind(_ => run), Fin: static _ => Executing(ExecutionState.Continuous))
            : Started("/usr/bin/caffeinate", ["-i", "-w", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)])
                .Bracket(
                    Use: _ => run,
                    Fin: static caffeinate => IO.lift(() => {
                        caffeinate.Kill();
                        caffeinate.Dispose();
                    }));

    [SupportedOSPlatform("windows")]
    private static IO<Unit> Executing(ExecutionState flags) =>
        IO.lift(() => Refused.Unless(SafeNativeMethods.SetThreadExecutionState(flags) != ExecutionState.None, nameof(SafeNativeMethods.SetThreadExecutionState))).Post();

    // --- [SHUTDOWN]
    public static IO<Unit> ShutDown =>
        "60" switch {
            var seconds => OperatingSystem.IsWindows()
                ? use(Started("shutdown", ["/s", "/t", seconds]))
                    .Bind(static shutdown =>
                        from token in cancelToken
                        from exited in IO.liftAsync(async () => {
                            await shutdown.WaitForExitAsync(token).ConfigureAwait(false);
                            return unit;
                        })
                        from code in IO.lift(() => Refused.Unless(shutdown.ExitCode == 0, nameof(Process.ExitCode)))
                        select unit)
                    .Bracket()
                : use(Started("/usr/bin/osascript", ["-e", $"delay {seconds}", "-e", "tell application \"System Events\" to shut down"])).Map(static _ => unit).Bracket(),
        };

    private static IO<Process> Started(string program, Seq<string> arguments) =>
        IO.lift(() => Missing.Unless(Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true }), nameof(Process.Start)));
}
