---
name: observation
description: "Use when a task reads hook event rows or writes finding rows, covering the sink, views, finding states, checker mapping, shell facts, and delivery."
user-invocable: false
---

# [OBSERVATION]

Sink `<main>/.cache/observation/observation.db` holds one row per hook event the `function-hooks` plugin records, views over the rows, and finding tables agents write, one file per repository under its main worktree. `<main>` is the first `worktree` line of `git worktree list --porcelain`, `<worktree>` the `git rev-parse --show-toplevel` line, `<branch>` the `git branch --show-current` line, and `<db>` the sink path. Readers run `mise exec -- sqlite3 -json <db> "<select>"`, writers `mise exec -- sqlite3 -bail -cmd '.timeout 10000' -json <db>` with the script on stdin, both from `<worktree>`.

## [01]-[SINK]

Table `observation(event, ts, session_id, prompt_id, agent_id, tool, tool_use_id, payload)` holds `ts` in milliseconds and `payload` as JSON under harness names, with indexes on `(session_id, ts)`, `agent_id`, `prompt_id`, and `json_extract(payload, '$.turnId')`. Columns `event`, `session_id`, `prompt_id`, `agent_id`, `tool`, and `tool_use_id` take `hook_event_name`, `session_id`, `prompt_id`, `agent_id`, `tool_name`, and `tool_use_id`, every other field stays in `payload`. Main-loop rows carry `agent_id` null, a subagent's rows its id, and the parent's `Agent` row `tool_response.agentId` equal to it. `turn.start` and `turn.step` rows come from the main loop alone, a subagent's run writes one `turn.complete` row with its `agent_id`, and an agent the plugin spawned records `SubagentStart` and `turn.complete` alone. Classic rows carry `cwd`, `transcript_path`, and `scratchpad_dir`, `permission_mode` and `effort` where the event supplies them, and subagent rows `agent_type`. `usage` on `Stop` and `SessionEnd` rows is `$.session.usage()` with `context`, `cost`, and `rateLimits`, `cost.usd` per process and zero after a resume, `usage` on turn rows `{model, input_tokens, output_tokens, cache_read_input_tokens, cache_creation_input_tokens}`. `SessionStart` rows on `resume` and `fork` add `session_title`, `seconds_since_last_response`, `context_tokens`, `prompt_cache_likely_expired`, and `estimated_cache_write_usd`:

| [INDEX] | [EVENT]               | [PAYLOAD_KEYS]                                                                                           |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `SessionStart`        | `source` (`startup`, `resume`, `clear`, `compact`, `fork`), `agent_type`, `model`                        |
|  [02]   | `UserPromptSubmit`    | `prompt`                                                                                                 |
|  [03]   | `PostToolUse`         | `tool_input`, `tool_response`, `duration_ms`                                                             |
|  [04]   | `PostToolUse` `Edit`  | `tool_response{originalFile, structuredPatch}`                                                           |
|  [05]   | `PostToolUse` `Agent` | `tool_response{agentId, agentType, resolvedModel, status, isAsync, usage, totalTokens, totalDurationMs}` |
|  [06]   | `PostToolUse` `Bash`  | `tool_response.gitOperation.commit{sha, kind, branch}` on a commit                                       |
|  [07]   | `PostToolUseFailure`  | `tool_input`, `error`, `is_interrupt`, `duration_ms`                                                     |
|  [08]   | `PostToolBatch`       | `tool_calls[]{tool_name, tool_use_id, tool_input}`                                                       |
|  [09]   | `PermissionDenied`    | `tool_input`, `reason`, auto mode alone                                                                  |
|  [10]   | `SubagentStart`       | `agent_type`                                                                                             |
|  [11]   | `SubagentStop`        | `Stop` keys without `usage`, `agent_transcript_path`, `agent_type` `''` if untyped, fork at `PreCompact` |
|  [12]   | `Stop`                | `stop_hook_active`, `last_assistant_message`, `background_tasks[]`, `session_crons[]`, `usage.cost.usd`  |
|  [13]   | `PreCompact`          | `trigger`, `custom_instructions`                                                                         |
|  [14]   | `PostCompact`         | `trigger`, `compact_summary`                                                                             |
|  [15]   | `SessionEnd`          | `reason`, `usage`                                                                                        |
|  [16]   | `tool.call`           | Denied calls alone: `deny`, `trace`, and the call's input keys (`command`, `file_path`, `content`)       |
|  [17]   | `turn.start`          | `turnId`, `text`                                                                                         |
|  [18]   | `turn.step`           | `turnId`, `index`, `model`, `effort`, `messageCount`, `stopReason`, `toolUses`, `answer`, `usage`        |
|  [19]   | `turn.complete`       | `turnId`, `answer`, `durationMs`, `isAborted`, `reason`, `refusal` on a refusal, `usage`                 |
|  [20]   | `WorktreeCreate`      | `name`                                                                                                   |
|  [21]   | `WorktreeRemove`      | `worktree_path`, reaches no plugin                                                                       |

## [02]-[VIEWS]

Views sit in `hooks/observation/sql.ts` of the plugin, each reader below spells the view's columns, `<session>` and `<prompt>` the ids a reader filters by:

```bash
# [EDITED_FILES] Every Edit, Write, and NotebookEdit row with its path and cwd
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, ts, tool, tool_use_id, file_path, cwd from edited_files where prompt_id = '<prompt>' order by ts"

# [LINEAGE] One row per subagent with its parent, spawn fields, and span
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, parent_id, agent_type, description, resolved_model, is_async, status, total_tokens, total_duration_ms, started_ts, stopped_ts from lineage where session_id = '<session>' order by started_ts"

# [PROCESSES] Process boundaries, SessionStart rows with a source other than compact
mise exec -- sqlite3 -json <db> "select session_id, ts, source from processes where session_id = '<session>' order by ts"

# [TURN_COST] Tokens per turn from turn.complete, dollars between consecutive Stop rows, usd null with no SessionStart before the Stop
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, turn_id, started_ts, model, input_tokens, output_tokens, cache_read_input_tokens, cache_creation_input_tokens, steps, duration_ms, reason, usd from turn_cost where session_id = '<session>' order by started_ts"

# [AGENT_COST] Span, tool uses, and tool time per subagent
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, agent_type, span_ms, tool_uses, tool_duration_ms, total_tokens, total_duration_ms from agent_cost where session_id = '<session>' order by span_ms desc"

# [EDIT_CHURN] Files edited more than once inside one prompt
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, file_path, edits, agents, first_ts, last_ts from edit_churn where session_id = '<session>'"

# [DENIALS] Every refusal with the command or path it refused, kind policy, permission, failure, or host
mise exec -- sqlite3 -json <db> "select ts, session_id, prompt_id, agent_id, tool, tool_use_id, kind, reason, command, file_path from denials where session_id = '<session>' order by ts"

# [SESSION_AUDIT] Prompts, agents, compactions, end reason, and dollars per session, dollars summed per process
mise exec -- sqlite3 -json <db> "select session_id, first_ts, last_ts, processes, prompts, agents, forks, compactions, compact_summary_bytes, end_reason, background_tasks_at_end, usd from session_audit order by first_ts desc limit 10"

# [COMMITS] Commits the harness classified on Bash rows
mise exec -- sqlite3 -json <db> "select ts, session_id, prompt_id, agent_id, tool_use_id, sha, kind, branch, cwd from commits where session_id = '<session>' order by ts"

# [AGENT_DIGEST] Tool use counts per agent and main loop, agent_id null for the main loop
mise exec -- sqlite3 -json <db> "select session_id, agent_id, agent_type, tool_uses, first_ts, last_ts, reads, edits, writes, bash_calls, agent_calls, denials, files from agent_digest where session_id = '<session>'"

# [REPEATED_CALLS] Same tool and tool_input more than once by one agent in one session, Read excluded by the reader
mise exec -- sqlite3 -json <db> "select session_id, agent_id, tool, tool_input, calls, first_ts, last_ts from repeated_calls where session_id = '<session>' and tool <> 'Read'"

# [EDITS_OUTSIDE_CWD] Edits with a path outside the row's cwd
mise exec -- sqlite3 -json <db> "select ts, session_id, prompt_id, agent_id, tool, tool_use_id, file_path, cwd from edits_outside_cwd where session_id = '<session>'"

# [RUNNING_AGENTS] SubagentStart rows with no later SessionEnd, own turn.complete, or Stop or SubagentStop lacking the id in background_tasks
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, agent_type, cwd, started_ts from running_agents where cwd = '<worktree>' order by started_ts"
```

## [03]-[FINDINGS]

Four finding tables in `sql.ts`, every one `strict`, their `state`, `channel`, `kind`, and `verdict` values rows of `transition_state`, `delivery_channel`, `range_kind`, and `bar_verdict`:

| [INDEX] | [TABLE]              | [PURPOSE]                                                  | [WRITER]                            |
| :-----: | :------------------- | :--------------------------------------------------------- | :---------------------------------- |
|  [01]   | `finding`            | One row per site, never updated                            | Checker statement or judgment agent |
|  [02]   | `finding_transition` | Every state change, append-only                            | Judgment agents, checks, the user   |
|  [03]   | `finding_delivery`   | One row per context line delivered, with its `lineage_key` | Delivery hook alone                 |
|  [04]   | `judged_range`       | One range per row, `kind` `edit`, key parts as columns     | Judgment agent spawned over a range |

Identity, `sha3` of the `sqlite3` shell as the one hasher and every hash lowercase hex:
- `finding_id` is `sha3` over `category`, `path`, `text_hash`, and `occurrence` joined by `char(0)`, computed by the table
- Repeat inserts are `on conflict do nothing`, `source` is `checker:<tool>` or `agent:<id>`
- `session_id`, `prompt_id`, `agent_id`, and `tool_use_id` of a finding are the last `edited_files` row of its file, `source` names the writer
- `text_hash` is `sha3` over `text` with tabs, returns, and newlines as spaces and every run of spaces as one, computed by the `site` table
- `subject_hash` is `lower(hex(sha3(readfile(<path>), 256)))` over the whole file at observation, `''` for a gone path, equal to no stored hash
- `occurrence` is the ordinal of `text` within the file at observation
- `path` is relative to `<worktree>`, lines and columns are one-based, `byte_start` and `byte_end` zero-based
- `lineage_key` is `<main>/<worktree>/<branch>`, generated by `judged_range` from its columns, `<branch>` empty on a detached head
- Timestamps are `cast(unixepoch('subsec') * 1000 as integer)`
- `by` is `user`, `agent:<id>`, or `check:<tool>`, `<id>` the writer's `agent_id` in a subagent and its `session_id` on the main loop
- `evidence` is required for `wrong`, `waived`, `checker_owned`, and `checker_silent`
- `verdict` is the verifier's bar verdict on its `confirmed`, a `bar_verdict` row, carried by a re-confirm and a move, null on every other transition
- `bar_verdict` opens empty, the judging agent fills it from its rubric's bar table through the bar statement
- Retired verdicts keep their `bar_verdict` row, `finding_transition.verdict` references it
- `confirmed` under another bar leaves `verdict` null, which `recurring_categories` reads as not refused

| [INDEX] | [STATE]          | [MEANING]                                              | [WRITER]                     | [EVIDENCE]                 |
| :-----: | :--------------- | :----------------------------------------------------- | :--------------------------- | :------------------------- |
|  [01]   | `proposed`       | Judgment agent claims the site violates the standard   | `agent:<id>`                 | Correction in one line     |
|  [02]   | `confirmed`      | Present at the hash, fix keeps behavior                | `agent:<id>`, `check:<tool>` | Proof line, `verdict`      |
|  [03]   | `wrong`          | Fix changes behavior, or false positive                | `agent:<id>`                 | Reason, final at this hash |
|  [04]   | `checker_owned`  | Checker rule states the category, a checker row covers | `agent:<id>`                 | `<tool>:<rule id>`         |
|  [05]   | `checker_silent` | Checker rule states the category and missed the site   | `agent:<id>`                 | `<tool>:<rule id>`         |
|  [06]   | `fixed`          | Text absent after an edit whose removed lines held it  | `check:<tool>`               | `tool_use_id` or `null`    |
|  [07]   | `vanished`       | Text absent with no edit removing it, or `path` gone   | `check:<tool>`               | `null`                     |
|  [08]   | `moved`          | Text at a path the range's edits name, same occurrence | `check:<tool>`               | `successor` column, new id |
|  [09]   | `waived`         | User accepts the site as is                            | `user`                       | Reason                     |

Readers derive staleness and coverage: a row is stale when `subject_hash` of its latest `confirmed` differs from `lower(hex(sha3(readfile(path), 256)))` at read time, and covered when its latest transition is `checker_owned`, its evidence the checker rule stating its correction, a checker row on the same span with another correction covers nothing. `readfile` runs in a statement alone, never inside a view, and from `<worktree>`:

```bash
# [OWN_ID] Writer's <id> and <start>, a subagent from its SubagentStart row by <agent>, the main loop from a Bash row holding its own <literal>
mise exec -- sqlite3 -json <db> "select agent_id, ts from observation where event = 'SubagentStart' and json_extract(payload, '$.agent_type') = '<agent>' and (json_extract(payload, '$.cwd') = '<worktree>' or json_extract(payload, '$.cwd') like '<worktree>/%') order by ts desc limit 1"
mise exec -- sqlite3 -json <db> "select session_id, ts from observation where event = 'PostToolUse' and tool = 'Bash' and json_extract(payload, '$.tool_input.command') like '%<literal>%' order by ts desc limit 1"

# [BAR_VERDICTS] Verdicts and whether each earns a rule
mise exec -- sqlite3 -json <db> "select verdict, earns from bar_verdict order by earns desc, verdict"

# [BAR] Verdict row before a transition names <verdict>, <earns> 1 where the verdict earns a rule
mise exec -- sqlite3 -bail -cmd '.timeout 10000' -json <db> "insert into bar_verdict(verdict, earns) values ('<verdict>', <earns>) on conflict(verdict) do update set earns = excluded.earns"

# [FINDING_STATE] Every finding with its latest transition and last_closed_at
mise exec -- sqlite3 -json <db> "select finding_id, category, path, text, text_hash, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, severity, message, replacement, source, session_id, prompt_id, agent_id, tool_use_id, observed_at, state, subject_hash, at, by, evidence, verdict, successor, last_closed_at from finding_state where finding_id in ('<a>', '<b>')"

# [CONFIRMED_FINDINGS] agent:* findings with latest transition confirmed and no wrong at the same hash
mise exec -- sqlite3 -json <db> "select finding_id, category, path, text, occurrence, start_line, start_column, end_line, end_column, message, replacement, source, session_id, prompt_id, agent_id, subject_hash, confirmed_at, evidence, verdict, last_closed_at from confirmed_findings where finding_id in ('<a>', '<b>')"

# [OPEN_FINDINGS] Confirmed rows, delivered_on keys told of it or a predecessor since the last close, gone at SessionEnd or non-report PostCompact
mise exec -- sqlite3 -json <db> "select finding_id, category, path, text, occurrence, start_line, start_column, end_line, end_column, message, replacement, source, session_id, prompt_id, agent_id, subject_hash, confirmed_at, evidence, verdict, last_closed_at, delivered_on from open_findings where subject_hash = lower(hex(sha3(readfile(path), 256))) and not exists (select 1 from json_each(delivered_on) where value = '<lineage_key>')"

# [RECURRING_CATEGORIES] Two or more confirmed sites, no refused verdict, reported_on their report keys
mise exec -- sqlite3 -json <db> "select category, sites, sites_at, first_at, last_at, reported_on from recurring_categories order by sites desc"

# [JUDGED_EDITS] In-tree edits inside an edit range of their deepest worktree
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, ts, tool, tool_use_id, file_path, cwd, lineage_key from judged_edits where cwd = '<worktree>' order by ts"

# [UNJUDGED_EDITS] In-tree edits no edit range covers
mise exec -- sqlite3 -json <db> "select session_id, prompt_id, agent_id, ts, tool, tool_use_id, file_path, cwd from unjudged_edits where cwd = '<worktree>' order by ts"

# [CATEGORY_FIRES] Sites, check:* transitions, and prompts per checker:* category, beside prompts judged
mise exec -- sqlite3 -json <db> "select category, sites, sightings, prompts_fired, prompts_judged, first_at, last_at from category_fires order by sightings desc"

# [MISSED_SITES] Sites with latest transition checker_silent
mise exec -- sqlite3 -json <db> "select finding_id, category, path, start_line, start_column, evidence, at from missed_sites order by category, path, start_line"
```

One writer process from `<worktree>` writes a batch: the pragma, `begin immediate`, the `site` table, one statement per source filling it, the two insert statements, then `commit`, with `<state>` `confirmed` and `<by>` `check:<tool>` for a checker source, `proposed` and `agent:<id>` for an agent source. First `returning` lists the sites new to the table, the second every site of the batch, a nonzero exit leaves nothing applied:

```sql
-- [BATCH] Site table with one values row per site of an agent source, a checker source runs its mapping statement in place of the values row
pragma foreign_keys = on;
begin immediate;
create temp table site(category text not null, path text not null, text text not null, text_hash text generated always as (lower(hex(sha3(replace(replace(replace(replace(replace(replace(text, char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' '), 256)))) stored, occurrence integer not null, start_line integer not null, start_column integer not null, end_line integer not null, end_column integer not null, byte_start integer, byte_end integer, subject_hash text not null, severity text, message text not null, replacement text, source text not null) strict;
insert into site(category, path, text, occurrence, start_line, start_column, end_line, end_column, subject_hash, message, replacement, source) values ('<category>', '<path>', '<text>', <occurrence>, <start_line>, <start_column>, <end_line>, <end_column>, lower(hex(sha3(readfile('<path>'), 256))), '<message>', <replacement or null>, 'agent:<id>');
-- [INSERT] Finding rows new to the table, then one transition per site of the batch
insert into finding(category, path, text, text_hash, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source, session_id, prompt_id, agent_id, tool_use_id, observed_at) select s.category, s.path, s.text, s.text_hash, s.occurrence, s.start_line, s.start_column, s.end_line, s.end_column, s.byte_start, s.byte_end, s.subject_hash, s.severity, s.message, s.replacement, s.source, e.session_id, e.prompt_id, e.agent_id, e.tool_use_id, cast(unixepoch('subsec') * 1000 as integer) from site s left join (select file_path, session_id, prompt_id, agent_id, tool_use_id, max(ts) as ts from edited_files group by file_path) e on e.file_path = '<worktree>/' || s.path where true on conflict do nothing returning finding_id;
insert into finding_transition(finding_id, state, subject_hash, start_line, start_column, end_line, end_column, occurrence, at, by, evidence) select f.finding_id, '<state>', s.subject_hash, s.start_line, s.start_column, s.end_line, s.end_column, s.occurrence, cast(unixepoch('subsec') * 1000 as integer), '<by>', s.message from site s join finding f on f.category = s.category and f.path = s.path and f.occurrence = s.occurrence and f.text_hash = s.text_hash returning finding_id;
commit;
```

One transition alone, the lifecycle statements whose predicate is the check, and the ledger row of a range run, each script opening with `pragma foreign_keys = on;`:

```sql
-- [TRANSITION] <state> with <by> and <evidence> of the states table, <verdict> a bar_verdict row on the verifier's confirmed, null otherwise
insert into finding_transition(finding_id, state, subject_hash, at, by, evidence, verdict) values ('<finding_id>', '<state>', lower(hex(sha3(readfile('<path>'), 256))), cast(unixepoch('subsec') * 1000 as integer), '<by>', '<evidence>', <verdict>) returning finding_id, state;
-- [RECONFIRM] Confirmed again at a new hash while the latest is confirmed and the file holds the text, evidence and verdict carried
with latest as (select state, subject_hash, evidence, verdict from finding_transition where finding_id = '<finding_id>' order by at desc, rowid desc limit 1) insert into finding_transition(finding_id, state, subject_hash, start_line, start_column, end_line, end_column, occurrence, at, by, evidence, verdict) select f.finding_id, 'confirmed', lower(hex(sha3(readfile(f.path), 256))), <start_line>, <start_column>, <end_line>, <end_column>, <occurrence>, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', l.evidence, l.verdict from finding f, latest l where f.finding_id = '<finding_id>' and l.state = 'confirmed' and instr(readfile(f.path), f.text) > 0 and lower(hex(sha3(readfile(f.path), 256))) <> l.subject_hash returning finding_id, state;
-- [FIXED] After edit <tool_use_id> with removed lines holding the text, null with no row of it, nothing while the file holds the text or is gone
insert into finding_transition(finding_id, state, subject_hash, at, by, evidence) select finding_id, 'fixed', lower(hex(sha3(readfile(path), 256))), cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', <tool_use_id or null> from finding where finding_id = '<finding_id>' and instr(replace(replace(replace(replace(replace(replace(cast(readfile(path) as text), char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' '), replace(replace(replace(replace(replace(replace(text, char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' ')) = 0 returning finding_id, state;
-- [MOVED_FILE] New path among edited_files paths and git mv targets between <from_ts> and <to_ts> holding the text at least occurrence times
with candidate as (select distinct e.file_path from edited_files e where e.ts between <from_ts> and <to_ts> and e.file_path <> '<worktree>/' || (select path from finding where finding_id = '<old>') union select '<worktree>/' || trim(substr(json_extract(o.payload, '$.tool_input.command'), instr(json_extract(o.payload, '$.tool_input.command'), ' ' || f.path || ' ') + length(f.path) + 2)) from observation o join finding f on f.finding_id = '<old>' where o.event = 'PostToolUse' and o.tool = 'Bash' and o.ts between <from_ts> and <to_ts> and json_extract(o.payload, '$.tool_input.command') like '%git mv ' || f.path || ' %') select f.finding_id, substr(c.file_path, length('<worktree>') + 2) as new_path from finding f join candidate c where f.finding_id = '<old>' and readfile(c.file_path) is not null and (length(readfile(c.file_path)) - length(cast(replace(readfile(c.file_path), cast(f.text as blob), x'') as blob))) / length(cast(f.text as blob)) >= f.occurrence;
-- [MOVED] Successor row at <new path> with the old identity columns, moved on <old> naming it, confirmed on it with the old evidence and verdict
insert into finding(category, path, text, text_hash, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source, session_id, prompt_id, agent_id, tool_use_id, observed_at) select f.category, '<new path>', f.text, f.text_hash, f.occurrence, f.start_line, f.start_column, f.end_line, f.end_column, f.byte_start, f.byte_end, lower(hex(sha3(readfile('<new path>'), 256))), f.severity, f.message, f.replacement, f.source, e.session_id, e.prompt_id, e.agent_id, e.tool_use_id, cast(unixepoch('subsec') * 1000 as integer) from finding f left join (select file_path, session_id, prompt_id, agent_id, tool_use_id, max(ts) as ts from edited_files group by file_path) e on e.file_path = '<worktree>/<new path>' where f.finding_id = '<old>' on conflict do nothing;
insert into finding_transition(finding_id, state, subject_hash, at, by, successor) select f.finding_id, 'moved', (select subject_hash from finding_transition where finding_id = f.finding_id order by at desc, rowid desc limit 1), cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', s.finding_id from finding f join finding s on s.category = f.category and s.text_hash = f.text_hash and s.occurrence = f.occurrence and s.path = '<new path>' where f.finding_id = '<old>' returning finding_id, successor;
insert into finding_transition(finding_id, state, subject_hash, start_line, start_column, end_line, end_column, occurrence, at, by, evidence, verdict) select s.finding_id, 'confirmed', s.subject_hash, s.start_line, s.start_column, s.end_line, s.end_column, s.occurrence, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', v.evidence, v.verdict from finding f join finding s on s.category = f.category and s.text_hash = f.text_hash and s.occurrence = f.occurrence and s.path = '<new path>' join finding_transition v on v.rowid = (select rowid from finding_transition where finding_id = f.finding_id and state = 'confirmed' order by at desc, rowid desc limit 1) where f.finding_id = '<old>' returning finding_id;
-- [VANISHED] Head hash as subject_hash, '' for a gone path, no evidence, nothing while the file holds the text
with head as materialized (select finding_id, text, readfile(path) as body from finding where finding_id = '<finding_id>') insert into finding_transition(finding_id, state, subject_hash, at, by) select finding_id, 'vanished', lower(hex(sha3(body, 256))), cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3' from head where body is null or instr(replace(replace(replace(replace(replace(replace(cast(body as text), char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' '), replace(replace(replace(replace(replace(replace(text, char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' ')) = 0 returning finding_id, state, subject_hash;
-- [LEDGER] Range run row, from_ts the lineage's last to_ts or 0
insert into judged_range(kind, main_worktree, worktree, branch, from_ts, to_ts, agent_id, at) values ('edit', '<main>', '<worktree>', '<branch>', <from_ts>, <to_ts>, '<agent_id>', cast(unixepoch('subsec') * 1000 as integer));
```

## [04]-[MAPPING]

Checker JSON becomes `site` rows: ast-grep through one statement, ruff, biome, and Roslyn through one `hit` insert each and the shared statement slicing text and bytes from the file. Commands and statements run from `<worktree>` with paths relative to it, `<out>` the JSON each command wrote, named anything but `biome.json`, which biome reads as its configuration. ast-grep, ruff, and biome exit 1 on a finding, an empty `<out>` marks a checker failure and `json_each` over it raises, a missing one writes zero rows. Roslyn's exit is `ls`'s, nonzero when no log was written, a result with no location is dropped, and one log lands per compiled project, so `<project>` is the one owning the scope's `.cs` files and `--no-incremental` compiles it when up to date:

```bash
# [AST_GREP] Rule hits as JSON
ast-grep scan --no-ignore hidden --json=compact <paths> > <out>
# [RUFF] Diagnostics as JSON
ruff check --output-format json <paths> > <out>
# [BIOME] Diagnostics as JSON
biome lint --reporter=json <paths> > <out>
# [ROSLYN] SARIF 2.1 with unencoded file URIs and one-based columns, ls fails where no compiler wrote the log
dotnet build <project> --no-dependencies --no-incremental -p:ErrorLog=<out>%2Cversion=2.1; ls <out>
```

```sql
-- [AST_GREP] Hits to site, text and bytes from the checker
with match as (select value ->> '$.ruleId' as rule_id, value ->> '$.file' as file, value ->> '$.text' as text, value ->> '$.range.byteOffset.start' as byte_start, value ->> '$.range.byteOffset.end' as byte_end, value ->> '$.range.start.line' as start_line, value ->> '$.range.start.column' as start_column, value ->> '$.range.end.line' as end_line, value ->> '$.range.end.column' as end_column, value ->> '$.severity' as severity, value ->> '$.message' as message, value ->> '$.replacement' as replacement from json_each(cast(readfile('<out>') as text))), content as (select distinct file, readfile(file) as blob from match) insert into site(category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source) select 'ast-grep:' || m.rule_id, m.file, m.text, (length(substr(c.blob, 1, m.byte_start)) - length(cast(replace(substr(c.blob, 1, m.byte_start), cast(m.text as blob), x'') as blob))) / length(cast(m.text as blob)) + 1, m.start_line + 1, m.start_column + 1, m.end_line + 1, m.end_column + 1, m.byte_start, m.byte_end, lower(hex(sha3(c.blob, 256))), m.severity, m.message, m.replacement, 'checker:ast-grep' from match m join content c on c.file = m.file;
-- [HIT] One-based lines and character columns from the ruff, biome, and roslyn inserts, read by the shared statement
create temp table hit(category text not null, path text not null, file text not null, start_line integer not null, start_column integer not null, end_line integer not null, end_column integer not null, severity text, message text not null, source text not null) strict;
-- [RUFF] Diagnostics to hit
insert into hit select 'ruff:' || (value ->> '$.code'), substr(value ->> '$.filename', length('<worktree>') + 2), value ->> '$.filename', value ->> '$.location.row', value ->> '$.location.column', value ->> '$.end_location.row', value ->> '$.end_location.column', value ->> '$.severity', value ->> '$.message', 'checker:ruff' from json_each(cast(readfile('<out>') as text));
-- [BIOME] Diagnostics to hit
insert into hit select 'biome:' || (value ->> '$.category'), value ->> '$.location.path', value ->> '$.location.path', value ->> '$.location.start.line', value ->> '$.location.start.column', value ->> '$.location.end.line', value ->> '$.location.end.column', value ->> '$.severity', value ->> '$.message', 'checker:biome' from json_each(cast(readfile('<out>') as text), '$.diagnostics');
-- [ROSLYN] Results with a location to hit
insert into hit select 'roslyn:' || (value ->> '$.ruleId'), substr(value ->> '$.locations[0].physicalLocation.artifactLocation.uri', length('file://<worktree>') + 2), substr(value ->> '$.locations[0].physicalLocation.artifactLocation.uri', 8), value ->> '$.locations[0].physicalLocation.region.startLine', value ->> '$.locations[0].physicalLocation.region.startColumn', value ->> '$.locations[0].physicalLocation.region.endLine', value ->> '$.locations[0].physicalLocation.region.endColumn', value ->> '$.level', value ->> '$.message.text', 'checker:roslyn' from json_each(cast(readfile('<out>') as text), '$.runs[0].results') where value ->> '$.locations[0].physicalLocation.artifactLocation.uri' is not null;
-- [SHARED] Hit to site, text and bytes sliced from the file
with content as materialized (select path, readfile(min(file)) as blob from hit group by path), line as (select c.path, l.key + 1 as n, 1 + coalesce(sum(length(cast(l.value as blob)) + 1) over (partition by c.path order by l.key rows between unbounded preceding and 1 preceding), 0) as pos, l.value as text from content c, json_each('[' || replace(replace(json_quote(cast(c.blob as text)), '\\', '\/'), '\n', '","') || ']') l), span as (select h.*, s.pos + length(cast(substr(s.text, 1, h.start_column - 1) as blob)) as byte_start, e.pos + length(cast(substr(e.text, 1, h.end_column - 1) as blob)) as byte_end, c.blob from hit h join content c on c.path = h.path join line s on s.path = h.path and s.n = h.start_line join line e on e.path = h.path and e.n = h.end_line) insert into site(category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source) select category, path, cast(substr(blob, byte_start, byte_end - byte_start) as text), (length(substr(blob, 1, byte_start - 1)) - length(cast(replace(substr(blob, 1, byte_start - 1), substr(blob, byte_start, byte_end - byte_start), x'') as blob))) / (byte_end - byte_start) + 1, start_line, start_column, end_line, end_column, byte_start - 1, byte_end - 1, lower(hex(sha3(blob, 256))), severity, message, null, source from span;
```

## [05]-[SHELL]

- `-json` renders a text column as a JSON string, a reader wanting `payload` nested pipes through `jq '[.[] | .payload |= fromjson]'`
- `pragma foreign_keys = on` precedes `begin`, inside a transaction it is ignored and an orphan `finding_id` is written at exit 0
- Every interpolated value doubles each `'`
- `->>` binds looser than `||`, a key reached beside a concatenation uses `json_extract` or parentheses
- `insert ... select ... on conflict` is a parse error, `where true` precedes `on conflict`
- `sha3` and `readfile` answer in the `sqlite3` shell alone
- `sqlite_query` runs its select in SQLite and types every column `VARCHAR`, a number takes a `cast` under an aggregate
- `s.<view>` rebinds a view in DuckDB's dialect and raises
- Transcripts read through the glob, a deleted transcript passed by name raises `IO Error`

```bash
# [DUCKDB_VIEW] View inside a DuckDB query, for a join to DuckDB data alone
duckdb -json -c "attach '<db>' as s (type sqlite, read_only); select count(*) as turns, sum(cast(usd as double)) as usd from sqlite_query('s', 'select * from turn_cost')"
# [TRANSCRIPT] Messages, model, and tokens of one transcript, <path> a Stop.transcript_path or a SubagentStop.agent_transcript_path
duckdb -json -c "select count(*) filter (type = 'assistant') as messages, any_value(message.model) as model, sum(message.usage.output_tokens)::bigint as output_tokens, sum(message.usage.cache_read_input_tokens)::bigint as cache_read_tokens, sum(message.usage.cache_creation_input_tokens)::bigint as cache_creation_tokens from read_json_auto('<path>', union_by_name = true, ignore_errors = true)"
# [AGENT_TRANSCRIPTS] Agent cost from transcripts joined to SubagentStop rows, background agents included
duckdb -json -c "attach '<db>' as s (type sqlite, read_only); select r.agent_id, any_value(r.agent_type) as agent_type, count(*) filter (t.type = 'assistant') as messages, any_value(t.message.model) as model, sum(t.message.usage.output_tokens)::bigint as output_tokens, sum(t.message.usage.cache_read_input_tokens)::bigint as cache_read_tokens, sum(t.message.usage.cache_creation_input_tokens)::bigint as cache_creation_tokens from read_json_auto('~/.claude/projects/*/*/subagents/*.jsonl', union_by_name = true, filename = true, ignore_errors = true) t join (select agent_id, json_extract_string(payload, '$.agent_transcript_path') as path, json_extract_string(payload, '$.agent_type') as agent_type from s.observation where event = 'SubagentStop') r on r.path = t.filename group by r.agent_id order by output_tokens desc"
```

## [06]-[DELIVERY]

At `Stop` the plugin reads `open_findings` at the head hash whose `delivered_on` lacks the lineage key, writes one `finding_delivery` row per id, and answers one `additionalContext` entry, `<n> findings on <branch>, ids <a, b>, apply the delivery section of the observation skill`, nothing at zero rows. A `report` row, one per site handed to a spawned agent with `agent_id` null, counts in `delivered_on` and `reported_on` until the site's next close or the delivering session's end, the spawned agent outliving a compaction and not the session. Model that receives the entry:
1. Read the ids through the `confirmed_findings` reader
2. Validate each against the current task and act on the sites in scope
3. Write one transition per id acted on, `fixed` with the edit's `tool_use_id`, `wrong` with the reason, `waived` on the user's word alone
4. Continue the task
