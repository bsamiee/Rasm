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
    internal const string PhaseBuild = "build";
    internal const string PhaseClean = "clean";
    internal const string PhaseConnect = "connect";
    internal const string PhaseDiagnostics = "diagnostics";
    internal const string PhaseExecute = "execute";
    internal const string PhaseLaunch = "launch";
    internal const string PhaseLifecycle = "lifecycle";
    internal const string PhaseLiveness = "liveness";
    internal const string PhaseResolve = "resolve";
    internal const string CategoryNugetLockDrift = "nuget-lock-drift";
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
        BridgePhase connect = await ConnectPhaseAsync(timeout: TimeoutPolicy.Default.Connect).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "launch", phases: [launch, connect]);
        return PrintResult(result: result, path: null);
    }
    internal static async Task<int> DoctorAsync(CliOptions options) {
        BridgePhase launch = await LaunchPhaseAsync().ConfigureAwait(false);
        BridgePhase connect = await ConnectPhaseAsync(timeout: TransportTimeout).ConfigureAwait(false);
        BridgePhase doctor = connect.Status.IsOk
            ? await RequestPhaseAsync(phase: BridgeWire.Doctor, request: BridgeWire.Request(command: BridgeWire.Doctor)).ConfigureAwait(false)
            : BridgePhase.Of(phase: BridgeWire.Doctor, status: PhaseStatus.Skipped, data: new { reason = "Bridge connect failed before doctor request." });
        BridgeResult result = BridgeResult.From(command: BridgeWire.Doctor, phases: [launch, connect, doctor]);
        return PrintResult(result: result, path: options.Result);
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
        BridgePhase execute = connect.Status.IsOk
            ? await ExecutePhaseAsync(script: script, scriptPath: scriptFile, worktree: worktree, references: [], hostPlugins: [], stageDirectory: null).ConfigureAwait(false)
            : BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Skipped, data: new { reason = "Bridge connect failed before script execution." });
        BridgePhase liveness = await LivenessPhaseAsync(execute: execute, isGrasshopperAware: false).ConfigureAwait(false);
        BridgeResult result = BridgeResult.From(command: "check", phases: [launch, connect, execute, liveness]);
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
            noRuntimeMessage: buildPhase.Status.IsOk && scenarioPath is null ? "Source build validated; no runtime script supplied." : "Source ownership or build failed before source script execution.",
            script: (projectBuild, reportPath) => ScenarioScriptAsync(project: projectBuild, scriptPath: scenarioPath, resultPath: reportPath)).ConfigureAwait(false);
    }
    internal static async Task<int> QuitAsync() {
        BridgeEndpoint? endpoint = TryReadEndpoint();
        BridgePhase lifecycle = endpoint is { } live
            ? await QuitPhaseAsync(endpoint: live).ConfigureAwait(false)
            : BridgePhase.Of(phase: PhaseLifecycle, status: PhaseStatus.Ok, data: new { reason = "No live Rhino endpoint; nothing to quit." });
        return PrintResult(result: BridgeResult.From(command: "quit", phases: [lifecycle]), path: null);
    }
    private static BridgeEndpoint? TryReadEndpoint() {
        try {
            return ReadEndpoint();
        } catch (Exception error) when (NonFatal(error: error)) {
            return null;
        }
    }
    private static async Task<BridgePhase> QuitPhaseAsync(BridgeEndpoint endpoint) {
        // Quit marks docs clean; client closes via Cocoa terminate then SIGKILL — RhinoApp.Exit self-SIGABRTs from the plugin idle frame.
        string prepared = await TryPrepareQuitAsync().ConfigureAwait(false);
        bool closed = await ReconcileAsync(endpoint: endpoint).ConfigureAwait(false);
        return BridgePhase.Of(phase: PhaseLifecycle, status: PhaseStatus.Ok, data: new { pid = endpoint.RhinoPid, prepared, closed });
    }
    private static async Task<string> TryPrepareQuitAsync() {
        try {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Quit), timeout: TransportTimeout).ConfigureAwait(false);
            return reply.Status.Wire;
        } catch (Exception error) when (NonFatal(error: error)) {
            return $"unreachable ({error.GetType().Name})";
        }
    }
    private static async Task<bool> ReconcileAsync(BridgeEndpoint? endpoint) {
        // Idempotent reconcile: force-close recorded PID, retire endpoint, clear macOS recovery markers (null endpoint clears markers only).
        bool closed = endpoint is { } live && await ForceCloseAsync(pid: live.RhinoPid).ConfigureAwait(false);
        RetireEndpoint(endpoint: endpoint);
        ClearRecoveryMarkers();
        return closed;
    }
    private static async Task<bool> ForceCloseAsync(int pid) {
        // RhinoWIP SIGTERM hits RhMacSignalHandler→abort(); use JXA NSRunningApplication.terminate (post-Quit clean docs), SIGKILL if wedged.
        string terminate = "ObjC.import('AppKit'); var a = $.NSRunningApplication.runningApplicationWithProcessIdentifier("
            + pid.ToString(provider: CultureInfo.InvariantCulture) + "); if (a) a.terminate;";
        _ = await ProcessResult.RunAsync(fileName: "osascript", arguments: ["-l", "JavaScript", "-e", terminate], timeout: TimeSpan.FromSeconds(value: 15.0)).ConfigureAwait(false);
        return await WaitForExitAsync(pid: pid, timeout: TimeoutPolicy.Default.QuitWait).ConfigureAwait(false)
            || (ForceKill(pid: pid) && await WaitForExitAsync(pid: pid, timeout: TimeoutPolicy.Default.QuitWait).ConfigureAwait(false));
    }
    private static bool ForceKill(int pid) {
        // BOUNDARY ADAPTER — SIGKILL a Rhino that ignored SIGTERM; an absent process counts as closed.
        try {
            using Process process = Process.GetProcessById(processId: pid);
            process.Kill(entireProcessTree: true);
            return true;
        } catch (Exception error) when (error is ArgumentException or InvalidOperationException or Win32Exception or NotSupportedException) {
            return true;
        }
    }
    private static void ClearRecoveryMarkers() {
        // BOUNDARY ADAPTER — delete RhinoWIP autosave/crash markers that wedge headless launch after unclean exit.
        if (!OperatingSystem.IsMacOS()) {
            return;
        }
        string library = Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library");
        string autosave = Path.Combine(path1: library, path2: "Autosave Information");
        string[] markers = [
            Path.Combine(path1: autosave, path2: "Unsaved RhinoWIP Document.3dm.rhl"),
            Path.Combine(path1: autosave, path2: "Unsaved RhinoWIP Document.3dm"),
            .. SafeReports(directory: Path.Combine(path1: library, path2: "Logs", path3: "DiagnosticReports")),
        ];
        System.Array.ForEach(array: markers, action: TryDelete);
    }
    private static string[] SafeReports(string directory) {
        // BOUNDARY ADAPTER — enumerate prior crash reports defensively; absence or permission denial yields an empty set.
        try {
            return Directory.Exists(path: directory) ? Directory.GetFiles(path: directory, searchPattern: "Rhinoceros-*.ips") : [];
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return [];
        }
    }
    private static void TryDelete(string path) {
        // BOUNDARY ADAPTER — best-effort marker cleanup; before-launch reconcile backstops async post-SIGKILL writes.
        try {
            File.Delete(path: path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or DirectoryNotFoundException) {
        }
    }
    private static void RetireEndpoint(BridgeEndpoint? endpoint) {
        // BOUNDARY ADAPTER — drop endpoint record after forced close; null endpoint is a no-op.
        if (endpoint is not { } record) {
            return;
        }
        try {
            BridgeEndpoint? current = BridgeWire.DeserializeEndpoint(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8));
            if (current is { } active && active.IsLiveFor(rhinoPid: record.RhinoPid, rhinoStartedAt: record.RhinoStartedAt) && active.MatchesPipe(reportedPipeName: record.PipeName)) {
                File.Delete(path: BridgeWire.EndpointPath);
            }
        } catch (Exception error) when (NonFatal(error: error)) {
        }
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
            BridgeReply live = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeoutPolicy.Default.Hello).ConfigureAwait(false);
            timer.Stop();
            return BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Skipped, timer: timer, data: new { reason = "Existing bridge endpoint answered.", endpoint = live.Data });
        } catch (Exception error) when (NonFatal(error: error)) {
            string? appPath = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH");
            if (string.IsNullOrWhiteSpace(value: appPath)) {
                timer.Stop();
                return BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Failed, timer: timer, message: "RHINO_WIP_APP_PATH is unset; launch resolves the bundle through the quality operator. Set it to a Rhino*.app path.");
            }
            // Hello failed: wedge may block the accept loop — reconcile before cold-open (`open` only activates a running app).
            bool reclaimed = await ReconcileAsync(endpoint: TryReadEndpoint()).ConfigureAwait(false);
            ProcessResult opened = await ProcessResult.RunAsync(fileName: "open", arguments: [appPath, "--args", "-nosplash"], timeout: TimeSpan.FromSeconds(value: 30.0)).ConfigureAwait(false);
            timer.Stop();
            return opened.ExitCode == 0
                ? BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Ok, timer: timer, data: new { appPath, reclaimedWedgedEndpoint = reclaimed }, outputs: opened.Outputs)
                : BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Failed, timer: timer, message: "Failed to open RhinoWIP.", outputs: opened.Outputs);
        }
    }
    private static async Task<BridgePhase> ConnectPhaseAsync(TimeSpan timeout) {
        // Poll Hello until Connect deadline; one delay site — each attempt waits for pipe availability inside SendAsync.
        Stopwatch timer = Stopwatch.StartNew();
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);
        TimeSpan poll = TimeSpan.FromMilliseconds(value: 250.0);
        BridgePhase last = BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Failed, message: "Bridge did not answer before connect polling started.");
        while (DateTimeOffset.UtcNow < deadline) {
            last = await TryConnectAsync(timer: timer).ConfigureAwait(false);
            if (last.Status.IsOk) {
                return last;
            }
            await Task.Delay(delay: poll, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
        timer.Stop();
        return last with { DurationMs = (int)timer.ElapsedMilliseconds };
    }
    private static async Task<BridgePhase> TryConnectAsync(Stopwatch timer) {
        try {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeoutPolicy.Default.Hello).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: PhaseConnect, reply: reply) with { DurationMs = (int)timer.ElapsedMilliseconds };
        } catch (Exception error) when (NonFatal(error: error)) {
            return BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: PhaseConnect, error: error));
        }
    }
    private static async Task<BridgePhase> ExecutePhaseAsync(string script, string? scriptPath, string worktree, IReadOnlyList<string> references, IReadOnlyList<string> hostPlugins, string? stageDirectory) {
        string stagedScript = scriptPath ?? StageScript(worktree: worktree, script: script, stageDirectory: stageDirectory);
        return await RequestPhaseAsync(
            phase: PhaseExecute,
            request: BridgeWire.Request(command: BridgeWire.Execute, payload: new BridgeExecuteRequest(Script: script, ScriptPath: stagedScript, References: references, HostPlugins: hostPlugins), timeoutMs: (int)TimeoutPolicy.Default.Transport.TotalMilliseconds)).ConfigureAwait(false);
    }
    private static async Task<BridgePhase> RequestPhaseAsync(string phase, BridgeRequest request) =>
        await PhaseAsync(phase: phase, work: async () => {
            BridgeReply reply = await SendAsync(request: request, timeout: TransportTimeout).ConfigureAwait(false);
            return BridgePhase.FromReply(phase: phase, reply: reply);
        }).ConfigureAwait(false);
    // A scenario can leave the host on a deferred AppKit fault (e.g. a GH2 editor StatusBar paint NPE) that SIGABRTs
    // Rhino seconds AFTER execute returns ok, so prove the host still answers Hello — a crash surfaces as a failed run
    // here instead of hiding behind the silent relaunch on the next invocation. The deferred-paint window only exists
    // for GH-aware scenarios (which can realize the editor); non-GH scenarios run synchronous RhinoCommon ops where an
    // immediate Hello catches any fault with no settle latency.
    private static async Task<BridgePhase> LivenessPhaseAsync(BridgePhase execute, bool isGrasshopperAware) {
        if (!execute.Status.IsOk) {
            return BridgePhase.Of(phase: PhaseLiveness, status: PhaseStatus.Skipped, data: new { reason = "Execute did not complete; host liveness not assessed." });
        }
        Stopwatch timer = Stopwatch.StartNew();
        if (isGrasshopperAware) {
            await Task.Delay(delay: TimeoutPolicy.Default.LivenessSettle, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
        try {
            BridgeReply reply = await SendAsync(request: BridgeWire.Request(command: BridgeWire.Hello), timeout: TimeoutPolicy.Default.Hello).ConfigureAwait(false);
            timer.Stop();
            return BridgePhase.Of(phase: PhaseLiveness, status: PhaseStatus.Ok, timer: timer, data: new { survived = true, endpoint = reply.Data });
        } catch (Exception error) when (NonFatal(error: error)) {
            timer.Stop();
            return BridgePhase.Of(phase: PhaseLiveness, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: "rhino-crash", message: "Scenario executed but the Rhino host stopped answering within the post-execute settle window — likely a deferred host crash triggered by the scenario.")) with { DurationMs = (int)timer.ElapsedMilliseconds };
        }
    }
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
            (Message: "dotnet msbuild ResolveReferences failed.", Arguments: ["msbuild", project, "-target:ResolveReferences", "-getProperty:TargetPath", "-getProperty:TargetDir", "-getProperty:TargetFramework", "-getProperty:AssemblyName", "-getProperty:TargetExt", "-getProperty:RestorePackagesPath", "-getProperty:LangVersion", "-getProperty:IsGrasshopperAwareProject", "-getItem:ReferenceCopyLocalPaths", "-getItem:ReferencePath", $"-p:Configuration={Configuration}", "-p:RestoreLockedMode=true", "-nologo"]),
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
                return (IsLockMismatch(result: target)
                    ? BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: CategoryNugetLockDrift, message: "Stale packages.lock.json: --locked-mode restore rejected the lock as out of sync with the declared package graph (NU1004/NU1403). This is dependency drift, NOT a compile error. Regenerate deliberately with `dotnet restore Workspace.slnx --force-evaluate`, review the lock diff, then commit — the bridge will not auto-regenerate (that would silently mask the drift).")) with { DurationMs = (int)timer.ElapsedMilliseconds, Outputs = outputs }
                    : BridgePhase.Of(phase: PhaseBuild, status: PhaseStatus.Failed, timer: timer, message: message, outputs: outputs), null);
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

    // NuGet emits NU1004 (lockfile inconsistent with the project graph) / NU1403 (content-hash mismatch) when a step
    // using the lock (`restore --locked-mode` or msbuild `-p:RestoreLockedMode=true`) meets a stale packages.lock.json
    // after a Directory.Packages.props change. Classify it as a distinct fault so a drifted lock is never read as a
    // compile error. Read the raw (untruncated) process output; never auto-regenerate — that would mask real drift.
    private static bool IsLockMismatch(ProcessResult result) =>
        (result.Stdout + result.Stderr) is { } output
        && (output.Contains(value: "NU1004", comparisonType: StringComparison.Ordinal)
            || output.Contains(value: "NU1403", comparisonType: StringComparison.Ordinal));
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
    private static async Task<int> CheckRuntimeAsync(string command, BridgePhase resolve, BridgePhase build, ProjectBuild? project, string worktree, CliOptions options, string resultPath, string noRuntimeMessage, Func<ProjectBuild, string, Task<(string Script, IReadOnlyList<string> References)?>> script) {
        (string Script, IReadOnlyList<string> References)? checkScript = project is { } projectBuild && build.Status.IsOk
            ? await script(projectBuild, resultPath).ConfigureAwait(false)
            : null;
        bool canRun = checkScript is not null && build.Status.IsOk;
        IReadOnlyList<string> hostPlugins = project?.IsGrasshopperAware == true ? [BridgeWire.GrasshopperPluginId] : [];
        BridgePhase launch = canRun ? await LaunchPhaseAsync().ConfigureAwait(false) : BridgePhase.Of(phase: PhaseLaunch, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage });
        BridgePhase connect = canRun ? await ConnectPhaseAsync(timeout: TransportTimeout).ConfigureAwait(false) : BridgePhase.Of(phase: PhaseConnect, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage });
        Task<BridgePhase> executeTask = (checkScript, connect.Status.IsOk, build.Status.IsOk) switch {
            ( { } current, true, _) => ExecutePhaseAsync(
                script: current.Script,
                scriptPath: null,
                worktree: worktree,
                references: current.References,
                hostPlugins: hostPlugins,
                stageDirectory: Path.GetDirectoryName(path: resultPath)),
            (null, _, true) => Task.FromResult(BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Unsupported, fault: BridgeFault.MessageOnly(category: PhaseStatus.Unsupported.Wire, message: noRuntimeMessage))),
            _ => Task.FromResult(BridgePhase.Of(phase: PhaseExecute, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage })),
        };
        BridgePhase execute = await executeTask.ConfigureAwait(false);
        BridgePhase liveness = canRun ? await LivenessPhaseAsync(execute: execute, isGrasshopperAware: project?.IsGrasshopperAware == true).ConfigureAwait(false) : BridgePhase.Of(phase: PhaseLiveness, status: PhaseStatus.Skipped, data: new { reason = noRuntimeMessage });
        BridgePhase diagnostics = execute.Diagnostics.Count > 0
            ? BridgePhase.Of(phase: PhaseDiagnostics, status: PhaseStatus.Ok, data: new { count = execute.Diagnostics.Count, sourcePhase = execute.Phase }, diagnostics: execute.Diagnostics)
            : BridgePhase.Of(phase: PhaseDiagnostics, status: PhaseStatus.Skipped, data: new { reason = "No RhinoCode diagnostics were reported." });
        BridgeResult result = BridgeResult.From(command: command, phases: [resolve, build, launch, connect, execute, liveness, diagnostics, BridgePhase.Of(phase: PhaseLifecycle, status: PhaseStatus.Skipped, data: new { reason = "No lifecycle action was requested." })]);
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
            }.Concat(script.Take(count: bodyIndex))
                .Concat([BridgeWire.ScenarioBaseUsings])
                .Concat(project.IsGrasshopperAware ? [BridgeWire.ScenarioHostUsings] : [])
                .Concat([
                    $"const string SCENARIO_NAME = \"{Escape(value: scenario)}\";",
                    $"const string CAPTURE_PATH = \"{Escape(value: capture)}\";",
                    BridgeWire.LanguageExtBootstrap,
                    BridgeWire.ScenarioBodyMarker,
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
        string framework = project.TargetFramework ?? throw new InvalidOperationException(message: "MSBuild did not return TargetFramework; scenario kit artifact resolution requires it.");
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
    private static bool SourceScenarioPreambleLine(string line) => ScenarioLine.Classify(line: line).IsPreamble;
    private static bool SourceScenarioReferenceLine(string line) => ScenarioLine.Classify(line: line) == ScenarioLine.ReferenceDirective;
    private static string ScenarioName(string scriptPath) =>
        Path.GetFileName(path: scriptPath).Replace(oldValue: ".verify.csx", newValue: string.Empty, comparisonType: StringComparison.OrdinalIgnoreCase);
    private static (string Script, IReadOnlyList<string> References) SmokeScript(ProjectBuild project, string resultPath) {
        IReadOnlyList<string> scriptReferences = ScriptReferences(project: project, resultPath: resultPath);
        string references = ReferenceDirectives(references: scriptReferences);
        string targetFile = Path.GetFileName(path: project.TargetPath);
        string target = scriptReferences.FirstOrDefault(predicate: reference => string.Equals(a: Path.GetFileName(path: reference), b: targetFile, comparisonType: StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(message: $"Staged references did not include target assembly '{targetFile}'.");
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
            .OrderBy(reference => BridgeWire.ReferenceLoadOrder(path: reference.Path, targetPath: target))
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
    private static async Task<BridgeReply> SendAsync(BridgeRequest request, TimeSpan timeout) {
        using CancellationTokenSource cancellation = new(delay: timeout);
        BridgeEndpoint endpoint = ReadEndpoint();
        using NamedPipeClientStream pipe = new(serverName: ".", pipeName: endpoint.PipeName, direction: PipeDirection.InOut, options: BridgeWire.PipePolicy);
        await pipe.ConnectAsync(cancellationToken: cancellation.Token).ConfigureAwait(false);
        await BridgeWire.WriteMessageAsync(stream: pipe, message: request, token: cancellation.Token).ConfigureAwait(false);
        return await BridgeWire.ReadMessageAsync<BridgeReply>(stream: pipe, token: cancellation.Token).ConfigureAwait(false)
            ?? throw new InvalidOperationException(message: "Bridge returned no response.");
    }
    private static BridgeEndpoint ReadEndpoint() {
        BridgeEndpoint endpoint = BridgeWire.DeserializeEndpoint(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8))
            ?? throw new InvalidOperationException(message: $"Endpoint metadata is invalid: {BridgeWire.EndpointPath}");
        using Process process = Process.GetProcessById(processId: endpoint.RhinoPid);
        DateTimeOffset startedAt = new(dateTime: process.StartTime.ToUniversalTime());
        bool validPipe = endpoint.PipeName.Length <= 64 && endpoint.PipeName.StartsWith(value: string.Create(CultureInfo.InvariantCulture, $"rb-{endpoint.RhinoPid}-"), comparisonType: StringComparison.Ordinal);
        return (process.HasExited, validPipe, endpoint.IsLiveFor(rhinoPid: endpoint.RhinoPid, rhinoStartedAt: startedAt)) switch {
            (false, true, true) => endpoint,
            _ => throw new InvalidOperationException(message: $"Endpoint metadata is stale or unsupported: {BridgeWire.EndpointPath}"),
        };
    }
    private static TimeSpan TransportTimeout => TimeoutPolicy.Default.Transport;
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
[SmartEnum<int>]
internal sealed partial class PhaseClassification {
    public static readonly PhaseClassification Decisive = new(key: 0);
    public static readonly PhaseClassification Lifecycle = new(key: 1);
    public bool IsDecisive(PhaseStatus status) {
        ArgumentNullException.ThrowIfNull(argument: status);
        return Key switch {
            0 => true,
            _ => status.IsDecisive,
        };
    }
    internal static PhaseClassification Of(string phaseName) =>
        phaseName switch {
            Program.PhaseLifecycle => Lifecycle,
            _ => Decisive,
        };
}

// Classify scenario lines: preamble hoisted; BodyMarker or first Body line starts body (`using var`/`using (` stay Body).
[SmartEnum<int>]
internal sealed partial class ScenarioLine {
    public static readonly ScenarioLine Blank = new(key: 0);
    public static readonly ScenarioLine LineComment = new(key: 1);
    public static readonly ScenarioLine ImportUsing = new(key: 2);
    public static readonly ScenarioLine ReferenceDirective = new(key: 3);
    public static readonly ScenarioLine BodyMarker = new(key: 4);
    public static readonly ScenarioLine Body = new(key: 5);

    internal bool IsPreamble => Key <= ReferenceDirective.Key;

    internal static ScenarioLine Classify(string line) {
        string trimmed = line.TrimStart();
        return trimmed switch {
            "" => Blank,
            _ when trimmed.StartsWith(value: BridgeWire.ScenarioBodyMarker, comparisonType: StringComparison.Ordinal) => BodyMarker,
            _ when trimmed.StartsWith(value: "//", comparisonType: StringComparison.Ordinal) => LineComment,
            _ when trimmed.StartsWith(value: "#r ", comparisonType: StringComparison.Ordinal) || trimmed.StartsWith(value: "#load ", comparisonType: StringComparison.Ordinal) => ReferenceDirective,
            _ when IsImportUsing(trimmed: trimmed) => ImportUsing,
            _ => Body,
        };
    }

    private static bool IsImportUsing(string trimmed) =>
        (trimmed.StartsWith(value: "using static ", comparisonType: StringComparison.Ordinal) || trimmed.StartsWith(value: "using ", comparisonType: StringComparison.Ordinal))
        && !trimmed.StartsWith(value: "using var ", comparisonType: StringComparison.Ordinal)
        && !trimmed.StartsWith(value: "using (", comparisonType: StringComparison.Ordinal);
}

internal readonly record struct PhaseAggregate(PhaseStatus Status, BridgeFault? Fault) {
    internal static readonly PhaseAggregate Identity = new(Status: PhaseStatus.Ok, Fault: null);
    internal PhaseAggregate Combine(BridgePhase phase) =>
        PhaseClassification.Of(phaseName: phase.Phase).IsDecisive(status: phase.Status)
            ? new(Status: Status.Worst(other: phase.Status), Fault: Fault ?? phase.Fault)
            : this;
}

internal sealed record BridgeResult(string Command, PhaseStatus Status, string? ReportPath, IReadOnlyList<BridgePhase> Phases, BridgeFault? Fault) {
    internal static BridgeResult From(string command, IReadOnlyList<BridgePhase> phases) {
        BridgePhase? execute = phases.FirstOrDefault(predicate: static phase => string.Equals(a: phase.Phase, b: Program.PhaseExecute, comparisonType: StringComparison.Ordinal));
        PhaseAggregate aggregate = phases.Aggregate(seed: PhaseAggregate.Identity, func: static (acc, phase) => acc.Combine(phase: phase));
        return new(
            Command: command,
            Status: aggregate.Status,
            ReportPath: null,
            Phases: phases,
            Fault: execute is { Fault: not null } && !execute.Status.IsOk ? execute.Fault : aggregate.Fault);
    }
}
