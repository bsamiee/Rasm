using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.RhinoBridge.Client;

// --- [COMPOSITION] ----------------------------------------------------------------------
internal static class Program {
    private const string DefaultRhinoWipBundleId = "com.mcneel.rhinoceros.9";
    private const string DefaultRhinoWipAppPath = "/Applications/RhinoWIP.app";
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(value: 5.0);
    public static async Task<int> Main(string[] args) {
        string command = args.Length > 0 ? args[0] : string.Empty;
        string[] rest = args.Length > 1 ? args[1..] : [];
        try {
            return command switch {
                "doctor" when rest.Length == 0 => await ReplyCommandAsync(command: BridgeWire.Doctor, phase: "doctor", request: BridgeWire.Request(command: BridgeWire.Doctor), resultPath: null).ConfigureAwait(false),
                "launch" when rest.Length == 0 => await LaunchCommandAsync().ConfigureAwait(false),
                "restart" when rest.Length == 0 => await RestartAsync().ConfigureAwait(false),
                "script" when rest.Length >= 1 => await ScriptAsync(scriptPath: rest[0], options: CliOptions.Parse(rest[1..])).ConfigureAwait(false),
                "load-smoke" when rest.Length >= 1 => await LoadSmokeAsync(assemblyPath: rest[0], options: CliOptions.Parse(rest[1..])).ConfigureAwait(false),
                "load" when rest.Length >= 1 => await LoadAsync(assemblyPath: rest[0], options: CliOptions.Parse(rest[1..])).ConfigureAwait(false),
                "unload" when rest is [string sessionId] => await ReplyCommandAsync(command: BridgeWire.Unload, phase: "unload", request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId)), resultPath: null).ConfigureAwait(false),
                "check" when rest.Length >= 1 => await CheckProjectAsync(projectPath: rest[0], options: CliOptions.Parse(rest[1..])).ConfigureAwait(false),
                "check-source" when rest.Length >= 1 => await CheckSourceAsync(sourcePath: rest[0], options: CliOptions.Parse(rest[1..])).ConfigureAwait(false),
                "quit" when rest.Length == 0 => await QuitAsync().ConfigureAwait(false),
                _ => Usage(),
            };
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException or Win32Exception or UnauthorizedAccessException) {
            return CheckCommand(command: command)
                ? PrintFailure(command: command, rest: rest, error: error)
                : Fail(message: error.Message);
        }
    }
    private static bool CheckCommand(string command) =>
        command is "doctor" or "launch" or "restart" or "script" or "load-smoke" or "load" or "unload" or "check" or "check-source" or "quit";
    private static int PrintFailure(string command, IReadOnlyList<string> rest, Exception error) {
        string phase = FailurePhase(command: command);
        BridgeResult result = BridgeResult.From(command: command, phases: [BridgePhase.Failed(phase: phase, message: error.Message, fault: BridgeFault.FromException(category: phase, error: error))]);
        return PrintResult(result: result, path: ResultPath(args: rest));
    }
    private static string FailurePhase(string command) =>
        command switch {
            "doctor" => "doctor",
            "load" => "load",
            "unload" => "unload",
            "quit" or "restart" => "lifecycle",
            "script" or "check" or "check-source" or "load-smoke" => "resolve",
            "launch" => "launch",
            _ => "resolve",
        };
    private static string? ResultPath(IReadOnlyList<string> args) =>
        args.Select((value, index) => new { value, index })
            .Where(item => string.Equals(a: item.value, b: "--result", comparisonType: StringComparison.Ordinal) && item.index + 1 < args.Count)
            .Select(item => args[item.index + 1])
            .FirstOrDefault();
    private static int Usage() {
        Console.Error.WriteLine("Usage: rhino-bridge-client doctor | launch | restart | script <script.csx|.cs> [options] | load-smoke <assembly> [options] | load <assembly> [--worktree <path>] [--timeout-ms <ms>] | unload <session> | check <project.csproj> [options] | check-source <source.cs> [--script <script.csx|.cs>] [options] | quit");
        Console.Error.WriteLine("Options: --configuration <name> --worktree <path> --timeout-ms <ms> --result <path>");
        Console.Error.WriteLine("Launch env: RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app or RHINO_WIP_BUNDLE_ID=com.mcneel.rhinoceros.9");
        return 2;
    }
    private static async Task<int> LaunchCommandAsync() {
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: LaunchWait()).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "launch", phases: [launch, connect]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> RestartAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgePhase quit = await PhaseAsync(phase: "lifecycle", work: async () => {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: "lifecycle", reply: reply);
        }).ConfigureAwait(false);
        bool exited = !string.Equals(a: quit.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            || await WaitForExitAsync(pid: endpoint.RhinoPid, timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (string.Equals(a: quit.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)) {
            quit = exited ? quit : BridgePhase.Failed(phase: "lifecycle", message: $"Rhino process {endpoint.RhinoPid.ToString(provider: CultureInfo.InvariantCulture)} did not exit before restart timeout.");
        }
        BridgePhase launch = string.Equals(a: quit.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            ? await LaunchPhaseAsync().ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "launch", message: "Lifecycle quit failed before restart launch.");
        bool canConnect = string.Equals(a: quit.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            && (string.Equals(a: launch.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) || string.Equals(a: launch.Status, b: BridgeWire.Skipped, comparisonType: StringComparison.Ordinal));
        BridgePhase connect = canConnect
            ? await ConnectPhaseAsync(timeout: LaunchWait()).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "connect", message: "Lifecycle quit failed before restart connect.");
        BridgeResult result = BridgeResult.From(command: "restart", phases: [quit, launch, connect]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> LoadAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase load = await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, timeoutMs: options.TimeoutMs).ConfigureAwait(false);
        return PrintResult(result: BridgeResult.From(command: BridgeWire.Load, phases: [load]), path: options.Result);
    }
    private static async Task<int> LoadSmokeAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase load = string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            ? await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "load", message: "Bridge connect failed before load-smoke.");
        BridgePhase unload = load.DataValue<BridgeLoadReport>() is { SessionId: string sessionId }
            ? await UnloadPhaseAsync(sessionId: sessionId).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "unload", message: "No load session was created.");
        BridgeResult result = BridgeResult.From(command: "load-smoke", phases: [connect, load, unload]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> ScriptAsync(string scriptPath, CliOptions options) {
        string scriptFile = ExistingFile(path: scriptPath, label: "script");
        string script = await File.ReadAllTextAsync(path: scriptFile, encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        string worktree = await WorkspaceRootAsync(path: scriptFile, options: options).ConfigureAwait(false);
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase scriptServer = await ScriptServerPhaseAsync().ConfigureAwait(false);
        BridgePhase execute = string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            ? await ExecutePhaseAsync(script: script, scriptPath: scriptFile, worktree: worktree, references: [], timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "execute", message: "Bridge connect failed before script execution.");
        BridgeResult result = BridgeResult.From(command: "script", phases: [launch, connect, scriptServer, execute]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> CheckProjectAsync(string projectPath, CliOptions options) {
        string project = ExistingFile(path: projectPath, label: "project");
        string worktree = await WorkspaceRootAsync(path: project, options: options).ConfigureAwait(false);
        BridgePhase resolve = BridgePhase.Ok(phase: "resolve", data: new { projectPath = project, workspaceRoot = worktree });
        (BridgePhase buildPhase, ProjectBuild? buildProject) = await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false);
        BridgePhase launch = string.Equals(a: buildPhase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: "launch", message: "Build failed before Rhino launch.");
        BridgePhase connect = string.Equals(a: buildPhase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false) : BridgePhase.Skipped(phase: "connect", message: "Build failed before bridge connect.");
        BridgePhase scriptServer = string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await ScriptServerPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: "scriptServer", message: "Bridge connect failed before RhinoCode discovery.");
        BridgePhase load = BridgePhase.Skipped(phase: "load", message: "RhinoCode check uses #r references without a separate bridge load session.");
        BridgePhase execute = buildProject is { } projectBuild && string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            ? await ExecutePhaseAsync(script: SmokeScript(project: projectBuild), scriptPath: null, worktree: worktree, references: projectBuild.References, timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: "execute", message: "Build or bridge connect failed before RhinoCode execution.");
        BridgePhase diagnostics = DiagnosticsPhase(execute: execute);
        BridgeResult result = BridgeResult.From(command: "check", phases: [resolve, buildPhase, launch, connect, scriptServer, load, execute, diagnostics, BridgePhase.Skipped(phase: "unload", message: "No bridge load session was created."), BridgePhase.Skipped(phase: "lifecycle", message: "No lifecycle action was requested.")]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> CheckSourceAsync(string sourcePath, CliOptions options) {
        string source = ExistingFile(path: sourcePath, label: "source");
        string worktree = await WorkspaceRootAsync(path: source, options: options).ConfigureAwait(false);
        (BridgePhase resolvePhase, string? resolvedProject) = await ResolveSourcePhaseAsync(source: source, worktree: worktree, configuration: options.Configuration).ConfigureAwait(false);
        (BridgePhase buildPhase, ProjectBuild? buildProject) = resolvedProject is string project
            ? await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false)
            : (BridgePhase.Skipped(phase: "build", message: "Source ownership could not be resolved."), null);
        BridgePhase unsupported = options.Script is null
            ? BridgePhase.Unsupported(phase: "execute", message: "Source files are not executable without --script or a future --symbol target.")
            : BridgePhase.Skipped(phase: "execute", message: "Build or bridge connect failed before source script execution.");
        string blockedReason = options.Script is null ? "No executable script was supplied." : "Build failed before source script execution.";
        BridgePhase launch = options.Script is not null && string.Equals(a: buildPhase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: "launch", message: blockedReason);
        BridgePhase connect = options.Script is not null && string.Equals(a: buildPhase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false) : BridgePhase.Skipped(phase: "connect", message: blockedReason);
        BridgePhase scriptServer = string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal) ? await ScriptServerPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: "scriptServer", message: "Bridge connect failed before RhinoCode discovery.");
        BridgePhase load = BridgePhase.Skipped(phase: "load", message: "Source checks use RhinoCode #r scripts without a separate bridge load session.");
        BridgePhase execute = options.Script is string sourceScriptPath && buildProject is { } projectBuild && string.Equals(a: connect.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)
            ? await ExecutePhaseAsync(script: ComposeScript(projectBuild: projectBuild, script: await File.ReadAllTextAsync(path: ExistingFile(path: sourceScriptPath, label: "script"), encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false)), scriptPath: null, worktree: worktree, references: projectBuild.References, timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : unsupported;
        BridgePhase diagnostics = DiagnosticsPhase(execute: execute);
        BridgeResult result = BridgeResult.From(command: "check-source", phases: [resolvePhase, buildPhase, launch, connect, scriptServer, load, execute, diagnostics, BridgePhase.Skipped(phase: "unload", message: "No bridge load session was created."), BridgePhase.Skipped(phase: "lifecycle", message: "No lifecycle action was requested.")]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> QuitAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "quit", phases: [BridgePhase.FromReply(phase: "lifecycle", reply: reply)]);
        int exitCode = PrintResult(result: result, path: null);
        if (string.Equals(a: reply.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)) {
            _ = await WaitForExitAsync(pid: endpoint.RhinoPid, timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        }
        return exitCode;
    }
    private static async Task<bool> WaitForExitAsync(int pid, TimeSpan timeout) {
        try {
            using Process process = Process.GetProcessById(processId: pid);
            using CancellationTokenSource cancellation = new(delay: timeout);
            await process.WaitForExitAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
            return true;
        } catch (OperationCanceledException) {
            return false;
        } catch (Exception error) when (error is ArgumentException or InvalidOperationException) {
            return true;
        }
    }
    private static async Task<BridgePhase> LaunchPhaseAsync() {
        Stopwatch timer = Stopwatch.StartNew();
        try {
            BridgeReply live = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeSpan.FromSeconds(value: 3.0)).ConfigureAwait(false);
            timer.Stop();
            return BridgePhase.Skipped(phase: "launch", timer: timer, data: new { reason = "Existing bridge endpoint answered.", endpoint = live.Data });
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            string? appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH");
            string bundleId = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_BUNDLE_ID") ?? DefaultRhinoWipBundleId;
            ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: string.IsNullOrWhiteSpace(value: appPath) ? ["-b", bundleId, "--args", "-nosplash"] : [appPath, "--args", "-nosplash"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
            timer.Stop();
            return opened.ExitCode == 0
                ? BridgePhase.Ok(phase: "launch", timer: timer, data: new { bundleId, appPath }, outputs: opened.Outputs)
                : BridgePhase.Failed(phase: "launch", timer: timer, message: "Failed to open RhinoWIP.", outputs: opened.Outputs);
        }
    }
    private static async Task<BridgePhase> ConnectPhaseAsync(TimeSpan timeout) {
        Stopwatch timer = Stopwatch.StartNew();
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);
        BridgePhase last = BridgePhase.Failed(phase: "connect", message: "Bridge did not answer before connect polling started.");
        while (DateTimeOffset.UtcNow < deadline) {
            try {
                BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeSpan.FromSeconds(value: 3.0)).ConfigureAwait(false);
                timer.Stop();
                BridgePhase phase = BridgePhase.FromReply(phase: "connect", reply: reply) with { DurationMs = (int)timer.ElapsedMilliseconds };
                if (string.Equals(a: phase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal)) {
                    return phase;
                }
                last = phase;
                await Task.Delay(delay: TimeSpan.FromMilliseconds(value: 500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException) {
                last = BridgePhase.Failed(phase: "connect", message: error.Message, fault: BridgeFault.FromException(category: "connect", error: error));
                await Task.Delay(delay: TimeSpan.FromMilliseconds(value: 500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }
        timer.Stop();
        return last with { DurationMs = (int)timer.ElapsedMilliseconds };
    }
    private static TimeSpan LaunchWait() => TimeSpan.FromSeconds(value: 45.0);
    private static async Task<BridgePhase> ScriptServerPhaseAsync() {
        Stopwatch timer = Stopwatch.StartNew();
        string rhinoCodePath = RhinoCodeCliPath();
        if (!File.Exists(path: rhinoCodePath)) {
            timer.Stop();
            return BridgePhase.Skipped(phase: "scriptServer", timer: timer, data: new { reason = "rhinocode CLI was not found.", path = rhinoCodePath });
        }
        ProcessResult direct = await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0)).ConfigureAwait(false);
        ProcessResult rolled = direct.ExitCode == 0
            ? direct
            : await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0), environment: new Dictionary<string, string>(StringComparer.Ordinal) { ["DOTNET_ROLL_FORWARD"] = "Major" }).ConfigureAwait(false);
        timer.Stop();
        BridgeOutput[] outputs = [.. direct.Outputs.Concat(ReferenceEquals(objA: rolled, objB: direct) ? [] : rolled.Outputs)];
        object data = new { path = rhinoCodePath, directExitCode = direct.ExitCode, rollForwardExitCode = rolled.ExitCode };
        return rolled.ExitCode == 0
            ? BridgePhase.Ok(phase: "scriptServer", timer: timer, data: data, outputs: outputs)
            : BridgePhase.Failed(phase: "scriptServer", timer: timer, message: "rhinocode list --json failed.", outputs: outputs);
    }
    private static string RhinoCodeCliPath() =>
        Path.Combine(path1: Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH") ?? DefaultRhinoWipAppPath, path2: "Contents/Resources/bin/rhinocode");
    private static async Task<BridgePhase> LoadPhaseAsync(string assembly, string workspaceRoot, int timeoutMs) =>
        await PhaseAsync(phase: "load", work: async () => {
            BridgeReply reply = await SendAsync(
                request: BridgeWire.Request(command: BridgeWire.Load, payload: BridgeWire.LoadRequest(assemblyPath: assembly, workspaceRoot: workspaceRoot), timeoutMs: timeoutMs),
                timeout: TransportTimeout(timeoutMs: timeoutMs)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: "load", reply: reply);
        }).ConfigureAwait(false);
    private static async Task<BridgePhase> UnloadPhaseAsync(string sessionId) =>
        await PhaseAsync(phase: "unload", work: async () => {
            BridgeReply reply = await SendAsync(
                request: BridgeWire.Request(command: BridgeWire.Unload, payload: BridgeWire.UnloadRequest(sessionId: sessionId)),
                timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: "unload", reply: reply);
        }).ConfigureAwait(false);
    private static async Task<BridgePhase> ExecutePhaseAsync(string script, string? scriptPath, string worktree, IReadOnlyList<string> references, int timeoutMs) =>
        await PhaseAsync(phase: "execute", work: async () => {
            string stagedScript = scriptPath ?? StageScript(worktree: worktree, script: script);
            BridgeReply reply = await SendAsync(
                request: BridgeWire.Request(command: BridgeWire.Execute, payload: BridgeWire.ExecuteRequest(script: script, scriptPath: stagedScript, references: references), timeoutMs: timeoutMs),
                timeout: TransportTimeout(timeoutMs: timeoutMs)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: "execute", reply: reply);
        }).ConfigureAwait(false);
    private static string StageScript(string worktree, string script) {
        string root = Path.Combine(path1: worktree, path2: ".artifacts/rhino/bridge", path3: string.Create(provider: CultureInfo.InvariantCulture, $"execute-{Environment.ProcessId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"));
        _ = Directory.CreateDirectory(path: root);
        string path = Path.Combine(path1: root, path2: "script.csx");
        string temp = string.Create(provider: CultureInfo.InvariantCulture, $"{path}.{Environment.ProcessId}.tmp");
        File.WriteAllText(path: temp, contents: script, encoding: Encoding.UTF8);
        File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
        return path;
    }
    private static async Task<(BridgePhase Build, ProjectBuild? Project)> BuildPhaseAsync(string project, string configuration) {
        Stopwatch timer = Stopwatch.StartNew();
        ProcessResult restore = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["restore", project, "--locked-mode"], timeout: ProcessTimeout).ConfigureAwait(false);
        if (restore.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Failed(phase: "build", timer: timer, message: "dotnet restore failed.", outputs: restore.Outputs), null);
        }
        ProcessResult build = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["build", project, "--configuration", configuration, "--no-restore"], timeout: ProcessTimeout).ConfigureAwait(false);
        if (build.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Failed(phase: "build", timer: timer, message: "dotnet build failed.", outputs: [.. restore.Outputs.Concat(build.Outputs)]), null);
        }
        ProcessResult target = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["msbuild", project, "-target:ResolveReferences", "-getProperty:TargetPath", "-getProperty:TargetDir", "-getProperty:TargetFramework", "-getProperty:AssemblyName", "-getProperty:TargetExt", "-getItem:ReferenceCopyLocalPaths", "-getItem:ReferencePath", $"-p:Configuration={configuration}", "-p:RestoreLockedMode=true", "-nologo"], timeout: ProcessTimeout).ConfigureAwait(false);
        if (target.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Failed(phase: "build", timer: timer, message: "dotnet msbuild ResolveReferences failed.", outputs: [.. restore.Outputs.Concat(build.Outputs).Concat(target.Outputs)]), null);
        }
        try {
            ProjectBuild projectBuild = ProjectBuild.Parse(projectPath: project, configuration: configuration, json: target.Stdout);
            timer.Stop();
            return (BridgePhase.Ok(phase: "build", timer: timer, data: projectBuild, outputs: [.. restore.Outputs.Concat(build.Outputs).Concat(target.Outputs)]), projectBuild);
        } catch (Exception error) when (error is JsonException or InvalidOperationException or ArgumentException) {
            timer.Stop();
            return (BridgePhase.Failed(phase: "build", message: "MSBuild reference projection could not be parsed.", fault: BridgeFault.FromException(category: "build", error: error)) with { DurationMs = (int)timer.ElapsedMilliseconds, Outputs = [.. restore.Outputs.Concat(build.Outputs).Concat(target.Outputs)] }, null);
        }
    }
    private static async Task<(BridgePhase Resolve, string? Project)> ResolveSourcePhaseAsync(string source, string worktree, string configuration) {
        Stopwatch timer = Stopwatch.StartNew();
        ProcessResult tracked = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", worktree, "ls-files", "*.csproj"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (tracked.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Failed(phase: "resolve", timer: timer, message: "git ls-files failed during project discovery.", outputs: tracked.Outputs), null);
        }
        string[] projects = [.. tracked.Stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(path => Path.GetFullPath(path: Path.Combine(path1: worktree, path2: path)))];
        SourceOwner[] owners = [.. (await Task.WhenAll(projects.Select(project => SourceOwner.ResolveAsync(project: project, source: source, configuration: configuration))).ConfigureAwait(false)).OfType<SourceOwner>()];
        timer.Stop();
        return owners.Length switch {
            1 => (BridgePhase.Ok(phase: "resolve", timer: timer, data: new { sourcePath = source, projectPath = owners[0].ProjectPath, link = owners[0].Link }), owners[0].ProjectPath),
            0 => (BridgePhase.Failed<object>(phase: "resolve", timer: timer, category: "source", message: $"No tracked SDK project owns source file: {source}", data: null, outputs: tracked.Outputs), null),
            _ => (BridgePhase.Failed(phase: "resolve", timer: timer, category: "ambiguous", message: $"Multiple projects own source file: {source}", data: new { sourcePath = source, candidates = owners }, outputs: tracked.Outputs), null),
        };
    }
    private static BridgePhase DiagnosticsPhase(BridgePhase execute) =>
        execute.Diagnostics.Count > 0
            ? BridgePhase.Ok(phase: "diagnostics", data: new { count = execute.Diagnostics.Count, sourcePhase = execute.Phase }, diagnostics: execute.Diagnostics)
            : BridgePhase.Skipped(phase: "diagnostics", message: "No RhinoCode diagnostics were reported.");
    private static string ComposeScript(ProjectBuild projectBuild, string script) =>
        string.Concat(ReferenceDirectives(references: projectBuild.References), Environment.NewLine, script);
    private static string SmokeScript(ProjectBuild project) {
        string references = ReferenceDirectives(references: project.References);
        string target = Escape(value: project.TargetPath);
        string nonce = Guid.NewGuid().ToString(format: "N");
        return string.Join(separator: Environment.NewLine, values: [
            references,
            "using System;",
            "using System.Globalization;",
            "using System.Reflection;",
            "using Rhino;",
            string.Empty,
            string.Create(provider: CultureInfo.InvariantCulture, $"Assembly targetAssembly = Assembly.LoadFrom(\"{target}\");"),
            string.Create(provider: CultureInfo.InvariantCulture, $"Console.WriteLine(\"rasm.rhino-bridge.nonce={nonce}\");"),
            "Console.WriteLine(\"loadedAssembly=\" + targetAssembly.FullName);",
            "Console.WriteLine(\"rhinoVersion=\" + RhinoApp.Version);",
            "Console.WriteLine(\"activeDocument=\" + (RhinoDoc.ActiveDoc is not null));",
            "Console.WriteLine(\"modelAbsoluteTolerance=\" + (RhinoDoc.ActiveDoc?.ModelAbsoluteTolerance.ToString(CultureInfo.InvariantCulture) ?? \"none\"));",
            string.Create(provider: CultureInfo.InvariantCulture, $"Console.Error.WriteLine(\"rasm.rhino-bridge.stderr={nonce}\");"),
        ]);
    }
    private static string ReferenceDirectives(IReadOnlyList<string> references) =>
        string.Join(separator: Environment.NewLine, values: references.Select(static reference => $"#r \"{Escape(value: reference)}\""));
    private static string Escape(string value) =>
        value.Replace(oldValue: "\\", newValue: "\\\\", comparisonType: StringComparison.Ordinal).Replace(oldValue: "\"", newValue: "\\\"", comparisonType: StringComparison.Ordinal);
    private static async Task<BridgePhase> PhaseAsync(string phase, Func<Task<BridgePhase>> work) {
        try {
            return await work().ConfigureAwait(false);
        } catch (TimeoutException error) {
            return new(Phase: phase, Status: BridgeWire.Timeout, DurationMs: 0, Data: null, Outputs: [], Diagnostics: [], Fault: BridgeFault.FromException(category: phase, error: error));
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            return BridgePhase.Failed(phase: phase, message: error.Message, fault: BridgeFault.FromException(category: phase, error: error));
        }
    }
    private static async Task<int> ReplyCommandAsync(string command, string phase, BridgeRequest request, string? resultPath) {
        BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout(timeoutMs: request.TimeoutMs)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: command, phases: [BridgePhase.FromReply(phase: phase, reply: reply)]);
        return PrintResult(result: result, path: resultPath);
    }
    private static async Task<BridgeReply> SendAsync(BridgeRequest request, TimeSpan timeout) {
        using CancellationTokenSource cancellation = new(delay: timeout);
        BridgeEndpoint endpoint = ReadEndpoint();
        using NamedPipeClientStream pipe = new(serverName: ".", pipeName: endpoint.PipeName, direction: PipeDirection.InOut, options: PipePolicy);
        await pipe.ConnectAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
        StreamWriter writer = new(stream: pipe, encoding: Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        using StreamReader reader = new(stream: pipe, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            await writer.WriteLineAsync(buffer: JsonSerializer.Serialize(value: request, options: BridgeWire.CompactJson).AsMemory(), cancellationToken: cancellation.Token).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
            string? line = await reader.ReadLineAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
            BridgeReply reply = string.IsNullOrWhiteSpace(value: line)
                ? throw new InvalidOperationException(message: "Bridge returned no response.")
                : JsonSerializer.Deserialize<BridgeReply>(json: line, options: BridgeWire.CompactJson) ?? throw new InvalidOperationException(message: "Bridge returned an invalid response.");
            return BridgeWire.IsCurrent(schema: reply.Schema)
                ? reply
                : throw new InvalidOperationException(message: $"Bridge returned unsupported schema '{reply.Schema}'.");
        }
    }
    private static BridgeEndpoint ReadEndpoint() {
        BridgeEndpoint endpoint = JsonSerializer.Deserialize<BridgeEndpoint>(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8), options: BridgeWire.CompactJson)
            ?? throw new InvalidOperationException(message: $"Endpoint metadata is invalid: {BridgeWire.EndpointPath}");
        if (!BridgeWire.IsCurrent(schema: endpoint.Schema)) {
            throw new InvalidOperationException(message: $"Endpoint metadata has unsupported schema '{endpoint.Schema}': {BridgeWire.EndpointPath}");
        }
        using Process process = Process.GetProcessById(processId: endpoint.RhinoPid);
        DateTimeOffset startedAt = new(dateTime: process.StartTime.ToUniversalTime());
        bool validPipe = endpoint.PipeName.Length <= 64 && endpoint.PipeName.StartsWith(value: $"rb-{endpoint.RhinoPid}-", comparisonType: StringComparison.Ordinal);
        return (process.HasExited, validPipe, Math.Abs(value: (startedAt - endpoint.RhinoStartedAt).TotalSeconds) <= 2.0) switch {
            (false, true, true) => endpoint,
            _ => throw new InvalidOperationException(message: $"Endpoint metadata is stale or unsupported: {BridgeWire.EndpointPath}"),
        };
    }
    private static TimeSpan TransportTimeout(int timeoutMs) =>
        TimeSpan.FromMilliseconds(milliseconds: Math.Clamp(value: timeoutMs, min: 1, max: 300000) + 5000);
    private static int PrintResult(BridgeResult result, string? path) {
        string json = JsonSerializer.Serialize(value: result, options: BridgeWire.PrettyJson);
        if (path is not null) {
            try {
                string fullPath = Path.GetFullPath(path: path);
                _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: fullPath) ?? Directory.GetCurrentDirectory());
                string temp = string.Create(provider: CultureInfo.InvariantCulture, $"{fullPath}.{Environment.ProcessId}.tmp");
                File.WriteAllText(path: temp, contents: json, encoding: Encoding.UTF8);
                File.Move(sourceFileName: temp, destFileName: fullPath, overwrite: true);
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException) {
                BridgeResult failed = BridgeResult.From(command: result.Command, phases: [BridgePhase.Failed(phase: "result", message: $"Could not write result file: {path}", fault: BridgeFault.FromException(category: "result", error: error))]);
                Console.WriteLine(value: JsonSerializer.Serialize(value: failed, options: BridgeWire.PrettyJson));
                return ExitCode(status: failed.Status);
            }
        }
        Console.WriteLine(value: json);
        return ExitCode(status: result.Status);
    }
    private static int ExitCode(string status) =>
        status switch {
            BridgeWire.Ok => 0,
            BridgeWire.Unsupported => 3,
            BridgeWire.Busy or BridgeWire.Timeout => 5,
            _ => 1,
        };
    private static int Fail(string message) {
        Console.Error.WriteLine(value: message);
        return 1;
    }
    private static string ExistingFile(string path, string label) {
        string fullPath = Path.GetFullPath(path: path);
        return File.Exists(path: fullPath) ? fullPath : throw new InvalidOperationException(message: $"{label} does not exist: {fullPath}");
    }
    private static string ExistingDirectory(string path) {
        string fullPath = Path.GetFullPath(path: path);
        return Directory.Exists(path: fullPath) ? fullPath : throw new InvalidOperationException(message: $"worktree does not exist: {fullPath}");
    }
    private static async Task<string> WorkspaceRootAsync(string path, CliOptions options) =>
        options.Worktree is string explicitRoot ? ExistingDirectory(path: explicitRoot) : await WorktreeAsync(path: path).ConfigureAwait(false);
    private static async Task<string> WorktreeAsync(string path) {
        string directory = Path.GetDirectoryName(path: path) ?? Directory.GetCurrentDirectory();
        ProcessResult git = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", directory, "rev-parse", "--show-toplevel"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        return git.ExitCode switch {
            0 => git.Stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? directory,
            _ => directory,
        };
    }
}

// --- [MODELS] ---------------------------------------------------------------------------
internal sealed record CliOptions(string? Worktree, string Configuration, int TimeoutMs, string? Result, string? Script) {
    private static CliOptions Default =>
        new(Worktree: null, Configuration: Environment.GetEnvironmentVariable(variable: "CONFIGURATION") ?? "Release", TimeoutMs: 30000, Result: null, Script: null);
    internal static CliOptions Parse(IReadOnlyList<string> args) =>
        Parse(args: args, index: 0, current: Default);
    private static CliOptions Parse(IReadOnlyList<string> args, int index, CliOptions current) =>
        index >= args.Count
            ? current
            : args[index] switch {
                "--worktree" => Parse(args: args, index: index + 2, current: current with { Worktree = Value(args: args, index: index, option: "--worktree") }),
                "--configuration" => Parse(args: args, index: index + 2, current: current with { Configuration = Value(args: args, index: index, option: "--configuration") }),
                "--timeout-ms" => Parse(args: args, index: index + 2, current: current with { TimeoutMs = ParseTimeout(value: Value(args: args, index: index, option: "--timeout-ms")) }),
                "--result" => Parse(args: args, index: index + 2, current: current with { Result = Value(args: args, index: index, option: "--result") }),
                "--script" => Parse(args: args, index: index + 2, current: current with { Script = Value(args: args, index: index, option: "--script") }),
                string unknown when unknown.StartsWith(value: "--", comparisonType: StringComparison.Ordinal) => throw new InvalidOperationException(message: $"Unknown bridge option: {unknown}"),
                string value => throw new InvalidOperationException(message: $"Unexpected bridge argument: {value}"),
            };
    private static string Value(IReadOnlyList<string> args, int index, string option) =>
        (index + 1) < args.Count ? args[index + 1] : throw new InvalidOperationException(message: $"Missing value for {option}.");
    private static int ParseTimeout(string value) =>
        int.TryParse(s: value, provider: CultureInfo.InvariantCulture, result: out int parsed) && parsed > 0
            ? parsed
            : throw new InvalidOperationException(message: $"Invalid --timeout-ms value: {value}");
}

internal sealed record BridgeResult(string Schema, string Command, string Status, IReadOnlyList<BridgePhase> Phases, BridgeFault? Fault) {
    internal static BridgeResult From(string command, IReadOnlyList<BridgePhase> phases) =>
        new(Schema: BridgeWire.Schema, Command: command, Status: StatusOf(phases: phases), Phases: phases, Fault: FaultOf(phases: phases));
    private static BridgeFault? FaultOf(IReadOnlyList<BridgePhase> phases) {
        bool executeSucceeded = ExecuteSucceeded(phases: phases);
        return phases
            .Where(phase => !executeSucceeded || !string.Equals(a: phase.Phase, b: "scriptServer", comparisonType: StringComparison.Ordinal))
            .FirstOrDefault(static phase => phase.Fault is not null)
            ?.Fault;
    }
    private static string StatusOf(IReadOnlyList<BridgePhase> phases) {
        bool executeSucceeded = ExecuteSucceeded(phases: phases);
        return phases
            .Where(phase => !executeSucceeded || !string.Equals(a: phase.Phase, b: "scriptServer", comparisonType: StringComparison.Ordinal))
            .Select(static phase => phase.Status)
            .Aggregate(seed: BridgeWire.Ok, func: Worst);
    }
    private static bool ExecuteSucceeded(IReadOnlyList<BridgePhase> phases) =>
        phases.Any(static phase => string.Equals(a: phase.Phase, b: "execute", comparisonType: StringComparison.Ordinal) && string.Equals(a: phase.Status, b: BridgeWire.Ok, comparisonType: StringComparison.Ordinal));
    private static string Worst(string current, string next) =>
        Rank(next) > Rank(current) ? next : current;
    private static int Rank(string status) =>
        status switch {
            BridgeWire.Failed or BridgeWire.Timeout or BridgeWire.Busy => 3,
            BridgeWire.Unsupported => 2,
            BridgeWire.Ok or BridgeWire.Skipped => 1,
            _ => 3,
        };
}

internal sealed record BridgePhase(string Phase, string Status, int DurationMs, JsonElement? Data, IReadOnlyList<BridgeOutput> Outputs, IReadOnlyList<BridgeDiagnostic> Diagnostics, BridgeFault? Fault) {
    internal static BridgePhase Ok<TData>(string phase, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        new(Phase: phase, Status: BridgeWire.Ok, DurationMs: 0, Data: JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: outputs ?? [], Diagnostics: diagnostics ?? [], Fault: null);
    internal static BridgePhase Ok<TData>(string phase, Stopwatch timer, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        new(Phase: phase, Status: BridgeWire.Ok, DurationMs: (int)timer.ElapsedMilliseconds, Data: JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: outputs ?? [], Diagnostics: diagnostics ?? [], Fault: null);
    internal static BridgePhase Failed(string phase, string message, BridgeFault? fault = null) =>
        new(Phase: phase, Status: BridgeWire.Failed, DurationMs: 0, Data: null, Outputs: [], Diagnostics: [], Fault: fault ?? BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed(string phase, Stopwatch timer, string message, IReadOnlyList<BridgeOutput>? outputs = null) =>
        new(Phase: phase, Status: BridgeWire.Failed, DurationMs: (int)timer.ElapsedMilliseconds, Data: null, Outputs: outputs ?? [], Diagnostics: [], Fault: BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed<TData>(string phase, Stopwatch timer, string category, string message, TData? data, IReadOnlyList<BridgeOutput>? outputs = null) =>
        new(Phase: phase, Status: BridgeWire.Failed, DurationMs: (int)timer.ElapsedMilliseconds, Data: data is null ? null : JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: outputs ?? [], Diagnostics: [], Fault: BridgeFault.MessageOnly(category: category, message: message));
    internal static BridgePhase Skipped(string phase, string message) =>
        new(Phase: phase, Status: BridgeWire.Skipped, DurationMs: 0, Data: JsonSerializer.SerializeToElement(value: new { reason = message }, options: BridgeWire.CompactJson), Outputs: [], Diagnostics: [], Fault: null);
    internal static BridgePhase Skipped<TData>(string phase, Stopwatch timer, TData data) =>
        new(Phase: phase, Status: BridgeWire.Skipped, DurationMs: (int)timer.ElapsedMilliseconds, Data: JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: [], Diagnostics: [], Fault: null);
    internal static BridgePhase Unsupported(string phase, string message) =>
        new(Phase: phase, Status: BridgeWire.Unsupported, DurationMs: 0, Data: null, Outputs: [], Diagnostics: [], Fault: BridgeFault.MessageOnly(category: BridgeWire.Unsupported, message: message));
    internal static BridgePhase FromReply(string phase, BridgeReply reply) =>
        BridgeWire.IsStatus(status: reply.Status)
            ? new(Phase: phase, Status: reply.Status, DurationMs: 0, Data: reply.Data, Outputs: reply.Outputs, Diagnostics: reply.Diagnostics, Fault: reply.Fault)
            : Failed(phase: phase, message: $"Bridge returned unsupported status '{reply.Status}'.", fault: BridgeFault.MessageOnly(category: phase, message: $"Bridge returned unsupported status '{reply.Status}'."));
    internal T? DataValue<T>() =>
        Data is JsonElement data ? data.Deserialize<T>(options: BridgeWire.CompactJson) : default;
}

internal sealed record ProjectBuild(string ProjectPath, string Configuration, string? TargetFramework, string? AssemblyName, string TargetPath, string? TargetDir, string? TargetExt, IReadOnlyList<string> References) {
    internal static ProjectBuild Parse(string projectPath, string configuration, string json) {
        using JsonDocument document = JsonDocument.Parse(json: json);
        JsonElement properties = document.RootElement.GetProperty(propertyName: "Properties");
        string targetPath = Existing(Text(properties: properties, name: "TargetPath"), label: "TargetPath");
        string? targetDir = Text(properties: properties, name: "TargetDir");
        string[] references = [.. new[] { targetPath }
            .Concat(ReferenceCopyLocalPaths(root: document.RootElement))
            .Concat(ReferencePaths(root: document.RootElement))
            .Concat(DepsReferences(targetPath: targetPath, targetDir: targetDir))
            .Where(static path => IsReferenceFile(path: path) && !IsFrameworkReferencePack(path: path))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
        return new(ProjectPath: projectPath, Configuration: configuration, TargetFramework: Text(properties: properties, name: "TargetFramework"), AssemblyName: Text(properties: properties, name: "AssemblyName"), TargetPath: targetPath, TargetDir: targetDir, TargetExt: Text(properties: properties, name: "TargetExt"), References: references);
    }
    private static bool IsReferenceFile(string path) =>
        File.Exists(path: path)
        && (path.EndsWith(value: ".dll", comparisonType: StringComparison.OrdinalIgnoreCase) || path.EndsWith(value: ".rhp", comparisonType: StringComparison.OrdinalIgnoreCase));
    private static bool IsFrameworkReferencePack(string path) =>
        path.Contains(value: "/packs/Microsoft.NETCore.App.Ref/", comparisonType: StringComparison.Ordinal)
        || path.Contains(value: "/packs/NETStandard.Library.Ref/", comparisonType: StringComparison.Ordinal);
    private static string? Text(JsonElement properties, string name) =>
        properties.TryGetProperty(propertyName: name, value: out JsonElement value) ? value.GetString() : null;
    private static string Existing(string? path, string label) =>
        path is string full && File.Exists(path: full) ? full : throw new InvalidOperationException(message: $"MSBuild did not return an existing {label}: {path}");
    private static IEnumerable<string> ReferenceCopyLocalPaths(JsonElement root) =>
        ItemPaths(root: root, itemName: "ReferenceCopyLocalPaths");
    private static IEnumerable<string> ReferencePaths(JsonElement root) =>
        ItemPaths(root: root, itemName: "ReferencePath");
    private static IEnumerable<string> ItemPaths(JsonElement root, string itemName) =>
        root.TryGetProperty(propertyName: "Items", value: out JsonElement items)
        && items.TryGetProperty(propertyName: itemName, value: out JsonElement references)
            ? references.EnumerateArray().Select(static item => item.TryGetProperty(propertyName: "FullPath", value: out JsonElement fullPath) ? fullPath.GetString() : item.GetProperty(propertyName: "Identity").GetString()).OfType<string>()
            : [];
    private static IEnumerable<string> DepsReferences(string targetPath, string? targetDir) =>
        Path.ChangeExtension(path: targetPath, extension: ".deps.json") is string deps && File.Exists(path: deps) && targetDir is not null
            ? DepsRuntimeAssets(deps: deps, targetDir: targetDir)
            : [];
    private static IEnumerable<string> DepsRuntimeAssets(string deps, string targetDir) {
        using JsonDocument document = JsonDocument.Parse(json: File.ReadAllText(path: deps, encoding: Encoding.UTF8));
        return [.. document.RootElement.GetProperty(propertyName: "targets").EnumerateObject()
            .SelectMany(static target => target.Value.EnumerateObject())
            .SelectMany(static library => library.Value.TryGetProperty(propertyName: "runtime", value: out JsonElement runtime) ? runtime.EnumerateObject().Select(static asset => asset.Name) : [])
            .Select(asset => Path.Combine(path1: targetDir, path2: Path.GetFileName(path: asset)))
            .Where(File.Exists)];
    }
}

internal sealed record SourceOwner(string ProjectPath, string? Link) {
    internal static async Task<SourceOwner?> ResolveAsync(string project, string source, string configuration) {
        ProcessResult result = await ProcessResult.RunAsync(fileName: "dotnet", arguments: ["msbuild", project, "-getProperty:TargetPath", "-getItem:Compile", $"-p:Configuration={configuration}", "-nologo"], timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false);
        return result.ExitCode == 0 ? From(project: project, source: RealPath(path: source), json: result.Stdout) : null;
    }
    private static SourceOwner? From(string project, string source, string json) {
        using JsonDocument document = JsonDocument.Parse(json: json);
        return document.RootElement.GetProperty(propertyName: "Items").GetProperty(propertyName: "Compile").EnumerateArray()
            .Select(item => new { FullPath = Metadata(item: item, name: "FullPath") ?? item.GetProperty(propertyName: "Identity").GetString(), Link = Metadata(item: item, name: "Link") })
            .Where(item => item.FullPath is not null && string.Equals(a: Path.GetFullPath(path: item.FullPath), b: source, comparisonType: OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            .Select(item => new SourceOwner(ProjectPath: project, Link: item.Link))
            .FirstOrDefault();
    }
    private static string? Metadata(JsonElement item, string name) =>
        item.TryGetProperty(propertyName: name, value: out JsonElement direct)
            ? direct.GetString()
            : item.TryGetProperty(propertyName: "Metadata", value: out JsonElement metadata) && metadata.TryGetProperty(propertyName: name, value: out JsonElement nested)
                ? nested.GetString()
                : null;
    private static string RealPath(string path) =>
        File.Exists(path: path) ? new FileInfo(fileName: path).FullName : Path.GetFullPath(path: path);
}

internal sealed record ProcessResult(int ExitCode, string Stdout, string Stderr) {
    internal IReadOnlyList<BridgeOutput> Outputs => [
        new(Source: BridgeWire.OutputCommandStdout, Text: Stdout, Truncated: false),
        new(Source: BridgeWire.OutputCommandStderr, Text: Stderr, Truncated: false),
    ];
    internal static async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, TimeSpan timeout, IReadOnlyDictionary<string, string>? environment = null) {
        ProcessStartInfo start = new() {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments) {
            start.ArgumentList.Add(argument);
        }
        foreach (KeyValuePair<string, string> variable in environment ?? new Dictionary<string, string>(StringComparer.Ordinal)) {
            start.Environment[variable.Key] = variable.Value;
        }
        using Process process = Process.Start(startInfo: start) ?? throw new InvalidOperationException(message: $"Failed to start process: {fileName}");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(cancellationToken: CancellationToken.None);
        Task<string> stderr = process.StandardError.ReadToEndAsync(cancellationToken: CancellationToken.None);
        using CancellationTokenSource cancellation = new(delay: timeout);
        try {
            await process.WaitForExitAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
        } catch (OperationCanceledException error) {
            Kill(process: process);
            await process.WaitForExitAsync(cancellationToken: CancellationToken.None).ConfigureAwait(false);
            throw new TimeoutException(message: $"Process timed out after {timeout.TotalSeconds.ToString(provider: CultureInfo.InvariantCulture)}s: {fileName} {string.Join(separator: " ", values: arguments)}", innerException: error);
        }
        return new(ExitCode: process.ExitCode, Stdout: await stdout.ConfigureAwait(false), Stderr: await stderr.ConfigureAwait(false));
    }
    private static void Kill(Process process) {
        try {
            process.Kill(entireProcessTree: true);
        } catch (Exception error) when (error is InvalidOperationException or Win32Exception) {
        }
    }
}
