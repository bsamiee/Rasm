using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Rhino;
using Rhino.Runtime.InProcess;
using RhinoInside;
using Xunit.MicrosoftTestingPlatform;

namespace Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class RhinoProcess {

    // --- [PROCESS]

    private static readonly ManualResetEventSlim Requesting = new();

    [STAThread]
    private static void Main(string[] args) {
        Resolver.Initialize();
        ImmutableArray<string> runtimes = [.. Directory.EnumerateDirectories(Path.GetFullPath(Path.Join(Resolver.RhinoSystemDirectory, "..", "Plug-ins")), "net*.*", new EnumerationOptions { RecurseSubdirectories = true, MatchType = MatchType.Simple })];
        AssemblyLoadContext.Default.Resolving += (context, name) =>
            runtimes
                .Select(folder => Path.Combine(folder, $"{name.Name}.dll"))
                .Where(File.Exists)
                .Select(context.LoadFromAssemblyPath)
                .FirstOrDefault();
        Task tests = Task.Run(async () => TerminateProcess(GetCurrentProcess(), (uint)await TestPlatformTestFramework.RunAsync(args, SelfRegisteredExtensions.AddSelfRegisteredExtensions).ConfigureAwait(false)), CancellationToken.None);
        WaitHandle.WaitAny([Requesting.WaitHandle, ((IAsyncResult)tests).AsyncWaitHandle]);
        if (Requesting.IsSet)
            Pumped(Booted, tests);
        ((IAsyncResult)tests).AsyncWaitHandle.WaitOne();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Pumped(TaskCompletionSource booted, Task tests) {
        RhinoCore core;
        try {
            core = new RhinoCore([], Mode is RhinoMode.Interface ? WindowStyle.Hidden : WindowStyle.NoWindow);
            Captured();
        } catch (Exception exception) {
            booted.SetException(exception);
            return;
        }
        booted.SetResult();
        while (!((IAsyncResult)tests).AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(10))) {
            core.DoEvents();
            Eto.Forms.Application.Instance.RunIteration();
        }
    }

    private static partial Task RequestedAsync() {
        Requesting.Set();
        return Task.CompletedTask;
    }

    public static partial RhinoDoc? Untitled() => RhinoDoc.Create(modelTemplateFileName: null);

    [LibraryImport("kernel32")]
    private static partial nint GetCurrentProcess();

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TerminateProcess(nint process, uint status);
}
