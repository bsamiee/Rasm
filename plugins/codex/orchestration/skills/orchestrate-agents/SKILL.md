---
name: orchestrate-agents
description: "Use when sending agents tasks in parallel or in stages, covering task boundaries, independent checks, spawns, messages, and interrupts"
---

# [ORCHESTRATION_ORCHESTRATE_AGENTS]

Send an agent a bounded task with its own context. Keep decisions and file paths in the parent. Use `code:orchestrate` for briefs, file owners, relays, the ledger, and returned work of agents split by file.

## [01]-[SPLIT]

- Cohesion: a cohesive implementation or investigation goes whole to one sub-agent across files, and independent workstreams alone justify a split
- Batch: small edits of one kind are one dispatch listing each file and its change, reviewed as one diff, and judgment work takes its own
- Packet: good packets are narrow, bounded, and evidence-based (`trace data flow from route to storage`, `add tests for one module`), bad packets are `fix the whole thing`, `review whatever changed`, `edit any files you need`
- Staging: each item moves through every stage alone, its next stage dispatched when its result arrives, so wall-clock time is its slowest chain
- Waves: a stage waits for every prior result only when it reads them all (a merge across the set, a zero-count exit, a comparison of findings)
- Results: collect agents' results directly when the task requires raw, verbatim, or exactly structured output
- Convergence: a find-then-verify loop dedups against every finding seen, never the survivors alone, or it never converges
- Drops: each drop, cap, and truncation is reported

## [02]-[INDEPENDENCE]

- Angles: independent angles on the same question require fresh contexts; a context containing another angle's findings is not independent coverage
- Checks: send a separate agent to try to refute a finding, and one it leaves in doubt joins a pool read and run again after the last result
- Answers: a brief names a trap without answering it, as an answer written into a brief comes back as an echo

## [03]-[HAND_OFF]

- Spawns: each pass, round, and role is a new `spawn_agent`, and work that follows from a final answer goes to a new sub-agent in a new brief
- Messages: a sub-agent tells `/root` through `send_message` when a finding changes the plan or another sub-agent's brief, then keeps working
- Interrupts: a running sub-agent whose brief became wrong gets `interrupt_agent`, and a new sub-agent takes the corrected brief
