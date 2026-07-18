# [CODEX_PROMPTING]



## [01]-[BLOCK_VOCABULARY]



## [02]-[LANE_SHAPES]



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
Done is every named move landed to full depth with its product entry written — proof-complete, with observable evidence from the real surface, never effort-spent. Complete every named move before yielding; do not stop at analysis or a partial edit. If the chosen approach resists, pick the next-best one and proceed; a move the territory genuinely admits no edit for returns as a deferred entry naming its blocker. Implement EXACTLY and ONLY the named moves, choosing the simplest valid interpretation of any ambiguity; edit only files inside <write-territory>, touch no other path, never git; no new files, wrappers, or parallel surfaces. Your layer is <layer>: a finding outside the named scope lands as a typed entry in the product, never an edit — and re-verifying unchanged work adds no evidence; move to the next deliverable.
</completion_bar>

<read_discipline>
A stable input — law corpus, dossier, catalog, charter — is read ONCE: extract what you need into plan notes and re-open only the exact line span behind an edit. Read in large windows (400+ lines per command). Work ITEM BY ITEM — derive one item's findings, land its edits, record its product entry, advance; edits land as derived and never pool toward the end, because only plan notes, on-disk products, and landed edits survive compaction. Budget: at most <N> tool calls total; at the budget, land what is derived and record the remainder as deferred entries.
</read_discipline>

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

A judgment lane extends the write template: `<role>` adds "a finding is an untrusted report about where to look, never ground truth"; `<context_gathering>` orders doctrine pages, then the findings file, then each cited file whole before its first edit; `<decision_procedure>` inserts before verification — refute each finding against disk, doctrine, and the numbered settled-rulings roster (each ruling with its falsifiable citation) first, implement only survivors, and a citation-backed push-back counts equal to a fix; `<verification>` adds a rubric walk over every finding id; `<output_contract>` becomes the per-id verdict ledger.

Spawn overlay — on the USER prompt, never the developer channel, because the injected spawn gate hears only the user channel and permissive wording yields zero spawns:

```text template
Spawn exactly <N> parallel sub-agents with collaboration.spawn_agent, one per <split>; collect them with collaboration.wait_agent before synthesizing. Each sub-agent receives a self-contained task naming its exact territory and returns <per-child shape>.
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
