# [CODEX_PROMPTING]

Codex is maximally literal: it follows a clean contract with surgical precision and burns reasoning reconciling a contradictory one. Every dispatch carries four things — Goal, Context, Constraints, Done-when — split across two channels: durable lane law rides `developer-instructions` (MCP) / `-c developer_instructions=` (CLI) as named XML spec blocks with the output contract LAST; the user `prompt` carries only the task instance plus any imperative spawn step. Leaner prompts outperform — every directive earns its slot, one directive per concern, and a directive codex already ships is a conflict risk, not reinforcement.

## [01]-[BLOCK_VOCABULARY]

One canonical block per concern; each lane selects its rows in this order.

| [INDEX] | [BLOCK]                          | [JOB]                                          | [RECON] | [WRITE] | [JUDGMENT] |
| :-----: | :------------------------------- | :--------------------------------------------- | :-----: | :-----: | :--------: |
|  [01]   | `<role>`                         | identity, exact territory, hard exclusions     |   opt   |   yes   |    yes     |
|  [02]   | `<completion_bar>`               | done-definition per move, defer-on-blocker     |    —    |   yes   |    yes     |
|  [03]   | `<context_gathering>`            | read ladder, total-call budget, escape hatch   |   yes   |   yes   |    yes     |
|  [04]   | `<decision_procedure>`           | refute-first adjudication                      |    —    |    —    |    yes     |
|  [05]   | `<design_and_scope_constraints>` | exactly-and-only scope, settled rulings        |    —    |   yes   |    yes     |
|  [06]   | `<capability_mandate>`           | opt-in surface-raising replacement for [05]    |    —    |   opt   |    opt     |
|  [07]   | `<verification>`                 | post-edit re-read, truth rail, format gate     |   yes   |   yes   |    yes     |
|  [08]   | `<output_contract>`              | JSON-only shape, null-for-missing, always LAST |   yes   |   yes   |    yes     |

`<role>` on a judgment lane adds the findings-are-untrusted clause; `<completion_bar>` sits early, where it beats codex early-stop; `<verification>` extends with a cite-check on recon and a rubric walk on judgment. `<design_and_scope_constraints>` and `<capability_mandate>` are mutually exclusive: constraint is the default, the mandate is earned by a declared corpus-improvement campaign and states every expansion move as a measurable condition — figurative targets make a literal model over-engineer toward phantoms.

## [02]-[LANE_SHAPES]

| [INDEX] | [SHAPE]              | [SANDBOX]       | [DISTINCT_CLAUSES]                                                                        |
| :-----: | :------------------- | :-------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | recon / census       | read-only       | returns a MAP, never a verdict; coverage object is the honesty rail                       |
|  [02]   | fixer / findings     | workspace-write | findings file with stable ids, refute-first, settled-rulings roster, per-finding ledger   |
|  [03]   | implement / write    | workspace-write | enumerated moves as the bar, layer pin, item-by-item cadence, `edits[]` contract          |
|  [04]   | review / judgment    | either          | verdict-only under read-only; workspace-write folds a fixlog forward with an orphan drain |
|  [05]   | research / web       | read-only       | `-c web_search="live"`, cited-source contract                                             |
|  [06]   | volume extraction    | workspace-write | sol at `low`, `spawn_agents_on_csv`, per-row `output_schema`, one worker per row          |
|  [07]   | spawn-mandate parent | inherits        | overlay on any shape: the imperative spawn step rides the USER prompt                     |
|  [08]   | resume / follow-up   | inherited       | `codex-reply` or `exec resume` with a sharpened prompt only — never re-pay exploration    |

A recon lane differs from research only by the web flag and citation contract — the `cached` default answers from an index with no live fetch, so a research lane always states `live`. A fixer differs from implement by its findings anchor and refute rail; the spawn overlay composes onto any base shape.

## [03]-[TEMPLATES]

Recon developer-instructions:

```text template
<context_gathering>
Territory: <the exact files/dirs the lane may open>. Do not open files outside it, including skill or instruction files (.claude/, CLAUDE.md, AGENTS.md, ~/.codex/skills).
Budget: at most <N> tool calls total. Parallelize reads and take large windows (400+ lines per command), capping each command's output so nothing truncates; never concatenate the whole territory into one command.
Stop as soon as the product is complete. If something is still uncertain at the budget, proceed and record it in coverage.unverified instead of re-reading.
</context_gathering>

<verification>
Before the final message, confirm every cited spelling appears verbatim in the cited file; move anything unconfirmed into coverage.unverified.
</verification>

<output_contract>
Your final message is a single JSON object with exactly this shape:
<SHAPE with coverage{requested,read,skipped,unverified}>
- JSON only: no prose before or after it, no code fences, no markdown.
- Every key shown is required.
- Use null for a value you could not determine and [] for an empty list; never guess.
</output_contract>
```

Write developer-instructions:

```text template
<completion_bar>
Done is every named move landed to full depth with its product entry written — proof-complete, with observable evidence from the real surface, never effort-spent. Complete every named move before yielding; do not stop at analysis or a partial edit. If the chosen approach resists, pick the next-best one and proceed; a move the territory genuinely admits no edit for returns as a deferred entry naming its blocker. Your layer is <layer>: a finding outside the named scope lands as a typed entry in the product, never an edit — and re-verifying unchanged work adds no evidence; move to the next deliverable.
</completion_bar>

<read_discipline>
A stable input — law corpus, dossier, catalog, charter — is read ONCE: extract what you need into plan notes and re-open only the exact line span behind an edit. Read in large windows (400+ lines per command). Work ITEM BY ITEM — derive one item's findings, land its edits, record its product entry, advance; edits land as derived and never pool toward the end, because only plan notes, on-disk products, and landed edits survive compaction. Budget: at most <N> tool calls total; at the budget, land what is derived and record the remainder as deferred entries.
</read_discipline>

<design_and_scope_constraints>
Implement EXACTLY and ONLY the named moves; choose the simplest valid interpretation of any ambiguity. Edit only files inside <write-territory>; touch no other path, never git. No new files, wrappers, or parallel surfaces.
</design_and_scope_constraints>

<verification>
After editing, re-read each changed file and confirm it is coherent and nothing it carried was lost. Fix what fails before yielding; a check you did not run is never claimed as run. Verification matches the territory's medium: a markdown/prose territory verifies by reading — no compiler, build, or test gate runs against it. <verification-commands: truth rail per added host member; format gate once, batched, after the final edit>
</verification>

<output_contract>
Your final message is a single JSON object with exactly this shape:
{"edits":[{"file":"<path>","move":"<move-kind>","description":"<string>"}], <task-specific keys>, "summary":"<string>"}
- JSON only: no prose before or after it, no code fences, no markdown.
- Every key shown is required.
- Use null for a value you could not determine and [] for an empty list; never guess.
</output_contract>
```

A judgment lane extends the write template: `<role>` adds "a finding is an untrusted report about where to look, never ground truth"; `<context_gathering>` orders doctrine pages, then the findings file, then each cited file whole before its first edit; `<decision_procedure>` inserts before the scope block — refute each finding against disk, doctrine, and the settled-rulings roster first, implement only survivors, and a citation-backed push-back counts equal to a fix; `<design_and_scope_constraints>` carries the numbered settled-rulings roster, each with its falsifiable citation; `<verification>` adds a rubric walk over every finding id; `<output_contract>` becomes the per-id verdict ledger.

Spawn overlay — on the USER prompt, never the developer channel, because the injected spawn gate hears only the user channel and permissive wording yields zero spawns:

```text template
Spawn exactly <N> parallel sub-agents with collaboration.spawn_agent, one per <split>; collect them with collaboration.wait_agent before synthesizing. Each sub-agent is read-only, receives a self-contained task naming its exact territory, and returns <per-child shape>.
</text>
```

## [04]-[CALIBRATION]

- Budget caps TOTAL tool calls, never per-file reads — a per-file cap makes the lane aggregate the territory into one truncating command and completeness collapses. Size by shape: recon 20-40, write 40+, fixer per-finding.
- Autonomy states ONCE, naming safe actions — repeated "ask first" or "do not mutate" phrasing causes spurious approval pauses.
- Reserve ALWAYS/NEVER for true invariants (JSON-only, territory bans, no-git); phrase judgment calls as decision rules ("search again only for a missing required fact, never to improve phrasing").
- Done-claims name observable evidence from the real surface; "tests pass" or "checks ran" alone is not evidence.
- Ambiguity resolves by instruction, never by inviting questions — a headless lane has nobody to ask.
- A lane whose task matches one of codex's own skills legitimately reads that skill and runs its bundled gate; the territory exclusion targets aimless probing, not the skill system.
- Iterative deep work continues ONE thread with sharpened prompts; a follow-up turn carries only the delta, and a push-back identifies as Claude with evidence.
- When a lane misbehaves, repair the contract surgically: reproduce with the failing developer message plus a small batch of failure examples, then make small explicit edits — clarify conflicting rules, remove redundant lines — one change at a time.

## [05]-[EXCLUSIONS]

What never enters a lane prompt:

- Restated codex defaults — persistence, bias-to-action, parallel reads, `apply_patch` fluency, exploration, plan hygiene, generic error handling, prose styling. State deltas only.
- Persistence blocks ("keep going until fully resolved") — codex persists by default; the completion bar carries the anti-premature force, and a bare push amplifies scope-exceeding at high tiers.
- Chain-of-thought scaffolding and plan-narration preambles.
- Intensifier stacks ("THOROUGH", "exhaustive") — measured to cause tool over-use.
- Per-file read caps and "read each file at most once" phrasing.
- Hostile-stance paragraphs and estate-register law verbatim — a lane takes de-conflicted, task-scoped law.
- Broad write authority — the writable directory is the lane's whole world; cross-territory obligations belong to the caller.
- "Ask if unclear", "you may spawn", and every other permissive form whose imperative twin is the working one.
