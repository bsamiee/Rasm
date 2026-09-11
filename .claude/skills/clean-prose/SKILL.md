---
name: clean-prose
description: "Use when writing or rewriting markdown, comments, messages, identifiers, or file names, or reviewing for coined terms, filler, or restatement."
---

# [CLEAN_PROSE]

Governs every English text in a project (markdown, comments, messages, identifiers, file names). Corrections are structural, silent removal before replacement, a fix removes the category of problem: a swap of one marker for another moves it, a rewrite that fails a rule is a regression. Rewrites keep every fact, add no cause, frequency, or certainty its source did not state, and end with fewer words and bytes unless a fact was wrong or missing.

[REFERENCES]:
- [01]-[WORD_MAP](references/word-map.md): Words to delete or replace, with replacement for each
- [02]-[REWRITES](references/rewrites.md): Before and after pairs for structural moves, with rewrites that look right and fail

[SCRIPTS]:
- [01]-[PROSE](scripts/prose.py): `uv run --script scripts/prose.py check <path>...` prints findings, `fix` writes fixable, NOT authoritative or replacement for direct read

## [01]-[TERMINOLOGY]

Each term comes from current documentation of its language, tool, or field at newest standard context supports, and passes each test:
- Documentation uses the term with that meaning
- Term names what the thing is
- Verbs name the operation

Words that fail take the current term for what they name, and a real term of a field (SIMD lane) stays:

| [INDEX] | [COINED]                                                 | [REAL]                                                  |
| :-----: | :------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `ship`, `shipped`, `shipping`                            | `publish`, `include`, `copy`, `release`                 |
|  [02]   | `seat`, `seated`, `roster`                               | `register`, `place`, `list`                             |
|  [03]   | `admit`, `admission`, `blessed`, `mint`, `minted`        | `accept`, `validate`, `approved`, `create`, `issue`     |
|  [04]   | `strata`, `stratum`, `substrate`, `fabric`, `backbone`   | `layer`, `base`, `infrastructure`                       |
|  [05]   | `lane`, `realm`, `landscape`, `surface`                  | `pipeline`, `packaging project`, `area`, `feature set`  |
|  [06]   | `capsule`, `island`, `box` (as isolation)                | `package`, `module`, `sandbox`, `host`                  |
|  [07]   | `rung`, `ladder`                                         | `arm`, `case`, `chain of guard clauses`                 |
|  [08]   | `charter`, `doctrine`, `law`, `ruling`, `canon`          | `rule`, `policy`, `decision`, `convention`              |
|  [09]   | `anchor` (as a metaphor)                                 | `pin`, `root`, `reference`, `positive rule`             |
|  [10]   | `payload` (outside a message, request, or call body)     | `file`, `asset`, `content`, `package path`, `arguments` |
|  [11]   | `vocabulary table`, `closed family`                      | `lookup table`, `const object`, `sealed hierarchy`      |
|  [12]   | `custody`, `custodian`, `posture`, `guardrail`, `beacon` | `storage`, `holder`, `configuration`, `check`, `signal` |
|  [13]   | `verb` (for a CLI action), `twin`, `sibling variant`     | `subcommand`, `overload`, `suffix variant`              |
|  [14]   | `rides`, `carries`, `travels`, `lives` (for a value)     | `holds`, `stores`, `sets`, `belongs to`, `goes in`      |
|  [15]   | `probe` (as a test double), `fan-out degree`             | `spy`, `degree of parallelism`                          |
|  [16]   | `phantom`, `ghost`, `census`, `sweep` (check)            | `missing`, `undefined`, `coverage check`, `assertion`   |
|  [17]   | `materialize`, `pristine`, `in-flight` (change)          | `write`, `empty`, `uncommitted`                         |
|  [18]   | `interior`, `flips`, `mirrors` (as matches)              | `inside`, `disabled`, `matches`                         |
|  [19]   | `bare` (name), `edge` (as a boundary), `bespoke`         | `unqualified`, `boundary`, `custom`                     |
|  [20]   | `weave`, `unlock`, `surfaces` (verb)                     | `insert`, `enable`, `throws`                            |
|  [21]   | `topology` (declared resources), `estate` (program)      | `resources`, `program`                                  |
|  [22]   | `intent` (an input value)                                | `value`                                                 |
|  [23]   | `blanket` (catch-all), `peers on`, `floors` (version)    | `discard`, `declares a peer on`, `requires or later`    |
|  [24]   | `host-free`, `feature parity`                            | `without a browser`, `covers the same cases`            |
|  [25]   | `repair` (missing asset), `inert`                        | `add`, `does nothing`                                   |
|  [26]   | `harmless` (epeated call), `wiring`                      | `changes nothing`, `composing`                          |
|  [27]   | `building blocks` (after package name)                   | `primitives`                                            |
|  [28]   | `pool` (concurrent jobs), `migration shim`               | `jobs`, `degree of parallelism`, `the old path`         |
|  [29]   | `settled` (fact), `hard-won`, `land` (hange)             | `proven`, `proven`, `commit`, `write`                   |
|  [30]   | `pluggable`, `publication-quality`                       | Delete                                                  |
|  [31]   | `toolkit`, `suite` (after a package name)                | Delete                                                  |

Identifiers and every other name in code, build, and rule files say what the thing is in their language's vocabulary, renames go through language tooling to update every reference, test, and file name. Prose writes code names in backticks with exact spelling, shows a tool use as command itself (`ruff check`), and keeps tool or product names used as words plain. Names and text another system resolves or emits stay exact, reports name each coupling. Examples, snippets, and comments in guidance use placeholder names (`<tool>`, `<dir>`, `Item`, `Command`) and neutral values, domain names appear where a fact belongs to that domain. Repository, product, and organization names belong in identifiers an ecosystem requires, package descriptions, CLI help text, opening sentence of their README, and in prose as a contrast with another product.

## [02]-[REMOVALS]

Every word earns its place by deletion: remove it, reread the sentence, keep the remainder when nothing of value is lost. Words that narrow what a sentence permits (`only`, `alone`, `in place of`, `nothing else`) stay. Articles, conjunctions, qualifiers, and word map rows are common cases, clauses that followed a deleted connective continue after a comma with their verb. Additive markers tack a fact onto the previous sentence: fact joins list it belongs to or stands as its own statement, marker goes. `because` bolts a reason onto a claim through indirection, the same clause after a comma or reversed in front of instruction is same tail: clause goes whole, a reason the subject makes obvious or a reader does not act on is no fact, a reason that names a tool behavior or criterion becomes the condition before instruction or fact in its own sentence.

Hedges and frequency words with a measured or real uncertainty stay as "can" or as condition. Real values stay in their original spelling, counts of items a reader can see go. Prose names a package, tool, or API at newest standard without a version, version stays in its manifest or where a fact holds for one version alone.

Facts one run produced are observations, and prose keeps the rule they showed:

| [INDEX] | [OBSERVATION]                                           | [KEEP]                                               |
| :-----: | :------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | Duration, size, or count an output printed              | Condition or shape that decides case                 |
|  [02]   | Line number in a generated or external file             | Literal the code spells, read through a search       |
|  [03]   | Release version, issue number, or defect of one release | Behavior, and retirement condition when one is known |
|  [04]   | Path, key, or name of one proof, probe, or session      | Placeholder form (`<proof>`, `<session>`)            |

Values a declaration, manifest, or option states (`timeout: 600000`) are facts and stay with their source named. Size, byte, duration, and count limits stay when a named tool enforces them, every other threshold goes. Examples stay when they are a command, a sequence, or a case of the file's subject, examples with no source on disk or in a tool's documentation go.

## [03]-[SENTENCES]

Each sentence states one instruction or fact in active voice and simple present or past, with condition and reason a reader needs to act on it:
- Joined clauses (chained by `and`, `:`, `;`, `—`, or repeated article) hold one concept in parts, rewrite states it as one clear part
- `plus` names a literal math operation alone, elsewhere the word goes with no replacement
- Second fact opens a new sentence, facts that are parallel cases become a list or a table
- Runs of short sentences on one fact are fragments, one sentence states that fact with its condition and reason
- Clauses that complete one fact continue after a comma or "and"
- Lines a reader must act on are instructions, lines that record what a system does are statements
- Statements and entries open with a subject noun, plural when generic, or with an instruction verb, a condition precedes the verb with a comma
- Noun chains stop at three words, a longer chain is rebuilt whole
- Instructions state their required form, a sentence opening with `never`, `not`, or `no` states the required form, a restriction that is fact stays
- Forbidden forms go in the section's anti-pattern table beside the correct form, and stay as facts when section has none
- Warnings precede their step and state its command or condition, then its risk
- Statements name their actor as subject, passive voice stays for an unknown actor
- Verbs name actions, nominalizations and phrasal verbs take their verb from word map
- Modals are must for a requirement, can for a possibility, will for what comes next
- `should` becomes must or goes as a suggestion, `may`, `might`, `could`, and `would` become can or condition, `may have` stays
- One word names one concept for the whole file
- Contractions expand, present perfect becomes simple past or present, spelling is American
- Em dashes appear as `value — description` in a list item or table cell alone, a semicolon between clauses marks a joined sentence to rebuild as one
- Parentheses hold a phrase, a sentence inside them folds into its sentence or goes
- Tail clauses (participle or relative clause after a comma) fold into their sentence or go
- `such as` becomes a parenthetical or list, `whose` becomes `with` or `that`
- `e.g.` and `i.e.` become their words, `etc.` names items, `and/or` names one or both
- Demonstratives (`this`, `these`, `that`, `those`) as a determiner or subject go, or the noun repeats
- Pronouns and possessives (`it`, `its`, `they`, `their`, `them`) for a subject the sentence, heading, or file makes evident go, or the noun repeats
- Cross-references (`above`, `below`, `see`, `[NN]`, `this file`, a later passage pointing back) go, fact sits where it is needed
- Questions outside a quoted message become fact or purpose
- Sentences that walk steps a command, target, or script runs are paraphrased code, sentence names it with its purpose

`the` points at one referent its sentence, heading, or previous sentence named, and a sentence carries at most one. Sentences that need more restate or chain, and a swap of `the` for `a`, `each`, or `this` corrects nothing:
- Sentences and entries open with no article
- Nouns before an identifier take no article (target `lint`)
- `the <noun> of the <noun>` becomes a compound noun or a possessive (`store keys`, `file's diff`)
- Appositions listing an output or a file's contents drop

## [04]-[DOCUMENTS]

Sections follow work or dependency order under `## [NN]-[NOUN]` headings with no parenthetical, and open with the sentence a reader needs before their list or table:
- Each fact appears once, in its owning file under the heading that names it
- Sentences that restate what their file, heading, code, or previous sentence supplies at the same scope go
- Paths and names that locate what a sentence acts on stay
- Phrasings that differ in subject, scope, or value are two facts or one wrong fact, the phrasing its owning source proves stays
- Facts a deleted sentence alone held move to the sentence that holds their topic
- Heading, lead-in, and previous sentence supply subject, its noun repeats where a fact otherwise attaches to another subject
- Opening sentences state their scope as one category, every other sentence about the file, section, or skill goes
- Lines under a subject state facts about it without naming it: `the repository` in its README, `this file`, and a possessive for it go
- Pointers to another file or skill are one line, `Use <name> for <purpose>`, a sentence that `<name>` owns a topic takes the pointer form
- Pointers name the file and its purpose alone, a section, heading, row, line, or category inside it drifts
- Pointer purposes name one category a reader recognizes a task by, a list stays where no term covers its members
- `Use <name>` excludes every other source, tail naming one (`in place of memory`) restates the instruction
- Facts sit where readers need them, a link is the location of a thing readers open
- External URLs outside a package page, download, or tool document go, the fact they cited is stated
- Prose states purpose, contents stay in the thing: a file, directory, section, diagram node, reference, or comment line names what it is for
- Tree and index lines with a list as their fact keep the list
- Guidance that enumerates quirks or copies one tool's configuration rows drifts, one rule for the category replaces it
- Prose about code names the command, identifier, or file and states its purpose, its steps stay in code
- Steps that name a command hold one reading that decides what follows
- Paragraphs hold one topic, parallel cases sit in one list, or in one sentence when each case is a phrase
- Entries (list items, steps, listing lines, table rows, tree comments) hold one fact or purpose in one line under 150 columns
- Entries over the width hold a chained or restated sentence, the content is rebuilt whole
- Entries open with a capital letter or an identifier and end without a period
- List items share one grammatical form and follow their lead-in colon without a blank line
- Items under an uppercase label hold the sentences the label needs
- Labels and sentence position give emphasis, `**`, emoji, and uppercase words outside code and `[LABELS]` go

Snippets show one rule or operation of the package their file owns, the owned type stands alone:
- Construct is complete with its rule visible, every member serves the rule
- Every local carries its declared type, prose beside the snippet names each undeclared placeholder member
- One snippet shows one shape, a construct its source shows in more shapes keeps each shape
- Names keep one shape within a file

Tables hold values a reader decides by, the sentence that explains them stays in section text:
- Headers are one or two words, cells hold values, identifiers, or short phrases without an article, period, or semicolon
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
- Each sentence stays whole on its line within language line length, intent that needs two lines goes
- Consecutive full-line comments merge into one, a comment that repeats its code goes
- Inline comments stay and get the same removals
- Test case comments name case's shape as a noun phrase, then the fact that decides it
- Comments naming a value's ported source go once the source leaves the repository or names a file repository does not hold
- Section dividers, structured doc comments with one element per line, commented-out configuration templates, and tool directives keep their form
- Doc comment summaries are one sentence that states what the member returns or does without restating its name, remarks keep one fact per sentence
- Use `dotnet-document`, `python-document`, and `typescript-document` for a doc comment's tags, sections, period, and presence
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
9. Compare the result with its fact list, `git log -p`
10. Report bytes before and after, renames, coined terms removed, couplings left in place, facts added, corrected, or kept in longer form

New text follows the same rules from its first draft. Reviews report one row per finding: line, rule, offending text, rewrite.
