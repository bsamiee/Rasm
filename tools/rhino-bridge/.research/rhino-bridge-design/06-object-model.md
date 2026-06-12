# [06] Object Model — Every Shape the Build Wave Will Create

Design-wave task D6, 2026-06-10. Inputs: the doctrine charter (01 — its §8 checklist binds this doc), 02-05 of this wave, the full prior corpus (cited by bare number), `docs/stacks/csharp/**`, and current-source reads of `tools/rhino-bridge/` (2,158 LOC C#), `tests/csharp/_testkit/Scenarios/` (162 LOC), and `tools/assay/core/model.py:369-419`. This is the complete type inventory: every public type per component as a doctrine-conformant sketch, the dispatch surfaces, the file/LOC budget, and the red-team shape-budget table. A type not in this doc is a type the build wave must argue into existence against R15.

## [0]-[TWO_RECONCILIATIONS] — conflicts between 01 and 05, resolved here

[THINKTECTURE_IN_CONTRACT]: 01 §3 binds the wire vocabulary to Thinktecture generated owners (`[SmartEnum<string>]` status rows — port-verbatim obligation 8; `[ComplexValueObject]` endpoint admission — obligation 3) and places the Thinktecture runtime in the shell-private ALC. 05 §4 law 3 says "Shell and Contract carry no LanguageExt/Thinktecture". Resolution: the charter wins where it binds — Contract references `Thinktecture.Runtime.Extensions` + `Thinktecture.Runtime.Extensions.Json` as DIRECT PackageReferences (never via `UseWorkspaceLibraries`, which stays `false`). 05's real laws survive intact: zero LanguageExt anywhere in the shell ALC, zero workspace-global injection, and the closure grows by exactly two isolated assemblies (`Thinktecture.Runtime.Extensions.dll`, `.Json.dll`) that collide with nothing. Both are Phase-1 central-manifest admissions alongside StreamJsonRpc (01 finding 2).

[NO_FIN_IN_SHELL]: 01 §1 row 3 gives the shell impl a "`Fin<T>` internal" rail posture; 05 §4 law 3 says Fin rails begin cargo-side and supervisor-side. Resolution: 05 wins, and the charter's own D-3(c) is the reason — shell-held LanguageExt materializing `Fin<CargoType>` is a named ALC-pinning hazard, and every shell-ALC assembly is relaunch-priced. The shell's rail IS the Contract vocabulary: foreign exceptions convert once per boundary into `BridgeFault` data (R3 satisfied with Contract types instead of `Error` derivatives), and shell-internal flow is the thin adapter + capsule code the charter already exempts. `Fin`/`Eff`/`Schedule` live where they are load-bearing: cargo and supervisor.

Both resolutions are flagged for charter-owner sign-off in §14.

## [1]-[SYSTEM_TYPE_MAP]

Thirty-four types total across six components. Wire concepts appear exactly once (Contract); everything else is component-private.

| [COMPONENT] | [PUBLIC / SEAM TYPES] | [PRIVATE TYPES] |
| ----------- | --------------------- | --------------- |
| Contract (2 files) | `PhaseStatus`, `SessionPhase`, `BridgeFault`, `BridgeEvent`, `EventStamp`, `EndpointRecord`, `HostFingerprint`, `CapabilityEntry`, `ScenarioEntry`, `ScenarioReceipt`, `CrashFact`, `UnloadReceipt`, `CargoReceipt`, `CargoManifest`, `Handshake`, `ScenarioSelection`, `SessionEnvelope`, `IBridgeShell`, `IBridgeEvents`, `IBridgeCargo`, `BridgeJsonContext` | — |
| Shell stub (1 file) | `RasmBridgePlugin : PlugIn` | `file` ALC bootstrap |
| Shell impl (3 files) | `ShellHost` (activation entry) | `IdlePump`, `CargoGate`, `HostAssemblyTable` |
| Cargo (3 files) | `CargoHost : IBridgeCargo` (activation entry) | `HostCapability`, `Gh2Lane`, `Spool` (owner-local) |
| Supervisor (4 files) | `Program` entry | `SupervisorVerb`, `SessionState`, `LiveHost`, `SupervisorRuntime`, `SessionPolicy`, `SessionFold`, `HostWatch`, `BundleInfo`, `Lease`, `Staging` |
| Scenario SDK (testkit, 2 files) | `RhinoScenarioAttribute`, `ScenarioContext`, `DocumentScope`, `Capture` | — |

The assay consumer (python) adds ZERO new types: `SessionEnvelope` is designed to land field-for-field in the existing `VerifySummary` (`model.py:369-383`) through one msgspec decode.

## [2]-[CONTRACT] — the wire vocabulary, declared once

`Rasm.Bridge.Contract`: pure data + two RPC interfaces + one in-process seam interface. No LanguageExt, no `Error` base, no host types (T-blind per 04 §2 row 21). Packages: `StreamJsonRpc` (for `[JsonRpcContract]` + in-package proxy source-gen, 05 §3.1), `Thinktecture.Runtime.Extensions` + `.Json` (§0). Two files: `Wire.cs` (vocabulary), `Rpc.cs` (interfaces, negotiation, serializer context).

### [2.1] Status and phase vocabulary — `Wire.cs`

```csharp
// Ownership: the status algebra is the tool's best diagnostics asset (obligation 8); rows carry
// wire key + severity rank + process exit code; Worst is the rank-max monoid the folds use.
[SmartEnum<string>]
public sealed partial class PhaseStatus {
    public static readonly PhaseStatus Ok          = new(key: "ok",          rank: 1, exitCode: 0);
    public static readonly PhaseStatus Skipped     = new(key: "skipped",     rank: 1, exitCode: 0);
    public static readonly PhaseStatus Unsupported = new(key: "unsupported", rank: 2, exitCode: 3);
    public static readonly PhaseStatus Failed      = new(key: "failed",      rank: 3, exitCode: 1);
    public static readonly PhaseStatus Timeout     = new(key: "timeout",     rank: 3, exitCode: 5);
    public static readonly PhaseStatus Busy        = new(key: "busy",        rank: 3, exitCode: 5);

    public int Rank { get; }
    public int ExitCode { get; }
    public bool IsDecisive => this != Skipped;
    public PhaseStatus Worst(PhaseStatus other) => other.Rank > Rank ? other : this;
}

// Ownership: the closed session-phase vocabulary; first-non-ok taxonomy and remedy routing are
// projections over this enum, never parallel tables. Quit-ladder rungs are phases (01 §7 row 1:
// closed:false FAILS its rung). Decisiveness per verb is fold policy in the supervisor (§5.4),
// not a row flag — verify exempts post-verdict lifecycle rungs, quit/redeploy fold them hard.
[SmartEnum<string>]
public sealed partial class SessionPhase {
    public static readonly SessionPhase Reconcile = new(key: "reconcile");
    public static readonly SessionPhase Launch    = new(key: "launch");
    public static readonly SessionPhase Connect   = new(key: "connect");
    public static readonly SessionPhase Hello     = new(key: "hello");
    public static readonly SessionPhase Stage     = new(key: "stage");
    public static readonly SessionPhase Load      = new(key: "load");
    public static readonly SessionPhase Probe     = new(key: "probe");
    public static readonly SessionPhase Execute   = new(key: "execute");
    public static readonly SessionPhase Unload    = new(key: "unload");
    public static readonly SessionPhase QuitAe    = new(key: "quit.ae");
    public static readonly SessionPhase QuitForce = new(key: "quit.force");
    public static readonly SessionPhase QuitKill  = new(key: "quit.kill");
    public static readonly SessionPhase Install   = new(key: "install");
    public static readonly SessionPhase Doctor    = new(key: "doctor");
}
```

Both serialize as their key string via `Thinktecture.Runtime.Extensions.Json` generated converters — the hand-written `PhaseStatusConverter` (current `BridgeWire.cs`) is deleted, not ported.

### [2.2] Fault union — `Wire.cs`

```csharp
// Ownership: the closed failure taxonomy of 03's eight feature stories. Prescription and status
// are DERIVED projections (one Switch each) — remedy text is never a parallel table (DERIVED_LOGIC).
[JsonDerivedType(typeof(LaunchFailed),     "launch-failed")]
[JsonDerivedType(typeof(ConnectFailed),    "connect-failed")]
[JsonDerivedType(typeof(BusyHeld),         "busy-held")]
[JsonDerivedType(typeof(ShellSkew),        "shell-skew")]
[JsonDerivedType(typeof(HostDrift),        "host-drift")]
[JsonDerivedType(typeof(CargoUnloadLeak),  "cargo-unload-leak")]
[JsonDerivedType(typeof(RhinoCrash),       "rhino-crash")]
[JsonDerivedType(typeof(DialogSuspected),  "dialog-suspected")]
[JsonDerivedType(typeof(UiWedged),         "ui-wedged")]
[JsonDerivedType(typeof(ExecuteDeadline),  "execute-deadline")]
[JsonDerivedType(typeof(NugetLockDrift),   "nuget-lock-drift")]
[JsonDerivedType(typeof(CapabilityAbsent), "capability-absent")]
[JsonDerivedType(typeof(RedeployIncomplete), "redeploy-incomplete")]
[SkipUnionOps]
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
        busyHeld:         static _ => PhaseStatus.Busy,
        executeDeadline:  static _ => PhaseStatus.Timeout,
        uiWedged:         static _ => PhaseStatus.Timeout,
        capabilityAbsent: static _ => PhaseStatus.Unsupported,
        launchFailed:     static _ => PhaseStatus.Failed,
        connectFailed:    static _ => PhaseStatus.Failed,
        shellSkew:        static _ => PhaseStatus.Failed,
        hostDrift:        static _ => PhaseStatus.Failed,
        cargoUnloadLeak:  static _ => PhaseStatus.Failed,
        rhinoCrash:       static _ => PhaseStatus.Failed,
        dialogSuspected:  static _ => PhaseStatus.Failed,
        nugetLockDrift:   static _ => PhaseStatus.Failed,
        redeployIncomplete: static _ => PhaseStatus.Failed);

    public string Prescription => Switch(
        shellSkew:        static f => $"shell contract v{f.ShellContract} < supervisor v{f.SupervisorContract}: run redeploy",
        nugetLockDrift:   static f => $"lock drift ({f.Detail}): run dotnet restore --force-evaluate via the static rail; the bridge never mutates lockfiles",
        busyHeld:         static f => $"session lease held by pid {f.HolderPid} for {f.AgeSeconds:F0}s: wait or quit that session",
        capabilityAbsent: static f => $"capability '{f.Capability}' unavailable on this host: {f.ProbeReceipt}",
        /* remaining arms: one remedy line each, same shape */
        launchFailed:     static f => f.Detail,
        connectFailed:    static f => f.Detail,
        hostDrift:        static f => $"host moved under compiled cargo ({f.MissingMember}): rerun verify (auto-rebuild) ",
        cargoUnloadLeak:  static f => $"cargo ALC leaked; gcdump at {f.GcdumpPath}; session fell back to host recycle",
        rhinoCrash:       static f => $"host crashed in '{f.Scenario}': {f.Crash.ExceptionType} on {f.Crash.CrashThread}",
        dialogSuspected:  static f => $"host alive but silent {f.SilentForMs:F0}ms after launch: modal dialog suspected",
        uiWedged:         static f => $"UI thread silent {f.SilentForMs:F0}ms inside '{f.Scenario}'",
        executeDeadline:  static f => $"'{f.Scenario}' exceeded the session deadline at {f.ElapsedMs:F0}ms",
        redeployIncomplete: static f => $"relaunched shell failed doctor check '{f.FailingCheck}'");
}
```

Faults named in 03 but absent here, deliberately: `forensics-degraded` and `facts.empty` are FACTS (degraded evidence notes), not faults — 03 F4/F3 word them as notes; host-exception flips ride the status fold over `HostExceptionCase` events, needing no fault case.

### [2.3] The one evidence event family — `Wire.cs`

01 §4 demands fact/capture/phase/progress/host-exception/lifecycle as one closed family. This model collapses lifecycle INTO the phase case: a lifecycle transition (ready, shutdown-started, quit rung, cargo load/unload) has the identical payload shape as a phase transition (phase + status + duration + optional fault) — collapse-scan signal 7 (several shapes sharing fields for one concept). `SessionPhase` rows carry the lifecycle vocabulary; R4's six informational categories remain expressible, as five cases.

```csharp
// Ownership: inert envelope stamp — no invariant, so plain record struct (shapes.md §1 row 10),
// not a decorative generated type. Scenario is null for session-scoped events; consumers admit
// to Option at their boundary (absence law) — null never travels inland.
public readonly record struct EventStamp(Guid SessionId, long Sequence, long AtUnixMs, string? Scenario);

// Ownership: THE evidence stream. Notification payload, JSONL spool line, and envelope evidence
// are this exact type — three folds, zero re-encoding (01 §4). SkipUnionOps: evidence union.
[JsonDerivedType(typeof(FactCase),          "fact")]
[JsonDerivedType(typeof(CaptureCase),       "capture")]
[JsonDerivedType(typeof(PhaseCase),         "phase")]
[JsonDerivedType(typeof(ProgressCase),      "progress")]
[JsonDerivedType(typeof(HostExceptionCase), "host-exception")]
[SkipUnionOps]
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
```

Fact KEYS stay author-open strings — the one sanctioned open vocabulary (01 §4). `CaptureCase.OnFailure` is a recorded fact about the world (which trigger fired), not a behavior-selecting knob; MODAL_ARITY governs parameters, not data fields.

Serialized wire shape, exhibited (resolves 01 open question 2; mechanism verified against the maintainer's integration guidance 2026-06-10 — regular `[Union]` types use STJ-native `[JsonDerivedType]` polymorphism; the `.Json` companion package converters cover smart enums and value objects only):

```json
{"$type":"fact","stamp":{"sessionId":"6a8e…","sequence":17,"atUnixMs":1765432100123,"scenario":"blocks.baseline"},"key":"cargo.swapMs","value":412.3}
{"$type":"phase","stamp":{"sessionId":"6a8e…","sequence":41,"atUnixMs":1765432130001,"scenario":"gh.canvas"},"phase":"execute","status":"failed","durationMs":30000.0,"fault":{"$type":"execute-deadline","scenario":"gh.canvas","elapsedMs":30000.0}}
{"$type":"capture","stamp":{"sessionId":"6a8e…","sequence":42,"atUnixMs":1765432130020,"scenario":"gh.canvas"},"path":".artifacts/rhino/verify/6a8e…/gh.canvas.png","width":1280,"height":720,"onFailure":true}
```

### [2.4] Records, receipts, negotiation — `Wire.cs` + `Rpc.cs`

```csharp
// Ownership: endpoint admission (obligation 3). The validation partial owns structural admission
// (pipe prefix + length cap); liveness against a live process is the pure method — staleness
// becomes a typed rejection on the supervisor's Fin bridge, never a bool drifting inland.
[ComplexValueObject]
public sealed partial class EndpointRecord {
    public string PipeName { get; }
    public int RhinoPid { get; }
    public long RhinoStartedAtUnixMs { get; }
    public int ContractVersion { get; }
    public string ShellVersion { get; }
    public string RhinoVersion { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string pipeName, ref int rhinoPid,
        ref long rhinoStartedAtUnixMs, ref int contractVersion, ref string shellVersion, ref string rhinoVersion) =>
        validationError = pipeName is { Length: <= 64 } && pipeName.StartsWith("rb-", StringComparison.Ordinal)
            ? null
            : new ValidationError("endpoint pipe name must be rb-prefixed and <= 64 chars");

    public bool IsLiveFor(int pid, long startedAtUnixMs) =>
        RhinoPid == pid && Math.Abs(RhinoStartedAtUnixMs - startedAtUnixMs) <= 1_000;
}

// Inert wire data: plain records/record structs (no invariants — no decorative generated shapes).
public readonly record struct HostFingerprint(string BundleVersion, string RhinoCommonVersion, string Grasshopper2Version, string RuntimeVersion);
public readonly record struct CapabilityEntry(string Key, PhaseStatus Outcome, string Receipt);
public readonly record struct ScenarioEntry(string Theme, string Name, string[] Requires, int BudgetMs);
public readonly record struct ScenarioReceipt(string Scenario, PhaseStatus Status, double DurationMs, BridgeFault? Fault);
public readonly record struct CrashFact(string IpsPath, string CrashThread, string ExceptionType, string Detail);
public readonly record struct UnloadReceipt(bool Confirmed, bool DebuggerAttached, int GcRetries, double ElapsedMs);

public sealed record CargoManifest(string ContentHash, string StagePath, Guid[] HostPlugins, HostFingerprint BuiltAgainst);
public sealed record CargoReceipt(string ContentHash, double SwapMs, ScenarioEntry[] Scenarios, CapabilityEntry[] Capabilities);

// Ownership: M3's one frozen-forever negotiation shape, both directions. Fingerprint/endpoint are
// null in the supervisor->shell direction; Capabilities carries the shell's fail-open T2 tap facts
// in the reply (04 hook 5).
// [SUPERSEDED — 10 §2 amendment 16] The RpcVersion member below is NOT built: the StreamJsonRpc
// assembly version rides the reply's Capabilities[] as the fact row `rpc.streamjsonrpc`
// (07 §2 pin 2 is the operative shape; the "(05 OQ5)" cite this section once carried is closed there).
public sealed record Handshake(
    int ContractVersion, string SenderVersion, /* RpcVersion: superseded by amendment 16 */
    CapabilityEntry[] Capabilities, HostFingerprint? Fingerprint, EndpointRecord? Endpoint);

// Ownership: MODAL_ARITY on the wire — selection discriminates by value shape (one union/collection
// parameter), never runScenario/runScenarios/runAll siblings (01 §4 VERBS).
[JsonDerivedType(typeof(AllCase),    "all")]
[JsonDerivedType(typeof(ThemesCase), "themes")]
[JsonDerivedType(typeof(NamesCase),  "names")]
[SkipUnionOps]
[Union]
public abstract partial record ScenarioSelection {
    private ScenarioSelection() { }
    public sealed record AllCase : ScenarioSelection;
    public sealed record ThemesCase(string[] Themes) : ScenarioSelection;
    public sealed record NamesCase(string[] Names) : ScenarioSelection;
}

// Ownership: the terminal fold of one session — the ONE document python decodes (01 §1 row 7).
// Fields are fold RESULTS materialized at the terminal edge; nothing here is independently
// maintained state (R4). Evidence carries fact+capture cases; phase history lives in Scenarios
// receipts + the on-disk spool referenced by ReportDir.
public sealed record SessionEnvelope(
    string RunId, string Verb, PhaseStatus Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioReceipt[] Scenarios,
    BridgeEvent[] Evidence, string FirstFailure, BridgeFault? Fault);
```

### [2.5] RPC + seam interfaces — `Rpc.cs`

```csharp
// Ownership: the supervisor->shell verb surface. Six methods, no arity/mode siblings, no boolean
// knobs; JSON-RPC name dispatch is the protocol's law and the one boundary where names are allowed
// (01 §4). Per-method CancellationToken = transport hygiene only, never UI-thread abort (D-1).
[JsonRpcContract]
public partial interface IBridgeShell {
    Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct);
    Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct);   // load + discover + probe, one transaction (M1 trigger = after load)
    Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct);
    Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct);
    Task<long> PingAsync(CancellationToken ct);                                        // answered on the pipe thread: discriminates dead vs alive-but-wedged
    Task PrepareQuitAsync(CancellationToken ct);                                       // docs-clean before the AE rung
}

// Ownership: the shell->supervisor evidence stream. ONE notification method; the event union is
// the discriminator (the five-channel collapse of 01 §4 EVIDENCE made literal).
[JsonRpcContract]
public partial interface IBridgeEvents {
    Task PublishAsync(BridgeEvent evt);
}

// Ownership: the in-process shell<->cargo seam (not RPC). Contract-typed and primitive-only, so no
// cargo type can reach shell-held state (D-3c). Synchronous: every call rides one IdlePump frame.
// publish is a SHELL-owned delegate handed into cargo (collectible->non-collectible: legal direction).
// Dispose is the D-2 unload precondition: scenario scopes + host-event detachers drained.
public interface IBridgeCargo : IDisposable {
    ScenarioEntry[] Discover();
    CapabilityEntry[] Probe(Action<BridgeEvent> publish);
    ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish);
}

// Ownership: codec policy at the edge with the Contract (wire law). Unmapped-member tolerance is
// DECLARED, not inherited (04 hook 4). Thinktecture converters arrive via the .Json companion's
// attribute wiring; unions ride [JsonDerivedType] (§2.3 exhibit).
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(BridgeEvent))]
[JsonSerializable(typeof(BridgeFault))]
[JsonSerializable(typeof(Handshake))]
[JsonSerializable(typeof(CargoManifest))]
[JsonSerializable(typeof(CargoReceipt))]
[JsonSerializable(typeof(ScenarioSelection))]
[JsonSerializable(typeof(ScenarioReceipt[]))]
[JsonSerializable(typeof(UnloadReceipt))]
[JsonSerializable(typeof(SessionEnvelope))]
public sealed partial class BridgeJsonContext : JsonSerializerContext;
```

### [2.6] Additive-evolution law

The contract is frozen after v1 in SHAPE, open in CONTENT, under five rules the red team can check per change:

1. Fields are never removed, renamed, retyped, or semantically reused; new meaning is a new optional field with a default. STJ unmapped-member skip (declared in `BridgeJsonContext` options) makes old readers drop new fields harmlessly (M3).
2. New operations are new `IBridgeShell` methods; an older shell answers them with JSON-RPC `-32601`, which the supervisor's rail bridge folds to the SAME `CapabilityAbsent` outcome as a failed host probe (04 M3 — one degradation lattice).
3. Union growth is direction-gated: `BridgeEvent`/`BridgeFault` cases may grow because they flow shell→supervisor and the supervisor (rebuilt from repo HEAD every invocation) is always >= the shell — an unknown `$type` can only ever reach the newer reader. Supervisor→shell payloads (`CargoManifest`, `ScenarioSelection`, `Handshake`) grow by FIELDS only, never by new union cases, unless the hello capability set gates the send. Phase-1 must verify STJ's unknown-discriminator failure shape under `SystemTextJsonFormatter` and encode this direction rule beside the serializer options (§14).
4. `Handshake` itself never changes shape — it is the one frozen method's single extensible record (M3 audit row).
5. Smart-enum vocabularies (`PhaseStatus`, `SessionPhase`) grow by rows; rank/exit semantics of existing rows are immutable.

## [3]-[SHELL] — stub + impl

### [3.1] Stub — `RasmBridge.rhp`, default ALC, one file (D-5 two-assembly shape)

```csharp
// Ownership: the only type Rhino's shared plugin context ever sees. References RhinoCommon and
// nothing else (05 §4): immune to host bundle churn and co-resident plugin closures (RhinoMCP).
// One reflective activation hop into the shell ALC is the stub's entire job — the named exemption
// to ONE_HOP_RESOLUTION, forced by the zero-dependency requirement (D-5).
public sealed class RasmBridgePlugin : Rhino.PlugIns.PlugIn {
    // PlugInLoadTime.AtStartup via assembly attribute; OnLoad defers to first RhinoApp.Idle (F15 fix:
    // a failed start writes a poisoned endpoint record naming the load fault instead of one WriteLine).
}

file sealed class ShellLoadContext : System.Runtime.Loader.AssemblyLoadContext {
    // non-collectible, AssemblyDependencyResolver over the shell deploy dir;
    // constructed once, hands back the ShellHost entry via one LoadFromAssemblyPath + Activator call.
}
```

The current tool's three Rhino commands (`RasmBridgeStart/Stop/Status`) are NOT ported: agents never type Rhino commands, `doctor` owns status, and the supervisor owns lifecycle — deletion dividend (agent-first directive).

### [3.2] Shell impl — shell-private ALC, three files

`ShellHost.cs` — composition root + RPC target (one owner block):

```csharp
// Ownership: pipe server lifecycle, StreamJsonRpc attach, endpoint file write/heal, host event
// subscriptions (disposable + symmetric — event law), GH2 preload effect, readiness PhaseCase.
// Implements IBridgeShell directly: method bodies are one-hop boundary adapters — admit Contract
// payload, dispatch to CargoGate/IdlePump, project Contract outcome. No rail types (§0).
public sealed class ShellHost : IBridgeShell, IDisposable {
    public static ShellHost Start(string deployDir, int rhinoPid);    // called by the stub via the ALC seam

    public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct);     // version+capability negotiation; fingerprint read
    public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct); // GH2 preload (GUIDs from manifest — 04 row 2) -> CargoGate.Swap -> Discover+Probe on one idle frame
    public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct);
    public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct);
    public Task<long> PingAsync(CancellationToken ct);                 // pipe-thread monotonic stamp
    public Task PrepareQuitAsync(CancellationToken ct);

    // internals: IBridgeEvents proxy (notifications out), OnExceptionReport tap -> HostExceptionCase,
    // RhinoApp.Closing -> PhaseCase(QuitAe-adjacent shutdown signal), endpoint EnsureEndpoint port.
}
```

`IdlePump.cs` — the D-1 kernel:

```csharp
// Ownership: ALL host mutation marshals through here; one job per RhinoApp.Idle pulse; the drain
// loop is the charter's named statement exemption (D-1). Failure is captured per job and projected
// as BridgeFault data — raw InvokeOnUiThread is forbidden (it swallows).
internal sealed class IdlePump : IDisposable {
    internal Task<T> OnUiThread<T>(Func<T> job, CancellationToken ct);  // ct abandons the WAIT only — host law
}
```

`CargoGate.cs` — the D-3 owner:

```csharp
// Ownership: collectible-ALC lifecycle. Token-gated single ownership (the token-gated-singleton
// pattern re-housed without LanguageExt: System.Threading.Lock + generation token). Swap = dispose
// IBridgeCargo -> Unload() -> WeakReference + bounded-GC confirm kernel (D-3a) -> new collectible
// ALC over the staged dir -> Activator the CargoHost entry -> cast to Contract's IBridgeCargo.
internal sealed class CargoGate : IDisposable {
    internal CargoReceipt Swap(CargoManifest manifest, Action<BridgeEvent> publish);  // on the idle frame
    internal UnloadReceipt Unload();                                                  // typed receipt; debugger-gated (D-3d)
    internal IBridgeCargo? Current { get; }
}

// Ownership: D-4's forwarding table as data — one declaration (SYMBOLIC_REFERENCE). Host-owned
// names -> default ALC instances; bridge-owned names (Contract + StreamJsonRpc closure) -> shell
// ALC instances; everything else cargo-first. Ports the current HostAssemblyNames deny-list.
internal static class HostAssemblyTable {
    internal static readonly FrozenSet<string> HostOwned;    // RhinoCommon, Grasshopper2.*, Eto.*, Microsoft.macOS, …
    internal static readonly FrozenSet<string> ShellOwned;   // Rasm.Bridge.Contract, StreamJsonRpc, Nerdbank.Streams, …
}
```

This file set is the resolver design 01 §3 ALC_PLACEMENT demanded: cargo's `Load` override consults `HostAssemblyTable` — parent-first for bridge-owned names (type identity single per host process), default-ALC for host-owned (FM11's root fix generalized), cargo-first otherwise (per-swap LanguageExt/Thinktecture copies, 07 §1 pitfall 7).
