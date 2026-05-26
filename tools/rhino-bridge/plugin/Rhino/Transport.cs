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
    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(value: 2.0);
    private static readonly TimeSpan IdleDispatchTimeout = TimeSpan.FromMinutes(value: 5.0);
    private const int PipeInstances = 4;
    private const int OutputLimit = 32768;
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
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
            handshake.CancelAfter(delay: HandshakeTimeout);
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
        Stopwatch timer = Stopwatch.StartNew();
        global::Rhino.Runtime.Code.Execution.RunContextStream stdout = new();
        global::Rhino.Runtime.Code.Execution.RunContextStream stderr = new();
        using global::Rhino.Runtime.Code.Execution.RunContext context = new(defaultOutputStream: false, defaultErrorStream: false) {
            CachePolicy = global::Rhino.Runtime.Code.Execution.CachePolicy.NeverCache,
            PreferBasePathResolution = false,
            Options = { ["csharp.resolver.isolate"] = true },
            OutputStream = stdout,
            ErrorStream = stderr,
            ExclusiveStreams = false,
            ResetStreamsPolicy = global::Rhino.Runtime.Code.Execution.ResetStreamPolicy.ResetToPreviousStream,
        };
        (global::Rhino.Runtime.Code.Code? code, Exception? error) = TryRunScript(request: request, context: context);
        timer.Stop();
        string stdoutText = stdout.GetContents();
        string stderrText = stderr.GetContents();
        BridgeReturnValue? returnValue = BridgeMarker.Scan(stdout: stdoutText).OfType<BridgeMarker.Returned>().LastOrDefault() is { } returned
            ? new(Value: returned.Value, Source: BridgeWire.OutputStdout)
            : null;
        BridgeDiagnostic[] diagnostics = Diagnose(code: code, error: error);
        BridgeFault? fault = error is null ? null : BridgeFault.FromException(category: diagnostics.Length > 0 ? "diagnostics" : "execute", error: error);
        PhaseStatus status = error is null ? PhaseStatus.Ok : PhaseStatus.Failed;
        Assembly self = typeof(BridgeServer).Assembly;
        AssemblyName name = self.GetName();
        BridgeReport.Execute report = new(
            Status: status,
            Fault: fault,
            DurationMs: (int)timer.ElapsedMilliseconds,
            ServerExecutionCancelable: false,
            BridgeAssemblyName: name.Name ?? "unknown",
            BridgeAssemblyVersion: name.Version?.ToString() ?? "unknown",
            BridgeAssemblyInformationalVersion: self.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? name.Version?.ToString() ?? "unknown",
            RhinoVersion: RhinoApp.Version.ToString(),
            RhinoCode: new(
                CachePolicy: context.CachePolicy.ToString(),
                ResolverIsolated: context.Options.Get(key: "csharp.resolver.isolate", defaultValue: false),
                PreferBasePathResolution: context.PreferBasePathResolution),
            Document: document is null
                ? new(Active: false, RuntimeSerialNumber: null, Name: null, Path: null, Modified: null, ModelAbsoluteTolerance: null, ModelUnitSystem: null)
                : new(Active: true, RuntimeSerialNumber: document.RuntimeSerialNumber, Name: document.Name, Path: document.Path, Modified: document.Modified, ModelAbsoluteTolerance: document.ModelAbsoluteTolerance, ModelUnitSystem: document.ModelUnitSystem.ToString()),
            ReturnValue: returnValue,
            References: request.References);
        return BridgeWire.Reply(
            command: BridgeWire.Execute,
            status: status,
            data: report,
            outputs: [
                BridgeWire.Capture(source: BridgeWire.OutputStdout, text: stdoutText, limit: OutputLimit),
                BridgeWire.Capture(source: BridgeWire.OutputStderr, text: stderrText, limit: OutputLimit),
            ],
            diagnostics: diagnostics,
            fault: fault);
    }
    private static (global::Rhino.Runtime.Code.Code? Code, Exception? Error) TryRunScript(BridgeExecuteRequest request, global::Rhino.Runtime.Code.Execution.RunContext context) {
        // BOUNDARY ADAPTER — RhinoCode compile/execute throws; collapse to report data.
        try {
            RhinoCodePlatform.Rhino3D.Registrar.StartScriptingLanguages(spec: global::Rhino.Runtime.Code.Languages.LanguageSpec.CSharp, startServer: false);
            global::Rhino.Runtime.Code.Code? code = request.ScriptPath is string scriptPath && File.Exists(path: scriptPath)
                ? global::Rhino.Runtime.Code.RhinoCode.RunScript(uri: new Uri(uriString: scriptPath), context: context)
                : global::Rhino.Runtime.Code.RhinoCode.RunScript(text: request.Script, context: context);
            return (Code: code, Error: null);
        } catch (Exception error) when (error is global::Rhino.Runtime.Code.Execution.CompileException or global::Rhino.Runtime.Code.Execution.ExecuteException || NonFatal(error: error)) {
            return (Code: null, Error: error);
        }
    }
    private static BridgeDiagnostic[] Diagnose(global::Rhino.Runtime.Code.Code? code, Exception? error) =>
        CompileFailure(error: error) switch {
            global::Rhino.Runtime.Code.Execution.CompileException compile => [.. compile.Diagnosis.Select(ToDiagnostic)],
            _ => code?.Diagnostics.Select(ToDiagnostic).ToArray() ?? [],
        };
    private static global::Rhino.Runtime.Code.Execution.CompileException? CompileFailure(Exception? error) =>
        error switch {
            null => null,
            global::Rhino.Runtime.Code.Execution.CompileException compile => compile,
            global::Rhino.Runtime.Code.Execution.ExecuteException execute when execute.TryGetCompileException(out global::Rhino.Runtime.Code.Execution.CompileException compile) => compile,
            { InnerException: Exception inner } => CompileFailure(error: inner),
            _ => null,
        };
    private static BridgeDiagnostic ToDiagnostic(global::Rhino.Runtime.Code.Diagnostics.Diagnostic diagnostic) =>
        new(
            Severity: diagnostic.Severity.ToString(),
            Message: diagnostic.Message,
            Source: diagnostic.Reference.Uri?.ToString(),
            Code: diagnostic.HasId ? diagnostic.Id : null,
            File: diagnostic.Reference.Uri?.LocalPath,
            Line: diagnostic.Reference.Position.LineNumber,
            Column: diagnostic.Reference.Position.ColumnNumber,
            Category: "rhinocode");
    internal static BridgeReport.Quit Quit(RhinoDoc? document) =>
        RhinoDoc.OpenDocuments().Any(static open => open.Modified) switch {
            true => new(Status: PhaseStatus.Failed, Fault: BridgeFault.MessageOnly(category: "quit", message: "At least one Rhino document has unsaved changes; refusing automated quit."), RhinoPid: Environment.ProcessId, ActiveDocument: document is not null, Modified: true),
            false => new(Status: PhaseStatus.Ok, Fault: null, RhinoPid: Environment.ProcessId, ActiveDocument: document is not null, Modified: false),
        };
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
        timeout.CancelAfter(delay: IdleDispatchTimeout);
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
        new(pipeName, PipeDirection.InOut, maxNumberOfServerInstances: PipeInstances, transmissionMode: PipeTransmissionMode.Byte, options: PipePolicy);
    private void DeleteEndpoint() {
        // BOUNDARY ADAPTER — cleanup is best-effort; do not propagate IO/JSON noise on shutdown.
        try {
            BridgeEndpoint? current = ReadEndpointMetadata();
            if (current is { } active && active.RhinoPid == endpoint.RhinoPid && string.Equals(a: active.PipeName, b: endpoint.PipeName, comparisonType: StringComparison.Ordinal)) {
                File.Delete(path: BridgeWire.EndpointPath);
            }
        } catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException) {
            OnMain(work: _ => RhinoApp.WriteLine(message: $"[RasmBridge] endpoint cleanup failed: {error.Message}"));
        }
    }
}
