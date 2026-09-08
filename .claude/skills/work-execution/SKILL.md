---
name: work-execution
description: "Use when planned steps must run from a prompt, plan file, or document set, covering the orchestration decision and shared rules."
---

# [WORK_EXECUTION]

Runs settled steps of scoped changes from the input as it arrives (steps in the prompt, a plan file, a document set, or a branch cut from a larger task), and decides who reads, who writes, who reviews, and in what order. Use `work-planning` for writing a plan when the steps need one.

[REFERENCES]:
- [01]-[FORK_DISPATCH](references/fork-dispatch.md): One orchestrator per branch, a fork per step, fresh agents where bias is the risk

## [01]-[DECISION]

The main agent orchestrates the work itself or delegates it, and the shape of the steps decides:

| [INDEX] | [CRITERION] | [MAIN_ORCHESTRATES]                                   | [DELEGATES_ORCHESTRATORS]                       |
| :-----: | :---------- | :---------------------------------------------------- | :---------------------------------------------- |
|  [01]   | Scopes      | One branch, or scopes that consume each other's files | Independent branches with disjoint files        |
|  [02]   | File count  | Every changed file fits in the main context whole     | Changed files exceed the main context           |
|  [03]   | Parallelism | Steps run in sequence on one tree                     | Branches run at the same time on disjoint paths |

When the main agent orchestrates, it runs each step as one fresh agent or one fork over its scope, in sequence, and reads each changed file as it lands. When it delegates, each orchestrator runs one branch from its brief (the scope, the steps or the intent, the starting commit, and the checks). The main agent holds the steps, every record the session keeps, and the final review.

## [02]-[SHARED_RULES]

Every style runs under the same rules:
- Read a file whole before its first write, write one scoped change, read the result, and continue, and no file is rewritten in one move
- Fresh Fable agents decide, edit, and prove, and Opus agents gather information the deciding agent judges
- Forks take work where context is the point (a step, the main review), and fresh agents take work where bias is the risk
- Forks hold the dispatch tool under a default that tells them to execute directly, and a brief that needs a fork to dispatch states it in plain words
- Every brief pastes in the standards the work meets by file kind, and the spawn hook adds the edit rule to a `fork` or `general-purpose` spawn
- Spawn `Agent(subagent_type: "prose-editor")` with the files touched since the last checkpoint listed
- Every dispatcher's brief to a worker names its own address, `main` from the main agent, else the `name` given at its own spawn
- Readers of a report land each finding in it, make the change, or dispatch a focused agent in the same round
- Hold no finding back for a later pass, and defer, store, or hedge nothing
- The main agent, or a fork of its context, reads every touched file whole at the end and lists findings as file, line, finding, correction
- Commits follow the user's instruction for the session, and a run that must show its work stays uncommitted against the starting commit
- The main agent rewrites its memory of the work's intent and execution shape at the close from the run
- Implement each improvement to the skill or a reference that a run identifies in place: delete, reframe, or correct
