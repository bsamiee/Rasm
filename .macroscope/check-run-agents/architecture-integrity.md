---
title: Rasm Architecture Integrity
input: full_diff
effort: medium
conclusion: neutral
include:
  - "libs/**"
  - "libs/**/.planning/**"
  - "libs/**/.api/**"
---

# [ARCHITECTURE_INTEGRITY]

Cross-folder boundary integrity is the lens the file-local correctness pass cannot see: whether a change places a concern in the wrong stratum, splits or scatters an owner, or couples a seam to a sibling's interior. Judge against `libs/.planning/ARCHITECTURE.md` (the stratification law, universal-vs-capture, geometry flow, cross-language wire), `docs/laws/topology.md`, and each folder `ARCHITECTURE.md` seam section, reading them before any finding. This owns the semantic boundary judgment; the mechanical seam-ledger mirror and codemap 1:1 belong to the correctness planning-corpus check and are not re-litigated here.

Hunt these classes, each finding naming file, anchor, the boundary breached, and the corrected placement:
- Strata direction breach: resolve the owning branch's `[02]-[STRATA]` table live, never a copy of its rows, and judge each placement against it — a design page seating a concern outside its stratum, a host-neutral owner reaching a host-boundary surface, a peer referencing a peer the table forbids, or capability consumed downward.
- Concern ownership: a concern owned twice within one runtime, a folder mixing unrelated concerns, or a single concern scattered across sibling folders where one owner belongs.
- Owner-shape at folder tier: a new page or owner modelling what an existing owner in this or a sibling page absorbs as a case, row, or policy value; pressure to add a second surface is the signal to deepen the first.
- Seam coupling: a cross-folder or cross-language touchpoint coupled to a sibling's interior rather than recorded as an aligned port or wire seam — coupling beyond the declared ports and the wire seams is the defect.
- Cross-language second-mint: `tests/contracts/MANIFEST.md` classes each entry and the class alone decides who mints, so read the entry live and name no language — a DOMAIN entry forks its semantic model on a second producer beside the one its entry names, an INFRASTRUCTURE entry forks the parity the corpus proves when a branch mints it twice or skips its own mint and decodes a peer's instead.
- Universal-vs-capture inversion: a host-neutral owner minted for a contract with no cross-runtime consumer (a Rhino feature wearing universal clothes), or a rich Rhino surface thinned to a host-neutral contract that guts capability.

Settled topology outranks intuition: a placement `ARCHITECTURE.md` rules is never a finding. Finding nothing after a genuine cross-folder pass is a first-class verdict.
