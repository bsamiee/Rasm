using System.Globalization;
using System.Text.Json;
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
    public static readonly PhaseStatus Unsupported = new(key: "unsupported", rank: 2, exitCode: 3);
    public static readonly PhaseStatus Failed = new(key: "failed", rank: 3, exitCode: 1);
    public static readonly PhaseStatus Timeout = new(key: "timeout", rank: 4, exitCode: 5);
    public static readonly PhaseStatus Busy = new(key: "busy", rank: 5, exitCode: 5);

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
// Fact keys stay author-open; OnFailure records the trigger and never selects behavior.
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
    public sealed record CaptureCase(string Path, int Width, int Height, bool OnFailure) : BridgeEvent;
    public sealed record PhaseCase(SessionPhase Phase, PhaseStatus Status, double DurationMs, BridgeFault? Fault) : BridgeEvent;
    public sealed record ProgressCase(int Done, int Total) : BridgeEvent;
    public sealed record HostExceptionCase(string Report) : BridgeEvent;
}

// Ownership: the single ~/.rasm home for bridge endpoint, lease, and quit-journal state. Every bridge path under the
// home resolves through this owner; no other site in Contract or Supervisor reconstructs the home directory. The
// dependency-zero Stub mirrors only the home name locally because it loads before the shell ALC exists.
public static class RasmHome {
    public static string Directory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm");

    public static string Resolve(string name) => Path.Combine(path1: Directory, path2: name);
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
public readonly record struct ScenarioReceipt(string Scenario, PhaseStatus Status, double DurationMs, BridgeFault? Fault);
public readonly record struct CrashFact(string IpsPath, string CrashThread, string ExceptionType, string Detail);
public readonly record struct UnloadReceipt(bool Confirmed, bool DebuggerAttached, int GcRetries, double ElapsedMs);

// Ownership: per-session cargo carrier; SessionId and ReportDir source all in-host stamps and
// artifacts, while content-hash reuse stays inside the shell swap.
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

// Ownership: wire selection by value shape; supervisor->shell payloads grow by fields unless
// handshake capabilities gate a new case.
[JsonDerivedType(typeof(AllCase), "all")]
[JsonDerivedType(typeof(ThemesCase), "themes")]
[JsonDerivedType(typeof(NamesCase), "names")]
[Union]
public abstract partial record ScenarioSelection {
    private ScenarioSelection() { }
    public sealed record AllCase : ScenarioSelection;
    public sealed record ThemesCase(string[] Themes) : ScenarioSelection;
    public sealed record NamesCase(string[] Names) : ScenarioSelection;
}

// Ownership: the terminal session fold. Fields materialize fold results only; evidence carries
// facts and captures while phase history stays in receipts and spool artifacts.
public sealed record SessionEnvelope(
    string RunId, string Verb, PhaseStatus Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioReceipt[] Scenarios,
    BridgeEvent[] Evidence, string FirstFailure, SessionPhase? FirstFaultPhase, BridgeFault? Fault);
