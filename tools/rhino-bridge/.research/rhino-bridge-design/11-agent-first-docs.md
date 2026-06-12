# [11] Agent-First Docs — How the Rebuilt Tool's Own Help, Errors, and Docs Must Read

Design-wave appendix D11, 2026-06-11. Scope: the AUTHORSHIP contract for the rebuilt tool's self-describing surfaces — `doctor` output, `--help`, fault prescriptions, the `SessionEnvelope` JSON, the per-lib scenario README, and the rail docstrings — written for an agent-only consumer. Inputs: the binding frame (every behavior hardened, diagnostics decision-useful to agents automatically), the diagnostic surface (07 §7, the 16 failure classes), the assay reshape (08), the MCP seam (07 §9, mcp §3.2.2), and a line-verified read of the current tool's documentation drift (current tree HEAD caf5f75b2). No live-Rhino contact: every claim is design- or tree-verified. This appendix also carries the line-cite precision amendments the D7-D10 push surfaced against the exact checkout (§4) — both-sides cited, written here and NOT back-edited into the graded historical docs — and the enshrined tri-tool standards in their bridge specialization (§6).

Citation key (continues 07-10): bare numbers = review corpus (`../rhino-bridge/`); named keys = design wave (charter=01, mcp=02, features=03, tolerance=04, packages=05, model=06).

Headline: the rebuilt tool has NO human-facing prose lane. The audience of every string it emits — help, error, doctor, envelope, prescription — is an agent that will act on it without a human translating. This is not a tone preference; it is a STRUCTURAL property that the existing tool's docs violate three ways (§1) and the rebuild closes by making the typed envelope the only documentation surface that can drift (§2-§3). The 26-feature cut list (features §2) already removed the human-affordances (interactive prompts, progress spinners, colorized TTY output); this doc states the positive obligation that replaces them.

## [1]-[THE_DRIFT_THE_REBUILD_INHERITS] — current docs are human-prose, and stale

The existing tool's documentation is the SAME failure class as its evidence rail: a hand-authored string surface that drifts from the code with no single-declaration gate. Verified current (08 §7 check 8 names this; restated here as the doc-authorship root cause):

| [#] | [DRIFT] | [WHERE — DOC SIDE] | [WHERE — CODE SIDE] | [CLASS] |
| :-: | ------- | ------------------ | ------------------- | ------- |
| [1] | a `schema` top-level field documented, never emitted | `tools/rhino-bridge/README.md:152-158` | `BridgeWire.cs:261-279` (no such field) | invented affordance |
| [2] | `--timeout-ms` flag + per-timeout `RASM_BRIDGE_<NAME>_TIMEOUT_S` env vars documented | `README.md:163,170` | only `RASM_BRIDGE_TIMEOUT_SCALE` exists (`BridgeWire.cs:261-279`) | invented affordance |
| [3] | the documented `verify` aggregate shape is the DEAD quality-stack shape | `README.md:283` | the live envelope is `Program.cs:69`/`ClientVerb.cs:95` | stale-against-rewrite |

Root cause is identical to the evidence-scrape root cause (02 §4, 04 FM6): a prose document maintained out-of-band from the type that it claims to describe. Three drifts in one README is the doc-side proof of the same generator the rebuild retires everywhere else. The lesson the rebuild encodes: an agent that reads `--timeout-ms` from the help and passes it gets a usage error, then must DISCOVER the truth empirically — exactly the fragility (no IDE, no analyzer, offset diagnostics, contract-in-private-memory) that the dialect kill (03 §5) removes from scenarios. Docs are the last hand-authored dialect; this appendix kills it too.

## [2]-[AUTHORSHIP_LAWS] — six binding rules for every emitted string

The same density doctrine the code obeys (one declaration, typed, fold-derived) governs the prose. Six laws, each with the failure it prevents:

| [#] | [LAW] | [SHAPE] | [PREVENTS] |
| :-: | ----- | ------- | ---------- |
| [L1] | THE_ENVELOPE_IS_THE_DOC | every fact an agent needs to act is a TYPED FIELD on `SessionEnvelope`/`BridgeFault`/`CapabilityEntry`, never free prose the agent must parse | the agent re-scraping our output — the exact anti-pattern we deleted from python (08 §2 row 1) |
| [L2] | PRESCRIPTION_IS_AN_ACTION | `BridgeFault.Prescription` (DERIVED via `Switch`, model §2.2) names the NEXT TOOL CALL or the NAMED EDIT SITE, never "an error occurred" or "check your configuration" | the agent guessing the recovery path; every 07 §7 row already states an imperative |
| [L3] | NO_INVENTED_AFFORDANCES | help/docs describe ONLY surfaces the code exposes; flags/fields/env vars are emitted FROM the type, not transcribed by hand | §1 drift #1-#2 — a documented flag that errors on use |
| [L4] | SINGLE_DECLARATION | `--help`, the `outputSchema` (07 §9), and the README all DERIVE from `SupervisorVerb` + `BridgeJsonContext`; no second authored copy of the verb/field set exists | §1 drift #3 — doc and code as two truths that diverge (tolerance M3 evidence rule) |
| [L5] | DETERMINISTIC_PHRASING | one fault class → one prescription string; no run-varying, timestamped, or randomized phrasing in the decision-bearing fields | the agent unable to pattern-match a recovery it has seen before (stable keys, charter §4) |
| [L6] | FAILURE_IS_SELF_DESCRIBING | the failure envelope carries the evidence to act WITHOUT a re-run or a flag (07 §7: "ZERO flags, ZERO re-runs") | the agent issuing a second diagnostic call to understand the first failure — the round-trip cost we are eliminating |

L1 and L6 are the load-bearing pair: together they mean an agent NEVER needs a human in the loop to interpret a result, and NEVER needs a second call to enrich a first. That is the binding frame ("diagnostics decision-useful to agents automatically") expressed as a documentation contract.

## [3]-[SURFACE_BY_SURFACE] — what each emitted artifact must read like

### [3.1] `doctor` output

`doctor` is the agent's environment probe. It emits a `SessionEnvelope` whose `Capabilities[]` and a doctor-row fold carry one row per checked precondition — bundle found, contract version, lease state, `mcp.listener` (07 §9), each capability key (07 §4.1), EventPipe socket (probe 0c). DECIDED (zero-new-types law, model §1): the `(key, observed, ok, prescription?)` row rides `Capabilities[]` plus the doctor fold — no new envelope field; the Phase-2 spec pins the law, not the shape. Every row is a structured row, not a log line. A failing row's `prescription` names the fix as an action (`run redeploy`, `dotnet restore --force-evaluate`, `quit that session`), matching the 07 §7 prescription column. An agent reads `doctor` and KNOWS whether to proceed or which single call repairs the environment; it never parses a sentence.

### [3.2] `--help` and verb surface

Help is GENERATED from `SupervisorVerb` (07 §5.1) — four verbs, each with its typed parameter shape (`Verify(ScenarioSelection, ClosureManifest)`, `Redeploy(PackagePath)`, `Doctor`, `Quit`). DECIDED: rendered at runtime from the union metadata, not build-time codegen — the supervisor is not AOT (07 §9), reflection over the generated union is legal, and runtime rendering leaves exactly one declaration with zero generated artifacts to drift (L4). No hand-authored flag list (L3); no `--mode`/`--timeout-ms`/`--cold` (cut list 03 §2 + L1 — `verify` is modality-polymorphic on input shape, never on flags). The help's job is to let an agent CONSTRUCT a valid invocation from the type, not to narrate usage. `ScenarioSelection` discriminants (all/themes/names — the union's three cases; path-shaped input resolves python-side to themes/names before the wire, 08 §5) are the only selection vocabulary — the dead `--pattern`-as-glob is gone.

### [3.3] Fault prescriptions

Every `BridgeFault` case (model §2.2, 13 cases) carries a `Status` and a `Prescription` derived by `Switch` — NOT a stored string per construction site (L5: one class, one phrasing). The 16-row agent diagnostic surface (07 §7) IS the prescription catalogue: each row's final column is the exact `Prescription` text. Authorship rule: the prescription names the actionable next step at the agent's altitude — a tool call (`run redeploy`, `rerun verify`), an edit site (`the failing label IS the pointer — edit the named site`), or a non-action (`lane unavailable on THIS host — not a bug`, `session already recovered`). It never says "see logs" — the logs ARE the envelope. It never says "contact support" — there is no human support lane.

### [3.4] The `SessionEnvelope` JSON

This is the primary document; everything else derives from it (L1). It is a single JSON document on stdout (07 §5.1, the assay decode contract). Authorship laws specific to the wire:
- Field names are the agent's vocabulary: `FirstFailure`, `FirstFaultPhase`, `Fault`, `Evidence` — discriminable by `$type` (07 §2, 08 §3), never positional or abbreviated.
- `facts` carry `(scenario_stem, json)` pairs the agent filters structurally (the model.py docstring already states the intent: "agents filter structurally instead of scanning notes" — model.py:371-384, verified current); the rebuild makes the C# authorship honor it by emitting typed facts, not note strings.
- Empty-evidence is impossible by construction (07 §7: every failure class attaches its evidence before the envelope is written; a scenario emitting zero facts gets the `facts.empty` warning fact, 07 §6) — so the agent never reads a success-shaped envelope hiding a silent loss (the asymmetric-truncation class, `BridgeWire.cs:63-66,295` current, dies).

### [3.5] Scenario authoring docs (per-lib)

Scenarios are ordinary repo C# 14 (R12), so their "documentation" is the analyzer + IDE + XML doc the dialect kill restores (03 §5) — NOT a 15-rule contract living in private agent memory (03 §5.1, the named root cause). The `[RhinoScenario]` attribute (07 §6) is self-documenting: `Requires`, `BudgetMs`, theme are declaration-time and IDE-visible. The authorship obligation: the testkit `ScenarioContext` API (07 §6) carries XML docs that state caller-visible semantics only (per repo comment discipline) — `Require(label, observed)` is "assert + fact, one call"; an agent reading the signature knows the contract without a side document. The 15-rule private contract is retired into the type system; that retirement IS the doc-reframing for the scenario lane. DECIDED: the per-lib scenario README is OMITTED — the type is the doc; revival shares the `verify --list` demand gate (07 §6) if an agent-consumer ever needs a pre-host scenario manifest.

### [3.6] The MCP facade surface (phase-5+, demand-gated)

When the deferred stdio facade ships (07 §9), the agent-first contract becomes literally MCP-native: `outputSchema` is GENERATED from `BridgeJsonContext` (L4 — single declaration), so the facade's tool descriptions cannot drift from the envelope; `structuredContent` carries the typed `SessionEnvelope`; captures ride as `resource_link`. The facade authors ZERO new prose — its entire self-description is the schema the envelope already implies (mcp matrix EITHER-6). This is the end-state proof of L1: when even the protocol surface is fold-derived, there is no hand-authored doc left to drift.

## [4]-[LINE_CITE_AMENDMENTS] — precision deltas surfaced by the D7-D10 push, both-sides cited

These are exact-checkout corrections to line cites in 08-10; the COUNTS and verdicts they support are unchanged, only the line numbers drift. Graded docs are never back-edited (corpus amendment rule, 10 §2); 08-10 are same-wave 2026-06-11 docs, so their cites are ALSO corrected in place — this table is the register of what moved. Verified against HEAD caf5f75b2 this session; re-verified by the completeness audit the same day, which caught two off-by-one defects in this table's own original "EXACT" rows (4-5 below).

| [#] | [CLAIM AS WRITTEN] | [WHERE] | [CURRENT TREE] | [DISPOSITION] |
| :-: | ------------------ | ------- | -------------- | ------------- |
| [1] | `Claim.BRIDGE` Bind rows at `registry.py:779-785` | 08 §5 | the seven rows are at `registry.py:775-781` (verify 775, doctor 776, launch 777, quit 778, check 779, clean 780, build 781) | corrected in place in 08 §5; the 7-row set and 7→5 collapse verdict stand verbatim |
| [2] | `bridge.py` is 559 LOC | 08 header, 08 §2, 09 C1, 10 §1 | 557 LOC at this exact checkout | corrected in place (08 header/§2, 10 §1); the →~150-180 arithmetic is unaffected — the deletion table is line-range-anchored, not total-anchored, with a uniform −2 interior drift noted in the 08 header |
| [3] | `package.py` is 708 LOC | 08 header, 07 layout note | 706 LOC at this exact checkout | corrected in place (08 header); the bridge-arm deletion set (08 §4, lines :59/:71-84/:233-238/:546/:568-578) is range-anchored and unaffected |
| [4] | cutover Tool row at `catalog.py:174` | 08 §1 | the `Tool("rasm-bridge", DOTNET, ("run","--no-build","--","verify"), PROJECT, CS, Claim.BRIDGE, mode=Mode.VERIFY)` row sits at `catalog.py:173` — this table's original "EXACT at 174" claim was itself off-by-one (completeness audit 2026-06-11, `grep -n` proof) | corrected in place in 08 §1; the one-line reversible cutover holds at :173 |
| [5] | `VerifySummary` fields `exceptions, report_dir, first_failure, first_fault_phase, first_fault_output, facts, captures` | 08 §3, 09 B1 | the 7 fields sit at `model.py:376-382` (`first_fault_output` is `Annotated[str, max_length=256]`) — the original "EXACT at 377-383" span was off-by-one | field set [CONFIRMED] exactly; span corrected — the field-for-field reconciliation (08 §3) and the `first_fault_output` deletion target are precise |
| [6] | `test_rail_bridge.py` is 790 LOC | 08 §6 | 787 LOC at this exact checkout | corrected in place (08 §6); the ~42-test disposition table is class-anchored, unaffected |

Net: zero verdict moves. The ±2-LOC drift on `bridge.py`/`package.py` and the 4-line shift on the registry block are noise against range-anchored deletion tables; the cutover row and the envelope-field reconciliation — the two cites the whole assay reshape pivots on — are FIELD-SET-EXACT with line anchors corrected to `catalog.py:173` / `model.py:376-382` (rows 4-5). This is the expected drift profile of a corpus frozen at `ece2f0c0d` read against `caf5f75b2` (the bridge-touching delta is the +2 commits already registered, 10 §2 amendment 2).

## [5]-[MCP_MATRIX_CURRENCY] — re-corroboration, no verdict move

The 2026-06-11 landscape re-check (mcp §5, 07 §9, 09 F2) confirms the COMPLEMENT verdict holds and TIGHTENS it; recorded here so the agent-first frame inherits the current matrix without re-editing the graded mcp verdict (02):
- rhinomcp 0.1.5 (2026-06-05) release notes are stability-only — no transport overhaul, no new tool family, no Rhino-9 work since the 02 source-read. The architecture (Kestrel-in-host request/response plugin + official-SDK stdio router + SQLite slots + `RhinoCrashReportFinder`) is unchanged. [02 source-read vs 2026-06-11 release notes]
- Anthropic's 2026-04-28 "Claude for Creative Work" shipped nine official MCP connectors (Adobe CC, Affinity, Autodesk Fusion, Blender, Ableton, Splice, SketchUp, Resolume Arena/Wire); Rhino is NOT among them. The conversational-modeling lane is now a vendor commodity — the "never build the modeling lane" call (03 §2 cut, mcp §3) is reinforced, and McNeel's MCP remains unsubmitted to the marketplace (a MOVING target, not a stable substrate). [new evidence; corroborates substrate-REJECT]
- macOS RhinoCode CLI fragility (a `RhinoCode.runtimeconfig.json` .NET 8.0.0 vs WIP-shipped 9.0.x mismatch broke CLI launch Nov-Dec 2025, McNeel-ack'd, fix regressed) is fresh evidence AGAINST any CLI-shell-out verification lane and FOR the in-process bridge (05's rhinocode-as-precedent-not-replacement call, reinforced). [new evidence]
- The MCP spec (2025-06-18) `structuredContent` + `outputSchema` makes the deferred facade MORE attractive than at corpus time — the typed evidence rail maps natively where McNeel's `{stdout, error}` string-scrape cannot. This is the agent-first payoff of L1/L4: the facade's self-description is fold-derived, drift-proof. [modelcontextprotocol.io/specification/2025-06-18/server/tools vs 02 §3.2.2]

Agent-first consequence: the complement boundary is itself agent-first by construction — the bridge owns the TYPED verification lane (facts as oracles), MCP owns the token-economical exploration lane (JPG for agent eyes); an agent picks the lane by the question shape, never by reading a doc that tells it which tool to use.

## [6]-[ENSHRINED_STANDARDS] — the tri-tool charter, bridge specialization

The three quality tools (assay + rhino-bridge + cs-analyzer) form one surface held to one standard (`tools/assay/BACKLOG.md`, tri-tool charter — the binding home; this section carries the bridge's concretization, referenced from the corpus README). What each standard means HERE, structurally:

| [STANDARD] | [CONCRETELY, FOR THIS TOOL] |
| ---------- | --------------------------- |
| Agent-first/only | every consumer is an agent: ONE structured envelope per invocation (07 §5.1); diagnostics carry the FIX — the 16-row prescription catalogue (07 §7), zero flags, zero re-runs (L6); fail fast with typed faults over hangs — every non-terminal state deadline-rowed (07 §5.3), busy = clean exit-5 envelope naming the holder (07 §5.5); help/errors/docs are agent operating contracts (L1-L6), no human-prose lane exists |
| Monorepo-first, never coupled | no hardcoded path, plugin, or host literal: bundle discovery by `CFBundleVersion` metadata, marker names DERIVE from `CFBundleName`/`CFBundleExecutable` — no "RhinoWIP" string anywhere (07 §5.5); `HostPlugins` GUIDs derive from the closure manifest, never a wired-in plugin list (07 §5.6); scenarios are per-lib repo C# 14 discovered by attribute — a new package integrates by declaring `*.Scenarios.csproj`, never by editing bridge internals (07 §6); the tool serves ANY host-bound verification need, not one plugin |
| Future-facing | no version pin anywhere: capability probes over version checks (tolerance M1), choke-point host adapters (M2), versioned additive wire (M3), fingerprint-gated self-heal (M4); hello carries versions as evidence, never as gates (07 §2 pin 2); the MCP seam is wired now, shipped on demand (07 §9) |
| Doctrine-bound | rebuild code per `docs/stacks/csharp/` (charter §8 R1-R15 instantiate it); corpus docs to page-craft standards; minimal surface — 13 files, 4 verbs, one envelope; external packages first (05; amendments registered, 10 §2); adding a capability is a case/row inside an existing owner, never a sibling surface |
| Hardened, performance-honest | leases serialize contended hosts at the resource with staleness reclaim (07 §5.5); no orphaned processes — quit ladder + kqueue NOTE_EXIT + journal-scoped reconcile (07 §5.5); performance claims are gated on measurement, never asserted (probes 0c/0d, proofs 8/12; cutover criteria 4, 10 §5) |
| Inter-relations + order | the bridge is consumed by assay's bridge/package rails; the rebuild deletes assay-side choreography (08 §0.3-§0.4 — the authoritative who-drives-whom and bridge-first/assay-last order); cs-analyzer is independent and may land first |
