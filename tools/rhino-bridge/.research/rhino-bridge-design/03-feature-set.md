# [03] Agent-First Feature Set — Full Stories, Cut List, Ranking

Design-wave task D3, 2026-06-10. Derives the capability set for the C1 SUPERVISOR rebuild from the prior corpus (`tools/rhino-bridge/.research/rhino-bridge/` — bare numbers below cite those docs), the assay envelope contract (`tools/assay/core/model.py:369-419`), and the operator directives (agent-first, no pinning, no fragile logic, end-to-end sequence of logic). Every feature ships with its full story — agent job, trigger→behavior→evidence→decision chain, why automatic beats declared, and loud failure behavior — or it does not ship.

## [0]-[CONSUMER_CONTRACT_AND_SURFACE_LAW]

The only consumer is a coding agent driving `tools/assay`. Assay decodes ONE typed JSON document per session into `Envelope.report.detail: VerifySummary` (`facts`/`captures` as typed `(scenario, JSON)` pairs, `first_failure`, `first_fault_phase`) and maps `Envelope.exit_code` (model.py:401-419). Today that document is assembled by two regexes over 32 KiB-truncated stdout (`rails/bridge.py:55-56,206-207`) — the rebuild's evidence rail terminates in this exact struct with zero scraping, which is what shrinks the rail to ~150 LOC (02 §7).

Surface law, from doctrine (`MODAL_ARITY`, `POLICY_VALUES`, `DEEP_SURFACES`) applied to a CLI:

| [INDEX] | [VERB] | [OWNS] | [EVERYTHING ELSE] |
| :-----: | ------ | ------ | ----------------- |
| [1] | `verify <refs...>` | the session: lease → host → cargo → N scenarios → envelope | launch, connect, build consumption, staleness, GH2 preload, capture, retention — all automatic inside it |
| [2] | `doctor` | self-diagnosis with prescriptions | the same checks run implicitly (micro-doctor) at every session hello |
| [3] | `redeploy` | the shell quit→install→relaunch transaction | triggered explicitly; demanded automatically when hello detects shell/contract skew |
| [4] | `quit` | supervised clean shutdown (host-update hygiene; RhinoWIP updates monthly) | reconcile runs inside it; never needed mid-iteration |

Four verbs. The current system's five client verbs + ten python rail verbs + four wire commands (02 §1) collapse here. `verify` is modality-polymorphic on input shape — scenario file, glob, theme name, project, or nothing (= changed-scope from assay routing) — never on verb suffixes or flags. Session policy (warm reuse, recycle triggers, deadlines, retention) is a policy value inside the supervisor, not a flag set: the no-pinning directive plus agent-first means the common path is `verify <scope>` with zero options.

Substrate assumed from 10 §4 (not re-derived): Contract/Shell/Cargo/Supervisor split, StreamJsonRpc over the UDS-backed pipe, repo-compiled `[RhinoScenario]` assemblies at C# 14, port-verbatim list (AE-quit ladder, marker reconcile, endpoint liveness, GH2 preload, `OnExceptionReport` tap, status algebra, NU-drift classification).

## [1]-[FEATURES] — full stories

### F1 — Scenario sessions: N scenarios, one handshake

- [JOB] An agent finishing a `Rasm.Rhino` change needs the full affected scenario set verdicted in one invocation, paying launch/connect/build once — not 3-8 s handshake + ~3 s rebuild per scenario (FM10, FM9), and not N timeout-paced failures when the host is dead (02 §5).
- [SEQUENCE] `verify <refs>` → supervisor takes the resource-side lease (busy → typed exit 5 naming the holder; the flock leaves python, D11) → hello against the endpoint; warm host reuses it, cold path runs reconcile→launch→connect with the readiness notification replacing the 250 ms blind poll (F5/FM8) → version+capability negotiation → cargo closure staged by content hash and loaded if changed (F2) → cargo enumerates `[RhinoScenario]` entrypoints in-host after load (discovery never loads host-native types out-of-host — 08 §4[5]) → per scenario: `phaseStarted` notification, UI-thread idle dispatch, facts stream live + spool to JSONL (F3), heartbeat continues independently (F4) → rank-max fold → one envelope JSON on stdout, artifacts under the report dir → agent reads `first_failure` + facts and edits the named file/line (PDB-real stack traces, since scenarios are repo-compiled).
- [AUTOMATIC] No `launch`/`connect`/`check` choreography exists to mis-order: the session ladder is internal, warm-vs-cold is detected by hello not declared, GH2 preload triggers off the scenario assembly's reference closure (one truth, replacing the MSBuild-prop-OR-dll-sniff dual heuristic, 01 F17), and builds are consumed from the rail-proven output tree rather than re-run (D8). Grouping folklore (`feedback_bridge_handshake_amortization`) dies: scenario-per-concern files become free because batching is a tool property.
- [FAILURE] Dead host → ONE named launch/connect fault for the whole batch, not N (the quality-rail swallow class, 02 §5). One authoritative deadline (supervisor) with server/python values derived from it (FM7); a scenario exceeding it yields `execute-deadline` naming the scenario, theme, elapsed, and facts-to-point-of-death — never a naked `OperationCanceledException`. Busy lease → exit 5 with holder PID and age.

### F2 — Hot cargo redeploy: staleness is hashed, never declared

- [JOB] An agent that just rebuilt `Rasm.Rhino.dll` (or bumped LanguageExt in `Directory.Packages.props`) needs the new bits live in the warm host on the next `verify`, in seconds, with zero yak/quit/relaunch and zero dialogs (FM3 — the operator's killer pain).
- [SEQUENCE] Next `verify` → supervisor fingerprints the staged closure (same content-hash staging as today's `refs/<sha>/`, now per session) → hash equal: skip, fact `cargo.reused` → hash differs: `unloadCargo` (scenario scopes disposed, host-event detachers drained per the reference-direction law, 07 §1) → unload confirmation via `WeakReference` + bounded GC retries → new collectible ALC, `loadCargo(hash)` → facts `cargo.hash`, `cargo.swapMs`, `cargo.unloadConfirmed` → scenarios run against the new bits.
- [AUTOMATIC] There is no `redeploy-cargo` verb and no `--fresh` flag: the hash IS the decision. Per-ALC copies of LanguageExt/Thinktecture make dependency bumps hot too (07 §1 pitfall 7) — the version-tolerance directive lands here: bridge logic, libs-under-test, scenario kit, and dependency bumps all ride cargo; only shell/GH2/host changes relaunch (10 §1.2 change-class table).
- [FAILURE] `unloadConfirmed=false` → automatic gcdump artifact naming the rooting path (07 §4) + named fault `cargo-unload-leak` → supervisor falls back to a supervised host recycle inside the same invocation (degraded, loud, still one command). Under an attached debugger the confirmation is flagged unreliable rather than asserted (07 §1 pitfall 6). If Phase 0a proves Rasm statics pin the ALC un-fixably, this feature degrades to cheap-and-diagnosed per-session recycling without touching any other feature (10 §4 fallback).

### F3 — Typed evidence rail: facts and captures, streamed and crash-durable

- [JOB] An agent triaging a red scenario needs the failed assertion's label, observed value, and visual state from the FIRST run — not a re-run with added probes (FM6), and not facts silently severed past a 32 KiB stdout cap (01 F1).
- [SEQUENCE] Scenario body calls the kit — `Probe.Require(label, observed)` fuses assertion and fact in one call; every `Probe.Expect<TCase>` auto-facts its projection (03 §5.3, replacing the 19 hand-rolled switch-throw sites) → each fact is simultaneously (a) an RPC notification streamed live to the supervisor and (b) a line appended to the per-scenario JSONL spool on disk in-host (08 §3: persist at the execution site; streaming is the enhancement) → on failure inside a `DocumentScope`/viewport scenario, auto-capture writes the PNG and emits a typed capture record (the rail finally earns its threading — today zero corpus scenarios use it, 03 §4.3) → envelope carries facts/captures typed end-to-end into `VerifySummary.facts/captures`; both regexes and both msgspec mirrors are deleted.
- [AUTOMATIC] Evidence is a side effect of asserting, not a parallel duty (today rule 12 — "remember to also add the fact" — is folklore enforced by failure). Captures fire on failure without any declaration; explicit `Capture.Snapshot` remains for green-path visual evidence. Retention is a policy value: failure-run artifacts retained on the order of days, green runs aggressively pruned — replacing the 300 s TTL that evaporates debugging history faster than a session (04 §5).
- [FAILURE] Host dies mid-scenario → the JSONL spool survives; the supervisor harvests facts-to-point-of-death and attaches them to the crash-attributed scenario (F4). No size cliff: facts are wire fields, not log lines. A scenario that emits zero facts is itself a named warning fact (`facts.empty`) so thin evidence is visible at review, not discovered during triage.

### F4 — Crash forensics auto-capture: host death is a parsed outcome

- [JOB] When Rhino SIGABRTs 6 s after a green execute (FM4) or wedges on a modal dialog, the agent needs the cause named in THIS invocation's envelope — not a misattributed `connect` failure on the next scenario, and never a silently-ignored error window.
- [SEQUENCE] Session start → supervisor snapshots the `.ips` set and arms kqueue `NOTE_EXIT` on the host PID (G3 prices the ~50-100 LOC kevent interop; 250 ms PID polling is the accepted fallback) → during execution three signals discriminate the failure space: process exit (kqueue) ⇒ death; heartbeat silent + process alive ⇒ `ui-wedged` or, when launch-adjacent, `dialog-suspected` (the operator's recovery-dialog pain becomes a NAMED fault instead of an opaque 90 s timeout, 02 §4.1); `shutdownStarted` notification ⇒ AE-quit in progress, not a hang → on death: `.ips` diff → parse crash thread + exception type into a typed crash fact → attribute to the in-flight scenario → harvest its JSONL spool → reconcile markers → envelope status `rhino-crash` with the faulting scenario named → restart-budget policy decides whether the supervisor relaunches for the remainder of the batch (compute.frontend pattern, 08 §2.4).
- [AUTOMATIC] Zero configuration and zero plugin code for the OS layer: `.ips` snapshot/diff, kqueue, and marker reconcile are supervisor-side. The `OnExceptionReport` tap (the only window into UI-thread-swallowed faults — must survive any rebuild, 09 §2.6) flips green verdicts with the report attached, as today.
- [FAILURE] Forensics never blocks the verdict: an unparseable `.ips` attaches as a raw artifact path with a `forensics-degraded` note; a missed kqueue registration degrades to polling with the degradation itself recorded as a fact. The liveness "4 s settle sleep" heuristic (01 F6) is deleted — deferred crashes are caught by the exit watch whenever they fire, not within a guessed window.

### F5 — Supervised lifecycle + the `redeploy` transaction

- [JOB] Shell/contract changes and monthly RhinoWIP updates need quit→install→relaunch as ONE verdicted transaction — today it is a two-language policy table driving five verbs with at least five swallow points (`closed:false` → Ok, discarded JXA exit, uncaught stage copy — 02 §4), which is exactly where the "error-ignoring windows" pain lives.
- [SEQUENCE] `redeploy` → AE-terminate (the only clean macOS route; `Exit(false)`/SIGTERM self-SIGABRT — port verbatim, 09 §4) → kqueue-confirmed exit with deadline → escalate `forceTerminate`→SIGKILL only on overrun, each step a typed phase → marker reconcile → `yak install` → cold launch → connect → doctor proves the new shell → one envelope. Hello-time skew detection closes the loop: if negotiation finds shell older than contract, `verify` fails fast with `shell-skew` and the prescription "run redeploy" — the agent never debugs a lockstep mismatch as a JSON decode error (D9).
- [AUTOMATIC] The transaction is demanded by the system, not remembered by the agent: skew is detected, not documented. Within `verify`, lifecycle is invisible — launch and quit are session policy (warm by default; recycle on crash, on unload-leak fallback, on host-update detection via bundle version change), not agent choreography.
- [FAILURE] Every historical swallow point is a typed phase failure: `closed:false` FAILS the quit phase; the JXA exit code is read; install errors carry yak output; a relaunch that connects but fails doctor rolls the envelope to `redeploy-incomplete` with the failing check named. Alive-but-silent after relaunch ⇒ `dialog-suspected` (F4), the precise diagnosis the current tool cannot make.

### F6 — Doctor: self-diagnosis with prescriptions

- [JOB] An agent hitting any tooling fault needs "what is broken and what command fixes it" in one read — today diagnosis is distributed across endpoint files, env vars, yak state, and private memory files.
- [SEQUENCE] `doctor` → typed check rows: bundle discovery (newest `Rhino*.app` by `CFBundleVersion` — restored from the archived quality stack, 02 §6.4), env, endpoint file vs live PID, shell version vs contract version, host runtime version, GH2 presence + preload state, cargo ALC state, capability flags (`gh2.dataflow`, `eventpipe`, `cargo.hotswap`) carrying the recorded Phase 0 probe verdicts, lease state → each row ok/failed + one-line prescription → envelope.
- [AUTOMATIC] The same checks run as a micro-doctor inside every session hello, so `verify` self-diagnoses without being asked; explicit `doctor` exists for the deep read and for post-`redeploy` proof. Capability flags are how version tolerance stays a design property: a WIP host update that changes a capability surfaces as a flag flip with a named consequence ("dataflow lane → render-only"), not as scattered scenario failures — re-verified per WIP bump per G1.
- [FAILURE] Doctor never throws and never short-circuits: all rows always evaluate, so one broken layer cannot hide another. A doctor that cannot even reach the host still reports the workstation-side rows plus the discriminated host state (no process / process-no-pipe / pipe-no-hello).

### F7 — GH2 lanes: mutation oracle now, dataflow oracle gated, `.ghz` diff later

- [JOB] An agent changing `Rasm.Grasshopper` needs GH2 behavior verdicted: today that means document mutation + render evidence; it should mean computed-value assertions the moment the host permits them.
- [SEQUENCE] Lane 1 — mutation oracle (v1-core): cargo requests GH2 preload (default-ALC `PlugIn.LoadPlugIn` + `FromAssembly` injection — port verbatim, D12) → headless `GhEditor` via constructor, never `Show()` (the StatusBar SIGABRT root fix) → place/wire/mutate through the wrappers → facts + render captures via `DrawToBitmap`. Lane 2 — dataflow oracle (gated on Phase 0b): `Start(SolutionMode.Headless)` → `Task<Solution>`/`SolutionServer` events with deadline → `ObjectSolutionState.Data` → `SolutionData.Tree()` reads as typed value facts (06 §2 contradicts the old "never settles" ceiling; only the live probe decides). Lane 3 — `.ghz` golden-diff (post-cutover): GrasshopperIO `Differences` archive assertions, no solving at all (06 §3).
- [AUTOMATIC] Lane selection is capability-driven, not author-declared: if probe 0b passes, `gh2.dataflow=true` and value-oracle scenarios run; if not, they report `unsupported` by name. The 4 s GH liveness sleep is deleted in favor of F4's exit watch.
- [FAILURE] A dataflow scenario on a dataflow-incapable host is a typed `unsupported` row with the capability flag cited — never a hang into `StartWait` deadlock and never a silent skip. A GH2 solve that exceeds its deadline reports solver phase, `ComputableCount`, and the `SolutionRecord` tail as facts (the post-hoc history GH2 already keeps, 06 §2).

### F8 — Iterate mode: the warm inner loop as an emergent property

- [JOB] An agent in an edit→verify cycle on one scenario needs edit→verdict in <10 s warm (cutover criterion 4, 10 §3) and the full batch at ≤50% of the old tool's wall time.
- [SEQUENCE] Emergent from F1+F2, not a mode: warm host (no launch), hash check (~0 when unchanged), cargo swap only when bits changed, scoped selection (`verify <one theme>`; changed-file→scenario mapping stays in assay routing — workspace knowledge is python's legitimate remit, 02 §7) → verdict. The supervisor holds nothing exclusively between invocations: lease is taken and released per session so a human Rhino session or a second workflow is never starved.
- [AUTOMATIC] There is no watch daemon and no `--warm` flag: warmth is a fact about the world (host up, hashes equal) that the tool detects per invocation. Agents invoke explicitly; a resident fs-watcher is human-shaped furniture (cut below).
- [FAILURE] A warm-path fault (stale endpoint, wedged accept loop) is exactly the cold-path fault taxonomy — the session ladder reconciles and relaunches inside the same invocation, with the cold transition recorded as a fact so latency regressions are attributable.

## [2]-[CUT_LIST] — considered and rejected

| [X] | [BELL] | [WHY CUT] |
| :-: | ------ | --------- |
| 1 | Resident fs-watch daemon (true "watch mode") | Agents invoke explicitly; per-invocation hash detection delivers the same warm latency with no daemon lifecycle to supervise (F8) |
| 2 | MTP adapter in v1 (C2) | Committed phase-5 front end over the same session API; two unknowns at once is wrong v1 sequencing (10 §1.3) |
| 3 | In-host REPL/eval verb (C3 compiler) | Roslyn-coexistence risk + violates frozen shell; session API leaves the verb slot, compiler drops into cargo later (10 §1.4) |
| 4 | MessagePack/binary wire formatter | Control-plane payloads are JSON-sized; debuggable wire wins until captures move inline (07 §3) |
| 5 | AX/window-enumeration dialog detection as primary | Reconcile prevents the dialog; alive-but-silent heuristic names it; TCC-granted positive detection is post-cutover fallback only (07 §5) |
| 6 | Concurrent scenario execution | One UI thread is host law; sessions amortize instead of parallelize (08 §4[9]) |
| 7 | Flaky-retry / auto-rerun of failed scenarios | Masks real host faults; the restart budget exists for host DEATH, never for assertion failure |
| 8 | `--cold` / `--fresh` host flag | Recycle triggers are automatic policy (crash, unload-leak, host-update); a determinism flag re-smuggles the knob `MODAL_ARITY` forbids |
| 9 | Always-on per-scenario viewport capture | Token/disk waste; auto-capture-on-failure + explicit `Capture.Snapshot` covers both halves (F3) |
| 10 | Pixel-diff golden-image oracle on captures | Render output drifts with every WIP bump — fragile-by-construction against an unpinnable host; captures are evidence, facts are oracles |
| 11 | OTel collector/exporter integration | Single-workstation dev tool; JSONL artifacts + envelope suffice (07 §4) |
| 12 | TCP/HTTP/WebSocket transport, remote hosts | Pipe IS a UDS on macOS; remote execution has no consumer (11 §4 modalities) |
| 13 | Multi-instance / multi-host orchestration | One workstation, one supervised RhinoWIP; the lease is the law |
| 14 | Per-scenario timeout CLI knobs | One authoritative supervisor deadline; a long-running scenario declares its budget at definition time on `[RhinoScenario]`, not at invocation |
| 15 | Out-of-host scenario discovery by loading types | Anti-pattern with receipts (Rhino.Testing discovery failures, 08 §4[5]); enumeration is in-host post-load |
| 16 | Telemetry sidecar / results database | pyRevit's Go+Mongo path is fleet-scale machinery; retention-tiered JSONL answers every agent query |
| 17 | TUI/dashboard/pretty reporters | The consumer is an agent; the envelope is the UI |
| 18 | Scenario front-matter directive DSL (`//#:`) | Dead with repo-compiled scenarios; attributes are the declaration surface (07 §2 borrowed the pattern; C1 obsoletes it) |
| 19 | `.verify.csx` compatibility layer in the new tool | Greenfield law; the OLD tool runs side-by-side as the oracle until cutover — compat shims forbidden (`ROOT_REBUILD`) |
| 20 | Rhino 8 stable-host lane | Dead: R8 bundles .NET 8, repo plugins are net10.0; closed by audit so nobody re-asks (11 G2) |
| 21 | GH1 lane | Repo is GH2-only; GH1 has no consumer in any Rasm surface |
| 22 | Cancellation of in-flight UI-thread work | Host law — not cancelable; RPC cancellation frees the pipe, the deadline + named hang fault owns the rest (D5 verdict) |
| 23 | Yak publishing of the bridge for external users | Repo-internal tool; packaging exists only as the shell's own redeploy step |
| 24 | Auto-regenerating NuGet lock drift | Deliberate refusal kept from today: classify `nuget-lock-drift` + prescription, never mutate lockfiles from a runtime tool (FM9) |
| 25 | EventPipe counters on every run | Gated on probe 0c and post-cutover even then; counters are diagnosis-grade, not verdict-grade — auto-attach only when forensics escalates |
| 26 | In-envelope full stdout/stderr transcripts | The 32 KiB truncation class dies by NOT routing evidence through process output; transcripts remain as artifacts on disk, referenced by path |

## [3]-[RANKING]

| [TIER] | [FEATURES] | [GATE] |
| ------ | ---------- | ------ |
| v1-core | F1 sessions; F2 hot cargo redeploy; F3 typed evidence (incl. auto-capture-on-failure, retention tiers); F4 crash forensics (kqueue, `.ips` diff, heartbeat, `dialog-suspected`); F5 supervised lifecycle + `redeploy`; F6 doctor + capability flags; F7 lane 1 (GH2 mutation oracle); F8 warm iterate | Phase 0a sizes F2's disposal contract; everything else gate-free |
| v1-nice | F7 lane 2 (GH2 dataflow oracle); session-scoped `.3dm` fixture snapshots (amortize expensive fixtures across themes, 10 §1.2); `facts.empty` lint row; capability-map page seeded from 03 §3 F6 inventory | Lane 2 strictly on probe 0b; the rest are kit-level additions |
| post-cutover | MTP adapter (C2) over the session API; `.ghz` golden-diff lane (GrasshopperIO `Differences`); cargo-resident eval/REPL lane (C3 compiler in cargo); EventPipe artifacts (probe 0c); AX-positive dialog naming | Each lands with its own scenario-grade proof (10 §4 phase 5) |

Sequence-of-logic check, per the operator directive: every v1-core feature's trigger is either the agent's one common verb (`verify`) or a detected world-state (hash drift, version skew, process exit, heartbeat silence) — no feature requires the agent to know an ordering, a flag, or a folklore rule to get its value, and every failure path terminates in a named fault inside the envelope the agent was already reading.

## [4]-[OPEN_QUESTIONS]

1. Scenario enumeration contract: in-host post-load enumeration is anti-pattern-safe but means "list scenarios" requires a live host — should the supervisor additionally read entrypoint names supervisor-side via `MetadataLoadContext` (metadata-only, no type loading) so `verify --list`-style introspection and the future MTP adapter share one discovery path? Needs a one-hour spike, not a probe.
2. Restart-budget policy values: after a mid-batch host crash, how many auto-relaunches before the session folds to `rhino-crash` for the remainder (proposal: 1 relaunch, then fail remaining scenarios as `skipped` with the crash cited)? Operator taste decision.
3. Retention tiers need numbers: proposal failure-runs 7 days / green-runs next-run-pruned; the corpus only establishes that 300 s is wrong (04 §5).
4. Does auto-capture-on-failure apply to GH2 canvas scenarios (DrawToBitmap on a failed mutation oracle), or only `DocumentScope` viewport scenarios? Cheap to include; capture cost on the UI thread mid-failure needs one measurement.
5. Whether `quit` deserves verb status or folds into `redeploy --host-update` semantics — kept as a verb here because monthly RhinoWIP updates are a real recurring path that should not require a shell redeploy to express; revisit when the supervisor state machine is concrete (D-wave lifecycle doc owns the final call).
