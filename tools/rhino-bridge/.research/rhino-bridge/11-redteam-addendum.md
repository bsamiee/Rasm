# [11] Red-Team Addendum — Adversarial Verification of Docs 01-10

Audit date: 2026-06-10. Method: hostile spot-check of load-bearing claims against working-tree source, `.artifacts/**`, git history, the installed `RhinoWIP.app`/`Rhino 8.app` bundles, repo memory, and current web sources (NuGet, yak, Microsoft). No live-Rhino contact. Corrections were applied in place; this doc records the audit trail, the coherence verdict on 10, and the gaps the corpus did not cover.

## [1]-[VERIFICATION_MATRIX]

25 load-bearing claims spot-checked. Reproduction = the cited line/count/value reproduced exactly from the primary source this session.

| [#] | [CLAIM] | [DOC] | [CHECKED AGAINST] | [STATUS] |
| :-: | ------- | :---: | ----------------- | :------- |
| 1 | 59 bridge commits, 2026-05-16 → 2026-06-08 | 01/04 | `git log -- tools/rhino-bridge` | [VERIFIED] exact |
| 2 | 2,158 LOC C#; 538/644/976 per project | 01 | `wc -l` over the 7 source files | [VERIFIED] exact |
| 3 | `ServerExecutionCancelable: false` at `Transport.cs:254` | 01/09 | source | [VERIFIED] |
| 4 | `BridgeRequest.TimeoutMs` written, never read plugin-side | 01/09 | `rg TimeoutMs` — 3 hits, all protocol | [VERIFIED] |
| 5 | `OutputLimit = 32768` (protocol :295); `LanguageExtBootstrap` (:327-329) | 01 | source | [VERIFIED] |
| 6 | Zero `schema` in bridge C#; README documents `rasm.rhino-bridge.v1` (:163) | 01/09 | `rg` | [VERIFIED] |
| 7 | `clean` arity drift: client requires `clean <target>` (`ClientVerb.cs:56`); assay sends bare `clean` (`rails/bridge.py:532`) | 02/09 | source both sides | [VERIFIED] |
| 8 | quality launch swallow: `client_run(..., "launch", check=False)` (`bridge.py:364`) | 02/04/09 | `tools/quality` (still present) | [VERIFIED] |
| 9 | C# 10 RhinoCode pin comment at `CodeEngine.cs:31` | 01/07/09 | source | [VERIFIED] |
| 10 | Pipe `rb-{pid}-{8hex}`, `PipeInstances = 4` | 01 | `Transport.cs:58,71,83` | [VERIFIED] |
| 11 | Scenario corpus: 21 files / 2,036 LOC / 27 themes / 5 Linq files / 0 `Capture.Snapshot` / Exchange 259 LOC | 03 | `fd` + `rg` census | [VERIFIED] all six numbers exact |
| 12 | 19 union-projection sites (8 in one 132-LOC file) | 03 | 8 switch-expressions (all gh-ui-motion-layout) + 11 ternary `is X.Case v ? … : throw` in Blocks = 19 | [VERIFIED] count exact; phrasing lumps both forms under "switch" |
| 13 | Python rails: assay ~1,188 LOC, quality ~795 LOC; fact/capture regexes declared twice | 02/09 | `wc -l` (547+641; 382+413); `_EVIDENCE_RE`/`_CAPTURE_RE` in both | [VERIFIED] |
| 14 | GH2 `Start` returns `Task<Solution>`, sets `SolutionPhase.Running`, runs solve in `Task.Run` — contradicting memory `reference_gh2_headless_solution_limits` | 06 | `.artifacts/quality/api/2026-06-10T06-02-26…/query-gh2-…SolutionServer/decompile.cs:340,377-378` | [VERIFIED] decompile artifact backs the doc; the memory's "never settles" model is the stale party. 06's gating of the conclusion on a live probe is the correct posture |
| 15 | `SolutionMode.Headless` doc-comment names headless Rhino as design target | 06 | SolutionMode decompile artifact :11-16 | [VERIFIED] |
| 16 | RhinoWIP `9.0.26153.12416`; bundles .NET `10.0.2`; `rhinocode` runtimeconfig pins `net8.0` (LatestMinor — cannot roll to 10 unaided); Roslyn in RhCore resources | 05/07 | local bundle inspection | [VERIFIED] all four |
| 17 | `Microsoft.CodeAnalysis.CSharp` 5.3.0 central (`Directory.Packages.props:75`); repo `net10.0`/`LangVersion 14.0` | 07 | manifests | [VERIFIED] |
| 18 | .NET 10 GA 2025-11-11, LTS | 07 | devblogs.microsoft.com + dotnet/core release notes | [VERIFIED] |
| 19 | StreamJsonRpc 2.25.25 current | 07/10 | nuget.org, released 2026-06-02 | [VERIFIED] 8 days old at audit |
| 20 | RhinoMCP yak `Rhino-MCP-Platform` 0.1.5, updated 2026-06-04, authors incl. Sykes/Cascaval/Claude | 05 | yak.rhino3d.com | [VERIFIED] exact |
| 21 | Rhino.Testing latest 9.0.8-beta (2026-04-13), Rhino.Inside ≥ 9.0.26084.13070-beta dep | 05 | nuget.org | [VERIFIED] — but 08 said 9.0.4-beta [STALE, FIXED] |
| 22 | `RhinoApp.Exit(false)` + SIGTERM both self-SIGABRT (quit ladder rationale) | 04/07/09 | repo memory `reference_rhino_macos_crash_markers` (live-host verified 2026-05-29) + ladder code | [CONSISTENT] — corpus and memory agree |
| 23 | "D1/D4/D5/D7 generate nine of eleven failure families" | 09/10 | recount against 09's own decision register | [WRONG — FIXED] union of the four GENERATES columns = FM1-FM5, FM7, FM9, FM10 = eight; FM6/FM8/FM11 belong to D6/D10/D12 |
| 24 | Scorecard tallies: C0=0, C1=6 ELIM (FM3/4/5/6/9/10), C2=6, C3=5 | 10 | row-by-row against 10 §2 table and 04 FM definitions | [VERIFIED] internally consistent |
| 25 | Cutover criterion "27/27 themes" | 10 | corpus census (claim 11) | [VERIFIED] |

## [2]-[CORRECTIONS_APPLIED]

| [#] | [FILE] | [CHANGE] |
| :-: | ------ | -------- |
| 1 | `08-prior-art.md` §1 + §5 | Rhino.Testing 9.0.4-beta → 9.0.8-beta (NuGet 2026-04-13), two sites |
| 2 | `08-prior-art.md` §1 | Rhino.Compute currency cell: "Windows-only" → Windows + Linux WIP (2026-03, non-production), aligning with 05 §1 |
| 3 | `09-critique.md` §1 | "nine of eleven" → "generate or amplify eight of eleven" with explicit family list |
| 4 | `10-candidate-architectures.md` §0 | "produce 9 of 11" → "produce or amplify 8 of 11" |

## [3]-[COHERENCE_VERDICT_ON_10]

The candidate scores are derived, not vibes. Every [ELIM]/[MITI]/[KEEP]/[SOUND] cell in 10 §2 traces to a named 04 failure family and a named 09 decision verdict; the tally line reproduces from the table; the invariant core in 10 §0 matches 09's port-verbatim list item-for-item; the C2/C3 demotions cite specific evidence (Rhino.Testing discovery failures for C2; roslyn#41722/InteractiveAssemblyLoader and the frozen-shell violation for C3) rather than preference. The single arithmetic defect found (8-vs-9 of 11) inflated the rebuild case by one family and is fixed; it does not change the verdict ordering, because the four generators remain [REBUILD-ONLY] and the C1 elimination count is computed from the FM table, not from that sentence. One soft spot: FM4's [ELIM] for C1-C3 is definitionally generous — the crash itself remains (host law); what is eliminated is the misattribution class. The table says this honestly, but a skimmer reading the tally "6 eliminations" will overcount capability.

## [4]-[COMPLETENESS_GAPS]

Gaps material to the decision, none fatal to it:

| [ID] | [GAP] | [WHY IT MATTERS] | [DISPOSITION] |
| :--: | ----- | ---------------- | ------------- |
| G1 | Rhino 9 WIP churn during the rebuild window is unpriced. 05 §5 flags host-runtime drift as a hazard and 06 §6.5 demands `api query` re-verification per WIP bump, but 10 §3's dual-run plan never states that a WIP service release can break BOTH tools simultaneously, invalidate parity baselines mid-migration, or change the RhinoCommon surface the new Shell compiles against. The shell is "frozen by design" against an explicitly unfrozen host. | The migration's regression oracle (verdict parity old-vs-new) silently assumes one host version across the comparison window. | Add to Phase 2-4 execution: pin the WIP build for the duration of each parity phase; treat a host update as a parity-baseline reset; re-run `api doctor` + the Phase 0 probes after every WIP bump. |
| G2 | The stable-host modality (Rhino 8 + yak GH2) was never evaluated as a fragility reducer. | Closed by this audit: `Rhino 8.app` bundles .NET `8.0.14` (local inspection); the repo's `net10.0` plugin and libraries cannot load. GH2-in-R8 is also a yak `-wip` lane, and GH2 script components require Rhino 9 (06 §4). | [DEAD] — record so nobody re-asks. |
| G3 | kqueue `NOTE_EXIT` evidence (07 §5) is Python (`select.kqueue`), but C1's supervisor is C#. .NET has no BCL kqueue surface; the supervisor needs a small P/Invoke (`kqueue`/`kevent`) or a poll fallback. | Minor unpriced work in the component the corpus calls the v1 spine; "instant exit detection" is presented as free. | Price ~50-100 LOC interop in Phase 2, or accept 250 ms PID polling as the degraded fallback (still better than today: today nothing watches at all). |
| G4 | Shell dependency coexistence in the default ALC is unpriced. 07 prices Roslyn coexistence (C3 concern) but C1's Shell carries StreamJsonRpc + its System.Text.Json closure into Rhino's default plugin context, alongside whatever RhinoMCP (now installed-by-operators, 05 §5) or other plugins carry. Version unification conflicts in the default ALC are the classic Rhino-plugin failure mode. | A frozen shell that breaks when an unrelated plugin updates is not frozen. | Phase 1 design rule: shell loads its own dependencies into a shell-private (non-collectible) ALC; only the `.rhp` stub + Contract DTOs live in the default context. One paragraph of design, cheap insurance. |
| G5 | Effort sizing is asymmetric: C0 is sized (~400-600 LOC of edits) but C1 is never sized in LOC or calendar terms across its 5 phases. | The 09 §4 rebuild justification leans on "small LOC count lowers rebuild cost" without stating the rebuild's own cost; a decision-maker cannot compare C0-now-plus-C1-later against C1-now on cost because only one side has a number. | Estimate before Phase 1 commits. Order-of-magnitude from the corpus's own data: current tool is 2,158 LOC C# + ~1,188 LOC python consumer; C1 re-uses the DTO/status/ladder/reconcile core verbatim, so net-new is plausibly 1.5-2.5× the current C# body plus the scenario-corpus migration (21 files). |
| G6 | The empirical base for frequency claims is one hour of artifacts (6 runs, 2026-06-02, 300 s TTL) — 04 §5 admits this — and the operator's "error-ignoring windows" pain remains unevidenced beyond inference (FM3 mid-cycle plugin-load dialogs). | Frequency-weighted prioritization (e.g. how often FM2 actually fires per week) is reconstructed, not measured; one observed reproduction of the error-window would pin the last unnamed failure mode. | Carry as open: the fault-injection matrix in 10 §3 partially compensates; the new tool's failure-retention policy closes it structurally. |

Modalities checked and confirmed absent for good reason (no doc needed them): WebSocket/HTTP transports (07 §3 covers the gRPC/UDS spectrum; pipe-as-UDS makes them strictly worse), out-of-process GH2 via GrasshopperIO (06 §3 covers it as a probe-gated capability), Windows/Linux CI lane (05 bookmarks Rhino.Testing/Linux compute explicitly as future, not substitute).

## [5]-[CURRENCY_AUDIT]

All freshness-sensitive landscape claims re-verified within the 3-6 month window: RhinoMCP (yak 2026-06-04, discourse 2026-05-07), StreamJsonRpc 2.25.25 (2026-06-02), Rhino.Testing 9.0.8-beta (2026-04-13), Linux compute WIP (2026-03-18), GH2 yak + Rutten activity (2026-01..06), .NET 10 GA (2025-11-11, LTS — release-date fact, stable). Older citations (Rhino.Inside mac discourse 2024-01, corefx UDS PR, kqueue BSD spec, RTF architecture) are correctly flagged historical/stable in their docs. One stale claim found in the whole corpus (08's Rhino.Testing version, ~2 months behind); fixed. Local-bundle facts (RhinoWIP 9.0.26153.12416, .NET 10.0.2, rhinocode net8 mis-target) reproduced exactly on this machine 2026-06-10.

## [6]-[COLD_READ_GRADE]

**Grade: A-.** A reader with zero session context gets: the problem (04's failure catalog with per-FM trigger/blast-radius/mitigation), the evidence (01-03's line-cited tool map, 05-08's dated landscape), the causal model (09's decision register), and a recommendation whose scores recompute from the tables (10). Every code claim sampled reproduced exactly — the corpus's evidence discipline is its strongest property. Docked from A for: (a) no `00`-index stating reading order or decoding the M1/M2/S1/S2 task tags in titles; (b) citations into private agent-memory files (`reference_*`, `feedback_*`) that an outside reader cannot resolve — the load-bearing ones are restated inline, but not all; (c) the two numeric defects found (8-vs-9 arithmetic, stale Rhino.Testing version), both now fixed; (d) heavy intra-corpus cross-reference density (e.g. "09 §2.2", "07 §1 pitfall 7") that is precise but demands non-linear reading. None of these obscure the recommendation; they tax the reader's first hour.

## [7]-[UNRESOLVED]

- [OPEN] G1 (WIP-churn pricing) and G5 (C1 effort sizing) need owner decisions before Phase 1; both are planning items, not research items.
- [OPEN] The two Phase 0 live probes (cargo-ALC unload of Rasm libs; GH2 `Start(Headless)` settle) remain the gating unknowns the corpus itself names; this audit confirms the decompile evidence behind probe (b) but cannot substitute for the live run.
- [OPEN] "Error-ignoring windows" (G6) still lacks one observed reproduction naming the exact dialog.
- [NOTE] 03 §1's "19 occurrences" of union-projection boilerplate counts two syntactic forms (8 switch-expression blocks + 11 ternary `is`-pattern projections); the count is exact, the label "switch" is loose. Not corrected — the substance (one boilerplate class, ~4 LOC per site) is unaffected.
