# [03] Scenario Corpus + Authoring-Constraints Map

Research task M3 of the rhino-bridge review. Evidence base: all 21 `*.verify.csx` files (2,036 LOC, 27 `Scenario.Run` themes), the client rewrite pipeline (`tools/rhino-bridge/client/Program.cs:495-567`), the compile boundary (`tools/rhino-bridge/plugin/Rhino/CodeEngine.cs`), the wire constants (`tools/rhino-bridge/protocol/BridgeWire.cs:326-360`), the testkit harness (`tests/csharp/_testkit/Scenarios/{Probe,Capture,DocumentScope}.cs`), the verify rail (`tools/quality/rails/bridge.py`), a live failure artifact (`.artifacts/rhino/verify/2026-06-02T06-03-19*/`), and the accumulated constraint folklore in the operator's agent-memory corpus (`reference_bridge_csx_scenario_constraints` + 5 siblings).

Verdict up front: the operator's claim — "scenario creation is the fragility epicenter" — is confirmed, and the root cause is identifiable and singular. Scenarios are written in a **third dialect of C#** that exists nowhere else in the repo: C# 10 (RhinoCode's hardcoded pin), compiled against a shadow-copied reference set that differs from the project closure, with five invisible injections (base usings, two consts, a LanguageExt bootstrap incantation, reference directives) rewritten into the file before compile, no IDE/analyzer/LSP coverage, and a 3-8s live-host handshake on every compile attempt. Every documented authoring rule below is a downstream symptom of that dialect gap.

---

## [1]-[CORPUS_SURVEY]

| [MODULE] | [FILES] | [LOC] | [THEMES] | [CHARACTER] |
| -------- | :-----: | :---: | :------: | ----------- |
| Rasm/Analysis | 1 | 36 | 1 | Pure-ish native measures, no doc mutation |
| Rasm/Vectors | 4 | 674 | 10 | Heavy receipt assertion (spectral/DEC/cloud); mesh fixtures built inline |
| Rasm.Rhino/Blocks | 10 | 552 | 10 | Doc mutation + union-outcome projection; one theme per file (pre-grouping era) |
| Rasm.Rhino/Camera | 1 | 134 | 1 | Named-view lifecycle on live viewport |
| Rasm.Rhino/Exchange | 1 | 259 | 1 | PDF publish pipeline, file I/O, sheet edits — largest file, over the 250 hard cap |
| Rasm.Rhino/UI | 3 | 249 | 3 | Viewport projection, HUD layout, paint capture |
| Rasm.Grasshopper/UI | 1 | 132 | 1 | GH2 headless editor, placement, layout lattice oracle |

Idiom census (mechanical scan):

| [IDIOM] | [COUNT] | [SIGNIFICANCE] |
| ------- | :-----: | -------------- |
| `Scenario.Run(theme, CAPTURE_PATH, (key, facts) => …)` | 27 themes / 21 files | Universal entry; `Op key` couples the harness signature to `Rasm.Domain` |
| `using DocumentScope scope = DocumentScope.Open()` | 14 files | Doc lifecycle capsule; Blocks/Camera/Exchange/UI only |
| Union-result projection via `switch { Case v => …, other => throw }` | 19 occurrences (8 in one 132-LOC file) | ~4 LOC of ceremony per result read; the single largest boilerplate class |
| `using System.Linq;` declared explicitly | 5 files | Works fine — directly contradicts the folklore rule "NO System.Linq" (see [3.7]) |
| `Probe` aliased (`TestProbe = Rasm.TestKit.Scenarios.Probe`) | 1 file | Name collision with product `Probe` symbols forces per-file aliasing |
| `Capture.Snapshot` (testkit PNG channel) | 0 files | The capture rail exists, is threaded through every signature, and is unused |
| Manual `view.CaptureToBitmap()` + `bitmap.Save(CAPTURE_PATH)` | 1 file (ui-paint) | The only real PNG evidence in the corpus bypasses the testkit helper |
| Inline mesh/brep fixture builders (static local functions) | 6 files | No shared fixture library; tetrahedron/torus/box rebuilt per file |

Grouping discipline (the handshake-amortization rule from `feedback_bridge_handshake_amortization`) is honored in Vectors (3 themes/file) but not in Blocks (10 files × 1 theme — 10 × handshake where 3 files would do). Exchange (259 LOC) violates the skill's own 250 hard cap. Guidance and practice have drifted apart inside a 21-file corpus — a measure of how unenforced the contract is.

---

## [2]-[THE_AUTHORING_CONTRACT_AS_IT_ACTUALLY_IS]

What a `.verify.csx` author must know. Almost none of this is discoverable from the repo; the authoritative list lives in the operator's private agent memory, not in `docs/` or `tools/`.

### [2.1]-[WHAT_HAPPENS_TO_YOUR_FILE]

The client (`Program.cs:503-530 SourceScenarioScriptAsync`) rewrites the file before Rhino ever sees it:

1. Rejects any author `#r` / `#load` (references are bridge-owned).
2. Prepends N `#r` directives pointing at shadow-copied DLLs under `.artifacts/rhino/verify/<run>/refs/<hash>/` (16 in the observed artifact), ordered by `BridgeWire.ReferenceLoadOrder` (FSharp.Core → LanguageExt → Thinktecture → … → Rasm → TestKit/Protocol).
3. Splits the file at the first non-preamble line (`ScenarioLine.Classify`); author `using` lines stay above the split.
4. Injects after the preamble: `ScenarioBaseUsings` (`LanguageExt` + `static LanguageExt.Prelude` + `Rasm.TestKit.Scenarios`), plus `Eto.Drawing` for GH-aware projects; `const string SCENARIO_NAME`; `const string CAPTURE_PATH`; the `LanguageExtBootstrap` (two magic `HashMap` instantiations that pre-warm LanguageExt trait resolution under the isolated ALC); a `// --- [SCENARIO_BODY] ---` marker.
5. Stages the rewritten script as `script.csx` in the artifact dir and executes it via RhinoCode with `csharp.resolver.isolate=true`, `CachePolicy.NeverCache` (`CodeEngine.cs:23-58`).

Consequences the author inherits: compile diagnostics carry **line numbers of the rewritten staged script**, offset from source by `#r` count + injection block (~20+ lines, varies per project); `SCENARIO_NAME` is injected but typically unused → a benign-but-noisy CS0219 co-reported with real errors; `CAPTURE_PATH` must be forwarded literally as the second `Scenario.Run` argument.

### [2.2]-[THE_COMPILE_SURFACE]

| [AXIS] | [CSPROJ BUILD (what the repo gates prove)] | [CSX EXECUTE (what actually runs)] |
| ------ | ----------------------------------------- | ---------------------------------- |
| Compiler | Roslyn via SDK, C# 14 (preview features) | RhinoCode `CSharp<TCode>`, **C# 10 hardcoded** (decompile-verified; not configurable via `RunContext.Options`) |
| References | Full project closure incl. `Grasshopper2.dll` | Shadow set: project target + host-filtered runtime refs + `Rasm.TestKit.dll` + `Rasm.RhinoBridge.Protocol.dll`; GH2 only via `CompileReference.FromAssembly` of the already-loaded host assembly, only for GH-aware projects (`CodeEngine.cs:84-95`) |
| Analyzers | 80+ CSP descriptors + IDE/CA/MA | None |
| IDE/LSP | Full | None — `.verify.csx` resolves nothing in an editor |
| Internal access | IVT to `<Project>.Tests` + `Rasm.TestKit` | None — script compiles as an unnamed external assembly; public API only |
| Proof relationship | Green csproj build proves **nothing** about csx compilability | The bridge compiler is the only authority |

### [2.3]-[THE_RULE_LIST]

The undocumented rules an author must somehow know (consolidated from the memory corpus, verified against current source where checkable):

| [#] | [RULE] | [FAILURE MODE IF VIOLATED] |
| :-: | ------ | -------------------------- |
| 1 | C# 10 ceiling: no collection expressions `[]`, no list/slice patterns, no `params ReadOnlySpan<T>` caller expansion | Compile error naming a language version, at an offset line number, after a 3-8s+ handshake |
| 2 | `Array.Empty<T>()` collides with injected `Prelude.Array<T>()` | CS0119; use `new T[0]` or `System.Array.Empty` |
| 3 | `Probe` may collide with product symbols (`Rasm.Analysis`) | Per-file alias required (`TestProbe = …`) |
| 4 | `[GenerateUnionOps]` unions have **no verb factories** — construct `new XOp.YCase(...)`; hand-written factories exist on SOME unions (`DocumentOp.Mutate`, `EditorOp.Show`) | CS0117 "does not contain a definition"; recon agents reliably hallucinate the factories |
| 5 | `[ValueObject<T>].Create(x)` returns the unwrapped value (throws on invalid), not `Fin<T>` | Type mismatch confusion at compile or a runtime throw outside the rail |
| 6 | `internal` product types/members are unreachable (no IVT to the script) | Coverage must be re-routed to static specs; discovered per-attempt |
| 7 | No author `#r`/`#load`; references are bridge-owned | Client throws before execute |
| 8 | A `_`-named lambda parameter is a real binding at C# 10, so inner `_ = expr` assigns to it | Misleading CS0029 at the wrong conceptual location |
| 9 | GH2 types nameable ONLY in GH-aware projects (host-plugin preload + `FromAssembly` injection); `#r "Grasshopper2"` is forbidden (splits ALC singletons) | `Could not load assembly 'Grasshopper2'` or downstream "Canvas required but absent" in OTHER scenarios |
| 10 | Scenario body runs on the Rhino UI thread; GH2 solver never settles headless; `StartWait` deadlocks | Hung scenario → 180s timeout → raw `OperationCanceledException` |
| 11 | `Seq<T>` interop: prefer instance members (`.Map/.Filter/.Distinct`, indexer); explicit `using System.Linq` works but `Seq` lacks some Linq-shaped members (`.ToArray`); bridge via `.AsIterable()` | Member-not-found at offset line numbers |
| 12 | Per-assertion `facts.Add(key, value)` is mandatory for triagable grouped failure | Silent: failures become un-triagable, discovered at review time |
| 13 | Evidence goes through `FactBag`/`BridgeMarker.EmitFacts` only; ad-hoc `Console.WriteLine("key=value")` is dead (legacy parser removed) | Silent: evidence lost |
| 14 | Group themes per file to amortize handshake; ≤200 LOC target, 250 hard cap | Slow rail (cost shows up at PR time, not authoring time) |
| 15 | Display-pipeline draw callbacks' `Fin` is swallowed; raster codec path throws `TypeLoadException` on macOS RhinoWIP (hosted System.Drawing.Common gap); detail-scale needs `CommitChanges` not `CommitViewportChanges` | Hours of dead-end assertion design against unobservable or environmentally-blocked surfaces |

Rules 1-11 are *compile-or-crash* knowledge; 12-15 are *evidence-quality* knowledge. None are enforced by tooling; all are enforced by failure.

### [2.4]-[WHERE_THE_CONTRACT_LIVES]

| [LOCATION] | [CONTENT] | [PROBLEM] |
| ---------- | --------- | --------- |
| `~/.claude/.../memory/reference_bridge_csx_scenario_constraints.md` + 5 siblings | The only accurate, complete constraint list (15 numbered rules with corrections layered on corrections) | Outside the repo; invisible to humans, other agents, CI |
| `.claude/skills/testing-cs/references/bridge-runtime.md` | Grouping/LOC guidance, ownership table — deliberately project-agnostic phrasing | Names no concrete repo constraint (no C# 10, no collision list) |
| `docs/stacks/csharp/testing/*` | Rail-routing classification only | Zero authoring mechanics |
| `tools/rhino-bridge/client/Program.cs`, `protocol/BridgeWire.cs` | The rewrite pipeline itself | Source-dive required to learn what is injected |
| The 21 existing scenarios | Idioms by example | Inconsistent (5 files use Linq, 1 aliases Probe, 0 use Capture) |

There is no in-repo document that tells an author "scenarios compile at C# 10 against these references with these names pre-injected." The single most load-bearing fact about the dialect lives in a private memory file.

---

## [3]-[FRICTION_TAXONOMY_RANKED_BY_COST]

Ranked by (frequency × cost-per-hit × discoverability-of-cause). Each entry names the structural cause.

| [RANK] | [FRICTION] | [EVIDENCE] | [COST PROFILE] | [STRUCTURAL CAUSE] |
| :----: | ---------- | ---------- | -------------- | ------------------ |
| F1 | **Compile-surface divergence**: csx is a third C# dialect; green csproj proves nothing | §2.2 table; spike-verified rejection list in memory; rules 1, 2, 8, 11 | Every authoring loop; each failed attempt costs handshake + Release rebuild + offset-line diagnosis | RhinoCode's hardcoded `LanguageVersion.CSharp10` + bridge-owned shadow refs |
| F2 | **API-shape guessing**: factory-vs-case-ctor, Create-returns-unwrapped, internal-invisible | Rules 4, 5, 6; `feedback_recon_agent_api_verification` | High per-hit (compile fail or wrong-rail runtime throw); hits every new union touched | No design-time symbol resolution (no IDE/LSP on csx) — the author cannot complete or go-to-definition anything |
| F3 | **Inner-loop latency**: every compile attempt requires live Rhino: locked-mode Release rebuild of the owning project + 3-8s handshake + execute; 180s timeout on hangs | `bridge.py:207` (`scenario_timeout_s=180`); `Program.cs` build phase; handshake memory | 30s-3min per iteration; multiplies F1/F2 into hours | No offline compile verb; the only compiler that speaks the dialect lives inside Rhino |
| F4 | **Result-union projection boilerplate** | 19 `switch { Case v => …, other => throw }` blocks; 8 in gh-ui-motion-layout (~24% of the file) | Constant tax; obscures intent; copy-paste error surface | `Probe.ExpectCase` exists but its `Func<T, Option<TCase>>` shape is more awkward than the raw switch — harness API missed the actual need |
| F5 | **Diagnostics opacity**: line numbers refer to the rewritten staged script; execute hang surfaces as a raw `OperationCanceledException` named-pipe stack; CS0219 noise from injected `SCENARIO_NAME` | `CodeEngine.cs:109-118` (no source-map back through the injection offset); observed artifact `summary.json` fault | Medium per-hit but corrosive to trust; "the operation was canceled" carries zero scenario context | No source-mapping past the rewrite; no phase-aware timeout fault ("scenario X exceeded 180s during execute on theme Y") |
| F6 | **Host semantic ceilings discovered by excavation**: GH2 solver settle, DisplayPipeline Fin swallow, raster TypeLoadException, IVT wall | Rules 9, 10, 15; ui-projection-hud's 10-line apology comment (lines 9-18) documenting what CANNOT be asserted | Highest single-incident cost (multi-session dead ends); produced 4 memory files | The bridge offers no capability map — what is observable/assertable per module is learned empirically and recorded post-hoc |
| F7 | **Folklore drift**: the constraint list itself is partially wrong and self-correcting | "No GH2 types nameable" → CORRECTED; "`.ToArray()` advice WRONG"; "NO System.Linq" folklore vs 5 corpus files importing it successfully | Authors (and agents) over-constrain or mis-constrain; wrong rules propagate into new scenarios and reviews | Contract is undocumented + unenforced, so knowledge lives as oral tradition with no falsification loop |
| F8 | **Harness/evidence inconsistency**: capture rail threaded everywhere, used once, via a bypass; fixture builders duplicated per file; grouping discipline inconsistent | §1 idiom census | Low per-hit, but signals a harness that does not pull its weight | `Capture.Snapshot` exists but no scenario calls it; no shared fixture module; no lint on grouping/LOC |

The ranking's headline: **F1+F3 together form the fragility epicenter** — a dialect nobody can check offline, compiled only inside a live host, with diagnostics that point at a file the author did not write.

---

## [4]-[OUTPUT_MODEL]

### [4.1]-[PHASES]

Every client run emits a fixed 8-phase `BridgeResult` (`Program.cs:492`): `resolve → build → launch → connect → execute → liveness → diagnostics → lifecycle`, each `BridgePhase{phase, status, data, fault, diagnostics, outputs}` with status ∈ {ok, failed, skipped, unsupported}. The verify rail (`bridge.py`) folds this into `VerifyScenario{name, status, report_path, facts, captures, fault, exception_reports}` + summary `VerifyReport`, using the **first non-ok phase** as the error taxonomy:

| [FIRST NON-OK PHASE] | [MEANING] | [REMEDY ROUTE] |
| -------------------- | --------- | -------------- |
| build | Owning project compile (Release, locked-mode); `nuget-lock-drift` is a distinct fault (NU1004/NU1403) | Fix product code / `dotnet restore --force-evaluate` |
| execute + CS#### diagnostics | Scenario C# bug (the dialect) | Fix the csx (against offset line numbers) |
| execute + `InvalidOperationException`, no diagnostics | `Probe` assertion failure (behavior) | Read facts, fix product or scenario |
| execute + `OperationCanceledException` | Timeout/hang (180s default) — surfaced as a raw pipe-read stack | Guess; no phase-aware context today |
| launch / connect / liveness / timeout / busy | Bridge/Rhino tooling | Tool/host lifecycle |

### [4.2]-[FACTS]

`Scenario.Run` wraps the body in try/finally and flushes `FactBag.Snapshot()` via `BridgeMarker.EmitFacts` → a `rasm.rhino-bridge.evidence=facts={json}` stdout marker → captured in `phases[].outputs[stdout]` → regex-decoded by `bridge.py` into `VerifyScenario.facts`. Partial facts survive a throw (post-2026-05 fix). Properties: facts are stringly-keyed (`"layout.gridMoves"`), value-typed as `object`, conventions (dotted namespaces, `mainThread` first) are unenforced idiom. The assertion (`Probe.Require`) and the evidence (`facts.Add`) are separate calls — authors must remember both, and the failure message and the fact key duplicate the same information by hand.

### [4.3]-[CAPTURES]

`CAPTURE_PATH` (injected const) → `Scenario.Run` derives a per-theme PNG path → available to the body. Actual capture requires the author to call `Capture.Snapshot` (testkit, emits a `BridgeMarker.Capture` marker decoded into `VerifyScenario.captures`) — **zero corpus scenarios do**; the one real capture (ui-paint) calls `RhinoView.CaptureToBitmap` + `bitmap.Save` directly, bypassing the marker, so it never appears in `captures`. The PNG evidence channel is effectively decorative today.

### [4.4]-[ARTIFACTS]

`.artifacts/rhino/verify/<timestamp>/`: `summary.json` (counts, report_dir, first_failure, 300s TTL via `expires_in_seconds`), `<scenario>.json` (full phase report), `script.csx` (the rewritten script — the only place an author can resolve offset line numbers), `refs/<fingerprint>/` (shadow DLLs). Streams are split: structlog → stderr, machine JSON → stdout.

---

## [5]-[WORLD_CLASS_AUTHORING_REQUIREMENTS]

What it would take, ranked by leverage. The first item dominates; most others fall out of it or shrink to polish.

### [5.1]-[KILL_THE_DIALECT] — compile scenarios with the repo toolchain

The single highest-leverage move: scenarios become **normal repo C#** — real `.cs` files in (or beside) the owning test project, compiled by the repo's Roslyn at C# 14 with the full reference closure, analyzers, IVT, and IDE/LSP — and the bridge's job shrinks to *load the compiled scenario assembly into the host and invoke entrypoints*. Two implementation shapes:

| [SHAPE] | [MECHANICS] | [TRADE] |
| ------- | ----------- | ------- |
| A. Attribute-discovered scenario assembly | `[RhinoScenario("theme")]` static methods in a `*.Scenarios.csproj`; repo build compiles it; bridge loads the DLL (default ALC or isolated-with-host-fallback as today) and invokes via reflection; facts/captures via the same testkit | Assembly unload/staleness management replaces script staging; this is the mechanism the Windows ecosystem already validated (McNeel `Rhino.Testing` runs NUnit/xUnit fixtures in-process headless on Windows — same architecture, different host entry) |
| B. Keep csx, bring our own Roslyn | Plugin embeds `Microsoft.CodeAnalysis.CSharp` and compiles the script at repo LangVersion against the same shadow refs; drop RhinoCodePlatform from the execute path entirely | Smaller step; still no IDE/analyzer story for csx, still a rewrite pipeline — fixes F1's version axis only |

Shape A eliminates F1 (no dialect), F2 (IDE + analyzers + go-to-definition), most of F4 (full C# + IVT + shared helpers as ordinary code), F5's offset problem (real PDB line numbers in real files), and F7 (the constraint list largely ceases to exist). It also removes rules 1-8 and 11 from §2.3 outright. Risk to evaluate: RhinoCode is currently the only sanctioned in-host script engine; loading repo-built assemblies into the host raises staleness/unload questions the current shadow-copy scheme already half-solves (`refs/<fingerprint>/`). This is a rebuild-grade decision, consistent with the operator's ground-up framing.

### [5.2]-[OFFLINE_COMPILE_GATE] — take Rhino out of the inner loop

Whatever the dialect, an author needs a sub-second `compile` verb that uses the **identical** compiler + reference set without a live host. Under 5.1-A this is just `static build` (free). Under the status quo it means a client-side Roslyn compile at C# 10 against the staged refs — fully buildable today, and the cheapest incremental fix if a rebuild is deferred. Removes F3 from the authoring loop (the host is then only needed for *behavioral* iteration).

### [5.3]-[PROJECTION_AND_EVIDENCE_ERGONOMICS]

- One generic projection owner: `Probe.Expect<TCase>(Fin<TUnion>, label, facts)` where `TCase : TUnion`, replacing the 19 hand-rolled switch-throw blocks (4 LOC → 1 LOC each; the existing `ExpectCase`'s `Option`-selector shape failed to win adoption — replace it, don't add a sibling).
- Fuse assertion + evidence: `Probe.Require` overload that takes the fact key/value pair, so the failure message and the fact stop being maintained twice; auto-fact on every `Expect` label (success value summary), making rule 12 enforcement-free.
- A shared fixture module in the testkit (tetrahedron/torus/box/page builders) — 6 files currently duplicate them.
- Make captures real or remove the channel: auto-capture on failure for `DocumentScope` scenarios would make the PNG rail earn its threading; today it is parameter ceremony with zero consumers.

### [5.4]-[CONTRACT_IN_THE_REPO, ENFORCED]

- A single in-repo authoring reference (the §2.3 table belongs in `docs/` or the skill, not in private agent memory) — under 5.1-A it shrinks to a page.
- A scenario template per module family (doc-mutation, receipt-assertion, GH2-headless) — the corpus shows three stable shapes; templates encode `mainThread` fact, scope hygiene, grouping skeleton.
- Mechanical lint where guidance exists: LOC caps, facts-per-theme ≥ 1, `Capture`/fact-key conventions — a trivial analyzer or rail check under 5.1-A; impossible to enforce on csx today.

### [5.5]-[DIAGNOSTIC_DIGNITY]

- Source-mapped compile diagnostics (subtract the injection offset; the `[SCENARIO_BODY]` marker already exists to compute it) — only needed if csx survives.
- Phase-aware timeout faults: "execute exceeded 180s in theme `gh-ui-core` after facts {…}" instead of a naked pipe-cancellation stack; the partial-facts channel already carries the data to say it.
- A per-module **capability map** (what is observable/assertable: solver-settle ceiling, conduit Fin swallow, raster block) maintained next to the scenarios — converts F6 from excavation into lookup. The ui-projection-hud header comment is the prototype; it should be a document, not a comment.

---

## [6]-[OPEN_QUESTIONS]

1. Can repo-built scenario assemblies (5.1-A) load into RhinoWIP's host context with acceptable staleness semantics — i.e., does the existing collectible-ALC + shadow-fingerprint machinery transfer, and is GH2's default-ALC preload compatible with invoking attribute entrypoints from an isolated ALC?
2. Is McNeel's C# 10 pin moving? Rhino 9/net10 does NOT auto-raise it (decompile-verified 2026-05); if a WIP release raises `CSharpVersion`, option 5.1-B's value collapses further while 5.1-A is unaffected — worth a check against current RhinoCodePlatform bits before committing.
3. Should the GH2 dataflow ceiling (no synchronous solve) be accepted as a permanent contract of the rebuilt tool, or does a rebuilt bridge owe a pumped-message-loop / background-`StartWait` execution mode that would unlock computed-value oracles?
4. Who owns scenario *scheduling* in a rebuild — per-file invocation (today: N × handshake, discipline-dependent grouping) or a session-scoped runner that executes many scenarios per handshake and makes the grouping folklore obsolete?
5. Does the `Op key` parameter in `Scenario.Run`'s signature (coupling the harness to `Rasm.Domain`) carry its weight, or should provenance be harness-internal in the next testkit?
