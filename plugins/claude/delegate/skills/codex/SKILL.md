---
name: codex
description: "Use when work needs OpenAI's agent: its own review of a diff, a precise second reader, an exhaustive defect hunt, a 3D scene or formal proof, or a cheap bounded task run beside this session, covering headless `codex exec` runs, the choice between Sol and Astra, briefs, structured results, and result checks"
argument-hint: "[task]"
---

# [DELEGATE_CODEX]

`codex exec` runs OpenAI's Codex agent headless in the current checkout. It reads `AGENTS.md`, `.codex/config.toml`, and every plugin and skill they enable, so a brief states the job and never the rules or tools those hold.

## [01]-[JOBS]

Every job runs at `xhigh` effort on the standard service tier, and its row picks the model:

| [INDEX] | [JOB]  | [WHAT_IT_DOES]                                                               | [MODEL]         |
| :-----: | :----- | :--------------------------------------------------------------------------- | :-------------- |
|  [01]   | Review | Codex's own review of uncommitted changes, a branch, or a commit             | `gpt-6.2-sol`   |
|  [02]   | Read   | Precise second reading of a finished feature, few findings and few wrong     | `gpt-6.2-sol`   |
|  [03]   | Task   | Bounded terminal or code task its brief states, proven by a command          | `gpt-6.2-sol`   |
|  [04]   | Hunt   | Exhaustive defect search where a miss costs more than triage of false alarms | `gpt-6.2-astra` |
|  [05]   | Trace  | One defect crossing services or processes this session failed to explain     | `gpt-6.2-astra` |
|  [06]   | Scene  | 3D or interactive scene: WebGL, a game prototype, Blender scripting          | `gpt-6.2-astra` |
|  [07]   | Proof  | Formal mathematics, a proof assistant, an algorithm with a stated bound      | `gpt-6.2-astra` |

Mode is this session's call per run: `read` changes no file, `write` edits files in place in this checkout. Task, Scene, and Proof write, every other job reads, and findings this session confirmed go to a fresh write run.

Features to merge, large refactors, unattended runs of hours, security review, and prose for readers stay here or go to `delegate:antigravity`: Astra stops early, reports tersely, and refuses some vulnerability work.

Host work (Blender, Rhino, Adobe applications) is briefed by application name alone, as Codex loads this repository's host plugins. One agent drives one running host, so a host this session uses stays here, and a headless Blender is free to hand over.

## [02]-[RUN]

Every `codex` command runs through `mcp__delegate__run`, loaded once with `ToolSearch` `select:mcp__delegate__run`:

```json
{
    "command": "codex exec <mode> -m <model> -c model_reasoning_effort='\"xhigh\"' -c service_tier='\"default\"' -o <answer> \"$(cat <brief>)\" </dev/null",
    "description": "<job> <subject>",
    "limit": "90 minutes"
}
```

- Limit: `codex exec` has no time limit of its own, so `limit` ends a hung run, reported `ended at limit`
- Options: every option precedes `review` and `resume`, and a resumed run inherits none
- Input: piped stdin joins the prompt as a `<stdin>` block, `</dev/null` keeps it empty
- Folder: `--cd <dir>` sets the working root, `--skip-git-repo-check` allows one outside a repository
- Read: `--sandbox read-only` refuses every write
- Write: no `--sandbox`, the run takes the sandbox `config.toml` sets and the brief alone bounds its task
- Data: `--output-schema <file>` makes the final message JSON of that schema, each object closed with every field required
- Events: `--json > <events.jsonl>` writes JSONL, `thread.started` holds the `thread_id`
- Follow-up: `codex exec <options> resume <thread_id> "<prompt>"` continues that run, `--ephemeral` keeps no session to resume
- Redirect: a run that went wrong is stopped and started again from a new brief, steering it costs more

Review runs Codex's own rubric without subagents over one selector, or over a prompt alone that names what to review, never both, each command a run's `command`:

```bash
codex exec --sandbox read-only -m gpt-6.2-sol -c model_reasoning_effort='"xhigh"' -o <answer> review --uncommitted
codex exec --sandbox read-only -m gpt-6.2-sol -c model_reasoning_effort='"xhigh"' -o <answer> review --base <branch>
codex exec --sandbox read-only -m gpt-6.2-sol -c model_reasoning_effort='"xhigh"' -o <answer> review --commit <sha>
```

Rules a review enforces sit under `## Code Review Rules` in the `AGENTS.md` nearest the code, each stating the behavior to flag and its safe path.

## [03]-[BRIEF]

- Outcome first, then what this session checks when the work returns
- Done condition as a command or an observable state: make it run, read the result, fix what fails
- Actions that proceed without asking, listed, in place of a rule to ask first
- For a read or hunt, each finding with its trigger, what is observed, and how to verify it
- For a write, the task and the command that proves it done, and no commit
- For a scene or proof, the artifact's path and the command that opens or checks it
- Final message complete without the run's history, with every blocker it met
- Headless runs ask nothing: Codex decides each open choice itself, so the brief states every choice that matters

## [04]-[RESULT]

Reports and the files they name decide a run's result:

- Good results have exit `0` and an `-o` file of some size, findings included, and `turn.failed` in the events marks a failed turn
- Stderr holds connection errors of MCP servers whose application is closed, the `-o` file alone is the verdict
- Blockers Astra met can be missing from its report: a task is judged by its diff and proving command rerun here
- Findings are read against the code before they are used, Astra's hunt most of all
- Use `delegate:audit` for a reading of paths by parallel readers, `delegate:council` for a scored review of a change
