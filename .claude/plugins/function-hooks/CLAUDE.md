# [FUNCTION_HOOKS]

Policies refuse tool calls. With `observation` true, hook events become sink rows, and stop boundaries spawn judging agents and deliver findings:
- Use `observation` skill for sink names, scripts, and readers.

[TOOL_CALL]: Function hooks hold one decision the harness takes on every tool call before the tool runs, `deny` or `next`
- ALWAYS use `plugin-authoring` skill for writing or changing a function hook
- ALWAYS write a policy as a pure function from the parsed call to a decision, the registered hook alone reads `$` and answers `deny` or `next`
- ALWAYS fold every policy under the one `tool.call` registration, the first refusal is the call's answer

## [01]-[POLICIES]

Git, stdin, and wait policies read Bash and Monitor commands, script and walker policies Bash alone, path policy Write, worktree policy Agent and EnterWorktree:
- Refusals state what the call does and the repository form replacing it (`hyperfine` for `time`, own exit or `run_in_background` for a wait)
- Parsing a command costs one `ast-grep` process per call and one more per `sh -c` or `eval` body to depth 8, a failed parse refuses the call
- Policies read every program of a command's wrapper chain and the command a launcher runs after `--`
- `invocation.ts` declares each program's valued options, a valued option missing there makes its value an operand
- Walker policy joins the Bash policies when `walkPolicy` is true, its default false passes every walker and reads no `HOME` or cwd
- Stdin policy refuses a program of `stdin.ts` that no operand, pipe, heredoc, herestring, or input redirect on it or an enclosing statement feeds
- Wait policy refuses `sleep`, `pwait`, `wait` with an id, `caffeinate -w` or without a command, `tail --pid`, and `lsof` repeat mode
- Wait policy refuses every command inside a `while` or `until` loop not driven by `read` and inside a `for ((;;))` loop
- Git policy checks each operand of `git reset` and `git checkout` through `$.fs.exists`, an existing path passes `reset` and refuses `checkout`
- Refusal is proven headless through `--plugin-dir`, read back from a denied call's result, and from view `denials` when `observation` is true

## [02]-[RECORDING]

With `observation` true, every classic event the matcher lists, every `turn.*` event, and every refused `tool.call` becomes one row stamped from `$.clock`:
- `observation` false registers the `tool.call` decision alone, opens no sink, writes no row, spawns no agent, and draws no footer label
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
- Rebuilt tables recompute generated columns, transitions keep the finding ids stored before the rebuild
- Observation, finding, transition, delivery, and judged range rows insert once, no statement updates or deletes one
- Payload key the harness adds reaches new rows alone, older rows answer null to `json_extract` over the key
- Scripts run under `-bail`, a failed statement without it reaches `commit`
- Failed open is one log line and no rows until reload, a session outside a git repository opens nothing
- Lost journal switch at a concurrent first open is one log line, the sink stays open in its journal mode
- WAL and the busy timeout serialize the processes and worktrees writing one file
- Removing `.cache/observation/` is the reset

## [04]-[EXTENSION]

New purpose takes a view, an agent, or a trigger pair, each touching its own owner and live while `observation` is true:
- View is one `views` element of `open` in `sql.ts` with its reader in the skill and its count in `views.test.ts`
- View reads what rows hold and touches no registration, column, agent, or option
- Agent is one file under `.claude/agents/` preloading `observation` and its rubric's skill
- Agent reads rows and the working tree, writes findings through the skill's scripts, and touches nothing in the module
- Trigger pair is `<kind>Threshold` and `<kind>Agent` in `userConfig` with a `range_kind` row and a `delivery.ts` trigger over one view
- Options are read once at `register`, a changed value waits for the module's reload, `observation` false leaves every trigger option unread
- Evidence no row holds is a gap at the module, one payload key or one matcher entry

## [05]-[PROOF]

`claude plugin validate`, target `typecheck`, and `views.test.ts` pass before a live session proves a module change:
- `views.test.ts` prepares every view and rebuilds a changed table over rows through `node:sqlite`, run under target `check`
- Sink change without its skill change is a drift no checker reports, an agent's command fails first
- Agent name matching no definition refuses the spawn, one log line names it
