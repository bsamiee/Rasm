namespace Rasm.RhinoBridge.Rhino;

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
    private const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    private readonly CancellationTokenSource cancellation = new();
    private readonly SemaphoreSlim clientGate = new(initialCount: 1, maxCount: 1);
    private readonly BridgeEndpoint endpoint;
    private readonly BridgeSessions sessions = new();
    private readonly Task acceptLoop;
    private BridgeFault? fault;
    private bool disposed;
    private BridgeServer(BridgeEndpoint endpoint, NamedPipeServerStream initialPipe) {
        this.endpoint = endpoint;
        acceptLoop = AcceptLoopAsync(initialPipe: initialPipe, token: cancellation.Token);
    }
    internal bool IsRunning => !disposed && !acceptLoop.IsCompleted;
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
        NamedPipeServerStream? initialPipe = new(metadata.PipeName, PipeDirection.InOut, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte, PipePolicy);
        try {
            BridgeServer server = new(endpoint: metadata, initialPipe: initialPipe);
            initialPipe = null;
            WriteEndpoint(endpoint: metadata);
            return server;
        } finally {
            initialPipe?.Dispose();
        }
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
    private async Task AcceptLoopAsync(NamedPipeServerStream initialPipe, CancellationToken token) {
        NamedPipeServerStream? nextPipe = initialPipe;
        while (!token.IsCancellationRequested) {
            if (nextPipe is NamedPipeServerStream firstPipe) {
                nextPipe = null;
                await using (firstPipe.ConfigureAwait(false)) {
                    await AcceptOneAsync(pipe: firstPipe, token: token).ConfigureAwait(false);
                }
            } else {
                NamedPipeServerStream pipe = new(endpoint.PipeName, PipeDirection.InOut, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte, PipePolicy);
                await using (pipe.ConfigureAwait(false)) {
                    await AcceptOneAsync(pipe: pipe, token: token).ConfigureAwait(false);
                }
            }
        }
    }
    private async Task AcceptOneAsync(NamedPipeServerStream pipe, CancellationToken token) {
        try {
            await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
            await HandlePipeAsync(pipe: pipe, token: token).ConfigureAwait(false);
        } catch (OperationCanceledException) when (token.IsCancellationRequested) {
        } catch (Exception error) when (!token.IsCancellationRequested && error is IOException or JsonException or InvalidOperationException or ArgumentOutOfRangeException) {
            fault = BridgeFault.FromException(category: "transport", error: error);
            WriteRhinoLine(message: $"[RasmBridge] {error.GetType().Name}: {error.Message}");
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
                : HandleRequest(JsonSerializer.Deserialize<BridgeRequest>(json: line, options: BridgeWire.CompactJson));
            await WriteAsync(pipe: pipe, reply: reply, token: token).ConfigureAwait(false);
        } catch (OperationCanceledException) when (!token.IsCancellationRequested) {
            await WriteAsync(pipe: pipe, reply: BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Timeout, fault: BridgeFault.MessageOnly(category: "protocol", message: "Bridge client did not send a request before the handshake deadline.")), token: CancellationToken.None).ConfigureAwait(false);
        } finally {
            try {
                _ = clientGate.Release();
            } catch (ObjectDisposedException) {
            }
        }
    }
    private BridgeReply HandleRequest(BridgeRequest? request) =>
        request switch {
            null => BridgeReply.Rejected(command: BridgeWire.Hello, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: "Request payload was empty or invalid.")),
            { Schema: string schema } when !string.Equals(schema, BridgeWire.Schema, StringComparison.Ordinal) =>
                BridgeReply.Rejected(command: request.Command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Unsupported schema '{request.Schema}'.")),
            _ => InvokeOnRhinoThread(command: request.Command, work: () => request.Command switch {
                BridgeWire.Hello => BridgeReply.HelloOk(endpoint: endpoint),
                BridgeWire.Doctor => BridgeReply.DoctorOk(doctor: sessions.Doctor()),
                BridgeWire.Load => BridgeReply.LoadOk(load: sessions.Load(ReadPayload<BridgeLoadRequest>(request: request))),
                BridgeWire.Run => BridgeReply.RunOk(run: sessions.Run(request: ReadPayload<BridgeRunRequest>(request: request), timeoutMs: request.TimeoutMs)),
                BridgeWire.Unload => BridgeReply.UnloadOk(unload: sessions.Unload(ReadPayload<BridgeUnloadRequest>(request: request))),
                string command => BridgeReply.Rejected(command: command, status: BridgeWire.Unsupported, fault: BridgeFault.MessageOnly(category: "protocol", message: $"Unsupported command '{command}'.")),
            }),
        };
    private static TPayload ReadPayload<TPayload>(BridgeRequest request) =>
        request.Payload switch {
            JsonElement json => json.Deserialize<TPayload>(BridgeWire.CompactJson) ?? throw new InvalidOperationException($"Bridge request '{request.Command}' had an invalid payload."),
            _ => throw new InvalidOperationException($"Bridge request '{request.Command}' requires a payload."),
        };
    private static BridgeReply InvokeOnRhinoThread(string command, Func<BridgeReply> work) {
        ArgumentNullException.ThrowIfNull(work);
        if (!RhinoApp.InvokeRequired) {
            return Safe(work: work, command: command);
        }
        BridgeReply? reply = null;
        RhinoApp.InvokeAndWait(() => reply = Safe(work: work, command: command));
        return reply ?? BridgeReply.Rejected(command: command, status: BridgeWire.Failed, fault: BridgeFault.MessageOnly(category: "rhino", message: "Rhino UI thread returned no bridge reply."));
    }
    private static BridgeReply Safe(Func<BridgeReply> work, string command) {
        try {
            return work();
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
