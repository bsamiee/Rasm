# [07] Code Blueprints — Module-by-Module Skeleton of the C1 Rebuild

Design-wave task D7, 2026-06-11. Inputs: the charter (charter §8 R1-R15 binds this doc), the object model (model §1-§3 — the Contract/Shell half is settled there and NOT restated; this doc supplies the Cargo/Supervisor/SDK half the model's table promised), features §1, tolerance M1-M4, packages §4-§5, mcp §4, the review corpus (bare numbers), and current-tree verification 2026-06-11 (HEAD caf5f75b2). No live-Rhino contact this session: every excerpt is design-validated only; compile and live proofs are enumerated in §10 and gated per the Phase-0 rule (10 §4). Amendments to settled shapes are marked [AMENDED] with both citations and cross-referenced to the red-team attack that forced them (09).

Corpus citation key for this doc and 08-10: bare numbers = review corpus (`../rhino-bridge/`); named keys = design wave (charter=01, mcp=02, features=03, tolerance=04, packages=05, model=06).

## [1]-[REPO_LAYOUT] — 13 source files, every file one owner block

Sibling root during dual-run (10 §3): `tools/rhino-bridge-next/`. Budget: charter §6 ceiling ≤14 files; this layout uses 13.

| [INDEX] | [FILE] | [OWNER BLOCK] | [LOC ENVELOPE] |
| :-----: | ------ | ------------- | :------------: |
| [1] | `Rasm.Bridge.Contract/Wire.cs` | wire vocabulary (model §2.1-§2.4, amended §2 below) | ~420 |
| [2] | `Rasm.Bridge.Contract/Rpc.cs` | RPC + seam interfaces + `BridgeJsonContext` (model §2.5) | ~120 |
| [3] | `Rasm.Bridge.Stub/Stub.cs` | `RasmBridgePlugin` + `file` ShellLoadContext (model §3.1; stub-as-second-tiny-csproj per packages §5 — D-5 ALC hygiene wins over single-project) | ~80 |
| [4] | `Rasm.Bridge.Shell/ShellHost.cs` | composition root + RPC target (model §3.2) | ~350 |
| [5] | `Rasm.Bridge.Shell/IdlePump.cs` | D-1 UI-marshal kernel (model §3.2) | ~120 |
| [6] | `Rasm.Bridge.Shell/CargoGate.cs` | D-3 collectible-ALC owner + `HostAssemblyTable` (model §3.2) | ~250 |
| [7] | `Rasm.Bridge.Cargo/CargoHost.cs` | `IBridgeCargo` impl: discovery, capability probes, scenario bracket (§4.1) | ~400 |
| [8] | `Rasm.Bridge.Cargo/Gh2Lane.cs` | M2 choke point: sole `Grasshopper2.*`/`GrasshopperIO` owner (§4.3) | ~350 |
| [9] | `Rasm.Bridge.Cargo/Spool.cs` | JSONL evidence spool + capture writer (§4.2) | ~150 |
| [10] | `Rasm.Bridge.Supervisor/Program.cs` | `SupervisorVerb` union + `SupervisorRuntime` + terminal collapse (§5.1) | ~200 |
| [11] | `Rasm.Bridge.Supervisor/Session.cs` | `SessionState` union + transition dispatch + `SessionPolicy` + `SessionFold` (§5.2-§5.4) | ~450 |
| [12] | `Rasm.Bridge.Supervisor/HostControl.cs` | `LiveHost`/`BundleInfo`/`Lease`, launch + quit ladder + reconcile + kqueue `HostWatch` kernel (§5.5) | ~400 |
| [13] | `Rasm.Bridge.Supervisor/Evidence.cs` | `Staging` (XxHash3), `.ips` diff (RhinoCrashReportFinder port), spool harvest, gcdump trigger (§5.6) | ~300 |

Scenario SDK: 2 testkit files (`tests/csharp/_testkit/Scenarios/Scenario.cs`, `Scope.cs` — §6), outside the bridge budget per model §1. Scenario corpus: per-lib `*.Scenarios.csproj`, ordinary repo C# 14 (R12). Deletion dividend carried by this layout: the 538-LOC hand-rolled `BridgeWire` framing, the 708-LOC client god-process, `SmokeTemplate` string codegen (F12), `LanguageExtBootstrap` (F11), the `BridgeMarker` stdout grammar, all three Rhino commands (`RasmBridgeStart/Stop/Status`), and the scenario rewrite pipeline (Program.cs:503-529 current tree).

## [2]-[CONTRACT_AMENDMENTS] — four corrections to model §2 (two audit-forced, two completeness-forced)

The Contract type set stays the model's (settled — model §1). Four field-level amendments:

[AMENDED-1] `PhaseStatus` ranks. Model §2.1 gives `Failed`/`Timeout`/`Busy` equal rank 3, making `Worst` order-dependent across DISTINCT exit codes (1 vs 5): a batch folding failed-then-timeout reports exit 1, timeout-then-failed exit 5 (09 attack A4). The fold must be a total order:

```csharp
public static readonly PhaseStatus Ok          = new(key: "ok",          rank: 1, exitCode: 0);
public static readonly PhaseStatus Skipped     = new(key: "skipped",     rank: 1, exitCode: 0);
public static readonly PhaseStatus Unsupported = new(key: "unsupported", rank: 2, exitCode: 3);
public static readonly PhaseStatus Failed      = new(key: "failed",      rank: 3, exitCode: 1);
public static readonly PhaseStatus Timeout     = new(key: "timeout",     rank: 4, exitCode: 5);
public static readonly PhaseStatus Busy        = new(key: "busy",        rank: 5, exitCode: 5);
```

`Timeout` outranks `Failed` (a deadline overrun invalidates later verdicts), `Busy` outranks all (nothing ran) — preserves the current tool's exit precedence (BridgeWire.cs:13-38). Tie law, pinned: the `Ok`=`Skipped` rank-1 tie is deliberate (identical exit code 0); `Worst` keeps the accumulator on rank ties and `SessionFold` seeds with `Ok`, so an all-skipped run reports `ok` at the envelope root while every receipt reads `skipped` — the agent-visible skip signal is the receipts, never the root status (and capability gaps ride `Unsupported`, never `Skipped`). "Strict total order" in 09 A4/10 §2.4 reads precisely as: strict above rank 1, one deliberate rank-1 tie.

[AMENDED-2] `SessionEnvelope` gains `FirstFaultPhase`. Model §2.4 claims field-for-field landing in `VerifySummary`, but `model.py:371-384` (verified current) carries `first_fault_phase`, and the model's envelope excludes phase events from `Evidence` ("phase history lives in receipts + spool") — the consumer's existing contract cannot be derived from the envelope as drawn (09 attack B1). Fold-result field, supervisor-owned, R4-clean:

```csharp
public sealed record SessionEnvelope(
    string RunId, string Verb, PhaseStatus Status, double DurationMs, string ReportDir,
    HostFingerprint Host, CapabilityEntry[] Capabilities, ScenarioReceipt[] Scenarios,
    BridgeEvent[] Evidence, string FirstFailure, SessionPhase? FirstFaultPhase, BridgeFault? Fault);
```

`VerifySummary.first_fault_output` has NO envelope source and gets none: it existed to carry scraped stdout; the no-scrape design deletes the python field (08 §3).

[AMENDED-3] `CargoManifest` gains `SessionId` + `ReportDir`. Model §2.4's manifest carries no session identity and no artifact root, leaving `EventStamp.SessionId` and the in-host writers (§4.2 spool, capture PNG) with no source — nothing in `Handshake`/`LoadCargoAsync`/`RunAsync` tells the host where evidence lands (completeness audit 2026-06-11; the same derivation-gap class as 09 B1). Amended shape: `CargoManifest(Guid SessionId, string ReportDir, string ContentHash, string StagePath, Guid[] HostPlugins, HostFingerprint BuiltAgainst)`. Invocation law: `LoadCargoAsync` runs EVERY session — hash equality short-circuits the swap inside `CargoGate`, never the call (features F2's "skip" is the swap skip, evidenced by `cargo.reused`) — so the manifest is the per-session carrier. The shell stamps `SessionId` together with `Sequence` at publish (the single-in-host-writer pin below); cargo derives `<ReportDir>/<scenario>.jsonl` and `<ReportDir>/<scenario>.png`. Pre-`Loading` emissions need no in-host stamp source: hello-time capability facts ride the `Handshake` reply (model §2.4), and workstation-observed phases are supervisor-stamped at the fold.

[AMENDED-4] The endpoint pipe prefix is ONE named Contract constant, value `rbx-` for the rebuilt tool. Model §2.4's validation partial hard-codes `rb-` while the dual-run law (10 §3; §1 above) mandates a pipe prefix DISTINCT from the old tool's `rb-{pid}-` (Transport.cs:83, current tree) — as written, the rebuilt shell's own `EndpointRecord` admission would reject its own endpoint file at first Phase-1 connect. The validation partial admits the constant, never a literal; the prefix stays `rbx-` after cutover (distinctness from any lingering old-tool artifact is permanent and free; a post-cutover revert is churn with no consumer).

Everything else in model §2 ports unchanged, including the exhibited union wire shape (model §2.3).

Two semantic pins, not shape changes:

1. `EventStamp.Sequence` ownership was unassigned (09 attack B3). Rule: the shell assigns the session-global sequence for every in-host event at publish time (cargo publishes through the shell-owned delegate — model §2.5 — so the shell is the single in-host writer); the supervisor's fold assigns sequence to workstation-side events from a counter seeded above the last relayed value. Ordering law: `(AtUnixMs, Sequence)`.
2. The hello reply carries the StreamJsonRpc assembly version as the fact row `rpc.streamjsonrpc` (closes packages §7 OQ4): minor-version skew across the pipe becomes diagnosable evidence with zero Contract shape change — fact keys are the one sanctioned open string vocabulary (charter §4, R10). The row rides the hello reply's `Capabilities[]`; model §2.4's dedicated `Handshake.RpcVersion` field (annotated "05 OQ5" — a mis-number, packages §7 ends at OQ4) is superseded by this pin and is NOT built (10 §2 amendment 16).

## [3]-[SHELL_ADDENDA] — model §3 stands; two admissions added

Model §3.1-§3.2 are the shell blueprint; not restated. Two rules the audit forced:

1. [BUSY_ADMISSION] (09 attack A3): `ShellHost.HelloAsync` while a session is live answers `Handshake` normally but `LoadCargoAsync`/`RunAsync` from a second connection project `BridgeFault.BusyHeld(holderPid, ageSeconds)`. The supervisor lease is the primary gate; the shell admission is the last line against router-spawned or hand-run clients (mcp §4 rule 2). One generation token in `ShellHost`, no semaphore queueing — second caller fails fast, typed.
2. [ENDPOINT_POISONING] (model §3.1 note, made concrete): a failed `ShellHost.Start` writes the endpoint file with a `fault` field naming the load exception instead of deleting it — `doctor` then discriminates "no process / process-no-pipe / pipe-no-hello / poisoned-start" in one read (F15 fix; features F6).

## [4]-[CARGO_BLUEPRINT] — the half model §1 tabled but never sketched

### [4.1] `CargoHost.cs` — activation entry, discovery, probes, scenario bracket

```csharp
// Ownership: the IBridgeCargo implementation activated by CargoGate inside the collectible ALC.
// Full repo doctrine applies here (Fin/Eff legal — cargo-private LanguageExt per D-3b).
public sealed class CargoHost : IBridgeCargo {
    public ScenarioEntry[] Discover();
    // In-host post-load enumeration ONLY (08 §4[5]): assembly scan for [RhinoScenario] static
    // methods over the staged scenario assemblies; entry = (theme, name, requires, budgetMs)
    // read from the attribute. No out-of-host type loading exists anywhere (R12).

    public CapabilityEntry[] Probe(Action<BridgeEvent> publish);
    // M1 rows: HostCapability [SmartEnum] — each row carries its probe as a constructor
    // delegate (gh2.dataflow = trivial 3-component Start(Headless) solve + Tree() read under
    // deadline, OFF the pipe thread, awaited via IdlePump-frame polling — 0b thread law, §10;
    // ghz.diff = 2-node archive round-trip; exception.tap = subscription took; eventpipe,
    // cargo.hotswap = recorded Phase-0 verdicts). Probes run inside the same spool + heartbeat
    // as scenarios so a crashing probe is attributed to the probe (tolerance M1).

    public ScenarioReceipt Run(ScenarioEntry scenario, Action<BridgeEvent> publish);
    // The IO bracket: acquire ScenarioContext -> invoke entrypoint -> release. Finally rung
    // flushes the spool footer (port obligation 7 — belt; the per-line spool is structural
    // crash-durability). Requires-gating: unmet capability keys -> PhaseStatus.Unsupported with
    // the probe receipt attached (tolerance M4 lattice), entrypoint never invoked.
    // Host-drift floor: MissingMethodException/TypeLoadException at invoke -> BridgeFault
    // .HostDrift carrying member name + both fingerprints (tolerance M4(d)).

    public void Dispose();
    // D-2 unload PREcondition: drains undisposed DocumentScopes (leak = a named fact, then
    // forced dispose), detaches host-event subscriptions (symmetric detachers), disposes
    // Gh2Lane state. CargoGate.Unload() runs only after this returns.
}
```

`HostCapability` lives in this file (owner-local). `[ThreadStatic]`/static caches are banned in cargo except inside `Gh2Lane` with disposal in `Dispose` — every static is an ALC pin candidate (07 §1 pitfall 3).

### [4.2] `Spool.cs` — crash-durable evidence + capture writer

```csharp
// Ownership: the per-scenario JSONL spool at <ReportDir>/<scenario>.jsonl (ReportDir arrives in
// CargoManifest, [AMENDED-3] §2; append-per-event, FileStream + WriteThrough on the
// event, no buffering layer — crash-durability is the point, 08 §3) and the PNG capture writer.
internal sealed class Spool : IDisposable {
    internal void Append(BridgeEvent evt);              // one JSONL line, BridgeJsonContext encoding —
                                                        // the SAME bytes the RPC notification carries (R4)
    internal BridgeEvent.CaptureCase Capture(RhinoView view, string scenario, bool onFailure);
    // ViewCapture on the current idle frame -> <reportDir>/<scenario>.png; emits the capture
    // event PLUS the metadata facts (camera, bounds, visible-object count) adopted from the
    // RhinoMCP capture-metadata convention (mcp §3.2.3 crib) so an empty capture is diagnosable
    // without re-shooting.
}
```

Capture triggers (features F3, OQ4 resolved INCLUSIVE): auto-on-failure for any scenario holding a live `DocumentScope` with a realized viewport; auto-on-failure for GH2 canvas scenarios via `Gh2Lane.DrawToBitmap` (one capture-cost measurement lands in Phase 3 — if >250 ms it becomes a policy row, not a cut); explicit `Capture.Snapshot(label)` for green-path evidence. Captures are EVIDENCE, never oracles (features cut 10).

### [4.3] `Gh2Lane.cs` — the M2 choke point

```csharp
// Ownership: the ONLY file in the rebuilt tool that names Grasshopper2.* / GrasshopperIO types
// (tolerance M2). Scenarios speak this vocabulary — solve receipts, tree projections, archive
// diffs as typed values — never raw GH2 object graphs (09 attack E2 reconciliation: charter
// D-4 lifts the host-law prohibition on naming GH2 types; M2's churn budget still routes all
// GH2 touches through this owner; a scenario naming a raw GH2 type is a lint, not a crash).
internal sealed class Gh2Lane : IDisposable {
    internal static Fin<Gh2Lane> Acquire(Action<BridgeEvent> publish);  // headless GhEditor via
        // ctor, NEVER Show() — the StatusBar-SIGABRT root fix (reference_gh2_editor_statusbar_crash:
        // ShowEditor always Form.Show()s; ctor builds Canvas without AppKit DrawRect) — port verbatim.
    internal Fin<SolveReceipt> Solve(Gh2Doc doc, TimeSpan deadline);    // lane 2, gated on the
        // gh2.dataflow capability row (probe 0b); Start(SolutionMode.Headless) -> Task<Solution>
        // awaited off the UI thread; Tree() projections -> typed value facts.
    internal Fin<CaptureFile> DrawCanvas(Gh2Doc doc);                   // lane 1 render evidence.
    internal Fin<ArchiveDiff> Diff(string ghzA, string ghzB);           // lane 3 (post-cutover).
}
```

GH2 preload stays shell-side (`PlugIn.LoadPlugIn` by GUID, default ALC — port obligation 4); the GUID arrives in `CargoManifest.HostPlugins`, derived from the scenario closure manifest (§5.6) — one truth, replacing the MSBuild-prop-OR-dll-sniff dual heuristic (01 F17).

## [5]-[SUPERVISOR_BLUEPRINT]

### [5.1] `Program.cs` — verb union, runtime, terminal collapse

```csharp
// Ownership: argv -> SupervisorVerb [Union] (ports the ClientVerb extension-dispatch shape,
// 01 §1) -> Eff<SupervisorRuntime, SessionEnvelope> -> ONE terminal collapse at Main: envelope
// JSON to stdout via BridgeJsonContext, exit code from PhaseStatus.ExitCode. structlog-style
// progress to stderr; stdout carries exactly one JSON document (the assay decode contract).
[Union]
public abstract partial record SupervisorVerb {
    public sealed record Verify(ScenarioSelection Selection, string ClosureManifest) : SupervisorVerb;
    public sealed record Doctor : SupervisorVerb;
    public sealed record Redeploy(string PackagePath) : SupervisorVerb;
    public sealed record Quit : SupervisorVerb;
}

public sealed record SupervisorRuntime(
    Atom<Option<LeaseToken>> Lease, TimeProvider Clock, CancellationToken Root,
    SessionPolicy Policy, string ArtifactRoot, BundleInfo Bundle);
// Constructed once at the composition edge; Schedule values come from Policy (R7).
```

`verify` is modality-polymorphic on input shape (features §0): assay passes a `ScenarioSelection` union case + the staged closure manifest path; no flags, no modes.

### [5.2] `SessionState` — the lifecycle state machine, real definition

```csharp
// Ownership: the session has ONE owner (R5). Cases carry their evidence; no boolean phase
// flags, no nullable payload bags. Transitions are one total state-threaded Switch (§5.3).
[Union]
public abstract partial record SessionState {
    private SessionState() { }
    public sealed record Idle(BundleInfo Bundle) : SessionState;
    public sealed record Reconciling(BundleInfo Bundle, Seq<string> MarkersCleared) : SessionState;
    public sealed record Launching(BundleInfo Bundle, long LaunchedAtMs) : SessionState;
    public sealed record Connecting(LiveHost Host, int PollsRemaining) : SessionState;
    public sealed record Negotiating(LiveHost Host, Handshake Ours) : SessionState;
    public sealed record Loading(LiveHost Host, Handshake Peer, CargoManifest Manifest) : SessionState;
    public sealed record Running(LiveHost Host, CargoReceipt Cargo, Seq<ScenarioReceipt> Done,
                                 Seq<ScenarioEntry> Remaining, int RestartBudget) : SessionState;
    public sealed record Quitting(LiveHost Host, SessionPhase Rung, long RungStartedMs) : SessionState;
    public sealed record Faulted(BridgeFault Fault, SessionPhase At, Seq<ScenarioReceipt> Done) : SessionState;
    public sealed record Terminal(SessionEnvelope Envelope) : SessionState;
}
```

Lease token and restart budget live INSIDE states (`SupervisorRuntime.Lease` holds the token cell; `Running.RestartBudget` carries the policy countdown — default 1 relaunch then remaining scenarios fold `Skipped` with the crash cited; features OQ2 adopted as the default policy row).

### [5.3] Transition dispatch + external events

```csharp
// Ownership: the closed signal vocabulary the watchers produce (supervisor-private; never wire).
[Union]
public abstract partial record SessionSignal {
    private SessionSignal() { }
    public sealed record HostExited(int Pid, long AtUnixMs) : SessionSignal;             // kqueue NOTE_EXIT (§5.5)
    public sealed record HeartbeatSilent(TimeSpan SilentFor) : SessionSignal;
    public sealed record ShutdownStarted(long AtUnixMs) : SessionSignal;                 // AE-quit notification observed
    public sealed record RpcCompleted(SessionPhase Phase, PhaseStatus Status) : SessionSignal;
    public sealed record DeadlineHit(SessionPhase Phase, TimeSpan Elapsed) : SessionSignal;
}

internal static class SessionDispatch {
    internal static SessionState Apply(SessionState state, SessionSignal signal, SessionPolicy policy);
    // ONE state-threaded Switch (≥3 arms share context — the state-parameter overload per
    // feedback_polymorphic_state_threaded_collapse); total over state × signal.
}
```

Stuck-state law (09 attack A1-A3): EVERY non-terminal state has a deadline row in `SessionPolicy` and a `DeadlineHit` transition to `Faulted` — there is no state an external event cannot exit, and `Faulted` always reaches `Terminal` (envelope emission cannot be skipped; it is the fold of whatever evidence exists).

Discrimination matrix (features F4, made structural):

| [SIGNAL] | [IN Connecting] | [IN Running] | [IN Quitting] |
| -------- | --------------- | ------------ | ------------- |
| `HostExited` | `LaunchFailed` (+`.ips` diff) | `RhinoCrash` attributed to in-flight scenario, spool harvested, restart budget consulted | rung confirmed — CLEAN |
| `HeartbeatSilent` | `DialogSuspected(silentForMs)` | `UiWedged(silentForMs, scenario)` | escalate to next rung |
| `DeadlineHit` | `ConnectFailed(elapsed)` | `ExecuteDeadline(scenario, elapsed)` | escalate to next rung |
| `ShutdownStarted` | — | AE-quit in progress, not a hang — transition to `Quitting` | expected |

### [5.4] `SessionPolicy` + `SessionFold`

```csharp
// Ownership: EVERY duration, cadence, budget, and retention number in the tool (R7). One
// authoritative deadline; shell and python derive. Values are rows, not scattered literals.
public sealed record SessionPolicy(
    Schedule Connect,          // spaced(250ms) | upto(90s) — replaces the hand poll (01 F5)
    Schedule QuitLadder,       // ONE composed Schedule: ae rung -> force rung -> kill rung (§5.5);
                               // rung identity derives from SessionPhase rows, ladder order is law
                               // (DECIDED — closes the one-Schedule-vs-three-rows question)
    TimeSpan SessionDeadline, TimeSpan ScenarioDefaultBudget, TimeSpan HeartbeatWindow,
    int RestartBudget, TimeSpan FailureRetention, bool PruneGreenRuns);
    // defaults: 1, 7 days, true (features OQ2/OQ3 adopted; operator-tunable rows)

// Ownership: the terminal fold — SessionEnvelope fields are fold RESULTS over the event
// stream + receipts (R4): Status = Worst over receipts and phase events; FirstFailure +
// FirstFaultPhase = first-non-ok projection in wire order; Evidence = fact/capture cases
// (phase/progress cases live in receipts + spool — envelope stays bounded, 09 attack B2);
// spool reconciliation: at fold time the supervisor compares relayed event count/sequence
// against the spool tail and emits a `evidence.divergence` fact on mismatch (09 attack B4).
internal static class SessionFold {
    internal static SessionEnvelope Run(string runId, SupervisorVerb verb, SessionState final,
        Seq<BridgeEvent> stream, (long Count, long LastSequence) spoolTail, SessionPolicy policy);
    internal static PhaseStatus Worst(PhaseStatus left, PhaseStatus right);
    // total order §2 [AMENDED-1]; keeps the accumulator on rank ties; SessionFold seeds Ok.
}
```

### [5.5] `HostControl.cs` — launch, quit ladder, reconcile, watch, lease

```csharp
public sealed record BundleInfo(string AppPath, string CFBundleName, string CFBundleExecutable,
                                string CFBundleVersion);
// Discovery by bundle metadata (newest Rhino*.app by CFBundleVersion, restored per 02 §6.4);
// marker names DERIVE from CFBundleName/CFBundleExecutable — no "RhinoWIP" literal anywhere
// (tolerance §1 GA hazard). RHINO_WIP_APP_PATH narrows discovery; it is not required.

public sealed record LiveHost(int Pid, long StartedAtUnixMs, EndpointRecord Endpoint,
                              HostFingerprint Fingerprint);

public sealed record LeaseToken(int HolderPid, long AcquiredAtUnixMs, string Path);
// The SupervisorRuntime.Lease cell payload (§5.1); token-gated singleton ownership pattern.

internal static class QuitLadder {
    internal static Eff<SupervisorRuntime, PhaseStatus> Run(LiveHost host, Action<BridgeEvent> publish);
    // PORT VERBATIM, rungs DEFINED (09 attack D1): quit.ae = Apple Event terminate
    // (NSRunningApplication.terminate via osascript JXA — the only clean exit);
    // quit.force = NSRunningApplication.forceTerminate (Cocoa-mediated SIGKILL, requires the
    // live NSRunningApplication handle); quit.kill = kill(2) SIGKILL by PID (handle-free floor).
    // SIGTERM IS BANNED AT EVERY RUNG: RhinoApp.Exit(false) and SIGTERM both self-SIGABRT via
    // RhMacSignalHandler and manufacture the very crash markers reconcile exists to clear
    // (empirically corrected 2026-05-29). Each rung emits a typed PhaseCase; closed:false FAILS
    // its rung (kills swallow points 13a-13e, 02 §4). Schedule-sequenced via Policy.QuitLadder.
}

internal static class Reconcile {
    internal static Eff<SupervisorRuntime, Seq<BridgeEvent>> Run(BundleInfo bundle);
    // Pre-launch placement (port obligation 2: macOS writes .ips ASYNC after kill — only the
    // before-launch clear beats the race). INSTANCE-SCOPED (mcp §4 rule 3 + 09 attack D2): the
    // supervisor persists a quit journal (~/.rasm/rhino-bridge-quits.jsonl — retired endpoint
    // records + kill timestamps); reconcile matches autosave/.rhl/.ips markers against
    // SUPERVISED instances' [launch, kill] windows only. A human-operated Rhino 8's recovery
    // state is structurally untouchable. Markers found/cleared/SKIPPED-as-foreign are evidence
    // events; deletion stays a boundary IO effect.
}

internal sealed class HostWatch : IDisposable {
    internal static Fin<HostWatch> Attach(int pid, Action<SessionSignal> raise);
    // D-7 kernel: ~50-100 LOC kqueue/kevent P/Invoke, NOTE_EXIT on the host PID -> HostExited;
    // 250 ms PID polling via Schedule is the swap-in fallback, the degradation itself a fact.
}

internal static class Lease {
    internal static Fin<LeaseToken> Acquire(string artifactRoot, TimeProvider clock);
    internal static Unit Release(LeaseToken token);
    // At the resource (charter §4 SESSIONS): O_EXCL lease file under ~/.rasm/ + token-gated
    // Atom cell; busy -> typed exit 5 naming holder PID + age. STALENESS ADMISSION (09 attack
    // A2): a lease whose holder PID is dead (same start-time validation as EndpointRecord) is
    // reclaimed with a `lease.reclaimed` fact — a crashed supervisor cannot wedge the next run.
}
```

Launch wires the coexistence rules at the call site: `open -a <bundle> --env RHINO_MCP_AUTOSTART_PORT=0 --args -nosplash` (mcp §4 rule 1 — re-verify the variable per installed rhinomcp release; fallback policy: no `Rhino-MCP-Platform` yak on the verification profile). The supervisor never binds TCP (rule 4).

### [5.6] `Evidence.cs` — staging, forensics, harvest

```csharp
// Ownership: every workstation-side evidence producer outside the session fold — one owner block.
internal static class Evidence {
    internal static Fin<CargoManifest> Stage(string closureManifest, Guid sessionId,
                                             string reportDir, string refsRoot);
    internal static Seq<string> IpsBaseline(BundleInfo bundle);                  // pre-launch snapshot
    internal static Option<CrashSummary> IpsDiff(Seq<string> baseline, BundleInfo bundle);
    internal static Seq<BridgeEvent> HarvestSpool(string reportDir, string scenario);
    internal static Option<string> GcDump(int pid, string reportDir);            // artifact path or None
}
internal sealed record CrashSummary(string Thread, string ExceptionType, string ReportPath);
// Supervisor-private parse result feeding the RhinoCrash fault case; unparseable reports
// degrade to raw paths + a `forensics-degraded` fact, never a throw.
```

- `Stage`: content-hash (`XxHash3`, packages §3.5) closure staging into `refs/<hash>/` per session. Closure SOURCE [AMENDED vs implicit]: a build-time MSBuild target in the scenario projects' `Directory.Build.props` emits `bridge-closure.json` (assembly list + host-plugin GUIDs + `HostFingerprint` built-against) into the output dir; the supervisor READS it — zero MSBuild evaluation, zero `dotnet build` children at invoke time (09 attack C1; kills the N×45 s `-getProperty` class, Build.cs:99-109 current tree). `deps.json` is the fallback source if the target proves redundant (§10 proof 6).
- `.ips` diff: snapshot pre-launch, diff on `HostExited`; parse via a port of RhinoMCP's MIT `RhinoCrashReportFinder` (mcp §3.3 crib — field-tested, do not rebuild); unparseable reports attach as raw paths with a `forensics-degraded` fact. Markers found at reconcile (pre-session crashes, no watcher alive between invocations) surface as evidence events, never as misattributed session faults (09 attack D3).
- Spool harvest: on crash, read the per-scenario JSONL tail, attach facts-to-point-of-death to the crash-attributed receipt.
- gcdump: on `unloadConfirmed=false`, trigger `dotnet-gcdump` against the host PID (packages §3.7), attach the artifact path inside `CargoUnloadLeak`.

## [6]-[SCENARIO_SDK] — testkit, 2 files

```csharp
// Scenario.cs — declaration + context + fused assert/fact (features F3)
[AttributeUsage(AttributeTargets.Method)]
public sealed class RhinoScenarioAttribute(string theme) : Attribute {
    public string[] Requires { get; init; } = [];   // capability keys -> M4 run/skip lattice
    public int BudgetMs { get; init; }              // declaration-time budget (features cut 14)
}
public sealed class ScenarioContext {               // the entrypoint's single parameter
    public RhinoDoc Doc { get; }
    public Fin<Unit> Require(string label, bool observed);        // assert + fact, ONE call
    public Fin<T> Expect<T>(string label, Fin<T> projection);     // auto-facts the projection
    public void Fact(string key, object value);                   // evidence without assertion
}

// Scope.cs — resource capsules (D-2; re-houses the repo capsule patterns, never reinvents)
public sealed class DocumentScope : IDisposable {
    public static Fin<DocumentScope> Open(ScenarioContext ctx);   // doc create/teardown capsule
    public RhinoDoc Doc { get; }
    public bool ViewportRealized { get; }                          // feeds auto-capture-on-failure (§4.2)
}
public static class Capture { public static Fin<string> Snapshot(string label); }
```

Entrypoint shape: `static Fin<Unit> <Name>(ScenarioContext ctx)`. A scenario emitting zero facts gets the `facts.empty` warning fact (features F3). Shared fixtures live in the testkit module, not per-file builders (charter §6).

Pre-host enumeration is NOT built (closes features OQ1): in-host post-load discovery is the only enumeration lane; a `verify --list` revival is demand-gated on a named consumer needing a pre-host scenario manifest, and its shape is fixed in advance — the `MetadataLoadContext` attribute-name-only spike (no type loading, anti-pattern-safe, ~1 hour). The same gate owns the per-lib scenario README decision (11 §3.5: omitted — the attribute is the doc).

## [7]-[AGENT_DIAGNOSTIC_SURFACE] — what an agent sees on every failure class, automatically

The envelope is the UI (features cut 17). Every row below is produced with ZERO flags, ZERO re-runs — the evidence is attached by the supervisor before the envelope is written. Exit codes derive from `PhaseStatus` (amended ranks §2).

| [INDEX] | [FAILURE CLASS] | [STATUS/EXIT] | [IN THE ENVELOPE, AUTOMATICALLY] | [PRESCRIPTION FIELD SAYS] |
| :-----: | --------------- | :-----------: | -------------------------------- | ------------------------- |
| [1] | scenario assertion failure | failed/1 | failing `Require` label + observed value as facts; auto-capture PNG; PDB-real stack frame (repo-compiled); receipt names file-owning scenario | the failing label IS the pointer — edit the named site |
| [2] | `launch-failed` | failed/1 | ONE batch-level fault (never N per-scenario timeouts); launch stderr; bundle + env rows from micro-doctor | named missing precondition |
| [3] | `connect-failed` | failed/1 | elapsed; endpoint-file state discrimination (no process / process-no-pipe / pipe-no-hello / poisoned-start §3.2) | per discriminated state |
| [4] | `busy-held` | busy/5 | holder PID + lease age; stale leases auto-reclaimed (§5.5) so this is always a LIVE holder | wait or `quit` that session |
| [5] | `shell-skew` | failed/1 | both contract versions from hello | `run redeploy` |
| [6] | `host-drift` | failed/1 | missing member name + built-against and running fingerprints; auto-rebuild receipt if self-heal ran (M4) | rerun verify (rebuild is automatic) |
| [7] | `cargo-unload-leak` | failed/1 | gcdump artifact path naming the rooting chain; `UnloadReceipt` (gcRetries, debugger gate); session auto-fell-back to host recycle | inspect gcdump; session already recovered |
| [8] | `rhino-crash` | failed/1 | crash thread + exception type from `.ips` parse; faulting scenario NAMED (kqueue attribution); facts-to-point-of-death from spool harvest; restart-budget disposition per remaining scenario | crash cause + scenario, one read |
| [9] | `dialog-suspected` | failed/1 | silent-for duration; marker-reconcile journal (what was cleared, what was skipped as foreign) | modal dialog on host — the one class needing eyes |
| [10] | `ui-wedged` | timeout/5 | silent-for + in-flight scenario; spool facts to wedge point | scenario named; host recycled per budget |
| [11] | `execute-deadline` | timeout/5 | scenario, elapsed, declared budget; facts to deadline | raise `BudgetMs` at declaration or fix the hang |
| [12] | `nuget-lock-drift` | failed/1 | NU code + project; bridge NEVER mutates lockfiles | `dotnet restore --force-evaluate` via static rail |
| [13] | `capability-absent` | unsupported/3 | capability key + probe receipt; affected scenarios reported `unsupported`, run stays green-readable | lane unavailable on THIS host — not a bug |
| [14] | `redeploy-incomplete` | failed/1 | failing doctor check post-relaunch; per-rung quit receipts; yak output | named check to chase |
| [15] | scenario compile failure | (never reaches supervisor) | ordinary `static build` diagnostics at repo C# 14 with analyzers — the dialect is dead (FM5 [ELIM]) | fix like any repo code |
| [16] | host exception report (green scenario) | failed/1 | `HostExceptionCase` event with the swallowed report; status fold flips the verdict (port obligation 5) | the swallow is surfaced, not lost |

## [8]-[CAPTURE_PIPELINE] — end to end

1. TRIGGER (§4.2): auto-on-failure (DocumentScope-realized viewport, or GH2 canvas via `Gh2Lane.DrawCanvas`) + explicit `Capture.Snapshot`.
2. SHOOT: `ViewCapture` on the current idle frame (no new frame scheduling — the failure frame is the evidence); PNG to `<reportDir>/<scenario>.png` at viewport resolution (full-resolution is bridge-owned territory — MCP's 1280×720 JPG lane is deliberately the other lane, mcp matrix row B3).
3. RECORD: `CaptureCase(path, w, h, onFailure)` event + camera/bounds/object-count metadata facts (rhinomcp convention, adopted).
4. CARRY: event rides notification + spool + envelope (three folds of one record, R4).
5. REGISTER: the assay rail registers PNGs as artifacts alongside JSON (closing the verified gap: `_scenario_artifacts` globs `*.json` only, bridge.py:468-475, so PNGs are invisible and TTL-swept today — 08 §2).
6. RETAIN: tiered policy rows — failure runs 7 days, green runs pruned next run (replaces the 300 s TTL, bridge.py:48,364-388).

## [9]-[MCP_SEAM] — the complement boundary, wired now, shipped later

Verdict unchanged and externally re-corroborated 2026-06-11 (mcp §5; rhinomcp 0.1.5 release notes are stability-only; Anthropic's 2026-04-28 nine-connector "Claude for Creative Work" wave makes the conversational-modeling lane a vendor commodity — never build it). What the blueprint commits NOW:

- Coexistence is wired at three named sites: launch env suppression (§5.5), instance-scoped reconcile journal (§5.5), and a `doctor` row `mcp.listener` that fails with a prescription if a rhinomcp listener is bound in the verification host (their plugin auto-listens per document — the unilateral hazard, mcp §3.3).
- The seam IS `SessionEnvelope` + the four verbs. The deferred phase-5+ facade is a supervisor-side stdio MCP server (official `ModelContextProtocol` SDK 1.4.0, spec 2025-06-18; reflection-based `WithToolsFromAssembly` is acceptable — the supervisor is not AOT) exposing OUR verbs only, zero new capability:

| [OUR VERB] | [MCP TOOL] | [RESULT MAPPING] |
| ---------- | ---------- | ---------------- |
| `verify` | `rasm_verify` | `structuredContent` = `SessionEnvelope` validated by an `outputSchema` generated from `BridgeJsonContext`; captures as `resource_link` entries |
| `doctor` | `rasm_doctor` | check rows as `structuredContent`; `isError` only on transport failure |
| `redeploy` / `quit` | `rasm_redeploy` / `rasm_quit` | envelope as above |

The typed rail maps NATIVELY onto `structuredContent`/`outputSchema` — the seam costs zero Contract change because the envelope already is the document (mcp matrix EITHER-6). Demand gate unchanged: ships only when a shell-less agent becomes a real consumer.

## [10]-[PHASE1_PROOFS] — compile/live items this doc asserts but cannot prove offline

| [#] | [PROOF] | [FALLBACK] |
| :-: | ------- | ---------- |
| 1 | `[JsonRpcContract]` source-gen proxies compile under net10/C#14 + repo analyzers (packages OQ2) | runtime `JsonRpc.Attach<T>` — same interfaces, zero wiring change |
| 2 | `[Union]` + `[JsonDerivedType]` + `required EventStamp` base property round-trips under `SystemTextJsonFormatter`; unknown-`$type` failure DIRECTION matches the model §2.6 rule-3 gate | explicit converter on `BridgeEvent` only |
| 3 | Thinktecture `.Json` converters (smart enums, value objects) compose with `BridgeJsonContext` source-gen | manual `JsonConverter` registration in context options |
| 4 | STJ unmapped-member skip DECLARED in context options + StreamJsonRpc `-32601` → `RemoteMethodNotFoundException` mapping (tolerance hook 4) | — (must hold; M3 depends on it) |
| 5 | GH2 dataflow probe runs `Start(Headless)` wait OFF the UI thread without deadlock — probe 0b restated WITH thread context pinned: the prior live "never settles" result (reference_gh2_headless_solution_limits: bridge scenarios ran ON the UI thread, solver idle-driven, StartWait deadlocked) and the decompile (06 §2: solve runs in Task.Run on threadpool) are BOTH true in their thread regimes; an unpinned probe reproduces the deadlock and falsely confirms the ceiling | render-only GH2 lane (features F7 lane 1) |
| 6 | `bridge-closure.json` MSBuild target emits the full closure incl. `Private=false` host refs; or `deps.json` alone suffices | keep the target (it is ~15 lines of props) |
| 7 | `RHINO_MCP_AUTOSTART_PORT` suppression behavior per installed rhinomcp release | profile policy: no `Rhino-MCP-Platform` yak on verification machines |
| 8 | Capture cost on the failure idle frame (GH2 canvas + viewport) under ~250 ms | demote GH2-canvas auto-capture to explicit |
| 9 | Stub two-assembly publish layout under dual-run yak install (distinct GUID, `rbx-` pipe, endpoint file — mechanics settled 10 §3); one Phase-1 dry run | collapse stub to a single-csproj shape (packages §5 weighed both; D-5 hygiene preference, not a dependency) |
| 10 | `PlugIn.LoadPlugIn` failure mode for an absent/repackaged GH2 — return-false vs throw (tolerance §6 hook 1) | none needed — both outcomes map to the same `gh2.*` capability rows reading absent |
| 11 | GA marker naming: the `CFBundleName`-derivation rule reproduces on a Rhino 9 GA candidate bundle (tolerance §6 hook 2) | marker-name override as a `SessionPolicy`-adjacent policy row; derivation stays the default |
| 12 | Capability probe wall cost ≤~1 s (tolerance §6 hook 3) | key the probe result cache by `HostFingerprint` — a cache row, zero design impact |
| 13 | Deploy-set: ship the FULL restored closure; one live session with `AssemblyLoadContext.Default.Resolving` logging decides whether the 5-assembly trim is worth taking (packages §7 OQ1 — full closure is correct-by-construction, trim is optimization only) | keep the full closure permanently |

Former open questions, all closed at finalization (register: 10 §7): `verify --list` enumeration → demand-gated NOT-built (§6); `QuitLadder` one-composed-`Schedule` → decided (§5.4); stub publish layout → proof 9. Tolerance §6 hook 4 IS proof 4; packages §7 OQ2/OQ3 are proof 1 and probe 0c; packages §7 OQ4 is closed by §2 pin 2.
