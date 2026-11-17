using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Runtime.InProcess;
using RhinoInside;
using Xunit.MicrosoftTestingPlatform;

namespace Rasm.Rhino.Tests.Host;

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class RhinoProcess {

    // --- [PROCESS]

    private static readonly ManualResetEventSlim Requesting = new();

    [STAThread]
    private static void Main(string[] args) {
        Resolver.Initialize();
        Task tests = Task.Run(async () => TerminateProcess(GetCurrentProcess(), (uint)await TestPlatformTestFramework.RunAsync(args, SelfRegisteredExtensions.AddSelfRegisteredExtensions).ConfigureAwait(false)), CancellationToken.None);
        WaitHandle.WaitAny([Requesting.WaitHandle, ((IAsyncResult)tests).AsyncWaitHandle]);
        Started.Value.Iter(booted => Try.lift(Cored).Run().Match(
            Succ: core => {
                booted.SetResult();
                while (!((IAsyncResult)tests).AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(10)))
                    core.DoEvents();
            },
            Fail: error => booted.SetException(error.ToException())));
        ((IAsyncResult)tests).AsyncWaitHandle.WaitOne();
        ExceptionDispatchInfo.Throw(tests.Exception?.InnerException ?? new InvalidOperationException(nameof(TerminateProcess)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RhinoCore Cored() {
        RhinoCore core = new([], Mode switch { RhinoMode.Headless => WindowStyle.NoWindow, RhinoMode.Interface => WindowStyle.Hidden });
        ignore(Captured());
        return core;
    }

    private static partial Task RequestedAsync() {
        Requesting.Set();
        return Task.CompletedTask;
    }

    internal static partial RhinoDoc? Untitled() => RhinoDoc.Create(modelTemplateFileName: null);

    [LibraryImport("kernel32")]
    private static partial nint GetCurrentProcess();

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TerminateProcess(nint process, uint status);

}
