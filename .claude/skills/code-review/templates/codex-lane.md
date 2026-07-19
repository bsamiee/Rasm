# [CODEX_LANE]

Fixer-lane dispatch form: one detached codex CLI process per lane, lane law on the developer channel, the task instance on the user prompt. Codex is maximally literal — every word earns its slot, no intensifiers, no persistence blocks, no chain-of-thought scaffolding, and the completion bar is the enumerated deliverables the lane law's output contract carries. Lane law rides named XML spec blocks (`<role>` through `<output_contract>` last) — the battery-measured highest-adherence prompt form on gpt-5.6; markdown bullets lose that adherence property.

On a prose corpus the lane-law `<corpus-idiom>` slot fills with the register projection embedded whole — a codex lane loads no skills, so a docgen pointer is inert; docgen stays the register owner and this is its task-scoped projection: every law sentence seats the owning subject with an ownership verb (mints, owns, folds, binds, admits, rejects) — supports/offers/provides rewrites to total ownership; no hedge, future-gate, narration, or process reference; one owner per fact — a second prose copy collapses to a pointer naming the owner and one consumed symbol; a nameable surface is a code span in its exact verified spelling; a frozen roster, count, or corpse litany cures to the extension rule stated where the anchor stood; every word load-bearing — identical law in a third fewer tokens, zero capability lost.

Every judgment a Claude lane reaches by loaded doctrine fills for codex as an explicit decision rule; two fill rules are round-proven:
- `<verification>` fills with the territory's exact suffix-to-check table — the batched prose gate for `.md`, `uv run ruff check` plus `uv run ty check` for `.py`, `shellcheck` for `.sh`, `swiftc -typecheck` for `.swift`, the bundle's own `scripts/` validator where one ships — one batched run after the final edit, and a suffix absent from the table gets no invented check.
- Verdict evidence names itself per ledger row: `already_resolved` cites the current-disk content that resolves it; `pushed-back` carries its refuting citation; a version or payload currency claim lands only after its live oracle confirms the value, an unconfirmed one riding the `uncertain` row instead of a guessed edit; a finding carrying two independent claims adjudicates each, the note naming both verdicts.
- A collapse or densification proves winner-completeness in its note: every decision, path, and precedence rule the collapsed copy carried is named with its surviving line, and a fact with no surviving line blocks the collapse — round-proven: a codex lane claimed "mechanics remain on the owners" while three custody facts survived nowhere.

## [01]-[LAUNCH]

One `run_in_background` Bash keeper per lane — batched keepers lose per-lane completion notifications:

```bash template
codex exec [-m <model>] [-c 'model_reasoning_effort="<tier>"'] \
  -c developer_instructions="$(cat <round-dir>/lane-law.md)" \
  -o <round-dir>/lane-<letter>-report.json \
  "$(cat <round-dir>/task-<letter>.md)" </dev/null 2><round-dir>/lane-<letter>-stderr.log >/dev/null
```

- `-c developer_instructions=` is the only CLI path to the developer role; `model_instructions_file` REPLACES codex's base instructions and never carries lane law.
- `</dev/null` is mandatory: codex reads piped stdin to EOF even with a prompt argument, and an open silent stdin hangs the lane forever with zero output.
- Purge stale report and stderr files before launch — a leftover report reads as instant completion carrying the prior run's data.
- A codex task block never names a report file: the JSON rides the final message and `-o` captures it. A task naming the `-o` path arms a clobber — the lane's own file-write lands first and the captured final message overwrites it; recovery is `codex exec resume <session-id>` re-emitting the file with no `-o`.
- Effort inherits the config default; state the tier only to deviate. Run under the full user config — miner spawns need the estate's multi-agent depth row.
- Fix waves run the config default unflagged — fable-parity proven over live rounds once the discernment blocks landed; deviate only for purely mechanical slices.

## [02]-[TASK_MESSAGE]

Every task message carries the instance plus the imperative miner-spawn step on the user prompt, because the injected spawn gate hears ONLY that channel: permissive wording ("you may spawn") fails it, and imperative wording naming the exact tools — `collaboration.spawn_agent`, `collaboration.wait_agent` — passes it. Spawn tasks spell the lane-law mining charges whole, since a spawned miner never sees the lane law.

```markdown template
<task>
Disposition every finding in <round-dir>/lane-<letter>-brief.md (ids and structured rows: <round-dir>/lane-<letter>.json). Your write territory is exactly these files:
- <sliced-file-1>
- <sliced-file-2>

Before your first edit, spawn exactly two parallel sub-agents with the collaboration.spawn_agent tool, then collect both with collaboration.wait_agent. Each spawn task must be fully self-contained — spawned agents inherit none of your conversation, so state absolute paths and return shapes explicitly:
- miner-A: census <catalog-paths: the .api catalogs for a code corpus; the doctrine references and owning charters for a prose corpus> for members, package capability, or owning law that the territory files above under-use; return a candidate roster of rows {member-or-law, target_page, why}.
- miner-B: sweep the territory files above for composition seams — repeated shapes one owner could carry across pages, under-parameterized owners, flat case families, duplicated law; return candidate rows {page, seam, opportunity}.
Miners never edit files and never propose code to paste; their rosters are data you judge under your capability mandate.
</task>
```

## [03]-[HEALTH_AND_AUDIT]

- A codex report's `model` field is self-reported and unknowable in-lane — the CLI banner sits outside the lane's context — so the stderr banner's `model:` line is truth and the orchestrator stamps it over the report at reconcile; a slug demand in the task prompt is dead weight.
- Spawn health reads each lane's stderr banner within a minute of launch — `Reading additional input from stdin...` prints pre-EOF even under `</dev/null` and is notice, not a hang; an empty stderr log marks a dead lane, the log tail carries the reason, and a blind relaunch races a live process.
- Liveness is `pgrep -f "lane-<letter>-report"`; an absent report under a live process is normal — never relaunch a live run.
- A spawn audit runs `rg '"name":"spawn_agent"' ~/.codex/sessions/<YYYY>/<MM>/<DD>/rollout-*.jsonl` against the PARENT rollout — the lane's stderr banner `session id: <uuid>` picks the file; children mint their own rollout files, and `collab_tool_call` is a stale marker yielding false negatives.
- A long-quiet live process with near-zero CPU (`ps -p <pid> -o time=`) is wedged: `pkill -f "lane-<letter>-report"`, relaunch once; a second wedge is the failure.
