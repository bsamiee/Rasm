---
name: delegate-codex
description: "Use when a task, review, or analysis is delegated to Codex, covering the command, sandbox, prompt, resume, result, and review options."
argument-hint: "[task]"
---

# [DELEGATE_CODEX]

`codex exec` runs Codex non-interactively over the repository's own `AGENTS.md`, `.codex/config.toml`, skills, agents, and hooks.

## [01]-[COMMAND]

With a positional prompt, Codex appends piped stdin as a `<stdin>` block.

1. Supply checkout, scope, writable files, acceptance criteria, and distinct event, diagnostic, and final-answer paths in the dispatch brief
2. Keep `--sandbox read-only` for analysis and review, omitting it only for an authorized writing assignment
3. Run through `Bash` with `run_in_background: true` and retain its process handle for waiting, cancellation, and exit status

```bash
codex exec --json --cd "$checkout" --sandbox read-only -o "$final_answer" "$prompt" </dev/null >"$events_jsonl" 2>"$diagnostics_log"
```

Capture `thread_id` from `thread.started` in JSONL stdout and resume that conversation explicitly:

```bash
codex exec --cd "$checkout" --sandbox read-only resume --json -o "$next_final_answer" "$thread_id" "$followup" </dev/null >"$next_events_jsonl" 2>"$next_diagnostics_log"
```

## [02]-[RESULTS]

Read the `-o` destination after process termination and accept the requested artifacts against the dispatch brief.

- Item identities and started, updated, and completed events describe progress
- Completed `agent_message` items can be commentary; item completion does not identify a final answer
- `turn.completed`, `turn.failed`, and stream errors report turn termination, separately from process exit
- `--last` can select another concurrent conversation; `--ephemeral` prevents persisted resume
- Forks isolate conversation history, not files or checkout-bound services
- `--output-schema` serves a consumer requiring structured final fields

## [03]-[REFERENCE]

Place checkout and sandbox options before `resume` or `review`.

| [INDEX] | [USE_CASE]                 | [COMMAND_OPTIONS]                    |
| :-----: | :------------------------- | :----------------------------------- |
|  [01]   | Outside a Git repository   | `--cd "<DIR>" --skip-git-repo-check` |
|  [02]   | Review uncommitted changes | `review --uncommitted`               |
|  [03]   | Review against a branch    | `review --base "<BRANCH>"`           |
|  [04]   | Review a commit            | `review --commit "<SHA>"`            |
|  [05]   | Custom review instructions | `review '<PROMPT>'`                  |
