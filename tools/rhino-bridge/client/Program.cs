using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.RhinoBridge.Client;

// --- [COMPOSITION] ----------------------------------------------------------------------
internal static class Program {
    private const string DefaultRhinoWipBundleId = "com.mcneel.rhinoceros.9";
    private const string DefaultRhinoWipAppPath = "/Applications/RhinoWIP.app";
    internal const string PhaseBuild = "build";
    internal const string PhaseClean = "clean";
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
    private static readonly Encoding OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(value: 5.0);
    public static async Task<int> Main(string[] args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        Fin<ClientVerb> parsed = ClientVerb.Parse(args: args);
        ClientVerb? verb = parsed.IsSucc ? (ClientVerb)parsed : null;
        try {
            return verb is null ? ClientVerb.Usage() : await verb.RunAsync().ConfigureAwait(continueOnCapturedContext: false);
        } catch (Exception error) when (error is IOException or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException or TimeoutException or Win32Exception or UnauthorizedAccessException) {
            string command = args.Length > 0 ? args[0] : string.Empty;
            string phase = verb?.FailurePhase ?? command;
            return PrintFailure(command: command, phase: phase, rest: args.Length > 1 ? args[1..] : [], error: error);
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
    internal static async Task<int> LaunchCommandAsync() {
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "launch", phases: [launch, connect]);
        return PrintResult(result: result, path: null);
    }
    internal static async Task<int> RestartAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgePhase quit = await PhaseAsync(phase: PhaseLifecycle, work: async () => {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: PhaseLifecycle, reply: reply);
        }).ConfigureAwait(false);
        bool exited = !quit.Status.IsOk
            || await WaitForExitAsync(pid: endpoint.RhinoPid, timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (quit.Status.IsOk) {
            quit = exited ? quit : BridgePhase.Failed(phase: PhaseLifecycle, message: $"Rhino process {endpoint.RhinoPid.ToString(provider: CultureInfo.InvariantCulture)} did not exit before restart timeout.");
        }
        BridgePhase launch = quit.Status.IsOk
            ? await LaunchPhaseAsync().ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseLaunch, message: "Lifecycle quit failed before restart launch.");
        bool canConnect = quit.Status.IsOk
            && (launch.Status.IsOk || !launch.Status.IsDecisive);
        BridgePhase connect = canConnect
            ? await ConnectPhaseAsync(timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseConnect, message: "Lifecycle quit failed before restart connect.");
        BridgeResult result = BridgeResult.From(command: "restart", phases: [quit, launch, connect]);
        return PrintResult(result: result, path: null);
    }
    internal static async Task<int> LoadAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase load = await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, packageCacheRoot: null, timeoutMs: options.TimeoutMs).ConfigureAwait(false);
        return PrintResult(result: BridgeResult.From(command: BridgeWire.Load, phases: [load]), path: options.Result);
    }
    internal static async Task<int> LoadSmokeAsync(string assemblyPath, CliOptions options) {
        string assembly = ExistingFile(path: assemblyPath, label: "assembly");
        string workspaceRoot = await WorkspaceRootAsync(path: assembly, options: options).ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase load = connect.Status.IsOk
            ? await LoadPhaseAsync(assembly: assembly, workspaceRoot: workspaceRoot, packageCacheRoot: null, timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseLoad, message: "Bridge connect failed before load-smoke.");
        BridgePhase unload = load.DataValue<BridgeReport.Load>() is { SessionId: string sessionId }
            ? await UnloadPhaseAsync(sessionId: sessionId).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseUnload, message: "No load session was created.");
        BridgeResult result = BridgeResult.From(command: "load-smoke", phases: [connect, load, unload]);
        return PrintResult(result: result, path: options.Result);
    }
    internal static async Task<int> CheckTargetAsync(string targetPath, string? scenarioPath, CliOptions options) {
        string target = ExistingFile(path: targetPath, label: "target");
        return Path.GetExtension(path: target).ToUpperInvariant() switch {
            ".CSPROJ" => await CheckProjectAsync(projectPath: target, scenarioPath: scenarioPath, options: options).ConfigureAwait(false),
            ".CSX" => scenarioPath is null
                ? await CheckScriptAsync(scriptPath: target, options: options).ConfigureAwait(false)
                : throw new InvalidOperationException(message: "Script targets do not accept a second scenario path."),
            ".CS" => await CheckSourceAsync(sourcePath: target, scenarioPath: scenarioPath, options: options).ConfigureAwait(false),
            string extension => throw new InvalidOperationException(message: $"Unsupported check target extension '{extension}': {target}"),
        };
    }
    internal static async Task<int> CleanAsync(string targetPath) {
        string target = Path.GetFullPath(path: targetPath);
        string worktree = await WorktreeAsync(path: target).ConfigureAwait(false);
        string reportDir = ReportDirectory(worktree: worktree, target: target);
        string reportRoot = ReportRoot(worktree: worktree);
        string fullRoot = Path.GetFullPath(path: reportRoot);
        string fullDir = Path.GetFullPath(path: reportDir);
        bool safe = fullDir.StartsWith(value: fullRoot + Path.DirectorySeparatorChar, comparisonType: PathComparison);
        if (!safe) {
            throw new InvalidOperationException(message: $"Refusing to clean outside bridge check artifacts: {fullDir}");
        }
        bool deleted = Directory.Exists(path: fullDir);
        if (deleted) {
            Directory.Delete(path: fullDir, recursive: true);
        }
        BridgeResult result = BridgeResult.From(command: "clean", phases: [BridgePhase.Ok(phase: PhaseClean, data: new { targetPath = target, reportDir = fullDir, deleted })]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> CheckScriptAsync(string scriptPath, CliOptions options) {
        string scriptFile = ExistingFile(path: scriptPath, label: "script");
        string script = await File.ReadAllTextAsync(path: scriptFile, encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        string worktree = await WorkspaceRootAsync(path: scriptFile, options: options).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: scriptFile);
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false);
        BridgePhase scriptServer = await RhinoCodeCliPhaseAsync().ConfigureAwait(false);
        BridgePhase execute = connect.Status.IsOk
            ? await ExecutePhaseAsync(script: script, scriptPath: scriptFile, worktree: worktree, references: [], timeoutMs: options.TimeoutMs).ConfigureAwait(false)
            : BridgePhase.Skipped(phase: PhaseExecute, message: "Bridge connect failed before script execution.");
        BridgeResult result = BridgeResult.From(command: "check", phases: [launch, connect, scriptServer, execute]);
        return PrintResult(result: result, path: resultPath);
    }
    private static async Task<int> CheckProjectAsync(string projectPath, string? scenarioPath, CliOptions options) {
        string project = ExistingFile(path: projectPath, label: "project");
        string worktree = await WorkspaceRootAsync(path: project, options: options).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: project);
        BridgePhase resolve = BridgePhase.Ok(phase: PhaseResolve, data: new { projectPath = project, workspaceRoot = worktree });
        (BridgePhase buildPhase, ProjectBuild? buildProject) = await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false);
        return await CheckRuntimeAsync(
            command: "check",
            resolve: resolve,
            build: buildPhase,
            project: buildProject,
            worktree: worktree,
            options: options,
            resultPath: resultPath,
            loadMessage: "RhinoCode check uses #r references without a separate bridge load session.",
            noRuntimeMessage: "Build failed before RhinoCode execution.",
            script: (projectBuild, reportPath) => ProjectScriptAsync(project: projectBuild, scriptPath: scenarioPath, resultPath: reportPath)).ConfigureAwait(false);
    }
    private static async Task<int> CheckSourceAsync(string sourcePath, string? scenarioPath, CliOptions options) {
        string source = ExistingFile(path: sourcePath, label: "source");
        string worktree = await WorkspaceRootAsync(path: source, options: options).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: source);
        (BridgePhase resolvePhase, string? resolvedProject) = await ResolveSourcePhaseAsync(source: source, worktree: worktree, configuration: options.Configuration).ConfigureAwait(false);
        (BridgePhase buildPhase, ProjectBuild? buildProject) = resolvedProject is string project
            ? await BuildPhaseAsync(project: project, configuration: options.Configuration).ConfigureAwait(false)
            : (BridgePhase.Skipped(phase: PhaseBuild, message: "Source ownership could not be resolved."), null);
        return await CheckRuntimeAsync(
            command: "check",
            resolve: resolvePhase,
            build: buildPhase,
            project: buildProject,
            worktree: worktree,
            options: options,
            resultPath: resultPath,
            loadMessage: "Source checks use RhinoCode #r scripts without a separate bridge load session.",
            noRuntimeMessage: buildPhase.Status.IsOk && scenarioPath is null ? "Source build validated; no runtime script supplied." : "Source ownership or build failed before source script execution.",
            script: (projectBuild, reportPath) => ScenarioScriptAsync(project: projectBuild, scriptPath: scenarioPath, resultPath: reportPath)).ConfigureAwait(false);
    }
    internal static async Task<int> QuitAsync() {
        BridgeEndpoint endpoint = ReadEndpoint();
        BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout(timeoutMs: 15000)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "quit", phases: [BridgePhase.FromReply(phase: PhaseLifecycle, reply: reply)]);
        int exitCode = PrintResult(result: result, path: null);
        if (reply.Status.IsOk) {
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
                if (phase.Status.IsOk) {
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
            return BridgePhase.Create<object>(phase: PhaseRhinoCodeCli, status: PhaseStatus.Failed, durationMs: (int)timer.ElapsedMilliseconds, fault: BridgeFault.FromException(category: PhaseRhinoCodeCli, error: error));
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
    private static async Task<BridgePhase> ExecutePhaseAsync(string script, string? scriptPath, string worktree, IReadOnlyList<string> references, int timeoutMs, string? stageDirectory = null) {
        string stagedScript = scriptPath ?? StageScript(worktree: worktree, script: script, stageDirectory: stageDirectory);
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
    private static string StageScript(string worktree, string script, string? stageDirectory) {
        string root = stageDirectory ?? Path.Combine(path1: worktree, path2: ".artifacts/rhino/bridge", path3: string.Create(provider: CultureInfo.InvariantCulture, $"execute-{Environment.ProcessId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"));
        _ = Directory.CreateDirectory(path: root);
        string path = Path.Combine(path1: root, path2: "script.csx");
        BoundaryIO.Write(path: path, contents: script, encoding: Encoding.UTF8);
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
    private static async Task<int> CheckRuntimeAsync(string command, BridgePhase resolve, BridgePhase build, ProjectBuild? project, string worktree, CliOptions options, string resultPath, string loadMessage, string noRuntimeMessage, Func<ProjectBuild, string, Task<(string Script, IReadOnlyList<string> References)?>> script) {
        (string Script, IReadOnlyList<string> References)? checkScript = project is { } projectBuild && build.Status.IsOk
            ? await script(projectBuild, resultPath).ConfigureAwait(false)
            : null;
        bool canRun = checkScript is not null && build.Status.IsOk;
        BridgePhase launch = canRun ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseLaunch, message: noRuntimeMessage);
        BridgePhase connect = canRun ? await ConnectPhaseAsync(timeout: TransportTimeout(timeoutMs: options.TimeoutMs)).ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseConnect, message: noRuntimeMessage);
        BridgePhase scriptServer = connect.Status.IsOk ? await RhinoCodeCliPhaseAsync().ConfigureAwait(false) : BridgePhase.Skipped(phase: PhaseRhinoCodeCli, message: "Bridge connect failed before RhinoCode CLI discovery.");
        BridgePhase load = BridgePhase.Skipped(phase: PhaseLoad, message: loadMessage);
        Task<BridgePhase> executeTask = (checkScript, connect.Status.IsOk, build.Status.IsOk) switch {
            ( { } current, true, _) => ExecutePhaseAsync(
                script: current.Script,
                scriptPath: null,
                worktree: worktree,
                references: current.References,
                timeoutMs: options.TimeoutMs,
                stageDirectory: Path.GetDirectoryName(path: resultPath)),
            (null, _, true) => Task.FromResult(BridgePhase.Create<object>(phase: PhaseExecute, status: PhaseStatus.Unsupported, fault: BridgeFault.MessageOnly(category: PhaseStatus.Unsupported.Wire, message: noRuntimeMessage))),
            _ => Task.FromResult(BridgePhase.Skipped(phase: PhaseExecute, message: noRuntimeMessage)),
        };
        BridgePhase execute = await executeTask.ConfigureAwait(false);
        BridgePhase diagnostics = execute.Diagnostics.Count > 0
            ? BridgePhase.Ok(phase: PhaseDiagnostics, data: new { count = execute.Diagnostics.Count, sourcePhase = execute.Phase }, diagnostics: execute.Diagnostics)
            : BridgePhase.Skipped(phase: PhaseDiagnostics, message: "No RhinoCode diagnostics were reported.");
        BridgeResult result = BridgeResult.From(command: command, phases: [resolve, build, launch, connect, scriptServer, load, execute, diagnostics, BridgePhase.Skipped(phase: PhaseUnload, message: "No bridge load session was created."), BridgePhase.Skipped(phase: PhaseLifecycle, message: "No lifecycle action was requested.")]);
        return PrintResult(result: result, path: resultPath);
    }
    private static async Task<(string Script, IReadOnlyList<string> References)?> ProjectScriptAsync(ProjectBuild project, string? scriptPath, string resultPath) =>
        scriptPath is string sourceScriptPath
            ? await SourceScenarioScriptAsync(project: project, scriptPath: ExistingFile(path: sourceScriptPath, label: "script"), resultPath: resultPath).ConfigureAwait(false)
            : SmokeScript(project: project, resultPath: resultPath);
    private static async Task<(string Script, IReadOnlyList<string> References)?> ScenarioScriptAsync(ProjectBuild project, string? scriptPath, string resultPath) =>
        scriptPath is string sourceScriptPath
            ? await SourceScenarioScriptAsync(project: project, scriptPath: ExistingFile(path: sourceScriptPath, label: "script"), resultPath: resultPath).ConfigureAwait(false)
            : null;
    private static async Task<(string Script, IReadOnlyList<string> References)> SourceScenarioScriptAsync(ProjectBuild project, string scriptPath, string resultPath) {
        IReadOnlyList<string> references = ScenarioScriptReferences(project: project, resultPath: resultPath);
        string[] script = await File.ReadAllLinesAsync(path: scriptPath, encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        string? embeddedReference = script.Select((line, index) => new { Line = line.TrimStart(), Number = index + 1 })
            .Where(static item => SourceScenarioReferenceLine(line: item.Line))
            .Select(static item => $"{item.Number.ToString(provider: CultureInfo.InvariantCulture)}:{item.Line}")
            .FirstOrDefault();
        if (embeddedReference is not null) {
            throw new InvalidOperationException(message: $"Scenario references are bridge-owned; remove '#r' or '#load' from {scriptPath}:{embeddedReference}");
        }
        string scenario = ScenarioName(scriptPath: scriptPath);
        string capture = Path.Combine(path1: Path.GetDirectoryName(path: resultPath) ?? Environment.CurrentDirectory, path2: $"{Path.GetFileNameWithoutExtension(path: resultPath)}.png");
        int foundBodyIndex = System.Array.FindIndex(array: script, match: static line => !SourceScenarioPreambleLine(line: line));
        int bodyIndex = foundBodyIndex < 0 ? script.Length : foundBodyIndex;
        return (
            string.Join(separator: Environment.NewLine, values: new[] {
                ReferenceDirectives(references: references),
            }.Concat(script.Take(count: bodyIndex)).Concat([
                "using Rasm.TestKit.Scenarios;",
                $"const string SCENARIO_NAME = \"{Escape(value: scenario)}\";",
                $"const string CAPTURE_PATH = \"{Escape(value: capture)}\";",
            ]).Concat(script.Skip(count: bodyIndex))),
            references);
    }
    private static IReadOnlyList<string> ScenarioScriptReferences(ProjectBuild project, string resultPath) =>
        ShadowReferences(
            targetPath: project.TargetPath,
            references: new[] { project.TargetPath }
                .Concat(project.HostFilteredRuntimeReferences)
                .Concat(ScenarioKitArtifacts(project: project))
                .Distinct(StringComparer.OrdinalIgnoreCase),
            resultPath: resultPath);
    private static IEnumerable<string> ScenarioKitArtifacts(ProjectBuild project) {
        string? worktreeRoot = LocateWorktreeRoot(project: project);
        if (worktreeRoot is null) {
            yield break;
        }
        string framework = project.TargetFramework ?? "net10.0";
        string testKitArtifact = Path.Combine(paths: [worktreeRoot, "tests/csharp/_testkit/bin", project.Configuration, framework, "Rasm.TestKit.dll"]);
        if (File.Exists(path: testKitArtifact)) {
            yield return testKitArtifact;
        }
        string protocolArtifact = Path.Combine(paths: [worktreeRoot, "tools/rhino-bridge/protocol/bin", project.Configuration, framework, "Rasm.RhinoBridge.Protocol.dll"]);
        if (File.Exists(path: protocolArtifact)) {
            yield return protocolArtifact;
        }
    }
    private static string? LocateWorktreeRoot(ProjectBuild project) {
        string? dir = Path.GetDirectoryName(path: project.ProjectPath);
        while (!string.IsNullOrEmpty(value: dir)) {
            if (Directory.Exists(path: Path.Combine(path1: dir, path2: "tests/csharp/_testkit"))) {
                return dir;
            }
            dir = Path.GetDirectoryName(path: dir);
        }
        return null;
    }
    private static bool SourceScenarioPreambleLine(string line) =>
        string.IsNullOrWhiteSpace(value: line)
        || line.TrimStart().StartsWith(value: "//", comparisonType: StringComparison.Ordinal)
        || SourceScenarioReferenceLine(line: line)
        || line.TrimStart().StartsWith(value: "using ", comparisonType: StringComparison.Ordinal)
        || line.TrimStart().StartsWith(value: "using static ", comparisonType: StringComparison.Ordinal);
    private static bool SourceScenarioReferenceLine(string line) =>
        line.TrimStart().StartsWith(value: "#r ", comparisonType: StringComparison.Ordinal)
        || line.TrimStart().StartsWith(value: "#load ", comparisonType: StringComparison.Ordinal);
    private static string ScenarioName(string scriptPath) =>
        Path.GetFileName(path: scriptPath).Replace(oldValue: ".verify.csx", newValue: string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase);
    private static (string Script, IReadOnlyList<string> References) SmokeScript(ProjectBuild project, string resultPath) {
        IReadOnlyList<string> scriptReferences = ScriptReferences(project: project, resultPath: resultPath);
        string references = ReferenceDirectives(references: scriptReferences);
        string target = Escape(value: scriptReferences[0]);
        string sourceTarget = Escape(value: project.TargetPath);
        string nonce = Guid.NewGuid().ToString(format: "N");
        return (string.Join(separator: Environment.NewLine, values: [
            references,
            "using System;",
            "using System.Globalization;",
            "using System.IO;",
            "using System.Reflection;",
            "using System.Text.Json;",
            "using Rhino;",
            string.Empty,
$"string targetLocation = Path.GetFullPath(\"{target}\");",
$"string sourceTargetLocation = Path.GetFullPath(\"{sourceTarget}\");",
"AssemblyName targetName = AssemblyName.GetAssemblyName(targetLocation);",
"StringComparison pathComparison = OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;",
"Assembly loadedByPath = Assembly.LoadFile(targetLocation);",
"Assembly[] nameMatches = Array.FindAll(AppDomain.CurrentDomain.GetAssemblies(), assembly => string.Equals(assembly.GetName().Name, targetName.Name, StringComparison.Ordinal));",
"Assembly? targetAssembly = Array.Find(nameMatches, assembly => ReferenceEquals(assembly, loadedByPath) || (!string.IsNullOrWhiteSpace(assembly.Location) && string.Equals(Path.GetFullPath(assembly.Location), targetLocation, pathComparison)));",
"Assembly? otherAssembly = Array.Find(nameMatches, assembly => !ReferenceEquals(assembly, targetAssembly));",
"Version? loadedVersion = targetAssembly?.GetName().Version;",
            "string? preLoadLocation = otherAssembly?.Location;",
            "bool alreadyLoaded = otherAssembly is not null;",
            "string postLoadLocation = targetAssembly?.Location ?? \"none\";",
            "string loadedVersionText = loadedVersion?.ToString() ?? \"none\";",
            "string targetVersionText = targetName.Version?.ToString() ?? \"none\";",
            "string assemblyVersionText = targetAssembly?.GetName().Version?.ToString() ?? \"none\";",
            "string assemblyInformationalVersion = targetAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? \"none\";",
            $"Console.WriteLine(\"{BridgeWire.ReturnPrefix}\" + JsonSerializer.Serialize(new {{ kind = \"assemblyFreshness\", nonce = \"{nonce}\", sourceTargetLocation, targetLocation, loadedLocation = string.IsNullOrWhiteSpace(postLoadLocation) ? \"none\" : postLoadLocation, preLoadLocation = preLoadLocation ?? \"none\", alreadyLoaded, sameNameAssemblyCount = nameMatches.Length, refreshRequired = targetAssembly is null, loadedVersion = loadedVersionText, targetVersion = targetVersionText, assemblyVersion = assemblyVersionText, assemblyInformationalVersion, resolverIsolated = true }}));",
            "Assembly targetAssemblyFresh = targetAssembly ?? throw new InvalidOperationException($\"RhinoCode did not load target assembly through isolated #r reference. target={targetLocation}; sameName={preLoadLocation ?? \"none\"}\");",
$"Console.WriteLine(\"rasm.rhino-bridge.nonce={nonce}\");",
            "Console.WriteLine(\"loadedAssembly=\" + targetAssemblyFresh.FullName);",
            "Console.WriteLine(\"targetLocation=\" + targetLocation);",
            "Console.WriteLine(\"loadedLocation=\" + (string.IsNullOrWhiteSpace(postLoadLocation) ? \"none\" : postLoadLocation));",
            "Console.WriteLine(\"preLoadLocation=\" + (preLoadLocation ?? \"none\"));",
            "Console.WriteLine(\"alreadyLoaded=\" + alreadyLoaded);",
            "Console.WriteLine(\"assemblyVersion=\" + targetAssemblyFresh.GetName().Version);",
            "Console.WriteLine(\"targetAssemblyVersion=\" + targetName.Version);",
            "Console.WriteLine(\"assemblyInformationalVersion=\" + assemblyInformationalVersion);",
            "Console.WriteLine(\"rhinoVersion=\" + RhinoApp.Version);",
            "Console.WriteLine(\"activeDocument=\" + (RhinoDoc.ActiveDoc is not null));",
            "Console.WriteLine(\"modelAbsoluteTolerance=\" + (RhinoDoc.ActiveDoc?.ModelAbsoluteTolerance.ToString(CultureInfo.InvariantCulture) ?? \"none\"));",
$"Console.Error.WriteLine(\"rasm.rhino-bridge.stderr={nonce}\");",
        ]), scriptReferences);
    }
    private static IReadOnlyList<string> ScriptReferences(ProjectBuild project, string resultPath) =>
        ShadowReferences(
            targetPath: project.TargetPath,
            references: new[] { project.TargetPath }.Concat(project.HostFilteredRuntimeReferences).Distinct(StringComparer.OrdinalIgnoreCase),
            resultPath: resultPath);
    private static IReadOnlyList<string> ShadowReferences(string targetPath, IEnumerable<string> references, string resultPath) {
        string target = Path.GetFullPath(path: targetPath);
        ReferenceFile[] ordered = [.. references
            .Select(Path.GetFullPath)
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static reference => ReferenceFile.From(path: reference))
            .OrderBy(reference => string.Equals(a: reference.Path, b: target, comparisonType: PathComparison) ? 0 : 1)
            .ThenBy(static reference => reference.Identity, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static reference => reference.Path, StringComparer.OrdinalIgnoreCase)];
        string fingerprint = ContentFingerprint(references: ordered);
        string root = Path.Combine(path1: Path.GetDirectoryName(path: resultPath) ?? Environment.CurrentDirectory, path2: "refs", path3: fingerprint);
        _ = Directory.CreateDirectory(path: root);
        Dictionary<string, int> seen = new(StringComparer.OrdinalIgnoreCase);
        return [.. ordered.Select(reference => {
            string file = Path.GetFileName(path: reference.Path);
            int index = seen.GetValueOrDefault(key: file);
            seen[file] = index + 1;
            string directory = index == 0 ? root : Path.Combine(path1: root, path2: index.ToString(format: "00", provider: CultureInfo.InvariantCulture));
            _ = Directory.CreateDirectory(path: directory);
            string target = Path.Combine(path1: directory, path2: file);
            string temp = string.Create(provider: CultureInfo.InvariantCulture, $"{target}.{Environment.ProcessId}.tmp");
            File.Copy(sourceFileName: reference.Path, destFileName: temp, overwrite: true);
            File.Move(sourceFileName: temp, destFileName: target, overwrite: true);
            return target;
        })];
    }
    private static string ContentFingerprint(IReadOnlyList<ReferenceFile> references) {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (ReferenceFile reference in references) {
            byte[] identity = Encoding.UTF8.GetBytes(s: reference.Identity);
            byte[] content = File.ReadAllBytes(path: reference.Path);
            hash.AppendData(data: identity);
            hash.AppendData(data: [0]);
            hash.AppendData(data: content);
            hash.AppendData(data: [0]);
        }
        return Convert.ToHexString(inArray: hash.GetHashAndReset())[..32];
    }
    private static string ReferenceDirectives(IReadOnlyList<string> references) =>
        string.Join(separator: Environment.NewLine, values: references.Select(static reference => $"#r \"{Escape(value: reference)}\""));
    private static string Escape(string value) =>
        value.Replace(oldValue: "\\", newValue: "\\\\", comparisonType: StringComparison.Ordinal).Replace(oldValue: "\"", newValue: "\\\"", comparisonType: StringComparison.Ordinal);
    private static async Task<BridgePhase> PhaseAsync(string phase, Func<Task<BridgePhase>> work) {
        try {
            return await work().ConfigureAwait(false);
        } catch (TimeoutException error) {
            return BridgePhase.Create<object>(phase: phase, status: PhaseStatus.Timeout, fault: BridgeFault.FromException(category: phase, error: error));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception or JsonException or InvalidOperationException or OperationCanceledException or ArgumentException) {
            return BridgePhase.Failed(phase: phase, message: error.Message, fault: BridgeFault.FromException(category: phase, error: error));
        }
    }
    internal static async Task<int> ReplyCommandAsync(string command, string phase, BridgeRequest request, string? resultPath) {
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
        string? fullPath = path is null ? null : Path.GetFullPath(path: path);
        BridgeResult published = fullPath is null ? result : result with { ReportPath = fullPath };
        string json = JsonSerializer.Serialize(value: published, options: BridgeWire.PrettyJson);
        if (fullPath is not null) {
            try {
                BoundaryIO.Write(path: fullPath, contents: json + Environment.NewLine, encoding: OutputEncoding);
            } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException) {
                BridgeResult failed = BridgeResult.From(command: result.Command, phases: [BridgePhase.Failed(phase: "result", message: $"Could not write result file: {path}", fault: BridgeFault.FromException(category: "result", error: error))]);
                Console.WriteLine(value: JsonSerializer.Serialize(value: failed, options: BridgeWire.PrettyJson));
                return failed.Status.Exit;
            }
        }
        Console.WriteLine(value: json);
        return result.Status.Exit;
    }
    private static string DefaultResultPath(string worktree, string target) =>
        Path.Combine(path1: ReportDirectory(worktree: worktree, target: target), path2: string.Create(provider: CultureInfo.InvariantCulture, $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}-{Path.GetFileName(path: target)}.json"));
    private static string ReportDirectory(string worktree, string target) =>
        Path.Combine(path1: ReportRoot(worktree: worktree), path2: TargetReportPath(worktree: worktree, target: target));
    private static string ReportRoot(string worktree) =>
        Path.Combine(path1: worktree, path2: ".artifacts/rhino/bridge/check");
    private static string TargetReportPath(string worktree, string target) {
        string relative = Path.GetRelativePath(relativeTo: worktree, path: target);
        string reportPath = relative.StartsWith(value: ".." + Path.DirectorySeparatorChar, comparisonType: StringComparison.Ordinal)
            || Path.IsPathRooted(path: relative)
                ? Path.Combine(path1: "_external", path2: SanitizePath(path: target))
                : relative;
        return SanitizePath(path: reportPath);
    }
    private static string SanitizePath(string path) =>
        Path.Combine(paths: [.. path.Split(separator: [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], options: StringSplitOptions.RemoveEmptyEntries).Select(static part => string.Concat(values: part.Select(static character => Path.GetInvalidFileNameChars().Contains(value: character) ? '_' : character)))]);
    private static StringComparison PathComparison =>
        OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
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
[Union]
internal abstract partial record ClientVerb {
    internal sealed record Doctor(CliOptions Options) : ClientVerb;
    internal sealed record Launch : ClientVerb;
    internal sealed record Restart : ClientVerb;
    internal sealed record LoadSmoke(string AssemblyPath, CliOptions Options) : ClientVerb;
    internal sealed record Load(string AssemblyPath, CliOptions Options) : ClientVerb;
    internal sealed record Unload(string SessionId) : ClientVerb;
    internal sealed record Check(string TargetPath, Option<string> ScenarioPath, CliOptions Options) : ClientVerb;
    internal sealed record Clean(string TargetPath) : ClientVerb;
    internal sealed record Quit : ClientVerb;
    private static readonly (string Name, string Synopsis)[] Synopses = [
        ("doctor", "doctor [options]"),
        ("launch", "launch"),
        ("restart", "restart"),
        ("load-smoke", "load-smoke <assembly> [options]"),
        ("load", "load <assembly> [options]"),
        ("unload", "unload <session>"),
        ("check", "check <target> [scenario.csx|scenario.verify.csx] [options]"),
        ("clean", "clean <target>"),
        ("quit", "quit"),
    ];
    internal string FailurePhase => Switch(
        doctor: static _ => BridgeWire.Doctor,
        launch: static _ => Program.PhaseLaunch,
        restart: static _ => Program.PhaseLifecycle,
        loadSmoke: static _ => Program.PhaseResolve,
        load: static _ => Program.PhaseLoad,
        unload: static _ => Program.PhaseUnload,
        check: static _ => Program.PhaseResolve,
        clean: static _ => Program.PhaseClean,
        quit: static _ => Program.PhaseLifecycle);
    internal Task<int> RunAsync() => Switch(
        doctor: static d => Program.ReplyCommandAsync(command: BridgeWire.Doctor, phase: BridgeWire.Doctor, request: BridgeWire.Request(command: BridgeWire.Doctor, timeoutMs: d.Options.TimeoutMs), resultPath: d.Options.Result),
        launch: static _ => Program.LaunchCommandAsync(),
        restart: static _ => Program.RestartAsync(),
        loadSmoke: static s => Program.LoadSmokeAsync(assemblyPath: s.AssemblyPath, options: s.Options),
        load: static l => Program.LoadAsync(assemblyPath: l.AssemblyPath, options: l.Options),
        unload: static u => Program.ReplyCommandAsync(command: BridgeWire.Unload, phase: Program.PhaseUnload, request: BridgeWire.Request(command: BridgeWire.Unload, payload: new BridgeUnloadRequest(SessionId: u.SessionId)), resultPath: null),
        check: static c => Program.CheckTargetAsync(targetPath: c.TargetPath, scenarioPath: c.ScenarioPath.IsSome ? c.ScenarioPath.IfNone(string.Empty) : null, options: c.Options),
        clean: static c => Program.CleanAsync(targetPath: c.TargetPath),
        quit: static _ => Program.QuitAsync());
    internal static Fin<ClientVerb> Parse(string[] args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        return args switch {
            { Length: 0 } => Fin.Fail<ClientVerb>(error: Error.New(message: "Bridge command missing.")),
            _ => args[0] switch {
                "doctor" => Fin.Succ<ClientVerb>(value: new Doctor(Options: CliOptions.Parse(args: args[1..]))),
                "launch" when args.Length == 1 => Fin.Succ<ClientVerb>(value: new Launch()),
                "restart" when args.Length == 1 => Fin.Succ<ClientVerb>(value: new Restart()),
                "load-smoke" when args.Length >= 2 => Fin.Succ<ClientVerb>(value: new LoadSmoke(AssemblyPath: args[1], Options: CliOptions.Parse(args: args[2..]))),
                "load" when args.Length >= 2 => Fin.Succ<ClientVerb>(value: new Load(AssemblyPath: args[1], Options: CliOptions.Parse(args: args[2..]))),
                "unload" when args.Length == 2 => Fin.Succ<ClientVerb>(value: new Unload(SessionId: args[1])),
                "check" when args.Length >= 2 => ParseCheck(args: args[1..]),
                "clean" when args.Length == 2 => Fin.Succ<ClientVerb>(value: new Clean(TargetPath: args[1])),
                "quit" when args.Length == 1 => Fin.Succ<ClientVerb>(value: new Quit()),
                string verb => Fin.Fail<ClientVerb>(error: Error.New(message: $"Unsupported bridge command '{verb}'.")),
            },
        };
    }
    private static Fin<ClientVerb> ParseCheck(string[] args) {
        CliInvocation invocation = CliOptions.ParseInvocation(args: args);
        return invocation.Positionals.Count switch {
            1 => Fin.Succ<ClientVerb>(value: new Check(TargetPath: invocation.Positionals[0], ScenarioPath: Option<string>.None, Options: invocation.Options)),
            2 => Fin.Succ<ClientVerb>(value: new Check(TargetPath: invocation.Positionals[0], ScenarioPath: Some(value: invocation.Positionals[1]), Options: invocation.Options)),
            _ => Fin.Fail<ClientVerb>(error: Error.New(message: "Usage: check <target> [scenario.csx|scenario.verify.csx] [options]")),
        };
    }
    internal static int Usage() {
        Console.Error.WriteLine(value: "Usage:");
        foreach ((_, string synopsis) in Synopses) {
            Console.Error.WriteLine(value: $"  rhino-bridge-client {synopsis}");
        }
        Console.Error.WriteLine(value: "Options: --configuration <name> --worktree <path> --timeout-ms <ms> --result <path>");
        Console.Error.WriteLine(value: "--timeout-ms controls client transport waits; RhinoCode execution is synchronous inside Rhino.");
        Console.Error.WriteLine(value: "Launch env: RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app or RHINO_WIP_BUNDLE_ID=com.mcneel.rhinoceros.9");
        return 2;
    }
}

internal sealed record CliOptions(string? Worktree, string Configuration, int TimeoutMs, string? Result) {
    private static CliOptions Default =>
        new(Worktree: null, Configuration: Environment.GetEnvironmentVariable(variable: "CONFIGURATION") ?? "Release", TimeoutMs: 30000, Result: null);
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
                "--worktree" => ParseInvocation(args: args, index: index + 2, current: current with { Worktree = Value(args: args, index: index, option: "--worktree") }, positionals: positionals),
                "--configuration" => ParseInvocation(args: args, index: index + 2, current: current with { Configuration = Value(args: args, index: index, option: "--configuration") }, positionals: positionals),
                "--timeout-ms" => ParseInvocation(args: args, index: index + 2, current: current with { TimeoutMs = ParseTimeout(value: Value(args: args, index: index, option: "--timeout-ms")) }, positionals: positionals),
                "--result" => ParseInvocation(args: args, index: index + 2, current: current with { Result = Value(args: args, index: index, option: "--result") }, positionals: positionals),
                string unknown when unknown.StartsWith(value: "--", comparisonType: StringComparison.Ordinal) => throw new InvalidOperationException(message: $"Unknown bridge option: {unknown}"),
                string value => ParseInvocation(args: args, index: index + 1, current: current, positionals: [.. positionals, value]),
            };
    private static string Value(IReadOnlyList<string> args, int index, string option) =>
        (index + 1) < args.Count ? args[index + 1] : throw new InvalidOperationException(message: $"Missing value for {option}.");
    private static int ParseTimeout(string value) =>
        int.TryParse(s: value, provider: CultureInfo.InvariantCulture, result: out int parsed) && parsed > 0
            ? parsed
            : throw new InvalidOperationException(message: $"Invalid --timeout-ms value: {value}");
}

internal sealed record CliInvocation(CliOptions Options, IReadOnlyList<string> Positionals);

[SmartEnum<int>]
internal sealed partial class PhaseClassification {
    public static readonly PhaseClassification Decisive = new(key: 0);
    public static readonly PhaseClassification Advisory = new(key: 1);
    public static readonly PhaseClassification Lifecycle = new(key: 2);
    public bool IsDecisive(bool executeSucceeded, PhaseStatus status) {
        ArgumentNullException.ThrowIfNull(argument: status);
        return Key switch {
            0 => true,
            1 => !executeSucceeded && !status.IsOk && status.IsDecisive,
            _ => status.IsDecisive,
        };
    }
    internal static PhaseClassification Of(string phaseName) =>
        phaseName switch {
            Program.PhaseRhinoCodeCli => Advisory,
            Program.PhaseLoad or Program.PhaseUnload or Program.PhaseLifecycle => Lifecycle,
            _ => Decisive,
        };
}

internal readonly record struct PhaseAggregate(PhaseStatus Status, BridgeFault? Fault) {
    internal static readonly PhaseAggregate Identity = new(Status: PhaseStatus.Ok, Fault: null);
    internal PhaseAggregate Combine(BridgePhase phase, bool executeSucceeded) =>
        PhaseClassification.Of(phaseName: phase.Phase).IsDecisive(executeSucceeded: executeSucceeded, status: phase.Status)
            ? new(Status: Status.Worst(other: phase.Status), Fault: Fault ?? phase.Fault)
            : this;
}

internal sealed record BridgeResult(string Schema, string Command, PhaseStatus Status, string? ReportPath, IReadOnlyList<BridgePhase> Phases, BridgeFault? Fault) {
    internal static BridgeResult From(string command, IReadOnlyList<BridgePhase> phases) {
        bool executeSucceeded = phases.Any(predicate: static phase => string.Equals(a: phase.Phase, b: Program.PhaseExecute, comparisonType: StringComparison.Ordinal) && phase.Status.IsOk);
        BridgePhase? execute = phases.FirstOrDefault(predicate: static phase => string.Equals(a: phase.Phase, b: Program.PhaseExecute, comparisonType: StringComparison.Ordinal));
        PhaseAggregate aggregate = phases.Aggregate(seed: PhaseAggregate.Identity, func: (acc, phase) => acc.Combine(phase: phase, executeSucceeded: executeSucceeded));
        return new(
            Schema: BridgeWire.Schema,
            Command: command,
            Status: aggregate.Status,
            ReportPath: null,
            Phases: phases,
            Fault: execute is { Fault: not null } && !execute.Status.IsOk ? execute.Fault : aggregate.Fault);
    }
}

internal sealed record BridgePhase(string Phase, PhaseStatus Status, int DurationMs, JsonElement? Data, IReadOnlyList<BridgeOutput> Outputs, IReadOnlyList<BridgeDiagnostic> Diagnostics, BridgeFault? Fault) {
    internal static BridgePhase Create<TData>(
        string phase,
        PhaseStatus status,
        int durationMs = 0,
        TData? data = default,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(Phase: phase, Status: status, DurationMs: durationMs, Data: data is null ? null : JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson), Outputs: outputs ?? [], Diagnostics: diagnostics ?? [], Fault: fault);
    internal static BridgePhase Ok<TData>(string phase, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        Create(phase: phase, status: PhaseStatus.Ok, data: data, outputs: outputs, diagnostics: diagnostics);
    internal static BridgePhase Ok<TData>(string phase, Stopwatch timer, TData data, IReadOnlyList<BridgeOutput>? outputs = null, IReadOnlyList<BridgeDiagnostic>? diagnostics = null) =>
        Create(phase: phase, status: PhaseStatus.Ok, durationMs: (int)timer.ElapsedMilliseconds, data: data, outputs: outputs, diagnostics: diagnostics);
    internal static BridgePhase Failed(string phase, string message, BridgeFault? fault = null) =>
        Create<object>(phase: phase, status: PhaseStatus.Failed, fault: fault ?? BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed(string phase, Stopwatch timer, string message, IReadOnlyList<BridgeOutput>? outputs = null) =>
        Create<object>(phase: phase, status: PhaseStatus.Failed, durationMs: (int)timer.ElapsedMilliseconds, outputs: outputs, fault: BridgeFault.MessageOnly(category: phase, message: message));
    internal static BridgePhase Failed<TData>(string phase, Stopwatch timer, string category, string message, TData? data, IReadOnlyList<BridgeOutput>? outputs = null) =>
        Create(phase: phase, status: PhaseStatus.Failed, durationMs: (int)timer.ElapsedMilliseconds, data: data, outputs: outputs, fault: BridgeFault.MessageOnly(category: category, message: message));
    internal static BridgePhase Skipped(string phase, string message) =>
        Create(phase: phase, status: PhaseStatus.Skipped, data: new { reason = message });
    internal static BridgePhase Skipped<TData>(string phase, Stopwatch timer, TData data) =>
        Create(phase: phase, status: PhaseStatus.Skipped, durationMs: (int)timer.ElapsedMilliseconds, data: data);
    internal static BridgePhase FromReply(string phase, BridgeReply reply) =>
        new(Phase: phase, Status: reply.Status, DurationMs: 0, Data: reply.Data, Outputs: reply.Outputs, Diagnostics: reply.Diagnostics, Fault: reply.Fault);
    internal T? DataValue<T>() =>
        Data is JsonElement data ? data.Deserialize<T>(options: BridgeWire.CompactJson) : default;
}

internal sealed record ReferenceFile(string Path, string Identity) {
    internal static ReferenceFile From(string path) =>
        new(Path: path, Identity: IdentityOf(path: path));
    private static string IdentityOf(string path) {
        try {
            System.Reflection.AssemblyName name = System.Reflection.AssemblyName.GetAssemblyName(assemblyFile: path);
            return string.Create(provider: CultureInfo.InvariantCulture, $"{name.Name}|{name.Version}|{File.GetLastWriteTimeUtc(path).Ticks}|{new FileInfo(fileName: path).Length}");
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or BadImageFormatException or FileLoadException or ArgumentException) {
            return string.Create(provider: CultureInfo.InvariantCulture, $"{System.IO.Path.GetFileName(path)}|{File.GetLastWriteTimeUtc(path).Ticks}|{new FileInfo(fileName: path).Length}");
        }
    }
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
        BridgeWire.IsHostAssemblyName(name: Path.GetFileNameWithoutExtension(path: path));
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
