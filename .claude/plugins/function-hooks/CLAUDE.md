# [FUNCTION_HOOKS]

Policies refuse tool calls. With `observation` true, hook events become database rows, and stop boundaries spawn agents and deliver findings:
- Use `observation` skill for database reads and writes

[TOOL_CALL]: Function hooks hold one decision the harness takes on every tool call before the tool runs, `deny`, `next` over the call, or `next` over its rewrite
- ALWAYS use `plugin-authoring` skill for writing or changing a function hook
- ALWAYS write a policy as a pure function from the parsed call to its refusal, `register.ts` alone reads `$` and answers `deny` or `next`
- ALWAYS write a rewrite as a pure function from the parsed call and text to a command, a note of what changed, and an instruction of what to write
- `register.ts` re-parses the rewritten command and runs every refusal policy over it before `next`
- `register.ts` logs each note to the transcript and adds it with its instruction to result `context` for the model
- ALWAYS write `$.ui.log` text as one line
- ALWAYS fold every policy under the one `tool.call` registration, the first refusing policy is the call's answer

[RUNTIME]: Harness loads `hooks/register.ts` and the files it imports in an environment with no DOM and no Node
- Modules import their own files by relative path and `claude-code` alone, `effect` and every other package fail at load
- Session values that outlive a reload sit in `$.state`, module variables restart at every reload
- `$.state` keys sit in `hooks/state.d.ts`, the contract `plugin.json` names under `types`, which the harness validator requires for every state key

## [01]-[POLICIES]

Git, stdin, and wait policies read Bash and Monitor commands, script and walker policies Bash alone, path policy Write, worktree policy Agent and EnterWorktree:
- Refusals state what the call does and the repository form replacing it (`hyperfine` for `time`, own exit or `run_in_background` for a wait)
- Failed parse refuses the call
- Redirect words past the first destination are operands of the command, and a statement's redirect belongs to its last command
- Policies read every program of a command's wrapper chain and the command a launcher runs after `--`
- `invocation.ts` declares each program's valued options, wrappers, launchers, reader and recursion options, a valued option missing there makes its value an operand
- Walker policy joins the Bash policies when `walkPolicy` is true, its default false passes every walker and reads no `HOME` or path
- Walker policy compares each start operand's real path, or its folder's when the operand does not resolve, with the real path of `~/Library/CloudStorage`
- Stdin policy refuses a reader program of `invocation.ts` that no operand, pipe, heredoc, herestring, or input redirect on it or an enclosing statement feeds
- Bash tool stdin is a character device in foreground and background runs, `rg` with no operand searches the working directory and is no reader
- Wait policy refuses a program of `policies.ts` that blocks the call on time or another process
- Wait policy refuses every command inside a `while` or `until` loop not driven by `read` and inside a `for ((;;))` loop
- Git policy checks each operand of `git reset` and `git checkout` through `$.fs.exists`, an existing path passes `reset` and refuses `checkout`
- Rewrite policy adds `-A` to an `sd` invocation lacking it
- Rewrite policy adds `--` before the find of an `sd` invocation with an operand opening with `-` outside its `invocation.ts` row's `flags` and `valued` options
- Rewrite policy splices top-level commands by the byte spans of their words, a command inside an inline body keeps its text

## [02]-[RECORDING]

With `observation` true, `Stop`, every classic event the matcher lists, every `turn.*` event, and every refused `tool.call` becomes one row stamped from `$.clock`:
- `observation` false registers the `tool.call` decision alone, opens no database, writes no row, spawns no agent, and sets no status line
- Read, Write, and Edit call bodies, batch call responses, and deny trace values drop before the write
- One awaited `sqlite3` process writes the row before `next(e)`, a deny row after the answer
- Statements are constant text, values bind as parameters from one JSON object through `json_each`
- Failed write is one `$.ui.log` line naming the row, the row is lost and nothing retries
- Shell rewrite of a file (`sd`, `sed -i`, a redirect) writes no edit row, `editThreshold` counts none
- `$.ui.status` holds the counts a boundary read, cleared at `SessionEnd`

## [03]-[SCHEMA]

Declarations of `hooks/observation/sql.ts` are the schema, applied as a delta at a load's first recorded event, a refused delta rolls back whole:
- Delta file beside the database holds the drops and rebuilds the last open computed
- Views drop and create at every open, a new body applies at the next load
- Lookup table takes its declared rows at open, updates their other columns, and retires a value no row references
- Rebuild refuses a `not null` column with no default over rows, a renamed strict key, a declaration sharing no stored column, a check old rows fail
- Column added over rows holds a default, a check over a fact old rows lack stays the writer's gate
- Declared and stored names match case-insensitively
- Retired view goes at the next open, a retired table or index stays until a statement drops it
- Rebuilt tables recompute generated columns, transitions keep the finding ids stored before the rebuild
- Observation, finding, transition, delivery, and judged range rows insert once, no statement updates or deletes one
- Payload key the harness adds reaches new rows alone, older rows answer null to `->>` over the key
- Scripts run under `-bail`, a failed statement without it reaches `commit`
- Failed open is one log line and no rows until reload, a session outside a git repository opens nothing
- Lost journal switch at a concurrent first open is one log line, the database stays open in its journal mode
- WAL and the busy timeout serialize the processes and worktrees writing one file
- Removing `.cache/observation/` is the reset

## [04]-[EXTENSION]

New purpose takes a view or an agent, each touching its own owner and live while `observation` is true:
- View is one `views` element of `open` in `sql.ts` with its reader in the skill
- `views.test.ts` prepares every view and rebuilds a changed table over rows through `node:sqlite` under target `check`
- View reads what rows hold and touches no registration, column, agent, or option
- Agent is one file under `.claude/agents/` preloading `observation` and its rubric's skill
- Agent name matching no definition refuses the spawn, one log line names it
- Agent reads rows and the working tree, writes findings through the skill's scripts, and touches nothing in the module
- Options are read once at `register`, a changed value reloads the module, `observation` false leaves every agent option unread
- Range agent spawns on the `editThreshold` and `editAgent` pair alone, `edit` is the one range kind
- Evidence no row holds is a gap at the module, one payload key or one matcher entry
- Database change without its skill change is a drift no checker reports, an agent's command fails first
