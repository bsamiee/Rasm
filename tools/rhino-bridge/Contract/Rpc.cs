using System.Text.Json.Serialization;
using StreamJsonRpc;

namespace Rasm.Bridge.Contract;

// --- [SERVICES] ---------------------------------------------------------------------------

// Ownership: supervisor->shell JSON-RPC verbs. Method names are the protocol boundary; cancellation
// is transport hygiene only, and older shells degrade missing methods through CapabilityAbsent.
[JsonRpcContract]
public partial interface IBridgeShell {
    public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct);
    public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct);
    public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct);
    public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct);
    public Task<long> PingAsync(CancellationToken ct);
    public Task<QuitPrepareReceipt> PrepareQuitAsync(CancellationToken ct);
}

// Ownership: shell->supervisor evidence; the event union is the only channel discriminator.
[JsonRpcContract]
public partial interface IBridgeEvents {
    public Task PublishAsync(BridgeEvent evt);
}

// Ownership: the in-process shell<->cargo seam. Contract-only payloads keep shell state out of
// cargo, synchronous calls ride one idle-pump frame, and Dispose is the unload precondition.
public interface IBridgeCargo : IDisposable {
    public ScenarioEntry[] Discover();
    public CapabilityEntry[] Probe(Action<BridgeEvent> publish);
    public ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish);
}

// --- [COMPOSITION] ------------------------------------------------------------------------

// Ownership: Contract-edge codec policy. Unmapped members are skipped deliberately; fields grow
// additively, supervisor-bound unions may add cases, shell-bound payloads grow by fields unless a
// handshake capability gates a new shape, and smart-enum row semantics are immutable.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(BridgeEvent))]
[JsonSerializable(typeof(BridgeFault))]
[JsonSerializable(typeof(Handshake))]
[JsonSerializable(typeof(CargoManifest))]
[JsonSerializable(typeof(CargoReceipt))]
[JsonSerializable(typeof(ScenarioSelection))]
[JsonSerializable(typeof(ScenarioReceipt[]))]
[JsonSerializable(typeof(UnloadReceipt))]
[JsonSerializable(typeof(QuitPrepareReceipt))]
[JsonSerializable(typeof(SessionEnvelope))]
public sealed partial class BridgeJsonContext : JsonSerializerContext;
