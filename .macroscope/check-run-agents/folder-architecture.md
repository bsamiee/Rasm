---
title: Rasm Folder Architecture
input: full_diff
effort: medium
conclusion: neutral
include:
  - "libs/**"
---

# [FOLDER_ARCHITECTURE]

Folder-topology richness is this pass's one question: whether the sub-domain tree grows into real domain concepts or accretes thin, generically-named, or eponymous folders. Judge against `libs/.planning/ARCHITECTURE.md` (no weak-or-mini sibling, source-mirroring sub-domains, planning lifecycle) and `libs/.planning/README.md` (codemap law, doc-set, gap-fuelling `IDEAS`/`TASKLOG`), reading them before any finding. This owns the qualitative structure judgment; the mechanical checks — router-row and codemap 1:1, branch casing, the richness floor and eponymous-pair rows — belong to the correctness planning-corpus check and are not repeated here.

Hunt these classes, each finding naming the folder, the structural defect, and the stronger shape:
- Eponymous restatement: a sub-domain folder whose single page only restates the folder name and adds no domain structure the folder concept implies — a thin concern that folds into the bigger concept it belongs to rather than standing as its own sub-tree. A legitimately atomic sub-domain is not a finding; the defect is thinness against an implied family.
- Weak-or-mini sibling: a small isolated folder that is a diminished twin of a sibling where one richer owner spans both.
- Generic naming: a codemap node or eventual-source file named by a generic file-naming scheme (`helpers`, `utils`, `misc`, `manager`) rather than its real domain concept.
- Empty-sub-domain gap: a planned sub-domain named in the `ARCHITECTURE.md` codemap with no design page and no idea or task seeding the gap — the domain map fuels `IDEAS.md`/`TASKLOG.md`, so a visible gap with no forward pool entry is the finding.
- Lifecycle breach: a `.planning/` scaffold nested inside a real source sub-folder, more than one `.planning/` per package, or design pages living outside the single package-root `.planning/`.
- Richness deficit: a sub-domain carrying one shallow page where the domain concept implies a family of owners — the folder shaped for the instance in hand rather than the domain it names.

Settled structure outranks intuition: a folder shape `ARCHITECTURE.md` or the folder `ARCHITECTURE.md` rules is never a finding. A clean cross-folder pass states that verdict plainly.
