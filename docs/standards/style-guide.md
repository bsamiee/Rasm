# [STYLE_GUIDE]

One term names one concept, one owner decides each wording question, and every sentence carries its law in the fewest mechanically sound words.

## [01]-[USE_WHEN]

Apply when writing or reviewing prose, headings, captions, notes, examples, comments, commands, paths, symbols, placeholders, terminology, and accessibility-sensitive wording in any durable artifact — docs, standards, specs, skills, prompts, templates, and code fences.

## [02]-[WORDING_PRECEDENCE]

Use the first owner that decides a naming or wording question:
1. Current repository material, manifests, generated contracts, API fields, and actual product or package names.
2. Active document-type standard for the document being written.
3. Maintained product names, UI labels, APIs, commands, and support status.
4. This guide for local prose and notation rules.
5. Established editorial style for unresolved general mechanics.

When owners conflict, choose the repo-local term only when a current route or contract proves it. Never alternate terms to avoid the conflict.

## [03]-[SENTENCES]

Use a front-and-close paragraph shape: sentence one states the rule, claim, scope, outcome, or transition, and the last closes on the consequence, boundary, confirmation, or route the reader retains.

[KEEP_OR_CONVERT]:
- Keep a standalone sentence when it leads, transitions, captions, closes, states a consequence, names a route boundary, or marks a gap.
- Convert a sentence to `Label: value` only when the value is scanned, quoted, or updated independently.
- Convert local detail to a note, alert, or row-owned record only when its scope is local to one row, claim, risk, or artifact.
- Delete or fold a sentence that only repeats emphasis.

[MECHANICS]:
- Coordinating conjunctions join equal-rank ideas: `for`, `and`, `nor`, `but`, `or`, `yet`, and `so`.
- Use a comma before a coordinating conjunction joining two independent clauses.
- Omit the comma for imperative clauses sharing an implied subject unless contrast, length, or ambiguity requires it.
- Omit the comma before a coordinating conjunction that joins only a compound predicate.
- Use a semicolon before and a comma after a conjunctive adverb joining two independent clauses: `however`, `therefore`, `otherwise`.
- Do not join independent clauses with a comma alone.
- Use a semicolon only for closely related independent clauses or complex inline lists with internal commas.
- Use `that` for restrictive clauses and `which` for nonrestrictive clauses.
- Use sentence-initial conjunctions, split infinitives, and terminal prepositions when they produce the clearest technical sentence.
- Construct both arms of a correlative pair in parallel: `both ... and`, `either ... or`, `not only ... but also`.
- State conditions before the actions they control; keep modifiers next to the terms they govern; keep parallel items parallel in grammar and scope.

## [04]-[TERMINOLOGY]

Use one term for one concept inside a bounded context. Names are load-bearing: product names, package names, API members, commands, file paths, UI labels, and config keys match the actual surface.

- Preserve verified product, package, API, command, file, and UI names exactly.
- Define a domain term before relying on it unless the target reader already owns that domain; settled vocabulary arrives settled and is never re-taught.
- Introduce an acronym once per document unless the acronym is more recognizable than its expansion.
- Use singular `they` when gender is unknown, irrelevant, or intentionally unspecified.
- Do not invent reader-facing names for internal concepts; do not name stale commands, removed tools, transient task labels, or removed skills.

## [05]-[PUNCTUATION]

[LOCAL_MECHANICS]:
- Use U.S. English spelling unless a product or proper name differs.
- Form singular possessives with `'s`; use the serial comma; use one space after terminal punctuation.
- Use ASCII hyphen-minus in commands, flags, paths, slugs, identifiers, code, config, tracker literals, and copyable text.

[COLONS_DASHES]:
- Use a colon after a complete sentence that introduces a list, table, example, consequence, or explanation.
- Capitalize the first word after a colon only when the following material is a complete sentence.
- Use parentheses for nonessential clarification; promote required conditions into the main sentence.
- If a parenthetical is a full sentence, put the period inside the closing parenthesis; otherwise outside.
- Use spaced em dashes only for prose interruptions, spelled as the character — ASCII `--` or `---` never stands in for it in prose; en dashes carry inclusive ranges and name-based compounds outside copyable text.
- Hyphenate compound modifiers before nouns when needed for clarity; omit hyphens after `-ly` adverbs; use a suspended hyphen for shared compounds such as `short- and long-term`.
- Prefer sentence splits or vertical lists over semicolon-ended bullets.

[QUOTES_NUMBERS]:
- Use numerals for versions, measurements, commands, flags, identifiers, dates, editions, counts, thresholds, and compared values.
- Spell out isolated nontechnical counts from zero through nine only when the value is not a comparison, threshold, field value, or literal.
- Use backticks for UI labels, commands, code, exact strings, and copied literals.
- Place commas and periods inside closing quotation marks in ordinary prose quotations; preserve literal punctuation for copied text.
- Use straight quotes and ASCII-safe punctuation in copyable Markdown.

## [06]-[CODE_SAFE_MARKDOWN]

- Wrap commands, flags, paths, environment variables, package IDs, symbols, literal values, and placeholders in backticks.
- Name placeholders by route, such as `<scenario-glob>` or `<package-name>`.
- Use language-valid neutral identifiers in reusable code examples: `Shape`, `RefinedShape`, `Variant`, `PRIMARY`, `Field`, `KEY`, `Row`, `ROW_A`, `TABLE`, `SELECTED`, `"<value-a>"`, `"<result-a>"`.
- Do not use repository, host, customer, pricing, deployment, or product nouns in reusable examples unless the documented concept is that domain.
- Omit shell prompts from copyable commands; put sentence punctuation outside a code span unless it is part of the literal.
- When inline code contains a backtick, use a longer matching backtick delimiter rather than a backslash escape.

[PLACEHOLDER_EXAMPLES]:
- Accepted: `Shape`, `RefinedShape`, and `OtherShape`. Near miss: `Customer`, `PremiumCustomer`, and `WebhookPayload`. Reason: accepted names show type structure without importing a business domain.
- Accepted: `Variant`, `PRIMARY`, `SECONDARY`, `"<variant-a>"`. Near miss: `Mode`, `PROD`, `STAGING`, `"premium"`. Reason: accepted names teach enum shape without deployment or pricing semantics.
- Accepted: `SELECTED`, `SELECTED_RESULT`, `"<value-a>"`. Near miss: `active_customer`, `invoice_total`, `"alice"`. Reason: accepted names mark selection and data position without anchoring the sample to an identity or product.

## [07]-[EXAMPLES]

Examples prove shape or prevent misuse. Place them beside the rule they clarify.

- Use one positive example and one near miss only where agents copy the distinction incorrectly without them.
- Keep example wording aligned with the terminology it teaches.
- Mark every example block with its declared fence intent.

## [08]-[ACCESSIBILITY]

- Carry meaning through text, not through color, position, shape, pointer movement, screenshots, or sound alone.
- Provide text equivalents for visual, audio, or screenshot-only information — diagrams carry `accTitle`/`accDescr`, glyph-heavy structures carry adjacent prose stating the relation they encode. A syntax-teaching fence inside a diagram skill's own reference corpus is exempt where the directives corrupt or pollute the taught grammar and adjacent prose already states the relation.
- Use input-neutral UI verbs when keyboard, pointer, touch, voice, or automation can perform the action; preserve exact UI labels when the label is the action target.
- Prefer conventional grammar and punctuation for machine translation.
