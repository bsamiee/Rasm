namespace Rasm.RhinoBridge.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
internal sealed record BridgeRuntimeStatus(bool Running, BridgeEndpoint? Endpoint, BridgeFault? Fault) {
    internal static BridgeRuntimeStatus Stopped(BridgeFault? fault = null) => new(Running: false, Endpoint: null, Fault: fault);
    internal static BridgeRuntimeStatus Started(BridgeEndpoint endpoint) => new(Running: true, Endpoint: endpoint, Fault: null);
}

// --- [SERVICES] -------------------------------------------------------------------------
internal static class BridgeRuntime {
    private static readonly Lock Sync = new();
    private static BridgeServer? server;
    internal static BridgeRuntimeStatus Start() {
        lock (Sync) {
            return server switch {
                { IsRunning: true } active => active.Status(),
                _ => StartFresh(),
            };
        }
    }
    internal static BridgeRuntimeStatus Status() {
        lock (Sync) {
            return server?.Status() ?? BridgeRuntimeStatus.Stopped();
        }
    }
    internal static void Stop() {
        lock (Sync) {
            server?.Dispose();
            server = null;
        }
    }
    private static BridgeRuntimeStatus StartFresh() {
        try {
            server?.Dispose();
            BridgeServer fresh = BridgeServer.Start();
            server = fresh;
            return fresh.Status();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) {
            return BridgeRuntimeStatus.Stopped(BridgeFault.FromException(category: "transport", error: error));
        }
    }
}

internal sealed class BridgeServer : IDisposable {
    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(2.0);
    private const int PipeInstances = 4;
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private readonly CancellationTokenSource cancellation = new();
    private readonly SemaphoreSlim clientGate = new(initialCount: 1, maxCount: 1);
    private readonly BridgeEndpoint endpoint;
    private readonly BridgeSessions sessions = new();
    private readonly Task[] acceptLoops;
    private BridgeFault? fault;
    private bool disposed;
    private BridgeServer(BridgeEndpoint endpoint) {
        this.endpoint = endpoint;
        acceptLoops = [.. Enumerable.Range(start: 0, count: PipeInstances).Select(_ => AcceptLoopAsync(token: cancellation.Token))];
    }
    internal bool IsRunning => !disposed && acceptLoops.Any(static loop => !loop.IsCompleted);
    internal static BridgeServer Start() {
        _ = Directory.CreateDirectory(BridgeWire.EndpointDirectory);
        RestrictDirectory(path: BridgeWire.EndpointDirectory);
        using Process process = Process.GetCurrentProcess();
        string pipeSuffix = Guid.NewGuid().ToString(format: "N")[..8];
        BridgeEndpoint metadata = new(
            Schema: BridgeWire.Schema,
            PipeName: string.Create(CultureInfo.InvariantCulture, $"rb-{Environment.ProcessId}-{pipeSuffix}"),
            RhinoPid: Environment.ProcessId,
            RhinoStartedAt: new DateTimeOffset(dateTime: process.StartTime.ToUniversalTime()),
            StartedAt: DateTimeOffset.UtcNow,
            BridgeAssemblyVersion: typeof(BridgeServer).Assembly.GetName().Version?.ToString() ?? "unknown",
            RhinoVersion: RhinoApp.Version.ToString());
        BridgeServer server = new(endpoint: metadata);
        WriteEndpoint(endpoint: metadata);
        return server;
    }
    internal BridgeRuntimeStatus Status() => IsRunning ? BridgeRuntimeStatus.Started(endpoint: endpoint) : BridgeRuntimeStatus.Stopped(fault: fault);
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing) {
        bool alreadyDisposed = disposed;
        disposed = true;
        if (disposing && !alreadyDisposed) {
            DisposeManaged();
        }
    }
    private void DisposeManaged() {
        cancellation.Cancel();
        sessions.Dispose();
        clientGate.Dispose();
        cancellation.Dispose();
        DeleteEndpoint();
    }
    private async Task AcceptLoopAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                NamedPipeServerStream pipe = CreatePipe(pipeName: endpoint.PipeName);
                await using (pipe.ConfigureAwait(false)) {
                    await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
                    await HandlePipeAsync(pipe: pipe, token: token).ConfigureAwait(false);
                }
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
            } catch (Exception error) when (!token.IsCancellationRequested && error is IOException or InvalidOperationException or UnauthorizedAccessException or ObjectDisposedException or ArgumentOutOfRangeException) {
                fault = BridgeFault.FromException(category: "transport", error: error);
                WriteRhinoLine(message: $"[RasmBridge] accept failed: {error.GetType().Name}: {error.Message}");
                await Task.Delay(millisecondsDelay: 100, cancellationToken: token).ConfigureAwait(false);
            }
        }
    }
    private async Task HandlePipeAsync(Stream pipe, CancellationToken token) {
        bool acquired = await clientGate.WaitAsync(timeout: TimeSpan.Zero, cancellationToken: token).ConfigureAwait(false);
        if (!acquired) {
            await WriteAsync(pipe: pipe, reply: BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Busy, fault: BridgeFault.MessageOnly(category: "transport", message: "Another bridge client is active.")), token: token).ConfigureAwait(false);
            return;
        }
        try {
            using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            using CancellationTokenSource handshake = CancellationTokenSource.CreateLinkedTokenSource(token);
            handshake.CancelAfter(delay: HandshakeTimeout);
            string? line = await reader.ReadLineAsync(handshake.Token).ConfigureAwait(false);
            BridgeReply reply = string.IsNullOrWhiteSpace(line)
                ? BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge clients must send a hello or command request."))
                : ParseRequest(json: line);
            await WriteAsync(pipe: pipe, reply: reply, token: token).ConfigureAwait(false);
        } catch (OperationCanceledException) when (!token.IsCancellationRequested) {
            await WriteAsync(pipe: pipe, reply: BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Timeout, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge client did not send a request before the handshake deadline.")), token: CancellationToken.None).ConfigureAwait(false);
        } finally {
            ReleaseGate();
        }
    }
    private BridgeReply ParseRequest(string json) {
        try {
            return HandleRequest(JsonSerializer.Deserialize<BridgeRequest>(json: json, options: BridgeWire.CompactJson));
        } catch (JsonException error) {
            return BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Failed, fault: BridgeFault.FromException(category: "protocol", error: error));
        }
    }
    private BridgeReply HandleRequest(BridgeRequest? request) =>
        request switch {
            null => BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: "Request payload was empty or invalid.")),
            { } current when !BridgeWire.IsCurrent(schema: current.Schema) => BridgeReply.Rejected(command: request.Command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Unsupported schema '{request.Schema}'.")),
            _ => InvokeOnRhinoThread(command: request.Command, work: document => request.Command switch {
                BridgeWire.Hello => BridgeReply.HelloOk(endpoint: endpoint),
                BridgeWire.Doctor => BridgeReply.DoctorOk(doctor: sessions.Doctor(document: document)),
                BridgeWire.Load => WithPayload<BridgeLoadRequest>(request: request, work: payload => BridgeReply.LoadOk(load: sessions.Load(request: payload))),
                BridgeWire.Run => WithPayload<BridgeRunRequest>(request: request, work: payload => BridgeReply.RunOk(run: sessions.Run(request: payload, document: document, timeoutMs: request.TimeoutMs))),
                BridgeWire.Unload => WithPayload<BridgeUnloadRequest>(request: request, work: payload => BridgeReply.UnloadOk(unload: sessions.Unload(request: payload))),
                string command => BridgeReply.Rejected(command: command, status: BridgeWire.Unsupported, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Unsupported command '{command}'.")),
            }),
        };
    private static BridgeReply WithPayload<TPayload>(BridgeRequest request, Func<TPayload, BridgeReply> work) {
        ArgumentNullException.ThrowIfNull(work);
        try {
            return request.Payload switch {
                JsonElement json when json.Deserialize<TPayload>(BridgeWire.CompactJson) is TPayload payload && ValidPayload(payload: payload) => work(payload),
                JsonElement => BridgeReply.Rejected(command: request.Command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Bridge request '{request.Command}' had an invalid payload.")),
                _ => BridgeReply.Rejected(command: request.Command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Bridge request '{request.Command}' requires a payload.")),
            };
        } catch (JsonException error) {
            return BridgeReply.Rejected(command: request.Command, status: BridgeWire.Failed, fault: BridgeFault.FromException(category: "protocol", error: error));
        }
    }
    private static bool ValidPayload<TPayload>(TPayload payload) =>
        payload switch {
            BridgeLoadRequest { AssemblyPath.Length: > 0, WorkspaceRoot.Length: > 0 } => true,
            BridgeRunRequest { SessionId.Length: > 0 } => true,
            BridgeUnloadRequest { SessionId.Length: > 0 } => true,
            _ => false,
        };
    private static BridgeReply InvokeOnRhinoThread(string command, Func<RhinoDoc?, BridgeReply> work) {
        ArgumentNullException.ThrowIfNull(work);
        if (RhinoApp.IsClosing || RhinoApp.IsExiting) {
            return BridgeReply.Rejected(command: command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "rhino", message: "Rhino is closing; bridge command rejected."));
        }
        if (!RhinoApp.InvokeRequired) {
            return Safe(work: work, command: command);
        }
        BridgeReply? reply = null;
        RhinoApp.InvokeAndWait(() => reply = Safe(work: work, command: command));
        return reply ?? BridgeReply.Rejected(command: command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "rhino", message: "Rhino UI thread returned no bridge reply."));
    }
    private static BridgeReply Safe(Func<RhinoDoc?, BridgeReply> work, string command) {
        try {
            return work(RhinoDoc.ActiveDoc);
        } catch (Exception error) when (error is InvalidOperationException or IOException or JsonException or ArgumentException or ReflectionTypeLoadException) {
            return BridgeReply.Rejected(command: command, status: BridgeWire.Failed, fault: BridgeFault.FromException(category: "runtime", error: error));
        }
    }
    private static async Task WriteAsync(Stream pipe, BridgeReply reply, CancellationToken token) {
        StreamWriter writer = new(pipe, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            string payload = JsonSerializer.Serialize(value: reply, options: BridgeWire.CompactJson);
            await writer.WriteLineAsync(payload.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }
    }
    private static void WriteEndpoint(BridgeEndpoint endpoint) {
        string temp = string.Create(CultureInfo.InvariantCulture, $"{BridgeWire.EndpointPath}.{Environment.ProcessId}.tmp");
        File.WriteAllText(path: temp, contents: JsonSerializer.Serialize(value: endpoint, options: BridgeWire.CompactJson), encoding: Encoding.UTF8);
        Restrict(path: temp);
        File.Move(sourceFileName: temp, destFileName: BridgeWire.EndpointPath, overwrite: true);
        Restrict(path: BridgeWire.EndpointPath);
    }
    private static NamedPipeServerStream CreatePipe(string pipeName) =>
        new(pipeName, PipeDirection.InOut, maxNumberOfServerInstances: PipeInstances, PipeTransmissionMode.Byte, PipePolicy);
    private static void Observe(Task task) =>
        _ = task.ContinueWith(static completed => _ = completed.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    private void ReleaseGate() {
        try {
            _ = clientGate.Release();
        } catch (ObjectDisposedException) {
        } catch (SemaphoreFullException) {
        }
    }
    private static void Restrict(string path) {
        if (!OperatingSystem.IsWindows()) {
            RestrictUnix(path: path);
        }
    }
    private static void RestrictDirectory(string path) {
        if (!OperatingSystem.IsWindows()) {
            RestrictUnixDirectory(path: path);
        }
    }
    private void DeleteEndpoint() {
        try {
            BridgeEndpoint? current = File.Exists(BridgeWire.EndpointPath)
                ? JsonSerializer.Deserialize<BridgeEndpoint>(json: File.ReadAllText(path: BridgeWire.EndpointPath, encoding: Encoding.UTF8), options: BridgeWire.CompactJson)
                : null;
            if (current is { } active && active.RhinoPid == endpoint.RhinoPid && string.Equals(active.PipeName, endpoint.PipeName, StringComparison.Ordinal)) {
                File.Delete(BridgeWire.EndpointPath);
            }
        } catch (Exception error) when (error is IOException or JsonException or UnauthorizedAccessException) {
            WriteRhinoLine(message: $"[RasmBridge] endpoint cleanup failed: {error.Message}");
        }
    }
    private static void WriteRhinoLine(string message) {
        if (RhinoApp.InvokeRequired) {
            RhinoApp.InvokeOnUiThread((Action)(() => RhinoApp.WriteLine(message)));
        } else {
            RhinoApp.WriteLine(message);
        }
    }
    [UnsupportedOSPlatform("windows")]
    private static void RestrictUnix(string path) =>
        File.SetUnixFileMode(path: path, mode: UnixFileMode.UserRead | UnixFileMode.UserWrite);
    [UnsupportedOSPlatform("windows")]
    private static void RestrictUnixDirectory(string path) =>
        File.SetUnixFileMode(path: path, mode: UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
}
