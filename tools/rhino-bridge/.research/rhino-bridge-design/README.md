# Rhino-Bridge Design Wave (partial — 6 of 10 docs)

Design corpus for the ground-up bridge rebuild. Evidence base: `../rhino-bridge/` (12-doc review corpus, red-team grade A-). Status 2026-06-10: foundations + object model complete; the wave was intentionally stopped before blueprints per operator decision — remaining docs run at the start of the implementation session.

| [DOC] | [STATUS] | [CONTENT] |
| ----- | :------: | --------- |
| 01-doctrine-charter.md | [DONE] | repo C# doctrine applied in-host; 15-rule checklist binding 06+ |
| 02-mcp-verdict.md | [DONE] | McNeel Rhino MCP Platform: COMPLEMENT (two-lane); substrate REJECTED (re-imports 3 of 4 failure generators); coexistence design mandatory (their plugin auto-listens per document) |
| 03-feature-set.md | [DONE] | agent-first capability set with full stories + cut list; v1-core / v1-nice / post-cutover |
| 04-version-tolerance.md | [DONE] | no-pinning engineering: capability probes over version checks, choke-point host adapters, additive wire |
| 05-packages.md | [DONE] | validated package plan (StreamJsonRpc closure, shell-private ALC placement, central-manifest entries) + rejected list |
| 06-object-model.md | [DONE] | complete 34-type inventory across 6 components (Contract/Shell stub/Shell impl/Cargo/Supervisor/Scenario SDK); wire declared once; assay consumer adds zero types; UNAUDITED — D9 red-team deferred |
| 07-code-blueprints.md | [DEFERRED] | validated excerpts: cargo-ALC unload contract, StreamJsonRpc session, scenario SDK discovery, redeploy transaction, evidence flush, GH2 choke-point lane |
| 08-assay-impact.md | [DEFERRED] | ~150 LOC typed consumer reshape; one-line cutover switch; LOC before/after |
| 09 red-team | [DEFERRED] | doctrine-checklist enforcement over 06-08; fragility audit; MUST run before any implementation |
| 10 distill | [DEFERRED] | final index + build-phase outline |

Implementation gates (from `../rhino-bridge/10-candidate-architectures.md`): the four Phase-0 live probes (cargo-ALC unload + gcdump; GH2 Start(Headless) + Phase poll + Tree() read; EventPipe enablement; dotnet-run cost benchmark) precede any build code.
