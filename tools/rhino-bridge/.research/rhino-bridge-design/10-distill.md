# [10] Distill — The Rebuild Charter, One Page

Design-wave task D10, 2026-06-11. The corpus is COMPLETE: review 01-11 (grade A-) + design 01-11. This page is what the implementation session codes from; everything below is settled and cited — relitigation requires NEW evidence filed as a corpus amendment with both citations.

Why the tool exists, restated once for the implementing session: Rhino has no headless mode on macOS and no COM/automation lane (review README; 08 prior-art — Rhino.Testing is Windows-only, Rhino.Compute has no macOS), so in-process execution inside a live `RhinoWIP.app` is the ONLY way to verdict RhinoCommon + GH2 code — the bridge IS that channel, and `tools.assay` rides it (`rails/bridge.py`). The official Rhino MCP platform is a COMPLEMENT for the exploration lane, never the substrate (mcp verdict; §1 MCP row). The binding frame is agent-first/agent-only: every behavior hardened, every diagnostic decision-useful to an agent automatically (07 §7; 11 L1-L6).

## [1]-[SETTLED] — do not reopen

| [AREA] | [DECISION] | [WHERE] |
| ------ | ---------- | ------- |
| Architecture | C1 SUPERVISOR: Contract / Stub / Shell / Cargo / Supervisor + thin assay rail; old tool side-by-side as regression oracle until cutover | 10 §4; 07 §1 (13 files) |
| Wire | Contract declares every wire concept ONCE (Thinktecture owners + `[JsonDerivedType]` unions); StreamJsonRpc 2.25.25 / `SystemTextJsonFormatter` over the UDS pipe; additive-evolution law | model §2, §2.6; charter §3 |
| Reconciliations | Thinktecture IN Contract; NO Fin in shell — both RATIFIED by audit | model §0; 09 §4 |
| Envelope | `SessionEnvelope` + `FirstFaultPhase` [AMENDED]; `PhaseStatus` total order ok/skipped(1)<unsupported(2)<failed(3)<timeout(4)<busy(5), rank-1 tie deliberate with fold tie-law pinned [AMENDED] | 07 §2; 09 B1/A4; §2.13 |
| Manifest | `CargoManifest` carries `SessionId` + `ReportDir` [AMENDED]; `LoadCargoAsync` invoked every session (hash gates the swap, never the call); endpoint pipe prefix = one Contract constant `rbx-` [AMENDED] | 07 §2 AMENDED-3/4; §2.9-.10 |
| Session | One `SessionState` `[Union]`, every non-terminal state deadline-rowed; lease at the resource with staleness reclaim; shell busy admission | 07 §5.2-§5.5; 09 A1-A3 |
| Quit | quit.ae (Apple Event) → quit.force (`forceTerminate` = Cocoa SIGKILL) → quit.kill (`kill(2)`); SIGTERM banned every rung; `closed:false` fails the rung | 07 §5.5; 09 D1 |
| Reconcile | Pre-launch, instance-scoped via the quit journal; foreign markers skipped + reported; marker names derive from `CFBundleName`/`CFBundleExecutable` | 07 §5.5; tolerance §1; 09 D2 |
| Evidence | ONE `BridgeEvent` family; shell-assigned sequence; spool+stream+envelope are three folds; terminal spool reconciliation fact; captures auto-on-failure + tiered retention (7d/prune) | model §2.3; 07 §4.2, §5.4, §8 |
| Scenarios | Repo C# 14 `[RhinoScenario]` in per-lib `*.Scenarios.csproj`; in-host post-load discovery; GH2 only via `Gh2Lane` (raw GH2 naming = lint, not loader error) | 10 §1.2; 07 §4.3, §6; 09 E2 |
| Closure | Build-time `bridge-closure.json` target; supervisor never runs MSBuild; `HostPlugins` GUIDs derive from the closure | 07 §5.6; 09 C1/E4 |
| Verbs | 4 supervisor verbs (verify/doctor/redeploy/quit) + rail-owned `build`; assay registry 7→5 Bind rows; `launch`/`check`/`clean` deleted (arity defect dies with the verb) | features §0; 08 §5 |
| Assay | bridge.py 557→~150-180 LOC (verb collapse REQUIRED for ~150); one msgspec decode, `$type` folds, no mirrors; `first_fault_output` deleted; PNGs registered; package.py bridge arms deleted (refresh defect dies) | 08 §2-§5 |
| MCP | COMPLEMENT, re-corroborated 2026-06-11; coexistence wired at 3 sites + `mcp.listener` doctor probe; stdio facade phase-5+, demand-gated | mcp §4-§5; 07 §9; 09 F1-F3 |
| Cut list | 26 features stay cut (features §2) — re-adding any is a charter violation | features §2 |
| Port-verbatim | All 8 obligations re-housed, none re-derived | charter §7; 09 R14 |

## [2]-[AMENDMENTS_REGISTER] — corpus corrections, both citations each

1. `tools/quality` is GONE → `.archive/quality` (corpus cites of `tools/quality/rails/bridge.py` are historical; live rail = `tools.assay`, non-bridge parity closed 2026-06-11). This includes 05 §5's graph-proof command: `uv run python -m tools.quality static full` is dead — the live command is `uv run python -m tools.assay static full`. [08 header vs review README/01-04; 05 §5]
2. Client LOC redistributed: Program.cs 708 (not 976), total 2,158 unchanged; 61 bridge commits (not 59). Cosmetic; god-process framing stands. [01 §1 vs current tree]
3. `SessionEnvelope.FirstFaultPhase` added; `VerifySummary.first_fault_output` deleted. [model §2.4 vs model.py:371-384; 07 §2]
4. `PhaseStatus` ranks made a strict total order. [model §2.1 vs BridgeWire.cs:13-38; 09 A4]
5. Probe 0b definition pins thread context (off-UI-thread solve wait) — the "never settles" memory and the decompile are both true in their regimes. [reference_gh2_headless_solution_limits vs 06 §2; 09 E3]
6. Model type count: 29 public/seam, 47 total (not 34). [model §1; 09 S2]
7. Exact-checkout line-cite drift (HEAD caf5f75b2): assay `Claim.BRIDGE` Bind rows at `registry.py:775-781` (not 779-785); `bridge.py` 557 / `package.py` 706 / `test_rail_bridge.py` 787 LOC. Cutover Tool row at `catalog.py:173` and `VerifySummary` fields at `model.py:376-382` (11 §4's original "EXACT at 174 / 377-383" claims were themselves off-by-one — caught and corrected in place 2026-06-11); the 7-field SET is exact — no verdict moves; deletion tables are range-anchored. [11 §4; current tree vs 08 §1/§2/§5]
8. Agent-first authorship contract: the rebuilt tool emits NO human-prose lane — help/errors/doctor/envelope/scenario docs are typed, fold-derived, single-declaration (laws L1-L6). Current tool inherits 3 README drifts (invented `--timeout-ms`/`schema` field, dead quality-stack aggregate shape) that the single-declaration law closes. [11 §1-§3; README.md:152-158,163,170,283]
9. `CargoManifest` gains `SessionId` + `ReportDir`; `LoadCargoAsync` is invoked EVERY session (hash equality short-circuits the swap, never the call); shell stamps SessionId with Sequence at publish; spool/capture paths derive from ReportDir. Without this no in-host writer has an evidence destination and `EventStamp.SessionId` has no source. [07 §2 AMENDED-3 vs model §2.4]
10. The endpoint pipe prefix is ONE named Contract constant, value `rbx-`; model §2.4's hard-coded `rb-` literal would make the rebuilt shell reject its OWN dual-run endpoint file (old tool's prefix is `rb-{pid}-`, Transport.cs:83). Permanent, no post-cutover revert. [07 §2 AMENDED-4 vs model §2.4; 10 §3 dual-run law]
11. 09's attack arithmetic corrected: 23 attacks, 13 VALID, 8 REFUTED, 2 NEEDS_LIVE_PROOF (header claimed 16/7/7 against its own §3 enumeration — the S2 defect class in the auditor itself); zero verdict moves. [09 header/§5 vs 09 §3]
12. Cutover criterion 1's "27/27 themes" does not reproduce: the live corpus is 21 `.verify.csx` files at BOTH the corpus freeze (`ece2f0c0d`) and HEAD (`caf5f75b2`); the binding criterion is N/N over the corpus enumerated at parity start (§5). [review 10 §3 vs `git ls-tree` both checkouts]
13. `PhaseStatus` tie law pinned: `Ok`=`Skipped` rank-1 tie is deliberate; `Worst` keeps the accumulator on ties, `SessionFold` seeds `Ok`; the skip signal is receipt-level, never the envelope root. [07 §2 vs 09 A4 "strict" phrasing]
14. The phase-5+ MCP facade names `ModelContextProtocol` 1.4.0 (official C# SDK), a package 05's plan does not carry. Registered as a deferred central-manifest admission that ships WITH the facade's demand gate; 05's closure stays complete and binding for Phases 1-4 — no Phase-1-4 build consumes the package. [05 §3-§5 absence vs 07 §9; 02 §5 source table]
15. Hello carries the StreamJsonRpc assembly version as the fact row `rpc.streamjsonrpc` — packages §7 OQ4 closed as a fact-row adoption with zero Contract shape change (fact keys are the sanctioned open vocabulary, R10). [05 §7 OQ4 vs 07 §2 pin 2]
16. `Handshake` carries NO dedicated rpc-version field: model §2.4's `RpcVersion` member and its "(05 OQ5)" cite are superseded — the carrier is the `rpc.streamjsonrpc` row riding the hello reply's `Capabilities[]` per #15, and packages §7 enumerates OQ1-OQ4 only (no OQ5 exists). [model §2.4 vs 07 §2 pin 2 / #15; 05 §7] [INLINE MARKER PLACED 2026-06-12: 06 §2.4 now carries the supersession at the definition site — the trap is closed at source.]
17. Line-cite drift outside assay files: 05 §3.1's `Directory.Packages.props:73` for the `Microsoft.VisualStudio.Threading.Analyzers` 17.14.15 pin reads :57 at the corpus freeze and :125 at HEAD caf5f75b2 (the props file grew +88 lines post-freeze); the pin fact holds — same anchor-drift noise profile 11 §4 establishes. [05 §3.1 vs Directory.Packages.props:125]

## [3]-[PHASE_0_GATE] — four live probes, the session's first action (run via the OLD tool; only 0a/0b BLOCK build code — §4 sequencing law)

| [PROBE] | [DEFINITION] | [DECIDES] | [FALLBACK] |
| :-----: | ------------ | --------- | ---------- |
| 0a | Collectible-ALC load/unload of current Rasm libs in the live host; gcdump on unload failure | cargo disposal contract (the ONLY structural unknown — 09 E1) | per-session host recycling, all other features intact |
| 0b | GH2 `Start(SolutionMode.Headless)` + `Phase` poll + `Tree()` read, solve wait OFF the UI thread, idle pump alive | dataflow oracle lane vs render-only | F7 lane 1 only |
| 0c | `dotnet-counters ps` vs live RhinoWIP | EventPipe lane; MDNC package admission | drop package + counters row; gcdump only if socket exists |
| 0d | Controlled supervisor-spawn benchmark (daemon-vs-batch) | process model; validates the <10 s warm claim (09 C2) | none needed — data sizes policy rows |

0b recipe under the OLD tool, pinned: `.verify.csx` phases execute ON the UI thread (the E3 regime), so the probe MUST be split-phase — an early phase hands `Start(SolutionMode.Headless)`'s `Task<Solution>` to a background waiter and returns; the host idle loop pumps between bridge phases; a later phase polls `Phase`/reads `Tree()` under deadline. A single blocking phase reproduces the old deadlock and falsely confirms the ceiling (09 E3).

Plus operator sign-off on G5 effort sizing (order-of-magnitude 1.5-2.5× the 2,158-LOC C# body + 21-file corpus migration) before Phase 1.

## [4]-[BUILD_ORDER] — phases and gates (10 §4, refined)

1. PHASE 1 — Contract + Stub + Shell: wire vocabulary with §2 amendments; hello/load/unload round-trip vs stub cargo; side-by-side install (`rbx-` pipe, distinct GUID/endpoint). Gate: 07 §10 wire proofs 1-4 (source-gen proxies, union round-trip + unknown-`$type` direction, Thinktecture converters, `-32601` mapping) green BEFORE the Contract freezes; the remaining §10 rows land at their named phases (5 = probe 0b; 6, 7, 9-13 Phase 1; 8 Phase 3).
2. PHASE 2 — Supervisor: state machine + policy + quit ladder + journal + reconcile + kqueue + lease + `.ips` diff (RhinoCrashReportFinder port) + redeploy transaction. Gate: fault-injection subset (launch-fail, planted dialog, kill-mid-connect, stale endpoint, stale lease) each yields its named fault.
3. PHASE 3 — Cargo + SDK + pilot: `CargoHost`/`Gh2Lane`/`Spool`, testkit SDK, Blocks themes migrated, capture-cost measurement (07 proof 8). Gate: pilot verdict-parity vs old tool; 10-swap unload soak; kill-mid-scenario proves crash-durable spool.
4. PHASE 4 — Full migration + assay rail v2: remaining themes (Vectors, Exchange, Camera, UI, gh-ui last); 08's deltas land (bridge.py reshape, package.py deletions, registry 7→5, model.py edit, spec migration per 08 §6); parity checklist 08 §7 runs.
5. PHASE 5 — beyond parity, each demand-gated with its own proof: MTP adapter, GH2 dataflow lane (0b), `.ghz` diff lane, cargo REPL, EventPipe artifacts (0c), MCP facade (07 §9).

Sequencing law: Phases 1-2 may start before Phase 0 completes EXCEPT cargo ALC design (0a) and GH2 lane shape (0b). The bridge-first/assay-last ORDER is law, with the why consolidated in 08 §0.4 (old tool = regression oracle; envelope = the contract the assay reshape consumes; cutover = one reversible Tool row; cs-analyzer independent, may land first).

## [5]-[DONE_CRITERIA] — cutover gates the archive step (10 §3, binding)

1. Verdict parity over the FULL live scenario corpus, old-vs-new (21 `.verify.csx` files at HEAD caf5f75b2; review 10 §3's "27 themes" does not reproduce at any readable checkout — §2.12; the criterion is N/N over the corpus enumerated at parity start), 3 consecutive full batches. 2. 10 consecutive cargo swaps, zero relaunches/dialogs, `unloadConfirmed` each. 3. Injected-FM matrix each yields its named fault; zero empty-evidence results. 4. Warm edit→verdict <10 s; full batch ≤50% old wall time (0d-calibrated). 5. Two-week soak, no capability fallback to the old tool. Then: old tool → `.archive/rhino-bridge-v1/`, `rhino-bridge-next` → `tools/rhino-bridge`, `.verify.csx` corpus deleted per closed parity record.

## [6]-[CORPUS_INDEX]

Review (`../rhino-bridge/`): 00 README/verdict · 01 architecture+F1-F18 · 02 python linkage · 03 authoring/dialect · 04 FM1-FM11 · 05 Rhino9 landscape · 06 GH2 landscape · 07 .NET10 patterns · 08 prior art · 09 critique D1-D12 · 10 candidates+recommendation · 11 red-team addendum.
Design (here): 01 doctrine charter (R1-R15) · 02 MCP verdict · 03 feature set + cut list · 04 version tolerance M1-M4 · 05 packages · 06 object model · 07 code blueprints · 08 assay impact (+ §0 old-flow/inversion account) · 09 design red-team · 10 this charter (+ §7 deferred closeout) · 11 agent-first docs appendix (authorship laws L1-L6 + line-cite amendments + MCP-matrix re-corroboration + §6 enshrined tri-tool standards).

## [7]-[DEFERRED_CLOSEOUT] — every deferred/open item across BOTH corpora, dispositioned; zero remain open

Closure vocabulary: [CLOSED] = decided by a design doc; [GATED] = converted to a mandatory Phase-0/1 proof row WITH a named fallback (a decision to measure, never an open thread); [NOT-BUILT] = deliberately unbuilt, revival condition named (demand gate). Audited 2026-06-11 at finalization; nothing below awaits an answer.

| [SOURCE] | [ITEM] | [DISPOSITION] |
| -------- | ------ | ------------- |
| review 00 §operator 1 | authorize live probes 0a/0b | [CLOSED] §3 — the four-probe gate is binding |
| review 00 §operator 2 / review 11 G1 | WIP-churn policy: pin per parity phase? | [CLOSED] tolerance M1-M4 — no pinning; re-probe per WIP bump; parity re-baselines |
| review 00 §operator 3 / review 11 G5 | C1 effort sizing | [CLOSED] §3 — order-of-magnitude pinned (1.5-2.5× the 2,158-LOC body + 21-file migration), operator sign-off gates Phase 1 |
| review 00 §operator 4 / review 11 G6 / review 04 gap 1-2 | dialog-pain reproduction; frequency-evidence window | [CLOSED] features F4 (`dialog-suspected` is detected, not reproduced; 07 §5.3 matrix) + retention tiers replace the 300 s window (07 §8.6) |
| review 00 §operator 5 | dual-run mechanics blessing | [CLOSED] §3 dual-run law + 07 §10 proof 9 (stub publish dry run) |
| review 00 §operator 6 / review 11 G4 | shell-private ALC rule | [CLOSED] charter §3 / packages placement; 09 §4 ratifies Thinktecture-in-Contract beside it |
| review 02 OQ1 | positive dialog detection (AX/JXA) | [NOT-BUILT] — heartbeat-silent heuristic + journal-scoped reconcile are the lane (07 §5.3/§5.5); AX detection revives only with a named consumer the heuristic fails |
| review 02 OQ2 | yak-install corruption race | [CLOSED] features F5 — cargo hot-swap removes yak from the loop except shell/GH2 redeploys, which the supervisor serializes |
| review 02 OQ3, review 03 OQ2, review 07 OQ3-4 | RhinoCode resolver/C#10-pin/Roslyn-coexistence/CSharpScript | [CLOSED] — RhinoCode is eliminated (FM5 [ELIM]; features F2); the questions dissolve with their subject |
| review 03 OQ1 / review 07 OQ2 | repo-built assemblies load + unload cleanly | [GATED] probe 0a (§3) — fallback per-session host recycling |
| review 03 OQ3 / review 06 | GH2 dataflow ceiling | [GATED] probe 0b (§3, thread-pinned per §2.5) — fallback render-only lane |
| review 03 OQ4 | scenario scheduling owner | [CLOSED] features F1 — session-scoped runner |
| review 03 OQ5 | `Op key` parameter weight | [CLOSED] features F3 — fused assert+fact deletes the parameter |
| review 04 gap 3 / review 02 §marker | reconcile touching a human Rhino's state | [CLOSED] 07 §5.5 quit journal — instance-scoped matching, foreign markers skipped + reported (09 D2) |
| review 07 OQ1 | EventPipe enabled on bundled CoreCLR | [GATED] probe 0c (§3) — fallback drop package + counters row |
| review 07 OQ5 | GH2 cargo-ALC carve-out | [CLOSED] `HostAssemblyTable` forwarding (model §3.2; 09 E5) |
| review 10 | MTP adapter in v1? | [CLOSED] §4 Phase 5 — demand-gated, never v1 |
| review 11 G3 | kqueue NOTE_EXIT is python evidence, C# needs interop | [CLOSED] 07 §5.5 `HostWatch` — ~50-100 LOC P/Invoke kernel, 250 ms poll fallback as a fact |
| design 03 OQ1 | pre-host scenario enumeration | [NOT-BUILT] 07 §6 — in-host discovery is the lane; `verify --list` revival demand-gated, spike shape fixed |
| design 03 OQ2 | restart-budget policy value | [CLOSED] 07 §5.2/§5.4 — `RestartBudget` row, default 1 |
| design 03 OQ3 | retention tier numbers | [CLOSED] 07 §5.4 — 7 days failure / green pruned next run, policy rows |
| design 03 OQ4 | GH2 canvas auto-capture | [CLOSED] 07 §4.2 — INCLUSIVE; cost measured at proof 8, demotion path named |
| design 03 OQ5 | `quit` verb status | [CLOSED] 08 §5 — kept, 1:1 supervisor verb |
| design 04 hook 1 | `LoadPlugIn` failure mode | [GATED] 07 §10 proof 10 — both outcomes map to capability rows |
| design 04 hook 2 | GA marker-name derivation | [GATED] 07 §10 proof 11 — policy-row override fallback |
| design 04 hook 3 | probe cost ceiling | [GATED] 07 §10 proof 12 — fingerprint-keyed cache fallback |
| design 04 hook 4 | STJ skip + `-32601` tolerance | [GATED] 07 §10 proof 4 (no fallback — M3 depends on it) |
| design 05 OQ1 | deploy-set trim | [GATED] 07 §10 proof 13 — full closure ships; trim is evidence-gated optimization |
| design 05 OQ2 | source-gen proxy fidelity | [GATED] 07 §10 proof 1 — runtime `Attach<T>` fallback |
| design 05 OQ3 | EventPipe admission | [GATED] probe 0c (§3) |
| design 05 OQ4 | StreamJsonRpc skew visibility | [CLOSED] 07 §2 pin 2 — `rpc.streamjsonrpc` hello fact (§2.15) |
| design 06 §0 | reconciliation sign-off (§14 never existed) | [CLOSED] 09 §4 — both RATIFIED; 09 S1 closes the truncation finding via 07 §4-§6 |
| design 07 OQ1-3 | list-enumeration / ladder shape / stub layout | [CLOSED]/[CLOSED]/[GATED] — 07 §6, 07 §5.4, 07 §10 proof 9 |
| design 08 OQ1-2 | `bridge_lease` survival / `Report.counts` vocabulary | [CLOSED] 08 §8 — KEEP demoted; `Counts(ok,failed,total)` suffices, tallies ride facts |
| design 09 NEEDS_LIVE_PROOF ×2 | unload contract / warm benchmark | [GATED] — they ARE probes 0a/0d (§3); the audit added no new gate |
| design 11 OQ1-3 | help rendering / scenario README / doctor-row carrier | [CLOSED] 11 §3.2 (runtime-rendered), §3.5 (omitted), §3.1 (rides `Capabilities[]` + fold) |
