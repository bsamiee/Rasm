using System.Text.Json.Serialization;
using StreamJsonRpc;

namespace Rasm.Bridge.Contract;

// --- [SERVICES] ---------------------------------------------------------------------------

// Ownership: the supervisor->shell verb surface. Six methods, no arity/mode siblings, no boolean
// knobs; JSON-RPC name dispatch is the protocol's law and the one boundary where names are
// allowed. Per-method CancellationToken = transport hygiene only, never UI-thread abort. New
// operations are new methods: an older shell answers them with JSON-RPC -32601, which the
// supervisor's rail bridge folds to the SAME CapabilityAbsent outcome as a failed host probe —
// one degradation lattice.
[JsonRpcContract]
public partial interface IBridgeShell {
    public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct);
    public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct);   // load + discover + probe, one transaction
    public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct);
    public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct);
    public Task<long> PingAsync(CancellationToken ct);                                        // answered on the pipe thread: discriminates dead vs alive-but-wedged
    public Task PrepareQuitAsync(CancellationToken ct);                                       // docs-clean before the AE rung
}

// Ownership: the shell->supervisor evidence stream. ONE notification method; the event union is
// the discriminator (the five-channel collapse made literal).
[JsonRpcContract]
public partial interface IBridgeEvents {
    public Task PublishAsync(BridgeEvent evt);
}

// Ownership: the in-process shell<->cargo seam (not RPC). Contract-typed and primitive-only, so no
// cargo type can reach shell-held state. Synchronous: every call rides one idle-pump frame.
// publish is a SHELL-owned delegate handed into cargo (collectible->non-collectible: the legal
// reference direction). Dispose is the unload precondition: scenario scopes + host-event
// detachers drained.
public interface IBridgeCargo : IDisposable {
    public ScenarioEntry[] Discover();
    public CapabilityEntry[] Probe(Action<BridgeEvent> publish);
    public ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish);
}

// --- [COMPOSITION] ------------------------------------------------------------------------

// Ownership: codec policy at the edge with the Contract (wire law). Unmapped-member tolerance is
// DECLARED, not inherited. Thinktecture converters (smart enums, value objects) arrive via the
// .Json companion's attribute wiring; unions ride [JsonDerivedType].
//
// Additive-evolution law, checked per change beside these options:
// 1. Fields are never removed, renamed, retyped, or semantically reused; new meaning is a new
//    optional field with a default — the declared unmapped-member skip makes old readers drop new
//    fields harmlessly.
// 2. New operations are new IBridgeShell methods; -32601 from an older shell folds to
//    CapabilityAbsent.
// 3. Union growth is direction-gated: BridgeEvent/BridgeFault cases may grow (shell->supervisor;
//    the supervisor, rebuilt from repo HEAD every invocation, is always >= the shell, so an
//    unknown $type can only ever reach the newer reader). Supervisor->shell payloads
//    (CargoManifest, ScenarioSelection, Handshake) grow by FIELDS only, never by new union cases,
//    unless the hello capability set gates the send. An unknown discriminator fails LOUD — there
//    is no fallback case.
// 4. Handshake itself never changes shape — it is the one frozen method's single extensible
//    record.
// 5. Smart-enum vocabularies (PhaseStatus, SessionPhase) grow by rows; rank/exit semantics of
//    existing rows are immutable.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(BridgeEvent))]
[JsonSerializable(typeof(BridgeFault))]
[JsonSerializable(typeof(Handshake))]
[JsonSerializable(typeof(CargoManifest))]
[JsonSerializable(typeof(CargoReceipt))]
[JsonSerializable(typeof(ScenarioSelection))]
[JsonSerializable(typeof(ScenarioReceipt[]))]
[JsonSerializable(typeof(UnloadReceipt))]
[JsonSerializable(typeof(SessionEnvelope))]
public sealed partial class BridgeJsonContext : JsonSerializerContext;
