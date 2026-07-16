---
name: codex
description: Dispatch work to Codex (gpt-5.6 Terra/Sol/Luna) via the `codex` MCP tool or the CLI (`codex exec` / `codex review`) — each run executes in an isolated context and returns only a report, and usage is effectively free, so prefer it over Claude subagents for self-contained legs. Trigger whenever a leg is transcript-heavy or mechanical - repo sweeps, audits, investigation, many-file reads, log or dataset distillation, live web research, bulk clear-spec implementation, migrations - and for an independent second perspective on any plan, diff, or implementation. Also trigger whenever the user references codex, gpt-5.6, Terra, Sol, Luna, OpenAI models, or asks to offload work or conserve usage; delegation across Claude's own surfaces belongs to agent-dispatch, and Gemini second-model calls belong to agy.
---

# [CODEX]

`codex exec` runs a gpt-5.6 model as a non-interactive agent in its own context window and returns one final message. `~/.codex/config.toml` may carry an interactive model and a permissive sandbox — those two axes are always stated per lane — while its `model_reasoning_effort` row IS the estate dispatch default, inherited by every lane that states no tier; the model and effort law is [05]. Choosing WHERE work runs across Claude's own surfaces (subagent, fork, team, workflow) is the agent-dispatch skill; this skill owns the codex leg once dispatch picks it.

## [01]-[ROUTING]

- [01]-[META_MANAGEMENT](references/meta-management.md): skill format, discovery, port deltas, agent TOML, spawn, `[agents]`, MCP health, wiring
- [02]-[LANE_TEMPLATES](references/lane-templates.md): recon and write lanes — developer-instructions, budget, territory, output contracts, exclusions

## [02]-[DISPATCH]

- Investigation legs whose transcripts flood this context — repo sweeps, audits, log distillation, data analysis: codex returns the conclusion.
- Bulk mechanical implementation with a clear spec: migrations, renames, format conversions, boilerplate expansion.
- Independent second perspective on a plan, implementation, or diff — a different model lineage catches different failure modes.
- Research legs needing live sources: add `-c web_search="live"`.
- Long-running work that must not occupy this session: run in the background against a report file.

Keep in Claude: work inseparable from conversation context too large to restate, and anything mid-flight with the user where iteration latency dominates.

## [03]-[INVOCATION]

Two surfaces carry every leg. Any caller holding the fleet — the main loop, subagents, workflow agents — dispatches through the `codex` MCP tool (fleet server `codex`, tools `codex` and `codex-reply`): the prompt rides a tool argument (no shell quoting, no prompt files), `model`, `sandbox`, `cwd`, and `approval-policy` are first-class parameters, effort inherits the config default (pass the `config` object — `{"model_reasoning_effort": "..."}` — only where a lane deviates), and concurrent calls on the one server process complete independently. Each tool call returns a JSON envelope `{threadId, content}` — `content` holds the final-message text (parse the envelope; a consumer that treats the raw result as the product double-encodes every downstream read), and `threadId` feeds `codex-reply`. Three more parameters the schema carries: `developer-instructions` injects a developer-role message — the channel for lane law (mandates, output contracts, read-only clauses) so `prompt` carries only the task; `base-instructions` REPLACES codex's default system instructions entirely (deliberate use only — it drops the default agent behavior); `compact-prompt` overrides the conversation-compaction prompt on long threads. A blocking tool call is legal waiting wherever an agent cannot sleep, and a multi-minute high-effort call completes inside one call — TWO ceilings own the call, both captured ONCE at session start: the total-duration `MCP_TOOL_TIMEOUT` row in `~/.claude/settings.json`, and the per-server IDLE abort (default 1800s — codex streams no progress mid-call, so idle equals wall-clock and the idle ceiling is the one that fires on a long silent lane), raised by the `timeout` field on the server's `mcpServers` row in `~/.claude.json` or `CLAUDE_CODE_MCP_TOOL_IDLE_TIMEOUT` globally. A session running when either changes keeps its old ceilings; `env | rg MCP_TOOL` is the live truth. Estate law anchors the whole ladder at the fleet manifest codex row (`toolTimeoutSec`): the supervisor idle lease, the client ceilings, and every workflow wrapper stall derive from it, ordered client-aborts-first — an independent timeout constant anywhere in that chain is drift. A call a ceiling kills is abandoned, not cancelled — the codex session runs to completion server-side (a workspace-write lane keeps EDITING), so a write lane polls its report on disk before any re-dispatch: a blind retry mints a duplicate concurrent writer on the same files. From the MAIN loop the blocking MCP call is the wrong default: it freezes the session with zero visibility for the call's full duration — a main-loop leg runs as the background CLI keeper below, and the blocking MCP call is reserved for workflow wrappers and subagents that have nothing else to do. Below, the CLI form serves the terminal, background legs that must outlive a turn, legs past that ceiling, and image-bearing legs — `-i` is CLI-only, the MCP tool takes no image parameter:

An MCP tool call issued from INSIDE a codex turn is unbounded — no per-call timeout exists on that inner hop, so one heavy call (`mcp__nuget__get_package_context` against a large package) wedges the whole lane until the outer ceiling kills it. Pin nested lookups to light calls (`get_latest_package_version`, never `get_package_context`) and have the lane prompt race any nested `mcp__*` call against a short deadline; a per-lane wall-clock watchdog cancels the codex call well before the ceiling.

```bash template
codex exec -s <sandbox> -m <model> [-c 'model_reasoning_effort="<tier>"'] --skip-git-repo-check [-C <dir>] [-o <report>] "<prompt>" </dev/null 2>/dev/null
```

- `</dev/null` is mandatory from a harness: `codex exec` reads piped stdin to EOF even with a prompt argument, appending it as a `<stdin>` block.
- Open-but-silent stdin blocks that read forever; the symptom is zero output, zero CPU, indefinite hang.
- Codex prints `Reading additional input from stdin...` on stderr before the read even under `</dev/null` — pre-EOF notice, not a hang.
- `2>/dev/null` drops thinking tokens; keep stderr only when diagnosing a failing run.
- `--skip-git-repo-check` always.
- Always pass `-s` and `-m` explicitly: user config may carry a flagship interactive model and a permissive sandbox, so neither axis rides a default.
- Effort inherits — `model_reasoning_effort` in `~/.codex/config.toml` IS the estate dispatch tier, stated on a lane only to deviate.
- A `--ignore-user-config` lane restates model AND effort, because the skip drops that default.
- Codex prints the final message to `stdout`; the banner and reasoning stream go to stderr.
- Capture a synchronous run straight from stdout — `out=$(codex exec -s read-only … "<prompt>" </dev/null 2>/dev/null)` — clean, no banner.
- Synchronous capture is the default: a heavy read (doctrine plus catalogues) lands inside the 10-minute Bash ceiling; `-o` only when backgrounding.
- Background a long leg with a bare `&`: `codex exec … -o <report> "<prompt>" </dev/null >/dev/null 2><report>-stderr.log &`
- That bare `&` form survives the launching shell's exit; never prepend `nohup` — a redirect-less `nohup` writes a stray `nohup.out`.
- A dying run's stderr log tail IS the crash reason when no report lands; fleet-grade lanes add `--json` events and a notify push ([08]-[SIGNALS]).
    - A main-loop foreground Bash call reaps its detached child minutes after the call returns, killing the lane reportless mid-run.
    - From the main loop, run codex SYNCHRONOUSLY inside a `run_in_background` Bash task with promote-on-finish — that background task is the keeper.
    - Poll a detached leg by its signals ladder ([08]): liveness (`pgrep -f "<report-basename>"`) is the RUNNING check.
    - An absent report under a live process is normal; never relaunch a live run.
    - Liveness is not health — a process past its tier deadline with no report and near-zero CPU (`ps -p <pid> -o time=`) is WEDGED.
    - Kill a wedged lane (`pkill -f "<report-basename>"`) and relaunch once; a second wedge is the failure.
- `--output-schema` serves the one case where a MACHINE parses the report blind — jq pipelines, exported artifacts, no model in between.
- A leg whose output a Claude agent reads takes a prose JSON contract instead, validated at that agent's own boundary.
- That schema is STRICT: every key in `properties` must also appear in `required`, under `additionalProperties: false`.
- A conditional field is required-but-empty; anything less 400s `invalid_json_schema` and the run silently degrades to unvalidated output.
- Write task and schema files with a real file-write (never a shell heredoc) at ABSOLUTE paths.
- Cwd drift plus heredoc quoting land files where codex cannot find them, and the launch dies instantly on the missing schema.
- A detached typed leg owns its artifacts in one ephemeral folder: `task.md` the quoting-proof prompt, `schema.json` the `--output-schema` path.
- `report.json` (`-o`) holds the product, `stderr.log` the crash reason, and fleet-grade lanes add `events.jsonl` (`--json` stdout).
- Purge stale report and stderr before launch: a leftover report reads as instant completion carrying last run's data.
- A short SYNCHRONOUS leg needs zero files: inline prompt, stdout capture.

| [INDEX] | [NEED]                                        | [FLAGS]                                                                                |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | Read, analyze, research                       | `-s read-only` — blocks `uv run`; uv needs row [02] plus `UV_CACHE_DIR=.cache/uv`      |
|  [02]   | Edit files in the workspace                   | `-s workspace-write` — `.git/` stays write-blocked; default uv cache is off-workspace  |
|  [03]   | Extra writable roots alongside the workspace  | `--add-dir <dir>`                                                                      |
|  [04]   | Network or system access beyond the workspace | `-s danger-full-access` — only with explicit user authorization                        |
|  [05]   | Run rooted in another directory               | `-C <dir>`                                                                             |
|  [06]   | Durable report artifact                       | `-o <file>` — an empty file means the process was killed before completion             |
|  [07]   | Typed JSON final message                      | `--output-schema <schema.json>`                                                        |
|  [08]   | Live web search                               | `-c web_search="live"` — default `cached` answers from an OpenAI index, no live fetch  |
|  [09]   | Attach images (screenshots, diagrams)         | `-i <file>` (repeatable)                                                               |
|  [10]   | Zero-config lane (no MCP fleet, no notify)    | `--ignore-user-config` — auth and skills kept; RESTATE model AND effort                |
|  [11]   | Fan-out legs with no session persistence      | `--ephemeral` — not resumable                                                          |
|  [12]   | Streamed JSONL events for live monitoring     | `--json` (thread/turn/item events to stdout; composes with `-o`)                       |
|  [13]   | Per-lane completion push                      | `-c 'notify=["<sink>","<lane>"]'` — fires at turn end with a JSON payload              |
|  [14]   | Config canary                                 | `--strict-config` — unknown config fields and `-c` keys fail fast                      |

## [04]-[MCP_SELECTION]

MCP selection is GRADED — pick the tier by what the task actually calls, never default to the full fleet:

| [INDEX] | [TIER]     | [INVOCATION]                                                       | [BEHAVIOR]                                             |
| :-----: | :--------- | :----------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | FULL FLEET | bare invocation                                                    | every config + plugin server spawns; slow; fail-closed |
|  [02]   | SELECTED   | `-c 'mcp_servers.<name>.enabled=false'` per unused server          | disabled servers spawn no child, no tools              |
|  [03]   | NONE       | `--ignore-user-config -m <model> -c model_reasoning_effort=<tier>` | config.toml skipped; zero MCP children                 |

- `--ignore-user-config` drops MCP, plugins, notify, and the trust table; auth and all skills survive the skip.
- Model-catalog defaults vary between bundled and refreshed catalogs, so a zero-config lane always restates both model and effort.
- Every flag spares the AGENTS.md chain — `--ignore-rules` drops only execpolicy `.rules`, and no flag suppresses instruction files.
- `--ignore-user-config` also drops the config's `project_doc_max_bytes` row, reverting the chain budget to the 32KiB default.
- Codex stops adding files at that cap root-to-leaf, so an oversized global `~/.codex/AGENTS.md` silently evicts every repo and nested AGENTS.md.
- A zero-config lane that needs the chain restates the cap (`-c project_doc_max_bytes=65536`) beside model and effort.
- Per-lane scope bounds therefore ride `developer-instructions` — truncation-immune, developer-role above the user-role AGENTS.md messages.
- A lane needing a guaranteed-clean surface constructs it with `base-instructions`, never a suppression flag.
- FAIL-CLOSED HAZARD: a `required = true` server that misses its `startup_timeout_sec` handshake kills the whole run at session creation.
- Symptoms: `thread/start failed`, exit 1, no model turn, no report, ZERO JSONL events.
- One unreachable required server (a down VPS behind a stdio bridge) fails every full-fleet lane on the machine.
- Rescue a fail-closed lane with `-c 'mcp_servers.<name>.enabled=false'`; disable required servers first when trimming.
- `-c 'mcp_servers={}'` is a merge NO-OP — `-c` table overrides deep-merge, so an empty table clears nothing and the fleet still spawns.
- Tool-level `enabled_tools` allowlists are schema-valid but blank the server's ENTIRE toolset regardless of the names given.
- Probed with both server-side and normalized tool names: selective granularity stops at the server level.
- A repo-root `.codex/config.toml` shapes every codex run rooted in a TRUSTED project.
- Trust comes from the `[projects]` table in the user config; a `-c 'projects.….trust_level'` grant does not activate it.
- Precedence is `-c` flag over project file over user config; a `-c 'mcp_servers.<name>.enabled=…'` override beats a stray repo row either direction.
- Codex configuration is home-only: `~/.codex` owns config, estate repos are all trusted, and lane shaping rides flags.
- A project-local `.codex/` directory is a defect — port any load-bearing row to `~/.codex`, then delete the directory.
- Overrides compose freely, holding under `--profile`: `-c 'mcp_servers.<name>.enabled=false'` rides beside `-m` and an effort override in one call.
- `--strict-config` validates every config row and `-c` override against the installed binary, failing fast on `unknown configuration field`.
- That validation holds even under `--ignore-user-config`; put `--strict-config` on fleet canaries, keep it off steady-state lanes.
- An optional OAuth server's `AuthRequired` line may leave the Codex process alive, but that server is unusable — a fleet-health failure.
- Inspect `codex mcp list --json` or `forge-mcp doctor --network`; a concurrent system-skill `Directory not empty (os error 66)` alone is nonfatal.
- Fan-out lanes that skip Heptabase disable `heptabase-mcp`: codex never serializes rotating refresh-token transactions across concurrent processes.
- Unused Heptabase initialization then creates avoidable reauthorization races.
- HEADLESS TOOL-CALL BOUNDARY: `codex exec` runs at `approval: never`, and callability combines tool annotations with `default_tools_approval_mode`.
- Annotated read-only tools call cleanly everywhere; an unannotated tool calls only under mode `approve`.
- Modes `auto` and `writes` cancel an unannotated call mid-flight: `started` → `(failed)` → `user cancelled MCP tool call`.
- Codex may FABRICATE a plausible result instead of surfacing that cancellation.
- `default_tools_approval_mode = "approve"` lands permanently in the server's `~/.codex/config.toml` row for pure information-retrieval servers.
- Per lane that grant rides `-c 'mcp_servers.<name>.default_tools_approval_mode="approve"'`; `tools.<tool>.approval_mode` narrows it to one tool.
- NEVER grant `approve` to a write-capable server on a headless lane — MCP servers run OUTSIDE the sandbox, so an approved write executes unconfined.
- Prove a server before betting a lane on it: a probe lane calling one tool with "report the raw result or the exact error text" settles callability.
- Route a leg whose tool cannot be granted `approve` to a Claude agent, which holds the native fleet.
- A codex-exec cancellation never proves a server broken; verify by spawning its exact configured command and piping `initialize`+`tools/list`.
- That probe reads each tool's `annotations`, which combine with the effective approval mode as the callability discriminant.
- Disable keys ride UNQUOTED: a TOML-quoted key in `-c 'mcp_servers."<name>".enabled=false'` mints a phantom transportless server.
- That phantom kills the run at config load (`invalid transport`); the unquoted spelling merges correctly, hyphenated names included.

## [05]-[MODEL_AND_EFFORT]

Every lane pins the model; effort inherits the `model_reasoning_effort` default in `~/.codex/config.toml`, stated only to deviate. Terra is the dispatch default; Sol is the surgical escalation for legs whose value concentrates in judgment; Luna is the volume lane. Bare `gpt-5.6` is an API-only slug that 400s under ChatGPT auth — always the suffixed name.

| [INDEX] | [MODEL]         | [ROLE]                                                                                                            |
| :-----: | :-------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | `gpt-5.6-terra` | dispatch default — sweeps, audits, investigation, research, bulk clear-spec implementation, second perspectives   |
|  [02]   | `gpt-5.6-sol`   | flagship for ambiguous, open-ended, high-value legs: complex code authoring, deep design, the hardest reviews     |
|  [03]   | `gpt-5.6-luna`  | high-volume legs with a known-good result shape: extraction, classification, transformation, structured summaries |

To run at the config default, pass no tier; each row is the flag to deviate to it, and its timeout sizes that tier's task class.

| [INDEX] | [TIER] | [SELECT]                             | [USE]                                                                   | [TIMEOUT] |
| :-----: | :----- | :----------------------------------- | :---------------------------------------------------------------------- | :-------- |
|  [01]   | low    | `-c model_reasoning_effort="low"`    | trivial glue: probes, one-line extractions, mechanical relabels         | 300s      |
|  [02]   | medium | `-c model_reasoning_effort="medium"` | bulk mechanical fan-out legs where throughput beats depth               | 300s      |
|  [03]   | high   | `-c model_reasoning_effort="high"`   | research and review legs; solid depth for the common case               | 600s      |
|  [04]   | xhigh  | `-c model_reasoning_effort="xhigh"`  | deeper single-agent reasoning for harder investigation and design       | 1200s     |
|  [05]   | max    | `-c model_reasoning_effort="max"`    | the hardest problems — maximum single-agent depth, multi-minute latency | 1800s     |
|  [06]   | ultra  | `-c model_reasoning_effort="ultra"`  | RARE: the lane itself must self-decompose into parallel subagents       | 1800s     |

- Sol and Terra accept low through ultra; Luna accepts low through max and rejects ultra.
- Factory default is medium; the operator config pins the estate default estate-wide.
- Subagent spawning is prompt-triggered at EVERY tier — an explicit "spawn N parallel subagents" lands `collab_tool_call` items even at medium.
- Ultra only biases codex to decompose unasked, redundant where the caller — a workflow, a fan-out orchestrator — already owns the decomposition.
- Reach for ultra only when a single lane must run its own internal fleet.
- Deviate surgically, one axis at a time: max deepens the single hardest leg, low/medium serve throughput.
- Subagent concurrency is `features.multi_agent_v2.max_concurrent_threads_per_session`, spawn depth is `agents.max_depth`.
- Both rows read from `~/.codex/config.toml`, never assumed.
- Overrun means CIRCLING — re-verifying unchanged work, re-reading covered territory, loops adding no new evidence — never duration.
- A well-scoped high-tier leg legitimately runs an hour of real work.
- Circling resolves in fixed order: a missing completion bar goes FIRST — effort raises persistence with depth, and an open mandate fills its room.
- A lane still circling with the bar in place downshifts to `medium`.
- Added instructions come last, one at a time — repetition measurably degrades adherence.
- Upshift is the mirror — xhigh earns its latency on review-shaped legs with demonstrated benefit.
- An upshift without a bar buys longer runs, not better ones.
- Latency tracks task shape, not tier: a trivial prompt returns in seconds at any tier.
- That column is the ceiling for the tier's intended task class, never an expected wall, and it sizes the CLI `timeout`.
- An MCP call ignores that column entirely — a heavy high-effort call runs past the 600s row and completes, bounded only by `MCP_TOOL_TIMEOUT`.
- Codex emits its result only at completion — there is no partial output to salvage from a killed run.
- Run synchronously with the Bash timeout set to the tier; from the higher tiers up prefer background execution against the signals ladder.

## [06]-[PROMPT_CONTRACT]

Codex shares no conversation state — every prompt is self-contained. Include: the goal, absolute paths, scope bounds and what must not be touched, constraints that matter, and an explicit final-message contract ("Final message: a report with sections X, Y, Z" or an `--output-schema` file). For edit runs, name the acceptance signals codex must run before finishing (build, test, lint commands).

GPT-5.x prompting is its own discipline — a Claude-shaped prompt underperforms:

- ROLE SPLIT: durable lane law — mandate, tool rules, output contract — rides `developer-instructions`; the `prompt` carries only the task instance.
- Battery-validated: the split posted the strongest depth and territory discipline at zero adherence cost.
- DE-CONFLICT: GPT-5.x burns reasoning reconciling contradictory or overlapping directives instead of picking one — one directive per concern.
- Never restate behavior codex already ships by default: persistence, `apply_patch` fluency, exploration.
- Emphatic intensifiers ("THOROUGH", "exhaustive") are production-measured to cause tool over-use.
- Chain-of-thought scaffolding and preamble or plan-narration requests cause early stops on codex.
- STRUCTURE: named XML spec blocks (`<context_gathering>`, `<verification>`, `<output_contract>` last) are the highest-adherence form.
- Bounded recon adds explicit early-stop criteria plus an uncertainty escape hatch ("proceed even if not fully certain").
- A `<persistence>` block ("keep going until fully resolved, never hand back") is a liability on gpt-5.6.
- System-card-measured: persistence blocks amplify scope-exceeding and unverified-claim rates at the high tiers.
- Codex already persists; prompts supply stop rules, never push.
- STOP RULES: every lane states its completion bar — the enumerated deliverables and the proof each is met.
- That bar bounds scope, never depth: the leg stays exhaustive and adversarial inside the enumerated territory.
- A lane finishes when every deliverable carries its proof, and lands beyond-bar findings as typed residue rows.
- A bare "fix everything you find" mandate at high+ effort is the measured failure regime — scope substitution and verification claims without runs.
- LAYER PIN: a long leg names its layer — research, design, implementation, review — and never migrates out of it.
- An audit leg reports the change, an implementation leg never re-opens design; out-of-layer discoveries land as typed rows in the product.
- SCOPE + AMBIGUITY: a `<design_and_scope_constraints>` block bounds the flagship's documented tendency to expand work and take extra tool actions.
- That block reads "implement EXACTLY and ONLY the named moves; choose the simplest valid interpretation of any ambiguity".
- Constrain ambiguity by instruction, never by inviting clarifying questions — a headless lane has nobody to ask.
- Flagship lanes reward a resolution instruction more than codex-tuned models do.
- BUDGET PHRASING: cap TOTAL tool calls, never per-file reads, and forbid aggregation explicitly.
- Phrase it "read in small batches; never concatenate the territory into one command — tool output truncates".
- Battery-measured: a per-file read cap made every lane aggregate the territory into one truncating command, collapsing completeness 25/25 to 1/25.
- Codex obeys budgets literally enough that a mis-calibrated one is a foot-gun.
- A well-phrased budget bought 2-3x speed, 4-7x fewer tokens, and perfect territory discipline.
- TERRITORY: name instruction and skill files as out of scope explicitly — unbudgeted lanes spontaneously read `~/.codex/skills/*` and repo law files.
- CONTRACT: a prose JSON contract is reliable on terra (battery: 8/8 raw-parse, zero fences) when it carries the word "JSON" and the exact shape.
- It also carries "no prose outside the JSON, no code fences" and null-for-missing; the null clause measurably improves honesty.
- Schema enforcement exists only on the API path, so the MCP/CLI final message stays best-effort by construction.
- MCP tool calls SERIALIZE within a lane — prompting for parallel batching buys no wall time on the MCP surface.
- CLI lanes with native tools still parallelize reads.

## [07]-[USAGE_EXHAUSTION]

Quota exhaustion fails the individual call LOUDLY with no partial output — a wrapper receives a plain tool error, returns `ok: false` with the error text, and NEVER performs the work itself (the dispatch-role contract forbids it; battery-observed to hold). Each caller owns its fallback, mapped by role: terra legs re-dispatch to a native opus agent, sol legs to fable, luna legs to sonnet — same task text, native execution — or the leg's failure flows into the run's unmapped-territory path where downstream consumers already cold-read dead lanes' scope. Never leave a sonnet wrapper as the implicit fallback executor, and never pre-assign a Claude twin to idle alongside a healthy codex leg.

## [08]-[SIGNALS]

Detached CLI lanes — main-loop background legs, never workflow wrappers — carry machine-readable completion signals; report-file existence is the fallback witness, never the primary. Fleet-grade launch:

```bash template
codex exec -s <sandbox> --skip-git-repo-check --json -o <report> \
  -c 'notify=["<sink>","<lane>"]' "<prompt>" </dev/null ><events.jsonl> 2><stderr.log> &
```

- `--json` streams JSONL to stdout — route it to the lane's `events.jsonl`; stderr stays the crash log, and `-o` still writes the report.
- Event sequence: `thread.started{thread_id}` the resume id, `turn.started`, `item.started`/`item.completed`, then `turn.completed` or `turn.failed`.
- Item types: `agent_message`, `reasoning`, `command_execution`, `mcp_tool_call`, `web_search`, `file_change`, `error`.
- `agent_message` carries the final message text; `command_execution` carries `{command,aggregated_output,exit_code,status}`.
- `collab_tool_call` marks a subagent spawn under effort ultra.
- `turn.completed` carries `usage`; its fields are `input_tokens`, `cached_input_tokens`, `output_tokens`, `reasoning_output_tokens`.
- An `item.type=="error"` item is NOT a run failure — the skills-listing-budget warning rides one on every run under a large skill library.
- Classify by turn events only.
- `notify` is the push channel: the sink program runs at turn end with its configured leading args followed by ONE JSON arg.
- Payload: `{"type":"agent-turn-complete","thread-id":…,"turn-id":…,"cwd":…,"client":"codex_exec","input-messages":[…],"last-assistant-message":…}`.
- Riding the CLI layer, the `-c` override works under `--ignore-user-config` and replaces the user-config notify for that run.
- A keeper writes the estate sink as a three-liner that appends `"$@"` plus a timestamp to ONE fleet `events.log`.
- Lane id, resume id, and final-message headline then arrive as a single ledger line per completion.
- CLASSIFICATION LADDER, applied per lane in order: `turn.completed` in events plus a non-empty report → READY.
- `turn.failed` in events → FAILED with the event message.
- Process gone plus an empty events file → DEAD, the stderr tail is the reason.
- Pre-thread deaths — config errors, required-MCP init — emit ZERO JSONL and never fire notify.
- Process alive with no turn event → RUNNING (wedge check per [03]).
- `turn.completed.usage` is the fleet's token ledger — sum it across lanes' events files for per-run accounting.
- Codex tokens stay invisible to Claude-side budgets.

## [09]-[SESSIONS]

Every run mints a resumable thread on both surfaces — capture the id whenever follow-up is plausible, and run iterative deep work as ONE thread continued with sharpened prompts, never fresh runs re-paying the exploration cost.

- MCP: every `codex` tool result envelope carries `threadId` beside `content`; continue with `codex-reply`.
- Each continuation inherits the thread's model, effort, and sandbox; a caller that discards the threadId has severed the chain.
- Capture the threadId in the same turn that made the call.
- CLI: every run prints `session id: <uuid>` in its banner; under `--json`, `thread.started.thread_id` carries the same id.
- Resume with `codex exec resume <session-id> "<follow-up>" </dev/null 2>/dev/null`, config flags between `exec` and `resume`.
- A resumed session inherits the original model, effort, and sandbox.
- `resume --last` resolves to the newest recorded session — valid only when nothing else ran in between; under any concurrency resume by explicit id.
- `--ephemeral` runs record nothing and cannot be resumed.
- On-disk store: one rollout file per session at `~/.codex/sessions/<YYYY>/<MM>/<DD>/rollout-<local-ISO-ts>-<uuidv7>.jsonl`.
- `session_index.jsonl` maps id to thread name but LAGS — unwritten for still-live runs.
- Live correlation rides the datestamped filename, never the index.
- A dead or wedged lane whose `threadId` rode its receipt recovers with `codex exec resume <threadId>`.
- A lane receipt therefore persists the threadId at dispatch.
- Bulk cleanup: `codex delete` removes one saved session, and `--force` is unreliable across spawned child threads.
- Working bulk path: a date-scoped prune of `state_5.sqlite` rows plus the matching rollout-file deletions plus `VACUUM`.
- `[features] memories`/`chronicle` rows in `~/.codex/config.toml` gate those features independently of corpus deletion.

## [10]-[REVIEW]

`codex review` runs an independent non-interactive code review; the positional prompt sets focus, omitted for a general pass.

| [INDEX] | [SCOPE]                       | [COMMAND]                               |
| :-----: | :---------------------------- | :-------------------------------------- |
|  [01]   | Staged + unstaged + untracked | `codex review --uncommitted "<focus>"`  |
|  [02]   | Branch against a base         | `codex review --base <branch>`          |
|  [03]   | One commit                    | `codex review --commit <sha> "<focus>"` |

- Branch against a base: the CLI rejects a prompt alongside `--base`; a FOCUSED range review routes through `codex exec` with an explicit diff task.

A fleet-grade review lane runs `codex exec review` instead — the same scope flags plus `--json` events, `-o`, and `--output-schema` typed findings, so a detached review composes with the signals ladder ([08]) like any other lane.

## [11]-[FAN_OUT]

Independent scopes run as concurrent `codex exec` processes, one per scope, each launched fleet-grade ([08]) with its own report, events, and stderr files. Reconcile the reports after all complete. Sandbox isolation is per-process; concurrent workspace-write runs against overlapping paths collide — partition write scopes or serialize them.

A row-shaped sweep — one similar task per work item — rides ONE lane's `spawn_agents_on_csv` tool instead of N processes: the lane builds a CSV, spawns one worker subagent per row under the active agent-concurrency cap, and exports a combined results CSV (`output_schema` types each worker's row; `codex exec` streams one-line batch progress on stderr). Under `-s read-only` the workers return results in-band but the `output_csv_path` export never lands — a lane that needs the CSV artifact runs `workspace-write`. Reusable worker personas are custom agent files the prompt spawns by name; the TOML schema and `[agents]` globals are the meta-management reference's.

A large fleet runs as ONE launcher script in a `run_in_background` Bash task: prompts written to files, lanes pooled with `wait -n` against a max-concurrency cap, the notify sink and ledger written by the same keeper, promotion at the end — launcher and harvester in the same keeper. A keeper's exit code proves nothing about lanes: a spawn-time defect (missing wrapper binary, bad flag) kills every lane in seconds while the keeper still exits 0 and its ledger reads all-FAILED. Verify the fleet within a minute of launch — `pgrep -f 'codex exec'` count at or near the cap plus a clean first stderr log — before trusting it. Never wrap codex lanes in `timeout` — this overrides the general background-kick timeout rule: a deadline kill discards the whole run — codex emits only at completion, so there is no partial result to salvage; hang protection is the wedge check (CPU-time vs wall-time) in [03], or a manual kill of a stalled lane.

## [12]-[WORKFLOWS]

`agent()` accepts only Claude models, so a workflow dispatches codex through a thin sonnet wrapper whose whole job is one blocking `codex` MCP tool call and a thin receipt — the wrapper contract, receipt shape, batching economics, and scale law are the workflow-creator skill's codex-lanes reference. A lane expected to outrun the MCP tool timeout is the one case that still runs the detached CLI form ([08]) — from the MAIN loop as a `run_in_background` Bash keeper, never inside a workflow wrapper. That same disk-product discipline binds a plain Claude subagent making a codex call — a product past the subagent's final-message budget goes to disk and the subagent returns the path, never the inlined body. Re-emitting a codex product through a Claude agent's own Write is fallibly lossy at scale (production-observed: a mid-string tail drop minting invalid JSON behind an `ok` receipt) — a `workspace-write` lane writes its own report as its final act and the caller verifies (`jq -e`); an unavoidable re-emission is verified the same way before the receipt returns. Codex tokens never count toward `budget.spent()`.

## [13]-[JUDGMENT]

Codex output is a colleague's, not an oracle's. Verify load-bearing claims against source or current documentation before acting on them; push back through a session resume, identified as Claude, with evidence — both lineages produce errors. A non-zero exit is reported verbatim; retry only with a changed command. A run that dies with "ran out of room in the model's context window" is OVER-SCOPED, not transient — split the task into narrower sequential lanes (research vs design; sweep vs synthesis) whose later lanes read the earlier lanes' report files; relaunching as-is fails identically.
