using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rasm.RhinoBridge.Rhino;

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
        } catch (IOException error) {
            return BridgeRuntimeStatus.Stopped(BridgeFault.Of("transport", error));
        } catch (UnauthorizedAccessException error) {
            return BridgeRuntimeStatus.Stopped(BridgeFault.Of("transport", error));
        } catch (InvalidOperationException error) {
            return BridgeRuntimeStatus.Stopped(BridgeFault.Of("transport", error));
        }
    }
}

internal sealed class BridgeServer : IDisposable {
    private readonly CancellationTokenSource cancellation = new();
    private readonly SemaphoreSlim clientGate = new(1, 1);
    private readonly BridgeEndpoint endpoint;
    private readonly Task acceptLoop;
    private bool disposed;
    private BridgeServer(BridgeEndpoint endpoint) {
        this.endpoint = endpoint;
        acceptLoop = AcceptLoopAsync(token: cancellation.Token);
    }
    internal bool IsRunning => !disposed && !acceptLoop.IsCompleted;
    internal static BridgeServer Start() {
        _ = Directory.CreateDirectory(BridgeProtocol.EndpointDirectory);
        string tokenHash = Convert.ToHexString(SHA256.HashData(RandomNumberGenerator.GetBytes(32)));
        BridgeEndpoint metadata = new(
            Schema: BridgeProtocol.Schema,
            PipeName: string.Create(CultureInfo.InvariantCulture, $"rasm-rhino-bridge-{Environment.ProcessId}"),
            RhinoPid: Environment.ProcessId,
            TokenHash: tokenHash,
            StartedAt: DateTimeOffset.UtcNow,
            BridgeAssemblyVersion: typeof(BridgeServer).Assembly.GetName().Version?.ToString() ?? "unknown",
            RhinoVersion: RhinoApp.Version.ToString());
        File.WriteAllText(BridgeProtocol.EndpointPath, JsonSerializer.Serialize(metadata, BridgeProtocol.Json), Encoding.UTF8);
        return new BridgeServer(endpoint: metadata);
    }
    internal BridgeRuntimeStatus Status() => BridgeRuntimeStatus.Started(endpoint: endpoint);
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing) {
        if (disposed) {
            return;
        }
        if (disposing) {
            cancellation.Cancel();
            clientGate.Dispose();
            cancellation.Dispose();
            File.Delete(BridgeProtocol.EndpointPath);
        }
        disposed = true;
    }
    private async Task AcceptLoopAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            using NamedPipeServerStream pipe = new(endpoint.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try {
                await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
                await HandlePipeAsync(pipe: pipe, token: token).ConfigureAwait(false);
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
            } catch (IOException error) when (!token.IsCancellationRequested) {
                RhinoApp.WriteLine($"[RasmBridge] transport error: {error.Message}");
            } catch (JsonException error) when (!token.IsCancellationRequested) {
                RhinoApp.WriteLine($"[RasmBridge] protocol error: {error.Message}");
            } catch (InvalidOperationException error) when (!token.IsCancellationRequested) {
                RhinoApp.WriteLine($"[RasmBridge] runtime error: {error.Message}");
            }
        }
    }
    private async Task HandlePipeAsync(Stream pipe, CancellationToken token) {
        bool acquired = await clientGate.WaitAsync(TimeSpan.Zero, token).ConfigureAwait(false);
        if (!acquired) {
            await WriteAsync(pipe: pipe, reply: BridgeReply.Rejected(BridgeProtocol.Hello, BridgeProtocol.Busy, BridgeFault.MessageOnly("transport", "Another bridge client is active.")), token: token).ConfigureAwait(false);
            return;
        }
        try {
            using StreamReader reader = new(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            string? line = await reader.ReadLineAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line)) {
                return;
            }
            BridgeRequest? request = JsonSerializer.Deserialize<BridgeRequest>(line, BridgeProtocol.Json);
            BridgeReply reply = HandleRequest(request);
            await WriteAsync(pipe: pipe, reply: reply, token: token).ConfigureAwait(false);
        } finally {
            _ = clientGate.Release();
        }
    }
    private BridgeReply HandleRequest(BridgeRequest? request) =>
        request switch {
            null => BridgeReply.Rejected(BridgeProtocol.Hello, BridgeProtocol.Failed, BridgeFault.MessageOnly("protocol", "Request payload was empty or invalid.")),
            { Schema: string schema } when !string.Equals(schema, BridgeProtocol.Schema, StringComparison.Ordinal) =>
                BridgeReply.Rejected(request.Command, BridgeProtocol.Failed, BridgeFault.MessageOnly("protocol", $"Unsupported schema '{request.Schema}'.")),
            { TokenHash: string hash } when !string.Equals(hash, endpoint.TokenHash, StringComparison.Ordinal) =>
                BridgeReply.Rejected(request.Command, BridgeProtocol.Unauthorized, BridgeFault.MessageOnly("auth", "Bridge token hash did not match endpoint metadata.")),
            _ => InvokeOnRhinoThread(work: () => request.Command switch {
                BridgeProtocol.Hello => BridgeReply.HelloOk(endpoint: endpoint),
                BridgeProtocol.Doctor => BridgeChecks.Doctor(),
                BridgeProtocol.Check => BridgeChecks.Check(request: request),
                string command => BridgeReply.Rejected(command, BridgeProtocol.Unsupported, BridgeFault.MessageOnly("protocol", $"Unsupported command '{command}'.")),
            }),
        };
    private static BridgeReply InvokeOnRhinoThread(Func<BridgeReply> work) {
        ArgumentNullException.ThrowIfNull(work);
        if (!RhinoApp.InvokeRequired) {
            return work();
        }
        BridgeReply? reply = null;
        RhinoApp.InvokeAndWait(() => reply = work());
        return reply ?? BridgeReply.Rejected(BridgeProtocol.Hello, BridgeProtocol.Failed, BridgeFault.MessageOnly("rhino", "Rhino UI thread returned no bridge reply."));
    }
    private static async Task WriteAsync(Stream pipe, BridgeReply reply, CancellationToken token) {
        StreamWriter writer = new(pipe, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
        await using (writer.ConfigureAwait(false)) {
            string payload = JsonSerializer.Serialize(reply, BridgeProtocol.Json);
            await writer.WriteLineAsync(payload.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }
    }
}
