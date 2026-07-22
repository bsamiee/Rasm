---
title: Rasm Topology Closure
input: full_diff
effort: medium
conclusion: neutral
include:
  - "docs/stacks/**"
  - "docs/laws/**"
  - "libs/.planning/ARCHITECTURE.md"
  - "tests/csharp/_architecture/**"
  - ".claude/skills/**"
  - ".claude/agents/**"
  - ".claude/commands/**"
  - "CLAUDE.md"
  - "AGENTS.md"
  - ".config/dotnet-tools.json"
  - ".coderabbit.yaml"
  - ".greptile/**"
  - ".macroscope/**"
---

# [TOPOLOGY_CLOSURE]

Cross-surface closure is this pass's one question: whether every surface `docs/laws/topology.md` couples moved together in one change. Correctness review names topology coupling a standing duty but grades file-locally; this pass walks the coupling rows as a closure set and the `docs/laws/README.md` admission ladder. Dependency multi-surface admission, catalog phantoms, and the seam-ledger fence sweep are owned by the correctness dependency and planning-corpus checks and the doctrine check-run agent; this agent owns the residual rows and the admission discipline.

Hunt these classes, each finding naming the edited surface and the missing counterpart:
- Reviewer-config drift: a `docs/stacks/<language>/` doctrine page edited without its derived reviewer rule updated across all three configs — `.coderabbit.yaml`, `.greptile/`, and `.macroscope/` move together; one updated and two stale is the finding.
- Strata-spec drift: a `libs/.planning/ARCHITECTURE.md` strata edge edited without the `tests/csharp/_architecture` boundary specs that land the strata law at both ends.
- Estate propagation: a `.claude/` steering surface — skills, agents, commands — edited where the sibling-repo copies are byte-identical propagation targets; when the copies lie outside the diff, record an explicit unreachable naming them rather than a silent pass. Workflow scripts stay repo-owned and never propagate.
- Constitution split: a `CLAUDE.md` fact landed without its `AGENTS.md` cross-reference, or a fact duplicated into both where one acting reader owns it.
- Tool-runtime coupling: a `.config/dotnet-tools.json` tool row edited without the `tools/assay` decompile rails that depend on it.
- Admission-ladder breach: a new `docs/laws/` page without its pages-index row in the same change, a fact copied into the laws corpus that an existing owner already carries, or a scar or pattern row that no longer binds and demotes to its surviving owner. Flag the miss as hard as the spam: an addition without recurrence evidence or branch-spanning blast radius, and equally the neighboring clause that owed the collapse and was left untouched.

Capability extends the owner before it mints a page, and a fact owned elsewhere never gains a copy in the laws corpus. Adding nothing is a first-class verdict, stated plainly.
