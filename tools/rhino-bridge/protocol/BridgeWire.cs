using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
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
        Console.WriteLine(value: $"facts={serialized}");
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
    string PipeName,
    int RhinoPid,
    DateTimeOffset RhinoStartedAt,
    DateTimeOffset StartedAt,
    string BridgeAssemblyName,
    string BridgeAssemblyVersion,
    string BridgeAssemblyInformationalVersion,
    string RhinoVersion) {
    public const double StartTimeToleranceSeconds = 1.0;
    /// Liveness: PID + process start time within <see cref="StartTimeToleranceSeconds"/>.
    public bool IsLiveFor(int rhinoPid, DateTimeOffset rhinoStartedAt) =>
        RhinoPid == rhinoPid && Math.Abs(value: (RhinoStartedAt - rhinoStartedAt).TotalSeconds) <= StartTimeToleranceSeconds;
    /// Staleness: Hello pipe name must match the on-disk record (guards recycled PID with a new nonce).
    public bool MatchesPipe(string? reportedPipeName) =>
        !string.IsNullOrWhiteSpace(value: reportedPipeName) && string.Equals(a: PipeName, b: reportedPipeName, comparisonType: StringComparison.Ordinal);
}

public sealed record BridgeRequest(string Command, int TimeoutMs, JsonElement? Payload);

public sealed record BridgeReply(
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
        string HostRuntime,
        BridgeFault? ScriptEngine,
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
        IReadOnlyList<string> References,
        BridgeRuntimeDiagnostics Diagnostics) : BridgeReport;
    public sealed record Quit(
        PhaseStatus Status,
        BridgeFault? Fault,
        int RhinoPid,
        bool ActiveDocument,
        bool Modified) : BridgeReport;
}

/// Command-window strings, <c>HostUtils.OnExceptionReport</c> faults, and optional GH2 snapshot (reflection; null when GH2 absent).
public sealed record BridgeRuntimeDiagnostics(IReadOnlyList<string> CommandWindow, IReadOnlyList<BridgeFault> ExceptionReports, BridgeGhSolution? Gh);
/// GH2 solution snapshot via reflection; counts are best-effort; State is the solution server phase name.
public sealed record BridgeGhSolution(bool DocumentInPlay, int ObjectCount, int? WarningCount, int? ErrorCount, string State);

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
public sealed record BridgeExecuteRequest(string Script, string? ScriptPath, IReadOnlyList<string> References, IReadOnlyList<string> HostPlugins);
public sealed record BridgeRhinoCodeReport(string CachePolicy, bool ResolverIsolated, bool PreferBasePathResolution);
public sealed record BridgeDocumentReport(bool Active, uint? RuntimeSerialNumber, string? Name, string? Path, bool? Modified, double? ModelAbsoluteTolerance, string? ModelUnitSystem);
public sealed record BridgeReturnValue(JsonElement Value, string Source);
public sealed record BridgeOutput(string Source, string Text, bool Truncated, int Length, int Limit);
public sealed record BridgeDiagnostic(string Severity, string Message, string? Source = null, string? Code = null, string? File = null, int? Line = null, int? Column = null, string? Category = null);

// --- [ERRORS] ---------------------------------------------------------------------------
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

// --- [POLICIES] -------------------------------------------------------------------------
// All bridge timeouts scale via RASM_BRIDGE_TIMEOUT_SCALE (>0, default 1.0) instead of per-timeout env knobs.
public sealed record TimeoutPolicy(TimeSpan Hello, TimeSpan Connect, TimeSpan Transport, TimeSpan QuitWait, TimeSpan Handshake, TimeSpan IdleDispatch, TimeSpan LivenessSettle) {
    public static TimeoutPolicy Default { get; } = Scaled(scale: EnvScale());
    private static TimeoutPolicy Scaled(double scale) =>
        new(
            Hello: Seconds(seconds: 3.0, scale: scale),
            Connect: Seconds(seconds: 90.0, scale: scale),
            Transport: Seconds(seconds: 35.0, scale: scale),
            QuitWait: Seconds(seconds: 30.0, scale: scale),
            Handshake: Seconds(seconds: 2.0, scale: scale),
            IdleDispatch: Seconds(seconds: 300.0, scale: scale),
            // Settle past the GH2 deferred-paint window so a scenario that destabilises the host (e.g. a
            // StatusBar paint NPE on a shown editor) is caught by the post-execute liveness probe.
            LivenessSettle: Seconds(seconds: 4.0, scale: scale));
    private static TimeSpan Seconds(double seconds, double scale) => TimeSpan.FromSeconds(value: seconds * scale);
    private static double EnvScale() =>
        double.TryParse(s: Environment.GetEnvironmentVariable(variable: "RASM_BRIDGE_TIMEOUT_SCALE"), style: NumberStyles.Float | NumberStyles.AllowThousands, provider: CultureInfo.InvariantCulture, result: out double scale) && scale > 0.0
            ? scale
            : 1.0;
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class BridgeWire {
    public const string ReturnPrefix = BridgeMarker.Prefix + "return=";
    public const string Hello = "hello";
    public const string Doctor = "doctor";
    public const string Execute = "execute";
    public const string Quit = "quit";
    public const string OutputStdout = "stdout";
    public const string OutputStderr = "stderr";
    public const string OutputRhinoCommand = "rhino.command";
    public const string OutputCommandStdout = "process.stdout";
    public const string OutputCommandStderr = "process.stderr";
    public const PipeOptions PipePolicy = PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly;
    public const int OutputLimit = 32768;
    public const int ProcessOutputLimit = 16384;
    /// GH2 plugin id — pre-load into default ALC before isolated execution so the resolver binds, not re-loads, GH2.
    public const string GrasshopperPluginId = "8307876d-a461-4daa-bb77-eb3715925513";
    private static readonly FrozenSet<string> FrameworkReferencePackMarkers = new[] {
        "/packs/Microsoft.NETCore.App.Ref/",
        "/packs/NETStandard.Library.Ref/",
    }.ToFrozenSet(comparer: StringComparer.Ordinal);
    private static readonly FrozenSet<string> HostAssemblyNames = new[] {
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
    public static bool IsHostAssemblyName(string? name) =>
        !string.IsNullOrWhiteSpace(value: name) && HostAssemblyNames.Contains(item: name);
    public static bool IsFrameworkReferencePack(string? path) =>
        !string.IsNullOrEmpty(value: path) && FrameworkReferencePackMarkers.Any(predicate: marker => path.Contains(value: marker, comparisonType: StringComparison.Ordinal));
    /// Bootstrap LanguageExt traits before staged Rasm assemblies; use primitive/tuple HashMap keys only under isolated warmup.
    public const string LanguageExtBootstrap =
        "global::LanguageExt.HashMap<string, int> __rasmBridgeLanguageExtBootstrap = global::LanguageExt.HashMap<string, int>.Empty;\n"
        + "global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int> __rasmBridgeLanguageExtTupleBootstrap = global::LanguageExt.HashMap<(uint Serial, System.Guid DefId), int>.Empty;";
    /// Canonical base usings injected into every scenario so authors drop the repeated testkit/LanguageExt preamble.
    public const string ScenarioBaseUsings =
        "using LanguageExt;\n"
        + "using static LanguageExt.Prelude;\n"
        + "using Rasm.TestKit.Scenarios;\n";
    /// Grasshopper-aware scenario compile surface: host-resolvable usings only (Eto). GH2 namespaces stay in scenario preamble when needed.
    public const string ScenarioHostUsings =
        "using Eto.Drawing;\n";
    /// Injected after preamble; body starts here or at the first non-preamble line.
    public const string ScenarioBodyMarker = "// --- [SCENARIO_BODY] ---";
    private static readonly FrozenSet<string> CollisionWatchAssemblyNames = new[] {
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
        new(Command: command, TimeoutMs: timeoutMs, Payload: JsonSerializer.SerializeToElement(value: payload, options: CompactJson));
    public static BridgeRequest Request(string command, int timeoutMs = 15000) =>
        new(Command: command, TimeoutMs: timeoutMs, Payload: null);
    public static string Serialize(BridgeEndpoint endpoint) =>
        JsonSerializer.Serialize(value: endpoint, options: CompactJson);
    public static BridgeEndpoint? DeserializeEndpoint(string json) =>
        JsonSerializer.Deserialize<BridgeEndpoint>(json: json, options: CompactJson);
    // Length-prefixed UTF-8 JSON frames; lengths outside [0, MaxFrameBytes] fault without unbounded allocation.
    public const int MaxFrameBytes = 64 * 1024 * 1024;
    public static async Task WriteMessageAsync<TMessage>(Stream stream, TMessage message, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(argument: stream);
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(value: message, options: CompactJson);
        byte[] frame = new byte[sizeof(int) + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(destination: frame, value: payload.Length);
        payload.CopyTo(array: frame, index: sizeof(int));
        await stream.WriteAsync(buffer: frame, cancellationToken: token).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken: token).ConfigureAwait(false);
    }
    public static async Task<TMessage?> ReadMessageAsync<TMessage>(Stream stream, CancellationToken token) {
        ArgumentNullException.ThrowIfNull(argument: stream);
        byte[] prefix = new byte[sizeof(int)];
        int header = await stream.ReadAtLeastAsync(buffer: prefix, minimumBytes: prefix.Length, throwOnEndOfStream: false, cancellationToken: token).ConfigureAwait(false);
        // BOUNDARY ADAPTER — a clean EOF before any prefix bytes means the peer closed without sending; yield default.
        if (header < prefix.Length) {
            return default;
        }
        byte[] payload = new byte[FrameLength(prefix: prefix)];
        await stream.ReadExactlyAsync(buffer: payload, cancellationToken: token).ConfigureAwait(false);
        return JsonSerializer.Deserialize<TMessage>(utf8Json: payload, options: CompactJson);
    }
    private static int FrameLength(ReadOnlySpan<byte> prefix) =>
        BinaryPrimitives.ReadInt32LittleEndian(source: prefix) switch {
            >= 0 and <= MaxFrameBytes and int length => length,
            int invalid => throw new InvalidOperationException(message: $"Bridge frame length {invalid.ToString(provider: CultureInfo.InvariantCulture)} is outside [0, {MaxFrameBytes.ToString(provider: CultureInfo.InvariantCulture)}]."),
        };
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
        string watchNames = string.Join(
            separator: ", ",
            values: CollisionWatchAssemblyNames.Select(selector: name => $"\"{Escape(value: name)}\""));
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
            $"string[] watchNames = new[] {{ {watchNames} }};",
            "StringComparison pathCmp = OperatingSystem.IsMacOS() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;",
            "object Probe() {",
            "  AssemblyName targetName = AssemblyName.GetAssemblyName(targetLocation);",
            "  Assembly loaded = Assembly.LoadFile(targetLocation);",
            "  Assembly[] domain = AppDomain.CurrentDomain.GetAssemblies();",
            "  Assembly[] matches = Array.FindAll(domain, a => string.Equals(a.GetName().Name, targetName.Name, StringComparison.Ordinal));",
            "  Assembly target = Array.Find(matches, a => ReferenceEquals(a, loaded) || (!string.IsNullOrWhiteSpace(a.Location) && string.Equals(Path.GetFullPath(a.Location), targetLocation, pathCmp)));",
            "  Assembly stale = Array.Find(matches, a => !ReferenceEquals(a, target));",
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
            "Assembly postTarget = Array.Find(postMatches, a => !string.IsNullOrWhiteSpace(a.Location) && string.Equals(Path.GetFullPath(a.Location), targetLocation, pathCmp));",
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

// --- [OPERATIONS] -----------------------------------------------------------------------
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
