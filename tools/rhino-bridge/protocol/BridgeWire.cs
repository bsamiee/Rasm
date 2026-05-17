using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Protocol;

// --- [CONSTANTS] ------------------------------------------------------------------------
public static class BridgeWire {
    public const string Schema = "rasm.rhino-bridge.v1";
    public const string Hello = "hello";
    public const string Doctor = "doctor";
    public const string Load = "load";
    public const string Run = "run";
    public const string Unload = "unload";
    public const string Ok = "ok";
    public const string Failed = "failed";
    public const string Unsupported = "unsupported";
    public const string Busy = "busy";
    public const string Timeout = "timeout";
    public const string Skipped = "skipped";
    public const string NotLoaded = "not-loaded";
    public static JsonSerializerOptions CompactJson { get; } = Options(writeIndented: false);
    public static JsonSerializerOptions PrettyJson { get; } = Options(writeIndented: true);
    public static string EndpointDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
    public static string EndpointPath => Path.Combine(EndpointDirectory, "rhino-bridge.json");
    public static BridgeLoadRequest LoadRequest(string assemblyPath, string workspaceRoot) =>
        new(AssemblyPath: assemblyPath, WorkspaceRoot: workspaceRoot);
    public static BridgeRunRequest RunRequest(string sessionId, string? probe, JsonElement arguments) =>
        new(SessionId: sessionId, Probe: probe, Arguments: arguments);
    public static BridgeUnloadRequest UnloadRequest(string sessionId) =>
        new(SessionId: sessionId);
    public static BridgeRequest Request<TPayload>(string command, TPayload payload, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: JsonSerializer.SerializeToElement(payload, CompactJson));
    public static BridgeRequest Request(string command, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: null);
    private static JsonSerializerOptions Options(bool writeIndented) => new(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = writeIndented,
    };
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record BridgeEndpoint(
    string Schema,
    string PipeName,
    int RhinoPid,
    DateTimeOffset RhinoStartedAt,
    DateTimeOffset StartedAt,
    string BridgeAssemblyVersion,
    string RhinoVersion);

public sealed record BridgeRequest(string Schema, string Command, int TimeoutMs, JsonElement? Payload);

public sealed record BridgeReply(
    string Schema,
    string Command,
    string Status,
    BridgeEndpoint? Endpoint = null,
    BridgeDoctor? Doctor = null,
    BridgeLoadReport? Load = null,
    BridgeRunReport? Run = null,
    BridgeUnloadReport? Unload = null,
    BridgeFault? Fault = null) {
    public static BridgeReply HelloOk(BridgeEndpoint endpoint) {
        ArgumentNullException.ThrowIfNull(endpoint);
        return new(Schema: BridgeWire.Schema, Command: BridgeWire.Hello, Status: BridgeWire.Ok, Endpoint: endpoint);
    }
    public static BridgeReply DoctorOk(BridgeDoctor doctor) {
        ArgumentNullException.ThrowIfNull(doctor);
        return new(Schema: BridgeWire.Schema, Command: BridgeWire.Doctor, Status: BridgeWire.Ok, Doctor: doctor);
    }
    public static BridgeReply LoadOk(BridgeLoadReport load) {
        ArgumentNullException.ThrowIfNull(load);
        return new(Schema: BridgeWire.Schema, Command: BridgeWire.Load, Status: load.Status, Load: load, Fault: load.Fault);
    }
    public static BridgeReply RunOk(BridgeRunReport run) {
        ArgumentNullException.ThrowIfNull(run);
        return new(Schema: BridgeWire.Schema, Command: BridgeWire.Run, Status: run.Status, Run: run, Fault: run.Fault);
    }
    public static BridgeReply UnloadOk(BridgeUnloadReport unload) {
        ArgumentNullException.ThrowIfNull(unload);
        return new(Schema: BridgeWire.Schema, Command: BridgeWire.Unload, Status: unload.Status, Unload: unload, Fault: unload.Fault);
    }
    public static BridgeReply Rejected(string command, string status, BridgeFault fault) =>
        new(Schema: BridgeWire.Schema, Command: command, Status: status, Fault: fault);
}

public sealed record BridgeRuntimeStatus(bool Running, BridgeEndpoint? Endpoint, BridgeFault? Fault) {
    public static BridgeRuntimeStatus Stopped(BridgeFault? fault = null) => new(Running: false, Endpoint: null, Fault: fault);
    public static BridgeRuntimeStatus Started(BridgeEndpoint endpoint) => new(Running: true, Endpoint: endpoint, Fault: null);
}

public sealed record BridgeDoctor(
    string RhinoName,
    string RhinoVersion,
    int RhinoPid,
    bool ActiveDocument,
    IReadOnlyList<BridgeAssemblyReport> Assemblies,
    IReadOnlyList<BridgeSessionReport> Sessions);

public sealed record BridgeAssemblyReport(string Name, string Status, bool Required, string? Version, string? Location, BridgeFault? Fault);
public sealed record BridgeSessionReport(string SessionId, string AssemblyName, string Location, IReadOnlyList<BridgeProbeDescriptor> Probes);
public sealed record BridgeLoadRequest(string AssemblyPath, string WorkspaceRoot);
public sealed record BridgeRunRequest(string SessionId, string? Probe, JsonElement Arguments);
public sealed record BridgeUnloadRequest(string SessionId);
public sealed record BridgeLoadReport(string Status, string? SessionId, string? AssemblyName, string? Location, string? PdbPath, IReadOnlyList<BridgeProbeDescriptor> Probes, BridgeFault? Fault);
public sealed record BridgeProbeDescriptor(string Id, string TypeName, string AssemblyName);
public sealed record BridgeRunReport(string Status, string SessionId, int DurationMs, IReadOnlyList<BridgeProbeReport> Probes, BridgeFault? Fault);
public sealed record BridgeProbeReport(string Id, string TypeName, string Status, int DurationMs, IReadOnlyList<BridgeDiagnostic> Diagnostics, string Output, bool OutputTruncated, JsonElement? Summary, BridgeFault? Fault);
public sealed record BridgeUnloadReport(string Status, string SessionId, bool UnloadRequested, bool Unloaded, BridgeFault? Fault);
public sealed record BridgeDiagnostic(string Severity, string Message, string? Source = null, string? Code = null, string? File = null, int? Line = null, int? Column = null, string? Category = null);
public sealed record BridgeFault(string Category, string Message, string? Type = null, string? StackTrace = null, IReadOnlyList<BridgeFault>? Causes = null) {
    public static BridgeFault MessageOnly(string category, string message) => new(Category: category, Message: message);
    public static BridgeFault FromException(string category, Exception error) {
        ArgumentNullException.ThrowIfNull(error);
        return new(Category: category, Message: error.Message, Type: error.GetType().FullName, StackTrace: error.StackTrace, Causes: error.InnerException switch {
            Exception inner => [FromException(category: category, error: inner)],
            _ => null,
        });
    }
}
