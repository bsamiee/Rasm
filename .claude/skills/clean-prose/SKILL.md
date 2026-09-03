---
name: clean-prose
description: "Use when writing or rewriting markdown, comments, docstrings, messages, identifiers, or file names, or when reviewing text for coined terms, filler, or duplicated sentences."
---

# [CLEAN_PROSE]

Governs every English text in a repository, from markdown and comments to messages, identifiers, and file names. Every rewrite keeps every fact, adds no cause, frequency, or certainty the source did not state, and ends with fewer bytes unless a fact was wrong or missing.

- [01]-[WORD_MAP](references/word-map.md): Words to delete or replace, with the replacement for each
- [02]-[REWRITES](references/rewrites.md): Before and after pairs for the rules that need one, with the rewrites that look right and fail

## [01]-[TERMINOLOGY]

Each term is the current established term of the language, tool, or field the text belongs to, at the newest standard the context supports, and meets each test:
- The word appears with that meaning in the current documentation of the language, tool, or field
- The word names what the thing is
- The verb names the operation

Every word that fails a test takes the current term for the thing it names, and the real term of a field (SIMD lane) stays:

| [INDEX] | [COINED]                                           | [REAL]                                         |
| :-----: | :------------------------------------------------- | :--------------------------------------------- |
|  [01]   | ship, shipped, shipping                            | publish, include, copy, release                |
|  [02]   | seat, seated, roster                               | register, place, list                          |
|  [03]   | admit, admission, blessed, mint, minted            | accept, validate, approved, create, issue      |
|  [04]   | strata, stratum, substrate, fabric, backbone       | layer, base, infrastructure                    |
|  [05]   | lane, estate, realm, landscape, surface            | pipeline, packaging project, area, feature set |
|  [06]   | capsule, island, box (as isolation)                | package, module, sandbox, host                 |
|  [07]   | rung, ladder                                       | arm, case, chain of guard clauses              |
|  [08]   | charter, doctrine, law, ruling, canon              | rule, policy, decision, convention             |
|  [09]   | corpus                                             | codebase, document set, directory              |
|  [10]   | anchor (as a metaphor)                             | pin, root, reference, positive rule            |
|  [11]   | payload (outside a message, request, or call body) | file, asset, content, package path             |
|  [12]   | vocabulary table, closed family                    | lookup table, const object, sealed hierarchy   |
|  [13]   | custody, custodian, posture, guardrail, beacon     | storage, holder, configuration, check, signal  |
|  [14]   | verb (for a CLI action), twin, sibling variant     | subcommand, overload, suffix variant           |
|  [15]   | rides, carries, travels, lives (for a value)       | holds, stores, sets, belongs to                |
|  [16]   | probe (as a test double), fan-out degree           | spy, degree of parallelism                     |
|  [17]   | phantom, ghost, census, sweep                      | missing, undefined, coverage check, assertion  |
|  [18]   | materialize, egress, pristine, in-flight           | write, exporter, empty, uncommitted            |
|  [19]   | interior, flips, autosave, mirrors (verb)          | inside, disabled, output file, matches         |
|  [20]   | bare (name), edge (as a boundary), bespoke         | unqualified, boundary, custom                  |
|  [21]   | weave, unlock, surfaces (verb)                     | insert, enable, throws                         |

The repository, product, or organization name belongs in identifiers the ecosystem requires, in package descriptions, in CLI help text, and in prose as a contrast with another product.

## [02]-[NAMES]

Identifiers and every other name in code, build, and rule files say what the thing is in the vocabulary of their language. Prose writes code names in backticks with their exact spelling, shows a tool use as the command itself (`ruff check`), and keeps tool or product names used as words plain. Rename through the language tooling to update every reference, test, and file name.

Names and text that another system resolves or emits stay exact, and the report names the coupling.

Examples, snippets, and comments in guidance use placeholder names (`<tool>`, `<dir>`, `Item`, `Command`) and neutral values, and a domain name appears where the fact belongs to that domain.

## [03]-[REMOVALS]

Delete the word and read the sentence again, and the remainder stands when it still states the fact. The clause that followed a connective continues after the comma with its verb, and "plus" or "+" between nouns becomes "with".

| [INDEX] | [CATEGORY]                 | [DELETE]                                                                                           |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | Connectives                | so (as a result), therefore, thus, hence, plus, consequently, as such, furthermore, moreover, also |
|  [02]   | Fillers                    | simply, just, actually, essentially, basically, really, very, quite, rather, somewhat, easily      |
|  [03]   | Marketing                  | robust, powerful, comprehensive, seamless, elegant, clean (as praise), modern, lightweight, best   |
|  [04]   | Meta phrases               | note that, it is worth noting, it is important to, in other words, as mentioned, in summary        |
|  [05]   | Hedges with no uncertainty | possibly, typically, generally, usually, often, in some cases, in most cases, where appropriate    |
|  [06]   | Enumeration devices        | one, two, three, first, second, several, a number of, various, multiple, counts before a list      |
|  [07]   | Version markers            | since version, as of, in version, upgrade from, legacy, new and current (as a version), migrate    |

| [INDEX] | [RESTRUCTURE] | [FORM]                                                                                          |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------- |
|  [01]   | such as       | Parenthetical after the noun, a direct list, or drop the examples, "for example" as last resort |
|  [02]   | whose         | "with", "of", "keyed by", or a relative clause with "that"                                      |
|  [03]   | e.g., i.e.    | for example, that is                                                                            |
|  [04]   | etc.          | Name the items                                                                                  |
|  [05]   | and/or        | One of them, or "X, Y, or both"                                                                 |

Numbers stay as real values, and counts of items the reader can see go. Prose names a package, tool, or API at the newest standard without a version, and a version stays in a package manager manifest or where the fact holds for one version alone. Frequency and time words stay when the source measured them and otherwise become the condition. Hedges with real uncertainty stay as "can" or as the condition under which the statement holds. Sentences and clauses that repeat a fact the file or the owning file states, or state what the heading, the code, or the previous sentence supplies, go, and a fact only they held moves into the sentence that holds its topic.

## [04]-[SENTENCES]

Each sentence states one instruction or one fact in active voice and simple present or past, in the words the reader needs to act on it:
- The instruction or fact carries its condition and the reason the reader needs to apply it to the next case
- Each further fact opens a new sentence, and parallel facts become a list or a table
- Clauses continue after a comma or "and" when they complete the same instruction or fact
- Em dashes appear as `value — description` in a list item or table cell
- Parentheses hold a phrase, and a sentence inside them folds into the surrounding sentence or goes
- Statements, list items, and table cells open with the noun that names their subject, and instructions open with the verb
- Generic singular subjects ("a target that") become the plural ("targets that") or the verb, and "the" opens a subject the context identifies
- Runs of list items with one noun opener restructure around the verb or the category noun
- Overlap inside a sentence, one fact in two phrasings, keeps one phrasing
- Noun chains stop at three words, and a longer chain breaks with a preposition (`the timeout value for the connection pool`)
- Lines the reader must act on are instructions, and lines that record what the system does are statements
- Instructions use the imperative, with the condition before the command and a comma between them
- Instructions state the required form once, and forbidden forms sit in an anti-pattern table as patterns beside the correct form
- Warnings precede the step they guard and state the command or condition, then the risk
- Verbs name actions, and nominalizations and phrasal verbs take the verb from the word map
- Statements name the actor as their subject, and passive voice stays for an unknown actor
- Articles and "that" stay inside the sentence, except before a noun followed by an identifier (`restart pod web-7f9b2`)
- One word names one concept for the whole file: "check" or "verify" for one operation
- Contractions expand, and spelling is American

Modals are must, can, and will:

| [INDEX] | [MODAL]                       | [REWRITE]                    |
| :-----: | :---------------------------- | :--------------------------- |
|  [01]   | should, for a requirement     | must                         |
|  [02]   | should, for a suggestion      | Delete                       |
|  [03]   | may, might, could, would      | can, or the condition        |
|  [04]   | may have, before a participle | Stays, a possible past event |

## [05]-[DOCUMENTS]

Sections follow the order of work or dependency, each under a `## [NN]-[NOUN]` heading with no parenthetical, and each opens with the sentence a reader needs before its list or table:
- Each fact appears once, in the file that owns its topic and the section with the heading that names it
- The heading, the lead-in, and the previous sentence supply the subject, and sentences under them open with a part of it, its pronoun, or the verb
- The subject noun repeats where the fact otherwise attaches to another subject
- The opening sentence states the scope as one category, and every other sentence about the file, the section, or the skill goes
- Pointers to another file or skill are one line, `Use <name> for <purpose>`, naming the whole file and the purpose it serves
- Lines that describe a file, directory, section, or diagram node state its purpose, and the contents stay in it
- Prose about code names the command, identifier, or file and states its purpose, and the steps it performs stay in the code
- Facts sit where the reader needs them, and a link is the location of a thing the reader opens
- Paragraphs hold one topic, and parallel cases sit in one list, or in one sentence when each case is a phrase
- Entries (list items, steps, listing lines, table rows, tree and diagram comments) hold one fact or purpose in one line under 150 columns
- Entries open with a capital letter or an identifier and end without a period
- List items share one grammatical form and follow their lead-in colon without a blank line
- Items under an uppercase label hold the sentences the label needs
- Labels and sentence position give emphasis

## [06]-[TABLES]

Tables hold values, and the sentence that explains them stays in the section text:
- Column headers are one or two words that name the column content
- Cells hold values, identifiers, or short phrases without a period or semicolon
- Cells open with a capital letter, except backticked identifiers and literal words
- Rows over the entry width mean a cell narrates or the table has too many columns

Bracket headers with an [INDEX] column are the house form:

```markdown
| [INDEX] | [FLAG]    | [EFFECT]                            |
| :-----: | :-------- | :---------------------------------- |
|  [01]   | `--force` | Deletes rows absent from the source |
```

## [07]-[COMMENTS]

Comments state intent or a constraint the code cannot show, in one line and one statement, a sentence or a noun phrase, with no trailing period:
- The first letter is capitalized unless the comment starts with a backticked identifier or a tool name
- Each sentence stays whole on its line within the language line length, and intent that needs two lines moves to documentation or goes
- Consecutive full-line comments merge into one, or the one that repeats the code goes
- Inline comments stay and get the same removals
- Section dividers, structured doc comments with one element per line, commented-out configuration templates, and tool directives keep their form
- Doc comment summaries are one sentence that states what the member returns or does, and remarks keep one fact per sentence
- Members keep every `<param>` element or none
- Python docstrings keep the first-line period and follow the other rules, and every public module, class, and function keeps its docstring
- Messages (log, error, exception, diagnostic) state what happened, then the cause when known, then the action, each in one sentence with no period
- Commit subjects are imperative, and commit and pull request bodies state past facts

## [08]-[PROCESS]

Rewrite of an existing file:
1. Read the whole file and the files it points to, and check each fact against the files on disk
2. List every fact once, mark duplicates and facts the context or the owning file supplies, and choose the file and section for each
3. When the list holds no finding, report that the file complies and change nothing
4. Rename coined identifiers and files, with every reference
5. Edit section by section as targeted edits, terminology, then removals, then sentences, then document structure, and reread each touched section
6. Search the file for every entry in the removal, restructure, and word map tables and every scan pattern, and fix each hit
7. Run the language checkers and tests to zero warnings
8. Report bytes before and after, renames, coined terms removed, couplings left in place, facts added or corrected, and facts kept in longer form

New text follows the same rules from the first draft and gets the table and scan search before delivery. A review reports one row per finding: line, rule, offending text, rewrite.

## [09]-[SCAN]

| [INDEX] | [PATTERN]                                                         | [VIOLATION]             | [FIX]                                     |
| :-----: | :---------------------------------------------------------------- | :---------------------- | :---------------------------------------- |
|  [01]   | `;` outside code                                                  | Semicolon               | One sentence                              |
|  [02]   | `—`, ` - `, ` -- ` between statements                             | Dash                    | Rebuild, or `value — description`         |
|  [03]   | `^A `, `^An `, `. A `, `: A `, `- A `, and a cell opening `A `    | Article-led sentence    | Pluralize, or open with the verb          |
|  [04]   | Clauses chained by `:`, `;`, `—`, `, and `, or a repeated article | Listed content          | State it once, delete the rest            |
|  [05]   | Run of sentences under 8 words in a paragraph or list item        | Fragmented instruction  | State it once with condition and reason   |
|  [06]   | `\. \(` and a period inside parentheses                           | Parenthetical sentence  | Fold into the sentence or delete          |
|  [07]   | `'ll`, `'re`, `n't`, `'s` as a verb                               | Contraction             | Expand                                    |
|  [08]   | `has been`, `have been`, `is being`, `has` + participle           | Present perfect         | Simple past or present                    |
|  [09]   | `, making`, `, ensuring`, `, which means`, `, where`              | Tail clause             | Fold into the sentence or delete          |
|  [10]   | `should`, `may`, `might`, `could`, `would`                        | Modal                   | must, can, a condition, or delete         |
|  [11]   | ` if `, ` when ` after the command                                | Trailing condition      | Move before the command with a comma      |
|  [12]   | `such as`, `whose`, `e.g.`, `i.e.`, `etc.`, `and/or`              | Restructure entry       | Parenthetical, list, with, that, examples |
|  [13]   | `one`, `two`, `first`, `several`, `various`, `the two`            | Enumeration device      | Delete or pluralize unless a real value   |
|  [14]   | Comment line ending in `.`, `.-->`, or `.</` outside doc elements | Comment period          | Delete the period                         |
|  [15]   | Consecutive comment lines outside doc comments and sample labels  | Stacked comment         | Merge or delete                           |
|  [16]   | Period before a cell boundary inside a table                      | Cell period             | Rewrite the cell                          |
|  [17]   | Entry over 150 columns                                            | Entry width             | One fact, shorter cells, or fewer columns |
|  [18]   | `above`, `below`, `see `, `[NN]` in a sentence, `this file`       | Cross-reference or meta | Delete, place the fact where it is needed |
|  [19]   | `this`, `these`, `that`, `those` before a noun or as the subject  | Demonstrative           | Delete, or repeat the noun                |
|  [20]   | `<name>` owns, `is responsible for` before a topic                | Ownership statement     | `Use <name> for <purpose>`                |
|  [21]   | File, directory, or reference line holding a comma list           | Content enumeration     | State the purpose                         |
|  [22]   | `**`, emoji, or an uppercase word outside code and labels         | Emphasis markup         | Label or sentence position                |
|  [23]   | External URL outside a package page, download, or tool document   | Citation                | Delete, state the fact                    |
|  [24]   | `never`, `only`, `instead of`, `not` stating what a rule forbids  | Negative framing        | State the form, forbidden form to a row   |
|  [25]   | Sentence that walks the steps a command, target, or script runs   | Paraphrased code        | Name it, state its purpose                |
|  [26]   | Opening sentence or docstring with a `:` list of the contents     | Scope enumeration       | One category, or delete                   |
|  [27]   | `?` outside a quoted message                                      | Question                | State the fact or the purpose             |
