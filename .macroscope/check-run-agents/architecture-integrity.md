---
title: Rasm Architecture Integrity
input: full_diff
reasoning: high
effort: medium
conclusion: neutral
include:
  - "libs/**"
---

# [ARCHITECTURE_INTEGRITY]

Cross-folder boundary integrity is the lens the file-local correctness pass cannot see: whether a change places a concern in the wrong stratum, splits or scatters an owner, or couples a seam to a sibling's interior. Judge against `libs/.planning/architecture.md` (strata, dependency direction, universal-vs-capture, geometry flow, cross-language wire), `docs/laws/topology.md`, and each folder `ARCHITECTURE.md` seam section, reading them before any finding. This owns the semantic boundary judgment; the mechanical seam-ledger mirror and codemap 1:1 belong to the correctness planning-corpus check and are not re-litigated here.

Hunt these classes, each finding naming file, anchor, the boundary breached, and the corrected placement:
- Strata direction breach: a design page placing a concern below or above its stratum, a host-neutral owner reaching a RhinoCommon or host-boundary surface, an AEC peer referencing another peer, or app-platform capability consumed downward — against the `architecture.md` upward law (KERNEL to AEC-DOMAIN to APP-PLATFORM to HOST-BOUNDARY).
- Concern ownership: a concern owned twice within one runtime, a folder mixing unrelated concerns, or a single concern scattered across sibling folders where one owner belongs.
- Owner-shape at folder altitude: a new page or owner modelling what an existing owner in this or a sibling page absorbs as a case, row, or policy value; pressure to add a second surface is the signal to deepen the first.
- Seam coupling: a cross-folder or cross-language touchpoint coupled to a sibling's interior rather than recorded as an aligned port or wire seam — coupling beyond the declared ports and the wire seams is the defect.
- Cross-language second-mint: a branch minting a parallel of a shared canonical concept C# owns (content-address identity, the proto wire vocabulary, the GLB tessellation rail, the capability descriptor) instead of decoding the one owner at the boundary.
- Universal-vs-capture inversion: a host-neutral owner minted for a contract with no cross-runtime consumer (a Rhino feature wearing universal clothes), or a rich Rhino surface thinned to a host-neutral contract that guts capability.

Settled topology outranks intuition: a placement `architecture.md` rules is never a finding. Finding nothing after a genuine cross-folder pass is a first-class verdict.
