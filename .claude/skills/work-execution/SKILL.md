---
name: work-execution
description: Use when a settled plan must run, the main agent orchestrates the steps itself or delegates one orchestrator per system under one edit rule.
---

# [WORK_EXECUTION]

Runs a settled plan of scoped changes, and decides who reads, who writes, who reviews, and in what order.

[REFERENCES]: the execution styles, each a way to run the plan's steps:
- [01]-[FORK_DISPATCH](references/fork-dispatch.md): One fresh orchestrator per system, a fork per step, fresh agents where bias is the risk

## [01]-[DECISION]

The main agent orchestrates the work itself or delegates it, and the plan's shape decides:

| [INDEX] | [CRITERION] | [MAIN_ORCHESTRATES]                                   | [DELEGATES_ORCHESTRATORS]                      |
| :-----: | :---------- | :---------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Scopes      | One system, or scopes that consume each other's files | Independent systems with disjoint files        |
|  [02]   | File count  | Every changed file fits in the main context whole     | Changed files exceed the main context          |
|  [03]   | Parallelism | Steps run in sequence on one tree                     | Systems run at the same time on disjoint paths |

When the main agent orchestrates, it runs each step as one fresh agent or one fork over its scope, in sequence, and reads each changed file as it lands. When it delegates, each orchestrator runs one system under an execution style, and the main agent holds the plan, the messages, and the final review.

## [02]-[SHARED_RULES]

Every style runs under the same rules:
- Read a file whole before its first write, write one scoped change, read the result, and continue, and no file is rewritten in one move
- Fresh Fable agents decide, edit, and prove, and Opus agents gather information the deciding agent judges
- Forks take work where context is the point (a step, a cleaning pass, the main review), and fresh agents take work where bias is the risk
- A fork holds the dispatch tool under a default that tells it to execute directly, and a brief that needs a fork to dispatch says so in plain words
- Every brief opens with the edit rule and pastes in the standards the work meets: prose, placement, language rules, checkers at zero warnings
- Every agent messages `main` with a finding outside its scope, and `main` relays it, makes the change, or dispatches a focused agent as it arrives
- Hold no finding back for a later pass, and defer, store, or hedge nothing
- The main agent, or a fork of its context, reads every touched file whole at the end and lists findings as file, line, finding, correction
- Commits follow the user's instruction for the session, and a run that must show its work stays uncommitted against the starting commit
- The main agent writes its memory of the plan's intent and execution shape before the first dispatch and rewrites it at the close from the run
- Implement each improvement to the skill or a reference that a run identifies in place: delete, reframe, or correct
