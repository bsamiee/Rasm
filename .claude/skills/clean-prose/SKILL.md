---
name: clean-prose
description: "Use when writing, rewriting, or reviewing any English text in a project, names and comments included"
---

# [CLEAN_PROSE]

Applies to project English, including Markdown, comments, messages, identifiers, and file names. Corrections remove whole problem categories through deletion before replacement. Rewrites preserve facts and stated uncertainty without adding unstated causes or frequencies. Rewrites reduce words and bytes unless a fact is wrong or missing.

[REFERENCES]:
- [01]-[WORD_MAP](references/word-map.md): Word choice
- [02]-[REWRITES](references/rewrites.md): Rewrite examples
- [03]-[ACTIONS](references/actions.md): Reader action verbs

[SCRIPTS]:
- [01]-[PROSE](scripts/prose.py): Markdown and source comment corrections

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
|  [15]   | `pool` (concurrent jobs), `fan-out degree`                        | `degree of parallelism`                                          |
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

Names in code, build, and rule files come from their subject:
1. Names state what the code is, in place of what it does to data
2. Subjects belong to a field (a language, a domain), and names and divider labels take that field's own terms
3. Categories name the field spanning their present and anticipated packages, packages sharing only a mechanism belong to separate fields
4. Packages hold one concern of their field whole, named by the term its practitioners use, and modules name the concepts inside it

Names outside their declaration take these forms:
- Renames use language tooling and update every reference, test, file, and folder
- Prose writes code names in backticks with exact spelling, a tool use as its command (`ruff check`), and tool or product names used as words plain
- Names, control labels, and text another system resolves or emits stay exact, reports name each coupling
- Guidance examples, snippets, and comments use placeholder names (`<tool>`, `<dir>`, `Item`) and neutral values
- Repository, product, and organization names belong in required identifiers, package descriptions, CLI help, README openings, and product contrasts

## [02]-[REMOVALS]

Words stay only when rereading the sentence without them loses something of value:
- Words that narrow what a sentence permits (`only`, `alone`, `in place of`, `nothing else`) stay
- Articles, conjunctions, qualifiers, and word map rows are common deletions
- Connective deletion keeps each clause's verb and joins clauses with a comma
- Facts after additive markers join their list or stand alone
- Obvious reasons and reasons readers cannot act on go with their connective
- Tool behavior and decision criteria precede instructions as conditions or stand as facts
- Measured or real uncertainty stays as `can` or an explicit condition
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

Examples are commands, sequences, or cases of a file's subject with a source on disk or in tool documentation.

## [03]-[SENTENCES]

Sentences state one instruction or fact in active voice and simple present or past, with the condition and reason a reader acts on:
- Fragments and chained clauses on one concept become one statement with its condition and reason
- Entries state one fact or purpose; list items, steps, and table rows use one line under 150 columns
- Entries open with a capital letter, an identifier, or a tool name and end without a period
- Second sentences in an entry hold only a fact a reader acts on
- Rebuild long passages and oversized entries whole to remove overlap and framing and preserve instructions
- `plus` names a math operation alone, elsewhere the word goes with no replacement
- Clauses that complete one fact continue after a comma or "and"
- Lines a reader must act on are instructions, lines that record what a system does are statements
- Statements and entries open with subject nouns, plural when generic, or instruction verbs; conditions precede verbs with a comma
- Noun chains stop at 3 words, a longer chain is rebuilt whole
- Instructions state required forms positively; restrictions describing behavior stay as facts
- Forbidden forms go in the section's anti-pattern table beside the correct form, or stay as facts when the section has none
- Warnings precede their step and state its command or condition, then its risk
- Statements name their actor as subject, passive voice stays for an unknown actor
- Modals are must for a requirement, can for a possibility, will for what comes next
- `should` becomes must or goes as a suggestion, `may`, `might`, `could`, and `would` become can or condition, `may have` stays
- One word names one concept across a file and every file of its area, identifiers included
- Contractions expand, present perfect becomes simple past or present, spelling is American
- Em dashes appear as `value — description` in a list item or table cell alone
- Parentheses hold a phrase, a sentence inside them folds into its sentence or goes
- Tail clauses (participle or relative clause after a comma) fold into their sentence or go
- `such as` becomes a parenthetical or list, `whose` becomes `with` or `that`
- `e.g.` and `i.e.` become their words, `etc.` names items, `and/or` names one or both
- Demonstratives (`this`, `these`, `that`, `those`) as a determiner or subject go, or the noun repeats
- Pronouns and possessives for an evident subject repeat the noun or go
- Text omits subjects supplied by its field, heading, or file
- Subject names stay where a text spans more than one subject or a fact otherwise attaches to another subject
- Facts sit where readers need them without passage cross-references
- Questions outside a quoted message become fact or purpose

`the` appears at most once per sentence, referring to a noun named in the same sentence, heading, or preceding sentence. Extra articles require rewriting; swaps with `a`, `each`, or `this` preserve the defect:
- Sentence and entry text opens without articles, including text after field keys or markup
- Nouns before an identifier take no article (target `lint`)
- `the <noun> of the <noun>` becomes a compound noun or a possessive (`store keys`, `file's diff`)
- Appositions listing an output or a file's contents drop

## [04]-[DOCUMENTS]

Sections follow work or dependency order under `## [NN]-[NOUN]` headings without parentheses and introduce their list or table:
- Each fact appears once in its owning file and section, including facts shared by skills and references
- Sentences repeating context from their file, heading, code, or preceding text go
- Paths and names that locate what a sentence acts on stay
- Subject, scope, or value differences are distinct facts; conflicts retain wording from the owning source
- Facts unique to a deleted sentence join a rewrite of the topic's owning line
- Openings name one scope category; other descriptions of a file, section, or skill go
- Reference topics stay in reference files; pointers use one line, `Use <name> for <purpose>`
- Pointers name files and purposes; internal headings, rows, lines, and categories drift
- Pointer purposes name one category a reader recognizes a task by, a list stays where no term covers its members
- `Use <name>` excludes every other source, tail naming one (`in place of memory`) restates the instruction
- Links locate what readers open
- External URLs outside a package page, download, or tool document go, the fact they cited is stated
- Tree and index lines with a list as their fact keep the list
- Guidance states category rules; code members, quirks, and tool configuration rows stay with their owner
- Labels for commands, identifiers, paths, sections, diagram nodes, and references state purpose; contents and steps stay with their owner
- Steps that name a command hold one reading that decides what follows
- Paragraphs keep one topic on one line without a column limit; parallel cases use a list, table, or one sentence of phrases
- List items share one grammatical form and follow their lead-in colon without a blank line
- Items under an uppercase label hold the sentences the label needs
- Files open with one `# [TOKEN]`; `##` numbers start at `[01]` in file order and `###` restart under each `##`
- Labels close with `:` before their list or table, entry leaders are `[NN]-[TOKEN]:` chains
- Link cards number from `[01]` among sibling entries, the file stem as their token
- One blank line follows a heading or surrounds a table, at most one separates other blocks, none ends a line or the file
- Labels and sentence position give emphasis, `**` and `_` markers, emoji, and uppercase words outside code and `[LABELS]` go

Snippets show one package rule or operation, with its owning type alone:
- Constructs are complete with their rule visible, every member serves the rule
- Every local declares its type, prose beside the snippet names each undeclared placeholder member
- Snippets show one shape each and retain every supported form
- Names keep one shape within a file
- Code snippets follow their language's native formatter and project settings, output copied from a tool keeps its spacing in a `text` fence

Tables hold decision values; explanations stay in section text:
- Headers use at most 2 words; cells hold values, identifiers, or short phrases without articles, periods, or semicolons
- Cells open with a capital letter, except backticked identifiers and literal words
- Constant columns move to the lead-in
- Rows exceeding 150 columns lose narration or redundant columns

Tables use bracket headers and an `[INDEX]` column:

```markdown
| [INDEX] | [FLAG]    | [EFFECT]                        |
| :-----: | :-------- | :------------------------------ |
|  [01]   | `--force` | Deletes rows absent from source |
```

## [05]-[COMMENTS]

Code files hold section dividers and tool directives alone, laid out by `CLAUDE.md` file organization. Comments in config files and Markdown state intent or constraint their content cannot show:
- Sentences stay whole within language line length; intent needing another line goes
- Consecutive full-line comments merge into one, a comment that repeats its content goes
- Inline comments stay and get the same removals
- Comments naming a value's ported source go once the source leaves the repository or names a file the repository does not hold
- Structured doc comments with one element per line, commented-out configuration templates, and tool directives keep their form
- Doc summaries state returns or behavior without repeating member names; remarks keep one fact per sentence
- Messages and output fields derive event details and known causes from values, then give agents one available action after a comma
- Commit subjects open `<type>(<project>): ` with an imperative subject, `!` before the colon marks a breaking change
- Commit and pull request bodies state past facts
- Commit types mark behavior: `feat` adds, `fix` corrects, `refactor` reworks, and `perf` improves performance
- Changes with no product behavior use `build`, `ci`, `docs`, `test`, `style`, or `chore`
- Commit scopes use the changed project's Nx name; changes across projects omit scope
- Comments warning of a wrong call result go; calls use parameters or forms that return the required result

## [06]-[PROCESS]

Rewrite of an existing file:
1. Read the whole file, linked files, and every `references/` file before editing
2. List facts and owners and mark restatements
3. When no finding remains, report compliance and stop
4. Rewrite sentences in owning sections using word-map meanings and reader action verbs; add unlisted words to their category
5. Report bytes before and after, renames, coined terms removed, couplings left in place, facts added, corrected, or kept in longer form

Guidance and message edits rewrite or delete owning lines and widen categories for new cases. Appended restatements are regressions. New text uses direct positive statements without framing or validation language.

`scripts/prose.py` fixes format Markdown blocks and tables, normalize dividers, delete empty sections and lone subsections, and set one blank line beside each divider except gaps the file's formatter owns. Reports cover code comments outside dividers and directives, sections out of `CLAUDE.md` table order, and wording.

Run `scripts/prose.py`: `check` reports findings at their lines on disk, `fix` writes fixes and reports remaining findings:

```bash
python "${CLAUDE_SKILL_DIR}/scripts/prose.py" check <path>...
python "${CLAUDE_SKILL_DIR}/scripts/prose.py" fix <path>...
```
