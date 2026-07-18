---
name: codex
description: Dispatch self-contained work to Codex (gpt-5.6 Sol/Terra) through the `codex` MCP tool or the `codex` CLI — an isolated peer agent returning one report; writes, design, hard reviews, fleet fix waves, research, and volume extraction all ride it. Trigger on repo sweeps, audits, investigation, log or dataset distillation, live web research, implementation from spec, migrations, a second perspective on any plan or diff, any mention of codex, gpt-5.6, Sol, Terra, or OpenAI models, and any request to offload work or conserve usage. Claude-surface delegation belongs to agent-dispatch; Gemini calls belong to agy.
---

# [CODEX]

`codex exec` runs a gpt-5.6 model as a non-interactive peer agent in its own context window and returns one final message. Every lane states model and sandbox explicitly; `~/.codex/config.toml` carries the operator's interactive stance, its `model_reasoning_effort` row the dispatch default.

## [01]-[ROUTING]

- [01]-[PROMPTING](references/prompting.md): block vocabulary, lane-shape templates, calibration law, exclusions — the authoring surface for every codex prompt
- [02]-[META_MANAGEMENT](references/meta-management.md): skills, custom agents, MCP membership and health, config ownership

## [02]-[DISPATCH]

- Investigation legs whose transcripts flood this context — repo sweeps, audits, log distillation, data analysis: codex returns the conclusion.
- Fleet fix and critique waves: concurrent workspace-write lanes draining findings under lane law, each with disjoint write territory.
- Critique lanes writing on-disk fixlogs, reports, or dossiers another agent consumes downstream.
- Implementation from spec: migrations, renames, conversions, boilerplate expansion, full features with enumerated moves.
- Independent second perspective on a plan, implementation, or diff — a different model lineage catches different failure modes.
- Research legs needing live sources (`-c web_search="live"`), plus long-running work that must not occupy this session.

Main-loop legs run as background CLI keepers — a blocking MCP call freezes the session with zero visibility; subagents with nothing else to do take the blocking MCP call. Image legs ride the CLI `-i`; the MCP tool takes no image parameter.

## [03]-[INVOCATION]

MCP surface — fleet server `codex`, tools `codex` and `codex-reply`; the prompt rides a tool argument, no shell quoting:

- Parameters: `prompt` (required), `model`, `sandbox`, `cwd`, `approval-policy`, `config` (object — effort deviates via `{"model_reasoning_effort": "..."}`), `developer-instructions` (the lane-law channel), `base-instructions`, `compact-prompt`. `sandbox` has no schema default — pass it on every call; a headless dispatch passes `approval-policy: "never"`.
- `base-instructions` (MCP) / `model_instructions_file` (CLI) REPLACE the shipped system prompt — deliberate use only, never a lane-law channel.
- Result envelope `{threadId, content}`: parse `content` for the final message — a consumer treating the raw result as the product double-encodes every downstream read; `threadId` feeds `codex-reply` (`conversationId` is its deprecated alias).
- Concurrent calls on the one server process complete independently; a blocking call returns when the turn completes.
- A heavy MCP tool call issued from INSIDE a codex turn can wedge the lane — pin nested lookups to light calls (`get_latest_package_version`, never `get_package_context`).
- MCP tool calls serialize within a lane; CLI lanes with native tools parallelize reads.

CLI surface — the default form inherits the config effort (high); the second form pins a deviation:

```bash template
codex exec -s <sandbox> -m <model> [-c developer_instructions="<lane-law>"] --skip-git-repo-check [-C <dir>] [-o <report>] "<prompt>" </dev/null 2>/dev/null
```

```bash template
codex exec -s <sandbox> -m <model> -c 'model_reasoning_effort="<tier>"' [-c developer_instructions="<lane-law>"] --skip-git-repo-check [-C <dir>] [-o <report>] "<prompt>" </dev/null 2>/dev/null
```

- `</dev/null` is mandatory from a harness: `codex exec` reads piped stdin to EOF even with a prompt argument — an open-but-silent stdin blocks forever; the stderr `Reading additional input from stdin...` notice is pre-EOF, not a hang.
- Final message prints to stdout, banner and reasoning to stderr — keep stderr only when diagnosing a failing run.
- Synchronous stdout capture is the default — `out=$(codex exec … "<prompt>" </dev/null 2>/dev/null)` — inside the 10-minute Bash ceiling; `-o` when backgrounding.
- `-c developer_instructions="<lane law>"` lands the developer-role message — `-c` is the only CLI path, no flag exists.
- Task and schema files land through a real file-write at ABSOLUTE paths, never a shell heredoc — cwd drift plus heredoc quoting land files where codex cannot find them.
- A short synchronous leg needs zero files: inline prompt, stdout capture.

| [INDEX] | [NEED]                                        | [FLAGS]                                                                               |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Read, analyze, research                       | `-s read-only` — blocks `uv run`; uv needs row [02] plus `UV_CACHE_DIR=.cache/uv`     |
|  [02]   | Edit files in the workspace                   | `-s workspace-write` — `.git/` stays write-blocked; default uv cache is off-workspace |
|  [03]   | Extra writable roots alongside the workspace  | `--add-dir <dir>`                                                                     |
|  [04]   | Network or system access beyond the workspace | `-s danger-full-access` — only with explicit user authorization                       |
|  [05]   | Run rooted in another directory               | `-C <dir>`                                                                            |
|  [06]   | Durable report artifact                       | `-o <file>` — an empty file means the process was killed before completion            |
|  [07]   | Typed JSON final message                      | `--output-schema <schema.json>` — validated final-message shape                       |
|  [08]   | Live web search                               | `-c web_search="live"` — default `cached` answers from an index, no live fetch        |
|  [09]   | Attach images (screenshots, diagrams)         | `-i <file>` (repeatable)                                                              |
|  [10]   | Feature toggles per lane                      | `--enable <feature>` / `--disable <feature>` (repeatable)                             |
|  [11]   | Fan-out legs with no session persistence      | `--ephemeral` — not resumable                                                         |
|  [12]   | Streamed JSONL events for live monitoring     | `--json` (thread/turn/item events to stdout; composes with `-o`)                      |
|  [13]   | Per-lane completion push                      | `-c 'notify=["<sink>","<lane>"]'` — fires at turn end with a JSON payload             |

## [04]-[MODEL_AND_EFFORT]

Every lane pins the model; effort inherits the config default (high), stated only to deviate. Bare `gpt-5.6` is an API-only slug that 400s under ChatGPT auth — always the suffixed name.

Sol is the primary model, carrying roughly 80% of dispatch: every non-trivial write, deep design, hard reviews, whole fix/critique fleets, and second perspectives. Sol at `medium` covers menial file writes — api catalogs and their kin — over terra at any tier, and sol at `low` covers volume extraction and classification. Terra runs at `medium` or lower for navigation, exploration, and research. `gpt-5.6-luna` is NEVER dispatched.

| [INDEX] | [MODEL]         | [ROLE]                                                                                        |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `gpt-5.6-sol`   | primary — writes, design, reviews, fleets; `medium` menial writes, `low` extraction            |
|  [02]   | `gpt-5.6-terra` | navigation, exploration, research at `medium` or lower                                         |

Sol carries the family's highest measured reward-hacking rate — high tiers tighten sandbox and approval, never relax them. Each effort row is the flag to deviate to it:

| [INDEX] | [TIER] | [SELECT]                             | [USE]                                                             |
| :-----: | :----- | :----------------------------------- | :---------------------------------------------------------------- |
|  [01]   | low    | `-c model_reasoning_effort="low"`    | trivial glue: probes, extraction, classification, relabels        |
|  [02]   | medium | `-c model_reasoning_effort="medium"` | menial writes and fan-out legs where throughput beats depth       |
|  [03]   | high   | `-c model_reasoning_effort="high"`   | the config default — research, review, and write legs             |
|  [04]   | xhigh  | `-c model_reasoning_effort="xhigh"`  | deeper single-agent reasoning for the hardest investigation, design, and review legs |

- Deviate surgically, one axis at a time: xhigh deepens the single hardest leg, low/medium serve throughput; latency tracks task shape, not tier.
- Overrun means CIRCLING — re-verifying unchanged work, loops adding no evidence — never duration; a well-scoped high-tier leg legitimately runs an hour.
- Circling resolves in fixed order: land the missing completion bar first; still circling, downshift to medium; added instructions come last, one at a time.
- Upshift is the mirror — xhigh earns its latency on review-shaped legs; an upshift without a completion bar buys longer runs, not better ones.
- Subagent concurrency and depth are config rows read live, never assumed — the estate pins 12 slots and depth 1; tables are meta-management's.

## [05]-[FLEET]

A lane runs the operator config whole — MCP fleet, skills, notify. Selection narrows only where a real hazard bites:

- FAIL-CLOSED: a `required = true` server missing its startup handshake kills the run at session creation — `thread/start failed`, exit 1, no model turn, ZERO JSONL events. Rescue with `-c 'mcp_servers.<name>.enabled=false'`. Disable keys ride UNQUOTED — a TOML-quoted key mints a phantom transportless server that kills config load; `-c 'mcp_servers={}'` is a merge no-op.
- HEADLESS CALLABILITY: `codex exec` runs at `approval: never`; callability combines tool annotations with `default_tools_approval_mode`. Annotated read-only tools call cleanly; an unannotated tool calls only under mode `approve` — under `auto`/`writes` the call cancels mid-flight, and codex may FABRICATE a plausible result instead of surfacing the cancellation. Permanent `approve` rows serve pure information-retrieval servers; a per-lane grant rides `-c 'mcp_servers.<name>.default_tools_approval_mode="approve"'`, `tools.<tool>.approval_mode` narrows to one tool, `disabled_tools` is the per-tool deny list. NEVER grant `approve` to a write-capable server — MCP servers run OUTSIDE the sandbox. Prove a server before betting a lane on it: one probe call returning the raw result or exact error settles callability.
- Fan-out lanes disable `heptabase-mcp` — codex never serializes rotating refresh-token transactions across concurrent processes.
- No flag suppresses the AGENTS.md instruction chain; `project_doc_max_bytes` bounds it root-to-leaf, and an oversized global `~/.codex/AGENTS.md` silently evicts every repo file below it. Per-lane scope bounds ride `developer-instructions` — truncation-immune, above the user-role chain.
- Precedence: `-c` flag over project file over user config, composing freely under `--profile`; project trust activates only through the user-config `[projects]` table.

## [06]-[PROMPT_CONTRACT]

Codex shares no conversation state — every prompt is self-contained: Goal, Context, Constraints, Done-when. references/prompting.md owns block vocabulary, lane-shape templates, and calibration; laws below bind every dispatch:

- ROLE SPLIT: durable lane law — mandate, territory, output contract — rides the developer channel; the `prompt` carries only the task instance plus any imperative spawn step.
- STRUCTURE: named XML spec blocks on the developer channel are the highest-adherence form; the output contract sits LAST.
- LEAN: leaner prompts measure better on gpt-5.6 — one directive per concern, no restated codex defaults, no intensifiers, no chain-of-thought scaffolding, no persistence blocks. Reserve ALWAYS/NEVER for true invariants; phrase judgment calls as decision rules.
- AUTONOMY ONCE: state the autonomy policy a single time, naming safe actions — repeated "ask first" or "do not mutate" phrasing causes spurious approval pauses.
- STOP RULES: every lane states its completion bar — enumerated deliverables, each proven by observable evidence from the real surface, never "checks ran". That bar bounds scope, not depth; beyond-bar findings land as typed residue rows. A bare "fix everything you find" mandate at high effort is the failure regime.
- LAYER PIN: a long leg names its layer — research, design, implementation, review — and never migrates out; out-of-layer discoveries land as typed rows in the product.
- SCOPE: `<design_and_scope_constraints>` is the default block — implement EXACTLY and ONLY the named moves, simplest valid interpretation of any ambiguity, resolution by instruction never by clarifying questions.
- CONTRACT: `--output-schema` enforces the final-message shape at the CLI — STRICT: every key in `properties` also in `required`, `additionalProperties: false`; anything less degrades silently to unvalidated output. A leg whose output a Claude agent reads takes a prose JSON contract instead — the word "JSON", the exact shape, "no prose outside the JSON, no code fences", null-for-missing.

## [07]-[BACKGROUND]

A main-loop leg runs codex SYNCHRONOUSLY inside a `run_in_background` Bash task — that keeper is the lane's owner, its exit re-invokes the agent. A foreground Bash call reaps its detached children minutes after returning, killing lanes reportless mid-run. Codex emits its result only at completion; the lane runs until it finishes.

```bash template
codex exec -s <sandbox> -m <model> --skip-git-repo-check --json -o <report> \
  -c 'notify=["<sink>","<lane>"]' "<prompt>" </dev/null ><events.jsonl> 2><stderr.log> &
```

- A detached leg owns one ephemeral folder: `task.md`, `schema.json`, `report.json` (`-o`), `stderr.log`, `events.jsonl` (`--json`). Purge stale report and stderr before launch — a leftover report reads as instant completion carrying last run's data. A bare `&` survives the launching shell; never prepend `nohup`.
- Event stream: `thread.started{thread_id}` (the resume id), `turn.started`, `item.started`/`item.completed`, then `turn.completed` or `turn.failed`; `agent_message` carries the final text, `command_execution` carries `{command, aggregated_output, exit_code, status}`. An `item.type=="error"` item is NOT a run failure — the skills-budget warning rides one on every run under a large library; classify by turn events only.
- `turn.completed.usage` carries `input_tokens`, `cached_input_tokens`, `output_tokens`, `reasoning_output_tokens` — the fleet token ledger, summed across lanes' events files.
- `notify` runs the sink at turn end with one JSON argument: `{"type":"agent-turn-complete","thread-id":…,"turn-id":…,"cwd":…,"client":"codex_exec","input-messages":[…],"last-assistant-message":…}`. One estate sink appends `"$@"` plus a timestamp to one fleet `events.log` — one ledger line per completion.
- CLASSIFICATION, per lane in order: `turn.completed` plus a non-empty report → READY; `turn.failed` → FAILED with the event message; process gone plus empty events → DEAD, stderr tail is the reason; process alive with no turn event → RUNNING. Pre-thread deaths — config errors, required-MCP init — emit ZERO JSONL and never fire notify.
- Liveness is `pgrep -f "<report-basename>"`; health is not liveness — a long-quiet process with no report and near-zero CPU (`ps -p <pid> -o time=`) is WEDGED. Kill (`pkill -f`) and relaunch once; a second wedge is the failure.
- A fleet runs as ONE launcher script in a `run_in_background` task: prompts on disk, lanes pooled with `wait -n` under the concurrency cap, sink and ledger written by the same keeper. A keeper's exit code proves nothing about lanes — verify within a minute of launch: `pgrep -f 'codex exec'` count near the cap plus a clean first stderr log.

## [08]-[FAN_OUT]

Independent scopes run as concurrent `codex exec` processes, one per scope, each fleet-grade with its own artifacts; reconcile reports after all complete. Sandbox isolation is per-process — concurrent workspace-write runs against overlapping paths collide; partition write scopes or serialize.

Inside ONE lane, `collaboration.*` is the subagent tool family — `spawn_agent`, `followup_task`, `send_message`, `wait_agent`, `interrupt_agent`, `list_agents` (rollout-verified names; official docs list a different, stale set — never "correct" toward it). Children share the workspace live, inherit sandbox and model, and inherit NO conversation turns unless `fork_turns` opts in — a spawn task is self-contained like any codex prompt.

- GATE: an injected developer-role gate admits children ONLY on imperative spawn wording in the user prompt or AGENTS.md/skill chain — exact tool names, count, wait, per-child return shape. Permissive "you may spawn" and `developer_instructions` mandates fail silently with zero spawns.
- AUDIT: grep the parent rollout for `function_call` items named `spawn_agent` — `collab_tool_call` is a stale marker that false-negatives; the sessions store's `thread_spawn_edges` table is the version-stable SQL audit path.
- Worthwhile shapes are read-only star fan-outs — miners, censuses, verifiers returning distilled results; anti-patterns are parallel writers on the shared tree plus delegated fix-judgment — findings are signals, the lane owns the judgment.
- A row-shaped sweep rides ONE lane's `spawn_agents_on_csv`: one worker per CSV row under the concurrency cap, `output_schema` typing each row; the `output_csv_path` export lands only under `workspace-write`. Reusable personas are `~/.codex/agents/*.toml` files spawned by name.
- A `workspace-write` lane writes its own report as its final act; the caller verifies (`jq -e`) — re-emitting a codex product through another agent's Write is lossy at scale.

## [09]-[SESSIONS]

Every run mints a resumable thread on both surfaces — capture the id at dispatch whenever follow-up is plausible (the lane receipt persists it), and run iterative deep work as ONE thread continued with sharpened prompts, never fresh runs re-paying the exploration cost.

- MCP: the envelope carries `threadId`; continue with `codex-reply`. CLI: the banner prints `session id: <uuid>` (= `thread.started.thread_id` under `--json`); resume with `codex exec resume <id> "<follow-up>" </dev/null 2>/dev/null`, config flags between `exec` and `resume`; `resume --all` surfaces sessions recorded under another cwd.
- A continuation inherits model, effort, and sandbox. `resume --last` is valid only when nothing else ran in between; under concurrency resume by explicit id. `--ephemeral` runs record nothing.
- Store: one rollout per session at `~/.codex/sessions/<YYYY>/<MM>/<DD>/rollout-<ts>-<uuidv7>.jsonl` — the filename timestamp is LOCAL time, the payload's is UTC; `session_index.jsonl` LAGS live runs, so correlate by datestamped filename. Sqlite store filenames embed the migration version (`state_5` today) — resolve the live name, never pin the number.
- `codex archive <id>` / `unarchive <id>` is the reversible lifecycle; `codex delete` removes one session, its `--force` unreliable across spawned children — bulk cleanup is a date-scoped sqlite prune plus matching rollout deletions plus `VACUUM`. `[features] memories`/`chronicle` rows gate those features independently of corpus deletion.

## [10]-[REVIEW]

`codex review` runs an independent non-interactive review. Scope flags — `--uncommitted`, `--base <branch>`, `--commit <sha>` — are mutually exclusive with each other AND with a focus prompt: a prompt is valid only on bare `codex review`, and every focused scoped review routes through `codex exec` with an explicit diff task. A fleet-grade review lane runs `codex exec review` — the same scope flags plus `--json`, `-o`, and `--output-schema` typed findings, composing with the background keeper like any lane.

## [11]-[FAILURE]

- Quota exhaustion fails the call LOUDLY with no partial output — a wrapper returns `ok: false` with the error text and NEVER performs the work itself. Each caller owns its fallback by role: sol legs re-dispatch to a native fable agent, terra to opus — same task text, native execution. Never pre-assign a Claude twin to idle alongside a healthy codex leg.
- Codex output is a colleague's, not an oracle's — verify load-bearing claims against source or current documentation before acting; push back through a session resume, identified as Claude, with evidence.
- A non-zero exit reports verbatim; retry only with a changed command. "Ran out of room in the model's context window" is OVER-SCOPED, not transient — split into narrower sequential lanes whose later legs read the earlier legs' reports; relaunching as-is fails identically.
