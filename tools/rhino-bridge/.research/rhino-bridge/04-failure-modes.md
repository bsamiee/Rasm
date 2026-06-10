# [04] Failure-Mode Dossier — tools/rhino-bridge (empirical)

Evidence base: `git log` on `tools/rhino-bridge` (59 commits, 2026-05-16 to 2026-06-08); surviving verify artifacts under `.artifacts/rhino/verify/` (6 runs, all 2026-06-02); `tools/quality/rails/bridge.py` + `tools/quality/rails/package.py`; `tools/rhino-bridge/client/Program.cs`, `plugin/Rhino/{Host,Transport,CodeEngine}.cs`, `protocol/BridgeWire.cs`; repo memory corpus (crash markers, StatusBar crash, headless solver limits, csx constraints). No live-host interaction was performed.

## [1] Fix cadence (frequency evidence)

59 commits touch the bridge in 24 days. Burst days: 2026-05-16 (4), 2026-05-17 (7), 2026-05-24 (5), 2026-05-26 (6), 2026-05-28 (3). The tool was architecturally reworked twice inside that window (probe-runtime redesign 2026-05-16; `InvokeAndWait` polling to Idle-queue dispatch 2026-05-26) and hardened in at least five dedicated commits. Classification of the substantive bridge commits:

| Date | Commit | Class | Content |
| --- | --- | --- | --- |
| 05-16/17 | `cf1752caa`..`fce3bf393` (11) | redesign + hot-fix churn | probe runtime redesign, "Fix Rhino bridge truth checks", diagnostics refinement, check-flow cleanup — shape-finding |
| 05-24 | `f24890e4c`, `59284a069` | deploy friction | Yak packaging refined then simplified within one day |
| 05-26 | `e7ee7936a` | redesign #2 | polling -> `ConcurrentQueue<IdleJob>` + `RhinoApp.Idle` dispatch; ALC table collapse |
| 05-26 | `31c4e99f0` | fix + surface trim | scenario rail migration; fixes a scenario referencing an undefined `Evidence` type that had been green |
| 05-26 | `9794091b6`, `1781241b5`, `e12898a96` | hardening | deploy/dependency boundary; command-window capture; `ScenarioHostUsings` compile surface |
| 05-28 | `1feeae6bb` | hardening + gap fixes | drop version-fragile rhinocode CLI phase; drop blind retry-once; scenario-kit bin gap; ALC resolver gap |
| 05-28 | `75d740adc` | lifecycle hardening | force-quit ladder, recovery-marker clearing, GH2 preload (UI scenarios 1/8 -> 6/8), timeout consolidation |
| 05-29 | landed via `60893160b`/`49b20ecd3`/`eecbfa18d` | empirical correction | termination ladder REVERSED (prior "SIGTERM clean / Exit(false) clean" claim shipped wrong); StatusBar headless-show fix; liveness probe; `_verify_discover` glob bug fix |
| 05-31 | `cde11be18` | diagnostics thinness fix | "surface bridge verify first-failure evidence" |
| 06-07 | `1b2d070bd` | regression fix | assay-parity port broke the `verify` pattern positional; restored |

Pattern: roughly one hardening or corrective commit every other day across the tool's whole life, with two full redesigns in week one and corrections-of-corrections (termination ladder) in week two.

## [2] Failure catalog

### FM1 — Programmatic exit self-SIGABRTs (forced-quit on clean-exit attempts)

- Trigger: `RhinoApp.Exit(false)` from the plugin Idle frame drives native `CRhinoApp_Exit` into `abort()`; `SIGTERM` hits `RhMacSignalHandler` which also `abort()`s. Both write `Rhinoceros-*.ips`.
- Blast radius: every exit attempt that uses the "obvious" APIs produces a crash report, which arms FM2 on the next launch. The team shipped a wrong ladder first (SIGTERM-then-SIGKILL, `75d740adc`) and corrected it empirically on 2026-05-29.
- Current mitigation: server `Quit` only marks docs clean (`Transport.cs:337-343`); client `ForceCloseAsync` (`Program.cs:242-249`) sends a PID-targeted Cocoa Apple-Event terminate via `osascript` JXA, waits `QuitWait` (30s), then SIGKILL. [ROOT-FIX] for the terminate path (it is the only clean macOS route); [BAND-AID] for the SIGKILL fallback, which deliberately re-creates the unclean state FM2 must then clean.
- Frequency: every quit/relaunch cycle exercises this; the SIGKILL branch fires whenever Rhino is wedged (FM7).

### FM2 — Crash-recovery dialog / autosave markers wedge the next launch

- Trigger: any unclean exit (SIGKILL fallback, SIGABRT, host crash) leaves `~/Library/Autosave Information/Unsaved RhinoWIP Document.3dm` + `.rhl` lock and `Rhinoceros-*.ips`; macOS writes the `.ips` asynchronously, so a clear executed immediately after a kill can lose the race. No macOS launch argument suppresses the dialog.
- Blast radius: next cold launch blocks on a modal "did not shut down correctly" dialog; the verify rail then burns the full 90s connect budget and fails the whole batch at `connect`.
- Current mitigation: `ClearRecoveryMarkers()` (`Program.cs:260-273`) deletes all three marker classes inside the idempotent `ReconcileAsync`, which runs BEFORE every cold launch (the reliable backstop against the async `.ips` race) and after every forced close. [BAND-AID] — deletes another application's recovery state out from under it; correct only as long as McNeel's marker scheme is stable and the bridge Rhino is the only Rhino whose state should be discarded.
- Frequency: structural — every unclean exit; the before-launch placement exists precisely because the after-kill clear was observed to race.

### FM3 — Package/plugin update forces a full quit -> install -> relaunch cycle

- Trigger: any new bridge plugin build or Rasm host package: `package.py` deploy policy is literally `steps=("quit", "install", "refresh")` (`rails/package.py:135`), where refresh = cold launch + doctor (`bridge.py:322-326`). The cause is structural: the `.rhp` loads `AtStartup` into the default ALC (`Host.cs:14`) and .NET cannot unload it.
- Blast radius: 30-90s+ per update; each cycle traverses FM1 and FM2; a failure mid-cycle (install error, launch wedge) produces exactly the operator-reported symptoms — forced-quit SIGABRT, crash-recovery dialog, and plugin-load error windows that must be manually dismissed before the host answers.
- Current mitigation: `client_quit` treats no-endpoint as ok; `client_refresh` proves the new plugin with a doctor round trip. The scenario-side equivalent problem is root-fixed (shadow-copied refs + `CachePolicy.NeverCache`, `Program.cs:585-611`), so user-code iteration does NOT need relaunch — only plugin/protocol changes do. [BAND-AID] at the plugin layer: nothing reduces the relaunch itself; the design response so far has been to keep the plugin surface frozen at 4 verbs so it rarely changes.
- Frequency: every plugin version bump; two Yak commits in one day (2026-05-24) and `9794091b6` are deploy-friction fixes.

### FM4 — Deferred host crash after a green execute (GH2 StatusBar family)

- Trigger: a scenario realizes the GH2 editor window under the bridge's programmatic GH2 load; un-warmed icon resources NPE inside an AppKit `DrawRect` trampoline ~6-8s AFTER `execute` returned ok; managed exception cannot cross the native callback -> SIGABRT.
- Blast radius: the crashing scenario was recorded `ok`; the host died; the NEXT scenario failed at `connect` — a misattributed cascade. Invisible for weeks because a `_verify_discover` glob bug prevented batch runs from ever sequencing two scenarios on one host.
- Current mitigation: (a) [ROOT-FIX] never `Show()` the editor headless — construct `GhEditor` directly, fixed at the source in `GrasshopperUi.Scope.Resolve`; (b) [BAND-AID, deliberate sentinel] `LivenessPhaseAsync` (`Program.cs:375-391`) re-Hellos after a 4s `LivenessSettle` for GH-aware scenarios so any future deferred crash surfaces as a `rhino-crash` fault instead of a silent green; cost is +4s per GH scenario, forever.
- Frequency: all four gh-ui scenarios crashed the host until 2026-05-29; resolved, but the probe persists because the class (deferred native fault windows) is open-ended.

### FM5 — Scenario authoring compile wall (C#10 + divergent reference set + preamble surgery)

- Trigger: RhinoCode hardcodes csx compilation at C# 10 with a reference set (Eto + built Rasm assemblies + preloaded GH2 metadata, no `System.Linq`, no `Rasm.Domain`) that matches neither the repo's C# 14 csproj surface nor any locally runnable compiler. The client performs textual surgery on the scenario (`ScenarioLine` classification + `#r` injection + base usings + `SCENARIO_NAME`/`CAPTURE_PATH` consts + bootstrap + body marker, `Program.cs:503-530`).
- Blast radius: a green csproj build proves nothing about the csx; errors surface only at the `execute` phase against the live host, so the authoring loop is a multi-second host round trip per compiler error. The accumulated authoring rules fill a 15-point memory file (`reference_bridge_csx_scenario_constraints`): `Prelude.Array` collision, no collection expressions, no `params ReadOnlySpan` expansion, union case ctors instead of hallucinated verb factories, `[ValueObject].Create` returning unwrapped values, injected-const warnings co-reported with real errors. None of this knowledge lives in the tool.
- Current mitigation: bridge-owned using injection (`ScenarioBaseUsings`/`ScenarioHostUsings`), a hard ban on embedded `#r` (client throws), and good per-diagnostic line/column reporting in the `diagnostics` phase. [BAND-AID] — the divergence itself is untouched and untouchable from this side (the C#10 pin is McNeel's constant); there is no pre-flight compile because the only authoritative compiler is in-host.
- Frequency: in the surviving artifacts, the first two failures of the 2026-06-02 session are exactly this class — `CS1061` ('DrawPlan' has no 'Apply') and `CS1501`/`CS1503` (`Mutate` arity + params-span conversion) — i.e. drift between scenarios and a refactored production API caught only at host runtime.

### FM6 — Silent or thin failure evidence

- Trigger: a scenario assertion throws inside `Scenario.Run`. Some failure paths carry category/message/stack (`fault.category=execute`); others record `execute` with `data: {}`, `durationMs: 0`, no fault, no diagnostics, and skipped `liveness`/`diagnostics` phases.
- Blast radius: agent debugs blind and must re-run with added probes; each probe iteration costs a full host round trip (and, during the same session, occasionally a relaunch — see PID churn below).
- Current mitigation: facts now flush on failure (try/finally in `Scenario.Run`), verify decodes stdout markers into per-scenario facts, and `cde11be18` added first-failure surfacing to the summary. [BAND-AID accumulation] — each fix patches one evidence hole; the channel itself (stdout regex markers `rasm.rhino-bridge.evidence=facts=...`, `bridge.py:162-163`, subject to `OutputLimit` truncation) and the triple decode (plugin reply -> client JSON -> python msgspec + regex scan) remain.
- Frequency: 2 of 6 surviving artifact runs (`2026-06-02T06-01-44`, `06-03-19`) show the empty-`data` shape. Failed execute phases consistently report `durationMs: 0` — duration is not captured on the failure path at all.

### FM7 — Hang family: three competing timeout layers, no cancellation

- Trigger: a scenario blocks the UI thread (GH2 `SolutionServer.StartWait` deadlocks by design on the UI thread; any modal dialog; any long native call). Execution is synchronous on the Idle frame with `ServerExecutionCancelable: false` (`Transport.cs:254`).
- Blast radius: a blocked UI thread stops idle pumping, so the host cannot answer Hello; the client sees `Transport` (35s) or `Hello` (3s) timeouts; python sees `scenario_timeout_s` (180s); server-side `IdleDispatch` is 300s. Layers disagree: the client gives up at 35s while the host may still be executing the same script, and the only recovery is the FM1/FM2 force-quit cascade. The 180s python timeout can kill `dotnet run` while Rhino still holds the lease semantics of "busy".
- Current mitigation: `TimeoutPolicy` single-knob scaling (`RASM_BRIDGE_TIMEOUT_SCALE`, `BridgeWire.cs:262-277`) made the numbers coherent-ish in one place. [BAND-AID] — no cancellation channel, no watchdog, no host-side progress signal exists.
- Frequency: the headless-solver deadlock is documented as a hard ceiling (memory `reference_gh2_headless_solution_limits`); hangs are why the SIGKILL fallback exists.

### FM8 — Stale endpoint / wedged accept loop

- Trigger: Rhino killed without endpoint cleanup leaves a stale `endpoint.json`; PID reuse; a wedge in one of the 4 pipe accept loops.
- Blast radius: clients connect to a dead or wrong host; before hardening this surfaced as opaque connect failures.
- Current mitigation: defensive endpoint validation (PID start-time match, `rb-<pid>-` prefix, 64-char cap, `Program.cs:646-656`), `EnsureEndpoint` rewrite on every Hello, and Hello-failure -> `ReconcileAsync` before cold open ("wedge may block the accept loop", `Program.cs:326-328`). [ROOT-FIX-grade defensive validation] within the chosen file-discovery design; the staleness class itself is a consequence of that design.
- Frequency: hardened in `75d740adc`; "blind retry-once" removed in `1feeae6bb` (an earlier band-aid retired).

### FM9 — NuGet lock drift surfacing inside the bridge build phase

- Trigger: a `Directory.Packages.props` change with a stale `packages.lock.json`; the bridge's own `restore --locked-mode` rejects with NU1004/NU1403.
- Blast radius: previously read as a compile error, sending agents down the wrong path mid-verify.
- Current mitigation: `IsLockMismatch` (`Program.cs:433-440`) classifies it as a distinct `nuget-lock-drift` fault with explicit remediation text and a deliberate refusal to auto-regenerate. [ROOT-FIX for the misclassification; BAND-AID for the duplication] — the deeper fact is that the bridge client owns a full restore/build/msbuild pipeline (`Program.cs:399-431`) that duplicates the static rail and re-runs per scenario: in the 2026-06-02 batch, the same `Rasm.Grasshopper.csproj` was rebuilt for every scenario JSON (~2.5-3.0s each).
- Frequency: recurrent enough to earn a dedicated category constant (`CategoryNugetLockDrift`).

### FM10 — Single-flight contention and handshake amortization pushed to authors

- Trigger: one `clientGate` (semaphore 1) in the host plus the process-global python `bridge` lease; a second client gets `Busy`; a wedged holder blocks everything until exit-5.
- Blast radius: zero concurrency by design (one Rhino), but the consequence is that the 3-8s per-invocation handshake cost is managed by HUMANS organizing scenarios into thematic multi-Run files (memory `feedback_bridge_handshake_amortization`; `e7ee7936a` consolidated 9 vectors scenarios into 3 files explicitly to amortize it 3x).
- Current mitigation: `Busy` reply + lease exit-5. [BAND-AID] — batching as an authoring convention rather than a tool capability (no session/batch verb exists; each scenario is a fresh `dotnet run` + pipe + build + execute).

### FM11 — GH2 isolated-ALC load violations [RESOLVED, root-fixed]

- Trigger: `#r`-referencing GH2 from a scenario reloads it into the collectible ALC (`COR_E_INVALIDOPERATION`); raw `RunScript _G2` paths crash differently.
- Mitigation: pre-load GH2 via `PlugIn.LoadPlugIn` into the default ALC on the UI thread, then inject the already-loaded assemblies as `CompileReference.FromAssembly` (`CodeEngine.cs:84-95`); client hard-bans embedded `#r` (`Program.cs:510-512`). [ROOT-FIX]. Took UI scenarios from 1/8 to 6/8 passing in one commit (`75d740adc`).

## [3] Session-level evidence: one debugging hour, 2026-06-02

The six surviving verify runs are a single iteration session (05:59 to 06:05). They show three distinct Rhino PIDs (79524 -> 86685 -> 90031) inside six minutes — at least two full quit/relaunch cycles mid-session — plus two csx compile failures (FM5), two assertion failures with healthy fault text, two assertion failures with empty `data: {}` (FM6), and a per-scenario rebuild of the same project every run (FM9 note). This is the operator's "fragile" experience in miniature, captured in the tool's own artifacts.

A second-order observation: `run_verify` launches the host with `check=False` (`bridge.py:364`) — a failed launch does not stop the batch; every scenario then fails individually at `connect`, multiplying one launch failure into N timeout-paced failures.

## [4] ROOT-CAUSE CANDIDATES

Design decisions — not bugs — each of which generates a whole failure family above:

1. Lifecycle fights the host instead of supervising it. The bridge treats RhinoWIP as a peer to be launched, terminated, and tidied (Apple-Event/SIGKILL ladder, marker deletion, async `.ips` race) rather than as a supervised child with an owned lifecycle and owned recovery state. Generates FM1, FM2, and the relaunch portion of FM3. Any rebuild should make host supervision (launch, health, crash forensics, marker hygiene, restart budget) a first-class component with its own state machine, not a side effect of `launch`/`quit` verbs.
2. The plugin is version-frozen by ALC physics, so all intelligence was pushed outward. Because the `.rhp` cannot be hot-swapped, the plugin was kept deliberately dumb (4 verbs), and orchestration intelligence accreted in the client and python layers. That choice creates FM3 (every plugin change is a relaunch) AND the triple-layer report pipeline behind FM6. The leverage point is a two-tier plugin: a microscopic, never-changing loader shim in the default ALC, with the actual bridge logic in a collectible/replaceable layer — the same trick already proven for scenario assemblies (shadow copy + NeverCache).
3. The authoritative compiler lives only in the host. The C#10/RhinoCode/shadow-ref compile surface is unreproducible locally, so every authoring error costs a host round trip and authoring knowledge calcifies in memory files (FM5). A rebuild can host a pre-flight Roslyn compile at `LanguageVersion.CSharp10` against the exact staged reference closure — not perfect fidelity, but it would have caught both 2026-06-02 compile failures before touching Rhino.
4. Synchronous UI-thread execution with no cancellation or progress channel. `ServerExecutionCancelable: false` plus idle-frame dispatch means hangs are unkillable and only timeouts disagree about when to give up (FM7); deferred native faults are invisible without the bolted-on settle probe (FM4). A rebuild needs a host-side watchdog/heartbeat distinct from the request channel.
5. Evidence rides stdout as regex-scanned marker lines. Facts, captures, and return values are string markers subject to truncation, interleaving, and loss on crash, re-parsed by two downstream layers (FM6). The fix history (facts-flush-on-failure, first-failure surfacing, command-window capture) is a sequence of patches on a channel that should be a structured, crash-durable sink (e.g. host-written JSONL fact stream per scenario).
6. Stateless per-invocation client. Every command is a fresh `dotnet run` + pipe handshake + full project rebuild; batching is a human convention (FM10), per-scenario builds repeat identical work (FM9), and launch-failure multiplies across the batch because each scenario independently rediscovers the dead host. A session/daemon model (one client process per verify batch, build once, execute N) removes three failure surfaces at once.
7. Build-system duplication inside the runtime tool. The client embeds restore/build/msbuild with its own lock-mode policy, creating a second place where package drift, configuration, and artifact paths can disagree with the static rail (FM9). The rebuild question is directional: either the bridge consumes build outputs proven by the quality/assay rail, or it owns builds — not both.

## [5] Open verification gaps

- Artifact retention is 300s (`verify_retention_seconds`), so the empirical window is one session; longer-horizon frequency claims (how often FM2 fires per week) are reconstructed from git/memory, not measured. A rebuild should retain failure-run artifacts far longer than green runs.
- The "error-ignoring windows" the operator reports (beyond the crash-recovery dialog) are not directly evidenced in artifacts — most likely plugin-load error or package-manager dialogs after a failed install (FM3 mid-cycle); needs one observed reproduction to pin the exact dialog source.
- Whether `ReconcileAsync`'s marker clear can ever delete state belonging to a SECOND, human-operated Rhino session on the same machine is untested; the JXA terminate is PID-targeted but the marker paths are global.
