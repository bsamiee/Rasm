# [STYLE_GUIDE]

This standard carries craft: the words inside a document, sentence mechanics, terminology, punctuation, links, and code-safe Markdown. Apply it after the document type and container are chosen. It does not decide content position, container form, or evidence strength.

## [1][USE_WHEN]

Apply this standard when writing or reviewing:
- prose, headings, captions, notes, warnings, and example wording
- commands, paths, symbols, placeholders, and technical notation
- terminology, acronyms, UI labels, product names, and package names
- accessibility-sensitive wording and text equivalents

Salience and ordering, container form, and evidence strength belong to the position, form, and evidence standards.

## [2][WORDING_PRECEDENCE]

Use the first source that decides a naming or wording question:
1. Current repository source, manifests, generated contracts, API fields, and verified product or package names.
2. The active document-type standard for the document being written.
3. Maintained source for product names, UI labels, APIs, commands, and support status.
4. This guide for local prose and notation rules.
5. Established editorial style references for unresolved general mechanics.

When sources conflict, choose the repo-local term only when a current route or contract proves it. Never alternate terms to dodge the conflict.

For unresolved sentence composition, use concision, parallelism, positive form, and concrete language. For unresolved punctuation, capitalization, numbers, and quotation mechanics, use established editorial style unless repository notation or code-safe Markdown overrides it.

## [3][PROSE_RULES]

[SHAPE]:
- Put the controlling rule, constraint, or outcome before supporting detail.
- Keep one controlling idea in each paragraph.
- Keep each exception next to the rule it modifies.

[VOICE]:
- Prefer concrete nouns and verbs over abstract labels.
- Use active voice for instructions and decisions by default.
- Use passive voice when the actor is irrelevant or less important than the result.
- Use present tense for durable standards, future tense for planned work, and past tense only for historical evidence or completed verification.
- Write instructions as positive imperatives that name the action to take. Convert `do not omit the unit` into `attach the unit to its value`. The cognition rationale for this framing is the position standard's concern.
- Use `must`, `should`, and `may` only for requirements, recommendations, and permitted options.

[NOISE_REMOVAL]:
- Remove filler, marketing claims, draft notes, and transient interaction language.
- In coding-project documents, prefer concrete paths, commands, contracts, failure modes, gates, and artifacts over vague planning or operational prose. Remove generic routing language, alignment language, momentum language, or value language when it does not name a real route or change an implementation, validation, access, support, or recovery decision.
- Reject prompt-era process vocabulary in durable docs unless quoted as archival evidence: `dictum`, `dossier`, `stage N`, `validated snippet`, task-ledger labels, and agent-invented abstraction names. Prefer current source names, configured command names, public symbols, and maintained document types.
- Cut a hedge that carries no information. Preserve a qualifier that marks genuine uncertainty, and let the evidence standard govern it.

## [4][PARAGRAPH_ARCHITECTURE]

Use a front-and-close shape. The first sentence states the rule, claim, scope, outcome, or transition. The middle qualifies, explains, or proves it. The final sentence closes on the consequence, boundary, evidence requirement, or next route.

Give a required condition the first sentence or a separate paragraph. Do not hide the strongest constraint in the middle. End on the term, action, proof, or route the reader should retain, not on a weak qualifier.

Use isolated sentences deliberately. A one-sentence paragraph must be a lead, transition, caption, closing consequence, route boundary, or explicit proof gap. If the sentence is only a named fact, make it a field line. If it is only a category name, make it a group label or heading. If it is only emphasis, delete it or fold it into the controlling paragraph.

Use paragraph pairs when one idea needs a setup and a consequence. The first paragraph states the claim or rule; the second paragraph states the operational consequence, exception, proof route, or route-away. Do not let two short paragraphs become loose labels with periods. A prose sentence stays a sentence when it expresses a complete thought, predicate, and reader consequence; imperative sentences may use an implied subject. Do not turn a sentence into a label.

[KEEP_OR_CONVERT]:
- Keep a standalone sentence when it leads, transitions, captions, closes, states a consequence, names a route boundary, or marks a proof gap.
- Convert it to `Label: value` only when the value is scanned, quoted, or updated independently.
- Convert it to a note, alert, or row-owned record only when its scope is local to a row, claim, risk, or provenance point.
- Delete or fold it when it only repeats emphasis.

Use the smallest carrier that preserves the reader job. A packet with only 1 or 2 independently useful fields usually wants prose or a short definition block, not a full template. Omit absent fields rather than filling a packet to satisfy shape.

## [5][SENTENCE_MECHANICS]

[CLAUSE_JOINING]:
- Coordinating conjunctions are `for`, `and`, `nor`, `but`, `or`, `yet`, and `so`.
- Use a comma before a coordinating conjunction joining two independent clauses.
- For imperative clauses joined by a coordinating conjunction, omit the comma when they share an implied subject and cannot be misread. Split or punctuate when contrast, length, or ambiguity appears.
- Omit the comma before a coordinating conjunction that joins only a compound predicate.
- Use a semicolon before and a comma after a conjunctive adverb joining two independent clauses: `however`, `therefore`, `otherwise`.
- Do not join independent clauses with a comma alone.

[RANK_PARALLELISM]:
- Join elements of equal rank with a coordinating conjunction: for, and, nor, but, or, yet, so.
- Subordinate one element to another with a subordinating conjunction: because, although, since, while, if, when.
- Coordinate only equal ideas and subordinate unequal ones; do not chain equal clauses with repeated `and` or `but`.
- Construct both arms of a correlative pair in parallel: `both ... and`, `either ... or`, `not only ... but also`.
- Keep parallel items parallel in grammar and scope.

[CLARITY]:
- Use `that` for a restrictive clause; use `which`, set off by commas, for a nonrestrictive one.
- Use sentence-initial conjunctions, split infinitives, and terminal prepositions when they produce the clearest technical sentence.
- Break center-embedded or multi-condition sentences into shorter sentences.
- Use ordering transitions such as `First`, `Then`, and `Finally` only when order matters.
- State a condition before the action it controls: `If <signal>, do <action>`.
- Keep modifiers and related words next to the terms they govern.

## [6][TERMINOLOGY]

- Preserve verified product, package, API, command, file, and UI names exactly.
- Use one term for one concept within a bounded context.
- Define a domain term before relying on it unless the target reader must already know it.
- Introduce an acronym once per document unless the acronym is more recognizable than its expansion.
- Use singular `they` when gender is unknown, irrelevant, or intentionally unspecified.
- Do not invent reader-facing names for internal concepts.
- Do not name stale commands, removed tools, transient task labels, or removed skills unless a current route proves they are still supported.

## [7][PUNCTUATION_NUMBERS_NOTATION]

[LOCAL_MECHANICS]:
- Use U.S. English spelling unless a product or source name differs.
- Form the possessive singular by adding `'s` regardless of the final consonant: `Charles's`, `the witness's`, `the boss's`.
- Use the serial comma in lists of three or more items.
- Use one space after terminal punctuation.

[STOPS_SEPARATORS]:
- Use a colon after a complete sentence that directly introduces a list, table, example, consequence, or complete explanation.
- Capitalize the first word after a colon only when the following material is a complete sentence. Keep fragments, definition-block values, table cells, and field continuations lowercase unless source casing differs.
- Use semicolons only for closely related independent clauses or complex inline lists with internal commas.
- Prefer sentence splits or vertical lists in agent-facing documentation, and do not end bullet items with semicolons.
- Use parentheses for nonessential clarification; promote required conditions into the main sentence. If a parenthetical is a full sentence, put the period inside the closing parenthesis. If it is part of the enclosing sentence, put the period outside.

[DASHES_HYPHENS]:
- Use spaced em dashes for prose interruptions.
- Route field separators, checklist trailing fields, and table absence values to [formatting.md](formatting.md).
- Use en dashes for inclusive ranges, open-compound modifiers in prose such as `New York–based`, and compound modifiers formed from two names.
- Hyphenate compound modifiers before nouns when needed for clarity.
- Omit needless hyphens after `-ly` adverbs, and use a suspended hyphen for shared compounds such as `short- and long-term`.
- Use ASCII hyphen-minus in commands, flags, paths, slugs, identifiers, code, config, tracker literals, and copyable text.

Notation rules use these groups:

[MARKUP_LABELS]:
- Avoid slashes for prose alternatives; use `or`, `and`, or `or both`. Keep slashes for code, paths, URLs, and exact product syntax.
- Use `[X_Y_Z]:` for a standalone group label before a list or table. Formatting owns the exact group-label shape. Keep prose labels short and concrete.
- Use `Label: value` only for a short item-scoped fact, checklist field, or definition-block record line. Formatting owns field-line rendering; this guide owns label wording.
- Do not bold a whole label line for emphasis. Promote it to a heading when it needs an anchor or independent retrieval; otherwise keep it as a bracketed set label.

[NUMBERS_QUOTES]:
- Reserve title case for titles of works, source labels, and verified names outside bracketed notation. In title case, capitalize prepositions of 5 or more letters and capitalize later elements in hyphenated prefixed compounds when the title style requires it.
- Use numerals for versions, measurements, commands, flags, identifiers, dates, editions, counts, thresholds, exact quantities, and any value readers compare, and keep units attached to their values. Spell out isolated nontechnical counts from zero through nine only when the value is not a comparison, threshold, field value, or literal. A year may open a sentence in numeral form.
- In ordinary prose quotations, place commas and periods inside closing quotation marks and colons and semicolons outside. For UI labels, commands, code, exact strings, and copied source text, preserve literal punctuation and prefer backticks over quotation marks.
- Use straight quotes and ASCII-safe punctuation in any Markdown a reader may copy into commands, code, config, or trackers. Curly quotation marks and apostrophes are acceptable only in prose that is never copied as a literal.

## [8][CODE_SAFE_MARKDOWN]

- Wrap commands, flags, paths, environment variables, package IDs, symbols, literal values, and placeholders in backticks.
- Name a placeholder by route, such as `<scenario-glob>` or `<package-name>`, and use one only when the reader must substitute a value.
- Omit the shell prompt from a copyable command unless the prompt is the subject.
- Put surrounding sentence punctuation outside a code span unless the punctuation is part of the literal value.
- When inline code contains a backtick, use a longer matching backtick delimiter rather than a backslash escape.

## [9][LINKS]

- Use link text that describes the destination, and avoid bare URLs unless the URL is the example value.
- Prefer canonical repo docs, generated contracts, manifests, or maintained source routes, and avoid deep links when a stable top-level reference carries the same truth.
- Route generated, mirrored, or version-sensitive target freshness and generated mirror behavior to [proof.md](proof.md).

## [10][EXAMPLES]

Examples must prove shape or prevent misuse. Place local examples beside the rule they clarify per [information-structure.md](information-structure.md). A late example section is valid only when it is a named reader-route example set that demonstrates how several rules interact. This guide controls example wording; the form standard controls placement.

Use examples under these limits:
- Prefer one positive shape and one rejected shape when the distinction matters.
- Keep an example's wording consistent with the terminology it teaches.
- When a block could be copied, run, or mistaken for current policy, mark its reuse risk.
- When the contrast is only short prose or values, use the compact contrast record from the formatting standard instead of separate fences.

Accepted: Attach the unit to its value.
Rejected: It is probably best to try to avoid not stating the unit when you can.
Reason: The accepted sentence names the action directly; the rejected sentence hedges, negates, and hides the unit rule.

## [11][ACCESSIBILITY]

- Carry meaning through text, not through color, position, shape, pointer movement, screenshots, or sound alone.
- Provide text equivalents for visual, audio, or screenshot-only information, and write alt text that states what the image proves.
- Write text equivalents for diagrams, alerts, glyph-heavy graphics, progress bars, and monospace structures when the meaning is not already recoverable from adjacent fields. State the relation, state, or proof that the visual encodes, not its appearance.
- When keyboard, pointer, touch, voice, or automation can perform the action, use input-neutral UI verbs. When the UI label is the action target, describe it exactly.
- Prefer conventional grammar and punctuation for machine translation and avoid idioms and culture-specific figurative wording.

## [12][FINAL_PROOFING_PASS]

Review craft in 4 passes before publication:
1. Composition: scope, controlling idea, paragraph order, and section endings.
2. Mechanics: grammar, punctuation, capitalization, numbers, parallelism, code-safe Markdown, semicolon-ended bullets, prose slashes, digit-hyphen-digit ranges, and colon-lead/list adjacency.
3. Terminology: names, commands, paths, links, and examples use exact source wording.
4. Accessibility wording: sensory cues, UI labels, and text equivalents are direct and input-neutral.

## [13][BOUNDARIES]

- [information-structure.md](information-structure.md) carries container form, example placement, and the code-block intent-label vocabulary.
- [proof.md](proof.md) carries evidence strength and when a hedge is a load-bearing uncertainty marker rather than filler.
- [formatting.md](formatting.md) carries status and invocation markers, table styling, and whitespace; this standard carries the words.
- [README.md](README.md) carries document-type routing and cross-standard links.

## [14][VALIDATION]

Use this verification checklist by group:

[NAMES_PROSE]:
- [ ] Repo-local and maintained source names are preserved exactly.
- [ ] One concept has one term.
- [ ] Paragraphs carry one controlling idea and close on the intended route, action, proof, or boundary.
- [ ] Sentences are direct, concrete, and free of needless words.
- [ ] Conditions appear before the actions they control.
- [ ] Root-file audit prose states the score basis directly and avoids praise, blame, and session-process narration.
- [ ] Root-file audit prose uses the 4 shared axes from [README.md](README.md) and does not invent a local grading vocabulary.

[MECHANICS_SOURCES]:
- [ ] Commands, paths, flags, symbols, and placeholders use backticks.
- [ ] Punctuation around code spans preserves literal copyability.
- [ ] Bullet items do not end with semicolons.
- [ ] Prose alternatives use words instead of slashes.
- [ ] Prose numeric ranges use en dashes unless they are copyable literals.
- [ ] A colon lead that introduces a list has no blank gap before the list.
- [ ] Examples are necessary and placed beside the rule they clarify.
- [ ] Links use meaningful text and point to canonical sources.
- [ ] Accessibility does not depend on sensory-only cues.
- [ ] No transient task language, obsolete command, or unsupported product claim remains.
