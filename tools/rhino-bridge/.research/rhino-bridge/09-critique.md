# [09] Root-Cause Critique — tools/rhino-bridge

Synthesis over docs 01-08 answering the operator's question: "understand the issue we have, and why we have it." Every load-bearing claim re-verified against working-tree source 2026-06-10:

| [CLAIM] | [SITE] | [STATUS] |
| --- | --- | --- |
| `ServerExecutionCancelable: false` hard-coded | `plugin/Rhino/Transport.cs:254` | [VERIFIED] |
| `BridgeRequest.TimeoutMs` written by client, never read by plugin | `protocol/BridgeWire.cs:133,363-365`; zero plugin reads | [VERIFIED] |
| C# 10 ceiling is a RhinoCode pin, not a bridge choice | `plugin/Rhino/CodeEngine.cs:31` (comment + `RunContext` shape) | [VERIFIED] |
| No `schema`/version field anywhere on the wire | zero `schema` hits in bridge C# | [VERIFIED] |
| quality verify swallows launch failure | `tools/quality/rails/bridge.py:364` (`check=False`, result discarded) | [VERIFIED] |
| `assay bridge clean` cannot succeed (arity drift) | `tools/assay/rails/bridge.py:532` sends bare `clean`; `client/ClientVerb.cs:56` requires `clean <target>` | [VERIFIED] |
| Evidence truncation cap | `protocol/BridgeWire.cs:295` (`OutputLimit = 32768`) | [VERIFIED] |

## [1]-[DECISION_REGISTER] — the designs that generate the failures

Eleven failure families (04) trace to twelve design decisions. Bugs express the decision; the decision is the unit of verdict.

| [ID] | [DECISION] | [GENERATES] | [VERDICT] |
| :--: | ---------- | ----------- | :-------- |
| D1 | All bridge logic in one `AtStartup` `.rhp` in Rhino's default ALC | FM3 directly; FM1/FM2 frequency (every bridge change traverses the quit gauntlet) | [REBUILD-ONLY] |
| D2 | Host lifecycle as best-effort side effects of `launch`/`quit` verbs: swallowed escalation results (`closed:false` → phase Ok, JXA exit discarded, `ForceKill` true-on-exception), no supervisor state machine, no dialog diagnosis | FM1, FM2 blast radius, the §4.1 stale-host 90s-timeout class in 02 | [REBUILD-ONLY] |
| D3 | Apple-Event terminate → SIGKILL quit ladder + pre-launch crash-marker reconcile | mitigates FM1/FM2; empirically corrected 2026-05-29 | [SOUND] — forced by host (`RhinoApp.Exit(false)` and SIGTERM both self-SIGABRT); port verbatim |
| D4 | RhinoCode as sole scenario compiler + client-side textual rewrite (preamble hoist, injected `#r`/usings/consts/bootstrap) with no source map | FM5 entire; F1/F2/F5/F7 of 03; the 15-rule private-memory contract | [REBUILD-ONLY] — the dialect cannot be patched away while RhinoCode owns compile; offline C#10 pre-flight is palliative only |
| D5 | One-shot request/reply wire: no server→client channel, no heartbeat, no cancellation, three uncoordinated timeout authorities (35s client / 180s python / 300s server) | FM4 (settle-probe band-aid), FM7 | timeout unification [FIXABLE-IN-PLACE] (client deadline becomes authoritative, others derive); heartbeat/notification channel [REBUILD-ONLY]; non-cancelable UI-thread work itself [SOUND] — host law, every prior-art system accepts it (08 §4[9]) |
| D6 | Evidence as stdout marker lines inside the JSON protocol, re-parsed by regex in two python stacks, capped at 32 KiB, lost on crash | FM6 entire; asymmetric fact loss (01 F1) | typed facts on the wire [FIXABLE-IN-PLACE] (plugin already holds typed `FactBag`); crash-durable disk spool + live streaming [REBUILD-ONLY] |
| D7 | Stateless process-per-scenario client: fresh `dotnet run` + build + handshake per scenario; no session/batch verb; batching is authoring folklore | FM10; FM9 repetition; N× failure multiplication on dead host; the 3-8s handshake tax | [REBUILD-ONLY] — the session concept is the rebuild's spine; bolting a batch verb onto the current client still leaves lease/launch/build ownership split |
| D8 | Client embeds its own restore/build/msbuild pipeline with independent lock-mode policy | FM9; duplicate build truth vs static rail; ~3s rebuild of the same csproj per scenario (04 §3) | [FIXABLE-IN-PLACE] — consume rail-proven outputs or cache per closure |
| D9 | No wire versioning; client/plugin must deploy in lockstep; README documents a `schema` field, per-name timeout knobs, and `--timeout-ms` that code never had | silent skew → `JsonException`/null fields; doc drift on own contract (01 F2) | [FIXABLE-IN-PLACE] — one field + skew tolerance; mandatory in any rebuild |
| D10 | Endpoint-file discovery (`~/.rasm/rhino-bridge.json`) + PID/start-time liveness + 250ms hello polling | FM8 residual staleness class | [SOUND] — defensive validation landed (`75d740adc`); a plugin readiness signal is polish, not architecture |
| D11 | Mutual exclusion (flock lease) lives in each python wrapper, not at the resource; C# client has no locking | direct-client invocations race quit/launch; guard and capability live in different processes (02 §3) | [FIXABLE-IN-PLACE] mechanically, but lands naturally in the D7 rebuild |
| D12 | GH2 pre-load into default ALC via `PlugIn.LoadPlugIn` + `CompileReference.FromAssembly` injection | resolved FM11 (root fix, 1/8 → 6/8 in one commit) | [SOUND] — keep; 06 confirms the isolated-ALC discipline is structurally unchanged |

FM-to-decision index: FM1→D2 (ladder content D3 sound); FM2→D1+D2 (reconcile backstop D3 sound); FM3→D1 (+ Rhino-layer structural fact, 05 §6); FM4→D5; FM5→D4; FM6→D6; FM7→D5(+D7 lease semantics); FM8→D10; FM9→D8(+D9); FM10→D7; FM11→D12 [RESOLVED].

The headline reading: four decisions — D1, D4, D5, D7 — generate or amplify eight of eleven failure families (FM1-FM5, FM7, FM9, FM10 per the register's GENERATES column; FM6, FM8, FM11 trace to D6, D10, D12). All four are [REBUILD-ONLY]. The 59-commit fix cadence (04 §1) is the empirical proof that patching expressions of these decisions does not retire them: two dispatch redesigns, three lifecycle-hardening passes, and a correction-of-a-correction all landed without touching any of the four generators.

## [2]-[NAMED_VERDICTS]

### [2.1] The 3-project split (protocol / plugin / client)

[SOUND-SHAPE, WRONG-CONTENTS]. The topology is convergent, not idiosyncratic: ricaun.RevitTest ships the identical adapter/console/application triple over pipes; McNeel's own rhinocode CLI is a thin client to an in-host script server (08 §2.3, 05 §2). The defect is content allocation, not count: the client is a 976-LOC god-process (build system + lifecycle + codegen + phase fold + JXA process control), the dependency-free protocol assembly is sound but its shapes are re-declared three more times downstream (client mirror, two msgspec mirrors), and a fourth and fifth implicit "project" exist in python (quality ~795 LOC, assay ~1,188 LOC, near-duplicates). A rebuild keeps three components with redrawn boundaries: frozen shell `.rhp`, swappable cargo (bridge logic + scenario kit in a collectible ALC), session-owning supervisor client. Collapsing to one project would be wrong (the `.rhp`/client process boundary is real); the failure is that the boundaries do not align with rates of change.

### [2.2] The custom wire protocol

Framing (length-prefixed JSON), status algebra (`PhaseStatus` rank-max fold), and endpoint handshake: [SOUND] — small, stable, never the source of an FM. Three defects ride on top. (a) The RPC layer is hand-rolled where StreamJsonRpc 2.25.x covers the concern over the same pipe — by this repo's own `LIBRARY_DEPTH` law that is a violation, and it is also why no notification channel exists (D5). (b) Zero versioning (D9) while the deploy model demands lockstep — the worst possible pairing. (c) The real protocol is not the JSON: it is the `rasm.rhino-bridge.*` stdout marker grammar smuggled inside it (D6), a string protocol inside a typed protocol, decoded twice downstream. Verdict: keep the DTO vocabulary and status algebra; replace the envelope/dispatch layer with a bidirectional RPC library in the rebuild; add a version field immediately regardless.

### [2.3] Scenario `.csx` + the C# version ceiling

[REBUILD-ONLY], and this is the fragility epicenter (03's verdict, confirmed). The ceiling is McNeel's `CSharpVersion` pin inside RhinoCode — so any design that keeps RhinoCode on the compile path keeps the third dialect: C# 10, shadow refs, no analyzers, no IVT, no IDE, diagnostics pointing at a rewritten file the author never wrote, and a 15-rule contract living only in private agent memory. The decision was rational at inception (RhinoCode is the sanctioned in-host engine; fastest path to first scenario) and is now the dominant cost center: both 2026-06-02 compile failures, the folklore-drift class (one rule — "GH2 solver never settles" — is now contradicted by 06's decompile), and the entire F1-F3 friction stack. Exit routes, in preference order: (1) attribute-discovered scenario assemblies compiled by the repo toolchain at C# 14 with analyzers/IVT/PDBs, bridge reduced to load-into-cargo-ALC + invoke (03 §5.1-A; the architecture Rhino.Testing already validates on Windows); (2) in-shell plain `CSharpCompilation` at `LanguageVersion.Latest` into the cargo ALC (07 §2 Option B, ~100-150 LOC; avoid CSharpScript — `InteractiveAssemblyLoader` leaks per run). If a rebuild were deferred, the only worthwhile in-place move is an offline Roslyn pre-flight compile at C# 10 against the staged refs — it takes Rhino out of the inner loop but preserves the dialect.

### [2.4] Package-update → quit → relaunch methodology

Split verdict. The Rhino-layer facts are immovable: yak takes effect next start, `.rhp` plugins are unloadable by design, no collectible-ALC story exists for plugins (05 §6) — so relaunch-as-last-resort is [SOUND] and the deploy steps `quit → install → refresh` are the correct choreography for what they cover. What is [REBUILD-ONLY] is the exposure: D1 makes EVERY bridge change a plugin change, so the gauntlet (FM1 → FM2 → dialog roulette) runs far more often than the host requires. The shell/cargo split confines relaunch to shell-protocol and GH2 changes; per-ALC copies of LanguageExt/Thinktecture even make dependency bumps hot-swappable (07 §1). Independently [FIXABLE-IN-PLACE]: the choreography's swallow points — `closed:false` reported as Ok, JXA exit code discarded, quality's uncaught stage `copy2`, and the absence of any "process alive but pipe silent ⇒ probable modal dialog" diagnosis that turns the operator's worst failure (recovery dialog) into a nameable fault instead of an opaque 90s timeout.

### [2.5] Linkage direction

The operator's suspicion ("bridge wired into the python tool may be backwards") is half-right, and right for a different reason than implied. The code refutes "intelligence in python, C# dumb pipe" — the inverse holds: the client is the smartest layer; python contributes lease/discovery/decode/fold/yak, declared twice in near-duplicate stacks with four total declarations of one wire shape and verified behavioral drift (launch-swallow fixed in assay only; `clean` arity broken in both; bundle discovery dropped in migration). What is genuinely backwards is that the SESSION has no owner — it is sliced across the plugin gate, five client verbs, and two python leases, joined by a lossy text seam. Verdict: invert the seam, not the stack. Bridge-side gains: lease/single-instance, bundle discovery + launch policy, N-scenario session verb, typed evidence end-to-end, a `redeploy` transaction verb. Python keeps: repo routing conventions, the Assay envelope fold, yak staging for non-bridge slugs — a ~150-LOC thin typed consumer. The bridge should NOT become assay-native: binding host knowledge into workspace tooling re-couples the layers in the other direction and kills direct-client use; 08's stronger long-horizon answer is fronting the bridge with an MTP adapter (the repo's existing test platform), making python orchestration optional rather than load-bearing.

### [2.6] Diagnostics architecture

Thin by construction, not by neglect — five structural causes: (1) one-shot request/reply means nothing can be reported DURING execution, so deferred crashes need the 4s settle heuristic and hangs surface as naked pipe cancellations; (2) the evidence channel is truncation-capped stdout text that dies with the connection (anti-pattern 08 §4[1]); (3) failure paths drop data (`durationMs: 0`, empty `data:{}` in 2 of 6 surviving runs); (4) artifacts expire in 300s, so failure history evaporates faster than a debugging session; (5) the states that matter most — recovery dialog, plugin-load error window, `.ips` content — are visible only OUTSIDE the host, and nothing looks. Verdict: [REBUILD-grade]. Target shape from 07/08: bidirectional notifications (live facts/progress), plugin-side disk-spooled per-scenario evidence (crash-durable), supervisor-side `.ips` snapshot-diff + optional window screenshot, EventPipe attach by PID (zero plugin code), PDB-real stack traces from self-compiled scenarios, failure-run retention ≫ green-run retention. One existing asset must survive any rebuild: the `HostUtils.OnExceptionReport` capture — the only window into UI-thread-swallowed faults.

## [3]-[FUNDAMENTAL_APPROACH_CHECK]

The operator's premise — on macOS, an in-process bridge inside live RhinoWIP is the only way to run RhinoCommon + GH2 code — is [CONFIRMED] by the landscape evidence:

| [ALTERNATIVE] | [STATUS 2026-06] | [VERDICT AS REPLACEMENT] |
| --- | --- | --- |
| Headless Rhino on macOS | Does not exist; no roadmap item; Rhino 9's headless expansion is Linux compute only (2026-03, non-production) | [NO] |
| Rhino.Testing | Windows-only by construction (Rhino.Inside dependency, `net*-windows` TFMs) | [NO] — bookmark for a future Windows/Linux CI lane |
| rhinocode CLI | In-host script server exists (Rhino ≥8.11) but: no args/stdout/exit-code/artifact contract, server off by default, WIP binary mis-targeted (net8 vs bundled .NET 10) | [NO] — but it is McNeel's own validation of the bridge topology |
| compute.rhino3d / rhino3dm | Geometry-only; no doc/UI/viewport/Eto/GH2-canvas surface; no mac server | [NO] |
| RhinoMCP (official, 2026-05) | In-host automation on mac: script exec, viewport capture, GH1/GH2 reach — an official PEER, alpha 0.1.5, no typed protocol or repo-assembly story | [NO] as replacement; [YES] as architectural confirmation |

McNeel itself now ships three in-host automation surfaces (rhinocode script server, in-app `test_*` panel, RhinoMCP), all converging on the bridge's shape: thin in-host listener, external driver, per-run payloads. The approach is not merely viable — it is the vendor-blessed pattern.

Within the approach, the essential/incidental partition:

- [ESSENTIAL — sound core, keep]: in-host execution on the UI thread of a live RhinoWIP; out-of-process client + thin in-host server over a local pipe; endpoint discovery with PID/start-time liveness; phase/status algebra with first-non-ok taxonomy; per-run-compiled payloads (no test discovery outside the host — 08 §4[5]); GH2 default-ALC preload discipline; AE-quit ladder + marker reconcile.
- [INCIDENTAL — complexity the approach does not require]: RhinoCode as compiler (the dialect); stdout-marker evidence channel; monolithic `.rhp`; process-per-scenario client and client-owned builds; two duplicated python stacks and four wire-shape declarations; unversioned wire; lifecycle as verb side effects rather than a supervised state machine.

Everything the operator experiences as fragility lives in the incidental column.

## [4]-[REBUILD_VERDICT]

[REBUILD WARRANTED] — scoped, with the old tool kept side-by-side per the operator's constraint.

Justification:

1. The four dominant failure generators (D1, D4, D5, D7) are each [REBUILD-ONLY]: shell/cargo plugin split, dialect removal, bidirectional RPC, and a session-owning supervisor are topology changes, not patches. The git record demonstrates the alternative: 59 commits in 24 days fixed expressions of these decisions repeatedly without retiring any of them, including a shipped-wrong termination ladder corrected a week later. In-place work has already hit its ceiling — what remains fixable in place (D8, D9, D11, choreography swallows) does not touch the operator's three named pains.
2. Measured against `docs/stacks/csharp` doctrine, the current tool is below the repo's own bar in ways that are architectural, not cosmetic: `SHAPE_BUDGET` — one wire concept declared four times across two languages; `LIBRARY_DEPTH` — hand-rolled RPC envelope where StreamJsonRpc owns the concern, plus a 60-line C# program built by string concatenation (`SmokeTemplate`); `ONE_HOP_RESOLUTION` — four verb vocabularies between assay and the wire; `BOUNDARY_ADMISSION` — typed facts degraded to regex over truncated text at the boundary that matters most; `DEEP_SURFACES` — the session concern fragmented across three processes and two languages. The `ROOT_REBUILD` law itself prescribes the remedy: reshape the owner, break the API, no shims.
3. Honest counterweights, stated and weighed: the tool is small (2,158 LOC C#); the 4-command wire core has been stable since inception; several families are already root-fixed (FM11, the StatusBar crash, endpoint validation); and repo memory warns that collapse refactors must beat break-even math (`feedback_op_result_collapse_break_even`) and that SHAPE questions precede collapse (`feedback_bridge_shape_question_first`). None of these flip the verdict, because this is precisely a SHAPE question: the rebuild is not density-chasing within files, it is moving four boundaries (plugin/cargo, compile-site, RPC direction, session ownership) that in-place edits cannot reach without rewriting the same files anyway. The small LOC count lowers rebuild cost; it does not lower the cost of keeping the generators.
4. Capabilities beyond parity become reachable only post-rebuild: live fact/progress streaming; computed-value GH2 oracles if the `Start(Headless)` probe in 06 succeeds; `.ghz` golden-file + archive-diff assertions via GrasshopperIO (no solving required); hot-swap of libs-under-test AND dependency bumps without relaunch; dialog-suspected fault naming; EventPipe/gcdump evidence.

Port-verbatim list (hard-won, empirically corrected — do not rediscover): AE-terminate → SIGKILL ladder and its ordering; pre-launch marker-reconcile placement (the async `.ips` race); endpoint liveness validation (PID + start-time tolerance + pipe-name prefix); GH2 preload + `FromAssembly` injection; `OnExceptionReport` capture; NU1004/NU1403 lock-drift classification; try/finally evidence-flush semantics; the phase vocabulary and first-non-ok taxonomy.

Sequencing gate: two cheap live probes de-risk the two riskiest rebuild bets before any code — (a) collectible-ALC unload of current Rasm libs (gcdump on failure names the rooting path; sizes the disposal-contract work); (b) GH2 `Start(SolutionMode.Headless)` + `Phase` poll + `SolutionData.Tree()` read (decides whether the new bridge owes computed-value oracles or inherits the render-only ceiling).

## [5]-[CROSS_DOC_CORRECTIONS]

- 03's authoring rule 10 ("GH2 solver never settles headless; `StartWait` deadlocks") is partially contradicted by 06's decompile of the current build (`Start` runs the whole solve on a threadpool worker; `ExpiredCount` was the wrong oracle). The folklore-drift finding thus applies to the constraint list itself — the rebuild must re-verify every host ceiling empirically before encoding it.
- 04 §4.3's recommendation (client-side C# 10 pre-flight compile) is an interim palliative only; 07 establishes the ceiling is RhinoCode policy, so the rebuild target is dialect removal, not dialect emulation. Do not let the cheap fix defer the structural one.
- 01 F2 (README documents `schema: rasm.rhino-bridge.v1`, per-name timeout env knobs, `--timeout-ms` — none implemented) is itself diagnostic: contract documentation drifted because no versioning discipline forces the contract to be a single artifact. The rebuild's protocol definition should be the generated source of both code and doc.
