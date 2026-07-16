---
name: docgen
description: >-
    Doc-gen foundation for durable agent-facing documents: the owning-voice register, altitude
    control, file-kind consistency and marker systems, the anti-anchoring law against enumerating
    or freezing facts prose cannot keep true, the named defect catalog (capability-gating,
    legacy-compat, imported upstream posture, process-ledger, deleted-form litanies, coupling),
    and a deterministic prose gate. Use when authoring, editing, reviewing, or rewriting any durable markdown — README,
    ARCHITECTURE, specs, standards, prompts, tool docs, the prose inside skill files — when
    designing a document schema or marker vocabulary, or when hunting noise, fragility,
    staleness, poison framing, or context anchoring. Skill bundle design — triggers, disclosure,
    bundled scripts, evals — belongs to the skill-writer skill; Mermaid fences to
    mermaid-diagramming; interactive HTML artifacts to html-studio. Do not trigger for transient
    chat replies, code-only edits, or research whose deliverable is not durable markdown.
---

# [DOCGEN]

Durable prose is law for an agent that loads it with no memory of why it was written, and every line either changes that agent's next action or poisons it. One register, one owner per fact, one schema per file kind, one decision per line — everything else is deleted, never softened. Skill bundle design — trigger descriptions, disclosure architecture, bundled scripts, evals — is the skill-writer skill's deliverable; this register binds the prose inside every bundle it produces.

## [01]-[ROUTING]

[REFERENCES]:
- [01]-[STRUCTURE](references/structure.md): altitude tiers, routing law, representation choice, table design, file-kind schemas, marker systems
- [02]-[DEFECTS](references/defects.md): defect classes — definitions, detection tests, pairs, reframe rules
- [03]-[REWRITING](references/rewriting.md): rebuilding an existing document without inheriting its frame

[TEMPLATES]: authoring a new instance of a known file kind starts from its template; a finished instance carries zero residual slot tokens and its heading census matches the template.
- [01]-[IDEAS](templates/ideas.template.md): copy verbatim; fill only the H1 token and lead slot
- [02]-[TASKLOG](templates/tasklog.template.md): copy verbatim; fill only the H1 token and lead slot
- [03]-[README](templates/readme.template.md): exact structure, slots only; a multi-folder unit groups cards under one `[FOLDER_TOKEN]:` per folder
- [04]-[ARCHITECTURE](templates/architecture.template.md): exact structure, replace only the slots
- [05]-[SPEC](templates/spec.template.md): exact section spine; slots carry their budgets, cluster count follows the domain
- [06]-[API_CATALOG](templates/api-catalog.template.md): exact structure, replace only the slots
- [07]-[LAWS](templates/laws.template.md): one law family in the shape it needs — table, `[TOKEN]:` cards, or numbered sections; none is the default

[EXAMPLES]: symptom-indexed worked pairs; consult the matching set before building or repairing the container.
- [01]-[TABLES](examples/tables.md): table crimes and their structural repairs
- [02]-[LISTS](examples/lists.md): mega bullets, shredded splits, the classified repair
- [03]-[MARKERS](examples/markers.md): entry leaders, status vocabularies, glyphs
- [04]-[INTROS](examples/intros.md): leads that legislate and the rejected frames

[SCRIPTS]:
- [01]-[PROSE_GATE](scripts/prose_gate.py): deterministic gate and fixer; invocation in the gate section

## [02]-[GATE]

```bash template
uv run scripts/prose_gate.py [--json] <paths...>
```

```bash template
uv run scripts/prose_gate.py fix [--write] <paths...>
```

Run the gate on every touched durable doc before returning. `fix` applies every deterministic repair — header rubrics, the `[INDEX]` column and its `[NN]` entries, alignment colons, H2/H3 numbering, loose numbered list leaders, the colon a floating list label lacks, the label-to-list hug, trailing whitespace, table spacing, and the canonical column-aligned render — printing a per-change plan as a dry run by default and mutating only under `--write`; judgment-tier repairs surface as `SKIP` rows for review. Gate and fixer share one table model, so a grid whose canonical render exceeds the 150-column width cap fails `table-width` regardless of its current padding. It is the mechanical floor — fence, heading, table, link, list, and source section-divider and comment-stack structure, severed rows, fence-intent mislabels, and group labels echoing their heading; link custody, failing a relative link in any file outside the routing class (`README.md`, `SKILL.md`, `CLAUDE.md`, `AGENTS.md`, `MEMORY.md`), beside sibling-interior anchor pointers and page self-references; template residue, bold emphasis in prose and table cells alike, ASCII em-dash misuse, the definite-article opener over prose spans and comment leads in fences and source alike, and trailing whitespace; the banned-lexeme roster of hedges, meta-phrases, self-counts, version anchors and bands, freshness deictics, permission verbs, and grade-and-intensity no-ops — and a skill bundle additionally gates frontmatter shape, name shape and directory identity, description voice and budgets, the root line ceiling, and orphan bundle files, the mechanical floor beneath the skill-writer skill's authoring law; the full check census is the script's `Check` vocabulary. Prose spans alone are scanned: fence bodies, code spans, link destinations, template placeholders, and example-leader rows are structural carriers, not prose findings. Every semantic class and every altitude judgment remains review work against the catalog.

## [03]-[REGISTER]

Register laws produce the voice; every catalog defect is a way of breaking one of them.

[OWNING_SUBJECT] — Every law sentence seats the owner as its grammatical subject, and the verb is an act of ownership over the whole concern: mints, owns, folds, routes, binds, admits, rejects. A total positive claim forecloses every alternative by construction; a surface that merely `supports`, `offers`, or `provides` has not been designed yet. Durable law is the partition principle; a count is a consequence the reader derives, never a ceiling the prose enforces.

```markdown rejected
The package offers an optional graph lane and supports self-hosted deployments.
```

```markdown accepted
Only the self-hosted profile row binds the graph lane.
```

[VERB_LED_OPENER] — No prose opener rides the definite article: a sentence leads with its owner's name, a quantifier, or the claim itself — never `The` — and an annotation after a named surface (the clause a dash or colon hangs off a code-span or link leader) opens with the owning verb, because the leader is the subject and an article opener buries or re-labels it while deciding nothing. Comments answer the same law in a fence body and a source file alike, so lowercasing `The` to `the` is the dodge the gate refuses in both cases: it clears the finding and leaves the subject buried. `article-opener` is the mechanical form — sentence starts, label colons, list entries, leader annotations, and comment leads; the recast is judgment — a named subject or an owning verb, never a synonym noun.

```markdown rejected
- `Codec.Frame` — the frame decoder over the ingress stream.
```

```markdown accepted
- `Codec.Frame` — decodes each ingress frame into the admitted owner.
```

[PROHIBITION_BY_STRUCTURE] — A forbidden form appears only with its enforcing mechanism as the subject of the same clause; never as a roster of corpses, a ritual closing tail, or a naked impossibility claim. One total positive law makes the negation list redundant.

```markdown rejected
Cross-tenant batching is unspellable. A per-op check, an ambient flag, or a second meter is the deleted form.
```

```markdown accepted
Batch keys carry the tenant brand, so a batch is tenant-scoped by construction.
```

[DECISION_PER_LINE] — Each line states one resolved fact in a timeless voice with at most one load-bearing reason: no reassurance, no anticipated objection, no ship-status, no tag, no origin. A reader cannot tell which parts were written when, by whom, or in what order.

```markdown rejected
DECISION [V10]: the trust gate binds every identifier crossing into engine SQL — LANDED, verify and extend, never re-initialize (this replaces the phantom the prior draft carried).
```

```markdown accepted
Every identifier crossing into engine SQL passes the trust gate.
```

[LOAD_BEARING_WORD] — Every word earns its slot or dies, so a finished passage states identical law in a third fewer tokens. Voice is present and active: an owner acts on its concern now, never was-acting, will-act, or is-acted-upon. Deleted on sight — an intensifier grading what the fact already carries, a lead-in delaying the operative verb, a coordinator padding one clause into two, a phrase one verb states, a clause restating its neighbour. Economy is never bought with capability: every decision, condition, and code span survives the cut, and an entry holding two decisions splits into sibling entries, never a longer line.

```markdown rejected
It is important to note that the resolver is a very robust component which will be responsible for handling the seamless mapping of keys, and it also provides comprehensive support for the batch case.
```

```markdown accepted
`Resolver` maps every key, batch included.
```

[CAPABILITY] — Capability lands as present-tense owned fact with deep foresight, or the sentence does not exist. Consumer count is never a design axis: nothing is deferred until demand, defended against an imagined objection, or preserved for an old reader. A capability that cannot yet be modeled gets silence; genuinely open work is a tracked card, never a soft sentence.

```markdown rejected
Incremental rendering is planned once demand justifies it; until then full rebuilds are acceptable for current needs.
```

```markdown accepted
Rendering rebuilds only nodes whose content hash changed.
```

[NAMED_SURFACE] — A nameable surface — member, command, key, flag, token — is written as a code span in its exact verified spelling; verification precedes authoring, an unverifiable member is a tracked open item never settled prose, and a nameable surface paraphrased in prose is a defect: the code span is the instruction.

```markdown rejected
Enable the merge-edges option and the network-simplex placement strategy in the layout block.
```

```markdown accepted
Layout blocks carry `mergeEdges: true` and `nodePlacementStrategy: NETWORK_SIMPLEX`.
```

Rebuilding an existing document under this register is its own discipline — the source's frame is the primary contaminant; run the `rewriting.md` procedure.

## [04]-[FACT_LAW]

[FACT_CLASSES] — Every fact in a durable doc is exactly one of:
- [LAW]: Intent, invariant, boundary, prohibition. Survives any rename. Carried in prose.
- [REPRESENTATION]: A structure snapshot — tree, codemap, diagram. Regenerable from disk, tool-verified, fenced, never restated in prose.
- [REGISTRY]: A fact whose system of record is the doc itself — the only class allowed to enumerate.

[REGENERATION_TEST] — Delete the sentence. A fresh agent regenerating the fact from disk plus the document's stated invariants proves it was a mirror: delete it, or demote it to a fenced representation when structure must be shown. A fact that cannot be regenerated is law kept at the altitude that owns it, still answerable to every defect class: surviving regeneration rules out the mirror alone, never a freeze, hedge, or ledger the catalog deletes.

[ONE_OWNER] — A second prose copy of any fact, at any altitude, is a fork waiting to drift. A pointer names the owner and the one consumed symbol, nothing of its internals; a doctrine is stated once at its charter and never recited per page. A corpus's vocabulary, owners, and policy values arrive settled in every later document — composed as given, never re-taught, renamed, or locally re-defined.

[ANCHOR] — Loaded prose is the working model the next agent generates inside, so every shape the prose names becomes a wall: a member roster anchors the rebuild to today's census, a forbidden-alternative litany anchors it to the anti-shape, a frozen count or seal anchors it to the ceiling. Its cure is always the extension rule — how one new row, case, or member lands — stated where the anchor stood, with the roster living on the surface tooling keeps true.

Altitude tiers, routing law, representation choice, table design, file-kind schemas, and marker systems are `structure.md`'s.

## [05]-[DEFECTS]

Defect classes are the review vocabulary; findings cite class and line. Definitions, detection tests, pairs, and reframe rules are `defects.md`'s.

[STRUCTURAL]: `ENUMERATION_ANCHOR` `STALE_MIRROR` `MECHANISM_LEAK` `META_FRAME` `TWIN_TRUTH` `REPORT_FRAME` `COUPLING`
[SEMANTIC]: `HEDGE` `CAPABILITY_GATE` `LEGACY_COMPAT` `IMPORTED_POSTURE` `VERSION_ANCHOR` `SET_IN_STONE` `WEAK_VERBS` `PROCESS_LEDGER` `ASSERTED_IMPOSSIBILITY` `DELETED_FORM_LITANY`

Skill-bundle authoring classes — triggers, disclosure, instruction bodies — are the skill-writer skill's catalog; a bundle review cites both catalogs, each from its owner.

## [06]-[TABLES]

A table is built right at authoring: a one-sentence lead stating the shared invariant, bracketed rubric headers, a centered `[INDEX]`, explicit alignment colons, atomic cells inside the 150-column rendered width cap — then `scripts/prose_gate.py fix --write` pads. Repair is a sequence of minor surgical moves applied in place, exhausted before conversion is even weighed, and conversion is earned by structural non-tabularity or a type-standard-owned shape, never by cell width — the move sequence, eligibility triple, cell budget, and header compression are `structure.md` [03]'s law.

## [07]-[COMMENTS]

Comments — in source and in transcription-complete fences alike — serve the agent editing the file in isolation. One in-situ constraint the code cannot show, uncoupled from paths, sessions, and siblings, never a duplicate of card or index content; a machine-config comment naming its owning module's path carries an in-situ ownership constraint and survives, while a path that merely narrates is deleted. A comment line fills toward the 150-column width before wrapping; a block is 1-2 wrapped lines, 3-4 only when the constraint truly needs them, and the gate fails a stack past 4 (`comment-stack`) and warns a 2-4 line stack whose every line sits under 100 columns (`comment-shred`) — stacked short fragments merge into one dense line, while a genuinely wrapped block rides its early lines near the cap and passes by construction. A wrapped block never ends on a runt: a trailing fragment far under the width is the re-flow signal (`comment-runt`) — tighten into fewer lines, letting a near-fit collapse ride slightly past 150, or rebalance so the last line carries real width; when content exceeds one cap the split is asymmetric, line one packing to 100 columns or more and the tail keeping 50 or more, never an even split. Header identity rows above the dash divider, shebangs, doc-comment glyphs, dividers, and tool pragmas are structural and never count; the charter docstring below the divider is prose under full comment discipline — it fills toward the cap and merges its shreds — exempt only from the stack cap, since a dense filled charter earns its length. Every pass that touches a file prunes its stale or drifted comments in the same pass; the ladder audits existing comments and never obligates authoring one on a surface whose code already carries the constraint.

Remediation runs one ladder per comment, in order: a comment carrying no load — narration, restatement of the code, a human-facing tour — is deleted whole; a load-bearing comment is tightened in place — active voice, coordinating conjunctions, filler dropped — until the fewest lines carry the constraint; a multi-line survivor re-wraps to fill each line toward the cap, collapsing 3 lines to 2 and 2 to 1 wherever the tightened prose fits; a one-line survivor governing exactly one line or entry inlines as that line's trailing tail. A trailing comment rides its code line outside every count and width budget — the preferred spelling for a per-line or per-entry constraint — while a comment governing a block, category, or more than one line stays a full-line comment above what it governs. Inlining follows tightening, never replaces it: the tail carries the same discipline — short, atomic, re-tightened on every pass that touches the line — and runs long only where the file kind's own convention carries columnar tails. Deletion never drops payload: a constraint the code cannot show survives tightening, never the delete. A merge is a rewrite, never a concatenation — packing runs only over already-tightened prose, and a collapse that joins fragments verbatim or preserves weak prose is a rejected fix. Comment repair is exclusively read-and-rewrite judgment: the gate detects, and no fixer arm, `sed`/regex pass, or scripted bulk rewrite ever mutates comment text — mechanical passes break spacing, structure, and the coherence of the constraint. Section dividers, sub-section dividers, and file docstring headers are structural surfaces — corrected in style, structure, or label, never deleted as comment noise. A divider's label truthfully charters the block below it: a phantom section — a label naming a concern the block does not own, or a full divider standing where a sub-section belongs — is a correction made only after reading the enclosing section structure, never a lexical swap.

## [08]-[CAMPAIGNS]

A backlog too large for one pass runs as an adjudicated campaign: the gate census partitions findings by check class, find-only lanes adjudicate each class against the law files with a closed per-class verdict vocabulary and a CHECK_DEFECTS section, judgment lands the verdicts — a check defect fixes the gate or the law at the owner, never the instance — and fix waves apply the survivors with the live gate as ground truth. A false-positive pattern proven by a dossier is a gate defect by definition; the finding class is repaired before its findings are.

## [09]-[GOTCHAS]

- Rigor's look-alike is the worst defect — a complete roster, an exact count, a frozen chant, a corpse litany; each reads as discipline, rots as law.
- Deleting a defective sentence never deletes capability: demote mechanism to its owner first, then collapse the copy; dropped payload is a defect.
- Legal prohibition is house register, not defect — once at its owner it stands, per page `TWIN_TRUTH`, as a roster `DELETED_FORM_LITANY`.
- Contract qualifiers survive the hedge ban: `optional`, `if present`, `where supported`, `when configured`, `only when`, `unless` scope behavior.
- Future tense survives only on a card growth line and a research item, which carries the exact question plus its verification route, never citations.
- Tooling docs are durable law: a tool doc states its contract and routes verbs to live `--help`, never mirroring output or narrating a command tour.
- Author satisfaction is never the exit: a doc seals on a cold read that raises nothing — the producer's grade admits, the cold read decides.

## [10]-[REPO_INTEGRATION]

When the host repo declares a prose canon in its instruction chain, that canon wins on any conflict and this skill is its portable twin; the repo docs gate consumes `scripts/prose_gate.py --json`.
