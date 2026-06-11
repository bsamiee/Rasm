# [REGION_MAP]

Companion artifact to the doc-stack prompt set — the template and law for the region ledger. The orchestrator installs one ledger at the workspace root (`<REGIONS>`) when a stack's first pipeline begins, seeded from every fence and every finalized page already in the corpus. The ledger is the corpus's ownership memory: it makes duplication detectable without re-reading the corpus, and it is what lets a new page build on finalized pages implicitly — owned territory is visible before research begins. Concept pages never cite it.

## [1]-[LAW]

- Two altitudes, one ledger. PAGE_REGIONS records, per finalized page, the concerns its cards legislate — prose ownership at card altitude. SNIPPET_REGIONS records, per fence, the surface region it demonstrates — code ownership at demonstration altitude.
- A region has exactly one owner. A snippet demonstrates a region no other snippet in the corpus shows; a card legislates a concern no sibling page owns. Finalized surfaces composed as supporting material occupy no region and duplicate nothing — the region is the spotlight, not the cast.
- Write order is the integrity guarantee: snippet rows are written BEFORE any code exists (the assignment is the duplication contract the code is then held to); page rows are written at finalization, when the clean verification pass flips the page's state.
- A duplicated region is repaired by routing to its owner — the owner keeps the demonstration or law, the duplicate is rebuilt on an unclaimed region — never by deleting the nuance and never by re-teaching.
- Consumers: lane definition reads PAGE_REGIONS so OWNS/ASSUMES lines exclude owned territory before research begins; distillation's ownership pass routes atlas-owned material out by it; authoring and review stages test re-teach against it; snippet stages hold SNIPPET_REGIONS as the automatic-fail arbiter; the corpus sweep re-grades the whole ledger.
- Entries carry no proper nouns except admitted package names, no paths, and no process narration — a region row is a neutral description of territory, readable for any codebase in the stack's domain.

## [2]-[STRUCTURE]

One H1, then three sections:

[PAGE_REGIONS]: a narrow table — `[PAGE]` and `[REGIONS]` — one row per finalized page, the regions cell holding the card-altitude concerns as short semicolon-separated phrases. Rows appear only at finalization and are amended only when a page is re-finalized.

[SNIPPET_REGIONS]: a narrow table — `[ID]`, `[PAGE]`, `[CARD]`, `[REGION]` — one row per fence, IDs prefixed per page. Rows are written at snippet assignment, updated when refinement deepens a region, and removed only when the snippet is removed.

[KNOWN_OVERLAPS]: recorded collisions awaiting repair — each names the colliding entries, the assigned owner, and the stage where the repair lands. An overlap is a debt with a due date, never a tolerated state.
