Question: Which external information-architecture and technical-writing sources should shape a practical chooser for prose, lists, tables, records, and diagrams?
Type: standards-research
Lane: track-external-research
Merge key: docs/standards/information-structure.md :: information signature chooser :: promote
Target owner: docs/standards/information-structure.md
Source basis: active owner reads named by task, prior information-structure lane report, and 6 external sources linked below
Promotion target: docs/standards/information-structure.md
Outcome: PROMOTE

## [FINDINGS]

Finding 1
    Owner route: `docs/standards/information-structure.md`.
    External evidence: Diataxis frames documentation around user needs and the matching forms of tutorial, how-to, reference, and explanation; OASIS DITA treats a topic as the reusable authoring unit and ties topic orientation to retrievability, task support, and minimal instruction.
    Repo fit: The current standard already chooses containers by reader question, and `track-information-structure/01-information-structure-container-toolbelt.md` asks for an input-first chooser. The external evidence supports adding the chooser, but only as a form selector inside the existing owner, not as a new document taxonomy.
    Correction shape: Add an `INFORMATION_SIGNATURE_CHOOSER` before the current reader-question table. It should map raw author inputs to containers, then let type standards decide full document shape.
    Decision: PROMOTE.

Finding 2
    Owner route: `docs/standards/information-structure.md`; craft-only wording ripples route to `docs/standards/style-guide.md`.
    External evidence: NN/G's recognition guidance says visible, easily retrievable information reduces recall burden. Google Technical Writing's list guidance requires parallel list items, which supports stable patterns for similar information.
    Repo fit: This confirms the existing Rasm rule that scannable values should become field lines, lists, records, or tables instead of being hidden in prose islands.
    Correction shape: The chooser should prefer recognition surfaces for lookup, update, proof, and comparison questions: field lines for one record, tables for homogeneous comparison, records for independently maintained items, and diagrams only for relation shape.
    Decision: MERGE.

Finding 3
    Owner route: `docs/standards/information-structure.md`.
    External evidence: Google Technical Writing teaches converting embedded prose lists into bullets or numbered lists, keeping list items parallel, labeling table columns clearly, and moving overlong cell content out of tables. Google Developer Style says a single unit belongs in a list, term/value pairs belong in a description-list-like structure, 3 or more related data pieces commonly belong in a table, one-column tables should become lists, layout tables are rejected, and complex tables should split.
    Repo fit: The existing table rules already set ceilings, prose-cell limits, and one-row-table rejection. The missing piece is a compact arity rule that starts from the author's packet.
    Correction shape: Add a small arity ladder: one item or one-dimensional set -> prose or list; pair fields -> definition block; 3 or more short homogeneous fields across rows -> table; any row needing lists, code, independent proof, or independent update -> per-item record.
    Decision: PROMOTE.

Finding 4
    Owner route: `docs/standards/information-structure.md`; task-standard ripples remain deferred until active edits.
    External evidence: OASIS DITA's minimalist source says readers have goals, want to learn or do something, and should get the smallest amount of instruction that supports successful task completion. Diataxis separates goal-oriented how-to material from information-oriented reference and understanding-oriented explanation.
    Repo fit: This supports the Rasm split between ordered action containers, lookup/reference containers, and explanatory prose. It also argues against a single mega-table that tries to choose every structure through one dense matrix.
    Correction shape: Use a short grouped chooser plus a few subchoosers, not one oversized decision table. Put action sequences in numbered procedures or command-step records, reference facts in lookup tables or records, and explanatory consequences in paragraph pairs.
    Decision: PROMOTE.

Finding 5
    Owner route: `docs/standards/information-structure.md`; accessibility wording ripples route to `docs/standards/style-guide.md`.
    External evidence: Google Developer Style says images should be used only when necessary for visual UI elements or diagrams, not for code, text, or terminal output; it also requires useful visual explanations, contextual introductions, alt text, and longer text descriptions for complex images.
    Repo fit: This confirms the current standard's diagram co-location rule: diagrams own topology, sequence, branch/rejoin, lifecycle, or boundary crossing; records and tables own proof, status, and update facts.
    Correction shape: Add an explicit diagram test to the chooser: choose Mermaid or a codemap only when deleting the graphic removes relation or sequence understanding; otherwise use prose, lists, tables, records, or monospace text. Keep the visible text equivalent beside complex diagrams.
    Decision: MERGE.

## [EVIDENCE]

[SOURCE_SET]:
- Diataxis: https://diataxis.fr/; current official framework site, crawled today by search, used for the user-need/document-form separation.
- OASIS DITA `What are topics?`: https://docs.oasis-open.org/dita/v1.0/archspec/topics.html; stable official specification source, used for topic-based authoring, retrievability, and minimalism.
- Google Technical Writing, lists and tables: https://developers.google.com/tech-writing/one/lists-and-tables; used for prose-to-list conversion, list parallelism, table introductions, and concise cells.
- Google Developer Style, tables: https://developers.google.com/style/tables; last updated 2025-03-21, used for list-versus-table arity, one-column and layout-table rejection, and complex-table splitting.
- NN/G, memory recognition and recall: https://www.nngroup.com/articles/recognition-and-recall/; used for the recognition-over-recall rationale behind visible chooser surfaces.
- Google Developer Style, diagrams and images: https://developers.google.com/style/images; last updated 2025-05-16, used for diagram necessity, text equivalents, and avoiding images for text, code, or terminal output.

The external-source set stays at 6 sources. Treat the Google image page as diagram-specific support, not a separate promotion lane.

## [RECOMMENDATIONS]

[PROMOTE]:
- Add `INFORMATION_SIGNATURE_CHOOSER` to `docs/standards/information-structure.md`.
- Keep the current reader-question chooser as the second pass, after the author has identified the raw information signature.
- Promote the arity rule for lists, definition blocks, tables, and records.
- Add a diagram necessity test that separates relation/sequence graphics from proof/status/update containers.
- Keep command cards, CLI envelope records, API field cards, proof records, and ordered records as subchooser packets, not independent standards.

[CORRECTION_SHAPES]:
- Raw input signature -> container row.
- Container row -> reject row where misuse is common.
- Subchooser only when one row needs finer routing, such as table forms, decision tables, diagrams, command/output packets, or record packets.
- Proof-bearing or drift-prone fact -> proof record routed through `docs/standards/proof.md`, not a new proof vocabulary in this owner.
- Diagram plus text equivalent -> co-located pair only when each carries a distinct reader job.

[DO_NOT_PROMOTE]:
- Do not replace the existing reader-question table with a larger external-framework taxonomy.
- Do not route Diataxis or DITA document-type categories into `information-structure.md`; they support the standard but do not own Rasm type-standard routing.
- Do not use screenshots, images, or Mermaid diagrams for text, code, command output, status, proof, or lookup data.
- Do not create a new active standard for "structure chooser"; the owner is already `docs/standards/information-structure.md`.

## [CANDIDATE_WORDING]

```markdown template
[INFORMATION_SIGNATURE_CHOOSER]:
- One claim, boundary, caveat, consequence, or transition -> prose paragraph; use a paragraph pair when the next unit states consequence, proof, or route-away.
- One unordered set of peer facts -> bullets; use numbered steps only when order changes the outcome.
- One item's fields -> definition block; promote to a subsection record when fields need lists, code blocks, proof, or independent updates.
- Repeated homogeneous items with short shared fields -> table; split or promote to records when cells become prose, nested, independently updated, or too wide.
- One key to one value, owner, class, or behavior -> lookup table or classification table profile.
- Multiple independent conditions to one action -> decision table with hit policy.
- Ordered operation -> numbered procedure; use a command-step record when command, expected signal, failure route, and proof travel together.
- Reusable command or output contract -> command card, CLI envelope record, API field card, or field packet.
- Relation, topology, branch/rejoin, sequence, lifecycle, or boundary crossing -> Mermaid, codemap tree, type-shape block, or edge list; keep proof, status, and update facts in records or tables.
- Literal input, output, config, schema, or source-shaped example -> fenced code block with language and intent.
- Drift-prone claim -> proof record beside the claim, using `proof.md` field semantics.
```

## [PROOF_GAPS]

- Active standards were not edited, so this report does not prove final wording against the whole active corpus.
- DITA is older than the repo's normal freshness window, but it is an official stable architecture specification and is used only for settled topic-based-authoring principles.
- No renderer behavior was validated. Any active Mermaid or image-example change still needs local renderer proof or an explicit proof gap.
- The prior lane report included repo samples outside this task's allowed read set. This report uses that prior report only as source material and does not reassert its sampled repo line evidence as current proof.

## [USER_CHOICE_POINTS]

- Decide whether `classification table` should be named as its own profile or folded into `lookup table` to minimize shape count.
- Decide whether the promoted chooser should be a compact table or grouped definition block; the external evidence supports both, but Rasm's current style likely favors a grouped chooser if the table would exceed one prose column.
- Decide whether command/output packet names are ready to promote now or should wait for the command-surface report lane.
- Decide whether diagrams should require a visible "job split" sentence every time they sit beside a table or record, or only when duplication risk is visible.
