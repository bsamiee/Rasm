using System.Runtime.CompilerServices;
using Rhino.Runtime.InProcess;
using RhinoInside;
using Xunit.MicrosoftTestingPlatform;
using Xunit.Runner.InProc.SystemConsole;

namespace Rasm.Rhino.Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------

internal static class RhinoHost {
    private static Task<int> Main(string[] args) {
        Resolver.Initialize();
        return HostedAsync(args);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<int> HostedAsync(string[] args) {
        using RhinoCore core = new();
        Task<Fin<int>> tests = Session.RunSafeAsync(
            IO.liftAsync(() => Task.Run(() => args.Any(static arg => arg is "--server" or "--internal-msbuild-node")
                    ? TestPlatformTestFramework.RunAsync(args, SelfRegisteredExtensions.AddSelfRegisteredExtensions)
                    : ConsoleRunner.Run(args)))
                .Finally(IO.lift(Eto.Forms.Application.Instance.Quit).OnUiThread()),
            CancellationToken.None);
        Eto.Forms.Application.Instance.Run();
        return (await tests.ConfigureAwait(false)).ThrowIfFail();
    }
}
