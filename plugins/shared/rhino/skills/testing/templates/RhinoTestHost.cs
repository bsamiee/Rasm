using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Runtime;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(Tests.RhinoTestHost))]
[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Tests;

// --- [TYPES] ---------------------------------------------------------------------------

public enum RhinoMode {
    Headless = 0,
    Interface = 1,
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class RhinoTestHost : IAsyncLifetime, INotifyTestLifecycleAsync {

    // --- [CHECKS]

    private ImmutableArray<Exception> reported = [];
    private int alerted;

    private void Report(string source, Exception exception) => ImmutableInterlocked.Update(ref reported, static (held, raised) => held.Add(raised), exception);

    private async ValueTask SettleAsync(Action<string> write) {
        foreach (string line in await OnMainThread(static () => RhinoApp.CapturedCommandWindowStrings(clearBuffer: true)).ConfigureAwait(false))
            write(line);
        string[] alerts = Environment.GetEnvironmentVariable(RhinoProcess.Alerts) is { } path && File.Exists(path) ? await File.ReadAllLinesAsync(path).ConfigureAwait(false) : [];
        ImmutableArray<Exception> raised = ImmutableInterlocked.InterlockedExchange(ref reported, []).AddRange(alerts.Skip(alerted).Select(static alert => new InvalidOperationException(alert)));
        alerted = alerts.Length;
        if (!raised.IsEmpty)
            throw raised is [var one] ? one : new AggregateException(raised);
    }

    public async ValueTask InitializeAsync() {
        await RhinoProcess.BootAsync().ConfigureAwait(false);
        HostUtils.OnExceptionReport += Report;
        TestContext.Current.SendDiagnosticMessage($"Rhino {RhinoApp.Version} in {RhinoProcess.Mode} mode, license validated {RhinoApp.IsLicenseValidated}");
    }

    public async ValueTask DisposeAsync() {
        await SettleAsync(static line => TestContext.Current.SendDiagnosticMessage(line)).ConfigureAwait(false);
        HostUtils.OnExceptionReport -= Report;
    }

    public ValueTask OnTestStartingAsync(IXunitTest test) => SettleAsync(static line => TestContext.Current.SendDiagnosticMessage(line));

    public ValueTask OnTestFinishedAsync(IXunitTest test) => SettleAsync(static line => TestContext.Current.TestOutputHelper?.Write(line));

    // --- [MAIN_THREAD]

    public static Task<T> OnMainThread<T>(Func<T> work) {
        TaskCompletionSource<T> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RhinoApp.InvokeOnUiThread(() => {
            try {
                done.SetResult(work());
            } catch (Exception exception) {
                done.SetException(exception);
            }
        });
        return done.Task;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class RhinoProcess {

    // --- [PROCESS]

    internal const string Alerts = "RHINO_TEST_ALERTS";

    private static readonly TimeSpan Startup = TimeSpan.FromMinutes(2);
    private static readonly TaskCompletionSource Booted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static RhinoMode Mode { get; } = Enum.Parse<RhinoMode>(AppContext.GetData(nameof(RhinoMode)) as string ?? nameof(RhinoMode.Headless));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Captured() => RhinoApp.CommandWindowCaptureEnabled = true;

    internal static async Task BootAsync() {
        await RequestedAsync().ConfigureAwait(false);
        await Booted.Task.WaitAsync(Startup, TimeProvider.System, TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    private static partial Task RequestedAsync();

    public static partial RhinoDoc? Untitled();
}
