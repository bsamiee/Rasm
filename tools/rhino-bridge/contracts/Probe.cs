using System.Text.Json;

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
    IReadOnlyList<RhinoBridgeDiagnostic> Diagnostics,
    JsonElement? Summary = null) {
    public static RhinoBridgeProbeResult Ok(JsonElement? summary = null) =>
        new(Status: RhinoBridgeProbeStatus.Ok, Diagnostics: [], Summary: summary);
    public static RhinoBridgeProbeResult Failed(params RhinoBridgeDiagnostic[] diagnostics) =>
        new(Status: RhinoBridgeProbeStatus.Failed, Diagnostics: diagnostics);
    public static RhinoBridgeProbeResult Skipped(params RhinoBridgeDiagnostic[] diagnostics) =>
        new(Status: RhinoBridgeProbeStatus.Skipped, Diagnostics: diagnostics);
}

public sealed record RhinoBridgeDiagnostic(
    string Severity,
    string Message,
    string? Source = null,
    string? Code = null,
    string? File = null,
    int? Line = null,
    int? Column = null,
    string? Category = null);

public static class RhinoBridgeProbeStatus {
    public const string Ok = "ok";
    public const string Failed = "failed";
    public const string Skipped = "skipped";
}
