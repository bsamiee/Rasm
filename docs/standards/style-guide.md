---
description: Sentence and word craft, terminology, notation, and code-safe Markdown
---

# Style guide

This standard owns craft: the words inside a document, the sentences that carry them, terminology, punctuation, links, and code-safe Markdown. Apply it after the document type and container are chosen. It does not decide where content sits in a unit, which container holds it, or how strong the evidence behind a claim is.

## Use when

Apply this standard when writing or reviewing:

- prose, headings, captions, notes, warnings, and example wording;
- commands, paths, symbols, placeholders, and technical notation;
- terminology, acronyms, UI labels, product names, and package names;
- accessibility-sensitive wording and text equivalents.

Salience and ordering, container form, and evidence strength belong to the position, form, and evidence standards.

## Source order

Use the first source that decides a naming or wording question:

1. Current repository source, manifests, generated contracts, API metadata, and verified product or package names.
2. The active document-type standard for the document being written.
3. Official vendor documentation for external product names, UI labels, APIs, commands, and support status.
4. This guide for local prose and notation rules.
5. Established editorial style references for unresolved general mechanics.

When sources conflict, choose the repo-local term only when a current owner or contract proves it, and do not alternate terms to dodge the conflict.

## Prose rules

- Put the controlling rule, constraint, or outcome before supporting detail.
- Keep one controlling idea in each paragraph.
- Prefer concrete nouns and verbs over abstract labels.
- Use active voice for instructions and decisions by default; use passive only when the actor is irrelevant or less important than the result.
- Use present tense for durable standards, future tense for planned work, and past tense only for historical evidence or completed verification.
- Write instructions as positive imperatives that name the action to take; convert `do not omit the unit` into `attach the unit to its value`. The cognition rationale for this framing is the position standard's concern.
- Use `must`, `should`, and `may` only for requirements, recommendations, and permitted options.
- Remove filler, marketing claims, author notes, and transient interaction language; cut a hedge that carries no information, but keep a qualifier that marks genuine uncertainty and let the evidence standard govern it.
- Keep each exception next to the rule it modifies.

## Paragraph architecture

Use a front-and-close shape. The first sentence carries the point: rule, claim, scope, outcome, or transition. The middle qualifies, explains, or proves it. The final sentence closes on the consequence, boundary, evidence requirement, or next route. Do not hide the strongest constraint in the middle of a paragraph; if a reader must obey a condition, give it the first sentence or its own. End on the term, action, proof, or owner the reader should retain, not on a weak qualifier.

## Sentence mechanics

- Use a comma before a coordinating conjunction joining two independent clauses.
- Use a semicolon before and a comma after a conjunctive adverb joining two independent clauses, and do not join independent clauses with a comma alone.
- Use the serial comma in lists of three or more items.
- Break center-embedded or multi-condition sentences into shorter sentences.
- Keep parallel items parallel in grammar and scope.
- Use ordering transitions such as `First`, `Then`, and `Finally` only when order matters.
- State a condition before the action it controls: `If <signal>, do <action>`.

## Terminology

- Preserve verified product, package, API, command, file, and UI names exactly.
- Use one term for one concept within a bounded context.
- Define a domain term before relying on it unless the target reader must already know it.
- Introduce an acronym once per document unless the acronym is more recognizable than its expansion.
- Do not invent reader-facing names for internal concepts.
- Do not name stale commands, removed tools, temporary task labels, or obsolete skills unless a current owner proves they are still supported.

## Punctuation, numbers, and notation

- Use U.S. English spelling unless an official product or source name differs.
- Use sentence-style capitalization for headings and ordinary labels; reserve title case for titles of works, official labels, and externally required names.
- Use numerals for versions, measurements, commands, flags, identifiers, and any value readers compare, and keep units attached to their values.
- Use straight quotes and ASCII-safe punctuation in any Markdown a reader may copy into commands, code, config, or trackers; typographic punctuation is acceptable only in prose that is never copied as a literal.

## Code-safe Markdown

- Wrap commands, flags, paths, environment variables, package IDs, symbols, literal values, and placeholders in backticks.
- Name a placeholder by role, such as `<scenario-glob>` or `<package-name>`, and use one only when the reader must substitute a value.
- Omit the shell prompt from a copyable command unless the prompt is the subject.

## Links

- Use link text that describes the destination, and avoid bare URLs unless the URL is the example value.
- Prefer canonical repo docs, generated contracts, manifests, or official vendor documentation, and avoid deep external links when a stable top-level reference carries the same truth.
- Mark a generated, mirrored, or version-sensitive target in the owning document when freshness can drift.

## Examples

Examples must prove shape or prevent misuse. Place an example beside the rule it clarifies per the form standard, and own only its wording here.

- Prefer one accepted shape and one rejected shape when the distinction matters.
- Keep an example's wording consistent with the terminology it teaches.
- When a block could be copied, run, or mistaken for current policy, mark its reuse risk; the intent-label vocabulary is the form standard's concern.

The pair below shows the discipline this section names: a hedge-laden sentence rejected, and its de-hedged positive-imperative rewrite marked copy-safe.

```text rejected
It is probably best to try to avoid not stating the unit when you can.
```

```text copy-safe
Attach the unit to its value.
```

## Accessibility

- Carry meaning through text, not through color, position, shape, pointer movement, screenshots, or sound alone.
- Provide text equivalents for visual, audio, or screenshot-only information, and write alt text that states what the image proves.
- Use input-neutral UI verbs when keyboard, pointer, touch, voice, or automation can all perform the action, and describe a UI label exactly when it is the action target.
- Prefer conventional grammar and punctuation for machine translation, and avoid idioms and culture-specific figurative wording.

## Final proofing pass

Review in four passes before publication:

1. Composition: scope, controlling idea, paragraph order, and section endings.
2. Mechanics: grammar, punctuation, capitalization, numbers, parallelism, and code-safe Markdown.
3. Technical truth: names, commands, paths, versions, links, and examples.
4. Accessibility: sensory cues, UI labels, and text equivalents.

## Boundaries

- [agentic-documentation.md](agentic-documentation.md) owns content placement and the cognition rationale behind positive, imperative framing.
- [information-structure.md](information-structure.md) owns container form, example placement, and the code-block intent-label vocabulary.
- [proof.md](proof.md) owns evidence strength and when a hedge is a load-bearing uncertainty marker rather than filler.
- [formatting.md](formatting.md) owns status and invocation markers, table styling, and whitespace; this standard owns the words.
- [README.md](README.md) owns document-type routing and cross-standard links.

## Review checklist

- [ ] Repo-local and vendor names are preserved exactly.
- [ ] One concept has one term.
- [ ] Paragraphs carry one controlling idea and close on the intended owner, action, proof, or boundary.
- [ ] Sentences are direct, concrete, and free of needless words.
- [ ] Conditions appear before the actions they control.
- [ ] Headings use sentence style unless an official label requires otherwise.
- [ ] Commands, paths, flags, symbols, and placeholders use backticks.
- [ ] Examples are necessary and placed beside the rule they clarify.
- [ ] Links use meaningful text and point to canonical sources.
- [ ] Accessibility does not depend on sensory-only cues.
- [ ] No transient task language, obsolete command, or unsupported product claim remains.
