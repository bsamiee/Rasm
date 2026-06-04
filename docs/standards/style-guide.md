---
description: Cross-cutting prose, terminology, notation, and Markdown style standard
---

# Style guide

This standard controls the words inside Rasm documentation: sentence shape,
paragraph shape, terminology, punctuation, links, examples, and code-safe
Markdown. Document type comes from [documentation-system.md](documentation-system.md);
page structure comes from [information-structure.md](information-structure.md);
evidence comes from [proof.md](proof.md).

## Use when

Use this standard when writing or reviewing:

- prose in durable documentation;
- headings, paragraphs, lists, captions, notes, warnings, and examples;
- commands, paths, symbols, placeholders, and technical notation;
- terminology choices, acronyms, UI labels, product names, and package names;
- accessibility and localization-sensitive wording.

Do not use it to decide document type, evidence strength, retrieval metadata, or
operational procedure shape. Route those concerns to the owning standard.

## Source order

Use the first source that decides the question:

1. Current repository source, manifests, generated contracts, API metadata, and
   verified product or package names.
2. The nearest active document-type standard in this folder.
3. Official vendor documentation for external product names, UI labels, APIs,
   commands, and support status.
4. This guide for local prose and notation rules.
5. Microsoft, Apple, Red Hat, IBM, Chicago 18, Merriam-Webster, and
   Elements-style prose craft for unresolved mechanics.

When sources conflict, choose the repo-local term only if a current owner or
contract proves it. Do not alternate terms to avoid the conflict.

## Prose rules

- Put the controlling rule, constraint, or outcome before supporting detail.
- Keep one controlling idea in each paragraph.
- Prefer concrete nouns and verbs over abstract labels.
- Use active voice for instructions and decisions by default.
- Use passive voice when the actor is irrelevant, unknown, or less important
  than the technical result.
- Use present tense for durable standards, future tense for planned work, and
  past tense only for historical evidence or completed verification.
- Use positive form when it is clearer than describing what not to do.
- Use `must`, `should`, and `may` only for requirements, recommendations, and
  permitted options.
- Remove filler, marketing claims, hedging, author notes, task history, and
  transient interaction language.
- Keep exceptions close to the rule they modify.
- Use sentence-style headings unless a fixed template label or official name
  requires another form.

## Paragraph architecture

Use a front-and-close pattern for durable documentation. The first sentence of
a paragraph carries the point: rule, claim, scope, outcome, or transition. The
middle sentence qualifies, explains, or proves the point. The final sentence
closes with the consequence, boundary, evidence requirement, or next route.

Do not hide the strongest constraint in the middle of a paragraph. If a reader
or agent must obey a condition, put the condition first or give it its own
sentence.

Keep paragraph endings strong. End on the term, action, proof, or owner the
reader should retain, not on a weak qualifier such as `as needed` or `in some
cases` unless that qualifier is the rule.

## Sentence mechanics

Use coordination to show equal ideas and subordination to show dependency.
FANBOYS coordination is a readability heuristic, not a ritual.

- Use a comma before a coordinating conjunction that joins two independent
  clauses.
- Use a semicolon before and a comma after a conjunctive adverb joining two
  independent clauses.
- Do not join independent clauses with only a comma.
- Use the serial comma in lists of three or more items.
- Break center-embedded or multi-condition sentences into shorter sentences.
- Keep parallel items parallel in grammar and scope.
- Use transitions such as `First`, `Then`, and `Finally` only when order
  matters.
- State a condition before the action it controls: `If <signal>, do <action>`.

## Terminology

- Preserve verified product, package, API, command, file, and UI names exactly.
- Use one term for one concept within a bounded context.
- Define domain terms before relying on them unless the target reader must
  already know the term.
- Introduce acronyms once per document unless the acronym is more recognizable
  than the expansion.
- Do not invent reader-facing names for internal concepts.
- Do not name stale commands, removed tools, temporary task labels, or obsolete
  skills unless a current owner proves they are still supported.

## Punctuation, numbers, and capitalization

- Use U.S. English spelling unless an official product or source name differs.
- Use sentence-style capitalization for headings and ordinary labels.
- Use title case only for titles of works, official labels, and externally
  required names.
- Use numerals for versions, measurements, commands, flags, identifiers, and
  values that readers compare.
- Spell out small numbers only when they are ordinary prose and not compared,
  configured, versioned, measured, or part of a sequence.
- Keep units attached to values and state the unit when ambiguity is possible.
- Use straight quotes and ASCII-safe punctuation in source Markdown that readers
  may copy into commands, code, config, or issue trackers.

## Code-safe Markdown

- Wrap commands, flags, file paths, environment variables, package IDs,
  symbols, literal values, and placeholders in backticks.
- Use placeholders only when the reader must substitute a value. Name the
  placeholder by role, such as `<scenario-glob>` or `<package-name>`.
- Do not include shell prompts in copyable commands unless the prompt itself is
  the subject.
- Use fenced blocks with info strings when a block is necessary.
- Keep examples short enough to review.
- Summarize long command output and include the command or source that
  reproduces it.
- Link generated contracts, manifests, or source files instead of copying long
  machine-readable output.

## Links

- Use link text that describes the destination.
- Do not use bare URLs unless the URL itself is the example value.
- Prefer canonical repo docs, generated contracts, manifests, or official
  vendor documentation.
- Avoid deep external links when a stable top-level reference carries the same
  truth.
- Mark generated, mirrored, or version-sensitive targets in the owning document
  when freshness can drift.

## Examples

Examples must prove shape or prevent misuse.

- Put examples next to the rule they clarify.
- Prefer one accepted shape and one rejected shape when the distinction matters.
- Mark reuse risk before examples that could be copied, executed, or mistaken
  for current policy: `copy-safe`, `conceptual`, `test-only`, `generated`,
  `output-only`, `migration-only`, or `rejected`.
- Keep placeholder values explicit.
- Do not publish interaction fragments, private paths, local task notes, or
  machine-specific values as reusable examples.

## Accessibility and localization

- Do not rely on color, position, shape, pointer movement, screenshots, or sound
  alone.
- Provide text equivalents for visual, audio, or screenshot-only information.
- Use input-neutral UI verbs when keyboard, pointer, touch, voice, or automation
  paths can all perform the action.
- Describe UI labels exactly when they are action targets.
- Prefer conventional grammar and punctuation for machine translation.
- Avoid idioms, culture-specific jokes, and unnecessary figurative wording.
- Use inclusive, role-based descriptions rather than personal traits unless the
  trait is technically relevant.

## Final proofing pass

Review in four passes before publication:

1. Composition: scope, controlling idea, paragraph order, section endings, and
   adjacent-owner links.
2. Mechanics: grammar, punctuation, capitalization, numbers, parallelism, and
   code-safe Markdown.
3. Technical truth: names, commands, paths, versions, links, examples, and
   evidence anchors.
4. Accessibility: sensory cues, UI labels, alt text or equivalents, and
   translation-sensitive wording.

## Review checklist

- [ ] Repo-local and vendor names are preserved exactly.
- [ ] One concept has one term.
- [ ] Paragraphs carry one controlling idea and close on the intended owner,
      action, proof, or boundary.
- [ ] Sentences are direct, concrete, and free of needless words.
- [ ] Conditions appear before controlled actions.
- [ ] Headings use sentence style unless an official label requires otherwise.
- [ ] Commands, paths, flags, symbols, and placeholders use backticks.
- [ ] Examples are necessary, labeled when reuse risk exists, and placed near
      the rule they clarify.
- [ ] Links use meaningful text and point to canonical sources.
- [ ] Accessibility does not depend on sensory-only cues.
- [ ] No transient task language, obsolete command, or unsupported product claim
      remains.
