using System.Globalization;
using System.IO.Enumeration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Thinktecture;

namespace Rasm.Bridge.Contract;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: wire status rows carry severity rank and exit code; Worst is the fold monoid.
// Ok=Skipped rank ties keep skip receipt-local, while Timeout and Busy outrank failed work.
// Converter attributes stay in source because STJ generation cannot observe companion output.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<PhaseStatus, ValidationError>))]
public sealed partial class PhaseStatus {
    public static readonly PhaseStatus Ok = new(key: "ok", rank: 1, exitCode: 0);
    public static readonly PhaseStatus Skipped = new(key: "skipped", rank: 1, exitCode: 0);
    public static readonly PhaseStatus Degraded = new(key: "degraded", rank: 2, exitCode: 2);
    public static readonly PhaseStatus Unsupported = new(key: "unsupported", rank: 3, exitCode: 3);
    public static readonly PhaseStatus Failed = new(key: "failed", rank: 4, exitCode: 1);
    public static readonly PhaseStatus Timeout = new(key: "timeout", rank: 5, exitCode: 5);
    public static readonly PhaseStatus Busy = new(key: "busy", rank: 6, exitCode: 5);

    public int Rank { get; }
    public int ExitCode { get; }
    public bool IsDecisive => this != Skipped;
    public PhaseStatus Worst(PhaseStatus other) {
        ArgumentNullException.ThrowIfNull(argument: other);
        return other.Rank > Rank ? other : this;
    }
}

// Ownership: closed session-phase vocabulary. First-fault taxonomy, remedy routing, and quit-rung
// verdicts project from these rows; per-verb decisiveness remains fold policy.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<SessionPhase, ValidationError>))]
public sealed partial class SessionPhase {
    public static readonly SessionPhase Reconcile = new(key: "reconcile");
    public static readonly SessionPhase Launch = new(key: "launch");
    public static readonly SessionPhase Connect = new(key: "connect");
    public static readonly SessionPhase Hello = new(key: "hello");
    public static readonly SessionPhase Stage = new(key: "stage");
    public static readonly SessionPhase Load = new(key: "load");
    public static readonly SessionPhase Probe = new(key: "probe");
    public static readonly SessionPhase Execute = new(key: "execute");
    public static readonly SessionPhase Unload = new(key: "unload");
    public static readonly SessionPhase QuitAe = new(key: "quit.ae");
    public static readonly SessionPhase QuitForce = new(key: "quit.force");
    public static readonly SessionPhase QuitKill = new(key: "quit.kill");
    public static readonly SessionPhase Install = new(key: "install");
    public static readonly SessionPhase Status = new(key: "status");
}

// Ownership: evidence-mode admission. Verify demands reviewed references; Author emits candidates.
// Wire projections are the frozen "verify"/"author" tokens on argv and bridge-closure.json.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<EvidenceMode, ValidationError>))]
public sealed partial class EvidenceMode {
    public static readonly EvidenceMode Verify = new(key: "verify");
    public static readonly EvidenceMode Author = new(key: "author");
}

// Ownership: scenario evidence class. Smoke is admission evidence; CertifiedReference is the
// mandatory reviewed-reference comparison that turns a run into proof.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<EvidenceClass, ValidationError>))]
public sealed partial class EvidenceClass {
    public static readonly EvidenceClass Smoke = new(key: "smoke");
    public static readonly EvidenceClass Semantic = new(key: "semantic");
    public static readonly EvidenceClass Geometry = new(key: "geometry");
    public static readonly EvidenceClass Visual = new(key: "visual");
    public static readonly EvidenceClass CertifiedReference = new(key: "certified-reference");
}

// Ownership: evidence role is the bridge artifact index AND fact-key vocabulary. FactPrefix rows
// are frozen wire law rendered by ScenarioKit FactKey and parsed by the session fold; consumers
// classify through OfFactKey instead of scattering prefix literals or suffix scans.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<EvidenceRole, ValidationError>))]
public sealed partial class EvidenceRole {
    public static readonly EvidenceRole Fact = new(key: "fact", factPrefix: "");
    public static readonly EvidenceRole Assertion = new(key: "assertion", factPrefix: "case.");
    public static readonly EvidenceRole Reference = new(key: "reference", factPrefix: "reference.");
    public static readonly EvidenceRole ObjectManifest = new(key: "object-manifest", factPrefix: "manifest.object.");
    public static readonly EvidenceRole GeometryManifest = new(key: "geometry-manifest", factPrefix: "manifest.geometry.");
    public static readonly EvidenceRole ViewportManifest = new(key: "viewport-manifest", factPrefix: "manifest.viewport.");
    public static readonly EvidenceRole Gh2CanvasManifest = new(key: "gh2-canvas-manifest", factPrefix: "manifest.gh2.");
    public static readonly EvidenceRole Capture = new(key: "capture", factPrefix: "");
    public static readonly EvidenceRole Artifact = new(key: "artifact", factPrefix: "artifact.");
    public static readonly EvidenceRole Scratch = new(key: "scratch", factPrefix: "");
    public static readonly EvidenceRole Certificate = new(key: "certificate", factPrefix: "");
    public static readonly EvidenceRole Forensic = new(key: "forensic", factPrefix: "");
    public static readonly EvidenceRole Spool = new(key: "spool", factPrefix: "");

    public string FactPrefix { get; }

    public static EvidenceRole OfFactKey(string key) =>
        Items.FirstOrDefault(predicate: role => role.OwnsFactKey(key: key)) ?? Fact;

    public bool OwnsFactKey(string key) {
        ArgumentNullException.ThrowIfNull(argument: key);
        return FactPrefix.Length > 0 && key.StartsWith(value: FactPrefix, comparisonType: StringComparison.Ordinal);
    }

    public string FactArgument(string key) => OwnsFactKey(key: key) ? key[FactPrefix.Length..] : key;
}

// Ownership: reviewed-reference admission. Candidate exists only in authoring mode; verify mode
// requires Reviewed plus a Matched result. Unpromoted marks a verify run whose reference root
// carries no reviewed corpus yet — a distinct degraded state, never a structural failure.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<ReferenceAdmission, ValidationError>))]
public sealed partial class ReferenceAdmission {
    public static readonly ReferenceAdmission Reviewed = new(key: "reviewed");
    public static readonly ReferenceAdmission Candidate = new(key: "candidate");
    public static readonly ReferenceAdmission Unpromoted = new(key: "unpromoted");
    public static readonly ReferenceAdmission Missing = new(key: "missing");
    public static readonly ReferenceAdmission Mismatch = new(key: "mismatch");
    public static readonly ReferenceAdmission Matched = new(key: "matched");
}

// Ownership: artifact retention belongs to the bridge certificate, not Assay directory pruning.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[JsonConverter(typeof(Thinktecture.Text.Json.Serialization.ThinktectureSpanParsableJsonConverterFactory<ArtifactRetentionClass, ValidationError>))]
public sealed partial class ArtifactRetentionClass {
    public static readonly ArtifactRetentionClass Evidence = new(key: "evidence");
    public static readonly ArtifactRetentionClass Forensic = new(key: "forensic");
    public static readonly ArtifactRetentionClass Scratch = new(key: "scratch");
    public static readonly ArtifactRetentionClass Transient = new(key: "transient");
}

// --- [ERRORS] -----------------------------------------------------------------------------

// Ownership: closed failure taxonomy. Status and prescription derive from the union, and new
// cases flow only shell->supervisor where the reader is the newer assembly.
[JsonDerivedType(typeof(LaunchFailed), "launch-failed")]
[JsonDerivedType(typeof(ConnectFailed), "connect-failed")]
[JsonDerivedType(typeof(BusyHeld), "busy-held")]
[JsonDerivedType(typeof(ShellSkew), "shell-skew")]
[JsonDerivedType(typeof(HostDrift), "host-drift")]
[JsonDerivedType(typeof(CargoUnloadLeak), "cargo-unload-leak")]
[JsonDerivedType(typeof(RhinoCrash), "rhino-crash")]
[JsonDerivedType(typeof(DialogSuspected), "dialog-suspected")]
[JsonDerivedType(typeof(UiWedged), "ui-wedged")]
[JsonDerivedType(typeof(ExecuteDeadline), "execute-deadline")]
[JsonDerivedType(typeof(NugetLockDrift), "nuget-lock-drift")]
[JsonDerivedType(typeof(CapabilityAbsent), "capability-absent")]
[JsonDerivedType(typeof(RedeployIncomplete), "redeploy-incomplete")]
[Union]
public abstract partial record BridgeFault {
    private BridgeFault() { }
    public sealed record LaunchFailed(string Detail) : BridgeFault;
    public sealed record ConnectFailed(string Detail, double ElapsedMs) : BridgeFault;
    public sealed record BusyHeld(int HolderPid, double AgeSeconds) : BridgeFault;
    public sealed record ShellSkew(int ShellContract, int SupervisorContract) : BridgeFault;
    public sealed record HostDrift(string MissingMember, HostFingerprint BuiltAgainst, HostFingerprint Running) : BridgeFault;
    public sealed record CargoUnloadLeak(string GcdumpPath) : BridgeFault;
    public sealed record RhinoCrash(CrashFact Crash, string Scenario) : BridgeFault;
    public sealed record DialogSuspected(double SilentForMs) : BridgeFault;
    public sealed record UiWedged(double SilentForMs, string Scenario) : BridgeFault;
    public sealed record ExecuteDeadline(string Scenario, double ElapsedMs) : BridgeFault;
    public sealed record NugetLockDrift(string Detail) : BridgeFault;
    public sealed record CapabilityAbsent(string Capability, string ProbeReceipt) : BridgeFault;
    public sealed record RedeployIncomplete(string FailingCheck) : BridgeFault;

    public PhaseStatus Status => Switch(
        busyHeld: static _ => PhaseStatus.Busy,
        executeDeadline: static _ => PhaseStatus.Timeout,
        uiWedged: static _ => PhaseStatus.Timeout,
        capabilityAbsent: static _ => PhaseStatus.Unsupported,
        launchFailed: static _ => PhaseStatus.Failed,
        connectFailed: static _ => PhaseStatus.Failed,
        shellSkew: static _ => PhaseStatus.Failed,
        hostDrift: static _ => PhaseStatus.Failed,
        cargoUnloadLeak: static _ => PhaseStatus.Failed,
        rhinoCrash: static _ => PhaseStatus.Failed,
        dialogSuspected: static _ => PhaseStatus.Failed,
        nugetLockDrift: static _ => PhaseStatus.Failed,
        redeployIncomplete: static _ => PhaseStatus.Failed);

    public string Prescription => Switch(
        shellSkew: static f => string.Create(provider: CultureInfo.InvariantCulture, $"shell contract v{f.ShellContract} < supervisor v{f.SupervisorContract}: run redeploy"),
        nugetLockDrift: static f => $"lock drift ({f.Detail}): run dotnet restore --force-evaluate via the static rail; the bridge never mutates lockfiles",
        busyHeld: static f => string.Create(provider: CultureInfo.InvariantCulture, $"session lease held by pid {f.HolderPid} for {f.AgeSeconds:F0}s: wait or quit that session"),
        capabilityAbsent: static f => $"capability '{f.Capability}' unavailable on this host: {f.ProbeReceipt}",
        launchFailed: static f => f.Detail,
        connectFailed: static f => f.Detail,
        hostDrift: static f => $"host moved under compiled cargo ({f.MissingMember}): rerun verify (auto-rebuild)",
        cargoUnloadLeak: static f => $"cargo ALC leaked; gcdump at {f.GcdumpPath}; session fell back to host recycle",
        rhinoCrash: static f => $"host crashed in '{f.Scenario}': {f.Crash.ExceptionType} on {f.Crash.CrashThread}",
        dialogSuspected: static f => string.Create(provider: CultureInfo.InvariantCulture, $"host alive but silent {f.SilentForMs:F0}ms after launch: modal dialog suspected"),
        uiWedged: static f => string.Create(provider: CultureInfo.InvariantCulture, $"UI thread silent {f.SilentForMs:F0}ms inside '{f.Scenario}'"),
        executeDeadline: static f => string.Create(provider: CultureInfo.InvariantCulture, $"'{f.Scenario}' exceeded the session deadline at {f.ElapsedMs:F0}ms"),
        redeployIncomplete: static f => $"relaunched shell failed status check '{f.FailingCheck}'");
}

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: inert event stamp; session-scoped events leave Scenario null for boundary admission.
public readonly record struct EventStamp(Guid SessionId, long Sequence, long AtUnixMs, string? Scenario);

// Ownership: one evidence type for RPC notifications, JSONL spool lines, and envelope facts.
// Fact keys stay author-open; OnFailure records the trigger and never selects behavior. The Fact
// factories are the single FactCase construction owner across shell, cargo, and supervisor.
[JsonDerivedType(typeof(FactCase), "fact")]
[JsonDerivedType(typeof(CaptureCase), "capture")]
[JsonDerivedType(typeof(PhaseCase), "phase")]
[JsonDerivedType(typeof(ProgressCase), "progress")]
[JsonDerivedType(typeof(HostExceptionCase), "host-exception")]
[Union]
public abstract partial record BridgeEvent {
    private BridgeEvent() { }
    public required EventStamp Stamp { get; init; }

    public sealed record FactCase(string Key, JsonElement Value) : BridgeEvent;
    public sealed record CaptureCase(string Path, int Width, int Height, bool OnFailure) : BridgeEvent {
        public ArtifactRef? Artifact { get; init; }
        public CaptureArtifact? Capture { get; init; }
        public EvidenceClass Class { get; init; } = EvidenceClass.Visual;
    }
    public sealed record PhaseCase(SessionPhase Phase, PhaseStatus Status, double DurationMs, BridgeFault? Fault) : BridgeEvent;
    public sealed record ProgressCase(int Done, int Total) : BridgeEvent;
    public sealed record HostExceptionCase(string Report) : BridgeEvent;

    public static FactCase Fact(string key, JsonElement value, EventStamp stamp = default) =>
        new(Key: key, Value: value) { Stamp = stamp };

    public static FactCase Fact(string key, string value, EventStamp stamp = default) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) { Stamp = stamp };

    public static FactCase Fact(string key, JsonNode payload, EventStamp stamp = default) {
        ArgumentNullException.ThrowIfNull(argument: payload);
        using JsonDocument value = JsonDocument.Parse(json: payload.ToJsonString());
        return new FactCase(Key: key, Value: value.RootElement.Clone()) { Stamp = stamp };
    }
}

// Ownership: the single ~/.rasm home for bridge endpoint, lease, and quit-journal state. Every bridge path under the
// home resolves through this owner; no other site in Contract or Supervisor reconstructs the home directory. The
// dependency-zero Stub mirrors only the home name locally because it loads before the shell ALC exists.
public static class RasmHome {
    public static string Directory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm");

    public static string Resolve(string name) => Path.Combine(path1: Directory, path2: name);
}

// Ownership: the single report-dir layout vocabulary. Every bridge writer and the session fold
// resolve report paths through these rows; the strings are frozen wire law for assay artifact reads.
public static class ReportLayout {
    public const string CertificateFile = "bridge-certificate.json";
    public const string CapturesDirectory = "captures";
    public const string EventsDirectory = "events";
    public const string Gh2Directory = "gh2";
    public const string ManifestsDirectory = "manifests";
    public const string ReferencesDirectory = "references";
    public const string ScratchDirectory = "scratch";

    public static string Certificate(string reportDir) => Path.Combine(path1: reportDir, path2: CertificateFile);

    public static string Spool(string reportDir, string scenario) =>
        Path.Combine(path1: reportDir, path2: EventsDirectory, path3: scenario + ".jsonl");
}

// Ownership: partial-load type enumeration shared by shell and cargo ALC residents; loader faults
// surface through the optional sink instead of aborting discovery.
public static class HostReflection {
    public static IEnumerable<Type> LoadableTypes(Assembly assembly, Action<Exception>? onLoaderFault = null) {
        ArgumentNullException.ThrowIfNull(argument: assembly);
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException partial) {
            foreach (Exception? error in partial.LoaderExceptions) {
                if (error is not null)
                    onLoaderFault?.Invoke(obj: error);
            }
            return partial.Types.Where(predicate: static type => type is not null)!;
        }
    }
}

// Ownership: endpoint admission. Validation owns pipe shape, IsLiveFor owns liveness, and `rbx-`
// remains the distinct pipe family so stale or foreign endpoints reject typed.
[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
[JsonConverter(typeof(Converter))]
public sealed partial class EndpointRecord {
    public const string EndpointFileName = "rhino-bridge-rbx.json";
    public const string PipePrefix = "rbx-";
    public static string EndpointDirectory => RasmHome.Directory;
    public static string EndpointPath => RasmHome.Resolve(name: EndpointFileName);

    public string PipeName { get; }
    public int RhinoPid { get; }
    public long RhinoStartedAtUnixMs { get; }
    public int ContractVersion { get; }
    public string ShellVersion { get; }
    public string RhinoVersion { get; }
    // Live and poisoned endpoint records share this codec so startup failure remains typed evidence.
    public string Fault { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string pipeName, ref int rhinoPid,
        ref long rhinoStartedAtUnixMs, ref int contractVersion, ref string shellVersion,
        ref string rhinoVersion, ref string fault) =>
        validationError = pipeName switch {
            { Length: 0 } => null,
            { Length: <= 64 } when pipeName.StartsWith(value: PipePrefix, comparisonType: StringComparison.Ordinal) => null,
            _ => new ValidationError(message: $"endpoint pipe name must be '{PipePrefix}'-prefixed and <= 64 chars, or empty for a poisoned record"),
        };

    public bool IsLiveFor(int pid, long startedAtUnixMs) =>
        RhinoPid == pid && Math.Abs(value: RhinoStartedAtUnixMs - startedAtUnixMs) <= 1_000;

    // Boundary codec: user-declared JsonConverter suppresses the generated value-object converter,
    // so reads route through Validate while unknown members preserve additive evolution.
    public sealed class Converter : JsonConverter<EndpointRecord> {
        public override EndpointRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            ArgumentNullException.ThrowIfNull(argument: options);
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException(message: $"unexpected token '{reader.TokenType}' deserializing EndpointRecord");
            StringComparer comparer = options.PropertyNameCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            string pipeName = string.Empty;
            int rhinoPid = 0;
            long rhinoStartedAtUnixMs = 0L;
            int contractVersion = 0;
            string shellVersion = string.Empty;
            string rhinoVersion = string.Empty;
            string fault = string.Empty;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                string property = reader.GetString() ?? string.Empty;
                if (!reader.Read())
                    throw new JsonException(message: $"unexpected end of JSON reading '{property}' on EndpointRecord");
                if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(PipeName))))
                    pipeName = reader.GetString() ?? string.Empty;
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(RhinoPid))))
                    rhinoPid = reader.GetInt32();
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(RhinoStartedAtUnixMs))))
                    rhinoStartedAtUnixMs = reader.GetInt64();
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(ContractVersion))))
                    contractVersion = reader.GetInt32();
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(ShellVersion))))
                    shellVersion = reader.GetString() ?? string.Empty;
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(RhinoVersion))))
                    rhinoVersion = reader.GetString() ?? string.Empty;
                else if (comparer.Equals(x: property, y: WireName(options: options, name: nameof(Fault))))
                    fault = reader.GetString() ?? string.Empty;
                else
                    reader.Skip();
            }
            ValidationError? validationError = Validate(
                pipeName: pipeName, rhinoPid: rhinoPid, rhinoStartedAtUnixMs: rhinoStartedAtUnixMs,
                contractVersion: contractVersion, shellVersion: shellVersion, rhinoVersion: rhinoVersion, fault: fault, obj: out EndpointRecord? record);
            return validationError is null && record is not null
                ? record
                : throw new JsonException(message: validationError?.ToString() ?? "unable to deserialize EndpointRecord");
        }

        public override void Write(Utf8JsonWriter writer, EndpointRecord value, JsonSerializerOptions options) {
            ArgumentNullException.ThrowIfNull(argument: writer);
            ArgumentNullException.ThrowIfNull(argument: value);
            ArgumentNullException.ThrowIfNull(argument: options);
            writer.WriteStartObject();
            writer.WriteString(propertyName: WireName(options: options, name: nameof(PipeName)), value: value.PipeName);
            writer.WriteNumber(propertyName: WireName(options: options, name: nameof(RhinoPid)), value: value.RhinoPid);
            writer.WriteNumber(propertyName: WireName(options: options, name: nameof(RhinoStartedAtUnixMs)), value: value.RhinoStartedAtUnixMs);
            writer.WriteNumber(propertyName: WireName(options: options, name: nameof(ContractVersion)), value: value.ContractVersion);
            writer.WriteString(propertyName: WireName(options: options, name: nameof(ShellVersion)), value: value.ShellVersion);
            writer.WriteString(propertyName: WireName(options: options, name: nameof(RhinoVersion)), value: value.RhinoVersion);
            writer.WriteString(propertyName: WireName(options: options, name: nameof(Fault)), value: value.Fault);
            writer.WriteEndObject();
        }

        private static string WireName(JsonSerializerOptions options, string name) =>
            options.PropertyNamingPolicy?.ConvertName(name: name) ?? name;
    }
}

// Inert wire data stays plain because no member carries an admission invariant.
public readonly record struct HostFingerprint(string BundleVersion, string RhinoCommonVersion, string Grasshopper2Version, string RuntimeVersion);
public readonly record struct CapabilityEntry(string Key, PhaseStatus Outcome, string Receipt);
public readonly record struct ScenarioEntry(string Theme, string Name, string[] Requires, int BudgetMs);
public readonly record struct ReferenceRoot(string Assembly, string Theme, string Path);
public readonly record struct EvidenceName(string Key);
public readonly record struct ArtifactHash(string Algorithm, string Value);
public readonly record struct ReferenceTolerance(string Mode, double Absolute, double Relative);
public sealed record ArtifactRef(
    string Id, EvidenceRole Role, string RelativePath, string MediaType, long Bytes,
    ArtifactHash Hash, ArtifactRetentionClass Retention, string Scenario, bool OnFailure);
public sealed record CaptureArtifact(
    ArtifactRef Artifact, int Width, int Height, bool OnFailure, string Label,
    string Frame, string Camera, bool NonBlank);
public sealed record ObjectManifest(string Scenario, int Count, JsonElement[] Objects);
public sealed record GeometryManifest(string Scenario, int Count, JsonElement[] Geometry);
public sealed record ViewportManifest(string Scenario, string ActiveView, JsonElement[] Viewports);
public sealed record Gh2CanvasManifest(string Scenario, int ObjectCount, int WireCount, JsonElement[] Objects, JsonElement[] Wires);
public sealed record ScratchManifest(string Scenario, string Root, string[] Files, ArtifactRef[] Artifacts);
public sealed record ReferenceEvidence(
    EvidenceName Name, EvidenceClass Class, JsonElement Expected,
    ReferenceTolerance Tolerance, ReferenceAdmission Admission, string ReviewedBy, string ReviewedAt);
public sealed record ReferenceEvidenceResult(
    EvidenceName Name, EvidenceClass Class, ReferenceAdmission Admission, bool Matched,
    string ReferencePath, string Detail, ReferenceTolerance Tolerance) {
    public string Scenario { get; init; } = string.Empty;
}
[StructLayout(LayoutKind.Auto)]
public readonly record struct EvidenceCounts(
    int Facts, int Assertions, int References, int ReferenceMatches, int ReferenceFailures,
    int Captures, int Artifacts, int ObjectManifests, int GeometryManifests,
    int ViewportManifests, int Gh2CanvasManifests, int ScratchManifests);
[StructLayout(LayoutKind.Auto)]
public readonly record struct ScenarioCounts(int Total, int Ok, int Failed, int Skipped, int Unsupported, int Timeout, int Busy, int Degraded);
[StructLayout(LayoutKind.Auto)]
public readonly record struct StatusBreakdown(PhaseStatus ScenarioStatus, PhaseStatus SessionStatus, PhaseStatus OverallStatus);
[StructLayout(LayoutKind.Auto)]
public readonly record struct PhaseReceipt(SessionPhase Phase, PhaseStatus Status, double DurationMs, BridgeFault? Fault);
public sealed record FaultSummary(SessionPhase? Phase, BridgeFault? Fault, string Message);
[StructLayout(LayoutKind.Auto)]
public readonly record struct SpoolSummary(long DurableEvents, long RelayedEvents, long LastSequence, bool Diverged, int Failures);
public sealed record EvidenceCertificate(
    string RunId, string Scenario, StatusBreakdown Status, EvidenceClass[] Classes,
    EvidenceCounts Counts, ArtifactRef[] Artifacts, ReferenceEvidenceResult[] References,
    ObjectManifest[] ObjectManifests, GeometryManifest[] GeometryManifests,
    ViewportManifest[] ViewportManifests, Gh2CanvasManifest[] Gh2CanvasManifests,
    ScratchManifest[] ScratchManifests, PhaseReceipt[] Phases, FaultSummary? FirstFault);
// Certificate, artifact index, and evidence counts are session-scoped envelope facts; the receipt
// carries only per-scenario verdict, reference rows, and first failure.
public readonly record struct ScenarioReceipt(string Scenario, PhaseStatus Status, double DurationMs, BridgeFault? Fault) {
    public PhaseStatus ScenarioStatus { get; init; } = Status;
    public ReferenceEvidenceResult[] ReferenceResults { get; init; } = [];
    public string FirstScenarioFailure { get; init; } = string.Empty;
}
public readonly record struct CrashFact(string IpsPath, string CrashThread, string ExceptionType, string Detail);
public readonly record struct UnloadReceipt(bool Confirmed, bool DebuggerAttached, int GcRetries, double ElapsedMs);

// Ownership: quit-scrub receipt. The shell marks every open RhinoDoc clean and re-reads to report
// the residual still-Modified count and any persisted doc Path that would raise the AppKit save sheet
// on terminate; ResidualDirty == 0 is the supervisor's AE-rung precondition. SavedPaths carries the
// on-disk Path of any doc the scrub could not fully clean so a dirty terminate is typed evidence.
public readonly record struct QuitPrepareReceipt(int Documents, int MarkedClean, int ResidualDirty, string Gh2, string[] SavedPaths) {
    public bool Scrubbed => ResidualDirty == 0;
}

// Ownership: per-session cargo carrier; SessionId and ReportDir source all in-host stamps and
// artifacts, while content-hash reuse stays inside the shell swap. Evidence mode and reference
// roots are supervisor-side concerns and never cross into the host.
public sealed record CargoManifest(
    Guid SessionId, string ReportDir, string ContentHash, string StagePath,
    Guid[] HostPlugins, HostFingerprint BuiltAgainst, string[] ScenarioAssemblies);
public sealed record CargoReceipt(string ContentHash, double SwapMs, ScenarioEntry[] Scenarios, CapabilityEntry[] Capabilities);

// Ownership: the frozen negotiation shape. Directional nulls and capability facts keep handshake
// growth additive without dedicated one-off version fields.
public sealed record Handshake(
    int ContractVersion, string SenderVersion,
    CapabilityEntry[] Capabilities, HostFingerprint? Fingerprint, EndpointRecord? Endpoint) {
    // The single contract-version declaration drives both directions and ShellSkew projection.
    public const int CurrentVersion = 1;
    public const string ShellContentCapability = "shell.content.sha256";
}

// Ownership: wire selection by value shape AND its matching semantics; supervisor->shell payloads
// grow by fields unless handshake capabilities gate a new case. Filter is the single owner of
// theme/name resolution: fnmatch-style `*`/`?` wildcards, ordinal case, and bare-method-name hits.
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
        ArgumentNullException.ThrowIfNull(argument: entries);
        return Switch(
            state: entries,
            allCase: static (all, _) => all,
            themesCase: static (all, themes) => [.. all.Where(entry => themes.Themes.Any(pattern => Matches(pattern: pattern, candidate: entry.Theme)))],
            namesCase: static (all, names) => [.. all.Where(entry => names.Names.Any(pattern =>
                Matches(pattern: pattern, candidate: entry.Name) || Matches(pattern: pattern, candidate: MethodOf(name: entry.Name))))]);
    }

    private static bool Matches(string pattern, string candidate) =>
        FileSystemName.MatchesSimpleExpression(expression: pattern, name: candidate, ignoreCase: false);

    private static string MethodOf(string name) {
        int split = name.IndexOf(value: '.', comparisonType: StringComparison.Ordinal);
        return split > 0 && split < name.Length - 1 ? name[(split + 1)..] : name;
    }
}

// Ownership: the terminal session fold. Fields materialize fold results only; evidence carries
// facts and captures while phase history stays in receipts and spool artifacts.
public sealed record SessionEnvelope(
    string RunId, string Verb, PhaseStatus Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioReceipt[] Scenarios,
    BridgeEvent[] Evidence, string FirstFailure, SessionPhase? FirstFaultPhase, BridgeFault? Fault) {
    public PhaseStatus ScenarioStatus { get; init; } = Status;
    public PhaseStatus SessionStatus { get; init; } = Status;
    public PhaseReceipt[] PhaseReceipts { get; init; } = [];
    public string FirstScenarioFailure { get; init; } = string.Empty;
    public string FirstSessionFault { get; init; } = string.Empty;
    public string CertificatePath { get; init; } = string.Empty;
    public ArtifactRef[] ArtifactRefs { get; init; } = [];
    public EvidenceCounts EvidenceCounts { get; init; }
    public ScenarioCounts ScenarioCounts { get; init; }
    public SpoolSummary Spool { get; init; }
}
