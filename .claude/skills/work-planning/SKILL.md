---
name: work-planning
description: Use when a request in plan mode must become a plan with companion files, covering reads, questions, companion files, plan sections, adversarial planner, and close.
---

# [WORK_PLANNING]

Plans work over a body of files in plan mode, and ends where a fresh session holds everything it needs in the plan file and the companion files, with none of the planner's context. The plan structures the work as systems with disjoint files, steps per system, and pre-dispatch steps for the files the systems share, and names no execution style. Use `work-execution` for running the plan.

[REFERENCES]: the plan's structure for one execution style:
- [01]-[ORCHESTRATED_RUN](references/orchestrated-run.md): What the plan states when one orchestrator per system runs it

## [01]-[READS]

Read every file in scope whole before the first question, and measure each in the same pass:
- Line count per file
- Entries at or over 150 columns: `awk 'length >= 150 && /^(- |\| |[0-9]+\. )/' <file>`
- Self-references inside the file, the cross-reference row of the `clean-prose` scan table
- The commit the line numbers refer to, recorded once beside the measurements

Facts about the harness or a tool come from its documentation or a probe, and each enters the findings record with its source. The status record lists the files read for context and left untouched under their own heading.

## [02]-[QUESTIONS]

Questions gate the structural choices (which files exist, what each owns, the shape they share) and come in rounds through `AskUserQuestion` before any candidate becomes a change:
- Each answer enters the findings record verbatim as a dated decision, and the candidates derive from the decisions
- State decisions made on the user's behalf in the plan and in the report with the step each binds
- Ask a later round when the drafts raise a choice the files cannot settle

## [03]-[COMPANION_SET]

The planner chooses the companion directory, `.claude/scratch/<run>/` in the project unless the prompt names another location. Every pointer in the plan names the companion directory. Plan mode writes the plan file under `~/.claude/plans/` (or `plansDirectory`) and auto-approves no other write. The planner writes the companion files in the companion directory when the write is possible, else under the session scratchpad or a local temporary directory, and the first step of the build order copies them into the companion directory.

| [INDEX] | [FILE]        | [HOLDS]                                                                               |
| :-----: | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `STATUS.md`   | Edit rule, per-step check, intent, files in scope by system, steps in order with ids  |
|  [02]   | `SYSTEM.md`   | What each file or system holds, the intent, and how the files reach each other        |
|  [03]   | `FINDINGS.md` | Rows with a source, decisions verbatim, settled facts, disagreements, open questions  |
|  [04]   | `CHANGES.md`  | One entry per step under a heading per system, the exact change and its proof command |
|  [05]   | `REVIEW.md`   | Empty frame with the columns file, line, finding, correction, standard, verdict       |

Each file follows its own rules:
- The status record opens with the edit rule and the check each step ends with, then the intent under 300 words and the pointer to the system record
- The status record groups the files in scope by system with new files marked, then lists the steps in order, each one line with its change id
- The system record draws how the files reach each other as a diagram when a list cannot show the relation
- The findings record holds the open questions in its last section
- Each of the adversarial planner's disagreements enters the findings record as a row of plan item, found, and changed
- Ids in the changes record read `<system letter><n>`, and each entry holds the exact change as text or as line moves that name each destination
- Entries in the changes record carry the proof command and a one-line reason, because a later agent reads the intent from the entry alone
- Entries a later finding deletes stay as deleted entries with the reason
- The review frame is filled at the close of the run

## [04]-[PLAN_FILE]

The plan file holds the sections in the order a fresh agent reads them, and each holds one category:

| [INDEX] | [SECTION]         | [HOLDS]                                                                                   |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | Intro             | What a fresh agent needs before the plan                                                  |
|  [02]   | Context           | The system as it is and the intent                                                        |
|  [03]   | Established facts | Every fact the design rests on, each with its source                                      |
|  [04]   | Design            | One section per system, every file named, rejected alternatives with the reason           |
|  [05]   | Build order       | Pre-dispatch steps for the shared files, steps per system with a proof each, dependencies |
|  [06]   | Structure         | The systems and their files, the files the main agent owns, the companion directory       |
|  [07]   | Verification      | Static checks at zero findings, runtime proofs as a table, the close conditions           |

The intro tells a fresh agent:
- It holds none of the planner's inferred understanding, and it reads the named files whole, the smallest set that supplies the understanding
- The intent in the user's words
- The edit rule (read the file whole, one scoped change, read the result, no whole-file rewrite)
- Wrong specifics (index, file name, API shape, line number, a claim the documentation or the repository contradicts) are corrected from the intent
- Corrections enter the findings record with the plan text and the replacing fact, and one touching another's work reaches `main` in the same round
- Nothing is deferred, hedged, guarded, or worked around
- Missing packages go into the catalog, missing settings into their owning file, wrong rules into the rule, in the same run

Structure names once the standards and the checks every brief carries per file kind, and marks the files in scope that are lasting repository content. Verification's runtime proofs are a table of input, expected, and proof text.

## [05]-[ADVERSARIAL_PLANNER]

One fresh agent redoes the analysis over the files in scope and the plan files before the plan closes:
- It disagrees where the files or the recorded decisions show a draft wrong, and edits the plan files
- It holds no `AskUserQuestion`, and it messages the planning session with the questions it cannot settle
- The planner puts each relayed question to the user verbatim and sends the answers back
- The planner reads the plan files whole once more against the intent and the decisions

## [06]-[CLOSE]

The plan closes when each check over the plan file and the companion files holds:
- Every path the files name exists
- Every step in the status record has its entry in the changes record, and every entry has its proof command
- Every decision on the user's behalf is stated with its step
- The memory of the plan's intent and the standards the user set is written
