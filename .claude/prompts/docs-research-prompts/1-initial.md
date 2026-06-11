# [INITIAL_RESEARCH]

- `<TARGET>` =
- `<DOCTRINE>` =
- `<WORKSPACE>` =
- `<WAVES>` =

This work mines the verified, bleeding-edge material from which `<TARGET>` is authored: a deep reservoir of current, advanced findings, organized by lane.

[DOCTRINE]:
- Every agent — the orchestrator and every wave agent — fully reads `<DOCTRINE>` before any other work: the laws, the collapse scan, the page craft, and what `<TARGET>` owns. That single read is the whole alignment requirement, and every finding, edit, and grade answers to it.

[ORCHESTRATE]:
- With that grounding, study `<TARGET>`'s domain enough to clarify what it owns and to identify its lanes — distinct, non-overlapping sub-concerns that together cover everything it governs. Right-size each lane so it can be mined across many passes without exhausting it or straying into another.
- `<TARGET>`'s scope comes from `<DOCTRINE>`'s atlas and roadmap, never from the page itself: when `<TARGET>` already exists on disk, neither the orchestrator nor any wave agent reads it — the research stays fresh and outside-in.
- Enumerate every admitted external package relevant to `<TARGET>` from the manifests the doctrine names, and give that integration surface its own lane — or lanes when the package set is wide. No admitted package is left out.
- This understanding is the orchestrator's only research; it writes no findings.
- Create `<WORKSPACE>` and a subfolder per lane.
- Dispatch one agent per lane folder, in parallel waves run in sequence across the `<WAVES>` passes.

[WAVES]:
- One agent per lane per pass; agents within a pass run in parallel, and the passes run in sequence. The first pass establishes a lane's core; each later pass reads every prior file in its lane, takes its depth as the floor, and opens new facets without re-tread, staying inside the lane's concern and the target's domain.
- Each agent researches external truth only — current official docs, language standards and proposals, and installed library source — verifies each finding, and writes a `NN-<slug>.md` in its lane folder.
- The same agent then grades its file, and runs its own loop on it until it clears the bar: re-read and critique; strip every anti-pattern: no meta-commentary, no sourcing, no versions (assume and target newest only), and no coupling to any other file or the repository; correct or cut old, wrong, or stale claims; consolidate near-identical lines into one finding that carries the combined value of all of them; and rework each remaining line by its grade — cutting low-value findings (anything table-stakes), replacing them with more accurate material, and keeping only what is relevant to the lane.

[GRADE]:
- The agent scores its own file one to ten on four axes — richness, veracity, density, advancement — and on relevance to the lane, against a bar of 9.2 on each. Grades are honest; a real seven is reworked until it clears, and grading is critical, harsh, but fair — not a 9.2 out of convenience, not low for extreme nit-picking.

[DISCIPLINE]:
- Findings only — the rule, the mechanism, the API shape, the design pressure, the constraint, the gap, and the code that proves it. Say nothing about the file or the work; never use meta-commentary, "this is..." or "X is..." framing, or any introduction, routing, summary, sources, or validation section — just logical category groups filled with research findings, with code blocks only where justified against a high bar.
- Assume the newest release of every tool and library; state current behavior, and never name, contrast, or replace an older version, default, or paradigm.
- Name no version, source, citation, path, or date; make no reference to another file, page, lane, or any repository name, path, or pattern. One H1, then `[GROUP_LABEL]:` groups of contiguous bullets with no blank line between items, and a blank line only around a fenced code block, with neutral placeholder names.
- Advanced and non-obvious only; a finding a strong engineer already holds is deepened until it is not, or cut.
- The lane series is mined far past what one page can print: later stages consolidate, distill, enrich, and verify code against it, so depth beyond the page is the deliverable, never waste.
- External truth never overrides the doctrine: a verified finding that opposes a law enters only as the constraint or boundary it imposes, or is cut — the doctrine selects among truths and is never graded by them.

[COMMIT]: stage and commit `<WORKSPACE>`.
