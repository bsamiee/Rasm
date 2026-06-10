# [CONSOLIDATE]

- `<TARGET>` = 
- `<README>` = 

`<README>` is the atlas; read it for the shared doctrine and what `<TARGET>` owns. Consolidation folds each lane's cleaned files into one bedrock that holds every finding's value while removing duplication and contradiction; it adds no new research.

Run per lane folder, sequentially:
- Copy `01-<slug>.md` to `00-bedrock.md` as the base.
- One agent per remaining file, one at a time, integrates the next file into `00-bedrock.md`: it reads all of `00`, then merges the source in section by section, placing each finding where it belongs — extending a coherent group where the material fits, opening a new group where it is genuinely distinct, never grouping at random.
- Consolidate while integrating: where two or three lines say near the same thing, refactor them into one finding that carries the combined value of all of them, carefully, losing nothing.
- Resolve contradictions: where a new finding conflicts with one already in `00`, investigate both. If they are real nuances rather than an error, correct each line precisely so both read true, without duplicating; if one is wrong, keep only the correct one.
- Each integrating agent confirms its merge left no duplication or contradiction and clean categorization before handing off.
- When every file is integrated, delete `01-<slug>.md` through the last file, leaving only `00-bedrock.md`.

[DISCIPLINE]:
- The bedrock is findings only — logical category groups filled with findings, with no meta-commentary, sourcing, versions, dates, cross-references, or introduction, routing, summary, or validation section. One H1, then `[GROUP_LABEL]:` groups of contiguous bullets with no blank line between items, and a blank line only around a fenced code block.
