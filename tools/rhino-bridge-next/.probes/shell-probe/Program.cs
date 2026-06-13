using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Nodes;
using Rasm.Bridge.Contract;
using StreamJsonRpc;

// U4 live acceptance driver. argv: [0] = staged cargo dir (defaults to the cargo-stub bin).
// Emits ONE JSON document to stdout; progress lines to stderr.

if (args.Length > 0 && args[0] == "--selftest") {
    return await Rasm.Bridge.ShellProbe.SelfTest.RunAsync();
}

string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string endpointPath = Path.Combine(home, ".rasm", "rhino-bridge-rbx.json");
string stagePath = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "cargo-stub", "bin", "Release", "net10.0"));

JsonObject result = new() { ["probe"] = "u4-shell-acceptance", ["stagePath"] = stagePath };
ConcurrentQueue<string> received = new();

string endpointJson = File.ReadAllText(endpointPath);
EndpointRecord endpoint = JsonSerializer.Deserialize(endpointJson, BridgeJsonContext.Default.EndpointRecord)
    ?? throw new InvalidOperationException("endpoint record unreadable");
result["endpoint"] = JsonNode.Parse(endpointJson);
Console.Error.WriteLine($"[probe] endpoint pipe={endpoint.PipeName} pid={endpoint.RhinoPid}");

(JsonRpc rpcA, IBridgeShell shellA) = await ConnectAsync(endpoint.PipeName, received);

// 1. hello (carries client.pid as a hello fact row).
Handshake helloA = await shellA.HelloAsync(Supervisor(), CancellationToken.None);
result["hello"] = JsonSerializer.SerializeToNode(helloA, BridgeJsonContext.Default.Handshake);
Console.Error.WriteLine($"[probe] hello ok: shell={helloA.SenderVersion} contract={helloA.ContractVersion}");

// 2. ping.
result["pingTickMs"] = await shellA.PingAsync(CancellationToken.None);

// 3. load cargo (GH2 preload row + absent-plugin row = proof 10 evidence).
CargoManifest manifest = new(
    SessionId: Guid.NewGuid(),
    ReportDir: Path.Combine(Path.GetTempPath(), "rbx-u4"),
    ContentHash: "stub-hash-1",
    StagePath: stagePath,
    HostPlugins: [Guid.Parse("8307876d-a461-4daa-bb77-eb3715925513"), Guid.Parse("00000000-dead-beef-0000-000000000001")],
    BuiltAgainst: helloA.Fingerprint ?? default);
CargoReceipt loadA = await shellA.LoadCargoAsync(manifest, CancellationToken.None);
result["load"] = JsonSerializer.SerializeToNode(loadA, BridgeJsonContext.Default.CargoReceipt);
Console.Error.WriteLine($"[probe] load ok: swapMs={loadA.SwapMs:F0} scenarios={loadA.Scenarios.Length}");

// 4. busy admission: second connection, hello fine, load -> BusyHeld.
(JsonRpc rpcB, IBridgeShell shellB) = await ConnectAsync(endpoint.PipeName, received);
Handshake helloB = await shellB.HelloAsync(Supervisor(), CancellationToken.None);
result["helloSecondConnection"] = helloB.SenderVersion;
try {
    _ = await shellB.LoadCargoAsync(manifest, CancellationToken.None);
    result["busy"] = "UNEXPECTED-SUCCESS";
} catch (RemoteInvocationException busy) {
    result["busy"] = new JsonObject {
        ["errorCode"] = busy.ErrorCode,
        ["message"] = busy.Message,
        ["errorDataClrType"] = busy.DeserializedErrorData?.GetType().FullName,
        ["errorData"] = busy.DeserializedErrorData switch {
            BridgeFault fault => JsonSerializer.SerializeToNode(fault, BridgeJsonContext.Default.BridgeFault),
            JsonElement element => JsonNode.Parse(element.GetRawText()),
            var other => (JsonNode?)other?.ToString(),
        },
    };
    Console.Error.WriteLine($"[probe] busy admission ok: code={busy.ErrorCode} message={busy.Message}");
}

// 5. run all.
ScenarioReceipt[] receipts = await shellA.RunAsync(new ScenarioSelection.AllCase(), CancellationToken.None);
result["run"] = JsonSerializer.SerializeToNode(receipts, BridgeJsonContext.Default.ScenarioReceiptArray);
Console.Error.WriteLine($"[probe] run ok: receipts={receipts.Length} status={receipts[0].Status}");

// 6. reload with the SAME hash -> swap short-circuit (cargo.reused fact rides the event stream).
CargoReceipt reuse = await shellA.LoadCargoAsync(manifest, CancellationToken.None);
result["reload"] = JsonSerializer.SerializeToNode(reuse, BridgeJsonContext.Default.CargoReceipt);

// 7. unload (probe-0a posture: Confirmed=false is evidence, not failure) -> releases the session.
UnloadReceipt unload = await shellA.UnloadCargoAsync(CancellationToken.None);
result["unload"] = JsonSerializer.SerializeToNode(unload, BridgeJsonContext.Default.UnloadReceipt);
Console.Error.WriteLine($"[probe] unload: confirmed={unload.Confirmed} gcRetries={unload.GcRetries}");

// 8. after release the second connection may take the session.
CargoReceipt loadB = await shellB.LoadCargoAsync(manifest, CancellationToken.None);
result["loadAfterRelease"] = JsonSerializer.SerializeToNode(loadB, BridgeJsonContext.Default.CargoReceipt);
UnloadReceipt unloadB = await shellB.UnloadCargoAsync(CancellationToken.None);
result["unloadSecond"] = JsonSerializer.SerializeToNode(unloadB, BridgeJsonContext.Default.UnloadReceipt);

// 9. fresh hello after the session: proof-13 row now reflects live default-ALC resolution traffic.
Handshake helloAfter = await shellA.HelloAsync(Supervisor(), CancellationToken.None);
result["helloAfterSession"] = JsonSerializer.SerializeToNode(helloAfter, BridgeJsonContext.Default.Handshake);

await Task.Delay(500);
result["eventsReceived"] = new JsonArray([.. received.Select(static line => (JsonNode?)JsonNode.Parse(line))]);

rpcA.Dispose();
rpcB.Dispose();

Console.WriteLine(result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
return 0;

static Handshake Supervisor() => new(
    ContractVersion: Handshake.CurrentVersion,
    SenderVersion: "u4-probe",
    Capabilities: [new CapabilityEntry(Key: "client.pid", Outcome: PhaseStatus.Ok, Receipt: Environment.ProcessId.ToString())],
    Fingerprint: null,
    Endpoint: null);

static async Task<(JsonRpc Rpc, IBridgeShell Shell)> ConnectAsync(string pipeName, ConcurrentQueue<string> received) {
    NamedPipeClientStream stream = new(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
    await stream.ConnectAsync(10_000);
    SystemTextJsonFormatter formatter = new();
    formatter.JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
        TypeInfoResolver = System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.Combine(
            BridgeJsonContext.Default, new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()),
    };
    HeaderDelimitedMessageHandler handler = new(stream, stream, formatter);
    JsonRpc rpc = new Rasm.Bridge.ShellProbe.BridgeRpc(handler);
    rpc.AddLocalRpcTarget<IBridgeEvents>(new EventSink(received), null);
    IBridgeShell shell = rpc.Attach<IBridgeShell>();
    rpc.StartListening();
    return (rpc, shell);
}

internal sealed class EventSink(ConcurrentQueue<string> received) : IBridgeEvents {
    public Task PublishAsync(BridgeEvent evt) {
        received.Enqueue(JsonSerializer.Serialize(evt, BridgeJsonContext.Default.BridgeEvent));
        return Task.CompletedTask;
    }
}

namespace Rasm.Bridge.ShellProbe {
    // Supervisor-shaped error read: shell faults ride error.data; mapping the data type to
    // BridgeFault makes DeserializedErrorData a typed union case ($type preserved).
    internal sealed class BridgeRpc(StreamJsonRpc.IJsonRpcMessageHandler handler) : JsonRpc(handler) {
        protected override Type? GetErrorDetailsDataType(StreamJsonRpc.Protocol.JsonRpcError? error) =>
            error?.Error?.Code == (StreamJsonRpc.Protocol.JsonRpcErrorCode)(-32050) ? typeof(BridgeFault) : base.GetErrorDetailsDataType(error);
    }
}
