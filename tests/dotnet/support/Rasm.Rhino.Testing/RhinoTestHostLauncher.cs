using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;

namespace Rasm.Rhino.Testing;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class RhinoTestHostLauncher : ITestHostLauncher {
    // --- [IDENTITY]

    public string Uid => nameof(RhinoTestHostLauncher);

    public string Version { get; } = typeof(RhinoTestHostLauncher).Assembly.GetName().Version?.ToString() ?? "";

    public string DisplayName => "Rhino test host";

    public string Description => "Runs the test host inside a Rhino launched for this test application";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    // --- [LAUNCH]

    private const string Profile = "RHINO_TEST_PROFILE";

    private const string Install = "RhinoInstallPath";

    public Task<ITestHostHandle> LaunchTestHostAsync(TestHostLaunchContext context, CancellationToken cancellationToken) =>
        Launch(context).RunAsync(cancellationToken).AsTask();

    private static IO<ITestHostHandle> Launch(TestHostLaunchContext context) =>
        from start in IO.lift(() => (
                Optional(Environment.GetEnvironmentVariable(Profile)).ToFin(Error.New($"{Profile} names no activated Rhino profile home, set it in .config/mise.macos.toml and sign in once")),
                Optional(AppContext.GetData(Install) as string).ToFin(Error.New($"{Install} is absent from the runtime configuration, build the suite on a machine with Rhino installed")),
                Optional(Assembly.GetEntryAssembly()).ToFin(Error.New("The test application has no entry assembly")))
            .Apply((profile, rhino, tests) => new ProcessStartInfo("watch-apps", [
                "launch", rhino, "--new", "--wait",
                .. (toSeq(context.EnvironmentVariables)
                        .Filter(static pair => pair.Value is { } value && !string.Equals(value, Environment.GetEnvironmentVariable(pair.Key), StringComparison.Ordinal))
                        .Map(static pair => $"{pair.Key}={pair.Value}")
                    + Seq($"HOME={profile}", $"CFFIXED_USER_HOME={profile}", $"RHINO_TEST_ASSEMBLY={tests.Location}", $"RHINO_TEST_ARGUMENTS={JsonSerializer.Serialize(context.Arguments)}"))
                    .Bind(static variable => Seq("--environment", variable)),
                "--arguments",
                $"""
                -runscript=_-ScriptEditor _Run "{Path.Combine(AppContext.BaseDirectory, "Boot.cs")}"
                """,
            ]) { RedirectStandardOutput = true, UseShellExecute = false }).As())
        from process in IO.lift(Optional(Process.Start(start)).ToFin(Error.New("watch-apps started no process")))
        from _ in IO.lift(process.BeginOutputReadLine)
        select (ITestHostHandle)new Launched(process);

    private sealed class Launched(Process process) : ITestHostHandle, ITestHostHandleExitCodePolicy {
        public string? Identifier => null;

        public int ExitCode => process.ExitCode;

        public bool HasExited => process.HasExited;

        public bool IsExitCodeAuthoritative => false;

        public Task WaitForExitAsync(CancellationToken cancellationToken) => process.WaitForExitAsync(cancellationToken);

        public void Terminate() => Process.Start(new ProcessStartInfo("kill", ["-TERM", process.Id.ToString(CultureInfo.InvariantCulture)]) { UseShellExecute = false })?.Dispose();

        public void Dispose() => process.Dispose();
    }

    // --- [REGISTRATION]

    public static void AddExtensions(ITestApplicationBuilder builder, string[] _) =>
        builder.TestHostControllers.AddTestHostLauncher(static _ => new RhinoTestHostLauncher());
}
