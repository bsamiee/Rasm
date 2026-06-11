# [HYGIENE]

- `<TARGET>` =
- `<DOCTRINE>` =
- `<WORKSPACE>` =

Hygiene brings every research file in `<WORKSPACE>` to the bar. One agent per lane folder; each owns every `NN-<slug>.md` in its folder and reads them line by line.

[DOCTRINE]:
- Every agent — the orchestrator and every wave agent — fully reads `<DOCTRINE>` before any other work: the laws, the collapse scan, the page craft, and what `<TARGET>` owns. That single read is the whole alignment requirement, and every finding, edit, and grade answers to it.
- Every agent fully reads `docs/standards/style-guide.md` and applies its craft to the prose itself.

[CLEAN]:
- Strip every line that is not a finding — meta-commentary, "this is..." or "X is..." framing, any sourcing (external links, package or file paths, version numbers, dates), coupling to another file or the repository, and any introduction, routing, summary, sources, or validation section. Keep only logical category groups filled with findings.
- Fix formatting: one H1, then `[GROUP_LABEL]:` groups of contiguous bullets with no blank line between items, and a blank line only around a fenced code block. Group like and adjacent findings; rename a group only when the new name names real shared content.
- Consolidate: where findings overlap, fold them into one entry that carries the combined value of all of them.

[PROSE]:
- Apply the style guide surgically: front-load the controlling rule, tighten every sentence, cut filler and non-load-bearing hedges, one term per concept — never changing a finding's meaning to shorten it.
- Trim 3-10% of the content where the trim costs no finding value. Careful and surgical; nothing major, nothing structural.
- These files feed a lane-wide merge next: distinct nuance survives every trim; only prose fat leaves.

[VALIDATE_AND_DEEPEN]:
- Verify every claim against the newest source. Delete what is false or stale, correct what is wrong in detail, and sharpen what is unclear — never a blind cut.
- Dig a table-stakes finding deeper until it carries advanced value, or remove it when nothing deeper is there. Build on the file slightly; do not re-run the research.

[GRADE]:
- Grade every file against the research ladder in `_grading.md` at the reports root — the minimum across its axes clears the stage bar, automatic fails override any score, and the drift checks apply. Rework until every file clears.

[COMMIT]: stage and commit `<WORKSPACE>`.
