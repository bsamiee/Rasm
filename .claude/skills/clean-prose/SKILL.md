---
name: clean-prose
description: "Use when writing or rewriting markdown, comments, messages, identifiers, or file names, or reviewing for coined terms, filler, or restatement."
---

# [CLEAN_PROSE]

Governs every English text in a project (markdown, comments, messages, identifiers, file names). Corrections remove a problem's whole category, deletion before replacement. Swaps of one marker for another and rewrites that fail a rule are regressions. Rewrites keep every fact and add no cause, frequency, or certainty their source did not state. Rewrites end with fewer words and bytes unless a fact was wrong or missing.

[REFERENCES]:
- [01]-[WORD_MAP](references/word-map.md): Words to delete or replace, with replacement for each
- [02]-[REWRITES](references/rewrites.md): Before and after pairs for structural moves, with rewrites that look right and fail

[SCRIPTS]:
- [01]-[PROSE](scripts/prose.py): `python ${CLAUDE_SKILL_DIR}/scripts/prose.py check <path>...` prints findings over named files and unignored files of named folders, `fix` writes the fixable ones

## [01]-[TERMINOLOGY]

Terms come from current documentation of their language, tool, or field at the newest standard the project supports, and pass each test:
- Documentation uses the term with that meaning
- Term names what the thing is
- Verbs name the operation

Words that fail take the current term for what they name, real field terms (SIMD lane) stay:

| [INDEX] | [COINED]                                                          | [REAL]                                                           |
| :-----: | :---------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `ship`, `shipped`, `shipping`                                     | `publish`, `include`, `copy`, `release`                          |
|  [02]   | `seat`, `seated`, `roster`                                        | `register`, `place`, `list`                                      |
|  [03]   | `admit`, `admission`, `blessed`, `mint`, `minted`                 | `accept`, `validate`, `approved`, `create`, `issue`              |
|  [04]   | `strata`, `stratum`, `substrate`, `fabric`, `backbone`            | `layer`, `base`, `infrastructure`                                |
|  [05]   | `lane`, `realm`, `landscape`, `surface`                           | `pipeline`, `packaging project`, `area`, `feature set`           |
|  [06]   | `capsule`, `island`, `box` (as isolation)                         | `package`, `module`, `sandbox`, `host`                           |
|  [07]   | `rung`, `ladder`                                                  | `arm`, `case`, `chain of guard clauses`                          |
|  [08]   | `charter`, `doctrine`, `law`, `ruling`, `canon`                   | `rule`, `policy`, `decision`, `convention`                       |
|  [09]   | `anchor` (as a metaphor)                                          | `pin`, `root`, `reference`, `positive rule`                      |
|  [10]   | `payload` (outside a message, request, or call body)              | `file`, `asset`, `content`, `package path`, `arguments`          |
|  [11]   | `vocabulary table`, `closed family`                               | `lookup table`, `const object`, `sealed hierarchy`               |
|  [12]   | `custody`, `custodian`, `posture`, `guardrail`, `beacon`, `lamp`  | `storage`, `holder`, `configuration`, `check`, `signal`, `light` |
|  [13]   | `verb` (for a CLI action), `twin`, `sibling variant`              | `subcommand`, `overload`, `suffix variant`                       |
|  [14]   | `rides`, `carries`, `travels`, `lives`, `furnished` (for a value) | `holds`, `stores`, `sets`, `supplies`, `belongs to`, `goes in`   |
|  [15]   | `probe`, `probing`, `pool` (concurrent jobs), `fan-out degree`    | `run`, `read`, `scan`, `spy`, `degree of parallelism`            |
|  [16]   | `phantom`, `ghost`, `census`, `sweep` (check)                     | `missing`, `undefined`, `coverage check`, `assertion`            |
|  [17]   | `materialize`, `pristine`, `in-flight` (change)                   | `write`, `empty`, `uncommitted`                                  |
|  [18]   | `interior`, `flips`, `mirrors` (as matches)                       | `inside`, `disabled`, `matches`                                  |
|  [19]   | `bare` (name), `edge` (as a boundary), `bespoke`                  | `unqualified`, `boundary`, `custom`                              |
|  [20]   | `weave`, `unlock`, `surfaces` (verb)                              | `insert`, `enable`, `throws`                                     |
|  [21]   | `topology` (declared resources), `estate` (program)               | `resources`, `program`                                           |
|  [22]   | `intent` (an input value)                                         | `value`                                                          |
|  [23]   | `blanket` (catch-all), `peers on`, `floors` (version)             | `discard`, `declares a peer on`, `requires or later`             |
|  [24]   | `host-free`, `feature parity`                                     | `without a browser`, `covers the same cases`                     |
|  [25]   | `repair` (missing asset), `unrecord`, `unstock`, `inert`          | `add`, `delete`, `remove`, `does nothing`                        |
|  [26]   | `harmless` (repeated call), `wiring`, `wired`                     | `changes nothing`, `composing`, `registered`                     |
|  [27]   | `building blocks` (after package name)                            | `primitives`                                                     |
|  [28]   | `shim`, `migration shim`                                          | `adapter`, `the old path`                                        |
|  [29]   | `land` (change), `settle`                                         | `commit`, `write`, `finish`                                      |
|  [30]   | `pluggable`, `publication-quality`, `settled` (fact)              | Delete                                                           |
|  [31]   | `toolkit`, `suite` (after a package name), `hard-won`             | Delete                                                           |

`nx run rasm:lint:ast-grep -- --filter='^no-coined-identifier-' <path>...` reports coined words in identifiers

Names in code, build, and rule files say what the thing is in their language's vocabulary:
- Renames go through language tooling and update every reference, test, and file name
- Prose writes code names in backticks with exact spelling, a tool use as its command (`ruff check`), and tool or product names used as words plain
- Names and text another system resolves or emits stay exact, reports name each coupling
- Guidance examples, snippets, and comments use placeholder names (`<tool>`, `<dir>`, `Item`) and neutral values outside a domain's own facts
- Repository, product, and organization names belong in required identifiers, package descriptions, CLI help, README openings, and product contrasts

## [02]-[REMOVALS]

Words stay only when rereading the sentence without them loses something of value:
- Words that narrow what a sentence permits (`only`, `alone`, `in place of`, `nothing else`) stay
- Articles, conjunctions, qualifiers, and word map rows are common deletions
- Clauses after a deleted connective continue after a comma with their verb
- Additive markers go, their fact joins the list it belongs to or stands as its own statement
- Reason clauses (`because`, the same reason after a comma or before an instruction) go whole when the subject makes them obvious or no reader acts on them
- Reasons that name a tool behavior or criterion become the condition before the instruction, or a fact in their own sentence
- Hedges and frequency words with a measured or real uncertainty stay as "can" or as a condition
- Real values keep their original spelling, counts of items a reader can see go
- Versions stay in their project file, or in prose where a fact holds for one version alone

Facts one run produced are observations, and prose keeps the rule they showed:

| [INDEX] | [OBSERVATION]                                           | [KEEP]                                               |
| :-----: | :------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | Duration, size, or count an output printed              | Condition or shape that decides case                 |
|  [02]   | Line number in a generated or external file             | Literal the code spells, read through a search       |
|  [03]   | Release version, issue number, or defect of one release | Behavior, and retirement condition when one is known |
|  [04]   | Path, key, or name of one run or session                | Placeholder form (`<run>`, `<session>`)              |

Stated values stay with their source named, every other threshold goes:
- Values a declaration, project file, or option states (`timeout: 600000`)
- Size, byte, duration, and count limits a named tool enforces

Examples stay when they are a command, a sequence, or a case of the file's subject with a source on disk or in a tool's documentation.

## [03]-[SENTENCES]

Sentences state one instruction or fact in active voice and simple present or past, with the condition and reason a reader acts on:
- Joined clauses (chained by `and`, `:`, `;`, `—`, or a repeated article) split one concept into parts, a rewrite states it once
- `plus` names a math operation alone, elsewhere the word goes with no replacement
- Second facts open their own sentence
- Runs of short sentences on one fact are fragments, one sentence states the fact with its condition and reason
- Clauses that complete one fact continue after a comma or "and"
- Lines a reader must act on are instructions, lines that record what a system does are statements
- Statements and entries open with a subject noun, plural when generic, or with an instruction verb, a condition precedes the verb with a comma
- Noun chains stop at 3 words, a longer chain is rebuilt whole
- Instructions opening with `never`, `not`, or `no` state the required form instead, a restriction that is a fact stays
- Forbidden forms go in the section's anti-pattern table beside the correct form, or stay as facts when the section has none
- Warnings precede their step and state its command or condition, then its risk
- Statements name their actor as subject, passive voice stays for an unknown actor
- Verbs name actions, nominalizations and phrasal verbs take their verb from the word map
- Modals are must for a requirement, can for a possibility, will for what comes next
- `should` becomes must or goes as a suggestion, `may`, `might`, `could`, and `would` become can or condition, `may have` stays
- One word names one concept for the whole file
- Contractions expand, present perfect becomes simple past or present, spelling is American
- Em dashes appear as `value — description` in a list item or table cell alone
- Parentheses hold a phrase, a sentence inside them folds into its sentence or goes
- Tail clauses (participle or relative clause after a comma) fold into their sentence or go
- `such as` becomes a parenthetical or list, `whose` becomes `with` or `that`
- `e.g.` and `i.e.` become their words, `etc.` names items, `and/or` names one or both
- Demonstratives (`this`, `these`, `that`, `those`) as a determiner or subject go, or the noun repeats
- Pronouns and possessives (`it`, `its`, `they`, `their`, `them`) for a subject the sentence, heading, or file makes evident go, or the noun repeats
- Cross-references (`above`, `below`, `see`, `[NN]`, `this file`, a later passage pointing back) go, the fact sits where readers need it
- Questions outside a quoted message become fact or purpose

`the` appears at most once per sentence, pointing at one referent its sentence, heading, or previous sentence named. Sentences that need more restate or chain, a swap to `a`, `each`, or `this` corrects nothing:
- Sentences and entries open with no article
- Nouns before an identifier take no article (target `lint`)
- `the <noun> of the <noun>` becomes a compound noun or a possessive (`store keys`, `file's diff`)
- Appositions listing an output or a file's contents drop

## [04]-[DOCUMENTS]

Sections follow work or dependency order under `## [NN]-[NOUN]` headings with no parenthetical, and open with the sentence a reader needs before their list or table:
- Each fact appears once, in its owning file under the heading that names it
- Sentences that restate what their file, heading, code, or previous sentence supplies at the same scope go
- Paths and names that locate what a sentence acts on stay
- Phrasings that differ in subject, scope, or value are separate facts or one wrong fact, the owning source's phrasing stays
- Facts a deleted sentence alone held move to the sentence that holds their topic
- Headings, lead-ins, and previous sentences supply the subject, its noun repeats where a fact otherwise attaches to another subject
- Opening sentences state their scope as one category, every other sentence about the file, section, or skill goes
- Lines under a subject state facts about it without naming it (`the repository` in its README, `this file`, a possessive for it)
- Pointers to another file or skill are one line, `Use <name> for <purpose>`, a sentence that `<name>` owns a topic takes the pointer form
- Pointers name the file and its purpose alone, a section, heading, row, line, or category inside it drifts
- Pointer purposes name one category a reader recognizes a task by, a list stays where no term covers its members
- `Use <name>` excludes every other source, tail naming one (`in place of memory`) restates the instruction
- Facts sit where readers need them, links locate what readers open
- External URLs outside a package page, download, or tool document go, the fact they cited is stated
- Tree and index lines with a list as their fact keep the list
- Guidance that enumerates code members, quirks, or one tool's configuration rows drifts, one rule for the category replaces it
- New cases widen the category of the line holding their rule, an appended clause or item restates that rule
- Prose names a command, identifier, file, directory, section, diagram node, or reference with its purpose, contents and steps stay in the thing
- Steps that name a command hold one reading that decides what follows
- Paragraphs hold one topic on one line, parallel cases sit in one list or table, or in one sentence when each case is a phrase
- Entries (list items, steps, table rows, listing lines, diagram labels, fence comments) hold one fact or purpose in one line under 150 columns
- Entries over the width hold a chained or restated sentence, the content is rebuilt whole
- Entries open with a capital letter or an identifier and end without a period
- List items share one grammatical form and follow their lead-in colon without a blank line
- Items under an uppercase label hold the sentences the label needs
- Files open with one `# [TOKEN]` H1, `##` headings number from `[01]` in file order and `###` restart under each `##`
- Labels close with `:` before their list or table, entry leaders are `[NN]-[TOKEN]:` chains
- Link cards number from `[01]` among sibling entries, the file stem as their token
- One blank line follows a heading or surrounds a table, at most one separates other blocks, none ends a line or the file
- Labels and sentence position give emphasis, `**` and `_` markers, emoji, and uppercase words outside code and `[LABELS]` go

Snippets show one rule or operation of the package their file owns, the owned type stands alone:
- Constructs are complete with their rule visible, every member serves the rule
- Every local declares its type, prose beside the snippet names each undeclared placeholder member
- One snippet shows one shape, a construct its source shows in more shapes keeps each shape
- Names keep one shape within a file
- Code indents by the `.editorconfig` indent size of its language, output copied from a tool keeps its spacing in a `text` fence

Tables hold values a reader decides by, the sentence that explains them stays in section text:
- Headers hold at most 2 words, cells hold values, identifiers, or short phrases without an article, period, or semicolon
- Cells open with a capital letter, except backticked identifiers and literal words
- Rows that list what a file exports, declares, or registers go, section text names the file
- Rows that repeat a step or section sentence go, a step names the table it applies
- Columns with one value down every row go, their lead-in states value
- Rows over the width hold a narrating cell or too many columns

Bracket headers with an [INDEX] column are house style:

```markdown
| [INDEX] | [FLAG]    | [EFFECT]                        |
| :-----: | :-------- | :------------------------------ |
|  [01]   | `--force` | Deletes rows absent from source |
```

## [05]-[COMMENTS]

Comments state intent or constraint code cannot show, in one line and statement with no trailing period:
- Comments open with a capital letter, backticked identifier, or a tool name, and no article
- Each sentence stays whole on its line within language line length, intent that needs a second line goes
- Consecutive full-line comments merge into one, a comment that repeats its code goes
- Inline comments stay and get the same removals
- Test case comments name case's shape as a noun phrase, then the fact that decides it
- Comments naming a value's ported source go once the source leaves the repository or names a file the repository does not hold
- Section dividers, structured doc comments with one element per line, commented-out configuration templates, and tool directives keep their form
- Doc comment summaries are one sentence that states what the member returns or does without restating its name, remarks keep one fact per sentence
- Use `Skill(python-document)` for a doc comment's tags, sections, period, and presence, `/dotnet-document` and `/typescript-document` hold the C# and TypeScript forms
- Messages (log, error, exception, diagnostic) state what happened, its cause when known, then the action, each in one sentence with no period
- Commit subjects are imperative, commit and pull request bodies state past facts
- Comments naming a wrong result a call can return are guards, the call takes the parameter or form returning the right result, or the line goes

## [06]-[PROCESS]

Rewrite of an existing file:
1. Read the whole file and every file it points to
2. Check each fact against disk
3. List every fact once, mark each fact that context, an owning file, or a table restates
4. Choose a file and section for each fact
5. When the list holds no finding, report compliance and stop
6. Rename coined identifiers and files with every reference
7. Replace coined terms and delete filler by the word map, a word outside the map joins the row of its category
8. Rewrite each remaining sentence to state one fact in its section
9. Report bytes before and after, renames, coined terms removed, couplings left in place, facts added, corrected, or kept in longer form

New text follows the same rules from its first draft. Reviews report one row per finding: line, rule, offending text, rewrite.

!`cat ${CLAUDE_SKILL_DIR}/references/word-map.md ${CLAUDE_SKILL_DIR}/references/rewrites.md`
