using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.RhinoBridge.Client;

// --- [COMPOSITION] ----------------------------------------------------------------------
internal static class Program {
    private const string DefaultRhinoWipBundleId = "com.mcneel.rhinoceros.9";
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private static readonly TimeSpan LaunchWait = TimeSpan.FromSeconds(45.0);
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(5.0);
    public static async Task<int> Main(string[] args) {
        try {
            string command = args.Length > 0 ? args[0] : string.Empty;
            string[] rest = args.Length > 1 ? args[1..] : [];
            return command switch {
                "doctor" when rest.Length == 0 => await SendAndPrintAsync(request: BridgeWire.Request(command: BridgeWire.Doctor)).ConfigureAwait(false),
                "launch" when rest.Length == 0 => await LaunchAsync().ConfigureAwait(false),
                "load" when rest is [string assemblyPath, .. string[] options] => await LoadAsync(assemblyPath: assemblyPath, options: CliOptions.Parse(options)).ConfigureAwait(false),
                "run" when rest is [string sessionId, .. string[] options] => await RunProbeAsync(sessionId: sessionId, options: CliOptions.Parse(options)).ConfigureAwait(false),
                "unload" when rest is [string sessionId] => await SendAndPrintAsync(request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId))).ConfigureAwait(false),
                "check" when rest is [string projectPath, .. string[] options] => await CheckAsync(projectPath: projectPath, options: CliOptions.Parse(options)).ConfigureAwait(false),
                _ => Usage(),
            };
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException) {
            return Fail(message: error.Message);
        }
    }
    private static int Usage() {
        Console.Error.WriteLine("Usage: rhino-bridge-client doctor | launch | load <assembly> [--worktree <path>] [--timeout-ms <ms>] | run <session> [--probe <id>] [--args <json>] [--timeout-ms <ms>] | unload <session> | check <project.csproj> [--probe <id>] [--args <json>] [--configuration <name>] [--worktree <path>] [--timeout-ms <ms>]");
        Console.Error.WriteLine("Launch env: RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app or RHINO_WIP_BUNDLE_ID=com.mcneel.rhinoceros.9");
        return 2;
    }
    private static async Task<int> LaunchAsync() {
        string? appPath = Environment.GetEnvironmentVariable("RHINO_WIP_APP_PATH");
        string bundleId = Environment.GetEnvironmentVariable("RHINO_WIP_BUNDLE_ID") ?? DefaultRhinoWipBundleId;
        ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: string.IsNullOrWhiteSpace(appPath) ? ["-b", bundleId] : [appPath], timeout: TimeSpan.FromSeconds(30.0)).ConfigureAwait(false);
        if (opened.ExitCode != 0) {
            return Fail(message: $"Failed to open RhinoWIP.{Environment.NewLine}{opened.Output}");
        }
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(LaunchWait);
        Exception? lastFailure = null;
        while (DateTimeOffset.UtcNow < deadline) {
            try {
                BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeSpan.FromSeconds(5.0)).ConfigureAwait(false);
                Print(reply: reply);
                return ExitCode(reply: reply);
            } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
                lastFailure = error;
                await Task.Delay(delay: TimeSpan.FromMilliseconds(500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }
        return Fail(message: $"RhinoWIP opened, but no live bridge answered at {BridgeWire.EndpointPath}. Last bridge error: {lastFailure?.Message ?? "none"}. In Rhino, run RasmBridgeStatus or RasmBridgeStart.");
    }
    private static async Task<int> LoadAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        return await SendAndPrintAsync(request: BridgeWire.Request(
            command: BridgeWire.Load,
            payload: BridgeWire.LoadRequest(assemblyPath: assembly, workspaceRoot: workspaceRoot),
            timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
    }
    private static async Task<int> RunProbeAsync(string sessionId, CliOptions options) =>
        await SendAndPrintAsync(request: BridgeWire.Request(
            command: BridgeWire.Run,
            payload: BridgeWire.RunRequest(sessionId: sessionId, probe: options.Probe, arguments: options.Arguments),
            timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
    private static async Task<int> CheckAsync(string projectPath, CliOptions options) {
        string project = ExistingFile(path: projectPath, label: "project");
        string worktree = await WorkspaceRootAsync(path: project, options: options).ConfigureAwait(false);
        string assembly = await BuildTargetAsync(project: project, configuration: options.Configuration).ConfigureAwait(false);
        BridgeReply loaded = await SendAsync(
            request: BridgeWire.Request(command: BridgeWire.Load, payload: BridgeWire.LoadRequest(assemblyPath: assembly, workspaceRoot: worktree), timeoutMs: options.TimeoutMs),
            timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        Print(reply: loaded);
        return loaded switch {
            { Status: BridgeWire.Ok, Load.SessionId: string sessionId } => await RunCheckAsync(sessionId: sessionId, options: options).ConfigureAwait(false),
            { Status: BridgeWire.Ok } => Fail(message: "Bridge load returned ok without a session id."),
            _ => ExitCode(reply: loaded),
        };
    }
    private static async Task<int> RunCheckAsync(string sessionId, CliOptions options) {
        BridgeReply? run = null;
        ExceptionDispatchInfo? failure = null;
        try {
            run = await SendAsync(
                request: BridgeWire.Request(
                    command: BridgeWire.Run,
                    payload: BridgeWire.RunRequest(sessionId: sessionId, probe: options.Probe, arguments: options.Arguments),
                    timeoutMs: options.TimeoutMs),
                timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
            Print(reply: run);
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            failure = ExceptionDispatchInfo.Capture(error);
        }
        BridgeReply unloaded = await UnloadCleanupAsync(sessionId: sessionId).ConfigureAwait(false);
        failure?.Throw();
        int runExit = run is null ? 1 : ExitCode(reply: run);
        int unloadExit = ExitCode(reply: unloaded);
        return runExit == 0 ? unloadExit : runExit;
    }
    private static async Task<BridgeReply> UnloadCleanupAsync(string sessionId) {
        try {
            BridgeReply unloaded = await SendAsync(
                request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId)),
                timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
            if (ExitCode(reply: unloaded) != 0) {
                Print(reply: unloaded);
            }
            return unloaded;
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            BridgeReply failed = BridgeReply.Rejected(command: BridgeWire.Unload, status: BridgeWire.Failed, fault: BridgeFault.FromException(category: "cleanup", error: error));
            Print(reply: failed);
            return failed;
        }
    }
    private static async Task<string> BuildTargetAsync(string project, string configuration) {
        ProcessResult build = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["build", project, "--configuration", configuration], timeout: ProcessTimeout).ConfigureAwait(false);
        if (build.ExitCode != 0) {
            throw new InvalidOperationException($"dotnet build failed for {project}{Environment.NewLine}{build.Output}");
        }
        ProcessResult target = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["msbuild", project, "-getProperty:TargetPath", $"-p:Configuration={configuration}"], timeout: ProcessTimeout).ConfigureAwait(false);
        if (target.ExitCode != 0) {
            throw new InvalidOperationException($"dotnet msbuild TargetPath failed for {project}{Environment.NewLine}{target.Output}");
        }
        string path = target.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault()
            ?? throw new InvalidOperationException($"MSBuild did not return TargetPath for {project}.");
        return ExistingFile(path: path, label: "MSBuild TargetPath");
    }
    private static string ExistingFile(string path, string label) {
        string fullPath = Path.GetFullPath(path);
        return File.Exists(fullPath) ? fullPath : throw new InvalidOperationException($"{label} does not exist: {fullPath}");
    }
    private static string ExistingDirectory(string path) {
        string fullPath = Path.GetFullPath(path);
        return Directory.Exists(fullPath) ? fullPath : throw new InvalidOperationException($"worktree does not exist: {fullPath}");
    }
    private static async Task<string> WorkspaceRootAsync(string path, CliOptions options) =>
        options.Worktree is string explicitRoot ? ExistingDirectory(path: explicitRoot) : await WorktreeAsync(path: path).ConfigureAwait(false);
    private static async Task<string> WorktreeAsync(string path) {
        string directory = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
        ProcessResult git = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", directory, "rev-parse", "--show-toplevel"], timeout: TimeSpan.FromSeconds(30.0)).ConfigureAwait(false);
        return git.ExitCode switch {
            0 => git.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? directory,
            _ => directory,
        };
    }
    private static async Task<int> SendAndPrintAsync(BridgeRequest request) {
        BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout(timeoutMs: request.TimeoutMs)).ConfigureAwait(false);
        Print(reply: reply);
        return ExitCode(reply: reply);
    }
    private static async Task<BridgeReply> SendAsync(BridgeRequest request, TimeSpan timeout) {
        using CancellationTokenSource cancellation = new(timeout);
        BridgeEndpoint endpoint = ReadEndpoint();
        using NamedPipeClientStream pipe = new(serverName: ".", pipeName: endpoint.PipeName, direction: PipeDirection.InOut, options: PipePolicy);
        await pipe.ConnectAsync(cancellation.Token).ConfigureAwait(false);
        StreamWriter writer = new(pipe, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            await writer.WriteLineAsync(JsonSerializer.Serialize(value: request, options: BridgeWire.CompactJson).AsMemory(), cancellation.Token).ConfigureAwait(false);
            await writer.FlushAsync(cancellation.Token).ConfigureAwait(false);
            string? line = await reader.ReadLineAsync(cancellation.Token).ConfigureAwait(false);
            BridgeReply reply = string.IsNullOrWhiteSpace(line)
                ? throw new InvalidOperationException("Bridge returned no response.")
                : JsonSerializer.Deserialize<BridgeReply>(json: line, options: BridgeWire.CompactJson) ?? throw new InvalidOperationException("Bridge returned an invalid response.");
            return BridgeWire.IsCurrent(schema: reply.Schema)
                ? reply
                : throw new InvalidOperationException($"Bridge returned unsupported schema '{reply.Schema}'.");
        }
    }
    private static BridgeEndpoint ReadEndpoint() {
        BridgeEndpoint endpoint = JsonSerializer.Deserialize<BridgeEndpoint>(File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8), BridgeWire.CompactJson)
            ?? throw new InvalidOperationException($"Endpoint metadata is invalid: {BridgeWire.EndpointPath}");
        if (!BridgeWire.IsCurrent(schema: endpoint.Schema)) {
            throw new InvalidOperationException($"Endpoint metadata has unsupported schema '{endpoint.Schema}': {BridgeWire.EndpointPath}");
        }
        using Process process = Process.GetProcessById(endpoint.RhinoPid);
        DateTimeOffset startedAt = new(dateTime: process.StartTime.ToUniversalTime());
        bool validPipe = endpoint.PipeName.Length <= 64 && endpoint.PipeName.StartsWith(value: $"rb-{endpoint.RhinoPid}-", comparisonType: StringComparison.Ordinal);
        return (process.HasExited, validPipe, Math.Abs((startedAt - endpoint.RhinoStartedAt).TotalSeconds) <= 2.0) switch {
            (false, true, true) => endpoint,
            _ => throw new InvalidOperationException($"Endpoint metadata is stale or unsupported: {BridgeWire.EndpointPath}"),
        };
    }
    private static TimeSpan TransportTimeout(int timeoutMs) =>
        TimeSpan.FromMilliseconds(Math.Clamp(value: timeoutMs, min: 1, max: 300000) + 5000);
    private static void Print(BridgeReply reply) =>
        Console.WriteLine(JsonSerializer.Serialize(value: reply, options: BridgeWire.PrettyJson));
    private static int ExitCode(BridgeReply reply) =>
        reply.Status switch {
            BridgeWire.Ok => 0,
            BridgeWire.Unsupported => 3,
            BridgeWire.Busy or BridgeWire.Timeout => 5,
            _ => 1,
        };
    private static int Fail(string message) {
        Console.Error.WriteLine(message);
        return 1;
    }
}

// --- [MODELS] ---------------------------------------------------------------------------
internal sealed record CliOptions(string? Probe, string? Worktree, string Configuration, int TimeoutMs, JsonElement Arguments) {
    private static CliOptions Default =>
        new(Probe: null, Worktree: null, Configuration: Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Release", TimeoutMs: 30000, Arguments: JsonSerializer.SerializeToElement(new { }, BridgeWire.CompactJson));
    internal static CliOptions Parse(IReadOnlyList<string> args) =>
        Parse(args: args, index: 0, current: Default);
    private static CliOptions Parse(IReadOnlyList<string> args, int index, CliOptions current) =>
        index >= args.Count
            ? current
            : args[index] switch {
                "--probe" => Parse(args: args, index: index + 2, current: current with { Probe = Value(args: args, index: index, option: "--probe") }),
                "--worktree" => Parse(args: args, index: index + 2, current: current with { Worktree = Value(args: args, index: index, option: "--worktree") }),
                "--configuration" => Parse(args: args, index: index + 2, current: current with { Configuration = Value(args: args, index: index, option: "--configuration") }),
                "--timeout-ms" => Parse(args: args, index: index + 2, current: current with { TimeoutMs = ParseTimeout(value: Value(args: args, index: index, option: "--timeout-ms")) }),
                "--args" => Parse(args: args, index: index + 2, current: current with { Arguments = ParseArguments(value: Value(args: args, index: index, option: "--args")) }),
                string unknown when unknown.StartsWith(value: "--", comparisonType: StringComparison.Ordinal) => throw new InvalidOperationException($"Unknown bridge option: {unknown}"),
                string value => throw new InvalidOperationException($"Unexpected bridge argument: {value}"),
            };
    private static string Value(IReadOnlyList<string> args, int index, string option) =>
        (index + 1) < args.Count ? args[index + 1] : throw new InvalidOperationException($"Missing value for {option}.");
    private static JsonElement ParseArguments(string value) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: value);
            return document.RootElement.Clone();
        } catch (JsonException error) {
            throw new InvalidOperationException($"Invalid --args JSON: {error.Message}", error);
        }
    }
    private static int ParseTimeout(string value) =>
        int.TryParse(value, CultureInfo.InvariantCulture, out int parsed) && parsed > 0
            ? parsed
            : throw new InvalidOperationException($"Invalid --timeout-ms value: {value}");
}

internal sealed record ProcessResult(int ExitCode, string Output) {
    internal static async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, TimeSpan timeout) {
        ProcessStartInfo start = new() {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments) {
            start.ArgumentList.Add(argument);
        }
        using Process process = Process.Start(start) ?? throw new InvalidOperationException($"Failed to start process: {fileName}");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        Task<string> stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
        using CancellationTokenSource cancellation = new(timeout);
        try {
            await process.WaitForExitAsync(cancellation.Token).ConfigureAwait(false);
        } catch (OperationCanceledException error) {
            Kill(process: process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            string output = string.Concat(await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false));
            throw new TimeoutException($"Process timed out after {timeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)}s: {fileName} {string.Join(separator: " ", values: arguments)}{Environment.NewLine}{output}", error);
        }
        return new(ExitCode: process.ExitCode, Output: string.Concat(await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false)));
    }
    private static void Kill(Process process) {
        try {
            process.Kill(entireProcessTree: true);
        } catch (Exception error) when (error is InvalidOperationException or Win32Exception) {
        }
    }
}
