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

One rail carries the whole cycle: engine rounds normalize into one finding schema, fixer lanes drain the findings, closers drain the routed rows, and each round distills refuted classes and lessons into the reviewer configs so the next round runs harder.

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

Every round runs COLLECT -> FIX -> DISPOSITION -> DISTILL on two custody lanes that never mix: work under review stays uncommitted or on its slice commits, while distillation — reviewer configs, `docs/`, `tools/` — pushes to origin's default branch on landing, because hosted engines read only the indexed default branch. Rounds number from 1 per campaign: archiving the prior `.cache/review/` round dirs and `rounds.jsonl` before first launch keeps numbering and the delta threading within one campaign.

Step order never proves a drain — the owning receipt does: `reconcile` surfaces `routing.pending`, `round` refuses `routing-undrained` while it is non-empty, and `harvest`/`round` refuse a partial lane-report set.

1. [CUSTODY]: commit non-review surfaces first — reviewer configs, `docs/`, `tools/`, `.claude/` infra — so the review scope holds exactly the work under review. WATCH: a reviewed-work file in the custody commit shrinks the next round's scope silently. KNOB: the commit-scope roster.
2. [LAUNCH]: `launch` per engine at its canonical scope ([03]) with `--follow` arming the watcher in the same verb; a bare `launch` prints `watch_cmd` to run instead. Every diff-scoped engine shares one blind spot — an untouched file never enters review — so an estate round also runs `.claude/skills/skill-writer/scripts/estate_audit.py` beside the engines, its rows hand-merged into `<round-dir>/findings.json` between normalize and slice, while ids are unstamped. KNOB: engine choice and `--focus`.
3. [NORMALIZE]: `findings --normalize` per completed round; `gather` unions concurrent engines; `findings --digest` and the query filters answer every read — never hand-jq over findings.json. WATCH: the provenance histogram before any count judgment — a flat total decomposes into `relitigation|refuted_remint|new_work|late_discovery`, and rising new-work beside falling relitigation is convergence. KNOB: `--dedup-against`.
4. [SLICE]: `slice` into balanced per-lane manifests, each with its dispatch-ready `lane-<letter>-brief.md`; `lanes --fill` then emits each lane's corpus data for the template fill. WATCH: severity and folder balance across lanes. KNOB: explicit `--lanes`, one per balanced folder slice up to the ceiling — small slices direct freed capacity into capability depth, never early finish.
5. [FIX]: one keeper per lane under `templates/fix.md`, carrying both dispatch lanes — codex takes the law block as developer instructions and the task as the user prompt, claude takes both in one Agent-call message, law first; lanes dispatch concurrently, transport mechanics the codex skill's. WATCH: `lanes --spawns` proves each lane's miner passes; a lost report relaunches once, a second failure routes to a closer. KNOB: lane count and template wording — sharpen by replacement, never accumulate.
    - Codex lanes run the config-default model and effort unflagged under the full user config (miner spawns need the multi-agent depth row; deviate only for purely mechanical slices); the task never names the report file, and `-o <round-dir>/lane-<letter>-report.json` materializes the captured final message — a violation recovering via `codex exec resume <session-id>` re-emitting with no `-o` — the report's self-stated `model` casefolding into one grade cohort at reconcile.
    - Each lane returns only its report path — the codex lane's `-o` capture, the claude lane's own final write — so report bodies never enter the orchestrator's context, and an observed shortfall hardens the universal template as an explicit decision rule, never a weaker per-model variant.
6. [RECONCILE]: `reconcile` proves per-lane id bijection, names what each lane touched (`files`/`strays`), and emits the routing drain. WATCH: verdict-mix honesty — fixed-versus-upgraded inflation, citation-backed push-back share; `report_valid: false` beside a `fault` is a decode failure to repair first.
7. [CLOSERS]: routing rows alone arm this stage — a finding `reconcile` reports dropped or phantom joins as one more routed demand; `slice --closers` cuts the rows into file-disjoint territories, and closers dispatch concurrently under `templates/close.md`, sized by row weight, each writing `close-<letter>-report.json` answering by the brief's stamped keys. WATCH: honest land-versus-refute — a mined candidate that cannot ground is refuted with its citation, never forced. KNOB: closer sizing.
8. [DISTILL]: `harvest` assembles feed and memory proposals (`round --harvest` in one verb); dispatch under `templates/harvest.md` — claude rides `Agent(subagent_type: "reviewer-harvest")` with the filled `[01]` round slots as its whole prompt, codex adds the `[02]` law block as developer instructions. WATCH: diff-sample touched blocks — density rose, integrations weave, additions earned; the `trimmed` self-report verifies against the diff. KNOB: the agent file, the single versioned tuning surface.
9. [VERIFY]: land the surface ledger and prove it per [05]. WATCH: an ineffective row marks failed wording — harden the owner and re-verify before `round`, since the close is one-shot; never re-skip. KNOB: `templates/refuted-classes.yaml` rows.
10. [CLOSE]: commit and push the distillation lane, then `round` appends the `rounds.jsonl` row and prints the delta ([03] item 10 carries the refusal ladder). Verdicts and class calibrations distill into the campaign memory (`project_cr_review_cycle_machine`) as merged rulings, never narration. WATCH: findings trending down while capability rows rise is the goal line.
11. [NEXT]: grade the round on the [GRADING] axes, then pick the next engine — recurrence judges per engine, counts flattening under one engine rotate the next round to another, and `--focus` aims a round within one: greptile rides early rounds while the accumulated diff fits its size caps, coderabbit carries breadth at any diff size, macroscope joins whenever its lane is open. WATCH: plateau under a hardened config. KNOB: rotation and focus.

- [FRAMING]: both framings ride every surface — negative framing kills false-positive classes through do-not-flag guards citing the refuting ruling, positive framing steers toward house demands through hunt axes and hit-shape rosters. Hit-shape rosters land on every surface regardless of what an engine emits, because fixer miners execute them.
- [GRADING]: REVIEW-THE-REVIEWER grades every round on the round row's typed fields, each naming its oracle, so a between-rounds template edit reads as prompt-change to behavior-change mechanically. Standing verdict: CodeRabbit emits no missing-capability findings under any `path_instructions` wording — capability-direction rides Macroscope check-run agents and Greptile structured rules.
    - Round axes and oracles: `fp_share`/`relitigation_share` (pre-prune provenance histogram), `novel_quality` (accepted-verdict share over novel-provenance rows), `hunt_axis_fire` (fixer-stamped improvement axes), `shape_fire` (fixer-stamped ledger `shape`; `LaneStat.shapes` counts each lane's unstamped gap), `wall_spread` (lane wall times; an outlier exceeds twice the median), `prompts` (content hashes of the three templates and the `reviewer-harvest` agent).
    - Finding strength stays a judgment sample, never a computed grade: `anchored` and `actionable` are rail-stamped bits, while discriminating (the claim states why the shape is wrong) and novelty read by operator judgment — a computed stand-in fabricates the grade.
    - Fixer lanes grade on five per-model axes in `grades` beside the raw `by_model` verdict rollup — `depth_of_fix` (`upgraded` over `fixed` + `upgraded`), `scope_clean` (phantom-free lanes), `refute_cited` (citation-backed push-backs), `gate_clean`, `bijective` — so the claude-versus-codex comparison is one jq group; upgraded claims surviving diff re-read stay the orchestrator's judgment axis.
    - Prose FP classes live as `corpus: prose` registry rows citing the docgen defect catalog, graded by the same recurrence machinery.

## [03]-[USAGE]

Shortest correct path in drive order; each row names the receipt proof and the decision that branches it, and every decision point rides its [02] step's WATCH and KNOB rows.

1. `launch --reviewer <engine> --scope <scope> --follow --normalize` — receipt proves the spawn (round number, pgid, argv, `watch_cmd`); the follow exits 0 only on `completed` and lands the findings receipt in the same verb, a short receipt naming its fault `signature` and `remedy`. Without `--follow`, run `watch_cmd` — it self-exits at the terminal phase, its exit re-invokes the agent, and `findings --normalize --round N` then lands the receipt.
2. `findings --digest --round N` — prints the round read: severity counts, class fires, folder cuts, top rows per severity; engine rotation and lane count decide here. `findings --file '<glob>' --severity <floor> --claim '<regex>' [--in claim|fix|both]` answers targeted queries; bare `findings` reprints the admission summary.
3. `slice --round N` — receipt proves the cut: per-lane manifests, stamped ids, brief paths. Zero findings skip to step 10; a round already carrying lane reports refuses without `--recut`.
4. `lanes --fill --round N` — per-lane corpus data for the orchestrator to fill `templates/fix.md` with: files, owning packages with `.api` overlays, substrate `.api`, doctrine roots, unplaced files. Dispatch fixer lanes from the briefs, each writing `<round-dir>/lane-<letter>-report.json`.
5. `reconcile --round N` — exit 0 only when `bijective: true` proves every stamped id carries exactly one verdict; `missing`/`phantom` name the defecting lane, `files`/`strays` name cross-territory writes, and `routing` states the closer drain `{total, drained, verdicts, pending}`.
6. `lanes --spawns --round N` — maps each dispatch lane to its codex session and rollout and counts real `spawn_agent` calls; run it whenever a lane's mining depth is in question.
7. `slice --closers --round N` — cuts the routing rows into file-disjoint closer territories (`close-<letter>.json` and brief); dispatch closers, then re-run `reconcile` to prove the drain empty. Existing closer reports refuse without `--recut`.
8. `harvest --round N` — receipt proves the feed: recurred classes, fresh refutations, improvement/capability/routing counts, proposal paths. `round --harvest` folds it into the close.
9. `registry --apply --rows <path>` lands only a fully clean set, append-only, refusing whole and naming each fault; `registry --check --rows <path>` diagnoses the refusal, bare `registry --check` lints the standing yaml. `verify --round N` proves each surface-ledger guard in its surface's own oracle ([05]).
10. `round [--harvest] [--defer-routing] --round N` — appends the ledger row (grades, `shape_fire`, `wall_spread`, `routing`, `prompts`) and prints the delta; refuses `routing-undrained` while routing rows lack closer verdicts unless `--defer-routing` records the deferral; a zero-findings round closes clean from normalize.
11. `gather --all-live|--reviewer <names>|--rounds <numbers>` — after concurrent engine rounds, unions completed normalized rounds into one pre-normalized round (receipt: sources, total, corroborated) that steps 3-10 then target; `--all-live` takes every engine round with no ledger row and no prior gather, so pass `--rounds` when older rounds sit open.

```bash template
rail() { uv run "${CLAUDE_SKILL_DIR}/scripts/review_rail.py" "$@"; }   # shell state dies per tool call — re-define per shell, or use the literal path
rail launch --reviewer <engine> --scope <scope> [--focus <text-or-file>] [--follow [--json] [--normalize]]
rail status --follow --round <N> [--json] [--normalize]
rail findings --normalize --round <N> [--dedup-against <M>]
rail findings --digest --round <N>
rail findings --file '<glob>' --severity <floor> --claim '<regex>' [--in claim|fix|both] --round <N> [--json]
rail slice [--lanes 3] [--recut] --round <N>
rail lanes --fill --round <N>
# fixer lanes dispatch here (orchestrator): each prompt cites its lane-<letter>-brief.md; each lane writes <round-dir>/lane-<letter>-report.json
rail reconcile --round <N>
rail lanes --spawns [--lane <name>] --round <N>
rail slice --closers [--lanes 2] [--recut] --round <N>
# closers dispatch here: each cites its close-<letter>-brief.md and writes <round-dir>/close-<letter>-report.json
rail harvest --round <N>
# dispatch the reviewer-harvest agent on the feed; persist its ledger as <round-dir>/surface-ledger.json
rail registry --apply --rows <path>
rail verify --round <N>
rail round [--harvest] [--defer-routing] --round <N>
```

```bash template
rail launch --reviewer coderabbit --scope uncommitted
rail launch --reviewer greptile --scope base:<prior-boundary>
rail launch --reviewer macroscope --scope base:<default-branch>
rail status --follow --all-live            # exits when every live round is terminal
rail findings --normalize --round <N>      # once per engine round
rail gather --all-live                     # union round: fingerprint collapse, corroborators, max severity
# slice / reconcile / closers / harvest / verify / round then target the gather round
```

Every round-scoped verb accepts `--round N` and `--dir` (synonym `--directory`) and prints one JSON receipt; a refusal exits nonzero carrying `{code, detail}`. `REVIEW_RAIL_BUNDLE` points a relocated copy of the script back at the bundle's `templates/` and registry.

`launch` refuses an unsupported engine-scope pair and a live same-engine round; the scope family is `all|committed|uncommitted|base:<ref>|base-commit:<sha>`, every `--reviewer` takes the `cr|gt|ms` aliases, and conflicting selectors refuse `bad-flag` on `status`/`kill`. `--focus` takes inline text or a file path — greptile rides `--instructions`, coderabbit a round-scoped `-c` instruction file, and macroscope refuses `--focus` before any preflight side effect.

- [CODERABBIT]: sweeps working-tree quality and style at full breadth every run — a retry re-spends quota; canonical round `--scope uncommitted`, full scope family accepted. `launch` resolves and injects a base for every working-tree scope, refusing with the persist remedy when none resolves, since the engine otherwise fails within a second while exiting 0. `--light` is coderabbit-only.
- [GREPTILE]: hunts cross-file logic over committed commits against a base; canonical round `--scope base:<prior-boundary>` after committing the slice, `committed` reviews against the repo default, and `base:`/`base-commit:` refs take any committish.
    - Scope is a base..HEAD range, so a cumulative campaign pins one base — `base-commit:<campaign-boundary>` held across rounds — and the whole accumulated delta reviews as one change regardless of commit count, with cross-round dedup and provenance keeping adjudicated rows out of lanes; local commits suffice for the CLI review, and a later push carries them to origin.
    - A pinned base holds until greptile's client-side size caps refuse, then the campaign falls back to slice commits at the prior boundary; `--resume` and `--include` are greptile-only passthroughs.
- [MACROSCOPE]: streams AST-level correctness in place; canonical round `--scope base:<default-branch>` spanning committed branch work and uncommitted edits, `uncommitted` for tree-only — an in-place run without a base on a branch reviews almost nothing. Always in place — fixes land in the files the review read — and `launch` preflight-sweeps stray review worktrees.

Terminal phases are `completed|failed|refused|stalled|timed-out|killed`; a receipt short of `completed` names its fault `signature`, `remedy`, and engine `exit_code`, and `status --follow --all-live` exits 0 only when every followed round completed. `--json` on any follow streams one `StatusReceipt` JSON line per phase change or minute pulse for a piping orchestrator; the terminal receipt prints either way.

Bare `status` resolves the single live round, else the highest-numbered on disk; several live rounds refuse `ambiguous` naming them — pass `--round`, `--reviewer`, or `--all-live`. `kill` (same selectors, same `no-round`/`ambiguous` selection refusals) escalates SIGTERM to SIGKILL over the process group, sweeps detached engine survivors and macroscope worktrees, and reaps a stalled or timed-out round whose engine still breathes; per round it refuses a gather round (`no-process`) and a round whose process group is truly gone (`already-closed`).

`findings --normalize` gates on `completed` and refuses an already-sliced round. Minting strips each engine's constant fix-preamble (one `ENGINE_PREAMBLES` row per preamble) and carries content fields only; the receipt `source` names the store a raw payload re-reads from.

Cross-round dedup runs against the latest prior round carrying lane reports — `--dedup-against N` pins an earlier pool — and dropped fingerprints leave only after the provenance histogram computes over the full set, so relitigation surfaces while lanes never re-fix adjudicated rows. Round 1 and a round after a zero-findings close have no qualifying prior and read `unstamped` — undecomposable, never a config failure.

`scope_misfire` on the receipt flags zero rows over a dirty scope; an exit-0 zero-comment envelope over a clean scope is ordinary success. Normalize refuses `dedup-collapse` when over half of eight or more admitted rows drop as intra-round fingerprint duplicates — engine format drift, never real duplication — the refusal `detail` carrying the admitted count, survivor count, and collapse share.

Two engine refusals are terminal for their round: `store-missing` (coderabbit) means the store epoch never correlated — relaunch, accepting the re-spend; `no-payload` (greptile) means no `--json` envelope reached the log — read the `stream.log` head for an unmatched auth or billing line, then relaunch.

Bare `findings` reprints the summary receipt; `--digest [--top N]` prints the round read, and `--file <glob>`, `--severity <floor>`, `--claim <regex>` filter the normalized set as a query, composable with `--digest` — digest and query print human-scan text, `--json` the typed form. `--in claim|fix|both` scopes the `--claim` regex (default both) and refuses without `--claim`. `gather` takes exactly one of `--all-live`, `--reviewer cr,gt,ms`, or `--rounds 1,2,3`; every source must be `completed` and normalized.

`slice [--lanes N] --balance count|loc` partitions by folder affinity — grouping deepens past common prefixes and splits oversized folders at sub-folder seams until lanes balance, so N lanes over at least N findings always cut N non-degenerate lanes. It clears stale lane artifacts only after the cut proves out, stamps round-scoped ids, and emits per-lane manifests carrying the corpus-matched settled-rulings roster and the `lane-<letter>-brief.md` path; a registry row tagged `corpus` slices only into lanes whose file set resolves to that corpus.

`slice --closers` instead keys one row per routed demand as `<lane-letter>:<target_file>`, groups by target file, and greedy-packs whole files into territories, so two closers never share a write target; an omitted `--lanes` derives the count, clamped to the distinct-file ceiling, and the cut never rewrites `findings.json`.

`reconcile` covers all lanes bare (`--all` is the explicit synonym; a named lane with `--all` refuses). `lanes` takes exactly one of `--fill` or `--spawns` with an optional `--lane <name>` filter (a bare letter reads as `lane-<letter>`); its spawn oracle is the codex rollout store — the per-lane events stream records zero spawns by design and is never consulted — and a `fault` of `no-events|no-session|no-rollout` is a per-lane fact, not a refusal.

`round` refuses a duplicate close and fails loud on findings without lane reports. `verify` takes exactly one of `--rule <text> [--path <file>]` (greptile cascade check) or `--round N` (all-surface ledger check). `selftest` proves the rail's pure contracts — report decode against the shipped maximal-shape fixture, registry checking, mint hygiene, query scoping, routing drain, closer cut, spawn scan, tier fill, grading — exiting nonzero naming each failed proof.

## [04]-[LANE_CONTRACT]

Fixer-lane doctrine lives in `templates/fix.md`. Per lane the rail writes `lane-<letter>.json` and `lane-<letter>-brief.md`; dispatch writes `task-<letter>.md`, `lane-<letter>-report.json`, `lane-<letter>-events.jsonl`, and `lane-<letter>-stderr.log`. Each report is the machine intake `reconcile`, `harvest`, and the reviewer-harvest agent parse, written to `<round-dir>/lane-<letter>-report.json` as the lane's final act:

```json signature
{
  "ledger": [{"id": "", "file": "", "severity": "", "verdict": "fixed|upgraded|pushed-back|already_resolved", "note": "", "shape": ""}],
  "improvements": [{"page": "", "pattern": "", "what": "", "axis": ""}],
  "refuted": [{"claim": "", "evidence": ""}],
  "capability": [],
  "routing": [],
  "uncertain": [],
  "files": [],
  "gate_clean": true,
  "model": "",
  "wall_s": 0.0
}
```

`axis` and `shape` are fixer-stamped metadata, never inferred — `shape` takes `substitution|collapse|completion` per repaired row, and a blank stamp counts under `unstamped` in `LaneStat.shapes`. `files` is the lane's actually-touched roster; the rail diffs it against the manifest into `strays`.

`routing` rows carry `target_file`/`needed_change`; a `capability` row carrying a `members`, `roster`, or `symbols` list feeds the `reference_*` memory proposals; only `uncertain` is free-shape, reprinted verbatim by the feed. `gate_clean` records the final batched format-gate run over the lane's touched files, and `model` keys the grade cohorts casefolded at reconcile.

Closer doctrine lives in `templates/close.md`. Per territory the rail writes `close-<letter>.json` and `close-<letter>-brief.md`; each closer writes `<round-dir>/close-<letter>-report.json`:

```json signature
{"rows": [{"id": "<lane-letter>:<target_file>", "verdict": "landed|refuted|already-landed|blocked", "note": ""}], "files": []}
```

One row per routed demand — `id` echoes the brief's stamped key (a bare target path in `id` or `file` also joins), and the drain counts a row answered under any verdict; `files` is the closer's touched roster.

## [05]-[DISTILL]

Reviewer-harvest owns the distill leg: dispatch on either transport under `templates/harvest.md`; it returns a typed surface ledger. Round dirs sit outside the agent's write territory, so project its landed guards into `<round-dir>/surface-ledger.json` yourself as `[{surface, text, path}]` rows — `surface` an engine name or alias, `text` the shortest guard substring unique on the surface, `path` blank defaulting per engine (`.coderabbit.yaml`; the greptile repo-wide cascade; a full-text scan across `.macroscope/**/*.md`).

Project one row per landed addition and consolidation, then `verify --round N` proves each row in its surface's oracle; a receipt whose `source` is `surface-ledger` marks a malformed row (blank text or an unmapped surface name), never failed wording.

- [REGISTRY]: `templates/refuted-classes.yaml` rows compile into the claim classifier, the recurrence detector, and the settled-rulings roster sliced into lane manifests by `corpus` tag; `harvest` proposes new rows in the feed, `registry --check --rows` proves them (matcher compile, schema, dedup), and `registry --apply` lands the clean set append-only; a judgment-bearing merge into an existing row stays a hand edit in the yaml, re-proved by bare `registry --check`, never in the script.
- [RAIL_GAPS]: a rail gap a round exposes lands on the rail's own script and data surfaces.
- [VERIFY]: `verify --round` re-proves each landed guard — a `.coderabbit.yaml` `path_instructions` clause by text, a greptile rule by substring over the resolved `greptile config` output (never by id — org rules re-key to server UUIDs) falling back to full-text over `.greptile/config.json` and `.greptile/rules.md` when the cascade truncates or a scoped rule never resolves at the probe path, the receipt `source` naming the oracle that matched, a macroscope topic file by presence and content.
- [FEED_OUTPUTS]: provenance and corroboration histograms feed review-the-reviewer — a class one engine raises and rounds keep refuting marks that engine's false-positive tendency, and multi-engine corroboration marks high-confidence work.
- [PROPOSALS]: `memory-proposals/` under the round dir carries candidate memory files the orchestrator curates, never the live memory dir — the campaign memory (`project_cr_review_cycle_machine`) is the destination owner, and a proposal duplicating a repo-owned fact dies at curation; the feed's `[GUARD_INEFFECTIVE]` section names classes recurring despite landed guards.

## [06]-[NON_GOALS]

- Rail territory ends at manifests, feeds, and proof — fixer and closer dispatch belong to the orchestrator, which judges slicing, models, and prompts.
- Watchers arm per round and die at terminal; no standing watcher, daemon, or per-event hook.
- Cross-reviewer merge stops at fingerprint dedup and the corroborator record — two engines disagreeing is signal, never noise to reconcile silently.
- `rounds.jsonl` is the health readout and the row-to-row delta the only progress instrument — jsonl and jq, never a dashboard.
