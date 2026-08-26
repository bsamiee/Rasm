using System.Collections.Frozen;
using System.Globalization;
using System.IO.Enumeration;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Rasm.Bridge.Contract;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<PhaseStatus, ValidationError>))]
public sealed partial class PhaseStatus {
    public static readonly PhaseStatus Ok = new("ok", exitCode: 0);
    public static readonly PhaseStatus Skipped = new("skipped", exitCode: 0);
    public static readonly PhaseStatus Degraded = new("degraded", exitCode: 2);
    public static readonly PhaseStatus Unsupported = new("unsupported", exitCode: 3);
    public static readonly PhaseStatus Failed = new("failed", exitCode: 1);
    public static readonly PhaseStatus Timeout = new("timeout", exitCode: 5);
    public static readonly PhaseStatus Busy = new("busy", exitCode: 5);

    private static readonly Lazy<FrozenDictionary<PhaseStatus, int>> Severity = new(() =>
        Items.Select((status, rank) => KeyValuePair.Create(status, rank)).ToFrozenDictionary());

    public int ExitCode { get; }
    public int Rank => Severity.Value[this];
    public bool IsDecisive => this != Skipped;

    public PhaseStatus Worst(PhaseStatus other) {
        ArgumentNullException.ThrowIfNull(other);
        return other.IsDecisive && other.Rank > Rank ? other : this;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<SessionPhase, ValidationError>))]
public sealed partial class SessionPhase {
    public static readonly SessionPhase Reconcile = new("reconcile");
    public static readonly SessionPhase Launch = new("launch");
    public static readonly SessionPhase Connect = new("connect");
    public static readonly SessionPhase Hello = new("hello");
    public static readonly SessionPhase Stage = new("stage");
    public static readonly SessionPhase Load = new("load");
    public static readonly SessionPhase Probe = new("probe");
    public static readonly SessionPhase Execute = new("execute");
    public static readonly SessionPhase Unload = new("unload");
    public static readonly SessionPhase QuitAe = new("quit.ae");
    public static readonly SessionPhase QuitForce = new("quit.force");
    public static readonly SessionPhase QuitKill = new("quit.kill");
    public static readonly SessionPhase Status = new("status");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<EvidenceRole, ValidationError>))]
public sealed partial class EvidenceRole {
    public static readonly EvidenceRole Fact = new("fact", factPrefix: "");
    public static readonly EvidenceRole Assertion = new("assertion", factPrefix: "case.");
    public static readonly EvidenceRole ObjectManifest = new("object-manifest", factPrefix: "manifest.object.");
    public static readonly EvidenceRole GeometryManifest = new("geometry-manifest", factPrefix: "manifest.geometry.");
    public static readonly EvidenceRole ViewportManifest = new("viewport-manifest", factPrefix: "manifest.viewport.");
    public static readonly EvidenceRole Gh2CanvasManifest = new("gh2-canvas-manifest", factPrefix: "manifest.gh2.");
    public static readonly EvidenceRole Artifact = new("artifact", factPrefix: "artifact.");
    public static readonly EvidenceRole Capture = new("capture", factPrefix: "");
    public static readonly EvidenceRole Scratch = new("scratch", factPrefix: "");
    public static readonly EvidenceRole Spool = new("spool", factPrefix: "");

    public string FactPrefix { get; }
    public bool IsManifest => FactPrefix.StartsWith("manifest.", StringComparison.Ordinal);

    public static EvidenceRole OfFactKey(string key) =>
        Items.FirstOrDefault(role => role.OwnsFactKey(key)) ?? Fact;

    public bool OwnsFactKey(string key) {
        ArgumentNullException.ThrowIfNull(key);
        return FactPrefix.Length > 0 && key.StartsWith(FactPrefix, StringComparison.Ordinal);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

[JsonDerivedType(typeof(LaunchFailed), "launch-failed")]
[JsonDerivedType(typeof(ConnectFailed), "connect-failed")]
[JsonDerivedType(typeof(BusyHeld), "busy-held")]
[JsonDerivedType(typeof(HostDrift), "host-drift")]
[JsonDerivedType(typeof(CargoRecycleRequired), "cargo-recycle-required")]
[JsonDerivedType(typeof(RhinoCrash), "rhino-crash")]
[JsonDerivedType(typeof(ExecuteDeadline), "execute-deadline")]
[JsonDerivedType(typeof(CapabilityAbsent), "capability-absent")]
[Union]
public abstract partial record BridgeFault {
    public const int RpcErrorCode = -32050;

    private BridgeFault() { }
    public sealed record LaunchFailed(string Detail) : BridgeFault;
    public sealed record ConnectFailed(string Detail, double ElapsedMs) : BridgeFault;
    public sealed record BusyHeld(int HolderPid, double AgeSeconds) : BridgeFault;
    public sealed record HostDrift(string MissingMember, HostFingerprint Running) : BridgeFault;
    public sealed record CargoRecycleRequired(string ActiveContentHash, string RequestedContentHash) : BridgeFault;
    public sealed record RhinoCrash(CrashFact Crash, string Scenario) : BridgeFault;
    public sealed record ExecuteDeadline(string Scenario, double ElapsedMs) : BridgeFault;
    public sealed record CapabilityAbsent(string Capability, string Detail) : BridgeFault;

    public PhaseStatus Status => Switch(
        busyHeld: static _ => PhaseStatus.Busy,
        executeDeadline: static _ => PhaseStatus.Timeout,
        capabilityAbsent: static _ => PhaseStatus.Unsupported,
        launchFailed: static _ => PhaseStatus.Failed,
        connectFailed: static _ => PhaseStatus.Failed,
        hostDrift: static _ => PhaseStatus.Failed,
        cargoRecycleRequired: static _ => PhaseStatus.Failed,
        rhinoCrash: static _ => PhaseStatus.Failed);

    public string Prescription => Switch(
        launchFailed: static f => f.Detail,
        connectFailed: static f => f.Detail,
        busyHeld: static f => string.Create(CultureInfo.InvariantCulture, $"session lease held by pid {f.HolderPid} for {f.AgeSeconds:F0}s: wait or quit that session"),
        hostDrift: static f => $"host capability unavailable ({f.MissingMember}): rebuild against the running Rhino bundle",
        cargoRecycleRequired: static f => $"cargo '{f.ActiveContentHash}' is active while '{f.RequestedContentHash}' was requested: recycle the host before loading changed cargo",
        rhinoCrash: static f => $"host crashed in '{f.Scenario}': {f.Crash.ExceptionType} on {f.Crash.CrashThread}",
        executeDeadline: static f => string.Create(CultureInfo.InvariantCulture, $"'{f.Scenario}' exceeded its deadline at {f.ElapsedMs:F0}ms"),
        capabilityAbsent: static f => $"capability '{f.Capability}' unavailable on this host: {f.Detail}");
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct EventStamp(Guid SessionId, long Sequence, long AtUnixMs, string? Scenario);

[JsonDerivedType(typeof(FactCase), "fact")]
[JsonDerivedType(typeof(CaptureCase), "capture")]
[JsonDerivedType(typeof(PhaseCase), "phase")]
[Union]
public abstract partial record BridgeEvent {
    private BridgeEvent() { }
    public required EventStamp Stamp { get; init; }

    public sealed record FactCase(string Key, JsonElement Value) : BridgeEvent;
    public sealed record CaptureCase(ArtifactRef Artifact, int Width, int Height, string Label, string Camera) : BridgeEvent;
    public sealed record PhaseCase(SessionPhase Phase, PhaseStatus Status, double DurationMs, BridgeFault? Fault) : BridgeEvent;

    public bool IsEvidence => this is FactCase or CaptureCase;

    public BridgeEvent Stamped(EventStamp stamp) => Switch<EventStamp, BridgeEvent>(
        state: stamp,
        factCase: static (at, row) => row with { Stamp = at },
        captureCase: static (at, row) => row with { Stamp = at },
        phaseCase: static (at, row) => row with { Stamp = at });

    public static FactCase Fact(string key, JsonElement value, EventStamp stamp = default) =>
        new(key, value) { Stamp = stamp };

    public static FactCase Fact(string key, string value, EventStamp stamp = default) =>
        new(key, JsonSerializer.SerializeToElement(value, BridgeJsonContext.Default.String)) { Stamp = stamp };

    public static FactCase Fact(string key, JsonNode payload, EventStamp stamp = default) {
        ArgumentNullException.ThrowIfNull(payload);
        return new FactCase(key, JsonSerializer.SerializeToElement(payload, BridgeJsonContext.Default.JsonNode)) { Stamp = stamp };
    }
}

public static class RasmHome {
    public static string Directory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rasm");
    public static string Resolve(string name) => Path.Combine(Directory, name);
}

public static class ReportLayout {
    public const string ProbeSlot = "probe";
    public const string CertificateFile = "bridge-certificate.json";
    public const string CapturesDirectory = "captures";
    public const string EventsDirectory = "events";
    public const string Gh2Directory = "gh2";
    public const string ScratchDirectory = "scratch";
    public const string StageDirectory = "stage";

    public static string Certificate(string reportDir) => Path.Combine(reportDir, CertificateFile);
    public static string Spool(string reportDir, string scenario) => Path.Combine(reportDir, EventsDirectory, scenario + ".jsonl");
    public static string Scratch(string reportDir, string scenario) => Path.Combine(reportDir, ScratchDirectory, scenario);
}

[JsonDerivedType(typeof(Live), "live")]
[JsonDerivedType(typeof(Poisoned), "poisoned")]
[Union]
public abstract partial record EndpointRecord {
    public const string FileName = "rhino-bridge-rbx.json";
    public const string PipePrefix = "rbx-";
    public const long LivenessSkewMs = 1_000;
    public static string FilePath => RasmHome.Resolve(FileName);

    private EndpointRecord() { }
    public sealed record Live(string PipeName, int RhinoPid, long RhinoStartedAtUnixMs, string RhinoVersion) : EndpointRecord {
        public bool IsLiveFor(int pid, long startedAtUnixMs) =>
            RhinoPid == pid && Math.Abs(RhinoStartedAtUnixMs - startedAtUnixMs) <= LivenessSkewMs;
    }
    public sealed record Poisoned(int RhinoPid, long RhinoStartedAtUnixMs, string RhinoVersion, string Fault) : EndpointRecord;
}

public readonly record struct HostFingerprint(string BundleVersion, string RhinoCommonVersion, string Grasshopper2Version, string RuntimeVersion);
public readonly record struct CapabilityEntry(string Key, PhaseStatus Outcome, string Detail);
public sealed record ScenarioEntry(string Theme, string Name, string[] Requires, int BudgetMs);

[ValueObject<string>(
    KeyMemberName = nameof(Key),
    KeyMemberAccessModifier = AccessModifier.Public,
    ConversionFromKeyMemberType = ConversionOperatorsGeneration.None,
    ConversionToKeyMemberType = ConversionOperatorsGeneration.Explicit)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureJsonConverterFactory<EvidenceName, string, ValidationError>))]
public sealed partial class EvidenceName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string key) {
        key = key.Trim();
        if (key.Length == 0) {
            validationError = ValidationError.Create("Evidence name must not be blank.");
        }
    }
}

public sealed record ArtifactRef(string Path, EvidenceRole Role, string MediaType, long Bytes, string Sha256, string Scenario, bool OnFailure) {
    public static ArtifactRef Index(string reportDir, string path, EvidenceRole role, string scenario, bool onFailure) {
        FileInfo info = new(path);
        string relative = System.IO.Path.GetRelativePath(reportDir, path).Replace(System.IO.Path.DirectorySeparatorChar, '/');
        string media = System.IO.Path.GetExtension(path).ToUpperInvariant() switch {
            ".PNG" => "image/png",
            ".JSON" => "application/json",
            ".JSONL" => "application/x-ndjson",
            _ => "application/octet-stream",
        };
        return new ArtifactRef(relative, role, media, info.Exists ? info.Length : 0L, Digest(path), scenario, onFailure);
    }

    private static string Digest(string path) {
        try {
            return Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return string.Empty;
        }
    }
}
public readonly record struct EvidenceCounts(int Facts, int Assertions, int Manifests, int Captures, int Artifacts);
public readonly record struct StatusBreakdown(PhaseStatus Scenario, PhaseStatus Session, PhaseStatus Overall);
public readonly record struct SpoolSummary(long DurableEvents, long RelayedEvents, long LastSequence) {
    public bool Diverged => DurableEvents > RelayedEvents;
}
public readonly record struct ScenarioOutcome(string Scenario, PhaseStatus Status, double DurationMs, BridgeFault? Fault);
public readonly record struct CrashFact(string IpsPath, string CrashThread, string ExceptionType, string Detail);
public readonly record struct UnloadOutcome(bool ReleaseRequested, double ElapsedMs);

[JsonDerivedType(typeof(NotLoaded), "not-loaded")]
[JsonDerivedType(typeof(Scrubbed), "scrubbed")]
[JsonDerivedType(typeof(Failed), "failed")]
[Union]
public abstract partial record Gh2Scrub {
    private Gh2Scrub() { }
    public sealed record NotLoaded : Gh2Scrub;
    public sealed record Scrubbed(int Documents, int ModifiedBefore, int ModifiedAfter) : Gh2Scrub;
    public sealed record Failed(string Detail) : Gh2Scrub;

    public bool IsClean => Switch(
        notLoaded: static _ => true,
        scrubbed: static s => s.ModifiedAfter == 0,
        failed: static _ => false);
}

public sealed record QuitScrub(int Documents, int MarkedClean, int ResidualDirty, Gh2Scrub Gh2, string[] DirtyPaths) {
    public bool Scrubbed => ResidualDirty == 0 && Gh2.IsClean;
}

public sealed record CargoManifest(Guid SessionId, string ReportDir, string ContentHash, string StagePath) {
    public const string AssemblyFile = "Rasm.Bridge.Cargo.dll";
    public const string ScenariosDirectory = "scenarios";
}
public sealed record LoadedCargo(string ContentHash, double LoadMs, ScenarioEntry[] Scenarios, CapabilityEntry[] Capabilities);

[JsonDerivedType(typeof(AllCase), "all")]
[JsonDerivedType(typeof(ThemesCase), "themes")]
[JsonDerivedType(typeof(NamesCase), "names")]
[Union]
public abstract partial record ScenarioSelection {
    private ScenarioSelection() { }
    public sealed record AllCase : ScenarioSelection;
    public sealed record ThemesCase(string[] Themes) : ScenarioSelection;
    public sealed record NamesCase(string[] Names) : ScenarioSelection;

    public ScenarioEntry[] Filter(ScenarioEntry[] entries) {
        ArgumentNullException.ThrowIfNull(entries);
        return Switch(
            state: entries,
            allCase: static (all, _) => all,
            themesCase: static (all, themes) => [.. all.Where(entry => themes.Themes.Any(pattern => Matches(pattern, entry.Theme)))],
            namesCase: static (all, names) => [.. all.Where(entry => names.Names.Any(pattern =>
                Matches(pattern, entry.Name) || Matches(pattern, entry.Name[(entry.Theme.Length + 1)..])))]);
    }

    private static bool Matches(string pattern, string candidate) =>
        FileSystemName.MatchesSimpleExpression(pattern, candidate, ignoreCase: false);
}

public sealed record SessionEnvelope(
    string RunId, string Verb, StatusBreakdown Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioOutcome[] Scenarios, BridgeEvent.PhaseCase[] Phases,
    BridgeEvent[] Evidence, ArtifactRef[] Artifacts, EvidenceCounts Counts, SpoolSummary Spool,
    string FirstFailure, SessionPhase? FaultPhase, BridgeFault? Fault) {
    public int ExitCode => Status.Overall.ExitCode;
}
