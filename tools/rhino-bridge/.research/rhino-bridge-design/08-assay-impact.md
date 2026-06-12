# [08] Assay Impact — Exact File-Level Deltas When the Rebuild Lands

Design-wave task D8, 2026-06-11. Inputs: blueprints (07), model §2.4, features §0, the review corpus (bare numbers), and a line-verified read of the CURRENT assay tree this session (HEAD caf5f75b2): `tools/assay/rails/bridge.py` (557 LOC), `rails/package.py` (706 LOC), `core/model.py`, `composition/registry.py`, `composition/catalog.py`, `tests/python/tools/assay/rails/test_rail_bridge.py` (787 LOC). Interior line cites in §2/§4 carry a uniform −2 drift at this exact checkout (e.g. `bridge_lease` at :399 vs the cited :401) — ranges are deletion-table anchors, verdicts unaffected (11 §4). Corpus staleness amendment carried throughout: `tools/quality` no longer exists — it moved to `.archive/quality`; every corpus cite of `tools/quality/rails/bridge.py` is historical, and the live rail is `tools.assay` (the non-bridge quality→assay parity audit closed 2026-06-11; this doc owns only the bridge + package arms).

## [0]-[OLD_FLOW_AND_INVERSION] — the authoritative account; 10 §4 carries the phases, this section carries the why

### [0.1] How the old tool runs a scenario, end to end (line-verified)

One `verify` traverses SEVEN owners: assay registry Bind row (`registry.py:775-781`) → `bridge.py` handler → per-FILE orchestration loop (`_client_ready/_run_scenario/_run_decoded`, bridge.py:274-326) → `dotnet run --no-build -- verify <file>` client spawn per file (`catalog.py:173`; Program.cs, 708 LOC god-process) → client locates RhinoWIP (`RHINO_WIP_APP_PATH` REQUIRED — bundle discovery was dropped in the quality→assay migration, Program.cs:321-324), launches, hand-rolls framing (538-LOC `BridgeWire`) over the `rb-{pid}-` pipe (Transport.cs:83) → shell compiles the `.verify.csx` at C# 10 via RhinoCode (the dialect — no IDE, no analyzers, contract-in-private-memory, 03 §5.1) and executes its phases ON the UI thread (the 09 E3 regime) → evidence returns as the `BridgeMarker` stdout grammar, regex-scraped python-side (`_EVIDENCE_RE`/`_CAPTURE_RE`, bridge.py:55-56,201-208), swept by a 300 s TTL (bridge.py:48,364-388). Cost shape: 3-8 s handshake per FILE — N files = N × handshake (review 03 §F3; the 21-file corpus pays it 21×) — plus 3 MSBuild `-getProperty` children even warm (Program.cs:399-431) and N×45 s worst-case cold (Build.cs:99-109).

### [0.2] Every fragility of that flow, and where each dies

| [#] | [FRAGILITY] | [EVIDENCE] | [DIES AT] |
| :-: | ----------- | ---------- | --------- |
| 1 | `.verify.csx` dialect: C# 10, no analyzers/IDE, 15-rule contract in agent memory | 03 §5.1; FM5 | repo C# 14 `[RhinoScenario]` (07 §6, R12) |
| 2 | stdout evidence grammar + regex scrape; 32 KiB asymmetric truncation silently eats evidence | FM6; BridgeWire.cs:63-66,295 | typed envelope, no scrape (R10; §2 row 1) |
| 3 | per-file handshake multiplier | FM10; review 03 §F3 | one session owns the batch (features F1; §2 row 4) |
| 4 | phases execute ON the UI thread — GH2 solve waits deadlock | 09 E3 | off-UI solve wait + idle pump (07 §10 proof 5) |
| 5 | choreography split across python (steps/discovery/scrape/TTL) and the C# client (launch/connect/build) — no single session owner | bridge.py vs Program.cs | `SessionState` union, ONE owner (07 §5.2, R5) |
| 6 | 300 s TTL evidence evaporator | bridge.py:48,364-388 | retention tiers (07 §8.6; §2 row 6) |
| 7 | capture PNGs invisible to the rail (globs `*.json` only) | bridge.py:468-475 | PNG registration (07 §8.5) |
| 8 | `clean`-arity defect: bare `clean` vs `clean <target>` → usage exit 2 | bridge.py:537-544 vs ClientVerb.cs:56 | verb deleted (§5) |
| 9 | `refresh`-verb defect: package deploy invokes a verb the client lacks | package.py vs ClientVerb.cs:50-60 | arm deleted (§4) |
| 10 | bundle-discovery regression: env var REQUIRED | Program.cs:321-324 | `BundleInfo` metadata discovery (07 §5.5) |
| 11 | SIGTERM-shaped quits manufacture the crash markers reconcile must then clear | 2026-05-29 correction; 09 D1 | quit ladder, SIGTERM banned (07 §5.5) |

### [0.3] The circular relationship, named

The bridge rail was born in `tools/quality` (now `.archive/quality`) and inherited by assay near-verbatim. Assay DRIVES the bridge (registry → handler → client spawn), while the bridge's stdout grammar silently constrains assay's parser — a two-sided coupling where neither side owns the session and a wire change on either side breaks the other without a compile error. `package.py` rides the same client for bridge-slug deploys (`client_run`); the `refresh` defect (row 9) is this circularity's live proof: the python side grew a step the C# side never had. The rebuild breaks the circle structurally: choreography lives ONLY in the supervisor (07 §5.2-§5.4), the wire is declared ONCE in Contract, and assay becomes a thin typed consumer — decode one document, fold, register artifacts (§2-§3). Who drives whom, after: assay still initiates (registry rows 1:1 onto supervisor verbs, §5), but nothing python-side can drift against the wire — the envelope is the only contract surface, and it is versioned + additive (tolerance M3).

### [0.4] Rebuild order — bridge-first, assay-last, and why

The phases live in 10 §4; the ORDER is law for these reasons: (1) the old tool is the regression oracle — assay's current bridge rail must stay byte-stable until verdict parity over the live corpus (10 §5 criteria 1-5), so no assay reshape may precede the bridge; (2) the `SessionEnvelope` IS the contract the assay reshape consumes — the Contract freezes at the Phase-1 gate (07 §10 proofs green) before any python line changes; (3) the cutover is ONE reversible Tool row (§1), so the assay landing is atomic at Phase 4 and rollback is the same row; (4) tri-tool sequencing (tools/assay/BACKLOG.md, tri-tool charter): the bridge-vs-assay order is fixed HERE per that charter's delegation to each corpus's distill doc; the cs-analyzer rebuild is independent of both and may land first.

## [1]-[CUTOVER_SWITCH] — one line, reversible

The switch is `composition/catalog.py:173`: today `Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--", "verify"), PROJECT, CS, Claim.BRIDGE, mode=Mode.VERIFY)` points the rail at the old client project. Cutover = repointing this one Tool row at `tools/rhino-bridge-next/Rasm.Bridge.Supervisor` (and the Bind handler bodies behind it, §2). The old tool stays invocable via direct `dotnet run` for the parity harness (10 §3) until the five cutover criteria are green; rollback is the same one row.

## [2]-[BRIDGE_PY_TARGET_SHAPE] — 557 → ~150-180 LOC, arithmetic shown

The supervisor emits ONE `SessionEnvelope` JSON document on stdout (07 §5.1). Python's job collapses to: route scope → invoke supervisor under lease → decode → fold into `Report`/`Envelope`. Deletions, line-verified against the current file:

| [INDEX] | [DELETE] | [LINES] | [~LOC] | [WHY DEAD] |
| :-----: | -------- | ------- | :----: | ---------- |
| [1] | regex scrapers `_EVIDENCE_RE`/`_CAPTURE_RE` + `_markers` | 55-56, 201-208 | 15 | evidence is typed envelope fields; the stdout grammar no longer exists (R10) |
| [2] | msgspec wire mirrors `_BridgeFault/_BridgeDiagnostics/_BridgeOutput/_BridgePhase/_BridgeResult` + `_RESULT_DECODER` | 91-131 | 41 | R2: python decodes ONE document; no RPC-wire mirrors |
| [3] | triage/decode block `_exceptions/_diagnostics/_coerce_diagnostics/first_fault/_fault_message/_phase_diagnostic/_decode_result/_read_result` | 182-260 | 79 | first-failure taxonomy is a supervisor fold (07 §5.4); python reads results, never derives them |
| [4] | per-scenario orchestration `_client_ready/_run_scenario/_run_decoded` | 274-326 | 53 | the session owns batching (FM10 [ELIM]); one invocation per verify, not per file |
| [5] | discovery `_discover/_direct/_glob` | 329-361 | 33 | selection moves bridge-side as `ScenarioSelection` (model §2.4); assay passes the union case |
| [6] | TTL sweep `_expire_stale/_is_stale/_rmtree` | 364-388 | 25 | supervisor retention tiers (07 §8.6) replace the 300 s evidence evaporator |
| [7] | fold plumbing `_fold_scenarios/_faulted` + per-scenario artifact walk | 435-489 (parts) | 55 | one envelope decode replaces the N-scenario fold |
| [8] | lifecycle verb wrappers `_lifecycle/doctor/launch/quit/check/clean/build` glue | 491-554 (reshaped) | ~70 | see verb disposition table §5 |

Gross deletion ~370 LOC against 557 → ~187 retained, MINUS wrapper reshape, PLUS new code: supervisor invocation (~10), envelope decode + fold into `VerifySummary`/`Report` (~25), PNG+JSON artifact registration (~10). Honest landing zone: **~150-180 LOC**. The ~150 figure REQUIRES the verb-wrapper collapse (row 8) — it is not a free byproduct of scraper deletion; state this at review rather than discovering it.

KEEP, verbatim or near: `BridgeParams` (:63), `_resolve_project` repo routing (:263-271 — workspace knowledge is python's legitimate remit, 02 §7), `bridge_lease` (:401-410 — DEMOTED to a courtesy gate; the authoritative lease is supervisor-owned at the resource, 07 §5.5; python's flock only prevents two assay processes burning a supervisor spawn each), `_build_closure` + `build` (:456-465, 547-554 — the supervisor consumes rail-proven outputs and owns no build pipeline, 10 §1.2; build target list changes to Contract→Stub→Shell→Cargo→Supervisor→testkit+scenarios).

## [3]-[ENVELOPE_DECODE] — the model §2.4 "field-for-field" claim, reconciled

Verified today: `VerifySummary` (model.py:371-384) carries `exceptions, report_dir, first_failure, first_fault_phase, first_fault_output, facts, captures`. The claim was ~70% true; full reconciliation, with the 07 amendments applied:

| [VerifySummary FIELD] | [SOURCE IN SessionEnvelope] | [DISPOSITION] |
| --------------------- | --------------------------- | ------------- |
| `report_dir` | `ReportDir` | direct |
| `first_failure` | `FirstFailure` | direct |
| `first_fault_phase` | `FirstFaultPhase` | direct — NEW envelope field [AMENDED, 07 §2; citations: model §2.4 vs model.py:371-384] |
| `first_fault_output` | none, deliberately | DELETED from `VerifySummary` — it carried scraped stdout; no scrape, no field (model.py edit) |
| `exceptions` | fold: count of `Evidence` entries with `$type == "host-exception"` | derived python-side (a fold is within the decode-and-fold remit, charter §6) |
| `facts` | fold: `Evidence` `$type == "fact"` → `(scenario_stem, json)` pairs | derived |
| `captures` | fold: `Evidence` `$type == "capture"` → `(scenario_stem, path-json)` pairs | derived |
| — | `Host`, `Capabilities` | land as session-scoped FACTS (`host.fingerprint`, `capability.<key>`) inside `facts` — zero new python types, zero new fields (model §1's zero-new-types law holds) |
| — | `Scenarios` receipts | tallies ride `Report.counts`; per-receipt detail stays in `ReportDir` artifacts |
| `Envelope.run_id/verb/status/exit_code/duration_ms/error` | `RunId/Verb/Status(+ExitCode)/DurationMs/Fault` | direct (model.py:404-419) |

Decode mechanics, R2-preserving: ONE `msgspec` decode of the envelope where `Evidence` admits as raw JSON objects (`list[dict]` / `msgspec.Raw`), folded by the `"$type"` key — python NEVER declares Struct mirrors of `BridgeEvent`/`BridgeFault` cases. The union vocabulary lives in Contract alone.

## [4]-[PACKAGE_PY_DELTAS] — line-verified

| [INDEX] | [CHANGE] | [LINES] | [DETAIL] |
| :-----: | -------- | ------- | -------- |
| [1] | DELETE `client_run` import (keep `bridge_lease` only if the courtesy gate survives review) | :59 | supervisor owns lifecycle |
| [2] | DELETE `_LifecycleStep.QUIT`/`.REFRESH` + the `needs_bridge` axis | :71-84 | the enum collapses to `INSTALL`/`PUSH`; the bool axis dies with its only consumers |
| [3] | DELETE `_STEP_POLICY` `(verb, True)` rows | :233-238 | bridge-slug deploys stop being step sequences |
| [4] | DELETE the `client_run(settings, str(step))` dispatch + `bridge_lease(settings, run_steps)` branch | :546, :568-578 | replaced by ONE supervisor `redeploy <package>` invocation — the verdicted transaction (features F5) |
| [5] | KEEP `INSTALL`/`PUSH` rows + yak staging for non-bridge slugs | — | unchanged |

This closes the second live contract defect verified 2026-06-11: package deploy/publish for slug `rasm-bridge` runs step `refresh` via `client_run`, but the client HAS no `refresh` verb (`ClientVerb.cs:50-60`) — every bridge-slug deploy currently ends in a failed step row. The defect dies by deletion, not by adding the verb.

## [5]-[REGISTRY_AND_CATALOG_ROWS] — verb disposition

Current `Claim.BRIDGE` Bind rows (registry.py:775-781): `verify, doctor, launch, quit, check, clean, build` — seven. Target: **five**.

| [OLD VERB] | [DISPOSITION] | [WHY] |
| ---------- | ------------- | ----- |
| `verify` | KEPT — handler reshapes to supervisor invoke + envelope decode | the session owns launch/connect/build-consumption/discovery internally (features F1) |
| `doctor` | KEPT — 1:1 supervisor verb | plus new rows: capability flags, `mcp.listener`, lease state (07 §9) |
| `quit` | KEPT — 1:1 supervisor verb | monthly host-update hygiene (features OQ5 kept it a verb) |
| `build` | KEPT — rail-owned, never a supervisor concern | supervisor consumes rail-proven outputs (D8/10 §1.2) |
| `redeploy` | ADDED — 1:1 supervisor verb | the shell quit→install→relaunch transaction; also the package.py §4 target |
| `launch` | DELETED | warm-vs-cold is detected by hello, never declared (features F1 [AUTOMATIC]) |
| `check` | DELETED | liveness is a doctor row + every session's hello micro-doctor |
| `clean` | DELETED | reconcile is a `SessionState`, always traversed pre-launch; the verified `clean`-arity defect (bridge.py:537-544 sends bare `clean`, ClientVerb.cs:56 demands `clean <target>` → usage exit 2) dies with the verb, unfixed by design |

R6 check: assay verbs map 1:1 onto supervisor verbs (+`build` rail-side); no renames anywhere in the chain. Catalog: the one Tool row (§1). `BridgeParams` drops `pattern`-as-glob semantics in favor of passing `ScenarioSelection` — the union's THREE cases (all/themes/names, model §2.4; there is no fourth wire case): path-shaped params resolve python-side through `_resolve_project` workspace routing into themes/names BEFORE the wire (changed-file→scenario mapping is python's legitimate remit, features F8 / 02 §7) — value-shape selection, no new flags.

## [6]-[TEST_AND_LAW_MIGRATION]

`tests/python/tools/assay/rails/test_rail_bridge.py` — 787 LOC, ~42 tests. Disposition by class:

| [CLASS] | [TESTS] | [DISPOSITION] |
| ------- | :-----: | ------------- |
| marker/scrape laws (`test_markers`, marker-stdout surfacing) | ~3 | DELETE with the scrapers |
| wire-mirror decode laws (`test_decode_result_*`, `test_first_fault*`, `test_coerce_diagnostics*`) | ~10 | DELETE — first-fault taxonomy is now supervisor-owned C# (law moves to the C# spec suite: `SessionFold` first-non-ok projection + `PhaseStatus.Worst` total-order law, the 09 attack A4 regression) |
| TTL sweep laws (`test_expire_stale*`, `test_is_stale*`, `test_rmtree*`) | ~6 | DELETE — retention is supervisor policy; its law lands as a C# policy-row spec |
| discovery laws (`test_direct`, `test_run_scenario` glob halves) | ~4 | DELETE — selection is bridge-side; replace with ONE law: params → `ScenarioSelection` case mapping |
| lease laws (`test_bridge_lease_second_contender_sees_busy`) | 1 | PORT — courtesy-gate semantics; add the holder-dead staleness companion law mirroring 07 §5.5 |
| routing/params/surface laws (`test_resolve_project*`, `test_bridge_params*`, `test_bridge_module_public_surface`) | ~8 | PORT with surface list updated to the shrunk module |
| build/artifact laws (`test_build_routes_build_tool`, `test_scenario_artifacts_json_files_become_rhino_artifacts`) | ~3 | PORT; artifact law EXTENDS to assert PNGs register (the verified gap: `_scenario_artifacts` globs `*.json` only, bridge.py:468-475) |
| NEW laws | — | envelope decode round-trip from a canned `SessionEnvelope` JSON exhibit (the model §2.3 wire exhibit doubles as the fixture); `$type` fold projections (facts/captures/exceptions); `first_fault_output` absence; exit-code passthrough |

Target spec file: ~300 LOC. Scenario-corpus migration (21 `.verify.csx` files → `[RhinoScenario]` methods) is NOT assay work — it follows the parity plan order Blocks → Vectors/Exchange → gh-ui last (10 §3) and the testing-cs authoring pipeline.

## [7]-[PARITY_AUDIT_CHECKLIST] — bridge+package arms vs `.archive/quality`

Non-bridge quality→assay parity closed 2026-06-11; this checklist is the remaining bridge/package audit, runnable at cutover. Historical cites point at `.archive/quality` (corpus amendment: review README/01/02/03/04 wrote `tools/quality/...` — that path is dead).

| [#] | [CHECK] | [PROOF] |
| :-: | ------- | ------- |
| 1 | Every archived quality bridge behavior is either present in assay+supervisor or named on the cut list | diff `.archive/quality/rails/bridge.py` verb surface vs §5 table; the launch-swallow fix (`check=False` at archived bridge.py:364) must hold in the supervisor's typed launch fault |
| 2 | Bundle discovery restored (dropped in the quality→assay migration — `RHINO_WIP_APP_PATH` is currently REQUIRED, Program.cs:321-324) | `doctor` finds the bundle with the env var unset (07 §5.5 `BundleInfo`) |
| 3 | `clean`-arity and `refresh`-verb defects closed by deletion | grep: no `client_run`, no `refresh` step, no bare-`clean` call sites |
| 4 | PNG artifacts registered + retention tiers active | failure-run PNG present in `Report.artifacts`; green-run pruned next run; no 300 s sweep |
| 5 | Envelope contract: `VerifySummary` populated solely from one decode; `first_fault_output` gone from model.py | spec §6 NEW laws green |
| 6 | Exit-code parity: busy=5, timeout=5, unsupported=3, failed=1, ok=0 across rail and supervisor | `PhaseStatus` rows vs `Envelope.exit_code` passthrough law |
| 7 | Lease single-ownership: python flock defers to supervisor lease; no double-acquire deadlock | two concurrent `verify` runs — one proceeds, one exits 5 naming the holder |
| 8 | README/docs drift purged (current README documents nonexistent `schema` field, `--timeout-ms`, per-timeout env vars — README.md:152-158,163,170,283) | new README generated from Contract; doc-drift class closed by single-declaration (tolerance M3 evidence rule) |

## [8]-[FINALIZATION_DECISIONS] — former open questions, closed (register: 10 §7)

1. `bridge_lease` — DECIDED KEEP as the demoted courtesy gate (10 LOC, zero risk; it only prevents two assay processes burning a supervisor spawn each — §2 KEEP list). Post-cutover deletion is a named cleanup contingent on parity check 7 evidence that the supervisor lease + busy admission (07 §3.1, §5.5) alone suffice — a cleanup row, not an open design.
2. `Report.counts` — DECIDED: the existing `Counts(ok, failed, total)` vocabulary (model.py:239-244, verified) suffices. Unsupported/skipped tallies land as session-scoped facts (`scenario.unsupported`, `scenario.skipped`) exactly per the §3 `Host`/`Capabilities` precedent — zero new model.py fields, zero new types; per-receipt detail stays in `ReportDir` artifacts.
