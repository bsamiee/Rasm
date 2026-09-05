---
name: post-refactor-review
description: Use after a proven rebuild, refactor, or hard fix in any language, covering patterns into ast-grep rules, rule hardening, skill, reference, and agent improvement.
---

# [POST_REFACTOR_REVIEW]

Run the review after a large pass landed with proof, while the diff, the research, and the corrections are in context, and end with a stronger rule set, a skill and agents that would have produced the fix sooner, and a codebase with every finding acted on. Use `ast-grep` for rule mechanics and proofs, `clean-prose` for every line written, and the memories for the placement, rebuild, dispatch, and reduction standards each edit meets.

[REFERENCES]:
- [01]-[DEEP_RESEARCH](references/deep-research.md): The gathering sequence a step of the review runs when a domain decides the rules and the skill

## [01]-[DIGEST]

Before the first agent starts, write one digest file in the scratchpad from the session, with a table of each pattern corrected as a higher-order pattern with its instance and correction, a table of each library fact verified with its source, and a table of each smell seen and left in place. A fork carries the digest in context, and a fresh agent gets its path.

## [02]-[SEQUENCE]

A fork carries the session and cannot dispatch agents, so forks implement and fresh agents research, verify, and attack. Each step lands with proof before the next starts, and a cold pass by a fresh agent separates any two steps that build rules:

| [INDEX] | [STEP]                 | [AGENT]      | [WORK]                                                                                      |
| :-----: | :--------------------- | :----------- | :------------------------------------------------------------------------------------------ |
|  [01]   | Configuration          | Fork         | Tool config, the rules tree the `ast-grep` skill lays out, one rule proving the pipeline    |
|  [02]   | Skill and rules, cold  | Fresh        | Research through gathering agents, the skill rebuilt, rules rebuilt into families           |
|  [03]   | Rules from the session | Fork         | Code the rules catch fixed, then one rule per digest pattern with its node shape            |
|  [04]   | Verification, cold     | Fresh        | Each rule against the criteria, near-duplicate rules collapsed, dead rules deleted          |
|  [05]   | Rules from the code    | Fork         | Smells no checker catches, the code fixed first, then one rule per proven pattern           |
|  [06]   | Wide pass, cold        | Fresh, forks | Skill against the full research, then a reference and an agent per area, each attacked      |
|  [07]   | Consolidation, cold    | Fresh        | Mistakes, duplicates, filler, negative framing, and guidance that drives no behavior        |
|  [08]   | Prose, cold            | Fresh        | Terminology and prose, then the description and its routing line by the description process |
|  [09]   | Final checks, cold     | Fresh        | Every check run, listing lines measured, the memories rewritten against the skill           |

In the wide pass, a fresh agent rebuilds the skill against the full research, forks write a reference and an agent per area, and a fresh agent attacks each pair. Research integrates into the skill in the same session as soon as its files are free, and ownership passes to the integrating agent.

## [03]-[TRACKS]

Sibling folders of `references/` (`assets/`, `scripts/`, tests) run as parallel tracks outside the sequence, and neither side waits on the other. One track covers every folder an agent reads whole in one pass, and a folder past that gets its own track. A track is an agent then a fresh adversarial agent, both reading the whole skill, at two or at most three checkpoints, each after a step that changed the skill content, and the track owns its folder and the listing lines that name its files.

## [04]-[DISPATCH]

- A brief names the files to read in order, the ownership, the standards, the proof, and a report of at most 15 lines
- Parallel agents own disjoint files and send a finding on another agent's file to that agent, or to `main`, without `ListAgents`
- `main` is the bus, relays each finding to its owner as it arrives, and sends a focused fresh agent for a real finding no agent owns
- An area without a reference and an agent gets both, the reference for the sequenced work beyond the skill and the agent for its procedure

## [05]-[CRITERIA]

- A rule passes the `ast-grep` qualifying proofs, and a collapse into a stronger pattern merges the tests and utils and deletes the superseded ids
- A code fix meets the reduction bar, adds no indirection, throw, drop, or deferral, and passes every checker the codebase runs
- An improvement to a skill, reference, agent, or memory rebuilds the owning section under the placement and rebuild standards
- An agent reports a gap in its own agent file or in the skill as the principle or the poor guidance to correct, and the fix rebuilds the section
- A memory related to the work, directly or tangentially, joins the set the consolidation pass and the close improve under the same criteria
- A memory a skill now records goes, one with steps the skills no longer match is rewritten once, and each stays a behavior trigger, no journal
- A new example or template enters when the work exposes a category the skill teaches and no asset shows, on placeholder names, never a copied rule

## [06]-[CLOSE]

Run `git diff --stat` over every touched path and read the diff of a large pass in full. Run every declared check: the `ast-grep` gate, the lint targets, the compiler, and a width check over every listing line, table row, and memory index line. Close when each finding any agent reported landed or has a focused agent on it, and each fact across the skills and the related memories sits with one owner.
