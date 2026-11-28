---
name: council
description: Trigger this if the user says 'council'.
---

# [DELEGATE_COUNCIL]

Before an audit, address outstanding PR comments, fix known issues, and run relevant checks. Give council the updated implementation; do not defer known fixes until reviews return. If the user wants to discard the implementation and explore new ideas, skip fixes to code being replaced.

Bring in one delegate per provider, usually for review or feedback, each a strong model at high reasoning effort. If the user's instructions name council members, use those. Otherwise the members are Claude Fable, GPT Sol, and Gemini Neon.

Your own provider's delegate is a fresh background subagent on `fable`. Reach the OpenAI and Google members through `mcp__delegate__run`, loaded once with `ToolSearch` `select:mcp__delegate__run`, with the brief written to a file. Each call answers at once with its runner agent's id, and the run's report arrives as that agent's notice. Commands follow provider order OpenAI, Google:

```json
{ "command": "codex exec --sandbox read-only -m gpt-6.2-sol -c model_reasoning_effort='\"xhigh\"' -o <answer> \"$(cat <brief>)\" </dev/null", "description": "council <round> sol", "limit": "90 minutes" }
{ "command": "agy -p \"$(cat <brief>)\" --disable-slash-commands --model gemini-4-neon-xhigh --output-format json --print-timeout 90m --settings '{\"permissions\":{\"deny\":[\"write_file(*)\"]}}' > <result.json>", "description": "council <round> neon", "limit": "100 minutes" }
```

`agy` answers in `.response` of the result file: a report whose fields show an empty `response` or a `finished_reason` is a failed delegate whatever the exit code. If a provider is unreachable, or a named member is unavailable, report it instead of substituting.

Give each delegate the request and relevant context. For review or feedback, keep their work read-only. Tell each delegate to start no delegates of its own. Collect their responses and summarize the useful findings, disagreements, and recommended next steps.

Do not wait idle. When a report arrives, check its findings against the code at once and decide what to fix while the other delegates run. Do not edit files while a delegate still reviews the same checkout. If the user's instructions mark a delegate as non-blocking, do not wait for it; treat a late result as feedback and do not record its score.

For a code review, ask each delegate for a score out of 10. Count each full review pass as one round. A fresh review after fixes starts a new round.

If the work ends in a pull request, put the results in the PR description. Put this section before the model and harness line at the end:

```markdown
## Council review

Rounds: 2

| Delegate | Round 1 | Round 2 |
| --- | --- | --- |
| Claude Fable | 7/10 | 9/10 |
| GPT Sol | 8/10 | 10/10 |
| Gemini Neon | 7/10 | 9/10 |
```

If a delegate gives no score, write `no score`. Do not invent one. If the PR is open, edit the description. Do not post a comment for this. If there is no PR, put the same results in the final reply only.

If you agree with their recommendations, make the changes and request a fresh review after. Continue this process until all delegates are satisfied with the outcome.

Do not let repeated reviews result in scope creep. It is your job to fight to keep scope limited.
