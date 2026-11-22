using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document.Files;
using Rasm.Testing;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(RhinoHost))]
[assembly: AssemblyFixture(typeof(Fixtures))]
[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Rasm.Rhino.Tests.Host;

// --- [TYPES] ---------------------------------------------------------------------------

public enum RhinoMode {
    Headless = 0,
    Interface = 1,
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class RhinoHost : IAsyncLifetime, INotifyTestLifecycleAsync {

    // --- [CHECKS]

    private int seen;
    private ImmutableArray<(string Source, Exception Fault)> faults = [];

    private void Reported(string source, Exception exception) => ImmutableInterlocked.Update(ref faults, static (held, raised) => held.Add(raised), (source, exception));

    private async ValueTask<Seq<(string Source, Exception Fault)>> DrainedAsync(Action<string> write) {
        toSeq((await RunSafeAsync(IO.lift(static () => RhinoApp.CapturedCommandWindowStrings(clearBuffer: true)).OnUiThread()).ConfigureAwait(false)).ThrowIfFail()).Iter(write);
        Seq<string> logged = Environment.GetEnvironmentVariable("RASM_RHINO_ALERTS") is { } path && File.Exists(path) ? toSeq(await File.ReadAllLinesAsync(path, CancellationToken.None).ConfigureAwait(false)) : [];
        Seq<(string Source, Exception Fault)> refused = logged.Skip(Interlocked.Exchange(ref seen, logged.Count)).Map(static alert => ("modal", (Exception)new InvalidOperationException(alert)));
        return toSeq(ImmutableInterlocked.InterlockedExchange(ref faults, [])) + refused;
    }

    public async ValueTask InitializeAsync() {
        await RhinoProcess.BootAsync().ConfigureAwait(false);
        HostUtils.OnExceptionReport += Reported;
        TestContext.Current.SendDiagnosticMessage($"Rhino {RhinoApp.Version} in {RhinoProcess.Mode} mode, license validated {RhinoApp.IsLicenseValidated}, headless {RhinoApp.IsRunningHeadless}");
    }

    public async ValueTask DisposeAsync() {
        (await DrainedAsync(static line => TestContext.Current.SendDiagnosticMessage(line)).ConfigureAwait(false))
            .Iter(static fault => TestContext.Current.SendDiagnosticMessage($"{fault.Source} after the last test: {fault.Fault}"));
        HostUtils.OnExceptionReport -= Reported;
    }

    public async ValueTask OnTestStartingAsync(IXunitTest test) =>
        (await DrainedAsync(static line => TestContext.Current.SendDiagnosticMessage(line)).ConfigureAwait(false))
            .Iter(static fault => TestContext.Current.AddWarning($"{fault.Source} raised between tests, before this one started: {fault.Fault.Message}"));

    public async ValueTask OnTestFinishedAsync(IXunitTest test) {
        if (await DrainedAsync(line => (TestContext.Current.TestOutputHelper ?? throw new InvalidOperationException($"{test.TestDisplayName} finished with no output helper")).Write(line)).ConfigureAwait(false) is { IsEmpty: false } raised)
            throw new AggregateException($"Rhino reported {string.Join(", ", raised.Map(static fault => fault.Source).Distinct())} during {test.TestDisplayName}", raised.Map(static fault => fault.Fault));
    }

    // --- [EFFECTS]

    public static Task<Fin<T>> RunSafeAsync<T>(IO<T> effect) => Session.RunSafeAsync(effect, TestContext.Current.CancellationToken);

    public static async Task<T> RunAsync<T>(IO<T> effect) => (await RunSafeAsync(effect).ConfigureAwait(false)).ThrowIfFail();

    // --- [DOCUMENTS]

    public static IO<Fixture<RhinoDoc>> Document { get; } =
        from doc in IO.lift(static () => Conversions.Required(RhinoProcess.Mode switch { RhinoMode.Headless => RhinoDoc.CreateHeadless(file3dmTemplatePath: null), RhinoMode.Interface => RhinoProcess.Untitled() })).OnUiThread()
        select new Fixture<RhinoDoc>(doc, result =>
            (from host in IO.liftVAsync(static () => TestContext.Current.GetFixture<RhinoHost>())
             from attached in when(result is TestResult.Failed || !host.faults.IsEmpty, Attached(doc)).As()
             select unit).Finally(Released(doc)).OnUiThread());

    public static IO<T> Headless<T>(Func<RhinoDoc, IO<T>> body) => Documents.With(new DocumentSource.Headless(None), doc => body(doc).OnUiThread());

    public static (bool Valid, string Log) Audit(RhinoDoc doc) {
        using TextLog log = new();
        return (doc.Audit(log, attemptRepair: false), log.ToString());
    }

    private static IO<Unit> Released(RhinoDoc doc) =>
        RhinoProcess.Mode switch {
            RhinoMode.Headless => IO.lift(doc.Dispose),
            RhinoMode.Interface => IO.lift(() => { doc.Modified = false; }),
        };

    private static IO<Unit> Attached(RhinoDoc doc) =>
        (from folder in use(static () => Directory.CreateTempSubdirectory(), static folder => IO.lift(() => folder.Delete(recursive: true)))
         let file = Path.Combine(folder.FullName, $"{doc.RuntimeSerialNumber}.3dm")
         from written in Products.Accepted(() => doc.Write3dmFile(file, new FileWriteOptions { SuppressDialogBoxes = true }))
         from attached in IO.lift(() => TestContext.Current.AddAttachment(Path.GetFileName(file), File.ReadAllBytes(file)))
         select unit).Bracket();
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Expect<E> where E : Error {
    public static async Task<E> FailAsync<T>(IO<T> effect) => Failed(await RhinoHost.RunSafeAsync(effect).ConfigureAwait(false));

    public static E Failed<T>(Fin<T> result) =>
        result switch {
            Fin<T>.Fail { Error: E fault } => fault,
            var other => throw FailException.ForFailure($"expected a {typeof(E).Name}, the effect gave {other}"),
        };
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class RhinoProcess {

    private static readonly TimeSpan Startup = TimeSpan.FromMinutes(2);
    private static readonly Atom<Option<TaskCompletionSource>> Started = Atom(Option<TaskCompletionSource>.None);

    public static RhinoMode Mode { get; } = AppContext.GetData(nameof(RhinoMode)) is string mode ? Enum.Parse<RhinoMode>(mode) : throw new InvalidOperationException($"runtime option {nameof(RhinoMode)} is absent, build through a configuration of the project");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Unit Captured() {
        RhinoApp.CommandWindowCaptureEnabled = true;
        return unit;
    }

    internal static async Task BootAsync() {
        TaskCompletionSource booted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Started.Swap(_ => Some(booted));
        await RequestedAsync().ConfigureAwait(false);
        await booted.Task.WaitAsync(Startup, TimeProvider.System, TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    private static partial Task RequestedAsync();

    internal static partial RhinoDoc? Untitled();

}
