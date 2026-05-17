using System.Text.Json;
using Rasm.RhinoBridge.Protocol;

namespace Rasm.RhinoBridge.Contracts;

// --- [TYPES] ----------------------------------------------------------------------------
public interface IRhinoBridgeProbe {
    public string Id { get; }
    public RhinoBridgeProbeResult Run(RhinoBridgeProbeContext context);
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record RhinoBridgeProbeContext(
    Rhino.RhinoDoc? Document,
    string WorkspaceRoot,
    JsonElement Arguments,
    TextWriter Output,
    CancellationToken Cancellation);

public sealed record RhinoBridgeProbeResult(
    string Status,
    IReadOnlyList<BridgeDiagnostic> Diagnostics,
    JsonElement? Summary = null) {
    public static RhinoBridgeProbeResult Ok(JsonElement? summary = null) =>
        new(Status: BridgeWire.Ok, Diagnostics: [], Summary: summary);
    public static RhinoBridgeProbeResult Failed(params BridgeDiagnostic[] diagnostics) =>
        new(Status: BridgeWire.Failed, Diagnostics: diagnostics);
    public static RhinoBridgeProbeResult Skipped(params BridgeDiagnostic[] diagnostics) =>
        new(Status: BridgeWire.Skipped, Diagnostics: diagnostics);
}
