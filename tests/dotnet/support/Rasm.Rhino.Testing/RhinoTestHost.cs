using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;
using Microsoft.Testing.Platform.Services;

namespace Rasm.Rhino.Testing;

// --- [SERVICES] ------------------------------------------------------------------------

[SupportedOSPlatform("macos")]
public sealed class RhinoTestHost(IServiceProvider services) : ITestHostLauncher, ICommandLineOptionsProvider {
    // --- [IDENTITY]

    public string Uid => nameof(RhinoTestHost);

    public string Version { get; } = typeof(RhinoTestHost).Assembly.GetName().Version?.ToString() ?? throw new UnreachableException($"{nameof(RhinoTestHost)} loads from a versioned assembly");

    public string DisplayName => "Rhino test host";

    public string Description => "Runs the test host inside a Rhino started for the test application";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    // --- [OPTIONS]

    private static readonly CommandLineOption Profile = new("rhino-profile", "Home folder of a Rhino profile signed in once, the started Rhino's HOME", ArgumentArity.ExactlyOne, isHidden: false);

    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() => [Profile];

    public Task<ValidationResult> ValidateOptionArgumentsAsync(CommandLineOption commandOption, string[] arguments) =>
        Directory.Exists(arguments[0]) ? ValidationResult.ValidTask : ValidationResult.InvalidTask($"--{Profile.Name} '{arguments[0]}' names no folder");

    public Task<ValidationResult> ValidateCommandLineOptionsAsync(ICommandLineOptions commandLineOptions) =>
        commandLineOptions.IsOptionSet(Profile.Name) ? ValidationResult.ValidTask : ValidationResult.InvalidTask($"--{Profile.Name} is required, naming the home folder of a Rhino profile signed in once");

    // --- [LAUNCH]

    private const string Executable = "Rasm.Rhino.Testing.Executable";

    private string Home => services.GetCommandLineOptions().TryGetOptionArgumentList(Profile.Name, out string[]? home) ? home[0] : throw new InvalidOperationException($"--{Profile.Name} is unset");

    public Task<ITestHostHandle> LaunchTestHostAsync(TestHostLaunchContext context, CancellationToken cancellationToken) =>
        Task.Run<ITestHostHandle>(() => new Launched(Process.Start(Started(context))), cancellationToken);

    private ProcessStartInfo Started(TestHostLaunchContext context) {
        ProcessStartInfo start = new(AppContext.GetData(Executable) as string ?? throw new InvalidOperationException($"{Executable} is absent from the runtime configuration")) {
            ArgumentList = {
                "-batchmode",
                $"""
                -runscript=_-ScriptEditor _Run "{Path.Combine(AppContext.BaseDirectory, "Rasm.Rhino.Testing.Boot.cs")}"
                """,
            },
            WorkingDirectory = context.WorkingDirectory,
            UseShellExecute = false,
        };
        start.Environment.Clear();
        foreach ((string name, string? value) in context.EnvironmentVariables.Concat([new("HOME", Home), new("CFFIXED_USER_HOME", Home), new("RHINO_TEST_COMMAND", JsonSerializer.Serialize(context.Arguments.Prepend(context.FileName)))]))
            start.Environment[name] = value;
        return start;
    }

    private sealed class Launched(Process? started) : ITestHostHandle, ITestHostHandleExitCodePolicy {
        private readonly Process rhino = started ?? throw new InvalidOperationException("Rhino started no process");

        public string? Identifier => rhino.Id.ToString(CultureInfo.InvariantCulture);

        public int ExitCode => rhino.ExitCode;

        public bool HasExited => rhino.HasExited;

        public bool IsExitCodeAuthoritative => false;

        public Task WaitForExitAsync(CancellationToken cancellationToken) => rhino.WaitForExitAsync(cancellationToken);

        public void Terminate() => rhino.Kill();

        public void Dispose() => rhino.Dispose();
    }

    // --- [REGISTRATION]

    public static void AddExtensions(ITestApplicationBuilder builder, string[] _) {
        if (!OperatingSystem.IsMacOS()) return;
        builder.CommandLine.AddProvider(static services => new RhinoTestHost(services));
        builder.TestHostControllers.AddTestHostLauncher(static services => new RhinoTestHost(services));
    }
}
