using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Protocol;

// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PhaseStatus {
    public static readonly PhaseStatus Ok = new(key: 0, wire: "ok", rank: 1, exit: 0);
    public static readonly PhaseStatus Skipped = new(key: 1, wire: "skipped", rank: 1, exit: 0);
    public static readonly PhaseStatus Unsupported = new(key: 2, wire: "unsupported", rank: 2, exit: 3);
    public static readonly PhaseStatus Failed = new(key: 3, wire: "failed", rank: 3, exit: 1);
    public static readonly PhaseStatus Timeout = new(key: 4, wire: "timeout", rank: 3, exit: 5);
    public static readonly PhaseStatus Busy = new(key: 5, wire: "busy", rank: 3, exit: 5);
    public string Wire { get; }
    public int Rank { get; }
    public int Exit { get; }
    public bool IsOk => ReferenceEquals(objA: this, objB: Ok);
    public bool IsDecisive => !ReferenceEquals(objA: this, objB: Skipped);
    public PhaseStatus Worst(PhaseStatus other) {
        ArgumentNullException.ThrowIfNull(argument: other);
        return other.Rank > Rank ? other : this;
    }
    public static Fin<PhaseStatus> FromWire(string? wire) =>
        wire is null
            ? Fin.Fail<PhaseStatus>(error: Error.New(message: "phase status wire was null"))
            : Items.FirstOrDefault(predicate: status => string.Equals(a: status.Wire, b: wire, comparisonType: StringComparison.Ordinal))
                is { } match
                    ? Fin.Succ(value: match)
                    : Fin.Fail<PhaseStatus>(error: Error.New(message: $"unsupported phase status wire '{wire}'"));
}

[Union]
public abstract partial record BridgeMarker {
    public const string Prefix = "rasm.rhino-bridge.";
    public sealed record Returned(JsonElement Value) : BridgeMarker;
    public sealed record Evidence(string Key, string Value) : BridgeMarker;
    public sealed record Capture(string Path, int Width, int Height) : BridgeMarker;
    public sealed record Nonce(string Value) : BridgeMarker;
    public string Serialize() => Switch(
        returned: static r => $"{Prefix}return={JsonSerializer.Serialize(value: r.Value, options: BridgeWire.CompactJson)}",
        evidence: static e => $"{Prefix}evidence={e.Key}={e.Value}",
        capture: static c => $"{Prefix}capture={JsonSerializer.Serialize(value: new { path = c.Path, width = c.Width, height = c.Height }, options: BridgeWire.CompactJson)}",
        nonce: static n => $"{Prefix}nonce={n.Value}");
    public static Fin<BridgeMarker> Parse(string line) =>
        (line ?? string.Empty).Trim() switch {
            { Length: 0 } => Fin.Fail<BridgeMarker>(error: Error.New(message: "bridge marker line was empty")),
            string trimmed when !trimmed.StartsWith(value: Prefix, comparisonType: StringComparison.Ordinal) => Fin.Fail<BridgeMarker>(error: Error.New(message: $"bridge marker line lacks prefix '{Prefix}'")),
            string trimmed => ParsePayload(payload: trimmed[Prefix.Length..]),
        };
    public static Seq<BridgeMarker> Scan(string stdout) =>
        toSeq(value: (stdout ?? string.Empty).Split(separator: ['\r', '\n'], options: StringSplitOptions.RemoveEmptyEntries))
            .Choose(selector: line => Parse(line: line).ToOption());
    public static void EmitReturn(object value) {
        ArgumentNullException.ThrowIfNull(argument: value);
        Console.WriteLine(value: new Returned(Value: JsonSerializer.SerializeToElement(value: value, options: BridgeWire.CompactJson)).Serialize());
    }
    public static void EmitEvidence(string key, string value) {
        ArgumentNullException.ThrowIfNull(argument: key);
        ArgumentNullException.ThrowIfNull(argument: value);
        Console.WriteLine(value: new Evidence(Key: key, Value: value).Serialize());
    }
    public static void EmitFact(string key, object value) {
        ArgumentNullException.ThrowIfNull(argument: key);
        ArgumentNullException.ThrowIfNull(argument: value);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"{key}={Format(value: value)}"));
    }
    public static void EmitFacts(object facts) {
        ArgumentNullException.ThrowIfNull(argument: facts);
        string serialized = JsonSerializer.Serialize(value: facts, options: BridgeWire.CompactJson);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"facts={serialized}"));
        Console.WriteLine(value: new Evidence(Key: "facts", Value: serialized).Serialize());
    }
    public static void EmitCapture(string path, int width, int height) {
        ArgumentNullException.ThrowIfNull(argument: path);
        Console.WriteLine(value: new Capture(Path: path, Width: width, Height: height).Serialize());
    }
    public static void EmitScenarioHeader(string scenario, string capturePath) {
        ArgumentNullException.ThrowIfNull(argument: scenario);
        ArgumentNullException.ThrowIfNull(argument: capturePath);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"scenario={scenario}"));
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"capture={capturePath}"));
    }
    private static Fin<BridgeMarker> ParsePayload(string payload) =>
        payload.IndexOf(value: '=', comparisonType: StringComparison.Ordinal) switch {
            < 0 => Fin.Fail<BridgeMarker>(error: Error.New(message: $"bridge marker payload missing '=': {payload}")),
            int separator => (payload[..separator], payload[(separator + 1)..]) switch {
                ("return", string json) => ParseReturn(json: json),
                ("evidence", string body) => ParseEvidence(body: body),
                ("capture", string json) => ParseCapture(json: json),
                ("nonce", string value) => Fin.Succ<BridgeMarker>(value: new Nonce(Value: value)),
                (string kind, _) => Fin.Fail<BridgeMarker>(error: Error.New(message: $"unsupported bridge marker kind '{kind}'")),
            },
        };
    private static Fin<BridgeMarker> ParseReturn(string json) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: json);
            return Fin.Succ<BridgeMarker>(value: new Returned(Value: document.RootElement.Clone()));
        } catch (JsonException error) {
            return Fin.Fail<BridgeMarker>(error: Error.New(message: $"bridge marker return payload was not valid JSON: {error.Message}"));
        }
    }
    private static Fin<BridgeMarker> ParseEvidence(string body) =>
        body.IndexOf(value: '=', comparisonType: StringComparison.Ordinal) switch {
            < 0 => Fin.Fail<BridgeMarker>(error: Error.New(message: $"bridge marker evidence payload missing 'key=value' separator: {body}")),
            int separator => Fin.Succ<BridgeMarker>(value: new Evidence(Key: body[..separator], Value: body[(separator + 1)..])),
        };
    private static Fin<BridgeMarker> ParseCapture(string json) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: json);
            JsonElement root = document.RootElement;
            return Fin.Succ<BridgeMarker>(value: new Capture(
                Path: root.TryGetProperty(propertyName: "path", value: out JsonElement path) ? path.GetString() ?? string.Empty : string.Empty,
                Width: root.TryGetProperty(propertyName: "width", value: out JsonElement width) && width.TryGetInt32(value: out int w) ? w : 0,
                Height: root.TryGetProperty(propertyName: "height", value: out JsonElement height) && height.TryGetInt32(value: out int h) ? h : 0));
        } catch (JsonException error) {
            return Fin.Fail<BridgeMarker>(error: Error.New(message: $"bridge marker capture payload was not valid JSON: {error.Message}"));
        }
    }
    private static string Format(object value) =>
        value switch {
            string text => text,
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(format: null, formatProvider: CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "<null>",
        };
}

public enum FileSystemKind { File, Directory }

// --- [CONSTANTS] ------------------------------------------------------------------------
public static class BridgeWire {
    public const string Schema = "rasm.rhino-bridge.v1";
    public const string ReturnPrefix = BridgeMarker.Prefix + "return=";
    public const string Hello = "hello";
    public const string Doctor = "doctor";
    public const string Execute = "execute";
    public const string Load = "load";
    public const string Unload = "unload";
    public const string Quit = "quit";
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
    }.ToFrozenSet(comparer: StringComparer.OrdinalIgnoreCase);
    public static JsonSerializerOptions CompactJson { get; } = Options(writeIndented: false);
    public static JsonSerializerOptions PrettyJson { get; } = Options(writeIndented: true);
    public static string EndpointDirectory => Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm");
    public static string EndpointPath => Path.Combine(path1: EndpointDirectory, path2: "rhino-bridge.json");
    public static bool IsCurrent(string? schema) =>
        string.Equals(a: schema, b: Schema, comparisonType: StringComparison.Ordinal);
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
        ArgumentNullException.ThrowIfNull(argument: source);
        ArgumentNullException.ThrowIfNull(argument: text);
        ArgumentOutOfRangeException.ThrowIfNegative(value: limit);
        return text.Length <= limit
            ? new(Source: source, Text: text, Truncated: false, Length: text.Length, Limit: limit)
            : new(Source: source, Text: text[..limit], Truncated: true, Length: text.Length, Limit: limit);
    }
    public static BridgeReply Reply<TData>(
        string command,
        PhaseStatus status,
        TData? data = default,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(
            Schema: Schema,
            Command: command,
            Status: status,
            Data: data is null ? null : JsonSerializer.SerializeToElement(value: data, options: CompactJson),
            Outputs: outputs ?? [],
            Diagnostics: diagnostics ?? [],
            Fault: fault);
    public static BridgeReply Reply(
        string command,
        PhaseStatus status,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        Reply<object>(command: command, status: status, data: null, outputs: outputs, diagnostics: diagnostics, fault: fault);
    private static JsonSerializerOptions Options(bool writeIndented) {
        JsonSerializerOptions options = new(defaults: JsonSerializerDefaults.Web) {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = writeIndented,
        };
        options.Converters.Add(item: new PhaseStatusConverter());
        return options;
    }
}

// --- [SERVICES] -------------------------------------------------------------------------
public sealed class PhaseStatusConverter : JsonConverter<PhaseStatus> {
    public override PhaseStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType is JsonTokenType.String
            ? PhaseStatus.FromWire(wire: reader.GetString()).IfFail(error => throw new JsonException(message: error.Message))
            : throw new JsonException(message: $"phase status expected a string token; got {reader.TokenType}");
    public override void Write(Utf8JsonWriter writer, PhaseStatus value, JsonSerializerOptions options) {
        ArgumentNullException.ThrowIfNull(argument: writer);
        ArgumentNullException.ThrowIfNull(argument: value);
        writer.WriteStringValue(value: value.Wire);
    }
}

public static class BoundaryIO {
    public static void Write(string path, string contents, Encoding encoding, Action<string>? restrict = null) {
        ArgumentNullException.ThrowIfNull(argument: path);
        ArgumentNullException.ThrowIfNull(argument: contents);
        ArgumentNullException.ThrowIfNull(argument: encoding);
        _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? Directory.GetCurrentDirectory());
        string temp = string.Create(provider: CultureInfo.InvariantCulture, $"{path}.{Environment.ProcessId}.tmp");
        File.WriteAllText(path: temp, contents: contents, encoding: encoding);
        restrict?.Invoke(obj: temp);
        File.Move(sourceFileName: temp, destFileName: path, overwrite: true);
        restrict?.Invoke(obj: path);
    }
    public static void Restrict(string path, FileSystemKind kind) {
        if (OperatingSystem.IsWindows()) return;
        UnixFileMode mode = kind switch {
            FileSystemKind.Directory => UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
            _ => UnixFileMode.UserRead | UnixFileMode.UserWrite,
        };
        File.SetUnixFileMode(path: path, mode: mode);
    }
    public static void RestrictFile(string path) => Restrict(path: path, kind: FileSystemKind.File);
    public static void RestrictDirectory(string path) => Restrict(path: path, kind: FileSystemKind.Directory);
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
    PhaseStatus Status,
    JsonElement? Data,
    IReadOnlyList<BridgeOutput> Outputs,
    IReadOnlyList<BridgeDiagnostic> Diagnostics,
    BridgeFault? Fault);

[Union]
public abstract partial record BridgeReport {
    public sealed record Doctor(
        PhaseStatus Status,
        BridgeFault? Fault,
        string RhinoName,
        string RhinoVersion,
        int RhinoPid,
        bool ActiveDocument,
        double? ModelAbsoluteTolerance,
        IReadOnlyList<BridgeAssemblyReport> Assemblies,
        IReadOnlyList<BridgeSessionReport> Sessions) : BridgeReport;
    public sealed record Load(
        PhaseStatus Status,
        BridgeFault? Fault,
        string? SessionId,
        string? AssemblyName,
        string? Location,
        string? PdbPath,
        string? WorkspaceRoot,
        string? PackageCacheRoot,
        IReadOnlyList<BridgeAssemblyReport> Assemblies) : BridgeReport;
    public sealed record Execute(
        PhaseStatus Status,
        BridgeFault? Fault,
        int DurationMs,
        bool ServerExecutionCancelable,
        string BridgeAssemblyName,
        string BridgeAssemblyVersion,
        string BridgeAssemblyInformationalVersion,
        string RhinoVersion,
        BridgeDocumentReport Document,
        BridgeReturnValue? ReturnValue,
        IReadOnlyList<string> References) : BridgeReport;
    public sealed record Unload(
        PhaseStatus Status,
        BridgeFault? Fault,
        string SessionId,
        bool UnloadRequested,
        bool Unloaded) : BridgeReport;
    public sealed record Quit(
        PhaseStatus Status,
        BridgeFault? Fault,
        int RhinoPid,
        bool ActiveDocument,
        bool Modified) : BridgeReport;
}

public sealed record BridgeAssemblyReport(string Name, PhaseStatus Status, bool Required, string? Version, string? InformationalVersion, string? Location, BridgeFault? Fault);
public sealed record BridgeSessionReport(string SessionId, string AssemblyName, string Location, PhaseStatus Status);
public sealed record BridgeLoadRequest(string AssemblyPath, string WorkspaceRoot, string? PackageCacheRoot);
public sealed record BridgeExecuteRequest(string Script, string? ScriptPath, IReadOnlyList<string> References);
public sealed record BridgeUnloadRequest(string SessionId);
public sealed record BridgeDocumentReport(bool Active, uint? RuntimeSerialNumber, string? Name, string? Path, bool? Modified, double? ModelAbsoluteTolerance, string? ModelUnitSystem);
public sealed record BridgeReturnValue(JsonElement Value, string Source);
public sealed record BridgeOutput(string Source, string Text, bool Truncated, int Length, int Limit);
public sealed record BridgeDiagnostic(string Severity, string Message, string? Source = null, string? Code = null, string? File = null, int? Line = null, int? Column = null, string? Category = null);
public sealed record BridgeFault(string Category, string Message, string? Type = null, string? StackTrace = null, IReadOnlyList<BridgeFault>? Causes = null) {
    public static BridgeFault MessageOnly(string category, string message) => new(Category: category, Message: message);
    public static BridgeFault FromException(string category, Exception error) {
        ArgumentNullException.ThrowIfNull(argument: error);
        return new(Category: category, Message: error.Message, Type: error.GetType().FullName, StackTrace: error.StackTrace, Causes: error.InnerException switch {
            Exception inner => [FromException(category: category, error: inner)],
            _ => null,
        });
    }
}
