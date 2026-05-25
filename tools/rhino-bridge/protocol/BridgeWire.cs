using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Protocol;

// --- [CONSTANTS] ------------------------------------------------------------------------
public static class BridgeWire {
    public const string Schema = "rasm.rhino-bridge.v1";
    public const string ReturnPrefix = "rasm.rhino-bridge.return=";
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
    public static FrozenSet<string> HostAssemblyNames { get; } = new[] {
        "Eto",
        "Eto.macOS",
        "Grasshopper",
        "Grasshopper2",
        "GrasshopperIO",
        "Microsoft.macOS",
        "Rasm.RhinoBridge.Protocol",
        "rasm-bridge",
        "Rhino.Runtime.Code",
        "Rhino.UI",
        "RhinoCodePlatform.Rhino3D",
        "RhinoCommon",
        "System.Drawing.Common",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static JsonSerializerOptions CompactJson { get; } = Options(writeIndented: false);
    public static JsonSerializerOptions PrettyJson { get; } = Options(writeIndented: true);
    public static string EndpointDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
    public static string EndpointPath => Path.Combine(EndpointDirectory, "rhino-bridge.json");
    public static bool IsCurrent(string? schema) =>
        string.Equals(schema, Schema, StringComparison.Ordinal);
    public static bool IsStatus(string? status) =>
        status is Ok or Failed or Unsupported or Busy or Timeout or Skipped;
    public static bool IsOk(string? status) =>
        string.Equals(status, Ok, StringComparison.Ordinal);
    public static int Rank(string status) =>
        status switch {
            Failed or Timeout or Busy => 3,
            Unsupported => 2,
            Ok or Skipped => 1,
            _ => 3,
        };
    public static string Worst(string current, string next) =>
        Rank(status: next) > Rank(status: current) ? next : current;
    public static int ExitCode(string status) =>
        status switch {
            Ok => 0,
            Unsupported => 3,
            Busy or Timeout => 5,
            _ => 1,
        };
    public static bool IsHostAssemblyName(string? name) =>
        !string.IsNullOrWhiteSpace(value: name) && HostAssemblyNames.Contains(item: name);
    public static BridgeRequest Request<TPayload>(string command, TPayload payload, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: JsonSerializer.SerializeToElement(value: payload, options: CompactJson));
    public static BridgeRequest Request(string command, int timeoutMs = 15000) =>
        new(Schema: Schema, Command: command, TimeoutMs: timeoutMs, Payload: null);
    public static string Serialize(BridgeRequest request) =>
        JsonSerializer.Serialize(value: request, options: CompactJson);
    public static string Serialize(BridgeReply reply) =>
        JsonSerializer.Serialize(value: reply, options: CompactJson);
    public static string Serialize(BridgeEndpoint endpoint) =>
        JsonSerializer.Serialize(value: endpoint, options: CompactJson);
    public static BridgeReply? DeserializeReply(string json) =>
        JsonSerializer.Deserialize<BridgeReply>(json: json, options: CompactJson);
    public static BridgeEndpoint? DeserializeEndpoint(string json) =>
        JsonSerializer.Deserialize<BridgeEndpoint>(json: json, options: CompactJson);
    public static BridgeOutput Capture(string source, string text, int limit) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfNegative(limit);
        return text.Length <= limit
            ? new(Source: source, Text: text, Truncated: false, Length: text.Length, Limit: limit)
            : new(Source: source, Text: text[..limit], Truncated: true, Length: text.Length, Limit: limit);
    }
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
    string BridgeAssemblyName,
    string BridgeAssemblyVersion,
    string BridgeAssemblyInformationalVersion,
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

public sealed record BridgeAssemblyReport(string Name, string Status, bool Required, string? Version, string? InformationalVersion, string? Location, BridgeFault? Fault);
public sealed record BridgeSessionReport(string SessionId, string AssemblyName, string Location, string Status);
public sealed record BridgeLoadRequest(string AssemblyPath, string WorkspaceRoot, string? PackageCacheRoot);
public sealed record BridgeExecuteRequest(string Script, string? ScriptPath, IReadOnlyList<string> References);
public sealed record BridgeUnloadRequest(string SessionId);
public sealed record BridgeLoadReport(string Status, string? SessionId, string? AssemblyName, string? Location, string? PdbPath, string? WorkspaceRoot, string? PackageCacheRoot, IReadOnlyList<BridgeAssemblyReport> Assemblies, BridgeFault? Fault);
public sealed record BridgeDocumentReport(bool Active, uint? RuntimeSerialNumber, string? Name, string? Path, bool? Modified, double? ModelAbsoluteTolerance, string? ModelUnitSystem);
public sealed record BridgeReturnValue(JsonElement Value, string Source);
public sealed record BridgeRhinoCodePolicy(bool ResolverIsolated, string ResolverOption, string CachePolicy, bool CacheReusable);
public sealed record BridgeExecuteReport(string Status, int DurationMs, bool ServerExecutionCancelable, string BridgeAssemblyName, string BridgeAssemblyVersion, string BridgeAssemblyInformationalVersion, string RhinoVersion, BridgeRhinoCodePolicy RhinoCode, BridgeDocumentReport Document, BridgeReturnValue? ReturnValue, IReadOnlyList<string> References, BridgeFault? Fault);
public sealed record BridgeUnloadReport(string Status, string SessionId, bool UnloadRequested, bool Unloaded, BridgeFault? Fault);
public sealed record BridgeQuitReport(string Status, int RhinoPid, bool ActiveDocument, bool Modified, BridgeFault? Fault);
public sealed record BridgeOutput(string Source, string Text, bool Truncated, int Length, int Limit);
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
