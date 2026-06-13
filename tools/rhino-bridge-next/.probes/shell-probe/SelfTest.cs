using System.Text.Json;
using Nerdbank.Streams;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

namespace Rasm.Bridge.ShellProbe;

// Offline regression for the busy-path error shape: LocalRpcException with BridgeFault ErrorData
// over an in-proc duplex pair. Pinned findings: (1) inserting BridgeJsonContext into a fresh
// formatter chain DROPS the implicit reflection fallback, so CommonErrorData on any error
// response carrying data becomes undecodable and the peer fatals the connection — the
// DefaultJsonTypeInfoResolver tail is mandatory; (2) GetErrorDetailsDataType mapped to
// BridgeFault gives the client a typed union case with $type preserved.
internal static class SelfTest {
    internal static async Task<int> RunAsync() {
        (System.IO.Stream clientStream, System.IO.Stream serverStream) = FullDuplexStream.CreatePair();
        using SystemTextJsonFormatter serverFormatter = new();
        serverFormatter.JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
            TypeInfoResolver = System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.Combine(
                BridgeJsonContext.Default, new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()),
        };
        using HeaderDelimitedMessageHandler serverHandler = new(serverStream, serverStream, serverFormatter);
        using JsonRpc server = new(serverHandler);
        server.AddLocalRpcTarget<IBridgeShell>(new ThrowingShell(), null);
        server.StartListening();

        using SystemTextJsonFormatter clientFormatter = new();
        clientFormatter.JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
            TypeInfoResolver = System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.Combine(
                BridgeJsonContext.Default, new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()),
        };
        using HeaderDelimitedMessageHandler clientHandler = new(clientStream, clientStream, clientFormatter);
        using JsonRpc client = new BridgeRpc(clientHandler);
        IBridgeShell proxy = client.Attach<IBridgeShell>();
        client.StartListening();

        try {
            _ = await proxy.LoadCargoAsync(
                new CargoManifest(Guid.NewGuid(), "/tmp", "h", "/tmp", [], default),
                CancellationToken.None);
            Console.WriteLine("UNEXPECTED-SUCCESS");
            return 1;
        } catch (RemoteInvocationException remote) {
            Console.WriteLine($"REMOTE: code={remote.ErrorCode} message={remote.Message}");
            Console.WriteLine($"DATA-CLR: {remote.DeserializedErrorData?.GetType().FullName ?? "<null>"}");
            Console.WriteLine($"DATA: {(remote.DeserializedErrorData is BridgeFault fault ? JsonSerializer.Serialize(fault, BridgeJsonContext.Default.BridgeFault) : remote.DeserializedErrorData?.ToString() ?? "<null>")}");
            return remote.DeserializedErrorData is BridgeFault.BusyHeld ? 0 : 3;
        } catch (Exception other) {
            Console.WriteLine($"OTHER: {other.GetType().Name}: {other.Message}");
            return 2;
        }
    }

    private sealed class ThrowingShell : IBridgeShell {
        public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct) => throw new NotSupportedException();

        public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct) {
            BridgeFault fault = new BridgeFault.BusyHeld(HolderPid: 777, AgeSeconds: 1.5);
            throw new LocalRpcException(fault.Prescription) {
                ErrorCode = -32050,
                ErrorData = JsonSerializer.SerializeToElement(fault, BridgeJsonContext.Default.BridgeFault),
            };
        }

        public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct) => throw new NotSupportedException();
        public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<long> PingAsync(CancellationToken ct) => Task.FromResult(0L);
        public Task PrepareQuitAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
