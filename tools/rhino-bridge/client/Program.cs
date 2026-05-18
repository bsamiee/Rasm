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
    internal const string PhaseBuild = "build";
    internal const string PhaseConnect = "connect";
    internal const string PhaseDiagnostics = "diagnostics";
    internal const string PhaseExecute = "execute";
    internal const string PhaseLaunch = "launch";
    internal const string PhaseLifecycle = "lifecycle";
    internal const string PhaseLoad = "load";
    internal const string PhaseResolve = "resolve";
    internal const string PhaseRhinoCodeCli = "rhinoCodeCli";
    internal const string PhaseUnload = "unload";
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(value: 5.0);
    private static readonly IReadOnlyDictionary<string, ClientCommand> Commands =
        new Dictionary<string, ClientCommand>(StringComparer.Ordinal) {
            ["doctor"] = new(Usage: "doctor [options]", FailurePhase: BridgeWire.Doctor, MinArgs: 0, MaxArgs: 999, Run: static rest => {
                CliOptions options = CliOptions.Parse(args: rest);
                return ReplyCommandAsync(command: BridgeWire.Doctor, phase: BridgeWire.Doctor, request: BridgeWire.Request(command: BridgeWire.Doctor, timeoutMs: options.TimeoutMs), resultPath: options.Result);
            }),
            ["launch"] = new(Usage: "launch", FailurePhase: PhaseLaunch, MinArgs: 0, MaxArgs: 0, Run: static _ => LaunchCommandAsync()),
            ["restart"] = new(Usage: "restart", FailurePhase: PhaseLifecycle, MinArgs: 0, MaxArgs: 0, Run: static _ => RestartAsync()),
            ["script"] = new(Usage: "script <script.csx|.cs> [options]", FailurePhase: PhaseResolve, MinArgs: 1, MaxArgs: 999, Run: static rest => ScriptAsync(scriptPath: rest[0], options: CliOptions.Parse(rest[1..]))),
            ["load-smoke"] = new(Usage: "load-smoke <assembly> [options]", FailurePhase: PhaseResolve, MinArgs: 1, MaxArgs: 999, Run: static rest => LoadSmokeAsync(assemblyPath: rest[0], options: CliOptions.Parse(rest[1..]))),
            ["load"] = new(Usage: "load <assembly> [options]", FailurePhase: PhaseLoad, MinArgs: 1, MaxArgs: 999, Run: static rest => LoadAsync(assemblyPath: rest[0], options: CliOptions.Parse(rest[1..]))),
            ["unload"] = new(Usage: "unload <session>", FailurePhase: PhaseUnload, MinArgs: 1, MaxArgs: 1, Run: static rest => ReplyCommandAsync(command: BridgeWire.Unload, phase: PhaseUnload, request: BridgeWire.Request(command: BridgeWire.Unload, payload: new BridgeUnloadRequest(SessionId: rest[0])), resultPath: null)),
            ["check"] = new(Usage: "check <project.csproj> [options]", FailurePhase: PhaseResolve, MinArgs: 1, MaxArgs: 999, Run: static rest => CheckProjectAsync(projectPath: rest[0], options: CliOptions.Parse(rest[1..]))),
            ["check-source"] = new(Usage: "check-source <source.cs> [--script <script.csx|.cs>] [options]", FailurePhase: PhaseResolve, MinArgs: 1, MaxArgs: 999, Run: static rest => CheckSourceAsync(sourcePath: rest[0], options: CliOptions.Parse(rest[1..]))),
            ["quit"] = new(Usage: "quit", FailurePhase: PhaseLifecycle, MinArgs: 0, MaxArgs: 0, Run: static _ => QuitAsync()),
        };
    public static async Task<int> Main(string[] args) {
        string command = args.Length > 0 ? args[0] : string.Empty;
        string[] rest = args.Length > 1 ? args[1..] : [];
        ClientCommand? route = Commands.GetValueOrDefault(key: command);
        try {
            return route is null ? Usage() : await route.InvokeAsync(args: rest).ConfigureAwait(false);
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException or Win32Exception or UnauthorizedAccessException) {
            return route is not null
                ? PrintFailure(command: command, phase: route.FailurePhase, rest: rest, error: error)
                : BridgeWire.ExitCode(status: BridgeWire.Failed);
        }
    }
    private static int PrintFailure(string command, string phase, IReadOnlyList<string> rest, Exception error) {
        BridgeResult result = BridgeResult.From(command: command, phases: [BridgePhase.Failed(phase: phase, message: error.Message, fault: BridgeFault.FromException(category: phase, error: error))]);
        return PrintResult(result: result, path: ResultPath(args: rest));
    }
    private static string? ResultPath(IReadOnlyList<string> args) =>
        args.Select((value, index) => new { value, index })
            .Where(item => string.Equals(a: item.value, b: "--result", comparisonType: StringComparison.Ordinal) && item.index + 1 < args.Count)
            .Select(item => args[item.index + 1])
            .FirstOrDefault();
    internal static int Usage() {
        Console.Error.WriteLine("Usage:");
        foreach (KeyValuePair<string, ClientCommand> command in Commands.OrderBy(static item => item.Key, StringComparer.Ordinal)) {
            Console.Error.WriteLine($"  rhino-bridge-client {command.Value.Usage}");
        }
        Console.Error.WriteLine("Options: --configuration <name> --worktree <path> --timeout-ms <ms> --result <path> --script <script.csx|.cs>");
        Console.Error.WriteLine("--timeout-ms controls client transport waits; RhinoCode execution is synchronous inside Rhino.");
        Console.Error.WriteLine("Launch env: RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app or RHINO_WIP_BUNDLE_ID=com.mcneel.rhinoceros.9");
        return 2;
    }
    private static async Task<int> LaunchCommandAsync() {
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "launch", phases: [launch, connect]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> RestartAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgePhase quit = await PhaseAsync(phase: PhaseLifecycle, work: async () => {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: PhaseLifecycle, reply: reply);
        }).ConfigureAwait(false);
        bool exited = !BridgeWire.IsOk(status: quit.Status)
            || await WaitForExitAsync(pid: endpoint.RhinoPid, timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (BridgeWire.IsOk(status: quit.Status)) {
            quit = exited ? quit : BridgePhase.Failed(phase: PhaseLifecycle, message: $"Rhino process {endpoint.RhinoPid.ToString(provider: CultureInfo.InvariantCulture)} did not exit before restart timeout.");
        }
        BridgePhase launch = BridgeWire.IsOk(status: quit.Status)
            ? await LaunchPhaseAsync().ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseLaunch, message: "Lifecycle quit failed before restart launch.");
        bool canConnect = BridgeWire.IsOk(status: quit.Status)
            && (BridgeWire.IsOk(status: launch.Status) || string.Equals(a: launch.Status, b: BridgeWire.Skipped, comparisonType: StringComparison.Ordinal));
        BridgePhase connect = canConnect
            ? await ConnectPhaseAsync(timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseConnect, message: "Lifecycle quit failed before restart connect.");
        BridgeResult result = BridgeResult.From(command: "restart", phases: [quit, launch, connect]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> LoadAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase load = await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, packageCacheRoot: null, timeoutMs: options.TimeoutMs).ConfigureAwait(false);
        return PrintResult(result: BridgeResult.From(command: BridgeWire.Load, phases: [load]), path: options.Result);
    }
    private static async Task<int> LoadSmokeAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase load = BridgeWire.IsOk(status: connect.Status)
            ? await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, packageCacheRoot: null, timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseLoad, message: "Bridge connect failed before load-smoke.");
        BridgePhase unload = load.DataValue<BridgeLoadReport>() is { SessionId: string sessionId }
            ? await UnloadPhaseAsync(sessionId: sessionId).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseUnload, message: "No load session was created.");
        BridgeResult result = BridgeResult.From(command: "load-smoke", phases: [connect, load, unload]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> ScriptAsync(string scriptPath, CliOptions options) {
        string scriptFile = ExistingFile(path: scriptPath, label: "script");
        string script = await File.ReadAllTextAsync(path: scriptFile, encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        string worktree = await WorkspaceRootAsync(path: scriptFile, options: options).ConfigureAwait(false);
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase scriptServer = await RhinoCodeCliPhaseAsync().ConfigureAwait(false);
        BridgePhase execute = BridgeWire.IsOk(status: connect.Status)
            ? await ExecutePhaseAsync(script: script, scriptPath: scriptFile, worktree: worktree, references: [], timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseExecute, message: "Bridge connect failed before script execution.");
        BridgeResult result = BridgeResult.From(command: "script", phases: [launch, connect, scriptServer, execute]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<int> CheckProjectAsync(string projectPath, CliOptions options) {
        string project = ExistingFile(path: projectPath, label: "project");
        string worktree = await WorkspaceRootAsync(path: project, options: options).ConfigureAwait(false);
        BridgePhase resolve = BridgePhase.Ok(phase: PhaseResolve, data: new { projectPath = project, workspaceRoot = worktree });
        (BridgePhase buildPhase, ProjectBuild? buildProject) = await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false);
        return await CheckRuntimeAsync(
            command: "check",
            resolve: resolve,
            build: buildPhase,
            project: buildProject,
            worktree: worktree,
            options: options,
            loadMessage: "RhinoCode check uses #r references without a separate bridge load session.",
            noRuntimeMessage: "Build failed before RhinoCode execution.",
            script: static projectBuild => Task.FromResult<(string Script, IReadOnlyList<string> References)?>((SmokeScript(project: projectBuild), projectBuild.HostFilteredRuntimeReferences))).ConfigureAwait(false);
    }
    private static async Task<int> CheckSourceAsync(string sourcePath, CliOptions options) {
        string source = ExistingFile(path: sourcePath, label: "source");
        string worktree = await WorkspaceRootAsync(path: source, options: options).ConfigureAwait(false);
        (BridgePhase resolvePhase, string? resolvedProject) = await ResolveSourcePhaseAsync(source: source, worktree: worktree, configuration: options.Configuration).ConfigureAwait(false);
        (BridgePhase buildPhase, ProjectBuild? buildProject) = resolvedProject is string project
            ? await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false)
            : (BridgePhase.Skipped(phase: PhaseBuild, message: "Source ownership could not be resolved."), null);
        return await CheckRuntimeAsync(
            command: "check-source",
            resolve: resolvePhase,
            build: buildPhase,
            project: buildProject,
            worktree: worktree,
            options: options,
            loadMessage: "Source checks use RhinoCode #r scripts without a separate bridge load session.",
            noRuntimeMessage: BridgeWire.IsOk(status: buildPhase.Status) && options.Script is null ? "Source build validated; no runtime script supplied." : "Source ownership or build failed before source script execution.",
            script: projectBuild => SourceScriptAsync(project: projectBuild, scriptPath: options.Script)).ConfigureAwait(false);
    }
    private static async Task<int> QuitAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "quit", phases: [BridgePhase.FromReply(phase: PhaseLifecycle, reply: reply)]);
        int exitCode = PrintResult(result: result, path: null);
        if (BridgeWire.IsOk(status: reply.Status)) {
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
            return BridgePhase.Skipped(phase: PhaseLaunch, timer: timer, data: new { reason = "Existing bridge endpoint answered.", endpoint = live.Data });
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            string? appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH");
            string bundleId = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_BUNDLE_ID") ?? DefaultRhinoWipBundleId;
            ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: string.IsNullOrWhiteSpace(value: appPath) ? ["-b", bundleId, "--args", "-nosplash"] : [appPath, "--args", "-nosplash"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
            timer.Stop();
            return opened.ExitCode == 0
                ? BridgePhase.Ok(phase: PhaseLaunch, timer: timer, data: new { bundleId, appPath }, outputs: opened.Outputs)
                : BridgePhase.Failed(phase: PhaseLaunch, timer: timer, message: "Failed to open RhinoWIP.", outputs: opened.Outputs);
        }
    }
    private static async Task<BridgePhase> ConnectPhaseAsync(TimeSpan timeout) {
        Stopwatch timer = Stopwatch.StartNew();
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);
        BridgePhase last = BridgePhase.Failed(phase: PhaseConnect, message: "Bridge did not answer before connect polling started.");
        while (DateTimeOffset.UtcNow < deadline) {
            try {
                BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeSpan.FromSeconds(value: 3.0)).ConfigureAwait(false);
                timer.Stop();
                BridgePhase phase = BridgePhase.FromReply(phase: PhaseConnect, reply: reply) with { DurationMs = (int)timer.ElapsedMilliseconds };
                if (BridgeWire.IsOk(status: phase.Status)) {
                    return phase;
                }
                last = phase;
                await Task.Delay(delay: TimeSpan.FromMilliseconds(value: 500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException) {
                last = BridgePhase.Failed(phase: PhaseConnect, message: error.Message, fault: BridgeFault.FromException(category: PhaseConnect, error: error));
                await Task.Delay(delay: TimeSpan.FromMilliseconds(value: 500.0), cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
        }
        timer.Stop();
        return last with { DurationMs = (int)timer.ElapsedMilliseconds };
    }
    private static async Task<BridgePhase> RhinoCodeCliPhaseAsync() {
        Stopwatch timer = Stopwatch.StartNew();
        try {
            string rhinoCodePath = Path.Combine(path1: Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH") ?? DefaultRhinoWipAppPath, path2: "Contents/Resources/bin/rhinocode");
            if (!File.Exists(path: rhinoCodePath)) {
                timer.Stop();
                return BridgePhase.Skipped(phase: PhaseRhinoCodeCli, timer: timer, data: new { reason = "rhinocode CLI was not found.", path = rhinoCodePath });
            }
            ProcessResult direct = await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0)).ConfigureAwait(false);
            ProcessResult rolled = direct.ExitCode == 0
                ? direct
                : await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0), environment: new Dictionary<string, string>(StringComparer.Ordinal) { ["DOTNET_ROLL_FORWARD"] = "Major" }).ConfigureAwait(false);
            timer.Stop();
            BridgeOutput[] outputs = [.. direct.Outputs.Concat(ReferenceEquals(objA: rolled, objB: direct) ? [] : rolled.Outputs)];
            object data = new { path = rhinoCodePath, directExitCode = direct.ExitCode, rollForwardExitCode = rolled.ExitCode, rollForward = !ReferenceEquals(objA: rolled, objB: direct) };
            return rolled.ExitCode == 0
                ? BridgePhase.Ok(phase: PhaseRhinoCodeCli, timer: timer, data: data, outputs: outputs)
                : BridgePhase.Failed(phase: PhaseRhinoCodeCli, timer: timer, message: "rhinocode list --json failed.", outputs: outputs);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException) {
            timer.Stop();
            return BridgePhase.Create<object>(phase: PhaseRhinoCodeCli, status: BridgeWire.Failed, durationMs: (int)timer.ElapsedMilliseconds, fault: BridgeFault.FromException(category: PhaseRhinoCodeCli, error: error));
        }
    }
    private static async Task<BridgePhase> LoadPhaseAsync(string assembly, string workspaceRoot, string? packageCacheRoot, int timeoutMs) =>
        await RequestPhaseAsync(
            phase: PhaseLoad,
            request: BridgeWire.Request(command: BridgeWire.Load, payload: new BridgeLoadRequest(AssemblyPath: assembly, WorkspaceRoot: workspaceRoot, PackageCacheRoot: packageCacheRoot), timeoutMs: timeoutMs),
            timeoutMs: timeoutMs).ConfigureAwait(false);
    private static async Task<BridgePhase> UnloadPhaseAsync(string sessionId) =>
        await RequestPhaseAsync(
            phase: PhaseUnload,
            request: BridgeWire.Request(command: BridgeWire.Unload, payload: new BridgeUnloadRequest(SessionId: sessionId)),
            timeoutMs: 15000).ConfigureAwait(false);
    private static async Task<BridgePhase> ExecutePhaseAsync(string script, string? scriptPath, string worktree, IReadOnlyList<string> references, int timeoutMs) {
        string stagedScript = scriptPath ?? StageScript(worktree: worktree, script: script);
        return await RequestPhaseAsync(
            phase: PhaseExecute,
            request: BridgeWire.Request(command: BridgeWire.Execute, payload: new BridgeExecuteRequest(Script: script, ScriptPath: stagedScript, References: references), timeoutMs: timeoutMs),
            timeoutMs: timeoutMs).ConfigureAwait(false);
    }
    private static async Task<BridgePhase> RequestPhaseAsync(string phase, BridgeRequest request, int timeoutMs) =>
        await PhaseAsync(phase: phase, work: async () => {
            BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout(timeoutMs: timeoutMs)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: phase, reply: reply);
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
        (string Message, string[] Arguments)[] steps = [
            (Message: "dotnet restore failed.", Arguments: ["restore", project, "--locked-mode"]),
            (Message: "dotnet build failed.", Arguments: ["build", project, "--configuration", configuration, "--no-restore"]),
            (Message: "dotnet msbuild ResolveReferences failed.", Arguments: ["msbuild", project, "-target:ResolveReferences", "-getProperty:TargetPath", "-getProperty:TargetDir", "-getProperty:TargetFramework", "-getProperty:AssemblyName", "-getProperty:TargetExt", "-getProperty:RestorePackagesPath", "-getItem:ReferenceCopyLocalPaths", "-getItem:ReferencePath", $"-p:Configuration={configuration}", "-p:RestoreLockedMode=true", "-nologo"]),
        ];
        List<BridgeOutput> outputs = [];
        List<BridgeOutput> buildOutputs = [];
        ProcessResult target = new(ExitCode: 1, Stdout: string.Empty, Stderr: string.Empty);
        for (int index = 0; index < steps.Length; index++) {
            (string message, string[] arguments) = steps[index];
            target = await ProcessResult.RunAsync(fileName: "dotnet", arguments: arguments, timeout: ProcessTimeout).ConfigureAwait(false);
            outputs.AddRange(collection: target.Outputs);
            if (index < 2) {
                buildOutputs.AddRange(collection: target.Outputs);
            }
            if (target.ExitCode != 0) {
                timer.Stop();
                return (BridgePhase.Failed(phase: PhaseBuild, timer: timer, message: message, outputs: outputs), null);
            }
        }
        try {
            ProjectBuild projectBuild = ProjectBuild.Parse(projectPath: project, configuration: configuration, json: target.Stdout);
            timer.Stop();
            return (BridgePhase.Ok(phase: PhaseBuild, timer: timer, data: projectBuild, outputs: buildOutputs), projectBuild);
        } catch (Exception error) when (error is JsonException or InvalidOperationException or ArgumentException) {
            timer.Stop();
            return (BridgePhase.Failed(phase: PhaseBuild, message: "MSBuild reference projection could not be parsed.", fault: BridgeFault.FromException(category: PhaseBuild, error: error)) with { DurationMs = (int)timer.ElapsedMilliseconds, Outputs = target.Outputs }, null);
        }
    }
    private static async Task<(BridgePhase Resolve, string? Project)> ResolveSourcePhaseAsync(string source, string worktree, string configuration) {
        Stopwatch timer = Stopwatch.StartNew();
        ProcessResult tracked = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", worktree, "ls-files", "*.csproj"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (tracked.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Failed(phase: PhaseResolve, timer: timer, message: "git ls-files failed during project discovery.", outputs: tracked.Outputs), null);
        }
        string[] projects = [.. tracked.Stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(path => Path.GetFullPath(path: Path.Combine(path1: worktree, path2: path)))];
        SourceOwnerEvaluation[] evaluations = await Task.WhenAll(projects.Select(project => SourceOwnerEvaluation.ResolveAsync(project: project, source: source, configuration: configuration))).ConfigureAwait(false);
        SourceOwnerEvaluation[] failures = [.. evaluations.Where(static evaluation => evaluation.Failed)];
        SourceOwner[] owners = [.. evaluations.Select(static evaluation => evaluation.Owner).OfType<SourceOwner>()];
        timer.Stop();
        return failures.Length > 0 ? (BridgePhase.Failed(
                phase: PhaseResolve,
                timer: timer,
                category: PhaseResolve,
                message: $"MSBuild source-owner evaluation failed for {failures.Length.ToString(provider: CultureInfo.InvariantCulture)} tracked project(s).",
                data: new { sourcePath = source, configuration, failures },
                outputs: [.. failures.SelectMany(static failure => failure.Outputs)]), null)
            : owners.Length switch {
                1 => (BridgePhase.Ok(phase: PhaseResolve, timer: timer, data: new { sourcePath = source, projectPath = owners[0].ProjectPath, link = owners[0].Link }), owners[0].ProjectPath),
                0 => (BridgePhase.Failed<object>(phase: PhaseResolve, timer: timer, category: "source", message: $"No tracked SDK project owns source file: {source}", data: null, outputs: tracked.Outputs), null),
                _ => (BridgePhase.Failed(phase: PhaseResolve, timer: timer, category: "ambiguous", message: $"Multiple projects own source file: {source}", data: new { sourcePath = source, candidates = owners }, outputs: tracked.Outputs), null),
            };
    }
    private static async Task<int> CheckRuntimeAsync(string command, BridgePhase resolve, BridgePhase build, ProjectBuild? project, string worktree, CliOptions options, string loadMessage, string noRuntimeMessage, Func<ProjectBuild, Task<(string Script, IReadOnlyList<string> References)?>> script) {
        (string Script, IReadOnlyList<string> References)? checkScript = project is { } projectBuild && BridgeWire.IsOk(status: build.Status)
            ? await script(projectBuild).ConfigureAwait(false)
            : null;
        bool canRun = checkScript is not null && BridgeWire.IsOk(status: build.Status);
        BridgePhase launch = canRun ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseLaunch, message: noRuntimeMessage);
        BridgePhase connect = canRun ? await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseConnect, message: noRuntimeMessage);
        BridgePhase scriptServer = BridgeWire.IsOk(status: connect.Status) ? await RhinoCodeCliPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseRhinoCodeCli, message: "Bridge connect failed before RhinoCode CLI discovery.");
        BridgePhase load = BridgePhase.Skipped(phase: PhaseLoad, message: loadMessage);
        Task<BridgePhase> executeTask = (checkScript, BridgeWire.IsOk(status: connect.Status), BridgeWire.IsOk(status: build.Status)) switch {
            ( { } current, true, _) => ExecutePhaseAsync(
                script: current.Script,
                scriptPath: null,
                worktree: worktree,
                references: current.References,
                timeoutMs: options.TimeoutMs),
            (null, _, true) => Task.FromResult(BridgePhase.Create<object>(phase: PhaseExecute, status: BridgeWire.Unsupported, fault: BridgeFault.MessageOnly(category: BridgeWire.Unsupported, message: noRuntimeMessage))),
            _ => Task.FromResult(BridgePhase.Skipped(phase: PhaseExecute, message: noRuntimeMessage)),
        };
        BridgePhase execute = await executeTask.ConfigureAwait(false);
        BridgePhase diagnostics = execute.Diagnostics.Count > 0
            ? BridgePhase.Ok(phase: PhaseDiagnostics, data: new { count = execute.Diagnostics.Count, sourcePhase = execute.Phase }, diagnostics: execute.Diagnostics)
            : BridgePhase.Skipped(phase: PhaseDiagnostics, message: "No RhinoCode diagnostics were reported.");
        BridgeResult result = BridgeResult.From(command: command, phases: [resolve, build, launch, connect, scriptServer, load, execute, diagnostics, BridgePhase.Skipped(phase: PhaseUnload, message: "No bridge load session was created."), BridgePhase.Skipped(phase: PhaseLifecycle, message: "No lifecycle action was requested.")]);
        return PrintResult(result: result, path: options.Result);
    }
    private static async Task<(string Script, IReadOnlyList<string> References)?> SourceScriptAsync(ProjectBuild project, string? scriptPath) =>
        scriptPath is string sourceScriptPath
            ? (string.Concat(
                    ReferenceDirectives(references: project.HostFilteredRuntimeReferences),
                    Environment.NewLine,
                    await File.ReadAllTextAsync(path: ExistingFile(path: sourceScriptPath, label: "script"), encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false)),
                project.HostFilteredRuntimeReferences)
            : null;
    private static string SmokeScript(ProjectBuild project) {
        string references = ReferenceDirectives(references: project.HostFilteredRuntimeReferences);
        string target = Escape(value: project.TargetPath);
        string assemblyName = Escape(value: project.AssemblyName ?? Path.GetFileNameWithoutExtension(path: project.TargetPath));
        string nonce = Guid.NewGuid().ToString(format: "N");
        return string.Join(separator: Environment.NewLine, values: [
            references,
            "using System;",
            "using System.Globalization;",
            "using System.IO;",
            "using System.Reflection;",
            "using Rhino;",
            string.Empty,
$"string targetLocation = Path.GetFullPath(\"{target}\");",
"AssemblyName targetName = AssemblyName.GetAssemblyName(targetLocation);",
$"Assembly? loadedAssembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), assembly => string.Equals(assembly.GetName().Name, \"{assemblyName}\", StringComparison.Ordinal));",
"Version? loadedVersion = loadedAssembly?.GetName().Version;",
            "string? preLoadLocation = loadedAssembly?.Location;",
            "bool alreadyLoaded = loadedAssembly is not null;",
            "bool staleLocation = alreadyLoaded && !string.Equals(Path.GetFullPath(preLoadLocation ?? string.Empty), targetLocation, OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);",
            "bool staleIdentity = alreadyLoaded && !Equals(loadedVersion, targetName.Version);",
            "bool staleLoaded = staleLocation || staleIdentity;",
            "if (staleLoaded) throw new InvalidOperationException($\"Already-loaded assembly does not match built target. loaded={preLoadLocation}; target={targetLocation}; loadedVersion={loadedVersion}; targetVersion={targetName.Version}\");",
            "Assembly targetAssembly = loadedAssembly ?? Assembly.LoadFrom(targetLocation);",
            "string postLoadLocation = targetAssembly.Location;",
$"Console.WriteLine(\"rasm.rhino-bridge.nonce={nonce}\");",
            "Console.WriteLine(\"loadedAssembly=\" + targetAssembly.FullName);",
            "Console.WriteLine(\"targetLocation=\" + targetLocation);",
            "Console.WriteLine(\"loadedLocation=\" + (string.IsNullOrWhiteSpace(postLoadLocation) ? \"none\" : postLoadLocation));",
            "Console.WriteLine(\"preLoadLocation=\" + (preLoadLocation ?? \"none\"));",
            "Console.WriteLine(\"alreadyLoaded=\" + alreadyLoaded);",
            "Console.WriteLine(\"assemblyVersion=\" + targetAssembly.GetName().Version);",
            "Console.WriteLine(\"targetAssemblyVersion=\" + targetName.Version);",
            "Console.WriteLine(\"assemblyInformationalVersion=\" + (targetAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? \"none\"));",
            "Console.WriteLine(\"rhinoVersion=\" + RhinoApp.Version);",
            "Console.WriteLine(\"activeDocument=\" + (RhinoDoc.ActiveDoc is not null));",
            "Console.WriteLine(\"modelAbsoluteTolerance=\" + (RhinoDoc.ActiveDoc?.ModelAbsoluteTolerance.ToString(CultureInfo.InvariantCulture) ?? \"none\"));",
$"Console.Error.WriteLine(\"rasm.rhino-bridge.stderr={nonce}\");",
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
            return BridgePhase.Create<object>(phase: phase, status: BridgeWire.Timeout, fault: BridgeFault.FromException(category: phase, error: error));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
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
            await writer.WriteLineAsync(buffer: BridgeWire.Serialize(request: request).AsMemory(), cancellationToken: cancellation.Token).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
            string? line = await reader.ReadLineAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
            BridgeReply reply = string.IsNullOrWhiteSpace(value: line)
                ? throw new InvalidOperationException(message: "Bridge returned no response.")
                : BridgeWire.DeserializeReply(json: line) ?? throw new InvalidOperationException(message: "Bridge returned an invalid response.");
            return BridgeWire.IsCurrent(schema: reply.Schema)
                ? reply
                : throw new InvalidOperationException(message: $"Bridge returned unsupported schema '{reply.Schema}'.");
        }
    }
    private static BridgeEndpoint ReadEndpoint() {
        BridgeEndpoint endpoint = BridgeWire.DeserializeEndpoint(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8))
            ?? throw new InvalidOperationException(message: $"Endpoint metadata is invalid: {BridgeWire.EndpointPath}");
        if (!BridgeWire.IsCurrent(schema: endpoint.Schema)) {
            throw new InvalidOperationException(message: $"Endpoint metadata has unsupported schema '{endpoint.Schema}': {BridgeWire.EndpointPath}");
        }
        using Process process = Process.GetProcessById(processId: endpoint.RhinoPid);
        DateTimeOffset startedAt = new(dateTime: process.StartTime.ToUniversalTime());
        bool validPipe = endpoint.PipeName.Length <= 64 && endpoint.PipeName.StartsWith(value: string.Create(CultureInfo.InvariantCulture, $"rb-{endpoint.RhinoPid}-"), comparisonType: StringComparison.Ordinal);
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
                return BridgeWire.ExitCode(status: failed.Status);
            }
        }
        Console.WriteLine(value: json);
        return BridgeWire.ExitCode(status: result.Status);
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
internal sealed record ClientCommand(string Usage, string FailurePhase, int MinArgs, int MaxArgs, Func<string[], Task<int>> Run) {
    internal Task<int> InvokeAsync(string[] args) =>
        args.Length >= MinArgs && args.Length <= MaxArgs ? Run(args) : Task.FromResult(Program.Usage());
}

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
    internal static BridgeResult From(string command, IReadOnlyList<BridgePhase> phases) {
        bool executeSucceeded = phases.Any(static phase => string.Equals(a: phase.Phase, b: Program.PhaseExecute, comparisonType: StringComparison.Ordinal) && BridgeWire.IsOk(status: phase.Status));
        BridgePhase[] decisive = [.. phases.Where(phase => phase.Phase switch {
            Program.PhaseRhinoCodeCli => !executeSucceeded && !BridgeWire.IsOk(status: phase.Status) && !string.Equals(a: phase.Status, b: BridgeWire.Skipped, comparisonType: StringComparison.Ordinal),
            Program.PhaseLoad or Program.PhaseUnload or Program.PhaseLifecycle => !string.Equals(a: phase.Status, b: BridgeWire.Skipped, comparisonType: StringComparison.Ordinal),
            _ => true,
        })];
        BridgePhase? execute = phases.FirstOrDefault(static phase => string.Equals(a: phase.Phase, b: Program.PhaseExecute, comparisonType: StringComparison.Ordinal));
        return new(
            Schema: BridgeWire.Schema,
            Command: command,
            Status: decisive.Select(static phase => phase.Status).Aggregate(seed: BridgeWire.Ok, func: BridgeWire.Worst),
            Phases: phases,
            Fault: execute is { Fault: not null } && !BridgeWire.IsOk(status: execute.Status)
                ? execute.Fault
                : decisive.FirstOrDefault(static phase => phase.Fault is not null)?.Fault);
    }
}

internal sealed record BridgePhase(string Phase, string Status, int DurationMs, JsonElement? Data, IReadOnlyList<BridgeOutput> Outputs, IReadOnlyList<BridgeDiagnostic> Diagnostics, BridgeFault? Fault) {
    internal static BridgePhase Create<TData>(
        string phase,
        string status,
        int durationMs = 0,
        TData? data = default,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(Phase: phase, Status: status, DurationMs: durationMs, Data: data is null ? null : JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: outputs ?? [], Diagnostics: diagnostics ?? [], Fault: fault);
    internal static BridgePhase Ok<TData>(string phase, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        Create(phase: phase, status: BridgeWire.Ok, data: data, outputs: outputs, diagnostics: diagnostics);
    internal static BridgePhase Ok<TData>(string phase, Stopwatch timer, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        Create(phase: phase, status: BridgeWire.Ok, durationMs: (int)timer.ElapsedMilliseconds, data: data, outputs: outputs, diagnostics: diagnostics);
    internal static BridgePhase Failed(string phase, string message, BridgeFault? fault = null) =>
        Create<object>(phase: phase, status: BridgeWire.Failed, fault: fault ?? BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed(string phase, Stopwatch timer, string message, IReadOnlyList<BridgeOutput>? outputs = null) =>
        Create<object>(phase: phase, status: BridgeWire.Failed, durationMs: (int)timer.ElapsedMilliseconds, outputs: outputs, fault: BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed<TData>(string phase, Stopwatch timer, string category, string message, TData? data, IReadOnlyList<BridgeOutput>? outputs = null) =>
        Create(phase: phase, status: BridgeWire.Failed, durationMs: (int)timer.ElapsedMilliseconds, data: data, outputs: outputs, fault: BridgeFault.MessageOnly(category: category, message: message));
    internal static BridgePhase Skipped(string phase, string message) =>
        Create(phase: phase, status: BridgeWire.Skipped, data: new { reason = message });
    internal static BridgePhase Skipped<TData>(string phase, Stopwatch timer, TData data) =>
        Create(phase: phase, status: BridgeWire.Skipped, durationMs: (int)timer.ElapsedMilliseconds, data: data);
    internal static BridgePhase FromReply(string phase, BridgeReply reply) =>
        BridgeWire.IsStatus(status: reply.Status)
            ? new(Phase: phase, Status: reply.Status, DurationMs: 0, Data: reply.Data, Outputs: reply.Outputs, Diagnostics: reply.Diagnostics, Fault: reply.Fault)
            : Failed(phase: phase, message: $"Bridge returned unsupported status '{reply.Status}'.", fault: BridgeFault.MessageOnly(category: phase, message: $"Bridge returned unsupported status '{reply.Status}'."));
    internal T? DataValue<T>() =>
        Data is JsonElement data ? data.Deserialize<T>(options: BridgeWire.CompactJson) : default;
}

internal sealed record ProjectBuild(string ProjectPath, string Configuration, string? TargetFramework, string? AssemblyName, string TargetPath, string? TargetDir, string? TargetExt, string? PackageCacheRoot, IReadOnlyList<string> References) {
    internal IReadOnlyList<string> RuntimeReferences =>
        [.. References.Where(reference => !string.Equals(a: Path.GetFullPath(path: reference), b: Path.GetFullPath(path: TargetPath), comparisonType: OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))];
    internal IReadOnlyList<string> HostFilteredRuntimeReferences =>
        [.. RuntimeReferences.Where(static reference => !IsHostReference(path: reference))];
    internal static ProjectBuild Parse(string projectPath, string configuration, string json) {
        using JsonDocument document = JsonDocument.Parse(json: json);
        JsonElement properties = document.RootElement.GetProperty(propertyName: "Properties");
        string targetPath = Existing(Text(properties: properties, name: "TargetPath"), label: "TargetPath");
        string? targetDir = Text(properties: properties, name: "TargetDir");
        string[] references = ReferencesOf(root: document.RootElement, targetPath: targetPath, targetDir: targetDir);
        return new(ProjectPath: projectPath, Configuration: configuration, TargetFramework: Text(properties: properties, name: "TargetFramework"), AssemblyName: Text(properties: properties, name: "AssemblyName"), TargetPath: targetPath, TargetDir: targetDir, TargetExt: Text(properties: properties, name: "TargetExt"), PackageCacheRoot: Text(properties: properties, name: "RestorePackagesPath"), References: references);
    }
    private static string[] ReferencesOf(JsonElement root, string targetPath, string? targetDir) =>
        [.. new[] { targetPath }
            .Concat(ItemPaths(root: root, itemName: "ReferenceCopyLocalPaths"))
            .Concat(DepsReferences(targetPath: targetPath, targetDir: targetDir))
            .Concat(ItemPaths(root: root, itemName: "ReferencePath"))
            .Select(Path.GetFullPath)
            .Where(static path => IsReferenceFile(path: path) && !IsFrameworkReferencePack(path: path))
            .DistinctBy(static path => ReferenceIdentity(path: path), StringComparer.OrdinalIgnoreCase)];
    private static bool IsReferenceFile(string path) =>
        File.Exists(path: path)
        && (path.EndsWith(value: ".dll", comparisonType: StringComparison.OrdinalIgnoreCase) || path.EndsWith(value: ".rhp", comparisonType: StringComparison.OrdinalIgnoreCase));
    private static bool IsFrameworkReferencePack(string path) =>
        path.Contains(value: "/packs/Microsoft.NETCore.App.Ref/", comparisonType: StringComparison.Ordinal)
        || path.Contains(value: "/packs/NETStandard.Library.Ref/", comparisonType: StringComparison.Ordinal);
    private static bool IsHostReference(string path) =>
        Path.GetFileNameWithoutExtension(path: path) is "RhinoCommon" or "Rhino.UI" or "Grasshopper" or "Grasshopper2" or "GrasshopperIO" or "Rasm.RhinoBridge.Protocol" or "Rasm.RhinoBridge.Plugin";
    private static string ReferenceIdentity(string path) {
        try {
            System.Reflection.AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(assemblyFile: path);
            return $"{name.Name}|{name.Version}";
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or BadImageFormatException or FileLoadException or ArgumentException) {
            return Path.GetFileName(path: path);
        }
    }
    private static string? Text(JsonElement properties, string name) =>
        properties.TryGetProperty(propertyName: name, value: out JsonElement value) ? value.GetString() : null;
    private static string Existing(string? path, string label) =>
        path is string full && File.Exists(path: full) ? full : throw new InvalidOperationException(message: $"MSBuild did not return an existing {label}: {path}");
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

internal sealed record SourceOwner(string ProjectPath, string? Link);

internal sealed record SourceOwnerEvaluation(string ProjectPath, IReadOnlyList<string> Command, int ExitCode, IReadOnlyList<BridgeOutput> Outputs, SourceOwner? Owner, BridgeFault? Fault) {
    internal bool Failed => ExitCode != 0 || Fault is not null;
    internal static async Task<SourceOwnerEvaluation> ResolveAsync(string project, string source, string configuration) {
        string[] arguments = ["msbuild", project, "-getProperty:TargetPath", "-getItem:Compile", $"-p:Configuration={configuration}", "-nologo"];
        ProcessResult result = await ProcessResult.RunAsync(fileName: "dotnet", arguments: arguments, timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false);
        BridgeFault? exitFault = result.ExitCode == 0 ? null : BridgeFault.MessageOnly(category: Program.PhaseResolve, message: $"MSBuild source-owner evaluation failed for project: {project}");
        try {
            SourceOwner? owner = result.ExitCode == 0 ? From(project: project, source: RealPath(path: source), json: result.Stdout) : null;
            return new(ProjectPath: project, Command: ["dotnet", .. arguments], ExitCode: result.ExitCode, Outputs: result.Outputs, Owner: owner, Fault: exitFault);
        } catch (Exception error) when (error is JsonException or InvalidOperationException or ArgumentException) {
            return new(ProjectPath: project, Command: ["dotnet", .. arguments], ExitCode: result.ExitCode, Outputs: result.Outputs, Owner: null, Fault: BridgeFault.FromException(category: Program.PhaseResolve, error: error));
        }
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
    private const int OutputLimit = 16384;
    internal IReadOnlyList<BridgeOutput> Outputs => [
        BridgeWire.Capture(source: BridgeWire.OutputCommandStdout, text: Stdout, limit: OutputLimit),
        BridgeWire.Capture(source: BridgeWire.OutputCommandStderr, text: Stderr, limit: OutputLimit),
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
            throw new TimeoutException(message: $"Process timed out after {timeout.TotalSeconds.ToString(provider: CultureInfo.InvariantCulture)}s: {fileName} {string.Join(separator: ' ', values: arguments)}", innerException: error);
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
