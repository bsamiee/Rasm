using System.Collections.Concurrent;

namespace Rasm.RhinoBridge.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
internal abstract record BridgeCommand {
    internal sealed record Hello : BridgeCommand {
        internal override BridgeReply Dispatch(BridgeServer server, RhinoDoc? document) => server.Hello();
    }
    internal sealed record Doctor : BridgeCommand {
        internal override BridgeReply Dispatch(BridgeServer server, RhinoDoc? document) =>
            BridgeWire.Reply(command: BridgeWire.Doctor, status: PhaseStatus.Ok, data: BridgeServer.Doctor(document: document));
    }
    internal sealed record Execute(BridgeExecuteRequest Payload) : BridgeCommand {
        internal override BridgeReply Dispatch(BridgeServer server, RhinoDoc? document) =>
            BridgeServer.Execute(request: Payload, document: document);
    }
    internal sealed record Quit : BridgeCommand {
        internal override BridgeReply Dispatch(BridgeServer server, RhinoDoc? document) =>
            BridgeServer.Quit(document: document) is BridgeReport.Quit report
                ? BridgeWire.Reply(command: BridgeWire.Quit, status: report.Status, data: report, fault: report.Fault)
                : throw new UnreachableException();
    }
    internal abstract BridgeReply Dispatch(BridgeServer server, RhinoDoc? document);
    internal static BridgeCommandParse Parse(BridgeRequest? request) =>
        request switch {
            null => BridgeCommandParse.Fail(command: BridgeWire.Hello, message: "Request payload was empty or invalid."),
            { Command: null or "" } => BridgeCommandParse.Fail(command: BridgeWire.Hello, message: "Request command was missing."),
            { } current when !BridgeWire.IsCurrent(schema: current.Schema) => BridgeCommandParse.Fail(command: current.Command, message: $"Unsupported schema '{current.Schema}'."),
            { Command: BridgeWire.Hello } => BridgeCommandParse.Ok(command: new Hello()),
            { Command: BridgeWire.Doctor } => BridgeCommandParse.Ok(command: new Doctor()),
            { Command: BridgeWire.Execute } => ParseExecute(request: request),
            { Command: BridgeWire.Quit } => BridgeCommandParse.Ok(command: new Quit()),
            { Command: string command } => BridgeCommandParse.Fail(command: command, message: $"Unsupported command '{command}'."),
        };
    private static BridgeCommandParse ParseExecute(BridgeRequest request) {
        // BOUNDARY ADAPTER — JsonSerializer.Deserialize throws on malformed JSON.
        try {
            return request.Payload switch {
                JsonElement json when json.Deserialize<BridgeExecuteRequest>(options: BridgeWire.CompactJson) is { Script.Length: > 0 } payload => BridgeCommandParse.Ok(command: new Execute(Payload: payload)),
                JsonElement => BridgeCommandParse.Fail(command: request.Command, message: $"Bridge request '{request.Command}' had an invalid payload."),
                _ => BridgeCommandParse.Fail(command: request.Command, message: $"Bridge request '{request.Command}' requires a payload."),
            };
        } catch (JsonException error) {
            return BridgeCommandParse.Fail(command: request.Command, message: $"Bridge request '{request.Command}' payload was not valid JSON: {error.Message}");
        }
    }
}

internal sealed record BridgeCommandParse(BridgeCommand? Command, BridgeReply? Failure) {
    internal static BridgeCommandParse Ok(BridgeCommand command) => new(Command: command, Failure: null);
    internal static BridgeCommandParse Fail(string command, string message) =>
        new(Command: null, Failure: BridgeWire.Reply(command: command, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: message)));
}

// --- [SERVICES] -------------------------------------------------------------------------
internal sealed class BridgeServer : IDisposable {
    private const int PipeInstances = 4;
    private readonly CancellationTokenSource cancellation = new();
    private readonly SemaphoreSlim clientGate = new(initialCount: 1, maxCount: 1);
    private readonly ConcurrentQueue<IdleJob> idleQueue = new();
    private readonly EventHandler idleHandler;
    private readonly BridgeEndpoint endpoint;
    private readonly Task[] acceptLoops;
    private Exception? lastError;
    private bool disposed;
    private BridgeServer(BridgeEndpoint endpoint) {
        this.endpoint = endpoint;
        idleHandler = (_, _) => DrainIdleQueue();
        RhinoApp.Idle += idleHandler;
        acceptLoops = [.. Enumerable.Range(start: 0, count: PipeInstances).Select(_ => AcceptLoopAsync(token: cancellation.Token))];
    }
    private sealed record IdleJob(Func<RhinoDoc?, BridgeReply> Work, TaskCompletionSource<BridgeReply> Completion);
    internal bool IsRunning => !disposed && acceptLoops.Any(static loop => !loop.IsCompleted);
    internal static BridgeServer Start() {
        _ = Directory.CreateDirectory(path: BridgeWire.EndpointDirectory);
        BoundaryIO.RestrictDirectory(path: BridgeWire.EndpointDirectory);
        using Process process = Process.GetCurrentProcess();
        Assembly bridgeAssembly = typeof(BridgeServer).Assembly;
        AssemblyName bridgeName = bridgeAssembly.GetName();
        string pipeSuffix = Guid.NewGuid().ToString(format: "N")[..8];
        BridgeEndpoint metadata = new(
            Schema: BridgeWire.Schema,
            PipeName: string.Create(provider: CultureInfo.InvariantCulture, $"rb-{Environment.ProcessId}-{pipeSuffix}"),
            RhinoPid: Environment.ProcessId,
            RhinoStartedAt: new DateTimeOffset(dateTime: process.StartTime.ToUniversalTime()),
            StartedAt: DateTimeOffset.UtcNow,
            BridgeAssemblyName: bridgeName.Name ?? "unknown",
            BridgeAssemblyVersion: bridgeName.Version?.ToString() ?? "unknown",
            BridgeAssemblyInformationalVersion: bridgeAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? bridgeName.Version?.ToString() ?? "unknown",
            RhinoVersion: RhinoApp.Version.ToString());
        BridgeServer server = new(endpoint: metadata);
        try {
            WriteEndpoint(endpoint: metadata);
            return server;
        } catch {
            server.Dispose();
            throw;
        }
    }
    internal BridgeEndpoint State() {
        if (!IsRunning) {
            throw new InvalidOperationException(message: lastError?.Message ?? "Bridge stopped.");
        }
        EnsureEndpoint();
        return endpoint;
    }
    public void Dispose() {
        bool alreadyDisposed = disposed;
        disposed = true;
        if (!alreadyDisposed) {
            RhinoApp.Idle -= idleHandler;
            cancellation.Cancel();
            DrainPending();
            clientGate.Dispose();
            cancellation.Dispose();
            DeleteEndpoint();
        }
    }
    private void DrainPending() {
        while (idleQueue.TryDequeue(result: out IdleJob? entry)) {
            _ = entry.Completion.TrySetCanceled();
        }
    }
    private void DrainIdleQueue() {
        if (disposed || !idleQueue.TryDequeue(result: out IdleJob? entry)) {
            return;
        }
        // BOUNDARY ADAPTER — work delegate executes scenario / native code; isolate non-fatal failures.
        try {
            BridgeReply reply = entry.Work(RhinoDoc.ActiveDoc);
            _ = entry.Completion.TrySetResult(reply);
        } catch (Exception error) when (NonFatal(error: error)) {
            _ = entry.Completion.TrySetException(error);
        }
    }
    private async Task AcceptLoopAsync(CancellationToken token) {
        // BOUNDARY ADAPTER — NamedPipeServerStream IO surface; cancellation + transient IO must not kill the loop.
        while (!token.IsCancellationRequested) {
            try {
                NamedPipeServerStream pipe = CreatePipe(pipeName: endpoint.PipeName);
                await using (pipe.ConfigureAwait(false)) {
                    await pipe.WaitForConnectionAsync(cancellationToken: token).ConfigureAwait(false);
                    await HandlePipeAsync(pipe: pipe, token: token).ConfigureAwait(false);
                }
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
            } catch (Exception error) when (!token.IsCancellationRequested && error is IOException or InvalidOperationException or UnauthorizedAccessException or ObjectDisposedException or ArgumentOutOfRangeException) {
                lastError = error;
                OnMain(work: _ => RhinoApp.WriteLine(message: $"[RasmBridge] accept failed: {error.GetType().Name}: {error.Message}"));
                await Task.Delay(millisecondsDelay: 100, cancellationToken: token).ConfigureAwait(false);
            }
        }
    }
    private void EnsureEndpoint() {
        BridgeEndpoint? current = ReadEndpointMetadata();
        if (current is null || current != endpoint) {
            WriteEndpoint(endpoint: endpoint);
        }
    }
    private static BridgeEndpoint? ReadEndpointMetadata() {
        // BOUNDARY ADAPTER — endpoint file may be malformed mid-write.
        try {
            return File.Exists(path: BridgeWire.EndpointPath)
                ? JsonSerializer.Deserialize<BridgeEndpoint>(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8), options: BridgeWire.CompactJson)
                : null;
        } catch (JsonException) {
            return null;
        }
    }
    private async Task HandlePipeAsync(Stream pipe, CancellationToken token) {
        bool acquired = await clientGate.WaitAsync(timeout: TimeSpan.Zero, cancellationToken: token).ConfigureAwait(false);
        if (!acquired) {
            await WriteAsync(pipe: pipe, reply: BridgeWire.Reply(command: BridgeWire.Hello, status: PhaseStatus.Busy, fault: BridgeFault.MessageOnly(category: "transport", message: "Another bridge client is active.")), token: token).ConfigureAwait(false);
            return;
        }
        try {
            using StreamReader reader = new(stream: pipe, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            using CancellationTokenSource handshake = CancellationTokenSource.CreateLinkedTokenSource(token);
            handshake.CancelAfter(delay: BridgeTimeouts.Handshake);
            string? line = await reader.ReadLineAsync(cancellationToken: handshake.Token).ConfigureAwait(false);
            BridgeReply reply = string.IsNullOrWhiteSpace(value: line)
                ? BridgeWire.Reply(command: BridgeWire.Hello, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge clients must send a hello or command request."))
                : await ParseRequestAsync(json: line, token: token).ConfigureAwait(false);
            await WriteAsync(pipe: pipe, reply: reply, token: token).ConfigureAwait(false);
            if (reply is { Command: BridgeWire.Quit, Status.IsOk: true }) {
                OnMain(work: static _ => RhinoApp.Exit(false));
            }
        } catch (OperationCanceledException) when (!token.IsCancellationRequested) {
            await WriteAsync(pipe: pipe, reply: BridgeWire.Reply(command: BridgeWire.Hello, status: PhaseStatus.Timeout, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge client did not send a request before the handshake deadline.")), token: CancellationToken.None).ConfigureAwait(false);
        } finally {
            _ = !disposed && clientGate.Release() > 0;
        }
    }
    private async Task<BridgeReply> ParseRequestAsync(string json, CancellationToken token) {
        // BOUNDARY ADAPTER — request envelope may be malformed.
        try {
            return await HandleRequestAsync(request: JsonSerializer.Deserialize<BridgeRequest>(json: json, options: BridgeWire.CompactJson), token: token).ConfigureAwait(false);
        } catch (JsonException error) {
            return BridgeWire.Reply(command: BridgeWire.Hello, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: "protocol", error: error));
        }
    }
    private Task<BridgeReply> HandleRequestAsync(BridgeRequest? request, CancellationToken token) =>
        BridgeCommand.Parse(request: request) switch {
            { Command: BridgeCommand command } => ExecuteOnRhinoAsync(command: request!.Command, work: document => command.Dispatch(server: this, document: document), token: token),
            { Failure: BridgeReply failure } => Task.FromResult(result: failure),
            _ => Task.FromResult(result: BridgeWire.Reply(command: request?.Command ?? BridgeWire.Hello, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge request could not be parsed."))),
        };
    internal BridgeReply Hello() {
        EnsureEndpoint();
        return BridgeWire.Reply(command: BridgeWire.Hello, status: PhaseStatus.Ok, data: endpoint);
    }
    internal static BridgeReport.Doctor Doctor(RhinoDoc? document) =>
        new(
            Status: PhaseStatus.Ok,
            Fault: null,
            RhinoName: RhinoApp.Name,
            RhinoVersion: RhinoApp.Version.ToString(),
            HostRuntime: Environment.Version.ToString(),
            ScriptEngine: CodeEngine.Readiness(),
            RhinoPid: Environment.ProcessId,
            ActiveDocument: document is not null,
            ModelAbsoluteTolerance: document?.ModelAbsoluteTolerance,
            Assemblies: [.. AppDomain.CurrentDomain.GetAssemblies()
                .Where(static assembly => IsHostAssembly(assembly: assembly))
                .Concat([typeof(BridgeServer).Assembly, typeof(BridgeWire).Assembly, typeof(RhinoApp).Assembly])
                .Where(static assembly => !string.IsNullOrWhiteSpace(assembly.GetName().Name))
                .GroupBy(static assembly => assembly.GetName().Name!, StringComparer.Ordinal)
                .Select(static group => AssemblyReport(assembly: group.First()))
                .OrderBy(static report => report.Name, StringComparer.Ordinal)]);
    internal static BridgeReply Execute(BridgeExecuteRequest request, RhinoDoc? document) {
        IReadOnlyList<BridgeDiagnostic> pluginDiagnostics = EnsureHostPlugins(pluginIds: request.HostPlugins);
        bool priorCapture = RhinoApp.CommandWindowCaptureEnabled;
        string historyBefore = RhinoApp.CommandHistoryWindowText ?? string.Empty;
        RhinoApp.CommandWindowCaptureEnabled = true;
        ConcurrentBag<BridgeFault> reportSink = [];
        void OnExceptionReport(string source, Exception error) =>
            reportSink.Add(item: BridgeFault.FromException(category: string.IsNullOrWhiteSpace(value: source) ? "exception-report" : source, error: error));
        global::Rhino.Runtime.HostUtils.ExceptionReportDelegate onReport = OnExceptionReport;
        CodeOutcome outcome;
        string[] commandStrings;
        // BOUNDARY ADAPTER — capture the command window AND every HostUtils.OnExceptionReport around execution (the path
        // RhinoApp.InvokeOnUiThread routes swallowed faults through); subscribe before, unsubscribe + restore in finally.
        global::Rhino.Runtime.HostUtils.OnExceptionReport += onReport;
        try {
            outcome = CodeEngine.Run(request: request);
            commandStrings = CommandWindowCapture(historyBefore: historyBefore);
        } finally {
            global::Rhino.Runtime.HostUtils.OnExceptionReport -= onReport;
            RhinoApp.CommandWindowCaptureEnabled = priorCapture;
        }
        BridgeFault[] exceptionReports = [.. reportSink];
        BridgeRuntimeDiagnostics diagnostics = new(
            CommandWindow: commandStrings,
            ExceptionReports: exceptionReports,
            Gh: ReflectGrasshopperSolution(pluginIds: request.HostPlugins));
        // Fault-surfacing fold: a swallowed exception report is a real failure even when the script itself did not throw.
        PhaseStatus status = outcome.Status.IsOk && exceptionReports.Length > 0 ? PhaseStatus.Failed : outcome.Status;
        BridgeFault? fault = outcome.Fault ?? (exceptionReports.Length > 0 ? exceptionReports[0] : null);
        Assembly self = typeof(BridgeServer).Assembly;
        AssemblyName name = self.GetName();
        BridgeReport.Execute report = new(
            Status: status,
            Fault: fault,
            DurationMs: outcome.DurationMs,
            ServerExecutionCancelable: false,
            BridgeAssemblyName: name.Name ?? "unknown",
            BridgeAssemblyVersion: name.Version?.ToString() ?? "unknown",
            BridgeAssemblyInformationalVersion: self.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? name.Version?.ToString() ?? "unknown",
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoCode: outcome.RhinoCode,
            Document: document is null
                ? new(Active: false, RuntimeSerialNumber: null, Name: null, Path: null, Modified: null, ModelAbsoluteTolerance: null, ModelUnitSystem: null)
                : new(Active: true, RuntimeSerialNumber: document.RuntimeSerialNumber, Name: document.Name, Path: document.Path, Modified: document.Modified, ModelAbsoluteTolerance: document.ModelAbsoluteTolerance, ModelUnitSystem: document.ModelUnitSystem.ToString()),
            ReturnValue: outcome.ReturnValue,
            References: request.References,
            Diagnostics: diagnostics);
        return BridgeWire.Reply(
            command: BridgeWire.Execute,
            status: status,
            data: report,
            outputs: [
                BridgeWire.Capture(source: BridgeWire.OutputStdout, text: outcome.Stdout, limit: BridgeWire.OutputLimit),
                BridgeWire.Capture(source: BridgeWire.OutputStderr, text: outcome.Stderr, limit: BridgeWire.OutputLimit),
                BridgeWire.Capture(source: BridgeWire.OutputRhinoCommand, text: string.Concat(values: commandStrings), limit: BridgeWire.OutputLimit),
            ],
            diagnostics: [.. pluginDiagnostics, .. outcome.Diagnostics],
            fault: fault);
    }
    private static IReadOnlyList<BridgeDiagnostic> EnsureHostPlugins(IReadOnlyList<string>? pluginIds) =>
        [.. (pluginIds ?? []).Select(LoadHostPlugin).OfType<BridgeDiagnostic>()];
    private static BridgeDiagnostic? LoadHostPlugin(string pluginId) {
        // BOUNDARY ADAPTER — pre-load a host plugin (e.g. Grasshopper2) into the default ALC so the isolated RhinoCode
        // resolver binds it by name instead of illegally re-loading it into the collectible context (COR_E_INVALIDOPERATION).
        try {
            return Guid.TryParse(input: pluginId, result: out Guid id) switch {
                false => new BridgeDiagnostic(Severity: "Warning", Message: $"Ignored invalid host plugin id '{pluginId}'.", Category: "hostplugin"),
                true when PlugIn.GetPlugInInfo(pluginId: id)?.IsLoaded == true => null,
                true when PlugIn.LoadPlugIn(pluginId: id, loadQuietly: true, forceLoad: false) && PlugIn.GetPlugInInfo(pluginId: id)?.IsLoaded == true => null,
                _ => new BridgeDiagnostic(Severity: "Error", Message: $"Host plugin {id} failed to load before execution.", Category: "hostplugin"),
            };
        } catch (Exception error) when (NonFatal(error: error)) {
            return new BridgeDiagnostic(Severity: "Error", Message: $"Host plugin {pluginId} load threw: {error.Message}", Category: "hostplugin");
        }
    }
    private static string[] CommandWindowCapture(string historyBefore) {
        string[] captured = RhinoApp.CapturedCommandWindowStrings(clearBuffer: true);
        return captured.Length > 0
            ? captured
            : (RhinoApp.CommandHistoryWindowText ?? string.Empty) switch {
                string after when after.Length > historyBefore.Length && after.StartsWith(value: historyBefore, comparisonType: StringComparison.Ordinal) => [after[historyBefore.Length..]],
                _ => [],
            };
    }

    private static BridgeGhSolution? ReflectGrasshopperSolution(IReadOnlyList<string>? pluginIds) {
        if (pluginIds is null || !pluginIds.Any(predicate: static id => string.Equals(a: id, b: BridgeWire.GrasshopperPluginId, comparisonType: StringComparison.Ordinal))) {
            return null;
        }
        // BOUNDARY ADAPTER — read GH2 active-document/solution state by reflection so the bridge keeps zero compile-time
        // Grasshopper2 reference; any shape drift collapses to a null/partial snapshot rather than a throw.
        try {
            Type? editorType = Type.GetType(typeName: "Grasshopper2.UI.Editor, Grasshopper2");
            object? editor = editorType?.GetProperty(name: "Instance", bindingAttr: BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null);
            object? canvas = editor is null ? null : editorType!.GetProperty(name: "Canvas", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: editor);
            object? doc = canvas?.GetType().GetProperty(name: "Document", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: canvas);
            if (doc is null) {
                return new BridgeGhSolution(DocumentInPlay: false, ObjectCount: 0, WarningCount: null, ErrorCount: null, State: "none");
            }
            object? objects = doc.GetType().GetProperty(name: "Objects", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: doc);
            int objectCount = objects switch {
                System.Collections.ICollection collection => collection.Count,
                System.Collections.IEnumerable enumerable => enumerable.Cast<object>().Count(),
                _ => 0,
            };
            object? solution = doc.GetType().GetProperty(name: "Solution", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: doc);
            object? serverState = solution?.GetType().GetProperty(name: "State", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: solution);
            // ServerState is a class (no meaningful ToString); its `Phase` field is the SolutionPhase enum whose name is
            // the truthful state (Idle/Running/...). Fall back to the type name only if the field shape ever drifts.
            object? phase = serverState?.GetType().GetField(name: "Phase", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: serverState);
            return new BridgeGhSolution(
                DocumentInPlay: true,
                ObjectCount: objectCount,
                WarningCount: null,
                ErrorCount: null,
                State: phase?.ToString() ?? serverState?.GetType().Name ?? "none");
        } catch (Exception error) when (NonFatal(error: error)) {
            return null;
        }
    }

    internal static BridgeReport.Quit Quit(RhinoDoc? document) {
        // BOUNDARY ADAPTER — force-quit: this is a disposable test/diagnostic Rhino, so discard unsaved document state
        // (mark clean) to bypass macOS save prompts before RhinoApp.Exit(false) closes the host.
        RhinoDoc[] open = RhinoDoc.OpenDocuments();
        bool hadModified = open.Any(static doc => doc.Modified);
        Array.ForEach(array: open, action: static doc => doc.Modified = false);
        return new(Status: PhaseStatus.Ok, Fault: null, RhinoPid: Environment.ProcessId, ActiveDocument: document is not null, Modified: hadModified);
    }
    private void OnMain(Action<RhinoDoc?> work) {
        if (disposed) {
            return;
        }
        if (RhinoApp.IsOnMainThread) {
            SafeInvoke(work: work, document: RhinoDoc.ActiveDoc);
            return;
        }
        TaskCompletionSource<BridgeReply> sink = new(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
        idleQueue.Enqueue(item: new IdleJob(Work: doc => {
            SafeInvoke(work: work, document: doc);
            return BridgeWire.Reply(command: "internal", status: PhaseStatus.Ok);
        }, Completion: sink));
    }
    private static void SafeInvoke(Action<RhinoDoc?> work, RhinoDoc? document) {
        // BOUNDARY ADAPTER — fire-and-forget UI work; swallow non-fatal failures with a Rhino-line trace.
        try {
            work(document);
        } catch (Exception error) when (NonFatal(error: error)) {
            RhinoApp.WriteLine(message: $"[RasmBridge] OnMain failed: {error.Message}");
        }
    }
    private async Task<BridgeReply> ExecuteOnRhinoAsync(string command, Func<RhinoDoc?, BridgeReply> work, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(work);
        if (RhinoApp.IsClosing || RhinoApp.IsExiting) {
            return BridgeWire.Reply(command: command, status: PhaseStatus.Failed, fault: BridgeFault.MessageOnly(category: "rhino", message: "Rhino is closing; bridge command rejected."));
        }
        if (RhinoApp.IsOnMainThread) {
            return SafeDispatch(work: work, command: command, document: RhinoDoc.ActiveDoc);
        }
        TaskCompletionSource<BridgeReply> completion = new(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
        idleQueue.Enqueue(item: new IdleJob(Work: doc => SafeDispatch(work: work, command: command, document: doc), Completion: completion));
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(delay: BridgeTimeouts.IdleDispatch);
        try {
            return await completion.Task.WaitAsync(cancellationToken: timeout.Token).ConfigureAwait(false);
        } catch (OperationCanceledException) {
            return BridgeWire.Reply(command: command, status: PhaseStatus.Timeout, fault: BridgeFault.MessageOnly(category: "rhino", message: "Bridge command timed out waiting for Rhino UI thread idle dispatch."));
        }
    }
    private static BridgeReply SafeDispatch(Func<RhinoDoc?, BridgeReply> work, string command, RhinoDoc? document) {
        // BOUNDARY ADAPTER — request-handling work; isolate non-fatal failures into a Failed reply.
        try {
            return work(document);
        } catch (Exception error) when (NonFatal(error: error)) {
            return BridgeWire.Reply(command: command, status: PhaseStatus.Failed, fault: BridgeFault.FromException(category: "runtime", error: error));
        }
    }
    private static bool NonFatal(Exception error) =>
        error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException;
    private static BridgeAssemblyReport AssemblyReport(Assembly assembly) =>
        new(
            Name: assembly.GetName().Name ?? "unknown",
            Status: PhaseStatus.Ok,
            Required: true,
            Version: assembly.GetName().Version?.ToString(),
            InformationalVersion: assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            Location: assembly.Location,
            Fault: null);
    private static bool IsHostAssembly(Assembly assembly) =>
        IsUnder(path: assembly.Location, root: HostRoot);
    private static string HostRoot =>
        Path.GetDirectoryName(path: typeof(RhinoApp).Assembly.Location) ?? string.Empty;
    private static bool IsUnder(string path, string root) =>
        !string.IsNullOrWhiteSpace(value: path)
        && !string.IsNullOrWhiteSpace(value: root)
        && Path.GetRelativePath(relativeTo: Path.GetFullPath(path: root), path: Path.GetFullPath(path: path)) is string relative
        && !relative.StartsWith(value: "..", comparisonType: StringComparison.Ordinal)
        && !Path.IsPathRooted(path: relative);
    private static async Task WriteAsync(Stream pipe, BridgeReply reply, CancellationToken token) {
        StreamWriter writer = new(stream: pipe, encoding: Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            string payload = BridgeWire.Serialize(reply: reply);
            await writer.WriteLineAsync(buffer: payload.AsMemory(), cancellationToken: token).ConfigureAwait(false);
            await writer.FlushAsync(cancellationToken: token).ConfigureAwait(false);
        }
    }
    private static void WriteEndpoint(BridgeEndpoint endpoint) {
        BoundaryIO.RestrictDirectory(path: BridgeWire.EndpointDirectory);
        BoundaryIO.Write(path: BridgeWire.EndpointPath, contents: BridgeWire.Serialize(endpoint: endpoint), encoding: Encoding.UTF8, restrict: BoundaryIO.RestrictFile);
    }
    private static NamedPipeServerStream CreatePipe(string pipeName) =>
        new(pipeName, PipeDirection.InOut, maxNumberOfServerInstances: PipeInstances, transmissionMode: PipeTransmissionMode.Byte, options: BridgeWire.PipePolicy);
    private void DeleteEndpoint() {
        // BOUNDARY ADAPTER — cleanup is best-effort; do not propagate IO/JSON noise on shutdown.
        try {
            BridgeEndpoint? current = ReadEndpointMetadata();
            if (current is { } active && active.IsLiveFor(rhinoPid: endpoint.RhinoPid, rhinoStartedAt: endpoint.RhinoStartedAt)) {
                File.Delete(path: BridgeWire.EndpointPath);
            }
        } catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException) {
            OnMain(work: _ => RhinoApp.WriteLine(message: $"[RasmBridge] endpoint cleanup failed: {error.Message}"));
        }
    }
}
