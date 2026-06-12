# [07] Integration Map — Monorepo Wiring, Assay Contract, ID Policy, Agent Consumption

How the rebuilt analyzer wires into the monorepo, the assay static rail, and the agent-consumption surface. POST-red-team: lanes D2 (scope), F1 (severity plumbing), G1 (contracts identity), G2 (sdk pin), G3 (Build.props rename + generator), and E1/F2 (agent surface) are resolved here. Evidence base: `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `tools/assay/`, all re-verified this session (see README §VERIFIED).

## [1]-[MONOREPO_WIRING]

**Analyzer reference.** Keep `ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false"` (`Directory.Build.props:350-352`) + the Skip flag for self/tests. No NuGet packaging, single Roslyn target 5.3.0 (current pin `Directory.Packages.props:127-132`, latest stable, nuget 2026-03-10); skip the `roslynX.Y` multi-targeting — no external IDE matrix (doc 03 §1, §8). 08 G4 REFUTES any concern here: project-ref-as-Analyzer + no NuGet + single target is sound for an agent-only CLI monorepo.

**By-declaration guarantee (the monorepo non-coupling law, explicit).** A new project enters enforcement with ZERO analyzer edits: the Build.props analyzer reference flows to it automatically, CSP0901 errors until it declares `CspScope`, the rule×scope matrix (doc 05 §10) then selects its band, and data-driven rules read the same repo-level AdditionalFiles. Symmetrically, no rule, path, or wiring inside the analyzer names a project: scope is declared (build property / attribute / editorconfig), symbol sets and vocabularies are AdditionalFiles data, rule names carry no host tokens (doc 05 §2 0723 rename), and CSP0802's attribute names arrive via `build_property`. The one deliberately project-coupled component is `Rasm.Generators` — which is exactly WHY it leaves the analyzer assembly (generator extraction below).

**Rename.** `Directory.Build.props:85` hardcodes `CsAnalyzer.csproj` in `LocalCSharpAnalyzerProject`; the rebuild renames the project to `Csp.Analyzer.csproj` (assembly `Csp.Analyzer`, currently `Foundation.CSharp.Analyzers`). Build.props must update this literal (08 G3). `Workspace.slnx` lists `CsAnalyzer.csproj` + `CsAnalyzer.Tests.csproj` — both rename.

**Contracts type-identity (08 G1 — the forced change).** Compile-linking `CspContracts.cs` into EVERY project re-creates the collision the Skip list exists to dodge. The current `BoundaryContracts.cs` is ALREADY zero-dependency (pure System attributes), so "made zero-dependency so the Skip list dies" misdiagnoses the cause: the Skip set (Rhino/GH/test/testkit/bridge, `:94`) matches EXACTLY the IVT-connected projects (`:250-256` grants InternalsVisibleTo to `$(MSBuildProjectName).Tests`, `Rasm.TestKit`, `Rasm.Grasshopper`, `Rasm.Rhino`). Public attributes → CS0436 across ProjectReference chains; internal attributes → CS0436 via IVT; TWAE makes either fatal. RESOLUTION: a tiny `netstandard2.0` contracts PROJECT `Contracts/Csp.Contracts.csproj` referenced NORMALLY everywhere (solves type identity — one assembly, one identity, no CS0436), replacing the Compile-linked source file and retiring `SkipBoundaryExemptionContracts` (`:94-95`). Rhino/GH/test/bridge projects can now use `[BoundaryAdapter]`/`[CspExempt]` (the old Skip list blocked them).

**Generator extraction (08 G3).** `UnionOpsGenerator` moves to `libs/csharp/Rasm.Generators/` (doc 04 §8). Build.props references it as an Analyzer ONLY by Rasm-family projects (the generator emits `Rasm.Domain.Op`; it must not load into Foundation/tooling projects). The original design omitted this condition.

**SDK pin (08 G2 — the forced change).** `global.json` has NO `sdk` block (verified: only the MTP `test` runner). "Single Roslyn target 5.3.0" rests on the ambient SDK; an older host compiler refuses the analyzer (CS8032 warning → fatal under TWAE) on any non-canonical machine. RESOLUTION: pin `sdk.version` in `global.json` as part of the rebuild (matched to the Roslyn 5.3.0 / VS 2026 / SDK band, doc 03 §1).

**csproj hygiene.** Wire `AdditionalFiles` for AnalyzerReleases md (release tracking goes live — the Analyzers package is already referenced but the md is not wired, so RS2000-RS2008 are inert today). Delete the stray `<Compile Remove="tests/**"/>` and the producer-side `EmitCompilerGeneratedFiles`.

**Demolition inventory (tabula rasa — the explicit DELETE list, verified against the live tree).** Source deleted outright: `DomainStandardsAnalyzer.cs`, `Dispatch/AnalyzerDispatcher.cs`, `Kernel/{RuleCatalog,SymbolFacts,AnalyzerState}.cs`, `Rules/{FlowRules,RuntimeRules,ShapeRules,TypeShapeRules}.cs`, `Contracts/BoundaryContracts.cs` (superseded by the `Csp.Contracts.csproj` project), `CsAnalyzer.csproj` (superseded by `Csp.Analyzer.csproj`; `packages.lock.json` regenerated). MOVED, not deleted: `Generators/UnionOpsGenerator.cs` → `libs/csharp/Rasm.Generators/` (rebuilt per doc 04 §8). Tests deleted: `tests/tools/cs-analyzer/{RuleBehaviorTests,ReleaseDisciplineTests,Infrastructure/AnalyzerTestHarness}.cs` + `CsAnalyzer.Tests.csproj` (superseded per doc 06; no test is "migrated" — every rule gets a fresh markup-span file). REWRITTEN, not deleted: `AnalyzerReleases.{Shipped,Unshipped}.md` (Shipped gains the real release history + the 32 tombstones, doc 05 §7). `Directory.Build.props` lines retired: the `BoundaryContracts.cs` Compile-link block (`:354-356`) and `SkipBoundaryExemptionContracts` flag (`:94-95`); updated: the `LocalCSharpAnalyzerProject` literal (`:85`); `Workspace.slnx` entries renamed. Nothing else exists under `tools/cs-analyzer/` or `tests/tools/cs-analyzer/` — this list IS the full demolition.

## [2]-[ASSAY_STATIC_RAIL_CONTRACT]

**Current gap (verified).** `tools/assay/core/model.py` `fold()` (`:532`) emits at most ONE `Match` per failed tool with `text=(stderr or stdout)[-400:]`, `severity="failed"`, `kind=ArtifactKind.PROCESS` (`:543-545`); `Match`'s `severity`/`score`/`line`/`confidence` and the dedicated `Diagnostic` Detail struct are NEVER populated from CSP output, so an agent gets no per-CSP-ID rows, no `file:line`, no rule title — only the `/clp:ErrorsOnly` console tail interleaving CS/CA/CSP/IDE/MA/NU. The bridge rail's `VerifySummary` (per-scenario facts) has no analyzer analog.

**Analyzer guarantee.** The analyzer emits standard MSBuild diagnostic lines `path(line,col): error|warning CSP####: single-line-message` (the message grammar from doc 04 §7).

**Assay decoder (the assay-side change).** assay adds ONE decoder in `tools/assay/core/model.py` `fold()` lifting each MSBuild diagnostic line into its own `Match` (id/severity/file/line) and splitting CSP from CS/CA/MA/IDE.

**Severity-plumbing changes (08 F1 — "this is the only assay-side change" was FALSE).** Two verified facts force MORE than the decoder:
- `Directory.Build.props:75` `TreatWarningsAsErrors=true` repo-wide → `Pressure=Warning` promotes to error. RESOLUTION: `<WarningsNotAsErrors>` listing the Pressure-band CSP IDs (or `dotnet_analyzer_diagnostic.category-X.severity` config) so Pressure stays warning and is distinguishable from Law.
- assay build runs `/clp:ErrorsOnly` (`tools/assay/composition/catalog.py:160`) → warnings NEVER reach the agent rail and CSP0903 Info is invisible. RESOLUTION: the assay console logger must include warnings (drop `/clp:ErrorsOnly` or widen it), OR adopt `/errorlog` SARIF so `fold()` reads structured diagnostics; CSP0903 Info needs a non-console emission path (the SARIF artifact or a dedicated inventory file).

**Exit-code posture.** The static rail's pass/fail stays ERROR-only: Law errors fail the rail; Pressure warnings and CSP0903 Info rows are surfaced as DATA (`Match` rows / SARIF / inventory artifact), never folded into the exit code. Widening the logger must not convert warning visibility into rail failure — that would re-promote Pressure to de facto Law through the back door.

Routing prefix, props wiring, and release tests reuse as-is; the decoder + severity band + SARIF/logger change + the error-only exit-code guarantee is the assay-side delta.

## [3]-[ID_STABILITY_POLICY]

CSP#### immutable once SHIPPED. CARRY keeps its ID; message-compatible COLLAPSE survivor takes the LOWEST constituent ID, constituents tombstoned; semantic-broadening/polarity-flip COLLAPSE gets a NEW CSP09xx ID with all constituents tombstoned and an old→new map (08 C1 — 0002/0303/0705→0910, 0007/0401/0714→0911); new rules allocate CSP09xx; retired IDs never reused; never-shipped IDs (0016/0716/0721/0722) are RESERVED not tombstoned (08 C3). Full ledger in doc 05 §1, §6, §7. Rationale: agent memory and docs reference IDs (MEMORY.md cites CSP0705/CSP0742); the old→new map is published in `analyzer.md` and MEMORY.md is updated (CSP0705 → CSP0910).

## [4]-[AGENT_CONSUMPTION_SPEC]

The analyzer is agent-only; the load-bearing surface is the SINGLE-LINE diagnostic message, not the IDE lightbulb (doc 03 §9, §12).

- **Message grammar** (doc 04 §7): `"<fact naming the symbol>; fix: <exact canonical API/transform>; exempt: <route>"`. One line, assay-parseable, names the violated standard and the canonical fix (carries the remediation-in-message convention `RuleCatalog.cs:148,177,193`).
- **`helpLinkUri`** = `analyzer.md#cspNNNN` (08 F2 — the file does NOT exist; it is the companion deliverable's concrete output, doc 02 §4). Treat anchors as documentation policy; agents never see `helpLink` in console output, so it is not diagnostic plumbing.
- **Properties bag** (`fix`/`doctrine`/`scope`) and `helpLinkUri` flow ONLY through SARIF/IDE (08 E1). DECISION: keep `fix`/`doctrine`/`scope` strictly in the message; populate the bag + help ONLY IF the assay SARIF path (§2) lands. The bag/help are conditional, not speculative scaffolding.
- **Span** = narrowest fixable node (operator token, member identifier), never the whole declaration — so an agent edits the exact site.
- **Tier signal** must survive to the agent: Law = error (act), Pressure = warning (architecture pressure / hypothesis — weigh, may suppress with Justification), Info = inventory (CSP0903 exemption audit). This signal is dead unless §2's severity plumbing lands.

## [5]-[FIRST_RUN_ADOPTION] (what happens when 63 rules first meet the existing tree)

Greenfield, no grandfathering: NO baseline files, NO bulk `[SuppressMessage]` waves, NO temporary severity downgrades (a Law-tier downgrade is itself a CSP0902 error). First-run TRUE positives are production fixes inside the rebuild wave; first-run FALSE positives are rule bugs fixed in the analyzer BEFORE landing (CLAUDE.md [4]; doc 05 §8 FP doctrine). The port (README build order step 4) proves each category with `assay static build` over the affected closure; the rebuild is not done until `assay static full` is green with all 63 rules at target tiers.

Tier-change consequences, stated so nobody "fixes" them back:
- The 9 Pressure rules (`0013 0501 0607 0719 0730 0738 0741 0744 0745`) STOP failing builds — intentional; they are architecture pressure, surfaced as warnings through the §2 plumbing.
- **CSP0911 TimeDiscipline migration scale is VERIFIED ≈ zero:** `rg NodaTime --glob '*.cs'` over the tree hits ONLY the old `RuleCatalog.cs` message text — zero NodaTime call sites exist in libs/apps. The residue is dependency hygiene, executed in the same wave: drop the dead `NodaTime` references from `Rasm.AppHost.csproj` and `Rasm.Persistence.csproj`, then the central pins (`Directory.Packages.props:37` NodaTime, `:54` NodaTime.Serialization.SystemTextJson), after confirming no non-source wiring (serializer converter registration) names them. `DateTime.Now`/`Stopwatch.StartNew` call sites in libs/apps: rg-verified zero, so 0911 lands quiet.
- New rules 0904/0905/0906 and the rebuilt data-driven 0742 may surface genuine first-run findings; they are triaged under the FP doctrine above, not pre-suppressed.

## [6]-[COMPANION_DELIVERABLE]

`docs/stacks/csharp/analyzer.md` (doc 02 §4) is a hard prerequisite, not optional: it owns the enforceable-vs-judgment partition, the positive+negative-test law, false-positive-is-a-rule-bug, the tier taxonomy, the no-namespace-coupling law, and the per-CSP `#cspNNNN` anchors the analyzer links to. Author it FIRST (README next-session build order step 1); the rules cite it, the `Description` fields embed its section anchors, and without it the rebuild re-derives authoring policy ad hoc.

## [7]-[FAST_FEEDBACK_VERDICT] (no-compile consideration — settled, every option dispositioned)

Two loops exist, and only one ever needed a compile dodge.

**Rule-author loop — ADOPT, already no-compile.** `CspTest` (doc 04 §3) compiles a tiny in-memory snippet against pinned reference assemblies; a rule under development iterates sub-second inside `assay test run` on the analyzer test project with zero repo builds. This IS the fast-feedback surface for analyzer development; nothing faster exists or is needed.

**Consumer loop — agents editing product code.** Diagnostics are a compile artifact; each option is judged against that fact:

| [OPTION] | [VERDICT] | [REASON] |
| -------- | :-------: | -------- |
| Scoped warm `assay static build [paths]` | **ADOPT — primary** | routes changed files to the owning closure under the stable per-closure `--artifacts-path` (warm incremental); a one-project closure re-analyzes in seconds and the analyzer rides the compile already being paid; the §2 decoder lands each CSP line as its own `Match` |
| `dotnet format analyzers` / `assay static report` | **ADOPT — diagnostics-only lane** | the mutation-disallowed read-only path; NOT faster than build (workspace load performs a design-time build per project) — its value is reporting posture, never latency |
| Analyzer hot-reload | **REJECT** | Roslyn loads analyzer assemblies into the compiler process with no reload protocol; VBCSCompiler additionally caches loaded analyzer assemblies, so an analyzer-DLL rebuild needs `dotnet build-server shutdown` before consumers observe new behavior — the analyzer-dev loop documents that step instead of pretending a hot path exists |
| IDE/LSP live analysis | **REJECT** | no human IDE sits in the agent loop, and the CompilationEnd band (0501/0503/0724/0744/0906) is documented batch-only — live analysis silently drops it |
| Standalone runner over Roslyn workspaces | **REJECT** | `MSBuildWorkspace` pays the same design-time build the scoped rail pays, then re-runs analysis OUTSIDE the rail — a second execution surface that drifts from the canonical one for zero latency gain; one rail per concern, assay static owns it |

Floor statement: the compile cannot be skipped without forking the execution surface. The design spends its budget making the analysis pass cheap (single Driver pass, per-compilation indices, no `GetSemanticModel`, no global Literal registration — doc 04 §9) and the closure small (assay routing), not on dodging compilation.
