---
name: delegate-codex
description: Use when the user asks to run Codex, or references OpenAI Codex for code analysis, refactoring, or automated editing
---

# [DELEGATE_CODEX]

`codex exec` runs a non-interactive agent in its own context window and returns one final message:
- Codex is maximally literal, it follows a clean contract exactly and exhaustively, ambiguity or implicit intent burns tokens reconciling scope, leaner prompts outperform
- Codex loads all the same skills and project guidance instructions do NOT re-iterate or repeat Codex already has (content from `CLAUDE.md`, `AGENTS.md`, skills, etc)

## [01]-[EXECUTION]

1. Use model and reasoning effort from `.codex/config.toml` (model: Astra, effort: High)
2. Use `--dangerously-bypass-approvals-and-sandbox` unless the task is analysis/read-only
3. Append `</dev/null 2>/dev/null` after the prompt, Codex reads piped stdin as additional context and waits for EOF
4. Assemble the command, invoke Bash with `run_in_background: true` and `timeout: 1200000` (1200 seconds)

Keep command flags on one line and the prompt on separate lines:

```bash
codex exec --model gpt-6-astra --config 'model_reasoning_effort="high"' --dangerously-bypass-approvals-and-sandbox --cd "<DIR>" --skip-git-repo-check '
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
' </dev/null 2>/dev/null
```

Resume with a positional prompt if follow-up is needed:

```bash
codex exec --model gpt-6-astra --config 'model_reasoning_effort="high"' --dangerously-bypass-approvals-and-sandbox --cd "<DIR>" --skip-git-repo-check resume --last '
<context>

New evidence or decisions since the previous turn

</context>

<objective>

Next action or question to resolve

</objective>

<output>

Deliverables and information to return

</output>
' </dev/null 2>/dev/null
```

## [02]-[QUICK_REFERENCE]

| [INDEX] | [USE_CASE]                 | [COMMAND_OPTIONS]                                                |
| :-----: | :------------------------- | :--------------------------------------------------------------- |
|  [01]   | Apply local edits          | `--dangerously-bypass-approvals-and-sandbox`                     |
|  [02]   | Read-only analysis         | `--sandbox read-only --config 'approval_policy="never"'`         |
|  [03]   | Resume recent session      | `resume --last` after exec options, before the structured prompt |
|  [04]   | Run from another directory | `--cd "<DIR>"` before the prompt, `resume`, or `review`          |
|  [05]   | Review uncommitted changes | `review --uncommitted` after read-only exec options              |
|  [06]   | Review a commit            | `review --commit "<SHA>"` after read-only exec options           |

`review` accepts one of `--uncommitted`, `--base`, `--commit`, or a custom positional prompt. Target flags cannot combine with a prompt.

```bash
codex exec --model gpt-6-astra --config 'model_reasoning_effort="high"' --sandbox read-only --config 'approval_policy="never"' --cd "<DIR>" --skip-git-repo-check review --uncommitted </dev/null 2>/dev/null
```

## [03]-[EVALUATION]

Codex is powered by OpenAI models with their own limitations, treat Codex as a colleague, not an authority:
- Trust your own knowledge when confident, if Codex claims something you know is incorrect, push back directly
- Research disagreements using relevant skills/tools available before accepting Codex's claims, share findings with Codex via resume
- Don't defer blindly, Codex can be wrong, evaluate its suggestions critically

## [04]-[DISAGREEMENTS]

1. Gather evidence from current official documentation and source
2. Resume the Codex session, identify yourself as Claude with your actual model name, and ask for Codex's reasoning and evidence

```bash
codex exec --model gpt-6-astra --config 'model_reasoning_effort="high"' --sandbox read-only --config 'approval_policy="never"' --cd "<DIR>" --skip-git-repo-check resume --last '
<context>

This is Claude (your current model name). I disagree with [X] because [evidence].

</context>

<objective>

Identify all justifications for your conclusion and cite supporting evidence. Evaluate the counterclaim and evidence supplied by Claude, then state whether your conclusion holds or requires revision.

</objective>
' </dev/null 2>/dev/null
```

1. Discuss the disagreement with Codex; either model can be wrong
2. Compare both explanations against evidence, resolve the disagreement, and proceed with the supported conclusion
3. Let the user decide how to proceed if there's genuine ambiguity

## [03]-[ERROR_HANDLING]

- Stop and report failures whenever `codex --version` or a `codex exec` command exits non-zero; request direction before retrying
- When output includes warnings or partial results, summarize them and ask how to adjust using `AskUserQuestion`
