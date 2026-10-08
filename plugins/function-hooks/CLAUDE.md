# [FUNCTION_HOOKS]

Hooks guard tool calls, check edits, and draw session facts. With `observation` true, events become rows, and `Stop` boundaries spawn agents and deliver findings:
- ALWAYS write each pure function in the plugin folder of its concern, `hooks/register.ts` alone reads `$`
- Use `observation` skill for database reads and writes

[TOOL_CALL]: Function hooks answer each tool call before it runs with `deny`, `next` over the call, or `next` over its rewrite
- ALWAYS use `plugin-authoring` skill for writing or changing a function hook
- ALWAYS write a policy as a pure function from the parsed call to its refusal, `register.ts` answers `deny` or `next`
- ALWAYS write a rewrite as a pure function from parsed call, text, locks, root, and call id to command, notice, and model context
- Identical rewrite context reaches the call's loop once per turn
- `policies.ts` `commandDecision` runs every refusal policy over the parsed call, and queues and rewrites a call no policy refuses
- `commandDecision` reads files, real paths, environment, and the repository root through a `Host`, `register.ts` builds one from `$`
- ALWAYS fold every policy under the one `tool.call` registration, the first refusing policy answering
- `tool.call` `.catch` refuses a call the hook could not judge

[RENDERING]: Render hooks draw from `$.state` alone and write nothing, handlers and events write
- Band rows draw a label and facts joined by ` · `, a band with no fact or with a survey answers `next(e)`
- `hooks` row holds the latest rewrite, spawn, or restart notice until a newer notice, a prompt, or 8 s, a timer clearing only the value it set
- `services` row lists `mise.toml` launchd agents with no listener on their `--port` at a 60 s `lsof` read, its `1: restart` button runs `launchctl kickstart -k` per agent and drops each restarted one
- `ToolResult` rows keyed by `tool_use_id` draw diagnostics under Edit and Write results and a capture under a capturing MCP call's result
- Texts the model acts on hold a fact sentence, then an action sentence where an action follows

[RUNTIME]: Harness loads `hooks/register.ts` and the files it imports in an environment with no DOM and no Node
- Modules import their own files by relative path and `claude-code` alone, `effect` and every other package fail at load
- `composition.ts` holds the one result type in place of `effect`, every module composes its faults through it
- Session values that outlive a reload sit in `$.state`, module variables and timers restart at every reload
- `$.state` keys and their value types sit in `hooks/state.d.ts`, the `types` contract `claude plugin validate` holds every state key to
- Contract is a `.d.ts` of exported types alone with no import, modules import the state value types from it
- JSX in `ui/render.tsx` compiles against global `h`, trees take the surface's element table from `$.ui.resolve(e)` as an argument

## [01]-[POLICIES]

Policies read Bash and Monitor (git, stdin, wait, rewrite), Bash alone (script, walker, queue), Write (path), Agent and EnterWorktree (worktree):
- Refusals state what the call does, then the repository form replacing it where one exists
- Failed parse refuses the call
- Redirect words past the first destination are operands of the command, and a statement's redirect belongs to its last command
- Policies read every program of a command's wrapper chain and the command a launcher runs after `--`
- Inline bodies (`eval` words, a shell's `-c` operand, operands and `bodies` option values of a program with `bodies`) parse under the same policies
- `invocation.ts` declares each program's options, bodies, input, and inner command, an undeclared option's value parses as an operand
- Path policy refuses a Write of a mini config and names its owner
- Walker policy joins the Bash policies when `walkPolicy` is true, its default false passes every walker and reads no `HOME` or path
- Walker policy compares each start operand's real path, or its folder's for an unresolved operand, with the real path of `~/Library/CloudStorage`
- Stdin policy refuses an `invocation.ts` reader that no operand, pipe, heredoc, herestring, or input redirect on it or an enclosing statement feeds
- Bash tool stdin is a character device in foreground and background runs, `rg` with no operand searches the working directory and is no reader
- Wait policy refuses a program of `policies.ts` that blocks the call on time or another process
- Wait policy refuses every command inside a `while` or `until` loop not driven by `read` and inside a `for ((;;))` loop
- Git policy checks each operand of `git reset` and `git checkout` through `Host` `exists`, an existing path passes `reset` and refuses `checkout`
- Rewrite policy adds `-A` to an `sd` invocation lacking it when its find holds a line break, line mode never matches one
- Rewrite policy adds `--` before an `sd` find when an operand opens with `-` outside the `flags` and `valued` options of its `invocation.ts` row
- Rewrite policy drops an `npx`, `npm`, or `pnpm` launcher before `nx`
- Rewrite policy adds `-bl` naming `<subcommand>-<tool_use_id>-<n>.binlog` under `.artifacts/dotnet/binlog/` to a `dotnet publish`, `pack`, or `msbuild` call passing none, help and version calls excepted
- Rewrite policy splices top-level commands by the byte spans of their words, a command inside an inline body keeps its text
- Queue policy wraps a command in `{ lockf 9 && {`, its lines, and `} 9>&-; } 9>>'<root>/<lock>'` once per lock the command needs
- Queued commands from every agent, session, harness, and worktree share each lock under the main working tree and run one at a time per lock
- Brace group runs the command in Bash tool's shell with its `cd`, aliases, functions, and exit status unchanged
- Shell holds the lock on descriptor 9 and closes it for the command, a process the command leaves running holds no lock
- `policies.ts` `_queues` declares each `.cache/` lock with its commands and options, uv and uvx resolves and `ast-grep scan` or `sg scan` writes
- `nx` calls queue when a task they name or its `dependsOn` closure in Nx's `project-graph.json` runs a queued command
- `nx` words name tasks as `run <project>:<target>`, `run-many` or `affected` with `-t` and `-p`, or `<target> [<project>]`
- Nx writes its graph under `NX_WORKSPACE_DATA_DIRECTORY`, a call before the first graph queues on its own words
- Unset `NX_WORKSPACE_DATA_DIRECTORY`, an unread or malformed project graph, or an unparsed target command refuses the `nx` call
- `lockf` on a descriptor number skips every wrap and on a lock's path skips that lock's wrap, a nested wrap waits on its own holder

## [02]-[EDITS]

Each Edit, Write, and NotebookEdit path under the repository root joins its loop's `edited` family (`agentId` or `main`), cleared at the loop's `turn.complete`:
- Failed calls join nothing and draw nothing
- At `Stop` and `SubagentStop` with `stop_hook_active` false, each writer runs over the loop's edited files its `lint:*` inputs match
- Reformatted files and writer failures reach the model as context
- Writers accept the exit a checker gives for findings it cannot fix, `lint:*` targets report those
- Repository root is `git rev-parse --show-toplevel` from the session root, read per edit, capture, and boundary
- Main-loop Read, Write, and Edit of a `.md` under `~/.claude/plans`, and Write or Edit of a `/tmp` task file holding a numbered row, set the plan pointer
- `prompt.compose` adds the pointer as a session section, `session.compact` adds it to main-loop compaction instructions

## [03]-[CODEX]

Codex rows in `.codex/config.toml` run `.codex/hooks/rewrite.ts` over `Bash` and `apply_patch` at `PreToolUse`, `.codex/hooks/format.ts` at `Stop`:
- `host.ts` builds the policies' `Host` and the writers' `Formatter` over Node's `child_process` and `fs`
- `rewrite.ts` answers Bash calls through `commandDecision` with Codex's `tool_use_id`
- Path policy reads each `*** Add File:` header of an `apply_patch` patch, the call Codex makes in place of Write
- Refusal answers `deny` with its reason, rewrite answers `allow` with `updatedInput`, `additionalContext`, and its notice as `systemMessage`
- `format.ts` with `stop_hook_active` false runs writers over files the turn's `apply_patch` headers name, changes and failures continue the turn as one `decision: block` reason
- Rollouts hold `apply_patch` calls inside `exec` code strings, each header ends at an escaped newline
- `--walk-policy` on the row turns walker policy on, as `walkPolicy` does for Claude Code
- Codex names an MCP tool `mcp__<server>__<tool>` with each character outside `[A-Za-z0-9_]` as `_`, skill hook matchers spell it so
- Codex runs a row while `~/.codex/config.toml` `[hooks.state."<file>:pre_tool_use:<group>:<handler>"]` holds its `hooks/list` `currentHash`

## [04]-[RECORDING]

With `observation` true, every `classic.*` and `turn.*` event `register.ts` names and every refused `tool.call` becomes a row stamped from `$.clock`:
- `observation` false registers policies, edits, and rendering alone, opens no database, writes no row, and spawns no agent
- Read, Write, and Edit call bodies, batch call responses, and deny trace values drop before the write
- One awaited `sqlite3` process writes the row before `next(e)`, a deny row after the answer
- Statements are constant text, values bind as parameters from one JSON object through `json_each`
- Failed write loses the row with no retry
- Shell rewrite of a file (`sd`, `sed -i`, a redirect) writes no edit row, `editThreshold` counts none
- `row.ts` `USAGE` names the events with rows holding `$.session.usage()`

## [05]-[SCHEMA]

Declarations of `observation/sql.ts` are the schema, applied as a delta at a load's first recorded event, a refused delta rolls back whole:
- Delta file beside the database holds the statements the last open computed
- Views drop and create at every open, a new body applies at the next load
- Lookup table takes the rows its declaration lists at open, updates their other columns, and retires a value no row references
- Rebuild refuses a `not null` column with no default over rows, a renamed strict key, a declaration sharing no stored column, a check old rows fail
- Column added over rows holds a default, a check over a fact old rows lack stays the writer's gate
- Declared and stored names match case-insensitively
- Retired view goes at the next open, a retired table or index stays until a statement drops it
- Rebuilt tables recompute generated columns, transitions keep the finding ids stored before the rebuild
- Observation, finding, transition, delivery, and judged range rows insert once, no statement updates or deletes one
- Payload key the harness adds reaches new rows alone, older rows answer null to `->>` over the key
- Scripts run under `-bail`, a failed statement without it reaches `commit`
- Failed open is one log line and no rows until reload, a session outside a git repository opens nothing
- Lost journal switch at a concurrent first open keeps the prior journal mode
- WAL and the busy timeout serialize the processes and worktrees writing one file
- Removing `.cache/observation/` is the reset

## [06]-[EXTENSION]

New purpose takes a view or an agent, each touching its own owner and live while `observation` is true:
- View is one `_VIEWS` element in `sql.ts` with its reader in the skill
- View reads what rows hold and touches no registration, column, agent, or option
- Agent is one file under `.claude/agents/` preloading `observation` and its rubric's skill
- Agent name matching no definition refuses the spawn, one log line names it
- Agent reads rows and the working tree, writes findings through the skill's scripts, and touches nothing in the module
- Options are read once at `register`, a changed value reloads the module, `observation` false leaves every agent option unread
- Range agent spawns on the `editThreshold` and `editAgent` pair alone, from a session with edits in the range
- `edit` is the one range kind
- Spawn writes the range's `judged_range` row or the category's `report` rows, and the boundary awaits the start alone
- Evidence no row holds is a gap at the module, one payload key or one matcher entry
- Database change without its skill change is a drift no checker reports, an agent's command fails first
