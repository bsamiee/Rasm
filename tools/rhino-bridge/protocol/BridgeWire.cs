using System.Collections.Frozen;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.RhinoBridge.Protocol;

// --- [TYPES] ----------------------------------------------------------------------------
public sealed class PhaseStatus {
    public static readonly PhaseStatus Ok = new(wire: "ok", rank: 1, exit: 0);
    public static readonly PhaseStatus Skipped = new(wire: "skipped", rank: 1, exit: 0);
    public static readonly PhaseStatus Unsupported = new(wire: "unsupported", rank: 2, exit: 3);
    public static readonly PhaseStatus Failed = new(wire: "failed", rank: 3, exit: 1);
    public static readonly PhaseStatus Timeout = new(wire: "timeout", rank: 3, exit: 5);
    public static readonly PhaseStatus Busy = new(wire: "busy", rank: 3, exit: 5);
    public static readonly IReadOnlyList<PhaseStatus> Items = [Ok, Skipped, Unsupported, Failed, Timeout, Busy];
    private PhaseStatus(string wire, int rank, int exit) {
        Wire = wire;
        Rank = rank;
        Exit = exit;
    }
    public string Wire { get; }
    public int Rank { get; }
    public int Exit { get; }
    public bool IsOk => ReferenceEquals(objA: this, objB: Ok);
    public bool IsDecisive => !ReferenceEquals(objA: this, objB: Skipped);
    public PhaseStatus Worst(PhaseStatus other) {
        ArgumentNullException.ThrowIfNull(argument: other);
        return other.Rank > Rank ? other : this;
    }
    public static PhaseStatus FromWire(string? wire) =>
        Items.FirstOrDefault(predicate: status => string.Equals(a: status.Wire, b: wire, comparisonType: StringComparison.Ordinal))
            ?? throw new JsonException(message: wire is null ? "phase status wire was null" : $"unsupported phase status wire '{wire}'");
}

public enum FileSystemKind { File, Directory }

// --- [MODELS] ---------------------------------------------------------------------------
public abstract record BridgeMarker {
    public const string Prefix = "rasm.rhino-bridge.";
    public sealed record Returned(JsonElement Value) : BridgeMarker;
    public sealed record Evidence(string Key, string Value) : BridgeMarker;
    public sealed record Capture(string Path, int Width, int Height) : BridgeMarker;
    public sealed record Nonce(string Value) : BridgeMarker;
    public string Serialize() =>
        this switch {
            Returned returned => $"{Prefix}return={JsonSerializer.Serialize(value: returned.Value, options: BridgeWire.CompactJson)}",
            Evidence evidence => $"{Prefix}evidence={evidence.Key}={evidence.Value}",
            Capture capture => $"{Prefix}capture={JsonSerializer.Serialize(value: new { path = capture.Path, width = capture.Width, height = capture.Height }, options: BridgeWire.CompactJson)}",
            Nonce nonce => $"{Prefix}nonce={nonce.Value}",
            _ => throw new UnreachableException(),
        };
    public static BridgeMarker? Parse(string line) =>
        (line ?? string.Empty).Trim() switch {
            { Length: 0 } => null,
            string trimmed when !trimmed.StartsWith(value: Prefix, comparisonType: StringComparison.Ordinal) => null,
            string trimmed => ParsePayload(payload: trimmed[Prefix.Length..]),
        };
    public static IReadOnlyList<BridgeMarker> Scan(string stdout) =>
        [.. (stdout ?? string.Empty).Split(separator: ['\r', '\n'], options: StringSplitOptions.RemoveEmptyEntries)
            .Select(Parse)
            .OfType<BridgeMarker>()];
    public static void Emit(BridgeMarker marker) {
        ArgumentNullException.ThrowIfNull(argument: marker);
        Console.WriteLine(value: marker.Serialize());
    }
    public static void EmitFacts(IReadOnlyDictionary<string, object> facts) {
        ArgumentNullException.ThrowIfNull(argument: facts);
        string serialized = JsonSerializer.Serialize(value: facts, options: BridgeWire.CompactJson);
        Console.WriteLine(value: string.Create(provider: CultureInfo.InvariantCulture, $"facts={serialized}"));
        Emit(marker: new Evidence(Key: "facts", Value: serialized));
    }
    private static BridgeMarker? ParsePayload(string payload) =>
        payload.IndexOf(value: '=', comparisonType: StringComparison.Ordinal) switch {
            < 0 => null,
            int separator => (payload[..separator], payload[(separator + 1)..]) switch {
                ("return", string json) => ParseReturn(json: json),
                ("evidence", string body) => ParseEvidence(body: body),
                ("capture", string json) => ParseCapture(json: json),
                ("nonce", string value) => new Nonce(Value: value),
                _ => null,
            },
        };
    private static Returned? ParseReturn(string json) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: json);
            return new Returned(Value: document.RootElement.Clone());
        } catch (JsonException) {
            return null;
        }
    }
    private static Evidence? ParseEvidence(string body) =>
        body.IndexOf(value: '=', comparisonType: StringComparison.Ordinal) switch {
            < 0 => null,
            int separator => new Evidence(Key: body[..separator], Value: body[(separator + 1)..]),
        };
    private static Capture? ParseCapture(string json) {
        try {
            using JsonDocument document = JsonDocument.Parse(json: json);
            JsonElement root = document.RootElement;
            return new Capture(
                Path: root.TryGetProperty(propertyName: "path", value: out JsonElement path) ? path.GetString() ?? string.Empty : string.Empty,
                Width: root.TryGetProperty(propertyName: "width", value: out JsonElement width) && width.TryGetInt32(value: out int w) ? w : 0,
                Height: root.TryGetProperty(propertyName: "height", value: out JsonElement height) && height.TryGetInt32(value: out int h) ? h : 0);
        } catch (JsonException) {
            return null;
        }
    }
}

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

public abstract record BridgeReport {
    public sealed record Doctor(
        PhaseStatus Status,
        BridgeFault? Fault,
        string RhinoName,
        string RhinoVersion,
        int RhinoPid,
        bool ActiveDocument,
        double? ModelAbsoluteTolerance,
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
        BridgeRhinoCodeReport RhinoCode,
        BridgeDocumentReport Document,
        BridgeReturnValue? ReturnValue,
        IReadOnlyList<string> References) : BridgeReport;
    public sealed record Quit(
        PhaseStatus Status,
        BridgeFault? Fault,
        int RhinoPid,
        bool ActiveDocument,
        bool Modified) : BridgeReport;
}

public sealed record BridgePhase(
    string Phase,
    PhaseStatus Status,
    int DurationMs,
    JsonElement? Data,
    IReadOnlyList<BridgeOutput> Outputs,
    IReadOnlyList<BridgeDiagnostic> Diagnostics,
    BridgeFault? Fault) {
    public static BridgePhase Of<TData>(
        string phase,
        PhaseStatus status,
        TData? data = default,
        string? message = null,
        string? category = null,
        int durationMs = 0,
        Stopwatch? timer = null,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        new(
            Phase: phase,
            Status: status,
            DurationMs: timer is null ? durationMs : (int)timer.ElapsedMilliseconds,
            Data: data is null ? null : JsonSerializer.SerializeToElement(value: data, options: BridgeWire.CompactJson),
            Outputs: outputs ?? [],
            Diagnostics: diagnostics ?? [],
            Fault: fault ?? (message is null ? null : BridgeFault.MessageOnly(category: category ?? phase, message: message)));
    public static BridgePhase Of(
        string phase,
        PhaseStatus status,
        string? message = null,
        string? category = null,
        int durationMs = 0,
        Stopwatch? timer = null,
        IReadOnlyList<BridgeOutput>? outputs = null,
        IReadOnlyList<BridgeDiagnostic>? diagnostics = null,
        BridgeFault? fault = null) =>
        Of<object>(phase: phase, status: status, data: null, message: message, category: category, durationMs: durationMs, timer: timer, outputs: outputs, diagnostics: diagnostics, fault: fault);
    public static BridgePhase FromReply(string phase, BridgeReply reply) {
        ArgumentNullException.ThrowIfNull(argument: reply);
        return new(Phase: phase, Status: reply.Status, DurationMs: 0, Data: reply.Data, Outputs: reply.Outputs, Diagnostics: reply.Diagnostics, Fault: reply.Fault);
    }
    public T? DataValue<T>() =>
        Data is JsonElement data ? data.Deserialize<T>(options: BridgeWire.CompactJson) : default;
}

public sealed record BridgeAssemblyReport(string Name, PhaseStatus Status, bool Required, string? Version, string? InformationalVersion, string? Location, BridgeFault? Fault);
public sealed record BridgeExecuteRequest(string Script, string? ScriptPath, IReadOnlyList<string> References);
public sealed record BridgeRhinoCodeReport(string CachePolicy, bool ResolverIsolated, bool PreferBasePathResolution);
public sealed record BridgeDocumentReport(bool Active, uint? RuntimeSerialNumber, string? Name, string? Path, bool? Modified, double? ModelAbsoluteTolerance, string? ModelUnitSystem);
public sealed record BridgeReturnValue(JsonElement Value, string Source);
public sealed record BridgeOutput(string Source, string Text, bool Truncated, int Length, int Limit);
public sealed record BridgeDiagnostic(string Severity, string Message, string? Source = null, string? Code = null, string? File = null, int? Line = null, int? Column = null, string? Category = null);
public sealed record BridgeFault(string Category, string Message, string? Type = null, string? StackTrace = null, IReadOnlyList<BridgeFault>? Causes = null) {
    public static BridgeFault MessageOnly(string category, string message) => new(Category: category, Message: message);
    public static BridgeFault FromException(string category, Exception error) {
        ArgumentNullException.ThrowIfNull(argument: error);
        Exception? cursor = error.InnerException;
        List<BridgeFault> causes = [];
        while (cursor is Exception inner) {
            causes.Add(item: new BridgeFault(
                Category: category,
                Message: inner.Message,
                Type: inner.GetType().FullName,
                StackTrace: inner.StackTrace));
            cursor = inner.InnerException;
        }
        return new(
            Category: category,
            Message: error.Message,
            Type: error.GetType().FullName,
            StackTrace: error.StackTrace,
            Causes: causes.Count > 0 ? causes : null);
    }
}

// --- [CONSTANTS] ------------------------------------------------------------------------
public static class BridgeWire {
    public const string Schema = "rasm.rhino-bridge.v1";
    public const string ReturnPrefix = BridgeMarker.Prefix + "return=";
    public const string Hello = "hello";
    public const string Doctor = "doctor";
    public const string Execute = "execute";
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
    /// RhinoCode isolated #r order must initialize LanguageExt traits before staged Rasm assemblies touch HashMap keys.
    /// Staged Rasm assemblies must use primitive or built-in tuple HashMap keys only; custom record-struct keys fail under isolated resolver trait warmup.
    public const string LanguageExtBootstrap =
        "global::LanguageExt.HashMap<string, int> __rasmBridgeLanguageExtBootstrap = global::LanguageExt.HashMap<string, int>.Empty;\n"
        + "global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int> __rasmBridgeLanguageExtTupleBootstrap = global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int>.Empty;";
    public static FrozenSet<string> CollisionWatchAssemblyNames { get; } = new[] {
        "FSharp.Core",
        "LanguageExt.Core",
        "Thinktecture.Runtime.Extensions",
    }.ToFrozenSet(comparer: StringComparer.OrdinalIgnoreCase);
    public static int ReferenceLoadOrder(string path, string targetPath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: targetPath);
        string fullPath = Path.GetFullPath(path: path);
        string fullTarget = Path.GetFullPath(path: targetPath);
        return string.Equals(a: fullPath, b: fullTarget, comparisonType: OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
            ? 1000
            : Path.GetFileNameWithoutExtension(path: fullPath) switch {
                "FSharp.Core" => 0,
                "LanguageExt.Core" => 10,
                "Thinktecture.Runtime.Extensions" => 20,
                "Rasm" => 900,
                "Rasm.TestKit" => 910,
                "Rasm.RhinoBridge.Protocol" => 910,
                _ => 100,
            };
    }
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
    public static string SmokeTemplate(string targetPath, string sourceTargetPath, string nonce) {
        ArgumentNullException.ThrowIfNull(argument: targetPath);
        ArgumentNullException.ThrowIfNull(argument: sourceTargetPath);
        ArgumentNullException.ThrowIfNull(argument: nonce);
        string escTarget = Escape(value: targetPath);
        string escSource = Escape(value: sourceTargetPath);
        string watchNames = JsonSerializer.Serialize(value: CollisionWatchAssemblyNames, options: CompactJson);
        return string.Join(separator: Environment.NewLine, values: new[] {
            "using System;",
            "using System.IO;",
            "using System.Linq;",
            "using System.Reflection;",
            "using System.Text.Json;",
            "using Rhino;",
            string.Empty,
            $"string targetLocation = Path.GetFullPath(\"{escTarget}\");",
            $"string sourceTargetLocation = Path.GetFullPath(\"{escSource}\");",
            $"string nonce = \"{nonce}\";",
            $"string[] watchNames = {watchNames};",
            "StringComparison pathCmp = OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;",
            "object Probe() {",
            "  AssemblyName targetName = AssemblyName.GetAssemblyName(targetLocation);",
            "  Assembly loaded = Assembly.LoadFile(targetLocation);",
            "  Assembly[] domain = AppDomain.CurrentDomain.GetAssemblies();",
            "  Assembly[] matches = Array.FindAll(domain, a => string.Equals(a.GetName().Name, targetName.Name, StringComparison.Ordinal));",
            "  Assembly? target = Array.Find(matches, a => ReferenceEquals(a, loaded) || (!string.IsNullOrWhiteSpace(a.Location) && string.Equals(Path.GetFullPath(a.Location), targetLocation, pathCmp)));",
            "  Assembly? stale = Array.Find(matches, a => !ReferenceEquals(a, target));",
            "  var dependencyCollisions = watchNames",
            "    .Select(name => (Name: name, Hits: Array.FindAll(domain, a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase))))",
            "    .Where(entry => entry.Hits.Length > 1)",
            "    .Select(entry => new { name = entry.Name, count = entry.Hits.Length, locations = entry.Hits.Select(a => a.Location ?? string.Empty).ToArray(), versions = entry.Hits.Select(a => a.GetName().Version?.ToString() ?? \"none\").ToArray() })",
            "    .ToArray();",
            "  return new {",
            "    kind = \"assemblyFreshness\",",
            "    nonce,",
            "    sourceTargetLocation,",
            "    targetLocation,",
            "    loadedLocation = target?.Location ?? \"none\",",
            "    preLoadLocation = stale?.Location ?? \"none\",",
            "    alreadyLoaded = stale is not null,",
            "    sameNameAssemblyCount = matches.Length,",
            "    refreshRequired = target is null,",
            "    loadedVersion = target?.GetName().Version?.ToString() ?? \"none\",",
            "    targetVersion = targetName.Version?.ToString() ?? \"none\",",
            "    assemblyInformationalVersion = target?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? \"none\",",
            "    resolverIsolated = true,",
            "    collisionDetected = stale is not null || dependencyCollisions.Length > 0,",
            "    dependencyCollisions,",
            "  };",
            "}",
            $"Console.WriteLine(\"{ReturnPrefix}\" + JsonSerializer.Serialize(Probe()));",
            "_ = AssemblyName.GetAssemblyName(targetLocation);",
            "Assembly[] postProbe = AppDomain.CurrentDomain.GetAssemblies();",
            "AssemblyName postName = AssemblyName.GetAssemblyName(targetLocation);",
            "Assembly[] postMatches = Array.FindAll(postProbe, a => string.Equals(a.GetName().Name, postName.Name, StringComparison.Ordinal));",
            "Assembly? postTarget = Array.Find(postMatches, a => !string.IsNullOrWhiteSpace(a.Location) && string.Equals(Path.GetFullPath(a.Location), targetLocation, pathCmp));",
            "_ = postTarget ?? throw new InvalidOperationException($\"RhinoCode did not load target assembly through isolated #r reference. target={targetLocation}\");",
            $"Console.WriteLine(\"{BridgeMarker.Prefix}nonce={nonce}\");",
            $"Console.Error.WriteLine(\"{BridgeMarker.Prefix}stderr={nonce}\");",
        });
    }
    private static string Escape(string value) =>
        value.Replace(oldValue: "\\", newValue: "\\\\", comparisonType: StringComparison.Ordinal)
             .Replace(oldValue: "\"", newValue: "\\\"", comparisonType: StringComparison.Ordinal);
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
            ? PhaseStatus.FromWire(wire: reader.GetString())
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
