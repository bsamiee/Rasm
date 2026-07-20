---
name: code-review
description: >-
    Owns the local review cycle over three engines — CodeRabbit, Greptile, Macroscope — across
    anything they review. Deliverables are normalized findings, drained fixer lanes, the standing
    refuted-class registry, and the reviewer configs hardened each round: `.coderabbit.yaml`,
    `.greptile/`, `.macroscope/`. Use on any request to review, autonomously when landed work
    warrants one, and whenever those config files are read, tuned, or authored — this skill owns
    their schemas and rule grammar. Hosted PR reviewer round-trips belong to pr-loop.
---

# [CODE_REVIEW]

One rail folds CodeRabbit, Greptile, and Macroscope into a single improvement machine: it launches, watches, and harvests each into one normalized finding schema, fixer lanes drain the findings under the shipped universal fixer template, and each round distills refuted classes and lessons into the reviewer configs so the next round runs harder.

## [01]-[ROUTING]

[REFERENCES]:
- [01]-[CODERABBIT](references/coderabbit.md): `.coderabbit.yaml` config, the `--agent` stream, and the rich finding store
- [02]-[GREPTILE](references/greptile.md): `.greptile/` cascade, the `--json` envelope and status oracle, and the MCP bridge
- [03]-[MACROSCOPE](references/macroscope.md): `.macroscope/` concern files, the stream grammar, and worktree custody

[TEMPLATES]: the prompt templates are disjoint; a fact appearing in two of them is a defect.
- [01]-[FIX](templates/fix.md): universal fixer template — conduct law, corpus fills, and both dispatch lanes
- [02]-[CLOSE](templates/close.md): universal closer template — weight-sized arming, land-or-refute stance
- [03]-[HARVEST](templates/harvest.md): per-round reviewer-harvest dispatch — round-instance slots and both transports
- [04]-[REFUTED_CLASSES](templates/refuted-classes.yaml): refuted-class registry compiled into classifier, recurrence detector, and rulings roster

[SCRIPTS]:
- [01]-[REVIEW_RAIL](scripts/review_rail.py): verb rail printing one JSON receipt per verb

[AGENT]:
- `reviewer-harvest`: owns the DISTILL leg, dispatched via the Agent tool from `[05]`; its standing law lives in its own system prompt

## [02]-[CYCLE]

Every round runs COLLECT -> FIX -> DISPOSITION -> DISTILL on two custody lanes that never mix: work under review stays uncommitted or on its slice commits, while distillation — reviewer configs, `docs/`, `tools/` — pushes to origin's default branch on landing, because hosted engines read only the indexed default branch. Steps run in order, each with its watch and tuning knob. Rounds number from 1 per campaign: archiving the prior `.cache/review/` round dirs and `rounds.jsonl` before first launch keeps numbering and the ledger delta threading within one campaign, never across.

1. [CUSTODY]: commit non-review surfaces first — reviewer configs, `docs/`, `tools/`, `.claude/` infra — so the review scope holds exactly the work under review. WATCH: a reviewed-work file in the custody commit shrinks the next round's scope silently. KNOB: the commit-scope roster.
2. [LAUNCH]: `launch` per engine at its canonical scope ([03]) with `--follow` arming the watcher in the same verb; a bare `launch` prints `watch_cmd` to run instead. Every diff-scoped engine shares one blind spot — an untouched file never enters review — so an estate round also runs `.claude/skills/skill-writer/scripts/estate_audit.py` beside the engines, its rows hand-merged into `<round-dir>/findings.json` between normalize and slice, while ids are unstamped. KNOB: engine choice and `--focus`.
3. [NORMALIZE]: `findings --normalize` per completed round; `gather` unions concurrent engines; `findings --digest` is the post-normalize read and the query filters answer targeted inspections — never hand-jq over findings.json. WATCH: the provenance histogram before any count judgment — a flat total decomposes into `relitigation|refuted_remint|new_work|late_discovery` before any config takes blame, and rising new-work beside falling relitigation is convergence. KNOB: `--dedup-against`.
4. [SLICE]: `slice` into balanced per-lane manifests, each with its dispatch-ready `lane-<letter>-brief.md`; an omitted `--lanes` derives the cut from finding count against the rail's `LANE_LOAD` target, clamped to `LANES_CAP`. WATCH: severity and folder balance across lanes. KNOB: explicit `--lanes`, one per balanced folder slice up to the ceiling — small slices direct freed capacity into capability depth, never early finish.
5. [FIX]: one keeper per lane under `templates/fix.md`, carrying both dispatch lanes — codex takes the law block as developer instructions and the task as the user prompt, claude takes both in one Agent-call message, law first; lanes dispatch concurrently; transport mechanics stay the codex skill's. WATCH: stderr banner and spawn audit per lane; a lost report relaunches once, a second failure routes to a closer. KNOB: lane count and template wording — sharpen by replacement, never accumulate.
    - Codex lanes run the config-default model and effort unflagged under the full user config (miner spawns need the multi-agent depth row; deviate only for purely mechanical slices); the task never names the report file, and `-o <round-dir>/lane-<letter>-report.json` materializes the captured final message — a violation recovering via `codex exec resume <session-id>` re-emitting with no `-o` — the report's self-stated `model` re-stamping from the stderr banner at reconcile.
    - Each lane returns only its report path — the codex lane's `-o` capture, the claude lane's own final write — so report bodies never enter the orchestrator's context, and an observed shortfall hardens the universal template as an explicit decision rule, never a weaker per-model variant.
6. [RECONCILE]: `reconcile` proves per-lane id bijection and emits the verdict histogram, exit 0 only when bijective. WATCH: verdict-mix honesty — fixed-versus-upgraded inflation, citation-backed push-back share; `report_valid: false` beside a `fault` is a decode failure to repair first, since `harvest` and `round` refuse a partial report set. KNOB: a dropped finding closes through one small closer armed with the slice's stack doctrine, never a session resume.
7. [CLOSERS]: routing rows drain concurrently over disjoint territories under `templates/close.md` — a small closer for single-file mechanical rows, a full-arming closer for family, design, or capability work, sized by row weight. WATCH: honest land-versus-refute — a mined candidate that cannot ground is refuted with its citation, never forced. KNOB: closer sizing.
8. [DISTILL]: `harvest` assembles feed and memory proposals (`round --harvest` in one verb); dispatch under `templates/harvest.md` — claude rides `Agent(subagent_type: "reviewer-harvest")` with the filled `[01]` round slots as its whole prompt, codex adds the `[02]` law block as developer instructions. WATCH: diff-sample touched blocks — density rose, integrations weave, additions earned; the `trimmed` self-report verifies against the diff. KNOB: the agent file, the single versioned tuning surface.
9. [VERIFY]: project the agent's ledger into `<round-dir>/surface-ledger.json` ([05]), then `verify --round N` proves each guard in its surface's own oracle. WATCH: an ineffective row marks failed wording — harden the owner and re-verify before `round`, since the close is one-shot; never re-skip. KNOB: `templates/refuted-classes.yaml` rows.
10. [CLOSE]: commit and push the distillation lane, then `round` appends the `rounds.jsonl` row — `grades` carrying the computed [GRADING] lane axes — and prints the delta; a zero-findings round closes clean from normalize; verdicts and class calibrations distill into the campaign memory (`project_cr_review_cycle_machine`) as merged rulings, never narration. WATCH: findings trending down while capability rows rise is the goal line. KNOB: none — the ledger is the sole progress instrument.
11. [NEXT]: grade the round on the [GRADING] axes, then pick the next engine — recurrence judges per engine, counts flattening under one engine rotate the next round to another, and `--focus` aims a round within one: greptile rides early rounds while the accumulated diff fits its size caps, coderabbit carries breadth at any diff size, macroscope joins whenever its lane is open. WATCH: plateau under a hardened config. KNOB: rotation and focus.

- [FRAMING]: both framings ride every surface — negative framing kills false-positive classes through do-not-flag guards citing the refuting ruling, positive framing steers toward house demands through hunt axes and hit-shape rosters. Hit-shape rosters land on every surface regardless of what an engine emits, because fixer miners execute them — the flywheel: fixer discoveries become next-round hit-shapes.
- [GRADING]: REVIEW-THE-REVIEWER grades every round on the round row's typed axis fields — `fp_share`, `relitigation_share`, `novel_quality` (accepted-verdict share over novel-provenance rows), `hunt_axis_fire` (per-axis fire counts) — ledger-recorded so config changes read before-and-after mechanically. Standing verdict: CodeRabbit emits no missing-capability findings under any `path_instructions` wording — capability-direction rides Macroscope check-run agents and Greptile structured rules.
    - Finding strength scores a per-round sample 0-4: anchored (file and non-zero range, rail-stamped), actionable (repair direction, rail-stamped), discriminating (the claim states why the shape is wrong), novel (no adjudicated-class relitigation); a sub-2 mean marks a noise lane.
    - Fixer lanes grade on five axes per model, computed into the round row's `grades` field beside the raw `by_model` verdict rollup — `depth_of_fix` (`upgraded` over `fixed` + `upgraded`), `scope_clean` (phantom-free lanes), `refute_cited` (citation-backed push-backs), `gate_clean`, `bijective` (id bijection) — so the claude-versus-codex comparison is one jq group; upgraded claims surviving diff re-read stay the orchestrator's judgment axis.
    - Grades drive the tuning loop: a template, profile, or config edit lands between rounds and logs against the round it precedes, so the next round's delta reads as prompt-change to behavior-change mechanically. Grades stay truthful — an inflated grade poisons the attribution, and a competent-but-unremarkable lane grades exactly that; a depth shortfall tunes the dispatch template's mandate, never the grade.
    - Prose FP classes live as `corpus: prose` registry rows citing the docgen defect catalog, graded by the same recurrence machinery.

## [03]-[USAGE]

Receipt proofs per verb, in drive order; every decision point rides its [02] step's WATCH and KNOB rows.

1. `launch --reviewer <engine> --scope <scope> --follow [--normalize]` — receipt proves the spawn (round number, pgid, argv, `watch_cmd`); follow ends at a terminal phase, exit 0 only on `completed`, a short receipt naming its fault `signature` and `remedy`; `--normalize` lands the findings receipt the moment the follow completes.
2. `findings --normalize --round N` — receipt proves admission: `admitted`/`total`/`deduped`, the provenance histogram, `scope_misfire`.
3. `findings --digest --round N` — prints the round read: severity counts, class fires, folder cuts, top rows per severity; engine rotation and lane count decide here.
4. `slice --round N` — receipt proves the cut: per-lane manifests, stamped ids, brief paths; fixer lanes dispatch from the briefs. Zero findings skip to item 9, and a round already carrying lane reports refuses without `--recut`.
5. `reconcile --round N` — `bijective: true` proves every stamped id carries exactly one verdict, exit 0 only then; `missing`/`phantom` name the defecting lane.
6. `harvest --round N` — receipt proves the feed: recurred classes, fresh refutations, improvement/capability/routing counts, proposal paths; refuses while any lane lacks a decodable report.
7. `registry --apply --rows <path>` — lands only a fully clean set, append-only, refusing whole and naming each fault; `registry --check --rows <path>` is the post-refusal diagnostic with per-row detail, and bare `registry --check` lints the standing yaml.
8. `verify --round N` — each surface-ledger guard proves in its surface's own oracle; `effective: false` marks failed wording, and a `source: surface-ledger` receipt marks a malformed row, never wording.
9. `round [--harvest] --round N` — appends the ledger row with computed `grades` and prints the delta against the prior row, closing the round.
10. `gather --all-live|--reviewer <names>|--rounds <numbers>` — after concurrent engine rounds, unions completed normalized rounds into one pre-normalized round (receipt: sources, total, corroborated) that items 4-9 then target; `--all-live` here takes every engine round with no ledger row and no prior gather, so pass `--rounds` when older rounds sit open.

```bash template
rail() { uv run "${CLAUDE_SKILL_DIR}/scripts/review_rail.py" "$@"; }   # shell state dies per tool call — re-define per shell, or use the literal path
rail launch --reviewer <engine> --scope <scope> [--focus <text-or-file>] [--follow [--json] [--normalize]]
# without --follow, run the receipt's watch_cmd; its exit re-invokes the agent
rail status --follow --round <N> [--json] [--normalize]
rail findings --normalize --round <N> [--dedup-against <M>]
rail findings --digest --round <N>
rail findings --file '<glob>' --severity <floor> --claim '<regex>' [--in claim|fix|both] --round <N> [--json]
rail slice [--lanes 3] [--recut] --round <N>
# fixer lanes run here (orchestrator dispatch): each prompt cites its lane-<letter>-brief.md; each lane writes <round-dir>/lane-<letter>-report.json
rail reconcile --round <N>
rail harvest --round <N>
# dispatch the reviewer-harvest agent on the feed; persist its ledger as <round-dir>/surface-ledger.json
rail registry --apply --rows <path>   # append-only land of fully-clean rows; any fault refuses whole, --check diagnoses
rail verify --round <N>
rail round [--harvest] --round <N>
```

```bash template
rail launch --reviewer coderabbit --scope uncommitted
rail launch --reviewer greptile --scope base:<prior-boundary>
rail launch --reviewer macroscope --scope base:<default-branch>
rail status --follow --all-live            # exits when every live round is terminal
rail findings --normalize --round <N>      # once per engine round
rail gather --all-live                     # union round: fingerprint collapse, corroborators, max severity
# slice / reconcile / harvest / verify / round then target the gather round
```

Every round-scoped verb accepts `--round N` and `--dir` and prints one JSON receipt; a refusal exits nonzero carrying `{code, detail}`. `launch` refuses an unsupported engine-scope pair and a live same-engine round; the scope family is `all|committed|uncommitted|base:<ref>|base-commit:<sha>`, every `--reviewer` takes the `cr|gt|ms` aliases, and conflicting selectors refuse `bad-flag` on `status`/`kill`. `--focus` takes inline text or a file path — greptile rides `--instructions`, coderabbit a round-scoped `-c` instruction file, and macroscope refuses `--focus` before any preflight side effect.

Engine scope selection; each reference owns its engine's mechanics — base resolution, size gating, language routing, and config surface.

- [CODERABBIT]: sweeps working-tree quality and style at full breadth every run — a retry re-spends quota; canonical round `--scope uncommitted`, full scope family accepted. `launch` resolves and injects a base for every working-tree scope, refusing with the persist remedy when none resolves, since the engine otherwise fails within a second while exiting 0. `--light` is coderabbit-only.
- [GREPTILE]: hunts cross-file logic over committed commits against a base; canonical round `--scope base:<prior-boundary>` after committing the slice, `committed` reviews against the repo default, and `base:`/`base-commit:` refs take any committish.
    - Scope is a base..HEAD range, so a cumulative campaign pins one base — `base-commit:<campaign-boundary>` held across rounds — and the whole accumulated delta reviews as one change regardless of commit count, with cross-round dedup and provenance keeping adjudicated rows out of lanes; local commits suffice for the CLI review, and a later push carries them to origin.
    - A pinned base holds until greptile's client-side size caps refuse, then the campaign falls back to slice commits at the prior boundary; `--resume` and `--include` are greptile-only passthroughs.
- [MACROSCOPE]: streams AST-level correctness in place; canonical round `--scope base:<default-branch>` spanning committed branch work and uncommitted edits, `uncommitted` for tree-only — an in-place run without a base on a branch reviews almost nothing. Always in place — fixes land in the files the review read — and `launch` preflight-sweeps stray review worktrees.

`watch_cmd` on the launch receipt self-exits at the terminal phase. Terminal phases are `completed|failed|refused|stalled|timed-out|killed`; a receipt short of `completed` names its fault `signature`, `remedy`, and engine `exit_code`, and `status --follow --all-live` exits 0 only when every followed round completed. `--json` on any follow streams one `StatusReceipt` JSON line per phase change or minute pulse for a piping orchestrator; the terminal receipt prints either way.

Bare `status` resolves the single live round, else the highest-numbered on disk; several live rounds refuse `ambiguous` naming them — pass `--round`, `--reviewer`, or `--all-live`. `kill` (same selectors, same `no-round`/`ambiguous` selection refusals) escalates SIGTERM to SIGKILL over the process group, sweeps detached engine survivors and macroscope worktrees, and reaps a stalled or timed-out round whose engine still breathes; per round it refuses a gather round (`no-process`) and a round whose process group is truly gone (`already-closed`).

`findings --normalize` gates on `completed` and refuses an already-sliced round. Minting strips each engine's constant fix-preamble (one `ENGINE_PREAMBLES` row each) and carries content fields only; the receipt `source` names the store a raw payload re-reads from.

Cross-round dedup runs against the latest prior round carrying lane reports — `--dedup-against N` pins an earlier pool — and dropped fingerprints leave only after the provenance histogram computes over the full set, so relitigation surfaces while lanes never re-fix adjudicated rows. Round 1 and a round after a zero-findings close have no qualifying prior and read `unstamped` — undecomposable, never a config failure.

`scope_misfire` on the receipt flags zero rows over a dirty scope; an exit-0 zero-comment envelope over a clean scope is ordinary success. Normalize refuses `dedup-collapse` when over half of eight or more admitted rows drop as intra-round fingerprint duplicates — engine format drift, never real duplication — the refusal `detail` carrying the admitted count, survivor count, and collapse share.

Two engine refusals are terminal for their round: `store-missing` (coderabbit) means the store epoch never correlated — relaunch, accepting the re-spend; `no-payload` (greptile) means no `--json` envelope reached the log — read the `stream.log` head for an unmatched auth or billing line, then relaunch.

Bare `findings` reprints the summary receipt; `--digest [--top N]` prints the round read, and `--file <glob>`, `--severity <floor>`, `--claim <regex>` filter the normalized set as a query, composable with `--digest` — digest and query print human-scan text, `--json` the typed form. `--in claim|fix|both` scopes the `--claim` regex (default both) and refuses without `--claim`. `gather` takes exactly one of `--all-live`, `--reviewer cr,gt,ms`, or `--rounds 1,2,3`; every source must be `completed` and normalized.

`slice [--lanes N] --balance count|loc` partitions by folder affinity — grouping deepens past common prefixes and splits oversized folders at sub-folder seams until lanes balance, so N lanes over at least N findings always cut N non-degenerate lanes. It clears stale lane artifacts only after the cut proves out (existing lane reports refuse without `--recut`), stamps round-scoped ids, and emits per-lane manifests carrying the corpus-matched settled-rulings roster and the `lane-<letter>-brief.md` path; a registry row tagged `corpus` slices only into lanes whose file set resolves to that corpus.

`reconcile` covers all lanes bare (`--all` is the explicit synonym; a named lane with `--all` refuses). `round` refuses a duplicate close, fails loud on findings without lane reports, and closes a zero-findings round clean. `verify` takes exactly one of `--rule <text> [--path <file>]` (greptile cascade check) or `--round N` (all-surface ledger check). `selftest` proves the rail's pure contracts — lane-report decode against the shipped maximal-shape fixture, registry checking, mint hygiene, query scoping, grading, lane clamp — exiting nonzero naming each failed proof.

## [04]-[LANE_CONTRACT]

Fixer-lane doctrine lives in `templates/fix.md`. Per lane the rail writes `lane-<letter>.json` and `lane-<letter>-brief.md`; dispatch writes `task-<letter>.md`, `lane-<letter>-report.json`, `lane-<letter>-events.jsonl`, and `lane-<letter>-stderr.log`. Each report is the machine intake `reconcile`, `harvest`, and the reviewer-harvest agent parse, written to `<round-dir>/lane-<letter>-report.json` as the lane's final act:

```json signature
{
  "ledger": [{"id": "", "file": "", "severity": "", "verdict": "fixed|upgraded|pushed-back|already_resolved", "note": ""}],
  "improvements": [{"page": "", "pattern": "", "what": "", "axis": ""}],
  "refuted": [{"claim": "", "evidence": ""}],
  "capability": [],
  "routing": [],
  "uncertain": [],
  "gate_clean": true,
  "model": "",
  "wall_s": 0.0
}
```

`axis` is fixer-stamped hunt-axis metadata, never inferred. `routing` rows carry `target_file`/`needed_change` — closers grep them per target; a `capability` row carrying a `members`, `roster`, or `symbols` list feeds the `reference_*` memory proposals; only `uncertain` is free-shape, reprinted verbatim by the feed. `gate_clean` records the final batched format-gate run over the lane's touched files, and `model` keys the grade cohorts casefolded at reconcile.

## [05]-[DISTILL]

Reviewer-harvest owns the distill leg: dispatch on either transport under `templates/harvest.md`; it returns a typed surface ledger. Round dirs sit outside the agent's write territory, so project its landed guards into `<round-dir>/surface-ledger.json` yourself as `[{surface, text, path}]` rows — `surface` an engine name or alias, `text` the shortest guard substring unique on the surface, `path` blank defaulting per engine (`.coderabbit.yaml`; the greptile repo-wide cascade; a full-text scan across `.macroscope/**/*.md`) — then `verify --round N` proves each row in its surface's oracle.

Project one row per landed addition and consolidation; a receipt whose `source` is `surface-ledger` marks a malformed row (blank text or an unmapped surface name), never failed wording.

- [REGISTRY]: `templates/refuted-classes.yaml` rows compile into the claim classifier, the recurrence detector, and the settled-rulings roster sliced into lane manifests by `corpus` tag; `harvest` proposes new rows in the feed, `registry --check --rows` proves them (matcher compile, schema, dedup), and `registry --apply` lands the clean set append-only; a judgment-bearing merge into an existing row stays a hand edit in the yaml, re-proved by bare `registry --check`, never in the script.
- [RAIL_GAPS]: a rail gap a round exposes lands on the rail's own script and data surfaces.
- [VERIFY]: `verify --round` re-proves each landed guard — a `.coderabbit.yaml` `path_instructions` clause by text, a greptile rule by substring over the resolved `greptile config` output (never by id — org rules re-key to server UUIDs) falling back to full-text over `.greptile/config.json` and `.greptile/rules.md` when the cascade truncates or a scoped rule never resolves at the probe path, the receipt `source` naming the oracle that matched, a macroscope topic file by presence and content.
- [FEED_OUTPUTS]: provenance and corroboration histograms feed review-the-reviewer — a class one engine raises and rounds keep refuting marks that engine's false-positive tendency, and multi-engine corroboration marks high-confidence work.
- [PROPOSALS]: `memory-proposals/` under the round dir carries candidate memory files the orchestrator curates, never the live memory dir — the campaign memory (`project_cr_review_cycle_machine`) is the destination owner, and a proposal duplicating a repo-owned fact dies at curation; the feed's `[GUARD_INEFFECTIVE]` section names classes recurring despite landed guards.

## [06]-[NON_GOALS]

- Rail territory ends at manifests and feeds — fixer dispatch belongs to the orchestrator, which judges slicing, models, and prompts.
- Watchers arm per round and die at terminal; no standing watcher, daemon, or per-event hook.
- Cross-reviewer merge stops at fingerprint dedup and the corroborator record — two engines disagreeing is signal, never noise to reconcile silently.
- `rounds.jsonl` is the health readout and the row-to-row delta the only progress instrument — jsonl and jq, never a dashboard.
