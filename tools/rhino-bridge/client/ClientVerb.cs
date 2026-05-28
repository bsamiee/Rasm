using Rasm.RhinoBridge.Protocol;

namespace Rasm.RhinoBridge.Client;

// --- [MODELS] ---------------------------------------------------------------------------
[Union]
internal abstract partial record CheckTarget {
    internal sealed record Script(string Path) : CheckTarget;
    internal sealed record Project(string Path, Option<string> ScenarioPath) : CheckTarget;
    internal sealed record Source(string Path, Option<string> ScenarioPath) : CheckTarget;
    internal static Fin<CheckTarget> From(string targetPath, Option<string> scenarioPath) =>
        Path.GetExtension(path: targetPath).ToUpperInvariant() switch {
            ".CSPROJ" => Fin.Succ<CheckTarget>(value: new Project(Path: targetPath, ScenarioPath: scenarioPath)),
            ".CSX" => scenarioPath.IsSome
                ? Fin.Fail<CheckTarget>(error: Error.New(message: "Script targets do not accept a second scenario path."))
                : Fin.Succ<CheckTarget>(value: new Script(Path: targetPath)),
            ".CS" => Fin.Succ<CheckTarget>(value: new Source(Path: targetPath, ScenarioPath: scenarioPath)),
            string extension => Fin.Fail<CheckTarget>(error: Error.New(message: $"Unsupported check target extension '{extension}': {targetPath}")),
        };
}

[Union]
internal abstract partial record ClientVerb {
    internal sealed record Doctor(CliOptions Options) : ClientVerb;
    internal sealed record Launch : ClientVerb;
    internal sealed record Check(CheckTarget Target, CliOptions Options) : ClientVerb;
    internal sealed record Clean(string TargetPath) : ClientVerb;
    internal sealed record Quit : ClientVerb;
    private static readonly (string Name, string Synopsis)[] Synopses = [
        ("doctor", "doctor [--result <path>]"),
        ("launch", "launch"),
        ("check", "check <target> [scenario.csx|scenario.verify.csx] [--result <path>]"),
        ("clean", "clean <target>"),
        ("quit", "quit"),
    ];
    internal string FailurePhase => Switch(
        doctor: static _ => BridgeWire.Doctor,
        launch: static _ => Program.PhaseLaunch,
        check: static _ => Program.PhaseResolve,
        clean: static _ => Program.PhaseClean,
        quit: static _ => Program.PhaseLifecycle);
    internal Task<int> RunAsync() => Switch(
        doctor: static d => Program.DoctorAsync(options: d.Options),
        launch: static _ => Program.LaunchAsync(),
        check: static c => Program.CheckAsync(target: c.Target, options: c.Options),
        clean: static c => Program.CleanAsync(targetPath: c.TargetPath),
        quit: static _ => Program.QuitAsync());
    internal static Fin<ClientVerb> Parse(string[] args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        return args switch {
            { Length: 0 } => Fin.Fail<ClientVerb>(error: Error.New(message: "Bridge command missing.")),
            _ => args[0] switch {
                "doctor" => Fin.Succ<ClientVerb>(value: new Doctor(Options: CliOptions.Parse(args: args[1..]))),
                "launch" when args.Length == 1 => Fin.Succ<ClientVerb>(value: new Launch()),
                "check" when args.Length >= 2 => ParseCheck(args: args[1..]),
                "clean" when args.Length == 2 => Fin.Succ<ClientVerb>(value: new Clean(TargetPath: args[1])),
                "quit" when args.Length == 1 => Fin.Succ<ClientVerb>(value: new Quit()),
                string verb => Fin.Fail<ClientVerb>(error: Error.New(message: $"Unsupported bridge command '{verb}'.")),
            },
        };
    }
    private static Fin<ClientVerb> ParseCheck(string[] args) {
        CliInvocation invocation = CliOptions.ParseInvocation(args: args);
        Fin<CheckTarget> target = invocation.Positionals.Count switch {
            1 => CheckTarget.From(targetPath: invocation.Positionals[0], scenarioPath: Option<string>.None),
            2 => CheckTarget.From(targetPath: invocation.Positionals[0], scenarioPath: Some(value: invocation.Positionals[1])),
            _ => Fin.Fail<CheckTarget>(error: Error.New(message: "Usage: check <target> [scenario.csx|scenario.verify.csx] [--result <path>]")),
        };
        return target.Map<ClientVerb>(f: t => new Check(Target: t, Options: invocation.Options));
    }
    internal static int Usage() {
        Console.Error.WriteLine(value: "Usage:");
        foreach ((_, string synopsis) in Synopses) {
            Console.Error.WriteLine(value: $"  rhino-bridge-client {synopsis}");
        }
        Console.Error.WriteLine(value: "Launch env: RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app or RHINO_WIP_BUNDLE_ID=com.mcneel.rhinoceros.9");
        return 2;
    }
}

internal sealed record CliOptions(string? Result) {
    private static CliOptions Default => new(Result: null);
    internal static CliOptions Parse(IReadOnlyList<string> args) {
        CliInvocation invocation = ParseInvocation(args: args);
        return invocation.Positionals.Count == 0
            ? invocation.Options
            : throw new InvalidOperationException(message: $"Unexpected bridge argument: {invocation.Positionals[0]}");
    }
    internal static CliInvocation ParseInvocation(IReadOnlyList<string> args) =>
        ParseInvocation(args: args, index: 0, current: Default, positionals: []);
    private static CliInvocation ParseInvocation(IReadOnlyList<string> args, int index, CliOptions current, IReadOnlyList<string> positionals) =>
        index >= args.Count
            ? new(Options: current, Positionals: positionals)
            : args[index] switch {
                "--result" => ParseInvocation(args: args, index: index + 2, current: current with { Result = Value(args: args, index: index, option: "--result") }, positionals: positionals),
                string unknown when unknown.StartsWith(value: "--", comparisonType: StringComparison.Ordinal) => throw new InvalidOperationException(message: $"Unknown bridge option: {unknown}"),
                string value => ParseInvocation(args: args, index: index + 1, current: current, positionals: [.. positionals, value]),
            };
    private static string Value(IReadOnlyList<string> args, int index, string option) =>
        (index + 1) < args.Count ? args[index + 1] : throw new InvalidOperationException(message: $"Missing value for {option}.");
}

internal sealed record CliInvocation(CliOptions Options, IReadOnlyList<string> Positionals);
