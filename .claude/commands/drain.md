---
description: Drain one planning corpus to zero — dispatch a single corpus-drainer coordinator, shepherd it to the consolidated receipt, audit the close on disk
argument-hint: <folder path or branch .planning tier; e.g. libs/csharp/Rasm.Bim>
---

# [DRAIN]

Finalize `TARGET: $ARGUMENTS` to zero open ideas, tasks, and research rows at redteam grade. One `corpus-drainer` agent owns the work end to end; this session orchestrates — census, dispatch, shepherd, audit — and never edits the target inline. Multiple targets run as parallel dispatches only when their write sets are disjoint; name every overlap as a carve.

## [01]-[CENSUS]

Run before dispatch and again at audit; the disk is the truth, receipts are claims:

```bash template
rg -c '^\[[A-Z_]+\]-\[(QUEUED|ACTIVE|BLOCKED)\]' <target>/IDEAS.md <target>/TASKLOG.md
rg -n '^(- )?\[[A-Z_]+\]-\[(OPEN|BLOCKED)\]:' <target>/.planning/ | rg -v 'TOKEN|source-only'
```

Record page count, `loc` total, catalog count, and the open-item roster — the dispatch brief carries them.

## [02]-[DISPATCH]

Dispatch ONE `corpus-drainer` agent (background) with: the target and its census; every carved surface a concurrent pass owns, file-exact; folder-bearing rulings and seam owners the census implicates; any acquisition or probe context already in hand. Mandate standards ride the agent definition — the brief adds target facts, never restates them, widening only for session-scoped directives (a raised bar, a scope addition, a peer handoff).

## [03]-[SHEPHERD]

- Relay every child receipt that leaks to this session back to the coordinator by SendMessage — substance summary with the child's raw agent id as the retrieval route; a leaked receipt is never harvested by proxy.
- On a session-limit or API kill, resume the coordinator with the recovery protocol: dead children are unharvested, audit the tree against the dispatch ledger, repair tears, re-dispatch.
- On a coordinator stop without its consolidated receipt, resume it naming the exact tail owed; never accept a child's report as the close.
- Route obligations a receipt hands upward — owed exposures, both-ends units, broken references — back to the live owner for immediate landing, never to a note or a later pass.

## [04]-[AUDIT]

Re-run `[01]` on the coordinator's return. Zero open items closes the target; a card outliving the pass carries a named observable no repo write can fire AND its estate-side half landed whole — anything short of that returns to the SAME agent with the gap named, never forward to a fresh pass. Spot-check fence balance on rebuilt pages and one seam mirror per touched seam.

## [05]-[CLOSE]

Report: the census delta, receipt highlights with premise corrections, surviving proof-carrying cards, ripples landed by folder, and the next target if one is queued.
