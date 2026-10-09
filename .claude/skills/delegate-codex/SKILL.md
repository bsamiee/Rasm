---
name: delegate-codex
description: "Use when a task, review, or analysis is delegated to Codex, covering the command, sandbox, prompt, resume, result, and failure."
argument-hint: "[task]"
---

# [DELEGATE_CODEX]

`codex exec` runs Codex non-interactively over the repository's own `AGENTS.md`, `.codex/config.toml`, skills, agents, and hooks.

## [01]-[COMMAND]

With a positional prompt, Codex appends piped stdin as a `<stdin>` block.

1. Write the prompt as the brief the task's dispatcher gives any agent, in the block form below where no plan file holds the brief
2. For analysis or review, add `--sandbox read-only` before the prompt or subcommand
3. Run through `Bash` with `run_in_background: true`, stdout holding the final message and stderr the settings header with `session id:` and the transcript

```bash
codex exec --cd "${CLAUDE_PROJECT_DIR}" '
<context>

Task-specific facts and relevant paths

</context>

<objective>

Work to perform and boundaries

</objective>

<done_when>

Concrete outcome to achieve

</done_when>

<output>

Deliverables and information to return

</output>
' </dev/null
```

Resume with `session id:` from stderr:

```bash
codex exec --cd "${CLAUDE_PROJECT_DIR}" resume <SESSION_ID> '<PROMPT>' </dev/null
```

## [02]-[REFERENCE]

Place `codex exec` options before `resume` or `review`.

| [INDEX] | [USE_CASE]                 | [COMMAND_OPTIONS]                    |
| :-----: | :------------------------- | :----------------------------------- |
|  [01]   | Outside a Git repository   | `--cd "<DIR>" --skip-git-repo-check` |
|  [02]   | Review uncommitted changes | `review --uncommitted`               |
|  [03]   | Review against a branch    | `review --base "<BRANCH>"`           |
|  [04]   | Review a commit            | `review --commit "<SHA>"`            |
|  [05]   | Custom review instructions | `review '<PROMPT>'`                  |
|  [06]   | Final message to a file    | `-o "<FILE>"`                        |

## [03]-[RESULT]

- Stdout's final message is read against the task as any agent's report, and a correction resumes the session with the finding rows
- Non-zero exits leave their cause in stderr, and the task goes to an `Agent` call with the same brief as `prompt`
