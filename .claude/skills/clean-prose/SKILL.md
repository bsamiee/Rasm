---
name: clean-prose
description: "Use when writing or rewriting markdown, comments, messages, identifiers, or file names, or reviewing for coined terms, filler, or restatement."
---

# [CLEAN_PROSE]

Governs every English text in a repository, from markdown and comments to messages, identifiers, and file names. Most poorly structured prose is several parts of a sentence explaining one concept, information restated in more than one way, and references to what was already said. State each thing once, in one place, and delete the rest. Corrections are structural, a silent removal before a replacement, and a fix removes the category of problem: a swap of one marker for another moves it, and a rewrite that fails a rule the original passed is a regression. Every rewrite keeps every fact with each sentence's subject, scope, and referents, adds no cause, frequency, or certainty its source did not state, and ends with fewer words and bytes unless a fact was wrong or missing.

- [01]-[WORD_MAP](references/word-map.md): Words to delete or replace, with the replacement for each
- [02]-[REWRITES](references/rewrites.md): Before and after pairs for structural moves, with rewrites that look right and fail

The word map and the pairs are known cases, not the whole set: a problem found in a pass joins them, and its category joins the section that owns it. Guidance that enumerates quirks, probe tables, or row maps drifts, and one rule for the category replaces it.

## [01]-[TERMINOLOGY]

Each term comes from current documentation of its language, tool, or field at the newest standard its context supports, and passes each test:
- Current documentation uses it with that meaning
- It names what the thing is
- Verbs name the operation

Words that fail take the current term for what they name, and a real term of a field (SIMD lane) stays:

| [INDEX] | [COINED]                                           | [REAL]                                         |
| :-----: | :------------------------------------------------- | :--------------------------------------------- |
|  [01]   | ship, shipped, shipping                            | publish, include, copy, release                |
|  [02]   | seat, seated, roster                               | register, place, list                          |
|  [03]   | admit, admission, blessed, mint, minted            | accept, validate, approved, create, issue      |
|  [04]   | strata, stratum, substrate, fabric, backbone       | layer, base, infrastructure                    |
|  [05]   | lane, realm, landscape, surface                    | pipeline, packaging project, area, feature set |
|  [06]   | capsule, island, box (as isolation)                | package, module, sandbox, host                 |
|  [07]   | rung, ladder                                       | arm, case, chain of guard clauses              |
|  [08]   | charter, doctrine, law, ruling, canon              | rule, policy, decision, convention             |
|  [09]   | corpus                                             | codebase, document set, directory              |
|  [10]   | anchor (as a metaphor)                             | pin, root, reference, positive rule            |
|  [11]   | payload (outside a message, request, or call body) | file, asset, content, package path             |
|  [12]   | vocabulary table, closed family                    | lookup table, const object, sealed hierarchy   |
|  [13]   | custody, custodian, posture, guardrail, beacon     | storage, holder, configuration, check, signal  |
|  [14]   | verb (for a CLI action), twin, sibling variant     | subcommand, overload, suffix variant           |
|  [15]   | rides, carries, travels, lives (for a value)       | holds, stores, sets, belongs to, goes in       |
|  [16]   | probe (as a test double), fan-out degree           | spy, degree of parallelism                     |
|  [17]   | phantom, ghost, census, sweep                      | missing, undefined, coverage check, assertion  |
|  [18]   | materialize, egress, pristine, in-flight           | write, exporter, empty, uncommitted            |
|  [19]   | interior, flips, autosave, mirrors (verb)          | inside, disabled, output file, matches         |
|  [20]   | bare (name), edge (as a boundary), bespoke         | unqualified, boundary, custom                  |
|  [21]   | weave, unlock, surfaces (verb)                     | insert, enable, throws                         |
|  [22]   | topology (declared resources), estate (a program)  | resources, program                             |
|  [23]   | payload (call arguments), intent (an input value)  | arguments, value                               |
|  [24]   | blanket (a catch-all), peers on, floors (version)  | discard, declares a peer on, requires or later |
|  [25]   | host-free, feature parity, publication-quality     | without a browser, covers the same cases       |
|  [26]   | tolerate, repair (a missing asset), inert          | register, add, does nothing                    |
|  [27]   | harmless (a repeated call), pluggable, wiring      | changes nothing, delete, composing             |
|  [28]   | toolkit, suite, building blocks (after a package)  | delete, primitives                             |
|  [29]   | pool (concurrent jobs), migration shim             | jobs, degree of parallelism, the old path      |
|  [30]   | settled (a fact), hard-won, land (a change)        | proven, proven, commit, write                  |

Identifiers and every other name in code, build, and rule files say what the thing is in their language's vocabulary, and renames go through language tooling to update every reference, test, and file name. Prose writes code names in backticks with their exact spelling, shows a tool use as the command itself (`ruff check`), and keeps tool or product names used as words plain. Names and text another system resolves or emits stay exact, and reports name each coupling. Examples, snippets, and comments in guidance use placeholder names (`<tool>`, `<dir>`, `Item`, `Command`) and neutral values, and domain names appear where a fact belongs to that domain. Repository, product, and organization names belong in identifiers an ecosystem requires, in package descriptions, in CLI help text, in the opening sentence of their README, and in prose as a contrast with another product.

## [02]-[REMOVALS]

Every word earns its place by deletion: remove it, reread the sentence, and keep the remainder when nothing of value is lost. Words whose deletion widens what a sentence permits (`only`, `alone`, `in place of`, `nothing else`) stay. Articles, conjunctions, qualifiers, and the words listed are the common cases, and clauses that followed a deleted connective continue after the comma with their verb. Additive markers (`also`, `as well`, `too`, `likewise`, `additionally`) tack a fact onto the previous sentence: the fact joins the list it belongs to or stands as its own statement, and the marker goes. `because` bolts a reason onto a claim through indirection, and the same clause after a comma or reversed in front of the instruction is the same tail: the clause goes whole, a reason the subject makes obvious or a reader does not act on is no fact, and a reason that names a tool behavior or a criterion becomes the condition before the instruction or a fact in its own sentence.

| [INDEX] | [CATEGORY]                 | [DELETE]                                                                                                   |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | Connectives                | because, so (as a result), therefore, thus, hence, which is why, consequently, furthermore, moreover, also |
|  [02]   | Fillers                    | simply, just, actually, essentially, basically, really, very, quite, rather, somewhat, easily              |
|  [03]   | Marketing                  | robust, powerful, comprehensive, seamless, elegant, clean (as praise), modern, lightweight, best           |
|  [04]   | Meta phrases               | note that, it is worth noting, it is important to, in other words, as mentioned, in summary                |
|  [05]   | Hedges with no uncertainty | possibly, typically, generally, usually, often, in some cases, in most cases, where appropriate            |
|  [06]   | Enumeration devices        | one, two, three, first, second, several, a number of, various, multiple, counts before a list              |
|  [07]   | Version markers            | since version, as of, in version, upgrade from, legacy, new and current (as a version), migrate            |
|  [08]   | Contrast fillers           | still, already, rather than, when the contrasted form is absent from the sentence                          |

Hedges and frequency words with a measured or real uncertainty stay as "can" or as the condition. Real values stay in their original spelling, and counts of items a reader can see go. Prose names a package, tool, or API at the newest standard without a version, and a version stays in its manifest or where a fact holds for one version alone.

Facts one run produced are observations, a change record holds them, and prose keeps the rule they showed:

| [INDEX] | [OBSERVATION]                                           | [KEEP]                                                   |
| :-----: | :------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | Duration, size, or count an output printed              | Condition or shape that decides the case                 |
|  [02]   | Line number in a generated or external file             | Literal the code spells, read through a search           |
|  [03]   | Release version, issue number, or defect of one release | Behavior, and the retirement condition when one is known |
|  [04]   | Path, key, or name of one proof, probe, or session      | Placeholder form (`<proof>`, `<session>`)                |

Values a declaration, manifest, or option states (`timeout: 600000`) are facts, and they stay with their source named.

## [03]-[SENTENCES]

Each sentence states one instruction or one fact in active voice and simple present or past, with the condition and reason a reader needs to act on it:
- Joined clauses (chained by `and`, `:`, `;`, `—`, or a repeated article) hold one concept in parts, and the rewrite states it as one clear part
- A second fact opens a new sentence, and facts that are parallel cases become a list or a table
- Runs of short sentences on one fact are fragments, and one sentence states that fact with its condition and reason
- Clauses continue after a comma or "and" when they complete that fact
- Lines a reader must act on are instructions, and lines that record what a system does are statements
- Statements and list items open with their subject noun, and instructions open with the verb, condition before command with a comma between them
- Generic singular subjects ("a target that") go plural ("targets that") or become the verb
- Runs of list items with one noun opener restructure around the verb or the category noun
- Noun chains stop at three words, and a longer chain breaks with a preposition (`the timeout value for the connection pool`)
- Instructions state their required form, a sentence opening with `never`, `not`, or `no` states that form, and a restriction that is the fact stays
- Forbidden forms go in the section's anti-pattern table beside the correct form, and stay as facts when the section has none
- Warnings precede their step and state its command or condition, then its risk
- Statements name their actor as subject, and passive voice stays for an unknown actor
- Verbs name actions, and nominalizations and phrasal verbs take their verb from the word map
- Modals are must for a requirement, can for a possibility, and will for what comes next
- `should` becomes must or goes as a suggestion, `may`, `might`, `could`, and `would` become can or the condition, and `may have` stays
- One word names one concept for the whole file
- Contractions expand, present perfect becomes simple past or present, and spelling is American
- Em dashes appear as `value — description` in a list item or table cell alone, and a semicolon between clauses marks a joined sentence
- Parentheses hold a phrase, and a sentence inside them folds into its sentence or goes
- Tail clauses (`, making`, `, ensuring`, `, which means`, `, where`) fold into their sentence or go
- `such as` becomes a parenthetical or a list, `whose` becomes `with` or `that`, and `plus` becomes `with`
- `e.g.` and `i.e.` become their words, `etc.` names the items, and `and/or` names one or both
- Demonstratives (`this`, `these`, `that`, `those`) as a determiner or subject go, or the noun repeats
- Pronouns and possessives (`it`, `its`, `they`, `their`, `them`) for a subject the sentence, heading, or file makes evident go, or the noun repeats
- Cross-references (`above`, `below`, `see`, `[NN]`, `this file`, a later passage pointing back) go, and the fact sits where it is needed
- Questions outside a quoted message become the fact or the purpose
- Sentences that walk the steps a command, target, or script runs are paraphrased code, and the sentence names it with its purpose

`the` points at one referent its sentence, heading, or previous sentence named, and a sentence carries at most one, ideally none. Sentences that need more restate or chain, and a swap of `the` for `a`, `each`, or `this` corrects nothing:
- Generic nouns go plural, and a noun before an identifier takes no article (target `lint`)
- `the <noun> of the <noun>` becomes a compound noun or a possessive (`store keys`, `the file's diff`)
- Appositions listing an output or a file's contents drop

`the` stays on a superlative, an ordinal, and a referent context makes unique (`the repository root`, `the first edit`).

## [04]-[DOCUMENTS]

Sections follow work or dependency order under `## [NN]-[NOUN]` headings with no parenthetical, and open with the sentence a reader needs before their list or table:
- Each fact appears once, in its owning file under the heading that names it
- Sentences that restate what their file, heading, code, or previous sentence supplies go, when that place states the same fact at the same scope
- Paths and names that locate what a sentence acts on stay
- Phrasings that differ in subject, scope, or value are two facts or one wrong fact, and the one the owning source proves stays
- Facts a deleted sentence alone held move to the sentence that holds their topic
- Heading, lead-in, and previous sentence supply the subject, and its noun repeats where a fact otherwise attaches to another subject
- Opening sentences state their scope as one category, and every other sentence about the file, section, or skill goes
- Lines under a subject state facts about it without naming it: `the repository` in its README, `this file`, and a possessive for it go
- Pointers to another file or skill are one line, `Use <name> for <purpose>`, and a sentence that `<name>` owns a topic takes that form
- Pointers name the file and its purpose alone, and a section, heading, row, line, or category inside it drifts
- Facts sit where readers need them, and a link is the location of a thing readers open
- External URLs outside a package page, download, or tool document go, and the fact they cited is stated
- Prose states purpose, and contents stay in the thing: a file, directory, section, diagram node, reference, or comment line names what it is for
- Tree and index lines with a list as their fact keep the list
- Prose about code names the command, identifier, or file and states its purpose, and its steps stay in code
- Steps that name a command hold the one reading that decides what follows
- Paragraphs hold one topic, and parallel cases sit in one list, or in one sentence when each case is a phrase
- Entries (list items, steps, listing lines, table rows, tree comments) hold one fact or purpose in one line under 150 columns
- Entries over the width hold a chained or restated sentence, and the content is rebuilt, never sheared
- Entries open with a capital letter or an identifier and end without a period
- List items share one grammatical form and follow their lead-in colon without a blank line
- Items under an uppercase label hold the sentences that label needs
- Labels and sentence position give emphasis, and `**`, emoji, and uppercase words outside code and `[LABELS]` go

Tables hold values a reader decides by, and the sentence that explains them stays in section text:
- Headers are one or two words, and cells hold values, identifiers, or short phrases without an article, a period, or a semicolon
- Cells open with a capital letter, except backticked identifiers and literal words
- Rows that list what a file exports, declares, or registers go, and section text names the file
- Rows that repeat a step or a section sentence go, and a step names the table it applies
- Columns with one value down every row go, and their lead-in states the value
- Rows over the width hold a narrating cell or too many columns

Bracket headers with an [INDEX] column are the house style:

```markdown
| [INDEX] | [FLAG]    | [EFFECT]                            |
| :-----: | :-------- | :---------------------------------- |
|  [01]   | `--force` | Deletes rows absent from the source |
```

## [05]-[COMMENTS]

Comments state intent or a constraint the code cannot show, in one line and one statement with no trailing period:
- Comments open with a capital letter, a backticked identifier, or a tool name, and no article
- Each sentence stays whole on its line within the language line length, and intent that needs two lines moves to documentation or goes
- Consecutive full-line comments merge into one, and a comment that repeats its code goes
- Inline comments stay and get the same removals
- Test case comments name the case's shape as a noun phrase, then the fact that decides it
- Comments naming a value's ported source go once that source leaves the repository or names a file the repository does not hold
- Section dividers, structured doc comments with one element per line, commented-out configuration templates, and tool directives keep their form
- Doc comment summaries are one sentence that states what the member returns or does, and remarks keep one fact per sentence
- Members keep every `<param>` element or none
- Python docstrings keep the first-line period, and each public module, class, or function keeps its docstring
- Messages (log, error, exception, diagnostic) state what happened, then its cause when known, then the action, each in one sentence with no period
- Commit subjects are imperative, and commit and pull request bodies state past facts

## [06]-[PROCESS]

Rewrite of an existing file:
1. Read the whole file and every file it points to
2. Check each fact against disk
3. List every fact once, and mark each fact that context, an owning file, or a table restates
4. Choose a file and section for each fact
5. When the list holds no finding, report compliance and stop
6. Rename coined identifiers and files with every reference
7. Replace coined terms and delete filler by the word map
8. Rewrite each remaining sentence to state its one fact in its section
9. Compare the result with its fact list and `git log -p`
10. Run the language checkers and tests to zero warnings
11. Report bytes before and after, renames, coined terms removed, couplings left in place, and facts added, corrected, or kept in longer form

New text follows the same rules from its first draft. Reviews report one row per finding: line, rule, offending text, rewrite.
