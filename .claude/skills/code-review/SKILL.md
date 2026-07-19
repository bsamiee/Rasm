---
name: code-review
description: >-
    Owns the local review cycle over three engines — CodeRabbit, Greptile, Macroscope —
    across both corpora they review: code diffs and estate prose (skills, agent definitions,
    docs, CLAUDE.md). Deliverables are normalized findings, drained fixer lanes, and reviewer
    configs hardened each round — `.coderabbit.yaml`, `.greptile/`, `.macroscope/` — and the
    standing refuted-class registry. Trigger on any explicit review request, autonomously when
    landed work warrants review, on "run coderabbit", "run greptile", "run macroscope", "review
    my changes", "review the skills estate", and on any reviewer-config or refuted-class tuning.
    Hosted PR reviewer round-trips belong to pr-loop; prose law itself to docgen.
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

Every round runs COLLECT -> FIX -> DISPOSITION -> DISTILL on two custody lanes that never mix: work under review stays uncommitted or on its slice commits, while distillation — reviewer configs, `docs/`, `tools/` — commits and pushes to origin's default branch the moment it lands, because hosted engines read the indexed default branch and an unpushed guard protects nothing. Steps run in order; each carries what to watch and the knob that tunes it.

1. [CUSTODY]: commit non-review surfaces first — reviewer configs, `docs/`, `tools/`, `.claude/` infra — so the review scope holds exactly the work under review. WATCH: a reviewed-work file in the custody commit shrinks the next round's scope silently. KNOB: the commit-scope roster.
2. [LAUNCH]: `launch` per engine at its canonical scope ([03]), then run the receipt's `watch_cmd` through Bash `run_in_background`. Every diff-scoped engine shares one blind spot — an untouched file never enters review — so an estate round also runs the fleet-scope collectors beside the engines: `estate_audit.py` over the full skill and agent roots, its rows joining the round's finding pool as orchestrator-seeded lane rows. WATCH: the liveness line — `alive=yes` with a counting-down budget is healthy on blind engines. KNOB: engine choice and `--focus`.
3. [NORMALIZE]: `findings --normalize` per completed round; `gather` unions concurrent engines; `findings --digest` is the post-normalize read and the query filters answer targeted inspections — never hand-jq over findings.json. WATCH: the provenance histogram before any count judgment — a flat total decomposes into `relitigation|refuted_remint|new_work|late_discovery` before any config takes blame, and rising new-work beside falling relitigation is convergence. KNOB: `--dedup-against`.
4. [SLICE]: `slice --lanes N` into balanced per-lane manifests, each with its dispatch-ready `lane-<letter>-brief.md`. WATCH: severity and folder balance across lanes. KNOB: lane count, up to 12 sliced by folder ownership — small slices direct freed capacity into capability depth, never early finish.
5. [FIX]: one keeper per lane under `templates/fix.md` — the identical filled law block on every lane; a codex lane takes it as developer instructions with the filled task as the user prompt, a claude lane takes both in one Agent call, and transport mechanics beyond these facts are the codex skill's, never restated. Per-lane files under `<round-dir>`: `lane-<letter>-brief.md`, `lane-<letter>.json`, `task-<letter>.md`, `lane-<letter>-report.json`, `lane-<letter>-stderr.log` (codex). Codex lanes run the config-default model and effort unflagged under the full user config (miner spawns need the multi-agent depth row; deviate only for purely mechanical slices); a codex task never names the report file — the captured final message IS the report, a violation recovering via `codex exec resume <session-id>` re-emitting with no `-o` — and the report's self-stated `model` re-stamps from the stderr banner at reconcile. A claude lane writes its report itself and returns only the path. Each lane returns only its report path, so report bodies never enter the orchestrator's context. Lane mix follows the graduation law: a corpus NEW to the machine runs its first fix wave on claude lanes to mint the quality baseline; the first codex wave on that corpus runs mixed — one claude lane beside codex lanes on comparable slices — and grades codex against the baseline on the standing [GRADING] axes; an observed shortfall hardens the universal template as an explicit decision rule, never a retreat to claude-only; at parity the next wave runs 100% codex, and the intermediate ratio widens only while shortfalls persist. WATCH: codex — stderr banner within a minute, spawn audit, an absent report under a live process is normal (never relaunch a live run; a wedged lane relaunches once, a second wedge closes its rows under `templates/close.md`); claude — the report on disk at return (prose instead of the path relaunches once fresh; a second failure routes to a closer). KNOB: lane mix and template wording — sharpen by replacement, never accumulate.
6. [RECONCILE]: `reconcile` proves per-lane id bijection and emits the verdict histogram. WATCH: verdict-mix honesty — fixed-versus-upgraded inflation, citation-backed push-back share. KNOB: a dropped finding closes through one small closer armed with the slice's stack doctrine, never a session resume.
7. [CLOSERS]: routing rows drain concurrently over disjoint territories under `templates/close.md` — a small closer for single-file mechanical rows, a full-arming closer for family, design, or capability work, sized by row weight. WATCH: honest land-versus-refute — a mined candidate that cannot ground is refuted with its citation, never forced. KNOB: closer sizing.
8. [DISTILL]: `harvest` assembles the feed and memory proposals; dispatch under `templates/harvest.md` — claude rides `Agent(subagent_type: "reviewer-harvest")` with the filled round slots, codex rides the template's law block with the slots as the user prompt. WATCH: diff-sample the touched blocks — density rose (fewer words, same law), integrations weave rather than append, additions passed the earn test; the ledger's `trimmed` self-report verifies against the actual diff, never trusted alone. KNOB: the agent file — the single versioned tuning surface.
9. [VERIFY]: project the agent's ledger into `<round-dir>/surface-ledger.json` ([05]), then `verify --round N` proves each guard in its surface's own oracle. WATCH: an ineffective row marks failed wording — harden the owner, never re-skip. KNOB: `templates/refuted-classes.yaml` rows.
10. [CLOSE]: commit and push the distillation lane, then `round` appends the `rounds.jsonl` row and prints the round-over-round delta; a zero-findings round arrives here straight from normalize and closes clean, and round grades, engine verdicts, and refuted-class calibrations land on the campaign memory (`project_cr_review_cycle_machine`) — orchestrator-curated under the harness-steering mining law, adjudicated rows merged into the owning verdict, never per-round narration. A campaign close curates the memory itself: accumulated round entries collapse to universal law carrying at most one calibration each (per-round numbers stay in `rounds.jsonl`), chaff and superseded guidance delete with their index lines, and near-similar entries restructure at the owning bullet under the harness-steering consolidation law, never appended siblings. WATCH: findings trending down while capability rows rise is the goal line. KNOB: none — the ledger is the sole progress instrument.
11. [NEXT]: grade the round on the [GRADING] axes, then pick the next engine — recurrence judges per engine, counts flattening under one engine rotate the next round to another, and `--focus` aims a round within one: greptile rides early rounds while the accumulated diff fits its size caps, coderabbit carries breadth at any diff size, macroscope joins whenever its lane is open. WATCH: plateau under a hardened config. KNOB: rotation and focus.

- [FRAMING]: both framings ride every surface — negative framing kills false-positive classes through do-not-flag guards citing the refuting ruling, positive framing steers toward house demands through hunt axes and hit-shape rosters. Hit-shape rosters land on every surface regardless of what an engine emits, because fixer miners execute them — the flywheel: fixer discoveries become next-round hit-shapes.
- [GRADING]: REVIEW-THE-REVIEWER grades every round on the round row's typed axis fields — `fp_share`, `relitigation_share`, `novel_quality` (accepted-verdict share over novel-provenance rows), `hunt_axis_fire` (per-axis fire counts) — ledger-recorded so config changes read before-and-after mechanically. Standing verdict: CodeRabbit emits no missing-capability findings under any `path_instructions` wording — capability-direction rides Macroscope check-run agents and Greptile structured rules.
    - Finding strength scores a per-round sample 0-4: anchored (file and non-zero range, rail-stamped), actionable (repair direction, rail-stamped), discriminating (the claim states why the shape is wrong), novel (no adjudicated-class relitigation); a sub-2 mean marks a noise lane.
    - Fixer lanes grade on five axes per model from the round row's `by_model` rollup — depth-of-fix (`upgraded` over `fixed` + `upgraded`), scope discipline (phantom-clean, routing-honest), refute quality (citation-backed push-backs), gate cleanliness (`gate_clean`), verdict-mix honesty (id bijection, upgraded claims surviving diff re-read) — so the claude-versus-codex comparison is one jq group.
    - Grades drive the tuning loop: a template, profile, or config edit lands between rounds and logs against the round it precedes, so the next round's delta reads as prompt-change to behavior-change mechanically. Grades stay truthful — an inflated grade poisons the attribution, and a competent-but-unremarkable lane grades exactly that; a depth shortfall tunes the dispatch template's mandate, never the grade.
    - Prose FP classes live as `corpus: prose` registry rows citing the docgen defect catalog, graded by the same recurrence machinery.

## [03]-[USAGE]

```bash template
RAIL="uv run ${CLAUDE_SKILL_DIR}/scripts/review_rail.py"
$RAIL launch --reviewer <engine> --scope <scope> [--focus <text-or-file>]   # receipt: round N, pgid, watch_cmd
# run the receipt's watch_cmd through Bash run_in_background; its exit re-invokes the agent
$RAIL status --follow --round <N>
$RAIL findings --normalize --round <N> [--dedup-against <M>]
$RAIL findings --digest --round <N>
$RAIL findings --file '<glob>' --severity <floor> --claim '<regex>' --round <N> [--json]
$RAIL slice --lanes 3 --round <N>
# fixer lanes run here (orchestrator dispatch): each prompt cites its lane-<letter>-brief.md; each lane writes <round-dir>/lane-<letter>-report.json
$RAIL reconcile --round <N>
$RAIL harvest --round <N>
# dispatch the reviewer-harvest agent on the feed; persist its ledger as <round-dir>/surface-ledger.json
$RAIL registry --check --rows <path>   # validate the agent's proposed registry rows against the standing yaml
$RAIL registry --apply --rows <path>   # append-only land of fully-clean rows; any fault refuses whole
$RAIL verify --round <N>
$RAIL round --round <N>
```

```bash template
$RAIL launch --reviewer coderabbit --scope uncommitted
$RAIL launch --reviewer greptile --scope base:<prior-boundary>
$RAIL launch --reviewer macroscope --scope base:<default-branch>
$RAIL status --follow --all-live            # run_in_background; exits when every live round is terminal
$RAIL findings --normalize --round <N>      # once per engine round
$RAIL gather --all-live                     # union round: fingerprint collapse, corroborators, max severity
# slice / reconcile / harvest / verify / round then target the gather round
```

Every verb accepts `--round N` and `--dir` and prints one JSON receipt; a refusal exits nonzero carrying `{code, detail}`. `launch` refuses an unsupported engine-scope pair and a live same-engine round loudly; the scope family is `all|committed|uncommitted|base:<ref>|base-commit:<sha>`, and the `--reviewer` selectors on `status`/`kill`/`gather` also take `cr|gt|ms`. `--focus` takes inline text or a file path — greptile rides `--instructions`, coderabbit a round-scoped `-c` instruction file, and macroscope refuses focus loudly because config files are its only steering.

Engine scope selection; each reference owns its engine's mechanics — base resolution, size gating, language routing, and config surface.

- [CODERABBIT]: sweeps working-tree quality and style at full breadth every run — a retry re-spends quota; canonical round `--scope uncommitted`, full scope family accepted. `launch` resolves and injects a base for every working-tree scope, refusing with the persist remedy when none resolves, since the engine otherwise fails within a second while exiting 0. `--light` is the cheap post-fix re-review lever, coderabbit-only.
- [GREPTILE]: hunts cross-file logic over committed commits against a base; canonical round `--scope base:<prior-boundary>` after committing the slice, `committed` reviews against the repo default, and `base:`/`base-commit:` refs take any committish. Scope is a base..HEAD range, so a cumulative campaign pins one base — `base-commit:<campaign-boundary>` held across rounds — and the whole accumulated delta reviews as one change regardless of commit count, with cross-round dedup and provenance keeping adjudicated rows out of lanes; local commits suffice for the CLI review, and a later push carries them to origin. A quota-free client-side preflight refuses an oversized range instantly — the pin holds until the caps refuse, then the campaign falls back to slice commits at the prior boundary. `--resume` and `--include` are greptile-only passthroughs.
- [MACROSCOPE]: streams AST-level correctness in place; canonical round `--scope base:<default-branch>` spanning committed branch work and uncommitted edits, `uncommitted` for tree-only — an in-place run without a base on a branch reviews almost nothing. Always in place — fixes land in the files the review read — and `launch` preflight-sweeps stray review worktrees. Config files are its only steering channel.

Waiting is one mechanic: run the launch receipt's `watch_cmd` through Bash `run_in_background` — never foreground, never a per-turn `status` poll. Its loop prints a liveness line each minute and self-exits at the terminal phase. Blind engines — greptile and macroscope — stream nothing between launch and the terminal dump: quiet is healthy, `alive=yes` with a counting-down `budget_s` is the health read, the deadline the sole timeout; coderabbit, the keepalive engine, also converts heartbeat silence past its grace to `stalled`.

Terminal phases are `completed|failed|refused|stalled|timed-out|killed`; a receipt short of `completed` names its fault `signature`, `remedy`, and engine `exit_code`, and `status --follow --all-live` exits 0 only when every followed round completed.

Bare `status` resolves the single live round, else the highest-numbered on disk; several live rounds refuse `ambiguous` naming them — pass `--round`, `--reviewer`, or `--all-live`. `kill` (same selectors) escalates SIGTERM to SIGKILL over the process group, sweeps detached survivors and macroscope worktrees, and reaps a wedged stalled or timed-out round whose engine still breathes; it refuses only a gather round (`no-process`) and a round whose process group is truly gone (`already-closed`).

`findings --normalize` gates on `completed` and refuses an already-sliced round. Cross-round dedup runs against the latest prior round carrying lane reports — `--dedup-against N` pins the pool — and dropped fingerprints leave only after the provenance histogram (stamped against that round's ledger) computes over the full set, so relitigation surfaces while lanes never re-fix adjudicated rows.

`scope_misfire` on the receipt flags zero rows over a dirty scope; an exit-0 zero-comment envelope over a clean scope is ordinary success. Normalize refuses `dedup-collapse` when over half of eight or more admitted rows fold to one fingerprint pool — engine format drift, never real duplication — the receipt carrying `admitted` and `collapse_ratio`.

Bare `findings` reprints the summary receipt from disk; `--digest [--top N]` prints the round read, and `--file <glob>`, `--severity <floor>`, `--claim <regex>` filter the normalized set as a query, composable with `--digest` — digest and query print human-scan text, `--json` the typed form. `gather` takes exactly one of `--all-live`, `--reviewer cr,gt,ms`, or `--rounds 1,2,3`; every source must be `completed` and normalized.

`slice --lanes N --balance count|loc` partitions by adaptive folder affinity — grouping deepens past common prefixes and splits oversized folders at sub-folder seams until lanes balance, so N lanes over at least N findings always cut N non-degenerate lanes. It clears stale lane files and briefs, stamps round-scoped ids, and emits per-lane manifests carrying the corpus-matched settled-rulings roster and a dispatch-ready `lane-<letter>-brief.md` whose path the manifest carries; a registry row tagged `corpus` slices only into lanes whose file set resolves to that corpus.

`reconcile` covers all lanes bare (`--all` is the explicit synonym; a named lane with `--all` refuses). `round` refuses a duplicate close, fails loud on findings without lane reports, and closes a zero-findings round clean. `verify` takes exactly one of `--rule <text> [--path <file>]` (greptile cascade check) or `--round N` (all-surface ledger check). `selftest` proves the lane-report decode contract against the rail's shipped maximal-shape fixture and exits nonzero naming each failed proof.

## [04]-[LANE_CONTRACT]

Fixer-lane doctrine lives in `templates/fix.md`; each lane's report is the machine intake `reconcile`, `harvest`, and the reviewer-harvest agent parse, written to `<round-dir>/lane-<letter>-report.json` as the lane's final act:

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

`axis` is fixer-stamped hunt-axis metadata, never inferred; `capability`/`routing`/`uncertain` rows are free-shape JSON the harvest feed reprints; `gate_clean` records the final batched format-gate run over the lane's touched files.

## [05]-[DISTILL]

Reviewer-harvest owns the distill leg: dispatch on either transport under `templates/harvest.md`; it returns a typed surface ledger. Round dirs sit outside the agent's write territory, so project its landed guards into `<round-dir>/surface-ledger.json` yourself as `[{surface, text, path}]` rows — `surface` an engine name or alias, `text` the guard substring, `path` blank defaulting per engine (`.coderabbit.yaml`; the greptile repo-wide cascade; a macroscope topic file under `.macroscope/`) — then `verify --round N` proves each row in its surface's oracle.

- [REGISTRY]: `templates/refuted-classes.yaml` rows compile into the claim classifier, the recurrence detector, and the settled-rulings roster sliced into lane manifests by `corpus` tag; `harvest` proposes new rows in the feed, `registry --check --rows` proves them (matcher compile, schema, dedup — the loud complement to the classifier's lenient runtime matching), and `registry --apply` lands the clean set append-only; a judgment-bearing merge into an existing row stays a hand edit in the yaml, never in the script.
- [RAIL_GAPS]: a rail gap a round exposes lands on the rail's own script and data surfaces.
- [VERIFY]: `verify --round` re-proves each landed guard — a `.coderabbit.yaml` `path_instructions` clause by text, a greptile rule by substring over the resolved `greptile config` output (never by id — org rules re-key to server UUIDs) falling back to full-text over `.greptile/config.json` and `.greptile/rules.md` when the cascade truncates or a scoped rule never resolves at the probe path, the receipt `source` naming the oracle that matched, a macroscope topic file by presence and content.
- [FEED_OUTPUTS]: provenance and corroboration histograms feed review-the-reviewer — a class one engine raises and rounds keep refuting marks that engine's false-positive tendency, and multi-engine corroboration marks high-confidence work.
- [PROPOSALS]: `memory-proposals/` under the round dir carries candidate memory files the orchestrator curates, never the live memory dir — the campaign memory (`project_cr_review_cycle_machine`) is the destination owner, and a proposal duplicating a repo-owned fact dies at curation; the feed's `[GUARD_INEFFECTIVE]` section names classes recurring despite landed guards.

## [06]-[NON_GOALS]

- Rail territory ends at manifests and feeds — fixer dispatch belongs to the orchestrator, which judges slicing, models, and prompts.
- Watchers arm per round and die at terminal; no standing watcher, daemon, or per-event hook.
- Cross-reviewer merge stops at fingerprint dedup and the corroborator record — two engines disagreeing is signal, never noise to reconcile silently.
- `rounds.jsonl` is the health readout and the row-to-row delta the only progress instrument — jsonl and jq, never a dashboard.
