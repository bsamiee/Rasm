---
include:
  - "tools/**"
  - ".claude/**"
---

# [OPERATORS_AND_WORKFLOWS]

Operator and workflow code is deterministic orchestration judged as production source: schema-typed agent outputs, bounded loops, every stage writing its product to disk and returning a path receipt. Bare promises without await ownership, unbounded polling, wall-clock branching, subagents instructed to wait, and hand-authored exit semantics beside a typed envelope contract are findings.

## [01]-[ASSAY_CONTRACTS]

- Normal invocation emits exactly one JSON `Envelope` on stdout, diagnostics on stderr, exit code a projection of envelope status; a second stdout emit, a print reaching the protocol stream, or a hand-parsed status is a finding.
- `Completed(FAILED)` means a tool ran and found defects; a `Fault` means routing, spawn, lease, or timeout failure — conflating the two in a caller is a finding.
- `assay static` diagnoses by default and mutates only under `--fix`; no dry-run flag exists, and proposing one re-litigates a settled contract. Build gating reads `dotnet build` rc plus scoped zero-new findings, never the raw `--project` advisory aggregate.
- Artifacts route through `.artifacts/assay` via the artifact store; a tool write landing at repo root is a finding by name.

## [02]-[WORKFLOW_AND_SKILL_SHAPE]

- Workflow scripts under `.claude/workflows/` fan out fresh-context agents through plain control flow: retry lanes are attempt-counted with bounded backoff, a dead pass isolates without rejecting its chain, and stage handoffs carry navigation facts, never verdicts.
- `tools/cs-analyzer`'s rule register IS the code (`Kernel/Catalog.cs` plus the shipped-releases ledger); a prose catalog of its rules anywhere is a stale mirror. `tools/biome` GritQL rows are the TypeScript doctrine's mechanical counterparts — reviewer prose never restates what those gates enforce.
- Skill bundles are judged under the skill-writer craft law: third-person trigger descriptions without keyword stuffing, `SKILL.md` a lean router with reference banks one hop down, and no body rule whose deletion changes no agent output.
- A durable finding lands at exactly one owner — workflow script, owning skill, or owning `CLAUDE.md` — never duplicated across them.
