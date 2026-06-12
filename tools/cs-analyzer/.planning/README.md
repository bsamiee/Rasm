# cs-analyzer Rebuild Planning Corpus (COMPLETE — 9 of 9 docs)

Design corpus for the ground-up rebuild of `tools/cs-analyzer` (current assembly `Foundation.CSharp.Analyzers`). Same posture as the assay-vs-old-quality-tool rebuild and the rhino-bridge design wave: tabula rasa, evidence-cited, post-red-team. The corpus presents the SETTLED design (04/05/07 already absorb every red-team-forced change); 08 records the deltas and the refuted attacks. Consumption is agent-first/agent-only: the analyzer exists to enforce `docs/stacks/csharp/` doctrine for AI agents working in this monorepo, so every diagnostic must be decision-useful to an agent and every rule body must be non-fragile.

Evidence base: the demolition survey of the current 87-descriptor analyzer (doc 01), the finalized 8-file doctrine corpus `docs/stacks/csharp/` (doc 02), 2026-06 web research on modern Roslyn analyzers (doc 03), and direct live-source verification of every wiring claim against tree HEAD on 2026-06-11 (counts, line cites, Build.props, assay, global.json all re-checked this session — see §VERIFIED below).

| [DOC] | [STATUS] | [CONTENT] |
| ----- | :------: | --------- |
| 01-source-audit.md | [DONE] | demolition survey: god files, ~70 flat rule bodies, hand-rolled Roslyn machinery, name-heuristic rules, scope-substring fragility, perf hazards, correctness defects; the 9 carry-forward concepts; 8 rebuild-critical verdicts (a)-(h) |
| 02-doctrine-constraints.md | [DONE] | binding constraint table (constraint → owning doc:section → enforceability); the ENFORCEABLE / JUDGMENT partition; two doctrine-drift resolutions (TIME→TimeProvider, un-finalized framework rules); the missing analyzer-authoring page mandate |
| 03-modern-analyzer-landscape.md | [DONE] | cited 2026-06 web research: SDK posture (Roslyn 5.3.0), registration semantics, IOperation-vs-syntax, perf doctrine, config surface, testing state-of-the-art, BannedApi data-driven pattern, three major-analyzer architectures; recommended-pattern shortlist |
| 04-target-architecture.md | [DONE] | folder layout (add-a-rule contract), the single Driver, Catalog/Row/Facts/Walkers kernel, the FULL kernel/contracts/harness signature set (§3 — implementer writes every surface without inventing one), the scope model (build-property + attribute + per-tree override), config surface, severity tiers, agent diagnostic contract, hardening posture, the authoritative package table (§10) — POST-red-team |
| 05-rule-catalog-parity.md | [DONE] | per-rule CARRY / COLLAPSE / DROP table for all 87 (corrected arithmetic + ID policy); tombstone/reserved-ID register; the new-rule authoring recipe; the six new CSP09xx rules — POST-red-team |
| 06-testing-harness.md | [DONE] | CSharpAnalyzerVerifier + DefaultVerifier harness, real-package references, one-file-per-rule positive+negative mandate, the generated-surface strategy, meta-test catalog invariants, concurrency + config coverage |
| 07-integration-map.md | [DONE] | monorepo wiring (contracts project, Build.props rename, generator extraction, sdk pin, the by-declaration guarantee), assay static-rail contract + the severity-plumbing changes, ID stability policy, agent-consumption spec, fast-feedback verdict (§7 — every no-compile option dispositioned) — POST-red-team |
| 08-red-team.md | [DONE] | A-I attack lanes; the five design-forcing VALID verdicts resolved into 04/05/07, every REFUTED attack recorded, every delta registered |

## Rebuild charter (the binding posture)

The rebuild collapses the 87 carried descriptors into a **data-table analyzer**: ONE sealed `DiagnosticAnalyzer`, a `Catalog.All` of `RuleRow` records, with the add-a-rule contract = one catalog row + one rule file + one test file + one `AnalyzerReleases.Unshipped.md` row + one `analyzer.md#cspNNNN` anchor (08 A1 corrected the naive "1+1+1"). It excises every project-coupled heuristic (Rhino type names, receipt name fragments, scope substrings, `ToDisplayString` identity) into config (`AdditionalFiles`) or unspoofable declarations (`build_property.CspScope`, assembly/type `[CspScope]`, member `[BoundaryAdapter]`/`[CspExempt]`). It splits the uniform-Error catalog into Law (Error) / Pressure (Warning) / Info tiers. It retargets time doctrine from NodaTime to `TimeProvider`. It drops 10 rules whose owning doctrine page is not finalized, and adds 6 new rules (CSP09xx) that the scope model and deferred backlog require.

`docs/stacks/csharp/` adherence is BINDING, in both directions: every rule's `Description` cites its owning page#section (doc 02's constraint table is the authority map), rules whose owning page is PLANNED-not-finalized are dropped until the page finalizes (doc 05 §5), `analyzer.md` joins the corpus as its 9th page, and the rebuilt analyzer's own source obeys the settled self-application posture (doc 04 §9) — doctrine authors the tool, never the reverse (`README.md` DOCTRINE:31).

Five red-team-forced changes are already resolved into the design and must NOT be re-litigated at build time (see 08 for provenance):
1. **ID policy** — semantic broadening or remediation-polarity flip ⇒ a NEW CSP09xx ID with all constituents tombstoned; survivor-ID reuse only for message-compatible strictly-narrower-or-equal merges (08 C1).
2. **Severity plumbing** — `<WarningsNotAsErrors>` band for Pressure + an assay logger/SARIF change; without it the tier taxonomy is inert under repo-wide TWAE (08 F1).
3. **Scope granularity** — per-tree override via `.editorconfig` `csp.scope` key + a published rule×scope applicability matrix; assembly-level scope alone strips domain enforcement from Rhino/GH kernels (08 D2).
4. **Contract type identity** — a real `netstandard2.0` contracts PROJECT referenced normally, not a Compile-linked source file; the linked file re-creates the CS0436 collision under IVT (08 G1).
5. **Registration truth** — 0724/0501/0503 stay `CompilationEnd` (documented batch-only); closure detection stays an IOperation walk, NOT `IFlowCaptureOperation` (08 B1, B3).

Companion deliverable the doctrine audit mandates (doc 02 §PAGE-CRAFT, 04 §AGENT_CONTRACT): author `docs/stacks/csharp/analyzer.md` owning the enforceable-vs-judgment partition, the positive+negative-test law, false-positive-is-a-rule-bug, the tier taxonomy, and the no-namespace-coupling law. The analyzer's `helpLinkUri` anchors target this file; it does not exist yet (verified: `docs/stacks/csharp/` has no `analyzer.md`).

## Enshrined standards (tri-tool charter — the analyzer specialization)

The tri-tool charter (`tools/assay/BACKLOG.md`, Rebuild-owned section) binds this corpus. What each clause means HERE, concretely:

- **Agent-first / agent-only.** Every diagnostic is an agent operating instruction, never human prose: the single-line message carries the fact, the doctrine-compliant FIX (`fix: <exact canonical API/transform>`), and the exemption route; the span is the narrowest fixable node; an agent fixes a finding without ever reading rule source (doc 04 §7, doc 07 §4). The tier signal is the action signal — Law: act; Pressure: weigh; Info: inventory.
- **Monorepo-first, never coupled.** Rules describe semantic shapes only — never `Rasm.*` namespaces, paths, host tokens, or one-off symbols; scope, banned symbols, and vocabularies are declarations and data (build property, attributes, AdditionalFiles); a new package or project is analyzed by declaration alone with zero analyzer edits (doc 07 §1 by-declaration guarantee).
- **Doctrine-bound, both directions.** The analyzer flags everything that contradicts `docs/stacks/csharp/` and nothing else: every rule cites its owning page#section, rules without a finalized owning page do not ship (doc 05 §5), and the analyzer's own source obeys the doctrine at the architectural layer with the Roslyn callback boundary as the named exemption (doc 04 §9). Doctrine authors the tool, never the reverse; a false positive is a rule bug.
- **Hardened, non-fragile, performance-honest.** Deterministic and culture-invariant (meta-grepped), concurrency-enabled AND concurrency-tested, total rule bodies (no AD0001), config absence is an error never a silent disable (CSP0901), suppression is audited (CSP0902), every exemption is inventoried (CSP0903), and every perf claim is registration-level fact, not folklore (doc 04 §9).
- **Future-facing.** ALL post-rebuild work is adding rules: one catalog row + one rule body + one test file + one Unshipped row + one doc anchor — nothing else touched. Growth lands as rows inside existing owners, never as new surfaces beside them.
- **Inter-relations + order.** The analyzer is consumed by assay's static rail — the diagnostic-line contract, severity plumbing, and CSP ID stability are owned by THIS corpus (doc 07 §2-§4); the rebuild is independent of the other tool rebuilds and lands FIRST (roadmap phase R1, `tools/assay/BACKLOG.md`), so all subsequent C# — the bridge rebuild included — is born under the rebuilt rules instead of being re-analyzed later.

## Next-session entry point

Cold-start read order: this README end-to-end → 04 (architecture + signatures + packages) → 05 (work queue + scope matrix) → 07 (wiring + assay delta) → 06 (gate). 01/02/03/08 are evidence reservoirs — consulted on demand, never re-litigated.

Build order: (1) author `docs/stacks/csharp/analyzer.md` (policy must exist before rules cite it); (2) stand up `Csp.Analyzer.csproj` + `Contracts/Csp.Contracts.csproj` + the Kernel (Driver/Catalog/Row/Facts/Walkers) with zero rules and a green meta-test harness; (3) wire Build.props (rename, contracts project ref, sdk pin, generator extraction), AnalyzerReleases md as AdditionalFiles, and the assay decoder + severity band; (4) port rules folder-by-folder (Flow→Shape→Surface→Rail→Boundary→Perf), each as catalog-row + rule-file + test-file + Unshipped-row, lowest-ID-first; (5) add the six CSP0901-0906 rules; (6) tombstone the 32 genuinely-shipped retired IDs and register the 4 never-shipped reserved IDs (0016/0716/0721/0722) in Shipped.md; (7) execute the demolition inventory + first-run adoption (doc 07 §1, §5) and update MEMORY.md (CSP0705 → CSP0910 map). Doc 05 is the work queue; doc 06 is the gate; doc 07 is the wiring checklist.

## Definition of done (the 100-percent bar — every box verifiable, no partials)

- [ ] All **63 active rules** present: catalog row + rule file (or per-category row-table entry) + test file with ≥1 positive markup span + ≥1 negative + per-exemption case + Unshipped row + `analyzer.md#cspNNNN` anchor (doc 05 §8 contract).
- [ ] All meta-test invariants green (doc 06 §5 a-g), including the Rule×Scope matrix completeness (doc 05 §10) and the `<WarningsNotAsErrors>` Pressure-band sync.
- [ ] `docs/stacks/csharp/analyzer.md` authored FIRST and every `helpLinkUri` anchor resolves; old→new ID map (0910/0911) published in it.
- [ ] Assay delta landed: per-CSP `Match` decoder, severity plumbing (warnings visible OR SARIF), CSP0903 non-console path, error-only exit codes (doc 07 §2).
- [ ] Demolition inventory executed to zero residue: old source/tests deleted, generator moved to `Rasm.Generators`, Build.props lines retired, `Workspace.slnx` renamed, `SkipBoundaryExemptionContracts` gone (doc 07 §1).
- [ ] `Shipped.md` carries the real release history: 32 tombstones in Removed Rules, 4 reserved IDs registered in `Catalog.Reserved`, RS2000-RS2008 live (doc 05 §7).
- [ ] First-run adoption complete: `uv run python -m tools.assay static full` green over the whole tree at target tiers; NodaTime dead references and central pins removed (doc 07 §5).
- [ ] MEMORY.md updated (CSP0705 → CSP0910; CSP0742 reference re-verified against the rebuilt data-driven predicate).
- [ ] `global.json` sdk pin present; analyzer loads clean (no CS8032) on a cold `dotnet build`.
- [ ] Package truth (doc 04 §10): `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` central pin added — the one new pin; `Csp.Analyzer`/`Csp.Contracts` reference ONLY `Microsoft.CodeAnalysis.*` (no runtime packages); harness `AddPackages` versions mirror central pins (doc 06 §5h).

## VERIFIED (this session, tree HEAD 2026-06-11)

- 87 descriptors, all `DiagnosticSeverity.Error` + `WellKnownDiagnosticTags.NotConfigurable` via one `Err` factory (`Kernel/RuleCatalog.cs:25-26`); `RuleCatalog.All` hand-lists all 87 (`:215-228`).
- ID gaps: CSP0016, CSP0716, CSP0721, CSP0722 absent from source (0 hits each); `AnalyzerReleases.Shipped.md` is a 2-line empty header — so these were NEVER shipped (08 C3 confirmed).
- `Directory.Build.props:75` sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` repo-wide (08 F1 confirmed).
- assay build tool: `dotnet build --no-restore -v:quiet /clp:ErrorsOnly` (`tools/assay/composition/catalog.py:160`); `fold()` (`tools/assay/core/model.py:532`) emits one `Match` per failed tool with `kind=ArtifactKind.PROCESS`, `severity="failed"` (`:543-545`) — no per-CSP-ID rows (08 F1, 07 §ASSAY confirmed).
- IVT grants (`Directory.Build.props:250-256`): `$(MSBuildProjectName).Tests`, `Rasm.TestKit`, `Rasm.Grasshopper`, `Rasm.Rhino` — exactly the Skip-list set (`:94`) (08 G1 confirmed).
- `LocalCSharpAnalyzerProject` hardcodes `CsAnalyzer.csproj` (`:85`); analyzer wired via `OutputItemType="Analyzer" ReferenceOutputAssembly="false"` (`:351`); `BoundaryContracts.cs` Compile-linked (`:355`) but skipped for Rhino/GH/test/bridge (`:94`).
- Assembly `Foundation.CSharp.Analyzers`, `netstandard2.0`, `IsRoslynComponent`, references `Microsoft.CodeAnalysis.CSharp` + `Microsoft.CodeAnalysis.Analyzers` both `PrivateAssets="all"`; stray `<Compile Remove="tests/**"/>` + producer-side `EmitCompilerGeneratedFiles` present (`CsAnalyzer.csproj`).
- Roslyn pinned 5.3.0 (`Directory.Packages.props:127-132`); `global.json` has NO `sdk` block — only the MTP runner (08 G2 confirmed).
- `docs/stacks/csharp/analyzer.md` does not exist; the 8 doctrine files are present and finalized; `domain/` roadmap pages planned not finalized.
