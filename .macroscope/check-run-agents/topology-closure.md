---
title: Rasm Topology Closure
input: full_diff
effort: medium
conclusion: neutral
include:
  - "docs/stacks/**"
  - "docs/laws/**"
  - "docs/glossary/**"
  - "libs/.planning/ARCHITECTURE.md"
  - "libs/.planning/README.md"
  - "libs/contracts/**"
  - "libs/contracts/.api/**"
  - "tests/dotnet/_architecture/**"
  - ".claude/skills/**"
  - ".claude/agents/**"
  - ".claude/commands/**"
  - "CLAUDE.md"
  - "AGENTS.md"
  - "Directory.Packages.props"
  - ".config/dotnet-tools.json"
  - ".coderabbit.yaml"
  - ".greptile/**"
  - ".macroscope/**"
---

# [TOPOLOGY_CLOSURE]

Cross-surface closure is this pass's one question: whether every surface `docs/laws/topology.md` couples moved together in one change. Correctness review names topology coupling a standing duty but grades file-locally; this pass resolves the coupling table live, walks it as a closure set, and grades additions against the `docs/laws/README.md` admission ladder.

Derive the hunt from the table itself — a lens carrying its own copy of the rows reads stale the moment a row lands, which is the enforcement-transcription-drift defect:

- Read `docs/laws/topology.md` `[01]-[COUPLINGS]` and every `[02]-[CONDITIONED]` card at review time. Each row's SURFACE column is one hunt class; its OBLIGATES column is that class's closure set.
- For every row whose SURFACE the diff edits, resolve the obligated counterparts on disk and prove each moved in the same change. A counterpart lying outside the diff records an explicit unreachable naming it, never a silent pass.
- A conditioned card states its proof condition as law: hunt the card only where its condition holds, and cite the condition in the finding.
- A diff satisfying a row through a surface the row does not name is drift in the table — report the row and the surface so the table repairs, not the diff.
- A row the live tree no longer proves is a cull candidate reported at the table, and a coupling the diff demonstrates that no row carries earns a new row.

Dependency multi-surface admission, catalog phantoms, and the seam-ledger fence sweep belong to the correctness dependency and planning-corpus checks and the doctrine check-run agent; this agent owns the residual rows and the admission discipline.

Admission-ladder breach is the class this lens owns outright: a new `docs/laws/` page without its pages-index row in the same change, a fact copied into the laws corpus an existing owner already carries, or a scar or pattern row that no longer binds and demotes to its surviving owner. Flag the miss as hard as the spam — an addition without recurrence evidence or branch-spanning blast radius, and equally the neighbouring clause that owed the collapse and was left untouched.

Capability extends the owner before it mints a page, and a fact owned elsewhere never gains a copy in the laws corpus. Adding nothing is a first-class verdict, stated plainly.
