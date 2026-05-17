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
    public static async Task<int> Main(string[] args) {
        try {
            return args switch {
                ["doctor"] => await SendAndPrintAsync(request: BridgeWire.Request(command: BridgeWire.Doctor)).ConfigureAwait(false),
                ["launch"] => await LaunchAsync().ConfigureAwait(false),
                ["load", string assemblyPath, .. var rest] => await LoadAsync(assemblyPath: assemblyPath, options: CliOptions.Parse(rest)).ConfigureAwait(false),
                ["run", string sessionId, .. var rest] => await RunAsync(sessionId: sessionId, options: CliOptions.Parse(rest)).ConfigureAwait(false),
                ["unload", string sessionId] => await SendAndPrintAsync(request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId))).ConfigureAwait(false),
                ["check", string projectPath, .. var rest] => await CheckAsync(projectPath: projectPath, options: CliOptions.Parse(rest)).ConfigureAwait(false),
                _ => Usage(),
            };
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            return Fail(message: error.Message);
        }
    }
    private static int Usage() {
        Console.Error.WriteLine("Usage: rhino-bridge-client doctor | launch | load <assembly> [--worktree <path>] | run <session> [--probe <id>] [--args <json>] | unload <session> | check <project.csproj> [--probe <id>] [--args <json>] [--configuration <name>]");
        return 2;
    }
    private static async Task<int> LaunchAsync() {
        string? appPath = Environment.GetEnvironmentVariable("RHINO_WIP_APP_PATH");
        string bundleId = Environment.GetEnvironmentVariable("RHINO_WIP_BUNDLE_ID") ?? DefaultRhinoWipBundleId;
        ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: string.IsNullOrWhiteSpace(appPath) ? ["-b", bundleId] : [appPath]).ConfigureAwait(false);
        if (opened.ExitCode != 0) {
            return Fail(message: $"Failed to open RhinoWIP.{Environment.NewLine}{opened.Output}");
        }
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(45.0);
        while (DateTimeOffset.UtcNow < deadline) {
            try {
                BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeSpan.FromSeconds(5.0)).ConfigureAwait(false);
                Print(reply: reply);
                return ExitCode(reply: reply);
            } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
                await Task.Delay(delay: TimeSpan.FromMilliseconds(500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }
        return Fail(message: $"RhinoWIP launch command completed, but no live bridge answered at {BridgeWire.EndpointPath}.");
    }
    private static async Task<int> LoadAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = options.Worktree ?? await WorktreeAsync(path: assembly).ConfigureAwait(false);
        return await SendAndPrintAsync(request: BridgeWire.Request(
            command: BridgeWire.Load,
            payload: BridgeWire.LoadRequest(assemblyPath: assembly, workspaceRoot: workspaceRoot),
            timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
    }
    private static async Task<int> RunAsync(string sessionId, CliOptions options) =>
        await SendAndPrintAsync(request: BridgeWire.Request(
            command: BridgeWire.Run,
            payload: BridgeWire.RunRequest(sessionId: sessionId, probe: options.Probe, arguments: options.Arguments),
            timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
    private static async Task<int> CheckAsync(string projectPath, CliOptions options) {
        string project = Path.GetFullPath(projectPath);
        string worktree = options.Worktree ?? await WorktreeAsync(path: project).ConfigureAwait(false);
        string assembly = await BuildTargetAsync(project: project, configuration: options.Configuration).ConfigureAwait(false);
        BridgeReply loaded = await SendAsync(
            request: BridgeWire.Request(command: BridgeWire.Load, payload: BridgeWire.LoadRequest(assemblyPath: assembly, workspaceRoot: worktree), timeoutMs: options.TimeoutMs),
            timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        Print(reply: loaded);
        return loaded.Status switch {
            BridgeWire.Ok when loaded.Load is { SessionId: string sessionId } => await RunCheckAsync(sessionId: sessionId, options: options).ConfigureAwait(false),
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
        await TryUnloadAsync(sessionId: sessionId).ConfigureAwait(false);
        failure?.Throw();
        return run is null ? 1 : ExitCode(reply: run);
    }
    private static async Task TryUnloadAsync(string sessionId) {
        try {
            BridgeReply unloaded = await SendAsync(
                request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId)),
                timeout: TimeSpan.FromSeconds(15.0)).ConfigureAwait(false);
            if (ExitCode(reply: unloaded) != 0) {
                Print(reply: unloaded);
            }
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            await Console.Error.WriteLineAsync($"bridge unload cleanup failed for session {sessionId}: {error.Message}").ConfigureAwait(false);
        }
    }
    private static async Task<string> BuildTargetAsync(string project, string configuration) {
        ProcessResult build = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["build", project, "--configuration", configuration]).ConfigureAwait(false);
        if (build.ExitCode != 0) {
            throw new InvalidOperationException($"dotnet build failed for {project}{Environment.NewLine}{build.Output}");
        }
        ProcessResult target = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["msbuild", project, "-getProperty:TargetPath", $"-p:Configuration={configuration}"]).ConfigureAwait(false);
        if (target.ExitCode != 0) {
            throw new InvalidOperationException($"dotnet msbuild TargetPath failed for {project}{Environment.NewLine}{target.Output}");
        }
        string path = target.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault()
            ?? throw new InvalidOperationException($"MSBuild did not return TargetPath for {project}.");
        return File.Exists(path) switch {
            true => path,
            false => throw new InvalidOperationException($"MSBuild TargetPath does not exist: {path}"),
        };
    }
    private static string ExistingFile(string path, string label) {
        string fullPath = Path.GetFullPath(path);
        return File.Exists(fullPath) ? fullPath : throw new InvalidOperationException($"{label} does not exist: {fullPath}");
    }
    private static async Task<string> WorktreeAsync(string path) {
        ProcessResult git = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory(), "rev-parse", "--show-toplevel"]).ConfigureAwait(false);
        return git.ExitCode switch {
            0 => git.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? Directory.GetCurrentDirectory(),
            _ => Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory(),
        };
    }
    private static async Task<int> SendAndPrintAsync(BridgeRequest request) {
        BridgeReply reply = await SendAsync(request: request, timeout: TimeSpan.FromMilliseconds(request.TimeoutMs)).ConfigureAwait(false);
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
            return string.IsNullOrWhiteSpace(line)
                ? throw new InvalidOperationException("Bridge returned no response.")
                : JsonSerializer.Deserialize<BridgeReply>(json: line, options: BridgeWire.CompactJson) ?? throw new InvalidOperationException("Bridge returned an invalid response.");
        }
    }
    private static BridgeEndpoint ReadEndpoint() {
        BridgeEndpoint endpoint = JsonSerializer.Deserialize<BridgeEndpoint>(File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8), BridgeWire.CompactJson)
            ?? throw new InvalidOperationException($"Endpoint metadata is invalid: {BridgeWire.EndpointPath}");
        using Process process = Process.GetProcessById(endpoint.RhinoPid);
        DateTimeOffset startedAt = new(dateTime: process.StartTime.ToUniversalTime());
        bool validPipe = endpoint.PipeName.Length <= 64 && endpoint.PipeName.StartsWith(value: $"rb-{endpoint.RhinoPid}-", comparisonType: StringComparison.Ordinal);
        return (endpoint.Schema, process.HasExited, validPipe, Math.Abs((startedAt - endpoint.RhinoStartedAt).TotalSeconds) <= 2.0) switch {
            (BridgeWire.Schema, false, true, true) => endpoint,
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
    internal static CliOptions Parse(IReadOnlyList<string> args) {
        string? probe = null;
        string? worktree = null;
        string configuration = Environment.GetEnvironmentVariable("CONFIGURATION") ?? "Release";
        int timeoutMs = 30000;
        JsonElement arguments = JsonSerializer.SerializeToElement(new { }, BridgeWire.CompactJson);
        for (int index = 0; index < args.Count; index++) {
            (probe, worktree, configuration, timeoutMs, arguments, index) = args[index] switch {
                "--probe" when index + 1 < args.Count => (args[index + 1], worktree, configuration, timeoutMs, arguments, index + 1),
                "--worktree" when index + 1 < args.Count => (probe, args[index + 1], configuration, timeoutMs, arguments, index + 1),
                "--configuration" when index + 1 < args.Count => (probe, worktree, args[index + 1], timeoutMs, arguments, index + 1),
                "--timeout-ms" when index + 1 < args.Count => (probe, worktree, configuration, ParseTimeout(value: args[index + 1]), arguments, index + 1),
                "--args" when index + 1 < args.Count => (probe, worktree, configuration, timeoutMs, JsonDocument.Parse(args[index + 1]).RootElement.Clone(), index + 1),
                string unknown when unknown.StartsWith(value: "--", comparisonType: StringComparison.Ordinal) => throw new InvalidOperationException($"Unknown bridge option: {unknown}"),
                string value => throw new InvalidOperationException($"Unexpected bridge argument: {value}"),
            };
        }
        return new(Probe: probe, Worktree: worktree, Configuration: configuration, TimeoutMs: timeoutMs, Arguments: arguments);
    }
    private static int ParseTimeout(string value) =>
        int.TryParse(value, CultureInfo.InvariantCulture, out int parsed) && parsed > 0
            ? parsed
            : throw new InvalidOperationException($"Invalid --timeout-ms value: {value}");
}

internal sealed record ProcessResult(int ExitCode, string Output) {
    internal static async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments) {
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
        await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        return new(ExitCode: process.ExitCode, Output: string.Concat(await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false)));
    }
}
