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
    private const int TransportTimeoutMs = 30000;
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private static readonly Encoding OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(value: 5.0);
    internal static string Configuration => Environment.GetEnvironmentVariable(variable: "CONFIGURATION") ?? "Release";
    public static async Task<int> Main(string[] args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        Fin<ClientVerb> parsed = ClientVerb.Parse(args: args);
        ClientVerb? verb = parsed.IsSucc ? (ClientVerb)parsed : null;
        try {
            return verb is null ? ClientVerb.Usage() : await verb.RunAsync().ConfigureAwait(continueOnCapturedContext: false);
        } catch (Exception error) when (NonFatal(error: error)) {
            string command = args.Length > 0 ? args[0] : string.Empty;
            string phase = verb?.FailurePhase ?? command;
            return PrintFailure(command: command, phase: phase, rest: args.Length > 1 ? args[1..] : [], error: error);
        }
    }
    private static int PrintFailure(string command, string phase, IReadOnlyList<string> rest, Exception error) {
        BridgeResult result = BridgeResult.From(command: command, phases: [BridgePhase.Of(phase: phase, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: phase, error: error))]);
        return PrintResult(result: result, path: ResultPath(args: rest));
    }
    private static string? ResultPath(IReadOnlyList<string> args) =>
        args.Select((value, index) => new { value, index })
            .Where(item => string.Equals(a: item.value, b: "--result", comparisonType: StringComparison.Ordinal) && item.index + 1 < args.Count)
            .Select(item => args[item.index + 1])
            .FirstOrDefault();
    internal static async Task<int> LaunchAsync() {
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TimeSpan.FromSeconds(value: 45.0)).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "launch", phases: [launch, connect]);
        return PrintResult(result: result, path: null);
    }
    internal static async Task<int> CheckAsync(CheckTarget target, CliOptions options) =>
        await target.Switch(
            script: s => CheckScriptAsync(scriptPath: s.Path, options: options),
            project: p => CheckProjectAsync(projectPath: p.Path, scenarioPath: p.ScenarioPath.IsSome ? p.ScenarioPath.IfNone(string.Empty) : null, options: options),
            source: s => CheckSourceAsync(sourcePath: s.Path, scenarioPath: s.ScenarioPath.IsSome ? s.ScenarioPath.IfNone(string.Empty) : null, options: options)).ConfigureAwait(false);
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
        BridgeResult result = BridgeResult.From(command: "clean", phases: [BridgePhase.Of(phase: PhaseClean, status: PhaseStatus.Ok, data: new { targetPath = target, reportDir = fullDir, deleted })]);
        return PrintResult(result: result, path: null);
    }
    private static async Task<int> CheckScriptAsync(string scriptPath, CliOptions options) {
        string scriptFile = ExistingFile(path: scriptPath, label: "script");
        string script = await File.ReadAllTextAsync(path: scriptFile, encoding: Encoding.UTF8, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        string worktree = await WorktreeAsync(path: scriptFile).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: scriptFile);
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout).ConfigureAwait(false);
        BridgePhase scriptServer = await RhinoCodeCliPhaseAsync().ConfigureAwait(false);
        BridgePhase execute = connect.Status.IsOk
            ? await ExecutePhaseAsync(script: script, scriptPath: scriptFile, worktree: worktree, references: [], stageDirectory: null).ConfigureAwait(false)
            : BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Skipped, data: new { reason = "Bridge connect failed before script execution." });
        BridgeResult result = BridgeResult.From(command: "check", phases: [launch, connect, scriptServer, execute]);
        return PrintResult(result: result, path: resultPath);
    }
    private static async Task<int> CheckProjectAsync(string projectPath, string? scenarioPath, CliOptions options) {
        string project = ExistingFile(path: projectPath, label: "project");
        string worktree = await WorktreeAsync(path: project).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: project);
        BridgePhase resolve = BridgePhase.Of(phase: PhaseResolve, status: PhaseStatus.Ok, data: new { projectPath = project, workspaceRoot = worktree });
        (BridgePhase buildPhase, ProjectBuild? buildProject) = await BuildPhaseAsync(project: project).ConfigureAwait(false);
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
        string worktree = await WorktreeAsync(path: source).ConfigureAwait(false);
        string resultPath = options.Result ?? DefaultResultPath(worktree: worktree, target: source);
        (BridgePhase resolvePhase, string? resolvedProject) = await ResolveSourcePhaseAsync(source: source, worktree: worktree).ConfigureAwait(false);
        (BridgePhase buildPhase, ProjectBuild? buildProject) = resolvedProject is string project
            ? await BuildPhaseAsync(project: project).ConfigureAwait(false)
            : (BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Skipped, data: new { reason = "Source ownership could not be resolved." }), null);
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
        BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout).ConfigureAwait(false);
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
            return BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Skipped, timer: timer, data: new { reason = "Existing bridge endpoint answered.", endpoint = live.Data });
        } catch (Exception error) when (NonFatal(error: error)) {
            string? appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH");
            string bundleId = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_BUNDLE_ID") ?? DefaultRhinoWipBundleId;
            ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: string.IsNullOrWhiteSpace(value: appPath) ? ["-b", bundleId, "--args", "-nosplash"] : [appPath, "--args", "-nosplash"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
            timer.Stop();
            return opened.ExitCode == 0
                ? BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Ok, timer: timer, data: new { bundleId, appPath }, outputs: opened.Outputs)
                : BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Failed, timer: timer, message: "Failed to open RhinoWIP.", outputs: opened.Outputs);
        }
    }
    private static async Task<BridgePhase> ConnectPhaseAsync(TimeSpan timeout) {
        Stopwatch timer = Stopwatch.StartNew();
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);
        BridgePhase last = BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Failed, message: "Bridge did not answer before connect polling started.");
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
            } catch (Exception error) when (NonFatal(error: error)) {
                last = BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: PhaseConnect, error: error));
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
                return BridgePhase.Of(phase: PhaseRhinoCodeCli, status: PhaseStatus.Skipped, timer: timer, data: new { reason = "rhinocode CLI was not found.", path = rhinoCodePath });
            }
            ProcessResult direct = await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0)).ConfigureAwait(false);
            ProcessResult rolled = direct.ExitCode == 0
                ? direct
                : await ProcessResult.RunAsync(fileName: rhinoCodePath, arguments: ["list", "--json"], timeout: TimeSpan.FromSeconds(value: 10.0), environment: new Dictionary<string, string>(StringComparer.Ordinal) { ["DOTNET_ROLL_FORWARD"] = "Major" }).ConfigureAwait(false);
            timer.Stop();
            BridgeOutput[] outputs = [.. direct.Outputs.Concat(ReferenceEquals(objA: rolled, objB: direct) ? [] : rolled.Outputs)];
            object data = new { path = rhinoCodePath, directExitCode = direct.ExitCode, rollForwardExitCode = rolled.ExitCode, rollForward = !ReferenceEquals(objA: rolled, objB: direct) };
            return rolled.ExitCode == 0
                ? BridgePhase.Of(phase: PhaseRhinoCodeCli, status: PhaseStatus.Ok, timer: timer, data: data, outputs: outputs)
                : BridgePhase.Of(phase: PhaseRhinoCodeCli, status: PhaseStatus.Failed, timer: timer, message: "rhinocode list --json failed.", outputs: outputs);
        } catch (Exception error) when (NonFatal(error: error)) {
            timer.Stop();
            return BridgePhase.Of(phase: PhaseRhinoCodeCli, status: PhaseStatus.Failed, timer: timer, fault: BridgeFault.FromException(category: PhaseRhinoCodeCli, error: error));
        }
    }
    private static async Task<BridgePhase> ExecutePhaseAsync(string script, string? scriptPath, string worktree, IReadOnlyList<string> references, string? stageDirectory) {
        string stagedScript = scriptPath ?? StageScript(worktree: worktree, script: script, stageDirectory: stageDirectory);
        return await RequestPhaseAsync(
            phase: PhaseExecute,
            request: BridgeWire.Request(command: BridgeWire.Execute, payload: new BridgeExecuteRequest(Script: script, ScriptPath: stagedScript, References: references), timeoutMs: TransportTimeoutMs)).ConfigureAwait(false);
    }
    private static async Task<BridgePhase> RequestPhaseAsync(string phase, BridgeRequest request) =>
        await PhaseAsync(phase: phase, work: async () => {
            BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: phase, reply: reply);
        }).ConfigureAwait(false);
    private static string StageScript(string worktree, string script, string? stageDirectory) {
        string root = stageDirectory ?? Path.Combine(path1: worktree, path2: ".artifacts/rhino/bridge", path3: string.Create(provider: CultureInfo.InvariantCulture, $"execute-{Environment.ProcessId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"));
        _ = Directory.CreateDirectory(path: root);
        string path = Path.Combine(path1: root, path2: "script.csx");
        BoundaryIO.Write(path: path, contents: script, encoding: Encoding.UTF8);
        return path;
    }
    private static async Task<(BridgePhase Build, ProjectBuild? Project)> BuildPhaseAsync(string project) {
        Stopwatch timer = Stopwatch.StartNew();
        (string Message, string[] Arguments)[] steps = [
            (Message: "dotnet restore failed.", Arguments: ["restore", project, "--locked-mode"]),
            (Message: "dotnet build failed.", Arguments: ["build", project, "--configuration", Configuration, "--no-restore"]),
            (Message: "dotnet msbuild ResolveReferences failed.", Arguments: ["msbuild", project, "-target:ResolveReferences", "-getProperty:TargetPath", "-getProperty:TargetDir", "-getProperty:TargetFramework", "-getProperty:AssemblyName", "-getProperty:TargetExt", "-getProperty:RestorePackagesPath", "-getItem:ReferenceCopyLocalPaths", "-getItem:ReferencePath", $"-p:Configuration={Configuration}", "-p:RestoreLockedMode=true", "-nologo"]),
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
                return (BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Failed, timer: timer, message: message, outputs: outputs), null);
            }
        }
        try {
            ProjectBuild projectBuild = ProjectBuild.Parse(projectPath: project, configuration: Configuration, json: target.Stdout);
            timer.Stop();
            return (BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Ok, timer: timer, data: projectBuild, outputs: buildOutputs), projectBuild);
        } catch (Exception error) when (error is JsonException or InvalidOperationException or ArgumentException) {
            timer.Stop();
            return (BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: PhaseBuild, error: error)) with { DurationMs = (int)timer.ElapsedMilliseconds, Outputs = target.Outputs }, null);
        }
    }
    private static async Task<(BridgePhase Resolve, string? Project)> ResolveSourcePhaseAsync(string source, string worktree) {
        Stopwatch timer = Stopwatch.StartNew();
        ProcessResult tracked = await ProcessResult.RunAsync(fileName: "git", arguments: ["-C", worktree, "ls-files", "*.csproj"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
        if (tracked.ExitCode != 0) {
            timer.Stop();
            return (BridgePhase.Of(phase: PhaseResolve, status: PhaseStatus.Failed, timer: timer, message: "git ls-files failed during project discovery.", outputs: tracked.Outputs), null);
        }
        string[] projects = [.. tracked.Stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(path => Path.GetFullPath(path: Path.Combine(path1: worktree, path2: path)))];
        SourceOwnerEvaluation[] evaluations = await Task.WhenAll(projects.Select(project => SourceOwnerEvaluation.ResolveAsync(project: project, source: source, configuration: Configuration))).ConfigureAwait(false);
        SourceOwnerEvaluation[] failures = [.. evaluations.Where(static evaluation => evaluation.Failed)];
        SourceOwner[] owners = [.. evaluations.Select(static evaluation => evaluation.Owner).OfType<SourceOwner>()];
        timer.Stop();
        return failures.Length > 0
            ? (BridgePhase.Of(
                phase: PhaseResolve,
                status: PhaseStatus.Failed,
                timer: timer,
                category: PhaseResolve,
                message: $"MSBuild source-owner evaluation failed for {failures.Length.ToString(provider: CultureInfo.InvariantCulture)} tracked project(s).",
                data: new { sourcePath = source, configuration = Configuration, failures },
                outputs: [.. failures.SelectMany(static failure => failure.Outputs)]), null)
            : owners.Length switch {
                1 => (BridgePhase.Of(phase: PhaseResolve, status: PhaseStatus.Ok, timer: timer, data: new { sourcePath = source, projectPath = owners[0].ProjectPath, link = owners[0].Link }), owners[0].ProjectPath),
                0 => (BridgePhase.Of<object>(phase: PhaseResolve, status: PhaseStatus.Failed, timer: timer, category: "source", message: $"No tracked SDK project owns source file: {source}", data: null, outputs: tracked.Outputs), null),
                _ => (BridgePhase.Of(phase: PhaseResolve, status: PhaseStatus.Failed, timer: timer, category: "ambiguous", message: $"Multiple projects own source file: {source}", data: new { sourcePath = source, candidates = owners }, outputs: tracked.Outputs), null),
            };
    }
    private static async Task<int> CheckRuntimeAsync(string command, BridgePhase resolve, BridgePhase build, ProjectBuild? project, string worktree, CliOptions options, string resultPath, string loadMessage, string noRuntimeMessage, Func<ProjectBuild, string, Task<(string Script, IReadOnlyList<string> References)?>> script) {
        (string Script, IReadOnlyList<string> References)? checkScript = project is { } projectBuild && build.Status.IsOk
            ? await script(projectBuild, resultPath).ConfigureAwait(false)
            : null;
        bool canRun = checkScript is not null && build.Status.IsOk;
        BridgePhase launch = canRun ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage });
        BridgePhase connect = canRun ? await ConnectPhaseAsync(timeout: TransportTimeout).ConfigureAwait(false) : BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage });
        BridgePhase scriptServer = connect.Status.IsOk ? await RhinoCodeCliPhaseAsync().ConfigureAwait(false) : BridgePhase.Of(phase: PhaseRhinoCodeCli, status: PhaseStatus.Skipped, data: new { reason = "Bridge connect failed before RhinoCode CLI discovery." });
        BridgePhase load = BridgePhase.Of(phase: PhaseLoad, status: PhaseStatus.Skipped, data: new { reason = loadMessage });
        Task<BridgePhase> executeTask = (checkScript, connect.Status.IsOk, build.Status.IsOk) switch {
            ( { } current, true, _) => ExecutePhaseAsync(
                script: current.Script,
                scriptPath: null,
                worktree: worktree,
                references: current.References,
                stageDirectory: Path.GetDirectoryName(path: resultPath)),
            (null, _, true) => Task.FromResult(BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Unsupported, fault: BridgeFault.MessageOnly(category: PhaseStatus.Unsupported.Wire, message: noRuntimeMessage))),
            _ => Task.FromResult(BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage })),
        };
        BridgePhase execute = await executeTask.ConfigureAwait(false);
        BridgePhase diagnostics = execute.Diagnostics.Count > 0
            ? BridgePhase.Of(phase: PhaseDiagnostics, status: PhaseStatus.Ok, data: new { count = execute.Diagnostics.Count, sourcePhase = execute.Phase }, diagnostics: execute.Diagnostics)
            : BridgePhase.Of(phase: PhaseDiagnostics, status: PhaseStatus.Skipped, data: new { reason = "No RhinoCode diagnostics were reported." });
        BridgeResult result = BridgeResult.From(command: command, phases: [resolve, build, launch, connect, scriptServer, load, execute, diagnostics, BridgePhase.Of(phase: PhaseUnload, status: PhaseStatus.Skipped, data: new { reason = "No bridge load session was created." }), BridgePhase.Of(phase: PhaseLifecycle, status: PhaseStatus.Skipped, data: new { reason = "No lifecycle action was requested." })]);
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
        string target = scriptReferences[0];
        string sourceTarget = project.TargetPath;
        string nonce = Guid.NewGuid().ToString(format: "N");
        return (
            string.Join(separator: Environment.NewLine, values: [references, BridgeWire.SmokeTemplate(targetPath: target, sourceTargetPath: sourceTarget, nonce: nonce)]),
            scriptReferences);
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
            return BridgePhase.Of(phase: phase, status: PhaseStatus.Timeout, fault: BridgeFault.FromException(category: phase, error: error));
        } catch (Exception error) when (NonFatal(error: error)) {
            return BridgePhase.Of(phase: phase, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: phase, error: error));
        }
    }
    internal static async Task<int> ReplyCommandAsync(string command, string phase, BridgeRequest request, string? resultPath) {
        BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout).ConfigureAwait(false);
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
    private static TimeSpan TransportTimeout =>
        TimeSpan.FromMilliseconds(milliseconds: TransportTimeoutMs + 5000);
    private static bool NonFatal(Exception error) =>
        error is IOException or JsonException or InvalidOperationException
              or OperationCanceledException or ArgumentException
              or TimeoutException or Win32Exception or UnauthorizedAccessException;
    private static int PrintResult(BridgeResult result, string? path) {
        string? fullPath = path is null ? null : Path.GetFullPath(path: path);
        BridgeResult published = fullPath is null ? result : result with { ReportPath = fullPath };
        string json = JsonSerializer.Serialize(value: published, options: BridgeWire.PrettyJson);
        if (fullPath is not null) {
            try {
                BoundaryIO.Write(path: fullPath, contents: json + Environment.NewLine, encoding: OutputEncoding);
            } catch (Exception error) when (NonFatal(error: error)) {
                BridgeResult failed = BridgeResult.From(command: result.Command, phases: [BridgePhase.Of(phase: "result", status: PhaseStatus.Failed, message: $"Could not write result file: {path}", fault: BridgeFault.FromException(category: "result", error: error))]);
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
        doctor: static d => Program.ReplyCommandAsync(command: BridgeWire.Doctor, phase: BridgeWire.Doctor, request: BridgeWire.Request(command: BridgeWire.Doctor), resultPath: d.Options.Result),
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
