using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Thinktecture;

namespace Rasm.Bridge.Contract;

// --- [TYPES] ------------------------------------------------------------------------------

// Ownership: the status algebra is the tool's best diagnostics asset; rows carry wire key +
// severity rank + process exit code; Worst is the rank-max monoid the folds use. Total order law:
// strict above rank 1 with one deliberate Ok=Skipped rank-1 tie (identical exit code 0); Worst
// keeps the accumulator on rank ties and folds seed Ok, so an all-skipped run reads ok at the
// envelope root while every receipt reads skipped — the skip signal is receipt-level, never the
// root. Timeout outranks Failed (a deadline overrun invalidates later verdicts); Busy outranks all
// (nothing ran). Rank/exit semantics of existing rows are immutable; the vocabulary grows by rows.
// The [JsonConverter] rows on the Thinktecture owners are declared in SOURCE, not left to the
// .Json companion's generated attribute: source generators cannot see each other's output, so the
// STJ source generator only defers to the converter when the attribute is visible here. The
// factory is the same public Thinktecture type the generator itself would wire on net9+.
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

// Ownership: the closed session-phase vocabulary; first-non-ok taxonomy and remedy routing are
// projections over this enum, never parallel tables. Quit-ladder rungs are phases (closed:false
// FAILS its rung). Decisiveness per verb is fold policy in the supervisor, not a row flag —
// verify exempts post-verdict lifecycle rungs, quit/redeploy fold them hard.
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
    public static readonly SessionPhase Doctor = new(key: "doctor");
}

// --- [ERRORS] -----------------------------------------------------------------------------

// Ownership: the closed failure taxonomy. Prescription and status are DERIVED projections (one
// Switch each) — remedy text is never a parallel table. Union growth is direction-gated: fault
// cases flow shell->supervisor where the reader is always the newer assembly, so an unknown
// discriminator can only ever reach a reader that knows it.
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
        redeployIncomplete: static f => $"relaunched shell failed doctor check '{f.FailingCheck}'");
}

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: inert envelope stamp — no invariant, so plain record struct, not a decorative
// generated type. Scenario is null for session-scoped events; consumers admit to Option at their
// boundary — null never travels inland.
public readonly record struct EventStamp(Guid SessionId, long Sequence, long AtUnixMs, string? Scenario);

// Ownership: THE evidence stream. Notification payload, JSONL spool line, and envelope evidence
// are this exact type — three folds, zero re-encoding. Fact keys stay author-open strings — the
// one sanctioned open vocabulary. CaptureCase.OnFailure is a recorded fact about the world (which
// trigger fired), not a behavior-selecting knob. Case growth rides the same shell->supervisor
// direction gate as BridgeFault.
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

// Ownership: endpoint admission. The validation partial owns structural admission (pipe prefix +
// length cap); liveness against a live process is the pure method — staleness becomes a typed
// rejection on the supervisor's Fin bridge, never a bool drifting inland. PipePrefix is THE one
// named pipe-prefix constant: `rbx-` is permanently distinct from the old tool's `rb-{pid}-`
// endpoints so dual-run artifacts can never be mistaken for each other.
[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
[JsonConverter(typeof(Converter))]
public sealed partial class EndpointRecord {
    public const string PipePrefix = "rbx-";

    public string PipeName { get; }
    public int RhinoPid { get; }
    public long RhinoStartedAtUnixMs { get; }
    public int ContractVersion { get; }
    public string ShellVersion { get; }
    public string RhinoVersion { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string pipeName, ref int rhinoPid,
        ref long rhinoStartedAtUnixMs, ref int contractVersion, ref string shellVersion, ref string rhinoVersion) =>
        validationError = pipeName is { Length: <= 64 } && pipeName.StartsWith(value: PipePrefix, comparisonType: StringComparison.Ordinal)
            ? null
            : new ValidationError(message: $"endpoint pipe name must be '{PipePrefix}'-prefixed and <= 64 chars");

    public bool IsLiveFor(int pid, long startedAtUnixMs) =>
        RhinoPid == pid && Math.Abs(value: RhinoStartedAtUnixMs - startedAtUnixMs) <= 1_000;

    // Boundary codec, hand-declared for the same generator-isolation reason as the smart-enum
    // converter rows above (a user-declared [JsonConverter] also suppresses the .Json companion's
    // generated complex-value-object converter, so this nested converter IS the wire codec).
    // Deserialization routes through Validate so admission law holds on read; unknown members are
    // skipped per the additive-evolution law, never thrown on.
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
                else
                    reader.Skip();
            }
            ValidationError? validationError = Validate(
                pipeName: pipeName, rhinoPid: rhinoPid, rhinoStartedAtUnixMs: rhinoStartedAtUnixMs,
                contractVersion: contractVersion, shellVersion: shellVersion, rhinoVersion: rhinoVersion, obj: out EndpointRecord? record);
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
            writer.WriteEndObject();
        }

        private static string WireName(JsonSerializerOptions options, string name) =>
            options.PropertyNamingPolicy?.ConvertName(name: name) ?? name;
    }
}

// Inert wire data: plain records/record structs (no invariants — no decorative generated shapes).
public readonly record struct HostFingerprint(string BundleVersion, string RhinoCommonVersion, string Grasshopper2Version, string RuntimeVersion);
public readonly record struct CapabilityEntry(string Key, PhaseStatus Outcome, string Receipt);
public readonly record struct ScenarioEntry(string Theme, string Name, string[] Requires, int BudgetMs);
public readonly record struct ScenarioReceipt(string Scenario, PhaseStatus Status, double DurationMs, BridgeFault? Fault);
public readonly record struct CrashFact(string IpsPath, string CrashThread, string ExceptionType, string Detail);
public readonly record struct UnloadReceipt(bool Confirmed, bool DebuggerAttached, int GcRetries, double ElapsedMs);

// Ownership: the per-session carrier. SessionId + ReportDir give every in-host writer its evidence
// destination (spool JSONL, capture PNG) and EventStamp.SessionId its source. LoadCargoAsync runs
// EVERY session — ContentHash equality short-circuits the swap inside the shell, never the call.
public sealed record CargoManifest(Guid SessionId, string ReportDir, string ContentHash, string StagePath, Guid[] HostPlugins, HostFingerprint BuiltAgainst);
public sealed record CargoReceipt(string ContentHash, double SwapMs, ScenarioEntry[] Scenarios, CapabilityEntry[] Capabilities);

// Ownership: the one frozen-forever negotiation shape, both directions. Fingerprint/endpoint are
// null in the supervisor->shell direction; Capabilities carries the shell's fail-open tap facts in
// the reply. The StreamJsonRpc assembly version rides the reply's Capabilities[] as the fact row
// `rpc.streamjsonrpc` — there is deliberately no dedicated rpc-version field.
public sealed record Handshake(
    int ContractVersion, string SenderVersion,
    CapabilityEntry[] Capabilities, HostFingerprint? Fingerprint, EndpointRecord? Endpoint) {
    // THE one contract-version declaration: both sides send it in ContractVersion and the
    // supervisor projects skew to BridgeFault.ShellSkew. Bumps ride the additive-evolution law.
    public const int CurrentVersion = 1;
}

// Ownership: selection on the wire — discrimination by value shape (one union parameter), never
// runScenario/runScenarios/runAll verb siblings. Supervisor->shell payloads grow by FIELDS only,
// never by new union cases, unless the hello capability set gates the send.
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

// Ownership: the terminal fold of one session — the ONE document python decodes. Fields are fold
// RESULTS materialized at the terminal edge; nothing here is independently maintained state.
// Evidence carries fact+capture cases; phase history lives in Scenarios receipts + the on-disk
// spool referenced by ReportDir. FirstFailure + FirstFaultPhase are the first-non-ok projection in
// wire order.
public sealed record SessionEnvelope(
    string RunId, string Verb, PhaseStatus Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioReceipt[] Scenarios,
    BridgeEvent[] Evidence, string FirstFailure, SessionPhase? FirstFaultPhase, BridgeFault? Fault);
