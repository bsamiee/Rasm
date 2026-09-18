---
name: observation
description: "Use when a task reads hook event rows or writes finding rows, covering the sink, views, finding states, scripts, checker mapping, and delivery."
user-invocable: false
---

# [OBSERVATION]

Sink `<main>/.cache/observation/observation.db` holds one row per hook event the `function-hooks` plugin records, views over the rows, and finding tables agents write, one file per repository under its main worktree. `<main>` is the first `worktree` line of `git worktree list --porcelain`, `<worktree>` the `git rev-parse --show-toplevel` line, `<branch>` the `git branch --show-current` line, `<db>` the sink path, and `<scripts>` `.claude/skills/observation/scripts`. Readers run `sqlite3 -json -cmd ".param set :<name> <value>" <db> "<select>"` from `<worktree>`, one binding per id the select reads, `-json` renders `payload` as a JSON string and `jq '[.[] | .payload |= fromjson]'` nests it. Writers run one script from `<worktree>` with its parameters bound before the read:

```bash
sqlite3 -bail -json -cmd ".timeout 10000" -cmd ".param set :worktree '<worktree>'" <db> ".read <scripts>/lifecycle.sql"
```

One `-cmd ".param set :<name> <value>"` binds each parameter, the value SQL-evaluated: text goes as `'<text>'`, a text holding `'` as `"'<text>'"` with each `'` doubled, a number bare, an absent value as `null`, and a name left unbound reads null. `:ids` names a JSON array of finding ids. `:out` and `:sites` hold JSON text, `:out` the JSON a checker printed, `:sites` a JSON array of objects keyed by `site` columns. JSON text holds `'` and `"`, the quotes `.param set` splits its value on, each binds as one row of the shell's binding table through an SQL argument after `<db>`, where `''` spells one `'`, an SQL `-cmd` ends a `-bail` process before the arguments run:

```bash
sqlite3 -bail -json -cmd ".timeout 10000" -cmd ".param set :worktree '<worktree>'" <db> "insert into temp.sqlite_parameters(key, value) values(':out', '$(<checker command> | sd -F "'" "''")')" ".read <scripts>/<script>"
```

DuckDB scripts run `duckdb -json -cmd "set variable transcript = '<transcript>'" -f <script>`, the sink attached through `-cmd "attach '<db>' as s (type sqlite, read_only)"`.

[REFERENCES]:
- [01]-[SQLITE](references/sqlite.md): SQLite and DuckDB facts that decide a script's form, one section per documentation page

[SCRIPTS]:
- [01]-[LIFECYCLE](scripts/lifecycle.sql): `:worktree`, closes, moves, and reconfirms every open row, one returned row per transition with its `state`
- [02]-[BATCH](scripts/batch.sql): `:worktree`, `:sites`, `:id`, proposes an agent's sites, returns the ids new to `finding`, then every site's id
- [03]-[AST_GREP](scripts/ast-grep.sql): `:worktree`, `:out`, confirms `ast-grep scan --json=compact` hits, returns as `batch.sql`
- [04]-[RUFF](scripts/ruff.sql): `:worktree`, `:out`, confirms `ruff check --output-format json` diagnostics, returns as `batch.sql`
- [05]-[BIOME](scripts/biome.sql): `:worktree`, `:out`, confirms `biome lint --reporter=json` diagnostics, returns as `batch.sql`
- [06]-[ROSLYN](scripts/roslyn.sql): `:worktree`, `:out`, confirms SARIF results with a location, returns as `batch.sql`
- [07]-[TRANSITION](scripts/transition.sql): `:finding_id`, `:state`, `:by`, `:evidence`, `:verdict`, one transition, returns id and state
- [08]-[BAR](scripts/bar.sql): `:verdict`, `:earns`, one `bar_verdict` row updated in place, returns nothing
- [09]-[GATE_CATALOGER](scripts/gate-cataloger.sql): `:id`, the shape-cataloger's gate counts, one named array per select
- [10]-[GATE_VERIFIER](scripts/gate-verifier.sql): `:id`, `:start`, `:ids`, the shape-verifier's gate counts, one named array per select
- [11]-[TRANSCRIPT](scripts/transcript.sql): DuckDB, variable `transcript`, messages, model, and tokens of one transcript
- [12]-[AGENT_TRANSCRIPTS](scripts/agent-transcripts.sql): DuckDB, attached `s`, cost per subagent from its transcript, background ones included

`site.sql`, `hit.sql`, `span.sql`, `line.sql`, and `insert.sql` serve the scripts above through `.read`, none runs alone. A batch is one transaction, a nonzero exit leaves nothing applied.

## [01]-[SINK]

Table `observation(event, ts, session_id, prompt_id, agent_id, tool, tool_use_id, payload)` holds `ts` in milliseconds and `payload` as JSON under harness names:
- Indexes cover `(session_id, ts)`, `agent_id`, `prompt_id`, `tool_use_id`, `json_extract(payload, '$.turnId')`, and `(event, tool, ts)`
- Columns `event` and `tool` take `hook_event_name` and `tool_name`, id columns the same-named field, every other field stays in `payload`
- Main-loop rows hold `agent_id` null, a subagent's rows its id, the parent's `Agent` row `tool_response.agentId` equal to it
- `turn.start` rows come from the main loop alone, subagent runs write `turn.step` rows and one `turn.complete` row under their `agent_id`
- Resumed subagent runs write one more `SubagentStart` and `turn.complete` row under the same `agent_id`
- Plugin-spawned agents record `SubagentStart`, `turn.step`, `turn.complete`, and refused `tool.call` rows alone, `background_tasks` omits them
- `turn.complete` of a plugin-spawned category agent adds `placed`, the paths `git status --porcelain tools/ast-grep` printed at the event
- Classic rows hold `cwd`, `transcript_path`, and `scratchpad_dir`, subagent rows `agent_type`
- `permission_mode` and `effort` appear on classic rows where the event supplies them
- `Stop` and `SessionEnd` rows hold `$.session.usage()` as `usage` with `context`, `cost`, and `rateLimits`
- `cost.usd` covers one process, zero after a resume
- Turn rows hold `usage` as `{model, input_tokens, output_tokens, cache_read_input_tokens, cache_creation_input_tokens}`

`SessionStart` rows on `resume` and `fork` add `session_title`, `seconds_since_last_response`, `context_tokens`, `prompt_cache_likely_expired`, and `estimated_cache_write_usd`:

| [INDEX] | [EVENT]               | [PAYLOAD_KEYS]                                                                                            |
| :-----: | :-------------------- | :-------------------------------------------------------------------------------------------------------- |
|  [01]   | `SessionStart`        | `source` (`startup`, `resume`, `clear`, `compact`, `fork`), `agent_type`, `model`                         |
|  [02]   | `UserPromptSubmit`    | `prompt`                                                                                                  |
|  [03]   | `PostToolUse`         | `tool_input`, `tool_response`, `duration_ms`                                                              |
|  [04]   | `PostToolUse` `Edit`  | `tool_response{structuredPatch}`                                                                          |
|  [05]   | `PostToolUse` `Agent` | `tool_response{agentId, agentType, resolvedModel, status, isAsync, usage, totalTokens, totalDurationMs}`  |
|  [06]   | `PostToolUse` `Bash`  | `tool_response.gitOperation.commit{sha, kind, branch}` on a commit                                        |
|  [07]   | `PostToolUseFailure`  | `tool_input`, `error`, `is_interrupt`, `duration_ms`                                                      |
|  [08]   | `PostToolBatch`       | `tool_calls[]{tool_name, tool_use_id, tool_input}`                                                        |
|  [09]   | `PermissionDenied`    | `tool_input`, `reason`, auto mode alone                                                                   |
|  [10]   | `SubagentStart`       | `agent_type`                                                                                              |
|  [11]   | `SubagentStop`        | `Stop` keys without `usage`, `agent_transcript_path`, `agent_type` `''` if untyped, fork at `PreCompact`  |
|  [12]   | `Stop`                | `stop_hook_active`, `last_assistant_message`, `background_tasks[]`, `session_crons[]`, `usage.cost.usd`   |
|  [13]   | `StopFailure`         | `error` the API error type, `error_details`, `last_assistant_message` the API error text                  |
|  [14]   | `PreCompact`          | `trigger`, `custom_instructions`                                                                          |
|  [15]   | `PostCompact`         | `trigger`, `compact_summary`                                                                              |
|  [16]   | `SessionEnd`          | `reason`, `usage`                                                                                         |
|  [17]   | `tool.call`           | Denied calls alone: `deny`, `trace`, and the call's input keys (`command`, `file_path`, `content`)        |
|  [18]   | `turn.start`          | `turnId`, `text`                                                                                          |
|  [19]   | `turn.step`           | `turnId`, `index`, `model`, `effort`, `messageCount`, `stopReason`, `toolUses`, `answer`, `usage`         |
|  [20]   | `turn.complete`       | `turnId`, `answer` `''` on an error, `durationMs`, `isAborted`, `reason`, `refusal` on a refusal, `usage` |
|  [21]   | `WorktreeCreate`      | `name`                                                                                                    |
|  [22]   | `WorktreeRemove`      | `worktree_path`, reaches no plugin                                                                        |

## [02]-[VIEWS]

Views sit in `hooks/observation/sql.ts` of the plugin, `:session` and `:prompt` bound to the ids a reader filters by. `running_agents` covers sessions with a row within 48 hours:

```bash
# Which files each edit tool call touched, with the cwd it ran from
sqlite3 -json -cmd ".param set :prompt '<prompt>'" <db> "select * from edited_files where prompt_id = :prompt order by ts"

# Which agent spawned each subagent, how the spawn was shaped, and when it ran
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from lineage where session_id = :session order by started_ts"

# When each process of a session began, a compaction opens none
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from processes where session_id = :session order by ts"

# What each turn cost in tokens, in dollars between consecutive Stop rows of one process and null before the first, cause its StopFailure text
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from turn_cost where session_id = :session order by started_ts"

# How long each subagent ran, its tool time, tokens and duration from the parent Agent row or its turn.complete rows, cause its last StopFailure text
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from agent_cost where session_id = :session order by span_ms desc"

# Which files one prompt edited more than once, and by how many agents
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from edit_churn where session_id = :session"

# Which calls a policy, a permission, a failure, or the host refused, and what each asked
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from denials where session_id = :session order by ts"

# How each session ran, prompts, agents, forks, compactions, its end reason, and dollars summed per process
sqlite3 -json <db> "select * from session_audit order by first_ts desc limit 10"

# Which Bash calls committed, with the sha, kind, and branch the harness classified
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from commits where session_id = :session order by ts"

# How each agent and the main loop spent their tool calls, agent_id null for the main loop
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from agent_digest where session_id = :session"

# Which identical calls one agent repeated inside a session, Read excluded by the reader
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from repeated_calls where session_id = :session and tool <> 'Read'"

# Which edits reached outside the directory they ran from
sqlite3 -json -cmd ".param set :session '<session>'" <db> "select * from edits_outside_cwd where session_id = :session"

# Which spawned agents no later turn.complete, SessionEnd, non-compact SessionStart, or Stop lacking an Agent-tool child in background_tasks ended
sqlite3 -json -cmd ".param set :worktree '<worktree>'" <db> "select * from running_agents where cwd = :worktree order by started_ts"

# Which rule files each spawned category agent placed, with its category, lineage_key, and told_on, the lineage keys a rules entry reached
sqlite3 -json -cmd ".param set :key '<lineage_key>'" <db> "select * from placed_rules where lineage_key = :key order by ts"
```

## [03]-[FINDINGS]

Finding tables in `sql.ts`, every one `strict`, their `state`, `channel`, `kind`, and `verdict` values rows of `transition_state`, `delivery_channel`, `range_kind`, and `bar_verdict`:

| [INDEX] | [TABLE]              | [PURPOSE]                                                                   | [WRITER]                          |
| :-----: | :------------------- | :-------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `finding`            | One row per site, never updated                                             | Checker script or judgment agent  |
|  [02]   | `finding_transition` | Every state change, append-only                                             | Judgment agents, checks, the user |
|  [03]   | `finding_delivery`   | One row per id a context line or `report` reached, with its `lineage_key`   | Plugin alone                      |
|  [04]   | `judged_range`       | One range per row, `kind` `edit`, key parts, `agent_id`, `at` of the answer | Plugin at range agent's answer    |

Identity, `sha3` of the `sqlite3` shell as the one hasher, every hash lowercase hex:
- `finding` and `site` generate `ntext` as `<normalized>` over `text`, tabs, returns, and newlines as spaces, every run of spaces as one
- `finding` and `site` generate `text_hash` as `sha3` over `ntext`
- `finding` generates `finding_id` as `sha3` over `category`, observed `path`, `text_hash`, and `occurrence` joined by `char(0)`
- `source` takes `checker:<tool>` or `agent:<id>`
- Every transition writes `path`, the site's current path, `finding_state` reads the latest transition's `path` and the observed path under a null
- `subject_hash` is `<head>`, `lower(hex(sha3(readfile(path), 256)))` over the finding's current `path`
- `subject_hash` of a gone file is `''`, equal to no stored hash
- `occurrence` is the ordinal of `text` within the file at observation
- `path` is relative to `<worktree>`, lines and columns are one-based, `byte_start` and `byte_end` zero-based
- Transitions with null span columns leave `finding_state` reading the finding's span
- `finding_state` reads `state`, `evidence`, and `verdict` from the latest transition other than `moved`, every other column from the latest
- Live rows, `proposed`, `confirmed`, `checker_owned`, `checker_silent`, and `waived`, hold a site on disk, `lifecycle.sql` re-checks each one
- `lineage_key` is `<name>/<branch>`, `<name>` `.` for the main worktree or a linked worktree's directory name, `<branch>` empty on a detached head
- `by` takes `user`, `agent:<id>`, or `check:<tool>`, `<id>` the writer's `agent_id` in a subagent, its `session_id` on the main loop
- `finding_delivery.agent_id` is the category agent on a `report` row and on the rows a rules entry told, null on a finding entry
- `verdict` holds the rule builder's `bar_verdict` row on a `confirmed` under a refused category, copied by re-confirm, kept across a move, else null
- `bar_verdict` opens empty, the rule builder alone fills it from the bar table of `rule-building` through `bar.sql`
- Retired verdicts keep their `bar_verdict` row, `finding_transition.verdict` references it
- `recurring_categories` reads a null `verdict` as not refused
- `<rules>` and `<utils>` are the lines `yq -r '.ruleDirs[]' sgconfig.yml` and `yq -r '.utilDirs[]' sgconfig.yml` print
- Category id is free when `rg -l '^id: <category>(-<language>)?$' <rules> <utils>` exits 1
- Rules with their language and correction are the lines `fd -e yml . <rules> -x yq -r '[.id, .language, .message] | join(" | ")' {}` prints

| [INDEX] | [STATE]          | [MEANING]                                              | [WRITER]                     | [EVIDENCE]                 |
| :-----: | :--------------- | :----------------------------------------------------- | :--------------------------- | :------------------------- |
|  [01]   | `proposed`       | Judgment agent claims the site violates the standard   | `agent:<id>`                 | Correction in one line     |
|  [02]   | `confirmed`      | Present at the hash, fix keeps behavior                | `agent:<id>`, `check:<tool>` | Proof line, `verdict`      |
|  [03]   | `wrong`          | Fix changes behavior, or false positive                | `agent:<id>`                 | Reason, final at this hash |
|  [04]   | `checker_owned`  | Checker rule states the category, a checker row covers | `agent:<id>`                 | `<tool>:<rule id>`         |
|  [05]   | `checker_silent` | Checker rule states the category and missed the site   | `agent:<id>`                 | `<tool>:<rule id>`         |
|  [06]   | `fixed`          | Text absent, an edit removed it or a write lacks it    | `check:<tool>`               | `tool_use_id`              |
|  [07]   | `vanished`       | Text absent with no edit removing it, or `path` gone   | `check:<tool>`               | `null`                     |
|  [08]   | `moved`          | Text moved by an edit or `git mv` since its transition | `check:<tool>`               | `null`, `path` the new one |
|  [09]   | `waived`         | User accepts the site as is                            | `user`                       | Reason                     |

Rows are stale when `subject_hash` of the latest `confirmed` differs from `<head>` at read time, present when `ntext` occurs in `<normalized>` over `cast(readfile(path) as text)`. Rows are covered when the latest transition is `checker_owned` with the checker rule stating the correction as evidence, a checker row on the same span with another correction covers nothing. `readfile` runs from `<worktree>` in a statement alone, views hold none:

```bash
# Writer's <id> and <start>, a subagent by :agent, its definition name, the main loop by :literal, text of one of its own Bash commands
sqlite3 -json -cmd ".param set :agent '<agent>'" -cmd ".param set :worktree '<worktree>'" <db> "select agent_id, ts from observation where event = 'SubagentStart' and json_extract(payload, '$.agent_type') = :agent and (json_extract(payload, '$.cwd') = :worktree or json_extract(payload, '$.cwd') like :worktree || '/%') order by ts desc limit 1"
sqlite3 -json -cmd ".param set :literal '<literal>'" <db> "select session_id, ts from observation where event = 'PostToolUse' and tool = 'Bash' and json_extract(payload, '$.tool_input.command') like '%' || :literal || '%' order by ts desc limit 1"

# Every finding at its current path with its state, span, hash, and last close
sqlite3 -json -cmd ".param set :ids '[\"<a>\", \"<b>\"]'" <db> "select * from finding_state where finding_id in (select value from json_each(:ids))"

# Head hash of each finding's file beside the hash its last transition stored
sqlite3 -json -cmd ".param set :ids '[\"<a>\", \"<b>\"]'" <db> "select finding_id, path, subject_hash, <head> as head from finding_state where finding_id in (select value from json_each(:ids))"

# Which agent findings stand confirmed with no wrong at the same hash
sqlite3 -json -cmd ".param set :ids '[\"<a>\", \"<b>\"]'" <db> "select * from confirmed_findings where finding_id in (select value from json_each(:ids))"

# Which confirmed findings are open and present on disk, and which lineage keys were told since the last close
sqlite3 -json -cmd ".param set :key '<lineage_key>'" <db> "select * from open_findings where instr(<normalized>, ntext) > 0 and not exists (select 1 from json_each(delivered_on) where value = :key)"

# Every transition of one finding in order
sqlite3 -json -cmd ".param set :finding_id '<finding_id>'" <db> "select state, evidence, verdict, at, by from finding_transition where finding_id = :finding_id order by at"

# Which checker categories overlap one span
sqlite3 -json -cmd ".param set :path '<path>'" -cmd ".param set :start_line <n>" -cmd ".param set :end_line <n>" <db> "select category from finding where source like 'checker:%' and path = :path and start_line <= :end_line and end_line >= :start_line"

# Rows on the scope paths, every proposed row, and every confirmed row at a stale hash, :paths a JSON array relative to the worktree
sqlite3 -json -cmd ".param set :paths '[\"<a>\", \"<b>\"]'" <db> "select finding_id, category, path, text, occurrence, source, state, subject_hash = <head> as same_hash from finding_state where path in (select value from json_each(:paths)) or state = 'proposed' or (state = 'confirmed' and subject_hash <> <head>)"

# What each edit call changed, :ids the tool_use_ids
sqlite3 -json -cmd ".param set :ids '[\"<a>\", \"<b>\"]'" <db> "select tool_use_id, payload from observation where event = 'PostToolUse' and tool_use_id in (select value from json_each(:ids))" | jq '[.[] | .payload |= fromjson]'

# Which categories recur, two or more confirmed sites under no refused verdict, and the keys their report reached
sqlite3 -json <db> "select * from recurring_categories order by sites desc"

# Which in-tree edits an edit range of their deepest worktree covers
sqlite3 -json -cmd ".param set :worktree '<worktree>'" <db> "select * from judged_edits where cwd = :worktree order by ts"

# Which in-tree edits no edit range covers
sqlite3 -json -cmd ".param set :worktree '<worktree>'" <db> "select * from unjudged_edits where cwd = :worktree order by ts"

# How often each checker category fires, in sites, sightings, and prompts, beside the prompts judged
sqlite3 -json <db> "select * from category_fires order by sightings desc"

# Which sites a checker rule missed
sqlite3 -json <db> "select * from missed_sites order by category, path, start_line"
```

## [04]-[MAPPING]

Checker JSON becomes `site` rows through the checker's script. Commands run from `<worktree>`, `<paths>` the files to check relative to it, each command's output the `:out` its script binds. ast-grep, ruff, and biome exit 1 on a finding, an empty `:out` marks a checker failure, `json_each` over it raises. A zero-width diagnostic writes `text` `''` at `occurrence` 1. `dotnet build` writes one SARIF log per compiled project, `<project>` the one owning the scope's `.cs` files, `--no-incremental` compiles it when up to date, `<log>` `$(dotnet msbuild Directory.Build.props -getProperty:ArtifactsPath)/binlog/<project>.sarif`, `:out` bound as `-cmd ".param set :out \"cast(readfile('<log>') as text)\""`:

```bash
# [AST_GREP] Rule hits as JSON
ast-grep scan --no-ignore hidden --json=compact <paths>

# [RUFF] Diagnostics as JSON
ruff check --output-format json <paths>

# [BIOME] Diagnostics as JSON
biome lint --reporter=json <paths>

# [ROSLYN] SARIF 2.1 with unencoded file URIs and one-based columns, ls fails where no compiler wrote the log
dotnet build <project> --no-dependencies --no-incremental -p:ErrorLog=<log>%2Cversion=2.1; ls <log>
```

Diagnostic of a checker category comes from its owner, `ruff rule <code>`, `biome explain <rule>`, `<rules>/**/<rule id>.yml`, or the `.editorconfig` row of a Roslyn id.

## [05]-[DELIVERY]

Categories at `categoryThreshold` confirmed sites spawn `categoryAgent` with `category <category> lineage <key>`, one at a time, edit ranges at `editThreshold` files spawn `editAgent` with `range <key> <from_ts> <to_ts>`. `report` rows, one per `finding` row of the category, are written at the category agent's answer and count in `reported_on` until the site closes. `judged_range` row is written at the range agent's answer holding a transition by it. Spawned agents outlive a compaction, not the session.

At a quiet `Stop`, one with no editor of the unjudged range and no `editAgent` in `running_agents` or `background_tasks`, the plugin delivers every `open_findings` row present on disk with `delivered_on` lacking the lineage key: one `finding_delivery` row per id and one `additionalContext` entry, `<n> findings on <branch>, ids <a, b>, apply the delivery section of the observation skill`. The same `Stop` delivers every `placed_rules` row of the lineage with `told_on` lacking the key: one `finding_delivery` row per `report` id of the placing agent and one entry, `<n> rules placed on <branch>, <category> at <path>, apply the delivery section of the observation skill`. Deliveries count in `delivered_on` until the site closes and in `told_on` without end. Model that receives a findings entry:
1. Read the ids through the `confirmed_findings` reader
2. Validate each against the current task and act on the sites in scope
3. Write one transition per id through `transition.sql`, `fixed` with the edit's `tool_use_id`, `wrong` with the reason, `waived` on the user's word
4. Continue the task

Model that receives a rules entry, its paths rule files untracked or modified in the working tree:
1. Run `ast-grep scan --no-ignore hidden <paths of the task>` for the sites the rules cover
2. Keep or delete each rule on the user's word
3. Continue the task
