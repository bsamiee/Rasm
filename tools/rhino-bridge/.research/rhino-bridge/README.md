# [00] rhino-bridge Research — Index + Verdict

Research corpus on `tools/rhino-bridge`, produced 2026-06-10 by a staged workflow (map -> external research -> synthesis -> red-team audit). Read 09 -> 10 for the decision; 04 + 03 for the pain; 01-02 for tool ground truth; 05-08 as landscape reference; 11 as the audit trail. Citations to `reference_*`/`feedback_*` are private agent-memory files; the load-bearing ones are restated inline where cited.

## [THE_PROBLEM]

Rhino has no headless mode on macOS, so the only way to execute RhinoCommon + GH2 code is in-process inside a live `RhinoWIP.app` — the bridge does that: 3 C# projects (protocol/plugin/client, 2,158 LOC) plus python rails (01, 02).
The approach is sound and now vendor-blessed (McNeel ships three converging in-host automation surfaces — 05), but the implementation is fragile in four structural ways:
scenarios are a third C# dialect (RhinoCode's C# 10 pin, invisible rewrites, no IDE/analyzers/IVT — 03); every bridge change rides a monolithic `.rhp` through the quit -> yak -> relaunch dialog gauntlet (04 FM1-FM3);
the one-shot unversioned wire cannot stream, cancel, or report during execution, and evidence is regex-scraped stdout markers capped at 32 KiB (04 FM4/FM6/FM7); and no component owns the session — a fresh `dotnet run` + build + 3-8s handshake per scenario (04 FM9/FM10).
59 fix commits in 24 days patched expressions of these decisions without retiring any of them — in-place hardening has empirically hit its ceiling (04 §1, 09 §4).

## [VERDICT]

[REBUILD WARRANTED] — scoped, with the old tool kept side-by-side as the regression oracle (09 §4, 10 §3). Of twelve design decisions, the four dominant failure generators (D1 monolithic plugin, D4 RhinoCode dialect, D5 one-shot wire, D7 stateless client) are each [REBUILD-ONLY]; what remains fixable in place does not touch the operator's three named pains. Keep-and-harden (C0) is the migration fallback and parity baseline, not a destination. The in-host approach itself is [CONFIRMED] — no macOS alternative exists or is on any roadmap (09 §3).

## [RECOMMENDED_ARCHITECTURE] — C1 SUPERVISOR (10 §4)

| [COMPONENT] | [LIVES] | [OWNS] |
| ----------- | ------- | ------ |
| `Rasm.Bridge.Contract` | both sides, frozen + versioned | StreamJsonRpc interfaces, DTOs, status algebra, version negotiation |
| `Rasm.Bridge.Shell` (`.rhp`) | Rhino default ALC, near-zero change | pipe endpoint, UI marshal, collectible-ALC lifecycle, GH2 preload, `OnExceptionReport` tap |
| `Rasm.Bridge.Cargo` | collectible ALC, hot-swapped per session | scenario load/invoke, FactBag/captures, crash-durable JSONL evidence spool |
| `Rasm.Bridge.Supervisor` (exe) | workstation | session state machine: lease, launch/quit ladder, kqueue exit watch, `.ips` diff, `redeploy` transaction |

Scenarios become ordinary repo C# 14 — `[RhinoScenario]` entrypoints in per-lib `*.Scenarios.csproj`, compiled by the repo toolchain with analyzers/IVT/IDE/PDBs; RhinoCode leaves the compile path entirely (the dialect dies). Bidirectional RPC gives live fact/progress streaming plus heartbeat; relaunch is confined to shell/GH2/host changes — bridge logic, libs-under-test, and dependency bumps all hot-swap via cargo. Python shrinks to a ~150-LOC thin assay consumer of one typed JSON document. Structurally eliminates 6 of 11 failure families (FM3/4/5/6/9/10); host-law families (FM1/FM2/FM7) shrink in frequency and every residual occurrence gets a named fault. C2 (MTP `dotnet test` adapter) is the committed phase-5 front end over the same session API; C3's in-host compiler is a future cargo REPL lane. Beyond-parity capabilities (watch mode, GH2 computed-value oracles, `.ghz` golden-diff, dialog naming, EventPipe artifacts) are reachable only on this substrate (10 §1.2).

## [DOC_MAP]

| [DOC] | [ONE-LINE SCOPE] |
| ----- | ---------------- |
| `01-current-architecture.md` | Line-cited map of the 3-project C# tool: wire shapes, phase fold, lifecycle verbs, doc-vs-code contract drift |
| `02-python-linkage.md` | How two near-duplicate python stacks consume the client; package-update choreography + swallow points; linkage-direction answer |
| `03-scenario-authoring.md` | Scenario corpus census (21 files / 27 themes) + the third-dialect root cause of authoring fragility; exit routes |
| `04-failure-modes.md` | Empirical dossier: 11 failure families (FM1-FM11) with trigger/blast-radius/mitigation; 59-commit fix cadence |
| `05-rhino9-landscape.md` | Rhino 9 WIP / macOS landscape: no headless path; rhinocode/RhinoMCP convergence; plugin reload law; local bundle facts |
| `06-gh2-landscape.md` | GH2 state: solver decompile (contradicts the "never settles" memory — probe-gated); GrasshopperIO `.ghz` diffing |
| `07-dotnet10-patterns.md` | .NET 10 rebuild patterns: collectible ALCs, in-host Roslyn options, StreamJsonRpc/UDS, kqueue/EventPipe diagnostics |
| `08-prior-art.md` | In-host bridges elsewhere (ricaun.RevitTest, Rhino.Testing, compute.frontend, Gauntlet, MTP): transfers + anti-patterns |
| `09-critique.md` | Root cause: 12-decision register with verdicts; named verdicts on split/wire/dialect/reload/linkage/diagnostics; rebuild call |
| `10-candidate-architectures.md` | Candidates C0-C3, FM scorecard, side-by-side migration + cutover criteria, recommendation C1, phased build outline |
| `11-redteam-addendum.md` | Adversarial audit: 25-claim verification matrix, corrections applied, gaps G1-G6, currency audit, cold-read grade A- |

## [OPERATOR_INPUT_REQUIRED] — before an implementation wave

| [#] | [QUESTION] | [REF] |
| :-: | ---------- | :---: |
| 1 | Authorize live-host time for the two gating Phase 0 probes: (a) collectible-ALC unload of current Rasm libs (sizes the cargo disposal contract; fallback if it fails hard is per-session host recycling); (b) GH2 `Start(Headless)` settle (decides computed-value oracles vs render-only GH2 lane) | 10 §4 P0, 11 §7 |
| 2 | WIP-churn policy (G1): pin the RhinoWIP build per parity phase? A host service release can break both tools at once and invalidate parity baselines mid-migration | 11 §4 |
| 3 | Effort acceptance (G5): C1 has no LOC/calendar estimate while C0 does (~400-600 LOC); order-of-magnitude is 1.5-2.5x the current 2,158-LOC C# body + 21-file corpus migration — approve before Phase 1 commits | 11 §4 |
| 4 | Capture one observed reproduction of the "error-ignoring windows" pain naming the exact dialog (G6) — the last unevidenced failure mode; inferred to be plugin-load/package-manager dialogs mid-FM3 | 04 §5, 11 §4 |
| 5 | Bless the dual-run mechanics: exclusive lifecycle ownership by the new supervisor, distinct GUID/pipe/endpoint, cutover criteria 1-5, archive step | 10 §3 |
| 6 | Phase 1 design rule (G4): shell-private ALC for the StreamJsonRpc dependency closure, leaving only the `.rhp` stub + Contract DTOs in the default context | 11 §4 |

## [SEQUENCING]

Implementation is a FUTURE wave: it begins only after the assay and tests waves close. Throughout that wave the original `tools/rhino-bridge/` remains installed and runnable side-by-side (new tool in a sibling root, distinct plugin GUID/pipe/endpoint) as the regression oracle; the old tool is archived only when the five cutover criteria in 10 §3 are green. Nothing in this corpus mutates the tool today.
