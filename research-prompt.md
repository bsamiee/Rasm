# [RESEARCH_PIPELINE]

A parameterized research pipeline that mines a dense, verified body of doctrine for one new `docs/stacks/` concept page, then distills it into card-grade material. Fill the three parameters, then run one stage at a time. Each stage is a self-contained task: it states its own inputs, work, and output, and depends only on these parameters and the standing rules — never on having read the other stages.

[PARAMS]:
- `<TARGET>`: the concept page to build, e.g. `surfaces-and-dispatch.md`.
- `<SEED>`: the atlas section that already maps the page — its `[SECTIONS]`, `[EXTERNAL_LIBS]`, and `[ACCEPTANCE]`, e.g. `README.md [5]-[SURFACES_DISPATCH_TARGET]`. The seed is authored by hand before the pipeline; it is the domain decomposition the pipeline mines, never a thing the pipeline invents.
- `<WAVES>`: deepening waves per axis. Default 5.

[PATHS]:
- Working root: `docs/stacks/python/.reports/<TARGET-slug>/`.
- One sub-folder per axis: `.../<axis-slug>/`.
- Wave artifact: `<axis-slug>/NN-slug.md`, where `NN` is the wave number and `slug` is chosen by the agent from its own coverage.

Stage order, run one at a time: `[1] Initial -> [2] Hygiene -> [3] Consolidate -> [4] Refine bedrock -> [5] Divergent -> [6] Refine divergent -> [7] Distill`.

## [STANDING_RULES]

Every stage inherits these. State them to each sub-agent; do not weaken them per stage.

[ARTIFACT]:
- One H1, then spaced single-line bullet findings — one durable finding per bullet.
- Pure findings only: the rule, the verified mechanism, the API shape, the design pressure, the integration constraint, the open gap.
- No meta-commentary, no `this file is`, no `this research`, no role labels, no wave or process narration, no validation ceremony.
- Never cross-reference another file, page, or axis; each artifact reads as one self-contained body.
- Cite the source inline on the finding it backs: a URL or an installed path, plus the date read. A finding with no current source is removed.
- Code only where an advanced pattern cannot be carried in prose; keep it minimal, neutral placeholder names (`Shape`, `Variant`, `Field`, `Row`, `TABLE`, `SELECTED`), never project, product, or workflow nouns.

[VERIFICATION]:
- Current sources only. Freshness-sensitive facts — library versions and APIs, recent PEP status — require sources from the last 3-4 months; read the installed source or official docs and never assert an API from memory.
- Verify every symbol, signature, and behavioral claim against the installed package or primary spec before recording it. A claim that fails verification is deleted, not softened.

[GRADING]:
- Four axes, scored 1-10: richness (depth and coverage of the facet), veracity (every claim verified against a current source), density (signal per line, no filler), advancement (beyond table-stakes — the rich, non-obvious, high-order material). In a refining stage the same axes measure preserved-and-sharpened signal rather than new depth; the names and the bar do not change.
- Bar: an artifact is accepted only when all four axes clear 9.2. Grades are honest — a real 7 is recorded as 7 and re-worked, never inflated to pass.
- First pass, producer self-review: before reporting done, the producing or refining agent re-reads its artifact cold, scores the four axes, names the weakest finding on each, and re-works — adding verified depth, cutting table-stakes, tightening prose — until all four clear 9.2. Truthful improvements only; never bend a finding's meaning to raise a score.
- Second pass, orchestrator fresh-eyes review: after the agent reports done, the orchestrator reads the artifact cold, gives no weight to the agent's self-grade, and re-scores the four axes for a real analysis. Any axis under 9.2 returns the artifact with the specific defect named for one targeted revision, repeating until it clears. The orchestrator's grade is the gate.

---

# [1]-[INITIAL_RESEARCH]

Mine the first dense body of findings for `<TARGET>`, one axis at a time, deepening across `<WAVES>`. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- `<SEED>` exists and maps the page. No research artifacts exist yet.

[PRE_DISPATCH]:
- Read `<SEED>`. Its `[SECTIONS]` are the research axes — create one `docs/stacks/python/.reports/<TARGET-slug>/<axis-slug>/` folder per section. Split a first-class library in `[EXTERNAL_LIBS]` into its own axis when it owns a distinct surface deep enough for a dedicated body; fold a section that is a derived synthesis rather than a research domain into the axis it derives from. The axis count is whatever the seed decomposes into.
- You are the orchestrator, not a worker. After folders exist, your task is dispatching waves and gating quality.

[DISPATCH]:
- Wave 1: one agent per axis. Each mines high-density findings for its axis only, grounded in `<SEED>`'s intent for that section, and writes `01-slug.md`. When verified prior research for an axis already exists, deposit it as `01-slug.md` cleaned to [ARTIFACT] and begin deepening at wave 2.
- Waves 2 through `<WAVES>`: same task, one difference — each agent first reads every prior artifact in its own axis folder, then pushes past them into deeper concepts, further APIs, and higher-order doctrine without re-treading covered ground, writing `NN-slug.md`.
- [GRADING] gates every artifact: producer self-review to 9.2 on all four axes, then orchestrator fresh-eyes review before the artifact is accepted.

[GOAL]:
- A cohesive, high-density body for the `<TARGET>` domain: how its surfaces are selected, constructed, composed, and integrated; which admitted library owns which concern; where the design pressure and the unresolved gaps are. Positive doctrine over reject lists — convert every weak form into a stronger owner-selection finding.

---

# [2]-[HYGIENE]

Clean and harden every artifact in place. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds wave artifacts `NN-slug.md` from stage 1.

[TASK]:
- Dispatch one agent per `NN-slug.md` file; each owns one file and reads it line by line.
- Strip meta-commentary, cross-references, and `this is` framing; keep the source tag on each finding.
- Reorganize into logical sections; group like and adjacent findings; rename a section only when the new name names real shared content.
- De-duplicate: merge overlapping or near-overlapping findings into one entry carrying the union of their signal, then delete the duplicates.
- Validate every finding against a current source: delete what is false, fix what is wrong in detail, sharpen what is unclear.
- Cut outdated, low-value, and dead-simple table-stakes content.
- [GRADING] gates the cleaned file: producer self-review to 9.2, then orchestrator fresh-eyes review.

---

# [3]-[CONSOLIDATE]

Fold each axis folder's waves into one bedrock file. Run per folder; integrate sequentially within a folder. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds cleaned wave artifacts `01-slug.md` through the last wave.

[TASK]:
- Copy `01-slug.md` to `00-bedrock.md` as the base.
- One agent at a time, integrate the next wave into `00-bedrock.md`: read all of `00`, read the next wave file, merge its findings into `00` category by category, de-duplicating against what `00` already holds. Repeat for each remaining wave — always one agent, one source file, against the growing `00`.
- One agent then reads all of `00-bedrock.md` and improves organization: logical categories with no arbitrary grouping, all items in a category adjacent with no gaps, clean structure, minor wording fixes only.
- [GRADING] gates the reorganized `00-bedrock.md`: producer self-review to 9.2, then orchestrator fresh-eyes review.
- Delete `01-slug.md` through the last wave file; keep only `00-bedrock.md`.

[COMMIT]:
- Stage and commit the `docs/` folder.

---

# [4]-[REFINE_BEDROCK]

Sharpen and trim each bedrock. One agent per axis folder, in parallel. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds `00-bedrock.md`.

[TASK]:
- Read all of `00-bedrock.md`.
- Remove chaff irrelevant to advanced, universal-language doctrine and to this monorepo's surfaces — drop platform-specific noise that does not serve the standard.
- Refine every section's prose surgically using `docs/standards/style-guide.md` as law: front-load the controlling rule, cut non-load-bearing hedges, one term per concept, code-safe notation. Improve quality truthfully; never alter a finding's meaning to shorten it.
- Reduce length by 3 to 10 percent through tighter prose and de-duplication only — never by dropping a real finding.
- Examine the categories: confirm they fit the axis domain, are not arbitrary, and are named for real shared content; reorganize or rename where the structure is weak.
- [GRADING] gates the refined bedrock: producer self-review to 9.2, then orchestrator fresh-eyes review, with a final check that no outdated, table-stakes, or false content remains.

[COMMIT]:
- Stage and commit the `docs/` folder.

---

# [5]-[DIVERGENT_RESEARCH]

Build higher-order concepts on the bedrock, one axis at a time, deepening across `<WAVES>`. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds `00-bedrock.md`.

[TASK]:
- Each agent reads all of `00-bedrock.md` for its axis as a foundation of established knowledge — to understand the breadth of the domain, not as the basis of its approach.
- It then chooses a direction, facet, or angle freely and builds a markedly more advanced, higher-order body than the bedrock holds, without re-iterating bedrock findings. Each subsequent agent is free to diverge into its own direction rather than refining a prior one — the goal is a rich spread of distinct high-order concepts.
- Waves 2 through `<WAVES>` read prior divergent artifacts in their folder to avoid re-tread and push further. Write `NN-slug.md` alongside the bedrock.
- [GRADING] gates every artifact: producer self-review to 9.2, then orchestrator fresh-eyes review.

---

# [6]-[REFINE_DIVERGENT]

Refine and push the divergent artifacts. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds `00-bedrock.md` and divergent wave artifacts `NN-slug.md`.

[TASK]:
- Dispatch one agent per divergent `NN-slug.md` file.
- Run the full stage-2 hygiene on the file: strip, reorganize, group, de-duplicate, validate, trim.
- Then push further: where the artifact stops at a known level, extend it to a higher-order concept the domain supports.
- Grade the artifact's value against `00-bedrock.md`: cut anything that merely restates bedrock or scores table-stakes; keep only what advances beyond it.
- [GRADING] gates the result: producer self-review to 9.2, then orchestrator fresh-eyes review.

---

# [7]-[DISTILL]

Distill one axis folder into a single card-grade approach. Run per folder. Apply [PARAMS] and [STANDING_RULES].

[INPUT]:
- Each axis folder holds `00-bedrock.md` and refined divergent artifacts `NN-slug.md`.

[TASK]:
- Read every divergent artifact in the folder, excluding `00-bedrock.md`.
- Synthesize one distilled body that selects and orders the strongest high-order concepts into a coherent approach for `<TARGET>` — the cards worth making and the depth each should carry.
- Read `00-bedrock.md` last and only to pull in primitive or foundational material the distillate still needs; do not let the bedrock dilute the higher-order selection.
- Write the result to `00-distilled.md` at the `<TARGET-slug>` working root, applying [ARTIFACT] in full.
- [GRADING] gates the distillate: producer self-review to 9.2, then orchestrator fresh-eyes review.

The distillate is the mining source for authoring `<TARGET>` — a large reservoir of verified, high-order material from which the page's cards are selected. Much will not reach the page; that surplus is the point.
