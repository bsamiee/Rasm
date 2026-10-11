using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;

namespace Rasm.Rhino.Render.Queue;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MachinePower {
    // --- [HOLD]
    public static IO<T> HeldAwake<T>(IO<T> run) =>
        OperatingSystem.IsWindows()
            ? HeldAwakeOnWindows(run)
            : DisposalOps.Within<Process, T>(retain =>
                Started("caffeinate", ["-i", "-w", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)], retain).Bind(_ => run),
                static processes => Callbacks.Each(processes.Rev().Map(static process => Callbacks.Each(Seq(
                    IO.lift(() => process.Kill()), IO.lift(process.Dispose))).Map(static _ => unit))).Map(static _ => unit));

    [SupportedOSPlatform("windows")]
    private static IO<T> HeldAwakeOnWindows<T>(IO<T> run) =>
        DisposalOps.Within<IDisposable, T>(retain =>
            DisposalOps.Acquire(
                IO.lift(static () => SafeNativeMethods.SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired) switch {
                    var prior => Refused.Unless(prior != ExecutionState.None, prior, nameof(SafeNativeMethods.SetThreadExecutionState)),
                }),
                Restored,
                prior => retain(new Disposal<ExecutionState>(prior, static held => Restored(held).Run())))
                .Post().Bind(_ => run), DisposalOps.Release);

    [SupportedOSPlatform("windows")]
    private static IO<Unit> Restored(ExecutionState flags) =>
        IO.lift(() => Refused.Unless(SafeNativeMethods.SetThreadExecutionState(flags) != ExecutionState.None, nameof(SafeNativeMethods.SetThreadExecutionState))).Post();

    // --- [SHUTDOWN]
    public static IO<Unit> ShutDown =>
        "60" switch {
            var seconds => OperatingSystem.IsWindows()
                ? DisposalOps.Within<Process, Unit>(retain => Started("shutdown", ["/s", "/t", seconds], retain)
                    .Bind(static shutdown =>
                        from token in cancelToken
                        from exited in IO.liftAsync(async () => {
                            await shutdown.WaitForExitAsync(token).ConfigureAwait(false);
                            return unit;
                        })
                        from code in IO.lift(() => Refused.Unless(shutdown.ExitCode == 0, nameof(Process.ExitCode)))
                        select unit), DisposalOps.Release)
                : DisposalOps.Within<Process, Unit>(retain => Started("osascript", ["-e", $"delay {seconds}", "-e", "tell application \"System Events\" to shut down"], retain).Map(static _ => unit), DisposalOps.Release),
        };

    private static IO<Process> Started(string program, Seq<string> arguments, Action<Process> retain) =>
        DisposalOps.Acquire(IO.lift(() => Optional(Process.Start(new ProcessStartInfo(program, arguments) { UseShellExecute = false, CreateNoWindow = true }))),
            static process => DisposalOps.Release(process.ToSeq()), process => process.Iter(retain))
            .Bind(static process => IO.lift(process.ToFin(new Missing(nameof(Process.Start)))));
}
