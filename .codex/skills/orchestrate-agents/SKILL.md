---
name: orchestrate-agents
description: "Use when running a fan-out, one job split across several sub-agents in parallel or in stages, covering split shape, independent verifiers, file hand-off, a progress ledger, and correction rounds. Not for one scout, one worker, or one reviewer"
---

# [ORCHESTRATE_AGENTS]

The parent's context holds decisions and file paths, each agent's context one unit.

## [01]-[SPLIT]

- Cohesion: one cohesive implementation or investigation, even across several files, goes to one sub-agent with the complete task; at least two useful independent workstreams justify splitting, dependencies or separate implementation and test outputs alone do not
- Batch: several small edits of the same kind across files are one dispatch listing every file and its change, reviewed as one diff; one dispatch per task is for work that needs its own judgment, its own tests, or its own review
- Packet: good packets are narrow, bounded, and evidence-based (`trace data flow from route to storage`, `add tests for one module`), bad packets are `fix the whole thing`, `review whatever changed`, `edit any files you need`
- Staging: each item moves through every stage on its own, its next stage dispatched when its result arrives, so wall-clock is the slowest single-item chain and not the sum of stages; hold a wave only when a stage needs all prior results at once (dedup or merge across the full set, a zero-count early exit, a prompt comparing one finding against the others)
- End: a fan-out may end at the fan-out; when results must stay raw, verbatim, or exactly structured, no synthesizer follows, since a free-text consolidator cannot be trusted to preserve exact paths and counts
- Convergence: a find-then-verify loop dedups against everything seen, not against what survived, or it never converges; every drop, cap, and truncation is reported, none silent

## [02]-[INDEPENDENCE]

- Angles: independent angles on the same question require fresh contexts; a context containing another angle's findings is not independent coverage
- Verifiers: separate agents framed to refute (`try to refute this finding; default to refuted if uncertain`); a producer checking itself controls nothing
- Answers: never pre-author an agent's committed answer; name the trap without answering it, since an injected defense turns the answer into an echo of the brief

## [03]-[HAND_OFF]

- Files: everything pasted into a brief, and everything a sub-agent prints back, stays resident in the parent's context for the rest of the session and is re-read on every later turn, so artifacts change hands as files
- Briefs: where findings are bulky the brief names the file they go to and the return wanted beside that path (verdict, counts, concerns); a domain skill's own return form stands
- Sequence: a later brief holds the interfaces and decisions from earlier units that it cannot know, never accumulated prior-unit summaries; a real session's dispatch hit 42k chars of which 99% was pasted history
- Ledger: conversation memory does not survive compaction, and parents that lost their place have re-dispatched entire completed sequences, the most expensive failure observed; a run of more than one wave tracks each unit (agent, owned files, report path, state) in one ledger file and trusts it over recollection

## [04]-[CORRECTION]

- Rounds: rounds 1-3 resume the original sub-agent with the open findings verbatim, its context intact; later rounds go to a fresh sub-agent with the report path and the findings, since a loop that survives three resumes usually means the sub-agent cannot see its own problem
- Evidence: after one evidence-free response send one focused retry, after a second replace the sub-agent
- Recheck: after findings, the original reviewer rechecks the fix; a full review repeats only on an escalation trigger
