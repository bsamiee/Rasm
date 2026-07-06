# [AXES]

The interrogation-axis catalog: where unknown unknowns hide and how each is hunted. Every grounding pass selects the axes the request touches, runs each axis's probe, verifies every candidate on disk, and records survivors as confrontation-set entries. An axis that comes back clean is recorded clean; absence of findings is a finding.

Each axis carries one card: Definition (what the pathology is), Probe (what to read or sweep), Question (the shape that exposes it to the user), Evidence (what confirms a hit before it enters a question). The section label is the axis token — it names the axis in confrontation-set entries and in blindspot-ledger leaders.

[CONFRONTATION_SET] — The grounding product is a flat entry list, one row per verified finding:

```markdown
- [<AXIS>]: <path>:<line> — <the-observed-fact-in-one-clause>
```

[SCALING] — A corpus past one reading budget partitions into read-only legs by subtree at unit grain, each leg carrying the full selected axis set over its subtree; a leg reads its subtree's index docs in full and samples implementation pages, returns confrontation-set rows plus its clean-axis list, and the interviewer merges, dedupes, and re-verifies any row it embeds in a question.

## [01]-[EXTENDABILITY]

- Definition: Growth the domain will demand that no surface reserves — the next case, modality, provider, or scale step lands as a break instead of a row.
- Probe: Read the owner's case set and ask what the domain carries beyond it; sweep growth or deferred markers and test whether each names a real extension operator or only prose.
- Question: The domain carries `<future-case>`; which row, case, or policy value absorbs it, and what breaks when it arrives.
- Evidence: An extension scenario with no landing surface, or a growth note with no operator behind it.

## [02]-[CAPABILITY_WEAKNESS]

- Definition: An owner modeling a thin slice of its concept — few fields or cases where the domain is rich — or an index claiming capability its pages do not carry.
- Probe: Sample the owner against its domain's full attribute and state space; cross-check every index-tier capability claim against the page and evidence catalog that must carry it.
- Question: The owner carries `<n>` cases where the domain carries `<m>`; which of the missing are refused by design and which are gaps.
- Evidence: A claim-to-carriage mismatch anchored on both sides, or an owner field set visibly below its concept's known shape.

## [03]-[STRUCTURE]

- Definition: Architecture fighting itself — parallel near-duplicate rails, one concern owned twice, split-brain designs where siblings solve the same concept differently.
- Probe: Search for sibling surfaces sharing a concept vocabulary; compare their case sets and entry shapes; sweep for duplicate-marker and retirement-scar language.
- Question: `<surface-a>` and `<surface-b>` both own `<concern>`; which is the canonical owner and what folds the other in.
- Evidence: Two verified surfaces carrying the same concern with divergent shapes.

## [04]-[FOLDER_TOPOLOGY]

- Definition: Structure pathologies of the tree itself — one-folder-one-file chains, over-nesting with single children, one concern scattered across many small siblings, flat dumps past navigability.
- Probe: Sweep the tree for single-child directories and single-file folders; count files per directory and flag flat sets past the point where naming convention is the only index.
- Question: `<folder>` holds one file its parent map already names; which rule decides folder against section, and what does the extra level buy.
- Evidence: The tree listing itself — counts and chains verified by a fresh sweep.

## [05]-[APPROACH]

- Definition: A design that fights the grain — hand-rolling what an admitted dependency owns, unusual mechanisms where the ecosystem has a native one, entry proliferation over one polymorphic surface.
- Probe: For each mechanism, ask which admitted dependency or platform primitive owns the concern; sweep for multiple entry points over one stated aspect.
- Question: `<surface>` hand-carries `<concern>` that `<dependency>` owns; what does the local mechanism add that the native one refuses.
- Evidence: A verified dependency member covering the hand-rolled ground, or three entry names over one declared aspect.

## [06]-[NAMING]

- Definition: A name carrying one or two semantic values where the content carries many — generic tokens, tautological file names, names that do not predict content.
- Probe: Sweep for the generic-name roster (`core`, `common`, `utils`, `helpers`, `base`, `shared`, `data`, `info`, `manager`, `index`, `misc`, `plan`); compare each hit's name against its first owner sentence.
- Question: An agent hunting `<capability>` reads the tree; does `<name>` ever get chosen, and what name predicts the content it hides.
- Evidence: A name-to-content mismatch verified by reading the file's own charter line.

## [07]-[LONG_TERM]

- Definition: Lifecycle ground no page reserves — versioning, migration, deprecation, retention, multi-host reach, scale steps — that arrives later as a surface break.
- Probe: For each owner, walk the lifecycle stages its concept carries and mark which have a reserved surface; sweep closed-count declarations for missing escape paths.
- Question: When `<lifecycle-event>` arrives, which surface absorbs it; what is the reserved path for cases that do not fit the current set.
- Evidence: A lifecycle stage with no owner anywhere in the corpus, verified by axis-scoped search.

## [08]-[COUNTERFACTUAL]

- Definition: The unexamined inverse — what failure looks like with the goal met, what happens when the unlikely event fires, which trigger chains nobody mapped.
- Probe: Take each settled judgment and assume its negation happened; build the backward pathway and name the observables that mark each stage.
- Question: The goal is met and the result still counts as failure; what happened.
- Evidence: A plausible pathway to the negated outcome that no stated constraint blocks.

## [09]-[OVERSIGHT]

- Definition: Skipped stages and unnamed parties — teardown, recovery, observation, migration nobody mentions; consumers the request never names.
- Probe: Walk the standard stage roster (create, operate, observe, recover, migrate, retire) against the request; enumerate consumers from the seam registers and diff against the named set.
- Question: `<stage>` is absent from the request; refused or forgotten.
- Evidence: A stage or consumer present in the corpus's own registers that the request omits.

## [10]-[SCOPE_NAIVETY]

- Definition: Goals assuming without stating — an environment always present, a single user, a happy path, an unnamed dependency's availability.
- Probe: For each goal sentence, extract what must be true for it to hold; sweep for external dependencies carried by convention rather than by declared contract.
- Question: `<goal>` holds only while `<condition>` does; what is the story when it fails.
- Evidence: A must-be-true condition with no declared owner, fallback, or refusal.

## [11]-[ASSUMPTION]

- Definition: Unstated premises under settled judgments — the roster a key-assumptions check produces, ranked by importance against evidence.
- Probe: Write the current line; articulate its stated and unstated premises; place each on the importance-by-evidence quadrant; the high-importance low-evidence corner is the question queue.
- Question: The design rests on `<premise>`; what evidence carries it, and does the judgment survive its failure.
- Evidence: A load-bearing premise whose supporting evidence cannot be produced on request.

## [12]-[TOUCH_POINT]

- Definition: Incomplete seams — a contract declared on one side only, a consumer without a provider page, an evidence catalog referenced but absent or stale.
- Probe: For every declared seam edge, open both endpoints and verify the reciprocal declaration; for every capability claim naming a dependency, verify the adjacent evidence entry exists and matches.
- Question: `<side-a>` declares the seam; `<side-b>` does not know it exists — which side owns the contract and what lands the missing half.
- Evidence: A one-sided declaration verified by opening both ends.
