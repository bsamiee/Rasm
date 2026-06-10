# [10] Candidate Architectures + Recommendation

Input: docs 01-09 (09's decision register is the foundation). Constraint set carried forward: macOS has no headless Rhino (05); the in-host bridge approach is [CONFIRMED] essential (09 §3); four rebuild-only decision generators D1/D4/D5/D7 produce or amplify 8 of 11 failure families; the port-verbatim list (09 §4) and the two live probes (ALC unload, GH2 `Start(Headless)`) gate any build. .NET 10 is the floor, not a constraint.

All candidates share the invariant core that no design may regress: in-host UI-thread execution; out-of-process driver; endpoint discovery with PID/start-time liveness (D10); AE-terminate→SIGKILL ladder + pre-launch marker reconcile (D3); GH2 default-ALC preload + `FromAssembly` injection (D12); phase vocabulary + first-non-ok taxonomy; `OnExceptionReport` capture.

## [1]-[CANDIDATES]

| [ID] | [NAME] | [ONE-LINE SHAPE] |
| :--: | ------ | ---------------- |
| C0 | NULL — keep-and-harden | Current topology; apply every [FIXABLE-IN-PLACE] item from 09 §1; no boundary moves |
| C1 | SUPERVISOR — shell/cargo plugin + session-owning C# supervisor + repo-compiled scenarios | The 09-converged rebuild: boundaries redrawn to rates of change |
| C2 | TESTHOST — C1 substrate fronted by an MTP framework adapter from v1 | `dotnet test` is the user surface; python optional |
| C3 | RESIDENT — shell/cargo plugin + in-host Roslyn C# 14 compile of single-file scenarios | Keeps `.csx`-style authoring, deletes RhinoCode; thin daemon client |

### [1.1] C0 — NULL: keep-and-harden

Scope = the [FIXABLE-IN-PLACE] column of 09 §1 only: wire `schema` field + skew tolerance (D9); client deadline authoritative, python/server timeouts derived (D5 partial); typed facts promoted from stdout markers into `BridgePhase.data` (D6 partial — plugin already holds typed `FactBag`); consume rail-proven build outputs instead of client-owned restore/build (D8); flock lease moved into the C# client (D11); `clean` arity fix; choreography swallow fixes (`closed:false` fails the phase, JXA exit code read, dialog-suspected message on alive-but-silent); offline Roslyn pre-flight compile at C# 10 against staged refs (04 §4.3 palliative); bundle discovery restored into the client.

- Component shape: unchanged 3 projects + 1 python rail (quality archived). ~400-600 LOC of edits.
- Scenario model: unchanged third dialect (C# 10, no IDE/analyzers/IVT, rewrite pipeline, offset diagnostics). The 15-rule folklore contract persists; pre-flight only moves WHERE rule violations are caught.
- Reload story: unchanged — every plugin/protocol change is quit→install→relaunch through the dialog gauntlet (D1 untouched).
- Diagnostics: typed facts on the wire is a real upgrade; no streaming, no crash-durable spool, no `.ips` parse, settle-probe heuristic stays.
- Honest assessment: this is the floor, not a candidate to choose. 09 §4 already showed 59 commits of exactly this class of work hit a ceiling without retiring any generator. C0's value is as the regression oracle during migration and as the fallback if Phase 0 probes fail catastrophically.

### [1.2] C1 — SUPERVISOR (recommended; details in §4)

Component shape — three C# components + one thin python consumer, boundaries aligned to rates of change:

| [COMPONENT] | [PROJECT] | [LIVES] | [RATE OF CHANGE] | [OWNS] |
| ----------- | --------- | ------- | ---------------- | ------ |
| Contract | `Rasm.Bridge.Contract` | both sides | frozen after v1 (versioned) | RPC interfaces, DTOs, `PhaseStatus` algebra (ported), fact/capture records, version negotiation |
| Shell | `Rasm.Bridge.Shell` (`.rhp`, AtStartup, default ALC) | in Rhino | near-zero by design | StreamJsonRpc endpoint over named pipe; idle-queue UI marshal; collectible-ALC lifecycle (load/unload/confirm); GH2 preload; `OnExceptionReport` tap; endpoint file; `shutdownStarted` notification on `RhinoApp.Closing` |
| Cargo | `Rasm.Bridge.Cargo` (+ scenario kit) | collectible ALC, swapped per session | every iteration | scenario assembly load + invoke; `FactBag`/capture; `DocumentScope`; GH2 wrappers; JSONL evidence spool to disk |
| Supervisor | `Rasm.Bridge.Supervisor` (exe) | workstation | iterates freely (out-of-host) | session state machine: lease, bundle discovery, launch/reconcile, kqueue `NOTE_EXIT` watch, connect, N-scenario session loop, quit ladder, `redeploy` transaction, `.ips` diff, result envelope |
| assay rail | `tools/assay/rails/bridge.py` | python | thin | repo routing (scenario→project), invoke supervisor, decode ONE typed JSON document, fold Assay envelope (~150 LOC; both regex scrapers and both msgspec mirrors deleted) |

Transport + contract: StreamJsonRpc 2.25.x over the existing named pipe (UDS on macOS — 07 §3). One shared interface pair in Contract: `IBridgeShell` (hello/negotiate, loadCargo(hash), runScenario(ref), unloadCargo, prepareQuit) + `IBridgeEvents` notifications (fact, progress, phaseStarted, exceptionReported, shutdownStarted). `SystemTextJsonFormatter` (control-plane payloads; wire stays debuggable). Version field + capability set exchanged at hello; client/shell tolerate one minor-version skew — lockstep deploys end (D9). The hand-rolled envelope/dispatch in `BridgeWire` retires; DTO vocabulary and rank-max status fold port verbatim (09 §2.2).

Scenario model — kill the dialect (03 §5.1-A): scenarios are normal repo C# — `[RhinoScenario("theme")]` static entrypoints in per-lib `*.Scenarios.csproj` referencing the owning lib + testkit, compiled by the repo toolchain at C# 14 with analyzers, IVT, IDE/LSP, PDBs. The bridge's job collapses to: stage content-hashed assembly closure → load into cargo ALC → invoke entrypoint → stream typed facts. Authoring discoverability becomes ordinary: go-to-definition works, CSP analyzers fire at edit time, a green `static build` IS the compile proof (F1/F2/F3 of 03 die), templates live as ordinary code, and 03 §2.3 rules 1-8 + 11 cease to exist. The remaining contract (UI-thread semantics, GH2 wrapper discipline, capability map) fits one in-repo page. No Roslyn ships in the host at all — the Roslyn-coexistence open question evaporates for this candidate.

Package-update/reload story (the killer pain), by change class:

| [CHANGE] | [TODAY] | [C1] |
| -------- | ------- | ---- |
| Lib-under-test rebuild | hot (shadow refs) — already root-fixed | hot (cargo ALC swap; same hash-staging, now with unload) |
| Scenario code | hot, but compile-fails in-host | hot, compile-proven offline first |
| Dependency bump (LanguageExt/Thinktecture) | relaunch risk via plugin | hot — per-ALC copies load into cargo (07 §1 pitfall 7) |
| Bridge logic (today: plugin) | quit→yak→relaunch→dialog roulette | hot — bridge logic IS cargo |
| Shell/Contract change | n/a | relaunch — but shell is frozen by design; budget ≤1 redeploy/week post-stabilization, executed as a supervised `redeploy` transaction with dialog-suspected diagnosis |
| GH2 / RhinoWIP update | relaunch | relaunch (host law, 05 §6) — unchanged and correctly rare |

Diagnostics: live `IBridgeEvents` stream replaces post-hoc stdout scraping; cargo spools facts/phases/captures to per-scenario JSONL on disk as they happen (crash-durable — anti-pattern 08 §4[1] closed); supervisor snapshots/diffs `.ips` pre/post and parses crash thread + exception type into a typed fact; `unloadConfirmed` fact per session with automatic gcdump artifact on failure; PDB-real stack traces from repo-compiled scenarios; EventPipe counters as run artifacts if the Phase 0 probe confirms `DOTNET_EnableDiagnostics`; failure-run retention ≫ green-run retention (replaces the 300 s TTL that evaporates debugging history).

Failure recovery — supervised lifecycle, no error-ignoring windows: the supervisor owns a real state machine (idle → reconcile → launch → connect → session → quit → reconcile), with kqueue `NOTE_EXIT` distinguishing process-death from UI-wedge instantly, heartbeat distinguishing wedge from slow scenario, `shutdownStarted` distinguishing AE-quit-in-progress from hang, and a restart budget (compute.frontend pattern, 08 §2.4). Alive-but-silent past deadline ⇒ named `dialog-suspected` fault; AX/window-enumeration positive detection is the optional fallback, not the primary defense (07 §5). All 02 §4 swallow points become typed phase failures.

Capabilities beyond parity (reachable only on this substrate):

- Session batching as a tool capability: one launch + one cargo load + N scenarios; the grouping folklore (`feedback_bridge_handshake_amortization`) and the 10-files×1-theme drift become obsolete.
- Watch mode: supervisor watches lib/scenario bin outputs → re-stage → ALC swap → re-run affected themes; warm inner loop target <10 s.
- GH2 computed-value oracles: `Start(SolutionMode.Headless)` → `Task<Solution>`/`SolutionServer` events → `SolutionData.Tree()` reads, gated on the Phase 0 probe (06 §2 contradicts the old "never settles" ceiling).
- `.ghz` golden-file + archive-diff assertions via GrasshopperIO's built-in `Differences` engine — regression rail with no solving at all (06 §3).
- Doc-state snapshots: session-scoped `.3dm` fixture save/restore so expensive fixtures amortize across themes.
- Auto-capture on failure for `DocumentScope` scenarios — the PNG rail finally earns its threading (03 §5.3).
- Dialog naming, `.ips`-as-evidence, gcdump-on-leak: host failures become parsed outcomes (Gauntlet property, 08 §2.2).
- Interactive REPL: deferred — requires in-host Roslyn (see C3); the session API leaves room for an `eval` verb later.

### [1.3] C2 — TESTHOST: MTP adapter from v1

Identical plugin substrate (Contract/Shell/Cargo as C1). The difference is the front end: scenarios are MTP-native tests (`[RhinoScenario]` surfaces as a Microsoft.Testing.Platform framework adapter), so `dotnet test --filter`, IDE Test Explorer, TRX, and CI reporting come free; the supervisor logic lives inside the MTP test-host process with crash/hang observation in MTP's out-of-process extension model (08 §2.5). ricaun.RevitTest proves the shape end-to-end for a GUI host.

Deltas vs C1:

- [+] Standard front end from day 1; python orchestration optional immediately; scenario enumeration/filtering/reporting never bespoke.
- [−] Discovery constraint is hard: enumerating tests must never load host-native types out-of-host (anti-pattern 08 §4[5]; Rhino.Testing's documented discovery failures). Requires `MetadataLoadContext`/source-metadata discovery — solvable but it is NEW protocol work stacked on top of the session core.
- [−] Two unknowns ship at once (session/ALC core + adapter protocol); a failure in either blocks the only front end. C1's bespoke session CLI is ~the same code the MTP adapter would call anyway.
- [−] The Assay envelope must still be fed for repo-uniform reporting; MTP does not remove that consumer, it adds a second one.

Verdict: right long-horizon front end, wrong v1 sequencing. Build C1 with the session API deliberately MTP-shaped (enumerable scenario refs, per-scenario typed results, stream-friendly), then add the adapter as a phase-5 front end with zero substrate change. This resolves 09's open question "MTP from v1 vs bespoke CLI first" in favor of CLI-first-MTP-second.

### [1.4] C3 — RESIDENT: in-host C# 14 compile, single-file scenarios

Shell/cargo plugin as C1, but scenario compilation moves INTO the shell: plain incremental `CSharpCompilation` at `LanguageVersion.Latest` (07 §2 Option B, ~100-150 LOC; CSharpScript rejected — `InteractiveAssemblyLoader` leaks), emitting with PDBs into the cargo ALC. Scenarios stay single files with `#:`-style front-matter directives (`//#: plugins grasshopper`); the client thins to a daemon that proxies and supervises.

- [+] Lowest-ceremony authoring (no csproj per lib); closest to McNeel's own converging surfaces (rhinocode script server, RhinoMCP, in-app `test_*` runner — 05 §7); the identical compiler is invokable offline, so pre-flight is EXACT, not approximate; natural home for a REPL verb and for ad-hoc agent-driven probes.
- [−] Still a dialect: no analyzers, no IVT (internal surfaces untestable — 03 rule 6 persists), no IDE symbol resolution (F2 persists), injected context object instead of ordinary code sharing. The C# version axis of F1 dies; the tooling axis survives.
- [−] Reintroduces the Roslyn-coexistence question (bridge-pinned 5.3.0 in a private ALC vs RhinoCode's bundled 5.x daily — 07 open question 3) that C1 avoids entirely.
- [−] Shell is no longer minimal: the compiler service lives in the relaunch-expensive layer, violating the frozen-shell principle the candidate exists to serve (compiler bugs/upgrades = relaunch).

Verdict: not the v1 architecture. Its compiler service is the right FUTURE add-on (REPL/eval verb, agent probe lane) — placed in CARGO, not shell, so it hot-swaps — once C1 is stable.

## [2]-[FM_SCORECARD] — line-by-line against the 04 dossier

[ELIM] = the generating decision is removed (structural). [MITI] = exposure/diagnosis materially improved, generator remains (usually host law). [KEEP] = unchanged. [SOUND] = already root-fixed; ported verbatim.

| [FM] | [FAILURE FAMILY] | [C0] | [C1] | [C2] | [C3] |
| :--: | ---------------- | :--: | :--: | :--: | :--: |
| FM1 | Exit self-SIGABRT (host law) | [MITI] ladder kept | [MITI] ladder kept; exposure collapses with relaunch count | [MITI] same | [MITI] same |
| FM2 | Crash-recovery dialog wedge | [MITI] reconcile kept | [MITI] reconcile kept + `dialog-suspected` named fault + AX fallback; exposure collapses | [MITI] same | [MITI] same |
| FM3 | Package update ⇒ relaunch gauntlet | [KEEP] every bridge change relaunches | [ELIM] for bridge logic, libs, deps, scenario kit (cargo swap); [KEEP] only shell/GH2/host — rare by construction | [ELIM] same | [ELIM] same, minus compiler-in-shell caveat |
| FM4 | Deferred crash after green execute | [MITI] 4 s settle probe | [ELIM] as misattribution class: kqueue `NOTE_EXIT` + heartbeat + crash-durable spool attribute the crash to the right scenario; native crash itself is host territory | [ELIM] same + MTP crash-dump observer | [ELIM] same |
| FM5 | Authoring compile wall (dialect) | [MITI] offline C#10 pre-flight only | [ELIM] no dialect: repo C# 14, IDE, analyzers, IVT, PDBs; rules 1-8, 11 deleted | [ELIM] same | [MITI+] version axis dies; tooling axis (no IDE/IVT/analyzers) survives |
| FM6 | Thin/silent failure evidence | [MITI] typed facts in phase data; still connection-bound | [ELIM] typed wire events + disk JSONL spool + failure retention; empty-`data`/`durationMs:0` class impossible by construction | [ELIM] same | [ELIM] same |
| FM7 | Hang: 3 timeout authorities, no cancel | [MITI] unify deadlines | [MITI+] one authoritative deadline + heartbeat + `shutdownStarted` + watchdog policy (kill+relaunch budget); UI work stays non-cancelable (host law, 08 §4[9]) | [MITI+] same | [MITI+] same |
| FM8 | Stale endpoint | [SOUND] | [SOUND] + shell readiness notification kills the 250 ms blind poll | [SOUND] | [SOUND] |
| FM9 | NuGet lock drift inside bridge build | [MITI] classification + consume rail outputs | [ELIM] supervisor consumes rail-proven outputs only; second build truth deleted (D8) | [ELIM] builds via normal `dotnet test` pipeline | [ELIM] same as C1 |
| FM10 | Handshake tax / no session | [KEEP] (batch verb bolt-on still leaves split ownership) | [ELIM] session is the spine: 1 launch + 1 cargo load + N scenarios; folklore grouping obsolete | [ELIM] same | [ELIM] same |
| FM11 | GH2 ALC violations | [SOUND] | [SOUND] preload + `FromAssembly` ported | [SOUND] | [SOUND] |
| — | 03 friction F1-F8 (authoring stack) | F1 partially, F3 partially | F1-F5, F7 eliminated; F6 becomes capability-map lookup; F8 fixed in kit | same | F3, F5 eliminated; F1 partial; F2, F7 persist |

Tally of structural eliminations: C0 = 0; C1 = 6 (FM3/4/5/6/9/10); C2 = 6 (same, plus front-end leverage, plus discovery risk); C3 = 5 (FM5 only partial). FM1/FM2/FM7 are host-law families — no architecture eliminates them; C1-C3 shrink their FREQUENCY by removing most relaunch triggers and give every residual occurrence a name.

## [3]-[MIGRATION] — side-by-side, old tool as regression oracle

Mechanics of coexistence (both tools against one RhinoWIP):

- New tool builds in a sibling root `tools/rhino-bridge-next/` (Contract/Shell/Cargo/Supervisor). Old `tools/rhino-bridge/` untouched and runnable throughout.
- Both plugins install simultaneously: distinct plugin GUID, distinct pipe prefix (`rbx-` vs `rb-`), distinct endpoint file (`~/.rasm/rhino-bridge-next.json`). Shell coexists with `rasm-bridge.rhp` in the default ALC; neither sees the other.
- Lifecycle ownership is EXCLUSIVE during dual-run: only the new supervisor launches/quits/reconciles. The old client is invoked verb `check` only — its launch phase no-ops when hello succeeds against the live host. Old-tool `quit`/`launch` verbs are off-limits (two ladder owners racing is the one new failure mode dual-run could invent). One lease covers both (supervisor-owned; assay's old `bridge_lease` defers to it).
- Scenario corpus: each migrated theme exists twice until cutover — original `.verify.csx` (old tool) + new `[RhinoScenario]` method (new tool). A parity harness runs both against the same host serially and diffs verdict + facts per theme. Blocks (10 single-theme files) is the pilot module — highest file count, simplest assertions; Vectors and Exchange next; gh-ui last (it exercises FM4's probe machinery).
- The 04 dossier doubles as the fault-injection suite: deliberately induce each FM (compile error, assertion fail, UI hang, SIGKILL mid-scenario, planted recovery markers, stale endpoint, lock drift) and require the new tool to produce the named taxonomy entry.

Cutover criteria (all measurable, all gate the archive step):

| [#] | [CRITERION] | [BAR] |
| :-: | ----------- | ----- |
| 1 | Verdict parity | 27/27 themes equal status old-vs-new on 3 consecutive full batch runs |
| 2 | Hot reload | 10 consecutive cargo swaps (lib rebuild + bridge-logic change + dependency bump) with zero relaunches, zero dialogs; `unloadConfirmed` true each time |
| 3 | Fault naming | Injected-failure matrix (FM1-FM10 inductions) each yields its named fault; zero empty-`data` results; every failure carries facts-to-point-of-death |
| 4 | Inner loop | Scenario edit → warm re-run <10 s; full 27-theme batch wall-time ≤50% of old tool (one launch, one build consumption) |
| 5 | Soak | 2 weeks of real use with no fallback to the old tool for a capability reason |

On cutover: `tools/rhino-bridge/` → `.archive/rhino-bridge-v1/` with tombstone README; `tools/rhino-bridge-next/` → `tools/rhino-bridge/`; `.verify.csx` corpus deleted as each theme's parity record closes; old plugin uninstalled via yak at the next natural relaunch.

## [4]-[RECOMMENDATION] — C1, with C2 as the committed phase-5 front end

C1 (SUPERVISOR) is the recommendation. Rationale in one paragraph: it is the only candidate that eliminates all four rebuild-only generators (D1 via shell/cargo, D4 via repo-compiled scenarios, D5 via StreamJsonRpc bidirectionality, D7 via the session-owning supervisor) while shipping ZERO new in-host compiler risk (no Roslyn in the host, no RhinoCode on the compile path, no MTP discovery protocol), keeps every [SOUND] decision verbatim, lands the linkage inversion exactly as 02 §7 prescribed (one typed JSON document to a ~150-LOC assay rail; both regex scrapers, both msgspec mirrors, both step-policy tables deleted), and leaves both deferred candidates reachable without rework — C2 bolts onto the session API, C3's compiler drops into cargo as a REPL lane.

### Phased build outline (input to the implementation wave)

| [PHASE] | [DELIVERABLE] | [CONTENT] | [PROOF / GATE] |
| :-----: | ------------- | --------- | -------------- |
| 0 | Probe verdicts | (a) collectible-ALC load/unload of current Rasm libs in live host, gcdump on failure; (b) GH2 `Start(Headless)` + `Phase` poll + `Tree()` read; (c) `dotnet-counters ps` against RhinoWIP (EventPipe enabled?); (d) per-invocation `dotnet run` cost benchmark (daemon-vs-batch data) | (a) sizes the cargo disposal contract — if Rasm statics pin the ALC, scope a disposal pass into Phase 3; (b) decides computed-value oracles vs render-only GH2 lane; (c)/(d) size diagnostics + supervisor process model. Probes run via the OLD tool (it exists for exactly this) |
| 1 | `Rasm.Bridge.Contract` + `Rasm.Bridge.Shell` v1 | StreamJsonRpc interface pair; version/capability hello; ported DTO vocabulary + status algebra; shell = endpoint + idle marshal + ALC manager + GH2 preload + `OnExceptionReport` + `shutdownStarted`; side-by-side install (new GUID/pipe/endpoint) | Shell deploys once; hello + version negotiation + loadCargo/unloadCargo round-trip green against a stub cargo; old tool unaffected |
| 2 | `Rasm.Bridge.Supervisor` v1 | Session state machine; port-verbatim list (AE ladder, reconcile placement, endpoint validation, NU-drift classification); kqueue NOTE_EXIT; bundle discovery; lease; `.ips` snapshot-diff; `redeploy` transaction; consumes rail-built outputs (no embedded build pipeline) | Fault-injection matrix subset: launch-fail, dialog-planted, kill-mid-connect, stale endpoint — each produces its named fault |
| 3 | Cargo + scenario kit v2 + pilot corpus | `Rasm.Bridge.Cargo` (invoke, FactBag v2 with fused assert+fact, JSONL spool, auto-capture-on-failure, shared fixture module); `[RhinoScenario]` attribute + per-lib `Rasm.Rhino.Scenarios.csproj` pilot; Blocks themes migrated; parity harness | Pilot themes verdict-parity vs old tool; `unloadConfirmed` across a 10-swap soak; crash-durable evidence demonstrated by killing the host mid-scenario |
| 4 | Full migration + assay rail v2 | Remaining themes (Vectors, Exchange, Camera, UI, gh-ui); `rails/bridge.py` rewritten to thin consumer; capability map doc seeded from 03's F6 inventory; full cutover-criteria run | §3 criteria 1-5 green → archive old tool |
| 5 | Beyond-parity wave | Watch mode; GH2 computed-value lane (per Phase 0b) + `.ghz` golden-diff lane; MTP framework adapter over the session API (C2); optional cargo-resident eval/REPL (C3's compiler, in cargo); EventPipe artifacts (per Phase 0c) | Each capability lands with its own scenario-grade proof; MTP adapter passes the same parity harness as the CLI front end |

Sequencing rule: phases 1-2 are buildable before Phase 0 completes EXCEPT the cargo ALC design (gated on 0a) and the GH2 lane shape (gated on 0b). If 0a fails hard (Rasm statics un-fixably pin the ALC), the fallback inside C1 is per-session host recycling with the supervisor's relaunch made cheap and fully diagnosed — degraded but still strictly better than today; the shell/session/evidence layers are unaffected.
