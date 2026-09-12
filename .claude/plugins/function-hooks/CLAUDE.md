# [FUNCTION_HOOKS]

Layers of the plugin from the hooks module up to the units a purpose adds, each built on the capability of the layer beneath, with the touch point that extends each layer and what a change there costs. Use `observation` for every name, script, and command against the sink, `plugin-authoring` for the harness contract of a hooks module.

## [01]-[ENGINE]

`hooks/register.ts` is the one module the harness loads, holds every registration and every `$` call, and turns each event it records into one row:
- `tool.call` folds `hooks/policies/` through `hooks/events/tool-call.ts`, wait and script policies on Bash alone, the first refusal answers
- Bash and Monitor commands take one `ast-grep` process per call and one more per `sh -c` or `eval` body to depth 8, an unread command is refused
- Deny from a policy or a plugin beneath is recorded on `tool.call`, a host permission denial as `PermissionDenied`, a passed call as `PostToolUse`
- `classic.*` records the events its matcher lists, `turn.*` the turn events, `ui.render` on `SessionMode` draws the footer label
- `record` is the one path every row takes: event name, value, a `Columns` selection, and the clock, session id from the selection or the harness
- `hooks/observation/row.ts` builds the row: selected fields become columns, every other field stays in `payload` under its harness name
- Drops are file bodies and images of Read, Write, and Edit calls, batch call responses, and deny trace values
- `script` of `hooks/observation/sql.ts` renders one insert, one awaited `sqlite3` process writes it before `next(e)`, a deny row after the answer
- `sqlite3` is `bin/sqlite3` under the install `mise where sqlite` names, read once per load, a `mise exec` launcher costs more than a write
- Failed write is one `$.ui.log` line naming the row, the row is lost, nothing retries
- `open` runs once per load at its first recorded event, one `sqlite3` process applying the declarations, a second switching the journal to WAL
- Failed open is one log line and no rows until reload, a session outside a git repository opens nothing
- Lost journal switch at a concurrent first open of a fresh sink is one log line, the sink stays open in its journal mode
- Boundary branch runs after the row at `Stop` and at a `SubagentStop` whose `agent_type` is not blank, both with `stop_hook_active` false
- Boundary reads the lineage from git, runs one state query, spawns due agents by name, writes delivery rows, and answers `additionalContext` entries
- `hooks/observation/delivery.ts` holds the boundary's pure parts: settings, statements, state parsing, due predicates, prompts, and texts
- Environment held from `register` until reload: settings read once, spawn claims, logged texts, and the footer label empty at the start
- Footer label is set at boundary events alone and cleared at `SessionEnd`, no tool event costs a state query
- Claim holds an agent name from its spawn call until the spawn resolves, later boundary events list the running agent under `background_tasks`
- Run of the agent from another session on the worktree is a `running_agents` row the state query reads, the trigger waits on it
- Boundary event with no `background_tasks` field reads state and sets the footer, then skips spawn and delivery, one log line names the absence
- Two sessions of one lineage stopping inside one state read both spawn, their duplicate ledger and delivery rows read as one range and one key
- Session killed with no end row leaves its spawn's start row standing, triggers naming that agent wait until the session resumes and stops
- Hook that throws, outruns its budget, or answers a wrong shape is skipped and the event's answer stands, a module defect costs rows, never the turn
- `hooks/composition/` holds `Decision` for a policy, `Result` for a process, `Option` for absence, `hooks/text/` the parser and path helper

## [02]-[SINK]

One SQLite file per repository, at the path `observation` names, holds every row the engine writes and every table an agent writes:
- Rows are one table with identity columns beside one JSON `payload`, written once, updated and deleted by nothing in the plugin
- Payload key the harness adds lands on new rows with no change, older rows answer null to `json_extract` over it
- Views are the `_VIEWS` elements of `sql.ts`, each one question over rows, dropped and created at every open, a new body applies at the next load
- Views reach other views through joins, a correlated subquery over a view runs its body once per outer row
- Lookup tables hold every state, channel, and kind, `_ROWS` adds a declared value at open and retires one no row references
- `bar_verdict` holds the verdict shape and no declared row
- Finding tables of `_TABLES` hold category, path, text, span, message, and source, never a shape, a rubric, or a purpose, any judgment writes them
- Writers are disjoint, agents write finding, transition, ledger, and bar rows through the skill, the engine observation, delivery, and enum rows
- `text_hash` and `finding_id` are generated columns fixed once rows exist, a rebuild recomputes them while transitions keep the old ids
- Moved site keeps its id, its `moved` transition carries the new path, `finding_state` reads the current path from the latest transition
- Evolution is `open`: declarations are the schema, the delta to the file is applied at each load's first event, a refused delta rolls back whole
- Delta file beside the sink holds the drops and rebuilds the last open computed, the record of a refused open
- Rebuild refuses a `not null` column with no default over rows, a renamed strict key, a declaration sharing no stored column, a check old rows fail
- Column added over rows carries a default, a check over a fact old rows lack stays the writer's gate
- Declared and stored names match case-insensitively
- Retired view goes at the next open, a retired table or index stays until a statement drops it
- Engine scripts run under `-bail`, without it the shell continues past a failed statement and reaches `commit`
- `views.test.ts` runs the open in memory through `node:sqlite`, prepares every view, and rebuilds a changed table over rows, the proof under `check`
- Several processes and worktrees write one file at once, WAL and the busy timeout serialize them, a reader never waits on a writer
- Finding rows are never deleted, a refused category spawn takes its `report` delivery rows back, removing `.cache/observation/` is the reset

## [03]-[SKILL]

`.claude/skills/observation/SKILL.md` is the read and write contract between the sink and every reader and writer above it:
- Skill owns every write script, every view reader, and every name a row, view, state, or channel carries, an agent's file holds its scope selects
- Skill's `references/sqlite.md` holds the SQLite and DuckDB facts that decide a script's form, one section per documentation page
- Verdict names stay in the rubric skill that judges them, the skill's `bar.sql` writes them into `bar_verdict`
- Skill owns the checker mapping, one script per checker turning its JSON into site rows, a new checker is one more script there
- To a specialization the skill is preloaded whole at spawn through `skills`, an agent's file names a script and restates none of it
- Main agent loads the skill by its description when it reads rows or acts on a delivered line
- Skill holds the main agent's side of the contract: what the context line asks and the transitions that close it
- Reader is a command under an allow row the tree holds, never a registered tool, a tool wraps the command and adds no fact

## [04]-[SPECIALIZATION]

Purpose above the sink is one of three units, chosen by what the purpose needs that the layers beneath lack, each touching its owner alone:
- View answers a question a query over rows answers: one `_VIEWS` element of `sql.ts`, its reader in the skill, and the views test's count
- View never touches `register.ts`, `row.ts`, an agent, or an option, and reads what rows already hold
- Agent profile answers a judgment needing the working tree or a rubric: one file under `.claude/agents/` preloading `observation` and the rubric
- Agent reads rows and the working tree, writes finding rows through the skill's scripts, and touches nothing in the plugin beyond its name
- Rubric stays in its owning skill, `ast-grep` for shapes, `clean-prose` for prose, the agent holds run order, prompt-supplied scope, and its gate
- Option pair answers a trigger: `<kind>Threshold` and `<kind>Agent` in `userConfig`, a trigger in `delivery.ts` naming a view, a `range_kind` row
- Option pair touches no row or agent, names a count and never a purpose, its spawn joins the judging step and its kind the skill's `ledger.sql`
- Option values come from `pluginConfigs` of user settings, `--settings`, or managed settings, a project `.claude/settings.json` reaches no option
- Options are read once at `register`, a changed value waits for the plugin's reload
- Category trigger is the second pair, `categoryThreshold` and `categoryAgent` over recurring confirmed categories, free of purpose the same way
- Category threshold defaults to 0 while `CLAUDE.md` keeps a rule edit a task the user asks for
- Prompt a spawn passes is the range or the category with the lineage key alone and `cwd` at the worktree, the agent derives the rest from rows
- Spawned agent runs in the session's process on its file's model, else the session's, its reply is its `turn.complete` `answer` and opens no turn
- Purpose whose evidence no row holds has its gap at the engine, a payload key or a matcher entry, the layers above read rows alone

## [05]-[OBSERVED]

Rows hold every event the module registers, from every session, process, and worktree of the repository, and nothing a dispatch does not carry:
- Every dispatched classic event the matcher lists, every `turn.*` event, and every refused `tool.call` lands, one row per dispatch at `$.clock`
- Subagents run in the session's process through the same module
- Running spawn is listed under `background_tasks` at boundary events, its `turn.complete` row ends it in `running_agents`
- Classic events outside the matcher reach no row, `PreToolUse` among them
- `Stop` fires before the turn's last transcript records and `SessionEnd` sees them, `/clear` ends one session id and starts another
- Transcript text and per-message usage stay in the transcript files, rows keep the paths as pointers the skill's transcript scripts read
- Shell rewrite of a file (`sd`, `sed -i`, `perl -i`, a redirect) leaves no patch row, `edited_files` and the lifecycle's `-` line see none
- Remote-isolated subagent runs elsewhere and lands no row, its `Agent` row holds `remote_launched`, `taskId`, and `sessionUrl`, no `agentId`

## [06]-[OTHER_PURPOSES]

Loop with another purpose, one whose evidence is harness events, runs on the same layers and touches nothing in the plugin:
- Rows hold the evidence: files edited with their patches, commands run, agents spawned, denials, compactions, tokens, and dollars
- Question is a query an agent runs through the skill's read command, a view joins `sql.ts` when the query recurs across readers
- Judgment is one agent under `.claude/agents/` preloading `observation` and its own rubric's skill, writing `finding` rows under its own categories
- Verification is the agent's concern: a judgment writes `proposed` and its verifier confirms, a checker source writes `confirmed` and never delivers
- Trigger is the existing count, `editAgent` set to the new agent replaces the one it names over the same edits, or a person's `Agent` call
- Delivery and closing come with the layers, confirmed `agent:*` rows at the head hash undelivered on the lineage reach the main agent at `Stop`
- Category trigger comes the same way: two confirmed sites of one category with no refusing verdict make it recurring, `categoryAgent` names who acts
- Case, documentation drift: an agent reads `edited_files` of its range, finds prose naming the edited paths, and writes findings the loop delivers

## [07]-[COST]

Change at a level costs in proportion to what sits above it:
- Engine change touches every row after it, a dropped payload key is gone from those rows for good, a column change reaches every view and reader
- Engine change is proven by a live session after `claude plugin validate`, the typecheck, and the views test pass
- Engine defect costs a row per event it fails on until reload, a fault the module catches is one `$.ui.log` line in the transcript and the debug log
- Boundary costs two git processes and one state query at every stop event, a delivery or a report one write more
- Delivered context line costs the main agent one more model step at that `Stop`
- Sink change applies at the next load in every session, a table body change rebuilds the table, a view change reaches every reader from then
- Sink change without its skill change is a drift no checker reports, an agent's command fails first
- Skill change runs in every agent that preloads it on its next spawn, a wrong script writes wrong rows under every purpose
- Skill script is proven by running it against a copy of the sink, the skill's own command form is the proof
- Agent change costs model tokens per run and rows others act on, `confirmed` reaches the main agent at the next `Stop`, `proposed` waits
- Option change costs cadence, a lower threshold spawns more often over smaller ranges, a name matching no definition refuses the spawn, logged
- View change costs nothing at write time and one query per reader, a wrong view answers wrong rows to every reader, the views test proves it prepares
