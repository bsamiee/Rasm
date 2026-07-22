---
include:
  - "tools/**"
  - ".claude/**"
---

# [WORKFLOWS_AND_SKILLS]

Operator and workflow code is deterministic orchestration judged as production source: schema-typed agent outputs, bounded loops, every stage writing its product to disk and returning a path receipt. Bare promises without await ownership, unbounded polling, wall-clock branching, subagents instructed to wait, and hand-authored exit semantics beside a typed envelope contract are findings.

- Workflow scripts under `.claude/workflows/` fan out fresh-context agents through plain control flow: retry lanes are attempt-counted with bounded backoff, a dead pass isolates without rejecting its chain, and stage handoffs carry navigation facts, never verdicts.
- A law a lane must enforce rides a carrier its instruction chain reaches: a lane barred from the owning file cannot be bound by it, so the dispatch prompt parameterizes the law — a sole carrier unreachable by a consumer lane is a silent no-op, and the finding names the unreachable owner and the prompt slot owed.
- Every `agent()` call carries a label, model tier, effort, and a closed JSON schema (`additionalProperties: false` with explicit `required`); an unschematized call, or a retry loop that is unbounded or aborts the whole chain on one death, is a finding.
- Per-run scratch directories mint deterministically from the normalized target set, never clock or randomness, so a resume rehydrates the same directory; a stage returns a thin path receipt with its full product on disk, never bulk product inline.
- Read-only recon lanes ride codex wrappers behind the `CODEX` flag; a pass whose acceptance gate runs a network-bound toolchain (`dotnet restore`, `uv sync`, `pnpm install`) stays native, and routing it onto a codex wrapper is a finding.
- `SessionStart` mutations are the hook-owned secret snapshot and session caches only; a hook sourcing local shell files, installing tools, or wiring host bootstrap is a finding — machine tooling routes through the Forge owner, never a repo script.
- A durable finding lands at exactly one owner — workflow script, owning skill, or owning `CLAUDE.md` — never duplicated across them.
