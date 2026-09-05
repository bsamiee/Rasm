# [SKILL_IMPROVEMENT]

Improve the ast-grep skill by comparing every section against the tool's sources ranked by authority, landing each correction in one section, and proving it by a run.

## [01]-[SOURCES]

A fact is settled by the highest-ranked source that states it, and a disagreement between two sources ends with a probe against the installed binary:

| [INDEX] | [SOURCE]               | [DECIDES]                                                   |
| :-----: | :--------------------- | :---------------------------------------------------------- |
|  [01]   | Binary probe           | Every disputed behavior, exit codes, flag conflicts         |
|  [02]   | Crate source           | Match order, `stopBy` walks, env inheritance, error text    |
|  [03]   | JSON schema            | Key names, value shapes, the kind and field enums per grammar |
|  [04]   | Documentation          | Intended semantics, worked forms, the catalog               |
|  [05]   | Changelog and releases | When a capability landed, what a bump changed               |
|  [06]   | Issues and discussions | Idioms the maintainer gave, declined features, edge cases   |
|  [07]   | Maintained rule sets   | Folder layouts, naming, utils, tests, CI wiring in real use |
|  [08]   | Subcommand help        | Flag set and defaults of the installed version              |
|  [09]   | MCP server source      | What each tool passes, returns, hides, and rejects           |

A probe is one scratch project, one rule, one file, the command, and the exit code, and its result outranks every page. The crate sources that decide a rule question sit under `crates/config/src` and `crates/core/src` at the installed tag, the CLI under `crates/cli/src`, and the schemas under `schemas/`. The documentation states intent and lags the binary in both directions: a claim a probe disproves (`regex` matching the whole node text, the binary matches an unanchored substring), a type the schema widens (`metadata` as string to string, the value is free-form), a value the site keeps to the CLI and the schema accepts in a rule (`template` strictness in a pattern object), and a capability the site omits (parameterized utils, the `fix` list, `$CONTENT`, `$LANG`, `metaVarChar`), each settled by the source ranked above the site. A thread answer describes the version of its date, and a thread claim about order or scope gets the source read at the installed tag before it lands.

## [02]-[SEQUENCE]

Run the steps in order, and stop at the step with a failing criterion. A claim that entered without a rank stays wrong through later rebuilds, because each rebuild trusts what is already there, and the first step ranks every existing fact before any source is read:

| [INDEX] | [STEP]                                                   | [CRITERION]                                                               |
| :-----: | :------------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | List every fact of `SKILL.md` with section, rank, owner  | Fact with no rank from the source table is the first probe candidate      |
|  [02]   | Gather every ranked source to the skill's `.archive/`    | Sources below the probe have a file on disk, the probe runs on disagreement |
|  [03]   | Read the findings files, then the sources they cite      | Every fact the skill states or lacks is read in its source                |
|  [04]   | Compare section by section, classify each finding        | Missing capability, wrong claim, obsolete claim, thin claim, or coupling  |
|  [05]   | Decide by rank, probe disagreements and rules of thumb   | Default, order, or limit has a probe result beside it                     |
|  [06]   | Edit one section at a time, read, rewrite, read again    | Each rule states the category and the criterion for the next unseen case |
|  [07]   | Verify against the fact list and the prose rules         | One owner per fact, entries under 150 columns, the `clean-prose` scan empty |
|  [08]   | Compare against history                                  | Every more precise earlier criterion, capability, or flag is restored     |
|  [09]   | Prove by a run                                           | `ast-grep test`, the gate scan, one template filled, one example run      |
|  [10]   | Report                                                   | Weaknesses with sources, sections changed, proofs, out-of-scope findings  |

The gather dispatches one Opus general-purpose gatherer per source kind into the skill's `.archive/`, each briefed with the output folder, the installed version, the method, the coverage criterion, and a report of at most 15 lines, and the gather reads an archive that holds a source kind in place of gathering that kind again. A capability qualifies when an agent following the section fails a real task without it, and a source naming a key qualifies nothing. Placement follows one test: a fact goes in `SKILL.md` when leaving it out lets an agent violate a standard, and in a reference when leaving it out only slows the agent, and a flow, a long example, or a history moves to a reference behind a one-line pointer. The history comparison reads `git log -p -- <file>` and `git diff HEAD -- <file>`, and where the earlier text and the current text disagree, a probe decides which one is restored.

## [03]-[SMELLS]

| [INDEX] | [SMELL]                                                | [CORRECT_FORM]                                                            |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Example that matches one literal shape                 | Family shape in `utils` with the siblings the correction applies to       |
|  [02]   | Table row with a mechanism and no criterion            | Category, the criterion for the next case, then the mechanism             |
|  [03]   | "Do not" with no correct form beside it                | Correct form as the rule, the forbidden form in an anti-pattern row       |
|  [04]   | Fact stated in two sections or two files               | One owner, a one-line pointer where the other reader needs it             |
|  [05]   | Worked flow inside `SKILL.md`                          | Rule in the skill, the flow in the reference the listing line names       |
|  [06]   | Listing line naming keys, flags, or mechanisms         | Purpose the file serves, under 150 columns                                |
|  [07]   | Claim entered from one page without a rank             | Rank from the source table, a probe when the rank sits under the source     |
|  [08]   | Guidance coupled to one repository's paths or packages | Placeholder paths, the packages as vocabulary alone                       |
|  [09]   | Version marker or "since" in a rule                    | Behavior of the installed release stated as a fact                        |
|  [10]   | Hedge that lets an agent skip a step                   | Condition under which the step runs, then the step                        |
|  [11]   | Layout or naming taken from another project's habit   | Form the maintained sets and the binary agree on, probed                  |
|  [12]   | Workaround where the binary documents an option        | Option from the help or the schema, `strictness: ast` for a comma         |
|  [13]   | Sequence that starts from an existing rule             | Correction to real code first, the rule derived from it                   |
|  [14]   | History or log of past failures kept in the guidance   | Class as a smell row with its correct form, the instance dropped          |

One rebuild, with the source that decided it:

```text
BEFORE  `expandStart`/`expandEnd` consume exactly one adjacent sibling matching the sub-rule (`stopBy` inert)
AFTER   `expandStart`/`expandEnd` extend the fix range to the first sibling matching the sub-rule, the adjacent one by default and any under `stopBy: end`
SOURCE  crates/config/src/fixer.rs walks node.next() under neighbor and node.next_all() under end, proven by scan --json replacementOffsets
```

## [04]-[OPPORTUNITIES]

A real opportunity is a documented or proven capability an agent following the skill needs on a task it names and no section uses, or a section with steps an agent can follow and still produce a weak result. Each lands in the section that owns its mechanism, as one line with a criterion, and stays out when no task in the skill's scope reaches it. The source rows yield candidates in each listed category, and the check for each is the run that exposes it:

| [INDEX] | [CATEGORY]                                     | [CHECK]                                                            |
| :-----: | :--------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Flag conflict that silently drops an operation | Two flags on one command, the file and the exit code read afterward |
|  [02]   | Default that decides what a green run proved   | Flag omitted, the severity or the threshold read from the output     |
|  [03]   | Path resolved against a root the caller is not | Command run from a subdirectory, the reported paths compared         |
|  [04]   | Output field a format carries and another does not | Both formats over one match, the key sets diffed                 |
|  [05]   | Interactive key or prompt the session depends on | Prompt source at the installed tag, the keys read                  |
|  [06]   | Filter or severity flag that hides a rule      | Scan with and without it, the rule counts compared                   |
|  [07]   | Tool wrapper folding two outcomes into one     | Tool call and the CLI form over one input, exit codes compared       |

## [05]-[CRITERIA]

Improvements from an agent's suggestions land when each meets every criterion:
- A step an agent skipped or ran without its criterion is rebuilt with the criterion it lacked, in place, and the step count stays
- A source an agent needed and did not reach becomes a row of the source table, and the agent's sources table gains the exact command
- A failure class the smell table lacks joins it with its correct form, and its check joins the step that owns it
- A finding reported as a log of a run is rewritten as the principle or the poor guidance it exposes before it lands anywhere
- A second instance of a class collapses into the class row, and a second phrasing of a step replaces the first
