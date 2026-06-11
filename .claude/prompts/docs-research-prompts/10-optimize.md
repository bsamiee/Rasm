# [PAGE_OPTIMIZE]

- `<TARGET>` =
- `<DOCTRINE>` =
- `<WORKSPACE>` =

`<TARGET>` exists with its structure and cards in place and no fenced code, and each lane folder in `<WORKSPACE>` holds a `99-distilled.md`. This work runs up to five sequential optimization passes that re-cut the page's cards and structure toward maximum signal in a shrinking footprint.

[GROUNDING]:
- Every pass agent fully reads `<DOCTRINE>`, then `docs/standards/style-guide.md`, `docs/standards/information-structure.md`, and `docs/standards/formatting.md`, before any other work: the laws, the collapse scan, the page craft, and the prose, container, and rendering standards.
- Every pass agent reads every `99-distilled.md` in `<WORKSPACE>` and all of `<TARGET>`.

[PASS] — up to five, sequentially, one fresh agent each:
- Grade first. A pass that finds nothing material states so, makes at most surgical trims, and ends the loop — the pass count is a ceiling, not a quota, and manufactured change is the failure mode.
- The goal is better cards, not more content: merge cards that share a spine, collapse complementary table rows into the card that legislates them, re-cut a weak card around its strongest finding, trim table-stakes and low-signal material, and tighten section order where the structure is weak. The card set this stage leaves is the snippet-selection inventory — every surviving card stands whole without code.
- Hold the page under 300 lines and push the count down, never up; quality rises as the footprint shrinks.
- End the pass with a cold re-read, critique, and grade.

[CARD_LAW]:
- Cards are the unit of value. A card earns its place by deciding something a strong engineer would otherwise get wrong; table-stakes content is banned.
- Cards stack: complementary findings fuse into one dense card rather than scattering as near-peers, and complementary table rows collapse into the card that legislates them. When two or three candidate cards share a spine, they merge. Tables enumerate; cards legislate.

[TABLE_CRAFT]:
- A table is designed with its prose, never in isolation: prose carries the decision criteria and invariants, cells carry atomic values, and neither restates the other.
- Header rubrics are bracketed, uppercase, and semantically rich — the header gives every cell in its column meaning the cell does not repeat.
- Every cell serves its row and every row serves the table's one question. A cell past 8 words, a second prose column, a link in a cell, or a crammed qualification disqualifies the construction — the content moves to a card.
- Narrow beats wide: split by row axis or move detail to cards before widening.

[GRADE]:
- Grade against the page ladder in `_grading.md` at the reports root: the minimum across signal, coverage, card power, structure, and doctrine fit clears the stage bar, automatic fails override any score, and the drift checks apply. Rework until every axis clears.

[COMMIT]: stage and commit `<TARGET>`.
