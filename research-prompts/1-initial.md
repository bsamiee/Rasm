# [INITIAL_RESEARCH]

- `<TARGET>` = 
- `<README>` = 
- `<WAVES>` = 

`<README>` is the atlas: a set of implicit, skill-style laws — one page per layer of building Python — that lead an agent to generate dense, polymorphic, modern, library-first code. This work mines the verified, bleeding-edge material from which `<TARGET>` is authored: a deep reservoir of current, advanced findings, organized by lane.

[ORCHESTRATE]:
- Read `<README>` for the shared doctrine, the content standards, what each page owns, and the purpose of this research.
- With that grounding, study `<TARGET>`'s domain enough to clarify what it owns and to identify its lanes — its distinct, non-overlapping sub-concerns that together cover everything it governs, one of them always the external libraries it leans on. Right-size each lane so it can be mined across many passes without exhausting it or straying into another. This understanding is the orchestrator's only research; it writes no findings, and it stays within the doctrine and the content standards.
- Create `docs/stacks/python/.reports/<TARGET-slug>/` and a subfolder per lane.
- Dispatch one agent per lane folder, in parallel waves run in sequence across the `<WAVES>` passes.
- When the waves are done, stage and commit everything under `docs/`.

[WAVES]:
- One agent per lane per pass; agents within a pass run in parallel, and the passes run in sequence. The first pass establishes a lane's core; each later pass reads every prior file in its lane, takes its depth as the floor, and opens new facets without re-tread, staying inside the lane's concern and the target's domain.
- Each agent researches external truth only — current official docs, PEPs, and installed library source — verifies each finding, and writes a `NN-<slug>.md` in its lane folder.
- The same agent then runs its own loop on that file until it clears the bar: re-read and critique; strip every anti-pattern: no meta-commentary, no sourcing, no versions (assume/target newest only), and no coupling any other files or the repository; correct or cut old, wrong, or stale claims; consolidate near-identical lines into one finding that carries the combined value of all of them; and rework each remaining line by its grade — cutting low value findings (anything that is table-stakes), replacing it with more accurate material, and keeping only what is relevant to the lane.

[GRADE]:
- The agent scores its own file one to ten on four axes — richness, veracity, density, advancement — and on relevance to the lane, against a bar of 9.2 on each. Grades are honest; a real seven is reworked until it clears, and grading must be critical, harsh, but fair, not grading something a 9.2 out of convenience, or low for extreme nit-picking.

[DISCIPLINE]:
- Findings only — the rule, the mechanism, the API shape, the design pressure, the constraint, the gap, and the code that proves it. Say nothing about the file or the work. NEVER use meta commentary, "this is... "X is...", no sections that are not finding related, sourcing sections, no validation gate sections, no introduction sections, etc... just sectioned by logical categories based on research filled with only research findings, code blocks only if justified (and a high bar/standard).
- Assume the newest release of every tool and library; state current behavior, and never name, contrast, or replace an older version, default, or paradigm.
- Name no version, source, citation, path, or date; make no reference to another file, page, lane, or any repository name, path, or pattern. One H1, then `[GROUP_LABEL]:` groups of contiguous bullets with no blank line between items, and a blank line only around a fenced code block, with neutral placeholder names.
- Advanced and non-obvious only; a finding a strong engineer already holds is deepened until it is not, or cut.
