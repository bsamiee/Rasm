using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Protocol;

// --- [CONSTANTS] ------------------------------------------------------------------------
public static class BridgeWire {
    public const string Schema = "rasm.rhino-bridge.v1";
    public const string Hello = "hello";
    public const string Doctor = "doctor";
    public const string Execute = "execute";
    public const string Load = "load";
    public const string Unload = "unload";
    public const string Quit = "quit";
    public const string Ok = "ok";
    public const string Failed = "failed";
    public const string Unsupported = "unsupported";
    public const string Busy = "busy";
    public const string Timeout = "timeout";
    public const string Skipped = "skipped";
    public const string OutputStdout = "stdout";
    public const string OutputStderr = "stderr";
    public const string OutputCommandStdout = "process.stdout";
    public const string OutputCommandStderr = "process.stderr";
    public static JsonSerializerOptions CompactJson { get; } = Options(writeIndented: false);
    public static JsonSerializerOptions PrettyJson { get; } = Options(writeIndented: true);
    public static string EndpointDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
    public static string EndpointPath => Path.Combine(EndpointDirectory, "rhino-bridge.json");
    public static bool IsCurrent(string? schema) =>
        string.Equals(schema, Schema, StringComparison.Ordinal);
    public static bool IsStatus(string? status) =>
        status is Ok or Failed or Unsupported or Busy or Timeout or Skipped;
    public static BridgeLoadRequest LoadRequest(string assemblyPath, string workspaceRoot) =>
        new(AssemblyPath: assemblyPath, WorkspaceRoot: workspaceRoot);
    public static BridgeExecuteRequest ExecuteRequest(string script, string? scriptPath, IReadOnlyList<string> references) =>
        new(Script: script, ScriptPath: scriptPath, References: references);
    public static BridgeUnloadRequest UnloadRequest(string sessionId) =>
        new(SessionId: sessionId);
    public static BridgeRequest Request<TPayload>(string command, TPayload payload, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: JsonSerializer.SerializeToElement(value: payload, options: CompactJson));
    public static BridgeRequest Request(string command, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: null);
    public static BridgeReply Reply<TData>(
        string command,
        string status,
        TData data,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(
            Schema: Schema,
            Command: command,
            Status: IsStatus(status) ? status : Failed,
            Data: JsonSerializer.SerializeToElement(value: data, options: CompactJson),
            Outputs: outputs ?? [],
            Diagnostics: diagnostics ?? [],
            Fault: fault);
    public static BridgeReply Reply(
        string command,
        string status,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(
            Schema: Schema,
            Command: command,
            Status: IsStatus(status) ? status : Failed,
            Data: null,
            Outputs: outputs ?? [],
            Diagnostics: diagnostics ?? [],
            Fault: fault);
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
    JsonElement? Data,
    IReadOnlyList<BridgeOutput> Outputs,
    IReadOnlyList<BridgeDiagnostic> Diagnostics,
    BridgeFault? Fault);

public sealed record BridgeDoctor(
    string RhinoName,
    string RhinoVersion,
    int RhinoPid,
    bool ActiveDocument,
    double? ModelAbsoluteTolerance,
    IReadOnlyList<BridgeAssemblyReport> Assemblies,
    IReadOnlyList<BridgeSessionReport> Sessions);

public sealed record BridgeAssemblyReport(string Name, string Status, bool Required, string? Version, string? Location, BridgeFault? Fault);
public sealed record BridgeSessionReport(string SessionId, string AssemblyName, string Location, string Status);
public sealed record BridgeLoadRequest(string AssemblyPath, string WorkspaceRoot);
public sealed record BridgeExecuteRequest(string Script, string? ScriptPath, IReadOnlyList<string> References);
public sealed record BridgeUnloadRequest(string SessionId);
public sealed record BridgeLoadReport(string Status, string? SessionId, string? AssemblyName, string? Location, string? PdbPath, IReadOnlyList<BridgeAssemblyReport> Assemblies, BridgeFault? Fault);
public sealed record BridgeExecuteReport(string Status, int DurationMs, string RhinoVersion, bool ActiveDocument, double? ModelAbsoluteTolerance, string? ActiveDocumentName, IReadOnlyList<string> References, BridgeFault? Fault);
public sealed record BridgeUnloadReport(string Status, string SessionId, bool UnloadRequested, bool Unloaded, BridgeFault? Fault);
public sealed record BridgeQuitReport(string Status, int RhinoPid, bool ActiveDocument, bool Modified, BridgeFault? Fault);
public sealed record BridgeOutput(string Source, string Text, bool Truncated);
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
