using System.Text.Json;
using System.Text.Json.Serialization;
using StreamJsonRpc;

namespace Rasm.Bridge.Contract;

// --- [SERVICES] ------------------------------------------------------------------------

[JsonRpcContract]
public partial interface IBridgeShell {
    public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct);
    public Task<LoadedCargo> LoadCargoAsync(CargoManifest manifest, CancellationToken ct);
    public Task<ScenarioOutcome[]> RunAsync(ScenarioSelection selection, CancellationToken ct);
    public Task<UnloadOutcome> UnloadCargoAsync(CancellationToken ct);
    public Task<long> PingAsync(CancellationToken ct);
    public Task<QuitScrub> PrepareQuitAsync(CancellationToken ct);
}

[JsonRpcContract]
public partial interface IBridgeEvents {
    public Task PublishAsync(BridgeEvent evt);
}

public interface IBridgeCargo : IDisposable {
    public ScenarioEntry[] Discover();
    public CapabilityEntry[] Probe(Action<BridgeEvent> publish);
    public ScenarioOutcome Run(ScenarioEntry scenario, Action<BridgeEvent> publish);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(BridgeEvent))]
[JsonSerializable(typeof(BridgeFault))]
[JsonSerializable(typeof(Handshake))]
[JsonSerializable(typeof(CargoManifest))]
[JsonSerializable(typeof(LoadedCargo))]
[JsonSerializable(typeof(ScenarioSelection))]
[JsonSerializable(typeof(ScenarioOutcome[]))]
[JsonSerializable(typeof(UnloadOutcome))]
[JsonSerializable(typeof(QuitScrub))]
[JsonSerializable(typeof(SessionEnvelope))]
[JsonSerializable(typeof(EvidenceClass))]
[JsonSerializable(typeof(EvidenceRole))]
[JsonSerializable(typeof(ArtifactRetentionClass))]
[JsonSerializable(typeof(EvidenceName))]
[JsonSerializable(typeof(ArtifactHash))]
[JsonSerializable(typeof(ArtifactRef))]
[JsonSerializable(typeof(CaptureArtifact))]
[JsonSerializable(typeof(ObjectManifest))]
[JsonSerializable(typeof(GeometryManifest))]
[JsonSerializable(typeof(ViewportManifest))]
[JsonSerializable(typeof(Gh2CanvasManifest))]
[JsonSerializable(typeof(ScratchManifest))]
[JsonSerializable(typeof(EvidenceCounts))]
[JsonSerializable(typeof(ScenarioCounts))]
[JsonSerializable(typeof(StatusBreakdown))]
[JsonSerializable(typeof(PhaseOutcome))]
[JsonSerializable(typeof(FaultSummary))]
[JsonSerializable(typeof(SpoolSummary))]
[JsonSerializable(typeof(EvidenceCertificate))]
[JsonSerializable(typeof(ArtifactRef[]))]
[JsonSerializable(typeof(ObjectManifest[]))]
[JsonSerializable(typeof(GeometryManifest[]))]
[JsonSerializable(typeof(ViewportManifest[]))]
[JsonSerializable(typeof(Gh2CanvasManifest[]))]
[JsonSerializable(typeof(ScratchManifest[]))]
[JsonSerializable(typeof(PhaseOutcome[]))]
[JsonSerializable(typeof(EvidenceClass[]))]
[JsonSerializable(typeof(JsonElement[]))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
public sealed partial class BridgeJsonContext : JsonSerializerContext;
