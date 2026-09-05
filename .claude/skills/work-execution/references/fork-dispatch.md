# [FORK_DISPATCH]

Fork dispatch runs a plan over independent systems in parallel: the main agent sends one fresh orchestrator per system, each orchestrator forks one agent per step in sequence and cleans and checks its own result, and the main agent reviews every touched file whole at the end.

## [01]-[FIT]

The style fits a plan with the steps numbered per system, the exact change per step written down, and the files of each system disjoint from the others. Shared files or one system leave the orchestrator layer nothing to relay or review.

## [02]-[DISPATCH]

The main agent dispatches every orchestrator in one message, as fresh Fable `general-purpose` agents, and the systems run at the same time from the same starting state. Before the first dispatch it reads the plan whole and lists the files in scope without reading them.

Each orchestrator brief holds, in order:
1. The edit rule in its first lines
2. The system, the repository root, the starting commit, and the sibling systems it leaves untouched
3. The steps, each pointing at its plan entry, and the reads before the first fork: the plan whole and every file in scope whole
4. The sub-briefs pasted in as fill-in templates: the fork brief, the cleaning fork brief, and the adversarial brief
5. The standards every brief holds, and the definition of each file kind the work produces
6. The done-when: every step landed as its entry states, every file reads as its kind, the checks print nothing, nothing partial or loose remains
7. The report contract, bounded in lines: steps done, the change per file in the plan's measure, facts not landed with their source, questions

## [03]-[ORCHESTRATOR_SEQUENCE]

Each orchestrator decides every judgment itself, messages `main` alone, and runs:
1. Read the plan whole, the standards the brief names, then every file in scope whole
2. Fork one agent (subagent type `fork`) per step in order with the step, its entry, the file, and the edit rule, and read the changed file on return
3. Fork one cleaning agent over the diff of the files each major step touched: a section of steps, a rebuilt section, or a new file
4. Correct course in the next brief from what the changed file shows
5. When every step is done, dispatch one fresh Fable `general-purpose` adversarial agent over the whole diff against the starting commit
6. Read every file in scope whole once more and make the final prose and structure pass with surgical edits
7. Run the checks the standards name over every file in scope, and fix each hit
8. Report to `main` under the contract

Forks hold the orchestrator's context, and their briefs name the step and the entry and repeat nothing the context holds. A fork brief that names no fresh reviewer gets a self-review recorded in place of the fresh pass. When the fork type is unavailable, a fresh `general-purpose` agent takes the step with the entry text and the standards pasted in.

## [04]-[TEMPLATES]

Fork brief:

```text
Never rewrite a whole file in one move: read the file, read the entry, write one scoped change, read the result.
Implement step <N> from entry <id> in <file>. Read the entry and the file section it names before the first edit,
apply the change as exact-string replacements that assert one match, read the result after each edit, and keep every
fact the entry lands elsewhere by writing it there in the same step. Report the lines changed and any fact the entry
names with no place in the file, in at most 12 lines.
```

Cleaning fork brief:

```text
Never rewrite a whole file in one move: read, one scoped change, read. Read `git diff` for <files> since <step>.
Clean the new text surgically: delete filler, restore a criterion the step paraphrased away, remove an
enumeration that binds a category to its current instances, and undo a structure copied from another file where
it does not fit. Change the fewest lines that fix each finding, assume nothing beyond the entry, and report in at
most 10 lines.
```

Adversarial brief:

```text
Never rewrite a whole file in one move: read, one scoped change, read. Read the plan and `git diff <commit> -- <files>`,
with new files whole. Check each fact against the file it came from at <commit>, against the file on disk where it
names a configuration value, and against the tool's documentation or help where it is about a tool. Correct what you
can justify: a fact lost between the old file and the new, a fact that is false, a structure that hides a rule,
and a pattern from one file copied into others to the result's cost. Report each correction as file, line, finding,
and reason, in at most 20 lines.
```

## [05]-[MESSAGING]

Orchestrators message `main` alone, and the main agent relays a finding about one system to the orchestrator that holds it. Findings that name a file outside every system (a repository manifest, a setting outside the plan) are the main agent's: it reads the file, makes the change, proves it by a run, and tells the orchestrator what changed, and the orchestrator's next brief holds the changed state.

## [06]-[CLOSE]

When every orchestrator returns, the main agent closes the run:
1. Fork one review agent that holds the plan and the intent and reads every touched file whole
2. Dispatch one fresh correction orchestrator with the findings, and it delegates each correction to a fresh agent per scope and reads the result
3. Send one fresh reviewer over anything built beside the main work against the intent it was built from

The review lists a missed or partial step, a dropped or false fact, a paraphrased criterion, prose that assumes the session, and a pattern spread across files. The review runs as a fork because the intent it checks against sits in the main context, and the correction agents start fresh because a fork of the reviewer holds its reading with its finding.

The run ends when the close checks print nothing over every file in scope and the report to the user states each file changed in the plan's measure, each fact added or corrected, and what the run left as it found it with the reason.
