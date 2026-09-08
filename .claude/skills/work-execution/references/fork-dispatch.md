# [FORK_DISPATCH]

Fork dispatch runs work over independent branches in parallel: the main agent sends one `system-orchestrator` per branch, each orchestrator forks one agent per step and cleans and checks its own result, and the main agent reviews every touched file whole at the end.

## [01]-[FIT]

The style fits work with the steps stated per branch, in the prompt, in a plan file, or in a document set, and the files of each branch disjoint from the others. Shared files or one branch leave the orchestrator layer nothing to relay or review.

## [02]-[DISPATCH]

The main agent dispatches every orchestrator in one message, and the branches run at the same time from the same starting state. Before the first dispatch it reads the steps whole and lists the files in scope without reading them.

Each orchestrator brief holds, in order:
1. The scope as paths from the repository root, the steps or the intent with the document line each points at, the starting commit, and the checks
2. The standards every brief holds by file kind, and the definition of each file kind the work produces

The main agent keeps every record the session holds from the reports, and no orchestrator reads or writes one.

## [03]-[WORKERS]

Each orchestrator decides every judgment itself. Forks take the steps, because they hold the orchestrator's context, and each fork's brief names the step and the source line and repeats nothing the context holds. Skills the orchestrator loaded before the fork reach it. Fresh `general-purpose` agents take the adversarial pass over the whole diff and every follow-up to a returned worker, because they hold their brief alone and a resumed fork can read the message as another agent's. When the fork type is unavailable, a fresh `general-purpose` agent takes the step with the step text and the standards pasted in. Fork briefs that name no fresh reviewer get a self-review recorded in place of the fresh pass. The `prose-editor` agent runs over the files touched since the last checkpoint, listed in its prompt, because the starting commit on a shared tree covers every branch.

## [04]-[TEMPLATES]

The `function-hooks` plugin appends its shared lines to every `fork` and `general-purpose` spawn, and each template holds the task alone.

Fork brief:

```text
Implement step <N> in <file>: <step as stated>, from <document line> where the prompt points at one. Read the source
line and the file section it names before the first edit, keep every fact the step lands elsewhere by writing it
there in the same step, and run <check> after the edit.
```

Adversarial brief:

```text
Read the steps as stated and `git diff <commit> -- <scope>`, with new files whole. Check each fact against the file it
came from at <commit>, against the file on disk where it names a configuration value, and against the tool's
documentation or help where it is about a tool. Correct a fact lost between the old file and the new, a fact that is
false, a structure that hides a rule, and a pattern from one file copied into others to the result's cost.
```
