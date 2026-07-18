# [CODEX_LANE]

Fixer-lane dispatch form: one detached codex CLI process per lane, lane law on the developer channel, the task instance on the user prompt. Codex is maximally literal — every word earns its slot, no intensifiers, no persistence blocks, and the completion bar is the enumerated deliverables the lane law's output contract carries.

## [01]-[LAUNCH]

One `run_in_background` Bash keeper per lane — batched keepers lose per-lane completion notifications:

```bash template
codex exec -s workspace-write -m <model> --skip-git-repo-check \
  -c developer_instructions="$(cat <round-dir>/lane-law.md)" \
  -o <round-dir>/lane-<letter>-report.json \
  "$(cat <round-dir>/task-<letter>.md)" </dev/null 2><round-dir>/lane-<letter>-stderr.log >/dev/null
```

- `-c developer_instructions=` is the only CLI path to the developer role; `model_instructions_file` REPLACES codex's base instructions and never carries lane law.
- `</dev/null` is mandatory: codex reads piped stdin to EOF even with a prompt argument, and an open silent stdin hangs the lane forever with zero output.
- Purge stale report and stderr files before launch — a leftover report reads as instant completion carrying the prior run's data.

## [02]-[TASK_MESSAGE]

Every task message carries the instance plus the imperative miner-spawn step on the user prompt, because the injected spawn gate hears ONLY that channel: permissive wording ("you may spawn") fails it, and imperative wording naming the exact tools — `collaboration.spawn_agent`, `collaboration.wait_agent` — passes it.

```markdown template
<task>
Disposition every finding in <round-dir>/lane-<letter>.json. Your write territory is exactly these files:
- <sliced-file-1>
- <sliced-file-2>

Before your first edit, spawn exactly two parallel sub-agents with the collaboration.spawn_agent tool, then collect both with collaboration.wait_agent. Each spawn task is fully self-contained — spawned agents inherit none of your conversation, so state absolute paths and return shapes explicitly:
- miner-A (read-only): census <catalog-tier-paths> for members and capability the territory files under-use; return a candidate roster of rows {member, target_page, why}.
- miner-B (read-only): sweep the territory files for composition seams — repeated shapes two pages could share through one owner, under-parameterized owners, flat case families; return candidate rows {page, seam, opportunity}.
Miners never edit files and never propose code to paste; their rosters are data you judge under your capability mandate.
</task>
```

## [03]-[HEALTH_AND_AUDIT]

- Spawn health reads each lane's stderr banner within a minute of launch; a missing banner marks a dead lane — the stderr log carries the reason, and a blind relaunch races a live process.
- A spawn audit greps the PARENT rollout for `function_call` items named `spawn_agent` — children mint their own rollout files, and `collab_tool_call` is a stale marker yielding false negatives.
- A live process past its deadline with near-zero CPU is wedged: kill by report basename, relaunch once; a second wedge is the failure.
