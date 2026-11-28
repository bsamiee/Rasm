---
name: council
description: "Use when the owner says council, or asks for a scored review of a change by one model per provider, covering preparation, members, runs, rounds, scores, and the pull request table"
---

# [DELEGATE_COUNCIL]

Councils give one change to one delegate per provider, each a strong model at high reasoning effort, usually for review or feedback, and fix what they find until every delegate is satisfied.

## [01]-[PREPARATION]

- Outstanding pull request comments, known issues, and relevant checks are handled before a round, so the council reads the updated implementation
- Known fixes are made before the round, never deferred until reviews return
- Code the owner discards to explore new ideas takes no fixes

## [02]-[MEMBERS]

Members the owner names stand, else Claude Fable, GPT Sol, and Gemini Neon:
- Claude Fable is a fresh background subagent on `fable`
- GPT Sol and Gemini Neon run through `mcp__delegate__run`, loaded once with `ToolSearch` `select:mcp__delegate__run`, each with its brief in a file
- Unreachable providers and unavailable named members are reported, never substituted

Commands follow provider order OpenAI, Google:

```json
{ "command": "codex exec --sandbox read-only -m gpt-6.2-sol -c model_reasoning_effort='\"xhigh\"' -o <answer> \"$(cat <brief>)\" </dev/null", "description": "council <round> sol", "limit": "90 minutes" }
{ "command": "agy -p \"$(cat <brief>)\" --disable-slash-commands --model gemini-4-neon-xhigh --output-format json --print-timeout 90m --settings '{\"permissions\":{\"deny\":[\"write_file(*)\"]}}' > <result.json>", "description": "council <round> neon", "limit": "100 minutes" }
```

`agy` answers in `.response` of the result file, and a report whose fields show an empty `response` or a `finished_reason` is a failed delegate whatever the exit code.

## [03]-[ROUNDS]

- Briefs hold the request and its context, keep a review or feedback run read-only, and tell the delegate to start no delegates of its own
- Code reviews ask each delegate for a score out of 10
- Rounds count full review passes, and a review after fixes starts a new round
- Reports are checked against the code as each arrives, and fixes are decided while the other delegates run
- Files stay unedited while a delegate still reviews the same checkout
- Delegates the owner marks non-blocking are not waited for, and a late result is feedback with no recorded score
- Findings, disagreements, and recommended next steps of every response are summarized together
- Recommendations this session agrees with are made, then a fresh round reviews them, until every delegate is satisfied
- Scope stays at the change: a finding outside it is no reason for another round

## [04]-[RESULTS]

Work ending in a pull request takes the results in its description, before the model and harness line, by editing the description and posting no comment, and work with no pull request takes them in the final reply alone:

```markdown
## Council review

Rounds: 2

| Delegate | Round 1 | Round 2 |
| --- | --- | --- |
| Claude Fable | 7/10 | 9/10 |
| GPT Sol | 8/10 | 10/10 |
| Gemini Neon | 7/10 | 9/10 |
```

Delegates that gave no score read `no score`.
