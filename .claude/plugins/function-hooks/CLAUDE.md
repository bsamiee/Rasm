# [FUNCTION_HOOKS]

Policies refuse a tool call, every recorded event becomes a sink row, and stop events spawn judging agents and deliver findings. Use `observation` for every sink name, script, and reader.

[TOOL_CALL]: Function hooks hold one decision the harness takes on every tool call before the tool runs, `deny` or `next`
- ALWAYS use `plugin-authoring` skill for writing or changing a function hook
- ALWAYS write a policy as a pure function from the parsed call to a decision, the registered hook alone reads `$` and answers `deny` or `next`
- ALWAYS fold every policy under the one `tool.call` registration, the first refusal is the call's answer

## [01]-[POLICIES]

Each refusal states what the call does, then the target, MCP tool, or owner file that is the path:
- Parsing a Bash or Monitor command costs one `ast-grep` process per call and one more per `sh -c` or `eval` body to depth 8
- Option lists a walker policy splits by are read from the installed binary, a valued option missing there makes its value a start path
- One `git check-ignore` process per written path resolves ignored paths
- Refusal is proven headless through `--plugin-dir` and read back from the `denials` view

## [02]-[RECORDING]

Every classic event the matcher lists, every `turn.*` event, and every refused `tool.call` becomes one row stamped from `$.clock`:
- Classic events outside the matcher reach no row
- Read, Write, and Edit call bodies, batch call responses, and deny trace values drop before the write
- One awaited `sqlite3` process writes the row before `next(e)`, a deny row after the answer
- `sqlite3` is `bin/sqlite3` under the install `mise where sqlite` names, resolved once per load
- Scan, locate, open, and row writes run at the repository root, where `mise` resolves their binaries
- Failed write is one `$.ui.log` line naming the row, the row is lost and nothing retries
- Shell rewrite of a file (`sd`, `sed -i`, a redirect) writes no edit row, the edit trigger counts none
- `ui.render` on `SessionMode` draws the footer label, set at a boundary event alone and cleared at `SessionEnd`

## [03]-[SCHEMA]

Declarations of `hooks/observation/sql.ts` are the schema, applied as a delta at a load's first recorded event, a refused delta rolls back whole:
- Delta file beside the sink holds the drops and rebuilds the last open computed
- Views drop and create at every open, a new body applies at the next load
- Lookup table takes a declared value at open and retires one no row references
- Rebuild refuses a `not null` column with no default over rows, a renamed strict key, a declaration sharing no stored column, a check old rows fail
- Column added over rows holds a default, a check over a fact old rows lack stays the writer's gate
- Declared and stored names match case-insensitively
- Retired view goes at the next open, a retired table or index stays until a statement drops it
- Generated columns recompute over a rebuilt table while transitions keep the ids stored before it
- Rows insert once, no statement updates or deletes one
- Payload key the harness adds reaches new rows alone, older rows answer null to `json_extract` over the key
- Scripts run under `-bail`, a failed statement without it reaches `commit`
- Failed open is one log line and no rows until reload, a session outside a git repository opens nothing
- Lost journal switch at a concurrent first open is one log line, the sink stays open in its journal mode
- WAL and the busy timeout serialize the processes and worktrees writing one file
- Removing `.cache/observation/` is the reset

## [04]-[BOUNDARY]

Boundary runs after the row at `Stop` and at a `SubagentStop` with a non-empty `agent_type`, both with `stop_hook_active` false:
- Lineage comes from the worktree and branch git reports at the event's `cwd`, statements and spawns run at the worktree
- One state query per boundary event reads the counts, sets the footer label, and decides the spawns, no tool event costs a query
- Spawn waits while a task under `background_tasks` edited the unjudged range or the agent runs on the worktree from another session
- Unjudged count is distinct files edited in the tree since the lineage's last `to_ts`, the threshold counts files
- Ledger row of an edit range is written when its spawn resolves with an agent id, the next range opens after that `to_ts` while the agent runs
- Prompt a spawn passes is the range or the category with the lineage key alone, the agent derives the rest from rows
- Category spawn that refuses takes its `report` delivery rows back
- Boundary event with no `background_tasks` field reads state and sets the footer, skips spawn and delivery, and logs the absence

## [05]-[EXTENSION]

New purpose takes a view, an agent, or a trigger pair, each touching its own owner:
- View is one `_VIEWS` element of `sql.ts` with its reader in the skill and its count in `views.test.ts`
- View reads what rows already hold and touches no registration, column, agent, or option
- Agent is one file under `.claude/agents/` preloading `observation` and its rubric's skill
- Agent reads rows and the working tree, writes findings through the skill's scripts, and touches nothing in the module
- Trigger pair is `<kind>Threshold` and `<kind>Agent` in `userConfig` with a `range_kind` row and a `delivery.ts` trigger over one view
- Options are read once at `register`, a changed value waits for the module's reload
- Evidence no row holds is a gap at the module, one payload key or one matcher entry

## [06]-[PROOF]

`claude plugin validate`, the typecheck, and the views test pass before a live session proves a module change:
- `views.test.ts` prepares every view and rebuilds a changed table over rows through `node:sqlite`, run under target `check`
- Sink change without its skill change is a drift no checker reports, an agent's command fails first
- Agent name matching no definition refuses the spawn, one log line names it
