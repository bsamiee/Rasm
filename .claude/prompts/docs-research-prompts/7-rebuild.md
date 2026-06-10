# [PAGE_REBUILD]

- `<TARGET>` =
- `<DOCTRINE>` =
- `<WORKSPACE>` =

`<TARGET>` exists as a published page, and each lane folder in `<WORKSPACE>` holds a `00-distilled.md`. This work replaces the page from the distilled reservoir — a greenfield draft first, an earned-value reconciliation second. The replacement is structure, tables, cards, and prose only: no fenced code, and no card may lean on a future snippet to stand.

[GROUNDING]:
- Fully read `<DOCTRINE>`, then `docs/standards/style-guide.md`, `docs/standards/information-structure.md`, and `docs/standards/formatting.md`, before any other work: the laws, the collapse scan, the page craft, and the prose, container, and rendering standards. Every decision answers to them.
- Read the sibling pages in `<TARGET>`'s folder for the corpus voice — every page except `<TARGET>` itself, which stays unread until reconciliation. The replacement takes whatever form serves it best — siblings are reference, not a mold, and the incumbent is neither.

[PLAN]:
- Read every `00-distilled.md` in `<WORKSPACE>`. Plan the replacement before writing it: section order, the card inventory with each card's stacked concerns named, which distilled findings feed each card, and which tables exist with the one question each answers. The plan is working state, never a file — selection happens before sentences exist to defend themselves.

[DRAFT]:
- Author the replacement at or under 225 lines, without reading the incumbent page: its structure derives from the distillates and the doctrine, never from the page it replaces.
- The opening is the most expensive real estate on the page: the lead paragraph states the page's whole law, and the first screen stands alone as an operating brief — no warm-up, no inventory, no naming of what the page covers.
- Findings-grade prose only: state as fact and law, with no meta-commentary, no sourcing, no links, and no narration of what the page is or does.
- Re-read the draft cold and critique it harshly: opening signal, table width and quality, card power, capture of the distillates' strongest material, and doctrine fit. Rework, then grade; the draft must clear before reconciliation.

[RECONCILE]:
- Only after the draft clears, read the incumbent page once, whole.
- Harvest only earned value the draft lacks and the reservoir could not see: verified repository decisions, named routes, constraints, vocabularies, and facts the page carries from real use. Judge each by strength against what the draft already holds — weaker overlap stays behind, stronger replaces, distinct value integrates into the owning card, row, or section.
- The incumbent's structure, section order, card shapes, and snippets carry no authority; the draft's shape stands, and snippets are not carried over.
- Replace the file with the reconciled draft, still at or under 225 lines, then re-read cold, critique, and grade again.

[CARD_LAW]:
- Cards are the unit of value and the hardest part of the page. A card earns its place by deciding something a strong engineer would otherwise get wrong; table-stakes content is banned.
- Cards stack: complementary findings — a rule, its accept and reject edges, its boundary, and the interaction that makes it advanced — fuse into one dense card rather than scattering as near-peers, and complementary table rows collapse into the card that legislates them.
- When two or three candidate cards share a spine, they merge. Tables enumerate; cards legislate.

[TABLE_CRAFT]:
- A table is designed with its prose, never in isolation: prose carries the decision criteria and invariants, cells carry atomic values, and neither restates the other.
- Header rubrics are bracketed, uppercase, and semantically rich — the header gives every cell in its column meaning the cell does not repeat.
- Every cell serves its row and every row serves the table's one question. A cell past 8 words, a second prose column, a link in a cell, or a crammed qualification disqualifies the construction — the content moves to a card.
- Narrow beats wide: split by row axis or move detail to cards before widening.

[GRADE]:
- Score one to ten on five axes — signal, coverage, card power, structure, doctrine fit — against a bar of 9.2 on each. Grading is critical, harsh, but fair — not a 9.2 out of convenience, not low for nit-picking. Rework until every axis clears.

[COMMIT]: stage and commit `<TARGET>`.
