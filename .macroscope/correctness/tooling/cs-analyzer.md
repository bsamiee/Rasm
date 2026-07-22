---
include:
  - "tools/cs-analyzer/**"
  - "tools/biome/**"
---

# [ANALYZER_RULES]

Promoted analyzer rules are executable law; their registers are code, never prose.

- `tools/cs-analyzer`'s rule register IS the code — `Kernel/Catalog.cs` with both `AnalyzerReleases.{Shipped,Unshipped}.md` ledgers, whose divergence is a build failure; a `RuleRow` without its ledger entry or witness spans, and a prose catalog of the rules anywhere, are findings. Suppression rides `[CspExempt(justification)]` from the one contracts assembly, never `#pragma` or `.editorconfig`.
- A promoted rule — cs-analyzer `RuleRow` or `tools/biome` GritQL row — describes the semantic shape of one anti-pattern (trigger, predicate, exemption) and ships with witnesses that must fire and compact code that must not; reviewer prose never restates what those gates enforce.
- Analyzer changes land with their suite change under `tests/csharp/tools/cs-analyzer`; the TS rule roster proves live through the `tests/typescript/_architecture` lint gauge, never a text diff.
