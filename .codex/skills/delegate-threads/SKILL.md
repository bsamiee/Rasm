---
name: delegate-threads
description: "Use when work splits into pieces that each end in their own deliverable or should keep running after this turn, covering Codex app threads: when a thread beats a sub-agent, the prompt a thread starts from, cloud and review threads, and a heartbeat that advances a queue"
---

# [DELEGATE_THREADS]

A sub-agent answers a question this turn consumes. A thread is a separate chat with its own context and lifetime: it owns one deliverable, the owner can open and steer it, and it outlives this turn. This skill is the owner's standing request to create threads and to message the threads this chat created, so the thread tools' "only when the user asks" condition is met whenever it applies

## [01]-[BEFORE_DISPATCH]

- Investigate here first: the split comes from what the code shows, and a thread started from a guess repeats the investigation
- Decide the shape out loud: one piece or several, and for several whether each stands alone or builds on the last
- Write each piece's plan to a file, the thread reads the file and the chat keeps only its path
- One coherent change stays in this chat however large, a thread is for a second deliverable

## [02]-[PROMPT]

The prompt is the thread's first user message and all it knows: the outcome and why it matters, the plan file path, what is already decided, and what done means.

## [03]-[TARGET]

Words in the request choose the target, and the default is a local thread in the project:

| [INDEX] | [REQUEST]                 | [TARGET]                                                           |
| :-----: | :------------------------ | :----------------------------------------------------------------- |
|  [01]   | `cloud`                   | `codexCloud` with an id from `list_cloud_environments`             |
|  [02]   | `review`                  | A fresh thread per review, given the finished work and requirement |
|  [03]   | A named model or provider | That `model` and `thinking` on the thread                          |

A review thread has not seen the author's reasoning, which is its value: it gets the work and the requirement, never the author thread's summary.

## [04]-[FOLLOWING]

This chat follows its threads by their state, and each result is read against the work itself before the next piece is dispatched.

`loop`, `overnight`, or a queue longer than this turn: attach one heartbeat to this chat and design what each wake does from this job's own pieces (what it checks, what earns a review thread, what starts the next piece, when the queue is done), then delete the heartbeat when it is. The number of threads is the number of independent pieces, never a target to fill.
